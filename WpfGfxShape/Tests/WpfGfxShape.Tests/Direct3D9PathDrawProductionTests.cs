using System.Numerics;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenProductionPathHasFillAndStrokeThenBothUseTheStronglyTypedPipelineInNativeOrder()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(2, 3, 0);
        Direct3D9ProductionPathDrawOperations operations = CreateProductionPathOperations(calls);

        int result = renderTarget.ProductionDrawPath(worldToDevice, 11, 13, 17, 19, operations);

        Assert.AreEqual(
            (0, "Realize:19|Bounds:11|Clip:11:True|Brush:19|Ensure|Guidelines:11:True|BrushClip:11:119:True|DeviceBounds:11|HardwareBrush:119|Aliased:11:True|Shader:219|Fixed:219|CreateConverter|Geometry:11|RealizeColor|State|Draw:TriangleList:3|ReleaseColors|ReleaseBrush:119|ReleaseGeometry:11|Realize:17|Widen:11:13:True|Bounds:23|Clip:23:False|Brush:17|Ensure|Guidelines:23:False|BrushClip:23:117:False|DeviceBounds:23|HardwareBrush:117|Aliased:23:False|Shader:217|Fixed:217|CreateConverter|Geometry:23|RealizeColor|State|Draw:TriangleList:3|ReleaseColors|ReleaseBrush:117|ReleaseGeometry:23"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionFillFailsThenStrokeIsNotPrepared()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        Direct3D9ProductionPathDrawOperations operations = CreateProductionPathOperations(
            calls,
            createHardwareBrush: (nint brush, Direct3D9PathBrushContext _, out Direct3D9PathHardwareBrush? hardwareBrush) =>
            {
                calls.Add($"HardwareBrush:{brush}");
                hardwareBrush = null;
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = renderTarget.ProductionDrawPath(Matrix4x4.Identity, 11, 13, 17, 19, operations);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Realize:19|Bounds:11|Clip:11:True|Brush:19|Ensure|Guidelines:11:True|BrushClip:11:119:True|DeviceBounds:11|HardwareBrush:119"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionHardwarePathIsUnsupportedThenSoftwareFallbackRunsBeforeStroke()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        Direct3D9ProductionPathDrawOperations operations = CreateProductionPathOperations(
            calls,
            createHardwareBrush: (nint brush, Direct3D9PathBrushContext _, out Direct3D9PathHardwareBrush? hardwareBrush) =>
            {
                calls.Add($"HardwareBrush:{brush}");
                hardwareBrush = new Direct3D9PathHardwareBrush(
                    static (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
                    {
                        description = null;
                        return Direct3D9Factory.NotImplementedHResult;
                    },
                    static (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
                    {
                        description = null;
                        return Direct3D9Factory.NotImplementedHResult;
                    },
                    () => calls.Add($"ReleaseBrush:{brush}"));
                return 0;
            });

        int result = renderTarget.ProductionDrawPath(Matrix4x4.Identity, 11, 0, 0, 19, operations);

        Assert.AreEqual(
            (0, "Realize:19|Bounds:11|Clip:11:True|Brush:19|Ensure|Guidelines:11:True|BrushClip:11:119:True|DeviceBounds:11|HardwareBrush:119|Aliased:11:True|ReleaseBrush:119|ReleaseGeometry:11|Software:11:True:19"),
            (result, string.Join('|', calls)));
    }

    private static Direct3D9ProductionPathDrawOperations CreateProductionPathOperations(
        List<string> calls,
        Direct3D9CreatePathHardwareBrush? createHardwareBrush = null) => new(
        brushRealizer =>
        {
            calls.Add($"Realize:{brushRealizer}");
            return 0;
        },
        (nint shape, out MilRectF bounds) =>
        {
            calls.Add($"Bounds:{shape}");
            bounds = shape == 11 ? new MilRectF(0, 0, 4, 4) : new MilRectF(1, 1, 5, 5);
            return 0;
        },
        (nint shape, nint pen, Matrix4x4? shapeToDevice, Direct3D9SurfaceRect _, out nint widenedShape) =>
        {
            calls.Add($"Widen:{shape}:{pen}:{shapeToDevice.HasValue}");
            widenedShape = 23;
            return 0;
        },
        (nint brushRealizer, bool _, out nint brush, out nint effects) =>
        {
            calls.Add($"Brush:{brushRealizer}");
            brush = brushRealizer + 100;
            effects = brushRealizer + 200;
            return 0;
        },
        (nint shape, Matrix4x4? shapeToDevice, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
        {
            calls.Add($"Clip:{shape}:{shapeToDevice.HasValue}");
            clippedShape = new Direct3D9SafeClippedShape(shape, shapeToDevice, bounds, false);
            return 0;
        },
        () =>
        {
            calls.Add("Ensure");
            return 0;
        },
        (shape, shapeToDevice, brushRealizer, _) =>
        {
            calls.Add($"Software:{shape}:{shapeToDevice.HasValue}:{brushRealizer}");
            return 0;
        },
        (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) =>
        {
            calls.Add($"Guidelines:{state.Shape}:{state.ShapeToDevice.HasValue}");
            updated = state;
            return 0;
        },
        (Direct3D9PathClipperState state, nint brush, Matrix4x4 _, out Direct3D9PathClipperState updated) =>
        {
            calls.Add($"BrushClip:{state.Shape}:{brush}:{state.ShapeToDevice.HasValue}");
            updated = state;
            return 0;
        },
        (Direct3D9PathClipperState state, out MilRectF bounds) =>
        {
            calls.Add($"DeviceBounds:{state.Shape}");
            bounds = state.Bounds;
            return 0;
        },
        createHardwareBrush ?? ((nint brush, Direct3D9PathBrushContext _, out Direct3D9PathHardwareBrush? hardwareBrush) =>
        {
            calls.Add($"HardwareBrush:{brush}");
            hardwareBrush = new Direct3D9PathHardwareBrush(
                (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint effects, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
                {
                    calls.Add($"Shader:{effects}");
                    description = null;
                    return Direct3D9Factory.NotImplementedHResult;
                },
                (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint effects, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
                {
                    calls.Add($"Fixed:{effects}");
                    description = new Direct3D9PathPipelineDescription(CreateProductionPathInitializer(calls), null, true);
                    return 0;
                },
                () => calls.Add($"ReleaseBrush:{brush}"));
            return 0;
        }),
        static (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) =>
        {
            geometry = null;
            return Direct3D9Factory.GenericFailureHResult;
        },
        (Direct3D9PathClipperState state, out Direct3D9PathGeometryGenerator? geometry) =>
        {
            calls.Add($"Aliased:{state.Shape}:{state.ShapeToDevice.HasValue}");
            geometry = new Direct3D9PathGeometryGenerator(
                builder =>
                {
                    calls.Add($"Geometry:{state.Shape}");
                    builder.AddTriangleListVertices(
                        new Direct3D9ExpandedVertex { X = 0, Y = 0, Diffuse = uint.MaxValue },
                        new Direct3D9ExpandedVertex { X = 1, Y = 0, Diffuse = uint.MaxValue },
                        new Direct3D9ExpandedVertex { X = 0, Y = 1, Diffuse = uint.MaxValue });
                    return 0;
                },
                () => calls.Add($"ReleaseGeometry:{state.Shape}"));
            return 0;
        },
        MilCompositingMode.SourceOver,
        MilAntiAliasMode.None,
        new Direct3D9SurfaceRect(0, 0, 8, 8));

    private static Direct3D9ProductionPipelineInitializer CreateProductionPathInitializer(List<string> calls)
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
            [new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => { calls.Add("RealizeColor"); return 0; })],
            owner,
            () => { calls.Add("State"); return 0; },
            () => calls.Add("ReleaseColors"));
    }
}