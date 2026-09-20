using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
public sealed unsafe class Direct3D9RenderTargetStateTests
{
    private const Transformstatetype World = (Transformstatetype) 256;

    private static nint _nativeRenderTarget;
    private static uint _nativeRenderTargetIndex;
    private static int _nativeSetRenderTargetResult;
    private static int _nativeSetRenderTargetCallCount;
    private static int _nativeBeginSceneCallCount;
    private static int _nativeBeginSceneResult;
    private static int _nativeEndSceneCallCount;
    private static int _nativeEndSceneResult;
    private static readonly List<string> NativeCalls = [];

    [TestInitialize]
    public void Initialize()
    {
        _nativeRenderTarget = 0;
        _nativeRenderTargetIndex = uint.MaxValue;
        _nativeSetRenderTargetResult = 0;
        _nativeSetRenderTargetCallCount = 0;
        _nativeBeginSceneCallCount = 0;
        _nativeBeginSceneResult = 0;
        _nativeEndSceneCallCount = 0;
        _nativeEndSceneResult = 0;
        NativeCalls.Clear();
    }

    [TestMethod]
    public void WhenRenderTargetChangesThenCachesAndViewportFollowNativeOrder()
    {
        List<string> calls = [];
        Viewport9 nativeViewport = default;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            getDescription: _ =>
            {
                calls.Add("description");
                return new SurfaceDesc(width: 16, height: 24);
            },
            setRenderTarget: _ =>
            {
                calls.Add("renderTarget");
                return 0;
            },
            setViewport: viewport =>
            {
                calls.Add("viewport");
                nativeViewport = viewport;
                return 0;
            },
            setSurfaceToClippingMatrix: _ =>
            {
                calls.Add("matrix");
                return 0;
            });
        using Direct3D9Surface surface = CreateSurface();
        device.SetClipSet(true);

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual(
            (0, false, new Direct3D9PointAndSizeRect(0, 0, 16, 24), new Direct3D9PointAndSizeRect(0, 0, 16, 24)),
            (result, device.IsClipSet, device.ScissorRectCache, device.ViewportCache));
        CollectionAssert.AreEqual(new[] { "description", "renderTarget", "viewport", "matrix" }, calls);
        Assert.AreEqual((0u, 0u, 16u, 24u, 0f, 1f),
            (nativeViewport.X, nativeViewport.Y, nativeViewport.Width, nativeViewport.Height, nativeViewport.MinZ, nativeViewport.MaxZ));
    }

    [TestMethod]
    public void WhenRenderTargetSetFailsThenFollowingStateIsUnchanged()
    {
        int viewportCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            getDescription: _ => new SurfaceDesc(width: 16, height: 24),
            setRenderTarget: _ => Direct3D9Factory.GenericFailureHResult,
            setViewport: _ =>
            {
                viewportCallCount++;
                return 0;
            });
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9PointAndSizeRect previousScissor = new(1, 2, 3, 4);
        device.SetClipSet(true);
        device.ScissorRectChanged(previousScissor);

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, previousScissor, default(Direct3D9PointAndSizeRect), 0),
            (result, device.IsClipSet, device.ScissorRectCache, device.ViewportCache, viewportCallCount));
    }

    [TestMethod]
    public void WhenScissorIsUnsupportedThenRenderTargetDoesNotChangeScissorCache()
    {
        Direct3D9PointAndSizeRect previousScissor = new(1, 2, 3, 4);
        using Direct3D9Device device = CreateDevice(
            supportsScissor: false,
            getDescription: _ => new SurfaceDesc(width: 16, height: 24),
            setRenderTarget: _ => 0,
            setViewport: _ => 0);
        using Direct3D9Surface surface = CreateSurface();
        device.ScissorRectChanged(previousScissor);

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual((0, previousScissor), (result, device.ScissorRectCache));
    }

    [TestMethod]
    public void WhenViewportSetFailsThenRenderTargetCachesStillMatchRequestedViewport()
    {
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            getDescription: _ => new SurfaceDesc(width: 16, height: 24),
            setRenderTarget: _ => 0,
            setViewport: _ => Direct3D9Factory.GenericFailureHResult);
        using Direct3D9Surface surface = CreateSurface();
        device.SetClipSet(true);

        int result = device.SetRenderTarget(surface);

        Direct3D9PointAndSizeRect expected = new(0, 0, 16, 24);
        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, expected, expected),
            (result, device.IsClipSet, device.ScissorRectCache, device.ViewportCache));
    }

    [TestMethod]
    public void WhenClippingMatrixSetFailsThenFailureIsReturnedAfterViewport()
    {
        int matrixCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            getDescription: _ => new SurfaceDesc(width: 16, height: 24),
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setSurfaceToClippingMatrix: _ =>
            {
                matrixCallCount++;
                return Direct3D9Factory.GenericFailureHResult;
            });
        using Direct3D9Surface surface = CreateSurface();

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 1), (result, matrixCallCount));
    }

    [TestMethod]
    [DataRow(0u, 24u, true, false)]
    [DataRow(16u, 0u, false, true)]
    public void WhenRenderTargetViewportIsDegenerateThenNativeFloatingPointMatrixSemanticsArePreserved(
        uint width,
        uint height,
        bool expectDegenerateWidth,
        bool expectDegenerateHeight)
    {
        List<(Transformstatetype State, Matrix4x4 Matrix)> transforms = [];
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            getDescription: _ => new SurfaceDesc(width: width, height: height),
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setTransform: (state, matrix) =>
            {
                transforms.Add((state, matrix));
                return 0;
            },
            useProductionClippingMatrix: true);
        using Direct3D9Surface surface = CreateSurface();

        int result = device.SetRenderTarget(surface);

        Matrix4x4 projection = transforms[2].Matrix;
        Assert.AreEqual(
            (0, World, Transformstatetype.View, Transformstatetype.Projection, expectDegenerateWidth, expectDegenerateWidth,
                expectDegenerateHeight, expectDegenerateHeight),
            (result, transforms[0].State, transforms[1].State, transforms[2].State,
                float.IsPositiveInfinity(projection.M11), float.IsNaN(projection.M41),
                float.IsNegativeInfinity(projection.M22), float.IsNaN(projection.M42)));
    }

    [TestMethod]
    public void WhenNativeRenderTargetSetReturnsDriverInternalErrorThenFailureIsReinterpretedAndFollowingStateIsSkipped()
    {
        int viewportCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            getDescription: _ => new SurfaceDesc(width: 16, height: 24),
            setRenderTarget: _ => Direct3D9Factory.DriverInternalErrorHResult,
            setViewport: _ =>
            {
                viewportCallCount++;
                return 0;
            });
        using Direct3D9Surface surface = CreateSurface();

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 0),
            (result, device.UnusableReasonHResult, viewportCallCount));
    }

    [TestMethod]
    public void WhenViewportSetReturnsDriverInternalErrorThenFailureIsReinterpretedAndClippingMatrixIsSkipped()
    {
        int matrixCallCount = 0;
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            getDescription: _ => new SurfaceDesc(width: 16, height: 24),
            setRenderTarget: _ => 0,
            setViewport: _ => Direct3D9Factory.DriverInternalErrorHResult,
            setSurfaceToClippingMatrix: _ =>
            {
                matrixCallCount++;
                return 0;
            });
        using Direct3D9Surface surface = CreateSurface();

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 0),
            (result, device.UnusableReasonHResult, matrixCallCount));
    }

    [TestMethod]
    public void WhenClippingMatrixSetReturnsDriverInternalErrorThenFailureIsReinterpreted()
    {
        using Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            getDescription: _ => new SurfaceDesc(width: 16, height: 24),
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setSurfaceToClippingMatrix: _ => Direct3D9Factory.DriverInternalErrorHResult);
        using Direct3D9Surface surface = CreateSurface();

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenViewportFailsAfterNativeRenderTargetCommitThenOriginalFailureWinsAndCurrentUseIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject renderTargetObject = new();
        using FakeSurfaceObject dummyObject = new();
        using FakeSurfaceObject depthStencilObject = new();
        nint clearedDepthStencil = -1;
        int depthStencilCallCount = 0;
        using Direct3D9Device device = CreateNativeDevice(
            deviceObject.Device,
            dummyObject.Surface,
            depthStencilSurface =>
            {
                depthStencilCallCount++;
                clearedDepthStencil = (nint) depthStencilSurface;
                return depthStencilCallCount == 1 ? 0 : Direct3D9Factory.InvalidCallHResult;
            },
            setViewport: _ =>
            {
                _nativeSetRenderTargetResult = Direct3D9Factory.InvalidCallHResult;
                return Direct3D9Factory.GenericFailureHResult;
            });
        using Direct3D9Surface renderTarget = CreateSurface(renderTargetObject.Surface);
        Assert.AreEqual(0, device.SetDepthStencilSurfaceForCurrentRenderTarget(depthStencilObject.Surface, 16, 24));

        int result = device.SetRenderTarget(renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, 0u, (nint) dummyObject.Surface, 0, 0, false),
            (result, _nativeSetRenderTargetCallCount, _nativeRenderTargetIndex, _nativeRenderTarget,
                _nativeEndSceneCallCount, clearedDepthStencil, device.IsInScene));
    }

    [TestMethod]
    public void WhenClippingMatrixFailsAfterNativeRenderTargetCommitThenOriginalFailureWinsAndSceneIsEndedDuringRollback()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject renderTargetObject = new();
        using FakeSurfaceObject dummyObject = new();
        using Direct3D9Device device = CreateNativeDevice(
            deviceObject.Device,
            dummyObject.Surface,
            setSurfaceToClippingMatrix: _ =>
            {
                _nativeSetRenderTargetResult = Direct3D9Factory.InvalidCallHResult;
                return Direct3D9Factory.GenericFailureHResult;
            });
        using Direct3D9Surface renderTarget = CreateSurface(renderTargetObject.Surface);
        Assert.AreEqual(0, device.BeginScene());

        int result = device.SetRenderTarget(renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, (nint) dummyObject.Surface, 1, 1, false),
            (result, _nativeSetRenderTargetCallCount, _nativeRenderTarget, _nativeBeginSceneCallCount,
                _nativeEndSceneCallCount, device.IsInScene));
    }

    [TestMethod]
    public void WhenClippingMatrixFailsThenCleanupDriverInternalErrorDoesNotReplaceOriginalFailure()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject renderTargetObject = new();
        using FakeSurfaceObject dummyObject = new();
        using FakeSurfaceObject depthStencilObject = new();
        bool failDepthStencilCleanup = false;
        Direct3D9Device device = null!;
        device = CreateNativeDevice(
            deviceObject.Device,
            dummyObject.Surface,
            depthStencilSurface =>
            {
                NativeCalls.Add(depthStencilSurface == null ? "depthStencil:null" : "depthStencil:value");
                return failDepthStencilCleanup ? Direct3D9Factory.DriverInternalErrorHResult : 0;
            },
            setSurfaceToClippingMatrix: viewport =>
            {
                _ = device.BeginScene();
                _nativeEndSceneResult = Direct3D9Factory.DriverInternalErrorHResult;
                _nativeSetRenderTargetResult = Direct3D9Factory.InvalidCallHResult;
                failDepthStencilCleanup = true;
                NativeCalls.Add("matrix");
                return Direct3D9Factory.GenericFailureHResult;
            });
        using (device)
        using (Direct3D9Surface renderTarget = CreateSurface(renderTargetObject.Surface))
        {
            Assert.AreEqual(0,
                device.SetDepthStencilSurfaceForCurrentRenderTarget(depthStencilObject.Surface, 16, 24));
            NativeCalls.Clear();

            int result = device.SetRenderTarget(renderTarget);

            Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, true),
                (result, device.UnusableReasonHResult, device.IsInScene));
            CollectionAssert.AreEqual(
                new[] { "setRenderTarget", "matrix", "endScene", "setRenderTarget", "depthStencil:null" },
                NativeCalls);
        }
    }

    [TestMethod]
    public void WhenViewportReturnsDriverInternalErrorThenCleanupFailuresDoNotReplaceOuterMapping()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject renderTargetObject = new();
        using FakeSurfaceObject dummyObject = new();
        using FakeSurfaceObject depthStencilObject = new();
        bool failDepthStencilCleanup = false;
        Direct3D9Device device = null!;
        device = CreateNativeDevice(
            deviceObject.Device,
            dummyObject.Surface,
            depthStencilSurface =>
            {
                NativeCalls.Add(depthStencilSurface == null ? "depthStencil:null" : "depthStencil:value");
                return failDepthStencilCleanup ? Direct3D9Factory.InvalidCallHResult : 0;
            },
            setViewport: viewport =>
            {
                _ = device.BeginScene();
                _nativeEndSceneResult = Direct3D9Factory.GenericFailureHResult;
                _nativeSetRenderTargetResult = Direct3D9Factory.DeviceLostHResult;
                failDepthStencilCleanup = true;
                NativeCalls.Add("viewport");
                return Direct3D9Factory.DriverInternalErrorHResult;
            });
        using (device)
        using (Direct3D9Surface renderTarget = CreateSurface(renderTargetObject.Surface))
        {
            Assert.AreEqual(0,
                device.SetDepthStencilSurfaceForCurrentRenderTarget(depthStencilObject.Surface, 16, 24));
            NativeCalls.Clear();

            int result = device.SetRenderTarget(renderTarget);

            Assert.AreEqual(
                (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, true),
                (result, device.UnusableReasonHResult, device.IsInScene));
            CollectionAssert.AreEqual(
                new[] { "setRenderTarget", "viewport", "endScene", "setRenderTarget", "depthStencil:null" },
                NativeCalls);
        }
    }

    [TestMethod]
    public void WhenNativeRenderTargetIsSetThenSlot37ReceivesIndexZeroAndBorrowedSurface()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Device device = CreateNativeDevice(deviceObject.Device);
        using Direct3D9Surface surface = CreateSurface(surfaceObject.Surface);

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual((0, 0u, (nint) surfaceObject.Surface, 1),
            (result, _nativeRenderTargetIndex, _nativeRenderTarget, _nativeSetRenderTargetCallCount));
    }

    [TestMethod]
    public void WhenNativeRenderTargetReturnsNonzeroSuccessThenFollowingSuccessfulStagesDetermineResultAndNativeCallRunsOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Device device = CreateNativeDevice(deviceObject.Device);
        using Direct3D9Surface surface = CreateSurface(surfaceObject.Surface);
        _nativeSetRenderTargetResult = 1;

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual((0, 1), (result, _nativeSetRenderTargetCallCount));
    }

    [TestMethod]
    public void WhenNativeRenderTargetReturnsDriverInternalErrorThenFailureIsMappedAfterSingleCall()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Device device = CreateNativeDevice(deviceObject.Device);
        using Direct3D9Surface surface = CreateSurface(surfaceObject.Surface);
        _nativeSetRenderTargetResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.SetRenderTarget(surface);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1),
            (result, device.UnusableReasonHResult, _nativeSetRenderTargetCallCount));
    }

    [TestMethod]
    public void WhenSameNativeRenderTargetIsSetTwiceThenSecondCallIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Device device = CreateNativeDevice(deviceObject.Device);
        using Direct3D9Surface surface = CreateSurface(surfaceObject.Surface);

        int firstResult = device.SetRenderTarget(surface);
        int secondResult = device.SetRenderTarget(surface);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, _nativeSetRenderTargetCallCount));
    }

    [TestMethod]
    public void WhenOwningCurrentRenderTargetIsReleasedThenDeviceEndsSceneAndBorrowsDummyBeforeComRelease()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject renderTargetObject = new(D3D9.UsageRendertarget);
        using FakeSurfaceObject dummyObject = new();
        using FakeSurfaceObject depthStencilObject = new();
        nint dummyIdentity = (nint) dummyObject.Surface;
        nint clearedDepthStencil = -1;
        using Direct3D9Device device = CreateNativeDevice(
            deviceObject.Device,
            dummyObject.Surface,
            depthStencilSurface =>
            {
                clearedDepthStencil = (nint) depthStencilSurface;
                return 0;
            });
        Assert.AreEqual(0, Direct3D9Surface.TryCreate(
            device.ResourceManager,
            renderTargetObject.Surface,
            out Direct3D9Surface? renderTarget));
        Assert.IsNotNull(renderTarget);
        using (renderTarget)
        {
            Assert.AreEqual(0, device.SetRenderTarget(renderTarget));
            Assert.AreEqual(0, device.SetDepthStencilSurfaceForCurrentRenderTarget(depthStencilObject.Surface, 16, 24));
        }

        Assert.AreEqual(
            (2, dummyIdentity, 1, 1, 0, false),
            (_nativeSetRenderTargetCallCount, _nativeRenderTarget, _nativeBeginSceneCallCount,
                _nativeEndSceneCallCount, clearedDepthStencil, device.IsInScene));
    }

    [TestMethod]
    public void WhenNonCurrentRenderTargetIsReleasedThenDeviceStateIsUnchanged()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject currentObject = new(D3D9.UsageRendertarget);
        using FakeSurfaceObject otherObject = new(D3D9.UsageRendertarget);
        using FakeSurfaceObject dummyObject = new();
        using Direct3D9Device device = CreateNativeDevice(deviceObject.Device, dummyObject.Surface);
        Assert.AreEqual(0, Direct3D9Surface.TryCreate(
            device.ResourceManager,
            currentObject.Surface,
            out Direct3D9Surface? current));
        Assert.AreEqual(0, Direct3D9Surface.TryCreate(
            device.ResourceManager,
            otherObject.Surface,
            out Direct3D9Surface? other));
        Assert.IsNotNull(current);
        Assert.IsNotNull(other);
        using (current)
        {
            Assert.AreEqual(0, device.SetRenderTarget(current));
            NativeCalls.Clear();
            other.Dispose();

            Assert.AreEqual((1, 1, 0, true),
                (_nativeSetRenderTargetCallCount, _nativeBeginSceneCallCount, _nativeEndSceneCallCount,
                    device.IsInScene));
            CollectionAssert.AreEqual(new[] { "releaseSurface" }, NativeCalls);
        }
    }

    [TestMethod]
    public void WhenCurrentRenderTargetIsReleasedThenCleanupFailuresAreIgnoredAndDepthStencilPrecedesComRelease()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject renderTargetObject = new(D3D9.UsageRendertarget);
        using FakeSurfaceObject dummyObject = new();
        using FakeSurfaceObject depthStencilObject = new();
        bool failDepthStencilCleanup = false;
        using Direct3D9Device device = CreateNativeDevice(
            deviceObject.Device,
            dummyObject.Surface,
            depthStencilSurface =>
            {
                NativeCalls.Add(depthStencilSurface == null ? "depthStencil:null" : "depthStencil:value");
                return failDepthStencilCleanup ? Direct3D9Factory.GenericFailureHResult : 0;
            });
        Assert.AreEqual(0, Direct3D9Surface.TryCreate(
            device.ResourceManager,
            renderTargetObject.Surface,
            out Direct3D9Surface? renderTarget));
        Assert.IsNotNull(renderTarget);
        Assert.AreEqual(0, device.SetRenderTarget(renderTarget));
        Assert.AreEqual(0,
            device.SetDepthStencilSurfaceForCurrentRenderTarget(depthStencilObject.Surface, 16, 24));
        failDepthStencilCleanup = true;
        _nativeEndSceneResult = Direct3D9Factory.DeviceLostHResult;
        _nativeSetRenderTargetResult = Direct3D9Factory.DriverInternalErrorHResult;
        NativeCalls.Clear();

        renderTarget.Dispose();

        CollectionAssert.AreEqual(
            new[] { "endScene", "setRenderTarget", "depthStencil:null", "releaseSurface" },
            NativeCalls);
        Assert.AreEqual((2, 1, 1, true, 0),
            (_nativeSetRenderTargetCallCount, _nativeBeginSceneCallCount, _nativeEndSceneCallCount,
                device.IsInScene, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenCurrentRenderTargetAndDepthStencilShareSurfaceThenRenderTargetCleanupPrecedesSingleComRelease()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sharedSurfaceObject = new(D3D9.UsageRendertarget | D3D9.UsageDepthstencil);
        using FakeSurfaceObject dummyObject = new();
        using Direct3D9Device device = CreateNativeDevice(
            deviceObject.Device,
            dummyObject.Surface,
            depthStencilSurface =>
            {
                NativeCalls.Add(depthStencilSurface == null ? "depthStencil:null" : "depthStencil:value");
                return 0;
            });
        Assert.AreEqual(0, Direct3D9Surface.TryCreate(
            device.ResourceManager,
            sharedSurfaceObject.Surface,
            out Direct3D9Surface? sharedSurface));
        Assert.IsNotNull(sharedSurface);
        Assert.AreEqual(0, device.SetRenderTarget(sharedSurface));
        Assert.AreEqual(0,
            device.SetDepthStencilSurfaceForCurrentRenderTarget(sharedSurface.SurfaceForDeviceCall, 16, 24));
        NativeCalls.Clear();

        sharedSurface.Dispose();

        CollectionAssert.AreEqual(
            new[] { "endScene", "setRenderTarget", "depthStencil:null", "releaseSurface" },
            NativeCalls);
    }

    [TestMethod]
    public void WhenPresentFailsThenDeviceBorrowsDummyAndReleasesDepthStencilWithoutEndingScene()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject renderTargetObject = new(D3D9.UsageRendertarget);
        using FakeSurfaceObject dummyObject = new();
        using FakeSurfaceObject depthStencilObject = new();
        nint dummyIdentity = (nint) dummyObject.Surface;
        nint clearedDepthStencil = -1;
        using Direct3D9Device device = CreateNativeDevice(
            deviceObject.Device,
            dummyObject.Surface,
            depthStencilSurface =>
            {
                clearedDepthStencil = (nint) depthStencilSurface;
                return 0;
            });
        using Direct3D9Surface renderTarget = CreateSurface(renderTargetObject.Surface);
        Assert.AreEqual(0, device.SetRenderTarget(renderTarget));
        Assert.AreEqual(0, device.SetDepthStencilSurfaceForCurrentRenderTarget(depthStencilObject.Surface, 16, 24));

        Direct3D9DeviceState state = device.HandlePresentFailure(Direct3D9Factory.InvalidArgumentHResult);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, 2, dummyIdentity, 1, 0, 0, true),
            (state.HResult, _nativeSetRenderTargetCallCount, _nativeRenderTarget, _nativeBeginSceneCallCount,
                _nativeEndSceneCallCount, clearedDepthStencil, device.IsInScene));
    }

    [TestMethod]
    public void WhenRenderTargetChangesDuringSceneThenSceneIsPairedAroundNativeStateSetup()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject firstSurfaceObject = new();
        using FakeSurfaceObject secondSurfaceObject = new();
        using Direct3D9Device device = CreateNativeDevice(deviceObject.Device);
        using Direct3D9Surface firstSurface = CreateSurface(firstSurfaceObject.Surface);
        using Direct3D9Surface secondSurface = CreateSurface(secondSurfaceObject.Surface);
        Assert.AreEqual(0, device.SetRenderTarget(firstSurface));

        int result = device.SetRenderTarget(secondSurface);

        Assert.AreEqual(
            (0, 2, 2, 1, (nint) secondSurfaceObject.Surface, true),
            (result, _nativeSetRenderTargetCallCount, _nativeBeginSceneCallCount, _nativeEndSceneCallCount,
                _nativeRenderTarget, device.IsInScene));
    }

    [TestMethod]
    public void WhenInitialEndSceneFailsThenDescriptionAndNativeRenderTargetAreSkippedWithoutRollback()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject firstSurfaceObject = new();
        using FakeSurfaceObject secondSurfaceObject = new();
        int descriptionCallCount = 0;
        using Direct3D9Device device = CreateNativeDevice(
            deviceObject.Device,
            getDescription: _ =>
            {
                descriptionCallCount++;
                return new SurfaceDesc(width: 16, height: 24);
            });
        using Direct3D9Surface firstSurface = CreateSurface(firstSurfaceObject.Surface);
        using Direct3D9Surface secondSurface = CreateSurface(secondSurfaceObject.Surface);
        Assert.AreEqual(0, device.SetRenderTarget(firstSurface));
        descriptionCallCount = 0;
        _nativeEndSceneResult = Direct3D9Factory.GenericFailureHResult;

        int result = device.SetRenderTarget(secondSurface);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, 1, 1, 0, (nint) firstSurfaceObject.Surface, true),
            (result, _nativeSetRenderTargetCallCount, _nativeBeginSceneCallCount, _nativeEndSceneCallCount,
                descriptionCallCount, _nativeRenderTarget, device.IsInScene));
    }

    [TestMethod]
    public void WhenBeginSceneFailsThenOriginalFailureWinsAndCommittedRenderTargetIsReleasedWithoutEndingSceneAgain()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject renderTargetObject = new();
        using FakeSurfaceObject dummyObject = new();
        using FakeSurfaceObject depthStencilObject = new();
        nint clearedDepthStencil = -1;
        int depthStencilCallCount = 0;
        using Direct3D9Device device = CreateNativeDevice(
            deviceObject.Device,
            dummyObject.Surface,
            depthStencilSurface =>
            {
                depthStencilCallCount++;
                clearedDepthStencil = (nint) depthStencilSurface;
                return depthStencilCallCount == 1 ? 0 : Direct3D9Factory.InvalidCallHResult;
            });
        using Direct3D9Surface renderTarget = CreateSurface(renderTargetObject.Surface);
        Assert.AreEqual(0, device.SetDepthStencilSurfaceForCurrentRenderTarget(depthStencilObject.Surface, 16, 24));
        _nativeBeginSceneResult = Direct3D9Factory.GenericFailureHResult;
        _nativeSetRenderTargetResult = 0;

        int result = device.SetRenderTarget(renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, 1, 0, (nint) dummyObject.Surface, 0, false),
            (result, _nativeSetRenderTargetCallCount, _nativeBeginSceneCallCount, _nativeEndSceneCallCount,
                _nativeRenderTarget, clearedDepthStencil, device.IsInScene));
    }

    [TestMethod]
    public void WhenSettingNativeRenderTargetAfterDisposeThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9Device device = CreateNativeDevice(deviceObject.Device);
        using Direct3D9Surface surface = CreateSurface(surfaceObject.Surface);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetRenderTarget(surface));
        Assert.AreEqual(0, _nativeSetRenderTargetCallCount);
    }

    [TestMethod]
    public void WhenSettingRenderTargetAfterDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice(
            supportsScissor: true,
            getDescription: _ => new SurfaceDesc(width: 16, height: 24),
            setRenderTarget: _ => 0,
            setViewport: _ => 0);
        using Direct3D9Surface surface = CreateSurface();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetRenderTarget(surface));
    }

    private static Direct3D9Device CreateDevice(
        bool supportsScissor,
        Func<Direct3D9Surface, SurfaceDesc> getDescription,
        Func<Direct3D9Surface, int> setRenderTarget,
        Func<Viewport9, int> setViewport,
        Func<Direct3D9PointAndSizeRect, int>? setSurfaceToClippingMatrix = null,
        Direct3D9SetTransform? setTransform = null,
        bool useProductionClippingMatrix = false)
    {
        Caps9 capabilities = default;
        if (supportsScissor)
        {
            capabilities.RasterCaps = (uint) D3D9.PrastercapsScissortest;
        }

        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            getRenderTargetDescription: getDescription,
            setRenderTarget: setRenderTarget,
            setViewport: setViewport,
            setSurfaceToClippingMatrix: useProductionClippingMatrix
                ? null
                : setSurfaceToClippingMatrix ?? (_ => 0),
            setTransform: setTransform);
    }

    private static Direct3D9Device CreateNativeDevice(
        IDirect3DDevice9* device,
        IDirect3DSurface9* dummyBackBuffer = null,
        Direct3D9SetDepthStencilSurface? setDepthStencilSurface = null,
        Func<Viewport9, int>? setViewport = null,
        Func<Direct3D9PointAndSizeRect, int>? setSurfaceToClippingMatrix = null,
        Func<Direct3D9Surface, SurfaceDesc>? getDescription = null)
    {
        return new Direct3D9Device(
            device,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getRenderTargetDescription: getDescription ?? (_ => new SurfaceDesc(width: 16, height: 24)),
            setViewport: setViewport ?? (_ => 0),
            setSurfaceToClippingMatrix: setSurfaceToClippingMatrix ?? (_ => 0),
            setDepthStencilSurface: setDepthStencilSurface,
            dummyBackBuffer: dummyBackBuffer);
    }

    private static Direct3D9Surface CreateSurface(IDirect3DSurface9* surface = null)
    {
        return new Direct3D9Surface(new Direct3D9ResourceManager(), surface);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetRenderTarget(IDirect3DDevice9* self, uint renderTargetIndex, IDirect3DSurface9* renderTarget)
    {
        _nativeRenderTargetIndex = renderTargetIndex;
        _nativeRenderTarget = (nint) renderTarget;
        _nativeSetRenderTargetCallCount++;
        NativeCalls.Add("setRenderTarget");
        return _nativeSetRenderTargetResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int BeginScene(IDirect3DDevice9* self)
    {
        _nativeBeginSceneCallCount++;
        return _nativeBeginSceneResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int EndScene(IDirect3DDevice9* self)
    {
        _nativeEndSceneCallCount++;
        NativeCalls.Add("endScene");
        return _nativeEndSceneResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSurface(IDirect3DSurface9* self)
    {
        NativeCalls.Add("releaseSurface");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        *description = new SurfaceDesc(width: 16, height: 24, usage: *((uint*) self + 2));
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 44);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[37] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DSurface9*, int>) &SetRenderTarget;
            vtable[41] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int>) &BeginScene;
            vtable[42] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int>) &EndScene;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private sealed class FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        public FakeSurfaceObject(uint usage = 0)
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 16);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 3;
            Surface->LpVtbl = vtable;
            *((uint*) Surface + 2) = usage;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSurface;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetSurfaceDescription;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
