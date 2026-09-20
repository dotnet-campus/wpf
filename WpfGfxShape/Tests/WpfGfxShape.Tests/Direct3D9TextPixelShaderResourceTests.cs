using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9TextPixelShaderResourceTests
{
    [TestMethod]
    [DataRow(100u, 0xFFFF0101u)]
    [DataRow(104u, 0xFFFF0101u)]
    [DataRow(108u, 0xFFFF0200u)]
    [DataRow(112u, 0xFFFF0200u)]
    public void WhenNativeTextShaderResourceIsLoadedThenBytecodeHasExpectedVersionAndEndToken(
        uint resourceId,
        uint expectedVersion)
    {
        bool found = Direct3D9TextPixelShaderResources.TryGetShaderBytecode(resourceId, out ReadOnlySpan<uint> bytecode);

        Assert.AreEqual((true, expectedVersion, 0x0000FFFFu), (found, bytecode[0], bytecode[^1]));
    }

    [TestMethod]
    public void WhenAllNativeTextShaderResourcesAreLoadedThenEveryResourceIsAvailable()
    {
        int loadedResourceCount = 0;

        for (uint resourceId = 100; resourceId <= 115; resourceId++)
        {
            if (Direct3D9TextPixelShaderResources.TryGetShaderBytecode(resourceId, out ReadOnlySpan<uint> bytecode)
                && !bytecode.IsEmpty)
            {
                loadedResourceCount++;
            }
        }

        Assert.AreEqual(16, loadedResourceCount);
    }

    [TestMethod]
    public void WhenUnknownTextShaderResourceIsRequestedThenItIsRejected()
    {
        bool found = Direct3D9TextPixelShaderResources.TryGetShaderBytecode(99, out ReadOnlySpan<uint> bytecode);

        Assert.AreEqual((false, true), (found, bytecode.IsEmpty));
    }
}
