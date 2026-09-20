using System.Runtime.Intrinsics.X86;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9GraphicsAccelerationSnapshot(
    bool IsRecentDriver,
    Direct3D9GraphicsAccelerationCaps Capabilities);

internal static class Direct3D9HardwareCapabilities
{
    private const int Tier1 = 1 << 16;
    private const int Tier2 = 2 << 16;
    private const uint PixelShaderVersion20 = 0xFFFF0200;
    private const uint VertexShaderVersion20 = 0xFFFE0200;
    private const uint Tier1RequiredMemory = 60 * 1024 * 1024;
    private const uint Tier2RequiredMemory = 120 * 1024 * 1024;

    private const uint RequiredSourceBlendCapabilities =
        (uint) (D3D9.PblendcapsZero
            | D3D9.PblendcapsOne
            | D3D9.PblendcapsSrcalpha
            | D3D9.PblendcapsInvdestalpha);

    private const uint RequiredDestinationBlendCapabilities =
        (uint) (D3D9.PblendcapsZero
            | D3D9.PblendcapsOne
            | D3D9.PblendcapsInvsrccolor
            | D3D9.PblendcapsInvsrcalpha);

    internal static int CheckDeviceLevel1(Direct3D9Device device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return CheckDeviceLevel1(device.Capabilities);
    }

    internal static int CheckDeviceLevel1(Caps9 capabilities)
    {
        bool requiresPowerOfTwoTextures =
            (capabilities.TextureCaps & D3D9.PtexturecapsPow2) != 0;
        bool supportsConditionalNonPowerOfTwoTextures =
            (capabilities.TextureCaps & D3D9.PtexturecapsNonpow2Conditional) != 0;
        if (requiresPowerOfTwoTextures && !supportsConditionalNonPowerOfTwoTextures)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        if ((capabilities.TextureCaps & D3D9.PtexturecapsSquareonly) != 0)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        if (capabilities.MaxTextureBlendStages < 2 || capabilities.MaxSimultaneousTextures < 2)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        if (capabilities.DeviceType != Devtype.SW
            && (capabilities.PrimitiveMiscCaps & D3D9.PmisccapsColorwriteenable) == 0)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        if ((capabilities.SrcBlendCaps & RequiredSourceBlendCapabilities) != RequiredSourceBlendCapabilities
            || (capabilities.DestBlendCaps & RequiredDestinationBlendCapabilities) != RequiredDestinationBlendCapabilities)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        return 0;
    }

    internal static bool HasWddmSupport(Caps9 capabilities)
    {
        return (capabilities.Caps2 & D3D9.Caps2Canshareresource) != 0;
    }

    internal static int GetTier(uint memorySize, Caps9 capabilities)
    {
        bool hasWddmSupport = HasWddmSupport(capabilities);
        if ((!hasWddmSupport && memorySize < Tier1RequiredMemory)
            || capabilities.PixelShaderVersion < PixelShaderVersion20
            || CheckDeviceLevel1(capabilities) < 0)
        {
            return 0;
        }

        if ((!hasWddmSupport && memorySize < Tier2RequiredMemory)
            || capabilities.VertexShaderVersion < VertexShaderVersion20
            || capabilities.MaxTextureBlendStages < 4
            || (capabilities.SrcBlendCaps & (uint) D3D9.PblendcapsBlendfactor) == 0)
        {
            return Tier1;
        }

        return Tier2;
    }

    internal static Direct3D9GraphicsAccelerationSnapshot ReadGraphicsAccelerationCaps(
        uint bitsPerPixel,
        uint memorySize,
        bool isRecentDriver,
        bool isBadDriver,
        int checkDisplayFormatHResult,
        int getDeviceCapsHResult,
        Caps9 capabilities)
    {
        if (isBadDriver || checkDisplayFormatHResult < 0)
        {
            return new Direct3D9GraphicsAccelerationSnapshot(
                isRecentDriver,
                CreateNoHardwareAccelerationCaps(bitsPerPixel));
        }

        if (getDeviceCapsHResult < 0)
        {
            return new Direct3D9GraphicsAccelerationSnapshot(
                isRecentDriver,
                CreateNoHardwareAccelerationCaps(bitsPerPixel) with { WindowCompatibleMode = 1 });
        }

        bool hasWddmSupport = HasWddmSupport(capabilities);
        bool hasRecentDriver = hasWddmSupport || isRecentDriver;
        return new Direct3D9GraphicsAccelerationSnapshot(
            hasRecentDriver,
            new Direct3D9GraphicsAccelerationCaps(
                hasRecentDriver ? GetTier(memorySize, capabilities) : 0,
                hasWddmSupport ? 1 : 0,
                capabilities.PixelShaderVersion,
                capabilities.VertexShaderVersion,
                capabilities.MaxTextureWidth,
                capabilities.MaxTextureHeight,
                1,
                bitsPerPixel,
                Sse2.IsSupported ? 1U : 0U,
                capabilities.MaxPixelShader30InstructionSlots));
    }

    internal static Direct3D9GraphicsAccelerationCaps CreateNoHardwareAccelerationCaps(uint bitsPerPixel)
    {
        return new Direct3D9GraphicsAccelerationCaps(0, 0, 0, 0, 0, 0, 0, bitsPerPixel, 0, 0);
    }
}
