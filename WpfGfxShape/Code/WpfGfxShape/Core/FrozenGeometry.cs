namespace WpfGfxShape.Core;

internal sealed class FrozenGeometry : IDisposable
{
    private readonly List<GeneratedProtocolResource> _owned = [];
    internal GeneratedProtocolResource? Geometry { get; private set; }

    internal static int CapturePure(GeneratedProtocolResource? resource,
        HashSet<GeneratedProtocolResource> active, out FrozenGeometry? snapshot)
    {
        snapshot = null;
        var candidate = new FrozenGeometry();
        bool captured = false;
        try
        {
            int hr = candidate.Capture(resource, active, out var geometry);
            if (hr < 0) return hr;
            candidate.Geometry = geometry;
            snapshot = candidate;
            captured = true;
            return 0;
        }
        finally { if (!captured) candidate.Dispose(); }
    }

    private int Capture(GeneratedProtocolResource? resource, HashSet<GeneratedProtocolResource> active,
        out GeneratedProtocolResource? geometry)
    {
        geometry = null;
        if (resource is null) return 0;
        if (active.Count >= 256 || !active.Add(resource)) return Direct3D9Factory.NotImplementedHResult;
        try
        {
            GeneratedProtocolResource? transform;
            GeneratedGeometryResource clone;
            switch (resource)
            {
                case GeneratedLineGeometryResource line:
                    clone = line.CaptureValue(); transform = line.Transform; break;
                case GeneratedRectangleGeometryResource rectangle:
                    clone = rectangle.CaptureValue(); transform = rectangle.Transform; break;
                case GeneratedEllipseGeometryResource ellipse:
                    clone = ellipse.CaptureValue(); transform = ellipse.Transform; break;
                case GeneratedPathGeometryResource path:
                    clone = path.CaptureValue(); transform = path.Transform; break;
                case GeneratedGeometryGroupResource group:
                {
                    var children = new GeneratedProtocolResource[group.Children.Count];
                    for (int i = 0; i < children.Length; i++)
                    {
                        int hr = Capture(group.Children[i], active, out var child);
                        if (hr < 0) return hr;
                        if (child is null) return Direct3D9Factory.NotImplementedHResult;
                        children[i] = child;
                    }
                    clone = group.CaptureValue(children); transform = group.Transform; break;
                }
                case GeneratedCombinedGeometryResource combined:
                {
                    int hr = Capture(combined.Geometry1, active, out var first);
                    if (hr < 0) return hr;
                    hr = Capture(combined.Geometry2, active, out var second);
                    if (hr < 0) return hr;
                    clone = combined.CaptureValue(first, second); transform = combined.Transform; break;
                }
                default: return Direct3D9Factory.NotImplementedHResult;
            }
            _owned.Add(clone);
            if (!GeneratedImageTransform.TryResolve(transform, out var matrix))
                return Direct3D9Factory.NotImplementedHResult;
            var detached = GeneratedMatrixTransformResource.CreateStatic(
                new(matrix.ScaleX, matrix.M12, matrix.M21, matrix.ScaleY, matrix.X, matrix.Y));
            _owned.Add(detached);
            switch (clone)
            {
                case GeneratedLineGeometryResource line: line.SetStaticTransform(detached); break;
                case GeneratedRectangleGeometryResource rectangle: rectangle.SetStaticTransform(detached); break;
                case GeneratedEllipseGeometryResource ellipse: ellipse.SetStaticTransform(detached); break;
                case GeneratedPathGeometryResource path: path.SetStaticTransform(detached); break;
                case GeneratedGeometryGroupResource group: group.SetStaticTransform(detached); break;
                case GeneratedCombinedGeometryResource combined: combined.SetStaticTransform(detached); break;
            }
            geometry = clone;
            return 0;
        }
        finally { active.Remove(resource); }
    }

    public void Dispose()
    {
        Geometry = null;
        for (int i = _owned.Count - 1; i >= 0; i--) _owned[i].Release();
        _owned.Clear();
    }
}

internal sealed partial class GeneratedLineGeometryResource
{
    internal GeneratedLineGeometryResource CaptureValue() => new() { Value = CurrentValue };
    internal void SetStaticTransform(GeneratedProtocolResource transform) => Transform = transform;
}
internal sealed partial class GeneratedRectangleGeometryResource
{
    internal GeneratedRectangleGeometryResource CaptureValue() => new() { Value = CurrentValue };
    internal void SetStaticTransform(GeneratedProtocolResource transform) => Transform = transform;
}
internal sealed partial class GeneratedEllipseGeometryResource
{
    internal GeneratedEllipseGeometryResource CaptureValue() => new() { Value = CurrentValue };
    internal void SetStaticTransform(GeneratedProtocolResource transform) => Transform = transform;
}
internal sealed partial class GeneratedPathGeometryResource
{
    internal GeneratedPathGeometryResource CaptureValue() => new() { FillRule = FillRule, Figures = Figures.ToArray() };
    internal void SetStaticTransform(GeneratedProtocolResource transform) => Transform = transform;
}
internal sealed partial class GeneratedGeometryGroupResource
{
    internal GeneratedGeometryGroupResource CaptureValue(GeneratedProtocolResource[] children)
        => new() { FillRule = FillRule, Children = children };
    internal void SetStaticTransform(GeneratedProtocolResource transform) => Transform = transform;
}
internal sealed partial class GeneratedCombinedGeometryResource
{
    internal GeneratedCombinedGeometryResource CaptureValue(GeneratedProtocolResource? first, GeneratedProtocolResource? second)
        => new() { CombineMode = CombineMode, Geometry1 = first, Geometry2 = second };
    internal void SetStaticTransform(GeneratedProtocolResource transform) => Transform = transform;
}
