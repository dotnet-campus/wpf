namespace NativeAcceptance.Pcore;

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

internal static partial class Program
{
    private static unsafe void RenderShaderBounds()
    {
        // Initialize WPF before toggling the native switch; MediaSystem initializes it once.
        new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Pbgra32).Render(new DrawingVisual());
        var set = (delegate* unmanaged[Stdcall]<int, void>)NativeLibrary.GetExport(module, "WpfGfx_SetDisableBoundsCheckProtection");
        var observations = new List<object>();
        try
        {
            observations.Add(new { mode = "default", pixel = RenderBoundsShader([0xffff0200, 0x02000001, 0x800f0800, 0xa0e40000, 0xffff]) });
            foreach (int disabled in new[] { 0, 1, -1, 2, 0 })
            {
                set(disabled);
                foreach (int size in new[] { 1024 * 1024 - 4, 1024 * 1024, 1024 * 1024 + 4 })
                {
                    uint[] words = new uint[size / 4];
                    words[0] = 0xffff0200;
                    // NOP padding changes resource bytes without adding translated instructions.
                    words[^4] = 0x02000001;
                    words[^3] = 0x800f0800;
                    words[^2] = 0xa0e40000;
                    words[^1] = 0xffff;
                    uint pixel = RenderBoundsShader(words);
                    observations.Add(new { disabled, size, pixel });
                }
                uint invalid = RenderBoundsShader([0xffff0100, 0xffff]);
                observations.Add(new { disabled, size = 8, pixel = invalid, invalidVersion = true });
                observations.Add(new { disabled, kind = "end-trailing", pixel = RenderBoundsShader([0xffff0200, 0x02000001, 0x800f0800, 0xa0e40000, 0xffff, 0]) });
                observations.Add(new { disabled, kind = "truncated-operand", pixel = RenderBoundsShader([0xffff0200, 0x02000001, 0x800f0800, 0xffff]) });
            }
        }
        finally { set(0); }
        Evidence["shaderBounds"] = observations;
    }

    private static uint RenderBoundsShader(uint[] words)
    {
        using var stream = new MemoryStream(MemoryMarshal.AsBytes(words.AsSpan()).ToArray());
        var shader = new PixelShader { ShaderRenderMode = ShaderRenderMode.SoftwareOnly };
        shader.SetStreamSource(stream);
        var visual = new DrawingVisual { Effect = new BoundsEffect(shader) };
        using (var drawing = visual.RenderOpen()) drawing.DrawRectangle(Brushes.White, null, new Rect(0, 0, 4, 4));
        var bitmap = new RenderTargetBitmap(4, 4, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        uint[] pixels = new uint[16];
        bitmap.CopyPixels(pixels, 16, 0);
        return pixels[0];
    }

    private sealed class BoundsEffect : ShaderEffect
    {
        private static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(BoundsEffect), 0);
        private static readonly DependencyProperty ConstantProperty = DependencyProperty.Register(
            "Constant", typeof(System.Windows.Media.Media3D.Point4D), typeof(BoundsEffect),
            new UIPropertyMetadata(new System.Windows.Media.Media3D.Point4D(1, 0, 0, 1), PixelShaderConstantCallback(0)));

        internal BoundsEffect(PixelShader shader)
        {
            PixelShader = shader;
            UpdateShaderValue(InputProperty);
            UpdateShaderValue(ConstantProperty);
        }
    }
}
