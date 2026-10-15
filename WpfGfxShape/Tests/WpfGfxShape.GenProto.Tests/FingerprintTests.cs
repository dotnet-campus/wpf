using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MS.Internal.Csp;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class FingerprintTests
{
    [TestMethod]
    public void WhenOriginalFingerprintGeneratorRunsThenSameProcessReproducibilityAndFrozenDifferencesAreRecorded()
    {
        string root = typeof(FingerprintTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;
        string mcg = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg");
        string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "fingerprint-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        string[] source = File.ReadAllLines(Path.Combine(mcg, "main/Resources.rsp")).Where(l => l.StartsWith("-s:", StringComparison.Ordinal)).Select(l => Path.Combine(mcg, l[3..].Replace('\\', Path.DirectorySeparatorChar))).Append(Path.Combine(mcg, "generators/ProtocolFingerprint.cs")).ToArray();
        string Hash(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
        string[] parsed = CsPrimeParser.Parse(source, false);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p)).ToList();
        references.Add(MetadataReference.CreateFromFile(typeof(CsPrimeRuntime).Assembly.Location));
        var compilation = CSharpCompilation.Create("FingerprintAuthority_" + Guid.NewGuid().ToString("N"), parsed.Select(s => CSharpSyntaxTree.ParseText(s)), references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        using var binary = new MemoryStream();
        var emitted = compilation.Emit(binary);
        File.WriteAllLines(Path.Combine(archive, "compilation.txt"), emitted.Diagnostics.Select(d => d.ToString()));
        Assert.IsTrue(emitted.Success);
        binary.Position = 0;
        var context = new AssemblyLoadContext("Fingerprint-" + Guid.NewGuid(), true);
        string previousDirectory = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(mcg);
            var assembly = context.LoadFromStream(binary);
            var resourceModel = assembly.GetType("MS.Internal.MilCodeGen.ResourceModel.ResourceModel", true)!;
            var generator = assembly.GetType("MS.Internal.MilCodeGen.Generators.ProtocolFingerprint", true)!;
            var main = assembly.GetType("MS.Internal.MilCodeGen.Main.ResourceGenerator", true)!;
            var processArgs = main.GetMethod("ProcessArgs", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
            var runs = new List<string>();
            for (int i = 0; i < 2; i++)
            {
                string output = Path.Combine(archive, "run" + i);
                Directory.CreateDirectory(Path.Combine(output, "src/WpfGfx/Include/Generated"));
                object?[] args = [new[] { "-xmlFile:" + Path.Combine(mcg, "xml/Resource.xml"), "-xsdFile:" + Path.Combine(mcg, "xml/Resource.xsd"), "-outputDirectory:" + output }, null];
                processArgs.Invoke(null, args);
                Assert.IsTrue(resourceModel.IsInstanceOfType(args[1]));
                generator.GetMethod("Go")!.Invoke(Activator.CreateInstance(generator, args[1]), null);
                runs.Add(Path.Combine(output, "src/WpfGfx/Include/Generated/wgx_sdk_version.cs"));
            }
            string frozen = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_sdk_version.cs");
            string Tokens(string p) => string.Join("|", CSharpSyntaxTree.ParseText(File.ReadAllText(p)).GetRoot().DescendantTokens().Select(t => t.Text));
            bool sameProcess = Tokens(runs[0]) == Tokens(runs[1]);
            bool frozenEqual = Tokens(runs[0]) == Tokens(frozen);
            File.WriteAllText(Path.Combine(archive, "probe.json"), JsonSerializer.Serialize(new { executed = true, algorithmChanges = "None", runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription, processArchitecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(), inputs = new[] { "main/Resources.rsp", "xml/Resource.xml", "xml/Resource.xsd", "xml/Version.xml", "xml/Version.xsd" }.Select(p => new { path = p, sha256 = Hash(Path.Combine(mcg, p)) }), commonConsumerSha256 = Hash(Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_sdk_version.cs")), authority = source.Select(p => new { path = Path.GetRelativePath(root, p), sha256 = Hash(p) }), sameProcess, frozenEqual, frozenSha256 = Hash(frozen), generatedSha256 = runs.Select(Hash), generatedText = File.ReadAllText(runs[0]), frozenText = File.ReadAllText(frozen), status = frozenEqual ? "FrozenTokensEqual-ABIClosureNotClaimed" : "HistoricalFingerprintMismatch-NotClosureApproved", gap = "Original GetCommandHash uses runtime string.GetHashCode; historical CLR hashing provenance must be established before frozen fingerprint equivalence can be asserted." }, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("Fingerprint probe: " + archive);
            Assert.IsTrue(sameProcess);
        }
        finally { Directory.SetCurrentDirectory(previousDirectory); context.Unload(); }
    }
}
