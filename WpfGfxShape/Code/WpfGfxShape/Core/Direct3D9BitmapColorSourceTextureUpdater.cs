namespace WpfGfxShape.Core;

internal delegate bool Direct3D9GetBitmapDirtyRectangles(
    out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
    out uint newestUniquenessToken);

internal sealed unsafe class Direct3D9BitmapColorSourceTextureUpdater
{
    private readonly Direct3D9BitmapColorSourceRealizationState _realizationState;
    private readonly Direct3D9BitmapTexturePopulator _texturePopulator;
    private readonly Direct3D9GetBitmapDirtyRectangles _getDirtyRectangles;

    internal Direct3D9BitmapColorSourceTextureUpdater(
        Direct3D9BitmapColorSourceRealizationState realizationState,
        Direct3D9BitmapTexturePopulator texturePopulator)
        : this(
            realizationState,
            texturePopulator,
            (out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles, out uint newestUniquenessToken) =>
                Direct3D9Bitmap.GetDirtyRectangles(
                    realizationState.Bitmap,
                    realizationState.CachedUniquenessToken,
                    out dirtyRectangles,
                    out newestUniquenessToken))
    {
    }

    internal Direct3D9BitmapColorSourceTextureUpdater(
        Direct3D9BitmapColorSourceRealizationState realizationState,
        Direct3D9BitmapTexturePopulator texturePopulator,
        Direct3D9GetBitmapDirtyRectangles getDirtyRectangles)
    {
        ArgumentNullException.ThrowIfNull(realizationState);
        ArgumentNullException.ThrowIfNull(texturePopulator);
        ArgumentNullException.ThrowIfNull(getDirtyRectangles);

        _realizationState = realizationState;
        _texturePopulator = texturePopulator;
        _getDirtyRectangles = getDirtyRectangles;
    }

    internal int Update()
    {
        bool dirtyRectanglesAreValid = _getDirtyRectangles(
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
            out uint newestUniquenessToken);
        if (dirtyRectangles is null)
        {
            throw new InvalidOperationException("The bitmap dirty rectangle provider returned a null collection.");
        }

        IReadOnlyList<Direct3D9BitmapRealizationRectangle> updateRectangles =
            _realizationState.GetUpdateRectangles(dirtyRectanglesAreValid, dirtyRectangles);
        Direct3D9BitmapRealizationRectangle[] rectangleBuffer = new Direct3D9BitmapRealizationRectangle[updateRectangles.Count];
        for (int i = 0; i < rectangleBuffer.Length; i++)
        {
            rectangleBuffer[i] = updateRectangles[i];
        }

        fixed (Direct3D9BitmapRealizationRectangle* rectanglePointer = rectangleBuffer)
        {
            return _texturePopulator.Populate(
                _realizationState.BitmapSource,
                checked((uint) rectangleBuffer.Length),
                rectangleBuffer.Length == 0 ? 0 : (nint) rectanglePointer,
                newestUniquenessToken,
                _realizationState.RequiredRealizationBounds);
        }
    }
}
