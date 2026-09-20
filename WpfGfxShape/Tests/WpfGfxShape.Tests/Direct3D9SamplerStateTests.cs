using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9SamplerStateTests
{
    [TestMethod]
    public void WhenInitializingMaximumAnisotropyThenEveryBlendStageUsesRequestedLevel()
    {
        List<(uint Stage, Samplerstatetype State, uint Value)> calls = [];

        int result = Direct3D9SamplerState.InitializeMaximumAnisotropy(
            4,
            3,
            (stage, state, value) =>
            {
                calls.Add((stage, state, value));
                return 0;
            });

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(
            Enumerable.Range(0, 4)
                .Select(stage => ((uint) stage, Samplerstatetype.Maxanisotropy, 3u))
                .ToArray(),
            calls);
    }

    [TestMethod]
    public void WhenSettingMaximumAnisotropyReturnsNonzeroSuccessThenLastResultIsPreserved()
    {
        int result = Direct3D9SamplerState.InitializeMaximumAnisotropy(
            3,
            4,
            (stage, _, _) => checked((int) stage + 1));

        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void WhenSettingMaximumAnisotropyFailsThenFirstFailureIsReturnedAndLaterStagesAreSkipped()
    {
        List<uint> stages = [];

        int result = Direct3D9SamplerState.InitializeMaximumAnisotropy(
            6,
            4,
            (stage, _, _) =>
            {
                stages.Add(stage);
                return stage == 2 ? Direct3D9Factory.GenericFailureHResult : 0;
            });

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new uint[] { 0, 1, 2 }, stages);
    }

    [DataTestMethod]
    [DataRow(0u, 1u)]
    [DataRow(1u, 1u)]
    [DataRow(3u, 3u)]
    [DataRow(12u, 4u)]
    public void WhenInitializingDefaultStateThenDesiredAnisotropyMatchesNativeDeviceLimit(
        uint maximumAnisotropy,
        uint expectedLevel)
    {
        uint actualLevel = 0;
        Caps9 capabilities = default;
        capabilities.MaxTextureBlendStages = 1;
        capabilities.MaxAnisotropy = maximumAnisotropy;

        int result = Direct3D9DefaultState.Initialize(
            capabilities,
            (_, _) => 0,
            (_, _, _) => 0,
            (_, state, value) =>
            {
                Assert.AreEqual(Samplerstatetype.Maxanisotropy, state);
                actualLevel = value;
                return 0;
            },
            (_, _) => 0,
            _ => 0,
            _ => 0,
            () => 0,
            () => 0,
            _ => 0,
            () => 0);

        Assert.AreEqual(0, result);
        Assert.AreEqual(expectedLevel, actualLevel);
    }
}
