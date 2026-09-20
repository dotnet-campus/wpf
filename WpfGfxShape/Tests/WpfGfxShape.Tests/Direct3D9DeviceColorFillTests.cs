using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceColorFillTests
{
    private static nint _surface;
    private static Direct3D9SurfaceRect _rectangle;
    private static bool _rectangleWasNull;
    private static uint _color;
    private static int _result;
    private static int _callCount;

    [TestInitialize]
    public void Initialize()
    {
        _surface = 0;
        _rectangle = default;
        _rectangleWasNull = false;
        _color = 0;
        _result = 0;
        _callCount = 0;
    }

    [TestMethod]
    public void WhenFillingRectangleThenNativeSlot35ReceivesArguments()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DSurface9* surface = (IDirect3DSurface9*) 1;
        Direct3D9SurfaceRect rectangle = new(3, 4, 13, 14);

        int result = device.ColorFill(surface, rectangle, 0xA1B2C3D4);

        Assert.AreEqual(
            (0, (nint) surface, rectangle, false, 0xA1B2C3D4u, 1),
            (result, _surface, _rectangle, _rectangleWasNull, _color, _callCount));
    }

    [TestMethod]
    public void WhenFillingWholeSurfaceThenRectangleIsNull()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.ColorFill((IDirect3DSurface9*) 1, null, 0xFF010203);

        Assert.AreEqual((0, true, 1), (result, _rectangleWasNull, _callCount));
    }

    [TestMethod]
    public void WhenColorFillReturnsNonzeroSuccessThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = 1;

        int result = device.ColorFill((IDirect3DSurface9*) 1, null, 0);

        Assert.AreEqual((1, 1), (result, _callCount));
    }

    [TestMethod]
    public void WhenColorFillFailsThenHResultIsReturnedWithoutDeviceErrorMapping()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.ColorFill((IDirect3DSurface9*) 1, null, 0);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, 0),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenColorFillReturnsDeviceLostThenOriginalHResultIsPreservedWithoutMarkingDeviceUnusable()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = Direct3D9Factory.DeviceLostHResult;

        int result = device.ColorFill((IDirect3DSurface9*) 1, null, 0);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 0, 1),
            (result, device.UnusableReasonHResult, _callCount));
    }

    [TestMethod]
    public void WhenFillingAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.ColorFill((IDirect3DSurface9*) 1, null, 0);

        Assert.AreEqual((0, 1), (result, _callCount));
    }

    [TestMethod]
    public void WhenFillingWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.ColorFill((IDirect3DSurface9*) 1, null, 0));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int ColorFill(
        IDirect3DDevice9* self,
        IDirect3DSurface9* surface,
        Direct3D9SurfaceRect* rectangle,
        uint color)
    {
        _callCount++;
        _surface = (nint) surface;
        _rectangleWasNull = rectangle is null;
        _rectangle = rectangle is null ? default : *rectangle;
        _color = color;
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 38);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[35] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, uint, int>) &ColorFill;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
