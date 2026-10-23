using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class DdUploadBoundaryTests
{
    /// <summary>Records the actual uploaded input before the selected smoke tests create results.</summary>
    [TestMethod]
    public void WhenPackageIsInspectedThenRuntimeAndInputSizesAreRecorded()
    {
        if (!AcceptancePaths.Sandbox) return;
        string root = AppContext.BaseDirectory;
        string[] manifest = File.ReadAllLines(Path.Combine(root, "package-files.txt"));
        var files = manifest.Select(relative =>
        {
            var file = new FileInfo(Path.Combine(root, relative));
            return new { Path = relative, Bytes = file.Length };
        }).ToArray();
        string officialSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.OriginalDll)));
        foreach (var reference in new[] { (File: "glyph-arial-original.json", Property: "OriginalLibrarySha256"), (File: "original-wpf.json", Property: "NativeSha256") })
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "ReferenceData", reference.File)));
            Assert.AreEqual(officialSha, document.RootElement.GetProperty(reference.Property).GetString());
        }
        string assembly = typeof(DdUploadBoundaryTests).Assembly.Location;
        string output = Path.Combine(root, "Results", "dd-upload-boundary.json");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllText(output, JsonSerializer.Serialize(new
        {
            SourceRoot = root,
            Runtime = RuntimeInformation.FrameworkDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            WpfAssembly = typeof(System.Windows.Media.Brush).Assembly.Location,
            TestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))),
            TestMvid = typeof(DdUploadBoundaryTests).Module.ModuleVersionId,
            CandidateSha256 = AcceptancePaths.ValidateIdentity(),
            TotalBytes = files.Sum(file => file.Bytes),
            Files = files,
            Distribution = files.GroupBy(file => file.Path.Contains('/') ? file.Path.Split('/')[0] : "root")
                .Select(group => new { Directory = group.Key, Count = group.Count(), Bytes = group.Sum(file => file.Bytes) }).ToArray(),
            PreviousInputs = new[] { "ccfa9636676f4137aeadc60cc3341767", "d93c60b3f67a46cc8534c06774de42ca" }
                .Select(id => InspectPreviousInput(Path.Combine(@"C:\CodingAgentSandbox\Tasks", id))).ToArray()
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.IsFalse(manifest.Any(path => path.StartsWith("Results/", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("TestResults/", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase)));
    }

#if DE_UPLOAD_REVIEW
    /// <summary>Recovers the known DD smoke evidence and inventories installed runtimes without executing another acceptance run.</summary>
    [TestMethod]
    public void WhenExplicitDeReviewRunsThenKnownEvidenceAndRuntimeInventoryAreRecorded()
    {
        string destination = Path.Combine(AppContext.BaseDirectory, "Results", "de-upload-review");
        Directory.CreateDirectory(destination);
        var recovered = new List<(string Id, string Relative, bool Exists)>();
        foreach (var task in new[]
        {
            (Id: "216843acbe554e38badc6ccbd7c5aba1", Arch: "x64"),
            (Id: "174197efec39493d9e070e63a1151c4d", Arch: "x86")
        })
        {
            string source = Path.Combine(@"C:\CodingAgentSandbox\Tasks", task.Id);
            foreach (string relative in new[]
            {
                $"Results/dd-final-{task.Arch}/TestResults/child.trx",
                $"Results/dd-final-{task.Arch}/supervision.json",
                "artifacts/bw-curved-fill/precision/comparison.json",
                "artifacts/bw-curved-fill/precision/candidate/identity.json",
                "artifacts/bw-curved-fill/precision/official/identity.json",
                "artifacts/bw-curved-fill/precision/official-repeat/identity.json",
                "artifacts/bw-curved-fill/precision/candidate/supervision.json",
                "artifacts/bw-curved-fill/precision/official/supervision.json",
                "artifacts/bw-curved-fill/precision/official-repeat/supervision.json"
            })
            {
                string path = Path.Combine(source, relative);
                bool exists = File.Exists(path);
                recovered.Add((task.Id, relative, exists));
                if (!exists) continue;
                var file = new FileInfo(path);
                Assert.IsTrue(file.Length <= 16 * 1024 * 1024);
                string target = Path.Combine(destination, task.Arch, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(path, target, false);
            }
        }
        var runtimes = new List<object>();
        foreach (string root in new[] { @"C:\Program Files\dotnet", @"C:\Program Files (x86)\dotnet" })
        foreach (string framework in new[] { "Microsoft.NETCore.App", "Microsoft.WindowsDesktop.App" })
        {
            string shared = Path.Combine(root, "shared", framework);
            string[] versions = Directory.Exists(shared) ? Directory.GetDirectories(shared) : [];
            foreach (string version in versions)
            {
                var modules = new List<object>();
                foreach (string name in new[] { "coreclr.dll", "PresentationCore.dll", "wpfgfx_cor3.dll" })
                {
                    string path = Path.Combine(version, name);
                    if (!File.Exists(path)) continue;
                    using var stream = File.OpenRead(path);
                    using var pe = new System.Reflection.PortableExecutable.PEReader(stream, System.Reflection.PortableExecutable.PEStreamOptions.LeaveOpen);
                    string machine = pe.PEHeaders.CoffHeader.Machine.ToString();
                    stream.Position = 0;
                    modules.Add(new { Path = path, Machine = machine, Sha256 = Convert.ToHexString(SHA256.HashData(stream)),
                        Version = System.Diagnostics.FileVersionInfo.GetVersionInfo(path).ProductVersion });
                }
                runtimes.Add(new { Framework = framework, Directory = version, Modules = modules });
            }
        }
        string[] manifest = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "package-files.txt"));
        var inventory = manifest.Select(relative => new { Path = relative, Bytes = new FileInfo(Path.Combine(AppContext.BaseDirectory, relative)).Length }).ToArray();
        File.WriteAllText(Path.Combine(destination, "review.json"), JsonSerializer.Serialize(new
        {
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(), Runtime = RuntimeInformation.FrameworkDescription,
            Recovered = recovered.Select(item => new { item.Id, item.Relative, item.Exists }), InstalledRuntimes = runtimes, TotalBytes = inventory.Sum(file => file.Bytes),
            LargestFiles = inventory.OrderByDescending(file => file.Bytes).Take(30).ToArray(), Files = inventory
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.IsTrue(recovered.All(item => item.Exists));
    }
#endif

    private static object InspectPreviousInput(string root)
    {
        if (!Directory.Exists(root)) return new { Root = root, Status = "not-visible" };
        var files = Directory.EnumerateFiles(root, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            AttributesToSkip = FileAttributes.ReparsePoint,
            IgnoreInaccessible = false
        }).Select(path => new { Path = Path.GetRelativePath(root, path), Bytes = new FileInfo(path).Length }).ToArray();
        return new { Root = root, Status = "observed-current-snapshot-not-original-request", TotalBytes = files.Sum(file => file.Bytes), Files = files };
    }
}
