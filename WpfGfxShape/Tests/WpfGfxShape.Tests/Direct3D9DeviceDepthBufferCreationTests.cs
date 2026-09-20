using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceDepthBufferCreationTests
{
    private static uint _width;
    private static uint _height;
    private static Format _format;
    private static MultisampleType _multisampleType;
    private static uint _multisampleQuality;
    private static int _discard;
    private static bool _sharedHandleWasNull;
    private static nint _surfaceToReturn;
    private static int _createResult;
    private static int _createCallCount;
    private static int _surfaceGetDescriptionCallCount;
    private static int _surfaceGetDescriptionResult;
    private static int _surfaceReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _width = 0;
        _height = 0;
        _format = 0;
        _multisampleType = 0;
        _multisampleQuality = 0;
        _discard = 0;
        _sharedHandleWasNull = false;
        _surfaceToReturn = 0;
        _createResult = 0;
        _createCallCount = 0;
        _surfaceGetDescriptionCallCount = 0;
        _surfaceGetDescriptionResult = 0;
        _surfaceReleaseCount = 0;
    }

    [TestMethod]
    public void WhenCreatingDepthBufferThenSlot29ArgumentsAndResourceOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.CreateDepthBuffer(
            320,
            240,
            MultisampleType.Multisample4Samples,
            out Direct3D9Surface? surface);
        nint nativeSurface = (nint) surface!.Surface;
        SurfaceDesc firstDescription = surface.GetDescription();
        _width = 1;
        SurfaceDesc secondDescription = surface.GetDescription();
        int resourceCount = device.ResourceCount;
        uint videoMemoryConsumption = device.ResourceManager.TotalVideoMemoryConsumption;
        surface.Dispose();

        Assert.AreEqual(
            (0, 320u, 240u, Format.D24S8, MultisampleType.Multisample4Samples, 0u, 0, true,
                (nint) surfaceObject.Surface, 1, 1, 1, 1_228_800u, 1, firstDescription, firstDescription),
            (result, firstDescription.Width, _height, _format, _multisampleType, _multisampleQuality, _discard,
                _sharedHandleWasNull, nativeSurface, _createCallCount, _surfaceGetDescriptionCallCount, resourceCount,
                videoMemoryConsumption, _surfaceReleaseCount, firstDescription, secondDescription));
    }

    [TestMethod]
    public void WhenDepthBufferCreationFailsWithPointerThenPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.CreateDepthBuffer(
            1,
            1,
            MultisampleType.MultisampleNone,
            out Direct3D9Surface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 0, null),
            (result, _createCallCount, _surfaceReleaseCount, device.ResourceCount, surface));
    }

    [TestMethod]
    public void WhenDepthBufferCreationReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.CreateDepthBuffer(
            1,
            1,
            MultisampleType.MultisampleNone,
            out Direct3D9Surface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, null),
            (result, device.UnusableReasonHResult, _surfaceReleaseCount, surface));
    }

    [TestMethod]
    public void WhenDepthBufferCreationReturnsNonzeroSuccessThenWrapperSuccessAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.CreateDepthBuffer(
            uint.MaxValue,
            240,
            MultisampleType.Multisample2Samples,
            out Direct3D9Surface? surface);
        surface!.Dispose();

        Assert.AreEqual((0, uint.MaxValue, 240u, 1, 1),
            (result, _width, _height, _createCallCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenDepthBufferCreationReturnsOrdinaryFailureThenDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.CreateDepthBuffer(
            1,
            1,
            MultisampleType.MultisampleNone,
            out Direct3D9Surface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, surface));
    }

    [TestMethod]
    public void WhenDepthBufferCreationReturnsDeviceLostThenResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.CreateDepthBuffer(
            1,
            1,
            MultisampleType.MultisampleNone,
            out Direct3D9Surface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, surface));
    }

    [TestMethod]
    public void WhenCreatingDepthBufferAfterDeviceLossWasProcessedThenNativeCreationStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.CreateDepthBuffer(
            1,
            1,
            MultisampleType.MultisampleNone,
            out Direct3D9Surface? surface);
        surface!.Dispose();

        Assert.AreEqual((0, 1, 1), (result, _createCallCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenDepthBufferDescriptionInitializationFailsThenSurfaceIsReleasedAndResourceIsUnregistered()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _surfaceGetDescriptionResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.CreateDepthBuffer(
            1,
            1,
            MultisampleType.MultisampleNone,
            out Direct3D9Surface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 1, 0, 0u, null),
            (result, _createCallCount, _surfaceGetDescriptionCallCount, _surfaceReleaseCount, device.ResourceCount,
                device.ResourceManager.TotalVideoMemoryConsumption, surface));
    }

    [TestMethod]
    public void WhenBoundDepthBufferIsReleasedThenDeviceBindingIsClearedBeforeSurfaceRelease()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(deviceObject.Device, surface =>
        {
            calls.Add(surface is null ? "clear" : "bind");
            return 0;
        });
        _ = device.CreateDepthBuffer(16, 24, MultisampleType.MultisampleNone, out Direct3D9Surface? surface);
        _ = device.SetDepthStencilSurfaceInline(surface!.Surface, 16, 24);

        surface.Dispose();
        calls.Add($"release:{_surfaceReleaseCount}");

        CollectionAssert.AreEqual(new[] { "bind", "clear", "release:1" }, calls);
    }

    [TestMethod]
    public void WhenDepthBufferUnbindingFailsThenSurfaceIsStillReleasedAndStateBecomesUnknown()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, _ =>
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        _ = device.CreateDepthBuffer(16, 24, MultisampleType.MultisampleNone, out Direct3D9Surface? surface);
        _ = device.SetDepthStencilSurfaceInline(surface!.Surface, 16, 24);

        surface.Dispose();
        bool stateIsUnknown = device.IsDepthStencilSurfaceSmallerThan(1, 1);

        Assert.AreEqual((2, 1, true), (callCount, _surfaceReleaseCount, stateIsUnknown));
    }

    [TestMethod]
    public void WhenDepthBufferSwitchFailsThenReleasingSurfacesDoesNotClearUnknownBinding()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject oldSurfaceObject = new();
        using FakeSurfaceObject replacementSurfaceObject = new();
        List<nint> calls = [];
        using Direct3D9Device device = CreateDevice(deviceObject.Device, surface =>
        {
            calls.Add((nint) surface);
            return surface == replacementSurfaceObject.Surface
                ? Direct3D9Factory.GenericFailureHResult
                : 0;
        });
        _surfaceToReturn = (nint) oldSurfaceObject.Surface;
        _ = device.CreateDepthBuffer(16, 24, MultisampleType.MultisampleNone, out Direct3D9Surface? oldSurface);
        _ = device.SetDepthStencilSurfaceInline(oldSurface!.Surface, 16, 24);
        _surfaceToReturn = (nint) replacementSurfaceObject.Surface;
        _ = device.CreateDepthBuffer(16, 24, MultisampleType.MultisampleNone, out Direct3D9Surface? replacementSurface);

        int switchResult = device.SetDepthStencilSurfaceInline(replacementSurface!.Surface, 16, 24);
        oldSurface.Dispose();
        replacementSurface.Dispose();
        bool stateIsUnknown = device.IsDepthStencilSurfaceSmallerThan(1, 1);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, (nint) oldSurfaceObject.Surface,
                (nint) replacementSurfaceObject.Surface, 2, true),
            (switchResult, calls.Count, calls[0], calls[1], _surfaceReleaseCount, stateIsUnknown));
    }

    [TestMethod]
    public void WhenCreatingDepthBufferWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.CreateDepthBuffer(
            1,
            1,
            MultisampleType.MultisampleNone,
            out _));
        Assert.AreEqual(0, _createCallCount);
    }

    private static Direct3D9Device CreateDevice(
        IDirect3DDevice9* device,
        Direct3D9SetDepthStencilSurface? setDepthStencilSurface = null)
    {
        return new Direct3D9Device(
            device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setDepthStencilSurface: setDepthStencilSurface);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self)
    {
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateDepthStencilSurface(
        IDirect3DDevice9* self,
        uint width,
        uint height,
        Format format,
        MultisampleType multisampleType,
        uint multisampleQuality,
        int discard,
        IDirect3DSurface9** surface,
        void** sharedHandle)
    {
        _createCallCount++;
        _width = width;
        _height = height;
        _format = format;
        _multisampleType = multisampleType;
        _multisampleQuality = multisampleQuality;
        _discard = discard;
        _sharedHandleWasNull = sharedHandle is null;
        *surface = (IDirect3DSurface9*) _surfaceToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        _surfaceGetDescriptionCallCount++;
        *description = new SurfaceDesc(
            format: Format.D24S8,
            type: Resourcetype.Surface,
            usage: D3D9.UsageDepthstencil,
            pool: Pool.Default,
            multiSampleType: _multisampleType,
            multiSampleQuality: _multisampleQuality,
            width: _width,
            height: _height);
        return _surfaceGetDescriptionResult;
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
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 32);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[29] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int>) &CreateDepthStencilSurface;
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
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 14);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            Surface->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetSurfaceDescription;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
