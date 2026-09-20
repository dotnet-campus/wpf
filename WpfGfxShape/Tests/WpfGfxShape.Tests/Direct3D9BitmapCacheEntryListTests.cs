using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapCacheEntryListTests
{
    [TestMethod]
    public void WhenEntryIsAddedThenCacheOwnsOneReference()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);

        int index = entries.Add(31);

        Assert.AreEqual("0|1|31|add:31", $"{index}|{entries.Count}|{entries.GetColorSourceNoReference(index)}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenNaturalFullSourceHasMatchingSizeThenItIsReusableAcrossLayouts()
    {
        Direct3D9BitmapRealizationProperties cached = CreateProperties();
        Direct3D9BitmapRealizationProperties requested = CreateProperties() with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };

        Direct3D9BitmapSizeLayoutMatch match = Direct3D9BitmapCacheEntryList.CheckSizeLayoutMatch(cached, requested);

        Assert.AreEqual(Direct3D9BitmapSizeLayoutMatch.ReusableSource, match);
    }

    [TestMethod]
    public void WhenNaturalFullSourceHasExactLayoutThenItMeetsAllRequirements()
    {
        Direct3D9BitmapSizeLayoutMatch match = Direct3D9BitmapCacheEntryList.CheckSizeLayoutMatch(
            CreateProperties(),
            CreateProperties());

        Assert.AreEqual(Direct3D9BitmapSizeLayoutMatch.MeetsAllRequirements, match);
    }

    [TestMethod]
    public void WhenSizeDiffersThenThereIsNoMatch()
    {
        Direct3D9BitmapRealizationProperties requested = CreateProperties() with { Width = 41 };

        Direct3D9BitmapSizeLayoutMatch match = Direct3D9BitmapCacheEntryList.CheckSizeLayoutMatch(
            CreateProperties(),
            requested);

        Assert.AreEqual(Direct3D9BitmapSizeLayoutMatch.NoMatch, match);
    }

    [TestMethod]
    public void WhenEitherLayoutHasBorderThenItCannotBeAReusableSource()
    {
        Direct3D9BitmapRealizationProperties cached = CreateProperties() with
        {
            LayoutV = CreateLayout(Direct3D9TexelLayout.EdgeWrapped)
        };
        Direct3D9BitmapRealizationProperties requested = CreateProperties() with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.EdgeMirrored)
        };

        Direct3D9BitmapSizeLayoutMatch match = Direct3D9BitmapCacheEntryList.CheckSizeLayoutMatch(cached, requested);

        Assert.AreEqual(Direct3D9BitmapSizeLayoutMatch.NoMatch, match);
    }

    [TestMethod]
    public void WhenCachedMipMapLevelIsLowerThenOnlyReusableSourceMatchRemains()
    {
        Direct3D9BitmapRealizationProperties cached = CreateProperties();
        Direct3D9BitmapRealizationProperties requested = CreateProperties() with
        {
            MipMapLevel = Direct3D9TextureMipMapLevel.All
        };

        Direct3D9BitmapSizeLayoutMatch match = Direct3D9BitmapCacheEntryList.CheckSizeLayoutMatch(cached, requested);

        Assert.AreEqual(Direct3D9BitmapSizeLayoutMatch.ReusableSource, match);
    }

    [TestMethod]
    public void WhenCachedSubRectangleContainsRequestedRectangleThenItMeetsAllRequirements()
    {
        Direct3D9BitmapRealizationProperties cached = CreateSubRectangleProperties(new(5, 7, 35, 27));
        Direct3D9BitmapRealizationProperties requested = CreateSubRectangleProperties(new(10, 9, 30, 20));

        Direct3D9BitmapSizeLayoutMatch match = Direct3D9BitmapCacheEntryList.CheckSizeLayoutMatch(cached, requested);

        Assert.AreEqual(Direct3D9BitmapSizeLayoutMatch.MeetsAllRequirements, match);
    }

    [TestMethod]
    public void WhenCachedSubRectangleOnlyOverlapsRequestedRectangleThenMatchIsPartial()
    {
        Direct3D9BitmapRealizationProperties cached = CreateSubRectangleProperties(new(5, 7, 20, 27));
        Direct3D9BitmapRealizationProperties requested = CreateSubRectangleProperties(new(10, 9, 30, 20));

        Direct3D9BitmapSizeLayoutMatch match = Direct3D9BitmapCacheEntryList.CheckSizeLayoutMatch(cached, requested);

        Assert.AreEqual(Direct3D9BitmapSizeLayoutMatch.PartialOverlap, match);
    }

    [TestMethod]
    public void WhenSubRectanglesOnlyTouchAtAnEdgeThenThereIsNoMatch()
    {
        Direct3D9BitmapRealizationProperties cached = CreateSubRectangleProperties(new(0, 0, 10, 10));
        Direct3D9BitmapRealizationProperties requested = CreateSubRectangleProperties(new(10, 0, 20, 10));

        Direct3D9BitmapSizeLayoutMatch match = Direct3D9BitmapCacheEntryList.CheckSizeLayoutMatch(cached, requested);

        Assert.AreEqual(Direct3D9BitmapSizeLayoutMatch.NoMatch, match);
    }

    [TestMethod]
    public void WhenEntryIsReplacedThenOldReferenceIsReleasedBeforeNewReferenceIsAdded()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);
        int index = entries.Add(31);
        calls.Clear();

        entries.Replace(index, 37);

        Assert.AreEqual("37|release:31|add:37", $"{entries.GetColorSourceNoReference(index)}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenValidEntryIsAcquiredThenCallerReceivesAnAdditionalReference()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);
        int index = entries.Add(31);
        calls.Clear();

        bool acquired = entries.TryAcquireValid(index, value => value == 31, out nint colorSource);

        Assert.AreEqual("True|31|31|add:31", $"{acquired}|{colorSource}|{entries.GetColorSourceNoReference(index)}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenMatchedEntryIsInvalidThenCachedReferenceIsReleasedAndCleared()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);
        int index = entries.Add(31);
        calls.Clear();

        bool acquired = entries.TryAcquireValid(index, _ => false, out nint colorSource);

        Assert.AreEqual("False|0|0|release:31", $"{acquired}|{colorSource}|{entries.GetColorSourceNoReference(index)}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenLaterEntriesAreRemovedThenEachReferenceIsReleasedAndLastEntryFillsTheGap()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);
        entries.Add(31);
        entries.Add(37);
        entries.Add(41);
        entries.Add(43);
        calls.Clear();

        entries.RemoveEntriesAfter(0, (_, value) => value is 37 or 43);

        Assert.AreEqual("2|31|41|release:37|release:43", $"{entries.Count}|{entries.GetColorSourceNoReference(0)}|{entries.GetColorSourceNoReference(1)}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenReusableEntryPrecedesExactEntryThenCandidateIsReportedBeforeExactEntryIsAcquired()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);
        entries.Add(CreateProperties(), 31);
        Direct3D9BitmapRealizationProperties exact = CreateProperties() with
        {
            InterpolationMode = MilBitmapInterpolationMode.NearestNeighbor,
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };
        entries.Add(exact, 37);
        Dictionary<nint, nint> next = new() { [31] = 0, [37] = 0 };
        using Direct3D9BitmapReusableRealizationCandidates candidates = new(
            value => next[value],
            (value, nextValue) =>
            {
                next[value] = nextValue;
                calls.Add($"next:{value}:{nextValue}");
            },
            value => calls.Add($"candidate-add:{value}"),
            value => calls.Add($"candidate-release:{value}"));
        calls.Clear();
        Direct3D9BitmapRealizationProperties requested = exact with
        {
            InterpolationMode = MilBitmapInterpolationMode.Linear,
            LayoutU = exact.LayoutU with { TextureAddress = Textureaddress.Wrap }
        };

        bool acquired = entries.TryAcquire(
            ref requested,
            value =>
            {
                calls.Add($"valid:{value}");
                return true;
            },
            candidates,
            out nint colorSource);

        Assert.AreEqual(
            "True|37|31|NearestNeighbor|TaddressWrap|next:31:0|candidate-add:31|valid:37|add:37",
            $"{acquired}|{colorSource}|{candidates.Head}|{requested.InterpolationMode}|{requested.LayoutU.TextureAddress}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenExactEntryIsInvalidThenItsCachedReferenceIsReleased()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);
        entries.Add(CreateProperties(), 31);
        calls.Clear();
        Direct3D9BitmapRealizationProperties requested = CreateProperties();

        bool acquired = entries.TryAcquire(ref requested, _ => false, null, out nint colorSource);

        Assert.AreEqual(
            "False|0|0|release:31",
            $"{acquired}|{colorSource}|{entries.GetColorSourceNoReference(0)}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenPartialOverlapIsFoundThenPlaceholderIsUpdatedAndLaterOverlapsAreRemoved()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);
        entries.Add(CreateSubRectangleProperties(new(0, 0, 20, 20)), 31);
        entries.Add(CreateSubRectangleProperties(new(15, 0, 35, 20)), 37);
        entries.Add(CreateSubRectangleProperties(new(60, 0, 80, 20)), 41);
        calls.Clear();
        Direct3D9BitmapRealizationProperties requested = CreateSubRectangleProperties(new(10, 0, 30, 20));

        bool acquired = entries.TryAcquire(ref requested, _ => true, null, out _);

        Assert.AreEqual(
            "False|2|0|41|release:31|release:37",
            $"{acquired}|{entries.Count}|{entries.GetColorSourceNoReference(0)}|{entries.GetColorSourceNoReference(1)}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenStoredPropertiesMatchExistingEntryThenReferenceIsReplaced()
    {
        List<string> calls = [];
        using Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);
        entries.Store(CreateProperties(), 31);
        calls.Clear();

        int index = entries.Store(CreateProperties(), 37);

        Assert.AreEqual(
            "0|1|37|release:31|add:37",
            $"{index}|{entries.Count}|{entries.GetColorSourceNoReference(index)}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenDisposedThenEveryRemainingReferenceIsReleasedInEntryOrder()
    {
        List<string> calls = [];
        Direct3D9BitmapCacheEntryList entries = CreateEntries(calls);
        entries.Add(31);
        entries.Add(0);
        entries.Add(37);
        calls.Clear();

        entries.Dispose();
        entries.Dispose();

        Assert.AreEqual("release:31|release:37", string.Join('|', calls));
    }

    [TestMethod]
    public void WhenDisposedThenMembersAreProtected()
    {
        Direct3D9BitmapCacheEntryList entries = CreateEntries([]);
        entries.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = entries.Count);
        Assert.ThrowsExactly<ObjectDisposedException>(() => entries.Add(31));
        Assert.ThrowsExactly<ObjectDisposedException>(() => entries.Replace(0, 31));
        Assert.ThrowsExactly<ObjectDisposedException>(() => entries.TryAcquireValid(0, _ => true, out _));
        Assert.ThrowsExactly<ObjectDisposedException>(() => entries.RemoveEntriesAfter(0, (_, _) => true));
        Assert.ThrowsExactly<ObjectDisposedException>(() => entries.GetColorSourceNoReference(0));
    }

    private static Direct3D9BitmapRealizationProperties CreateProperties() =>
        new(
            MilBitmapInterpolationMode.Linear,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            MilPixelFormat.Pbgra32Bpp,
            true,
            100,
            80,
            40,
            30)
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.Natural),
            LayoutV = CreateLayout(Direct3D9TexelLayout.Natural),
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 100, 80)
        };

    private static Direct3D9BitmapRealizationProperties CreateSubRectangleProperties(
        Direct3D9BitmapRealizationRectangle sourceContained) =>
        CreateProperties() with
        {
            OnlyContainsSubRectangleOfSource = true,
            SourceContained = sourceContained
        };

    private static Direct3D9BitmapDimensionLayout CreateLayout(Direct3D9TexelLayout layout) =>
        new(40, layout, Textureaddress.Clamp);

    private static Direct3D9BitmapCacheEntryList CreateEntries(List<string> calls) =>
        new(
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
}
