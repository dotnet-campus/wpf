using System.Numerics;
using System.Runtime.InteropServices;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe partial class Direct3D9SoftwareImageRenderer
{
    private Vector4[] _floatSource = [];

    private int RealizeFloat(nint source, MilRectD rectangle, GeneratedImageTransform transform, bool isMilSource)
    {
        if (!SoftwareImageCoverage.TryCreate(_context.SourceCoverage ?? rectangle, transform, out var coverage))
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
        for (uint i = 0; i < count; i++, offset += 16)
        {
            int coverage = _context.Antialias ? _coverage!.GetCoverage(x + (int)i, y) : _coverage!.GetAliasedCoverage(x + (int)i, y);
            if (coverage == 0) continue;
            var point = _deviceToSource.Apply(x + i + 0.5, y + 0.5);
            Vector4 sample;
            if (ScalingMode == 3)
                sample = FloatPixel((int)Math.Clamp(Math.Floor(point.X), 0, _sourceWidth - 1), (int)Math.Clamp(Math.Floor(point.Y), 0, _sourceHeight - 1));
            else
            {
                double u = Math.Clamp(point.X - 0.5, 0, _sourceWidth - 1), v = Math.Clamp(point.Y - 0.5, 0, _sourceHeight - 1);
                int left = (int)Math.Floor(u), top = (int)Math.Floor(v);
                int right = Math.Min(left + 1, (int)_sourceWidth - 1), bottom = Math.Min(top + 1, (int)_sourceHeight - 1);
                sample = Vector4.Lerp(Vector4.Lerp(FloatPixel(left, top), FloatPixel(right, top), (float)(u - left)),
                    Vector4.Lerp(FloatPixel(left, bottom), FloatPixel(right, bottom), (float)(u - left)), (float)(v - top));
            }
            if (_context.Effects is { } effects) sample = effects.Apply(sample, x + (int)i, y);
            sample *= coverage / 64f;
            var span = destination.AsSpan(offset, 16);
            if (CompositingMode == MilCompositingMode.SourceOver)
                sample += MemoryMarshal.Read<Vector4>(span) * (1 - sample.W);
            MemoryMarshal.Write(span, in sample);
        }
    }

    private Vector4 FloatPixel(int x, int y) => _floatSource[checked(y * (int)_sourceWidth + x)];

    private int CompositeFloatLayer(byte[] source, byte[] destination, SoftwareBitmapEffects effects, SoftwareImageCoverage? mask)
    {
        var input = MemoryMarshal.Cast<byte, Vector4>(source);
        var output = MemoryMarshal.Cast<byte, Vector4>(destination.AsSpan());
        for (int i = 0; i < input.Length; i++)
        {
            int x = i % (int)_width, y = i / (int)_width;
            Vector4 sample = effects.Apply(input[i], x, y);
            if (mask is not null) sample *= mask.GetCoverage(x, y) / 64f;
            output[i] = sample + output[i] * (1 - sample.W);
        }
        return 0;
    }
}
