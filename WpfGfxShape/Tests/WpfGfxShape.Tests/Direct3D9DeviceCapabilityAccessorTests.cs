using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceCapabilityAccessorTests
{
    [TestMethod]
    [DataRow(Devtype.Hal, false, true, false)]
    [DataRow(Devtype.Hal, true, true, false)]
    [DataRow(Devtype.Ref, false, false, false)]
    [DataRow(Devtype.Ref, true, false, false)]
    [DataRow(Devtype.SW, false, false, true)]
    [DataRow(Devtype.SW, true, false, true)]
    public void WhenDeviceKindsAreQueriedThenEachNativeCapabilityIdentityIsIndependent(
        Devtype deviceType,
        bool isLddmDevice,
        bool isHardwareDevice,
        bool isSoftwareDevice)
    {
        Caps9 capabilities = new()
        {
            DeviceType = deviceType,
            Caps2 = isLddmDevice ? (uint) D3D9.Caps2Canshareresource : 0
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (isLddmDevice, isHardwareDevice, isSoftwareDevice),
            (device.IsLddmDevice, device.IsHardwareDevice, device.IsSoftwareDevice));
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void WhenDeviceKindsAreQueriedThenDevicePointerAndCreationIdentityDoNotInterfere(
        bool isPureDevice,
        bool isExtendedDevice)
    {
        Caps9 capabilities = new()
        {
            DeviceType = Devtype.SW,
            Caps = uint.MaxValue,
            Caps2 = uint.MaxValue,
            Caps3 = uint.MaxValue,
            DevCaps = uint.MaxValue,
            DevCaps2 = uint.MaxValue,
            PrimitiveMiscCaps = uint.MaxValue,
            RasterCaps = uint.MaxValue,
            SrcBlendCaps = uint.MaxValue,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            TextureAddressCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        uint behaviorFlags = D3D9.CreateMultithreaded | (isPureDevice ? D3D9.CreatePuredevice : 0u);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            uint.MaxValue,
            Devtype.Hal,
            behaviorFlags,
            default,
            capabilities: capabilities,
            adapterLuid: long.MinValue);
        device.UpdateTier(int.MaxValue);

        Assert.AreEqual(
            (true, false, true, true, false),
            (device.IsLddmDevice,
                device.IsHardwareDevice,
                device.IsSoftwareDevice,
                device.IsLddmDevice,
                device.IsEntered()));
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenAdapterOrdinalAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.AdapterOrdinal);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenDeviceTypeAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.DeviceType);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenBehaviorFlagsAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.BehaviorFlags);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenFocusWindowAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.FocusWindow);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenLddmIdentityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.IsLddmDevice);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenHardwareIdentityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.IsHardwareDevice);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenSoftwareIdentityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.IsSoftwareDevice);
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void WhenPureDeviceIdentityIsQueriedThenOnlyCachedCreationBehaviorFlagIsUsed(
        bool isPureDevice,
        bool isExtendedDevice)
    {
        const uint unrelatedBehaviorFlag = D3D9.CreateMultithreaded;
        uint behaviorFlags = unrelatedBehaviorFlag | (isPureDevice ? D3D9.CreatePuredevice : 0u);
        Caps9 capabilities = new()
        {
            DeviceType = isPureDevice ? Devtype.SW : Devtype.Hal,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            0,
            isPureDevice ? Devtype.Hal : Devtype.SW,
            behaviorFlags,
            default,
            capabilities: capabilities);
        device.UpdateTier(2 << 16);

        Assert.AreEqual(
            (isPureDevice, isPureDevice, false),
            (device.IsPureDevice, device.IsPureDevice, device.IsEntered()));
    }

    [TestMethod]
    public void WhenTextureAndStretchCapabilitiesAreQueriedThenNativeBitsAreUsed()
    {
        Caps9 capabilities = new()
        {
            Caps2 = (uint) D3D9.Caps2Canautogenmipmap,
            StretchRectFilterCaps = (uint) D3D9.PtfiltercapsMinflinear,
            DevCaps2 = (uint) D3D9.Devcaps2CanStretchrectFromTextures,
            TextureAddressCaps = (uint) D3D9.PtaddresscapsBorder
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (true, true, true, true),
            (device.CanAutoGenerateMipmaps,
                device.CanGenerateMipmapsWithStretchRect,
                device.CanStretchRectFromTextures,
                device.SupportsBorderColor));
    }

    [TestMethod]
    [DataRow(false, false, false, false, false)]
    [DataRow(true, false, false, true, false)]
    [DataRow(false, true, false, false, true)]
    [DataRow(false, false, true, true, true)]
    [DataRow(true, true, true, false, true)]
    public void WhenMipmapCapabilitiesAreQueriedThenOnlyTheirCachedBitsAreTested(
        bool canAutoGenerateMipmaps,
        bool canGenerateMipmapsWithStretchRect,
        bool canStretchRectFromTextures,
        bool isPureDevice,
        bool isExtendedDevice)
    {
        const uint unrelatedCapability = 0x00000001;
        Caps9 capabilities = new()
        {
            Caps2 = unrelatedCapability | (canAutoGenerateMipmaps ? (uint) D3D9.Caps2Canautogenmipmap : 0),
            StretchRectFilterCaps = unrelatedCapability | (canGenerateMipmapsWithStretchRect ? (uint) D3D9.PtfiltercapsMinflinear : 0),
            DevCaps2 = unrelatedCapability | (canStretchRectFromTextures ? (uint) D3D9.Devcaps2CanStretchrectFromTextures : 0),
            DeviceType = canAutoGenerateMipmaps ? Devtype.SW : Devtype.Hal,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            0,
            capabilities.DeviceType,
            D3D9.CreateMultithreaded | (isPureDevice ? D3D9.CreatePuredevice : 0u),
            default,
            capabilities: capabilities);
        device.UpdateTier(2 << 16);

        Assert.AreEqual(
            (canAutoGenerateMipmaps,
                canGenerateMipmapsWithStretchRect,
                canStretchRectFromTextures,
                canAutoGenerateMipmaps,
                canGenerateMipmapsWithStretchRect,
                canStretchRectFromTextures,
                false),
            (device.CanAutoGenerateMipmaps,
                device.CanGenerateMipmapsWithStretchRect,
                device.CanStretchRectFromTextures,
                device.CanAutoGenerateMipmaps,
                device.CanGenerateMipmapsWithStretchRect,
                device.CanStretchRectFromTextures,
                device.IsEntered()));
    }

    [TestMethod]
    public void WhenRenderCapabilitiesAreQueriedThenNativeBitsAreUsed()
    {
        Caps9 capabilities = new()
        {
            PrimitiveMiscCaps = (uint) D3D9.PmisccapsColorwriteenable,
            SrcBlendCaps = (uint) D3D9.PblendcapsBlendfactor,
            RasterCaps = (uint) D3D9.PrastercapsScissortest,
            Caps3 = (uint) D3D9.Caps3LinearToSrgbPresentation
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (true, true, true, true),
            (device.CanMaskColorChannels,
                device.CanHandleBlendFactor,
                device.SupportsScissorRectangle,
                device.SupportsLinearToSrgbPresentation));
    }

    [TestMethod]
    [DataRow(false, false, false, false, false)]
    [DataRow(false, false, true, true, false)]
    [DataRow(false, true, false, false, true)]
    [DataRow(false, true, true, true, true)]
    [DataRow(true, false, false, true, true)]
    [DataRow(true, false, true, false, false)]
    [DataRow(true, true, false, true, false)]
    [DataRow(true, true, true, false, true)]
    public void WhenBaseRenderCapabilitiesAreQueriedThenOnlyTheirCachedBitsAreTested(
        bool canMaskColorChannels,
        bool canHandleBlendFactor,
        bool supportsBorderColor,
        bool isPureDevice,
        bool isExtendedDevice)
    {
        const uint unrelatedCapability = 0x00000001;
        Caps9 capabilities = new()
        {
            PrimitiveMiscCaps = unrelatedCapability | (canMaskColorChannels ? (uint) D3D9.PmisccapsColorwriteenable : 0),
            SrcBlendCaps = unrelatedCapability | (canHandleBlendFactor ? (uint) D3D9.PblendcapsBlendfactor : 0),
            TextureAddressCaps = unrelatedCapability | (supportsBorderColor ? (uint) D3D9.PtaddresscapsBorder : 0),
            DeviceType = canMaskColorChannels ? Devtype.SW : Devtype.Hal,
            Caps2 = uint.MaxValue,
            Caps3 = uint.MaxValue,
            RasterCaps = uint.MaxValue,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            0,
            capabilities.DeviceType,
            D3D9.CreateMultithreaded | (isPureDevice ? D3D9.CreatePuredevice : 0u),
            default,
            capabilities: capabilities);
        device.UpdateTier(2 << 16);

        Assert.AreEqual(
            (canMaskColorChannels,
                canHandleBlendFactor,
                supportsBorderColor,
                canMaskColorChannels,
                canHandleBlendFactor,
                supportsBorderColor,
                false),
            (device.CanMaskColorChannels,
                device.CanHandleBlendFactor,
                device.SupportsBorderColor,
                device.CanMaskColorChannels,
                device.CanHandleBlendFactor,
                device.SupportsBorderColor,
                device.IsEntered()));
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void WhenScissorAndLinearPresentationCapabilitiesAreQueriedThenOnlyTheirCachedBitsAreTested(
        bool supportsScissorRectangle,
        bool supportsLinearPresentation)
    {
        const uint unrelatedCapability = 0x00000001;
        Caps9 capabilities = new()
        {
            RasterCaps = unrelatedCapability | (supportsScissorRectangle ? (uint) D3D9.PrastercapsScissortest : 0),
            Caps3 = unrelatedCapability | (supportsLinearPresentation ? (uint) D3D9.Caps3LinearToSrgbPresentation : 0),
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue,
            DeviceType = Devtype.Hal
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        device.UpdateTier(2 << 16);

        Assert.AreEqual(
            (supportsScissorRectangle, supportsLinearPresentation, false),
            (device.SupportsScissorRectangle, device.SupportsLinearToSrgbPresentation, device.IsEntered()));
    }

    [TestMethod]
    [DataRow(false, false, false, false, false)]
    [DataRow(false, false, true, true, false)]
    [DataRow(false, true, false, false, true)]
    [DataRow(false, true, true, true, true)]
    [DataRow(true, false, false, true, true)]
    [DataRow(true, false, true, false, false)]
    [DataRow(true, true, false, true, false)]
    [DataRow(true, true, true, false, true)]
    public void WhenTextureFormatSupportIsUpdatedThenEachAccessorUsesOnlyItsCachedResult(
        bool supportsA8,
        bool supportsP8,
        bool supportsL8,
        bool isPureDevice,
        bool isExtendedDevice)
    {
        Caps9 capabilities = new()
        {
            DeviceType = supportsA8 ? Devtype.SW : Devtype.Hal,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            TextureAddressCaps = uint.MaxValue,
            PrimitiveMiscCaps = uint.MaxValue,
            SrcBlendCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            MaxTextureBlendStages = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            0,
            capabilities.DeviceType,
            D3D9.CreateMultithreaded | (isPureDevice ? D3D9.CreatePuredevice : 0u),
            default,
            capabilities: capabilities);
        device.UpdateTier(2 << 16);
        device.UpdateTextureFormatSupport(new Direct3D9TextureFormatSupport(
            supportsA8,
            supportsP8,
            supportsL8,
            MilPixelFormat.Prgba128BppFloat,
            MilPixelFormat.Rgb128BppFloat,
            MilPixelFormat.Bgr32Bpp101010,
            MilPixelFormat.Pbgra32Bpp,
            MilPixelFormat.Bgr32Bpp));

        Assert.AreEqual(
            (supportsA8, supportsP8, supportsL8, supportsA8, supportsP8, supportsL8, false),
            (device.SupportsA8TextureFormat,
                device.SupportsP8TextureFormat,
                device.SupportsL8TextureFormat,
                device.SupportsA8TextureFormat,
                device.SupportsP8TextureFormat,
                device.SupportsL8TextureFormat,
                device.IsEntered()));
    }

    [TestMethod]
    public void WhenTextureFormatSupportIsUpdatedAgainThenAccessorsUseFinalCachedResults()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateTextureFormatSupport(new Direct3D9TextureFormatSupport(
            SupportsA8: true,
            SupportsP8: false,
            SupportsL8: true,
            default,
            default,
            default,
            default,
            default));

        device.UpdateTextureFormatSupport(new Direct3D9TextureFormatSupport(
            SupportsA8: false,
            SupportsP8: true,
            SupportsL8: false,
            default,
            default,
            default,
            default,
            default));

        Assert.AreEqual(
            (false, true, false, false),
            (device.SupportsA8TextureFormat,
                device.SupportsP8TextureFormat,
                device.SupportsL8TextureFormat,
                device.IsEntered()));
    }

    [TestMethod]
    public void WhenAnisotropyAndTextureShapeCapabilitiesAreQueriedThenNativeRulesAreUsed()
    {
        Caps9 capabilities = new()
        {
            MaxAnisotropy = 8,
            TextureCaps = (uint) D3D9.PtexturecapsPow2 | (uint) D3D9.PtexturecapsNonpow2Conditional
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (4u, true, true, false, true),
            (device.MaximumDesiredAnisotropicFilterLevel,
                device.SupportsAnisotropicFiltering,
                device.SupportsConditionalNonPowerOfTwoTextures,
                device.SupportsUnconditionalNonPowerOfTwoTextures,
                device.SupportsTextureCapability((uint) D3D9.PtexturecapsPow2)));
    }

    [TestMethod]
    [DataRow(0u, 1u, false)]
    [DataRow(1u, 1u, false)]
    [DataRow(2u, 2u, true)]
    [DataRow(3u, 3u, true)]
    [DataRow(4u, 4u, true)]
    [DataRow(5u, 4u, true)]
    [DataRow(uint.MaxValue, 4u, true)]
    public void WhenAnisotropicCapabilitiesAreQueriedThenCachedMaximumIsCappedAtFour(
        uint maximumAnisotropy,
        uint expectedMaximumDesiredLevel,
        bool expectedSupport)
    {
        Caps9 capabilities = new()
        {
            MaxAnisotropy = maximumAnisotropy
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (expectedMaximumDesiredLevel, expectedSupport, false),
            (device.MaximumDesiredAnisotropicFilterLevel, device.SupportsAnisotropicFiltering, device.IsEntered()));
    }

    [TestMethod]
    [DataRow(false, false, false, true)]
    [DataRow(false, true, false, false)]
    [DataRow(true, false, false, false)]
    [DataRow(true, true, true, false)]
    public void WhenNonPowerOfTwoTextureCapabilitiesAreQueriedThenBothNativeBitsDetermineSupport(
        bool hasConditionalCapability,
        bool hasPowerOfTwoCapability,
        bool expectedConditionalSupport,
        bool expectedUnconditionalSupport)
    {
        uint textureCapabilities = 0;
        if (hasConditionalCapability)
        {
            textureCapabilities |= (uint) D3D9.PtexturecapsNonpow2Conditional;
        }

        if (hasPowerOfTwoCapability)
        {
            textureCapabilities |= (uint) D3D9.PtexturecapsPow2;
        }

        Caps9 capabilities = new()
        {
            TextureCaps = textureCapabilities
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (expectedConditionalSupport, expectedUnconditionalSupport),
            (device.SupportsConditionalNonPowerOfTwoTextures, device.SupportsUnconditionalNonPowerOfTwoTextures));
    }

    [TestMethod]
    [DataRow(Devtype.Hal, false, false)]
    [DataRow(Devtype.Hal, true, true)]
    [DataRow(Devtype.Ref, false, true)]
    [DataRow(Devtype.Ref, true, false)]
    [DataRow(Devtype.SW, false, false)]
    [DataRow(Devtype.SW, true, true)]
    public void WhenNonPowerOfTwoTextureCapabilitiesAreReadRepeatedlyThenOtherCapabilitiesAndDeviceIdentityDoNotInterfere(
        Devtype deviceType,
        bool isExtendedDevice,
        bool conditionalSupport)
    {
        uint nonPowerOfTwoCapabilities = (uint) D3D9.PtexturecapsNonpow2Conditional | (uint) D3D9.PtexturecapsPow2;
        Caps9 capabilities = new()
        {
            DeviceType = deviceType,
            TextureCaps = uint.MaxValue & ~nonPowerOfTwoCapabilities
                | (conditionalSupport ? nonPowerOfTwoCapabilities : 0)
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            capabilities);

        Assert.AreEqual(
            (conditionalSupport, !conditionalSupport, conditionalSupport, !conditionalSupport, 0u, false),
            (device.SupportsConditionalNonPowerOfTwoTextures,
                device.SupportsUnconditionalNonPowerOfTwoTextures,
                device.SupportsConditionalNonPowerOfTwoTextures,
                device.SupportsUnconditionalNonPowerOfTwoTextures,
                deviceObject.ReleaseCallCount,
                device.IsEntered()));
    }

    [TestMethod]
    public void WhenSingleTextureCapabilityIsQueriedThenOnlyThatCachedBitIsTested()
    {
        Caps9 capabilities = new()
        {
            TextureCaps = (uint) D3D9.PtexturecapsPow2
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (true, false),
            (device.SupportsTextureCapability((uint) D3D9.PtexturecapsPow2),
                device.SupportsTextureCapability((uint) D3D9.PtexturecapsNonpow2Conditional)));
    }

    [TestMethod]
    [DataRow(0u, 1u, false)]
    [DataRow(1u, 1u, true)]
    [DataRow(0u, 0x80000000u, false)]
    [DataRow(0x80000000u, 0x80000000u, true)]
    [DataRow(uint.MaxValue ^ 1u, 1u, false)]
    [DataRow(uint.MaxValue ^ 0x80000000u, 0x80000000u, false)]
    public void WhenSingleTextureCapabilityBoundaryBitIsQueriedThenOnlyThatBitDeterminesSupport(
        uint textureCapabilities,
        uint textureCapability,
        bool expectedSupport)
    {
        Caps9 capabilities = new()
        {
            TextureCaps = textureCapabilities
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(expectedSupport, device.SupportsTextureCapability(textureCapability));
    }

    [TestMethod]
    [DataRow(Devtype.Hal, false)]
    [DataRow(Devtype.Hal, true)]
    [DataRow(Devtype.Ref, false)]
    [DataRow(Devtype.Ref, true)]
    [DataRow(Devtype.SW, false)]
    [DataRow(Devtype.SW, true)]
    public void WhenSingleTextureCapabilityIsReadRepeatedlyThenDeviceIdentityDoesNotInterfere(
        Devtype deviceType,
        bool isExtendedDevice)
    {
        const uint textureCapability = 0x80000000u;
        Caps9 capabilities = new()
        {
            DeviceType = deviceType,
            TextureCaps = textureCapability
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            capabilities);

        Assert.AreEqual(
            (true, true, 0u, false),
            (device.SupportsTextureCapability(textureCapability),
                device.SupportsTextureCapability(textureCapability),
                deviceObject.ReleaseCallCount,
                device.IsEntered()));
    }

    [TestMethod]
    public void WhenNonSingleTextureCapabilityMaskIsQueriedThenReleaseBitExpressionIsPreserved()
    {
        Caps9 capabilities = new()
        {
            TextureCaps = 2u
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (false, true, 0u, false),
            (device.SupportsTextureCapability(0),
                device.SupportsTextureCapability(3),
                deviceObject.ReleaseCallCount,
                device.IsEntered()));
    }

    [TestMethod]
    [DataRow(false, false, Texturefiltertype.Linear, Texturefiltertype.Linear, Texturefiltertype.None)]
    [DataRow(false, true, Texturefiltertype.Linear, Texturefiltertype.Anisotropic, Texturefiltertype.Linear)]
    [DataRow(true, false, Texturefiltertype.Linear, Texturefiltertype.Linear, Texturefiltertype.None)]
    [DataRow(true, true, Texturefiltertype.Anisotropic, Texturefiltertype.Anisotropic, Texturefiltertype.Linear)]
    public void WhenSupportedAnisotropicFilterModeIsQueriedThenInitializationCachesNativeFallbackMode(
        bool supportsMagnificationAnisotropy,
        bool supportsMinificationAnisotropy,
        Texturefiltertype expectedMagnificationFilter,
        Texturefiltertype expectedMinificationFilter,
        Texturefiltertype expectedMipmapFilter)
    {
        const uint unrelatedCapability = 0x00000001;
        uint textureFilterCapabilities = unrelatedCapability;
        if (supportsMagnificationAnisotropy)
        {
            textureFilterCapabilities |= (uint) D3D9.PtfiltercapsMagfanisotropic;
        }

        if (supportsMinificationAnisotropy)
        {
            textureFilterCapabilities |= (uint) D3D9.PtfiltercapsMinfanisotropic;
        }

        Caps9 capabilities = new()
        {
            TextureFilterCaps = textureFilterCapabilities,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue,
            DeviceType = Devtype.Hal
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        device.UpdateTier(2 << 16);
        Direct3D9FilterMode expected = new(
            expectedMagnificationFilter,
            expectedMinificationFilter,
            expectedMipmapFilter);

        Direct3D9FilterMode first = device.SupportedAnisotropicFilterMode;
        Direct3D9FilterMode second = device.SupportedAnisotropicFilterMode;

        Assert.AreEqual((expected, expected, false), (first, second, device.IsEntered()));
    }

    [TestMethod]
    public void WhenSupportedAnisotropicFilterModeIsReadRepeatedlyThenDeviceStateRemainsUnchanged()
    {
        int samplerStateCallCount = 0;
        int renderStateCallCount = 0;
        Caps9 capabilities = new()
        {
            TextureFilterCaps = (uint) D3D9.PtfiltercapsMagfanisotropic | (uint) D3D9.PtfiltercapsMinfanisotropic,
            DeviceType = Devtype.Hal
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: capabilities,
            setSamplerState: (_, _, _) =>
            {
                samplerStateCallCount++;
                return 0;
            },
            setRenderState: (_, _) =>
            {
                renderStateCallCount++;
                return 0;
            });

        Direct3D9FilterMode first = device.SupportedAnisotropicFilterMode;
        Direct3D9FilterMode second = device.SupportedAnisotropicFilterMode;

        Assert.AreEqual(
            (first, 0, 0, 0u, false),
            (second, samplerStateCallCount, renderStateCallCount, deviceObject.ReleaseCallCount, device.IsEntered()));
    }

    [TestMethod]
    public void WhenCapabilitiesAreUpdatedThenSupportedAnisotropicFilterModeCacheUsesFinalCapabilities()
    {
        Caps9 initialCapabilities = new()
        {
            TextureFilterCaps = (uint) D3D9.PtfiltercapsMagfanisotropic | (uint) D3D9.PtfiltercapsMinfanisotropic
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, initialCapabilities);
        Caps9 updatedCapabilities = initialCapabilities;
        updatedCapabilities.TextureFilterCaps = (uint) D3D9.PtfiltercapsMinfanisotropic;

        device.UpdateCapabilities(updatedCapabilities);

        Assert.AreEqual(
            new Direct3D9FilterMode(Texturefiltertype.Linear, Texturefiltertype.Anisotropic, Texturefiltertype.Linear),
            device.SupportedAnisotropicFilterMode);
    }

    [TestMethod]
    public void WhenCapabilitiesAreUpdatedWithZeroMaximumAnisotropyThenNativeDefaultIsCached()
    {
        Caps9 updatedCapabilities = new()
        {
            MaxAnisotropy = 0,
            TextureFilterCaps = (uint) D3D9.PtfiltercapsMagfanisotropic
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, new Caps9 { MaxAnisotropy = 4 });

        device.UpdateCapabilities(updatedCapabilities);

        Assert.AreEqual(
            (1u, false, new Direct3D9FilterMode(Texturefiltertype.Linear, Texturefiltertype.Linear, Texturefiltertype.None)),
            (device.MaximumDesiredAnisotropicFilterLevel,
                device.SupportsAnisotropicFiltering,
                device.SupportedAnisotropicFilterMode));
    }

    [TestMethod]
    [DataRow(0xFFFF0101u, 4u, true, false, true)]
    [DataRow(0xFFFF0100u, 4u, true, false, false)]
    [DataRow(0xFFFF0101u, 3u, true, false, false)]
    [DataRow(0xFFFF0101u, 4u, false, false, false)]
    [DataRow(0xFFFF0101u, 4u, true, true, false)]
    public void WhenTextRenderingPrerequisitesAreInitializedThenNativeCapabilityGateIsUsed(
        uint pixelShaderVersion,
        uint maximumTextureBlendStages,
        bool canHandleBlendFactor,
        bool isHardwareTextDisabled,
        bool expectedEligibility)
    {
        Caps9 capabilities = new()
        {
            PixelShaderVersion = pixelShaderVersion,
            MaxTextureBlendStages = maximumTextureBlendStages,
            SrcBlendCaps = canHandleBlendFactor ? (uint) D3D9.PblendcapsBlendfactor : 0
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        device.UpdateTextureFormatSupport(new Direct3D9TextureFormatSupport(
            SupportsA8: true,
            SupportsP8: false,
            SupportsL8: false,
            default,
            default,
            default,
            default,
            default));

        device.InitializeTextRenderingPrerequisites(isHardwareTextDisabled);

        Assert.AreEqual(
            (expectedEligibility,
                expectedEligibility ? Direct3D9GlyphAlphaTextureFormat.A8 : Direct3D9GlyphAlphaTextureFormat.Undefined,
                false,
                maximumTextureBlendStages),
            (device.IsTextPixelShaderInitializationEligible,
                device.GlyphAlphaTextureFormat,
                device.CanDrawText,
                device.MaximumTextureBlendStages));
    }

    [TestMethod]
    public void WhenTextAlphaTextureInitializationFailsThenPixelShaderInitializationIsIneligible()
    {
        Caps9 capabilities = new()
        {
            PixelShaderVersion = 0xFFFF0101,
            MaxTextureBlendStages = 4,
            SrcBlendCaps = (uint) D3D9.PblendcapsBlendfactor
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        device.InitializeTextRenderingPrerequisites(isHardwareTextDisabled: false);

        Assert.AreEqual(
            (false, Direct3D9GlyphAlphaTextureFormat.Undefined, false),
            (device.IsTextPixelShaderInitializationEligible, device.GlyphAlphaTextureFormat, device.CanDrawText));
    }

    [TestMethod]
    [DataRow(0, false)]
    [DataRow(1 << 16, false)]
    [DataRow((2 << 16) - 1, false)]
    [DataRow(2 << 16, true)]
    [DataRow(3 << 16, true)]
    [DataRow(int.MaxValue, true)]
    public void WhenMultisamplingEligibilityIsQueriedThenTierTwoIsTheMinimum(
        int tier,
        bool expectedEligibility)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateTier(tier);

        Assert.AreEqual(expectedEligibility, device.ShouldAttemptMultisample);
    }

    [TestMethod]
    public void WhenTierIsPromotedToTwoThenMultisamplingBecomesEligible()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateTier(1 << 16);
        bool beforePromotion = device.ShouldAttemptMultisample;

        device.UpdateTier(2 << 16);

        Assert.AreEqual((false, true), (beforePromotion, device.ShouldAttemptMultisample));
    }

    [TestMethod]
    public void WhenMultisamplingFailsThenFailureCacheIsIdempotentAndSurvivesTierUpdates()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateTier(2 << 16);
        bool beforeFailure = device.ShouldAttemptMultisample;

        device.SetMultisampleFailed();
        device.SetMultisampleFailed();
        bool afterRepeatedFailure = device.ShouldAttemptMultisample;
        device.UpdateTier(1 << 16);
        bool afterDemotion = device.ShouldAttemptMultisample;
        device.UpdateTier(int.MaxValue);

        Assert.AreEqual(
            (true, false, false, false),
            (beforeFailure, afterRepeatedFailure, afterDemotion, device.ShouldAttemptMultisample));
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void WhenMultisamplingEligibilityIsQueriedThenDeviceIdentityAndCapabilitiesDoNotInterfere(
        bool isPureDevice,
        bool isExtendedDevice)
    {
        Caps9 capabilities = new()
        {
            DeviceType = isPureDevice ? Devtype.SW : Devtype.Hal,
            Caps = uint.MaxValue,
            Caps2 = uint.MaxValue,
            Caps3 = uint.MaxValue,
            DevCaps = uint.MaxValue,
            DevCaps2 = uint.MaxValue,
            PrimitiveMiscCaps = uint.MaxValue,
            RasterCaps = uint.MaxValue,
            SrcBlendCaps = uint.MaxValue,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            TextureAddressCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        uint behaviorFlags = D3D9.CreateMultithreaded | (isPureDevice ? D3D9.CreatePuredevice : 0u);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            uint.MaxValue,
            isPureDevice ? Devtype.Hal : Devtype.SW,
            behaviorFlags,
            default,
            capabilities: capabilities);
        device.UpdateTier(2 << 16);

        Assert.AreEqual(
            (true, true, false),
            (device.ShouldAttemptMultisample, device.ShouldAttemptMultisample, device.IsEntered()));
    }

    [TestMethod]
    public void WhenMultisamplingFailureIsMarkedThenNoDeviceEntryIsUsed()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateTier(2 << 16);

        device.SetMultisampleFailed();

        Assert.AreEqual((false, false), (device.ShouldAttemptMultisample, device.IsEntered()));
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenMultisamplingEligibilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.ShouldAttemptMultisample);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenMultisamplingFailureMarkIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(device.SetMultisampleFailed);
    }

    [TestMethod]
    [DataRow((int) MilPixelFormat.Bgr32Bpp, MultisampleType.Multisample2Samples)]
    [DataRow((int) MilPixelFormat.Pbgra32Bpp, MultisampleType.Multisample3Samples)]
    [DataRow((int) MilPixelFormat.Bgr32Bpp101010, MultisampleType.Multisample4Samples)]
    [DataRow((int) MilPixelFormat.Undefined, MultisampleType.Multisample4Samples)]
    public void WhenSupportedMultisampleTypeIsQueriedThenDestinationFormatSelectsCachedType(
        int destinationFormat,
        MultisampleType expected)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateMultisampleSupport(new Direct3D9MultisampleSupport(
            MultisampleType.Multisample2Samples,
            MultisampleType.Multisample3Samples,
            MultisampleType.Multisample4Samples));

        Assert.AreEqual(expected, device.GetSupportedMultisampleType((MilPixelFormat) destinationFormat));
    }

    [TestMethod]
    public void WhenSupportedMultisampleTypeIsQueriedRepeatedlyThenOnlyCachedStateIsRead()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateMultisampleSupport(new Direct3D9MultisampleSupport(
            MultisampleType.Multisample2Samples,
            MultisampleType.Multisample3Samples,
            MultisampleType.Multisample4Samples));

        MultisampleType first = device.GetSupportedMultisampleType(MilPixelFormat.Pbgra32Bpp);
        MultisampleType second = device.GetSupportedMultisampleType(MilPixelFormat.Pbgra32Bpp);

        Assert.AreEqual(
            (MultisampleType.Multisample3Samples, MultisampleType.Multisample3Samples, false, 0u),
            (first, second, device.IsEntered(), deviceObject.ReleaseCallCount));
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenSupportedMultisampleTypeAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.GetSupportedMultisampleType(MilPixelFormat.Bgr32Bpp));
    }

    [TestMethod]
    [DataRow(0u, 0u, 0u, 0u, 0u, 0u, 0u)]
    [DataRow(3u, 4u, 5u, 1024u, 2048u, 0xFFFE0300u, 0xFFFF0300u)]
    [DataRow(uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue)]
    public void WhenNumericCapabilitiesAreQueriedThenCachedValuesAreReturnedUnchanged(
        uint maximumStreams,
        uint maximumTextureBlendStages,
        uint maximumSimultaneousTextures,
        uint maximumTextureWidth,
        uint maximumTextureHeight,
        uint vertexShaderVersion,
        uint pixelShaderVersion)
    {
        Caps9 capabilities = new()
        {
            MaxStreams = maximumStreams,
            MaxTextureBlendStages = maximumTextureBlendStages,
            MaxSimultaneousTextures = maximumSimultaneousTextures,
            MaxTextureWidth = maximumTextureWidth,
            MaxTextureHeight = maximumTextureHeight,
            VertexShaderVersion = vertexShaderVersion,
            PixelShaderVersion = pixelShaderVersion
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (maximumStreams,
                maximumTextureBlendStages,
                maximumSimultaneousTextures,
                maximumTextureWidth,
                maximumTextureHeight,
                vertexShaderVersion,
                pixelShaderVersion,
                false),
            (device.MaximumStreams,
                device.MaximumTextureBlendStages,
                device.MaximumSimultaneousTextures,
                device.MaximumTextureWidth,
                device.MaximumTextureHeight,
                device.VertexShaderVersion,
                device.PixelShaderVersion,
                device.IsEntered()));
    }

    [TestMethod]
    [DataRow(0u, uint.MaxValue, 0x13579BDFu)]
    [DataRow(0x2468ACE0u, 0x11223344u, 0x55667788u)]
    public void WhenMaximumStreamsIsQueriedThenOnlyCachedMaximumStreamsIsRead(
        uint maximumStreams,
        uint maximumTextureBlendStages,
        uint maximumSimultaneousTextures)
    {
        Caps9 capabilities = new()
        {
            MaxStreams = maximumStreams,
            MaxTextureBlendStages = maximumTextureBlendStages,
            MaxSimultaneousTextures = maximumSimultaneousTextures
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (maximumStreams, false, 0u),
            (device.MaximumStreams, device.IsEntered(), deviceObject.ReleaseCallCount));
    }

    [TestMethod]
    [DataRow(uint.MaxValue, 0u, 0x13579BDFu)]
    [DataRow(0x11223344u, 0x2468ACE0u, 0x55667788u)]
    public void WhenMaximumTextureBlendStagesIsQueriedThenOnlyCachedMaximumTextureBlendStagesIsRead(
        uint maximumStreams,
        uint maximumTextureBlendStages,
        uint maximumSimultaneousTextures)
    {
        Caps9 capabilities = new()
        {
            MaxStreams = maximumStreams,
            MaxTextureBlendStages = maximumTextureBlendStages,
            MaxSimultaneousTextures = maximumSimultaneousTextures
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (maximumTextureBlendStages, false, 0u),
            (device.MaximumTextureBlendStages, device.IsEntered(), deviceObject.ReleaseCallCount));
    }

    [TestMethod]
    [DataRow(uint.MaxValue, 0x13579BDFu, 0u)]
    [DataRow(0x11223344u, 0x55667788u, 0x2468ACE0u)]
    public void WhenMaximumSimultaneousTexturesIsQueriedThenOnlyCachedMaximumSimultaneousTexturesIsRead(
        uint maximumStreams,
        uint maximumTextureBlendStages,
        uint maximumSimultaneousTextures)
    {
        Caps9 capabilities = new()
        {
            MaxStreams = maximumStreams,
            MaxTextureBlendStages = maximumTextureBlendStages,
            MaxSimultaneousTextures = maximumSimultaneousTextures
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (maximumSimultaneousTextures, false, 0u),
            (device.MaximumSimultaneousTextures, device.IsEntered(), deviceObject.ReleaseCallCount));
    }

    [TestMethod]
    [DataRow(Devtype.Hal, false)]
    [DataRow(Devtype.Hal, true)]
    [DataRow(Devtype.Ref, false)]
    [DataRow(Devtype.Ref, true)]
    [DataRow(Devtype.SW, false)]
    [DataRow(Devtype.SW, true)]
    public void WhenMaximumStreamAndTextureCountsAreReadRepeatedlyThenDeviceIdentityDoesNotInterfere(
        Devtype deviceType,
        bool isExtendedDevice)
    {
        Caps9 capabilities = new()
        {
            DeviceType = deviceType,
            MaxStreams = 0x13579BDFu,
            MaxTextureBlendStages = 0x2468ACE0u,
            MaxSimultaneousTextures = 0x11223344u
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            capabilities);

        Assert.AreEqual(
            (0x13579BDFu, 0x2468ACE0u, 0x11223344u,
                0x13579BDFu, 0x2468ACE0u, 0x11223344u, false, 0u),
            (device.MaximumStreams, device.MaximumTextureBlendStages, device.MaximumSimultaneousTextures,
                device.MaximumStreams, device.MaximumTextureBlendStages, device.MaximumSimultaneousTextures,
                device.IsEntered(), deviceObject.ReleaseCallCount));
    }

    [TestMethod]
    [DataRow(0u, uint.MaxValue)]
    [DataRow(0x13579BDFu, 0x2468ACE0u)]
    public void WhenMaximumTextureWidthIsQueriedThenOnlyCachedWidthIsRead(
        uint maximumTextureWidth,
        uint maximumTextureHeight)
    {
        Caps9 capabilities = new()
        {
            MaxTextureWidth = maximumTextureWidth,
            MaxTextureHeight = maximumTextureHeight
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (maximumTextureWidth, false, 0u),
            (device.MaximumTextureWidth, device.IsEntered(), deviceObject.ReleaseCallCount));
    }

    [TestMethod]
    [DataRow(uint.MaxValue, 0u)]
    [DataRow(0x13579BDFu, 0x2468ACE0u)]
    public void WhenMaximumTextureHeightIsQueriedThenOnlyCachedHeightIsRead(
        uint maximumTextureWidth,
        uint maximumTextureHeight)
    {
        Caps9 capabilities = new()
        {
            MaxTextureWidth = maximumTextureWidth,
            MaxTextureHeight = maximumTextureHeight
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (maximumTextureHeight, false, 0u),
            (device.MaximumTextureHeight, device.IsEntered(), deviceObject.ReleaseCallCount));
    }

    [TestMethod]
    [DataRow(0u, uint.MaxValue)]
    [DataRow(0xFFFE0101u, 0xFFFF0300u)]
    public void WhenVertexShaderVersionIsQueriedThenOnlyCachedVertexVersionIsRead(
        uint vertexShaderVersion,
        uint pixelShaderVersion)
    {
        Caps9 capabilities = new()
        {
            VertexShaderVersion = vertexShaderVersion,
            PixelShaderVersion = pixelShaderVersion
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (vertexShaderVersion, false, 0u),
            (device.VertexShaderVersion, device.IsEntered(), deviceObject.ReleaseCallCount));
    }

    [TestMethod]
    [DataRow(uint.MaxValue, 0u)]
    [DataRow(0xFFFE0300u, 0xFFFF0101u)]
    public void WhenPixelShaderVersionIsQueriedThenOnlyCachedPixelVersionIsRead(
        uint vertexShaderVersion,
        uint pixelShaderVersion)
    {
        Caps9 capabilities = new()
        {
            VertexShaderVersion = vertexShaderVersion,
            PixelShaderVersion = pixelShaderVersion
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.AreEqual(
            (pixelShaderVersion, false, 0u),
            (device.PixelShaderVersion, device.IsEntered(), deviceObject.ReleaseCallCount));
    }

    [TestMethod]
    [DataRow(0L, false, false)]
    [DataRow(4294967295L, true, false)]
    [DataRow(-4294967296L, false, true)]
    [DataRow(long.MinValue, true, true)]
    public void WhenAdapterLuidIsQueriedThenOnlyInitializationValueIsReturned(
        long adapterLuid,
        bool isPureDevice,
        bool isExtendedDevice)
    {
        Caps9 capabilities = new()
        {
            DeviceType = isPureDevice ? Devtype.SW : Devtype.Hal,
            Caps = uint.MaxValue,
            Caps2 = uint.MaxValue,
            Caps3 = uint.MaxValue,
            DevCaps = uint.MaxValue,
            DevCaps2 = uint.MaxValue,
            PrimitiveMiscCaps = uint.MaxValue,
            RasterCaps = uint.MaxValue,
            SrcBlendCaps = uint.MaxValue,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            TextureAddressCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        uint behaviorFlags = D3D9.CreateMultithreaded | (isPureDevice ? D3D9.CreatePuredevice : 0u);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            uint.MaxValue,
            isPureDevice ? Devtype.Hal : Devtype.SW,
            behaviorFlags,
            default,
            capabilities: capabilities,
            adapterLuid: adapterLuid);
        device.UpdateTier(2 << 16);

        Assert.AreEqual(
            (adapterLuid, adapterLuid, false),
            (device.AdapterLuid, device.AdapterLuid, device.IsEntered()));
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenAdapterLuidAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = new(
            deviceObject.Device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            adapterLuid: 42);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.AdapterLuid);
    }

    [TestMethod]
    public void WhenRealizationCacheIndexIsQueriedThenConfiguredTokenIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            realizationCacheIndex: 17);

        Assert.AreEqual(17u, device.RealizationCacheIndex);
    }

    [TestMethod]
    public void WhenRealizationCacheIndicesAreExhaustedThenAcquisitionFailureKeepsInvalidToken()
    {
        List<uint> indices = [];
        try
        {
            for (int index = 1; index < 32; index++)
            {
                indices.Add(Direct3D9ResourceCacheIndexManager.AcquireIndex());
            }

            Assert.AreEqual(
                Direct3D9ImmediateBrushRealizer.InvalidRealizationCacheIndex,
                Direct3D9ResourceCacheIndexManager.AcquireIndex());
        }
        finally
        {
            foreach (uint index in indices)
            {
                Direct3D9ResourceCacheIndexManager.ReleaseIndex(index);
            }
        }
    }

    [TestMethod]
    public void WhenRealizationCacheIndexIsReleasedThenItCanBeAcquiredAgain()
    {
        uint index = Direct3D9ResourceCacheIndexManager.AcquireIndex();
        Direct3D9ResourceCacheIndexManager.ReleaseIndex(index);

        uint reacquiredIndex = Direct3D9ResourceCacheIndexManager.AcquireIndex();
        try
        {
            Assert.AreEqual(index, reacquiredIndex);
        }
        finally
        {
            Direct3D9ResourceCacheIndexManager.ReleaseIndex(reacquiredIndex);
        }
    }

    [TestMethod]
    public void WhenDeviceOwnsRealizationCacheIndexThenDisposeReleasesIt()
    {
        uint index = Direct3D9ResourceCacheIndexManager.AcquireIndex();
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = new(
            deviceObject.Device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            realizationCacheIndex: index,
            ownsRealizationCacheIndex: true);

        device.Dispose();

        uint reacquiredIndex = Direct3D9ResourceCacheIndexManager.AcquireIndex();
        try
        {
            Assert.AreEqual(index, reacquiredIndex);
        }
        finally
        {
            Direct3D9ResourceCacheIndexManager.ReleaseIndex(reacquiredIndex);
        }
    }

    [TestMethod]
    public void WhenBaseDeviceIdentityIsQueriedThenInitializationCachesBaseValues()
    {
        Caps9 capabilities = new()
        {
            DeviceType = Devtype.Hal,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        device.UpdateTier(2 << 16);

        Assert.AreEqual(
            (false, Pool.Managed, false, Pool.Managed, false),
            (device.IsExtended, device.ManagedPool, device.IsExtended, device.ManagedPool, device.IsEntered()));
    }

    [TestMethod]
    [DataRow(Devtype.Hal)]
    [DataRow(Devtype.Ref)]
    [DataRow(Devtype.SW)]
    public void WhenExtendedDeviceIdentityIsQueriedThenInitializationCachesExtendedValues(Devtype deviceType)
    {
        Caps9 capabilities = new()
        {
            DeviceType = deviceType,
            Caps = uint.MaxValue,
            Caps2 = uint.MaxValue,
            Caps3 = uint.MaxValue,
            DevCaps = uint.MaxValue,
            DevCaps2 = uint.MaxValue,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            (IDirect3DDevice9Ex*) deviceObject.Device,
            capabilities);
        device.UpdateTier(2 << 16);

        Assert.AreEqual(
            (true, (Pool) 6, true, (Pool) 6, false),
            (device.IsExtended, device.ManagedPool, device.IsExtended, device.ManagedPool, device.IsEntered()));
    }

    [TestMethod]
    public void WhenTierIsQueriedBeforeAnyUpdateThenTierZeroIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);

        Assert.AreEqual(0, device.Tier);
    }

    [TestMethod]
    [DataRow(int.MinValue)]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow((1 << 16) - 1)]
    [DataRow(1 << 16)]
    [DataRow((2 << 16) - 1)]
    [DataRow(2 << 16)]
    [DataRow(int.MaxValue)]
    public void WhenTierIsUpdatedThenRawCachedValueIsReturned(int tier)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);

        device.UpdateTier(tier);

        Assert.AreEqual(tier, device.Tier);
    }

    [TestMethod]
    public void WhenTierIsUpdatedRepeatedlyThenLastValueWins()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateTier(1 << 16);
        device.UpdateTier(int.MinValue);
        device.UpdateTier(2 << 16);

        Assert.AreEqual(2 << 16, device.Tier);
    }

    [TestMethod]
    [DataRow(Devtype.Hal, false, false, false)]
    [DataRow(Devtype.Hal, true, true, true)]
    [DataRow(Devtype.Ref, false, true, false)]
    [DataRow(Devtype.Ref, true, false, true)]
    [DataRow(Devtype.SW, false, false, true)]
    [DataRow(Devtype.SW, true, true, false)]
    public void WhenTierIsQueriedThenDeviceIdentityAndCapabilitiesDoNotInterfere(
        Devtype deviceType,
        bool isLddmDevice,
        bool isExtendedDevice,
        bool isPureDevice)
    {
        Caps9 capabilities = new()
        {
            DeviceType = deviceType,
            Caps = uint.MaxValue,
            Caps2 = isLddmDevice ? uint.MaxValue : 0,
            Caps3 = uint.MaxValue,
            DevCaps = uint.MaxValue,
            DevCaps2 = uint.MaxValue,
            PrimitiveMiscCaps = uint.MaxValue,
            RasterCaps = uint.MaxValue,
            SrcBlendCaps = uint.MaxValue,
            TextureCaps = uint.MaxValue,
            TextureFilterCaps = uint.MaxValue,
            TextureAddressCaps = uint.MaxValue,
            MaxAnisotropy = uint.MaxValue,
            PixelShaderVersion = uint.MaxValue,
            VertexShaderVersion = uint.MaxValue
        };
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            isExtendedDevice ? (IDirect3DDevice9Ex*) deviceObject.Device : null,
            uint.MaxValue,
            deviceType,
            isPureDevice ? D3D9.CreatePuredevice : 0u,
            default,
            capabilities: capabilities,
            adapterLuid: long.MinValue);
        device.UpdateTier(-123456789);

        Assert.AreEqual(-123456789, device.Tier);
    }

    [TestMethod]
    public void WhenTierIsReadRepeatedlyThenNoDeviceEntryOrComCallOccurs()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateTier(2 << 16);

        (int First, int Second, bool IsEntered, uint ReleaseCallCount) result =
            (device.Tier, device.Tier, device.IsEntered(), deviceObject.ReleaseCallCount);

        Assert.AreEqual((2 << 16, 2 << 16, false, 0u), result);
    }

    [TestMethod]
    public void WhenTierIsQueriedThenMultisampleFailureCacheIsUnchanged()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.UpdateTier(2 << 16);
        device.SetMultisampleFailed();

        _ = device.Tier;

        Assert.IsFalse(device.ShouldAttemptMultisample);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenCapabilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = (device.IsExtended, device.IsPureDevice, device.Tier, device.ManagedPool, device.RealizationCacheIndex, device.IsTextPixelShaderInitializationEligible));
    }

    [TestMethod]
    public void WhenPureDeviceIsReleasedThenPureIdentityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = new(
            deviceObject.Device,
            null,
            0,
            Devtype.Hal,
            D3D9.CreatePuredevice,
            default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.IsPureDevice);
    }

    [TestMethod]
    public void WhenRegularDeviceIsReleasedThenPureIdentityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.IsPureDevice);
    }

    [TestMethod]
    public void WhenBaseDeviceIsReleasedThenExtendedIdentityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.IsExtended);
    }

    [TestMethod]
    public void WhenBaseDeviceIsReleasedThenManagedPoolAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.ManagedPool);
    }

    [TestMethod]
    public void WhenExtendedDeviceIsReleasedThenExtendedIdentityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            (IDirect3DDevice9Ex*) deviceObject.Device,
            default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.IsExtended);
    }

    [TestMethod]
    public void WhenExtendedDeviceIsReleasedThenManagedPoolAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            (IDirect3DDevice9Ex*) deviceObject.Device,
            default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.ManagedPool);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenAutoGenerateMipmapCapabilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.CanAutoGenerateMipmaps);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenStretchRectMipmapCapabilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.CanGenerateMipmapsWithStretchRect);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenStretchRectFromTexturesCapabilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.CanStretchRectFromTextures);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenConditionalNonPowerOfTwoCapabilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsConditionalNonPowerOfTwoTextures);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenUnconditionalNonPowerOfTwoCapabilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsUnconditionalNonPowerOfTwoTextures);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenTextureCapabilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SupportsTextureCapability((uint) D3D9.PtexturecapsPow2));
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenMaximumDesiredAnisotropicFilterLevelAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.MaximumDesiredAnisotropicFilterLevel);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenAnisotropicFilteringSupportAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsAnisotropicFiltering);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenSupportedAnisotropicFilterModeAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportedAnisotropicFilterMode);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenScissorRectangleSupportAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsScissorRectangle);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenLinearToSrgbPresentationSupportAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsLinearToSrgbPresentation);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenA8TextureFormatSupportAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsA8TextureFormat);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenP8TextureFormatSupportAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsP8TextureFormat);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenL8TextureFormatSupportAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsL8TextureFormat);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenColorChannelMaskCapabilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.CanMaskColorChannels);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenBlendFactorCapabilityAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.CanHandleBlendFactor);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenBorderColorSupportAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsBorderColor);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenMaximumStreamsAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.MaximumStreams);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenMaximumTextureBlendStagesAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.MaximumTextureBlendStages);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenMaximumSimultaneousTexturesAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.MaximumSimultaneousTextures);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenMaximumTextureWidthAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.MaximumTextureWidth);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenMaximumTextureHeightAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.MaximumTextureHeight);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenVertexShaderVersionAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.VertexShaderVersion);
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenPixelShaderVersionAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.PixelShaderVersion);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device, Caps9 capabilities)
    {
        return CreateDevice(device, null, capabilities);
    }

    private static Direct3D9Device CreateDevice(
        IDirect3DDevice9* device,
        IDirect3DDevice9Ex* deviceEx,
        Caps9 capabilities)
    {
        return new Direct3D9Device(device, deviceEx, 0, capabilities.DeviceType, 0, default, capabilities: capabilities);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self)
    {
        uint* releaseCallCount = (uint*) self + (sizeof(nint) * 4 / sizeof(uint));
        (*releaseCallCount)++;
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        internal readonly uint ReleaseCallCount => *((uint*) Device + (sizeof(nint) * 4 / sizeof(uint)));

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 5);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
