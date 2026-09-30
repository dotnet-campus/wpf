namespace WpfGfxShape.Core;

/// <summary>
/// Owns the bitmap target lock and software image consumer for one render pass.
/// This is an internal managed lifetime boundary, not the native IRenderTargetInternal ABI.
/// </summary>
internal sealed unsafe class SoftwareImageRenderSession : IDisposable
{
    private nint _targetBitmap;
    private nint _milBitmap;
    private nint _bitmap;
    private nint _bitmapLock;
    private Direct3D9SoftwareImageRenderer? _renderer;

    internal static int Open(nint target, uint width, uint height, out SoftwareImageRenderSession? session)
    {
        session = null;
        var candidate = new SoftwareImageRenderSession();
        try
        {
            int hr = candidate.Initialize(target, width, height);
            if (hr < 0) return hr;
            session = candidate;
            return 0;
        }
        finally
        {
            if (session is null) candidate.Dispose();
        }
    }

    private int Initialize(nint target, uint width, uint height)
    {
        int hr = Query(target, new("00000201-a8f2-4877-ba0a-fd2b6645fb94"), out _targetBitmap);
        if (hr < 0) return hr;
        nint milBitmap = 0;
        hr = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)_targetBitmap)[9])(_targetBitmap, &milBitmap);
        _milBitmap = milBitmap;
        if (hr < 0) return hr;
        hr = Query(_milBitmap, new("00000121-a8f2-4877-ba0a-fd2b6645fb94"), out _bitmap);
        if (hr < 0) return hr;
        Guid format;
        hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)_bitmap)[4])(_bitmap, &format);
        if (hr < 0) return hr;
        bool floating = format == new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91a");
        if (!floating && format != new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910")) return Direct3D9Factory.NotImplementedHResult;
        uint w, h;
        hr = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)_bitmap)[3])(_bitmap, &w, &h);
        if (hr < 0) return hr;
        nint bitmapLock = 0;
        hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)_bitmap)[8])(_bitmap, 0, 3, &bitmapLock);
        _bitmapLock = bitmapLock;
        if (hr < 0) return hr;
        uint stride, size;
        byte* pixels;
        hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)_bitmapLock)[4])(_bitmapLock, &stride);
        if (hr < 0) return hr;
        hr = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)_bitmapLock)[5])(_bitmapLock, &size, &pixels);
        if (hr < 0) return hr;
        _renderer = new(pixels, size, stride, Math.Min(w, width), Math.Min(h, height), floating);
        return 0;
    }

    internal int Draw(nint source, MilRectD rectangle, bool isMilSource, SoftwareImageDrawingContext context)
    {
        if (_renderer is null) return Direct3D9Factory.WgxInvalidCallHResult;
        return _renderer.DrawTransformed(source, rectangle, context, isMilSource);
    }

    internal int BeginLayer(double opacity, SoftwareImageCoverage? mask = null, float? alphaMask = null, GeneratedImageMask? imageMask = null) => _renderer?.BeginOpacityLayer(opacity, mask, alphaMask, imageMask) ?? Direct3D9Factory.WgxInvalidCallHResult;
    internal int EndLayer() => _renderer?.EndOpacityLayer() ?? Direct3D9Factory.WgxInvalidCallHResult;

    public void Dispose()
    {
        // Detach first: final COM releases can invoke foreign callbacks.
        var renderer = _renderer;
        _renderer = null;
        nint bitmapLock = _bitmapLock, bitmap = _bitmap, milBitmap = _milBitmap, targetBitmap = _targetBitmap;
        _bitmapLock = _bitmap = _milBitmap = _targetBitmap = 0;
        // Unfinished opacity layers must be discarded before releasing the target lock.
        try { renderer?.Dispose(); }
        finally
        {
            Direct3D9Factory.Release(bitmapLock);
            Direct3D9Factory.Release(bitmap);
            Direct3D9Factory.Release(milBitmap);
            Direct3D9Factory.Release(targetBitmap);
        }
    }

    private static int Query(nint value, Guid id, out nint result)
    {
        result = 0;
        if (value == 0) return Direct3D9Factory.InvalidArgumentHResult;
        nint output = 0;
        int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)value)[0])(value, &id, &output);
        result = output;
        return hr;
    }
}
