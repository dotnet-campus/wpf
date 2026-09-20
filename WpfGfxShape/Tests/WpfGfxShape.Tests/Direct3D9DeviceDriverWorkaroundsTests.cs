using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DeviceDriverWorkaroundsTests
{
    [TestMethod]
    public void WhenHardwareDriverIsNotRecentThenCheckFails()
    {
        Caps9 capabilities = CreateCapabilities();

        int result = Direct3D9DeviceDriverWorkarounds.Apply(
            Devtype.Hal,
            skipDriverCheck: false,
            isRecentDriver: false,
            isBadDriver: false,
            ref capabilities);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenHardwareDriverIsBadThenCheckFails()
    {
        Caps9 capabilities = CreateCapabilities();

        int result = Direct3D9DeviceDriverWorkarounds.Apply(
            Devtype.Hal,
            skipDriverCheck: false,
            isRecentDriver: true,
            isBadDriver: true,
            ref capabilities);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenHardwareDriverIsAcceptedThenScissorCapabilityIsCleared()
    {
        Caps9 capabilities = CreateCapabilities();
        uint expected = capabilities.RasterCaps & unchecked((uint) ~D3D9.PrastercapsScissortest);

        int result = Direct3D9DeviceDriverWorkarounds.Apply(
            Devtype.Hal,
            skipDriverCheck: false,
            isRecentDriver: true,
            isBadDriver: false,
            ref capabilities);

        Assert.AreEqual((0, expected), (result, capabilities.RasterCaps));
    }

    [TestMethod]
    [DataRow(Devtype.SW)]
    [DataRow(Devtype.Ref)]
    public void WhenDeviceDoesNotUseHardwareDriverThenCapabilitiesAreUnchanged(Devtype deviceType)
    {
        Caps9 capabilities = CreateCapabilities();
        Caps9 expected = capabilities;

        int result = Direct3D9DeviceDriverWorkarounds.Apply(
            deviceType,
            skipDriverCheck: false,
            isRecentDriver: false,
            isBadDriver: true,
            ref capabilities);

        Assert.AreEqual((0, expected.RasterCaps), (result, capabilities.RasterCaps));
    }

    [TestMethod]
    public void WhenDriverCheckIsSkippedThenCapabilitiesAreUnchanged()
    {
        Caps9 capabilities = CreateCapabilities();
        Caps9 expected = capabilities;

        int result = Direct3D9DeviceDriverWorkarounds.Apply(
            Devtype.Hal,
            skipDriverCheck: true,
            isRecentDriver: false,
            isBadDriver: true,
            ref capabilities);

        Assert.AreEqual((0, expected.RasterCaps), (result, capabilities.RasterCaps));
    }

    private static Caps9 CreateCapabilities()
    {
        return new Caps9
        {
            RasterCaps = unchecked((uint) (D3D9.PrastercapsScissortest | 0x40))
        };
    }
}
