using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class RoundedRectangleContractTests
{
    /// <summary>Compares independent official and candidate rounded content, including corners and a fresh frame.</summary>
    [TestMethod]
    public Task WhenRoundedContentChangesThenOfficialCornersAndFreshPixelsMatch() => CompareAsync(false);

    /// <summary>Preserves the original controls and compares legal control points across a 28.4 rounding boundary.</summary>
    [TestMethod]
    public Task WhenCurveControlPointsCrossQuantizationBoundaryThenIndependentOfficialPixelsMatch() => CompareAsync(true);

    private static async Task CompareAsync(bool experiment)
    {
        string method = experiment ? nameof(WhenCurveControlPointsCrossQuantizationBoundaryThenIndependentOfficialPixelsMatch) : nameof(WhenRoundedContentChangesThenOfficialCornersAndFreshPixelsMatch);
        string? selectedRole = Environment.GetEnvironmentVariable("JM_ROUNDED_ROLE");
        if (selectedRole is not null)
        {
            Assert.IsTrue(selectedRole is "original" or "candidate");
            Assert.AreEqual(method, Environment.GetEnvironmentVariable("JM_ROUNDED_METHOD"));
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { Run(selectedRole, experiment); }
                catch (Exception exception) { failure = exception; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(45)));
            if (failure is not null) throw new AssertFailedException(failure.ToString());
            return;
        }
        string root = Path.GetFullPath(Path.Combine(NativeRun.PathsRoot, experiment ? "curve-an" : "rounded-ah", Guid.NewGuid().ToString("N")));
        await JmResolverIsolation.RunAsync(root, method, "original");
        await JmResolverIsolation.RunAsync(root, method, "candidate");
        var differences = new List<object>();
        bool exact = true;
        for (int frame = 0; frame < (experiment ? 10 : 6); frame++)
        {
            byte[] expected = File.ReadAllBytes(Path.Combine(root, "original", $"{frame}.pbgra32"));
            byte[] actual = File.ReadAllBytes(Path.Combine(root, "candidate", $"{frame}.pbgra32"));
            var bytes = Enumerable.Range(0, expected.Length).Where(i => expected[i] != actual[i])
                .Select(i => new { offset = i, x = i / 4 % 40, y = i / 160, channel = i % 4, official = expected[i], candidate = actual[i] }).ToArray();
            File.WriteAllText(Path.Combine(root, $"difference-{frame}.json"), JsonSerializer.Serialize(new { frame, count = bytes.Length, first = bytes.FirstOrDefault(), bytes }, new JsonSerializerOptions { WriteIndented = true }));
            differences.Add(new { frame, count = bytes.Length, first = bytes.FirstOrDefault() });
            exact &= bytes.Length == 0;
        }
        File.WriteAllText(Path.Combine(root, "comparison.json"), JsonSerializer.Serialize(new { threshold = 0, frames = experiment ? 10 : 6, exact, hardwareVerified = false, differences }));
        if (experiment)
        {
            var controls = new List<object>();
            foreach (string role in new[] { "original", "candidate" })
            {
                byte[] baseline = File.ReadAllBytes(Path.Combine(root, role, "6.pbgra32"));
                for (int frame = 7; frame < 10; frame++)
                {
                    byte[] pixels = File.ReadAllBytes(Path.Combine(root, role, $"{frame}.pbgra32"));
                    var alpha = Enumerable.Range(0, baseline.Length / 4)
                        .Where(i => baseline[i * 4 + 3] != pixels[i * 4 + 3])
                        .Select(i => new { x = i % 40, y = i / 40, baseline = baseline[i * 4 + 3], actual = pixels[i * 4 + 3] }).ToArray();
                    controls.Add(new { role, baselineFrame = 6, frame, count = alpha.Length, alpha });
                }
            }
            File.WriteAllText(Path.Combine(root, "control-alpha.json"), JsonSerializer.Serialize(controls, new JsonSerializerOptions { WriteIndented = true }));
        }
        if (experiment) CurveCausalityExperiment.Compare(root);
        Assert.IsTrue(exact, root);
    }

    private static void Run(string role, bool experiment)
    {
        string directory = Path.Combine(NativeRun.PathsRoot, role);
        Directory.CreateDirectory(directory);
        string selected = role == "original" ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = 0;
        foreach (Assembly owner in new[] { Assembly.Load("PresentationCore"), Assembly.Load("WindowsBase") })
            NativeLibrary.SetDllImportResolver(owner, (name, _, _) =>
            {
                if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
                return module == 0 ? module = NativeLibrary.Load(selected) : module;
            });
        Render(directory, experiment);
        if (experiment) CurveCausalityExperiment.Render(directory);
        Assert.AreNotEqual((nint)0, module);
        string actual = System.Diagnostics.Process.GetCurrentProcess().Modules.Cast<System.Diagnostics.ProcessModule>()
            .Single(m => m.BaseAddress == module).FileName;
        File.WriteAllText(Path.Combine(directory, "identity.json"), JsonSerializer.Serialize(new
        {
            role, pid = Environment.ProcessId, testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(RoundedRectangleContractTests).Assembly.Location))),
            testMvid = typeof(RoundedRectangleContractTests).Assembly.ManifestModule.ModuleVersionId,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(), runtime = RuntimeInformation.FrameworkDescription,
            requested = selected, path = actual, version = System.Diagnostics.FileVersionInfo.GetVersionInfo(actual).FileVersion,
            sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))),
            backend = "RenderTargetBitmap software contract control only; NOT window GPU evidence", hardwareVerified = false
        }));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void Render(string directory, bool experiment)
    {
        byte[]? previous = null;
        for (int frame = 0; frame < (experiment ? 10 : 6); frame++)
        {
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                if (frame >= 6)
                {
                    double controlX = frame switch { 6 => 4, 7 => 4.001, 8 => 4.02, _ => 4.04 };
                    var geometry = new StreamGeometry();
                    using (StreamGeometryContext path = geometry.Open())
                    {
                        path.BeginFigure(new Point(4, 28), true, true);
                        path.BezierTo(new Point(controlX, 4), new Point(36, 4), new Point(36, 28), true, false);
                    }
                    context.DrawGeometry(frame % 2 == 0 ? Brushes.Red : Brushes.Blue, null, geometry);
                    File.WriteAllText(Path.Combine(directory, $"parameters-{frame}.json"), JsonSerializer.Serialize(new
                    {
                        start = new { x = 4, y = 28 }, first = new { x = controlX, y = 4 },
                        second = new { x = 36, y = 4 }, end = new { x = 36, y = 28 },
                        quantizedFirstX = Math.Floor((controlX - 0.5) * 16 + 0.5),
                        purpose = "Legal cubic; frames 6/7/8 share 28.4 controls; frame 9 crosses the rounding boundary. Compare alpha independently of alternating color."
                    }));
                }
                else if (frame < 2)
                    context.DrawRoundedRectangle(frame == 0 ? Brushes.Red : Brushes.Blue, null, new Rect(4, 4, 32, 24), frame == 0 ? 8 : -80, frame == 0 ? 6 : 60);
                else if (frame == 2)
                    context.DrawRectangle(Brushes.Red, null, new Rect(4, 4, 32, 24));
                else if (frame == 3)
                    context.DrawEllipse(Brushes.Blue, null, new Point(20, 16), 16, 12);
                else
                {
                    var geometry = new StreamGeometry();
                    using (StreamGeometryContext path = geometry.Open())
                    {
                        const double k = 0.5522847498307933984;
                        double rx = frame == 4 ? 8 : 16, ry = frame == 4 ? 6 : 12;
                        path.BeginFigure(new Point(4, 4 + ry), true, true);
                        path.BezierTo(new Point(4, 4 + ry * (1 - k)), new Point(4 + rx * (1 - k), 4), new Point(4 + rx, 4), true, false);
                        path.LineTo(new Point(36 - rx, 4), true, false);
                        path.BezierTo(new Point(36 - rx * (1 - k), 4), new Point(36, 4 + ry * (1 - k)), new Point(36, 4 + ry), true, false);
                        path.LineTo(new Point(36, 28 - ry), true, false);
                        path.BezierTo(new Point(36, 28 - ry * (1 - k)), new Point(36 - rx * (1 - k), 28), new Point(36 - rx, 28), true, false);
                        path.LineTo(new Point(4 + rx, 28), true, false);
                        path.BezierTo(new Point(4 + rx * (1 - k), 28), new Point(4, 28 - ry * (1 - k)), new Point(4, 28 - ry), true, false);
                    }
                    context.DrawGeometry(frame == 4 ? Brushes.Red : Brushes.Blue, null, geometry);
                }
            }
            var bitmap = new RenderTargetBitmap(40, 32, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            byte[] pixels = new byte[40 * 32 * 4];
            bitmap.CopyPixels(pixels, 160, 0);
            File.WriteAllBytes(Path.Combine(directory, $"{frame}.pbgra32"), pixels);
            Assert.AreEqual(frame == 2 ? (byte)255 : (byte)0, pixels[(4 * 40 + 4) * 4 + 3], "Corner control.");
            Assert.AreEqual((byte)255, pixels[(16 * 40 + 20) * 4 + 3], "Interior must be nonempty.");
            Assert.AreEqual((byte)255, pixels[(16 * 40 + 20) * 4 + (frame is 0 or 2 or 4 or 6 or 8 ? 2 : 0)], "Fresh frame color.");
            if (previous is not null) Assert.IsFalse(previous.AsSpan().SequenceEqual(pixels), "Stale frame.");
            previous = pixels;
        }
    }
}
