using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

[assembly: DoNotParallelize]

namespace WpfGfxShape.WpfReference;

[TestClass]
public sealed class ReferenceBehaviorTests
{
    public TestContext TestContext { get; set; } = null!;

    [STATestMethod]
    public void WhenPaddingConsumersUseScalingScopesThenOriginalBehaviorIsVerified()
    {
        NativeMethods.EnsureLoaded();
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
        byte[] paddedPixels = [0, 0, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        var samples = new List<object>();
        var outputs = new Dictionary<string, byte[]>();
        foreach (string consumer in new[] { "fill", "stroke", "VisualMask", "DrawingGroupMask" })
        foreach (BitmapScalingMode scaling in new[] { BitmapScalingMode.LowQuality, BitmapScalingMode.HighQuality, BitmapScalingMode.NearestNeighbor })
        {
            var source = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Pbgra32, null, paddedPixels, 8);
            var brush = new ImageBrush(source)
            {
                Viewport = new Rect(0.25, 1, 2, 2), ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 2, 2), ViewboxUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, TileMode = TileMode.Tile
            };
            var visual = new DrawingVisual();
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(visual, scaling);
            RenderOptions.SetBitmapScalingMode(group, scaling);
            RenderOptions.SetBitmapScalingMode(brush, scaling);
            RenderOptions.SetBitmapScalingMode(source, scaling);
            if (consumer == "VisualMask") visual.OpacityMask = brush;
            if (consumer == "DrawingGroupMask") group.OpacityMask = brush;
            using (DrawingContext drawing = group.Open())
            {
                if (consumer == "stroke")
                    drawing.DrawRectangle(null, new Pen(brush, 2), new Rect(1.5, 1.5, 1, 1));
                else
                    drawing.DrawRectangle(consumer == "fill" ? brush : Brushes.Red, null, new Rect(-10, -10, 20, 20));
            }
            using (DrawingContext drawing = visual.RenderOpen()) drawing.DrawDrawing(group);
            var target = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);
            var pixels = new byte[64];
            target.CopyPixels(pixels, 16, 0);
            outputs.Add($"{consumer}/{scaling}", pixels);
            samples.Add(new { Consumer = consumer, ScalingMode = scaling.ToString(),
                ScalingScope = "Visual + content DrawingGroup + source + brush; mask remains at named owner",
                PixelsBgra = pixels.Select(value => (int)value).ToArray() });
        }
        string directory = typeof(ReferenceBehaviorTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "ReferenceResultsPath").Value
            ?? throw new InvalidOperationException("The reference results directory is not configured.");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, $"padding-consumer-scopes-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            NativeDll = NativeMethods.LibraryPath,
            NativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(NativeMethods.LibraryPath))),
            SourceDpi = new[] { 96, 96 }, SourceSize = new[] { 2, 2 },
            SourcePixelsBgra = paddedPixels.Select(value => (int)value).ToArray(),
            Viewport = new[] { 0.25, 1, 2, 2 }, Viewbox = new[] { 0, 0, 2, 2 },
            TargetRectangle = new[] { -10, -10, 20, 20 }, StrokeRectangle = new[] { 1.5, 1.5, 1, 1 },
            Transform = new[] { 1, 0, 0, 1, 0, 0 }, Width = 4, Height = 4, Samples = samples
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.AddResultFile(path);
        foreach (string consumer in new[] { "fill", "stroke", "VisualMask", "DrawingGroupMask" })
        {
            TestContext.WriteLine($"{consumer}: linear/nearest equal={outputs[$"{consumer}/LowQuality"].SequenceEqual(outputs[$"{consumer}/NearestNeighbor"])}");
            CollectionAssert.AreEqual(outputs[$"{consumer}/LowQuality"], outputs[$"{consumer}/HighQuality"]);
        }
        Assert.IsFalse(outputs["fill/LowQuality"].SequenceEqual(outputs["fill/NearestNeighbor"]));
        Assert.IsFalse(outputs["stroke/LowQuality"].SequenceEqual(outputs["stroke/NearestNeighbor"]));
        CollectionAssert.AreEqual(outputs["VisualMask/LowQuality"], outputs["VisualMask/NearestNeighbor"]);
        Assert.IsFalse(outputs["DrawingGroupMask/LowQuality"].SequenceEqual(outputs["DrawingGroupMask/NearestNeighbor"]));
    }

    [STATestMethod]
    public void WhenExplicitPaddingTileIsSampledThenOriginalSourceVariantsAreRecorded()
    {
        NativeMethods.EnsureLoaded();
        byte[] paddedPixels = [0, 0, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        var samples = new List<object>();
        foreach (BitmapScalingMode scaling in new[] { BitmapScalingMode.LowQuality, BitmapScalingMode.HighQuality, BitmapScalingMode.NearestNeighbor })
        foreach (string sourceKind in new[] { "DrawingImage", "Bitmap1x1", "PaddedBitmap2x2" })
        {
            ImageSource source = sourceKind switch
            {
                "DrawingImage" => new DrawingImage(new GeometryDrawing(Brushes.Red, null, new RectangleGeometry(new Rect(0, 0, 1, 1)))),
                "Bitmap1x1" => BitmapSource.Create(1, 1, 96, 96, PixelFormats.Pbgra32, null, new byte[] { 0, 0, 255, 255 }, 4),
                _ => BitmapSource.Create(2, 2, 96, 96, PixelFormats.Pbgra32, null, paddedPixels, 8)
            };
            var brush = new ImageBrush(source)
            {
                Viewport = new Rect(0.25, 1, 2, 2), ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 2, 2), ViewboxUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, TileMode = TileMode.Tile
            };
            var visual = new DrawingVisual();
            RenderOptions.SetBitmapScalingMode(visual, scaling);
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, scaling);
            using (DrawingContext drawing = group.Open()) drawing.DrawRectangle(brush, null, new Rect(-10, -10, 20, 20));
            using (DrawingContext drawing = visual.RenderOpen()) drawing.DrawDrawing(group);
            var target = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);
            var pixels = new byte[64];
            target.CopyPixels(pixels, 16, 0);
            samples.Add(new { SourceKind = sourceKind, ScalingMode = scaling.ToString(), PixelsBgra = pixels.Select(value => (int)value).ToArray() });
        }
        string directory = typeof(ReferenceBehaviorTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "ReferenceResultsPath").Value
            ?? throw new InvalidOperationException("The reference results directory is not configured.");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, $"explicit-padding-tile-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            NativeDll = NativeMethods.LibraryPath,
            NativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(NativeMethods.LibraryPath))),
            SourceDpi = new[] { 96, 96 }, PaddedSourcePixelsBgra = paddedPixels.Select(value => (int)value).ToArray(),
            Viewport = new[] { 0.25, 1, 2, 2 }, Viewbox = new[] { 0, 0, 2, 2 },
            TargetRectangle = new[] { -10, -10, 20, 20 }, Width = 4, Height = 4, Samples = samples
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.AddResultFile(path);
        Assert.HasCount(9, samples);
    }

    [STATestMethod]
    public void WhenRelativeBitmapStrokeIsMaskedThenOriginalHalfAlphaIsRecorded()
    {
        NativeMethods.EnsureLoaded();
        byte[] sourcePixels = [0, 0, 0, 0, 0, 0, 128, 128];
        var source = BitmapSource.Create(2, 1, 96, 96, PixelFormats.Pbgra32, null, sourcePixels, 8);
        var brush = new ImageBrush(source)
        {
            Viewbox = new Rect(0, 0, 1, 1), ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
            Viewport = new Rect(0, 0, 1, 1), ViewportUnits = BrushMappingMode.RelativeToBoundingBox,
            Stretch = Stretch.Fill, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, TileMode = TileMode.None
        };
        var samples = new List<object>();
        foreach (bool scopedMask in new[] { false, true })
        foreach (bool nullPenBrush in new[] { false, true })
        foreach (bool translucentSource in new[] { false, true })
        {
            byte channel = translucentSource ? (byte)128 : (byte)255;
            byte[] sampleSource = [0, 0, 0, 0, 0, 0, channel, channel];
            brush.ImageSource = BitmapSource.Create(2, 1, 96, 96, PixelFormats.Pbgra32, null, sampleSource, 8);
            var visual = new DrawingVisual();
            if (!scopedMask) visual.OpacityMask = brush;
            using (DrawingContext drawing = visual.RenderOpen())
            {
                if (scopedMask) drawing.PushOpacityMask(brush);
                drawing.PushOpacity(1);
                drawing.DrawGeometry(brush, new Pen(nullPenBrush ? null : brush, 2) { MiterLimit = 10 },
                    new RectangleGeometry(new Rect(1, 0, 1, 1)));
                drawing.Pop();
                if (scopedMask) drawing.Pop();
            }
            var target = new RenderTargetBitmap(2, 1, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);
            var pixels = new byte[8];
            target.CopyPixels(pixels, 8, 0);
            samples.Add(new { ScopedMask = scopedMask, NullPenBrush = nullPenBrush,
                TranslucentSource = translucentSource, HasFill = true,
                SourcePixelsBgra = sampleSource.Select(value => (int)value).ToArray(),
                PixelsBgra = pixels.Select(value => (int)value).ToArray() });
        }
        string directory = typeof(ReferenceBehaviorTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "ReferenceResultsPath").Value
            ?? throw new InvalidOperationException("The reference results directory is not configured.");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, $"relative-half-alpha-stroke-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            NativeDll = NativeMethods.LibraryPath,
            NativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(NativeMethods.LibraryPath))),
            SourcePixelsBgra = sourcePixels.Select(value => (int)value).ToArray(),
            SourceDpi = new[] { 96, 96 },
            Geometry = new[] { 1, 0, 1, 1 }, PenThickness = 2, MiterLimit = 10,
            Viewbox = new[] { 0, 0, 1, 1 }, Viewport = new[] { 0, 0, 1, 1 },
            MappingUnits = "RelativeToBoundingBox", ScalingMode = "Unspecified", Samples = samples
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.AddResultFile(path);
        Assert.HasCount(8, samples);
    }

    [STATestMethod]
    public void WhenOpaqueRedIsRenderedThenTheInteriorPixelIsRed()
    {
        NativeMethods.EnsureLoaded();
        var visual = new DrawingVisual();
        using (DrawingContext drawing = visual.RenderOpen())
        {
            drawing.DrawRectangle(Brushes.Red, null, new Rect(0, 0, 4, 4));
        }
        var target = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
        target.Render(visual);
        var pixels = new byte[64];
        target.CopyPixels(pixels, 16, 0);
        CollectionAssert.AreEqual(new byte[] { 0, 0, 255, 255 }, pixels[20..24]);
    }

    [STATestMethod]
    public void WhenFiniteShapeIntersectsSourceClipThenOriginalCoverageIsRecorded()
    {
        NativeMethods.EnsureLoaded();
        var clip = new StreamGeometry();
        using (StreamGeometryContext context = clip.Open())
        {
            context.BeginFigure(new Point(0.2, -0.3), true, true);
            context.PolyLineTo(new[] { new Point(2.6, 0.4), new Point(2.1, 1.8), new Point(-0.3, 1.1) }, true, false);
        }
        Geometry intersection = Geometry.Combine(new RectangleGeometry(new Rect(0.6, 0.2, 1.9, 1.1)),
            clip, GeometryCombineMode.Intersect, null);
        var visual = new DrawingVisual();
        using (DrawingContext drawing = visual.RenderOpen()) drawing.DrawGeometry(Brushes.Red, null, intersection);
        var target = new RenderTargetBitmap(4, 2, 96, 96, PixelFormats.Pbgra32);
        target.Render(visual);
        var pixels = new byte[32];
        target.CopyPixels(pixels, 16, 0);
        string directory = typeof(ReferenceBehaviorTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "ReferenceResultsPath").Value
            ?? throw new InvalidOperationException("The reference results directory is not configured.");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, $"source-clip-intersection-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            NativeDll = NativeMethods.LibraryPath,
            NativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(NativeMethods.LibraryPath))),
            TargetRectangle = new[] { 0.6, 0.2, 1.9, 1.1 },
            ClipPoints = new[] { new[] { 0.2, -0.3 }, new[] { 2.6, 0.4 }, new[] { 2.1, 1.8 }, new[] { -0.3, 1.1 } },
            Width = 4, Height = 2, Stride = 16,
            PixelsBgra = pixels.Select(value => (int)value).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.AddResultFile(path);
        Assert.HasCount(32, pixels);
    }

    [STATestMethod]
    public void WhenTileFixturesAreRenderedThenOriginalWpfEvidenceIsRecorded()
    {
        NativeMethods.EnsureLoaded();
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
        var samples = new List<object>();
        var bitmapControlGroups = new Dictionary<string, (int Compared, int Differences)>();
        foreach (int sourceMode in new[] { 17, 18, 22, 23, 24, 25, 26, 27 })
        foreach (int affineMode in sourceMode is 17 or 18 ? new[] { 0, 3 } : new[] { 0 })
        foreach (TileMode tileMode in new[] { TileMode.None, TileMode.Tile, TileMode.FlipX, TileMode.FlipY, TileMode.FlipXY })
        foreach (BitmapScalingMode scaling in new[] { BitmapScalingMode.LowQuality, BitmapScalingMode.HighQuality, BitmapScalingMode.NearestNeighbor })
        foreach (string consumer in new[] { "fill", "stroke", "mask" })
        {
            Rect viewport = sourceMode switch
            {
                17 or 18 => new Rect(0.25, 1, 2, 2),
                22 => new Rect(-1, -1, 1e6, 1e6),
                23 => new Rect(-1, -1, 4096, 4),
                24 => new Rect(-1, -1, 4, 4096),
                25 => new Rect(-2046, -1, 4096, 4096),
                26 => new Rect(-2, -2, 16, 16),
                _ => new Rect(14, 14, 16, 16)
            };
            float[] source = sourceMode is 17 or 18 ? [1, 0, 0, 1] : [1.25f, 0, 0, 0.5f];
            var sourceBrush = new SolidColorBrush(Color.FromScRgb(source[3], source[0], source[1], source[2]));
            var imageDrawing = new GeometryDrawing(sourceBrush, null, new RectangleGeometry(new Rect(0, 0, 1, 1)));
            var image = new DrawingImage(imageDrawing);
            var brush = new ImageBrush(image)
            {
                Viewport = viewport,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = sourceMode is 17 or 18
                    ? new Rect(sourceMode == 17 ? -0.5 : 0, 0, 2, 2)
                    : new Rect(0, 0, sourceMode == 25 ? 2 : 1, 1),
                ViewboxUnits = BrushMappingMode.Absolute,
                Stretch = sourceMode is 17 or 18 ? Stretch.None : Stretch.Fill,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                TileMode = tileMode
            };
            RenderOptions.SetBitmapScalingMode(brush, scaling);
            RenderOptions.SetBitmapScalingMode(image, scaling);
            var visual = new DrawingVisual();
            RenderOptions.SetBitmapScalingMode(visual, scaling);
            if (consumer == "mask") visual.OpacityMask = brush;
            if (affineMode == 3) visual.Transform = new MatrixTransform(0.8, 0.6, -0.6, 0.8, 3, 0);
            Rect targetGeometry = sourceMode == 18 ? new Rect(-10, -10, 20, 20)
                : consumer == "stroke" ? new Rect(1.5, 1.5, 1, 1) : new Rect(0, 0, 4, 4);
            var contentGroup = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(contentGroup, scaling);
            using (DrawingContext drawing = contentGroup.Open())
            {
                if (consumer == "mask")
                {
                    drawing.DrawRectangle(Brushes.Red, null, targetGeometry);
                }
                else if (consumer == "stroke")
                {
                    drawing.DrawRectangle(null, new Pen(brush, 2), targetGeometry);
                }
                else
                {
                    drawing.DrawRectangle(brush, null, targetGeometry);
                }
            }
            using (DrawingContext drawing = visual.RenderOpen()) drawing.DrawDrawing(contentGroup);
            var target = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
            target.Render(visual);
            var pixels = new byte[64];
            target.CopyPixels(pixels, 16, 0);
            int[]? solidTargetPixels = null;
            if (sourceMode is 17 or 18)
            {
                var controlVisual = new DrawingVisual { Transform = visual.Transform };
                RenderOptions.SetBitmapScalingMode(controlVisual, scaling);
                using (DrawingContext controlDrawing = controlVisual.RenderOpen())
                {
                    controlDrawing.DrawRectangle(consumer == "stroke" ? null : Brushes.Red,
                        consumer == "stroke" ? new Pen(Brushes.Red, 2) : null, targetGeometry);
                }
                var controlTarget = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
                controlTarget.Render(controlVisual);
                var controlPixels = new byte[64];
                controlTarget.CopyPixels(controlPixels, 16, 0);
                solidTargetPixels = controlPixels.Select(value => (int)value).ToArray();
            }
            int[]? bitmapSourcePixels = null;
            if (sourceMode is 17 or 18)
            {
                var bitmapBrush = brush.Clone();
                bitmapBrush.ImageSource = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Pbgra32, null,
                    new byte[] { 0, 0, 255, 255 }, 4);
                var bitmapVisual = new DrawingVisual { Transform = visual.Transform };
                RenderOptions.SetBitmapScalingMode(bitmapVisual, scaling);
                if (consumer == "mask") bitmapVisual.OpacityMask = bitmapBrush;
                var bitmapGroup = new DrawingGroup();
                RenderOptions.SetBitmapScalingMode(bitmapGroup, scaling);
                using (DrawingContext bitmapDrawing = bitmapGroup.Open())
                {
                    bitmapDrawing.DrawRectangle(consumer == "stroke" ? null : consumer == "mask" ? Brushes.Red : bitmapBrush,
                        consumer == "stroke" ? new Pen(bitmapBrush, 2) : null, targetGeometry);
                }
                using (DrawingContext bitmapDrawing = bitmapVisual.RenderOpen()) bitmapDrawing.DrawDrawing(bitmapGroup);
                var bitmapTarget = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
                bitmapTarget.Render(bitmapVisual);
                var bitmapPixels = new byte[64];
                bitmapTarget.CopyPixels(bitmapPixels, 16, 0);
                bitmapSourcePixels = bitmapPixels.Select(value => (int)value).ToArray();
                string groupKey = $"source={sourceMode}, affine={affineMode}, scaling={scaling}";
                bitmapControlGroups.TryGetValue(groupKey, out var group);
                bitmapControlGroups[groupKey] = (group.Compared + 1,
                    group.Differences + (bitmapPixels.SequenceEqual(pixels) ? 0 : 1));
            }
            Rect bounds = targetGeometry;
            samples.Add(new
            {
                SourceMode = sourceMode,
                AffineMode = affineMode,
                VisualTransform = affineMode == 3 ? new[] { 0.8, 0.6, -0.6, 0.8, 3d, 0d } : new[] { 1d, 0d, 0d, 1d, 0d, 0d },
                TileMode = tileMode.ToString(),
                ScalingMode = scaling.ToString(),
                ScalingScope = "DrawingGroup content; Visual opacity mask scope remains separate and requires review",
                Consumer = consumer,
                Viewport = new[] { viewport.X, viewport.Y, viewport.Width, viewport.Height },
                Viewbox = new[] { brush.Viewbox.X, brush.Viewbox.Y, brush.Viewbox.Width, brush.Viewbox.Height },
                SourceScRgbRgba = source,
                SourceGeometry = new[] { 0d, 0d, 1d, 1d },
                TargetGeometry = new[] { targetGeometry.X, targetGeometry.Y, targetGeometry.Width, targetGeometry.Height },
                PenThickness = consumer == "stroke" ? 2 : 0,
                Mapping = NativeMethods.GetMapping(brush, bounds, imageDrawing.Bounds),
                OutputFormat = "Pbgra32",
                Width = 4,
                Height = 4,
                Stride = 16,
                PixelsBgra = pixels.Select(value => (int)value).ToArray(),
                SolidTargetControlPixelsBgra = solidTargetPixels,
                BitmapSourceControlPixelsBgra = bitmapSourcePixels,
                BitmapSourceMatchesDrawingImage = bitmapSourcePixels is null ? (bool?)null
                    : bitmapSourcePixels.SequenceEqual(pixels.Select(value => (int)value))
            });
        }
        using Process process = Process.GetCurrentProcess();
        string[] modules = process.Modules.Cast<ProcessModule>()
            .Where(module => string.Equals(module.ModuleName, "wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase))
            .Select(module => module.FileName).ToArray();
        CollectionAssert.AreEqual(new[] { NativeMethods.LibraryPath }, modules);
        var evidence = new
        {
            SchemaVersion = 1,
            CapturedAtUtc = DateTimeOffset.UtcNow,
            Runtime = RuntimeInformation.FrameworkDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            PresentationCore = typeof(DrawingVisual).Assembly.Location,
            NativeDll = NativeMethods.LibraryPath,
            NativeVersion = FileVersionInfo.GetVersionInfo(NativeMethods.LibraryPath).FileVersion,
            NativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(NativeMethods.LibraryPath))),
            RenderMode = "SoftwareOnly",
            Limitation = "Pbgra32 output is quantized and is not a float/HDR oracle. Fixture correspondence must be reviewed before changing COM expectations.",
            BitmapControlGroups = bitmapControlGroups.Select(group => new { Key = group.Key, group.Value.Compared, group.Value.Differences }).ToArray(),
            Samples = samples
        };
        string resultsDirectory = typeof(ReferenceBehaviorTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "ReferenceResultsPath").Value
            ?? throw new InvalidOperationException("The reference results directory is not configured.");
        Directory.CreateDirectory(resultsDirectory);
        string path = Path.Combine(resultsDirectory, $"original-wpf-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.AddResultFile(path);
        TestContext.WriteLine(path);
        Assert.HasCount(450, samples);
    }
}

internal static unsafe class NativeMethods
{
    internal static string LibraryPath { get; } = Path.Combine(
        Path.GetDirectoryName(typeof(DrawingVisual).Assembly.Location)!, "wpfgfx_cor3.dll");

    static NativeMethods()
    {
        // Resolve only the installed WindowsDesktop DLL, never the translated artifact by name.
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, (name, assembly, searchPath) =>
            name == "OriginalWpfGfx" ? NativeLibrary.Load(LibraryPath) : IntPtr.Zero);
    }

    internal static void EnsureLoaded()
    {
        if (!File.Exists(LibraryPath))
        {
            throw new FileNotFoundException("The installed WindowsDesktop WPF native library was not found.", LibraryPath);
        }
        _ = GetMapping(new ImageBrush { ViewportUnits = BrushMappingMode.Absolute }, new Rect(0, 0, 4, 4), new Rect(0, 0, 2, 1));
    }

    internal static object GetMapping(ImageBrush brush, Rect bounds, Rect contentBounds)
    {
        var shape = new NativeRect(bounds);
        var content = new NativeRect(contentBounds);
        var viewport = new NativeRect(brush.Viewport);
        var viewbox = new NativeRect(brush.Viewbox);
        float* matrix = stackalloc float[16];
        new Span<float>(matrix, 16).Clear();
        int empty = 0;
        GetTileBrushMapping(null, null, (int)brush.Stretch, (int)brush.AlignmentX, (int)brush.AlignmentY,
            (int)brush.ViewportUnits, (int)brush.ViewboxUnits, &shape, &content, &viewport, &viewbox, matrix, &empty);
        return new
        {
            Empty = empty != 0,
            AbsoluteViewport = viewport.ToArray(),
            AbsoluteViewbox = viewbox.ToArray(),
            ContentToWorldD3DMatrix = new ReadOnlySpan<float>(matrix, 16).ToArray()
        };
    }

    [DllImport("OriginalWpfGfx", EntryPoint = "MilUtility_GetTileBrushMapping", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern void GetTileBrushMapping(float* transform, float* relativeTransform,
        int stretch, int alignmentX, int alignmentY, int viewportUnits, int viewboxUnits,
        NativeRect* shape, NativeRect* content, NativeRect* viewport, NativeRect* viewbox, float* matrix, int* empty);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect(Rect rectangle)
    {
        private double x = rectangle.X;
        private double y = rectangle.Y;
        private double width = rectangle.Width;
        private double height = rectangle.Height;

        internal readonly double[] ToArray() => [x, y, width, height];
    }
}
