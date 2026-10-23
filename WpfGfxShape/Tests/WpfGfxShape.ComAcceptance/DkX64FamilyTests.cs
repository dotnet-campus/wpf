#if DK_X64_FAMILY
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class DkX64FamilyTests
{
    /// <summary>Runs only the explicitly selected existing x64 families under the standard filtered supervisor.</summary>
    [TestMethod]
    public async Task WhenDkFamilyIsSelectedThenBoundedExistingFamiliesRunAsync()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "dk-x64");
        string[] families = ["BwCurvedFillPrecisionTests.", "CkFillIntegerTests.", "BrFillIntegerTests.", "GeometryUtilityTests.", "IndependentRealizerAbiTests.", "IndependentBitmapRealizerAbiTests.", "BitmapRenderTargetTests.WhenBitmapCopyBufferIsShort", "BitmapRenderTargetTests.WhenNativePathHasNoBrush", "BitmapRenderTargetTests.WhenNativeBitmapClipIsEmpty", "BitmapRenderTargetTests.WhenNativeOpaqueLayerClears", "DkX64FamilyTests.WhenPackageIdentityIsSelected"];
        string filter = string.Join("|", families.Select(name => "FullyQualifiedName~" + name));
        int exit = await ExitDiagnostics.RunAsync([root, "120", filter]);
        Assert.AreEqual(0, exit, root);
    }

    /// <summary>Records exact package lengths and installed module identities without changing default discovery.</summary>
    [TestMethod]
    public void WhenPackageIdentityIsSelectedThenActualInputsAreRecorded()
    {
        string root = AppContext.BaseDirectory;
        var files = File.ReadAllLines(Path.Combine(root, "package-files.txt"))
            .Select(relative => new { Path = relative, Bytes = new FileInfo(Path.Combine(root, relative)).Length }).ToArray();
        object Identity(string path) => new
        {
            Path = path,
            Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
            Version = FileVersionInfo.GetVersionInfo(path).FileVersion
        };
        string assembly = typeof(DkX64FamilyTests).Assembly.Location;
        Directory.CreateDirectory(Path.Combine(root, "Results"));
        File.WriteAllText(Path.Combine(root, "Results", "dk-package-identity.json"), JsonSerializer.Serialize(new
        {
            Runtime = RuntimeInformation.FrameworkDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            Test = Identity(assembly), TestMvid = typeof(DkX64FamilyTests).Module.ModuleVersionId,
            Wpf = Identity(typeof(System.Windows.Media.Visual).Assembly.Location),
            Official = Identity(AcceptancePaths.OriginalDll), Candidate = Identity(AcceptancePaths.AotDll),
            ValidatedCandidateSha256 = AcceptancePaths.ValidateIdentity(),
            TotalBytes = files.Sum(file => file.Bytes), FileCount = files.Length, Files = files
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.AreEqual("F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7", AcceptancePaths.ValidateIdentity());
    }
}
#endif
