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
    private static readonly string[] DropShadowCases = ["shadow-zero", "shadow-radius2", "shadow-fractional", "shadow-q1", "shadow-q2", "shadow-q3", "shadow-q4", "shadow-depth0", "shadow-opacity0", "shadow-opacity-half", "shadow-color", "shadow-quality", "shadow-transform-clip", "shadow-change"];

    private static void RenderDropShadow()
    {
        Evidence["caseId"] = "E2E-05-drop-shadow-v1";
        var results = new List<object>();
        foreach (string name in DropShadowCases)
        {
            var root = new DrawingVisual();
            var child = new DrawingVisual();
            var effect = new DropShadowEffect
            {
                BlurRadius = name == "shadow-zero" ? 0 : name == "shadow-fractional" ? 2.75 : 2,
                ShadowDepth = name == "shadow-depth0" ? 0 : 3,
                Direction = name switch { "shadow-q2" => 135, "shadow-q3" => 225, "shadow-q4" => 315, _ => 45 },
                Opacity = name == "shadow-opacity0" ? 0 : name == "shadow-opacity-half" ? 0.5 : 1,
                Color = name == "shadow-color" ? Color.FromArgb(128, 32, 180, 90) : Colors.Black,
                RenderingBias = name == "shadow-quality" ? RenderingBias.Quality : RenderingBias.Performance
            };
            try
            {
                child.Effect = effect;
                root.Children.Add(child);
                RenderOptions.SetBitmapScalingMode(root, BitmapScalingMode.NearestNeighbor);
                if (name == "shadow-transform-clip")
                {
                    child.Transform = new MatrixTransform(1.25, 0, 0, 0.75, 2, 3);
                    child.Clip = new RectangleGeometry(new Rect(3, 3, 18, 19));
                    child.Opacity = 0.5;
                }
                byte[]? previous = null;
                for (int frame = 0; frame < 2; frame++)
                {
                    if (name == "shadow-change")
                    {
                        effect.BlurRadius = frame == 0 ? 2 : 5;
                        effect.ShadowDepth = frame == 0 ? 3 : 4;
                        effect.Direction = frame == 0 ? 45 : 225;
                        effect.Color = frame == 0 ? Colors.Black : Colors.Green;
                        effect.Opacity = frame == 0 ? 1 : 0.5;
                    }
                    using (DrawingContext dc = child.RenderOpen())
                        dc.DrawRectangle(frame == 0 ? Brushes.Red : Brushes.Blue, null, new Rect(9, 9, 12, 12));
                    Checkpoint(name + "-frame-" + frame);
                    var target = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
                    target.Render(root);
                    byte[] pixels = new byte[4096];
                    target.CopyPixels(pixels, 128, 0);
                    int coloredPixels = Enumerable.Range(0, 1024).Count(i => pixels[i * 4 + 3] != 0 && (pixels[i * 4] != pixels[i * 4 + 1] || pixels[i * 4 + 1] != pixels[i * 4 + 2]));
                    if (coloredPixels == 0 || previous is not null && previous.AsSpan().SequenceEqual(pixels))
                        throw new InvalidOperationException("Shadow frames must be nonempty, colored and change after source replacement.");
                    string id = name + "-" + frame;
                    File.WriteAllBytes(Path.Combine(output, id + ".pbgra32"), pixels);
                    results.Add(new { name = id, status = "Green", bytes = pixels.Length, coloredPixels, sha256 = Convert.ToHexString(SHA256.HashData(pixels)) });
                    previous = pixels;
                    target.Clear();
                }
            }
            catch (Exception exception)
            {
                results.Add(new { name, status = "Red", hresult = $"0x{exception.HResult:X8}", exception = exception.ToString() });
            }
            finally
            {
                child.Effect = null;
                root.Children.Clear();
                using (child.RenderOpen()) { }
            }
        }
        File.WriteAllText(Path.Combine(output, "drop-shadow-scenes.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        Evidence["dropShadowScenes"] = results;
    }
}
