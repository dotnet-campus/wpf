using System.Buffers.Binary;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class PenConsumerContractTests
{
    private static string Root => typeof(PenConsumerContractTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;

    /// <summary>Executes the real managed producer body and a separately identified native-algorithm model.</summary>
    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(5)]
    public void WhenPenProducerFeedsConsumerThenDashBytesAndNativeStrideRemainSeparate(int count)
    {
        string[] paths = [
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.cs",
            "src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media/DashStyle.cs",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/geometry_api.cpp",
            "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h",
            "WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/PenConsumerContractTests.cs"
        ];
        string Read(int i) => File.ReadAllText(Path.Combine(Root, paths[i]));
        var declarations = CSharpSyntaxTree.ParseText(Read(0)).GetRoot();
        var pen = declarations.DescendantNodes().OfType<StructDeclarationSyntax>().Single(s => s.Identifier.ValueText == "MIL_PEN_DATA");
        var enums = declarations.DescendantNodes().OfType<EnumDeclarationSyntax>().Where(e => e.Identifier.ValueText is "MIL_PEN_CAP" or "MIL_PEN_JOIN");
        var producer = CSharpSyntaxTree.ParseText(Read(1)).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.ValueText == "GetDashData");
        string projection = "using System; using System.Collections.Generic; using System.Linq; using System.Runtime.InteropServices; namespace PenEvidence {\n" +
            string.Join("\n", enums.Select(e => e.ToFullString())) + pen.ToFullString() +
            "class DoubleCollection { internal List<double> _collection = new(); internal int Count => _collection.Count; } unsafe class DashStyle { internal DoubleCollection Dashes = new(); internal double Offset;\n" + producer.ToFullString() + "\n" +
            "public static string Produce(double[] input, double offset) { var style = new DashStyle(); style.Dashes._collection.AddRange(input); style.Offset = offset; MIL_PEN_DATA data = default; data.Thickness = 2.5; data.MiterLimit = 7.25; data.StartLineCap = (MIL_PEN_CAP)1; data.EndLineCap = (MIL_PEN_CAP)2; data.DashCap = (MIL_PEN_CAP)3; data.LineJoin = (MIL_PEN_JOIN)1; style.GetDashData(&data, out var dashes); byte[] bytes = new byte[sizeof(MIL_PEN_DATA)]; Marshal.Copy((IntPtr)(&data), bytes, 0, bytes.Length); return System.Text.Json.JsonSerializer.Serialize(new { bytes = Convert.ToHexString(bytes), dashes, size = sizeof(MIL_PEN_DATA) }); } } }";
        projection = System.Text.RegularExpressions.Regex.Replace(projection, @"^\s*#.*$", "", System.Text.RegularExpressions.RegexOptions.Multiline);
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("Pen_" + Guid.NewGuid().ToString("N"), [CSharpSyntaxTree.ParseText(projection)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        using var binary = new MemoryStream();
        var emitted = compilation.Emit(binary);
        Assert.IsTrue(emitted.Success, string.Join(Environment.NewLine, emitted.Diagnostics));
        binary.Position = 0;
        var context = new AssemblyLoadContext("PenContract", isCollectible: true);
        var observations = new List<object>();
        try
        {
            var assembly = context.LoadFromStream(binary);
            Type type = assembly.GetType("PenEvidence.MIL_PEN_DATA", throwOnError: true)!;
            string[] fields = ["Thickness", "MiterLimit", "DashOffset", "StartLineCap", "EndLineCap", "DashCap", "LineJoin", "DashArraySize"];
            int[] offsets = fields.Select(f => Marshal.OffsetOf(type, f).ToInt32()).ToArray();
            CollectionAssert.AreEqual(new[] { 0, 8, 16, 24, 28, 32, 36, 40 }, offsets);
            double[] input = Enumerable.Range(0, count).Select(i => i + 0.125).ToArray();
            string output = (string)assembly.GetType("PenEvidence.DashStyle", true)!.GetMethod("Produce")!.Invoke(null, [input, -1.75])!;
            using var result = JsonDocument.Parse(output);
            byte[] wire = Convert.FromHexString(result.RootElement.GetProperty("bytes").GetString()!);
            Assert.AreEqual(44, wire.Length);
            Assert.AreEqual((uint)(count * sizeof(double)), BinaryPrimitives.ReadUInt32LittleEndian(wire.AsSpan(40)));
            Assert.AreEqual(-1.75, BinaryPrimitives.ReadDoubleLittleEndian(wire.AsSpan(16)));
            Assert.AreEqual(2.5, BinaryPrimitives.ReadDoubleLittleEndian(wire));
            Assert.AreEqual(7.25, BinaryPrimitives.ReadDoubleLittleEndian(wire.AsSpan(8)));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 1 }, Enumerable.Range(0, 4).Select(i => BinaryPrimitives.ReadInt32LittleEndian(wire.AsSpan(24 + i * 4))).ToArray());
            double[] produced = result.RootElement.GetProperty("dashes").ValueKind == JsonValueKind.Null ? [] : result.RootElement.GetProperty("dashes").EnumerateArray().Select(v => v.GetDouble()).ToArray();
            CollectionAssert.AreEqual(input, produced);
            string native = Read(2);
            foreach (string expression in new[] { "pData->DashArraySize / sizeof(double)", "if (cDash & 1)", "UIntMult(cNewDash, 2, &cNewDash)", "static_cast<float>(rgDashDouble[i])", "rgDashFloat + cDash", "FreeHeap(rgDashFloat)" })
                Assert.Contains(expression, native);
            foreach (string architecture in new[] { "Win32", "x64" })
            {
                string report = Directory.EnumerateDirectories(Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence"), "native-core-" + architecture + "-*").Select(d => Path.Combine(d, "report.json")).MaxBy(File.GetLastWriteTimeUtc)!;
                using var measurement = JsonDocument.Parse(File.ReadAllText(report));
                var values = measurement.RootElement.GetProperty("values");
                int stride = values.GetProperty("MilPenData.size").GetInt32();
                Assert.AreEqual(48, stride);
                foreach (var field in fields.Select((name, i) => (name, i))) Assert.AreEqual(offsets[field.i], values.GetProperty("MilPenData." + field.name).GetInt32());
                byte[] records = Enumerable.Repeat((byte)0xA5, stride * 2).ToArray();
                wire.CopyTo(records, 0);
                wire.CopyTo(records, stride);
                BinaryPrimitives.WriteDoubleLittleEndian(records.AsSpan(stride), 9.5);
                Assert.AreEqual(9.5, BinaryPrimitives.ReadDoubleLittleEndian(records.AsSpan(stride)));
                Assert.AreNotEqual(9.5, BinaryPrimitives.ReadDoubleLittleEndian(records.AsSpan(wire.Length)));
                Assert.IsTrue(records.AsSpan(44, 4).ToArray().All(b => b == 0xA5));
                int dashCount = checked((int)(BinaryPrimitives.ReadUInt32LittleEndian(records.AsSpan(40)) / sizeof(double)));
                float[] converted = produced.Take(dashCount).Select(d => (float)d).ToArray();
                float[] consumed = (dashCount & 1) != 0 ? converted.Concat(converted).ToArray() : converted;
                float[] expected = input.Select(d => (float)d).ToArray();
                CollectionAssert.AreEqual(count % 2 == 1 ? expected.Concat(expected).ToArray() : expected, consumed);
                observations.Add(new { architecture, nativeStride = stride, wireExtent = wire.Length, fieldOffsets = offsets, dashCount, consumed, measurement = Path.GetRelativePath(Root, report), measurementHash = Hash(report), wrongStrideRejected = true, nativeRuntimeExecuted = false });
            }
            string archive = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence", "pen-consumer-" + count + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(archive);
            File.WriteAllText(Path.Combine(archive, "projection.cs"), projection);
            File.WriteAllText(Path.Combine(archive, "producer.json"), output);
            foreach (string path in paths) File.Copy(Path.Combine(Root, path), Path.Combine(archive, Path.GetFileName(path)));
            File.WriteAllText(Path.Combine(archive, "report.json"), JsonSerializer.Serialize(new
            {
                executed = true, finiteSubclause = "InitializePen producer byte count, all field offsets, odd/even dash conversion, native two-record stride separation",
                scope = "Real managed GetDashData body with collection/property scaffolding; consumer is a C# model of the cited native algorithm, NOT native production ABI execution",
                actualArchitecture = RuntimeInformation.ProcessArchitecture.ToString(), count, observations,
                sources = paths.Select(p => new { path = p, sha256 = Hash(Path.Combine(Root, p)) }),
                projectionHash = Hash(Path.Combine(archive, "projection.cs")), nativeProductionAbiExecuted = false,
                historicalClrBindingProven = false, arm64Executed = false, completeRequiredEvidenceSatisfied = false, stage4Closed = false,
                remaining = new[] { "Native production InitializePen call and CPlainPen policy/HRESULT/allocator failure execution", "ProcessCreate errors/references/close", "Full physical record and signature/enum denominator closure", "Historical input/CLR binding", "ARM64" }
            }, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("Pen consumer evidence: " + archive);
        }
        finally { context.Unload(); }
    }

    /// <summary>Archives a completed raw GenProto run without treating pre-existing COM results as a new run.</summary>
    [TestMethod]
    public void WhenCompletedGenProtoRunIsArchivedThenRawTrxAndSourceHashesArePreserved()
    {
        string results = Path.Combine(Root, "WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests/TestResults");
        string trx = Directory.EnumerateFiles(results, "*.trx", SearchOption.AllDirectories).MaxBy(File.GetLastWriteTimeUtc)!;
        var document = XDocument.Load(trx);
        XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
        var counters = document.Descendants(ns + "Counters").Single();
        string archive = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence", "pen-delivery-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        File.Copy(trx, Path.Combine(archive, "genproto-full.trx"));
        var sources = Directory.EnumerateFiles(Path.Combine(Root, "WpfGfxShape/Tests/WpfGfxShape.GenProto.Tests"), "*.cs").Select(p => new { path = Path.GetRelativePath(Root, p), sha256 = Hash(p) }).ToArray();
        var reports = Directory.EnumerateDirectories(Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence"), "pen-consumer-*").Select(d => Path.Combine(d, "report.json")).OrderByDescending(File.GetLastWriteTimeUtc).Take(5).ToArray();
        foreach (string report in reports)
        {
            string destination = Path.Combine(archive, Path.GetFileName(Path.GetDirectoryName(report))!);
            Directory.CreateDirectory(destination);
            foreach (string file in Directory.EnumerateFiles(Path.GetDirectoryName(report)!)) File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
        }
        File.WriteAllText(Path.Combine(archive, "report.json"), JsonSerializer.Serialize(new { trx = Path.GetRelativePath(Root, trx), trxHash = Hash(trx), runId = document.Root!.Attribute("id")!.Value, counters = counters.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value), sources, reports, scope = "Completed run selected by timestamp; inspect exact counters/test names, no COM run or publish performed", stage4Closed = false }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Pen delivery archive: " + archive);
        Assert.AreEqual(Hash(trx), Hash(Path.Combine(archive, "genproto-full.trx")));
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}
