using System.Runtime.InteropServices;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe partial class Direct3D9SoftwareImageRenderer : IDisposable
{
    private readonly byte* _destination;
    private readonly uint _destinationStride;
    private readonly uint _destinationSize;
    private readonly uint _width;
    private readonly uint _height;
    private readonly Direct3D9SoftwareRenderTargetSurface _surface;
    private byte[] _pixels = [];
    private readonly bool _floating;
    private int BytesPerPixel => _floating ? 16 : 4;
    private SoftwareImageDrawingContext _context;
    private uint ScalingMode => _context.BitmapScalingMode;
    private MilCompositingMode CompositingMode => _context.CompositingMode;
    private readonly Stack<(byte[] Pixels, double Opacity, SoftwareImageCoverage? Mask, float? AlphaMask, GeneratedImageMask? ImageMask)> _layers = new();

    internal int BeginOpacityLayer(double opacity, SoftwareImageCoverage? mask = null, float? alphaMask = null, GeneratedImageMask? imageMask = null)
    {
        if (!double.IsFinite(opacity) || opacity < 0 || opacity > 1) return Direct3D9Factory.InvalidArgumentHResult;
        _layers.Push((new byte[checked((int)_width * (int)_height * BytesPerPixel)], opacity, mask, alphaMask, imageMask));
        return 0;
    }

    internal int EndOpacityLayer()
    {
        if (_layers.Count == 0) return Direct3D9Factory.WgxInvalidCallHResult;
        var layer = _layers.Pop();
        nint effectList = 0;
        int result = MilEffectList.Create(out effectList);
        if (result < 0) return result;
        SoftwareBitmapEffects? effects = null;
        try
        {
            if (layer.ImageMask is { } imageMask)
            {
                result = AddImageMask(effectList, imageMask);
                if (result < 0) return result;
            }
            if (layer.AlphaMask is { } alpha)
            {
                result = AddSolidMask(effectList, alpha);
                if (result < 0) return result;
            }
            result = MilEffectList.AddAlphaScale(effectList, (float)layer.Opacity);
            if (result < 0) return result;
            result = SoftwareBitmapEffects.Capture(effectList, out effects);
            if (result < 0) return result;
        }
        finally { Direct3D9Factory.Release(effectList); }
        using var ownedEffects = effects;
        if (effects is null) return Direct3D9Factory.UnexpectedHResult;
        result = effects.Prepare(new(GeneratedImageTransform.Identity, null, 3, MilCompositingMode.SourceOver, PrefilterEnabled: false), _floating);
        if (result < 0) return result;
        result = Lock(out byte[] destination, out _);
        if (result < 0) return result;
        try
        {
            if (_floating) return CompositeFloatLayer(layer.Pixels, destination, effects, layer.Mask);
            for (int i = 0; i < layer.Pixels.Length; i += 4)
            {
                int x = i / 4 % (int)_width, y = i / 4 / (int)_width;
                uint sample = effects.Apply(MemoryMarshal.Read<uint>(layer.Pixels.AsSpan(i, 4)), x, y);
                int coverage = layer.Mask?.GetCoverage(x, y) ?? 64;
                int alpha = ((byte)(sample >> 24) * coverage + 32) >> 6;
                for (int component = 0; component < 4; component++)
                {
                    int value = ((byte)(sample >> (component * 8)) * coverage + 32) >> 6;
                    destination[i + component] = (byte)Math.Min(255, value + (destination[i + component] * (255 - alpha) + 127) / 255);
                }
            }
            return 0;
        }
        finally { Unlock(); }
    }
    private byte[] _sourceRow = [];
    private byte[] _nextRow = [];
    private int _drawWidth;
    private int _drawHeight;
    private long _uIncrement;
    private long _vIncrement;
    private long _uOrigin;
    private long _vOrigin;
    private int _yFraction;
    private nint _source;
    private uint _sourceWidth;
    private uint _sourceHeight;
    private int _x;
    private int _y;
    private bool _opaque;
    private Func<int>? _realizeBitmap;
    private Direct3D9SurfaceRect _drawClip;

    internal Direct3D9SoftwareImageRenderer(byte* destination, uint size, uint stride, uint width, uint height, bool floating = false)
    {
        _floating = floating;
        _destination = destination;
        _destinationStride = stride;
        _destinationSize = size;
        _width = width;
        _height = height;
        _surface = new(width, height, Lock, Unlock,
            (out Direct3D9Software3DSurface? value) => { value = null; return Direct3D9Factory.NotImplementedHResult; },
            _ => { }, _ => { }, pixelFormat: floating ? MilPixelFormat.Prgba128BppFloat : MilPixelFormat.Pbgra32Bpp,
            fillPathSoftwareRenderTarget: RealizeAndDrawLocked,
            scanPipelineCallbacks: new(
                _ => 0,
                _ => Direct3D9Factory.NotImplementedHResult,
                BlendSpan,
                _ => { },
                () => { }));
    }

    internal int Draw(nint source, MilRectD rectangle, double offsetX, double offsetY, bool isMilSource, bool prefiltered = false)
    {
        uint width, height;
        int hr = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)source)[3])(source, &width, &height);
        if (hr < 0) return hr;
        double x = rectangle.X + offsetX, y = rectangle.Y + offsetY;
        // Fractional bounds still require coverage rasterization.
        if (!double.IsFinite(x) || !double.IsFinite(y) || x != Math.Truncate(x) || y != Math.Truncate(y)
            || x < int.MinValue || x > int.MaxValue || y < int.MinValue || y > int.MaxValue
            || !double.IsFinite(rectangle.Width) || !double.IsFinite(rectangle.Height)
            || rectangle.Width != Math.Truncate(rectangle.Width) || rectangle.Height != Math.Truncate(rectangle.Height)
            || rectangle.Width <= 0 || rectangle.Height <= 0
            || rectangle.Width > int.MaxValue || rectangle.Height > int.MaxValue
            || width == 0 || height == 0 || width > int.MaxValue / 4 || height > int.MaxValue)
            return Direct3D9Factory.NotImplementedHResult;
        if (!prefiltered && (_context.PrefilterEnabled ?? (ScalingMode is not (1 or 3))))
        {
            uint filteredWidth = PrefilterSize(width, (float)rectangle.Width / width);
            uint filteredHeight = PrefilterSize(height, (float)rectangle.Height / height);
            if (filteredWidth != width || filteredHeight != height)
                return DrawPrefiltered(source, rectangle, offsetX, offsetY, isMilSource, filteredWidth, filteredHeight);
        }
        _drawWidth = (int)rectangle.Width;
        _drawHeight = (int)rectangle.Height;
        float scaleX = (float)width / _drawWidth, scaleY = (float)height / _drawHeight;
        _uIncrement = (long)MathF.Round(scaleX * 65536);
        _vIncrement = (long)MathF.Round(scaleY * 65536);
        _uOrigin = (long)MathF.Round((scaleX * 0.5f - 0.5f) * 65536);
        _vOrigin = (long)MathF.Round((scaleY * 0.5f - 0.5f) * 65536);
        if (isMilSource)
        {
            MilPixelFormat format;
            hr = ((delegate* unmanaged[Stdcall]<nint, MilPixelFormat*, int>)(*(void***)source)[4])(source, &format);
            if (hr < 0) return hr;
            _opaque = format == MilPixelFormat.Bgr32Bpp;
            if (!_opaque && format != MilPixelFormat.Pbgra32Bpp)
                return DrawConverted(source, rectangle, offsetX, offsetY, true, prefiltered);
        }
        else
        {
            Guid format;
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)source)[4])(source, &format);
            if (hr < 0) return hr;
            _opaque = format == new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90e");
            if (!_opaque && format != new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910"))
                return DrawConverted(source, rectangle, offsetX, offsetY, false, prefiltered);
        }
        _source = source;
        _sourceWidth = width;
        _sourceHeight = height;
        _x = (int)x; _y = (int)y;
        _sourceRow = new byte[checked((int)width * 4)];
        _nextRow = new byte[_sourceRow.Length];
        try { return FillBitmapPath(); }
        finally { _source = 0; _sourceRow = []; _nextRow = []; }
    }

    private uint PrefilterSize(uint original, float scale)
        => ComputePrefilterSize(original, scale, _context.PrefilterThreshold ?? MathF.Sqrt(2));

    internal static uint ComputePrefilterSize(uint original, float scale, float shrinkThreshold)
    {
        if (shrinkThreshold <= 0) return original;
        float threshold = 1 / shrinkThreshold;
        if (scale > threshold) return original;
        if (scale * original <= 1) return 1;
        float exponent = MathF.Log(scale) / MathF.Log(threshold);
        float desired = threshold >= 1 || !float.IsFinite(exponent) || exponent >= original
            ? original * scale
            : original * (float)Math.Pow(threshold, (int)MathF.Floor(exponent));
        return Math.Clamp((uint)MathF.Ceiling(desired), 1u, original);
    }

    private int DrawPrefiltered(nint source, MilRectD rectangle, double offsetX, double offsetY, bool isMilSource, uint width, uint height)
    {
        nint wicSource = 0, scaler = 0;
        try
        {
            nint input = source;
            if (isMilSource)
            {
                int query = MilBitmapSourceAdapter.Create(source, out wicSource);
                if (query < 0) return query;
                if (wicSource == 0) return Direct3D9Factory.NoInterfaceHResult;
                input = wicSource;
            }
            int result = BitmapFormatConverter.CreateScaler(input, width, height, out scaler);
            if (result < 0) return result;
            return Draw(scaler, rectangle, offsetX, offsetY, false, prefiltered: true);
        }
        finally { Direct3D9Factory.Release(scaler); Direct3D9Factory.Release(wicSource); }
    }

    private int DrawConverted(nint source, MilRectD rectangle, double offsetX, double offsetY, bool isMilSource, bool prefiltered)
    {
        nint wicSource = 0, converter = 0;
        try
        {
            nint input = source;
            if (isMilSource)
            {
                int query = MilBitmapSourceAdapter.Create(source, out wicSource);
                if (query < 0) return query;
                if (wicSource == 0) return Direct3D9Factory.NoInterfaceHResult;
                input = wicSource;
            }
            int result = BitmapFormatConverter.Create(input, new("6fddc324-4e03-4bfe-b185-3d77768dc910"), out converter);
            if (result < 0) return result;
            return Draw(converter, rectangle, offsetX, offsetY, false, prefiltered);
        }
        finally
        {
            Direct3D9Factory.Release(converter);
            Direct3D9Factory.Release(wicSource);
        }
    }

    private int Lock(out byte[] pixels, out int stride)
    {
        pixels = [];
        stride = checked((int)_width * BytesPerPixel);
        if (_layers.TryPeek(out var layer))
        {
            pixels = _pixels = layer.Pixels;
            return 0;
        }
        ulong required = _height == 0 ? 0 : (ulong)(_height - 1) * _destinationStride + (uint)stride;
        if (_destination == null || _destinationStride < stride || required > _destinationSize)
            return Direct3D9Factory.InvalidArgumentHResult;
        _pixels = new byte[checked(stride * (int)_height)];
        for (uint y = 0; y < _height; y++)
            new ReadOnlySpan<byte>(_destination + (nuint)y * _destinationStride, stride).CopyTo(_pixels.AsSpan(checked((int)y * stride), stride));
        pixels = _pixels;
        return 0;
    }

    private void Unlock()
    {
        if (_pixels.Length == 0) return;
        if (_layers.Count != 0)
        {
            _pixels = [];
            return;
        }
        int stride = checked((int)_width * BytesPerPixel);
        for (uint y = 0; y < _height; y++)
            _pixels.AsSpan(checked((int)y * stride), stride).CopyTo(new Span<byte>(_destination + (nuint)y * _destinationStride, stride));
        _pixels = [];
    }

    private int RealizeAndDrawLocked(byte[] pixels, int stride, Direct3D9SoftwareRenderTargetState state, Direct3D9SurfaceRect clip)
    {
        _drawClip = clip;
        int effectsResult = _context.Effects?.Prepare(_context, _floating) ?? 0;
        if (effectsResult < 0) return effectsResult;
        return _realizeBitmap is { } realize ? realize() : Direct3D9Factory.WgxInvalidCallHResult;
    }

    private int FillBitmapPath() => DrawLocked(_drawClip);

    private int DrawLocked(Direct3D9SurfaceRect clip)
    {
        if (_coverage is not null) return DrawAffineLocked(clip);
        int left = (int)Math.Max(clip.Left, (long)_x);
        int right = (int)Math.Min(clip.Right, (long)_x + _drawWidth);
        int top = (int)Math.Max(clip.Top, (long)_y);
        int bottom = (int)Math.Min(clip.Bottom, (long)_y + _drawHeight);
        if (left >= right || top >= bottom) return 0;
        int hr = _surface.SetupPipeline(MilPixelFormat.Pbgra32Bpp, _source, false, false, CompositingMode, (uint)(right - left));
        if (hr < 0) return hr;
        fixed (byte* row = _sourceRow)
        fixed (byte* nextRow = _nextRow)
        {
            for (int y = top; y < bottom; y++)
            {
                long v = _vOrigin + ((long)y - _y) * _vIncrement;
                var selection = Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(0, v, _sourceWidth, _sourceHeight, MilBitmapWrapMode.Extend);
                _yFraction = selection.YFraction;
                Direct3D9BitmapSourceRectangle region = new(0, selection.Y1, (int)_sourceWidth, 1);
                hr = Direct3D9BitmapSource.CopyPixels(_source, region, _sourceWidth * 4, _sourceWidth * 4, (nint)row);
                if (hr < 0) return hr;
                if (selection.Y1 == selection.Y2) _sourceRow.CopyTo(_nextRow, 0);
                else
                {
                    region = new(0, selection.Y2, (int)_sourceWidth, 1);
                    hr = Direct3D9BitmapSource.CopyPixels(_source, region, _sourceWidth * 4, _sourceWidth * 4, (nint)nextRow);
                    if (hr < 0) return hr;
                }
                hr = _surface.OutputSpan(y, left, right);
                if (hr < 0) return hr;
            }
        }
        return 0;
    }

    private void BlendSpan(byte[] destination, int offset, uint count, int x, int y)
    {
        if (_floating) { BlendFloatSpan(destination, offset, count, x, y); return; }
        if (_coverage is not null) { BlendAffineSpan(destination, offset, count, x, y); return; }
        long u = _uOrigin + ((long)x - _x) * _uIncrement;
        for (uint pixel = 0; pixel < count; pixel++, u += _uIncrement, offset += 4)
        {
            var selection = Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(u, 0, _sourceWidth, _sourceHeight, MilBitmapWrapMode.Extend);
            uint sample = Direct3D9SoftwareBilinearSpan.InterpolateBilinearArgb(
                ReadPixel(_sourceRow, selection.X1), ReadPixel(_sourceRow, selection.X2),
                ReadPixel(_nextRow, selection.X1), ReadPixel(_nextRow, selection.X2), selection.XFraction, _yFraction);
            if (_context.Effects is { } effects) sample = effects.Apply(sample, x + (int)pixel, y);
            int alpha = (byte)(sample >> 24);
            for (int component = 0; component < 4; component++)
            {
                int value = (byte)(sample >> (component * 8));
                destination[offset + component] = CompositingMode == MilCompositingMode.SourceCopy
                    ? (byte)value
                    : (byte)Math.Min(255, value + (destination[offset + component] * (255 - alpha) + 127) / 255);
            }
        }
    }

    private uint ReadPixel(byte[] row, int x)
    {
        uint value = MemoryMarshal.Read<uint>(row.AsSpan(checked(x * 4), 4));
        return _opaque ? value | 0xff000000 : value;
    }

    public void Dispose()
    {
        // An aborted frame must not composite unfinished layers.
        _surface.Dispose();
        _layers.Clear();
        _pixels = [];
    }
}
