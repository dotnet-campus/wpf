using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfGfxShape.Core;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class CkFillIntegerTests
{
    /// <summary>Compares the reconstructed implementation with real official cubic rasterization.</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    public void WhenReconstructedIntegerFillIsRasterizedThenOfficialPixelsMatch(int sample)
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

    /// <summary>Checks ties toward positive infinity independently of curve flattening.</summary>
    [TestMethod]
    [DataRow(-1.5f, -1)]
    [DataRow(-0.5f, 0)]
    [DataRow(0.5f, 1)]
    public void WhenCoordinateIsHalfIntegerThenRoundShiftsUp(float value, int expected)
    {
        Assert.IsTrue(SoftwareFillBezier.TryRound(value, out int actual) && actual == expected);
    }

    private static void Compare(int sample)
    {
        Point[][] samples =
        [
            [new(4,28), new(4.04,4), new(36,4), new(36,28)],
            [new(4,28), new(14,24), new(24,24), new(36,28)],
            [new(-2,24), new(-0.03125,-4), new(30,-4), new(34,24)],
            [new(4,28), new(-1024,-2048), new(2048,-1024), new(36,28)],
            [new(4,28), new(4.03125,4), new(35.96875,4), new(36,28)],
            [new(4,28), new(14,28), new(24,28), new(36,28)],
            [new(4,28), new(-32768,-65536), new(65536,-32768), new(36,28)]
        ];
        Point[] source = samples[sample];
        var controls = new SoftwareFillBezier.Point[4];
        for (int i = 0; i < 4; i++)
        {
            Assert.IsTrue(SoftwareFillBezier.TryRound((float)source[i].X * 16f - 8f, out int x));
            Assert.IsTrue(SoftwareFillBezier.TryRound((float)source[i].Y * 16f - 8f, out int y));
            controls[i] = new(x, y);
        }
        List<SoftwareFillBezier.Point> trace = SoftwareFillBezier.Flatten(controls);
        Point[] polygon = new[] { controls[0] }.Concat(trace)
            .Select(p => new Point(p.X / 16d + 0.5, p.Y / 16d + 0.5)).ToArray();
        byte[] cubic = Render(source, true), lines = Render(polygon, false);
        int differences = cubic.Zip(lines).Count(pair => pair.First != pair.Second);
        bool cubicNonempty = cubic.Where((_, i) => i % 4 == 3).Any(b => b != 0);
        bool linesNonempty = lines.Where((_, i) => i % 4 == 3).Any(b => b != 0);
        string root = Path.Combine(AcceptancePaths.Results, "ck-fill-integer", "oracle", sample.ToString());
        Directory.CreateDirectory(root);
        File.WriteAllBytes(Path.Combine(root, "official-cubic.pbgra32"), cubic);
        File.WriteAllBytes(Path.Combine(root, "reconstructed-lines.pbgra32"), lines);
        using Process process = Process.GetCurrentProcess();
        ProcessModule officialModule = OfficialRasterModule.Find(process);
        string module = officialModule.FileName;
        var assembly = typeof(CkFillIntegerTests).Assembly;
        File.WriteAllText(Path.Combine(root, "evidence.json"), JsonSerializer.Serialize(new
        {
            sample, controls, trace, differences, threshold = 0, cubicNonempty, linesNonempty,
            width = 40, height = 32, stride = 160,
            implementation = "linked production SoftwareFillBezier; no production entry hookup",
            assembly = assembly.Location, assemblySha256 = Hash(assembly.Location),
            mvid = assembly.ManifestModule.ModuleVersionId,
            module, moduleBaseAddress = officialModule.BaseAddress.ToString("X"), moduleSha256 = Hash(module), moduleVersion = FileVersionInfo.GetVersionInfo(module).FileVersion,
            runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(), hardwareVerified = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.IsTrue(cubicNonempty, root);
        Assert.IsTrue(linesNonempty, root);
        Assert.AreEqual(0, differences, root);
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static byte[] Render(Point[] points, bool cubic)
    {
        var geometry = new StreamGeometry();
        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(points[0], true, true);
            if (cubic) context.BezierTo(points[1], points[2], points[3], true, false);
            else foreach (Point point in points.Skip(1)) context.LineTo(point, true, false);
            // Give the zero-curvature termination case a nonempty closed interior.
            context.LineTo(new Point(20, 31), true, false);
        }
        var visual = new DrawingVisual();
        using (DrawingContext context = visual.RenderOpen()) context.DrawGeometry(Brushes.Blue, null, geometry);
        var bitmap = new RenderTargetBitmap(40, 32, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        byte[] result = new byte[40 * 32 * 4];
        bitmap.CopyPixels(result, 160, 0);
        return result;
    }
}
