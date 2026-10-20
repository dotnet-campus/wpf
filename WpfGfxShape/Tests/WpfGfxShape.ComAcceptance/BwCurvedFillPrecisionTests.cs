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
public sealed class BwCurvedFillPrecisionTests
{
    /// <summary>Checks current rectangle/ellipse output against independent repeated official rendering at zero threshold.</summary>
    [TestMethod]
    public async Task WhenNarrowPrimitivesAreTransformedThenIndependentOfficialContentMatches()
    {
        string? role = Environment.GetEnvironmentVariable("BW_PRECISION_ROLE");
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
        string root = Path.Combine(AppContext.BaseDirectory, "artifacts", "bw-curved-fill", "precision");
        Directory.CreateDirectory(root);
        foreach (string selected in new[] { "official", "candidate", "official-repeat" })
        {
            var start = Start("--filter", "FullyQualifiedName~BwCurvedFillPrecisionTests.WhenNarrowPrimitivesAreTransformedThenIndependentOfficialContentMatches", "--report-trx", "--report-trx-filename", "child.trx", "--results-directory", Path.Combine(root, selected, "TestResults"));
            start.Environment["BW_PRECISION_ROLE"] = selected;
            start.Environment["BW_PRECISION_ROOT"] = root;
            int exit = await RunAsync(start, Path.Combine(root, selected, "supervision.json"));
            Assert.AreEqual(0, exit, selected);
        }
        var results = new List<object>();
        bool exact = true;
        foreach (string kind in new[] { "rectangle", "ellipse" })
        {
            byte[] official = File.ReadAllBytes(Path.Combine(root, "official", kind + ".pbgra32"));
            foreach (string selected in new[] { "candidate", "official-repeat" })
            {
                byte[] actual = File.ReadAllBytes(Path.Combine(root, selected, kind + ".pbgra32"));
                int count = Differences(official, actual);
                bool nonempty = Nonempty(official) && Nonempty(actual);
                results.Add(new { kind, selected, count, nonempty });
                exact &= count == 0 && nonempty;
            }
        }
        File.WriteAllText(Path.Combine(root, "comparison.json"), JsonSerializer.Serialize(new { threshold = 0, exact, hardwareVerified = false, results }, JsonOptions));
        Assert.IsTrue(exact, root);
    }

    private static ProcessStartInfo Start(params string[] arguments)
    {
        var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException()) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = AppContext.BaseDirectory };
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        return start;
    }

    private static async Task<int> RunAsync(ProcessStartInfo start, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var process = Process.Start(start) ?? throw new InvalidOperationException();
        Task<string> output = process.StandardOutput.ReadToEndAsync(), error = process.StandardError.ReadToEndAsync();
        bool timedOut = false;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(100));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { timedOut = true; process.Kill(true); await process.WaitForExitAsync(); }
        File.WriteAllText(path + ".stdout.txt", await output);
        File.WriteAllText(path + ".stderr.txt", await error);
        var assembly = typeof(BwCurvedFillPrecisionTests).Assembly;
        File.WriteAllText(path, JsonSerializer.Serialize(new { process.ExitCode, timedOut, arguments = start.ArgumentList.ToArray(), testsSha256 = Hash(assembly.Location), testsMvid = assembly.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString() }, JsonOptions));
        Assert.IsFalse(timedOut, path);
        return process.ExitCode;
    }

    private static void Render(string role)
    {
        string selected = role == "candidate" ? AcceptancePaths.AotDll : AcceptancePaths.OriginalDll;
        nint module = 0;
        foreach (Assembly owner in new[] { Assembly.Load("PresentationCore"), Assembly.Load("WindowsBase") })
            NativeLibrary.SetDllImportResolver(owner, (name, _, _) =>
            {
                if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
                return module == 0 ? module = NativeLibrary.Load(selected) : module;
            });
        string directory = Path.Combine(Environment.GetEnvironmentVariable("BW_PRECISION_ROOT") ?? throw new InvalidOperationException(), role);
        Directory.CreateDirectory(directory);
        RenderContent(directory);
        Assert.AreNotEqual((nint)0, module);
        string actual = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        var assembly = typeof(BwCurvedFillPrecisionTests).Assembly;
        File.WriteAllText(Path.Combine(directory, "identity.json"), JsonSerializer.Serialize(new { role, requested = selected, actual, sha256 = Hash(actual), version = FileVersionInfo.GetVersionInfo(actual).FileVersion, testsSha256 = Hash(assembly.Location), testsMvid = assembly.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, backend = "RenderTargetBitmap software", hardwareVerified = false }, JsonOptions));
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void RenderContent(string directory)
    {
        foreach (string kind in new[] { "rectangle", "ellipse" })
        {
            // Retain the discriminating X parameters; increase height and translate into the target to avoid empty output.
            var rect = new Rect(1, 1, Math.ScaleB(1d, -20), 16);
            var local = new Matrix(1 + Math.ScaleB(1d, -24), 0, 0, 1, -1, 0);
            var parent = new Matrix(Math.ScaleB(1d, 20), 0, 0, 1, 8, 0);
            Geometry geometry = kind == "rectangle" ? new RectangleGeometry(rect) : new EllipseGeometry(rect);
            geometry.Transform = new MatrixTransform(local);
            var visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.PushTransform(new MatrixTransform(parent));
                context.DrawGeometry(Brushes.Blue, null, geometry);
                context.Pop();
            }
            var bitmap = new RenderTargetBitmap(32, 24, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            byte[] pixels = new byte[32 * 24 * 4];
            bitmap.CopyPixels(pixels, 128, 0);
            File.WriteAllBytes(Path.Combine(directory, kind + ".pbgra32"), pixels);
            File.WriteAllText(Path.Combine(directory, kind + ".json"), JsonSerializer.Serialize(new { kind, rect = new { rect.X, rect.Y, rect.Width, rect.Height }, local = new { local.M11, local.M12, local.M21, local.M22, local.OffsetX, local.OffsetY }, parent = new { parent.M11, parent.M12, parent.M21, parent.M22, parent.OffsetX, parent.OffsetY }, width = 32, height = 24, stride = 128, nonempty = Nonempty(pixels), alpha = pixels.Where((_, i) => i % 4 == 3).ToArray() }, JsonOptions));
            Assert.IsTrue(Nonempty(pixels), directory + "/" + kind);
        }
    }

    private static bool Nonempty(byte[] pixels) => pixels.Where((_, i) => i % 4 == 3).Any(b => b != 0);
    private static int Differences(byte[] a, byte[] b) { Assert.AreEqual(a.Length, b.Length); return Enumerable.Range(0, a.Length).Count(i => a[i] != b[i]); }
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static JsonSerializerOptions JsonOptions => new() { WriteIndented = true };
}
