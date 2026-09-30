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

    internal static SoftwareImageCoverage Combine(SoftwareImageCoverage? first, SoftwareImageCoverage? second, MilCombineMode mode)
        => new(first, second, mode);

    private SoftwareImageCoverage((long X, long Y)[] points) => _points = points;

    internal static bool TryCreate(MilRectD rectangle, GeneratedImageTransform transform, out SoftwareImageCoverage? coverage)
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
        var points = new (long X, long Y)[4];
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

    internal int GetCoverage(int x, int y) => System.Numerics.BitOperations.PopCount(GetSamples(x, y));

    internal int GetAliasedCoverage(int x, int y)
    {
        if (_points.Length != 4) return 0;
        long scanY = (long)y * 16;
        long left = long.MaxValue, right = long.MinValue;
        for (int i = 0; i < 4; i++)
        {
            var a = (_points[i].X / 8 - 8, _points[i].Y / 8 - 8);
            var b = (_points[(i + 1) % 4].X / 8 - 8, _points[(i + 1) % 4].Y / 8 - 8);
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
            for (int i = 0; i < 4; i++)
            {
                var a = _points[i]; var b = _points[(i + 1) % 4];
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
