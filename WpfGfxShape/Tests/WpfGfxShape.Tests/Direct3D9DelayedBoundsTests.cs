using System.Numerics;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DelayedBoundsTests
{
    [TestMethod]
    public void WhenBoundsAreNotInitializedThenBoundsCannotBeRetrieved()
    {
        Direct3D9DelayedBounds delayedBounds = new();

        bool result = delayedBounds.TryGetBounds(out MilRectF bounds);

        Assert.AreEqual((false, default(MilRectF)), (result, bounds));
    }

    [TestMethod]
    public void WhenTransformIsNotInvertibleThenBoundsCannotBeRetrieved()
    {
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(10, 20, 30, 60),
            new Matrix3x2(1, 0, 2, 0, 0, 0));

        bool result = delayedBounds.TryGetBounds(out MilRectF bounds);

        Assert.AreEqual((false, default(MilRectF)), (result, bounds));
    }

    [TestMethod]
    public void WhenBoundsAreRequestedThenInverseTransformBoundsAreComputed()
    {
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(10, 20, 30, 60),
            new Matrix3x2(2, 0, 0, 4, 10, 20));

        bool result = delayedBounds.TryGetBounds(out MilRectF bounds);

        Assert.AreEqual((true, new MilRectF(0, 0, 10, 10)), (result, bounds));
    }

    [TestMethod]
    public void WhenInverseTransformSkewsBoundsThenAllFourCornersContribute()
    {
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(0, 0, 10, 10),
            new Matrix3x2(1, 1, 0, 1, 0, 0));

        delayedBounds.TryGetBounds(out MilRectF bounds);

        Assert.AreEqual(new MilRectF(0, -10, 10, 10), bounds);
    }

    [TestMethod]
    public void WhenBoundsAreResetThenCachedResultIsInvalidated()
    {
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(new MilRectF(0, 0, 10, 10), Matrix3x2.Identity);
        delayedBounds.TryGetBounds(out _);
        delayedBounds.SetBoundsRectAndInverseTransform(new MilRectF(20, 30, 40, 50), Matrix3x2.Identity);

        delayedBounds.TryGetBounds(out MilRectF bounds);

        Assert.AreEqual(new MilRectF(20, 30, 40, 50), bounds);
    }

    [TestMethod]
    public void WhenMinimumBoundsUseDelayedBoundsThenSamplingBoundsAreComputedOnDemand()
    {
        Direct3D9DelayedBounds delayedBounds = new();
        delayedBounds.SetBoundsRectAndInverseTransform(
            new MilRectF(20, 10, 60, 50),
            new Matrix3x2(2, 0, 0, 2, 0, 0));
        Direct3D9BitmapMinimumRealizationBounds calculator = new(delayedBounds);
        Direct3D9BitmapRealizationRectangle minimumBounds = new(0, 0, 40, 30);
        Direct3D9BitmapRealizationProperties properties = new(
            MilBitmapInterpolationMode.Linear,
            Direct3D9TextureMipMapLevel.One,
            MilBitmapWrapMode.Extend,
            MilPixelFormat.Pbgra32Bpp,
            IsMinimumRealizationRectComputed: false,
            BitmapWidth: 40,
            BitmapHeight: 30,
            Width: 40,
            Height: 30);

        bool result = calculator.Compute(0, properties, ref minimumBounds);

        Assert.AreEqual((true, new Direct3D9BitmapRealizationRectangle(9, 4, 31, 26)), (result, minimumBounds));
    }
}
