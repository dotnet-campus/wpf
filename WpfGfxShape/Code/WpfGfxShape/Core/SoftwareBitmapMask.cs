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

    internal SoftwareBitmapMask(nint ownedSource, GeneratedImageTransform maskToEffect)
    {
        _source = ownedSource;
        _maskToEffect = maskToEffect;
    }

    internal int Prepare(SoftwareImageDrawingContext context, bool floating)
    {
        _pixels = [];
        if (_source == 0) return Direct3D9Factory.WgxInvalidCallHResult;
        GeneratedImageTransform transform = context.WorldToDevice.Prepend(_maskToEffect);
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
            _pixels = pixels;
            return 0;
        }
        finally { Direct3D9Factory.Release(converter); Direct3D9Factory.Release(scaler); Direct3D9Factory.Release(wic); }
    }

    internal float Alpha(int x, int y)
    {
        var point = _deviceToMask.Apply(x + 0.5, y + 0.5);
        if (_nearest)
            return ReadAlpha((int)Math.Clamp(Math.Floor(point.X), 0, _width - 1), (int)Math.Clamp(Math.Floor(point.Y), 0, _height - 1));
        if (!_floating)
        {
            long u = (long)Math.Round(Math.Clamp(point.X - 0.5, -1, _width) * 65536);
            long v = (long)Math.Round(Math.Clamp(point.Y - 0.5, -1, _height) * 65536);
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
        nint source = _source; _source = 0; _pixels = [];
        Direct3D9Factory.Release(source);
    }
}
