#if FF_INTEROP_PREFLIGHT
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class FeInteropPackageTests
{
    /// <summary>Separates uploaded inputs from execution output while verifying every listed input.</summary>
    [TestMethod]
    public void WhenFePackageIsSelectedThenManifestAndNativeHashMatch()
    {
        string root = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        string[] manifest = File.ReadAllLines(Path.Combine(root, "package-files.txt"))
            .Select(path => path.Replace('\\', '/')).ToArray();
        var files = manifest.Select(path =>
        {
            string full = Path.Combine(root, path);
            return new { Path = path, Bytes = new FileInfo(full).Length,
                Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(full))) };
        }).ToArray();
        string[] actual = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal).ToArray();
        string[] missing = manifest.Except(actual, StringComparer.Ordinal).ToArray();
        string[] extra = actual.Except(manifest, StringComparer.Ordinal).ToArray();
        string[] outputs = extra.Where(path => path.StartsWith("Results/", StringComparison.Ordinal)
            || path.StartsWith("TestResults/", StringComparison.Ordinal)).ToArray();
        string[] unexpected = extra.Except(outputs, StringComparer.Ordinal).ToArray();
        string candidate = files.Single(file => file.Path == "Native/wpfgfx_cor3.dll").Sha256;
        string expected = File.ReadAllText(Path.Combine(root, "aot.sha256")).Trim();
        using var stream = File.OpenRead(Path.Combine(root, "WpfGfxShape.ComAcceptance.dll"));
        using var pe = new System.Reflection.PortableExecutable.PEReader(stream);
        var reader = System.Reflection.Metadata.PEReaderExtensions.GetMetadataReader(pe);
        Guid mvid = reader.GetGuid(reader.GetModuleDefinition().Mvid);
        string results = Path.Combine(root, "Results");
        Directory.CreateDirectory(results);
        File.WriteAllText(Path.Combine(results, "upload-preflight.json"), JsonSerializer.Serialize(new
        {
            ExecutionDirectory = root, Files = files, CandidateFileSha256 = candidate,
            ExpectedNativeSha256 = expected,
            TestSha256 = files.Single(file => file.Path == "WpfGfxShape.ComAcceptance.dll").Sha256,
            TestMvid = mvid, MissingInputs = missing, ExecutionOutputs = outputs,
            UnexpectedInputs = unexpected, ExactDirectoryMatches = actual.SequenceEqual(manifest.Order(StringComparer.Ordinal)),
            NativeMatches = candidate == expected,
            Note = "File hash only; actual loaded module identity must come from DR business evidence."
        }, new JsonSerializerOptions { WriteIndented = true }));
        Assert.IsTrue(manifest.Length > 0, "Input manifest must be nonempty.");
        Assert.IsTrue(files.All(file => file.Bytes > 0), "Every listed input must be nonempty.");
        Assert.AreEqual(manifest.Length, manifest.Distinct(StringComparer.Ordinal).Count(), "Duplicate manifest inputs.");
        Assert.AreEqual(0, missing.Length, string.Join(";", missing));
        Assert.AreEqual(0, unexpected.Length, string.Join(";", unexpected));
        Assert.AreEqual(expected, candidate, "Native file SHA must match aot.sha256.");
        Assert.AreEqual("1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53", candidate,
            "This x86 experiment reuses the unchanged FE candidate.");
    }
}
#endif
