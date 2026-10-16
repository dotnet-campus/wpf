using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class X86ClrWireTests
{
    /// <summary>Checks actual sandbox x86 CLR measurements against the frozen native authority and reads every result leaf.</summary>
    [TestMethod]
    public void WhenX86ClrResultsAreReadThenAllFiniteWireFieldsRetainTheirAuthority()
    {
        string root = typeof(X86ClrWireTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string Full(string p) => Path.Combine(root, p);
        string Hash(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
        string directory = Full("WpfGfxShape/Investigations/GenProtoEvidence/x86-clr-wire-final/Results");
        string reportPath = Path.Combine(directory, "report.json");
        using var report = JsonDocument.Parse(File.ReadAllText(reportPath));
        using var identity = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "identity.json")));
        var data = report.RootElement;
        Assert.AreEqual(identity.RootElement.GetProperty("reportSha256").GetString(), Hash(reportPath));
        Assert.AreEqual("X86", data.GetProperty("processArchitecture").GetString());
        Assert.AreEqual("win-x86", data.GetProperty("runtimeIdentifier").GetString());
        Assert.AreEqual("win-x86", data.GetProperty("buildRuntimeIdentifier").GetString());
        Assert.AreEqual(4, data.GetProperty("pointerSize").GetInt32());
        Assert.IsTrue(data.GetProperty("actualClrExecution").GetBoolean());
        Assert.IsTrue(data.GetProperty("passed").GetBoolean());
        Assert.AreEqual(116, data.GetProperty("rows").GetArrayLength());
        Assert.AreEqual(108, data.GetProperty("managedCount").GetInt32());
        Assert.AreEqual(8, data.GetProperty("nativeOnlyCount").GetInt32());
        Assert.AreEqual(654, data.GetProperty("definedFieldCount").GetInt32());
        Assert.AreEqual(6, data.GetProperty("structuralPaddingCount").GetInt32());
        Assert.AreEqual(0, data.GetProperty("sizeDifferences").GetInt32());
        Assert.AreEqual(0, data.GetProperty("definedOffsetDifferences").GetInt32());
        Assert.AreEqual(0, data.GetProperty("enumDifferences").GetInt32());
        Assert.AreEqual(25, data.GetProperty("enums").GetArrayLength());
        string published = Full("WpfGfxShape/artifacts/publish/GenProtoX86Clr.Diagnostics/Release/win-x86");
        Assert.AreEqual(data.GetProperty("assemblySha256").GetString(), Hash(Path.Combine(published, "GenProtoX86Clr.Diagnostics.dll")));
        Assert.AreEqual(data.GetProperty("managedDeclarationsSha256").GetString(), Hash(Path.Combine(published, "Inputs/ManagedFields.cs")));
        Assert.AreEqual(data.GetProperty("nativeAuthoritySha256").GetString(), Hash(Path.Combine(published, "Inputs/native.json")));
        Assert.AreEqual(data.GetProperty("wireAuthoritySha256").GetString(), Hash(Path.Combine(published, "Inputs/wire.json")));
        foreach (var source in data.GetProperty("declarationSources").EnumerateArray())
            Assert.AreEqual(source.GetProperty("sha256").GetString(), Hash(Full(source.GetProperty("path").GetString()!)));
        var values = data.GetProperty("nativeAuthority").GetProperty("values");
        var managedRows = data.GetProperty("rows").EnumerateArray().Where(r => r.GetProperty("managedSize").ValueKind != JsonValueKind.Null).ToArray();
        Assert.AreEqual(656, managedRows.Sum(r => r.GetProperty("managedFields").GetArrayLength()));
        foreach (var row in data.GetProperty("rows").EnumerateArray())
        {
            string name = row.GetProperty("name").GetString()!;
            Assert.AreEqual(values.GetProperty(name + ".size").GetInt32(), row.GetProperty("nativeSize").GetInt32());
            foreach (var field in row.GetProperty("fields").EnumerateArray())
                Assert.AreEqual(values.GetProperty(name + "." + field.GetProperty("name").GetString()).GetInt32(), field.GetProperty("nativeOffset").GetInt32());
        }
        foreach (var row in managedRows)
        {
            Assert.AreEqual(row.GetProperty("nativeSize").GetInt32(), row.GetProperty("managedSize").GetInt32());
            Assert.AreEqual(1, row.GetProperty("managedPack").GetInt32());
            foreach (var field in row.GetProperty("fields").EnumerateArray().Where(f => f.GetProperty("layer").GetString() == "DefinedField"))
                Assert.AreEqual(field.GetProperty("nativeOffset").GetInt32(), field.GetProperty("managedNamedOffset").GetInt32());
        }
        int ReadLeaves(JsonElement e) => e.ValueKind switch
        {
            JsonValueKind.Object => e.EnumerateObject().Sum(p => ReadLeaves(p.Value)),
            JsonValueKind.Array => e.EnumerateArray().Sum(ReadLeaves),
            _ => e.GetRawText().Length >= 0 ? 1 : 0
        };
        var runs = Directory.EnumerateFiles(Full("WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/TestResults"), "*.trx")
            .Select(p => new { path = p, doc = XDocument.Load(p) }).ToArray();
        object? Run(int minimum, int maximum) => runs.Where(r => r.doc.Descendants().Any(e => e.Name.LocalName == "Counters" && (int?)e.Attribute("total") >= minimum && (int?)e.Attribute("total") <= maximum))
            .OrderByDescending(r => File.GetLastWriteTimeUtc(r.path)).Select(r => new { path = Path.GetRelativePath(root, r.path), sha256 = Hash(r.path), runId = (string?)r.doc.Root?.Attribute("id"), counters = r.doc.Descendants().Single(e => e.Name.LocalName == "Counters").ToString() }).FirstOrDefault();
        string receipt = Path.Combine(directory, "verification.json");
        File.WriteAllText(receipt, JsonSerializer.Serialize(new { reportSha256 = Hash(reportPath), leavesRead = ReadLeaves(data), dataRunId = data.GetProperty("runId").GetGuid(), testAssemblySha256 = Hash(typeof(X86ClrWireTests).Assembly.Location), full = Run(43, 43), targeted = Run(1, 1) }, new JsonSerializerOptions { WriteIndented = true }));
        using var readback = JsonDocument.Parse(File.ReadAllText(receipt));
        Assert.IsTrue(readback.RootElement.GetProperty("leavesRead").GetInt32() > 10000);
        Console.WriteLine("X86_CLR_VERIFICATION=" + receipt + " SHA256=" + Hash(receipt));
    }
}
