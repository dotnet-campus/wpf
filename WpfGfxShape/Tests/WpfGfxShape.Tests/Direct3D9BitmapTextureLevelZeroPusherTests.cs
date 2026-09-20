using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapTextureLevelZeroPusherTests
{
    private static byte* _pixels;
    private static int _lockResult;
    private static int _unlockResult;
    private static int _copyResult;
    private static int _updateResult;
    private static int _lockCount;
    private static int _unlockCount;
    private static int _copyCount;
    private static Direct3D9BitmapSourceRectangle _copiedSource;
    private static uint _copiedPitch;
    private static uint _copiedBufferSize;
    private static nint _copiedDestination;
    private static int _getSurfaceCount;
    private static int _updateCount;
    private static Direct3D9SurfaceRect _updatedSource;
    private static Direct3D9Point _updatedDestination;
    private static nint _destinationSurface;

    [TestInitialize]
    public void Initialize()
    {
        _pixels = (byte*) NativeMemory.AllocZeroed(256);
        _lockResult = 0;
        _unlockResult = 0;
        _copyResult = 0;
        _updateResult = 0;
        _lockCount = 0;
        _unlockCount = 0;
        _copyCount = 0;
        _copiedSource = default;
        _copiedPitch = 0;
        _copiedBufferSize = 0;
        _copiedDestination = 0;
        _getSurfaceCount = 0;
        _updateCount = 0;
        _updatedSource = default;
        _updatedDestination = default;
        _destinationSurface = 0;
    }

    [TestCleanup]
    public void Cleanup()
    {
        NativeMemory.Free(_pixels);
        _pixels = null;
    }

    [TestMethod]
    public void WhenNaturalLayoutPushesCopiedBitsThenSurfaceIsUnlockedBeforeLevelZeroUpdate()
    {
        using FakeBitmapSourceObject bitmapSourceObject = new();
        using FakeSurfaceObject systemMemoryObject = new(isDestination: false);
        using FakeSurfaceObject destinationObject = new(isDestination: true);
        using FakeTextureObject textureObject = new(destinationObject.Surface);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9SystemMemoryUpdateSurface systemMemorySurface = new(systemMemoryObject.Surface);
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 8, 8);
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        Direct3D9BitmapTextureLevelZeroPusher pusher = CreatePusher(texture, device);
        Direct3D9BitmapRealizationRectangle[] dirtyRectangles =
        [
            new(11, 22, 14, 25)
        ];

        int result = pusher.Push(bitmapSourceObject.BitmapSource, dirtyRectangles, systemMemorySurface, copySourceToSystemMemorySurface: true);

        Assert.AreEqual(
            (0, 1, 1, new Direct3D9BitmapSourceRectangle(11, 22, 3, 3), 32u, 256u, (nint) (_pixels + 68), 1, 1, 1, new Direct3D9SurfaceRect(1, 2, 4, 5), new Direct3D9Point(1, 2), (nint) destinationObject.Surface),
            (result, _lockCount, _copyCount, _copiedSource, _copiedPitch, _copiedBufferSize, _copiedDestination, _unlockCount, _getSurfaceCount, _updateCount, _updatedSource, _updatedDestination, _destinationSurface));
    }

    [TestMethod]
    public void WhenBitmapCopyFailsThenSurfaceIsUnlockedAndVideoMemoryIsNotUpdated()
    {
        using FakeSurfaceObject systemMemoryObject = new(isDestination: false);
        using FakeSurfaceObject destinationObject = new(isDestination: true);
        using FakeTextureObject textureObject = new(destinationObject.Surface);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9SystemMemoryUpdateSurface systemMemorySurface = new(systemMemoryObject.Surface);
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 8, 8);
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        using FakeBitmapSourceObject bitmapSourceObject = new();
        _copyResult = Direct3D9Factory.GenericFailureHResult;

        int result = CreatePusher(texture, device).Push(
            bitmapSourceObject.BitmapSource,
            [new Direct3D9BitmapRealizationRectangle(10, 20, 12, 22)],
            systemMemorySurface,
            copySourceToSystemMemorySurface: true);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, 1, 1, 0, 0),
            (result, _lockCount, _copyCount, _unlockCount, _getSurfaceCount, _updateCount));
    }

    [TestMethod]
    public void WhenBitmapBitsAreSharedThenBitmapCoordinatesAreUpdatedWithoutLockingOrCopying()
    {
        using FakeSurfaceObject sharedSurfaceObject = new(isDestination: false);
        using FakeSurfaceObject destinationObject = new(isDestination: true);
        using FakeTextureObject textureObject = new(destinationObject.Surface);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9SystemMemoryUpdateSurface sharedSurface = new(sharedSurfaceObject.Surface);
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 8, 8);
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);

        int result = CreatePusher(texture, device).Push(
            0,
            [new Direct3D9BitmapRealizationRectangle(11, 22, 14, 25)],
            sharedSurface,
            copySourceToSystemMemorySurface: false);

        Assert.AreEqual(
            (0, 0, 0, 0, 1, 1, new Direct3D9SurfaceRect(11, 22, 14, 25), new Direct3D9Point(1, 2), (nint) destinationObject.Surface),
            (result, _lockCount, _copyCount, _unlockCount, _getSurfaceCount, _updateCount, _updatedSource, _updatedDestination, _destinationSurface));
    }

    [TestMethod]
    public void WhenSharedSurfaceUpdateFailsThenLaterDirtyRectanglesAreNotUpdated()
    {
        using FakeSurfaceObject sharedSurfaceObject = new(isDestination: false);
        using FakeSurfaceObject destinationObject = new(isDestination: true);
        using FakeTextureObject textureObject = new(destinationObject.Surface);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9SystemMemoryUpdateSurface sharedSurface = new(sharedSurfaceObject.Surface);
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 8, 8);
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        _updateResult = Direct3D9Factory.GenericFailureHResult;

        int result = CreatePusher(texture, device).Push(
            0,
            [
                new Direct3D9BitmapRealizationRectangle(10, 20, 12, 22),
                new Direct3D9BitmapRealizationRectangle(12, 22, 14, 24)
            ],
            sharedSurface,
            copySourceToSystemMemorySurface: false);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0, 1, 1, new Direct3D9SurfaceRect(10, 20, 12, 22), new Direct3D9Point(0, 0)),
            (result, _lockCount, _copyCount, _getSurfaceCount, _updateCount, _updatedSource, _updatedDestination));
    }

    [TestMethod]
    public void WhenWrappedBorderEdgeIsDirtyThenEntireSourceAndBorderAreUploaded()
    {
        using FakeBitmapSourceObject bitmapSourceObject = new();
        using FakeSurfaceObject systemMemoryObject = new(isDestination: false);
        using FakeSurfaceObject destinationObject = new(isDestination: true);
        using FakeTextureObject textureObject = new(destinationObject.Surface);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9SystemMemoryUpdateSurface systemMemorySurface = new(systemMemoryObject.Surface);
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 8, 8);
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        Direct3D9BitmapTextureLevelZeroPusher pusher = new(
            texture,
            device,
            CreateRequirements(),
            Direct3D9TexelLayout.EdgeWrapped,
            Direct3D9TexelLayout.EdgeWrapped,
            new Direct3D9BitmapRealizationRectangle(10, 20, 16, 26));

        int result = pusher.Push(
            bitmapSourceObject.BitmapSource,
            [new Direct3D9BitmapRealizationRectangle(10, 22, 12, 24)],
            systemMemorySurface,
            copySourceToSystemMemorySurface: true);

        Assert.AreEqual(
            (0, 1, new Direct3D9BitmapSourceRectangle(10, 20, 6, 6), (nint) (_pixels + 36), 1, 1, new Direct3D9SurfaceRect(0, 0, 8, 8), new Direct3D9Point(0, 0), 45, 40, 49, 44),
            (result, _copyCount, _copiedSource, _copiedDestination, _unlockCount, _updateCount, _updatedSource, _updatedDestination, ReadPixel(0, 1), ReadPixel(7, 1), ReadPixel(0, 2), ReadPixel(7, 2)));
    }

    [TestMethod]
    public void WhenMirroredBorderInteriorIsDirtyThenOnlyOffsetInteriorRectangleIsUploaded()
    {
        using FakeBitmapSourceObject bitmapSourceObject = new();
        using FakeSurfaceObject systemMemoryObject = new(isDestination: false);
        using FakeSurfaceObject destinationObject = new(isDestination: true);
        using FakeTextureObject textureObject = new(destinationObject.Surface);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9SystemMemoryUpdateSurface systemMemorySurface = new(systemMemoryObject.Surface);
        using Direct3D9Texture texture = new(new Direct3D9ResourceManager(), textureObject.Texture, 8, 8);
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        Direct3D9BitmapTextureLevelZeroPusher pusher = new(
            texture,
            device,
            CreateRequirements(),
            Direct3D9TexelLayout.EdgeMirrored,
            Direct3D9TexelLayout.EdgeMirrored,
            new Direct3D9BitmapRealizationRectangle(10, 20, 16, 26));

        int result = pusher.Push(
            bitmapSourceObject.BitmapSource,
            [new Direct3D9BitmapRealizationRectangle(11, 21, 13, 23)],
            systemMemorySurface,
            copySourceToSystemMemorySurface: true);

        Assert.AreEqual(
            (0, new Direct3D9BitmapSourceRectangle(11, 21, 2, 2), (nint) (_pixels + 72), new Direct3D9SurfaceRect(2, 2, 4, 4), new Direct3D9Point(2, 2)),
            (result, _copiedSource, _copiedDestination, _updatedSource, _updatedDestination));
    }

    private static Direct3D9BitmapTextureLevelZeroPusher CreatePusher(
        Direct3D9Texture texture,
        Direct3D9Device device) =>
        new(
            texture,
            device,
            CreateRequirements(),
            Direct3D9TexelLayout.Natural,
            Direct3D9TexelLayout.FirstOnly,
            new Direct3D9BitmapRealizationRectangle(10, 20, 18, 28));

    private static Direct3D9BitmapTextureRequirements CreateRequirements() =>
        new(
            new SurfaceDesc(
                format: Format.A8R8G8B8,
                type: Resourcetype.Texture,
                pool: Pool.Default,
                width: 8,
                height: 8),
            1);

    private static byte ReadPixel(int x, int y) => _pixels[y * 32 + x * 4];

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CopyPixels(
        void*** bitmapSource,
        Direct3D9BitmapSourceRectangle* sourceRectangle,
        uint destinationPitch,
        uint destinationBufferSize,
        byte* destinationPixels)
    {
        _copyCount++;
        _copiedSource = *sourceRectangle;
        _copiedPitch = destinationPitch;
        _copiedBufferSize = destinationBufferSize;
        _copiedDestination = (nint) destinationPixels;
        if (_copyResult >= 0)
        {
            for (int y = 0; y < sourceRectangle->Height; y++)
            {
                for (int x = 0; x < sourceRectangle->Width; x++)
                {
                    destinationPixels[y * destinationPitch + x * 4] = checked((byte) (40 + y * 4 + x));
                }
            }
        }
        return _copyResult;
    }

    private struct FakeBitmapSourceObject : IDisposable
    {
        private nint _memory;
        internal nint BitmapSource;

        public FakeBitmapSourceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 9);
            void** memory = (void**) _memory;
            BitmapSource = (nint) memory;
            void** vtable = memory + 1;
            *memory = vtable;
            vtable[7] = (void*) (delegate* unmanaged[Stdcall]<void***, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int>) &CopyPixels;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            BitmapSource = 0;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefSurface(IDirect3DSurface9* self) => 1;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        *description = CreateRequirements().Description;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockSurface(
        IDirect3DSurface9* self,
        LockedRect* lockedRect,
        Direct3D9SurfaceRect* rectangle,
        uint flags)
    {
        _lockCount++;
        lockedRect->Pitch = 32;
        lockedRect->PBits = _pixels;
        return _lockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockSurface(IDirect3DSurface9* self)
    {
        _unlockCount++;
        return _unlockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseTexture(IDirect3DTexture9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceLevel(IDirect3DTexture9* self, uint level, IDirect3DSurface9** surface)
    {
        _getSurfaceCount++;
        *surface = (IDirect3DSurface9*) _destinationSurface;
        return 0;
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
        _updateCount++;
        _updatedSource = *sourceRectangle;
        _updatedDestination = *destinationPoint;
        _destinationSurface = (nint) destination;
        return _updateResult;
    }

    private struct FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        internal FakeSurfaceObject(bool isDestination)
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 16);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            Surface->LpVtbl = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &AddRefSurface;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetSurfaceDescription;
            vtable[13] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, LockedRect*, Direct3D9SurfaceRect*, uint, int>) &LockSurface;
            vtable[14] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, int>) &UnlockSurface;
            if (isDestination)
            {
                _destinationSurface = (nint) Surface;
            }
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }

    private struct FakeTextureObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DTexture9* Texture;

        internal FakeTextureObject(IDirect3DSurface9* destination)
        {
            _destinationSurface = (nint) destination;
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 20);
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
            vtable[30] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9Point*, int>) &UpdateSurface;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
