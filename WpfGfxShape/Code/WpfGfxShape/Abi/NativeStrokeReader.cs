using System.Numerics;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class NativeStrokeReader
{
    internal static int Widen(nint shape, nint pen, Matrix4x4 matrix, out SoftwareImageCoverage? coverage)
        => Widen(shape, pen, matrix, out coverage, out _);

    internal static int Widen(nint shape, nint pen, Matrix4x4 matrix, out SoftwareImageCoverage? coverage,
        out MilRectD bounds, bool createCoverage = true, GeneratedImageTransform? toleranceTransform = null,
        Direct3D9SurfaceRect? clip = null)
    {
        coverage = null;
        bounds = default;
        if (shape == 0 || pen == 0) return Direct3D9Factory.InvalidArgumentHResult;
        float* geometry = (float*)pen;
        float width = geometry[0], height = geometry[1], angle = geometry[2];
        if (!float.IsFinite(width) || !float.IsFinite(height) || !float.IsFinite(angle))
            return Direct3D9Factory.InvalidArgumentHResult;
        bool elliptical = MathF.Abs(width) != MathF.Abs(height) || angle != 0;
        GeneratedImageTransform penToShape = GeneratedImageTransform.Identity;
        GeneratedImageTransform shapeToPen = GeneratedImageTransform.Identity;
        if (elliptical)
        {
            if (width == 0 || height == 0) return Direct3D9Factory.NotImplementedHResult;
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            penToShape = new(MathF.Abs(width) * cos, MathF.Abs(height) * cos, 0, 0,
                MathF.Abs(width) * sin, -MathF.Abs(height) * sin);
            if (!penToShape.TryInvert(out shapeToPen)) return Direct3D9Factory.InvalidArgumentHResult;
        }
        uint* caps = (uint*)(geometry + 3);
        if (caps[0] > 3 || caps[1] > 3 || caps[2] > 3 || caps[3] > 3)
            return Direct3D9Factory.InvalidArgumentHResult;
        // Release/x64 CPlainPen: CPenGeometry, style/offset, DynArray<REAL>, markers.
        if (sizeof(nint) != 8) return Direct3D9Factory.NotImplementedHResult;
        byte* native = (byte*)pen;
        if (*(uint*)(native + 72) != 0 || *(uint*)(native + 76) != 0
            || *(nint*)(native + 80) != 0 || *(nint*)(native + 88) != 0)
            return Direct3D9Factory.NotImplementedHResult;
        uint dashMode = *(uint*)(native + 32);
        if (dashMode > 5) return Direct3D9Factory.InvalidArgumentHResult;
        if (elliptical && dashMode != 0) return Direct3D9Factory.NotImplementedHResult;
        GeneratedDashStyleResource? dashStyle = null;
        if (dashMode != 0)
        {
            uint countDashes = *(uint*)(native + 64);
            uint capacity = *(uint*)(native + 60);
            float* data = *(float**)(native + 40);
            if (countDashes < 2 || (countDashes & 1) != 0 || countDashes > capacity
                || countDashes > int.MaxValue || data == null) return Direct3D9Factory.InvalidArgumentHResult;
            var dashes = new double[(int)countDashes];
            for (int i = 0; i < dashes.Length; i++)
            {
                if (!float.IsFinite(data[i])) return Direct3D9Factory.InvalidArgumentHResult;
                dashes[i] = data[i];
            }
            dashStyle = new(*(float*)(native + 36), dashes);
        }
        if (matrix.M13 != 0 || matrix.M14 != 0 || matrix.M23 != 0 || matrix.M24 != 0
            || matrix.M31 != 0 || matrix.M32 != 0 || matrix.M33 != 1 || matrix.M34 != 0
            || matrix.M43 != 0 || matrix.M44 != 1) return Direct3D9Factory.NotImplementedHResult;
        var transform = new GeneratedImageTransform(matrix.M11, matrix.M22, matrix.M41, matrix.M42, matrix.M12, matrix.M21);
        if (!float.IsFinite(geometry[7])) return Direct3D9Factory.InvalidArgumentHResult;
        var style = new GeneratedPenResource(elliptical ? 1 : width, Math.Max(1, geometry[7]),
            (MilPenCap)caps[0], (MilPenCap)caps[1], (MilPenCap)caps[2], (MilPenJoin)caps[3], dashStyle);
        var figures = new List<SoftwareStrokeContours.Figure>();
        void** table = *(void***)shape;
        uint count = ((delegate* unmanaged<nint, uint>)table[4])(shape);
        for (uint i = 0; i < count; i++)
        {
            nint figure = ((delegate* unmanaged<nint, uint, nint>)table[5])(shape, i);
            if (figure == 0) return Direct3D9Factory.InvalidArgumentHResult;
            void** methods = *(void***)figure;
            if (((delegate* unmanaged<nint, byte>)methods[1])(figure) != 0) continue;
            bool closed = ((delegate* unmanaged<nint, byte>)methods[4])(figure) != 0;
            float* start = (float*)((delegate* unmanaged<nint, nint>)methods[22])(figure);
            if (start == null) return Direct3D9Factory.InvalidArgumentHResult;
            var points = new List<MilPoint2D> { new(start[0], start[1]) };
            var joins = new List<bool> { false };
            MilPoint2D? firstTangent = null, lastTangent = null;
            static MilPoint2D? Direction(MilPoint2D origin, params MilPoint2D[] candidates)
            {
                foreach (var candidate in candidates)
                {
                    double dx = candidate.X - origin.X, dy = candidate.Y - origin.Y;
                    double length = Math.Sqrt(dx * dx + dy * dy);
                    if (double.IsFinite(length) && length > 0) return new(dx / length, dy / length);
                }
                return null;
            }
            if (!float.IsFinite(start[0]) || !float.IsFinite(start[1])) return Direct3D9Factory.InvalidArgumentHResult;
            bool startsAtGap = false, hasGaps = false;
            var runs = new List<SoftwareStrokeContours.Figure>();
            if (((delegate* unmanaged<nint, byte>)methods[15])(figure) != 0)
            {
                bool last;
                do
                {
                    byte type = 0; nint data = 0;
                    last = ((delegate* unmanaged<nint, byte*, nint*, byte>)methods[16])(figure, &type, &data) != 0;
                    if (data == 0) return Direct3D9Factory.InvalidArgumentHResult;
                    bool gap = ((delegate* unmanaged<nint, byte>)methods[7])(figure) != 0;
                    if (gap)
                    {
                        hasGaps = true;
                        if (points.Count > 1)
                            runs.Add(new(points.ToArray(), false, startsAtGap, true, joins.ToArray(),
                                StartTangent: firstTangent, EndTangent: lastTangent));
                        firstTangent = lastTangent = null;
                        var current = points[^1];
                        points.Clear(); points.Add(current);
                        joins.Clear(); joins.Add(false);
                        startsAtGap = true;
                    }
                    float* values = (float*)data;
                    var origin = points[^1];
                    MilPoint2D? outgoing, incoming;
                    if ((type & 3) == 1)
                    {
                        var endpoint = new MilPoint2D(values[0], values[1]);
                        outgoing = incoming = Direction(origin, endpoint);
                    }
                    else if ((type & 3) == 2)
                    {
                        var c1 = new MilPoint2D(values[0], values[1]);
                        var c2 = new MilPoint2D(values[2], values[3]);
                        var endpoint = new MilPoint2D(values[4], values[5]);
                        outgoing = Direction(origin, c1, c2, endpoint);
                        var backward = Direction(endpoint, c2, c1, origin);
                        incoming = backward is { } vector ? new(-vector.X, -vector.Y) : null;
                    }
                    else return Direct3D9Factory.NotImplementedHResult;
                    if (!gap && outgoing is not null)
                    {
                        firstTangent ??= outgoing;
                        lastTangent = incoming;
                    }
                    if ((type & 3) == 1) points.Add(new(values[0], values[1]));
                    else if ((type & 3) == 2)
                    {
                        var p = points[^1];
                        foreach (var end in SoftwareBezierFlattener.Flatten(new(p.X, p.Y),
                            new(values[0], values[1]), new(values[2], values[3]), new(values[4], values[5]), toleranceTransform ?? transform))
                            points.Add(new(end.X, end.Y));
                    }
                    else return Direct3D9Factory.NotImplementedHResult;
                    while (joins.Count < points.Count) joins.Add((type & 3) == 2);
                    joins[^1] = ((delegate* unmanaged<nint, byte>)methods[5])(figure) != 0;
                    if (points.Any(p => !double.IsFinite(p.X) || !double.IsFinite(p.Y)))
                        return Direct3D9Factory.InvalidArgumentHResult;
                    if (gap)
                    {
                        var end = points[^1];
                        points.Clear(); points.Add(end);
                        joins.Clear(); joins.Add(false);
                    }
                    if (!last && ((delegate* unmanaged<nint, byte>)methods[17])(figure) == 0)
                        return Direct3D9Factory.InvalidArgumentHResult;
                } while (!last);
            }
            if (!hasGaps)
                figures.Add(new(points.ToArray(), closed, SmoothJoins: joins.ToArray(),
                    StartTangent: firstTangent, EndTangent: lastTangent));
            else
            {
                var firstPoint = new MilPoint2D(start[0], start[1]);
                if (closed && points[^1] != firstPoint)
                {
                    points.Add(firstPoint);
                    joins.Add(false);
                }
                if (points.Count > 1)
                    runs.Add(new(points.ToArray(), false, startsAtGap, closed, joins.ToArray(),
                        StartTangent: firstTangent, EndTangent: lastTangent));
                if (closed && runs.Count > 1 && !runs[0].StartsAtGap
                    && runs[^1].Points[^1] == runs[0].Points[0])
                {
                    var first = runs[0]; var last = runs[^1];
                    runs[0] = new(last.Points.Concat(first.Points.Skip(1)).ToArray(), false,
                        last.StartsAtGap, first.EndsAtGap,
                        last.SmoothJoins!.Concat(first.SmoothJoins!.Skip(1)).ToArray(),
                        StartTangent: last.StartTangent, EndTangent: first.EndTangent);
                    runs.RemoveAt(runs.Count - 1);
                }
                figures.AddRange(runs);
            }
        }
        if (elliptical)
        {
            for (int i = 0; i < figures.Count; i++)
            {
                var figure = figures[i];
                var points = new MilPoint2D[figure.Points.Length];
                for (int j = 0; j < points.Length; j++)
                {
                    var point = shapeToPen.Apply(figure.Points[j].X, figure.Points[j].Y);
                    if (!double.IsFinite(point.X) || !double.IsFinite(point.Y)) return Direct3D9Factory.InvalidArgumentHResult;
                    points[j] = new(point.X, point.Y);
                }
                MilPoint2D? TransformDirection(MilPoint2D? direction)
                {
                    if (direction is not { } value) return null;
                    double x = value.X * shapeToPen.ScaleX + value.Y * shapeToPen.M21;
                    double y = value.X * shapeToPen.M12 + value.Y * shapeToPen.ScaleY;
                    double length = Math.Sqrt(x * x + y * y);
                    return length > 0 && double.IsFinite(length) ? new(x / length, y / length) : null;
                }
                figures[i] = figure with { Points = points,
                    StartTangent = TransformDirection(figure.StartTangent),
                    EndTangent = TransformDirection(figure.EndTangent) };
            }
            transform = transform.Prepend(penToShape);
        }
        return SoftwareStrokeWidening.TryCreate(figures, style, transform, out coverage, out bounds,
            createCoverage, toleranceTransform, clip)
            ? 0 : Direct3D9Factory.NotImplementedHResult;
    }
}
