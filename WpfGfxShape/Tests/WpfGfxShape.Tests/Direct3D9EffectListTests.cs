using System.Numerics;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public class Direct3D9EffectListTests
{
    [TestMethod]
    public void WhenEffectsAreAddedThenOrderParametersAndResourceLifetimeMatchNative()
    {
        List<string> calls = [];
        using Direct3D9EffectList effects = new();

        Assert.AreEqual(0, effects.AddAlphaMask(
            new Direct3D9AlphaMaskParameters(Matrix4x4.CreateTranslation(2, 3, 0)),
            17,
            (nint resource, out nint retained) =>
            {
                calls.Add($"Retain:{resource}");
                retained = 19;
                return 0;
            },
            resource => calls.Add($"Release:{resource}")));
        Assert.AreEqual(0, effects.AddAlphaScale(0.25f));

        Assert.AreEqual(
            "AlphaMask:19|AlphaScale:0.25|Retain:17",
            $"{effects.Entries[0].Type}:{effects.Entries[0].Resources[0].Handle}|{effects.Entries[1].Type}:{effects.Entries[1].Parameters}|{string.Join('|', calls)}");

        effects.Clear();
        Assert.AreEqual("Retain:17|Release:19", string.Join('|', calls));
    }

    [TestMethod]
    public void WhenShaderEffectsAreProcessedThenAlphaMaskPrecedesAlphaScaleAndOwnershipTransfersToItems()
    {
        List<string> calls = [];
        using Direct3D9EffectList effects = CreateEffects(calls);
        using Direct3D9ShaderPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(CreateBitmapColorSource("Primary", calls)));

        int result = Direct3D9EffectProcessor.Process(
            effects,
            builder,
            CreateContext(),
            (nint mask, Direct3D9AlphaMaskParameters parameters, Direct3D9PathBrushContext _, out Direct3D9BitmapPipelineColorSource? source) =>
            {
                calls.Add($"Derive:{mask}:{parameters.Transform.M41},{parameters.Transform.M42}");
                source = CreateBitmapColorSource("Mask", calls);
                return 0;
            });
        IReadOnlyList<Direct3D9ShaderPipelineItem> items = builder.DetachItems();
        Direct3D9ShaderPipelineItemBuilder.ReleaseOwnedColorSources(items);

        Assert.AreEqual(
            "0|Derive:23:4,5|Dispose:Mask|ReleaseColor:Mask|Dispose:Primary|ReleaseColor:Primary",
            $"{result}|{string.Join('|', calls.Where(static call => call.StartsWith("Derive:", StringComparison.Ordinal) || call.StartsWith("ReleaseColor:", StringComparison.Ordinal) || call.StartsWith("Dispose:", StringComparison.Ordinal)))}");
    }

    [TestMethod]
    public void WhenFixedFunctionEffectsAreProcessedThenAlphaMaskAndAlphaScaleUseSharedBuilderOrder()
    {
        List<string> calls = [];
        using Direct3D9EffectList effects = CreateEffects(calls);
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(CreateBitmapColorSource("Primary", calls)));

        int result = Direct3D9EffectProcessor.Process(
            effects,
            builder,
            CreateContext(),
            (nint mask, Direct3D9AlphaMaskParameters _, Direct3D9PathBrushContext _, out Direct3D9BitmapPipelineColorSource? source) =>
            {
                calls.Add($"Derive:{mask}");
                source = CreateBitmapColorSource("Mask", calls);
                return 0;
            });
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items = builder.DetachItems();
        string operations = string.Join(',', items.Select(static item => item.BlendOperation));
        Direct3D9FixedFunctionPipelineItemBuilder.ReleaseOwnedColorSources(items);

        Assert.AreEqual((0, "SelectSource,MultiplyByAlpha,Multiply", "Derive:23"),
            (result, operations, string.Join('|', calls.Where(static call => call.StartsWith("Derive:", StringComparison.Ordinal)))));
    }

    [TestMethod]
    public void WhenAlphaMaskDerivationFailsThenFirstErrorStopsFollowingAlphaScale()
    {
        using Direct3D9EffectList effects = new();
        Assert.AreEqual(0, effects.AddAlphaMask(
            new Direct3D9AlphaMaskParameters(Matrix4x4.Identity),
            29,
            static (nint resource, out nint retained) => { retained = resource; return 0; },
            static _ => { }));
        Assert.AreEqual(0, effects.AddAlphaScale(0.5f));
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new();
        Assert.AreEqual(0, builder.SetTexture(new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, static () => 0)));

        int result = Direct3D9EffectProcessor.Process(
            effects,
            builder,
            CreateContext(),
            static (nint _, Direct3D9AlphaMaskParameters _, Direct3D9PathBrushContext _, out Direct3D9BitmapPipelineColorSource? source) =>
            {
                source = null;
                return Direct3D9Factory.GenericFailureHResult;
            });

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 1), (result, builder.DetachItems().Count));
    }

    private static Direct3D9EffectList CreateEffects(List<string> calls)
    {
        Direct3D9EffectList effects = new();
        Assert.AreEqual(0, effects.AddAlphaMask(
            new Direct3D9AlphaMaskParameters(Matrix4x4.CreateTranslation(4, 5, 0)),
            23,
            static (nint resource, out nint retained) => { retained = resource; return 0; },
            resource => calls.Add($"ReleaseEffectResource:{resource}")));
        Assert.AreEqual(0, effects.AddAlphaScale(0.5f));
        return effects;
    }

    private static Direct3D9PathBrushContext CreateContext() => new(
        Matrix4x4.Identity,
        new Direct3D9SurfaceRect(0, 0, 8, 8),
        new MilRectF(0, 0, 8, 8),
        CanFallback: false);

    private static Direct3D9BitmapPipelineColorSource CreateBitmapColorSource(string name, List<string> calls) => new(
        name == "Primary" ? 31 : 37,
        new CallbackDisposable(() => calls.Add($"Dispose:{name}")),
        new Direct3D9PipelineColorSource(Direct3D9ColorSourceType.Texture, () => 0),
        _ => calls.Add($"ReleaseColor:{name}"));

    private sealed class CallbackDisposable(Action callback) : IDisposable
    {
        public void Dispose() => callback();
    }
}
