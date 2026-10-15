using System.Reflection;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class FiniteDifferenceReviewTests
{
    [TestMethod]
    public void WhenManagedContractsAreReviewedThenEveryDeclarationAndBehaviorDifferenceIsRecorded()
    {
        string root = typeof(FiniteDifferenceReviewTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string include = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include");
        string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "finite-review-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        var records = new List<object>();
        var errors = new List<string>();
        foreach (string family in new[] { "wgx_core_types", "wgx_av_types" })
        {
            string frozen = File.ReadAllText(Path.Combine(include, "processed", family + ".cs"));
            string generated = new TemplatePreprocessor(include).Generate(Path.Combine(include, family + ".w")).Cs;
            Dictionary<string, string> Declarations(string text) => CSharpSyntaxTree.ParseText(text).GetRoot().DescendantNodes()
                .Where(n => n is StructDeclarationSyntax or EnumDeclarationSyntax)
                .ToDictionary(n => n is StructDeclarationSyntax s ? s.Identifier.Text : ((EnumDeclarationSyntax)n).Identifier.Text,
                    n => string.Join("|", n.DescendantTokens().Select(t => t.Text)));
            var before = Declarations(frozen);
            var after = Declarations(generated);
            foreach (string name in before.Keys.Union(after.Keys))
            {
                before.TryGetValue(name, out string? expected);
                after.TryGetValue(name, out string? actual);
                bool equal = expected == actual;
                bool compatReviewed = false;
                object? independentEvidence = null;
                if (!equal && name == "MILRTInitializationFlags" && expected != null && actual != null)
                {
                    const string addition = "MIL_RT_DISABLE_DIRTY_RECTANGLES|=|0x00010000|,|";
                    string consumerPath = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_core_types.cs");
                    string nativePath = Path.Combine(include, "processed/wgx_core_types.h");
                    string consumerTokens = Declarations(File.ReadAllText(consumerPath))[name];
                    string native = File.ReadAllText(nativePath);
                    compatReviewed = actual.Replace(addition, "", StringComparison.Ordinal) == expected
                        && actual == consumerTokens
                        && native.Contains("DisableDirtyRectangles = 0x00010000", StringComparison.Ordinal);
                    independentEvidence = new { consumerPath, consumerSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(consumerPath))), nativePath, nativeSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(nativePath))), addedMember = addition, consumerTokens };
                }
                var colorDeclaration = CSharpSyntaxTree.ParseText(frozen).GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>().SingleOrDefault(s => s.Identifier.Text == "MilColorF");
                bool colorReviewed = !equal && name == "MilColorF" && expected != null && actual != null
                    && Declarations(File.ReadAllText(Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_core_types.cs")))[name] == expected
                    && colorDeclaration != null
                    && colorDeclaration.Members.OfType<MethodDeclarationSyntax>().Select(m => m.Identifier.Text).SequenceEqual(new[] { "GetHashCode", "Equals" })
                    && string.Join("|", colorDeclaration.RemoveNodes(colorDeclaration.Members.OfType<MethodDeclarationSyntax>(), Microsoft.CodeAnalysis.SyntaxRemoveOptions.KeepNoTrivia)?.DescendantTokens().Select(t => t.Text) ?? []) == actual;
                string reason = equal ? "All declaration tokens equal: visibility, fields, order, types, attributes, Pack, FieldOffset, enum names/aliases/values and methods retained."
                    : compatReviewed ? "Version-specific compatibility addition only: 0x10000; generated declaration equals current production Common and native retains the same literal. Frozen history is not declared equal."
                    : colorReviewed ? "Reviewed behavior difference: frozen Common retains XOR GetHashCode and ValueType.Equals; generated removes exactly those overrides. ToString is inherited on both. Behavior execution is separately required; not a layout waiver."
                    : "Unreviewed semantic difference";
                if (!equal && !compatReviewed && !colorReviewed) errors.Add(family + ": " + name);
                records.Add(new { family, name, reviewed = equal || compatReviewed || colorReviewed, equal, reason, independentEvidence, frozenTokens = expected, generatedTokens = actual });
            }
        }
        File.WriteAllText(Path.Combine(archive, "managed-contract-review.json"), JsonSerializer.Serialize(new { executed = true, nativeAbiExecuted = false, records, errors }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine(archive);
        Assert.AreEqual(0, errors.Count, string.Join("; ", errors));
    }
}
