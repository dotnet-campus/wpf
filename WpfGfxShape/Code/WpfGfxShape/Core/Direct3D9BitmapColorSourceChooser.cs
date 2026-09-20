using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9CreateCachedBitmapColorSource(
    bool createAsRenderTarget,
    out nint bitmapColorSource);

internal delegate bool Direct3D9CheckBitmapColorSourceRequiredBounds(
    nint bitmapColorSource,
    Direct3D9DelayedBounds realizationBounds,
    MilBitmapInterpolationMode interpolationMode,
    MilBitmapWrapMode wrapMode,
    Direct3D9BitmapRequiredBoundsCheck check);

internal delegate bool Direct3D9ComputeDeviceBitmapMinimumRealizationBounds(
    Direct3D9DelayedBounds realizationBounds,
    Direct3D9BitmapRealizationProperties properties,
    ref Direct3D9BitmapRealizationRectangle minimumBounds);

internal static class Direct3D9BitmapColorSourceChooser
{
    internal static bool TryChooseDeviceBitmap(
        Direct3D9BitmapCacheLifetime cacheLifetime,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        Direct3D9BitmapRealizationProperties properties,
        bool isDeviceBitmap,
        Direct3D9ComputeDeviceBitmapMinimumRealizationBounds computeMinimumRealizationBounds,
        Func<nint, Direct3D9BitmapRealizationRectangle, bool> containsValidArea,
        Direct3D9GetBitmapSize getBitmapSize,
        bool supportsConditionalNonPowerOfTwoTextures,
        bool supportsUnconditionalNonPowerOfTwoTextures,
        Action<nint, Textureaddress, Textureaddress> setWrapModes,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        out nint bitmapColorSource)
    {
        ArgumentNullException.ThrowIfNull(cacheLifetime);
        ArgumentNullException.ThrowIfNull(realizationBounds);
        ArgumentNullException.ThrowIfNull(computeMinimumRealizationBounds);
        ArgumentNullException.ThrowIfNull(containsValidArea);
        ArgumentNullException.ThrowIfNull(getBitmapSize);
        ArgumentNullException.ThrowIfNull(setWrapModes);
        ArgumentNullException.ThrowIfNull(checkRequiredBounds);

        bitmapColorSource = 0;
        nint deviceBitmapColorSource = cacheLifetime.DeviceBitmapColorSource;
        if (deviceBitmapColorSource == 0)
        {
            return false;
        }

        if (!isDeviceBitmap)
        {
            if (!checkRequiredBounds(
                    deviceBitmapColorSource,
                    realizationBounds,
                    contextParameters.InterpolationMode,
                    contextParameters.WrapMode,
                    Direct3D9BitmapRequiredBoundsCheck.Cached))
            {
                return false;
            }

            bitmapColorSource = cacheLifetime.AcquireDeviceBitmapColorSource();
            return true;
        }

        Direct3D9BitmapRealizationRectangle requiredBounds =
            new(0, 0, properties.Width, properties.Height);
        if (!computeMinimumRealizationBounds(
                realizationBounds,
                properties,
                ref requiredBounds)
            || !containsValidArea(cacheLifetime.BitmapNoReference, requiredBounds))
        {
            return false;
        }

        int result = getBitmapSize(cacheLifetime.BitmapNoReference, out uint width, out uint height);
        if (result < 0)
        {
            return false;
        }

        (Textureaddress addressU, Textureaddress addressV) = contextParameters.WrapMode switch
        {
            MilBitmapWrapMode.Extend => (Textureaddress.Clamp, Textureaddress.Clamp),
            MilBitmapWrapMode.FlipX => (Textureaddress.Mirror, Textureaddress.Wrap),
            MilBitmapWrapMode.FlipY => (Textureaddress.Wrap, Textureaddress.Mirror),
            MilBitmapWrapMode.FlipXY => (Textureaddress.Mirror, Textureaddress.Mirror),
            MilBitmapWrapMode.Tile => (Textureaddress.Wrap, Textureaddress.Wrap),
            MilBitmapWrapMode.Border => (Textureaddress.Border, Textureaddress.Border),
            _ => throw new ArgumentOutOfRangeException(nameof(contextParameters))
        };

        bool dimensionsArePowerOfTwo = IsPowerOfTwo(width) && IsPowerOfTwo(height);
        bool conditionalNonPowerOfTwoUseIsValid = supportsConditionalNonPowerOfTwoTextures
            && addressU == Textureaddress.Clamp
            && addressV == Textureaddress.Clamp;
        if (!dimensionsArePowerOfTwo
            && !conditionalNonPowerOfTwoUseIsValid
            && !supportsUnconditionalNonPowerOfTwoTextures)
        {
            return false;
        }

        setWrapModes(deviceBitmapColorSource, addressU, addressV);
        bitmapColorSource = cacheLifetime.AcquireDeviceBitmapColorSource();
        return true;
    }

    private static bool IsPowerOfTwo(uint value) =>
        value != 0 && (value & (value - 1)) == 0;

    internal static bool TryChooseCachedDeviceBitmap(
        Direct3D9BitmapCacheLifetime cacheLifetime,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9DelayedBounds realizationBounds,
        bool isDeviceBitmap,
        Direct3D9CheckBitmapColorSourceRequiredBounds checkRequiredBounds,
        out nint bitmapColorSource)
    {
        ArgumentNullException.ThrowIfNull(cacheLifetime);
        ArgumentNullException.ThrowIfNull(realizationBounds);
        ArgumentNullException.ThrowIfNull(checkRequiredBounds);

        bitmapColorSource = 0;
        nint deviceBitmapColorSource = cacheLifetime.DeviceBitmapColorSource;
        if (isDeviceBitmap || deviceBitmapColorSource == 0)
        {
            return false;
        }

        if (!checkRequiredBounds(
                deviceBitmapColorSource,
                realizationBounds,
                contextParameters.InterpolationMode,
                contextParameters.WrapMode,
                Direct3D9BitmapRequiredBoundsCheck.Cached))
        {
            return false;
        }

        bitmapColorSource = cacheLifetime.AcquireDeviceBitmapColorSource();
        return true;
    }

    internal static bool TryChooseLastUsed(
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

        bitmapColorSource = 0;
        nint lastUsedColorSource = cacheLifetime.LastUsedColorSource;
        if (lastUsedColorSource == 0)
        {
            return false;
        }

        Direct3D9BitmapColorSourceContextParameters lastUsedContext =
            cacheLifetime.LastUsedContextParameters;
        if (lastUsedContext.BitmapBrushUniqueness != contextParameters.BitmapBrushUniqueness
            || lastUsedContext.PrefilterEnabled != contextParameters.PrefilterEnabled
            || Direct3D9BitmapColorSourceContextParameters.UsesMipMapping(lastUsedContext.InterpolationMode)
                != Direct3D9BitmapColorSourceContextParameters.UsesMipMapping(contextParameters.InterpolationMode)
            || lastUsedContext.BitmapBrush != contextParameters.BitmapBrush
            || lastUsedContext.RenderTargetFormat != contextParameters.RenderTargetFormat
            || lastUsedContext.WrapMode != contextParameters.WrapMode)
        {
            return false;
        }

        Direct3D9BitmapRequiredBoundsCheck check = isDeviceBitmap
            ? Direct3D9BitmapRequiredBoundsCheck.PossibleAndUpdateRequired
            : Direct3D9BitmapRequiredBoundsCheck.Required;
        if (!checkRequiredBounds(
                lastUsedColorSource,
                realizationBounds,
                contextParameters.InterpolationMode,
                contextParameters.WrapMode,
                check))
        {
            return false;
        }

        bitmapColorSource = cacheLifetime.AcquireLastUsedColorSource();
        nint deviceBitmapColorSource = cacheLifetime.DeviceBitmapColorSource;
        if (deviceBitmapColorSource != 0 && isColorSourceValid(deviceBitmapColorSource))
        {
            reusableCandidates.Add(deviceBitmapColorSource);
        }

        return true;
    }

    internal static int Choose(
        Direct3D9BitmapFormatCacheEntry cacheEntries,
        Direct3D9BitmapCacheLifetime cacheLifetime,
        nint bitmapSourceNoReference,
        ref Direct3D9BitmapRealizationProperties properties,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9BitmapReusableRealizationCandidates reusableCandidates,
        Func<nint, bool> isColorSourceValid,
        Func<nint, bool> isRenderTarget,
        bool canStretchRectFromTextures,
        Direct3D9CreateCachedBitmapColorSource createColorSource,
        out nint bitmapColorSource)
    {
        ArgumentNullException.ThrowIfNull(cacheEntries);
        ArgumentNullException.ThrowIfNull(cacheLifetime);
        ArgumentNullException.ThrowIfNull(reusableCandidates);
        ArgumentNullException.ThrowIfNull(isColorSourceValid);
        ArgumentNullException.ThrowIfNull(isRenderTarget);
        ArgumentNullException.ThrowIfNull(createColorSource);

        bitmapColorSource = 0;
        cacheLifetime.AssociateBitmapSource(bitmapSourceNoReference);

        bool found = cacheEntries.TryAcquire(
            ref properties,
            isColorSourceValid,
            reusableCandidates,
            out bitmapColorSource);

        nint deviceBitmapColorSource = cacheLifetime.DeviceBitmapColorSource;
        if (deviceBitmapColorSource != 0
            && !HasBorder(properties.LayoutU.TexelLayout)
            && !HasBorder(properties.LayoutV.TexelLayout)
            && isColorSourceValid(deviceBitmapColorSource))
        {
            reusableCandidates.Add(deviceBitmapColorSource);
        }

        if (!found)
        {
            nint reusableSource = reusableCandidates.Head;
            bool createAsRenderTarget = reusableSource != 0
                && (canStretchRectFromTextures || isRenderTarget(reusableSource));

            int result = createColorSource(createAsRenderTarget, out bitmapColorSource);
            if (result < 0)
            {
                return result;
            }

            if (bitmapColorSource == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            cacheEntries.Store(properties, bitmapColorSource);
        }

        if (cacheLifetime.LastUsedColorSource != bitmapColorSource)
        {
            bool retainAsLastUsed = !contextParameters.PrefilterEnabled
                || Direct3D9BitmapColorSourceContextParameters.UsesMipMapping(contextParameters.InterpolationMode);
            cacheLifetime.SetLastUsedColorSource(
                retainAsLastUsed ? bitmapColorSource : 0,
                contextParameters);
        }
        else
        {
            cacheLifetime.SetLastUsedColorSource(bitmapColorSource, contextParameters);
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private static bool HasBorder(Direct3D9TexelLayout layout) =>
        layout is Direct3D9TexelLayout.EdgeWrapped or Direct3D9TexelLayout.EdgeMirrored;
}
