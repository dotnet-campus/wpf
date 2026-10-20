namespace NativeAcceptance.Pcore;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

internal static partial class Program
{
    private static string[] NestedCases = ["image-geometry", "drawing-geometry", "visual-geometry", "image-mask", "drawing-mask", "visual-mask", "drawing-nested", "visual-effect"];
    private static readonly string[] BlurCases = ["visual-blur-zero", "visual-blur-gaussian", "visual-blur-box", "visual-blur-quality", "visual-blur-box-quality", "visual-blur-change", "visual-blur-fractional"];

    private static void RenderNested()
    {
        Evidence["caseId"] = "E2E-05-nested-brush-lifetime-v1";
        var results = new List<object>();
        foreach (string name in NestedCases)
        {
            try
            {
                var image = new WriteableBitmap(8, 8, 96, 96, PixelFormats.Pbgra32, null);
                var geometry = new RectangleGeometry(new Rect(0, 0, 8, 8));
                var drawing = new GeometryDrawing(Brushes.Red, null, geometry);
                var child = new DrawingVisual();
                using (DrawingContext dc = child.RenderOpen()) dc.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 8, 8));
                TileBrush brush = name.StartsWith("image", StringComparison.Ordinal) ? new ImageBrush(image)
                    : name.StartsWith("visual", StringComparison.Ordinal) ? new VisualBrush(child) : new DrawingBrush(drawing);
                brush.ViewportUnits = BrushMappingMode.Absolute;
                brush.Viewport = new Rect(0, 0, 24, 24);
                brush.ViewboxUnits = BrushMappingMode.Absolute;
                brush.Viewbox = new Rect(0, 0, 8, 8);
                var root = new DrawingVisual();
                RenderOptions.SetBitmapScalingMode(root, BitmapScalingMode.NearestNeighbor);
                if (name.EndsWith("effect", StringComparison.Ordinal) || name.StartsWith("visual-blur-", StringComparison.Ordinal))
                    root.Effect = new BlurEffect
                    {
                        Radius = name.EndsWith("zero", StringComparison.Ordinal) ? 0 : name.EndsWith("fractional", StringComparison.Ordinal) ? 2.75 : 2,
                        KernelType = name.Contains("box", StringComparison.Ordinal) ? KernelType.Box : KernelType.Gaussian,
                        RenderingBias = name.Contains("quality", StringComparison.Ordinal) ? RenderingBias.Quality : RenderingBias.Performance
                    };
                using (DrawingContext dc = root.RenderOpen())
                {
                    dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, 32, 32));
                    dc.PushClip(new RectangleGeometry(new Rect(2, 2, 26, 26)));
                    if (name.EndsWith("mask", StringComparison.Ordinal)) dc.PushOpacityMask(brush);
                    dc.PushOpacity(0.5);
                    if (name == "drawing-nested")
                    {
                        var group = new DrawingGroup { ClipGeometry = new RectangleGeometry(new Rect(1, 1, 22, 22)), Opacity = 0.5, OpacityMask = brush };
                        group.Children.Add(new GeometryDrawing(brush, null, new RectangleGeometry(new Rect(0, 0, 24, 24))));
                        dc.DrawDrawing(group);
                    }
                    else dc.DrawGeometry(brush, null, new RectangleGeometry(new Rect(0, 0, 24, 24)));
                    dc.Pop();
                    if (name.EndsWith("mask", StringComparison.Ordinal)) dc.Pop();
                    dc.Pop();
                }
                byte[]? previous = null;
                for (int frame = 0; frame < 2; frame++)
                {
                    if (name == "visual-blur-change" && root.Effect is BlurEffect changing) changing.Radius = frame == 0 ? 2 : 5;
                    byte[] source = new byte[256];
                    for (int i = 0; i < source.Length; i += 4) { source[i + (frame == 0 ? 2 : 0)] = 255; source[i + 3] = 255; }
                    image.WritePixels(new Int32Rect(0, 0, 8, 8), source, 32, 0);
                    drawing.Brush = frame == 0 ? Brushes.Red : Brushes.Blue;
                    using (DrawingContext dc = child.RenderOpen()) dc.DrawRectangle(frame == 0 ? Brushes.Red : Brushes.Blue, null, new Rect(0, 0, 8, 8));
                    Checkpoint(name + "-frame-" + frame);
                    var target = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
                    target.Render(root);
                    byte[] pixels = new byte[4096];
                    target.CopyPixels(pixels, 128, 0);
                    int coloredPixels = Enumerable.Range(0, 1024).Count(i => pixels[i * 4 + 3] != 0 && (pixels[i * 4] != pixels[i * 4 + 1] || pixels[i * 4 + 1] != pixels[i * 4 + 2]));
                    if (coloredPixels == 0 || previous is not null && previous.AsSpan().SequenceEqual(pixels)) throw new InvalidOperationException("Nested content must be colored and replacement must change pixels.");
                    string id = name + "-" + frame;
                    File.WriteAllBytes(Path.Combine(output, id + ".pbgra32"), pixels);
                    results.Add(new { name = id, status = "Green", bytes = pixels.Length, coloredPixels, sha256 = Convert.ToHexString(SHA256.HashData(pixels)) });
                    previous = pixels;
                    target.Clear();
                }
                if (brush is VisualBrush vb) vb.Visual = null;
                if (brush is DrawingBrush db) db.Drawing = null;
                if (brush is ImageBrush ib) ib.ImageSource = null;
                root.Effect = null;
                using (root.RenderOpen()) { }
                using (child.RenderOpen()) { }
            }
            catch (Exception exception)
            {
                results.Add(new { name, status = "Red", hresult = $"0x{exception.HResult:X8}", exception = exception.ToString() });
            }
        }
        File.WriteAllText(Path.Combine(output, "nested-scenes.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        Evidence["nestedScenes"] = results;
    }

    private static int CompareNested()
    {
        var results = new List<object>();
        bool passed = true;
        foreach (string name in NestedCases)
            for (int frame = 0; frame < (NestedCases == LifetimeCases ? 6 : 2); frame++)
            {
                string id = name + "-" + frame;
                string original = Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "Baseline", id + ".pbgra32");
                string candidate = Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "ComparisonCandidate", id + ".pbgra32");
                byte[] a = File.Exists(original) ? File.ReadAllBytes(original) : [];
                byte[] b = File.Exists(candidate) ? File.ReadAllBytes(candidate) : [];
                bool equal = a.Length == 4096 && a.AsSpan().SequenceEqual(b);
                passed &= equal;
                results.Add(new { name = id, status = equal ? "Green" : "Red", threshold = 0, originalBytes = a.Length, candidateBytes = b.Length, differingBytes = a.Length == b.Length ? a.Zip(b).Count(p => p.First != p.Second) : -1, originalSha256 = Convert.ToHexString(SHA256.HashData(a)), candidateSha256 = Convert.ToHexString(SHA256.HashData(b)) });
            }
        string destination = Path.Combine(WpfGfxShape.ComAcceptance.NativeRun.PathsRoot, "Results", "diff");
        Directory.CreateDirectory(destination);
        File.WriteAllText(Path.Combine(destination, "nested-comparison.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        return passed ? 0 : 1;
    }
}
