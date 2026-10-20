using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.ComAcceptance;

internal sealed class BmFillControlPointTests
{
    /// <summary>Preserves independent arc, rounded and stroke controls for the fill consumer experiment.</summary>
    internal async Task WhenFillControlPointsAreQuantizedThenIndependentSiblingsMatch()
    {
        if (Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT") is not null)
        {
            string[] args = Environment.GetCommandLineArgs();
            string role = args[Array.IndexOf(args, "--results-directory") + 1];
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { Render(role); }
                catch (Exception exception) { failure = exception; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(45)));
            if (failure is not null) throw new AssertFailedException(failure.ToString());
            return;
        }
        string root = Path.Combine(AcceptancePaths.Results, "bm-siblings", Guid.NewGuid().ToString("N"));
        int official = await NativeRun.RoleAsync(root, "--bm-fill-control-points", "--results-directory", "original");
        int candidate = await NativeRun.RoleAsync(root, "--bm-fill-control-points", "--results-directory", "candidate");
        Assert.AreEqual(0, official, root);
        Assert.AreEqual(0, candidate, root);
        var results = new List<object>();
        bool exact = true;
        foreach (string file in Directory.GetFiles(Path.Combine(root, "original"), "*.pbgra32").Order())
        {
            string name = Path.GetFileName(file);
            byte[] expected = File.ReadAllBytes(file);
            byte[] actual = File.ReadAllBytes(Path.Combine(root, "candidate", name));
            int[] offsets = Enumerable.Range(0, expected.Length).Where(i => expected[i] != actual[i]).ToArray();
            bool nonempty = expected.Where((_, i) => i % 4 == 3).Any(b => b != 0)
                && actual.Where((_, i) => i % 4 == 3).Any(b => b != 0);
            File.WriteAllText(Path.Combine(root, name + ".differences.json"), JsonSerializer.Serialize(offsets));
            results.Add(new { name, count = offsets.Length, nonempty });
            exact &= offsets.Length == 0 && nonempty;
        }
        File.WriteAllText(Path.Combine(root, "comparison.json"), JsonSerializer.Serialize(new
        {
            threshold = 0, exact, hardwareVerified = false, results
        }));
        Assert.IsTrue(exact, root);
    }

    private static void Render(string role)
    {
        string directory = Path.Combine(NativeRun.PathsRoot, role);
        Directory.CreateDirectory(directory);
        string selected = role == "original" ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = 0;
        foreach (Assembly owner in new[] { Assembly.Load("PresentationCore"), Assembly.Load("WindowsBase") })
            NativeLibrary.SetDllImportResolver(owner, (name, _, _) =>
            {
                if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase)
                    && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
                return module == 0 ? module = NativeLibrary.Load(selected) : module;
            });
        RenderContent(directory);
        Assert.AreNotEqual((nint)0, module);
        string actual = System.Diagnostics.Process.GetCurrentProcess().Modules.Cast<System.Diagnostics.ProcessModule>()
            .Single(m => m.BaseAddress == module).FileName;
        var assembly = typeof(BmFillControlPointTests).Assembly;
        File.WriteAllText(Path.Combine(directory, "identity.json"), JsonSerializer.Serialize(new
        {
            role, requested = selected, path = actual,
            sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))),
            version = System.Diagnostics.FileVersionInfo.GetVersionInfo(actual).FileVersion,
            testsPath = assembly.Location,
            testsSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
            testsMvid = assembly.ManifestModule.ModuleVersionId,
            runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size,
            backend = "RenderTargetBitmap software", hardwareVerified = false
        }));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void RenderContent(string directory)
    {
        foreach (string kind in new[] { "arc", "rounded", "ellipse", "cubic" })
        foreach (bool stroke in new[] { false, true })
        foreach (bool transformed in new[] { false, true })
        {
            Geometry geometry;
            if (kind == "rounded") geometry = new RectangleGeometry(new Rect(4.04, 4.02, 31.91, 23.96), 7.13, 5.09);
            else if (kind == "ellipse") geometry = new EllipseGeometry(new Point(20.04, 16.02), 15.91, 11.96);
            else
            {
                var path = new StreamGeometry();
                using (StreamGeometryContext context = path.Open())
                {
                    context.BeginFigure(new Point(4, 28), !stroke, !stroke);
                    if (kind == "arc") context.ArcTo(new Point(36, 28), new Size(16.04, 23.97), 13.17,
                        false, SweepDirection.Clockwise, true, false);
                    else context.BezierTo(new Point(4.04, 4), new Point(36, 4), new Point(36, 28), true, false);
                }
                geometry = path;
            }
            var local = new Matrix(0.875, 0.125, -0.0625, 0.75, 2.17, 3.09);
            var outer = new Matrix(1.125, -0.0625, 0.125, 0.875, 1.13, 1.07);
            if (transformed) geometry.Transform = new MatrixTransform(local);
            var visual = new DrawingVisual();
            if (transformed) visual.Transform = new MatrixTransform(outer);
            using (DrawingContext context = visual.RenderOpen())
                context.DrawGeometry(stroke ? null : Brushes.Blue, stroke ? new Pen(Brushes.Red, 1.25) : null, geometry);
            var bitmap = new RenderTargetBitmap(40, 32, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            byte[] pixels = new byte[5120];
            bitmap.CopyPixels(pixels, 160, 0);
            string name = $"{kind}-{(stroke ? "stroke" : "fill")}-{(transformed ? "nested" : "identity")}";
            File.WriteAllBytes(Path.Combine(directory, name + ".pbgra32"), pixels);
            File.WriteAllText(Path.Combine(directory, name + ".json"), JsonSerializer.Serialize(new
            {
                kind, stroke, transformed, local, outer, width = 40, height = 32, stride = 160,
                nonempty = pixels.Where((_, i) => i % 4 == 3).Any(b => b != 0)
            }));
        }
    }
}
