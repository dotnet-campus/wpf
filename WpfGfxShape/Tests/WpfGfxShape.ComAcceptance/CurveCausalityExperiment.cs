// Portions adapted from .NET WPF bezier.h and bezier.cpp.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses those files under the MIT license.
// See LICENSE.TXT in the repository root.
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.ComAcceptance;

internal static class CurveCausalityExperiment
{
    private sealed class Basis(int a, int b, int c, int d)
    {
        internal long E0 = (long)a << 10;
        internal long E1 = (long)(d - a) << 10;
        internal long E2 = (long)(6 * (b - 2 * c + d)) << 10;
        internal long E3 = (long)(6 * (a - 2 * b + c)) << 10;
        internal long Error => Math.Max(Math.Abs(E2), Math.Abs(E3));
        internal long Parent => Math.Max(Math.Abs(E3), Math.Abs(2 * E2 - E3));
        internal void Lazy(int shift) { E2 = (E2 + E3) >> 1; E1 = (E1 - (E2 >> shift)) >> 1; }
        internal void Steady(int shift)
        {
            E0 <<= 3; E1 <<= 3;
            if (shift < 3) { E2 <<= 3 - shift; E3 <<= 3 - shift; }
            else { E2 >>= shift - 3; E3 >>= shift - 3; }
        }
        internal void Half() { E2 = (E2 + E3) >> 3; E1 = (E1 - E2) >> 1; E3 >>= 2; }
        internal void Double() { E1 = 2 * E1 + E2; E3 <<= 2; E2 = (E2 << 3) - E3; }
        internal void Step() { E0 += E1; long old = E2; E1 += old; E2 += old - E3; E3 = old; }
    }

    private static double Q(double value) => Math.Floor((value - 0.5) * 16 + 0.5);
    private static Point Quantize(Point point) => new(Q(point.X) / 16 + 0.5, Q(point.Y) / 16 + 0.5);

    // This bounded intervention only accepts the small control polygons in this experiment.
    // The 64-bit branch requires a single high step; clipped or larger curves are not modeled.
    private static Point[] Integer(Point[] points, bool exactEvaluation, out double[] times)
    {
        int[] px = points.Select(p => (int)Q(p.X)).ToArray();
        int[] py = points.Select(p => (int)Q(p.Y)).ToArray();
        int ox = px.Min(), oy = py.Min();
        if (px.Max() - ox >= 1024 || py.Max() - oy >= 1024)
            throw new InvalidOperationException("Experiment exceeds the audited HFD32 domain.");
        var x = new Basis(px[0] - ox, px[1] - ox, px[2] - ox, px[3] - ox);
        var y = new Basis(py[0] - oy, py[1] - oy, py[2] - oy, py[3] - oy);
        bool wide = Math.Max(x.Error, y.Error) >= (24L << 6) * 1024;
        int fraction = wide ? 28 : 13;
        long tolerance = 24L << fraction;
        int steps = 1, shift = 0;
        double dt = 1, t = 0;
        if (wide)
        {
            // With no high-stage subdivision, the inverse transform returns the controls exactly.
            foreach (Basis axis in new[] { x, y })
            { axis.E0 <<= 18; axis.E1 <<= 18; axis.E2 <<= 18; axis.E3 <<= 18; }
            if (Math.Max(x.Error, y.Error) > (196608L << 28))
                throw new InvalidOperationException("Experiment requires high-stage subdivision.");
            while (Math.Max(x.Error, y.Error) > tolerance)
            { x.Half(); y.Half(); steps *= 2; dt /= 2; }
        }
        else
        {
            while (Math.Max(x.Error, y.Error) > (24L << 10) << shift)
            {
                shift += 2; x.Lazy(shift); y.Lazy(shift); steps *= 2; dt /= 2;
            }
            x.Steady(shift); y.Steady(shift);
        }
        x.Step(); y.Step(); steps--; t += dt;
        var result = new List<Point>();
        var schedule = new List<double>();
        Point[] q = points.Select(Quantize).ToArray();
        while (true)
        {
            schedule.Add(t);
            double s = 1 - t;
            result.Add(exactEvaluation
                ? Quantize(new Point(s * s * s * q[0].X + 3 * s * s * t * q[1].X + 3 * s * t * t * q[2].X + t * t * t * q[3].X,
                    s * s * s * q[0].Y + 3 * s * s * t * q[1].Y + 3 * s * t * t * q[2].Y + t * t * t * q[3].Y))
                : new Point((((x.E0 + (1L << (fraction - 1))) >> fraction) + ox) / 16.0 + 0.5, (((y.E0 + (1L << (fraction - 1))) >> fraction) + oy) / 16.0 + 0.5));
            if (steps == 0) break;
            if (Math.Max(x.Error, y.Error) > tolerance)
            { x.Half(); y.Half(); steps *= 2; dt /= 2; }
            while ((steps & 1) == 0 && Math.Max(x.Parent, y.Parent) <= tolerance / 4)
            { x.Double(); y.Double(); steps /= 2; dt *= 2; }
            steps--; x.Step(); y.Step(); t += dt;
        }
        times = schedule.ToArray();
        return result.ToArray();
    }

    private static Point[] Floating(Point[] p)
    {
        const double tolerance = 1.5;
        Vector e1 = p[3] - p[0], e2 = 6 * ((p[1] - p[2]) + (p[3] - p[2])),
            e3 = 6 * ((p[0] - p[1]) + (p[2] - p[1]));
        Point e0 = p[0];
        int steps = 1;
        double dt = 1;
        static double Error(Vector value) => Math.Max(Math.Abs(value.X), Math.Abs(value.Y));
        void Half() { e2 = (e2 + e3) * 0.125; e1 = (e1 - e2) * 0.5; e3 *= 0.25; steps *= 2; dt *= 0.5; }
        while ((Error(e2) > tolerance || Error(e3) > tolerance) && dt > 1e-3) Half();
        var result = new List<Point>();
        while (steps > 1)
        {
            e0 += e1; Vector old = e2; e1 += old; e2 = e2 + old - e3; e3 = old; steps--;
            result.Add(e0);
            if (Error(e2) > tolerance && dt > 1e-3) Half();
            else while ((steps & 1) == 0)
            {
                Vector doubled = 2 * e2 - e3;
                if (Error(e3) > tolerance * 0.25 || Error(doubled) > tolerance * 0.25) break;
                e1 = 2 * e1 + e2; e3 *= 4; e2 = doubled * 4; steps /= 2; dt *= 2;
            }
        }
        result.Add(p[3]);
        return result.ToArray();
    }

    internal static void Render(string directory)
    {
        RenderBoundaries(directory);
        CurveDeviceQuantizationExperiment.Render(directory);
        const double k = 0.5522847498307933984;
        Point[][] rounded =
        [
            [new(4,16), new(4,16-12*k), new(20-16*k,4), new(20,4)],
            [new(20,4), new(20+16*k,4), new(36,16-12*k), new(36,16)],
            [new(36,16), new(36,16+12*k), new(20+16*k,28), new(20,28)],
            [new(20,28), new(20-16*k,28), new(4,16+12*k), new(4,16)]
        ];
        foreach (int frame in new[] { 5, 6, 7, 8, 9 })
        {
            double cx = frame switch { 6 => 4, 7 => 4.001, 8 => 4.02, _ => 4.04 };
            Point[][] segments = frame == 5 ? rounded : [[new(4,28), new(cx,4), new(36,4), new(36,28)]];
            foreach (string mode in new[] { "double", "prequantized-double", "integer-schedule-exact", "integer" })
            {
                var points = new List<Point> { mode == "double" ? segments[0][0] : Quantize(segments[0][0]) };
                var traces = new List<object>();
                foreach (Point[] segment in segments)
                {
                    Point[] output;
                    double[] schedule = [];
                    if (mode == "double") output = Floating(segment);
                    else if (mode == "prequantized-double") output = Floating(segment.Select(Quantize).ToArray());
                    else output = Integer(segment, mode == "integer-schedule-exact", out schedule);
                    points.AddRange(output);
                    traces.Add(new { input = segment.Select(p => new { p.X, p.Y }), schedule,
                        quantizedOutput = output.Select(Quantize).Select(p => new { p.X, p.Y }) });
                }
                var geometry = new StreamGeometry();
                using (StreamGeometryContext path = geometry.Open())
                {
                    path.BeginFigure(points[0], true, true);
                    foreach (Point point in points.Skip(1)) path.LineTo(point, true, false);
                }
                var visual = new DrawingVisual();
                using (DrawingContext context = visual.RenderOpen())
                    context.DrawGeometry(frame % 2 == 0 ? Brushes.Red : Brushes.Blue, null, geometry);
                var bitmap = new RenderTargetBitmap(40, 32, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(visual);
                byte[] pixels = new byte[5120];
                bitmap.CopyPixels(pixels, 160, 0);
                File.WriteAllBytes(Path.Combine(directory, $"aq-{frame}-{mode}.pbgra32"), pixels);
                File.WriteAllText(Path.Combine(directory, $"aq-{frame}-{mode}.json"), JsonSerializer.Serialize(traces));
            }
        }
    }

    private static void RenderBoundaries(string directory)
    {
        var cases = new[]
        {
            (name: "negative-half-below", points: new Point[] { new(-2,24), new(-0.0312501,-4), new(30,-4), new(34,24) }, matrix: Matrix.Identity),
            (name: "negative-half", points: new Point[] { new(-2,24), new(-0.03125,-4), new(30,-4), new(34,24) }, matrix: Matrix.Identity),
            (name: "negative-half-above", points: new Point[] { new(-2,24), new(-0.0312499,-4), new(30,-4), new(34,24) }, matrix: Matrix.Identity),
            (name: "transform-order", points: new Point[] { new(-2.03125,20), new(-1.04,-3), new(24.02,-3), new(28,20) }, matrix: new Matrix(1.125,0,0,0.875,3.17,2.09)),
            (name: "small", points: new Point[] { new(8.02,12), new(8.04,8), new(12,8), new(12,12) }, matrix: Matrix.Identity),
            (name: "large-clipped", points: new Point[] { new(-80,28), new(-79.96,-180), new(200,-180), new(200,28) }, matrix: Matrix.Identity)
        };
        foreach (var item in cases)
        foreach (string mode in new[] { "cubic", "double", "device-prequantized", "object-prequantized" })
        {
            Point[] device = item.points.Select(item.matrix.Transform).ToArray();
            Point[] controls = mode == "object-prequantized"
                ? item.points.Select(Quantize).Select(item.matrix.Transform).ToArray()
                : mode == "device-prequantized" ? device.Select(Quantize).ToArray() : device;
            Point[] output = mode == "cubic" ? controls[1..] : Floating(controls);
            var geometry = new StreamGeometry();
            using (StreamGeometryContext path = geometry.Open())
            {
                path.BeginFigure(controls[0], true, true);
                if (mode == "cubic") path.BezierTo(output[0], output[1], output[2], true, false);
                else foreach (Point point in output) path.LineTo(point, true, false);
            }
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen()) context.DrawGeometry(Brushes.Blue, null, geometry);
            var bitmap = new RenderTargetBitmap(40, 32, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            byte[] pixels = new byte[5120];
            bitmap.CopyPixels(pixels, 160, 0);
            File.WriteAllBytes(Path.Combine(directory, $"ay-{item.name}-{mode}.pbgra32"), pixels);
            File.WriteAllText(Path.Combine(directory, $"ay-{item.name}-{mode}.json"), JsonSerializer.Serialize(new
            {
                item.name, mode, objectControls = item.points.Select(p => new { p.X, p.Y }),
                matrix = new[] { item.matrix.M11, item.matrix.M12, item.matrix.M21, item.matrix.M22, item.matrix.OffsetX, item.matrix.OffsetY },
                deviceControls = device.Select(p => new { p.X, p.Y }), controls = controls.Select(p => new { p.X, p.Y }),
                output = output.Select(p => new { p.X, p.Y }), quantizedOutput = output.Select(Quantize).Select(p => new { p.X, p.Y }),
                nonempty = pixels.Any(p => p != 0), threshold = 0, hardwareVerified = false
            }));
        }
    }

    internal static void Compare(string root)
    {
        CurveDeviceQuantizationExperiment.Compare(root);
        var boundaries = new List<object>();
        foreach (string file in Directory.GetFiles(Path.Combine(root, "original"), "ay-*-cubic.pbgra32"))
        {
            string name = Path.GetFileName(file)[..^"-cubic.pbgra32".Length];
            byte[] expected = File.ReadAllBytes(file);
            foreach (string role in new[] { "original", "candidate" })
            foreach (string mode in new[] { "cubic", "double", "device-prequantized", "object-prequantized" })
            {
                byte[] actual = File.ReadAllBytes(Path.Combine(root, role, $"{name}-{mode}.pbgra32"));
                var differences = Enumerable.Range(0, expected.Length).Where(i => expected[i] != actual[i])
                    .Select(i => new { offset = i, official = expected[i], actual = actual[i] }).ToArray();
                boundaries.Add(new { name, role, mode, nonempty = expected.Any(p => p != 0), count = differences.Length, differences });
            }
        }
        File.WriteAllText(Path.Combine(root, "ay-boundaries.json"), JsonSerializer.Serialize(boundaries, new JsonSerializerOptions { WriteIndented = true }));
        var results = new List<object>();
        var summary = new List<object>();
        foreach (int frame in new[] { 5, 6, 7, 8, 9 })
        foreach (string role in new[] { "original", "candidate" })
        foreach (string mode in new[] { "double", "prequantized-double", "integer-schedule-exact", "integer" })
        {
            byte[] original = File.ReadAllBytes(Path.Combine(root, "original", $"{frame}.pbgra32"));
            byte[] actual = File.ReadAllBytes(Path.Combine(root, role, $"aq-{frame}-{mode}.pbgra32"));
            var differences = Enumerable.Range(0, original.Length).Where(i => original[i] != actual[i])
                .Select(i => new { offset = i, x = i / 4 % 40, y = i / 160, channel = i % 4, official = original[i], actual = actual[i] }).ToArray();
            results.Add(new { frame, role, mode, count = differences.Length, differences });
            summary.Add(new { frame, role, mode, count = differences.Length });
        }
        File.WriteAllText(Path.Combine(root, "aq-causality.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        File.WriteAllText(Path.Combine(root, "aq-summary.json"), JsonSerializer.Serialize(summary));
    }
}
