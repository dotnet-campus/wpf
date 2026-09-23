using System.Numerics;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ProductionPipelineTests
{
    [TestMethod]
    public void WhenGeometryIsEmptyWithoutOutsideBoundsThenPipelineStaysLazy()
    {
        List<string> calls = [];
        Direct3D9ProductionPipelineInitializer initializer = CreateInitializer(calls);
        Assert.AreEqual(0, initializer.Initialize(1, _ => Direct3D9Factory.EmptyFillHResult, false, null, true, out Direct3D9Pipeline? pipeline));

        int result = pipeline!.Execute();

        Assert.AreEqual((0, "CreateConverter"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenEmptyGeometryHasOutsideBoundsThenOutsideGeometryRealizesAndDraws()
    {
        List<string> calls = [];
        Direct3D9ProductionPipelineInitializer initializer = CreateInitializer(calls);
        Assert.AreEqual(0, initializer.Initialize(
            1,
            _ => Direct3D9Factory.EmptyFillHResult,
            false,
            new Direct3D9SurfaceRect(0, 0, 4, 4),
            false,
            out Direct3D9Pipeline? pipeline));

        int result = pipeline!.Execute();

        Assert.AreEqual((0, "CreateConverter|Realize|State|Draw:TriangleStrip:6"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPipelineExecutesTwiceThenSecondPassUsesCachedVertexBuffer()
    {
        List<string> calls = [];
        Direct3D9ProductionPipelineInitializer initializer = CreateInitializer(calls);
        Assert.AreEqual(0, initializer.Initialize(
            1,
            builder =>
            {
                calls.Add("Geometry");
                builder.AddTriangleListVertices(Vertex(0, 0), Vertex(1, 0), Vertex(0, 1));
                return 0;
            },
            false,
            null,
            true,
            out Direct3D9Pipeline? pipeline));

        int first = pipeline!.Execute();
        int second = pipeline.Execute();

        Assert.AreEqual(
            (0, 0, "CreateConverter|Geometry|Realize|State|Draw:TriangleList:3|State|Draw:TriangleList:3"),
            (first, second, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenComplexScanIsSentThenItUsesTheProductionBuilder()
    {
        List<string> calls = [];
        Direct3D9ProductionPipelineInitializer initializer = CreateInitializer(calls);
        Assert.AreEqual(0, initializer.Initialize(
            1,
            builder => builder.AddComplexScan(
                2,
                [new Direct3D9CoverageInterval(0, 64), new Direct3D9CoverageInterval(3, 0), new Direct3D9CoverageInterval(int.MaxValue, 0)],
                0,
                []),
            false,
            null,
            true,
            out Direct3D9Pipeline? pipeline));

        int result = pipeline!.Execute();

        Assert.AreEqual((0, "CreateConverter|Realize|State|Draw:LineList:2"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenMappingFailsThenBuilderAndOwnedResourcesAreReleased()
    {
        List<string> calls = [];
        Direct3D9PipelineColorSource colorSource = new(
            Direct3D9ColorSourceType.Texture,
            () => 0,
            SendBuilderVertexMapping: (_, _) => Direct3D9Factory.InvalidCallHResult);
        Direct3D9ProductionPipelineInitializer initializer = CreateInitializer(
            calls,
            [colorSource],
            [new CallbackDisposable(() => calls.Add("Owned"))]);

        int result = initializer.Initialize(1, _ => 0, false, null, true, out Direct3D9Pipeline? pipeline);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, true, "CreateConverter|Owned|ReleaseColors"),
            (result, pipeline is null, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenReleasedThenCachedBufferAndColorSourcesCannotBeReused()
    {
        List<string> calls = [];
        Direct3D9ProductionPipelineInitializer initializer = CreateInitializer(calls);
        Assert.AreEqual(0, initializer.Initialize(
            1,
            builder =>
            {
                builder.AddTriangleListVertices(Vertex(0, 0), Vertex(1, 0), Vertex(0, 1));
                return 0;
            },
            false,
            null,
            true,
            out Direct3D9Pipeline? pipeline));
        Assert.AreEqual(0, pipeline!.Execute());

        pipeline.ReleaseExpensiveResources();

        Assert.ThrowsExactly<InvalidOperationException>(() => pipeline.Execute());
    }

    private static Direct3D9ProductionPipelineInitializer CreateInitializer(
        List<string> calls,
        IReadOnlyList<Direct3D9PipelineColorSource?>? colorSources = null,
        IReadOnlyList<IDisposable?>? ownedColorSources = null)
    {
        Direct3D9VertexPipelineOwner owner = new(
            (out Direct3D9ExpandedVertexConverter? converter) =>
            {
                calls.Add("CreateConverter");
                return Direct3D9ExpandedVertexConverter.Create(
                    Direct3D9VertexFormatAttribute.Xy | Direct3D9VertexFormatAttribute.Diffuse,
                    Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse,
                    Direct3D9VertexFormatAttribute.Diffuse,
                    out converter);
            },
            batch =>
            {
                calls.Add($"Draw:{batch.Kind}:{batch.Vertices.Length}");
                return 0;
            });
        return new Direct3D9ProductionPipelineInitializer(
            colorSources ??
            [
                new Direct3D9PipelineColorSource(
                    Direct3D9ColorSourceType.Texture,
                    () =>
                    {
                        calls.Add("Realize");
                        return 0;
                    })
            ],
            owner,
            () =>
            {
                calls.Add("State");
                return 0;
            },
            () => calls.Add("ReleaseColors"),
            ownedColorSources);
    }

    private static Direct3D9ExpandedVertex Vertex(float x, float y) => new()
    {
        X = x,
        Y = y,
        Diffuse = uint.MaxValue,
    };

    private sealed class CallbackDisposable(Action callback) : IDisposable
    {
        public void Dispose() => callback();
    }
}
