using System.Numerics;
using System.Runtime.InteropServices;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe partial class Direct3D9SoftwareImageRenderer
{
    private Vector4[] _floatSource = [];

    private int RealizeFloat(nint source, MilRectD rectangle, GeneratedImageTransform transform, bool isMilSource)
    {
        if (!TryCreateDrawCoverage(rectangle, transform, out var coverage))
            return Direct3D9Factory.NotImplementedHResult;
        if (!transform.TryInvert(out _)) return 0;
        nint wic = 0, scaler = 0, converter = 0;
        try
        {
            nint input = source;
            int hr;
            if (isMilSource)
            {
                hr = MilBitmapSourceAdapter.Create(source, out wic);
                if (hr < 0) return hr;
                input = wic;
            }
            uint width, height;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)input)[3])(input, &width, &height);
            if (hr < 0) return hr;
            if (width == 0 || height == 0) return 0;
            if (_context.PrefilterEnabled ?? (ScalingMode is not (1 or 3)))
            {
                uint fw = PrefilterSize(width, (float)(rectangle.Width / width * Math.Sqrt(transform.ScaleX * transform.ScaleX + transform.M12 * transform.M12)));
                uint fh = PrefilterSize(height, (float)(rectangle.Height / height * Math.Sqrt(transform.ScaleY * transform.ScaleY + transform.M21 * transform.M21)));
                if (fw != width || fh != height)
                {
                    hr = BitmapFormatConverter.CreateScaler(input, fw, fh, out scaler);
                    if (hr < 0) return hr;
                    input = scaler; width = fw; height = fh;
                }
            }
            Guid format;
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)input)[4])(input, &format);
            if (hr < 0) return hr;
            Guid floatingFormat = new("6fddc324-4e03-4bfe-b185-3d77768dc91a");
            if (format != floatingFormat)
            {
                hr = BitmapFormatConverter.Create(input, floatingFormat, out converter);
                if (hr < 0) return hr;
                input = converter;
            }
            if (!transform.Prepend(new(rectangle.Width / width, rectangle.Height / height, rectangle.X, rectangle.Y)).TryInvert(out _deviceToSource)) return 0;
            _floatSource = new Vector4[checked((int)width * (int)height)];
            fixed (Vector4* pixels = _floatSource)
                hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)input)[7])(input, 0, checked(width * 16), checked((uint)_floatSource.Length * 16), (byte*)pixels);
            if (hr < 0) return hr;
            _sourceWidth = width; _sourceHeight = height; _coverage = coverage;
            hr = _surface.SetupPipeline(MilPixelFormat.Prgba128BppFloat, input, _context.Antialias, false, CompositingMode, (uint)(_drawClip.Right - _drawClip.Left));
            if (hr < 0) return hr;
            for (int y = _drawClip.Top; y < _drawClip.Bottom; y++)
            {
                hr = _surface.OutputSpan(y, _drawClip.Left, _drawClip.Right);
                if (hr < 0) return hr;
            }
            return 0;
        }
        finally
        {
            _floatSource = []; _coverage = null;
            Direct3D9Factory.Release(converter); Direct3D9Factory.Release(scaler); Direct3D9Factory.Release(wic);
        }
    }

    private void BlendFloatSpan(byte[] destination, int offset, uint count, int x, int y)
    {
        // scRGB spans advance single-precision IPC coordinates, including covered-out pixels.
        float m11 = (float)_deviceToSource.ScaleX, m12 = (float)_deviceToSource.M12;
        float m21 = (float)_deviceToSource.M21, m22 = (float)_deviceToSource.ScaleY;
        float dx = (float)_deviceToSource.X + 0.5f * (m11 + m21) - 0.5f;
        float dy = (float)_deviceToSource.Y + 0.5f * (m12 + m22) - 0.5f;
        float u0 = x * m11 + y * m21 + dx, v0 = x * m12 + y * m22 + dy;
        float uEnd = (x + count) * m11 + y * m21 + dx;
        float vEnd = (x + count) * m12 + y * m22 + dy;
        float du = (uEnd - u0) / count, dv = (vEnd - v0) / count;
        for (uint i = 0; i < count; i++, offset += 16, u0 += du, v0 += dv)
        {
            int coverage = _context.Antialias ? _coverage!.GetCoverage(x + (int)i, y) : _coverage!.GetAliasedCoverage(x + (int)i, y);
            if (_context.GeometryMask is { } mask)
                coverage = (coverage * (_context.Antialias ? mask.GetCoverage(x + (int)i, y)
                    : mask.GetAliasedCoverage(x + (int)i, y)) + 32) >> 6;
            if (coverage == 0) continue;
            Vector4 sample;
            if (ScalingMode == 3)
                sample = WrappedFloatPixel(Math.Floor((double)u0 + 0.5), Math.Floor((double)v0 + 0.5));
            else
            {
                double ix = Math.Floor(u0), iy = Math.Floor(v0);
                float horizontal = u0 - (float)ix, vertical = v0 - (float)iy;
                var topLeft = WrappedFloatPixel(ix, iy);
                var bottomLeft = WrappedFloatPixel(ix, iy + 1);
                // scRGB wraps individual integer texels, not fixed-point byte-span coordinates.
                var top = topLeft + (WrappedFloatPixel(ix + 1, iy) - topLeft) * horizontal;
                var bottom = bottomLeft + (WrappedFloatPixel(ix + 1, iy + 1) - bottomLeft) * horizontal;
                sample = top == bottom ? top : (1 - vertical) * top + vertical * bottom;
            }
            sample *= _context.AlphaScale;
            if (_context.Effects is { } effects) sample = effects.Apply(sample, x + (int)i, y);
            sample *= coverage / 64f;
            if (_context.GlyphAlpha is { } glyph) sample *= glyph.Sample(x + (int)i, y);
            var span = destination.AsSpan(offset, 16);
            if (CompositingMode == MilCompositingMode.SourceOver)
                sample += MemoryMarshal.Read<Vector4>(span) * (1 - sample.W);
            MemoryMarshal.Write(span, in sample);
        }
    }

    private static Vector4 PbgraToFloat(uint pixel)
    {
        float alpha = (byte)(pixel >> 24) / 255f;
        if (alpha == 0) return Vector4.Zero;
        float Channel(int shift)
        {
            float value = (byte)(pixel >> shift) / (255f * alpha);
            return (value <= 0.04045f ? value / 12.92f : MathF.Pow((value + 0.055f) / 1.055f, 2.4f)) * alpha;
        }
        return new(Channel(16), Channel(8), Channel(0), alpha);
    }

    private Vector4 WrappedFloatPixel(double x, double y)
    {
        static int Wrap(double coordinate, uint size, bool flip)
        {
            double period = flip ? 2d * size : size;
            double remainder = coordinate % period;
            if (remainder < 0) remainder += period;
            return (int)(flip && remainder >= size ? period - 1 - remainder : remainder);
        }

        if (_context.WrapMode == MilBitmapWrapMode.Border)
            return x >= 0 && y >= 0 && x < _sourceWidth && y < _sourceHeight
                ? FloatPixel((int)x, (int)y) : FloatPixel(-1, -1);
        if (_context.WrapMode == MilBitmapWrapMode.Extend)
            return FloatPixel((int)Math.Clamp(x, 0, _sourceWidth - 1), (int)Math.Clamp(y, 0, _sourceHeight - 1));
        return FloatPixel(Wrap(x, _sourceWidth, _context.WrapMode is MilBitmapWrapMode.FlipX or MilBitmapWrapMode.FlipXY),
            Wrap(y, _sourceHeight, _context.WrapMode is MilBitmapWrapMode.FlipY or MilBitmapWrapMode.FlipXY));
    }

    private Vector4 FloatPixel(int x, int y)
    {
        if ((uint)x < _sourceWidth && (uint)y < _sourceHeight)
            return _floatSource[checked(y * (int)_sourceWidth + x)];
        var color = _context.BorderColor;
        return new Vector4(color.Red * color.Alpha, color.Green * color.Alpha, color.Blue * color.Alpha, color.Alpha);
    }

}
