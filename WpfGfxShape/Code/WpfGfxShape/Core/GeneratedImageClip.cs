namespace WpfGfxShape.Core;

internal static class GeneratedImageClip
{
    internal static bool TryIntersect(GeneratedProtocolResource? resource, GeneratedImageTransform transform,
        Direct3D9SurfaceRect? inherited, out Direct3D9SurfaceRect? clip)
    {
        clip = inherited;
        if (resource is null) return true;
        if (!TryRectangle(resource, transform, out var rectangle, out var combined)) return false;
        if (!combined.IsPositiveAxisAligned) return false;
        MilRectD bounds = combined.Apply(rectangle);
        double right = bounds.X + bounds.Width, bottom = bounds.Y + bounds.Height;
        if (!IsInteger(bounds.X) || !IsInteger(bounds.Y) || !IsInteger(right) || !IsInteger(bottom)) return false;
        int left = (int)bounds.X, top = (int)bounds.Y;
        int r = Math.Max(left, (int)right), b = Math.Max(top, (int)bottom);
        if (inherited is { } previous)
        {
            left = Math.Max(left, previous.Left); top = Math.Max(top, previous.Top);
            r = Math.Max(left, Math.Min(r, previous.Right)); b = Math.Max(top, Math.Min(b, previous.Bottom));
        }
        clip = new(left, top, r, b);
        return true;
    }

    internal static bool TryCreateMask(GeneratedProtocolResource? resource, GeneratedImageTransform transform,
        out SoftwareImageCoverage? mask, int depth = 0)
    {
        mask = null;
        if (resource is null) return true;
        if (depth > 256) return false;
        if (resource is GeneratedLineGeometryResource line)
        {
            if (!GeneratedImageTransform.TryResolve(line.Transform, out _)) return false;
            mask = SoftwareImageCoverage.Combine(null, null, MilCombineMode.Union);
            return true;
        }
        if (resource is GeneratedRectangleGeometryResource rounded
            && (rounded.CurrentValue.RadiusX != 0 || rounded.CurrentValue.RadiusY != 0))
        {
            if (!SoftwarePathCoverage.TryCreateRoundedRectangle(rounded, transform, out var coverage) || coverage is null) return false;
            mask = new(coverage);
            return true;
        }
        if (resource is GeneratedEllipseGeometryResource ellipse)
        {
            if (!SoftwarePathCoverage.TryCreateEllipse(ellipse, transform, out var coverage) || coverage is null) return false;
            mask = new(coverage);
            return true;
        }
        if (resource is GeneratedPathGeometryResource path)
        {
            if (!SoftwarePathCoverage.TryCreate(path, transform, out var coverage) || coverage is null) return false;
            mask = new(coverage);
            return true;
        }
        if (resource is GeneratedGeometryGroupResource group)
        {
            if (!SoftwarePathCoverage.TryCreateGroup(group, transform, out var coverage) || coverage is null) return false;
            mask = new(coverage);
            return true;
        }
        if (resource is GeneratedCombinedGeometryResource combined)
        {
            if (!GeneratedImageTransform.TryResolve(combined.Transform, out var combinedTransform)) return false;
            var current = transform.Prepend(combinedTransform);
            if (!TryCreateMask(combined.Geometry1, current, out var first, depth + 1)
                || !TryCreateMask(combined.Geometry2, current, out var second, depth + 1)) return false;
            mask = SoftwareImageCoverage.Combine(first, second, combined.CombineMode);
            return true;
        }
        if (!TryRectangle(resource, transform, out var rectangle, out var matrix)) return false;
        return SoftwareImageCoverage.TryCreate(rectangle, matrix, out mask);
    }

    private static bool TryRectangle(GeneratedProtocolResource resource, GeneratedImageTransform parent,
        out MilRectD rectangle, out GeneratedImageTransform transform)
    {
        rectangle = default;
        transform = parent;
        if (resource is not GeneratedRectangleGeometryResource geometry) return false;
        var current = geometry.CurrentValue;
        if (current.RadiusX != 0 || current.RadiusY != 0
            || !GeneratedImageTransform.TryResolve(geometry.Transform, out var local)) return false;
        rectangle = current.Rect;
        transform = parent.Prepend(local);
        return true;
    }

    private static bool IsInteger(double value) => double.IsFinite(value) && value >= int.MinValue
        && value <= int.MaxValue && value == Math.Truncate(value);
}
