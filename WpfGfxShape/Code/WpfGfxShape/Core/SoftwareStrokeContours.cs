using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static class SoftwareStrokeContours
{
    internal readonly record struct Figure(MilPoint2D[] Points, bool Closed,
        bool StartsAtGap = false, bool EndsAtGap = false, bool[]? SmoothJoins = null,
        MilPoint2D? PointTangent = null, bool Fillable = true, bool[]? SegmentGaps = null,
        Segment[]? Segments = null, MilPoint2D? StartTangent = null, MilPoint2D? EndTangent = null);

    internal readonly record struct Segment(MilPoint2D[] Points, bool Smooth, bool Gap);

    internal static bool TryRead(GeneratedPathGeometryResource path, out List<Figure> figures, bool preserveGaps = false,
        bool allowNonFinite = false, bool fillLocalFloat = false)
    {
        MilPoint2D ApplyFillLocal(GeneratedImageTransform matrix, MilPoint2D value)
        {
            float x = (float)value.X, y = (float)value.Y;
            return new(x * (float)matrix.ScaleX + y * (float)matrix.M21 + (float)matrix.X,
                x * (float)matrix.M12 + y * (float)matrix.ScaleY + (float)matrix.Y);
        }
        bool Add(List<MilPoint2D> points, GeneratedImageTransform matrix, MilPoint2D value)
        {
            if (!fillLocalFloat) return SoftwareStrokeContours.Add(points, matrix, value, allowNonFinite);
            var next = ApplyFillLocal(matrix, value);
            if (!allowNonFinite && (!float.IsFinite((float)next.X) || !float.IsFinite((float)next.Y))) return false;
            if (points.Count == 0 || points[^1] != next) points.Add(next);
            return true;
        }
        figures = [];
        if (!GeneratedImageTransform.TryResolve(path.Transform, out var transform)) return false;
        if (path.Figures.Length == 0) return true;
        ReadOnlySpan<byte> data = path.Figures;
        if (!MilPathGeometryValidator.IsValid(data)) return false;
        for (int offset = 48; offset < data.Length;)
        {
            var figure = data[offset..];
            int size = checked((int)MemoryMarshal.Read<uint>(figure[12..]));
            var current = MemoryMarshal.Read<MilPoint2D>(figure[16..]);
            var points = new List<MilPoint2D>();
            var smoothJoins = new List<bool>();
            var segmentGaps = new List<bool>();
            var rawSegments = new List<Segment>();
            bool CaptureSegment(MilPoint2D[] values, bool smooth, bool gap)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var p = transform.Apply(values[i].X, values[i].Y);
                    if (fillLocalFloat)
                    {
                        var local = ApplyFillLocal(transform, values[i]);
                        p = (local.X, local.Y);
                    }
                    if (!allowNonFinite && (!float.IsFinite((float)p.X) || !float.IsFinite((float)p.Y))) return false;
                    values[i] = new((float)p.X, (float)p.Y);
                }
                rawSegments.Add(new(values, smooth, gap));
                return true;
            }
            var runs = new List<Figure>();
            bool startsAtGap = false, hasGaps = false;
            bool closed = (MemoryMarshal.Read<uint>(figure[4..]) & 4) != 0;
            if (!Add(points, transform, current)) return false;
            smoothJoins.Add(false);
            for (int segment = 40; segment < size;)
            {
                var record = figure[segment..];
                var type = (MilSegmentType)MemoryMarshal.Read<uint>(record);
                uint flags = MemoryMarshal.Read<uint>(record[4..]);
                bool gap = (flags & 4) != 0;
                if (gap && !preserveGaps)
                {
                    hasGaps = true;
                    if (points.Count > 1)
                        runs.Add(new(points.ToArray(), false, startsAtGap, true, smoothJoins.ToArray()));
                    points.Clear();
                    smoothJoins.Clear();
                    if (!Add(points, transform, current)) return false;
                    smoothJoins.Add(false);
                    startsAtGap = true;
                }
                void CompleteSegment()
                {
                    while (smoothJoins.Count < points.Count)
                        smoothJoins.Add(type is not (MilSegmentType.Line or MilSegmentType.PolyLine));
                    smoothJoins[^1] = (flags & 8) != 0;
                    if (preserveGaps)
                    {
                        while (segmentGaps.Count < points.Count - 1) segmentGaps.Add(gap);
                        return;
                    }
                    if (!gap) return;
                    var end = points[^1];
                    points.Clear(); points.Add(end);
                    smoothJoins.Clear(); smoothJoins.Add(false);
                }
                if (type == MilSegmentType.Arc)
                {
                    var end = MemoryMarshal.Read<MilPoint2D>(record[16..]);
                    if (!SoftwareArcConverter.TryConvert(current, end,
                        MemoryMarshal.Read<MilSizeD>(record[32..]), MemoryMarshal.Read<double>(record[48..]),
                        MemoryMarshal.Read<uint>(record[12..]) != 0, MemoryMarshal.Read<uint>(record[56..]) != 0,
                        out var arc, out bool line)) return false;
                    if (line || arc.Length == 0)
                    {
                        if (!Add(points, transform, end)
                            || !CaptureSegment([end], (flags & 8) != 0, gap)) return false;
                    }
                    for (int i = 0; i < arc.Length; i += 3)
                    {
                        if (!(preserveGaps ? Add(points, transform, arc[i + 2]) : Curve(points, transform, current, arc[i], arc[i + 1], arc[i + 2]))
                            || !CaptureSegment([arc[i], arc[i + 1], arc[i + 2]], i + 3 < arc.Length || (flags & 8) != 0, gap)) return false;
                        current = arc[i + 2];
                    }
                    current = end;
                    CompleteSegment();
                    segment += 64; continue;
                }
                int arity = type is MilSegmentType.QuadraticBezier or MilSegmentType.PolyQuadraticBezier ? 2 : 3;
                int count = type switch
                {
                    MilSegmentType.Line => 1,
                    MilSegmentType.Bezier => 3,
                    MilSegmentType.QuadraticBezier => 2,
                    MilSegmentType.PolyLine or MilSegmentType.PolyBezier or MilSegmentType.PolyQuadraticBezier
                        => checked((int)MemoryMarshal.Read<uint>(record[12..])),
                    _ => -1
                };
                if (count < 0) return false;
                if (type is MilSegmentType.Line or MilSegmentType.PolyLine)
                {
                    for (int i = 0; i < count; i++)
                    {
                        current = MemoryMarshal.Read<MilPoint2D>(record[(16 + i * 16)..]);
                        if (!Add(points, transform, current)
                            || !CaptureSegment([current], i + 1 == count && (flags & 8) != 0, gap)) return false;
                    }
                }
                else
                {
                    for (int i = 0; i < count; i += arity)
                    {
                        var a = MemoryMarshal.Read<MilPoint2D>(record[(16 + i * 16)..]);
                        var b = MemoryMarshal.Read<MilPoint2D>(record[(32 + i * 16)..]);
                        var end = arity == 2 ? b : MemoryMarshal.Read<MilPoint2D>(record[(48 + i * 16)..]);
                        if (arity == 2)
                        {
                            b = new(end.X + (a.X - end.X) * (2d / 3), end.Y + (a.Y - end.Y) * (2d / 3));
                            a = new(current.X + (a.X - current.X) * (2d / 3), current.Y + (a.Y - current.Y) * (2d / 3));
                        }
                        if (!(preserveGaps ? Add(points, transform, end) : Curve(points, transform, current, a, b, end))
                            || !CaptureSegment([a, b, end], i + arity == count && (flags & 8) != 0, gap)) return false;
                        current = end;
                    }
                }
                CompleteSegment();
                segment += 16 + count * 16;
            }
            if (!hasGaps)
                figures.Add(new(points.ToArray(), closed, SmoothJoins: smoothJoins.ToArray(),
                    Fillable: (MemoryMarshal.Read<uint>(figure[4..]) & 8) != 0,
                    SegmentGaps: preserveGaps ? segmentGaps.ToArray() : null,
                    Segments: preserveGaps ? rawSegments.ToArray() : null));
            else
            {
                if (closed)
                {
                    var start = MemoryMarshal.Read<MilPoint2D>(figure[16..]);
                    if (!Add(points, transform, start)) return false;
                    while (smoothJoins.Count < points.Count) smoothJoins.Add(false);
                }
                if (points.Count > 1)
                    runs.Add(new(points.ToArray(), false, startsAtGap, closed, smoothJoins.ToArray()));
                if (closed && runs.Count > 1 && !runs[0].StartsAtGap
                    && runs[^1].Points[^1] == runs[0].Points[0])
                {
                    var first = runs[0]; var last = runs[^1];
                    runs[0] = new(last.Points.Concat(first.Points.Skip(1)).ToArray(), false,
                        last.StartsAtGap, first.EndsAtGap,
                        last.SmoothJoins!.Concat(first.SmoothJoins!.Skip(1)).ToArray());
                    runs.RemoveAt(runs.Count - 1);
                }
                figures.AddRange(runs);
            }
            offset += size;
        }
        return true;
    }

    private static bool Add(List<MilPoint2D> points, GeneratedImageTransform transform, MilPoint2D value,
        bool allowNonFinite = false)
    {
        var p = transform.Apply(value.X, value.Y);
        // GetShapeDataCore stores REAL coordinates before widening; do not raster-quantize the centerline.
        var next = new MilPoint2D((float)p.X, (float)p.Y);
        if (!allowNonFinite && (!float.IsFinite((float)p.X) || !float.IsFinite((float)p.Y))) return false;
        if (points.Count == 0 || points[^1] != next) points.Add(next);
        return true;
    }

    private static bool Curve(List<MilPoint2D> points, GeneratedImageTransform transform,
        MilPoint2D start, MilPoint2D a, MilPoint2D b, MilPoint2D end)
    {
        var s = transform.Apply(start.X, start.Y); var p = transform.Apply(a.X, a.Y);
        var q = transform.Apply(b.X, b.Y); var e = transform.Apply(end.X, end.Y);
        if (!double.IsFinite(s.X) || !double.IsFinite(s.Y) || !double.IsFinite(p.X) || !double.IsFinite(p.Y)
            || !double.IsFinite(q.X) || !double.IsFinite(q.Y) || !double.IsFinite(e.X) || !double.IsFinite(e.Y)) return false;
        foreach (var point in SoftwareBezierFlattener.Flatten(new(s.X, s.Y), new(p.X, p.Y), new(q.X, q.Y), new(e.X, e.Y)))
            if (!Add(points, GeneratedImageTransform.Identity, new(point.X, point.Y))) return false;
        return true;
    }
}
