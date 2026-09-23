using System.Numerics;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenTypedGlyphRunDrawsTwiceThenBankMissHitAndPersistentPainterLifecycleMatchNative()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9GlyphBank bank = new(2);
        List<string> calls = [];
        Direct3D9GlyphRun run = CreateGlyphRun(11, useSubpixel: true);
        Direct3D9GlyphRunDrawOperations operations = CreateGlyphOperations(calls, bank);

        int first = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            run,
            operations);
        int second = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: true, CanDrawText: true),
            run,
            operations);

        Assert.AreEqual(
            "0:0|RealizeBrush|Ensure|Create:11:ClearType:True|Paint:11:True:False|ReleasePainter|RealizeBrush|Ensure|Paint:11:True:True|ReleasePainter",
            $"{first}:{second}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenTargetDoesNotSupportClearTypeThenGlyphRunUsesGrayscaleBankKey()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9GlyphBank bank = new(2);
        List<string> calls = [];

        int result = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(TargetSupportsClearType: false, CanDrawText: true),
            CreateGlyphRun(13, useSubpixel: false),
            CreateGlyphOperations(calls, bank));

        Assert.AreEqual(
            "0|Create:13:Grayscale:False|Paint:13:False:False",
            $"{result}|{string.Join('|', calls.Where(static call => call.StartsWith("Create:", StringComparison.Ordinal) || call.StartsWith("Paint:", StringComparison.Ordinal)))}");
    }

    [TestMethod]
    public void WhenGlyphBankExceedsCapacityThenLeastRecentlyUsedEntryIsReleasedAndDeviceInvalidationReleasesRemainingEntries()
    {
        using Direct3D9GlyphBank bank = new(2);
        List<string> calls = [];
        Direct3D9GlyphRun first = CreateGlyphRun(1, useSubpixel: false);
        Direct3D9GlyphRun second = CreateGlyphRun(2, useSubpixel: false);
        Direct3D9GlyphRun third = CreateGlyphRun(3, useSubpixel: false);

        Assert.AreEqual(0, bank.GetOrCreate(first, CreateKey(1, device: 7), Create, out _));
        Assert.AreEqual(0, bank.GetOrCreate(second, CreateKey(2, device: 7), Create, out _));
        Assert.AreEqual(0, bank.GetOrCreate(first, CreateKey(1, device: 7), Create, out _));
        Assert.AreEqual(0, bank.GetOrCreate(third, CreateKey(3, device: 7), Create, out _));
        bank.InvalidateDevice(7);

        Assert.AreEqual("Create:1|Create:2|Create:3|Release:2|Release:1|Release:3", string.Join('|', calls));

        int Create(Direct3D9GlyphRun run, Direct3D9GlyphBankKey _, out Direct3D9GlyphRealization? realization)
        {
            calls.Add($"Create:{run.CacheKey}");
            realization = new Direct3D9GlyphRealization(false, () => calls.Add($"Release:{run.CacheKey}"));
            return 0;
        }
    }

    [TestMethod]
    public unsafe void WhenTypedGlyphRunIsInvalidOrEmptyThenNoBrushBankOrPainterResourcesAreUsed()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9GlyphBank bank = new(2);
        List<string> calls = [];
        Direct3D9GlyphRun invalid = CreateGlyphRun(17, useSubpixel: false) with { Advances = [float.NaN] };
        Direct3D9GlyphRun empty = CreateGlyphRun(19, useSubpixel: false) with
        {
            GlyphIndices = [],
            Advances = [],
            Offsets = [],
        };
        Direct3D9GlyphRunDrawOperations operations = CreateGlyphOperations(calls, bank);

        int invalidResult = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(true, true), invalid, operations);
        int emptyResult = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(true, true), empty, operations);

        Assert.AreEqual((Direct3D9Factory.InvalidArgumentHResult, 0, string.Empty),
            (invalidResult, emptyResult, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenTypedGlyphBoundsAreClippedOutThenNoBrushBankOrPainterResourcesAreUsed()
    {
        using Direct3D9Device device = CreatePresentDevice(static _ => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        using Direct3D9GlyphBank bank = new(2);
        List<string> calls = [];
        Direct3D9GlyphRun run = CreateGlyphRun(21, useSubpixel: false) with { Bounds = new MilRectF(20, 20, 30, 30) };

        int result = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(true, true),
            run,
            CreateGlyphOperations(calls, bank));

        Assert.AreEqual((0, string.Empty), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public unsafe void WhenTypedGlyphHardwareIsUnsupportedThenPainterIsNotCreatedAndSoftwareFallbackReceivesReason()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9GlyphBank bank = new(2);
        List<string> calls = [];
        Direct3D9GlyphRunDrawOperations operations = CreateGlyphOperations(calls, bank) with
        {
            CreateRealization = (Direct3D9GlyphRun _, Direct3D9GlyphBankKey _, out Direct3D9GlyphRealization? realization) =>
            {
                realization = null;
                calls.Add("Unsupported");
                return Direct3D9Factory.NotImplementedHResult;
            },
        };

        int result = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(true, true),
            CreateGlyphRun(23, useSubpixel: false),
            operations);

        Assert.AreEqual(
            "0|RealizeBrush|Ensure|Unsupported|SoftwareRealize|Fallback:-2147467263|SoftwareDraw:True:0.5",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenTypedGlyphPaintFailsThenPainterIsReleasedAndSoftwareFallbackIsNotUsed()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        using Direct3D9GlyphBank bank = new(2);
        List<string> calls = [];
        Direct3D9GlyphRunDrawOperations operations = CreateGlyphOperations(calls, bank) with
        {
            PaintRealization = (Direct3D9GlyphRun _, Direct3D9GlyphRealization _, bool _) =>
            {
                calls.Add("PaintFailure");
                return Direct3D9Factory.GenericFailureHResult;
            },
        };

        int result = renderTarget.ProductionDrawGlyphs(
            new Direct3D9GlyphDrawState(true, true),
            CreateGlyphRun(27, useSubpixel: false),
            operations);

        Assert.AreEqual(
            $"{Direct3D9Factory.GenericFailureHResult}|RealizeBrush|Ensure|Create:27:ClearType:False|PaintFailure|ReleasePainter",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenGlyphRealizationCreationFailsAfterReturningResourceThenResourceIsReleasedAndNotCached()
    {
        using Direct3D9GlyphBank bank = new(1);
        List<string> calls = [];

        int result = bank.GetOrCreate(
            CreateGlyphRun(29, useSubpixel: false),
            CreateKey(29, device: 9),
            (Direct3D9GlyphRun _, Direct3D9GlyphBankKey _, out Direct3D9GlyphRealization? realization) =>
            {
                realization = new Direct3D9GlyphRealization(false, () => calls.Add("ReleasePartial"));
                return Direct3D9Factory.GenericFailureHResult;
            },
            out Direct3D9GlyphRealization? realization);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, null, 0, "ReleasePartial"),
            (result, realization, bank.Count, string.Join('|', calls)));
    }

    private static Direct3D9GlyphRun CreateGlyphRun(long key, bool useSubpixel) => new(
        key,
        FontFace: 5,
        EmSize: 12,
        GlyphIndices: [7],
        Advances: [8],
        Offsets: [new Direct3D9GlyphOffset(0.25f, -0.5f)],
        GlyphToDevice: Matrix3x2.Identity,
        Bounds: new MilRectF(0, 0, 8, 12),
        UseSubpixelPositioning: useSubpixel);

    private static Direct3D9GlyphBankKey CreateKey(long runKey, nint device) => new(
        device,
        DisplayIndex: 0,
        runKey,
        Direct3D9GlyphBlendMode.Grayscale,
        UseSubpixelPositioning: false,
        ScaleX: 1,
        ScaleY: 1);

    private static Direct3D9GlyphRunDrawOperations CreateGlyphOperations(
        List<string> calls,
        Direct3D9GlyphBank bank) => new(
        (out Direct3D9RealizedGlyphBrushState state) =>
        {
            calls.Add("RealizeBrush");
            state = new Direct3D9RealizedGlyphBrushState(true);
            return 0;
        },
        () =>
        {
            calls.Add("Ensure");
            return 0;
        },
        bank,
        (Direct3D9GlyphRun run, Direct3D9GlyphBankKey key, out Direct3D9GlyphRealization? realization) =>
        {
            calls.Add($"Create:{run.CacheKey}:{key.BlendMode}:{key.UseSubpixelPositioning}");
            realization = new Direct3D9GlyphRealization(false, () => calls.Add($"ReleaseRealization:{run.CacheKey}"));
            return 0;
        },
        (run, realization, clearType) =>
        {
            calls.Add($"Paint:{run.CacheKey}:{clearType}:{realization.IsPersistent}");
            return 0;
        },
        () => calls.Add("ReleasePainter"),
        new Direct3D9SoftwareGlyphRenderer(
            (out Direct3D9RealizedSoftwareGlyphBrushState state) =>
            {
                calls.Add("SoftwareRealize");
                state = new Direct3D9RealizedSoftwareGlyphBrushState(true, 0.5f);
                return 0;
            },
            reason =>
            {
                calls.Add($"Fallback:{reason}");
                return 0;
            },
            (clearType, alpha) =>
            {
                calls.Add($"SoftwareDraw:{clearType}:{alpha}");
                return 0;
            }),
        DeviceIdentity: 41,
        DisplayIndex: 3,
        RecommendedBlendMode: Direct3D9GlyphBlendMode.ClearType);
}
