namespace WpfGfxShape.Core;

internal static class GeneratedStrokeGeometry
{
    internal static bool TryGetBounds(GeneratedProtocolResource geometry, GeneratedPenResource pen,
        GeneratedImageTransform parent, out MilRectD bounds)
        => TryCreate(geometry, pen, parent, out _, out bounds, createCoverage: false);

    internal static bool TryCreate(GeneratedProtocolResource geometry, GeneratedPenResource pen,
        GeneratedImageTransform parent, out SoftwareImageCoverage? coverage, out MilRectD bounds,
        bool createCoverage = true)
    {
        coverage = null;
        bounds = default;
        if (geometry is GeneratedGeometryGroupResource or GeneratedEllipseGeometryResource or GeneratedCombinedGeometryResource)
            return SoftwareGroupStrokeContours.TryRead(geometry, out var groupFigures, deviceTransform: parent)
                && SoftwareStrokeWidening.TryCreate(groupFigures, pen, parent, out coverage, out bounds, createCoverage);
        if (geometry is GeneratedPathGeometryResource path)
            return SoftwareStrokeContours.TryRead(path, out var figures)
                && SoftwareStrokeWidening.TryCreate(figures, pen, parent, out coverage, out bounds, createCoverage);
        if (geometry is GeneratedLineGeometryResource line)
            return TryCreateLine(line, pen, parent, out coverage, out bounds, createCoverage);
        if (geometry is not GeneratedRectangleGeometryResource rectangle
            || !float.IsFinite((float)pen.MiterLimit)
            || !GeneratedImageTransform.TryResolve(rectangle.Transform, out var local)) return false;
        var value = rectangle.CurrentValue;
        if (value.RadiusX != 0 || value.RadiusY != 0)
            return SoftwareCurvedStrokeContours.TryRead(rectangle, out var roundedFigure)
                && SoftwareStrokeWidening.TryCreate([roundedFigure], pen, parent, out coverage, out bounds, createCoverage);
        var rect = value.Rect;
        float width = MathF.Abs((float)pen.CurrentThickness);
        if (!float.IsFinite(width) || !double.IsFinite(rect.X) || !double.IsFinite(rect.Y)
            || !double.IsFinite(rect.Width) || !double.IsFinite(rect.Height)) return false;
        if (rect.Width < 0 || rect.Height < 0) return true;
        if (pen.DashStyle is { Dashes.Length: > 0 }
            || pen.LineJoin != MilPenJoin.Miter || (float)pen.MiterLimit < MathF.Sqrt(2))
        {
            var corners = new MilPoint2D[]
            {
                new((float)rect.X, (float)rect.Y),
                new((float)(rect.X + rect.Width), (float)rect.Y),
                new((float)(rect.X + rect.Width), (float)(rect.Y + rect.Height)),
                new((float)rect.X, (float)(rect.Y + rect.Height))
            };
            for (int i = 0; i < corners.Length; i++)
            {
                var point = local.Apply(corners[i].X, corners[i].Y);
                corners[i] = new((float)point.X, (float)point.Y);
            }
            return SoftwareStrokeWidening.TryCreate([new(corners, true)], pen, parent, out coverage, out bounds, createCoverage);
        }
        // CRectangle::WidenToShape uses alternate outer/inner figures, then transforms them.
        float half = 0.5f * width;
        float leftEdge = (float)rect.X, topEdge = (float)rect.Y;
        float rightEdge = (float)(rect.X + rect.Width), bottomEdge = (float)(rect.Y + rect.Height);
        float outerLeft = leftEdge - half, outerTop = topEdge - half;
        float outerRight = rightEdge + half, outerBottom = bottomEdge + half;
        var outer = new MilRectD(outerLeft, outerTop, outerRight - outerLeft, outerBottom - outerTop);
        var transform = parent.Prepend(local);
        SoftwareImageCoverage? outside = null;
        if (createCoverage && !SoftwareImageCoverage.TryCreate(outer, transform, out outside)) return false;
        SoftwareImageCoverage? inside = null;
        if (createCoverage && width < rightEdge - leftEdge && width < bottomEdge - topEdge)
        {
            float innerLeft = leftEdge + half, innerTop = topEdge + half;
            float innerRight = rightEdge - half, innerBottom = bottomEdge - half;
            if (!SoftwareImageCoverage.TryCreate(new(innerLeft, innerTop,
                innerRight - innerLeft, innerBottom - innerTop), transform, out inside)) return false;
        }
        if (createCoverage) coverage = SoftwareImageCoverage.Combine(outside, inside, MilCombineMode.Exclude);
        var a = transform.Apply(outer.X, outer.Y);
        var b = transform.Apply(outer.X + outer.Width, outer.Y);
        var c = transform.Apply(outer.X, outer.Y + outer.Height);
        var d = transform.Apply(outer.X + outer.Width, outer.Y + outer.Height);
        double left = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
        double top = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));
        double right = Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X));
        double bottom = Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y));
        bounds = new(left, top, right - left, bottom - top);
        return double.IsFinite(left) && double.IsFinite(top)
            && double.IsFinite(bounds.Width) && double.IsFinite(bounds.Height);
    }

    private static bool TryCreateLine(GeneratedLineGeometryResource line, GeneratedPenResource pen,
        GeneratedImageTransform parent, out SoftwareImageCoverage? coverage, out MilRectD bounds,
        bool createCoverage)
    {
        coverage = null;
        bounds = default;
        if (!GeneratedImageTransform.TryResolve(line.Transform, out var local)) return false;
        var value = line.CurrentValue;
        // GetShapeDataCore transforms CLine before the render transform widens its rails.
        var start = local.Apply((float)value.StartPoint.X, (float)value.StartPoint.Y);
        var end = local.Apply((float)value.EndPoint.X, (float)value.EndPoint.Y);
        double x0 = (float)start.X, y0 = (float)start.Y;
        double x1 = (float)end.X, y1 = (float)end.Y;
        double half = MathF.Abs((float)pen.CurrentThickness) * 0.5f;
        double dx = x1 - x0, dy = y1 - y0;
        double length = Math.Sqrt(dx * dx + dy * dy);
        if (!double.IsFinite(length) || !double.IsFinite(half)) return false;
        if (length != 0 && (pen.DashStyle is { Dashes.Length: > 0 }
            || pen.StartLineCap == MilPenCap.Round || pen.EndLineCap == MilPenCap.Round))
            return SoftwareStrokeWidening.TryCreate(
                [new([new(x0, y0), new(x1, y1)], false)], pen, parent, out coverage, out bounds, createCoverage);
        if (length == 0)
        {
            return SoftwareStrokeWidening.TryCreate(
                [new([new(x0, y0)], false)], pen, parent, out coverage, out bounds, createCoverage);
        }
        double tx = dx / length * half, ty = dy / length * half;
        double nx = -ty, ny = tx;
        if (pen.StartLineCap == MilPenCap.Square) { x0 -= tx; y0 -= ty; }
        if (pen.EndLineCap == MilPenCap.Square) { x1 += tx; y1 += ty; }
        // DoBaseCap connects the rails through the triangle apex one pen radius away.
        var outline = new List<MilPoint2D>(6) { new(x0 + nx, y0 + ny), new(x1 + nx, y1 + ny) };
        if (pen.EndLineCap == MilPenCap.Triangle) outline.Add(new(x1 + tx, y1 + ty));
        outline.Add(new(x1 - nx, y1 - ny));
        outline.Add(new(x0 - nx, y0 - ny));
        if (pen.StartLineCap == MilPenCap.Triangle) outline.Add(new(x0 - tx, y0 - ty));
        MilPoint2D[] points = outline.ToArray();
        SoftwarePathCoverage? path = null;
        if (createCoverage && (!SoftwarePathCoverage.TryCreatePolygon(points, parent, out path) || path is null)) return false;
        double left = double.PositiveInfinity, top = double.PositiveInfinity;
        double right = double.NegativeInfinity, bottom = double.NegativeInfinity;
        foreach (var point in points)
        {
            var transformed = parent.Apply(point.X, point.Y);
            left = Math.Min(left, transformed.X); top = Math.Min(top, transformed.Y);
            right = Math.Max(right, transformed.X); bottom = Math.Max(bottom, transformed.Y);
        }
        bounds = new(left, top, right - left, bottom - top);
        if (!double.IsFinite(left) || !double.IsFinite(top)
            || !double.IsFinite(bounds.Width) || !double.IsFinite(bounds.Height)) return false;
        if (path is not null) coverage = new(path);
        return true;
    }
}
