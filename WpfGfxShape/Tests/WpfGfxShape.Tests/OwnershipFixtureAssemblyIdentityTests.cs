using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class OwnershipFixtureAssemblyIdentityTests
{
    /// <summary>Records the actual loaded test and product assemblies for ownership fixture validation.</summary>
    [TestMethod]
    public void WhenOwnershipFixtureIdentityIsRequestedThenLoadedAssembliesAreRecorded()
    {
        string results = Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(results);
        Assembly[] assemblies = [typeof(OwnershipFixtureAssemblyIdentityTests).Assembly, typeof(Direct3D9Device).Assembly];
        string path = Path.Combine(results, "av-ownership-assembly-identity.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            BaseDirectory = AppContext.BaseDirectory,
            ProcessPath = Environment.ProcessPath,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            PointerSize = IntPtr.Size,
            Runtime = RuntimeInformation.FrameworkDescription,
            Assemblies = assemblies.Select(assembly => new
            {
                assembly.Location,
                assembly.FullName,
                Mvid = assembly.ManifestModule.ModuleVersionId,
                Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)))
            }).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true }));

        Assert.IsTrue(File.Exists(path));
    }
}
