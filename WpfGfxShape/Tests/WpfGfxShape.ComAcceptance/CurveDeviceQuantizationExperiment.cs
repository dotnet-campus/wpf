using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.ComAcceptance;

internal static class CurveDeviceQuantizationExperiment
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    internal static void Render(string directory)
    {
        var assembly = typeof(CurveDeviceQuantizationExperiment).Assembly;
        File.WriteAllText(Path.Combine(directory, "bi-experiment-identity.json"), JsonSerializer.Serialize(new
        {
            path = assembly.Location,
            sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(assembly.Location))),
            mvid = assembly.ManifestModule.ModuleVersionId,
            runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            pointerSize = IntPtr.Size
        }, Options));
        RenderSeparators(directory);
        foreach (string name in new[] { "negative-below", "negative-exact", "negative-above", "float-below", "float-above", "geometry", "drawing", "visual", "nested" })
        foreach (bool stroke in new[] { false, true })
        {
            double x = name switch
            {
                "negative-below" => -0.0312501,
                "negative-above" => -0.0312499,
                "float-below" => -0.03125000001,
                "float-above" => -0.03124999999,
                _ => -0.03125
            };
            Point[] points = [new(-2,24), new(x,-4), new(30,-4), new(34,24)];
            bool transformed = name is "geometry" or "drawing" or "visual" or "nested";
            Matrix local = transformed ? new(0.875,0.125,-0.0625,0.75,2.17,3.09) : Matrix.Identity;
            Matrix outer = name == "nested" ? new(1.125,-0.0625,0.125,0.875,1.13,1.07) : Matrix.Identity;
            Matrix combined = local;
            combined.Append(outer);
            foreach (string mode in new[] { "real-cubic", "explicit-cubic", "float-device-cubic", "float-device-polygon", "line-sibling" })
            {
                Point[] device = points.Select(combined.Transform).ToArray();
                Point[] quantized = points.Select(p => Quantize(p, combined)).ToArray();
                bool real = mode is "real-cubic" or "line-sibling";
                Point[] controls = real ? points : mode == "explicit-cubic" ? device : quantized;
                var geometry = new StreamGeometry();
                using (StreamGeometryContext path = geometry.Open())
                {
                    path.BeginFigure(controls[0], !stroke, !stroke);
                    if (mode == "line-sibling")
                        foreach (Point p in controls[1..]) path.LineTo(p, true, false);
                    else if (mode == "float-device-polygon")
                        foreach (Point p in Floating(controls)) path.LineTo(p, true, false);
                    else path.BezierTo(controls[1], controls[2], controls[3], true, false);
                }
                if (real && name is "geometry" or "nested") geometry.Transform = new MatrixTransform(local);
                var visual = new DrawingVisual();
                if (real && name == "visual") visual.Transform = new MatrixTransform(local);
                using (DrawingContext context = visual.RenderOpen())
                {
                    if (real && name == "drawing") context.PushTransform(new MatrixTransform(local));
                    context.DrawGeometry(stroke ? null : Brushes.Blue, stroke ? new Pen(Brushes.Red, 1.25) : null, geometry);
                    if (real && name == "drawing") context.Pop();
                }
                var parent = new ContainerVisual();
                if (real && name == "nested") parent.Transform = new MatrixTransform(outer);
                parent.Children.Add(visual);
                var bitmap = new RenderTargetBitmap(40,32,96,96,PixelFormats.Pbgra32);
                bitmap.Render(parent);
                byte[] pixels = new byte[5120];
                bitmap.CopyPixels(pixels,160,0);
                string stem = $"be-{name}-{(stroke ? "stroke" : "fill")}-{mode}";
                File.WriteAllBytes(Path.Combine(directory, stem + ".pbgra32"),pixels);
                File.WriteAllText(Path.Combine(directory, stem + ".json"),JsonSerializer.Serialize(new
                {
                    name, stroke, mode, objectControls = Coordinates(points), deviceControls = Coordinates(device),
                    floatDeviceControls = Coordinates(quantized),
                    local = Elements(local), outer = Elements(outer), combined = Elements(combined),
                    nonempty = pixels.Any(p => p != 0), hardwareVerified = false
                },Options));
            }
        }
    }

    private static Point FloatTransform(Point p, Matrix m)
    {
        float x1 = (float)p.X * (float)m.M11, x2 = (float)p.Y * (float)m.M21;
        float y1 = (float)p.X * (float)m.M12, y2 = (float)p.Y * (float)m.M22;
        float x = x1 + x2, y = y1 + y2;
        return new((float)(x + (float)m.OffsetX), (float)(y + (float)m.OffsetY));
    }

    private static void RenderSeparators(string directory)
    {
        Matrix local = new(0.875, 0.125, -0.0625, 0.75, 2.17, 3.09);
        Matrix outer = new(1.125, -0.0625, 0.125, 0.875, 1.13, 1.07);
        Matrix combined = local; combined.Append(outer);
        Matrix floatCombined = FloatCompose(local, outer);
        var samples = new List<(string Name, Point Point)>();
        foreach (bool alternatives in new[] { false, true })
        foreach (bool nested in new[] { false, true })
        foreach (bool negative in new[] { true, false })
        {
            int found = 0;
            Matrix search = nested ? combined : local;
            for (int k = negative ? -24 : 8; k < (negative ? -8 : 440) && found < 4; k++)
            {
                double target = (k + 0.5) / 16 + 0.5;
                double x = (target + 4 * search.M21 - search.OffsetX) / search.M11;
                double[] neighbors = Enumerable.Range(-256, 513).Select(i => x + i * 0.00000001).Concat(new[] { Math.BitDecrement(x), x, Math.BitIncrement(x), (double)MathF.BitDecrement((float)x), (float)x, MathF.BitIncrement((float)x) }).ToArray();
                foreach (double value in neighbors)
                {
                    Point p = new(value, -4);
                    Point a = Quantize(local.Transform(p), Matrix.Identity);
                    Point b = Quantize(p, local);
                    Point c = Quantize(p, combined);
                    Point d = Quantize(FloatTransform(p, local), outer);
                    Point e = Quantize(FloatTransform(p, local), Matrix.Identity);
                    Point f = Quantize(p, floatCombined);
                    if (alternatives ? (nested ? d == f : b == e) : (nested ? c == d : a == b)) continue;
                    samples.Add(($"{(alternatives ? (nested ? "df" : "be") : (nested ? "cd" : "ab"))}-{(negative ? "negative" : "positive")}-{found++}", p));
                    break;
                }
            }
            File.AppendAllText(Path.Combine(directory, "separator-search.jsonl"), JsonSerializer.Serialize(new { alternatives, nested, negative, found }) + Environment.NewLine);
        }
        foreach (var sample in samples)
        foreach (string placement in new[] { "geometry", "drawing", "visual", "nested" })
        foreach (bool stroke in new[] { false, true })
        {
            Point[] points = [new(-2,24), sample.Point, new(30,-4), new(34,24)];
            Point[] a = points.Select(p => Quantize(local.Transform(p), Matrix.Identity)).ToArray();
            Point[] b = points.Select(p => Quantize(p, local)).ToArray();
            Point[] c = points.Select(p => Quantize(p, combined)).ToArray();
            Point[] d = points.Select(p => Quantize(FloatTransform(p, local), outer)).ToArray();
            Point[] e = points.Select(p => Quantize(FloatTransform(p, local), Matrix.Identity)).ToArray();
            Point[] f = points.Select(p => Quantize(p, floatCombined)).ToArray();
            string stem = $"separator-{sample.Name}-{placement}-{(stroke ? "stroke" : "fill")}";
            foreach (string mode in new[] { "real", "A", "B", "C", "D", "E", "F", "line" })
            {
                bool real = mode is "real" or "line";
                Point[] controls = mode switch { "A" => a, "B" => b, "C" => c, "D" => d, "E" => e, "F" => f, _ => points };
                var geometry = new StreamGeometry();
                using (var path = geometry.Open())
                {
                    path.BeginFigure(controls[0], !stroke, !stroke);
                    if (mode == "line") foreach (Point p in controls[1..]) path.LineTo(p, true, false);
                    else path.BezierTo(controls[1], controls[2], controls[3], true, false);
                }
                if (real && placement is "geometry" or "nested") geometry.Transform = new MatrixTransform(local);
                var visual = new DrawingVisual();
                if (real && placement == "visual") visual.Transform = new MatrixTransform(local);
                using (var context = visual.RenderOpen())
                {
                    if (real && placement == "drawing") context.PushTransform(new MatrixTransform(local));
                    context.DrawGeometry(stroke ? null : Brushes.Blue, stroke ? new Pen(Brushes.Red, 1.25) : null, geometry);
                    if (real && placement == "drawing") context.Pop();
                }
                var parent = new ContainerVisual();
                if (real && placement == "nested") parent.Transform = new MatrixTransform(outer);
                parent.Children.Add(visual);
                var bitmap = new RenderTargetBitmap(40,32,96,96,PixelFormats.Pbgra32);
                bitmap.Render(parent);
                byte[] pixels = new byte[5120]; bitmap.CopyPixels(pixels,160,0);
                File.WriteAllBytes(Path.Combine(directory, $"{stem}-{mode}.pbgra32"), pixels);
            }
            static string Bits(double v) => $"{BitConverter.DoubleToInt64Bits(v):X16}";
            static string FloatBits(double v) => $"{BitConverter.SingleToInt32Bits((float)v):X8}";
            object Trace(Point p, Matrix m)
            {
                float ax = (float)m.M11 * 16f, cx = (float)m.M21 * 16f;
                float by = (float)m.M12 * 16f, dy = (float)m.M22 * 16f;
                float tx = ((float)m.OffsetX - 0.5f) * 16f, ty = ((float)m.OffsetY - 0.5f) * 16f;
                float x1 = ax * (float)p.X, x2 = cx * (float)p.Y, y1 = by * (float)p.X, y2 = dy * (float)p.Y;
                float xs = x1 + x2, ys = y1 + y2, x = xs + tx, y = ys + ty;
                return new { input = new[] { p.X, p.Y }, doubleBits = new[] { Bits(p.X), Bits(p.Y) }, floatBits = new[] { FloatBits(p.X), FloatBits(p.Y) }, matrix = Elements(m), stages = new[] { ax,cx,by,dy,tx,ty,x1,x2,y1,y2,xs,ys,x,y }.Select(v => new { value = v, bits = FloatBits(v) }), integer = new[] { (int)Math.Floor((double)x + 0.5), (int)Math.Floor((double)y + 0.5) } };
            }
            File.WriteAllText(Path.Combine(directory, stem + ".json"), JsonSerializer.Serialize(new
            {
                sample.Name, placement, stroke, local = Elements(local), outer = Elements(outer), combined = Elements(combined), floatCombined = Elements(floatCombined),
                points = Coordinates(points), A = Coordinates(a), B = Coordinates(b), C = Coordinates(c), D = Coordinates(d), E = Coordinates(e), F = Coordinates(f),
                traces = points.Select(p => new { A = Trace(local.Transform(p), Matrix.Identity), B = Trace(p, local), C = Trace(p, combined), localFloat = Coordinates([FloatTransform(p,local)]), D = Trace(FloatTransform(p, local), outer), E = Trace(FloatTransform(p, local), Matrix.Identity), F = Trace(p, floatCombined) })
            }, Options));
        }
    }

    private static Matrix FloatCompose(Matrix a, Matrix b)
    {
        static float Sum(double x, double y, double z, double w)
        {
            float first = (float)x * (float)y;
            float second = (float)z * (float)w;
            return first + second;
        }
        return new(Sum(a.M11,b.M11,a.M12,b.M21), Sum(a.M11,b.M12,a.M12,b.M22),
            Sum(a.M21,b.M11,a.M22,b.M21), Sum(a.M21,b.M12,a.M22,b.M22),
            (float)(Sum(a.OffsetX,b.M11,a.OffsetY,b.M21) + (float)b.OffsetX),
            (float)(Sum(a.OffsetX,b.M12,a.OffsetY,b.M22) + (float)b.OffsetY));
    }

    private static double[] Elements(Matrix m) => [m.M11,m.M12,m.M21,m.M22,m.OffsetX,m.OffsetY];
    private static double[][] Coordinates(Point[] points) => points.Select(p => new[] {p.X,p.Y}).ToArray();

    private static Point Quantize(Point p, Matrix m)
    {
        float a = (float)m.M11 * 16f, b = (float)m.M12 * 16f;
        float c = (float)m.M21 * 16f, d = (float)m.M22 * 16f;
        float tx = ((float)m.OffsetX - 0.5f) * 16f, ty = ((float)m.OffsetY - 0.5f) * 16f;
        float x = a * (float)p.X + c * (float)p.Y + tx;
        float y = b * (float)p.X + d * (float)p.Y + ty;
        return new(Math.Floor((double)x + 0.5) / 16 + 0.5, Math.Floor((double)y + 0.5) / 16 + 0.5);
    }

    private static Point[] Floating(Point[] p)
    {
        Vector e1 = p[3] - p[0], e2 = 6 * ((Vector)p[1] - 2 * (Vector)p[2] + (Vector)p[3]);
        Vector e3 = 6 * ((Vector)p[0] - 2 * (Vector)p[1] + (Vector)p[2]);
        Point e0 = p[0];
        int steps = 1;
        double dt = 1;
        static double Error(Vector v) => Math.Max(Math.Abs(v.X),Math.Abs(v.Y));
        void Half() { e2 = (e2 + e3) * 0.125; e1 = (e1 - e2) * 0.5; e3 *= 0.25; steps *= 2; dt *= 0.5; }
        while ((Error(e2) > 1.5 || Error(e3) > 1.5) && dt > 1e-3) Half();
        var output = new List<Point>();
        while (steps > 1)
        {
            e0 += e1; Vector old = e2; e1 += old; e2 += old - e3; e3 = old; steps--;
            output.Add(e0);
            if (Error(e2) > 1.5 && dt > 1e-3) Half();
            else while ((steps & 1) == 0)
            {
                Vector doubled = 2 * e2 - e3;
                if (Error(e3) > 0.375 || Error(doubled) > 0.375) break;
                e1 = 2 * e1 + e2; e3 *= 4; e2 = doubled * 4; steps /= 2; dt *= 2;
            }
        }
        output.Add(p[3]);
        return output.ToArray();
    }

    internal static void Compare(string root)
    {
        var separators = new List<object>();
        foreach (string file in Directory.GetFiles(Path.Combine(root, "original"), "separator-*-real.pbgra32"))
        {
            string stem = Path.GetFileName(file)[..^"-real.pbgra32".Length];
            byte[] real = File.ReadAllBytes(file);
            byte[] Read(string role, string mode) => File.ReadAllBytes(Path.Combine(root, role, $"{stem}-{mode}.pbgra32"));
            int Difference(byte[] x, byte[] y) => Enumerable.Range(0,x.Length).Count(i => x[i] != y[i]);
            separators.Add(new { stem, ab = Difference(Read("original","A"), Read("original","B")), cd = Difference(Read("original","C"), Read("original","D")),
                be = Difference(Read("original","B"), Read("original","E")), df = Difference(Read("original","D"), Read("original","F")),
                models = new[] { "A","B","C","D","E","F","real","line" }.Select(mode => new { mode, official = Difference(mode == "line" ? Read("original","line") : real, Read("original",mode)), candidate = Difference(mode == "line" ? Read("original","line") : real, Read("candidate",mode)), nonempty = Read("original",mode).Any(v => v != 0) }).ToArray() });
        }
        File.WriteAllLines(Path.Combine(root,"separator-summary.jsonl"), separators.Select(item => JsonSerializer.Serialize(item)));
        var results = new List<object>();
        var compact = new List<object>();
        foreach (string file in Directory.GetFiles(Path.Combine(root,"original"),"be-*-real-cubic.pbgra32"))
        {
            string stem = Path.GetFileName(file)[..^"-real-cubic.pbgra32".Length];
            byte[] expected = File.ReadAllBytes(file);
            foreach (string role in new[] { "original","candidate" })
            foreach (string mode in new[] { "real-cubic","explicit-cubic","float-device-cubic","float-device-polygon","line-sibling" })
            {
                byte[] actual = File.ReadAllBytes(Path.Combine(root,role,$"{stem}-{mode}.pbgra32"));
                byte[] reference = mode == "line-sibling" ? File.ReadAllBytes(Path.Combine(root,"original",$"{stem}-{mode}.pbgra32")) : expected;
                var differences = Enumerable.Range(0,reference.Length).Where(i => reference[i] != actual[i])
                    .Select(i => new {offset = i, official = reference[i], actual = actual[i]}).ToArray();
                results.Add(new {stem,role,mode,count = differences.Length,nonempty = actual.Any(p => p != 0),differences});
                compact.Add(new {stem,role,mode,count = differences.Length,nonempty = actual.Any(p => p != 0)});
            }
        }
        File.WriteAllText(Path.Combine(root,"be-device-summary.json"),JsonSerializer.Serialize(results,Options));
        File.WriteAllLines(Path.Combine(root,"be-device-compact.jsonl"),compact.Select(item => JsonSerializer.Serialize(item)));
    }
}
