using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceStateBlockCreationTests
{
    private static Stateblocktype _type;
    private static nint _stateBlockToReturn;
    private static int _createResult;
    private static int _createCallCount;
    private static int _stateBlockReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _type = 0;
        _stateBlockToReturn = 0;
        _createResult = 0;
        _createCallCount = 0;
        _stateBlockReleaseCount = 0;
    }

    [TestMethod]
    public void WhenCreatingStateBlockAfterDeviceLossWasProcessedThenNativeCreationStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeStateBlockObject stateBlockObject = new();
        _stateBlockToReturn = (nint) stateBlockObject.StateBlock;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.TryCreateStateBlock(Stateblocktype.Pixelstate, out Direct3D9StateBlock? stateBlock);
        nint nativeStateBlock = (nint) stateBlock!.StateBlock;
        stateBlock.Dispose();

        Assert.AreEqual(
            (0, Stateblocktype.Pixelstate, 1, (nint) stateBlockObject.StateBlock, 1),
            (result, _type, _createCallCount, nativeStateBlock, _stateBlockReleaseCount));
    }

    [TestMethod]
    public void WhenStateBlockCreationFailsWithPointerThenPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeStateBlockObject stateBlockObject = new();
        _stateBlockToReturn = (nint) stateBlockObject.StateBlock;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateStateBlock(Stateblocktype.All, out Direct3D9StateBlock? stateBlock);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, Stateblocktype.All, 1, 1, null),
            (result, _type, _createCallCount, _stateBlockReleaseCount, stateBlock));
    }

    [TestMethod]
    public void WhenStateBlockCreationFailsNormallyThenResultIsPreservedWithoutMarkingDeviceUnusable()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateStateBlock(Stateblocktype.Vertexstate, out Direct3D9StateBlock? stateBlock);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, Stateblocktype.Vertexstate, 1, 0, null),
            (result, _type, _createCallCount, device.UnusableReasonHResult, stateBlock));
    }

    [TestMethod]
    public void WhenStateBlockCreationReturnsDeviceLostThenOriginalResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateStateBlock(Stateblocktype.Pixelstate, out Direct3D9StateBlock? stateBlock);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, Stateblocktype.Pixelstate, 1, 0, null),
            (result, _type, _createCallCount, device.UnusableReasonHResult, stateBlock));
    }

    [TestMethod]
    public void WhenStateBlockCreationReturnsNonzeroSuccessThenOriginalHResultAndPointerArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeStateBlockObject stateBlockObject = new();
        _stateBlockToReturn = (nint) stateBlockObject.StateBlock;
        _createResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateStateBlock(Stateblocktype.Vertexstate, out Direct3D9StateBlock? stateBlock);
        nint nativeStateBlock = (nint) stateBlock!.StateBlock;
        stateBlock.Dispose();

        Assert.AreEqual(
            (1, Stateblocktype.Vertexstate, 1, (nint) stateBlockObject.StateBlock, 1),
            (result, _type, _createCallCount, nativeStateBlock, _stateBlockReleaseCount));
    }

    [TestMethod]
    public void WhenStateBlockCreationReturnsDriverInternalErrorThenDeviceErrorIsMappedAndPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeStateBlockObject stateBlockObject = new();
        _stateBlockToReturn = (nint) stateBlockObject.StateBlock;
        _createResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateStateBlock(Stateblocktype.Pixelstate, out Direct3D9StateBlock? stateBlock);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, _stateBlockReleaseCount, stateBlock));
    }

    [TestMethod]
    public void WhenStateBlockIsDisposedRepeatedlyThenNativePointerIsReleasedOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeStateBlockObject stateBlockObject = new();
        _stateBlockToReturn = (nint) stateBlockObject.StateBlock;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryCreateStateBlock(Stateblocktype.All, out Direct3D9StateBlock? stateBlock);

        stateBlock!.Dispose();
        stateBlock.Dispose();

        Assert.AreEqual(1, _stateBlockReleaseCount);
    }

    [TestMethod]
    public void WhenUsingReleasedStateBlockThenNativePointerAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeStateBlockObject stateBlockObject = new();
        _stateBlockToReturn = (nint) stateBlockObject.StateBlock;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryCreateStateBlock(Stateblocktype.Vertexstate, out Direct3D9StateBlock? stateBlock);
        stateBlock!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = stateBlock.StateBlock);
    }

    [TestMethod]
    public void WhenCreatingStateBlockAfterDeviceWasDisposedThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.TryCreateStateBlock(Stateblocktype.All, out _));
        Assert.AreEqual(0, _createCallCount);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self)
    {
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateStateBlock(
        IDirect3DDevice9* self,
        Stateblocktype type,
        IDirect3DStateBlock9** stateBlock)
    {
        _createCallCount++;
        _type = type;
        *stateBlock = (IDirect3DStateBlock9*) _stateBlockToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseStateBlock(IDirect3DStateBlock9* self)
    {
        _stateBlockReleaseCount++;
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 62);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[59] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Stateblocktype, IDirect3DStateBlock9**, int>) &CreateStateBlock;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeStateBlockObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DStateBlock9* StateBlock;

        public FakeStateBlockObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            StateBlock = (IDirect3DStateBlock9*) memory;
            void** vtable = memory + 1;
            StateBlock->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DStateBlock9*, uint>) &ReleaseStateBlock;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            StateBlock = null;
        }
    }
}
