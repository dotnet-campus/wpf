using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.ComAcceptance;

internal static class StrictRenderCase
{
    internal const string Rectangle = "geometry-public-r27";
    internal const string ClosedBezier = "closed-path-bezier-v1";
    internal const string TranslatedScaledBezier = "closed-path-bezier-translate-scale-v1";
    internal const string RotatedBezier = "closed-path-bezier-rotate-v1";
    internal const string ClippedBezier = "closed-path-bezier-clip-v1";
    internal const string MediaAudioRatePreflight = "media-audio-rate-preflight-v1";
    internal const string MediaAudioSeek = "media-audio-seek-v1";
    internal const string MediaAudioInitializeClose = "media-audio-initialize-close-v1";
    internal const string MediaSource = "media-source-v1";
    internal const string MediaContinuousSeek = "media-continuous-seek-v1";
    internal const string MediaPlayPauseResume = "media-play-pause-resume-v1";
    internal const string MediaSpeedRatio = "media-speed-ratio-v1";
    internal static bool IsMedia(string value) => value is MediaAudioRatePreflight or MediaAudioSeek or MediaAudioInitializeClose or MediaSource or MediaContinuousSeek or MediaPlayPauseResume or MediaSpeedRatio;
    internal const string AlphaOverlap = "alpha-overlap-v1";
    internal static bool IsKnown(string value) => value is Rectangle or ClosedBezier or TranslatedScaledBezier or RotatedBezier or ClippedBezier or AlphaOverlap or MediaAudioRatePreflight or MediaAudioSeek or MediaAudioInitializeClose or MediaSource or MediaContinuousSeek or MediaPlayPauseResume or MediaSpeedRatio;
    internal static string CaseId(string value) => value switch
    {
        MediaAudioRatePreflight => "E2E-media-audio-rate-preflight-v1",
        MediaAudioSeek => "E2E-media-audio-seek-v1",
        MediaAudioInitializeClose => "E2E-media-audio-initialize-close-v1",
        MediaSpeedRatio => "E2E-media-speed-ratio-v1",
        MediaPlayPauseResume => "E2E-media-play-pause-resume-v1",
        MediaSource => "E2E-media-source-v1",
        MediaContinuousSeek => "E2E-media-continuous-seek-v1",
        AlphaOverlap => "E2E-alpha-overlap-v1",
        Rectangle => "E2E-05-fixed-geometry-v1",
        ClosedBezier => "E2E-closed-path-bezier-v1",
        TranslatedScaledBezier => "E2E-closed-path-bezier-translate-scale-v1",
        RotatedBezier => "E2E-closed-path-bezier-rotate-v1",
        ClippedBezier => "E2E-closed-path-bezier-clip-v1",
        _ => throw new ArgumentException("Unknown strict scene.", nameof(value))
    };
    internal static string Input(string value) => value switch
    {
        MediaAudioRatePreflight => "public MediaPlayer;no Open;11 independent players;episode0:Default,Set0.5,Pause,Set2,Play;episodes1-9:Default,Set[-1,-Infinity,-0,+0,1,Epsilon,MaxValue,+Infinity,NaN](one/player);episode10:Default,Pause,SetNaN,Play;each:ApplicationIdle,Close,Close-ApplicationIdle;cache after every operation including exceptions;double IEEE754 hex/classification;fixed continuation;barrier sampling not completion;opened=false;effectiveRate=Unknown;dspValidated=false;no pixels",
        MediaAudioSeek => "public MediaPlayer;content.bin=RIFF-WAVE PCM mono8000Hz16bit 4s silence;Open/OpenedOrFailed(bound20s)/terminal-snapshot/ifOpened:Pause,Position=0.5s,1.5s,1s(each immediate query),observe final1s or Failed(bound20s)/Close/Close-barrier;no Play/rate/Balance;no pixels;Failed is not Seek success;Position ticks strict",
        MediaAudioInitializeClose => "public MediaPlayer;content.bin=RIFF-WAVE PCM mono8000Hz16bit 4s silence;Open/OpenedOrFailed(bound20s)/terminal-snapshot/ifOpened:Pause,Volume=0/Close/Close-barrier;no Position/Seek/rate/Balance/Play;no pixels;Failed is observed not suppressed",
        ClosedBezier => "32x32;Pbgra32;stride128;96dpi;white;fill=#FF2060C0;pen=null;Nonzero;M4,6 L24,6 C29,10 26,25 16,27 L5,23 Z",
        TranslatedScaledBezier => "32x32;Pbgra32;stride128;96dpi;white;fill=#FF2060C0;pen=null;Nonzero;M4,6 L24,6 C29,10 26,25 16,27 L5,23 Z;PathGeometry.Transform=TransformGroup[Translate(1.25,-0.75),Scale(0.85,1.125,center=0,0)];order=translate-then-scale;matrix=0.85,0,0,1.125,1.0625,-0.84375;interiorPixel=12,12;expectedBGRA=C06020FF",
        RotatedBezier => "32x32;Pbgra32;stride128;96dpi;white;fill=#FF2060C0;pen=null;Nonzero;M4,6 L24,6 C29,10 26,25 16,27 L5,23 Z;PathGeometry.Transform=RotateTransform(angle=17,center=16,16);additionalTranslation=none;interiorPixel=12,12;expectedBGRA=C06020FF",
        ClippedBezier => "32x32;Pbgra32;stride128;96dpi;white;fill=#FF2060C0;pen=null;Nonzero;M4,6 L24,6 C29,10 26,25 16,27 L5,23 Z;PushClip=RectangleGeometry(Rect(8,8,12,12));order=white,PushClip,DrawGeometry,Pop;transform=none;interiorPixel=12,12;expectedBGRA=C06020FF;clippedOutPixel=22,12;expectedClippedOutBGRA=FFFFFFFF",
        AlphaOverlap => "32x32;Pbgra32;stride128;96dpi;transparent;pen=null;transform=none;brushOpacity=1;order=red,blue;red=#55FF0000,Rect(4,4,16,16);blue=#990000FF,Rect(12,12,16,16);source-over;BGRA:background(0,0)=00000000,redOnly(8,8)=00005555,blueOnly(24,24)=99000099,overlap(16,16)=990022BB;premultipliedRGB<=A",
        MediaSpeedRatio => "public MediaPlayer;AVI DIB24 purevideo 10fps;4s/2x1/red-then-blue;Open/Opened/SpeedRatio=0.5/Play/bounded-Rendering-position-and-red-frame/Pause-submitted/SpeedRatio=2/Play/bounded-Rendering-position-and-blue-frame/Close;each-observation-bound=5s;public-DrawVideo-2x1-readback-is-frame-presence-not-pixel-certification;PauseCompleted=Unavailable;audio=Unknown;raw-strict-no-normalization",
        MediaPlayPauseResume => "public MediaPlayer;AVI DIB24 purevideo 10fps;4s/2x1/red-then-blue;Open/Opened/Play/Rendering-position-advance/Pause/3-Rendering-observations/Resume-via-Play/Rendering-position-advance/rapid-Pause-Play/Rendering-position-advance/Close;each-observation-bound=5s;Pause-return=submission;pause-completion=Unknown;freeze-and-resume-continuity=Undecidable;no-pixel-certification;raw-strict-no-normalization",
        MediaContinuousSeek => "public MediaPlayer;AVI DIB24 purevideo 10fps;4s/2x1/red-then-blue;Open/Opened/Pause/ScrubbingEnabled=true/PositionTicks=5000000,25000000,10000000/no-wait-between-submissions/ApplicationIdle-observation/Close/ApplicationIdle;completion=Unknown;paused-state=Unknown;no-frame-completion-signal;no-pixel-certification",
        MediaSource => "public MediaPlayer;AVI DIB24 purevideo 10fps;A=1s/2x1/red;B=2s/4x2/green;OpenA/Opened/OpenB/Opened/OpenNull/dispatcher-barrier/OpenA/Opened/Close/dispatcher-barrier;same-code;no-play;no-visual-cache",
        Rectangle => "32x32;Pbgra32;stride128;96dpi;legacy-fixed-geometry-v1",
        _ => throw new ArgumentException("Unknown strict scene.", nameof(value))
    };
    internal static bool MetadataMatches(System.Text.Json.JsonElement receipt, string value)
    {
        if (IsMedia(value)) return receipt.TryGetProperty("mediaContractPassed", out var passed) && passed.GetBoolean()
            && receipt.GetProperty("caseId").GetString() == CaseId(value) && receipt.GetProperty("sceneInput").GetString() == Input(value);
        if (!IsKnown(value) || !receipt.TryGetProperty("imageMetadata", out var m)) return false;
        return receipt.GetProperty("caseId").GetString() == CaseId(value)
            && receipt.GetProperty("sceneInput").GetString() == Input(value)
            && m.GetProperty("width").GetInt32() == 32 && m.GetProperty("height").GetInt32() == 32
            && m.GetProperty("stride").GetInt32() == 128 && m.GetProperty("format").GetString() == "Pbgra32"
            && m.GetProperty("dpiX").GetDouble() == 96 && m.GetProperty("dpiY").GetDouble() == 96;
    }
    internal static bool AlphaPixelsMatch(byte[] data)
    {
        if (data.Length != 4096) return false;
        foreach (var sample in new[] { (x: 0, y: 0, bgra: "00000000"), (x: 8, y: 8, bgra: "00005555"), (x: 24, y: 24, bgra: "99000099"), (x: 16, y: 16, bgra: "990022BB") })
            if (Convert.ToHexString(data.AsSpan((sample.y * 32 + sample.x) * 4, 4)) != sample.bgra) return false;
        for (int i = 0; i < data.Length; i += 4)
            if (data[i] > data[i + 3] || data[i + 1] > data[i + 3] || data[i + 2] > data[i + 3]) return false;
        return true;
    }
    internal static int ByteCount(System.Text.Json.JsonElement receipt) => checked(receipt.GetProperty("imageMetadata").GetProperty("stride").GetInt32() * receipt.GetProperty("imageMetadata").GetProperty("height").GetInt32());
}

internal static partial class StrictRenderBusiness
{
    private static string sceneCase = StrictRenderCase.Rectangle;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RenderGeometry()
    {
        Evidence["caseId"] = StrictRenderCase.CaseId(sceneCase);
        Evidence["sceneInput"] = StrictRenderCase.Input(sceneCase);
        Evidence["consumer"] = "RTB, not desktop HWND/resize";
        Checkpoint("DrawingVisual-construction");
        var visual = new DrawingVisual();
        Checkpoint("DrawingContext-open");
        using (DrawingContext dc = visual.RenderOpen())
        {
            if (sceneCase != StrictRenderCase.AlphaOverlap) dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, 32, 32));
            if (sceneCase == StrictRenderCase.AlphaOverlap)
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(85, 255, 0, 0)), null, new Rect(4, 4, 16, 16));
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(153, 0, 0, 255)), null, new Rect(12, 12, 16, 16));
            }
            else if (sceneCase is StrictRenderCase.ClosedBezier or StrictRenderCase.TranslatedScaledBezier or StrictRenderCase.RotatedBezier or StrictRenderCase.ClippedBezier)
            {
                var figure = new PathFigure { StartPoint = new Point(4, 6), IsClosed = true, IsFilled = true };
                figure.Segments.Add(new LineSegment(new Point(24, 6), true));
                figure.Segments.Add(new BezierSegment(new Point(29, 10), new Point(26, 25), new Point(16, 27), true));
                figure.Segments.Add(new LineSegment(new Point(5, 23), true));
                var geometry = new PathGeometry(new[] { figure }, FillRule.Nonzero, null);
                if (sceneCase == StrictRenderCase.TranslatedScaledBezier)
                {
                    var transform = new TransformGroup();
                    transform.Children.Add(new TranslateTransform(1.25, -0.75));
                    transform.Children.Add(new ScaleTransform(0.85, 1.125));
                    geometry.Transform = transform;
                }
                else if (sceneCase == StrictRenderCase.RotatedBezier)
                {
                    geometry.Transform = new RotateTransform(17, 16, 16);
                }
                if (sceneCase == StrictRenderCase.ClippedBezier) dc.PushClip(new RectangleGeometry(new Rect(8, 8, 12, 12)));
                dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(255, 32, 96, 192)), null, geometry);
                if (sceneCase == StrictRenderCase.ClippedBezier) dc.Pop();
            }
            else
            {
            dc.DrawRectangle(Brushes.Red, null, new Rect(2, 2, 12, 10));
            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(128, 0, 0, 255)), null, new Rect(8, 5, 12, 12));
            dc.PushTransform(new TranslateTransform(3, 2));
            dc.DrawGeometry(Brushes.Green, new Pen(Brushes.Black, 1), Geometry.Parse("M 1,22 L 10,18 18,27 Z"));
            dc.DrawLine(new Pen(Brushes.Black, 2), new Point(20, 2), new Point(25, 15));
            dc.Pop();
            }
        }
        Checkpoint("RenderTargetBitmap-construction");
        var bitmap = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
        Checkpoint("RenderTargetBitmap-Render");
        bitmap.Render(visual);
        Checkpoint("CopyPixels");
        byte[] pixels = new byte[4096];
        bitmap.CopyPixels(pixels, 128, 0);
        File.WriteAllBytes(Path.Combine(output, "pixels.pbgra32"), pixels);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using (FileStream image = File.Create(Path.Combine(output, "image.png"))) encoder.Save(image);
        Evidence["pixelsSha256"] = Convert.ToHexString(SHA256.HashData(pixels));
        Evidence["pixelByteCount"] = pixels.Length;
        Evidence["imageMetadata"] = new { width = bitmap.PixelWidth, height = bitmap.PixelHeight, stride = 128, format = bitmap.Format.ToString(), dpiX = bitmap.DpiX, dpiY = bitmap.DpiY };
        Evidence["backgroundActual"] = Convert.ToHexString(pixels.AsSpan(0, 4));
        int red = sceneCase != StrictRenderCase.Rectangle ? (12 * 32 + 12) * 4 : (3 * 32 + 3) * 4;
        Evidence["redActual"] = Convert.ToHexString(pixels.AsSpan(red, 4));
        Checkpoint("original-pixel-assertions");
        if (sceneCase == StrictRenderCase.AlphaOverlap)
        {
            if (!StrictRenderCase.AlphaPixelsMatch(pixels)) throw new InvalidOperationException("Registered alpha source-over pixels or premultiplication failed.");
        }
        else
        {
        if (!pixels.AsSpan(0, 4).SequenceEqual(new byte[] { 255, 255, 255, 255 })) throw new InvalidOperationException("Original scene background is not white.");
        byte[] interior = sceneCase != StrictRenderCase.Rectangle ? [192, 96, 32, 255] : [0, 0, 255, 255];
        if (!pixels.AsSpan(red, 4).SequenceEqual(interior)) throw new InvalidOperationException("Scene interior was not rendered.");
        if (sceneCase == StrictRenderCase.ClippedBezier)
        {
            ReadOnlySpan<byte> clippedOut = pixels.AsSpan((12 * 32 + 22) * 4, 4);
            Evidence["clippedOutActual"] = Convert.ToHexString(clippedOut);
            if (!clippedOut.SequenceEqual(new byte[] { 255, 255, 255, 255 })) throw new InvalidOperationException("Scene clip exterior was not white.");
        }
        }
        Evidence["originalAssertionsPassed"] = true;
        bitmap.Clear();
        Checkpoint("RenderTargetBitmap-cleared");
    }
}
