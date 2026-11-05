namespace NativeAcceptance.Pcore;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Media;

internal static partial class Program
{
    private static void RenderPureVideoPublic()
    {
        Evidence["caseId"] = "Stage17-public-MediaPlayer-purevideo-v2";
        Evidence["testAssembly"] = Identity(typeof(Program).Assembly.Location);
        Evidence["testMvid"] = typeof(Program).Module.ModuleVersionId;
        Evidence["businessSourceLinkUsed"] = false;
        var events = new List<object>();
        var frames = new List<object>();
        var diagnostics = new List<object>();
        Evidence["mediaEvents"] = events;
        Evidence["mediaFrames"] = frames;
        Evidence["cleanupDiagnostics"] = diagnostics;
        Evidence["samplingContract"] = "paused seek 0.5/2.5 and composition synchronization; play until media clock >= target+0.3; pause before post-clock Rendering; require actual < target+1; one RTB per target; independent dominant-color assertion";
        string path = Path.Combine(output, "purevideo.avi");
        WriteMediaAsset(path, 0, includeAudio: false);
        Evidence["asset"] = Identity(path);
        var player = new MediaPlayer();
        Exception? failure = null;
        ExceptionDispatchInfo? primary = null;
        int opened = 0, ended = 0;
        var clock = Stopwatch.StartNew();
        player.MediaOpened += (_, _) => { opened++; events.Add(new { kind = "Opened", elapsedMs = clock.Elapsed.TotalMilliseconds }); Checkpoint("MediaOpened"); };
        player.MediaEnded += (_, _) => { ended++; events.Add(new { kind = "Ended", elapsedMs = clock.Elapsed.TotalMilliseconds }); Checkpoint("MediaEnded"); };
        player.MediaFailed += (_, e) => { failure = e.ErrorException; events.Add(new { kind = "Failed", error = failure.ToString(), hr = $"0x{failure.HResult:X8}", elapsedMs = clock.Elapsed.TotalMilliseconds }); Checkpoint("MediaFailed"); };
        var visual = new DrawingVisual();
        var drawing = new VideoDrawing { Player = player, Rect = new Rect(0, 0, 32, 16) };
        using (DrawingContext context = visual.RenderOpen()) context.DrawDrawing(drawing);
        var presentation = new System.Windows.Interop.HwndSource(new System.Windows.Interop.HwndSourceParameters("Stage17 live video composition")
        {
            Width = 32, Height = 16, WindowStyle = unchecked((int)0x90000000)
        });
        presentation.RootVisual = visual;
        Evidence["presentation"] = "Live visible HwndSource root DrawingVisual, VideoDrawing 32x16, opacity1";
        long renderingEvents = 0;
        EventHandler rendering = (_, _) => { renderingEvents++; events.Add(new { kind = "Rendering", qpc = Stopwatch.GetTimestamp(), elapsedMs = clock.Elapsed.TotalMilliseconds, position = player.Position.Ticks, renderingEvents }); };
        CompositionTarget.Rendering += rendering;
        try
        {
            WaitMedia(() => renderingEvents > 0, () => failure, "live composition Rendering");
            player.ScrubbingEnabled = true;
            Checkpoint("public-Open-before");
            player.Open(new Uri(path));
            events.Add(new { kind = "Open-return", hr = "0x00000000" });
            Checkpoint("public-Open-return");
            WaitMedia(() => opened == 1, () => failure, "Opened");
            Evidence["openedState"] = new { player.HasVideo, player.HasAudio, player.CanPause, duration = player.NaturalDuration.ToString(), player.NaturalVideoWidth, player.NaturalVideoHeight };
            if (!player.HasVideo || player.HasAudio || !player.CanPause || !player.NaturalDuration.HasTimeSpan || player.NaturalDuration.TimeSpan.TotalSeconds < 3.9)
                throw new InvalidOperationException("Unexpected purevideo public metadata.");
            player.Pause();
            byte[]? previous = null;
            foreach (double position in new[] { 0.5, 2.5 })
            {
                events.Add(new { kind = "seek-before", qpc = Stopwatch.GetTimestamp(), elapsedMs = clock.Elapsed.TotalMilliseconds, position = player.Position.Ticks });
                player.Position = TimeSpan.FromSeconds(position);
                double seekMs = clock.Elapsed.TotalMilliseconds;
                events.Add(new { kind = "seek-return", qpc = Stopwatch.GetTimestamp(), elapsedMs = clock.Elapsed.TotalMilliseconds, position = player.Position.Ticks });
                long seekRendering = renderingEvents;
                WaitMedia(() => renderingEvents > seekRendering, () => failure, "paused seek composition Rendering");
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
                events.Add(new { kind = "paused-seek-rendered", qpc = Stopwatch.GetTimestamp(), elapsedMs = clock.Elapsed.TotalMilliseconds, position = player.Position.Ticks });
                player.Play();
                events.Add(new { kind = "Play-return", qpc = Stopwatch.GetTimestamp(), elapsedMs = clock.Elapsed.TotalMilliseconds, position = player.Position.Ticks });
                Checkpoint($"seek-{position}");
                events.Add(new { kind = "seek-checkpoint-return", qpc = Stopwatch.GetTimestamp(), elapsedMs = clock.Elapsed.TotalMilliseconds, position = player.Position.Ticks });
                WaitMedia(() => player.Position.TotalSeconds >= position + 0.3, () => failure, "preregistered media clock");
                events.Add(new { kind = "clock-window-reached", qpc = Stopwatch.GetTimestamp(), position = player.Position.Ticks, elapsedMs = clock.Elapsed.TotalMilliseconds });
                player.Pause();
                events.Add(new { kind = "Pause-return", qpc = Stopwatch.GetTimestamp(), elapsedMs = clock.Elapsed.TotalMilliseconds, position = player.Position.Ticks });
                long beforeRender = renderingEvents;
                WaitMedia(() => renderingEvents > beforeRender, () => failure, "post-clock composition Rendering");
                events.Add(new { kind = "post-clock-rendering", qpc = Stopwatch.GetTimestamp(), position = player.Position.Ticks, elapsedMs = clock.Elapsed.TotalMilliseconds });
                System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
                events.Add(new { kind = "render-dispatch-complete", qpc = Stopwatch.GetTimestamp(), position = player.Position.Ticks, elapsedMs = clock.Elapsed.TotalMilliseconds });
                double actual = player.Position.TotalSeconds;
                byte[] pixels = CopyMedia(visual);
                int offset = (8 * 32 + 16) * 4;
                string name = $"purevideo-{position}.pbgra32";
                File.WriteAllBytes(Path.Combine(output, name), pixels);
                frames.Add(new { requestedPosition = position, actualPosition = actual, seekMs, captureMs = clock.Elapsed.TotalMilliseconds,
                    sample = Convert.ToHexString(pixels.AsSpan(offset, 4)), file = name, player.HasVideo, drawing.Rect, opacity = visual.Opacity,
                    contentBounds = visual.ContentBounds.ToString(), renderingEvents, width = 32, height = 16 });
                Checkpoint($"pixel-{position}-captured");
                bool dominant = position < 2 ? pixels[offset + 2] > pixels[offset] && pixels[offset + 2] > pixels[offset + 1]
                    : pixels[offset] > pixels[offset + 1] && pixels[offset] > pixels[offset + 2];
                if (actual >= position + 1 || pixels[offset + 3] != 255 || !dominant || (previous is not null && pixels.AsSpan().SequenceEqual(previous)))
                    throw new InvalidOperationException("Independent purevideo content/clock/opacity/stale-frame assertion failed.");
                previous = pixels;
            }
            player.Play();
            WaitMedia(() => ended == 1, () => failure, "Ended");
            Evidence["continuousPlaybackContract"] = "additional acceptance after original EOS: paused seek to zero and composition synchronization, then one Play with no intervening seek/Pause; capture public VideoDrawing RTB at 0.8/2.8, using original 0.5/2.5 color and upper-window assertions; no pixel retry; second EOS required; finally same-player immediate Close/Open";
            player.Pause();
            var playingFrames = new List<object>();
            Evidence["continuousPlaybackFrames"] = playingFrames;
            player.Position = TimeSpan.Zero;
            long restartRendering = renderingEvents;
            WaitMedia(() => renderingEvents > restartRendering, () => failure, "continuous playback paused seek Rendering");
            System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            player.Play();
            events.Add(new { kind = "continuous-Play-return", qpc = Stopwatch.GetTimestamp(), elapsedMs = clock.Elapsed.TotalMilliseconds, position = player.Position.Ticks });
            byte[]? previousPlaying = null;
            foreach (double position in new[] { 0.5, 2.5 })
            {
                WaitMedia(() => player.Position.TotalSeconds >= position + 0.3, () => failure, "continuous playback media clock");
                double before = player.Position.TotalSeconds;
                byte[] pixels = CopyMedia(visual);
                double after = player.Position.TotalSeconds;
                int offset = (8 * 32 + 16) * 4;
                string name = $"purevideo-playing-{position}.pbgra32";
                File.WriteAllBytes(Path.Combine(output, name), pixels);
                playingFrames.Add(new { requestedPosition = position, beforePosition = before, afterPosition = after,
                    captureMs = clock.Elapsed.TotalMilliseconds, qpc = Stopwatch.GetTimestamp(),
                    sample = Convert.ToHexString(pixels.AsSpan(offset, 4)), file = name,
                    width = 32, height = 16, renderingEvents, paused = false });
                Checkpoint($"continuous-pixel-{position}-captured");
                bool dominant = position < 2 ? pixels[offset + 2] > pixels[offset] && pixels[offset + 2] > pixels[offset + 1]
                    : pixels[offset] > pixels[offset + 1] && pixels[offset] > pixels[offset + 2];
                if (before >= position + 1 || after >= position + 1 || pixels[offset + 3] != 255 || !dominant
                    || (previousPlaying is not null && pixels.AsSpan().SequenceEqual(previousPlaying)))
                    throw new InvalidOperationException("Independent continuous purevideo content/clock/opacity/stale-frame assertion failed.");
                previousPlaying = pixels;
            }
            WaitMedia(() => ended == 2, () => failure, "continuous replay Ended");
            Checkpoint("continuous-playback-completed");
            var reopenUri = new Uri(path);
            for (int cycle = 0; cycle < 3; cycle++)
            {
                player.Close();
                player.Open(reopenUri);
                events.Add(new { kind = "reopen-Close-Open-return", cycle, elapsedMs = clock.Elapsed.TotalMilliseconds });
                int expectedOpened = cycle + 2;
                WaitMedia(() => opened == expectedOpened, () => failure, "reopened Opened");
                Evidence["reopenedState"] = new { cycle, opened, ended, player.HasVideo, player.HasAudio, player.CanPause, duration = player.NaturalDuration.ToString(), player.NaturalVideoWidth, player.NaturalVideoHeight };
                if (ended != 2 || !player.HasVideo || player.HasAudio || !player.CanPause || !player.NaturalDuration.HasTimeSpan || player.NaturalDuration.TimeSpan.TotalSeconds < 3.9)
                    throw new InvalidOperationException("Unexpected reopened purevideo public metadata or stale Ended event.");
            }
        }
        catch (Exception error)
        {
            primary = ExceptionDispatchInfo.Capture(error);
            Evidence["primaryException"] = new { text = error.ToString(), hr = $"0x{error.HResult:X8}", stage };
        }
        finally
        {
            Capture("metadata-before-Close", () => Evidence["beforeClose"] = new { position = player.Position.Ticks, player.HasVideo, player.HasAudio, duration = player.NaturalDuration.ToString() });
            Capture("Close", () => { player.Close(); events.Add(new { kind = "Close-return", hr = "0x00000000" }); });
            Capture("drawing-detach", () => drawing.Player = null);
            Capture("Rendering-unsubscribe", () => CompositionTarget.Rendering -= rendering);
            Capture("Window-close", () => { presentation.RootVisual = null; presentation.Dispose(); events.Add(new { kind = "Window-close", presentation.IsDisposed }); });
            Capture("systemMediaModules", () => Evidence["systemMediaModules"] = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.StartsWith("mf", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Contains("quartz", StringComparison.OrdinalIgnoreCase)).Select(m => new { m.ModuleName, m.FileName }).ToArray());
            Capture("allWpfgfxModules", () => Evidence["allWpfgfxModules"] = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).Select(m => Identity(m.FileName)).ToArray());
        }
        primary?.Throw();
        if (diagnostics.Count != 0) throw new InvalidOperationException("Independent cleanup or evidence collection failed.");
        void Capture(string operation, Action action)
        {
            try { action(); }
            catch (Exception error) { diagnostics.Add(new { operation, error = error.ToString(), hr = $"0x{error.HResult:X8}" }); }
        }
    }
}
