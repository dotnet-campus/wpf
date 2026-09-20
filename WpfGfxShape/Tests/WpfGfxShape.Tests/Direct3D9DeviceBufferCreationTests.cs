using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceBufferCreationTests
{
    private static uint _length;
    private static uint _usage;
    private static uint _flexibleVertexFormat;
    private static Format _format;
    private static Pool _pool;
    private static nint _bufferToReturn;
    private static int _createResult;
    private static int _createCallCount;
    private static bool _sharedHandleWasNull;
    private static int _bufferReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _length = 0;
        _usage = 0;
        _flexibleVertexFormat = 0;
        _format = 0;
        _pool = 0;
        _bufferToReturn = 0;
        _createResult = 0;
        _createCallCount = 0;
        _sharedHandleWasNull = false;
        _bufferReleaseCount = 0;
    }

    [TestMethod]
    public void WhenCreatingVertexBufferThenArgumentsAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateVertexBuffer(4096, 0x208, 0x112, Pool.Default, out Direct3D9VertexBuffer? buffer);
        nint nativeBuffer = (nint) buffer!.VertexBuffer;
        buffer.Dispose();

        Assert.AreEqual(
            (0, 4096u, 0x208u, 0x112u, Pool.Default, 1, true, (nint) bufferObject.VertexBuffer, 1),
            (result, _length, _usage, _flexibleVertexFormat, _pool, _createCallCount, _sharedHandleWasNull,
                nativeBuffer, _bufferReleaseCount));
    }

    [TestMethod]
    public void WhenCreatingVertexBufferReturnsNonzeroSuccessThenResultAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        _createResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateVertexBuffer(
            uint.MaxValue,
            uint.MaxValue,
            uint.MaxValue,
            Pool.Systemmem,
            out Direct3D9VertexBuffer? buffer);
        nint nativeBuffer = (nint) buffer!.VertexBuffer;
        buffer.Dispose();

        Assert.AreEqual(
            (1, uint.MaxValue, uint.MaxValue, uint.MaxValue, Pool.Systemmem, 1, true,
                (nint) bufferObject.VertexBuffer, 1),
            (result, _length, _usage, _flexibleVertexFormat, _pool, _createCallCount, _sharedHandleWasNull,
                nativeBuffer, _bufferReleaseCount));
    }

    [TestMethod]
    public void WhenCreatingIndexBufferThenArgumentsAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateIndexBuffer(2048, 0x208, Format.Index16, Pool.Default, out Direct3D9IndexBuffer? buffer);
        nint nativeBuffer = (nint) buffer!.IndexBuffer;
        buffer.Dispose();

        Assert.AreEqual(
            (0, 2048u, 0x208u, Format.Index16, Pool.Default, 1, true, (nint) bufferObject.IndexBuffer, 1),
            (result, _length, _usage, _format, _pool, _createCallCount, _sharedHandleWasNull, nativeBuffer,
                _bufferReleaseCount));
    }

    [TestMethod]
    public void WhenCreatingIndexBufferReturnsNonzeroSuccessThenResultAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        _createResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateIndexBuffer(
            uint.MaxValue,
            uint.MaxValue,
            Format.Index32,
            Pool.Systemmem,
            out Direct3D9IndexBuffer? buffer);
        nint nativeBuffer = (nint) buffer!.IndexBuffer;
        buffer.Dispose();

        Assert.AreEqual(
            (1, uint.MaxValue, uint.MaxValue, Format.Index32, Pool.Systemmem, 1, true,
                (nint) bufferObject.IndexBuffer, 1),
            (result, _length, _usage, _format, _pool, _createCallCount, _sharedHandleWasNull, nativeBuffer,
                _bufferReleaseCount));
    }

    [TestMethod]
    public void WhenBufferCreationFailsWithPointerThenPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateVertexBuffer(64, 0, 0, Pool.Managed, out Direct3D9VertexBuffer? buffer);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, null),
            (result, _createCallCount, _bufferReleaseCount, buffer));
    }

    [TestMethod]
    public void WhenIndexBufferCreationFailsWithPointerThenPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateIndexBuffer(64, 0, Format.Index16, Pool.Managed, out Direct3D9IndexBuffer? buffer);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, null),
            (result, _createCallCount, _bufferReleaseCount, buffer));
    }

    [TestMethod]
    public void WhenCreatingVertexBufferReturnsOrdinaryFailureThenDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateVertexBuffer(64, 0, 0, Pool.Default, out Direct3D9VertexBuffer? buffer);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, buffer));
    }

    [TestMethod]
    public void WhenCreatingVertexBufferReturnsDeviceLostThenResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateVertexBuffer(64, 0, 0, Pool.Default, out Direct3D9VertexBuffer? buffer);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, buffer));
    }

    [TestMethod]
    public void WhenCreatingIndexBufferReturnsOrdinaryFailureThenDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateIndexBuffer(64, 0, Format.Index16, Pool.Default, out Direct3D9IndexBuffer? buffer);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, buffer));
    }

    [TestMethod]
    public void WhenCreatingIndexBufferReturnsDeviceLostThenResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateIndexBuffer(64, 0, Format.Index32, Pool.Default, out Direct3D9IndexBuffer? buffer);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, buffer));
    }

    [TestMethod]
    public void WhenCreatingVertexBufferAfterDeviceReleaseThenNativeCreationDoesNotRun()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.TryCreateVertexBuffer(64, 0, 0, Pool.Default, out _));
        Assert.AreEqual(0, _createCallCount);
    }

    [TestMethod]
    public void WhenCreatingIndexBufferAfterDeviceReleaseThenNativeCreationDoesNotRun()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.TryCreateIndexBuffer(64, 0, Format.Index16, Pool.Default, out _));
        Assert.AreEqual(0, _createCallCount);
    }

    [TestMethod]
    public void WhenBufferCreationReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        _createResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateIndexBuffer(64, 0, Format.Index32, Pool.Default, out Direct3D9IndexBuffer? buffer);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, null),
            (result, device.UnusableReasonHResult, _bufferReleaseCount, buffer));
    }

    [TestMethod]
    public void WhenCreatingBuffersAfterDeviceLossWasProcessedThenNativeCreationStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.TryCreateVertexBuffer(64, 0, 0, Pool.Default, out Direct3D9VertexBuffer? buffer);
        buffer!.Dispose();

        Assert.AreEqual((0, 1, 1), (result, _createCallCount, _bufferReleaseCount));
    }

    [TestMethod]
    public void WhenVertexBufferIsDisposedTwiceThenNativeBufferIsReleasedOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryCreateVertexBuffer(64, 0, 0, Pool.Default, out Direct3D9VertexBuffer? buffer);

        buffer!.Dispose();
        buffer.Dispose();

        Assert.AreEqual(1, _bufferReleaseCount);
    }

    [TestMethod]
    public void WhenIndexBufferIsDisposedTwiceThenNativeBufferIsReleasedOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryCreateIndexBuffer(64, 0, Format.Index16, Pool.Default, out Direct3D9IndexBuffer? buffer);

        buffer!.Dispose();
        buffer.Dispose();

        Assert.AreEqual(1, _bufferReleaseCount);
    }

    [TestMethod]
    public void WhenUsingReleasedVertexBufferThenNativePointerAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryCreateVertexBuffer(64, 0, 0, Pool.Default, out Direct3D9VertexBuffer? buffer);
        buffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = buffer.VertexBuffer);
    }

    [TestMethod]
    public void WhenUsingReleasedIndexBufferThenNativePointerAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryCreateIndexBuffer(64, 0, Format.Index16, Pool.Default, out Direct3D9IndexBuffer? buffer);
        buffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = buffer.IndexBuffer);
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
    private static int CreateVertexBuffer(
        IDirect3DDevice9* self,
        uint length,
        uint usage,
        uint flexibleVertexFormat,
        Pool pool,
        IDirect3DVertexBuffer9** buffer,
        void** sharedHandle)
    {
        _createCallCount++;
        _length = length;
        _usage = usage;
        _flexibleVertexFormat = flexibleVertexFormat;
        _pool = pool;
        _sharedHandleWasNull = sharedHandle is null;
        *buffer = (IDirect3DVertexBuffer9*) _bufferToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateIndexBuffer(
        IDirect3DDevice9* self,
        uint length,
        uint usage,
        Format format,
        Pool pool,
        IDirect3DIndexBuffer9** buffer,
        void** sharedHandle)
    {
        _createCallCount++;
        _length = length;
        _usage = usage;
        _format = format;
        _pool = pool;
        _sharedHandleWasNull = sharedHandle is null;
        *buffer = (IDirect3DIndexBuffer9*) _bufferToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseBuffer(void* self)
    {
        _bufferReleaseCount++;
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 30);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[26] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, Pool, IDirect3DVertexBuffer9**, void**, int>) &CreateVertexBuffer;
            vtable[27] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DIndexBuffer9**, void**, int>) &CreateIndexBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeBufferObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DVertexBuffer9* VertexBuffer;
        internal IDirect3DIndexBuffer9* IndexBuffer;

        public FakeBufferObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            VertexBuffer = (IDirect3DVertexBuffer9*) memory;
            IndexBuffer = (IDirect3DIndexBuffer9*) memory;
            void** vtable = memory + 1;
            VertexBuffer->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &ReleaseBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            VertexBuffer = null;
            IndexBuffer = null;
        }
    }
}
