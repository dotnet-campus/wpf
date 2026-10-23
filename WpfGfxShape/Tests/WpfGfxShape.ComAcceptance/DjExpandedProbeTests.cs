#if DJ_EXPANDED_PROBE
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class DjExpandedProbeTests
{
    /// <summary>Runs the bounded filtered supervisor with an absolute results directory inside this task.</summary>
    [TestMethod]
    public async Task WhenDjSupervisionIsSelectedThenBoundedBusinessSmokeRunsAsync()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "dj-x64");
        string filter = "FullyQualifiedName~BwCurvedFillPrecisionTests|FullyQualifiedName~AcceptancePathsTests.WhenSandboxPath|FullyQualifiedName~AcceptancePathsTests.WhenLocalPath|FullyQualifiedName~AcceptancePathsTests.WhenArtifactIsValidated|FullyQualifiedName~ExitDiagnosticsTests.WhenLegacyFilteredBudget|FullyQualifiedName~ExitDiagnosticsTests.WhenBudgetIsOutsideModeBounds|FullyQualifiedName~DjExpandedProbeTests.WhenExpandedProbeRuns";
        int exit = await ExitDiagnostics.RunAsync([root, "120", filter]);
        Assert.AreEqual(0, exit, root);
    }

    /// <summary>Records the clean package and installed WPF identities for the explicitly selected DJ probe.</summary>
    [TestMethod]
    public void WhenExpandedProbeRunsThenActualPackageAndWpfIdentitiesAreRecorded()
    {
        string root = AppContext.BaseDirectory;
        var files = File.ReadAllLines(Path.Combine(root, "package-files.txt"))
            .Select(relative => new { Path = relative, Bytes = new FileInfo(Path.Combine(root, relative)).Length }).ToArray();
        string wpf = typeof(System.Windows.Media.Visual).Assembly.Location;
        string official = AcceptancePaths.OriginalDll;
        string assembly = typeof(DjExpandedProbeTests).Assembly.Location;
        object Identity(string path) => new
        {
            Path = path,
            Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
            Version = FileVersionInfo.GetVersionInfo(path).FileVersion
        };
        Directory.CreateDirectory(Path.Combine(root, "Results"));
        File.WriteAllText(Path.Combine(root, "Results", "dj-package-identity.json"), JsonSerializer.Serialize(new
        {
            Runtime = RuntimeInformation.FrameworkDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            Test = Identity(assembly),
            TestMvid = typeof(DjExpandedProbeTests).Module.ModuleVersionId,
            Wpf = Identity(wpf),
            Official = Identity(official),
            Candidate = Identity(AcceptancePaths.AotDll),
            ValidatedCandidateSha256 = AcceptancePaths.ValidateIdentity(),
            TotalBytes = files.Sum(file => file.Bytes),
            FileCount = files.Length,
            Files = files
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.AreEqual("F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7", AcceptancePaths.ValidateIdentity());
    }
}
#endif
