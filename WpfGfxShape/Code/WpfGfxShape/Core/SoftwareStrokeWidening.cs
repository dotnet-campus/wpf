namespace WpfGfxShape.Core;

internal static class SoftwareStrokeWidening
{
    internal static bool TryCreate(IEnumerable<SoftwareStrokeContours.Figure> figures, GeneratedPenResource pen,
        GeneratedImageTransform transform, out SoftwareImageCoverage? coverage, out MilRectD bounds,
        bool createCoverage = true, GeneratedImageTransform? toleranceTransform = null,
        Direct3D9SurfaceRect? clip = null, List<SoftwareStrokeContours.Figure>? outputContours = null)
    {
        coverage = null; bounds = default;
        var arcTransform = toleranceTransform ?? transform;
        double radius = MathF.Abs((float)pen.CurrentThickness) * 0.5;
        if (!double.IsFinite(radius) || !double.IsFinite(pen.MiterLimit)) return false;
        if (pen.DashStyle is { Dashes.Length: > 0 } dash)
        {
            if (!SoftwareStrokeDashes.TrySplit(figures, dash, radius * 2, pen.DashCap, out var runs)) return false;
            figures = runs;
        }
        var parts = new List<SoftwareImageCoverage>();
        double left = double.PositiveInfinity, top = double.PositiveInfinity;
        double right = double.NegativeInfinity, bottom = double.NegativeInfinity;
        bool Polygon(List<MilPoint2D> points)
        {
            if (createCoverage)
            {
                var devicePoints = points.Select(point =>
                {
                    var device = transform.Apply(point.X, point.Y);
                    return new MilPoint2D(device.X, device.Y);
                }).ToArray();
                if (!SoftwarePathCoverage.TryCreateFigures([devicePoints], MilFillMode.Winding, out var path, clip)
                    || path is null) return false;
                parts.Add(new(path));
            }
            foreach (var p in points)
            {
                var device = transform.Apply(p.X, p.Y);
                if (!double.IsFinite(device.X) || !double.IsFinite(device.Y)) return false;
                left = Math.Min(left, device.X); top = Math.Min(top, device.Y);
                right = Math.Max(right, device.X); bottom = Math.Max(bottom, device.Y);
            }
            if (outputContours is not null)
            {
                var contour = points.Select(point =>
                {
                    var device = transform.Apply(point.X, point.Y);
                    return new MilPoint2D(device.X, device.Y);
                }).ToArray();
                double area = 0;
                for (int i = 0; i < contour.Length; i++)
                {
                    var a = contour[i]; var b = contour[(i + 1) % contour.Length];
                    area += a.X * b.Y - a.Y * b.X;
                }
                if (area < 0) Array.Reverse(contour);
                outputContours.Add(new(contour, true));
            }
            return true;
        }
        foreach (var figure in figures)
        {
            var distinctPoints = new List<MilPoint2D>();
            var distinctJoins = new List<bool>();
            for (int i = 0; i < figure.Points.Length; i++)
            {
                var point = figure.Points[i];
                bool smooth = figure.SmoothJoins is { } source && i < source.Length && source[i];
                if (distinctPoints.Count != 0 && distinctPoints[^1] == point)
                {
                    distinctJoins[^1] &= smooth;
                    continue;
                }
                distinctPoints.Add(point);
                distinctJoins.Add(smooth);
            }
            var points = distinctPoints.ToArray();
            int count = points.Length;
            if (figure.Closed && count > 1 && points[0] == points[^1]) count--;
            if (count == 0) continue;
            if (outputContours is null && figure.Closed && count == 4 && pen.LineJoin == MilPenJoin.Miter
                && pen.MiterLimit >= Math.Sqrt(2) && pen.DashStyle is not { Dashes.Length: > 0 }
                && !distinctJoins.Take(count).Any(smooth => smooth)
                && Enumerable.Range(0, count).All(i =>
                    (points[i].X == points[(i + 1) % count].X) != (points[i].Y == points[(i + 1) % count].Y)))
            {
                double minX = points.Take(count).Min(point => point.X), maxX = points.Take(count).Max(point => point.X);
                double minY = points.Take(count).Min(point => point.Y), maxY = points.Take(count).Max(point => point.Y);
                if (maxX > minX && maxY > minY
                    && points.Take(count).Distinct().Count() == 4
                    && points.Take(count).All(point => (point.X == minX || point.X == maxX) && (point.Y == minY || point.Y == maxY)))
                {
                    // CRectangle widens simple circular miter pens as outer/inner alternate contours.
                    List<MilPoint2D> outer = [new(minX - radius, minY - radius), new(maxX + radius, minY - radius),
                        new(maxX + radius, maxY + radius), new(minX - radius, maxY + radius)];
                    if (!Polygon(outer)) return false;
                    if (createCoverage && radius * 2 < maxX - minX && radius * 2 < maxY - minY)
                    {
                        MilPoint2D[] inner = [new(minX + radius, minY + radius), new(maxX - radius, minY + radius),
                            new(maxX - radius, maxY - radius), new(minX + radius, maxY - radius)];
                        MilPoint2D[] ToDevice(IEnumerable<MilPoint2D> contour) => contour.Select(point =>
                        {
                            var device = transform.Apply(point.X, point.Y);
                            return new MilPoint2D(device.X, device.Y);
                        }).ToArray();
                        if (!SoftwarePathCoverage.TryCreateFigures([ToDevice(outer), ToDevice(inner)], MilFillMode.Alternate, out var path, clip)
                            || path is null) return false;
                        parts[^1] = new(path);
                    }
                    continue;
                }
            }
            if (count == 1 || points.All(point => point == points[0]))
            {
                var center = points[0];
                for (int end = 0; end < 2; end++)
                {
                    var cap = end == 0
                        ? (figure.StartsAtGap ? pen.DashCap : pen.StartLineCap)
                        : (figure.EndsAtGap ? pen.DashCap : pen.EndLineCap);
                    if (cap == MilPenCap.Flat) continue;
                    double direction = end == 0 ? -1 : 1;
                    var a = new MilPoint2D(center.X, center.Y + radius);
                    var b = new MilPoint2D(center.X, center.Y - radius);
                    var tip = new MilPoint2D(center.X + direction * radius, center.Y);
                    var outline = new List<MilPoint2D> { a };
                    if (cap == MilPenCap.Square)
                    {
                        outline.Add(new(tip.X, a.Y)); outline.Add(new(tip.X, b.Y));
                    }
                    else if (cap == MilPenCap.Triangle) outline.Add(tip);
                    else if (cap == MilPenCap.Round) AppendArc(outline, center, a, b, end == 0 ? 1 : -1, arcTransform);
                    else return false;
                    outline.Add(b);
                    if (figure.PointTangent is { } tangent)
                    {
                        for (int i = 0; i < outline.Count; i++)
                        {
                            double x = outline[i].X - center.X, y = outline[i].Y - center.Y;
                            outline[i] = new(center.X + x * tangent.X - y * tangent.Y,
                                center.Y + x * tangent.Y + y * tangent.X);
                        }
                    }
                    if (!Polygon(outline)) return false;
                }
                continue;
            }
            int segments = figure.Closed ? count : count - 1;
            var tangents = new MilPoint2D[segments];
            var normals = new MilPoint2D[segments];
            var starts = new MilPoint2D[segments, 2];
            for (int i = 0; i < segments; i++)
            {
                var a = points[i]; var b = points[(i + 1) % count];
                double dx = b.X - a.X, dy = b.Y - a.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (!double.IsFinite(length) || length == 0) return false;
                tangents[i] = new(dx / length, dy / length);
                var n = new MilPoint2D(-dy / length * radius, dx / length * radius);
                normals[i] = n;
                starts[i, 0] = new(a.X + n.X, a.Y + n.Y);
                starts[i, 1] = new(a.X - n.X, a.Y - n.Y);
            }
            for (int i = figure.Closed ? 0 : 1; i < (figure.Closed ? count : count - 1); i++)
            {
                int previous = (i + segments - 1) % segments;
                var incoming = tangents[previous]; var outgoing = tangents[i];
                double cross = incoming.X * outgoing.Y - incoming.Y * outgoing.X;
                MilPenJoin join = distinctJoins[i] ? MilPenJoin.Round : pen.LineJoin;
                if (cross == 0)
                {
                    if (incoming.X * outgoing.X + incoming.Y * outgoing.Y >= 0) continue;
                    var reversal = points[i]; var n = normals[previous];
                    if (join == MilPenJoin.Miter)
                    {
                        double extension = radius * Math.Max(1, (float)pen.MiterLimit);
                        var tip = new MilPoint2D(reversal.X + incoming.X * extension, reversal.Y + incoming.Y * extension);
                        if (!Polygon([new(reversal.X + n.X, reversal.Y + n.Y), new(tip.X + n.X, tip.Y + n.Y),
                            new(tip.X - n.X, tip.Y - n.Y), new(reversal.X - n.X, reversal.Y - n.Y)])) return false;
                    }
                    else if (join == MilPenJoin.Round)
                    {
                        var arcStart = new MilPoint2D(reversal.X + n.X, reversal.Y + n.Y);
                        var arcEnd = new MilPoint2D(reversal.X - n.X, reversal.Y - n.Y);
                        var outline = new List<MilPoint2D> { arcStart };
                        AppendArc(outline, reversal, arcStart, arcEnd, -1, arcTransform);
                        outline.Add(arcEnd);
                        if (!Polygon(outline)) return false;
                    }
                    continue;
                }
                double side = cross > 0 ? -1 : 1;
                var center = points[i];
                var a = new MilPoint2D(center.X + normals[previous].X * side, center.Y + normals[previous].Y * side);
                var b = new MilPoint2D(center.X + normals[i].X * side, center.Y + normals[i].Y * side);
                var corner = new List<MilPoint2D> { center, a };
                bool extendToOffset = true;
                if (join == MilPenJoin.Round)
                    AppendArc(corner, center, a, b, cross > 0 ? 1 : -1, arcTransform);
                else if (join is MilPenJoin.Miter or MilPenJoin.MiterClipped)
                {
                    double distance = ((b.X - a.X) * outgoing.Y - (b.Y - a.Y) * outgoing.X) / cross;
                    var miter = new MilPoint2D(a.X + incoming.X * distance, a.Y + incoming.Y * distance);
                    double mx = miter.X - center.X, my = miter.Y - center.Y;
                    if (mx * mx + my * my <= radius * radius * Math.Max(1, pen.MiterLimit) * Math.Max(1, pen.MiterLimit))
                    {
                        corner.Add(miter);
                        // MiterTo keeps the miter as the next segment's current outer point.
                        if (!figure.Closed || i != 0)
                        {
                            starts[i, side > 0 ? 0 : 1] = miter;
                            extendToOffset = false;
                        }
                    }
                    else if (join == MilPenJoin.Miter)
                    {
                        // DoLimitedMiter cuts both rails at the nominal radial limit.
                        double dot = incoming.X * outgoing.X + incoming.Y * outgoing.Y;
                        double denominator = Math.Sqrt(Math.Max(0, (1 - dot) * 0.5));
                        if (denominator == 0) return false;
                        double offset = radius * (Math.Max(1, (float)pen.MiterLimit)
                            - Math.Sqrt(Math.Max(0, (1 + dot) * 0.5))) / denominator;
                        offset = Math.Max(0, offset);
                        corner.Add(new(a.X + incoming.X * offset, a.Y + incoming.Y * offset));
                        corner.Add(new(b.X - outgoing.X * offset, b.Y - outgoing.Y * offset));
                    }
                }
                if (extendToOffset) corner.Add(b);
                if (!Polygon(corner)) return false;
            }
            for (int i = 0; i < segments; i++)
            {
                var b = points[(i + 1) % count];
                var n = normals[i];
                if (!Polygon([starts[i, 0], new(b.X + n.X, b.Y + n.Y),
                    new(b.X - n.X, b.Y - n.Y), starts[i, 1]])) return false;
            }
            if (!figure.Closed)
            {
                for (int end = 0; end < 2; end++)
                {
                    int index = end == 0 ? 0 : segments - 1;
                    var center = points[end == 0 ? 0 : count - 1];
                    var t = (end == 0 ? figure.StartTangent : figure.EndTangent) ?? tangents[index];
                    var n = new MilPoint2D(-t.Y * radius, t.X * radius);
                    double direction = end == 0 ? -1 : 1;
                    var cap = end == 0
                        ? (figure.StartsAtGap ? pen.DashCap : pen.StartLineCap)
                        : (figure.EndsAtGap ? pen.DashCap : pen.EndLineCap);
                    var a = new MilPoint2D(center.X + n.X, center.Y + n.Y);
                    var b = new MilPoint2D(center.X - n.X, center.Y - n.Y);
                    var tip = new MilPoint2D(center.X + t.X * radius * direction, center.Y + t.Y * radius * direction);
                    var outline = new List<MilPoint2D> { a };
                    if (cap == MilPenCap.Flat) continue;
                    if (cap == MilPenCap.Square)
                    {
                        outline.Add(new(tip.X + n.X, tip.Y + n.Y));
                        outline.Add(new(tip.X - n.X, tip.Y - n.Y));
                    }
                    else if (cap == MilPenCap.Triangle) outline.Add(tip);
                    else if (cap == MilPenCap.Round) AppendArc(outline, center, a, b, end == 0 ? 1 : -1, arcTransform);
                    else return false;
                    outline.Add(b);
                    if (!Polygon(outline)) return false;
                }
            }
        }
        foreach (var part in parts)
            coverage = coverage is null ? part : SoftwareImageCoverage.Combine(coverage, part, MilCombineMode.Union);
        if (double.IsPositiveInfinity(left))
        {
            if (createCoverage) coverage = SoftwareImageCoverage.Combine(null, null, MilCombineMode.Union);
            return true;
        }
        bounds = new(left, top, right - left, bottom - top);
        return double.IsFinite(left) && double.IsFinite(top) && double.IsFinite(bounds.Width) && double.IsFinite(bounds.Height);
    }

    private static void AppendArc(List<MilPoint2D> points, MilPoint2D center, MilPoint2D start, MilPoint2D end, int direction, GeneratedImageTransform transform)
    {
        double radius = Math.Sqrt((start.X - center.X) * (start.X - center.X) + (start.Y - center.Y) * (start.Y - center.Y));
        double angle = Math.Atan2(start.Y - center.Y, start.X - center.X);
        double sweep = Math.Atan2(end.Y - center.Y, end.X - center.X) - angle;
        if (direction > 0 && sweep <= 0) sweep += Math.PI * 2;
        if (direction < 0 && sweep >= 0) sweep -= Math.PI * 2;
        int pieces = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) / (Math.PI / 2)));
        for (int i = 0; i < pieces; i++)
        {
            double a = angle + sweep * i / pieces, b = angle + sweep * (i + 1) / pieces;
            double k = 4d / 3 * Math.Tan((b - a) / 4);
            var p = new SoftwareBezierFlattener.Point(center.X + radius * Math.Cos(a), center.Y + radius * Math.Sin(a));
            var q = new SoftwareBezierFlattener.Point(center.X + radius * Math.Cos(b), center.Y + radius * Math.Sin(b));
            foreach (var value in SoftwareBezierFlattener.Flatten(p,
                new(p.X - k * radius * Math.Sin(a), p.Y + k * radius * Math.Cos(a)),
                new(q.X + k * radius * Math.Sin(b), q.Y - k * radius * Math.Cos(b)), q, transform))
                points.Add(new(value.X, value.Y));
        }
    }
}
