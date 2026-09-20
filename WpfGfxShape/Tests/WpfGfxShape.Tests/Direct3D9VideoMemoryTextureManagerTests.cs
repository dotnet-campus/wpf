using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9VideoMemoryTextureManagerTests
{
    private static nint _surfaceToReturn;
    private static nint _videoMemorySurface;
    private static nint _textureToReturn;
    private static int _getDescriptionResult;
    private static int _getSurfaceLevelResult;
    private static int _updateSurfaceResult;
    private static int _surfaceAddRefCount;
    private static int _surfaceReleaseCount;
    private static int _videoMemorySurfaceAddRefCount;
    private static int _videoMemorySurfaceReleaseCount;
    private static int _textureAddRefCount;
    private static int _textureReleaseCount;
    private static int _createTextureCallCount;
    private static int _generateMipSubLevelsCallCount;
    private static uint _textureLevelCount;
    private static int _lockCallCount;
    private static int _unlockCallCount;
    private static int _getSurfaceLevelCallCount;
    private static int _updateSurfaceCallCount;
    private static nint _updateSource;
    private static nint _updateDestination;
    private static Direct3D9SurfaceRect _lockRectangle;
    private static uint _lockFlags;

    [TestInitialize]
    public void Initialize()
    {
        _surfaceToReturn = 0;
        _videoMemorySurface = 0;
        _textureToReturn = 0;
        _getDescriptionResult = Direct3D9Factory.SuccessHResult;
        _getSurfaceLevelResult = Direct3D9Factory.SuccessHResult;
        _updateSurfaceResult = Direct3D9Factory.SuccessHResult;
        _surfaceAddRefCount = 0;
        _surfaceReleaseCount = 0;
        _videoMemorySurfaceAddRefCount = 0;
        _videoMemorySurfaceReleaseCount = 0;
        _textureAddRefCount = 0;
        _textureReleaseCount = 0;
        _createTextureCallCount = 0;
        _generateMipSubLevelsCallCount = 0;
        _textureLevelCount = 1;
        _lockCallCount = 0;
        _unlockCallCount = 0;
        _getSurfaceLevelCallCount = 0;
        _updateSurfaceCallCount = 0;
        _updateSource = 0;
        _updateDestination = 0;
        _lockRectangle = default;
        _lockFlags = 0;
    }

    [TestMethod]
    public void WhenRecreatingSystemMemorySurfaceThenWrapperOwnsReferenceAndTemporaryReferenceIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new(isVideoMemory: false);
        _surfaceToReturn = (nint) surfaceObject.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9VideoMemoryTextureManager manager = new();
        manager.SetRealizationParameters(device, CreateRequirements(16, 8));

        int lockResult = manager.ReCreateAndLockSystemMemorySurface(out LockedRect lockedRect);
        int unlockResult = manager.UnlockSystemMemorySurface();
        manager.Dispose();

        Assert.AreEqual(
            (0, 0, 64, (nint) 42, new Direct3D9SurfaceRect(0, 0, 16, 8), (uint) D3D9.LockNoDirtyUpdate, 1, 2, 1, 1),
            (lockResult, unlockResult, lockedRect.Pitch, (nint) lockedRect.PBits, _lockRectangle, _lockFlags, _surfaceAddRefCount, _surfaceReleaseCount, _lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenSystemMemorySurfaceWrappingFailsThenBothReferencesAreReleasedWithoutLocking()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new(isVideoMemory: false);
        _surfaceToReturn = (nint) surfaceObject.Surface;
        _getDescriptionResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9VideoMemoryTextureManager manager = new();
        manager.SetRealizationParameters(device, CreateRequirements(4, 2));

        int result = manager.ReCreateAndLockSystemMemorySurface(out LockedRect lockedRect);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, 1, 2, 0, false),
            (result, lockedRect.Pitch, _surfaceAddRefCount, _surfaceReleaseCount, _lockCallCount, manager.IsSystemMemorySurfaceValid));
    }

    [TestMethod]
    public void WhenPushingBitsThenWholeSurfaceIsUploadedAndTemporaryLevelReferenceIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject systemMemorySurface = new(isVideoMemory: false);
        using FakeSurfaceObject videoMemorySurface = new(isVideoMemory: true);
        using FakeTextureObject textureObject = new();
        _surfaceToReturn = (nint) systemMemorySurface.Surface;
        _videoMemorySurface = (nint) videoMemorySurface.Surface;
        _textureToReturn = (nint) textureObject.Texture;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9VideoMemoryTextureManager manager = new();
        manager.SetRealizationParameters(device, CreateRequirements(16, 8));
        _ = manager.ReCreateAndLockSystemMemorySurface(out _);
        _ = manager.UnlockSystemMemorySurface();

        uint useContextDepth = device.ResourceManager.EnterUseContext();
        int result;
        try
        {
            result = manager.PushBitsToVideoMemoryTexture();
        }
        finally
        {
            device.ResourceManager.ExitUseContext(useContextDepth);
        }

        Assert.AreEqual(
            (0, 1, (nint) systemMemorySurface.Surface, (nint) videoMemorySurface.Surface, 1, 3, 2, true),
            (result, _updateSurfaceCallCount, _updateSource, _updateDestination, _getSurfaceLevelCallCount,
                _videoMemorySurfaceAddRefCount, _videoMemorySurfaceReleaseCount, manager.VideoMemoryTexture is not null));
    }

    [TestMethod]
    public void WhenVideoMemorySurfaceQueryFailsThenTemporaryReferenceIsReleasedAndUploadIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject systemMemorySurface = new(isVideoMemory: false);
        using FakeSurfaceObject videoMemorySurface = new(isVideoMemory: true);
        using FakeTextureObject textureObject = new();
        _surfaceToReturn = (nint) systemMemorySurface.Surface;
        _videoMemorySurface = (nint) videoMemorySurface.Surface;
        _textureToReturn = (nint) textureObject.Texture;
        _getSurfaceLevelResult = Direct3D9Factory.OutOfMemoryHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9VideoMemoryTextureManager manager = new();
        manager.SetRealizationParameters(device, CreateRequirements(16, 8));
        _ = manager.ReCreateAndLockSystemMemorySurface(out _);
        _ = manager.UnlockSystemMemorySurface();

        uint useContextDepth = device.ResourceManager.EnterUseContext();
        int result;
        try
        {
            result = manager.PushBitsToVideoMemoryTexture();
        }
        finally
        {
            device.ResourceManager.ExitUseContext(useContextDepth);
        }

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, 1, 0, 1, 1, 1, true),
            (result, _getSurfaceLevelCallCount, _updateSurfaceCallCount, _videoMemorySurfaceAddRefCount,
                _videoMemorySurfaceReleaseCount, _textureReleaseCount, manager.VideoMemoryTexture is not null));
    }

    [TestMethod]
    public void WhenSurfaceUploadFailsThenFirstHResultIsReturnedAndMipmapsAreSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject systemMemorySurface = new(isVideoMemory: false);
        using FakeSurfaceObject videoMemorySurface = new(isVideoMemory: true);
        using FakeTextureObject textureObject = new();
        _surfaceToReturn = (nint) systemMemorySurface.Surface;
        _videoMemorySurface = (nint) videoMemorySurface.Surface;
        _textureToReturn = (nint) textureObject.Texture;
        _textureLevelCount = 2;
        _updateSurfaceResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, canAutoGenerateMipmaps: true);
        using Direct3D9VideoMemoryTextureManager manager = new();
        manager.SetRealizationParameters(device, CreateRequirements(16, 8, levels: 2));
        _ = manager.ReCreateAndLockSystemMemorySurface(out _);
        _ = manager.UnlockSystemMemorySurface();

        uint useContextDepth = device.ResourceManager.EnterUseContext();
        int result;
        try
        {
            result = manager.PushBitsToVideoMemoryTexture();
        }
        finally
        {
            device.ResourceManager.ExitUseContext(useContextDepth);
        }

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 0, 2, true),
            (result, _updateSurfaceCallCount, _generateMipSubLevelsCallCount,
                _videoMemorySurfaceReleaseCount, manager.VideoMemoryTexture is not null));
    }

    [TestMethod]
    public void WhenVideoMemoryTextureBecomesInvalidThenItIsReleasedAndRecreatedBeforeUpload()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject systemMemorySurface = new(isVideoMemory: false);
        using FakeSurfaceObject videoMemorySurface = new(isVideoMemory: true);
        using FakeTextureObject firstTextureObject = new();
        using FakeTextureObject secondTextureObject = new();
        _surfaceToReturn = (nint) systemMemorySurface.Surface;
        _videoMemorySurface = (nint) videoMemorySurface.Surface;
        _textureToReturn = (nint) firstTextureObject.Texture;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9VideoMemoryTextureManager manager = new();
        manager.SetRealizationParameters(device, CreateRequirements(16, 8));
        _ = manager.ReCreateAndLockSystemMemorySurface(out _);
        _ = manager.UnlockSystemMemorySurface();

        uint useContextDepth = device.ResourceManager.EnterUseContext();
        int firstResult;
        int secondResult;
        try
        {
            firstResult = manager.PushBitsToVideoMemoryTexture();
            manager.VideoMemoryTexture!.Dispose();
            _textureToReturn = (nint) secondTextureObject.Texture;
            secondResult = manager.PushBitsToVideoMemoryTexture();
        }
        finally
        {
            device.ResourceManager.ExitUseContext(useContextDepth);
        }

        Assert.AreEqual(
            (0, 0, 2, 2, 3, true),
            (firstResult, secondResult, _createTextureCallCount, _updateSurfaceCallCount,
                _textureReleaseCount, manager.VideoMemoryTexture is not null));
    }

    [TestMethod]
    public void WhenPreparingForNewRealizationThenResourcesAndParametersAreReset()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject systemMemorySurface = new(isVideoMemory: false);
        _surfaceToReturn = (nint) systemMemorySurface.Surface;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9VideoMemoryTextureManager manager = new();
        manager.SetRealizationParameters(device, CreateRequirements(16, 8));
        _ = manager.ReCreateAndLockSystemMemorySurface(out _);
        _ = manager.UnlockSystemMemorySurface();

        manager.PrepareForNewRealization();
        manager.SetRealizationParameters(device, CreateRequirements(4, 2));

        Assert.AreEqual((true, false, null, 2),
            (manager.HasRealizationParameters, manager.IsSystemMemorySurfaceValid,
                manager.VideoMemoryTexture, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenDisposedThenFurtherOperationsAreRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9VideoMemoryTextureManager manager = new();
        manager.Dispose();
        manager.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            manager.SetRealizationParameters(device, CreateRequirements(1, 1)));
        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.ReCreateAndLockSystemMemorySurface(out _));
        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.UnlockSystemMemorySurface());
        Assert.ThrowsExactly<ObjectDisposedException>(() => manager.PushBitsToVideoMemoryTexture());
        Assert.ThrowsExactly<ObjectDisposedException>(manager.PrepareForNewRealization);
    }

    private static Direct3D9BitmapTextureRequirements CreateRequirements(uint width, uint height, uint levels = 1) =>
        new(
            new SurfaceDesc(
                format: Format.A8R8G8B8,
                type: Resourcetype.Texture,
                pool: Pool.Default,
                width: width,
                height: height),
            Levels: levels);

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device, bool canAutoGenerateMipmaps = false)
    {
        Caps9 capabilities = default;
        capabilities.Caps2 = D3D9.Caps2Canshareresource
            | (canAutoGenerateMipmaps ? (uint) D3D9.Caps2Canautogenmipmap : 0);
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default, capabilities: capabilities);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

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
        *texture = (IDirect3DTexture9*) _textureToReturn;
        return Direct3D9Factory.SuccessHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UpdateSurface(
        IDirect3DDevice9* self,
        IDirect3DSurface9* source,
        Direct3D9SurfaceRect* sourceRectangle,
        IDirect3DSurface9* destination,
        Direct3D9Point* destinationPoint)
    {
        _updateSurfaceCallCount++;
        _updateSource = (nint) source;
        _updateDestination = (nint) destination;
        return _updateSurfaceResult;
    }

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
        *surface = (IDirect3DSurface9*) _surfaceToReturn;
        return Direct3D9Factory.SuccessHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefSurface(IDirect3DSurface9* self)
    {
        _surfaceAddRefCount++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self)
    {
        _surfaceReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        *description = new SurfaceDesc(
            format: Format.A8R8G8B8,
            type: Resourcetype.Surface,
            pool: Pool.Systemmem,
            width: 16,
            height: 8);
        return _getDescriptionResult;
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
        return Direct3D9Factory.SuccessHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockSurface(IDirect3DSurface9* self)
    {
        _unlockCallCount++;
        return Direct3D9Factory.SuccessHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefVideoMemorySurface(IDirect3DSurface9* self)
    {
        _videoMemorySurfaceAddRefCount++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseVideoMemorySurface(IDirect3DSurface9* self)
    {
        _videoMemorySurfaceReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetVideoMemorySurfaceDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        *description = new SurfaceDesc(
            format: Format.A8R8G8B8,
            type: Resourcetype.Surface,
            pool: Pool.Default,
            width: 16,
            height: 8);
        return Direct3D9Factory.SuccessHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefTexture(IDirect3DTexture9* self)
    {
        _textureAddRefCount++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseTexture(IDirect3DTexture9* self)
    {
        _textureReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void GenerateMipSubLevels(IDirect3DTexture9* self)
    {
        _generateMipSubLevelsCallCount++;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint GetTextureLevelCount(IDirect3DTexture9* self) => _textureLevelCount;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetTextureLevelDescription(IDirect3DTexture9* self, uint level, SurfaceDesc* description)
    {
        *description = new SurfaceDesc(
            format: Format.A8R8G8B8,
            type: Resourcetype.Texture,
            pool: Pool.Default,
            width: 16,
            height: 8);
        return Direct3D9Factory.SuccessHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetTextureSurfaceLevel(IDirect3DTexture9* self, uint level, IDirect3DSurface9** surface)
    {
        _getSurfaceLevelCallCount++;
        *surface = (IDirect3DSurface9*) _videoMemorySurface;
        if (*surface is not null)
        {
            _videoMemorySurfaceAddRefCount++;
        }

        return _getSurfaceLevelResult;
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
            vtable[30] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9Point*, int>) &UpdateSurface;
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
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 22);
            void** memory = (void**) _memory;
            Texture = (IDirect3DTexture9*) memory;
            void** vtable = memory + 1;
            Texture->LpVtbl = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &AddRefTexture;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &ReleaseTexture;
            vtable[13] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &GetTextureLevelCount;
            vtable[16] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, void>) &GenerateMipSubLevels;
            vtable[17] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, SurfaceDesc*, int>) &GetTextureLevelDescription;
            vtable[18] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) &GetTextureSurfaceLevel;
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

        public FakeSurfaceObject(bool isVideoMemory = false)
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 18);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            Surface->LpVtbl = vtable;
            vtable[1] = isVideoMemory
                ? (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &AddRefVideoMemorySurface
                : (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &AddRefSurface;
            vtable[2] = isVideoMemory
                ? (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseVideoMemorySurface
                : (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
            vtable[12] = isVideoMemory
                ? (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetVideoMemorySurfaceDescription
                : (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetSurfaceDescription;
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
