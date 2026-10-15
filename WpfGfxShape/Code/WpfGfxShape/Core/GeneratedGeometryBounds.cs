using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal sealed class GeneratedGeometryBounds
{
    private double _left = double.PositiveInfinity;
    private double _top = double.PositiveInfinity;
    private double _right = double.NegativeInfinity;
    private double _bottom = double.NegativeInfinity;

    private GeneratedImageTransform _toleranceTransform;

    internal static bool TryGet(GeneratedProtocolResource resource, GeneratedImageTransform transform, out MilRectD bounds,
        GeneratedImageTransform? toleranceTransform = null)
    {
        var accumulator = new GeneratedGeometryBounds { _toleranceTransform = toleranceTransform ?? transform };
        bool success = accumulator.Append(resource, transform, 0);
        bounds = double.IsPositiveInfinity(accumulator._left) ? default
            : new(accumulator._left, accumulator._top, accumulator._right - accumulator._left, accumulator._bottom - accumulator._top);
        return success && double.IsFinite(bounds.Width) && double.IsFinite(bounds.Height);
    }

    private bool Append(GeneratedProtocolResource resource, GeneratedImageTransform parent, int depth)
    {
        if (depth > 256) return false;
        if (resource is GeneratedLineGeometryResource) return true;
        if (resource is GeneratedCombinedGeometryResource combined)
        {
            if (combined.CombineMode != 0)
            {
                if (AppendCombinedRectangles(combined, parent, depth)) return true;
                if (!SoftwareGroupStrokeContours.TryRead(combined, out var figures,
                    preserveGaps: true, deviceTransform: _toleranceTransform)) return false;
                foreach (var figure in figures)
                    foreach (var point in figure.Points)
                        if (!Point(parent.Apply(point.X, point.Y))) return false;
                return true;
            }
            if (!GeneratedImageTransform.TryResolve(combined.Transform, out var local)) return false;
            var transform = parent.Prepend(local);
            // Union extrema are operand extrema, including after the shared affine transform.
            return (combined.Geometry1 is null || Append(combined.Geometry1, transform, depth + 1))
                && (combined.Geometry2 is null || Append(combined.Geometry2, transform, depth + 1));
        }
        if (resource is GeneratedGeometryGroupResource group)
        {
            if (!GeneratedImageTransform.TryResolve(group.Transform, out var local)) return false;
            foreach (var child in group.Children)
                if (!Append(child, parent.Prepend(local), depth + 1)) return false;
            return true;
        }
        if (resource is GeneratedPathGeometryResource path)
        {
            if (!GeneratedImageTransform.TryResolve(path.Transform, out var local)) return false;
            return AppendPath(path.Figures, parent.Prepend(local));
        }
        if (resource is GeneratedRectangleGeometryResource rectangle)
        {
            if (!GeneratedImageTransform.TryResolve(rectangle.Transform, out var local)) return false;
            var transform = parent.Prepend(local);
            var value = rectangle.CurrentValue;
            var r = value.Rect;
            if (r.Width <= 0 || r.Height <= 0) return true;
            double rx = Math.Min(Math.Abs(value.RadiusX), r.Width / 2), ry = Math.Min(Math.Abs(value.RadiusY), r.Height / 2);
            if (!double.IsFinite(rx) || !double.IsFinite(ry)) return false;
            double l = r.X, t = r.Y, right = l + r.Width, bottom = t + r.Height;
            if (rx == 0 || ry == 0 || (transform.M12 == 0 && transform.M21 == 0))
                return Point(transform.Apply(l, t)) && Point(transform.Apply(right, t))
                    && Point(transform.Apply(right, bottom)) && Point(transform.Apply(l, bottom));
            const double k = 1 - 0.5522847498307933984;
            double bx = rx * k, by = ry * k;
            (double X, double Y)[] points = [(l, t + ry), (l, t + by), (l + bx, t), (l + rx, t),
                (right - rx, t), (right - bx, t), (right, t + by), (right, t + ry),
                (right, bottom - ry), (right, bottom - by), (right - bx, bottom), (right - rx, bottom),
                (l + rx, bottom), (l + bx, bottom), (l, bottom - by), (l, bottom - ry)];
            for (int i = 0; i < points.Length; i += 4)
                if (!Curve(transform.Apply(points[i].X, points[i].Y), transform.Apply(points[i + 1].X, points[i + 1].Y),
                    transform.Apply(points[i + 2].X, points[i + 2].Y), transform.Apply(points[i + 3].X, points[i + 3].Y))) return false;
            return true;
        }
        if (resource is GeneratedEllipseGeometryResource ellipse)
        {
            if (!GeneratedImageTransform.TryResolve(ellipse.Transform, out var local)) return false;
            var transform = parent.Prepend(local);
            var value = ellipse.CurrentValue;
            double x = value.Center.X, y = value.Center.Y, rx = Math.Abs(value.RadiusX), ry = Math.Abs(value.RadiusY);
            if (!double.IsFinite(rx) || !double.IsFinite(ry)) return false;
            if (rx == 0 || ry == 0) return true;
            const double k = 0.5522847498307933984;
            (double X, double Y)[] points = [(x + rx, y), (x + rx, y + ry * k), (x + rx * k, y + ry), (x, y + ry),
                (x - rx * k, y + ry), (x - rx, y + ry * k), (x - rx, y),
                (x - rx, y - ry * k), (x - rx * k, y - ry), (x, y - ry),
                (x + rx * k, y - ry), (x + rx, y - ry * k), (x + rx, y)];
            for (int i = 0; i < 12; i += 3)
                if (!Curve(transform.Apply(points[i].X, points[i].Y), transform.Apply(points[i + 1].X, points[i + 1].Y),
                    transform.Apply(points[i + 2].X, points[i + 2].Y), transform.Apply(points[i + 3].X, points[i + 3].Y))) return false;
            return true;
        }
        return false;
    }

    private bool AppendCombinedRectangles(GeneratedCombinedGeometryResource combined, GeneratedImageTransform parent, int depth)
    {
        if (!GeneratedImageTransform.TryResolve(combined.Transform, out var local)
            || !TryCombinedRegions(combined.Geometry1, depth + 1, out var first)
            || !TryCombinedRegions(combined.Geometry2, depth + 1, out var second)) return false;
        var transform = parent.Prepend(local);
        foreach (var region in CombineRegions(first, second, combined.CombineMode))
            if (!Point(transform.Apply(region.X, region.Y))
                || !Point(transform.Apply(region.X + region.Width, region.Y))
                || !Point(transform.Apply(region.X + region.Width, region.Y + region.Height))
                || !Point(transform.Apply(region.X, region.Y + region.Height))) return false;
        return true;
    }

    private static bool TryCombinedRegions(GeneratedProtocolResource? resource, int depth, out List<MilRectD> regions)
    {
        regions = [];
        if (depth > 256) return false;
        if (resource is GeneratedCombinedGeometryResource combined)
        {
            if (!GeneratedImageTransform.TryResolve(combined.Transform, out var transform)
                || !TryCombinedRegions(combined.Geometry1, depth + 1, out var first)
                || !TryCombinedRegions(combined.Geometry2, depth + 1, out var second)) return false;
            // Only axis-preserving operand transforms keep this exact rectangular representation.
            if ((transform.M12 != 0 || transform.M21 != 0)
                && (transform.ScaleX != 0 || transform.ScaleY != 0)) return false;
            foreach (var region in CombineRegions(first, second, combined.CombineMode))
            {
                var a = transform.Apply(region.X, region.Y);
                var b = transform.Apply(region.X + region.Width, region.Y + region.Height);
                if (!double.IsFinite(a.X) || !double.IsFinite(a.Y) || !double.IsFinite(b.X) || !double.IsFinite(b.Y)) return false;
                var transformed = new MilRectD(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
                if (!double.IsFinite(transformed.Width) || !double.IsFinite(transformed.Height)) return false;
                if (transformed.Width > 0 && transformed.Height > 0) regions.Add(transformed);
            }
            return true;
        }
        if (!TryCombinedRectangle(resource, GeneratedImageTransform.Identity, out var rectangle)) return false;
        if (rectangle.Width > 0 && rectangle.Height > 0) regions.Add(rectangle);
        return true;
    }

    private static List<MilRectD> CombineRegions(List<MilRectD> first, List<MilRectD> second, MilCombineMode mode)
    {
        var xs = new SortedSet<double>();
        var ys = new SortedSet<double>();
        foreach (var region in first.Concat(second))
        {
            xs.Add(region.X); xs.Add(region.X + region.Width);
            ys.Add(region.Y); ys.Add(region.Y + region.Height);
        }
        double[] xEdges = xs.ToArray(), yEdges = ys.ToArray();
        List<MilRectD> result = [];
        for (int x = 0; x < xEdges.Length - 1; x++)
        for (int y = 0; y < yEdges.Length - 1; y++)
        {
            double mx = xEdges[x] / 2 + xEdges[x + 1] / 2, my = yEdges[y] / 2 + yEdges[y + 1] / 2;
            bool a = first.Exists(region => Contains(region, mx, my));
            bool b = second.Exists(region => Contains(region, mx, my));
            bool included = mode switch
            {
                MilCombineMode.Union => a || b,
                MilCombineMode.Intersect => a && b,
                MilCombineMode.Xor => a != b,
                MilCombineMode.Exclude => a && !b,
                _ => false
            };
            if (included) result.Add(new(xEdges[x], yEdges[y], xEdges[x + 1] - xEdges[x], yEdges[y + 1] - yEdges[y]));
        }
        return result;
    }

    private static bool Contains(MilRectD rectangle, double x, double y)
        => rectangle.Width > 0 && rectangle.Height > 0 && x > rectangle.X && y > rectangle.Y
            && x < rectangle.X + rectangle.Width && y < rectangle.Y + rectangle.Height;

    private static bool TryCombinedRectangle(GeneratedProtocolResource? resource, GeneratedImageTransform parent, out MilRectD bounds)
    {
        bounds = default;
        // An open line has no fill area, including when used as a Boolean operand.
        if (resource is null or GeneratedLineGeometryResource) return true;
        // Native GetShapeDataCore returns EmptyShape before the first figures update.
        if (resource is GeneratedPathGeometryResource { Figures.Length: 0 }) return true;
        if (resource is not GeneratedRectangleGeometryResource rectangle
            || !GeneratedImageTransform.TryResolve(rectangle.Transform, out var local)) return false;
        var value = rectangle.CurrentValue;
        if (value.RadiusX != 0 && value.RadiusY != 0) return false;
        var transform = parent.Prepend(local);
        // Axis swaps preserve rectangles just as diagonal scales do; shear does not.
        if ((transform.M12 != 0 || transform.M21 != 0)
            && (transform.ScaleX != 0 || transform.ScaleY != 0)) return false;
        var r = value.Rect;
        if (!double.IsFinite(r.X) || !double.IsFinite(r.Y) || !double.IsFinite(r.Width) || !double.IsFinite(r.Height)) return false;
        if (r.Width <= 0 || r.Height <= 0) return true;
        var a = transform.Apply(r.X, r.Y);
        var b = transform.Apply(r.X + r.Width, r.Y + r.Height);
        if (!double.IsFinite(a.X) || !double.IsFinite(a.Y) || !double.IsFinite(b.X) || !double.IsFinite(b.Y)) return false;
        bounds = new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
        return double.IsFinite(bounds.Width) && double.IsFinite(bounds.Height);
    }

    private bool AppendPath(ReadOnlySpan<byte> data, GeneratedImageTransform transform)
    {
        if (data.IsEmpty) return true;
        if (!MilPathGeometryValidator.IsValid(data)) return false;
        for (int offset = 48; offset < data.Length;)
        {
            var figure = data[offset..];
            int size = checked((int)MemoryMarshal.Read<uint>(figure[12..]));
            if ((MemoryMarshal.Read<uint>(figure[4..]) & 8) != 0)
            {
                var current = MemoryMarshal.Read<MilPoint2D>(figure[16..]);
                if (!Point(transform.Apply(current.X, current.Y))) return false;
                for (int segment = 40; segment < size;)
                {
                    var record = figure[segment..];
                    var type = (MilSegmentType)MemoryMarshal.Read<uint>(record);
                    if (type == MilSegmentType.Arc)
                    {
                        var end = MemoryMarshal.Read<MilPoint2D>(record[16..]);
                        if (!SoftwareArcConverter.TryConvert(current, end, MemoryMarshal.Read<MilSizeD>(record[32..]),
                            MemoryMarshal.Read<double>(record[48..]), MemoryMarshal.Read<uint>(record[12..]) != 0,
                            MemoryMarshal.Read<uint>(record[56..]) != 0, out var arc, out bool line)) return false;
                        if (line || arc.Length == 0)
                        {
                            if (!Point(transform.Apply(end.X, end.Y))) return false;
                        }
                        for (int i = 0; i < arc.Length; i += 3)
                        {
                            if (!Curve(transform.Apply(current.X, current.Y), transform.Apply(arc[i].X, arc[i].Y),
                                transform.Apply(arc[i + 1].X, arc[i + 1].Y), transform.Apply(arc[i + 2].X, arc[i + 2].Y))) return false;
                            current = arc[i + 2];
                        }
                        current = end; segment += 64; continue;
                    }
                    int arity = type switch
                    {
                        MilSegmentType.Line or MilSegmentType.PolyLine => 1,
                        MilSegmentType.QuadraticBezier or MilSegmentType.PolyQuadraticBezier => 2,
                        MilSegmentType.Bezier or MilSegmentType.PolyBezier => 3,
                        _ => 0
                    };
                    if (arity == 0) return false;
                    int count = type is MilSegmentType.PolyLine or MilSegmentType.PolyBezier or MilSegmentType.PolyQuadraticBezier
                        ? checked((int)MemoryMarshal.Read<uint>(record[12..])) : arity;
                    for (int i = 0; i < count; i += arity)
                    {
                        var end = MemoryMarshal.Read<MilPoint2D>(record[(16 + (i + arity - 1) * 16)..]);
                        if (arity == 1)
                        {
                            if (!Point(transform.Apply(end.X, end.Y))) return false;
                        }
                        else
                        {
                            var control = MemoryMarshal.Read<MilPoint2D>(record[(16 + i * 16)..]);
                            var a = transform.Apply(current.X, current.Y); var b = transform.Apply(control.X, control.Y);
                            var d = transform.Apply(end.X, end.Y);
                            (double X, double Y) c;
                            if (arity == 2)
                            {
                                c = (d.X + (b.X - d.X) * (2d / 3), d.Y + (b.Y - d.Y) * (2d / 3));
                                b = (a.X + (b.X - a.X) * (2d / 3), a.Y + (b.Y - a.Y) * (2d / 3));
                            }
                            else
                            {
                                control = MemoryMarshal.Read<MilPoint2D>(record[(32 + i * 16)..]);
                                c = transform.Apply(control.X, control.Y);
                            }
                            if (!Curve(a, b, c, d)) return false;
                        }
                        current = end;
                    }
                    segment += checked(16 + count * 16);
                }
            }
            offset += size;
        }
        return true;
    }

    private bool Point((double X, double Y) point)
    {
        if (!double.IsFinite(point.X) || !double.IsFinite(point.Y)) return false;
        _left = Math.Min(_left, point.X); _right = Math.Max(_right, point.X);
        _top = Math.Min(_top, point.Y); _bottom = Math.Max(_bottom, point.Y);
        return true;
    }

    private bool Curve((double X, double Y) a, (double X, double Y) b, (double X, double Y) c, (double X, double Y) d)
    {
        if (!Point(a) || !Point(d) || !double.IsFinite(b.X) || !double.IsFinite(b.Y)
            || !double.IsFinite(c.X) || !double.IsFinite(c.Y)) return false;
        return Extrema(a.X, b.X, c.X, d.X, true) && Extrema(a.Y, b.Y, c.Y, d.Y, false);
    }

    private bool Extrema(double a, double b, double c, double d, bool horizontal)
    {
        // CBounds uses positive roots in s-space to avoid unstable endpoint subtraction.
        if ((b - a) * (d - b) >= 0 && (c - a) * (d - c) >= 0) return true;
        double first = b - a, middle = c - b, last = d - c;
        if (Math.Abs(first) < Math.Abs(middle) * 1e-6 && Math.Abs(last) < Math.Abs(middle) * 1e-6) return true;
        bool reverse = Math.Abs(first) > Math.Abs(last);
        double coefficient = reverse ? first : last, constant = reverse ? last : first;
        double discriminant = middle * middle - coefficient * constant;
        if (!double.IsFinite(discriminant)) return false;
        if (discriminant <= 0) return true;
        double root = Math.Sqrt(discriminant);
        for (int sign = -1; sign <= 1; sign += 2)
        {
            double s = (-middle + sign * root) / coefficient;
            if (!double.IsFinite(s)) return false;
            if (s <= 0) continue;
            double t = reverse ? 1 / (1 + s) : s / (1 + s), u = 1 - t;
            double value = a * u * u * u + 3 * b * t * u * u + 3 * c * u * t * t + d * t * t * t;
            if (!double.IsFinite(value)) return false;
            if (horizontal) { _left = Math.Min(_left, value); _right = Math.Max(_right, value); }
            else { _top = Math.Min(_top, value); _bottom = Math.Max(_bottom, value); }
        }
        return true;
    }
}
