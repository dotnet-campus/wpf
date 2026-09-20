using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapReusableRealizationContextTests
{
    [TestMethod]
    public void WhenContextIsDisposedThenCandidateAndSourceOwnershipAreReleasedInNativeOrder()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = new(
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            source => calls.Add($"candidate:{source}"));
        Direct3D9BitmapReusableRealizationSources sources = CreateSources(next, calls);
        candidates.Add(2);
        sources.Consume(candidates);
        candidates.Add(1);
        calls.Clear();
        Direct3D9BitmapReusableRealizationPopulation population = CreatePopulation(next);
        Direct3D9BitmapReusableRealizationContext context = new(false, candidates, sources, population);

        context.Dispose();
        context.Dispose();

        Assert.AreEqual("candidate:1,source:2", string.Join(',', calls));
    }

    [TestMethod]
    public void WhenContextWasDisposedThenOwnedStateAccessIsRejected()
    {
        Dictionary<nint, nint> next = [];
        using Direct3D9BitmapReusableRealizationCandidates candidates = new(
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            _ => { });
        using Direct3D9BitmapReusableRealizationSources sources = CreateSources(next, []);
        Direct3D9BitmapReusableRealizationContext context = new(
            false,
            candidates,
            sources,
            CreatePopulation(next));
        context.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = context.Sources);
    }

    private static Direct3D9BitmapReusableRealizationSources CreateSources(
        Dictionary<nint, nint> next,
        List<string> calls)
    {
        return new Direct3D9BitmapReusableRealizationSources(
            new Direct3D9BitmapReusableRealizationTargetState(
                0,
                true,
                true,
                40,
                30,
                new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30),
                0),
            (nint source, out Direct3D9BitmapReusableRealizationSourceState state) =>
            {
                state = new Direct3D9BitmapReusableRealizationSourceState(
                    0,
                    true,
                    true,
                    40,
                    30,
                    new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30),
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
    }

    private static Direct3D9BitmapReusableRealizationPopulation CreatePopulation(
        Dictionary<nint, nint> next)
    {
        Direct3D9BitmapReusableRealizationUpdater updater = new(
            new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30),
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
            _ => { });
        return new Direct3D9BitmapReusableRealizationPopulation(
            updater,
            source => next.GetValueOrDefault(source),
            (nint _, out Direct3D9BitmapRealizationRectangle rectangle) =>
            {
                rectangle = new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30);
                return true;
            });
    }
}
