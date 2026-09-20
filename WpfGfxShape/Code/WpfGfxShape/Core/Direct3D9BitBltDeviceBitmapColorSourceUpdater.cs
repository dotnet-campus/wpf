using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9GetBitmapSurfaceDeviceContext(nint surface, out nint deviceContext);

internal delegate int Direct3D9ReleaseBitmapSurfaceDeviceContext(nint surface, nint deviceContext);

internal delegate int Direct3D9BitBltBitmapRectangle(
    nint destinationDeviceContext,
    Direct3D9BitmapRealizationRectangle destinationRectangle,
    nint sourceDeviceContext,
    uint sourceX,
    uint sourceY);

internal delegate int Direct3D9GetBitmapDestinationSurface(out nint surface);

internal delegate int Direct3D9StretchBitmapRectangle(
    nint sourceSurface,
    Direct3D9SurfaceRect sourceRectangle,
    nint destinationSurface,
    Direct3D9SurfaceRect destinationRectangle,
    Texturefiltertype filter);

internal sealed class Direct3D9BitBltDeviceBitmapColorSourceUpdater
{
    private readonly Direct3D9Device _device;
    private readonly nint _transferSurface;
    private readonly Direct3D9GetBitmapSurfaceDeviceContext _getDeviceContext;
    private readonly Direct3D9ReleaseBitmapSurfaceDeviceContext _releaseDeviceContext;
    private readonly Direct3D9BitBltBitmapRectangle _bitBlt;
    private readonly Direct3D9GetBitmapDestinationSurface _getDestinationSurface;
    private readonly Direct3D9StretchBitmapRectangle _stretchRectangle;
    private readonly Action<nint> _releaseSurface;

    internal Direct3D9BitBltDeviceBitmapColorSourceUpdater(
        Direct3D9Device device,
        nint transferSurface,
        Direct3D9GetBitmapSurfaceDeviceContext getDeviceContext,
        Direct3D9ReleaseBitmapSurfaceDeviceContext releaseDeviceContext,
        Direct3D9BitBltBitmapRectangle bitBlt,
        Direct3D9GetBitmapDestinationSurface getDestinationSurface,
        Direct3D9StretchBitmapRectangle stretchRectangle,
        Action<nint> releaseSurface)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentOutOfRangeException.ThrowIfZero(transferSurface);
        ArgumentNullException.ThrowIfNull(getDeviceContext);
        ArgumentNullException.ThrowIfNull(releaseDeviceContext);
        ArgumentNullException.ThrowIfNull(bitBlt);
        ArgumentNullException.ThrowIfNull(getDestinationSurface);
        ArgumentNullException.ThrowIfNull(stretchRectangle);
        ArgumentNullException.ThrowIfNull(releaseSurface);

        _device = device;
        _transferSurface = transferSurface;
        _getDeviceContext = getDeviceContext;
        _releaseDeviceContext = releaseDeviceContext;
        _bitBlt = bitBlt;
        _getDestinationSurface = getDestinationSurface;
        _stretchRectangle = stretchRectangle;
        _releaseSurface = releaseSurface;
    }

    internal int Update(
        ReadOnlySpan<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
        nint sourceSurface)
    {
        ArgumentOutOfRangeException.ThrowIfZero(sourceSurface);

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);

        nint sourceDeviceContext = 0;
        nint transferDeviceContext = 0;
        nint destinationSurface = 0;

        try
        {
            int result = _getDeviceContext(sourceSurface, out sourceDeviceContext);
            if (result < 0)
            {
                return result;
            }

            if (sourceDeviceContext == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            result = _getDeviceContext(_transferSurface, out transferDeviceContext);
            if (result < 0)
            {
                return result;
            }

            if (transferDeviceContext == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            foreach (Direct3D9BitmapRealizationRectangle dirtyRectangle in dirtyRectangles)
            {
                result = _bitBlt(
                    transferDeviceContext,
                    dirtyRectangle,
                    sourceDeviceContext,
                    dirtyRectangle.Left,
                    dirtyRectangle.Top);
                if (result < 0)
                {
                    return result;
                }
            }

            result = _releaseDeviceContext(sourceSurface, sourceDeviceContext);
            if (result < 0)
            {
                return result;
            }

            sourceDeviceContext = 0;

            result = _releaseDeviceContext(_transferSurface, transferDeviceContext);
            if (result < 0)
            {
                return result;
            }

            transferDeviceContext = 0;

            result = _getDestinationSurface(out destinationSurface);
            if (result < 0)
            {
                return result;
            }

            if (destinationSurface == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            foreach (Direct3D9BitmapRealizationRectangle dirtyRectangle in dirtyRectangles)
            {
                Direct3D9SurfaceRect rectangle = new(
                    checked((int) dirtyRectangle.Left),
                    checked((int) dirtyRectangle.Top),
                    checked((int) dirtyRectangle.Right),
                    checked((int) dirtyRectangle.Bottom));
                result = _stretchRectangle(
                    _transferSurface,
                    rectangle,
                    destinationSurface,
                    rectangle,
                    Texturefiltertype.None);
                if (result < 0)
                {
                    return result;
                }
            }

            return Direct3D9Factory.SuccessHResult;
        }
        finally
        {
            if (sourceDeviceContext != 0)
            {
                _ = _releaseDeviceContext(sourceSurface, sourceDeviceContext);
            }

            if (transferDeviceContext != 0)
            {
                _ = _releaseDeviceContext(_transferSurface, transferDeviceContext);
            }

            if (destinationSurface != 0)
            {
                _releaseSurface(destinationSurface);
            }
        }
    }
}
