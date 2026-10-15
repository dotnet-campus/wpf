using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class CoreAvPreprocessingTests
{
    private static string Root => typeof(CoreAvPreprocessingTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static string Relative(string path) => Path.GetRelativePath(Root, path).Replace('\\', '/');

    [DataTestMethod]
    [DataRow("wgx_core_types")]
    [DataRow("wgx_av_types")]
    public void WhenTemplatePortRunsThenAllFrozenOutputDifferencesAreRecorded(string family)
    {
        new GeneratedDifferenceResolutionTests().WhenFullCoreAvDifferencesAreAlignedThenEveryHunkIsArchived();
        new FiniteDifferenceReviewTests().WhenManagedContractsAreReviewedThenEveryDeclarationAndBehaviorDifferenceIsRecorded();
        new MilColorBehaviorTests().WhenColorDeclarationsExecuteThenFrozenBehaviorAndGeneratedDifferencesAreReported();
        string reviewPath = Directory.EnumerateDirectories(Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence"), "resolution-*")
            .Select(p => Path.Combine(p, "report.json")).Where(File.Exists).MaxBy(File.GetLastWriteTimeUtc)!;
        using var review = JsonDocument.Parse(File.ReadAllText(reviewPath));
        string include = Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include");
        string archive = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence", "preprocessing-" + family + "-" + Guid.NewGuid().ToString("N"));
        string isolated = Path.Combine(archive, "authority");
        Directory.CreateDirectory(isolated);
        // Copy inputs only. Frozen processed files are never generator inputs.
        foreach (string file in Directory.EnumerateFiles(include, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(include, file);
            if (relative.StartsWith("processed" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) continue;
            string target = Path.Combine(isolated, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
        var port = new TemplatePreprocessor(isolated);
        var generated = port.Generate(Path.Combine(isolated, family + ".w"));
        var comparisons = new List<object>();
        var errors = new List<string>();
        foreach (var output in new[] { (Extension: ".h", Text: generated.Cpp), (Extension: ".cs", Text: generated.Cs) })
        {
            string path = Path.Combine(archive, family + output.Extension);
            File.WriteAllText(path, output.Text.Replace("\n", "\r\n", StringComparison.Ordinal));
            string baseline = Path.Combine(include, "processed", family + output.Extension);
            string expected = File.ReadAllText(baseline);
            string normalized = expected.Replace("\r\n", "\n", StringComparison.Ordinal);
            string[] Tokens(string text) => output.Extension == ".cs"
                ? SyntaxFactory.ParseTokens(text).Select(t => t.Text).ToArray()
                : Regex.Matches(Regex.Replace(text, @"//[^\n]*|/\*[\s\S]*?\*/", ""), @"[A-Za-z_]\w*|0[xX][0-9a-fA-F]+|\d+|[^\s]").Select(m => m.Value).ToArray();
            string[] expectedTokens = Tokens(normalized);
            string[] actualTokens = Tokens(output.Text);
            bool tokensEqual = expectedTokens.SequenceEqual(actualTokens);
            int firstDifferentLine = expected.Split('\n').Zip(output.Text.Split('\n')).TakeWhile(p => p.First.TrimEnd('\r') == p.Second).Count() + 1;
            int tokenIndex = expectedTokens.Zip(actualTokens).TakeWhile(p => p.First == p.Second).Count();
            var differences = new { firstDifferentLine, firstDifferentToken = tokenIndex, expectedTokenContext = expectedTokens.Skip(Math.Max(0, tokenIndex - 8)).Take(24).ToArray(), actualTokenContext = actualTokens.Skip(Math.Max(0, tokenIndex - 8)).Take(24).ToArray(), expectedLines = normalized.Split('\n'), generatedLines = output.Text.Split('\n'), expectedTokens, actualTokens };
            Console.WriteLine(output.Extension + " first token difference: " + tokenIndex + " expected=" + string.Join(" ", expectedTokens.Skip(tokenIndex).Take(12)) + " generated=" + string.Join(" ", actualTokens.Skip(tokenIndex).Take(12)));
            File.WriteAllText(Path.Combine(archive, "differences" + output.Extension + ".json"), JsonSerializer.Serialize(differences, new JsonSerializerOptions { WriteIndented = true }));
            bool rawEqual = File.ReadAllBytes(baseline).SequenceEqual(File.ReadAllBytes(path));
            bool crlfNormalizedEqual = normalized == output.Text;
            comparisons.Add(new { path = Relative(path), sha256 = Hash(path), baselinePath = Relative(baseline), baselineSha256 = Hash(baseline), rawTextEqual = rawEqual, crlfNormalizedEqual, tokensEqual, firstDifferentLine });
            var identity = review.RootElement.GetProperty("identities").EnumerateArray().Single(i => i.GetProperty("family").GetString() == family && i.GetProperty("Extension").GetString() == output.Extension);
            if (identity.GetProperty("baselineSha256").GetString() != Hash(baseline)
                || identity.GetProperty("generatedSha256").GetString() != Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(output.Text))))
                errors.Add(output.Extension + ": review identity mismatch");
            string classificationPath = Path.Combine(Path.GetDirectoryName(reviewPath)!, family + output.Extension + ".classification.json");
            using var classifications = JsonDocument.Parse(File.ReadAllText(classificationPath));
            if (classifications.RootElement.EnumerateArray().Any(h => h.GetProperty("category").GetString() == "Unresolved"))
                errors.Add(output.Extension + ": unreviewed full-text hunk");
        }
        var inputs = port.Inputs.Distinct(StringComparer.OrdinalIgnoreCase).Select(p => new { path = Relative(Path.Combine(include, Path.GetRelativePath(isolated, p))), sha256 = Hash(p), copyPath = Relative(p), copySha256 = Hash(p) }).ToArray();
        string report = Path.Combine(archive, "report.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            schemaVersion = "1.0.0", kind = "TemplateAlgorithmPortDifferential", executed = true,
            testName = nameof(WhenTemplatePortRunsThenAllFrozenOutputDifferencesAreRecorded), family,
            runtime = RuntimeInformation.FrameworkDescription, actualArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            algorithmSourceIdentity = new { path = Relative(Path.Combine(include, "gencode.pl")), sha256 = Hash(Path.Combine(include, "gencode.pl")), originalPerlExecuted = false, port = Relative(Path.Combine(Root, "WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/TemplatePreprocessor.cs")), portSha256 = Hash(Path.Combine(Root, "WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/TemplatePreprocessor.cs")) },
            entryPoint = new { path = Relative(Path.Combine(include, "GraphicsInclude.nativeproj")), sha256 = Hash(Path.Combine(include, "GraphicsInclude.nativeproj")), target = "GenerateCodeFromSchemas", wcpSuffix = "", milSuffix = "" },
            inputManifest = inputs, lineMap = port.LineMap, comparisons, errors,
            reviewPath = Relative(reviewPath), reviewSha256 = Hash(reviewPath), equalityRequired = false, acceptanceRule = "Isolated regeneration plus hash-bound full-text classification, strict declaration whitelist and executed frozen/generated behavior; historical text differences remain reported", 
            status = errors.Count == 0 ? "Passed" : "FailedDifferential", nativeLayoutExecuted = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        using var readback = JsonDocument.Parse(File.ReadAllText(report));
        Console.WriteLine("Template differential: " + Relative(report) + " SHA256=" + Hash(report));
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors));
    }
}
