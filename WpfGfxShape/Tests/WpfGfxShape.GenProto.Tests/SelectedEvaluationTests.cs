using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class SelectedEvaluationTests
{
    /// <summary>Records selected compile inputs without confusing evaluation with compilation or historical authority.</summary>
    [TestMethod]
    public void WhenSelectedNativeProjectsAreEvaluatedThenEveryConfigurationRetainsItsActualSourcesAndAuthority()
    {
        string root = typeof(SelectedEvaluationTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value ?? throw new InvalidOperationException("RepositoryRoot missing");
        string Full(string p) => Path.GetFullPath(Path.Combine(root, p));
        string Relative(string p) => Path.GetRelativePath(root, p).Replace('\\', '/');
        string Hash(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
        object Identity(string p) => new { path = Relative(p), sha256 = Hash(p) };
        string[] Paths(string value) => value.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string directory = Full("WpfGfxShape/Investigations/GenProtoEvidence/c5-selected-evaluation");
        string[] files = Directory.GetFiles(directory, "*.evaluation.xml").Order(StringComparer.Ordinal).ToArray();
        Assert.HasCount(20, files);
        var selectedSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var directEdges = new List<(string From, string Reference, string? Target)>();
        var evaluations = files.Select(path =>
        {
            var xml = XElement.Load(path);
            string project = xml.Attribute("Project")!.Value;
            string Value(string key) => xml.Element("Properties")!.Elements("Property").Single(p => p.Attribute("Name")!.Value == key).Value;
            bool generator = project.EndsWith("mcg.proj", StringComparison.OrdinalIgnoreCase);
            string platform = Value("Platform");
            string configuration = Value("Configuration");
            string context = Path.GetFileName(project) + " " + configuration + "|" + platform;
            var properties = xml.Element("Properties")!.Elements().ToDictionary(p => p.Attribute("Name")!.Value, p => p.Value);
            string[] environmentIncludes = Paths(Value("IncludePath"));
            string[] externalIncludes = Paths(Value("ExternalIncludePath"));
            bool missingToolchain = !generator && string.IsNullOrWhiteSpace(Value("VCToolsInstallDir"));
            if (missingToolchain)
            {
                Assert.AreEqual("arm64", platform, context + ": unexpected missing non-ARM64 toolchain");
                Assert.AreEqual("", Value("VCToolsVersion"), context);
                Assert.AreEqual("", Value("WindowsSdkDir"), context);
                Assert.HasCount(0, environmentIncludes, context);
                Assert.HasCount(0, externalIncludes, context);
            }
            if (!generator && !missingToolchain)
            {
                Assert.AreEqual("v143", Value("PlatformToolset"), context);
                Assert.AreEqual("14.44.35207", Value("VCToolsVersion"), context);
                Assert.AreEqual("10.0.26100.0", Value("WindowsTargetPlatformVersion"), context);
                Assert.IsTrue(Directory.Exists(Value("WindowsSdkDir")), context);
            }
            var imports = xml.Element("Imports")!.Elements().Select(i => new
            {
                source = Identity(i.Attribute("Path")!.Value), importingProject = Identity(i.Attribute("ImportingProject")!.Value), line = (int)i.Attribute("Line")!
            }).ToArray();
            Assert.IsGreaterThan(0, imports.Length, context);
            var units = xml.Element("Items")!.Elements().Where(i => i.Attribute("Type")!.Value == "ClCompile").Select(item =>
            {
                string Metadata(string name) => item.Elements("Metadata").SingleOrDefault(m => m.Attribute("Name")!.Value == name)?.Value ?? "";
                string source = item.Attribute("FullPath")!.Value;
                selectedSources.Add(Relative(source));
                string[] additional = Paths(Metadata("AdditionalIncludeDirectories"));
                string[] ordered = additional.Concat(environmentIncludes).Concat(externalIncludes).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                var headers = new[] { "windows.h", "sal.h", "stddef.h" }.Select(name =>
                {
                    string? selected = ordered.Select(p => Path.Combine(p, name)).FirstOrDefault(File.Exists);
                    if (!missingToolchain) Assert.IsNotNull(selected, context + " " + source + " " + name + " in evaluated compile paths");
                    return new { name, source = selected == null ? null : Identity(selected), disposition = selected == null ? "MissingSelectedToolchainInput" : "ResolvedByEvaluatedCompileSearchPathsNotProofOfInclusion" };
                }).ToArray();
                var directives = Regex.Matches(File.ReadAllText(source), "(?m)^\\s*#\\s*include\\s*(?<operand>[^\\r\\n]+)").Select(match =>
                {
                    string operand = match.Groups["operand"].Value.Trim();
                    var literal = Regex.Match(operand, "^[\"<](?<name>[^\">]+)[\">]");
                    string? reference = literal.Success ? literal.Groups["name"].Value : null;
                    string[] search = operand.StartsWith('"') ? new[] { Path.GetDirectoryName(source)! }.Concat(ordered).ToArray() : ordered;
                    string? target = reference == null ? null : search.Select(p => Path.GetFullPath(Path.Combine(p, reference))).FirstOrDefault(File.Exists);
                    directEdges.Add((Relative(source), reference ?? operand, target == null ? null : Relative(target)));
                    return new { operand, target = target == null ? null : Identity(target), condition = "NotPreprocessed; conditional applicability remains unknown", disposition = reference == null ? "MacroIncludeNeedsPreprocessing" : target == null ? "UnresolvedLiteralInclude" : "LiteralCandidateResolvedNotActiveClosure" };
                }).ToArray();
                return new
                {
                    source = Identity(source), definingProject = Identity(item.Attribute("DefiningProject")!.Value), excludedFromBuild = Metadata("ExcludedFromBuild"),
                    additionalIncludes = additional, environmentIncludes, externalIncludes, orderedCompileSearchPaths = ordered,
                    defines = Paths(Metadata("PreprocessorDefinitions")), forcedIncludes = Paths(Metadata("ForcedIncludeFiles")),
                    precompiledHeader = Metadata("PrecompiledHeader"), precompiledHeaderFile = Metadata("PrecompiledHeaderFile"),
                    sdkIdentitySamples = headers, directIncludeCandidates = directives
                };
            }).ToArray();
            if (!generator) Assert.IsGreaterThan(0, units.Length, context);
            var execTargets = xml.Element("Targets")!.Elements().Where(t => t.Elements("Task").Any(task => task.Attribute("Name")!.Value == "Exec")).Select(t => new { name = t.Attribute("Name")!.Value, source = Identity(t.Attribute("DefiningProject")!.Value), executed = false }).ToArray();
            if (generator) Assert.IsTrue(execTargets.Any(t => t.name == "RunTool"), context);
            return new
            {
                evaluation = Identity(path), project = Identity(project), configuration, platform, generator, properties, imports, units, execTargets,
                toolchainState = generator ? "NotNativeCompilation" : missingToolchain ? "MissingArm64ToolchainNotHostEquivalent" : "SelectedCompileInputsResolvedNotNativeBuild",
                nativeCompilationExecuted = false
            };
        }).ToArray();
        Assert.HasCount(18, evaluations.Where(e => !e.generator).ToArray());
        Assert.HasCount(6, evaluations.Where(e => e.toolchainState == "MissingArm64ToolchainNotHostEquivalent").ToArray());
        Assert.HasCount(12, evaluations.Where(e => e.toolchainState == "SelectedCompileInputsResolvedNotNativeBuild").ToArray());
        using var groupDocument = JsonDocument.Parse(File.ReadLines(Full("WpfGfxShape/Ledger/groups.jsonl")).Single(l => JsonDocument.Parse(l).RootElement.GetProperty("groupId").GetString() == "GENPROTO-MILCMD"));
        var group = groupDocument.RootElement;
        var physicalIds = new[] { "memberRecordIds", "authorityRecordIds", "consumerRecordIds" }.SelectMany(key => group.GetProperty(key).EnumerateArray().Select(id => id.GetString()!)).Distinct().ToArray();
        Assert.HasCount(6, physicalIds);
        var records = File.ReadLines(Full("WpfGfxShape/Ledger/records.jsonl")).Select(l => JsonDocument.Parse(l).RootElement.Clone()).ToArray();
        var physical = physicalIds.Select(id => new { id, record = records.Single(r => r.GetProperty("recordId").GetString() == id), currentBuildBinding = "Existing frozen record; evaluated project roots or Resource.xml/xsd generator input; historical generation binding not established" }).ToArray();
        var evidenceIds = group.GetProperty("requiredEvidenceIds").EnumerateArray().Select(id => id.GetString()!).ToArray();
        Assert.HasCount(3, evidenceIds);
        string priorPath = Full("WpfGfxShape/Investigations/GenProtoEvidence/milcmd-build-contract-b6261bbffd564a0998b8cc208ac28724/report.json");
        using var prior = JsonDocument.Parse(File.ReadAllText(priorPath));
        var audit = prior.RootElement.GetProperty("audit").EnumerateArray().Select(row => new
        {
            ordinal = row.GetProperty("ordinal").GetInt32(), original = row.Clone(),
            disposition = "PreservedHistoricalEnvelopeNotAutomaticallyFormalRequired",
            cause = "Direct literal candidates and evaluated items do not decide transitive conditional preprocessor applicability"
        }).ToArray();
        Assert.HasCount(721, audit);
        var reconciliation = prior.RootElement.GetProperty("boundedEdges").EnumerateArray()
            .Where(e => e.GetProperty("Classification").GetString() is "Undetermined" or "RealMissing")
            .Select(e =>
            {
                string from = e.GetProperty("From").GetString()!;
                string reference = e.GetProperty("Reference").GetString()!;
                var candidates = directEdges.Where(d => string.Equals(d.From, from, StringComparison.OrdinalIgnoreCase) && d.Reference == reference).Select(d => d.Target).Distinct().ToArray();
                return new { original = e.Clone(), selectedTranslationUnit = selectedSources.Contains(from), directCandidateTargets = candidates,
                    disposition = candidates.Any(t => t != null) ? "CurrentDirectCandidateResolvedConditionStillUnproven" : selectedSources.Contains(from) ? "SelectedTURequiresConditionalOrMacroPreprocessing" : "TransitiveApplicabilityUnprovenNotExcluded",
                    requiredSource = "Captured selected TU macros/forced includes and include stack to this exact original edge; not a new required entry", c5Satisfied = false };
            }).ToArray();
        Assert.HasCount(333, reconciliation);
        var report = new
        {
            scope = "Frozen 6 physical + 3 evidence entries; 18 native + 2 generator current evaluations; bounded direct TU literal candidates only",
            currentEvaluationExecuted = true, c5Satisfied = false, childSatisfied = false, parentBlocked = true, stage4Closed = false,
            testSource = Identity(Full("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/SelectedEvaluationTests.cs")),
            testAssembly = Identity(typeof(SelectedEvaluationTests).Assembly.Location), assemblyMvid = typeof(SelectedEvaluationTests).Assembly.ManifestModule.ModuleVersionId,
            captureSource = Identity(Path.Combine(directory, "Evaluate.proj")),
            preservedLegacyEvaluations = Directory.GetFiles(directory, "*.*.*.evaluated.txt").Order(StringComparer.Ordinal).Select(Identity).ToArray(),
            priorReport = Identity(priorPath), evaluations, physicalRecords = physical, evidenceIds, historicalAudit = audit, reconciliation,
            directCandidateCount = directEdges.Count, distinctDirectCandidateCount = directEdges.Distinct().Count(),
            autonomousRemainingIsZero = false,
            autonomousCauses = new[] { "For selected current TUs, preprocess with captured per-TU defines, forced includes and ordered search paths; reconcile only original unresolved 332+1 envelope by source/line/condition and existing physical owner", "Bind each applicable generated declaration to frozen record/signature/generation/consumer/assertion; do not promote all 721 historical edges or recurse repository-wide" },
            external = new[] { "Original frozen generation CLR/hash/input/build-version authority binding", "Missing ARM64 toolchain and real execution environment (evaluation is not execution)", "Official full 10.0.12 reference identity", "Official player protocol and visible DWM session/control prerequisite" }
        };
        string output = Path.Combine(directory, "report.json");
        File.WriteAllText(output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        using var readBack = JsonDocument.Parse(File.ReadAllText(output));
        Assert.HasCount(20, readBack.RootElement.GetProperty("evaluations").EnumerateArray().ToArray());
        File.WriteAllText(Path.Combine(directory, "summary.json"), JsonSerializer.Serialize(new { report = Identity(output), nativeEvaluations = 18, generatorEvaluations = 2, missingArm64Toolchain = 6, availableCompileInputs = 12, physicalRecords = 6, evidenceEntries = 3, historicalEdges = 721, directCandidates = directEdges.Count, c5Satisfied = false }));
    }
}
