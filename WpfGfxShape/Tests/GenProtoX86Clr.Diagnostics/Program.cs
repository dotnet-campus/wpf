using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

var assembly = Assembly.GetExecutingAssembly();
string input = Path.Combine(AppContext.BaseDirectory, "Inputs");
string results = Path.Combine(AppContext.BaseDirectory, "Results");
Directory.CreateDirectory(results);
string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
using var wire = JsonDocument.Parse(File.ReadAllText(Path.Combine(input, "wire.json")));
using var native = JsonDocument.Parse(File.ReadAllText(Path.Combine(input, "native.json")));
var values = native.RootElement.GetProperty("values");
var rows = new List<object>();
int sizes = 0, offsets = 0, padding = 0, defined = 0, managed = 0, nativeOnly = 0;
foreach (var row in wire.RootElement.GetProperty("rows").EnumerateArray().Where(r => r.GetProperty("architecture").GetString() == "Win32"))
{
    string name = row.GetProperty("name").GetString()!;
    Type? type = assembly.GetType("WireEvidence." + name);
    int nativeSize = values.GetProperty(name + ".size").GetInt32();
    int? size = type is null ? null : Marshal.SizeOf(type);
    if (type is null) nativeOnly++; else { managed++; if (size != nativeSize) sizes++; }
    var fields = new List<object>();
    foreach (var field in row.GetProperty("fields").EnumerateArray())
    {
        string fieldName = field.GetProperty("Name").GetString()!;
        var info = type?.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        int actualNative = values.GetProperty(name + "." + fieldName).GetInt32();
        int? actual = info is null ? null : checked((int)Marshal.OffsetOf(type!, fieldName));
        bool structural = type is not null && info is null && fieldName.StartsWith("UINT16Padding", StringComparison.Ordinal);
        if (structural) padding++;
        else if (type is not null) { defined++; if (actual != actualNative) offsets++; }
        fields.Add(new { name = fieldName, nativeOffset = actualNative, managedNamedOffset = actual, managedType = info?.FieldType.FullName, layer = type is null ? "NativeOnlyNoManagedCounterpart" : structural ? "ExplicitOffsetHoleOrTrailingSizeMarker" : "DefinedField", equal = info is null ? (bool?)null : actual == actualNative });
    }
    rows.Add(new { name, nativeSize, managedSize = size, nativeAlignment = values.GetProperty(name + ".alignment").GetInt32(), managedPack = type?.StructLayoutAttribute?.Pack, layout = type?.StructLayoutAttribute?.Value.ToString(), managedFields = type?.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(f => new { name = f.Name, offset = checked((int)Marshal.OffsetOf(type, f.Name)), fieldType = f.FieldType.FullName }).ToArray(), fields });
}
int enumDifferences = 0;
var enums = new List<object>();
foreach (var item in wire.RootElement.GetProperty("enums").EnumerateArray())
{
    string name = item.GetProperty("name").GetString()!;
    var type = assembly.GetType("WireEvidence." + name, true)!;
    int width = Marshal.SizeOf(Enum.GetUnderlyingType(type));
    if (width != item.GetProperty("width").GetInt32()) enumDifferences++;
    var members = new List<object>();
    foreach (var member in item.GetProperty("values").EnumerateObject())
    {
        long actual = Convert.ToInt64(type.GetField(member.Name)!.GetRawConstantValue());
        long expected = member.Value.GetInt64();
        if (actual != expected) enumDifferences++;
        members.Add(new { name = member.Name, actual, archivedManagedValue = expected, equal = actual == expected });
    }
    enums.Add(new { name, width, members, nativeMapping = wire.RootElement.GetProperty("enumComparisons").EnumerateArray().Single(e => e.GetProperty("architecture").GetString() == "Win32" && e.GetProperty("managedName").GetString() == name).Clone() });
}
bool architecturePassed = RuntimeInformation.ProcessArchitecture == Architecture.X86 && IntPtr.Size == 4;
bool passed = architecturePassed && rows.Count == 116 && managed == 108 && nativeOnly == 8 && padding == 6 && sizes == 0 && offsets == 0 && enumDifferences == 0;
var report = new
{
    runId = Guid.NewGuid(), utc = DateTimeOffset.UtcNow, runtimeIdentifier = RuntimeInformation.RuntimeIdentifier,
    buildRuntimeIdentifier = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RuntimeIdentifier").Value,
    processArchitecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size,
    framework = RuntimeInformation.FrameworkDescription, actualClrExecution = true, architecturePassed, passed,
    nativeCount = rows.Count, managedCount = managed, nativeOnlyCount = nativeOnly, definedFieldCount = defined,
    structuralPaddingCount = padding, sizeDifferences = sizes, definedOffsetDifferences = offsets, enumDifferences,
    rows, enums, declarationSources = wire.RootElement.GetProperty("sources").Clone(),
    managedDeclarationsSha256 = Hash(Path.Combine(input, "ManagedFields.cs")),
    wireAuthoritySha256 = Hash(Path.Combine(input, "wire.json")), nativeAuthoritySha256 = Hash(Path.Combine(input, "native.json")),
    nativeAuthority = native.RootElement.Clone(), assemblySha256 = Hash(assembly.Location), mvid = assembly.ManifestModule.ModuleVersionId,
    nativeOnlyExecutionProven = false, paddingByteEqualityProven = false, parent = "Blocked", stage4Closed = false
};
string output = Path.Combine(results, "report.json");
File.WriteAllText(output, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText(Path.Combine(results, "identity.json"), JsonSerializer.Serialize(new { reportSha256 = Hash(output), report.runId, report.passed, report.processArchitecture, report.pointerSize, report.runtimeIdentifier, report.assemblySha256, report.nativeAuthoritySha256, report.nativeCount, report.managedCount, report.nativeOnlyCount, report.definedFieldCount, report.structuralPaddingCount, report.sizeDifferences, report.definedOffsetDifferences, report.enumDifferences }, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(File.ReadAllText(Path.Combine(results, "identity.json")));
return passed ? 0 : 1;
