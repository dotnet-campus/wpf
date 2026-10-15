namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionTests
{
    [TestMethod]
    [DataRow(17, 0)]
    [DataRow(17, 3)]
    [DataRow(18, 3)]
    public void WhenDrawingImageVisualMaskIsRenderedThenCompletePbgraSurfaceMatchesOriginalWpf(int sourceMode, int affineMode)
    {
        using var reference = System.Text.Json.JsonDocument.Parse(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "ReferenceData", "original-wpf.json")));
        var sample = reference.RootElement.GetProperty("Samples").EnumerateArray().Single(value =>
            value.GetProperty("SourceMode").GetInt32() == sourceMode
            && value.GetProperty("AffineMode").GetInt32() == affineMode
            && value.GetProperty("TileMode").GetString() == "Tile"
            && value.GetProperty("ScalingMode").GetString() == "LowQuality"
            && value.GetProperty("Consumer").GetString() == "mask");
        byte[] expected = sample.GetProperty("PixelsBgra").EnumerateArray()
            .Select(value => value.GetByte()).ToArray();
        byte[] actual = [];
        WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
            curves: true, drawingImage: true, affineMode: affineMode, sourceMode: sourceMode,
            tileMode: 4, sourceAlpha: 0.5f, floating: false, scalingMode: 1, consumerMode: 2,
            observeBgraSurface: pixels => actual = pixels);
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(false, 1u)]
    [DataRow(false, 2u)]
    [DataRow(false, 3u)]
    [DataRow(true, 1u)]
    [DataRow(true, 2u)]
    [DataRow(true, 3u)]
    public void WhenRealWicDecoderReadFailsThenPresentReturnsFirstErrorAndRetryRestoresPixels(bool tiled, uint scalingMode)
    {
        WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(0,
            geometryKind: 3, solidFill: true, deviceMode: 2, drawingBrushSource: true,
            scalingMode: scalingMode, filteredDrawing: !tiled, tiledDrawing: tiled, failDecoder: true);
    }

    [TestMethod]
    [DataRow(0, 1u, false)]
    [DataRow(3, 1u, false)]
    [DataRow(4, 1u, false)]
    [DataRow(8, 1u, false)]
    [DataRow(9, 1u, false)]
    [DataRow(0, 2u, false)]
    [DataRow(3, 2u, false)]
    [DataRow(4, 2u, false)]
    [DataRow(8, 2u, false)]
    [DataRow(9, 2u, false)]
    [DataRow(0, 3u, false)]
    [DataRow(3, 3u, false)]
    [DataRow(4, 3u, false)]
    [DataRow(8, 3u, false)]
    [DataRow(9, 3u, false)]
    [DataRow(0, 1u, true)]
    [DataRow(3, 1u, true)]
    [DataRow(8, 1u, true)]
    [DataRow(9, 1u, true)]
    [DataRow(0, 2u, true)]
    [DataRow(3, 2u, true)]
    [DataRow(8, 2u, true)]
    [DataRow(9, 2u, true)]
    [DataRow(0, 3u, true)]
    [DataRow(3, 3u, true)]
    [DataRow(8, 3u, true)]
    [DataRow(9, 3u, true)]
    public void WhenRotatedDrawingDecoderReentersThenIntermediateLifetimePreservesPixels(int action, uint scalingMode, bool tiled)
    {
        WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(action,
            geometryKind: 3, solidFill: true, deviceMode: 2, drawingBrushSource: true,
            scalingMode: scalingMode, filteredDrawing: !tiled, tiledDrawing: tiled);
    }

    [TestMethod]
    [DataRow(1u, false)]
    [DataRow(2u, false)]
    [DataRow(3u, false)]
    [DataRow(1u, true)]
    [DataRow(2u, true)]
    [DataRow(3u, true)]
    public void WhenRotatedDrawingHasNoCallbackThenFixedDevicePixelsAreCovered(uint scalingMode, bool tiled)
    {
        WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(0,
            geometryKind: 3, solidFill: true, deviceMode: 2, drawingBrushSource: true,
            scalingMode: scalingMode, filteredDrawing: !tiled, tiledDrawing: tiled, callbackBaseline: true);
    }

    [TestMethod]
    [DataRow(1u)]
    [DataRow(2u)]
    [DataRow(3u)]
    public void WhenRotatedDrawingFillLockConflictsThenReleaseAndRetryPreservePixels(uint scalingMode)
    {
        WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(0,
            geometryKind: 3, failFill: true, solidFill: true, deviceMode: 2,
            drawingBrushSource: true, scalingMode: scalingMode, filteredDrawing: true);
    }
}
