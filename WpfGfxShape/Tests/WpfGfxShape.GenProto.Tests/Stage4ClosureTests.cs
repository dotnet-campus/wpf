using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class Stage4ClosureTests
{
    private static string Root => typeof(Stage4ClosureTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;

    [TestMethod]
    public void WhenLedgerIsValidatedThenAllReferencesResolveAndCountsAreRecorded()
    {
        string ledger = Path.Combine(Root, "WpfGfxShape/Ledger");
        string[] names = ["records.jsonl", "groups.jsonl", "evidence.jsonl", "work-packages.jsonl"];
        var files = names.ToDictionary(n => n, n => File.ReadLines(Path.Combine(ledger, n)).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(l)!).ToArray());
        string[] idFields = ["recordId", "groupId", "evidenceId", "workPackageId"];
        var ids = files.Values.SelectMany(v => v).Select(o => idFields.Where(o.ContainsKey).Select(k => o[k].GetString()!).Single()).ToArray();
        var known = ids.ToHashSet(StringComparer.Ordinal);
        var errors = new List<string>();
        if (known.Count != ids.Length) errors.Add("Duplicate global ID");
        string[] fields = ["dependencies", "generatedGroupIds", "typeFamilyIds", "cycleGroupIds", "memberRecordIds", "authorityRecordIds", "consumerRecordIds", "requiredEvidenceIds", "satisfiedEvidenceIds", "subjectIds", "ownerWorkPackageId"];
        foreach (var file in files)
        foreach (var row in file.Value)
        {
            if (!row.TryGetValue("schemaVersion", out var version) || version.GetString() != "1.0.0") errors.Add(file.Key + ": invalid schemaVersion");
            foreach (string field in fields)
            {
                if (!row.TryGetValue(field, out var value) || value.ValueKind == JsonValueKind.Null) continue;
                string[] references = value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Select(v => v.GetString()!).ToArray() : [value.GetString()!];
                foreach (string reference in references)
                    if (!known.Contains(reference)) errors.Add(file.Key + ": " + field + " -> " + reference);
            }
        }
        using var metadata = JsonDocument.Parse(File.ReadAllText(Path.Combine(ledger, "schema-version.json")));
        int metadataRecordCount = metadata.RootElement.GetProperty("recordCount").GetInt32();
        var groups = files["groups.jsonl"].Where(g => new[] { "GENPROTO-MILCMD", "GENPAIR-CORE-TYPES", "GENPAIR-AV-TYPES" }.Contains(g["groupId"].GetString())).Select(g => new
        {
            groupId = g["groupId"].GetString(), doneRule = g["doneRule"].GetString(), status = g["status"].GetString(),
            required = g["requiredEvidenceIds"].EnumerateArray().Select(e => e.GetString()).ToArray(),
            evidence = g["requiredEvidenceIds"].EnumerateArray().Select(e => files["evidence.jsonl"].Single(v => v["evidenceId"].GetString() == e.GetString())).ToArray()
        }).ToArray();
        if (metadataRecordCount != files["records.jsonl"].Length) errors.Add("Record count metadata mismatch");
        foreach (var row in files["evidence.jsonl"].Where(e => e.TryGetValue("satisfactionState", out var state) && state.GetString() == "Satisfied"))
        {
            if (row["status"].GetString() != "Passed" || !row["executed"].GetBoolean()) errors.Add("Satisfied evidence was not passed and executed");
            string path = Path.Combine(Root, row["path"].GetString()!);
            if (!File.Exists(path) || Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) != row["hash"].GetString()) errors.Add("Satisfied evidence path/hash mismatch: " + row["evidenceId"].GetString() + " -> " + path);
            if (row.TryGetValue("sourceReport", out var sourceReport))
            {
                string sourcePath = Path.Combine(Root, sourceReport.GetString()!);
                if (!row.TryGetValue("sourceReportHash", out var sourceHash) || !File.Exists(sourcePath) || Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(sourcePath))) != sourceHash.GetString())
                    errors.Add("Satisfied evidence source path/hash mismatch: " + row["evidenceId"].GetString() + " -> " + sourcePath);
            }
        }
        foreach (var group in groups)
        {
            if (group.status != "Closed") continue;
            foreach (var evidence in group.evidence)
            {
                string? status = evidence.TryGetValue("status", out var value) ? value.GetString() : null;
                bool executed = evidence.TryGetValue("executed", out var execution) && execution.ValueKind == JsonValueKind.True;
                if (status != "Passed" || !executed) errors.Add(group.groupId + ": Closed without passed executed required evidence " + evidence["evidenceId"].GetString());
            }
        }
        string archive = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence", "closure-validation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        string report = Path.Combine(archive, "report.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            schemaVersion = "1.0.0", executed = true, kind = "IndependentLedgerReferenceAndClosureValidation",
            actualArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            files = names.Select(n => new { path = "WpfGfxShape/Ledger/" + n, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(ledger, n)))), actualCount = files[n].Length }),
            metadataRecordCount, actualRecordCount = files["records.jsonl"].Length,
            countMetadataEqual = metadataRecordCount == files["records.jsonl"].Length, groups, errors,
            stage4Closed = groups.Length == 3 && groups.All(g => g.status == "Closed") && errors.Count == 0
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Closure validation: " + report + " SHA256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(report))));
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors));
    }
}
