using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class RemainingWireMappingTests
{
    /// <summary>Prepares only the previously unmeasured command enum expressions for independent compilation.</summary>
    [TestMethod]
    public void WhenCommandEnumExpressionsArePreparedThenNamesComeFromFrozenNativeDeclarations()
    {
        string root = typeof(RemainingWireMappingTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string source = File.ReadAllText(Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/Generated/wgx_command_types.h"));
        string body = Regex.Match(source, @"typedef\s+enum\s*\{(?<body>[\s\S]*?)\}\s*MILCMD\s*;").Groups["body"].Value;
        Assert.IsFalse(string.IsNullOrWhiteSpace(body), "Frozen MILCMD typedef was not found.");
        string[] names = Regex.Matches(body, @"\b(Mil\w+)\s*=").Select(m => m.Groups[1].Value).Distinct().ToArray();
        Assert.HasCount(143, names);
        string content = "struct RemainingEntry { char name[128]; unsigned int value; };\nextern \"C\" __declspec(dllexport) const RemainingEntry RemainingEnums[] = {\n{\"enum:MILCMD.width\", sizeof(MILCMD)},\n" + string.Join(",\n", names.Select(n => "{\"enum:MILCMD." + n + "\", static_cast<unsigned int>(" + n + ")}")) + "\n};\nextern \"C\" __declspec(dllexport) const unsigned int RemainingEnumCount = sizeof(RemainingEnums)/sizeof(RemainingEnums[0]);\n";
        File.WriteAllText(Path.Combine(root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/RemainingEnums.inc"), content);
    }

    /// <summary>Closes the archived mapping denominator without treating source contracts as native execution.</summary>
    [TestMethod]
    public void WhenRemainingWireMappingsAreReviewedThenEveryArchivedGapHasAnAuthorityAndDisposition()
    {
        string root = typeof(RemainingWireMappingTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        const string native = "src/Microsoft.DotNet.Wpf/src/WpfGfx/";
        const string archive = "WpfGfxShape/Investigations/GenProtoEvidence/wirecompiler-delivery-b1889ee96dc74a8589cae3542c6f807a/";
        var sources = new HashSet<string>(StringComparer.Ordinal);
        string Full(string p) => Path.Combine(root, p);
        string Hash(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Full(p))));
        string Read(string p) { sources.Add(p); return File.ReadAllText(Full(p)); }
        using var delivery = JsonDocument.Parse(Read(archive + "report.json"));
        using var wire = JsonDocument.Parse(Read(archive + "wire-report.json"));
        Assert.AreEqual("83016676BC1B386A4F26252ADAC217D22AD28BF9C74E064D565A57F3C4471F50", Hash(archive + "report.json"));
        var xml = XDocument.Parse(Read(native + "codegen/mcg/xml/Resource.xml"));
        var xmlEnums = xml.Descendants("Enum").Where(e => e.Attribute("Name") is not null).ToArray();
        string helper = Read(native + "codegen/mcg/helpers/EnumHelper.cs");
        StringAssert.Contains(helper, "[[enumValue.Name]] = [[enumValue.Value]]");
        StringAssert.Contains(helper, "FORCE_WORD = 0xffff");
        StringAssert.Contains(helper, "FORCE_DWORD = 0xffffffff");
        string header = Read(native + "include/processed/wgx_core_types.h");
        string commandHeader = Read(native + "include/Generated/wgx_command_types.h");
        Read(native + "include/processed/wgx_core_types.cs");
        Read(native + "include/Generated/wgx_command_types.cs");
        var compilerDocuments = wire.RootElement.GetProperty("sources").EnumerateArray()
            .Select(s => s.GetProperty("path").GetString()!).Where(p => p.Contains("native-core-", StringComparison.Ordinal))
            .ToDictionary(p => p.Contains("Win32", StringComparison.Ordinal) ? "Win32" : "x64", p => JsonDocument.Parse(Read(p)));
        var additional = new Dictionary<string, Dictionary<string, uint>>();
        foreach (string architecture in new[] { "Win32", "x64" })
        {
            string path = "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/artifacts/" + architecture + "/RemainingEnumLayout.dll";
            sources.Add(path);
            byte[] bytes = File.ReadAllBytes(Full(path));
            using var pe = new PEReader(new MemoryStream(bytes));
            Assert.AreEqual(architecture == "Win32" ? 0x14c : 0x8664, (int)pe.PEHeaders.CoffHeader.Machine);
            int Offset(int rva) { var s = pe.PEHeaders.SectionHeaders.Single(s => rva >= s.VirtualAddress && rva < s.VirtualAddress + Math.Max(s.VirtualSize, s.SizeOfRawData)); return s.PointerToRawData + rva - s.VirtualAddress; }
            uint U32(int offset) => BitConverter.ToUInt32(bytes, offset);
            int exports = Offset(pe.PEHeaders.PEHeader!.ExportTableDirectory.RelativeVirtualAddress);
            var addresses = new Dictionary<string, int>();
            for (int i = 0; i < U32(exports + 24); i++)
            {
                int p = Offset((int)U32(Offset((int)U32(exports + 32)) + i * 4));
                string name = Encoding.ASCII.GetString(bytes, p, Array.IndexOf(bytes, (byte)0, p) - p).TrimStart('_');
                int ordinal = BitConverter.ToUInt16(bytes, Offset((int)U32(exports + 36)) + i * 2);
                addresses.Add(name, Offset((int)U32(Offset((int)U32(exports + 28)) + ordinal * 4)));
            }
            var measured = new Dictionary<string, uint>();
            for (int i = 0; i < U32(addresses["RemainingEnumCount"]); i++)
            {
                int p = addresses["RemainingEnums"] + i * 132;
                measured.Add(Encoding.ASCII.GetString(bytes, p, Array.IndexOf(bytes, (byte)0, p, 128) - p), U32(p + 128));
            }
            additional.Add(architecture, measured);
        }
        Read("WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/RemainingEnums.cpp");
        Read(native + "codegen/mcg/generators/CommandType.cs");
        var enumRows = new List<object>();
        try
        {
            foreach (var audit in delivery.RootElement.GetProperty("enumAudit").EnumerateArray())
            {
                string architecture = audit.GetProperty("architecture").GetString()!;
                string name = audit.GetProperty("name").GetString()!;
                var definition = xmlEnums.SingleOrDefault(e => (string?)e.Attribute("Name") == name);
                string? nativeName = (string?)definition?.Attribute("UnmanagedName");
                // Missing XML names stay unresolved; no prefix or numeric-alias inference.
                if (nativeName is null && definition is not null) nativeName = name;
                var compiler = compilerDocuments[architecture].RootElement.GetProperty("values");
                foreach (var member in audit.GetProperty("missing").EnumerateArray().Concat(audit.GetProperty("differences").EnumerateArray()))
                {
                    string memberName = member.GetProperty("name").GetString()!;
                    var field = definition?.Descendants("Field").SingleOrDefault(f => (string?)f.Attribute("Name") == memberName);
                    string? expression = (string?)field?.Attribute("Value");
                    bool sentinel = memberName is "FORCE_WORD" or "FORCE_DWORD";
                    if (name == "MILCMD") nativeName = "MILCMD";
                    string nativeMember = memberName;
                    if (name == "FillRule")
                    {
                        Assert.AreEqual("MilFillMode", nativeName);
                        StringAssert.Contains(xml.ToString(), "Could be generated from FillRule, but that uses different names");
                        var nativeDefinition = xmlEnums.Single(e => (string?)e.Attribute("Name") == "MilFillMode");
                        nativeMember = memberName switch { "EvenOdd" => "Alternate", "Nonzero" => "Winding", _ => memberName };
                        var nativeField = nativeDefinition.Descendants("Field").Single(e => (string?)e.Attribute("Name") == nativeMember);
                        Assert.AreEqual(expression, (string?)nativeField.Attribute("Value"));
                        StringAssert.Contains(header, "BEGIN_MILENUM( MilFillMode )");
                        StringAssert.Contains(header, nativeMember + " = " + expression);
                    }
                    string? key = nativeName is null ? null : "enum:" + nativeName + "." + nativeMember;
                    uint? actual = key is not null && compiler.TryGetProperty(key, out var value) ? value.GetUInt32() : null;
                    if (key is not null && additional[architecture].TryGetValue(key, out uint extra)) actual = extra;
                    string declaration = name == "MILCMD" ? commandHeader : header;
                    bool frozenMember = Regex.IsMatch(declaration, @"\b" + Regex.Escape(memberName) + @"\s*=");
                    string state = actual.HasValue ? "CompilerMapped" : name == "MILCMD" && frozenMember ? "FrozenCommandSourceOnlyNoCompilerKey" : "BlockedNoIndependentCompilerKey";
                    if (actual.HasValue) Assert.AreEqual(member.GetProperty("managedValue").GetUInt32(), actual.Value, architecture + "/" + name + "/" + memberName);
                    enumRows.Add(new { id = architecture + "/" + name + "/" + memberName, architecture, managedEnum = name, member = memberName,
                        managedValue = member.GetProperty("managedValue").GetUInt32(), nativeName, compilerKey = key, nativeValue = actual,
                        xmlAuthority = definition?.ToString(), xmlMemberExpression = expression, explicitAlias = expression is not null && Regex.IsMatch(expression, @"^[A-Za-z_]\w*$"),
                        sentinel, sentinelAuthority = sentinel ? "EnumHelper.GetManagedEnumForceSize/GetNativeEnumForceSize; not an application value" : null,
                        frozenMember, mappingAuthority = name == "MILCMD" ? native + "include/Generated/wgx_command_types.h" : native + "codegen/mcg/xml/Resource.xml + helpers/EnumHelper.cs + include/processed/wgx_core_types.h",
                        state, productionExecutionProven = false });
                }
            }
        }
        finally { foreach (var document in compilerDocuments.Values) document.Dispose(); }
        string managedHeader = Read(native + "include/Generated/wgx_commands.cs");
        Read(native + "include/Generated/wgx_commands.h");
        string paddingHelper = Read(native + "codegen/mcg/helpers/CodeGenHelpers.cs");
        Read(native + "codegen/mcg/generators/CommandStructure.cs");
        StringAssert.Contains(paddingHelper, "if (unmanagedStruct)");
        StringAssert.Contains(paddingHelper, "private byte \" + GetNextName(\"BYTEPacking\")");
        var paddingRows = new List<object>();
        foreach (var packet in wire.RootElement.GetProperty("rows").EnumerateArray().Where(r => r.GetProperty("offsetsEqual").ValueKind == JsonValueKind.False))
        {
            string name = packet.GetProperty("name").GetString()!;
            string managedBody = Regex.Match(managedHeader, @"struct\s+" + name + @"\s*\{(?<body>[^}]+)\}").Groups["body"].Value;
            foreach (var field in packet.GetProperty("fields").EnumerateArray().Where(f => f.GetProperty("managedOffset").ValueKind == JsonValueKind.Null))
            {
                string fieldName = field.GetProperty("Name").GetString()!;
                StringAssert.StartsWith(fieldName, "UINT16Padding");
                int offset = field.GetProperty("nativeOffset").GetInt32();
                int size = packet.GetProperty("managedSize").GetInt32();
                Assert.IsTrue(offset >= 0 && offset + 2 <= size);
                // A named trailing byte can overlap the padding range without becoming a semantic field.
                var overlapping = Regex.Matches(managedBody, @"\[FieldOffset\((\d+)\)\]\s+private byte (\w+)")
                    .Where(m => int.Parse(m.Groups[1].Value) >= offset && int.Parse(m.Groups[1].Value) < offset + 2).Select(m => m.Groups[2].Value).ToArray();
                paddingRows.Add(new { id = packet.GetProperty("architecture").GetString() + "/" + name + "/" + fieldName,
                    architecture = packet.GetProperty("architecture").GetString(), packet = name, field = fieldName, nativeOffset = offset, length = 2,
                    managedNamedOffset = (int?)null, managedRepresentation = "ExplicitOffsetHoleOrTrailingSizeMarker", overlappingSizeMarkers = overlapping,
                    authority = native + "codegen/mcg/helpers/CodeGenHelpers.cs: AppendStructParameters/AlignedFieldOffsetHelper + generators/CommandStructure.cs",
                    state = "StructuralPaddingMappedNotSemanticField", rawOriginalPreserved = archive + "wire-report.json", byteEqualityRequired = false, rawTransportExecutionProven = false });
            }
        }
        Assert.HasCount(12, paddingRows);
        string[] chain = ["src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/Visual.cs", "src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/VisualProxy.cs", native + "include/exports.cs", native + "core/uce/apifunc.cpp", native + "core/uce/clientchannel.cpp", native + "core/uce/htmaster.cpp", native + "core/uce/composition.cpp", native + "core/uce/generated_process_message.inl", native + "core/resources/node.cpp"];
        var chainText = chain.ToDictionary(p => p, Read);
        StringAssert.Contains(chainText[chain[0]], "_proxy.CreateOrAddRefOnChannel(this, channel, resourceType)");
        StringAssert.Contains(chainText[chain[5]], "MilCmdChannelCreateResource");
        StringAssert.Contains(chainText[chain[8]], "Unexpected MILCMD_VISUAL_CREATE.");
        StringAssert.Contains(chainText[chain[8]], "RRETURN(E_UNEXPECTED)");
        string[] extensions = [".cs", ".cpp", ".h", ".inl"];
        var hits = Directory.EnumerateFiles(Full("src/Microsoft.DotNet.Wpf/src"), "*", SearchOption.AllDirectories)
            .Where(p => extensions.Contains(Path.GetExtension(p), StringComparer.OrdinalIgnoreCase))
            .Select(p => new { path = Path.GetRelativePath(root, p).Replace('\\', '/'), text = File.ReadAllText(p) })
            .Where(p => p.text.Contains("MilCmdVisualCreate", StringComparison.Ordinal) || p.text.Contains("MILCMD_VISUAL_CREATE", StringComparison.Ordinal))
            .Select(p => { sources.Add(p.path); return new { p.path, lines = p.text.Split('\n').Select((text, index) => new { line = index + 1, text = text.TrimEnd('\r') }).Where(l => l.text.Contains("MilCmdVisualCreate", StringComparison.Ordinal) || l.text.Contains("MILCMD_VISUAL_CREATE", StringComparison.Ordinal)).ToArray() }; }).ToArray();
        sources.Add("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/RemainingWireMappingTests.cs");
        string historicalTrx = "WpfGfxShape/artifacts/sandbox-readback/dispatcher-fixed-full/Results/dispatcher-fixed-full.trx";
        var historical = XDocument.Parse(Read(historicalTrx));
        Assert.AreEqual("262f277c-5f57-44d7-80a2-1ece70cba1e0", (string?)historical.Root?.Attribute("id"));
        var nativeOnly = wire.RootElement.GetProperty("nativeOnly").EnumerateArray().Select(e =>
        {
            string name = e.GetProperty("name").GetString()!;
            string? test = name switch
            {
                "MILCMD_BITMAP_SOURCE" => "WhenTypedBitmapBatchStopsThenCommittedSourceLivesUntilChannelClose",
                "MILCMD_CHANNEL_CREATERESOURCE" or "MILCMD_CHANNEL_DUPLICATEHANDLE" => "WhenTypedBitmapBatchStopsThenCommittedSourceLivesUntilChannelClose",
                "MILCMD_MEDIAPLAYER" => "WhenRealSystemMediaIsTransferredThenBothChannelModesConsumeTheBatch",
                _ => null
            };
            var cases = historical.Descendants().Where(n => n.Name.LocalName == "UnitTestResult" && test is not null && ((string?)n.Attribute("testName"))?.StartsWith(test, StringComparison.Ordinal) == true)
                .Select(n => new { name = (string?)n.Attribute("testName"), outcome = (string?)n.Attribute("outcome"), executionId = (string?)n.Attribute("executionId") }).ToArray();
            string state = name switch
            {
                "MILCMD_VISUAL_CREATE" => "NativeConsumerUnsupportedNoTypedProducerLocated",
                "MILCMD_MEDIAPLAYER" => "PublicTypedProducerHistoricalAotCaseBindingNotFullPlayerProtocol",
                "MILCMD_CHANNEL_DELETERESOURCE" => "PublicReleaseProducerMissingPacketSpecificRuntimeBinding",
                "MILCMD_TRANSPORT_DESTROYRESOURCESONCHANNEL" => "InternalChannelTeardownProducerMissingPacketSpecificRuntimeBinding",
                "MILCMD_TRANSPORT_SYNCFLUSH" => "PublicFlushProducerMissingPacketSpecificRuntimeBinding",
                _ => "PublicTypedProducerHistoricalAotCaseBindingNotNativePacketTrace"
            };
            return new { name, source = e.GetProperty("producerSource").GetString(), state, typedCaseBinding = test, cases,
                historicalRunId = (string?)historical.Root?.Attribute("id"), historicalTrx, nativePacketExecutionProven = false };
        }).ToArray();
        Assert.HasCount(8, nativeOnly);
        Assert.HasCount(342, enumRows);
        StringAssert.Contains(Read("WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/RemainingEnums.x64.vcxproj"), "DBG=1");
        Read("WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/RemainingEnums.Win32.vcxproj");
        Read("WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/RemainingEnums.inc");
        var enumJson = JsonSerializer.SerializeToElement(enumRows);
        string directory = Full("WpfGfxShape/Investigations/GenProtoEvidence/remaining-wire-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string report = Path.Combine(directory, "report.json");
        var result = new { enumCount = enumRows.Count, enumStates = enumJson.EnumerateArray().GroupBy(e => e.GetProperty("state").GetString()).ToDictionary(g => g.Key!, g => g.Count()),
            enumMappings = enumRows, paddingCount = paddingRows.Count, paddingMappings = paddingRows,
            visualCreate = new { state = "NativeConsumerUnsupportedNoTypedProductionProducerLocated", searchRoot = "src/Microsoft.DotNet.Wpf/src", extensions, hitFileCount = hits.Length, hits,
                actualVisualCreationChain = chain, actualPacket = "MILCMD_CHANNEL_CREATERESOURCE", legacyPacket = "MILCMD_VISUAL_CREATE", interchangeable = false, productionExecutionProven = false },
            nativeOnlyCount = nativeOnly.Length, nativeOnly, x86Clr = "NotExecuted", arm64 = "BlockedMissingCompilerAndRuntime", requiredParent = "Blocked", stage4Closed = false,
            sources = sources.Order(StringComparer.Ordinal).Select(p => new { path = p, sha256 = Hash(p) }).ToArray(),
            testAssembly = new { path = typeof(RemainingWireMappingTests).Assembly.Location, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(RemainingWireMappingTests).Assembly.Location))), mvid = typeof(RemainingWireMappingTests).Assembly.ManifestModule.ModuleVersionId } };
        File.WriteAllText(report, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
        using var readback = JsonDocument.Parse(File.ReadAllText(report));
        Assert.AreEqual(enumRows.Count, readback.RootElement.GetProperty("enumMappings").GetArrayLength());
        Assert.AreEqual(enumRows.Count, readback.RootElement.GetProperty("enumStates").EnumerateObject().Sum(p => p.Value.GetInt32()));
        foreach (var source in readback.RootElement.GetProperty("sources").EnumerateArray()) Assert.AreEqual(source.GetProperty("sha256").GetString(), Hash(source.GetProperty("path").GetString()!));
        Console.WriteLine("Remaining mapping report: " + report + " SHA256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(report))));
        Console.WriteLine("Counts: " + JsonSerializer.Serialize(new { enumCount = enumRows.Count, result.enumStates, paddingCount = paddingRows.Count, nativeOnlyCount = nativeOnly.Length, visualHitFiles = hits.Length }));
    }
}
