#if CS_FILL_ACCEPTANCE
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class CrFillExactIntegrationTests
{
    /// <summary>Stages the retained CF binary inside the isolated task without rebuilding either candidate.</summary>
    [TestInitialize]
    public void PrepareRetainedBaseline()
    {
        string target = Path.Combine(AppContext.BaseDirectory, "Old", "Native", "wpfgfx_cor3.dll");
        if (File.Exists(target)) return;
        Assert.AreEqual(Architecture.X64, RuntimeInformation.ProcessArchitecture);
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string source = Path.Combine(directory.FullName, "cr-fill-exact", "baseline-x64", "Native", "wpfgfx_cor3.dll");
            if (File.Exists(source))
            {
                Assert.AreEqual("36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A",
                    Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(source))));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(source, target, overwrite: false);
                return;
            }
            directory = directory.Parent;
        }
        Assert.Fail(target);
    }

    /// <summary>Checks the retained CF candidate against CR using the unchanged CQ complete-buffer stroke contract.</summary>
    [TestMethod]
    public async Task WhenCfAndCrRenderStrokeThenCompleteBuffersAreUnchanged()
    {
        string architecture = RuntimeInformation.ProcessArchitecture == Architecture.X86 ? "x86" : "x64";
        string baseline = Path.Combine(AppContext.BaseDirectory, "Old", "Native", "wpfgfx_cor3.dll");
        Assert.IsTrue(File.Exists(baseline), baseline);
        string oldSha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(baseline)));
        string expectedSha = architecture == "x86"
            ? "C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB"
            : "36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A";
        Assert.AreEqual(expectedSha, oldSha);
        string newSha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(AcceptancePaths.AotDll)));
        Assert.AreNotEqual(oldSha, newSha);
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
