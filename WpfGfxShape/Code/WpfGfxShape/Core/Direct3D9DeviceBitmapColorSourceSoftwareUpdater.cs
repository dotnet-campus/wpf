using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9GetBitmapSurfaceDescription(nint surface, out SurfaceDesc description);

internal delegate int Direct3D9CreateBitmapSystemMemoryTexture(
    SurfaceDesc description,
    out nint texture);

internal delegate int Direct3D9LockBitmapSystemMemoryTexture(
    nint texture,
    out LockedRect lockedRectangle,
    uint flags);

internal delegate int Direct3D9ReadBitmapRenderTarget(
    nint sourceSurface,
    Direct3D9BitmapRealizationRectangle sourceRectangle,
    MilPixelFormat outputFormat,
    uint outputStride,
    uint outputBufferSize,
    nint outputBuffer);

internal delegate int Direct3D9AddBitmapTextureDirtyRectangle(
    nint texture,
    Direct3D9SurfaceRect dirtyRectangle);

internal delegate int Direct3D9UnlockBitmapSystemMemoryTexture(nint texture);

internal delegate int Direct3D9UpdateBitmapTexture(
    nint systemMemorySourceTexture,
    nint destinationTexture);

internal sealed unsafe class Direct3D9DeviceBitmapColorSourceSoftwareUpdater : IDisposable
{
    private readonly nint _destinationTexture;
    private readonly MilPixelFormat _textureFormat;
    private readonly uint _bitmapHeight;
    private readonly Direct3D9GetBitmapSurfaceDescription _getSourceDescription;
    private readonly Direct3D9CreateBitmapSystemMemoryTexture _createSystemMemoryTexture;
    private readonly Direct3D9LockBitmapSystemMemoryTexture _lockTexture;
    private readonly Direct3D9ReadBitmapRenderTarget _readRenderTarget;
    private readonly Direct3D9AddBitmapTextureDirtyRectangle _addDirtyRectangle;
    private readonly Direct3D9UnlockBitmapSystemMemoryTexture _unlockTexture;
    private readonly Direct3D9UpdateBitmapTexture _updateTexture;
    private readonly Action<nint> _releaseTexture;
    private nint _systemMemoryTexture;
    private bool _isDisposed;

    internal Direct3D9DeviceBitmapColorSourceSoftwareUpdater(
        nint destinationTexture,
        MilPixelFormat textureFormat,
        uint bitmapHeight,
        Direct3D9GetBitmapSurfaceDescription getSourceDescription,
        Direct3D9CreateBitmapSystemMemoryTexture createSystemMemoryTexture,
        Direct3D9LockBitmapSystemMemoryTexture lockTexture,
        Direct3D9ReadBitmapRenderTarget readRenderTarget,
        Direct3D9AddBitmapTextureDirtyRectangle addDirtyRectangle,
        Direct3D9UnlockBitmapSystemMemoryTexture unlockTexture,
        Direct3D9UpdateBitmapTexture updateTexture,
        Action<nint> releaseTexture)
    {
        ArgumentOutOfRangeException.ThrowIfZero(destinationTexture);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapHeight);
        ArgumentNullException.ThrowIfNull(getSourceDescription);
        ArgumentNullException.ThrowIfNull(createSystemMemoryTexture);
        ArgumentNullException.ThrowIfNull(lockTexture);
        ArgumentNullException.ThrowIfNull(readRenderTarget);
        ArgumentNullException.ThrowIfNull(addDirtyRectangle);
        ArgumentNullException.ThrowIfNull(unlockTexture);
        ArgumentNullException.ThrowIfNull(updateTexture);
        ArgumentNullException.ThrowIfNull(releaseTexture);

        _destinationTexture = destinationTexture;
        _textureFormat = textureFormat;
        _bitmapHeight = bitmapHeight;
        _getSourceDescription = getSourceDescription;
        _createSystemMemoryTexture = createSystemMemoryTexture;
        _lockTexture = lockTexture;
        _readRenderTarget = readRenderTarget;
        _addDirtyRectangle = addDirtyRectangle;
        _unlockTexture = unlockTexture;
        _updateTexture = updateTexture;
        _releaseTexture = releaseTexture;
    }

    internal int Update(
        ReadOnlySpan<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
        nint sourceSurface)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfZero(dirtyRectangles.Length);
        ArgumentOutOfRangeException.ThrowIfZero(sourceSurface);

        int result = EnsureSystemMemoryTexture(sourceSurface);
        if (result < 0)
        {
            return result;
        }

        result = _lockTexture(
            _systemMemoryTexture,
            out LockedRect lockedRectangle,
            (uint) D3D9.LockNoDirtyUpdate);
        if (result < 0)
        {
            return result;
        }

        bool unlockNeeded = true;
        try
        {
            if (lockedRectangle.Pitch <= 0 || lockedRectangle.PBits is null)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            uint stride = checked((uint) lockedRectangle.Pitch);
            ulong bufferSize = (ulong) _bitmapHeight * stride;
            if (bufferSize > uint.MaxValue)
            {
                return Direct3D9Factory.ArithmeticOverflowHResult;
            }

            foreach (Direct3D9BitmapRealizationRectangle dirtyRectangle in dirtyRectangles)
            {
                result = _readRenderTarget(
                    sourceSurface,
                    dirtyRectangle,
                    _textureFormat,
                    stride,
                    (uint) bufferSize,
                    (nint) lockedRectangle.PBits);
                if (result < 0)
                {
                    return result;
                }

                Direct3D9SurfaceRect rectangle = new(
                    checked((int) dirtyRectangle.Left),
                    checked((int) dirtyRectangle.Top),
                    checked((int) dirtyRectangle.Right),
                    checked((int) dirtyRectangle.Bottom));
                result = _addDirtyRectangle(_systemMemoryTexture, rectangle);
                if (result < 0)
                {
                    return result;
                }
            }

            result = _unlockTexture(_systemMemoryTexture);
            if (result < 0)
            {
                return result;
            }

            unlockNeeded = false;
            return _updateTexture(_systemMemoryTexture, _destinationTexture);
        }
        finally
        {
            if (unlockNeeded)
            {
                _ = _unlockTexture(_systemMemoryTexture);
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        if (_systemMemoryTexture != 0)
        {
            _releaseTexture(_systemMemoryTexture);
            _systemMemoryTexture = 0;
        }
    }

    private int EnsureSystemMemoryTexture(nint sourceSurface)
    {
        if (_systemMemoryTexture != 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        int result = _getSourceDescription(sourceSurface, out SurfaceDesc description);
        if (result < 0)
        {
            return result;
        }

        description.Usage = 0;
        description.Pool = Pool.Systemmem;
        description.MultiSampleType = MultisampleType.MultisampleNone;
        description.MultiSampleQuality = 0;

        nint texture = 0;
        result = _createSystemMemoryTexture(description, out texture);
        if (result < 0)
        {
            if (texture != 0)
            {
                _releaseTexture(texture);
            }

            return result;
        }

        if (texture == 0)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        _systemMemoryTexture = texture;
        return Direct3D9Factory.SuccessHResult;
    }
}
