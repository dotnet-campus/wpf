using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class WireCompilerArchiveTests
{
    /// <summary>Reads every archived JSON leaf and binds finite wire observations to an original full TRX without closing its parent.</summary>
    [TestMethod]
    public void WhenWireEvidenceIsArchivedThenEveryFieldAndFullRunIdentityIsReadBack()
    {
        string root = typeof(WireCompilerArchiveTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string Full(string p) => Path.Combine(root, p);
        string Relative(string p) => Path.GetRelativePath(root, p).Replace('\\', '/');
        string Hash(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
        string evidence = Full("WpfGfxShape/Investigations/GenProtoEvidence");
        int ReadEveryLeaf(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object) return element.EnumerateObject().Sum(p => ReadEveryLeaf(p.Value));
            if (element.ValueKind == JsonValueKind.Array) return element.EnumerateArray().Sum(ReadEveryLeaf);
            _ = element.GetRawText();
            return 1;
        }
        string original = Path.Combine(evidence, "wirecompiler-b91bd310535142a896609a32781d1e4d/report.json");
        Assert.AreEqual("542F796BB1BECE3E53582DF7DF1F695BE14A61FAACC4F55F58C4C221801E39B2", Hash(original));
        using var old = JsonDocument.Parse(File.ReadAllText(original));
        using var oldSummary = JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(original)!, "summary.json")));
        string latest = Directory.EnumerateDirectories(evidence, "wirecompiler-*")
            .Where(d => Guid.TryParseExact(Path.GetFileName(d)["wirecompiler-".Length..], "N", out _))
            .Select(d => Path.Combine(d, "report.json")).MaxBy(File.GetLastWriteTimeUtc)!;
        using var report = JsonDocument.Parse(File.ReadAllText(latest));
        using var summary = JsonDocument.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(latest)!, "summary.json")));
        Assert.AreEqual(Hash(latest), summary.RootElement.GetProperty("sha256").GetString());
        var sources = report.RootElement.GetProperty("sources").EnumerateArray().Select(s =>
        {
            string path = Full(s.GetProperty("path").GetString()!);
            string hash = Hash(path);
            Assert.AreEqual(s.GetProperty("sha256").GetString(), hash, path);
            return new { path = Relative(path), sha256 = hash };
        }).ToArray();
        string managed = report.RootElement.GetProperty("assembly").GetProperty("path").GetString()!;
        Assert.AreEqual(report.RootElement.GetProperty("assembly").GetProperty("sha256").GetString(), Hash(managed));
        string assembly = typeof(WireCompilerArchiveTests).Assembly.Location;
        Assert.AreEqual(report.RootElement.GetProperty("testAssembly").GetProperty("sha256").GetString(), Hash(assembly));
        var runs = Directory.EnumerateFiles(Full("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/TestResults"), "*.trx")
            .Select(p => new { Path = p, Document = XDocument.Load(p) }).ToArray();
        var full = runs.Where(r => r.Document.Descendants().Any(e => e.Name.LocalName == "Counters" && (int?)e.Attribute("total") >= 39)).MaxBy(r => File.GetLastWriteTimeUtc(r.Path))!;
        var targeted = runs.Where(r => r.Document.Descendants().Any(e => e.Name.LocalName == "Counters" && (int?)e.Attribute("total") == 1) && r.Document.ToString().Contains("WhenAllWireCommandsAreMeasuredThenMissingManagedPacketsRemainExplicitNativeContracts", StringComparison.Ordinal)).MaxBy(r => File.GetLastWriteTimeUtc(r.Path))!;
        string directory = Path.Combine(evidence, "wirecompiler-delivery-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string Save(string source, string name)
        {
            string destination = Path.Combine(directory, name);
            File.Copy(source, destination);
            Assert.AreEqual(Hash(source), Hash(destination));
            return Relative(destination);
        }
        string c5Path = Path.Combine(evidence, "c5-selected-evaluation/report.json");
        string c5SummaryPath = Path.Combine(evidence, "c5-selected-evaluation/summary.json");
        using var c5 = JsonDocument.Parse(File.ReadAllText(c5Path));
        using var c5Summary = JsonDocument.Parse(File.ReadAllText(c5SummaryPath));
        Assert.AreEqual(Hash(c5Path), c5Summary.RootElement.GetProperty("report").GetProperty("sha256").GetString());
        Assert.AreEqual(Hash(assembly), c5.RootElement.GetProperty("testAssembly").GetProperty("sha256").GetString());
        Assert.AreEqual(Hash(Full(c5.RootElement.GetProperty("testSource").GetProperty("path").GetString()!)), c5.RootElement.GetProperty("testSource").GetProperty("sha256").GetString());
        Assert.AreEqual(333, c5.RootElement.GetProperty("reconciliation").GetArrayLength());
        var rows = report.RootElement.GetProperty("rows").EnumerateArray().ToArray();
        Assert.AreEqual(232, rows.Length);
        var enumComparisons = report.RootElement.GetProperty("enumComparisons").EnumerateArray().ToArray();
        var enumAudit = enumComparisons.Select(e => new
        {
            architecture = e.GetProperty("architecture").GetString(),
            name = e.GetProperty("managedName").GetString(),
            nativeName = e.GetProperty("nativeName").GetString(),
            managedWidth = e.GetProperty("managedWidth"), nativeWidth = e.GetProperty("nativeWidth"), widthEqual = e.GetProperty("widthEqual"),
            missing = e.GetProperty("members").EnumerateArray().Where(m => m.GetProperty("equal").ValueKind == JsonValueKind.Null).ToArray(),
            differences = e.GetProperty("members").EnumerateArray().Where(m => m.GetProperty("equal").ValueKind == JsonValueKind.False).ToArray(),
            comparedMembers = e.GetProperty("members").EnumerateArray().Count(m => m.GetProperty("equal").ValueKind != JsonValueKind.Null)
        }).ToArray();
        var sourceIdentity = Directory.EnumerateFiles(Full("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests"), "*.cs").Select(p => new { path = Relative(p), sha256 = Hash(p) }).ToArray();
        var output = new
        {
            c5 = new { path = Relative(c5Path), sha256 = Hash(c5Path), leavesRead = ReadEveryLeaf(c5.RootElement), summaryLeavesRead = ReadEveryLeaf(c5Summary.RootElement), snapshot = Save(c5Path, "c5-report.json"), summary = Save(c5SummaryPath, "c5-summary.json"), reconciliationCount = c5.RootElement.GetProperty("reconciliation").GetArrayLength(), c5Satisfied = c5.RootElement.GetProperty("c5Satisfied").GetBoolean() },
            original = new { path = Relative(original), sha256 = Hash(original), leavesRead = ReadEveryLeaf(old.RootElement), summaryLeavesRead = ReadEveryLeaf(oldSummary.RootElement) },
            current = new { path = Relative(latest), sha256 = Hash(latest), leavesRead = ReadEveryLeaf(report.RootElement), summaryLeavesRead = ReadEveryLeaf(summary.RootElement), snapshot = Save(latest, "wire-report.json"), summary = Save(Path.Combine(Path.GetDirectoryName(latest)!, "summary.json"), "wire-summary.json") },
            full = new { original = Relative(full.Path), snapshot = Save(full.Path, "genproto-full.trx"), sha256 = Hash(full.Path), runId = (string?)full.Document.Root!.Attribute("id"), counters = full.Document.Descendants().Single(e => e.Name.LocalName == "Counters").ToString() },
            targeted = new { path = Relative(targeted.Path), sha256 = Hash(targeted.Path), runId = (string?)targeted.Document.Root!.Attribute("id") },
            testAssembly = new { path = Relative(assembly), sha256 = Hash(assembly), mvid = typeof(WireCompilerArchiveTests).Assembly.ManifestModule.ModuleVersionId },
            managedAssembly = report.RootElement.GetProperty("assembly"), sourceIdentity, measuredSources = sources,
            observations = rows.GroupBy(r => r.GetProperty("architecture").GetString()).Select(g => new { architecture = g.Key, commands = g.Count(), sizeDifferences = g.Count(r => r.GetProperty("sizeEqual").ValueKind == JsonValueKind.False), offsetDifferences = g.Count(r => r.GetProperty("offsetsEqual").ValueKind == JsonValueKind.False), nativeOnly = g.Count(r => r.GetProperty("managedSize").ValueKind == JsonValueKind.Null) }).ToArray(),
            enumAudit,
            finiteMeasurementState = "Satisfied", productionExecutionState = "Blocked", requiredParent = "Blocked", stage4Closed = false,
            x86Clr = "DerivedFromX64FixedWidthObservationNotExecuted", arm64 = "BlockedMissingCompilerAndRuntime",
            nativeOnly = report.RootElement.GetProperty("nativeOnly"),
            remaining = new[] { "Native padding has no managed named field; no byte-content equivalence claim", "VisualCreate has no direct producer located; consumer CMilVisual::ProcessCreate returns E_UNEXPECTED", "Native-only producer execution is not established by layout", "Enum members without measured native aliases remain unproven", "Historical authority binding, ARM64, full runtime, official player protocol and observable HW session remain blockers" }
        };
        string path = Path.Combine(directory, "report.json");
        File.WriteAllText(path, JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true }));
        using var readback = JsonDocument.Parse(File.ReadAllText(path));
        Assert.IsGreaterThan(0, ReadEveryLeaf(readback.RootElement));
        string ledger = Path.Combine(directory, "wirecompiler-finite-acceptance.json");
        File.WriteAllText(ledger, JsonSerializer.Serialize(new { evidenceId = "EVID-GENPROTO-WIRECOMPILER-FINITE", satisfactionState = "Satisfied", scope = "Independent compiler layout observation and explicit difference review only", path = Relative(path), sha256 = Hash(path), requiredParent = "Blocked", stage4Closed = false, productionExecution = "Blocked", x86ClrExecuted = false, arm64 = "Blocked" }, new JsonSerializerOptions { WriteIndented = true }));
        using var ledgerReadback = JsonDocument.Parse(File.ReadAllText(ledger));
        Assert.AreEqual(Hash(path), ledgerReadback.RootElement.GetProperty("sha256").GetString());
        Console.WriteLine("WIRECOMPILER_DELIVERY=" + path + " SHA256=" + Hash(path));
    }
}
