using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class MilcmdPhysicalClosureTests
{
    private static string Root => typeof(MilcmdPhysicalClosureTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
    private static string Full(string path) => Path.Combine(Root, path);
    private static string Relative(string path) => Path.GetRelativePath(Root, path).Replace('\\', '/');
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>Freezes a conservative recursive source envelope and records signatures and unsatisfied execution obligations separately.</summary>
    [TestMethod]
    public void WhenMilcmdPhysicalDependenciesAreFrozenThenEveryFileAndDispatchObligationIsReported()
    {
        const string native = "src/Microsoft.DotNet.Wpf/src/WpfGfx/";
        const string core = "WpfGfxShape/Code/WpfGfxShape/Core/";
        string evidenceRoot = Full("WpfGfxShape/Investigations/GenProtoEvidence");
        string run = Guid.NewGuid().ToString("N");
        string directory = Path.Combine(evidenceRoot, "milcmd-physical-" + run);
        Directory.CreateDirectory(directory);
        string Latest(string pattern, string name) => Directory.EnumerateDirectories(evidenceRoot, pattern).Select(d => Path.Combine(d, name)).Where(File.Exists).MaxBy(File.GetLastWriteTimeUtc) ?? throw new InvalidOperationException(pattern);
        var group = File.ReadLines(Full("WpfGfxShape/Ledger/groups.jsonl")).Select(l => JsonNode.Parse(l)!.AsObject()).Single(g => (string?)g["groupId"] == "GENPROTO-MILCMD");
        string[] entries = new[] { "memberRecordIds", "authorityRecordIds", "consumerRecordIds", "requiredEvidenceIds" }.SelectMany(k => group[k]!.AsArray().Select(v => v!.GetValue<string>())).Distinct().Order(StringComparer.Ordinal).ToArray();
        Assert.HasCount(9, entries);
        var records = File.ReadLines(Full("WpfGfxShape/Ledger/records.jsonl")).Select(l => JsonNode.Parse(l)!.AsObject()).ToArray();
        var entryFiles = records.Where(r => entries.Contains((string?)r["recordId"])).Select(r => new { id = (string?)r["recordId"], path = r["nativePath"]!.GetValue<string>() }).ToArray();
        Assert.HasCount(6, entryFiles);
        // A conservative directory envelope prevents omitted siblings from being silently excluded by a textual include resolver.
        string[] roots = [native, "src/Microsoft.DotNet.Wpf/src/Common/Graphics/", "src/Microsoft.DotNet.Wpf/src/PresentationCore/", "WpfGfxShape/Code/WpfGfxShape/", "WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/"];
        string[] extensions = [".cs", ".cpp", ".c", ".h", ".hpp", ".inl", ".w", ".xml", ".xsd", ".rsp", ".proj", ".vcxproj", ".csproj", ".props", ".targets", ".cmd", ".pl", ".rc", ".def", ".inc"];
        // Retain the original audit envelope without recursively enrolling unrelated new files.
        using var baseline = JsonDocument.Parse(File.ReadAllText(Full("WpfGfxShape/Investigations/GenProtoEvidence/milcmd-physical-05f3d6983cde4339b77bcf1641a2dc50/source-manifest.json")));
        var files = baseline.RootElement.GetProperty("files").EnumerateArray().Select(f => Full(f.GetProperty("path").GetString() == "WpfGfxShape/Code/WpfGfxShape/Class1.cs"
            ? "WpfGfxShape/Tests/ReferenceData/GenProto/EmptyClass1.cs"
            : f.GetProperty("path").GetString()!)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entryFiles) files.Add(Full(entry.path));
        var lookup = files.GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key!, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>(files);
        var edges = new List<object>();
        var unresolved = new List<object>();
        while (queue.TryDequeue(out string? file))
        {
            string text = File.ReadAllText(file);
            var references = Regex.Matches(text, "(?m)^\\s*#\\s*include\\s*[<\"]([^>\"]+)[>\"]|(?:Include|Project|Projects|schemaLocation)=\"([^\"]+)\"|(?m)^-s:([^\\r\\n]+)");
            foreach (Match match in references)
            {
                string value = match.Groups.Cast<Group>().Skip(1).First(g => g.Success).Value.Trim().Replace('\\', '/');
                string candidate = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, value));
                string[] resolved = File.Exists(candidate) && candidate.StartsWith(Root, StringComparison.OrdinalIgnoreCase) ? [candidate]
                    : lookup.GetValueOrDefault(Path.GetFileName(value)) ?? [];
                if (resolved.Length == 0) unresolved.Add(new { from = Relative(file), reference = value, reason = "SDK/property/conditional or outside indexed envelope; not silently satisfied" });
                foreach (string target in resolved)
                {
                    edges.Add(new { from = Relative(file), reference = value, to = Relative(target), resolution = resolved.Length > 1 ? "ConservativeAllNameCandidates" : "PhysicalFile" });
                    if (files.Add(target)) queue.Enqueue(target);
                }
            }
        }
        var frozen = files.Order(StringComparer.Ordinal).Select(p => new { path = Relative(p), sha256 = Hash(p), bytes = new FileInfo(p).Length }).ToArray();
        string manifest = Path.Combine(directory, "source-manifest.json");
        File.WriteAllText(manifest, JsonSerializer.Serialize(new { entries, entryFiles, roots, extensions, boundary = "Conservative recursive physical envelope, not all-C execution closure; every unresolved edge retained", files = frozen, edges, unresolved }, Options));
        string dispatchPath = native + "core/uce/generated_process_message.inl";
        string dispatch = File.ReadAllText(Full(dispatchPath));
        var matches = Regex.Matches(dispatch, @"case (MilCmd\w+):");
        var dispatchRows = matches.Select((m, i) =>
        {
            string body = dispatch[m.Index..(i + 1 < matches.Count ? matches[i + 1].Index : dispatch.Length)];
            return new { command = m.Groups[1].Value, packetTypes = Regex.Matches(body, @"\bMILCMD_\w+\b").Select(x => x.Value).Distinct().ToArray(), calls = Regex.Matches(body, @"(?:IFC\(|->)(\w+)\s*\(").Select(x => x.Groups[1].Value).Distinct().ToArray(), body, executionBinding = "Not inferred from source or unrelated Passed test name" };
        }).ToArray();
        var managed = files.Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && Relative(p).StartsWith("WpfGfxShape/Code/", StringComparison.Ordinal)).Select(p => new { path = Relative(p), tree = CSharpSyntaxTree.ParseText(File.ReadAllText(p)).GetRoot() }).ToArray();
        var signatures = managed.SelectMany(f => f.tree.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.ValueText is "ProcessUpdate" or "UpdateResource" or "Route" or "Write" or "GetResourceTypeForCommand").Select(m => new { f.path, owner = m.Ancestors().OfType<TypeDeclarationSyntax>().First().Identifier.ValueText, method = m.Identifier.ValueText, returns = m.ReturnType.ToString(), parameters = m.ParameterList.ToString(), body = m.Body?.ToString() ?? m.ExpressionBody?.ToString() })).ToArray();
        var declarations = frozen.Where(f => f.path.Contains("/Generated/", StringComparison.Ordinal) || f.path.Contains("/processed/", StringComparison.Ordinal) || f.path.StartsWith(core, StringComparison.Ordinal)).Select(f => new { f.path, f.sha256, structs = Regex.Matches(File.ReadAllText(Full(f.path)), @"\b(?:struct|enum)\s+(\w+)").Select(m => m.Groups[1].Value).ToArray(), fields = Regex.Matches(File.ReadAllText(Full(f.path)), @"(?m)^.*(?:FieldOffset|StructLayout|#pragma pack|HMIL_RESOURCE|MIL_RESOURCE_TYPE|MilResourceType).*$").Select(m => m.Value).ToArray() }).ToArray();
        Assert.IsGreaterThan(0, dispatchRows.Length);
        Assert.IsGreaterThan(0, signatures.Length);
        string generation = Latest("regeneration-*", "results.json");
        using var generated = JsonDocument.Parse(File.ReadAllText(generation));
        var outputs = generated.RootElement.GetProperty("outputs").EnumerateArray().Select(o => new { path = o.GetProperty("path").GetString()!, baseline = o.GetProperty("baselinePath").GetString()!, expected = o.GetProperty("sha256").GetString(), exact = o.GetProperty("exact").GetBoolean(), semantic = o.GetProperty("semantic").Clone(), diffPath = o.GetProperty("diffPath").GetString()! }).ToArray();
        foreach (var output in outputs) Assert.AreEqual(output.expected, Hash(Full(output.path)));
        string aot = Full("WpfGfxShape/artifacts/sandbox-readback/dispatcher-fixed-full/Results/dispatcher-fixed-full.trx");
        var trx = XDocument.Load(aot);
        XNamespace ns = trx.Root!.Name.Namespace;
        var execution = trx.Descendants(ns + "UnitTestResult").Select(r => new { testId = (string?)r.Attribute("testId"), name = (string?)r.Attribute("testName"), outcome = (string?)r.Attribute("outcome") }).ToArray();
        string[] reports = [generation, Path.Combine(Path.GetDirectoryName(generation)!, "authority.json"), Latest("consumer-assertions-*", "assertions.json"), Latest("factory-physical-*", "report.json"), Latest("pen-delivery-*", "report.json"), aot, Path.Combine(Path.GetDirectoryName(aot)!, "aot-identity.json")];
        var bindings = reports.Select((p, i) =>
        {
            string copy = Path.Combine(directory, i + "-" + Path.GetFileName(p));
            File.Copy(p, copy);
            Assert.AreEqual(Hash(p), Hash(copy));
            return new { source = Relative(p), copy = Relative(copy), sha256 = Hash(copy) };
        }).ToArray();
        var changed = frozen.Where(f => Hash(Full(f.path)) != f.sha256).ToArray();
        Assert.HasCount(0, changed);
        string report = Path.Combine(directory, "report.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new { runId = run, executed = true, entries, physicalFileCount = frozen.Length, dependencyEdgeCount = edges.Count, unresolvedEdgeCount = unresolved.Count, sourceManifest = Relative(manifest), sourceManifestSha256 = Hash(manifest), sourceStable = changed.Length == 0, dispatchRows, signatures, declarations, outputs, bindings, aotRunId = (string?)trx.Root.Attribute("id"), execution, aotExecutedThisRun = false, nativeLayoutExecutedByThisTest = false, independentSatisfied = new[] { "Conservative recursive source envelope frozen and read back", "Full native dispatch case bodies and actual managed update interface signatures enumerated", "Current generation output hash bindings independently checked; per-output full text/token differential retained" }, remainingRequired = new[] { "Unresolved property/SDK dependency edges require build-evaluated classification", "Per-command native/managed wire size/offset/enum denominator compiler comparison beyond existing finite layout evidence", "Every producer/resource reference/update command bound to a specific actual production execution assertion; no name-based inference", "Historical CLR hash/input authority binding", "ARM64 native toolchain and real execution" }, parentRequiredSatisfied = false, stage4Closed = false, errors = Array.Empty<string>() }, Options));
        using var readback = JsonDocument.Parse(File.ReadAllText(report));
        Assert.AreEqual(frozen.Length, readback.RootElement.GetProperty("physicalFileCount").GetInt32());
        string ledgerPath = Full("WpfGfxShape/Ledger/evidence.jsonl");
        var ledger = File.ReadLines(ledgerPath).Select(l => JsonNode.Parse(l)!.AsObject()).ToArray();
        foreach (var row in ledger.Where(r => (string?)r["evidenceId"] is "EVID-GENPROTO-MILCMD-CLOSURE" or "EVID-GENPROTO-MILCMD-REGEN-DIFF"))
        {
            row["path"] = Relative(report); row["hash"] = Hash(report); row["runId"] = run; row["executed"] = true;
            row["notes"] = "Current generator and input authority manifest exist and were executed; current output differential and recursive source/signature evidence attached. Historical binding is distinct, not a missing current manifest.";
            row["blockerReason"] = "Independent source-envelope/signature/current-differential clauses recorded in report; unresolved dependency classification, full command native wire/compiler and assertion-specific execution mapping, historical binding and ARM64 remain required. Parent Blocked, not all clauses Open.";
        }
        File.WriteAllLines(ledgerPath, ledger.Select(r => r.ToJsonString()));
        Console.WriteLine("MILCMD physical report: " + report + " SHA256=" + Hash(report));
    }

    /// <summary>Archives the most recent original full TRX together with the exact current test sources.</summary>
    [TestMethod]
    public void WhenMilcmdFullRunIsArchivedThenTrxAndSourceHashesAreReadBack()
    {
        string tests = Full("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests");
        string trx = Directory.EnumerateFiles(Path.Combine(tests, "TestResults"), "*.trx").Where(p => XDocument.Load(p).Descendants().Any(e => e.Name.LocalName == "Counters" && (int?)e.Attribute("total") >= 35)).MaxBy(File.GetLastWriteTimeUtc) ?? throw new InvalidOperationException("No full GenProto TRX");
        string directory = Full("WpfGfxShape/Investigations/GenProtoEvidence/milcmd-delivery-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string copy = Path.Combine(directory, "genproto-full.trx");
        File.Copy(trx, copy);
        Assert.AreEqual(Hash(trx), Hash(copy));
        var document = XDocument.Load(copy);
        string buildReport = Directory.EnumerateDirectories(Full("WpfGfxShape/Investigations/GenProtoEvidence"), "milcmd-build-contract-*").Select(d => Path.Combine(d, "report.json")).MaxBy(File.GetLastWriteTimeUtc) ?? throw new InvalidOperationException("No build contract report");
        using var build = JsonDocument.Parse(File.ReadAllText(buildReport));
        Assert.AreEqual(721, build.RootElement.GetProperty("audit").GetArrayLength());
        foreach (var input in build.RootElement.GetProperty("testSources").EnumerateArray().Concat(build.RootElement.GetProperty("sources").EnumerateArray()).Concat(build.RootElement.GetProperty("wire").GetProperty("sources").EnumerateArray()))
            Assert.AreEqual(input.GetProperty("Sha256").GetString(), Hash(Full(input.GetProperty("Path").GetString()!)));
        Assert.AreEqual(build.RootElement.GetProperty("assembly").GetProperty("Sha256").GetString(), Hash(typeof(MilcmdPhysicalClosureTests).Assembly.Location));
        string target = Directory.EnumerateFiles(Path.Combine(tests, "TestResults"), "*.trx").Where(p => { var d = XDocument.Load(p); return d.Descendants().Any(e => e.Name.LocalName == "Counters" && (int?)e.Attribute("total") == 1) && d.ToString().Contains("WhenBuildContractsBoundMilcmdThenEveryHistoricalUnresolvedEdgeRetainsAnAuthorityAndDisposition", StringComparison.Ordinal); }).MaxBy(File.GetLastWriteTimeUtc) ?? throw new InvalidOperationException("No build contract target TRX");
        string report = Path.Combine(directory, "report.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new { original = Relative(trx), copy = Relative(copy), trxSha256 = Hash(copy), runId = (string?)document.Root!.Attribute("id"), counters = document.Descendants().Single(e => e.Name.LocalName == "Counters").ToString(), target = new { path = Relative(target), sha256 = Hash(target), runId = (string?)XDocument.Load(target).Root!.Attribute("id") }, buildContract = new { path = Relative(buildReport), sha256 = Hash(buildReport), fullyParsed = true, classificationCounts = build.RootElement.GetProperty("classificationCounts"), auditScope = build.RootElement.GetProperty("auditScope"), productionUndeterminedCount = build.RootElement.GetProperty("productionUndeterminedCount") }, sources = Directory.EnumerateFiles(tests, "*.cs").Select(p => new { path = Relative(p), sha256 = Hash(p) }), assemblyIdentity = typeof(MilcmdPhysicalClosureTests).Assembly.FullName, assemblyMvid = typeof(MilcmdPhysicalClosureTests).Assembly.ManifestModule.ModuleVersionId, assemblySha256 = Hash(typeof(MilcmdPhysicalClosureTests).Assembly.Location) }, Options));
        using var readback = JsonDocument.Parse(File.ReadAllText(report));
        Console.WriteLine("MILCMD full archive: " + report + " SHA256=" + Hash(report));
    }
}
