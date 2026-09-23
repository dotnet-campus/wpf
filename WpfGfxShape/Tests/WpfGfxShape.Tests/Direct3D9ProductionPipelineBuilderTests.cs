using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ProductionPipelineBuilderTests
{
    [TestMethod]
    public void WhenFixedFunctionPipelineBuildsThenNativeOperationOrderMappingsAndDrawAreShared()
    {
        List<string> calls = [];
        Direct3D9ConstantColorSource constant = new(new MilColorF(0.5f, 1, 0, 0));
        Direct3D9ProductionPipelineBuildContext context = new(
            constant.PrimaryColorSource,
            ProcessFixedFunctionEffects: _ => { calls.Add("Effects"); return 0; },
            SendFixedFunctionGeometryModifiers: _ => { calls.Add("GeometryModifiers"); return 0; },
            SendFixedFunctionLighting: _ => { calls.Add("Lighting"); return 0; },
            ProcessFixedFunctionClip: _ => { calls.Add("Clip"); return 0; });
        Direct3D9VertexPipelineOwner owner = CreateOwner(calls);

        int buildResult = Direct3D9ProductionPipelineBuilder.BuildFixedFunction(
            context,
            owner,
            () => { calls.Add("State"); return 0; },
            () => calls.Add("ReleaseColors"),
            out Direct3D9ProductionPipelineInitializer? initializer);
        int initializeResult = initializer!.Initialize(
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
            out Direct3D9Pipeline? pipeline);
        int executeResult = pipeline!.Execute();

        Assert.AreEqual(
            (0, 0, 0, "Effects|GeometryModifiers|Lighting|Clip|CreateConverter|Geometry|State|Draw:TriangleList:3:80800000"),
            (buildResult, initializeResult, executeResult, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenShaderOperationFailsThenLaterOperationsAreSkippedAndOwnedResourcesReleaseInReverseOrder()
    {
        List<string> calls = [];
        Direct3D9ConstantColorSource first = new(new MilColorF(1, 1, 0, 0));
        Direct3D9ConstantAlphaScalableColorSource second = new(0.5f);
        Direct3D9PrimaryColorSource primary = new(sender =>
        {
            int result = sender.SetConstant(first);
            if (result >= 0)
            {
                result = second.PipelineColorSource.Realize();
            }

            return result;
        });
        Direct3D9ProductionPipelineBuildContext context = new(
            primary,
            ProcessShaderEffects: builder =>
            {
                Assert.AreEqual(0, builder.MultiplyConstantAlpha(second));
                return Direct3D9Factory.InvalidCallHResult;
            },
            SendShaderGeometryModifiers: _ => { calls.Add("GeometryModifiers"); return 0; });
        Direct3D9VertexPipelineOwner owner = CreateOwner(calls);

        int result = Direct3D9ProductionPipelineBuilder.BuildShader(
            context,
            owner,
            () => 0,
            () => calls.Add("ReleaseColors"),
            out Direct3D9ProductionPipelineInitializer? initializer);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, true, string.Empty),
            (result, initializer is null, string.Join('|', calls)));
    }

    private static Direct3D9VertexPipelineOwner CreateOwner(List<string> calls) => new(
        (out Direct3D9ExpandedVertexConverter? converter) =>
        {
            calls.Add("CreateConverter");
            return Direct3D9ExpandedVertexConverter.Create(
                Direct3D9VertexFormatAttribute.Xy,
                Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse,
                Direct3D9VertexFormatAttribute.None,
                out converter);
        },
        batch =>
        {
            calls.Add($"Draw:{batch.Kind}:{batch.Vertices.Length}:{batch.Vertices.Span[0].Diffuse:X8}");
            return 0;
        });

    private static Direct3D9ExpandedVertex Vertex(float x, float y) => new()
    {
        X = x,
        Y = y,
    };
}
