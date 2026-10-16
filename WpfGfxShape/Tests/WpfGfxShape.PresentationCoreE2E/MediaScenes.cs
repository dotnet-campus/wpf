using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

internal static partial class Program
{
    private static void RenderMedia()
    {
        Evidence["caseId"] = "E2E-08-public-continuous-media-v1";
        var events = new List<object>();
        Evidence["mediaEvents"] = events;
        var player = new MediaPlayer();
        Exception? failure = null;
        int opened = 0, ended = 0;
        player.MediaOpened += (_, _) => { opened++; events.Add(new { kind = "opened", opened, position = player.Position.TotalSeconds }); };
        player.MediaFailed += (_, e) => { failure = e.ErrorException; events.Add(new { kind = "failed", error = e.ErrorException.ToString() }); };
        player.MediaEnded += (_, _) => { ended++; events.Add(new { kind = "ended", ended, position = player.Position.TotalSeconds }); };
        var drawing = new VideoDrawing { Player = player, Rect = new Rect(0, 0, 32, 16) };
        var visual = new DrawingVisual();
        using (DrawingContext context = visual.RenderOpen()) context.DrawDrawing(drawing);
        var frames = new List<object>();
        Evidence["mediaFrames"] = frames;
        try
        {
            for (int asset = 0; asset < 2; asset++)
            {
                string path = Path.Combine(output, $"media-{asset}.avi");
                WriteMediaAsset(path, asset);
                Checkpoint($"media-{asset}-open");
                player.Open(new Uri(path));
                player.Volume = 0;
                player.Play();
                WaitMedia(() => opened == asset + 1, () => failure, "MediaOpened");
                if (!player.HasVideo || !player.HasAudio || player.NaturalVideoWidth != 2 || player.NaturalVideoHeight != 1)
                    throw new InvalidOperationException("Opened media has unexpected video/audio/dimensions.");
                byte[]? previous = null;
                foreach (double position in new[] { 0.5, 2.5 })
                {
                    player.Position = TimeSpan.FromSeconds(position);
                    byte[]? pixels = null;
                    Checkpoint($"media-{asset}-position-{position}");
                    WaitMedia(() =>
                    {
                        if (player.Position.TotalSeconds < position || player.Position.TotalSeconds >= position + 1) return false;
                        pixels = CopyMedia(visual);
                        bool visible = pixels.Where((_, index) => index % 4 != 3).Any(value => value != 0);
                        return visible && (previous is null || !pixels.AsSpan().SequenceEqual(previous));
                    }, () => failure, "decoded frame and position");
                    player.Pause();
                    string name = $"media-{asset}-{(int)position}.pbgra32";
                    File.WriteAllBytes(Path.Combine(output, name), pixels!);
                    frames.Add(new { asset, requestedPosition = position, actualPosition = player.Position.TotalSeconds,
                        player.HasVideo, player.HasAudio, player.NaturalVideoWidth, player.NaturalVideoHeight,
                        file = name, sha256 = Convert.ToHexString(SHA256.HashData(pixels!)), sample = Convert.ToHexString(pixels!.AsSpan(0, 8)) });
                    previous = pixels;
                    player.Play();
                }
                player.Pause();
                var replacementDrawing = new VideoDrawing { Player = player, Rect = drawing.Rect };
                using (DrawingContext context = visual.RenderOpen()) context.DrawDrawing(replacementDrawing);
                drawing.Player = null;
                drawing = replacementDrawing;
                byte[] retained = CopyMedia(visual);
                if (!retained.AsSpan().SequenceEqual(previous)) throw new InvalidOperationException("Resource reference replacement changed the paused frame.");
                player.Position = TimeSpan.FromSeconds(3.6);
                player.Play();
                WaitMedia(() => ended == asset + 1, () => failure, "MediaEnded");
            }
            VerifyParallelMediaClose(events);
        }
        finally
        {
            player.Close();
            drawing.Player = null;
            using (DrawingContext context = visual.RenderOpen()) { }
            Evidence["mediaClosed"] = new { player.HasVideo, player.HasAudio, opened, ended };
        }
    }

    private static void VerifyParallelMediaClose(List<object> events)
    {
        var first = new MediaPlayer();
        var second = new MediaPlayer();
        var failed = new MediaPlayer();
        int closed = 0, failures = 0;
        Exception? error = null;
        first.MediaOpened += (_, _) => { first.Close(); closed++; events.Add(new { kind = "parallel-close-first" }); };
        second.MediaOpened += (_, _) => { second.Close(); closed++; events.Add(new { kind = "parallel-close-second" }); };
        first.MediaFailed += (_, e) => error = e.ErrorException;
        second.MediaFailed += (_, e) => error = e.ErrorException;
        failed.MediaFailed += (_, e) => { failures++; failed.Close(); events.Add(new { kind = "expected-failed-close", hresult = e.ErrorException.HResult }); };
        try
        {
            first.Open(new Uri(Path.Combine(output, "media-0.avi")));
            second.Open(new Uri(Path.Combine(output, "media-1.avi")));
            failed.Open(new Uri(Path.Combine(output, "missing-asset.avi")));
            WaitMedia(() => closed == 2 && failures == 1, () => error, "parallel callback closes and MediaFailed");
        }
        finally { first.Close(); second.Close(); failed.Close(); }
    }

    private static int CompareMedia()
    {
        string destination = Path.Combine(AppContext.BaseDirectory, "Results", "diff-media");
        Directory.CreateDirectory(destination);
        var comparisons = new List<object>();
        bool green = true;
        foreach (int asset in new[] { 0, 1 })
        {
            foreach (int time in new[] { 0, 2 })
            {
                string name = $"media-{asset}-{time}.pbgra32";
                byte[] original = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Baseline", name));
                byte[] candidate = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "ComparisonCandidate", name));
                bool equal = original.Length == 2048 && original.AsSpan().SequenceEqual(candidate);
                green &= equal;
                comparisons.Add(new { name, equal, bytes = original.Length, originalSha256 = Convert.ToHexString(SHA256.HashData(original)), candidateSha256 = Convert.ToHexString(SHA256.HashData(candidate)) });
            }
        }
        File.WriteAllText(Path.Combine(destination, "comparison.json"), System.Text.Json.JsonSerializer.Serialize(new { status = green ? "Green" : "Red", threshold = 0, comparisons }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        return green ? 0 : 1;
    }

    private static byte[] CopyMedia(DrawingVisual visual)
    {
        var bitmap = new RenderTargetBitmap(32, 16, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        byte[] pixels = new byte[32 * 16 * 4];
        bitmap.CopyPixels(pixels, 32 * 4, 0);
        bitmap.Clear();
        return pixels;
    }

    private static void WaitMedia(Func<bool> completed, Func<Exception?> failure, string condition)
    {
        var frame = new DispatcherFrame();
        var clock = Stopwatch.StartNew();
        Exception? error = null;
        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(25) };
        timer.Tick += (_, _) =>
        {
            try
            {
                if (failure() is Exception mediaError) throw new InvalidOperationException("MediaFailed before " + condition, mediaError);
                if (completed()) frame.Continue = false;
                else if (clock.Elapsed > TimeSpan.FromSeconds(15)) throw new TimeoutException("No bounded completion: " + condition);
            }
            catch (Exception exception) { error = exception; frame.Continue = false; }
        };
        timer.Start();
        try { Dispatcher.PushFrame(frame); }
        finally { timer.Stop(); }
        if (error is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
    }

    private static void WriteMediaAsset(string path, int asset)
    {
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream, Encoding.ASCII);
        long Begin(string id, string? type = null)
        {
            writer.Write(Encoding.ASCII.GetBytes(id)); writer.Write(0);
            long start = stream.Position;
            if (type is not null) writer.Write(Encoding.ASCII.GetBytes(type));
            return start;
        }
        void End(long start)
        {
            long end = stream.Position;
            stream.Position = start - 4; writer.Write(checked((int)(end - start))); stream.Position = end;
        }
        long riff = Begin("RIFF", "AVI "), header = Begin("LIST", "hdrl"), main = Begin("avih");
        writer.Write(100000); writer.Write(16080); writer.Write(0); writer.Write(0);
        writer.Write(40); writer.Write(0); writer.Write(2); writer.Write(1600); writer.Write(2); writer.Write(1);
        writer.Write(new byte[16]); End(main);
        long list = Begin("LIST", "strl"), info = Begin("strh");
        writer.Write(Encoding.ASCII.GetBytes("vidsDIB ")); writer.Write(0); writer.Write(0);
        writer.Write(0); writer.Write(1); writer.Write(10); writer.Write(0); writer.Write(40);
        writer.Write(8); writer.Write(-1); writer.Write(0);
        writer.Write((short)0); writer.Write((short)0); writer.Write((short)2); writer.Write((short)1); End(info);
        long format = Begin("strf");
        writer.Write(40); writer.Write(2); writer.Write(1); writer.Write((short)1); writer.Write((short)24);
        writer.Write(0); writer.Write(8); writer.Write(new byte[16]); End(format); End(list);
        list = Begin("LIST", "strl"); info = Begin("strh");
        writer.Write(Encoding.ASCII.GetBytes("auds")); writer.Write(0); writer.Write(0); writer.Write(0);
        writer.Write(0); writer.Write(2); writer.Write(16000); writer.Write(0); writer.Write(32000);
        writer.Write(1600); writer.Write(-1); writer.Write(2); writer.Write(new byte[8]); End(info);
        format = Begin("strf");
        writer.Write((short)1); writer.Write((short)1); writer.Write(8000); writer.Write(16000);
        writer.Write((short)2); writer.Write((short)16); End(format); End(list); End(header);
        long frames = Begin("LIST", "movi");
        for (int frame = 0; frame < 40; frame++)
        {
            uint color = (asset, frame < 20) switch { (0, true) => 0xffff0000, (0, false) => 0xff0000ff, (1, true) => 0xff00ff00, _ => 0xffffff00 };
            long chunk = Begin("00db");
            writer.Write(new byte[] { (byte)color, (byte)(color >> 8), (byte)(color >> 16), (byte)color, (byte)(color >> 8), (byte)(color >> 16), 0, 0 });
            End(chunk);
            chunk = Begin("01wb");
            for (int sample = 0; sample < 800; sample++) writer.Write((short)(1000 * Math.Sin(2 * Math.PI * 440 * (frame * 800 + sample) / 8000)));
            End(chunk);
        }
        End(frames); End(riff);
    }
}
