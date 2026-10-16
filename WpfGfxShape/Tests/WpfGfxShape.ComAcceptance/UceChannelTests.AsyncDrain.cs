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

    private static void RunAsyncWicDrain(bool system, int queueState)
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
            if (owned != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)owned)[2])(owned);
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
                releaseThread = DrainGetCurrentThreadId();
                if (!system) reentry = ((delegate* unmanaged[Stdcall]<nint, int>)destroyEntry)(sibling);
                Interlocked.Increment(ref disposals);
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
            Check("Transfer", transfer(bitmap, wrapper, channel));
            ReleaseCom(ref wrapper); ReleaseCom(ref frame);
            int state = queueState % 3;
            if (state >= 1) Check("CloseBatch", close(channel));
            if (state >= 2) Check("Commit", commit(channel));
            Check("Disconnect.ReferenceOnly", disconnect(connection)); connection = 0;
            if (!directClose)
            {
                int afterDisconnect = flush(channel);
                calls.Add(new { operation = "Flush.AfterDisconnect", result = afterDisconnect });
                Assert.AreEqual(failed ? unchecked((int)0x88980406) : 0, afterDisconnect);
                Assert.AreEqual(0, Volatile.Read(ref disposals), "External stream remains owned after disconnect/flush.");
            }
            ReleaseCom(ref stream);
            if (!failed) Assert.AreEqual(0, Volatile.Read(ref disposals), "Pending transport or executed bitmap source retains stream after external release.");
            Check("Destroy.Drain", destroy(channel)); channel = 0;
            Assert.IsTrue(SpinWait.SpinUntil(() => Volatile.Read(ref disposals) == 1, TimeSpan.FromSeconds(5)));
            if (!failed || (directClose && state < 2)) Assert.AreNotEqual(DrainGetCurrentThreadId(), releaseThread);
            if (!system)
            {
                Check("Callback.DestroySibling", reentry); other = 0;
            }
            else
            {
                Check("Destroy.Sibling", destroy(other)); other = 0;
            }
            Check("Deinitialize", deinitialize()); initialized = false;
            if (!system && (!failed || (directClose && state < 2)))
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
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"async-wic-drain-{system}-{queueState}.json"),
                JsonSerializer.Serialize(new { system, queueState, completed, path, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), disposals, releaseThread, reentry, calls }, new JsonSerializerOptions { WriteIndented = true }));
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
