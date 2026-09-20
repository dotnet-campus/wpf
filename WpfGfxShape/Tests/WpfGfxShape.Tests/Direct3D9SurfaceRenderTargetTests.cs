using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    private static int _videoBitmapAddRefCount;
    private static int _videoBitmapReleaseCount;
    private static int _videoBitmapGetSizeResult;
    private static uint _videoBitmapWidth;
    private static uint _videoBitmapHeight;
    private static readonly List<string> RenderTargetReleaseOrder = [];
    private static Direct3D9Device? _releaseOrderDevice;

    [TestMethod]
    public unsafe void WhenDisplayRenderTargetIsCreatedOrResizedThenContentsAreInvalid()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        bool initiallyValid = renderTarget.HasValidContents;

        Assert.AreEqual(0, renderTarget.Resize(16, 12));

        Assert.AreEqual((false, false), (initiallyValid, renderTarget.HasValidContents));
    }

    [TestMethod]
    public unsafe void WhenDisplayRenderingIsDisabledThenClearSucceedsWithoutMakingContentsValid()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 0));

        int result = renderTarget.Clear(new MilColorF(1f, 1f, 0f, 0f), null);

        Assert.AreEqual((0, false), (result, renderTarget.HasValidContents));
    }

    [TestMethod]
    public unsafe void WhenDisplayRenderingIsDisabledThenBegin3DSucceedsWithoutDelegating()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        int beginCalls = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                beginCalls++;
                return new Direct3D9Begin3DResult(Direct3D9Factory.GenericFailureHResult, multisampleType);
            });
        Assert.AreEqual(0, renderTarget.Resize(0, 0));

        int result = renderTarget.Begin3D(default, MilAntiAliasMode.None, useZBuffer: false, z: 0f);

        Assert.AreEqual((0, 0, false), (result, beginCalls, renderTarget.In3D));
    }

    [TestMethod]
    public unsafe void WhenDisplayRenderingIsDisabledThenEnd3DSucceedsWithoutRequiringBegin3D()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 0));

        int result = renderTarget.End3D();

        Assert.AreEqual((0, false), (result, renderTarget.In3D));
    }

    [TestMethod]
    public unsafe void WhenDisplayDrawingCompletesThenOnlySuccessMakesContentsValid()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));

        int failedResult = renderTarget.DrawBitmap(static () => Direct3D9Factory.GenericFailureHResult);
        bool validAfterFailure = renderTarget.HasValidContents;
        int successfulResult = renderTarget.DrawBitmap(static () => 0);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, 0, true),
            (failedResult, validAfterFailure, successfulResult, renderTarget.HasValidContents));
    }

    [TestMethod]
    [DataRow(0, true)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, false)]
    public unsafe void WhenDisplayBitmapDrawingDelegatesThenBaseResultIsReturnedAfterExactlyOneScopedCall(
        int drawResult,
        bool expectedValidContents)
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        int drawCalls = 0;
        bool enteredDuringDraw = false;
        bool inUseContextDuringDraw = false;

        int result = renderTarget.DrawBitmap(() =>
        {
            drawCalls++;
            enteredDuringDraw = device.IsEntered();
            inUseContextDuringDraw = device.IsInUseContext();
            return drawResult;
        });

        Assert.AreEqual(
            (drawResult, 1, true, true, expectedValidContents, false, false),
            (result, drawCalls, enteredDuringDraw, inUseContextDuringDraw, renderTarget.HasValidContents,
                device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDisplayBitmapDrawingIsDisabledThenNullOperationIsIgnoredBeforeEnteringDeviceScopes()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));

        int result = renderTarget.DrawBitmap(null!);

        Assert.AreEqual(
            (0, false, false, false),
            (result, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenPathPipelineCompletesThenOnlySuccessMakesDisplayContentsValid()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 12));
        Assert.AreEqual(0, renderTarget.Resize(16, 12));

        int realizationResult = Direct3D9Factory.GenericFailureHResult;
        int failedResult = DrawPath();
        bool validAfterFailure = renderTarget.HasValidContents;
        realizationResult = 0;
        int successfulResult = DrawPath();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, 0, true),
            (failedResult, validAfterFailure, successfulResult, renderTarget.HasValidContents));

        int DrawPath() => renderTarget.DrawPath(
            Matrix4x4.Identity,
            1,
            0,
            0,
            2,
            _ => realizationResult,
            static (nint _, out MilRectF bounds) =>
            {
                bounds = new MilRectF(0, 0, 4, 4);
                return 0;
            },
            static (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                widenedShape = 0;
                return Direct3D9Factory.InternalErrorHResult;
            },
            static (nint _, bool _, out nint brush, out nint effects) =>
            {
                brush = 3;
                effects = 0;
                return 0;
            },
            static (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            static () => 0,
            static (_, _, _, _, _, _) => 0,
            static (_, _, _, _) => Direct3D9Factory.GenericFailureHResult);
    }

    [TestMethod]
    public unsafe void WhenGlyphPipelineCompletesThenOnlySuccessMakesDisplayContentsValid()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        int hardwareResult = Direct3D9Factory.GenericFailureHResult;

        int failedResult = DrawGlyphs();
        bool validAfterFailure = renderTarget.HasValidContents;
        hardwareResult = 0;
        int successfulResult = DrawGlyphs();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, 0, true),
            (failedResult, validAfterFailure, successfulResult, renderTarget.HasValidContents));

        int DrawGlyphs() => renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            static () => 0,
            static () => 0,
            _ => hardwareResult,
            static (_, _) => Direct3D9Factory.GenericFailureHResult);
    }

    [TestMethod]
    public unsafe void WhenDeviceInternalErrorWasRecordedThenPresentConsumesItBeforeSwapChain()
    {
        int presentCallCount = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCallCount++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        device.MarkUnusable(Direct3D9Factory.DriverInternalErrorHResult, mayBeMultithreadedCall: true);

        int firstResult = renderTarget.Present();
        int secondResult = renderTarget.Present();

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DisplayStateInvalidHResult, 0, false),
            (firstResult, secondResult, presentCallCount, renderTarget.IsRenderingEnabled));
    }

    [TestMethod]
    public unsafe void WhenCopyPresentSucceedsThenContentsRemainValid()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            swapEffect: Swapeffect.Copy);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Assert.AreEqual(0, renderTarget.DrawBitmap(static () => 0));

        int result = renderTarget.Present();

        Assert.AreEqual((0, true), (result, renderTarget.HasValidContents));
    }

    [TestMethod]
    public unsafe void WhenDiscardPresentSucceedsThenContentsBecomeInvalid()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Assert.AreEqual(0, renderTarget.DrawBitmap(static () => 0));

        int result = renderTarget.Present();

        Assert.AreEqual((0, false), (result, renderTarget.HasValidContents));
    }

    [TestMethod]
    public unsafe void WhenBitmapPipelineCompletesThenOnlySuccessMakesDisplayContentsValid()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Direct3D9BitmapDrawState drawState = new(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(0, 0, 4, 4));

        int failedResult = renderTarget.DrawBitmap(
            drawState,
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            static () => 0,
            static (_, _, _, _, _, _) => 0);
        bool validAfterFailure = renderTarget.HasValidContents;
        int successfulResult = renderTarget.DrawBitmap(
            drawState,
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 41;
                return 0;
            },
            static () => 0,
            static (_, _, _, _, _, _) => 0);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, 0, true),
            (failedResult, validAfterFailure, successfulResult, renderTarget.HasValidContents));
    }

    [TestMethod]
    public unsafe void WhenVideoPipelineCompletesThenOnlySuccessMakesDisplayContentsValid()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };

        int failedResult = renderTarget.DrawVideo(
            renderState,
            null,
            bitmapSource.Pointer,
            static _ => Direct3D9Factory.GenericFailureHResult);
        bool validAfterFailure = renderTarget.HasValidContents;
        int successfulResult = renderTarget.DrawVideo(renderState, null, bitmapSource.Pointer, static _ => 0);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, 0, true),
            (failedResult, validAfterFailure, successfulResult, renderTarget.HasValidContents));
    }

    [TestMethod]
    public unsafe void WhenVideoPipelineReturnsNoFrameThenDisplayContentsBecomeValid()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int drawCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                static (Direct3D9Device _, out nint bitmapSource) =>
                {
                    bitmapSource = 0;
                    return 0;
                },
                static () => Direct3D9Factory.GenericFailureHResult),
            0,
            _ => ++drawCalls);

        Assert.AreEqual((0, 0, true, true), (result, drawCalls, renderState.PrefilterEnabled, renderTarget.HasValidContents));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenDrawBitmapSucceedsWithoutDrawing()
    {
        AssertDisplayDrawingIsSkippedWhenRenderingIsDisabled(static (renderTarget, draw) => renderTarget.DrawBitmap(draw));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenBitmapPipelineDependenciesAreSkipped()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));
        int callbackCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            bitmapSource.Pointer,
            0,
            (out nint scratchBrush) =>
            {
                callbackCalls++;
                scratchBrush = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            () => ++callbackCalls,
            (_, _, _, _, _, _) => ++callbackCalls);

        Assert.AreEqual((0, 0, false, false), (result, callbackCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenDrawMesh3DSucceedsWithoutDrawing()
    {
        AssertDisplayDrawingIsSkippedWhenRenderingIsDisabled(static (renderTarget, draw) => renderTarget.DrawMesh3D(draw));
    }

    [TestMethod]
    [DataRow(0, true)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, false)]
    public unsafe void WhenDisplayMeshDrawingDelegatesThenBaseResultIsReturnedAfterExactlyOneScopedCall(
        int drawResult,
        bool expectedValidContents)
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        int drawCalls = 0;
        bool enteredDuringDraw = false;
        bool inUseContextDuringDraw = false;

        int result = renderTarget.DrawMesh3D(() =>
        {
            drawCalls++;
            enteredDuringDraw = device.IsEntered();
            inUseContextDuringDraw = device.IsInUseContext();
            return drawResult;
        });

        Assert.AreEqual(
            (drawResult, 1, true, true, expectedValidContents, false, false),
            (result, drawCalls, enteredDuringDraw, inUseContextDuringDraw, renderTarget.HasValidContents,
                device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDisplayMeshDrawingIsDisabledThenNullOperationIsIgnoredBeforeEnteringDeviceScopes()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));

        int result = renderTarget.DrawMesh3D(null!);

        Assert.AreEqual(
            (0, false, false, false),
            (result, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenMeshShaderLifecycleIsSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(0, 16);
        int calls = 0;

        int result = renderTarget.DrawMesh3D(
            () => ++calls,
            () => true,
            () => ++calls,
            () => ++calls,
            () => ++calls);

        Assert.AreEqual((0, 0), (result, calls));
    }

    [TestMethod]
    public unsafe void WhenMeshBoundsAreEmptyThenMeshPipelineIsSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        int draw3DDisabledQueries = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(4, 4, 4, 12),
            isDraw3DDisabled: () =>
            {
                draw3DDisabledQueries++;
                return true;
            });
        int calls = 0;

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => ++calls,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                calls++;
                state = default;
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                calls++;
                shader = null;
                return 0;
            });

        Assert.AreEqual((0, 0, 0, false, false), (result, calls, draw3DDisabledQueries, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingMeshThenSurfaceCoreUsesSingleDeviceUseContext()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using TestResource resource = new(device.ResourceManager);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        uint observedUseContextDepth = 0;

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () =>
            {
                resource.SetAsEvictable();
                observedUseContextDepth = resource.ActiveUseContextDepth;
                return Direct3D9Factory.GenericFailureHResult;
            },
            static (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                state = default;
                return 0;
            },
            static (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                shader = null;
                return 0;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1u, 0u, false, false),
            (result, observedUseContextDepth, resource.ActiveUseContextDepth, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenMeshRenderTargetBecomesInvalidThenBegin3DClearsBoundsAndNativePipelineIsSkipped()
    {
        Direct3D9SwapChain? swapChain = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            });
        int begin3DCalls = 0;
        int draw3DDisabledQueries = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                begin3DCalls++;
                return new Direct3D9Begin3DResult(0, multisampleType);
            },
            isDraw3DDisabled: () =>
            {
                draw3DDisabledQueries++;
                return false;
            });
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Direct3D9SurfaceRect originalBounds = renderTarget.Bounds;
        swapChain!.Dispose();
        int pipelineCalls = 0;

        int beginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 12),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);
        Direct3D9SurfaceRect boundsAfterBegin = renderTarget.Bounds;
        bool in3DAfterBegin = renderTarget.In3D;
        int drawResult = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => ++pipelineCalls,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                pipelineCalls++;
                state = default;
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                pipelineCalls++;
                shader = null;
                return 0;
            });
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, 0, 0, 0, 0, default(Direct3D9SurfaceRect), true, originalBounds, false, true, false, false),
            (beginResult, drawResult, endResult, begin3DCalls, pipelineCalls, draw3DDisabledQueries, boundsAfterBegin, in3DAfterBegin, renderTarget.Bounds, renderTarget.In3D, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingMeshToInvalidDirectSurfaceThenBegin3DClearsBoundsAndDependenciesAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        int begin3DCalls = 0;
        int draw3DDisabledQueries = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 12,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 12),
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                begin3DCalls++;
                return new Direct3D9Begin3DResult(0, multisampleType);
            },
            isDraw3DDisabled: () =>
            {
                draw3DDisabledQueries++;
                return false;
            });
        Direct3D9SurfaceRect originalBounds = renderTarget.Bounds;
        surface.Dispose();
        int dependencyCalls = 0;

        int beginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 12),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);
        Direct3D9SurfaceRect boundsAfterBegin = renderTarget.Bounds;
        int drawResult = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => ++dependencyCalls,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                dependencyCalls++;
                state = default;
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                dependencyCalls++;
                shader = null;
                return 0;
            });
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, 0, 0, 0, 0, default(Direct3D9SurfaceRect), originalBounds, false, true, false, false),
            (beginResult, drawResult, endResult, begin3DCalls, draw3DDisabledQueries, dependencyCalls, boundsAfterBegin, renderTarget.Bounds, renderTarget.In3D, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDraw3DIsDisabledThenMeshPipelineIsSkippedWithoutEnding3D()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface renderSurface = CreateSurface();
        int draw3DDisabledQueries = 0;
        int begin3DCalls = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16),
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                begin3DCalls++;
                return new Direct3D9Begin3DResult(0, multisampleType);
            },
            isDraw3DDisabled: () =>
            {
                draw3DDisabledQueries++;
                return true;
            });
        int pipelineCalls = 0;

        int beginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);
        int disabledDrawResult = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => ++pipelineCalls,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                pipelineCalls++;
                state = default;
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                pipelineCalls++;
                shader = null;
                return 0;
            });
        bool in3DAfterDisabledDraw = renderTarget.In3D;
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, true, 0, 0, 1, 1, false, false, false),
            (beginResult, disabledDrawResult, in3DAfterDisabledDraw, endResult, pipelineCalls, draw3DDisabledQueries, begin3DCalls, renderTarget.In3D, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingMeshThenShaderLifecycleRunsThroughSurfaceEntry()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawMesh3D(
            () => AddDrawMeshCall(calls, "Begin"),
            () => false,
            () => AddDrawMeshCall(calls, "Shader"),
            () => AddDrawMeshCall(calls, "FixedFunction"),
            () => AddDrawMeshCall(calls, "Finish"));

        Assert.AreEqual((0, "Begin|FixedFunction|Finish"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenPreparingVisibleMeshThenSurfaceOrderMatchesNativeAndShaderIsReleased()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            resetPerPrimitiveResourceUsage: () => calls.Add("ResetResources"),
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        Direct3D9ContextState contextState = Create3DContextState(isAntialiasingEnabled: false) with
        {
            AliasedClip = new Direct3D9SurfaceRect(2, 3, 12, 14)
        };

        int result = renderTarget.DrawMesh3D(
            contextState,
            () => AddDrawMeshCall(calls, "Brushes"),
            (Direct3D9ContextState _, Direct3D9SurfaceRect clip, out Direct3D9ProjectedMeshState state) =>
            {
                calls.Add($"Project:{clip.Left},{clip.Top},{clip.Right},{clip.Bottom}");
                state = new Direct3D9ProjectedMeshState(
                    Matrix4x4.CreateScale(2f),
                    new Direct3D9SurfaceRect(1, 2, 8, 9),
                    new MilRectF(0, 0, 1, 1),
                    IsVisible: true);
                return 0;
            },
            (Direct3D9ProjectedMeshState state, out Direct3D9DerivedMeshShader? shader) =>
            {
                calls.Add($"Derive:{state.RenderBoundsDeviceSpace.Right}");
                shader = CreateDerivedMeshShader(calls);
                return 0;
            });

        Assert.AreEqual(
            "Brushes|Description:1|RenderTarget:1|Viewport|Matrix|Scissor|ResetResources|DepthStencil|World|View|Projection|Cullmode:1|Zfunc:4|Zwriteenable:0|Description:1|Project:2,3,12,14|Derive:8|Begin|FixedFunction|Finish|ReleaseShader",
            string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenMeshBoundsDebugIsEnabledThenBoundsFailuresAreIgnoredAndShaderIsReleasedLast()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        Direct3D9Box expectedBounds = new(1, 2, 3, 4, 5, 6);

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            static () => 0,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                state = new Direct3D9ProjectedMeshState(default, default, default, IsVisible: true);
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                shader = new Direct3D9DerivedMeshShader(
                    () => AddDrawMeshCall(calls, "Begin"),
                    static () => false,
                    static () => 0,
                    () => AddDrawMeshCall(calls, "Draw"),
                    () => AddDrawMeshCall(calls, "Finish"),
                    () => calls.Add("ReleaseShader"));
                return 0;
            },
            () =>
            {
                calls.Add("DebugEnabled");
                return true;
            },
            (out Direct3D9Box bounds) =>
            {
                calls.Add("GetBounds");
                bounds = expectedBounds;
                return 0;
            },
            bounds =>
            {
                calls.Add($"DrawBounds:{bounds.LengthZ}");
                return Direct3D9Factory.GenericFailureHResult;
            });

        Assert.AreEqual(
            (0, "Begin|Draw|Finish|DebugEnabled|GetBounds|DrawBounds:6|ReleaseShader"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenMeshDrawingFailsThenBoundsDebugIsSkipped()
    {
        int debugCalls = 0;
        using Direct3D9Device device = CreateEnsureStateDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            static () => 0,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                state = new Direct3D9ProjectedMeshState(default, default, default, IsVisible: true);
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                shader = new Direct3D9DerivedMeshShader(
                    static () => 0,
                    static () => false,
                    static () => 0,
                    static () => Direct3D9Factory.GenericFailureHResult,
                    static () => 0,
                    static () => { });
                return 0;
            },
            () =>
            {
                debugCalls++;
                return true;
            },
            (out Direct3D9Box bounds) =>
            {
                debugCalls++;
                bounds = default;
                return 0;
            },
            _ => ++debugCalls);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, debugCalls));
    }

    [TestMethod]
    public unsafe void WhenDrawingFixedFunctionMeshThenSurfaceDerivesShaderAndPassInputsReachDeviceRenderer()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        Direct3D9ImmediateBrushRealizer realizer = new(
            11,
            static _ => { },
            _ => calls.Add("ReleaseRealizer"),
            static (_, _) => { },
            static _ => false,
            static _ => Direct3D9BrushType.Solid,
            static _ => false,
            static _ => false,
            static _ => 0,
            static _ => 0,
            static (_, _, _, _) => 0,
            static (_, _, _, _) => 0,
            static (_, _) => { },
            static (_, _) => { });
        Direct3D9FixedFunctionMeshShaderFactory factory = new(
            (state, value) =>
            {
                calls.Add($"ShaderState:{state}:{value}");
                return 0;
            },
            (nint brush, Direct3D9ProjectedMeshState state, out Direct3D9MeshHardwareBrush? hardwareBrush) =>
            {
                calls.Add($"DeriveBrush:{brush}:{state.RenderBoundsDeviceSpace.Right}");
                hardwareBrush = new Direct3D9MeshHardwareBrush(
                    builder => builder.SetConstant(new Direct3D9ConstantColorSource(new MilColorF(1, 1, 1, 1))),
                    () => calls.Add("ReleaseBrush"));
                return 0;
            },
            static (_, _, _) => throw new InvalidOperationException(),
            static _ => { },
            static _ => { },
            zBufferEnabled: true);
        Direct3D9FixedFunctionMeshDrawData drawData = new(
            1,
            new Vector3[] { new(0, 0, 0), new(1, 0, 0), new(0, 1, 0) },
            new Vector2[] { default, default, default },
            ReadOnlyMemory<uint>.Empty,
            new uint[] { uint.MaxValue, uint.MaxValue, uint.MaxValue },
            ReadOnlyMemory<uint>.Empty,
            default,
            () =>
            {
                calls.Add("Lighting");
                return 0;
            },
            (Direct3D9FixedFunctionPassInputs passInputs, MilCompositingMode mode, ref Direct3D9GeometryRenderer<uint> _, out Direct3D9Pipeline? pipeline) =>
            {
                calls.Add($"Setup:{mode}");
                int result = passInputs.CreateItems(out IReadOnlyList<Direct3D9FixedFunctionPipelineItem>? items);
                Direct3D9FixedFunctionPipelineItemBuilder.ReleaseOwnedColorSources(items ?? []);
                pipeline = null;
                return result < 0 ? result : Direct3D9Factory.GenericFailureHResult;
            });

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => 0,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                state = new Direct3D9ProjectedMeshState(
                    Matrix4x4.Identity,
                    new Direct3D9SurfaceRect(0, 0, 8, 9),
                    new MilRectF(0, 0, 1, 1),
                    IsVisible: true);
                return 0;
            },
            Direct3D9MeshShaderType.Diffuse,
            (out Direct3D9ImmediateBrushRealizer? surfaceSource) =>
            {
                calls.Add("GetSurfaceSource");
                surfaceSource = realizer;
                return 0;
            },
            factory,
            drawData);

        int surfaceSourceIndex = calls.IndexOf("GetSurfaceSource");
        int brushIndex = calls.IndexOf("DeriveBrush:11:8");
        int shaderStateIndex = calls.IndexOf($"ShaderState:{Renderstatetype.Zwriteenable}:1");
        int lightingIndex = calls.IndexOf("Lighting");
        int setupIndex = calls.IndexOf("Setup:SourceOver");
        int releaseBrushIndex = calls.IndexOf("ReleaseBrush");

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, true, true, true, true),
            (result,
                surfaceSourceIndex >= 0 && surfaceSourceIndex < brushIndex,
                brushIndex < shaderStateIndex,
                shaderStateIndex < lightingIndex,
                lightingIndex < setupIndex,
                setupIndex < releaseBrushIndex));
    }

    [TestMethod]
    public unsafe void WhenProjectedMeshIsInvisibleThenShaderIsNotDerivedAndScopesAreReleased()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => AddDrawMeshCall(calls, "Brushes"),
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                calls.Add("ProjectInvisible");
                state = new Direct3D9ProjectedMeshState(default, default, default, IsVisible: false);
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                calls.Add("Derive");
                shader = CreateDerivedMeshShader(calls);
                return 0;
            });

        Assert.AreEqual(
            (0, true, true, false, false),
            (result,
                calls[0] == "Brushes",
                calls[^1] == "ProjectInvisible",
                device.IsEntered(),
                device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenBrushRealizationFailsThenStateProjectionAndDerivationAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        int laterCalls = 0;

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => Direct3D9Factory.GenericFailureHResult,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                laterCalls++;
                state = default;
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                laterCalls++;
                shader = null;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, laterCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenBrushRealizationCannotPrepareMeshThenOnlyNonInvertibleMatrixIsIgnored(int failureResult, int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        int laterCalls = 0;

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => failureResult,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                laterCalls++;
                state = default;
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                laterCalls++;
                shader = null;
                return 0;
            });

        Assert.AreEqual((expectedResult, 0), (result, laterCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenMeshProjectionFailsThenOnlyNonInvertibleMatrixIsIgnored(int failureResult, int expectedResult)
    {
        using Direct3D9Device device = CreateEnsureStateDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        int deriveCalls = 0;

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => 0,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                state = default;
                return failureResult;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                deriveCalls++;
                shader = null;
                return 0;
            });

        Assert.AreEqual((expectedResult, 0), (result, deriveCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenShaderDerivationFailsAfterReturningShaderThenResultIsNormalizedAndShaderIsReleased(int failureResult, int expectedResult)
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => 0,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                state = new Direct3D9ProjectedMeshState(default, default, default, IsVisible: true);
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                shader = CreateDerivedMeshShader(calls);
                return failureResult;
            });

        Assert.AreEqual((expectedResult, "ReleaseShader"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenMeshDrawingFailsThenOnlyNonInvertibleMatrixIsIgnoredAndShaderIsReleased(int failureResult, int expectedResult)
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => 0,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                state = new Direct3D9ProjectedMeshState(default, default, default, IsVisible: true);
                return 0;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                shader = new Direct3D9DerivedMeshShader(
                    static () => 0,
                    static () => false,
                    static () => 0,
                    () => failureResult,
                    static () => 0,
                    () => calls.Add("ReleaseShader"));
                return 0;
            });

        Assert.AreEqual((expectedResult, "ReleaseShader"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenMeshPreparationIsSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(0, 16);
        int calls = 0;

        int result = renderTarget.DrawMesh3D(
            Create3DContextState(isAntialiasingEnabled: false),
            () => ++calls,
            (Direct3D9ContextState _, Direct3D9SurfaceRect _, out Direct3D9ProjectedMeshState state) =>
            {
                state = default;
                return ++calls;
            },
            (Direct3D9ProjectedMeshState _, out Direct3D9DerivedMeshShader? shader) =>
            {
                shader = null;
                return ++calls;
            });

        Assert.AreEqual((0, 0), (result, calls));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenDrawPathSucceedsWithoutDrawing()
    {
        AssertDisplayDrawingIsSkippedWhenRenderingIsDisabled(static (renderTarget, draw) => renderTarget.DrawPath(draw));
    }

    [TestMethod]
    [DataRow(0, true)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, false)]
    public unsafe void WhenDisplayPathDrawingDelegatesThenBaseResultIsReturnedAfterExactlyOneScopedCall(
        int drawResult,
        bool expectedValidContents)
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        int drawCalls = 0;
        bool enteredDuringDraw = false;
        bool inUseContextDuringDraw = false;

        int result = renderTarget.DrawPath(() =>
        {
            drawCalls++;
            enteredDuringDraw = device.IsEntered();
            inUseContextDuringDraw = device.IsInUseContext();
            return drawResult;
        });

        Assert.AreEqual(
            (drawResult, 1, true, true, expectedValidContents, false, false),
            (result, drawCalls, enteredDuringDraw, inUseContextDuringDraw, renderTarget.HasValidContents,
                device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDisplayPathDrawingIsDisabledThenNullOperationIsIgnoredBeforeEnteringDeviceScopes()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));

        int result = renderTarget.DrawPath(null!);

        Assert.AreEqual(
            (0, false, false, false),
            (result, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenPathRenderTargetIsInvalidThenNativePipelineIsSkipped()
    {
        Direct3D9SwapChain? swapChain = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        swapChain!.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            19,
            _ => ++callbackCalls,
            (nint _, out MilRectF bounds) =>
            {
                callbackCalls++;
                bounds = default;
                return 0;
            },
            (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                callbackCalls++;
                widenedShape = 0;
                return 0;
            },
            (nint _, bool _, out nint brush, out nint effects) =>
            {
                callbackCalls++;
                brush = 0;
                effects = 0;
                return 0;
            },
            (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                callbackCalls++;
                clippedShape = default;
                return 0;
            },
            () => ++callbackCalls,
            (_, _, _, _, _, _) => ++callbackCalls,
            (_, _, _, _) => ++callbackCalls);

        Assert.AreEqual(
            (0, 0, true, false, false),
            (result, callbackCalls, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingPathToInvalidDirectSurfaceThenDependenciesAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            19,
            _ => ++callbackCalls,
            (nint _, out MilRectF bounds) =>
            {
                callbackCalls++;
                bounds = default;
                return 0;
            },
            (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                callbackCalls++;
                widenedShape = 0;
                return 0;
            },
            (nint _, bool _, out nint brush, out nint effects) =>
            {
                callbackCalls++;
                brush = 0;
                effects = 0;
                return 0;
            },
            (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                callbackCalls++;
                clippedShape = default;
                return 0;
            },
            () => ++callbackCalls,
            (_, _, _, _, _, _) => ++callbackCalls,
            (_, _, _, _) => ++callbackCalls);

        Assert.AreEqual((0, 0, false, false), (result, callbackCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingPathWithFillAndStrokeThenNativeRealizationAndFillOrderIsPreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 100, 80));
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            worldToDevice,
            11,
            13,
            17,
            19,
            brushRealizer =>
            {
                calls.Add($"Realize:{brushRealizer}");
                return 0;
            },
            (nint shape, out MilRectF bounds) =>
            {
                calls.Add($"Bounds:{shape}");
                bounds = shape == 23 ? new MilRectF(2, 3, 8, 9) : new MilRectF(1, 2, 9, 10);
                return 0;
            },
            (nint shape, nint pen, Matrix4x4? transform, Direct3D9SurfaceRect bounds, out nint widenedShape) =>
            {
                calls.Add($"Widen:{shape}:{pen}:{transform == worldToDevice}:{bounds}");
                widenedShape = 23;
                return 0;
            },
            (nint brushRealizer, bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{brushRealizer}:{convertNull}");
                brush = brushRealizer + 100;
                effects = brushRealizer + 200;
                return 0;
            },
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform == worldToDevice}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (shape, transform, bounds, brush, world, effects) =>
            {
                calls.Add($"Fill:{shape}:{transform == worldToDevice}:{bounds}:{brush}:{world == worldToDevice}:{effects}");
                return 0;
            },
            static (_, _, _, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (0, "Realize:19|Bounds:11|Clip:11:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }|Brush:19:False|Ensure|Fill:11:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }:119:True:219|Realize:17|Widen:11:13:True:Direct3D9SurfaceRect { Left = 0, Top = 0, Right = 100, Bottom = 80 }|Bounds:23|Clip:23:False:MilRectF { Left = 2, Top = 3, Right = 8, Bottom = 9 }|Brush:17:False|Ensure|Fill:23:False:MilRectF { Left = 2, Top = 3, Right = 8, Bottom = 9 }:117:True:217"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult, "Realize:19")]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0, "Realize:19|Realize:17|Widen")]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0, "Realize:19|Realize:17|Widen")]
    public unsafe void WhenPathFillFailsThenStrokePreparationFollowsNormalizedResult(int fillResult, int expectedResult, string expectedCalls)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            19,
            brushRealizer =>
            {
                calls.Add($"Realize:{brushRealizer}");
                return 0;
            },
            static (nint _, out MilRectF bounds) =>
            {
                bounds = new MilRectF(1, 2, 3, 4);
                return 0;
            },
            (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                calls.Add("Widen");
                widenedShape = 23;
                return 0;
            },
            static (nint brushRealizer, bool _, out nint brush, out nint effects) =>
            {
                brush = brushRealizer;
                effects = 0;
                return 0;
            },
            static (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            static () => 0,
            (_, _, _, _, _, _) => fillResult,
            static (_, _, _, _) => 0);

        Assert.AreEqual(
            (expectedResult, expectedCalls),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenStrokeBrushRealizationFailsThenResultIsNormalizedAndWidenIsSkipped(int realizationResult, int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int widenCalls = 0;

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            0,
            brushRealizer => brushRealizer == 17 ? realizationResult : 0,
            static (nint _, out MilRectF bounds) =>
            {
                bounds = default;
                return 0;
            },
            (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                widenCalls++;
                widenedShape = 23;
                return 0;
            },
            static (nint _, bool _, out nint brush, out nint effects) =>
            {
                brush = 0;
                effects = 0;
                return 0;
            },
            static (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            static () => 0,
            static (_, _, _, _, _, _) => 0,
            static (_, _, _, _) => 0);

        Assert.AreEqual((expectedResult, 0), (result, widenCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenFillBoundsFailThenResultIsNormalizedAndStrokeIsSkipped(int boundsResult, int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int realizationCalls = 0;
        int widenCalls = 0;

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            19,
            _ =>
            {
                realizationCalls++;
                return 0;
            },
            (nint _, out MilRectF bounds) =>
            {
                bounds = default;
                return boundsResult;
            },
            (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                widenCalls++;
                widenedShape = 23;
                return 0;
            },
            static (nint _, bool _, out nint brush, out nint effects) =>
            {
                brush = 0;
                effects = 0;
                return 0;
            },
            static (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            static () => 0,
            static (_, _, _, _, _, _) => 0,
            static (_, _, _, _) => 0);

        Assert.AreEqual((expectedResult, 1, 0), (result, realizationCalls, widenCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenStrokeWidenFailsThenResultIsNormalizedAndStrokeFillIsSkipped(int widenResult, int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int realizedBrushCalls = 0;

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            0,
            static _ => 0,
            static (nint _, out MilRectF bounds) =>
            {
                bounds = default;
                return 0;
            },
            (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                widenedShape = 0;
                return widenResult;
            },
            (nint _, bool _, out nint brush, out nint effects) =>
            {
                realizedBrushCalls++;
                brush = 0;
                effects = 0;
                return 0;
            },
            static (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            static () => 0,
            static (_, _, _, _, _, _) => 0,
            static (_, _, _, _) => 0);

        Assert.AreEqual((expectedResult, 0), (result, realizedBrushCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenWidenedShapeBoundsFailThenResultIsNormalizedAndStrokeFillIsSkipped(int boundsResult, int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int realizedBrushCalls = 0;

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            0,
            static _ => 0,
            (nint shape, out MilRectF bounds) =>
            {
                bounds = default;
                return shape == 23 ? boundsResult : 0;
            },
            static (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                widenedShape = 23;
                return 0;
            },
            (nint _, bool _, out nint brush, out nint effects) =>
            {
                realizedBrushCalls++;
                brush = 0;
                effects = 0;
                return 0;
            },
            static (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            static () => 0,
            static (_, _, _, _, _, _) => 0,
            static (_, _, _, _) => 0);

        Assert.AreEqual((expectedResult, 0), (result, realizedBrushCalls));
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenStrokeHardwareFillIsNotImplementedThenSoftwareFallbackUsesWidenedShapeAndNormalizesResult(
        int softwareResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            0,
            brushRealizer =>
            {
                calls.Add($"Realize:{brushRealizer}");
                return 0;
            },
            (nint shape, out MilRectF bounds) =>
            {
                calls.Add($"Bounds:{shape}");
                bounds = new MilRectF(1, 2, 9, 10);
                return 0;
            },
            (nint shape, nint pen, Matrix4x4? transform, Direct3D9SurfaceRect bounds, out nint widenedShape) =>
            {
                calls.Add($"Widen:{shape}:{pen}:{transform == Matrix4x4.Identity}:{bounds}");
                widenedShape = 23;
                return 0;
            },
            (nint brushRealizer, bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{brushRealizer}:{convertNull}");
                brush = 29;
                effects = 31;
                return 0;
            },
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform is null}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (shape, transform, bounds, brush, _, effects) =>
            {
                calls.Add($"Hardware:{shape}:{transform is null}:{bounds}:{brush}:{effects}");
                return Direct3D9Factory.NotImplementedHResult;
            },
            (shape, transform, brushRealizer, reason) =>
            {
                calls.Add($"Software:{shape}:{transform is null}:{brushRealizer}:{reason:X8}");
                return softwareResult;
            });

        Assert.AreEqual(
            (expectedResult, "Realize:17|Widen:11:13:True:Direct3D9SurfaceRect { Left = 0, Top = 0, Right = 0, Bottom = 0 }|Bounds:23|Clip:23:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }|Brush:17:False|Ensure|Hardware:23:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }:29:31|Software:23:True:17:80004001"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenStrokeSafeBoundsClippingFailsThenResultIsNormalizedAndLaterFillStagesAreSkipped(
        int clipResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            0,
            brushRealizer =>
            {
                calls.Add($"Realize:{brushRealizer}");
                return 0;
            },
            (nint shape, out MilRectF bounds) =>
            {
                calls.Add($"Bounds:{shape}");
                bounds = new MilRectF(1, 2, 9, 10);
                return 0;
            },
            (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                calls.Add("Widen");
                widenedShape = 23;
                return 0;
            },
            (nint _, bool _, out nint brush, out nint effects) =>
            {
                calls.Add("Brush");
                brush = 29;
                effects = 31;
                return 0;
            },
            (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add("Clip");
                clippedShape = default;
                return clipResult;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (_, _, _, _, _, _) =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (expectedResult, "Realize:17|Widen|Bounds:23|Clip"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenClippedStrokeBrushEffectsFailThenResultIsNormalizedAndStateAndFillAreSkipped(
        int brushResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            0,
            brushRealizer =>
            {
                calls.Add($"Realize:{brushRealizer}");
                return 0;
            },
            (nint shape, out MilRectF bounds) =>
            {
                calls.Add($"Bounds:{shape}");
                bounds = new MilRectF(1, 2, 9, 10);
                return 0;
            },
            (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                calls.Add("Widen");
                widenedShape = 23;
                return 0;
            },
            (nint brushRealizer, bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{brushRealizer}:{convertNull}");
                brush = 29;
                effects = 31;
                return brushResult;
            },
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform is null}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(37, Matrix4x4.Identity, new MilRectF(4, 5, 7, 8), true);
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (_, _, _, _, _, _) =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (expectedResult, "Realize:17|Widen|Bounds:23|Clip:23:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }|Brush:17:False"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    [DataRow(Direct3D9Factory.ClippedToEmptyHResult, 0)]
    public unsafe void WhenStrokeEnsureStateDoesNotPermitDrawingThenResultIsNormalizedAndFillIsSkipped(
        int ensureResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            0,
            brushRealizer =>
            {
                calls.Add($"Realize:{brushRealizer}");
                return 0;
            },
            (nint shape, out MilRectF bounds) =>
            {
                calls.Add($"Bounds:{shape}");
                bounds = new MilRectF(1, 2, 9, 10);
                return 0;
            },
            (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                calls.Add("Widen");
                widenedShape = 23;
                return 0;
            },
            (nint brushRealizer, bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{brushRealizer}:{convertNull}");
                brush = 29;
                effects = 31;
                return 0;
            },
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform is null}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(37, Matrix4x4.Identity, new MilRectF(4, 5, 7, 8), true);
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return ensureResult;
            },
            (_, _, _, _, _, _) =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (expectedResult, "Realize:17|Widen|Bounds:23|Clip:23:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }|Brush:17:False|Ensure"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenStrokeIsClippedThenHardwareFillUsesClippedShapeBoundsAndNullTransform()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            worldToDevice,
            11,
            13,
            17,
            0,
            brushRealizer =>
            {
                calls.Add($"Realize:{brushRealizer}");
                return 0;
            },
            (nint shape, out MilRectF bounds) =>
            {
                calls.Add($"Bounds:{shape}");
                bounds = new MilRectF(1, 2, 9, 10);
                return 0;
            },
            (nint shape, nint pen, Matrix4x4? transform, Direct3D9SurfaceRect bounds, out nint widenedShape) =>
            {
                calls.Add($"Widen:{shape}:{pen}:{transform == worldToDevice}:{bounds}");
                widenedShape = 23;
                return 0;
            },
            (nint brushRealizer, bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{brushRealizer}:{convertNull}");
                brush = 29;
                effects = 31;
                return 0;
            },
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform is null}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(37, Matrix4x4.Identity, new MilRectF(4, 5, 7, 8), true);
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (shape, transform, bounds, brush, world, effects) =>
            {
                calls.Add($"Hardware:{shape}:{transform is null}:{bounds}:{brush}:{world == worldToDevice}:{effects}");
                return 0;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (0, "Realize:17|Widen:11:13:True:Direct3D9SurfaceRect { Left = 0, Top = 0, Right = 0, Bottom = 0 }|Bounds:23|Clip:23:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }|Brush:17:False|Ensure|Hardware:37:True:MilRectF { Left = 4, Top = 5, Right = 7, Bottom = 8 }:29:True:31"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenClippedStrokeRealizesNullBrushThenStateAndFillStagesAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            0,
            brushRealizer =>
            {
                calls.Add($"Realize:{brushRealizer}");
                return 0;
            },
            (nint shape, out MilRectF bounds) =>
            {
                calls.Add($"Bounds:{shape}");
                bounds = new MilRectF(1, 2, 9, 10);
                return 0;
            },
            (nint shape, nint pen, Matrix4x4? transform, Direct3D9SurfaceRect bounds, out nint widenedShape) =>
            {
                calls.Add($"Widen:{shape}:{pen}:{transform is not null}:{bounds}");
                widenedShape = 23;
                return 0;
            },
            (nint brushRealizer, bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{brushRealizer}:{convertNull}");
                brush = 0;
                effects = 31;
                return 0;
            },
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform is null}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(37, Matrix4x4.Identity, new MilRectF(4, 5, 7, 8), true);
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (_, _, _, _, _, _) =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (0, "Realize:17|Widen:11:13:True:Direct3D9SurfaceRect { Left = 0, Top = 0, Right = 0, Bottom = 0 }|Bounds:23|Clip:23:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }|Brush:17:False"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenClippedStrokeHardwareFillDoesNotPermitDrawingThenResultIsNormalizedWithoutSoftwareFallback(
        int hardwareResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            worldToDevice,
            11,
            13,
            17,
            0,
            static _ => 0,
            static (nint _, out MilRectF bounds) =>
            {
                bounds = new MilRectF(1, 2, 9, 10);
                return 0;
            },
            static (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                widenedShape = 23;
                return 0;
            },
            static (nint _, bool _, out nint brush, out nint effects) =>
            {
                brush = 29;
                effects = 31;
                return 0;
            },
            static (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(37, Matrix4x4.Identity, new MilRectF(4, 5, 7, 8), true);
                return 0;
            },
            static () => 0,
            (shape, transform, bounds, brush, world, effects) =>
            {
                calls.Add($"Hardware:{shape}:{transform is null}:{bounds}:{brush}:{world == worldToDevice}:{effects}");
                return hardwareResult;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (expectedResult, "Hardware:37:True:MilRectF { Left = 4, Top = 5, Right = 7, Bottom = 8 }:29:True:31"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenClippedStrokeHardwareFillIsNotImplementedThenSoftwareFallbackUsesClippedShapeAndNullTransform(
        int softwareResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawPath(
            Matrix4x4.Identity,
            11,
            13,
            17,
            0,
            static _ => 0,
            static (nint _, out MilRectF bounds) =>
            {
                bounds = new MilRectF(1, 2, 9, 10);
                return 0;
            },
            static (nint _, nint _, Matrix4x4? _, Direct3D9SurfaceRect _, out nint widenedShape) =>
            {
                widenedShape = 23;
                return 0;
            },
            static (nint _, bool _, out nint brush, out nint effects) =>
            {
                brush = 29;
                effects = 31;
                return 0;
            },
            static (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(37, Matrix4x4.Identity, new MilRectF(4, 5, 7, 8), true);
                return 0;
            },
            static () => 0,
            (shape, transform, bounds, brush, _, effects) =>
            {
                calls.Add($"Hardware:{shape}:{transform is null}:{bounds}:{brush}:{effects}");
                return Direct3D9Factory.NotImplementedHResult;
            },
            (shape, transform, brushRealizer, reason) =>
            {
                calls.Add($"Software:{shape}:{transform is null}:{brushRealizer}:{reason:X8}");
                return softwareResult;
            });

        Assert.AreEqual(
            (expectedResult, "Hardware:37:True:MilRectF { Left = 4, Top = 5, Right = 7, Bottom = 8 }:29:31|Software:37:True:17:80004001"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenSafeBoundsClippingIsNotImplementedThenSoftwareFallbackUsesOriginalPathState()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Matrix4x4 shapeToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        List<string> calls = [];

        int result = renderTarget.FillPath(
            11,
            shapeToDevice,
            new MilRectF(1, 2, 9, 10),
            17,
            Matrix4x4.Identity,
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform == shapeToDevice}:{bounds}");
                clippedShape = default;
                return Direct3D9Factory.NotImplementedHResult;
            },
            (bool _, out nint brush, out nint effects) =>
            {
                calls.Add("Brush");
                brush = 0;
                effects = 0;
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (_, _, _, _, _, _) =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (shape, transform, brushRealizer, reason) =>
            {
                calls.Add($"Software:{shape}:{transform == shapeToDevice}:{brushRealizer}:{reason:X8}");
                return 0;
            });

        Assert.AreEqual(
            (0, "Clip:11:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }|Software:11:True:17:80004001"),
            (result, string.Join('|', calls)));
    }

    private static Direct3D9DerivedMeshShader CreateDerivedMeshShader(List<string> calls)
    {
        return new Direct3D9DerivedMeshShader(
            () => AddDrawMeshCall(calls, "Begin"),
            () => false,
            () => AddDrawMeshCall(calls, "Shader"),
            () => AddDrawMeshCall(calls, "FixedFunction"),
            () => AddDrawMeshCall(calls, "Finish"),
            () => calls.Add("ReleaseShader"));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenDrawInfinitePathSucceedsWithoutDrawing()
    {
        AssertDisplayDrawingIsSkippedWhenRenderingIsDisabled(static (renderTarget, draw) => renderTarget.DrawInfinitePath(draw));
    }

    [TestMethod]
    [DataRow(0, true)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, false)]
    public unsafe void WhenDisplayInfinitePathDrawingDelegatesThenBaseResultIsReturnedAfterExactlyOneScopedCall(
        int drawResult,
        bool expectedValidContents)
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        int drawCalls = 0;
        bool enteredDuringDraw = false;
        bool inUseContextDuringDraw = false;

        int result = renderTarget.DrawInfinitePath(() =>
        {
            drawCalls++;
            enteredDuringDraw = device.IsEntered();
            inUseContextDuringDraw = device.IsInUseContext();
            return drawResult;
        });

        Assert.AreEqual(
            (drawResult, 1, true, true, expectedValidContents, false, false),
            (result, drawCalls, enteredDuringDraw, inUseContextDuringDraw, renderTarget.HasValidContents,
                device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDisplayInfinitePathDrawingIsDisabledThenNullOperationIsIgnoredBeforeEnteringDeviceScopes()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));

        int result = renderTarget.DrawInfinitePath(null!);

        Assert.AreEqual(
            (0, false, false, false),
            (result, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenInfinitePathRenderTargetIsInvalidThenShapeCreationIsSkipped()
    {
        Direct3D9SwapChain? swapChain = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        swapChain!.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.DrawInfinitePath(
            Matrix4x4.Identity,
            19,
            (MilRectF _, out nint shape) =>
            {
                callbackCalls++;
                shape = 0;
                return 0;
            },
            _ => ++callbackCalls,
            (nint _, out MilRectF bounds) =>
            {
                callbackCalls++;
                bounds = default;
                return 0;
            },
            (nint _, bool _, out nint brush, out nint effects) =>
            {
                callbackCalls++;
                brush = 0;
                effects = 0;
                return 0;
            },
            (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                callbackCalls++;
                clippedShape = default;
                return 0;
            },
            () => ++callbackCalls,
            (_, _, _, _, _, _) => ++callbackCalls,
            (_, _, _, _) => ++callbackCalls);

        Assert.AreEqual(
            (0, 0, true, false, false),
            (result, callbackCalls, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingInfinitePathToInvalidDirectSurfaceThenDependenciesAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.DrawInfinitePath(
            Matrix4x4.Identity,
            19,
            (MilRectF _, out nint shape) =>
            {
                callbackCalls++;
                shape = 0;
                return 0;
            },
            _ => ++callbackCalls,
            (nint _, out MilRectF bounds) =>
            {
                callbackCalls++;
                bounds = default;
                return 0;
            },
            (nint _, bool _, out nint brush, out nint effects) =>
            {
                callbackCalls++;
                brush = 0;
                effects = 0;
                return 0;
            },
            (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                callbackCalls++;
                clippedShape = default;
                return 0;
            },
            () => ++callbackCalls,
            (_, _, _, _, _, _) => ++callbackCalls,
            (_, _, _, _) => ++callbackCalls);

        Assert.AreEqual(
            (0, 0, true, false, false),
            (result, callbackCalls, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingInfinitePathThenRenderTargetBoundsAreFilledInDeviceSpace()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initialBounds: new Direct3D9SurfaceRect(2, 3, 102, 83));
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(5, 7, 0);
        List<string> calls = [];

        int result = renderTarget.DrawInfinitePath(
            worldToDevice,
            19,
            (MilRectF bounds, out nint shape) =>
            {
                calls.Add($"Shape:{bounds}");
                shape = 23;
                return 0;
            },
            brushRealizer =>
            {
                calls.Add($"Realize:{brushRealizer}");
                return 0;
            },
            (nint shape, out MilRectF bounds) =>
            {
                calls.Add($"Bounds:{shape}");
                bounds = new MilRectF(2, 3, 102, 83);
                return 0;
            },
            (nint brushRealizer, bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{brushRealizer}:{convertNull}");
                brush = 29;
                effects = 31;
                return 0;
            },
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform is null}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (shape, transform, bounds, brush, world, effects) =>
            {
                calls.Add($"Fill:{shape}:{transform is null}:{bounds}:{brush}:{world == worldToDevice}:{effects}");
                return 0;
            },
            static (_, _, _, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (0, "Shape:MilRectF { Left = 2, Top = 3, Right = 102, Bottom = 83 }|Realize:19|Bounds:23|Clip:23:True:MilRectF { Left = 2, Top = 3, Right = 102, Bottom = 83 }|Brush:19:False|Ensure|Fill:23:True:MilRectF { Left = 2, Top = 3, Right = 102, Bottom = 83 }:29:True:31"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenInfinitePathShapeCreationFailsThenBrushRealizationIsSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int realizationCalls = 0;

        int result = renderTarget.DrawInfinitePath(
            Matrix4x4.Identity,
            19,
            static (MilRectF _, out nint shape) =>
            {
                shape = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            _ => ++realizationCalls,
            static (nint _, out MilRectF bounds) =>
            {
                bounds = default;
                return 0;
            },
            static (nint _, bool _, out nint brush, out nint effects) =>
            {
                brush = 0;
                effects = 0;
                return 0;
            },
            static (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = default;
                return 0;
            },
            static () => 0,
            static (_, _, _, _, _, _) => 0,
            static (_, _, _, _) => 0);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, realizationCalls));
    }

    [TestMethod]
    public unsafe void WhenIntermediateDimensionsExceedFloatIntegerRangeThenCreationFailsBeforeFactories()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int factoryCalls = 0;

        int result = renderTarget.CreateRenderTargetBitmap(
            (1u << 24) + 1,
            1,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.None, MilBitmapWrapMode.Extend),
            Direct3D9RenderTargetInitializationFlags.None,
            hasValidRealizationCacheIndex: true,
            (bool _, out nint bitmap) =>
            {
                factoryCalls++;
                bitmap = 1;
                return 0;
            },
            (bool _, out nint bitmap) =>
            {
                factoryCalls++;
                bitmap = 2;
                return 0;
            },
            out nint renderTargetBitmap);

        Assert.AreEqual((Direct3D9Factory.UnsupportedTextureSizeHResult, 0, 0), (result, renderTargetBitmap, factoryCalls));
    }

    [TestMethod]
    public unsafe void WhenTwoDimensionalIntermediateIsTiledThenSoftwareFactoryIsUsed()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.CreateRenderTargetBitmap(
            32,
            64,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.ForBlending, MilBitmapWrapMode.Tile),
            Direct3D9RenderTargetInitializationFlags.None,
            hasValidRealizationCacheIndex: true,
            (bool _, out nint bitmap) =>
            {
                calls.Add("Hardware");
                bitmap = 1;
                return 0;
            },
            (bool forBlending, out nint bitmap) =>
            {
                calls.Add($"Software:{forBlending}:{device.IsEntered()}");
                bitmap = 2;
                return 0;
            },
            out nint renderTargetBitmap);

        Assert.AreEqual((0, 2, "Software:True:True", false), (result, renderTargetBitmap, string.Join('|', calls), device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenThreeDimensionalIntermediateIsTiledThenHardwareFactoryIsUsed()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int result = renderTarget.CreateRenderTargetBitmap(
            32,
            64,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.ForUseIn3D, MilBitmapWrapMode.Tile),
            Direct3D9RenderTargetInitializationFlags.None,
            hasValidRealizationCacheIndex: true,
            (bool forBlending, out nint bitmap) =>
            {
                bitmap = forBlending ? 1 : 3;
                return 0;
            },
            static (bool _, out nint bitmap) =>
            {
                bitmap = 2;
                return Direct3D9Factory.GenericFailureHResult;
            },
            out nint renderTargetBitmap);

        Assert.AreEqual((0, 3, true), (result, renderTargetBitmap, renderTarget.WasUsedToCreateHardwareRenderTarget));
    }

    [TestMethod]
    public unsafe void WhenCreatingIntermediateFromInvalidDirectSurfaceThenHardwareFactoryStillRunsInsideDeviceScope()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();
        int softwareCalls = 0;

        int result = renderTarget.CreateRenderTargetBitmap(
            32,
            64,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.None, MilBitmapWrapMode.Extend),
            Direct3D9RenderTargetInitializationFlags.None,
            hasValidRealizationCacheIndex: true,
            (bool forBlending, out nint bitmap) =>
            {
                Assert.AreEqual((false, true), (forBlending, device.IsEntered()));
                bitmap = 3;
                return 0;
            },
            (bool _, out nint bitmap) =>
            {
                softwareCalls++;
                bitmap = 2;
                return 0;
            },
            out nint renderTargetBitmap);

        Assert.AreEqual(
            (0, 3, 0, true, false),
            (result, renderTargetBitmap, softwareCalls, renderTarget.WasUsedToCreateHardwareRenderTarget, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenForceCompatibleCannotUseHardwareThenCreationFailsWithoutSoftwareFactory()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int softwareCalls = 0;

        int result = renderTarget.CreateRenderTargetBitmap(
            32,
            64,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.None, MilBitmapWrapMode.Extend),
            Direct3D9RenderTargetInitializationFlags.ForceCompatible,
            hasValidRealizationCacheIndex: false,
            static (bool _, out nint bitmap) =>
            {
                bitmap = 1;
                return 0;
            },
            (bool _, out nint bitmap) =>
            {
                softwareCalls++;
                bitmap = 2;
                return 0;
            },
            out nint renderTargetBitmap);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 0), (result, renderTargetBitmap, softwareCalls));
    }

    [TestMethod]
    public unsafe void WhenHardwareIntermediateFactoryFailsThenFailureIsPreservedWithoutSoftwareFallbackOrUsedState()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int softwareCalls = 0;

        int result = renderTarget.CreateRenderTargetBitmap(
            32,
            64,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.None, MilBitmapWrapMode.Extend),
            Direct3D9RenderTargetInitializationFlags.None,
            hasValidRealizationCacheIndex: true,
            static (bool _, out nint bitmap) =>
            {
                bitmap = 0;
                return Direct3D9Factory.OutOfVideoMemoryHResult;
            },
            (bool _, out nint bitmap) =>
            {
                softwareCalls++;
                bitmap = 2;
                return 0;
            },
            out nint renderTargetBitmap);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, 0, 0, false),
            (result, renderTargetBitmap, softwareCalls, renderTarget.WasUsedToCreateHardwareRenderTarget));
    }

    [TestMethod]
    public unsafe void WhenHardwareIntermediateIsCreatedThenNativeParametersAndCallerOwnershipArePreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            associatedDisplayIndex: 7);

        int result = renderTarget.CreateRenderTargetBitmap(
            17,
            31,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.ForBlending, MilBitmapWrapMode.Extend),
            Direct3D9RenderTargetInitializationFlags.None,
            hasValidRealizationCacheIndex: true,
            out Direct3D9TextureRenderTarget? intermediate,
            (uint width, uint height, Direct3D9Device candidateDevice, uint? displayIndex, bool forBlending, out Direct3D9TextureRenderTarget? candidate) =>
            {
                Assert.AreEqual((17u, 31u, device, (uint?) 7, true, true),
                    (width, height, candidateDevice, displayIndex, forBlending, device.IsEntered()));
                candidate = new Direct3D9TextureRenderTarget(
                    device,
                    new Direct3D9SurfaceRenderTarget(device, MultisampleType.MultisampleNone, width: width, height: height, associatedDisplayIndex: displayIndex),
                    texture: null);
                return 0;
            });

        Assert.AreEqual((0, true, (17u, 31u), (uint?) 7),
            (result, renderTarget.WasUsedToCreateHardwareRenderTarget, intermediate!.GetSize(), intermediate.GetDisplayId()));
        intermediate.Dispose();
    }

    [TestMethod]
    public unsafe void WhenHardwareIntermediateCreationFailsThenCandidateIsReleasedAndOutputRemainsEmpty()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9TextureRenderTarget? failedCandidate = null;

        int result = renderTarget.CreateRenderTargetBitmap(
            17,
            31,
            new Direct3D9IntermediateRenderTargetUsage(Direct3D9IntermediateRenderTargetUsageFlags.None, MilBitmapWrapMode.Extend),
            Direct3D9RenderTargetInitializationFlags.None,
            hasValidRealizationCacheIndex: true,
            out Direct3D9TextureRenderTarget? intermediate,
            (uint width, uint height, Direct3D9Device candidateDevice, uint? displayIndex, bool forBlending, out Direct3D9TextureRenderTarget? candidate) =>
            {
                failedCandidate = new Direct3D9TextureRenderTarget(
                    candidateDevice,
                    new Direct3D9SurfaceRenderTarget(candidateDevice, MultisampleType.MultisampleNone, width: width, height: height, associatedDisplayIndex: displayIndex),
                    texture: null);
                candidate = failedCandidate;
                return Direct3D9Factory.OutOfVideoMemoryHResult;
            });

        Assert.ThrowsExactly<ObjectDisposedException>(() => failedCandidate!.GetSize());
        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, (Direct3D9TextureRenderTarget?) null, false),
            (result, intermediate, renderTarget.WasUsedToCreateHardwareRenderTarget));
    }

    [TestMethod]
    public unsafe void WhenEffectIsComposedThenNativeOuterSequenceAndScopesArePreserved()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        List<string> calls = [];
        Direct3D9EffectComposeState composeState = new(11, 12, 32, 64, 13);

        int result = renderTarget.ComposeEffect(
            composeState,
            () =>
            {
                calls.Add($"State:{device.IsEntered()}:{device.IsInUseContext()}");
                return 0;
            },
            mode =>
            {
                calls.Add($"Blend:{mode}");
                return 0;
            },
            (nint input, out nint textureRenderTarget) =>
            {
                calls.Add($"Resolve:{input}");
                textureRenderTarget = 14;
                return 0;
            },
            input =>
            {
                calls.Add($"Valid:{input}");
                return true;
            },
            (effect, scaleTransform, width, height, input) =>
            {
                calls.Add($"Apply:{effect}:{scaleTransform}:{width}:{height}:{input}");
                return 0;
            });

        Assert.AreEqual(
            (0, "State:True:True|Blend:SourceOver|Resolve:13|Valid:14|Apply:12:11:32:64:14", false, false),
            (result, string.Join('|', calls), device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenComposingEffectToInvalidDirectSurfaceThenDependenciesAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.ComposeEffect(
            new Direct3D9EffectComposeState(1, 2, 3, 4, 5),
            () => ++callbackCalls,
            _ => ++callbackCalls,
            (nint _, out nint input) =>
            {
                callbackCalls++;
                input = 6;
                return 0;
            },
            _ =>
            {
                callbackCalls++;
                return true;
            },
            (_, _, _, _, _) => ++callbackCalls);

        Assert.AreEqual((0, 0, false, false), (result, callbackCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenEffectStateIsClippedToEmptyThenCompositionStopsSuccessfully()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        int laterCalls = 0;

        int result = renderTarget.ComposeEffect(
            new Direct3D9EffectComposeState(1, 2, 3, 4, 5),
            static () => Direct3D9Factory.ClippedToEmptyHResult,
            _ =>
            {
                laterCalls++;
                return 0;
            },
            (nint _, out nint input) =>
            {
                laterCalls++;
                input = 6;
                return 0;
            },
            _ => true,
            (_, _, _, _, _) =>
            {
                laterCalls++;
                return 0;
            });

        Assert.AreEqual((0, 0), (result, laterCalls));
    }

    [TestMethod]
    public unsafe void WhenImplicitEffectInputIsInvalidThenEffectIsNotApplied()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        int applyCalls = 0;

        int result = renderTarget.ComposeEffect(
            new Direct3D9EffectComposeState(1, 2, 3, 4, 5),
            static () => 0,
            static _ => 0,
            static (nint _, out nint input) =>
            {
                input = 6;
                return 0;
            },
            static _ => false,
            (_, _, _, _, _) =>
            {
                applyCalls++;
                return 0;
            });

        Assert.AreEqual((0, 0), (result, applyCalls));
    }

    [TestMethod]
    public unsafe void WhenEffectStateSetupFailsThenFirstFailureStopsLaterCalls()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        int laterCalls = 0;

        int result = renderTarget.ComposeEffect(
            new Direct3D9EffectComposeState(1, 2, 3, 4, 5),
            static () => Direct3D9Factory.OutOfVideoMemoryHResult,
            _ =>
            {
                laterCalls++;
                return 0;
            },
            (nint _, out nint input) =>
            {
                laterCalls++;
                input = 6;
                return 0;
            },
            _ => true,
            (_, _, _, _, _) =>
            {
                laterCalls++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.OutOfVideoMemoryHResult, 0), (result, laterCalls));
    }

    [TestMethod]
    public unsafe void WhenAlphaLayerBeginsThenEntireTargetIsCapturedBeforeTransparentClear()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        List<string> calls = [];
        Direct3D9SurfaceRect bounds = new(1, 2, 9, 10);

        int result = renderTarget.BeginLayerInternal(
            new Direct3D9LayerBeginState(bounds, HasAlphaMaskBrush: false),
            (out IReadOnlyList<Direct3D9SurfaceRect> rects) =>
            {
                calls.Add("Partial");
                rects = [];
                return true;
            },
            (Direct3D9SurfaceRect layerBounds, IReadOnlyList<Direct3D9SurfaceRect>? copyRects, out nint bitmap) =>
            {
                calls.Add($"Capture:{layerBounds}:{copyRects is null}:{device.IsEntered()}:{device.IsInUseContext()}");
                bitmap = 23;
                return 0;
            },
            layerBounds =>
            {
                calls.Add($"Clear:{layerBounds}");
                return 0;
            },
            out nint sourceBitmap);

        Assert.AreEqual(
            (0, (nint) 23, $"Capture:{bounds}:True:True:True|Clear:{bounds}", false, false),
            (result, sourceBitmap, string.Join('|', calls), device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenOpaqueLayerHasPartialCaptureRectsThenOnlyThoseRectsAreCaptured()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Bgr32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        Direct3D9SurfaceRect copyRect = new(2, 3, 4, 5);
        int clearCalls = 0;

        int result = renderTarget.BeginLayerInternal(
            new Direct3D9LayerBeginState(new Direct3D9SurfaceRect(0, 0, 8, 8), HasAlphaMaskBrush: false),
            (out IReadOnlyList<Direct3D9SurfaceRect> rects) =>
            {
                rects = [copyRect];
                return true;
            },
            (Direct3D9SurfaceRect _, IReadOnlyList<Direct3D9SurfaceRect>? rects, out nint bitmap) =>
            {
                bitmap = rects is { Count: 1 } && rects[0] == copyRect ? 29 : 0;
                return 0;
            },
            _ => ++clearCalls,
            out nint sourceBitmap);

        Assert.AreEqual((0, (nint) 29, 0), (result, sourceBitmap, clearCalls));
    }

    [TestMethod]
    public unsafe void WhenOpaqueLayerPartialCaptureIsEmptyThenCaptureAndEndFixupAreSkippedWhileStateIsRestored()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRect layerBounds = new(1, 2, 9, 10);
        Direct3D9SurfaceRect previousBounds = new(0, 0, 16, 16);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Bgr32Bpp,
            initialBounds: layerBounds,
            forceClearType: true);
        int captureCalls = 0;
        int clearCalls = 0;

        int beginResult = renderTarget.BeginLayerInternal(
            new Direct3D9LayerBeginState(layerBounds, HasAlphaMaskBrush: false),
            static (out IReadOnlyList<Direct3D9SurfaceRect> rects) =>
            {
                rects = [];
                return true;
            },
            (Direct3D9SurfaceRect _, IReadOnlyList<Direct3D9SurfaceRect>? _, out nint bitmap) =>
            {
                captureCalls++;
                bitmap = 71;
                return 0;
            },
            _ => ++clearCalls,
            out nint sourceBitmap);
        int internalEndCalls = 0;
        int releaseCalls = 0;

        int endResult = renderTarget.EndLayer(
            new Direct3D9LayerEndState(
                layerBounds,
                layerBounds,
                sourceBitmap,
                previousBounds,
                SavedClearTypeHint: false),
            () => ++internalEndCalls,
            _ => releaseCalls++);

        Assert.AreEqual(
            (0, (nint) 0, 0, 0, 0, 0, previousBounds, false, false),
            (beginResult, sourceBitmap, captureCalls, clearCalls, endResult, internalEndCalls + releaseCalls, renderTarget.Bounds, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenOpaqueLayerPartialCaptureIsUnavailableThenEntireLayerIsCaptured()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Bgr32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        Direct3D9SurfaceRect layerBounds = new(1, 2, 9, 10);
        int clearCalls = 0;
        IReadOnlyList<Direct3D9SurfaceRect>? capturedRects = [];

        int result = renderTarget.BeginLayerInternal(
            new Direct3D9LayerBeginState(layerBounds, HasAlphaMaskBrush: false),
            static (out IReadOnlyList<Direct3D9SurfaceRect> rects) =>
            {
                rects = [];
                return false;
            },
            (Direct3D9SurfaceRect capturedBounds, IReadOnlyList<Direct3D9SurfaceRect>? rects, out nint bitmap) =>
            {
                capturedRects = rects;
                bitmap = capturedBounds == layerBounds ? 73 : 0;
                return 0;
            },
            _ => ++clearCalls,
            out nint sourceBitmap);

        Assert.AreEqual((0, (nint) 73, null, 0), (result, sourceBitmap, capturedRects, clearCalls));
    }

    [TestMethod]
    public unsafe void WhenDisposedRepeatedlyThenBeginLayerRejectsUseBeforeInspectingCallbacks()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.BeginLayerInternal(
            default,
            null!,
            null!,
            null!,
            out _));
    }

    [TestMethod]
    public unsafe void WhenDisposedRepeatedlyThenEndLayerRejectsUseBeforeInspectingCallbacks()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.EndLayer(default, null!, null!));
    }

    [TestMethod]
    public unsafe void WhenLayerHasAlphaMaskThenNotImplementedIsReturnedBeforeCapture()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        int callbackCalls = 0;

        int result = renderTarget.BeginLayerInternal(
            new Direct3D9LayerBeginState(new Direct3D9SurfaceRect(0, 0, 8, 8), HasAlphaMaskBrush: true),
            (out IReadOnlyList<Direct3D9SurfaceRect> rects) =>
            {
                callbackCalls++;
                rects = [];
                return false;
            },
            (Direct3D9SurfaceRect _, IReadOnlyList<Direct3D9SurfaceRect>? _, out nint bitmap) =>
            {
                callbackCalls++;
                bitmap = 0;
                return 0;
            },
            _ => ++callbackCalls,
            out nint sourceBitmap);

        Assert.AreEqual((Direct3D9Factory.NotImplementedHResult, (nint) 0, 0), (result, sourceBitmap, callbackCalls));
    }

    [TestMethod]
    public unsafe void WhenLayerBeginsOnInvalidDirectSurfaceThenUnsupportedChecksAndCaptureAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 12,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 12));
        surface.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.BeginLayerInternal(
            new Direct3D9LayerBeginState(
                new Direct3D9SurfaceRect(1, 2, 9, 10),
                HasAlphaMaskBrush: true),
            (out IReadOnlyList<Direct3D9SurfaceRect> rects) =>
            {
                callbackCalls++;
                rects = [];
                return true;
            },
            (Direct3D9SurfaceRect _, IReadOnlyList<Direct3D9SurfaceRect>? _, out nint bitmap) =>
            {
                callbackCalls++;
                bitmap = 0;
                return 0;
            },
            _ => ++callbackCalls,
            out nint sourceBitmap);

        Assert.AreEqual(
            (0, (nint) 0, 0, false, false, false),
            (result, sourceBitmap, callbackCalls, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenAlphaLayerClearFailsThenCapturedBitmapAndFirstFailureArePreserved()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Prgba64Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));

        int result = renderTarget.BeginLayerInternal(
            new Direct3D9LayerBeginState(new Direct3D9SurfaceRect(0, 0, 8, 8), HasAlphaMaskBrush: false),
            static (out IReadOnlyList<Direct3D9SurfaceRect> rects) =>
            {
                rects = [];
                return true;
            },
            static (Direct3D9SurfaceRect _, IReadOnlyList<Direct3D9SurfaceRect>? _, out nint bitmap) =>
            {
                bitmap = 31;
                return 0;
            },
            static _ => Direct3D9Factory.GenericFailureHResult,
            out nint sourceBitmap);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, (nint) 31), (result, sourceBitmap));
    }

    [TestMethod]
    public unsafe void WhenAlphaLayerClearFailsThenUpperLayerCleanupReleasesCapturedBitmapExactlyOnce()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        Direct3D9SurfaceRect layerBounds = new(0, 0, 8, 8);

        int beginResult = renderTarget.BeginLayerInternal(
            new Direct3D9LayerBeginState(layerBounds, HasAlphaMaskBrush: false),
            static (out IReadOnlyList<Direct3D9SurfaceRect> rects) =>
            {
                rects = [];
                return true;
            },
            static (Direct3D9SurfaceRect _, IReadOnlyList<Direct3D9SurfaceRect>? _, out nint bitmap) =>
            {
                bitmap = 37;
                return 0;
            },
            static _ => Direct3D9Factory.OutOfVideoMemoryHResult,
            out nint sourceBitmap);
        int cleanupCalls = 0;
        nint releasedBitmap = 0;

        int cleanupResult = renderTarget.EndLayer(
            new Direct3D9LayerEndState(layerBounds, layerBounds, sourceBitmap),
            () => beginResult,
            bitmap =>
            {
                cleanupCalls++;
                releasedBitmap = bitmap;
            });

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, Direct3D9Factory.OutOfVideoMemoryHResult, (nint) 37, 1, (nint) 37, false, false),
            (beginResult, cleanupResult, sourceBitmap, cleanupCalls, releasedBitmap, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenAlphaLayerCaptureFailsThenUpperLayerCleanupHasNoBitmapToRelease()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        Direct3D9SurfaceRect layerBounds = new(0, 0, 8, 8);
        int clearCalls = 0;

        int beginResult = renderTarget.BeginLayerInternal(
            new Direct3D9LayerBeginState(layerBounds, HasAlphaMaskBrush: false),
            static (out IReadOnlyList<Direct3D9SurfaceRect> rects) =>
            {
                rects = [];
                return true;
            },
            static (Direct3D9SurfaceRect _, IReadOnlyList<Direct3D9SurfaceRect>? _, out nint bitmap) =>
            {
                bitmap = 43;
                return Direct3D9Factory.OutOfVideoMemoryHResult;
            },
            _ => ++clearCalls,
            out nint sourceBitmap);
        int cleanupCalls = 0;

        int cleanupResult = renderTarget.EndLayer(
            new Direct3D9LayerEndState(layerBounds, layerBounds, sourceBitmap),
            static () => Direct3D9Factory.GenericFailureHResult,
            _ => cleanupCalls++);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, 0, (nint) 0, 0, 0, false, false),
            (beginResult, cleanupResult, sourceBitmap, clearCalls, cleanupCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenAlphaLayerEndsThenSavedTargetIsCompositedUnderAfterStateSetup()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        List<string> calls = [];
        Direct3D9SurfaceRect bounds = new(1, 2, 9, 10);
        Direct3D9SurfaceRect currentClip = new(3, 4, 12, 13);

        int result = renderTarget.EndLayerInternal(
            new Direct3D9LayerEndState(bounds, currentClip, 41),
            () =>
            {
                calls.Add($"Target:{device.IsEntered()}:{device.IsInUseContext()}");
                return 0;
            },
            clip =>
            {
                calls.Add($"Clip:{clip}");
                return 0;
            },
            () =>
            {
                calls.Add("2D");
                return 0;
            },
            (bitmap, layerBounds, compositingMode) =>
            {
                calls.Add($"Composite:{bitmap}:{layerBounds}:{compositingMode}");
                return 0;
            });

        Assert.AreEqual(
            (0, $"Target:True:True|Clip:{currentClip}|2D|Composite:41:{bounds}:{MilCompositingMode.SourceUnder}", false, false),
            (result, string.Join('|', calls), device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenOpaqueLayerEndsThenSavedTargetIsNotCompositedAfterStateSetup()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Bgr32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        int compositeCalls = 0;

        int result = renderTarget.EndLayerInternal(
            new Direct3D9LayerEndState(
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                43),
            static () => 0,
            static _ => 0,
            static () => 0,
            (nint _, Direct3D9SurfaceRect _, MilCompositingMode _) => ++compositeCalls);

        Assert.AreEqual((0, 0), (result, compositeCalls));
    }

    [TestMethod]
    public unsafe void WhenEndLayerStateSetupFailsThenFirstFailureStopsSavedTargetComposite()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Prgba64Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        int ensure2DCalls = 0;
        int compositeCalls = 0;

        int result = renderTarget.EndLayerInternal(
            new Direct3D9LayerEndState(
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                47),
            static () => 0,
            static _ => Direct3D9Factory.GenericFailureHResult,
            () => ++ensure2DCalls,
            (nint _, Direct3D9SurfaceRect _, MilCompositingMode _) => ++compositeCalls);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 0, false, false),
            (result, ensure2DCalls, compositeCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenEndLayer2DStateSetupFailsThenCompositeIsSkipped()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        int compositeCalls = 0;

        int result = renderTarget.EndLayerInternal(
            new Direct3D9LayerEndState(
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                new Direct3D9SurfaceRect(1, 1, 7, 7),
                49),
            static () => 0,
            static _ => 0,
            static () => Direct3D9Factory.OutOfVideoMemoryHResult,
            (nint _, Direct3D9SurfaceRect _, MilCompositingMode _) => ++compositeCalls);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, 0, false, false),
            (result, compositeCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenAlphaSavedTargetCompositeFailsThenOuterEndRestoresStateAndReleasesBitmap()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        Direct3D9SurfaceRect layerBounds = new(1, 2, 9, 10);
        Direct3D9SurfaceRect previousBounds = new(0, 0, 16, 16);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp,
            initialBounds: layerBounds);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        nint compositedBitmap = 0;
        Direct3D9SurfaceRect compositedBounds = default;
        MilCompositingMode compositingMode = MilCompositingMode.SourceOver;
        nint releasedBitmap = 0;
        Direct3D9LayerEndState layerState = new(
            layerBounds,
            layerBounds,
            51,
            previousBounds,
            SavedClearTypeHint: true);

        int result = renderTarget.EndLayer(
            layerState,
            () => renderTarget.EndLayerInternal(
                layerState,
                static () => 0,
                static _ => 0,
                static () => 0,
                (bitmap, bounds, mode) =>
                {
                    compositedBitmap = bitmap;
                    compositedBounds = bounds;
                    compositingMode = mode;
                    return Direct3D9Factory.OutOfVideoMemoryHResult;
                }),
            bitmap => releasedBitmap = bitmap);
        bool? supportsClearType = null;
        int glyphResult = renderTarget.DrawGlyphs(
            canDrawText: true,
            static () => 0,
            static () => 0,
            value =>
            {
                supportsClearType = value;
                return 0;
            },
            static (_, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, (nint) 51, layerBounds, MilCompositingMode.SourceUnder, (nint) 51, previousBounds, 0, true, false, false),
            (result, compositedBitmap, compositedBounds, compositingMode, releasedBitmap, renderTarget.Bounds, glyphResult, supportsClearType, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenEndLayerSucceedsWithoutStateSetupOrComposite()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));
        int callbackCalls = 0;

        int result = renderTarget.EndLayerInternal(
            new Direct3D9LayerEndState(
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                53),
            () => ++callbackCalls,
            _ => ++callbackCalls,
            () => ++callbackCalls,
            (nint _, Direct3D9SurfaceRect _, MilCompositingMode _) => ++callbackCalls);

        Assert.AreEqual((0, 0, false, false), (result, callbackCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenEndingLayerThenNoRenderFailureIsNormalizedAndStateIsRestored()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRect layerBounds = new(1, 2, 9, 10);
        Direct3D9SurfaceRect previousBounds = new(0, 0, 16, 16);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp,
            initialBounds: layerBounds);
        int internalCalls = 0;
        nint releasedBitmap = 0;

        int result = renderTarget.EndLayer(
            new Direct3D9LayerEndState(
                layerBounds,
                layerBounds,
                61,
                previousBounds,
                SavedClearTypeHint: true),
            () =>
            {
                internalCalls++;
                return Direct3D9Factory.BadNumberHResult;
            },
            bitmap => releasedBitmap = bitmap);
        bool? supportsClearType = null;
        int glyphResult = renderTarget.DrawGlyphs(
            canDrawText: true,
            static () => 0,
            static () => 0,
            value =>
            {
                supportsClearType = value;
                return 0;
            },
            static (_, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (0, 1, (nint) 61, previousBounds, 0, true),
            (result, internalCalls, releasedBitmap, renderTarget.Bounds, glyphResult, supportsClearType));
    }

    [TestMethod]
    public unsafe void WhenLayerHasNoSavedTargetThenInternalEndAndReleaseAreSkippedWhileStateIsRestored()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRect layerBounds = new(1, 2, 9, 10);
        Direct3D9SurfaceRect previousBounds = new(0, 0, 16, 16);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp,
            initialBounds: layerBounds,
            forceClearType: true);
        int internalCalls = 0;
        int releaseCalls = 0;

        int result = renderTarget.EndLayer(
            new Direct3D9LayerEndState(
                layerBounds,
                layerBounds,
                0,
                previousBounds,
                SavedClearTypeHint: false),
            () => ++internalCalls,
            _ => releaseCalls++);
        bool? supportsClearType = null;
        int glyphResult = renderTarget.DrawGlyphs(
            canDrawText: true,
            static () => 0,
            static () => 0,
            value =>
            {
                supportsClearType = value;
                return 0;
            },
            static (_, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (0, 0, 0, previousBounds, 0, false),
            (result, internalCalls, releaseCalls, renderTarget.Bounds, glyphResult, supportsClearType));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenDrawGlyphsSucceedsWithoutDrawing()
    {
        AssertDisplayDrawingIsSkippedWhenRenderingIsDisabled(static (renderTarget, draw) => renderTarget.DrawGlyphs(draw));
    }

    [TestMethod]
    [DataRow(0, true)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, false)]
    public unsafe void WhenDisplayGlyphDrawingDelegatesThenBaseResultIsReturnedAfterExactlyOneScopedCall(
        int drawResult,
        bool expectedValidContents)
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        int drawCalls = 0;
        bool enteredDuringDraw = false;
        bool inUseContextDuringDraw = false;

        int result = renderTarget.DrawGlyphs(() =>
        {
            drawCalls++;
            enteredDuringDraw = device.IsEntered();
            inUseContextDuringDraw = device.IsInUseContext();
            return drawResult;
        });

        Assert.AreEqual(
            (drawResult, 1, true, true, expectedValidContents, false, false),
            (result, drawCalls, enteredDuringDraw, inUseContextDuringDraw, renderTarget.HasValidContents,
                device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDisplayGlyphDrawingIsDisabledThenNullOperationIsIgnoredBeforeEnteringDeviceScopes()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));

        int result = renderTarget.DrawGlyphs(null!);

        Assert.AreEqual(
            (0, false, false, false),
            (result, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenDrawVideoSucceedsWithoutDrawing()
    {
        AssertDisplayDrawingIsSkippedWhenRenderingIsDisabled(static (renderTarget, draw) => renderTarget.DrawVideo(draw));
    }

    [TestMethod]
    [DataRow(0, true)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, false)]
    public unsafe void WhenDisplayVideoDrawingDelegatesThenBaseResultIsReturnedAfterExactlyOneScopedCall(
        int drawResult,
        bool expectedValidContents)
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        int drawCalls = 0;
        bool enteredDuringDraw = false;
        bool inUseContextDuringDraw = false;

        int result = renderTarget.DrawVideo(() =>
        {
            drawCalls++;
            enteredDuringDraw = device.IsEntered();
            inUseContextDuringDraw = device.IsInUseContext();
            return drawResult;
        });

        Assert.AreEqual(
            (drawResult, 1, true, true, expectedValidContents, false, false),
            (result, drawCalls, enteredDuringDraw, inUseContextDuringDraw, renderTarget.HasValidContents,
                device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDisplayVideoDrawingIsDisabledThenNullOperationIsIgnoredBeforeEnteringDeviceScopes()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));

        int result = renderTarget.DrawVideo(null!);

        Assert.AreEqual(
            (0, false, false, false),
            (result, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenGlyphRenderTargetIsInvalidThenNativePipelineIsSkippedAndDisplayContentsBecomeValid()
    {
        Direct3D9SwapChain? swapChain = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        swapChain!.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            () => ++callbackCalls,
            () => ++callbackCalls,
            _ => ++callbackCalls,
            (_, _) => ++callbackCalls);

        Assert.AreEqual(
            (0, 0, true, false, false),
            (result, callbackCalls, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingGlyphsToInvalidDirectSurfaceThenDependenciesAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            () => ++callbackCalls,
            () => ++callbackCalls,
            _ => ++callbackCalls,
            (_, _) => ++callbackCalls);

        Assert.AreEqual((0, 0, false, false), (result, callbackCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenHardwareGlyphDrawingSucceedsThenNativeOuterOrderAndScopesArePreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            () =>
            {
                calls.Add($"Realize:{device.IsEntered()}:{device.IsInUseContext()}");
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            supportsClearType =>
            {
                calls.Add($"Hardware:{supportsClearType}");
                return 0;
            },
            static (_, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual((0, "Realize:True:True|Ensure|Hardware:True", false, false),
            (result, string.Join('|', calls), device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    [DataRow(false, "Ensure|Software:88980088")]
    [DataRow(true, "Realize|Ensure|Hardware")]
    public unsafe void WhenDrawingGlyphsThenHardwareEligibilityComesFromDevice(bool canDrawText, string expectedCalls)
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            canDrawText: canDrawText);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            () =>
            {
                calls.Add("Realize");
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            _ =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (_, reason) =>
            {
                calls.Add($"Software:{reason:X8}");
                return 0;
            });

        Assert.AreEqual((0, expectedCalls), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenSettingClearTypeHintThenSubsequentGlyphDrawingUsesUpdatedHintWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        bool? actualSupportsClearType = null;

        int setResult = renderTarget.SetClearTypeHint(forceClearType: true);
        int drawResult = renderTarget.DrawGlyphs(
            canDrawText: true,
            static () => 0,
            static () => 0,
            supportsClearType =>
            {
                actualSupportsClearType = supportsClearType;
                return 0;
            },
            static (_, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, true, false),
            (setResult, drawResult, actualSupportsClearType, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenClearingClearTypeHintThenSubsequentGlyphDrawingUsesRestoredTargetCapabilityWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        bool? actualSupportsClearType = null;

        int enableResult = renderTarget.SetClearTypeHint(forceClearType: true);
        int disableResult = renderTarget.SetClearTypeHint(forceClearType: false);
        int drawResult = renderTarget.DrawGlyphs(
            canDrawText: true,
            static () => 0,
            static () => 0,
            supportsClearType =>
            {
                actualSupportsClearType = supportsClearType;
                return 0;
            },
            static (_, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, false, false, false),
            (enableResult, disableResult, drawResult, actualSupportsClearType, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenSettingClearTypeHintOnInvalidDirectSurfaceThenStateIsUpdatedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        surface.Dispose();
        bool? actualSupportsClearType = null;

        int setResult = renderTarget.SetClearTypeHint(forceClearType: true);
        int drawResult = renderTarget.DrawGlyphs(
            canDrawText: true,
            static () => 0,
            static () => 0,
            supportsClearType =>
            {
                actualSupportsClearType = supportsClearType;
                return 0;
            },
            static (_, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, Direct3D9Factory.SuccessHResult, null, false, false),
            (setResult, drawResult, actualSupportsClearType, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    [DataRow(false, false, true)]
    [DataRow(true, false, false)]
    [DataRow(true, true, true)]
    public unsafe void WhenDrawingGlyphsThenClearTypeSupportComesFromTargetAlphaAndForceFlag(
        bool hasAlpha,
        bool forceClearType,
        bool expectedSupportsClearType)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: hasAlpha ? MilPixelFormat.Pbgra32Bpp : MilPixelFormat.Bgr32Bpp,
            forceClearType: forceClearType);
        bool? actualSupportsClearType = null;

        int result = renderTarget.DrawGlyphs(
            canDrawText: true,
            static () => 0,
            static () => 0,
            supportsClearType =>
            {
                actualSupportsClearType = supportsClearType;
                return 0;
            },
            static (_, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual((0, expectedSupportsClearType), (result, actualSupportsClearType));
    }

    [TestMethod]
    public unsafe void WhenHardwareTextIsUnavailableThenSoftwareFallbackReceivesNativeReason()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: false, CanDrawText: false),
            static () => Direct3D9Factory.GenericFailureHResult,
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            static _ => Direct3D9Factory.GenericFailureHResult,
            (supportsClearType, reason) =>
            {
                calls.Add($"Software:{supportsClearType}:{reason:X8}");
                return 0;
            });

        Assert.AreEqual((0, "Ensure|Software:False:88980088"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenHardwareGlyphDrawingIsNotImplementedThenSoftwareFallbackReceivesFirstFailure()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int fallbackReason = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            static () => 0,
            static () => 0,
            static _ => Direct3D9Factory.NotImplementedHResult,
            (_, reason) =>
            {
                fallbackReason = reason;
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.NotImplementedHResult), (result, fallbackReason));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.ClippedToEmptyHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenHardwareGlyphDrawingReturnsNoRenderResultThenDrawSucceedsWithoutFallback(int noRenderResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int fallbackCalls = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            static () => 0,
            static () => 0,
            _ => noRenderResult,
            (_, _) => ++fallbackCalls);

        Assert.AreEqual((0, 0), (result, fallbackCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.ClippedToEmptyHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenSoftwareGlyphFallbackReturnsNoRenderResultThenDrawSucceeds(int noRenderResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int fallbackReason = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            static () => 0,
            static () => 0,
            static _ => Direct3D9Factory.DeviceCannotRenderTextHResult,
            (_, reason) =>
            {
                fallbackReason = reason;
                return noRenderResult;
            });

        Assert.AreEqual((0, Direct3D9Factory.DeviceCannotRenderTextHResult), (result, fallbackReason));
    }

    [TestMethod]
    public unsafe void WhenGlyphBrushRealizationCannotRenderTextThenSoftwareFallbackReceivesFirstFailure()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            (out Direct3D9RealizedGlyphBrushState realizedBrushState) =>
            {
                calls.Add("Realize");
                realizedBrushState = default;
                return Direct3D9Factory.DeviceCannotRenderTextHResult;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            _ =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (supportsClearType, reason) =>
            {
                calls.Add($"Software:{supportsClearType}:{reason:X8}");
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "Realize|Software:True:88980088"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenGlyphBrushRealizationIsNotImplementedThenSoftwareFallbackSuccessReplacesFailure()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int fallbackReason = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: false, CanDrawText: true),
            (out Direct3D9RealizedGlyphBrushState realizedBrushState) =>
            {
                realizedBrushState = default;
                return Direct3D9Factory.NotImplementedHResult;
            },
            static () => Direct3D9Factory.GenericFailureHResult,
            static _ => Direct3D9Factory.GenericFailureHResult,
            (_, reason) =>
            {
                fallbackReason = reason;
                return 0;
            });

        Assert.AreEqual((0, Direct3D9Factory.NotImplementedHResult), (result, fallbackReason));
    }

    [TestMethod]
    public unsafe void WhenGlyphStateIsClippedToEmptyThenDrawingAndFallbackAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int drawCalls = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            static () => 0,
            static () => Direct3D9Factory.ClippedToEmptyHResult,
            _ => ++drawCalls,
            (_, _) => ++drawCalls);

        Assert.AreEqual((0, 0), (result, drawCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenGlyphStateReturnsNoRenderResultThenDrawSucceedsWithoutFallback(int noRenderResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int subsequentCalls = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            static () => 0,
            () => noRenderResult,
            _ => ++subsequentCalls,
            (_, _) => ++subsequentCalls);

        Assert.AreEqual((0, 0), (result, subsequentCalls));
    }

    [TestMethod]
    public unsafe void WhenGlyphStateIsNotImplementedThenSoftwareFallbackReplacesFailure()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            static () => 0,
            () =>
            {
                calls.Add("Ensure");
                return Direct3D9Factory.NotImplementedHResult;
            },
            _ =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (supportsClearType, reason) =>
            {
                calls.Add($"Software:{supportsClearType}:{reason:X8}");
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "Ensure|Software:True:80004001"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenGlyphStateCannotRenderTextThenSoftwareFallbackSuccessReplacesFailure()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int fallbackReason = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: false, CanDrawText: true),
            static () => 0,
            static () => Direct3D9Factory.DeviceCannotRenderTextHResult,
            static _ => Direct3D9Factory.GenericFailureHResult,
            (_, reason) =>
            {
                fallbackReason = reason;
                return 0;
            });

        Assert.AreEqual((0, Direct3D9Factory.DeviceCannotRenderTextHResult), (result, fallbackReason));
    }

    [TestMethod]
    public unsafe void WhenGlyphStateFailsThenFailureIsPreservedWithoutFallback()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int subsequentCalls = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            static () => 0,
            static () => Direct3D9Factory.GenericFailureHResult,
            _ => ++subsequentCalls,
            (_, _) => ++subsequentCalls);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, subsequentCalls));
    }

    [TestMethod]
    public unsafe void WhenGlyphBrushMayNeedNonPowerOfTwoTilingThenHardwareRealizationIsSkipped()
    {
        Caps9 capabilities = default;
        capabilities.TextureCaps = (uint) D3D9.PtexturecapsPow2;
        using Direct3D9Device device = new(null, null, 0, Devtype.Hal, 0, new PresentParameters(windowed: true), capabilities: capabilities);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(
                TargetSupportsClearType: true,
                CanDrawText: true,
                RealizedBrushMayNeedNonPowerOfTwoTiling: true),
            () =>
            {
                calls.Add("Realize");
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            _ =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (_, reason) =>
            {
                calls.Add($"Software:{reason:X8}");
                return 0;
            });

        Assert.AreEqual((0, "Ensure|Software:88980088"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenRealizedGlyphBrushIsEmptyThenStateAndDrawingAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int subsequentCalls = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            (out Direct3D9RealizedGlyphBrushState realizedBrushState) =>
            {
                realizedBrushState = new Direct3D9RealizedGlyphBrushState(HasBrush: false);
                return 0;
            },
            () => ++subsequentCalls,
            _ => ++subsequentCalls,
            (_, _) => ++subsequentCalls);

        Assert.AreEqual((0, 0), (result, subsequentCalls));
    }

    [TestMethod]
    public unsafe void WhenRealizedBitmapGlyphBrushSourceClipIsPartialThenSoftwareFallbackRunsAfterState()
    {
        Caps9 capabilities = default;
        capabilities.TextureAddressCaps = (uint) D3D9.PtaddresscapsBorder;
        using Direct3D9Device device = new(null, null, 0, Devtype.Hal, 0, new PresentParameters(windowed: true), capabilities: capabilities);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(
                TargetSupportsClearType: false,
                CanDrawText: true,
                RealizedBrushWillHaveSourceClip: true,
                RealizedBrushSourceClipMayBeEntireSource: true),
            (out Direct3D9RealizedGlyphBrushState realizedBrushState) =>
            {
                calls.Add("Realize");
                realizedBrushState = new Direct3D9RealizedGlyphBrushState(
                    HasBrush: true,
                    IsBitmapBrush: true,
                    HasSourceClip: true,
                    SourceClipIsEntireSource: false);
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            _ =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (supportsClearType, reason) =>
            {
                calls.Add($"Software:{supportsClearType}:{reason:X8}");
                return 0;
            });

        Assert.AreEqual((0, "Realize|Ensure|Software:False:88980088"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenSoftwareGlyphFallbackRunsThenItUsesNestedUseContext()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using TestResource resource = new(device.ResourceManager);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        uint realizationDepth = 0;
        uint fallbackDepth = 0;
        uint drawingDepth = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: false),
            static (out Direct3D9RealizedGlyphBrushState state) =>
            {
                state = default;
                return 0;
            },
            static () => 0,
            static _ => 0,
            new Direct3D9SoftwareGlyphRenderer(
                (out Direct3D9RealizedSoftwareGlyphBrushState state) =>
                {
                    resource.SetAsEvictable();
                    realizationDepth = resource.ActiveUseContextDepth;
                    state = new Direct3D9RealizedSoftwareGlyphBrushState(HasBrush: true);
                    return 0;
                },
                _ =>
                {
                    resource.SetAsEvictable();
                    fallbackDepth = resource.ActiveUseContextDepth;
                    return 0;
                },
                (_, _) =>
                {
                    resource.SetAsEvictable();
                    drawingDepth = resource.ActiveUseContextDepth;
                    return 0;
                }));

        Assert.AreEqual(
            (0, 2u, 2u, 2u, 0u, false, false),
            (result, realizationDepth, fallbackDepth, drawingDepth, resource.ActiveUseContextDepth, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenSoftwareGlyphBrushRealizationFailsThenFailureIsPreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: false),
            static (out Direct3D9RealizedGlyphBrushState state) =>
            {
                state = default;
                return 0;
            },
            () =>
            {
                calls.Add("EnsureState");
                return 0;
            },
            static _ => 0,
            new Direct3D9SoftwareGlyphRenderer(
                (out Direct3D9RealizedSoftwareGlyphBrushState state) =>
                {
                    calls.Add("RealizeSoftware");
                    state = default;
                    return Direct3D9Factory.InvalidCallHResult;
                },
                _ =>
                {
                    calls.Add("GetFallback");
                    return 0;
                },
                (_, _) =>
                {
                    calls.Add("DrawSoftware");
                    return 0;
                }));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "EnsureState|RealizeSoftware"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenSoftwareGlyphBrushIsEmptyThenFallbackAndDrawingAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int subsequentCalls = 0;

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: false),
            static (out Direct3D9RealizedGlyphBrushState state) =>
            {
                state = default;
                return 0;
            },
            static () => 0,
            static _ => 0,
            new Direct3D9SoftwareGlyphRenderer(
                static (out Direct3D9RealizedSoftwareGlyphBrushState state) =>
                {
                    state = new Direct3D9RealizedSoftwareGlyphBrushState(HasBrush: false);
                    return 0;
                },
                _ => ++subsequentCalls,
                (_, _) => ++subsequentCalls));

        Assert.AreEqual((0, 0), (result, subsequentCalls));
    }

    [TestMethod]
    public unsafe void WhenSoftwareGlyphFallbackAcquisitionFailsThenDrawingIsSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: false, CanDrawText: false),
            static (out Direct3D9RealizedGlyphBrushState state) =>
            {
                state = default;
                return 0;
            },
            static () => 0,
            static _ => 0,
            new Direct3D9SoftwareGlyphRenderer(
                (out Direct3D9RealizedSoftwareGlyphBrushState state) =>
                {
                    calls.Add("RealizeSoftware");
                    state = new Direct3D9RealizedSoftwareGlyphBrushState(HasBrush: true, EffectAlpha: 0.5f);
                    return 0;
                },
                reason =>
                {
                    calls.Add($"GetFallback:{reason:X8}");
                    return Direct3D9Factory.GenericFailureHResult;
                },
                (_, _) =>
                {
                    calls.Add("DrawSoftware");
                    return 0;
                }));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "RealizeSoftware|GetFallback:88980088"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenSoftwareGlyphDrawingSucceedsThenNativeArgumentsAndOrderArePreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: false),
            static (out Direct3D9RealizedGlyphBrushState state) =>
            {
                state = default;
                return 0;
            },
            () =>
            {
                calls.Add("EnsureState");
                return 0;
            },
            static _ => 0,
            new Direct3D9SoftwareGlyphRenderer(
                (out Direct3D9RealizedSoftwareGlyphBrushState state) =>
                {
                    calls.Add("RealizeSoftware");
                    state = new Direct3D9RealizedSoftwareGlyphBrushState(HasBrush: true, EffectAlpha: 0.375f);
                    return 0;
                },
                reason =>
                {
                    calls.Add($"GetFallback:{reason:X8}");
                    return 0;
                },
                (supportsClearType, effectAlpha) =>
                {
                    calls.Add($"DrawSoftware:{supportsClearType}:{effectAlpha}");
                    return 0;
                }));

        Assert.AreEqual(
            (0, "EnsureState|RealizeSoftware|GetFallback:88980088|DrawSoftware:True:0.375"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenSoftwareGlyphDrawingFailsThenFailureAndNativeArgumentsArePreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: false, CanDrawText: false),
            static (out Direct3D9RealizedGlyphBrushState state) =>
            {
                state = default;
                return 0;
            },
            () =>
            {
                calls.Add("EnsureState");
                return 0;
            },
            static _ => 0,
            new Direct3D9SoftwareGlyphRenderer(
                (out Direct3D9RealizedSoftwareGlyphBrushState state) =>
                {
                    calls.Add("RealizeSoftware");
                    state = new Direct3D9RealizedSoftwareGlyphBrushState(HasBrush: true, EffectAlpha: 0.625f);
                    return 0;
                },
                reason =>
                {
                    calls.Add($"GetFallback:{reason:X8}");
                    return 0;
                },
                (supportsClearType, effectAlpha) =>
                {
                    calls.Add($"DrawSoftware:{supportsClearType}:{effectAlpha}");
                    return Direct3D9Factory.InvalidCallHResult;
                }));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult,
                "EnsureState|RealizeSoftware|GetFallback:88980088|DrawSoftware:False:0.625"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.ClippedToEmptyHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenSoftwareGlyphDrawingReturnsNoRenderResultThenDrawSucceeds(int noRenderResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: false),
            static (out Direct3D9RealizedGlyphBrushState state) =>
            {
                state = default;
                return 0;
            },
            () =>
            {
                calls.Add("EnsureState");
                return 0;
            },
            static _ => 0,
            new Direct3D9SoftwareGlyphRenderer(
                (out Direct3D9RealizedSoftwareGlyphBrushState state) =>
                {
                    calls.Add("RealizeSoftware");
                    state = new Direct3D9RealizedSoftwareGlyphBrushState(HasBrush: true, EffectAlpha: 0.75f);
                    return 0;
                },
                reason =>
                {
                    calls.Add($"GetFallback:{reason:X8}");
                    return 0;
                },
                (supportsClearType, effectAlpha) =>
                {
                    calls.Add($"DrawSoftware:{supportsClearType}:{effectAlpha}");
                    return noRenderResult;
                }));

        Assert.AreEqual(
            (0, "EnsureState|RealizeSoftware|GetFallback:88980088|DrawSoftware:True:0.75"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenFillPathClipsShapeThenRealizedBrushAndHardwarePathUseClippedValues()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        Matrix4x4 shapeToDevice = Matrix4x4.CreateScale(2);
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        MilRectF originalBounds = new(1, 2, 9, 10);
        MilRectF clippedBounds = new(3, 4, 7, 8);
        List<string> calls = [];

        int result = renderTarget.FillPath(
            11,
            shapeToDevice,
            originalBounds,
            13,
            worldToDevice,
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform == shapeToDevice}:{bounds}:{device.IsInUseContext()}");
                clippedShape = new Direct3D9SafeClippedShape(17, null, clippedBounds, true);
                return 0;
            },
            (bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{convertNull}");
                brush = 19;
                effects = 23;
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (shape, transform, bounds, brush, world, effects) =>
            {
                calls.Add($"Hardware:{shape}:{transform is null}:{bounds}:{brush}:{world == worldToDevice}:{effects}");
                return 0;
            },
            static (_, _, _, _) => Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(
            (0, "Clip:11:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }:True|Brush:False|Ensure|Hardware:17:True:MilRectF { Left = 3, Top = 4, Right = 7, Bottom = 8 }:19:True:23", false),
            (result, string.Join('|', calls), device.IsInUseContext()));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenClippedFillPathHardwareFillDoesNotPermitDrawingThenResultIsNormalizedWithoutSoftwareFallback(
        int hardwareResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        List<string> calls = [];

        int result = renderTarget.FillPath(
            1,
            Matrix4x4.Identity,
            new MilRectF(1, 2, 9, 10),
            2,
            worldToDevice,
            static (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(5, Matrix4x4.Identity, new MilRectF(3, 4, 7, 8), true);
                return 0;
            },
            static (bool _, out nint brush, out nint effects) =>
            {
                brush = 11;
                effects = 13;
                return 0;
            },
            static () => 0,
            (shape, transform, bounds, brush, world, effects) =>
            {
                calls.Add($"Hardware:{shape}:{transform is null}:{bounds}:{brush}:{world == worldToDevice}:{effects}");
                return hardwareResult;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (expectedResult, "Hardware:5:True:MilRectF { Left = 3, Top = 4, Right = 7, Bottom = 8 }:11:True:13"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenFillPathSafeBoundsClippingIsNotImplementedThenSoftwareFallbackUsesOriginalShapeAndTransform()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        Matrix4x4 shapeToDevice = Matrix4x4.CreateScale(2);
        MilRectF shapeBounds = new(1, 2, 9, 10);
        List<string> calls = [];

        int result = renderTarget.FillPath(
            11,
            shapeToDevice,
            shapeBounds,
            13,
            Matrix4x4.Identity,
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform == shapeToDevice}:{bounds}:{device.IsInUseContext()}");
                clippedShape = default;
                return Direct3D9Factory.NotImplementedHResult;
            },
            (bool _, out nint brush, out nint effects) =>
            {
                calls.Add("Brush");
                brush = 17;
                effects = 19;
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (_, _, _, _, _, _) =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (shape, transform, realizer, reason) =>
            {
                calls.Add($"Software:{shape}:{transform == shapeToDevice}:{realizer}:{reason:X8}:{device.IsInUseContext()}");
                return 0;
            });

        Assert.AreEqual(
            (0, "Clip:11:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }:True|Software:11:True:13:80004001:True", false),
            (result, string.Join('|', calls), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenFillPathRealizesNullBrushThenDrawingSucceedsWithoutEnsureOrFallback()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        int ensureCalls = 0;
        int hardwareCalls = 0;
        int softwareCalls = 0;

        int result = renderTarget.FillPath(
            1,
            null,
            default,
            2,
            Matrix4x4.Identity,
            static (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            static (bool _, out nint brush, out nint effects) =>
            {
                brush = 0;
                effects = 0;
                return 0;
            },
            () =>
            {
                ensureCalls++;
                return 0;
            },
            (_, _, _, _, _, _) =>
            {
                hardwareCalls++;
                return 0;
            },
            (_, _, _, _) =>
            {
                softwareCalls++;
                return 0;
            });

        Assert.AreEqual((0, 0, 0, 0), (result, ensureCalls, hardwareCalls, softwareCalls));
    }

    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenClippedFillPathHardwareFillIsNotImplementedThenSoftwareFallbackResultIsNormalized(
        int softwareResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        (nint Shape, Matrix4x4? Transform, nint Realizer, int Reason) fallback = default;

        int result = renderTarget.FillPath(
            3,
            Matrix4x4.Identity,
            new MilRectF(0, 0, 4, 5),
            7,
            Matrix4x4.Identity,
            static (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(5, null, new MilRectF(1, 1, 3, 4), true);
                return 0;
            },
            static (bool _, out nint brush, out nint effects) =>
            {
                brush = 11;
                effects = 0;
                return 0;
            },
            static () => 0,
            static (_, _, _, _, _, _) => Direct3D9Factory.NotImplementedHResult,
            (shape, transform, realizer, reason) =>
            {
                fallback = (shape, transform, realizer, reason);
                return softwareResult;
            });

        Assert.AreEqual(
            (expectedResult, 5, null, 7, Direct3D9Factory.NotImplementedHResult),
            (result, fallback.Shape, fallback.Transform, fallback.Realizer, fallback.Reason));
    }

    [TestMethod]
    public unsafe void WhenFillPathBrushEffectsAreNotImplementedThenSoftwareFallbackReceivesOriginalShapeAndReason()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        (nint Shape, Matrix4x4? Transform, nint Realizer, int Reason) fallback = default;
        int ensureCalls = 0;
        int hardwareCalls = 0;

        int result = renderTarget.FillPath(
            3,
            Matrix4x4.Identity,
            new MilRectF(0, 0, 4, 5),
            7,
            Matrix4x4.Identity,
            static (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            static (bool _, out nint brush, out nint effects) =>
            {
                brush = 0;
                effects = 0;
                return Direct3D9Factory.NotImplementedHResult;
            },
            () => ++ensureCalls,
            (_, _, _, _, _, _) => ++hardwareCalls,
            (shape, transform, realizer, reason) =>
            {
                fallback = (shape, transform, realizer, reason);
                return 0;
            });

        Assert.AreEqual(
            (0, 3, Matrix4x4.Identity, 7, Direct3D9Factory.NotImplementedHResult, 0, 0),
            (result, fallback.Shape, fallback.Transform, fallback.Realizer, fallback.Reason, ensureCalls, hardwareCalls));
    }

    [TestMethod]
    public unsafe void WhenFillPathEnsureStateIsNotImplementedThenSoftwareFallbackReplacesFailure()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        (nint Shape, Matrix4x4? Transform, nint Realizer, int Reason) fallback = default;
        int hardwareCalls = 0;

        int result = renderTarget.FillPath(
            1,
            Matrix4x4.Identity,
            new MilRectF(1, 2, 9, 10),
            2,
            Matrix4x4.Identity,
            static (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                clippedShape = new Direct3D9SafeClippedShape(5, Matrix4x4.Identity, new MilRectF(3, 4, 7, 8), true);
                return 0;
            },
            static (bool _, out nint brush, out nint effects) =>
            {
                brush = 3;
                effects = 0;
                return 0;
            },
            static () => Direct3D9Factory.NotImplementedHResult,
            (_, _, _, _, _, _) => ++hardwareCalls,
            (shape, transform, realizer, reason) =>
            {
                fallback = (shape, transform, realizer, reason);
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 5, null, 2, Direct3D9Factory.NotImplementedHResult, 0),
            (result, fallback.Shape, fallback.Transform, fallback.Realizer, fallback.Reason, hardwareCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    public unsafe void WhenClippedFillPathBrushEffectsFailThenResultIsNormalizedAndLaterStagesAreSkipped(
        int brushResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        List<string> calls = [];

        int result = renderTarget.FillPath(
            1,
            Matrix4x4.Identity,
            new MilRectF(1, 2, 9, 10),
            2,
            Matrix4x4.Identity,
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform is not null}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(5, Matrix4x4.Identity, new MilRectF(3, 4, 7, 8), true);
                return 0;
            },
            (bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{convertNull}");
                brush = 11;
                effects = 13;
                return brushResult;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (_, _, _, _, _, _) =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (expectedResult, "Clip:1:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }|Brush:False"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult, 0)]
    [DataRow(Direct3D9Factory.BadNumberHResult, 0)]
    [DataRow(Direct3D9Factory.ClippedToEmptyHResult, 0)]
    public unsafe void WhenClippedFillPathEnsureStateDoesNotPermitDrawingThenResultIsNormalizedAndFillStagesAreSkipped(
        int ensureResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9DeviceEntryGuard deviceEntry = new(device);
        List<string> calls = [];

        int result = renderTarget.FillPath(
            1,
            Matrix4x4.Identity,
            new MilRectF(1, 2, 9, 10),
            2,
            Matrix4x4.Identity,
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform is not null}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(5, Matrix4x4.Identity, new MilRectF(3, 4, 7, 8), true);
                return 0;
            },
            (bool convertNull, out nint brush, out nint effects) =>
            {
                calls.Add($"Brush:{convertNull}");
                brush = 3;
                effects = 7;
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return ensureResult;
            },
            (_, _, _, _, _, _) =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (expectedResult, "Clip:1:True:MilRectF { Left = 1, Top = 2, Right = 9, Bottom = 10 }|Brush:False|Ensure"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenFillPathWithBrushIsAntialiasedThenClippingBrushAndGeometryOrderIsPreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Matrix4x4 shapeToDevice = Matrix4x4.CreateScale(2);
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        List<string> calls = [];
        Direct3D9PathBrushContext? derivedBrushContext = null;

        int result = renderTarget.FillPathWithBrush(
            3,
            shapeToDevice,
            new MilRectF(1, 2, 9, 10),
            5,
            worldToDevice,
            7,
            MilAntiAliasMode.EightByEight,
            new Direct3D9SurfaceRect(0, 0, 8, 9),
            (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) =>
            {
                calls.Add($"Guidelines:{state.Shape}:{state.ShapeToDevice == shapeToDevice}");
                updated = state with { Shape = 11 };
                return 0;
            },
            (Direct3D9PathClipperState state, nint brush, Matrix4x4 world, out Direct3D9PathClipperState updated) =>
            {
                calls.Add($"BrushClip:{state.Shape}:{brush}:{world == worldToDevice}");
                updated = state with { Shape = 13, ShapeToDevice = null };
                return 0;
            },
            (Direct3D9PathClipperState state, out MilRectF bounds) =>
            {
                calls.Add($"Bounds:{state.Shape}:{state.ShapeToDevice is null}");
                bounds = new MilRectF(-1.2f, 2.2f, 7.2f, 10.1f);
                return 0;
            },
            (nint brush, Direct3D9PathBrushContext context, out nint hardwareBrush) =>
            {
                derivedBrushContext = context;
                calls.Add($"Derive:{brush}:{context.WorldToDevice == worldToDevice}:{context.RenderingBounds}:{context.SamplingBounds}:{context.CanFallback}");
                hardwareBrush = 17;
                return 0;
            },
            (Direct3D9PathClipperState state, out nint geometry) =>
            {
                calls.Add($"AA:{state.Shape}");
                geometry = 19;
                return 0;
            },
            static (Direct3D9PathClipperState _, out nint geometry) =>
            {
                geometry = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (geometry, hardwareBrush, effects, context) =>
            {
                calls.Add($"Draw:{geometry}:{hardwareBrush}:{effects}:{ReferenceEquals(context, derivedBrushContext)}:{context.WorldToDevice == worldToDevice}:{context.RenderingBounds}:{context.SamplingBounds}:{context.CanFallback}");
                return 0;
            },
            hardwareBrush => calls.Add($"ReleaseBrush:{hardwareBrush}"),
            geometryGenerator => calls.Add($"ReleaseGeometry:{geometryGenerator}"));

        Assert.AreEqual(
            (0, "Guidelines:3:True|BrushClip:11:5:True|Bounds:13:True|Derive:5:True:Direct3D9SurfaceRect { Left = 0, Top = 2, Right = 8, Bottom = 9 }:MilRectF { Left = 0, Top = 2, Right = 8, Bottom = 9 }:True|AA:13|Draw:19:17:7:True:True:Direct3D9SurfaceRect { Left = 0, Top = 2, Right = 8, Bottom = 9 }:MilRectF { Left = 0, Top = 2, Right = 8, Bottom = 9 }:True|ReleaseBrush:17|ReleaseGeometry:19"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    [DataRow((int) MilAntiAliasMode.None, "Aliased")]
    [DataRow((int) MilAntiAliasMode.EightByEight, "Antialiased")]
    public unsafe void WhenShaderFillingPathThenSelectedGeometryDrawAndCleanupOrderMatchesNative(
        int antiAliasModeValue,
        string expectedGeometryCall)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Matrix4x4 shapeToDevice = Matrix4x4.CreateScale(2);
        Direct3D9SurfaceRect renderingBounds = new(1, 2, 9, 10);
        List<string> calls = [];

        int result = renderTarget.HwShaderFillPath(
            3,
            5,
            shapeToDevice,
            renderingBounds,
            (MilAntiAliasMode) antiAliasModeValue,
            useZBuffer: true,
            (Direct3D9PathClipperState state, out nint geometry) =>
            {
                calls.Add($"Antialiased:{state.Shape}:{state.ShapeToDevice == shapeToDevice}:{state.Bounds}");
                geometry = 7;
                return 0;
            },
            (Direct3D9PathClipperState state, out nint geometry) =>
            {
                calls.Add($"Aliased:{state.Shape}:{state.ShapeToDevice == shapeToDevice}:{state.Bounds}");
                geometry = 7;
                return 0;
            },
            (shader, geometry, bounds, useZBuffer) =>
            {
                calls.Add($"Draw:{shader}:{geometry}:{bounds}:{useZBuffer}");
                return 0;
            },
            geometry => calls.Add($"Release:{geometry}"));

        Assert.AreEqual(
            (0, $"{expectedGeometryCall}:5:True:MilRectF {{ Left = 1, Top = 2, Right = 9, Bottom = 10 }}|Draw:3:7:{renderingBounds}:True|Release:7"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenShaderPathGeometryIsEmptyThenDrawingAndCleanupAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int drawCalls = 0;
        int releaseCalls = 0;

        int result = renderTarget.HwShaderFillPath(
            3,
            5,
            null,
            new Direct3D9SurfaceRect(1, 2, 9, 10),
            MilAntiAliasMode.None,
            useZBuffer: false,
            static (Direct3D9PathClipperState _, out nint geometry) =>
            {
                geometry = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            static (Direct3D9PathClipperState _, out nint geometry) =>
            {
                geometry = 0;
                return Direct3D9Factory.EmptyFillHResult;
            },
            (_, _, _, _) => ++drawCalls,
            _ => releaseCalls++);

        Assert.AreEqual((0, 0, 0), (result, drawCalls, releaseCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult)]
    [DataRow(0, Direct3D9Factory.InternalErrorHResult)]
    public unsafe void WhenShaderPathGeometryCannotBeCreatedThenFailureIsPropagatedAndDrawingIsSkipped(
        int createResult,
        int expectedResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int drawCalls = 0;

        int result = renderTarget.HwShaderFillPath(
            3,
            5,
            null,
            new Direct3D9SurfaceRect(1, 2, 9, 10),
            MilAntiAliasMode.None,
            useZBuffer: false,
            static (Direct3D9PathClipperState _, out nint geometry) =>
            {
                geometry = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (Direct3D9PathClipperState _, out nint geometry) =>
            {
                geometry = 0;
                return createResult;
            },
            (_, _, _, _) => ++drawCalls,
            static _ => { });

        Assert.AreEqual((expectedResult, 0), (result, drawCalls));
    }

    [TestMethod]
    public unsafe void WhenShaderPathDrawFailsThenGeometryIsStillReleased()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.HwShaderFillPath(
            3,
            5,
            null,
            new Direct3D9SurfaceRect(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            (Direct3D9PathClipperState _, out nint geometry) =>
            {
                calls.Add("Create");
                geometry = 7;
                return 0;
            },
            static (Direct3D9PathClipperState _, out nint geometry) =>
            {
                geometry = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            (_, _, _, _) =>
            {
                calls.Add("Draw");
                return Direct3D9Factory.GenericFailureHResult;
            },
            geometry => calls.Add($"Release:{geometry}"));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Create|Draw|Release:7"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenFillPathWithBrushBoundsAreEmptyThenBrushAndGeometryAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int deriveCalls = 0;
        int geometryCalls = 0;

        int result = renderTarget.FillPathWithBrush(
            1,
            null,
            default,
            2,
            Matrix4x4.Identity,
            0,
            MilAntiAliasMode.None,
            new Direct3D9SurfaceRect(0, 0, 4, 4),
            static (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState state, nint _, Matrix4x4 _, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState _, out MilRectF bounds) => { bounds = new MilRectF(5, 5, 6, 6); return 0; },
            (nint _, Direct3D9PathBrushContext _, out nint hardwareBrush) => { deriveCalls++; hardwareBrush = 0; return 0; },
            (Direct3D9PathClipperState _, out nint geometry) => { geometryCalls++; geometry = 0; return 0; },
            (Direct3D9PathClipperState _, out nint geometry) => { geometryCalls++; geometry = 0; return 0; },
            static (_, _, _, _) => 0,
            static _ => { },
            static _ => { });

        Assert.AreEqual((0, 0, 0), (result, deriveCalls, geometryCalls));
    }

    [TestMethod]
    public unsafe void WhenAliasedGeometryIsEmptyThenHardwareBrushIsReleasedWithoutDrawing()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int antialiasedCalls = 0;
        int drawCalls = 0;
        nint releasedBrush = 0;

        int result = renderTarget.FillPathWithBrush(
            1,
            null,
            default,
            2,
            Matrix4x4.Identity,
            0,
            MilAntiAliasMode.None,
            new Direct3D9SurfaceRect(0, 0, 4, 4),
            static (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState state, nint _, Matrix4x4 _, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState _, out MilRectF bounds) => { bounds = new MilRectF(0, 0, 3, 3); return 0; },
            static (nint _, Direct3D9PathBrushContext _, out nint hardwareBrush) => { hardwareBrush = 11; return 0; },
            (Direct3D9PathClipperState _, out nint geometry) => { antialiasedCalls++; geometry = 0; return 0; },
            static (Direct3D9PathClipperState _, out nint geometry) => { geometry = 0; return Direct3D9Factory.EmptyFillHResult; },
            (_, _, _, _) => { drawCalls++; return 0; },
            hardwareBrush => releasedBrush = hardwareBrush,
            static _ => { });

        Assert.AreEqual((0, 0, 0, (nint) 11), (result, antialiasedCalls, drawCalls, releasedBrush));
    }

    [TestMethod]
    public unsafe void WhenAcceleratedFillPathFailsThenBrushAndGeometryAreReleasedInNativeOrder()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.FillPathWithBrush(
            1,
            null,
            default,
            2,
            Matrix4x4.Identity,
            3,
            MilAntiAliasMode.None,
            new Direct3D9SurfaceRect(0, 0, 4, 4),
            static (Direct3D9PathClipperState state, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState state, nint _, Matrix4x4 _, out Direct3D9PathClipperState updated) => { updated = state; return 0; },
            static (Direct3D9PathClipperState _, out MilRectF bounds) => { bounds = new MilRectF(0, 0, 3, 3); return 0; },
            static (nint _, Direct3D9PathBrushContext _, out nint hardwareBrush) => { hardwareBrush = 5; return 0; },
            static (Direct3D9PathClipperState _, out nint geometry) => { geometry = 0; return Direct3D9Factory.GenericFailureHResult; },
            static (Direct3D9PathClipperState _, out nint geometry) => { geometry = 7; return 0; },
            (_, _, _, _) => Direct3D9Factory.GenericFailureHResult,
            hardwareBrush => calls.Add($"Brush:{hardwareBrush}"),
            geometryGenerator => calls.Add($"Geometry:{geometryGenerator}"));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Brush:5|Geometry:7"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenAcceleratedFillPathSucceedsThenTwoDimensionalShaderPipelineOrderAndArgumentsArePreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9UseContextGuard useContext = new(device);
        Direct3D9SurfaceRect outsideBounds = new(1, 2, 7, 9);
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        Direct3D9PathBrushContext brushContext = new(
            worldToDevice,
            outsideBounds,
            new MilRectF(1, 2, 7, 9),
            true);
        List<string> calls = [];

        int result = renderTarget.AcceleratedFillPath(
            MilCompositingMode.SourceCopy,
            3,
            5,
            7,
            brushContext,
            outsideBounds,
            false,
            (is2D, currentDevice) =>
            {
                calls.Add($"Create:{is2D}:{ReferenceEquals(currentDevice, device)}:{device.IsInUseContext()}");
                return new Direct3D9ShaderPipeline(
                    (mode, geometry, brush, effects, context, bounds, needInside) =>
                    {
                        calls.Add($"Initialize:{mode}:{geometry}:{brush}:{effects}:{ReferenceEquals(context, brushContext)}:{context.WorldToDevice == worldToDevice}:{context.RenderingBounds}:{context.SamplingBounds}:{context.CanFallback}:{bounds}:{needInside}");
                        return 0;
                    },
                    () =>
                    {
                        calls.Add("Execute");
                        return 0;
                    },
                    () => calls.Add("Release"));
            });

        Assert.AreEqual(
            (0, "Create:True:True:True|Initialize:SourceCopy:3:5:7:True:True:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 7, Bottom = 9 }:MilRectF { Left = 1, Top = 2, Right = 7, Bottom = 9 }:True:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 7, Bottom = 9 }:False|Execute|Release"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenShaderPipelineSetupFailsAfterAttachingResourcesThenExecutionIsSkippedAndResourcesAreReleased()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.ShaderAcceleratedFillPath(
            MilCompositingMode.SourceOver,
            1,
            2,
            0,
            new Direct3D9PathBrushContext(Matrix4x4.Identity, default, default, true),
            null,
            true,
            (_, _) => new Direct3D9ShaderPipeline(
                (_, _, _, _, _, _, _) =>
                {
                    calls.Add("Initialize:AttachColorSource");
                    return Direct3D9Factory.GenericFailureHResult;
                },
                () =>
                {
                    calls.Add("Execute");
                    return 0;
                },
                () => calls.Add("ReleaseColorSource")));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Initialize:AttachColorSource|ReleaseColorSource"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenShaderPipelineExecutionFailsThenFailureIsPreservedAndResourcesAreReleased()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.ShaderAcceleratedFillPath(
            MilCompositingMode.DestInvert,
            1,
            2,
            0,
            new Direct3D9PathBrushContext(Matrix4x4.Identity, default, default, true),
            null,
            true,
            (_, _) => new Direct3D9ShaderPipeline(
                (_, _, _, _, _, _, _) =>
                {
                    calls.Add("Initialize");
                    return 0;
                },
                () =>
                {
                    calls.Add("Execute");
                    return Direct3D9Factory.GenericFailureHResult;
                },
                () => calls.Add("Release")));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Initialize|Execute|Release"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenDrawingBitmapToInvalidDirectSurfaceThenDependenciesAreSkipped()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            bitmapSource.Pointer,
            0,
            (out nint scratchBrush) =>
            {
                callbackCalls++;
                scratchBrush = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            () => ++callbackCalls,
            (_, _, _, _, _, _) => ++callbackCalls);

        Assert.AreEqual((0, 0, false, false), (result, callbackCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingBitmapWithExplicitSourceRectThenNativeDependencyOrderAndTransformsArePreserved()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        List<string> calls = [];

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(worldToDevice, new Direct3D9PointAndSizeRect(2, 5, 7, 11)),
            bitmapSource.Pointer,
            19,
            (out nint scratchBrush) =>
            {
                calls.Add($"Brush:{device.IsEntered()}:{device.IsInUseContext()}");
                scratchBrush = 23;
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (scratchBrush, source, effects, sourceRect, shapeToDevice, samplingToDevice) =>
            {
                calls.Add($"Fill:{scratchBrush}:{source == bitmapSource.Pointer}:{effects}:{sourceRect}:{shapeToDevice == worldToDevice}:{samplingToDevice == worldToDevice}");
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "Brush:True:True|Ensure|Fill:23:True:19:MilRectF { Left = 2, Top = 5, Right = 9, Bottom = 16 }:True:True", false, false),
            (result, string.Join('|', calls), device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingBitmapThroughBrushRealizerThenTemporaryStateIsReleasedInNativeOrder()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(1, 2, 3, 4)),
            bitmapSource.Pointer,
            19,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 23;
                return 0;
            },
            (brush, source, transform) => calls.Add($"Set:{brush}:{source == bitmapSource.Pointer}:{transform == Matrix4x4.Identity}"),
            brush => calls.Add($"Clear:{brush}"),
            brush => new Direct3D9ImmediateBrushRealizer(
                11,
                value => calls.Add($"AddRef:{value}"),
                value => calls.Add($"Release:{value}"),
                static (_, _) => { },
                static _ => false,
                static _ => Direct3D9BrushType.Bitmap,
                static _ => false,
                static _ => false,
                static _ => 0,
                static _ => 0,
                static (_, _, _, _) => 0,
                static (_, _, _, _) => 0,
                static (_, _) => { },
                static (_, _) => { }),
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (bounds, realizer, shapeToDevice, samplingToDevice) =>
            {
                calls.Add($"Fill:{bounds}:{realizer.GetRealizedBrush(false)}:{realizer.Effects}:{shapeToDevice == Matrix4x4.Identity}:{samplingToDevice == Matrix4x4.Identity}");
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "Ensure|Set:23:True:True|AddRef:23|AddRef:19|Fill:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }:23:19:True:True|Release:19|Release:23|Clear:23"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenDrawingBitmapThroughFillPathThenHardwareFallbackAndCleanupUseNativeOrder()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        List<string> calls = [];

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(worldToDevice, new Direct3D9PointAndSizeRect(1, 2, 3, 4)),
            bitmapSource.Pointer,
            19,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 23;
                return 0;
            },
            (brush, source, transform) => calls.Add($"Set:{brush}:{source == bitmapSource.Pointer}:{transform == worldToDevice}"),
            brush => calls.Add($"Clear:{brush}"),
            _ => new Direct3D9ImmediateBrushRealizer(
                11,
                value => calls.Add($"AddRef:{value}"),
                value => calls.Add($"Release:{value}"),
                static (_, _) => { },
                static _ => false,
                static _ => Direct3D9BrushType.Bitmap,
                static _ => false,
                static _ => false,
                static _ => 0,
                static _ => 0,
                static (_, _, _, _) => 0,
                static (_, _, _, _) => 0,
                static (_, _) => { },
                static (_, _) => { }),
            (MilRectF bounds, out nint shape) =>
            {
                calls.Add($"Shape:{bounds}");
                shape = 29;
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform == worldToDevice}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            (shape, transform, bounds, brush, world, effects) =>
            {
                calls.Add($"Hardware:{shape}:{transform == worldToDevice}:{bounds}:{brush}:{world == worldToDevice}:{effects}");
                return Direct3D9Factory.NotImplementedHResult;
            },
            (shape, transform, realizer, reason) =>
            {
                calls.Add($"Software:{shape}:{transform == worldToDevice}:{realizer is not null}:{reason}");
                return 0;
            });

        Assert.AreEqual(
            (0, "Shape:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }|Ensure|Set:23:True:True|AddRef:23|AddRef:19|Clip:29:True:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }|Ensure|Hardware:29:True:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }:23:True:19|Software:29:True:True:-2147467263|Release:19|Release:23|Clear:23"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenBitmapShapeCreationFailsThenStateAndTemporaryBrushInitializationAreSkipped()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(1, 2, 3, 4)),
            bitmapSource.Pointer,
            0,
            (out nint scratchBrush) =>
            {
                calls.Add("Brush");
                scratchBrush = 23;
                return 0;
            },
            (_, _, _) => calls.Add("Set"),
            _ => calls.Add("Clear"),
            _ =>
            {
                calls.Add("Realizer");
                throw new InvalidOperationException();
            },
            (MilRectF bounds, out nint shape) =>
            {
                calls.Add($"Shape:{bounds}");
                shape = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (nint _, Matrix4x4? _, MilRectF _, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add("Clip");
                clippedShape = default;
                return 0;
            },
            (_, _, _, _, _, _) =>
            {
                calls.Add("Hardware");
                return 0;
            },
            (_, _, _, _) =>
            {
                calls.Add("Software");
                return 0;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Brush|Shape:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenBitmapBrushRealizerCreationFailsThenTemporaryBrushIsCleared()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        Assert.ThrowsExactly<InvalidOperationException>(() => renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(0, 0, 1, 1)),
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 23;
                return 0;
            },
            (brush, _, _) => calls.Add($"Set:{brush}"),
            brush => calls.Add($"Clear:{brush}"),
            static _ => null!,
            static () => 0,
            static (_, _, _, _) => 0));

        Assert.AreEqual("Set:23|Clear:23", string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenBitmapRenderTargetIsInvalidThenNativePipelineIsSkippedAndDisplayContentsBecomeValid()
    {
        Direct3D9SwapChain? swapChain = null;
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        swapChain!.Dispose();
        int callbackCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            bitmapSource.Pointer,
            0,
            (out nint scratchBrush) =>
            {
                callbackCalls++;
                scratchBrush = 0;
                return 0;
            },
            () => ++callbackCalls,
            (_, _, _, _, _, _) => ++callbackCalls);

        Assert.AreEqual(
            (0, 0, true, false, false),
            (result, callbackCalls, renderTarget.HasValidContents, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDrawingBitmapWithoutSourceRectThenBitmapSizeDefinesBounds()
    {
        using FakeVideoBitmapSource bitmapSource = new(width: 31, height: 47);
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        MilRectF receivedBounds = default;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 1;
                return 0;
            },
            static () => 0,
            (_, _, _, bounds, _, _) =>
            {
                receivedBounds = bounds;
                return 0;
            });

        Assert.AreEqual((0, new MilRectF(0, 0, 31, 47)), (result, receivedBounds));
    }

    [TestMethod]
    public unsafe void WhenBitmapSizeFailsThenEnsureAndFillAreSkipped()
    {
        using FakeVideoBitmapSource bitmapSource = new(getSizeResult: Direct3D9Factory.GenericFailureHResult);
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int ensureCalls = 0;
        int fillCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 1;
                return 0;
            },
            () =>
            {
                ensureCalls++;
                return 0;
            },
            (_, _, _, _, _, _) =>
            {
                fillCalls++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 0), (result, ensureCalls, fillCalls));
    }

    [TestMethod]
    public unsafe void WhenScratchBitmapBrushRetrievalFailsThenDrawStopsBeforeReadingBitmapBounds()
    {
        using FakeVideoBitmapSource bitmapSource = new(getSizeResult: Direct3D9Factory.InvalidCallHResult);
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int subsequentCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            bitmapSource.Pointer,
            0,
            (out nint scratchBrush) =>
            {
                scratchBrush = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            () => ++subsequentCalls,
            (_, _, _, _, _, _) => ++subsequentCalls);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, subsequentCalls));
    }

    [TestMethod]
    public unsafe void WhenBitmapSourceRectIsExplicitThenBitmapBoundsAreNotRead()
    {
        using FakeVideoBitmapSource bitmapSource = new(getSizeResult: Direct3D9Factory.GenericFailureHResult);
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int fillCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(2, 3, 5, 7)),
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 1;
                return 0;
            },
            static () => 0,
            (_, _, _, sourceRect, _, _) =>
            {
                fillCalls++;
                return sourceRect == new MilRectF(2, 3, 7, 10)
                    ? 0
                    : Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual((0, 1), (result, fillCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenScratchBitmapBrushRetrievalReturnsNoRenderFailureThenDrawSucceeds(int noRenderResult)
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int subsequentCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(0, 0, 1, 1)),
            bitmapSource.Pointer,
            0,
            (out nint scratchBrush) =>
            {
                scratchBrush = 0;
                return noRenderResult;
            },
            () => ++subsequentCalls,
            (_, _, _, _, _, _) => ++subsequentCalls);

        Assert.AreEqual((0, 0), (result, subsequentCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenBitmapSizeReturnsNoRenderFailureThenDrawSucceedsWithoutEnsuringState(int noRenderResult)
    {
        using FakeVideoBitmapSource bitmapSource = new(getSizeResult: noRenderResult);
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int subsequentCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 1;
                return 0;
            },
            () => ++subsequentCalls,
            (_, _, _, _, _, _) => ++subsequentCalls);

        Assert.AreEqual((0, 0), (result, subsequentCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.ClippedToEmptyHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenBitmapStateReturnsNoRenderResultThenTemporaryBrushStateIsNotInitialized(int noRenderResult)
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int temporaryBrushCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(0, 0, 1, 1)),
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 1;
                return 0;
            },
            (_, _, _) => temporaryBrushCalls++,
            _ => temporaryBrushCalls++,
            _ =>
            {
                temporaryBrushCalls++;
                throw new InvalidOperationException();
            },
            () => noRenderResult,
            (_, _, _, _) => ++temporaryBrushCalls);

        Assert.AreEqual((0, 0), (result, temporaryBrushCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NotImplementedHResult)]
    public unsafe void WhenBitmapStateFailsThenTemporaryBrushStateIsNotInitialized(int stateResult)
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int temporaryBrushCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(0, 0, 1, 1)),
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 1;
                return 0;
            },
            (_, _, _) => temporaryBrushCalls++,
            _ => temporaryBrushCalls++,
            _ =>
            {
                temporaryBrushCalls++;
                throw new InvalidOperationException();
            },
            () => stateResult,
            (_, _, _, _) => ++temporaryBrushCalls);

        Assert.AreEqual((stateResult, 0), (result, temporaryBrushCalls));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.ClippedToEmptyHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenBitmapDrawingReturnsNoRenderResultThenDrawSucceeds(int noRenderResult)
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int fillCalls = 0;

        int result = renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity, new Direct3D9PointAndSizeRect(0, 0, 1, 1)),
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 1;
                return 0;
            },
            () => noRenderResult == Direct3D9Factory.ClippedToEmptyHResult ? noRenderResult : 0,
            (_, _, _, _, _, _) =>
            {
                fillCalls++;
                return noRenderResult;
            });

        Assert.AreEqual((0, noRenderResult == Direct3D9Factory.ClippedToEmptyHResult ? 0 : 1), (result, fillCalls));
    }

    [TestMethod]
    public unsafe void WhenDrawingBitmapAfterDisposeThenThrowsBeforeDependenciesRun()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.DrawBitmap(
            new Direct3D9BitmapDrawState(Matrix4x4.Identity),
            bitmapSource.Pointer,
            0,
            static (out nint scratchBrush) =>
            {
                scratchBrush = 0;
                return 0;
            },
            static () => 0,
            static (_, _, _, _, _, _) => 0));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenVideoSurfaceRendererIsSkipped()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int callbackCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device _, out nint bitmapSource) =>
                {
                    callbackCalls++;
                    bitmapSource = 0;
                    return Direct3D9Factory.GenericFailureHResult;
                },
                () => ++callbackCalls),
            0,
            _ => ++callbackCalls);

        Assert.AreEqual((0, 0, true, false, false), (result, callbackCalls, renderState.PrefilterEnabled, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenVideoBitmapSourceReferenceOperationsAreSkipped()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(0, 16));
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int drawCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            null,
            bitmapSource.Pointer,
            _ => ++drawCalls);

        Assert.AreEqual(
            (0, 0, 0, 0, true, false, false),
            (result, drawCalls, _videoBitmapAddRefCount, _videoBitmapReleaseCount, renderState.PrefilterEnabled, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenVideoRenderTargetIsInvalidThenNativePipelineIsSkippedAndDisplayContentsBecomeValid()
    {
        Direct3D9SwapChain? swapChain = null;
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        swapChain!.Dispose();
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int callbackCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device _, out nint currentBitmapSource) =>
                {
                    callbackCalls++;
                    currentBitmapSource = bitmapSource.Pointer;
                    return 0;
                },
                () => ++callbackCalls),
            bitmapSource.Pointer,
            _ => ++callbackCalls);

        Assert.AreEqual(
            (0, 0, true, true, 0, 0, false, false),
            (result, callbackCalls, renderState.PrefilterEnabled, renderTarget.HasValidContents, _videoBitmapAddRefCount, _videoBitmapReleaseCount, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenVideoDirectSurfaceIsInvalidThenNativePipelineIsSkipped()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 16);
        surface.Dispose();
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int callbackCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device _, out nint currentBitmapSource) =>
                {
                    callbackCalls++;
                    currentBitmapSource = bitmapSource.Pointer;
                    return 0;
                },
                () => ++callbackCalls),
            bitmapSource.Pointer,
            _ => ++callbackCalls);

        Assert.AreEqual(
            (0, 0, true, true, 0, 0, false, false),
            (result, callbackCalls, renderState.PrefilterEnabled, renderTarget.HasValidContents, _videoBitmapAddRefCount, _videoBitmapReleaseCount, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenVideoSurfaceRendererReturnsFrameThenDrawEndAndCleanupMatchNativeOrder()
    { 
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        List<string> calls = [];

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device currentDevice, out nint value) =>
                {
                    calls.Add($"Begin:{currentDevice.IsEntered()}:{currentDevice.IsInUseContext()}");
                    value = bitmapSource.Pointer;
                    return 0;
                },
                () =>
                {
                    calls.Add($"End:{device.IsEntered()}:{device.IsInUseContext()}");
                    return Direct3D9Factory.GenericFailureHResult;
                }),
            0,
            value =>
            {
                calls.Add($"Draw:{value == bitmapSource.Pointer}:{renderState.PrefilterEnabled}");
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, "Begin:True:True|Draw:True:False|End:True:True", true, 0, 1, false, false),
            (result, string.Join('|', calls), renderState.PrefilterEnabled, _videoBitmapAddRefCount, _videoBitmapReleaseCount, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenVideoDrawSucceedsAndEndRenderFailsThenDrawSuccessIsPreserved()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        List<string> calls = [];

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device _, out nint value) =>
                {
                    calls.Add("Begin");
                    value = bitmapSource.Pointer;
                    return 0;
                },
                () =>
                {
                    calls.Add("End");
                    return Direct3D9Factory.GenericFailureHResult;
                }),
            0,
            _ =>
            {
                calls.Add("Draw");
                return 0;
            });

        Assert.AreEqual(
            (0, "Begin|Draw|End", true, 1),
            (result, string.Join('|', calls), renderState.PrefilterEnabled, _videoBitmapReleaseCount));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    public unsafe void WhenVideoSurfaceRendererReturnsNoFrameThenEndRenderResultIsIgnoredWithoutDrawing(int endRenderResult)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int endRenderCalls = 0;
        int drawCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                static (Direct3D9Device _, out nint bitmapSource) =>
                {
                    bitmapSource = 0;
                    return 0;
                },
                () =>
                {
                    endRenderCalls++;
                    return endRenderResult;
                }),
            0,
            _ =>
            {
                drawCalls++;
                return 0;
            });

        Assert.AreEqual((0, 1, 0, true), (result, endRenderCalls, drawCalls, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public unsafe void WhenVideoBeginRenderFailsThenEndRenderAndDrawAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int endRenderCalls = 0;
        int drawCalls = 0;

        int result = renderTarget.DrawVideo(
            new Direct3D9VideoRenderState(),
            new Direct3D9VideoSurfaceRenderer(
                static (Direct3D9Device _, out nint bitmapSource) =>
                {
                    bitmapSource = 0;
                    return Direct3D9Factory.GenericFailureHResult;
                },
                () =>
                {
                    endRenderCalls++;
                    return 0;
                }),
            0,
            _ =>
            {
                drawCalls++;
                return 0;
            });

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0, 0), (result, endRenderCalls, drawCalls));
    }

    [TestMethod]
    public unsafe void WhenVideoBeginRenderFailsAfterReturningFrameThenFrameIsReleasedWithoutEndRenderOrDraw()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        int endRenderCalls = 0;
        int drawCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device _, out nint currentBitmapSource) =>
                {
                    currentBitmapSource = bitmapSource.Pointer;
                    return Direct3D9Factory.GenericFailureHResult;
                },
                () =>
                {
                    endRenderCalls++;
                    return 0;
                }),
            0,
            _ =>
            {
                drawCalls++;
                return 0;
            });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, 0, 0, true),
            (result, _videoBitmapReleaseCount, endRenderCalls, drawCalls, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public unsafe void WhenVideoBeginRenderGetsDriverInternalErrorThenMappedResultReleasesFrameWithoutEndRender()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int endRenderCalls = 0;
        int drawCalls = 0;

        int result = renderTarget.DrawVideo(
            new Direct3D9VideoRenderState(),
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device _, out nint currentBitmapSource) =>
                {
                    currentBitmapSource = bitmapSource.Pointer;
                    return Direct3D9Factory.DriverInternalErrorHResult;
                },
                () => ++endRenderCalls),
            0,
            _ => ++drawCalls);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, 0, 0),
            (result, device.UnusableReasonHResult, _videoBitmapReleaseCount, endRenderCalls, drawCalls));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.NonInvertibleMatrixHResult)]
    [DataRow(Direct3D9Factory.BadNumberHResult)]
    public unsafe void WhenVideoBitmapSourceIsSuppliedThenDrawResultAndReferenceLifetimeMatchNativeBehavior(int drawResult)
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        List<string> calls = [];

        int result = renderTarget.DrawVideo(
            renderState,
            null,
            bitmapSource.Pointer,
            value =>
            {
                calls.Add($"Draw:{value == bitmapSource.Pointer}:{renderState.PrefilterEnabled}:{_videoBitmapAddRefCount}:{_videoBitmapReleaseCount}");
                return drawResult;
            });

        Assert.AreEqual(
            (drawResult, "Draw:True:False:1:0", 1, 1, true),
            (result, string.Join('|', calls), _videoBitmapAddRefCount, _videoBitmapReleaseCount, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public unsafe void WhenVideoBitmapSourceIsNullThenDrawAndReferenceOperationsAreSkipped()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        _videoBitmapAddRefCount = 0;
        _videoBitmapReleaseCount = 0;
        int drawCalls = 0;

        int result = renderTarget.DrawVideo(
            renderState,
            null,
            0,
            _ =>
            {
                drawCalls++;
                return Direct3D9Factory.GenericFailureHResult;
            });

        Assert.AreEqual(
            (0, 0, 0, 0, true),
            (result, drawCalls, _videoBitmapAddRefCount, _videoBitmapReleaseCount, renderState.PrefilterEnabled));
    }

    [TestMethod]
    public unsafe void WhenDrawingVideoAfterDisposeThenThrowsBeforeCallbacks()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        int callbackCalls = 0;
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.DrawVideo(
            new Direct3D9VideoRenderState(),
            new Direct3D9VideoSurfaceRenderer(
                (Direct3D9Device _, out nint bitmapSource) =>
                {
                    callbackCalls++;
                    bitmapSource = 0;
                    return 0;
                },
                () => ++callbackCalls),
            0,
            _ => ++callbackCalls));
        Assert.AreEqual(0, callbackCalls);
    }

    [TestMethod]
    public unsafe void WhenDrawingVideoThroughBitmapPipelineThenFrameStateAndFallbackMatchNativeOrder()
    {
        using FakeVideoBitmapSource bitmapSource = new();
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Direct3D9VideoRenderState renderState = new() { PrefilterEnabled = true };
        Matrix4x4 worldToDevice = Matrix4x4.CreateTranslation(3, 4, 0);
        List<string> calls = [];

        int result = renderTarget.DrawVideo(
            renderState,
            null,
            bitmapSource.Pointer,
            new Direct3D9BitmapDrawState(worldToDevice, new Direct3D9PointAndSizeRect(1, 2, 3, 4)),
            19,
            (out nint scratchBrush) =>
            {
                calls.Add($"Brush:{renderState.PrefilterEnabled}:{device.IsEntered()}:{device.IsInUseContext()}");
                scratchBrush = 23;
                return 0;
            },
            (brush, source, transform) => calls.Add($"Set:{brush}:{source == bitmapSource.Pointer}:{transform == worldToDevice}"),
            brush => calls.Add($"Clear:{brush}"),
            _ => new Direct3D9ImmediateBrushRealizer(
                11,
                value => calls.Add($"AddRef:{value}"),
                value => calls.Add($"Release:{value}"),
                static (_, _) => { },
                static _ => false,
                static _ => Direct3D9BrushType.Bitmap,
                static _ => false,
                static _ => false,
                static _ => 0,
                static _ => 0,
                static (_, _, _, _) => 0,
                static (_, _, _, _) => 0,
                static (_, _) => { },
                static (_, _) => { }),
            (MilRectF bounds, out nint shape) =>
            {
                calls.Add($"Shape:{bounds}");
                shape = 29;
                return 0;
            },
            () =>
            {
                calls.Add("Ensure");
                return 0;
            },
            (nint shape, Matrix4x4? transform, MilRectF bounds, out Direct3D9SafeClippedShape clippedShape) =>
            {
                calls.Add($"Clip:{shape}:{transform == worldToDevice}:{bounds}");
                clippedShape = new Direct3D9SafeClippedShape(shape, transform, bounds, false);
                return 0;
            },
            (shape, transform, bounds, brush, world, effects) =>
            {
                calls.Add($"Hardware:{shape}:{transform == worldToDevice}:{bounds}:{brush}:{world == worldToDevice}:{effects}");
                return Direct3D9Factory.NotImplementedHResult;
            },
            (shape, transform, realizer, reason) =>
            {
                calls.Add($"Software:{shape}:{transform == worldToDevice}:{realizer is not null}:{reason}");
                return 0;
            });

        Assert.AreEqual(
            (0, "Brush:False:True:True|Shape:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }|Ensure|Set:23:True:True|AddRef:23|AddRef:19|Clip:29:True:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }|Ensure|Hardware:29:True:MilRectF { Left = 1, Top = 2, Right = 4, Bottom = 6 }:23:True:19|Software:29:True:True:-2147467263|Release:19|Release:23|Clear:23", true, 1, 1, false, false),
            (result, string.Join('|', calls), renderState.PrefilterEnabled, _videoBitmapAddRefCount, _videoBitmapReleaseCount, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsEnabledThenDrawingRunsInNativeDeviceScopesAndPropagatesFailure()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        bool wasEntered = false;
        bool wasInUseContext = false;

        int result = renderTarget.DrawBitmap(() =>
        {
            wasEntered = device.IsEntered();
            wasInUseContext = device.IsInUseContext();
            return Direct3D9Factory.GenericFailureHResult;
        });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, true, false, false),
            (result, wasEntered, wasInUseContext, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenDrawingRejectsUseBeforeInspectingDelegate()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.DrawBitmap(null!));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenSetPositionRejectsUse()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.SetPosition(default));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenUpdatePresentPropertiesRejectsUse()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.UpdatePresentProperties(default, 0, 0));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenScrollBltRejectsUse()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.ScrollBlt(default, default));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenClearRejectsUse()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.Clear(null, null));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenClearSucceedsWithoutDeviceCalls()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateClearDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Bgr32Bpp,
            width: 16,
            height: 16);
        _ = renderTarget.Resize(0, 16);

        int result = renderTarget.Clear(new MilColorF(1f, 1f, 0f, 0f), null);

        Assert.AreEqual(
            (0, 0, false, false),
            (result, calls.Count, renderTarget.HasValidContents, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenBegin3DSucceedsWithoutChanging3DState()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        int internalCallCount = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 16,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                internalCallCount++;
                return new Direct3D9Begin3DResult(Direct3D9Factory.GenericFailureHResult, multisampleType);
            });
        _ = renderTarget.Resize(0, 16);
        Direct3D9SurfaceRect boundsBeforeBegin = renderTarget.Bounds;

        int result = renderTarget.Begin3D(
            new MilRectF(2, 3, 10, 11),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 0.5f);

        Assert.AreEqual(
            (0, 0, false, boundsBeforeBegin, false),
            (result, internalCallCount, renderTarget.In3D, renderTarget.Bounds, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledDuring3DThenEnd3DSucceedsWithoutEnding3D()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 16,
            begin3DInternal: (_, _, _, multisampleType) => new Direct3D9Begin3DResult(0, multisampleType));
        _ = renderTarget.Begin3D(
            new MilRectF(2, 3, 10, 11),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);
        Direct3D9SurfaceRect boundsIn3D = renderTarget.Bounds;
        _ = renderTarget.Resize(0, 16);

        int result = renderTarget.End3D();

        Assert.AreEqual(
            (0, true, boundsIn3D, false),
            (result, renderTarget.In3D, renderTarget.Bounds, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenClearRenderTargetIsInvalidThenDeviceCallsAreSkippedAndDisplayContentsBecomeValid()
    {
        List<string> calls = [];
        Direct3D9SwapChain? swapChain = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            },
            setRenderTarget: _ =>
            {
                calls.Add("RenderTarget");
                return 0;
            },
            setViewport: _ =>
            {
                calls.Add("Viewport");
                return 0;
            },
            setScissorRect: _ =>
            {
                calls.Add("Scissor");
                return 0;
            },
            clear: (_, _, _, _, _) =>
            {
                calls.Add("Clear");
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        swapChain!.Dispose();

        int result = renderTarget.Clear(new MilColorF(1f, 1f, 0f, 0f), null);

        Assert.AreEqual((0, 0, true, false), (result, calls.Count, renderTarget.HasValidContents, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenClearingInvalidDirectSurfaceThenDeviceCallsAreSkippedInsideDeviceScope()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateClearDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 12);
        surface.Dispose();

        int result = renderTarget.Clear(new MilColorF(1f, 1f, 0f, 0f), null);

        Assert.AreEqual(
            (0, 0, true, false),
            (result, calls.Count, renderTarget.HasValidContents, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenClearColorIsNullThenNoDeviceCallsAreMade()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateClearDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Bgr32Bpp,
            width: 16,
            height: 16);

        int result = renderTarget.Clear(null, null);

        Assert.AreEqual((0, 0), (result, calls.Count));
    }

    [TestMethod]
    public unsafe void WhenClearClipDoesNotIntersectSurfaceThenNoDeviceCallsAreMade()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateClearDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Bgr32Bpp,
            width: 16,
            height: 16);

        int result = renderTarget.Clear(new MilColorF(1f, 1f, 0f, 0f), new Direct3D9SurfaceRect(16, 0, 32, 16));

        Assert.AreEqual((0, 0), (result, calls.Count));
    }

    [TestMethod]
    public unsafe void WhenClearClipIsNullThenCurrentRenderTargetBoundsLimitClear()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateClearDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Bgr32Bpp,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(2, 3, 11, 13));

        int result = renderTarget.Clear(new MilColorF(1f, 1f, 0f, 0f), null);

        Assert.AreEqual(
            "0|Description|RenderTarget|Viewport|Matrix|Scissor:2,3,9,10|Clear:0,1,FFFF0000,0,0",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenClearingNonPremultipliedTargetThenCallsRunWhileDeviceIsEntered()
    {
        List<string> calls = [];
        bool deviceWasEnteredDuringCall = true;
        Direct3D9Device? clearingDevice = null;
        using Direct3D9Device device = CreateClearDevice(
            calls,
            observeDeviceCall: () => deviceWasEnteredDuringCall &= clearingDevice?.IsEntered() == true);
        clearingDevice = device;
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Bgr32Bpp,
            width: 16,
            height: 16);

        int result = renderTarget.Clear(new MilColorF(0.5f, 1f, 0f, 0f), new Direct3D9SurfaceRect(-4, 2, 8, 20));

        Assert.AreEqual(
            ("0|Description|RenderTarget|Viewport|Matrix|Scissor:0,2,8,14|Clear:0,1,80FF0000,0,0", true, true, false),
            ($"{result}|{string.Join('|', calls)}", renderTarget.HasValidContents, deviceWasEnteredDuringCall, device.IsEntered()));
    }

    [DataTestMethod]
    [DataRow((int) MilPixelFormat.Pbgra32Bpp)]
    [DataRow((int) MilPixelFormat.Prgba64Bpp)]
    [DataRow((int) MilPixelFormat.Prgba128BppFloat)]
    public unsafe void WhenClearingPremultipliedTargetThenSrgbChannelsArePremultiplied(int pixelFormat)
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateClearDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: (MilPixelFormat) pixelFormat,
            width: 16,
            height: 16);

        int result = renderTarget.Clear(new MilColorF(0.5f, 1f, 0f, 0f), null);

        Assert.AreEqual("0|Clear:0,1,80800000,0,0", $"{result}|{calls[^1]}");
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    public unsafe void WhenClearDeviceStepFailsThenFirstFailureIsReturnedAndLaterCallsAreSkipped(int failAtStep)
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateClearDevice(calls, failAtStep);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Bgr32Bpp,
            width: 16,
            height: 16);

        int result = renderTarget.Clear(new MilColorF(1f, 1f, 0f, 0f), null);

        string[] terminalCalls = ["RenderTarget", "Scissor", "Clear:0,1,FFFF0000,0,0"];
        Assert.AreEqual(
            ($"{Direct3D9Factory.GenericFailureHResult}|{terminalCalls[failAtStep]}", false, false),
            ($"{result}|{calls[^1]}", renderTarget.HasValidContents, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenBeginningAliased3DThenBoundsUseRasterizerRoundingAndNoMultisampling()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRect receivedBounds = default;
        float receivedZ = 0;
        bool receivedUseZBuffer = false;
        MultisampleType receivedMultisampleType = MultisampleType.Multisample16Samples;
        bool deviceWasEntered = false;
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            width: 20,
            height: 20,
            begin3DInternal: (bounds, z, useZBuffer, multisampleType) =>
            {
                receivedBounds = bounds;
                receivedZ = z;
                receivedUseZBuffer = useZBuffer;
                receivedMultisampleType = multisampleType;
                deviceWasEntered = device.IsEntered();
                return new Direct3D9Begin3DResult(0, multisampleType);
            },
            initialBounds: new Direct3D9SurfaceRect(2, 3, 18, 19));

        int result = renderTarget.Begin3D(
            new MilRectF(2.49f, 3.5f, 17.51f, 18.49f),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 0.75f);

        Assert.AreEqual(
            (0, new Direct3D9SurfaceRect(2, 3, 17, 18), 0.75f, true, MultisampleType.MultisampleNone, true, true, false),
            (result, receivedBounds, receivedZ, receivedUseZBuffer, receivedMultisampleType, renderTarget.In3D, deviceWasEntered, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenBeginningAntialiased3DThenBoundsUseFloorCeilingAndSupportedMultisampling()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        EnableFourSampleMultisampling(device);
        Direct3D9SurfaceRect receivedBounds = default;
        MultisampleType receivedMultisampleType = MultisampleType.MultisampleNone;
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            width: 20,
            height: 20,
            begin3DInternal: (bounds, _, _, multisampleType) =>
            {
                receivedBounds = bounds;
                receivedMultisampleType = multisampleType;
                return new Direct3D9Begin3DResult(0, multisampleType);
            },
            initialBounds: new Direct3D9SurfaceRect(2, 3, 18, 19));

        int result = renderTarget.Begin3D(
            new MilRectF(2.49f, 3.5f, 17.01f, 18.01f),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (0, new Direct3D9SurfaceRect(2, 3, 18, 19), MultisampleType.Multisample4Samples),
            (result, receivedBounds, receivedMultisampleType));
    }

    [TestMethod]
    public unsafe void WhenBeginAndEnd3DSucceedThenDisplayContentsRemainInvalidAndDirtyRegionIsPreserved()
    {
        Direct3D9PresentRequest? presentRequest = null;
        using Direct3D9Device device = CreatePresentDevice(request =>
        {
            presentRequest = request;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            swapEffect: Swapeffect.Copy,
            begin3DInternal: (_, _, _, multisampleType) => new Direct3D9Begin3DResult(0, multisampleType));
        _ = renderTarget.Resize(16, 12);
        Direct3D9SurfaceRect dirtyRect = new(2, 3, 8, 9);
        _ = renderTarget.InvalidateRect(dirtyRect);

        int beginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 10, 11),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);
        int endResult = renderTarget.End3D();
        int presentResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual(
            (0, 0, 0, false, dirtyRect, dirtyRect, null),
            (beginResult, endResult, presentResult, renderTarget.HasValidContents, presentRequest?.SourceRect ?? default, presentRequest?.DestinationRect ?? default, presentRequest?.DirtyRegion));
    }

    [TestMethod]
    public unsafe void WhenBegin3DFailsThenDisplayContentsRemainInvalidAndDirtyRegionIsPreserved()
    {
        int presentCalls = 0;
        Direct3D9PresentRequest? presentRequest = null;
        using Direct3D9Device device = CreatePresentDevice(request =>
        {
            presentCalls++;
            presentRequest = request;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            swapEffect: Swapeffect.Copy,
            begin3DInternal: (_, _, _, multisampleType) =>
                new Direct3D9Begin3DResult(Direct3D9Factory.GenericFailureHResult, multisampleType));
        _ = renderTarget.Resize(16, 12);
        Direct3D9SurfaceRect dirtyRect = new(2, 3, 8, 9);
        _ = renderTarget.InvalidateRect(dirtyRect);

        int beginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 10, 11),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);
        int presentResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, false, false, 1, dirtyRect, dirtyRect, null),
            (beginResult, presentResult, renderTarget.In3D, renderTarget.HasValidContents, presentCalls, presentRequest?.SourceRect ?? default, presentRequest?.DestinationRect ?? default, presentRequest?.DirtyRegion));
    }

    [TestMethod]
    public unsafe void WhenBegin3DDirectSurfaceIsInvalidThenInternalPathIsSkippedAndEmpty3DContextIsEntered()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 16);
        surface.Dispose();

        int beginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 10, 11),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);
        Direct3D9SurfaceRect boundsDuring3D = renderTarget.Bounds;
        bool in3DDuring3D = renderTarget.In3D;
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, default(Direct3D9SurfaceRect), true, 0, false, new Direct3D9SurfaceRect(0, 0, 16, 16), false),
            (beginResult, boundsDuring3D, in3DDuring3D, endResult, renderTarget.In3D, renderTarget.Bounds, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenBegin3DIsCalledInside3DOnInvalidDirectSurfaceThenInvalidCallIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        int internalCallCount = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 16,
            begin3DInternal: (bounds, _, _, multisampleType) =>
            {
                internalCallCount++;
                return new Direct3D9Begin3DResult(0, multisampleType);
            });
        int firstResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 10, 11),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);
        Direct3D9SurfaceRect boundsDuring3D = renderTarget.Bounds;
        surface.Dispose();

        int secondResult = renderTarget.Begin3D(
            new MilRectF(3, 4, 12, 13),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 0.5f);

        Assert.AreEqual(
            (0, Direct3D9Factory.WgxInvalidCallHResult, 1, true, new Direct3D9SurfaceRect(1, 2, 10, 11), false),
            (firstResult, secondResult, internalCallCount, renderTarget.In3D, boundsDuring3D, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenBegin3DBoundsAreEmptyThenInternalPathIsSkippedAnd3DIsEntered()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        int internalCallCount = 0;
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 16,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                internalCallCount++;
                return new Direct3D9Begin3DResult(0, multisampleType);
            });

        int result = renderTarget.Begin3D(
            new MilRectF(20, 20, 30, 30),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (0, 0, default(Direct3D9SurfaceRect), true),
            (result, internalCallCount, renderTarget.Bounds, renderTarget.In3D));
    }

    [TestMethod]
    public unsafe void WhenBegin3DBoundsAreEmptyOnInvalidDirectSurfaceThenValidityCheckAndDeviceEntryAreSkipped()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        int internalCallCount = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 16,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                internalCallCount++;
                return new Direct3D9Begin3DResult(Direct3D9Factory.GenericFailureHResult, multisampleType);
            });
        surface.Dispose();

        int beginResult = renderTarget.Begin3D(
            new MilRectF(20, 20, 30, 30),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 0.5f);
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, 0, false, new Direct3D9SurfaceRect(0, 0, 16, 16), false, false),
            (beginResult, endResult, internalCallCount, renderTarget.In3D, renderTarget.Bounds, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenBegin3DBoundsAreEmptyThenMultisampleFailureStateIsNotChanged()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        EnableFourSampleMultisampling(device);
        int multisampleFailureCount = 0;
        int internalCallCount = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 16,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                internalCallCount++;
                return new Direct3D9Begin3DResult(0, MultisampleType.MultisampleNone);
            },
            setMultisampleFailed: () => multisampleFailureCount++);

        int beginResult = renderTarget.Begin3D(
            new MilRectF(20, 20, 30, 30),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, 0, 0, true, false, new Direct3D9SurfaceRect(0, 0, 16, 16)),
            (beginResult, endResult, internalCallCount, multisampleFailureCount, device.ShouldAttemptMultisample, renderTarget.In3D, renderTarget.Bounds));
    }

    [TestMethod]
    public unsafe void WhenDirectSurfaceBecomesInvalidDuringEmpty3DThenEnd3DRestoresStateWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 16);

        int beginResult = renderTarget.Begin3D(
            new MilRectF(20, 20, 30, 30),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        surface.Dispose();

        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, false, new Direct3D9SurfaceRect(0, 0, 16, 16), false, false),
            (beginResult, endResult, renderTarget.In3D, renderTarget.Bounds, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenBegin3DInternalFailsThenOriginalBoundsAreRestoredAnd3DIsNotEntered()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRect originalBounds = new(2, 3, 18, 19);
        bool deviceWasEntered = false;
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 20,
            height: 20,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                deviceWasEntered = device.IsEntered();
                return new Direct3D9Begin3DResult(Direct3D9Factory.GenericFailureHResult, multisampleType);
            },
            initialBounds: originalBounds);

        int result = renderTarget.Begin3D(
            new MilRectF(5, 6, 10, 11),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 0.5f);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, originalBounds, false, true, false),
            (result, renderTarget.Bounds, renderTarget.In3D, deviceWasEntered, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenBegin3DSucceedsWithReducedMultisamplingThenFutureAttemptsAreDisabled()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        EnableFourSampleMultisampling(device);
        int multisampleFailureCount = 0;
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            pixelFormat: MilPixelFormat.Bgr32Bpp,
            width: 16,
            height: 16,
            begin3DInternal: (_, _, _, _) =>
                new Direct3D9Begin3DResult(0, MultisampleType.MultisampleNone),
            setMultisampleFailed: () => multisampleFailureCount++);

        int result = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual((0, 1, false), (result, multisampleFailureCount, device.ShouldAttemptMultisample));
    }

    [TestMethod]
    public unsafe void WhenBegin3DFailsWithReducedMultisamplingThenFailureMarkerIsNotSet()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        EnableFourSampleMultisampling(device);
        int multisampleFailureCount = 0;
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            pixelFormat: MilPixelFormat.Bgr32Bpp,
            width: 16,
            height: 16,
            begin3DInternal: (_, _, _, _) =>
                new Direct3D9Begin3DResult(Direct3D9Factory.OutOfVideoMemoryHResult, MultisampleType.MultisampleNone),
            setMultisampleFailed: () => multisampleFailureCount++);

        int result = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, 0, true),
            (result, multisampleFailureCount, device.ShouldAttemptMultisample));
    }

    [TestMethod]
    public unsafe void WhenBeginning3DTwiceThenSecondCallReturnsWgxInvalidCall()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        int internalCallCount = 0;
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 16,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                internalCallCount++;
                return new Direct3D9Begin3DResult(0, multisampleType);
            });
        _ = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.None, false, 1f);

        int result = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.None, false, 1f);

        Assert.AreEqual((Direct3D9Factory.WgxInvalidCallHResult, 1), (result, internalCallCount));
    }

    [TestMethod]
    public unsafe void WhenBegin3DBoundsContainNaNThenInternalPathIsSkipped()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        int internalCallCount = 0;
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 16,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                internalCallCount++;
                return new Direct3D9Begin3DResult(0, multisampleType);
            });

        int result = renderTarget.Begin3D(
            new MilRectF(float.NaN, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual((0, 0, default(Direct3D9SurfaceRect)), (result, internalCallCount, renderTarget.Bounds));
    }

    [TestMethod]
    public unsafe void WhenBegin3DBoundsAreInfiniteThenBoundsAreClippedToTheSurface()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRect receivedBounds = default;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 12,
            begin3DInternal: (bounds, _, _, multisampleType) =>
            {
                receivedBounds = bounds;
                return new Direct3D9Begin3DResult(0, multisampleType);
            });

        int result = renderTarget.Begin3D(
            new MilRectF(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual((0, new Direct3D9SurfaceRect(0, 0, 16, 12)), (result, receivedBounds));
    }

    [TestMethod]
    public unsafe void WhenBegin3DBoundsStartAtPositiveInfinityThenInternalPathIsSkipped()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        int internalCallCount = 0;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 12,
            begin3DInternal: (_, _, _, multisampleType) =>
            {
                internalCallCount++;
                return new Direct3D9Begin3DResult(0, multisampleType);
            });

        int result = renderTarget.Begin3D(
            new MilRectF(float.PositiveInfinity, 0, float.PositiveInfinity, 12),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual((0, 0, default(Direct3D9SurfaceRect)), (result, internalCallCount, renderTarget.Bounds));
    }

    [DataTestMethod]
    [DataRow((int) MilAntiAliasMode.None, 2, 3, 9, 10)]
    [DataRow((int) MilAntiAliasMode.EightByEight, 1, 2, 10, 11)]
    public unsafe void WhenBegin3DBoundsAreFractionalThenRasterizerRoundingFollowsAntiAliasMode(
        int antiAliasMode,
        int expectedLeft,
        int expectedTop,
        int expectedRight,
        int expectedBottom)
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRect receivedBounds = default;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 16,
            height: 16,
            begin3DInternal: (bounds, _, _, multisampleType) =>
            {
                receivedBounds = bounds;
                return new Direct3D9Begin3DResult(0, multisampleType);
            });

        int result = renderTarget.Begin3D(
            new MilRectF(1.75f, 2.75f, 9.25f, 10.25f),
            (MilAntiAliasMode) antiAliasMode,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (0, new Direct3D9SurfaceRect(expectedLeft, expectedTop, expectedRight, expectedBottom)),
            (result, receivedBounds));
    }

    [TestMethod]
    public unsafe void WhenDefaultBegin3DDoesNotUseDepthThenTargetAndClipFollowOriginalOrder()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 16);

        int result = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 0.75f);

        Assert.AreEqual(
            "0|Description:1|Description:1|RenderTarget:1|Viewport|Matrix|Scissor",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenDefaultBegin3DUsesDepthThenDepthIsCreatedBoundAndCleared()
    {
        List<string> calls = [];
        using Direct3D9Surface depthSurface = CreateSurface();
        using Direct3D9Device device = CreateBegin3DDevice(calls, depthSurface);
        using Direct3D9Surface renderSurface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);

        int result = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 0.25f);

        Assert.AreEqual(
            "0|Description|Description|RenderTarget|Viewport|Matrix|Scissor|Description|CreateDepth:16,16,MultisampleNone|DepthStencil|Clear:0,2,00000000,0.25,0",
            $"{result}|{string.Join('|', calls)}");
    }

    [DataTestMethod]
    [DataRow(15u, 16u, MultisampleType.MultisampleNone)]
    [DataRow(16u, 16u, MultisampleType.Multisample2Samples)]
    public unsafe void WhenExistingDepthSurfaceIsInsufficientThenItIsReleasedAndReplaced(
        uint existingWidth,
        uint existingHeight,
        MultisampleType existingMultisampleType)
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? existingDepthSurface = null;
        Direct3D9Surface? replacementDepthSurface = null;
        bool useReplacementDescription = false;
        int createCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { RasterCaps = (uint) D3D9.PrastercapsScissortest },
            getRenderTargetDescription: surface =>
            {
                if (!ReferenceEquals(surface, renderSurface))
                {
                    return new SurfaceDesc(
                        width: existingWidth,
                        height: existingHeight,
                        multiSampleType: existingMultisampleType);
                }

                return useReplacementDescription
                    ? new SurfaceDesc(width: 16, height: 16, multiSampleType: MultisampleType.Multisample4Samples)
                    : new SurfaceDesc(
                        width: existingWidth,
                        height: existingHeight,
                        multiSampleType: existingMultisampleType);
            },
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setScissorRect: _ => 0,
            setSurfaceToClippingMatrix: _ => 0,
            createDepthBuffer: (uint width, uint height, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                calls.Add($"Create:{width},{height},{multisampleType}");
                surface = ++createCount == 1
                    ? existingDepthSurface = CreateSurfaceWithNativePointer()
                    : replacementDepthSurface = CreateSurfaceWithNativePointer();
                return 0;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "Depth:None" : $"Depth:{(createCount == 1 ? "Existing" : "Replacement")}");
                return 0;
            },
            setRenderState: (_, _) => 0,
            clear: (_, _, _, _, _) => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);

        int firstBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 1f);
        int firstEndResult = renderTarget.End3D();
        useReplacementDescription = true;
        int secondBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 1f);
        bool existingWasReleased = existingDepthSurface!.IsReleased;
        int secondEndResult = renderTarget.End3D();
        renderTarget.Dispose();

        Assert.AreEqual(
            (0, 0, 0, 0, true, true, false,
                $"Create:{existingWidth},{existingHeight},{existingMultisampleType}|Depth:Existing|Depth:None|Create:16,16,Multisample4Samples|Depth:Replacement|Depth:None"),
            (firstBeginResult, firstEndResult, secondBeginResult, secondEndResult,
                existingWasReleased, replacementDepthSurface!.IsReleased, renderTarget.In3D, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenCachedDepthSurfaceIsInvalidThenDescriptionIsSkippedAndSurfaceIsReplaced()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? cachedDepthSurface = null;
        Direct3D9Surface replacementDepthSurface = CreateSurfaceWithNativePointer();
        int createCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { RasterCaps = (uint) D3D9.PrastercapsScissortest },
            getRenderTargetDescription: surface =>
            {
                calls.Add(ReferenceEquals(surface, renderSurface) ? "Description:RenderTarget" : "Description:Depth");
                return new SurfaceDesc(width: 16, height: 16);
            },
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setScissorRect: _ => 0,
            setSurfaceToClippingMatrix: _ => 0,
            createDepthBuffer: (uint _, uint _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                surface = ++createCount == 1
                    ? cachedDepthSurface = CreateSurfaceWithNativePointer()
                    : replacementDepthSurface;
                calls.Add(createCount == 1 ? "Create:Cached" : "Create:Replacement");
                return 0;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "Depth:None" : createCount == 1 ? "Depth:Cached" : "Depth:Replacement");
                return 0;
            },
            clear: (_, _, _, _, _) => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);

        int firstBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 1f);
        int firstEndResult = renderTarget.End3D();
        cachedDepthSurface!.Dispose();
        calls.Clear();

        int secondBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 1f);
        int secondEndResult = renderTarget.End3D();
        renderTarget.Dispose();

        Assert.AreEqual(
            (0, 0, 0, 0, true, true, "Description:RenderTarget|Description:RenderTarget|Description:RenderTarget|Create:Replacement|Depth:Replacement|Depth:None"),
            (firstBeginResult, firstEndResult, secondBeginResult, secondEndResult,
                cachedDepthSurface.IsReleased, replacementDepthSurface.IsReleased, string.Join('|', calls)));
    }

    [DataTestMethod]
    [DataRow(15u, 16u, MultisampleType.MultisampleNone)]
    [DataRow(16u, 16u, MultisampleType.Multisample2Samples)]
    public unsafe void WhenExistingDepthSurfaceIsInsufficientAndReplacementFailsThenOldSurfaceIsReleasedAndFailurePropagates(
        uint existingWidth,
        uint existingHeight,
        MultisampleType existingMultisampleType)
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? existingDepthSurface = null;
        bool useReplacementDescription = false;
        int createCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { RasterCaps = (uint) D3D9.PrastercapsScissortest },
            getRenderTargetDescription: surface =>
            {
                if (!ReferenceEquals(surface, renderSurface))
                {
                    return new SurfaceDesc(
                        width: existingWidth,
                        height: existingHeight,
                        multiSampleType: existingMultisampleType);
                }

                return useReplacementDescription
                    ? new SurfaceDesc(width: 16, height: 16, multiSampleType: MultisampleType.Multisample4Samples)
                    : new SurfaceDesc(
                        width: existingWidth,
                        height: existingHeight,
                        multiSampleType: existingMultisampleType);
            },
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setScissorRect: _ => 0,
            setSurfaceToClippingMatrix: _ => 0,
            createDepthBuffer: (uint width, uint height, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                calls.Add($"Create:{width},{height},{multisampleType}");
                if (++createCount == 1)
                {
                    surface = existingDepthSurface = CreateSurfaceWithNativePointer();
                    return 0;
                }

                surface = null;
                return Direct3D9Factory.GenericFailureHResult;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "Depth:None" : "Depth:Existing");
                return 0;
            },
            setRenderState: (_, _) => 0,
            clear: (_, _, _, _, _) =>
            {
                calls.Add("Clear");
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);

        int firstBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 1f);
        int firstEndResult = renderTarget.End3D();
        useReplacementDescription = true;
        int secondBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 1f);

        Assert.AreEqual(
            (0, 0, Direct3D9Factory.GenericFailureHResult, true, false,
                $"Create:{existingWidth},{existingHeight},{existingMultisampleType}|Depth:Existing|Clear|Depth:None|Create:16,16,Multisample4Samples"),
            (firstBeginResult, firstEndResult, secondBeginResult, existingDepthSurface!.IsReleased,
                renderTarget.In3D, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenDedicatedThreeDimensionalSurfaceExistsThenMultisampleIntermediateIsStillCreated()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        using Direct3D9Surface dedicatedSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                calls.Add("CreateTarget");
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            getRenderTargetDescription: surface =>
            {
                if (ReferenceEquals(surface, renderSurface))
                {
                    return new SurfaceDesc(
                        format: Format.A8R8G8B8,
                        width: 16,
                        height: 16,
                        multiSampleType: MultisampleType.MultisampleNone);
                }

                if (ReferenceEquals(surface, dedicatedSurface))
                {
                    calls.Add("DescribeDedicated");
                }

                return new SurfaceDesc(
                    format: Format.A8R8G8B8,
                    width: 16,
                    height: 16,
                    multiSampleType: MultisampleType.Multisample4Samples);
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            renderTargetSurfaceFor3D: dedicatedSurface,
            width: 16,
            height: 16);

        int result = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (0, false, true, false),
            (result, calls.Contains("DescribeDedicated"), calls.Contains("CreateTarget"), dedicatedSurface.IsReleased));
    }

    [TestMethod]
    public unsafe void WhenMultisampleTargetIsRequiredThenIntermediateIsCreatedBoundAndPopulated()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                calls.Add($"CreateTarget:{multisampleType}");
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);

        int result = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            "0|Description:Default|CreateTarget:Multisample4Samples|Description:Intermediate|RenderTarget:Intermediate|Viewport|Matrix|Scissor|Stretch:Default:1,2,9,10:Intermediate:1,2,9,10",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenPopulatingMultisampleTargetRunsOutOfVideoMemoryThenFailurePropagatesWithoutDowngrade()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        int multisampleFailureCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            stretchRect: (_, _, _, _) => Direct3D9Factory.OutOfVideoMemoryHResult);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            setMultisampleFailed: () => multisampleFailureCount++);

        int result = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            $"{Direct3D9Factory.OutOfVideoMemoryHResult}|0|False|False|Description:Default|Description:Intermediate|RenderTarget:Intermediate|Viewport|Matrix|Scissor|Stretch:Default:1,2,9,10:Intermediate:1,2,9,10",
            $"{result}|{multisampleFailureCount}|{intermediateSurface!.IsReleased}|{renderTarget.In3D}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenPopulatingMultisampleTargetFailsThenNextBegin3DReusesIntermediateUntilRenderTargetIsReleased()
    {
        List<string> calls = [];
        Queue<int> stretchResults = new([Direct3D9Factory.GenericFailureHResult, 0, 0]);
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        int createCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                createCount++;
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            stretchRect: (_, _, _, _) => stretchResults.Dequeue());
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);

        int firstBeginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        bool releasedAfterFailure = intermediateSurface!.IsReleased;
        int secondBeginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        int endResult = renderTarget.End3D();
        renderTarget.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, 0, 0, 1, 3, true, false),
            (firstBeginResult, releasedAfterFailure, secondBeginResult, endResult, createCount, 3 - stretchResults.Count, intermediateSurface.IsReleased, renderTarget.In3D));
    }

    [DataTestMethod]
    [DataRow(15u, 16u, MultisampleType.Multisample4Samples)]
    [DataRow(16u, 16u, MultisampleType.Multisample2Samples)]
    public unsafe void WhenExistingMultisampleTargetIsInsufficientAndReplacementFailsThenOldTargetIsReleasedAndFailurePropagates(
        uint existingWidth,
        uint existingHeight,
        MultisampleType existingMultisampleType)
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? firstIntermediateSurface = null;
        int createCount = 0;
        int intermediateDescriptionCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                createCount++;
                if (createCount == 1)
                {
                    firstIntermediateSurface = CreateSurface();
                    surface = firstIntermediateSurface;
                    return 0;
                }

                surface = null;
                return Direct3D9Factory.GenericFailureHResult;
            },
            getRenderTargetDescription: surface =>
            {
                if (ReferenceEquals(surface, renderSurface))
                {
                    return new SurfaceDesc(
                        format: Format.A8R8G8B8,
                        width: 16,
                        height: 16,
                        multiSampleType: MultisampleType.MultisampleNone);
                }

                intermediateDescriptionCount++;
                return new SurfaceDesc(
                    format: Format.A8R8G8B8,
                    width: intermediateDescriptionCount == 1 ? 16u : existingWidth,
                    height: intermediateDescriptionCount == 1 ? 16u : existingHeight,
                    multiSampleType: intermediateDescriptionCount == 1
                        ? MultisampleType.Multisample4Samples
                        : existingMultisampleType);
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);

        int firstBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        int endResult = renderTarget.End3D();
        int secondBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            (0, 0, Direct3D9Factory.GenericFailureHResult, 2, true, false),
            (firstBeginResult, endResult, secondBeginResult, createCount, firstIntermediateSurface!.IsReleased, renderTarget.In3D));
    }

    [DataTestMethod]
    [DataRow(15u, 16u, MultisampleType.Multisample4Samples)]
    [DataRow(16u, 16u, MultisampleType.Multisample2Samples)]
    public unsafe void WhenExistingMultisampleTargetIsInsufficientThenReplacementIsUsedForPopulateAndCopyBack(
        uint existingWidth,
        uint existingHeight,
        MultisampleType existingMultisampleType)
    {
        List<string> stretches = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? firstIntermediateSurface = null;
        Direct3D9Surface? replacementIntermediateSurface = null;
        int createCount = 0;
        int intermediateDescriptionCount = 0;
        bool firstReleasedBeforeReplacement = false;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            [],
            renderSurface,
            createRenderTarget: (uint width, uint height, Format format, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                createCount++;
                if (createCount == 1)
                {
                    firstIntermediateSurface = CreateSurface();
                    surface = firstIntermediateSurface;
                    return 0;
                }

                firstReleasedBeforeReplacement = firstIntermediateSurface!.IsReleased;
                replacementIntermediateSurface = CreateSurface();
                surface = replacementIntermediateSurface;
                Assert.AreEqual((16u, 16u, Format.A8R8G8B8, MultisampleType.Multisample4Samples), (width, height, format, multisampleType));
                return 0;
            },
            stretchRect: (source, _, destination, _) =>
            {
                string sourceName = ReferenceEquals(source, renderSurface)
                    ? "Default"
                    : ReferenceEquals(source, firstIntermediateSurface) ? "First" : "Replacement";
                string destinationName = ReferenceEquals(destination, renderSurface)
                    ? "Default"
                    : ReferenceEquals(destination, firstIntermediateSurface) ? "First" : "Replacement";
                stretches.Add($"{sourceName}->{destinationName}");
                return 0;
            },
            getRenderTargetDescription: surface =>
            {
                if (ReferenceEquals(surface, renderSurface))
                {
                    return new SurfaceDesc(
                        format: Format.A8R8G8B8,
                        width: 16,
                        height: 16,
                        multiSampleType: MultisampleType.MultisampleNone);
                }

                intermediateDescriptionCount++;
                return new SurfaceDesc(
                    format: Format.A8R8G8B8,
                    width: intermediateDescriptionCount == 1 ? 16u : existingWidth,
                    height: intermediateDescriptionCount == 1 ? 16u : existingHeight,
                    multiSampleType: intermediateDescriptionCount == 1
                        ? MultisampleType.Multisample4Samples
                        : existingMultisampleType);
            });
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);

        int firstBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        int firstEndResult = renderTarget.End3D();
        int secondBeginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        int secondEndResult = renderTarget.End3D();
        renderTarget.Dispose();

        Assert.AreEqual(
            (0, 0, 0, 0, 2, true, true, true, "Default->First|First->Default|Default->Replacement|Replacement->Default", false),
            (firstBeginResult, firstEndResult, secondBeginResult, secondEndResult, createCount, firstReleasedBeforeReplacement,
                firstIntermediateSurface!.IsReleased, replacementIntermediateSurface!.IsReleased, string.Join('|', stretches), renderTarget.In3D));
    }

    [TestMethod]
    public unsafe void WhenMultisampleAllocationRunsOutOfVideoMemoryThenBegin3DRetriesWithoutMultisampling()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        int multisampleFailureCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                calls.Add($"CreateTarget:{multisampleType}");
                surface = null;
                return Direct3D9Factory.OutOfVideoMemoryHResult;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            setMultisampleFailed: () => multisampleFailureCount++);

        int result = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        Assert.AreEqual(
            "0|1|Description:Default|CreateTarget:Multisample4Samples|Description:Default|Description:Default|RenderTarget:Default|Viewport|Matrix|Scissor",
            $"{result}|{multisampleFailureCount}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenBindingMultisampleTargetRunsOutOfVideoMemoryThenIntermediateIsReleasedBeforeRetry()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        int multisampleFailureCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            setRenderTarget: surface => ReferenceEquals(surface, renderSurface)
                ? 0
                : Direct3D9Factory.OutOfVideoMemoryHResult);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            setMultisampleFailed: () => multisampleFailureCount++);

        int beginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            $"0|0|1|True|False|Description:Default|Description:Intermediate|RenderTarget:Intermediate|Description:Default|Description:Default|RenderTarget:Default|Viewport|Matrix|Scissor",
            $"{beginResult}|{endResult}|{multisampleFailureCount}|{intermediateSurface!.IsReleased}|{renderTarget.In3D}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenClearingMultisampleDepthRunsOutOfVideoMemoryThenDepthAndIntermediateAreRecreatedForRetry()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        Direct3D9Surface? multisampleDepthSurface = null;
        Direct3D9Surface? defaultDepthSurface = null;
        int clearCount = 0;
        int depthSetCount = 0;
        int multisampleFailureCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            createDepthBuffer: (uint _, uint _, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                calls.Add($"CreateDepth:{multisampleType}");
                surface = multisampleType == MultisampleType.MultisampleNone
                    ? defaultDepthSurface = CreateSurfaceWithNativePointer()
                    : multisampleDepthSurface = CreateSurfaceWithNativePointer();
                return 0;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "DepthStencil:None" : "DepthStencil:Set");
                if (surface is not null)
                {
                    depthSetCount++;
                }

                return 0;
            },
            clear: (_, _, _, _, _) =>
            {
                calls.Add("Clear");
                return ++clearCount == 1 ? Direct3D9Factory.OutOfVideoMemoryHResult : 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            setMultisampleFailed: () => multisampleFailureCount++);

        int beginResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 0.5f);

        Assert.AreEqual(
            $"0|1|2|True|True|False|Description:Default|Description:Intermediate|RenderTarget:Intermediate|Viewport|Matrix|Scissor|Description:Intermediate|CreateDepth:Multisample4Samples|DepthStencil:Set|Clear|Description:Default|Description:Default|RenderTarget:Default|Viewport|Matrix|Scissor|Description:Default|Description:Intermediate|DepthStencil:None|CreateDepth:MultisampleNone|DepthStencil:Set|Clear",
            $"{beginResult}|{multisampleFailureCount}|{depthSetCount}|{intermediateSurface!.IsReleased}|{multisampleDepthSurface!.IsReleased}|{defaultDepthSurface!.IsReleased}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenCreatingMultisampleDepthRunsOutOfVideoMemoryThenDepthStateIsClearedAndBegin3DRetriesWithoutMultisampling()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        Direct3D9Surface? defaultDepthSurface = null;
        int multisampleFailureCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            createDepthBuffer: (uint _, uint _, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                calls.Add($"CreateDepth:{multisampleType}");
                if (multisampleType != MultisampleType.MultisampleNone)
                {
                    surface = null;
                    return Direct3D9Factory.OutOfVideoMemoryHResult;
                }

                defaultDepthSurface = CreateSurfaceWithNativePointer();
                surface = defaultDepthSurface;
                return 0;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "DepthStencil:None" : "DepthStencil:Set");
                return 0;
            },
            clear: (_, _, _, _, _) => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            setMultisampleFailed: () => multisampleFailureCount++);

        int result = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 0.5f);

        Assert.AreEqual(
            (0, 1, true, false, 1, 1),
            (result, multisampleFailureCount, intermediateSurface!.IsReleased,
                defaultDepthSurface!.IsReleased,
                calls.Count(call => call == "DepthStencil:None"),
                calls.Count(call => call == "DepthStencil:Set")));
    }

    [TestMethod]
    public unsafe void WhenBindingMultisampleDepthRunsOutOfVideoMemoryThenCreatedDepthAndIntermediateAreReleasedBeforeRetry()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        Direct3D9Surface? multisampleDepthSurface = null;
        Direct3D9Surface? defaultDepthSurface = null;
        int depthBindCount = 0;
        int multisampleFailureCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            createDepthBuffer: (uint _, uint _, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                surface = multisampleType == MultisampleType.MultisampleNone
                    ? defaultDepthSurface = CreateSurfaceWithNativePointer()
                    : multisampleDepthSurface = CreateSurfaceWithNativePointer();
                return 0;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "DepthStencil:None" : "DepthStencil:Set");
                if (surface is not null && ++depthBindCount == 1)
                {
                    return Direct3D9Factory.OutOfVideoMemoryHResult;
                }

                return 0;
            },
            clear: (_, _, _, _, _) => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            setMultisampleFailed: () => multisampleFailureCount++);

        int result = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 0.5f);

        Assert.AreEqual(
            (0, 1, true, true, false, 2, 1),
            (result, multisampleFailureCount, intermediateSurface!.IsReleased,
                multisampleDepthSurface!.IsReleased, defaultDepthSurface!.IsReleased,
                depthBindCount, calls.Count(call => call == "DepthStencil:None")));
    }

    [TestMethod]
    public unsafe void WhenCreatingMultisampleDepthFailsThenIntermediateIsRetainedAndNextBegin3DRetriesCreation()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        Direct3D9Surface? depthSurface = null;
        int createDepthCount = 0;
        int multisampleFailureCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            createDepthBuffer: (uint _, uint _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                if (++createDepthCount == 1)
                {
                    surface = null;
                    return Direct3D9Factory.GenericFailureHResult;
                }

                depthSurface = CreateSurfaceWithNativePointer();
                surface = depthSurface;
                return 0;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "DepthStencil:None" : "DepthStencil:Set");
                return 0;
            },
            clear: (_, _, _, _, _) => 0);
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            setMultisampleFailed: () => multisampleFailureCount++);

        int firstResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 0.5f);
        bool retainedAfterFailure = !intermediateSurface!.IsReleased && !renderTarget.In3D;
        int secondResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 0.5f);
        int endResult = renderTarget.End3D();
        bool retainedAfterRetry = !intermediateSurface.IsReleased && !depthSurface!.IsReleased;
        renderTarget.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, 0, 0, 2, 2, 1, true, true, true),
            (firstResult, retainedAfterFailure, secondResult, endResult, createDepthCount,
                calls.Count(call => call == "DepthStencil:None"),
                calls.Count(call => call == "DepthStencil:Set"),
                retainedAfterRetry, intermediateSurface.IsReleased, depthSurface.IsReleased));
        Assert.AreEqual(0, multisampleFailureCount);
    }

    [TestMethod]
    public unsafe void WhenBindingMultisampleDepthFailsThenDepthAndIntermediateAreRetainedForNextBegin3D()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        Direct3D9Surface? depthSurface = null;
        int createDepthCount = 0;
        int depthBindCount = 0;
        int multisampleFailureCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            createDepthBuffer: (uint _, uint _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                createDepthCount++;
                depthSurface = CreateSurfaceWithNativePointer();
                surface = depthSurface;
                return 0;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "DepthStencil:None" : "DepthStencil:Set");
                if (surface is not null && ++depthBindCount == 1)
                {
                    return Direct3D9Factory.GenericFailureHResult;
                }

                return 0;
            },
            clear: (_, _, _, _, _) => 0);
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            setMultisampleFailed: () => multisampleFailureCount++);

        int firstResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 0.5f);
        bool retainedAfterFailure = !intermediateSurface!.IsReleased && !depthSurface!.IsReleased && !renderTarget.In3D;
        int secondResult = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 0.5f);
        int endResult = renderTarget.End3D();
        renderTarget.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, 0, 0, 1, 2, 2, true, true),
            (firstResult, retainedAfterFailure, secondResult, endResult, createDepthCount,
                depthBindCount, calls.Count(call => call == "DepthStencil:None"),
                intermediateSurface.IsReleased, depthSurface.IsReleased));
        Assert.AreEqual(0, multisampleFailureCount);
    }

    [TestMethod]
    public unsafe void WhenDirectSurfaceBecomesInvalidDuringDirect3DThenEnd3DSkipsCopyBackAndRestoresState()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        int stretchCallCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                surface = null;
                Assert.Fail("A direct 3D target must not create an intermediate surface.");
                return Direct3D9Factory.GenericFailureHResult;
            },
            stretchRect: (_, _, _, _) =>
            {
                stretchCallCount++;
                return Direct3D9Factory.GenericFailureHResult;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        int beginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.None,
            useZBuffer: false,
            z: 1f);
        Direct3D9SurfaceRect boundsDuring3D = renderTarget.Bounds;
        renderSurface.Dispose();
        calls.Clear();

        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, 0, new Direct3D9SurfaceRect(1, 2, 9, 10), 0, "", false, new Direct3D9SurfaceRect(0, 0, 16, 16), false, false),
            (beginResult, endResult, boundsDuring3D, stretchCallCount, string.Join('|', calls), renderTarget.In3D, renderTarget.Bounds, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDirectSurfaceBecomesInvalidDuring3DThenEnd3DStillAttemptsCopyBackAndRestoresState()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        Direct3D9Device? observedDevice = null;
        int stretchCallCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            stretchRect: (_, _, destination, _) =>
            {
                stretchCallCount++;
                calls.Add($"Copy:{stretchCallCount}:{destination.IsReleased}:{observedDevice!.IsEntered()}");
                return stretchCallCount == 1 ? 0 : Direct3D9Factory.GenericFailureHResult;
            });
        observedDevice = device;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        int beginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        renderSurface.Dispose();
        calls.Clear();

        int endResult = renderTarget.End3D();

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, "Stretch:Intermediate:1,2,9,10:Default:1,2,9,10|Copy:2:True:True", false, new Direct3D9SurfaceRect(0, 0, 16, 16), false, false),
            (beginResult, endResult, string.Join('|', calls), renderTarget.In3D, renderTarget.Bounds, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenEnd3DUsesIntermediateTargetThenPixelsAreCopiedBackAndBoundsAreRestored()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        bool deviceWasEnteredDuringCopyBack = false;
        Direct3D9Device? observedDevice = null;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            stretchRect: (_, _, _, _) =>
            {
                deviceWasEnteredDuringCopyBack = observedDevice!.IsEntered();
                return 0;
            });
        observedDevice = device;
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        _ = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        calls.Clear();

        int result = renderTarget.End3D();

        Assert.AreEqual(
            "0|False|0,0,16,16|Stretch:Intermediate:1,2,9,10:Default:1,2,9,10|True|False",
            $"{result}|{renderTarget.In3D}|{renderTarget.Bounds.Left},{renderTarget.Bounds.Top},{renderTarget.Bounds.Right},{renderTarget.Bounds.Bottom}|{string.Join('|', calls)}|{deviceWasEnteredDuringCopyBack}|{device.IsEntered()}");
    }

    [TestMethod]
    public unsafe void WhenEnd3DCopyBackFailsThenIntermediateIsReusedUntilRenderTargetIsReleased()
    {
        Queue<int> stretchResults = new([0, Direct3D9Factory.GenericFailureHResult, 0, 0]);
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        int createCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            [],
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                createCount++;
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            stretchRect: (_, _, _, _) => stretchResults.Dequeue());
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));

        int firstBeginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        int firstEndResult = renderTarget.End3D();
        bool releasedAfterFailure = intermediateSurface!.IsReleased;
        bool contentsValidAfterFailure = renderTarget.HasValidContents;
        int secondBeginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);
        int secondEndResult = renderTarget.End3D();
        renderTarget.Dispose();

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, false, false, 0, 0, 1, 4, true, false),
            (firstBeginResult, firstEndResult, releasedAfterFailure, contentsValidAfterFailure,
                secondBeginResult, secondEndResult, createCount, 4 - stretchResults.Count,
                intermediateSurface.IsReleased, renderTarget.In3D));
    }

    [TestMethod]
    public unsafe void WhenEnd3DCopyBackFailsThenDepthAndActiveTargetAreReusedUntilRenderTargetIsReleased()
    {
        Queue<int> stretchResults = new([0, Direct3D9Factory.GenericFailureHResult, 0, 0]);
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        Direct3D9Surface? depthSurface = null;
        int intermediateCreateCount = 0;
        int depthCreateCount = 0;
        int depthStencilCallCount = 0;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            [],
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateCreateCount++;
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            stretchRect: (_, _, _, _) => stretchResults.Dequeue(),
            createDepthBuffer: (uint _, uint _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                depthCreateCount++;
                depthSurface = CreateSurface();
                surface = depthSurface;
                return 0;
            },
            setDepthStencilSurface: _ =>
            {
                depthStencilCallCount++;
                return 0;
            },
            clear: (_, _, _, _, _) => 0);
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));

        int firstBeginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 1f);
        int firstEndResult = renderTarget.End3D();
        bool intermediateValidAfterFailure = intermediateSurface!.IsValid;
        bool depthValidAfterFailure = depthSurface!.IsValid;
        int secondBeginResult = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: true,
            z: 1f);
        int secondEndResult = renderTarget.End3D();
        renderTarget.Dispose();

        Assert.AreEqual(
            (0, Direct3D9Factory.GenericFailureHResult, true, true, 0, 0, 1, 1, 1, true, true),
            (firstBeginResult, firstEndResult, intermediateValidAfterFailure, depthValidAfterFailure,
                secondBeginResult, secondEndResult, intermediateCreateCount, depthCreateCount,
                depthStencilCallCount, intermediateSurface.IsReleased, depthSurface.IsReleased));
    }

    [TestMethod]
    public unsafe void WhenEnd3DCopyBackFailsThen3DStillEndsAndBoundsAreRestored()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        using Direct3D9Surface intermediateSurface = CreateSurface();
        Caps9 capabilities = default;
        capabilities.RasterCaps = (uint) D3D9.PrastercapsScissortest;
        int stretchCount = 0;
        bool deviceWasEnteredDuringCopyBack = false;
        Direct3D9Device? observedDevice = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            getRenderTargetDescription: surface => new SurfaceDesc(
                format: Format.A8R8G8B8,
                width: 16,
                height: 16,
                multiSampleType: ReferenceEquals(surface, renderSurface)
                    ? MultisampleType.MultisampleNone
                    : MultisampleType.Multisample4Samples),
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setScissorRect: _ => 0,
            setSurfaceToClippingMatrix: _ => 0,
            stretchRect: (_, _, _, _) =>
            {
                if (stretchCount++ == 0)
                {
                    return 0;
                }

                deviceWasEnteredDuringCopyBack = observedDevice!.IsEntered();
                return Direct3D9Factory.GenericFailureHResult;
            },
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                surface = intermediateSurface;
                return 0;
            });
        observedDevice = device;
        EnableFourSampleMultisampling(device);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        _ = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        int result = renderTarget.End3D();

        Assert.AreEqual(
            $"{Direct3D9Factory.GenericFailureHResult}|False|0,0,16,16|True|False",
            $"{result}|{renderTarget.In3D}|{renderTarget.Bounds.Left},{renderTarget.Bounds.Top},{renderTarget.Bounds.Right},{renderTarget.Bounds.Bottom}|{deviceWasEnteredDuringCopyBack}|{device.IsEntered()}");
    }

    [TestMethod]
    public unsafe void WhenEnd3DIsCalledOutside3DThenInvalidCallIsReturned()
    {
        using Direct3D9Device device = CreateEnsureStateDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int result = renderTarget.End3D();

        Assert.AreEqual(Direct3D9Factory.WgxInvalidCallHResult, result);
    }

    [TestMethod]
    public unsafe void WhenEnd3DIsCalledOutside3DOnInvalidDirectSurfaceThenInvalidCallIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Surface renderSurface = CreateSurface();
        using Direct3D9Device device = CreateEnsureStateDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        renderSurface.Dispose();

        int result = renderTarget.End3D();

        Assert.AreEqual(
            (Direct3D9Factory.WgxInvalidCallHResult, false, new Direct3D9SurfaceRect(0, 0, 16, 16), false),
            (result, renderTarget.In3D, renderTarget.Bounds, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenDisposedDuring3DThenActiveIntermediateIsReleasedAndBoundsAreRestored()
    {
        List<string> calls = [];
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        using Direct3D9Device device = CreateMultisampleBegin3DDevice(
            calls,
            renderSurface,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            });
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample4Samples,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16,
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        _ = renderTarget.Begin3D(
            new MilRectF(1, 2, 9, 10),
            MilAntiAliasMode.EightByEight,
            useZBuffer: false,
            z: 1f);

        renderTarget.Dispose();

        Assert.AreEqual(
            "False|0,0,16,16|False",
            $"{renderTarget.In3D}|{renderTarget.Bounds.Left},{renderTarget.Bounds.Top},{renderTarget.Bounds.Right},{renderTarget.Bounds.Bottom}|{intermediateSurface?.IsValid}");
    }

    [TestMethod]
    public unsafe void WhenDisposedAfterCreatingDepthThenDeviceUseIsReleasedAndDepthIsInvalidated()
    {
        List<string> calls = [];
        using Direct3D9Surface depthSurface = CreateSurface();
        using Direct3D9Device device = CreateBegin3DDevice(calls, depthSurface);
        using Direct3D9Surface renderSurface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);
        _ = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 1f);
        calls.Clear();

        renderTarget.Dispose();

        Assert.AreEqual("|False", $"{string.Join('|', calls)}|{depthSurface.IsValid}");
    }

    [TestMethod]
    public unsafe void WhenDisposedThenRenderTargetOperationsThrowObjectDisposedException()
    {
        using Direct3D9Device device = CreateEnsureStateDevice([]);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 16);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.Begin3D(default, MilAntiAliasMode.None, false, 1f));
        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.End3D());
        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.Clear(new MilColorF(1f, 0f, 0f, 0f), null));
    }

    [TestMethod]
    public unsafe void WhenDefaultBegin3DTargetBindingFailsThenLaterCallsAreSkipped()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls, failRenderTarget: true);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 16);

        int result = renderTarget.Begin3D(
            new MilRectF(0, 0, 16, 16),
            MilAntiAliasMode.None,
            useZBuffer: true,
            z: 1f);

        Assert.AreEqual(
            $"{Direct3D9Factory.GenericFailureHResult}|Description:1|Description:1|RenderTarget:1",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenGettingRenderTargetTypeThenHardwareRasterTypeIsReturned()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        InternalRenderTargetType result = renderTarget.GetRenderTargetType();

        Assert.AreEqual(InternalRenderTargetType.HardwareRaster, result);
    }

    [TestMethod]
    public unsafe void WhenGettingRenderTargetTypeFromInvalidDirectSurfaceThenHardwareRasterTypeIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();

        InternalRenderTargetType result = renderTarget.GetRenderTargetType();

        Assert.AreEqual((InternalRenderTargetType.HardwareRaster, false), (result, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenGettingPixelFormatThenConstructionValueIsReturned()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);

        MilPixelFormat result = renderTarget.GetPixelFormat();

        Assert.AreEqual(MilPixelFormat.Pbgra32Bpp, result);
    }

    [TestMethod]
    public unsafe void WhenGettingPixelFormatFromInvalidDirectSurfaceThenCachedFormatIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        surface.Dispose();

        MilPixelFormat result = renderTarget.GetPixelFormat();

        Assert.AreEqual(
            (MilPixelFormat.Pbgra32Bpp, false, false),
            (result, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenGettingSizeThenConstructionValuesAreReturned()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 1920,
            height: 1080);

        (uint Width, uint Height) result = renderTarget.GetSize();

        Assert.AreEqual((1920u, 1080u), result);
    }

    [TestMethod]
    public unsafe void WhenGettingSizeFromInvalidDirectSurfaceThenCachedSizeIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 1920,
            height: 1080);
        surface.Dispose();

        (uint Width, uint Height) result = renderTarget.GetSize();

        Assert.AreEqual(
            (1920u, 1080u, false, false),
            (result.Width, result.Height, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenGettingBoundsThenMaximumSurfaceBoundsAreReturned()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 1920,
            height: 1080);

        MilRectF result = renderTarget.GetBounds();

        Assert.AreEqual(new MilRectF(0, 0, 1920, 1080), result);
    }

    [TestMethod]
    public unsafe void WhenGettingDeviceTransformThenCachedPrimaryDisplayDpiScaleIsReturned()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        Matrix3x2 result = renderTarget.GetDeviceTransform();

        Assert.AreEqual(Direct3D9PrimaryDisplayDpi.GetDeviceTransform(), result);
    }

    [TestMethod]
    public unsafe void WhenGettingDeviceTransformFromInvalidDirectSurfaceThenCachedTransformIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();

        Matrix3x2 result = renderTarget.GetDeviceTransform();

        Assert.AreEqual(
            (Direct3D9PrimaryDisplayDpi.GetDeviceTransform(), false, false),
            (result, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenGettingBoundsFromInvalidDirectSurfaceThenCachedSizeIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 100,
            height: 80);
        surface.Dispose();

        MilRectF result = renderTarget.GetBounds();

        Assert.AreEqual((new MilRectF(0, 0, 100, 80), false), (result, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenGettingBoundsDuring3DThenMaximumSurfaceBoundsAreReturned()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            width: 100,
            height: 80,
            begin3DInternal: (_, _, _, multisampleType) => new Direct3D9Begin3DResult(0, multisampleType));
        _ = renderTarget.Begin3D(new MilRectF(10, 20, 30, 40), MilAntiAliasMode.None, useZBuffer: false, z: 0);

        MilRectF result = renderTarget.GetBounds();

        Assert.AreEqual(new MilRectF(0, 0, 100, 80), result);
    }

    [TestMethod]
    public unsafe void WhenGettingDirect3DTextureFormatFromInvalidDirectSurfaceThenCachedFormatIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Bgr32Bpp101010,
            renderTargetSurface: surface);
        surface.Dispose();

        Format result = renderTarget.GetDirect3DTextureFormat();

        Assert.AreEqual((Format.A2R10G10B10, false), (result, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenGettingDirect3DTextureFormatThenTargetFormatIsReturned()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Bgr32Bpp101010);

        Format result = renderTarget.GetDirect3DTextureFormat();

        Assert.AreEqual(Format.A2R10G10B10, result);
    }

    [TestMethod]
    [DataRow(0xFFFE0200u, 0xFFFF0200u, true)]
    [DataRow(0xFFFE0101u, 0xFFFF0200u, false)]
    [DataRow(0xFFFE0200u, 0xFFFF0104u, false)]
    public unsafe void WhenShaderPipelineCapabilityIsQueriedThenShaderModelTwoIsRequired(
        uint vertexShaderVersion,
        uint pixelShaderVersion,
        bool expected)
    {
        Caps9 capabilities = new()
        {
            VertexShaderVersion = vertexShaderVersion,
            PixelShaderVersion = pixelShaderVersion
        };
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: capabilities);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        bool result = renderTarget.CanUseShaderPipeline();

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public unsafe void WhenShaderPipelineCapabilityIsQueriedFromInvalidDirectSurfaceThenDeviceCapabilityIsReturnedWithoutDeviceEntry()
    {
        Caps9 capabilities = new()
        {
            VertexShaderVersion = 0xFFFE0200,
            PixelShaderVersion = 0xFFFF0200
        };
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: capabilities);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();

        bool result = renderTarget.CanUseShaderPipeline();

        Assert.AreEqual((true, false), (result, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenRealizationCacheIndexIsQueriedThenDeviceTokenIsReturned()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            realizationCacheIndex: 23);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        uint result = renderTarget.GetRealizationCacheIndex();

        Assert.AreEqual(23u, result);
    }

    [TestMethod]
    public unsafe void WhenRealizationCacheIndexIsQueriedFromInvalidDirectSurfaceThenDeviceTokenIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            realizationCacheIndex: 23);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();

        uint result = renderTarget.GetRealizationCacheIndex();

        Assert.AreEqual((23u, false), (result, device.IsEntered()));
    }

    [TestMethod]
    [DataRow((int) MilPixelFormat.Pbgra32Bpp, true)]
    [DataRow((int) MilPixelFormat.Prgba64Bpp, true)]
    [DataRow((int) MilPixelFormat.Prgba128BppFloat, true)]
    [DataRow((int) MilPixelFormat.Bgra32Bpp, false)]
    public unsafe void WhenAlphaCapabilityIsQueriedThenOnlyNativePremultipliedTargetFormatsReturnTrue(
        int pixelFormat,
        bool expected)
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: (MilPixelFormat) pixelFormat);

        bool result = renderTarget.HasAlpha();

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public unsafe void WhenAlphaCapabilityIsQueriedFromInvalidDirectSurfaceThenCachedFormatCapabilityIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        surface.Dispose();

        bool result = renderTarget.HasAlpha();

        Assert.AreEqual((true, false), (result, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenAlphaCapabilityQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.HasAlpha());
    }

    [TestMethod]
    public unsafe void WhenGettingDisplayIdThenAssociatedDisplayIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            associatedDisplayIndex: 1);

        uint? result = renderTarget.GetDisplayId();

        Assert.AreEqual((1u, false, false), (result, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenGettingDisplayIdWithoutAssociatedDisplayThenNoneIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        uint? result = renderTarget.GetDisplayId();

        Assert.AreEqual((null, false, false), (result, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenGettingDisplayIdFromInvalidDirectSurfaceThenCachedDisplayIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            associatedDisplayIndex: 1);
        surface.Dispose();

        uint? result = renderTarget.GetDisplayId();

        Assert.AreEqual((1u, false, false), (result, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenDisplayIdQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetDisplayId());
    }

    [TestMethod]
    public unsafe void WhenReadingEnabledDisplaysThenOnlyAssociatedDisplayIsEnabled()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            associatedDisplayIndex: 1);
        bool[] enabledDisplays = [true, false, true];

        int result = renderTarget.ReadEnabledDisplays(enabledDisplays);

        CollectionAssert.AreEqual(new[] { false, true, false }, enabledDisplays);
        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
    }

    [TestMethod]
    public unsafe void WhenRenderTargetHasNoAssociatedDisplayThenAllDisplaysAreDisabled()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        bool[] enabledDisplays = [true, true];

        int result = renderTarget.ReadEnabledDisplays(enabledDisplays);

        CollectionAssert.AreEqual(new[] { false, false }, enabledDisplays);
        Assert.AreEqual(Direct3D9Factory.SuccessHResult, result);
    }

    [TestMethod]
    public unsafe void WhenAssociatedDisplayIsOutsideCurrentDisplaySetThenReadFailsWithoutChangingOutput()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            associatedDisplayIndex: 2);
        bool[] enabledDisplays = [true, false];

        int result = renderTarget.ReadEnabledDisplays(enabledDisplays);

        CollectionAssert.AreEqual(new[] { true, false }, enabledDisplays);
        Assert.AreEqual(Direct3D9Factory.InvalidArgumentHResult, result);
    }

    [TestMethod]
    public unsafe void WhenReadingEnabledDisplaysFromInvalidDirectSurfaceThenAssociatedDisplayIsReturnedWithoutDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            associatedDisplayIndex: 1);
        bool[] enabledDisplays = [true, false, true];
        surface.Dispose();

        int result = renderTarget.ReadEnabledDisplays(enabledDisplays);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|False,True,False|False",
            $"{result}|{string.Join(',', enabledDisplays)}|{device.IsEntered()}");
    }

    [TestMethod]
    public unsafe void WhenDisposedThenEnabledDisplayReadThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.ReadEnabledDisplays(new bool[1]));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenRenderTargetTypeQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetRenderTargetType());
    }

    [TestMethod]
    public unsafe void WhenDisposedThenRealizationCacheIndexQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetRealizationCacheIndex());
    }

    [TestMethod]
    public unsafe void WhenDisposedThenShaderPipelineCapabilityQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.CanUseShaderPipeline());
    }

    [TestMethod]
    public unsafe void WhenDisposedThenPixelFormatQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetPixelFormat());
    }

    [TestMethod]
    public unsafe void WhenDisposedThenSizeQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetSize());
    }

    [TestMethod]
    public unsafe void WhenDisposedThenDeviceTransformQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetDeviceTransform());
    }

    [TestMethod]
    public unsafe void WhenDisposedThenBoundsQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetBounds());
    }

    [TestMethod]
    public unsafe void WhenDisposedThenDirect3DTextureFormatQueryThrowsObjectDisposedException()
    {
        using Direct3D9Device device = CreateDevice([], failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetDirect3DTextureFormat());
    }

    [TestMethod]
    public unsafe void WhenEnsuringNonMultisample2DStateThenNativeCallsFollowOriginalOrder()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls, failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int result = renderTarget.Ensure2DState();

        Assert.AreEqual(0, result);
        Assert.AreEqual(
            "DepthStencil|World|View|Projection|Cullmode:1|Zfunc:4|Zwriteenable:0",
            string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenEnsuringMultisample2DStateThenAntialiasingIsDisabledLast()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls, failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.Multisample2Samples);

        int result = renderTarget.Ensure2DState();

        Assert.AreEqual(0, result);
        Assert.AreEqual(
            "DepthStencil|World|View|Projection|Cullmode:1|Zfunc:4|Zwriteenable:0|Multisampleantialias:0",
            string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenTwoDimensionalSurfaceIsNotMultisampledThenCachedMultisampleTypeIsIgnored()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.Multisample2Samples,
            renderTargetSurface: surface);

        int result = renderTarget.Ensure2DState();

        Assert.AreEqual(
            (0, "DepthStencil|World|View|Projection|Cullmode:1|Zfunc:4|Zwriteenable:0|Description:1"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenTwoDimensionalSurfaceIsMultisampledThenAntialiasingIsDisabledDespiteCachedType()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls, targetMultisampleType: MultisampleType.Multisample2Samples);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);

        int result = renderTarget.Ensure2DState();

        Assert.AreEqual(
            (0, "DepthStencil|World|View|Projection|Cullmode:1|Zfunc:4|Zwriteenable:0|Description:1|Multisampleantialias:0"),
            (result, string.Join('|', calls)));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    public unsafe void WhenEnsuring2DStateFailsThenFirstFailureIsReturnedAndLaterCallsAreSkipped(int failAtCall)
    {
        string[] expectedCalls =
        [
            "DepthStencil",
            "World",
            "View",
            "Projection",
            "Cullmode:1",
            "Zfunc:4",
            "Zwriteenable:0",
            "Multisampleantialias:0"
        ];
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls, failAtCall);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.Multisample2Samples);

        int result = renderTarget.Ensure2DState();

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        string expected = string.Join('|', expectedCalls[..(failAtCall + 1)]);
        if (failAtCall == 0)
        {
            expected += "|DepthStencil";
        }

        Assert.AreEqual(expected, string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void When2DStateIsAlreadyEnsuredThenDeviceStateCachesSkipNativeCalls()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls, failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.Multisample2Samples);
        _ = renderTarget.Ensure2DState();

        int result = renderTarget.Ensure2DState();

        Assert.AreEqual((0, 8), (result, calls.Count));
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenEnsuring2DStateThrowsObjectDisposedException()
    {
        List<string> calls = [];
        Direct3D9Device device = CreateDevice(calls, failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.Ensure2DState());
    }

    [TestMethod]
    public unsafe void WhenEnsuringMultisample3DStateWithDepthThenNativeCallsFollowOriginalOrder()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls, failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample2Samples,
            (IDirect3DSurface9*) 1,
            isDepthBufferEnabled: true);

        int result = renderTarget.Ensure3DState(Create3DContextState(isAntialiasingEnabled: true));

        Assert.AreEqual(0, result);
        Assert.AreEqual(
            "World|View|Projection|Cullmode:2|DepthStencil:1|Zfunc:5|Multisampleantialias:1",
            string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenBegin3DEnablesDepthThenEnsuring3DStateBindsCreatedDepthSurface()
    {
        List<string> calls = [];
        using Direct3D9Surface depthSurface = CreateSurfaceWithNativePointer();
        using Direct3D9Device device = CreateBegin3DDevice(calls, depthSurface, record3DState: true);
        using Direct3D9Surface renderSurface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);
        _ = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.None, useZBuffer: true, z: 0.25f);
        _ = device.SetDepthStencilSurface(null);
        calls.Clear();

        int result = renderTarget.Ensure3DState(Create3DContextState(isAntialiasingEnabled: false));

        Assert.AreEqual(
            $"0|World|View|Projection|Cullmode:2|DepthStencil:{(nint) depthSurface.SurfaceForDeviceCall}|Zfunc:5|Description",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenEnd3DCompletesThenEnsuring3DStateNoLongerUsesActiveDepthSurface()
    {
        List<string> calls = [];
        using Direct3D9Surface depthSurface = CreateSurface();
        using Direct3D9Device device = CreateBegin3DDevice(calls, depthSurface, record3DState: true);
        using Direct3D9Surface renderSurface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: renderSurface,
            width: 16,
            height: 16);
        _ = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.None, useZBuffer: true, z: 0.25f);
        _ = renderTarget.End3D();
        calls.Clear();

        int result = renderTarget.Ensure3DState(Create3DContextState(isAntialiasingEnabled: false));

        Assert.AreEqual(
            "0|World|View|Projection|Cullmode:2|Description",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenEnsuringMultisample3DStateWithoutDepthThenZFunctionIsSkipped()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls, failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample2Samples);

        int result = renderTarget.Ensure3DState(Create3DContextState(isAntialiasingEnabled: false));

        Assert.AreEqual(0, result);
        Assert.AreEqual(
            "World|View|Projection|Cullmode:2|DepthStencil|Multisampleantialias:0",
            string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenThreeDimensionalSurfaceIsNotMultisampledThenCachedMultisampleTypeIsIgnored()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample2Samples,
            renderTargetSurfaceFor3D: surface);

        int result = renderTarget.Ensure3DState(Create3DContextState(isAntialiasingEnabled: true));

        Assert.AreEqual(
            (0, "World|View|Projection|Cullmode:2|DepthStencil|Description:1"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenThreeDimensionalSurfaceIsMultisampledThenAntialiasingUsesContextDespiteCachedType()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls, targetMultisampleType: MultisampleType.Multisample2Samples);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.MultisampleNone,
            renderTargetSurfaceFor3D: surface);

        int result = renderTarget.Ensure3DState(Create3DContextState(isAntialiasingEnabled: true));

        Assert.AreEqual(
            (0, "World|View|Projection|Cullmode:2|DepthStencil|Description:1|Multisampleantialias:1"),
            (result, string.Join('|', calls)));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    public unsafe void WhenEnsuring3DStateFailsThenFirstFailureIsReturnedAndLaterCallsAreSkipped(int failAtCall)
    {
        string[] expectedCalls =
        [
            "World",
            "View",
            "Projection",
            "Cullmode:2",
            "DepthStencil:1",
            "Zfunc:5",
            "Multisampleantialias:1"
        ];
        List<string> calls = [];
        using Direct3D9Device device = CreateDevice(calls, failAtCall);
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            MultisampleType.Multisample2Samples,
            (IDirect3DSurface9*) 1,
            isDepthBufferEnabled: true);

        int result = renderTarget.Ensure3DState(Create3DContextState(isAntialiasingEnabled: true));

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        string expected = string.Join('|', expectedCalls[..(failAtCall + 1)]);
        if (failAtCall == 4)
        {
            expected += "|DepthStencil";
        }

        Assert.AreEqual(expected, string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenEnsuring3DStateThrowsObjectDisposedException()
    {
        List<string> calls = [];
        Direct3D9Device device = CreateDevice(calls, failAtCall: -1);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => renderTarget.Ensure3DState(Create3DContextState(isAntialiasingEnabled: false)));
    }

    [TestMethod]
    public unsafe void WhenEnsuring2DStateThenRenderTargetClipResetAndStateFollowOriginalOrder()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            resetPerPrimitiveResourceUsage: () => calls.Add("ResetResources"),
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        Direct3D9ContextState contextState = Create3DContextState(isAntialiasingEnabled: false) with
        {
            AliasedClip = new Direct3D9SurfaceRect(1, 2, 9, 10)
        };

        int result = renderTarget.EnsureState(contextState);

        Assert.AreEqual(0, result);
        Assert.AreEqual(
            "Description:1|RenderTarget:1|Viewport|Matrix|Scissor|ResetResources|DepthStencil|World|View|Projection|Cullmode:1|Zfunc:4|Zwriteenable:0|Description:1",
            string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenEnsureStateClipIsEmptyThenResetAndSpecificStateAreSkipped()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            resetPerPrimitiveResourceUsage: () => calls.Add("ResetResources"));
        Direct3D9ContextState contextState = Create3DContextState(isAntialiasingEnabled: false) with
        {
            AliasedClip = new Direct3D9SurfaceRect(20, 20, 30, 30)
        };

        int result = renderTarget.EnsureState(contextState);

        Assert.AreEqual(Direct3D9Factory.ClippedToEmptyHResult, result);
        Assert.AreEqual("Description:1|RenderTarget:1|Viewport|Matrix", string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenEnsureClipExtendsBeyondCurrentBoundsThenBoundsIntersectionIsSet()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(3, 4, 12, 14));
        _ = device.SetRenderTarget(surface);
        calls.Clear();
        Direct3D9ContextState contextState = Create3DContextState(isAntialiasingEnabled: false) with
        {
            AliasedClip = new Direct3D9SurfaceRect(1, 6, 15, 20)
        };

        int result = renderTarget.EnsureClip(contextState);

        Assert.AreEqual(
            (0, new Direct3D9PointAndSizeRect(3, 6, 9, 8)),
            (result, device.GetClipRect()));
    }

    [TestMethod]
    public unsafe void WhenEnsureClipDoesNotIntersectCurrentBoundsThenClippedToEmptyIsReturnedWithoutDeviceStateChange()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(3, 4, 12, 14));
        _ = device.SetRenderTarget(surface);
        calls.Clear();
        Direct3D9ContextState contextState = Create3DContextState(isAntialiasingEnabled: false) with
        {
            AliasedClip = new Direct3D9SurfaceRect(12, 4, 20, 14)
        };

        int result = renderTarget.EnsureClip(contextState);

        Assert.AreEqual(
            (Direct3D9Factory.ClippedToEmptyHResult, false),
            (result, device.IsClipSet));
    }

    [TestMethod]
    public unsafe void WhenEnsureStateClipExtendsBeyondCurrentBoundsThenBoundsIntersectionIsUsed()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            initialBounds: new Direct3D9SurfaceRect(3, 4, 12, 14));
        Direct3D9ContextState contextState = Create3DContextState(isAntialiasingEnabled: false) with
        {
            AliasedClip = new Direct3D9SurfaceRect(1, 6, 15, 20)
        };

        int result = renderTarget.EnsureState(contextState);

        Assert.AreEqual(
            (0, new Direct3D9PointAndSizeRect(3, 6, 9, 8)),
            (result, device.GetClipRect()));
    }

    [TestMethod]
    public unsafe void WhenEnsuring3DStateThen3DRenderTargetIsSelectedBeforeClipAndState()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls);
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        _ = device.SetRenderTarget(surface2D);
        calls.Clear();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D,
            resetPerPrimitiveResourceUsage: () => calls.Add("ResetResources"),
            begin3DInternal: (_, _, _, requested) => new Direct3D9Begin3DResult(0, requested),
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        _ = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.None, false, 1);
        Direct3D9ContextState contextState = Create3DContextState(isAntialiasingEnabled: false) with
        {
            In3D = true,
            AliasedClip = new Direct3D9SurfaceRect(0, 0, 16, 16)
        };

        int result = renderTarget.EnsureState(contextState);

        Assert.AreEqual(0, result);
        Assert.AreEqual(
            "Description:2|RenderTarget:2|Viewport|Matrix|Scissor|ResetResources|World|View|Projection|Cullmode:2|DepthStencil|Description:2",
            string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenEnsureStateRenderTargetFailsThenLaterCallsAreSkipped()
    {
        List<string> calls = [];
        using Direct3D9Device device = CreateEnsureStateDevice(calls, failRenderTarget: true);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            resetPerPrimitiveResourceUsage: () => calls.Add("ResetResources"));

        int result = renderTarget.EnsureState(Create3DContextState(isAntialiasingEnabled: false));

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual("Description:1|RenderTarget:1", string.Join('|', calls));
    }

    [TestMethod]
    public unsafe void WhenRenderTargetIsValidThenQueuedPresentCountIsForwardedInsideDeviceEntry()
    {
        int queryCalls = 0;
        Direct3D9Device? queriedDevice = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            },
            getNumQueuedPresents: (out uint queuedPresentCount) =>
            {
                queryCalls++;
                queuedPresentCount = 2;
                Assert.IsTrue(queriedDevice!.IsEntered());
                return 0;
            });
        queriedDevice = device;
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 2u, 1, false), (result, queuedPresentCount, queryCalls, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenQueuedPresentQueryFailsThenFailureAndOutputArePreserved()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            },
            getNumQueuedPresents: (out uint queuedPresentCount) =>
            {
                queuedPresentCount = 1;
                return Direct3D9Factory.GenericFailureHResult;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 1u), (result, queuedPresentCount));
    }

    [TestMethod]
    public unsafe void WhenRenderTargetIsInvalidThenQueuedPresentQueryReturnsZeroWithoutDeviceCall()
    {
        int queryCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getNumQueuedPresents: (out uint queuedPresentCount) =>
            {
                queryCalls++;
                queuedPresentCount = 2;
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 0u, 0), (result, queuedPresentCount, queryCalls));
    }

    [TestMethod]
    public unsafe void WhenDirectSurfaceIsInvalidThenQueuedPresentQueryReturnsZeroWithoutDeviceEntry()
    {
        int queryCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getNumQueuedPresents: (out uint queuedPresentCount) =>
            {
                queryCalls++;
                queuedPresentCount = 2;
                return 0;
            });
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 12);
        surface.Dispose();

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual(
            (0, 0u, 0, false),
            (result, queuedPresentCount, queryCalls, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenQueuedPresentCountIsQueriedFromInvalidDirectSurfaceThenZeroIsReturnedWithoutDeviceEntry()
    {
        int queryCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getNumQueuedPresents: (out uint queuedPresentCount) =>
            {
                queryCalls++;
                queuedPresentCount = 2;
                return 0;
            });
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        surface.Dispose();

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 0u, 0, false), (result, queuedPresentCount, queryCalls, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenQueuedPresentQueryThrows()
    { 
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.GetNumQueuedPresents(out _));
    }

    [TestMethod]
    public unsafe void WhenAdvancingToDifferentFrameThenResourceManagerStagesRunOnce()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            associatedDisplayIndex: 0);

        renderTarget.AdvanceFrame(1);
        renderTarget.AdvanceFrame(1);

        Assert.AreEqual((1u, 1u, 1u), (
            device.ResourceManager.CompletedFrameCount,
            device.ResourceManager.ReleasedResourcesFromLastFrameDestroyCount,
            device.ResourceManager.DelayedResourceDestroyCount));
    }

    [TestMethod]
    public unsafe void WhenAdvancingToNewFramesThenEachFrameRunsResourceManagerStages()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            associatedDisplayIndex: 0);

        renderTarget.AdvanceFrame(1);
        renderTarget.AdvanceFrame(2);

        Assert.AreEqual((2u, 2u, 2u), (
            device.ResourceManager.CompletedFrameCount,
            device.ResourceManager.ReleasedResourcesFromLastFrameDestroyCount,
            device.ResourceManager.DelayedResourceDestroyCount));
    }

    [TestMethod]
    public unsafe void WhenDisplayIsInvalidThenAdvanceFrameSkipsResourceManagerStages()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = null;
                return Direct3D9Factory.DisplayStateInvalidHResult;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            associatedDisplayIndex: 0);
        _ = renderTarget.Resize(16, 12);

        renderTarget.AdvanceFrame(1);

        Assert.AreEqual((0u, 0u, 0u), (
            device.ResourceManager.CompletedFrameCount,
            device.ResourceManager.ReleasedResourcesFromLastFrameDestroyCount,
            device.ResourceManager.DelayedResourceDestroyCount));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenAdvanceFrameRejectsUse()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.AdvanceFrame(1));
    }

    [TestMethod]
    public unsafe void WhenRenderingAndFlippingChainAreValidThenRenderTargetIsValidWithoutSideEffects()
    {
        int presentCalls = 0;
        Direct3D9SwapChain? swapChain = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()),
                    present: () =>
                    {
                        presentCalls++;
                        return 0;
                    });
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));

        bool isValid = renderTarget.IsValid;

        Assert.AreEqual(
            (true, true, 0, false, 0, false),
            (isValid, swapChain!.IsValid, presentCalls, device.IsEntered(), renderTarget.DisplayInvalidHResult, renderTarget.HasValidContents));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenRenderTargetIsInvalidWhileFlippingChainRemainsValid()
    {
        int presentCalls = 0;
        Direct3D9SwapChain? swapChain = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()),
                    present: () =>
                    {
                        presentCalls++;
                        return Direct3D9Factory.InvalidArgumentHResult;
                    });
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Assert.AreEqual(Direct3D9Factory.InvalidArgumentHResult, renderTarget.Present());

        bool isValid = renderTarget.IsValid;

        Assert.AreEqual(
            (false, true, 1, false, 0, false),
            (isValid, swapChain!.IsValid, presentCalls, device.IsEntered(), renderTarget.DisplayInvalidHResult, renderTarget.HasValidContents));
    }

    [TestMethod]
    public unsafe void WhenFlippingChainIsInvalidThenRenderTargetIsInvalidWhileRenderingRemainsEnabled()
    {
        Direct3D9SwapChain? swapChain = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                swapChain = createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        swapChain!.Dispose();

        Assert.AreEqual((false, true, false), (renderTarget.IsValid, renderTarget.IsRenderingEnabled, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenDisplayRenderTargetIsDisposedThenValiditySafelyReturnsFalse()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));

        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.IsFalse(renderTarget.IsValid);
    }

    [TestMethod]
    public unsafe void WhenPresentingWithoutFlippingChainThenGenericFailureIsReturned()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int result = renderTarget.Present();

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    [DataRow(0, 0, true, 2)]
    [DataRow(Direct3D9Factory.PresentOccludedHResult, 0, true, 0)]
    [DataRow(Direct3D9Factory.PresentModeChangedHResult, Direct3D9Factory.DisplayStateInvalidHResult, false, 0)]
    [DataRow(Direct3D9Factory.DeviceLostHResult, Direct3D9Factory.DisplayStateInvalidHResult, false, 0)]
    [DataRow(Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.DisplayStateInvalidHResult, false, 0)]
    public unsafe void WhenPresentingThenResultAndRenderingStateFollowOriginalFailureHandling(
        int presentResult,
        int expectedResult,
        bool expectedRenderingEnabled,
        int expectedCreatedQueries)
    {
        int presentCalls = 0;
        int createdQueries = 0;
        Direct3D9Device? presentingDevice = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()),
                    present: () =>
                    {
                        presentCalls++;
                        Assert.IsTrue(presentingDevice!.IsEntered());
                        return presentResult;
                    });
                return 0;
            },
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                createdQueries++;
                query = new Direct3D9GpuQuery(() => 0, _ => 1);
                return 0;
            });
        presentingDevice = device;
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);

        int result = renderTarget.Present();

        Assert.AreEqual(
            (expectedResult, 1, expectedRenderingEnabled, false, expectedCreatedQueries),
            (result, presentCalls, renderTarget.IsRenderingEnabled, device.IsEntered(), createdQueries));
    }

    [TestMethod]
    [DataRow(false, Direct3D9Factory.InvalidArgumentHResult)]
    [DataRow(true, Direct3D9Factory.NeedRecreateAndPresentHResult)]
    public unsafe void WhenPresentReturnsInvalidArgumentThenLddmClassificationIsPreservedByRenderTarget(
        bool isLddmDevice,
        int expectedResult)
    {
        Caps9 capabilities = new()
        {
            Caps2 = isLddmDevice ? D3D9.Caps2Canshareresource : 0,
            MaxTextureWidth = 4096,
            MaxTextureHeight = 4096,
        };
        capabilities = isLddmDevice
            ? new Caps9
            {
                Caps2 = (uint) D3D9.Caps2Canshareresource,
                MaxTextureWidth = 4096,
                MaxTextureHeight = 4096,
            }
            : new Caps9
            {
                MaxTextureWidth = 4096,
                MaxTextureHeight = 4096,
            };
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()),
                    present: () => Direct3D9Factory.InvalidArgumentHResult);
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);

        int result = renderTarget.Present();

        Assert.AreEqual((expectedResult, false), (result, device.IsUnusable));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenPresentReturnsRememberedFailureWithoutCallingSwapChain()
    {
        int presentCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()),
                    present: () =>
                    {
                        presentCalls++;
                        return 0;
                    });
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.Resize(0, 12);

        int result = renderTarget.Present();

        Assert.AreEqual((0, 0), (result, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenSwapChainPrecedesOwnedTargetAndBorrowedThreeDSurfaceIsRetained()
    {
        RenderTargetReleaseOrder.Clear();
        Direct3D9Surface backBuffer = CreateTrackedSurface();
        using Direct3D9Surface borrowedThreeDSurface = CreateTrackedSurface();
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, backBuffer),
                    () => RenderTargetReleaseOrder.Add($"SwapChain:{_releaseOrderDevice?.IsEntered()}"));
                return 0;
            });
        _releaseOrderDevice = device;
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurfaceFor3D: borrowedThreeDSurface);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));

        renderTarget.Dispose();
        renderTarget.Dispose();

        CollectionAssert.AreEqual(
            new[] { "SwapChain:True", "Surface:True" },
            RenderTargetReleaseOrder);
        Assert.IsTrue(borrowedThreeDSurface.IsValid);
        _releaseOrderDevice = null;
    }

    [TestMethod]
    public unsafe void WhenResizingThenOldFlippingChainResourcesAreReplaced()
    { 
        List<PresentParameters> parameters = [];
        List<Direct3D9Surface> backBuffers = [];
        int releasedSwapChains = 0;
        Direct3D9Device? resizingDevice = null;
        using Direct3D9Device device = CreateResizeDevice(
            parameters,
            backBuffers,
            () =>
            {
                releasedSwapChains++;
                Assert.IsTrue(resizingDevice!.IsEntered());
            },
            () => Assert.IsTrue(resizingDevice!.IsEntered()));
        resizingDevice = device;
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int firstResult = renderTarget.Resize(16, 12);
        Direct3D9Surface firstBackBuffer = backBuffers[0];
        int secondResult = renderTarget.Resize(8, 6);

        Assert.AreEqual(
            (0, 0, 2, 1, true, (8u, 6u), new Direct3D9SurfaceRect(0, 0, 8, 6), false),
            (firstResult, secondResult, parameters.Count, releasedSwapChains, firstBackBuffer.IsReleased, renderTarget.GetSize(), renderTarget.Bounds, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenResizingToZeroThenOldSizeIsRetainedAndResourcesAreReleased()
    {
        List<PresentParameters> parameters = [];
        List<Direct3D9Surface> backBuffers = [];
        int releasedSwapChains = 0;
        using Direct3D9Device device = CreateResizeDevice(parameters, backBuffers, () => releasedSwapChains++);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        Direct3D9Surface backBuffer = backBuffers[0];

        int result = renderTarget.Resize(0, 12);

        Assert.AreEqual(
            (0, 1, 1, true, false, (16u, 12u), new Direct3D9SurfaceRect(0, 0, 16, 12)),
            (result, parameters.Count, releasedSwapChains, backBuffer.IsReleased, renderTarget.IsRenderingEnabled, renderTarget.GetSize(), renderTarget.Bounds));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenOwnedResourcesAreReleasedWhileDeviceIsEntered()
    {
        List<PresentParameters> parameters = [];
        List<Direct3D9Surface> backBuffers = [];
        bool deviceWasEnteredDuringRelease = false;
        Direct3D9Device? createdDevice = null;
        using Direct3D9Device device = CreateResizeDevice(
            parameters,
            backBuffers,
            () => deviceWasEnteredDuringRelease = createdDevice?.IsEntered() == true);
        createdDevice = device;
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);

        renderTarget.Dispose();

        Assert.AreEqual((true, false), (deviceWasEnteredDuringRelease, device.IsEntered()));
    }

    [DataTestMethod]
    [DataRow(8u, false, 0)]
    [DataRow(7u, true, 0)]
    [DataRow(8u, false, Direct3D9Factory.DeviceLostHResult)]
    [DataRow(7u, true, Direct3D9Factory.DeviceLostHResult)]
    public unsafe void WhenResizeAreaCrossesQuarterThenIntermediateReleasePrecedesFlippingChainCreation(
        uint size,
        bool expectedReleased,
        int creationResult)
    {
        using Direct3D9Surface defaultSurface = CreateSurface();
        Direct3D9Surface? intermediateSurface = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getRenderTargetDescription: surface => new SurfaceDesc(
                format: Format.A8R8G8B8,
                width: 16,
                height: 16,
                multiSampleType: ReferenceEquals(surface, defaultSurface)
                    ? MultisampleType.MultisampleNone
                    : MultisampleType.Multisample4Samples),
            setRenderTarget: _ => 0,
            setViewport: _ => 0,
            setScissorRect: _ => 0,
            setSurfaceToClippingMatrix: _ => 0,
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                intermediateSurface = CreateSurface();
                surface = intermediateSurface;
                return 0;
            },
            stretchRect: (_, _, _, _) => 0,
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                if (creationResult < 0)
                {
                    swapChain = null;
                    return creationResult;
                }

                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            });
        EnableFourSampleMultisampling(device);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            multisampleTypeFor3D: MultisampleType.Multisample4Samples,
            renderTargetSurface: defaultSurface,
            width: 16,
            height: 16);
        _ = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.EightByEight, false, 1f);
        _ = renderTarget.End3D();

        int result = renderTarget.Resize(size, size);

        int expectedResult = creationResult == Direct3D9Factory.DeviceLostHResult
            ? Direct3D9Factory.DisplayStateInvalidHResult
            : creationResult;
        Assert.AreEqual((expectedResult, expectedReleased), (result, intermediateSurface!.IsReleased));
    }

    [TestMethod]
    public unsafe void WhenFlippingChainCreationReportsDisplayInvalidThenFailureIsRemembered()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = null;
                return Direct3D9Factory.DisplayStateInvalidHResult;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int result = renderTarget.Resize(16, 12);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DisplayStateInvalidHResult, false),
            (result, renderTarget.DisplayInvalidHResult, renderTarget.IsRenderingEnabled));
    }

    [TestMethod]
    public unsafe void WhenGettingNewBackBufferFailsThenNewSizeIsRetainedAndSwapChainIsReleased()
    {
        int releasedSwapChains = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (Direct3D9Factory.GenericFailureHResult, null),
                    () => releasedSwapChains++);
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int result = renderTarget.Resize(16, 12);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, false, (16u, 12u), new Direct3D9SurfaceRect(0, 0, 16, 12)),
            (result, releasedSwapChains, renderTarget.IsRenderingEnabled, renderTarget.GetSize(), renderTarget.Bounds));
    }

    [TestMethod]
    public unsafe void WhenResizeSucceedsThenPreviouslyInvalidatedRectsAreCleared()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(0, 0, 4, 4));

        int resizeResult = renderTarget.Resize(8, 6);
        int presentResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 4, 4));

        Assert.AreEqual((0, 0, 0), (resizeResult, presentResult, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenResizeFailsThenContentsAreInvalidAndRenderingIsDisabled()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = null;
                return Direct3D9Factory.GenericFailureHResult;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.DrawBitmap(static () => 0));

        int result = renderTarget.Resize(16, 12);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, false),
            (result, renderTarget.HasValidContents, renderTarget.IsRenderingEnabled));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenResizeRejectsUse()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.Resize(16, 12));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenInvalidateRectSucceedsWithoutRetainingDirtyRegion()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.Resize(0, 12);

        int invalidateResult = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(0, 0, 4, 4));
        int presentResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 4, 4));

        Assert.AreEqual((0, 0, 0), (invalidateResult, presentResult, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenInvalidateRectRejectsUse()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.InvalidateRect(default));
    }

    [TestMethod]
    public unsafe void WhenInvalidatedBoundsDoNotIntersectPresentRectThenSwapChainIsNotPresented()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(0, 0, 4, 4));

        int firstResult = renderTarget.Present(new Direct3D9SurfaceRect(8, 6, 16, 12));
        int secondResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 4, 4));

        Assert.AreEqual((0, 0, 0), (firstResult, secondResult, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenEmptyRectIsInvalidatedThenEntireInputRectIsPresented()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(default);

        int result = renderTarget.Present(new Direct3D9SurfaceRect(8, 6, 16, 12));

        Assert.AreEqual((0, 1), (result, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenInvalidatedRectsAreClearedThenPresentSkipsPreviouslyDirtyRegion()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        Direct3D9SurfaceRect rect = new(0, 0, 4, 4);
        _ = renderTarget.InvalidateRect(rect);

        int clearResult = renderTarget.ClearInvalidatedRects();
        int presentResult = renderTarget.Present(rect);

        Assert.AreEqual((0, 0, 0), (clearResult, presentResult, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenEmptyInvalidationIsClearedThenPresentSkipsFullRefreshSentinel()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(default);

        int clearResult = renderTarget.ClearInvalidatedRects();
        int presentResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual((0, 0, 0), (clearResult, presentResult, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenClearInvalidatedRectsRejectsUse()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.ClearInvalidatedRects());
    }

    [TestMethod]
    public unsafe void WhenDisposedThenPresentRejectsUse()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.Present());
    }

    [TestMethod]
    public unsafe void WhenDisposedThenRectangularPresentRejectsUse()
    {
        using Direct3D9Device device = CreatePresentDevice(static () => 0);
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.Present(default));
    }

    [TestMethod]
    public unsafe void WhenPresentCompletesThenInvalidatedRectsAreCleared()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        Direct3D9SurfaceRect rect = new(0, 0, 4, 4);
        _ = renderTarget.InvalidateRect(rect);

        int firstResult = renderTarget.Present(rect);
        int secondResult = renderTarget.Present(rect);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenDirtyRectanglesAreDisabledThenEveryCopyPresentUsesEntireInputRect()
    {
        List<Direct3D9PresentRequest> requests = [];
        using Direct3D9Device device = CreatePresentDevice(request =>
        {
            requests.Add(request);
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            swapEffect: Swapeffect.Copy,
            initializationFlags: Direct3D9RenderTargetInitializationFlags.DisableDirtyRectangles);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(0, 0, 2, 2));
        Direct3D9SurfaceRect inputRect = new(8, 6, 16, 12);

        int firstResult = renderTarget.Present(inputRect);
        int secondResult = renderTarget.Present(inputRect);

        Assert.AreEqual(
            "0|0|8,6,16,12;8,6,16,12|True",
            $"{firstResult}|{secondResult}|{string.Join(';', requests.Select(request => FormatRect(request.SourceRect)))}|{requests.All(request => request.DirtyRegion is null)}");
    }

    [TestMethod]
    public unsafe void WhenCopyPresentThenRectanglesDirtyRegionWindowAndFlagsAreForwarded()
    {
        Direct3D9PresentRequest request = default;
        const nint deviceWindow = 123;
        using Direct3D9Device device = CreatePresentDevice(value =>
        {
            request = value;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            presentParameters: new PresentParameters(
                backBufferFormat: Format.X8R8G8B8,
                swapEffect: Swapeffect.Copy,
                hDeviceWindow: deviceWindow,
                windowed: true));
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(0, 0, 4, 4));
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(6, 4, 8, 6));
        Direct3D9SurfaceRect presentRect = new(0, 0, 8, 6);

        int result = renderTarget.Present(presentRect);

        Assert.AreEqual(
            $"0|{FormatRect(presentRect)}|{FormatRect(presentRect)}|{deviceWindow}|0|2",
            $"{result}|{FormatRect(request.SourceRect)}|{FormatRect(request.DestinationRect)}|{request.DestinationWindowOverride}|{request.Flags}|{request.DirtyRegion?.Count}");
    }

    [TestMethod]
    public unsafe void WhenFlipPresentThenRectanglesAndDirtyRegionAreClearedButWindowAndFlagsAreForwarded()
    {
        Direct3D9PresentRequest request = default;
        const nint deviceWindow = 456;
        using Direct3D9Device device = CreatePresentDevice(value =>
        {
            request = value;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            presentParameters: new PresentParameters(
                backBufferFormat: Format.X8R8G8B8,
                swapEffect: Swapeffect.Flip,
                hDeviceWindow: deviceWindow,
                windowed: true));
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(0, 0, 4, 4));

        int result = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 8, 6));

        Assert.AreEqual(
            (0, null, null, null, deviceWindow, 0u),
            (result, request.SourceRect, request.DestinationRect, request.DirtyRegion, request.DestinationWindowOverride, request.Flags));
    }

    [TestMethod]
    public unsafe void WhenCopyPresentIntersectsComplexInvalidRegionThenDirtyRectanglesArePreserved()
    {
        Direct3D9PresentRequest request = default;
        using Direct3D9Device device = CreatePresentDevice(value =>
        {
            request = value;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            swapEffect: Swapeffect.Copy);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(0, 0, 4, 4));
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(8, 6, 12, 10));

        int result = renderTarget.Present(new Direct3D9SurfaceRect(2, 2, 10, 8));

        Assert.AreEqual(
            "0|2,2,10,8|2,2,4,4;8,6,10,8",
            $"{result}|{FormatRect(request.SourceRect)}|{string.Join(';', request.DirtyRegion!.Select(rectangle => FormatRect(rectangle)))}");
    }

    [TestMethod]
    public unsafe void WhenCopyPresentIntersectsSimpleInvalidRegionThenPresentRectIsReduced()
    {
        Direct3D9PresentRequest request = default;
        using Direct3D9Device device = CreatePresentDevice(value =>
        {
            request = value;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            swapEffect: Swapeffect.Copy);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(4, 3, 12, 9));

        int result = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 8, 6));

        Assert.AreEqual("0|4,3,8,6|True", $"{result}|{FormatRect(request.SourceRect)}|{request.DirtyRegion is null}");
    }

    [TestMethod]
    public unsafe void WhenDiscardPresentIntersectsComplexInvalidRegionThenPresentParametersAreNull()
    {
        Direct3D9PresentRequest request = new(default, default, []);
        using Direct3D9Device device = CreatePresentDevice(value =>
        {
            request = value;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(0, 0, 4, 4));
        _ = renderTarget.InvalidateRect(new Direct3D9SurfaceRect(8, 6, 12, 10));

        int result = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual((0, null, null, null), (result, request.SourceRect, request.DestinationRect, request.DirtyRegion));
    }

    [TestMethod]
    public void WhenBackBufferIsNotTenBitThenLinearContentPresentIsNotRequested()
    {
        int result = Direct3D9SurfaceRenderTarget.GetPresentFlags(
            Format.A8R8G8B8,
            Format.X8R8G8B8,
            default,
            out uint presentFlags);

        Assert.AreEqual((0, 0u), (result, presentFlags));
    }

    [TestMethod]
    public void WhenDisplayModeIsTenBitThenLinearContentPresentIsNotRequested()
    {
        int result = Direct3D9SurfaceRenderTarget.GetPresentFlags(
            Format.A2R10G10B10,
            Format.A2R10G10B10,
            default,
            out uint presentFlags);

        Assert.AreEqual((0, 0u), (result, presentFlags));
    }

    [TestMethod]
    public void WhenTenBitBackBufferRequiresUnsupportedLinearPresentThenDisplayFormatIsRejected()
    {
        int result = Direct3D9SurfaceRenderTarget.GetPresentFlags(
            Format.A2R10G10B10,
            Format.X8R8G8B8,
            default,
            out uint presentFlags);

        Assert.AreEqual((Direct3D9Factory.DisplayFormatNotSupportedHResult, 0u), (result, presentFlags));
    }

    [TestMethod]
    public unsafe void WhenDisplayRenderTargetInitializationFailsThenCandidateIsReleasedAndOutputIsCleared()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            displayMode: new Displaymode(format: Format.X8R8G8B8));
        Direct3D9SurfaceRenderTarget candidate = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Bgr32Bpp101010);

        int result = Direct3D9SurfaceRenderTarget.TransferInitializedDisplayRenderTarget(
            candidate,
            out Direct3D9SurfaceRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayFormatNotSupportedHResult, null),
            (result, renderTarget));
        Assert.ThrowsExactly<ObjectDisposedException>(() => candidate.GetPixelFormat());
    }

    [TestMethod]
    public unsafe void WhenTenBitBackBufferRequiresUnsupportedLinearPresentThenResizeAndPresentFailWithoutSwapChain()
    {
        int createSwapChainCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            displayMode: new Displaymode(format: Format.X8R8G8B8),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                createSwapChainCalls++;
                swapChain = null;
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Bgr32Bpp101010);

        int resizeResult = renderTarget.Resize(16, 12);
        int presentResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual(
            (Direct3D9Factory.DisplayFormatNotSupportedHResult, Direct3D9Factory.DisplayFormatNotSupportedHResult, 0, false),
            (resizeResult, presentResult, createSwapChainCalls, renderTarget.IsRenderingEnabled));
    }

    [TestMethod]
    public unsafe void WhenTenBitBackBufferRequiresSupportedLinearPresentThenPresentFlagIsForwarded()
    {
        Direct3D9PresentRequest request = default;
        Caps9 capabilities = new()
        {
            Caps3 = (uint) D3D9.Caps3LinearToSrgbPresentation,
            MaxTextureWidth = 4096,
            MaxTextureHeight = 4096
        };
        Displaymode displayMode = new(format: Format.X8R8G8B8);
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            displayMode: displayMode,
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()),
                    presentWithParameters: value =>
                    {
                        request = value;
                        return 0;
                    });
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Bgr32Bpp101010,
            swapEffect: Swapeffect.Copy);
        _ = renderTarget.Resize(16, 12);
        _ = renderTarget.InvalidateRect(default);

        int result = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual((0, (uint) D3D9.PresentLinearContent), (result, request.Flags));
    }

    [TestMethod]
    public unsafe void WhenCreatingHalDisplayRenderTargetThenGetDeviceContextFailureDoesNotBlockCreation()
    {
        using Direct3D9DeviceManager manager = CreateDisplayRenderTargetDeviceManager(Direct3D9Factory.InvalidCallHResult);
        Direct3D9DeviceRequest request = new(1, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);

        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ => 0,
            out Direct3D9SurfaceRenderTarget? renderTarget);

        using (renderTarget)
        {
            Assert.AreEqual((0, MilPixelFormat.Bgr32Bpp, false), (result, renderTarget?.GetPixelFormat(), renderTarget?.IsRenderingEnabled));
        }
    }

    [TestMethod]
    public unsafe void WhenCreatingNonHalDisplayRenderTargetAndGetDeviceContextFailsThenFactoryIsSkippedAndOutputIsCleared()
    {
        using Direct3D9DeviceManager manager = CreateDisplayRenderTargetDeviceManager(Direct3D9Factory.InvalidCallHResult);
        Direct3D9DeviceRequest request = new(
            1,
            (Direct3D9RenderTargetInitializationFlags) 0x40000000,
            0,
            Devtype.Hal);
        int formatTestCalls = 0;
        int renderTargetFactoryCalls = 0;

        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ =>
            {
                formatTestCalls++;
                return 0;
            },
            (_, _) =>
            {
                renderTargetFactoryCalls++;
                throw new InvalidOperationException();
            },
            out Direct3D9SurfaceRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.NoHardwareDeviceHResult, null, 1, 0),
            (result, renderTarget, formatTestCalls, renderTargetFactoryCalls));
    }

    [TestMethod]
    public unsafe void WhenCreatingNonHalDisplayRenderTargetAndGetDeviceContextSucceedsThenFactoryRunsOnceAndOutputIsCommitted()
    {
        using Direct3D9DeviceManager manager = CreateDisplayRenderTargetDeviceManager(0);
        Direct3D9DeviceRequest request = new(
            1,
            (Direct3D9RenderTargetInitializationFlags) 0x40000000,
            0,
            Devtype.Hal);
        int formatTestCalls = 0;
        int renderTargetFactoryCalls = 0;
        Direct3D9SurfaceRenderTarget? candidate = null;

        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ =>
            {
                formatTestCalls++;
                return 0;
            },
            (deviceParameters, creationRequest) =>
            {
                renderTargetFactoryCalls++;
                candidate = new Direct3D9SurfaceRenderTarget(
                    deviceParameters.Device,
                    MultisampleType.MultisampleNone,
                    pixelFormat: MilPixelFormat.Bgr32Bpp,
                    associatedDisplayIndex: creationRequest.DisplayAdapterOrdinal,
                    initializationFlags: creationRequest.InitializationFlags,
                    presentParameters: deviceParameters.PresentParameters);
                return candidate;
            },
            out Direct3D9SurfaceRenderTarget? renderTarget);

        using (renderTarget)
        {
            Assert.AreEqual((0, candidate, 1, 1), (result, renderTarget, formatTestCalls, renderTargetFactoryCalls));
        }
    }

    [TestMethod]
    public unsafe void WhenGettingDisplayRenderTargetDeviceThrowsComExceptionThenLaterStagesAreSkippedAndOutputIsCleared()
    {
        int deviceFactoryCalls = 0;
        using Direct3D9DeviceManager manager = new(
            (_, _, _) =>
            {
                deviceFactoryCalls++;
                Marshal.ThrowExceptionForHR(Direct3D9Factory.InvalidCallHResult);
                throw new InvalidOperationException();
            },
            isWindow: _ => true);
        Direct3D9DeviceRequest request = new(1, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);
        int formatTestCalls = 0;
        int renderTargetFactoryCalls = 0;

        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ =>
            {
                formatTestCalls++;
                return 0;
            },
            (_, _) =>
            {
                renderTargetFactoryCalls++;
                throw new InvalidOperationException();
            },
            out Direct3D9SurfaceRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, 1, 0, 0),
            (result, renderTarget, deviceFactoryCalls, formatTestCalls, renderTargetFactoryCalls));
    }

    [TestMethod]
    public unsafe void WhenGettingDisplayRenderTargetDeviceRunsOutOfMemoryThenLaterStagesAreSkippedAndOutputIsCleared()
    {
        int deviceFactoryCalls = 0;
        using Direct3D9DeviceManager manager = new(
            (_, _, _) =>
            {
                deviceFactoryCalls++;
                Marshal.ThrowExceptionForHR(Direct3D9Factory.OutOfMemoryHResult);
                throw new InvalidOperationException();
            },
            isWindow: _ => true);
        Direct3D9DeviceRequest request = new(1, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);
        int formatTestCalls = 0;
        int renderTargetFactoryCalls = 0;

        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ =>
            {
                formatTestCalls++;
                return 0;
            },
            (_, _) =>
            {
                renderTargetFactoryCalls++;
                throw new InvalidOperationException();
            },
            out Direct3D9SurfaceRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, null, 1, 0, 0),
            (result, renderTarget, deviceFactoryCalls, formatTestCalls, renderTargetFactoryCalls));
    }

    [TestMethod]
    public unsafe void WhenCreatingDisplayRenderTargetAndFormatTestFailsThenFactoryIsSkippedAndOutputIsCleared()
    {
        using Direct3D9DeviceManager manager = CreateDisplayRenderTargetDeviceManager(0);
        Direct3D9DeviceRequest request = new(1, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);
        int formatTestCalls = 0;
        int renderTargetFactoryCalls = 0;

        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ =>
            {
                formatTestCalls++;
                return Direct3D9Factory.InvalidCallHResult;
            },
            (_, _) =>
            {
                renderTargetFactoryCalls++;
                throw new InvalidOperationException();
            },
            out Direct3D9SurfaceRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, 1, 0),
            (result, renderTarget, formatTestCalls, renderTargetFactoryCalls));
    }

    [TestMethod]
    public unsafe void WhenCheckingDisplayRenderTargetFormatThrowsComExceptionThenFactoryIsSkippedAndOutputIsCleared()
    {
        using Direct3D9DeviceManager manager = CreateDisplayRenderTargetDeviceManager(0);
        Direct3D9DeviceRequest request = new(1, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);
        int formatTestCalls = 0;
        int renderTargetFactoryCalls = 0;

        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ =>
            {
                formatTestCalls++;
                Marshal.ThrowExceptionForHR(Direct3D9Factory.InvalidCallHResult);
                throw new InvalidOperationException();
            },
            (_, _) =>
            {
                renderTargetFactoryCalls++;
                throw new InvalidOperationException();
            },
            out Direct3D9SurfaceRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, 1, 0),
            (result, renderTarget, formatTestCalls, renderTargetFactoryCalls));
    }

    [TestMethod]
    public unsafe void WhenCheckingDisplayRenderTargetFormatRunsOutOfMemoryThenFactoryIsSkippedAndOutputIsCleared()
    {
        using Direct3D9DeviceManager manager = CreateDisplayRenderTargetDeviceManager(0);
        Direct3D9DeviceRequest request = new(1, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);
        int formatTestCalls = 0;
        int renderTargetFactoryCalls = 0;

        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ =>
            {
                formatTestCalls++;
                Marshal.ThrowExceptionForHR(Direct3D9Factory.OutOfMemoryHResult);
                throw new InvalidOperationException();
            },
            (_, _) =>
            {
                renderTargetFactoryCalls++;
                throw new InvalidOperationException();
            },
            out Direct3D9SurfaceRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, null, 1, 0),
            (result, renderTarget, formatTestCalls, renderTargetFactoryCalls));
    }

    [TestMethod]
    public unsafe void WhenDisplayRenderTargetInitializationThrowsComExceptionThenOriginalFailureIsReturnedAndOutputIsCleared()
    {
        using Direct3D9DeviceManager manager = CreateDisplayRenderTargetDeviceManager(0);
        Direct3D9DeviceRequest request = new(1, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);

        int formatTestCalls = 0;
        int factoryCalls = 0;
        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ =>
            {
                formatTestCalls++;
                return 0;
            },
            (_, _) =>
            {
                factoryCalls++;
                Marshal.ThrowExceptionForHR(Direct3D9Factory.InvalidCallHResult);
                throw new InvalidOperationException();
            },
            out Direct3D9SurfaceRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, 1, 1),
            (result, renderTarget, formatTestCalls, factoryCalls));
    }

    [TestMethod]
    public unsafe void WhenCreatingDisplayRenderTargetRunsOutOfMemoryThenHResultIsReturnedAndOutputIsCleared()
    {
        using Direct3D9DeviceManager manager = CreateDisplayRenderTargetDeviceManager(0);
        Direct3D9DeviceRequest request = new(1, Direct3D9RenderTargetInitializationFlags.None, 0, Devtype.Hal);

        int formatTestCalls = 0;
        int factoryCalls = 0;
        int result = Direct3D9SurfaceRenderTarget.TryCreateDisplayRenderTarget(
            manager,
            request,
            default,
            _ =>
            {
                formatTestCalls++;
                return 0;
            },
            (_, _) =>
            {
                factoryCalls++;
                Marshal.ThrowExceptionForHR(Direct3D9Factory.OutOfMemoryHResult);
                throw new InvalidOperationException();
            },
            out Direct3D9SurfaceRenderTarget? renderTarget);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, null, 1, 1),
            (result, renderTarget, formatTestCalls, factoryCalls));
    }

    [TestMethod]
    public void WhenCreatingDirtyRegionDataThenHeaderAndRectanglesMatchWin32Layout()
    {
        byte[] data = Direct3D9SwapChain.CreateDirtyRegionData(
        [
            new Direct3D9SurfaceRect(2, 3, 7, 11),
            new Direct3D9SurfaceRect(13, 17, 19, 23)
        ])!;
        int[] values = new int[data.Length / sizeof(int)];
        Buffer.BlockCopy(data, 0, values, 0, data.Length);

        Assert.AreEqual(
            "32,1,2,32,2,3,19,23|2,3,7,11|13,17,19,23",
            $"{string.Join(',', values[..8])}|{string.Join(',', values[8..12])}|{string.Join(',', values[12..16])}");
    }

    [TestMethod]
    public void WhenDirtyRegionIsEmptyThenNoWin32RegionDataIsCreated()
    {
        Assert.IsNull(Direct3D9SwapChain.CreateDirtyRegionData([]));
    }

    private static unsafe Direct3D9DeviceManager CreateDisplayRenderTargetDeviceManager(int getDeviceContextHResult)
    {
        return new Direct3D9DeviceManager(
            (parameters, unusableNotification, disposedNotification) =>
            new Direct3D9Device(
                null,
                null,
                parameters.AdapterOrdinal,
                parameters.DeviceType,
                parameters.BehaviorFlags,
                parameters.PresentParameters,
                unusableNotification,
                disposedNotification,
                parameters.Capabilities,
                parameters.DisplayMode,
                focusWindow: parameters.FocusWindow,
                checkDepthStencilMatch: (_, _, _, _, _) => 0,
                testLockableSwapChainForFormatTest: (_, status) =>
                {
                    status.TestGetDeviceContext(
                        () => new Direct3D9SurfaceDeviceContextResult(getDeviceContextHResult, 0),
                        _ => 0);
                    return 0;
                },
                setRenderTargetForFormatTest: () => 0,
                clearDepthStencilSurfaceForFormatTest: () => 0,
                createLockableTextureForFormatTest: (_, _, _, _, _, _) => 0,
                beginSceneForFormatTest: () => 0,
                renderTextureForFormatTest: () => 0,
                endSceneForFormatTest: () => 0),
            isWindow: _ => true);
    }

    private static string FormatRect(Direct3D9SurfaceRect? rectangle)
    {
        Direct3D9SurfaceRect value = rectangle.GetValueOrDefault();
        return $"{value.Left},{value.Top},{value.Right},{value.Bottom}";
    }

    private static unsafe Direct3D9Device CreatePresentDevice(Func<Direct3D9PresentRequest, int> present)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { MaxTextureWidth = 4096, MaxTextureHeight = 4096 },
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()),
                    presentWithParameters: present);
                return 0;
            });
    }

    private static unsafe Direct3D9Device CreatePresentDevice(Func<int> present)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { MaxTextureWidth = 4096, MaxTextureHeight = 4096 },
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()),
                    present: present);
                return 0;
            });
    }


    private static unsafe Direct3D9Device CreateResizeDevice(
        List<PresentParameters> parameters,
        List<Direct3D9Surface> backBuffers,
        Action releaseSwapChain,
        Action? createSwapChain = null)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters presentParameters, out Direct3D9SwapChain? swapChain) =>
            {
                createSwapChain?.Invoke();
                parameters.Add(presentParameters);
                swapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ =>
                    {
                        Direct3D9Surface surface = CreateSurface();
                        backBuffers.Add(surface);
                        return (0, surface);
                    },
                    releaseSwapChain);
                return 0;
            });
    }

    private static Direct3D9ContextState Create3DContextState(bool isAntialiasingEnabled)
    { 
        return new Direct3D9ContextState(
            Matrix4x4.CreateTranslation(1f, 2f, 3f),
            Matrix4x4.CreateScale(2f),
            Matrix4x4.CreatePerspectiveFieldOfView(1f, 1f, 1f, 100f),
            Matrix4x4.CreateTranslation(4f, 5f, 0f),
            Cull.CW,
            Cmpfunc.Greater,
            isAntialiasingEnabled);
    }

    private static unsafe Direct3D9Device CreateEnsureStateDevice(
        List<string> calls,
        bool failRenderTarget = false,
        MultisampleType targetMultisampleType = MultisampleType.MultisampleNone)
    {
        Dictionary<Direct3D9Surface, int> surfaceIds = [];
        int GetSurfaceId(Direct3D9Surface surface)
        {
            if (!surfaceIds.TryGetValue(surface, out int id))
            {
                id = surfaceIds.Count + 1;
                surfaceIds.Add(surface, id);
            }

            return id;
        }

        Caps9 capabilities = default;
        capabilities.RasterCaps = (uint) D3D9.PrastercapsScissortest;
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            getRenderTargetDescription: surface =>
            {
                calls.Add($"Description:{GetSurfaceId(surface)}");
                return new SurfaceDesc(
                    width: 16,
                    height: 16,
                    multiSampleType: targetMultisampleType);
            },
            setRenderTarget: surface =>
            {
                calls.Add($"RenderTarget:{GetSurfaceId(surface)}");
                return failRenderTarget ? Direct3D9Factory.GenericFailureHResult : 0;
            },
            setViewport: _ =>
            {
                calls.Add("Viewport");
                return 0;
            },
            setScissorRect: _ =>
            {
                calls.Add("Scissor");
                return 0;
            },
            setSurfaceToClippingMatrix: _ =>
            {
                calls.Add("Matrix");
                return 0;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(surface is null ? "DepthStencil" : $"DepthStencil:{(nint) surface}");
                return 0;
            },
            setRenderState: (state, value) =>
            {
                if (state is not Renderstatetype.Zenable and not Renderstatetype.Stencilenable)
                {
                    calls.Add($"{GetRenderStateName(state)}:{value}");
                }

                return 0;
            },
            setTransform: (state, _) =>
            {
                calls.Add(GetTransformName(state));
                return 0;
            });
    }

    private static unsafe Direct3D9Surface CreateSurface()
    {
        return new Direct3D9Surface(new Direct3D9ResourceManager(), null);
    }

    private static unsafe Direct3D9Surface CreateSurfaceWithNativePointer()
    {
        void** memory = (void**) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
        void** vtable = memory + 1;
        memory[0] = vtable;
        vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseTestSurface;
        return new Direct3D9Surface(new Direct3D9ResourceManager(), (IDirect3DSurface9*) memory);
    }

    private static unsafe Direct3D9Surface CreateTrackedSurface()
    {
        void** memory = (void**) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
        void** vtable = memory + 1;
        memory[0] = vtable;
        vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseTrackedSurface;
        return new Direct3D9Surface(new Direct3D9ResourceManager(), (IDirect3DSurface9*) memory);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe uint ReleaseTrackedSurface(IDirect3DSurface9* surface)
    {
        RenderTargetReleaseOrder.Add($"Surface:{_releaseOrderDevice?.IsEntered()}");
        NativeMemory.Free(surface);
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe uint ReleaseTestSurface(IDirect3DSurface9* surface)
    {
        NativeMemory.Free(surface);
        return 0;
    }

    private static void EnableFourSampleMultisampling(Direct3D9Device device)
    {
        device.UpdateTier(2 << 16);
        device.UpdateMultisampleSupport(new Direct3D9MultisampleSupport(
            MultisampleType.Multisample4Samples,
            MultisampleType.Multisample4Samples,
            MultisampleType.Multisample4Samples));
    }

    private static unsafe Direct3D9Device CreateBegin3DDevice(
        List<string> calls,
        Direct3D9Surface depthSurface,
        bool record3DState = false)
    {
        Caps9 capabilities = default;
        capabilities.RasterCaps = (uint) D3D9.PrastercapsScissortest;
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            getRenderTargetDescription: _ =>
            {
                calls.Add("Description");
                return new SurfaceDesc(width: 16, height: 16);
            },
            setRenderTarget: _ =>
            {
                calls.Add("RenderTarget");
                return 0;
            },
            setViewport: _ =>
            {
                calls.Add("Viewport");
                return 0;
            },
            setScissorRect: _ =>
            {
                calls.Add("Scissor");
                return 0;
            },
            setSurfaceToClippingMatrix: _ =>
            {
                calls.Add("Matrix");
                return 0;
            },
            setDepthStencilSurface: surface =>
            {
                calls.Add(record3DState && surface is not null
                    ? $"DepthStencil:{(nint) surface}"
                    : "DepthStencil");
                return 0;
            },
            setRenderState: record3DState
                ? (state, value) =>
                {
                    if (state is not Renderstatetype.Zenable and not Renderstatetype.Stencilenable)
                    {
                        calls.Add($"{GetRenderStateName(state)}:{value}");
                    }

                    return 0;
                }
                : null,
            setTransform: record3DState
                ? (state, _) =>
                {
                    calls.Add(GetTransformName(state));
                    return 0;
                }
                : null,
            createDepthBuffer: (uint width, uint height, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                calls.Add($"CreateDepth:{width},{height},{multisampleType}");
                surface = depthSurface;
                return 0;
            },
            clear: (count, flags, color, depth, stencil) =>
            {
                calls.Add($"Clear:{count},{flags},{color:X8},{depth},{stencil}");
                return 0;
            });
    }

    private static unsafe Direct3D9Device CreateMultisampleBegin3DDevice(
        List<string> calls,
        Direct3D9Surface renderSurface,
        Direct3D9CreateRenderTarget createRenderTarget,
        Direct3D9StretchRect? stretchRect = null,
        Func<Direct3D9Surface, int>? setRenderTarget = null,
        Direct3D9CreateDepthBuffer? createDepthBuffer = null,
        Direct3D9SetDepthStencilSurface? setDepthStencilSurface = null,
        Direct3D9Clear? clear = null,
        Func<Direct3D9Surface, SurfaceDesc>? getRenderTargetDescription = null)
    {
        Direct3D9Surface? intermediateSurface = null;
        string GetSurfaceName(Direct3D9Surface surface) => ReferenceEquals(surface, renderSurface) ? "Default" : "Intermediate";

        Caps9 capabilities = default;
        capabilities.RasterCaps = (uint) D3D9.PrastercapsScissortest;
        Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            getRenderTargetDescription: surface =>
            {
                calls.Add($"Description:{GetSurfaceName(surface)}");
                return getRenderTargetDescription?.Invoke(surface)
                    ?? new SurfaceDesc(
                        format: Format.A8R8G8B8,
                        width: 16,
                        height: 16,
                        multiSampleType: ReferenceEquals(surface, renderSurface)
                            ? MultisampleType.MultisampleNone
                            : MultisampleType.Multisample4Samples);
            },
            setRenderTarget: surface =>
            {
                intermediateSurface ??= ReferenceEquals(surface, renderSurface) ? null : surface;
                calls.Add($"RenderTarget:{GetSurfaceName(surface)}");
                return setRenderTarget?.Invoke(surface) ?? 0;
            },
            setViewport: _ =>
            {
                calls.Add("Viewport");
                return 0;
            },
            setScissorRect: _ =>
            {
                calls.Add("Scissor");
                return 0;
            },
            setSurfaceToClippingMatrix: _ =>
            {
                calls.Add("Matrix");
                return 0;
            },
            createRenderTarget: (uint width, uint height, Format format, MultisampleType multisampleType, out Direct3D9Surface? surface) =>
            {
                int result = createRenderTarget(width, height, format, multisampleType, out surface);
                intermediateSurface = surface;
                return result;
            },
            stretchRect: (source, sourceRect, destination, destinationRect) =>
            {
                calls.Add(
                    $"Stretch:{GetSurfaceName(source)}:{sourceRect.Left},{sourceRect.Top},{sourceRect.Right},{sourceRect.Bottom}:" +
                    $"{GetSurfaceName(destination)}:{destinationRect.Left},{destinationRect.Top},{destinationRect.Right},{destinationRect.Bottom}");
                return stretchRect?.Invoke(source, sourceRect, destination, destinationRect) ?? 0;
            },
            createDepthBuffer: createDepthBuffer,
            setDepthStencilSurface: setDepthStencilSurface,
            setRenderState: (_, _) => 0,
            clear: clear);
        EnableFourSampleMultisampling(device);
        return device;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe uint AddRefVideoBitmap(void*** instance)
    {
        _videoBitmapAddRefCount++;
        return 2;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe uint ReleaseVideoBitmap(void*** instance)
    {
        _videoBitmapReleaseCount++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int GetVideoBitmapSize(void*** instance, uint* width, uint* height)
    {
        *width = _videoBitmapWidth;
        *height = _videoBitmapHeight;
        return _videoBitmapGetSizeResult;
    }

    private sealed unsafe class FakeVideoBitmapSource : IDisposable
    {
        private nint _memory;

        internal FakeVideoBitmapSource(uint width = 16, uint height = 16, int getSizeResult = 0)
        {
            _videoBitmapAddRefCount = 0;
            _videoBitmapReleaseCount = 0;
            _videoBitmapWidth = width;
            _videoBitmapHeight = height;
            _videoBitmapGetSizeResult = getSizeResult;
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 5);
            void** memory = (void**) _memory;
            void** vtable = memory + 1;
            memory[0] = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &AddRefVideoBitmap;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &ReleaseVideoBitmap;
            vtable[3] = (void*) (delegate* unmanaged[Stdcall]<void***, uint*, uint*, int>) &GetVideoBitmapSize;
        }

        internal nint Pointer => _memory;

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
        }
    }

    private static int AddDrawMeshCall(List<string> calls, string call)
    {
        calls.Add(call);
        return 0;
    }

    private sealed class TestResource(Direct3D9ResourceManager manager) : Direct3D9Resource(manager)
    {
        protected override void ReleaseD3DResources()
        {
        }
    }

    private static unsafe void AssertDisplayDrawingIsSkippedWhenRenderingIsDisabled(
        Func<Direct3D9SurfaceRenderTarget, Func<int>, int> draw)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(0, 16);
        int drawCalls = 0;

        int result = draw(renderTarget, () =>
        {
            drawCalls++;
            return Direct3D9Factory.GenericFailureHResult;
        });

        Assert.AreEqual((0, 0), (result, drawCalls));
    }

    private static unsafe Direct3D9Device CreateClearDevice(
        List<string> calls,
        int failAtStep = -1,
        Action? observeDeviceCall = null)
    {
        int step = 0;
        int RecordTerminalCall(string call)
        {
            observeDeviceCall?.Invoke();
            calls.Add(call);
            return step++ == failAtStep ? Direct3D9Factory.GenericFailureHResult : 0;
        }

        Caps9 capabilities = default;
        capabilities.RasterCaps = (uint) D3D9.PrastercapsScissortest;
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            getRenderTargetDescription: _ =>
            {
                observeDeviceCall?.Invoke();
                calls.Add("Description");
                return new SurfaceDesc(width: 16, height: 16);
            },
            setRenderTarget: _ => RecordTerminalCall("RenderTarget"),
            setViewport: _ =>
            {
                observeDeviceCall?.Invoke();
                calls.Add("Viewport");
                return 0;
            },
            setScissorRect: rect => RecordTerminalCall(
                rect.HasValue
                    ? $"Scissor:{rect.Value.X},{rect.Value.Y},{rect.Value.Width},{rect.Value.Height}"
                    : "Scissor"),
            setSurfaceToClippingMatrix: _ =>
            {
                observeDeviceCall?.Invoke();
                calls.Add("Matrix");
                return 0;
            },
            clear: (count, flags, color, depth, stencil) => RecordTerminalCall(
                $"Clear:{count},{flags},{color:X8},{depth},{stencil}"));
    }

    private static unsafe Direct3D9Device CreateDevice(List<string> calls, int failAtCall)
    {
        int RecordCall(string call)
        {
            calls.Add(call);
            return calls.Count - 1 == failAtCall ? Direct3D9Factory.GenericFailureHResult : 0;
        }

        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            setDepthStencilSurface: surface => RecordCall(surface is null ? "DepthStencil" : $"DepthStencil:{(nint) surface}"),
            setRenderState: (state, value) =>
                state is Renderstatetype.Zenable or Renderstatetype.Stencilenable
                    ? 0
                    : RecordCall($"{GetRenderStateName(state)}:{value}"),
            setTransform: (state, _) => RecordCall(GetTransformName(state)));
    }

    private static string GetRenderStateName(Renderstatetype state)
    {
        return state switch
        {
            Renderstatetype.Cullmode => "Cullmode",
            Renderstatetype.Zenable => "Zenable",
            Renderstatetype.Stencilenable => "Stencilenable",
            Renderstatetype.Zfunc => "Zfunc",
            Renderstatetype.Zwriteenable => "Zwriteenable",
            Renderstatetype.Multisampleantialias => "Multisampleantialias",
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };
    }

    private static string GetTransformName(Transformstatetype state)
    {
        return state switch
        {
            (Transformstatetype) 256 => "World",
            Transformstatetype.View => "View",
            Transformstatetype.Projection => "Projection",
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };
    }
}
