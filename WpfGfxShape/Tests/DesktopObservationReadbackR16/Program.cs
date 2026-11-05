using System.Security.Cryptography;
using System.Text.Json;

string root = AppContext.BaseDirectory;
var images = new List<object>();
foreach (string file in Directory.GetFiles(Path.Combine(root, "Input"), "*.bmp").Order())
{
    byte[] data = File.ReadAllBytes(file);
    int offset = BitConverter.ToInt32(data, 10), width = BitConverter.ToInt32(data, 18), height = -BitConverter.ToInt32(data, 22);
    if (BitConverter.ToUInt16(data, 0) != 0x4D42 || width != 32 || height != 16 || BitConverter.ToUInt16(data, 28) != 32 || data.Length != offset + width * height * 4) return 2;
    int white = 0, target = 0;
    var colors = new Dictionary<string, int>();
    for (int i = offset; i < data.Length; i += 4)
    {
        uint rgb = BitConverter.ToUInt32(data, i) & 0xFFFFFF;
        if (rgb == 0xFFFFFF) white++;
        if (rgb == 0x123456) target++;
        string key = rgb.ToString("X6");
        colors[key] = colors.GetValueOrDefault(key) + 1;
    }
    images.Add(new { file = Path.GetFileName(file), sha256 = Convert.ToHexString(SHA256.HashData(data)), width, height, white, target, other = width * height - white - target, sampleRgb = (BitConverter.ToUInt32(data, offset + (8 * width + 16) * 4) & 0xFFFFFF).ToString("X6"), colors });
}
Directory.CreateDirectory(Path.Combine(root, "Evidence"));
File.WriteAllText(Path.Combine(root, "Evidence", "bmp-readback.json"), JsonSerializer.Serialize(images, new JsonSerializerOptions { WriteIndented = true }));
return images.Count == 3 ? 0 : 3;
