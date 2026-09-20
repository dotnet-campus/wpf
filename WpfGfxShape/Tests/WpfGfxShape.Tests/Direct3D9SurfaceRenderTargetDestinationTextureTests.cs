using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9SurfaceRenderTargetDestinationTextureTests
{
    private static int _getSurfaceLevelResult;
    private static uint _requestedLevel;
    private static int _stretchRectResult;
    private static int _stretchRectCallCount;
    private static Direct3D9SurfaceRect _sourceRect;
    private static Direct3D9SurfaceRect _destinationRect;
    private static Texturefiltertype _filter;
    private static bool _deviceWasInUseContext;
    private static Direct3D9Device? _device;
    private static IDirect3DSurface9* _destinationSurface;

    [TestInitialize]
    public void Initialize()
    {
        _getSurfaceLevelResult = 0;
        _requestedLevel = uint.MaxValue;
        _stretchRectResult = 0;
        _stretchRectCallCount = 0;
        _sourceRect = default;
        _destinationRect = default;
        _filter = default;
        _deviceWasInUseContext = false;
        _device = null;
        _destinationSurface = null;
    }

    [TestMethod]
    public void WhenPopulatingDestinationTextureThenLevelZeroAndRectanglesAreCopiedWithoutFiltering()
    {
        using FakeNativeObjects native = new();
        using Direct3D9Device device = new(native.Device, null, 0, Devtype.Hal, 0, default);
        _device = device;
        using Direct3D9Surface sourceSurface = new(new Direct3D9ResourceManager(), native.SourceSurface);
        using Direct3D9Texture destinationTexture = new(new Direct3D9ResourceManager(), native.Texture, 64, 64);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: sourceSurface);
        Direct3D9SurfaceRect sourceRect = new(2, 3, 18, 19);
        Direct3D9SurfaceRect destinationRect = new(5, 7, 21, 23);

        int result = renderTarget.PopulateDestinationTexture(sourceRect, destinationRect, destinationTexture);

        Assert.AreEqual(
            (0, 0u, 1, sourceRect, destinationRect, Texturefiltertype.None, true),
            (result, _requestedLevel, _stretchRectCallCount, _sourceRect, _destinationRect, _filter, _deviceWasInUseContext));
    }

    [TestMethod]
    public void WhenGettingDestinationSurfaceFailsThenStretchRectIsSkipped()
    {
        using FakeNativeObjects native = new();
        using Direct3D9Device device = new(native.Device, null, 0, Devtype.Hal, 0, default);
        using Direct3D9Surface sourceSurface = new(new Direct3D9ResourceManager(), native.SourceSurface);
        using Direct3D9Texture destinationTexture = new(new Direct3D9ResourceManager(), native.Texture, 64, 64);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: sourceSurface);
        _getSurfaceLevelResult = Direct3D9Factory.InvalidCallHResult;

        int result = renderTarget.PopulateDestinationTexture(
            new Direct3D9SurfaceRect(0, 0, 16, 16),
            new Direct3D9SurfaceRect(8, 8, 24, 24),
            destinationTexture);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 0), (result, _stretchRectCallCount));
    }

    [TestMethod]
    public void WhenStretchRectFailsThenFirstFailureIsReturned()
    {
        using FakeNativeObjects native = new();
        using Direct3D9Device device = new(native.Device, null, 0, Devtype.Hal, 0, default);
        using Direct3D9Surface sourceSurface = new(new Direct3D9ResourceManager(), native.SourceSurface);
        using Direct3D9Texture destinationTexture = new(new Direct3D9ResourceManager(), native.Texture, 64, 64);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: sourceSurface);
        _stretchRectResult = Direct3D9Factory.GenericFailureHResult;

        int result = renderTarget.PopulateDestinationTexture(
            new Direct3D9SurfaceRect(0, 0, 16, 16),
            new Direct3D9SurfaceRect(8, 8, 24, 24),
            destinationTexture);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenRenderTargetIsDisposedThenPopulateDestinationTextureThrows()
    {
        using FakeNativeObjects native = new();
        using Direct3D9Device device = new(native.Device, null, 0, Devtype.Hal, 0, default);
        using Direct3D9Surface sourceSurface = new(new Direct3D9ResourceManager(), native.SourceSurface);
        using Direct3D9Texture destinationTexture = new(new Direct3D9ResourceManager(), native.Texture, 64, 64);
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: sourceSurface);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.PopulateDestinationTexture(
            new Direct3D9SurfaceRect(0, 0, 1, 1),
            new Direct3D9SurfaceRect(0, 0, 1, 1),
            destinationTexture));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int StretchRect(
        IDirect3DDevice9* self,
        IDirect3DSurface9* source,
        Direct3D9SurfaceRect* sourceRect,
        IDirect3DSurface9* destination,
        Direct3D9SurfaceRect* destinationRect,
        Texturefiltertype filter)
    {
        _stretchRectCallCount++;
        _sourceRect = *sourceRect;
        _destinationRect = *destinationRect;
        _filter = filter;
        _deviceWasInUseContext = _device?.IsInUseContext() == true;
        return _stretchRectResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefSurface(IDirect3DSurface9* self) => 1;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        *description = new SurfaceDesc(
            format: Format.A8R8G8B8,
            type: Resourcetype.Surface,
            pool: Pool.Default,
            width: 64,
            height: 64);
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseTexture(IDirect3DTexture9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceLevel(IDirect3DTexture9* self, uint level, IDirect3DSurface9** surface)
    {
        _requestedLevel = level;
        *surface = _getSurfaceLevelResult < 0 ? null : _destinationSurface;
        return _getSurfaceLevelResult;
    }

    private struct FakeNativeObjects : IDisposable
    {
        private nint _deviceMemory;
        private nint _textureMemory;
        private nint _sourceSurfaceMemory;
        private nint _destinationSurfaceMemory;

        internal IDirect3DDevice9* Device;
        internal IDirect3DTexture9* Texture;
        internal IDirect3DSurface9* SourceSurface;

        public FakeNativeObjects()
        {
            _deviceMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 107);
            void** deviceMemory = (void**) _deviceMemory;
            Device = (IDirect3DDevice9*) deviceMemory;
            Device->LpVtbl = deviceMemory + 1;
            Device->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            Device->LpVtbl[34] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int>) &StretchRect;

            _sourceSurfaceMemory = CreateSurface(out SourceSurface);
            _destinationSurfaceMemory = CreateSurface(out _destinationSurface);

            _textureMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 24);
            void** textureMemory = (void**) _textureMemory;
            Texture = (IDirect3DTexture9*) textureMemory;
            Texture->LpVtbl = textureMemory + 1;
            Texture->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &ReleaseTexture;
            Texture->LpVtbl[18] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) &GetSurfaceLevel;
        }

        private static nint CreateSurface(out IDirect3DSurface9* surface)
        {
            nint allocation = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 15);
            void** memory = (void**) allocation;
            surface = (IDirect3DSurface9*) memory;
            surface->LpVtbl = memory + 1;
            surface->LpVtbl[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &AddRefSurface;
            surface->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
            surface->LpVtbl[12] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetSurfaceDescription;
            return allocation;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _textureMemory);
            NativeMemory.Free((void*) _destinationSurfaceMemory);
            NativeMemory.Free((void*) _sourceSurfaceMemory);
            NativeMemory.Free((void*) _deviceMemory);
            _textureMemory = 0;
            _destinationSurfaceMemory = 0;
            _sourceSurfaceMemory = 0;
            _deviceMemory = 0;
            Device = null;
            Texture = null;
            SourceSurface = null;
            _destinationSurface = null;
        }
    }
}
