using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9ImmediateBrushRealizerTests
{
    [TestMethod]
    public void WhenBrushIsSetThenBrushAndEffectsAreRetained()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);

        realizer.SetBrush(3, 5, skipMetaFixups: true);

        Assert.AreEqual(((nint) 3, (nint) 5, "Color:11:0:0:0:0|AddRef:3|AddRef:5"),
            (realizer.GetRealizedBrush(false), realizer.Effects, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenNullBrushIsRequestedAsTransparentThenEmbeddedSolidBrushIsReturned()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);

        nint brush = realizer.GetRealizedBrush(convertNullToTransparent: true);

        Assert.AreEqual((nint) 11, brush);
    }

    [TestMethod]
    public void WhenSolidColorIsSetThenEmbeddedBrushIsUpdatedAndRetained()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);

        realizer.SetSolidColor(new MilColorF(1, 0.25f, 0.5f, 0.75f));

        Assert.AreEqual(
            ((nint) 11, "Color:11:0:0:0:0|Color:11:1:0.25:0.5:0.75|AddRef:11"),
            (realizer.GetRealizedBrush(false), string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenMetaFixupsAreSkippedThenEnsureRealizationDoesNothing()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);
        realizer.SetBrush(3, 5, skipMetaFixups: true);
        calls.Clear();

        int result = realizer.EnsureRealization(7, 13);

        Assert.AreEqual((0, string.Empty), (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenMetaIntermediatesExistThenBrushIsReplacedBeforeEffect()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);
        realizer.SetBrush(3, 5, skipMetaFixups: false);
        calls.Clear();

        int result = realizer.EnsureRealization(7, 13);

        Assert.AreEqual(
            (0, "ReplaceBrush:3:17:7:13|ReplaceEffect:5:19:7:13"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenCacheIndexIsInvalidThenSoftwareIndexIsUsed()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);
        realizer.SetBrush(3, 5, skipMetaFixups: false);
        calls.Clear();

        int result = realizer.EnsureRealization(Direct3D9ImmediateBrushRealizer.InvalidRealizationCacheIndex, 13);

        Assert.AreEqual(
            "ReplaceBrush:3:17:0:13|ReplaceEffect:5:19:0:13",
            string.Join('|', calls));
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void WhenBrushReplacementFailsThenEffectReplacementIsSkipped()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(
            calls,
            replaceBrushMetaIntermediate: (brush, meta, cacheIndex, destination) =>
            {
                calls.Add($"ReplaceBrush:{brush}:{meta}:{cacheIndex}:{destination}");
                return Direct3D9Factory.GenericFailureHResult;
            });
        realizer.SetBrush(3, 5, skipMetaFixups: false);
        calls.Clear();

        int result = realizer.EnsureRealization(7, 13);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "ReplaceBrush:3:17:7:13"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenEffectReplacementFailsThenFailureIsPreserved()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(
            calls,
            replaceEffectMetaIntermediate: (effects, meta, cacheIndex, destination) =>
            {
                calls.Add($"ReplaceEffect:{effects}:{meta}:{cacheIndex}:{destination}");
                return Direct3D9Factory.GenericFailureHResult;
            });
        realizer.SetBrush(3, 5, skipMetaFixups: false);
        calls.Clear();

        int result = realizer.EnsureRealization(7, 13);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "ReplaceBrush:3:17:7:13|ReplaceEffect:5:19:7:13"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenReleasedThenMetaIntermediatesAreRestoredBeforeOwnedReferences()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);
        realizer.SetBrush(3, 5, skipMetaFixups: false);
        calls.Clear();

        realizer.Release();
        realizer.Release();

        Assert.AreEqual(
            "RestoreBrush:3:17|RestoreEffect:5:19|Release:5|Release:3",
            string.Join('|', calls));
    }

    [TestMethod]
    public void WhenBrushIsBitmapThenTilingAndSourceClipQueriesUseTheBrush()
    {
        List<string> calls = [];
        Direct3D9ImmediateBrushRealizer realizer = CreateRealizer(calls);
        realizer.SetBrush(3, 0, skipMetaFixups: true);

        bool mayTile = realizer.RealizedBrushMayNeedNonPow2Tiling();
        bool hasClip = realizer.RealizedBrushWillHaveSourceClip();
        bool entireSource = realizer.RealizedBrushSourceClipMayBeEntireSource();

        Assert.AreEqual((true, true, true), (mayTile, hasClip, entireSource));
    }

    private static Direct3D9ImmediateBrushRealizer CreateRealizer(
        List<string> calls,
        Direct3D9ReplaceMetaIntermediate? replaceBrushMetaIntermediate = null,
        Direct3D9ReplaceMetaIntermediate? replaceEffectMetaIntermediate = null)
    {
        return new Direct3D9ImmediateBrushRealizer(
            11,
            value => calls.Add($"AddRef:{value}"),
            value => calls.Add($"Release:{value}"),
            (brush, color) => calls.Add($"Color:{brush}:{color.Alpha}:{color.Red}:{color.Green}:{color.Blue}"),
            _ => true,
            _ => Direct3D9BrushType.Bitmap,
            _ => true,
            _ => true,
            _ => 17,
            _ => 19,
            replaceBrushMetaIntermediate ?? ((brush, meta, cacheIndex, destination) =>
            {
                calls.Add($"ReplaceBrush:{brush}:{meta}:{cacheIndex}:{destination}");
                return 0;
            }),
            replaceEffectMetaIntermediate ?? ((effects, meta, cacheIndex, destination) =>
            {
                calls.Add($"ReplaceEffect:{effects}:{meta}:{cacheIndex}:{destination}");
                return 0;
            }),
            (brush, meta) => calls.Add($"RestoreBrush:{brush}:{meta}"),
            (effects, meta) => calls.Add($"RestoreEffect:{effects}:{meta}"));
    }
}
