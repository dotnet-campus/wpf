using System.Diagnostics;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    /// <summary>Checks shared asynchronous failure and independent partition survival against the official module.</summary>
    [TestMethod]
    public void WhenOriginalAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives() => RunPartitionFailureIsolation(true);

    /// <summary>Checks the identical asynchronous isolation and close contract against the current native module.</summary>
    [TestMethod]
    public void WhenAsyncPartitionFailsThenSiblingFailsAndIndependentPartitionSurvives() => RunPartitionFailureIsolation(false);

    private sealed record IsolationCall(int Order, string Operation, long Channel, long Source, uint Resource, int HResult);
    private sealed record IsolationOutcome(int A, int S, int BeforeClose, int AfterClose);

    private static bool IsolationAccepts(IsolationOutcome result) =>
        result == new IsolationOutcome(unchecked((int)0x88980406), unchecked((int)0x88980406), 0, 0);

    private static void RunPartitionFailureIsolation(bool original)
    {
        string requested = Path.GetFullPath(original ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll);
        string uid = Guid.NewGuid().ToString("N");
        string root = Path.Combine(NativeRun.PathsRoot, "PartitionFailureIsolation", uid);
        Directory.CreateDirectory(root);
        string message = new ResourceManager("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(UceChannelTests).Assembly)
            .GetString("PartitionIsolationContract") ?? nameof(RunPartitionFailureIsolation);
        List<IsolationCall> calls = [];
        List<Exception> errors = [];
        ExceptionDispatchInfo? primary = null;
        object? identity = null;
        IsolationOutcome? outcome = null;
        bool initialized = false, completed = false;
        nint module = 0;
        nint* handles = stackalloc nint[4];
        new Span<nint>(handles, 4).Clear();
        uint[] visuals = new uint[3];
        List<long> created = [], destroyed = [];
        bool disconnected = false, deinitialized = false;
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
                requested, actual, requestedEqualsActual = matches, moduleHandle = (long)module,
                actualBaseAddress = (long)loaded.BaseAddress,
                sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))),
                version = loaded.FileVersionInfo.FileVersion,
                candidateInitializationFile = AcceptancePaths.AotDll,
                candidateInitializationSha256 = AcceptancePaths.ValidateIdentity(),
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
            void Update(nint channel, uint visual, double alpha, string stage)
            {
                IsolationAlphaPacket packet = new() { Type = 0x20, Handle = visual, Alpha = alpha };
                Check(stage + ".Send", send(&packet, (uint)sizeof(IsolationAlphaPacket), 0, channel), channel, resource: visual);
                Check(stage + ".CloseBatch", close(channel), channel);
                Check(stage + ".Commit", commit(channel), channel);
            }
            int init = Record("Initialize.0", initialize(0));
            initialized = init == 0;
            Assert.AreEqual(0, init, message);
            Check("Connect.Async0", connect(0, handles));
            Assert.AreNotEqual(0L, (long)handles[0], message);
            for (int i = 1; i <= 3; i++)
            {
                nint source = i == 2 ? handles[1] : 0;
                int hr = channelCreate(handles[0], source, handles + i);
                if (handles[i] != 0) created.Add((long)handles[i]);
                Check("Create." + i, hr, handles[i], source);
                Assert.AreNotEqual(0L, (long)handles[i], message);
                uint visual = 0;
                int resourceHr = create(handles[i], 39, &visual);
                Check("Create.Visual39", resourceHr, handles[i], resource: visual);
                Assert.AreNotEqual(0u, visual, message);
                visuals[i - 1] = visual;
                Update(handles[i], visual, 0.75, "Setup." + i);
                Check("Setup.Flush." + i, flush(handles[i]), handles[i]);
            }
            uint bitmap = 0;
            int bitmapHr = create(handles[1], 95, &bitmap);
            Check("Create.Bitmap95", bitmapHr, handles[1], resource: bitmap);
            Assert.AreNotEqual(0u, bitmap, message);
            Check("Bitmap.CloseBatch", close(handles[1]), handles[1]);
            Check("Bitmap.Commit", commit(handles[1]), handles[1]);
            Check("Bitmap.Flush", flush(handles[1]), handles[1]);
            IsolationBitmapPacket bad = new() { Type = 0x0c, Handle = bitmap, Source = 0 };
            Check("Send.CompleteNullSource", send(&bad, (uint)sizeof(IsolationBitmapPacket), 0, handles[1]), handles[1], resource: bitmap);
            Check("Failure.CloseBatch", close(handles[1]), handles[1]);
            Check("Failure.Commit", commit(handles[1]), handles[1]);
            int a = Record("Failure.Flush.A", flush(handles[1]), handles[1]);
            int s = Record("Failure.Flush.S", flush(handles[2]), handles[2]);
            Update(handles[3], visuals[2], 0.5, "Independent.BeforeClose");
            int before = Record("Independent.Flush.BeforeClose", flush(handles[3]), handles[3]);
            for (int i = 1; i <= 2; i++)
            {
                nint owned = handles[i];
                handles[i] = 0;
                int hr = Record("Destroy.Failed." + i, destroy(owned), owned);
                if (hr == 0) destroyed.Add((long)owned);
                Assert.AreEqual(0, hr, message);
            }
            Update(handles[3], visuals[2], 0.25, "Independent.AfterClose");
            int after = Record("Independent.Flush.AfterClose", flush(handles[3]), handles[3]);
            outcome = new(a, s, before, after);
            completed = true;
        }
        catch (Exception error)
        {
            // Preserve the first failure while every independently owned object is cleaned below.
            primary = ExceptionDispatchInfo.Capture(error);
        }
        for (int i = 1; i <= 3; i++)
        {
            if (handles[i] == 0) continue;
            nint owned = handles[i];
            handles[i] = 0;
            try
            {
                var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
                int hr = Record("Cleanup.Destroy." + i, destroy(owned), owned);
                if (hr == 0) destroyed.Add((long)owned);
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
        bool cleaned = initialized && created.Count == 3 && created.SequenceEqual(destroyed) && disconnected && deinitialized && errors.Count == 0;
        bool siblingNegativeRejected = outcome is not null && !IsolationAccepts(outcome with { S = 0 });
        bool independentNegativeRejected = outcome is not null && !IsolationAccepts(outcome with { BeforeClose = unchecked((int)0x88980406) });
        var assembly = typeof(UceChannelTests).Assembly;
        File.WriteAllText(Path.Combine(root, "business.json"), JsonSerializer.Serialize(new
        {
            uid, original, identity, completed, cleaned, created, destroyed, disconnected, deinitialized,
            connection = (long)handles[0], calls, outcome,
            parameters = new { asyncConnection = 0, manager = 0, visualType = 39, bitmapType = 95,
                alphaCommand = 0x20, alphaPacketBytes = sizeof(IsolationAlphaPacket), setupAlpha = 0.75,
                beforeCloseAlpha = 0.5, afterCloseAlpha = 0.25, bitmapCommand = 0x0c,
                bitmapPacketBytes = sizeof(IsolationBitmapPacket), bitmapSource = 0,
                relation = "A(source0), S(sourceA), B(source0)", expectedFailure = unchecked((int)0x88980406) },
            siblingNegativeRejected, independentNegativeRejected,
            negativeControlScope = "Result copies validate assertions only; actual results are unchanged; not product fault injection",
            primary = primary?.SourceException.ToString(), cleanupErrors = errors.Select(error => error.ToString()).ToArray(),
            testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
            testMvid = assembly.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, pid = Environment.ProcessId
        }, new JsonSerializerOptions { WriteIndented = true }));
        if (primary is not null)
        {
            if (errors.Count > 0) primary.SourceException.Data["PartitionIsolationCleanup"] = errors.ToArray();
            primary.Throw();
        }
        if (errors.Count > 0) throw new AggregateException(message, errors);
        Assert.IsTrue(completed && cleaned && outcome is not null && IsolationAccepts(outcome), message);
        Assert.IsTrue(siblingNegativeRejected && independentNegativeRejected, message);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct IsolationAlphaPacket { internal uint Type, Handle; internal double Alpha; }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct IsolationBitmapPacket { internal uint Type, Handle; internal nint Source; }
}
