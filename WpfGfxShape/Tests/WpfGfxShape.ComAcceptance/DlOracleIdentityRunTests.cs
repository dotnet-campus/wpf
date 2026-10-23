#if DK_X64_FAMILY
namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class DlOracleIdentityRunTests
{
    /// <summary>Repeats the original DK families plus the default dual-module regression within one bounded supervisor.</summary>
    [TestMethod]
    public async Task WhenOriginalFamilyIsSelectedThenIdentityRegressionRunsAsync()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "dl-oracle-identity");
        string[] families = ["BwCurvedFillPrecisionTests.", "CkFillIntegerTests.", "BrFillIntegerTests.", "GeometryUtilityTests.", "IndependentRealizerAbiTests.", "IndependentBitmapRealizerAbiTests.", "BitmapRenderTargetTests.WhenBitmapCopyBufferIsShort", "BitmapRenderTargetTests.WhenNativePathHasNoBrush", "BitmapRenderTargetTests.WhenNativeBitmapClipIsEmpty", "BitmapRenderTargetTests.WhenNativeOpaqueLayerClears", "DkX64FamilyTests.WhenPackageIdentityIsSelected", "OfficialRasterModuleTests.WhenSameNamedLibrariesAreLoaded"];
        string filter = string.Join("|", families.Select(name => "FullyQualifiedName~" + name));
        int exit = await ExitDiagnostics.RunAsync([root, "120", filter]);
        Assert.AreEqual(0, exit, root);
    }
}
#endif
