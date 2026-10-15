using System.Numerics;
using System.Runtime.InteropServices;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe class SoftwareBitmapMask : IDisposable
{
    private nint _source;
    private readonly GeneratedImageTransform _maskToEffect;
    private GeneratedImageTransform _deviceToMask;
    private byte[] _pixels = [];
    private uint _width, _height;
    private bool _floating, _nearest;
    private long _m11, _m12, _m21, _m22, _dx, _dy;
    private float[] _spanAlpha = [];
    private int _spanX, _spanY, _spanCount;

    internal SoftwareBitmapMask(nint ownedSource, GeneratedImageTransform maskToEffect)
    {
        _source = ownedSource;
        _maskToEffect = maskToEffect;
    }

    internal int Prepare(SoftwareImageDrawingContext context, bool floating)
    {
        _pixels = [];
        _spanCount = 0;
        if (_source == 0) return Direct3D9Factory.WgxInvalidCallHResult;
        GeneratedImageTransform transform = (context.EffectToDevice ?? context.WorldToDevice).Prepend(_maskToEffect);
        if (!transform.TryInvert(out _)) return Direct3D9Factory.NotImplementedHResult;
        nint wic = 0, scaler = 0, converter = 0;
        try
        {
            int hr = MilBitmapSourceAdapter.Create(_source, out wic);
            if (hr < 0) return hr;
            nint input = wic;
            uint width, height;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)input)[3])(input, &width, &height);
            if (hr < 0) return hr;
            if (width == 0 || height == 0) return Direct3D9Factory.InvalidArgumentHResult;
            uint fw = width, fh = height;
            if (context.PrefilterEnabled ?? (context.BitmapScalingMode is not (1 or 3)))
            {
                float threshold = context.PrefilterThreshold ?? MathF.Sqrt(2);
                fw = Direct3D9SoftwareImageRenderer.ComputePrefilterSize(width, (float)Math.Sqrt(transform.ScaleX * transform.ScaleX + transform.M12 * transform.M12), threshold);
                fh = Direct3D9SoftwareImageRenderer.ComputePrefilterSize(height, (float)Math.Sqrt(transform.ScaleY * transform.ScaleY + transform.M21 * transform.M21), threshold);
                if (fw != width || fh != height)
                {
                    hr = BitmapFormatConverter.CreateScaler(input, fw, fh, out scaler);
                    if (hr < 0) return hr;
                    input = scaler;
                }
            }
            transform = transform.Prepend(new((double)width / fw, (double)height / fh, 0, 0));
            if (!transform.TryInvert(out _deviceToMask)) return Direct3D9Factory.NotImplementedHResult;
            Guid format;
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)input)[4])(input, &format);
            if (hr < 0) return hr;
            // Byte working sources retain the integer alpha sampler even on FLOAT storage.
            floating &= format != new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910");
            Guid desired = floating ? new("6fddc324-4e03-4bfe-b185-3d77768dc91a") : new("6fddc324-4e03-4bfe-b185-3d77768dc910");
            if (format != desired)
            {
                hr = BitmapFormatConverter.Create(input, desired, out converter);
                if (hr < 0) return hr;
                input = converter;
            }
            uint stride = checked(fw * (floating ? 16u : 4u));
            var pixels = new byte[checked((int)((ulong)stride * fh))];
            fixed (byte* data = pixels)
                hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)input)[7])(input, 0, stride, (uint)pixels.Length, data);
            if (hr < 0) return hr;
            _width = fw; _height = fh; _floating = floating; _nearest = context.BitmapScalingMode == 3;
            if (!floating)
            {
                float m11 = (float)_deviceToMask.ScaleX, m12 = (float)_deviceToMask.M12;
                float m21 = (float)_deviceToMask.M21, m22 = (float)_deviceToMask.ScaleY;
                _m11 = (long)MathF.Round(m11 * 65536); _m12 = (long)MathF.Round(m12 * 65536);
                _m21 = (long)MathF.Round(m21 * 65536); _m22 = (long)MathF.Round(m22 * 65536);
                _dx = (long)MathF.Round(((float)_deviceToMask.X + (m11 + m21) * 0.5f - 0.5f) * 65536);
                _dy = (long)MathF.Round(((float)_deviceToMask.Y + (m12 + m22) * 0.5f - 0.5f) * 65536);
            }
            _pixels = pixels;
            return 0;
        }
        finally { Direct3D9Factory.Release(converter); Direct3D9Factory.Release(scaler); Direct3D9Factory.Release(wic); }
    }

    internal void GenerateSpan(int x, int y, uint count)
    {
        _spanCount = 0;
        if (!_floating) return;
        int length = checked((int)count);
        if (_spanAlpha.Length < length) _spanAlpha = new float[length];
        float m11 = (float)_deviceToMask.ScaleX, m12 = (float)_deviceToMask.M12;
        float m21 = (float)_deviceToMask.M21, m22 = (float)_deviceToMask.ScaleY;
        float dx = (float)_deviceToMask.X + (m11 + m21) * 0.5f - 0.5f;
        float dy = (float)_deviceToMask.Y + (m12 + m22) * 0.5f - 0.5f;
        float u = x * m11 + y * m21 + dx, v = x * m12 + y * m22 + dy;
        float du = (((x + count) * m11 + y * m21 + dx) - u) / count;
        float dv = (((x + count) * m12 + y * m22 + dy) - v) / count;
        for (int i = 0; i < length; i++, u += du, v += dv)
        {
            if (_nearest)
                _spanAlpha[i] = ReadAlpha((int)Math.Clamp(Math.Floor((double)u + 0.5), 0, _width - 1),
                    (int)Math.Clamp(Math.Floor((double)v + 0.5), 0, _height - 1));
            else
            {
                double ix = Math.Floor(u), iy = Math.Floor(v);
                float horizontal = u - (float)ix, vertical = v - (float)iy;
                int left = (int)Math.Clamp(ix, 0, _width - 1), right = (int)Math.Clamp(ix + 1, 0, _width - 1);
                int top = (int)Math.Clamp(iy, 0, _height - 1), bottom = (int)Math.Clamp(iy + 1, 0, _height - 1);
                float a = ReadAlpha(left, top), c = ReadAlpha(left, bottom);
                float upper = a + (ReadAlpha(right, top) - a) * horizontal;
                float lower = c + (ReadAlpha(right, bottom) - c) * horizontal;
                _spanAlpha[i] = upper == lower ? upper : (1 - vertical) * upper + vertical * lower;
            }
        }
        _spanX = x; _spanY = y; _spanCount = length;
    }

    internal float Alpha(int x, int y)
    {
        if (_floating && y == _spanY && x >= _spanX && x - _spanX < _spanCount)
            return _spanAlpha[x - _spanX];
        var point = _deviceToMask.Apply(x + 0.5, y + 0.5);
        if (_nearest)
            return ReadAlpha((int)Math.Clamp(Math.Floor(point.X), 0, _width - 1), (int)Math.Clamp(Math.Floor(point.Y), 0, _height - 1));
        if (!_floating)
        {
            // MaskAlpha uses the same quantized IPC matrix as the byte color-source span.
            long u = x * _m11 + y * _m21 + _dx;
            long v = x * _m12 + y * _m22 + _dy;
            var t = Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(u, v, _width, _height, MilBitmapWrapMode.Extend);
            uint alpha = Direct3D9SoftwareBilinearSpan.InterpolateBilinearArgb(
                ReadPixel(t.X1, t.Y1), ReadPixel(t.X2, t.Y1), ReadPixel(t.X1, t.Y2), ReadPixel(t.X2, t.Y2), t.XFraction, t.YFraction);
            return (byte)(alpha >> 24) / 255f;
        }
        double sx = Math.Clamp(point.X - 0.5, 0, _width - 1), sy = Math.Clamp(point.Y - 0.5, 0, _height - 1);
        int left = (int)Math.Floor(sx), top = (int)Math.Floor(sy);
        int right = Math.Min(left + 1, (int)_width - 1), bottom = Math.Min(top + 1, (int)_height - 1);
        float a = ReadAlpha(left, top), b = ReadAlpha(right, top), c = ReadAlpha(left, bottom), d = ReadAlpha(right, bottom);
        float upper = a + (b - a) * (float)(sx - left), lower = c + (d - c) * (float)(sx - left);
        return upper + (lower - upper) * (float)(sy - top);
    }

    private uint ReadPixel(int x, int y) => MemoryMarshal.Read<uint>(_pixels.AsSpan(checked((y * (int)_width + x) * 4), 4));
    private float ReadAlpha(int x, int y) => _floating
        ? MemoryMarshal.Read<Vector4>(_pixels.AsSpan(checked((y * (int)_width + x) * 16), 16)).W
        : (byte)(ReadPixel(x, y) >> 24) / 255f;

    public void Dispose()
    {
        nint source = _source; _source = 0; _pixels = []; _spanAlpha = []; _spanCount = 0;
        Direct3D9Factory.Release(source);
    }
}
