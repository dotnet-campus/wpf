using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static class OfficialRasterModule
{
    internal static ProcessModule Find(Process process) => process.Modules.Cast<ProcessModule>().Single(module =>
        string.Equals(Path.GetFullPath(module.FileName),
            Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.Visual).Assembly.Location)!, "wpfgfx_cor3.dll")),
            StringComparison.OrdinalIgnoreCase));
}

[TestClass]
public sealed class OfficialRasterModuleTests
{
    /// <summary>Rejects basename ambiguity by binding the installed official module while both libraries are loaded.</summary>
    [TestMethod]
    public void WhenSameNamedLibrariesAreLoadedThenInstalledOfficialModuleIsSelected()
    {
        string installed = Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.Visual).Assembly.Location)!, "wpfgfx_cor3.dll");
        nint candidate = NativeLibrary.Load(AcceptancePaths.AotDll);
        nint official = NativeLibrary.Load(installed);
        using Process process = Process.GetCurrentProcess();
        ProcessModule selected = OfficialRasterModule.Find(process);
        ProcessModule translated = process.Modules.Cast<ProcessModule>().Single(module => module.BaseAddress == candidate);
        Assert.AreNotEqual(candidate, official);
        Assert.AreEqual(translated.ModuleName, selected.ModuleName, true);
        Assert.AreEqual(official, selected.BaseAddress);
        string root = Path.Combine(AcceptancePaths.Results, "dl-oracle-identity");
        Directory.CreateDirectory(root);
        object Identity(ProcessModule module) => new
        {
            Path = module.FileName, BaseAddress = module.BaseAddress.ToString("X"),
            Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(module.FileName)))
        };
        File.WriteAllText(Path.Combine(root, "dual-modules.json"), JsonSerializer.Serialize(new
        {
            Official = Identity(selected), Candidate = Identity(translated),
            OfficialHandle = official.ToString("X"), CandidateHandle = candidate.ToString("X"),
            SameNameCount = process.Modules.Cast<ProcessModule>().Count(module =>
                string.Equals(module.ModuleName, selected.ModuleName, StringComparison.OrdinalIgnoreCase)),
            Runtime = RuntimeInformation.FrameworkDescription,
            Mvid = typeof(OfficialRasterModuleTests).Module.ModuleVersionId
        }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
