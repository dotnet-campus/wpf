using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapTexturePopulatorStagingTests
{
    private static int _releaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _releaseCount = 0;
    }

    [TestMethod]
    public void WhenDirtyPopulationSucceedsThenTemporaryStagingSurfaceIsReleasedAfterCommit()
    {
        using FakeSurfaceObject surfaceObject = new();
        List<string> calls = [];
        Direct3D9BitmapRealizationRectangle[] dirtyRectangles = [new(1, 2, 3, 4)];
        fixed (Direct3D9BitmapRealizationRectangle* dirtyRectanglePointer = dirtyRectangles)
        {
            Direct3D9BitmapTexturePopulator populator = new(
                CreateStagingSurface,
                Push,
                UpdateMipmaps,
                Commit);

            int result = populator.Populate(
                5,
                1,
                (nint) dirtyRectanglePointer,
                7,
                new Direct3D9BitmapRealizationRectangle(8, 9, 10, 11));

            Assert.AreEqual(
                (0, "create,push:5:1:2:3:4:True,mipmaps,commit:7:8:9:10:11", 1),
                (result, string.Join(',', calls), _releaseCount));
        }

        int CreateStagingSurface(out Direct3D9SystemMemoryUpdateSurface? surface)
        {
            calls.Add("create");
            surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
            return Direct3D9Factory.SuccessHResult;
        }

        int Push(
            nint bitmapSource,
            ReadOnlySpan<Direct3D9BitmapRealizationRectangle> rectangles,
            Direct3D9SystemMemoryUpdateSurface surface,
            bool copySource)
        {
            Direct3D9BitmapRealizationRectangle rectangle = rectangles[0];
            calls.Add($"push:{bitmapSource}:{rectangle.Left}:{rectangle.Top}:{rectangle.Right}:{rectangle.Bottom}:{copySource}");
            return Direct3D9Factory.SuccessHResult;
        }

        int UpdateMipmaps()
        {
            calls.Add("mipmaps");
            return Direct3D9Factory.SuccessHResult;
        }

        void Commit(uint uniqueness, Direct3D9BitmapRealizationRectangle bounds) =>
            calls.Add($"commit:{uniqueness}:{bounds.Left}:{bounds.Top}:{bounds.Right}:{bounds.Bottom}");
    }

    [TestMethod]
    public void WhenStagingSurfaceCreationFailsThenPushMipmapsAndCommitAreSkipped()
    {
        List<string> calls = [];
        Direct3D9BitmapRealizationRectangle dirtyRectangle = new(1, 2, 3, 4);
        Direct3D9BitmapTexturePopulator populator = new(
            (out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                calls.Add("create");
                surface = null;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (nint _, ReadOnlySpan<Direct3D9BitmapRealizationRectangle> _, Direct3D9SystemMemoryUpdateSurface _, bool _) =>
            {
                calls.Add("push");
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                calls.Add("mipmaps");
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _) => calls.Add("commit"));

        int result = populator.Populate(5, 1, (nint) (&dirtyRectangle), 7, default);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "create", 0),
            (result, string.Join(',', calls), _releaseCount));
    }

    [TestMethod]
    public void WhenLevelZeroPushFailsThenTemporaryStagingSurfaceIsReleasedAndLaterWorkIsSkipped()
    {
        using FakeSurfaceObject surfaceObject = new();
        List<string> calls = [];
        Direct3D9BitmapRealizationRectangle dirtyRectangle = new(1, 2, 3, 4);
        Direct3D9BitmapTexturePopulator populator = new(
            (out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                calls.Add("create");
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, ReadOnlySpan<Direct3D9BitmapRealizationRectangle> _, Direct3D9SystemMemoryUpdateSurface _, bool _) =>
            {
                calls.Add("push");
                return Direct3D9Factory.GenericFailureHResult;
            },
            () =>
            {
                calls.Add("mipmaps");
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _) => calls.Add("commit"));

        int result = populator.Populate(5, 1, (nint) (&dirtyRectangle), 7, default);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "create,push", 1),
            (result, string.Join(',', calls), _releaseCount));
    }

    [TestMethod]
    public void WhenStagingSurfaceCreationFailsAfterReturningSurfaceThenSurfaceIsReleasedAndLaterWorkIsSkipped()
    {
        using FakeSurfaceObject surfaceObject = new();
        List<string> calls = [];
        Direct3D9BitmapRealizationRectangle dirtyRectangle = new(1, 2, 3, 4);
        Direct3D9BitmapTexturePopulator populator = new(
            (out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                calls.Add("create");
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.GenericFailureHResult;
            },
            (nint _, ReadOnlySpan<Direct3D9BitmapRealizationRectangle> _, Direct3D9SystemMemoryUpdateSurface _, bool _) =>
            {
                calls.Add("push");
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                calls.Add("mipmaps");
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _) => calls.Add("commit"));

        int result = populator.Populate(5, 1, (nint) (&dirtyRectangle), 7, default);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "create", 1),
            (result, string.Join(',', calls), _releaseCount));
    }

    [TestMethod]
    public void WhenMipmapUpdateFailsThenTemporaryStagingSurfaceIsReleasedAndCommitIsSkipped()
    {
        using FakeSurfaceObject surfaceObject = new();
        List<string> calls = [];
        Direct3D9BitmapRealizationRectangle dirtyRectangle = new(1, 2, 3, 4);
        Direct3D9BitmapTexturePopulator populator = new(
            (out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                calls.Add("create");
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, ReadOnlySpan<Direct3D9BitmapRealizationRectangle> _, Direct3D9SystemMemoryUpdateSurface _, bool _) =>
            {
                calls.Add("push");
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                calls.Add("mipmaps");
                return Direct3D9Factory.GenericFailureHResult;
            },
            (_, _) => calls.Add("commit"));

        int result = populator.Populate(5, 1, (nint) (&dirtyRectangle), 7, default);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "create,push,mipmaps", 1),
            (result, string.Join(',', calls), _releaseCount));
    }

    [TestMethod]
    public void WhenNoRegionsAreDirtyThenStagingSurfaceIsNotCreated()
    {
        List<string> calls = [];
        Direct3D9BitmapTexturePopulator populator = new(
            (out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                calls.Add("create");
                surface = null;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, ReadOnlySpan<Direct3D9BitmapRealizationRectangle> _, Direct3D9SystemMemoryUpdateSurface _, bool _) =>
                Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            (uniqueness, _) => calls.Add($"commit:{uniqueness}"));

        int result = populator.Populate(5, 0, 0, 7, default);

        Assert.AreEqual((0, "commit:7", 0), (result, string.Join(',', calls), _releaseCount));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void*** surface)
    {
        _releaseCount++;
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
