using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class NativeCompilerLayoutTests
{
    private static string Root => typeof(NativeCompilerLayoutTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint ReadLayout(uint index);

    [DataTestMethod]
    [DataRow("Win32", 4, 0x14c)]
    [DataRow("x64", 8, 0x8664)]
    public void WhenOriginalHeadersAreCompiledThenPeExportContainsMeasuredLayout(string architecture, int pointerSize, int machine)
    {
        string dll = Path.Combine(Root, "WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/artifacts", architecture, "Stage4NativeLayout.dll");
        byte[] bytes = File.ReadAllBytes(dll);
        using var pe = new PEReader(new MemoryStream(bytes));
        Assert.AreEqual(machine, (int)pe.PEHeaders.CoffHeader.Machine);
        int Offset(int rva)
        {
            var section = pe.PEHeaders.SectionHeaders.Single(s => rva >= s.VirtualAddress && rva < s.VirtualAddress + Math.Max(s.VirtualSize, s.SizeOfRawData));
            return section.PointerToRawData + rva - section.VirtualAddress;
        }
        uint U32(int offset) => BitConverter.ToUInt32(bytes, offset);
        int exports = Offset(pe.PEHeaders.PEHeader!.ExportTableDirectory.RelativeVirtualAddress);
        int names = Offset((int)U32(exports + 32));
        int ordinals = Offset((int)U32(exports + 36));
        int functions = Offset((int)U32(exports + 28));
        int data = -1;
        for (int i = 0; i < U32(exports + 24); i++)
        {
            int name = Offset((int)U32(names + 4 * i));
            string text = System.Text.Encoding.ASCII.GetString(bytes, name, Array.IndexOf(bytes, (byte)0, name) - name);
            if (text.TrimStart('_') == "Stage4Layout")
                data = Offset((int)U32(functions + 4 * BitConverter.ToUInt16(bytes, ordinals + 2 * i)));
        }
        Assert.IsGreaterThanOrEqualTo(0, data);
        uint[] actual = Enumerable.Range(0, 26).Select(i => U32(data + i * 4)).ToArray();
        uint p = (uint)pointerSize;
        uint[] expected = [0x5347344c, p, 20, 4, 0, 4, 8, 12, 16, 4, 4, 4, p, p, p, p, p, p, 16, 4, 0, 4, 8, 12, 0x200184c0, 0x0bddcb2b];
        CollectionAssert.AreEqual(expected, actual);
        bool runtimeExecuted = RuntimeInformation.ProcessArchitecture.ToString().Equals(architecture, StringComparison.OrdinalIgnoreCase)
            || architecture == "Win32" && RuntimeInformation.ProcessArchitecture == Architecture.X86;
        if (runtimeExecuted)
        {
            nint handle = NativeLibrary.Load(dll);
            // Native diagnostic modules remain resident for the lifetime of the test process.
            var read = Marshal.GetDelegateForFunctionPointer<ReadLayout>(NativeLibrary.GetExport(handle, "Stage4ReadLayout"));
            CollectionAssert.AreEqual(actual, Enumerable.Range(0, 26).Select(i => read((uint)i)).ToArray());
        }
        string archive = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence", "native-compiler-" + architecture + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        string report = Path.Combine(archive, "report.json");
        string[] sources = ["WpfGfxShape/Tests/GenProtoNativeLayoutDiagnostics/NativeLayout.cpp", "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h", "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_av_types.h", "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wincodec_private.h", "src/Microsoft.DotNet.Wpf/src/WpfGfx/include/Generated/wincodec_private_generated.h"];
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            schemaVersion = "1.0.0", architecture, machine, hostArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            nativeCompilerExecuted = true, nativeRuntimeExecuted = runtimeExecuted, measurement = "Independent PE export RVA parsing; matching host also invokes native export",
            status = "PassedMeasuredSubset", completeCoreRequiredTypes = false, dll = Path.GetRelativePath(Root, dll), dllSha256 = Convert.ToHexString(SHA256.HashData(bytes)),
            values = actual, AVEventData = new { size = actual[2], alignment = actual[3], offsets = actual[4..9], payloadStart = actual[8], managedCounterpart = "Absent: CPP_ONLY" },
            sources = sources.Select(path => new { path, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root, path)))) })
        }, new JsonSerializerOptions { WriteIndented = true }));
        using var readback = JsonDocument.Parse(File.ReadAllText(report));
        Console.WriteLine("Native compiler layout report: " + report + " SHA256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(report))));
    }
}
