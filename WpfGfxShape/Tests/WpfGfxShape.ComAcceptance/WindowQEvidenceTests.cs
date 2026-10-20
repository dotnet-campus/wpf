using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class WindowQEvidenceTests
{
    private static bool IsExpectedFrame(string name, byte[] pixels)
    {
        string[] parts = Path.GetFileNameWithoutExtension(name).Split('-');
        if (parts.Length != 4 || parts[0] != "frame" || !int.TryParse(parts[2], out int frame) || frame < 0 || frame > 3)
            return false;
        string[] size = parts[3].Split('x');
        if (size.Length != 2 || !int.TryParse(size[0], out int width) || !int.TryParse(size[1], out int height))
            return false;
        int expectedWidth = frame < 2 ? 24 : 40;
        int expectedHeight = frame < 2 ? 16 : 24;
        if (width != expectedWidth || height != expectedHeight || pixels.Length != expectedWidth * expectedHeight * 4)
            return false;
        byte[][] colors = [[0, 0, 255, 255], [255, 0, 0, 255], [0, 255, 0, 255], [0, 255, 255, 255]];
        return pixels.Chunk(4).All(pixel => pixel.AsSpan().SequenceEqual(colors[frame]));
    }

    /// <summary>Checks all original and resized frame colors independently of the official byte comparison.</summary>
    [TestMethod]
    [DataRow(0, 24, 16, 0, 0, 255)]
    [DataRow(1, 24, 16, 255, 0, 0)]
    [DataRow(2, 40, 24, 0, 255, 0)]
    [DataRow(3, 40, 24, 0, 255, 255)]
    public void WhenQFrameHasExpectedSizeAndColorThenAuditAccepts(int frame, int width, int height, int blue, int green, int red)
    {
        byte[] pixels = Enumerable.Range(0, width * height).SelectMany(_ => new byte[] { (byte)blue, (byte)green, (byte)red, 255 }).ToArray();
        Assert.IsTrue(IsExpectedFrame($"frame-0-{frame}-{width}x{height}.pbgra32", pixels));
    }

    /// <summary>Rejects incomplete buffers, stale colors and incorrect resize dimensions.</summary>
    [TestMethod]
    [DataRow("frame-0-2-40x24.pbgra32", 3840, 0, 0, 255)]
    [DataRow("frame-0-2-40x24.pbgra32", 1536, 0, 255, 0)]
    [DataRow("frame-0-2-24x16.pbgra32", 1536, 0, 255, 0)]
    [DataRow("frame-0-2-40x24.pbgra32", 0, 0, 255, 0)]
    [DataRow("frame-0-2-40x24.pbgra32", 3839, 0, 255, 0)]
    public void WhenQFrameIsStaleOrIncompleteThenAuditRejects(string name, int length, int blue, int green, int red)
    {
        byte[] color = [(byte)blue, (byte)green, (byte)red, 255];
        byte[] pixels = Enumerable.Range(0, length).Select(index => color[index % 4]).ToArray();
        Assert.IsFalse(IsExpectedFrame(name, pixels));
    }

    /// <summary>Audits the preserved Q frames independently without treating partial output as sequence completion.</summary>
    [TestMethod]
    public void WhenPreservedQFramesAreAuditedThenEveryByteMatchesOfficial()
    {
        string root = Path.Combine(Environment.CurrentDirectory, "readback");
        if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Results", "replacement-rectangle-window"))) root = Environment.CurrentDirectory;
        if (!Directory.Exists(root))
        {
            DirectoryInfo? current = new(AppContext.BaseDirectory);
            while (current is not null && !Directory.Exists(Path.Combine(current.FullName, "artifacts", "replacement-window-q-2026", "readback")))
                current = current.Parent;
            Assert.IsNotNull(current);
            root = Path.Combine(current.FullName, "artifacts", "replacement-window-q-2026", "readback");
        }
        var evidence = new List<object>();
        foreach (string architecture in Directory.Exists(Path.Combine(root, "Results")) ? new[] { "." } : new[] { "final-x64", "final-x86", "rectangle-x64" })
        {
            string directory = Path.Combine(root, architecture, "Results", "replacement-rectangle-window");
            foreach (string run in Directory.GetDirectories(directory))
            {
                foreach (string file in Directory.GetFiles(Path.Combine(run, "candidate"), "*.pbgra32").Order())
                {
                    byte[] candidate = File.ReadAllBytes(file);
                    byte[] official = File.ReadAllBytes(Path.Combine(run, "original", Path.GetFileName(file)));
                    bool exact = candidate.AsSpan().SequenceEqual(official);
                    bool red = candidate.Chunk(4).All(pixel => pixel.AsSpan().SequenceEqual(new byte[] { 0, 0, 255, 255 }));
                    bool blue = candidate.Chunk(4).All(pixel => pixel.AsSpan().SequenceEqual(new byte[] { 255, 0, 0, 255 }));
                    evidence.Add(new { architecture, run = Path.GetFileName(run), file = Path.GetFileName(file), bytes = candidate.Length,
                        candidateSha256 = Convert.ToHexString(SHA256.HashData(candidate)), officialSha256 = Convert.ToHexString(SHA256.HashData(official)), exact, red, blue });
                    Assert.IsTrue(exact, file);
                    Assert.IsTrue(IsExpectedFrame(Path.GetFileName(file), candidate), file);
                }
            }
        }
        Assert.IsTrue(evidence.Count > 0);
        Directory.CreateDirectory("audit-t");
        File.WriteAllText(Path.Combine("audit-t", "q-raw-frames.json"), JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
        var identities = Directory.GetFiles(root, "wpfgfx_cor3.dll", SearchOption.AllDirectories)
            .Select(path => new { path = Path.GetRelativePath(root, path), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) });
        File.WriteAllText(Path.Combine("audit-t", "q-dependencies.json"), JsonSerializer.Serialize(identities, new JsonSerializerOptions { WriteIndented = true }));
    }
}
