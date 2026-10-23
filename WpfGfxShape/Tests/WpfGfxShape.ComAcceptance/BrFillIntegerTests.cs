// Portions adapted from .NET WPF bezier.h and bezier.cpp under the MIT license.
// See LICENSE.TXT in the repository root.
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class BrFillIntegerTests
{
    /// <summary>Checks one integer control polygon against an independently rasterized official cubic.</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void WhenIntegerSubdivisionIsRasterizedThenOfficialCubicPixelsMatch(int sample)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { Compare(sample); }
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(30)));
        if (failure is not null) throw new AssertFailedException(failure.ToString());
    }

    private static void Compare(int sample)
    {
        Point[][] samples =
        [
            [new(4,28), new(4.04,4), new(36,4), new(36,28)],
            [new(4,28), new(14,24), new(24,24), new(36,28)],
            [new(-2,24), new(-0.03125,-4), new(30,-4), new(34,24)],
            [new(4,28), new(-1024,-2048), new(2048,-1024), new(36,28)],
            [new(4,28), new(4.03125,4), new(35.96875,4), new(36,28)]
        ];
        Point[] source = samples[sample];
        // The rasterizer applies the half-pixel translation before rounding to 28.4.
        IntPoint[] controls = source.Select(p => new IntPoint(
            (int)Math.Floor(((float)p.X * 16f - 8f) + 0.5f),
            (int)Math.Floor(((float)p.Y * 16f - 8f) + 0.5f))).ToArray();
        List<IntPoint> trace = Flatten(controls);
        Point[] polygon = new[] { controls[0] }.Concat(trace)
            .Select(p => new Point(p.X / 16d + 0.5, p.Y / 16d + 0.5)).ToArray();
        byte[] cubic = Render(source, true), lines = Render(polygon, false);
        int[] differences = Enumerable.Range(0, cubic.Length).Where(i => cubic[i] != lines[i]).ToArray();
        string root = Path.Combine(AcceptancePaths.Results, "br-integer-oracle", sample.ToString(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllBytes(Path.Combine(root, "official-cubic.pbgra32"), cubic);
        File.WriteAllBytes(Path.Combine(root, "integer-lines.pbgra32"), lines);
        var assembly = typeof(BrFillIntegerTests).Assembly;
        using Process process = Process.GetCurrentProcess();
        ProcessModule officialModule = OfficialRasterModule.Find(process);
        string official = officialModule.FileName;
        File.WriteAllText(Path.Combine(root, "evidence.json"), JsonSerializer.Serialize(new
        {
            sample, controls, trace, differences, threshold = 0,
            nonempty = cubic.Where((_, i) => i % 4 == 3).Any(b => b != 0),
            assembly = assembly.Location,
            sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
            mvid = assembly.ManifestModule.ModuleVersionId,
            official, officialBaseAddress = officialModule.BaseAddress.ToString("X"), officialSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(official))),
            officialVersion = FileVersionInfo.GetVersionInfo(official).FileVersion,
            runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(), hardwareVerified = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.IsTrue(cubic.Where((_, i) => i % 4 == 3).Any(b => b != 0), root);
        Assert.IsTrue(lines.Where((_, i) => i % 4 == 3).Any(b => b != 0), root);
        Assert.AreEqual(0, differences.Length, root);
    }

    private static byte[] Render(Point[] points, bool cubic)
    {
        var geometry = new StreamGeometry();
        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(points[0], true, true);
            if (cubic) context.BezierTo(points[1], points[2], points[3], true, false);
            else foreach (Point point in points.Skip(1)) context.LineTo(point, true, false);
        }
        var visual = new DrawingVisual();
        using (DrawingContext context = visual.RenderOpen()) context.DrawGeometry(Brushes.Blue, null, geometry);
        var bitmap = new RenderTargetBitmap(40, 32, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        byte[] result = new byte[40 * 32 * 4];
        bitmap.CopyPixels(result, 160, 0);
        return result;
    }

    private readonly record struct IntPoint(int X, int Y);

    private sealed class Basis
    {
        internal long E0, E1, E2, E3;
        internal Basis(int a, int b, int c, int d, int fraction)
        {
            E0 = (long)a << fraction;
            E1 = ((long)d - a) << fraction;
            E2 = (6L * (b - 2L * c + d)) << fraction;
            E3 = (6L * (a - 2L * b + c)) << fraction;
        }
        internal long Error => Math.Max(Math.Abs(E2), Math.Abs(E3));
        internal long Parent => Math.Max(Math.Abs(E3), Math.Abs(2 * E2 - E3));
        internal void Half() { E2 = (E2 + E3) >> 3; E1 = (E1 - E2) >> 1; E3 >>= 2; }
        internal void Double() { E1 = 2 * E1 + E2; E3 <<= 2; E2 = (E2 << 3) - E3; }
        internal void Step() { E0 += E1; long old = E2; E1 += old; E2 += old - E3; E3 = old; }
        internal int Value(int fraction) => (int)((E0 + (1L << (fraction - 1))) >> fraction);
        internal int[] Controls()
        {
            long b = E0 + (6 * E1 - E2 - 2 * E3) / 18;
            long c = E0 + (12 * E1 - 2 * E2 - E3) / 18;
            return new[] { E0, b, c, E0 + E1 }.Select(v => (int)((v + (1L << 27)) >> 28)).ToArray();
        }
    }

    private static List<IntPoint> Flatten(IntPoint[] p)
    {
        int ox = p.Min(v => v.X), oy = p.Min(v => v.Y);
        var x = new Basis(p[0].X - ox, p[1].X - ox, p[2].X - ox, p[3].X - ox, 10);
        var y = new Basis(p[0].Y - oy, p[1].Y - oy, p[2].Y - oy, p[3].Y - oy, 10);
        bool narrow = p.Max(v => v.X) - ox < 16384 && p.Max(v => v.Y) - oy < 16384
            && x.Error < (1536L << 10) && y.Error < (1536L << 10);
        var output = new List<IntPoint>();
        if (narrow)
        {
            int shift = 0, steps = 1;
            while (Math.Max(x.Error, y.Error) > (24L << 10) << shift)
            {
                shift += 2;
                foreach (Basis axis in new[] { x, y })
                { axis.E2 = (axis.E2 + axis.E3) >> 1; axis.E1 = (axis.E1 - (axis.E2 >> shift)) >> 1; }
                steps <<= 1;
            }
            foreach (Basis axis in new[] { x, y })
            {
                axis.E0 <<= 3; axis.E1 <<= 3;
                if (shift < 3) { axis.E2 <<= 3 - shift; axis.E3 <<= 3 - shift; }
                else { axis.E2 >>= shift - 3; axis.E3 >>= shift - 3; }
            }
            x.Step(); y.Step(); steps--;
            while (true)
            {
                output.Add(new(x.Value(13) + ox, y.Value(13) + oy));
                if (steps == 0) break;
                Adjust(x, y, ref steps, 24L << 13);
                steps--; x.Step(); y.Step();
            }
        }
        else
        {
            const long highTolerance = 196608L << 28;
            const long lowTolerance = 3L << 31;
            var hx = new Basis(p[0].X, p[1].X, p[2].X, p[3].X, 28);
            var hy = new Basis(p[0].Y, p[1].Y, p[2].Y, p[3].Y, 28);
            int highSteps = 1;
            while (Math.Max(hx.Error, hy.Error) > highTolerance)
            { hx.Half(); hy.Half(); highSteps <<= 1; }
            do
            {
                int[] cx = hx.Controls(), cy = hy.Controls();
                x = new(cx[0], cx[1], cx[2], cx[3], 28);
                y = new(cy[0], cy[1], cy[2], cy[3], 28);
                int lowSteps = 1;
                while (Math.Max(x.Error, y.Error) > lowTolerance)
                { x.Half(); y.Half(); lowSteps <<= 1; }
                if (--highSteps != 0)
                { hx.Step(); hy.Step(); Adjust(hx, hy, ref highSteps, highTolerance); }
                do
                {
                    x.Step(); y.Step(); output.Add(new(x.Value(28), y.Value(28)));
                    lowSteps--;
                    if (lowSteps == 0) break;
                    Adjust(x, y, ref lowSteps, lowTolerance);
                } while (true);
            } while (highSteps != 0);
        }
        if (output[^1] != p[3]) output.Add(p[3]);
        return output;
    }

    private static void Adjust(Basis x, Basis y, ref int steps, long tolerance)
    {
        if (Math.Max(x.Error, y.Error) > tolerance)
        { x.Half(); y.Half(); steps <<= 1; }
        while ((steps & 1) == 0 && Math.Max(x.Parent, y.Parent) <= tolerance / 4)
        { x.Double(); y.Double(); steps >>= 1; }
    }
}
