using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal enum MilPixelFormat
{
    Undefined = 0,
    Indexed1Bpp = 0x01,
    Indexed2Bpp = 0x02,
    Indexed4Bpp = 0x03,
    Indexed8Bpp = 0x04,
    BlackWhite = 0x05,
    Gray2Bpp = 0x06,
    Gray4Bpp = 0x07,
    Gray8Bpp = 0x08,
    Bgr16Bpp555 = 0x09,
    Bgr16Bpp565 = 0x0A,
    Gray16Bpp = 0x0B,
    Bgr24Bpp = 0x0C,
    Rgb24Bpp = 0x0D,
    Bgr32Bpp = 0x0E,
    Bgra32Bpp = 0x0F,
    Pbgra32Bpp = 0x10,
    Gray32BppFloat = 0x11,
    Rgb48BppFixedPoint = 0x12,
    Gray16BppFixedPoint = 0x13,
    Bgr32Bpp101010 = 0x14,
    Rgb48Bpp = 0x15,
    Rgba64Bpp = 0x16,
    Prgba64Bpp = 0x17,
    Bgr96BppFixedPoint = 0x18,
    Rgba128BppFloat = 0x19,
    Prgba128BppFloat = 0x1A,
    Rgb128BppFloat = 0x1B,
    Cmyk32Bpp = 0x1C,
    Rgba64BppFixedPoint = 0x1D,
    Rgba128BppFixedPoint = 0x1E,
    Cmyk64Bpp = 0x1F,
    CmykAlpha40Bpp = 0x2C,
    CmykAlpha80Bpp = 0x2D
}

internal static class MilPixelFormatInfo
{
    internal static bool HasAlphaChannel(MilPixelFormat format) => format is
        MilPixelFormat.Indexed1Bpp or
        MilPixelFormat.Indexed2Bpp or
        MilPixelFormat.Indexed4Bpp or
        MilPixelFormat.Indexed8Bpp or
        MilPixelFormat.Bgra32Bpp or
        MilPixelFormat.Pbgra32Bpp or
        MilPixelFormat.Rgba64Bpp or
        MilPixelFormat.Prgba64Bpp or
        MilPixelFormat.Rgba128BppFloat or
        MilPixelFormat.Prgba128BppFloat;

    internal static byte GetBitsPerPixel(MilPixelFormat format) => format switch
    {
        MilPixelFormat.Indexed1Bpp or MilPixelFormat.BlackWhite => 1,
        MilPixelFormat.Indexed2Bpp or MilPixelFormat.Gray2Bpp => 2,
        MilPixelFormat.Indexed4Bpp or MilPixelFormat.Gray4Bpp => 4,
        MilPixelFormat.Indexed8Bpp or MilPixelFormat.Gray8Bpp => 8,
        MilPixelFormat.Bgr16Bpp555 or MilPixelFormat.Bgr16Bpp565 or
            MilPixelFormat.Gray16BppFixedPoint or MilPixelFormat.Gray16Bpp => 16,
        MilPixelFormat.Bgr24Bpp or MilPixelFormat.Rgb24Bpp => 24,
        MilPixelFormat.Gray32BppFloat or MilPixelFormat.Bgr32Bpp or
            MilPixelFormat.Bgra32Bpp or MilPixelFormat.Pbgra32Bpp or
            MilPixelFormat.Cmyk32Bpp or MilPixelFormat.Bgr32Bpp101010 => 32,
        MilPixelFormat.CmykAlpha40Bpp => 40,
        MilPixelFormat.Rgb48Bpp or MilPixelFormat.Rgb48BppFixedPoint => 48,
        MilPixelFormat.Rgba64Bpp or MilPixelFormat.Prgba64Bpp or
            MilPixelFormat.Rgba64BppFixedPoint or MilPixelFormat.Cmyk64Bpp => 64,
        MilPixelFormat.CmykAlpha80Bpp => 80,
        MilPixelFormat.Bgr96BppFixedPoint => 96,
        MilPixelFormat.Rgb128BppFloat or MilPixelFormat.Rgba128BppFloat or
            MilPixelFormat.Prgba128BppFloat or MilPixelFormat.Rgba128BppFixedPoint => 128,
        _ => 0
    };

    internal static Format ToDirect3DFormat(MilPixelFormat format) => format switch
    {
        MilPixelFormat.Bgr24Bpp => Format.R8G8B8,
        MilPixelFormat.Pbgra32Bpp or MilPixelFormat.Bgra32Bpp => Format.A8R8G8B8,
        MilPixelFormat.Bgr32Bpp => Format.X8R8G8B8,
        MilPixelFormat.Bgr16Bpp565 => Format.R5G6B5,
        MilPixelFormat.Bgr16Bpp555 => Format.X1R5G5B5,
        MilPixelFormat.Indexed8Bpp => Format.P8,
        MilPixelFormat.Gray8Bpp => Format.L8,
        MilPixelFormat.Bgr32Bpp101010 => Format.A2R10G10B10,
        MilPixelFormat.Rgba128BppFloat or MilPixelFormat.Prgba128BppFloat => Format.A32B32G32R32f,
        _ => Format.Unknown
    };
}

internal enum Direct3D9GlyphAlphaTextureFormat
{
    Undefined,
    A8,
    L8,
    P8
}

internal readonly record struct Direct3D9TextureFormatSupport(
    bool SupportsA8,
    bool SupportsP8,
    bool SupportsL8,
    MilPixelFormat SupportFor128BppPrgbaFloat,
    MilPixelFormat SupportFor128BppRgbFloat,
    MilPixelFormat SupportFor32BppBgr101010,
    MilPixelFormat SupportFor32BppPbgra,
    MilPixelFormat SupportFor32BppBgr);

internal static class Direct3D9TextureFormatSupportFactory
{
    internal static Direct3D9TextureFormatSupport Gather(
        uint adapterOrdinal,
        Devtype deviceType,
        Format displayFormat,
        Func<uint, Devtype, Format, uint, Resourcetype, Format, int> checkDeviceFormat)
    {
        ArgumentNullException.ThrowIfNull(checkDeviceFormat);

        bool supportsA8 = IsSupported(Format.A8);
        bool supportsP8 = IsSupported(Format.P8);
        bool supportsL8 = IsSupported(Format.L8);

        MilPixelFormat supportFor128BppPrgbaFloat = IsSupported(Format.A32B32G32R32f)
            ? MilPixelFormat.Prgba128BppFloat
            : MilPixelFormat.Undefined;
        MilPixelFormat supportFor128BppRgbFloat = supportFor128BppPrgbaFloat;
        MilPixelFormat supportFor32BppBgr101010 = IsSupported(Format.A2R10G10B10)
            ? MilPixelFormat.Bgr32Bpp101010
            : supportFor128BppRgbFloat;
        MilPixelFormat supportFor32BppPbgra = IsSupported(Format.A8R8G8B8)
            ? MilPixelFormat.Pbgra32Bpp
            : supportFor128BppPrgbaFloat;
        MilPixelFormat supportFor32BppBgr = IsSupported(Format.X8R8G8B8)
            ? MilPixelFormat.Bgr32Bpp
            : supportFor32BppPbgra != MilPixelFormat.Undefined
                ? supportFor32BppPbgra
                : supportFor32BppBgr101010;

        return new Direct3D9TextureFormatSupport(
            supportsA8,
            supportsP8,
            supportsL8,
            supportFor128BppPrgbaFloat,
            supportFor128BppRgbFloat,
            supportFor32BppBgr101010,
            supportFor32BppPbgra,
            supportFor32BppBgr);

        bool IsSupported(Format candidateFormat)
        {
            return checkDeviceFormat(
                adapterOrdinal,
                deviceType,
                displayFormat,
                0,
                Resourcetype.Texture,
                candidateFormat) >= 0;
        }
    }
}
