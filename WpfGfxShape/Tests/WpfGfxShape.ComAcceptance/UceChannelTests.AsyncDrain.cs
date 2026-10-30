using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    /// <summary>Exercises native asynchronous drain with real WIC transport and external ownership.</summary>
    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    [DataRow(11)]
    [Timeout(30000)]
    public void WhenAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates(int queueState) => RunAsyncWicDrain(false, queueState);

    /// <summary>Runs the same normal protocol independently against the original native library.</summary>
    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [Timeout(30000)]
    public void WhenOriginalAsyncWicTransportIsDisconnectedThenCloseDrainsAllQueueStates(int queueState) => RunAsyncWicDrain(true, queueState);

#if HS_NOTIFICATION_CLOSE
    /// <summary>Diagnoses whether close relinquishes transport ownership before the caller releases its surviving stream.</summary>
    [DataTestMethod]
    [DataRow(true, 6)]
    [DataRow(true, 7)]
    [DataRow(true, 8)]
    [DataRow(false, 6)]
    [DataRow(false, 7)]
    [DataRow(false, 8)]
    [Timeout(30000)]
    public void WhenAsyncCloseReturnsThenSurvivingExternalStreamReleasesExactlyOnce(bool system, int queueState)
    {
        string root = Path.Combine(AcceptancePaths.Results, "hu-roles", $"{system}-{queueState}");
        Directory.CreateDirectory(root);
        var start = new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException())
        {
            UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory
        };
        foreach (string argument in new[] { "--filter", "FullyQualifiedName~WhenHuCloseRoleRunsThenFinalStreamDisposalIsRecorded", "--report-trx", "--report-trx-filename", "role.trx", "--results-directory", root, "--no-progress" })
            start.ArgumentList.Add(argument);
        start.Environment["HU_CLOSE_ROLE"] = $"{system}:{queueState}";
        using var process = System.Diagnostics.Process.Start(start) ?? throw new InvalidOperationException();
        bool exited = process.WaitForExit(25000);
        if (!exited) { process.Kill(true); process.WaitForExit(); }
        File.WriteAllText(Path.Combine(root, "exit.json"), JsonSerializer.Serialize(new { exited, pid = process.Id, exitCode = process.ExitCode, arguments = start.ArgumentList.ToArray() }));
        Assert.IsTrue(exited);
        Assert.AreEqual(0, process.ExitCode);
    }

    /// <summary>Executes exactly one module and queue state in a fresh formal test process.</summary>
    [TestMethod]
    [Timeout(22000)]
    public void WhenHuCloseRoleRunsThenFinalStreamDisposalIsRecorded()
    {
        string[] role = (Environment.GetEnvironmentVariable("HU_CLOSE_ROLE") ?? throw new InvalidOperationException()).Split(':');
        RunAsyncWicDrain(bool.Parse(role[0]), int.Parse(role[1], System.Globalization.CultureInfo.InvariantCulture), retainExternal: true);
    }
#endif

    private static void RunAsyncWicDrain(bool system, int queueState, bool retainExternal = false)
    {
        string path = system
            ? Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll")
            : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var transfer = (delegate* unmanaged[Stdcall]<uint, nint, nint, int>)Export("MilResource_SendCommandBitmapSource");
        var wrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)Export("MilResource_CreateCWICWrapperBitmap");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        using var wic = new SystemWicBitmap();
        nint connection = 0, channel = 0, other = 0, stream = 0, frame = 0, wrapper = 0;
        int disposals = 0, reentry = int.MinValue;
        int sequence = 0, closeReturned = 0, callbackOrder = 0;
        int callbackExited = 0;
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
        WicFinalReleaseCapture? ownerCapture = system && queueState == 6 && !retainExternal ? new() : null;
#endif
        string stage = "setup";
        var events = new System.Collections.Concurrent.ConcurrentQueue<object>();
        var waits = new List<object>();
        void Event(string name) => events.Enqueue(new { order = Interlocked.Increment(ref sequence), name, stage = Volatile.Read(ref stage), thread = DrainGetCurrentThreadId(), disposals = Volatile.Read(ref disposals) });
        bool WaitForCallback(string phase)
        {
            Volatile.Write(ref stage, phase);
            Event("wait.before");
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (Volatile.Read(ref callbackExited) == 0 && !cancellation.IsCancellationRequested)
                cancellation.Token.WaitHandle.WaitOne(10);
            bool finished = Volatile.Read(ref callbackExited) != 0;
            waits.Add(new { phase, finished, condition = "callback.exit observed", disposals = Volatile.Read(ref disposals) });
            Event("wait.after");
            return finished;
        }
        uint externalReleaseReferences = uint.MaxValue;
        var loaded = System.Diagnostics.Process.GetCurrentProcess().Modules.Cast<System.Diagnostics.ProcessModule>().Single(item => item.BaseAddress == module);
        string actualPath = loaded.FileName;
        uint releaseThread = 0;
        bool completed = false, initialized = false;
        List<object> calls = [];
        void Check(string operation, int result)
        {
            calls.Add(new { operation, result });
            Assert.AreEqual(0, result, operation);
        }
        void ReleaseCom(ref nint value)
        {
            nint owned = value;
            value = 0;
            if (owned != 0)
            {
                uint references = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)owned)[2])(owned);
                calls.Add(new { operation = "Release.External", references });
            }
        }
        try
        {
            Check("Initialize", initialize(0)); initialized = true;
            Check("Connect.Async", connect(0, &connection));
            Check("Channel", channelCreate(connection, 0, &channel));
            Check("Sibling", channelCreate(connection, channel, &other));
            nint sibling = other;
            uint bitmap = 0;
            Check("Create.Bitmap", create(channel, 95, &bitmap));
            byte[] bmp = new byte[62];
            bmp[0] = (byte)'B'; bmp[1] = (byte)'M';
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(2), bmp.Length);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(10), 54);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(14), 40);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(18), 2);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(22), 1);
            bmp[26] = 1; bmp[28] = 24; bmp[56] = 255;
            nint destroyEntry = (nint)destroy;
            stream = ManagedStreamTests.CreateCallbackStream(module, bmp, () => { }, () =>
            {
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
                ownerCapture?.Capture();
#endif
                Event("callback.enter");
                releaseThread = DrainGetCurrentThreadId();
                if (!system && !retainExternal) reentry = ((delegate* unmanaged[Stdcall]<nint, int>)destroyEntry)(sibling);
                callbackOrder = Interlocked.Increment(ref sequence);
                Interlocked.Increment(ref disposals);
                Event("callback.exit");
                Volatile.Write(ref callbackExited, 1);
            });
            frame = wic.DecodeFrame(stream);
            Check("Wrap", wrap(frame, &wrapper));
            bool failed = queueState % 6 >= 3;
            bool directClose = queueState >= 6;
            if (failed)
            {
                var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
                DrainBitmapPacket bad = new() { Type = 0x0c, Handle = bitmap };
                Check("Send.CompleteNullSource", send(&bad, (uint)sizeof(DrainBitmapPacket), 0, channel));
                int first = flush(channel);
                calls.Add(new { operation = "Flush.NullSourceFailure", result = first });
                Assert.AreEqual(unchecked((int)0x88980406), first);
            }
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(1);
#endif
            Check("Transfer", transfer(bitmap, wrapper, channel));
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(2);
#endif
            ReleaseCom(ref wrapper);
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(3);
#endif
            ReleaseCom(ref frame);
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(4);
#endif
            int state = queueState % 3;
            if (state >= 1) Check("CloseBatch", close(channel));
            if (state >= 2) Check("Commit", commit(channel));
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(5);
#endif
            Check("Disconnect.ReferenceOnly", disconnect(connection)); connection = 0;
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(6);
#endif
            if (!directClose)
            {
                int afterDisconnect = flush(channel);
                calls.Add(new { operation = "Flush.AfterDisconnect", result = afterDisconnect });
                Assert.AreEqual(failed ? unchecked((int)0x88980406) : 0, afterDisconnect);
                Assert.AreEqual(0, Volatile.Read(ref disposals), "External stream remains owned after disconnect/flush.");
            }
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(7);
#endif
            if (!retainExternal) ReleaseCom(ref stream);
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(8);
#endif
            if (!failed) Assert.AreEqual(0, Volatile.Read(ref disposals), "Pending transport or executed bitmap source retains stream after external release.");
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(9);
#endif
            Check("Destroy.Drain", destroy(channel)); channel = 0;
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(10);
#endif
            closeReturned = Interlocked.Increment(ref sequence);
            Event("closeReturn");
            if (retainExternal)
            {
                Assert.AreEqual(0, Volatile.Read(ref disposals));
                nint owned = stream;
                stream = 0;
                Volatile.Write(ref stage, "externalRelease");
                Event("externalRelease.before");
                externalReleaseReferences = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)owned)[2])(owned);
                Event("externalRelease.after");
                calls.Add(new { operation = "Release.SurvivingExternal", references = externalReleaseReferences });
                _ = WaitForCallback("afterExternalRelease");
            }
            else Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref disposals) == 1, TimeSpan.FromSeconds(5)));
            if (!retainExternal) Assert.IsTrue(WaitForCallback("afterClose"));
            if (!system && !retainExternal && (!failed || (directClose && state < 2))) Assert.AreNotEqual(DrainGetCurrentThreadId(), releaseThread);
            if (!system && !retainExternal)
            {
                Check("Callback.DestroySibling", reentry); other = 0;
            }
            else
            {
                Volatile.Write(ref stage, "siblingDestroy");
                Event("siblingDestroy.before");
                Check("Destroy.Sibling", destroy(other)); other = 0;
                Event("siblingDestroy.after");
                if (retainExternal) _ = WaitForCallback("afterSiblingDestroy");
            }
            Volatile.Write(ref stage, "deinitialize");
            Event("deinit.before");
            Check("Deinitialize", deinitialize()); initialized = false;
            Event("deinit.after");
            if (retainExternal)
            {
                Assert.IsTrue(WaitForCallback("afterDeinitialize"));
                Assert.IsTrue(callbackOrder > closeReturned);
            }
            if (!retainExternal && !system && (!failed || (directClose && state < 2)))
                Assert.IsTrue(SpinWait.SpinUntil(() => !DrainThreadExists(releaseThread), TimeSpan.FromSeconds(5)), "Composition worker must exit after deinitialization.");
            Assert.AreEqual(1, Volatile.Read(ref disposals));
            completed = true;
        }
        finally
        {
            if (channel != 0) _ = destroy(channel);
            if (other != 0) _ = destroy(other);
            if (connection != 0) _ = disconnect(connection);
            ReleaseCom(ref wrapper); ReleaseCom(ref frame); ReleaseCom(ref stream);
            if (initialized) _ = deinitialize();
#if WIC_FINAL_RELEASE_OWNER_DIAGNOSTIC
            ownerCapture?.Mark(11);
            if (Volatile.Read(ref callbackExited) != 0) ownerCapture?.Write();
#endif
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"async-wic-drain-{system}-{queueState}-{retainExternal}.json"),
                JsonSerializer.Serialize(new { system, queueState, retainExternal, completed, path, actualPath, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actualPath))), version = loaded.FileVersionInfo.FileVersion, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), testMvid = typeof(UceChannelTests).Assembly.ManifestModule.ModuleVersionId, testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(UceChannelTests).Assembly.Location))), pid = Environment.ProcessId, events = events.ToArray(), waits, callbackExited, closeReturned, callbackOrder, externalReleaseReferences, disposals, releaseThread, reentry, calls }, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DrainBitmapPacket { internal uint Type, Handle; internal nint Bitmap; }

    private static bool DrainThreadExists(uint id)
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return process.Threads.Cast<System.Diagnostics.ProcessThread>().Any(thread => thread.Id == id);
    }

    [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
    private static extern uint DrainGetCurrentThreadId();
}
