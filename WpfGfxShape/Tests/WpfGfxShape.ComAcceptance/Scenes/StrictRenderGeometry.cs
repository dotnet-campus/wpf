using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.ComAcceptance;

internal static partial class StrictRenderBusiness
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RenderGeometry()
    {
        Evidence["caseId"] = "E2E-05-fixed-geometry-v1";
        Evidence["consumer"] = "RTB, not desktop HWND/resize";
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
        int red = (3 * 32 + 3) * 4;
        Evidence["redActual"] = Convert.ToHexString(pixels.AsSpan(red, 4));
        Checkpoint("original-pixel-assertions");
        if (!pixels.AsSpan(0, 4).SequenceEqual(new byte[] { 255, 255, 255, 255 })) throw new InvalidOperationException("Original scene background is not white.");
        if (!pixels.AsSpan(red, 4).SequenceEqual(new byte[] { 0, 0, 255, 255 })) throw new InvalidOperationException("Scene red rectangle was not rendered.");
        Evidence["originalAssertionsPassed"] = true;
        bitmap.Clear();
        Checkpoint("RenderTargetBitmap-cleared");
    }
}
