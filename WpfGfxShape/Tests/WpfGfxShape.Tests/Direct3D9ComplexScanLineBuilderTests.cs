using System.Numerics;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ComplexScanLineBuilderTests
{
    [TestMethod]
    public void WhenWafflingIsNotRequestedAndLineIsBelowTopRowThenDirectLineListIsUsed()
    {
        List<string> calls = [];
        Direct3D9ComplexScanLineBuilder builder = CreateBuilder(
            textureCoordinates: [],
            viewportTop: 0f,
            calls);

        int result = builder.AddLine(
            new Direct3D9WafflePoint(0.5f, 1.5f, 0.5f),
            new Direct3D9WafflePoint(2.5f, 1.5f, 0.5f));

        Assert.AreEqual((0, false, Direct3D9ComplexScanLinePath.DirectLineList, "direct"),
            (result, builder.WafflersUsed, builder.SelectPath(1.5f), calls.Single()));
    }

    [TestMethod]
    public void WhenLineIsInTopViewportRowThenVertexBufferSinkIsUsed()
    {
        List<string> calls = [];
        Direct3D9ComplexScanLineBuilder builder = CreateBuilder(
            textureCoordinates: [],
            viewportTop: 2f,
            calls);

        int result = builder.AddLine(
            new Direct3D9WafflePoint(0.5f, 2.5f, 0.5f),
            new Direct3D9WafflePoint(2.5f, 2.5f, 0.5f));

        Assert.AreEqual((0, Direct3D9ComplexScanLinePath.VertexBufferSink, "vertex"),
            (result, builder.SelectPath(2.5f), calls.Single()));
    }

    [TestMethod]
    public void WhenLineIsExactlyOnePixelBelowViewportTopThenDirectLineListIsUsed()
    {
        List<string> calls = [];
        Direct3D9ComplexScanLineBuilder builder = CreateBuilder(
            textureCoordinates: [],
            viewportTop: 2f,
            calls);

        int result = builder.AddLine(
            new Direct3D9WafflePoint(0.5f, 3f, 0.5f),
            new Direct3D9WafflePoint(2.5f, 3f, 0.5f));

        Assert.AreEqual((0, Direct3D9ComplexScanLinePath.DirectLineList, "direct"),
            (result, builder.SelectPath(3f), calls.Single()));
    }

    [TestMethod]
    public void WhenWaffleModeIsSetButAllColumnsAreTooDenseThenTopRowFallbackStillApplies()
    {
        List<string> calls = [];
        Direct3D9ComplexScanLineBuilder builder = CreateBuilder(
            [new Direct3D9WaffleTextureCoordinate(new Matrix3x2(4f, 0f, 0f, 4f, 0f, 0f), Direct3D9WaffleMode.Enabled)],
            viewportTop: 0f,
            calls);

        int result = builder.AddLine(
            new Direct3D9WafflePoint(0.5f, 0.5f, 0.5f),
            new Direct3D9WafflePoint(2.5f, 0.5f, 0.5f));

        Assert.AreEqual((0, false, Direct3D9ComplexScanLinePath.VertexBufferSink, "vertex"),
            (result, builder.WafflersUsed, builder.SelectPath(0.5f), calls.Single()));
    }

    [TestMethod]
    public void WhenWafflersAreUsableThenTheyTakePrecedenceOverTopRowFallback()
    {
        List<string> calls = [];
        Direct3D9ComplexScanLineBuilder builder = CreateBuilder(
            [new Direct3D9WaffleTextureCoordinate(new Matrix3x2(1f, 0f, 0f, 4f, 0f, 0f), Direct3D9WaffleMode.Enabled)],
            viewportTop: 0f,
            calls);

        int result = builder.AddLine(
            new Direct3D9WafflePoint(0.25f, 0.5f, 0.5f),
            new Direct3D9WafflePoint(2.25f, 0.5f, 0.5f));

        Assert.AreEqual((0, true, Direct3D9ComplexScanLinePath.WafflePipeline, 3),
            (result, builder.WafflersUsed, builder.SelectPath(0.5f), calls.Count));
    }

    [TestMethod]
    public void WhenSelectedSinkFailsThenItsHResultIsReturned()
    {
        Direct3D9ComplexScanLineBuilder builder = new(
            viewportTop: 0f,
            textureCoordinates: [],
            directLineListSink: (_, _) => Direct3D9Factory.DeviceLostHResult,
            vertexBufferSink: (_, _) => 0);

        int result = builder.AddLine(
            new Direct3D9WafflePoint(0.5f, 1.5f, 0.5f),
            new Direct3D9WafflePoint(2.5f, 1.5f, 0.5f));

        Assert.AreEqual(Direct3D9Factory.DeviceLostHResult, result);
    }

    private static Direct3D9ComplexScanLineBuilder CreateBuilder(
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates,
        float viewportTop,
        List<string> calls)
    {
        return new Direct3D9ComplexScanLineBuilder(
            viewportTop,
            textureCoordinates,
            (start, end) =>
            {
                calls.Add("direct");
                return 0;
            },
            (start, end) =>
            {
                calls.Add("vertex");
                return 0;
            });
    }
}
