using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal sealed unsafe class GeneratedHardwareBitmapOperations : IDisposable
{
    private GCHandle _handle;
    private readonly Direct3D9Device _device;
    private readonly Direct3D9PipelineColorSource _source;
    private readonly GeneratedImageTransform _transform;
    private readonly MilCompositingMode _compositing;
    private readonly IReadOnlyList<MilPoint2D>? _triangles;
    private readonly uint _diffuse;
    private readonly bool _solid;

    internal GeneratedHardwareBitmapOperations(Direct3D9Device device, Direct3D9SurfaceRenderTarget target,
        Direct3D9PipelineColorSource source, GeneratedImageTransform transform,
        Direct3D9SurfaceRect clip, MilCompositingMode compositing,
        IReadOnlyList<MilPoint2D>? triangles = null, uint diffuse = uint.MaxValue, bool solid = false)
    {
        _device = device; _source = source; _transform = transform; _compositing = compositing;
        _triangles = triangles; _diffuse = diffuse; _solid = solid;
        _handle = GCHandle.Alloc(this);
        nint token = GCHandle.ToIntPtr(_handle);
        var state = new Direct3D9ContextState(Matrix4x4.Identity, Matrix4x4.Identity,
            Matrix4x4.Identity, Matrix4x4.Identity, Cull.None, Cmpfunc.Always, false, AliasedClip: clip);
        Operations = new(
            (out nint brush) => { brush = token; return 0; },
            static (_, _, _) => { }, static _ => { },
            _ => new Direct3D9ImmediateBrushRealizer(token,
                static _ => { }, static _ => { }, static (_, _) => { },
                static _ => false, static _ => Direct3D9BrushType.Bitmap,
                static _ => false, static _ => true, static _ => 0, static _ => 0,
                static (_, _, _, _) => 0, static (_, _, _, _) => 0,
                static (_, _) => { }, static (_, _) => { }),
            (MilRectF _, out nint shape) => { shape = token; return 0; },
            () => target.EnsureState(state),
            static (nint shape, Matrix4x4? matrix, MilRectF bounds, out Direct3D9SafeClippedShape clipped) =>
            { clipped = new(shape, matrix, bounds, false); return 0; },
            static (_, _, _, reason) => reason,
            static (Direct3D9PathClipperState input, out Direct3D9PathClipperState output) =>
            { output = input; return 0; },
            static (Direct3D9PathClipperState input, nint _, Matrix4x4 _, out Direct3D9PathClipperState output) =>
            { output = input; return 0; },
            GetBounds, CreateBrush, CreateGeometry, CreateGeometry,
            compositing, MilAntiAliasMode.None, clip);
    }

    internal Direct3D9ProductionBitmapDrawOperations Operations { get; }
    internal nint BitmapSource => GCHandle.ToIntPtr(_handle);

    private int GetBounds(Direct3D9PathClipperState state, out MilRectF bounds)
    {
        double left = state.Bounds.Left, top = state.Bounds.Top;
        double right = state.Bounds.Right, bottom = state.Bounds.Bottom;
        if (_triangles is { Count: > 0 } triangles)
        {
            left = triangles.Min(static point => point.X);
            top = triangles.Min(static point => point.Y);
            right = triangles.Max(static point => point.X);
            bottom = triangles.Max(static point => point.Y);
        }
        var a = _transform.Apply(left, top);
        var b = _transform.Apply(right, top);
        var c = _transform.Apply(left, bottom);
        var d = _transform.Apply(right, bottom);
        bounds = new((float)Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X)),
            (float)Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y)),
            (float)Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X)),
            (float)Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y)));
        return 0;
    }

    private int CreateGeometry(Direct3D9PathClipperState state, out Direct3D9PathGeometryGenerator? geometry)
    {
        Direct3D9ExpandedVertex Vertex(float x, float y)
        {
            var point = _transform.Apply(x, y);
            return new() { X = (float)point.X, Y = (float)point.Y, Diffuse = _diffuse };
        }
        var a = Vertex(state.Bounds.Left, state.Bounds.Top);
        var b = Vertex(state.Bounds.Right, state.Bounds.Top);
        var c = Vertex(state.Bounds.Left, state.Bounds.Bottom);
        var d = Vertex(state.Bounds.Right, state.Bounds.Bottom);
        geometry = new(builder =>
        {
            if (_triangles is { } triangles)
            {
                for (int i = 0; i < triangles.Count; i += 3)
                    builder.AddTriangleListVertices(
                        Vertex((float)triangles[i].X, (float)triangles[i].Y),
                        Vertex((float)triangles[i + 1].X, (float)triangles[i + 1].Y),
                        Vertex((float)triangles[i + 2].X, (float)triangles[i + 2].Y));
            }
            else
            {
                builder.AddTriangleListVertices(a, b, c);
                builder.AddTriangleListVertices(c, b, d);
            }
            return 0;
        }, static () => { });
        return 0;
    }

    private int CreateBrush(nint brush, Direct3D9PathBrushContext context, out Direct3D9PathHardwareBrush? hardwareBrush)
    {
        hardwareBrush = new(
            static (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
            { description = null; return Direct3D9Factory.NotImplementedHResult; },
            (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
            {
                var draw = new Direct3D9ProductionVertexDraw(_device);
                var owner = new Direct3D9VertexPipelineOwner(
                    (out Direct3D9ExpandedVertexConverter? converter) => Direct3D9ExpandedVertexConverter.Create(
                        Direct3D9VertexFormatAttribute.Xy | Direct3D9VertexFormatAttribute.Diffuse,
                        Direct3D9VertexFormatAttribute.Xyz | Direct3D9VertexFormatAttribute.Diffuse
                            | (_solid ? Direct3D9VertexFormatAttribute.None : Direct3D9VertexFormatAttribute.Uv1),
                        Direct3D9VertexFormatAttribute.None, out converter), draw.Draw);
                description = new(new Direct3D9ProductionPipelineInitializer([_source], owner,
                    SendState, static () => { }, mappings: _solid ? [] : null), null, true);
                return 0;
            }, static () => { });
        return 0;
    }

    private int SendState()
    {
        int hr = _solid ? _device.SetTexture(0, null)
            : _source.SendDeviceStates?.Invoke(0, 0) ?? Direct3D9Factory.NotImplementedHResult;
        if (hr >= 0) hr = _device.SetVertexShader(null);
        if (hr >= 0) hr = _device.SetPixelShader(null);
        if (hr >= 0) hr = _device.SetRenderState((Renderstatetype)27, _compositing == MilCompositingMode.SourceOver ? 1u : 0u);
        if (hr >= 0) hr = _device.SetRenderState((Renderstatetype)19, 2);
        if (hr >= 0) hr = _device.SetRenderState((Renderstatetype)20, 6);
        if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Colorop, 4);
        if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Colorarg1, 2);
        if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Colorarg2, 0);
        if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Alphaop, 4);
        if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Alphaarg1, 2);
        if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Alphaarg2, 0);
        if (_solid)
        {
            if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Colorop, 2);
            if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Alphaop, 2);
            if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Colorarg1, 0);
            if (hr >= 0) hr = _device.SetTextureStageState(0, Texturestagestatetype.Alphaarg1, 0);
        }
        if (hr >= 0) hr = _device.DisableTextureStage(1);
        return hr;
    }

    public void Dispose()
    {
        if (_handle.IsAllocated) _handle.Free();
    }
}
