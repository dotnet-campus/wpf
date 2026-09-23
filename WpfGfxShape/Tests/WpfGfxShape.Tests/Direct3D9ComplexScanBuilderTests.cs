using System.Numerics;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ComplexScanBuilderTests
{
    [TestMethod]
    public void WhenDirectScanSucceedsThenStratumIsPreparedAndSegmentsUseOneBatchAllocation()
    {
        List<(float Top, float Bottom)> strata = [];
        List<Direct3D9ComplexScanVertex[]> lineAllocations = [];
        Direct3D9ComplexScanBuilder builder = CreateBuilder(strata, lineAllocations);

        int result = builder.AddComplexScan(3,
        [
            new Direct3D9CoverageInterval(1, 16),
            new Direct3D9CoverageInterval(4, 32),
            new Direct3D9CoverageInterval(7, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, (3f, 4f), 1, 4, new Direct3D9ComplexScanVertex(1.5f, 3.5f, 0.25f), new Direct3D9ComplexScanVertex(7.5f, 3.5f, 0.5f)),
            (result, strata.Single(), lineAllocations.Count, lineAllocations[0].Length, lineAllocations[0][0], lineAllocations[0][3]));
    }

    [TestMethod]
    public void WhenScanIsEmptyThenNoVertexAllocationIsRequested()
    {
        int allocationCount = 0;
        Direct3D9ComplexScanBuilder builder = CreateBuilder(
            allocateLineListVertices: (int count, out Memory<Direct3D9ComplexScanVertex> vertices) =>
            {
                allocationCount++;
                vertices = new Direct3D9ComplexScanVertex[count];
                return 0;
            });

        int result = builder.AddComplexScan(0, [new Direct3D9CoverageInterval(int.MaxValue, 0)]);

        Assert.AreEqual((0, 0), (result, allocationCount));
    }

    [TestMethod]
    public void WhenInsideAndOutsideCoverageAreFilteredThenAllocationMatchesRemainingSegments()
    {
        List<Direct3D9ComplexScanVertex[]> lineAllocations = [];
        Direct3D9ComplexScanBuilder builder = CreateBuilder(
            lineAllocations: lineAllocations,
            viewportTop: -1f,
            needInsideGeometry: false,
            outsideBounds: new Direct3D9SurfaceRect(0, 0, 10, 10));

        int result = builder.AddComplexScan(0,
        [
            new Direct3D9CoverageInterval(0, 64),
            new Direct3D9CoverageInterval(2, 32),
            new Direct3D9CoverageInterval(4, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, 1, 4, 0.5f, 0f),
            (result, lineAllocations.Count, lineAllocations[0].Length, lineAllocations[0][0].Alpha, lineAllocations[0][2].Alpha));
    }

    [TestMethod]
    public void WhenOutsideBoundsArePresentThenNativeClipOrderPreservesEmptyLines()
    {
        List<Direct3D9ComplexScanVertex[]> lineAllocations = [];
        Direct3D9ComplexScanBuilder builder = CreateBuilder(
            lineAllocations: lineAllocations,
            outsideBounds: new Direct3D9SurfaceRect(2, 0, 6, 10));

        int result = builder.AddComplexScan(1,
        [
            new Direct3D9CoverageInterval(int.MinValue, 0),
            new Direct3D9CoverageInterval(0, 32),
            new Direct3D9CoverageInterval(1, 0),
            new Direct3D9CoverageInterval(4, 64),
            new Direct3D9CoverageInterval(8, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, 10, 0.5f, 0.5f, 4.5f, 6.5f, 8.5f),
            (result, lineAllocations[0].Length, lineAllocations[0][0].X, lineAllocations[0][1].X, lineAllocations[0][6].X, lineAllocations[0][7].X, lineAllocations[0][8].X));
    }

    [TestMethod]
    public void WhenScanIsInTopViewportRowThenSixVertexTriangleStripIsGenerated()
    {
        List<Direct3D9ComplexScanVertex[]> stripAllocations = [];
        Direct3D9ComplexScanBuilder builder = CreateBuilder(
            viewportTop: 2f,
            stripAllocations: stripAllocations);

        int result = builder.AddComplexScan(2,
        [
            new Direct3D9CoverageInterval(1, 32),
            new Direct3D9CoverageInterval(4, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Direct3D9ComplexScanVertex[] vertices = stripAllocations.Single();
        Assert.AreEqual((0, 6,
            new Direct3D9ComplexScanVertex(1f, 2f, 0.5f),
            new Direct3D9ComplexScanVertex(1f, 2f, 0.5f),
            new Direct3D9ComplexScanVertex(1f, 3f, 0.5f),
            new Direct3D9ComplexScanVertex(4f, 2f, 0.5f),
            new Direct3D9ComplexScanVertex(4f, 3f, 0.5f),
            new Direct3D9ComplexScanVertex(4f, 3f, 0.5f)),
            (result, vertices.Length, vertices[0], vertices[1], vertices[2], vertices[3], vertices[4], vertices[5]));
    }

    [TestMethod]
    public void WhenScanIsExactlyAtTopBoundaryThenDirectLineListIsUsed()
    {
        List<Direct3D9ComplexScanVertex[]> lineAllocations = [];
        List<Direct3D9ComplexScanVertex[]> stripAllocations = [];
        Direct3D9ComplexScanBuilder builder = CreateBuilder(
            viewportTop: 1.5f,
            lineAllocations: lineAllocations,
            stripAllocations: stripAllocations);

        int result = builder.AddComplexScan(2,
        [
            new Direct3D9CoverageInterval(0, 64),
            new Direct3D9CoverageInterval(1, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, 1, 0), (result, lineAllocations.Count, stripAllocations.Count));
    }

    [TestMethod]
    public void WhenWafflingIsUsedThenSplitSegmentsAreWrittenAsTriangleStrips()
    {
        List<Direct3D9ComplexScanVertex[]> stripAllocations = [];
        Direct3D9ComplexScanBuilder builder = CreateBuilder(
            textureCoordinates: [new Direct3D9WaffleTextureCoordinate(new Matrix3x2(1f, 0f, 0f, 4f, 0f, 0f), Direct3D9WaffleMode.Enabled)],
            stripAllocations: stripAllocations);

        int result = builder.AddComplexScan(1,
        [
            new Direct3D9CoverageInterval(0, 32),
            new Direct3D9CoverageInterval(3, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((0, 4), (result, stripAllocations.Count));
    }

    [TestMethod]
    public void WhenBatchAllocationFailsThenItsHResultIsReturnedWithoutOutput()
    {
        int stripAllocationCount = 0;
        Direct3D9ComplexScanBuilder builder = CreateBuilder(
            allocateLineListVertices: (int count, out Memory<Direct3D9ComplexScanVertex> vertices) =>
            {
                vertices = default;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            allocateTriangleStripVertices: (int count, out Memory<Direct3D9ComplexScanVertex> vertices) =>
            {
                stripAllocationCount++;
                vertices = new Direct3D9ComplexScanVertex[count];
                return 0;
            });

        int result = builder.AddComplexScan(1,
        [
            new Direct3D9CoverageInterval(0, 64),
            new Direct3D9CoverageInterval(2, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, 0), (result, stripAllocationCount));
    }

    [TestMethod]
    public void WhenTriangleStripAllocationFailsThenFirstHResultStopsIntervals()
    {
        int allocationCount = 0;
        Direct3D9ComplexScanBuilder builder = CreateBuilder(
            viewportTop: 0f,
            allocateTriangleStripVertices: (int count, out Memory<Direct3D9ComplexScanVertex> vertices) =>
            {
                allocationCount++;
                vertices = default;
                return Direct3D9Factory.DeviceLostHResult;
            });

        int result = builder.AddComplexScan(0,
        [
            new Direct3D9CoverageInterval(0, 16),
            new Direct3D9CoverageInterval(1, 32),
            new Direct3D9CoverageInterval(2, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((Direct3D9Factory.DeviceLostHResult, 1), (result, allocationCount));
    }

    [TestMethod]
    public void WhenPrepareStratumFailsThenNoAllocationIsRequested()
    {
        int allocationCount = 0;
        Direct3D9ComplexScanBuilder builder = CreateBuilder(
            prepareStratum: (_, _) => Direct3D9Factory.DeviceLostHResult,
            allocateLineListVertices: (int count, out Memory<Direct3D9ComplexScanVertex> vertices) =>
            {
                allocationCount++;
                vertices = new Direct3D9ComplexScanVertex[count];
                return 0;
            });

        int result = builder.AddComplexScan(0,
        [
            new Direct3D9CoverageInterval(0, 64),
            new Direct3D9CoverageInterval(1, 0),
            new Direct3D9CoverageInterval(int.MaxValue, 0),
        ]);

        Assert.AreEqual((Direct3D9Factory.DeviceLostHResult, 0), (result, allocationCount));
    }

    private static Direct3D9ComplexScanBuilder CreateBuilder(
        List<(float Top, float Bottom)>? strata = null,
        List<Direct3D9ComplexScanVertex[]>? lineAllocations = null,
        List<Direct3D9ComplexScanVertex[]>? stripAllocations = null,
        float viewportTop = 0f,
        bool needInsideGeometry = true,
        Direct3D9SurfaceRect? outsideBounds = null,
        IReadOnlyList<Direct3D9WaffleTextureCoordinate>? textureCoordinates = null,
        Direct3D9PrepareComplexScanStratum? prepareStratum = null,
        Direct3D9AllocateComplexScanVertices? allocateLineListVertices = null,
        Direct3D9AllocateComplexScanVertices? allocateTriangleStripVertices = null)
    {
        strata ??= [];
        lineAllocations ??= [];
        stripAllocations ??= [];
        return new Direct3D9ComplexScanBuilder(
            viewportTop,
            needInsideGeometry,
            outsideBounds,
            textureCoordinates ?? [],
            prepareStratum ?? ((top, bottom) =>
            {
                strata.Add((top, bottom));
                return 0;
            }),
            allocateLineListVertices ?? ((int count, out Memory<Direct3D9ComplexScanVertex> vertices) =>
            {
                Direct3D9ComplexScanVertex[] allocation = new Direct3D9ComplexScanVertex[count];
                lineAllocations.Add(allocation);
                vertices = allocation;
                return 0;
            }),
            allocateTriangleStripVertices ?? ((int count, out Memory<Direct3D9ComplexScanVertex> vertices) =>
            {
                Direct3D9ComplexScanVertex[] allocation = new Direct3D9ComplexScanVertex[count];
                stripAllocations.Add(allocation);
                vertices = allocation;
                return 0;
            }));
    }
}
