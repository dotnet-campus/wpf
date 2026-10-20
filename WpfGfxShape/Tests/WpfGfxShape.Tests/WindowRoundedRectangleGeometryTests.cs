using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class WindowRoundedRectangleGeometryTests
{
    [TestMethod]
    [DataRow(8d, 6d)]
    [DataRow(-8d, -6d)]
    [DataRow(80d, 60d)]
    public void WhenRoundedCoverageHasNonzeroRadiiThenCornerIsExcludedAndInteriorIncluded(double x, double y)
    {
        var geometry = GeneratedRectangleGeometryResource.CreateStatic(new(4, 4, 32, 24), x, y);
        Assert.IsTrue(SoftwarePathCoverage.TryCreateRoundedRectangle(geometry, GeneratedImageTransform.Identity, out var path));
        var coverage = new SoftwareImageCoverage(path!);
        Assert.AreEqual((0, 64), (coverage.GetCoverage(4, 4), coverage.GetCoverage(20, 16)));
    }

    [TestMethod]
    public void WhenRoundedCoverageMovesThenOldInteriorBecomesEmpty()
    {
        var geometry = GeneratedRectangleGeometryResource.CreateStatic(new(4, 4, 32, 24), 8, 6);
        Assert.IsTrue(SoftwarePathCoverage.TryCreateRoundedRectangle(geometry, new(1, 1, 40, 0), out var path));
        var coverage = new SoftwareImageCoverage(path!);
        Assert.AreEqual((0, 64), (coverage.GetCoverage(20, 16), coverage.GetCoverage(60, 16)));
    }
}
