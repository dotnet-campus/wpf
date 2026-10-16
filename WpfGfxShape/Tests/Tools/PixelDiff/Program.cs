using System.Text.Json;

var root = AppContext.BaseDirectory;
var original = File.ReadAllBytes(Path.Combine(root, "original.pbgra32"));
var candidate = File.ReadAllBytes(Path.Combine(root, "candidate.pbgra32"));
var differences = new List<object>();
for (int i = 0; i < original.Length; i += 4)
{
    if (!original.AsSpan(i, 4).SequenceEqual(candidate.AsSpan(i, 4)))
        differences.Add(new { x = i / 4 % 32, y = i / 4 / 32, original = original[i..(i + 4)], candidate = candidate[i..(i + 4)] });
}
Directory.CreateDirectory(Path.Combine(root, "Results"));
File.WriteAllText(Path.Combine(root, "Results", "pixels.json"), JsonSerializer.Serialize(differences, new JsonSerializerOptions { WriteIndented = true }));
return differences.Count == 0 ? 0 : 1;
