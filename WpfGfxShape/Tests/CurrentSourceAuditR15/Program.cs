using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

string root = AppContext.BaseDirectory;
string output = Path.Combine(root, "Evidence");
Directory.CreateDirectory(output);
if (args is ["geometry-r27"])
{
    var runs = Directory.GetDirectories(Path.Combine(root, "Readback")).OrderBy(p => p, StringComparer.Ordinal).Select(directory =>
    {
        byte[] pixels = File.ReadAllBytes(Path.Combine(directory, "candidate-raw", "pixels.pbgra32"));
        using var result = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "candidate-video.json")));
        var receipt = result.RootElement;
        var samples = new[] { 0, (3 * 32 + 3) * 4 }.Select(offset => new { offset, bytes = Convert.ToHexString(pixels.AsSpan(offset, 4)) }).ToArray();
        var colors = Enumerable.Range(0, pixels.Length / 4).GroupBy(i => Convert.ToHexString(pixels.AsSpan(i * 4, 4))).ToDictionary(g => g.Key, g => g.Count());
        string sha = Convert.ToHexString(SHA256.HashData(pixels));
        var controls = new[] { "missing-digest", "wrong-sha", "wrong-architecture" }.Select(name =>
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, name + ".json")));
            var control = document.RootElement;
            return new { name, stage = control.GetProperty("failureStage").GetString(), requests = control.GetProperty("requests").GetArrayLength(), graphicsModules = control.GetProperty("finalModules").EnumerateArray().Count(m => m.GetString()!.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)), harness = control.GetProperty("harness").Clone(), mvid = control.GetProperty("mvid").Clone() };
        }).ToArray();
        var text = Directory.GetFiles(directory, "*.txt").Where(p => p.EndsWith("stdout.txt", StringComparison.Ordinal) || p.EndsWith("stderr.txt", StringComparison.Ordinal)).ToDictionary(p => Path.GetFileName(p) ?? throw new InvalidDataException(p), File.ReadAllText);
        return new { run = Path.GetFileName(directory), byteCount = pixels.Length, sha, samples, colors,
            passed = pixels.Length == 4096 && samples[0].bytes == "FFFFFFFF" && samples[1].bytes == "0000FFFF" && sha == receipt.GetProperty("pixelsSha256").GetString(),
            reportedStage = receipt.GetProperty("stage").Clone(), harness = receipt.GetProperty("harness").Clone(), mvid = receipt.GetProperty("mvid").Clone(), actualNative = receipt.GetProperty("actualNative").Clone(), presentationCore = receipt.GetProperty("PresentationCore").Clone(), basename = receipt.GetProperty("nativeBasenameHandleMatches").Clone(),
            graphicsModules = receipt.GetProperty("finalModules").EnumerateArray().Where(m => m.GetString()!.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).Select(m => m.GetString()).ToArray(),
            resolverRequests = receipt.GetProperty("requests").GetArrayLength(), controls, text };
    }).ToArray();
    bool passed = runs.Length == 2 && runs.All(run => run.passed && run.controls.All(control => control.requests == 0 && control.graphicsModules == 0));
    File.WriteAllText(Path.Combine(output, "geometry-r27.json"), JsonSerializer.Serialize(new { readOnly = true, nativeLoaded = false, passed, runs }, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine(JsonSerializer.Serialize(new { passed, runs = runs.Length }));
    return passed ? 0 : 1;
}
if (args is ["frames-r22"])
{
    var frames = Directory.EnumerateFiles(Path.Combine(root, "Frames"), "*.pbgra32", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal).Select(path =>
    {
        byte[] bytes = File.ReadAllBytes(path);
        var colors = new Dictionary<string, int>();
        for (int i = 0; i < bytes.Length; i += 4)
        {
            string color = Convert.ToHexString(bytes.AsSpan(i, 4));
            colors[color] = colors.GetValueOrDefault(color) + 1;
        }
        string expected = Path.GetFileName(path).Contains("0.5", StringComparison.Ordinal) ? "0000FFFF" : "FF0000FF";
        return new { path = Path.GetRelativePath(root, path), length = bytes.Length, sha256 = Convert.ToHexString(SHA256.HashData(bytes)), colors, expected, passed = bytes.Length == 2048 && colors.Count == 1 && colors.GetValueOrDefault(expected) == 512 };
    }).ToArray();
    bool passed = frames.Length == 8 && frames.All(frame => frame.passed);
    File.WriteAllText(Path.Combine(output, "frames-r22.json"), JsonSerializer.Serialize(new { readOnly = true, nativeLoaded = false, passed, frames }, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine(JsonSerializer.Serialize(new { passed, count = frames.Length }));
    return passed ? 0 : 1;
}
if (args is ["freeze"])
{
    var snapshot = Directory.EnumerateFiles(Path.Combine(root, "Source"), "*", SearchOption.AllDirectories)
        .OrderBy(p => p, StringComparer.Ordinal)
        .Select(p => new { path = Path.GetRelativePath(Path.Combine(root, "Source"), p).Replace('\\', '/'), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))), length = new FileInfo(p).Length }).ToArray();
    string receipt = string.Join("\n", snapshot.Select(f => $"{f.sha256}  {f.path}")) + "\n";
    File.WriteAllText(Path.Combine(output, "source.sha256"), receipt, new UTF8Encoding(false));
    File.WriteAllText(Path.Combine(output, "freeze.json"), JsonSerializer.Serialize(new { sourceManifestSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(receipt))), sourceFileCount = snapshot.Length, files = snapshot }, new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine(JsonSerializer.Serialize(new { sourceFileCount = snapshot.Length, sourceManifestSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(receipt))) }));
    return 0;
}
var files = Directory.EnumerateFiles(Path.Combine(root, "Source"), "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal)
    .Select(p => new { path = Path.GetRelativePath(root, p).Replace('\\', '/'), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))), length = new FileInfo(p).Length }).ToArray();
string manifest = string.Join("\n", files.Select(f => $"{f.sha256}  {f.path}")) + "\n";
File.WriteAllText(Path.Combine(output, "source.sha256"), manifest, new UTF8Encoding(false));
var comparisons = File.ReadAllLines(Path.Combine(root, "Previous", "source.sha256")).Select(line =>
{
    string previous = line[66..];
    string current = previous.Replace("Source/Production/", "Source/Code/WpfGfxShape/").Replace("Source/Harness/", "Source/Tests/WpfGfxShape.ComAcceptance/").Replace("Source/Build/", "Source/");
    string path = Path.Combine(root, current);
    string? sha = File.Exists(path) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) : null;
    return new { previous, current, previousSha = line[..64], currentSha = sha, equal = sha == line[..64] };
}).ToArray();
string harness = Path.Combine(root, "Previous", "CandidatePureVideo.dll");
using var stream = File.OpenRead(harness);
using var pe = new PEReader(stream);
var metadata = pe.GetMetadataReader();
var locks = metadata.MethodDefinitions.Select(handle =>
{
    var method = metadata.GetMethodDefinition(handle);
    string name = metadata.GetString(method.Name);
    byte[] il = method.RelativeVirtualAddress == 0 ? [] : pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes() ?? [];
    var monitorReferences = metadata.MemberReferences.Where(h =>
    {
        var member = metadata.GetMemberReference(h);
        if (member.Parent.Kind != HandleKind.TypeReference) return false;
        var type = metadata.GetTypeReference((TypeReferenceHandle)member.Parent);
        return metadata.GetString(type.Name) == "Monitor";
    }).Select(h => new { name = metadata.GetString(metadata.GetMemberReference(h).Name), token = MetadataTokens.GetToken(h) })
      .Where(m => Enumerable.Range(0, Math.Max(0, il.Length - 4)).Any(i => il[i] == 0x28 && BitConverter.ToInt32(il, i + 1) == m.token)).ToArray();
    return new { name, monitorReferences };
}).Where(m => m.name is "Checkpoint" or "Resolve").ToArray();
string native = Path.Combine(root, "Previous", "wpfgfx_cor3.dll");
using var nativeStream = File.OpenRead(native);
using var nativePe = new PEReader(nativeStream, PEStreamOptions.LeaveOpen);
nativeStream.Position = 0;
var result = new { sourceManifestSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(manifest))), sourceFileCount = files.Length,
    sourceFiles = files, previousComparisons = comparisons, previousHarnessSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(harness))),
    previousHarnessMvid = metadata.GetGuid(metadata.GetModuleDefinition().Mvid), previousHarnessLockCalls = locks,
    previousNativeSha256 = Convert.ToHexString(SHA256.HashData(nativeStream)), previousNativeLength = nativeStream.Length,
    previousNativeMachine = nativePe.PEHeaders.CoffHeader.Machine.ToString(), previousNativeHasClr = nativePe.PEHeaders.CorHeader is not null };
File.WriteAllText(Path.Combine(output, "audit.json"), JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(JsonSerializer.Serialize(new { result.sourceManifestSha256, result.sourceFileCount, changedPreviousFiles = comparisons.Count(c => !c.equal), result.previousHarnessSha256, locks }));
return 0;
