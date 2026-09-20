using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapReusableRealizationPopulationFactoryTests
{
    [TestMethod]
    public void WhenProductionContextIsCreatedThenSelectedColorSourceBuildsReusablePopulation()
    {
        List<string> calls = [];
        Dictionary<nint, nint> next = [];
        Direct3D9BitmapReusableRealizationCandidates candidates = new(
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            _ => { });
        candidates.Add(23);
        Direct3D9BitmapReusableRealizationPopulationFactory populationFactory = new(
            colorSource =>
            {
                calls.Add($"destination-bounds:{colorSource}");
                return new Direct3D9BitmapRealizationRectangle(10, 20, 14, 24);
            },
            (nint source, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
            {
                calls.Add($"valid:{source}");
                rectangles = [new Direct3D9BitmapRealizationRectangle(10, 20, 14, 24)];
                return Direct3D9Factory.SuccessHResult;
            },
            source =>
            {
                calls.Add($"realize:{source}");
                return Direct3D9Factory.SuccessHResult;
            },
            (nint source, out nint surface) =>
            {
                calls.Add($"source-surface:{source}");
                surface = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            colorSource =>
            {
                calls.Add($"destination-surface:{colorSource}");
                return (Direct3D9Factory.SuccessHResult, (nint) 41);
            },
            (sourceSurface, _, destinationSurface, _) =>
            {
                calls.Add($"stretch:{sourceSurface}:{destinationSurface}");
                return Direct3D9Factory.SuccessHResult;
            },
            surface => calls.Add($"release-surface:{surface}"),
            source => next.GetValueOrDefault(source),
            (nint source, out Direct3D9BitmapRealizationRectangle bounds) =>
            {
                calls.Add($"source-bounds:{source}");
                bounds = new Direct3D9BitmapRealizationRectangle(10, 20, 14, 24);
                return true;
            });
        Direct3D9BitmapReusableRealizationContextFactory contextFactory = new(
            false,
            _ => throw new AssertFailedException(),
            colorSource =>
            {
                calls.Add($"target:{colorSource}");
                return new Direct3D9BitmapReusableRealizationTargetState(
                    7,
                    true,
                    true,
                    4,
                    4,
                    new Direct3D9BitmapRealizationRectangle(10, 20, 14, 24),
                    2);
            },
            (nint source, out Direct3D9BitmapReusableRealizationSourceState state) =>
            {
                state = new Direct3D9BitmapReusableRealizationSourceState(
                    7,
                    true,
                    true,
                    4,
                    4,
                    new Direct3D9BitmapRealizationRectangle(10, 20, 14, 24),
                    1,
                    true,
                    false);
                return source == 23;
            },
            source => next.GetValueOrDefault(source),
            (source, value) => next[source] = value,
            _ => { },
            _ => { },
            _ => { },
            populationFactory);

        using Direct3D9BitmapReusableRealizationContext context = contextFactory.Create(17, candidates);
        context.Sources.Consume(context.Candidates);
        calls.Clear();
        int result = context.Population.UpdateFromSources(
            context.Sources.Head,
            [new Direct3D9BitmapRealizationRectangle(10, 20, 14, 24)],
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remaining);

        Assert.AreEqual(
            "0|0|source-bounds:23,valid:23,realize:23,source-surface:23,destination-surface:17,stretch:31:41,release-surface:31,release-surface:41",
            $"{result}|{remaining.Count}|{string.Join(',', calls)}");
    }
}
