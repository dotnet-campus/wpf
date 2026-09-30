using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal sealed class SoftwarePathCoverage
{
    private readonly List<((long X, long Y) A, (long X, long Y) B)> _edges = [];
    private readonly MilFillMode _fillRule;

    private SoftwarePathCoverage(MilFillMode fillRule) => _fillRule = fillRule;

    internal static bool TryCreate(GeneratedPathGeometryResource path, GeneratedImageTransform parent, out SoftwarePathCoverage? result)
    {
        result = null;
        // Native GetShapeDataCore exposes EmptyShape before the first figures update.
        if (path.Figures.Length == 0)
        {
            result = new(path.FillRule);
            return true;
        }
        if (!GeneratedImageTransform.TryResolve(path.Transform, out var local)) return false;
        var transform = parent.Prepend(local);
        var coverage = new SoftwarePathCoverage(path.FillRule);
        ReadOnlySpan<byte> data = path.Figures;
        if (!MilPathGeometryValidator.IsValid(data)) return false;
        int offset = 48;
        while (offset < data.Length)
        {
            var figure = data[offset..];
            int size = checked((int)MemoryMarshal.Read<uint>(figure[12..]));
            if ((MemoryMarshal.Read<uint>(figure[4..]) & 8) != 0)
            {
                if (!Point(figure[16..], transform, out var start)) return false;
                var previous = start;
                var currentPoint = MemoryMarshal.Read<MilPoint2D>(figure[16..]);
                int segment = 40;
                while (segment < size)
                {
                    var record = figure[segment..];
                    var type = (MilSegmentType)MemoryMarshal.Read<uint>(record);
                    int count;
                    int bytes;
                    if (type == MilSegmentType.Arc)
                    {
                        var end = MemoryMarshal.Read<MilPoint2D>(record[16..]);
                        if (!SoftwareArcConverter.TryConvert(currentPoint, end,
                            MemoryMarshal.Read<MilSizeD>(record[32..]), MemoryMarshal.Read<double>(record[48..]),
                            MemoryMarshal.Read<uint>(record[12..]) != 0, MemoryMarshal.Read<uint>(record[56..]) != 0,
                            out var arc, out bool line)) return false;
                        // PathFigureData::SetArcData treats every non-positive piece count as a line.
                        if (line || arc.Length == 0)
                        {
                            if (!Point(record[16..], transform, out var next)) return false;
                            coverage._edges.Add((previous, next)); previous = next;
                        }
                        for (int i = 0; i < arc.Length; i += 3)
                        {
                            var a = transform.Apply(currentPoint.X, currentPoint.Y);
                            var b = transform.Apply(arc[i].X, arc[i].Y);
                            var c = transform.Apply(arc[i + 1].X, arc[i + 1].Y);
                            var d = transform.Apply(arc[i + 2].X, arc[i + 2].Y);
                            if (!Quantize(a.X, a.Y, out _) || !Quantize(b.X, b.Y, out _)
                                || !Quantize(c.X, c.Y, out _) || !Quantize(d.X, d.Y, out _)) return false;
                            foreach (var p in SoftwareBezierFlattener.Flatten(new(a.X, a.Y), new(b.X, b.Y), new(c.X, c.Y), new(d.X, d.Y)))
                            {
                                if (!Quantize(p.X, p.Y, out var next)) return false;
                                coverage._edges.Add((previous, next)); previous = next;
                            }
                            currentPoint = arc[i + 2];
                        }
                        currentPoint = end;
                        segment += 64;
                        continue;
                    }
                    if (type == MilSegmentType.Line) { count = 1; bytes = 32; }
                    else if (type == MilSegmentType.PolyLine)
                    {
                        count = checked((int)MemoryMarshal.Read<uint>(record[12..]));
                        bytes = checked(16 + count * 16);
                    }
                    else if (type is MilSegmentType.Bezier or MilSegmentType.QuadraticBezier
                        or MilSegmentType.PolyBezier or MilSegmentType.PolyQuadraticBezier)
                    {
                        int arity = type is MilSegmentType.QuadraticBezier or MilSegmentType.PolyQuadraticBezier ? 2 : 3;
                        int pointCount = type is MilSegmentType.PolyBezier or MilSegmentType.PolyQuadraticBezier
                            ? checked((int)MemoryMarshal.Read<uint>(record[12..])) : arity;
                        var controls = new SoftwareBezierFlattener.Point[3];
                        for (int curve = 0; curve < pointCount; curve += arity)
                        {
                            var transformedStart = transform.Apply(currentPoint.X, currentPoint.Y);
                            var startPoint = new SoftwareBezierFlattener.Point(transformedStart.X, transformedStart.Y);
                            for (int i = 0; i < arity; i++)
                            {
                                var value = MemoryMarshal.Read<MilPoint2D>(record[(16 + (curve + i) * 16)..]);
                                var p = transform.Apply(value.X, value.Y);
                                if (!double.IsFinite(p.X) || !double.IsFinite(p.Y)
                                    || Math.Abs(p.X) > 524287 || Math.Abs(p.Y) > 524287) return false;
                                controls[i] = new(p.X, p.Y);
                            }
                            if (arity == 2)
                            {
                                controls[2] = controls[1];
                                controls[1] = controls[2] + (controls[0] - controls[2]) * (2.0 / 3);
                                controls[0] = startPoint + (controls[0] - startPoint) * (2.0 / 3);
                            }
                            foreach (var p in SoftwareBezierFlattener.Flatten(startPoint, controls[0], controls[1], controls[2]))
                            {
                                if (!Quantize(p.X, p.Y, out var next)) return false;
                                coverage._edges.Add((previous, next));
                                previous = next;
                            }
                            currentPoint = MemoryMarshal.Read<MilPoint2D>(record[(16 + (curve + arity - 1) * 16)..]);
                        }
                        segment += checked(16 + pointCount * 16);
                        continue;
                    }
                    else return false;
                    for (int i = 0; i < count; i++)
                    {
                        currentPoint = MemoryMarshal.Read<MilPoint2D>(record[(16 + i * 16)..]);
                        if (!Point(record[(16 + i * 16)..], transform, out var next)) return false;
                        coverage._edges.Add((previous, next));
                        previous = next;
                    }
                    segment += bytes;
                }
                coverage._edges.Add((previous, start));
            }
            offset += size;
        }
        result = coverage;
        return true;
    }

    internal static bool TryCreateGroup(GeneratedGeometryGroupResource group, GeneratedImageTransform parent,
        out SoftwarePathCoverage? result)
    {
        var coverage = new SoftwarePathCoverage(group.FillRule);
        result = null;
        if (!coverage.AppendGeometry(group, parent, 0)) return false;
        result = coverage;
        return true;
    }

    internal static bool TryCreateEllipse(GeneratedEllipseGeometryResource ellipse, GeneratedImageTransform parent,
        out SoftwarePathCoverage? result)
    {
        result = null;
        var coverage = new SoftwarePathCoverage(MilFillMode.Winding);
        if (!coverage.AppendEllipse(ellipse, parent)) return false;
        result = coverage;
        return true;
    }

    private bool AppendEllipse(GeneratedEllipseGeometryResource ellipse, GeneratedImageTransform parent)
    {
        var value = ellipse.CurrentValue;
        if (!GeneratedImageTransform.TryResolve(ellipse.Transform, out var local)) return false;
        var transform = parent.Prepend(local);
        double x = value.Center.X, y = value.Center.Y;
        double rx = Math.Abs(value.RadiusX), ry = Math.Abs(value.RadiusY);
        if (!double.IsFinite(x) || !double.IsFinite(y) || !double.IsFinite(rx) || !double.IsFinite(ry)) return false;
        if (rx == 0 || ry == 0) return true;
        const double k = 0.5522847498307933984;
        (double X, double Y)[] points = [
            (x + rx, y), (x + rx, y + ry * k), (x + rx * k, y + ry), (x, y + ry),
            (x - rx * k, y + ry), (x - rx, y + ry * k), (x - rx, y),
            (x - rx, y - ry * k), (x - rx * k, y - ry), (x, y - ry),
            (x + rx * k, y - ry), (x + rx, y - ry * k), (x + rx, y)];
        var controls = new SoftwareBezierFlattener.Point[13];
        for (int i = 0; i < points.Length; i++)
        {
            var p = transform.Apply(points[i].X, points[i].Y);
            if (!Quantize(p.X, p.Y, out _)) return false;
            controls[i] = new(p.X, p.Y);
        }
        if (!Quantize(controls[0].X, controls[0].Y, out var previous)) return false;
        for (int i = 0; i < 12; i += 3)
        {
            foreach (var p in SoftwareBezierFlattener.Flatten(controls[i], controls[i + 1], controls[i + 2], controls[i + 3]))
            {
                if (!Quantize(p.X, p.Y, out var next)) return false;
                _edges.Add((previous, next));
                previous = next;
            }
        }
        return true;
    }

    internal static bool TryCreateRoundedRectangle(GeneratedRectangleGeometryResource rectangle,
        GeneratedImageTransform parent, out SoftwarePathCoverage? result)
    {
        result = null;
        var coverage = new SoftwarePathCoverage(MilFillMode.Winding);
        if (!coverage.AppendRoundedRectangle(rectangle, parent)) return false;
        result = coverage;
        return true;
    }

    private bool AppendRoundedRectangle(GeneratedRectangleGeometryResource rectangle, GeneratedImageTransform parent)
    {
        var value = rectangle.CurrentValue;
        var r = value.Rect;
        if (!GeneratedImageTransform.TryResolve(rectangle.Transform, out var local)) return false;
        if (r.Width <= 0 || r.Height <= 0) return true;
        if (!double.IsFinite(value.RadiusX) || !double.IsFinite(value.RadiusY)) return false;
        double rx = Math.Min(Math.Abs(value.RadiusX), r.Width / 2);
        double ry = Math.Min(Math.Abs(value.RadiusY), r.Height / 2);
        double l = r.X, t = r.Y, right = l + r.Width, bottom = t + r.Height;
        const double k = 1 - 0.5522847498307933984;
        double bx = rx * k, by = ry * k;
        (double X, double Y)[] points = [
            (l, t + ry), (l, t + by), (l + bx, t), (l + rx, t),
            (right - rx, t), (right - bx, t), (right, t + by), (right, t + ry),
            (right, bottom - ry), (right, bottom - by), (right - bx, bottom), (right - rx, bottom),
            (l + rx, bottom), (l + bx, bottom), (l, bottom - by), (l, bottom - ry)];
        var transform = parent.Prepend(local);
        var controls = new SoftwareBezierFlattener.Point[16];
        for (int i = 0; i < controls.Length; i++)
        {
            var p = transform.Apply(points[i].X, points[i].Y);
            if (!Quantize(p.X, p.Y, out _)) return false;
            controls[i] = new(p.X, p.Y);
        }
        if (!Quantize(controls[0].X, controls[0].Y, out var previous)) return false;
        for (int i = 0; i < 16; i += 4)
        {
            foreach (var p in SoftwareBezierFlattener.Flatten(controls[i], controls[i + 1], controls[i + 2], controls[i + 3]))
            {
                if (!Quantize(p.X, p.Y, out var next)) return false;
                _edges.Add((previous, next));
                previous = next;
            }
            var end = controls[(i + 4) % 16];
            if (!Quantize(end.X, end.Y, out var lineEnd)) return false;
            _edges.Add((previous, lineEnd));
            previous = lineEnd;
        }
        return true;
    }

    private bool AppendGeometry(GeneratedProtocolResource resource, GeneratedImageTransform parent, int depth)
    {
        if (depth > 256) return false;
        if (resource is GeneratedLineGeometryResource line)
            return GeneratedImageTransform.TryResolve(line.Transform, out _);
        if (resource is GeneratedEllipseGeometryResource ellipse) return AppendEllipse(ellipse, parent);
        if (resource is GeneratedGeometryGroupResource group)
        {
            if (!GeneratedImageTransform.TryResolve(group.Transform, out var local)) return false;
            foreach (var child in group.Children)
                if (!AppendGeometry(child, parent.Prepend(local), depth + 1)) return false;
            return true;
        }
        if (resource is GeneratedPathGeometryResource path)
        {
            if (!TryCreate(path, parent, out var coverage) || coverage is null) return false;
            _edges.AddRange(coverage._edges);
            return true;
        }
        if (resource is GeneratedRectangleGeometryResource rectangle)
        {
            var value = rectangle.CurrentValue;
            if (value.RadiusX != 0 || value.RadiusY != 0) return AppendRoundedRectangle(rectangle, parent);
            if (!GeneratedImageTransform.TryResolve(rectangle.Transform, out var local)) return false;
            var r = value.Rect;
            if (r.Width <= 0 || r.Height <= 0) return true;
            var transform = parent.Prepend(local);
            MilPoint2D[] corners = [new(r.X, r.Y), new(r.X + r.Width, r.Y),
                new(r.X + r.Width, r.Y + r.Height), new(r.X, r.Y + r.Height)];
            var points = new (long X, long Y)[4];
            for (int i = 0; i < 4; i++)
                if (!Point(MemoryMarshal.AsBytes(corners.AsSpan(i, 1)), transform, out points[i])) return false;
            for (int i = 0; i < 4; i++) _edges.Add((points[i], points[(i + 1) % 4]));
            return true;
        }
        return false;
    }

    private static bool Point(ReadOnlySpan<byte> data, GeneratedImageTransform transform, out (long X, long Y) point)
    {
        var value = MemoryMarshal.Read<MilPoint2D>(data);
        var transformed = transform.Apply(value.X, value.Y);
        return Quantize(transformed.X, transformed.Y, out point);
    }

    private static bool Quantize(double deviceX, double deviceY, out (long X, long Y) point)
    {
        double x = (deviceX - 0.5) * 16, y = (deviceY - 0.5) * 16;
        point = default;
        if (!double.IsFinite(x) || !double.IsFinite(y) || Math.Abs(x) > 8388608 || Math.Abs(y) > 8388608) return false;
        point = (((long)Math.Round(x) + 8) * 8, ((long)Math.Round(y) + 8) * 8);
        return true;
    }

    internal ulong GetSamples(int x, int y)
    {
        ulong samples = 0;
        Span<int> winding = stackalloc int[8];
        for (int row = 0; row < 8; row++)
        {
            winding.Clear();
            long scanY = ((long)y * 8 + row) * 16;
            foreach (var edge in _edges)
            {
                var a = edge.A; var b = edge.B;
                int direction = 1;
                if (a.Y > b.Y) { (a, b) = (b, a); direction = -1; }
                if (scanY < a.Y || scanY >= b.Y) continue;
                long denominator = (b.Y - a.Y) * 16;
                long numerator = a.X * (b.Y - a.Y) + (scanY - a.Y) * (b.X - a.X);
                long intersection = numerator / denominator;
                if (numerator % denominator > 0) intersection++;
                for (int column = 0; column < 8; column++)
                    if ((long)x * 8 + column >= intersection) winding[column] += direction;
            }
            for (int column = 0; column < 8; column++)
                if (_fillRule == MilFillMode.Alternate ? (winding[column] & 1) != 0 : winding[column] != 0)
                    samples |= 1UL << (row * 8 + column);
        }
        return samples;
    }
}
