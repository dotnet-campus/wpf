using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class ShaderBoundsComparisonTests
{
    /// <summary>Compares safe bytecode resource boundaries against the same-runtime original with in-process BOOL transitions.</summary>
    [TestMethod]
    public async Task WhenShaderResourceCrossesOneMiBThenOriginalAndCandidateAgree()
    {
        string root = Path.Combine(AcceptancePaths.Results, "shaderbounds", Guid.NewGuid().ToString("N"));
        Assert.AreEqual(0, await NativeRun.RoleAsync(root, "--pcore", "original", "shaderbounds"), root);
        Assert.AreEqual(0, await NativeRun.RoleAsync(root, "--pcore", "candidate", "shaderbounds"), root);
        using var original = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "Results", "original", "result.json")));
        using var candidate = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "Results", "candidate", "result.json")));
        Assert.AreEqual(original.RootElement.GetProperty("shaderBounds").GetRawText(), candidate.RootElement.GetProperty("shaderBounds").GetRawText(), root);
    }
}
