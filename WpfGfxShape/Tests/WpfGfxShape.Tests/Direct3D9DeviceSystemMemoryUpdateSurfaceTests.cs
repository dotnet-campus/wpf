using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceSystemMemoryUpdateSurfaceTests
{
    private static uint _width;
    private static uint _height;
    private static uint _levels;
    private static uint _usage;
    private static Format _format;
    private static Pool _pool;
    private static nint _pixelReference;
    private static uint _surfaceLevel;
    private static nint _surfaceToReturn;
    private static nint _textureToReturn;
    private static int _createSurfaceResult;
    private static int _createTextureResult;
    private static int _getSurfaceLevelResult;
    private static int _createSurfaceCallCount;
    private static int _createTextureCallCount;
    private static int _getSurfaceLevelCallCount;
    private static int _surfaceReleaseCount;
    private static int _textureReleaseCount;
    private static Direct3D9SurfaceRect _lockRectangle;
    private static uint _lockFlags;
    private static int _lockResult;
    private static int _unlockResult;
    private static int _lockCallCount;
    private static int _unlockCallCount;

    [TestInitialize]
    public void Initialize()
    {
        _width = 0;
        _height = 0;
        _levels = 0;
        _usage = 0;
        _format = 0;
        _pool = 0;
        _pixelReference = 0;
        _surfaceLevel = uint.MaxValue;
        _surfaceToReturn = 0;
        _textureToReturn = 0;
        _createSurfaceResult = 0;
        _createTextureResult = 0;
        _getSurfaceLevelResult = 0;
        _createSurfaceCallCount = 0;
        _createTextureCallCount = 0;
        _getSurfaceLevelCallCount = 0;
        _surfaceReleaseCount = 0;
        _textureReleaseCount = 0;
        _lockRectangle = default;
        _lockFlags = 0;
        _lockResult = 0;
        _unlockResult = 0;
        _lockCallCount = 0;
        _unlockCallCount = 0;
    }

    [TestMethod]
    public void WhenCreatingWddmUpdateSurfaceThenOffscreenSurfaceReferencesPixels()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: true);
        byte* pixels = stackalloc byte[128];

        int result = device.TryCreateSystemMemoryUpdateSurface(
            16,
            8,
            Format.A8R8G8B8,
            pixels,
            out Direct3D9SystemMemoryUpdateSurface? surface);
        nint nativeSurface = (nint) surface!.Surface;
        surface.Dispose();

        Assert.AreEqual(
            (0, 16u, 8u, Format.A8R8G8B8, Pool.Systemmem, (nint) pixels, (nint) surfaceObject.Surface, 1, 0, 1),
            (result, _width, _height, _format, _pool, _pixelReference, nativeSurface, _createSurfaceCallCount, _createTextureCallCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenCreatingWddmAllocatedUpdateSurfaceThenSharedHandleIsNull()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: true);

        int result = device.TryCreateSystemMemoryUpdateSurface(
            4,
            2,
            Format.X8R8G8B8,
            null,
            out Direct3D9SystemMemoryUpdateSurface? surface);
        surface!.Dispose();

        Assert.AreEqual((0, 0, 1, 0), (result, _pixelReference, _createSurfaceCallCount, _createTextureCallCount));
    }

    [TestMethod]
    public void WhenWddmSurfaceCreationFailsThenReturnedPointerIsReleasedAndDieIsHandled()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createSurfaceResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: true);

        int result = device.TryCreateSystemMemoryUpdateSurface(1, 1, Format.A8R8G8B8, null, out Direct3D9SystemMemoryUpdateSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, null),
            (result, device.UnusableReasonHResult, _surfaceReleaseCount, surface));
    }

    [TestMethod]
    public void WhenWddmSurfaceCreationReturnsNonzeroSuccessThenResultAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createSurfaceResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: true);

        int result = device.TryCreateSystemMemoryUpdateSurface(3, 2, Format.A8R8G8B8, null, out Direct3D9SystemMemoryUpdateSurface? surface);
        surface!.Dispose();

        Assert.AreEqual((1, 1, 1), (result, _createSurfaceCallCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenWddmSurfaceCreationReturnsOrdinaryFailureThenReturnedPointerIsReleasedAndDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createSurfaceResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: true);

        int result = device.TryCreateSystemMemoryUpdateSurface(1, 1, Format.A8R8G8B8, null, out Direct3D9SystemMemoryUpdateSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, 1, 1, null),
            (result, device.UnusableReasonHResult, _createSurfaceCallCount, _surfaceReleaseCount, surface));
    }

    [TestMethod]
    public void WhenWddmSurfaceCreationReturnsDeviceLostThenResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _createSurfaceResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: true);

        int result = device.TryCreateSystemMemoryUpdateSurface(1, 1, Format.A8R8G8B8, null, out Direct3D9SystemMemoryUpdateSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 0, 1, 1, null),
            (result, device.UnusableReasonHResult, _createSurfaceCallCount, _surfaceReleaseCount, surface));
    }

    [TestMethod]
    public void WhenCreatingWddmUpdateSurfaceAfterDeviceReleaseThenNativeCreationIsNotCalled()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: true);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.TryCreateSystemMemoryUpdateSurface(1, 1, Format.A8R8G8B8, null, out _));
        Assert.AreEqual(0, _createSurfaceCallCount);
    }

    [TestMethod]
    public void WhenCreatingXpdmUpdateSurfaceThenTextureLevelZeroSuppliesOwnedSurface()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeTextureObject textureObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _textureToReturn = (nint) textureObject.Texture;
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: false);

        int result = device.TryCreateSystemMemoryUpdateSurface(
            32,
            12,
            Format.A8R8G8B8,
            (void*) 1,
            out Direct3D9SystemMemoryUpdateSurface? surface);
        nint nativeSurface = (nint) surface!.Surface;
        surface.Dispose();

        Assert.AreEqual(
            (0, 32u, 12u, 1u, 0u, Format.A8R8G8B8, Pool.Systemmem, 0u, (nint) surfaceObject.Surface, 0, 1, 1, 1, 1),
            (result, _width, _height, _levels, _usage, _format, _pool, _surfaceLevel, nativeSurface, _createSurfaceCallCount, _createTextureCallCount, _getSurfaceLevelCallCount, _textureReleaseCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenXpdmGetSurfaceLevelFailsThenTextureAndReturnedSurfaceAreReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeTextureObject textureObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _textureToReturn = (nint) textureObject.Texture;
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _getSurfaceLevelResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: false);

        int result = device.TryCreateSystemMemoryUpdateSurface(1, 1, Format.A8R8G8B8, null, out Direct3D9SystemMemoryUpdateSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 1, null),
            (result, _getSurfaceLevelCallCount, _textureReleaseCount, _surfaceReleaseCount, surface));
    }

    [TestMethod]
    public void WhenXpdmDeviceLossWasProcessedThenTextureCreationIsBlocked()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: false);
        device.MarkUnusable();

        int result = device.TryCreateSystemMemoryUpdateSurface(1, 1, Format.A8R8G8B8, null, out Direct3D9SystemMemoryUpdateSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, 0, 0, null),
            (result, _createSurfaceCallCount, _createTextureCallCount, surface));
    }

    [TestMethod]
    public void WhenWddmDeviceLossWasProcessedThenOffscreenSurfaceCreationStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, isWddm: true);
        device.MarkUnusable();

        int result = device.TryCreateSystemMemoryUpdateSurface(1, 1, Format.A8R8G8B8, null, out Direct3D9SystemMemoryUpdateSurface? surface);
        surface!.Dispose();

        Assert.AreEqual((0, 1, 0, 1), (result, _createSurfaceCallCount, _createTextureCallCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenLockingAndUnlockingUpdateSurfaceThenNativeSlotsReceiveArgumentsAndResults()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9SystemMemoryUpdateSurface surface = new(surfaceObject.Surface);
        Direct3D9SurfaceRect rectangle = new(1, 2, 11, 12);
        _unlockResult = Direct3D9Factory.InvalidCallHResult;

        int lockResult = surface.LockRect(out LockedRect lockedRect, rectangle, 0x10);
        int unlockResult = surface.UnlockRect();

        Assert.AreEqual(
            (0, Direct3D9Factory.InvalidCallHResult, rectangle, 0x10u, 64, (nint) 42, 1, 1),
            (lockResult, unlockResult, _lockRectangle, _lockFlags, lockedRect.Pitch, (nint) lockedRect.PBits, _lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenDisposingUpdateSurfaceRepeatedlyThenNativeSurfaceIsReleasedOnce()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9SystemMemoryUpdateSurface surface = new(surfaceObject.Surface);

        surface.Dispose();
        surface.Dispose();

        Assert.AreEqual(1, _surfaceReleaseCount);
    }

    [TestMethod]
    public void WhenUsingReleasedUpdateSurfaceThenNativeOperationsAreRejected()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9SystemMemoryUpdateSurface surface = new(surfaceObject.Surface);
        surface.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = surface.Surface);
        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.LockRect(out _, default, 0));
        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.UnlockRect());
        Assert.AreEqual((0, 0), (_lockCallCount, _unlockCallCount));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device, bool isWddm)
    {
        Caps9 capabilities = default;
        capabilities.Caps2 = isWddm ? D3D9.Caps2Canshareresource : 0;
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default, capabilities: capabilities);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateOffscreenPlainSurface(
        IDirect3DDevice9* self,
        uint width,
        uint height,
        Format format,
        Pool pool,
        IDirect3DSurface9** surface,
        void** sharedHandle)
    {
        _createSurfaceCallCount++;
        _width = width;
        _height = height;
        _format = format;
        _pool = pool;
        _pixelReference = sharedHandle is null ? 0 : (nint) (*sharedHandle);
        *surface = (IDirect3DSurface9*) _surfaceToReturn;
        return _createSurfaceResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateTexture(
        IDirect3DDevice9* self,
        uint width,
        uint height,
        uint levels,
        uint usage,
        Format format,
        Pool pool,
        IDirect3DTexture9** texture,
        void** sharedHandle)
    {
        _createTextureCallCount++;
        _width = width;
        _height = height;
        _levels = levels;
        _usage = usage;
        _format = format;
        _pool = pool;
        *texture = (IDirect3DTexture9*) _textureToReturn;
        return _createTextureResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceLevel(IDirect3DTexture9* self, uint level, IDirect3DSurface9** surface)
    {
        _getSurfaceLevelCallCount++;
        _surfaceLevel = level;
        *surface = (IDirect3DSurface9*) _surfaceToReturn;
        return _getSurfaceLevelResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseTexture(IDirect3DTexture9* self)
    {
        _textureReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self)
    {
        _surfaceReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockSurface(
        IDirect3DSurface9* self,
        LockedRect* lockedRect,
        Direct3D9SurfaceRect* rectangle,
        uint flags)
    {
        _lockCallCount++;
        _lockRectangle = *rectangle;
        _lockFlags = flags;
        lockedRect->Pitch = 64;
        lockedRect->PBits = (void*) 42;
        return _lockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockSurface(IDirect3DSurface9* self)
    {
        _unlockCallCount++;
        return _unlockResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 39);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[23] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int>) &CreateTexture;
            vtable[36] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DSurface9**, void**, int>) &CreateOffscreenPlainSurface;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeTextureObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DTexture9* Texture;

        public FakeTextureObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 21);
            void** memory = (void**) _memory;
            Texture = (IDirect3DTexture9*) memory;
            void** vtable = memory + 1;
            Texture->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &ReleaseTexture;
            vtable[18] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) &GetSurfaceLevel;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Texture = null;
        }
    }

    private struct FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        public FakeSurfaceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 16);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            Surface->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
            vtable[13] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, LockedRect*, Direct3D9SurfaceRect*, uint, int>) &LockSurface;
            vtable[14] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, int>) &UnlockSurface;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
