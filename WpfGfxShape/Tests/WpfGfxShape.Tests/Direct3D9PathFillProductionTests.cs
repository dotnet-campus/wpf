using System.Numerics;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    [DataRow((int) MilAntiAliasMode.None, "Aliased")]
    [DataRow((int) MilAntiAliasMode.EightByEight, "Antialiased")]
    public unsafe void WhenProductionPathFillRunsThenGeometrySelectionPipelineFallbackAndCleanupMatchNative(
        int antiAliasModeValue,
        string expectedGeometry)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        string geometryName = string.Empty;

        int result = renderTarget.ProductionFillPathWithBrush(
            1,
            Matrix4x4.Identity,
            new MilRectF(0, 0, 4, 4),
            2,
            Matrix4x4.Identity,
            3,
            (MilAntiAliasMode) antiAliasModeValue,
            new Direct3D9SurfaceRect(0, 0, 4, 4),
            static (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState state, nint _, Matrix4x4 _, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState _, out MilRectF bounds) => { bounds = new MilRectF(0, 0, 4, 4); return 0; },
            (nint _, Direct3D9PathBrushContext _, out Direct3D9PathHardwareBrush? hardwareBrush) =>
            {
                calls.Add("Brush");
                hardwareBrush = CreateBrush(calls, 3);
                return 0;
            },
            (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) =>
            {
                calls.Add("Antialiased");
                geometryName = "AntialiasedGeometry";
                geometry = CreateGeometry(geometryName, calls);
                return 0;
            },
            (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) =>
            {
                calls.Add("Aliased");
                geometryName = "AliasedGeometry";
                geometry = CreateGeometry(geometryName, calls);
                return 0;
            },
            (geometry, hardwareBrush, effects, context) => renderTarget.ProductionAcceleratedFillPath(
                MilCompositingMode.SourceOver,
                geometry,
                hardwareBrush,
                effects,
                context));

        Assert.AreEqual(
            (0, $"Brush|{expectedGeometry}|Shader|Fixed:3|CreateConverter|Geometry:{geometryName}|Realize|State|Draw:TriangleList:3|ReleaseColors|ReleaseBrush|ReleaseGeometry:{geometryName}"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionPathGeometrySendsComplexScanThenTheSharedVertexBuilderDrawsIt()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        using Direct3D9PathHardwareBrush brush = CreateBrush(calls, 0);
        using Direct3D9PathGeometryGenerator geometry = new(
            builder =>
            {
                calls.Add("ComplexScan");
                return builder.AddComplexScan(
                    2,
                    [new Direct3D9CoverageInterval(0, 64), new Direct3D9CoverageInterval(3, 0), new Direct3D9CoverageInterval(int.MaxValue, 0)],
                    0,
                    []);
            },
            () => calls.Add("GeometryReleased"));

        int result = renderTarget.ProductionAcceleratedFillPath(
            MilCompositingMode.SourceOver,
            geometry,
            brush,
            0,
            new Direct3D9PathBrushContext(Matrix4x4.Identity, default, default, true));

        Assert.AreEqual(
            (0, "Shader|Fixed:0|CreateConverter|ComplexScan|Realize|State|Draw:LineList:2|ReleaseColors"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionPathGeometryIsEmptyThenPipelineResourcesAndOwnersAreReleasedWithoutDrawing()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.ProductionFillPathWithBrush(
            1,
            null,
            default,
            2,
            Matrix4x4.Identity,
            0,
            MilAntiAliasMode.None,
            new Direct3D9SurfaceRect(0, 0, 4, 4),
            static (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState state, nint _, Matrix4x4 _, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState _, out MilRectF bounds) => { bounds = new MilRectF(0, 0, 4, 4); return 0; },
            (nint _, Direct3D9PathBrushContext _, out Direct3D9PathHardwareBrush? brush) =>
            {
                brush = new Direct3D9PathHardwareBrush(
                    static (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) => { description = null; return 0; },
                    static (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) => { description = null; return 0; },
                    () => calls.Add("ReleaseBrush"));
                return 0;
            },
            static (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) => { geometry = null; return Direct3D9Factory.GenericFailureHResult; },
            (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) =>
            {
                geometry = null;
                calls.Add("Empty");
                return Direct3D9Factory.EmptyFillHResult;
            },
            (_, _, _, _) =>
            {
                calls.Add("Draw");
                return 0;
            });

        Assert.AreEqual((0, "Empty|ReleaseBrush"), (result, string.Join('|', calls)));
    }

    private static Direct3D9PathGeometryGenerator CreateGeometry(string name, List<string> calls) => new(
        builder =>
        {
            calls.Add($"Geometry:{name}");
            builder.AddTriangleListVertices(
                new Direct3D9ExpandedVertex { X = 0, Y = 0, Diffuse = uint.MaxValue },
                new Direct3D9ExpandedVertex { X = 1, Y = 0, Diffuse = uint.MaxValue },
                new Direct3D9ExpandedVertex { X = 0, Y = 1, Diffuse = uint.MaxValue });
            return 0;
        },
        () => calls.Add($"ReleaseGeometry:{name}"));

    private static Direct3D9PathHardwareBrush CreateBrush(List<string> calls, nint expectedEffects) => new(
        (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
        {
            calls.Add("Shader");
            description = null;
            return Direct3D9Factory.NotImplementedHResult;
        },
        (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint effects, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
        {
            calls.Add($"Fixed:{effects}");
            Assert.AreEqual(expectedEffects, effects);
            description = new Direct3D9PathPipelineDescription(CreateInitializer(calls), null, true);
            return 0;
        },
        () => calls.Add("ReleaseBrush"));

    private static Direct3D9ProductionPipelineInitializer CreateInitializer(List<string> calls)
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
            [new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => { calls.Add("Realize"); return 0; })],
            owner,
            () => { calls.Add("State"); return 0; },
            () => calls.Add("ReleaseColors"));
    }
}
