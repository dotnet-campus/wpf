using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapColorSourceCacheCoordinatorTests
{
    [TestMethod]
    public void WhenBitmapCacheIsProvidedThenItIsRetainedChosenAndReleased()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapColorSourceCacheCoordinator.Get(
            11,
            value => calls.Add($"add:{value}"),
            UnexpectedResolve,
            (nint cache, out nint colorSource, out nint reusableSource) =>
            {
                calls.Add($"choose:{cache}");
                colorSource = 21;
                reusableSource = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            UnexpectedCreate,
            value => calls.Add($"release:{value}"),
            out nint bitmapColorSource,
            out nint reusableRealizationSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|21|31|add:11|choose:11|release:11",
            $"{result}|{bitmapColorSource}|{reusableRealizationSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenBitmapCacheIsResolvedThenTransferredReferenceIsReleasedAfterChoose()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapColorSourceCacheCoordinator.Get(
            0,
            _ => Assert.Fail(),
            (out nint cache) =>
            {
                calls.Add("resolve");
                cache = 12;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint cache, out nint colorSource, out nint reusableSource) =>
            {
                calls.Add($"choose:{cache}");
                colorSource = 22;
                reusableSource = 32;
                return Direct3D9Factory.SuccessHResult;
            },
            UnexpectedCreate,
            value => calls.Add($"release:{value}"),
            out nint bitmapColorSource,
            out nint reusableRealizationSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|22|32|resolve|choose:12|release:12",
            $"{result}|{bitmapColorSource}|{reusableRealizationSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenCacheResolutionFailsThenUncachedColorSourceIsCreated()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapColorSourceCacheCoordinator.Get(
            0,
            _ => Assert.Fail(),
            (out nint cache) =>
            {
                calls.Add("resolve");
                cache = 0;
                return Direct3D9Factory.NotImplementedHResult;
            },
            UnexpectedChoose,
            (out nint colorSource) =>
            {
                calls.Add("create");
                colorSource = 23;
                return Direct3D9Factory.SuccessHResult;
            },
            value => calls.Add($"release:{value}"),
            out nint bitmapColorSource,
            out nint reusableRealizationSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|23|0|resolve|create",
            $"{result}|{bitmapColorSource}|{reusableRealizationSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenCacheResolutionFailsWithReferenceThenReferenceIsReleasedAfterFallbackCreation()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapColorSourceCacheCoordinator.Get(
            0,
            _ => Assert.Fail(),
            (out nint cache) =>
            {
                calls.Add("resolve");
                cache = 12;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            UnexpectedChoose,
            (out nint colorSource) =>
            {
                calls.Add("create");
                colorSource = 23;
                return Direct3D9Factory.SuccessHResult;
            },
            value => calls.Add($"release:{value}"),
            out nint bitmapColorSource,
            out _);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|23|resolve|create|release:12",
            $"{result}|{bitmapColorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenCachedChooseFailsThenFailureAndOutputsArePreservedAndCacheIsReleased()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapColorSourceCacheCoordinator.Get(
            11,
            value => calls.Add($"add:{value}"),
            UnexpectedResolve,
            (nint cache, out nint colorSource, out nint reusableSource) =>
            {
                calls.Add($"choose:{cache}");
                colorSource = 21;
                reusableSource = 31;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            UnexpectedCreate,
            value => calls.Add($"release:{value}"),
            out nint bitmapColorSource,
            out nint reusableRealizationSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.OutOfMemoryHResult}|21|31|add:11|choose:11|release:11",
            $"{result}|{bitmapColorSource}|{reusableRealizationSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenFallbackCreationFailsThenCreationFailureIsReturned()
    {
        int result = Direct3D9BitmapColorSourceCacheCoordinator.Get(
            0,
            _ => Assert.Fail(),
            (out nint cache) =>
            {
                cache = 0;
                return Direct3D9Factory.NotImplementedHResult;
            },
            UnexpectedChoose,
            (out nint colorSource) =>
            {
                colorSource = 0;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            _ => Assert.Fail(),
            out nint bitmapColorSource,
            out nint reusableRealizationSource);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, 0, 0),
            (result, bitmapColorSource, reusableRealizationSource));
    }

    [TestMethod]
    public void WhenResolvedCacheIsMissingThenGenericFailureIsReturnedWithoutFallbackCreation()
    {
        int result = Direct3D9BitmapColorSourceCacheCoordinator.Get(
            0,
            _ => Assert.Fail(),
            (out nint cache) =>
            {
                cache = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            UnexpectedChoose,
            UnexpectedCreate,
            _ => Assert.Fail(),
            out nint bitmapColorSource,
            out nint reusableRealizationSource);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0),
            (result, bitmapColorSource, reusableRealizationSource));
    }

    [TestMethod]
    public void WhenCachedDeviceBitmapMatchesThenLastUsedLookupIsSkipped()
    {
        List<string> calls = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => calls.Add("clear"),
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        using Direct3D9BitmapReusableRealizationCandidates candidates = new(
            _ => 0,
            (_, _) => Assert.Fail(),
            _ => Assert.Fail(),
            _ => Assert.Fail());
        Direct3D9BitmapColorSourceContextParameters context = CreateContext();
        lifetime.CacheDeviceBitmapColorSource(31);
        lifetime.SetLastUsedColorSource(41, context);
        calls.Clear();

        bool found = Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            false,
            candidates,
            _ =>
            {
                calls.Add("valid");
                return true;
            },
            (source, _, _, _, check) =>
            {
                calls.Add($"bounds:{source}:{check}");
                return true;
            },
            out nint bitmapColorSource);

        Assert.AreEqual(
            "True|31|0|bounds:31:Cached|add:31",
            $"{found}|{bitmapColorSource}|{candidates.Head}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenCachedDeviceBitmapDoesNotMatchThenLastUsedLookupRuns()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => calls.Add("clear"),
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        using Direct3D9BitmapReusableRealizationCandidates candidates = new(
            source => next.GetValueOrDefault(source),
            (source, value) =>
            {
                calls.Add($"next:{source}:{value}");
                next[source] = value;
            },
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        Direct3D9BitmapColorSourceContextParameters context = CreateContext();
        lifetime.CacheDeviceBitmapColorSource(31);
        lifetime.SetLastUsedColorSource(41, context);
        calls.Clear();

        bool found = Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            false,
            candidates,
            source =>
            {
                calls.Add($"valid:{source}");
                return true;
            },
            (source, _, _, _, check) =>
            {
                calls.Add($"bounds:{source}:{check}");
                return check != Direct3D9BitmapRequiredBoundsCheck.Cached;
            },
            out nint bitmapColorSource);

        Assert.AreEqual(
            "True|41|31|bounds:31:Cached|bounds:41:Required|add:41|valid:31|next:31:0|add:31",
            $"{found}|{bitmapColorSource}|{candidates.Head}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenBitmapIsDeviceBitmapThenCachedFallbackIsSkippedBeforeLastUsedLookup()
    {
        List<string> calls = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => calls.Add("clear"),
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        using Direct3D9BitmapReusableRealizationCandidates candidates = new(
            _ => 0,
            (_, _) => Assert.Fail(),
            _ => Assert.Fail(),
            _ => Assert.Fail());
        Direct3D9BitmapColorSourceContextParameters context = CreateContext();
        lifetime.CacheDeviceBitmapColorSource(31);
        lifetime.SetLastUsedColorSource(41, context);
        calls.Clear();

        bool found = Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            true,
            candidates,
            _ => false,
            (source, _, _, _, check) =>
            {
                calls.Add($"bounds:{source}:{check}");
                return true;
            },
            out nint bitmapColorSource);

        Assert.AreEqual(
            "True|41|bounds:41:PossibleAndUpdateRequired|add:41",
            $"{found}|{bitmapColorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenDeviceBitmapMatchesThenWrapModesAreSetBeforeAcquireAndLastUsedIsSkipped()
    {
        List<string> calls = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => calls.Add("clear"),
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        using Direct3D9BitmapReusableRealizationCandidates candidates = new(
            _ => 0,
            (_, _) => Assert.Fail(),
            _ => Assert.Fail(),
            _ => Assert.Fail());
        Direct3D9BitmapColorSourceContextParameters context = CreateContext();
        lifetime.CacheDeviceBitmapColorSource(31);
        lifetime.SetLastUsedColorSource(41, context);
        calls.Clear();

        bool found = Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            CreateProperties(),
            true,
            candidates,
            (Direct3D9DelayedBounds _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle bounds) =>
            {
                bounds = new(2, 3, 29, 17);
                calls.Add("minimum");
                return true;
            },
            (bitmap, bounds) =>
            {
                calls.Add($"valid-area:{bitmap}:{bounds.Left}");
                return true;
            },
            (nint bitmap, out uint width, out uint height) =>
            {
                calls.Add($"size:{bitmap}");
                width = 63;
                height = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            supportsConditionalNonPowerOfTwoTextures: true,
            supportsUnconditionalNonPowerOfTwoTextures: false,
            (source, addressU, addressV) => calls.Add($"wrap:{source}:{addressU}:{addressV}"),
            _ =>
            {
                Assert.Fail();
                return false;
            },
            (_, _, _, _, _) =>
            {
                Assert.Fail();
                return false;
            },
            out nint bitmapColorSource);

        Assert.AreEqual(
            "True|31|minimum|valid-area:11:2|size:11|wrap:31:TaddressClamp:TaddressClamp|add:31",
            $"{found}|{bitmapColorSource}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenDeviceBitmapCannotTileNonPowerOfTwoTextureThenLastUsedLookupRunsWithoutUsingItAsPrimary()
    {
        List<string> calls = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => calls.Add("clear"),
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        using Direct3D9BitmapReusableRealizationCandidates candidates = new(
            _ => 0,
            (source, next) => calls.Add($"next:{source}:{next}"),
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        Direct3D9BitmapColorSourceContextParameters context = CreateContext() with { WrapMode = MilBitmapWrapMode.Tile };
        lifetime.CacheDeviceBitmapColorSource(31);
        lifetime.SetLastUsedColorSource(41, context);
        calls.Clear();

        bool found = Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            CreateProperties(),
            true,
            candidates,
            (Direct3D9DelayedBounds _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle _) => true,
            (_, _) => true,
            (nint _, out uint width, out uint height) =>
            {
                width = 63;
                height = 31;
                calls.Add("size");
                return Direct3D9Factory.SuccessHResult;
            },
            supportsConditionalNonPowerOfTwoTextures: true,
            supportsUnconditionalNonPowerOfTwoTextures: false,
            (_, _, _) => calls.Add("wrap"),
            _ => true,
            (source, _, _, _, check) =>
            {
                calls.Add($"bounds:{source}:{check}");
                return true;
            },
            out nint bitmapColorSource);

        Assert.AreEqual(
            "True|41|31|size|bounds:41:PossibleAndUpdateRequired|add:41|next:31:0|add:31",
            $"{found}|{bitmapColorSource}|{candidates.Head}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenOwnedLookupFindsLastUsedThenReusableCandidateOwnershipIsTransferred()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => { },
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        Direct3D9BitmapColorSourceContextParameters context = CreateContext();
        lifetime.CacheDeviceBitmapColorSource(31);
        lifetime.SetLastUsedColorSource(41, context);
        calls.Clear();

        bool found = Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            false,
            () => new Direct3D9BitmapReusableRealizationCandidates(
                source => next.GetValueOrDefault(source),
                (source, value) => next[source] = value,
                value => calls.Add($"add:{value}"),
                value => calls.Add($"release:{value}")),
            _ => true,
            (source, _, _, _, check) => check != Direct3D9BitmapRequiredBoundsCheck.Cached,
            out nint bitmapColorSource,
            out nint reusableRealizationSources);

        Assert.AreEqual(
            "True|41|31|add:41|add:31",
            $"{found}|{bitmapColorSource}|{reusableRealizationSources}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenOwnedLookupFindsCachedDeviceBitmapThenReusableListRemainsNull()
    {
        List<string> calls = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => { },
            value => calls.Add($"add:{value}"),
            value => calls.Add($"release:{value}"));
        lifetime.CacheDeviceBitmapColorSource(31);
        calls.Clear();

        bool found = Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
            lifetime,
            CreateContext(),
            new Direct3D9DelayedBounds(),
            false,
            () => new Direct3D9BitmapReusableRealizationCandidates(
                _ => 0,
                (_, _) => Assert.Fail(),
                _ => Assert.Fail(),
                _ => Assert.Fail()),
            _ =>
            {
                Assert.Fail();
                return false;
            },
            (_, _, _, _, _) => true,
            out nint bitmapColorSource,
            out nint reusableRealizationSources);

        Assert.AreEqual(
            "True|31|0|add:31",
            $"{found}|{bitmapColorSource}|{reusableRealizationSources}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenOwnedLookupMissesThenReusableListRemainsOwnedAndIsDisposed()
    {
        List<string> calls = [];
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => { },
            _ => { },
            _ => { });

        bool found = Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
            lifetime,
            CreateContext(),
            new Direct3D9DelayedBounds(),
            false,
            () => new Direct3D9BitmapReusableRealizationCandidates(
                _ => 0,
                (_, _) => Assert.Fail(),
                _ => Assert.Fail(),
                value => calls.Add($"release:{value}")),
            _ =>
            {
                Assert.Fail();
                return false;
            },
            (_, _, _, _, _) =>
            {
                Assert.Fail();
                return false;
            },
            out nint bitmapColorSource,
            out nint reusableRealizationSources);

        Assert.AreEqual(
            "False|0|0|",
            $"{found}|{bitmapColorSource}|{reusableRealizationSources}|{string.Join('|', calls)}");
    }

    private static Direct3D9BitmapColorSourceContextParameters CreateContext() =>
        Direct3D9BitmapColorSourceContextParameters.FromExplicitSettings(
            MilBitmapInterpolationMode.Linear,
            false,
            MilPixelFormat.Pbgra32Bpp,
            MilBitmapWrapMode.Extend);

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
            LayoutU = new(64, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new(32, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

    private static int UnexpectedResolve(out nint bitmapCache)
    {
        bitmapCache = 0;
        Assert.Fail();
        return Direct3D9Factory.GenericFailureHResult;
    }

    private static int UnexpectedChoose(nint _, out nint bitmapColorSource, out nint reusableRealizationSource)
    {
        bitmapColorSource = 0;
        reusableRealizationSource = 0;
        Assert.Fail();
        return Direct3D9Factory.GenericFailureHResult;
    }

    private static int UnexpectedCreate(out nint bitmapColorSource)
    {
        bitmapColorSource = 0;
        Assert.Fail();
        return Direct3D9Factory.GenericFailureHResult;
    }
}
