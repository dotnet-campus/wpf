using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class RequiredIntegrationAuditTests
{
    /// <summary>Verifies complete regeneration artifacts and independently executed media ownership results without promoting parent gates.</summary>
    [TestMethod]
    public void WhenRequiredDifferencesAreIntegratedThenRawArtifactsAndFiniteScopesAreVerified()
    {
        string root = typeof(RequiredIntegrationAuditTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string Full(string path) => Path.Combine(root, path);
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        string Relative(string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
        int ReadLeaves(JsonElement element) => element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().Sum(p => ReadLeaves(p.Value)),
            JsonValueKind.Array => element.EnumerateArray().Sum(ReadLeaves),
            _ => element.GetRawText().Length >= 0 ? 1 : 0
        };
        string Semantic(string text) => string.Join("|", CSharpSyntaxTree.ParseText(text).GetRoot().DescendantTokens().Select(t => t.Text));
        string regeneration = Full("WpfGfxShape/Investigations/GenProtoEvidence/regeneration-941a112ea3d44800ad1527f53bf9528c");
        var artifacts = new List<object>();
        foreach (string path in new[] { "authority.json", "invocation.json", "results.json" })
        {
            string file = Path.Combine(regeneration, path);
            using var json = JsonDocument.Parse(File.ReadAllText(file));
            artifacts.Add(new { path = Relative(file), sha256 = Hash(file), leavesRead = ReadLeaves(json.RootElement) });
        }
        using var authority = JsonDocument.Parse(File.ReadAllText(Path.Combine(regeneration, "authority.json")));
        foreach (var file in authority.RootElement.GetProperty("files").EnumerateArray())
            Assert.AreEqual(file.GetProperty("sha256").GetString(), Hash(Full(file.GetProperty("path").GetString()!)));
        using var results = JsonDocument.Parse(File.ReadAllText(Path.Combine(regeneration, "results.json")));
        Assert.AreEqual(results.RootElement.GetProperty("authorityManifestSha256").GetString(), Hash(Path.Combine(regeneration, "authority.json")));
        var differences = new List<object>();
        int exactCount = 0, equivalentCount = 0;
        foreach (var output in results.RootElement.GetProperty("outputs").EnumerateArray())
        {
            string generated = Full(output.GetProperty("path").GetString()!);
            string baseline = Full(output.GetProperty("baselinePath").GetString()!);
            string diff = Full(output.GetProperty("diffPath").GetString()!);
            Assert.AreEqual(output.GetProperty("sha256").GetString(), Hash(generated));
            Assert.AreEqual(output.GetProperty("baselineSha256").GetString(), Hash(baseline));
            Assert.AreEqual(output.GetProperty("diffSha256").GetString(), Hash(diff));
            using var delta = JsonDocument.Parse(File.ReadAllText(diff));
            int leaves = ReadLeaves(delta.RootElement);
            string before = File.ReadAllText(baseline), after = File.ReadAllText(generated);
            Assert.AreEqual(before, delta.RootElement.GetProperty("frozenText").GetString());
            Assert.AreEqual(after, delta.RootElement.GetProperty("generatedText").GetString());
            bool exact = File.ReadAllBytes(generated).SequenceEqual(File.ReadAllBytes(baseline));
            bool newlineOnly = before.Replace("\r\n", "\n") == after.Replace("\r\n", "\n");
            bool semantic = generated.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && Semantic(before) == Semantic(after);
            Assert.AreEqual(output.GetProperty("exact").GetBoolean(), exact);
            Assert.IsTrue(exact || newlineOnly || semantic, baseline);
            if (exact) exactCount++;
            equivalentCount++;
            differences.Add(new { path = Relative(baseline), generatedSha256 = Hash(generated), baselineSha256 = Hash(baseline), diffSha256 = Hash(diff), exact, newlineOnly, semantic, leavesRead = leaves });
        }
        var media = new List<object>();
        var runs = new List<object>();
        foreach (bool candidate in new[] { true, false })
        {
            string side = candidate ? "candidate" : "reference";
            string directory = Full("WpfGfxShape/Investigations/MediaOwnershipRetry/" + side + "-ownership");
            string trx = Path.Combine(directory, "TestResults", side + "-ownership.trx");
            var document = XDocument.Load(trx);
            var counters = document.Descendants().Single(e => e.Name.LocalName == "Counters");
            Assert.AreEqual(3, (int?)counters.Attribute("total"));
            Assert.AreEqual(3, (int?)counters.Attribute("passed"));
            Assert.AreEqual(0, (int?)counters.Attribute("failed"));
            runs.Add(new { path = Relative(trx), sha256 = Hash(trx), runId = (string?)document.Root!.Attribute("id"), passed = 3 });
            foreach (string scenario in new[] { "RejectedSend", "UncommittedDestroy", "CommittedDestroy" })
            {
                string file = Path.Combine(directory, "Results", $"required-media-{candidate}-{scenario}.json");
                using var json = JsonDocument.Parse(File.ReadAllText(file));
                var item = json.RootElement;
                Assert.AreEqual(1u, item.GetProperty("before").GetUInt32());
                Assert.AreEqual(1u, item.GetProperty("after").GetUInt32());
                Assert.IsTrue(item.GetProperty("balanced").GetBoolean());
                Assert.IsFalse(item.GetProperty("synchronousTransport").GetBoolean());
                Assert.AreEqual(scenario == "RejectedSend" ? unchecked((int)0x80070057) : 0, item.GetProperty("sent").GetInt32());
                var transport = item.GetProperty("calls").EnumerateArray().Single(c => c.GetProperty("operation").GetString() == "transportAddRef");
                Assert.AreEqual(2u, transport.GetProperty("references").GetUInt32());
                string progress = Path.Combine(directory, "Results", $"required-progress-{candidate}-{scenario}.json");
                using var stage = JsonDocument.Parse(File.ReadAllText(progress));
                Assert.AreEqual("afterShutdownAndRootRelease", stage.RootElement.GetProperty("operation").GetString());
                media.Add(new { path = Relative(file), sha256 = Hash(file), leavesRead = ReadLeaves(item), progressSha256 = Hash(progress), data = item.Clone() });
            }
        }
        var required = File.ReadLines(Full("WpfGfxShape/Ledger/evidence.jsonl")).Select(l => JsonSerializer.Deserialize<JsonElement>(l))
            .Where(e => e.GetProperty("evidenceId").GetString() is "EVID-GENPROTO-MILCMD-CLOSURE" or "EVID-GENPROTO-MILCMD-REGEN-DIFF").ToArray();
        Assert.AreEqual(2, required.Length);
        string destination = Full("WpfGfxShape/Investigations/GenProtoEvidence/required-integration");
        Directory.CreateDirectory(destination);
        File.WriteAllText(Path.Combine(destination, "report.json"), JsonSerializer.Serialize(new
        {
            executed = true, artifacts, differences, outputs = differences.Count, exactCount, equivalentCount, media, runs, required,
            testSourceSha256 = Hash(Full("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/RequiredIntegrationAuditTests.cs")),
            testAssemblySha256 = Hash(typeof(RequiredIntegrationAuditTests).Assembly.Location),
            regenerationScope = "Original Resources.rsp outputs only; full text and Roslyn tokens verified. No ProtocolFingerprint or historical hash equality claim.",
            mediaScope = "CrossThread public API: rejected transport reference and committed/implicitly flushed destroy balance while caller root stays alive. Not a queue-discard or final destruction oracle.",
            autonomousRemaining = new[] { "transport packet-specific execution", "media queue-discard independent oracle", "full required physical/assertion mapping reconciliation" },
            externalRemaining = new[] { "historical generation/build binding", "ARM64 compiler/runtime" },
            allBlockedExternal = false, parentBlocked = true, stage4Closed = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Required integration: " + destination);
    }
}
