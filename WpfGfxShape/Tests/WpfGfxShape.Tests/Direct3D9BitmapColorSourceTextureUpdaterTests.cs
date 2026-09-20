using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class Direct3D9BitmapColorSourceTextureUpdaterTests
{
    [TestMethod]
    public void WhenDirtyRectanglesAreValidThenComputedPrefilteredRectanglesArePopulatedAndCommitted()
    {
        List<string> calls = [];
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 12);
        state.SetRequiredRealizationBounds(new Direct3D9BitmapRealizationRectangle(5, 7, 45, 37));
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(5, 7, 45, 37));
        Direct3D9BitmapTexturePopulator populator = CreatePopulator(state, calls);
        Direct3D9BitmapColorSourceTextureUpdater updater = new(
            state,
            populator,
            (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles, out uint uniqueness) =>
            {
                calls.Add("dirty");
                dirtyRectangles = [new Direct3D9BitmapRealizationRectangle(11, 15, 19, 21)];
                uniqueness = 12;
                return true;
            });

        int result = updater.Update();

        Assert.AreEqual(
            (0, "dirty,prepare:23:1:5:7:10:11,push:23:1:5:7:10:11:0:True,mipmaps,commit:12:5:7:45:37,release:41", true),
            (result, string.Join(',', calls), state.IsRealizationValid()));
    }

    [TestMethod]
    public void WhenDirtyInformationIsInvalidThenEntireRequiredAreaIsPopulated()
    {
        List<string> calls = [];
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 12);
        Direct3D9BitmapRealizationRectangle required = new(10, 8, 30, 25);
        state.SetRequiredRealizationBounds(required);
        state.Commit(11, new Direct3D9BitmapRealizationRectangle(12, 9, 28, 24));
        Direct3D9BitmapColorSourceTextureUpdater updater = new(
            state,
            CreatePopulator(state, calls),
            (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles, out uint uniqueness) =>
            {
                dirtyRectangles = [];
                uniqueness = 12;
                return false;
            });

        int result = updater.Update();

        Assert.AreEqual(
            (0, "prepare:23:1:10:8:30:25,push:23:1:10:8:30:25:0:True,mipmaps,commit:12:10:8:30:25,release:41"),
            (result, string.Join(',', calls)));
    }

    [TestMethod]
    public void WhenPopulationFailsThenRealizationIsNotCommitted()
    {
        List<string> calls = [];
        Direct3D9BitmapColorSourceRealizationState state = CreateState(bitmap: 7, uniqueness: 12);
        Direct3D9BitmapRealizationRectangle required = new(10, 8, 30, 25);
        state.SetRequiredRealizationBounds(required);
        state.Commit(11, required);
        Direct3D9BitmapTexturePopulator populator = new(
            (nint _, uint _, nint _, out nint bitmapLock, out bool copySource, out nint surface) =>
            {
                calls.Add("prepare");
                bitmapLock = 41;
                copySource = true;
                surface = 42;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint _, nint _, nint _, bool _) =>
            {
                calls.Add("push");
                return Direct3D9Factory.GenericFailureHResult;
            },
            () =>
            {
                calls.Add("mipmaps");
                return Direct3D9Factory.SuccessHResult;
            },
            state,
            resource => calls.Add($"release:{resource}"));
        Direct3D9BitmapColorSourceTextureUpdater updater = new(
            state,
            populator,
            (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles, out uint uniqueness) =>
            {
                dirtyRectangles = [required];
                uniqueness = 12;
                return true;
            });

        int result = updater.Update();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "prepare,push,release:42,release:41", 11u),
            (result, string.Join(',', calls), state.CachedUniquenessToken));
    }

    private static Direct3D9BitmapTexturePopulator CreatePopulator(
        Direct3D9BitmapColorSourceRealizationState state,
        List<string> calls) =>
        new(
            (nint bitmapSource, uint count, nint rectangles, out nint bitmapLock, out bool copySource, out nint surface) =>
            {
                Direct3D9BitmapRealizationRectangle rectangle = *(Direct3D9BitmapRealizationRectangle*) rectangles;
                calls.Add($"prepare:{bitmapSource}:{count}:{rectangle.Left}:{rectangle.Top}:{rectangle.Right}:{rectangle.Bottom}");
                bitmapLock = 41;
                copySource = true;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint bitmapSource, uint count, nint rectangles, nint surface, bool copySource) =>
            {
                Direct3D9BitmapRealizationRectangle rectangle = *(Direct3D9BitmapRealizationRectangle*) rectangles;
                calls.Add($"push:{bitmapSource}:{count}:{rectangle.Left}:{rectangle.Top}:{rectangle.Right}:{rectangle.Bottom}:{surface}:{copySource}");
                return Direct3D9Factory.SuccessHResult;
            },
            () =>
            {
                calls.Add("mipmaps");
                return Direct3D9Factory.SuccessHResult;
            },
            (uniqueness, bounds) =>
            {
                state.Commit(uniqueness, bounds);
                calls.Add($"commit:{uniqueness}:{bounds.Left}:{bounds.Top}:{bounds.Right}:{bounds.Bottom}");
            },
            resource => calls.Add($"release:{resource}"));

    private static Direct3D9BitmapColorSourceRealizationState CreateState(nint bitmap, uint uniqueness)
    {
        Direct3D9BitmapColorSourceRealizationState state = new(
            bitmap,
            MilPixelFormat.Pbgra32Bpp,
            _ => uniqueness);
        state.SetBitmapAndContextCacheParameters(
            23,
            new Direct3D9BitmapRealizationProperties(
                MilBitmapInterpolationMode.Linear,
                Direct3D9TextureMipMapLevel.One,
                MilBitmapWrapMode.Extend,
                MilPixelFormat.Pbgra32Bpp,
                IsMinimumRealizationRectComputed: false,
                BitmapWidth: 80,
                BitmapHeight: 60,
                Width: 40,
                Height: 30)
            {
                SourceContained = new Direct3D9BitmapRealizationRectangle(5, 7, 45, 37),
                LayoutU = new Direct3D9BitmapDimensionLayout(40, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
                LayoutV = new Direct3D9BitmapDimensionLayout(30, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
            });
        return state;
    }
}
