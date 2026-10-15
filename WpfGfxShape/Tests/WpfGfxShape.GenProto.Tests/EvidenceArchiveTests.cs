using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class EvidenceArchiveTests
{
    /// <summary>Archives independently produced evidence and preserves unsatisfied hard requirements.</summary>
    [TestMethod]
    public void WhenEvidenceIsArchivedThenMachineStatesAndBinaryIdentityAreReadBack()
    {
        string root = typeof(EvidenceArchiveTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string evidenceRoot = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence");
        string run = Guid.NewGuid().ToString("N");
        string archive = Path.Combine(evidenceRoot, "delivery-" + run);
        Directory.CreateDirectory(archive);
        string Relative(string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        string Save(string path, string name)
        {
            string destination = Path.Combine(archive, name);
            File.Copy(path, destination);
            Assert.AreEqual(Hash(path), Hash(destination));
            return destination;
        }
        string ledger = Path.Combine(root, "WpfGfxShape/Ledger");
        string evidencePath = Path.Combine(ledger, "evidence.jsonl");
        var rows = File.ReadLines(evidencePath).Select(l => JsonNode.Parse(l)!.AsObject()).ToList();
        var bindings = new List<object>();
        var addedIds = new Dictionary<string, List<string>>();
        foreach (var item in new[]
        {
            (Id: "EVID-GENPROTO-REVIEW-CORE", Group: "GENPAIR-CORE-TYPES", Pattern: "preprocessing-wgx_core_types-*", File: "report.json"),
            (Id: "EVID-GENPROTO-REVIEW-AV", Group: "GENPAIR-AV-TYPES", Pattern: "preprocessing-wgx_av_types-*", File: "report.json"),
            (Id: "EVID-GENPROTO-CONSUMER-DECLARATIONS", Group: "GENPROTO-MILCMD", Pattern: "consumer-assertions-*", File: "assertions.json"),
            (Id: "EVID-GENPROTO-COLOR-BEHAVIOR", Group: "GENPAIR-CORE-TYPES", Pattern: "color-behavior-*", File: "report.json"),
            (Id: "EVID-GENPROTO-NATIVE-CORE-WIN32", Group: "GENPAIR-CORE-TYPES", Pattern: "native-core-Win32-*", File: "report.json"),
            (Id: "EVID-GENPROTO-NATIVE-CORE-X64", Group: "GENPAIR-CORE-TYPES", Pattern: "native-core-x64-*", File: "report.json")
        })
        {
            string source = Directory.EnumerateDirectories(evidenceRoot, item.Pattern).Select(d => Path.Combine(d, item.File)).Where(File.Exists).MaxBy(File.GetLastWriteTimeUtc)!;
            using var report = JsonDocument.Parse(File.ReadAllText(source));
            if (report.RootElement.TryGetProperty("errors", out var errors)) Assert.AreEqual(0, errors.GetArrayLength());
            string copy = Save(source, item.Id + ".json");
            var row = rows.SingleOrDefault(r => r["evidenceId"]?.GetValue<string>() == item.Id);
            if (row == null) { row = new JsonObject(); rows.Add(row); }
            row["schemaVersion"] = "1.0.0";
            row["evidenceId"] = item.Id;
            row["evidenceType"] = "IndependentSubrequirement";
            row["subjectIds"] = new JsonArray(item.Group);
            row["status"] = "Passed";
            row["satisfactionState"] = "Satisfied";
            row["executed"] = true;
            row["path"] = Relative(copy);
            row["hash"] = Hash(copy);
            row["sourceReport"] = Relative(source);
            row["sourceReportHash"] = Hash(source);
            row["runId"] = run;
            row["configuration"] = "DedicatedTests";
            row["architecture"] = item.Pattern.Contains("Win32", StringComparison.Ordinal) ? "Win32CompilerDataOnly" : "X64Host";
            row["notes"] = "Satisfies only the named measured/reviewed subrequirement. Not raw-text equivalence, historical regeneration, complete managed layout, or ARM64 execution.";
            if (!addedIds.TryGetValue(item.Group, out var ids)) addedIds[item.Group] = ids = [];
            ids.Add(item.Id);
            bindings.Add(new { item.Id, source = Relative(source), sourceHash = Hash(source), archived = Relative(copy), hash = Hash(copy), report = report.RootElement.Clone() });
        }
        string trx = Save(Path.Combine(root, "WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx"), "com-full.trx");
        var xml = XDocument.Load(trx);
        XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var counters = xml.Descendants(ns + "Counters").Single();
        string comRun = xml.Root!.Attribute("id")!.Value;
        var failures = xml.Descendants(ns + "UnitTestResult").Where(e => e.Attribute("outcome")?.Value == "Failed")
            .Select(e => new { name = e.Attribute("testName")!.Value, message = e.Descendants(ns + "Message").FirstOrDefault()?.Value }).ToArray();
        string binary = Save(Path.Combine(root, "WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll"), "wpfgfx_cor3.dll");
        foreach (var row in rows.Where(r => new[] { "EVID-GENPAIR-CORE-TYPES-LAYOUT", "EVID-GENPAIR-AV-TYPES-LAYOUT", "EVID-GENPROTO-MILCMD-REGEN-DIFF", "EVID-GENPROTO-MILCMD-CLOSURE", "EVID-GENPAIR-CORE-TYPES-CLOSURE", "EVID-GENPAIR-AV-TYPES-CLOSURE" }.Contains(r["evidenceId"]?.GetValue<string>())))
        {
            row["status"] = "Blocked";
            row["satisfactionState"] = "Blocked";
            row["blockerReason"] = row["evidenceId"]!.GetValue<string>().Contains("LAYOUT", StringComparison.Ordinal)
                ? "Complete independent core managed per-field/enum/pack/size comparison remains absent; X86 runtime not executed; ARM64 v145 and ClangCL builds both fail MSB8020. Native compiler measurements are available separately and do not satisfy all architectures."
                : "Reviewed current-generation differences are available separately; complete physical generated/mapping/consumer closure and historical CLR/input authority remain unproven. No raw-text equality or historical execution claimed.";
        }
        string groupPath = Path.Combine(ledger, "groups.jsonl");
        var groups = File.ReadLines(groupPath).Select(l => JsonNode.Parse(l)!.AsObject()).ToList();
        foreach (var group in groups.Where(g => addedIds.ContainsKey(g["groupId"]!.GetValue<string>())))
        {
            var required = group["requiredEvidenceIds"]!.AsArray();
            var satisfied = group["satisfiedEvidenceIds"] as JsonArray ?? new JsonArray();
            foreach (string id in addedIds[group["groupId"]!.GetValue<string>()])
            {
                if (!required.Any(n => n?.GetValue<string>() == id)) required.Add(id);
                if (!satisfied.Any(n => n?.GetValue<string>() == id)) satisfied.Add(id);
            }
            group["satisfiedEvidenceIds"] = satisfied;
            group["status"] = "Blocked";
            group["blocked"] = true;
            group["lastReviewedSession"] = "GENPROTO-DELIVERY-" + run;
        }
        File.WriteAllLines(evidencePath, rows.Select(r => r.ToJsonString()));
        File.WriteAllLines(groupPath, groups.Select(g => g.ToJsonString()));
        string metadataPath = Path.Combine(ledger, "schema-version.json");
        var metadata = JsonNode.Parse(File.ReadAllText(metadataPath))!.AsObject();
        var counts = new JsonObject();
        foreach (var pair in new[] { (File: "records.jsonl", Key: "records"), (File: "groups.jsonl", Key: "groups"), (File: "evidence.jsonl", Key: "evidence"), (File: "work-packages.jsonl", Key: "workPackages") }) counts[pair.Key] = File.ReadLines(Path.Combine(ledger, pair.File)).Count();
        metadata["actualCounts"] = counts;
        metadata["recordCount"] = counts["records"]!.GetValue<int>();
        metadata["latestDeliveryArchive"] = Relative(archive);
        metadata["stage4Closed"] = false;
        File.WriteAllText(metadataPath, metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        var snapshots = new List<object>();
        foreach (string file in new[] { "records.jsonl", "groups.jsonl", "evidence.jsonl", "work-packages.jsonl", "schema-version.json" })
        {
            string copy = Save(Path.Combine(ledger, file), file);
            snapshots.Add(new { path = Relative(copy), hash = Hash(copy) });
        }
        var documents = new List<object>();
        foreach (string relative in new[] { "WpfGfxShape/Docs/handoffs/current-stable-baseline.md", "WpfGfxShape/Docs/remaining-gap-closure-plan.md" })
        {
            string path = Path.Combine(root, relative);
            documents.Add(new { path = relative, hash = Hash(path) });
        }
        string output = Path.Combine(archive, "report.json");
        File.WriteAllText(output, JsonSerializer.Serialize(new
        {
            schemaVersion = "1.0.0", runId = run, executed = true, stage4Closed = false,
            sourceVersion = metadata["originWpfRevision"]!.GetValue<string>(), sourceWorkingTree = "NotInspected",
            bindings, snapshots, documents,
            measurementIdentity = new { assemblySha256 = Hash(typeof(EvidenceArchiveTests).Assembly.Location), runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription, architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString() },
            existingBinary = new { path = Relative(binary), sha256 = Hash(binary), publishedThisRun = false, pairedWithArchivedComRun = false },
            ordinaryFull = new { executedThisRun = false, provenance = "Not measured by this archival test; consult independently preserved execution reports." },
            historicalComRun = new { executedThisRun = false, scope = "Unknown; counters read from the archived TRX, not asserted to be a full run", runId = comRun, path = Relative(trx), sha256 = Hash(trx), total = (int)counters.Attribute("total")!, passed = (int)counters.Attribute("passed")!, failed = (int)counters.Attribute("failed")!, failures },
            remaining = new[] { "ARM64: MSVC v145 and ClangCL MSB8020", "Complete independent core managed layout and enum comparison", "X86: compiler data available, managed X86 runtime not executed", "Historical CLR/frozen input authority", "Generated/mapping/consumer physical-record closure", "COM full RED: retain every TRX failure; no golden changes" }
        }, new JsonSerializerOptions { WriteIndented = true }));
        using var reread = JsonDocument.Parse(File.ReadAllText(output));
        Assert.IsFalse(reread.RootElement.GetProperty("stage4Closed").GetBoolean());
        foreach (var row in rows.Where(r => r["runId"]?.GetValue<string>() == run)) Assert.AreEqual(row["hash"]!.GetValue<string>(), Hash(Path.Combine(root, row["path"]!.GetValue<string>())));
        Console.WriteLine("Delivery archive: " + output + " SHA256=" + Hash(output));
    }
}
