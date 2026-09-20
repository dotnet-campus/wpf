using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9GetBitmapSurfaceDevice(nint surface, out nint device);

internal delegate int Direct3D9OpenSharedBitmapTexture(
    nint device,
    SurfaceDesc description,
    uint levels,
    ref nint sharedHandle,
    out nint texture);

internal delegate int Direct3D9GetBitmapTextureSurfaceLevel(
    nint texture,
    uint level,
    out nint surface);

internal delegate int Direct3D9StretchSharedBitmapRectangle(
    nint device,
    nint sourceSurface,
    Direct3D9SurfaceRect sourceRectangle,
    nint destinationSurface,
    Direct3D9SurfaceRect destinationRectangle,
    Texturefiltertype filter);

internal delegate int Direct3D9FlushSharedBitmapSurface(
    nint device,
    nint surface,
    SurfaceDesc description);

internal sealed class Direct3D9DeviceBitmapColorSourceSharedHandleUpdater
{
    private readonly SurfaceDesc _destinationDescription;
    private readonly uint _levels;
    private readonly Direct3D9GetBitmapSurfaceDevice _getSourceDevice;
    private readonly Direct3D9OpenSharedBitmapTexture _openSharedTexture;
    private readonly Direct3D9GetBitmapTextureSurfaceLevel _getSurfaceLevel;
    private readonly Direct3D9StretchSharedBitmapRectangle _stretchRectangle;
    private readonly Direct3D9FlushSharedBitmapSurface _flush;
    private readonly Action<nint> _releaseSurface;
    private readonly Action<nint> _releaseTexture;
    private readonly Action<nint> _releaseDevice;
    private nint _sharedHandle;

    internal Direct3D9DeviceBitmapColorSourceSharedHandleUpdater(
        nint sharedHandle,
        SurfaceDesc destinationDescription,
        uint levels,
        Direct3D9GetBitmapSurfaceDevice getSourceDevice,
        Direct3D9OpenSharedBitmapTexture openSharedTexture,
        Direct3D9GetBitmapTextureSurfaceLevel getSurfaceLevel,
        Direct3D9StretchSharedBitmapRectangle stretchRectangle,
        Direct3D9FlushSharedBitmapSurface flush,
        Action<nint> releaseSurface,
        Action<nint> releaseTexture,
        Action<nint> releaseDevice)
    {
        ArgumentOutOfRangeException.ThrowIfZero(sharedHandle);
        ArgumentOutOfRangeException.ThrowIfZero(levels);
        ArgumentNullException.ThrowIfNull(getSourceDevice);
        ArgumentNullException.ThrowIfNull(openSharedTexture);
        ArgumentNullException.ThrowIfNull(getSurfaceLevel);
        ArgumentNullException.ThrowIfNull(stretchRectangle);
        ArgumentNullException.ThrowIfNull(flush);
        ArgumentNullException.ThrowIfNull(releaseSurface);
        ArgumentNullException.ThrowIfNull(releaseTexture);
        ArgumentNullException.ThrowIfNull(releaseDevice);

        _sharedHandle = sharedHandle;
        _destinationDescription = destinationDescription;
        _levels = levels;
        _getSourceDevice = getSourceDevice;
        _openSharedTexture = openSharedTexture;
        _getSurfaceLevel = getSurfaceLevel;
        _stretchRectangle = stretchRectangle;
        _flush = flush;
        _releaseSurface = releaseSurface;
        _releaseTexture = releaseTexture;
        _releaseDevice = releaseDevice;
    }

    internal int Update(
        ReadOnlySpan<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
        nint sourceSurface)
    {
        ArgumentOutOfRangeException.ThrowIfZero(dirtyRectangles.Length);
        ArgumentOutOfRangeException.ThrowIfZero(sourceSurface);

        nint sourceDevice = 0;
        nint destinationTexture = 0;
        nint destinationSurface = 0;

        try
        {
            int result = _getSourceDevice(sourceSurface, out sourceDevice);
            if (result < 0)
            {
                return result;
            }

            if (sourceDevice == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            result = _openSharedTexture(
                sourceDevice,
                _destinationDescription,
                _levels,
                ref _sharedHandle,
                out destinationTexture);
            if (result < 0)
            {
                return result;
            }

            if (destinationTexture == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            result = _getSurfaceLevel(destinationTexture, 0, out destinationSurface);
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
                    sourceDevice,
                    sourceSurface,
                    rectangle,
                    destinationSurface,
                    rectangle,
                    Texturefiltertype.None);
                if (result < 0)
                {
                    return result;
                }
            }

            return _flush(sourceDevice, destinationSurface, _destinationDescription);
        }
        finally
        {
            if (destinationSurface != 0)
            {
                _releaseSurface(destinationSurface);
            }

            if (destinationTexture != 0)
            {
                _releaseTexture(destinationTexture);
            }

            if (sourceDevice != 0)
            {
                _releaseDevice(sourceDevice);
            }
        }
    }
}
