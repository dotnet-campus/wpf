namespace WpfGfxShape.Core;

internal delegate bool Direct3D9GetDependentBitBltDirtyRectangles(
    out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
    out uint newestUniquenessToken);

internal delegate int Direct3D9UpdateDependentBitBltSurface(
    ReadOnlySpan<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
    nint sourceSurface);

internal sealed class Direct3D9DependentBitBltColorSourceRealizer
{
    private readonly Direct3D9BitmapColorSourceRealizationState _realizationState;
    private readonly Direct3D9BitmapRealizationRectangle _fullBitmapBounds;
    private readonly Direct3D9GetDependentBitBltDirtyRectangles _getDirtyRectangles;
    private readonly Func<nint> _getPrimaryTransferSurface;
    private readonly Direct3D9UpdateDependentBitBltSurface _updateSurface;
    private readonly bool _isDependent;

    internal Direct3D9DependentBitBltColorSourceRealizer(
        Direct3D9BitmapColorSourceRealizationState realizationState,
        uint bitmapWidth,
        uint bitmapHeight,
        bool isDependent,
        Direct3D9GetDependentBitBltDirtyRectangles getDirtyRectangles,
        Func<nint> getPrimaryTransferSurface,
        Direct3D9UpdateDependentBitBltSurface updateSurface)
    {
        ArgumentNullException.ThrowIfNull(realizationState);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapWidth);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapHeight);
        ArgumentNullException.ThrowIfNull(getDirtyRectangles);
        ArgumentNullException.ThrowIfNull(getPrimaryTransferSurface);
        ArgumentNullException.ThrowIfNull(updateSurface);

        _realizationState = realizationState;
        _fullBitmapBounds = new Direct3D9BitmapRealizationRectangle(0, 0, bitmapWidth, bitmapHeight);
        _isDependent = isDependent;
        _getDirtyRectangles = getDirtyRectangles;
        _getPrimaryTransferSurface = getPrimaryTransferSurface;
        _updateSurface = updateSurface;
    }

    internal int Realize()
    {
        if (!_isDependent || _realizationState.IsRealizationValid())
        {
            return Direct3D9Factory.SuccessHResult;
        }

        bool dirtyRectanglesAreValid = _getDirtyRectangles(
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
            out uint newestUniquenessToken);
        ArgumentNullException.ThrowIfNull(dirtyRectangles);

        Direct3D9BitmapRealizationRectangle[]? fullBitmapRectangle = null;
        if (!dirtyRectanglesAreValid)
        {
            fullBitmapRectangle = [_fullBitmapBounds];
            dirtyRectangles = fullBitmapRectangle;
        }

        nint sourceSurface = _getPrimaryTransferSurface();
        if (sourceSurface == 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        Direct3D9BitmapRealizationRectangle[] updateRectangles = dirtyRectangles as Direct3D9BitmapRealizationRectangle[]
            ?? [.. dirtyRectangles];
        int result = _updateSurface(updateRectangles, sourceSurface);
        if (result < 0)
        {
            return result;
        }

        _realizationState.Commit(newestUniquenessToken, _realizationState.RequiredRealizationBounds);
        return result;
    }
}
