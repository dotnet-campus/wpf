using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace WpfGfxShape.WpfReference;

[TestClass]
public sealed class SoftwareShaderReferenceTests
{
    [STATestMethod]
    [DataRow(0.25, 0xff400000u, false)]
    [DataRow(0.5, 0xff800000u, false)]
    [DataRow(0.5, 0x00800000u, true)]
    public void WhenOriginalSoftwareOnlyShaderReadsConstantThenArgb32UsesNativePacking(double red, uint expected, bool killNegativeW)
    {
        uint[] words = killNegativeW
            ? [0xffff0200, 0x02000001, 0x800f0000, 0xa0e40000, 0x01000041, 0x800f0000, 0x02000001, 0x800f0800, 0xa0e40000, 0xffff]
            : [0xffff0200, 0x02000001, 0x800f0800, 0xa0e40000, 0xffff];
        using var stream = new MemoryStream(System.Runtime.InteropServices.MemoryMarshal.AsBytes(words.AsSpan()).ToArray());
        var shader = new PixelShader { ShaderRenderMode = ShaderRenderMode.SoftwareOnly };
        shader.SetStreamSource(stream);
        var effect = new ConstantEffect(shader) { Constant = new Point4D(red, 0, 0, killNegativeW ? -1 : 1) };
        var visual = new DrawingVisual { Effect = effect };
        using (var drawing = visual.RenderOpen()) drawing.DrawRectangle(Brushes.White, null, new Rect(0, 0, 5, 1));
        var bitmap = new RenderTargetBitmap(5, 1, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        uint[] pixels = new uint[5];
        bitmap.CopyPixels(pixels, 20, 0);
        CollectionAssert.AreEqual(Enumerable.Repeat(expected, 5).ToArray(), pixels);
    }

    private sealed class ConstantEffect : ShaderEffect
    {
        internal static readonly DependencyProperty ConstantProperty = DependencyProperty.Register(
            nameof(Constant), typeof(Point4D), typeof(ConstantEffect), new UIPropertyMetadata(new Point4D(0, 0, 0, 1), PixelShaderConstantCallback(0)));
        internal Point4D Constant { get => (Point4D)GetValue(ConstantProperty); set => SetValue(ConstantProperty, value); }
        internal ConstantEffect(PixelShader shader) { PixelShader = shader; UpdateShaderValue(ConstantProperty); }
    }
}
