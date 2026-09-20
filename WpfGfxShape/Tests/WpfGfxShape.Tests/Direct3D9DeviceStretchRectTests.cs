using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceStretchRectTests
{
    private static nint _source;
    private static nint _destination;
    private static Direct3D9SurfaceRect _sourceRectangle;
    private static Direct3D9SurfaceRect _destinationRectangle;
    private static bool _sourceRectangleWasNull;
    private static bool _destinationRectangleWasNull;
    private static Texturefiltertype _filter;
    private static int _stretchRectResult;
    private static int _stretchRectCallCount;

    [TestInitialize]
    public void Initialize()
    {
        _source = 0;
        _destination = 0;
        _sourceRectangle = default;
        _destinationRectangle = default;
        _sourceRectangleWasNull = false;
        _destinationRectangleWasNull = false;
        _filter = 0;
        _stretchRectResult = 0;
        _stretchRectCallCount = 0;
    }

    [TestMethod]
    public void WhenStretchingSurfaceThenNativeSlot34ReceivesArguments()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        IDirect3DSurface9* destination = (IDirect3DSurface9*) 2;
        Direct3D9SurfaceRect sourceRectangle = new(3, 4, 13, 14);
        Direct3D9SurfaceRect destinationRectangle = new(5, 6, 25, 26);

        int result = device.StretchRect(
            source,
            sourceRectangle,
            destination,
            destinationRectangle,
            Texturefiltertype.Linear);

        Assert.AreEqual(
            (0, (nint) sourceObject.Surface, (nint) destination, sourceRectangle, destinationRectangle, Texturefiltertype.Linear, false, false, 1),
            (result, _source, _destination, _sourceRectangle, _destinationRectangle, _filter, _sourceRectangleWasNull, _destinationRectangleWasNull, _stretchRectCallCount));
    }

    [TestMethod]
    public void WhenStretchingNativeSurfaceReferencesThenSlot34UsesPointFiltering()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9SurfaceRect sourceRectangle = new(3, 4, 13, 14);
        Direct3D9SurfaceRect destinationRectangle = new(5, 6, 25, 26);

        int result = device.StretchRect(1, sourceRectangle, 2, destinationRectangle);

        Assert.AreEqual(
            (0, (nint) 1, (nint) 2, sourceRectangle, destinationRectangle, Texturefiltertype.None, false, false, 1),
            (result, _source, _destination, _sourceRectangle, _destinationRectangle, _filter, _sourceRectangleWasNull, _destinationRectangleWasNull, _stretchRectCallCount));
    }

    [TestMethod]
    public void WhenStretchingWholeSurfaceThenOptionalRectanglesAreNull()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);

        int result = device.StretchRect(source, null, (IDirect3DSurface9*) 2, null, Texturefiltertype.None);

        Assert.AreEqual((0, true, true, 1), (result, _sourceRectangleWasNull, _destinationRectangleWasNull, _stretchRectCallCount));
    }

    [TestMethod]
    public void WhenStretchRectReturnsNonzeroSuccessThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        _stretchRectResult = 1;

        int result = device.StretchRect(source, null, (IDirect3DSurface9*) 2, null, Texturefiltertype.Point);

        Assert.AreEqual((1, 1), (result, _stretchRectCallCount));
    }

    [TestMethod]
    public void WhenStretchRectReturnsOrdinaryFailureThenDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        _stretchRectResult = Direct3D9Factory.InvalidCallHResult;

        int result = device.StretchRect(source, null, (IDirect3DSurface9*) 2, null, Texturefiltertype.Point);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.SuccessHResult, 1),
            (result, device.UnusableReasonHResult, _stretchRectCallCount));
    }

    [TestMethod]
    public void WhenStretchRectReturnsDeviceLostThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        _stretchRectResult = Direct3D9Factory.DeviceLostHResult;

        int result = device.StretchRect(source, null, (IDirect3DSurface9*) 2, null, Texturefiltertype.Point);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, Direct3D9Factory.SuccessHResult, 1),
            (result, device.UnusableReasonHResult, _stretchRectCallCount));
    }

    [TestMethod]
    public void WhenStretchRectReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        _stretchRectResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.StretchRect(source, null, (IDirect3DSurface9*) 2, null, Texturefiltertype.Point);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenStretchingAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        device.MarkUnusable();

        int result = device.StretchRect(source, null, (IDirect3DSurface9*) 2, null, Texturefiltertype.None);

        Assert.AreEqual((0, 1), (result, _stretchRectCallCount));
    }

    [TestMethod]
    public void WhenStretchingWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9Surface source = CreateSurface(sourceObject.Surface);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.StretchRect(source, null, (IDirect3DSurface9*) 2, null, Texturefiltertype.None));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    private static Direct3D9Surface CreateSurface(IDirect3DSurface9* surface)
    {
        return new Direct3D9Surface(new Direct3D9ResourceManager(), surface);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int StretchRect(
        IDirect3DDevice9* self,
        IDirect3DSurface9* source,
        Direct3D9SurfaceRect* sourceRectangle,
        IDirect3DSurface9* destination,
        Direct3D9SurfaceRect* destinationRectangle,
        Texturefiltertype filter)
    {
        _stretchRectCallCount++;
        _source = (nint) source;
        _destination = (nint) destination;
        _sourceRectangleWasNull = sourceRectangle is null;
        _destinationRectangleWasNull = destinationRectangle is null;
        _sourceRectangle = sourceRectangle is null ? default : *sourceRectangle;
        _destinationRectangle = destinationRectangle is null ? default : *destinationRectangle;
        _filter = filter;
        return _stretchRectResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 107);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[34] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int>) &StretchRect;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        public FakeSurfaceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 15);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            Surface->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
