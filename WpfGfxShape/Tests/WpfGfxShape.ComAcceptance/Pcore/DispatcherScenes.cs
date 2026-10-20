namespace NativeAcceptance.Pcore;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

internal static partial class Program
{
    private static int DispatcherAcceptance(string role)
    {
        string destination = Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "Results");
        Directory.CreateDirectory(destination);
        if (role == "compare-dispatcher") return CompareDispatchers(destination);
        string directory = Path.Combine(destination, role);
        Directory.CreateDirectory(directory);
        var records = new ConcurrentQueue<object>();
        var failures = new ConcurrentQueue<string>();
        var resolution = new List<object>();
        string native = "";
        object? pcIdentity = null;
        object? nativeIdentity = null;
        int joined = 0;
        try
        {
            Assembly pc = Assembly.Load("PresentationCore");
            Assembly wb = Assembly.Load("WindowsBase");
            pcIdentity = Identity(pc.Location);
            native = role == "original" ? WpfGfxShape.ComAcceptance.AcceptancePaths.OriginalDll
                : WpfGfxShape.ComAcceptance.AcceptancePaths.AotDll;
            nativeIdentity = Identity(native);
            object gate = new();
            nint loaded = 0;
            foreach (Assembly owner in new[] { pc, wb })
                NativeLibrary.SetDllImportResolver(owner, (name, assembly, flags) =>
                {
                    if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase)
                        && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
                    lock (gate)
                    {
                        if (loaded == 0)
                        {
                            loaded = NativeLibrary.Load(native);
                            ApplyProtection(loaded);
                        }
                        resolution.Add(new { name, owner = assembly.FullName, thread = Environment.CurrentManagedThreadId });
                        return loaded;
                    }
                });
            RunDispatchers(directory, records, failures, ref joined);
            string[] modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Where(item => item.ModuleName.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase))
                .Select(item => item.FileName).ToArray();
            string service = Path.Combine(Path.GetDirectoryName(pc.Location)!, "wpfgfx_cor3.dll");
            if (resolution.Count == 0 || !modules.Any(path => Path.GetFullPath(path).Equals(Path.GetFullPath(native), StringComparison.OrdinalIgnoreCase))
                || modules.Any(path => !Path.GetFullPath(path).Equals(Path.GetFullPath(native), StringComparison.OrdinalIgnoreCase)
                    && !Path.GetFullPath(path).Equals(Path.GetFullPath(service), StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Unexpected native module identity.");
            records.Enqueue(new { operation = "modules", modules = modules.Select(Identity).ToArray(), residentUntilProcessExit = true });
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException)
        {
            failures.Enqueue(exception.ToString());
        }
        bool passed = failures.IsEmpty && joined == 6;
        File.WriteAllText(Path.Combine(directory, "dispatcher.json"), JsonSerializer.Serialize(new
        {
            caseId = "E2E-09-concurrent-dispatcher-finite-v1", role, passed, joined,
            runtime = RuntimeInformation.FrameworkDescription, os = RuntimeInformation.OSDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(), presentationCore = pcIdentity,
            requestedNative = nativeIdentity, harness = Identity(typeof(Program).Assembly.Location),
            resolutions = resolution, records = records.ToArray(), failures = failures.ToArray(),
            target1012Accepted = false, allE2E09Accepted = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        return passed ? 0 : 1;
    }

    private static void RunDispatchers(string directory, ConcurrentQueue<object> records, ConcurrentQueue<string> failures, ref int joined)
    {
        var sharedBrush = new SolidColorBrush(Colors.Red);
        sharedBrush.Freeze();
        byte[] data = Enumerable.Repeat(new byte[] { 255, 0, 0, 255 }, 16).SelectMany(value => value).ToArray();
        BitmapSource sharedImage = BitmapSource.Create(4, 4, 96, 96, PixelFormats.Pbgra32, null, data, 16);
        sharedImage.Freeze();
        var sharedDrawing = new GeometryDrawing(sharedBrush, null, new RectangleGeometry(new Rect(0, 0, 8, 8)));
        sharedDrawing.Freeze();
        var affineBrush = new SolidColorBrush(Colors.Blue);
        var affineVisual = new DrawingVisual();
        for (int cycle = 0; cycle < 2; cycle++)
        {
            using var ready = new CountdownEvent(3);
            using var start = new ManualResetEventSlim();
            var threads = new List<Thread>();
            for (int worker = 0; worker < 3; worker++)
            {
                int index = worker, generation = cycle;
                var thread = new Thread(() =>
                {
                    Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
                    int callbacks = 0, stoppedCallbacks = 0;
                    bool shutdownStarted = false, shutdownFinished = false;
                    dispatcher.ShutdownStarted += (_, _) => shutdownStarted = true;
                    dispatcher.ShutdownFinished += (_, _) => shutdownFinished = true;
                    ready.Signal();
                    try
                    {
                        if (!start.Wait(TimeSpan.FromSeconds(20))) throw new TimeoutException("Concurrent start.");
                        bool brushRejected = false, visualRejected = false, createRejected = false;
                        try { affineBrush.Color = Colors.Green; }
                        catch (InvalidOperationException) { brushRejected = true; }
                        try { using DrawingContext context = affineVisual.RenderOpen(); }
                        catch (InvalidOperationException) { visualRejected = true; }
                        try { _ = new RenderTargetBitmap(0, 32, 96, 96, PixelFormats.Pbgra32); }
                        catch (ArgumentException) { createRejected = true; }
                        if (!brushRejected || !visualRejected || !createRejected)
                            throw new InvalidOperationException("Thread affinity or invalid creation was accepted.");
                        var visual = new DrawingVisual();
                        var local = new SolidColorBrush(Colors.Green);
                        var blur = new BlurEffect { Radius = 2, KernelType = KernelType.Gaussian };
                        var shadow = new DropShadowEffect { BlurRadius = 2, ShadowDepth = 1, Opacity = 0.5, Color = Colors.Black };
                        for (int frame = 0; frame < 6; frame++)
                        {
                            local.Color = frame % 2 == 0 ? Colors.Green : Colors.Blue;
                            visual.Effect = frame % 3 == 0 ? null : frame % 3 == 1 ? blur : shadow;
                            blur.Radius = frame % 2 + 1;
                            using (DrawingContext context = visual.RenderOpen())
                            {
                                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, 32, 32));
                                context.DrawRectangle(sharedBrush, null, new Rect(2, 2, 8, 8));
                                context.DrawDrawing(sharedDrawing);
                                context.DrawImage(sharedImage, new Rect(16, 2, 8, 8));
                                context.DrawRectangle(local, null, new Rect(4, 18, 16, 8));
                            }
                            var target = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
                            target.Render(visual);
                            byte[] pixels = new byte[4096];
                            target.CopyPixels(pixels, 128, 0);
                            if (pixels.All(value => value == 0)) throw new InvalidOperationException("Empty concurrent frame.");
                            string name = $"dispatcher-{generation}-{index}-{frame}.pbgra32";
                            File.WriteAllBytes(Path.Combine(directory, name), pixels);
                            records.Enqueue(new { operation = "frame", generation, worker = index, frame, name,
                                sha256 = Convert.ToHexString(SHA256.HashData(pixels)), thread = Environment.CurrentManagedThreadId });
                            target.Clear();
                        }
                        visual.Effect = null;
                        using (DrawingContext context = visual.RenderOpen()) { }
                        string asset = Path.Combine(directory, $"dispatcher-{generation}-{index}.avi");
                        WriteMediaAsset(asset, 0);
                        var player = new MediaPlayer();
                        var failed = new MediaPlayer();
                        Exception? mediaError = null;
                        int opened = 0, failedCount = 0;
                        player.MediaOpened += (_, _) => { callbacks++; opened++; player.Close(); };
                        player.MediaFailed += (_, e) => mediaError = e.ErrorException;
                        failed.MediaFailed += (_, _) => { callbacks++; failedCount++; failed.Close(); };
                        try
                        {
                            player.Open(new Uri(asset));
                            failed.Open(new Uri(Path.Combine(directory, $"missing-{generation}-{index}.avi")));
                            WaitMedia(() => opened == 1 && failedCount == 1, () => mediaError, "concurrent callback close");
                        }
                        finally { player.Close(); failed.Close(); }
                        stoppedCallbacks = callbacks;
                        dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                        Dispatcher.Run();
                        if (!shutdownStarted || !shutdownFinished || !dispatcher.HasShutdownFinished)
                            throw new InvalidOperationException("Dispatcher shutdown incomplete.");
                        DispatcherOperation rejected = dispatcher.InvokeAsync(() => throw new InvalidOperationException("Post-shutdown callback ran."));
                        if (rejected.Status != DispatcherOperationStatus.Aborted || callbacks != stoppedCallbacks)
                            throw new InvalidOperationException("Callbacks not quiescent after shutdown.");
                        records.Enqueue(new { operation = "lifecycle", generation, worker = index,
                            brushRejected, visualRejected, createRejected, opened, failedCount, callbacks,
                            shutdownStarted, shutdownFinished, postShutdownOperation = rejected.Status.ToString(),
                            thread = Environment.CurrentManagedThreadId });
                    }
                    catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException)
                    {
                        failures.Enqueue($"cycle={generation};worker={index}: {exception}");
                    }
                    finally
                    {
                        if (!dispatcher.HasShutdownFinished) dispatcher.InvokeShutdown();
                    }
                });
                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                threads.Add(thread);
                thread.Start();
            }
            if (!ready.Wait(TimeSpan.FromSeconds(20))) throw new TimeoutException("Dispatcher readiness.");
            start.Set();
            foreach (Thread thread in threads)
            {
                if (!thread.Join(TimeSpan.FromSeconds(60))) throw new TimeoutException("Dispatcher thread did not terminate.");
                if (thread.IsAlive) throw new InvalidOperationException("Joined Dispatcher thread remains alive.");
                joined++;
            }
            records.Enqueue(new { operation = "joined", cycle, threads = threads.Count, allStopped = threads.All(thread => !thread.IsAlive) });
        }
        Dispatcher.CurrentDispatcher.InvokeShutdown();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static int CompareDispatchers(string destination)
    {
        var comparisons = new List<object>();
        bool passed = true;
        foreach (int cycle in new[] { 0, 1 })
            foreach (int worker in new[] { 0, 1, 2 })
                foreach (int frame in new[] { 0, 1, 2, 3, 4, 5 })
                {
                    string name = $"dispatcher-{cycle}-{worker}-{frame}.pbgra32";
                    byte[] original = File.ReadAllBytes(Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "Baseline", name));
                    byte[] candidate = File.ReadAllBytes(Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "ComparisonCandidate", name));
                    bool equal = original.Length == 4096 && original.AsSpan().SequenceEqual(candidate);
                    passed &= equal;
                    comparisons.Add(new { name, equal, bytes = original.Length, differingBytes = original.Length == candidate.Length
                        ? original.Zip(candidate).Count(pair => pair.First != pair.Second) : -1,
                        originalSha256 = Convert.ToHexString(SHA256.HashData(original)), candidateSha256 = Convert.ToHexString(SHA256.HashData(candidate)) });
                }
        foreach (string folder in new[] { "Baseline", "ComparisonCandidate" })
        {
            using JsonDocument report = JsonDocument.Parse(File.ReadAllText(Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, folder, "dispatcher.json")));
            passed &= report.RootElement.GetProperty("passed").GetBoolean() && report.RootElement.GetProperty("joined").GetInt32() == 6;
        }
        File.WriteAllText(Path.Combine(destination, "dispatcher-comparison.json"), JsonSerializer.Serialize(new
        { passed, threshold = 0, comparisons }, new JsonSerializerOptions { WriteIndented = true }));
        return passed ? 0 : 1;
    }
}
