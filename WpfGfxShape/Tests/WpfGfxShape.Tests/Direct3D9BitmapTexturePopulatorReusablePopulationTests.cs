using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapTexturePopulatorReusablePopulationTests
{
    [TestMethod]
    public void WhenReusableSourceCoversPreparedPopulationThenBitmapPushIsSkippedButMipmapsAndCacheAreUpdated()
    {
        List<string> calls = [];
        using Direct3D9BitmapReusableRealizationSources sources = CreateReusableSources();
        Direct3D9BitmapReusableRealizationPopulation population = CreatePopulation(
            [new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20)],
            calls);
        using Direct3D9BitmapSystemMemorySurfaceSource surfaceSource = new(
            (uint _, uint _, Format _, void* _, out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                calls.Add("surface");
                surface = null;
                return Direct3D9Factory.GenericFailureHResult;
            },
            Format.A8R8G8B8);
        Direct3D9BitmapTexturePopulationPreparer preparer = new(
            surfaceSource,
            (out nint bitmapLock) =>
            {
                calls.Add("prepare");
                bitmapLock = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (nint _, out uint size, out nint data) =>
            {
                size = 0;
                data = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (nint _, out uint stride) =>
            {
                stride = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (nint _, out MilPixelFormat format) =>
            {
                format = default;
                return Direct3D9Factory.GenericFailureHResult;
            },
            _ => { },
            true,
            20,
            20,
            20);
        Direct3D9BitmapTexturePopulator populator = new(
            preparer,
            (nint _, ReadOnlySpan<Direct3D9BitmapRealizationRectangle> _, Direct3D9SystemMemoryUpdateSurface _, bool _) =>
            {
                calls.Add("push");
                return Direct3D9Factory.SuccessHResult;
            },
            sources,
            population,
            () =>
            {
                calls.Add("mipmaps");
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _) => calls.Add("commit"),
            _ => { });
        Direct3D9BitmapRealizationRectangle dirtyRectangle = new(0, 0, 20, 20);

        int result = populator.Populate(5, 1, (nint) (&dirtyRectangle), 7, dirtyRectangle);

        Assert.AreEqual((0, "stretch:0:0:20:20,mipmaps,commit"), (result, string.Join(',', calls)));
    }

    [TestMethod]
    public void WhenReusableSourcePartiallyCoversStagingPopulationThenOnlyRemainingRectangleIsPushed()
    {
        List<string> calls = [];
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9BitmapReusableRealizationSources sources = CreateReusableSources();
        Direct3D9BitmapReusableRealizationPopulation population = CreatePopulation(
            [new Direct3D9BitmapRealizationRectangle(0, 0, 10, 20)],
            calls);
        Direct3D9BitmapTexturePopulator populator = new(
            (out Direct3D9SystemMemoryUpdateSurface? surface) =>
            {
                calls.Add("create");
                surface = new Direct3D9SystemMemoryUpdateSurface(surfaceObject.Surface);
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, ReadOnlySpan<Direct3D9BitmapRealizationRectangle> rectangles, Direct3D9SystemMemoryUpdateSurface _, bool _) =>
            {
                Direct3D9BitmapRealizationRectangle rectangle = rectangles[0];
                calls.Add($"push:{rectangle.Left}:{rectangle.Top}:{rectangle.Right}:{rectangle.Bottom}");
                return Direct3D9Factory.SuccessHResult;
            },
            sources,
            population,
            () => Direct3D9Factory.SuccessHResult,
            (_, _) => { });
        Direct3D9BitmapRealizationRectangle dirtyRectangle = new(0, 0, 20, 20);

        int result = populator.Populate(5, 1, (nint) (&dirtyRectangle), 7, dirtyRectangle);

        Assert.AreEqual(
            (0, "stretch:0:0:10:20,create,push:10:0:20:20"),
            (result, string.Join(',', calls)));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void*** surface) => 0;

    private static Direct3D9BitmapReusableRealizationSources CreateReusableSources()
    {
        Direct3D9BitmapReusableRealizationSources sources = new(
            new Direct3D9BitmapReusableRealizationTargetState(
                0,
                true,
                true,
                20,
                20,
                new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20),
                1),
            (nint _, out Direct3D9BitmapReusableRealizationSourceState state) =>
            {
                state = new(
                    0,
                    true,
                    true,
                    20,
                    20,
                    new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20),
                    0,
                    false,
                    false);
                return true;
            },
            _ => 0,
            (_, _) => { },
            _ => { },
            _ => { },
            _ => { });
        sources.SetSources(1);
        return sources;
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

    private static Direct3D9BitmapReusableRealizationPopulation CreatePopulation(
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> validRectangles,
        List<string> calls)
    {
        Direct3D9BitmapReusableRealizationUpdater updater = new(
            new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20),
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                rectangles = validRectangles;
                return Direct3D9Factory.SuccessHResult;
            },
            _ => Direct3D9Factory.SuccessHResult,
            (nint _, out nint surface) =>
            {
                surface = 11;
                return Direct3D9Factory.SuccessHResult;
            },
            () => (Direct3D9Factory.SuccessHResult, (nint) 22),
            (_, sourceRectangle, _, _) =>
            {
                calls.Add($"stretch:{sourceRectangle.Left}:{sourceRectangle.Top}:{sourceRectangle.Right}:{sourceRectangle.Bottom}");
                return Direct3D9Factory.SuccessHResult;
            },
            _ => { });

        return new(
            updater,
            _ => 0,
            (nint _, out Direct3D9BitmapRealizationRectangle rectangle) =>
            {
                rectangle = new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20);
                return true;
            });
    }
}
