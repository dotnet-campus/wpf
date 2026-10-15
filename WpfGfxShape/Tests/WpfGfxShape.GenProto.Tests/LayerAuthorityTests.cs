using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class LayerAuthorityTests
{
    private static string Root => typeof(LayerAuthorityTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;

    /// <summary>Checks full identity denominators using names and literal expressions, never numeric alias inference.</summary>
    [TestMethod]
    public void WhenIdentityAuthoritiesAreComparedThenEveryCommandAndResourceMemberIsRetained()
    {
        var results = new List<object>();
        foreach (string family in new[] { "command", "resource" })
        {
            string prefix = "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/Generated/wgx_" + family + "_types";
            string managed = File.ReadAllText(Path.Combine(Root, prefix + ".cs"));
            string native = File.ReadAllText(Path.Combine(Root, prefix + ".h"));
            var declaration = CSharpSyntaxTree.ParseText(managed, new CSharpParseOptions(preprocessorSymbols: ["DBG"])).GetRoot().DescendantNodes().OfType<EnumDeclarationSyntax>().Single();
            var nativeMembers = Regex.Matches(native, @"(?m)^\s*/\*[^*]*\*/\s*(?<name>\w+)\s*=\s*(?<value>0x[0-9a-fA-F]+|[0-9]+)\s*,?")
                .ToDictionary(m => m.Groups["name"].Value, m => m.Groups["value"].Value, StringComparer.Ordinal);
            var managedMembers = declaration.Members.Where(m => m.EqualsValue != null).ToDictionary(m => m.Identifier.ValueText, m => m.EqualsValue!.Value.ToString(), StringComparer.Ordinal);
            Assert.IsNotEmpty(nativeMembers);
            string[] nativeOnly = nativeMembers.Keys.Except(managedMembers.Keys, StringComparer.Ordinal).ToArray();
            CollectionAssert.AreEquivalent(family == "resource" ? new[] { "TYPE_LAST", "TYPE_FORCE_DWORD" } : Array.Empty<string>(), nativeOnly, family + " explicit native-only sentinel set");
            CollectionAssert.AreEquivalent(nativeMembers.Keys.Except(nativeOnly, StringComparer.Ordinal).ToArray(), managedMembers.Keys.ToArray(), family + " identity member denominator");
            foreach (var member in managedMembers) Assert.AreEqual(nativeMembers[member.Key], member.Value, family + ": " + member.Key);
            results.Add(new { family, managedType = declaration.Identifier.ValueText, nativeType = family == "command" ? "MILCMD" : "MIL_RESOURCE_TYPE", denominator = nativeMembers.Count, nativeMembers, nativeOnlySentinels = nativeOnly, sentinelAliasSatisfied = false, members = managedMembers, managedAuthority = Source(prefix + ".cs"), nativeAuthority = Source(prefix + ".h"), declarationIdentitySatisfied = true, productionDispatchCoverageSatisfied = false });
        }
        Save("identity-authority", new { executed = true, results, completeRequiredEvidenceSatisfied = false });
    }

    /// <summary>Executes the exact production command layout in isolation; this does not execute ProcessCreate or a published ABI.</summary>
    [TestMethod]
    public void WhenProductionGlyphLayoutReadsIndependentBytesThenNonzeroPaddingDoesNotChangeFlags()
    {
        string path = "WpfGfxShape/Code/WpfGfxShape/Core/GeneratedGlyphRunResource.cs";
        var syntax = Parse(path);
        string command = syntax.DescendantNodes().OfType<StructDeclarationSyntax>().Single(s => s.Identifier.ValueText == "MilGlyphRunCreateCommand").ToFullString();
        string rectangle = Parse("WpfGfxShape/Code/WpfGfxShape/Core/GeneratedValueResources.cs").DescendantNodes().OfType<RecordDeclarationSyntax>().Single(s => s.Identifier.ValueText == "MilRectD").ToFullString();
        string enumeration = Parse("WpfGfxShape/Code/WpfGfxShape/Core/GeneratedProtocol.cs").DescendantNodes().OfType<EnumDeclarationSyntax>().Single(s => s.Identifier.ValueText == "MilCommand").ToFullString();
        string code = "using System; using System.Runtime.InteropServices; namespace LayoutEvidence {\n" + command + rectangle + enumeration + "\npublic static class Reader { public static ushort Read(byte[] packet) => MemoryMarshal.Read<MilGlyphRunCreateCommand>(packet).Flags; } }";
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("GlyphLayout_" + Guid.NewGuid().ToString("N"), [CSharpSyntaxTree.ParseText(code)], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var binary = new MemoryStream();
        var emitted = compilation.Emit(binary);
        Assert.IsTrue(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
        var context = new AssemblyLoadContext("GlyphLayout", isCollectible: true);
        try
        {
            binary.Position = 0;
            Assembly assembly = context.LoadFromStream(binary);
            var read = assembly.GetType("LayoutEvidence.Reader", true)!.GetMethod("Read")!.CreateDelegate<Func<byte[], ushort>>();
            var measurements = new List<object>();
            foreach (ushort flags in new ushort[] { 0, 0x10, 0x11, 0x8000, 0xffff })
            {
                byte[] bytes = new byte[76];
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(16), flags);
                BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(18), 0xa5a5);
                ushort actual = read(bytes);
                Assert.AreEqual(flags, actual);
                measurements.Add(new { flags, padding = 0xa5a5, actual, zeroExtended = (uint)actual });
            }
            Save("production-glyph-layout", new { executed = true, source = Source(path), declaration = command, projection = code, measurements, productionDeclarationReadExecuted = true, fullProductionConsumerExecuted = false, publishedAbiExecuted = false, satisfied = false });
        }
        finally { context.Unload(); }
    }

    /// <summary>Links actual source symbols by layer and checks consumer signatures without upgrading hash-only physical records.</summary>
    [TestMethod]
    public void WhenLayerSubjectsAreLinkedThenConsumerSignaturesAndUnclosedObligationsAreExplicit()
    {
        string glyphPath = "WpfGfxShape/Code/WpfGfxShape/Core/GeneratedGlyphRunResource.cs";
        var glyphSyntax = Parse(glyphPath);
        var method = glyphSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.ValueText == "ProcessCreate");
        Assert.AreEqual("int", method.ReturnType.ToString());
        Assert.AreEqual("ReadOnlySpan<byte>", method.ParameterList.Parameters.Single().Type!.ToString());
        var flags = glyphSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>().Single(f => f.Declaration.Variables.Any(v => v.Identifier.ValueText == "Flags"));
        Assert.AreEqual("ushort", flags.Declaration.Type.ToString());
        Assert.IsTrue(flags.AttributeLists.ToString().Contains("FieldOffset(16)", StringComparison.Ordinal));
        string header = "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h";
        string native = File.ReadAllText(Path.Combine(Root, header));
        Assert.IsTrue(Regex.IsMatch(native, @"WORD\s+GlyphRunFlags;\s+UINT16\s+UINT16Padding0;"));
        string penPath = "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/geometry_api.cpp";
        string pen = File.ReadAllText(Path.Combine(Root, penPath));
        Assert.IsTrue(pen.Contains("pData->DashArraySize / sizeof(double)", StringComparison.Ordinal));
        string[] penFields = ["Thickness", "MiterLimit", "DashOffset", "StartLineCap", "EndLineCap", "DashCap", "LineJoin", "DashArraySize"];
        foreach (string field in penFields) Assert.IsTrue(pen.Contains("pData->" + field, StringComparison.Ordinal), field);
        var records = File.ReadLines(Path.Combine(Root, "WpfGfxShape/Ledger/records.jsonl")).Select(l => JsonSerializer.Deserialize<JsonElement>(l)).ToArray();
        string[] paths = ["src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_types.w", "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_core_types_compat.cs", "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg/xml/Resource.xml", header, penPath, glyphPath];
        var links = paths.Select(p => new { source = Source(p), physicalRecordIds = records.Where(r => r.TryGetProperty("nativePath", out var n) && n.GetString() == p).Select(r => r.GetProperty("recordId").GetString()).ToArray(), closed = false }).ToArray();
        Save("layer-subject-ledger", new {
            executed = true, links,
            edges = new[] {
                new { from = "GENPROTO-GLYPH-DECLARATION", to = "GENPROTO-GLYPH-WIRE", relationship = "Resource.xml MarshaledSize to native WORD field" },
                new { from = "GENPROTO-GLYPH-WIRE", to = "GENPROTO-GLYPH-CONSUMER", relationship = "ushort FieldOffset16; padding18 excluded" },
                new { from = "GENPROTO-PEN-DECLARATION", to = "GENPROTO-PEN-NATIVE", relationship = "Pack1 named fields to InitializePen named reads, not sizeof copy" }
            },
            nativeOnlyRTMembers = new[] { "ForceCompatible", "DisableDirtyRectangles" },
            historicalOnlyRTMembers = new[] { "MIL_RT_FULLSCREEN", "MIL_RT_LINEAR_GAMMA", "MIL_RT_ALLOW_LOW_PRECISION", "MIL_RT_PRESENT_FLIP", "MIL_RT_FULLSCREEN_NO_AUTOROTATE", "MIL_UCE_RT_ENABLE_OCCLUSION", "MIL_RT_FULLSCREEN_TRANSPOSE_XY" },
            rtWarning = "MIL_UCE_RT_ENABLE_OCCLUSION and DisableDirtyRectangles share a value but are not aliases; all differences remain unsatisfied",
            subjects = new[] {
                new { id = "GENPROTO-GLYPH-DECLARATION", layer = "generated declaration", authority = paths[2], contract = "MilGlyphRun MarshaledSize=2", satisfied = false },
                new { id = "GENPROTO-GLYPH-WIRE", layer = "wire fields", authority = header, contract = "WORD GlyphRunFlags + UINT16Padding0; offset16/18", satisfied = false },
                new { id = "GENPROTO-GLYPH-CONSUMER", layer = "production consumer signature", authority = glyphPath, contract = method.ReturnType + " ProcessCreate" + method.ParameterList, satisfied = false },
                new { id = "GENPROTO-PEN-DECLARATION", layer = "managed declaration", authority = paths[1], contract = "Pack1 extent44", satisfied = false },
                new { id = "GENPROTO-PEN-NATIVE", layer = "native consumer", authority = penPath, contract = "InitializePen named fields; size48/fieldExtent44; no array stride proof", satisfied = false }
            }, signatureVerificationExecuted = true, physicalSourceLinksExecuted = true, physicalLedgerClosureSatisfied = false,
            remaining = new[] { "Full production glyph ProcessCreate and error/ownership execution", "Native InitializePen ABI execution and dash/error/stride evidence", "Per-record generator/consumer signatures and all required evidence", "Historical CLR/hash/input authority", "ARM64 compiler/runtime" }, completeRequiredEvidenceSatisfied = false
        });
    }

    private static SyntaxNode Parse(string path) => CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(Root, path))).GetRoot();
    private static object Source(string path) => new { path, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root, path)))) };
    private static void Save(string family, object report)
    {
        string archive = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence", family + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        File.WriteAllText(Path.Combine(archive, "report.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine(family + ": " + archive);
    }
}
