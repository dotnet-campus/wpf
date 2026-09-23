using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenProductionBitmapDrawRunsThenScratchBrushShapeAndPipelineUseNativeOrder()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);

        int result = renderTarget.ProductionDrawBitmap(
            new Direct3D9BitmapDrawState(worldToDevice, new Direct3D9PointAndSizeRect(1, 2, 3, 4)),
            bitmapSource.Pointer,
            19,
            CreateProductionBitmapOperations(calls, bitmapSource.Pointer, worldToDevice));

        Assert.AreEqual(
            (0, "Scratch|Shape:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }|Ensure|Set:23:True:True|AddRef:23|AddRef:19|Clip:29:True|Ensure|Guidelines:29:True|BrushClip:29:23:True|DeviceBounds:29|HardwareBrush:23|Aliased:29:True|Shader:19|Fixed:19|CreateConverter|Geometry:29|RealizeColor|State|Draw:TriangleList:3|ReleaseColors|ReleaseHardwareBrush:23|ReleaseGeometry:29|Release:19|Release:23|Clear:23"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionBitmapHardwareIsUnsupportedThenSoftwareFallbackRunsAndTemporaryStateIsReleased()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        Matrix4x4 worldToDevice = Matrix4x4.Identity;
        Direct3D9ProductionBitmapDrawOperations operations = CreateProductionBitmapOperations(
            calls,
            bitmapSource.Pointer,
            worldToDevice,
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
                    () => calls.Add($"ReleaseHardwareBrush:{brush}"));
                return 0;
            });

        int result = renderTarget.ProductionDrawBitmap(
            new Direct3D9BitmapDrawState(worldToDevice, new Direct3D9PointAndSizeRect(1, 2, 3, 4)),
            bitmapSource.Pointer,
            19,
            operations);

        Assert.AreEqual(
            (0, "Scratch|Shape:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }|Ensure|Set:23:True:True|AddRef:23|AddRef:19|Clip:29:True|Ensure|Guidelines:29:True|BrushClip:29:23:True|DeviceBounds:29|HardwareBrush:23|Aliased:29:True|ReleaseHardwareBrush:23|ReleaseGeometry:29|Software:29:True:-2147467263|Release:19|Release:23|Clear:23"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionBitmapShapeCreationFailsThenBrushAndPipelineAreNotCreated()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        Direct3D9ProductionBitmapDrawOperations operations = CreateProductionBitmapOperations(
            calls,
            bitmapSource.Pointer,
            Matrix4x4.Identity,
            createBitmapShape: (MilRectF bounds, out nint shape) =>
            {
                calls.Add($"Shape:{bounds}");
                shape = 0;
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = renderTarget.ProductionDrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(1, 2, 3, 4)),
            bitmapSource.Pointer,
            19,
            operations);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Scratch|Shape:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }"),
            (result, string.Join('|', calls)));
    }

    private static Direct3D9ProductionBitmapDrawOperations CreateProductionBitmapOperations(
        List<string> calls,
        nint expectedBitmapSource,
        Matrix4x4 expectedWorldToDevice,
        Direct3D9CreatePathHardwareBrush? createHardwareBrush = null,
        Direct3D9CreateBitmapShape? createBitmapShape = null) => new(
        (out nint scratchBrush) =>
        {
            calls.Add("Scratch");
            scratchBrush = 23;
            return 0;
        },
        (brush, bitmapSource, transform) => calls.Add($"Set:{brush}:{bitmapSource == expectedBitmapSource}:{transform == expectedWorldToDevice}"),
        brush => calls.Add($"Clear:{brush}"),
        _ => new Direct3D9ImmediateBrushRealizer(
            11,
            value => calls.Add($"AddRef:{value}"),
            value => calls.Add($"Release:{value}"),
            static (_, _) => { },
            static _ => false,
            static _ => Direct3D9BrushType.Bitmap,
            static _ => false,
            static _ => false,
            static _ => 0,
            static _ => 0,
            static (_, _, _, _) => 0,
            static (_, _, _, _) => 0,
            static (_, _) => { },
            static (_, _) => { }),
        createBitmapShape ?? ((MilRectF bounds, out nint shape) =>
        {
            calls.Add($"Shape:{bounds}");
            shape = 29;
            return 0;
        }),
        () =>
        {
            calls.Add("Ensure");
            return 0;
        },
        (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
        {
            calls.Add($"Clip:{shape}:{transform == expectedWorldToDevice}");
            clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
            return 0;
        },
        (shape, transform, _, reason) =>
        {
            calls.Add($"Software:{shape}:{transform == expectedWorldToDevice}:{reason}");
            return 0;
        },
        (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) =>
        {
            calls.Add($"Guidelines:{state.Shape}:{state.ShapeToDevice == expectedWorldToDevice}");
            updated = state;
            return 0;
        },
        (Direct3D9PathClipperState state, nint brush, Matrix4x4 _, out Direct3D9PathClipperState updated) =>
        {
            calls.Add($"BrushClip:{state.Shape}:{brush}:{state.ShapeToDevice == expectedWorldToDevice}");
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
                    description = new Direct3D9PathPipelineDescription(CreateProductionBitmapInitializer(calls), null, true);
                    return 0;
                },
                () => calls.Add($"ReleaseHardwareBrush:{brush}"));
            return 0;
        }),
        static (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) =>
        {
            geometry = null;
            return Direct3D9Factory.GenericFailureHResult;
        },
        (Direct3D9PathClipperState state, out Direct3D9PathGeometryGenerator? geometry) =>
        {
            calls.Add($"Aliased:{state.Shape}:{state.ShapeToDevice == expectedWorldToDevice}");
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

    private static Direct3D9ProductionPipelineInitializer CreateProductionBitmapInitializer(List<string> calls)
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