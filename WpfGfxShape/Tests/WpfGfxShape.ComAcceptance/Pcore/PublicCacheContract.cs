namespace NativeAcceptance.Pcore;

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static partial class Program
{
    private static void RenderPublicCacheContract()
    {
        Evidence["caseId"] = "public-offtree-cache-contract-v1";
        Evidence["consumer"] = "Public RenderTargetBitmap; not original HAL HWND consumer";
        var observations = new List<object>();
        Evidence["cacheContract"] = observations;
        foreach (bool cached in new[] { false, true })
        {
            var image = new WriteableBitmap(12, 1, 96, 96, PixelFormats.Pbgra32, null);
            var source = new DrawingVisual();
            using (var drawing = source.RenderOpen()) drawing.DrawImage(image, new Rect(0, 0, 32, 16));
            Brush brush = cached ? new BitmapCacheBrush(source) { BitmapCache = new BitmapCache(1) } : new VisualBrush(source);
            var consumer = new DrawingVisual();
            using (var drawing = consumer.RenderOpen()) drawing.DrawRectangle(brush, null, new Rect(0, 0, 32, 16));
            var target = new RenderTargetBitmap(32, 16, 96, 96, PixelFormats.Pbgra32);
            try
            {
                foreach (uint expected in new[] { 0xff00ff00u, 0xffff0000u })
                {
                    image.WritePixels(new Int32Rect(0, 0, 12, 1), Enumerable.Repeat(expected, 12).ToArray(), 48, 0);
                    target.Clear();
                    target.Render(consumer);
                    uint[] pixels = new uint[512];
                    target.CopyPixels(pixels, 128, 0);
                    string name = $"{(cached ? "cache" : "visual")}-{expected:X8}";
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(output, name + ".bgra"), MemoryMarshal.AsBytes(pixels.AsSpan()).ToArray());
                    var bounds = VisualTreeHelper.GetContentBounds(source);
                    observations.Add(new
                    {
                        name, expected = expected.ToString("X8"), actual = pixels.Select(p => p.ToString("X8")).Distinct().ToArray(),
                        parentIsNull = VisualTreeHelper.GetParent(source) == null, isUIElement = (object)source is UIElement,
                        bounds = new { bounds.X, bounds.Y, bounds.Width, bounds.Height },
                        brush.Opacity, transform = brush.Transform?.Value.ToString(), relativeTransform = brush.RelativeTransform?.Value.ToString(),
                        autoLayout = cached ? ((BitmapCacheBrush)brush).AutoLayoutContent : ((VisualBrush)brush).AutoLayoutContent,
                        renderAtScale = cached ? ((BitmapCacheBrush)brush).BitmapCache.RenderAtScale : (double?)null,
                        matches = pixels.Count(p => p == expected), count = pixels.Length
                    });
                    Checkpoint(name);
                    if (pixels.Any(p => p != expected)) throw new InvalidOperationException(name);
                }
            }
            finally
            {
                using (consumer.RenderOpen()) { }
                if (brush is BitmapCacheBrush cacheBrush) cacheBrush.Target = null;
                if (brush is VisualBrush visualBrush) visualBrush.Visual = null;
                target.Clear();
                Evidence["cacheSceneDetached"] = true;
                Checkpoint("cache-scene-detached");
            }
        }
    }
}
