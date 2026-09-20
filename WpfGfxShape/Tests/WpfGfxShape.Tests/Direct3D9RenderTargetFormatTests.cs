using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class Direct3D9RenderTargetFormatTests
{
    [TestMethod]
    public void WhenFormatTestSucceedsThenFirstResultIsReused()
    {
        using Direct3D9Device device = CreateDevice();
        int testCount = 0;

        int first = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                testCount++;
                return 0;
            },
            out int? firstGetDeviceContextResult);
        int second = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                testCount++;
                return Direct3D9Factory.GenericFailureHResult;
            },
            out int? secondGetDeviceContextResult);

        Assert.AreEqual((0, 0, 1, null, null), (first, second, testCount, firstGetDeviceContextResult, secondGetDeviceContextResult));
    }

    [TestMethod]
    public void WhenFormatTestReturnsNonzeroSuccessThenFirstResultIsReused()
    {
        using Direct3D9Device device = CreateDevice();
        int testCount = 0;

        int first = device.CheckRenderTargetFormat(
            Format.A8R8G8B8,
            _ =>
            {
                testCount++;
                return 1;
            },
            out _);
        int second = device.CheckRenderTargetFormat(
            Format.A8R8G8B8,
            _ =>
            {
                testCount++;
                return 2;
            },
            out _);

        Assert.AreEqual((1, 1, 1), (first, second, testCount));
    }

    [TestMethod]
    [DataRow(Format.X8R8G8B8)]
    [DataRow(Format.A8R8G8B8)]
    [DataRow(Format.A2R10G10B10)]
    public void WhenSupportedFormatIsCheckedThenDeviceIsEnteredOnlyForTheOperation(Format format)
    {
        using Direct3D9Device device = CreateDevice();
        bool enteredDuringTest = false;

        int result = device.CheckRenderTargetFormat(
            format,
            _ =>
            {
                enteredDuringTest = device.IsEntered();
                return 0;
            },
            out _);

        Assert.AreEqual((0, true, false), (result, enteredDuringTest, device.IsEntered()));
    }

    [TestMethod]
    public void WhenFormatCheckFailsThenDeviceEntryIsExited()
    {
        using Direct3D9Device device = CreateDevice();

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ => Direct3D9Factory.InvalidCallHResult,
            out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, false), (result, device.IsEntered()));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.OutOfVideoMemoryHResult)]
    [DataRow(Direct3D9Factory.OutOfMemoryHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenFormatTestHasContextDependentFailureThenNextCallRetests(int failure)
    {
        using Direct3D9Device device = CreateDevice();
        int testCount = 0;

        int first = device.CheckRenderTargetFormat(
            Format.A8R8G8B8,
            _ =>
            {
                testCount++;
                return failure;
            },
            out _);
        int second = device.CheckRenderTargetFormat(
            Format.A8R8G8B8,
            _ =>
            {
                testCount++;
                return 0;
            },
            out _);

        Assert.AreEqual((failure, 0, 2), (first, second, testCount));
    }

    [TestMethod]
    public void WhenStableFormatTestFailsBeforeGetDeviceContextThenFailurePopulatesBothStatuses()
    {
        using Direct3D9Device device = CreateDevice();

        int result = device.CheckRenderTargetFormat(
            Format.A2R10G10B10,
            _ => Direct3D9Factory.InvalidCallHResult,
            out int? getDeviceContextResult);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.InvalidCallHResult), (result, getDeviceContextResult));
    }

    [TestMethod]
    public void WhenGetDeviceContextWasTestedThenLaterFormatFailureDoesNotOverwriteIt()
    {
        using Direct3D9Device device = CreateDevice();

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            status =>
            {
                status.TestGetDeviceContext(
                    () => new Direct3D9SurfaceDeviceContextResult(0, 0),
                    _ => 0);
                return Direct3D9Factory.InvalidCallHResult;
            },
            out int? getDeviceContextResult);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 0), (result, getDeviceContextResult));
    }

    [TestMethod]
    public void WhenTestingDifferentFormatsThenEachFormatHasIndependentStatus()
    {
        using Direct3D9Device device = CreateDevice();
        int testCount = 0;

        int first = device.CheckRenderTargetFormat(Format.X8R8G8B8, _ => ++testCount, out _);
        int second = device.CheckRenderTargetFormat(Format.A8R8G8B8, _ => ++testCount, out _);

        Assert.AreEqual((1, 2, 2), (first, second, testCount));
    }

    [TestMethod]
    public void WhenHalChecksDepthStencilMatchThenUsesNativeParametersBeforeRemainingTest()
    {
        (uint Adapter, Devtype DeviceType, Format DisplayFormat, Format RenderTargetFormat, Format DepthStencilFormat)? parameters = null;
        bool remainingTestCalled = false;
        using Direct3D9Device device = CreateDevice(
            adapterOrdinal: 3,
            displayFormat: Format.R5G6B5,
            checkDepthStencilMatch: (adapter, deviceType, display, renderTarget, depthStencil) =>
            {
                parameters = (adapter, deviceType, display, renderTarget, depthStencil);
                return 0;
            });

        int result = device.CheckRenderTargetFormat(
            Format.A8R8G8B8,
            _ =>
            {
                remainingTestCalled = true;
                return 0;
            },
            out _);

        Assert.AreEqual(
            (0, true, (uint) 3, Devtype.Hal, Format.R5G6B5, Format.A8R8G8B8, Format.D24S8),
            (result, remainingTestCalled, parameters?.Adapter, parameters?.DeviceType, parameters?.DisplayFormat, parameters?.RenderTargetFormat, parameters?.DepthStencilFormat));
    }

    [TestMethod]
    public void WhenDepthStencilMatchFailsThenRemainingTestIsNotCalled()
    {
        bool remainingTestCalled = false;
        using Direct3D9Device device = CreateDevice(
            checkDepthStencilMatch: (_, _, _, _, _) => Direct3D9Factory.InvalidCallHResult);

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                remainingTestCalled = true;
                return 0;
            },
            out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, false), (result, remainingTestCalled));
    }

    [TestMethod]
    public void WhenSoftwareDeviceChecksFormatThenDepthStencilMatchIsSkipped()
    {
        int depthStencilCheckCount = 0;
        using Direct3D9Device device = CreateDevice(
            deviceType: Devtype.SW,
            checkDepthStencilMatch: (_, _, _, _, _) =>
            {
                depthStencilCheckCount++;
                return Direct3D9Factory.InvalidCallHResult;
            });

        int result = device.CheckRenderTargetFormat(Format.X8R8G8B8, _ => 0, out _);

        Assert.AreEqual((0, 0), (result, depthStencilCheckCount));
    }

    [TestMethod]
    public void WhenFocusWindowIsNullThenCreatesNativeLockableRenderTargetBeforeRemainingTest()
    {
        var calls = new List<string>();
        (uint Width, uint Height, Format Format, MultisampleType MultisampleType, uint Quality, bool Lockable)? parameters = null;
        using Direct3D9Device device = CreateDevice(
            checkDepthStencilMatch: (_, _, _, _, _) =>
            {
                calls.Add("depth");
                return 0;
            },
            createRenderTargetForFormatTest: (width, height, format, multisampleType, quality, lockable) =>
            {
                calls.Add("render-target");
                parameters = (width, height, format, multisampleType, quality, lockable);
                return 0;
            },
            setRenderTargetForFormatTest: () =>
            {
                calls.Add("set-render-target");
                return 0;
            },
            clearDepthStencilSurfaceForFormatTest: () =>
            {
                calls.Add("clear-depth-stencil");
                return 0;
            },
            createLockableTextureForFormatTest: (_, _, _, _, _, _) =>
            {
                calls.Add("texture");
                return 0;
            });

        int result = device.CheckRenderTargetFormat(
            Format.A2R10G10B10,
            _ =>
            {
                calls.Add("remaining");
                return 0;
            },
            out _);

        CollectionAssert.AreEqual(new[] { "depth", "render-target", "set-render-target", "clear-depth-stencil", "texture", "remaining" }, calls);
        Assert.AreEqual(
            (0, (uint) 128, (uint) 128, Format.A2R10G10B10, MultisampleType.MultisampleNone, (uint) 0, true),
            (result, parameters?.Width, parameters?.Height, parameters?.Format, parameters?.MultisampleType, parameters?.Quality, parameters?.Lockable));
    }

    [TestMethod]
    public void WhenLockableRenderTargetCreationFailsThenRemainingTestIsNotCalled()
    {
        bool remainingTestCalled = false;
        using Direct3D9Device device = CreateDevice(
            createRenderTargetForFormatTest: (_, _, _, _, _, _) => Direct3D9Factory.InvalidCallHResult);

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                remainingTestCalled = true;
                return 0;
            },
            out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, false), (result, remainingTestCalled));
    }

    [TestMethod]
    public void WhenFocusWindowIsNotNullThenTestsLockableSecondarySwapChainBeforeRemainingTest()
    {
        var calls = new List<string>();
        PresentParameters? parameters = null;
        using Direct3D9Device device = CreateDevice(
            focusWindow: 123,
            testLockableSwapChainForFormatTest: (presentParameters, status) =>
            {
                calls.Add("swap-chain");
                parameters = presentParameters;
                status.TestGetDeviceContext(
                    () => new Direct3D9SurfaceDeviceContextResult(0, 456),
                    deviceContext =>
                    {
                        calls.Add($"release-{deviceContext}");
                        return Direct3D9Factory.InvalidCallHResult;
                    });
                return 0;
            },
            setRenderTargetForFormatTest: () =>
            {
                calls.Add("set-render-target");
                return 0;
            },
            clearDepthStencilSurfaceForFormatTest: () =>
            {
                calls.Add("clear-depth-stencil");
                return 0;
            },
            createLockableTextureForFormatTest: (_, _, _, _, _, _) =>
            {
                calls.Add("texture");
                return 0;
            });

        int result = device.CheckRenderTargetFormat(
            Format.A8R8G8B8,
            _ =>
            {
                calls.Add("remaining");
                return 0;
            },
            out int? getDeviceContextResult);

        CollectionAssert.AreEqual(new[] { "swap-chain", "release-456", "set-render-target", "clear-depth-stencil", "texture", "remaining" }, calls);
        PresentParameters actual = parameters.GetValueOrDefault();
        PresentParameters expected = new(
            backBufferWidth: 128,
            backBufferHeight: 128,
            backBufferFormat: Format.A8R8G8B8,
            backBufferCount: 1,
            multiSampleType: MultisampleType.MultisampleNone,
            multiSampleQuality: 0,
            swapEffect: Swapeffect.Copy,
            hDeviceWindow: 123,
            windowed: true,
            enableAutoDepthStencil: false,
            autoDepthStencilFormat: Format.Unknown,
            flags: unchecked((uint) D3D9.PresentflagLockableBackbuffer),
            fullScreenRefreshRateInHz: 0,
            presentationInterval: D3D9.PresentIntervalImmediate);
        Assert.AreEqual((0, 0, expected), (result, getDeviceContextResult.GetValueOrDefault(), actual));
    }

    [TestMethod]
    public void WhenLockableSecondarySwapChainCreationFailsThenRemainingTestIsNotCalled()
    {
        bool remainingTestCalled = false;
        using Direct3D9Device device = CreateDevice(
            focusWindow: 1,
            testLockableSwapChainForFormatTest: (_, _) => Direct3D9Factory.InvalidCallHResult);

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                remainingTestCalled = true;
                return 0;
            },
            out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, false), (result, remainingTestCalled));
    }

    [TestMethod]
    public void WhenLockableSecondarySwapChainTestSucceedsThenCachedFormatSkipsSecondCreationAndGetDeviceContext()
    {
        int swapChainTestCount = 0;
        int getDeviceContextCount = 0;
        using Direct3D9Device device = CreateDevice(
            focusWindow: 1,
            testLockableSwapChainForFormatTest: (_, status) =>
            {
                swapChainTestCount++;
                status.TestGetDeviceContext(
                    () =>
                    {
                        getDeviceContextCount++;
                        return new Direct3D9SurfaceDeviceContextResult(0, 0);
                    },
                    _ => 0);
                return 0;
            });

        _ = device.CheckRenderTargetFormat(Format.A2R10G10B10, _ => 0, out _);
        int result = device.CheckRenderTargetFormat(Format.A2R10G10B10, _ => Direct3D9Factory.InvalidCallHResult, out int? getDeviceContextResult);

        Assert.AreEqual((0, 0, 1, 1), (result, getDeviceContextResult, swapChainTestCount, getDeviceContextCount));
    }

    [TestMethod]
    public void WhenSettingRenderTargetFailsThenRemainingTestIsNotCalled()
    {
        bool remainingTestCalled = false;
        using Direct3D9Device device = CreateDevice(
            setRenderTargetForFormatTest: () => Direct3D9Factory.InvalidCallHResult);

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                remainingTestCalled = true;
                return 0;
            },
            out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, false), (result, remainingTestCalled));
    }

    [TestMethod]
    public void WhenClearingDepthStencilSurfaceFailsThenRemainingTestIsNotCalled()
    {
        bool remainingTestCalled = false;
        using Direct3D9Device device = CreateDevice(
            clearDepthStencilSurfaceForFormatTest: () => Direct3D9Factory.InvalidCallHResult);

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                remainingTestCalled = true;
                return 0;
            },
            out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, false), (result, remainingTestCalled));
    }

    [TestMethod]
    public void WhenCreatingLockableTextureThenUsesNativeDescriptionBeforeRemainingTest()
    {
        (uint Width, uint Height, uint Levels, uint Usage, Format Format, Pool Pool)? parameters = null;
        bool remainingTestCalled = false;
        using Direct3D9Device device = CreateDevice(
            createLockableTextureForFormatTest: (width, height, levels, usage, format, pool) =>
            {
                parameters = (width, height, levels, usage, format, pool);
                return 0;
            });

        int result = device.CheckRenderTargetFormat(
            Format.A2R10G10B10,
            _ =>
            {
                remainingTestCalled = true;
                return 0;
            },
            out _);

        Assert.AreEqual(
            (0, true, (uint) 128, (uint) 128, (uint) 1, (uint) 0, Format.A8R8G8B8, Pool.Managed),
            (result, remainingTestCalled, parameters?.Width, parameters?.Height, parameters?.Levels, parameters?.Usage, parameters?.Format, parameters?.Pool));
    }

    [TestMethod]
    public void WhenLockableTextureCreationFailsThenRemainingTestIsNotCalled()
    {
        bool remainingTestCalled = false;
        using Direct3D9Device device = CreateDevice(
            createLockableTextureForFormatTest: (_, _, _, _, _, _) => Direct3D9Factory.InvalidCallHResult);

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                remainingTestCalled = true;
                return 0;
            },
            out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, false), (result, remainingTestCalled));
    }

    [TestMethod]
    public void WhenTestingRenderTargetFormatThenRendersTextureBeforeRemainingTest()
    {
        var calls = new List<string>();
        using Direct3D9Device device = CreateDevice(
            createLockableTextureForFormatTest: (_, _, _, _, _, _) =>
            {
                calls.Add("texture");
                return 0;
            },
            renderTextureForFormatTest: () =>
            {
                calls.Add("render-texture");
                return 0;
            });

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                calls.Add("remaining");
                return 0;
            },
            out _);

        CollectionAssert.AreEqual(new[] { "texture", "render-texture", "remaining" }, calls);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenTestingRenderTargetFormatThenSceneSurroundsRenderTexture()
    {
        var calls = new List<string>();
        using Direct3D9Device device = CreateDevice(
            beginSceneForFormatTest: () =>
            {
                calls.Add("begin-scene");
                return 0;
            },
            renderTextureForFormatTest: () =>
            {
                calls.Add("render-texture");
                return 0;
            },
            endSceneForFormatTest: () =>
            {
                calls.Add("end-scene");
                return 0;
            });

        _ = device.CheckRenderTargetFormat(Format.X8R8G8B8, _ => 0, out _);

        CollectionAssert.AreEqual(new[] { "begin-scene", "render-texture", "end-scene" }, calls);
    }

    [TestMethod]
    public void WhenBeginSceneFailsThenRenderTextureIsNotCalled()
    {
        bool renderTextureCalled = false;
        using Direct3D9Device device = CreateDevice(
            beginSceneForFormatTest: () => Direct3D9Factory.InvalidCallHResult,
            renderTextureForFormatTest: () =>
            {
                renderTextureCalled = true;
                return 0;
            });

        int result = device.CheckRenderTargetFormat(Format.X8R8G8B8, _ => 0, out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, false), (result, renderTextureCalled));
    }

    [TestMethod]
    public void WhenRenderingTextureFailsThenEndSceneRunsAndFirstFailureIsPreserved()
    {
        bool endSceneCalled = false;
        using Direct3D9Device device = CreateDevice(
            renderTextureForFormatTest: () => Direct3D9Factory.InvalidCallHResult,
            endSceneForFormatTest: () =>
            {
                endSceneCalled = true;
                return Direct3D9Factory.DriverInternalErrorHResult;
            });

        int result = device.CheckRenderTargetFormat(Format.X8R8G8B8, _ => 0, out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, true), (result, endSceneCalled));
    }

    [TestMethod]
    public void WhenSceneBodySucceedsThenEndSceneFailureIsReturned()
    {
        using Direct3D9Device device = CreateDevice(
            endSceneForFormatTest: () => Direct3D9Factory.InvalidCallHResult);

        int result = device.CheckRenderTargetFormat(Format.X8R8G8B8, _ => 0, out _);

        Assert.AreEqual(Direct3D9Factory.InvalidCallHResult, result);
    }

    [TestMethod]
    public void WhenRenderingTextureFailsThenRemainingTestIsNotCalled()
    {
        bool remainingTestCalled = false;
        using Direct3D9Device device = CreateDevice(
            renderTextureForFormatTest: () => Direct3D9Factory.InvalidCallHResult);

        int result = device.CheckRenderTargetFormat(
            Format.X8R8G8B8,
            _ =>
            {
                remainingTestCalled = true;
                return 0;
            },
            out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, false), (result, remainingTestCalled));
    }

    [TestMethod]
    public void WhenLockableRenderTargetTestSucceedsThenCachedFormatSkipsSecondCreation()
    {
        int renderTargetCreateCount = 0;
        using Direct3D9Device device = CreateDevice(
            createRenderTargetForFormatTest: (_, _, _, _, _, _) =>
            {
                renderTargetCreateCount++;
                return 0;
            });

        _ = device.CheckRenderTargetFormat(Format.A8R8G8B8, _ => 0, out _);
        int result = device.CheckRenderTargetFormat(Format.A8R8G8B8, _ => Direct3D9Factory.InvalidCallHResult, out _);

        Assert.AreEqual((0, 1), (result, renderTargetCreateCount));
    }

    [TestMethod]
    public void WhenSimpleFormatCheckUsesConfiguredDelegateThenFormatAndNonzeroSuccessArePreserved()
    {
        Format? checkedFormat = null;
        int checkCount = 0;
        using Direct3D9Device device = CreateDevice(
            checkRenderTargetFormat: format =>
            {
                checkedFormat = format;
                checkCount++;
                return 1;
            });

        int result = device.CheckRenderTargetFormat(Format.A2R10G10B10);

        Assert.AreEqual((1, Format.A2R10G10B10, 1), (result, checkedFormat, checkCount));
    }

    [TestMethod]
    public void WhenSimpleFormatCheckDelegateFailsThenFailureIsReturnedWithoutRetryOrMapping()
    {
        int checkCount = 0;
        using Direct3D9Device device = CreateDevice(
            checkRenderTargetFormat: _ =>
            {
                checkCount++;
                return Direct3D9Factory.DriverInternalErrorHResult;
            });

        int result = device.CheckRenderTargetFormat(Format.A8R8G8B8);

        Assert.AreEqual((Direct3D9Factory.DriverInternalErrorHResult, 1), (result, checkCount));
    }

    [TestMethod]
    public void WhenSimpleFormatCheckRunsAfterDisposeThenDelegateIsNotCalled()
    {
        int checkCount = 0;
        Direct3D9Device device = CreateDevice(
            checkRenderTargetFormat: _ =>
            {
                checkCount++;
                return 0;
            });
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.CheckRenderTargetFormat(Format.X8R8G8B8));
        Assert.AreEqual(0, checkCount);
    }

    [TestMethod]
    public void WhenCheckingUnsupportedFormatThenReturnsInvalidArgumentWithoutTesting()
    {
        using Direct3D9Device device = CreateDevice();
        int testCount = 0;

        int result = device.CheckRenderTargetFormat(
            Format.R5G6B5,
            _ =>
            {
                testCount++;
                return 0;
            },
            out int? getDeviceContextResult);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, 0, null, false),
            (result, testCount, getDeviceContextResult, device.IsEntered()));
    }

    [TestMethod]
    public void WhenCheckingFormatAfterDisposeThenThrowsObjectDisposedException()
    {
        Direct3D9Device device = CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.CheckRenderTargetFormat(Format.X8R8G8B8, _ => 0, out _));
    }

    private static Direct3D9Device CreateDevice(
        uint adapterOrdinal = 0,
        Devtype deviceType = Devtype.Hal,
        Format displayFormat = Format.X8R8G8B8,
        Func<uint, Devtype, Format, Format, Format, int>? checkDepthStencilMatch = null,
        nint focusWindow = 0,
        Func<uint, uint, Format, MultisampleType, uint, bool, int>? createRenderTargetForFormatTest = null,
        Func<PresentParameters, Direct3D9TargetFormatTestStatus, int>? testLockableSwapChainForFormatTest = null,
        Func<int>? setRenderTargetForFormatTest = null,
        Func<int>? clearDepthStencilSurfaceForFormatTest = null,
        Func<uint, uint, uint, uint, Format, Pool, int>? createLockableTextureForFormatTest = null,
        Func<int>? beginSceneForFormatTest = null,
        Func<int>? renderTextureForFormatTest = null,
        Func<int>? endSceneForFormatTest = null,
        Func<Format, int>? checkRenderTargetFormat = null)
    {
        return new Direct3D9Device(
            null,
            null,
            adapterOrdinal,
            deviceType,
            0,
            new PresentParameters(windowed: true),
            displayMode: new Displaymode(format: displayFormat),
            checkDepthStencilMatch: checkDepthStencilMatch ?? ((_, _, _, _, _) => 0),
            focusWindow: focusWindow,
            createRenderTargetForFormatTest: createRenderTargetForFormatTest ?? ((_, _, _, _, _, _) => 0),
            testLockableSwapChainForFormatTest: testLockableSwapChainForFormatTest,
            setRenderTargetForFormatTest: setRenderTargetForFormatTest ?? (() => 0),
            clearDepthStencilSurfaceForFormatTest: clearDepthStencilSurfaceForFormatTest ?? (() => 0),
            createLockableTextureForFormatTest: createLockableTextureForFormatTest ?? ((_, _, _, _, _, _) => 0),
            beginSceneForFormatTest: beginSceneForFormatTest ?? (() => 0),
            renderTextureForFormatTest: renderTextureForFormatTest ?? (() => 0),
            endSceneForFormatTest: endSceneForFormatTest ?? (() => 0),
            checkRenderTargetFormat: checkRenderTargetFormat);
    }
}
