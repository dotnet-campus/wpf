// Portions adapted from .NET WPF bezier.h and bezier.cpp under the MIT license.
// See LICENSE.TXT in the repository root.
namespace WpfGfxShape.Core;

internal static class SoftwareFillBezier
{
    internal readonly record struct Point(int X, int Y);

    // This entry accepts rasterizer 28.4 coordinates, not geometry coordinates.
    // No clip is supplied: invisible subcurves are still flattened.
    internal static List<Point> Flatten(ReadOnlySpan<Point> controls)
    {
        if (controls.Length != 4) throw new ArgumentException(message: null, paramName: nameof(controls));
        foreach (Point point in controls)
        {
            if (point.X < -8388608 || point.X > 8388608 || point.Y < -8388608 || point.Y > 8388608)
                throw new ArgumentOutOfRangeException(nameof(controls));
        }
        int left = controls[0].X, top = controls[0].Y;
        foreach (Point point in controls)
        {
            left = Math.Min(left, point.X);
            top = Math.Min(top, point.Y);
        }
        left -= 16;
        top -= 16;
        long bits = 0;
        foreach (Point point in controls) bits |= (point.X - left) | (point.Y - top);
        var x = new Basis(controls[0].X - left, controls[1].X - left, controls[2].X - left, controls[3].X - left, 10);
        var y = new Basis(controls[0].Y - top, controls[1].Y - top, controls[2].Y - top, controls[3].Y - top, 10);
        var result = new List<Point>();
        if ((bits & 0xffffc000L) == 0 && x.Error < (1536L << 10) && y.Error < (1536L << 10))
        {
            int shift = 0, steps = 1;
            while (Math.Max(x.Error, y.Error) > ((24L << 10) << shift))
            {
                shift += 2;
                x.LazyHalf(shift);
                y.LazyHalf(shift);
                steps = checked(steps * 2);
            }
            x.Steady(shift);
            y.Steady(shift);
            x.Step();
            y.Step();
            steps--;
            while (true)
            {
                result.Add(new(x.Value(13) + left, y.Value(13) + top));
                if (steps == 0) break;
                Adjust(x, y, ref steps, 24L << 13);
                steps--;
                x.Step();
                y.Step();
            }
        }
        else
        {
            const long highError = 196608L << 28;
            const long lowError = 3L << 31;
            var highX = new Basis(controls[0].X, controls[1].X, controls[2].X, controls[3].X, 28);
            var highY = new Basis(controls[0].Y, controls[1].Y, controls[2].Y, controls[3].Y, 28);
            int highSteps = 1;
            while (Math.Max(highX.Error, highY.Error) > highError)
            {
                highX.Half();
                highY.Half();
                highSteps = checked(highSteps * 2);
            }
            while (true)
            {
                x = highX.Reconstruct();
                y = highY.Reconstruct();
                int lowSteps = 1;
                while (Math.Max(x.Error, y.Error) > lowError)
                {
                    x.Half();
                    y.Half();
                    lowSteps = checked(lowSteps * 2);
                }
                if (--highSteps != 0)
                {
                    highX.Step();
                    highY.Step();
                    Adjust(highX, highY, ref highSteps, highError);
                }
                while (true)
                {
                    x.Step();
                    y.Step();
                    result.Add(new(x.Value(28), y.Value(28)));
                    if (--lowSteps == 0) break;
                    Adjust(x, y, ref lowSteps, lowError);
                }
                if (highSteps == 0) break;
            }
        }
        // CMILBezier does not promise an exact final control point; its caller adds it.
        if (result[^1] != controls[3]) result.Add(controls[3]);
        return result;
    }

    internal static bool TryRound(float value, out int result)
    {
        result = 0;
        if (!(value >= -8388608f && value <= 8388608f)) return false;
        result = (int)Math.Floor((double)value + 0.5);
        return true;
    }

    private static void Adjust(Basis x, Basis y, ref int steps, long error)
    {
        if (Math.Max(x.Error, y.Error) > error)
        {
            x.Half();
            y.Half();
            steps = checked(steps * 2);
        }
        // Zero is terminal, not an even remaining step count to be doubled forever.
        while (steps != 0 && (steps & 1) == 0 && x.ParentQuarter <= (error >> 2) && y.ParentQuarter <= (error >> 2))
        {
            x.Double();
            y.Double();
            steps >>= 1;
        }
    }

    private sealed class Basis
    {
        private long e0, e1, e2, e3;

        internal Basis(int a, int b, int c, int d, int fraction)
        {
            e0 = (long)a << fraction;
            e1 = ((long)d - a) << fraction;
            e2 = (6L * (b - 2L * c + d)) << fraction;
            e3 = (6L * (a - 2L * b + c)) << fraction;
        }

        internal long Error => Math.Max(Math.Abs(e2), Math.Abs(e3));
        internal long ParentQuarter => Math.Max(Math.Abs(e3), Math.Abs(2 * e2 - e3));
        internal int Value(int fraction) => checked((int)((e0 + (1L << (fraction - 1))) >> fraction));
        internal void LazyHalf(int shift)
        {
            e2 = (e2 + e3) >> 1;
            e1 = (e1 - (e2 >> shift)) >> 1;
        }
        internal void Steady(int shift)
        {
            e0 <<= 3;
            e1 <<= 3;
            if (shift < 3) { e2 <<= 3 - shift; e3 <<= 3 - shift; }
            else { e2 >>= shift - 3; e3 >>= shift - 3; }
        }
        internal void Half()
        {
            e2 = (e2 + e3) >> 3;
            e1 = (e1 - e2) >> 1;
            e3 >>= 2;
        }
        internal void Double()
        {
            e1 = 2 * e1 + e2;
            e3 <<= 2;
            e2 = (e2 << 3) - e3;
        }
        internal void Step()
        {
            e0 += e1;
            long previous = e2;
            e1 += previous;
            e2 += previous - e3;
            e3 = previous;
        }
        internal Basis Reconstruct()
        {
            long b = e0 + (6 * e1 - e2 - 2 * e3) / 18;
            long c = e0 + (12 * e1 - 2 * e2 - e3) / 18;
            return new(Round(e0), Round(b), Round(c), Round(e0 + e1), 28);
        }
        private static int Round(long value) => checked((int)((value + (1L << 27)) >> 28));
    }
}
