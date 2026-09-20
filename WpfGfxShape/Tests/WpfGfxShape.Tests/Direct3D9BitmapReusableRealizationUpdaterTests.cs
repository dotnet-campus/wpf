using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapReusableRealizationUpdaterTests
{
    [TestMethod]
    public void WhenValidRectanglePartiallyCoversDirtyRectangleThenIntersectionIsCopiedAndRemainderIsReturned()
    {
        UpdaterHarness harness = new();
        harness.ValidRectangles = [new(20, 20, 60, 60)];
        Direct3D9BitmapReusableRealizationUpdater updater = harness.CreateUpdater();

        int result = updater.UpdateFromReusableSource(
            1,
            new Direct3D9BitmapRealizationRectangle(10, 10, 90, 90),
            [new Direct3D9BitmapRealizationRectangle(0, 0, 50, 50)],
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remaining);

        Assert.AreEqual(
            (0, "ensure:1,get-source:1,get-destination,stretch:10:10:40:40:15:15:45:45,release:11,release:22", "0:0:50:20|0:20:20:50"),
            (result, harness.Calls, Format(remaining)));
    }

    [TestMethod]
    public void WhenSeveralValidRectanglesCoverDirtyAreaThenSurfacesArePreparedOnce()
    {
        UpdaterHarness harness = new();
        harness.ValidRectangles = [new(0, 0, 20, 20), new(20, 0, 40, 20)];
        Direct3D9BitmapReusableRealizationUpdater updater = harness.CreateUpdater();

        int result = updater.UpdateFromReusableSource(
            1,
            new Direct3D9BitmapRealizationRectangle(0, 0, 40, 20),
            [new Direct3D9BitmapRealizationRectangle(0, 0, 40, 20)],
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remaining);

        Assert.AreEqual(
            (0, "ensure:1,get-source:1,get-destination,stretch:0:0:20:20:-5:-5:15:15,stretch:20:0:40:20:15:-5:35:15,release:11,release:22", 0),
            (result, harness.Calls, remaining.Count));
    }

    [TestMethod]
    public void WhenNoValidRectangleIntersectsThenRealizationAndSurfacesAreNotRequested()
    {
        UpdaterHarness harness = new();
        harness.ValidRectangles = [new(60, 60, 80, 80)];
        Direct3D9BitmapReusableRealizationUpdater updater = harness.CreateUpdater();

        int result = updater.UpdateFromReusableSource(
            1,
            new Direct3D9BitmapRealizationRectangle(0, 0, 100, 100),
            [new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20)],
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> remaining);

        Assert.AreEqual((0, string.Empty, "0:0:20:20"), (result, harness.Calls, Format(remaining)));
    }

    [TestMethod]
    public void WhenDestinationSurfaceAcquisitionFailsThenSourceSurfaceIsReleasedAndFirstErrorIsReturned()
    {
        UpdaterHarness harness = new()
        {
            ValidRectangles = [new(0, 0, 20, 20)],
            DestinationResult = Direct3D9Factory.GenericFailureHResult
        };
        Direct3D9BitmapReusableRealizationUpdater updater = harness.CreateUpdater();

        int result = updater.UpdateFromReusableSource(
            1,
            new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20),
            [new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20)],
            out _);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "ensure:1,get-source:1,get-destination,release:11"),
            (result, harness.Calls));
    }

    [TestMethod]
    public void WhenStretchRectFailsThenBothSurfacesAreReleasedAndFirstErrorIsReturned()
    {
        UpdaterHarness harness = new()
        {
            ValidRectangles = [new(0, 0, 20, 20)],
            StretchResult = Direct3D9Factory.DriverInternalErrorHResult
        };
        Direct3D9BitmapReusableRealizationUpdater updater = harness.CreateUpdater();

        int result = updater.UpdateFromReusableSource(
            1,
            new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20),
            [new Direct3D9BitmapRealizationRectangle(0, 0, 20, 20)],
            out _);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, "ensure:1,get-source:1,get-destination,stretch:0:0:20:20:-5:-5:15:15,release:11,release:22"),
            (result, harness.Calls));
    }

    private static string Format(IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles) =>
        string.Join('|', rectangles.Select(rectangle => $"{rectangle.Left}:{rectangle.Top}:{rectangle.Right}:{rectangle.Bottom}"));

    private sealed class UpdaterHarness
    {
        private readonly List<string> _calls = [];

        internal IReadOnlyList<Direct3D9BitmapRealizationRectangle> ValidRectangles { get; set; } = [];

        internal int DestinationResult { get; init; }

        internal int StretchResult { get; init; }

        internal string Calls => string.Join(',', _calls);

        internal Direct3D9BitmapReusableRealizationUpdater CreateUpdater() =>
            new(
                new Direct3D9BitmapRealizationRectangle(5, 5, 105, 105),
                TryGetValidRectangles,
                source =>
                {
                    _calls.Add($"ensure:{source}");
                    return Direct3D9Factory.SuccessHResult;
                },
                (nint source, out nint surface) =>
                {
                    _calls.Add($"get-source:{source}");
                    surface = 11;
                    return Direct3D9Factory.SuccessHResult;
                },
                () =>
                {
                    _calls.Add("get-destination");
                    return (DestinationResult, DestinationResult < 0 ? 0 : 22);
                },
                Stretch,
                surface => _calls.Add($"release:{surface}"));

        private int TryGetValidRectangles(
            nint source,
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> rectangles)
        {
            rectangles = ValidRectangles;
            return Direct3D9Factory.SuccessHResult;
        }

        private int Stretch(
            nint sourceSurface,
            Direct3D9SurfaceRect sourceRectangle,
            nint destinationSurface,
            Direct3D9SurfaceRect destinationRectangle)
        {
            _calls.Add(
                $"stretch:{sourceRectangle.Left}:{sourceRectangle.Top}:{sourceRectangle.Right}:{sourceRectangle.Bottom}:" +
                $"{destinationRectangle.Left}:{destinationRectangle.Top}:{destinationRectangle.Right}:{destinationRectangle.Bottom}");
            return StretchResult;
        }
    }
}
