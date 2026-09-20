using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class Direct3D9ClipStateTests
{
    private const Transformstatetype World = (Transformstatetype) 256;

    [TestMethod]
    public void WhenClipIntersectsTargetThenScissorUsesIntersectionAndCachesIt()
    {
        Direct3D9PointAndSizeRect? appliedScissor = null;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            setScissorRect: value =>
            {
                appliedScissor = value;
                return 0;
            });

        int result = device.SetClipRect(new Direct3D9SurfaceRect(-5, 4, 12, 30));

        Direct3D9PointAndSizeRect expected = new(0, 4, 12, 16);
        Assert.AreEqual((0, expected, true, expected), (result, appliedScissor, device.IsClipSet, device.GetClipRect()));
    }

    [TestMethod]
    public void WhenClipDoesNotIntersectTargetThenReturnsClippedToEmptyWithoutCalls()
    {
        int callCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            setScissorRect: _ =>
            {
                callCount++;
                return 0;
            });

        int result = device.SetClipRect(new Direct3D9SurfaceRect(20, 20, 30, 30));

        Assert.AreEqual((Direct3D9Factory.ClippedToEmptyHResult, 0, false), (result, callCount, device.IsClipSet));
    }

    [TestMethod]
    public void WhenFullTargetClipIsAppliedThenScissorIsDisabled()
    {
        Direct3D9PointAndSizeRect? appliedScissor = new(1, 1, 1, 1);
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            setScissorRect: value =>
            {
                appliedScissor = value;
                return 0;
            });

        int result = device.SetClipRect(new Direct3D9SurfaceRect(-10, -10, 30, 30));

        Assert.AreEqual((0, null, true), (result, appliedScissor, device.IsClipSet));
    }

    [TestMethod]
    public void WhenClipIsClearedThenFullTargetIsAppliedBeforeCacheIsCleared()
    {
        bool? clipSetDuringCall = null;
        bool observeClipState = false;
        Direct3D9Device? device = null;
        device = CreateDevice(
            supportsScissor: true,
            setScissorRect: _ =>
            {
                if (observeClipState)
                {
                    clipSetDuringCall = device.IsClipSet;
                }

                return 0;
            });
        using (device)
        {
            _ = device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 10, 12));
            observeClipState = true;

            int result = device.SetClipRect(null);

            Assert.AreEqual((0, true, false, new Direct3D9PointAndSizeRect(0, 0, 16, 20)),
                (result, clipSetDuringCall, device.IsClipSet, device.GetClipRect()));
        }
    }

    [TestMethod]
    public void WhenScissorCallFailsThenClipCacheIsUnchanged()
    {
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            setScissorRect: _ => Direct3D9Factory.GenericFailureHResult);

        int result = device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 10, 12));

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, false, new Direct3D9PointAndSizeRect(0, 0, 16, 20)),
            (result, device.IsClipSet, device.GetClipRect()));
    }

    [TestMethod]
    public void WhenScissorIsUnsupportedThenViewportPrecedesClippingMatrix()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(
            supportsScissor: false,
            setViewport: _ =>
            {
                calls.Add("viewport");
                return 0;
            },
            setSurfaceToClippingMatrix: _ =>
            {
                calls.Add("matrix");
                return 0;
            });
        calls.Clear();

        int result = device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 10, 12));

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new[] { "viewport", "matrix" }, calls);
    }

    [TestMethod]
    public void WhenViewportFailsThenClippingMatrixAndClipCacheAreUnchanged()
    {
        int viewportCallCount = 0;
        int matrixCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: false,
            setViewport: _ => viewportCallCount++ == 0 ? 0 : Direct3D9Factory.GenericFailureHResult,
            setSurfaceToClippingMatrix: _ =>
            {
                matrixCallCount++;
                return 0;
            });
        matrixCallCount = 0;

        int result = device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 10, 12));

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, false), (result, matrixCallCount, device.IsClipSet));
    }

    [TestMethod]
    public void WhenGettingUnsetClipThenReturnsTargetBoundsWithoutNativeCalls()
    {
        int scissorCallCount = 0;
        int viewportCallCount = 0;
        int matrixCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            setScissorRect: _ =>
            {
                scissorCallCount++;
                return 0;
            },
            setViewport: _ =>
            {
                viewportCallCount++;
                return 0;
            },
            setSurfaceToClippingMatrix: _ =>
            {
                matrixCallCount++;
                return 0;
            });
        scissorCallCount = 0;
        viewportCallCount = 0;
        matrixCallCount = 0;

        Direct3D9PointAndSizeRect first = device.GetClipRect();
        Direct3D9PointAndSizeRect second = device.GetClipRect();

        Direct3D9PointAndSizeRect expected = new(0, 0, 16, 20);
        Assert.AreEqual((expected, expected, 0, 0, 0),
            (first, second, scissorCallCount, viewportCallCount, matrixCallCount));
    }

    [TestMethod]
    public void WhenGettingSetClipThenReturnsCachedIntersectionWithoutNativeCalls()
    {
        int scissorCallCount = 0;
        int viewportCallCount = 0;
        int matrixCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            setScissorRect: _ =>
            {
                scissorCallCount++;
                return 0;
            },
            setViewport: _ =>
            {
                viewportCallCount++;
                return 0;
            },
            setSurfaceToClippingMatrix: _ =>
            {
                matrixCallCount++;
                return 0;
            });
        Assert.AreEqual(0, device.SetClipRect(new Direct3D9SurfaceRect(-5, 4, 12, 30)));
        scissorCallCount = 0;
        viewportCallCount = 0;
        matrixCallCount = 0;

        Direct3D9PointAndSizeRect first = device.GetClipRect();
        Direct3D9PointAndSizeRect second = device.GetClipRect();

        Direct3D9PointAndSizeRect expected = new(0, 4, 12, 16);
        Assert.AreEqual((expected, expected, 0, 0, 0),
            (first, second, scissorCallCount, viewportCallCount, matrixCallCount));
    }

    [TestMethod]
    public void WhenRenderTargetChangesThenGettingUnsetClipReturnsLatestTargetBounds()
    {
        SurfaceDesc description = new(width: 16, height: 20);
        int setRenderTargetCallCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getRenderTargetDescription: _ => description,
            setRenderTarget: _ =>
            {
                setRenderTargetCallCount++;
                return 0;
            },
            setViewport: _ => 0,
            setScissorRect: _ => 0,
            setSurfaceToClippingMatrix: _ => 0);
        using Direct3D9Surface firstSurface = new(new Direct3D9ResourceManager(), null);
        using Direct3D9Surface secondSurface = new(new Direct3D9ResourceManager(), null);
        Assert.AreEqual(0, device.SetRenderTarget(firstSurface));
        description = new SurfaceDesc(width: 31, height: 47);
        Assert.AreEqual(0, device.SetRenderTarget(secondSurface));

        Direct3D9PointAndSizeRect first = device.GetClipRect();
        Direct3D9PointAndSizeRect second = device.GetClipRect();

        Direct3D9PointAndSizeRect expected = new(0, 0, 31, 47);
        Assert.AreEqual((expected, expected, 2, false),
            (first, second, setRenderTargetCallCount, device.IsClipSet));
    }

    [TestMethod]
    public void WhenGettingSetClipThenDeviceAndResourceStateAreUnchanged()
    {
        using Direct3D9Device device = CreateDevice(supportsScissor: true, setScissorRect: _ => 0);
        Assert.AreEqual(0, device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 10, 12)));
        Direct3D9PointAndSizeRect clipCache = device.ClipCache;
        Direct3D9PointAndSizeRect viewportCache = device.ViewportCache;
        Direct3D9PointAndSizeRect scissorCache = device.ScissorRectCache;
        int resourceCount = device.ResourceCount;
        bool inScene = device.IsInScene;

        Direct3D9PointAndSizeRect result = device.GetClipRect();

        Assert.AreEqual(
            (clipCache, viewportCache, scissorCache, resourceCount, inScene, false, false, true),
            (result, device.ViewportCache, device.ScissorRectCache, device.ResourceCount, device.IsInScene,
                device.IsEntered(), device.IsInUseContext(), device.IsClipSet));
    }

    [TestMethod]
    public void WhenGettingClipInsideExistingEntryAndUseContextThenOuterStateIsPreserved()
    {
        using Direct3D9Device device = CreateDevice(supportsScissor: true, setScissorRect: _ => 0);
        device.Enter();
        uint useContextDepth = device.EnterUseContext();
        try
        {
            Direct3D9PointAndSizeRect result = device.GetClipRect();

            Assert.AreEqual((new Direct3D9PointAndSizeRect(0, 0, 16, 20), true, true),
                (result, device.IsEntered(), device.IsInUseContext()));
        }
        finally
        {
            device.ExitUseContext(useContextDepth);
            device.Leave();
        }
    }

    [TestMethod]
    public void WhenScissorReturnsNonZeroSuccessThenResultAndClipCacheArePreserved()
    {
        const int nonZeroSuccess = 1;
        Direct3D9PointAndSizeRect? appliedScissor = null;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            setScissorRect: value =>
            {
                appliedScissor = value;
                return nonZeroSuccess;
            });

        int result = device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 10, 12));

        Direct3D9PointAndSizeRect expected = new(1, 2, 9, 10);
        Assert.AreEqual((nonZeroSuccess, expected, true, expected),
            (result, appliedScissor, device.IsClipSet, device.GetClipRect()));
    }

    [TestMethod]
    public void WhenSettingSameEffectiveClipThenReturnsSuccessWithoutDeviceCalls()
    {
        int scissorCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            setScissorRect: _ =>
            {
                scissorCallCount++;
                return 0;
            });
        Assert.AreEqual(0, device.SetClipRect(new Direct3D9SurfaceRect(-5, 4, 12, 30)));
        scissorCallCount = 0;

        int result = device.SetClipRect(new Direct3D9SurfaceRect(0, 4, 12, 20));

        Assert.AreEqual((0, 0, true, new Direct3D9PointAndSizeRect(0, 4, 12, 16)),
            (result, scissorCallCount, device.IsClipSet, device.GetClipRect()));
    }

    [TestMethod]
    public void WhenClippingMatrixFailsThenResultIsPreservedAndClipCacheIsUnchanged()
    {
        int viewportCallCount = 0;
        int matrixCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: false,
            setViewport: _ =>
            {
                viewportCallCount++;
                return 0;
            },
            setSurfaceToClippingMatrix: _ =>
            {
                matrixCallCount++;
                return matrixCallCount == 1 ? 0 : Direct3D9Factory.GenericFailureHResult;
            });
        viewportCallCount = 0;
        matrixCallCount = 1;

        int result = device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 10, 12));

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 1, 2, false, new Direct3D9PointAndSizeRect(0, 0, 16, 20)),
            (result, viewportCallCount, matrixCallCount, device.IsClipSet, device.GetClipRect()));
    }

    [TestMethod]
    public void WhenScissorIsUnsupportedThenSurfaceToClippingMatrixUsesHalfPixelOffsetAndYFlip()
    {
        List<(Transformstatetype State, Matrix4x4 Matrix)> transforms = [];
        using Direct3D9Device device = CreateDevice(
            supportsScissor: false,
            setTransform: (state, matrix) =>
            {
                transforms.Add((state, matrix));
                return 0;
            },
            useProductionClippingMatrix: true);
        transforms.Clear();

        int result = device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 11, 14));

        float reciprocalWidth = 1f / 10f;
        float reciprocalHeight = 1f / 12f;
        Matrix4x4 expectedProjection = Matrix4x4.Identity;
        expectedProjection.M11 = 2f * reciprocalWidth;
        expectedProjection.M41 = -((1f * expectedProjection.M11) + 1f + reciprocalWidth);
        expectedProjection.M22 = -2f * reciprocalHeight;
        expectedProjection.M42 = (-2f * expectedProjection.M22) + 1f + reciprocalHeight;
        Assert.AreEqual(
            (0, World, Matrix4x4.Identity, Transformstatetype.View, Matrix4x4.Identity, Transformstatetype.Projection, expectedProjection),
            (result, transforms[0].State, transforms[0].Matrix, transforms[1].State, transforms[1].Matrix, transforms[2].State, transforms[2].Matrix));
    }

    [TestMethod]
    public void WhenApplyingSurfaceToClippingMatrixFailsThenFirstFailureStopsTransformsAndClipCacheIsUnchanged()
    {
        List<Transformstatetype> transforms = [];
        bool failView = false;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: false,
            setTransform: (state, _) =>
            {
                transforms.Add(state);
                return failView && state == Transformstatetype.View
                    ? Direct3D9Factory.GenericFailureHResult
                    : 0;
            },
            useProductionClippingMatrix: true);
        transforms.Clear();
        failView = true;

        int result = device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 11, 14));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, new Direct3D9PointAndSizeRect(0, 0, 16, 20)),
            (result, device.IsClipSet, device.GetClipRect()));
        CollectionAssert.AreEqual(new[] { World, Transformstatetype.View }, transforms);
    }

    [TestMethod]
    public void WhenClipSetFlagIsClearedThenCachedRectangleIsPreservedIndependently()
    {
        using Direct3D9Device device = CreateDevice(supportsScissor: true, setScissorRect: _ => 0);
        Assert.AreEqual(0, device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 10, 12)));
        Direct3D9PointAndSizeRect cachedClip = device.ClipCache;

        device.SetClipSet(false);

        Assert.AreEqual((false, cachedClip, new Direct3D9PointAndSizeRect(0, 0, 16, 20)),
            (device.IsClipSet, device.ClipCache, device.GetClipRect()));
    }

    [TestMethod]
    public void WhenDefaultCacheIsResetThenCachedClipRectangleIsPreservedIndependently()
    {
        using Direct3D9Device device = CreateDevice(supportsScissor: true, setScissorRect: _ => 0);
        Assert.AreEqual(0, device.SetClipRect(new Direct3D9SurfaceRect(1, 2, 10, 12)));
        Direct3D9PointAndSizeRect cachedClip = device.ClipCache;

        device.ResetScissorAndClipCache();

        Assert.AreEqual((false, cachedClip, new Direct3D9PointAndSizeRect(0, 0, 16, 20)),
            (device.IsClipSet, device.ClipCache, device.GetClipRect()));
    }

    [TestMethod]
    public void WhenSettingClipAfterDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice(supportsScissor: true, setScissorRect: _ => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetClipRect(null));
    }

    [TestMethod]
    public void WhenGettingClipAfterDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice(supportsScissor: true, setScissorRect: _ => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.GetClipRect());
    }

    private static Direct3D9Device CreateDevice(
        bool supportsScissor,
        Func<Direct3D9PointAndSizeRect?, int>? setScissorRect = null,
        Func<Viewport9, int>? setViewport = null,
        Func<Direct3D9PointAndSizeRect, int>? setSurfaceToClippingMatrix = null,
        Direct3D9SetTransform? setTransform = null,
        bool useProductionClippingMatrix = false)
    {
        Caps9 capabilities = default;
        if (supportsScissor)
        {
            capabilities.RasterCaps = (uint) D3D9.PrastercapsScissortest;
        }

        Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            getRenderTargetDescription: _ => new SurfaceDesc(width: 16, height: 20),
            setRenderTarget: _ => 0,
            setViewport: setViewport ?? (_ => 0),
            setScissorRect: setScissorRect,
            setSurfaceToClippingMatrix: useProductionClippingMatrix
                ? null
                : setSurfaceToClippingMatrix ?? (_ => 0),
            setTransform: setTransform);
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), null);
        Assert.AreEqual(0, device.SetRenderTarget(surface));
        return device;
    }
}
