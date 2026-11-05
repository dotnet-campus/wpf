#if VIDEO_MP4_SANDBOX_DIAGNOSTIC
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed class UserVideoMp4Tests
{
    /// <summary>Displays only independent MF output; does not certify the candidate player ABI or audio.</summary>
    [TestMethod]
    public void WhenUserMp4IsDecodedThenTimestampClockUpdatesDiagnosticWindowAndCloses()
    {
        string root = Path.Combine(NativeRun.PathsRoot, "user-video-mp4");
        Directory.CreateDirectory(root);
        string input = Path.Combine(AppContext.BaseDirectory, "inputs", "Video.mp4");
        using var source = File.OpenRead(input);
        string sha = Convert.ToHexString(SHA256.HashData(source));
        long length = source.Length;
        source.Close();
        var frames = new List<object>();
        MediaFoundationSourceReader? reader = null;
        Window? window = null;
        WriteableBitmap? bitmap = null;
        Dispatcher? dispatcher = null;
        Exception? uiError = null;
        int renderEvents = 0, updates = 0, decoded = 0;
        string stopReason = "Failed";
        bool visible = false, closed = false, eos = false, partial = false, completed = false;
        long first = -1, last = -1, limit = 150000000;
        var clock = new Stopwatch();
        using var ready = new ManualResetEventSlim();
        var ui = new Thread(() =>
        {
            try
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                CompositionTarget.Rendering += OnRendering;
                ready.Set();
                Dispatcher.Run();
            }
            catch (Exception error) { uiError = error; ready.Set(); }
            finally { CompositionTarget.Rendering -= OnRendering; }
        });
        ui.SetApartmentState(ApartmentState.STA);
        ui.IsBackground = true;
        void OnRendering(object? sender, EventArgs args) => Interlocked.Increment(ref renderEvents);
        void Save() => File.WriteAllText(Path.Combine(root, "result.json"), JsonSerializer.Serialize(new
        {
            completed, boundary = "Diagnostic WPF shell showing independent MF frames; not candidate MediaPlayer ABI; no audio output",
            input = "inputs/Video.mp4", inputSha256 = sha, inputLength = length,
            contract = "Nonempty frames; increasing sample timestamps; bounded timestamp clock; visible HWND and bitmap updates; static content permitted; EOS required only for complete source",
            maxPlayback100ns = limit, firstTimestamp100ns = first, lastTimestamp100ns = last,
            elapsedSeconds = clock.Elapsed.TotalSeconds, stopReason, eos, partial, decoded, frameCount = frames.Count, updates, renderEvents, visible, closed,
            renderEvidenceBoundary = "UI WritePixels and Rendering events, not physical scanout",
            testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(UserVideoMp4Tests).Assembly.Location))),
            uiThreadExited = !ui.IsAlive, uiError = uiError?.ToString(),
            reader?.Width, reader?.Height, reader?.Stride, reader?.Subtype, reader?.FrameRate,
            reader?.SourceMetadata, reader?.Negotiation, reader?.Formats,
            reader?.DisplayX, reader?.DisplayY, reader?.DisplayWidth, reader?.DisplayHeight, reader?.DisplayAreaSource, reader?.Cleanup,
            reader?.UnlockCount, reader?.SampleReleaseCount, reader?.BufferReleaseCount,
            reader?.ReaderFinalReferences, reader?.ShutdownCount, reader?.UninitializeCount,
            runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            testMvid = typeof(UserVideoMp4Tests).Assembly.ManifestModule.ModuleVersionId,
            modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Where(m => m.ModuleName?.StartsWith("mf", StringComparison.OrdinalIgnoreCase) == true || m.ModuleName?.Contains("decoder", StringComparison.OrdinalIgnoreCase) == true)
                .Select(m => new { m.FileName, version = m.FileVersionInfo.FileVersion }).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true }));
        Save();
        try
        {
            Assert.AreEqual(0, MediaFoundationSourceReader.Create(input, out reader, enableCompressedVideo: true));
            Assert.IsNotNull(reader);
            Assert.IsTrue(reader.SourceMetadata.TryGetValue("duration100ns", out string? durationText));
            long duration = long.Parse(durationText!, System.Globalization.CultureInfo.InvariantCulture);
            Assert.IsTrue(duration > 0);
            limit = Math.Min(limit, duration);
            // Save the metadata-derived bound before any sample is read.
            Save();
            ui.Start();
            Assert.IsTrue(ready.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsNull(uiError);
            Assert.IsNotNull(dispatcher);
            dispatcher.Invoke(() =>
            {
                bitmap = new WriteableBitmap(reader.DisplayWidth, reader.DisplayHeight, 96, 96, PixelFormats.Bgr32, null);
                window = new Window { Width = 800, Height = 500, Content = new Image { Source = bitmap, Stretch = Stretch.Uniform }, ShowActivated = true };
                window.Show();
                visible = window.IsVisible && IsWindowVisible(new WindowInteropHelper(window).Handle);
            });
            using var frameLog = new StreamWriter(Path.Combine(root, "frames.jsonl"));
            clock.Start();
            for (int i = 0; i < 4096 && !eos; i++)
            {
                Assert.IsTrue(clock.Elapsed < TimeSpan.FromSeconds(45));
                int hr = reader.Read(out uint stream, out uint flags, out long timestamp, out byte[]? pixels);
                Assert.AreEqual(0, hr);
                eos = (flags & 2) != 0;
                if (pixels is null) { Assert.IsTrue(eos || (flags & 0x130) != 0); continue; }
                decoded++;
                if (first < 0) first = timestamp;
                long due = timestamp - first;
                if (due >= limit) { partial = !eos; stopReason = eos ? "EOS" : "PlaybackLimit"; break; }
                Assert.IsTrue(last < timestamp);
                Assert.AreEqual(checked(reader.Width * reader.Height * 4), pixels.Length);
                int displayStride = checked(reader.DisplayWidth * 4);
                byte[] visiblePixels = new byte[checked(displayStride * reader.DisplayHeight)];
                for (int row = 0; row < reader.DisplayHeight; row++)
                    pixels.AsSpan(checked(((row + reader.DisplayY) * reader.Width + reader.DisplayX) * 4), displayStride)
                        .CopyTo(visiblePixels.AsSpan(row * displayStride));
                while (clock.Elapsed.Ticks < due) Thread.Sleep(1);
                dispatcher.Invoke(() =>
                {
                    Assert.IsNotNull(bitmap);
                    if (bitmap.PixelWidth != reader.DisplayWidth || bitmap.PixelHeight != reader.DisplayHeight)
                    {
                        bitmap = new WriteableBitmap(reader.DisplayWidth, reader.DisplayHeight, 96, 96, PixelFormats.Bgr32, null);
                        Assert.IsNotNull(window);
                        ((Image)window.Content).Source = bitmap;
                    }
                    bitmap.WritePixels(new Int32Rect(0, 0, reader.DisplayWidth, reader.DisplayHeight), visiblePixels, displayStride, 0);
                    updates++;
                    if (updates == 1)
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        using var output = File.Create(Path.Combine(root, "first-writeable-bitmap.png"));
                        encoder.Save(output);
                    }
                }, DispatcherPriority.Render);
                last = timestamp;
                var frame = new { i, hr, stream, flags, timestamp100ns = timestamp, due100ns = due, clock100ns = clock.Elapsed.Ticks,
                    reader.Stride, reader.Width, reader.Height, reader.Subtype,
                    reader.DisplayX, reader.DisplayY, reader.DisplayWidth, reader.DisplayHeight, reader.DisplayAreaSource,
                    topDownOutput = true, reader.BufferLength, outputLength = pixels.Length,
                    sha256 = Convert.ToHexString(SHA256.HashData(pixels)) };
                frames.Add(frame);
                frameLog.WriteLine(JsonSerializer.Serialize(frame)); frameLog.Flush();
            }
            if (partial) while (clock.Elapsed.Ticks < limit) Thread.Sleep(1);
            Assert.IsTrue(eos || partial);
            Assert.IsTrue(frames.Count > 1 && last > first);
            Assert.IsTrue(visible && updates == frames.Count && Volatile.Read(ref renderEvents) > 0);
            if (eos) stopReason = "EOS";
            completed = true;
        }
        finally
        {
            clock.Stop();
            try
            {
                if (dispatcher is not null && ui.IsAlive)
                    dispatcher.Invoke(() => { window?.Close(); closed = window is null || !window.IsVisible; dispatcher.BeginInvokeShutdown(DispatcherPriority.Send); });
                if (ui.ThreadState != System.Threading.ThreadState.Unstarted) Assert.IsTrue(ui.Join(TimeSpan.FromSeconds(10)));
            }
            finally
            {
                try { if (reader is not null) { Assert.AreEqual(0, reader.Close()); reader.Dispose(); } }
                finally { Save(); }
            }
        }
        Assert.IsTrue(closed && !ui.IsAlive);
        Assert.IsNotNull(reader);
        Assert.AreEqual(1, reader.ShutdownCount);
        Assert.AreEqual(1, reader.UninitializeCount);
        Assert.AreEqual(0u, reader.ReaderFinalReferences);
        Assert.AreEqual(reader.SampleReleaseCount, reader.BufferReleaseCount);
    }

    [DllImport("user32.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(nint window);
}
#endif
