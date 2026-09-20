using System.Numerics;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[Flags]
internal enum Direct3D9MinimalTextureDescriptionFlags : uint
{
    CheckAll = 0,
    IgnoreWidth = 0x1,
    IgnoreHeight = 0x2,
    IgnoreFormat = 0x4,
    CheckWidth = IgnoreHeight | IgnoreFormat,
    CheckHeight = IgnoreWidth | IgnoreFormat,
    CheckFormat = IgnoreWidth | IgnoreHeight,
    NonPowerOfTwoConditionalAllowed = 0x10
}

internal static unsafe class Direct3D9TextureDescription
{
    internal static int GetMinimal(
        IDirect3DDevice9* device,
        Format adapterFormat,
        Caps9 capabilities,
        ref SurfaceDesc description,
        bool paletteUsesAlpha,
        Direct3D9MinimalTextureDescriptionFlags flags)
    {
        int result = 0;
        Direct3D9MinimalTextureDescriptionFlags ignoredDimensions =
            Direct3D9MinimalTextureDescriptionFlags.IgnoreWidth |
            Direct3D9MinimalTextureDescriptionFlags.IgnoreHeight;

        if ((flags & ignoredDimensions) != ignoredDimensions)
        {
            if ((flags & Direct3D9MinimalTextureDescriptionFlags.IgnoreWidth) == 0)
            {
                result = AdjustDimension(
                    ref description.Width,
                    capabilities.MaxTextureWidth,
                    capabilities.TextureCaps,
                    flags,
                    result);
            }

            if ((flags & Direct3D9MinimalTextureDescriptionFlags.IgnoreHeight) == 0)
            {
                result = AdjustDimension(
                    ref description.Height,
                    capabilities.MaxTextureHeight,
                    capabilities.TextureCaps,
                    flags,
                    result);
            }
        }

        if ((flags & Direct3D9MinimalTextureDescriptionFlags.IgnoreFormat) != 0)
        {
            return result;
        }

        int dimensionResult = result;
        IDirect3D9* direct3D = null;
        void** deviceVtable = device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3D9**, int> getDirect3D =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3D9**, int>) deviceVtable[6];
        result = getDirect3D(device, &direct3D);
        if (result < 0)
        {
            return result;
        }

        try
        {
            Format format = description.Format;
            if (format == Format.P8 &&
                paletteUsesAlpha &&
                (capabilities.TextureCaps & (uint) D3D9.PtexturecapsAlphapalette) == 0)
            {
                format = GetSuperiorFormat(format, paletteUsesAlpha);
            }

            void** direct3DVtable = direct3D->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, uint, Resourcetype, Format, int>
                checkDeviceFormat =
                    (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, uint, Resourcetype, Format, int>)
                    direct3DVtable[10];

            do
            {
                result = checkDeviceFormat(
                    direct3D,
                    capabilities.AdapterOrdinal,
                    capabilities.DeviceType,
                    adapterFormat,
                    description.Usage,
                    description.Type,
                    format);
                if (result >= 0)
                {
                    description.Format = format;
                    return dimensionResult;
                }

                format = GetSuperiorFormat(format, paletteUsesAlpha);
            }
            while (format != Format.Unknown);

            return result;
        }
        finally
        {
            Direct3D9Factory.Release(direct3D);
        }
    }

    private static int AdjustDimension(
        ref uint dimension,
        uint maximumDimension,
        uint textureCapabilities,
        Direct3D9MinimalTextureDescriptionFlags flags,
        int result)
    {
        if (dimension > maximumDimension)
        {
            dimension = maximumDimension;
            return 1;
        }

        if ((textureCapabilities & (uint) D3D9.PtexturecapsPow2) != 0 &&
            (flags & Direct3D9MinimalTextureDescriptionFlags.NonPowerOfTwoConditionalAllowed) == 0)
        {
            dimension = BitOperations.RoundUpToPowerOf2(dimension);
        }

        return result;
    }

    private static Format GetSuperiorFormat(Format format, bool paletteUsesAlpha)
    {
        return format switch
        {
            Format.P8 => paletteUsesAlpha ? Format.A8R8G8B8 : Format.R8G8B8,
            Format.X1R5G5B5 => Format.R5G6B5,
            Format.R5G6B5 => Format.R8G8B8,
            Format.R8G8B8 => Format.X8R8G8B8,
            Format.X8R8G8B8 => Format.A8R8G8B8,
            Format.A8P8 or Format.A1R5G5B5 => Format.A8R8G8B8,
            _ => Format.Unknown
        };
    }
}
