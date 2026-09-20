using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9FontSmoothingSettings(bool IsEnabled, uint SmoothingType);

internal readonly record struct Direct3D9RenderingParametersSnapshot(
    Direct3D9PixelGeometry PixelGeometry,
    float Gamma,
    float EnhancedContrast,
    float ClearTypeLevel);

internal readonly record struct Direct3D9GlyphBlendingParameters(
    float ContrastEnhanceFactor,
    float BlueSubpixelOffset,
    uint GammaIndex);

internal readonly record struct Direct3D9CompiledDisplaySettings(
    Direct3D9DisplaySettings Settings,
    bool AllowGamma,
    Direct3D9GlyphBlendingParameters GlyphBlendingParameters);

internal readonly record struct Direct3D9DisplaySettingsSnapshotResult(
    Direct3D9CompiledDisplaySettings DefaultSettings,
    ImmutableArray<Direct3D9CompiledDisplaySettings> Displays);

internal delegate Direct3D9RenderingParametersSnapshot Direct3D9MonitorRenderingParametersProvider(nint monitorHandle);

internal static class Direct3D9DisplaySettingsSnapshotFactory
{
    internal const uint FontSmoothingStandard = 1;
    internal const uint FontSmoothingClearType = 2;

    private const uint MirroringDriver = 0x00000008;
    private const uint MaxGammaIndex = 12;

    internal static Direct3D9DisplaySettingsSnapshotResult Read(
        Direct3D9AdapterArrangementResult adapters,
        ImmutableArray<Direct3D9DisplayModeSnapshot> displayModes,
        Direct3D9FontSmoothingSettings fontSmoothing,
        Direct3D9RenderingParametersSnapshot defaultRenderingParameters,
        Direct3D9MonitorRenderingParametersProvider monitorRenderingParametersProvider)
    {
        ArgumentNullException.ThrowIfNull(monitorRenderingParametersProvider);

        if (adapters.Displays.Length != displayModes.Length)
        {
            ThrowDisplayStateInvalid();
        }

        Direct3D9RenderingMode renderingMode = GetRenderingMode(fontSmoothing);
        Direct3D9CompiledDisplaySettings defaultSettings = Compile(
            defaultRenderingParameters,
            defaultRenderingParameters.PixelGeometry,
            renderingMode,
            allowGamma: true);

        ImmutableArray<Direct3D9CompiledDisplaySettings>.Builder displays =
            ImmutableArray.CreateBuilder<Direct3D9CompiledDisplaySettings>(adapters.Displays.Length);
        for (int index = 0; index < adapters.Displays.Length; index++)
        {
            Direct3D9AdapterSnapshot display = adapters.Displays[index];
            Direct3D9DisplayModeSnapshot mode = displayModes[index];
            if (display.DisplayIndex != mode.DisplayIndex)
            {
                ThrowDisplayStateInvalid();
            }

            Direct3D9RenderingParametersSnapshot renderingParameters =
                monitorRenderingParametersProvider(display.MonitorHandle);
            Direct3D9PixelGeometry pixelGeometry = renderingParameters.PixelGeometry;
            bool allowGamma = true;
            if ((display.StateFlags & MirroringDriver) != 0)
            {
                pixelGeometry = Direct3D9PixelGeometry.Flat;
            }
            else if (mode.BitsPerPixel < 16)
            {
                pixelGeometry = Direct3D9PixelGeometry.Flat;
                allowGamma = false;
            }

            displays.Add(Compile(renderingParameters, pixelGeometry, renderingMode, allowGamma));
        }

        return new Direct3D9DisplaySettingsSnapshotResult(defaultSettings, displays.MoveToImmutable());
    }

    private static Direct3D9RenderingMode GetRenderingMode(Direct3D9FontSmoothingSettings fontSmoothing)
    {
        if (!fontSmoothing.IsEnabled)
        {
            return Direct3D9RenderingMode.BiLevel;
        }

        return fontSmoothing.SmoothingType == FontSmoothingClearType
            ? Direct3D9RenderingMode.ClearType
            : Direct3D9RenderingMode.Grayscale;
    }

    private static Direct3D9CompiledDisplaySettings Compile(
        Direct3D9RenderingParametersSnapshot renderingParameters,
        Direct3D9PixelGeometry pixelGeometry,
        Direct3D9RenderingMode renderingMode,
        bool allowGamma)
    {
        uint gammaIndex = renderingParameters.Gamma < 1.0f
            ? 0
            : (uint) ((renderingParameters.Gamma - 1.0f) * 10.0f);
        gammaIndex = Math.Min(gammaIndex, MaxGammaIndex);

        float blueSubpixelOffset = renderingParameters.ClearTypeLevel / 3.0f;
        if (pixelGeometry == Direct3D9PixelGeometry.Bgr)
        {
            blueSubpixelOffset = -blueSubpixelOffset;
        }

        Direct3D9DisplaySettings settings = new(
            pixelGeometry,
            renderingParameters.Gamma,
            renderingParameters.EnhancedContrast,
            renderingParameters.ClearTypeLevel,
            renderingMode);
        Direct3D9GlyphBlendingParameters glyphBlendingParameters = new(
            renderingParameters.EnhancedContrast,
            blueSubpixelOffset,
            gammaIndex);
        return new Direct3D9CompiledDisplaySettings(settings, allowGamma, glyphBlendingParameters);
    }

    private static void ThrowDisplayStateInvalid()
    {
        Marshal.ThrowExceptionForHR(Direct3D9Factory.DisplayStateInvalidHResult);
    }
}
