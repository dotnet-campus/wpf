using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

string input = Path.Combine(AppContext.BaseDirectory, "Inputs");
string output = Path.Combine(AppContext.BaseDirectory, "Results");
Directory.CreateDirectory(output);
var options = new JsonSerializerOptions { WriteIndented = true };
var records = File.ReadLines(Path.Combine(input, "Ledger", "records.jsonl")).Select(l => JsonDocument.Parse(l).RootElement.Clone()).ToArray();
var group = File.ReadLines(Path.Combine(input, "Ledger", "groups.jsonl")).Select(l => JsonDocument.Parse(l).RootElement.Clone()).Single(g => g.GetProperty("groupId").GetString() == "GENPROTO-MILCMD");
string[] roots = new[] { "memberRecordIds", "authorityRecordIds", "consumerRecordIds" }.SelectMany(k => group.GetProperty(k).EnumerateArray().Select(x => x.GetString()!)).Distinct().ToArray();
string Normalize(string value) => value.Replace('\\', '/').TrimEnd('/');
string Relative(string value)
{
    string normalized = Normalize(value);
    const string marker = "/WpfLab/WpfGfxShape/";
    int index = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
    return index < 0 ? normalized : normalized[(index + marker.Length)..];
}
string Text(JsonElement e, string key) => e.TryGetProperty(key, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString()! : "";
string[] Records(string path) => records.Where(r => string.Equals(Relative(Text(r, "nativePath")), Relative(path), StringComparison.OrdinalIgnoreCase)).Select(r => Text(r, "recordId")).ToArray();
string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
var rows = new List<AuditRow>();
void Add(string kind, string config, string source, int line, string target, string owner, string signature, string generation, string assertion, string state, string reason, string evidence)
{
    rows.Add(new AuditRow(rows.Count + 1, kind, config, Relative(source), line, Relative(target), owner, Records(target), signature, generation, assertion, state, reason, evidence));
}
var identities = new List<object>();
var evaluations = Directory.GetFiles(Path.Combine(input, "Evaluation"), "*.xml").Order(StringComparer.Ordinal).ToArray();
foreach (string file in evaluations)
{
    var xml = XDocument.Load(file, LoadOptions.SetLineInfo);
    var e = xml.Root ?? throw new InvalidDataException(file);
    string project = e.Attribute("Project")!.Value;
    string config = Path.GetFileNameWithoutExtension(file).Replace(".evaluation", "");
    string owner = Records(project).Single();
    string evidence = "WpfGfxShape/Investigations/GenProtoEvidence/c5-selected-evaluation/" + Path.GetFileName(file);
    identities.Add(new { path = evidence, sha256 = Hash(file) });
    bool arm = config.Contains(".arm64", StringComparison.Ordinal);
    foreach (var property in e.Element("Properties")!.Elements())
        Add("SelectedProperty", config, project, ((IXmlLineInfo)property).LineNumber, project, owner, property.Attribute("Name")!.Value + "=" + property.Value, "Current evaluation; not historical binding", "C2/C5", arm ? "MissingExecution" : "ProvenSelectedValue", arm ? "ARM64 actual compile toolchain/execution absent; value retained without host extrapolation" : "Actual evaluated value; XML location is evidence location, not property definition origin", evidence);
    foreach (var import in e.Element("Imports")!.Elements())
        Add("ActiveImport", config, import.Attribute("ImportingProject")!.Value, (int)import.Attribute("Line")!, import.Attribute("Path")!.Value, owner, "Evaluated import identity", "Current selected build", "C2/C5", "ProvenEvaluatedImport", "Actual MSBuild import; root owner is build consumer, not a fabricated separate physical record or runtime assertion", evidence);
    foreach (var item in e.Element("Items")!.Elements())
    {
        string type = item.Attribute("Type")!.Value;
        string target = item.Attribute("FullPath")?.Value ?? item.Attribute("Include")?.Value ?? "";
        string source = item.Attribute("DefiningProject")?.Value ?? project;
        string metadata = string.Join(";", item.Elements().Select(m => m.Attribute("Name")?.Value + "=" + m.Value));
        bool missing = Normalize(target).EndsWith("/bilinearspan/bilinearspan.vcxproj", StringComparison.OrdinalIgnoreCase);
        Add("EvaluatedItem:" + type, config, source, ((IXmlLineInfo)item).LineNumber, target, owner, metadata, "Current evaluated item; XML line is evidence line, not declaration line", "C2/C4/C5", missing ? "MissingOriginalProject" : "ProvenEvaluatedItem", missing ? "Four Win32/x64 GetTargetPath MSB3202; ARM64 reference also retained. Need original project/dependencies or authoritative generator contract" : "Item selection and metadata retained, not proof of object compilation or per-command execution", evidence);
    }
}
var declarations = new Dictionary<string, SourceDeclaration[]>(StringComparer.OrdinalIgnoreCase);
SourceDeclaration[] SourceDeclarations(string source)
{
    if (declarations.TryGetValue(source, out var cached)) return cached;
    string path = Path.Combine(input, "Source", source.Replace('/', Path.DirectorySeparatorChar));
    if (!File.Exists(path)) return declarations[source] = [];
    if (source.EndsWith(".props", StringComparison.OrdinalIgnoreCase) || source.EndsWith(".targets", StringComparison.OrdinalIgnoreCase) || source.EndsWith(".proj", StringComparison.OrdinalIgnoreCase) || source.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase))
    {
        var doc = XDocument.Load(path, LoadOptions.SetLineInfo);
        return declarations[source] = doc.Descendants().SelectMany(e => e.Attributes().Where(a => a.Name.LocalName is "Include" or "Project" or "Update" or "Remove").Select(a => new SourceDeclaration(((IXmlLineInfo)e).LineNumber, a.Value, string.Join(" && ", e.AncestorsAndSelf().Reverse().Attributes("Condition").Select(c => c.Value)), e.ToString(SaveOptions.DisableFormatting), "BuildDeclaration"))).ToArray();
    }
    var result = new List<SourceDeclaration>();
    var conditions = new List<string>();
    bool comment = false;
    int number = 0;
    foreach (string raw in File.ReadLines(path))
    {
        number++;
        string text = AuditPatterns.InlineComment().Replace(raw, "");
        if (comment)
        {
            int end = text.IndexOf("*/", StringComparison.Ordinal);
            if (end < 0) continue;
            text = text[(end + 2)..]; comment = false;
        }
        int start = text.IndexOf("/*", StringComparison.Ordinal);
        if (start >= 0) { text = text[..start]; comment = true; }
        var directive = AuditPatterns.Directive().Match(text);
        if (!directive.Success) continue;
        string op = directive.Groups["op"].Value;
        string rest = directive.Groups["rest"].Value.Trim();
        if (op is "if" or "ifdef" or "ifndef") conditions.Add(op + " " + rest);
        else if (op is "else" or "elif") { if (conditions.Count > 0) conditions[^1] = op + " " + rest + " (prior: " + conditions[^1] + ")"; }
        else if (op == "endif") { if (conditions.Count > 0) conditions.RemoveAt(conditions.Count - 1); }
        else
        {
            var operand = AuditPatterns.Operand().Match(rest);
            if (operand.Success) result.Add(new SourceDeclaration(number, operand.Groups["value"].Value, string.Join(" && ", conditions), raw, "IncludeDirective"));
        }
    }
    return declarations[source] = result.ToArray();
}
var directEdges = new List<DirectEdge>();
var activeByConfig = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
var lineDirective = AuditPatterns.LineDirective();
foreach (string dir in Directory.GetDirectories(Path.Combine(input, "CL")).Order(StringComparer.Ordinal))
{
    string config = Path.GetFileName(dir);
    string projectName = config.Split('.')[0];
    string owner = roots.Single(r => r.EndsWith("-" + projectName.ToUpperInvariant() + "-VCXPROJ", StringComparison.Ordinal));
    var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    activeByConfig.Add(config, paths);
    foreach (string file in Directory.GetFiles(dir).Order(StringComparer.Ordinal))
    {
        string evidence = "artifacts/log/GenProto-C5/" + config + "/" + Path.GetFileName(file);
        identities.Add(new { path = evidence, sha256 = Hash(file) });
        if (!file.EndsWith(".i", StringComparison.OrdinalIgnoreCase)) continue;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int physicalLine = 0;
        string previousPath = "";
        var stack = new Stack<(string Parent, string Child, int EvidenceLine)>();
        foreach (string line in File.ReadLines(file))
        {
            physicalLine++;
            var match = lineDirective.Match(line);
            if (!match.Success) continue;
            string path = Relative(match.Groups["path"].Value);
            int logical = int.Parse(match.Groups["line"].Value, System.Globalization.CultureInfo.InvariantCulture);
            if (stack.TryPeek(out var frame) && string.Equals(path, frame.Parent, StringComparison.OrdinalIgnoreCase))
            {
                stack.Pop();
                // Only accept a child followed by an exact caller resumption at include-line + 1.
                var source = SourceDeclarations(path);
                foreach (var declaration in source.Where(d => d.Kind == "IncludeDirective" && d.Line + 1 == logical && Normalize(frame.Child).EndsWith("/" + Normalize(d.Reference), StringComparison.OrdinalIgnoreCase)))
                    directEdges.Add(new DirectEdge(config, path, declaration.Line, frame.Child, evidence + ":" + frame.EvidenceLine + "-" + physicalLine));
            }
            else if (previousPath.Length > 0 && !string.Equals(path, previousPath, StringComparison.OrdinalIgnoreCase) && logical == 1)
                stack.Push((previousPath, path, physicalLine));
            previousPath = path;
            paths.Add(path);
            if (!seen.Add(path)) continue;
            string name = Path.GetFileName(path);
            bool generated = name is "wgx_core_types.h" or "wgx_render_types_generated.h" or "generated_resource_factory.h" or "resources_generated.h" or "data_generated.h";
            Add("ObservedActivePhysicalInput", config, evidence, physicalLine, path, owner, "#line " + match.Groups["line"].Value + "; exact spelling preserved", generated ? (name == "wgx_core_types.h" ? "Actual obj input; historical generation binding unproven D4" : "Existing R generated-output mapping; no regeneration this audit") : "Not assigned generated authority by basename", generated ? "C3/C4/C5; P signatures/R mapping/F 97 IDs (existing execution only)" : "C5 physical input observation only; no new execution assertion", "ProvenActiveInputNotParentEdge", "CL #line proves selected input, not caller or full include condition stack; owner is consuming root, exact Ledger IDs shown separately", evidence);
        }
    }
}
using var prior = JsonDocument.Parse(File.ReadAllText(Path.Combine(input, "evaluation-report.json")));
var reconciliation = prior.RootElement.GetProperty("reconciliation");
foreach (var r in reconciliation.EnumerateArray())
{
    var original = r.GetProperty("original");
    string source = Text(original, "From");
    string reference = Text(original, "Reference");
    string target = Text(original, "Target");
    string detail = original.GetRawText();
    var contexts = activeByConfig.Where(c => c.Value.Contains(Relative(source))).Select(c => c.Key).ToArray();
    var selected = rows.Where(row => row.Kind.StartsWith("EvaluatedItem:", StringComparison.Ordinal) && string.Equals(row.Target, Relative(target), StringComparison.OrdinalIgnoreCase)).ToArray();
    bool build = source.EndsWith(".props", StringComparison.OrdinalIgnoreCase) || source.EndsWith(".targets", StringComparison.OrdinalIgnoreCase) || source.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase) || source.EndsWith(".proj", StringComparison.OrdinalIgnoreCase);
    bool missing = detail.Contains("bilinearspan.vcxproj", StringComparison.OrdinalIgnoreCase);
    string state = missing ? "MissingOriginalProject" : build && selected.Length > 0 ? "MatchedEvaluatedItemNotOriginalConditionProof" : build ? "UnprovenBuildPropertyOrItemCondition" : "UnprovenExactDirectiveApplicability";
    string reason = missing ? "Known explicit missing project; source and configuration scope retained in original row" : build ? "Build declaration, not C++ include. Need original source line and property expansion/condition at that declaration; matching selected items retained as evidence, not condition proof" : contexts.Length == 0 ? "Source absent from selected 16 TU #line observations; not Excluded. Need this source's actual selected TU/conditional trace, or authoritative exclusion" : "Source observed, but #line does not establish this directive branch/parent edge. Need source line plus evaluated macro branch or full include trace; not infer active by basename";
    var sourceDeclarations = SourceDeclarations(source);
    var matches = sourceDeclarations.Where(d => string.Equals(Normalize(d.Reference), Normalize(reference), StringComparison.OrdinalIgnoreCase) || d.Reference.Split(';').Any(p => string.Equals(Normalize(p.Trim()), Normalize(reference), StringComparison.OrdinalIgnoreCase))).ToArray();
    var proof = new List<string>();
    int sourceLine = matches.Length == 1 ? matches[0].Line : 0;
    string condition = string.Join(" || ", matches.Select(d => d.Condition));
    if (build && matches.Length == 1)
    {
        foreach (string evaluation in evaluations)
        {
            var root = XDocument.Load(evaluation).Root!;
            foreach (var import in root.Element("Imports")!.Elements().Where(i => string.Equals(Relative(i.Attribute("ImportingProject")!.Value), Relative(source), StringComparison.OrdinalIgnoreCase) && (int)i.Attribute("Line")! == sourceLine))
                proof.Add(Path.GetFileName(evaluation) + " exact Import " + import.Attribute("Path")!.Value);
            foreach (var item in root.Element("Items")!.Elements().Where(i => string.Equals(Relative(i.Attribute("DefiningProject")?.Value ?? ""), Relative(source), StringComparison.OrdinalIgnoreCase) && string.Equals(i.Attribute("UnevaluatedInclude")?.Value, reference, StringComparison.OrdinalIgnoreCase)))
                proof.Add(Path.GetFileName(evaluation) + " exact defining project/unevaluated include " + item.ToString(SaveOptions.DisableFormatting));
        }
        if (proof.Count > 0) { state = "ProvenApplicableDeclaration"; reason = "Exact source declaration selected by structured MSBuild evaluation; import path is actual expansion. ARM64 evaluation is not execution."; }
    }
    if (!build && matches.Length == 1)
    {
        proof.AddRange(directEdges.Where(e => string.Equals(e.Source, Relative(source), StringComparison.OrdinalIgnoreCase) && e.Line == sourceLine).Select(e => e.Configuration + " " + e.Evidence + " child=" + e.Target));
        if (proof.Count > 0) { state = "ProvenApplicableDirective"; reason = "Original include line and exact caller resumption after child expansion; not inferred from file membership."; }
    }
    string sourceFile = Path.Combine(input, "Source", source.Replace('/', Path.DirectorySeparatorChar));
    if (File.Exists(sourceFile) && matches.Length == 0)
    {
        state = "UnresolvedSourceDeclaration";
        reason = "Exact source read but no unique supported literal/XML declaration matched. Requires original audit location or macro/non-code reconciliation; not automatically NA.";
    }
    else if (!File.Exists(sourceFile)) { state = "MissingExactSource"; reason = "Exact original source unavailable: " + source + "; no basename substitution permitted."; }
    else if (proof.Count == 0 && !missing)
        reason = "Declaration resolved; condition=" + condition + "; " + (build ? "Retained evaluation does not export the required declaration-time property/item identity; need structured evaluation of this exact declaration with expanded operands and ancestor conditions." : "Need original CL macro/forced-include state and exact caller include trace for this source/line; presence or absence in selected TUs is not exclusion.");
    Add("LegacyCandidateReconciliationNotRequiredDenominator", string.Join(";", contexts.Concat(selected.Select(s => s.Configuration)).Distinct()), source, sourceLine, target.Length == 0 ? reference : target, string.Join(";", selected.Select(s => s.ActiveOwner).Distinct()), detail + "\nSOURCE=" + string.Join("\n", matches.Select(d => d.Text)) + "\nCONDITION=" + condition, "Original row retained verbatim in signature", "C5 applicability audit, not new required", state, reason, "c5-selected-evaluation/report.json#/reconciliation; " + string.Join("; ", proof));
}
var legacy = rows.Where(r => r.Kind == "LegacyCandidateReconciliationNotRequiredDenominator").ToArray();
var resolution = legacy.GroupBy(r => r.Source.EndsWith(".props", StringComparison.OrdinalIgnoreCase) || r.Source.EndsWith(".targets", StringComparison.OrdinalIgnoreCase) || r.Source.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase) || r.Source.EndsWith(".proj", StringComparison.OrdinalIgnoreCase) ? "Build" : "Include").ToDictionary(g => g.Key, g => new { raw = g.Count(), sourceResolved = g.Count(r => r.Line > 0), applicableProven = g.Count(r => r.State.StartsWith("ProvenApplicable", StringComparison.Ordinal)), notApplicable = g.Count(r => r.State.StartsWith("NotApplicable", StringComparison.Ordinal)), missing = g.Count(r => r.State.StartsWith("Missing", StringComparison.Ordinal)), remaining = g.Count(r => !r.State.StartsWith("ProvenApplicable", StringComparison.Ordinal) && !r.State.StartsWith("NotApplicable", StringComparison.Ordinal) && !r.State.StartsWith("Missing", StringComparison.Ordinal)) });
var runIdentity = new { executableSha256 = Hash(typeof(AuditRow).Assembly.Location), runUtc = DateTimeOffset.UtcNow, actualPlatform = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(), arm64Execution = false };
foreach (string source in declarations.Keys)
{
    string path = Path.Combine(input, "Source", source.Replace('/', Path.DirectorySeparatorChar));
    if (File.Exists(path)) identities.Add(new { path = source, sha256 = Hash(path) });
}
var counts = rows.GroupBy(r => r.Kind).ToDictionary(g => g.Key, g => g.Count());
var states = rows.GroupBy(r => r.State).ToDictionary(g => g.Key, g => g.Count());
var report = new
{
    schemaVersion = 1,
    authority = "Single adjudication table; observed inputs and legacy candidates are distinct domains, never added to formal denominator",
    formalPhysicalIds = roots,
    formalEvidenceIds = group.GetProperty("requiredEvidenceIds").Clone(),
    formalEntryDenominator = 9,
    nativeConfigurationDenominator = 18,
    generatorConfigurationDenominator = 2,
    selectedCLConfigurationDenominator = 12,
    selectedCLTranslationUnitDenominator = 16,
    exactAuditRowDenominator = rows.Count,
    counts, states, resolution, runIdentity,
    autonomousReadOnlyAuditExecuted = true,
    allApplicableEdgesProven = false,
    autonomousFormalClosureFinished = false,
    c5Satisfied = false, parentBlocked = true, stage4Closed = false,
    scopeAuthority = "Ledger/groups.jsonl GENPROTO-MILCMD readinessRule/doneRule/requiredEvidenceIds; Ledger/evidence.jsonl EVID-GENPROTO-MILCMD-CLOSURE and EVID-GENPROTO-MILCMD-REGEN-DIFF; original requirements reproduced in Docs/investigations/superseded/stage4-ledger-and-last-contract-closure-plan.md:68-82",
    legacyCandidateExhaustionIsRequiredGate = false,
    legacyCandidatesRetainedAsRiskRegister = true,
    reason = "Formal closure requires traceable actual generation inputs/components/frozen outputs/consumers and isolated differential evidence, not exhaustion of conservative scanner candidates. All 333 risk rows and their unresolved states are retained; no automatic NA or exclusion. Source tracing, evaluated build selection, selected CL observations, runtime consumer assertions and historical authority are separate proof levels. Explicit applicable bilinearspan project is missing, ARM64 execution and historical binding remain unproven; C5 and parent remain Blocked. No claim of complete parent-edge tracing or full product completion.",
    identities,
    rows
};
File.WriteAllText(Path.Combine(output, "C5-final-adjudication.json"), JsonSerializer.Serialize(report, options));
using (var readBack = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "C5-final-adjudication.json"))))
{
    var persisted = JsonSerializer.Deserialize<AuditRow[]>(readBack.RootElement.GetProperty("rows").GetRawText()) ?? throw new InvalidDataException("C5 rows");
    if (persisted.Length != rows.Count || persisted.Where((row, index) => JsonSerializer.Serialize(row) != JsonSerializer.Serialize(rows[index])).Any())
        throw new InvalidDataException("C5 report readback mismatch");
}
File.WriteAllText(Path.Combine(output, "summary.json"), JsonSerializer.Serialize(new { formalEntries = 9, exactAuditRowDenominator = rows.Count, counts, states, resolution, runIdentity, c5Satisfied = false, allApplicableEdgesProven = false, reconciliationRows = reconciliation.GetArrayLength(), filesHashed = identities.Count }, options));
Console.WriteLine(File.ReadAllText(Path.Combine(output, "summary.json")));

internal static partial class AuditPatterns
{
    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.CultureInvariant)]
    internal static partial Regex InlineComment();
    [GeneratedRegex(@"^\s*#\s*(?<op>if|ifdef|ifndef|elif|else|endif|include)\b(?<rest>.*)$", RegexOptions.CultureInvariant)]
    internal static partial Regex Directive();
    [GeneratedRegex("^[<\"](?<value>[^>\"]+)[>\"]", RegexOptions.CultureInvariant)]
    internal static partial Regex Operand();
    [GeneratedRegex("^#(?:line)?\\s+(?<line>[0-9]+)\\s+\"(?<path>[^\"]+)\"", RegexOptions.CultureInvariant)]
    internal static partial Regex LineDirective();
}

internal sealed record AuditRow(int Ordinal, string Kind, string Configuration, string Source, int Line, string Target, string ActiveOwner, string[] ExactRecordIds, string Signature, string Generation, string Assertion, string State, string Reason, string Evidence);
