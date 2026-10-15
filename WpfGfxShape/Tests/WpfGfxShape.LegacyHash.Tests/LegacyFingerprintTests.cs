using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MS.Internal.Csp;

namespace WpfGfxShape.LegacyHash.Tests
{
    [TestClass]
    public class LegacyFingerprintTests
    {
        // Execute unchanged authority under the installed Framework CLR, not a fitted hash.
        [TestMethod]
        public void WhenOriginalFingerprintRunsOnFrameworkThenFrozenValuesAreCompared()
        {
            string root = typeof(LegacyFingerprintTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value;
            string mcg = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/codegen/mcg");
            string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "legacy-fingerprint-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(archive);
            string[] sources = File.ReadAllLines(Path.Combine(mcg, "main/Resources.rsp")).Where(l => l.StartsWith("-s:", StringComparison.Ordinal)).Select(l => Path.Combine(mcg, l.Substring(3))).Concat(new[] { Path.Combine(mcg, "generators/ProtocolFingerprint.cs") }).ToArray();
            var refs = new[] { typeof(object).Assembly, typeof(Enumerable).Assembly, typeof(System.Xml.XmlDocument).Assembly, typeof(System.ComponentModel.TypeConverter).Assembly, typeof(CsPrimeRuntime).Assembly }.Distinct().Select(a => MetadataReference.CreateFromFile(a.Location));
            var compilation = CSharpCompilation.Create("LegacyFingerprint_" + Guid.NewGuid().ToString("N"), CsPrimeParser.Parse(sources, false).Select(s => CSharpSyntaxTree.ParseText(s)), refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using (var binary = new MemoryStream())
            {
                var result = compilation.Emit(binary);
                File.WriteAllLines(Path.Combine(archive, "compilation.txt"), result.Diagnostics.Select(d => d.ToString()));
                Assert.IsTrue(result.Success);
                Assembly authority = Assembly.Load(binary.ToArray());
                string previous = Directory.GetCurrentDirectory();
                try
                {
                    Directory.SetCurrentDirectory(mcg);
                    Type main = authority.GetType("MS.Internal.MilCodeGen.Main.ResourceGenerator", true);
                    object[] args = { new[] { "-xmlFile:" + Path.Combine(mcg, "xml/Resource.xml"), "-xsdFile:" + Path.Combine(mcg, "xml/Resource.xsd"), "-outputDirectory:" + archive }, null };
                    main.GetMethod("ProcessArgs", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Invoke(null, args);
                    Directory.CreateDirectory(Path.Combine(archive, "src/WpfGfx/Include/Generated"));
                    Type collectionType = authority.GetType("MS.Internal.MilCodeGen.Helpers.PaddedCommandCollection", true);
                    object collection = Activator.CreateInstance(collectionType, args[1]);
                    object Read(object instance, string name)
                    {
                        Type type = instance.GetType();
                        PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        return property != null ? property.GetValue(instance, null) : type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(instance);
                    }
                    var trace = new StringBuilder();
                    foreach (object command in (System.Collections.IEnumerable)Read(collection, "PaddedCommands"))
                    {
                        string name = (string)Read(command, "CommandName");
                        trace.AppendLine("COMMAND domain=" + Read(command, "Domain") + " name=" + name + " publicHash=" + name.GetHashCode());
                        foreach (object entry in (System.Collections.IEnumerable)Read(Read(command, "PaddedStructData"), "AlignmentEntries"))
                        {
                            string entryName = (string)Read(entry, "Name");
                            object field = Read(entry, "Field");
                            string typeName = field == null ? null : (string)Read(Read(field, "Type"), "Name");
                            trace.AppendLine("ENTRY name=" + entryName + " publicHash=" + entryName.GetHashCode() + " size=" + Read(entry, "Size") + " animation=" + Read(entry, "IsAnimation") + " pad=" + Read(entry, "IsPad") + " handle=" + Read(entry, "IsHandle") + " offset=" + Read(entry, "Offset") + " fieldType=" + typeName + " typeHash=" + (typeName == null ? "null" : typeName.GetHashCode().ToString()));
                        }
                    }
                    File.WriteAllText(Path.Combine(archive, "hash-inputs-trace.txt"), trace.ToString());
                    Type generator = authority.GetType("MS.Internal.MilCodeGen.Generators.ProtocolFingerprint", true);
                    generator.GetMethod("Go").Invoke(Activator.CreateInstance(generator, args[1]), null);
                    string generated = File.ReadAllText(Path.Combine(archive, "src/WpfGfx/Include/Generated/wgx_sdk_version.cs"));
                    string frozen = File.ReadAllText(Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_sdk_version.cs"));
                    string Tokens(string text) { return string.Join("|", CSharpSyntaxTree.ParseText(text).GetRoot().DescendantTokens().Select(t => t.Text)); }
                    var report = new StringBuilder("runtime=" + Environment.Version + "\npointerSize=" + IntPtr.Size + "\nalgorithmChanges=None\nfrozenTokensEqual=" + (Tokens(generated) == Tokens(frozen)) + "\n");
                    using (var sha = SHA256.Create())
                        foreach (string path in sources.Concat(new[] { Path.Combine(mcg, "xml/Resource.xml"), Path.Combine(mcg, "xml/Version.xml"), Path.Combine(mcg, "xml/Resource.xsd"), Path.Combine(mcg, "xml/Version.xsd"), Path.Combine(mcg, "main/Resources.rsp") }))
                            report.AppendLine(path + " SHA256=" + BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""));
                    report.AppendLine("GENERATED\n" + generated + "\nFROZEN\n" + frozen);
                    File.WriteAllText(Path.Combine(archive, "report.txt"), report.ToString());
                    Assert.AreEqual(4, IntPtr.Size);
                }
                finally { Directory.SetCurrentDirectory(previous); }
            }
        }
    }
}
