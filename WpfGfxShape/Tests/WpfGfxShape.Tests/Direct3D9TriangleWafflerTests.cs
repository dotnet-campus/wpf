using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9TriangleWafflerTests
{
    [TestMethod]
    public void WhenTriangleStaysInOneCellThenOriginalTriangleIsForwardedInScoreOrder()
    {
        List<(Direct3D9WafflePoint First, Direct3D9WafflePoint Second, Direct3D9WafflePoint Third)> triangles = [];
        Direct3D9TriangleWaffler waffler = CreateWaffler(triangles);
        Direct3D9WafflePoint first = new(0.8f, 0f, 0.8f);
        Direct3D9WafflePoint second = new(0.1f, 0f, 0.1f);
        Direct3D9WafflePoint third = new(0.4f, 0f, 0.4f);

        int result = waffler.AddTriangle(first, second, third);

        Assert.AreEqual((0, 1, second, third, first),
            (result, triangles.Count, triangles[0].First, triangles[0].Second, triangles[0].Third));
    }

    [TestMethod]
    public void WhenTwoVerticesShareFirstCellThenQuadAndTriangleAreGenerated()
    {
        List<(Direct3D9WafflePoint First, Direct3D9WafflePoint Second, Direct3D9WafflePoint Third)> triangles = [];
        Direct3D9TriangleWaffler waffler = CreateWaffler(triangles);

        int result = waffler.AddTriangle(
            new Direct3D9WafflePoint(0.1f, 0f, 0f),
            new Direct3D9WafflePoint(0.8f, 1f, 0.5f),
            new Direct3D9WafflePoint(1.2f, 2f, 1f));

        Assert.AreEqual((0, 3), (result, triangles.Count));
    }

    [TestMethod]
    public void WhenEachVertexUsesSuccessiveCellThenFiveTrianglesAreGenerated()
    {
        List<(Direct3D9WafflePoint First, Direct3D9WafflePoint Second, Direct3D9WafflePoint Third)> triangles = [];
        Direct3D9TriangleWaffler waffler = CreateWaffler(triangles);

        int result = waffler.AddTriangle(
            new Direct3D9WafflePoint(0.2f, 0f, 0f),
            new Direct3D9WafflePoint(1.2f, 2f, 0.5f),
            new Direct3D9WafflePoint(2.2f, 4f, 1f));

        Assert.AreEqual((0, 5), (result, triangles.Count));
    }

    [TestMethod]
    public void WhenTriangleSpansSeveralCellsThenEveryOutputTriangleFitsOnePartitionCell()
    {
        List<(Direct3D9WafflePoint First, Direct3D9WafflePoint Second, Direct3D9WafflePoint Third)> triangles = [];
        Direct3D9TriangleWaffler waffler = CreateWaffler(triangles);

        int result = waffler.AddTriangle(
            new Direct3D9WafflePoint(0.25f, 1f, 0f),
            new Direct3D9WafflePoint(1.5f, 3f, 0.5f),
            new Direct3D9WafflePoint(3.25f, 5f, 1f));

        Assert.AreEqual(0, result);
        Assert.IsTrue(triangles.All(triangle =>
        {
            float minimum = Math.Min(triangle.First.X, Math.Min(triangle.Second.X, triangle.Third.X));
            float maximum = Math.Max(triangle.First.X, Math.Max(triangle.Second.X, triangle.Third.X));
            return maximum - minimum <= 1.00001f;
        }));
    }

    [TestMethod]
    public void WhenEdgeIsSplitThenPositionAndAlphaAreInterpolated()
    {
        List<(Direct3D9WafflePoint First, Direct3D9WafflePoint Second, Direct3D9WafflePoint Third)> triangles = [];
        Direct3D9TriangleWaffler waffler = CreateWaffler(triangles);

        int result = waffler.AddTriangle(
            new Direct3D9WafflePoint(0f, 0f, 0f),
            new Direct3D9WafflePoint(0.5f, 1f, 0.25f),
            new Direct3D9WafflePoint(2f, 4f, 1f));
        Direct3D9WafflePoint split = triangles[0].Third;

        Assert.AreEqual((0, 1f, 2f, 0.5f), (result, split.X, split.Y, split.Alpha));
    }

    [TestMethod]
    public void WhenConsumerFailsThenFirstFailureStopsFanOutput()
    {
        int callCount = 0;
        Direct3D9TriangleWaffler waffler = new();
        waffler.Set(1f, 0f, 0f, (_, _, _) =>
        {
            callCount++;
            return callCount == 2 ? Direct3D9Factory.DeviceLostHResult : 0;
        });

        int result = waffler.AddTriangle(
            new Direct3D9WafflePoint(0.2f, 0f, 0f),
            new Direct3D9WafflePoint(1.2f, 1f, 0.5f),
            new Direct3D9WafflePoint(2.2f, 2f, 1f));

        Assert.AreEqual((Direct3D9Factory.DeviceLostHResult, 2), (result, callCount));
    }

    [TestMethod]
    public void WhenScoreIsNotANumberThenBadNumberIsReturnedWithoutOutput()
    {
        int callCount = 0;
        Direct3D9TriangleWaffler waffler = new();
        waffler.Set(1f, 0f, 0f, (_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int result = waffler.AddTriangle(
            new Direct3D9WafflePoint(float.NaN, 0f, 0f),
            new Direct3D9WafflePoint(0f, 0f, 0f),
            new Direct3D9WafflePoint(1f, 0f, 0f));

        Assert.AreEqual((Direct3D9Factory.BadNumberHResult, 0), (result, callCount));
    }

    [TestMethod]
    public void WhenHighestCellSaturatesThenBadNumberIsReturnedWithoutOutput()
    {
        int callCount = 0;
        Direct3D9TriangleWaffler waffler = new();
        waffler.Set(1f, 0f, 0f, (_, _, _) =>
        {
            callCount++;
            return 0;
        });

        int result = waffler.AddTriangle(
            new Direct3D9WafflePoint(float.MaxValue, 0f, 0f),
            new Direct3D9WafflePoint(float.MaxValue, 1f, 0f),
            new Direct3D9WafflePoint(float.MaxValue, 2f, 0f));

        Assert.AreEqual((Direct3D9Factory.BadNumberHResult, 0), (result, callCount));
    }

    [TestMethod]
    public void WhenConsumerIsReplacedThenFollowingTrianglesUseNewConsumer()
    {
        int firstCount = 0;
        int secondCount = 0;
        Direct3D9TriangleWaffler waffler = new();
        waffler.Set(1f, 0f, 0f, (_, _, _) =>
        {
            firstCount++;
            return 0;
        });
        waffler.SetConsumer((_, _, _) =>
        {
            secondCount++;
            return 0;
        });

        int result = waffler.AddTriangle(
            new Direct3D9WafflePoint(0.1f, 0f, 0f),
            new Direct3D9WafflePoint(0.2f, 0f, 0f),
            new Direct3D9WafflePoint(0.3f, 0f, 0f));

        Assert.AreEqual((0, 0, 1), (result, firstCount, secondCount));
    }

    private static Direct3D9TriangleWaffler CreateWaffler(
        List<(Direct3D9WafflePoint First, Direct3D9WafflePoint Second, Direct3D9WafflePoint Third)> triangles)
    {
        Direct3D9TriangleWaffler waffler = new();
        waffler.Set(1f, 0f, 0f, (first, second, third) =>
        {
            triangles.Add((first, second, third));
            return 0;
        });
        return waffler;
    }
}
