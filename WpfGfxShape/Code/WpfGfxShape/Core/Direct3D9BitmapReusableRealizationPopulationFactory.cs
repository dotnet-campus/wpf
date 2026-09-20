namespace WpfGfxShape.Core;

internal sealed class Direct3D9BitmapReusableRealizationPopulationFactory
{
    private readonly Func<nint, Direct3D9BitmapRealizationRectangle> _resolveDestinationPrefilteredBitmap;
    private readonly Direct3D9TryGetValidReusableSourceRectangles _tryGetValidSourceRectangles;
    private readonly Func<nint, int> _ensureRealization;
    private readonly Direct3D9TryGetReusableSurface _tryGetSourceSurface;
    private readonly Func<nint, (int HResult, nint Surface)> _tryGetDestinationSurface;
    private readonly Direct3D9StretchReusableSurfaceRectangle _stretchRectangle;
    private readonly Action<nint> _releaseSurface;
    private readonly Func<nint, nint> _getNext;
    private readonly Direct3D9TryGetReusableSourcePrefilteredBitmap _tryGetSourcePrefilteredBitmap;

    internal Direct3D9BitmapReusableRealizationPopulationFactory(
        Direct3D9Device device,
        Direct3D9BitmapColorSourceRegistry registry,
        Direct3D9GetDeviceBitmapValidSourceRectangles getDeviceBitmapValidSourceRectangles,
        Func<nint, nint> getNext)
        : this(
            device,
            bitmapColorSource => registry.Resolve(bitmapColorSource).RealizationState,
            bitmapColorSource => registry.Resolve(bitmapColorSource).TextureRealizer,
            bitmapColorSource => registry.Resolve(bitmapColorSource).Texture,
            bitmapColorSource => registry.Resolve(bitmapColorSource).IsDeviceBitmap,
            getDeviceBitmapValidSourceRectangles,
            getNext)
    {
        ArgumentNullException.ThrowIfNull(registry);
    }

    internal Direct3D9BitmapReusableRealizationPopulationFactory(
        Direct3D9Device device,
        Func<nint, Direct3D9BitmapColorSourceRealizationState> resolveRealizationState,
        Func<nint, Direct3D9BitmapColorSourceTextureRealizer> resolveTextureRealizer,
        Func<nint, Direct3D9Texture> resolveTexture,
        Func<nint, bool> isDeviceBitmap,
        Direct3D9GetDeviceBitmapValidSourceRectangles getDeviceBitmapValidSourceRectangles,
        Func<nint, nint> getNext)
        : this(
            bitmapColorSource => resolveRealizationState(bitmapColorSource).PrefilteredBitmap,
            (nint source, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
                resolveRealizationState(source).GetValidSourceRectangles(
                    isDeviceBitmap(source),
                    getDeviceBitmapValidSourceRectangles,
                    out rectangles),
            source => resolveTextureRealizer(source).Realize(),
            (nint source, out nint surface) => resolveTexture(source).TryGetSurfaceLevelReference(0, out surface),
            bitmapColorSource =>
            {
                int result = resolveTexture(bitmapColorSource).TryGetSurfaceLevelReference(0, out nint surface);
                return (result, surface);
            },
            device.StretchRect,
            Direct3D9Factory.Release,
            getNext,
            (nint source, out Direct3D9BitmapRealizationRectangle prefilteredBitmap) =>
            {
                prefilteredBitmap = resolveRealizationState(source).PrefilteredBitmap;
                return true;
            })
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(resolveRealizationState);
        ArgumentNullException.ThrowIfNull(resolveTextureRealizer);
        ArgumentNullException.ThrowIfNull(resolveTexture);
        ArgumentNullException.ThrowIfNull(isDeviceBitmap);
        ArgumentNullException.ThrowIfNull(getDeviceBitmapValidSourceRectangles);
        ArgumentNullException.ThrowIfNull(getNext);
    }

    internal Direct3D9BitmapReusableRealizationPopulationFactory(
        Func<nint, Direct3D9BitmapRealizationRectangle> resolveDestinationPrefilteredBitmap,
        Direct3D9TryGetValidReusableSourceRectangles tryGetValidSourceRectangles,
        Func<nint, int> ensureRealization,
        Direct3D9TryGetReusableSurface tryGetSourceSurface,
        Func<nint, (int HResult, nint Surface)> tryGetDestinationSurface,
        Direct3D9StretchReusableSurfaceRectangle stretchRectangle,
        Action<nint> releaseSurface,
        Func<nint, nint> getNext,
        Direct3D9TryGetReusableSourcePrefilteredBitmap tryGetSourcePrefilteredBitmap)
    {
        ArgumentNullException.ThrowIfNull(resolveDestinationPrefilteredBitmap);
        ArgumentNullException.ThrowIfNull(tryGetValidSourceRectangles);
        ArgumentNullException.ThrowIfNull(ensureRealization);
        ArgumentNullException.ThrowIfNull(tryGetSourceSurface);
        ArgumentNullException.ThrowIfNull(tryGetDestinationSurface);
        ArgumentNullException.ThrowIfNull(stretchRectangle);
        ArgumentNullException.ThrowIfNull(releaseSurface);
        ArgumentNullException.ThrowIfNull(getNext);
        ArgumentNullException.ThrowIfNull(tryGetSourcePrefilteredBitmap);

        _resolveDestinationPrefilteredBitmap = resolveDestinationPrefilteredBitmap;
        _tryGetValidSourceRectangles = tryGetValidSourceRectangles;
        _ensureRealization = ensureRealization;
        _tryGetSourceSurface = tryGetSourceSurface;
        _tryGetDestinationSurface = tryGetDestinationSurface;
        _stretchRectangle = stretchRectangle;
        _releaseSurface = releaseSurface;
        _getNext = getNext;
        _tryGetSourcePrefilteredBitmap = tryGetSourcePrefilteredBitmap;
    }

    internal Direct3D9BitmapReusableRealizationPopulation Create(nint bitmapColorSource) =>
        new(
            new Direct3D9BitmapReusableRealizationUpdater(
                _resolveDestinationPrefilteredBitmap(bitmapColorSource),
                _tryGetValidSourceRectangles,
                _ensureRealization,
                _tryGetSourceSurface,
                () => _tryGetDestinationSurface(bitmapColorSource),
                _stretchRectangle,
                _releaseSurface),
            _getNext,
            _tryGetSourcePrefilteredBitmap);
}
