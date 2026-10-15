using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class MilColorBehaviorTests
{
    /// <summary>Executes the original consumer and isolated generated declaration without replacing production overrides.</summary>
    [TestMethod]
    public void WhenColorDeclarationsExecuteThenFrozenBehaviorAndGeneratedDifferencesAreReported()
    {
        string root = typeof(MilColorBehaviorTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string include = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include");
        string consumer = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_core_types.cs");
        string generated = new TemplatePreprocessor(include).Generate(Path.Combine(include, "wgx_core_types.w")).Cs;
        var records = new List<object>();
        var errors = new List<string>();
        foreach (var input in new[] { (Kind: "FrozenConsumer", Text: File.ReadAllText(consumer)), (Kind: "Generated", Text: generated) })
        {
            var declaration = CSharpSyntaxTree.ParseText(input.Text).GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>().Single(s => s.Identifier.Text == "MilColorF");
            string source = "using System; using System.Runtime.InteropServices; namespace System.Windows.Media.Composition { " + declaration.ToFullString() + " }";
            var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p));
            var compilation = CSharpCompilation.Create("ColorReview_" + Guid.NewGuid().ToString("N"), [CSharpSyntaxTree.ParseText(source)], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using var stream = new MemoryStream();
            var emitted = compilation.Emit(stream);
            Assert.IsTrue(emitted.Success, string.Join("; ", emitted.Diagnostics));
            stream.Position = 0;
            var context = new AssemblyLoadContext("ColorReview", isCollectible: true);
            try
            {
                var assembly = context.LoadFromStream(stream);
                Type type = assembly.GetType("System.Windows.Media.Composition.MilColorF", throwOnError: true)!;
                foreach (float[] values in new[] { new[] { 1f, 2f, 3f, 4f }, new[] { float.NaN, -0f, float.PositiveInfinity, float.NegativeInfinity } })
                {
                    object Make(float[] components)
                    {
                        object value = Activator.CreateInstance(type)!;
                        string[] fields = ["r", "g", "b", "a"];
                        for (int i = 0; i < fields.Length; i++) type.GetField(fields[i], BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(value, components[i]);
                        return value;
                    }
                    object color = Make(values);
                    object same = Make(values);
                    float[] changed = (float[])values.Clone();
                    changed[3] = 123f;
                    bool equal = color.Equals(same);
                    bool unequal = !color.Equals(Make(changed));
                    int hash = color.GetHashCode();
                    int xor = values.Aggregate(0, (h, v) => h ^ v.GetHashCode());
                    string? text = color.ToString();
                    if (!equal || !unequal || color.Equals(null) || color.Equals(1)) errors.Add(input.Kind + ": Equals");
                    if (input.Kind == "FrozenConsumer" && hash != xor) errors.Add("FrozenConsumer: XOR hash");
                    if (text != type.FullName) errors.Add(input.Kind + ": inherited ToString");
                    records.Add(new { input.Kind, components = values.Select(v => v.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).ToArray(), equal, unequal, hash, xor, hashEqualsFrozenRule = hash == xor, text, hashDeclaredBy = type.GetMethod("GetHashCode")!.DeclaringType!.FullName, equalsDeclaredBy = type.GetMethod("Equals", [typeof(object)])!.DeclaringType!.FullName, toStringDeclaredBy = type.GetMethod("ToString")!.DeclaringType!.FullName });
                }
            }
            finally { context.Unload(); }
        }
        string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "color-behavior-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        string report = Path.Combine(archive, "report.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new { executed = true, productionOverridesPreserved = true, consumer, consumerSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(consumer))), generatedSha256 = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(generated))), records, errors }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Color behavior report: " + report + " SHA256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(report))));
        Assert.AreEqual(0, errors.Count, string.Join("; ", errors));
    }
}
