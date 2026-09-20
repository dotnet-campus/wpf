namespace WpfGfxShape.Core;

internal delegate int Direct3D9TryGetValidReusableSourceRectangles(
    nint source,
    out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles);

internal delegate int Direct3D9TryGetReusableSurface(nint source, out nint surface);

internal delegate int Direct3D9StretchReusableSurfaceRectangle(
    nint sourceSurface,
    Direct3D9SurfaceRect sourceRectangle,
    nint destinationSurface,
    Direct3D9SurfaceRect destinationRectangle);

internal sealed class Direct3D9BitmapReusableRealizationUpdater
{
    private readonly Direct3D9BitmapRealizationRectangle _destinationPrefilteredBitmap;
    private readonly Direct3D9TryGetValidReusableSourceRectangles _tryGetValidSourceRectangles;
    private readonly Func<nint, int> _ensureRealization;
    private readonly Direct3D9TryGetReusableSurface _tryGetSourceSurface;
    private readonly Func<(int HResult, nint Surface)> _tryGetDestinationSurface;
    private readonly Direct3D9StretchReusableSurfaceRectangle _stretchRectangle;
    private readonly Action<nint> _releaseSurface;

    internal Direct3D9BitmapReusableRealizationUpdater(
        Direct3D9BitmapRealizationRectangle destinationPrefilteredBitmap,
        Direct3D9TryGetValidReusableSourceRectangles tryGetValidSourceRectangles,
        Func<nint, int> ensureRealization,
        Direct3D9TryGetReusableSurface tryGetSourceSurface,
        Func<(int HResult, nint Surface)> tryGetDestinationSurface,
        Direct3D9StretchReusableSurfaceRectangle stretchRectangle,
        Action<nint> releaseSurface)
    {
        ArgumentNullException.ThrowIfNull(tryGetValidSourceRectangles);
        ArgumentNullException.ThrowIfNull(ensureRealization);
        ArgumentNullException.ThrowIfNull(tryGetSourceSurface);
        ArgumentNullException.ThrowIfNull(tryGetDestinationSurface);
        ArgumentNullException.ThrowIfNull(stretchRectangle);
        ArgumentNullException.ThrowIfNull(releaseSurface);

        _destinationPrefilteredBitmap = destinationPrefilteredBitmap;
        _tryGetValidSourceRectangles = tryGetValidSourceRectangles;
        _ensureRealization = ensureRealization;
        _tryGetSourceSurface = tryGetSourceSurface;
        _tryGetDestinationSurface = tryGetDestinationSurface;
        _stretchRectangle = stretchRectangle;
        _releaseSurface = releaseSurface;
    }

    internal int UpdateFromReusableSource(
        nint source,
        Direct3D9BitmapRealizationRectangle sourcePrefilteredBitmap,
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
        out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remainingRectangles)
    {
        List<Direct3D9BitmapRealizationRectangle>[] remainingBuffers = [[], []];
        int activeOutputBufferIndex = 0;
        return UpdateFromReusableSource(
            source,
            sourcePrefilteredBitmap,
            dirtyRectangles,
            remainingBuffers,
            ref activeOutputBufferIndex,
            out remainingRectangles);
    }

    internal int UpdateFromReusableSource(
        nint source,
        Direct3D9BitmapRealizationRectangle sourcePrefilteredBitmap,
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
        IReadOnlyList<List<Direct3D9BitmapRealizationRectangle>> remainingBuffers,
        ref int activeOutputBufferIndex,
        out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remainingRectangles)
    {
        ArgumentOutOfRangeException.ThrowIfZero(source);
        ArgumentNullException.ThrowIfNull(dirtyRectangles);
        ArgumentNullException.ThrowIfNull(remainingBuffers);

        remainingRectangles = dirtyRectangles;
        if (remainingBuffers.Count < 2
            || activeOutputBufferIndex < 0
            || activeOutputBufferIndex >= remainingBuffers.Count)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        int result = _tryGetValidSourceRectangles(source, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> validRectangles);
        if (result < 0)
        {
            return result;
        }

        if (validRectangles is null)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        nint sourceSurface = 0;
        nint destinationSurface = 0;

        try
        {
            IReadOnlyList<Direct3D9BitmapRealizationRectangle> current = dirtyRectangles;
            bool surfacesPrepared = false;

            foreach (Direct3D9BitmapRealizationRectangle validRectangle in validRectangles)
            {
                if (current.Count == 0)
                {
                    break;
                }

                List<Direct3D9BitmapRealizationRectangle> next = remainingBuffers[activeOutputBufferIndex];
                next.Clear();
                foreach (Direct3D9BitmapRealizationRectangle dirtyRectangle in current)
                {
                    if (!TryIntersect(dirtyRectangle, validRectangle, out Direct3D9BitmapRealizationRectangle intersection))
                    {
                        next.Add(dirtyRectangle);
                        continue;
                    }

                    AddSubtractionRectangles(dirtyRectangle, intersection, next);

                    if (!surfacesPrepared)
                    {
                        result = _ensureRealization(source);
                        if (result < 0)
                        {
                            return result;
                        }

                        result = _tryGetSourceSurface(source, out sourceSurface);
                        if (result < 0)
                        {
                            return result;
                        }

                        if (sourceSurface == 0)
                        {
                            return Direct3D9Factory.InvalidCallHResult;
                        }

                        (result, destinationSurface) = _tryGetDestinationSurface();
                        if (result < 0)
                        {
                            return result;
                        }

                        if (destinationSurface == 0)
                        {
                            return Direct3D9Factory.InvalidCallHResult;
                        }

                        surfacesPrepared = true;
                    }

                    result = _stretchRectangle(
                        sourceSurface,
                        OffsetToSurface(intersection, sourcePrefilteredBitmap),
                        destinationSurface,
                        OffsetToSurface(intersection, _destinationPrefilteredBitmap));
                    if (result < 0)
                    {
                        return result;
                    }
                }

                current = next;
                activeOutputBufferIndex = (activeOutputBufferIndex + 1) % remainingBuffers.Count;
            }

            remainingRectangles = current;
            return Direct3D9Factory.SuccessHResult;
        }
        catch (OverflowException)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }
        finally
        {
            if (sourceSurface != 0)
            {
                _releaseSurface(sourceSurface);
            }

            if (destinationSurface != 0)
            {
                _releaseSurface(destinationSurface);
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
        intersection = new Direct3D9BitmapRealizationRectangle(left, top, right, bottom);
        return left < right && top < bottom;
    }

    private static void AddSubtractionRectangles(
        Direct3D9BitmapRealizationRectangle rectangle,
        Direct3D9BitmapRealizationRectangle intersection,
        List<Direct3D9BitmapRealizationRectangle> destination)
    {
        AddIfNotEmpty(destination, new(rectangle.Left, rectangle.Top, rectangle.Right, intersection.Top));
        AddIfNotEmpty(destination, new(rectangle.Left, intersection.Bottom, rectangle.Right, rectangle.Bottom));
        AddIfNotEmpty(destination, new(rectangle.Left, intersection.Top, intersection.Left, intersection.Bottom));
        AddIfNotEmpty(destination, new(intersection.Right, intersection.Top, rectangle.Right, intersection.Bottom));
    }

    private static void AddIfNotEmpty(
        List<Direct3D9BitmapRealizationRectangle> destination,
        Direct3D9BitmapRealizationRectangle rectangle)
    {
        if (rectangle.Left < rectangle.Right && rectangle.Top < rectangle.Bottom)
        {
            destination.Add(rectangle);
        }
    }

    private static Direct3D9SurfaceRect OffsetToSurface(
        Direct3D9BitmapRealizationRectangle rectangle,
        Direct3D9BitmapRealizationRectangle prefilteredBitmap) =>
        new(
            checked((int) rectangle.Left - (int) prefilteredBitmap.Left),
            checked((int) rectangle.Top - (int) prefilteredBitmap.Top),
            checked((int) rectangle.Right - (int) prefilteredBitmap.Left),
            checked((int) rectangle.Bottom - (int) prefilteredBitmap.Top));
}
