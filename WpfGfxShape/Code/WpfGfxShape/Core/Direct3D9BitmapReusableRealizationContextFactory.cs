namespace WpfGfxShape.Core;

internal sealed class Direct3D9BitmapReusableRealizationContextFactory
{
    private readonly bool _isDeviceBitmap;
    private readonly Func<nint, bool> _hasContributorFromDifferentAdapter;
    private readonly Func<nint, Direct3D9BitmapReusableRealizationSources> _createSources;
    private readonly Func<nint, Direct3D9BitmapReusableRealizationPopulation> _createPopulation;

    internal Direct3D9BitmapReusableRealizationContextFactory(
        bool isDeviceBitmap,
        Func<bool> hasContributorFromDifferentAdapter,
        Func<Direct3D9BitmapReusableRealizationSources> createSources,
        Func<Direct3D9BitmapReusableRealizationPopulation> createPopulation)
        : this(
            isDeviceBitmap,
            _ => hasContributorFromDifferentAdapter(),
            _ => createSources(),
            _ => createPopulation())
    {
        ArgumentNullException.ThrowIfNull(createSources);
        ArgumentNullException.ThrowIfNull(createPopulation);
    }

    internal Direct3D9BitmapReusableRealizationContextFactory(
        bool isDeviceBitmap,
        Func<bool> hasContributorFromDifferentAdapter,
        Func<nint, Direct3D9BitmapReusableRealizationSources> createSources,
        Func<nint, Direct3D9BitmapReusableRealizationPopulation> createPopulation)
        : this(
            isDeviceBitmap,
            _ => hasContributorFromDifferentAdapter(),
            createSources,
            createPopulation)
    {
        ArgumentNullException.ThrowIfNull(hasContributorFromDifferentAdapter);
    }

    internal Direct3D9BitmapReusableRealizationContextFactory(
        bool isDeviceBitmap,
        Func<nint, bool> hasContributorFromDifferentAdapter,
        Direct3D9Device device,
        Direct3D9BitmapColorSourceRegistry registry,
        bool canStretchRectFromTextures,
        Direct3D9GetDeviceBitmapValidSourceRectangles getDeviceBitmapValidSourceRectangles,
        Func<nint, nint> getNext,
        Action<nint, nint> setNext,
        Action<nint> addReference,
        Action<nint> release)
        : this(
            isDeviceBitmap,
            hasContributorFromDifferentAdapter,
            registry,
            canStretchRectFromTextures,
            getNext,
            setNext,
            addReference,
            release,
            new Direct3D9BitmapReusableRealizationPopulationFactory(
                device,
                registry,
                getDeviceBitmapValidSourceRectangles,
                getNext))
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(getDeviceBitmapValidSourceRectangles);
    }

    internal Direct3D9BitmapReusableRealizationContextFactory(
        bool isDeviceBitmap,
        Func<nint, bool> hasContributorFromDifferentAdapter,
        Direct3D9BitmapColorSourceRegistry registry,
        bool canStretchRectFromTextures,
        Func<nint, nint> getNext,
        Action<nint, nint> setNext,
        Action<nint> addReference,
        Action<nint> release,
        Direct3D9BitmapReusableRealizationPopulationFactory populationFactory)
        : this(
            isDeviceBitmap,
            hasContributorFromDifferentAdapter,
            bitmapColorSource => CreateRegistrySources(
                registry,
                bitmapColorSource,
                canStretchRectFromTextures,
                getNext,
                setNext,
                addReference,
                release),
            populationFactory.Create)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(populationFactory);
    }

    internal Direct3D9BitmapReusableRealizationContextFactory(
        bool isDeviceBitmap,
        Func<nint, bool> hasContributorFromDifferentAdapter,
        Func<nint, Direct3D9BitmapReusableRealizationTargetState> resolveTargetState,
        Direct3D9TryGetReusableRealizationSourceState tryGetSourceState,
        Func<nint, nint> getNext,
        Action<nint, nint> setNext,
        Action<nint> addReference,
        Action<nint> release,
        Action<nint> tryAdoptSystemMemorySurface,
        Func<nint, Direct3D9BitmapReusableRealizationPopulation> createPopulation)
        : this(
            isDeviceBitmap,
            hasContributorFromDifferentAdapter,
            bitmapColorSource => new Direct3D9BitmapReusableRealizationSources(
                resolveTargetState(bitmapColorSource),
                tryGetSourceState,
                getNext,
                setNext,
                addReference,
                release,
                tryAdoptSystemMemorySurface),
            createPopulation)
    {
        ArgumentNullException.ThrowIfNull(resolveTargetState);
        ArgumentNullException.ThrowIfNull(tryGetSourceState);
        ArgumentNullException.ThrowIfNull(getNext);
        ArgumentNullException.ThrowIfNull(setNext);
        ArgumentNullException.ThrowIfNull(addReference);
        ArgumentNullException.ThrowIfNull(release);
        ArgumentNullException.ThrowIfNull(tryAdoptSystemMemorySurface);
    }

    internal Direct3D9BitmapReusableRealizationContextFactory(
        bool isDeviceBitmap,
        Func<nint, bool> hasContributorFromDifferentAdapter,
        Func<nint, Direct3D9BitmapReusableRealizationTargetState> resolveTargetState,
        Direct3D9TryGetReusableRealizationSourceState tryGetSourceState,
        Func<nint, nint> getNext,
        Action<nint, nint> setNext,
        Action<nint> addReference,
        Action<nint> release,
        Action<nint> tryAdoptSystemMemorySurface,
        Direct3D9BitmapReusableRealizationPopulationFactory populationFactory)
        : this(
            isDeviceBitmap,
            hasContributorFromDifferentAdapter,
            resolveTargetState,
            tryGetSourceState,
            getNext,
            setNext,
            addReference,
            release,
            tryAdoptSystemMemorySurface,
            populationFactory.Create)
    {
        ArgumentNullException.ThrowIfNull(populationFactory);
    }

    internal Direct3D9BitmapReusableRealizationContextFactory(
        bool isDeviceBitmap,
        Func<nint, bool> hasContributorFromDifferentAdapter,
        Func<nint, Direct3D9BitmapReusableRealizationSources> createSources,
        Func<nint, Direct3D9BitmapReusableRealizationPopulation> createPopulation)
    { 
        ArgumentNullException.ThrowIfNull(hasContributorFromDifferentAdapter);
        ArgumentNullException.ThrowIfNull(createSources);
        ArgumentNullException.ThrowIfNull(createPopulation);

        _isDeviceBitmap = isDeviceBitmap;
        _hasContributorFromDifferentAdapter = hasContributorFromDifferentAdapter;
        _createSources = createSources;
        _createPopulation = createPopulation;
    }

    private static Direct3D9BitmapReusableRealizationSources CreateRegistrySources(
        Direct3D9BitmapColorSourceRegistry registry,
        nint bitmapColorSource,
        bool canStretchRectFromTextures,
        Func<nint, nint> getNext,
        Action<nint, nint> setNext,
        Action<nint> addReference,
        Action<nint> release) =>
        registry.CreateReusableRealizationSources(
            bitmapColorSource,
            canStretchRectFromTextures,
            getNext,
            setNext,
            addReference,
            release);

    internal Direct3D9BitmapReusableRealizationContext Create(
        Direct3D9BitmapReusableRealizationCandidates candidates) =>
        Create(0, candidates);

    internal Direct3D9BitmapReusableRealizationContext Create(
        nint bitmapColorSource,
        Direct3D9BitmapReusableRealizationCandidates candidates) =>
        Create(bitmapColorSource, candidates, _isDeviceBitmap);

    internal Direct3D9BitmapReusableRealizationContext Create(
        nint bitmapColorSource,
        Direct3D9BitmapReusableRealizationCandidates candidates,
        bool isDeviceBitmap)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        Direct3D9BitmapReusableRealizationSources? sources = null;
        try
        {
            bool hasContributorFromDifferentAdapter =
                isDeviceBitmap && _hasContributorFromDifferentAdapter(bitmapColorSource);
            sources = _createSources(bitmapColorSource)
                ?? throw new InvalidOperationException("Reusable realization source creation returned null.");
            Direct3D9BitmapReusableRealizationPopulation population = _createPopulation(bitmapColorSource)
                ?? throw new InvalidOperationException("Reusable realization population creation returned null.");

            Direct3D9BitmapReusableRealizationContext context = new(
                hasContributorFromDifferentAdapter,
                candidates,
                sources,
                population);
            sources = null;
            candidates = null!;
            return context;
        }
        finally
        {
            sources?.Dispose();
            candidates?.Dispose();
        }
    }
}
