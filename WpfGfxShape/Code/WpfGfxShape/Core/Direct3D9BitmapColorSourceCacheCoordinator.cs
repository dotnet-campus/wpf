using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9ResolveBitmapCache(out nint bitmapCache);
internal delegate int Direct3D9ChooseCachedBitmapColorSource(
    nint bitmapCache,
    out nint bitmapColorSource,
    out nint reusableRealizationSource);
internal delegate int Direct3D9CreateUncachedBitmapColorSource(out nint bitmapColorSource);
internal delegate Direct3D9BitmapReusableRealizationCandidates Direct3D9CreateBitmapReusableRealizationCandidates();

internal static class Direct3D9BitmapColorSourceCacheCoordinator
{
    internal static bool TryChooseDeviceBitmapOrLastUsed(
        Direct3D9BitmapCacheLifetime cacheLifetime,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9CreateBitmapReusableRealizationCandidates createReusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        out nint bitmapColorSource,
        out nint reusableRealizationSources)
    {
        ArgumentNullException.ThrowIfNull(createReusableCandidates);

        bitmapColorSource = 0;
        reusableRealizationSources = 0;
        using Direct3D9BitmapReusableRealizationCandidates reusableCandidates =
            createReusableCandidates()
            ?? throw new InvalidOperationException("Reusable realization candidate creation returned null.");

        bool found = TryChooseDeviceBitmapOrLastUsed(
            cacheLifetime,
            contextParameters,
            realizationBounds,
            isDeviceBitmap,
            reusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            out bitmapColorSource);
        if (found)
        {
            reusableRealizationSources = reusableCandidates.Detach();
        }

        return found;
    }

    internal static bool TryChooseDeviceBitmapOrLastUsed(
        Direct3D9BitmapCacheLifetime cacheLifetime,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        Direct3D9BitmapRealizationProperties properties,
        bool isDeviceBitmap,
        Direct3D9BitmapReusableRealizationCandidates reusableCandidates,
        Direct3D9ComputeDeviceBitmapMinimumRealizationBounds computeMinimumRealizationBounds,
        Func<nint, Direct3D9BitmapRealizationRectangle, bool> containsValidArea,
        Direct3D9GetBitmapSize getBitmapSize,
        bool supportsConditionalNonPowerOfTwoTextures,
        bool supportsUnconditionalNonPowerOfTwoTextures,
        Action<nint, Textureaddress, Textureaddress> setWrapModes,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        out nint bitmapColorSource)
    {
        ArgumentNullException.ThrowIfNull(cacheLifetime);
        ArgumentNullException.ThrowIfNull(realizationBounds);
        ArgumentNullException.ThrowIfNull(reusableCandidates);
        ArgumentNullException.ThrowIfNull(isColorSourceValid);
        ArgumentNullException.ThrowIfNull(checkRequiredBounds);

        if (Direct3D9BitmapColorSourceChooser.TryChooseDeviceBitmap(
                cacheLifetime,
                contextParameters,
                realizationBounds,
                properties,
                isDeviceBitmap,
                computeMinimumRealizationBounds,
                containsValidArea,
                getBitmapSize,
                supportsConditionalNonPowerOfTwoTextures,
                supportsUnconditionalNonPowerOfTwoTextures,
                setWrapModes,
                checkRequiredBounds,
                out bitmapColorSource))
        {
            return true;
        }

        return Direct3D9BitmapColorSourceChooser.TryChooseLastUsed(
            cacheLifetime,
            contextParameters,
            realizationBounds,
            isDeviceBitmap,
            reusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            out bitmapColorSource);
    }

    internal static bool TryChooseDeviceBitmapOrLastUsed(
        Direct3D9BitmapCacheLifetime cacheLifetime,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9BitmapReusableRealizationCandidates reusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        out nint bitmapColorSource)
    {
        ArgumentNullException.ThrowIfNull(cacheLifetime);
        ArgumentNullException.ThrowIfNull(realizationBounds);
        ArgumentNullException.ThrowIfNull(reusableCandidates);
        ArgumentNullException.ThrowIfNull(isColorSourceValid);
        ArgumentNullException.ThrowIfNull(checkRequiredBounds);

        if (Direct3D9BitmapColorSourceChooser.TryChooseCachedDeviceBitmap(
                cacheLifetime,
                contextParameters,
                realizationBounds,
                isDeviceBitmap,
                checkRequiredBounds,
                out bitmapColorSource))
        {
            return true;
        }

        return Direct3D9BitmapColorSourceChooser.TryChooseLastUsed(
            cacheLifetime,
            contextParameters,
            realizationBounds,
            isDeviceBitmap,
            reusableCandidates,
            isColorSourceValid,
            checkRequiredBounds,
            out bitmapColorSource);
    }

    internal static int Get(
        nint bitmapCacheFromBitmap,
        Action<nint> addReference,
        Direct3D9ResolveBitmapCache resolveBitmapCache,
        Direct3D9ChooseCachedBitmapColorSource chooseBitmapColorSource,
        Direct3D9CreateUncachedBitmapColorSource createUncachedBitmapColorSource,
        Action<nint> release,
        out nint bitmapColorSource,
        out nint reusableRealizationSource)
    {
        ArgumentNullException.ThrowIfNull(addReference);
        ArgumentNullException.ThrowIfNull(resolveBitmapCache);
        ArgumentNullException.ThrowIfNull(chooseBitmapColorSource);
        ArgumentNullException.ThrowIfNull(createUncachedBitmapColorSource);
        ArgumentNullException.ThrowIfNull(release);

        bitmapColorSource = 0;
        reusableRealizationSource = 0;

        nint bitmapCache = bitmapCacheFromBitmap;
        int result;
        if (bitmapCache != 0)
        {
            addReference(bitmapCache);
            result = Direct3D9Factory.SuccessHResult;
        }
        else
        {
            result = resolveBitmapCache(out bitmapCache);
        }

        try
        {
            if (result >= 0)
            {
                if (bitmapCache == 0)
                {
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return chooseBitmapColorSource(
                    bitmapCache,
                    out bitmapColorSource,
                    out reusableRealizationSource);
            }

            reusableRealizationSource = 0;
            return createUncachedBitmapColorSource(out bitmapColorSource);
        }
        finally
        {
            if (bitmapCache != 0)
            {
                release(bitmapCache);
            }
        }
    }
}
