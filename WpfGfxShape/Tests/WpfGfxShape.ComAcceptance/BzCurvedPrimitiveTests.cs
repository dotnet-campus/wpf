using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.ComAcceptance;

internal sealed class BzCurvedPrimitiveTests
{
    /// <summary>Checks finite primitive siblings against independently loaded official content and records stroke identities.</summary>
    internal async Task WhenPrimitiveSiblingsRenderThenFillPrecisionAndStrokeEvidenceAreRetained()
    {
        string? role = Environment.GetEnvironmentVariable("BZ_PRIMITIVE_ROLE");
        if (role is not null)
        {
            Exception? failure = null;
            var thread = new Thread(() => { try { Render(role); } catch (Exception error) { failure = error; } });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(30)));
            if (failure is not null) throw new AssertFailedException(failure.ToString());
            return;
        }
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "bz-siblings");
        foreach (string selected in new[] { "official", "candidate" })
        {
            var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException())
            { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (string argument in new[] { "--filter", "FullyQualifiedName~BzCurvedPrimitiveTests", "--report-trx", "--report-trx-filename", "child.trx", "--results-directory", Path.Combine(root, selected, "TestResults") })
                start.ArgumentList.Add(argument);
            start.Environment["BZ_PRIMITIVE_ROLE"] = selected;
            Directory.CreateDirectory(Path.Combine(root, selected));
            using var process = Process.Start(start) ?? throw new InvalidOperationException();
            Task<string> output = process.StandardOutput.ReadToEndAsync(), error = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { process.Kill(true); await process.WaitForExitAsync(); throw; }
            File.WriteAllText(Path.Combine(root, selected, "stdout.txt"), await output);
            File.WriteAllText(Path.Combine(root, selected, "stderr.txt"), await error);
            File.WriteAllText(Path.Combine(root, selected, "exit.json"), JsonSerializer.Serialize(new { process.ExitCode }));
            Assert.AreEqual(0, process.ExitCode);
        }
        var results = new List<object>();
        bool fillsExact = true;
        foreach (string file in Directory.GetFiles(Path.Combine(root, "official"), "*.pbgra32").Order())
        {
            string name = Path.GetFileName(file);
            byte[] expected = File.ReadAllBytes(file), actual = File.ReadAllBytes(Path.Combine(root, "candidate", name));
            int count = Enumerable.Range(0, expected.Length).Count(i => expected[i] != actual[i]);
            bool nonempty = Nonempty(expected) && Nonempty(actual);
            results.Add(new { name, count, nonempty, officialSha256 = Convert.ToHexString(SHA256.HashData(expected)), candidateSha256 = Convert.ToHexString(SHA256.HashData(actual)) });
            fillsExact &= nonempty && (name.Contains("-stroke-", StringComparison.Ordinal) || count == 0);
        }
        File.WriteAllText(Path.Combine(root, "comparison.json"), JsonSerializer.Serialize(new { threshold = 0, fillsExact, hardwareVerified = false, results }, Options));
        Assert.IsTrue(fillsExact, root);
    }

    private static bool Nonempty(byte[] pixels) => pixels.Where((_, i) => i % 4 == 3).Any(b => b != 0);
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true, IncludeFields = true };

    private static void Render(string role)
    {
        string selected = role == "official" ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = 0;
        foreach (Assembly owner in new[] { Assembly.Load("PresentationCore"), Assembly.Load("WindowsBase") })
            NativeLibrary.SetDllImportResolver(owner, (name, _, _) =>
            {
                if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
                return module == 0 ? module = NativeLibrary.Load(selected) : module;
            });
        string directory = Path.Combine(AppContext.BaseDirectory, "Results", "bz-siblings", role);
        Directory.CreateDirectory(directory);
        RenderContent(directory);
        Assert.AreNotEqual((nint)0, module);
        string actual = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        var assembly = typeof(BzCurvedPrimitiveTests).Assembly;
        File.WriteAllText(Path.Combine(directory, "identity.json"), JsonSerializer.Serialize(new { role, requested = selected, actual, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))), version = FileVersionInfo.GetVersionInfo(actual).FileVersion, testsSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))), testsMvid = assembly.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, hardwareVerified = false }, Options));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void RenderContent(string directory)
    {
        foreach (string kind in new[] { "rectangle", "ellipse", "rounded", "group" })
        foreach (bool nested in new[] { false, true })
        foreach (bool stroke in new[] { false, true })
        {
            var rect = new Rect(4, 4, 24, 16);
            Geometry geometry = kind switch
            {
                "ellipse" => new EllipseGeometry(rect),
                "rounded" => new RectangleGeometry(rect, 4, 4),
                "group" => new GeometryGroup { Children = { new GeometryGroup { Children = { new RectangleGeometry(rect) }, Transform = new TranslateTransform(1, 0) } }, Transform = new TranslateTransform(0, 1) },
                _ => new RectangleGeometry(rect)
            };
            var local = new Matrix(0.875, 0.125, -0.0625, 0.75, 2.17, 3.09);
            var parent = new Matrix(1.125, -0.0625, 0.125, 0.875, 1.13, 1.07);
            if (nested) geometry.Transform = new TransformGroup { Children = { geometry.Transform, new MatrixTransform(local) } };
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                if (nested) context.PushTransform(new MatrixTransform(parent));
                context.DrawGeometry(stroke ? null : Brushes.Blue, stroke ? new Pen(Brushes.Red, 1.25) : null, geometry);
                if (nested) context.Pop();
            }
            var bitmap = new RenderTargetBitmap(40, 32, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            byte[] pixels = new byte[5120];
            bitmap.CopyPixels(pixels, 160, 0);
            string name = $"{kind}-{(stroke ? "stroke" : "fill")}-{(nested ? "nested" : "identity")}";
            File.WriteAllBytes(Path.Combine(directory, name + ".pbgra32"), pixels);
            File.WriteAllText(Path.Combine(directory, name + ".json"), JsonSerializer.Serialize(new { kind, nested, stroke, rect, local, parent, width = 40, height = 32, stride = 160, nonempty = Nonempty(pixels) }, Options));
        }
    }
}
