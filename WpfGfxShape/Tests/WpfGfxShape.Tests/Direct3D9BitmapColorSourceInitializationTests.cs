using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapColorSourceInitializationTests
{
    [TestMethod]
    public void WhenLastUsedColorSourceMatchesThenFallbackSelectionIsSkippedAndCandidateIsInitialized()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        Direct3D9BitmapColorSourceContextParameters context = CreateContext();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        lifetime.SetLastUsedColorSource(21, context);
        lifetime.CacheDeviceBitmapColorSource(23);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.ChooseOrSelectAndInitialize(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            true,
            harness.CreateCandidates,
            _ => true,
            (source, _, _, _, check) =>
            {
                harness.Record($"bounds:{source}:{check}");
                return true;
            },
            (out nint colorSource) =>
            {
                colorSource = 0;
                throw new AssertFailedException();
            },
            (colorSource, candidates) =>
            {
                harness.Record($"initialize:{colorSource}");
                sources.Consume(candidates);
                return Direct3D9Factory.SuccessHResult;
            },
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual(
            "0|21|23|bounds:21:PossibleAndUpdateRequired,add:21,set-next:23:0,add:23,initialize:21,set-next:23:0,set-next:23:0,add:23,release:23",
            $"{result}|{bitmapColorSource}|{sources.Head}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenFastLookupMissesThenFallbackSelectionUsesSameCandidateContainer()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();

        int result = Direct3D9BitmapColorSourceInitialization.ChooseOrSelectAndInitialize(
            lifetime,
            CreateContext(),
            new Direct3D9DelayedBounds(),
            false,
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            (out nint colorSource) =>
            {
                harness.Record("select");
                colorSource = 31;
                return 1;
            },
            (colorSource, candidates) =>
            {
                harness.Record($"initialize:{colorSource}:{candidates.Head}");
                sources.Consume(candidates);
                return 1;
            },
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual("1|31|0|select,initialize:31:0", $"{result}|{bitmapColorSource}|{sources.Head}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenFastLookupMatchesButInitializationFailsThenReferenceAndCandidateAreReleased()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        Direct3D9BitmapColorSourceContextParameters context = CreateContext();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        lifetime.SetLastUsedColorSource(21, context);
        lifetime.CacheDeviceBitmapColorSource(23);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.ChooseOrSelectAndInitialize(
            lifetime,
            context,
            new Direct3D9DelayedBounds(),
            true,
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => true,
            (out nint colorSource) =>
            {
                colorSource = 0;
                throw new AssertFailedException();
            },
            (_, _) => Direct3D9Factory.NotImplementedHResult,
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.NotImplementedHResult}|0|add:21,set-next:23:0,add:23,set-next:23:0,release:23,release:21",
            $"{result}|{bitmapColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenSelectionAndInitializationSucceedThenReferenceAndSuccessCodeAreTransferred()
    {
        InitializationHarness harness = new();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(31);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.SelectAndInitialize(
            candidates,
            (out nint colorSource) =>
            {
                harness.Record("select");
                colorSource = 21;
                return 1;
            },
            (colorSource, reusableCandidates) =>
            {
                harness.Record($"initialize:{colorSource}");
                sources.Consume(reusableCandidates);
                return 1;
            },
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual("1|21|31|select,initialize:21,set-next:31:0,set-next:31:0,add:31,release:31", $"{result}|{bitmapColorSource}|{sources.Head}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenSelectionFailsThenCandidatesAreReleasedAndFailureIsPreserved()
    {
        InitializationHarness harness = new();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(31);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.SelectAndInitialize(
            candidates,
            (out nint colorSource) =>
            {
                harness.Record("select");
                colorSource = 0;
                return Direct3D9Factory.NotImplementedHResult;
            },
            (_, _) => throw new AssertFailedException(),
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual($"{Direct3D9Factory.NotImplementedHResult}|0|select,set-next:31:0,release:31", $"{result}|{bitmapColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenSelectionSucceedsWithoutColorSourceThenCandidatesAreReleasedAndGenericFailureIsReturned()
    {
        InitializationHarness harness = new();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(31);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.SelectAndInitialize(
            candidates,
            (out nint colorSource) =>
            {
                colorSource = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _) => throw new AssertFailedException(),
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual($"{Direct3D9Factory.GenericFailureHResult}|0|set-next:31:0,release:31", $"{result}|{bitmapColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenInitializationFailsBeforeConsumptionThenColorSourceAndCandidatesAreReleased()
    {
        InitializationHarness harness = new();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(31);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.SelectAndInitialize(
            candidates,
            (out nint colorSource) =>
            {
                colorSource = 21;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _) => Direct3D9Factory.NotImplementedHResult,
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual($"{Direct3D9Factory.NotImplementedHResult}|0|set-next:31:0,release:31,release:21", $"{result}|{bitmapColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenInitializationConsumesCandidatesThenFailsThenOnlyColorSourceIsReleasedByCoordinator()
    {
        InitializationHarness harness = new();
        using Direct3D9BitmapReusableRealizationCandidates candidates = harness.CreateCandidates();
        candidates.Add(31);
        using Direct3D9BitmapReusableRealizationSources sources = harness.CreateSources();
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.SelectAndInitialize(
            candidates,
            (out nint colorSource) =>
            {
                colorSource = 21;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, reusableCandidates) =>
            {
                sources.Consume(reusableCandidates);
                return Direct3D9Factory.NotImplementedHResult;
            },
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual($"{Direct3D9Factory.NotImplementedHResult}|0|31|set-next:31:0,set-next:31:0,add:31,release:31,release:21", $"{result}|{bitmapColorSource}|{sources.Head}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenFastLookupMissesThenCompleteCacheSelectionCreatesAndInitializesWithSameCandidates()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapRealizationProperties cachedProperties = CreateProperties();
        Direct3D9BitmapRealizationProperties requestedProperties = cachedProperties with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };
        entries.Store(cachedProperties, 41);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.ChooseAndInitialize(
            entries,
            lifetime,
            17,
            ref requestedProperties,
            CreateContext(),
            new Direct3D9DelayedBounds(),
            false,
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            source =>
            {
                harness.Record($"render-target:{source}");
                return true;
            },
            false,
            (bool createAsRenderTarget, out nint colorSource) =>
            {
                harness.Record($"create:{createAsRenderTarget}");
                colorSource = 51;
                return Direct3D9Factory.SuccessHResult;
            },
            (colorSource, candidates) =>
            {
                harness.Record($"initialize:{colorSource}:{candidates.Head}");
                return Direct3D9Factory.SuccessHResult;
            },
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual(
            "0|51|51|set-next:41:0,add:41,render-target:41,create:True,add:51,add:51,initialize:51:41,set-next:41:0,release:41",
            $"{result}|{bitmapColorSource}|{lifetime.LastUsedColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenCompleteCacheSelectionFailsThenFailureAndCandidateCleanupArePreserved()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapRealizationProperties cachedProperties = CreateProperties();
        Direct3D9BitmapRealizationProperties requestedProperties = cachedProperties with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };
        entries.Store(cachedProperties, 41);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.ChooseAndInitialize(
            entries,
            lifetime,
            17,
            ref requestedProperties,
            CreateContext(),
            new Direct3D9DelayedBounds(),
            false,
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => false,
            false,
            (bool createAsRenderTarget, out nint colorSource) =>
            {
                harness.Record($"create:{createAsRenderTarget}");
                colorSource = 0;
                return Direct3D9Factory.NotImplementedHResult;
            },
            (_, _) => throw new AssertFailedException(),
            harness.Release,
            out nint bitmapColorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.NotImplementedHResult}|0|set-next:41:0,add:41,create:False,set-next:41:0,release:41",
            $"{result}|{bitmapColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenRealizerUsesProductionFactoryThenSelectedSourceFlowsIntoReusableContext()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        entries.Store(properties, 41);
        harness.ClearCalls();
        nint sourcesColorSource = 0;
        nint populationColorSource = 0;
        Direct3D9BitmapReusableRealizationContextFactory reusableContextFactory = new(
            false,
            () => throw new AssertFailedException(),
            colorSource =>
            {
                sourcesColorSource = colorSource;
                return harness.CreateSources();
            },
            colorSource =>
            {
                populationColorSource = colorSource;
                return harness.CreatePopulation();
            });
        Direct3D9BitmapPipelineColorSourceFactory pipelineFactory = new(
            (nint _, nint _, nint _, nint _, nint _, Direct3D9BitmapColorSourceContextParameters _, out Direct3D9BitmapColorSourceInitializationState? state) =>
            {
                harness.Record("resolve");
                state = new Direct3D9BitmapColorSourceInitializationState(
                    entries,
                    lifetime,
                    properties,
                    new Direct3D9DelayedBounds(),
                    false);
                return Direct3D9Factory.SuccessHResult;
            },
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => true,
            false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 51;
                return Direct3D9Factory.SuccessHResult;
            },
            reusableContextFactory,
            context => new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null),
                context.Dispose),
            (colorSource, textureRealizer) => new Direct3D9BitmapPipelineColorSource(
                colorSource,
                textureRealizer,
                new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => Direct3D9Factory.SuccessHResult),
                harness.Release),
            harness.Release);
        Direct3D9BitmapColorSourceRealizer realizer = new(
            (nint _, out nint bitmap, out nint cache) =>
            {
                bitmap = 7;
                cache = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            _ => throw new AssertFailedException(),
            (nint _, Direct3D9BitmapRealizationContext _, Direct3D9BitmapColorSourceContextParameters _, out nint realizationParameters) =>
            {
                realizationParameters = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            pipelineFactory,
            harness.Release);

        int result = realizer.Derive(
            17,
            0,
            0,
            new Direct3D9BitmapRealizationContext(1, 2, 3, 4, true, 5),
            CreateContext(),
            out Direct3D9BitmapPipelineColorSource? pipelineColorSource);

        nint bitmapColorSource = pipelineColorSource!.BitmapColorSource;
        pipelineColorSource.Dispose();

        Assert.AreEqual(
            "0|41|41|41|resolve,add:41,add:41,release:41",
            $"{result}|{bitmapColorSource}|{sourcesColorSource}|{populationColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenResolvedStateIsDeviceBitmapThenProductionFactoryQueriesAdapterContributorState()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        entries.Store(properties, 41);
        harness.ClearCalls();
        Direct3D9BitmapReusableRealizationContextFactory reusableContextFactory = new(
            false,
            () =>
            {
                harness.Record("adapter");
                return true;
            },
            _ => harness.CreateSources(),
            _ => harness.CreatePopulation());
        Direct3D9BitmapPipelineColorSourceFactory pipelineFactory = new(
            (nint _, nint _, nint _, nint _, nint _, Direct3D9BitmapColorSourceContextParameters _, out Direct3D9BitmapColorSourceInitializationState? state) =>
            {
                state = new Direct3D9BitmapColorSourceInitializationState(
                    entries,
                    lifetime,
                    properties,
                    new Direct3D9DelayedBounds(),
                    true);
                return Direct3D9Factory.SuccessHResult;
            },
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => true,
            false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 51;
                return Direct3D9Factory.SuccessHResult;
            },
            reusableContextFactory,
            context =>
            {
                Assert.IsTrue(context.HasContributorFromDifferentAdapter);
                return new Direct3D9BitmapColorSourceTextureRealizer(
                    () => (Direct3D9Factory.NotImplementedHResult, null),
                    context.Dispose);
            },
            (colorSource, textureRealizer) => new Direct3D9BitmapPipelineColorSource(
                colorSource,
                textureRealizer,
                new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => Direct3D9Factory.SuccessHResult),
                harness.Release),
            harness.Release);

        int result = pipelineFactory.ChooseAndCreate(
            17,
            7,
            0,
            31,
            0,
            CreateContext(),
            out Direct3D9BitmapPipelineColorSource? pipelineColorSource);
        pipelineColorSource!.Dispose();

        Assert.AreEqual($"{Direct3D9Factory.SuccessHResult}|add:41,add:41,adapter,release:41", $"{result}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenProductionFactoryCreatesDeviceBitmapThenRegistryTracksSelectedSourceInitializationState()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        entries.Store(properties, 41);
        harness.ClearCalls();
        Direct3D9BitmapColorSourceRegistry registry = new();
        Direct3D9BitmapReusableRealizationContextFactory reusableContextFactory = new(
            false,
            () => false,
            _ => harness.CreateSources(),
            _ => harness.CreatePopulation());
        Direct3D9BitmapColorSourceTextureRealizer? createdTextureRealizer = null;
        Direct3D9BitmapPipelineColorSourceFactory pipelineFactory = new(
            (nint _, nint _, nint _, nint _, nint _, Direct3D9BitmapColorSourceContextParameters _, out Direct3D9BitmapColorSourceInitializationState? state) =>
            {
                state = new Direct3D9BitmapColorSourceInitializationState(
                    entries,
                    lifetime,
                    properties,
                    new Direct3D9DelayedBounds(),
                    true);
                return Direct3D9Factory.SuccessHResult;
            },
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => true,
            false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 51;
                return Direct3D9Factory.SuccessHResult;
            },
            reusableContextFactory,
            context => createdTextureRealizer = new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null),
                context.Dispose),
            registry,
            (_, _) => new Direct3D9PipelineColorSource(
                Direct3D9ColorSourceType.Texture,
                () => Direct3D9Factory.SuccessHResult),
            harness.Release);

        int result = pipelineFactory.ChooseAndCreate(
            17,
            7,
            0,
            31,
            0,
            CreateContext(),
            out Direct3D9BitmapPipelineColorSource? pipelineColorSource);
        Direct3D9BitmapColorSourceOwner owner = registry.Resolve(pipelineColorSource!.BitmapColorSource);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|41|True|True",
            $"{result}|{pipelineColorSource.BitmapColorSource}|{owner.IsDeviceBitmap}|{ReferenceEquals(createdTextureRealizer, owner.TextureRealizer)}");

        pipelineColorSource.Dispose();
    }

    [TestMethod]
    public void WhenProductionFactoryRegistryTargetIsNotRegisteredThenCandidatesAreReleased()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        using Direct3D9Device device = CreateDevice();
        Direct3D9BitmapRealizationProperties properties = CreateProperties();
        entries.Store(properties, 41);
        harness.ClearCalls();
        Direct3D9BitmapPipelineColorSourceFactory pipelineFactory = new(
            (nint _, nint _, nint _, nint _, nint _, Direct3D9BitmapColorSourceContextParameters _, out Direct3D9BitmapColorSourceInitializationState? state) =>
            {
                state = new Direct3D9BitmapColorSourceInitializationState(
                    entries,
                    lifetime,
                    properties,
                    new Direct3D9DelayedBounds(),
                    false);
                return Direct3D9Factory.SuccessHResult;
            },
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => true,
            canStretchRectFromTextures: false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 51;
                return Direct3D9Factory.SuccessHResult;
            },
            device,
            _ => throw new AssertFailedException(),
            new Direct3D9BitmapColorSourceRegistry(),
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                rectangles = [];
                throw new AssertFailedException();
            },
            _ => 0,
            (_, _) => { },
            harness.AddReference,
            harness.Release,
            _ => throw new AssertFailedException(),
            (_, _) => throw new AssertFailedException());

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => pipelineFactory.ChooseAndCreate(
            17,
            7,
            0,
            31,
            0,
            CreateContext(),
            out _));

        Assert.AreEqual(
            "The bitmap color source is not registered.|add:41,add:41,release:41",
            $"{exception.Message}|{harness.Calls}");
    }

    private static unsafe Direct3D9Device CreateDevice() =>
        new(null, null, 0, Devtype.Hal, 0, default);

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

    [TestMethod]
    public void WhenTextureRealizerIsCreatedThenCandidatesRemainOwnedUntilRealizerDisposal()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapRealizationProperties cachedProperties = CreateProperties();
        Direct3D9BitmapRealizationProperties requestedProperties = cachedProperties with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };
        entries.Store(cachedProperties, 41);
        harness.ClearCalls();

        int result = Direct3D9BitmapColorSourceInitialization.ChooseAndCreateTextureRealizer(
            entries,
            lifetime,
            17,
            ref requestedProperties,
            CreateContext(),
            new Direct3D9DelayedBounds(),
            false,
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => true,
            false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 51;
                return Direct3D9Factory.SuccessHResult;
            },
            candidates => harness.CreateContext(candidates),
            context => new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null),
                context.Dispose),
            harness.Release,
            out nint bitmapColorSource,
            out Direct3D9BitmapColorSourceTextureRealizer? textureRealizer);

        string callsBeforeDisposal = harness.Calls;
        textureRealizer!.Dispose();

        Assert.AreEqual(
            "0|51|set-next:41:0,add:41,add:51,add:51|set-next:41:0,add:41,add:51,add:51,set-next:41:0,release:41",
            $"{result}|{bitmapColorSource}|{callsBeforeDisposal}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenTextureRealizerCreationFailsThenContextCandidatesAndColorSourceAreReleased()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapRealizationProperties cachedProperties = CreateProperties();
        Direct3D9BitmapRealizationProperties requestedProperties = cachedProperties with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };
        entries.Store(cachedProperties, 41);
        harness.ClearCalls();

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            Direct3D9BitmapColorSourceInitialization.ChooseAndCreateTextureRealizer(
                entries,
                lifetime,
                17,
                ref requestedProperties,
                CreateContext(),
                new Direct3D9DelayedBounds(),
                false,
                harness.CreateCandidates,
                _ => true,
                (_, _, _, _, _) => throw new AssertFailedException(),
                _ => true,
                false,
                (bool _, out nint colorSource) =>
                {
                    colorSource = 51;
                    return Direct3D9Factory.SuccessHResult;
                },
                candidates => harness.CreateContext(candidates),
                _ => throw new InvalidOperationException(),
                harness.Release,
                out _,
                out _));

        Assert.AreEqual(
            "set-next:41:0,add:41,add:51,add:51,set-next:41:0,release:41,release:51",
            harness.Calls);
    }

    [TestMethod]
    public void WhenPipelineColorSourceIsCreatedThenSelectedOwnershipIsTransferredUntilPipelineDisposal()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapRealizationProperties cachedProperties = CreateProperties();
        Direct3D9BitmapRealizationProperties requestedProperties = cachedProperties with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };
        entries.Store(cachedProperties, 41);
        harness.ClearCalls();
        Direct3D9BitmapReusableRealizationContextFactory reusableContextFactory = new(
            false,
            () => throw new AssertFailedException(),
            harness.CreateSources,
            harness.CreatePopulation);

        int result = Direct3D9BitmapColorSourceInitialization.ChooseAndCreatePipelineColorSource(
            entries,
            lifetime,
            17,
            ref requestedProperties,
            CreateContext(),
            new Direct3D9DelayedBounds(),
            false,
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => true,
            false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 51;
                return Direct3D9Factory.SuccessHResult;
            },
            reusableContextFactory,
            context => new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null),
                context.Dispose),
            (colorSource, textureRealizer) => new Direct3D9BitmapPipelineColorSource(
                colorSource,
                textureRealizer,
                new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => Direct3D9Factory.SuccessHResult),
                harness.Release),
            harness.Release,
            out Direct3D9BitmapPipelineColorSource? pipelineColorSource);

        nint bitmapColorSource = pipelineColorSource!.BitmapColorSource;
        string callsBeforeDisposal = harness.Calls;
        pipelineColorSource.Dispose();

        Assert.AreEqual(
            "0|51|set-next:41:0,add:41,add:51,add:51|set-next:41:0,add:41,add:51,add:51,set-next:41:0,release:41,release:51",
            $"{result}|{bitmapColorSource}|{callsBeforeDisposal}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenPipelineColorSourceCreationFailsThenRealizerAndColorSourceAreReleasedInNativeOrder()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapRealizationProperties cachedProperties = CreateProperties();
        Direct3D9BitmapRealizationProperties requestedProperties = cachedProperties with
        {
            LayoutU = CreateLayout(Direct3D9TexelLayout.CenterSplit)
        };
        entries.Store(cachedProperties, 41);
        harness.ClearCalls();

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            Direct3D9BitmapColorSourceInitialization.ChooseAndCreatePipelineColorSource(
                entries,
                lifetime,
                17,
                ref requestedProperties,
                CreateContext(),
                new Direct3D9DelayedBounds(),
                false,
                harness.CreateCandidates,
                _ => true,
                (_, _, _, _, _) => throw new AssertFailedException(),
                _ => true,
                false,
                (bool _, out nint colorSource) =>
                {
                    colorSource = 51;
                    return Direct3D9Factory.SuccessHResult;
                },
                candidates => harness.CreateContext(candidates),
                context => new Direct3D9BitmapColorSourceTextureRealizer(
                    () => (Direct3D9Factory.NotImplementedHResult, null),
                    context.Dispose),
                (_, _) => throw new InvalidOperationException(),
                harness.Release,
                out _));

        Assert.AreEqual(
            "set-next:41:0,add:41,add:51,add:51,set-next:41:0,release:41,release:51",
            harness.Calls);
    }

    [TestMethod]
    public void WhenDeviceBitmapPipelineInitializationFindsValidCachedSourceThenLastUsedIsSkipped()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapColorSourceContextParameters context = CreateContext();
        lifetime.CacheDeviceBitmapColorSource(31);
        lifetime.SetLastUsedColorSource(41, context);
        harness.ClearCalls();
        Direct3D9BitmapRealizationProperties properties = CreateProperties();

        int result = Direct3D9BitmapColorSourceInitialization.ChooseAndCreatePipelineColorSource(
            entries,
            lifetime,
            17,
            ref properties,
            context,
            new Direct3D9DelayedBounds(),
            true,
            new Direct3D9DeviceBitmapColorSourceSelection(
                (Direct3D9DelayedBounds _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle _) =>
                {
                    harness.Record("minimum");
                    return true;
                },
                (bitmap, _) =>
                {
                    harness.Record($"valid:{bitmap}");
                    return true;
                },
                (nint bitmap, out uint width, out uint height) =>
                {
                    harness.Record($"size:{bitmap}");
                    width = 64;
                    height = 32;
                    return Direct3D9Factory.SuccessHResult;
                },
                false,
                false,
                (source, addressU, addressV) => harness.Record($"wrap:{source}:{addressU}:{addressV}")),
            harness.CreateCandidates,
            _ => true,
            (_, _, _, _, _) => throw new AssertFailedException(),
            _ => throw new AssertFailedException(),
            false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 0;
                throw new AssertFailedException();
            },
            (_, candidates) => harness.CreateContext(candidates),
            reusableContext => new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null),
                reusableContext.Dispose),
            (colorSource, textureRealizer) => new Direct3D9BitmapPipelineColorSource(
                colorSource,
                textureRealizer,
                new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => Direct3D9Factory.SuccessHResult),
                harness.Release),
            harness.Release,
            out Direct3D9BitmapPipelineColorSource? pipelineColorSource);

        nint bitmapColorSource = pipelineColorSource!.BitmapColorSource;
        pipelineColorSource.Dispose();

        Assert.AreEqual(
            "0|31|minimum,valid:11,size:11,wrap:31:TaddressClamp:TaddressClamp,add:31,release:31",
            $"{result}|{bitmapColorSource}|{harness.Calls}");
    }

    [TestMethod]
    public void WhenDeviceBitmapCannotTileNpotSourceThenPipelineInitializationFallsBackWithReusableCandidate()
    {
        InitializationHarness harness = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9BitmapCacheLifetime lifetime = new(
            manager,
            11,
            13,
            () => harness.Record("clear"),
            harness.AddReference,
            harness.Release);
        using Direct3D9BitmapFormatCacheEntry entries = new(harness.AddReference, harness.Release);
        Direct3D9BitmapColorSourceContextParameters context = CreateContext() with { WrapMode = MilBitmapWrapMode.Tile };
        lifetime.CacheDeviceBitmapColorSource(31);
        lifetime.SetLastUsedColorSource(41, context);
        harness.ClearCalls();
        Direct3D9BitmapRealizationProperties properties = CreateProperties() with { WrapMode = MilBitmapWrapMode.Tile };

        int result = Direct3D9BitmapColorSourceInitialization.ChooseAndCreatePipelineColorSource(
            entries,
            lifetime,
            17,
            ref properties,
            context,
            new Direct3D9DelayedBounds(),
            true,
            new Direct3D9DeviceBitmapColorSourceSelection(
                (Direct3D9DelayedBounds _, Direct3D9BitmapRealizationProperties _, ref Direct3D9BitmapRealizationRectangle _) => true,
                (_, _) => true,
                (nint _, out uint width, out uint height) =>
                {
                    harness.Record("size");
                    width = 63;
                    height = 31;
                    return Direct3D9Factory.SuccessHResult;
                },
                true,
                false,
                (_, _, _) => throw new AssertFailedException()),
            harness.CreateCandidates,
            _ => true,
            (source, _, _, _, check) =>
            {
                harness.Record($"bounds:{source}:{check}");
                return true;
            },
            _ => throw new AssertFailedException(),
            false,
            (bool _, out nint colorSource) =>
            {
                colorSource = 0;
                throw new AssertFailedException();
            },
            (_, candidates) => harness.CreateContext(candidates),
            reusableContext => new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null),
                reusableContext.Dispose),
            (colorSource, textureRealizer) => new Direct3D9BitmapPipelineColorSource(
                colorSource,
                textureRealizer,
                new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => Direct3D9Factory.SuccessHResult),
                harness.Release),
            harness.Release,
            out Direct3D9BitmapPipelineColorSource? pipelineColorSource);

        nint bitmapColorSource = pipelineColorSource!.BitmapColorSource;
        pipelineColorSource.Dispose();

        Assert.AreEqual(
            "0|41|size,bounds:41:PossibleAndUpdateRequired,add:41,set-next:31:0,add:31,set-next:31:0,release:31,release:41",
            $"{result}|{bitmapColorSource}|{harness.Calls}");
    }

    private static Direct3D9BitmapColorSourceContextParameters CreateContext() =>
        Direct3D9BitmapColorSourceContextParameters.FromExplicitSettings(
            MilBitmapInterpolationMode.Linear,
            false,
            MilPixelFormat.Pbgra32Bpp,
            MilBitmapWrapMode.Extend);

    private sealed class InitializationHarness
    {
        private readonly Dictionary<nint, nint> _next = [];
        private readonly List<string> _calls = [];

        internal string Calls => string.Join(',', _calls);

        internal void Record(string call) => _calls.Add(call);

        internal void ClearCalls() => _calls.Clear();

        internal Direct3D9BitmapReusableRealizationCandidates CreateCandidates() =>
            new(GetNext, SetNext, AddReference, Release);

        internal Direct3D9BitmapReusableRealizationContext CreateContext(
            Direct3D9BitmapReusableRealizationCandidates candidates) =>
            new(false, candidates, CreateSources(), CreatePopulation());

        internal Direct3D9BitmapReusableRealizationPopulation CreatePopulation() =>
            new(
                new Direct3D9BitmapReusableRealizationUpdater(
                    new Direct3D9BitmapRealizationRectangle(0, 0, 100, 100),
                    (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
                    {
                        rectangles = [];
                        return Direct3D9Factory.SuccessHResult;
                    },
                    _ => Direct3D9Factory.SuccessHResult,
                    (nint _, out nint surface) =>
                    {
                        surface = 0;
                        return Direct3D9Factory.SuccessHResult;
                    },
                    () => (Direct3D9Factory.SuccessHResult, 0),
                    (_, _, _, _) => Direct3D9Factory.SuccessHResult,
                    _ => { }),
                GetNext,
                (nint _, out Direct3D9BitmapRealizationRectangle rectangle) =>
                {
                    rectangle = new Direct3D9BitmapRealizationRectangle(0, 0, 100, 100);
                    return true;
                });

        internal Direct3D9BitmapReusableRealizationSources CreateSources() =>
            new(
                new Direct3D9BitmapReusableRealizationTargetState(
                    41,
                    true,
                    true,
                    100,
                    100,
                    new Direct3D9BitmapRealizationRectangle(0, 0, 100, 100),
                    8),
                (nint source, out Direct3D9BitmapReusableRealizationSourceState state) =>
                {
                    state = new Direct3D9BitmapReusableRealizationSourceState(
                        0,
                        true,
                        true,
                        100,
                        100,
                        new Direct3D9BitmapRealizationRectangle(0, 0, 100, 100),
                        7,
                        true,
                        false);
                    return true;
                },
                GetNext,
                SetNext,
                AddReference,
                Release,
                _ => throw new AssertFailedException());

        internal void AddReference(nint value) => Record($"add:{value}");

        internal void Release(nint value) => Record($"release:{value}");

        private nint GetNext(nint value) => _next.GetValueOrDefault(value);

        private void SetNext(nint value, nint next)
        {
            _next[value] = next;
            Record($"set-next:{value}:{next}");
        }

    }
}
