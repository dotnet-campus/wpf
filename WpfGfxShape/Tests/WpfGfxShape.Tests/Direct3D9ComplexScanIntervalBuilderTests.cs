using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ComplexScanIntervalBuilderTests
{
    [TestMethod]
    public void WhenScanContainsCoverageIntervalsThenPixelCentersAndCoverageAreEmitted()
    {
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines = [];
        Direct3D9ComplexScanIntervalBuilder builder = CreateBuilder(lines);

        int result = builder.AddScan(3,
        [
            new Direct3D9CoverageInterval(1, 16),
            new Direct3D9CoverageInterval(4, 32),
            new Direct3D9CoverageInterval(7, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, 2, new Direct3D9WafflePoint(1.5f, 3.5f, 0.25f), new Direct3D9WafflePoint(7.5f, 3.5f, 0.5f)),
            (result, lines.Count, lines[0].Start, lines[1].End));
    }

    [TestMethod]
    public void WhenInsideGeometryIsNotNeededThenFullCoverageIsFiltered()
    {
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines = [];
        Direct3D9ComplexScanIntervalBuilder builder = CreateBuilder(
            lines,
            needInsideGeometry: false,
            outsideBounds: new Direct3D9SurfaceRect(0, 0, 10, 10));

        int result = builder.AddScan(0,
        [
            new Direct3D9CoverageInterval(0, 64),
            new Direct3D9CoverageInterval(2, 32),
            new Direct3D9CoverageInterval(4, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, 2, 0.5f, 0f),
            (result, lines.Count, lines[0].Start.Alpha, lines[1].Start.Alpha));
    }

    [TestMethod]
    public void WhenOutsideGeometryIsNotNeededThenZeroCoverageIsFiltered()
    {
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines = [];
        Direct3D9ComplexScanIntervalBuilder builder = CreateBuilder(lines);

        int result = builder.AddScan(0,
        [
            new Direct3D9CoverageInterval(int.MinValue, 0),
            new Direct3D9CoverageInterval(2, 64),
            new Direct3D9CoverageInterval(4, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, 1, new Direct3D9WafflePoint(2.5f, 0.5f, 1f), new Direct3D9WafflePoint(4.5f, 0.5f, 1f)),
            (result, lines.Count, lines[0].Start, lines[0].End));
    }

    [TestMethod]
    public void WhenOutsideBoundsArePresentThenInfiniteAndDisjointIntervalsAreClippedToEmptyLines()
    {
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines = [];
        Direct3D9ComplexScanIntervalBuilder builder = CreateBuilder(
            lines,
            outsideBounds: new Direct3D9SurfaceRect(2, 0, 6, 10));

        int result = builder.AddScan(1,
        [
            new Direct3D9CoverageInterval(int.MinValue, 0),
            new Direct3D9CoverageInterval(0, 32),
            new Direct3D9CoverageInterval(1, 0),
            new Direct3D9CoverageInterval(4, 64),
            new Direct3D9CoverageInterval(8, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, 5, 0.5f, 0.5f, 4.5f, 6.5f, 8.5f),
            (result, lines.Count, lines[0].Start.X, lines[0].End.X, lines[3].Start.X, lines[3].End.X, lines[4].Start.X));
    }

    [TestMethod]
    public void WhenLineSinkFailsThenFirstHResultStopsTraversal()
    {
        int callCount = 0;
        Direct3D9ComplexScanIntervalBuilder builder = new(
            needInsideGeometry: true,
            outsideBounds: null,
            (_, _) => ++callCount == 2 ? Direct3D9Factory.DeviceLostHResult : 0);

        int result = builder.AddScan(0,
        [
            new Direct3D9CoverageInterval(0, 16),
            new Direct3D9CoverageInterval(1, 32),
            new Direct3D9CoverageInterval(2, 48),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((Direct3D9Factory.DeviceLostHResult, 2), (result, callCount));
    }

    [TestMethod]
    public void WhenSentinelIsMissingThenScanIsRejected()
    {
        Direct3D9ComplexScanIntervalBuilder builder = CreateBuilder([]);

        Assert.ThrowsExactly<ArgumentException>(() => builder.AddScan(0,
        [
            new Direct3D9CoverageInterval(0, 64),
            new Direct3D9CoverageInterval(1, 0),
        ]));
    }

    private static Direct3D9ComplexScanIntervalBuilder CreateBuilder(
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines,
        bool needInsideGeometry = true,
        Direct3D9SurfaceRect? outsideBounds = null)
    {
        return new Direct3D9ComplexScanIntervalBuilder(
            needInsideGeometry,
            outsideBounds,
            (start, end) =>
            {
                lines.Add((start, end));
                return 0;
            });
    }
}
