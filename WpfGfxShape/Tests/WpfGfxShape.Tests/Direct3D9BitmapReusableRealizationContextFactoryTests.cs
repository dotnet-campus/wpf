using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapReusableRealizationContextFactoryTests
{
    [TestMethod]
    public void WhenDeviceBitmapContextIsCreatedThenAdapterContributorStateIsCaptured()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        candidates.Add(1);
        Direct3D9BitmapReusableRealizationContextFactory factory = new(
            true,
            () =>
            {
                calls.Add("adapter");
                return true;
            },
            () => CreateSources(next, calls),
            () => CreatePopulation(next));

        using Direct3D9BitmapReusableRealizationContext context = factory.Create(candidates);

        Assert.AreEqual("True|adapter", $"{context.HasContributorFromDifferentAdapter}|{string.Join(',', calls)}");
    }

    [TestMethod]
    public void WhenContextIsCreatedThenSelectedColorSourceIsPassedToProductionDependencies()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        Direct3D9BitmapReusableRealizationContextFactory factory = new(
            false,
            () => throw new AssertFailedException(),
            colorSource =>
            {
                calls.Add($"sources:{colorSource}");
                return CreateSources(next, calls);
            },
            colorSource =>
            {
                calls.Add($"population:{colorSource}");
                return CreatePopulation(next);
            });

        using Direct3D9BitmapReusableRealizationContext context = factory.Create(17, candidates);

        Assert.AreEqual("sources:17,population:17", string.Join(',', calls));
    }

    [TestMethod]
    public void WhenDeviceBitmapContextIsCreatedThenSelectedColorSourceIsPassedToContributorQuery()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        Direct3D9BitmapReusableRealizationContextFactory factory = new(
            false,
            colorSource =>
            {
                calls.Add($"adapter:{colorSource}");
                return true;
            },
            colorSource =>
            {
                calls.Add($"sources:{colorSource}");
                return CreateSources(next, calls);
            },
            colorSource =>
            {
                calls.Add($"population:{colorSource}");
                return CreatePopulation(next);
            });

        using Direct3D9BitmapReusableRealizationContext context = factory.Create(17, candidates, true);

        Assert.AreEqual(
            "True|adapter:17,sources:17,population:17",
            $"{context.HasContributorFromDifferentAdapter}|{string.Join(',', calls)}");
    }

    [TestMethod]
    public void WhenProductionContextIsCreatedThenSelectedColorSourceBuildsSourcesFromResolvedState()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        candidates.Add(23);
        calls.Clear();
        Direct3D9BitmapReusableRealizationContextFactory factory = new(
            false,
            _ => throw new AssertFailedException(),
            colorSource =>
            {
                calls.Add($"target:{colorSource}");
                return new Direct3D9BitmapReusableRealizationTargetState(
                    7,
                    true,
                    true,
                    1,
                    1,
                    new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
                    3);
            },
            (nint source, out Direct3D9BitmapReusableRealizationSourceState state) =>
            {
                calls.Add($"state:{source}");
                state = new Direct3D9BitmapReusableRealizationSourceState(
                    8,
                    true,
                    true,
                    1,
                    1,
                    new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
                    2,
                    true,
                    false);
                return true;
            },
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            source => calls.Add($"release:{source}"),
            source => calls.Add($"adopt:{source}"),
            colorSource =>
            {
                calls.Add($"population:{colorSource}");
                return CreatePopulation(next);
            });

        using Direct3D9BitmapReusableRealizationContext context = factory.Create(17, candidates);
        context.Sources.Consume(context.Candidates);

        Assert.AreEqual(
            "target:17,population:17,state:23,adopt:23,release:23",
            string.Join(',', calls));
    }

    [TestMethod]
    public void WhenRegistryTargetIsNotRegisteredThenContextCreationReleasesCandidates()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        candidates.Add(1);
        calls.Clear();
        Direct3D9BitmapReusableRealizationContextFactory factory = CreateRegistryFactory(
            new Direct3D9BitmapColorSourceRegistry(),
            next);

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => factory.Create(17, candidates));

        Assert.AreEqual(
            "The bitmap color source is not registered.|candidate:1",
            $"{exception.Message}|{string.Join(',', calls)}");
    }

    [TestMethod]
    public void WhenRegistryTargetHasNotBeenRealizedThenContextCreationReleasesCandidates()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        candidates.Add(1);
        calls.Clear();
        Direct3D9BitmapColorSourceRegistry registry = new();
        Direct3D9BitmapColorSourceOwner owner = new(
            new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null)),
            isDeviceBitmap: false);
        registry.Register(17, owner);
        Direct3D9BitmapReusableRealizationContextFactory factory = CreateRegistryFactory(registry, next);

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => factory.Create(17, candidates));

        Assert.AreEqual(
            "The bitmap color source has not been realized.|candidate:1",
            $"{exception.Message}|{string.Join(',', calls)}");
    }

    [TestMethod]
    public void WhenRegistrySourceIsNotRegisteredThenConsumeReleasesCurrentAndRemainingCandidates()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        candidates.Add(1);
        candidates.Add(2);
        candidates.Add(3);
        calls.Clear();
        Direct3D9BitmapColorSourceRegistry registry = new();
        using Direct3D9BitmapReusableRealizationSources sources = CreateRegistrySourcesForConsume(registry, next, calls);

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => sources.Consume(candidates));

        Assert.AreEqual(
            "The bitmap color source is not registered.|candidate:3,candidate:2,candidate:1",
            $"{exception.Message}|{string.Join(',', calls)}");
    }

    [TestMethod]
    public void WhenRegistrySourceHasNotBeenRealizedThenConsumeReleasesCurrentAndRemainingCandidates()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        candidates.Add(1);
        candidates.Add(2);
        candidates.Add(3);
        calls.Clear();
        Direct3D9BitmapColorSourceRegistry registry = new();
        registry.Register(3, CreateUnrealizedOwner());
        using Direct3D9BitmapReusableRealizationSources sources = CreateRegistrySourcesForConsume(registry, next, calls);

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => sources.Consume(candidates));

        Assert.AreEqual(
            "The bitmap color source has not been realized.|candidate:3,candidate:2,candidate:1",
            $"{exception.Message}|{string.Join(',', calls)}");
    }

    [TestMethod]
    public void WhenSourceCreationFailsThenCandidatesAreReleased()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        candidates.Add(1);
        calls.Clear();
        Direct3D9BitmapReusableRealizationContextFactory factory = new(
            false,
            () => throw new AssertFailedException(),
            () => throw new InvalidOperationException("sources"),
            () => throw new AssertFailedException());

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => factory.Create(candidates));

        Assert.AreEqual("sources|candidate:1", $"{exception.Message}|{string.Join(',', calls)}");
    }

    [TestMethod]
    public void WhenPopulationCreationFailsThenSourcesAndCandidatesAreReleasedInReverseOrder()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = CreateCandidates(next, calls);
        candidates.Add(1);
        calls.Clear();
        Direct3D9BitmapReusableRealizationContextFactory factory = new(
            false,
            () => throw new AssertFailedException(),
            () => CreateOwnedSources(next, calls),
            () => throw new InvalidOperationException("population"));

        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => factory.Create(candidates));

        Assert.AreEqual("population|source:2,candidate:1", $"{exception.Message}|{string.Join(',', calls)}");
    }

    private static Direct3D9BitmapReusableRealizationSources CreateRegistrySourcesForConsume(
        Direct3D9BitmapColorSourceRegistry registry,
        Dictionary<nint, nint> next,
        List<string> calls) =>
        new(
            new Direct3D9BitmapReusableRealizationTargetState(
                0,
                true,
                true,
                1,
                1,
                new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
                0),
            (nint source, out Direct3D9BitmapReusableRealizationSourceState state) =>
            {
                state = registry.ResolveReusableSourceState(source);
                return true;
            },
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            source => calls.Add($"candidate:{source}"),
            _ => { });

    private static Direct3D9BitmapColorSourceOwner CreateUnrealizedOwner() =>
        new(
            new Direct3D9BitmapColorSourceTextureRealizer(
                () => (Direct3D9Factory.NotImplementedHResult, null)),
            isDeviceBitmap: false);

    private static Direct3D9BitmapReusableRealizationContextFactory CreateRegistryFactory(
        Direct3D9BitmapColorSourceRegistry registry,
        Dictionary<nint, nint> next)
    {
        Direct3D9BitmapReusableRealizationPopulationFactory populationFactory = new(
            _ => throw new AssertFailedException(),
            (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                rectangles = [];
                throw new AssertFailedException();
            },
            _ => throw new AssertFailedException(),
            (nint _, out nint surface) =>
            {
                surface = 0;
                throw new AssertFailedException();
            },
            _ => throw new AssertFailedException(),
            (_, _, _, _) => throw new AssertFailedException(),
            _ => throw new AssertFailedException(),
            source => next.GetValueOrDefault(source),
            (nint _, out Direct3D9BitmapRealizationRectangle bounds) =>
            {
                bounds = default;
                throw new AssertFailedException();
            });
        return new Direct3D9BitmapReusableRealizationContextFactory(
            false,
            _ => throw new AssertFailedException(),
            registry,
            canStretchRectFromTextures: true,
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            _ => { },
            populationFactory);
    }

    private static Direct3D9BitmapReusableRealizationCandidates CreateCandidates(
        Dictionary<nint, nint> next,
        List<string> calls) =>
        new(
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            source => calls.Add($"candidate:{source}"));

    private static Direct3D9BitmapReusableRealizationSources CreateSources(
        Dictionary<nint, nint> next,
        List<string> calls) =>
        new(
            new Direct3D9BitmapReusableRealizationTargetState(
                0,
                true,
                true,
                1,
                1,
                new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
                0),
            (nint source, out Direct3D9BitmapReusableRealizationSourceState state) =>
            {
                state = new Direct3D9BitmapReusableRealizationSourceState(
                    0,
                    true,
                    true,
                    1,
                    1,
                    new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
                    0,
                    false,
                    false);
                return source != 0;
            },
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            source => calls.Add($"source:{source}"),
            _ => { });

    private static Direct3D9BitmapReusableRealizationSources CreateOwnedSources(
        Dictionary<nint, nint> next,
        List<string> calls)
    {
        Direct3D9BitmapReusableRealizationSources sources = CreateSources(next, calls);
        sources.SetSources(2);
        calls.Clear();
        return sources;
    }

    private static Direct3D9BitmapReusableRealizationPopulation CreatePopulation(
        Dictionary<nint, nint> next) =>
        new(
            new Direct3D9BitmapReusableRealizationUpdater(
                new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
                (nint _, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
                {
                    rectangles = [];
                    return Direct3D9Factory.SuccessHResult;
                },
                _ => Direct3D9Factory.SuccessHResult,
                (nint _, out nint surface) =>
                {
                    surface = 0;
                    return Direct3D9Factory.SuccessHResult;
                },
                () => (Direct3D9Factory.SuccessHResult, 0),
                (_, _, _, _) => Direct3D9Factory.SuccessHResult,
                _ => { }),
            source => next.GetValueOrDefault(source),
            (nint _, out Direct3D9BitmapRealizationRectangle rectangle) =>
            {
                rectangle = default;
                return true;
            });
}
