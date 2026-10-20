#if CO_FILL_INTEGER_EXPLORATION
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
public sealed class CnFillIntegerIntegrationTests
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>Requires exact real fill pixels and unchanged complete stroke buffers against a current-source baseline.</summary>
    [TestMethod]
    public async Task WhenRealFillUsesIntegerSubdivisionThenIndependentPixelsMatch()
    {
        string root = Environment.GetEnvironmentVariable("CN_FILL_ROOT") ?? Path.GetFullPath(Path.Combine(AcceptancePaths.Results, "cn-integration"));
        string? role = Environment.GetEnvironmentVariable("CN_FILL_ROLE");
        if (role is not null)
        {
            Exception? failure = null;
            var thread = new Thread(() => { try { Render(root, role); } catch (Exception error) { failure = error; } });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(90)));
            if (failure is not null) throw new AssertFailedException(failure.ToString());
            return;
        }
        foreach (string selected in new[] { "official", "baseline", "candidate", "official-repeat" })
        {
            Environment.SetEnvironmentVariable("CN_FILL_ROOT", root);
            Environment.SetEnvironmentVariable("CN_FILL_ROLE", selected);
            int exit;
            try { exit = await ExitDiagnostics.RunAsync([Path.Combine(root, selected), "120", "FullyQualifiedName~CnFillIntegerIntegrationTests"]); }
            finally
            {
                Environment.SetEnvironmentVariable("CN_FILL_ROOT", null);
                Environment.SetEnvironmentVariable("CN_FILL_ROLE", null);
            }
            Assert.AreEqual(0, exit, selected);
        }
        var rows = new List<object>();
        var failures = new List<string>();
        foreach (string file in Directory.GetFiles(Path.Combine(root, "official"), "*.pbgra32").Order())
        {
            string name = Path.GetFileName(file);
            byte[] official = File.ReadAllBytes(file);
            byte[] candidate = File.ReadAllBytes(Path.Combine(root, "candidate", name));
            byte[] baseline = File.ReadAllBytes(Path.Combine(root, "baseline", name));
            byte[] repeat = File.ReadAllBytes(Path.Combine(root, "official-repeat", name));
            int count = Differences(official, candidate), old = Differences(official, baseline), changed = Differences(baseline, candidate);
            int repeatDifference = Differences(official, repeat);
            bool nonempty = new[] { official, candidate, baseline, repeat }.All(p => p.Where((_, i) => i % 4 == 3).Any(b => b != 0));
            bool stroke = name.Contains("stroke", StringComparison.Ordinal);
            bool historical = name is "ellipse-fill-nested.pbgra32" or "rounded-fill-nested.pbgra32";
            bool accepted = nonempty && repeatDifference == 0 && (stroke ? changed == 0 : historical ? changed == 0 || count == 0 : count == 0);
            if (!accepted) failures.Add(name);
            rows.Add(new { name, count, baselineDifference = old, oldNewDifference = changed, repeatDifference, nonempty, accepted,
                officialSha = Hash(official), baselineSha = Hash(baseline), candidateSha = Hash(candidate) });
            Save(Path.Combine(root, name + ".diff.json"), Enumerable.Range(0, official.Length).Where(i => official[i] != candidate[i]).Select(i => new { offset = i, expected = official[i], actual = candidate[i] }));
        }
        Save(Path.Combine(root, "comparison.json"), new { threshold = 0, hardwareVerified = false, exactAcceptance = failures.Count == 0, failures, rows });
        Assert.AreEqual(0, failures.Count, string.Join(", ", failures));
    }

    private static void Render(string root, string role)
    {
        string arch = RuntimeInformation.ProcessArchitecture == Architecture.X86 ? "x86" : "x64";
        string selected = role switch
        {
            "candidate" => AcceptancePaths.AotDll,
            "baseline" => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "baseline-" + arch, "publish", "wpfgfx_cor3.dll")),
            _ => AcceptancePaths.OriginalDll
        };
        nint module = 0;
        foreach (Assembly owner in new[] { Assembly.Load("PresentationCore"), Assembly.Load("WindowsBase") })
            NativeLibrary.SetDllImportResolver(owner, (name, _, _) =>
            {
                if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
                return module == 0 ? module = NativeLibrary.Load(selected) : module;
            });
        string directory = Path.Combine(root, role);
        Directory.CreateDirectory(directory);
        RenderContent(directory);
        foreach (Type renderer in new[] { typeof(BzCurvedPrimitiveTests), typeof(BwCurvedFillPrecisionTests), typeof(BmFillControlPointTests) })
        {
            string output = renderer == typeof(BmFillControlPointTests) ? Path.Combine(directory, "siblings") : directory;
            Directory.CreateDirectory(output);
            (renderer.GetMethod("RenderContent", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException()).Invoke(null, [output]);
            if (output != directory)
                foreach (string file in Directory.GetFiles(output, "*.pbgra32")) File.Copy(file, Path.Combine(directory, "sibling-" + Path.GetFileName(file)), true);
        }
        CurveDeviceQuantizationExperiment.Render(directory);
        Assert.AreNotEqual((nint)0, module);
        string actual = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        Assembly tests = typeof(CnFillIntegerIntegrationTests).Assembly;
        Save(Path.Combine(directory, "identity.json"), new { role, requested = selected, actual, moduleSha = Hash(File.ReadAllBytes(actual)),
            version = FileVersionInfo.GetVersionInfo(actual).FileVersion, testsSha = Hash(File.ReadAllBytes(tests.Location)), testsMvid = tests.ManifestModule.ModuleVersionId,
            runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, hardwareVerified = false });
    }

    private static void RenderContent(string directory)
    {
        foreach (string placement in new[] { "identity", "geometry", "drawing", "visual", "nested" })
        foreach (bool stroke in new[] { false, true })
        {
            var path = new StreamGeometry();
            using (var context = path.Open())
            {
                context.BeginFigure(new Point(4, 28), !stroke, !stroke);
                context.BezierTo(new Point(4.04, 4), new Point(36, 4), new Point(36, 28), true, false);
            }
            var local = new MatrixTransform(new Matrix(0.875, 0.125, -0.0625, 0.75, 2.17, 3.09));
            if (placement is "geometry" or "nested") path.Transform = local;
            var visual = new DrawingVisual();
            if (placement == "visual") visual.Transform = local;
            using (var context = visual.RenderOpen())
            {
                if (placement == "drawing") context.PushTransform(local);
                context.DrawGeometry(stroke ? null : Brushes.Blue, stroke ? new Pen(Brushes.Red, 1.25) : null, path);
                if (placement == "drawing") context.Pop();
            }
            var parent = new ContainerVisual();
            if (placement == "nested") parent.Transform = new MatrixTransform(new Matrix(1.125, -0.0625, 0.125, 0.875, 1.13, 1.07));
            parent.Children.Add(visual);
            var bitmap = new RenderTargetBitmap(40, 32, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(parent);
            byte[] pixels = new byte[5120];
            bitmap.CopyPixels(pixels, 160, 0);
            string name = $"original-{placement}-{(stroke ? "stroke" : "fill")}";
            File.WriteAllBytes(Path.Combine(directory, name + ".pbgra32"), pixels);
            Save(Path.Combine(directory, name + ".json"), new { placement, stroke, local = local.Matrix, width = 40, height = 32, stride = 160 });
        }
    }

    private static int Differences(byte[] a, byte[] b)
    {
        Assert.AreEqual(a.Length, b.Length);
        return a.Zip(b).Count(p => p.First != p.Second);
    }
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static void Save(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value, Options));
}
#endif
