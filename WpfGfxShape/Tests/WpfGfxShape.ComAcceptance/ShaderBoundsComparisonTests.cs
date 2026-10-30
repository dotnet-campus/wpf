using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class ShaderBoundsComparisonTests
{
    /// <summary>Retains an independent default regression for the boundary and subsequent shader usability.</summary>
    [TestMethod]
    public async Task WhenProtectionTransitionsThenBoundaryConsumersMatchIndependentOfficial()
    {
        await WhenShaderResourceCrossesOneMiBThenOriginalAndCandidateAgree();
    }

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
        var expected = original.RootElement.GetProperty("shaderBoundaryConsumers");
        var actual = candidate.RootElement.GetProperty("shaderBoundaryConsumers");
        Assert.AreEqual(31, actual.GetArrayLength(), root);
        for (int i = 0; i < expected.GetArrayLength(); i++)
        {
            Assert.AreEqual(expected[i].GetProperty("sha256").GetString(), actual[i].GetProperty("sha256").GetString(), root);
            Assert.AreEqual(expected[i].GetProperty("pixels").GetRawText(), actual[i].GetProperty("pixels").GetRawText(), root);
            Assert.AreEqual(expected[i].GetProperty("invalidEvents").GetInt32(), actual[i].GetProperty("invalidEvents").GetInt32(), root);
        }
    }
}
