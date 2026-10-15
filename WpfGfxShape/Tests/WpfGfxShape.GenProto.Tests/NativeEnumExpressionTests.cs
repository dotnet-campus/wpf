using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class NativeEnumExpressionTests
{
    /// <summary>Archives compiler expressions rather than interpreting native enum arithmetic as measurements.</summary>
    [TestMethod]
    public void WhenNativeEnumExpressionsArePreparedThenAllMacroEnumMembersAreIncluded()
    {
        string root = typeof(NativeEnumExpressionTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string header = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h");
        string source = Regex.Replace(File.ReadAllText(header), @"//[^\r\n]*|/\*.*?\*/", "", RegexOptions.Singleline);
        var expressions = new List<(string Name, string Expression)>();
        foreach (Match declaration in Regex.Matches(source, @"^BEGIN_MIL(?<kind>FLAG)?ENUM\(\s*(?<name>\w+)\s*\)(?<body>.*?)^END_MIL(?:FLAG)?ENUM", RegexOptions.Multiline | RegexOptions.Singleline))
        {
            string name = declaration.Groups["name"].Value;
            string nativeType = name + (declaration.Groups["kind"].Success ? "::FlagsEnum" : "::Enum");
            expressions.Add(("enum:" + name + ".width", "sizeof(" + nativeType + ")"));
            foreach (Match member in Regex.Matches(declaration.Groups["body"].Value, @"^\s*(?<name>\w+)\s*(?:=|,)", RegexOptions.Multiline))
                expressions.Add(("enum:" + name + "." + member.Groups["name"].Value, "static_cast<unsigned int>(" + name + "::" + member.Groups["name"].Value + ")"));
            expressions.Add(("enum:" + name + ".FORCE_DWORD", "static_cast<unsigned int>(" + name + "::FORCE_DWORD)"));
        }
        Assert.IsGreaterThan(20, expressions.Count);
        string path = Path.Combine(root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/NativeEnums.inc");
        string content = "// Test-only native compiler enum measurements.\nextern \"C\" __declspec(dllexport) const Stage4CoreEntry Stage4EnumLayout[] = {\n" + string.Join(",\n", expressions.Select(e => "    {\"" + e.Name + "\", " + e.Expression + "}")) + "\n};\nextern \"C\" __declspec(dllexport) const unsigned int Stage4EnumLayoutCount = sizeof(Stage4EnumLayout) / sizeof(Stage4EnumLayout[0]);\n";
        File.WriteAllText(path, content);
        File.WriteAllText(Path.ChangeExtension(path, ".json"), JsonSerializer.Serialize(new { header, headerSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(header))), path, expressionsSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), nativeCompilerExecuted = false, expressions = expressions.Select(e => new { e.Name, e.Expression }) }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
