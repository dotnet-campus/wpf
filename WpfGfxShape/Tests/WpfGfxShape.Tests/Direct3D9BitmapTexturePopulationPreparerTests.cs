using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapTexturePopulationPreparerTests
{
    private static int _surfaceAddRefCount;
    private static int _surfaceReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _surfaceAddRefCount = 0;
        _surfaceReleaseCount = 0;
    }

    [TestMethod]
    public void WhenLddmStrideIsPixelAlignedThenBitmapBitsAreSharedAndLockIsTransferred()
    {
        using FakeSurfaceObject surfaceObject = new();
        List<string> calls = [];
        byte* bits = stackalloc byte[64];
        uint createdWidth = 0;
        uint createdHeight = 0;
        nint createdPixels = 0;
        using Direct3D9BitmapSystemMemorySurfaceSource surfaceSource = new(
            (uint width, uint height, Format _, void* pixels, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createdWidth = width;
                createdHeight = height;
                createdPixels = (nint) pixels;
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapTexturePopulationPreparer preparer = new(
            surfaceSource,
            (out nint bitmapLock) =>
            {
                calls.Add("lock");
                bitmapLock = 17;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint bufferSize, out nint data) =>
            {
                calls.Add("data");
                bufferSize = 64;
                data = (nint) bits;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint stride) =>
            {
                calls.Add("stride");
                stride = 32;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out MilPixelFormat format) =>
            {
                calls.Add("format");
                format = MilPixelFormat.Pbgra32Bpp;
                return Direct3D9Factory.SuccessHResult;
            },
            resource => calls.Add($"release:{resource}"),
            true,
            2,
            5,
            6);

        int result = preparer.Prepare(out nint bitmapLock, out bool copySource, out Direct3D9SystemMemoryUpdateSurface? surface);
        surface!.Dispose();
        surfaceSource.Dispose();

        Assert.AreEqual(
            (0, (nint) 17, false, "lock,data,stride,format", 8u, 2u, (nint) bits, 1, 2),
            (result, bitmapLock, copySource, string.Join(',', calls), createdWidth, createdHeight, createdPixels, _surfaceAddRefCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenStrideIsNotPixelAlignedThenLockIsReleasedAndCopySurfaceUsesRequiredSize()
    {
        using FakeSurfaceObject surfaceObject = new();
        List<nint> released = [];
        nint createdPixels = -1;
        uint createdWidth = 0;
        uint createdHeight = 0;
        using Direct3D9BitmapSystemMemorySurfaceSource surfaceSource = new(
            (uint width, uint height, Format _, void* pixels, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createdWidth = width;
                createdHeight = height;
                createdPixels = (nint) pixels;
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.R8G8B8);
        Direct3D9BitmapTexturePopulationPreparer preparer = new(
            surfaceSource,
            (out nint bitmapLock) =>
            {
                bitmapLock = 23;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint bufferSize, out nint data) =>
            {
                bufferSize = 30;
                data = 29;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint stride) =>
            {
                stride = 10;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out MilPixelFormat format) =>
            {
                format = MilPixelFormat.Bgr24Bpp;
                return Direct3D9Factory.SuccessHResult;
            },
            released.Add,
            true,
            3,
            11,
            12);

        int result = preparer.Prepare(out nint bitmapLock, out bool copySource, out Direct3D9SystemMemoryUpdateSurface? surface);
        surface!.Dispose();

        Assert.AreEqual(
            (0, (nint) 0, true, 11u, 12u, (nint) 0, "23", 0, 1),
            (result, bitmapLock, copySource, createdWidth, createdHeight, createdPixels, string.Join(',', released), _surfaceAddRefCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenLockMetadataQueryFailsThenLockIsReleasedAndSurfaceIsNotCreated()
    {
        int createCount = 0;
        List<nint> released = [];
        using Direct3D9BitmapSystemMemorySurfaceSource surfaceSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createCount++;
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapTexturePopulationPreparer preparer = new(
            surfaceSource,
            (out nint bitmapLock) =>
            {
                bitmapLock = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint bufferSize, out nint data) =>
            {
                bufferSize = 0;
                data = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (nint _, out uint stride) =>
            {
                stride = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out MilPixelFormat format) =>
            {
                format = MilPixelFormat.Undefined;
                return Direct3D9Factory.SuccessHResult;
            },
            released.Add,
            true,
            2,
            4,
            4);

        int result = preparer.Prepare(out nint bitmapLock, out bool copySource, out Direct3D9SystemMemoryUpdateSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, (nint) 0, true, null, 0, "31"),
            (result, bitmapLock, copySource, surface, createCount, string.Join(',', released)));
    }

    [TestMethod]
    public void WhenPreLddmBitmapIsDynamicThenBitsIdentifyCachedCopySurfaceAndLockIsTransferred()
    {
        using FakeSurfaceObject surfaceObject = new();
        List<string> calls = [];
        byte* bits = stackalloc byte[64];
        uint createdWidth = 0;
        uint createdHeight = 0;
        nint createdPixels = -1;
        using Direct3D9BitmapSystemMemorySurfaceSource surfaceSource = new(
            (uint width, uint height, Format _, void* pixels, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createdWidth = width;
                createdHeight = height;
                createdPixels = (nint) pixels;
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapTexturePopulationPreparer preparer = new(
            surfaceSource,
            (out nint bitmapLock) =>
            {
                calls.Add("lock");
                bitmapLock = 43;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint bufferSize, out nint data) =>
            {
                calls.Add("data");
                bufferSize = 64;
                data = (nint) bits;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint stride) =>
            {
                calls.Add("stride");
                stride = 32;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out MilPixelFormat format) =>
            {
                calls.Add("format");
                format = MilPixelFormat.Pbgra32Bpp;
                return Direct3D9Factory.SuccessHResult;
            },
            resource => calls.Add($"release:{resource}"),
            37,
            (nint bitmap, out nint dynamicResource) =>
            {
                calls.Add($"query:{bitmap}");
                dynamicResource = 41;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint resource, out bool isDynamic) =>
            {
                calls.Add($"dynamic:{resource}");
                isDynamic = true;
                return Direct3D9Factory.SuccessHResult;
            },
            true,
            false,
            2,
            5,
            6);

        int result = preparer.Prepare(out nint bitmapLock, out bool copySource, out Direct3D9SystemMemoryUpdateSurface? surface);
        surface!.Dispose();
        surfaceSource.Dispose();

        Assert.AreEqual(
            (0, (nint) 43, true, 5u, 6u, (nint) 0, "query:37,dynamic:41,lock,data,stride,format,release:41", 1, 2),
            (result, bitmapLock, copySource, createdWidth, createdHeight, createdPixels, string.Join(',', calls), _surfaceAddRefCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenPreLddmDynamicInterfaceIsUnavailableThenBitmapIsNotLocked()
    {
        using FakeSurfaceObject surfaceObject = new();
        int lockCount = 0;
        int dynamicCount = 0;
        List<nint> released = [];
        using Direct3D9BitmapSystemMemorySurfaceSource surfaceSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapTexturePopulationPreparer preparer = new(
            surfaceSource,
            (out nint bitmapLock) =>
            {
                lockCount++;
                bitmapLock = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out uint bufferSize, out nint data) => throw new InvalidOperationException(),
            (nint _, out uint stride) => throw new InvalidOperationException(),
            (nint _, out MilPixelFormat format) => throw new InvalidOperationException(),
            released.Add,
            47,
            (nint _, out nint dynamicResource) =>
            {
                dynamicResource = 0;
                return Direct3D9Factory.NoInterfaceHResult;
            },
            (nint _, out bool isDynamic) =>
            {
                dynamicCount++;
                isDynamic = false;
                return Direct3D9Factory.SuccessHResult;
            },
            true,
            false,
            2,
            5,
            6);

        int result = preparer.Prepare(out nint bitmapLock, out bool copySource, out Direct3D9SystemMemoryUpdateSurface? surface);
        surface!.Dispose();

        Assert.AreEqual(
            (0, (nint) 0, true, 0, 0, 0),
            (result, bitmapLock, copySource, lockCount, dynamicCount, released.Count));
    }

    [TestMethod]
    public void WhenPreLddmDynamicQueryFailsThenInterfaceIsReleasedAndSurfaceIsNotCreated()
    {
        int createCount = 0;
        List<nint> released = [];
        using Direct3D9BitmapSystemMemorySurfaceSource surfaceSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                createCount++;
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapTexturePopulationPreparer preparer = new(
            surfaceSource,
            (out nint bitmapLock) => throw new InvalidOperationException(),
            (nint _, out uint bufferSize, out nint data) => throw new InvalidOperationException(),
            (nint _, out uint stride) => throw new InvalidOperationException(),
            (nint _, out MilPixelFormat format) => throw new InvalidOperationException(),
            released.Add,
            53,
            (nint _, out nint dynamicResource) =>
            {
                dynamicResource = 59;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, out bool isDynamic) =>
            {
                isDynamic = false;
                return Direct3D9Factory.GenericFailureHResult;
            },
            true,
            false,
            2,
            5,
            6);

        int result = preparer.Prepare(out nint bitmapLock, out bool copySource, out Direct3D9SystemMemoryUpdateSurface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, (nint) 0, true, null, 0, "59"),
            (result, bitmapLock, copySource, surface, createCount, string.Join(',', released)));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(void*** surface)
    {
        _surfaceAddRefCount++;
        return (uint) (_surfaceAddRefCount + 1);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void*** surface)
    {
        _surfaceReleaseCount++;
        return 0;
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
            *memory = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &AddRef;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &Release;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
