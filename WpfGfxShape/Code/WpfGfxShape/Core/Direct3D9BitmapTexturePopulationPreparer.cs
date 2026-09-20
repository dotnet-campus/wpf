using System.Runtime.Versioning;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9LockBitmapForTexturePopulation(out nint bitmapLock);

internal delegate int Direct3D9GetBitmapLockData(
    nint bitmapLock,
    out uint bufferSize,
    out nint bits);

internal delegate int Direct3D9GetBitmapLockStride(nint bitmapLock, out uint stride);

internal delegate int Direct3D9GetBitmapLockPixelFormat(
    nint bitmapLock,
    out MilPixelFormat pixelFormat);

internal delegate int Direct3D9QueryBitmapDynamicResource(
    nint bitmap,
    out nint dynamicResource);

internal delegate int Direct3D9IsDynamicResource(
    nint dynamicResource,
    out bool isDynamic);

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9BitmapTexturePopulationPreparer
{
    private readonly Direct3D9BitmapSystemMemorySurfaceSource _surfaceSource;
    private readonly Direct3D9LockBitmapForTexturePopulation _lockBitmap;
    private readonly Direct3D9GetBitmapLockData _getData;
    private readonly Direct3D9GetBitmapLockStride _getStride;
    private readonly Direct3D9GetBitmapLockPixelFormat _getPixelFormat;
    private readonly Action<nint> _release;
    private readonly Direct3D9QueryBitmapDynamicResource? _queryDynamicResource;
    private readonly Direct3D9IsDynamicResource? _isDynamicResource;
    private readonly nint _bitmap;
    private readonly bool _canUseBitmapBits;
    private readonly bool _canShareBitmapBits;
    private readonly uint _bitmapHeight;
    private readonly uint _surfaceWidth;
    private readonly uint _surfaceHeight;

    internal Direct3D9BitmapTexturePopulationPreparer(
        nint bitmap,
        bool bitmapSourceIsBitmap,
        Direct3D9TexelLayout horizontalLayout,
        Direct3D9TexelLayout verticalLayout,
        bool isLddmDevice,
        uint bitmapWidth,
        uint bitmapHeight,
        uint surfaceWidth,
        uint surfaceHeight,
        Direct3D9BitmapSystemMemorySurfaceSource surfaceSource)
        : this(
            surfaceSource,
            (out nint bitmapLock) => Direct3D9Bitmap.Lock(
                bitmap,
                new Direct3D9BitmapSourceRectangle(
                    0,
                    0,
                    checked((int) bitmapWidth),
                    checked((int) bitmapHeight)),
                MilBitmapLockFlags.Read,
                out bitmapLock),
            Direct3D9BitmapLock.GetDataPointer,
            Direct3D9BitmapLock.GetStride,
            Direct3D9BitmapLock.GetPixelFormat,
            resource => Direct3D9Factory.Release(resource),
            bitmap,
            QueryDynamicResource,
            IsDynamicResource,
            bitmapSourceIsBitmap
                && IsDirectLayout(horizontalLayout)
                && IsDirectLayout(verticalLayout),
            isLddmDevice,
            bitmapHeight,
            surfaceWidth,
            surfaceHeight)
    {
    }

    internal Direct3D9BitmapTexturePopulationPreparer(
        Direct3D9BitmapSystemMemorySurfaceSource surfaceSource,
        Direct3D9LockBitmapForTexturePopulation lockBitmap,
        Direct3D9GetBitmapLockData getData,
        Direct3D9GetBitmapLockStride getStride,
        Direct3D9GetBitmapLockPixelFormat getPixelFormat,
        Action<nint> release,
        bool canUseBitmapBits,
        uint bitmapHeight,
        uint surfaceWidth,
        uint surfaceHeight)
        : this(
            surfaceSource,
            lockBitmap,
            getData,
            getStride,
            getPixelFormat,
            release,
            0,
            null,
            null,
            canUseBitmapBits,
            canUseBitmapBits,
            bitmapHeight,
            surfaceWidth,
            surfaceHeight)
    {
    }

    internal Direct3D9BitmapTexturePopulationPreparer(
        Direct3D9BitmapSystemMemorySurfaceSource surfaceSource,
        Direct3D9LockBitmapForTexturePopulation lockBitmap,
        Direct3D9GetBitmapLockData getData,
        Direct3D9GetBitmapLockStride getStride,
        Direct3D9GetBitmapLockPixelFormat getPixelFormat,
        Action<nint> release,
        nint bitmap,
        Direct3D9QueryBitmapDynamicResource? queryDynamicResource,
        Direct3D9IsDynamicResource? isDynamicResource,
        bool canUseBitmapBits,
        bool canShareBitmapBits,
        uint bitmapHeight,
        uint surfaceWidth,
        uint surfaceHeight)
    {
        ArgumentNullException.ThrowIfNull(surfaceSource);
        ArgumentNullException.ThrowIfNull(lockBitmap);
        ArgumentNullException.ThrowIfNull(getData);
        ArgumentNullException.ThrowIfNull(getStride);
        ArgumentNullException.ThrowIfNull(getPixelFormat);
        ArgumentNullException.ThrowIfNull(release);

        if ((queryDynamicResource is null) != (isDynamicResource is null))
        {
            throw new ArgumentException("Dynamic-resource query delegates must be provided together.");
        }

        _surfaceSource = surfaceSource;
        _lockBitmap = lockBitmap;
        _getData = getData;
        _getStride = getStride;
        _getPixelFormat = getPixelFormat;
        _release = release;
        _bitmap = bitmap;
        _queryDynamicResource = queryDynamicResource;
        _isDynamicResource = isDynamicResource;
        _canUseBitmapBits = canUseBitmapBits;
        _canShareBitmapBits = canShareBitmapBits;
        _bitmapHeight = bitmapHeight;
        _surfaceWidth = surfaceWidth;
        _surfaceHeight = surfaceHeight;
    }

    internal int Prepare(
        out nint bitmapLock,
        out bool copySourceToSystemMemorySurface,
        out Direct3D9SystemMemoryUpdateSurface? systemMemorySurface)
    {
        bitmapLock = 0;
        copySourceToSystemMemorySurface = true;
        systemMemorySurface = null;

        nint acquiredLock = 0;
        nint dynamicResource = 0;
        nint bits = 0;
        uint width = _surfaceWidth;
        uint height = _surfaceHeight;
        bool canShareBits = false;

        try
        {
            bool shouldLockBitmap = _canUseBitmapBits && _canShareBitmapBits;
            if (_canUseBitmapBits && !_canShareBitmapBits && _queryDynamicResource is not null)
            {
                int queryResult = _queryDynamicResource(_bitmap, out dynamicResource);
                if (queryResult >= 0)
                {
                    if (dynamicResource == 0)
                    {
                        throw new InvalidOperationException("Dynamic-resource querying returned a null interface.");
                    }

                    int dynamicResult = _isDynamicResource!(dynamicResource, out shouldLockBitmap);
                    if (dynamicResult < 0)
                    {
                        return dynamicResult;
                    }
                }
            }

            if (shouldLockBitmap)
            {
                int result = _lockBitmap(out acquiredLock);
                if (result < 0)
                {
                    return result;
                }
                if (acquiredLock == 0)
                {
                    throw new InvalidOperationException("Bitmap locking returned a null lock.");
                }

                result = _getData(acquiredLock, out _, out bits);
                if (result < 0)
                {
                    return result;
                }

                result = _getStride(acquiredLock, out uint stride);
                if (result < 0)
                {
                    return result;
                }

                result = _getPixelFormat(acquiredLock, out MilPixelFormat pixelFormat);
                if (result < 0)
                {
                    return result;
                }

                byte bitsPerPixel = MilPixelFormatInfo.GetBitsPerPixel(pixelFormat);
                if (bitsPerPixel == 0 || (bitsPerPixel % 8) != 0)
                {
                    return Direct3D9Factory.InvalidCallHResult;
                }

                uint bytesPerPixel = (uint) bitsPerPixel / 8;
                if (_canShareBitmapBits)
                {
                    if ((stride % bytesPerPixel) == 0)
                    {
                        width = stride / bytesPerPixel;
                        height = _bitmapHeight;
                        canShareBits = true;
                    }
                    else
                    {
                        _release(acquiredLock);
                        acquiredLock = 0;
                        bits = 0;
                    }
                }
            }

            int surfaceResult = _surfaceSource.GetSurface(
                (void*) bits,
                width,
                height,
                canShareBits,
                out systemMemorySurface);
            if (surfaceResult < 0)
            {
                return surfaceResult;
            }

            bitmapLock = acquiredLock;
            acquiredLock = 0;
            copySourceToSystemMemorySurface = !canShareBits;
            return surfaceResult;
        }
        finally
        {
            if (acquiredLock != 0)
            {
                _release(acquiredLock);
            }
            if (dynamicResource != 0)
            {
                _release(dynamicResource);
            }
        }
    }

    private static int QueryDynamicResource(nint bitmap, out nint dynamicResource)
    {
        Guid interfaceId = new("8CB53EB7-D409-4066-9487-C0D4152FE80A");
        void* queriedResource = null;
        void*** unknown = (void***) bitmap;
        delegate* unmanaged[Stdcall]<void***, Guid*, void**, int> queryInterface =
            (delegate* unmanaged[Stdcall]<void***, Guid*, void**, int>) (*unknown)[0];
        int result = queryInterface(unknown, &interfaceId, &queriedResource);
        dynamicResource = (nint) queriedResource;
        return result;
    }

    private static int IsDynamicResource(nint dynamicResource, out bool isDynamic)
    {
        byte dynamic = 0;
        void*** resource = (void***) dynamicResource;
        delegate* unmanaged[Stdcall]<void***, byte*, int> isDynamicResource =
            (delegate* unmanaged[Stdcall]<void***, byte*, int>) (*resource)[3];
        int result = isDynamicResource(resource, &dynamic);
        isDynamic = dynamic != 0;
        return result;
    }

    private static bool IsDirectLayout(Direct3D9TexelLayout layout) =>
        layout is Direct3D9TexelLayout.Natural or Direct3D9TexelLayout.FirstOnly;
}
