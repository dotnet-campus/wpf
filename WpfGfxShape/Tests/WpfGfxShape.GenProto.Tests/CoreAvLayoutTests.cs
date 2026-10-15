using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfGfxShape.GenProto.Tests;

[TestClass]
public class CoreAvLayoutTests
{
    private static string Root => typeof(CoreAvLayoutTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "RepositoryRoot").Value!;

    [DataTestMethod]
    [DataRow("X86", 4)]
    [DataRow("X64", 8)]
    [DataRow("Arm64", 8)]
    public void WhenAvDeclarationIsModeledThenCountedPayloadAndPointerRulesAreExplicit(string architecture, int pointerSize)
    {
        string include = Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/include");
        string av = File.ReadAllText(Path.Combine(include, "wgx_av_types.w"));
        string body = Regex.Match(av, @"struct __declspec\(align\(4\)\) AVEventData\s*\{([^}]+)\}", RegexOptions.Singleline).Groups[1].Value;
        var sizes = new Dictionary<string, int> { ["DWORD"] = 4, ["HRESULT"] = 4, ["ULONG"] = 4, ["WCHAR"] = 2 };
        int offset = 0;
        var fields = new List<object>();
        var offsets = new List<int>();
        foreach (Match field in Regex.Matches(body, @"(\w+)\s+(\w+)(?:\[(\d+)\])?\s*;"))
        {
            string type = field.Groups[1].Value;
            if (!sizes.TryGetValue(type, out int size)) throw new InvalidDataException("Unknown native type: " + type);
            int count = field.Groups[3].Success ? int.Parse(field.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture) : 1;
            int alignment = Math.Min(size, 4);
            offset = (offset + alignment - 1) / alignment * alignment;
            offsets.Add(offset);
            fields.Add(new { name = field.Groups[2].Value, type, count, offset, size = size * count, alignment });
            offset += size * count;
        }
        int structSize = (offset + 3) / 4 * 4;
        string core = File.ReadAllText(Path.Combine(include, "wgx_core_types.w"));
        string consumer = Path.Combine(Root, "src/Microsoft.DotNet.Wpf/src/WpfGfx/core/av/mediaeventproxy.cpp");
        string consumerText = File.ReadAllText(consumer);
        string[] pointerDeclarations = ["DECLARE_MIL_HANDLE(MIL_CHANNEL);", "typedef void *HMIL_PLAYER;", "typedef class CMilConnectionManager *HMIL_CONNECTIONMANAGER;", "typedef interface IMilCommandTransport *HMIL_COMMANDTRANSPORT;", "typedef class CMilCrossThreadTransport *HMIL_CROSSTHREADCOMMANDTRANSPORT;", "DECLARE_MIL_HANDLE(HMIL_CONNECTION);"];
        var errors = new List<string>();
        if (!offsets.SequenceEqual(new[] { 0, 4, 8, 12, 16 }) || structSize != 20) errors.Add("AVEventData field or size mismatch");
        foreach (string declaration in pointerDeclarations)
            if (!core.Contains(declaration, StringComparison.Ordinal)) errors.Add("Pointer declaration absent: " + declaration);
        if (!core.Contains("typedef UINT32 HMIL_OBJECT;", StringComparison.Ordinal) || !core.Contains("typedef HMIL_OBJECT HMIL_CHANNEL;", StringComparison.Ordinal)) errors.Add("UINT32 handle authority absent");
        if (!consumerText.Contains("SizeTAdd(stringSize, sizeof(AVEventData), &totalSize)", StringComparison.Ordinal) || !consumerText.Contains("pEventData->typeAndParamStrings", StringComparison.Ordinal)) errors.Add("Counted payload consumer changed");
        if (File.ReadAllText(Path.Combine(include, "processed/wgx_av_types.cs")).Contains("AVEventData", StringComparison.Ordinal)) errors.Add("Unexpected managed counterpart");
        string archive = Path.Combine(Root, "WpfGfxShape/Investigations/GenProtoEvidence", "layout-model-" + architecture + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(archive);
        string[] authorityFiles = [Path.Combine(include, "wgx_av_types.w"), Path.Combine(include, "wgx_core_types.w"), Path.Combine(include, "processed/wgx_av_types.h"), Path.Combine(include, "processed/wgx_av_types.cs"), consumer];
        string report = Path.Combine(archive, "report.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            schemaVersion = "1.0.0", kind = "IndependentNativeDeclarationModel", executed = true,
            actualArchitecture = RuntimeInformation.ProcessArchitecture.ToString(), targetArchitecture = architecture,
            nativeLayoutExecuted = false, managedCounterpart = "Absent: AVEventData is CPP_ONLY", status = "BlockedMissingNativeLayout",
            authorityFiles = authorityFiles.Select(p => new { path = Path.GetRelativePath(Root, p).Replace('\\', '/'), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))) }),
            AVEventData = new { fields, size = structSize, alignment = 4, variablePayloadStart = 16, allocationRule = "20 + 2 * (typeLength + paramLength)", lengthUnit = "UTF16 code units", expectedOrigin = "wgx_av_types.w:76-83; mediaeventproxy.cpp:297-377; model, not MSVC observation" },
            pointerRules = pointerDeclarations.Select(d => new { sourceDeclaration = d, size = pointerSize, alignment = pointerSize, packContext = "Declaration outside protocol #pragma pack(push,1); Windows target model, native compile evidence missing" }),
            fixedHandles = new { HMIL_OBJECT = 4, HMIL_RESOURCE = 4, HMIL_CHANNEL = 4 },
            protocolPackingSource = "wgx_core_types.w:336-348; Generated/wgx_misc.h has local push/pop contexts; do not apply protocol pack=1 to all declarations",
            errors
        }, new JsonSerializerOptions { WriteIndented = true }));
        using var readback = JsonDocument.Parse(File.ReadAllText(report));
        Console.WriteLine("Layout model: " + report + " SHA256=" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(report))));
        Assert.HasCount(0, errors, string.Join(Environment.NewLine, errors));
    }
}
