using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class FrozenShaderInvocationTests
{
    [TestMethod]
    public void WhenEffectArraysChangeThenInvocationKeepsCapturedValues()
    {
        GeneratedShaderEffectData data = new(default, [0], [.25f, 0, 0, 1],
            [1], [2, 3, 4, 5], [2], [1], [0, 1], [7], [null], null);
        using var invocation = new FrozenShaderInvocation(data);
        data.FloatValues[0] = .75f;
        data.FloatRegisters[0] = 3;
        data.IntValues[0] = 9;
        data.IntRegisters[0] = 4;
        data.BoolValues[0] = 0;
        data.BoolRegisters[0] = 5;
        data.SamplerInfo[0] = 8;
        data.SamplerHandles[0] = 10;
        Assert.AreEqual((.25f, (short)0, 2, (short)1, 1, (short)2, 0, 7u),
            (invocation.Data.FloatValues[0], invocation.Data.FloatRegisters[0],
                invocation.Data.IntValues[0], invocation.Data.IntRegisters[0],
                invocation.Data.BoolValues[0], invocation.Data.BoolRegisters[0],
                invocation.Data.SamplerInfo[0], invocation.Data.SamplerHandles[0]));
    }

    [TestMethod]
    public void WhenNextInvocationIsCapturedThenItUsesUpdatedValues()
    {
        GeneratedShaderEffectData data = new(default, [0], [.25f, 0, 0, 1],
            [], [], [], [], [], [], [], null);
        using var first = new FrozenShaderInvocation(data);
        data.FloatValues[0] = .75f;
        using var next = new FrozenShaderInvocation(data);
        Assert.AreEqual(.75f, next.Data.FloatValues[0]);
    }
}
