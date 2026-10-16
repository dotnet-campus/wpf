using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

internal static partial class Program
{
    private static readonly string[] LifetimeCases = ["lifetime-image-live", "lifetime-image-frozen", "lifetime-drawing-live", "lifetime-drawing-frozen", "lifetime-visual-live", "lifetime-shared-blur"];

    private static void RenderLifetime()
    {
        Evidence["caseId"] = "E2E-05-public-multiframe-lifetime-v1";
        var results = new List<object>();
        foreach (string name in LifetimeCases)
            RenderLifetimeCase(name, results);
        File.WriteAllText(Path.Combine(output, "lifetime-scenes.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        Evidence["lifetimeScenes"] = results;
    }

    private static void RenderLifetimeCase(string name, List<object> results)
    {
        bool frozen = name.EndsWith("frozen", StringComparison.Ordinal);
        var child = new DrawingVisual();
        var geometry = new RectangleGeometry(new Rect(0, 0, 8, 8));
        TileBrush brush = name.Contains("image", StringComparison.Ordinal) ? new ImageBrush()
            : name.Contains("drawing", StringComparison.Ordinal) ? new DrawingBrush() : new VisualBrush(child);
        brush.ViewboxUnits = BrushMappingMode.Absolute;
        brush.Viewbox = new Rect(0, 0, 8, 8);
        brush.ViewportUnits = BrushMappingMode.Absolute;
        brush.Viewport = new Rect(0, 0, 12, 12);
        var effect = new BlurEffect { Radius = 2 };
        var root = new DrawingVisual();
        RenderOptions.SetBitmapScalingMode(root, BitmapScalingMode.NearestNeighbor);
        var retained = new List<(RenderTargetBitmap Target, byte[] Pixels)>();
        for (int frame = 0; frame < 6; frame++)
        {
            int current = frame;
            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                Color color = current % 3 == 0 ? Colors.Red : current % 3 == 1 ? Colors.Blue : Colors.Green;
                var solid = new SolidColorBrush(color);
                if (frozen) solid.Freeze();
                if (brush is ImageBrush image)
                {
                    byte[] data = new byte[256];
                    for (int i = 0; i < data.Length; i += 4)
                    {
                        data[i] = color.B; data[i + 1] = color.G; data[i + 2] = color.R; data[i + 3] = 255;
                    }
                    var source = BitmapSource.Create(8, 8, 96, 96, PixelFormats.Pbgra32, null, data, 32);
                    if (frozen) source.Freeze();
                    image.ImageSource = source;
                }
                if (brush is DrawingBrush drawing)
                {
                    var source = new GeometryDrawing(solid, null, geometry);
                    if (frozen) source.Freeze();
                    drawing.Drawing = source;
                }
                using (DrawingContext dc = child.RenderOpen()) dc.DrawRectangle(solid, null, new Rect(0, 0, 8, 8));
                effect.Radius = current % 2 == 0 ? 2 : 3;
                effect.KernelType = current < 3 ? KernelType.Gaussian : KernelType.Box;
                root.Effect = name == "lifetime-shared-blur" ? effect : null;
                brush.Opacity = current % 2 == 0 ? 1 : 0.5;
                TileBrush frameBrush = frozen ? brush.CloneCurrentValue() : brush;
                if (frozen) frameBrush.Freeze();
                using DrawingContext context = root.RenderOpen();
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, 32, 32));
                context.DrawRectangle(frameBrush, null, new Rect(2, 2, 12, 12));
                context.DrawRectangle(frameBrush, null, new Rect(16, 16, 12, 12));
            }, DispatcherPriority.Normal).Wait();
            var target = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
            target.Render(root);
            byte[] pixels = ReadLifetimePixels(target);
            int colored = Enumerable.Range(0, 1024).Count(i => pixels[i * 4 + 3] != 0 && (pixels[i * 4] != pixels[i * 4 + 1] || pixels[i * 4 + 1] != pixels[i * 4 + 2]));
            if (colored == 0 || retained.Count > 0 && retained[^1].Pixels.AsSpan().SequenceEqual(pixels))
                throw new InvalidOperationException("Each lifecycle frame must be colored and change.");
            target.Freeze();
            bool rejected = false;
            try { target.Render(root); }
            catch (ArgumentException exception) when (exception.HResult == unchecked((int)0x80070057)) { rejected = true; }
            if (!rejected || !ReadLifetimePixels(target).AsSpan().SequenceEqual(pixels))
                throw new InvalidOperationException("Frozen target rejection must preserve its committed pixels.");
            foreach (var old in retained)
                if (!ReadLifetimePixels(old.Target).AsSpan().SequenceEqual(old.Pixels))
                    throw new InvalidOperationException("A retained old frame was contaminated by source replacement.");
            retained.Add((target, pixels));
            string id = name + "-" + frame;
            File.WriteAllBytes(Path.Combine(output, id + ".pbgra32"), pixels);
            results.Add(new { name = id, status = "Green", bytes = pixels.Length, coloredPixels = colored, frozenInputs = frozen, frozenTargetRejected = rejected, retainedFramesVerified = retained.Count, sha256 = Convert.ToHexString(SHA256.HashData(pixels)) });
        }
        if (brush is ImageBrush ib) ib.ImageSource = null;
        if (brush is DrawingBrush db) db.Drawing = null;
        if (brush is VisualBrush vb) vb.Visual = null;
        root.Effect = null;
        using (root.RenderOpen()) { }
        using (child.RenderOpen()) { }
        foreach (var old in retained)
            if (!ReadLifetimePixels(old.Target).AsSpan().SequenceEqual(old.Pixels))
                throw new InvalidOperationException("Detaching source ownership contaminated a retained frame.");
    }

    private static byte[] ReadLifetimePixels(BitmapSource target)
    {
        byte[] pixels = new byte[4096];
        target.CopyPixels(pixels, 128, 0);
        return pixels;
    }
}
