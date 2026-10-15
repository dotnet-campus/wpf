using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class CompositionNotificationTests
{
    private sealed record Observation(string Name, int HResult, uint Value);

    [TestMethod]
    [DataRow((byte)0)]
    [DataRow((byte)1)]
    public void WhenNotificationFamilyDrivesWindowAndWaitThenOriginalAndCandidateAgree(byte synchronous)
    {
        string reference = Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll");
        var original = Observe(reference, synchronous);
        var candidate = Observe(AcceptancePaths.AotDll, synchronous);
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"notifications-{synchronous}-{Guid.NewGuid():N}.json"),
            JsonSerializer.Serialize(new { Reference = Identity(reference), Candidate = Identity(AcceptancePaths.AotDll), Original = original, Actual = candidate }, new JsonSerializerOptions { WriteIndented = true }));
        CollectionAssert.AreEqual(original, candidate);
    }

    [TestMethod]
    public void WhenCompositionLockIsRecursiveThenAnotherThreadCannotEnterUntilFinalExit()
    {
        nint module = NativeLibrary.Load(AcceptancePaths.AotDll);
        nint enter = NativeLibrary.GetExport(module, "MilCompositionEngine_EnterCompositionEngineLock");
        nint exit = NativeLibrary.GetExport(module, "MilCompositionEngine_ExitCompositionEngineLock");
        using var started = new ManualResetEventSlim();
        using var acquired = new ManualResetEventSlim();
        ((delegate* unmanaged[Stdcall]<void>)enter)();
        ((delegate* unmanaged[Stdcall]<void>)enter)();
        var worker = new Thread(() =>
        {
            started.Set();
            ((delegate* unmanaged[Stdcall]<void>)enter)();
            try { acquired.Set(); }
            finally { ((delegate* unmanaged[Stdcall]<void>)exit)(); }
        });
        worker.Start();
        bool before, afterOne;
        try
        {
            Assert.IsTrue(started.Wait(5000));
            before = acquired.Wait(100);
            ((delegate* unmanaged[Stdcall]<void>)exit)();
            afterOne = acquired.Wait(100);
        }
        finally { ((delegate* unmanaged[Stdcall]<void>)exit)(); }
        Assert.IsTrue(worker.Join(5000));
        CollectionAssert.AreEqual(new[] { false, false, true }, new[] { before, afterOne, acquired.IsSet });
    }

    private static object Identity(string path) => new { Path = path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) };

    private static List<Observation> Observe(string path, byte synchronous)
    {
        nint module = NativeLibrary.Load(path);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)NativeLibrary.GetExport(module, "MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "MilCompositionEngine_DeinitializePartitionManager");
        var eventId = (delegate* unmanaged[Stdcall]<uint*, int>)NativeLibrary.GetExport(module, "MilCompositionEngine_GetComposedEventId");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var create = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var marshal = (delegate* unmanaged[Stdcall]<nint, uint*, int>)NativeLibrary.GetExport(module, "MilChannel_GetMarshalType");
        var broadcast = (delegate* unmanaged[Stdcall]<nint, byte, int>)NativeLibrary.GetExport(module, "MilChannel_SetReceiveBroadcastMessages");
        var notify = (delegate* unmanaged[Stdcall]<nint, nint, uint, int>)NativeLibrary.GetExport(module, "MilChannel_SetNotificationWindow");
        var wait = (delegate* unmanaged[Stdcall]<nint, uint, nint*, int, uint, uint*, int>)NativeLibrary.GetExport(module, "MilComposition_WaitForNextMessage");
        var peek = (delegate* unmanaged[Stdcall]<nint, void*, nuint, int*, int>)NativeLibrary.GetExport(module, "MilComposition_PeekNextMessage");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilComposition_SyncFlush");
        var observations = new List<Observation>();
        void Add(string name, int hr, uint value = 0) => observations.Add(new(name, hr, value));
        nint connection = 0, channel = 0;
        nint window = CreateWindowExW(0, "STATIC", "", 0, 0, 0, 1, 1, new nint(-3), 0, 0, 0);
        Assert.AreNotEqual(nint.Zero, window);
        const uint notification = 0x8123;
        Add("initialize", initialize(0));
        try
        {
            uint value = 0x13572468;
            int hr = eventId(&value); Add("composed-event-preserves-default-thread-output", hr, value);
            Add("connect", connect(synchronous, &connection));
            Add("create", create(connection, 0, &channel));
            value = 0;
            hr = marshal(channel, &value); Add("marshal", hr, value);
            Add("null-marshal", marshal(channel, null));
            Add("broadcast-off", broadcast(channel, 0));
            Add("broadcast-on", broadcast(channel, 1));
            Add("window", notify(channel, window, notification));
            Add("window-repeat", notify(channel, window, notification));
            Add("null-channel-window", notify(0, window, notification));
            Add("null-channel-broadcast", broadcast(0, 1));
            Add("null-channel-marshal", marshal(0, &value));
            value = 123;
            hr = wait(channel, 0, null, 0, 0, &value); Add("empty-timeout", hr, value);
            using var external = new EventWaitHandle(true, EventResetMode.ManualReset);
            nint handle = external.SafeWaitHandle.DangerousGetHandle();
            hr = wait(channel, 1, &handle, 0, 1000, &value); Add("external-event", hr, value);
            Add("null-wait-output", wait(channel, 0, null, 0, 0, null));
            Add("null-handles", wait(channel, 1, null, 0, 0, &value));
            Add("too-many-handles", wait(channel, 64, &handle, 0, 0, &value));
            nint invalidHandle = 0;
            value = 123;
            hr = wait(channel, 1, &invalidHandle, 0, 0, &value); Add("invalid-external-handle", hr, value);
            hr = wait(channel, 1, &handle, 0, 0, &value); Add("wait-after-handle-failure", hr, value);
            uint* packet = stackalloc uint[] { 3, 1 };
            Add("send-register", send(packet, 8, 0, channel));
            hr = wait(channel, 0, null, 0, 3000, &value); Add("command-queue-wake", hr, value);
            Add("flush", flush(channel));
            Message msg;
            Add("window-message", 0, PeekMessageW(&msg, window, notification, notification, 1) != 0 ? 1u : 0u);
            hr = wait(channel, 1, &handle, 1, 0, &value); Add("nonempty-precedes-wait-all", hr, value);
            byte* bytes = stackalloc byte[64];
            int retrieved = 0;
            hr = peek(channel, bytes, 64, &retrieved); Add("queue-callback-type", hr, retrieved != 0 ? *(uint*)bytes : 0);
            hr = wait(channel, 0, null, 0, 0, &value); Add("drained-timeout", hr, value);
            Add("send-register-repeat", send(packet, 8, 0, channel));
            Add("flush-register-repeat", flush(channel));
            hr = peek(channel, bytes, 64, &retrieved); Add("repeat-register-empty", hr, (uint)retrieved);
            Add("repeat-register-no-window-message", 0, PeekMessageW(&msg, window, notification, notification, 1) != 0 ? 1u : 0u);
            packet[1] = 0;
            Add("send-unregister", send(packet, 8, 0, channel));
            Add("send-unregister-repeat", send(packet, 8, 0, channel));
            Add("flush-unregister", flush(channel));
            hr = peek(channel, bytes, 64, &retrieved); Add("unregister-empty", hr, (uint)retrieved);
            Add("detach-window", notify(channel, 0, 0));
            Add("detach-window-repeat", notify(channel, 0, 0));
            packet[1] = 1;
            Add("send-register-detached", send(packet, 8, 0, channel));
            hr = wait(channel, 0, null, 0, 3000, &value); Add("detached-queue-wake", hr, value);
            Add("flush-detached", flush(channel));
            hr = peek(channel, bytes, 64, &retrieved); Add("detached-callback-type", hr, retrieved != 0 ? *(uint*)bytes : 0);
            Add("detached-no-window-message", 0, PeekMessageW(&msg, window, notification, notification, 1) != 0 ? 1u : 0u);
            Add("reattach-window", notify(channel, window, notification));
            packet[1] = 0;
            Add("unregister-before-close", send(packet, 8, 0, channel));
            Add("flush-before-close", flush(channel));
            Add("destroy", destroy(channel)); channel = 0;
            Add("disconnect", disconnect(connection)); connection = 0;
        }
        finally
        {
            if (channel != 0) destroy(channel);
            if (connection != 0) disconnect(connection);
            Add("deinitialize", deinitialize());
            DestroyWindow(window);
        }
        return observations;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Message { public nint Window; public uint Id; public nuint WParam; public nint LParam; public uint Time; public int X, Y; public uint Private; }
    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowExW(uint exStyle, string className, string name, uint style, int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int PeekMessageW(Message* message, nint window, uint min, uint max, uint remove);
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int DestroyWindow(nint window);
}
