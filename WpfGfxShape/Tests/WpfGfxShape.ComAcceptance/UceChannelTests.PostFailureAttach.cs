using System.Diagnostics;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    /// <summary>Checks late shared-channel attachment and source-close survival against the official module.</summary>
    [TestMethod]
    public void WhenOriginalFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure() => RunPostFailureAttach(true);

    /// <summary>Checks the identical late shared-channel contract against the current native module.</summary>
    [TestMethod]
    public void WhenFailedPartitionAcceptsLateSharedChannelThenSourceClosePreservesFailure() => RunPostFailureAttach(false);

    private sealed record AttachCall(int Order, string Operation, long Channel, long Source, uint Resource, int HResult);
    private sealed record AttachOutcome(int A, int Create, long Source, long L, int First, int After, int FailureOrder, int CreateOrder, int SourceDestroyOrder, int AfterOrder, bool DestroyA, bool DestroyL);

    private static bool AttachAccepts(AttachOutcome value) =>
        value.A == unchecked((int)0x88980406) && value.Create == 0 && value.Source != 0 && value.L != 0 && value.L != value.Source &&
        value.First == unchecked((int)0x88980406) && value.After == unchecked((int)0x88980406) &&
        value.FailureOrder < value.CreateOrder && value.CreateOrder < value.SourceDestroyOrder && value.SourceDestroyOrder < value.AfterOrder &&
        value.DestroyA && value.DestroyL;

    private static void RunPostFailureAttach(bool original)
    {
        string requested = Path.GetFullPath(original ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll);
        string uid = Guid.NewGuid().ToString("N");
        string root = Path.Combine(NativeRun.PathsRoot, "PostFailureAttach", uid);
        Directory.CreateDirectory(root);
        string message = new ResourceManager("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(UceChannelTests).Assembly)
            .GetString("PostFailureAttachContract") ?? nameof(RunPostFailureAttach);
        List<AttachCall> calls = [];
        List<Exception> errors = [];
        List<long> created = [], destroyed = [], destroyAttempted = [];
        ExceptionDispatchInfo? primary = null;
        object? identity = null;
        AttachOutcome? outcome = null;
        bool initialized = false, completed = false, unknownRegistration = false, disconnected = false, deinitialized = false;
        nint module = 0;
        nint* handles = stackalloc nint[3];
        new Span<nint>(handles, 3).Clear();
        int sequence = 0;
        int Record(string operation, int hr, nint channel = 0, nint source = 0, uint resource = 0)
        {
            calls.Add(new(++sequence, operation, (long)channel, (long)source, resource, hr));
            return hr;
        }
        void Check(string operation, int hr, nint channel = 0, nint source = 0, uint resource = 0) =>
            Assert.AreEqual(0, Record(operation, hr, channel, source, resource), message);
        try
        {
            module = NativeLibrary.Load(requested);
            using var process = Process.GetCurrentProcess();
            var loaded = process.Modules.Cast<ProcessModule>().Single(item => item.BaseAddress == module);
            string actual = Path.GetFullPath(loaded.FileName);
            bool matches = string.Equals(requested, actual, StringComparison.OrdinalIgnoreCase);
            identity = new
            {
                requested, actual, requestedEqualsActual = matches, moduleHandle = (long)module, actualBaseAddress = (long)loaded.BaseAddress,
                sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))), version = loaded.FileVersionInfo.FileVersion,
                candidateInitializationFile = AcceptancePaths.AotDll, candidateInitializationSha256 = AcceptancePaths.ValidateIdentity(),
                candidateInitializationIsNotActualRoleEvidence = true
            };
            Assert.IsTrue(matches, message);
            nint Export(string name) => NativeLibrary.GetExport(module, name);
            var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
            var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
            var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
            var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
            var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
            var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
            var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
            var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
            var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
            int init = Record("Initialize.0", initialize(0));
            initialized = init == 0;
            Assert.AreEqual(0, init, message);
            Check("Connect.Async0", connect(0, handles), handles[0]);
            Assert.AreNotEqual(0L, (long)handles[0], message);
            int createA = channelCreate(handles[0], 0, handles + 1);
            if (handles[1] != 0) created.Add((long)handles[1]);
            if (createA != 0 && handles[1] == 0) unknownRegistration = true;
            Check("Create.A", createA, handles[1]);
            Assert.AreNotEqual(0L, (long)handles[1], message);
            uint visual = 0;
            Check("Create.Visual39", create(handles[1], 39, &visual), handles[1], resource: visual);
            Assert.AreNotEqual(0u, visual, message);
            IsolationAlphaPacket alpha = new() { Type = 0x20, Handle = visual, Alpha = 0.75 };
            Check("Setup.Send", send(&alpha, (uint)sizeof(IsolationAlphaPacket), 0, handles[1]), handles[1], resource: visual);
            Check("Setup.CloseBatch", close(handles[1]), handles[1]);
            Check("Setup.Commit", commit(handles[1]), handles[1]);
            Check("Setup.Flush", flush(handles[1]), handles[1]);
            uint bitmap = 0;
            Check("Create.Bitmap95", create(handles[1], 95, &bitmap), handles[1], resource: bitmap);
            Assert.AreNotEqual(0u, bitmap, message);
            Check("Bitmap.CloseBatch", close(handles[1]), handles[1]);
            Check("Bitmap.Commit", commit(handles[1]), handles[1]);
            Check("Bitmap.Flush", flush(handles[1]), handles[1]);
            IsolationBitmapPacket bad = new() { Type = 0x0c, Handle = bitmap, Source = 0 };
            Check("Send.CompleteNullSource", send(&bad, (uint)sizeof(IsolationBitmapPacket), 0, handles[1]), handles[1], resource: bitmap);
            Check("Failure.CloseBatch", close(handles[1]), handles[1]);
            Check("Failure.Commit", commit(handles[1]), handles[1]);
            int a = Record("Failure.Flush.A", flush(handles[1]), handles[1]);
            int failureOrder = sequence;
            Assert.AreEqual(unchecked((int)0x88980406), a, message);
            nint source = handles[1];
            int late = channelCreate(handles[0], source, handles + 2);
            if (handles[2] != 0 && !created.Contains((long)handles[2])) created.Add((long)handles[2]);
            if (late != 0 && handles[2] == 0) unknownRegistration = true;
            Record("Create.L", late, handles[2], source);
            int createOrder = sequence;
            Assert.AreEqual(0, late, message);
            Assert.AreNotEqual(0L, (long)handles[2], message);
            Assert.AreNotEqual((long)source, (long)handles[2], message);
            int first = Record("Failure.Flush.L", flush(handles[2]), handles[2]);
            Assert.AreEqual(unchecked((int)0x88980406), first, message);
            destroyAttempted.Add((long)source);
            int destroyA = Record("Destroy.A", destroy(source), source);
            int sourceDestroyOrder = sequence;
            if (destroyA == 0) { destroyed.Add((long)source); handles[1] = 0; }
            Assert.AreEqual(0, destroyA, message);
            int after = Record("Failure.Flush.L.AfterSourceClose", flush(handles[2]), handles[2]);
            int afterOrder = sequence;
            Assert.AreEqual(unchecked((int)0x88980406), after, message);
            nint lateHandle = handles[2];
            destroyAttempted.Add((long)lateHandle);
            int destroyL = Record("Destroy.L", destroy(lateHandle), lateHandle);
            if (destroyL == 0) { destroyed.Add((long)lateHandle); handles[2] = 0; }
            Assert.AreEqual(0, destroyL, message);
            outcome = new(a, late, (long)source, (long)lateHandle, first, after, failureOrder, createOrder, sourceDestroyOrder, afterOrder, true, true);
            completed = true;
        }
        catch (Exception error)
        {
            // Preserve the primary assertion while attempting every independently known cleanup operation.
            primary = ExceptionDispatchInfo.Capture(error);
        }
        for (int i = 1; i <= 2; i++)
        {
            nint owned = handles[i];
            if (owned == 0 || destroyAttempted.Contains((long)owned)) continue;
            destroyAttempted.Add((long)owned);
            try
            {
                var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
                int hr = Record("Cleanup.Destroy." + i, destroy(owned), owned);
                if (hr == 0) { destroyed.Add((long)owned); handles[i] = 0; }
                Assert.AreEqual(0, hr, message);
            }
            catch (Exception error) { errors.Add(error); }
        }
        if (handles[0] != 0)
        {
            try
            {
                var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
                int hr = Record("Cleanup.Disconnect", disconnect(handles[0]), handles[0]);
                disconnected = hr == 0;
                Assert.AreEqual(0, hr, message);
            }
            catch (Exception error) { errors.Add(error); }
        }
        if (initialized)
        {
            try
            {
                var deinitialize = (delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "MilCompositionEngine_DeinitializePartitionManager");
                int hr = Record("Cleanup.Deinitialize", deinitialize());
                deinitialized = hr == 0;
                Assert.AreEqual(0, hr, message);
            }
            catch (Exception error) { errors.Add(error); }
        }
        bool cleaned = initialized && !unknownRegistration && created.Count == destroyed.Count && created.All(destroyed.Contains) && disconnected && deinitialized && errors.Count == 0;
        bool[] negativeRejected = outcome is null ? [] :
        [
            !AttachAccepts(outcome with { Create = unchecked((int)0x88980406), L = 0 }),
            !AttachAccepts(outcome with { First = 0 }),
            !AttachAccepts(outcome with { After = 0 }),
            !AttachAccepts(outcome with { DestroyL = false }),
            !AttachAccepts(outcome with { CreateOrder = outcome.FailureOrder - 1 })
        ];
        var assembly = typeof(UceChannelTests).Assembly;
        try
        {
            File.WriteAllText(Path.Combine(root, "business.json"), JsonSerializer.Serialize(new
            {
                uid, original, identity, completed, cleaned, unknownRegistration,
                cleanupClosure = unknownRegistration ? "unknown: failed create may have registered an unreturned channel; process exit is not normal cleanup" : cleaned ? "known objects closed" : "incomplete",
                created, destroyed, destroyAttempted, stillKnown = new[] { (long)handles[1], (long)handles[2] }.Where(value => value != 0).ToArray(),
                disconnected, deinitialized, connection = (long)handles[0], calls, outcome,
                parameters = new { asyncConnection = 0, manager = 0, visualType = 39, bitmapType = 95, alphaCommand = 0x20,
                    alphaPacketBytes = sizeof(IsolationAlphaPacket), setupAlpha = 0.75, bitmapCommand = 0x0c,
                    bitmapPacketBytes = sizeof(IsolationBitmapPacket), bitmapSource = 0, relation = "A(source0), after A failure L(sourceA)", expectedFailure = unchecked((int)0x88980406) },
                negativeRejected, negativeControlScope = "Result copies validate assertions only; actual calls are unchanged; not product fault injection",
                primary = primary?.SourceException.ToString(), cleanupErrors = errors.Select(error => error.ToString()).ToArray(),
                testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))), testMvid = assembly.ManifestModule.ModuleVersionId,
                runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, pid = Environment.ProcessId,
                loadedWpf = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(item => Path.GetFileName(item.FileName).Equals("PresentationNative_cor3.dll", StringComparison.OrdinalIgnoreCase))
                    .Select(item => new { path = item.FileName, version = item.FileVersionInfo.FileVersion }).ToArray()
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception error) { errors.Add(error); }
        if (primary is not null)
        {
            if (errors.Count > 0) primary.SourceException.Data["PostFailureAttachCleanup"] = errors.ToArray();
            primary.Throw();
        }
        if (errors.Count > 0) throw new AggregateException(message, errors);
        Assert.IsTrue(completed && cleaned && outcome is not null && AttachAccepts(outcome), message);
        Assert.IsTrue(negativeRejected.Length == 5 && negativeRejected.All(value => value), message);
    }
}
