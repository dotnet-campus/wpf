using System.Numerics;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal static unsafe class GeneratedHardwareCacheGeometry
{
    internal static int Draw(GeneratedRenderDataInstruction instruction, FrozenVisualTree? frozen,
        Direct3D9Device device, Direct3D9SurfaceRenderTarget target, GeneratedImageTransform transform,
        Direct3D9SurfaceRect? clip, uint scaling, MilCompositingMode compositing)
    {
        if (instruction.ResourceSlots[0] is null) return 0;
        if (instruction.ResourceSlots[1] is not null
            || instruction.ResourceSlots[2] is not GeneratedGeometryResource geometry
            || frozen?.CacheRequest(instruction.ResourceSlots[0]) is not { } request)
            return Direct3D9Factory.NotImplementedHResult;
        if (!GeneratedGeometryBounds.TryGet(geometry, GeneratedImageTransform.Identity, out var bounds, transform)
            || !SoftwareGroupStrokeContours.TryRead(geometry, out var figures, deviceTransform: transform))
            return Direct3D9Factory.NotImplementedHResult;
        if (bounds.Width <= 0 || bounds.Height <= 0) return 0;
        // A fan is valid only for one convex contour; never render holes or concave paths as their bounds.
        var filled = figures.Where(static f => f.Fillable && f.Points.Length >= 3).ToArray();
        if (filled.Length != 1) return Direct3D9Factory.NotImplementedHResult;
        var points = filled[0].Points;
        int count = points.Length;
        if (points[0] == points[^1]) count--;
        double sign = 0;
        for (int i = 0; i < count; i++)
        {
            var a = points[i]; var b = points[(i + 1) % count]; var c = points[(i + 2) % count];
            double cross = (b.X - a.X) * (c.Y - b.Y) - (b.Y - a.Y) * (c.X - b.X);
            if (cross == 0) continue;
            if (sign != 0 && Math.Sign(sign) != Math.Sign(cross)) return Direct3D9Factory.NotImplementedHResult;
            sign = cross;
        }
        var triangles = new List<MilPoint2D>();
        for (int i = 1; i + 1 < count; i++) { triangles.Add(points[0]); triangles.Add(points[i]); triangles.Add(points[i + 1]); }
        if (!double.IsFinite(request.CurrentOpacity)) return Direct3D9Factory.InvalidArgumentHResult;
        if (request.RelativeTransform is not null) return Direct3D9Factory.NotImplementedHResult;
        var mapping = transform.Prepend(request.Transform);
        if (!mapping.TryInvert(out var inverse)) return Direct3D9Factory.NonInvertibleMatrixHResult;
        int hr = request.Brush.AcquireHardwareCache(request, device, target.GetDirect3DTextureFormat(), scaling,
            out var cache, out bool candidate);
        if (hr < 0) return hr;
        if (cache is null) return 0;
        try
        {
            var uv = new Matrix3x2((float)(inverse.ScaleX / bounds.Width), (float)(inverse.M12 / bounds.Height),
                (float)(inverse.M21 / bounds.Width), (float)(inverse.ScaleY / bounds.Height),
                (float)((inverse.X - bounds.X) / bounds.Width), (float)((inverse.Y - bounds.Y) / bounds.Height));
            var texture = cache.Target.GetTextureNoRef();
            if (texture is null) return Direct3D9Factory.InvalidCallHResult;
            var state = new Direct3D9BitmapColorSourceDeviceState(device, (IDirect3DBaseTexture9*)texture.Texture,
                scaling == 1 ? MilBitmapInterpolationMode.NearestNeighbor : MilBitmapInterpolationMode.Linear,
                Textureaddress.Clamp, Textureaddress.Clamp, false, null, uv);
            var source = state.CreatePipelineColorSource(static () => 0);
            uint alpha = (uint)Math.Round(Math.Clamp(request.CurrentOpacity, 0, 1) * 255);
            uint diffuse = alpha * 0x01010101u;
            using var operations = new GeneratedHardwareBitmapOperations(device, target, source, transform,
                clip ?? new(0, 0, int.MaxValue, int.MaxValue), compositing, triangles, diffuse);
            hr = target.ProductionDrawBitmap(new(Matrix4x4.Identity,
                new(0, 0, checked((int)Math.Ceiling(bounds.Width)), checked((int)Math.Ceiling(bounds.Height)))),
                operations.BitmapSource, 0, operations.Operations);
            if (hr >= 0 && candidate) request.Brush.CommitHardwareCache(request, cache);
            return hr;
        }
        finally { cache.Dispose(); }
    }
}
