using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Stage4Layout.Diagnostics;

internal static class CompilerComparison
{
    internal static Dictionary<string, uint> ReadData(string dll, string export)
    {
        byte[] bytes = File.ReadAllBytes(dll);
        using var pe = new PEReader(new MemoryStream(bytes));
        int expectedMachine = RuntimeInformation.ProcessArchitecture == Architecture.X86 ? 0x14c : 0x8664;
        if ((int)pe.PEHeaders.CoffHeader.Machine != expectedMachine) throw new InvalidDataException("Native architecture mismatch.");
        int Offset(int rva)
        {
            var section = pe.PEHeaders.SectionHeaders.Single(s => rva >= s.VirtualAddress && rva < s.VirtualAddress + Math.Max(s.VirtualSize, s.SizeOfRawData));
            return section.PointerToRawData + rva - section.VirtualAddress;
        }
        uint U32(int offset) => BitConverter.ToUInt32(bytes, offset);
        int table = Offset(pe.PEHeaders.PEHeader.ExportTableDirectory.RelativeVirtualAddress);
        int names = Offset((int)U32(table + 32)), ordinals = Offset((int)U32(table + 36)), functions = Offset((int)U32(table + 28));
        var exports = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < U32(table + 24); i++)
        {
            int name = Offset((int)U32(names + i * 4));
            exports.Add(Encoding.ASCII.GetString(bytes, name, Array.IndexOf(bytes, (byte)0, name) - name).TrimStart('_'), Offset((int)U32(functions + 4 * BitConverter.ToUInt16(bytes, ordinals + i * 2))));
        }
        int data = exports[export], count = checked((int)U32(exports[export + "Count"]));
        var values = new Dictionary<string, uint>(StringComparer.Ordinal);
        for (int i = 0; i < count; i++)
        {
            int record = data + i * 132;
            values.Add(Encoding.ASCII.GetString(bytes, record, Array.IndexOf(bytes, (byte)0, record, 128) - record), U32(record + 128));
        }
        return values;
    }

    internal static List<string> Archive(string root, string archive, JsonElement entries, Type[] local)
    {
        string architecture = RuntimeInformation.ProcessArchitecture.ToString();
        string dll = Path.Combine(root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/artifacts", architecture == "X86" ? "Win32" : "x64", "Stage4NativeLayout.dll");
        var native = ReadData(dll, "Stage4CoreLayout");
        var enumValues = ReadData(dll, "Stage4EnumLayout");
        string manifestPath = Path.Combine(root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/core-expression-manifest.json");
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
        string[] required = manifest.RootElement.GetProperty("expressions").EnumerateArray().Select(e => e.GetProperty("Name").GetString()).ToArray();
        var errors = new List<string>();
        if (!required.ToHashSet(StringComparer.Ordinal).SetEquals(native.Keys)) errors.Add("Native compiler expression denominator mismatch");
        var rows = new List<object>();
        foreach (var entry in entries.EnumerateArray())
        {
            string name = entry.GetProperty("native").GetString();
            if (!entry.TryGetProperty("managed", out var managed))
            {
                rows.Add(new { nativeName = name, classification = "UNRESOLVED", comparisonExecuted = false, unresolvedReason = "No exact managed mapping or native-only consumer authority certificate" });
                errors.Add(name + ": unresolved counterpart authority");
                continue;
            }
            int size = entry.GetProperty("managedSize").GetInt32();
            int nativeSize = checked((int)native[name + ".size"]);
            int alignment = checked((int)native[name + ".alignment"]);
            bool payload = managed.GetString().StartsWith("System.Windows.Media.MILCMD_", StringComparison.Ordinal);
            int prefix = payload ? 4 : 0;
            var fields = new List<object>();
            bool fieldsEqual = true;
            foreach (var field in entry.GetProperty("fields").EnumerateArray())
            {
                string key = field.GetProperty("nativeField").GetString();
                int offset = field.GetProperty("managedOffset").GetInt32();
                bool found = native.TryGetValue(key, out uint value);
                int? normalized = found ? checked((int)value) - prefix : null;
                bool equal = normalized == offset;
                fieldsEqual &= equal;
                fields.Add(new { nativeName = key, managedName = field.GetProperty("managedField").GetString(), nativeOffset = found ? (int?)value : null, managedOffset = offset, normalizedOffset = normalized, equal });
            }
            string classification = fieldsEqual && size == nativeSize - prefix ? (payload ? "NORMALIZED_RENDERDATA_PAYLOAD" : "EXACT_FIXED_FIELDS") : "UNRESOLVED";
            if (name == "MilPenData" && fieldsEqual && size == 44 && nativeSize == 48 && alignment == 8 && entry.GetProperty("pack").GetInt32() == 1)
                classification = "MARSHAL_ONLY_FIXED_FIELDS/NATIVE_TAIL_PADDING_DIFFERENCE";
            if (classification == "UNRESOLVED") errors.Add(name + ": unclassified size/offset difference");
            string assembly = entry.GetProperty("assembly").GetString();
            rows.Add(new { nativeName = name, managedType = managed.GetString(), managedAssemblySha256 = Hash(assembly), managedAuthorityKind = entry.GetProperty("authority").GetString(), architecture, pointerSize = IntPtr.Size, runtimeVersion = RuntimeInformation.FrameworkDescription, nativeAlignment = alignment, nativeSize, managedPack = entry.GetProperty("pack").GetInt32(), managedMarshalSize = size, serializedPayloadSize = payload ? (int?)size : null, headerSize = payload ? 8 : 0, nativePrefixSize = prefix, tailPadding = name == "MilPenData" ? 4 : 0, fields, classification, comparisonExecuted = true });
        }
        var enumRows = new List<object>();
        var aliases = new Dictionary<string, string>(StringComparer.Ordinal) { ["MIL_SEGMENT_TYPE"] = "MilSegmentType", ["MIL_PEN_CAP"] = "MilPenCap", ["MIL_PEN_JOIN"] = "MilPenJoin", ["MILRTInitializationFlags"] = "MilRTInitialization" };
        foreach (Type type in local.Where(t => t.IsEnum))
        {
            string name = aliases.GetValueOrDefault(type.Name, type.Name);
            Type underlying = Enum.GetUnderlyingType(type);
            int width = Marshal.SizeOf(underlying);
            var members = new List<object>();
            var missing = new List<string>();
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                string member = field.Name.Contains("FORCE_DWORD", StringComparison.Ordinal) ? "FORCE_DWORD" : field.Name;
                string key = "enum:" + name + "." + member;
                uint bits = unchecked((uint)Convert.ToInt64(field.GetRawConstantValue()));
                bool found = enumValues.TryGetValue(key, out uint actual);
                bool equal = found && actual == bits;
                if (!equal) missing.Add(key);
                members.Add(new { managedName = field.Name, nativeName = member, managedBits = bits, nativeCompilerValue = found ? (uint?)actual : null, equal });
            }
            bool widthEqual = enumValues.TryGetValue("enum:" + name + ".width", out uint nativeWidth) && nativeWidth == width;
            if (!widthEqual || missing.Count != 0) errors.Add(type.FullName + ": enum comparison unresolved");
            enumRows.Add(new { nativeEnumName = name, managedEnumName = type.FullName, underlyingType = underlying.FullName, width, signedness = underlying == typeof(uint) ? "Unsigned" : "Signed", nativeWidth, widthEqual, members, missingNativeNames = missing, comparisonExecuted = true });
        }
        string sourcePath = Path.Combine(root, "src/Microsoft.DotNet.Wpf/src/Common/Graphics/wgx_core_types.cs");
        File.WriteAllText(Path.Combine(archive, "compiler-comparison.json"), JsonSerializer.Serialize(new { architecture, pointerSize = IntPtr.Size, runtimeVersion = RuntimeInformation.FrameworkDescription, dll, dllSha256 = Hash(dll), manifestPath, manifestSha256 = Hash(manifestPath), sourcePath, sourceSha256 = Hash(sourcePath), nativeCompilerDataRead = true, managedComparisonExecuted = true, requiredTypeCount = native.Keys.Count(k => k.EndsWith(".size", StringComparison.Ordinal)), rows, enumRows, nativeEnumValues = enumValues, enumComparisonComplete = errors.Count == 0, closureSatisfied = errors.Count == 0, errors }, new JsonSerializerOptions { WriteIndented = true }));
        return errors;
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}
