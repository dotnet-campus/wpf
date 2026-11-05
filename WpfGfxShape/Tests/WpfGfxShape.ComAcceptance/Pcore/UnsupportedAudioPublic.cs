namespace NativeAcceptance.Pcore;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using WpfGfxShape.Core.Av;

internal static partial class Program
{
    private static void RenderUnsupportedAudioPublic()
    {
        Evidence["caseId"] = "Stage19-media-unsupported-audio-public";
        Evidence["testAssembly"] = Identity(typeof(Program).Assembly.Location);
        Evidence["testMvid"] = typeof(Program).Module.ModuleVersionId;
        Evidence["businessSourceLinkUsed"] = false;
        string path = Path.Combine(AppContext.BaseDirectory, "inputs", "Video.mp4");
        Evidence["asset"] = Identity(path);
        using (var source = File.OpenRead(path))
        {
            if (source.Length != 47255617 || Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(source)) != "928410B89545B2DF0AA7C8A46756D512D2BE4879209282F90D92D578A30CA40A")
                throw new InvalidOperationException("Unexpected user media identity.");
        }
        // MF reader owns COM on its MTA thread; the public WPF player stays on this STA.
        Evidence["sourceProbe"] = Task.Run(() =>
        {
            int hr = MediaFoundationSourceReader.Create(path, out var reader, enableCompressedVideo: true);
            if (hr < 0) System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
            if (reader is null) throw new InvalidOperationException("Missing MF source reader.");
            using (reader)
            {
                var result = new { hr, reader.HasAudio, reader.AudioMetadataResult, reader.SourceMetadata, reader.Negotiation, reader.Width, reader.Height };
                if (reader.HasAudio != true || reader.AudioMetadataResult != 0)
                    throw new InvalidOperationException("Real MF audio metadata was not established.");
                return result;
            }
        }).GetAwaiter().GetResult();
        var events = new List<object>();
        var diagnostics = new List<object>();
        Evidence["mediaEvents"] = events;
        Evidence["cleanupDiagnostics"] = diagnostics;
        Evidence["boundary"] = "Expected capability rejection only; no audio output or full replacement certification; no decoded video submitted by this scene";
        var player = new MediaPlayer();
        var visual = new DrawingVisual();
        var drawing = new VideoDrawing { Player = player, Rect = new Rect(0, 0, 32, 16) };
        using (DrawingContext context = visual.RenderOpen()) context.DrawDrawing(drawing);
        using var presentation = new HwndSource(new HwndSourceParameters("Stage19 unsupported audio public")
        { Width = 32, Height = 16, WindowStyle = unchecked((int)0x90000000) });
        presentation.RootVisual = visual;
        int opened = 0, ended = 0, failed = 0;
        Exception? failure = null;
        ExceptionDispatchInfo? primary = null;
        var clock = Stopwatch.StartNew();
        player.MediaOpened += (_, _) => { opened++; events.Add(new { kind = "Opened", elapsedMs = clock.Elapsed.TotalMilliseconds }); };
        player.MediaEnded += (_, _) => { ended++; events.Add(new { kind = "Ended", elapsedMs = clock.Elapsed.TotalMilliseconds }); };
        player.MediaFailed += (_, e) => { failed++; failure = e.ErrorException; events.Add(new { kind = "Failed", hr = $"0x{failure.HResult:X8}", error = failure.ToString(), elapsedMs = clock.Elapsed.TotalMilliseconds }); Checkpoint("real-MediaFailed"); };
        long renderingEvents = 0;
        EventHandler rendering = (_, _) => renderingEvents++;
        CompositionTarget.Rendering += rendering;
        try
        {
            WaitMedia(() => renderingEvents > 0, () => null, "active HwndSource Rendering");
            Checkpoint("unsupported-audio-public-Open-before");
            player.Open(new Uri(path));
            events.Add(new { kind = "Open-return", hr = "0x00000000" });
            WaitMedia(() => failed > 0, () => null, "real unsupported-audio Failed");
            long before = renderingEvents;
            WaitMedia(() => renderingEvents > before, () => null, "post-failure Rendering");
            Evidence["failureContract"] = new { opened, ended, failed, hr = failure is null ? null : $"0x{failure.HResult:X8}", player.HasVideo, player.HasAudio, player.NaturalVideoWidth, player.NaturalVideoHeight, naturalDuration = player.NaturalDuration.ToString(), renderingEvents };
            byte[] pixels = CopyMedia(visual);
            File.WriteAllBytes(Path.Combine(output, "unsupported-audio.pbgra32"), pixels);
            Evidence["softwareRtb"] = new { width = 32, height = 16, nonzeroBytes = pixels.Count(b => b != 0), boundary = "Software RTB only; not HW pixels" };
            if (opened != 0 || ended != 0 || failed != 1 || failure?.HResult != unchecked((int)0xc00d36b4) || !player.HasAudio || !player.HasVideo || player.NaturalVideoWidth != 640 || player.NaturalVideoHeight != 360 || pixels.Any(b => b != 0))
                throw new InvalidOperationException("Real unsupported-audio public failure contract failed.");
            Evidence["prerolledPacketEvidence"] = "Not directly observed by WPF public event API; no Opened and no frame are independently asserted";
        }
        catch (Exception error) { primary = ExceptionDispatchInfo.Capture(error); }
        finally
        {
            Capture("Close", () => { player.Close(); events.Add(new { kind = "Close-return", hr = "0x00000000" }); });
            Capture("drawing-detach", () => drawing.Player = null);
            Capture("Rendering-unsubscribe", () => CompositionTarget.Rendering -= rendering);
            Capture("Window-close", () => { presentation.RootVisual = null; presentation.Dispose(); events.Add(new { kind = "Window-close", presentation.IsDisposed }); });
            Capture("modules", () => Evidence["allWpfgfxModules"] = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).Select(m => Identity(m.FileName)).ToArray());
        }
        primary?.Throw();
        if (diagnostics.Count != 0) throw new InvalidOperationException("Unsupported-audio cleanup failed.");
        void Capture(string operation, Action action)
        {
            try { action(); }
            catch (Exception error) { diagnostics.Add(new { operation, error = error.ToString(), hr = $"0x{error.HResult:X8}" }); }
        }
    }
}
