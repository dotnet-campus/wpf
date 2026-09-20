using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal sealed record Direct3D9DeviceBitmapColorSourceSelection(
    Direct3D9ComputeDeviceBitmapMinimumRealizationBounds ComputeMinimumRealizationBounds,
    Func<nint, Direct3D9BitmapRealizationRectangle, bool> ContainsValidArea,
    Direct3D9GetBitmapSize GetBitmapSize,
    bool SupportsConditionalNonPowerOfTwoTextures,
    bool SupportsUnconditionalNonPowerOfTwoTextures,
    Action<nint, Textureaddress, Textureaddress> SetWrapModes);

internal delegate int Direct3D9SelectBitmapColorSource(out nint bitmapColorSource);
internal delegate int Direct3D9InitializeBitmapColorSource(
    nint bitmapColorSource,
    Direct3D9BitmapReusableRealizationCandidates reusableCandidates);

internal static class Direct3D9BitmapColorSourceInitialization
{
    internal static int ChooseAndInitialize(
        Direct3D9BitmapFormatCacheEntry cacheEntries,
        Direct3D9BitmapCacheLifetime cacheLifetime,
        nint bitmapSourceNoReference,
        ref Direct3D9BitmapRealizationProperties properties,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Direct3D9InitializeBitmapColorSource initializeColorSource,
        Action<nint> release,
        out nint bitmapColorSource) =>
        ChooseAndInitializeCore(
            cacheEntries,
            cacheLifetime,
            bitmapSourceNoReference,
            ref properties,
            contextParameters,
            realizationBounds,
            isDeviceBitmap,
            null,
            createReusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            isRenderTarget,
            canStretchRectFromTextures,
            createColorSource,
            initializeColorSource,
            false,
            release,
            out bitmapColorSource);

    internal static int ChooseAndCreateTextureRealizer(
        Direct3D9BitmapFormatCacheEntry cacheEntries,
        Direct3D9BitmapCacheLifetime cacheLifetime,
        nint bitmapSourceNoReference,
        ref Direct3D9BitmapRealizationProperties properties,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Func<Direct3D9BitmapReusableRealizationCandidates, Direct3D9BitmapReusableRealizationContext> createReusableContext,
        Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> createTextureRealizer,
        Action<nint> release,
        out nint bitmapColorSource,
        out Direct3D9BitmapColorSourceTextureRealizer? textureRealizer) =>
        ChooseAndCreateTextureRealizerCore(
            cacheEntries,
            cacheLifetime,
            bitmapSourceNoReference,
            ref properties,
            contextParameters,
            realizationBounds,
            isDeviceBitmap,
            null,
            createReusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            isRenderTarget,
            canStretchRectFromTextures,
            createColorSource,
            (_, reusableCandidates) => createReusableContext(reusableCandidates),
            createTextureRealizer,
            release,
            out bitmapColorSource,
            out textureRealizer);

    private static int ChooseAndCreateTextureRealizerCore(
        Direct3D9BitmapFormatCacheEntry cacheEntries,
        Direct3D9BitmapCacheLifetime cacheLifetime,
        nint bitmapSourceNoReference,
        ref Direct3D9BitmapRealizationProperties properties,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9DeviceBitmapColorSourceSelection? deviceBitmapSelection,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Func<nint, Direct3D9BitmapReusableRealizationCandidates, Direct3D9BitmapReusableRealizationContext> createReusableContext,
        Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> createTextureRealizer,
        Action<nint> release,
        out nint bitmapColorSource,
        out Direct3D9BitmapColorSourceTextureRealizer? textureRealizer)
    {
        ArgumentNullException.ThrowIfNull(createReusableContext);
        ArgumentNullException.ThrowIfNull(createTextureRealizer);

        Direct3D9BitmapColorSourceTextureRealizer? selectedRealizer = null;
        int result = ChooseAndInitializeCore(
            cacheEntries,
            cacheLifetime,
            bitmapSourceNoReference,
            ref properties,
            contextParameters,
            realizationBounds,
            isDeviceBitmap,
            deviceBitmapSelection,
            createReusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            isRenderTarget,
            canStretchRectFromTextures,
            createColorSource,
            (selectedColorSource, reusableCandidates) =>
            {
                Direct3D9BitmapReusableRealizationContext reusableContext =
                    createReusableContext(selectedColorSource, reusableCandidates)
                    ?? throw new InvalidOperationException("Reusable realization context creation returned null.");
                try
                {
                    selectedRealizer = createTextureRealizer(reusableContext)
                        ?? throw new InvalidOperationException("Bitmap color source texture realizer creation returned null.");
                    reusableContext = null!;
                    return Direct3D9Factory.SuccessHResult;
                }
                finally
                {
                    reusableContext?.Dispose();
                }
            },
            true,
            release,
            out bitmapColorSource);
        textureRealizer = selectedRealizer;
        return result;
    }

    internal static int ChooseAndCreatePipelineColorSource(
        Direct3D9BitmapFormatCacheEntry cacheEntries,
        Direct3D9BitmapCacheLifetime cacheLifetime,
        nint bitmapSourceNoReference,
        ref Direct3D9BitmapRealizationProperties properties,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Direct3D9BitmapReusableRealizationContextFactory reusableContextFactory,
        Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> createTextureRealizer,
        Func<nint, Direct3D9BitmapColorSourceTextureRealizer, Direct3D9BitmapPipelineColorSource> createPipelineColorSource,
        Action<nint> release,
        out Direct3D9BitmapPipelineColorSource? pipelineColorSource)
    {
        ArgumentNullException.ThrowIfNull(reusableContextFactory);

        return ChooseAndCreatePipelineColorSource(
            cacheEntries,
            cacheLifetime,
            bitmapSourceNoReference,
            ref properties,
            contextParameters,
            realizationBounds,
            isDeviceBitmap,
            createReusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            isRenderTarget,
            canStretchRectFromTextures,
            createColorSource,
            reusableContextFactory.Create,
            createTextureRealizer,
            createPipelineColorSource,
            release,
            out pipelineColorSource);
    }

    internal static int ChooseAndCreatePipelineColorSource(
        Direct3D9BitmapFormatCacheEntry cacheEntries,
        Direct3D9BitmapCacheLifetime cacheLifetime,
        nint bitmapSourceNoReference,
        ref Direct3D9BitmapRealizationProperties properties,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Func<Direct3D9BitmapReusableRealizationCandidates, Direct3D9BitmapReusableRealizationContext> createReusableContext,
        Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> createTextureRealizer,
        Func<nint, Direct3D9BitmapColorSourceTextureRealizer, Direct3D9BitmapPipelineColorSource> createPipelineColorSource,
        Action<nint> release,
        out Direct3D9BitmapPipelineColorSource? pipelineColorSource) =>
        ChooseAndCreatePipelineColorSource(
            cacheEntries,
            cacheLifetime,
            bitmapSourceNoReference,
            ref properties,
            contextParameters,
            realizationBounds,
            isDeviceBitmap,
            null,
            createReusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            isRenderTarget,
            canStretchRectFromTextures,
            createColorSource,
            (_, reusableCandidates) => createReusableContext(reusableCandidates),
            createTextureRealizer,
            createPipelineColorSource,
            release,
            out pipelineColorSource);

    internal static int ChooseAndCreatePipelineColorSource(
        Direct3D9BitmapFormatCacheEntry cacheEntries,
        Direct3D9BitmapCacheLifetime cacheLifetime,
        nint bitmapSourceNoReference,
        ref Direct3D9BitmapRealizationProperties properties,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9DeviceBitmapColorSourceSelection? deviceBitmapSelection,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Func<nint, Direct3D9BitmapReusableRealizationCandidates, Direct3D9BitmapReusableRealizationContext> createReusableContext,
        Func<Direct3D9BitmapReusableRealizationContext, Direct3D9BitmapColorSourceTextureRealizer> createTextureRealizer,
        Func<nint, Direct3D9BitmapColorSourceTextureRealizer, Direct3D9BitmapPipelineColorSource> createPipelineColorSource,
        Action<nint> release,
        out Direct3D9BitmapPipelineColorSource? pipelineColorSource)
    {
        ArgumentNullException.ThrowIfNull(createPipelineColorSource);

        pipelineColorSource = null;
        int result = ChooseAndCreateTextureRealizerCore(
            cacheEntries,
            cacheLifetime,
            bitmapSourceNoReference,
            ref properties,
            contextParameters,
            realizationBounds,
            isDeviceBitmap,
            deviceBitmapSelection,
            createReusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            isRenderTarget,
            canStretchRectFromTextures,
            createColorSource,
            createReusableContext,
            createTextureRealizer,
            release,
            out nint bitmapColorSource,
            out Direct3D9BitmapColorSourceTextureRealizer? textureRealizer);
        if (result < 0)
        {
            return result;
        }
        if (bitmapColorSource == 0 || textureRealizer is null)
        {
            textureRealizer?.Dispose();
            if (bitmapColorSource != 0)
            {
                release(bitmapColorSource);
            }

            return Direct3D9Factory.GenericFailureHResult;
        }

        try
        {
            pipelineColorSource = createPipelineColorSource(bitmapColorSource, textureRealizer)
                ?? throw new InvalidOperationException("Bitmap pipeline color source creation returned null.");
            bitmapColorSource = 0;
            textureRealizer = null;
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            textureRealizer?.Dispose();
            if (bitmapColorSource != 0)
            {
                release(bitmapColorSource);
            }
        }
    }

    private static int ChooseAndInitializeCore(
        Direct3D9BitmapFormatCacheEntry cacheEntries,
        Direct3D9BitmapCacheLifetime cacheLifetime,
        nint bitmapSourceNoReference,
        ref Direct3D9BitmapRealizationProperties properties,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9DeviceBitmapColorSourceSelection? deviceBitmapSelection,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        Direct3D9InitializeBitmapColorSource initializeColorSource,
        bool transferReusableCandidatesOnSuccess,
        Action<nint> release,
        out nint bitmapColorSource)
    {
        ArgumentNullException.ThrowIfNull(cacheEntries);
        ArgumentNullException.ThrowIfNull(createReusableCandidates);

        Direct3D9BitmapReusableRealizationCandidates reusableCandidates =
            createReusableCandidates()
            ?? throw new InvalidOperationException("Reusable realization candidate creation returned null.");
        Direct3D9BitmapRealizationProperties selectedProperties = properties;
        int result = SelectAndInitialize(
            reusableCandidates,
            (out nint selectedColorSource) =>
            {
                bool found = deviceBitmapSelection is null
                    ? Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
                        cacheLifetime,
                        contextParameters,
                        realizationBounds,
                        isDeviceBitmap,
                        reusableCandidates,
                        isColorSourceValid,
                        checkRequiredBounds,
                        out selectedColorSource)
                    : Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
                        cacheLifetime,
                        contextParameters,
                        realizationBounds,
                        selectedProperties,
                        isDeviceBitmap,
                        reusableCandidates,
                        deviceBitmapSelection.ComputeMinimumRealizationBounds,
                        deviceBitmapSelection.ContainsValidArea,
                        deviceBitmapSelection.GetBitmapSize,
                        deviceBitmapSelection.SupportsConditionalNonPowerOfTwoTextures,
                        deviceBitmapSelection.SupportsUnconditionalNonPowerOfTwoTextures,
                        deviceBitmapSelection.SetWrapModes,
                        isColorSourceValid,
                        checkRequiredBounds,
                        out selectedColorSource);
                if (found)
                {
                    return Direct3D9Factory.SuccessHResult;
                }

                return Direct3D9BitmapColorSourceChooser.Choose(
                    cacheEntries,
                    cacheLifetime,
                    bitmapSourceNoReference,
                    ref selectedProperties,
                    contextParameters,
                    reusableCandidates,
                    isColorSourceValid,
                    isRenderTarget,
                    canStretchRectFromTextures,
                    createColorSource,
                    out selectedColorSource);
            },
            initializeColorSource,
            transferReusableCandidatesOnSuccess,
            release,
            out bitmapColorSource);
        properties = selectedProperties;
        return result;
    }

    internal static int ChooseOrSelectAndInitialize(
        Direct3D9BitmapCacheLifetime cacheLifetime,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        Direct3D9SelectBitmapColorSource selectColorSource,
        Direct3D9InitializeBitmapColorSource initializeColorSource,
        Action<nint> release,
        out nint bitmapColorSource)
    {
        ArgumentNullException.ThrowIfNull(createReusableCandidates);

        Direct3D9BitmapReusableRealizationCandidates reusableCandidates =
            createReusableCandidates()
            ?? throw new InvalidOperationException("Reusable realization candidate creation returned null.");

        return SelectAndInitialize(
            reusableCandidates,
            (out nint selectedColorSource) =>
            {
                if (Direct3D9BitmapColorSourceCacheCoordinator.TryChooseDeviceBitmapOrLastUsed(
                        cacheLifetime,
                        contextParameters,
                        realizationBounds,
                        isDeviceBitmap,
                        reusableCandidates,
                        isColorSourceValid,
                        checkRequiredBounds,
                        out selectedColorSource))
                {
                    return Direct3D9Factory.SuccessHResult;
                }

                return selectColorSource(out selectedColorSource);
            },
            initializeColorSource,
            false,
            release,
            out bitmapColorSource);
    }

    internal static int SelectAndInitialize(
        Direct3D9BitmapReusableRealizationCandidates reusableCandidates,
        Direct3D9SelectBitmapColorSource selectColorSource,
        Direct3D9InitializeBitmapColorSource initializeColorSource,
        Action<nint> release,
        out nint bitmapColorSource) =>
        SelectAndInitialize(
            reusableCandidates,
            selectColorSource,
            initializeColorSource,
            false,
            release,
            out bitmapColorSource);

    private static int SelectAndInitialize(
        Direct3D9BitmapReusableRealizationCandidates reusableCandidates,
        Direct3D9SelectBitmapColorSource selectColorSource,
        Direct3D9InitializeBitmapColorSource initializeColorSource,
        bool transferReusableCandidatesOnSuccess,
        Action<nint> release,
        out nint bitmapColorSource)
    {
        ArgumentNullException.ThrowIfNull(reusableCandidates);
        ArgumentNullException.ThrowIfNull(selectColorSource);
        ArgumentNullException.ThrowIfNull(initializeColorSource);
        ArgumentNullException.ThrowIfNull(release);

        bitmapColorSource = 0;
        nint selectedColorSource = 0;

        try
        {
            int result = selectColorSource(out selectedColorSource);
            if (result < 0)
            {
                return result;
            }

            if (selectedColorSource == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            result = initializeColorSource(selectedColorSource, reusableCandidates);
            if (result < 0)
            {
                return result;
            }

            bitmapColorSource = selectedColorSource;
            selectedColorSource = 0;
            if (transferReusableCandidatesOnSuccess)
            {
                reusableCandidates = null!;
            }
            return result;
        }
        finally
        {
            try
            {
                reusableCandidates?.Dispose();
            }
            finally
            {
                if (selectedColorSource != 0)
                {
                    release(selectedColorSource);
                }
            }
        }
    }
}
