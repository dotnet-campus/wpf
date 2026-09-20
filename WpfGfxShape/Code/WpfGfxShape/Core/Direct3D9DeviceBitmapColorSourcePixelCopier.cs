namespace WpfGfxShape.Core;

internal delegate int Direct3D9GetBitmapColorSourceSurface(out nint surface);

internal delegate int Direct3D9ReadBitmapColorSourceSurface(
    nint surface,
    Direct3D9BitmapRealizationRectangle sourceRectangle,
    IReadOnlyList<Direct3D9BitmapRealizationRectangle> clipRectangles,
    MilPixelFormat outputFormat,
    uint outputStride,
    uint outputBufferSize,
    nint outputBuffer);

internal sealed class Direct3D9DeviceBitmapColorSourcePixelCopier
{
    private readonly Direct3D9Device _device;
    private readonly Direct3D9BitmapRealizationRectangle _prefilteredBitmapBounds;
    private readonly Direct3D9BitmapRealizationRectangle _cachedRealizationBounds;
    private readonly Direct3D9GetBitmapColorSourceSurface _getSourceSurface;
    private readonly Direct3D9ReadBitmapColorSourceSurface _readSurface;
    private readonly Action<nint> _releaseSurface;

    internal Direct3D9DeviceBitmapColorSourcePixelCopier(
        Direct3D9Device device,
        Direct3D9BitmapRealizationRectangle prefilteredBitmapBounds,
        Direct3D9BitmapRealizationRectangle cachedRealizationBounds,
        Direct3D9GetBitmapColorSourceSurface getSourceSurface,
        Direct3D9ReadBitmapColorSourceSurface readSurface,
        Action<nint> releaseSurface)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(getSourceSurface);
        ArgumentNullException.ThrowIfNull(readSurface);
        ArgumentNullException.ThrowIfNull(releaseSurface);

        _device = device;
        _prefilteredBitmapBounds = prefilteredBitmapBounds;
        _cachedRealizationBounds = cachedRealizationBounds;
        _getSourceSurface = getSourceSurface;
        _readSurface = readSurface;
        _releaseSurface = releaseSurface;
    }

    internal int CopyPixels(
        Direct3D9BitmapRealizationRectangle copyRectangle,
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> clipRectangles,
        MilPixelFormat outputFormat,
        uint outputBufferSize,
        nint outputBuffer,
        uint outputStride)
    {
        ArgumentNullException.ThrowIfNull(clipRectangles);
        ArgumentOutOfRangeException.ThrowIfZero(outputBuffer);

        using Direct3D9DeviceEntryGuard deviceEntry = new(_device);

        Direct3D9BitmapRealizationRectangle validCopy = _cachedRealizationBounds;
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> remainingClipRectangles = clipRectangles;
        if (clipRectangles.Count == 1)
        {
            if (!TryIntersect(validCopy, clipRectangles[0], out validCopy))
            {
                return Direct3D9Factory.SuccessHResult;
            }

            remainingClipRectangles = Array.Empty<Direct3D9BitmapRealizationRectangle>();
        }

        if (!TryIntersect(validCopy, copyRectangle, out validCopy))
        {
            return Direct3D9Factory.SuccessHResult;
        }

        byte bitsPerPixel = MilPixelFormatInfo.GetBitsPerPixel(outputFormat);
        if (bitsPerPixel == 0 || (bitsPerPixel % 8) != 0)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        using Direct3D9UseContextGuard useContext = new(_device);

        ulong bufferInset = ((ulong) outputStride * (validCopy.Top - copyRectangle.Top))
            + (((ulong) bitsPerPixel / 8) * (validCopy.Left - copyRectangle.Left));
        if (bufferInset > outputBufferSize || bufferInset > (ulong) nint.MaxValue)
        {
            return Direct3D9Factory.ArithmeticOverflowHResult;
        }

        nint sourceSurface = 0;
        try
        {
            int result = _getSourceSurface(out sourceSurface);
            if (result < 0)
            {
                return result;
            }

            if (sourceSurface == 0)
            {
                return Direct3D9Factory.GenericFailureHResult;
            }

            Direct3D9BitmapRealizationRectangle surfaceRectangle = new(
                validCopy.Left - _prefilteredBitmapBounds.Left,
                validCopy.Top - _prefilteredBitmapBounds.Top,
                validCopy.Right - _prefilteredBitmapBounds.Left,
                validCopy.Bottom - _prefilteredBitmapBounds.Top);

            return _readSurface(
                sourceSurface,
                surfaceRectangle,
                remainingClipRectangles,
                outputFormat,
                outputStride,
                outputBufferSize - (uint) bufferInset,
                outputBuffer + (nint) bufferInset);
        }
        finally
        {
            if (sourceSurface != 0)
            {
                _releaseSurface(sourceSurface);
            }
        }
    }

    private static bool TryIntersect(
        Direct3D9BitmapRealizationRectangle first,
        Direct3D9BitmapRealizationRectangle second,
        out Direct3D9BitmapRealizationRectangle intersection)
    {
        uint left = Math.Max(first.Left, second.Left);
        uint top = Math.Max(first.Top, second.Top);
        uint right = Math.Min(first.Right, second.Right);
        uint bottom = Math.Min(first.Bottom, second.Bottom);
        intersection = left < right && top < bottom
            ? new Direct3D9BitmapRealizationRectangle(left, top, right, bottom)
            : default;
        return left < right && top < bottom;
    }
}
