namespace WpfGfxShape.Core;

internal sealed class SoftwareImageCoverage
{
    private readonly (long X, long Y)[] _points;
    private readonly SoftwareImageCoverage? _first;
    private readonly SoftwareImageCoverage? _second;
    private readonly MilCombineMode _mode;
    private readonly SoftwarePathCoverage? _path;

    internal SoftwareImageCoverage(SoftwarePathCoverage path)
    {
        _points = [];
        _path = path;
    }

    private SoftwareImageCoverage(SoftwareImageCoverage? first, SoftwareImageCoverage? second, MilCombineMode mode)
    {
        _points = [];
        _first = first;
        _second = second;
        _mode = mode;
    }

    internal bool TryClipParallelogram(MilPoint2D[] polygon, out SoftwareImageCoverage? result)
    {
        result = null;
        if (_path is not null)
        {
            if (!_path.TryClipParallelogram(polygon, out var path) || path is null) return false;
            result = new(path);
            return true;
        }
        if (_points.Length != 0) return false;
        SoftwareImageCoverage? first = null, second = null;
        if (_first is not null && !_first.TryClipParallelogram(polygon, out first)) return false;
        if (_second is not null && !_second.TryClipParallelogram(polygon, out second)) return false;
        result = Combine(first, second, _mode);
        return true;
    }

    internal static SoftwareImageCoverage Combine(SoftwareImageCoverage? first, SoftwareImageCoverage? second, MilCombineMode mode)
        => new(first, second, mode);

    private SoftwareImageCoverage((long X, long Y)[] points) => _points = points;

    internal static bool TryCreate(MilRectD rectangle, GeneratedImageTransform transform, out SoftwareImageCoverage? coverage,
        Direct3D9SurfaceRect? clip = null)
    {
        coverage = null;
        // Negative extents describe an empty geometry, not a reflected polygon.
        if (rectangle.Width <= 0 || rectangle.Height <= 0)
        {
            coverage = new(Array.Empty<(long X, long Y)>());
            return true;
        }
        (double X, double Y)[] corners = [
            transform.Apply(rectangle.X, rectangle.Y),
            transform.Apply(rectangle.X + rectangle.Width, rectangle.Y),
            transform.Apply(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height),
            transform.Apply(rectangle.X, rectangle.Y + rectangle.Height)];
        foreach (var corner in corners)
            if (!double.IsFinite(corner.X) || !double.IsFinite(corner.Y)) return false;
        if (clip is { } bounds)
        {
            // Clip in double precision before 28.4 conversion so enormous viewports remain representable.
            var polygon = corners.ToList();
            polygon = ClipPolygon(polygon, bounds.Left, true, true);
            polygon = ClipPolygon(polygon, bounds.Right, true, false);
            polygon = ClipPolygon(polygon, bounds.Top, false, true);
            polygon = ClipPolygon(polygon, bounds.Bottom, false, false);
            corners = polygon.ToArray();
        }
        var points = new (long X, long Y)[corners.Length];
        for (int i = 0; i < corners.Length; i++)
        {
            double x = (corners[i].X - 0.5) * 16, y = (corners[i].Y - 0.5) * 16;
            if (!double.IsFinite(x) || !double.IsFinite(y) || Math.Abs(x) > 8388608 || Math.Abs(y) > 8388608) return false;
            // Native path conversion rounds to 28.4 before the AA half-pixel fixup.
            points[i] = (((long)Math.Round(x) + 8) * 8, ((long)Math.Round(y) + 8) * 8);
        }
        coverage = new(points);
        return true;
    }

    private static List<(double X, double Y)> ClipPolygon(List<(double X, double Y)> polygon,
        double boundary, bool horizontal, bool keepGreater)
    {
        List<(double X, double Y)> result = [];
        if (polygon.Count == 0) return result;
        var previous = polygon[^1];
        double previousCoordinate = horizontal ? previous.X : previous.Y;
        bool previousInside = keepGreater ? previousCoordinate >= boundary : previousCoordinate <= boundary;
        foreach (var current in polygon)
        {
            double coordinate = horizontal ? current.X : current.Y;
            bool inside = keepGreater ? coordinate >= boundary : coordinate <= boundary;
            if (inside != previousInside)
            {
                double fraction = (boundary - previousCoordinate) / (coordinate - previousCoordinate);
                result.Add(horizontal
                    ? (boundary, previous.Y + fraction * (current.Y - previous.Y))
                    : (previous.X + fraction * (current.X - previous.X), boundary));
            }
            if (inside) result.Add(current);
            previous = current;
            previousCoordinate = coordinate;
            previousInside = inside;
        }
        return result;
    }

    internal int GetCoverage(int x, int y) => System.Numerics.BitOperations.PopCount(GetSamples(x, y));

    internal int GetAliasedCoverage(int x, int y)
    {
        if (_path is not null) return _path.GetAliasedCoverage(x, y);
        if (_points.Length == 0)
        {
            bool first = _first?.GetAliasedCoverage(x, y) == 64;
            bool second = _second?.GetAliasedCoverage(x, y) == 64;
            bool inside = _mode switch
            {
                MilCombineMode.Union => first || second,
                MilCombineMode.Intersect => first && second,
                MilCombineMode.Xor => first != second,
                MilCombineMode.Exclude => first && !second,
                _ => false
            };
            return inside ? 64 : 0;
        }
        if (_points.Length < 3) return 0;
        long scanY = (long)y * 16;
        long left = long.MaxValue, right = long.MinValue;
        for (int i = 0; i < _points.Length; i++)
        {
            var a = (_points[i].X / 8 - 8, _points[i].Y / 8 - 8);
            var b = (_points[(i + 1) % _points.Length].X / 8 - 8, _points[(i + 1) % _points.Length].Y / 8 - 8);
            if (a.Item2 > b.Item2) (a, b) = (b, a);
            if (scanY < a.Item2 || scanY >= b.Item2) continue;
            long denominator = (b.Item2 - a.Item2) * 16;
            long numerator = a.Item1 * (b.Item2 - a.Item2) + (scanY - a.Item2) * (b.Item1 - a.Item1);
            long intersection = numerator / denominator;
            if (numerator % denominator > 0) intersection++;
            left = Math.Min(left, intersection); right = Math.Max(right, intersection);
        }
        return x >= left && x < right ? 64 : 0;
    }

    private ulong GetSamples(int x, int y)
    {
        if (_path is not null) return _path.GetSamples(x, y);
        if (_points.Length == 0)
        {
            ulong first = _first?.GetSamples(x, y) ?? 0;
            ulong second = _second?.GetSamples(x, y) ?? 0;
            return _mode switch
            {
                MilCombineMode.Union => first | second,
                MilCombineMode.Intersect => first & second,
                MilCombineMode.Xor => first ^ second,
                MilCombineMode.Exclude => first & ~second,
                _ => 0
            };
        }
        ulong coverage = 0;
        for (int row = 0; row < 8; row++)
        {
            long scanY = ((long)y * 8 + row) * 16;
            long left = long.MaxValue, right = long.MinValue;
            for (int i = 0; i < _points.Length; i++)
            {
                var a = _points[i]; var b = _points[(i + 1) % _points.Length];
                if (a.Y > b.Y) (a, b) = (b, a);
                if (scanY < a.Y || scanY >= b.Y) continue;
                long denominator = (b.Y - a.Y) * 16;
                long numerator = a.X * (b.Y - a.Y) + (scanY - a.Y) * (b.X - a.X);
                long intersection = numerator / denominator;
                if (numerator % denominator > 0) intersection++;
                left = Math.Min(left, intersection); right = Math.Max(right, intersection);
            }
            for (int column = 0; column < 8; column++)
            {
                long sampleX = (long)x * 8 + column;
                if (sampleX >= left && sampleX < right)
                    coverage |= 1UL << (row * 8 + column);
            }
        }
        return coverage;
    }
}
