using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class LedgerRepairTests
{
    private static string Root => typeof(LedgerRepairTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
    private static string Normalize(string path) => Regex.Replace(path.ToUpperInvariant(), "[^A-Z0-9]+", "-").Trim('-');

    [TestMethod]
    public void WhenDanglingReferencesAreRestoredThenEachHasAnActualSourceOrExplicitExternalGap()
    {
        string ledger = Path.Combine(Root, "WpfGfxShape/Ledger");
        string archive = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence", "reference-repair-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        string[] names = ["records.jsonl", "groups.jsonl", "evidence.jsonl", "work-packages.jsonl"];
        var documents = names.ToDictionary(n => n, n => File.ReadLines(Path.Combine(ledger, n)).Select(l => JsonNode.Parse(l)!.AsObject()).ToList());
        foreach (string name in names) File.Copy(Path.Combine(ledger, name), Path.Combine(archive, name));
        var ids = documents.Values.SelectMany(v => v).Select(o => new[] { "recordId", "groupId", "evidenceId", "workPackageId" }.Select(k => o[k]?.GetValue<string>()).First(v => v != null)).ToHashSet(StringComparer.Ordinal);
        string[] fields = ["dependencies", "generatedGroupIds", "typeFamilyIds", "cycleGroupIds", "memberRecordIds", "authorityRecordIds", "consumerRecordIds", "requiredEvidenceIds", "satisfiedEvidenceIds", "subjectIds", "ownerWorkPackageId"];
        var references = new List<(string File, int Line, string Field, string Id)>();
        foreach (var file in documents)
        for (int i = 0; i < file.Value.Count; i++)
        foreach (string field in fields)
        {
            JsonNode? value = file.Value[i][field];
            if (value == null) continue;
            IEnumerable<string> values = value is JsonArray array ? array.Select(v => v!.GetValue<string>()) : [value.GetValue<string>()];
            foreach (string id in values.Where(id => !ids.Contains(id))) references.Add((file.Key, i + 1, field, id));
        }
        if (references.Count == 0)
        {
            Assert.IsTrue(documents["records.jsonl"].Count > 2);
            return;
        }
        string[] files = Directory.GetFiles(Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src"), "*", SearchOption.AllDirectories);
        var added = new List<JsonObject>();
        foreach (string id in references.Select(r => r.Id).Distinct(StringComparer.Ordinal))
        {
            Assert.StartsWith("REC-", id, "Unresolved group or evidence must not be replaced with a physical record.");
            bool external = id.StartsWith("REC-EXTERNAL-", StringComparison.Ordinal) && !id.Contains("OSVERSIONHELPER", StringComparison.Ordinal);
            string? path = null;
            if (!external)
            {
                string suffix = id[(id.IndexOf('-', 4) + 1)..];
                suffix = suffix[(suffix.IndexOf('-') + 1)..];
                if (id == "REC-EXTERNAL-PROJECT-SHARED-OSVERSIONHELPER") suffix = "SHARED-OSVERSIONHELPER-OSVERSIONHELPER-VCXPROJ";
                string[] matches = files.Where(f => Normalize(Path.GetRelativePath(Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src/WpfGfx"), f)).EndsWith(suffix, StringComparison.Ordinal)).ToArray();
                Assert.HasCount(1, matches, "Ambiguous or absent authority for " + id + ": " + suffix);
                path = Path.GetRelativePath(Root, matches[0]).Replace('\\', '/');
            }
            bool project = path?.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase) == true;
            bool generated = id.StartsWith("REC-GENERATED-", StringComparison.Ordinal);
            string[] required = documents["evidence.jsonl"].Where(e => e["subjectIds"]!.AsArray().Any(s => s!.GetValue<string>() == id)).Select(e => e["evidenceId"]!.GetValue<string>()).ToArray();
            var record = new JsonObject
            {
                ["schemaVersion"] = "1.0.0", ["recordId"] = id,
                ["recordType"] = external ? "ExternalSupply" : project ? "ProjectSentinel" : "PhysicalFile",
                ["scopeDisposition"] = id.Contains("EXTERNAL-EVIDENCE", StringComparison.Ordinal) ? "ContractOnly" : "ProductionDenominator",
                ["nativeProject"] = project ? path : null, ["nativePath"] = path,
                ["fileKind"] = external ? "External" : project || id.StartsWith("REC-BUILD-", StringComparison.Ordinal) ? "Build" : generated ? "GeneratedInput" : id.Contains("-ABI-", StringComparison.Ordinal) ? "ABI" : id.Contains("-RESOURCE-", StringComparison.Ordinal) ? "Resource" : "Implementation",
                ["sourceAuthority"] = external ? "ExternalBinary" : "CurrentConsumedSource",
                ["originalResponsibility"] = external ? "Referenced external supply or historical binary; acquisition and provenance remain required." : "Referenced original source; detailed translation and production closure remain unverified.",
                ["investigationBatch"] = "GENPROTO-REFERENCE-REPAIR", ["implementationBatch"] = "0",
                ["proposedCSharpPath"] = null, ["status"] = "NotInvestigated", ["ownerWorkPackageId"] = null,
                ["lastReviewedSession"] = "GENPROTO-REFERENCE-REPAIR",
                ["sourceSha256"] = path == null ? null : Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root, path))))
            };
            foreach (string key in new[] { "keySymbols", "pchAndDirectIncludes", "dependencies", "platformDependencies", "generatedGroupIds", "typeFamilyIds", "cycleGroupIds", "abiContractIds", "satisfiedEvidenceIds", "knownDeviations", "notesDecisionIds" }) record[key] = new JsonArray();
            record["buildParticipation"] = new JsonArray(external ? "TransportSupply" : project ? "ProjectReference" : generated ? "GeneratedInput" : "Unknown");
            record["readinessBlockers"] = new JsonArray(external ? "BLOCKER-EXTERNAL-PROVENANCE-AND-PAYLOAD" : "BLOCKER-DETAILED-PHYSICAL-AND-MULTIVIEW-REVIEW");
            record["requiredEvidenceIds"] = new JsonArray(required.Select(s => (JsonNode?)JsonValue.Create(s)).ToArray());
            added.Add(record);
        }
        var classification = references.Select(r => new { file = r.File, line = r.Line, field = r.Field, target = r.Id, classification = r.Id.StartsWith("REC-EXTERNAL-", StringComparison.Ordinal) ? "externalfuture" : r.Field == "authorityRecordIds" ? "generatedauthority" : r.Field == "consumerRecordIds" ? "mappedconsumer" : "member", record = added.Single(a => a["recordId"]!.GetValue<string>() == r.Id) }).ToArray();
        File.WriteAllText(Path.Combine(archive, "classification.json"), JsonSerializer.Serialize(new { executed = true, references = classification, inventedClosed = false }, new JsonSerializerOptions { WriteIndented = true }));
        documents["records.jsonl"].AddRange(added);
        File.WriteAllLines(Path.Combine(ledger, "records.jsonl"), documents["records.jsonl"].Select(o => o.ToJsonString()));
        var metadata = JsonNode.Parse(File.ReadAllText(Path.Combine(ledger, "schema-version.json")))!.AsObject();
        metadata["recordCount"] = documents["records.jsonl"].Count;
        metadata["validationStatus"] = "ReferenceRecordsRestored-PendingIndependentValidation";
        metadata["deferredValidationReason"] = "Reference existence is not production or evidence closure; external provenance and generated semantic/layout assertions remain required.";
        metadata["referenceRepairReport"] = Path.GetRelativePath(Root, Path.Combine(archive, "classification.json")).Replace('\\', '/');
        File.WriteAllText(Path.Combine(ledger, "schema-version.json"), metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Reference repair archive: " + archive);
        Assert.HasCount(documents["records.jsonl"].Count, File.ReadLines(Path.Combine(ledger, "records.jsonl")).Select(l => JsonDocument.Parse(l)).ToArray());
    }
}
