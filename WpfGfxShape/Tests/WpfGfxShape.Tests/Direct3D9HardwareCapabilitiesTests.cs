using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9HardwareCapabilitiesTests
{
    [TestMethod]
    public void WhenAllRequiredLevel1CapabilitiesArePresentThenCheckSucceeds()
    {
        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(CreateSupportedCapabilities());

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenPowerOfTwoTexturesAreRequiredWithoutConditionalSupportThenCheckFails()
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.TextureCaps = D3D9.PtexturecapsPow2;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenConditionalNonPowerOfTwoTexturesAreSupportedThenCheckSucceeds()
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.TextureCaps = D3D9.PtexturecapsPow2 | D3D9.PtexturecapsNonpow2Conditional;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenOnlySquareTexturesAreSupportedThenCheckFails()
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.TextureCaps = D3D9.PtexturecapsSquareonly;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    [DataRow(1u, 2u)]
    [DataRow(2u, 1u)]
    public void WhenFewerThanTwoTextureStagesOrTexturesAreSupportedThenCheckFails(
        uint maxTextureBlendStages,
        uint maxSimultaneousTextures)
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.MaxTextureBlendStages = maxTextureBlendStages;
        capabilities.MaxSimultaneousTextures = maxSimultaneousTextures;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenHardwareDeviceCannotMaskColorChannelsThenCheckFails()
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.PrimitiveMiscCaps = 0;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenSoftwareDeviceCannotMaskColorChannelsThenCheckSucceeds()
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.DeviceType = Devtype.SW;
        capabilities.PrimitiveMiscCaps = 0;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [DataRow(D3D9.PblendcapsZero)]
    [DataRow(D3D9.PblendcapsOne)]
    [DataRow(D3D9.PblendcapsSrcalpha)]
    [DataRow(D3D9.PblendcapsInvdestalpha)]
    public void WhenRequiredSourceBlendCapabilityIsMissingThenCheckFails(int missingCapability)
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.SrcBlendCaps &= ~(uint) missingCapability;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    [DataRow(D3D9.PblendcapsZero)]
    [DataRow(D3D9.PblendcapsOne)]
    [DataRow(D3D9.PblendcapsInvsrccolor)]
    [DataRow(D3D9.PblendcapsInvsrcalpha)]
    public void WhenRequiredDestinationBlendCapabilityIsMissingThenCheckFails(int missingCapability)
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.DestBlendCaps &= ~(uint) missingCapability;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenUnrestrictedNonPowerOfTwoTexturesAreSupportedThenCheckSucceeds()
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.TextureCaps = 0;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenCapabilitiesNotCheckedByNativeLevel1CheckAreAbsentThenCheckSucceeds()
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.TextureOpCaps = 0;
        capabilities.ShadeCaps = 0;
        capabilities.RasterCaps = 0;
        capabilities.MaxTextureWidth = 0;
        capabilities.MaxTextureHeight = 0;
        capabilities.PixelShaderVersion = 0;
        capabilities.VertexShaderVersion = 0;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenDisplayFormatCheckFailsThenHardwareCapabilitiesRemainAtDefaults()
    {
        Direct3D9GraphicsAccelerationSnapshot result = Direct3D9HardwareCapabilities.ReadGraphicsAccelerationCaps(
            32,
            128 * 1024 * 1024,
            isRecentDriver: false,
            isBadDriver: false,
            Direct3D9Factory.GenericFailureHResult,
            0,
            CreateTier2Capabilities());

        Assert.AreEqual(
            Direct3D9HardwareCapabilities.CreateNoHardwareAccelerationCaps(32),
            result.Capabilities);
    }

    [TestMethod]
    public void WhenDeviceCapsReadFailsThenOnlyWindowCompatibilityIsReported()
    {
        Direct3D9GraphicsAccelerationSnapshot result = Direct3D9HardwareCapabilities.ReadGraphicsAccelerationCaps(
            32,
            128 * 1024 * 1024,
            isRecentDriver: false,
            isBadDriver: false,
            0,
            Direct3D9Factory.GenericFailureHResult,
            default);

        Assert.AreEqual(1, result.Capabilities.WindowCompatibleMode);
    }

    [TestMethod]
    public void WhenWddmTier2CapabilitiesAreReadThenDriverAndHardwareFieldsAreReported()
    {
        Caps9 capabilities = CreateTier2Capabilities();
        capabilities.Caps2 = D3D9.Caps2Canshareresource;
        capabilities.MaxTextureWidth = 4096;
        capabilities.MaxTextureHeight = 2048;
        capabilities.MaxPixelShader30InstructionSlots = 512;

        Direct3D9GraphicsAccelerationSnapshot result = Direct3D9HardwareCapabilities.ReadGraphicsAccelerationCaps(
            32,
            0,
            isRecentDriver: false,
            isBadDriver: false,
            0,
            0,
            capabilities);

        Assert.AreEqual(
            (true, 2 << 16, 1, 4096U, 2048U, 512U),
            (result.IsRecentDriver,
             result.Capabilities.TierValue,
             result.Capabilities.HasWddmSupport,
             result.Capabilities.MaxTextureWidth,
             result.Capabilities.MaxTextureHeight,
             result.Capabilities.MaxPixelShader30InstructionSlots));
    }

    [TestMethod]
    public void WhenTier2SpecificCapsAreMissingThenTier1IsReported()
    {
        Caps9 capabilities = CreateTier2Capabilities();
        capabilities.MaxTextureBlendStages = 3;

        int result = Direct3D9HardwareCapabilities.GetTier(128 * 1024 * 1024, capabilities);

        Assert.AreEqual(1 << 16, result);
    }

    [TestMethod]
    public void WhenDriverIsBadThenHardwareCapabilitiesRemainAtDefaults()
    {
        Direct3D9GraphicsAccelerationSnapshot result = Direct3D9HardwareCapabilities.ReadGraphicsAccelerationCaps(
            32,
            128 * 1024 * 1024,
            isRecentDriver: true,
            isBadDriver: true,
            0,
            0,
            CreateTier2Capabilities());

        Assert.AreEqual(0, result.Capabilities.WindowCompatibleMode);
    }

    private static Caps9 CreateSupportedCapabilities()
    {
        return new Caps9(
            deviceType: Devtype.Hal,
            primitiveMiscCaps: D3D9.PmisccapsColorwriteenable,
            srcBlendCaps: D3D9.PblendcapsZero
                | D3D9.PblendcapsOne
                | D3D9.PblendcapsSrcalpha
                | D3D9.PblendcapsInvdestalpha,
            destBlendCaps: D3D9.PblendcapsZero
                | D3D9.PblendcapsOne
                | D3D9.PblendcapsInvsrccolor
                | D3D9.PblendcapsInvsrcalpha,
            maxTextureBlendStages: 2,
            maxSimultaneousTextures: 2);
    }

    private static Caps9 CreateTier2Capabilities()
    {
        Caps9 capabilities = CreateSupportedCapabilities();
        capabilities.PixelShaderVersion = 0xFFFF0200;
        capabilities.VertexShaderVersion = 0xFFFE0200;
        capabilities.MaxTextureBlendStages = 4;
        capabilities.SrcBlendCaps |= (uint) D3D9.PblendcapsBlendfactor;
        return capabilities;
    }
}
