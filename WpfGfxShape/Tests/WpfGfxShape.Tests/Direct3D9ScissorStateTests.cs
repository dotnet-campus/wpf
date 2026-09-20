using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ScissorStateTests
{
    [TestMethod]
    public void WhenEnablingScissorThenRectangleIsSetBeforeRenderState()
    {
        List<string> calls = [];
        Direct3D9SurfaceRect nativeRect = default;
        Direct3D9ScissorState state = new(
            rect =>
            {
                nativeRect = rect;
                calls.Add("rect");
                return 0;
            },
            (renderState, value) =>
            {
                Assert.AreEqual(Renderstatetype.Scissortestenable, renderState);
                Assert.AreEqual(1u, value);
                calls.Add("enable");
                return 0;
            });

        int result = state.SetScissorRect(new Direct3D9PointAndSizeRect(2, 3, 5, 7));

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new[] { "rect", "enable" }, calls);
        Assert.AreEqual(new Direct3D9SurfaceRect(2, 3, 7, 10), nativeRect);
        Assert.AreEqual(new Direct3D9PointAndSizeRect(2, 3, 5, 7), state.ScissorRect);
    }

    [TestMethod]
    public void WhenEnabledRectangleIsUnchangedThenNativeRectangleIsNotSetAgain()
    {
        int rectangleCallCount = 0;
        Direct3D9ScissorState state = new(
            _ =>
            {
                rectangleCallCount++;
                return 0;
            },
            (_, _) => 0);
        Direct3D9PointAndSizeRect rectangle = new(2, 3, 5, 7);
        Assert.AreEqual(0, state.SetScissorRect(rectangle));

        int result = state.SetScissorRect(rectangle);

        Assert.AreEqual(0, result);
        Assert.AreEqual(1, rectangleCallCount);
    }

    [TestMethod]
    public void WhenReenablingScissorThenNativeRectangleIsSetAgain()
    {
        int rectangleCallCount = 0;
        Direct3D9ScissorState state = new(
            _ =>
            {
                rectangleCallCount++;
                return 0;
            },
            (_, _) => 0);
        Direct3D9PointAndSizeRect rectangle = new(2, 3, 5, 7);
        Assert.AreEqual(0, state.SetScissorRect(rectangle));
        Assert.AreEqual(0, state.SetScissorRect(null));

        int result = state.SetScissorRect(rectangle);

        Assert.AreEqual(0, result);
        Assert.AreEqual(2, rectangleCallCount);
    }

    [TestMethod]
    public void WhenScissorRectangleSetFailsThenDisableIsAttemptedAndFailureIsPreserved()
    {
        List<uint> renderStateValues = [];
        Direct3D9ScissorState state = new(
            _ => Direct3D9Factory.GenericFailureHResult,
            (_, value) =>
            {
                renderStateValues.Add(value);
                return 0;
            });

        int result = state.SetScissorRect(new Direct3D9PointAndSizeRect(2, 3, 5, 7));

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new uint[] { 0 }, renderStateValues);
        Assert.AreEqual(default, state.ScissorRect);
    }

    [TestMethod]
    public void WhenScissorRectangleAndDisableFailThenFirstFailureIsPreservedAndRectangleIsRetried()
    {
        int rectangleCallCount = 0;
        int renderStateCallCount = 0;
        Direct3D9ScissorState state = new(
            _ => ++rectangleCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0,
            (_, value) =>
            {
                renderStateCallCount++;
                return value == 0 ? unchecked((int) 0x80070005) : 0;
            });
        Direct3D9PointAndSizeRect rectangle = new(2, 3, 5, 7);

        int failedResult = state.SetScissorRect(rectangle);
        int retryResult = state.SetScissorRect(rectangle);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 2, 2, rectangle),
            (failedResult, retryResult, rectangleCallCount, renderStateCallCount, state.ScissorRect));
    }

    [TestMethod]
    public void WhenRenderStateSetFailsThenCacheIsInvalidated()
    {
        int rectangleCallCount = 0;
        int renderStateCallCount = 0;
        Direct3D9ScissorState state = new(
            _ =>
            {
                rectangleCallCount++;
                return 0;
            },
            (_, _) => ++renderStateCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0);
        Direct3D9PointAndSizeRect rectangle = new(2, 3, 5, 7);
        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, state.SetScissorRect(rectangle));

        int result = state.SetScissorRect(rectangle);

        Assert.AreEqual(0, result);
        Assert.AreEqual(2, rectangleCallCount);
    }

    [TestMethod]
    public void WhenScissorRectChangesExternallyThenMatchingRectangleIsNotSetAgain()
    {
        int rectangleCallCount = 0;
        Direct3D9ScissorState state = new(
            _ =>
            {
                rectangleCallCount++;
                return 0;
            },
            (_, _) => 0);
        Direct3D9PointAndSizeRect rectangle = new(2, 3, 5, 7);
        state.Invalidate();
        state.ScissorRectChanged(rectangle);

        int result = state.SetScissorRect(rectangle);

        Assert.AreEqual(0, result);
        Assert.AreEqual(0, rectangleCallCount);
    }
}
