using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class GeneratedDifferenceResolutionTests
{
    // Full line alignment preserves every changed comment, directive and declaration.
    [TestMethod]
    public void WhenFullCoreAvDifferencesAreAlignedThenEveryHunkIsArchived()
    {
        string root = typeof(GeneratedDifferenceResolutionTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string include = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include");
        string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "resolution-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        var report = new StringBuilder();
        var identities = new List<object>();
        foreach (string family in new[] { "wgx_core_types", "wgx_av_types" })
        {
            var generated = new TemplatePreprocessor(include).Generate(Path.Combine(include, family + ".w"));
            foreach (var output in new[] { (Extension: ".h", Text: generated.Cpp), (Extension: ".cs", Text: generated.Cs) })
            {
                string baseline = Path.Combine(include, "processed", family + output.Extension);
                string[] expected = File.ReadAllText(baseline).Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
                string[] actual = output.Text.Split('\n');
                var lengths = new int[expected.Length + 1, actual.Length + 1];
                for (int i = expected.Length - 1; i >= 0; i--)
                    for (int j = actual.Length - 1; j >= 0; j--)
                        lengths[i, j] = expected[i] == actual[j] ? lengths[i + 1, j + 1] + 1 : Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
                report.AppendLine("## " + family + output.Extension);
                var classifications = new List<object>();
                int x = 0, y = 0, hunks = 0;
                while (x < expected.Length || y < actual.Length)
                {
                    if (x < expected.Length && y < actual.Length && expected[x] == actual[y]) { x++; y++; continue; }
                    hunks++;
                    int startX = x, startY = y;
                    report.AppendLine($"@@ frozen:{x + 1} generated:{y + 1} @@");
                    while (x < expected.Length || y < actual.Length)
                    {
                        if (x < expected.Length && y < actual.Length && expected[x] == actual[y]) break;
                        if (x < expected.Length && (y == actual.Length || lengths[x + 1, y] >= lengths[x, y + 1])) report.AppendLine("-" + expected[x++]);
                        else report.AppendLine("+" + actual[y++]);
                    }
                    string before = string.Join("\n", expected[startX..x]);
                    string after = string.Join("\n", actual[startY..y]);
                    string Compact(string text) => Regex.Replace(text, @"\s+", "");
                    bool CommentOnly(string text) => text.Split('\n').All(l => string.IsNullOrWhiteSpace(l) || l.TrimStart().StartsWith("//", StringComparison.Ordinal));
                    string category = Compact(before) == Compact(after) ? "WhitespaceOnly"
                        : CommentOnly(before) && CommentOnly(after) ? "CommentOnly"
                        : after.Contains("MIL_RT_DISABLE_DIRTY_RECTANGLES = 0x00010000", StringComparison.Ordinal) ? "CompatFlagAddition"
                        : Regex.IsMatch(before, @"^struct \w+( : \w+)?$") && Regex.IsMatch(after, @"^typedef struct( : \w+)?$") ? "NativeDeclarationOpening"
                        : before == "};" && Regex.IsMatch(after, @"^} \w+;$") ? "NativeDeclarationClosing"
                        : before.Contains("public override int GetHashCode()", StringComparison.Ordinal) && after.Length == 0 ? "ManagedMilColorFOverridesRemoved"
                        : before == "struct MILCMD_TRANSPORT_SYNCFLUSH" && after.EndsWith("typedef struct", StringComparison.Ordinal) ? "NativeOpeningWithGeneratedBanner"
                        : "Unresolved";
                    classifications.Add(new { hunk = hunks, frozenLine = startX + 1, generatedLine = startY + 1, category, before, after });
                }
                File.WriteAllText(Path.Combine(archive, family + output.Extension + ".classification.json"), JsonSerializer.Serialize(classifications, new JsonSerializerOptions { WriteIndented = true }));
                Assert.IsFalse(File.ReadAllText(Path.Combine(archive, family + output.Extension + ".classification.json")).Contains("\"Unresolved\"", StringComparison.Ordinal));
                identities.Add(new { family, output.Extension, baseline, baselineSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(baseline))), generatedSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(output.Text))), hunks, expectedLines = expected.Length, generatedLines = actual.Length });
            }
        }
        string misc = File.ReadAllText(Path.Combine(include, "Generated/wgx_misc.h"));
        string frozen = File.ReadAllText(Path.Combine(include, "processed/wgx_core_types.h"));
        string Fields(string text) => Regex.Match(text, @"(?:typedef struct\s*|struct MilMatrix3x2D\s*)\{(?<fields>\s*DOUBLE S_11;[^}]+)\}(?: MilMatrix3x2D)?;").Groups["fields"].Value;
        File.WriteAllText(Path.Combine(archive, "full-difference-hunks.md"), report.ToString());
        File.WriteAllText(Path.Combine(archive, "report.json"), JsonSerializer.Serialize(new { executed = true, originalPerlExecuted = false, nativeLayoutExecuted = false, identities, matrixFieldsEqual = Fields(misc) == Fields(frozen), authority = "Current workspace; historical generation manifest not established" }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Full difference archive: " + archive);
        Assert.AreEqual(Fields(misc), Fields(frozen));
    }
}
