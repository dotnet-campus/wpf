using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.WpfReference;

[TestClass]
public sealed class WorkingFormatContractIndependentTests
{
    public TestContext TestContext { get; set; } = null!;

    private void Record(string text)
    {
        TestContext.WriteLine(text);
        string directory = typeof(WorkingFormatContractIndependentTests).Assembly
            .GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>()
            .Single(value => value.Key == "ReferenceResultsPath").Value ?? throw new InvalidOperationException();
        System.IO.Directory.CreateDirectory(directory);
        System.IO.File.WriteAllText(System.IO.Path.Combine(directory,
            $"contract-independent-{Guid.NewGuid():N}.txt"), text);
    }

    [STATestMethod]
    public void WhenSamePublicGeometryRequestsDifferentFormatsThenActualFormatIsRecorded()
    {
        NativeMethods.EnsureLoaded();
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
        foreach (int source in new[] { 17, 18 })
        foreach (PixelFormat format in new[] { PixelFormats.Pbgra32, PixelFormats.Prgba128Float })
        {
            var image = new DrawingImage(new GeometryDrawing(Brushes.Red, null,
                new RectangleGeometry(new Rect(0, 0, 1, 1))));
            var brush = new ImageBrush(image)
            {
                Viewport = new Rect(0.25, 1, 2, 2), ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(source == 17 ? -0.5 : 0, 0, 2, 2),
                ViewboxUnits = BrushMappingMode.Absolute, Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top, TileMode = TileMode.FlipX
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.HighQuality);
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            using (DrawingContext dc = group.Open())
                dc.DrawRectangle(brush, null, source == 18 ? new Rect(-10, -10, 20, 20) : new Rect(0, 0, 4, 4));
            var visual = new DrawingVisual { Transform = new MatrixTransform(0.8, 0.6, -0.6, 0.8, 3, 0) };
            RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.HighQuality);
            using (DrawingContext dc = visual.RenderOpen()) dc.DrawDrawing(group);
            RenderTargetBitmap target;
            try { target = new RenderTargetBitmap(4, 4, 96, 96, format); }
            catch (ArgumentException error)
            {
                Record($"source={source}; requested={format}; constructor={error.GetType().FullName}; message={error.Message}; no FLOAT rendering oracle obtained");
                continue;
            }
            target.Render(visual);
            byte[] pixels = new byte[4 * 4 * target.Format.BitsPerPixel / 8];
            target.CopyPixels(pixels, 4 * target.Format.BitsPerPixel / 8, 0);
            Record($"source={source}; requested={format}; actual={target.Format}; full={Convert.ToHexString(pixels)}");
            Assert.AreEqual(format, target.Format);
        }
    }

    [STATestMethod]
    public void WhenSystemWicConvertsPremultipliedFloatThenActualQuantizationIsRecorded()
    {
        NativeMethods.EnsureLoaded();
        float[] values = [0.38f, 0, 0, 0.38f, 0.13f, 0, 0, 0.13f,
            0.00125f, 0, 0, 0.001f, 0.05f, 0, 0, 0.5f,
            0.13137255f, 0, 0, 0.13137255f];
        BitmapSource source = BitmapSource.Create(5, 1, 96, 96, PixelFormats.Prgba128Float, null, values, 80);
        var converted = new FormatConvertedBitmap(source, PixelFormats.Pbgra32, null, 0);
        byte[] pixels = new byte[20];
        converted.CopyPixels(pixels, 20, 0);
        Record($"System WIC Prgba128Float -> Pbgra32: {Convert.ToHexString(pixels)}; values={string.Join(",", values)}");
        Assert.AreEqual((byte)33, pixels[7]);
    }
}
