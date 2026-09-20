using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceComposeRectsTests
{
    private static nint _source;
    private static nint _destination;
    private static nint _sourceRectDescriptors;
    private static uint _rectangleCount;
    private static nint _destinationRectDescriptors;
    private static Composerectsop _operation;
    private static int _offsetX;
    private static int _offsetY;
    private static int _composeResult;
    private static int _composeCallCount;

    [TestInitialize]
    public void Initialize()
    {
        _source = 0;
        _destination = 0;
        _sourceRectDescriptors = 0;
        _rectangleCount = 0;
        _destinationRectDescriptors = 0;
        _operation = 0;
        _offsetX = -1;
        _offsetY = -1;
        _composeResult = 0;
        _composeCallCount = 0;
    }

    [TestMethod]
    public void WhenComposingRectanglesThenNativeSlot120ReceivesArgumentsAndZeroOffsets()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using FakeSurfaceObject destinationObject = new();
        using FakeVertexBufferObject sourceDescriptorsObject = new();
        using FakeVertexBufferObject destinationDescriptorsObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        using Direct3D9Surface destination = CreateSurface(destinationObject.Surface);
        using Direct3D9VertexBuffer sourceDescriptors = new(sourceDescriptorsObject.VertexBuffer);
        using Direct3D9VertexBuffer destinationDescriptors = new(destinationDescriptorsObject.VertexBuffer);

        int result = device.ComposeRects(
            source,
            destination,
            sourceDescriptors,
            17,
            destinationDescriptors,
            Composerectsop.Or);

        Assert.AreEqual(
            (0, (nint) sourceObject.Surface, (nint) destinationObject.Surface,
                (nint) sourceDescriptorsObject.VertexBuffer, 17u,
                (nint) destinationDescriptorsObject.VertexBuffer, Composerectsop.Or, 0, 0, 1),
            (result, _source, _destination, _sourceRectDescriptors, _rectangleCount,
                _destinationRectDescriptors, _operation, _offsetX, _offsetY, _composeCallCount));
    }

    [DataTestMethod]
    [DataRow(1)]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenComposeRectsDoesNotReturnDriverInternalErrorThenHResultIsPreservedWithoutMarkingDeviceUnusable(
        int composeResult)
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using FakeSurfaceObject destinationObject = new();
        using FakeVertexBufferObject sourceDescriptorsObject = new();
        using FakeVertexBufferObject destinationDescriptorsObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        using Direct3D9Surface destination = CreateSurface(destinationObject.Surface);
        using Direct3D9VertexBuffer sourceDescriptors = new(sourceDescriptorsObject.VertexBuffer);
        using Direct3D9VertexBuffer destinationDescriptors = new(destinationDescriptorsObject.VertexBuffer);
        _composeResult = composeResult;

        int result = device.ComposeRects(
            source,
            destination,
            sourceDescriptors,
            uint.MaxValue,
            destinationDescriptors,
            Composerectsop.Neg);

        Assert.AreEqual((composeResult, 0, 1), (result, device.UnusableReasonHResult, _composeCallCount));
    }

    [TestMethod]
    public void WhenComposeRectsReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using FakeSurfaceObject destinationObject = new();
        using FakeVertexBufferObject sourceDescriptorsObject = new();
        using FakeVertexBufferObject destinationDescriptorsObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        using Direct3D9Surface destination = CreateSurface(destinationObject.Surface);
        using Direct3D9VertexBuffer sourceDescriptors = new(sourceDescriptorsObject.VertexBuffer);
        using Direct3D9VertexBuffer destinationDescriptors = new(destinationDescriptorsObject.VertexBuffer);
        _composeResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.ComposeRects(
            source,
            destination,
            sourceDescriptors,
            1,
            destinationDescriptors,
            Composerectsop.Copy);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenComposingAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using FakeSurfaceObject destinationObject = new();
        using FakeVertexBufferObject sourceDescriptorsObject = new();
        using FakeVertexBufferObject destinationDescriptorsObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        using Direct3D9Surface destination = CreateSurface(destinationObject.Surface);
        using Direct3D9VertexBuffer sourceDescriptors = new(sourceDescriptorsObject.VertexBuffer);
        using Direct3D9VertexBuffer destinationDescriptors = new(destinationDescriptorsObject.VertexBuffer);
        device.MarkUnusable();

        int result = device.ComposeRects(
            source,
            destination,
            sourceDescriptors,
            1,
            destinationDescriptors,
            Composerectsop.Copy);

        Assert.AreEqual((0, 1), (result, _composeCallCount));
    }

    [TestMethod]
    public void WhenComposingWithoutExtendedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using FakeSurfaceObject destinationObject = new();
        using FakeVertexBufferObject sourceDescriptorsObject = new();
        using FakeVertexBufferObject destinationDescriptorsObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, null);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        using Direct3D9Surface destination = CreateSurface(destinationObject.Surface);
        using Direct3D9VertexBuffer sourceDescriptors = new(sourceDescriptorsObject.VertexBuffer);
        using Direct3D9VertexBuffer destinationDescriptors = new(destinationDescriptorsObject.VertexBuffer);

        Assert.ThrowsExactly<InvalidOperationException>(() => device.ComposeRects(
            source,
            destination,
            sourceDescriptors,
            1,
            destinationDescriptors,
            Composerectsop.Copy));
    }

    [TestMethod]
    public void WhenComposingWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using FakeSurfaceObject destinationObject = new();
        using FakeVertexBufferObject sourceDescriptorsObject = new();
        using FakeVertexBufferObject destinationDescriptorsObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        using Direct3D9Surface destination = CreateSurface(destinationObject.Surface);
        using Direct3D9VertexBuffer sourceDescriptors = new(sourceDescriptorsObject.VertexBuffer);
        using Direct3D9VertexBuffer destinationDescriptors = new(destinationDescriptorsObject.VertexBuffer);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.ComposeRects(
            source,
            destination,
            sourceDescriptors,
            1,
            destinationDescriptors,
            Composerectsop.Copy));
        Assert.AreEqual(0, _composeCallCount);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device, IDirect3DDevice9Ex* deviceEx)
    {
        return new Direct3D9Device(device, deviceEx, 0, Devtype.Hal, 0, default);
    }

    private static Direct3D9Surface CreateSurface(IDirect3DSurface9* surface)
    {
        return new Direct3D9Surface(new Direct3D9ResourceManager(), surface);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDeviceEx(IDirect3DDevice9Ex* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseVertexBuffer(IDirect3DVertexBuffer9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int ComposeRects(
        IDirect3DDevice9Ex* self,
        IDirect3DSurface9* source,
        IDirect3DSurface9* destination,
        IDirect3DVertexBuffer9* sourceRectDescriptors,
        uint rectangleCount,
        IDirect3DVertexBuffer9* destinationRectDescriptors,
        Composerectsop operation,
        int offsetX,
        int offsetY)
    {
        _composeCallCount++;
        _source = (nint) source;
        _destination = (nint) destination;
        _sourceRectDescriptors = (nint) sourceRectDescriptors;
        _rectangleCount = rectangleCount;
        _destinationRectDescriptors = (nint) destinationRectDescriptors;
        _operation = operation;
        _offsetX = offsetX;
        _offsetY = offsetY;
        return _composeResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _deviceMemory;
        private nint _deviceExMemory;
        internal IDirect3DDevice9* Device;
        internal IDirect3DDevice9Ex* DeviceEx;

        public FakeDeviceObject()
        {
            _deviceMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** deviceMemory = (void**) _deviceMemory;
            Device = (IDirect3DDevice9*) deviceMemory;
            Device->LpVtbl = deviceMemory + 1;
            Device->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;

            _deviceExMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 122);
            void** deviceExMemory = (void**) _deviceExMemory;
            DeviceEx = (IDirect3DDevice9Ex*) deviceExMemory;
            DeviceEx->LpVtbl = deviceExMemory + 1;
            DeviceEx->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, uint>) &ReleaseDeviceEx;
            DeviceEx->LpVtbl[120] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, IDirect3DSurface9*, IDirect3DSurface9*, IDirect3DVertexBuffer9*, uint, IDirect3DVertexBuffer9*, Composerectsop, int, int, int>) &ComposeRects;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _deviceExMemory);
            NativeMemory.Free((void*) _deviceMemory);
            _deviceExMemory = 0;
            _deviceMemory = 0;
            DeviceEx = null;
            Device = null;
        }
    }

    private struct FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        public FakeSurfaceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            Surface->LpVtbl = memory + 1;
            Surface->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }

    private struct FakeVertexBufferObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DVertexBuffer9* VertexBuffer;

        public FakeVertexBufferObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            VertexBuffer = (IDirect3DVertexBuffer9*) memory;
            VertexBuffer->LpVtbl = memory + 1;
            VertexBuffer->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DVertexBuffer9*, uint>) &ReleaseVertexBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            VertexBuffer = null;
        }
    }
}
