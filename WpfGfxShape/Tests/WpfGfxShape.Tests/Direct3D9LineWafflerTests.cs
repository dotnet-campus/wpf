using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9LineWafflerTests
{
    [TestMethod]
    public void WhenLineStaysInOneCellThenOriginalLineIsForwarded()
    {
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines = [];
        Direct3D9LineWaffler waffler = CreateWaffler(lines);
        Direct3D9WafflePoint start = new(0.1f, 1f, 0.2f);
        Direct3D9WafflePoint end = new(0.9f, 3f, 0.8f);

        int result = waffler.AddLine(start, end);

        Assert.AreEqual((0, 1, start, end), (result, lines.Count, lines[0].Start, lines[0].End));
    }

    [TestMethod]
    public void WhenLineCrossesCellsThenEachOutputLineStaysWithinOneCell()
    {
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines = [];
        Direct3D9LineWaffler waffler = CreateWaffler(lines);

        int result = waffler.AddLine(
            new Direct3D9WafflePoint(0.25f, 2f, 0f),
            new Direct3D9WafflePoint(2.25f, 6f, 1f));

        Assert.AreEqual(
            (0, 3, 0.25f, 1f, 2f, 2.25f),
            (result, lines.Count, lines[0].Start.X, lines[0].End.X, lines[1].End.X, lines[2].End.X));
    }

    [TestMethod]
    public void WhenLineDirectionIsReversedThenOutputKeepsCallerDirection()
    {
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines = [];
        Direct3D9LineWaffler waffler = CreateWaffler(lines);
        Direct3D9WafflePoint start = new(2.25f, 6f, 1f);
        Direct3D9WafflePoint end = new(0.25f, 2f, 0f);

        int result = waffler.AddLine(start, end);

        Assert.AreEqual((0, start, end, 3), (result, lines[0].Start, lines[^1].End, lines.Count));
    }

    [TestMethod]
    public void WhenTranslationIsLargeThenOnlyFractionalPartitionOffsetIsUsed()
    {
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines = [];
        Direct3D9LineWaffler waffler = new();
        waffler.Set(1f, 0f, 1000.25f, (start, end) =>
        {
            lines.Add((start, end));
            return 0;
        });

        int result = waffler.AddLine(
            new Direct3D9WafflePoint(0f, 0f, 0f),
            new Direct3D9WafflePoint(1f, 0f, 1f));

        Assert.AreEqual((0, 2, 0.75f), (result, lines.Count, lines[0].End.X));
    }

    [TestMethod]
    public void WhenConsumerFailsThenFirstFailureStopsFurtherOutput()
    {
        int callCount = 0;
        Direct3D9LineWaffler waffler = new();
        waffler.Set(1f, 0f, 0f, (_, _) =>
        {
            callCount++;
            return Direct3D9Factory.DeviceLostHResult;
        });

        int result = waffler.AddLine(
            new Direct3D9WafflePoint(0.25f, 0f, 0f),
            new Direct3D9WafflePoint(2.25f, 0f, 1f));

        Assert.AreEqual((Direct3D9Factory.DeviceLostHResult, 1), (result, callCount));
    }

    [TestMethod]
    public void WhenScoreIsNotANumberThenFailureIsReturnedWithoutOutput()
    {
        int callCount = 0;
        Direct3D9LineWaffler waffler = new();
        waffler.Set(1f, 0f, 0f, (_, _) =>
        {
            callCount++;
            return 0;
        });

        int result = waffler.AddLine(
            new Direct3D9WafflePoint(float.NaN, 0f, 0f),
            new Direct3D9WafflePoint(1f, 0f, 1f));

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, callCount));
    }

    [TestMethod]
    public void WhenConsumerIsReplacedThenFollowingLinesUseNewConsumer()
    {
        int firstCount = 0;
        int secondCount = 0;
        Direct3D9LineWaffler waffler = new();
        waffler.Set(1f, 0f, 0f, (_, _) =>
        {
            firstCount++;
            return 0;
        });
        waffler.SetConsumer((_, _) =>
        {
            secondCount++;
            return 0;
        });

        int result = waffler.AddLine(
            new Direct3D9WafflePoint(0f, 0f, 0f),
            new Direct3D9WafflePoint(0.5f, 0f, 1f));

        Assert.AreEqual((0, 0, 1), (result, firstCount, secondCount));
    }

    private static Direct3D9LineWaffler CreateWaffler(
        List<(Direct3D9WafflePoint Start, Direct3D9WafflePoint End)> lines)
    {
        Direct3D9LineWaffler waffler = new();
        waffler.Set(1f, 0f, 0f, (start, end) =>
        {
            lines.Add((start, end));
            return 0;
        });
        return waffler;
    }
}
