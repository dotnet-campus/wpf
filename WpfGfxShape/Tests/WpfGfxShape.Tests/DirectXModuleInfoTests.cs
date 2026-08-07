using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class DirectXModuleInfoTests
{
    [TestMethod]
    [DataRow((int) DirectXModule.Direct3D9, DirectXModuleInfo.Direct3D9LibraryName)]
    [DataRow((int) DirectXModule.Direct3D11, DirectXModuleInfo.Direct3D11LibraryName)]
    [DataRow((int) DirectXModule.Dxgi, DirectXModuleInfo.DxgiLibraryName)]
    public void WhenModuleIsKnownThenReturnsSystemLibraryName(int moduleValue, string expected)
    {
        string actual = DirectXModuleInfo.GetLibraryName((DirectXModule) moduleValue);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void WhenModuleIsUnknownThenThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => DirectXModuleInfo.GetLibraryName((DirectXModule) int.MaxValue));
    }

    [TestMethod]
    public void WhenReadingDirect3D9SdkVersionThenUsesSilkNetBinding()
    {
        Assert.AreEqual(32U, DirectXModuleInfo.Direct3D9SdkVersion);
    }

    [TestMethod]
    public void WhenReadingDirect3D9BindingThenReturnsSilkNetType()
    {
        Assert.AreEqual("Silk.NET.Direct3D9.D3D9", DirectXBindingInfo.Direct3D9ApiType.FullName);
    }

    [TestMethod]
    public void WhenReadingDirect3D11BindingThenReturnsSilkNetType()
    {
        Assert.AreEqual("Silk.NET.Direct3D11.D3D11", DirectXBindingInfo.Direct3D11ApiType.FullName);
    }

    [TestMethod]
    public void WhenReadingDxgiBindingThenReturnsSilkNetType()
    {
        Assert.AreEqual("Silk.NET.DXGI.DXGI", DirectXBindingInfo.DxgiApiType.FullName);
    }
}
