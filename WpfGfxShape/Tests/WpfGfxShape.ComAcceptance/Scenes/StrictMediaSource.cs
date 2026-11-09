using System.Diagnostics;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfGfxShape.ComAcceptance;

internal static class StrictAudioInput
{
    internal static string PathName => Path.Combine(AppContext.BaseDirectory, "Inputs", "audio-initialize-close", "content.bin");

    internal static byte[] Bytes()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write("RIFF"u8); writer.Write(64036); writer.Write("WAVEfmt "u8);
            writer.Write(16); writer.Write((ushort)1); writer.Write((ushort)1);
            writer.Write(8000); writer.Write(16000); writer.Write((ushort)2); writer.Write((ushort)16);
            writer.Write("data"u8); writer.Write(64000); writer.Write(new byte[64000]);
        }
        return stream.ToArray();
    }

    internal static void Prepare()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(PathName)!);
        File.WriteAllBytes(PathName, Bytes());
    }
}

internal static partial class StrictRenderBusiness
{
    private static unsafe object ObserveAudioEndpointCapability()
    {
        var evidence = new List<string>();
        nint enumerator = 0, device = 0, collection = 0;
        int init = AudioDiagnosticCoInitializeEx(0, 0);
        evidence.Add($"CoInitializeEx(MTA):0x{init:X8}");
        int? endpoint = null, enumeration = null, countResult = null;
        uint count = 0;
        try
        {
            if (init >= 0)
            {
                Guid clsid = new("bcde0395-e52f-467c-8e3d-c4579291692e"), iid = new("a95664d2-9614-4f35-a746-de8db63617e6");
                int create = AudioDiagnosticCoCreateInstance(&clsid, 0, 1, &iid, &enumerator);
                evidence.Add($"CoCreateInstance(MMDeviceEnumerator):0x{create:X8}");
                if (create >= 0 && enumerator != 0)
                {
                    nint* table = *(nint**)enumerator;
                    endpoint = ((delegate* unmanaged[Stdcall]<nint, int, int, nint*, int>)table[4])(enumerator, 0, 0, &device);
                    evidence.Add($"GetDefaultAudioEndpoint(render,console):0x{endpoint.Value:X8}");
                    enumeration = ((delegate* unmanaged[Stdcall]<nint, int, uint, nint*, int>)table[3])(enumerator, 0, 1, &collection);
                    evidence.Add($"EnumAudioEndpoints(active,render):0x{enumeration.Value:X8}");
                    if (enumeration >= 0 && collection != 0)
                    {
                        countResult = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(nint**)collection)[3])(collection, &count);
                        evidence.Add($"IMMDeviceCollection.GetCount:0x{countResult.Value:X8};count={count}");
                    }
                }
            }
        }
        finally
        {
            foreach (nint value in new[] { collection, device, enumerator })
                if (value != 0) evidence.Add($"Release:{((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)value)[2])(value)}");
            if (init >= 0) { AudioDiagnosticCoUninitialize(); evidence.Add("CoUninitialize:void"); }
        }
        return new { boundary = "Independent read-only system capability after public Close; not the player's internal call trace; no render, role fallback or system mutation", evidence };
    }

    [System.Runtime.InteropServices.DllImport("ole32.dll", EntryPoint = "CoInitializeEx")]
    private static extern int AudioDiagnosticCoInitializeEx(nint reserved, uint flags);
    [System.Runtime.InteropServices.DllImport("ole32.dll", EntryPoint = "CoUninitialize")]
    private static extern void AudioDiagnosticCoUninitialize();
    [System.Runtime.InteropServices.DllImport("ole32.dll", EntryPoint = "CoCreateInstance")]
    private static extern unsafe int AudioDiagnosticCoCreateInstance(Guid* clsid, nint outer, uint context, Guid* iid, nint* instance);

    private static void RenderAudioRatePreflight()
    {
        Evidence["caseId"] = StrictRenderCase.CaseId(sceneCase);
        Evidence["sceneInput"] = StrictRenderCase.Input(sceneCase);
        Evidence["consumer"] = "public MediaPlayer Rate preflight; no Open; no pixels; not native effective Rate or DSP";
        Evidence["mediaAssets"] = Array.Empty<object>();
        var operations = new List<object>();
        var events = new List<object>();
        Evidence["mediaOperations"] = operations;
        Evidence["mediaEvents"] = events;
        int sequence = 0;
        bool closed = true;
        for (int episode = 0; episode < StrictRatePreflight.EpisodeCount; episode++)
        {
            int currentEpisode = episode;
            string phase = "create";
            var player = new MediaPlayer();
            EventHandler onOpened = (_, _) => events.Add(new { sequence = ++sequence, episode = currentEpisode, phase, kind = "opened", exception = RateError(null) });
            EventHandler<ExceptionEventArgs> onFailed = (_, e) => events.Add(new { sequence = ++sequence, episode = currentEpisode, phase, kind = "failed", exception = RateError(e.ErrorException) });
            EventHandler onEnded = (_, _) => events.Add(new { sequence = ++sequence, episode = currentEpisode, phase, kind = "ended", exception = RateError(null) });
            player.MediaOpened += onOpened;
            player.MediaFailed += onFailed;
            player.MediaEnded += onEnded;
            object Cache()
            {
                try { return new { value = (object?)StrictRatePreflight.Encode(player.SpeedRatio), exception = RateError(null) }; }
                catch (Exception error) when (StrictRatePreflight.Recoverable(error))
                { return new { value = (object?)null, exception = RateError(error) }; }
            }
            try
            {
                StrictRatePreflight.Execute(episode, step =>
                {
                    phase = step.Name;
                    switch (step.Name)
                    {
                        case "Set": player.SpeedRatio = step.Value!.Value; break;
                        case "Pause": player.Pause(); break;
                        case "Play": player.Play(); break;
                        case "Close": player.Close(); break;
                        case "ApplicationIdle":
                        case "Close-ApplicationIdle":
                            var frame = new DispatcherFrame();
                            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
                            Dispatcher.PushFrame(frame);
                            break;
                    }
                }, (step, error) =>
                {
                    if (step.Name == "Close" && error is not null) closed = false;
                    operations.Add(new { sequence = ++sequence, episode = currentEpisode, name = step.Name,
                        input = step.Value.HasValue ? StrictRatePreflight.Encode(step.Value.Value) : null,
                        threw = error is not null, exception = RateError(error), cache = Cache() });
                });
            }
            finally
            {
                player.MediaOpened -= onOpened;
                player.MediaFailed -= onFailed;
                player.MediaEnded -= onEnded;
            }
        }
        Evidence["mediaLifecycle"] = new { opened = false, effectiveRate = "Unknown", dspValidated = false,
            episodes = StrictRatePreflight.EpisodeCount, closed, handlersReleased = true,
            completionBoundary = "ApplicationIdle is sampling only, not native application completion; no Open or endpoint initialization" };
        Evidence["mediaContractPassed"] = closed;
        Evidence["callbackDrain"] = "per-episode Close + ApplicationIdle sampling; handlers released; dispatcher/finalizer drain follows; no future-event certification";
    }

    private static object RateError(Exception? error) => new { type = error?.GetType().FullName, hresult = error?.HResult };

    private static void RenderAudioInitializeClose()
    {
        Evidence["caseId"] = StrictRenderCase.CaseId(sceneCase);
        Evidence["sceneInput"] = StrictRenderCase.Input(sceneCase);
        Evidence["consumer"] = "public MediaPlayer initialization/close; no playback; no pixels";
        byte[] input = File.ReadAllBytes(StrictAudioInput.PathName);
        if (!input.AsSpan().SequenceEqual(StrictAudioInput.Bytes())) throw new InvalidDataException("Audio input differs from fixed PCM WAVE.");
        Evidence["mediaAssets"] = new[] { new { name = "content.bin", bytes = input.Length, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(input)) } };
        var operations = new List<object>();
        var events = new List<object>();
        Evidence["mediaOperations"] = operations;
        Evidence["mediaEvents"] = events;
        var player = new MediaPlayer();
        bool seekCase = sceneCase == StrictRenderCase.MediaAudioSeek;
        bool seekExecuted = false, finalTargetObserved = false, seekOperationsValid = true;
        int opened = 0, failed = 0, sequence = 0;
        string phase = "create";
        bool timedOut = false, openThrew = false, closed = false, barrier = false, metadataValid = true;
        object Error(Exception? error) => new { type = error?.GetType().FullName, hresult = error?.HResult, message = error?.Message, detail = error?.ToString() };
        object Read(Func<object?> getter)
        {
            try { return new { value = getter(), exception = Error(null) }; }
            catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException)
            { return new { value = (object?)null, exception = Error(error) }; }
        }
        object Metadata() => new { width = Read(() => player.NaturalVideoWidth), height = Read(() => player.NaturalVideoHeight),
            durationTicks = Read(() => player.NaturalDuration.HasTimeSpan ? (long?)player.NaturalDuration.TimeSpan.Ticks : null),
            HasAudio = Read(() => player.HasAudio), HasVideo = Read(() => player.HasVideo) };
        object Snapshot() => seekCase ? new { metadata = Metadata(), positionTicks = Read(() => player.Position.Ticks) } : Metadata();
        void Event(string kind, Exception? error = null)
        {
            events.Add(new { sequence = ++sequence, phase, kind, exception = Error(error), metadata = Snapshot(), episodeOpened = opened, episodeFailed = failed });
            Checkpoint("media-event-" + kind);
        }
        EventHandler onOpened = (_, _) => { opened++; Event("opened"); };
        EventHandler<ExceptionEventArgs> onFailed = (_, e) => { failed++; Event("failed", e.ErrorException); };
        player.MediaOpened += onOpened;
        player.MediaFailed += onFailed;
        bool Invoke(string name, Action action)
        {
            phase = name;
            Exception? caught = null;
            try { action(); }
            catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException) { caught = error; }
            operations.Add(new { sequence = ++sequence, name, threw = caught is not null, exception = Error(caught), metadata = Snapshot(), episodeOpened = opened, episodeFailed = failed });
            Checkpoint("media-operation-" + name);
            return caught is null;
        }
        try
        {
            openThrew = !Invoke("Open", () => player.Open(new Uri(StrictAudioInput.PathName)));
            phase = "OpenedOrFailed";
            if (!openThrew && opened == 0 && failed == 0)
            {
                var frame = new DispatcherFrame();
                var clock = Stopwatch.StartNew();
                var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(10) };
                timer.Tick += (_, _) =>
                {
                    if (opened != 0 || failed != 0) frame.Continue = false;
                    else if (clock.Elapsed >= TimeSpan.FromSeconds(20)) { timedOut = true; frame.Continue = false; }
                };
                timer.Start();
                try { Dispatcher.PushFrame(frame); }
                finally { timer.Stop(); }
            }
            Invoke("OpenedOrFailed-terminal-snapshot", () => { if (timedOut) throw new TimeoutException("Expected MediaOpened or MediaFailed within 20 seconds."); });
            if (opened > 0)
            {
                metadataValid = Invoke("Opened-WAVE-metadata", () =>
                {
                    if (player.NaturalVideoWidth != 0 || player.NaturalVideoHeight != 0 || !player.HasAudio || player.HasVideo
                        || player.NaturalDuration.HasTimeSpan && player.NaturalDuration.TimeSpan != TimeSpan.FromSeconds(4))
                        throw new InvalidDataException("Opened PCM WAVE metadata mismatch.");
                });
                bool paused = Invoke("Pause", player.Pause);
                if (seekCase)
                {
                    seekExecuted = true;
                    seekOperationsValid = paused;
                    foreach (long target in new[] { 5_000_000L, 15_000_000L, 10_000_000L })
                    {
                        seekOperationsValid &= Invoke("Position-set-" + target, () => player.Position = TimeSpan.FromTicks(target));
                        seekOperationsValid &= Invoke("Position-immediate-query-" + target, () => { _ = player.Position.Ticks; });
                    }
                    phase = "Seek-final-target-or-Failed";
                    var frame = new DispatcherFrame();
                    var clock = Stopwatch.StartNew();
                    var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(10) };
                    timer.Tick += (_, _) =>
                    {
                        bool querySucceeded = Invoke("Position-final-observation", () => finalTargetObserved = player.Position.Ticks == 10_000_000L);
                        if (finalTargetObserved || failed > 0 || !querySucceeded) frame.Continue = false;
                        else if (clock.Elapsed >= TimeSpan.FromSeconds(20)) { timedOut = true; frame.Continue = false; }
                    };
                    timer.Start();
                    try { Dispatcher.PushFrame(frame); }
                    finally { timer.Stop(); }
                    Invoke("Seek-observation-terminal", () =>
                    {
                        if (timedOut) throw new TimeoutException("Final public Position target or MediaFailed was not observed within 20 seconds.");
                    });
                }
                else Invoke("Volume0", () => player.Volume = 0);
            }
        }
        finally
        {
            closed = Invoke("Close", player.Close);
            barrier = Invoke("Close-ApplicationIdle-barrier", () =>
            {
                var frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
                Dispatcher.PushFrame(frame);
            });
            player.MediaOpened -= onOpened;
            player.MediaFailed -= onFailed;
            Evidence["audioEndpointCapability"] = Task.Run(ObserveAudioEndpointCapability).GetAwaiter().GetResult();
            Evidence["mediaLifecycle"] = seekCase
                ? (object)new { episodeOpened = opened, episodeFailed = failed, openThrew, timedOut, closed, barrier, handlersReleased = true,
                    seekStatus = seekExecuted ? "SeekExecuted" : "SeekNotExecuted", finalTargetObserved,
                    seekPassed = seekExecuted && seekOperationsValid && finalTargetObserved && failed == 0 && !timedOut,
                    completionBoundary = "Public Position observation only; pending target can be immediately visible; no public seek-completed event or owner completion certification; no Play/listening" }
                : new { episodeOpened = opened, episodeFailed = failed, openThrew, timedOut, closed, barrier, handlersReleased = true };
            Evidence["mediaContractPassed"] = !timedOut && metadataValid && closed && barrier && (openThrew || opened + failed > 0);
            Evidence["callbackDrain"] = "public Close + ApplicationIdle barrier; handlers released; dispatcher/finalizer drain follows; not arbitrary late callback certification";
        }
    }

    private static void RenderMediaSource()
    {
        if (sceneCase == StrictRenderCase.MediaAudioRatePreflight)
        {
            RenderAudioRatePreflight();
            return;
        }
        if (sceneCase is StrictRenderCase.MediaAudioInitializeClose or StrictRenderCase.MediaAudioSeek)
        {
            RenderAudioInitializeClose();
            return;
        }
        Evidence["caseId"] = StrictRenderCase.CaseId(sceneCase);
        Evidence["sceneInput"] = StrictRenderCase.Input(sceneCase);
        Evidence["consumer"] = "public MediaPlayer metadata/events; no visual cache";
        var operations = new List<object>();
        var events = new List<object>();
        Evidence["mediaOperations"] = operations;
        Evidence["mediaEvents"] = events;
        var player = new MediaPlayer();
        string phase = "create";
        int opened = 0, sequence = 0;
        Exception? failure = null;
        object Read(Func<object?> getter)
        {
            try { return new { value = getter(), error = (string?)null, hresult = 0 }; }
            catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException)
            { return new { value = (object?)null, error = error.GetType().FullName, hresult = error.HResult }; }
        }
        object Metadata() => new { width = Read(() => player.NaturalVideoWidth), height = Read(() => player.NaturalVideoHeight),
            durationTicks = Read(() => { var duration = player.NaturalDuration; return duration.HasTimeSpan ? (long?)duration.TimeSpan.Ticks : null; }),
            positionTicks = Read(() => player.Position.Ticks), speedRatio = Read(() => player.SpeedRatio), HasAudio = Read(() => player.HasAudio), HasVideo = Read(() => player.HasVideo) };
        void Event(string kind, Exception? error = null)
        {
            events.Add(new { sequence = ++sequence, phase, kind, metadata = Metadata(), error = error?.ToString() });
            Checkpoint("media-event-" + kind);
        }
        player.MediaOpened += (_, _) => { opened++; Event("opened"); };
        player.MediaFailed += (_, e) => { failure = e.ErrorException; Event("failed", failure); };
        player.MediaEnded += (_, _) => Event("ended");
        player.BufferingStarted += (_, _) => Event("buffering-started");
        player.BufferingEnded += (_, _) => Event("buffering-ended");
        void Invoke(string name, Action action)
        {
            phase = name;
            try { action(); operations.Add(new { sequence = ++sequence, name, threw = false, hresult = 0, metadata = Metadata() }); }
            catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException)
            {
                operations.Add(new { sequence = ++sequence, name, threw = true, hresult = error.HResult, exception = error.ToString() });
                throw;
            }
            finally { Checkpoint("media-operation-" + name); }
        }
        void Barrier()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
            Dispatcher.PushFrame(frame);
        }
        void WaitOpened(int count, int seconds, int width, int height)
        {
            var frame = new DispatcherFrame();
            var clock = Stopwatch.StartNew();
            var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(10) };
            timer.Tick += (_, _) => { if (opened >= count || failure is not null || clock.Elapsed > TimeSpan.FromSeconds(20)) frame.Continue = false; };
            timer.Start();
            try { Dispatcher.PushFrame(frame); }
            finally { timer.Stop(); }
            if (failure is not null) throw new InvalidOperationException("MediaFailed at " + phase, failure);
            if (opened != count) throw new TimeoutException("Expected MediaOpened at " + phase);
            Invoke(phase + "-opened-snapshot", () => { });
            if (!player.NaturalDuration.HasTimeSpan || player.NaturalDuration.TimeSpan != TimeSpan.FromSeconds(seconds)
                || player.NaturalVideoWidth != width || player.NaturalVideoHeight != height
                || player.HasAudio || !player.HasVideo)
                throw new InvalidOperationException("Opened asset metadata mismatch at " + phase);
        }
        string a = Path.Combine(output, "source-a.avi"), b = Path.Combine(output, "source-b.avi");
        NativeAcceptance.Pcore.Program.WriteMediaAsset(a, 0, includeAudio: false, frameCount: sceneCase is StrictRenderCase.MediaContinuousSeek or StrictRenderCase.MediaPlayPauseResume or StrictRenderCase.MediaSpeedRatio ? 40 : 10, width: 2, height: 1);
        NativeAcceptance.Pcore.Program.WriteMediaAsset(b, 1, includeAudio: false, frameCount: 20, width: 4, height: 2);
        Evidence["mediaAssets"] = new[] { new { name = "A", bytes = new FileInfo(a).Length, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(a))) },
            new { name = "B", bytes = new FileInfo(b).Length, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(b))) } };
        try
        {
            if (sceneCase == StrictRenderCase.MediaSpeedRatio)
            {
                void ObserveFrame(string name, long baseline, bool blue)
                {
                    phase = name;
                    var frame = new DispatcherFrame();
                    bool delivered = false, advanced = false;
                    int samples = 0, nonempty = 0;
                    var timeout = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromSeconds(5) };
                    EventHandler rendering = (_, _) =>
                    {
                        var visual = new System.Windows.Media.DrawingVisual();
                        using (var drawing = visual.RenderOpen()) drawing.DrawVideo(player, new System.Windows.Rect(0, 0, 2, 1));
                        var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(2, 1, 96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(visual);
                        byte[] pixels = new byte[8];
                        bitmap.CopyPixels(pixels, 8, 0);
                        samples++;
                        if (pixels[3] != 0) nonempty++;
                        delivered |= pixels[3] != 0 && (blue ? pixels[0] > pixels[2] : pixels[2] > pixels[0]);
                        advanced |= player.Position.Ticks > baseline;
                        if (failure is not null || delivered && advanced) frame.Continue = false;
                    };
                    timeout.Tick += (_, _) => frame.Continue = false;
                    CompositionTarget.Rendering += rendering;
                    timeout.Start();
                    try { Dispatcher.PushFrame(frame); }
                    finally { timeout.Stop(); CompositionTarget.Rendering -= rendering; }
                    Invoke(name + "-samples-" + samples + "-nonempty-" + nonempty + "-frameDelivered-" + delivered + "-positionAdvanced-" + advanced + "-PTS-Unavailable", () => { });
                    if (failure is not null) throw new InvalidOperationException("MediaFailed at " + name, failure);
                    if (!delivered || !advanced) throw new TimeoutException("Public video frame/position observation incomplete at " + name);
                }
                Invoke("OpenA", () => player.Open(new Uri(a))); WaitOpened(1, 4, 2, 1);
                Invoke("SpeedRatio-0.5", () => player.SpeedRatio = 0.5);
                long beforePlay = player.Position.Ticks;
                Invoke("Play", player.Play);
                ObserveFrame("HalfRate-red-frame", beforePlay, false);
                Invoke("Pause-submitted-not-completed", player.Pause);
                Invoke("SpeedRatio-2", () => player.SpeedRatio = 2);
                long beforeResume = player.Position.Ticks;
                Invoke("Resume-via-Play", player.Play);
                ObserveFrame("DoubleRate-blue-frame", beforeResume, true);
                Invoke("PauseCompleted-Unavailable-audio-Unknown-no-rate-wallclock-certification", () => { });
                Invoke("Close", player.Close); Barrier();
                Invoke("Close-barrier", () => { });
                Evidence["mediaContractPassed"] = opened == 1 && failure is null;
                return;
            }
            if (sceneCase == StrictRenderCase.MediaPlayPauseResume)
            {
                bool ObserveRendering(string name, long baseline, bool requireAdvance)
                {
                    phase = name;
                    var frame = new DispatcherFrame();
                    int samples = 0;
                    bool advanced = false;
                    var timeout = new DispatcherTimer(DispatcherPriority.Send) { Interval = TimeSpan.FromSeconds(5) };
                    EventHandler rendering = (_, _) =>
                    {
                        Invoke(name + "-Rendering", () => { });
                        advanced |= player.Position.Ticks > baseline;
                        if (failure is not null || (requireAdvance ? advanced : ++samples >= 3)) frame.Continue = false;
                    };
                    timeout.Tick += (_, _) => frame.Continue = false;
                    CompositionTarget.Rendering += rendering;
                    timeout.Start();
                    try { Dispatcher.PushFrame(frame); }
                    finally { timeout.Stop(); CompositionTarget.Rendering -= rendering; }
                    if (failure is not null) throw new InvalidOperationException("MediaFailed at " + phase, failure);
                    if (requireAdvance ? !advanced : samples < 3) throw new TimeoutException("Rendering observation incomplete at " + name);
                    return advanced;
                }
                Invoke("OpenA", () => player.Open(new Uri(a))); WaitOpened(1, 4, 2, 1);
                long beforePlay = player.Position.Ticks;
                Invoke("Play", player.Play);
                bool playAdvanced = ObserveRendering("Play-observation", beforePlay, true);
                Invoke("Pause-submitted-not-completed", player.Pause);
                ObserveRendering("Pause-observation-not-completion", player.Position.Ticks, false);
                long beforeResume = player.Position.Ticks;
                Invoke("Resume-via-Play", player.Play);
                bool resumeAdvanced = ObserveRendering("Resume-observation", beforeResume, true);
                Invoke("RapidPause", player.Pause);
                long beforeFinalPlay = player.Position.Ticks;
                Invoke("RapidPlay-final-intent", player.Play);
                bool finalAdvanced = ObserveRendering("FinalPlay-observation", beforeFinalPlay, true);
                Invoke("Properties-play-" + playAdvanced + "-resume-" + resumeAdvanced + "-final-" + finalAdvanced + "-pauseFreeze-Undecidable-resumeContinuity-Undecidable", () => { });
                Invoke("Close", player.Close); Barrier();
                Invoke("Close-barrier", () => { });
                Evidence["mediaContractPassed"] = playAdvanced && resumeAdvanced && finalAdvanced && opened == 1 && failure is null;
                return;
            }
            if (sceneCase == StrictRenderCase.MediaContinuousSeek)
            {
                Invoke("OpenA", () => player.Open(new Uri(a))); WaitOpened(1, 4, 2, 1);
                Invoke("Pause", player.Pause);
                Invoke("EnableScrubbing", () => player.ScrubbingEnabled = true);
                bool accepted = true;
                foreach (long target in new long[] { 5000000, 25000000, 10000000 })
                {
                    phase = "Seek-" + target;
                    try
                    {
                        player.Position = TimeSpan.FromTicks(target);
                        operations.Add(new { sequence = ++sequence, name = phase, targetTicks = target, threw = false, hresult = 0, metadata = Metadata() });
                    }
                    catch (Exception error) when (error is not OutOfMemoryException and not StackOverflowException)
                    {
                        accepted = false;
                        operations.Add(new { sequence = ++sequence, name = phase, targetTicks = target, threw = true, hresult = error.HResult, exception = error.ToString(), metadata = Metadata() });
                    }
                }
                Checkpoint("continuous-seek-submitted");
                Barrier();
                Invoke("Seek-ApplicationIdle-observation-not-completion", () => { });
                Invoke("Close", player.Close); Barrier();
                Invoke("Close-barrier", () => { });
                Evidence["mediaContractPassed"] = accepted && opened == 1 && failure is null;
                if (!accepted) throw new InvalidOperationException("A legal continuous seek was rejected.");
                return;
            }
            Invoke("OpenA", () => player.Open(new Uri(a))); WaitOpened(1, 1, 2, 1);
            Invoke("OpenB", () => player.Open(new Uri(b))); WaitOpened(2, 2, 4, 2);
            Invoke("OpenNull", () => player.Open(null)); Barrier();
            Invoke("Null-barrier", () => { });
            Invoke("ReopenA", () => player.Open(new Uri(a))); WaitOpened(3, 1, 2, 1);
            Invoke("Close", player.Close); Barrier();
            Invoke("Close-barrier", () => { });
            if (opened != 3 || failure is not null) throw new InvalidOperationException("Unexpected media event count.");
            Evidence["mediaContractPassed"] = true;
        }
        finally { player.Close(); Barrier(); Evidence["callbackDrain"] = "public Close + ApplicationIdle barrier; dispatcher/finalizer drain follows; not arbitrary late callback certification"; }
    }


}
