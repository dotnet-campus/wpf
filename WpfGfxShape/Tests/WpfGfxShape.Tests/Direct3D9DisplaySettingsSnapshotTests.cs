using System.Collections.Immutable;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DisplaySettingsSnapshotTests
{
    [TestMethod]
    public void WhenFontSmoothingIsDisabledThenDefaultRenderingModeIsBiLevel()
    {
        Direct3D9DisplaySettingsSnapshotResult result = Read(
            new Direct3D9FontSmoothingSettings(false, Direct3D9DisplaySettingsSnapshotFactory.FontSmoothingClearType),
            DefaultParameters(),
            DefaultParameters(),
            bitsPerPixel: 32);

        Assert.AreEqual(Direct3D9RenderingMode.BiLevel, result.DefaultSettings.Settings.DisplayRenderingMode);
    }

    [TestMethod]
    public void WhenFontSmoothingTypeIsUnknownThenRenderingModeIsGrayscale()
    {
        Direct3D9DisplaySettingsSnapshotResult result = Read(
            new Direct3D9FontSmoothingSettings(true, 99),
            DefaultParameters(),
            DefaultParameters(),
            bitsPerPixel: 32);

        Assert.AreEqual(Direct3D9RenderingMode.Grayscale, result.DefaultSettings.Settings.DisplayRenderingMode);
    }

    [TestMethod]
    public void WhenClearTypeIsEnabledThenRenderingModeIsClearType()
    {
        Direct3D9DisplaySettingsSnapshotResult result = Read(
            new Direct3D9FontSmoothingSettings(true, Direct3D9DisplaySettingsSnapshotFactory.FontSmoothingClearType),
            DefaultParameters(),
            DefaultParameters(),
            bitsPerPixel: 32);

        Assert.AreEqual(Direct3D9RenderingMode.ClearType, result.DefaultSettings.Settings.DisplayRenderingMode);
    }

    [TestMethod]
    public void WhenRenderingParametersAreCompiledThenGammaContrastAndRgbOffsetArePreserved()
    {
        Direct3D9DisplaySettingsSnapshotResult result = Read(
            new Direct3D9FontSmoothingSettings(true, Direct3D9DisplaySettingsSnapshotFactory.FontSmoothingClearType),
            DefaultParameters(),
            new Direct3D9RenderingParametersSnapshot(Direct3D9PixelGeometry.Rgb, 2.2f, 0.75f, 0.6f),
            bitsPerPixel: 32);

        Assert.AreEqual(
            (12u, 0.75f, 0.2f, true),
            (result.Displays[0].GlyphBlendingParameters.GammaIndex,
                result.Displays[0].GlyphBlendingParameters.ContrastEnhanceFactor,
                result.Displays[0].GlyphBlendingParameters.BlueSubpixelOffset,
                result.Displays[0].AllowGamma));
    }

    [TestMethod]
    public void WhenPixelGeometryIsBgrThenBlueSubpixelOffsetIsNegative()
    {
        Direct3D9DisplaySettingsSnapshotResult result = Read(
            new Direct3D9FontSmoothingSettings(true, Direct3D9DisplaySettingsSnapshotFactory.FontSmoothingClearType),
            DefaultParameters(),
            new Direct3D9RenderingParametersSnapshot(Direct3D9PixelGeometry.Bgr, 1.8f, 0.5f, 0.9f),
            bitsPerPixel: 32);

        Assert.AreEqual(-0.3f, result.Displays[0].GlyphBlendingParameters.BlueSubpixelOffset, 0.0001f);
    }

    [TestMethod]
    public void WhenDisplayHasLowColorDepthThenClearTypeAndGammaAreDisabled()
    {
        Direct3D9DisplaySettingsSnapshotResult result = Read(
            new Direct3D9FontSmoothingSettings(true, Direct3D9DisplaySettingsSnapshotFactory.FontSmoothingClearType),
            DefaultParameters(),
            new Direct3D9RenderingParametersSnapshot(Direct3D9PixelGeometry.Rgb, 1.8f, 0.5f, 1.0f),
            bitsPerPixel: 8);

        Assert.AreEqual(
            (Direct3D9PixelGeometry.Flat, false),
            (result.Displays[0].Settings.PixelStructure, result.Displays[0].AllowGamma));
    }

    [TestMethod]
    public void WhenDisplayIsMirrorDeviceThenClearTypeIsDisabledButGammaRemainsAllowed()
    {
        Direct3D9DisplaySettingsSnapshotResult result = Read(
            new Direct3D9FontSmoothingSettings(true, Direct3D9DisplaySettingsSnapshotFactory.FontSmoothingClearType),
            DefaultParameters(),
            new Direct3D9RenderingParametersSnapshot(Direct3D9PixelGeometry.Rgb, 1.8f, 0.5f, 1.0f),
            bitsPerPixel: 8,
            stateFlags: 0x00000008);

        Assert.AreEqual(
            (Direct3D9PixelGeometry.Flat, true),
            (result.Displays[0].Settings.PixelStructure, result.Displays[0].AllowGamma));
    }

    [TestMethod]
    public void WhenGammaIsBelowOneThenGammaIndexIsZero()
    {
        Direct3D9DisplaySettingsSnapshotResult result = Read(
            new Direct3D9FontSmoothingSettings(true, Direct3D9DisplaySettingsSnapshotFactory.FontSmoothingStandard),
            DefaultParameters(),
            new Direct3D9RenderingParametersSnapshot(Direct3D9PixelGeometry.Flat, 0.8f, 0.5f, 0.0f),
            bitsPerPixel: 32);

        Assert.AreEqual(0u, result.Displays[0].GlyphBlendingParameters.GammaIndex);
    }

    [TestMethod]
    public void WhenDisplayModeCountDoesNotMatchThenDisplayStateIsInvalid()
    {
        COMException exception = Assert.ThrowsExactly<COMException>(() => Direct3D9DisplaySettingsSnapshotFactory.Read(
            Adapters(0),
            [],
            default,
            DefaultParameters(),
            _ => DefaultParameters()));

        Assert.AreEqual(Direct3D9Factory.DisplayStateInvalidHResult, exception.HResult);
    }

    [TestMethod]
    public void WhenDisplayIndicesDoNotMatchThenDisplayStateIsInvalid()
    {
        COMException exception = Assert.ThrowsExactly<COMException>(() => Direct3D9DisplaySettingsSnapshotFactory.Read(
            Adapters(0),
            [new Direct3D9DisplayModeSnapshot(1, default, 0, 32)],
            default,
            DefaultParameters(),
            _ => DefaultParameters()));

        Assert.AreEqual(Direct3D9Factory.DisplayStateInvalidHResult, exception.HResult);
    }

    private static Direct3D9DisplaySettingsSnapshotResult Read(
        Direct3D9FontSmoothingSettings fontSmoothing,
        Direct3D9RenderingParametersSnapshot defaultParameters,
        Direct3D9RenderingParametersSnapshot monitorParameters,
        uint bitsPerPixel,
        uint stateFlags = 0)
    {
        return Direct3D9DisplaySettingsSnapshotFactory.Read(
            Adapters(stateFlags),
            [new Direct3D9DisplayModeSnapshot(0, default, 0, bitsPerPixel)],
            fontSmoothing,
            defaultParameters,
            monitorHandle =>
            {
                Assert.AreEqual((nint) 10, monitorHandle);
                return monitorParameters;
            });
    }

    private static Direct3D9AdapterArrangementResult Adapters(uint stateFlags)
    {
        return new Direct3D9AdapterArrangementResult(
            1,
            [new Direct3D9AdapterSnapshot(0, 42, 10, ImmutableArray<Direct3D9DisplayBounds>.Empty, "display", stateFlags)]);
    }

    private static Direct3D9RenderingParametersSnapshot DefaultParameters()
    {
        return new Direct3D9RenderingParametersSnapshot(Direct3D9PixelGeometry.Rgb, 1.8f, 0.5f, 1.0f);
    }
}
