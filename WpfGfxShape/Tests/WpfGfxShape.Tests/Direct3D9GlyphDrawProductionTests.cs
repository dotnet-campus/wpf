using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenProductionGlyphHardwareDrawSucceedsThenRendererIsReleasedAfterPaint()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            CreateProductionGlyphOperations(calls));

        Assert.AreEqual(
            (0, "Realize|Ensure|Create:True|Paint|ReleaseRenderer"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionGlyphHardwareIsIneligibleThenSoftwareBrushFallbackAndAlphaAreUsed()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];

        int result = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(
                TargetSupportsClearType: false,
                CanDrawText: false),
            CreateProductionGlyphOperations(calls));

        Assert.AreEqual(
            (0, "Ensure|SoftwareRealize|Fallback:-2003304312|SoftwareDraw:False:0.5"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionGlyphHardwareReturnsNotImplementedThenRendererIsReleasedBeforeSoftwareFallback()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        Direct3D9ProductionGlyphDrawOperations operations = CreateProductionGlyphOperations(
            calls,
            paintResult: Direct3D9Factory.NotImplementedHResult);

        int result = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            operations);

        Assert.AreEqual(
            (0, "Realize|Ensure|Create:True|Paint|ReleaseRenderer|SoftwareRealize|Fallback:-2147467263|SoftwareDraw:True:0.5"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenProductionGlyphRunIsEmptyThenNoResourcesAreCreated()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        List<string> calls = [];
        Direct3D9ProductionGlyphDrawOperations operations = CreateProductionGlyphOperations(calls) with { HasGlyphs = false };

        int result = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            operations);

        Assert.AreEqual((0, string.Empty), (result, string.Join('|', calls)));
    }

    private static Direct3D9ProductionGlyphDrawOperations CreateProductionGlyphOperations(
        List<string> calls,
        int paintResult = Direct3D9Factory.SuccessHResult) => new(
        (out Direct3D9RealizedGlyphBrushState state) =>
        {
            calls.Add("Realize");
            state = new Direct3D9RealizedGlyphBrushState(HasBrush: true);
            return 0;
        },
        () =>
        {
            calls.Add("Ensure");
            return 0;
        },
        (bool supportsClearType, out Direct3D9ProductionGlyphRenderer? renderer) =>
        {
            calls.Add($"Create:{supportsClearType}");
            renderer = new Direct3D9ProductionGlyphRenderer(
                () =>
                {
                    calls.Add("Paint");
                    return paintResult;
                },
                () => calls.Add("ReleaseRenderer"));
            return 0;
        },
        new Direct3D9SoftwareGlyphRenderer(
            (out Direct3D9RealizedSoftwareGlyphBrushState state) =>
            {
                calls.Add("SoftwareRealize");
                state = new Direct3D9RealizedSoftwareGlyphBrushState(HasBrush: true, EffectAlpha: 0.5f);
                return 0;
            },
            reason =>
            {
                calls.Add($"Fallback:{reason}");
                return 0;
            },
            (supportsClearType, alpha) =>
            {
                calls.Add($"SoftwareDraw:{supportsClearType}:{alpha}");
                return 0;
            }));
}