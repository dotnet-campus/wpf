using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace WpfGfxShape.WpfReference;

[TestClass]
public sealed class SoftwareOpcodeReferenceTests
{
    [STATestMethod]
    [DataRow(1,64)] [DataRow(2,191)] [DataRow(3,0)] [DataRow(4,223)] [DataRow(5,32)]
    [DataRow(6,255)] [DataRow(7,255)] [DataRow(8,96)] [DataRow(9,128)] [DataRow(10,64)]
    [DataRow(11,128)] [DataRow(14,255)] [DataRow(15,0)] [DataRow(18,175)] [DataRow(19,64)]
    [DataRow(20,128)] [DataRow(21,128)] [DataRow(22,96)] [DataRow(23,96)] [DataRow(24,96)]
    [DataRow(32,127)] [DataRow(33,0)] [DataRow(35,64)] [DataRow(36,147)] [DataRow(37,191)]
    [DataRow(88,128)] [DataRow(90,255)]
    public void WhenOriginalSoftwareOnlyExecutesOpcodeThenIndependentPixelMatches(int opcode,int red)
    {
        uint a = opcode == 37 ? 0xa0e40006u : 0xa0e40000u;
        int count = opcode switch { 1 or 6 or 7 or 14 or 15 or 19 or 35 or 36 => 1, 4 or 18 or 37 or 88 or 90 => 3, _ => 2 };
        List<uint> words = [0xffff0200];
        for (uint register = 0; register <= 7; register++)
        {
            float value = register switch { 0 => .25f, 2 => .75f, 6 => 0, 7 => 1, _ => .5f };
            words.AddRange([0x05000051,0xa00f0000|register,BitConverter.SingleToUInt32Bits(value),
                BitConverter.SingleToUInt32Bits(value),BitConverter.SingleToUInt32Bits(value),BitConverter.SingleToUInt32Bits(value)]);
        }
        words.Add((uint)((count+1)<<24)|(uint)opcode);
        words.Add(0x80010000); words.Add(a);
        if (count>1) words.Add(0xa0e40001);
        if (count>2) words.Add(0xa0e40002);
        words.AddRange([0x02000001,0x80010800,0x80000000,0x02000001,0x80080800,0xa0ff0007,0xffff]);
        using var stream = new MemoryStream(MemoryMarshal.AsBytes(words.ToArray().AsSpan()).ToArray());
        var shader = new PixelShader { ShaderRenderMode = ShaderRenderMode.SoftwareOnly };
        shader.SetStreamSource(stream);
        var visual = new DrawingVisual { Effect = new OpcodeEffect(shader) };
        using (var drawing = visual.RenderOpen()) drawing.DrawRectangle(Brushes.White,null,new Rect(0,0,5,1));
        var bitmap = new RenderTargetBitmap(5,1,96,96,PixelFormats.Pbgra32);
        bitmap.Render(visual);
        uint[] pixels = new uint[5];
        bitmap.CopyPixels(pixels,20,0);
        CollectionAssert.AreEqual(Enumerable.Repeat(0xff000000u|((uint)red<<16),5).ToArray(),pixels);
    }
    private sealed class OpcodeEffect : ShaderEffect
    {
        internal OpcodeEffect(PixelShader shader) { PixelShader = shader; }
    }
}
