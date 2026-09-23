using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9VertexBufferBuilderTests
{
    [TestMethod]
    public void WhenOutsideBoundsContainNoShapeThenWholeBoundsAreEmittedAsTriangleStrip()
    {
        List<Direct3D9BuilderDrawBatch> batches = [];
        Direct3D9VertexBufferBuilder builder = CreateBuilder(batches);
        builder.SetOutsideBounds(new Direct3D9SurfaceRect(2, 3, 8, 9), needInsideGeometry: false);
        builder.BeginBuilding();

        int result = builder.FlushReset();

        Direct3D9ExpandedVertex[] vertices = batches.Single().Vertices.ToArray();
        Assert.AreEqual(
            (0, Direct3D9BuilderPrimitiveKind.TriangleStrip, 6, 2f, 3f, 8f, 9f, 0u),
            (result, batches[0].Kind, vertices.Length, vertices[0].X, vertices[0].Y, vertices[4].X, vertices[4].Y, vertices[0].Diffuse));
    }

    [TestMethod]
    public void WhenTrapezoidStrataHaveGapThenTopGapSidesMiddleGapAndBottomAreOrdered()
    {
        List<Direct3D9BuilderDrawBatch> batches = [];
        Direct3D9VertexBufferBuilder builder = CreateBuilder(batches);
        builder.SetOutsideBounds(new Direct3D9SurfaceRect(0, 0, 10, 10), needInsideGeometry: true);
        builder.BeginBuilding();

        int first = builder.PrepareStratum(2, 4, true, 3, 7);
        int second = builder.PrepareStratum(6, 8, true, -2, 12);
        int result = builder.FlushReset();

        Direct3D9ExpandedVertex[] vertices = batches.Single().Vertices.ToArray();
        Assert.AreEqual(
            (0, 0, 0, 30, 0f, 0f, -2f, 6f, 12f, 8f, 10f, 10f),
            (first, second, result, vertices.Length, vertices[0].X, vertices[0].Y, vertices[18].X, vertices[18].Y, vertices[22].X, vertices[22].Y, vertices[^1].X, vertices[^1].Y));
    }

    [TestMethod]
    public void WhenStrataAreAdjacentThenNoMiddleGapRectangleIsAdded()
    {
        List<Direct3D9BuilderDrawBatch> batches = [];
        Direct3D9VertexBufferBuilder builder = CreateBuilder(batches);
        builder.SetOutsideBounds(new Direct3D9SurfaceRect(0, 0, 10, 8), needInsideGeometry: true);
        builder.BeginBuilding();
        builder.PrepareStratum(0, 4, true, 2, 5);
        builder.PrepareStratum(4, 8, true, 3, 6);

        int result = builder.FlushReset();

        Assert.AreEqual((0, 18), (result, batches.Single().Vertices.Length));
    }

    [TestMethod]
    public void WhenInsideGeometryIsDisabledThenInsideListsAreFilteredButOutsideRemains()
    {
        List<Direct3D9BuilderDrawBatch> batches = [];
        Direct3D9VertexBufferBuilder builder = CreateBuilder(batches);
        builder.SetOutsideBounds(new Direct3D9SurfaceRect(0, 0, 4, 4), needInsideGeometry: false);
        builder.BeginBuilding();
        builder.AddTriangleListVertices(Vertex(1, 1), Vertex(2, 1), Vertex(1, 2));
        builder.AddLineListVertices(Vertex(0, 0), Vertex(1, 0));

        int result = builder.FlushReset();

        Assert.AreEqual((0, 1, Direct3D9BuilderPrimitiveKind.TriangleStrip), (result, batches.Count, batches[0].Kind));
    }

    [TestMethod]
    public void WhenPrimitiveKindsAreMixedThenExpansionPackingAndDrawOrderMatchNativeOrder()
    {
        List<Direct3D9BuilderDrawBatch> batches = [];
        List<(Direct3D9BuilderPrimitiveKind Kind, int GroupSize)> packed = [];
        Direct3D9VertexBufferBuilder builder = CreateBuilder(
            batches,
            packVertices: (kind, _, groupSize) =>
            {
                packed.Add((kind, groupSize));
                return 0;
            });
        builder.BeginBuilding();
        builder.AddIndexedTriangleVertices([Vertex(0, 0), Vertex(1, 0), Vertex(0, 1)], [0, 1, 2]);
        builder.AddTriangleListVertices(Vertex(0, 0), Vertex(1, 0), Vertex(0, 1));
        builder.AddTriangleStripVertices(Vertex(0, 0), Vertex(0, 0), Vertex(0, 1), Vertex(1, 0), Vertex(1, 1), Vertex(1, 1));
        builder.AddLineListVertices(Vertex(0, 0), Vertex(1, 0));

        int result = builder.FlushReset();

        Assert.AreEqual(
            (0,
             "IndexedTriangleList,TriangleList,TriangleStrip,LineList",
             "TriangleList:3,TriangleStrip:6,LineList:2"),
            (result,
             string.Join(',', batches.Select(batch => batch.Kind)),
             string.Join(',', packed.Select(item => $"{item.Kind}:{item.GroupSize}"))));
    }

    [TestMethod]
    public void WhenPrecomputedIndexedGeometryIsUsedThenPositionIsTransformedAndIndicesAreNarrowed()
    {
        List<Direct3D9BuilderDrawBatch> batches = [];
        Direct3D9ExpandedVertexConverter converter = CreateConverter();
        converter.SetPositionTransform(new System.Numerics.Matrix3x2(2, 0, 0, 3, 5, 7));
        Direct3D9VertexBufferBuilder builder = new(converter, batch => { batches.Add(batch); return 0; });
        builder.BeginBuilding();
        builder.SetPrecomputedIndexedTriangles([Vertex(1, 2), Vertex(2, 2), Vertex(1, 3)], [0, 1, 2]);

        int result = builder.FlushReset();

        Direct3D9BuilderDrawBatch batch = batches.Single();
        Assert.AreEqual(
            (0, Direct3D9BuilderPrimitiveKind.IndexedTriangleList, 7f, 13f, "0,1,2"),
            (result, batch.Kind, batch.Vertices.Span[0].X, batch.Vertices.Span[0].Y, string.Join(',', batch.Indices.ToArray())));
    }

    [TestMethod]
    public void WhenPipelineRealizationFailsThenNoDrawOccursAndFlushResetStillClearsState()
    {
        List<Direct3D9BuilderDrawBatch> batches = [];
        Direct3D9VertexBufferBuilder builder = CreateBuilder(batches, realizePipeline: () => Direct3D9Factory.InvalidCallHResult);
        builder.BeginBuilding();
        builder.AddTriangleListVertices(Vertex(0, 0), Vertex(1, 0), Vertex(0, 1));

        int result = builder.FlushReset();

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 0, true, true), (result, batches.Count, builder.IsEmpty, builder.HasFlushed));
    }

    [TestMethod]
    public void WhenDrawFailsThenFirstFailureIsReturnedAndFlushResetStillClearsState()
    {
        int draws = 0;
        Direct3D9VertexBufferBuilder builder = new(
            CreateConverter(),
            _ => ++draws == 2 ? Direct3D9Factory.InvalidCallHResult : 0);
        builder.BeginBuilding();
        builder.AddTriangleListVertices(Vertex(0, 0), Vertex(1, 0), Vertex(0, 1));
        builder.AddLineListVertices(Vertex(0, 0), Vertex(1, 0));

        int result = builder.FlushReset();

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 2, true), (result, draws, builder.IsEmpty));
    }

    [TestMethod]
    public void WhenTryingToReturnBufferAfterPriorResetFlushThenNoBufferIsReturned()
    {
        Direct3D9VertexBufferBuilder builder = CreateBuilder([]);
        builder.BeginBuilding();
        builder.AddTriangleListVertices(Vertex(0, 0), Vertex(1, 0), Vertex(0, 1));
        int resetResult = builder.FlushReset();

        int result = builder.FlushTryGetVertexBuffer(out Direct3D9VertexBufferBuilder? returned);

        Assert.AreEqual((0, 0, true), (resetResult, result, returned is null));
    }

    [TestMethod]
    public void WhenBufferIsReturnedBeforeAnyResetFlushThenBuilderCanBeReusedAfterBeginBuilding()
    {
        List<Direct3D9BuilderDrawBatch> batches = [];
        Direct3D9VertexBufferBuilder builder = CreateBuilder(batches);
        builder.BeginBuilding();
        builder.AddLineListVertices(Vertex(0, 0), Vertex(1, 0));
        int first = builder.FlushTryGetVertexBuffer(out Direct3D9VertexBufferBuilder? returned);
        builder.BeginBuilding();
        builder.AddTriangleListVertices(Vertex(0, 0), Vertex(1, 0), Vertex(0, 1));

        int second = builder.FlushReset();

        Assert.AreEqual((0, true, 0, 2, true), (first, ReferenceEquals(builder, returned), second, batches.Count, builder.IsEmpty));
    }

    [TestMethod]
    public void WhenEndBuildingIsRepeatedThenOutsideGeometryIsNotDuplicated()
    {
        Direct3D9VertexBufferBuilder builder = CreateBuilder([]);
        builder.SetOutsideBounds(new Direct3D9SurfaceRect(0, 0, 2, 2), needInsideGeometry: true);
        builder.BeginBuilding();

        int first = builder.EndBuilding();
        int second = builder.EndBuilding();
        int flush = builder.FlushTryGetVertexBuffer(out _);

        Assert.AreEqual((0, 0, 0), (first, second, flush));
    }

    private static Direct3D9VertexBufferBuilder CreateBuilder(
        List<Direct3D9BuilderDrawBatch> batches,
        Func<int>? realizePipeline = null,
        Direct3D9PackBuilderVertices? packVertices = null) =>
        new(CreateConverter(), batch => { batches.Add(batch); return 0; }, realizePipeline, packVertices);

    private static Direct3D9ExpandedVertexConverter CreateConverter()
    {
        Assert.AreEqual(0, Direct3D9ExpandedVertexConverter.Create(
            Direct3D9VertexFormatAttribute.Xy | Direct3D9VertexFormatAttribute.Diffuse,
            Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse,
            Direct3D9VertexFormatAttribute.Diffuse,
            out Direct3D9ExpandedVertexConverter? converter));
        Assert.AreEqual(0, converter!.SetZMapping(0.5f));
        Assert.AreEqual(0, converter.FinalizeMappings());
        return converter;
    }

    private static Direct3D9ExpandedVertex Vertex(float x, float y, float alpha = 1) => new()
    {
        X = x,
        Y = y,
        Diffuse = BitConverter.SingleToUInt32Bits(alpha),
    };
}
