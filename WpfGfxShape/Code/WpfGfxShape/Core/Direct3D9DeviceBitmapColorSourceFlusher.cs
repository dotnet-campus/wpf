using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9CreateBitmapFlushSurface(
    uint width,
    uint height,
    Format format,
    MultisampleType multisampleType,
    uint multisampleQuality,
    bool lockable,
    out nint surface);

internal delegate int Direct3D9StretchBitmapFlushSurface(
    nint sourceSurface,
    Direct3D9SurfaceRect sourceRectangle,
    nint destinationSurface,
    Direct3D9SurfaceRect destinationRectangle,
    Texturefiltertype filter);

internal delegate int Direct3D9LockBitmapFlushSurface(
    nint surface,
    Direct3D9SurfaceRect rectangle,
    uint flags);

internal sealed class Direct3D9DeviceBitmapColorSourceFlusher
{
    private const uint MaximumFlushDimension = 16;

    private readonly Direct3D9CreateBitmapFlushSurface _createRenderTarget;
    private readonly Direct3D9StretchBitmapFlushSurface _stretchRectangle;
    private readonly Direct3D9LockBitmapFlushSurface _lockRectangle;
    private readonly Func<nint, int> _unlockRectangle;
    private readonly Action<nint> _releaseSurface;

    internal Direct3D9DeviceBitmapColorSourceFlusher(
        Direct3D9CreateBitmapFlushSurface createRenderTarget,
        Direct3D9StretchBitmapFlushSurface stretchRectangle,
        Direct3D9LockBitmapFlushSurface lockRectangle,
        Func<nint, int> unlockRectangle,
        Action<nint> releaseSurface)
    {
        ArgumentNullException.ThrowIfNull(createRenderTarget);
        ArgumentNullException.ThrowIfNull(stretchRectangle);
        ArgumentNullException.ThrowIfNull(lockRectangle);
        ArgumentNullException.ThrowIfNull(unlockRectangle);
        ArgumentNullException.ThrowIfNull(releaseSurface);

        _createRenderTarget = createRenderTarget;
        _stretchRectangle = stretchRectangle;
        _lockRectangle = lockRectangle;
        _unlockRectangle = unlockRectangle;
        _releaseSurface = releaseSurface;
    }

    internal int Flush(nint sourceSurface, SurfaceDesc sourceDescription)
    {
        ArgumentOutOfRangeException.ThrowIfZero(sourceSurface);

        uint width = Math.Min(MaximumFlushDimension, sourceDescription.Width);
        uint height = Math.Min(MaximumFlushDimension, sourceDescription.Height);
        Direct3D9SurfaceRect copyRectangle = new(0, 0, checked((int) width), checked((int) height));
        Direct3D9SurfaceRect flushRectangle = new(0, 0, 1, 1);
        nint flushSurface = 0;

        try
        {
            int result = _createRenderTarget(
                width,
                height,
                sourceDescription.Format,
                MultisampleType.MultisampleNone,
                0,
                true,
                out flushSurface);
            if (result < 0)
            {
                return result;
            }

            if (flushSurface == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            result = _stretchRectangle(
                sourceSurface,
                copyRectangle,
                flushSurface,
                copyRectangle,
                Texturefiltertype.None);
            if (result < 0)
            {
                return result;
            }

            return _lockRectangle(flushSurface, flushRectangle, D3D9.LockReadonly);
        }
        finally
        {
            if (flushSurface != 0)
            {
                _ = _unlockRectangle(flushSurface);
                _releaseSurface(flushSurface);
            }
        }
    }
}
