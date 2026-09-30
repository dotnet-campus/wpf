using System.Runtime.InteropServices;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe partial class Direct3D9SoftwareImageRenderer
{
    private SoftwareImageCoverage? _coverage;
    private GeneratedImageTransform _deviceToSource;
    private byte[] _affinePixels = [];

    internal int DrawTransformed(nint source, MilRectD rectangle, SoftwareImageDrawingContext context, bool isMilSource)
    {
        if (_realizeBitmap is not null) return Direct3D9Factory.WgxInvalidCallHResult;
        if (rectangle.Width <= 0 || rectangle.Height <= 0) return 0;
        if (context.SourceCoverage is { } sourceCoverage && (sourceCoverage.Width <= 0 || sourceCoverage.Height <= 0)) return 0;
        _context = context;
        _realizeBitmap = () => RealizeTransformed(source, rectangle, context.WorldToDevice, isMilSource);
        try
        {
            return _surface.DrawPath(context.AliasedClip ?? new(0, 0, checked((int)_width), checked((int)_height)),
                hasFillBrush: true, hasPen: false, hasStrokeBrush: false);
        }
        finally { _realizeBitmap = null; _drawClip = default; _context = default; }
    }

    private int RealizeTransformed(nint source, MilRectD rectangle, GeneratedImageTransform transform, bool isMilSource)
    {
        if (_floating) return RealizeFloat(source, rectangle, transform, isMilSource);
        MilRectD bounds = transform.Apply(rectangle);
        if (_context.SourceCoverage is null && ScalingMode != 3 && transform.IsPositiveAxisAligned && bounds.X == Math.Truncate(bounds.X)
            && bounds.Y == Math.Truncate(bounds.Y) && bounds.Width == Math.Truncate(bounds.Width)
            && bounds.Height == Math.Truncate(bounds.Height))
            return Draw(source, bounds, 0, 0, isMilSource);
        if (!SoftwareImageCoverage.TryCreate(_context.SourceCoverage ?? rectangle, transform, out var coverage))
            return Direct3D9Factory.NotImplementedHResult;
        if (!transform.TryInvert(out _)) return 0;
        uint width, height;
        int result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)source)[3])(source, &width, &height);
        if (result < 0) return result;
        if (width == 0 || height == 0) return 0;
        nint wic = 0, scaler = 0, converter = 0;
        try
        {
            nint input = source;
            if (isMilSource)
            {
                result = MilBitmapSourceAdapter.Create(source, out wic);
                if (result < 0) return result;
                input = wic;
            }
            double sx = rectangle.Width / width, sy = rectangle.Height / height;
            uint filteredWidth = PrefilterSize(width, (float)(sx * Math.Sqrt(transform.ScaleX * transform.ScaleX + transform.M12 * transform.M12)));
            uint filteredHeight = PrefilterSize(height, (float)(sy * Math.Sqrt(transform.M21 * transform.M21 + transform.ScaleY * transform.ScaleY)));
            if ((_context.PrefilterEnabled ?? (ScalingMode is not (1 or 3))) && (filteredWidth != width || filteredHeight != height))
            {
                result = BitmapFormatConverter.CreateScaler(input, filteredWidth, filteredHeight, out scaler);
                if (result < 0) return result;
                input = scaler; width = filteredWidth; height = filteredHeight;
            }
            Guid format;
            result = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)input)[4])(input, &format);
            if (result < 0) return result;
            _opaque = format == new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90e");
            if (!_opaque && format != new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910"))
            {
                result = BitmapFormatConverter.Create(input, new("6fddc324-4e03-4bfe-b185-3d77768dc910"), out converter);
                if (result < 0) return result;
                input = converter;
            }
            var sourceToDevice = transform.Prepend(new(rectangle.Width / width, rectangle.Height / height, rectangle.X, rectangle.Y));
            if (!sourceToDevice.TryInvert(out _deviceToSource)) return 0;
            _affinePixels = new byte[checked((int)width * (int)height * 4)];
            fixed (byte* pixels = _affinePixels)
                result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)input)[7])(input, 0, checked(width * 4), (uint)_affinePixels.Length, pixels);
            if (result < 0) return result;
            _sourceWidth = width; _sourceHeight = height; _source = input; _coverage = coverage;
            return FillBitmapPath();
        }
        finally
        {
            _coverage = null; _affinePixels = []; _source = 0;
            Direct3D9Factory.Release(converter); Direct3D9Factory.Release(scaler); Direct3D9Factory.Release(wic);
        }
    }

    private int DrawAffineLocked(Direct3D9SurfaceRect clip)
    {
        int result = _surface.SetupPipeline(MilPixelFormat.Pbgra32Bpp, _source, _context.Antialias, false, CompositingMode, (uint)(clip.Right - clip.Left));
        if (result < 0) return result;
        for (int y = clip.Top; y < clip.Bottom; y++)
        {
            result = _surface.OutputSpan(y, clip.Left, clip.Right);
            if (result < 0) return result;
        }
        return 0;
    }

    private void BlendAffineSpan(byte[] destination, int offset, uint count, int x, int y)
    {
        for (uint i = 0; i < count; i++, offset += 4)
        {
            int coverage = _context.Antialias ? _coverage!.GetCoverage(x + (int)i, y)
                : _coverage!.GetAliasedCoverage(x + (int)i, y);
            if (coverage == 0) continue;
            var point = _deviceToSource.Apply(x + i + 0.5, y + 0.5);
            long u = (long)Math.Round(Math.Clamp(point.X - 0.5, -1, _sourceWidth) * 65536);
            long v = (long)Math.Round(Math.Clamp(point.Y - 0.5, -1, _sourceHeight) * 65536);
            var texels = Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(u, v, _sourceWidth, _sourceHeight, MilBitmapWrapMode.Extend);
            uint sample = Direct3D9SoftwareBilinearSpan.InterpolateBilinearArgb(
                ReadAffinePixel(texels.X1, texels.Y1), ReadAffinePixel(texels.X2, texels.Y1),
                ReadAffinePixel(texels.X1, texels.Y2), ReadAffinePixel(texels.X2, texels.Y2), texels.XFraction, texels.YFraction);
            if (ScalingMode == 3)
                sample = ReadAffinePixel((int)Math.Clamp(Math.Floor(point.X), 0, _sourceWidth - 1),
                    (int)Math.Clamp(Math.Floor(point.Y), 0, _sourceHeight - 1));
            if (_context.Effects is { } effects) sample = effects.Apply(sample, x + (int)i, y);
            int alpha = ((byte)(sample >> 24) * coverage + 32) >> 6;
            for (int component = 0; component < 4; component++)
            {
                int value = ((byte)(sample >> (component * 8)) * coverage + 32) >> 6;
                destination[offset + component] = CompositingMode == MilCompositingMode.SourceCopy
                    ? (byte)value
                    : (byte)Math.Min(255, value + (destination[offset + component] * (255 - alpha) + 127) / 255);
            }
        }
    }

    private uint ReadAffinePixel(int x, int y)
    {
        uint value = MemoryMarshal.Read<uint>(_affinePixels.AsSpan(checked(((int)_sourceWidth * y + x) * 4), 4));
        return _opaque ? value | 0xff000000 : value;
    }
}
