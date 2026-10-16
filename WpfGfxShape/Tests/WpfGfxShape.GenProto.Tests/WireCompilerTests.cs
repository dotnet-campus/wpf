using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class WireCompilerTests
{
    private static string Root => typeof(WireCompilerTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
    private const string Native = "src/Microsoft.DotNet.Wpf/src/WpfGfx/";
    private static string Full(string p) => Path.Combine(Root, p);
    private static string Hash(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>Compiles the frozen managed field graph independently and compares every command with compiler-produced native data.</summary>
    [TestMethod]
    public void WhenAllWireCommandsAreMeasuredThenMissingManagedPacketsRemainExplicitNativeContracts()
    {
        var inputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CompilationUnitSyntax Read(string path)
        {
            inputs.Add(path);
            return CSharpSyntaxTree.ParseText(File.ReadAllText(Full(path))).GetCompilationUnitRoot();
        }
        string commandsPath = Native + "include/Generated/wgx_commands.cs";
        var commands = Read(commandsPath).DescendantNodes().OfType<StructDeclarationSyntax>().Where(s => s.Identifier.ValueText.StartsWith("MILCMD_", StringComparison.Ordinal)).ToArray();
        Assert.HasCount(108, commands);
        var declarations = new Dictionary<string, MemberDeclarationSyntax>(StringComparer.Ordinal);
        void Add(CompilationUnitSyntax unit)
        {
            foreach (var d in unit.DescendantNodes().Where(n => n is StructDeclarationSyntax or EnumDeclarationSyntax))
            {
                string name = d is StructDeclarationSyntax s ? s.Identifier.ValueText : ((EnumDeclarationSyntax)d).Identifier.ValueText;
                declarations.TryAdd(name, (MemberDeclarationSyntax)d);
            }
        }
        Add(Read(Native + "include/processed/wgx_core_types.cs"));
        Add(Read(Native + "include/exports.cs"));
        foreach (var c in commands) declarations[c.Identifier.ValueText] = c;
        string[] sourceFiles = Directory.GetFiles(Full("src/Microsoft.DotNet.Wpf/src"), "*.cs", SearchOption.AllDirectories);
        var selected = new Dictionary<string, MemberDeclarationSyntax>(StringComparer.Ordinal);
        string Simple(string type) => type.Replace("DUCE.", "", StringComparison.Ordinal).Replace("MS.Win32.NativeMethods.", "", StringComparison.Ordinal).Split('.').Last();
        var primitives = new HashSet<string>(["double", "float", "int", "uint", "long", "ulong", "short", "ushort", "byte", "sbyte", "bool", "char", "Int32", "UInt32", "Int64", "UInt64", "Int16", "UInt16", "Single", "Double", "Byte", "Boolean", "BOOL"]);
        void Select(string name)
        {
            name = Simple(name);
            if (primitives.Contains(name) || selected.ContainsKey(name)) return;
            if (!declarations.ContainsKey(name))
            {
                foreach (string file in sourceFiles.Where(p => Path.GetFileNameWithoutExtension(p) == name || name == "RECT" && Path.GetFileName(p).Contains("NativeMethods", StringComparison.Ordinal)).OrderByDescending(p => p.Contains(Path.DirectorySeparatorChar + "Generated" + Path.DirectorySeparatorChar, StringComparison.Ordinal)))
                {
                    Add(Read(Path.GetRelativePath(Root, file)));
                    if (declarations.ContainsKey(name)) break;
                }
            }
            Assert.IsTrue(declarations.ContainsKey(name), "Missing authoritative dependency: " + name);
            var declaration = declarations[name];
            selected.Add(name, declaration);
            if (declaration is StructDeclarationSyntax structure)
                foreach (var field in structure.Members.OfType<FieldDeclarationSyntax>().Where(f => !f.Modifiers.Any(SyntaxKind.StaticKeyword) && !f.Modifiers.Any(SyntaxKind.ConstKeyword))) Select(field.Declaration.Type.ToString());
        }
        foreach (var c in commands) Select(c.Identifier.ValueText);
        string Clean(MemberDeclarationSyntax declaration)
        {
            if (declaration is EnumDeclarationSyntax e) return e.WithAttributeLists(default).ToFullString();
            var s = (StructDeclarationSyntax)declaration;
            var fields = s.Members.OfType<FieldDeclarationSyntax>().Where(f => !f.Modifiers.Any(SyntaxKind.StaticKeyword) && !f.Modifiers.Any(SyntaxKind.ConstKeyword))
                .Select(f => f.WithDeclaration(f.Declaration.WithType(SyntaxFactory.ParseTypeName(Simple(f.Declaration.Type.ToString())).WithTrailingTrivia(SyntaxFactory.Space))).WithAttributeLists(SyntaxFactory.List(f.AttributeLists.Where(a => a.ToString().Contains("FieldOffset", StringComparison.Ordinal)))));
            return s.WithBaseList(null).WithAttributeLists(SyntaxFactory.List(s.AttributeLists.Where(a => a.ToString().Contains("StructLayout", StringComparison.Ordinal)))).WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(fields)).ToFullString();
        }
        string code = "using System; using System.Runtime.InteropServices; using BOOL = System.UInt32; namespace WireEvidence {\n" + string.Join("\n", selected.Values.Select(Clean)) + "\n}";
        code = Regex.Replace(code, @"^\s*#.*$", "", RegexOptions.Multiline);
        string directory = Full("WpfGfxShape/Investigations/GenProtoEvidence/wirecompiler-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "ManagedFields.cs"), code);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("WireFields_" + Guid.NewGuid().ToString("N"), [CSharpSyntaxTree.ParseText(code)], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        string binary = Path.Combine(directory, "ManagedFields.dll");
        var emitted = compilation.Emit(binary);
        Assert.IsTrue(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
        var assembly = new AssemblyLoadContext("WireFields", isCollectible: false).LoadFromAssemblyPath(binary);
        var managed = commands.ToDictionary(c => c.Identifier.ValueText, c => assembly.GetType("WireEvidence." + c.Identifier.ValueText, true)!);
        var nativeHeader = File.ReadAllText(Full(Native + "include/Generated/wgx_commands.h"));
        var packets = Regex.Matches(nativeHeader, @"typedef struct\s*\{(?<body>[^}]+)\}\s*(?<name>MILCMD_\w+);").ToDictionary(m => m.Groups["name"].Value, m => Regex.Matches(m.Groups["body"].Value, @"(?<type>[\w:]+)\s+(?<name>\w+)\s*;").Select(f => (Name: f.Groups["name"].Value, Type: f.Groups["type"].Value)).ToArray());
        Assert.HasCount(116, packets);
        var rows = new List<object>();
        foreach (string architecture in new[] { "Win32", "x64" })
        {
            string report = Directory.GetDirectories(Full("WpfGfxShape/Investigations/GenProtoEvidence"), "native-core-" + architecture + "-*").Select(d => Path.Combine(d, "report.json")).MaxBy(File.GetLastWriteTimeUtc)!;
            inputs.Add(Path.GetRelativePath(Root, report));
            using var doc = JsonDocument.Parse(File.ReadAllText(report));
            var values = doc.RootElement.GetProperty("values");
            foreach (var packet in packets)
            {
                managed.TryGetValue(packet.Key, out var type);
                int? managedSize = type is null ? null : Marshal.SizeOf(type);
                int nativeSize = values.GetProperty(packet.Key + ".size").GetInt32();
                var fields = packet.Value.Select(f => new { f.Name, nativeType = f.Type, nativeOffset = values.GetProperty(packet.Key + "." + f.Name).GetInt32(), managedOffset = type?.GetField(f.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is null ? (int?)null : checked((int)Marshal.OffsetOf(type, f.Name)), managedType = type?.GetField(f.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.FieldType.FullName }).ToArray();
                rows.Add(new { architecture, name = packet.Key, nativeSize, nativeAlignment = values.GetProperty(packet.Key + ".alignment").GetInt32(), managedSize, managedPack = type?.StructLayoutAttribute?.Pack, managedLayout = type?.StructLayoutAttribute?.Value.ToString(), managedExecution = RuntimeInformation.ProcessArchitecture == Architecture.X64 ? (architecture == "x64" ? "ActualX64CLR" : "X64CLRFixedWidthObservationNotX86CLR") : RuntimeInformation.ProcessArchitecture.ToString(), fields, sizeEqual = managedSize is null ? (bool?)null : managedSize == nativeSize, offsetsEqual = type is null ? (bool?)null : fields.All(f => f.managedOffset == f.nativeOffset), disposition = type is null ? "NativeOnlyProducerContractRequiredNotEquivalent" : managedSize == nativeSize && fields.All(f => f.managedOffset == f.nativeOffset) ? "MeasuredLayoutMatch" : "MeasuredDifferenceRequiresWireVsContainerReview" });
            }
        }
        string[] authority = [Native + "include/Generated/wgx_commands.h", Native + "include/processed/wgx_core_types.h", Native + "codegen/mcg/xml/Resource.xml", Native + "codegen/mcg/helpers/PaddedCommand.cs", Native + "codegen/mcg/ResourceModel/Struct.cs", Native + "codegen/mcg/ResourceModel/ResourceModel.cs", Native + "codegen/mcg/generators/CommandStructure.cs", Native + "core/uce/apifunc.cpp", Native + "core/uce/generated_process_message.inl", Native + "core/resources/videoslave.cpp", "src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/MediaPlayerState.cs", "WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/WireCompilerTests.cs"];
        inputs.UnionWith(authority);
        var xml = XDocument.Load(Full(Native + "codegen/mcg/xml/Resource.xml"));
        var media = xml.Descendants("Command").Single(e => (string?)e.Attribute("Target") == "MediaPlayer" && (string?)e.Attribute("Name") == "Update");
        Assert.AreEqual("true", (string?)media.Attribute("UnmanagedOnly"));
        Assert.IsFalse(xml.Descendants("Generate").Any(e => (string?)e.Attribute("Name") == "MediaPlayer"));
        string[] missing = packets.Keys.Except(managed.Keys).Order(StringComparer.Ordinal).ToArray();
        Assert.HasCount(8, missing);
        var enums = selected.Values.OfType<EnumDeclarationSyntax>().Select(e => { var t = assembly.GetType("WireEvidence." + e.Identifier.ValueText, true)!; return new { name = e.Identifier.ValueText, width = Marshal.SizeOf(Enum.GetUnderlyingType(t)), values = t.GetFields(BindingFlags.Public | BindingFlags.Static).ToDictionary(f => f.Name, f => f.GetRawConstantValue()) }; }).ToArray();
        var producers = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["MILCMD_TRANSPORT_SYNCFLUSH"] = "core/uce/connection.cpp: SyncFlush; core/uce/connectioncontext.cpp: cmdFlush",
            ["MILCMD_TRANSPORT_DESTROYRESOURCESONCHANNEL"] = "core/uce/connectioncontext.cpp: cmdDestroy",
            ["MILCMD_CHANNEL_CREATERESOURCE"] = "core/uce/htmaster.cpp: MilCmdChannelCreateResource; core/uce/composition.cpp: cmd",
            ["MILCMD_CHANNEL_DELETERESOURCE"] = "core/uce/htmaster.cpp: MilCmdChannelDeleteResource",
            ["MILCMD_CHANNEL_DUPLICATEHANDLE"] = "core/uce/htmaster.cpp: MilCmdChannelDuplicateHandle",
            ["MILCMD_BITMAP_SOURCE"] = "core/uce/apifunc.cpp: MilCmdBitmapSource",
            ["MILCMD_MEDIAPLAYER"] = "core/uce/apifunc.cpp: MilResource_SendCommandMedia -> MilCmdMediaPlayer",
            ["MILCMD_VISUAL_CREATE"] = "No direct MilCmdVisualCreate producer found in source search; normal channel CreateResource is a distinct packet, not proof of this producer"
        };
        foreach (string path in new[] { "core/uce/connection.cpp", "core/uce/connectioncontext.cpp", "core/uce/htmaster.cpp", "core/uce/composition.cpp", "core/resources/node.cpp" }) inputs.Add(Native + path);
        var enumComparisons = new List<object>();
        foreach (string architecture in new[] { "Win32", "x64" })
        {
            string report = inputs.Single(p => p.Contains("native-core-" + architecture + "-", StringComparison.Ordinal));
            using var nativeData = JsonDocument.Parse(File.ReadAllText(Full(report)));
            var values = nativeData.RootElement.GetProperty("values");
            foreach (var e in enums)
            {
                string nativeName = e.name switch { "MILRTInitializationFlags" => "MilRTInitialization", "MILTransparencyFlags" => "MilTransparency", _ => e.name.StartsWith("Mil", StringComparison.Ordinal) || e.name.StartsWith("MIL", StringComparison.Ordinal) ? e.name : "Mil" + e.name };
                string key = "enum:" + nativeName + ".width";
                bool found = values.TryGetProperty(key, out var width);
                var members = e.values.Select(v =>
                {
                    bool measured = values.TryGetProperty("enum:" + nativeName + "." + v.Key, out var actual);
                    uint managedValue = unchecked((uint)Convert.ToInt64(v.Value));
                    return new { name = v.Key, managedValue, nativeValue = measured ? (uint?)actual.GetUInt32() : null, equal = measured ? (bool?)(actual.GetUInt32() == managedValue) : null };
                }).ToArray();
                enumComparisons.Add(new { architecture, managedName = e.name, nativeName, managedWidth = e.width, nativeWidth = found ? (int?)width.GetInt32() : null, widthEqual = found ? (bool?)(width.GetInt32() == e.width) : null, members, disposition = found ? "CompilerValuesComparedPreserveDifferencesAndMissingMembers" : "BlockedNoNativeEnumMeasurement" });
            }
        }
        var nativeOnly = missing.Select(name =>
        {
            var body = packets[name];
            var candidates = xml.Descendants("Command").Where(e => (string?)e.Attribute("UnmanagedOnly") == "true").Where(e =>
            {
                string domain = (string?)e.Attribute("Domain") ?? "Mil";
                string target = (string?)e.Attribute("Target") ?? "";
                string suffix = (string?)e.Attribute("Name") ?? "";
                string computed = "MILCMD" + (domain == "Mil" || domain == "Resource" ? "" : "_" + domain.ToUpperInvariant()) + (target.Length == 0 ? "" : "_" + target.ToUpperInvariant()) + (suffix == "Update" ? "" : "_" + suffix.ToUpperInvariant());
                return computed == name;
            }).ToArray();
            Assert.HasCount(1, candidates, name);
            return new { name, origin = "ExplicitUnmanagedOnlyCommand", xml = candidates.Single().ToString(), nativeFields = body.Select(f => new { f.Name, f.Type }).ToArray(), producerSource = producers[name], wireSource = Native + "include/Generated/wgx_commands.h", consumerSource = Native + "core/uce/generated_process_message.inl: " + name, equivalenceClaimed = false, producerExecution = "NotProvenByLayoutMeasurement" };
        }).ToArray();
        string output = Path.Combine(directory, "report.json");
        File.WriteAllText(output, JsonSerializer.Serialize(new { nativeCount = packets.Count, managedCount = managed.Count, fieldCount = packets.Values.Sum(f => f.Length), rows, enums, enumComparisons, nativeOnly, missingManaged = missing, mediaPlayer = new { origin = "ExplicitCommandNotResource", xml = media.ToString(), resourceNativeDuceSelected = false, resourceManagedClassSelected = false, commandTypeRule = "MilCmd + empty Mil domain + MediaPlayer + empty Update suffix", producer = "MilResource_SendCommandMedia: player={MilCmdMediaPlayer}; Handle,pMedia,notifyUceDirect; SendCommand sizeof(player)", consumer = "MilCmdMediaPlayer -> TYPE_MEDIAPLAYER -> CMilSlaveVideo.ProcessUpdate(MILCMD_MEDIAPLAYER*)", productionExecutionProven = false }, arm64 = "BlockedMissingCompilerAndRuntime", x86Clr = "NotExecutedNoX86CLRClaim", requiredParent = "Blocked", stage4Closed = false, sources = inputs.Order(StringComparer.Ordinal).Select(p => new { path = p, sha256 = Hash(Full(p)) }).ToArray(), assembly = new { path = binary, sha256 = Hash(binary), mvid = assembly.ManifestModule.ModuleVersionId }, testAssembly = new { sha256 = Hash(typeof(WireCompilerTests).Assembly.Location), mvid = typeof(WireCompilerTests).Assembly.ManifestModule.ModuleVersionId } }, Options));
        using var readback = JsonDocument.Parse(File.ReadAllText(output));
        Assert.AreEqual(232, readback.RootElement.GetProperty("rows").GetArrayLength());
        var measured = readback.RootElement.GetProperty("rows").EnumerateArray().ToArray();
        var summary = new { report = output, sha256 = Hash(output), nativeCount = packets.Count, managedCount = managed.Count, nativeFieldCount = packets.Values.Sum(f => f.Length), managedFieldCount = commands.Sum(c => c.Members.OfType<FieldDeclarationSyntax>().Sum(f => f.Declaration.Variables.Count)), enumCount = enums.Length, architectures = measured.GroupBy(r => r.GetProperty("architecture").GetString()!).Select(g => new { architecture = g.Key, commands = g.Count(), sizeDifferences = g.Where(r => r.GetProperty("sizeEqual").ValueKind == JsonValueKind.False).Select(r => new { name = r.GetProperty("name").GetString(), native = r.GetProperty("nativeSize").GetInt32(), managed = r.GetProperty("managedSize").GetInt32() }).ToArray(), offsetDifferences = g.Where(r => r.GetProperty("offsetsEqual").ValueKind == JsonValueKind.False).Select(r => new { name = r.GetProperty("name").GetString(), fields = r.GetProperty("fields").EnumerateArray().Where(f => f.GetProperty("managedOffset").ValueKind == JsonValueKind.Null || f.GetProperty("managedOffset").GetInt32() != f.GetProperty("nativeOffset").GetInt32()).ToArray() }).ToArray() }).ToArray() };
        string summaryPath = Path.Combine(directory, "summary.json");
        File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, Options));
        using var summaryReadback = JsonDocument.Parse(File.ReadAllText(summaryPath));
        Assert.AreEqual(2, summaryReadback.RootElement.GetProperty("architectures").GetArrayLength());
        Console.WriteLine("WIRECOMPILER_REPORT=" + output + " SHA256=" + Hash(output));
    }
}
