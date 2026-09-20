namespace WpfGfxShape.Core;

internal delegate int Direct3D9LockSoftwareRenderTargetBitmap(nint bitmap, out nint bitmapLock);
internal delegate int Direct3D9GetSoftwareBitmapLockSize(nint bitmapLock, out uint width, out uint height);

internal sealed unsafe class Direct3D9SoftwareRenderTargetBitmap : IDisposable
{
    private readonly Func<bool> _shouldTintBitmapSource;
    private readonly Direct3D9LockSoftwareRenderTargetBitmap _lockBitmap;
    private readonly Func<nint, (int Result, MilPixelFormat PixelFormat)> _getPixelFormat;
    private readonly Func<nint, (int Result, uint BufferSize, nint Data)> _getDataPointer;
    private readonly Direct3D9GetSoftwareBitmapLockSize _getSize;
    private readonly Func<nint, (int Result, uint Stride)> _getStride;
    private readonly Action<nint, uint, uint, uint> _tintBitmap;
    private readonly Action<nint> _release;
    private nint _internalSurface;

    internal Direct3D9SoftwareRenderTargetBitmapCreationRequest CreationRequest { get; }

    internal Direct3D9SoftwareRenderTargetBitmap(
        nint internalSurface,
        Direct3D9SoftwareRenderTargetBitmapCreationRequest creationRequest = default,
        Func<bool>? shouldTintBitmapSource = null,
        Direct3D9LockSoftwareRenderTargetBitmap? lockBitmap = null,
        Func<nint, (int Result, MilPixelFormat PixelFormat)>? getPixelFormat = null,
        Func<nint, (int Result, uint BufferSize, nint Data)>? getDataPointer = null,
        Direct3D9GetSoftwareBitmapLockSize? getSize = null,
        Func<nint, (int Result, uint Stride)>? getStride = null,
        Action<nint, uint, uint, uint>? tintBitmap = null,
        Action<nint>? release = null)
    {
        if (internalSurface == 0)
        {
            throw new ArgumentException("The internal surface must not be null.", nameof(internalSurface));
        }

        CreationRequest = creationRequest;
        _shouldTintBitmapSource = shouldTintBitmapSource ?? (() => false);
        _lockBitmap = lockBitmap ?? LockBitmap;
        _getPixelFormat = getPixelFormat ?? GetPixelFormat;
        _getDataPointer = getDataPointer ?? GetDataPointer;
        _getSize = getSize ?? Direct3D9BitmapLock.GetSize;
        _getStride = getStride ?? GetStride;
        _tintBitmap = tintBitmap ?? ((_, _, _, _) => { });
        _release = release ?? Direct3D9Factory.Release;

        AddRef(internalSurface);
        _internalSurface = internalSurface;
    }

    internal int GetBitmapSource(out nint bitmapSource)
    {
        ObjectDisposedException.ThrowIf(_internalSurface == 0, this);

        if (_shouldTintBitmapSource())
        {
            TintBitmapSource();
        }

        bitmapSource = _internalSurface;
        AddRef(bitmapSource);
        return Direct3D9Factory.SuccessHResult;
    }

    internal int GetCacheableBitmapSource(out nint bitmapSource)
    {
        return GetBitmapSource(out bitmapSource);
    }

    internal int GetBitmap(out nint bitmap)
    {
        ObjectDisposedException.ThrowIf(_internalSurface == 0, this);

        bitmap = _internalSurface;
        AddRef(bitmap);
        return Direct3D9Factory.SuccessHResult;
    }

    public void Dispose()
    {
        nint internalSurface = _internalSurface;
        if (internalSurface == 0)
        {
            return;
        }

        _internalSurface = 0;
        _release(internalSurface);
    }

    private void TintBitmapSource()
    {
        int result = _lockBitmap(_internalSurface, out nint bitmapLock);
        if (result < 0 || bitmapLock == 0)
        {
            return;
        }

        try
        {
            (result, MilPixelFormat pixelFormat) = _getPixelFormat(bitmapLock);
            if (result < 0 || pixelFormat is not (MilPixelFormat.Pbgra32Bpp or MilPixelFormat.Bgra32Bpp))
            {
                return;
            }

            (result, uint _, nint data) = _getDataPointer(bitmapLock);
            if (result < 0)
            {
                return;
            }

            result = _getSize(bitmapLock, out uint width, out uint height);
            if (result < 0)
            {
                return;
            }

            (result, uint stride) = _getStride(bitmapLock);
            if (result >= 0)
            {
                _tintBitmap(data, width, height, stride);
            }
        }
        finally
        {
            _release(bitmapLock);
        }
    }

    private static int LockBitmap(nint bitmap, out nint bitmapLock)
    {
        return Direct3D9Bitmap.LockEntireBitmap(
            bitmap,
            null,
            MilBitmapLockFlags.Read | MilBitmapLockFlags.Write,
            out bitmapLock);
    }

    private static (int Result, MilPixelFormat PixelFormat) GetPixelFormat(nint bitmapLock)
    {
        int result = Direct3D9BitmapLock.GetPixelFormat(bitmapLock, out MilPixelFormat pixelFormat);
        return (result, pixelFormat);
    }

    private static (int Result, uint BufferSize, nint Data) GetDataPointer(nint bitmapLock)
    {
        int result = Direct3D9BitmapLock.GetDataPointer(bitmapLock, out uint bufferSize, out nint data);
        return (result, bufferSize, data);
    }

    private static (int Result, uint Stride) GetStride(nint bitmapLock)
    {
        int result = Direct3D9BitmapLock.GetStride(bitmapLock, out uint stride);
        return (result, stride);
    }

    private static void AddRef(nint instance)
    {
        void*** unknown = (void***) instance;
        void** vtable = *unknown;
        delegate* unmanaged[Stdcall]<void***, uint> addRef =
            (delegate* unmanaged[Stdcall]<void***, uint>) vtable[1];
        _ = addRef(unknown);
    }
}
