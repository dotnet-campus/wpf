using System.Numerics;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionTests
{
    [TestMethod]
    [DataRow(1u, 0)]
    [DataRow(2u, 0)]
    [DataRow(3u, 0)]
    [DataRow(1u, 1)]
    [DataRow(2u, 1)]
    [DataRow(3u, 1)]
    [DataRow(1u, 2)]
    [DataRow(2u, 2)]
    [DataRow(3u, 2)]
    [DataRow(1u, 0, 1)]
    [DataRow(2u, 1, 1)]
    [DataRow(3u, 2, 1)]
    [DataRow(1u, 0, 2)]
    [DataRow(2u, 1, 2)]
    [DataRow(3u, 2, 2)]
    [DataRow(1u, 0, 3)]
    [DataRow(2u, 1, 3)]
    [DataRow(3u, 2, 3)]
    [DataRow(1u, 1, 3)]
    [DataRow(1u, 2, 3)]
    [DataRow(2u, 0, 3)]
    [DataRow(2u, 2, 3)]
    [DataRow(3u, 0, 3)]
    [DataRow(3u, 1, 3)]
    public void WhenCroppedDrawingTileMovesByOnePeriodThenCompleteFloatSurfaceIsUnchanged(uint scalingMode, int consumerMode, int affineMode = 0)
    {
        Vector4[] first = [], second = [];
        WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
            curves: true, drawingImage: true, sourceMode: 26, tileMode: 4,
            sourceAlpha: 0.5f, floating: true, scalingMode: scalingMode, consumerMode: consumerMode,
            affineMode: affineMode, observeSurface: pixels => first = pixels);
        WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
            curves: true, drawingImage: true, sourceMode: 27, tileMode: 4,
            sourceAlpha: 0.5f, floating: true, scalingMode: scalingMode, consumerMode: consumerMode,
            affineMode: affineMode, observeSurface: pixels => second = pixels);
        Assert.IsNotEmpty(first);
        Assert.IsTrue(first.Any(pixel => pixel.W > 0));
        CollectionAssert.AreEqual(first, second);
    }

    [TestMethod]
    [DataRow(1u, 0)]
    [DataRow(2u, 0)]
    [DataRow(3u, 0)]
    [DataRow(1u, 1)]
    [DataRow(2u, 1)]
    [DataRow(3u, 1)]
    [DataRow(1u, 2)]
    [DataRow(2u, 2)]
    [DataRow(3u, 2)]
    [DataRow(1u, 0, 1)]
    [DataRow(2u, 1, 1)]
    [DataRow(3u, 2, 1)]
    [DataRow(1u, 0, 2)]
    [DataRow(2u, 1, 2)]
    [DataRow(3u, 2, 2)]
    [DataRow(1u, 0, 3)]
    [DataRow(2u, 1, 3)]
    [DataRow(3u, 2, 3)]
    [DataRow(1u, 1, 3)]
    [DataRow(1u, 2, 3)]
    [DataRow(2u, 0, 3)]
    [DataRow(2u, 2, 3)]
    [DataRow(3u, 0, 3)]
    [DataRow(3u, 1, 3)]
    public void WhenCroppedDrawingTileMovesByOnePeriodThenCompletePbgraSurfaceIsUnchanged(uint scalingMode, int consumerMode, int affineMode = 0)
    {
        byte[] first = [], second = [];
        WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
            curves: true, drawingImage: true, sourceMode: 26, tileMode: 4,
            sourceAlpha: 0.5f, floating: false, scalingMode: scalingMode, consumerMode: consumerMode,
            affineMode: affineMode, observeBgraSurface: pixels => first = pixels);
        WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
            curves: true, drawingImage: true, sourceMode: 27, tileMode: 4,
            sourceAlpha: 0.5f, floating: false, scalingMode: scalingMode, consumerMode: consumerMode,
            affineMode: affineMode, observeBgraSurface: pixels => second = pixels);
        Assert.IsNotEmpty(first);
        Assert.IsTrue(first.Where((_, index) => index % 4 == 3).Any(alpha => alpha > 0));
        CollectionAssert.AreEqual(first, second);
    }
}
