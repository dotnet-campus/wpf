using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class MilcmdBuildContractTests
{
    private const string Native = "src/Microsoft.DotNet.Wpf/src/WpfGfx/";
    private const string Baseline = "WpfGfxShape/Investigations/GenProtoEvidence/milcmd-physical-05f3d6983cde4339b77bcf1641a2dc50/source-manifest.json";
    private static string Root => typeof(MilcmdBuildContractTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
    private static string Full(string p) => Path.GetFullPath(Path.Combine(Root, p.Replace('\\', '/')));
    private static string Relative(string p) => Path.GetRelativePath(Root, p).Replace('\\', '/');
    private static string Hash(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private sealed record Edge(string From, string Reference, string Classification, string[] Authority, string Reason, string? Target = null);
    private sealed record Input(string Path, string Sha256);
    private sealed record Directive(string Reference, int Line, bool? Applicable, string Condition);

    /// <summary>Audits the fixed historical envelope without promoting unrelated files into formal requirements.</summary>
    [TestMethod]
    public void WhenBuildContractsBoundMilcmdThenEveryHistoricalUnresolvedEdgeRetainsAnAuthorityAndDisposition()
    {
        string evidenceRoot = Full("WpfGfxShape/Investigations/GenProtoEvidence");
        string previousPath = Directory.EnumerateDirectories(evidenceRoot, "milcmd-build-contract-*").Select(d => Path.Combine(d, "report.json")).MaxBy(File.GetLastWriteTimeUtc) ?? throw new InvalidOperationException("No preceding build contract report");
        using var previous = JsonDocument.Parse(File.ReadAllText(previousPath));
        Assert.AreEqual(721, previous.RootElement.GetProperty("audit").GetArrayLength());
        var previousCounts = previous.RootElement.GetProperty("audit").EnumerateArray().GroupBy(e => e.GetProperty("edge").GetProperty("Classification").GetString()!).ToDictionary(g => g.Key, g => g.Count());
        foreach (var count in previousCounts) Assert.AreEqual(count.Value, previous.RootElement.GetProperty("classificationCounts").GetProperty(count.Key).GetInt32());
        var scope = new Scope();
        scope.Load();
        using var baseline = JsonDocument.Parse(File.ReadAllText(Full(Baseline)));
        var original = baseline.RootElement.GetProperty("unresolved").EnumerateArray().ToArray();
        Assert.HasCount(721, original);
        var audit = original.Select((e, i) => new { ordinal = i + 1, edge = scope.Classify(e.GetProperty("from").GetString()!, e.GetProperty("reference").GetString()!) }).ToArray();
        string directory = Full("WpfGfxShape/Investigations/GenProtoEvidence/milcmd-build-contract-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var sources = scope.Files.Concat(scope.Contracts).Concat(audit.SelectMany(a => a.edge.Authority)).Append(Baseline)
            .Where(p => File.Exists(Full(p))).Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).Select(p => new Input(p, Hash(Full(p)))).ToArray();
        var wire = WireDenominator();
        string report = Path.Combine(directory, "report.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            previousReport = new Input(Relative(previousPath), Hash(previousPath)), previousReportFullyParsed = true, previousCounts,
            baseline = Baseline, baselineSha256 = Hash(Full(Baseline)), historicalAuditDenominator = 721,
            applicabilityAuthorityMissing = new[] { "Per-project/per-translation-unit evaluated ClCompile items, imports, ordered include paths and defines for all six configurations", "Selected production WindowsTargetPlatformVersion/VCToolsVersion and resolved SDK/VC header hashes; installed header existence is not selection", "Unknown XML conditions, property functions and native preprocessor branches retain Blocked disposition" },
            boundedClassificationCounts = scope.Edges.GroupBy(e => e.Classification).ToDictionary(g => g.Key, g => g.Count()),
            auditScope = audit.GroupBy(a => scope.Files.Contains(a.edge.From) || scope.Contracts.Contains(a.edge.From) ? "ReachedConservativeProductionEnvelope" : "NotReachedNotProvenExcluded").ToDictionary(g => g.Key, g => g.Count()),
            formalEntryCount = 9, scopePolicy = "Only three formal native project roots, their explicit project references/imports/items and ordered include closure. Historical envelope is audit-only; unsupported conditions remain unknown.",
            configurations = new[] { "Debug|Win32", "Release|Win32", "Debug|x64", "Release|x64", "Debug|arm64", "Release|arm64" },
            audit, classificationCounts = audit.GroupBy(a => a.edge.Classification).ToDictionary(g => g.Key, g => g.Count()),
            undecidedCount = audit.Count(a => a.edge.Classification == "Undetermined"),
            productionProjects = scope.Projects.Order(StringComparer.Ordinal), productionFiles = scope.Files.Order(StringComparer.Ordinal),
            boundedEdges = scope.Edges, productionUndeterminedCount = scope.Edges.Count(e => e.Classification == "Undetermined"),
            sources, wire, compilerMeasurementsClaimed = false,
            remaining = new[] { "Unknown MSBuild/property/preprocessor contexts must not be excluded", "Full wire size/offset/pack and enum comparisons on Win32/x64", "ARM64 compiler/execution external prerequisite", "Per-command production assertion binding", "Historical authority binding; full runtime/player protocol and hardware prerequisites unchanged" },
            parentRequiredSatisfied = false, stage4Closed = false,
            assembly = new Input(Relative(typeof(MilcmdBuildContractTests).Assembly.Location), Hash(typeof(MilcmdBuildContractTests).Assembly.Location)),
            testSources = Directory.EnumerateFiles(Full("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests"), "*.cs").Order(StringComparer.Ordinal).Select(p => new Input(Relative(p), Hash(p))).ToArray()
        }, Options));
        using var readback = JsonDocument.Parse(File.ReadAllText(report));
        Assert.AreEqual(721, readback.RootElement.GetProperty("audit").GetArrayLength());
        Assert.IsTrue(sources.All(s => Hash(Full(s.Path)) == s.Sha256));
        Console.WriteLine($"MILCMD build contract report: {Relative(report)} SHA256={Hash(report)}");
        Console.WriteLine(JsonSerializer.Serialize(readback.RootElement.GetProperty("classificationCounts")));
    }

    private static object WireDenominator()
    {
        string managedPath = Native + "include/Generated/wgx_commands.cs";
        string nativePath = Native + "include/Generated/wgx_commands.h";
        string dispatchPath = Native + "core/uce/generated_process_message.inl";
        string xmlPath = Native + "codegen/mcg/xml/Resource.xml";
        string generatorPath = Native + "codegen/mcg/generators/CommandStructure.cs";
        string paddedPath = Native + "codegen/mcg/helpers/PaddedCommand.cs";
        var model = XDocument.Load(Full(xmlPath), LoadOptions.SetLineInfo);
        var nativeOnlyCommands = new Dictionary<string, (string Target, string Name)>(StringComparer.Ordinal)
        {
            ["MILCMD_BITMAP_SOURCE"] = ("Bitmap", "Source"),
            ["MILCMD_CHANNEL_CREATERESOURCE"] = ("Channel", "CreateResource"),
            ["MILCMD_CHANNEL_DELETERESOURCE"] = ("Channel", "DeleteResource"),
            ["MILCMD_CHANNEL_DUPLICATEHANDLE"] = ("Channel", "DuplicateHandle"),
            ["MILCMD_TRANSPORT_DESTROYRESOURCESONCHANNEL"] = ("Transport", "DestroyResourcesOnChannel"),
            ["MILCMD_TRANSPORT_SYNCFLUSH"] = ("Transport", "SyncFlush"),
            ["MILCMD_VISUAL_CREATE"] = ("Visual", "Create")
        };
        var nativeOnly = nativeOnlyCommands.Select(pair =>
        {
            var command = model.Descendants("Command").Single(e => ((string?)e.Attribute("Target") ?? (string?)e.Attribute("Domain")) == pair.Value.Target && (string?)e.Attribute("Name") == pair.Value.Name);
            Assert.AreEqual("true", (string?)command.Attribute("UnmanagedOnly"));
            return new { name = pair.Key, authority = xmlPath, line = ((System.Xml.IXmlLineInfo)command).LineNumber, xml = command.ToString(), disposition = "ManagedDeclarationIntentionallyNotGenerated; actual native dispatch remains required; no layout or execution equivalence claimed" };
        }).ToArray();
        Assert.IsTrue(File.ReadAllText(Full(generatorPath)).Contains("if (!command.CommandIsUnmanagedOnly)", StringComparison.Ordinal));
        Assert.IsTrue(File.ReadAllText(Full(paddedPath)).Contains("m_commandIsUnmanagedOnly = command.UnmanagedOnly;", StringComparison.Ordinal));
        var managed = CSharpSyntaxTree.ParseText(File.ReadAllText(Full(managedPath))).GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>()
            .Where(s => s.Identifier.ValueText.StartsWith("MILCMD_", StringComparison.Ordinal)).ToArray();
        string native = File.ReadAllText(Full(nativePath));
        string[] nativeNames = Regex.Matches(native, @"\bMILCMD_[A-Z0-9_]+\b").Select(m => m.Value).Distinct().Order(StringComparer.Ordinal).ToArray();
        string[] dispatch = Regex.Matches(File.ReadAllText(Full(dispatchPath)), @"\bMILCMD_[A-Z0-9_]+\b").Select(m => m.Value).Distinct().Order(StringComparer.Ordinal).ToArray();
        var rows = nativeNames.Union(managed.Select(s => s.Identifier.ValueText)).Union(dispatch).Order(StringComparer.Ordinal).Select(name =>
        {
            var declaration = managed.SingleOrDefault(s => s.Identifier.ValueText == name);
            return new
            {
                name, nativeDeclarationReference = nativeNames.Contains(name) ? nativePath : null,
                managedDeclarationReference = declaration is null ? null : managedPath, dispatchConsumer = dispatch.Contains(name) ? dispatchPath : null,
                fields = declaration?.Members.OfType<FieldDeclarationSyntax>().SelectMany(f => f.Declaration.Variables.Select(v => new { name = v.Identifier.ValueText, type = f.Declaration.Type.ToString(), sourceAttributes = f.AttributeLists.ToString(), measuredOffset = (int?)null })).ToArray(),
                sourceLayout = declaration?.AttributeLists.ToString(),
                obligations = new[] { "Win32: native sizeof/alignof/offsetof, managed compiled size/offset/pack, enum widths/values", "x64: native sizeof/alignof/offsetof, managed compiled size/offset/pack, enum widths/values", "ARM64: external compiler and execution required" },
                status = "RequiredNotMeasuredByThisTest"
            };
        }).ToArray();
        string[] missingManaged = nativeNames.Except(managed.Select(s => s.Identifier.ValueText)).Order(StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEquivalent(nativeOnlyCommands.Keys.Append("MILCMD_MEDIAPLAYER").ToArray(), missingManaged);
        return new { sources = new[] { managedPath, nativePath, dispatchPath, xmlPath, generatorPath, paddedPath }.Select(p => new Input(p, Hash(Full(p)))), nativeCount = nativeNames.Length, managedCount = managed.Length, dispatchCount = dispatch.Length, unionCount = rows.Length, missingManaged, nativeOnly,
            mediaPlayer = new { name = "MILCMD_MEDIAPLAYER", status = "Blocked", reason = "Resource-origin update needs ShouldGenerate(ManagedClass, MediaPlayer) section selection and exact CommandTypeName binding; explicit MediaPlayer Update UnmanagedOnly alone is not proof of this resource packet mapping", authority = new[] { xmlPath, paddedPath, generatorPath } },
            nextIndependentFormalGroup = "wirecompilermeasurement", rows, note = "Declaration inventory only, never compiler measurement. Native-only does not mean unconsumed: each has a native dispatch reference. Existing finite layouts are not extrapolated." };
    }

    private sealed class Scope
    {
        internal HashSet<string> Projects { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal HashSet<string> Contracts { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal HashSet<string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
        internal List<Edge> Edges { get; } = [];
        private readonly List<string> include = [];
        private readonly Dictionary<string, string> properties = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> loaded = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> FileItems = new(StringComparer.Ordinal) { "ClCompile", "ClInclude", "ResourceCompile", "Compile", "ProjectReference", "Import", "ProcessedFiles" };

        internal void Load()
        {
            properties["RepoRoot"] = Root.TrimEnd('\\', '/') + "/";
            ReadContract("eng/WpfArcadeSdk/tools/FolderPaths.props");
            properties["WpfSharedDir"] = "";
            properties["WpfCommonDir"] = "";
            properties["RepositoryName"] = "dotnet-wpf";
            ReadContract("eng/WpfArcadeSdk/tools/FolderPaths.targets");
            ReadContract(Native + "Graphics.Paths.props");
            ReadContract(Native + "Graphics.props");
            foreach (string p in new[] { "core/dll/wpfgfx.vcxproj", "core/resources/resources.vcxproj", "core/uce/uce.vcxproj", "codegen/mcg/mcg.proj" }) ReadContract(Native + p);
            var queue = new Queue<string>(Files);
            while (queue.TryDequeue(out string? file))
            {
                if (!new[] { ".h", ".hpp", ".inl", ".c", ".cpp" }.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase)) continue;
                foreach (var directive in Includes(file))
                {
                    Edge edge = ResolveInclude(file, directive);
                    Edges.Add(edge);
                    if (edge.Target is not null && edge.Classification is "ProductionResolvable" or "Undetermined" && Files.Add(edge.Target)) queue.Enqueue(edge.Target);
                }
            }
        }

        private string Expand(string value, string from)
        {
            value = value.Replace('\\', '/').Trim();
            for (int i = 0; i < 8; i++)
            {
                string next = Regex.Replace(value, @"\$\((\w+)\)", m => m.Groups[1].Value.Equals("MSBuildThisFileDirectory", StringComparison.OrdinalIgnoreCase) ? Path.GetDirectoryName(Full(from))!.Replace('\\', '/') + "/" : properties.GetValueOrDefault(m.Groups[1].Value) ?? m.Value);
                if (next == value) break;
                value = next;
            }
            return value;
        }

        private bool? Condition(string value, string from)
        {
            value = Expand(value.Trim(), from);
            if (value.Length == 0) return true;
            if (value.Contains("$(", StringComparison.Ordinal) && !value.Contains(" And ", StringComparison.OrdinalIgnoreCase)) return null;
            string[] conjunction = Regex.Split(value, @"\s+And\s+", RegexOptions.IgnoreCase);
            if (conjunction.Length > 1)
            {
                var terms = conjunction.Select(v => Condition(v, from)).ToArray();
                return terms.Contains(false) ? false : terms.Contains(null) ? null : true;
            }
            var exists = Regex.Match(value, @"^(!?)Exists\('([^']+)'\)$", RegexOptions.IgnoreCase);
            if (exists.Success && !exists.Groups[2].Value.Contains("$(", StringComparison.Ordinal))
            {
                string candidate = Path.Combine(Path.GetDirectoryName(Full(from))!, exists.Groups[2].Value);
                bool found = File.Exists(candidate) || Directory.Exists(candidate);
                return exists.Groups[1].Length == 0 ? found : !found;
            }
            var equality = Regex.Match(value, "^'([^']*)'\\s*(==|!=)\\s*'([^']*)'$");
            if (equality.Success && !value.Contains("$(", StringComparison.Ordinal))
            {
                bool equal = equality.Groups[1].Value.Equals(equality.Groups[3].Value, StringComparison.OrdinalIgnoreCase);
                return equality.Groups[2].Value == "==" ? equal : !equal;
            }
            return null;
        }

        private void ReadContract(string path)
        {
            path = Relative(Full(path));
            if (!File.Exists(Full(path)) || !loaded.Add(path)) return;
            Contracts.Add(path);
            if (Path.GetExtension(path) is ".vcxproj" or ".proj" or ".csproj") Projects.Add(path);
            var doc = XDocument.Load(Full(path), LoadOptions.SetLineInfo);
            foreach (var element in doc.Descendants())
            {
                var conditions = element.AncestorsAndSelf().Attributes("Condition").Select(a => Condition(a.Value, path)).ToArray();
                bool? active = conditions.Contains(false) ? false : conditions.Contains(null) ? null : true;
                if (element.Parent?.Name.LocalName == "PropertyGroup" && active == true) properties[element.Name.LocalName] = Expand(element.Value, path);
                if (element.Name.LocalName == "AdditionalIncludeDirectories" && active == true)
                    include.AddRange(Expand(element.Value, path).Split(';').Select(v => v.Trim()).Where(v => v.Length > 0 && !v.Contains("$(", StringComparison.Ordinal) && !v.Contains("%(", StringComparison.Ordinal)).Select(v => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Full(path))!, v))));
                if (!FileItems.Contains(element.Name.LocalName)) continue;
                string? reference = (string?)element.Attribute(element.Name.LocalName == "Import" ? "Project" : "Include");
                if (reference is null) continue;
                string expanded = Expand(reference, path);
                string reason = string.Join(" && ", element.AncestorsAndSelf().Attributes("Condition").Select(a => a.Value));
                if (active == false) { Edges.Add(new(path, reference, "ConditionalNotApplicable", [path], reason)); continue; }
                if (expanded.Contains("$(", StringComparison.Ordinal) || expanded.IndexOfAny(['*', '?', ';']) >= 0)
                { Edges.Add(new(path, reference, "Undetermined", [path], "Unexpanded property/item/wildcard contract: " + reason)); continue; }
                string candidate = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Full(path))!, expanded));
                if (!candidate.StartsWith(Root, StringComparison.OrdinalIgnoreCase)) { Edges.Add(new(path, reference, "Undetermined", [path], "External path needs actual SDK authority")); continue; }
                string target = Relative(candidate);
                Edges.Add(new(path, reference, active is null ? "Undetermined" : File.Exists(candidate) ? "ProductionResolvable" : "RealMissing", [path], active is null ? "Condition not evaluated: " + reason : "Explicit file item/import/project reference", target));
                if (!File.Exists(candidate)) continue;
                if (element.Name.LocalName is "ProjectReference" or "Import") ReadContract(target);
                else Files.Add(target);
            }
        }

        internal Edge Classify(string from, string reference)
        {
            if (new[] { ".props", ".proj", ".vcxproj", ".csproj", ".xml", ".xsd" }.Contains(Path.GetExtension(from), StringComparer.OrdinalIgnoreCase))
            {
                var doc = XDocument.Load(Full(from));
                var elements = doc.Descendants().Where(e => e.Attributes().Any(a => a.Value.Trim().Replace('\\', '/') == reference)).ToArray();
                if (elements.Length > 0 && elements.All(e => e.Name.LocalName is "ProjectConfiguration" or "Project" or "Output"))
                    return new(from, reference, "MisidentifiedNonCodeText", [from], "XML element is configuration identity, project GUID metadata or task output, not a file input");
                if (elements.Length > 0 && elements.All(e => e.AncestorsAndSelf().Attributes("Condition").Any(a => Condition(a.Value, from) == false)))
                    return new(from, reference, "ConditionalNotApplicable", [from, Native + "Graphics.props"], "Every matching item is disabled by its evaluated condition");
                string expanded = Expand(reference, from);
                if (!expanded.Contains("$(", StringComparison.Ordinal) && expanded.IndexOfAny(['*', '?', ';']) < 0)
                {
                    string candidate = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Full(from))!, expanded));
                    if (File.Exists(candidate)) return new(from, reference, "ProductionResolvable", [from, "eng/WpfArcadeSdk/tools/FolderPaths.props", Native + "Graphics.Paths.props"], "Exact expanded contract path; applicability is not inferred from existence", Relative(candidate));
                }
                return new(from, reference, "Undetermined", [from], "Needs unevaluated MSBuild property/function/item or schema context; missing file is not exclusion evidence");
            }
            var matches = Includes(from).Where(d => d.Reference == reference).ToArray();
            if (matches.Length == 0) return new(from, reference, "Undetermined", [from], "No lexical native include found; generator response/schema/non-native text context still needs explicit authority review");
            var resolved = matches.Select(d => ResolveInclude(from, d)).ToArray();
            return resolved.FirstOrDefault(e => e.Classification == "ProductionResolvable") ?? resolved.FirstOrDefault(e => e.Classification == "Undetermined") ?? resolved[0];
        }

        private Edge ResolveInclude(string from, Directive directive)
        {
            string[] authority = [from, Native + "Graphics.props", Native + "Graphics.Paths.props", "eng/WpfArcadeSdk/tools/FolderPaths.props"];
            if (directive.Applicable == false) return new(from, directive.Reference, "ConditionalNotApplicable", authority, directive.Condition);
            string[] candidates = new[] { Path.GetDirectoryName(Full(from))! }.Concat(include).Select(d => Path.GetFullPath(Path.Combine(d, directive.Reference.Replace('\\', '/')))).ToArray();
            string? found = candidates.FirstOrDefault(File.Exists);
            if (found is not null) return new(from, directive.Reference, directive.Applicable is null ? "Undetermined" : "ProductionResolvable", authority, "Ordered local/build include path; condition=" + directive.Condition, Relative(found));
            string kits = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Windows Kits/10/Include");
            if (Directory.Exists(kits))
            {
                foreach (string version in Directory.EnumerateDirectories(kits).OrderDescending(StringComparer.Ordinal))
                foreach (string component in new[] { "um", "shared", "ucrt", "winrt" })
                {
                    string header = Path.Combine(version, component, directive.Reference.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(header)) return new(from, directive.Reference, "SystemSdkExternal", [from, Relative(header), "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/NativeLayout.x64.vcxproj"], "Exact installed Windows SDK header; diagnostic project requests WindowsTargetPlatformVersion=10.0. Not proof of production architecture selection or execution.", Relative(header));
                }
            }
            string name = Path.GetFileName(directive.Reference);
            if (name is "wgx_core_types.h" or "wgx_av_types.h") return new(from, directive.Reference, "BuildGenerated", authority, "Graphics.props CopyGeneratedIncludeFilesToIntermediateFolder copies processed input into IntermediateOutputPath", Native + "include/processed/" + name);
            return new(from, directive.Reference, "Undetermined", authority, "No exact include-path match; SDK/imported include dirs or preprocessor condition still unproven: " + directive.Condition);
        }

        private static IEnumerable<Directive> Includes(string from)
        {
            string text = Regex.Replace(File.ReadAllText(Full(from)), @"/\*[\s\S]*?\*/", m => new string('\n', m.Value.Count(c => c == '\n')));
            text = Regex.Replace(text, @"//[^\r\n]*", "");
            var stack = new Stack<(bool? Active, string Text)>();
            int line = 0;
            foreach (string sourceLine in text.Split('\n'))
            {
                line++;
                var pre = Regex.Match(sourceLine, @"^\s*#\s*(if|ifdef|ifndef|elif|else|endif|include)\b(.*)$");
                if (!pre.Success) continue;
                string op = pre.Groups[1].Value, value = pre.Groups[2].Value.Trim();
                if (op is "if" or "ifdef" or "ifndef") stack.Push((op == "if" && value == "0" ? false : op == "if" && value == "1" ? true : null, op + " " + value));
                else if (op == "endif" && stack.Count > 0) stack.Pop();
                else if (op is "else" or "elif" && stack.Count > 0)
                {
                    var previous = stack.Pop();
                    stack.Push((op == "else" && previous.Active.HasValue ? !previous.Active.Value : null, previous.Text + "; " + op + " " + value));
                }
                else if (op == "include")
                {
                    var literal = Regex.Match(value, "^[<\"]([^>\"]+)[>\"]");
                    if (literal.Success) yield return new(literal.Groups[1].Value.Replace('\\', '/'), line, stack.Any(s => s.Active == false) ? false : stack.Any(s => s.Active is null) ? null : true, string.Join(" && ", stack.Reverse().Select(s => s.Text)));
                }
            }
        }
    }
}
