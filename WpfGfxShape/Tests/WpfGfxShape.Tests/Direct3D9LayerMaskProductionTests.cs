using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    [DataRow((int) MilAntiAliasMode.None, false, "SourceOverNonPremultiplied", "Combine|Aliased|Fill:SourceOverNonPremultiplied:0:none:True|Bounds|Effect:0.25|Fill:SourceOverNonPremultiplied:25:none:True")]
    [DataRow((int) MilAntiAliasMode.EightByEight, false, "SourceInverseAlphaOverNonPremultiplied", "Antialiased|Effect:0.75|Fill:SourceInverseAlphaOverNonPremultiplied:75:1,2,9,10:True")]
    [DataRow((int) MilAntiAliasMode.EightByEight, true, "SourceAlphaMultiply", "Antialiased|Effect:0.75|Fill:SourceAlphaMultiply:75:1,2,9,10:True|Bounds|Fill:SourceUnder:0:none:True")]
    public unsafe void WhenLayerGeometricMaskEndsThenAliasedAndAntialiasedFixupsMatchNative(
        int antiAliasMode,
        bool targetHasAlpha,
        string _,
        string expectedDraws)
    {
        using Direct3D9Device device = CreatePresentDevice(static _ => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: targetHasAlpha ? MilPixelFormat.Pbgra32Bpp : MilPixelFormat.Bgr32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        List<string> calls = [];
        Direct3D9LayerMaskOperations maskOperations = CreateLayerMaskOperations(calls);
        Direct3D9LayerOperations operations = CreateLayerOperations(calls, maskOperations: maskOperations);

        Assert.AreEqual(0, renderTarget.BeginLayer(
            new Direct3D9LayerState(
                new Direct3D9SurfaceRect(1, 2, 9, 10),
                new Direct3D9SurfaceRect(1, 2, 9, 10),
                Alpha: 0.75f,
                AntiAliasMode: (MilAntiAliasMode) antiAliasMode,
                GeometricMask: 41),
            operations));

        int result = renderTarget.EndLayer();
        string draws = string.Join('|', calls.Where(static call =>
            call is "Combine" or "Aliased" or "Antialiased" or "Bounds"
            || call.StartsWith("Effect:", StringComparison.Ordinal)
            || call.StartsWith("Fill:", StringComparison.Ordinal)));

        Assert.AreEqual((0, expectedDraws), (result, draws));
    }

    [TestMethod]
    public unsafe void WhenLayerMaskFillFailsThenFirstErrorStopsSourceUnderAndResourcesReleaseInReverseOrder()
    {
        using Direct3D9Device device = CreatePresentDevice(static _ => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        List<string> calls = [];
        Direct3D9LayerMaskOperations maskOperations = CreateLayerMaskOperations(
            calls,
            fillResult: Direct3D9Factory.GenericFailureHResult);

        Assert.AreEqual(0, renderTarget.BeginLayer(
            new Direct3D9LayerState(
                new Direct3D9SurfaceRect(1, 1, 8, 8),
                new Direct3D9SurfaceRect(1, 1, 8, 8),
                Alpha: 0.5f,
                AntiAliasMode: MilAntiAliasMode.EightByEight,
                GeometricMask: 51),
            CreateLayerOperations(calls, maskOperations: maskOperations)));

        int result = renderTarget.EndLayer();

        Assert.AreEqual(
            $"{Direct3D9Factory.GenericFailureHResult}|RetainGeometry:51|Capture:101|Clear:1,1,8,8|Restore:1,1,8,8|Mask:51|Antialiased|Effect:0.5|Brush:Black|Fill:SourceAlphaMultiply:50:1,1,8,8:True|ReleaseBrush:Black|ReleaseEffect:0.5|ReleaseGenerator:Antialiased|ReleaseMask:51|ReleaseSource:101|ReleaseGeometry:51",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public unsafe void WhenLayerMaskIsEmptyThenItIsNoRenderAndAlphaTargetStillRestoresSavedSourceUnder()
    {
        using Direct3D9Device device = CreatePresentDevice(static _ => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        List<string> calls = [];
        Direct3D9LayerMaskOperations maskOperations = CreateLayerMaskOperations(calls, emptyMask: true);

        Assert.AreEqual(0, renderTarget.BeginLayer(
            new Direct3D9LayerState(
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                new Direct3D9SurfaceRect(0, 0, 8, 8),
                AntiAliasMode: MilAntiAliasMode.EightByEight,
                GeometricMask: 61),
            CreateLayerOperations(calls, maskOperations: maskOperations)));

        int result = renderTarget.EndLayer();

        Assert.AreEqual(
            "0|Antialiased|Bounds|Brush:Source|Fill:SourceUnder:0:none:True",
            $"{result}|{string.Join('|', calls.Where(static call => call is "Antialiased" or "Bounds" || call.StartsWith("Brush:", StringComparison.Ordinal) || call.StartsWith("Fill:", StringComparison.Ordinal)))}");
    }

    [TestMethod]
    public unsafe void WhenLayerHasAlphaMaskAndOpacityThenEffectsAreConsumedBeforeAlphaTargetSourceUnder()
    {
        using Direct3D9Device device = CreatePresentDevice(static _ => 0);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            pixelFormat: MilPixelFormat.Pbgra32Bpp);
        Assert.AreEqual(0, renderTarget.Resize(16, 16));
        List<string> calls = [];
        Direct3D9LayerMaskOperations maskOperations = CreateLayerMaskOperations(calls, useEffectFill: true);

        Assert.AreEqual(0, renderTarget.BeginLayer(
            new Direct3D9LayerState(
                new Direct3D9SurfaceRect(1, 2, 9, 10),
                new Direct3D9SurfaceRect(1, 2, 9, 10),
                Alpha: 0.5f,
                AlphaMaskBrush: 71),
            CreateLayerOperations(calls, maskOperations: maskOperations)));

        int result = renderTarget.EndLayer();
        string effectCalls = string.Join('|', calls.Where(static call =>
            call.StartsWith("RetainEffect:", StringComparison.Ordinal)
            || call.StartsWith("EffectFill:", StringComparison.Ordinal)
            || call.StartsWith("ReleaseEffectResource:", StringComparison.Ordinal)));

        Assert.AreEqual(
            "0|RetainEffect:71|EffectFill:SourceAlphaMultiply:AlphaMask,AlphaScale|ReleaseEffectResource:171|EffectFill:SourceUnder:none",
            $"{result}|{effectCalls}");
    }

    private static Direct3D9LayerMaskOperations CreateLayerMaskOperations(
        List<string> calls,
        int fillResult = 0,
        bool emptyMask = false,
        bool useEffectFill = false)
    {
        int CreateGenerator(string name, out Direct3D9PathGeometryGenerator? generator)
        {
            calls.Add(name);
            if (emptyMask && name == "Antialiased")
            {
                generator = null;
                return Direct3D9Factory.EmptyFillHResult;
            }

            generator = new Direct3D9PathGeometryGenerator(
                static _ => 0,
                () => calls.Add($"ReleaseGenerator:{name}"));
            return 0;
        }

        return new Direct3D9LayerMaskOperations(
            (nint mask, out Direct3D9LayerMaskShape? shape) =>
            {
                calls.Add($"Mask:{mask}");
                shape = new Direct3D9LayerMaskShape(mask, () => calls.Add($"ReleaseMask:{mask}"));
                return 0;
            },
            (Direct3D9LayerMaskShape _, Direct3D9SurfaceRect _, out Direct3D9PathGeometryGenerator? generator) =>
                CreateGenerator("Antialiased", out generator),
            (Direct3D9SurfaceRect _, Direct3D9LayerMaskShape _, out Direct3D9LayerMaskShape? shape) =>
            {
                calls.Add("Combine");
                shape = new Direct3D9LayerMaskShape(91, () => calls.Add("ReleaseComplement"));
                return 0;
            },
            (Direct3D9LayerMaskShape _, Direct3D9SurfaceRect _, out Direct3D9PathGeometryGenerator? generator) =>
                CreateGenerator("Aliased", out generator),
            (Direct3D9LayerMaskShape _, Direct3D9SurfaceRect _, out Direct3D9PathGeometryGenerator? generator) =>
                CreateGenerator("Bounds", out generator),
            (nint _, bool black, out Direct3D9PathHardwareBrush? brush) =>
            {
                string name = black ? "Black" : "Source";
                calls.Add($"Brush:{name}");
                brush = new Direct3D9PathHardwareBrush(
                    static (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
                    {
                        description = null;
                        return Direct3D9Factory.NotImplementedHResult;
                    },
                    static (MilCompositingMode _, Direct3D9PathHardwareBrush _, nint _, Direct3D9PathBrushContext _, out Direct3D9PathPipelineDescription? description) =>
                    {
                        description = null;
                        return Direct3D9Factory.NotImplementedHResult;
                    },
                    () => calls.Add($"ReleaseBrush:{name}"));
                return 0;
            },
            (float alpha, out Direct3D9LayerEffectList? effects) =>
            {
                calls.Add($"Effect:{alpha}");
                effects = new Direct3D9LayerEffectList((nint) (int) (alpha * 100), () => calls.Add($"ReleaseEffect:{alpha}"));
                return 0;
            },
            (MilCompositingMode mode, Direct3D9PathGeometryGenerator _, Direct3D9PathHardwareBrush _, nint effects, Direct3D9PathBrushContext _, Direct3D9SurfaceRect? bounds, bool inside) =>
            {
                string formattedBounds = bounds is null
                    ? "none"
                    : $"{bounds.Value.Left},{bounds.Value.Top},{bounds.Value.Right},{bounds.Value.Bottom}";
                calls.Add($"Fill:{mode}:{effects}:{formattedBounds}:{inside}");
                return fillResult;
            },
            useEffectFill
                ? (nint resource, out nint retained) =>
                {
                    calls.Add($"RetainEffect:{resource}");
                    retained = resource + 100;
                    return 0;
                }
                : null,
            useEffectFill ? resource => calls.Add($"ReleaseEffectResource:{resource}") : null,
            useEffectFill
                ? (MilCompositingMode mode, Direct3D9PathGeometryGenerator _, Direct3D9PathHardwareBrush _, Direct3D9EffectList? effects, Direct3D9PathBrushContext _, Direct3D9SurfaceRect? _, bool _) =>
                {
                    string entries = effects is null ? "none" : string.Join(',', effects.Entries.Select(static entry => entry.Type));
                    calls.Add($"EffectFill:{mode}:{entries}");
                    return fillResult;
                }
                : null);
    }
}
