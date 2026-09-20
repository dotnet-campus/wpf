using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceRenderTargetCreationTests
{
    private static uint _width;
    private static uint _height;
    private static Format _format;
    private static MultisampleType _multisampleType;
    private static uint _multisampleQuality;
    private static int _lockable;
    private static nint _surfaceToReturn;
    private static int _createResult;
    private static int _createCallCount;
    private static int _surfaceReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _width = 0;
        _height = 0;
        _format = 0;
        _multisampleType = 0;
        _multisampleQuality = 0;
        _lockable = 0;
        _surfaceToReturn = 0;
        _createResult = 0;
        _createCallCount = 0;
        _surfaceReleaseCount = 0;
    }

    [TestMethod]
    public void WhenCreatingUntrackedRenderTargetThenSlot28ArgumentsAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateRenderTargetUntracked(
            320,
            240,
            Format.A2R10G10B10,
            MultisampleType.Multisample4Samples,
            7,
            true,
            out Direct3D9UntrackedSurface? surface);
        nint nativeSurface = (nint) surface!.Surface;
        int resourceCount = device.ResourceCount;
        surface.Dispose();

        Assert.AreEqual(
            (0, 320u, 240u, Format.A2R10G10B10, MultisampleType.Multisample4Samples, 7u, 1,
                (nint) surfaceObject.Surface, 1, 0, 1),
            (result, _width, _height, _format, _multisampleType, _multisampleQuality, _lockable,
                nativeSurface, _createCallCount, resourceCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenUntrackedRenderTargetCreationFailsWithPointerThenPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateRenderTargetUntracked(
            1,
            1,
            Format.X8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out Direct3D9UntrackedSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, null),
            (result, _createCallCount, _surfaceReleaseCount, surface));
    }

    [TestMethod]
    public void WhenUntrackedRenderTargetCreationReturnsNonzeroSuccessThenResultAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateRenderTargetUntracked(
            1,
            1,
            Format.X8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out Direct3D9UntrackedSurface? surface);
        surface!.Dispose();

        Assert.AreEqual((1, 1, 1), (result, _createCallCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenUntrackedRenderTargetCreationReturnsOrdinaryFailureThenDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateRenderTargetUntracked(
            1,
            1,
            Format.X8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out Direct3D9UntrackedSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, surface));
    }

    [TestMethod]
    public void WhenUntrackedRenderTargetCreationReturnsDeviceLostThenResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateRenderTargetUntracked(
            1,
            1,
            Format.X8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out Direct3D9UntrackedSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, surface));
    }

    [TestMethod]
    public void WhenRenderTargetCreationReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateRenderTarget(
            1,
            1,
            Format.X8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out Direct3D9Surface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, null),
            (result, device.UnusableReasonHResult, _surfaceReleaseCount, surface));
    }

    [TestMethod]
    public void WhenCreatingUntrackedRenderTargetAfterDeviceLossWasProcessedThenNativeCreationStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.TryCreateRenderTargetUntracked(
            1,
            1,
            Format.X8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out Direct3D9UntrackedSurface? surface);
        surface!.Dispose();

        Assert.AreEqual((0, 1, 1), (result, _createCallCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenDisposingUntrackedRenderTargetRepeatedlyThenNativeSurfaceIsReleasedOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _ = device.TryCreateRenderTargetUntracked(
            1,
            1,
            Format.X8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out Direct3D9UntrackedSurface? surface);

        surface!.Dispose();
        surface.Dispose();

        Assert.AreEqual(1, _surfaceReleaseCount);
    }

    [TestMethod]
    public void WhenUsingReleasedUntrackedRenderTargetThenPointerAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _ = device.TryCreateRenderTargetUntracked(
            1,
            1,
            Format.X8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out Direct3D9UntrackedSurface? surface);
        surface!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = surface.Surface);
    }

    [TestMethod]
    public void WhenCreatingUntrackedRenderTargetWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.TryCreateRenderTargetUntracked(
            1,
            1,
            Format.X8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out _));
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
    private static int CreateRenderTarget(
        IDirect3DDevice9* self,
        uint width,
        uint height,
        Format format,
        MultisampleType multisampleType,
        uint multisampleQuality,
        int lockable,
        IDirect3DSurface9** surface,
        void** sharedHandle)
    {
        _createCallCount++;
        _width = width;
        _height = height;
        _format = format;
        _multisampleType = multisampleType;
        _multisampleQuality = multisampleQuality;
        _lockable = lockable;
        *surface = (IDirect3DSurface9*) _surfaceToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self)
    {
        _surfaceReleaseCount++;
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 31);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[28] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int>) &CreateRenderTarget;
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
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
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
