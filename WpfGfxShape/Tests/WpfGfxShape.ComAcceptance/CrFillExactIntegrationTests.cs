#if CQ_FILL_EXACT_DIAGNOSTICS
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class CrFillExactIntegrationTests
{
    /// <summary>Checks the retained CF candidate against CR using the unchanged CQ complete-buffer stroke contract.</summary>
    [TestMethod]
    public async Task WhenCfAndCrRenderStrokeThenCompleteBuffersAreUnchanged()
    {
        string architecture = RuntimeInformation.ProcessArchitecture == Architecture.X86 ? "x86" : "x64";
        string baseline = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..",
            "cf-curved-precision", "default-" + architecture, "com-acceptance-package", "Native", "wpfgfx_cor3.dll"));
        Assert.IsTrue(File.Exists(baseline), baseline);
        string? previous = Environment.GetEnvironmentVariable("CQ_BASELINE_DLL");
        Environment.SetEnvironmentVariable("CQ_BASELINE_DLL", baseline);
        try
        {
            await new CqFillExactMatrixTests().WhenHistoricalStrokeIsRenderedThenOldAndNewBuffersAreUnchanged();
        }
        finally
        {
            Environment.SetEnvironmentVariable("CQ_BASELINE_DLL", previous);
        }
    }
}
#endif
