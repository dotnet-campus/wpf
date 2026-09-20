using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9SurfaceRenderTargetBindingTests
{
    [TestMethod]
    public unsafe void WhenBindingOutside3DThenTwoDimensionalSurfaceIsSelected()
    {
        Direct3D9Surface? selectedSurface = null;
        using Direct3D9Device device = CreateDevice(surface =>
        {
            selectedSurface = surface;
            return 0;
        });
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D);

        int result = renderTarget.SetAsRenderTarget();

        Assert.AreEqual((0, true), (result, ReferenceEquals(surface2D, selectedSurface)));
    }

    [TestMethod]
    public unsafe void WhenTwoDimensionalRenderTargetBindingFailsThenFailureIsReturned()
    {
        using Direct3D9Device device = CreateDevice(_ => Direct3D9Factory.GenericFailureHResult);
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);

        int result = renderTarget.SetAsRenderTarget();

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public unsafe void WhenBindingInside3DThenThreeDimensionalSurfaceIsSelected()
    {
        Direct3D9Surface? selectedSurface = null;
        using Direct3D9Device device = CreateDevice(surface =>
        {
            selectedSurface = surface;
            return 0;
        });
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D,
            begin3DInternal: (_, _, _, requested) => new Direct3D9Begin3DResult(0, requested),
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        _ = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.None, false, 1);

        int result = renderTarget.SetAsRenderTarget();

        Assert.AreEqual((0, true), (result, ReferenceEquals(surface3D, selectedSurface)));
    }

    [TestMethod]
    public unsafe void WhenBindingExplicitlyFor3DOutside3DThenThreeDimensionalSurfaceIsSelected()
    {
        Direct3D9Surface? selectedSurface = null;
        using Direct3D9Device device = CreateDevice(surface =>
        {
            selectedSurface = surface;
            return 0;
        });
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D);

        int result = renderTarget.SetAsRenderTargetFor3D();

        Assert.AreEqual((0, true), (result, ReferenceEquals(surface3D, selectedSurface)));
    }

    [TestMethod]
    public unsafe void WhenEnsuringStateOutside3DThenRenderTargetContextSelectsTwoDimensionalSurface()
    {
        Direct3D9Surface? selectedSurface = null;
        using Direct3D9Device device = CreateDevice(surface =>
        {
            selectedSurface = surface;
            return Direct3D9Factory.GenericFailureHResult;
        });
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D);
        Direct3D9ContextState mismatchedState = new() { In3D = true };

        int result = renderTarget.EnsureState(mismatchedState);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true),
            (result, ReferenceEquals(surface2D, selectedSurface)));
    }

    [TestMethod]
    public unsafe void WhenEnsuringStateInside3DThenRenderTargetContextSelectsThreeDimensionalSurface()
    {
        Direct3D9Surface? selectedSurface = null;
        using Direct3D9Device device = CreateDevice(surface =>
        {
            selectedSurface = surface;
            return Direct3D9Factory.GenericFailureHResult;
        });
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D,
            begin3DInternal: (_, _, _, requested) => new Direct3D9Begin3DResult(0, requested),
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16));
        _ = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.None, false, 1);
        Direct3D9ContextState mismatchedState = new() { In3D = false };

        int result = renderTarget.EnsureState(mismatchedState);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true),
            (result, ReferenceEquals(surface3D, selectedSurface)));
    }

    [TestMethod]
    public unsafe void WhenEnsuringClipAgainstEmptyTargetBoundsThenReturnsClippedToEmptyWithoutDeviceCall()
    {
        int deviceCalls = 0;
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getRenderTargetDescription: _ => new SurfaceDesc(width: 16, height: 16),
            setViewport: _ =>
            {
                deviceCalls++;
                return 0;
            },
            setSurfaceToClippingMatrix: _ =>
            {
                deviceCalls++;
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        Direct3D9ContextState contextState = new()
        {
            AliasedClip = new Direct3D9SurfaceRect(1, 2, 8, 9)
        };

        int result = renderTarget.EnsureClip(contextState);

        Assert.AreEqual((Direct3D9Factory.ClippedToEmptyHResult, 0), (result, deviceCalls));
    }

    [TestMethod]
    public unsafe void WhenBindingExplicitlyFor3DThenThreeDimensionalSurfaceIsSelectedAndFailureIsReturned()
    {
        Direct3D9Surface? selectedSurface = null;
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        using Direct3D9Device device = CreateDevice(surface =>
        {
            selectedSurface = surface;
            return Direct3D9Factory.GenericFailureHResult;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D);

        int result = renderTarget.SetAsRenderTargetFor3D();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true),
            (result, ReferenceEquals(surface3D, selectedSurface)));
    }

    [TestMethod]
    public unsafe void WhenClearingInside3DThenActiveThreeDimensionalSurfaceIsBoundBeforeClear()
    {
        List<string> calls = [];
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        using Direct3D9Device device = CreateDevice(
            surface =>
            {
                calls.Add(ReferenceEquals(surface, surface3D) ? "RenderTarget3D" : "RenderTarget2D");
                return 0;
            },
            (_, _, _, _, _) =>
            {
                calls.Add("Clear");
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D,
            begin3DInternal: (_, _, _, requested) => new Direct3D9Begin3DResult(0, requested),
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16),
            width: 16,
            height: 16);
        _ = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.None, false, 1);

        int result = renderTarget.Clear(new MilColorF(1, 0, 0, 0), null);

        Assert.AreEqual((0, "RenderTarget3D|Clear"), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenThreeDimensionalRenderTargetBindingFailsThenClearIsSkipped()
    {
        int clearCalls = 0;
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        using Direct3D9Device device = CreateDevice(
            _ => Direct3D9Factory.GenericFailureHResult,
            (_, _, _, _, _) =>
            {
                clearCalls++;
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D,
            begin3DInternal: (_, _, _, requested) => new Direct3D9Begin3DResult(0, requested),
            initialBounds: new Direct3D9SurfaceRect(0, 0, 16, 16),
            width: 16,
            height: 16);
        _ = renderTarget.Begin3D(new MilRectF(0, 0, 16, 16), MilAntiAliasMode.None, false, 1);

        int result = renderTarget.Clear(new MilColorF(1, 0, 0, 0), null);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, clearCalls));
    }

    [TestMethod]
    public unsafe void WhenAlphaLayerEndsThenProductionStatePathBindsTwoDimensionalTargetBeforeComposite()
    {
        List<string> calls = [];
        using Direct3D9Surface surface2D = CreateSurface();
        using Direct3D9Surface surface3D = CreateSurface();
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, surface2D));
                return 0;
            },
            getRenderTargetDescription: _ => new SurfaceDesc(width: 16, height: 16),
            setRenderTarget: surface =>
            {
                calls.Add(ReferenceEquals(surface, surface2D) ? "Target2D" : "Target3D");
                return 0;
            },
            setViewport: _ =>
            {
                calls.Add("Clip");
                return 0;
            },
            setSurfaceToClippingMatrix: _ => 0,
            setDepthStencilSurface: _ =>
            {
                calls.Add("2D");
                return 0;
            },
            setRenderState: (_, _) => 0,
            setTransform: (_, _) => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface2D,
            renderTargetSurfaceFor3D: surface3D,
            pixelFormat: MilPixelFormat.Pbgra32Bpp,
            width: 16,
            height: 16);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        calls.Clear();
        Direct3D9SurfaceRect bounds = new(1, 2, 9, 10);

        int result = renderTarget.EndLayerInternal(
            new Direct3D9LayerEndState(bounds, bounds, 41),
            (bitmap, layerBounds, compositingMode) =>
            {
                calls.Add($"Composite:{bitmap}:{layerBounds}:{compositingMode}");
                return 0;
            });

        Assert.AreEqual(
            (0, $"Target2D|Clip|Clip|2D|Composite:41:{bounds}:{MilCompositingMode.SourceUnder}"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionLayerTargetBindingFailsThenLaterStateAndCompositeAreSkipped()
    {
        int laterCalls = 0;
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9Device device = CreateDevice(_ => Direct3D9Factory.GenericFailureHResult);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(8, 8));

        int result = renderTarget.EndLayerInternal(
            new Direct3D9LayerEndState(
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                43),
            (_, _, _) => ++laterCalls);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0), (result, laterCalls));
    }

    [TestMethod]
    public unsafe void WhenNoRenderTargetSurfaceExistsThenBindingThrowsInvalidOperationException()
    {
        using Direct3D9Device device = CreateDevice(_ => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone);

        Assert.ThrowsExactly<InvalidOperationException>(() => renderTarget.SetAsRenderTarget());
    }

    [TestMethod]
    public unsafe void WhenNoThreeDimensionalRenderTargetSurfaceExistsThenExplicitBindingThrowsInvalidOperationException()
    {
        using Direct3D9Device device = CreateDevice(_ => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone);

        Assert.ThrowsExactly<InvalidOperationException>(() => renderTarget.SetAsRenderTargetFor3D());
    }

    [TestMethod]
    public unsafe void WhenDisposedRepeatedlyThenBindingRenderTargetThrows()
    {
        using Direct3D9Device device = CreateDevice(_ => 0);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.SetAsRenderTarget());
    }

    [TestMethod]
    public unsafe void WhenDisposedRepeatedlyThenExplicitThreeDimensionalBindingThrows()
    {
        using Direct3D9Device device = CreateDevice(_ => 0);
        using Direct3D9Surface surface = CreateSurface();
        Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface);
        renderTarget.Dispose();
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.SetAsRenderTargetFor3D());
    }

    private static unsafe Direct3D9Device CreateDevice(
        Func<Direct3D9Surface, int> setRenderTarget,
        Direct3D9Clear? clear = null)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? createdSwapChain) =>
            {
                createdSwapChain = new Direct3D9SwapChain(
                    new Direct3D9ResourceManager(),
                    null,
                    _ => (0, CreateSurface()));
                return 0;
            },
            getRenderTargetDescription: _ => new SurfaceDesc(width: 16, height: 16),
            setRenderTarget: setRenderTarget,
            setViewport: _ => 0,
            clear: clear,
            setSurfaceToClippingMatrix: _ => 0);
    }

    private static unsafe Direct3D9Surface CreateSurface()
    {
        return new Direct3D9Surface(new Direct3D9ResourceManager(), null);
    }
}
