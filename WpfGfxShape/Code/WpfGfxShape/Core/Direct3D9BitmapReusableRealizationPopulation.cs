namespace WpfGfxShape.Core;

internal delegate bool Direct3D9TryGetReusableSourcePrefilteredBitmap(
    nint source,
    out Direct3D9BitmapRealizationRectangle prefilteredBitmap);

internal sealed class Direct3D9BitmapReusableRealizationPopulation
{
    private readonly Direct3D9BitmapReusableRealizationUpdater _updater;
    private readonly Func<nint, nint> _getNext;
    private readonly Direct3D9TryGetReusableSourcePrefilteredBitmap _tryGetPrefilteredBitmap;
    private readonly List<Direct3D9BitmapRealizationRectangle>[] _remainingBuffers = [[], []];

    internal Direct3D9BitmapReusableRealizationPopulation(
        Direct3D9BitmapReusableRealizationUpdater updater,
        Func<nint, nint> getNext,
        Direct3D9TryGetReusableSourcePrefilteredBitmap tryGetPrefilteredBitmap)
    {
        ArgumentNullException.ThrowIfNull(updater);
        ArgumentNullException.ThrowIfNull(getNext);
        ArgumentNullException.ThrowIfNull(tryGetPrefilteredBitmap);

        _updater = updater;
        _getNext = getNext;
        _tryGetPrefilteredBitmap = tryGetPrefilteredBitmap;
    }

    internal int UpdateFromSources(
        nint source,
        IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
        out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remainingRectangles)
    {
        ArgumentNullException.ThrowIfNull(dirtyRectangles);

        remainingRectangles = dirtyRectangles;
        int activeOutputBufferIndex = 0;

        while (source != 0)
        {
            if (!_tryGetPrefilteredBitmap(source, out Direct3D9BitmapRealizationRectangle prefilteredBitmap))
            {
                return Direct3D9Factory.InvalidCallHResult;
            }

            int result = _updater.UpdateFromReusableSource(
                source,
                prefilteredBitmap,
                remainingRectangles,
                _remainingBuffers,
                ref activeOutputBufferIndex,
                out remainingRectangles);
            if (result < 0)
            {
                return result;
            }

            source = _getNext(source);
        }

        return Direct3D9Factory.SuccessHResult;
    }
}
