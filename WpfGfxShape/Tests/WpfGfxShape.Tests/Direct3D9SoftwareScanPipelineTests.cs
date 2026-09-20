using System.Runtime.Versioning;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed class Direct3D9SoftwareScanPipelineTests
{
    [TestMethod]
    public void WhenRenderingPipelineIsSetUpThenTargetFormatAndRenderingArgumentsAreForwarded()
    {
        Direct3D9SoftwareRenderingPipelineRequest actual = default;
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            new Direct3D9SoftwareScanPipelineCallbacks(
                request =>
                {
                    actual = request;
                    return Direct3D9Factory.SuccessHResult;
                },
                _ => Direct3D9Factory.SuccessHResult,
                (_, _, _, _, _) => { },
                _ => { },
                () => { }));

        int result = renderTarget.SetupPipeline(
            MilPixelFormat.Gray8Bpp,
            11,
            perPrimitiveAntiAliasing: true,
            complementAlpha: true,
            MilCompositingMode.SourceCopy,
            clipWidth: 23,
            effectList: 31,
            effectToDevice: 37,
            contextState: 41);

        Assert.AreEqual(
            (0, MilPixelFormat.Pbgra32Bpp, (nint) 11, true, true, MilCompositingMode.SourceCopy, 23u, (nint) 31, (nint) 37, (nint) 41),
            (result, actual.TargetPixelFormat, actual.ColorSource, actual.PerPrimitiveAntiAliasing, actual.ComplementAlpha,
                actual.CompositingMode, actual.ClipWidth, actual.EffectList, actual.EffectToDevice, actual.ContextState));
    }

    [TestMethod]
    public void WhenTextPipelineIsSetUpThenArgumentsAndFailureAreForwarded()
    {
        Direct3D9SoftwareTextPipelineRequest actual = default;
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => 0,
                request =>
                {
                    actual = request;
                    return Direct3D9Factory.OutOfMemoryHResult;
                },
                (_, _, _, _, _) => { },
                _ => { },
                () => { }));

        int result = renderTarget.SetupPipelineForText(13, MilCompositingMode.SourceOver, 17, needsAntiAliasing: true);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, MilPixelFormat.Pbgra32Bpp, (nint) 13, MilCompositingMode.SourceOver, (nint) 17, true),
            (result, actual.TargetPixelFormat, actual.ColorSource, actual.CompositingMode, actual.GlyphPainter, actual.NeedsAntiAliasing));
    }

    [TestMethod]
    public void WhenOutputSpanRunsDuringLockedPathThenDestinationOffsetAndPixelCountUseStrideAndBytesPerPixel()
    {
        byte[] pixels = new byte[80];
        (byte[]? Pixels, int Offset, uint Count, int X, int Y) actual = default;
        Direct3D9SoftwareRenderTargetSurface? renderTarget = null;
        renderTarget = CreateRenderTarget(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => 0,
                _ => 0,
                (destination, offset, count, x, y) => actual = (destination, offset, count, x, y),
                _ => { },
                () => { }),
            pixels,
            stride: 24,
            fillPath: (_, _, _, _) =>
            {
                int setupResult = renderTarget!.SetupPipeline(
                    MilPixelFormat.Gray8Bpp,
                    1,
                    false,
                    false,
                    MilCompositingMode.SourceOver,
                    4);
                return setupResult < 0 ? setupResult : renderTarget.OutputSpan(y: 2, xMin: 1, xMax: 4);
            });
        using (renderTarget)
        {
            int result = renderTarget.DrawInfinitePath(new Direct3D9SurfaceRect(0, 0, 4, 3));

            Assert.AreEqual((0, pixels, 52, 3u, 1, 2), (result, actual.Pixels, actual.Offset, actual.Count, actual.X, actual.Y));
        }
    }

    [TestMethod]
    [DataRow(-1, 0, 1)]
    [DataRow(0, -1, 1)]
    [DataRow(0, 1, 1)]
    [DataRow(0, 0, 5)]
    [DataRow(3, 0, 1)]
    public void WhenOutputSpanIsOutsideTargetThenInvalidCallIsReturnedWithoutRunningPipeline(int y, int xMin, int xMax)
    {
        int runCount = 0;
        Direct3D9SoftwareRenderTargetSurface? renderTarget = null;
        renderTarget = CreateRenderTarget(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => 0,
                _ => 0,
                (_, _, _, _, _) => runCount++,
                _ => { },
                () => { }),
            fillPath: (_, _, _, _) =>
            {
                int setupResult = renderTarget!.SetupPipeline(MilPixelFormat.Pbgra32Bpp, 1, false, false, MilCompositingMode.SourceOver, 4);
                return setupResult < 0 ? setupResult : renderTarget.OutputSpan(y, xMin, xMax);
            });
        using (renderTarget)
        {
            int result = renderTarget.DrawInfinitePath(new Direct3D9SurfaceRect(0, 0, 4, 3));

            Assert.AreEqual((Direct3D9Factory.WgxInvalidCallHResult, 0), (result, runCount));
        }
    }

    [TestMethod]
    public void WhenOutputSpanIsCalledWithoutLockedTargetThenInvalidCallIsReturned()
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(CreateCallbacks());

        int result = renderTarget.OutputSpan(0, 0, 1);

        Assert.AreEqual(Direct3D9Factory.WgxInvalidCallHResult, result);
    }

    [TestMethod]
    public void WhenPipelineIsInitializedTwiceThenSecondInitializationFailsUntilResourcesAreReleased()
    {
        int initializationCount = 0;
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ =>
                {
                    initializationCount++;
                    return 0;
                },
                _ => 0,
                (_, _, _, _, _) => { },
                _ => { },
                () => { }));

        int firstResult = renderTarget.SetupPipeline(MilPixelFormat.Gray8Bpp, 1, false, false, MilCompositingMode.SourceOver, 4);
        int secondResult = renderTarget.SetupPipeline(MilPixelFormat.Gray8Bpp, 2, false, false, MilCompositingMode.SourceOver, 4);
        renderTarget.ReleaseExpensiveResources();
        int thirdResult = renderTarget.SetupPipeline(MilPixelFormat.Gray8Bpp, 3, false, false, MilCompositingMode.SourceOver, 4);

        Assert.AreEqual((0, Direct3D9Factory.WgxInvalidCallHResult, 0, 2),
            (firstResult, secondResult, thirdResult, initializationCount));
    }

    [TestMethod]
    public void WhenAntialiasedFillerIsSetThenSameInstanceIsForwardedWithoutOwnershipChanges()
    {
        object filler = new();
        object? actual = null;
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => 0,
                _ => 0,
                (_, _, _, _, _) => { },
                value => actual = value,
                () => { }));

        _ = renderTarget.SetupPipeline(
            MilPixelFormat.Pbgra32Bpp,
            1,
            perPrimitiveAntiAliasing: true,
            complementAlpha: false,
            MilCompositingMode.SourceOver,
            clipWidth: 4);
        renderTarget.SetAntialiasedFiller(filler);

        Assert.AreSame(filler, actual);
    }

    [TestMethod]
    public void WhenPathRasterizerFailsAfterPipelineSetupThenPrimaryFailureIsPreservedAndResourcesReleaseBeforeUnlock()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface? renderTarget = null;
        renderTarget = CreateRenderTarget(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ =>
                {
                    calls.Add("Setup");
                    return 0;
                },
                _ => 0,
                (_, _, _, _, _) => { },
                _ => { },
                () => calls.Add("ReleasePipeline")),
            calls: calls,
            fillPath: (_, _, _, _) =>
            {
                int setupResult = renderTarget!.SetupPipeline(MilPixelFormat.Gray8Bpp, 1, false, false, MilCompositingMode.SourceOver, 4);
                calls.Add("Rasterize");
                return setupResult < 0 ? setupResult : Direct3D9Factory.OutOfMemoryHResult;
            });
        using (renderTarget)
        {
            int result = renderTarget.DrawInfinitePath(new Direct3D9SurfaceRect(0, 0, 4, 3));

            Assert.AreEqual(
                (Direct3D9Factory.OutOfMemoryHResult, "Lock|Setup|Rasterize|ReleasePipeline|Unlock"),
                (result, string.Join('|', calls)));
        }
    }

    [TestMethod]
    public void WhenGlyphTextPipelineSucceedsThenResourcesReleaseBeforeUnlock()
    {
        List<string> calls = [];
        Direct3D9SoftwareRenderTargetSurface? renderTarget = null;
        renderTarget = CreateRenderTarget(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => 0,
                _ =>
                {
                    calls.Add("SetupText");
                    return 0;
                },
                (_, _, _, _, _) => { },
                _ => { },
                () => calls.Add("ReleasePipeline")),
            calls: calls,
            drawGlyphs: (_, _, _, _, _, _) =>
            {
                int result = renderTarget!.SetupPipelineForText(1, MilCompositingMode.SourceOver, 2, true);
                calls.Add("GlyphRasterize");
                return result;
            });
        using (renderTarget)
        {
            int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

            Assert.AreEqual((0, "Realize|Lock|SetupText|GlyphRasterize|ReleasePipeline|Unlock"),
                (result, string.Join('|', calls)));
        }
    }

    [TestMethod]
    public void WhenSurfaceIsDisposedThenPipelineMethodsAreProtectedAndOwnedResourcesAreReleased()
    {
        int releaseCount = 0;
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => 0,
                _ => 0,
                (_, _, _, _, _) => { },
                _ => { },
                () => releaseCount++));
        _ = renderTarget.SetupPipeline(MilPixelFormat.Gray8Bpp, 1, false, false, MilCompositingMode.SourceOver, 4);
        renderTarget.Dispose();

        Action setup = () => renderTarget.SetupPipeline(MilPixelFormat.Gray8Bpp, 1, false, false, MilCompositingMode.SourceOver, 4);

        Assert.ThrowsExactly<ObjectDisposedException>(setup);
        Assert.AreEqual(1, releaseCount);
    }

    [TestMethod]
    public void WhenRenderingInitializationFailsThenPartialResourcesAreReleasedAndRetryCanRun()
    {
        int initializationCount = 0;
        int releaseCount = 0;
        int runCount = 0;
        using Direct3D9SoftwareScanPipeline pipeline = new(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => ++initializationCount == 1 ? Direct3D9Factory.OutOfMemoryHResult : 0,
                _ => 0,
                (_, _, _, _, _) => runCount++,
                _ => { },
                () => releaseCount++));
        Direct3D9SoftwareRenderingPipelineRequest request = new(
            MilPixelFormat.Pbgra32Bpp,
            1,
            false,
            false,
            MilCompositingMode.SourceOver,
            4,
            0,
            0,
            0);

        int firstResult = pipeline.InitializeForRendering(request);
        int secondResult = pipeline.InitializeForRendering(request);
        pipeline.Run(new byte[4], 0, 1, 0, 0);

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, 0, 2, 1, 1),
            (firstResult, secondResult, initializationCount, releaseCount, runCount));
    }

    [TestMethod]
    public void WhenInitializationReentersThenNestedInitializationIsRejectedWithoutReplacingBuiltPipeline()
    {
        int nestedResult = 0;
        int runCount = 0;
        Direct3D9SoftwareScanPipeline? pipeline = null;
        Direct3D9SoftwareRenderingPipelineRequest request = new(
            MilPixelFormat.Pbgra32Bpp,
            1,
            false,
            false,
            MilCompositingMode.SourceOver,
            4,
            0,
            0,
            0);
        pipeline = new Direct3D9SoftwareScanPipeline(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ =>
                {
                    nestedResult = pipeline.InitializeForRendering(request);
                    return 0;
                },
                _ => 0,
                (_, _, _, _, _) => runCount++,
                _ => { },
                () => { }));
        using (pipeline)
        {
            int result = pipeline.InitializeForRendering(request);
            pipeline.Run(new byte[4], 0, 1, 0, 0);

            Assert.AreEqual((0, Direct3D9Factory.WgxInvalidCallHResult, 1), (result, nestedResult, runCount));
        }
    }

    [TestMethod]
    public void WhenResourcesAreReleasedThenOldPipelineCannotRunAndRepeatedReleaseIsSafe()
    {
        int releaseCount = 0;
        using Direct3D9SoftwareScanPipeline pipeline = new(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => 0,
                _ => 0,
                (_, _, _, _, _) => { },
                _ => { },
                () => releaseCount++));
        _ = pipeline.InitializeForRendering(new Direct3D9SoftwareRenderingPipelineRequest(
            MilPixelFormat.Pbgra32Bpp,
            1,
            false,
            false,
            MilCompositingMode.SourceOver,
            4,
            0,
            0,
            0));

        pipeline.ReleaseExpensiveResources();
        pipeline.ReleaseExpensiveResources();
        Action run = () => pipeline.Run(new byte[4], 0, 1, 0, 0);

        Assert.ThrowsExactly<InvalidOperationException>(run);
        Assert.AreEqual(1, releaseCount);
    }

    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(true, 1)]
    public void WhenFillerIsSetThenOnlyPipelineWithPpaaOperationConsumesIt(bool perPrimitiveAntiAliasing, int expectedSetCount)
    {
        int setCount = 0;
        using Direct3D9SoftwareScanPipeline pipeline = new(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => 0,
                _ => 0,
                (_, _, _, _, _) => { },
                _ => setCount++,
                () => { }));
        _ = pipeline.InitializeForRendering(new Direct3D9SoftwareRenderingPipelineRequest(
            MilPixelFormat.Pbgra32Bpp,
            1,
            perPrimitiveAntiAliasing,
            false,
            MilCompositingMode.SourceOver,
            4,
            0,
            0,
            0));

        pipeline.SetAntialiasedFiller(new object());

        Assert.AreEqual(expectedSetCount, setCount);
    }

    [TestMethod]
    public void WhenTextInitializationFailsThenOldGenerationIsNotRunnableAndRetryCanSucceed()
    {
        int initializationCount = 0;
        int releaseCount = 0;
        using Direct3D9SoftwareScanPipeline pipeline = new(
            new Direct3D9SoftwareScanPipelineCallbacks(
                _ => 0,
                _ => ++initializationCount == 1 ? Direct3D9Factory.OutOfMemoryHResult : 0,
                (_, _, _, _, _) => { },
                _ => { },
                () => releaseCount++));
        Direct3D9SoftwareTextPipelineRequest request = new(
            MilPixelFormat.Pbgra32Bpp,
            1,
            MilCompositingMode.SourceOver,
            2,
            true);

        int firstResult = pipeline.InitializeForTextRendering(request);
        Action failedRun = () => pipeline.Run(new byte[4], 0, 1, 0, 0);
        Assert.ThrowsExactly<InvalidOperationException>(failedRun);
        int secondResult = pipeline.InitializeForTextRendering(request);

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, 0, 2, 1),
            (firstResult, secondResult, initializationCount, releaseCount));
    }

    private static Direct3D9SoftwareScanPipelineCallbacks CreateCallbacks()
    {
        return new Direct3D9SoftwareScanPipelineCallbacks(
            _ => 0,
            _ => 0,
            (_, _, _, _, _) => { },
            _ => { },
            () => { });
    }

    private static Direct3D9SoftwareRenderTargetSurface CreateRenderTarget(
        Direct3D9SoftwareScanPipelineCallbacks callbacks,
        byte[]? pixels = null,
        int stride = 16,
        List<string>? calls = null,
        Direct3D9DrawPathSoftwareRenderTarget? fillPath = null,
        Direct3D9DrawGlyphsSoftwareRenderTarget? drawGlyphs = null)
    {
        pixels ??= new byte[Math.Max(stride * 3, 48)];
        calls ??= [];
        return new Direct3D9SoftwareRenderTargetSurface(
            width: 4,
            height: 3,
            lockTarget: (out byte[] targetPixels, out int targetStride) =>
            {
                calls.Add("Lock");
                targetPixels = pixels;
                targetStride = stride;
                return 0;
            },
            unlockTarget: () => calls.Add("Unlock"),
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.NotAvailableHResult;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            fillPathSoftwareRenderTarget: fillPath,
            ensureGlyphBrushRealization: () =>
            {
                calls.Add("Realize");
                return 0;
            },
            hasRealizedGlyphBrush: () => true,
            getGlyphBrushOpacity: () => 1f,
            drawGlyphsSoftwareRenderTarget: drawGlyphs,
            scanPipelineCallbacks: callbacks);
    }
}
