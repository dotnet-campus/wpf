using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        string role = args.Single();
        string root = AppContext.BaseDirectory;
        string native = role == "original" ? Path.Combine(Path.GetDirectoryName(typeof(DrawingVisual).Assembly.Location)!, "wpfgfx_cor3.dll") : Path.Combine(root, "Candidate", "wpfgfx_cor3.dll");
        nint module = NativeLibrary.Load(native);
        foreach (var assembly in new[] { typeof(DrawingVisual).Assembly, typeof(Point).Assembly })
            NativeLibrary.SetDllImportResolver(assembly, (name, _, _) => name.StartsWith("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase) ? module : 0);
        string directory = Path.Combine(root, "Results", role);
        Directory.CreateDirectory(directory);
        var results = new List<object>();
        foreach (bool aliased in new[] { false, true })
        foreach (string mode in new[] { "fill", "stroke", "both" })
        foreach (PenLineJoin join in Enum.GetValues<PenLineJoin>())
        {
            string id = $"{mode}-{join}-{aliased}";
            var visual = new DrawingVisual();
            if (aliased) RenderOptions.SetEdgeMode(visual, EdgeMode.Aliased);
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, 32, 32));
                dc.DrawGeometry(mode == "stroke" ? null : Brushes.Green,
                    mode == "fill" ? null : new Pen(Brushes.Black, 1) { LineJoin = join },
                    Geometry.Parse("M 4,24 L 13,20 21,29 Z"));
            }
            var bitmap = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            byte[] pixels = new byte[4096];
            bitmap.CopyPixels(pixels, 128, 0);
            File.WriteAllBytes(Path.Combine(directory, id + ".bin"), pixels);
            string original = Path.Combine(root, "Baseline", id + ".bin");
            var differences = new List<object>();
            if (role == "candidate")
            {
                byte[] baseline = File.ReadAllBytes(original);
                for (int i = 0; i < pixels.Length; i += 4)
                    if (!pixels.AsSpan(i, 4).SequenceEqual(baseline.AsSpan(i, 4)))
                        differences.Add(new { x = i / 4 % 32, y = i / 128, original = baseline[i..(i + 4)], candidate = pixels[i..(i + 4)] });
            }
            results.Add(new { id, count = differences.Count, differences, sha256 = Convert.ToHexString(SHA256.HashData(pixels)) });
        }
        File.WriteAllText(Path.Combine(directory, "controls.json"), JsonSerializer.Serialize(new { role, native, nativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(native))), results }, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
}
