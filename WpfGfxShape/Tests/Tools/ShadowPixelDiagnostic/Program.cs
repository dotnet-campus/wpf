using System.Text.Json;
var differences = new List<object>();
foreach (string path in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Baseline"), "shadow-*.pbgra32"))
{
    byte[] original = File.ReadAllBytes(path);
    byte[] candidate = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "ComparisonCandidate", Path.GetFileName(path)));
    var pixels = new List<object>();
    for (int i = 0; i < original.Length; i += 4)
        if (!original.AsSpan(i, 4).SequenceEqual(candidate.AsSpan(i, 4)))
            pixels.Add(new { x = i / 4 % 32, y = i / 128, original = original.AsSpan(i, 4).ToArray(), candidate = candidate.AsSpan(i, 4).ToArray() });
    differences.Add(new { name = Path.GetFileNameWithoutExtension(path), pixels });
}
File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "pixel-differences.json"), JsonSerializer.Serialize(differences, new JsonSerializerOptions { WriteIndented = true }));