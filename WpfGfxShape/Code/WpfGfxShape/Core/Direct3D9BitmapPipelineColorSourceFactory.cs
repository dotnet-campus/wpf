namespace WpfGfxShape.Core;

internal sealed class Direct3D9BitmapColorSourceInitializationState
{
    internal Direct3D9BitmapColorSourceInitializationState(
        Direct3D9BitmapFormatCacheEntry cacheEntries,
        Direct3D9BitmapCacheLifetime cacheLifetime,
        Direct3D9BitmapRealizationProperties properties,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9DeviceBitmapColorSourceSelection? deviceBitmapSelection = null)
    {
        ArgumentNullException.ThrowIfNull(cacheEntries);
        ArgumentNullException.ThrowIfNull(cacheLifetime);
        ArgumentNullException.ThrowIfNull(realizationBounds);

        CacheEntries = cacheEntries;
        CacheLifetime = cacheLifetime;
        Properties = properties;
        RealizationBounds = realizationBounds;
        IsDeviceBitmap = isDeviceBitmap;
        DeviceBitmapSelection = deviceBitmapSelection;
    }

    internal Direct3D9BitmapFormatCacheEntry CacheEntries { get; }

    internal Direct3D9BitmapCacheLifetime CacheLifetime { get; }

    internal Direct3D9BitmapRealizationProperties Properties { get; set; }

    internal Direct3D9DelayedBounds RealizationBounds { get; }

    internal bool IsDeviceBitmap { get; }

    internal Direct3D9DeviceBitmapColorSourceSelection? DeviceBitmapSelection { get; }
}

internal delegate int Direct3D9ResolveBitmapColorSourceInitializationState(
    nint bitmapSource,
    nint bitmap,
    nint bitmapCache,
    nint realizationParameters,
    nint alternateCache,
    Direct3D9BitmapColorSourceContextParameters contextParameters,
    out Direct3D9BitmapColorSourceInitializationState? initializationState);

internal sealed class Direct3D9BitmapPipelineColorSourceFactory
{
    private readonly Direct3D9ResolveBitmapColorSourceInitializationState _resolveInitializationState;
    private readonly Direct3D9CreateBitmapReusableRealizationCandidates _createReusableCandidates;
    private readonly Func<nint, bool> _isColorSourceValid;
    private readonly Direct3D9CheckBitmapColorSourceRequiredBounds _checkRequiredBounds;
    private readonly Func<nint, bool> _isRenderTarget;
    private readonly bool _canStretchRectFromTextures;
    private readonly Direct3D9CreateCachedBitmapColorSource _createColorSource;
    private readonly Direct3D9BitmapReusableRealizationContextFactory _reusableContextFactory;
    private readonly Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> _createTextureRealizer;
    private readonly Func<nint, Direct3D9BitmapColorSourceTextureRealizer, bool, Direct3D9BitmapPipelineColorSource> _createPipelineColorSource;
    private readonly Action<nint> _release;

    internal Direct3D9BitmapPipelineColorSourceFactory(
        Direct3D9ResolveBitmapColorSourceInitializationState resolveInitializationState,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Direct3D9BitmapReusableRealizationContextFactory reusableContextFactory,
        Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> createTextureRealizer,
        Func<nint, Direct3D9BitmapColorSourceTextureRealizer, Direct3D9BitmapPipelineColorSource> createPipelineColorSource,
        Action<nint> release)
        : this(
            resolveInitializationState,
            createReusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            isRenderTarget,
            canStretchRectFromTextures,
            createColorSource,
            reusableContextFactory,
            createTextureRealizer,
            (bitmapColorSource, textureRealizer, _) => createPipelineColorSource(bitmapColorSource, textureRealizer),
            release)
    {
        ArgumentNullException.ThrowIfNull(createPipelineColorSource);
    }

    internal Direct3D9BitmapPipelineColorSourceFactory(
        Direct3D9ResolveBitmapColorSourceInitializationState resolveInitializationState,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Direct3D9BitmapReusableRealizationContextFactory reusableContextFactory,
        Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> createTextureRealizer,
        Direct3D9BitmapColorSourceRegistry registry,
        Func<nint, Direct3D9BitmapColorSourceTextureRealizer, Direct3D9PipelineColorSource> createPipelineColorSource,
        Action<nint> release)
        : this(
            resolveInitializationState,
            createReusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            isRenderTarget,
            canStretchRectFromTextures,
            createColorSource,
            reusableContextFactory,
            createTextureRealizer,
            (bitmapColorSource, textureRealizer, isDeviceBitmap) => new Direct3D9BitmapPipelineColorSource(
                bitmapColorSource,
                textureRealizer,
                createPipelineColorSource(bitmapColorSource, textureRealizer),
                release,
                registry,
                isDeviceBitmap),
            release)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(createPipelineColorSource);
    }

    internal Direct3D9BitmapPipelineColorSourceFactory(
        Direct3D9ResolveBitmapColorSourceInitializationState resolveInitializationState,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Direct3D9Device device,
        Func<nint, bool> hasContributorFromDifferentAdapter,
        Direct3D9BitmapColorSourceRegistry registry,
        Direct3D9GetDeviceBitmapValidSourceRectangles getDeviceBitmapValidSourceRectangles,
        Func<nint, nint> getNext,
        Action<nint, nint> setNext,
        Action<nint> addReference,
        Action<nint> release,
        Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> createTextureRealizer,
        Func<nint, Direct3D9BitmapColorSourceTextureRealizer, Direct3D9PipelineColorSource> createPipelineColorSource)
        : this(
            resolveInitializationState,
            createReusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            isRenderTarget,
            canStretchRectFromTextures,
            createColorSource,
            new Direct3D9BitmapReusableRealizationContextFactory(
                isDeviceBitmap: false,
                hasContributorFromDifferentAdapter,
                device,
                registry,
                canStretchRectFromTextures,
                getDeviceBitmapValidSourceRectangles,
                getNext,
                setNext,
                addReference,
                release),
            createTextureRealizer,
            registry,
            createPipelineColorSource,
            release)
    {
    }

    private Direct3D9BitmapPipelineColorSourceFactory(
        Direct3D9ResolveBitmapColorSourceInitializationState resolveInitializationState,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Direct3D9BitmapReusableRealizationContextFactory reusableContextFactory,
        Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> createTextureRealizer,
        Func<nint, Direct3D9BitmapColorSourceTextureRealizer, bool, Direct3D9BitmapPipelineColorSource> createPipelineColorSource,
        Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(resolveInitializationState);
        ArgumentNullException.ThrowIfNull(createReusableCandidates);
        ArgumentNullException.ThrowIfNull(isColorSourceValid);
        ArgumentNullException.ThrowIfNull(checkRequiredBounds);
        ArgumentNullException.ThrowIfNull(isRenderTarget);
        ArgumentNullException.ThrowIfNull(createColorSource);
        ArgumentNullException.ThrowIfNull(reusableContextFactory);
        ArgumentNullException.ThrowIfNull(createTextureRealizer);
        ArgumentNullException.ThrowIfNull(createPipelineColorSource);
        ArgumentNullException.ThrowIfNull(release);

        _resolveInitializationState = resolveInitializationState;
        _createReusableCandidates = createReusableCandidates;
        _isColorSourceValid = isColorSourceValid;
        _checkRequiredBounds = checkRequiredBounds;
        _isRenderTarget = isRenderTarget;
        _canStretchRectFromTextures = canStretchRectFromTextures;
        _createColorSource = createColorSource;
        _reusableContextFactory = reusableContextFactory;
        _createTextureRealizer = createTextureRealizer;
        _createPipelineColorSource = createPipelineColorSource;
        _release = release;
    }

    internal int ChooseAndCreate(
        nint bitmapSource,
        nint bitmap,
        nint bitmapCache,
        nint realizationParameters,
        nint alternateCache,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        out Direct3D9BitmapPipelineColorSource? pipelineColorSource)
    {
        pipelineColorSource = null;
        int result = _resolveInitializationState(
            bitmapSource,
            bitmap,
            bitmapCache,
            realizationParameters,
            alternateCache,
            contextParameters,
            out Direct3D9BitmapColorSourceInitializationState? initializationState);
        if (result < 0)
        {
            return result;
        }
        if (initializationState is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        Direct3D9BitmapRealizationProperties properties = initializationState.Properties;
        try
        {
            return Direct3D9BitmapColorSourceInitialization.ChooseAndCreatePipelineColorSource(
                initializationState.CacheEntries,
                initializationState.CacheLifetime,
                bitmapSource,
                ref properties,
                contextParameters,
                initializationState.RealizationBounds,
                initializationState.IsDeviceBitmap,
                initializationState.DeviceBitmapSelection,
                _createReusableCandidates,
                _isColorSourceValid,
                _checkRequiredBounds,
                _isRenderTarget,
                _canStretchRectFromTextures,
                _createColorSource,
                (colorSource, candidates) => _reusableContextFactory.Create(
                    colorSource,
                    candidates,
                    initializationState.IsDeviceBitmap),
                _createTextureRealizer,
                (bitmapColorSource, textureRealizer) => _createPipelineColorSource(
                    bitmapColorSource,
                    textureRealizer,
                    initializationState.IsDeviceBitmap),
                _release,
                out pipelineColorSource);
        }
        finally
        {
            initializationState.Properties = properties;
        }
    }
}
