using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapColorSourceTextureRealizationTests
{
    [TestMethod]
    public void WhenUpdatingProductionRealizationThenExistingUpdaterLifecycleIsUsed()
    {
        List<string> calls = [];
        Direct3D9BitmapColorSourceRealizationState state = CreateState();
        Direct3D9BitmapTexturePopulator populator = new(
            (nint _, uint count, nint _, out nint bitmapLock, out bool copySource, out nint surface) =>
            {
                calls.Add($"prepare:{count}");
                bitmapLock = 0;
                copySource = false;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint count, nint _, nint _, bool _) =>
            {
                calls.Add($"push:{count}");
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                calls.Add("mipmaps");
                return Direct3D9Factory.SuccessHResult;
            },
            state,
            _ => { });
        Direct3D9BitmapColorSourceTextureUpdater updater = new(
            state,
            populator,
            (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles, out uint uniqueness) =>
            {
                calls.Add("dirty");
                dirtyRectangles = [];
                uniqueness = 9;
                return false;
            });
        using TrackingDisposable surfaceSource = new(calls, "surface");
        using TrackingDisposable texture = new(calls, "texture");
        using Direct3D9BitmapColorSourceTextureRealization realization = new(updater, surfaceSource, texture);

        int result = realization.Update();

        Assert.AreEqual(
            (0, "dirty,prepare:1,push:1,mipmaps", 9u),
            (result, string.Join(',', calls), state.CachedUniquenessToken));
    }

    [TestMethod]
    public void WhenProductionRealizationIsDisposedThenOwnedResourcesAreReleasedInNativeOrder()
    {
        List<string> calls = [];
        Direct3D9BitmapColorSourceRealizationState state = CreateState();
        Direct3D9BitmapTexturePopulator populator = new(
            (nint _, uint _, nint _, out nint bitmapLock, out bool copySource, out nint surface) =>
            {
                bitmapLock = 0;
                copySource = false;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint _, nint _, nint _, bool _) => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            state,
            _ => { });
        Direct3D9BitmapColorSourceTextureUpdater updater = new(
            state,
            populator,
            (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles, out uint uniqueness) =>
            {
                dirtyRectangles = [];
                uniqueness = 0;
                return true;
            });
        TrackingDisposable surfaceSource = new(calls, "surface");
        TrackingDisposable texture = new(calls, "texture");
        Direct3D9BitmapColorSourceTextureRealization realization = new(updater, surfaceSource, texture);

        realization.Dispose();
        realization.Dispose();

        Assert.AreEqual("surface,texture", string.Join(',', calls));
    }

    [TestMethod]
    public void WhenRealizationPassCompletesThenTemporarySourcesAreReleasedForEachPass()
    {
        int releaseCount = 0;
        Direct3D9BitmapColorSourceRealizationState state = CreateState();
        Direct3D9BitmapTexturePopulator populator = new(
            (nint _, uint _, nint _, out nint bitmapLock, out bool copySource, out nint surface) =>
            {
                bitmapLock = 0;
                copySource = false;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint _, nint _, nint _, bool _) => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            state,
            _ => { });
        using Direct3D9BitmapColorSourceTextureRealization realization = new(
            new Direct3D9BitmapColorSourceTextureUpdater(
                state,
                populator,
                (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles, out uint uniqueness) =>
                {
                    dirtyRectangles = [];
                    uniqueness = 0;
                    return true;
                }),
            new TrackingDisposable([], "surface"),
            new TrackingDisposable([], "texture"),
            releaseRealizationSources: () => releaseCount++);

        realization.ReleaseRealizationSources();
        realization.ReleaseRealizationSources();

        Assert.AreEqual(2, releaseCount);
    }

    [TestMethod]
    public void WhenProductionRealizationWasDisposedThenUpdateIsRejected()
    {
        Direct3D9BitmapColorSourceRealizationState state = CreateState();
        Direct3D9BitmapTexturePopulator populator = new(
            (nint _, uint _, nint _, out nint bitmapLock, out bool copySource, out nint surface) =>
            {
                bitmapLock = 0;
                copySource = false;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint _, nint _, nint _, bool _) => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            state,
            _ => { });
        Direct3D9BitmapColorSourceTextureRealization realization = new(
            new Direct3D9BitmapColorSourceTextureUpdater(
                state,
                populator,
                (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles, out uint uniqueness) =>
                {
                    dirtyRectangles = [];
                    uniqueness = 0;
                    return true;
                }),
            new TrackingDisposable([], "surface"),
            new TrackingDisposable([], "texture"));
        realization.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => realization.Update());
    }

    private static Direct3D9BitmapColorSourceRealizationState CreateState()
    {
        Direct3D9BitmapColorSourceRealizationState state = new(
            0,
            MilPixelFormat.Pbgra32Bpp,
            _ => 0);
        state.SetBitmapAndContextCacheParameters(
            23,
            new Direct3D9BitmapRealizationProperties(
                MilBitmapInterpolationMode.NearestNeighbor,
                Direct3D9TextureMipMapLevel.One,
                MilBitmapWrapMode.Extend,
                MilPixelFormat.Pbgra32Bpp,
                IsMinimumRealizationRectComputed: false,
                BitmapWidth: 40,
                BitmapHeight: 30,
                Width: 40,
                Height: 30)
            {
                SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30)
            });
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(10, 8, 30, 25));
        return state;
    }

    private sealed class TrackingDisposable(List<string> calls, string name) : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            calls.Add(name);
            _isDisposed = true;
        }
    }
}
