using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9BitmapMinimumRealizationBoundsTests
{
    [TestMethod]
    public void WhenBoundsCannotBeRetrievedThenMinimumBoundsAreUnchanged()
    {
        Direct3D9BitmapMinimumRealizationBounds calculator = new(
            static (nint _, out MilRectF bounds) =>
            {
                bounds = default;
                return false;
            });
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);

        bool result = calculator.Compute(17, CreateProperties(), ref minimumBounds);

        Assert.AreEqual((false, new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30)), (result, minimumBounds));
    }

    [TestMethod]
    public void WhenBitmapWasPrefilteredThenSamplingBoundsAreScaledBeforeRounding()
    {
        Direct3D9BitmapMinimumRealizationBounds calculator = CreateCalculator(new MilRectF(20.25f, 10, 60.25f, 50));
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);

        bool result = calculator.Compute(17, CreateProperties(), ref minimumBounds);

        Assert.AreEqual((true, new Direct3D9BitmapRealizationRectangle(9, 4, 31, 26)), (result, minimumBounds));
    }

    [TestMethod]
    public void WhenNearestNeighborIsUsedThenOneTexelRoundingFactorIsApplied()
    {
        Direct3D9BitmapMinimumRealizationBounds calculator = CreateCalculator(new MilRectF(10.25f, 5.25f, 30.25f, 25.25f));
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);
        Direct3D9BitmapRealizationProperties properties = CreateProperties(
            bitmapWidth: 40,
            bitmapHeight: 30,
            interpolationMode: MilBitmapInterpolationMode.NearestNeighbor);

        calculator.Compute(17, properties, ref minimumBounds);

        Assert.AreEqual(new Direct3D9BitmapRealizationRectangle(10, 5, 31, 26), minimumBounds);
    }

    [TestMethod]
    public void WhenExtendSamplingIsBeyondLastTexelThenLastEdgeIsRetained()
    {
        Direct3D9BitmapMinimumRealizationBounds calculator = CreateCalculator(new MilRectF(50, 40, 60, 50));
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);

        calculator.Compute(17, CreateProperties(bitmapWidth: 40, bitmapHeight: 30), ref minimumBounds);

        Assert.AreEqual(new Direct3D9BitmapRealizationRectangle(39, 29, 40, 30), minimumBounds);
    }

    [TestMethod]
    public void WhenExtendSamplingIsBeforeFirstTexelThenFirstEdgeIsRetained()
    {
        Direct3D9BitmapMinimumRealizationBounds calculator = CreateCalculator(new MilRectF(-20, -20, -10, -10));
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);

        calculator.Compute(17, CreateProperties(bitmapWidth: 40, bitmapHeight: 30), ref minimumBounds);

        Assert.AreEqual(new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1), minimumBounds);
    }

    [TestMethod]
    public void WhenNonExtendSamplingStaysInsideBaseSpanThenMinimumSpanIsUsed()
    {
        Direct3D9BitmapMinimumRealizationBounds calculator = CreateCalculator(new MilRectF(10, 5, 20, 15));
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);
        Direct3D9BitmapRealizationProperties properties = CreateProperties(
            bitmapWidth: 40,
            bitmapHeight: 30,
            wrapMode: MilBitmapWrapMode.Tile);

        calculator.Compute(17, properties, ref minimumBounds);

        Assert.AreEqual(new Direct3D9BitmapRealizationRectangle(9, 4, 21, 16), minimumBounds);
    }

    [TestMethod]
    public void WhenNonExtendSamplingLeavesBaseSpanThenWholeSpanIsRequired()
    {
        Direct3D9BitmapMinimumRealizationBounds calculator = CreateCalculator(new MilRectF(-2, 5, 20, 15));
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);
        Direct3D9BitmapRealizationProperties properties = CreateProperties(
            bitmapWidth: 40,
            bitmapHeight: 30,
            wrapMode: MilBitmapWrapMode.Tile);

        calculator.Compute(17, properties, ref minimumBounds);

        Assert.AreEqual(new Direct3D9BitmapRealizationRectangle(0, 4, 40, 16), minimumBounds);
    }

    [TestMethod]
    public void WhenExtendStartContainsNaNThenSaturatedLowerBoundRetainsAvailableEnd()
    {
        Direct3D9BitmapMinimumRealizationBounds calculator = CreateCalculator(new MilRectF(float.NaN, 5, 20, 15));
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);

        calculator.Compute(17, CreateProperties(bitmapWidth: 40, bitmapHeight: 30), ref minimumBounds);

        Assert.AreEqual(new Direct3D9BitmapRealizationRectangle(0, 4, 21, 16), minimumBounds);
    }

    [TestMethod]
    public void WhenRoundedSamplingSpanIsEmptyThenWholeSpanIsRequired()
    {
        Direct3D9BitmapMinimumRealizationBounds calculator = CreateCalculator(new MilRectF(20, 5, 10, 15));
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);

        calculator.Compute(17, CreateProperties(bitmapWidth: 40, bitmapHeight: 30), ref minimumBounds);

        Assert.AreEqual(new Direct3D9BitmapRealizationRectangle(0, 4, 40, 16), minimumBounds);
    }

    private static Direct3D9BitmapMinimumRealizationBounds CreateCalculator(MilRectF bounds) =>
        new((nint _, out MilRectF result) =>
        {
            result = bounds;
            return true;
        });

    private static Direct3D9BitmapRealizationProperties CreateProperties(
        uint bitmapWidth = 80,
        uint bitmapHeight = 60,
        MilBitmapInterpolationMode interpolationMode = MilBitmapInterpolationMode.Linear,
        MilBitmapWrapMode wrapMode = MilBitmapWrapMode.Extend) =>
        new(
            interpolationMode,
            Direct3D9TextureMipMapLevel.One,
            wrapMode,
            MilPixelFormat.Pbgra32Bpp,
            IsMinimumRealizationRectComputed: false,
            bitmapWidth,
            bitmapHeight,
            Width: 40,
            Height: 30);
}
