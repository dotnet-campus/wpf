using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.WpfReference;

[TestClass]
public sealed class GlyphReferenceTests
{
    [STATestMethod]
    public void WhenGlyphRunDrawingUsesArialThenIndependentSurfaceContractIsRecorded()
    {
        NativeMethods.EnsureLoaded();
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
        var face = new GlyphTypeface(new Uri(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf")));
        ushort index = face.CharacterToGlyphMap['A'];
        double advance = face.AdvanceWidths[index] * 16;
        var run = new GlyphRun(face, 0, false, 16, new ushort[] { index }, new Point(2, 20),
            new double[] { advance }, null, null, null, null, null, null);
        var visual = new DrawingVisual();
        TextOptions.SetTextRenderingMode(visual, TextRenderingMode.Grayscale);
        TextOptions.SetTextHintingMode(visual, TextHintingMode.Fixed);
        using (var context = visual.RenderOpen()) context.DrawDrawing(new GlyphRunDrawing(Brushes.Red, run));
        var bitmap = new RenderTargetBitmap(32, 24, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        byte[] pixels = new byte[32 * 24 * 4];
        bitmap.CopyPixels(pixels, 128, 0);
        Rect bounds = run.ComputeInkBoundingBox();
        bounds.Offset(run.BaselineOrigin.X, run.BaselineOrigin.Y);
        string directory = typeof(GlyphReferenceTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(a => a.Key == "ReferenceResultsPath").Value ?? throw new InvalidOperationException();
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "glyph-arial-original.json"), JsonSerializer.Serialize(new
        {
            Font = face.FontUri.LocalPath,
            FontSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(face.FontUri.LocalPath))),
            Family = face.FamilyNames.Values.OrderBy(value => value).ToArray(),
            Style = face.Style.ToString(), Stretch = face.Stretch.ToOpenTypeStretch(), Weight = face.Weight.ToOpenTypeWeight(),
            FaceIndex = 0, GlyphIndices = new[] { index }, Advances = new[] { advance },
            Offsets = new[] { new[] { 0d, 0d } }, PixelsPerDip = 1, MeasuringMethod = "Natural",
            DpiX = 96, DpiY = 96,
            OriginalLibrary = NativeMethods.LibraryPath,
            OriginalLibrarySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(NativeMethods.LibraryPath))),
            Index = index, Advance = advance, EmSize = 16, OriginX = 2, OriginY = 20,
            Bounds = new[] { bounds.X, bounds.Y, bounds.Width, bounds.Height },
            Width = 32, Height = 24, Format = "Pbgra32", Rendering = "Grayscale", Hinting = "Fixed",
            Transform = new[] { 1, 0, 0, 1, 0, 0 },
            PixelsBgra = pixels.Select(value => (int)value).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.IsTrue(pixels.Any(value => value != 0));
    }
}
