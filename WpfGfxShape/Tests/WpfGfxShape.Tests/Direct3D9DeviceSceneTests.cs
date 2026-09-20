using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceSceneTests
{
    private static int _beginSceneResult;
    private static int _endSceneResult;
    private static int _clearResult;
    private static uint _clearCount;
    private static uint _clearFlags;
    private static uint _clearColor;
    private static float _clearDepth;
    private static uint _clearStencil;
    private static int _beginSceneCallCount;
    private static int _endSceneCallCount;
    private static int _clearCallCount;

    [TestInitialize]
    public void Initialize()
    {
        _beginSceneResult = 0;
        _endSceneResult = 0;
        _clearResult = 0;
        _clearCount = 0;
        _clearFlags = 0;
        _clearColor = 0;
        _clearDepth = 0;
        _clearStencil = 0;
        _beginSceneCallCount = 0;
        _endSceneCallCount = 0;
        _clearCallCount = 0;
    }

    [TestMethod]
    public void WhenBeginSceneReturnsDriverInternalErrorThenFailureIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _beginSceneResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.BeginScene();

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, false, 1),
            (result, device.UnusableReasonHResult, device.IsInScene, _beginSceneCallCount));
    }

    [TestMethod]
    public void WhenEndSceneReturnsDriverInternalErrorThenFailureIsMappedAndSceneIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Assert.AreEqual(0, device.BeginScene());
        _endSceneResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.EndScene();

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, true, 1),
            (result, device.UnusableReasonHResult, device.IsInScene, _endSceneCallCount));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void WhenBeginSceneSucceedsThenHResultIsPreservedAndSceneIsCommitted(int beginSceneResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _beginSceneResult = beginSceneResult;

        int result = device.BeginScene();

        Assert.AreEqual((beginSceneResult, true, 1), (result, device.IsInScene, _beginSceneCallCount));
    }

    [DataTestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenBeginSceneFailsThenHResultIsPreservedAndSceneIsNotCommitted(int beginSceneResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _beginSceneResult = beginSceneResult;

        int result = device.BeginScene();

        Assert.AreEqual((beginSceneResult, false, 1, 0),
            (result, device.IsInScene, _beginSceneCallCount, device.UnusableReasonHResult));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    public void WhenEndSceneSucceedsThenHResultIsPreservedAndSceneIsCleared(int endSceneResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Assert.AreEqual(0, device.BeginScene());
        _endSceneResult = endSceneResult;

        int result = device.EndScene();

        Assert.AreEqual((endSceneResult, false, 1), (result, device.IsInScene, _endSceneCallCount));
    }

    [DataTestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenEndSceneFailsThenHResultIsPreservedAndSceneRemainsCommitted(int endSceneResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Assert.AreEqual(0, device.BeginScene());
        _endSceneResult = endSceneResult;

        int result = device.EndScene();

        Assert.AreEqual((endSceneResult, true, 1, 0),
            (result, device.IsInScene, _endSceneCallCount, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenPresentIsPreflightedAsUnusableOutsideSceneThenNativeCallsAreSkipped()
    {
        int presentCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9SwapChain swapChain = new(
            device.ResourceManager,
            null,
            present: () =>
            {
                presentCallCount++;
                return 0;
            });
        device.MarkUnusable(Direct3D9Factory.DeviceLostHResult);

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 0, 0, 0, false),
            (state.HResult, _beginSceneCallCount, _endSceneCallCount, presentCallCount, device.IsInScene));
    }

    [TestMethod]
    public void WhenPresentIsPreflightedAsUnusableInsideSceneThenNativeCallsAreSkippedAndSceneIsPreserved()
    {
        int presentCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9SwapChain swapChain = new(
            device.ResourceManager,
            null,
            present: () =>
            {
                presentCallCount++;
                return 0;
            });
        Assert.AreEqual(0, device.BeginScene());
        device.MarkUnusable(Direct3D9Factory.DeviceLostHResult);

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 1, 0, 0, true),
            (state.HResult, _beginSceneCallCount, _endSceneCallCount, presentCallCount, device.IsInScene));
    }

    [TestMethod]
    public void WhenPresentSucceedsInsideSceneThenSceneIsEndedAndRestored()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: static () => 0);
        Assert.AreEqual(0, device.BeginScene());

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual((0, 2, 1, true),
            (state.HResult, _beginSceneCallCount, _endSceneCallCount, device.IsInScene));
    }

    [TestMethod]
    public void WhenPresentIsOccludedInsideSceneThenSceneIsEndedAndRestored()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            presentFailureDelay: static _ => { },
            postWindowMessage: static (_, _) => { });
        using Direct3D9SwapChain swapChain = new(
            device.ResourceManager,
            null,
            present: static () => Direct3D9Factory.PresentOccludedHResult);
        Assert.AreEqual(0, device.BeginScene());

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual((0, false, 2, 1, true),
            (state.HResult, state.PresentProcessed, _beginSceneCallCount, _endSceneCallCount, device.IsInScene));
    }

    [TestMethod]
    public void WhenPresentIsOccludedThenRegisteredWakeMessageIsPostedOnceToDestinationWindow()
    {
        const uint registeredMessage = 0xC123;
        nint destinationWindow = 0x123456;
        List<string> calls = [];
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            registerWindowMessage: messageName =>
            {
                calls.Add($"register:{messageName}");
                return registeredMessage;
            },
            presentFailureDelay: milliseconds => calls.Add($"delay:{milliseconds}"),
            postWindowMessage: (window, message) => calls.Add($"post:{window}:{message}"));
        using Direct3D9SwapChain swapChain = new(
            device.ResourceManager,
            null,
            presentWithParameters: request =>
            {
                calls.Add($"present:{request.DestinationWindowOverride}");
                return Direct3D9Factory.PresentOccludedHResult;
            });

        Direct3D9DeviceState state = device.Present(
            swapChain,
            new Direct3D9PresentRequest(null, null, null, DestinationWindowOverride: destinationWindow));

        Assert.AreEqual((0, false), (state.HResult, state.PresentProcessed));
        CollectionAssert.AreEqual(
            new[]
            {
                "register:NeedsRePresentOnWake",
                $"present:{destinationWindow}",
                "delay:100",
                $"post:{destinationWindow}:{registeredMessage}"
            },
            calls);
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(Direct3D9Factory.PresentModeChangedHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    public void WhenPresentIsNotOccludedThenWakeNotificationHasNoSideEffects(int presentResult)
    {
        int delayCallCount = 0;
        int postCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            presentFailureDelay: _ => delayCallCount++,
            postWindowMessage: (_, _) => postCallCount++);
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: () => presentResult);

        _ = device.Present(swapChain, default);

        Assert.AreEqual((0, 0), (delayCallCount, postCallCount));
    }

    [TestMethod]
    public void WhenPresentFailsInsideSceneThenSceneRemainsEnded()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9SwapChain swapChain = new(
            device.ResourceManager,
            null,
            present: static () => Direct3D9Factory.DeviceLostHResult);
        Assert.AreEqual(0, device.BeginScene());

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual((Direct3D9Factory.DeviceLostHResult, 1, 1, false),
            (state.HResult, _beginSceneCallCount, _endSceneCallCount, device.IsInScene));
    }

    [TestMethod]
    public void WhenEndSceneFailsBeforePresentThenPresentAndRestoreAreSkipped()
    {
        int presentCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: () =>
        {
            presentCallCount++;
            return 0;
        });
        Assert.AreEqual(0, device.BeginScene());
        _endSceneResult = Direct3D9Factory.GenericFailureHResult;

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 1, 1, true),
            (state.HResult, presentCallCount, _beginSceneCallCount, _endSceneCallCount, device.IsInScene));
    }

    [TestMethod]
    public void WhenSceneRestoreFailsAfterSuccessfulPresentThenRestoreFailureIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: static () => 0);
        Assert.AreEqual(0, device.BeginScene());
        _beginSceneResult = Direct3D9Factory.GenericFailureHResult;

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 2, 1, false),
            (state.HResult, _beginSceneCallCount, _endSceneCallCount, device.IsInScene));
    }

    [TestMethod]
    public void WhenPresentSucceedsThenMarkerIsRecordedAfterSceneRestore()
    {
        int markerCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, () =>
        {
            markerCallCount++;
            Assert.AreEqual((2, 1), (_beginSceneCallCount, _endSceneCallCount));
            return 0;
        });
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: static () => 0);
        Assert.AreEqual(0, device.BeginScene());

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual((0, 1), (state.HResult, markerCallCount));
    }

    [DataTestMethod]
    [DataRow(Direct3D9Factory.PresentOccludedHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenPresentIsNotProcessedThenMarkerIsNotRecorded(int presentResult)
    {
        int markerCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, () =>
        {
            markerCallCount++;
            return 0;
        });
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: () => presentResult);

        Direct3D9DeviceState state = device.Present(swapChain, default);

        int expectedResult = presentResult == Direct3D9Factory.PresentOccludedHResult ? 0 : presentResult;
        Assert.AreEqual((expectedResult, 0), (state.HResult, markerCallCount));
    }

    [TestMethod]
    public void WhenSceneRestoreFailsAfterProcessedPresentThenMarkerIsNotRecorded()
    {
        int markerCallCount = 0;
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, () =>
        {
            markerCallCount++;
            return 0;
        });
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: static () => 0);
        Assert.AreEqual(0, device.BeginScene());
        _beginSceneResult = Direct3D9Factory.GenericFailureHResult;

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (state.HResult, markerCallCount));
    }

    [TestMethod]
    public void WhenMarkerFailsAfterProcessedPresentThenMarkerFailureIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            static () => Direct3D9Factory.GenericFailureHResult);
        using Direct3D9SwapChain swapChain = new(device.ResourceManager, null, present: static () => 0);

        Direct3D9DeviceState state = device.Present(swapChain, default);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, state.HResult);
    }

    [TestMethod]
    public void WhenSceneStateIsReadAfterDeviceWasDisposedThenAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.IsInScene);
    }

    [TestMethod]
    public void WhenClearingTargetThenNativeSlot43ReceivesTargetArguments()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.ClearTarget(0xFF123456);

        Assert.AreEqual(
            (0, 0u, 1u, 0xFF123456u, 0f, 0u, 1),
            (result, _clearCount, _clearFlags, _clearColor, _clearDepth, _clearStencil, _clearCallCount));
    }

    [TestMethod]
    public void WhenClearingDepthThenNativeSlot43ReceivesDepthArguments()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.ClearDepth(0.75f);

        Assert.AreEqual(
            (0, 0u, 2u, 0u, 0.75f, 0u, 1),
            (result, _clearCount, _clearFlags, _clearColor, _clearDepth, _clearStencil, _clearCallCount));
    }

    [DataTestMethod]
    [DataRow(1)]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenClearDoesNotReturnDriverInternalErrorThenHResultIsPreservedWithoutMarkingDeviceUnusable(
        int clearResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _clearResult = clearResult;

        int result = device.ClearTarget(0xFF123456);

        Assert.AreEqual((clearResult, 0, 1), (result, device.UnusableReasonHResult, _clearCallCount));
    }

    [TestMethod]
    public void WhenClearTargetReturnsDriverInternalErrorThenFailureIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _clearResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.ClearTarget(0xFF123456);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1),
            (result, device.UnusableReasonHResult, _clearCallCount));
    }

    [TestMethod]
    public void WhenClearDepthReturnsDriverInternalErrorThenFailureIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _clearResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.ClearDepth(0.5f);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1),
            (result, device.UnusableReasonHResult, _clearCallCount));
    }

    [TestMethod]
    public void WhenClearingAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.ClearTarget(0xFF123456);

        Assert.AreEqual((0, 1), (result, _clearCallCount));
    }

    [TestMethod]
    public void WhenClearingAfterDeviceWasDisposedThenNativeCallIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.ClearTarget(0xFF123456));
        Assert.AreEqual(0, _clearCallCount);
    }

    [TestMethod]
    public void WhenClearTargetHasUnknownDepthStencilStateThenSurfaceIsClearedBeforeTarget()
    {
        List<string> calls = [];
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "depth:null" : "depth:set");
                return 0;
            },
            clear: (_, _, _, _, _) =>
            {
                calls.Add("clear");
                return 0;
            });

        int result = device.ClearTarget(0xFF123456);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new[] { "depth:null", "clear" }, calls);
    }

    [TestMethod]
    public void WhenDepthStencilSurfaceMatchesTargetSizeThenTargetIsClearedWithoutUnbinding()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateClearDevice(calls, 0);
        using Direct3D9Surface renderTarget = new(new Direct3D9ResourceManager(), null);
        Assert.AreEqual(0, device.SetRenderTarget(renderTarget));
        Assert.AreEqual(0, device.SetDepthStencilSurfaceInline((IDirect3DSurface9*) 1, 16, 24));
        calls.Clear();

        int result = device.ClearTarget(0xFF123456);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new[] { "clear" }, calls);
    }

    [DataTestMethod]
    [DataRow(15u, 24u)]
    [DataRow(16u, 23u)]
    public void WhenOneDepthStencilDimensionIsSmallerThanTargetThenSurfaceIsUnboundBeforeClear(
        uint depthStencilWidth,
        uint depthStencilHeight)
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateClearDevice(calls, 0);
        using Direct3D9Surface renderTarget = new(new Direct3D9ResourceManager(), null);
        Assert.AreEqual(0, device.SetRenderTarget(renderTarget));
        Assert.AreEqual(
            0,
            device.SetDepthStencilSurfaceInline((IDirect3DSurface9*) 1, depthStencilWidth, depthStencilHeight));
        calls.Clear();

        int result = device.ClearTarget(0xFF123456);

        Assert.AreEqual(0, result);
        CollectionAssert.AreEqual(new[] { "depth:null", "clear" }, calls);
    }

    [TestMethod]
    public void WhenDepthStencilUnbindForClearReturnsNonzeroSuccessThenTargetClearContinuesAndKnownNullIsCached()
    {
        List<string> calls = [];
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setDepthStencilSurface: surface =>
            {
                calls.Add("depth:null");
                return 1;
            },
            clear: (_, _, _, _, _) =>
            {
                calls.Add("clear");
                return 0;
            });

        int firstResult = device.ClearTarget(0xFF123456);
        int secondResult = device.ClearTarget(0xFF123456);

        Assert.AreEqual((0, 0), (firstResult, secondResult));
        CollectionAssert.AreEqual(new[] { "depth:null", "clear", "clear" }, calls);
    }

    [DataTestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult, 0)]
    [DataRow(Direct3D9Factory.DeviceLostHResult, Direct3D9Factory.DeviceLostHResult, 0)]
    [DataRow(
        Direct3D9Factory.DriverInternalErrorHResult,
        Direct3D9Factory.DisplayStateInvalidHResult,
        Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenDepthStencilUnbindForClearFailsThenFirstFailureIsReturnedAndTargetClearIsSkipped(
        int unbindResult,
        int expectedResult,
        int expectedUnusableReason)
    {
        int clearCallCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setDepthStencilSurface: _ => unbindResult,
            clear: (_, _, _, _, _) =>
            {
                clearCallCount++;
                return 0;
            });

        int result = device.ClearTarget(0xFF123456);

        Assert.AreEqual(
            (expectedResult, expectedUnusableReason, 0),
            (result, device.UnusableReasonHResult, clearCallCount));
    }

    [TestMethod]
    public void WhenDepthStencilUnbindForClearFailsThenUnknownStateIsRetriedBeforeNextClear()
    {
        List<string> calls = [];
        int unbindCallCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setDepthStencilSurface: _ =>
            {
                calls.Add("depth:null");
                unbindCallCount++;
                return unbindCallCount == 1 ? Direct3D9Factory.GenericFailureHResult : 0;
            },
            clear: (_, _, _, _, _) =>
            {
                calls.Add("clear");
                return 0;
            });

        int firstResult = device.ClearTarget(0xFF123456);
        int secondResult = device.ClearTarget(0xFF123456);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (firstResult, secondResult));
        CollectionAssert.AreEqual(new[] { "depth:null", "depth:null", "clear" }, calls);
    }

    private static Direct3D9Device CreateClearDevice(List<string> calls, int unbindResult)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            getRenderTargetDescription: _ => new SurfaceDesc(width: 16, height: 24),
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setSurfaceToClippingMatrix: _ => 0,
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "depth:null" : "depth:set");
                return surface is null ? unbindResult : 0;
            },
            clear: (_, _, _, _, _) =>
            {
                calls.Add("clear");
                return 0;
            });
    }

    private static Direct3D9Device CreateDevice(
        IDirect3DDevice9* device,
        Func<int>? recordSuccessfulPresent = null,
        Direct3D9RegisterWindowMessage? registerWindowMessage = null,
        Action<uint>? presentFailureDelay = null,
        Direct3D9PostWindowMessage? postWindowMessage = null)
    {
        Direct3D9Device result = new(
            device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            setDepthStencilSurface: _ => 0,
            recordSuccessfulPresent: recordSuccessfulPresent ?? (() => 0),
            registerWindowMessage: registerWindowMessage ?? (_ => 0xC000),
            presentFailureDelay: presentFailureDelay ?? (_ => { }),
            postWindowMessage: postWindowMessage ?? ((_, _) => { }));
        _ = result.ForceSetDepthStencilSurface(null, 0, 0);
        return result;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int BeginScene(IDirect3DDevice9* self)
    {
        _beginSceneCallCount++;
        return _beginSceneResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int EndScene(IDirect3DDevice9* self)
    {
        _endSceneCallCount++;
        return _endSceneResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Clear(
        IDirect3DDevice9* self,
        uint count,
        Rect* rectangles,
        uint flags,
        uint color,
        float depth,
        uint stencil)
    {
        _clearCount = count;
        _clearFlags = flags;
        _clearColor = color;
        _clearDepth = depth;
        _clearStencil = stencil;
        _clearCallCount++;
        return _clearResult;
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
            vtable[41] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int>) &BeginScene;
            vtable[42] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int>) &EndScene;
            vtable[43] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, Rect*, uint, uint, float, uint, int>) &Clear;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
