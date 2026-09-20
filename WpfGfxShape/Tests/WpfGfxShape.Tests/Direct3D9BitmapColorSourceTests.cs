using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapColorSourceTests
{
    [TestMethod]
    public void WhenBrushContextParametersAreCreatedThenNativeFieldsAreCaptured()
    {
        Direct3D9BitmapColorSourceContextParameters parameters =
            Direct3D9BitmapColorSourceContextParameters.FromBrushAndContext(
                3,
                MilBitmapInterpolationMode.Linear,
                prefilterEnabled: true,
                MilPixelFormat.Pbgra32Bpp,
                bitmapBrushUniqueness: 7,
                MilBitmapWrapMode.FlipXY,
                isSoftwareDevice: false,
                canAutoGenerateMipmaps: false,
                canGenerateMipmapsWithStretchRect: false,
                isFantScalerDisabled: false);

        Assert.AreEqual(
            new Direct3D9BitmapColorSourceContextParameters(
                3,
                MilBitmapInterpolationMode.Linear,
                true,
                MilPixelFormat.Pbgra32Bpp,
                7,
                MilBitmapWrapMode.FlipXY),
            parameters);
    }

    [TestMethod]
    public void WhenFantScalerIsDisabledThenEnabledPrefilteringIsDisabled()
    {
        Direct3D9BitmapColorSourceContextParameters parameters = CreateParameters(
            MilBitmapInterpolationMode.Linear,
            prefilterEnabled: true,
            isFantScalerDisabled: true);

        Assert.IsFalse(parameters.PrefilterEnabled);
    }

    [TestMethod]
    public void WhenPrefilteringIsAlreadyDisabledThenFantScalerSettingDoesNotChangeIt()
    {
        Direct3D9BitmapColorSourceContextParameters parameters = CreateParameters(
            MilBitmapInterpolationMode.Linear,
            prefilterEnabled: false,
            isFantScalerDisabled: true);

        Assert.IsFalse(parameters.PrefilterEnabled);
    }

    [TestMethod]
    [DataRow((int) MilBitmapInterpolationMode.TriLinear)]
    [DataRow((int) MilBitmapInterpolationMode.Anisotropic)]
    public void WhenSoftwareDeviceUsesMipMappingThenInterpolationFallsBackToLinear(int interpolationMode)
    {
        Direct3D9BitmapColorSourceContextParameters parameters = CreateParameters(
            (MilBitmapInterpolationMode) interpolationMode,
            isSoftwareDevice: true,
            canAutoGenerateMipmaps: true,
            canGenerateMipmapsWithStretchRect: true);

        Assert.AreEqual(MilBitmapInterpolationMode.Linear, parameters.InterpolationMode);
    }

    [TestMethod]
    [DataRow((int) MilBitmapInterpolationMode.TriLinear)]
    [DataRow((int) MilBitmapInterpolationMode.Anisotropic)]
    public void WhenHardwareCannotGenerateMipmapsThenInterpolationFallsBackToLinear(int interpolationMode)
    {
        Direct3D9BitmapColorSourceContextParameters parameters =
            CreateParameters((MilBitmapInterpolationMode) interpolationMode);

        Assert.AreEqual(MilBitmapInterpolationMode.Linear, parameters.InterpolationMode);
    }

    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    public void WhenHardwareCanGenerateMipmapsThenMipMappedInterpolationIsPreserved(
        bool canAutoGenerateMipmaps,
        bool canGenerateMipmapsWithStretchRect)
    {
        Direct3D9BitmapColorSourceContextParameters parameters = CreateParameters(
            MilBitmapInterpolationMode.Anisotropic,
            canAutoGenerateMipmaps: canAutoGenerateMipmaps,
            canGenerateMipmapsWithStretchRect: canGenerateMipmapsWithStretchRect);

        Assert.AreEqual(MilBitmapInterpolationMode.Anisotropic, parameters.InterpolationMode);
    }

    [TestMethod]
    public void WhenInterpolationDoesNotUseMipMappingThenMissingMipmapSupportDoesNotChangeIt()
    {
        Direct3D9BitmapColorSourceContextParameters parameters = CreateParameters(
            MilBitmapInterpolationMode.Cubic);

        Assert.AreEqual(MilBitmapInterpolationMode.Cubic, parameters.InterpolationMode);
    }

    [TestMethod]
    public void WhenExplicitSettingsAreUsedThenBrushIdentityIsEmpty()
    {
        Direct3D9BitmapColorSourceContextParameters parameters =
            Direct3D9BitmapColorSourceContextParameters.FromExplicitSettings(
                MilBitmapInterpolationMode.TriLinear,
                prefilterEnabled: true,
                MilPixelFormat.Bgr32Bpp101010,
                MilBitmapWrapMode.Border);

        Assert.AreEqual(
            new Direct3D9BitmapColorSourceContextParameters(
                0,
                MilBitmapInterpolationMode.TriLinear,
                true,
                MilPixelFormat.Bgr32Bpp101010,
                0,
                MilBitmapWrapMode.Border),
            parameters);
    }

    [TestMethod]
    public void WhenBitmapBrushIsNullThenBrushContextCreationIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            CreateParameters(MilBitmapInterpolationMode.Linear, bitmapBrush: 0));
    }

    [TestMethod]
    [DataRow((int) MilBitmapInterpolationMode.TriLinear, (int) Direct3D9TextureMipMapLevel.All)]
    [DataRow((int) MilBitmapInterpolationMode.Anisotropic, (int) Direct3D9TextureMipMapLevel.All)]
    [DataRow((int) MilBitmapInterpolationMode.Linear, (int) Direct3D9TextureMipMapLevel.One)]
    public void WhenTexturePropertiesAreComputedThenMipLevelFollowsInterpolation(
        int interpolationMode,
        int expectedMipMapLevel)
    {
        int result = ComputeTextureProperties(
            (MilBitmapInterpolationMode) interpolationMode,
            MilBitmapWrapMode.Tile,
            canFallback: false,
            out Direct3D9BitmapRealizationProperties properties);

        Assert.AreEqual(
            (0, (Direct3D9TextureMipMapLevel) expectedMipMapLevel),
            (result, properties.MipMapLevel));
    }

    [TestMethod]
    public void WhenTexturePropertiesAreComputedThenSourceTargetAndBorderAlphaAreForwarded()
    {
        MilPixelFormat actualSourceFormat = MilPixelFormat.Undefined;
        MilPixelFormat actualTargetFormat = MilPixelFormat.Undefined;
        bool actualForceAlpha = false;

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeTextureProperties(
            5,
            Direct3D9BitmapColorSourceContextParameters.FromExplicitSettings(
                MilBitmapInterpolationMode.Cubic,
                prefilterEnabled: true,
                MilPixelFormat.Bgr32Bpp101010,
                MilBitmapWrapMode.Border),
            canFallback: false,
            static (nint bitmapSource, out MilPixelFormat format) =>
            {
                format = MilPixelFormat.Rgb128BppFloat;
                return Direct3D9Factory.SuccessHResult;
            },
            (MilPixelFormat sourceFormat, MilPixelFormat targetFormat, bool forceAlpha, out MilPixelFormat textureFormat) =>
            {
                actualSourceFormat = sourceFormat;
                actualTargetFormat = targetFormat;
                actualForceAlpha = forceAlpha;
                textureFormat = MilPixelFormat.Prgba128BppFloat;
                return Direct3D9Factory.SuccessHResult;
            },
            out Direct3D9BitmapRealizationProperties properties);

        Assert.AreEqual(
            (0, MilPixelFormat.Rgb128BppFloat, MilPixelFormat.Bgr32Bpp101010, true, MilPixelFormat.Prgba128BppFloat),
            (result, actualSourceFormat, actualTargetFormat, actualForceAlpha, properties.TextureFormat));
    }

    [TestMethod]
    public void WhenWrapModeIsNotBorderThenTextureFormatDoesNotForceAlpha()
    {
        bool actualForceAlpha = true;

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeTextureProperties(
            5,
            CreateParameters(MilBitmapInterpolationMode.Linear),
            canFallback: false,
            static (nint bitmapSource, out MilPixelFormat format) =>
            {
                format = MilPixelFormat.Bgr32Bpp;
                return Direct3D9Factory.SuccessHResult;
            },
            (MilPixelFormat sourceFormat, MilPixelFormat targetFormat, bool forceAlpha, out MilPixelFormat textureFormat) =>
            {
                actualForceAlpha = forceAlpha;
                textureFormat = MilPixelFormat.Bgr32Bpp;
                return Direct3D9Factory.SuccessHResult;
            },
            out _);

        Assert.AreEqual((0, false), (result, actualForceAlpha));
    }

    [TestMethod]
    public void WhenBitmapPixelFormatQueryFailsThenTextureFormatIsNotQueried()
    {
        bool textureFormatQueried = false;

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeTextureProperties(
            5,
            CreateParameters(MilBitmapInterpolationMode.Linear),
            canFallback: true,
            static (nint bitmapSource, out MilPixelFormat format) =>
            {
                format = MilPixelFormat.Undefined;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (MilPixelFormat sourceFormat, MilPixelFormat targetFormat, bool forceAlpha, out MilPixelFormat textureFormat) =>
            {
                textureFormatQueried = true;
                textureFormat = MilPixelFormat.Undefined;
                return Direct3D9Factory.SuccessHResult;
            },
            out _);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false),
            (result, textureFormatQueried));
    }

    [TestMethod]
    public void WhenTextureFormatIsUnsupportedAndFallbackIsAllowedThenNotImplementedIsReturned()
    {
        int result = ComputeTextureProperties(
            MilBitmapInterpolationMode.Linear,
            MilBitmapWrapMode.Tile,
            canFallback: true,
            out _,
            Direct3D9Factory.UnsupportedPixelFormatHResult);

        Assert.AreEqual(Direct3D9Factory.NotImplementedHResult, result);
    }

    [TestMethod]
    public void WhenTextureFormatIsUnsupportedAndFallbackIsUnavailableThenOriginalErrorIsReturned()
    {
        int result = ComputeTextureProperties(
            MilBitmapInterpolationMode.Linear,
            MilBitmapWrapMode.Tile,
            canFallback: false,
            out _,
            Direct3D9Factory.UnsupportedPixelFormatHResult);

        Assert.AreEqual(Direct3D9Factory.UnsupportedPixelFormatHResult, result);
    }

    [TestMethod]
    public void WhenTextureSizeIsComputedThenBitmapSizeAndContextAreForwarded()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties();
        (uint MaximumWidth, uint MaximumHeight, nint Bounds, nint Matrix, MilBitmapWrapMode WrapMode,
            bool PrefilterEnabled, float Threshold, bool CanFallback, uint BitmapWidth, uint BitmapHeight) actual = default;

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeTextureSize(
            5,
            4096,
            2048,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            static (nint bitmapSource, out uint width, out uint height) =>
            {
                width = 640;
                height = 480;
                return Direct3D9Factory.SuccessHResult;
            },
            (uint maximumWidth, uint maximumHeight, nint bounds, nint matrix, MilBitmapWrapMode wrapMode,
                bool prefilterEnabled, float threshold, bool canFallback,
                ref Direct3D9BitmapRealizationProperties realizationProperties) =>
            {
                actual = (maximumWidth, maximumHeight, bounds, matrix, wrapMode, prefilterEnabled, threshold,
                    canFallback, realizationProperties.BitmapWidth, realizationProperties.BitmapHeight);
                return Direct3D9Factory.SuccessHResult;
            },
            ref properties);

        Assert.AreEqual(
            (0, 4096u, 2048u, (nint) 21, (nint) 22, MilBitmapWrapMode.Tile, true, 0.5f, true, 640u, 480u),
            (result, actual.MaximumWidth, actual.MaximumHeight, actual.Bounds, actual.Matrix, actual.WrapMode,
                actual.PrefilterEnabled, actual.Threshold, actual.CanFallback, actual.BitmapWidth, actual.BitmapHeight));
    }

    [TestMethod]
    public void WhenBitmapSizeQueryFailsThenRealizationSizeIsNotComputed()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties();
        bool realizationSizeComputed = false;

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeTextureSize(
            5,
            4096,
            2048,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            static (nint bitmapSource, out uint width, out uint height) =>
            {
                width = 0;
                height = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (uint maximumWidth, uint maximumHeight, nint bounds, nint matrix, MilBitmapWrapMode wrapMode,
                bool prefilterEnabled, float threshold, bool canFallback,
                ref Direct3D9BitmapRealizationProperties realizationProperties) =>
            {
                realizationSizeComputed = true;
                return Direct3D9Factory.SuccessHResult;
            },
            ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, 0u, 0u),
            (result, realizationSizeComputed, properties.BitmapWidth, properties.BitmapHeight));
    }

    [TestMethod]
    public void WhenRealizationSizeComputationFailsThenItsErrorAndBitmapSizeArePreserved()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties();

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeTextureSize(
            5,
            4096,
            2048,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            static (nint bitmapSource, out uint width, out uint height) =>
            {
                width = 800;
                height = 600;
                return Direct3D9Factory.SuccessHResult;
            },
            static (uint maximumWidth, uint maximumHeight, nint bounds, nint matrix, MilBitmapWrapMode wrapMode,
                bool prefilterEnabled, float threshold, bool canFallback,
                ref Direct3D9BitmapRealizationProperties realizationProperties) =>
                Direct3D9Factory.GenericFailureHResult,
            ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 800u, 600u),
            (result, properties.BitmapWidth, properties.BitmapHeight));
    }

    [TestMethod]
    [DataRow(4096u, 2048u, 4096u, 2048u)]
    [DataRow(4097u, 2049u, 4096u, 2048u)]
    public void WhenMipMappedBitmapReachesMaximumTextureSizeThenEachDimensionIsClamped(
        uint bitmapWidth,
        uint bitmapHeight,
        uint expectedWidth,
        uint expectedHeight)
    {
        Direct3D9BitmapRealizationProperties properties = CreateMipMappedRealizationProperties(bitmapWidth, bitmapHeight);

        Direct3D9BitmapRealizationParameterComputer.ComputeMipMappedTextureSize(
            4096,
            2048,
            MilBitmapWrapMode.Extend,
            prefilterEnabled: false,
            ref properties);

        Assert.AreEqual((expectedWidth, expectedHeight), (properties.Width, properties.Height));
    }

    [TestMethod]
    [DataRow(true, (int) MilBitmapWrapMode.Extend)]
    [DataRow(false, (int) MilBitmapWrapMode.Tile)]
    [DataRow(false, (int) MilBitmapWrapMode.Border)]
    public void WhenMipMappedBitmapRequiresFilteringOrWrappingThenDimensionsRoundUpToPowerOfTwo(
        bool prefilterEnabled,
        int wrapMode)
    {
        Direct3D9BitmapRealizationProperties properties = CreateMipMappedRealizationProperties(641, 257);

        Direct3D9BitmapRealizationParameterComputer.ComputeMipMappedTextureSize(
            4096,
            2048,
            (MilBitmapWrapMode) wrapMode,
            prefilterEnabled,
            ref properties);

        Assert.AreEqual((1024u, 512u), (properties.Width, properties.Height));
    }

    [TestMethod]
    public void WhenMipMappedBitmapUsesExtendWithoutPrefilteringThenNaturalDimensionsArePreserved()
    {
        Direct3D9BitmapRealizationProperties properties = CreateMipMappedRealizationProperties(641, 257);

        Direct3D9BitmapRealizationParameterComputer.ComputeMipMappedTextureSize(
            4096,
            2048,
            MilBitmapWrapMode.Extend,
            prefilterEnabled: false,
            ref properties);

        Assert.AreEqual((641u, 257u), (properties.Width, properties.Height));
    }

    [TestMethod]
    public void WhenMipMappedTextureSizeIsComputedThenMinimumRealizationRectIsMarkedComputed()
    {
        Direct3D9BitmapRealizationProperties properties = CreateMipMappedRealizationProperties(640, 480);

        Direct3D9BitmapRealizationParameterComputer.ComputeMipMappedTextureSize(
            4096,
            2048,
            MilBitmapWrapMode.Extend,
            prefilterEnabled: false,
            ref properties);

        Assert.IsTrue(properties.IsMinimumRealizationRectComputed);
    }

    [TestMethod]
    public void WhenNonMipMappedPrefilteringIsEnabledThenDimensionsAndContextAreForwarded()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            BitmapWidth = 640,
            BitmapHeight = 480
        };
        (nint Matrix, uint BitmapWidth, uint BitmapHeight, float Threshold) actual = default;

        Direct3D9BitmapRealizationParameterComputer.ComputeNonMipMappedTextureSize(
            22,
            prefilterEnabled: true,
            1.5f,
            (nint matrix, uint bitmapWidth, uint bitmapHeight, float threshold, out uint width, out uint height) =>
            {
                actual = (matrix, bitmapWidth, bitmapHeight, threshold);
                width = 320;
                height = 240;
            },
            ref properties);

        Assert.AreEqual(((nint) 22, 640u, 480u, 1.5f), actual);
    }

    [TestMethod]
    public void WhenNonMipMappedPrefilteringIsEnabledThenComputedDimensionsArePreserved()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            BitmapWidth = 640,
            BitmapHeight = 480
        };

        Direct3D9BitmapRealizationParameterComputer.ComputeNonMipMappedTextureSize(
            22,
            prefilterEnabled: true,
            1.5f,
            static (nint matrix, uint bitmapWidth, uint bitmapHeight, float threshold, out uint width, out uint height) =>
            {
                width = 320;
                height = 240;
            },
            ref properties);

        Assert.AreEqual((320u, 240u), (properties.Width, properties.Height));
    }

    [TestMethod]
    public void WhenNonMipMappedPrefilteringIsDisabledThenNaturalDimensionsArePreserved()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            BitmapWidth = 640,
            BitmapHeight = 480
        };
        bool dimensionsComputed = false;

        Direct3D9BitmapRealizationParameterComputer.ComputeNonMipMappedTextureSize(
            22,
            prefilterEnabled: false,
            1.5f,
            (nint matrix, uint bitmapWidth, uint bitmapHeight, float threshold, out uint width, out uint height) =>
            {
                dimensionsComputed = true;
                width = 0;
                height = 0;
            },
            ref properties);

        Assert.AreEqual((false, 640u, 480u), (dimensionsComputed, properties.Width, properties.Height));
    }

    [TestMethod]
    public void WhenNonMipMappedTextureSizeIsComputedThenMinimumRealizationRectRemainsPending()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            BitmapWidth = 640,
            BitmapHeight = 480
        };

        Direct3D9BitmapRealizationParameterComputer.ComputeNonMipMappedTextureSize(
            22,
            prefilterEnabled: false,
            1.5f,
            static (nint matrix, uint bitmapWidth, uint bitmapHeight, float threshold, out uint width, out uint height) =>
            {
                width = 0;
                height = 0;
            },
            ref properties);

        Assert.IsFalse(properties.IsMinimumRealizationRectComputed);
    }

    [TestMethod]
    public void WhenTextureFitsDeviceLimitsThenFullSourceRectangleIsInitialized()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 640,
            Height = 480
        };
        bool minimumBoundsComputed = false;

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeMinimumTextureBounds(
            1024,
            1024,
            21,
            prefilterEnabled: true,
            canFallback: true,
            (nint bounds, Direct3D9BitmapRealizationProperties input, ref Direct3D9BitmapRealizationRectangle minimumBounds) =>
            {
                minimumBoundsComputed = true;
                return false;
            },
            ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, false, false, new Direct3D9BitmapRealizationRectangle(0, 0, 640, 480)),
            (result, minimumBoundsComputed, properties.IsMinimumRealizationRectComputed, properties.SourceContained));
    }

    [TestMethod]
    public void WhenTextureExceedsDeviceLimitsThenMinimumBoundsContextAndRectangleAreForwarded()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            BitmapWidth = 1600,
            BitmapHeight = 1200,
            Width = 1200,
            Height = 900
        };
        (nint Bounds, Direct3D9BitmapRealizationRectangle InitialRectangle, bool MinimumComputed) actual = default;

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeMinimumTextureBounds(
            1024,
            768,
            21,
            prefilterEnabled: false,
            canFallback: false,
            (nint bounds, Direct3D9BitmapRealizationProperties input, ref Direct3D9BitmapRealizationRectangle minimumBounds) =>
            {
                actual = (bounds, minimumBounds, input.IsMinimumRealizationRectComputed);
                minimumBounds = new Direct3D9BitmapRealizationRectangle(10, 20, 810, 620);
                return true;
            },
            ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, ((nint) 21, new Direct3D9BitmapRealizationRectangle(0, 0, 1200, 900), true), new Direct3D9BitmapRealizationRectangle(10, 20, 810, 620)),
            (result, actual, properties.SourceContained));
    }

    [TestMethod]
    public void WhenNoMinimumBoundsAreFoundAndHighQualityFallbackIsAllowedThenNotImplementedIsReturned()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 1200,
            Height = 900
        };

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeMinimumTextureBounds(
            1024,
            768,
            21,
            prefilterEnabled: true,
            canFallback: true,
            static (nint bounds, Direct3D9BitmapRealizationProperties input, ref Direct3D9BitmapRealizationRectangle minimumBounds) => false,
            ref properties);

        Assert.AreEqual(Direct3D9Factory.NotImplementedHResult, result);
    }

    [TestMethod]
    public void WhenMinimumBoundsStillExceedDeviceLimitsAndHighQualityFallbackIsAllowedThenNotImplementedIsReturned()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 1200,
            Height = 900
        };

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeMinimumTextureBounds(
            1024,
            768,
            21,
            prefilterEnabled: true,
            canFallback: true,
            static (nint bounds, Direct3D9BitmapRealizationProperties input, ref Direct3D9BitmapRealizationRectangle minimumBounds) =>
            {
                minimumBounds = new Direct3D9BitmapRealizationRectangle(0, 0, 1025, 768);
                return true;
            },
            ref properties);

        Assert.AreEqual(Direct3D9Factory.NotImplementedHResult, result);
    }

    [TestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public void WhenHighQualityFallbackIsUnavailableThenMissingMinimumBoundsDoNotReturnNotImplemented(
        bool prefilterEnabled,
        bool canFallback)
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 1200,
            Height = 900
        };

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeMinimumTextureBounds(
            1024,
            768,
            21,
            prefilterEnabled,
            canFallback,
            static (nint bounds, Direct3D9BitmapRealizationProperties input, ref Direct3D9BitmapRealizationRectangle minimumBounds) => false,
            ref properties);

        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
    }

    [TestMethod]
    public void WhenMinimumBoundsAreMissingAndFallbackIsUnavailableThenTextureDimensionsAreClamped()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 1200,
            Height = 900
        };

        int result = Direct3D9BitmapRealizationParameterComputer.ComputeMinimumTextureBounds(
            1024,
            768,
            21,
            prefilterEnabled: false,
            canFallback: false,
            static (nint bounds, Direct3D9BitmapRealizationProperties input, ref Direct3D9BitmapRealizationRectangle minimumBounds) => false,
            ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 1024u, 768u, new Direct3D9BitmapRealizationRectangle(0, 0, 1024, 768)),
            (result, properties.Width, properties.Height, properties.SourceContained));
    }

    [TestMethod]
    public void WhenOneMinimumDimensionExceedsTheLimitThenThatAxisIsClampedAndOtherSubRectangleIsPreserved()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 1200,
            Height = 900
        };

        Direct3D9BitmapRealizationParameterComputer.ComputeMinimumTextureBounds(
            1024,
            768,
            21,
            prefilterEnabled: false,
            canFallback: false,
            static (nint bounds, Direct3D9BitmapRealizationProperties input, ref Direct3D9BitmapRealizationRectangle minimumBounds) =>
            {
                minimumBounds = new Direct3D9BitmapRealizationRectangle(50, 40, 1100, 700);
                return true;
            },
            ref properties);

        Assert.AreEqual(
            (1024u, 900u, true, new Direct3D9BitmapRealizationRectangle(0, 40, 1024, 700)),
            (properties.Width, properties.Height, properties.OnlyContainsSubRectangleOfSource, properties.SourceContained));
    }

    [TestMethod]
    public void WhenMinimumBoundsFitTextureLimitsThenOnlySubRectangleStateIsSet()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 1200,
            Height = 900
        };

        Direct3D9BitmapRealizationParameterComputer.ComputeMinimumTextureBounds(
            1024,
            768,
            21,
            prefilterEnabled: false,
            canFallback: false,
            static (nint bounds, Direct3D9BitmapRealizationProperties input, ref Direct3D9BitmapRealizationRectangle minimumBounds) =>
            {
                minimumBounds = new Direct3D9BitmapRealizationRectangle(10, 20, 810, 620);
                return true;
            },
            ref properties);

        Assert.IsTrue(properties.OnlyContainsSubRectangleOfSource);
    }

    [TestMethod]
    [DataRow((int) MilBitmapWrapMode.Extend, (int) Textureaddress.Clamp, (int) Textureaddress.Clamp)]
    [DataRow((int) MilBitmapWrapMode.FlipX, (int) Textureaddress.Mirror, (int) Textureaddress.Wrap)]
    [DataRow((int) MilBitmapWrapMode.FlipY, (int) Textureaddress.Wrap, (int) Textureaddress.Mirror)]
    [DataRow((int) MilBitmapWrapMode.FlipXY, (int) Textureaddress.Mirror, (int) Textureaddress.Mirror)]
    [DataRow((int) MilBitmapWrapMode.Tile, (int) Textureaddress.Wrap, (int) Textureaddress.Wrap)]
    [DataRow((int) MilBitmapWrapMode.Border, (int) Textureaddress.Border, (int) Textureaddress.Border)]
    public void WhenTextureLayoutIsInitializedThenWrapModeIsConvertedToNativeAddressModes(
        int wrapMode,
        int expectedAddressU,
        int expectedAddressV)
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(10, 20, 310, 220)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            (MilBitmapWrapMode) wrapMode,
            requiresPowerOfTwoTextures: false,
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            canFallback: true,
            ref properties);

        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);

        Assert.AreEqual(
            (300u, Direct3D9TexelLayout.Natural, expectedAddressU, 200u, Direct3D9TexelLayout.Natural, expectedAddressV),
            (properties.LayoutU.Length, properties.LayoutU.TexelLayout, (int) properties.LayoutU.TextureAddress,
             properties.LayoutV.Length, properties.LayoutV.TexelLayout, (int) properties.LayoutV.TextureAddress));
    }

    [TestMethod]
    public void WhenMipMappedLayoutDimensionsAreNotPowersOfTwoThenTheyUseFirstOnlyLayout()
    {
        Direct3D9BitmapRealizationProperties properties = CreateMipMappedRealizationProperties(300, 200) with
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 300, 200)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            MilBitmapWrapMode.Tile,
            requiresPowerOfTwoTextures: false,
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            canFallback: true,
            ref properties);

        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);

        Assert.AreEqual(
            (512u, Direct3D9TexelLayout.FirstOnly, 256u, Direct3D9TexelLayout.FirstOnly),
            (properties.LayoutU.Length, properties.LayoutU.TexelLayout,
             properties.LayoutV.Length, properties.LayoutV.TexelLayout));
    }

    [TestMethod]
    public void WhenMipMappedLayoutDimensionsArePowersOfTwoThenNaturalLayoutIsPreserved()
    {
        Direct3D9BitmapRealizationProperties properties = CreateMipMappedRealizationProperties(256, 128) with
        {
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 256, 128)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            MilBitmapWrapMode.FlipXY,
            requiresPowerOfTwoTextures: false,
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            canFallback: true,
            ref properties);

        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);

        Assert.AreEqual(
            (256u, Direct3D9TexelLayout.Natural, 128u, Direct3D9TexelLayout.Natural),
            (properties.LayoutU.Length, properties.LayoutU.TexelLayout,
             properties.LayoutV.Length, properties.LayoutV.TexelLayout));
    }

    [TestMethod]
    public void WhenPowerOfTwoTexturesAreRequiredThenAlternateNonPowerOfTwoWidthIsClamped()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 400,
            Height = 256,
            SourceContained = new Direct3D9BitmapRealizationRectangle(10, 0, 310, 256)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            MilBitmapWrapMode.FlipXY,
            requiresPowerOfTwoTextures: true,
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            canFallback: true,
            ref properties);

        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
        Assert.AreEqual(
            (Textureaddress.Clamp, Textureaddress.Mirror),
            (properties.LayoutU.TextureAddress, properties.LayoutV.TextureAddress));
    }

    [TestMethod]
    public void WhenPowerOfTwoTexturesAreRequiredThenAlternateNonPowerOfTwoHeightIsClamped()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 256,
            Height = 300,
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 10, 256, 210)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            MilBitmapWrapMode.Tile,
            requiresPowerOfTwoTextures: true,
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            canFallback: true,
            ref properties);

        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
        Assert.AreEqual(
            (Textureaddress.Wrap, Textureaddress.Clamp),
            (properties.LayoutU.TextureAddress, properties.LayoutV.TextureAddress));
    }

    [TestMethod]
    public void WhenAlternateSourceLengthIsPowerOfTwoThenOriginalAddressingIsPreserved()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 300,
            Height = 200,
            SourceContained = new Direct3D9BitmapRealizationRectangle(10, 20, 266, 148)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            MilBitmapWrapMode.FlipXY,
            requiresPowerOfTwoTextures: true,
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            canFallback: true,
            ref properties);

        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
        Assert.AreEqual(
            (Textureaddress.Mirror, Textureaddress.Mirror),
            (properties.LayoutU.TextureAddress, properties.LayoutV.TextureAddress));
    }

    [TestMethod]
    [DataRow((int) Textureaddress.Wrap, (int) Direct3D9TexelLayout.EdgeWrapped)]
    [DataRow((int) Textureaddress.Mirror, (int) Direct3D9TexelLayout.EdgeMirrored)]
    public void WhenConditionalNonPowerOfTwoLayoutCanAddBordersThenEdgesAreAddedAndAddressingIsClamped(
        int textureAddress,
        int expectedTexelLayout)
    {
        Direct3D9BitmapDimensionLayout layout = new(
            300,
            Direct3D9TexelLayout.Natural,
            (Textureaddress) textureAddress);

        int result = Direct3D9BitmapRealizationParameterComputer.AdjustLayoutForConditionalNonPowerOfTwo(
            302,
            ref layout);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 302u, expectedTexelLayout, Textureaddress.Clamp),
            (result, layout.Length, (int) layout.TexelLayout, layout.TextureAddress));
    }

    [TestMethod]
    [DataRow((int) Textureaddress.Wrap)]
    [DataRow((int) Textureaddress.Mirror)]
    public void WhenConditionalNonPowerOfTwoLayoutCannotAddBordersThenNotImplementedIsReturned(int textureAddress)
    {
        Direct3D9BitmapDimensionLayout originalLayout = new(
            300,
            Direct3D9TexelLayout.Natural,
            (Textureaddress) textureAddress);
        Direct3D9BitmapDimensionLayout layout = originalLayout;

        int result = Direct3D9BitmapRealizationParameterComputer.AdjustLayoutForConditionalNonPowerOfTwo(
            301,
            ref layout);

        Assert.AreEqual((Direct3D9Factory.NotImplementedHResult, originalLayout), (result, layout));
    }

    [TestMethod]
    public void WhenConditionalNonPowerOfTwoLayoutUsesClampThenNaturalLayoutIsPreserved()
    {
        Direct3D9BitmapDimensionLayout layout = new(
            300,
            Direct3D9TexelLayout.FirstOnly,
            Textureaddress.Clamp);

        int result = Direct3D9BitmapRealizationParameterComputer.AdjustLayoutForConditionalNonPowerOfTwo(
            300,
            ref layout);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 300u, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            (result, layout.Length, layout.TexelLayout, layout.TextureAddress));
    }

    [TestMethod]
    public void WhenConditionalNonPowerOfTwoLayoutUsesBorderThenLayoutIsUnchanged()
    {
        Direct3D9BitmapDimensionLayout originalLayout = new(
            300,
            Direct3D9TexelLayout.Natural,
            Textureaddress.Border);
        Direct3D9BitmapDimensionLayout layout = originalLayout;

        int result = Direct3D9BitmapRealizationParameterComputer.AdjustLayoutForConditionalNonPowerOfTwo(
            300,
            ref layout);

        Assert.AreEqual((Direct3D9Factory.SuccessHResult, originalLayout), (result, layout));
    }

    [TestMethod]
    public void WhenConditionalNonPowerOfTwoLayoutCanFallbackThenBothDimensionsAddWrappedEdges()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 300,
            Height = 200,
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 300, 200)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            MilBitmapWrapMode.Tile,
            requiresPowerOfTwoTextures: true,
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            canFallback: true,
            ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult,
             302u, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Clamp,
             202u, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Clamp),
            (result,
             properties.LayoutU.Length, properties.LayoutU.TexelLayout, properties.LayoutU.TextureAddress,
             properties.LayoutV.Length, properties.LayoutV.TexelLayout, properties.LayoutV.TextureAddress));
    }

    [TestMethod]
    public void WhenConditionalNonPowerOfTwoLayoutCannotFallbackThenDimensionsUseNaturalPowerOfTwoLayout()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 300,
            Height = 200,
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 300, 200)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            MilBitmapWrapMode.FlipXY,
            requiresPowerOfTwoTextures: true,
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            canFallback: false,
            ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult,
             512u, 256u,
             new Direct3D9BitmapRealizationRectangle(0, 0, 512, 256),
             new Direct3D9BitmapDimensionLayout(512, Direct3D9TexelLayout.Natural, Textureaddress.Mirror),
             new Direct3D9BitmapDimensionLayout(256, Direct3D9TexelLayout.Natural, Textureaddress.Mirror)),
            (result, properties.Width, properties.Height, properties.SourceContained, properties.LayoutU, properties.LayoutV));
    }

    [TestMethod]
    public void WhenConditionalNonPowerOfTwoWidthCannotAddEdgesThenFirstFailureIsReturned()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 300,
            Height = 200,
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 300, 200)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            MilBitmapWrapMode.Tile,
            requiresPowerOfTwoTextures: true,
            maximumTextureWidth: 301,
            maximumTextureHeight: 1024,
            canFallback: true,
            ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.NotImplementedHResult,
             new Direct3D9BitmapDimensionLayout(300, Direct3D9TexelLayout.Natural, Textureaddress.Wrap),
             new Direct3D9BitmapDimensionLayout(200, Direct3D9TexelLayout.Natural, Textureaddress.Wrap)),
            (result, properties.LayoutU, properties.LayoutV));
    }

    [TestMethod]
    public void WhenOnlyVerticalDimensionNeedsConditionalLayoutThenInitializationReconcilesHorizontalLayout()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            Width = 256,
            Height = 200,
            SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 256, 200)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeTextureLayoutAndWrapping(
            MilBitmapWrapMode.Tile,
            requiresPowerOfTwoTextures: true,
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            canFallback: true,
            ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult,
             new Direct3D9BitmapDimensionLayout(258, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Clamp),
             new Direct3D9BitmapDimensionLayout(202, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Clamp)),
            (result, properties.LayoutU, properties.LayoutV));
    }

    [TestMethod]
    public void WhenOnlyVerticalLayoutHasWrappedEdgesThenHorizontalLayoutIsReconciled()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            LayoutU = new Direct3D9BitmapDimensionLayout(256, Direct3D9TexelLayout.Natural, Textureaddress.Wrap),
            LayoutV = new Direct3D9BitmapDimensionLayout(202, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Clamp)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.ReconcileLayouts(1024, 1024, ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult,
             new Direct3D9BitmapDimensionLayout(258, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Clamp),
             new Direct3D9BitmapDimensionLayout(202, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Clamp)),
            (result, properties.LayoutU, properties.LayoutV));
    }

    [TestMethod]
    public void WhenOnlyHorizontalLayoutHasMirroredEdgesThenVerticalLayoutIsReconciled()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            LayoutU = new Direct3D9BitmapDimensionLayout(302, Direct3D9TexelLayout.EdgeMirrored, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(128, Direct3D9TexelLayout.Natural, Textureaddress.Mirror)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.ReconcileLayouts(1024, 1024, ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult,
             new Direct3D9BitmapDimensionLayout(302, Direct3D9TexelLayout.EdgeMirrored, Textureaddress.Clamp),
             new Direct3D9BitmapDimensionLayout(130, Direct3D9TexelLayout.EdgeMirrored, Textureaddress.Clamp)),
            (result, properties.LayoutU, properties.LayoutV));
    }

    [TestMethod]
    public void WhenReconciledLayoutCannotAddEdgesThenFailurePreservesBothLayouts()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            LayoutU = new Direct3D9BitmapDimensionLayout(256, Direct3D9TexelLayout.Natural, Textureaddress.Wrap),
            LayoutV = new Direct3D9BitmapDimensionLayout(202, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Clamp)
        };

        int result = Direct3D9BitmapRealizationParameterComputer.ReconcileLayouts(257, 1024, ref properties);

        Assert.AreEqual(
            (Direct3D9Factory.NotImplementedHResult,
             new Direct3D9BitmapDimensionLayout(256, Direct3D9TexelLayout.Natural, Textureaddress.Wrap),
             new Direct3D9BitmapDimensionLayout(202, Direct3D9TexelLayout.EdgeWrapped, Textureaddress.Clamp)),
            (result, properties.LayoutU, properties.LayoutV));
    }

    [TestMethod]
    public void WhenSingleLevelTextureDescriptionIsRequiredThenNativeCreationFieldsAreProduced()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            TextureFormat = MilPixelFormat.Pbgra32Bpp,
            LayoutU = new Direct3D9BitmapDimensionLayout(300, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(200, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

        SurfaceDesc description = Direct3D9BitmapRealizationParameterComputer.GetRequiredTextureDescription(
            canAutoGenerateMipmaps: true,
            properties,
            out uint levels);

        Assert.AreEqual(
            (Format.A8R8G8B8, Resourcetype.Texture, 0u, Pool.Default, MultisampleType.MultisampleNone, 0u, 300u, 200u, 1u),
            (description.Format, description.Type, description.Usage, description.Pool, description.MultiSampleType,
             description.MultiSampleQuality, description.Width, description.Height, levels));
    }

    [TestMethod]
    public void WhenAutomaticMipMapGenerationIsAvailableThenAutogenUsageAndZeroLevelsAreRequired()
    {
        Direct3D9BitmapRealizationProperties properties = CreateMipMappedRealizationProperties(256, 128) with
        {
            TextureFormat = MilPixelFormat.Bgr32Bpp,
            LayoutU = new Direct3D9BitmapDimensionLayout(256, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(128, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

        SurfaceDesc description = Direct3D9BitmapRealizationParameterComputer.GetRequiredTextureDescription(
            canAutoGenerateMipmaps: true,
            properties,
            out uint levels);

        Assert.AreEqual(((uint) D3D9.UsageAutogenmipmap, 0u), (description.Usage, levels));
    }

    [TestMethod]
    public void WhenStretchRectGeneratesMipMapsThenRenderTargetUsageAndAllLevelsAreRequired()
    {
        Direct3D9BitmapRealizationProperties properties = CreateMipMappedRealizationProperties(256, 64) with
        {
            TextureFormat = MilPixelFormat.Bgr32Bpp101010,
            LayoutU = new Direct3D9BitmapDimensionLayout(256, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(64, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

        SurfaceDesc description = Direct3D9BitmapRealizationParameterComputer.GetRequiredTextureDescription(
            canAutoGenerateMipmaps: false,
            properties,
            out uint levels);

        Assert.AreEqual(((uint) D3D9.UsageRendertarget, 9u), (description.Usage, levels));
    }

    [TestMethod]
    [DataRow((int) MilPixelFormat.Bgr24Bpp, (int) Format.R8G8B8)]
    [DataRow((int) MilPixelFormat.Bgra32Bpp, (int) Format.A8R8G8B8)]
    [DataRow((int) MilPixelFormat.Bgr16Bpp565, (int) Format.R5G6B5)]
    [DataRow((int) MilPixelFormat.Bgr16Bpp555, (int) Format.X1R5G5B5)]
    [DataRow((int) MilPixelFormat.Indexed8Bpp, (int) Format.P8)]
    [DataRow((int) MilPixelFormat.Gray8Bpp, (int) Format.L8)]
    [DataRow((int) MilPixelFormat.Bgr32Bpp101010, (int) Format.A2R10G10B10)]
    [DataRow((int) MilPixelFormat.Rgba128BppFloat, (int) Format.A32B32G32R32f)]
    [DataRow((int) MilPixelFormat.Undefined, (int) Format.Unknown)]
    public void WhenTextureDescriptionIsRequiredThenMilFormatIsConvertedToNativeFormat(
        int textureFormat,
        int expectedFormat)
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            TextureFormat = (MilPixelFormat) textureFormat,
            LayoutU = new Direct3D9BitmapDimensionLayout(1, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(1, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

        SurfaceDesc description = Direct3D9BitmapRealizationParameterComputer.GetRequiredTextureDescription(
            canAutoGenerateMipmaps: false,
            properties,
            out _);

        Assert.AreEqual((Format) expectedFormat, description.Format);
    }

    [TestMethod]
    public void WhenBitmapColorSourceIsCreatedAsRenderTargetThenRenderTargetUsageIsAdded()
    {
        Direct3D9BitmapRealizationProperties properties = CreateRealizationProperties() with
        {
            TextureFormat = MilPixelFormat.Pbgra32Bpp,
            LayoutU = new Direct3D9BitmapDimensionLayout(300, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(200, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

        Direct3D9BitmapTextureRequirements requirements =
            Direct3D9BitmapRealizationParameterComputer.GetTextureCreationRequirements(
                canAutoGenerateMipmaps: false,
                createAsRenderTarget: true,
                properties);

        Assert.AreEqual(((uint) D3D9.UsageRendertarget, 1u), (requirements.Description.Usage, requirements.Levels));
    }

    [TestMethod]
    public void WhenBitmapColorSourceDoesNotRequireRenderTargetThenComputedUsageIsPreserved()
    {
        Direct3D9BitmapRealizationProperties properties = CreateMipMappedRealizationProperties(256, 128) with
        {
            TextureFormat = MilPixelFormat.Bgr32Bpp,
            LayoutU = new Direct3D9BitmapDimensionLayout(256, Direct3D9TexelLayout.Natural, Textureaddress.Clamp),
            LayoutV = new Direct3D9BitmapDimensionLayout(128, Direct3D9TexelLayout.Natural, Textureaddress.Clamp)
        };

        Direct3D9BitmapTextureRequirements requirements =
            Direct3D9BitmapRealizationParameterComputer.GetTextureCreationRequirements(
                canAutoGenerateMipmaps: true,
                createAsRenderTarget: false,
                properties);

        Assert.AreEqual(((uint) D3D9.UsageAutogenmipmap, 0u), (requirements.Description.Usage, requirements.Levels));
    }

    [TestMethod]
    public void WhenDeviceBitmapTextureExceedsMaximumWidthThenValidationFails()
    {
        Direct3D9BitmapTextureRequirements requirements = new(
            new SurfaceDesc(width: 1025, height: 512),
            Levels: 1);

        int result = Direct3D9BitmapRealizationParameterComputer.ValidateDeviceBitmapTextureRequirements(
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            requirements);

        Assert.AreEqual(Direct3D9Factory.MaximumTextureSizeExceededHResult, result);
    }

    [TestMethod]
    public void WhenDeviceBitmapTextureFitsMaximumDimensionsThenValidationSucceeds()
    {
        Direct3D9BitmapTextureRequirements requirements = new(
            new SurfaceDesc(width: 1024, height: 1024),
            Levels: 1);

        int result = Direct3D9BitmapRealizationParameterComputer.ValidateDeviceBitmapTextureRequirements(
            maximumTextureWidth: 1024,
            maximumTextureHeight: 1024,
            requirements);

        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
    }

    [TestMethod]
    public void WhenExistingDeviceBitmapTextureIsInitializedThenItIsRetainedBeforeContextSetup()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeDeviceBitmapColorSource(
            bitmap: 5,
            CreateRealizationProperties(),
            existingTexture: 7,
            returnSharedHandle: false,
            (nint bitmap, out uint width, out uint height) =>
            {
                calls.Add("size");
                width = 300;
                height = 200;
                return Direct3D9Factory.SuccessHResult;
            },
            texture => calls.Add($"add-ref:{texture}"),
            (bool isEvictable, bool returnHandle, ref nint handle, out nint texture) =>
            {
                calls.Add("create");
                texture = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (bitmap, properties) => calls.Add($"set-context:{bitmap}"),
            out Direct3D9DeviceBitmapInitialization initialization);

        Assert.AreEqual(
            (0, new Direct3D9DeviceBitmapInitialization(300, 200, 7, 0), "size,add-ref:7,set-context:5"),
            (result, initialization, string.Join(',', calls)));
    }

    [TestMethod]
    public void WhenSharedDeviceBitmapTextureIsInitializedThenItIsCreatedAsNonEvictableAndHandleIsCaptured()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeDeviceBitmapColorSource(
            bitmap: 5,
            CreateRealizationProperties(),
            existingTexture: 0,
            returnSharedHandle: true,
            (nint bitmap, out uint width, out uint height) =>
            {
                calls.Add("size");
                width = 64;
                height = 32;
                return Direct3D9Factory.SuccessHResult;
            },
            texture => calls.Add($"add-ref:{texture}"),
            (bool isEvictable, bool returnHandle, ref nint handle, out nint texture) =>
            {
                calls.Add($"create:{isEvictable}:{returnHandle}");
                handle = 11;
                texture = 9;
                return Direct3D9Factory.SuccessHResult;
            },
            (bitmap, properties) => calls.Add($"set-context:{bitmap}"),
            out Direct3D9DeviceBitmapInitialization initialization);

        Assert.AreEqual(
            (0, new Direct3D9DeviceBitmapInitialization(64, 32, 9, 11), "size,create:False:True,set-context:5"),
            (result, initialization, string.Join(',', calls)));
    }

    [TestMethod]
    public void WhenDeviceBitmapSizeExceedsSurfaceRectangleMaximumThenInitializationStops()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeDeviceBitmapColorSource(
            bitmap: 5,
            CreateRealizationProperties(),
            existingTexture: 7,
            returnSharedHandle: false,
            (nint bitmap, out uint width, out uint height) =>
            {
                calls.Add("size");
                width = 1u << 27;
                height = 1;
                return Direct3D9Factory.SuccessHResult;
            },
            texture => calls.Add("add-ref"),
            (bool isEvictable, bool returnHandle, ref nint handle, out nint texture) =>
            {
                calls.Add("create");
                texture = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (bitmap, properties) => calls.Add("set-context"),
            out Direct3D9DeviceBitmapInitialization initialization);

        Assert.AreEqual(
            (Direct3D9Factory.UnsupportedOperationHResult, default(Direct3D9DeviceBitmapInitialization), "size"),
            (result, initialization, string.Join(',', calls)));
    }

    [TestMethod]
    public void WhenDeviceBitmapTextureCreationFailsThenContextIsNotSet()
    {
        List<string> calls = [];

        int result = Direct3D9BitmapRealizationParameterComputer.InitializeDeviceBitmapColorSource(
            bitmap: 5,
            CreateRealizationProperties(),
            existingTexture: 0,
            returnSharedHandle: true,
            (nint bitmap, out uint width, out uint height) =>
            {
                calls.Add("size");
                width = 64;
                height = 32;
                return Direct3D9Factory.SuccessHResult;
            },
            texture => calls.Add("add-ref"),
            (bool isEvictable, bool returnHandle, ref nint handle, out nint texture) =>
            {
                calls.Add("create");
                texture = 0;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            (bitmap, properties) => calls.Add("set-context"),
            out Direct3D9DeviceBitmapInitialization initialization);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, default(Direct3D9DeviceBitmapInitialization), "size,create"),
            (result, initialization, string.Join(',', calls)));
    }

    [TestMethod]
    public void WhenBitmapSourceIsNullThenDerivationReturnsInvalidArgument()
    {
        DerivationHarness harness = new();

        int result = harness.CreateDeriver().Derive(0, CreateParameters(MilBitmapInterpolationMode.Linear), out Direct3D9BitmapPipelineColorSource? source);

        Assert.AreEqual((Direct3D9Factory.InvalidArgumentHResult, (nint) 0), (result, source?.BitmapColorSource ?? 0));
    }

    [TestMethod]
    public void WhenCacheRetrievalFailsThenStandardDerivationStillRuns()
    {
        DerivationHarness harness = new()
        {
            RetrieveResult = Direct3D9Factory.GenericFailureHResult,
            DerivedSource = 41
        };

        int result = harness.CreateDeriver().Derive(5, CreateParameters(MilBitmapInterpolationMode.Linear), out Direct3D9BitmapPipelineColorSource? source);
        using (source)
        {
            Assert.AreEqual((0, (nint) 41, "retrieve,derive,set-mask:0"), (result, source?.BitmapColorSource ?? 0, harness.Calls));
        }
    }

    [TestMethod]
    public void WhenDeviceBitmapHasNoCacheThenDependentSourceIsCreatedBeforeReuse()
    {
        DerivationHarness harness = new()
        {
            Bitmap = 11,
            IsDeviceBitmap = true,
            CreatedCache = 12,
            ReusedSource = 13
        };

        int result = harness.CreateDeriver().Derive(
            5,
            CreateParameters(MilBitmapInterpolationMode.Anisotropic, canAutoGenerateMipmaps: true),
            out Direct3D9BitmapPipelineColorSource? source);
        using (source)
        {
            Assert.AreEqual(
                (0, (nint) 13, "retrieve,is-device,get-cache,create-dependent,reuse,set-filter,set-reusable,transform,create-pipeline,set-mask:0,release:12"),
                (result, source?.BitmapColorSource ?? 0, harness.Calls));
        }
    }

    [TestMethod]
    public void WhenDeviceBitmapIsLookedUpThenPrefilterAndMipMappingAreDisabled()
    {
        DerivationHarness harness = new()
        {
            Bitmap = 11,
            BitmapCache = 12,
            IsDeviceBitmap = true,
            DerivedSource = 13
        };

        harness.CreateDeriver().Derive(
            5,
            CreateParameters(MilBitmapInterpolationMode.TriLinear, canAutoGenerateMipmaps: true),
            out _);

        Assert.AreEqual((false, MilBitmapInterpolationMode.Linear),
            (harness.LastParameters.PrefilterEnabled, harness.LastParameters.InterpolationMode));
    }

    [TestMethod]
    public void WhenCachedSourceIsReusedThenContextSettingsAreUpdatedBeforeTransfer()
    {
        DerivationHarness harness = new()
        {
            BitmapCache = 12,
            ReusedSource = 13,
            ReusableSources = 14
        };

        int result = harness.CreateDeriver().Derive(5, CreateParameters(MilBitmapInterpolationMode.Cubic), out Direct3D9BitmapPipelineColorSource? source);
        using (source)
        {
            Assert.AreEqual(
                (0, (nint) 13, "retrieve,reuse,set-filter,set-reusable,transform,create-pipeline,set-mask:0,release:12,release:14"),
                (result, source?.BitmapColorSource ?? 0, harness.Calls));
        }
    }

    [TestMethod]
    public void WhenReusedSourceTransformFailsThenTemporaryReferencesAreReleased()
    {
        DerivationHarness harness = new()
        {
            BitmapCache = 12,
            ReusedSource = 13,
            ReusableSources = 14,
            TransformResult = Direct3D9Factory.GenericFailureHResult
        };

        int result = harness.CreateDeriver().Derive(5, CreateParameters(MilBitmapInterpolationMode.Linear), out Direct3D9BitmapPipelineColorSource? source);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, (nint) 0, "retrieve,reuse,set-filter,set-reusable,transform,release:12,release:14,release:13"),
            (result, source?.BitmapColorSource ?? 0, harness.Calls));
    }

    [TestMethod]
    public void WhenReuseMissesThenStandardDerivationReceivesCacheState()
    {
        DerivationHarness harness = new()
        {
            Bitmap = 11,
            BitmapCache = 12,
            DerivedSource = 13
        };

        int result = harness.CreateDeriver().Derive(5, CreateParameters(MilBitmapInterpolationMode.Linear), out Direct3D9BitmapPipelineColorSource? source);
        using (source)
        {
            Assert.AreEqual(
                (0, (nint) 13, "retrieve,is-device,reuse,derive,set-mask:0,release:12"),
                (result, source?.BitmapColorSource ?? 0, harness.Calls));
        }
    }

    [TestMethod]
    public void WhenSourceClipIsUsedOutside3DThenMaskIsClearedWithoutContainmentCheck()
    {
        DerivationHarness harness = new()
        {
            DerivedSource = 13
        };

        int result = harness.CreateDeriver().Derive(
            5,
            CreateParameters(MilBitmapInterpolationMode.Linear),
            new Direct3D9BitmapSourceClipContext(true, false, 21),
            out Direct3D9BitmapPipelineColorSource? source);
        using (source)
        {
            Assert.AreEqual((0, (nint) 13, "retrieve,derive,set-mask:0"), (result, source?.BitmapColorSource ?? 0, harness.Calls));
        }
    }

    [TestMethod]
    public void When3DRealizationIsContainedBySourceClipThenMaskIsCleared()
    {
        DerivationHarness harness = new()
        {
            DerivedSource = 13,
            IsRealizationContainedBySourceClip = true
        };

        int result = harness.CreateDeriver().Derive(
            5,
            CreateParameters(MilBitmapInterpolationMode.Linear),
            new Direct3D9BitmapSourceClipContext(true, true, 21),
            out Direct3D9BitmapPipelineColorSource? source);
        using (source)
        {
            Assert.AreEqual((0, (nint) 13, "retrieve,derive,contains-clip,set-mask:0"), (result, source?.BitmapColorSource ?? 0, harness.Calls));
        }
    }

    [TestMethod]
    public void When3DRealizationExceedsSourceClipThenWorldSpaceMaskIsSet()
    {
        DerivationHarness harness = new()
        {
            BitmapCache = 12,
            ReusedSource = 13
        };

        int result = harness.CreateDeriver().Derive(
            5,
            CreateParameters(MilBitmapInterpolationMode.Linear),
            new Direct3D9BitmapSourceClipContext(true, true, 21),
            out Direct3D9BitmapPipelineColorSource? source);
        using (source)
        {
            Assert.AreEqual(
                (0, (nint) 13, "retrieve,reuse,set-filter,set-reusable,transform,create-pipeline,contains-clip,set-mask:21,release:12"),
                (result, source?.BitmapColorSource ?? 0, harness.Calls));
        }
    }

    [TestMethod]
    public void WhenMaskSetupFailsThenDerivedSourceAndCacheAreReleased()
    {
        DerivationHarness harness = new()
        {
            BitmapCache = 12,
            DerivedSource = 13,
            SetMaskResult = Direct3D9Factory.GenericFailureHResult
        };

        int result = harness.CreateDeriver().Derive(
            5,
            CreateParameters(MilBitmapInterpolationMode.Linear),
            new Direct3D9BitmapSourceClipContext(true, true, 21),
            out Direct3D9BitmapPipelineColorSource? source);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, (nint) 0, "retrieve,reuse,derive,contains-clip,set-mask:21,release:12,dispose-realizer,release:13"),
            (result, source?.BitmapColorSource ?? 0, harness.Calls));
    }

    [TestMethod]
    public void WhenExistingBitmapCacheIsProvidedThenItIsAddReferencedAndReleased()
    {
        RealizationHarness harness = new()
        {
            BitmapColorSource = 13
        };

        int result = harness.CreateRealizer().Derive(
            5,
            11,
            12,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            out Direct3D9BitmapPipelineColorSource? source);

        Assert.IsNotNull(source);
        source.Dispose();
        Assert.AreEqual(
            "add-ref:12,compute,get-source,set-context,create-pipeline,release:12,dispose-realizer,release:13",
            harness.Calls);
    }

    [TestMethod]
    public void WhenBitmapCacheRetrievalFailsThenRealizationContinuesWithoutCache()
    {
        RealizationHarness harness = new()
        {
            RetrieveResult = Direct3D9Factory.GenericFailureHResult,
            BitmapColorSource = 13
        };

        int result = harness.CreateRealizer().Derive(
            5,
            0,
            0,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            out Direct3D9BitmapPipelineColorSource? source);

        Assert.AreEqual(
            (0, true, "retrieve,compute,get-source,set-context,create-pipeline"),
            (result, source is not null, harness.Calls));
        source?.Dispose();
    }

    [TestMethod]
    public void WhenRealizationParameterComputationFailsThenCacheIsReleasedBeforeReturningFirstError()
    {
        RealizationHarness harness = new()
        {
            RetrievedCache = 12,
            ComputeResult = Direct3D9Factory.GenericFailureHResult
        };

        int result = harness.CreateRealizer().Derive(
            5,
            0,
            0,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            out Direct3D9BitmapPipelineColorSource? source);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, "retrieve,compute,release:12"),
            (result, source is null, harness.Calls));
    }

    [TestMethod]
    public void WhenBitmapColorSourceLookupFailsThenReturnedReferencesAreReleased()
    {
        RealizationHarness harness = new()
        {
            RetrievedCache = 12,
            BitmapColorSource = 13,
            ReusableRealizationSource = 14,
            GetSourceResult = Direct3D9Factory.GenericFailureHResult
        };

        int result = harness.CreateRealizer().Derive(
            5,
            0,
            0,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            out Direct3D9BitmapPipelineColorSource? source);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, "retrieve,compute,get-source,release:12,release:14,release:13"),
            (result, source is null, harness.Calls));
    }

    [TestMethod]
    public void WhenBitmapContextSetupFailsThenAllTemporaryReferencesAreReleased()
    {
        RealizationHarness harness = new()
        {
            RetrievedCache = 12,
            BitmapColorSource = 13,
            ReusableRealizationSource = 14,
            SetContextResult = Direct3D9Factory.GenericFailureHResult
        };

        int result = harness.CreateRealizer().Derive(
            5,
            0,
            0,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            out Direct3D9BitmapPipelineColorSource? source);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, "retrieve,compute,get-source,set-context,release:12,release:14,release:13"),
            (result, source is null, harness.Calls));
    }

    [TestMethod]
    public void WhenAtomicPipelineInitializationSucceedsThenLegacyDelegatesAreSkippedAndCacheIsReleased()
    {
        RealizationHarness harness = new()
        {
            RetrievedCache = 12,
            BitmapColorSource = 13
        };

        int result = harness.CreateAtomicRealizer().Derive(
            5,
            0,
            0,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            out Direct3D9BitmapPipelineColorSource? source);

        Assert.IsNotNull(source);
        source.Dispose();
        Assert.AreEqual(
            "retrieve,compute,choose-and-create,release:12,dispose-realizer,release:13",
            harness.Calls);
    }

    [TestMethod]
    public void WhenAtomicPipelineInitializationFailsThenReturnedOwnerIsDisposedBeforeCacheRelease()
    {
        RealizationHarness harness = new()
        {
            RetrievedCache = 12,
            BitmapColorSource = 13,
            AtomicResult = Direct3D9Factory.GenericFailureHResult,
            ReturnAtomicSourceOnFailure = true
        };

        int result = harness.CreateAtomicRealizer().Derive(
            5,
            0,
            0,
            CreateRealizationContext(),
            CreateParameters(MilBitmapInterpolationMode.Linear),
            out Direct3D9BitmapPipelineColorSource? source);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, "retrieve,compute,choose-and-create,dispose-realizer,release:13,release:12"),
            (result, source is null, harness.Calls));
    }

    [TestMethod]
    public void WhenDirtyTexturePopulationSucceedsThenBitsMipmapsAndCacheAreUpdatedInOrder()
    {
        TexturePopulationHarness harness = new();

        int result = harness.CreatePopulator().Populate(
            5,
            2,
            6,
            7,
            new Direct3D9BitmapRealizationRectangle(1, 2, 3, 4));

        Assert.AreEqual(
            (0, "prepare:5:2:6,push:5:2:6:12:True,mipmaps,commit:7:1:2:3:4,release:12,release:11"),
            (result, harness.Calls));
    }

    [TestMethod]
    public void WhenNoTextureRegionsAreDirtyThenOnlyCacheStateIsCommitted()
    {
        TexturePopulationHarness harness = new();

        int result = harness.CreatePopulator().Populate(
            5,
            0,
            0,
            7,
            new Direct3D9BitmapRealizationRectangle(1, 2, 3, 4));

        Assert.AreEqual((0, "commit:7:1:2:3:4"), (result, harness.Calls));
    }

    [TestMethod]
    public void WhenTexturePopulationPreparationFailsThenFirstErrorIsReturnedAndResourcesAreReleasedInOrder()
    {
        TexturePopulationHarness harness = new()
        {
            PrepareResult = Direct3D9Factory.GenericFailureHResult
        };

        int result = harness.CreatePopulator().Populate(5, 1, 6, 7, default);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "prepare:5:1:6,release:12,release:11"),
            (result, harness.Calls));
    }

    [TestMethod]
    public void WhenPushingTextureBitsFailsThenMipmapsAndCacheAreNotUpdated()
    {
        TexturePopulationHarness harness = new()
        {
            PushResult = Direct3D9Factory.GenericFailureHResult
        };

        int result = harness.CreatePopulator().Populate(5, 1, 6, 7, default);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "prepare:5:1:6,push:5:1:6:12:True,release:12,release:11"),
            (result, harness.Calls));
    }

    [TestMethod]
    public void WhenMipmapUpdateFailsThenCacheIsNotCommitted()
    {
        TexturePopulationHarness harness = new()
        {
            MipmapResult = Direct3D9Factory.GenericFailureHResult
        };

        int result = harness.CreatePopulator().Populate(5, 1, 6, 7, default);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "prepare:5:1:6,push:5:1:6:12:True,mipmaps,release:12,release:11"),
            (result, harness.Calls));
    }

    private static int ComputeTextureProperties(
        MilBitmapInterpolationMode interpolationMode,
        MilBitmapWrapMode wrapMode,
        bool canFallback,
        out Direct3D9BitmapRealizationProperties properties,
        int textureFormatResult = Direct3D9Factory.SuccessHResult) =>
        Direct3D9BitmapRealizationParameterComputer.ComputeTextureProperties(
            5,
            Direct3D9BitmapColorSourceContextParameters.FromExplicitSettings(
                interpolationMode,
                prefilterEnabled: true,
                MilPixelFormat.Pbgra32Bpp,
                wrapMode),
            canFallback,
            static (nint bitmapSource, out MilPixelFormat format) =>
            {
                format = MilPixelFormat.Bgra32Bpp;
                return Direct3D9Factory.SuccessHResult;
            },
            (MilPixelFormat sourceFormat, MilPixelFormat targetFormat, bool forceAlpha, out MilPixelFormat textureFormat) =>
            {
                textureFormat = textureFormatResult < 0
                    ? MilPixelFormat.Undefined
                    : MilPixelFormat.Pbgra32Bpp;
                return textureFormatResult;
            },
            out properties);

    private static Direct3D9BitmapRealizationContext CreateRealizationContext() =>
        new(21, 22, 23, 0.5f, true, 24);

    private static Direct3D9BitmapRealizationProperties CreateRealizationProperties() =>
        new(
            MilBitmapInterpolationMode.Linear,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            MilPixelFormat.Pbgra32Bpp,
            IsMinimumRealizationRectComputed: false,
            BitmapWidth: 0,
            BitmapHeight: 0,
            Width: 0,
            Height: 0);

    private static Direct3D9BitmapRealizationProperties CreateMipMappedRealizationProperties(
        uint bitmapWidth,
        uint bitmapHeight) =>
        CreateRealizationProperties() with
        {
            InterpolationMode = MilBitmapInterpolationMode.TriLinear,
            MipMapLevel = Direct3D9TextureMipMapLevel.All,
            BitmapWidth = bitmapWidth,
            BitmapHeight = bitmapHeight
        };

    private static Direct3D9BitmapColorSourceContextParameters CreateParameters(
        MilBitmapInterpolationMode interpolationMode,
        bool prefilterEnabled = true,
        bool isSoftwareDevice = false,
        bool canAutoGenerateMipmaps = false,
        bool canGenerateMipmapsWithStretchRect = false,
        bool isFantScalerDisabled = false,
        nint bitmapBrush = 3) =>
        Direct3D9BitmapColorSourceContextParameters.FromBrushAndContext(
            bitmapBrush,
            interpolationMode,
            prefilterEnabled,
            MilPixelFormat.Pbgra32Bpp,
            bitmapBrushUniqueness: 7,
            MilBitmapWrapMode.Tile,
            isSoftwareDevice,
            canAutoGenerateMipmaps,
            canGenerateMipmapsWithStretchRect,
            isFantScalerDisabled);

    private sealed class TexturePopulationHarness
    {
        private readonly List<string> _calls = [];

        internal int PrepareResult { get; init; }
        internal int PushResult { get; init; }
        internal int MipmapResult { get; init; }
        internal string Calls => string.Join(',', _calls);

        internal Direct3D9BitmapTexturePopulator CreatePopulator() => new(
            Prepare,
            Push,
            UpdateMipmapLevels,
            (uniqueness, bounds) => _calls.Add(
                $"commit:{uniqueness}:{bounds.Left}:{bounds.Top}:{bounds.Right}:{bounds.Bottom}"),
            resource => _calls.Add($"release:{resource}"));

        private int Prepare(
            nint bitmapSource,
            uint dirtyRectangleCount,
            nint dirtyRectangles,
            out nint bitmapLock,
            out bool copySourceToSystemMemorySurface,
            out nint systemMemorySurface)
        {
            _calls.Add($"prepare:{bitmapSource}:{dirtyRectangleCount}:{dirtyRectangles}");
            bitmapLock = 11;
            copySourceToSystemMemorySurface = true;
            systemMemorySurface = 12;
            return PrepareResult;
        }

        private int Push(
            nint bitmapSource,
            uint dirtyRectangleCount,
            nint dirtyRectangles,
            nint systemMemorySurface,
            bool copySourceToSystemMemorySurface)
        {
            _calls.Add(
                $"push:{bitmapSource}:{dirtyRectangleCount}:{dirtyRectangles}:{systemMemorySurface}:{copySourceToSystemMemorySurface}");
            return PushResult;
        }

        private int UpdateMipmapLevels()
        {
            _calls.Add("mipmaps");
            return MipmapResult;
        }
    }

    private sealed class RealizationHarness
    {
        private readonly List<string> _calls = [];

        internal int RetrieveResult { get; init; }
        internal nint RetrievedBitmap { get; init; }
        internal nint RetrievedCache { get; init; }
        internal int ComputeResult { get; init; }
        internal nint RealizationParameters { get; init; } = 31;
        internal int GetSourceResult { get; init; }
        internal nint BitmapColorSource { get; init; }
        internal nint ReusableRealizationSource { get; init; }
        internal int SetContextResult { get; init; }
        internal int AtomicResult { get; init; }
        internal bool ReturnAtomicSourceOnFailure { get; init; }
        internal string Calls => string.Join(',', _calls);

        internal Direct3D9BitmapColorSourceRealizer CreateRealizer() => new(
            Retrieve,
            cache => _calls.Add($"add-ref:{cache}"),
            Compute,
            GetSource,
            SetContext,
            CreatePipelineSource,
            resource => _calls.Add($"release:{resource}"));

        internal Direct3D9BitmapColorSourceRealizer CreateAtomicRealizer() => new(
            Retrieve,
            cache => _calls.Add($"add-ref:{cache}"),
            Compute,
            ChooseAndCreatePipelineSource,
            resource => _calls.Add($"release:{resource}"));

        private int Retrieve(nint bitmapSource, out nint bitmap, out nint bitmapCache)
        {
            _calls.Add("retrieve");
            bitmap = RetrievedBitmap;
            bitmapCache = RetrievedCache;
            return RetrieveResult;
        }

        private int Compute(
            nint bitmapSource,
            Direct3D9BitmapRealizationContext realizationContext,
            Direct3D9BitmapColorSourceContextParameters contextParameters,
            out nint realizationParameters)
        {
            _calls.Add("compute");
            realizationParameters = RealizationParameters;
            return ComputeResult;
        }

        private int GetSource(
            nint bitmapSource,
            nint bitmap,
            nint bitmapCache,
            nint realizationParameters,
            nint alternateCache,
            Direct3D9BitmapColorSourceContextParameters contextParameters,
            out nint bitmapColorSource,
            out nint reusableRealizationSource)
        {
            _calls.Add("get-source");
            bitmapColorSource = BitmapColorSource;
            reusableRealizationSource = ReusableRealizationSource;
            return GetSourceResult;
        }

        private int SetContext(
            nint bitmapColorSource,
            nint bitmapSource,
            nint realizationBounds,
            nint bitmapToXSpaceTransform,
            nint realizationParameters,
            nint reusableRealizationSource)
        {
            _calls.Add("set-context");
            return SetContextResult;
        }

        private int CreatePipelineSource(
            nint bitmapColorSource,
            out Direct3D9BitmapPipelineColorSource? pipelineColorSource)
        {
            _calls.Add("create-pipeline");
            pipelineColorSource = CreateOwnedPipelineSource(bitmapColorSource);
            return Direct3D9Factory.SuccessHResult;
        }

        private int ChooseAndCreatePipelineSource(
            nint bitmapSource,
            nint bitmap,
            nint bitmapCache,
            nint realizationParameters,
            nint alternateCache,
            Direct3D9BitmapColorSourceContextParameters contextParameters,
            out Direct3D9BitmapPipelineColorSource? pipelineColorSource)
        {
            _calls.Add("choose-and-create");
            pipelineColorSource = AtomicResult >= 0 || ReturnAtomicSourceOnFailure
                ? CreateOwnedPipelineSource(BitmapColorSource)
                : null;
            return AtomicResult;
        }

        private Direct3D9BitmapPipelineColorSource CreateOwnedPipelineSource(nint bitmapColorSource) => new(
            bitmapColorSource,
            new CallbackDisposable(() => _calls.Add("dispose-realizer")),
            new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0),
            resource => _calls.Add($"release:{resource}"));
    }

    private sealed class DerivationHarness
    {
        private readonly List<string> _calls = [];

        internal int RetrieveResult { get; init; }
        internal nint Bitmap { get; init; }
        internal nint BitmapCache { get; init; }
        internal bool IsDeviceBitmap { get; init; }
        internal int GetCacheResult { get; init; }
        internal nint CreatedCache { get; init; }
        internal nint ReusedSource { get; init; }
        internal nint ReusableSources { get; init; }
        internal int DeriveResult { get; init; }
        internal nint DerivedSource { get; init; }
        internal int TransformResult { get; init; }
        internal bool IsRealizationContainedBySourceClip { get; init; }
        internal int SetMaskResult { get; init; }
        internal Direct3D9BitmapColorSourceContextParameters LastParameters { get; private set; }
        internal string Calls => string.Join(',', _calls);

        internal Direct3D9BitmapColorSourceDeriver CreateDeriver() => new(
            Retrieve,
            IsDevice,
            GetCache,
            (bitmap, cache) => _calls.Add("create-dependent"),
            TryReuse,
            Derive,
            CreatePipelineSource,
            (source, mode) => _calls.Add("set-filter"),
            (source, reusable) => _calls.Add("set-reusable"),
            source =>
            {
                _calls.Add("transform");
                return TransformResult;
            },
            () =>
            {
                _calls.Add("contains-clip");
                return IsRealizationContainedBySourceClip;
            },
            (source, mask) =>
            {
                _calls.Add($"set-mask:{mask}");
                return SetMaskResult;
            },
            resource => _calls.Add($"release:{resource}"));

        private int Retrieve(nint bitmapSource, out nint bitmap, out nint bitmapCache)
        {
            _calls.Add("retrieve");
            bitmap = Bitmap;
            bitmapCache = BitmapCache;
            return RetrieveResult;
        }

        private bool IsDevice(nint bitmap)
        {
            _calls.Add("is-device");
            return IsDeviceBitmap;
        }

        private int GetCache(nint bitmap, out nint bitmapCache)
        {
            _calls.Add("get-cache");
            bitmapCache = CreatedCache;
            return GetCacheResult;
        }

        private void TryReuse(
            nint bitmapCache,
            Direct3D9BitmapColorSourceContextParameters parameters,
            out nint bitmapColorSource,
            out nint reusableColorSources)
        {
            _calls.Add("reuse");
            LastParameters = parameters;
            bitmapColorSource = ReusedSource;
            reusableColorSources = ReusableSources;
        }

        private int Derive(
            nint bitmapSource,
            nint bitmap,
            nint bitmapCache,
            Direct3D9BitmapColorSourceContextParameters parameters,
            out Direct3D9BitmapPipelineColorSource? texturedColorSource)
        {
            _calls.Add("derive");
            LastParameters = parameters;
            texturedColorSource = DerivedSource == 0 ? null : CreatePipelineSourceCore(DerivedSource);
            return DeriveResult;
        }

        private Direct3D9BitmapPipelineColorSource CreatePipelineSource(nint bitmapColorSource)
        {
            _calls.Add("create-pipeline");
            return CreatePipelineSourceCore(bitmapColorSource);
        }

        private Direct3D9BitmapPipelineColorSource CreatePipelineSourceCore(nint bitmapColorSource) => new(
            bitmapColorSource,
            new CallbackDisposable(() => _calls.Add("dispose-realizer")),
            new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0),
            resource => _calls.Add($"release:{resource}"));
    }

    private sealed class CallbackDisposable(Action dispose) : IDisposable
    {
        private bool _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            dispose();
            _isDisposed = true;
        }
    }
}
