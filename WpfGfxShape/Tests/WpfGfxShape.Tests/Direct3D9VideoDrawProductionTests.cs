using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenProductionVideoUsesProvidedBitmapThenPrefilterAndBitmapPipelineMatchNativeOrder()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        List<string> calls = [];
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);

        int result = renderTarget.ProductionDrawVideo(
            renderState,
            null,
            bitmapSource.Pointer,
            new Direct3D9ProductionVideoDrawOperations(
                new Direct3D9BitmapDrawState(worldToDevice, new Direct3D9PointAndSizeRect(1, 2, 3, 4)),
                19,
                CreateProductionVideoBitmapOperations(calls, bitmapSource.Pointer, worldToDevice, renderState)));

        Assert.AreEqual(
            (0, true, "Scratch:False|Shape|Ensure|Set:True:True|AddRef:23|AddRef:19|Clip|Ensure|Guidelines|BrushClip:23|DeviceBounds|HardwareBrush:23|Aliased|Shader:19|Fixed:19|CreateConverter|Geometry|RealizeColor|State|Draw:TriangleList:3|ReleaseColors|ReleaseHardwareBrush|ReleaseGeometry|Release:19|Release:23|Clear"),
            (result, renderState.PrefilterEnabled, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionVideoSurfaceBeginSucceedsThenEndRenderRunsAfterBitmapPipelineFailure()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        List<string> calls = [];
        Direct3D9ProductionBitmapDrawOperations bitmapOperations = CreateProductionVideoBitmapOperations(
            calls,
            bitmapSource.Pointer,
            Matrix4x4.Identity,
            renderState,
            createBitmapShape: (MilRectF _, out nint shape) =>
            {
                calls.Add("ShapeFail");
                shape = 0;
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = renderTarget.ProductionDrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device _, out nint source) =>
                {
                    calls.Add("Begin");
                    source = bitmapSource.Pointer;
                    return 0;
                },
                () =>
                {
                    calls.Add("End");
                    return Direct3D9Factory.InvalidCallHResult;
                }),
            0,
            new Direct3D9ProductionVideoDrawOperations(
                new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(0, 0, 1, 1)),
                0,
                bitmapOperations));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, "Begin|Scratch:False|ShapeFail|End"),
            (result, renderState.PrefilterEnabled, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionVideoSurfaceBeginFailsThenEndAndBitmapPipelineAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        List<string> calls = [];

        int result = renderTarget.ProductionDrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device _, out nint source) =>
                {
                    calls.Add("Begin");
                    source = 0;
                    return Direct3D9Factory.GenericFailureHResult;
                },
                () =>
                {
                    calls.Add("End");
                    return 0;
                }),
            0,
            new Direct3D9ProductionVideoDrawOperations(
                new Direct3D9BitmapDrawState(Matrix4x4.Identity),
                0,
                CreateProductionVideoBitmapOperations(calls, 0, Matrix4x4.Identity, renderState)));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, "Begin"),
            (result, renderState.PrefilterEnabled, string.Join('|', calls)));
    }

    private static Direct3D9ProductionBitmapDrawOperations CreateProductionVideoBitmapOperations(
        List<string> calls,
        nint expectedBitmapSource,
        Matrix4x4 expectedTransform,
        Direct3D9VideoRenderState renderState,
        Direct3D9CreateBitmapShape? createBitmapShape = null) => new(
        (out nint scratchBrush) =>
        {
            calls.Add($"Scratch:{renderState.PrefilterEnabled}");
            scratchBrush = 23;
            return 0;
        },
        (_, source, transform) => calls.Add($"Set:{source == expectedBitmapSource}:{transform == expectedTransform}"),
        _ => calls.Add("Clear"),
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
        createBitmapShape ?? ((MilRectF _, out nint shape) =>
        {
            calls.Add("Shape");
            shape = 29;
            return 0;
        }),
        () =>
        {
            calls.Add("Ensure");
            return 0;
        },
        (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clipped) =>
        {
            calls.Add("Clip");
            clipped = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
            return 0;
        },
        static (_, _, _, _) => Direct3D9Factory.GenericFailureHResult,
        (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) =>
        {
            calls.Add("Guidelines");
            updated = state;
            return 0;
        },
        (Direct3D9PathClipperState state, nint brush, Matrix4x4 _, out Direct3D9PathClipperState updated) =>
        {
            calls.Add($"BrushClip:{brush}");
            updated = state;
            return 0;
        },
        (Direct3D9PathClipperState state, out MilRectF bounds) =>
        {
            calls.Add("DeviceBounds");
            bounds = state.Bounds;
            return 0;
        },
        (nint brush, Direct3D9PathBrushContext _, out Direct3D9PathHardwareBrush? hardwareBrush) =>
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
                    description = new Direct3D9PathPipelineDescription(CreateProductionVideoInitializer(calls), null, true);
                    return 0;
                },
                () => calls.Add("ReleaseHardwareBrush"));
            return 0;
        },
        static (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) =>
        {
            geometry = null;
            return Direct3D9Factory.GenericFailureHResult;
        },
        (Direct3D9PathClipperState _, out Direct3D9PathGeometryGenerator? geometry) =>
        {
            calls.Add("Aliased");
            geometry = new Direct3D9PathGeometryGenerator(
                builder =>
                {
                    calls.Add("Geometry");
                    builder.AddTriangleListVertices(
                        new Direct3D9ExpandedVertex { X = 0, Y = 0, Diffuse = uint.MaxValue },
                        new Direct3D9ExpandedVertex { X = 1, Y = 0, Diffuse = uint.MaxValue },
                        new Direct3D9ExpandedVertex { X = 0, Y = 1, Diffuse = uint.MaxValue });
                    return 0;
                },
                () => calls.Add("ReleaseGeometry"));
            return 0;
        },
        MilCompositingMode.SourceOver,
        MilAntiAliasMode.None,
        new Direct3D9SurfaceRect(0, 0, 8, 8));

    private static Direct3D9ProductionPipelineInitializer CreateProductionVideoInitializer(List<string> calls)
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