using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class LifecycleSequenceTests
{
    /// <summary>Compares fresh official and candidate WPF frames throughout seeded resize and cleanup cycles.</summary>
    [TestMethod]
    public async Task WhenSeededLifecycleRunsThenNewFramesAndClosedObjectsMatch()
    {
        string root = Path.Combine(AcceptancePaths.Results, "lifecycle-sequence", Guid.NewGuid().ToString("N"));
        Assert.AreEqual(0, await NativeRun.RoleAsync(root, "--pcore", "original", "lifecycle-sequence"), root);
        Assert.AreEqual(0, await NativeRun.RoleAsync(root, "--pcore", "candidate", "lifecycle-sequence"), root);
        string original = Path.Combine(root, "Results", "original");
        string candidate = Path.Combine(root, "Results", "candidate");
        string[] files = Directory.GetFiles(original, "sequence-*.pbgra32");
        Assert.HasCount(80, files);
        foreach (string path in files)
        {
            string counterpart = Path.Combine(candidate, Path.GetFileName(path));
            Assert.IsTrue(File.ReadAllBytes(path).AsSpan().SequenceEqual(File.ReadAllBytes(counterpart)), counterpart);
        }
        for (int cycle = 0; cycle < 8; cycle++)
        {
            string name = $"sequence-events-{cycle:D2}.json";
            string[] events = JsonSerializer.Deserialize<string[]>(File.ReadAllText(Path.Combine(candidate, name)))!;
            Assert.IsTrue(IsComplete(events), name);
            CollectionAssert.AreEqual(JsonSerializer.Deserialize<string[]>(File.ReadAllText(Path.Combine(original, name)))!, events);
        }
    }

    /// <summary>Checks that missing completion and stale output cannot satisfy the sequence oracle.</summary>
    [TestMethod]
    public void WhenCloseOrNewFrameIsMissingThenSequenceOracleRejectsIt()
    {
        string[] valid = ["create", "update:0", "frame:0", "close"];
        Assert.IsTrue(IsComplete(valid));
        Assert.IsFalse(IsComplete(valid[..^1]));
        Assert.IsFalse(IsComplete(["create", "frame:0", "update:0", "close"]));
        Assert.IsFalse(IsFresh([0, 0, 255, 255], [0, 0, 255, 255]));
        Assert.IsTrue(IsFresh([0, 0, 255, 255], [255, 0, 0, 255]));
    }

    private static bool IsFresh(byte[] previous, byte[] current) => !previous.AsSpan().SequenceEqual(current);

    private static bool IsComplete(string[] events)
    {
        if (events.Length < 4 || events[0] != "create" || events[^1] != "close") return false;
        int updates = 0, frames = 0;
        foreach (string operation in events)
        {
            if (operation.StartsWith("update:", StringComparison.Ordinal))
            {
                if (updates != frames || operation != "update:" + updates) return false;
                updates++;
            }
            if (operation.StartsWith("frame:", StringComparison.Ordinal))
            {
                if (updates != frames + 1 || operation != "frame:" + frames) return false;
                frames++;
            }
        }
        return frames > 0 && frames == updates;
    }
}
