using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class AsyncUceChannelTests
{
    [TestMethod]
    public void WhenAsyncBatchesAreCommittedThenSchedulerComposesPixelsInFifoOrder()
    {
        using var api = new Api();
        using var wic = new SystemWicBitmap();
        nint factory = 0, output = 0, target = 0, owner = 0, back = 0;
        try
        {
            var createFactory = (delegate* unmanaged[Stdcall]<nint*, uint, int>)api.Export("MILCreateFactory");
            var createTarget = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)api.Export("MILFactoryCreateSWRenderTargetForBitmap");
            var createDouble = (delegate* unmanaged[Stdcall]<uint, uint, double, double, Guid*, nint, nint*, int>)api.Export("MILSwDoubleBufferedBitmapCreate");
            var getBack = (delegate* unmanaged[Stdcall]<nint, nint*, uint*, int>)api.Export("MILSwDoubleBufferedBitmapGetBackBuffer");
            Assert.AreEqual(0, createFactory(&factory, 0x200184C0));
            output = wic.Create(2, 1);
            Assert.AreEqual(0, createTarget(factory, output, &target));
            Guid format = new("6fddc324-4e03-4bfe-b185-3d77768dc910");
            Assert.AreEqual(0, createDouble(2, 1, 96, 96, &format, 0, &owner));
            uint size = 0;
            Assert.AreEqual(0, getBack(owner, &back, &size));
            uint image = api.Resource(96), data = api.Resource(43), visual = api.Resource(39), destination = api.Resource(47);
            TargetPacket targetPacket = new() { Type = 0x34, Handle = destination, Target = (ulong)target, Width = 2, Height = 1 };
            api.Send(&targetPacket, sizeof(TargetPacket));
            ImagePacket imagePacket = new() { Type = 0x3b, Handle = image, Bitmap = (ulong)owner };
            AddRef(owner);
            api.Send(&imagePacket, sizeof(ImagePacket));
            DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 2, Height = 1, Image = image };
            api.Send(&draw, sizeof(DrawPacket));
            uint* binding = stackalloc uint[] { 0x22, visual, data };
            api.Send(binding, 12);
            binding[0] = 0x35; binding[1] = destination; binding[2] = visual;
            api.Send(binding, 12);
            api.CommitBatch();
            Write(back, 0xffff0000, 0xff00ff00);
            var dirty = (delegate* unmanaged[Stdcall]<nint, int*, int>)api.Export("MILSwDoubleBufferedBitmapAddDirtyRect");
            var protect = (delegate* unmanaged[Stdcall]<nint, int>)api.Export("MILSwDoubleBufferedBitmapProtectBackBuffer");
            int* rect = stackalloc int[] { 0, 0, 2, 1 };
            Assert.AreEqual(0, dirty(owner, rect));
            Assert.AreEqual(0, protect(owner));
            CopyPacket copy = new() { Type = 0x3c, Handle = image };
            api.Send(&copy, sizeof(CopyPacket));
            AlphaPacket alpha = new() { Type = 0x20, Handle = visual, Alpha = 0.25 };
            api.Send(&alpha, sizeof(AlphaPacket));
            api.CommitBatch();
            alpha.Alpha = 1;
            api.Send(&alpha, sizeof(AlphaPacket));
            api.CommitBatch();
            Assert.AreEqual(0, api.Flush());
            uint* pixels = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, pixels));
            Assert.AreEqual((0xffff0000u, 0xff00ff00u), (pixels[0], pixels[1]));
        }
        finally
        {
            api.CloseChannel();
            Release(back); Release(owner); Release(target); Release(output); Release(factory);
        }
    }

    [TestMethod]
    public void WhenAsyncPartitionFailsThenFlushPreservesFirstHResultAcrossLaterBatches()
    {
        using var api = new Api();
        uint invalid = uint.MaxValue;
        api.Send(&invalid, 4);
        api.CommitBatch();
        uint* later = stackalloc uint[] { 0x24, uint.MaxValue };
        api.Send(later, 8);
        api.CommitBatch();
        Assert.AreEqual(unchecked((int)0x88980406), api.Flush());
        api.CommitBatch();
        Assert.AreEqual(unchecked((int)0x88980406), api.Flush());
    }

    [TestMethod]
    public void WhenAsyncConnectionReferenceIsReleasedThenChannelStillDrainsAndCloses()
    {
        using var api = new Api();
        uint visual = api.Resource(39);
        api.CommitBatch();
        api.Disconnect();
        uint* update = stackalloc uint[] { 0x24, visual };
        api.Send(update, 8);
        api.CommitBatch();
        Assert.AreEqual(0, api.Flush());
        api.CloseChannel();
    }

    [TestMethod]
    public void WhenAsyncFailedBatchIsClosedThenTransportReleaseRunsOnCompositionThreadAndRejectsReentry()
    {
        using var api = new Api();
        CallbackState state = new() { Channel = api.Channel, Destroy = (delegate* unmanaged[Stdcall]<nint, int>)api.Export("MilConnection_DestroyChannel") };
        Descriptor descriptor = new() { Dispose = &DisposeEvent, Raise = &RaiseEvent, State = &state };
        nint instance = 0;
        var create = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)api.Export("MILCreateEventProxy");
        Assert.AreEqual(0, create(&descriptor, &instance));
        uint invalid = uint.MaxValue;
        api.Send(&invalid, 4);
        api.CommitBatch();
        BitmapPacket packet = new() { Type = 0x0c, Handle = uint.MaxValue, Bitmap = instance };
        api.Send(&packet, sizeof(BitmapPacket));
        instance = 0;
        int callerThread = Environment.CurrentManagedThreadId;
        api.CloseChannel();
        Assert.AreEqual((1, unchecked((int)0x80070057)), (state.Disposals, state.Result));
        Assert.AreNotEqual(callerThread, state.Thread);
    }

    [TestMethod]
    public void WhenPartitionManagerIsInitializedTwiceThenOneDeinitializationKeepsAsyncWorkAlive()
    {
        nint module = NativeLibrary.Load(AcceptancePaths.AotDll);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)NativeLibrary.GetExport(module, "MilCompositionEngine_InitializePartitionManager");
        var update = (delegate* unmanaged[Stdcall]<int, int>)NativeLibrary.GetExport(module, "MilCompositionEngine_UpdateSchedulerSettings");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "MilCompositionEngine_DeinitializePartitionManager");
        Assert.AreEqual(0, initialize(0));
        Assert.AreEqual(0, initialize(0));
        try
        {
            using var api = new Api();
            Assert.AreEqual(0, deinitialize());
            Assert.AreEqual(0, update(0));
            api.Resource(39);
            api.CommitBatch();
            Assert.AreEqual(0, api.Flush());
        }
        finally { Assert.AreEqual(0, deinitialize()); }
    }

    [TestMethod]
    public void WhenInvalidPriorityInitializationFailsThenNoSchedulerReferenceRemains()
    {
        nint module = NativeLibrary.Load(AcceptancePaths.AotDll);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)NativeLibrary.GetExport(module, "MilCompositionEngine_InitializePartitionManager");
        var update = (delegate* unmanaged[Stdcall]<int, int>)NativeLibrary.GetExport(module, "MilCompositionEngine_UpdateSchedulerSettings");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)NativeLibrary.GetExport(module, "MilCompositionEngine_DeinitializePartitionManager");
        Assert.AreEqual(unchecked((int)0x80070057), initialize(3));
        Assert.AreEqual(0, deinitialize());
        Assert.AreEqual(unchecked((int)0x8000FFFF), update(0));
    }

    [TestMethod]
    [Timeout(15000)]
    public void WhenShutdownDrainsClosedAndOpenFailedBatchesThenReleasesAreFifoAndWorkerExits()
    {
        using var api = new Api();
        int sequence = 0;
        CallbackState first = new() { Channel = api.Channel, Destroy = (delegate* unmanaged[Stdcall]<nint, int>)api.Export("MilConnection_DestroyChannel"), Sequence = &sequence };
        CallbackState second = new() { Channel = api.Channel, Destroy = first.Destroy, Sequence = &sequence };
        Descriptor one = new() { Dispose = &DisposeEvent, Raise = &RaiseEvent, State = &first };
        Descriptor two = new() { Dispose = &DisposeEvent, Raise = &RaiseEvent, State = &second };
        var create = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)api.Export("MILCreateEventProxy");
        nint firstProxy = 0, secondProxy = 0;
        Assert.AreEqual(0, create(&one, &firstProxy));
        Assert.AreEqual(0, create(&two, &secondProxy));
        uint invalid = uint.MaxValue;
        api.Send(&invalid, 4);
        BitmapPacket packet = new() { Type = 0x0c, Handle = uint.MaxValue, Bitmap = firstProxy };
        api.Send(&packet, sizeof(BitmapPacket));
        api.CommitBatch();
        packet.Bitmap = secondProxy;
        api.Send(&packet, sizeof(BitmapPacket));
        api.Disconnect();
        api.CloseChannel();
        Assert.AreEqual((1, 1, 1, 2), (first.Disposals, first.Order, second.Disposals, second.Order));
        Assert.AreEqual(first.NativeThread, second.NativeThread);
        uint worker = first.NativeThread;
        Assert.IsTrue(SpinWait.SpinUntil(() => !ThreadExists(worker), TimeSpan.FromSeconds(5)), "Composition worker must exit after the last connection/channel reference.");
    }

    [TestMethod]
    [Timeout(15000)]
    public void WhenReleaseCallbackClosesAnotherChannelThenQueuedCleanupDoesNotDeadlock()
    {
        using var api = new Api();
        nint other = 0;
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)api.Export("MilConnection_CreateChannel"))(api.Connection, api.Channel, &other));
        CallbackState state = new() { Channel = other, Destroy = (delegate* unmanaged[Stdcall]<nint, int>)api.Export("MilConnection_DestroyChannel") };
        Descriptor descriptor = new() { Dispose = &DisposeEvent, Raise = &RaiseEvent, State = &state };
        nint proxy = 0;
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)api.Export("MILCreateEventProxy"))(&descriptor, &proxy));
        uint invalid = uint.MaxValue;
        api.Send(&invalid, 4);
        BitmapPacket packet = new() { Type = 0x0c, Handle = uint.MaxValue, Bitmap = proxy };
        api.Send(&packet, sizeof(BitmapPacket));
        api.CloseChannel();
        api.Disconnect();
        Assert.AreEqual((1, 0), (state.Disposals, state.Result));
        Assert.AreEqual(unchecked((int)0x80070057), state.Destroy(other));
        uint worker = state.NativeThread;
        Assert.IsTrue(SpinWait.SpinUntil(() => !ThreadExists(worker), TimeSpan.FromSeconds(5)));
    }

    private static bool ThreadExists(uint id)
    {
        using var process = System.Diagnostics.Process.GetCurrentProcess();
        return process.Threads.Cast<System.Diagnostics.ProcessThread>().Any(thread => thread.Id == id);
    }

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern uint GetCurrentThreadId();

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapPacket { internal uint Type, Handle; internal nint Bitmap; }
    private struct CallbackState
    {
        internal nint Channel;
        internal delegate* unmanaged[Stdcall]<nint, int> Destroy;
        internal int Disposals, Result, Thread, Order;
        internal uint NativeThread;
        internal int* Sequence;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal delegate* unmanaged[Stdcall]<Descriptor*, void> Dispose;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, int> Raise;
        internal CallbackState* State;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void DisposeEvent(Descriptor* descriptor)
    {
        descriptor->State->Disposals++;
        descriptor->State->Thread = Environment.CurrentManagedThreadId;
        descriptor->State->NativeThread = GetCurrentThreadId();
        if (descriptor->State->Sequence != null) descriptor->State->Order = ++*descriptor->State->Sequence;
        descriptor->State->Result = descriptor->State->Destroy(descriptor->State->Channel);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int RaiseEvent(Descriptor* descriptor, byte* data, uint size) => 0;

    private static void Write(nint bitmap, uint first, uint second)
    {
        nint bitmapLock = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock));
            uint size; uint* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, uint**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels));
            pixels[0] = first; pixels[1] = second;
        }
        finally { Release(bitmapLock); }
    }
    private static void AddRef(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[1])(value);
    private static void Release(nint value) { if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value); }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TargetPacket { internal uint Type, Handle; internal ulong Window, Target; internal uint Width, Height, Dummy; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ImagePacket { internal uint Type, Handle; internal ulong Bitmap; internal int Back; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct CopyPacket { internal uint Type, Handle; internal ulong Event; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct AlphaPacket { internal uint Type, Handle; internal double Alpha; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DrawPacket { internal uint Type, Handle, DataSize, RecordSize, Command; internal double X, Y, Width, Height; internal uint Image, Padding; }

    private sealed class Api : IDisposable
    {
        private readonly nint _module = NativeLibrary.Load(AcceptancePaths.AotDll);
        private nint _connection, _channel;
        internal nint Export(string name) => NativeLibrary.GetExport(_module, name);
        internal nint Channel => _channel;
        internal nint Connection => _connection;
        internal Api()
        {
            nint connection = 0, channel = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create"))(0, &connection));
            _connection = connection;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel"))(connection, 0, &channel));
            _channel = channel;
        }
        internal uint Resource(uint type)
        {
            uint value = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel"))(_channel, type, &value));
            return value;
        }
        internal void Send(void* packet, int size) => Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand"))(packet, (uint)size, 0, _channel));
        internal void CommitBatch()
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch"))(_channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel"))(_channel));
        }
        internal int Flush() => ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush"))(_channel);
        internal void Disconnect()
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect"))(_connection));
            _connection = 0;
        }
        internal void CloseChannel()
        {
            if (_channel == 0) return;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel"))(_channel));
            _channel = 0;
        }
        public void Dispose()
        {
            CloseChannel();
            if (_connection != 0) Disconnect();
        }
    }
}
