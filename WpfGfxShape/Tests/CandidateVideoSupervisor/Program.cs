using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

string root = AppContext.BaseDirectory;
string package = Path.Combine(root, "Package");
string evidence = Path.Combine(root, "Evidence");
Directory.CreateDirectory(evidence);
var options = new JsonSerializerOptions { WriteIndented = true };
if (args.Length == 1 && args[0] == "verify-supervisor") return SupervisorProbe.Run(root);
if (args.Length == 1 && args[0] == "verify-video") return SupervisorProbe.Run(root, videoOnly: true);
if (args.Length > 1 || (args.Length == 1 && args[0] != "audit-only" && args[0] != "verify-failure-exit")) return 64;
ReadOnlyAudit.Write(package, evidence);
InitializationClosure.Write(package, evidence);
var audit = new List<object>();
foreach (string path in Directory.EnumerateFiles(package, "*.dll", SearchOption.AllDirectories))
{
    using var file = File.OpenRead(path);
    using var pe = new PEReader(file);
    var imports = new List<object>();
    if (pe.HasMetadata)
    {
        var reader = pe.GetMetadataReader();
        foreach (var handle in reader.MethodDefinitions)
        {
            var method = reader.GetMethodDefinition(handle);
            var import = method.GetImport();
            if (!import.Module.IsNil) imports.Add(new { method = reader.GetString(method.Name), entry = reader.GetString(import.Name), library = reader.GetString(reader.GetModuleReference(import.Module).Name) });
        }
    }
    file.Position = 0;
    string sha = Convert.ToHexString(SHA256.HashData(file));
    byte[] bytes = File.ReadAllBytes(path);
    string ascii = Encoding.ASCII.GetString(bytes);
    string unicode = Encoding.Unicode.GetString(bytes);
    audit.Add(new { path = Path.GetRelativePath(package, path), sha256 = sha, machine = pe.PEHeaders.CoffHeader.Machine.ToString(), managed = pe.HasMetadata, imports,
        containsGfxName = ascii.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase) || unicode.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase),
        containsLoadLibrary = ascii.Contains("LoadLibrary", StringComparison.Ordinal) || unicode.Contains("LoadLibrary", StringComparison.Ordinal) });
}
File.WriteAllText(Path.Combine(evidence, "runtime-audit.json"), JsonSerializer.Serialize(audit, options));
using (var document = JsonDocument.Parse(JsonSerializer.Serialize(audit)))
{
    var summary = document.RootElement.EnumerateArray().Where(item => item.GetProperty("path").GetString()!.Contains("Presentation", StringComparison.OrdinalIgnoreCase) || item.GetProperty("path").GetString()!.Contains("WindowsBase", StringComparison.OrdinalIgnoreCase) || item.GetProperty("path").GetString()!.Contains("DirectWrite", StringComparison.OrdinalIgnoreCase) || item.GetProperty("path").GetString()!.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase))
        .Select(item => new { path = item.GetProperty("path").GetString(), sha256 = item.GetProperty("sha256").GetString(), item = item.GetProperty("containsLoadLibrary").GetBoolean(), graphicsImports = item.GetProperty("imports").EnumerateArray().Where(import => import.GetProperty("library").GetString()!.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).Select(import => import.Clone()).ToArray() }).ToArray();
    File.WriteAllText(Path.Combine(evidence, "runtime-summary.json"), JsonSerializer.Serialize(summary, options));
}
if (args.Length == 1 && args[0] == "audit-only") return 0;
string candidatePath = Path.Combine(package, "Native", "wpfgfx_cor3.dll");
string candidateSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(candidatePath)));
if (!candidateSha.Equals(File.ReadAllText(Path.Combine(package, "aot.sha256")).Trim(), StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Package digest mismatch.");
string[] gfxFiles = Directory.GetFiles(package, "*", SearchOption.AllDirectories).Where(p => Path.GetFileName(p).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).ToArray();
if (gfxFiles.Length != 1 || !Path.GetFullPath(gfxFiles[0]).Equals(Path.GetFullPath(candidatePath), StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unexpected graphics package member.");
string harnessSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(package, "CandidatePureVideo.dll"))));
using var harnessPe = new PEReader(File.OpenRead(Path.Combine(package, "CandidatePureVideo.dll")));
var harnessMetadata = harnessPe.GetMetadataReader();
string harnessMvid = harnessMetadata.GetGuid(harnessMetadata.GetModuleDefinition().Mvid).ToString();
File.WriteAllText(Path.Combine(evidence, "package-identity.json"), JsonSerializer.Serialize(new { candidateSha, harnessSha, harnessMvid, gfxFiles }, options));
var results = new List<object>();
bool allPassed = true;
foreach (string control in new[] { "missing-digest", "wrong-sha", "wrong-architecture" })
{
    string directory = Path.Combine(root, control);
    Directory.CreateDirectory(directory);
    foreach (string path in Directory.EnumerateFiles(package, "*", SearchOption.AllDirectories))
    {
        string destination = Path.Combine(directory, Path.GetRelativePath(package, path));
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(path, destination, true);
    }
    string digest = Path.Combine(directory, "aot.sha256");
    if (control == "missing-digest") File.Delete(digest);
    if (control == "wrong-sha") File.WriteAllText(digest, new string('0', 64));
    if (control == "wrong-architecture")
    {
        string candidate = Path.Combine(directory, "Native", "wpfgfx_cor3.dll");
        byte[] bytes = File.ReadAllBytes(candidate);
        int header = BitConverter.ToInt32(bytes, 0x3c);
        ushort machine = BitConverter.ToUInt16(bytes, header + 4);
        ushort wrongMachine = machine == (ushort)Machine.I386 ? (ushort)Machine.Amd64 : (ushort)Machine.I386;
        bytes[header + 4] = (byte)wrongMachine; bytes[header + 5] = (byte)(wrongMachine >> 8);
        File.WriteAllBytes(candidate, bytes);
        File.WriteAllText(digest, Convert.ToHexString(SHA256.HashData(bytes)));
    }
    using var child = new Process { StartInfo = new ProcessStartInfo(Path.Combine(directory, "CandidatePureVideo.exe")) { WorkingDirectory = directory, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
    child.StartInfo.ArgumentList.Add("candidate"); child.StartInfo.ArgumentList.Add(SceneSelection.Name);
    child.Start();
    int pid = child.Id;
    Task<string> stdout = child.StandardOutput.ReadToEndAsync(), stderr = child.StandardError.ReadToEndAsync();
    bool timeout = !child.WaitForExit(30000);
    string? killError = null;
    if (timeout)
    {
        try { child.Kill(true); }
        catch (InvalidOperationException error) { killError = error.ToString(); }
        catch (System.ComponentModel.Win32Exception error) { killError = error.ToString(); }
    }
    bool reaped = child.WaitForExit(10000);
    bool drained = Task.WaitAll(new Task[] { stdout, stderr }, 10000);
    if (drained)
    {
        File.WriteAllText(Path.Combine(evidence, control + ".stdout.txt"), stdout.Result);
        File.WriteAllText(Path.Combine(evidence, control + ".stderr.txt"), stderr.Result);
    }
    string result = Path.Combine(directory, "Evidence", "result.json");
    if (File.Exists(result)) File.Copy(result, Path.Combine(evidence, control + ".json"), true);
    var failures = new List<string>();
    if (timeout) failures.Add("execution-timeout");
    if (!reaped) failures.Add("reap-incomplete");
    if (!drained) failures.Add("drain-incomplete");
    if (killError is not null) failures.Add("kill-error: " + killError);
    if (!reaped || child.ExitCode != 1) failures.Add("unexpected-process-exit");
    if (!File.Exists(result)) failures.Add("missing-result");
    else
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(result));
            var receipt = document.RootElement;
            string expectedStage = control switch { "missing-digest" => "digest-required", "wrong-sha" => "sha-validation", _ => "pe-architecture-validation" };
            string expectedException = control switch { "missing-digest" => "System.IO.FileNotFoundException:", "wrong-sha" => "System.IO.InvalidDataException:", _ => "System.BadImageFormatException:" };
            if (args.Length == 1 && args[0] == "verify-failure-exit" && control == "missing-digest") expectedStage = "intentional-wrong-stage-probe";
            if (receipt.GetProperty("failureStage").GetString() != expectedStage) failures.Add("wrong-failure-stage");
            if (receipt.GetProperty("stage").GetString() != expectedStage) failures.Add("wrong-final-stage");
            if (receipt.GetProperty("exitCode").GetInt32() != 1) failures.Add("wrong-reported-exit");
            if (!receipt.GetProperty("exception").GetString()!.StartsWith(expectedException, StringComparison.Ordinal)) failures.Add("wrong-exception");
            foreach (string snapshot in new[] { "baseline", "finalModules" })
            {
                var modules = receipt.GetProperty(snapshot).EnumerateArray().Select(p => p.GetString()!).ToArray();
                if (modules.Length == 0 || modules.Any(p => Path.GetFileName(p).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase))) failures.Add(snapshot + "-graphics-contamination");
            }
            if (receipt.GetProperty("requests").GetArrayLength() != 0) failures.Add("unexpected-resolver-request");
            if (receipt.TryGetProperty("candidate", out _) || receipt.TryGetProperty("preSceneModules", out _) || receipt.TryGetProperty("caseId", out _)) failures.Add("business-boundary-crossed");
            if (receipt.GetProperty("harness").GetProperty("sha256").GetString() != harnessSha || receipt.GetProperty("mvid").GetString() != harnessMvid) failures.Add("harness-identity-mismatch");
        }
        catch (Exception error) when (error is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            failures.Add("invalid-receipt: " + error.Message);
        }
    }
    bool passed = failures.Count == 0;
    allPassed &= passed;
    results.Add(new { control, pid, timeout, reaped, drained, killError, exitCode = reaped ? (int?)child.ExitCode : null, passed, failures });
}
File.WriteAllText(Path.Combine(evidence, "process-results.json"), JsonSerializer.Serialize(results, options));
Console.WriteLine(JsonSerializer.Serialize(results, options));
if (allPassed && args.Length == 0) allPassed = VideoRun.Run(package, evidence, candidateSha, harnessSha, harnessMvid);
return allPassed ? 0 : 1;
