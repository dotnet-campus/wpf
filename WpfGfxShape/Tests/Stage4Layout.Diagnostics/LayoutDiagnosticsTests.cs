using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media.Composition;

namespace Stage4Layout.Diagnostics;

[TestClass]
public class LayoutDiagnosticsTests
{
    /// <summary>Measures real consumer CLR types and preserves every unmapped native declaration.</summary>
    [TestMethod]
    public void WhenRealConsumerTypesAreMeasuredThenNativeDifferencesAreArchived()
    {
        string root = typeof(LayoutDiagnosticsTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value;
        string delivery = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence/delivery-8c77ebe1c3214149a5c0410d5e51eeb4");
        string arch = RuntimeInformation.ProcessArchitecture.ToString();
        string nativePath = Path.Combine(delivery, arch == "X86" ? "EVID-GENPROTO-NATIVE-CORE-WIN32.json" : "EVID-GENPROTO-NATIVE-CORE-X64.json");
        using var native = JsonDocument.Parse(File.ReadAllText(nativePath));
        var values = native.RootElement.GetProperty("values").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetInt32());
        var aliases = new Dictionary<string, Type>
        {
            ["MilPenData"] = typeof(MIL_PEN_DATA), ["MilGradientStop"] = typeof(DUCE.MIL_GRADIENTSTOP),
            ["MilPathGeometry"] = typeof(MIL_PATHGEOMETRY), ["MilPathFigure"] = typeof(MIL_PATHFIGURE),
            ["MilSegment"] = typeof(MIL_SEGMENT), ["MilSegmentLine"] = typeof(MIL_SEGMENT_LINE),
            ["MilSegmentBezier"] = typeof(MIL_SEGMENT_BEZIER), ["MilSegmentQuadraticBezier"] = typeof(MIL_SEGMENT_QUADRATICBEZIER),
            ["MilSegmentArc"] = typeof(MIL_SEGMENT_ARC), ["MilSegmentPoly"] = typeof(MIL_SEGMENT_POLY),
            ["MilPoint2D"] = typeof(System.Windows.Point), ["MilSizeD"] = typeof(System.Windows.Size)
        };
        Type[] local = typeof(MilColorF).Assembly.GetTypes().Where(t => t.Namespace == typeof(MilColorF).Namespace).ToArray();
        Type[] installed = typeof(System.Windows.Media.Brush).Assembly.GetTypes();
        var entries = new List<object>();
        foreach (string name in values.Keys.Where(k => k.EndsWith(".size", StringComparison.Ordinal)).Select(k => k[..^5]))
        {
            Type type = aliases.GetValueOrDefault(name) ?? local.SingleOrDefault(t => t.Name == name) ?? installed.SingleOrDefault(t => t.Name == name && t.IsValueType);
            if (type == null)
            {
                entries.Add(new { native = name, status = "NoResolvedManagedCounterpart", nativeSize = values[name + ".size"] });
                continue;
            }
            var fields = new List<object>();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                string nf = field.Name.TrimStart('_');
                if (nf.Length > 0) nf = char.ToUpperInvariant(nf[0]) + nf[1..];
                string raw = field.Name.TrimStart('_');
                string key = values.ContainsKey(name + "." + field.Name) ? name + "." + field.Name : values.ContainsKey(name + "." + raw) ? name + "." + raw : name + "." + nf;
                int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                fields.Add(new { managedField = field.Name, nativeField = key, managedOffset = offset, nativeOffset = values.TryGetValue(key, out int n) ? (int?)n : null, equal = values.TryGetValue(key, out n) && n == offset });
            }
            entries.Add(new { native = name, managed = type.FullName, assembly = type.Assembly.Location, authority = type.Assembly == typeof(MilColorF).Assembly ? "LinkedCurrentCommonSource" : "InstalledRuntimeConsumer", pack = type.StructLayoutAttribute.Pack, layout = type.StructLayoutAttribute.Value.ToString(), managedSize = Marshal.SizeOf(type), nativeSize = values[name + ".size"], sizeEqual = Marshal.SizeOf(type) == values[name + ".size"], fields });
        }
        var enums = local.Where(t => t.IsEnum).Select(t => new { managed = t.FullName, underlying = Enum.GetUnderlyingType(t).FullName, values = t.GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => new { f.Name, value = Convert.ToInt64(f.GetRawConstantValue()) }), nativeCompilerEnumComparisonExecuted = false }).ToArray();
        var trx = XDocument.Load(Path.Combine(delivery, "com-full.trx"));
        XNamespace ns = trx.Root.Name.Namespace;
        var results = trx.Descendants(ns + "UnitTestResult").ToArray();
        var failures = results.Where(r => (string)r.Attribute("outcome") == "Failed").Select(r => new { name = (string)r.Attribute("testName"), message = (string)r.Descendants(ns + "Message").FirstOrDefault(), stdout = (string)r.Descendants(ns + "StdOut").FirstOrDefault(), stack = (string)r.Descendants(ns + "StackTrace").FirstOrDefault() }).ToArray();
        using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(delivery, "report.json")));
        string archive = Path.Combine(root, "WpfGfxShape/Investigations/GenProtoEvidence", "last-managed-" + arch + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        string output = Path.Combine(archive, "report.json");
        File.WriteAllText(output, JsonSerializer.Serialize(new { actualArchitecture = arch, pointerSize = IntPtr.Size, runtime = RuntimeInformation.FrameworkDescription, actualTypeWitness = typeof(MilColorF).FullName, linkedCommonSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_core_types.cs")))), nativePath, entries, enums, enumComparisonComplete = false, trxCounters = trx.Descendants(ns + "Counters").Single().Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value), actualResults = results.Length, actualPassed = results.Count(r => (string)r.Attribute("outcome") == "Passed"), failures, deliveryRemaining = report.RootElement.GetProperty("remaining") }, new JsonSerializerOptions { WriteIndented = true }));
        using var measured = JsonDocument.Parse(File.ReadAllText(output));
        var lines = new List<string> { $"Architecture={arch}; pointerSize={IntPtr.Size}; runtime={RuntimeInformation.FrameworkDescription}; total={results.Length}; passed={results.Count(r => (string)r.Attribute("outcome") == "Passed")}; failed={failures.Length}" };
        foreach (var entry in measured.RootElement.GetProperty("entries").EnumerateArray())
        {
            if (!entry.TryGetProperty("managed", out var mt)) { lines.Add(entry.GetProperty("native").GetString() + " => UNRESOLVED (not proof of CPP_ONLY)"); continue; }
            lines.Add(entry.GetProperty("native").GetString() + " => " + mt.GetString() + "; size=" + entry.GetProperty("managedSize") + "/" + entry.GetProperty("nativeSize") + "; pack=" + entry.GetProperty("pack") + "; fields=" + string.Join(", ", entry.GetProperty("fields").EnumerateArray().Select(f => f.GetProperty("managedField").GetString() + "->" + f.GetProperty("nativeField").GetString() + "=" + f.GetProperty("managedOffset") + "/" + f.GetProperty("nativeOffset"))));
        }
        foreach (var f in failures) lines.Add(f.name + "\n" + f.message + "\n" + f.stdout);
        File.WriteAllLines(Path.Combine(archive, "summary.txt"), lines);
        Console.WriteLine("Independent layout and complete TRX archive: " + output);
        var comparisonErrors = CompilerComparison.Archive(root, archive, measured.RootElement.GetProperty("entries"), local);
        Assert.IsEmpty(comparisonErrors, string.Join(Environment.NewLine, comparisonErrors));
    }
}
