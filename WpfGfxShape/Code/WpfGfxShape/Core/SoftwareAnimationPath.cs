namespace WpfGfxShape.Core;

internal static class SoftwareAnimationPath
{
    private readonly record struct Segment(MilPoint2D A, MilPoint2D B, MilPoint2D C, MilPoint2D D, bool Curve)
    {
        internal MilPoint2D Velocity(double t)
        {
            if (!Curve) return new(D.X - A.X, D.Y - A.Y);
            double s = 1 - t;
            return new(3 * (s * s * (B.X - A.X) + 2 * s * t * (C.X - B.X) + t * t * (D.X - C.X)),
                3 * (s * s * (B.Y - A.Y) + 2 * s * t * (C.Y - B.Y) + t * t * (D.Y - C.Y)));
        }

        internal double Length(double end)
        {
            if (!Curve)
            {
                var v = Velocity(0);
                return end * Math.Sqrt(v.X * v.X + v.Y * v.Y);
            }
            double sum = 0;
            const int intervals = 64;
            double h = end / intervals;
            for (int i = 0; i < intervals; i++)
            {
                var a = Velocity((i + 0.2113248654051875) * h);
                var b = Velocity((i + 0.7886751345948125) * h);
                sum += Math.Sqrt(a.X * a.X + a.Y * a.Y) + Math.Sqrt(b.X * b.X + b.Y * b.Y);
            }
            return sum * h * 0.5;
        }

        internal MilPoint2D Position(double t)
        {
            if (!Curve) return new(A.X + t * (D.X - A.X), A.Y + t * (D.Y - A.Y));
            double s = 1 - t;
            return new(s * s * s * A.X + 3 * s * s * t * B.X + 3 * s * t * t * C.X + t * t * t * D.X,
                s * s * s * A.Y + 3 * s * s * t * B.Y + 3 * s * t * t * C.Y + t * t * t * D.Y);
        }
    }

    internal static bool TryGetPoint(GeneratedPathGeometryResource path, GeneratedImageTransform transform,
        double fraction, out MilPoint2D point, out MilPoint2D tangent)
    {
        point = tangent = default;
        if (!SoftwareStrokeContours.TryRead(path, out var figures, preserveGaps: true, allowNonFinite: true)) return false;
        MilPoint2D Map(MilPoint2D p)
        {
            var q = transform.Apply((float)p.X, (float)p.Y);
            return new((float)q.X, (float)q.Y);
        }
        var segments = new List<(Segment Value, double Length)>();
        foreach (var figure in figures)
        {
            if (figure.Points.Length == 0) continue;
            var start = Map(figure.Points[0]);
            var current = start;
            void Add(MilPoint2D[] controls)
            {
                var end = Map(controls[^1]);
                var segment = controls.Length == 1 ? new Segment(current, current, end, end, false)
                    : new Segment(current, Map(controls[0]), Map(controls[1]), end, true);
                double length = segment.Length(1);
                if (length != 0) segments.Add((segment, length));
                current = end;
            }
            foreach (var segment in figure.Segments ?? []) Add(segment.Points);
            if (figure.Closed && current != start)
            {
                var segment = new Segment(current, current, start, start, false);
                double length = segment.Length(1);
                if (length != 0) segments.Add((segment, length));
            }
        }
        if (segments.Count == 0) return false;
        double target = Math.Clamp((float)fraction, 0, 1) * segments.Sum(s => s.Length);
        for (int i = 0; i < segments.Count; i++)
        {
            var (segment, length) = segments[i];
            if (target <= length || i == segments.Count - 1)
            {
                double t = target / length;
                if (segment.Curve && double.IsFinite(t))
                {
                    double low = 0, high = 1;
                    for (int j = 0; j < 40; j++)
                    {
                        t = (low + high) * 0.5;
                        if (segment.Length(t) < target) low = t; else high = t;
                    }
                }
                var p = segment.Position(t);
                var v = segment.Velocity(t);
                double norm = Math.Sqrt(v.X * v.X + v.Y * v.Y);
                point = new((float)p.X, (float)p.Y);
                tangent = new((float)(v.X / norm), (float)(v.Y / norm));
                return true;
            }
            target -= length;
        }
        return false;
    }
}
