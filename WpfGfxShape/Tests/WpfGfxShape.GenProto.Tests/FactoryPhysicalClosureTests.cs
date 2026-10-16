using System.Globalization;
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
public class FactoryPhysicalClosureTests
{
    private const string Native = "src/Microsoft.DotNet.Wpf/src/WpfGfx/";
    private const string Core = "WpfGfxShape/Code/WpfGfxShape/Core/";
    private const string Results = "WpfGfxShape/artifacts/sandbox-readback/dispatcher-fixed-full/Results/";
    private const string ProductionHash = "3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D";
    private static string Root => typeof(FactoryPhysicalClosureTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
    private static string Full(string path) => Path.Combine(Root, path);
    private static string Read(string path) => File.ReadAllText(Full(path));
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Full(path))));
    private static CompilationUnitSyntax Parse(string path) => CSharpSyntaxTree.ParseText(Read(path)).GetCompilationUnitRoot();

    /// <summary>Checks the complete finite factory denominator against generated sources and actual public AOT results, without approving the parent ABI gate.</summary>
    [TestMethod]
    public void WhenFactoryPhysicalChainIsValidatedThenEveryResourceHasGenerationConsumerAndActualResult()
    {
        string factoryPath = Native + "core/uce/generated_resource_factory.cpp";
        string enumPath = Native + "include/Generated/wgx_resource_types.h";
        string protocolPath = Core + "GeneratedProtocol.cs";
        string valuesPath = Core + "GeneratedValueResources.cs";
        string modelPath = Native + "codegen/mcg/xml/Resource.xml";
        var errors = new List<string>();
        void Require(bool condition, string message) { if (!condition) errors.Add(message); }
        var nativeTypes = Regex.Matches(Read(enumPath), @"TYPE_(\w+)\s*=\s*(\d+)\s*,")
            .ToDictionary(m => int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture), m => m.Groups[1].Value);
        var factory = Regex.Matches(Read(factoryPath), @"case TYPE_(\w+):\s*pResource = new ([^;]+);")
            .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
        var protocol = Parse(protocolPath);
        var managedTypes = protocol.DescendantNodes().OfType<EnumDeclarationSyntax>().Single(e => e.Identifier.ValueText == "MilResourceType")
            .Members.Where(m => m.Identifier.ValueText != "ForceDword").ToDictionary(m => int.Parse(m.EqualsValue!.Value.ToString(), CultureInfo.InvariantCulture), m => m.Identifier.ValueText);
        var create = Parse(valuesPath).DescendantNodes().OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.ValueText == "Create");
        var arms = create.DescendantNodes().OfType<SwitchExpressionArmSyntax>().ToArray();
        var consumers = arms.SelectMany(a => Regex.Matches(a.Pattern.ToString(), @"MilResourceType\.(\w+)")
            .Select(m => new { name = m.Groups[1].Value, expression = a.Expression.ToString() })).ToDictionary(a => a.name, a => a.expression);
        var model = XDocument.Load(Full(modelPath));
        var resources = model.Descendants().Where(e => e.Name.LocalName == "Resource").ToArray();
        var regeneration = Directory.EnumerateDirectories(Full("WpfGfxShape/Investigations/GenProtoEvidence"), "regeneration-*")
            .Select(d => Path.Combine(d, "results.json")).Where(File.Exists).MaxBy(File.GetLastWriteTimeUtc)!;
        using var generation = JsonDocument.Parse(File.ReadAllText(regeneration));
        var generated = generation.RootElement.GetProperty("outputs").EnumerateArray().Single(o => o.GetProperty("baselinePath").GetString() == factoryPath);
        string generatedPath = generated.GetProperty("path").GetString()!;
        Require(Hash(generatedPath) == generated.GetProperty("sha256").GetString(), "Regenerated factory hash changed");
        Require(Read(generatedPath).Replace("\r\n", "\n", StringComparison.Ordinal) == Read(factoryPath).Replace("\r\n", "\n", StringComparison.Ordinal), "Factory regeneration differs");
        string authorityPath = Path.Combine(Path.GetDirectoryName(regeneration)!, "authority.json");
        using var authority = JsonDocument.Parse(File.ReadAllText(authorityPath));
        foreach (var input in authority.RootElement.GetProperty("files").EnumerateArray())
            Require(Hash(input.GetProperty("path").GetString()!) == input.GetProperty("sha256").GetString(), "Generation authority input changed: " + input.GetProperty("path").GetString());

        string trxPath = Results + "dispatcher-fixed-full.trx";
        var trx = XDocument.Load(Full(trxPath));
        XNamespace ns = trx.Root!.Name.Namespace;
        var results = trx.Descendants(ns + "UnitTestResult").ToArray();
        using var identity = JsonDocument.Parse(Read(Results + "aot-identity.json"));
        Require(identity.RootElement.GetProperty("Sha256").GetString() == ProductionHash, "Production identity mismatch");
        Require(identity.RootElement.GetProperty("ResolvedResultsPath").GetString() == identity.RootElement.GetProperty("WorkingDirectory").GetString() + "\\Results", "Results task identity mismatch");
        Require(trx.ToString().Contains(ProductionHash, StringComparison.Ordinal), "TRX does not bind production hash");
        Require((string?)trx.Root.Attribute("id") == "262f277c-5f57-44d7-80a2-1ece70cba1e0", "Unexpected AOT run identity");
        var rows = new List<object>();
        var consumerSources = Directory.EnumerateFiles(Full(Core), "*.cs").ToDictionary(p => p, p => CSharpSyntaxTree.ParseText(File.ReadAllText(p)).GetRoot());
        var links = new Dictionary<string, object>(StringComparer.Ordinal);
        void Link(string path)
        {
            if (links.ContainsKey(path)) return;
            links.Add(path, new { path, sha256 = Hash(path) });
        }
        foreach (string path in new[] { factoryPath, enumPath, protocolPath, valuesPath, modelPath, Native + "codegen/mcg/generators/resourcefactory.cs", Native + "include/Generated/wgx_commands.h", "WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/UceChannelTests.FactoryDenominator.cs", "WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/UceChannelTests.ProcessCreate.cs", "WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/UceChannelTests.cs" }) Link(path);
        foreach (int id in Enumerable.Range(1, 97))
        {
            string nativeName = nativeTypes[id];
            string managedName = managedTypes[id];
            Require(nativeName == managedName.ToUpperInvariant(), "Native/managed ID mismatch: " + id);
            bool supported = factory.TryGetValue(nativeName, out string? nativeConstructor);
            Require(consumers.ContainsKey(managedName) == supported, "Factory consumer coverage mismatch: " + id);
            var resource = resources.SingleOrDefault(r => ((string?)r.Attribute("Name"))?.ToUpperInvariant() == nativeName);
            Require(resource != null, "Missing Resource.xml declaration: " + nativeName);
            string test = supported ? "WhenNativeFactorySupportsTypeThenProductionCreatesAndReleasesIt" : "WhenNativeFactoryCannotCreateResourceTypeThenCommitRejectsIt";
            string testName = test + " (" + id.ToString(CultureInfo.InvariantCulture) + ")";
            var result = results.SingleOrDefault(r => (string?)r.Attribute("testName") == testName);
            Require(result != null && (string?)result.Attribute("outcome") == "Passed", "Missing passed public AOT execution: " + testName);
            string? consumerPath = null;
            string? consumerDeclaration = null;
            if (supported)
            {
                string expression = consumers[managedName];
                string className = Regex.Match(expression, @"new (\w+)").Groups[1].Value;
                var declarations = consumerSources.SelectMany(p => p.Value.DescendantNodes().OfType<ClassDeclarationSyntax>()
                    .Where(c => c.Identifier.ValueText == className).Select(c => new { path = p.Key, node = c })).ToArray();
                Require(declarations.Length > 0, "Missing concrete consumer declaration: " + className);
                consumerPath = string.Join(";", declarations.Select(d => Path.GetRelativePath(Root, d.path).Replace('\\', '/')));
                consumerDeclaration = string.Join(";", declarations.Select(d => d.node.Identifier.ValueText + (d.node.TypeParameterList?.ToString() ?? "") + " " + d.node.BaseList));
                foreach (var declaration in declarations) Link(Path.GetRelativePath(Root, declaration.path).Replace('\\', '/'));
            }
            rows.Add(new { id, nativeName, managedName, supported, modelDeclaration = resource?.ToString(SaveOptions.DisableFormatting), nativeConstructor,
                managedConstructor = supported ? consumers[managedName] : null, consumerPath, consumerDeclaration,
                ordinaryGlyphIgnored = id == 42, actualTestName = testName, actualTestId = (string?)result?.Attribute("testId"), outcome = (string?)result?.Attribute("outcome") });
        }
        Require(factory.Count == 76 && consumers.Count == 76, "Expected 76 concrete factory entries");
        Require(rows.Count == 97, "Expected complete 1..97 denominator");
        var wire = protocol.DescendantNodes().OfType<StructDeclarationSyntax>().Single(s => s.Identifier.ValueText == "MilChannelCreateResourceCommand");
        Require(wire.AttributeLists.ToString().Contains("Size = 12", StringComparison.Ordinal), "Create packet extent changed");
        Require(wire.Members.OfType<FieldDeclarationSyntax>().Select(f => f.ToString()).SequenceEqual(new[] {
            "[FieldOffset(0)] internal readonly MilCommand Type;", "[FieldOffset(4)] internal readonly uint Handle;", "[FieldOffset(8)] internal readonly MilResourceType ResourceType;" }), "Create packet fields changed");
        Require(Read(protocolPath).Contains("ProcessExact<MilChannelCreateResourceCommand>(packet, value => _handlers.ChannelCreateResource(value.Handle, value.ResourceType))", StringComparison.Ordinal), "Wire dispatch signature changed");
        Require(create.ReturnType.ToString() == "int" && create.ParameterList.Parameters.Select(p => p.ToString()).SequenceEqual(new[] { "MilResourceType resourceType", "out GeneratedProtocolResource? resource" }), "Managed factory signature changed");
        foreach (string name in new[] { "WhenOrdinaryGlyphCreateIsIgnoredThenItDoesNotReserveConsumerHandle (0)", "WhenOrdinaryGlyphCreateIsIgnoredThenItDoesNotReserveConsumerHandle (1)", "WhenOrdinaryGlyphCreateIsFollowedByDeleteThenMissingConsumerHandleIsRejected" })
            Require(results.Any(r => (string?)r.Attribute("testName") == name && (string?)r.Attribute("outcome") == "Passed"), "Missing GlyphRun ordinary-create execution: " + name);
        Link(Core + "SameThreadChannel.cs");
        Require(Read(Core + "SameThreadChannel.cs").Contains("GeneratedProtocolPacketWriter.WriteChannelCreateResource(candidate, type)", StringComparison.Ordinal), "Producer wire call changed");
        Require(Read(protocolPath).Contains("return Write(new MilChannelCreateResourceCommand(MilCommand.ChannelCreateResource, handle, resourceType));", StringComparison.Ordinal), "Packet writer changed");
        Require(Read("WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/UceChannelTests.cs").Contains("CreateResource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, \"MilResource_CreateOrAddRefOnChannel\");", StringComparison.Ordinal), "Public consumer export signature changed");
        Require(Regex.IsMatch(Read(Native + "include/Generated/wgx_commands.h"), @"MILCMD Type;\s*HMIL_RESOURCE Handle;\s*MIL_RESOURCE_TYPE resType;\s*\} MILCMD_CHANNEL_CREATERESOURCE;"), "Native wire field signature changed");
        string run = Guid.NewGuid().ToString("N");
        string directory = Full("WpfGfxShape/Investigations/GenProtoEvidence/factory-physical-" + run);
        Directory.CreateDirectory(directory);
        var reports = new[] { trxPath, Results + "aot-identity.json", Path.GetRelativePath(Root, regeneration).Replace('\\', '/'), Path.GetRelativePath(Root, authorityPath).Replace('\\', '/') };
        foreach (string path in reports) File.Copy(Full(path), Path.Combine(directory, Path.GetFileName(path)));
        string report = Path.Combine(directory, "report.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new {
            schemaVersion = "1.0.0", runId = run, executed = true, satisfactionState = errors.Count == 0 ? "Satisfied" : "Blocked",
            scope = "Current-source finite factory identity/generation/wire/constructor/public-AOT mapping, 1..97 only",
            requiredClauses = new[] { "All 97 native IDs match managed IDs and model declarations", "76 concrete factory cases match generated output and managed constructors; other 21 rejected", "Every concrete managed constructor has a physical class declaration", "12-byte producer/dispatch signature retained", "97 actual public AOT results pass under the recorded production/task identity", "Ordinary GlyphRun exception retained" },
            productionSha256 = ProductionHash, aotRunId = (string?)trx.Root.Attribute("id"), aotExecutedThisRun = false,
            generationReport = reports[2], generationExecutedThisTest = false, nativeAbiLayoutClaim = false,
            sourceFiles = links.Values, sourceReports = reports.Select(p => new { path = p, sha256 = Hash(p) }),
            validator = new { path = "WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/FactoryPhysicalClosureTests.cs", sha256 = Hash("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/FactoryPhysicalClosureTests.cs"), assemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(FactoryPhysicalClosureTests).Assembly.Location))) },
            rows, errors, parentRequiredSatisfied = false, stage4Closed = false,
            remaining = new[] { "Historical authority binding", "ARM64 compiler and real execution", "Full generated/consumer physical-file closure beyond this finite factory contract", "Independent native ABI/layout is not proved by managed declarations", "Complete producer/public export/HW contracts remain open" }
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Factory physical evidence: " + report + " SHA256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(report))));
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors));
        string evidencePath = Full("WpfGfxShape/Ledger/evidence.jsonl");
        var evidence = File.ReadLines(evidencePath).Select(l => JsonNode.Parse(l)!.AsObject()).ToList();
        const string evidenceId = "EVID-GENPROTO-FACTORY-PHYSICAL-FINITE";
        var row = evidence.SingleOrDefault(e => e["evidenceId"]?.GetValue<string>() == evidenceId);
        if (row == null) { row = new JsonObject(); evidence.Add(row); }
        row["schemaVersion"] = "1.0.0";
        row["evidenceId"] = evidenceId;
        row["evidenceType"] = "IndependentSubrequirement";
        row["subjectIds"] = new JsonArray("GENPROTO-MILCMD");
        row["status"] = "Passed";
        row["satisfactionState"] = "Satisfied";
        row["executed"] = true;
        row["path"] = Path.GetRelativePath(Root, report).Replace('\\', '/');
        row["hash"] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(report)));
        row["runId"] = run;
        row["configuration"] = "FiniteCurrentSourceMapping";
        row["architecture"] = "X64RecordedPublicAOT-NoNativeLayoutClaim";
        row["notes"] = "97 identities, 76 constructors, 21 rejections, ordinary GlyphRun exception; source/generation/wire/consumer/actual report binding only. Does not satisfy parent closure or historical authority.";
        File.WriteAllLines(evidencePath, evidence.Select(e => e.ToJsonString()));
        string metadataPath = Full("WpfGfxShape/Ledger/schema-version.json");
        var metadata = JsonNode.Parse(File.ReadAllText(metadataPath))!.AsObject();
        metadata["actualCounts"]!["evidence"] = evidence.Count;
        File.WriteAllText(metadataPath, metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }
}
