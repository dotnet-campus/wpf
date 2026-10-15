using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class ConsumerAssertionTests
{
    [TestMethod]
    public void WhenRegeneratedProtocolDeclarationsAreComparedThenFrozenCommonConsumerMembersAndOffsetsMatch()
    {
        string root = typeof(ConsumerAssertionTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string archives = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence");
        string report = Directory.EnumerateFiles(archives, "results.json", SearchOption.AllDirectories).MaxBy(File.GetLastWriteTimeUtc)!;
        using var document = JsonDocument.Parse(File.ReadAllText(report));
        var results = new List<object>();
        var errors = new List<string>();
        foreach (var output in document.RootElement.GetProperty("outputs").EnumerateArray())
        {
            string baseline = output.GetProperty("baselinePath").GetString()!;
            if (!baseline.Contains("/WpfGfx/include/Generated/", StringComparison.Ordinal) || !baseline.EndsWith(".cs", StringComparison.Ordinal)) continue;
            if (!File.Exists(Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/Common/Graphics/Generated", Path.GetFileName(baseline)))) continue;
            string name = Path.GetFileName(baseline);
            string consumer = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/Common/Graphics/Generated", name);
            string generated = Path.Combine(root, output.GetProperty("path").GetString()!);
            string Hash(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
            if (Hash(generated) != output.GetProperty("sha256").GetString()) errors.Add("Changed archived output: " + name);
            string[] Declarations(string path) => CSharpSyntaxTree.ParseText(File.ReadAllText(path)).GetRoot().DescendantNodes()
                .Where(n => n is StructDeclarationSyntax or EnumDeclarationSyntax)
                .Select(n => string.Join("|", n.DescendantTokens().Select(t => t.Text))).ToArray();
            string[] actual = Declarations(generated);
            string[] expected = Declarations(consumer);
            if (!actual.SequenceEqual(expected)) errors.Add("Frozen consumer declaration mismatch: " + name);
            results.Add(new { name, authorityOutput = output.GetProperty("path").GetString(), generatedSha256 = Hash(generated), frozenConsumer = Path.GetRelativePath(root, consumer).Replace('\\', '/'), consumerSha256 = Hash(consumer), declarations = actual.Length, equal = actual.SequenceEqual(expected), rule = "Independent frozen Common consumer structs/enums; every attribute, FieldOffset, member type/name, enum ID and literal retained" });
        }
        string archive = Path.Combine(archives, "consumer-assertions-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        File.WriteAllText(Path.Combine(archive, "assertions.json"), JsonSerializer.Serialize(new { executed = true, sourceReport = Path.GetRelativePath(root, report), results, errors, architectureLayoutClaim = false }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Consumer assertions: " + archive);
        Assert.IsNotEmpty(results);
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors));
    }
}
