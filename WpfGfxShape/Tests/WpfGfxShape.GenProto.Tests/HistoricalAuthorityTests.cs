using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class HistoricalAuthorityTests
{
    [TestMethod]
    public void WhenCurrentGeneratorEnvironmentIsAuditedThenHistoricalAuthorityIsNotInferred()
    {
        string root = typeof(HistoricalAuthorityTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string[] sources = ["global.json", "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/mcg.proj", "src/Microsoft.DotNet.Wpf/src/WpfGfx/tools/csp/csp.csproj", "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/generators/ProtocolFingerprint.cs", "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/xml/Version.xml", "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/xml/Resource.xml", "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/main/Resources.rsp"];
        XDocument project = XDocument.Load(Path.Combine(root, sources[2]));
        using var sdk = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "global.json")));
        string[] clrSources = Directory.EnumerateFiles(Path.Combine(root, "src"), "String.cs", SearchOption.AllDirectories).ToArray();
        string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "historical-authority-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        File.WriteAllText(Path.Combine(archive, "report.json"), JsonSerializer.Serialize(new
        {
            schemaVersion = "1.0.0", kind = "CurrentGeneratorEnvironmentAuthorityAudit", executed = true,
            currentSdk = sdk.RootElement.GetProperty("sdk"),
            currentCspTargetFramework = project.Descendants("TargetFramework").Single().Value,
            currentCspPlatformTarget = project.Descendants("PlatformTarget").Single().Value,
            currentConfiguration = "Not pinned by mcg.proj; MSBuild child build configuration inheritance is not historical proof",
            files = sources.Select(p => new { path = p, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, p)))) }),
            clrSourcesFound = clrSources,
            historicalAuthority = new { status = "BlockedMissingHistoricalAuthority", executed = false, branch = "NotProven", seed = "NotProven", runtimeBuild = "NotProven", historicalInputManifest = "Absent", independentKnownVectors = "Absent", pinnedClrSourceCommit = "Absent", reason = "Current net48/x86 CSP declaration identifies current build intent only. No historical runtime/configuration or frozen-generation input identity is supplied; no CLR String.cs in src. NuGet public API signatures do not establish private string hashing implementation, conditional compilation or vectors." },
            frozenContractChanged = false, compatibleHashAlgorithmImplemented = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.AreEqual("net48", project.Descendants("TargetFramework").Single().Value);
    }
}
