namespace NativeAcceptance.Pcore;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

internal static partial class Program
{
    private static readonly Dictionary<string, object?> Evidence = new();
    private static readonly List<object> Resolutions = new();
    private static string output = "";
    private static string stage = "startup";
    private static string selected = "";
    private static nint module;
    private static bool mediaScenario;
    private static string protectionMode = "default";

    private static unsafe void ApplyProtection(nint loaded)
    {
        if (protectionMode == "default") return;
        var set = (delegate* unmanaged[Stdcall]<int, void>)NativeLibrary.GetExport(loaded, "WpfGfx_SetDisableBoundsCheckProtection");
        set(protectionMode == "true" ? 1 : 0);
        Evidence["protectionMode"] = protectionMode;
    }

    [STAThread]
    internal static int Run(string[] args)
    {
        if (args.Length == 3)
        {
            protectionMode = args[2];
            args = args[..2];
            if (protectionMode != "default")
                AppContext.SetSwitch("Switch.MS.Internal.WpfGfx.DisableWpfGfxBoundsCheckProtection", protectionMode == "true");
        }
        string role = args.Length is 1 or 2 ? args[0] : "invalid";
        if (role == "compare-dispatcher" || args.Length == 2 && args[1] == "dispatcher" && role is "original" or "candidate")
            return DispatcherAcceptance(role);
        mediaScenario = args.Length == 2 && args[1] == "media";
        if (args.Length == 2 && args[1] == "blur" || role == "compare-blur") NestedCases = BlurCases;
        if (args.Length == 2 && args[1] == "lifetime" || role == "compare-lifetime") NestedCases = LifetimeCases;
        if (args.Length == 2 && args[1] == "shadow" || role == "compare-shadow") NestedCases = DropShadowCases;
        if (role == "compare-media") return CompareMedia();
        if (role == "compare-shadow") return CompareNested();
        if (role == "compare-lifetime") return CompareNested();
        if (role == "compare-blur") return CompareNested();
        if (role == "compare-nested") return CompareNested();
        if (role == "compare-images") return CompareImages();
        if (role == "compare") return CompareFiles();
        bool images = args.Length == 2 && args[1] == "images";
        if (role is not ("original" or "candidate")) return 64;
        output = Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "Results", role);
        Directory.CreateDirectory(output);
        Environment.SetEnvironmentVariable("WPFGFX_DIAGNOSTIC_LOG", Path.Combine(output, "uce-diagnostic.log"));
        Evidence["schemaVersion"] = 1;
        Evidence["caseId"] = "E2E-05-fixed-geometry-v1";
        Evidence["role"] = role;
        Evidence["startUtc"] = DateTimeOffset.UtcNow;
        Evidence["architecture"] = RuntimeInformation.ProcessArchitecture.ToString();
        Evidence["runtime"] = RuntimeInformation.FrameworkDescription;
        Evidence["os"] = RuntimeInformation.OSDescription;
        Evidence["resolverCalls"] = Resolutions;
        Evidence["input"] = new { width = 32, height = 32, dpi = 96, format = "Pbgra32", threshold = 0, seed = 0 };
        try
        {
            Assembly pc = Assembly.Load("PresentationCore");
            Assembly wb = Assembly.Load("WindowsBase");
            Evidence["presentationCore"] = Identity(pc.Location);
            Evidence["windowsBase"] = Identity(wb.Location);
            string original = WpfGfxShape.ComAcceptance.AcceptancePaths.OriginalDll;
            selected = role == "original" ? original : WpfGfxShape.ComAcceptance.AcceptancePaths.AotDll;
            Evidence["requestedNative"] = Identity(selected);
            Evidence["originalRuntimeNative"] = Identity(original);
            Evidence["frozenCandidate"] = Identity(WpfGfxShape.ComAcceptance.AcceptancePaths.AotDll);
            foreach (Assembly owner in new[] { pc, wb })
                NativeLibrary.SetDllImportResolver(owner, Resolve);
            Checkpoint("resolver-installed-before-WPF");
            if (args.Length == 2 && args[1] == "cache-contract") RenderPublicCacheContract();
            else if (args.Length == 2 && args[1] == "shaderbounds") RenderShaderBounds();
            else if (args.Length == 2 && args[1] == "media") RenderMedia();
            else if (args.Length == 2 && args[1] == "shadow") RenderDropShadow();
            else if (args.Length == 2 && args[1] == "lifecycle-sequence") RenderLifecycleSequence();
            else if (args.Length == 2 && args[1] == "lifetime") RenderLifetime();
            else if (args.Length == 2 && args[1] is "nested" or "blur") RenderNested();
            else if (images) RenderImages();
            else Render();
            Checkpoint("render-and-copy-complete");
            VerifyLoaded();
            if (Resolutions.Count == 0) throw new InvalidOperationException("No WPF native resolver invocation was observed.");
            Evidence["comparisonStatus"] = role == "candidate" ? "PendingIndependentComparator" : "OriginalOnly";
            Checkpoint("dispatcher-shutdown");
            Dispatcher.CurrentDispatcher.InvokeShutdown();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Evidence["cleanup"] = "DrawingContext disposed; bitmap/visual references released; Dispatcher shutdown; finalizers drained; native module retained until process exit";
            Evidence["status"] = "Green";
            Evidence["exitCode"] = 0;
            Checkpoint("completed");
            return 0;
        }
        catch (Exception exception)
        {
            Evidence["status"] = "Red";
            Evidence["exitCode"] = 1;
            Evidence["failureStage"] = stage;
            Evidence["exception"] = exception.ToString();
            Evidence["hresult"] = $"0x{exception.HResult:X8}";
            VerifyLoaded(false);
            try
            {
                Dispatcher.CurrentDispatcher.InvokeShutdown();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Evidence["failureCleanup"] = "Dispatcher shutdown and finalizers completed";
            }
            catch (InvalidOperationException cleanupException)
            {
                Evidence["failureCleanupException"] = cleanupException.ToString();
            }
            Checkpoint("failed");
            return 1;
        }
    }

    private static nint Resolve(string name, Assembly owner, DllImportSearchPath? flags)
    {
        if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
        string operation = stage;
        Resolutions.Add(new { name, owner = owner.FullName, selected, flags = flags?.ToString(), operation });
        Checkpoint("WPF-resolver-before-native-load");
        if (module == 0)
        {
            module = NativeLibrary.Load(selected);
            ApplyProtection(module);
        }
        // Enumerate modules after rendering, not inside a loader callback while media loads dependencies.
        Checkpoint("WPF-resolver-native-loaded");
        stage = operation;
        return module;
    }

    private static void VerifyLoaded(bool require = true)
    {
        var loaded = new List<object>();
        foreach (ProcessModule item in Process.GetCurrentProcess().Modules)
        {
            if (!item.ModuleName.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase)) continue;
            string service = Path.Combine(Path.GetDirectoryName(typeof(MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll");
            if (mediaScenario && !selected.Equals(service, StringComparison.OrdinalIgnoreCase)
                && Path.GetFullPath(item.FileName).Equals(Path.GetFullPath(service), StringComparison.OrdinalIgnoreCase))
            {
                Evidence["systemMediaServiceModule"] = Identity(item.FileName);
                continue;
            }
            loaded.Add(Identity(item.FileName));
            if (!Path.GetFullPath(item.FileName).Equals(Path.GetFullPath(selected), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Unexpected WPF native module: {item.FileName}");
        }
        Evidence["actualNativeModules"] = loaded;
        if (require && loaded.Count != 1) throw new InvalidOperationException($"Expected one native WPF module, found {loaded.Count}.");
    }

    private static object Identity(string path)
    {
        using FileStream stream = File.OpenRead(path);
        FileVersionInfo version = FileVersionInfo.GetVersionInfo(path);
        return new { path = Path.GetFullPath(path), sha256 = Convert.ToHexString(SHA256.HashData(stream)), length = stream.Length, fileVersion = version.FileVersion, productVersion = version.ProductVersion };
    }

    private static void Checkpoint(string value)
    {
        stage = value;
        Evidence["stage"] = stage;
        Evidence["lastCheckpointUtc"] = DateTimeOffset.UtcNow;
        File.WriteAllText(Path.Combine(output, "result.json"), JsonSerializer.Serialize(Evidence, new JsonSerializerOptions { WriteIndented = true }));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Render()
    {
        Checkpoint("DrawingVisual-construction");
        var visual = new DrawingVisual();
        Checkpoint("DrawingContext-open");
        using (DrawingContext dc = visual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, 32, 32));
            dc.DrawRectangle(Brushes.Red, null, new Rect(2, 2, 12, 10));
            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)), null, new Rect(8, 5, 12, 12));
            dc.PushTransform(new TranslateTransform(3, 2));
            dc.DrawGeometry(Brushes.Green, new Pen(Brushes.Black, 1), Geometry.Parse("M 1,22 L 10,18 18,27 Z"));
            dc.DrawLine(new Pen(Brushes.Black, 2), new Point(20, 2), new Point(25, 15));
            dc.Pop();
        }
        Checkpoint("RenderTargetBitmap-construction");
        var bitmap = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
        Checkpoint("RenderTargetBitmap-Render");
        bitmap.Render(visual);
        Checkpoint("CopyPixels");
        byte[] pixels = new byte[32 * 32 * 4];
        bitmap.CopyPixels(pixels, 32 * 4, 0);
        File.WriteAllBytes(Path.Combine(output, "pixels.pbgra32"), pixels);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (FileStream image = File.Create(Path.Combine(output, "image.png"))) encoder.Save(image);
        Evidence["pixelsSha256"] = Convert.ToHexString(SHA256.HashData(pixels));
        Evidence["pixelByteCount"] = pixels.Length;
        if (!pixels.AsSpan(0, 4).SequenceEqual(new byte[] { 255, 255, 255, 255 }))
            throw new InvalidOperationException("Original scene background is not white.");
        int red = (3 * 32 + 3) * 4;
        if (!pixels.AsSpan(red, 4).SequenceEqual(new byte[] { 0, 0, 255, 255 }))
            throw new InvalidOperationException("Scene red rectangle was not rendered.");
        bitmap.Clear();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RenderImages()
    {
        Evidence["caseId"] = "E2E-05-image-brush-family-v1";
        var results = new List<object>();
        foreach (string name in ImageCases)
        {
            try
            {
                Checkpoint(name + "-produce");
                byte[] source = new byte[8 * 8 * 4];
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                    {
                        int i = (y * 8 + x) * 4;
                        source[i] = (byte)(x < 4 ? 0 : 255);
                        source[i + 1] = (byte)(y < 4 ? 0 : 255);
                        source[i + 2] = (byte)(x < 4 ? 255 : 0);
                        source[i + 3] = 255;
                    }
                BitmapSource image = BitmapSource.Create(8, 8, 96, 96, PixelFormats.Pbgra32, null, source, 32);
                image.Freeze();
                var drawing = new ImageDrawing(image, new Rect(0, 0, 8, 8));
                drawing.Freeze();
                var child = new DrawingVisual();
                using (DrawingContext context = child.RenderOpen()) context.DrawImage(image, new Rect(0, 0, 8, 8));
                TileBrush brush = name.StartsWith("visual", StringComparison.Ordinal) ? new VisualBrush(child)
                    : name.StartsWith("drawing", StringComparison.Ordinal) ? new DrawingBrush(drawing) : new ImageBrush(image);
                brush.ViewportUnits = BrushMappingMode.Absolute;
                brush.Viewport = new Rect(0, 0, 16, 16);
                brush.ViewboxUnits = BrushMappingMode.Absolute;
                brush.Viewbox = new Rect(0, 0, 8, 8);
                brush.Stretch = Stretch.Fill;
                if (name.Contains("tile", StringComparison.Ordinal)) brush.TileMode = TileMode.FlipXY;
                if (name.Contains("matrix", StringComparison.Ordinal)) brush.Transform = new MatrixTransform(1, 0, 0, 1, 2, 3);
                if (name.Contains("opacity", StringComparison.Ordinal)) brush.Opacity = 0.5;
                if (brush.CanFreeze) brush.Freeze();
                var visual = new DrawingVisual();
                RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.NearestNeighbor);
                using (DrawingContext dc = visual.RenderOpen())
                {
                    dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, 32, 32));
                    if (name.Contains("clip", StringComparison.Ordinal))
                    {
                        dc.PushClip(new RectangleGeometry(new Rect(2, 2, 26, 26)));
                        dc.PushClip(new RectangleGeometry(new Rect(4, 3, 22, 24)));
                    }
                    if (name == "bitmap-direct") dc.DrawImage(image, new Rect(2, 2, 24, 24));
                    else dc.DrawRectangle(brush, null, new Rect(0, 0, 32, 32));
                    if (name.Contains("clip", StringComparison.Ordinal)) { dc.Pop(); dc.Pop(); }
                }
                Checkpoint(name + "-render");
                var target = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
                target.Render(visual);
                byte[] pixels = new byte[4096];
                target.CopyPixels(pixels, 128, 0);
                int opaquePixels = 0;
                int coloredPixels = 0;
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    if (pixels[i + 3] == 255) opaquePixels++;
                    if (pixels[i + 3] != 0 && (pixels[i] != pixels[i + 1] || pixels[i + 1] != pixels[i + 2])) coloredPixels++;
                }
                if (opaquePixels != 1024 || coloredPixels == 0)
                    throw new InvalidOperationException("Image content must contain opaque colored pixels over the white background.");
                File.WriteAllBytes(Path.Combine(output, name + ".pbgra32"), pixels);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(target));
                using (FileStream file = File.Create(Path.Combine(output, name + ".png"))) encoder.Save(file);
                results.Add(new { name, status = "Green", sha256 = Convert.ToHexString(SHA256.HashData(pixels)), bytes = pixels.Length, opaquePixels, coloredPixels });
                target.Clear();
            }
            catch (Exception exception)
            {
                results.Add(new { name, status = "Red", exception = exception.ToString() });
            }
        }
        Evidence["imageScenes"] = results;
        File.WriteAllText(Path.Combine(output, "image-scenes.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static readonly string[] ImageCases = ["bitmap-direct", "image-basic", "image-tile", "image-matrix-clip-opacity", "drawing-basic", "drawing-tile", "drawing-matrix-clip-opacity", "visual-basic", "visual-tile", "visual-matrix-clip-opacity"];

    private static int CompareImages()
    {
        ValidateBaseline(typeof(System.Windows.Media.Visual).Assembly.Location);
        string root = WpfGfxShape.ComAcceptance.NativeRun.PathsRoot;
        var results = new List<object>();
        bool passed = true;
        foreach (string name in ImageCases)
        {
            string original = Path.Combine(root, "Baseline", name + ".pbgra32");
            string candidate = Path.Combine(root, "ComparisonCandidate", name + ".pbgra32");
            byte[] a = File.Exists(original) ? File.ReadAllBytes(original) : [];
            byte[] b = File.Exists(candidate) ? File.ReadAllBytes(candidate) : [];
            bool equal = a.Length == 4096 && a.AsSpan().SequenceEqual(b);
            passed &= equal;
            results.Add(new { name, status = equal ? "Green" : "Red", threshold = 0, originalBytes = a.Length, candidateBytes = b.Length, differingBytes = a.Length == b.Length ? a.Zip(b).Count(p => p.First != p.Second) : -1, originalSha256 = Convert.ToHexString(SHA256.HashData(a)), candidateSha256 = Convert.ToHexString(SHA256.HashData(b)) });
        }
        string destination = Path.Combine(root, "Results", "diff");
        Directory.CreateDirectory(destination);
        File.WriteAllText(Path.Combine(destination, "image-comparison.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        return passed ? 0 : 1;
    }

    private static void ValidateBaseline(string pcPath)
    {
        using JsonDocument baseline = JsonDocument.Parse(File.ReadAllText(Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "Baseline", "result.json")));
        var oracle = baseline.RootElement;
        using var current = JsonDocument.Parse(JsonSerializer.Serialize(new { width = 32, height = 32, dpi = 96, format = "Pbgra32", threshold = 0, seed = 0 }));
        if (oracle.GetProperty("schemaVersion").GetInt32() != 1
            || oracle.GetProperty("role").GetString() != "original"
            || oracle.GetProperty("runtime").GetString() != RuntimeInformation.FrameworkDescription
            || !JsonElement.DeepEquals(oracle.GetProperty("input"), current.RootElement))
            throw new InvalidOperationException("Original schema, input or runtime differs.");
        if (baseline.RootElement.GetProperty("status").GetString() != "Green")
            throw new InvalidOperationException("Original did not pass.");
        using FileStream stream = File.OpenRead(pcPath);
        if (baseline.RootElement.GetProperty("presentationCore").GetProperty("sha256").GetString() != Convert.ToHexString(SHA256.HashData(stream)))
            throw new InvalidOperationException("PresentationCore differs from original process.");
    }

    private static int CompareFiles()
    {
        ValidateBaseline(typeof(System.Windows.Media.Visual).Assembly.Location);
        string root = Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "Results");
        Directory.CreateDirectory(Path.Combine(root, "diff"));
        string originalPath = Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "Baseline", "pixels.pbgra32");
        string candidatePath = Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "ComparisonCandidate", "pixels.pbgra32");
        bool available = File.Exists(candidatePath);
        byte[] original = File.ReadAllBytes(originalPath);
        byte[] candidate = available ? File.ReadAllBytes(candidatePath) : Array.Empty<byte>();
        bool equal = available && original.AsSpan().SequenceEqual(candidate);
        File.WriteAllText(Path.Combine(root, "diff", "comparison.json"), JsonSerializer.Serialize(new
        {
            role = "Comparator", status = equal ? "Green" : "Red", rule = "byte-exact", threshold = 0,
            originalSha256 = Convert.ToHexString(SHA256.HashData(original)),
            candidateSha256 = available ? Convert.ToHexString(SHA256.HashData(candidate)) : null,
            reason = available ? (equal ? "Equal" : "PixelMismatch") : "CandidateFailedBeforeCopyPixels",
            nativeModulesLoaded = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase)).Select(m => m.FileName).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true }));
        return equal ? 0 : 1;
    }
}
