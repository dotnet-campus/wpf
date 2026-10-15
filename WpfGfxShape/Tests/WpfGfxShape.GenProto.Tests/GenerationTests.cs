using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MS.Internal.Csp;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class GenerationTests
{
    private static string Root => typeof(GenerationTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static string Relative(string path) => Path.GetRelativePath(Root, path).Replace('\\', '/');
    private static string Archive(string kind) => Path.Combine(Root, "WpfGfxShape", "Investigations", "GenProtoEvidence", kind + "-" + Guid.NewGuid().ToString("N"));
    private static void Save(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));

    [TestMethod]
    public void WhenOriginalRspRunsInIsolationThenGeneratedOutputsHaveAuditableDifferentials()
    {
        string archive = Archive("regeneration");
        Directory.CreateDirectory(archive);
        string mcg = Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg");
        string input = Path.Combine(archive, "authority");
        string output = Path.Combine(archive, "output");
        Directory.CreateDirectory(output);
        // Native generators assume the existing product directory skeleton, not existing output content.
        foreach (string directory in Directory.EnumerateDirectories(Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src"), "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(output, "src", Path.GetRelativePath(Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src"), directory)));
        var authority = new List<object>();
        foreach (string file in Directory.EnumerateFiles(mcg, "*", SearchOption.AllDirectories))
        {
            string target = Path.Combine(input, Path.GetRelativePath(mcg, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
            authority.Add(new { path = Relative(file), sha256 = Hash(file), copySha256 = Hash(target) });
        }
        foreach (string name in new[] { "CsPrimeParser.cs", "CsPrimeRuntime.cs" })
        {
            string file = Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/tools/csp", name);
            authority.Add(new { path = Relative(file), sha256 = Hash(file), copySha256 = Hash(file) });
        }
        Save(Path.Combine(archive, "authority.json"), new { version = "44615ed4b9f033922b3361ea02c02f173b8bf82e (Ledger revision; working tree identified by individual SHA256)", files = authority });
        string[] sources = File.ReadAllLines(Path.Combine(input, "main/Resources.rsp"))
            .Where(l => l.StartsWith("-s:", StringComparison.Ordinal))
            .Select(l => ResolveCase(input, l[3..].Replace('\\', Path.DirectorySeparatorChar))).ToArray();
        string[] parsed = CsPrimeParser.Parse(sources, false);
        string transformed = Path.Combine(archive, "transformed");
        Directory.CreateDirectory(transformed);
        for (int i = 0; i < parsed.Length; i++) File.WriteAllText(Path.Combine(transformed, i + "-" + Path.GetFileName(sources[i])), parsed[i]);
        var trees = parsed.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray();
        string[] framework = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        var refs = framework.Select(p => MetadataReference.CreateFromFile(p)).ToList();
        refs.Add(MetadataReference.CreateFromFile(typeof(CsPrimeRuntime).Assembly.Location));
        var compilation = CSharpCompilation.Create("IsolatedMilCodeGen_" + Guid.NewGuid().ToString("N"), trees, refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        using var binary = new MemoryStream();
        var emitted = compilation.Emit(binary);
        File.WriteAllLines(Path.Combine(archive, "compilation.txt"), emitted.Diagnostics.Select(d => d.ToString()));
        Console.WriteLine("GENPROTO archive: " + archive);
        Assert.IsTrue(emitted.Success, "Original rsp compile failed; see " + Path.Combine(archive, "compilation.txt"));
        binary.Position = 0;
        var context = new AssemblyLoadContext("GENPROTO-" + Guid.NewGuid(), isCollectible: true);
        try
        {
            var library = context.LoadFromStream(binary);
            var args = new[] { "-xmlFile:" + Path.Combine(input, "xml/Resource.xml"), "-xsdFile:" + Path.Combine(input, "xml/Resource.xsd"), "-outputDirectory:" + output };
            Save(Path.Combine(archive, "invocation.json"), new { executor = "MSTest in-process; CsPrimeParser.Parse -> Roslyn.Emit -> original ResourceGenerator.Main", args, runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription, architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(), protocolFingerprintIncludedByRsp = sources.Any(s => Path.GetFileName(s).Equals("ProtocolFingerprint.cs", StringComparison.OrdinalIgnoreCase)), algorithmChanges = "None; source debugging #line directives disabled, original algorithm sources unmodified" });
            library.GetType("MS.Internal.MilCodeGen.Main.ResourceGenerator", true)!.GetMethod("Main")!.Invoke(null, new object[] { args });
            var outputs = new List<object>();
            var failures = new List<string>();
            string diff = Path.Combine(archive, "diff");
            Directory.CreateDirectory(diff);
            foreach (string file in Directory.EnumerateFiles(output, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
            {
                string relative = Path.GetRelativePath(output, file);
                string baseline = Path.Combine(Root, "src/Microsoft.DotNet.Wpf", relative);
                string generatedText = File.ReadAllText(file);
                string? frozenText = File.Exists(baseline) ? File.ReadAllText(baseline) : null;
                bool exact = frozenText != null && File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(baseline));
                string Semantic(string text) => string.Join("|", CSharpSyntaxTree.ParseText(text).GetRoot().DescendantTokens().Select(t => t.Text));
                bool? semantic = file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && frozenText != null ? Semantic(generatedText) == Semantic(frozenText) : null;
                bool newlineOnly = frozenText != null && generatedText.Replace("\r\n", "\n") == frozenText.Replace("\r\n", "\n");
                bool equivalent = semantic == true || newlineOnly || exact;
                if (!equivalent) failures.Add(Relative(baseline));
                string delta = Path.Combine(diff, relative + ".json");
                Directory.CreateDirectory(Path.GetDirectoryName(delta)!);
                Save(delta, new { baselinePath = Relative(baseline), frozenText, generatedText, exact, semantic, newlineOnly, equivalent, classification = exact ? "ByteExact" : newlineOnly ? "CRLFOnly" : semantic == true ? "CSharpTriviaOnly" : "UnreviewedSemanticDifference", semanticRule = semantic == null ? "Native full text equality after CRLF normalization; no native ABI claim" : "Roslyn token sequence excluding trivia; namespace and authority identifiers retained" });
                outputs.Add(new { path = Relative(file), baselinePath = Relative(baseline), sha256 = Hash(file), baselineSha256 = File.Exists(baseline) ? Hash(baseline) : null, exact, semantic, diffPath = Relative(delta), diffSha256 = Hash(delta), protocolFacts = Regex.Matches(generatedText, @"(?m)^.*(?:MILCMD_|MilCmd|pack|FieldOffset|StructLayout|SDK_VERSION|SdkVersion).*$").Select(m => m.Value).ToArray() });
            }
            Save(Path.Combine(archive, "results.json"), new { executed = true, status = "GeneratedDifferentialRecorded-NotClosureApproved", utc = DateTimeOffset.UtcNow, outputs, authorityManifestSha256 = Hash(Path.Combine(archive, "authority.json")), productionLayoutArchitectures = "Not executed: Win32/x64/arm64 native-to-managed comparison remains required" });
            Assert.IsNotEmpty(outputs, "Generator produced no outputs.");
            Assert.AreEqual(0, failures.Count, "Unreviewed differences: " + string.Join(Environment.NewLine, failures));
        }
        finally { context.Unload(); }
    }

    [TestMethod]
    public void WhenRegenerationArchiveIsReviewedThenSummaryAndFamilyBoundariesAreRecorded()
    {
        string directory = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence");
        var reports = Directory.EnumerateFiles(directory, "results.json", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        var summaries = new List<object>();
        foreach (string report in reports)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(report));
            var outputs = document.RootElement.GetProperty("outputs").EnumerateArray().ToArray();
            summaries.Add(new { path = Relative(report), sha256 = Hash(report), outputs = outputs.Length,
                exact = outputs.Count(o => o.GetProperty("exact").GetBoolean()),
                semanticEqual = outputs.Count(o => o.GetProperty("semantic").ValueKind == JsonValueKind.True),
                semanticDifferent = outputs.Where(o => o.GetProperty("semantic").ValueKind == JsonValueKind.False).Select(o => o.GetProperty("baselinePath").GetString()).ToArray(),
                nativeOrMissingBaseline = outputs.Where(o => o.GetProperty("semantic").ValueKind == JsonValueKind.Null).Select(o => o.GetProperty("baselinePath").GetString()).ToArray() });
        }
        var families = new List<object>();
        foreach (string family in new[] { "core", "av" })
        {
            string[] paths = new[] { "include/wgx_" + family + "_types.w", "include/processed/wgx_" + family + "_types.h", "include/processed/wgx_" + family + "_types.cs" }
                .Select(p => "src/Microsoft.DotNet.Wpf/src/WpfGfx/" + p)
                .Append("src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_" + family + "_types.cs").ToArray();
            families.Add(new { family, inputs = paths.Select(p => new { path = p, sha256 = Hash(Path.Combine(Root, p)) }).ToArray(),
                generatedByResourcesRsp = reports.Any(r => File.ReadAllText(r).Contains("wgx_" + family + "_types", StringComparison.Ordinal)),
                layout = "Not executed. These .w/processed/Common families are distinct from Resources.rsp output; no cross-architecture equivalence inferred." });
        }
        string archive = Archive("review");
        Directory.CreateDirectory(archive);
        Save(Path.Combine(archive, "summary.json"), new { executed = true, utc = DateTimeOffset.UtcNow, summaries, families,
            evidence = Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories).Where(p => Path.GetFileName(p) is "authority.json" or "invocation.json" or "results.json" or "validation.json").Select(p => new { path = Relative(p), sha256 = Hash(p) }).ToArray(),
            status = "Partial: actual regeneration complete, native semantic/layout and production mapping gaps not closed" });
        Console.WriteLine("GENPROTO review: " + archive);
        Assert.IsNotEmpty(reports);
    }

    private static string ResolveCase(string root, string relative)
    {
        string current = root;
        foreach (string part in relative.Split(Path.DirectorySeparatorChar))
            current = Directory.EnumerateFileSystemEntries(current).Single(p => Path.GetFileName(p).Equals(part, StringComparison.OrdinalIgnoreCase));
        return current;
    }

    [TestMethod]
    public void WhenLedgerIsReadLineByLineThenAllReferencesAndRequiredFieldsAreValidated()
    {
        string archive = Archive("ledger-validation");
        Directory.CreateDirectory(archive);
        string ledger = Path.Combine(Root, "WpfGfxShape/Ledger");
        var errors = new List<string>();
        var rows = new List<(string File, int Line, JsonElement Value)>();
        foreach (string name in new[] { "records.jsonl", "groups.jsonl", "evidence.jsonl", "work-packages.jsonl" })
        {
            string file = Path.Combine(ledger, name);
            File.Copy(file, Path.Combine(archive, name));
            int line = 0;
            foreach (string text in File.ReadLines(file))
            {
                line++;
                try
                {
                    using var doc = JsonDocument.Parse(text);
                    if (doc.RootElement.ValueKind != JsonValueKind.Object) errors.Add($"{name}:{line}: not object");
                    else rows.Add((name, line, doc.RootElement.Clone()));
                }
                catch (JsonException e) { errors.Add($"{name}:{line}: {e.Message}"); }
            }
        }
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            string key = row.File switch { "records.jsonl" => "recordId", "groups.jsonl" => "groupId", "evidence.jsonl" => "evidenceId", _ => "workPackageId" };
            if (!row.Value.TryGetProperty(key, out var id)) errors.Add($"{row.File}:{row.Line}: missing {key}");
            else if (!ids.Add(id.GetString()!)) errors.Add($"Duplicate ID: {id.GetString()}");
            if (!row.Value.TryGetProperty("schemaVersion", out var schema) || schema.GetString() != "1.0.0") errors.Add($"{row.File}:{row.Line}: schemaVersion");
            string required = row.File switch
            {
                "records.jsonl" => "recordType scopeDisposition nativeProject nativePath fileKind buildParticipation sourceAuthority originalResponsibility keySymbols pchAndDirectIncludes dependencies platformDependencies generatedGroupIds typeFamilyIds cycleGroupIds abiContractIds investigationBatch implementationBatch proposedCSharpPath status readinessBlockers requiredEvidenceIds satisfiedEvidenceIds knownDeviations ownerWorkPackageId lastReviewedSession notesDecisionIds",
                "groups.jsonl" => "groupType memberRecordIds authorityRecordIds consumerRecordIds invariants readinessRule doneRule requiredEvidenceIds status blocked lastReviewedSession",
                "evidence.jsonl" => "evidenceType status subjectIds path hash executed",
                _ => ""
            };
            foreach (string property in required.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                if (!row.Value.TryGetProperty(property, out _)) errors.Add($"{row.File}:{row.Line}: missing {property}");
        }
        string[] referenceFields = { "dependencies", "generatedGroupIds", "typeFamilyIds", "cycleGroupIds", "memberRecordIds", "authorityRecordIds", "consumerRecordIds", "requiredEvidenceIds", "satisfiedEvidenceIds", "subjectIds", "ownerWorkPackageId" };
        foreach (var row in rows)
        {
            foreach (string property in referenceFields)
            {
                if (!row.Value.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null) continue;
                IEnumerable<string?> references = value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().Select(v => v.GetString()) : new[] { value.GetString() };
                foreach (string? id in references) if (id != null && !ids.Contains(id)) errors.Add($"{row.File}:{row.Line}:{property}: unresolved {id}");
            }
            if (row.Value.TryGetProperty("status", out var status) && status.GetString() == "Accepted")
            {
                if (row.Value.TryGetProperty("blocked", out var blocked) && blocked.GetBoolean()) errors.Add($"{row.File}:{row.Line}: Accepted blocked");
                if (row.Value.TryGetProperty("readinessBlockers", out var blockers) && blockers.GetArrayLength() != 0) errors.Add($"{row.File}:{row.Line}: Accepted blockers");
                foreach (var required in row.Value.GetProperty("requiredEvidenceIds").EnumerateArray())
                {
                    var evidence = rows.FirstOrDefault(r => r.File == "evidence.jsonl" && r.Value.GetProperty("evidenceId").GetString() == required.GetString());
                    if (evidence.Value.ValueKind == JsonValueKind.Undefined || !evidence.Value.GetProperty("executed").GetBoolean() || evidence.Value.GetProperty("status").GetString() != "Passed") errors.Add($"{row.File}:{row.Line}: Accepted evidence not passed: {required}");
                }
            }
        }
        Save(Path.Combine(archive, "validation.json"), new { executed = true, utc = DateTimeOffset.UtcNow, rows = rows.Count, inputSha256 = Directory.EnumerateFiles(archive, "*.jsonl").Select(p => new { path = Relative(p), sha256 = Hash(p) }).ToArray(), errors, scope = "JSONL parse, schema version, required fields, unique IDs, selected foreign keys, Accepted invariants. Full production project/multiview closure is NOT asserted." });
        Console.WriteLine("Ledger validation archive: " + archive);
        Assert.AreEqual(0, errors.Count, string.Join(Environment.NewLine, errors));
    }
}
