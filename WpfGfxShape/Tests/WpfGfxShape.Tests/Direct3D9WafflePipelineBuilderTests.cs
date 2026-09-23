using System.Numerics;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9WafflePipelineBuilderTests
{
    [TestMethod]
    public void WhenNoCoordinateRequestsWafflingThenLineUsesFinalSinkDirectly()
    {
        int callCount = 0;
        Direct3D9LineWafflePipeline pipeline = Direct3D9WafflePipelineBuilder.BuildLine(
            [new Direct3D9WaffleTextureCoordinate(Matrix3x2.Identity, Direct3D9WaffleMode.None)],
            (_, _) =>
            {
                callCount++;
                return 0;
            });

        int result = pipeline.Sink(
            new Direct3D9WafflePoint(0f, 0f, 0f),
            new Direct3D9WafflePoint(2f, 0f, 1f));

        Assert.AreEqual((0, false, 0, 1), (result, pipeline.WafflersUsed, pipeline.WafflerCount, callCount));
    }

    [TestMethod]
    public void WhenCoordinateHasBothUsableColumnsThenLineTraversesBothWafflers()
    {
        int callCount = 0;
        Direct3D9LineWafflePipeline pipeline = Direct3D9WafflePipelineBuilder.BuildLine(
            [new Direct3D9WaffleTextureCoordinate(Matrix3x2.Identity, Direct3D9WaffleMode.Enabled)],
            (_, _) =>
            {
                callCount++;
                return 0;
            });

        int result = pipeline.Sink(
            new Direct3D9WafflePoint(0.25f, 0.25f, 0f),
            new Direct3D9WafflePoint(2.25f, 2.25f, 1f));

        Assert.AreEqual((0, true, 2, 5), (result, pipeline.WafflersUsed, pipeline.WafflerCount, callCount));
    }

    [TestMethod]
    public void WhenColumnMagnitudeReachesNativeLimitThenThatColumnIsSkipped()
    {
        int callCount = 0;
        Matrix3x2 pointToTexture = new(4f, 0f, 0f, 1f, 0f, 0f);
        Direct3D9LineWafflePipeline pipeline = Direct3D9WafflePipelineBuilder.BuildLine(
            [new Direct3D9WaffleTextureCoordinate(pointToTexture, Direct3D9WaffleMode.Enabled)],
            (_, _) =>
            {
                callCount++;
                return 0;
            });

        int result = pipeline.Sink(
            new Direct3D9WafflePoint(0f, 0.25f, 0f),
            new Direct3D9WafflePoint(0f, 2.25f, 1f));

        Assert.AreEqual((0, 1, 3), (result, pipeline.WafflerCount, callCount));
    }

    [TestMethod]
    public void WhenOnlyFlipFlagsAreSetThenNativeNonzeroModeStillBuildsWafflers()
    {
        Direct3D9LineWafflePipeline pipeline = Direct3D9WafflePipelineBuilder.BuildLine(
            [new Direct3D9WaffleTextureCoordinate(Matrix3x2.Identity, Direct3D9WaffleMode.FlipX)],
            (_, _) => 0);

        Assert.AreEqual((true, 2), (pipeline.WafflersUsed, pipeline.WafflerCount));
    }

    [TestMethod]
    public void WhenSeveralCoordinatesAreEnabledThenColumnsKeepCoordinateOrder()
    {
        List<Direct3D9WafflePoint> starts = [];
        Direct3D9LineWafflePipeline pipeline = Direct3D9WafflePipelineBuilder.BuildLine(
            [
                new Direct3D9WaffleTextureCoordinate(new Matrix3x2(1f, 0f, 0f, 4f, 0f, 0f), Direct3D9WaffleMode.Enabled),
                new Direct3D9WaffleTextureCoordinate(new Matrix3x2(0f, 4f, 1f, 0f, 0f, 0f), Direct3D9WaffleMode.Enabled),
            ],
            (start, _) =>
            {
                starts.Add(start);
                return 0;
            });

        int result = pipeline.Sink(
            new Direct3D9WafflePoint(0.25f, 0.25f, 0f),
            new Direct3D9WafflePoint(2.25f, 2.25f, 1f));

        Assert.AreEqual((0, 2, 5), (result, pipeline.WafflerCount, starts.Count));
    }

    [TestMethod]
    public void WhenTrianglePipelineHasTwoWafflersThenFinalSinkReceivesFullyPartitionedTriangles()
    {
        List<(Direct3D9WafflePoint First, Direct3D9WafflePoint Second, Direct3D9WafflePoint Third)> triangles = [];
        Direct3D9TriangleWafflePipeline pipeline = Direct3D9WafflePipelineBuilder.BuildTriangle(
            [new Direct3D9WaffleTextureCoordinate(Matrix3x2.Identity, Direct3D9WaffleMode.Enabled)],
            (first, second, third) =>
            {
                triangles.Add((first, second, third));
                return 0;
            });

        int result = pipeline.Sink(
            new Direct3D9WafflePoint(0.2f, 0.2f, 0f),
            new Direct3D9WafflePoint(1.2f, 0.8f, 0.5f),
            new Direct3D9WafflePoint(2.2f, 2.2f, 1f));

        Assert.AreEqual((0, true, 2, true),
            (result, pipeline.WafflersUsed, pipeline.WafflerCount, triangles.Count > 1));
    }

    [TestMethod]
    public void WhenFinalTriangleSinkFailsThenFirstFailureStopsPipelineOutput()
    {
        int callCount = 0;
        Direct3D9TriangleWafflePipeline pipeline = Direct3D9WafflePipelineBuilder.BuildTriangle(
            [new Direct3D9WaffleTextureCoordinate(Matrix3x2.Identity, Direct3D9WaffleMode.Enabled)],
            (_, _, _) =>
            {
                callCount++;
                return Direct3D9Factory.DeviceLostHResult;
            });

        int result = pipeline.Sink(
            new Direct3D9WafflePoint(0.2f, 0.2f, 0f),
            new Direct3D9WafflePoint(1.2f, 0.8f, 0.5f),
            new Direct3D9WafflePoint(2.2f, 2.2f, 1f));

        Assert.AreEqual((Direct3D9Factory.DeviceLostHResult, 1), (result, callCount));
    }
}
