using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceResourceUpdateTests
{
    private static nint _source;
    private static nint _destination;
    private static Direct3D9SurfaceRect _sourceRectangle;
    private static Direct3D9Point _destinationPoint;
    private static bool _sourceRectangleWasNull;
    private static bool _destinationPointWasNull;
    private static int _updateSurfaceResult;
    private static int _updateTextureResult;
    private static int _updateSurfaceCallCount;
    private static int _updateTextureCallCount;

    [TestInitialize]
    public void Initialize()
    {
        _source = 0;
        _destination = 0;
        _sourceRectangle = default;
        _destinationPoint = default;
        _sourceRectangleWasNull = false;
        _destinationPointWasNull = false;
        _updateSurfaceResult = 0;
        _updateTextureResult = 0;
        _updateSurfaceCallCount = 0;
        _updateTextureCallCount = 0;
    }

    [TestMethod]
    public void WhenUpdatingSurfaceThenNativeSlot30ReceivesArguments()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DSurface9* source = (IDirect3DSurface9*) 1;
        IDirect3DSurface9* destination = (IDirect3DSurface9*) 2;
        Direct3D9SurfaceRect sourceRectangle = new(3, 4, 13, 14);
        Direct3D9Point destinationPoint = new(5, 6);

        int result = device.UpdateSurface(source, sourceRectangle, destination, destinationPoint);

        Assert.AreEqual(
            (0, (nint) source, (nint) destination, sourceRectangle, destinationPoint, false, false, 1),
            (result, _source, _destination, _sourceRectangle, _destinationPoint, _sourceRectangleWasNull, _destinationPointWasNull, _updateSurfaceCallCount));
    }

    [TestMethod]
    public void WhenUpdatingWholeSurfaceThenOptionalPointersAreNull()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.UpdateSurface((IDirect3DSurface9*) 1, null, (IDirect3DSurface9*) 2, null);

        Assert.AreEqual((0, true, true, 1), (result, _sourceRectangleWasNull, _destinationPointWasNull, _updateSurfaceCallCount));
    }

    [TestMethod]
    public void WhenSurfaceUpdateReturnsNonzeroSuccessThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _updateSurfaceResult = 1;

        int result = device.UpdateSurface((IDirect3DSurface9*) 1, null, (IDirect3DSurface9*) 2, null);

        Assert.AreEqual((1, 1), (result, _updateSurfaceCallCount));
    }

    [TestMethod]
    public void WhenSurfaceUpdateReturnsOrdinaryFailureThenDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _updateSurfaceResult = Direct3D9Factory.InvalidCallHResult;

        int result = device.UpdateSurface((IDirect3DSurface9*) 1, null, (IDirect3DSurface9*) 2, null);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.SuccessHResult, 1),
            (result, device.UnusableReasonHResult, _updateSurfaceCallCount));
    }

    [TestMethod]
    public void WhenSurfaceUpdateReturnsDeviceLostThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _updateSurfaceResult = Direct3D9Factory.DeviceLostHResult;

        int result = device.UpdateSurface((IDirect3DSurface9*) 1, null, (IDirect3DSurface9*) 2, null);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, Direct3D9Factory.SuccessHResult, 1),
            (result, device.UnusableReasonHResult, _updateSurfaceCallCount));
    }

    [TestMethod]
    public void WhenUpdatingTextureThenNativeSlot31ReceivesArgumentsAndHResult()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DTexture9* source = (IDirect3DTexture9*) 3;
        IDirect3DTexture9* destination = (IDirect3DTexture9*) 4;
        _updateTextureResult = Direct3D9Factory.InvalidCallHResult;

        int result = device.UpdateTexture(source, destination);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, (nint) source, (nint) destination, 1),
            (result, _source, _destination, _updateTextureCallCount));
    }

    [TestMethod]
    public void WhenTextureUpdateReturnsNonzeroSuccessThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _updateTextureResult = 1;

        int result = device.UpdateTexture((IDirect3DTexture9*) 1, (IDirect3DTexture9*) 2);

        Assert.AreEqual((1, 1), (result, _updateTextureCallCount));
    }

    [TestMethod]
    public void WhenTextureUpdateReturnsOrdinaryFailureThenDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _updateTextureResult = Direct3D9Factory.InvalidCallHResult;

        int result = device.UpdateTexture((IDirect3DTexture9*) 1, (IDirect3DTexture9*) 2);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.SuccessHResult, 1),
            (result, device.UnusableReasonHResult, _updateTextureCallCount));
    }

    [TestMethod]
    public void WhenTextureUpdateReturnsDeviceLostThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _updateTextureResult = Direct3D9Factory.DeviceLostHResult;

        int result = device.UpdateTexture((IDirect3DTexture9*) 1, (IDirect3DTexture9*) 2);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, Direct3D9Factory.SuccessHResult, 1),
            (result, device.UnusableReasonHResult, _updateTextureCallCount));
    }

    [TestMethod]
    public void WhenSurfaceUpdateReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _updateSurfaceResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.UpdateSurface((IDirect3DSurface9*) 1, null, (IDirect3DSurface9*) 2, null);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenTextureUpdateReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _updateTextureResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.UpdateTexture((IDirect3DTexture9*) 1, (IDirect3DTexture9*) 2);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenUpdatingAfterDeviceLossWasProcessedThenNativeCallsStillRun()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int surfaceResult = device.UpdateSurface((IDirect3DSurface9*) 1, null, (IDirect3DSurface9*) 2, null);
        int textureResult = device.UpdateTexture((IDirect3DTexture9*) 3, (IDirect3DTexture9*) 4);

        Assert.AreEqual((0, 0, 1, 1), (surfaceResult, textureResult, _updateSurfaceCallCount, _updateTextureCallCount));
    }

    [TestMethod]
    public void WhenUpdatingSurfaceWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.UpdateSurface((IDirect3DSurface9*) 1, null, (IDirect3DSurface9*) 2, null));
    }

    [TestMethod]
    public void WhenUpdatingTextureWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.UpdateTexture((IDirect3DTexture9*) 1, (IDirect3DTexture9*) 2));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UpdateSurface(
        IDirect3DDevice9* self,
        IDirect3DSurface9* source,
        Direct3D9SurfaceRect* sourceRectangle,
        IDirect3DSurface9* destination,
        Direct3D9Point* destinationPoint)
    {
        _updateSurfaceCallCount++;
        _source = (nint) source;
        _destination = (nint) destination;
        _sourceRectangleWasNull = sourceRectangle is null;
        _destinationPointWasNull = destinationPoint is null;
        _sourceRectangle = sourceRectangle is null ? default : *sourceRectangle;
        _destinationPoint = destinationPoint is null ? default : *destinationPoint;
        return _updateSurfaceResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UpdateTexture(
        IDirect3DDevice9* self,
        IDirect3DTexture9* source,
        IDirect3DTexture9* destination)
    {
        _updateTextureCallCount++;
        _source = (nint) source;
        _destination = (nint) destination;
        return _updateTextureResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 34);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[30] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9Point*, int>) &UpdateSurface;
            vtable[31] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DTexture9*, IDirect3DTexture9*, int>) &UpdateTexture;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
