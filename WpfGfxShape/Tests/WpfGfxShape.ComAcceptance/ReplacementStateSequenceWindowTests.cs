using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class ReplacementStateSequenceWindowTests
{
    /// <summary>Compares independent WPF image updates, real window resize and callback teardown without changing shared role dispatch.</summary>
    [TestMethod]
    public async Task WhenWindowImageUpdatesAndResizesThenFreshFramesAndCallbacksConverge()
    {
        const string method = nameof(WhenWindowImageUpdatesAndResizesThenFreshFramesAndCallbacksConverge);
        if (WindowSequenceRoleIsolation.ExecuteRole(method, role => ReplacementStateSequenceWindowProbe.RunIsolated(role))) return;
        string root = Path.Combine(AcceptancePaths.Results, "replacement-state-window", Guid.NewGuid().ToString("N"));
        await WindowSequenceRoleIsolation.RunAsync(root, method, "original");
        await WindowSequenceRoleIsolation.RunAsync(root, method, "candidate");
        WindowSequenceRoleIsolation.Validate(root, method, "original");
        WindowSequenceRoleIsolation.Validate(root, method, "candidate");
        string original = Path.Combine(root, "original");
        string replacement = Path.Combine(root, "candidate");
        string[] files = Directory.GetFiles(original, "*.pbgra32");
        Assert.HasCount(12, files, root);
        foreach (string file in files)
            CollectionAssert.AreEqual(File.ReadAllBytes(file), File.ReadAllBytes(Path.Combine(replacement, Path.GetFileName(file))), file);
        File.WriteAllText(Path.Combine(root, "comparison.json"), JsonSerializer.Serialize(new { frames = files.Length, threshold = 0, exact = true }));
    }

    /// <summary>Compares actual solid rectangle content with an independently loaded official WPF module.</summary>
    [TestMethod]
    public async Task WhenWindowSolidRectangleUpdatesAndResizesThenOfficialContentMatches()
    {
        const string method = nameof(WhenWindowSolidRectangleUpdatesAndResizesThenOfficialContentMatches);
        if (WindowSequenceRoleIsolation.ExecuteRole(method, role => ReplacementStateSequenceWindowProbe.RunIsolated(role, rectangle: true))) return;
        string root = Path.Combine(AcceptancePaths.Results, "replacement-rectangle-window", Guid.NewGuid().ToString("N"));
        await WindowSequenceRoleIsolation.RunAsync(root, method, "original");
        await WindowSequenceRoleIsolation.RunAsync(root, method, "candidate");
        WindowSequenceRoleIsolation.Validate(root, method, "original");
        WindowSequenceRoleIsolation.Validate(root, method, "candidate");
        string[] candidateFrames = Directory.GetFiles(Path.Combine(root, "candidate"), "*.pbgra32");
        var contentEvidence = candidateFrames.Order().Select(file =>
        {
            byte[] pixels = File.ReadAllBytes(file);
            byte[] reference = File.ReadAllBytes(Path.Combine(root, "original", Path.GetFileName(file)));
            return new { file = Path.GetFileName(file), bytes = pixels.Length,
                sha256 = Convert.ToHexString(SHA256.HashData(pixels)),
                exact = pixels.AsSpan().SequenceEqual(reference),
                nonempty = pixels.Any(value => value != 0) };
        }).ToArray();
        File.WriteAllText(Path.Combine(root, "partial-content-evidence.json"), JsonSerializer.Serialize(contentEvidence));
        string[] files = Directory.GetFiles(Path.Combine(root, "original"), "*.pbgra32");
        Assert.HasCount(12, files, root);
        foreach (string file in files)
            CollectionAssert.AreEqual(File.ReadAllBytes(file), File.ReadAllBytes(Path.Combine(root, "candidate", Path.GetFileName(file))), file);
        File.WriteAllText(Path.Combine(root, "comparison.json"), JsonSerializer.Serialize(new { frames = files.Length, threshold = 0, exact = true }));
    }

    /// <summary>Checks explicit same-exe routing under a supervisor output root without creating parent WPF state.</summary>
    [TestMethod]
    public async Task WhenSupervisorOutputRootExistsThenExplicitWindowRolesExecuteInSeparateProcesses()
    {
        const string method = nameof(WhenSupervisorOutputRootExistsThenExplicitWindowRolesExecuteInSeparateProcesses);
        if (WindowSequenceRoleIsolation.ExecuteRole(method, WindowSequenceRoleIsolation.RunControl)) return;
        string? outputRoot = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT");
        Dispatcher? dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
        string[] owners = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name is "PresentationCore" or "WindowsBase").Select(a => a.FullName!).Order().ToArray();
        string root = Path.Combine(NativeRun.PathsRoot, "ka-window-control", Guid.NewGuid().ToString("N"));
        await WindowSequenceRoleIsolation.RunAsync(root, method, "original");
        await WindowSequenceRoleIsolation.RunAsync(root, method, "candidate");
        WindowSequenceRoleIsolation.Validate(root, method, "original");
        WindowSequenceRoleIsolation.Validate(root, method, "candidate");
        Assert.AreEqual(outputRoot, Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT"));
        Assert.IsNull(Environment.GetEnvironmentVariable("KA_WINDOW_SEQUENCE_ROLE"));
        Assert.IsNull(Environment.GetEnvironmentVariable("KA_WINDOW_SEQUENCE_METHOD"));
        CollectionAssert.AreEqual(owners, AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name is "PresentationCore" or "WindowsBase").Select(a => a.FullName!).Order().ToArray());
        File.WriteAllText(Path.Combine(root, "parent-state.json"), JsonSerializer.Serialize(new { pid = Environment.ProcessId, outputRoot, owners, dispatcherExisted = dispatcher is not null, dispatcherShutdown = dispatcher?.HasShutdownStarted, rolesValidated = 2 }));
        Assert.IsFalse(dispatcher?.HasShutdownStarted ?? false);
    }

    /// <summary>Rejects blank and stale frames independently of official/candidate equality.</summary>
    [TestMethod]
    public void WhenFrameIsBlankOrStaleThenContentOracleRejectsIt()
    {
        Assert.IsFalse(ReplacementStateSequenceWindowProbe.IsExpectedFreshFrame(new byte[4], null, Colors.Red));
        Assert.IsFalse(ReplacementStateSequenceWindowProbe.IsExpectedFreshFrame([0, 0, 255, 255], [0, 0, 255, 255], Colors.Red));
        Assert.IsFalse(ReplacementStateSequenceWindowProbe.IsExpectedFreshFrame([], null, Colors.Red));
        Assert.IsTrue(ReplacementStateSequenceWindowProbe.IsExpectedFreshFrame([255, 0, 0, 255], [0, 0, 255, 255], Colors.Blue));
    }
}

internal static class ReplacementStateSequenceWindowProbe
{
    internal static int RunIsolated(string role, bool rectangle = false)
    {
        if (role is not ("original" or "candidate")) return 64;
        int exit = 1;
        var thread = new Thread(() => exit = Run(role, rectangle)) { IsBackground = true, Name = "ReplacementStateWindow" };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!thread.Join(TimeSpan.FromSeconds(60))) return 124;
        return exit;
    }

    private static object Identity(string path)
    {
        FileVersionInfo version = FileVersionInfo.GetVersionInfo(path);
        return new { path, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), version.FileVersion, version.ProductVersion };
    }

    private static int Run(string role, bool rectangle)
    {
        string directory = Path.Combine(NativeRun.PathsRoot, role);
        Directory.CreateDirectory(directory);
        var events = new List<object>();
        var resources = new List<object>();
        var report = new Dictionary<string, object?>
        {
            ["role"] = role, ["architecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
            ["pointerSize"] = IntPtr.Size, ["runtime"] = RuntimeInformation.FrameworkDescription,
            ["events"] = events, ["resources"] = resources,
            ["backend"] = "Real WPF HWND/Image/WriteableBitmap producer; RenderTargetBitmap software content consumer. HWND presentation pixels and HW backend are NOT verified.",
            ["deviceLoss"] = "Not triggered; no simulated loss is accepted as recovery evidence."
        };
        int exit = 1;
        nint module = 0;
        var resolutions = new List<string>();
        try
        {
            Assembly core = Assembly.Load("PresentationCore");
            Assembly windows = Assembly.Load("WindowsBase");
            string selected = role == "original" ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
            report["requestedNative"] = Identity(selected);
            report["officialNative"] = Identity(AcceptancePaths.OriginalDll);
            report["presentationCore"] = Identity(core.Location);
            report["testAssembly"] = Identity(typeof(ReplacementStateSequenceWindowProbe).Assembly.Location);
            foreach (Assembly owner in new[] { core, windows })
                NativeLibrary.SetDllImportResolver(owner, (name, assembly, flags) =>
                {
                    if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
                    if (module == 0) module = NativeLibrary.Load(selected);
                    resolutions.Add(assembly.GetName().Name + ":" + name);
                    return module;
                });
            Environment.SetEnvironmentVariable("WPFGFX_DIAGNOSTIC_LOG", Path.Combine(directory, "uce-diagnostic.log"));
            File.WriteAllText(Path.Combine(directory, "startup-identity.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            for (int cycle = 0; cycle < 3; cycle++)
            {
                WeakReference producer = RunCycle(directory, cycle, events, rectangle);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                using Process process = Process.GetCurrentProcess();
                process.Refresh();
                resources.Add(new { cycle, producerAlive = producer.IsAlive, process.HandleCount, threads = process.Threads.Count, process.PrivateMemorySize64, managedBytes = GC.GetTotalMemory(false) });
                File.WriteAllText(Path.Combine(directory, "resources.json"), JsonSerializer.Serialize(resources));
                if (producer.IsAlive) throw new InvalidOperationException("Detached WriteableBitmap producer remains rooted after close.");
            }
            if (resolutions.Count == 0) throw new InvalidOperationException("WPF did not resolve the selected native module.");
            exit = 0;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException)
        {
            report["failure"] = exception.ToString();
        }
        finally
        {
            report["resolverCalls"] = resolutions;
            using Process process = Process.GetCurrentProcess();
            report["loadedNative"] = process.Modules.Cast<ProcessModule>()
                .Where(item => item.ModuleName.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase)).Select(item => Identity(item.FileName)).ToArray();
            if (module != 0)
            {
                string? actual = process.Modules.Cast<ProcessModule>().FirstOrDefault(item => item.BaseAddress == module)?.FileName;
                report["resolvedNative"] = actual is null ? null : Identity(actual);
                string selected = role == "original" ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
                if (actual is null || !Path.GetFullPath(actual).Equals(Path.GetFullPath(selected), StringComparison.OrdinalIgnoreCase)) exit = 1;
            }
            report["renderTierCapabilityOnly"] = RenderCapability.Tier >> 16;
            Dispatcher.CurrentDispatcher.InvokeShutdown();
            report["dispatcherShutdown"] = Dispatcher.CurrentDispatcher.HasShutdownFinished;
            report["exitCode"] = exit;
            File.WriteAllText(Path.Combine(directory, "report.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        }
        return exit;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RunCycle(string directory, int cycle, List<object> events, bool rectangle)
    {
        var bitmap = new WriteableBitmap(4, 4, 96, 96, PixelFormats.Pbgra32, null);
        var image = new Image { Source = bitmap, Width = 24, Height = 16, Stretch = Stretch.Fill };
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
        var shape = new System.Windows.Shapes.Rectangle { Width = 24, Height = 16, Fill = Brushes.Red };
        FrameworkElement content = rectangle ? shape : image;
        var window = new Window { Content = content, SizeToContent = SizeToContent.WidthAndHeight, WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false, Left = 80, Top = 80 };
        if (rectangle)
        {
            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetBinding(ContentPresenter.ContentProperty, new System.Windows.Data.Binding(nameof(Window.Content))
            {
                RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });
            window.Template = new ControlTemplate(typeof(Window)) { VisualTree = presenter };
        }
        int callbacks = 0, closed = 0;
        EventHandler rendering = (_, _) => callbacks++;
        EventHandler close = (_, _) => closed++;
        window.Closed += close;
        CompositionTarget.Rendering += rendering;
        events.Add(new { cycle, operation = "create" });
        File.WriteAllText(Path.Combine(directory, "events.json"), JsonSerializer.Serialize(events));
        using (Process process = Process.GetCurrentProcess())
            File.WriteAllText(Path.Combine(directory, "loaded-before-show.json"), JsonSerializer.Serialize(process.Modules.Cast<ProcessModule>()
                .Where(item => item.ModuleName.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase)).Select(item => Identity(item.FileName)).ToArray()));
        byte[]? previous = null;
        double initialWidth = 0, initialHeight = 0;
        bool callbacksConverged = false;
        try
        {
            window.Show();
            Color[] colors = [Colors.Red, Colors.Blue, Colors.Lime, Colors.Yellow];
            for (int frame = 0; frame < colors.Length; frame++)
            {
                if (frame == 2)
                {
                    content.Width = 40;
                    content.Height = 24;
                    events.Add(new { cycle, operation = "resize-request", width = 40, height = 24 });
                }
                Color color = colors[frame];
                if (rectangle) shape.Fill = new SolidColorBrush(color);
                byte[] source = new byte[64];
                for (int offset = 0; offset < source.Length; offset += 4)
                {
                    source[offset] = color.B; source[offset + 1] = color.G; source[offset + 2] = color.R; source[offset + 3] = 255;
                }
                bitmap.WritePixels(new Int32Rect(0, 0, 4, 4), source, 16, 0);
                File.WriteAllBytes(Path.Combine(directory, $"source-{cycle}-{frame}.bin"), source);
                events.Add(new { cycle, frame, operation = "resource-update", color = color.ToString() });
                window.UpdateLayout();
                Pump();
                int width = (int)content.Width, height = (int)content.Height;
                var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                target.Render(content);
                byte[] pixels = new byte[width * height * 4];
                target.CopyPixels(pixels, width * 4, 0);
                File.WriteAllBytes(Path.Combine(directory, $"frame-{cycle}-{frame}-{width}x{height}.pbgra32"), pixels);
                events.Add(new { cycle, frame, operation = "frame", width, height, window.ActualWidth, window.ActualHeight, callbacks });
                if (!IsExpectedFreshFrame(pixels, previous, color)) throw new InvalidOperationException("Blank, incorrect or stale content after resource update.");
                if (frame == 0) { initialWidth = window.ActualWidth; initialHeight = window.ActualHeight; }
                if (frame >= 2 && (window.ActualWidth <= initialWidth || window.ActualHeight <= initialHeight)) throw new InvalidOperationException("Actual HWND layout did not grow after resize.");
                previous = pixels;
            }
            if (callbacks == 0) throw new InvalidOperationException("No real CompositionTarget.Rendering callback was observed.");
        }
        finally
        {
            CompositionTarget.Rendering -= rendering;
            image.Source = null;
            window.Content = null;
            window.Close();
            window.Closed -= close;
            int atClose = callbacks;
            Pump();
            events.Add(new { cycle, operation = "close-drained", closed, callbacks, callbacksAtUnsubscribe = atClose });
            File.WriteAllText(Path.Combine(directory, "events.json"), JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true }));
            callbacksConverged = closed == 1 && callbacks == atClose;
        }
        if (!callbacksConverged) throw new InvalidOperationException("Close or callback unsubscription did not converge.");
        return new WeakReference(bitmap);
    }

    private static void Pump()
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(150) };
        EventHandler tick = (_, _) => frame.Continue = false;
        timer.Tick += tick;
        timer.Start();
        try { Dispatcher.PushFrame(frame); }
        finally { timer.Stop(); timer.Tick -= tick; }
    }

    internal static bool IsExpectedFreshFrame(byte[] pixels, byte[]? previous, Color color)
    {
        if (pixels.Length == 0 || pixels.Length % 4 != 0 || previous is not null && pixels.AsSpan().SequenceEqual(previous)) return false;
        for (int offset = 0; offset < pixels.Length; offset += 4)
            if (pixels[offset] != color.B || pixels[offset + 1] != color.G || pixels[offset + 2] != color.R || pixels[offset + 3] != 255) return false;
        return true;
    }
}
