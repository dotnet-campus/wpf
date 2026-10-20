using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class X86ContentDifferenceReproTests
{
    public TestContext TestContext { get; set; } = null!;

    /// <summary>Reduces source18/affine3 mask to one fresh official Pbgra32 surface and the identical candidate scene.</summary>
    [STATestMethod]
    public void WhenAffineSource18MaskIsRenderedThenSameFormatMatchesFreshOfficial()
    {
        WpfReference.NativeMethods.EnsureLoaded();
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
        var image = new DrawingImage(new GeometryDrawing(Brushes.Red, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
        var brush = new ImageBrush(image)
        {
            Viewport = new Rect(0.25, 1, 2, 2), ViewportUnits = BrushMappingMode.Absolute,
            Viewbox = new Rect(0, 0, 2, 2), ViewboxUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top,
            TileMode = TileMode.None
        };
        RenderOptions.SetBitmapScalingMode(brush, BitmapScalingMode.LowQuality);
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);
        var visual = new DrawingVisual
        {
            OpacityMask = brush,
            Transform = new MatrixTransform(0.8, 0.6, -0.6, 0.8, 3, 0)
        };
        RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.LowQuality);
        var group = new DrawingGroup();
        RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.LowQuality);
        using (DrawingContext drawing = group.Open()) drawing.DrawRectangle(Brushes.Red, null, new Rect(-10, -10, 20, 20));
        using (DrawingContext drawing = visual.RenderOpen()) drawing.DrawDrawing(group);
        var target = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
        target.Render(visual);
        byte[] official = new byte[64];
        target.CopyPixels(official, 16, 0);
        byte[] candidate = [];
        Vector4[] floats = [];
        var fixture = new CompositionTests();
        fixture.WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
            curves: true, drawingImage: true, affineMode: 3, sourceMode: 18, tileMode: 0,
            sourceAlpha: 0.5f, floating: false, scalingMode: 1, consumerMode: 2,
            observeBgraSurface: pixels => candidate = pixels);
        fixture.WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
            curves: true, drawingImage: true, affineMode: 3, sourceMode: 18, tileMode: 0,
            sourceAlpha: 0.5f, floating: true, scalingMode: 1, consumerMode: 2,
            observeSurface: pixels => floats = pixels);
        string report = Path.Combine(AcceptancePaths.Results, $"source18-affine3-mask-{Guid.NewGuid():N}.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            Architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            PointerSize = IntPtr.Size,
            OriginalModule = AcceptancePaths.OriginalDll,
            OriginalSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.OriginalDll))),
            CandidateModule = AcceptancePaths.AotDll,
            CandidateSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.AotDll))),
            Source = 18, Affine = 3, Consumer = "mask", Tile = "None", Scaling = "LowQuality",
            Width = 4, Height = 4, Stride = 16,
            OfficialBgra = official.Select(value => (int)value).ToArray(),
            CandidateBgra = candidate.Select(value => (int)value).ToArray(),
            DiagnosticFloatRgba = floats.Select(pixel => new[] { pixel.X, pixel.Y, pixel.Z, pixel.W }).ToArray(),
            Scope = "Only identical Pbgra32 surfaces are compared; float output is raw diagnostic, not a cross-format golden"
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.AddResultFile(report);
        Assert.IsTrue(official.Any(value => value != 0));
        CollectionAssert.AreEqual(official, candidate);
    }
}
