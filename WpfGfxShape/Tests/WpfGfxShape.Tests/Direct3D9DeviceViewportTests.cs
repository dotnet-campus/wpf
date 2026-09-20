using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceViewportTests
{
    private static Viewport9 _viewport;
    private static int _result;
    private static int _callCount;

    [TestInitialize]
    public void Initialize()
    {
        _viewport = default;
        _result = 0;
        _callCount = 0;
    }

    [TestMethod]
    public void WhenSettingViewportThenNativeSlot47ReceivesConvertedBoundsAndDepthRange()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9PointAndSizeRect viewport = new(-2, 3, 640, 480);

        int result = device.SetViewport(viewport);

        Assert.AreEqual(
            (0, unchecked((uint) -2), 3u, 640u, 480u, 0f, 1f, 1),
            (result, _viewport.X, _viewport.Y, _viewport.Width, _viewport.Height, _viewport.MinZ, _viewport.MaxZ, _callCount));
    }

    [TestMethod]
    public void WhenSettingViewportFailsThenOriginalHResultAndRequestedViewportArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9PointAndSizeRect viewport = new(5, 6, 7, 8);
        _result = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.SetViewport(viewport);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, viewport, 0),
            (result, device.ViewportCache, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingViewportWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetViewport(new(1, 2, 3, 4)));
        Assert.AreEqual(0, _callCount);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetViewport(IDirect3DDevice9* self, Viewport9* viewport)
    {
        _callCount++;
        _viewport = *viewport;
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 50);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[47] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Viewport9*, int>) &SetViewport;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
