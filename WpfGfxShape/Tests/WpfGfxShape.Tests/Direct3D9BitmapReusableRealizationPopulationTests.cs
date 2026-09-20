using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapReusableRealizationPopulationTests
{
    [TestMethod]
    public void WhenSourcesCoverDirtyAreaThenOnlyFinalRemainderIsReturned()
    {
        PopulationHarness harness = new();
        harness.ValidRectangles[1] = [new(0, 0, 20, 20)];
        harness.ValidRectangles[2] = [new(20, 0, 40, 20)];
        Direct3D9BitmapReusableRealizationPopulation population = harness.CreatePopulation();

        int result = population.UpdateFromSources(
            1,
            [new Direct3D9BitmapRealizationRectangle(0, 0, 60, 20)],
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remaining);

        Assert.AreEqual(
            (0, "prefilter:1,valid:1,ensure:1,source-surface:1,destination-surface,stretch:11:0:0:20:20:22:0:0:20:20,release:11,release:22,next:1,prefilter:2,valid:2,ensure:2,source-surface:2,destination-surface,stretch:12:0:0:20:20:22:20:0:40:20,release:12,release:22,next:2", "40:0:60:20"),
            (result, harness.Calls, Format(remaining)));
    }

    [TestMethod]
    public void WhenSourcesCoverAllDirtyAreaThenBitmapPushCanBeSkipped()
    {
        PopulationHarness harness = new();
        harness.ValidRectangles[1] = [new(0, 0, 20, 20)];
        Direct3D9BitmapReusableRealizationPopulation population = harness.CreatePopulation();

        int result = population.UpdateFromSources(
            1,
            [new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20)],
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remaining);

        Assert.AreEqual((0, 0), (result, remaining.Count));
    }

    [TestMethod]
    public void WhenReusableUpdateFailsThenLaterSourcesAreNotConsumed()
    {
        PopulationHarness harness = new()
        {
            StretchResult = Direct3D9Factory.DriverInternalErrorHResult
        };
        harness.ValidRectangles[1] = [new(0, 0, 20, 20)];
        Direct3D9BitmapReusableRealizationPopulation population = harness.CreatePopulation();

        int result = population.UpdateFromSources(
            1,
            [new Direct3D9BitmapRealizationRectangle(0, 0, 40, 20)],
            out _);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, false),
            (result, harness.Calls.Contains("next:1", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void WhenSourceMetadataIsUnavailableThenFirstErrorIsReturnedBeforeUpdate()
    {
        PopulationHarness harness = new();
        harness.MissingPrefilteredBitmapSource = 1;
        Direct3D9BitmapReusableRealizationPopulation population = harness.CreatePopulation();

        int result = population.UpdateFromSources(
            1,
            [new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20)],
            out _);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "prefilter:1"),
            (result, harness.Calls));
    }

    private static string Format(IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
        string.Join('|', rectangles.Select(static rectangle =>
            $"{rectangle.Left}:{rectangle.Top}:{rectangle.Right}:{rectangle.Bottom}"));

    private sealed class PopulationHarness
    {
        private readonly List<string> _calls = [];

        internal Dictionary<nint, IReadOnlyList<Direct3D9BitmapRealizationRectangle>> ValidRectangles { get; } = [];
        internal nint MissingPrefilteredBitmapSource { get; set; }
        internal int StretchResult { get; set; } = Direct3D9Factory.SuccessHResult;
        internal string Calls => string.Join(',', _calls);

        internal Direct3D9BitmapReusableRealizationPopulation CreatePopulation()
        {
            Direct3D9BitmapReusableRealizationUpdater updater = new(
                new Direct3D9BitmapRealizationRectangle(0, 0, 60, 20),
                (nint source, out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
                {
                    _calls.Add($"valid:{source}");
                    rectangles = ValidRectangles.GetValueOrDefault(source, []);
                    return Direct3D9Factory.SuccessHResult;
                },
                source =>
                {
                    _calls.Add($"ensure:{source}");
                    return Direct3D9Factory.SuccessHResult;
                },
                (nint source, out nint surface) =>
                {
                    _calls.Add($"source-surface:{source}");
                    surface = source + 10;
                    return Direct3D9Factory.SuccessHResult;
                },
                () =>
                {
                    _calls.Add("destination-surface");
                    return (Direct3D9Factory.SuccessHResult, (nint) 22);
                },
                (sourceSurface, sourceRectangle, destinationSurface, destinationRectangle) =>
                {
                    _calls.Add($"stretch:{sourceSurface}:{sourceRectangle.Left}:{sourceRectangle.Top}:{sourceRectangle.Right}:{sourceRectangle.Bottom}:{destinationSurface}:{destinationRectangle.Left}:{destinationRectangle.Top}:{destinationRectangle.Right}:{destinationRectangle.Bottom}");
                    return StretchResult;
                },
                surface => _calls.Add($"release:{surface}"));

            return new(
                updater,
                source =>
                {
                    _calls.Add($"next:{source}");
                    return source == 1 ? 2 : 0;
                },
                (nint source, out Direct3D9BitmapRealizationRectangle rectangle) =>
                {
                    _calls.Add($"prefilter:{source}");
                    rectangle = source == 1
                        ? new Direct3D9BitmapRealizationRectangle(0, 0, 60, 20)
                        : new Direct3D9BitmapRealizationRectangle(20, 0, 80, 20);
                    return source != MissingPrefilteredBitmapSource;
                });
        }
    }
}
