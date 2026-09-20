using System.Numerics;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal enum MilBitmapInterpolationMode
{
    NearestNeighbor = 0,
    Linear = 1,
    Cubic = 2,
    Fant = 3,
    TriLinear = 4,
    Anisotropic = 5,
    Last = 6
}

internal enum MilBitmapWrapMode
{
    Extend = 0,
    FlipX = 1,
    FlipY = 2,
    FlipXY = 3,
    Tile = 4,
    Border = 5
}

internal readonly record struct Direct3D9BitmapColorSourceContextParameters(
    nint BitmapBrush,
    MilBitmapInterpolationMode InterpolationMode,
    bool PrefilterEnabled,
    MilPixelFormat RenderTargetFormat,
    uint BitmapBrushUniqueness,
    MilBitmapWrapMode WrapMode)
{
    internal static Direct3D9BitmapColorSourceContextParameters FromBrushAndContext(
        nint bitmapBrush,
        MilBitmapInterpolationMode interpolationMode,
        bool prefilterEnabled,
        MilPixelFormat renderTargetFormat,
        uint bitmapBrushUniqueness,
        MilBitmapWrapMode wrapMode,
        bool isSoftwareDevice,
        bool canAutoGenerateMipmaps,
        bool canGenerateMipmapsWithStretchRect,
        bool isFantScalerDisabled)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmapBrush);

        if (prefilterEnabled && isFantScalerDisabled)
        {
            prefilterEnabled = false;
        }

        if (UsesMipMapping(interpolationMode)
            && (isSoftwareDevice || !(canAutoGenerateMipmaps || canGenerateMipmapsWithStretchRect)))
        {
            interpolationMode = MilBitmapInterpolationMode.Linear;
        }

        return new Direct3D9BitmapColorSourceContextParameters(
            bitmapBrush,
            interpolationMode,
            prefilterEnabled,
            renderTargetFormat,
            bitmapBrushUniqueness,
            wrapMode);
    }

    internal static Direct3D9BitmapColorSourceContextParameters FromExplicitSettings(
        MilBitmapInterpolationMode interpolationMode,
        bool prefilterEnabled,
        MilPixelFormat renderTargetFormat,
        MilBitmapWrapMode wrapMode) =>
        new(
            0,
            interpolationMode,
            prefilterEnabled,
            renderTargetFormat,
            0,
            wrapMode);

    internal static bool UsesMipMapping(MilBitmapInterpolationMode interpolationMode) =>
        interpolationMode is MilBitmapInterpolationMode.TriLinear or MilBitmapInterpolationMode.Anisotropic;
}

internal enum Direct3D9TextureMipMapLevel
{
    One,
    All
}

internal readonly record struct Direct3D9BitmapRealizationRectangle(
    uint Left,
    uint Top,
    uint Right,
    uint Bottom)
{
    internal uint Width => Right - Left;

    internal uint Height => Bottom - Top;
}

internal enum Direct3D9TexelLayout
{
    Natural,
    CenterSplit,
    EdgeWrapped,
    EdgeMirrored,
    FirstOnly
}

internal readonly record struct Direct3D9BitmapDimensionLayout(
    uint Length,
    Direct3D9TexelLayout TexelLayout,
    Textureaddress TextureAddress);

internal readonly record struct Direct3D9BitmapTextureRequirements(
    SurfaceDesc Description,
    uint Levels);

internal readonly record struct Direct3D9BitmapRealizationProperties(
    MilBitmapInterpolationMode InterpolationMode,
    Direct3D9TextureMipMapLevel MipMapLevel,
    MilBitmapWrapMode WrapMode,
    MilPixelFormat TextureFormat,
    bool IsMinimumRealizationRectComputed,
    uint BitmapWidth,
    uint BitmapHeight,
    uint Width,
    uint Height)
{
    internal bool OnlyContainsSubRectangleOfSource { get; init; }

    internal Direct3D9BitmapRealizationRectangle SourceContained { get; init; }

    internal Direct3D9BitmapDimensionLayout LayoutU { get; init; }

    internal Direct3D9BitmapDimensionLayout LayoutV { get; init; }
}

internal delegate int Direct3D9GetBitmapPixelFormat(nint bitmapSource, out MilPixelFormat pixelFormat);

internal delegate int Direct3D9GetSupportedBitmapTextureFormat(
    MilPixelFormat bitmapSourceFormat,
    MilPixelFormat renderTargetFormat,
    bool forceAlpha,
    out MilPixelFormat textureFormat);

internal delegate int Direct3D9GetBitmapSize(nint bitmapSource, out uint width, out uint height);

internal delegate int Direct3D9CreateDeviceBitmapTexture(
    bool isEvictable,
    bool returnSharedHandle,
    ref nint sharedHandle,
    out nint texture);

internal delegate void Direct3D9SetDeviceBitmapContext(
    nint bitmap,
    Direct3D9BitmapRealizationProperties realizationProperties);

internal readonly record struct Direct3D9DeviceBitmapInitialization(
    uint BitmapWidth,
    uint BitmapHeight,
    nint Texture,
    nint SharedHandle);

internal delegate void Direct3D9ComputePrefilteringDimensions(
    nint bitmapToIdealRealization,
    uint bitmapWidth,
    uint bitmapHeight,
    float prefilterThreshold,
    out uint width,
    out uint height);

internal delegate bool Direct3D9ComputeMinimumRealizationBounds(
    nint realizationBounds,
    Direct3D9BitmapRealizationProperties properties,
    ref Direct3D9BitmapRealizationRectangle minimumBounds);

internal delegate int Direct3D9ComputeBitmapRealizationSize(
    uint maximumTextureWidth,
    uint maximumTextureHeight,
    nint realizationBounds,
    nint bitmapToIdealRealization,
    MilBitmapWrapMode wrapMode,
    bool prefilterEnabled,
    float prefilterThreshold,
    bool canFallback,
    ref Direct3D9BitmapRealizationProperties properties);

internal static class Direct3D9BitmapRealizationParameterComputer
{
    internal static int ComputeTextureProperties(
        nint bitmapSource,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        bool canFallback,
        Direct3D9GetBitmapPixelFormat getBitmapPixelFormat,
        Direct3D9GetSupportedBitmapTextureFormat getSupportedTextureFormat,
        out Direct3D9BitmapRealizationProperties properties)
    {
        ArgumentNullException.ThrowIfNull(getBitmapPixelFormat);
        ArgumentNullException.ThrowIfNull(getSupportedTextureFormat);

        properties = new Direct3D9BitmapRealizationProperties(
            contextParameters.InterpolationMode,
            Direct3D9BitmapColorSourceContextParameters.UsesMipMapping(contextParameters.InterpolationMode)
                ? Direct3D9TextureMipMapLevel.All
                : Direct3D9TextureMipMapLevel.One,
            contextParameters.WrapMode,
            MilPixelFormat.Undefined,
            IsMinimumRealizationRectComputed: false,
            BitmapWidth: 0,
            BitmapHeight: 0,
            Width: 0,
            Height: 0);

        int result = getBitmapPixelFormat(bitmapSource, out MilPixelFormat bitmapSourceFormat);
        if (result < 0)
        {
            return result;
        }

        result = getSupportedTextureFormat(
            bitmapSourceFormat,
            contextParameters.RenderTargetFormat,
            contextParameters.WrapMode == MilBitmapWrapMode.Border,
            out MilPixelFormat textureFormat);
        if (result < 0)
        {
            return canFallback ? Direct3D9Factory.NotImplementedHResult : result;
        }

        properties = properties with { TextureFormat = textureFormat };
        return Direct3D9Factory.SuccessHResult;
    }

    internal static int ComputeTextureSize(
        nint bitmapSource,
        uint maximumTextureWidth,
        uint maximumTextureHeight,
        Direct3D9BitmapRealizationContext realizationContext,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9GetBitmapSize getBitmapSize,
        Direct3D9ComputeBitmapRealizationSize computeRealizationSize,
        ref Direct3D9BitmapRealizationProperties properties)
    {
        ArgumentNullException.ThrowIfNull(getBitmapSize);
        ArgumentNullException.ThrowIfNull(computeRealizationSize);

        int result = getBitmapSize(bitmapSource, out uint bitmapWidth, out uint bitmapHeight);
        if (result < 0)
        {
            return result;
        }

        properties = properties with
        {
            BitmapWidth = bitmapWidth,
            BitmapHeight = bitmapHeight
        };

        return computeRealizationSize(
            maximumTextureWidth,
            maximumTextureHeight,
            realizationContext.RealizationBounds,
            realizationContext.BitmapToIdealRealization,
            contextParameters.WrapMode,
            contextParameters.PrefilterEnabled,
            realizationContext.PrefilterThreshold,
            realizationContext.CanFallback,
            ref properties);
    }

    internal static void ComputeMipMappedTextureSize(
        uint maximumTextureWidth,
        uint maximumTextureHeight,
        MilBitmapWrapMode wrapMode,
        bool prefilterEnabled,
        ref Direct3D9BitmapRealizationProperties properties)
    {
        properties = properties with
        {
            Width = ComputeMipMappedDimension(
                properties.BitmapWidth,
                maximumTextureWidth,
                wrapMode,
                prefilterEnabled),
            Height = ComputeMipMappedDimension(
                properties.BitmapHeight,
                maximumTextureHeight,
                wrapMode,
                prefilterEnabled),
            IsMinimumRealizationRectComputed = true
        };
    }

    internal static void ComputeNonMipMappedTextureSize(
        nint bitmapToIdealRealization,
        bool prefilterEnabled,
        float prefilterThreshold,
        Direct3D9ComputePrefilteringDimensions computePrefilteringDimensions,
        ref Direct3D9BitmapRealizationProperties properties)
    {
        ArgumentNullException.ThrowIfNull(computePrefilteringDimensions);

        uint width = properties.BitmapWidth;
        uint height = properties.BitmapHeight;

        if (prefilterEnabled)
        {
            computePrefilteringDimensions(
                bitmapToIdealRealization,
                properties.BitmapWidth,
                properties.BitmapHeight,
                prefilterThreshold,
                out width,
                out height);
        }

        properties = properties with
        {
            Width = width,
            Height = height
        };
    }

    internal static int ComputeMinimumTextureBounds(
        uint maximumTextureWidth,
        uint maximumTextureHeight,
        nint realizationBounds,
        bool prefilterEnabled,
        bool canFallback,
        Direct3D9ComputeMinimumRealizationBounds computeMinimumRealizationBounds,
        ref Direct3D9BitmapRealizationProperties properties)
    {
        ArgumentNullException.ThrowIfNull(computeMinimumRealizationBounds);

        Direct3D9BitmapRealizationRectangle sourceContained = new(0, 0, properties.Width, properties.Height);
        properties = properties with
        {
            OnlyContainsSubRectangleOfSource = false,
            SourceContained = sourceContained
        };

        if (properties.Width <= maximumTextureWidth && properties.Height <= maximumTextureHeight)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        properties = properties with { IsMinimumRealizationRectComputed = true };
        bool foundAlternate = computeMinimumRealizationBounds(realizationBounds, properties, ref sourceContained);
        properties = properties with { SourceContained = sourceContained };

        bool exceedsTextureLimits = !foundAlternate
            || sourceContained.Width > maximumTextureWidth
            || sourceContained.Height > maximumTextureHeight;

        if (exceedsTextureLimits && canFallback && prefilterEnabled)
        {
            return Direct3D9Factory.NotImplementedHResult;
        }

        if (exceedsTextureLimits)
        {
            uint width = properties.Width;
            uint height = properties.Height;
            bool onlyContainsSubRectangleOfSource = false;

            if (sourceContained.Width > maximumTextureWidth)
            {
                width = maximumTextureWidth;
                sourceContained = sourceContained with
                {
                    Left = 0,
                    Right = width
                };
            }
            else if (sourceContained.Left > 0 || sourceContained.Right < width)
            {
                onlyContainsSubRectangleOfSource = true;
            }

            if (sourceContained.Height > maximumTextureHeight)
            {
                height = maximumTextureHeight;
                sourceContained = sourceContained with
                {
                    Top = 0,
                    Bottom = height
                };
            }
            else if (sourceContained.Top > 0 || sourceContained.Bottom < height)
            {
                onlyContainsSubRectangleOfSource = true;
            }

            properties = properties with
            {
                Width = width,
                Height = height,
                OnlyContainsSubRectangleOfSource = onlyContainsSubRectangleOfSource,
                SourceContained = sourceContained
            };
        }
        else
        {
            properties = properties with { OnlyContainsSubRectangleOfSource = true };
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal static int InitializeTextureLayoutAndWrapping(
        MilBitmapWrapMode wrapMode,
        bool requiresPowerOfTwoTextures,
        uint maximumTextureWidth,
        uint maximumTextureHeight,
        bool canFallback,
        ref Direct3D9BitmapRealizationProperties properties)
    {
        (Textureaddress addressU, Textureaddress addressV) = ConvertWrapModeToTextureAddressModes(wrapMode);
        Direct3D9BitmapDimensionLayout layoutU = new(
            properties.SourceContained.Width,
            Direct3D9TexelLayout.Natural,
            addressU);
        Direct3D9BitmapDimensionLayout layoutV = new(
            properties.SourceContained.Height,
            Direct3D9TexelLayout.Natural,
            addressV);

        if (Direct3D9BitmapColorSourceContextParameters.UsesMipMapping(properties.InterpolationMode))
        {
            layoutU = AdjustMipMappedLayout(layoutU);
            layoutV = AdjustMipMappedLayout(layoutV);
        }
        else if (requiresPowerOfTwoTextures)
        {
            int result = AdjustNonPowerOfTwoLayout(
                maximumTextureWidth,
                canFallback,
                isHorizontal: true,
                ref layoutU,
                ref properties);
            if (result < 0)
            {
                properties = properties with { LayoutU = layoutU, LayoutV = layoutV };
                return result;
            }

            result = AdjustNonPowerOfTwoLayout(
                maximumTextureHeight,
                canFallback,
                isHorizontal: false,
                ref layoutV,
                ref properties);
            if (result < 0)
            {
                properties = properties with { LayoutU = layoutU, LayoutV = layoutV };
                return result;
            }
        }

        properties = properties with
        {
            LayoutU = layoutU,
            LayoutV = layoutV
        };

        if (requiresPowerOfTwoTextures
            && !Direct3D9BitmapColorSourceContextParameters.UsesMipMapping(properties.InterpolationMode))
        {
            return ReconcileLayouts(maximumTextureWidth, maximumTextureHeight, ref properties);
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal static int ReconcileLayouts(
        uint maximumTextureWidth,
        uint maximumTextureHeight,
        ref Direct3D9BitmapRealizationProperties properties)
    {
        Direct3D9BitmapDimensionLayout layoutU = properties.LayoutU;
        Direct3D9BitmapDimensionLayout layoutV = properties.LayoutV;
        int result = Direct3D9Factory.SuccessHResult;

        if (layoutU.TexelLayout == Direct3D9TexelLayout.Natural
            && layoutV.TexelLayout != Direct3D9TexelLayout.Natural)
        {
            result = AdjustLayoutForConditionalNonPowerOfTwo(maximumTextureWidth, ref layoutU);
        }
        else if (layoutV.TexelLayout == Direct3D9TexelLayout.Natural
                 && layoutU.TexelLayout != Direct3D9TexelLayout.Natural)
        {
            result = AdjustLayoutForConditionalNonPowerOfTwo(maximumTextureHeight, ref layoutV);
        }

        properties = properties with
        {
            LayoutU = layoutU,
            LayoutV = layoutV
        };

        return result;
    }

    internal static SurfaceDesc GetRequiredTextureDescription(
        bool canAutoGenerateMipmaps,
        Direct3D9BitmapRealizationProperties properties,
        out uint levels)
    {
        uint usage;
        if (properties.MipMapLevel == Direct3D9TextureMipMapLevel.One)
        {
            usage = 0;
            levels = 1;
        }
        else if (canAutoGenerateMipmaps)
        {
            usage = D3D9.UsageAutogenmipmap;
            levels = 0;
        }
        else
        {
            usage = D3D9.UsageRendertarget;
            levels = (uint) System.Numerics.BitOperations.Log2(
                Math.Max(properties.LayoutU.Length, properties.LayoutV.Length)) + 1;
        }

        return new SurfaceDesc(
            format: ToDirect3DFormat(properties.TextureFormat),
            type: Resourcetype.Texture,
            usage: usage,
            pool: Pool.Default,
            multiSampleType: MultisampleType.MultisampleNone,
            multiSampleQuality: 0,
            width: properties.LayoutU.Length,
            height: properties.LayoutV.Length);
    }

    internal static Direct3D9BitmapTextureRequirements GetTextureCreationRequirements(
        bool canAutoGenerateMipmaps,
        bool createAsRenderTarget,
        Direct3D9BitmapRealizationProperties properties)
    {
        SurfaceDesc description = GetRequiredTextureDescription(
            canAutoGenerateMipmaps,
            properties,
            out uint levels);

        if (createAsRenderTarget)
        {
            description.Usage |= D3D9.UsageRendertarget;
        }

        return new Direct3D9BitmapTextureRequirements(description, levels);
    }

    internal static int ValidateDeviceBitmapTextureRequirements(
        uint maximumTextureWidth,
        uint maximumTextureHeight,
        Direct3D9BitmapTextureRequirements requirements) =>
        requirements.Description.Width > maximumTextureWidth
        || requirements.Description.Height > maximumTextureHeight
            ? Direct3D9Factory.MaximumTextureSizeExceededHResult
            : Direct3D9Factory.SuccessHResult;

    internal static int InitializeDeviceBitmapColorSource(
        nint bitmap,
        Direct3D9BitmapRealizationProperties realizationProperties,
        nint existingTexture,
        bool returnSharedHandle,
        Direct3D9GetBitmapSize getBitmapSize,
        Action<nint> addRef,
        Direct3D9CreateDeviceBitmapTexture createTexture,
        Direct3D9SetDeviceBitmapContext setBitmapAndContext,
        out Direct3D9DeviceBitmapInitialization initialization)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmap);
        ArgumentNullException.ThrowIfNull(getBitmapSize);
        ArgumentNullException.ThrowIfNull(addRef);
        ArgumentNullException.ThrowIfNull(createTexture);
        ArgumentNullException.ThrowIfNull(setBitmapAndContext);

        initialization = default;

        int result = getBitmapSize(bitmap, out uint bitmapWidth, out uint bitmapHeight);
        if (result < 0)
        {
            return result;
        }

        const uint SurfaceRectangleMaximum = (1u << 27) - 1;
        if (bitmapWidth > SurfaceRectangleMaximum || bitmapHeight > SurfaceRectangleMaximum)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        nint texture = existingTexture;
        nint sharedHandle = 0;

        if (texture != 0)
        {
            addRef(texture);
        }
        else
        {
            result = createTexture(
                isEvictable: false,
                returnSharedHandle,
                ref sharedHandle,
                out texture);
            if (result < 0)
            {
                return result;
            }
        }

        setBitmapAndContext(bitmap, realizationProperties);

        initialization = new Direct3D9DeviceBitmapInitialization(
            bitmapWidth,
            bitmapHeight,
            texture,
            returnSharedHandle ? sharedHandle : 0);
        return Direct3D9Factory.SuccessHResult;
    }

    internal static int AdjustLayoutForConditionalNonPowerOfTwo(
        uint maximumLength,
        ref Direct3D9BitmapDimensionLayout layout)
    {
        switch (layout.TextureAddress)
        {
            case Textureaddress.Wrap:
                if (layout.Length > maximumLength - Math.Min(maximumLength, 2u))
                {
                    return Direct3D9Factory.NotImplementedHResult;
                }

                layout = layout with
                {
                    Length = layout.Length + 2,
                    TexelLayout = Direct3D9TexelLayout.EdgeWrapped,
                    TextureAddress = Textureaddress.Clamp
                };
                break;

            case Textureaddress.Mirror:
                if (layout.Length > maximumLength - Math.Min(maximumLength, 2u))
                {
                    return Direct3D9Factory.NotImplementedHResult;
                }

                layout = layout with
                {
                    Length = layout.Length + 2,
                    TexelLayout = Direct3D9TexelLayout.EdgeMirrored,
                    TextureAddress = Textureaddress.Clamp
                };
                break;

            case Textureaddress.Clamp:
                layout = layout with { TexelLayout = Direct3D9TexelLayout.Natural };
                break;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private static int AdjustNonPowerOfTwoLayout(
        uint maximumLength,
        bool canFallback,
        bool isHorizontal,
        ref Direct3D9BitmapDimensionLayout layout,
        ref Direct3D9BitmapRealizationProperties properties)
    {
        if (IsPowerOfTwo(layout.Length))
        {
            return Direct3D9Factory.SuccessHResult;
        }

        uint realizationDimension = isHorizontal ? properties.Width : properties.Height;
        if (layout.Length != realizationDimension)
        {
            layout = layout with { TextureAddress = Textureaddress.Clamp };
            return Direct3D9Factory.SuccessHResult;
        }

        Textureaddress originalTextureAddress = layout.TextureAddress;
        int result = AdjustLayoutForConditionalNonPowerOfTwo(maximumLength, ref layout);
        if (result < 0 || canFallback || layout.TexelLayout == Direct3D9TexelLayout.Natural)
        {
            return result;
        }

        uint powerOfTwoDimension = RoundUpToPowerOfTwo(realizationDimension);
        layout = new Direct3D9BitmapDimensionLayout(
            powerOfTwoDimension,
            Direct3D9TexelLayout.Natural,
            originalTextureAddress);

        Direct3D9BitmapRealizationRectangle sourceContained = properties.SourceContained;
        if (isHorizontal)
        {
            properties = properties with
            {
                Width = powerOfTwoDimension,
                SourceContained = sourceContained with { Right = powerOfTwoDimension }
            };
        }
        else
        {
            properties = properties with
            {
                Height = powerOfTwoDimension,
                SourceContained = sourceContained with { Bottom = powerOfTwoDimension }
            };
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private static Direct3D9BitmapDimensionLayout AdjustMipMappedLayout(
        Direct3D9BitmapDimensionLayout layout)
    {
        if (IsPowerOfTwo(layout.Length))
        {
            return layout;
        }

        return layout with
        {
            Length = RoundUpToPowerOfTwo(layout.Length),
            TexelLayout = Direct3D9TexelLayout.FirstOnly
        };
    }

    private static (Textureaddress U, Textureaddress V) ConvertWrapModeToTextureAddressModes(
        MilBitmapWrapMode wrapMode) =>
        wrapMode switch
        {
            MilBitmapWrapMode.Extend => (Textureaddress.Clamp, Textureaddress.Clamp),
            MilBitmapWrapMode.FlipX => (Textureaddress.Mirror, Textureaddress.Wrap),
            MilBitmapWrapMode.FlipY => (Textureaddress.Wrap, Textureaddress.Mirror),
            MilBitmapWrapMode.FlipXY => (Textureaddress.Mirror, Textureaddress.Mirror),
            MilBitmapWrapMode.Tile => (Textureaddress.Wrap, Textureaddress.Wrap),
            MilBitmapWrapMode.Border => (Textureaddress.Border, Textureaddress.Border),
            _ => throw new ArgumentOutOfRangeException(nameof(wrapMode))
        };

    private static bool IsPowerOfTwo(uint value) =>
        value != 0 && (value & (value - 1)) == 0;

    private static uint ComputeMipMappedDimension(
        uint bitmapDimension,
        uint maximumTextureDimension,
        MilBitmapWrapMode wrapMode,
        bool prefilterEnabled)
    {
        if (bitmapDimension >= maximumTextureDimension)
        {
            return maximumTextureDimension;
        }

        if (!prefilterEnabled && wrapMode == MilBitmapWrapMode.Extend)
        {
            return bitmapDimension;
        }

        return RoundUpToPowerOfTwo(bitmapDimension);
    }

    private static uint RoundUpToPowerOfTwo(uint value)
    {
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }

    private static Format ToDirect3DFormat(MilPixelFormat pixelFormat) =>
        MilPixelFormatInfo.ToDirect3DFormat(pixelFormat);
}

internal readonly record struct Direct3D9BitmapRealizationContext(
    nint RealizationBounds,
    nint BitmapToIdealRealization,
    nint BitmapToXSpaceTransform,
    float PrefilterThreshold,
    bool CanFallback,
    nint AlternateCache);

internal delegate int Direct3D9RetrieveBitmapCache(
    nint bitmapSource,
    out nint bitmap,
    out nint bitmapCache);

internal delegate int Direct3D9ComputeBitmapRealizationParameters(
    nint bitmapSource,
    Direct3D9BitmapRealizationContext realizationContext,
    Direct3D9BitmapColorSourceContextParameters contextParameters,
    out nint realizationParameters);

internal delegate int Direct3D9GetBitmapColorSource(
    nint bitmapSource,
    nint bitmap,
    nint bitmapCache,
    nint realizationParameters,
    nint alternateCache,
    Direct3D9BitmapColorSourceContextParameters contextParameters,
    out nint bitmapColorSource,
    out nint reusableRealizationSource);

internal delegate int Direct3D9SetBitmapAndContext(
    nint bitmapColorSource,
    nint bitmapSource,
    nint realizationBounds,
    nint bitmapToXSpaceTransform,
    nint realizationParameters,
    nint reusableRealizationSource);

internal delegate int Direct3D9CreateBitmapPipelineColorSource(
    nint bitmapColorSource,
    out Direct3D9BitmapPipelineColorSource? pipelineColorSource);

internal delegate int Direct3D9ChooseAndCreateBitmapPipelineColorSource(
    nint bitmapSource,
    nint bitmap,
    nint bitmapCache,
    nint realizationParameters,
    nint alternateCache,
    Direct3D9BitmapColorSourceContextParameters contextParameters,
    out Direct3D9BitmapPipelineColorSource? pipelineColorSource);

internal sealed class Direct3D9BitmapPipelineColorSource : IDisposable
{
    private readonly IDisposable _realizationOwner;
    private readonly Direct3D9BitmapColorSourceRegistry? _registry;
    private readonly Direct3D9BitmapColorSourceOwner? _owner;
    private readonly Action<nint> _release;
    private nint _bitmapColorSource;
    private int _referenceCount = 1;

    internal Direct3D9BitmapPipelineColorSource(
        nint bitmapColorSource,
        IDisposable realizationOwner,
        Direct3D9PipelineColorSource pipelineColorSource,
        Action<nint> release,
        Direct3D9BitmapColorSourceRegistry? registry = null,
        bool isDeviceBitmap = false)
    {
        ArgumentOutOfRangeException.ThrowIfZero(bitmapColorSource);
        ArgumentNullException.ThrowIfNull(realizationOwner);
        ArgumentNullException.ThrowIfNull(pipelineColorSource);
        ArgumentNullException.ThrowIfNull(release);

        _bitmapColorSource = bitmapColorSource;
        _realizationOwner = realizationOwner;
        PipelineColorSource = pipelineColorSource;
        _release = release;

        if (registry is not null)
        {
            if (realizationOwner is not Direct3D9BitmapColorSourceTextureRealizer textureRealizer)
            {
                throw new ArgumentException("A registered bitmap color source must own a texture realizer.", nameof(realizationOwner));
            }

            _registry = registry;
            _owner = new Direct3D9BitmapColorSourceOwner(textureRealizer, isDeviceBitmap);
            registry.Register(bitmapColorSource, _owner);
        }
    }

    internal nint BitmapColorSource => _bitmapColorSource;

    internal Direct3D9PipelineColorSource PipelineColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource AddRef()
    {
        ObjectDisposedException.ThrowIf(_bitmapColorSource == 0, this);
        checked
        {
            _referenceCount++;
        }

        return this;
    }

    internal static Direct3D9BitmapPipelineColorSource Create(
        nint bitmapColorSource,
        Direct3D9BitmapColorSourceTextureRealizer textureRealizer,
        Direct3D9Device device,
        Direct3D9BitmapRealizationProperties properties,
        Matrix3x2 bitmapToXSpace,
        bool useHardwareTransform,
        uint? shaderTextureTransformRegister,
        Action<nint> release,
        Direct3D9SetPipelineShaderMatrix3x2? setShaderMatrix = null,
        Direct3D9BitmapColorSourceRegistry? registry = null,
        bool isDeviceBitmap = false)
    {
        ArgumentNullException.ThrowIfNull(textureRealizer);

        try
        {
            Direct3D9PipelineColorSource pipelineColorSource = textureRealizer.CreatePipelineColorSource(
                device,
                properties,
                bitmapToXSpace,
                useHardwareTransform,
                shaderTextureTransformRegister,
                setShaderMatrix);
            return new Direct3D9BitmapPipelineColorSource(
                bitmapColorSource,
                textureRealizer,
                pipelineColorSource,
                release,
                registry,
                isDeviceBitmap);
        }
        catch
        {
            textureRealizer.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (_bitmapColorSource == 0)
        {
            return;
        }

        _referenceCount--;
        if (_referenceCount != 0)
        {
            return;
        }

        if (_registry is not null && _owner is not null)
        {
            _registry.Unregister(_bitmapColorSource, _owner);
        }

        _realizationOwner.Dispose();
        _release(_bitmapColorSource);
        _bitmapColorSource = 0;
    }
}

internal sealed class Direct3D9BitmapColorSourceRealizer
{
    private readonly Direct3D9RetrieveBitmapCache _retrieveBitmapCache;
    private readonly Action<nint> _addRef;
    private readonly Direct3D9ComputeBitmapRealizationParameters _computeRealizationParameters;
    private readonly Direct3D9GetBitmapColorSource? _getBitmapColorSource;
    private readonly Direct3D9SetBitmapAndContext? _setBitmapAndContext;
    private readonly Direct3D9CreateBitmapPipelineColorSource? _createPipelineColorSource;
    private readonly Direct3D9ChooseAndCreateBitmapPipelineColorSource? _chooseAndCreatePipelineColorSource;
    private readonly Action<nint> _release;

    internal Direct3D9BitmapColorSourceRealizer(
        Direct3D9RetrieveBitmapCache retrieveBitmapCache,
        Action<nint> addRef,
        Direct3D9ComputeBitmapRealizationParameters computeRealizationParameters,
        Direct3D9GetBitmapColorSource getBitmapColorSource,
        Direct3D9SetBitmapAndContext setBitmapAndContext,
        Direct3D9CreateBitmapPipelineColorSource createPipelineColorSource,
        Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(retrieveBitmapCache);
        ArgumentNullException.ThrowIfNull(addRef);
        ArgumentNullException.ThrowIfNull(computeRealizationParameters);
        ArgumentNullException.ThrowIfNull(getBitmapColorSource);
        ArgumentNullException.ThrowIfNull(setBitmapAndContext);
        ArgumentNullException.ThrowIfNull(createPipelineColorSource);
        ArgumentNullException.ThrowIfNull(release);

        _retrieveBitmapCache = retrieveBitmapCache;
        _addRef = addRef;
        _computeRealizationParameters = computeRealizationParameters;
        _getBitmapColorSource = getBitmapColorSource;
        _setBitmapAndContext = setBitmapAndContext;
        _createPipelineColorSource = createPipelineColorSource;
        _release = release;
    }

    internal Direct3D9BitmapColorSourceRealizer(
        Direct3D9RetrieveBitmapCache retrieveBitmapCache,
        Action<nint> addRef,
        Direct3D9ComputeBitmapRealizationParameters computeRealizationParameters,
        Direct3D9ChooseAndCreateBitmapPipelineColorSource chooseAndCreatePipelineColorSource,
        Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(retrieveBitmapCache);
        ArgumentNullException.ThrowIfNull(addRef);
        ArgumentNullException.ThrowIfNull(computeRealizationParameters);
        ArgumentNullException.ThrowIfNull(chooseAndCreatePipelineColorSource);
        ArgumentNullException.ThrowIfNull(release);

        _retrieveBitmapCache = retrieveBitmapCache;
        _addRef = addRef;
        _computeRealizationParameters = computeRealizationParameters;
        _chooseAndCreatePipelineColorSource = chooseAndCreatePipelineColorSource;
        _release = release;
    }

    internal Direct3D9BitmapColorSourceRealizer(
        Direct3D9RetrieveBitmapCache retrieveBitmapCache,
        Action<nint> addRef,
        Direct3D9ComputeBitmapRealizationParameters computeRealizationParameters,
        Direct3D9BitmapPipelineColorSourceFactory pipelineColorSourceFactory,
        Action<nint> release)
        : this(
            retrieveBitmapCache,
            addRef,
            computeRealizationParameters,
            CreatePipelineColorSourceDelegate(pipelineColorSourceFactory),
            release)
    {
    }

    internal int Derive(
        nint bitmapSource,
        nint bitmap,
        nint bitmapCache,
        Direct3D9BitmapRealizationContext realizationContext,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        out Direct3D9BitmapPipelineColorSource? texturedColorSource)
    {
        texturedColorSource = null;
        if (bitmapSource == 0)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        nint bitmapColorSource = 0;
        nint reusableRealizationSource = 0;

        try
        {
            if (bitmapCache != 0)
            {
                _addRef(bitmapCache);
            }
            else
            {
                _retrieveBitmapCache(bitmapSource, out bitmap, out bitmapCache);
            }

            int result = _computeRealizationParameters(
                bitmapSource,
                realizationContext,
                contextParameters,
                out nint realizationParameters);
            if (result < 0)
            {
                return result;
            }

            if (_chooseAndCreatePipelineColorSource is not null)
            {
                result = _chooseAndCreatePipelineColorSource(
                    bitmapSource,
                    bitmap,
                    bitmapCache,
                    realizationParameters,
                    realizationContext.AlternateCache,
                    contextParameters,
                    out texturedColorSource);
                if (result < 0)
                {
                    texturedColorSource?.Dispose();
                    texturedColorSource = null;
                    return result;
                }
                if (texturedColorSource is null)
                {
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return Direct3D9Factory.SuccessHResult;
            }

            result = _getBitmapColorSource!(
                bitmapSource,
                bitmap,
                bitmapCache,
                realizationParameters,
                realizationContext.AlternateCache,
                contextParameters,
                out bitmapColorSource,
                out reusableRealizationSource);
            if (result < 0)
            {
                return result;
            }

            result = _setBitmapAndContext!(
                bitmapColorSource,
                bitmapSource,
                realizationContext.RealizationBounds,
                realizationContext.BitmapToXSpaceTransform,
                realizationParameters,
                reusableRealizationSource);
            if (result < 0)
            {
                return result;
            }

            result = _createPipelineColorSource!(bitmapColorSource, out texturedColorSource);
            if (result < 0)
            {
                texturedColorSource?.Dispose();
                texturedColorSource = null;
                return result;
            }
            if (texturedColorSource is null)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            bitmapColorSource = 0;
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            ReleaseIfNotNull(bitmapCache);
            ReleaseIfNotNull(reusableRealizationSource);
            ReleaseIfNotNull(bitmapColorSource);
        }
    }

    private static Direct3D9ChooseAndCreateBitmapPipelineColorSource CreatePipelineColorSourceDelegate(
        Direct3D9BitmapPipelineColorSourceFactory pipelineColorSourceFactory)
    {
        ArgumentNullException.ThrowIfNull(pipelineColorSourceFactory);
        return pipelineColorSourceFactory.ChooseAndCreate;
    }

    private void ReleaseIfNotNull(nint resource)
    {
        if (resource != 0)
        {
            _release(resource);
        }
    }
}

internal delegate int Direct3D9GetBitmapCache(nint bitmap, out nint bitmapCache);

internal delegate void Direct3D9TryBitmapColorSourceReuse(
    nint bitmapCache,
    Direct3D9BitmapColorSourceContextParameters contextParameters,
    out nint bitmapColorSource,
    out nint reusableColorSources);

internal delegate int Direct3D9DeriveBitmapColorSourceFromBitmap(
    nint bitmapSource,
    nint bitmap,
    nint bitmapCache,
    Direct3D9BitmapColorSourceContextParameters contextParameters,
    out Direct3D9BitmapPipelineColorSource? texturedColorSource);

internal readonly record struct Direct3D9BitmapSourceClipContext(
    bool HasSourceClip,
    bool IsIn3D,
    nint WorldSpaceSourceClip);

internal sealed class Direct3D9BitmapColorSourceDeriver
{
    private readonly Direct3D9RetrieveBitmapCache _retrieveBitmapCache;
    private readonly Func<nint, bool> _isDeviceBitmap;
    private readonly Direct3D9GetBitmapCache _getBitmapCache;
    private readonly Action<nint, nint> _tryCreateDependentDeviceColorSource;
    private readonly Direct3D9TryBitmapColorSourceReuse _tryReuse;
    private readonly Direct3D9DeriveBitmapColorSourceFromBitmap _deriveFromBitmap;
    private readonly Func<nint, Direct3D9BitmapPipelineColorSource> _createReusedPipelineColorSource;
    private readonly Action<nint, MilBitmapInterpolationMode> _setFilterMode;
    private readonly Action<nint, nint> _setReusableColorSources;
    private readonly Func<nint, int> _calculateTextureTransform;
    private readonly Func<bool> _isRealizationContainedBySourceClip;
    private readonly Func<nint, nint, int> _setMaskClipWorldSpace;
    private readonly Action<nint> _release;

    internal Direct3D9BitmapColorSourceDeriver(
        Direct3D9RetrieveBitmapCache retrieveBitmapCache,
        Func<nint, bool> isDeviceBitmap,
        Direct3D9GetBitmapCache getBitmapCache,
        Action<nint, nint> tryCreateDependentDeviceColorSource,
        Direct3D9TryBitmapColorSourceReuse tryReuse,
        Direct3D9DeriveBitmapColorSourceFromBitmap deriveFromBitmap,
        Func<nint, Direct3D9BitmapPipelineColorSource> createReusedPipelineColorSource,
        Action<nint, MilBitmapInterpolationMode> setFilterMode,
        Action<nint, nint> setReusableColorSources,
        Func<nint, int> calculateTextureTransform,
        Func<bool> isRealizationContainedBySourceClip,
        Func<nint, nint, int> setMaskClipWorldSpace,
        Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(retrieveBitmapCache);
        ArgumentNullException.ThrowIfNull(isDeviceBitmap);
        ArgumentNullException.ThrowIfNull(getBitmapCache);
        ArgumentNullException.ThrowIfNull(tryCreateDependentDeviceColorSource);
        ArgumentNullException.ThrowIfNull(tryReuse);
        ArgumentNullException.ThrowIfNull(deriveFromBitmap);
        ArgumentNullException.ThrowIfNull(createReusedPipelineColorSource);
        ArgumentNullException.ThrowIfNull(setFilterMode);
        ArgumentNullException.ThrowIfNull(setReusableColorSources);
        ArgumentNullException.ThrowIfNull(calculateTextureTransform);
        ArgumentNullException.ThrowIfNull(isRealizationContainedBySourceClip);
        ArgumentNullException.ThrowIfNull(setMaskClipWorldSpace);
        ArgumentNullException.ThrowIfNull(release);

        _retrieveBitmapCache = retrieveBitmapCache;
        _isDeviceBitmap = isDeviceBitmap;
        _getBitmapCache = getBitmapCache;
        _tryCreateDependentDeviceColorSource = tryCreateDependentDeviceColorSource;
        _tryReuse = tryReuse;
        _deriveFromBitmap = deriveFromBitmap;
        _createReusedPipelineColorSource = createReusedPipelineColorSource;
        _setFilterMode = setFilterMode;
        _setReusableColorSources = setReusableColorSources;
        _calculateTextureTransform = calculateTextureTransform;
        _isRealizationContainedBySourceClip = isRealizationContainedBySourceClip;
        _setMaskClipWorldSpace = setMaskClipWorldSpace;
        _release = release;
    }

    internal int Derive(
        nint bitmapSource,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        out Direct3D9BitmapPipelineColorSource? texturedColorSource) =>
        Derive(bitmapSource, contextParameters, default, out texturedColorSource);

    internal int Derive(
        nint bitmapSource,
        Direct3D9BitmapColorSourceContextParameters contextParameters,
        Direct3D9BitmapSourceClipContext sourceClipContext,
        out Direct3D9BitmapPipelineColorSource? texturedColorSource)
    {
        if (bitmapSource == 0)
        {
            texturedColorSource = null;
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        nint bitmap = 0;
        nint bitmapCache = 0;
        nint bitmapColorSource = 0;
        nint reusableColorSources = 0;
        Direct3D9BitmapPipelineColorSource? pipelineColorSource = null;
        texturedColorSource = null;

        try
        {
            int cacheResult = _retrieveBitmapCache(bitmapSource, out bitmap, out bitmapCache);
            if (cacheResult >= 0)
            {
                if (bitmap != 0 && _isDeviceBitmap(bitmap))
                {
                    contextParameters = contextParameters with
                    {
                        PrefilterEnabled = false,
                        InterpolationMode = Direct3D9BitmapColorSourceContextParameters.UsesMipMapping(
                            contextParameters.InterpolationMode)
                                ? MilBitmapInterpolationMode.Linear
                                : contextParameters.InterpolationMode
                    };

                    if (bitmapCache == 0)
                    {
                        int getCacheResult = _getBitmapCache(bitmap, out bitmapCache);
                        if (getCacheResult >= 0)
                        {
                            _tryCreateDependentDeviceColorSource(bitmap, bitmapCache);
                        }
                    }
                }

                if (bitmapCache != 0)
                {
                    _tryReuse(
                        bitmapCache,
                        contextParameters,
                        out bitmapColorSource,
                        out reusableColorSources);
                }
            }

            if (bitmapColorSource == 0)
            {
                int deriveResult = _deriveFromBitmap(
                    bitmapSource,
                    bitmap,
                    bitmapCache,
                    contextParameters,
                    out pipelineColorSource);
                if (deriveResult < 0)
                {
                    pipelineColorSource?.Dispose();
                    pipelineColorSource = null;
                    return deriveResult;
                }
                if (pipelineColorSource is null)
                {
                    return Direct3D9Factory.GenericFailureHResult;
                }

            }
            else
            {
                _setFilterMode(bitmapColorSource, contextParameters.InterpolationMode);
                _setReusableColorSources(bitmapColorSource, reusableColorSources);

                int transformResult = _calculateTextureTransform(bitmapColorSource);
                if (transformResult < 0)
                {
                    return transformResult;
                }

                pipelineColorSource = _createReusedPipelineColorSource(bitmapColorSource);
                bitmapColorSource = 0;
            }

            nint worldSpaceMask = 0;
            if (sourceClipContext.HasSourceClip
                && sourceClipContext.IsIn3D
                && !_isRealizationContainedBySourceClip())
            {
                worldSpaceMask = sourceClipContext.WorldSpaceSourceClip;
            }

            int maskResult = _setMaskClipWorldSpace(pipelineColorSource.BitmapColorSource, worldSpaceMask);
            if (maskResult < 0)
            {
                return maskResult;
            }

            texturedColorSource = pipelineColorSource;
            pipelineColorSource = null;
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            ReleaseIfNotNull(bitmapCache);
            ReleaseIfNotNull(reusableColorSources);
            pipelineColorSource?.Dispose();
            ReleaseIfNotNull(bitmapColorSource);
        }
    }

    private void ReleaseIfNotNull(nint resource)
    {
        if (resource != 0)
        {
            _release(resource);
        }
    }
}

internal delegate int Direct3D9PrepareBitmapTexturePopulation(
    nint bitmapSource,
    uint dirtyRectangleCount,
    nint dirtyRectangles,
    out nint bitmapLock,
    out bool copySourceToSystemMemorySurface,
    out nint systemMemorySurface);

internal delegate int Direct3D9PushBitmapTextureBits(
    nint bitmapSource,
    uint dirtyRectangleCount,
    nint dirtyRectangles,
    nint systemMemorySurface,
    bool copySourceToSystemMemorySurface);

internal delegate int Direct3D9CreateBitmapTextureStagingSurface(
    out Direct3D9SystemMemoryUpdateSurface? surface);

internal delegate int Direct3D9PushBitmapTextureLevelZero(
    nint bitmapSource,
    ReadOnlySpan<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
    Direct3D9SystemMemoryUpdateSurface systemMemorySurface,
    bool copySourceToSystemMemorySurface);

internal readonly record struct Direct3D9BitmapSourceRectangle(
    int X,
    int Y,
    int Width,
    int Height);

internal static unsafe class Direct3D9BitmapSource
{
    internal static int CopyPixels(
        nint bitmapSource,
        Direct3D9BitmapSourceRectangle sourceRectangle,
        uint destinationPitch,
        uint destinationBufferSize,
        nint destinationPixels)
    {
        if (bitmapSource == 0)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        void*** source = (void***) bitmapSource;
        void** vtable = *source;
        delegate* unmanaged[Stdcall]<void***, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int> copyPixels =
            (delegate* unmanaged[Stdcall]<void***, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int>) vtable[7];
        return copyPixels(
            source,
            &sourceRectangle,
            destinationPitch,
            destinationBufferSize,
            (byte*) destinationPixels);
    }
}

[Flags]
internal enum MilBitmapLockFlags : uint
{
    Read = 1,
    Write = 2
}

internal static unsafe class Direct3D9Bitmap
{
    private const uint MaximumDirtyRectangleCount = 5;

    internal static uint GetUniquenessToken(nint bitmap)
    {
        if (bitmap == 0)
        {
            return 0;
        }

        void*** instance = (void***) bitmap;
        void** vtable = *instance;
        uint uniquenessToken = 0;
        delegate* unmanaged[Stdcall]<void***, uint*, void> getUniquenessToken =
            (delegate* unmanaged[Stdcall]<void***, uint*, void>) vtable[14];
        getUniquenessToken(instance, &uniquenessToken);
        return uniquenessToken;
    }

    internal static bool GetDirtyRectangles(
        nint bitmap,
        uint cachedUniquenessToken,
        out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
        out uint newestUniquenessToken)
    {
        dirtyRectangles = [];
        newestUniquenessToken = cachedUniquenessToken;
        if (bitmap == 0)
        {
            return false;
        }

        void*** instance = (void***) bitmap;
        void** vtable = *instance;
        Direct3D9BitmapRealizationRectangle* nativeDirtyRectangles = null;
        uint dirtyRectangleCount = 0;
        uint nativeNewestUniquenessToken = cachedUniquenessToken;
        delegate* unmanaged[Stdcall]<void***, Direct3D9BitmapRealizationRectangle**, uint*, uint*, byte> getDirtyRectangles =
            (delegate* unmanaged[Stdcall]<void***, Direct3D9BitmapRealizationRectangle**, uint*, uint*, byte>) vtable[12];
        bool dirtyRectanglesAreValid = getDirtyRectangles(
            instance,
            &nativeDirtyRectangles,
            &dirtyRectangleCount,
            &nativeNewestUniquenessToken) != 0;
        newestUniquenessToken = nativeNewestUniquenessToken;
        if (!dirtyRectanglesAreValid)
        {
            return false;
        }

        if (dirtyRectangleCount > MaximumDirtyRectangleCount
            || (dirtyRectangleCount > 0 && nativeDirtyRectangles is null))
        {
            return false;
        }

        Direct3D9BitmapRealizationRectangle[] copiedDirtyRectangles =
            new Direct3D9BitmapRealizationRectangle[dirtyRectangleCount];
        for (int i = 0; i < copiedDirtyRectangles.Length; i++)
        {
            copiedDirtyRectangles[i] = nativeDirtyRectangles[i];
        }

        dirtyRectangles = copiedDirtyRectangles;
        return true;
    }

    internal static int Lock(
        nint bitmap,
        Direct3D9BitmapSourceRectangle lockRectangle,
        MilBitmapLockFlags flags,
        out nint bitmapLock)
    {
        return LockEntireBitmap(bitmap, &lockRectangle, flags, out bitmapLock);
    }

    internal static int LockEntireBitmap(
        nint bitmap,
        Direct3D9BitmapSourceRectangle* lockRectangle,
        MilBitmapLockFlags flags,
        out nint bitmapLock)
    {
        bitmapLock = 0;
        if (bitmap == 0)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        void*** instance = (void***) bitmap;
        void** vtable = *instance;
        void*** nativeBitmapLock = null;
        delegate* unmanaged[Stdcall]<void***, Direct3D9BitmapSourceRectangle*, uint, void****, int> lockBitmap =
            (delegate* unmanaged[Stdcall]<void***, Direct3D9BitmapSourceRectangle*, uint, void****, int>) vtable[8];
        int result = lockBitmap(instance, lockRectangle, (uint) flags, &nativeBitmapLock);
        if (result >= 0)
        {
            bitmapLock = (nint) nativeBitmapLock;
        }
        else if (nativeBitmapLock is not null)
        {
            Direct3D9Factory.Release((nint) nativeBitmapLock);
        }

        return result;
    }
}

internal static unsafe class Direct3D9BitmapLock
{
    internal static int GetSize(nint bitmapLock, out uint width, out uint height)
    {
        width = 0;
        height = 0;
        if (bitmapLock == 0)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        void*** instance = (void***) bitmapLock;
        void** vtable = *instance;
        delegate* unmanaged[Stdcall]<void***, uint*, uint*, int> getSize =
            (delegate* unmanaged[Stdcall]<void***, uint*, uint*, int>) vtable[3];
        uint nativeWidth = 0;
        uint nativeHeight = 0;
        int result = getSize(instance, &nativeWidth, &nativeHeight);
        if (result >= 0)
        {
            width = nativeWidth;
            height = nativeHeight;
        }

        return result;
    }

    internal static int GetStride(nint bitmapLock, out uint stride)
    {
        stride = 0;
        if (bitmapLock == 0)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        void*** instance = (void***) bitmapLock;
        void** vtable = *instance;
        delegate* unmanaged[Stdcall]<void***, uint*, int> getStride =
            (delegate* unmanaged[Stdcall]<void***, uint*, int>) vtable[4];
        uint nativeStride = 0;
        int result = getStride(instance, &nativeStride);
        if (result >= 0)
        {
            stride = nativeStride;
        }

        return result;
    }

    internal static int GetDataPointer(nint bitmapLock, out uint bufferSize, out nint data)
    {
        bufferSize = 0;
        data = 0;
        if (bitmapLock == 0)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        void*** instance = (void***) bitmapLock;
        void** vtable = *instance;
        uint nativeBufferSize = 0;
        byte* nativeData = null;
        delegate* unmanaged[Stdcall]<void***, uint*, byte**, int> getDataPointer =
            (delegate* unmanaged[Stdcall]<void***, uint*, byte**, int>) vtable[5];
        int result = getDataPointer(instance, &nativeBufferSize, &nativeData);
        if (result >= 0)
        {
            bufferSize = nativeBufferSize;
            data = (nint) nativeData;
        }

        return result;
    }

    internal static int GetPixelFormat(nint bitmapLock, out MilPixelFormat pixelFormat)
    {
        pixelFormat = MilPixelFormat.Undefined;
        if (bitmapLock == 0)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        void*** instance = (void***) bitmapLock;
        void** vtable = *instance;
        delegate* unmanaged[Stdcall]<void***, MilPixelFormat*, int> getPixelFormat =
            (delegate* unmanaged[Stdcall]<void***, MilPixelFormat*, int>) vtable[6];
        MilPixelFormat nativePixelFormat = MilPixelFormat.Undefined;
        int result = getPixelFormat(instance, &nativePixelFormat);
        if (result >= 0)
        {
            pixelFormat = nativePixelFormat;
        }

        return result;
    }
}

internal sealed unsafe class Direct3D9BitmapTextureLevelZeroPusher
{ 
    private readonly Direct3D9Texture _texture;
    private readonly Direct3D9Device _device;
    private readonly Direct3D9BitmapTextureRequirements _requirements;
    private readonly Direct3D9TexelLayout _uLayout;
    private readonly Direct3D9TexelLayout _vLayout;
    private readonly Direct3D9BitmapRealizationRectangle _prefilteredBitmapBounds;

    internal Direct3D9BitmapTextureLevelZeroPusher(
        Direct3D9Texture texture,
        Direct3D9Device device,
        Direct3D9BitmapTextureRequirements requirements,
        Direct3D9TexelLayout uLayout,
        Direct3D9TexelLayout vLayout,
        Direct3D9BitmapRealizationRectangle prefilteredBitmapBounds)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(device);

        _texture = texture;
        _device = device;
        _requirements = requirements;
        _uLayout = uLayout;
        _vLayout = vLayout;
        _prefilteredBitmapBounds = prefilteredBitmapBounds;
    }

    internal int Push(
        nint bitmapSource,
        ReadOnlySpan<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
        Direct3D9SystemMemoryUpdateSurface systemMemorySurface,
        bool copySourceToSystemMemorySurface)
    {
        ArgumentNullException.ThrowIfNull(systemMemorySurface);
        bool hasBorder = IsEdgeLayout(_uLayout) && IsEdgeLayout(_vLayout);
        if (!hasBorder
            && (_uLayout is not (Direct3D9TexelLayout.Natural or Direct3D9TexelLayout.FirstOnly)
                || _vLayout is not (Direct3D9TexelLayout.Natural or Direct3D9TexelLayout.FirstOnly)))
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        if (dirtyRectangles.IsEmpty)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        if (!copySourceToSystemMemorySurface)
        {
            if (hasBorder)
            {
                return Direct3D9Factory.UnsupportedOperationHResult;
            }

            int getSurfaceResult = _texture.TryGetSurfaceLevel(0, out Direct3D9Surface? destinationSurface);
            if (getSurfaceResult < 0)
            {
                return getSurfaceResult;
            }

            using (destinationSurface)
            foreach (Direct3D9BitmapRealizationRectangle dirtyRectangle in dirtyRectangles)
            {
                int updateResult = _device.UpdateSurface(
                    systemMemorySurface.Surface,
                    ToSurfaceRect(dirtyRectangle),
                    destinationSurface!.SurfaceForDeviceCall,
                    new Direct3D9Point(
                        checked((int) ((long) dirtyRectangle.Left - _prefilteredBitmapBounds.Left)),
                        checked((int) ((long) dirtyRectangle.Top - _prefilteredBitmapBounds.Top))));
                if (updateResult < 0)
                {
                    return updateResult;
                }
            }

            return Direct3D9Factory.SuccessHResult;
        }

        bool isLocked = false;
        int result;
        try
        {
            Direct3D9SurfaceRect textureBounds = new(
                    0,
                    0,
                    checked((int) _requirements.Description.Width),
                    checked((int) _requirements.Description.Height));
                result = systemMemorySurface.LockRect(
                    out LockedRect lockedRect,
                    textureBounds,
                    (uint) D3D9.LockNoDirtyUpdate);
                if (result < 0)
                {
                    return result;
                }

                isLocked = true;
                if (lockedRect.Pitch <= 0 || lockedRect.PBits is null)
                {
                    return Direct3D9Factory.InternalErrorHResult;
                }

                uint bufferSize = checked((uint) lockedRect.Pitch * _requirements.Description.Height);
                int pixelSize = GetPixelSize(_requirements.Description.Format);
                Direct3D9BitmapRealizationRectangle[] updateRectangles = dirtyRectangles.ToArray();
                bool updateBorder = hasBorder && TouchesBitmapEdge(updateRectangles);
                if (updateBorder)
                {
                    updateRectangles = [_prefilteredBitmapBounds];
                }

                foreach (Direct3D9BitmapRealizationRectangle dirtyRectangle in updateRectangles)
                {
                    int destinationX = checked((int) ((long) dirtyRectangle.Left - _prefilteredBitmapBounds.Left))
                        + (hasBorder ? 1 : 0);
                    int destinationY = checked((int) ((long) dirtyRectangle.Top - _prefilteredBitmapBounds.Top))
                        + (hasBorder ? 1 : 0);
                    nint destinationPixels = (nint) ((byte*) lockedRect.PBits
                        + checked(destinationY * lockedRect.Pitch)
                        + checked(destinationX * pixelSize));
                    Direct3D9BitmapSourceRectangle sourceRectangle = new(
                        checked((int) dirtyRectangle.Left),
                        checked((int) dirtyRectangle.Top),
                        checked((int) dirtyRectangle.Width),
                        checked((int) dirtyRectangle.Height));
                    result = Direct3D9BitmapSource.CopyPixels(
                        bitmapSource,
                        sourceRectangle,
                        checked((uint) lockedRect.Pitch),
                        bufferSize,
                        destinationPixels);
                    if (result < 0)
                    {
                        return result;
                    }
                }

                if (updateBorder)
                {
                    UpdateBorders((byte*) lockedRect.PBits, lockedRect.Pitch, pixelSize, bufferSize);
                }

            result = systemMemorySurface.UnlockRect();
            isLocked = false;
            if (result < 0)
            {
                return result;
            }

            result = _texture.TryGetSurfaceLevel(0, out Direct3D9Surface? destinationSurface);
            if (result < 0)
            {
                return result;
            }

            using Direct3D9Surface ownedDestinationSurface = destinationSurface!;
            Direct3D9BitmapRealizationRectangle[] videoMemoryRectangles = dirtyRectangles.ToArray();
            if (hasBorder && TouchesBitmapEdge(videoMemoryRectangles))
            {
                result = _device.UpdateSurface(
                    systemMemorySurface.Surface,
                    new Direct3D9SurfaceRect(
                        0,
                        0,
                        checked((int) _requirements.Description.Width),
                        checked((int) _requirements.Description.Height)),
                    destinationSurface!.SurfaceForDeviceCall,
                    new Direct3D9Point(0, 0));
                if (result < 0)
                {
                    return result;
                }
            }
            else
            {
                foreach (Direct3D9BitmapRealizationRectangle dirtyRectangle in videoMemoryRectangles)
                {
                    Direct3D9BitmapRealizationRectangle source = OffsetToTexture(dirtyRectangle);
                    if (hasBorder)
                    {
                        source = Offset(source, 1, 1);
                    }

                    Direct3D9SurfaceRect sourceRectangle = ToSurfaceRect(source);
                    Direct3D9Point destinationPoint = new(
                        checked((int) ((long) dirtyRectangle.Left - _prefilteredBitmapBounds.Left)) + (hasBorder ? 1 : 0),
                        checked((int) ((long) dirtyRectangle.Top - _prefilteredBitmapBounds.Top)) + (hasBorder ? 1 : 0));
                    result = _device.UpdateSurface(
                        systemMemorySurface.Surface,
                        sourceRectangle,
                        destinationSurface!.SurfaceForDeviceCall,
                        destinationPoint);
                    if (result < 0)
                    {
                        return result;
                    }
                }
            }

            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            if (isLocked)
            {
                _ = systemMemorySurface.UnlockRect();
            }
        }
    }

    private Direct3D9BitmapRealizationRectangle OffsetToTexture(
        Direct3D9BitmapRealizationRectangle rectangle) =>
        new(
            checked((uint) ((long) rectangle.Left - _prefilteredBitmapBounds.Left)),
            checked((uint) ((long) rectangle.Top - _prefilteredBitmapBounds.Top)),
            checked((uint) ((long) rectangle.Right - _prefilteredBitmapBounds.Left)),
            checked((uint) ((long) rectangle.Bottom - _prefilteredBitmapBounds.Top)));

    private bool TouchesBitmapEdge(ReadOnlySpan<Direct3D9BitmapRealizationRectangle> rectangles)
    {
        foreach (Direct3D9BitmapRealizationRectangle rectangle in rectangles)
        {
            if (rectangle.Left == _prefilteredBitmapBounds.Left
                || rectangle.Top == _prefilteredBitmapBounds.Top
                || rectangle.Right == _prefilteredBitmapBounds.Right
                || rectangle.Bottom == _prefilteredBitmapBounds.Bottom)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateBorders(byte* pixels, int pitch, int pixelSize, uint bufferSize)
    {
        int width = checked((int) _requirements.Description.Width);
        int height = checked((int) _requirements.Description.Height);
        int leftDestination = _uLayout == Direct3D9TexelLayout.EdgeMirrored ? 0 : width - 1;
        int rightDestination = _uLayout == Direct3D9TexelLayout.EdgeMirrored ? width - 1 : 0;
        int topDestination = _vLayout == Direct3D9TexelLayout.EdgeMirrored ? 0 : height - 1;
        int bottomDestination = _vLayout == Direct3D9TexelLayout.EdgeMirrored ? height - 1 : 0;

        CopyPixels(pixels, pitch, pixelSize, bufferSize, 1, 1, 1, height - 2, leftDestination, 1);
        CopyPixels(pixels, pitch, pixelSize, bufferSize, width - 2, 1, 1, height - 2, rightDestination, 1);
        CopyPixels(pixels, pitch, pixelSize, bufferSize, 0, 1, width, 1, 0, topDestination);
        CopyPixels(pixels, pitch, pixelSize, bufferSize, 0, height - 2, width, 1, 0, bottomDestination);
    }

    private static void CopyPixels(
        byte* pixels,
        int pitch,
        int pixelSize,
        uint bufferSize,
        int sourceX,
        int sourceY,
        int width,
        int height,
        int destinationX,
        int destinationY)
    {
        nuint rowSize = checked((nuint) (width * pixelSize));
        for (int row = 0; row < height; row++)
        {
            int sourceOffset = checked((sourceY + row) * pitch + sourceX * pixelSize);
            int destinationOffset = checked((destinationY + row) * pitch + destinationX * pixelSize);
            if (sourceOffset < 0
                || destinationOffset < 0
                || checked((uint) sourceOffset + (uint) rowSize) > bufferSize
                || checked((uint) destinationOffset + (uint) rowSize) > bufferSize)
            {
                throw new InvalidOperationException("Direct3D bitmap border update exceeds the locked surface.");
            }

            Buffer.MemoryCopy(pixels + sourceOffset, pixels + destinationOffset, rowSize, rowSize);
        }
    }

    private static bool IsEdgeLayout(Direct3D9TexelLayout layout) =>
        layout is Direct3D9TexelLayout.EdgeWrapped or Direct3D9TexelLayout.EdgeMirrored;

    private static Direct3D9BitmapRealizationRectangle Offset(
        Direct3D9BitmapRealizationRectangle rectangle,
        uint horizontal,
        uint vertical) =>
        new(
            checked(rectangle.Left + horizontal),
            checked(rectangle.Top + vertical),
            checked(rectangle.Right + horizontal),
            checked(rectangle.Bottom + vertical));

    private static Direct3D9SurfaceRect ToSurfaceRect(Direct3D9BitmapRealizationRectangle rectangle) =>
        new(
            checked((int) rectangle.Left),
            checked((int) rectangle.Top),
            checked((int) rectangle.Right),
            checked((int) rectangle.Bottom));

    private static int GetPixelSize(Format format) => format switch
    {
        Format.A8 => 1,
        Format.A8L8 => 2,
        Format.R5G6B5 => 2,
        Format.X1R5G5B5 => 2,
        Format.A1R5G5B5 => 2,
        Format.A4R4G4B4 => 2,
        Format.R8G8B8 => 3,
        Format.A8R8G8B8 => 4,
        Format.X8R8G8B8 => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}

internal sealed unsafe class Direct3D9BitmapTexturePopulator
{
    private readonly Direct3D9PrepareBitmapTexturePopulation? _prepare;
    private readonly Direct3D9PushBitmapTextureBits? _push;
    private readonly Direct3D9CreateBitmapTextureStagingSurface? _createStagingSurface;
    private readonly Direct3D9BitmapTexturePopulationPreparer? _populationPreparer;
    private readonly Direct3D9PushBitmapTextureLevelZero? _pushLevelZero;
    private readonly Direct3D9BitmapReusableRealizationSources? _reusableSources;
    private readonly Direct3D9BitmapReusableRealizationPopulation? _reusablePopulation;
    private readonly Func<int> _updateMipmapLevels;
    private readonly Action<uint, Direct3D9BitmapRealizationRectangle> _commit;
    private readonly Action<nint>? _release;

    internal Direct3D9BitmapTexturePopulator(
        Direct3D9Texture texture,
        Direct3D9Device device,
        Direct3D9PrepareBitmapTexturePopulation prepare,
        Direct3D9PushBitmapTextureBits push,
        Action<uint, Direct3D9BitmapRealizationRectangle> commit,
        Action<nint> release)
        : this(prepare, push, () => texture.UpdateMipmapLevels(device), commit, release)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(device);
    }

    internal Direct3D9BitmapTexturePopulator(
        Direct3D9Texture texture,
        Direct3D9Device device,
        Direct3D9PrepareBitmapTexturePopulation prepare,
        Direct3D9PushBitmapTextureBits push,
        Direct3D9BitmapColorSourceRealizationState realizationState,
        Action<nint> release)
        : this(texture, device, prepare, push, GetCommit(realizationState), release)
    {
    }

    internal Direct3D9BitmapTexturePopulator(
        Direct3D9Texture texture,
        Direct3D9Device device,
        Direct3D9BitmapTextureRequirements requirements,
        Direct3D9BitmapTextureLevelZeroPusher levelZeroPusher,
        Action<uint, Direct3D9BitmapRealizationRectangle> commit)
        : this(
            (out Direct3D9SystemMemoryUpdateSurface? surface) => device.TryCreateSystemMemoryUpdateSurface(
                requirements.Description.Width,
                requirements.Description.Height,
                requirements.Description.Format,
                null,
                out surface),
            levelZeroPusher.Push,
            () => texture.UpdateMipmapLevels(device),
            commit)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(levelZeroPusher);
    }

    internal Direct3D9BitmapTexturePopulator(
        Direct3D9CreateBitmapTextureStagingSurface createStagingSurface,
        Direct3D9PushBitmapTextureLevelZero pushLevelZero,
        Func<int> updateMipmapLevels,
        Action<uint, Direct3D9BitmapRealizationRectangle> commit)
        : this(createStagingSurface, pushLevelZero, null, null, updateMipmapLevels, commit)
    {
    }

    internal Direct3D9BitmapTexturePopulator(
        Direct3D9CreateBitmapTextureStagingSurface createStagingSurface,
        Direct3D9PushBitmapTextureLevelZero pushLevelZero,
        Direct3D9BitmapReusableRealizationSources? reusableSources,
        Direct3D9BitmapReusableRealizationPopulation? reusablePopulation,
        Func<int> updateMipmapLevels,
        Action<uint, Direct3D9BitmapRealizationRectangle> commit)
    {
        ArgumentNullException.ThrowIfNull(createStagingSurface);
        ArgumentNullException.ThrowIfNull(pushLevelZero);
        ArgumentNullException.ThrowIfNull(updateMipmapLevels);
        ArgumentNullException.ThrowIfNull(commit);
        if ((reusableSources is null) != (reusablePopulation is null))
        {
            throw new ArgumentException("Reusable realization sources and population must be provided together.");
        }

        _createStagingSurface = createStagingSurface;
        _pushLevelZero = pushLevelZero;
        _reusableSources = reusableSources;
        _reusablePopulation = reusablePopulation;
        _updateMipmapLevels = updateMipmapLevels;
        _commit = commit;
    }

    internal Direct3D9BitmapTexturePopulator(
        Direct3D9BitmapTexturePopulationPreparer populationPreparer,
        Direct3D9PushBitmapTextureLevelZero pushLevelZero,
        Func<int> updateMipmapLevels,
        Action<uint, Direct3D9BitmapRealizationRectangle> commit,
        Action<nint> release)
        : this(populationPreparer, pushLevelZero, null, null, updateMipmapLevels, commit, release)
    {
    }

    internal Direct3D9BitmapTexturePopulator(
        Direct3D9BitmapTexturePopulationPreparer populationPreparer,
        Direct3D9PushBitmapTextureLevelZero pushLevelZero,
        Direct3D9BitmapReusableRealizationSources? reusableSources,
        Direct3D9BitmapReusableRealizationPopulation? reusablePopulation,
        Func<int> updateMipmapLevels,
        Action<uint, Direct3D9BitmapRealizationRectangle> commit,
        Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(populationPreparer);
        ArgumentNullException.ThrowIfNull(pushLevelZero);
        ArgumentNullException.ThrowIfNull(updateMipmapLevels);
        ArgumentNullException.ThrowIfNull(commit);
        ArgumentNullException.ThrowIfNull(release);
        if ((reusableSources is null) != (reusablePopulation is null))
        {
            throw new ArgumentException("Reusable realization sources and population must be provided together.");
        }

        _populationPreparer = populationPreparer;
        _pushLevelZero = pushLevelZero;
        _reusableSources = reusableSources;
        _reusablePopulation = reusablePopulation;
        _updateMipmapLevels = updateMipmapLevels;
        _commit = commit;
        _release = release;
    }

    internal Direct3D9BitmapTexturePopulator(
        Direct3D9PrepareBitmapTexturePopulation prepare,
        Direct3D9PushBitmapTextureBits push,
        Func<int> updateMipmapLevels,
        Direct3D9BitmapColorSourceRealizationState realizationState,
        Action<nint> release)
        : this(prepare, push, updateMipmapLevels, GetCommit(realizationState), release)
    {
    }

    internal Direct3D9BitmapTexturePopulator(
        Direct3D9PrepareBitmapTexturePopulation prepare,
        Direct3D9PushBitmapTextureBits push,
        Func<int> updateMipmapLevels,
        Action<uint, Direct3D9BitmapRealizationRectangle> commit,
        Action<nint> release)
    {
        ArgumentNullException.ThrowIfNull(prepare);
        ArgumentNullException.ThrowIfNull(push);
        ArgumentNullException.ThrowIfNull(updateMipmapLevels);
        ArgumentNullException.ThrowIfNull(commit);
        ArgumentNullException.ThrowIfNull(release);

        _prepare = prepare;
        _push = push;
        _updateMipmapLevels = updateMipmapLevels;
        _commit = commit;
        _release = release;
    }

    internal int Populate(
        nint bitmapSource,
        uint dirtyRectangleCount,
        nint dirtyRectangles,
        uint newestUniquenessToken,
        Direct3D9BitmapRealizationRectangle requiredRealizationBounds)
    {
        if (_createStagingSurface is not null)
        {
            return PopulateWithTemporaryStagingSurface(
                bitmapSource,
                dirtyRectangleCount,
                dirtyRectangles,
                newestUniquenessToken,
                requiredRealizationBounds);
        }
        if (_populationPreparer is not null)
        {
            return PopulateWithPreparedSurface(
                bitmapSource,
                dirtyRectangleCount,
                dirtyRectangles,
                newestUniquenessToken,
                requiredRealizationBounds);
        }

        nint bitmapLock = 0;
        nint systemMemorySurface = 0;

        try
        {
            if (dirtyRectangleCount > 0)
            {
                int result = _prepare!(
                    bitmapSource,
                    dirtyRectangleCount,
                    dirtyRectangles,
                    out bitmapLock,
                    out bool copySourceToSystemMemorySurface,
                    out systemMemorySurface);
                if (result < 0)
                {
                    return result;
                }

                result = _push!(
                    bitmapSource,
                    dirtyRectangleCount,
                    dirtyRectangles,
                    systemMemorySurface,
                    copySourceToSystemMemorySurface);
                if (result < 0)
                {
                    return result;
                }

                result = _updateMipmapLevels();
                if (result < 0)
                {
                    return result;
                }
            }

            _commit(newestUniquenessToken, requiredRealizationBounds);
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            ReleaseIfNotNull(systemMemorySurface);
            ReleaseIfNotNull(bitmapLock);
        }
    }

    private int PopulateWithPreparedSurface(
        nint bitmapSource,
        uint dirtyRectangleCount,
        nint dirtyRectangles,
        uint newestUniquenessToken,
        Direct3D9BitmapRealizationRectangle requiredRealizationBounds)
    {
        if (dirtyRectangleCount > int.MaxValue || (dirtyRectangleCount > 0 && dirtyRectangles == 0))
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        int result = GetRemainingBitmapRectangles(
            dirtyRectangleCount,
            dirtyRectangles,
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remainingRectangles);
        if (result < 0)
        {
            return result;
        }

        nint bitmapLock = 0;
        Direct3D9SystemMemoryUpdateSurface? surface = null;
        try
        {
            if (remainingRectangles.Count > 0)
            {
                result = _populationPreparer!.Prepare(
                    out bitmapLock,
                    out bool copySourceToSystemMemorySurface,
                    out surface);
                if (result < 0)
                {
                    return result;
                }
                if (surface is null)
                {
                    throw new InvalidOperationException("Direct3D bitmap population preparation returned a null surface.");
                }

                result = _pushLevelZero!(
                    bitmapSource,
                    CopyRectangles(remainingRectangles),
                    surface,
                    copySourceToSystemMemorySurface);
                if (result < 0)
                {
                    return result;
                }
            }

            if (dirtyRectangleCount > 0)
            {
                result = _updateMipmapLevels();
                if (result < 0)
                {
                    return result;
                }
            }

            _commit(newestUniquenessToken, requiredRealizationBounds);
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            surface?.Dispose();
            if (bitmapLock != 0)
            {
                _release!(bitmapLock);
            }
        }
    }

    private int PopulateWithTemporaryStagingSurface(
        nint bitmapSource,
        uint dirtyRectangleCount,
        nint dirtyRectangles,
        uint newestUniquenessToken,
        Direct3D9BitmapRealizationRectangle requiredRealizationBounds)
    {
        if (dirtyRectangleCount > int.MaxValue || (dirtyRectangleCount > 0 && dirtyRectangles == 0))
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        int result = GetRemainingBitmapRectangles(
            dirtyRectangleCount,
            dirtyRectangles,
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remainingRectangles);
        if (result < 0)
        {
            return result;
        }

        Direct3D9SystemMemoryUpdateSurface? stagingSurface = null;
        try
        {
            if (remainingRectangles.Count > 0)
            {
                result = _createStagingSurface!(out stagingSurface);
                if (result < 0)
                {
                    return result;
                }
                if (stagingSurface is null)
                {
                    throw new InvalidOperationException("Direct3D bitmap staging surface creation returned a null surface.");
                }

                result = _pushLevelZero!(bitmapSource, CopyRectangles(remainingRectangles), stagingSurface, true);
                if (result < 0)
                {
                    return result;
                }
            }

            if (dirtyRectangleCount > 0)
            {
                result = _updateMipmapLevels();
                if (result < 0)
                {
                    return result;
                }
            }

            _commit(newestUniquenessToken, requiredRealizationBounds);
            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            stagingSurface?.Dispose();
        }
    }

    private int GetRemainingBitmapRectangles(
        uint dirtyRectangleCount,
        nint dirtyRectangles,
        out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remainingRectangles)
    {
        if (dirtyRectangleCount == 0)
        {
            remainingRectangles = [];
            return Direct3D9Factory.SuccessHResult;
        }

        Direct3D9BitmapRealizationRectangle[] rectangles = new ReadOnlySpan<Direct3D9BitmapRealizationRectangle>(
            (void*) dirtyRectangles,
            checked((int) dirtyRectangleCount)).ToArray();
        if (_reusablePopulation is null)
        {
            remainingRectangles = rectangles;
            return Direct3D9Factory.SuccessHResult;
        }

        return _reusablePopulation.UpdateFromSources(
            _reusableSources!.Head,
            rectangles,
            out remainingRectangles);
    }

    private static ReadOnlySpan<Direct3D9BitmapRealizationRectangle> CopyRectangles(
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles)
    {
        Direct3D9BitmapRealizationRectangle[] copy = new Direct3D9BitmapRealizationRectangle[rectangles.Count];
        for (int i = 0; i < copy.Length; i++)
        {
            copy[i] = rectangles[i];
        }

        return copy;
    }

    private static Action<uint, Direct3D9BitmapRealizationRectangle> GetCommit(
        Direct3D9BitmapColorSourceRealizationState realizationState)
    {
        ArgumentNullException.ThrowIfNull(realizationState);
        return realizationState.Commit;
    }

    private void ReleaseIfNotNull(nint resource)
    {
        if (resource != 0)
        {
            _release!(resource);
        }
    }
}
