namespace WpfGfxShape.Core;

internal static class SoftwareBooleanContours
{
    private readonly record struct Edge(MilPoint2D A, MilPoint2D B);

    internal static bool TryCombine(List<SoftwareStrokeContours.Figure> first, MilFillMode firstRule,
        List<SoftwareStrokeContours.Figure> second, MilFillMode secondRule, MilCombineMode mode,
        out List<SoftwareStrokeContours.Figure> figures)
    {
        figures = [];
        var a = Edges(first);
        var b = Edges(second);
        var edges = a.Concat(b).ToArray();
        if (edges.Any(e => !double.IsFinite(e.A.X) || !double.IsFinite(e.A.Y)
            || !double.IsFinite(e.B.X) || !double.IsFinite(e.B.Y))) return false;
        if (edges.Length == 0) return true;
        double left = edges.Min(e => Math.Min(e.A.X, e.B.X));
        double right = edges.Max(e => Math.Max(e.A.X, e.B.X));
        double top = edges.Min(e => Math.Min(e.A.Y, e.B.Y));
        double bottom = edges.Max(e => Math.Max(e.A.Y, e.B.Y));
        double extent = Math.Max(right - left, bottom - top);
        if (!double.IsFinite(extent)) return false;
        if (extent < 1.1754943508222875e-38) return true;
        double centerX = left + (right - left) * 0.5;
        double centerY = top + (bottom - top) * 0.5;
        double scale = 67108863 / extent;
        MilPoint2D ToWorkspace(MilPoint2D point) => Snap(new(
            (point.X - centerX) * scale, (point.Y - centerY) * scale));
        MilPoint2D FromWorkspace(MilPoint2D point) => new(
            point.X / scale + centerX, point.Y / scale + centerY);
        // CScanner rounds a bounds-relative copy; output remains in shape space.
        a = a.Select(e => new Edge(ToWorkspace(e.A), ToWorkspace(e.B))).ToList();
        b = b.Select(e => new Edge(ToWorkspace(e.A), ToWorkspace(e.B))).ToList();
        edges = a.Concat(b).ToArray();
        bool Inside(MilPoint2D p)
        {
            bool x = Contains(a, p, firstRule), y = Contains(b, p, secondRule);
            return mode switch
            {
                MilCombineMode.Union => x || y,
                MilCombineMode.Intersect => x && y,
                MilCombineMode.Exclude => x && !y,
                MilCombineMode.Xor => x != y,
                _ => false
            };
        }
        var boundary = new HashSet<Edge>();
        foreach (var edge in edges)
        {
            double dx = edge.B.X - edge.A.X, dy = edge.B.Y - edge.A.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length == 0) continue;
            var cuts = new SortedSet<double> { 0, 1 };
            foreach (var other in edges)
            {
                double ox = other.B.X - other.A.X, oy = other.B.Y - other.A.Y;
                double px = other.A.X - edge.A.X, py = other.A.Y - edge.A.Y;
                double determinant = dx * oy - dy * ox;
                if (determinant != 0)
                {
                    double t = (px * oy - py * ox) / determinant;
                    double u = (px * dy - py * dx) / determinant;
                    if (t > 0 && t < 1 && u >= 0 && u <= 1) cuts.Add(t);
                }
                else if (px * dy - py * dx == 0)
                {
                    double denominator = dx * dx + dy * dy;
                    double t = (px * dx + py * dy) / denominator;
                    double u = ((other.B.X - edge.A.X) * dx + (other.B.Y - edge.A.Y) * dy) / denominator;
                    if (t > 0 && t < 1) cuts.Add(t);
                    if (u > 0 && u < 1) cuts.Add(u);
                }
            }
            double previous = 0;
            foreach (double cut in cuts.Skip(1))
            {
                var start = Snap(new(edge.A.X + previous * dx, edge.A.Y + previous * dy));
                var end = Snap(new(edge.A.X + cut * dx, edge.A.Y + cut * dy));
                double interval = cut - previous;
                previous = cut;
                if (start == end) continue;
                // Classify against the original edge, before workspace rounding moves the endpoints.
                double midpoint = cut - interval * 0.5;
                var middle = new MilPoint2D(edge.A.X + midpoint * dx, edge.A.Y + midpoint * dy);
                double distance = Math.Min(length * interval * 1e-5, 1e-4);
                bool insideLeft = Inside(new(middle.X - dy / length * distance, middle.Y + dx / length * distance));
                bool insideRight = Inside(new(middle.X + dy / length * distance, middle.Y - dx / length * distance));
                if (insideLeft != insideRight) boundary.Add(insideLeft ? new(start, end) : new(end, start));
            }
        }
        while (boundary.Count != 0)
        {
            var edge = boundary.First();
            boundary.Remove(edge);
            var points = new List<MilPoint2D> { edge.A, edge.B };
            while (points[^1] != points[0])
            {
                var candidates = boundary.Where(e => e.A == points[^1]).ToArray();
                if (candidates.Length == 0) return false;
                // At a touching junction continue the boundary with interior on its left.
                double dx = points[^1].X - points[^2].X, dy = points[^1].Y - points[^2].Y;
                var next = candidates.OrderBy(e => Turn(dx, dy, e.B.X - e.A.X, e.B.Y - e.A.Y)).First();
                boundary.Remove(next);
                points.Add(next.B);
            }
            if (points.Count > 3) figures.Add(new(points.Select(FromWorkspace).ToArray(), true));
        }
        return true;
    }

    private static double Turn(double x, double y, double dx, double dy)
    {
        double angle = Math.Atan2(x * dy - y * dx, x * dx + y * dy);
        return angle < 0 ? angle + Math.PI * 2 : angle;
    }

    private static MilPoint2D Snap(MilPoint2D point) => new(Math.Round(point.X), Math.Round(point.Y));

    private static List<Edge> Edges(IEnumerable<SoftwareStrokeContours.Figure> figures)
    {
        var result = new List<Edge>();
        foreach (var figure in figures)
        {
            if (!figure.Fillable || figure.Points.Length < 3) continue;
            for (int i = 0; i < figure.Points.Length; i++)
            {
                var a = figure.Points[i]; var b = figure.Points[(i + 1) % figure.Points.Length];
                if (a != b) result.Add(new(a, b));
            }
        }
        return result;
    }

    private static bool Contains(IEnumerable<Edge> edges, MilPoint2D point, MilFillMode rule)
    {
        int winding = 0;
        foreach (var edge in edges)
        {
            bool up = edge.A.Y <= point.Y && edge.B.Y > point.Y;
            bool down = edge.B.Y <= point.Y && edge.A.Y > point.Y;
            if (!up && !down) continue;
            double x = edge.A.X + (point.Y - edge.A.Y) * (edge.B.X - edge.A.X) / (edge.B.Y - edge.A.Y);
            if (x > point.X) winding += up ? 1 : -1;
        }
        return rule == MilFillMode.Winding ? winding != 0 : (winding & 1) != 0;
    }
}
