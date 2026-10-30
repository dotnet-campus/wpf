using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    /// <summary>Checks real shared asynchronous WIC ownership against the official module.</summary>
    [TestMethod]
    public void WhenOriginalAsyncSharedBitmapOutlivesSourceThenFinalCloseReleasesOwnership() => RunAsyncSharedOwnership(true);

    /// <summary>Checks the same finite shared ownership contract against the candidate module.</summary>
    [TestMethod]
    public void WhenAsyncSharedBitmapOutlivesSourceThenFinalCloseReleasesOwnership() => RunAsyncSharedOwnership(false);

    private sealed record SharedOwnershipOutcome(int BeforeSourceClose, int AfterSourceClose, int FinalDisposals, int CallbackExited);

    private static bool SharedOwnershipAccepts(SharedOwnershipOutcome outcome) => outcome == new SharedOwnershipOutcome(0, 0, 1, 1);

    private sealed class SharedRoundState
    {
        internal int Disposals, CallbackExited, CallbackEntries;
        internal uint CallbackThread;
        internal readonly ConcurrentQueue<string> CallbackErrors = new();
        internal ConcurrentQueue<object>? Events;
        internal string? CallbackPath;
        internal string? EventPath;
        internal SharedKnownOwners? KnownOwners;

        internal void OnDispose()
        {
            try
            {
                CallbackThread = DrainGetCurrentThreadId();
                Interlocked.Increment(ref CallbackEntries);
                Record("Callback.Enter");
                Interlocked.Increment(ref Disposals);
                Record("Callback.Exit");
            }
            catch (Exception error) { CallbackErrors.Enqueue(error.ToString()); }
            finally { Interlocked.Increment(ref CallbackExited); }
        }

        private void Record(string operation)
        {
            var value = new { operation, utc = DateTimeOffset.UtcNow, qpc = Stopwatch.GetTimestamp(),
                pid = Environment.ProcessId, thread = CallbackThread, entries = Volatile.Read(ref CallbackEntries) };
            Events?.Enqueue(value);
            if (CallbackPath is not null) File.AppendAllText(CallbackPath, JsonSerializer.Serialize(value) + Environment.NewLine);
        }
    }

    private sealed record SharedKnownOwners(int CallerOwners, int Channels, int Duplicates,
        bool ConnectionOwned, bool ManagerInitialized, bool WicDisposed);

    private static void RunAsyncSharedOwnership(bool original, nint retainedModule = 0, string? roundRoot = null,
        SharedRoundState? roundState = null, Action? afterTargetUpdate = null)
    {
        string uid = Guid.NewGuid().ToString("N");
        string root = roundRoot ?? Path.Combine(NativeRun.PathsRoot, "AsyncSharedOwnership", uid);
        Directory.CreateDirectory(root);
        string requested = Path.GetFullPath(original ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll);
        string message = nameof(RunAsyncSharedOwnership);
        nint* handles = stackalloc nint[6];
        new Span<nint>(handles, 6).Clear();
        uint* resources = stackalloc uint[4];
        new Span<uint>(resources, 4).Clear();
        var events = new ConcurrentQueue<object>();
        var cleanupErrors = new List<Exception>();
        var state = roundState ?? new SharedRoundState();
        state.Events = events;
        var callbackErrors = state.CallbackErrors;
        var callerReleases = new List<object>();
        bool wicDisposed = false;
        ExceptionDispatchInfo? primary = null;
        object? identity = null;
        int sequence = 0;
        int beforeSourceClose = -1, afterSourceClose = -1;
        bool initialized = false, disconnected = false, deinitialized = false, completed = false;
        var created = new List<long>();
        var destroyed = new List<long>();
        var released = new List<uint>();
        nint module = 0;
        SystemWicBitmap? wic = null;
        void Event(string operation, object? detail = null)
        {
            var value = new
            {
                order = Interlocked.Increment(ref sequence), operation, detail, utc = DateTimeOffset.UtcNow,
                qpc = Stopwatch.GetTimestamp(), pid = Environment.ProcessId,
                thread = DrainGetCurrentThreadId(), managedThread = Environment.CurrentManagedThreadId,
                disposals = Volatile.Read(ref state.Disposals)
            };
            events.Enqueue(value);
            if (state.EventPath is not null) File.AppendAllText(state.EventPath, JsonSerializer.Serialize(value) + Environment.NewLine);
        }
        void Check(string operation, int hr, object? detail = null)
        {
            Event(operation, new { hr, detail });
            Assert.AreEqual(0, hr, message);
        }
        void ReleaseCaller(int index, string name)
        {
            nint owned = handles[index];
            if (owned == 0) return;
            handles[index] = 0;
            Event(name + ".before", new { pointer = (long)owned });
            uint references = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)owned)[2])(owned);
            callerReleases.Add(new { index, name, pointer = (long)owned, references });
            Event(name + ".after", new { pointer = (long)owned, references });
        }
        try
        {
            module = retainedModule != 0 ? retainedModule : NativeLibrary.Load(requested);
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
            var deinitialize = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
            var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
            var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
            var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
            var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
            var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
            var duplicate = (delegate* unmanaged[Stdcall]<nint, uint, nint, uint*, int>)Export("MilResource_DuplicateHandle");
            var release = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)Export("MilResource_ReleaseOnChannel");
            var wrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)Export("MilResource_CreateCWICWrapperBitmap");
            var transfer = (delegate* unmanaged[Stdcall]<uint, nint, nint, int>)Export("MilResource_SendCommandBitmapSource");
            var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
            var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
            var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
            var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
            Check("Initialize", initialize(0)); initialized = true;
            Check("Connect.Async", connect(0, handles));
            Check("Create.A", channelCreate(handles[0], 0, handles + 1), new { channel = (long)handles[1] });
            Assert.AreNotEqual((nint)0, handles[1], message); created.Add((long)handles[1]);
            Check("Create.B.SourceA", channelCreate(handles[0], handles[1], handles + 2), new { channel = (long)handles[2], source = (long)handles[1] });
            Assert.AreNotEqual((nint)0, handles[2], message); created.Add((long)handles[2]);
            Assert.AreNotEqual(handles[1], handles[2], message);
            Check("Create.A.Bitmap95", create(handles[1], 95, resources), new { channel = (long)handles[1], resource = resources[0] });
            Check("Create.A.Visual39", create(handles[1], 39, resources + 1), new { channel = (long)handles[1], resource = resources[1] });
            Assert.AreNotEqual(0u, resources[0], message); Assert.AreNotEqual(0u, resources[1], message);
            wic = new SystemWicBitmap();
            byte[] bmp = new byte[62];
            bmp[0] = (byte)'B'; bmp[1] = (byte)'M';
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(2), bmp.Length);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(10), 54);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(14), 40);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(18), 2);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(22), 1);
            bmp[26] = 1; bmp[28] = 24; bmp[56] = 255;
            handles[3] = ManagedStreamTests.CreateCallbackStream(module, bmp, () => { }, state.OnDispose);
            Event("Caller.Stream.Created", new { pointer = (long)handles[3] });
            handles[4] = wic.DecodeFrame(handles[3]);
            Event("Caller.WicFrame.Created", new { pointer = (long)handles[4] });
            Check("Caller.Wrapper.Created", wrap(handles[4], handles + 5), new { pointer = (long)handles[5] });
            Check("Transfer.A.Bitmap", transfer(resources[0], handles[5], handles[1]), new { resource = resources[0], wrapper = (long)handles[5] });
            Check("Duplicate.Bitmap.AtoB", duplicate(handles[1], resources[0], handles[2], resources + 2), new { source = resources[0], target = resources[2], a = (long)handles[1], b = (long)handles[2] });
            Check("Duplicate.Visual.AtoB", duplicate(handles[1], resources[1], handles[2], resources + 3), new { source = resources[1], target = resources[3], a = (long)handles[1], b = (long)handles[2] });
            Assert.AreNotEqual(0u, resources[2], message); Assert.AreNotEqual(0u, resources[3], message);
            Check("A.CloseBatch", close(handles[1]));
            Check("A.Commit", commit(handles[1]));
            Check("A.Flush.BeforeTargetUse", flush(handles[1]));
            ReleaseCaller(5, "Caller.Wrapper.Release");
            ReleaseCaller(4, "Caller.Frame.Release");
            ReleaseCaller(3, "Caller.Stream.Release");
            beforeSourceClose = Volatile.Read(ref state.Disposals);
            Event("Observe.BeforeSourceClose", beforeSourceClose);
            Assert.AreEqual(0, beforeSourceClose, message);
            nint source = handles[1];
            Check("A.Destroy", destroy(source), new { channel = (long)source });
            handles[1] = 0; destroyed.Add((long)source);
            afterSourceClose = Volatile.Read(ref state.Disposals);
            Event("Observe.AfterSourceClose", afterSourceClose);
            Assert.AreEqual(0, afterSourceClose, message);
            IsolationAlphaPacket alpha = new() { Type = 0x20, Handle = resources[3], Alpha = 0.25 };
            Check("B.Send.NewAlpha", send(&alpha, (uint)sizeof(IsolationAlphaPacket), 0, handles[2]), new { resource = resources[3], alpha = alpha.Alpha });
            Check("B.Update.CloseBatch", close(handles[2]));
            Check("B.Update.Commit", commit(handles[2]));
            Check("B.Update.Flush", flush(handles[2]));
            Assert.AreEqual(0, Volatile.Read(ref state.Disposals), message);
            state.KnownOwners = new(0, 1, 2, true, true, false);
            afterTargetUpdate?.Invoke();
            for (int index = 3; index >= 2; index--)
            {
                int deleted = 0;
                uint resource = resources[index];
                Check(index == 3 ? "B.Release.Visual" : "B.Release.Bitmap", release(handles[2], resource, &deleted), new { resource, deleted, channel = (long)handles[2] });
                Assert.AreEqual(1, deleted, message);
                released.Add(resource); resources[index] = 0;
            }
            Check("B.Release.CloseBatch", close(handles[2]));
            Check("B.Release.Commit", commit(handles[2]));
            Check("B.Release.Flush", flush(handles[2]));
            nint target = handles[2];
            Check("B.Destroy", destroy(target), new { channel = (long)target });
            handles[2] = 0; destroyed.Add((long)target);
            Check("Disconnect", disconnect(handles[0])); handles[0] = 0; disconnected = true;
            Check("Deinitialize", deinitialize()); initialized = false; deinitialized = true;
            Event("Wait.Callback.Before", new { deadlineSeconds = 5 });
            bool finished = SpinWait.SpinUntil(() => Volatile.Read(ref state.CallbackExited) != 0, TimeSpan.FromSeconds(5));
            Event("Wait.Callback.After", new { finished });
            Assert.IsTrue(finished, message);
            Assert.AreEqual(1, Volatile.Read(ref state.CallbackEntries), message);
            Assert.IsTrue(callbackErrors.IsEmpty, message);
            Assert.IsTrue(SharedOwnershipAccepts(new(beforeSourceClose, afterSourceClose, Volatile.Read(ref state.Disposals), Volatile.Read(ref state.CallbackExited))), message);
            completed = true;
        }
        catch (Exception error) { primary = ExceptionDispatchInfo.Capture(error); }

        // Each remaining owner is attempted independently; cleanup cannot replace the business failure.
        for (int index = 1; index <= 2; index++)
        {
            if (module == 0 || handles[index] == 0) continue;
            try
            {
                var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
                nint owned = handles[index]; handles[index] = 0;
                Check("Cleanup.Destroy", destroy(owned), new { channel = (long)owned });
                destroyed.Add((long)owned);
            }
            catch (Exception error) { cleanupErrors.Add(error); }
        }
        if (module != 0 && handles[0] != 0)
        {
            try
            {
                var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
                nint owned = handles[0]; handles[0] = 0;
                Check("Cleanup.Disconnect", disconnect(owned)); disconnected = true;
            }
            catch (Exception error) { cleanupErrors.Add(error); }
        }
        for (int index = 5; index >= 3; index--)
        {
            try { ReleaseCaller(index, "Cleanup.Caller.Release." + index); }
            catch (Exception error) { cleanupErrors.Add(error); }
        }
        if (module != 0 && initialized)
        {
            try
            {
                var deinitialize = (delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "MilCompositionEngine_DeinitializePartitionManager");
                Check("Cleanup.Deinitialize", deinitialize()); initialized = false; deinitialized = true;
            }
            catch (Exception error) { cleanupErrors.Add(error); }
        }
        try { wic?.Dispose(); wicDisposed = wic is not null; Event("Caller.WicScope.Dispose"); }
        catch (Exception error) { cleanupErrors.Add(error); }
        var outcome = new SharedOwnershipOutcome(beforeSourceClose, afterSourceClose, Volatile.Read(ref state.Disposals), Volatile.Read(ref state.CallbackExited));
        state.KnownOwners = new(
            (handles[3] != 0 ? 1 : 0) + (handles[4] != 0 ? 1 : 0) + (handles[5] != 0 ? 1 : 0),
            created.Count - destroyed.Count, 2 - released.Count, handles[0] != 0, initialized, wicDisposed);
        bool cleaned = created.Count == 2 && created.SequenceEqual(destroyed) && released.Count == 2 &&
            disconnected && deinitialized && cleanupErrors.Count == 0 && callbackErrors.IsEmpty;
        bool trendCleaned = state.KnownOwners == new SharedKnownOwners(0, 0, 0, false, false, true) && callerReleases.Count == 3;
        bool earlyDisposeNegativeRejected = !SharedOwnershipAccepts(outcome with { AfterSourceClose = 1 });
        bool missingDisposeNegativeRejected = !SharedOwnershipAccepts(outcome with { FinalDisposals = 0 });
        var assembly = typeof(UceChannelTests).Assembly;
        try
        {
            File.WriteAllText(Path.Combine(root, "business.json"), JsonSerializer.Serialize(new
            {
                uid, original, identity, completed, cleaned, created, destroyed, released, disconnected, deinitialized,
                outcome, callbackEntries = state.CallbackEntries, callbackThread = state.CallbackThread,
                workerExit = "NotProven: callback exit is not thread exit", callerReleases, knownOwners = state.KnownOwners, trendCleaned,
                callerPointers = new long[] { (long)handles[3], (long)handles[4], (long)handles[5] }, wicDisposed,
                callbackErrors = callbackErrors.ToArray(), events = events.ToArray(),
                earlyDisposeNegativeRejected, missingDisposeNegativeRejected,
                negativeControlScope = "Result-copy validator self-check only; no product fault injection",
                parameters = new { asyncConnection = 0, manager = 0, bitmapType = 95, visualType = 39,
                    alphaCommand = 0x20, alpha = 0.25, alphaBytes = sizeof(IsolationAlphaPacket),
                    relation = "A(source0), B(sourceA); bitmap and visual independently duplicated; visual does not reference bitmap",
                    callbackDeadlineSeconds = 5, pixelsOrHardwareProven = false },
                primary = primary?.SourceException.ToString(), cleanupErrors = cleanupErrors.Select(error => error.ToString()).ToArray(),
                testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
                testMvid = assembly.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription,
                architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, pid = Environment.ProcessId
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception error) { cleanupErrors.Add(error); }
        state.Events = null;
        if (primary is not null)
        {
            if (cleanupErrors.Count > 0) primary.SourceException.Data["AsyncSharedOwnershipCleanup"] = cleanupErrors.ToArray();
            primary.Throw();
        }
        if (cleanupErrors.Count > 0) throw new AggregateException(message, cleanupErrors);
        Assert.IsTrue(completed && cleaned && SharedOwnershipAccepts(outcome), message);
        Assert.IsTrue(earlyDisposeNegativeRejected && missingDisposeNegativeRejected, message);
        if (roundState is not null) Assert.IsTrue(trendCleaned, message);
    }
}
