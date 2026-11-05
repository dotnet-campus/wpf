using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NativeAcceptance.Pcore;

internal static partial class Program
{
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

    private static void WriteMediaAsset(string path, int asset, bool includeAudio = true)
    {
        if (includeAudio || asset != 0) throw new ArgumentOutOfRangeException(nameof(asset));
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
        writer.Write(40); writer.Write(0); writer.Write(1); writer.Write(1600); writer.Write(2); writer.Write(1);
        writer.Write(new byte[16]); End(main);
        long list = Begin("LIST", "strl"), info = Begin("strh");
        writer.Write(Encoding.ASCII.GetBytes("vidsDIB ")); writer.Write(0); writer.Write(0);
        writer.Write(0); writer.Write(1); writer.Write(10); writer.Write(0); writer.Write(40);
        writer.Write(8); writer.Write(-1); writer.Write(0);
        writer.Write((short)0); writer.Write((short)0); writer.Write((short)2); writer.Write((short)1); End(info);
        long format = Begin("strf");
        writer.Write(40); writer.Write(2); writer.Write(1); writer.Write((short)1); writer.Write((short)24);
        writer.Write(0); writer.Write(8); writer.Write(new byte[16]); End(format); End(list); End(header);
        long frames = Begin("LIST", "movi");
        for (int frame = 0; frame < 40; frame++)
        {
            uint color = frame < 20 ? 0xffff0000 : 0xff0000ff;
            long chunk = Begin("00db");
            writer.Write(new byte[] { (byte)color, (byte)(color >> 8), (byte)(color >> 16), (byte)color, (byte)(color >> 8), (byte)(color >> 16), 0, 0 });
            End(chunk);
        }
        End(frames); End(riff);
    }
}
