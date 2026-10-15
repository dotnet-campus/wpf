using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class NativeCoreCoverageTests
{
    private static string Root => typeof(NativeCoreCoverageTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;

    /// <summary>Prepares diagnostic expressions only; values are supplied exclusively by the native compiler.</summary>
    [TestMethod]
    public void WhenCoreDiagnosticsArePreparedThenEveryNativeStructAndFieldHasACompilerExpression()
    {
        string header = Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h");
        string source = File.ReadAllText(header);
        string withoutComments = Regex.Replace(source, @"//[^\r\n]*|/\*.*?\*/", "", RegexOptions.Singleline);
        var structs = Regex.Matches(withoutComments, @"^struct (?<name>\w+)(?:\s*:\s*(?<base>\w+))?\s*\{(?<body>[^}]+)\};", RegexOptions.Multiline)
            .ToDictionary(m => m.Groups["name"].Value, m => (Base: m.Groups["base"].Value, Fields: Regex.Matches(m.Groups["body"].Value, @"\b(?<field>\w+)\s*(?:\[\s*\d+\s*\])?\s*;").Select(f => f.Groups["field"].Value).ToArray()));
        Assert.IsTrue(structs.Count >= 25, "Native declaration coverage unexpectedly shrank.");
        var expressions = new List<(string Name, string Expression)>();
        foreach (var item in structs)
        {
            string type = item.Key;
            expressions.Add((type + ".size", "sizeof(" + type + ")"));
            expressions.Add((type + ".alignment", "alignof(" + type + ")"));
            IEnumerable<string> Fields(string name) => structs[name].Fields.Concat(string.IsNullOrEmpty(structs[name].Base) ? [] : Fields(structs[name].Base));
            foreach (string field in Fields(type)) expressions.Add((type + "." + field, "offsetof(" + type + ", " + field + ")"));
        }
        expressions.Add(("D3DMATRIX.size", "sizeof(D3DMATRIX)"));
        expressions.Add(("D3DMATRIX.alignment", "alignof(D3DMATRIX)"));
        foreach (int row in Enumerable.Range(1, 4))
        foreach (int column in Enumerable.Range(1, 4))
        {
            string field = "_" + row + column;
            expressions.Add(("D3DMATRIX." + field, "offsetof(D3DMATRIX, " + field + ")"));
        }
        expressions.Add(("MilColorF.size", "sizeof(MilColorF)"));
        expressions.Add(("MilColorF.alignment", "alignof(MilColorF)"));
        foreach (string field in new[] { "r", "g", "b", "a" }) expressions.Add(("MilColorF." + field, "offsetof(MilColorF, " + field + ")"));
        string content = "// Test-only expressions derived from the original read-only header.\n" +
            "struct Stage4CoreEntry { char name[128]; unsigned int value; };\n" +
            "extern \"C\" __declspec(dllexport) const Stage4CoreEntry Stage4CoreLayout[] = {\n" +
            string.Join(",\n", expressions.Select(e => "    {\"" + e.Name + "\", " + e.Expression + "}")) +
            "\n};\nextern \"C\" __declspec(dllexport) const unsigned int Stage4CoreLayoutCount = sizeof(Stage4CoreLayout) / sizeof(Stage4CoreLayout[0]);\n";
        string path = Path.Combine(Root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/NativeCore.generated.inc");
        if (!File.Exists(path) || File.ReadAllText(path) != content) File.WriteAllText(path, content);
        string manifest = Path.Combine(Root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/core-expression-manifest.json");
        File.WriteAllText(manifest, JsonSerializer.Serialize(new { header, headerSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(header))), path, expressionsSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), nativeCompilerExecuted = false, structs = structs.Keys, expressions = expressions.Select(e => new { e.Name, e.Expression }) }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Compiler expression manifest: " + manifest);
    }

    /// <summary>Reads all compiler-produced entries without loading a DLL of another architecture.</summary>
    [DataTestMethod]
    [DataRow("Win32", 0x14c)]
    [DataRow("x64", 0x8664)]
    public void WhenCoreNativeDataIsParsedThenEveryPreparedExpressionIsMeasured(string architecture, int machine)
    {
        string dll = Path.Combine(Root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/artifacts", architecture, "Stage4NativeLayout.dll");
        byte[] bytes = File.ReadAllBytes(dll);
        using var pe = new PEReader(new MemoryStream(bytes));
        Assert.AreEqual(machine, (int)pe.PEHeaders.CoffHeader.Machine);
        int Offset(int rva)
        {
            var section = pe.PEHeaders.SectionHeaders.Single(s => rva >= s.VirtualAddress && rva < s.VirtualAddress + Math.Max(s.VirtualSize, s.SizeOfRawData));
            return section.PointerToRawData + rva - section.VirtualAddress;
        }
        uint U32(int offset) => BitConverter.ToUInt32(bytes, offset);
        int exports = Offset(pe.PEHeaders.PEHeader!.ExportTableDirectory.RelativeVirtualAddress);
        int names = Offset((int)U32(exports + 32));
        int ordinals = Offset((int)U32(exports + 36));
        int functions = Offset((int)U32(exports + 28));
        var dataExports = new Dictionary<string, int>();
        for (int i = 0; i < U32(exports + 24); i++)
        {
            int name = Offset((int)U32(names + 4 * i));
            string text = Encoding.ASCII.GetString(bytes, name, Array.IndexOf(bytes, (byte)0, name) - name).TrimStart('_');
            dataExports[text] = Offset((int)U32(functions + 4 * BitConverter.ToUInt16(bytes, ordinals + 2 * i)));
        }
        int count = checked((int)U32(dataExports["Stage4CoreLayoutCount"]));
        int data = dataExports["Stage4CoreLayout"];
        var values = new Dictionary<string, uint>();
        for (int i = 0; i < count; i++)
        {
            int record = data + i * 132;
            string name = Encoding.ASCII.GetString(bytes, record, Array.IndexOf(bytes, (byte)0, record, 128) - record);
            values.Add(name, U32(record + 128));
        }
        int enumCount = checked((int)U32(dataExports["Stage4EnumLayoutCount"]));
        int enumData = dataExports["Stage4EnumLayout"];
        for (int i = 0; i < enumCount; i++)
        {
            int record = enumData + i * 132;
            string name = Encoding.ASCII.GetString(bytes, record, Array.IndexOf(bytes, (byte)0, record, 128) - record);
            values.Add(name, U32(record + 128));
        }
        string manifestPath = Path.Combine(Root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/core-expression-manifest.json");
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        string[] required = manifest.RootElement.GetProperty("expressions").EnumerateArray().Select(e => e.GetProperty("Name").GetString()!).ToArray();
        using var enumManifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/NativeEnums.json")));
        required = required.Concat(enumManifest.RootElement.GetProperty("expressions").EnumerateArray().Select(e => e.GetProperty("Name").GetString()!)).ToArray();
        CollectionAssert.AreEquivalent(required, values.Keys.ToArray());
        string archive = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence", "native-core-" + architecture + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        string report = Path.Combine(archive, "report.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new { architecture, machine, nativeCompilerExecuted = true, nativeRuntimeExecuted = false, managedComparisonExecuted = false, completeCoreRequiredTypes = false, status = "NativeStructMeasurementsOnly", dll, dllSha256 = Convert.ToHexString(SHA256.HashData(bytes)), manifestPath, manifestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(manifestPath))), values }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Native core report: " + report + " SHA256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(report))));
    }
}
