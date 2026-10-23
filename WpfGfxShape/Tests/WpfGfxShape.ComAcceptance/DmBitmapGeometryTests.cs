#if DM_BITMAP_GEOMETRY
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class DmBitmapGeometryTests
{
    /// <summary>Runs the complete existing bitmap target class and public geometry and independent realizer contracts under the bounded supervisor.</summary>
    [TestMethod]
    public async Task WhenExpandedFamiliesAreSelectedThenBoundedContractsRunAsync()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "dm-bitmap-geometry");
        string[] families = ["BitmapRenderTargetTests.", "GeometryUtilityTests.", "IndependentRealizerAbiTests.", "IndependentBitmapRealizerAbiTests.", "OfficialRasterModuleTests.WhenSameNamedLibrariesAreLoaded", "DmBitmapGeometryTests.WhenActualPackage"];
        string filter = string.Join("|", families.Select(name => "FullyQualifiedName~" + name));
        int exit = await ExitDiagnostics.RunAsync([root, "120", filter]);
        Assert.AreEqual(0, exit, root);
    }

    /// <summary>Records the real loaded module handles, package lengths and the unchanged default test case inventory.</summary>
    [TestMethod]
    public void WhenActualPackageIsConsumedThenIdentityAndDenominatorsAreRecorded()
    {
        object Identity(string path) => new { Path = path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), Version = FileVersionInfo.GetVersionInfo(path).FileVersion };
        object Loaded(string path)
        {
            nint handle = NativeLibrary.Load(path);
            using var process = Process.GetCurrentProcess();
            var module = process.Modules.Cast<ProcessModule>().Single(item => item.BaseAddress == handle);
            return new { Handle = handle.ToString("X"), Actual = Identity(module.FileName) };
        }
        Type[] types = [typeof(BitmapRenderTargetTests), typeof(GeometryUtilityTests), typeof(IndependentRealizerAbiTests), typeof(IndependentBitmapRealizerAbiTests)];
        var inventory = types.Select(type => new
        {
            Family = type.Name,
            Methods = type.GetMethods().Where(method => method.GetCustomAttribute<TestMethodAttribute>() is not null)
                .Select(method => new { Method = method.Name, Cases = Math.Max(1, method.GetCustomAttributes<DataRowAttribute>().Count()) }).ToArray()
        }).ToArray();
        var files = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "package-files.txt"))
            .Select(relative => new { Path = relative, Bytes = new FileInfo(Path.Combine(AppContext.BaseDirectory, relative)).Length }).ToArray();
        string root = Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "dm-package-identity.json"), JsonSerializer.Serialize(new
        {
            Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            Test = Identity(typeof(DmBitmapGeometryTests).Assembly.Location), TestMvid = typeof(DmBitmapGeometryTests).Module.ModuleVersionId,
            Wpf = Identity(typeof(System.Windows.Media.Visual).Assembly.Location),
            Official = Loaded(AcceptancePaths.OriginalDll), Candidate = Loaded(AcceptancePaths.AotDll),
            ValidatedCandidateSha256 = AcceptancePaths.ValidateIdentity(),
            FileCount = files.Length, TotalBytes = files.Sum(file => file.Bytes), Files = files, Inventory = inventory,
            BusinessCases = inventory.Sum(family => family.Methods.Sum(method => method.Cases))
        }, new JsonSerializerOptions { WriteIndented = true }));
        string expectedCandidateSha256 = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA",
            Architecture.X64 => "F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7",
            _ => throw new PlatformNotSupportedException()
        };
        Assert.AreEqual(expectedCandidateSha256, AcceptancePaths.ValidateIdentity());
    }
}
#endif
