namespace WpfGfxShape.Core;

internal static class SoftwareBezierFlattener
{
    internal readonly record struct Point(double X, double Y)
    {
        public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
        public static Point operator *(Point a, double scale) => new(a.X * scale, a.Y * scale);
        internal double Norm => Math.Max(Math.Abs(X), Math.Abs(Y));
    }

    internal static IEnumerable<Point> Flatten(Point start, Point first, Point second, Point end)
    {
        Point chord = end - start;
        bool OnChord(Point point)
        {
            Point delta = point - start;
            double projection = delta.X * chord.X + delta.Y * chord.Y;
            return delta.X * chord.Y == delta.Y * chord.X && projection >= 0
                && projection <= chord.X * chord.X + chord.Y * chord.Y;
        }
        // A collinear control polygon inside the chord describes exactly a line.
        if (chord.Norm > 0 && OnChord(first) && OnChord(second))
        {
            yield return end;
            yield break;
        }
        const double tolerance = 0.25 * 6;
        Point e0 = start, e1 = end - start;
        Point e2 = (first - second * 2 + end) * 6;
        Point e3 = (start - first * 2 + second) * 6;
        int steps = 1;
        double stepSize = 1;
        void Halve()
        {
            e2 = (e2 + e3) * 0.125;
            e1 = (e1 - e2) * 0.5;
            e3 *= 0.25;
            steps *= 2;
            stepSize *= 0.5;
        }
        while ((e2.Norm > tolerance || e3.Norm > tolerance) && stepSize > 1e-3) Halve();
        while (steps > 1)
        {
            e0 += e1;
            Point previous = e2;
            e1 += previous;
            e2 = e2 + previous - e3;
            e3 = previous;
            steps--;
            yield return e0;
            if (e2.Norm > tolerance && stepSize > 1e-3) Halve();
            else
            {
                while ((steps & 1) == 0)
                {
                    Point doubled = e2 * 2 - e3;
                    if (e3.Norm > tolerance * 0.25 || doubled.Norm > tolerance * 0.25) break;
                    e1 = e1 * 2 + e2;
                    e3 *= 4;
                    e2 = doubled * 4;
                    steps /= 2;
                    stepSize *= 2;
                }
            }
        }
        yield return end;
    }
}
