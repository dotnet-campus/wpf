using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapColorSourceChooserTests
{
    [TestMethod]
    public void WhenCachedSourceMatchesThenItIsReturnedAndValidDeviceBitmapIsPrependedForReuse()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapFormatCacheEntry entries = harness.CreateEntries();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        entries.Store(properties, 41);
        lifetime.CacheDeviceBitmapColorSource(31);
        harness.ValidSources.UnionWith([31, 41]);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceChooser.Choose(
            entries,
            lifetime,
            17,
            ref properties,
            CreateContext(),
            candidates,
            harness.IsValid,
            harness.IsRenderTarget,
            false,
            harness.UnexpectedCreate,
            out nint colorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|41|31|41|valid:41,add:41,valid:31,set-next:31:0,add:31,add:41",
            $"{result}|{colorSource}|{candidates.Head}|{lifetime.LastUsedColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenBitmapSourceAssociationChangesThenCacheIsClearedBeforeCacheSearch()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapFormatCacheEntry entries = harness.CreateEntries();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        entries.Store(properties, 41);
        lifetime.AssociateBitmapSource(17);
        harness.ValidSources.Add(41);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceChooser.Choose(
            entries,
            lifetime,
            19,
            ref properties,
            CreateContext(),
            candidates,
            harness.IsValid,
            harness.IsRenderTarget,
            false,
            harness.UnexpectedCreate,
            out nint colorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|41|19|clear,valid:41,add:41,add:41",
            $"{result}|{colorSource}|{lifetime.BitmapSourceNoReference}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenNoCachedSourceMatchesThenReusableHeadDeterminesRenderTargetCreationAndResultIsStored()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapFormatCacheEntry entries = harness.CreateEntries();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapRealizationProperties cached = CreateProperties();
        Direct3D9BitmapRealizationProperties requested = cached with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };
        entries.Store(cached, 41);
        lifetime.CacheDeviceBitmapColorSource(31);
        harness.ValidSources.Add(31);
        harness.RenderTargets.Add(31);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceChooser.Choose(
            entries,
            lifetime,
            17,
            ref requested,
            CreateContext(),
            candidates,
            harness.IsValid,
            harness.IsRenderTarget,
            false,
            (bool createAsRenderTarget, out nint colorSource) =>
            {
                harness.Record($"create:{createAsRenderTarget}");
                colorSource = 51;
                return Direct3D9Factory.SuccessHResult;
            },
            out nint colorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|51|31|51|set-next:41:0,add:41,valid:31,set-next:31:41,add:31,render-target:31,create:True,add:51,add:51",
            $"{result}|{colorSource}|{candidates.Head}|{lifetime.LastUsedColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenRequestedLayoutHasBorderThenDeviceBitmapIsNotAddedAndCreationIsNotRenderTarget()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapFormatCacheEntry entries = harness.CreateEntries();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with
        {
            LayoutV = CreateLayout(Direct3D9TexelLayout.EdgeWrapped)
        };
        lifetime.CacheDeviceBitmapColorSource(31);
        harness.ValidSources.Add(31);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceChooser.Choose(
            entries,
            lifetime,
            17,
            ref properties,
            CreateContext(),
            candidates,
            harness.IsValid,
            harness.IsRenderTarget,
            true,
            (bool createAsRenderTarget, out nint colorSource) =>
            {
                harness.Record($"create:{createAsRenderTarget}");
                colorSource = 51;
                return Direct3D9Factory.SuccessHResult;
            },
            out _);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|0|create:False,add:51,add:51",
            $"{result}|{candidates.Head}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenCreationFailsThenFirstFailureIsReturnedAndReusableCandidatesRemainOwnedByCaller()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapFormatCacheEntry entries = harness.CreateEntries();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapRealizationProperties cached = CreateProperties();
        Direct3D9BitmapRealizationProperties requested = cached with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };
        entries.Store(cached, 41);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceChooser.Choose(
            entries,
            lifetime,
            17,
            ref requested,
            CreateContext(),
            candidates,
            harness.IsValid,
            harness.IsRenderTarget,
            false,
            (bool createAsRenderTarget, out nint colorSource) =>
            {
                harness.Record($"create:{createAsRenderTarget}");
                colorSource = 0;
                return Direct3D9Factory.NotImplementedHResult;
            },
            out nint colorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.NotImplementedHResult}|0|41|0|set-next:41:0,add:41,render-target:41,create:False",
            $"{result}|{colorSource}|{candidates.Head}|{lifetime.LastUsedColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenPrefilteringWithoutMipMappingChoosesDifferentSourceThenLastUsedReferenceIsCleared()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapFormatCacheEntry entries = harness.CreateEntries();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        entries.Store(properties, 41);
        lifetime.SetLastUsedColorSource(61, CreateContext());
        harness.ValidSources.Add(41);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceChooser.Choose(
            entries,
            lifetime,
            17,
            ref properties,
            CreateContext(prefilterEnabled: true),
            candidates,
            harness.IsValid,
            harness.IsRenderTarget,
            false,
            harness.UnexpectedCreate,
            out _);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|0|valid:41,add:41,release:61",
            $"{result}|{lifetime.LastUsedColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenSameLastUsedSourceIsChosenThenOnlyWrapModeIsUpdated()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapFormatCacheEntry entries = harness.CreateEntries();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        entries.Store(properties, 41);
        lifetime.SetLastUsedColorSource(41, CreateContext(wrapMode: MilBitmapWrapMode.Extend));
        harness.ValidSources.Add(41);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceChooser.Choose(
            entries,
            lifetime,
            17,
            ref properties,
            CreateContext(wrapMode: MilBitmapWrapMode.Tile),
            candidates,
            harness.IsValid,
            harness.IsRenderTarget,
            false,
            harness.UnexpectedCreate,
            out _);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|Tile|valid:41,add:41",
            $"{result}|{lifetime.LastUsedContextParameters.WrapMode}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenDeviceBitmapValidAreaAndWrapCapabilitiesMatchThenWrapModesAreSetBeforeAcquire()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        lifetime.CacheDeviceBitmapColorSource(31);
        harness.ClearCalls();

        bool found = Direct3D9BitmapColorSourceChooser.TryChooseDeviceBitmap(
            lifetime,
            CreateContext(wrapMode: MilBitmapWrapMode.Extend),
            new Direct3D9DelayedBounds(),
            CreateProperties(),
            true,
            (Direct3D9DelayedBounds bounds, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle requiredBounds) =>
            {
                harness.Record($"minimum:{ReferenceEquals(bounds, null)}");
                requiredBounds = new(2, 3, 29, 17);
                return true;
            },
            (bitmap, requiredBounds) =>
            {
                harness.Record($"valid-area:{bitmap}:{requiredBounds}");
                return true;
            },
            (nint bitmap, out uint width, out uint height) =>
            {
                harness.Record($"size:{bitmap}");
                width = 63;
                height = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            supportsConditionalNonPowerOfTwoTextures: true,
            supportsUnconditionalNonPowerOfTwoTextures: false,
            (source, addressU, addressV) => harness.Record($"wrap:{source}:{addressU}:{addressV}"),
            (_, _, _, _, _) => throw new AssertFailedException(),
            out nint colorSource);

        Assert.AreEqual(
            "True|31|minimum:False,valid-area:11:Direct3D9BitmapRealizationRectangle { Left = 2, Top = 3, Right = 29, Bottom = 17 },size:11,wrap:31:TaddressClamp:TaddressClamp,add:31",
            $"{found}|{colorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenDeviceBitmapIsNonPowerOfTwoAndWrapRequiresTilingThenItIsNotAcquired()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        lifetime.CacheDeviceBitmapColorSource(31);
        harness.ClearCalls();

        bool found = Direct3D9BitmapColorSourceChooser.TryChooseDeviceBitmap(
            lifetime,
            CreateContext(wrapMode: MilBitmapWrapMode.Tile),
            new Direct3D9DelayedBounds(),
            CreateProperties(),
            true,
            (Direct3D9DelayedBounds _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle _) => true,
            (_, _) => true,
            (nint _, out uint width, out uint height) =>
            {
                width = 63;
                height = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            supportsConditionalNonPowerOfTwoTextures: true,
            supportsUnconditionalNonPowerOfTwoTextures: false,
            (_, _, _) => harness.Record("wrap"),
            (_, _, _, _, _) => throw new AssertFailedException(),
            out nint colorSource);

        Assert.AreEqual("False|0|", $"{found}|{colorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenCachedDeviceBitmapSourceContainsRequiredBoundsThenItIsAcquiredAfterTheCheck()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        lifetime.CacheDeviceBitmapColorSource(31);
        harness.ClearCalls();

        bool found = Direct3D9BitmapColorSourceChooser.TryChooseCachedDeviceBitmap(
            lifetime,
            CreateContext(wrapMode: MilBitmapWrapMode.Tile),
            new Direct3D9DelayedBounds(),
            false,
            (source, _, interpolationMode, wrapMode, check) =>
            {
                harness.Record($"bounds:{source}:{interpolationMode}:{wrapMode}:{check}");
                return true;
            },
            out nint colorSource);

        Assert.AreEqual(
            "True|31|bounds:31:Linear:Tile:Cached,add:31",
            $"{found}|{colorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenCachedDeviceBitmapSourceDoesNotContainRequiredBoundsThenItIsNotAcquired()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        lifetime.CacheDeviceBitmapColorSource(31);
        harness.ClearCalls();

        bool found = Direct3D9BitmapColorSourceChooser.TryChooseCachedDeviceBitmap(
            lifetime,
            CreateContext(),
            new Direct3D9DelayedBounds(),
            false,
            (source, _, _, _, check) =>
            {
                harness.Record($"bounds:{source}:{check}");
                return false;
            },
            out nint colorSource);

        Assert.AreEqual(
            "False|0|bounds:31:Cached",
            $"{found}|{colorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenBitmapIsDeviceBitmapThenCachedBoundsFallbackIsSkipped()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        lifetime.CacheDeviceBitmapColorSource(31);
        harness.ClearCalls();

        bool found = Direct3D9BitmapColorSourceChooser.TryChooseCachedDeviceBitmap(
            lifetime,
            CreateContext(),
            new Direct3D9DelayedBounds(),
            true,
            (_, _, _, _, _) =>
            {
                harness.Record("bounds");
                return true;
            },
            out nint colorSource);

        Assert.AreEqual("False|0|", $"{found}|{colorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenLastUsedSourceMatchesThenRequiredBoundsAreCheckedBeforeReferencesAreAcquired()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapColorSourceContextParameters context = CreateContext() with
        {
            BitmapBrush = 71,
            BitmapBrushUniqueness = 3
        };
        lifetime.SetLastUsedColorSource(41, context);
        lifetime.CacheDeviceBitmapColorSource(31);
        harness.ValidSources.Add(31);
        harness.ClearCalls();

        bool found = Direct3D9BitmapColorSourceChooser.TryChooseLastUsed(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            false,
            candidates,
            harness.IsValid,
            (source, _, interpolationMode, wrapMode, check) =>
            {
                harness.Record($"bounds:{source}:{interpolationMode}:{wrapMode}:{check}");
                return true;
            },
            out nint colorSource);

        Assert.AreEqual(
            "True|41|31|bounds:41:Linear:Extend:Required,add:41,valid:31,set-next:31:0,add:31",
            $"{found}|{colorSource}|{candidates.Head}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenLastUsedSourceIsForDeviceBitmapThenPossibleBoundsAreCheckedAndUpdated()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapColorSourceContextParameters context = CreateContext();
        lifetime.SetLastUsedColorSource(41, context);
        harness.ClearCalls();

        bool found = Direct3D9BitmapColorSourceChooser.TryChooseLastUsed(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            true,
            candidates,
            harness.IsValid,
            (source, _, _, _, check) =>
            {
                harness.Record($"bounds:{source}:{check}");
                return true;
            },
            out nint colorSource);

        Assert.AreEqual(
            "True|41|bounds:41:PossibleAndUpdateRequired,add:41",
            $"{found}|{colorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenLastUsedContextDoesNotMatchThenBoundsAreNotComputed()
    {
        ChooserHarness harness = new();
        using Direct3D9BitmapCacheLifetime lifetime = harness.CreateLifetime();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        Direct3D9BitmapColorSourceContextParameters previousContext = CreateContext() with
        {
            BitmapBrushUniqueness = 3
        };
        lifetime.SetLastUsedColorSource(41, previousContext);
        harness.ClearCalls();

        bool found = Direct3D9BitmapColorSourceChooser.TryChooseLastUsed(
            lifetime,
            previousContext with { BitmapBrushUniqueness = 5 },
            new Direct3D9DelayedBounds(),
            false,
            candidates,
            harness.IsValid,
            (_, _, _, _, _) =>
            {
                harness.Record("bounds");
                return true;
            },
            out nint colorSource);

        Assert.AreEqual("False|0|", $"{found}|{colorSource}|{harness.Calls}");
    }

    private static Direct3D9BitmapColorSourceContextParameters CreateContext(
        bool prefilterEnabled = false,
        MilBitmapWrapMode wrapMode = MilBitmapWrapMode.Extend) =>
        Direct3D9BitmapColorSourceContextParameters.FromExplicitSettings(
            MilBitmapInterpolationMode.Linear,
            prefilterEnabled,
            MilPixelFormat.Pbgra32Bpp,
            wrapMode);

    private static Direct3D9BitmapRealizationProperties CreateProperties() =>
        new(
            MilBitmapInterpolationMode.Linear,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            MilPixelFormat.Pbgra32Bpp,
            true,
            64,
            32,
            64,
            32)
        {
            SourceContained = new(0, 0, 64, 32),
            LayoutU = CreateLayout(Direct3D9TexelLayout.Natural),
            LayoutV = CreateLayout(Direct3D9TexelLayout.Natural)
        };

    private static Direct3D9BitmapDimensionLayout CreateLayout(Direct3D9TexelLayout layout) =>
        new(64, layout, Textureaddress.Clamp);

    private sealed class ChooserHarness
    {
        private readonly Dictionary<nint, nint> _next = [];
        private readonly List<string> _calls = [];
        private readonly Direct3D9ResourceManager _manager = new();

        internal HashSet<nint> ValidSources { get; } = [];

        internal HashSet<nint> RenderTargets { get; } = [];

        internal string Calls => string.Join(',', _calls);

        internal Direct3D9BitmapFormatCacheEntry CreateEntries() => new(AddReference, Release);

        internal Direct3D9BitmapCacheLifetime CreateLifetime() =>
            new(_manager, 11, 13, () => Record("clear"), AddReference, Release);

        internal Direct3D9BitmapReusableRealizationCandidates CreateCandidates() =>
            new(GetNext, SetNext, AddReference, Release);

        internal bool IsValid(nint source)
        {
            Record($"valid:{source}");
            return ValidSources.Contains(source);
        }

        internal bool IsRenderTarget(nint source)
        {
            Record($"render-target:{source}");
            return RenderTargets.Contains(source);
        }

        internal int UnexpectedCreate(bool createAsRenderTarget, out nint colorSource)
        {
            colorSource = 0;
            Assert.Fail($"Unexpected color-source creation request: {createAsRenderTarget}.");
            return Direct3D9Factory.GenericFailureHResult;
        }

        internal void Record(string call) => _calls.Add(call);

        internal void ClearCalls() => _calls.Clear();

        private nint GetNext(nint source) => _next.GetValueOrDefault(source);

        private void SetNext(nint source, nint next)
        {
            _next[source] = next;
            Record($"set-next:{source}:{next}");
        }

        private void AddReference(nint source) => Record($"add:{source}");

        private void Release(nint source) => Record($"release:{source}");
    }
}
