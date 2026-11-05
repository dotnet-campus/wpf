using System.Security.Cryptography;
using System.Text.Json;

string root = AppContext.BaseDirectory;
var images = new List<object>();
foreach (string file in Directory.GetFiles(Path.Combine(root, "Input"), "*.bmp").Order())
{
    byte[] data = File.ReadAllBytes(file);
    if (data.Length < 54) return 2;
    int offset = BitConverter.ToInt32(data, 10), width = BitConverter.ToInt32(data, 18), signedHeight = BitConverter.ToInt32(data, 22);
    if (BitConverter.ToUInt16(data, 0) != 0x4D42 || offset != 54 || width != 32 || signedHeight != -16 || BitConverter.ToUInt16(data, 26) != 1 || BitConverter.ToUInt16(data, 28) != 32 || BitConverter.ToInt32(data, 30) != 0 || data.Length != offset + 32 * 16 * 4) return 2;
    var colors = new Dictionary<string, int>();
    var alpha = new Dictionary<byte, int>();
    for (int i = offset; i < data.Length; i += 4)
    {
        string rgb = (BitConverter.ToUInt32(data, i) & 0xFFFFFF).ToString("X6");
        colors[rgb] = colors.GetValueOrDefault(rgb) + 1;
        alpha[data[i + 3]] = alpha.GetValueOrDefault(data[i + 3]) + 1;
    }
    images.Add(new { file = Path.GetFileName(file), length = data.Length, sha256 = Convert.ToHexString(SHA256.HashData(data)), width, signedHeight, sample = new[] { 16, 8 }, sampleRgb = (BitConverter.ToUInt32(data, offset + (8 * width + 16) * 4) & 0xFFFFFF).ToString("X6"), black = colors.GetValueOrDefault("000000"), white = colors.GetValueOrDefault("FFFFFF"), d3dClearRgb123456 = colors.GetValueOrDefault("123456"), originalGdiExpectedColorRef00563412 = "RGB123456", colors, alpha });
}
string rawPath = Path.Combine(root, "Input", "wgc-r18-1.bgra");
byte[] raw = File.ReadAllBytes(rawPath);
byte[] baselineBmp = File.ReadAllBytes(Path.Combine(root, "Input", "wgc-r18-1.bmp"));
if (raw.Length != 2048 || !raw.AsSpan().SequenceEqual(baselineBmp.AsSpan(54))) return 4;
Directory.CreateDirectory(Path.Combine(root, "Evidence"));
File.WriteAllText(Path.Combine(root, "Evidence", "raw-byte-audit.json"), JsonSerializer.Serialize(new { length = raw.Length, sha256 = Convert.ToHexString(SHA256.HashData(raw)), zeroBytes = raw.Count(value => value == 0), matchesBmpPayload = true }));
File.WriteAllText(Path.Combine(root, "Evidence", "bmp-byte-audit.json"), JsonSerializer.Serialize(new { readOnly = true, nativeLoaded = false, count = images.Count, images }, new JsonSerializerOptions { WriteIndented = true }));
return images.Count == 4 ? 0 : 3;
