using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;

string root = AppContext.BaseDirectory;
string evidence = Path.Combine(root, "Evidence");
Directory.CreateDirectory(evidence);
if (args.Length == 0 || args is ["rtb-lifecycle"])
{
    string mode = args.Length == 0 ? "run" : "run-rtb-lifecycle";
    return Execute(Path.Combine(root, "CandidateResizeSupervisor.exe"), root, [mode], "supervisor", 180000);
}
if (args.Length != 1 || args[0] is not ("run" or "run-rtb-lifecycle")) return 64;
bool rtbLifecycle = args[0] == "run-rtb-lifecycle";
string package = Path.Combine(root, "Package");
string native = Path.Combine(package, "Native", "wpfgfx_cor3.dll");
string digest = Path.Combine(package, "aot.sha256");
byte[] original = File.ReadAllBytes(native);
string sha = Convert.ToHexString(SHA256.HashData(original));
string savedDigest = File.ReadAllText(digest);
if (sha != "2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE" || sha != savedDigest.Trim() || File.Exists(Path.Combine(package, "wpfgfx_cor3.dll"))) return 65;
string harness = Path.Combine(package, "CandidateResize.dll");
string harnessSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(harness)));
using var stream = File.OpenRead(harness);
using var pe = new PEReader(stream);
var reader = pe.GetMetadataReader();
string mvid = reader.GetGuid(reader.GetModuleDefinition().Mvid).ToString();
File.WriteAllText(Path.Combine(evidence, "frozen-identity.json"), JsonSerializer.Serialize(new { nativeSha = sha, harnessSha, mvid }));
foreach (string test in new[] { "original", "full", "compare", "missingdigest", "wrongsha", "wrongarch" })
{
    string output = Path.Combine(package, "Evidence");
    if (Directory.Exists(output)) Directory.Delete(output, true);
    try
    {
        if (test == "missingdigest") File.Delete(digest);
        if (test == "wrongsha") File.WriteAllText(digest, new string('0', 64));
        if (test == "wrongarch")
        {
            byte[] altered = (byte[])original.Clone();
            int offset = BitConverter.ToInt32(altered, 0x3c) + 4;
            altered[offset] = 0x4c; altered[offset + 1] = 0x01;
            File.WriteAllBytes(native, altered);
            File.WriteAllText(digest, Convert.ToHexString(SHA256.HashData(altered)));
        }
        bool role = test is "original" or "full" or "compare";
        int exit = Execute(Path.Combine(package, "CandidateResize.exe"), package, [role ? test : "candidate", "resize"], test, 10000);
        string result = File.ReadAllText(Path.Combine(output, "result.json"));
        File.WriteAllText(Path.Combine(evidence, test + "-result.json"), result);
        using var document = JsonDocument.Parse(result);
        var receipt = document.RootElement;
        bool noNative = !receipt.GetProperty("finalModules").EnumerateArray().Any(x => x.GetString()!.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase));
        bool noRequests = !receipt.TryGetProperty("requests", out var requests) || requests.GetArrayLength() == 0;
        string expectedStage = test switch { "missingdigest" => "digest-required", "wrongsha" => "sha-validation", "wrongarch" => "pe-architecture-validation", _ => "bootstrap" };
        bool passed = exit == (role ? 64 : 1) && noNative && noRequests && receipt.GetProperty("stage").GetString() == expectedStage;
        File.WriteAllText(Path.Combine(evidence, test + "-assertions.json"), JsonSerializer.Serialize(new { passed, exit, noNative, noRequests, expectedStage }));
        if (!passed) return 1;
    }
    finally { File.WriteAllBytes(native, original); File.WriteAllText(digest, savedDigest); }
}
Directory.Delete(Path.Combine(package, "Evidence"), true);
return (rtbLifecycle ? RtbLifecycleRun.Run(package, evidence, sha, harnessSha, mvid) : ResizeRun.Run(package, evidence, sha, harnessSha, mvid)) ? 0 : 1;

int Execute(string executable, string workingDirectory, string[] arguments, string name, int milliseconds)
{
    using var child = new Process { StartInfo = new ProcessStartInfo(executable) { WorkingDirectory = workingDirectory, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
    foreach (string argument in arguments) child.StartInfo.ArgumentList.Add(argument);
    child.Start();
    var stdout = child.StandardOutput.ReadToEndAsync();
    var stderr = child.StandardError.ReadToEndAsync();
    bool timeout = !child.WaitForExit(milliseconds);
    string? killError = null;
    if (timeout)
    {
        try { child.Kill(true); }
        catch (InvalidOperationException error) { killError = error.ToString(); }
        catch (System.ComponentModel.Win32Exception error) { killError = error.ToString(); }
    }
    bool reaped = child.WaitForExit(10000);
    bool drained = Task.WaitAll([stdout, stderr], 10000);
    if (drained)
    {
        File.WriteAllText(Path.Combine(evidence, name + ".stdout.txt"), stdout.Result);
        File.WriteAllText(Path.Combine(evidence, name + ".stderr.txt"), stderr.Result);
    }
    int? exitCode = reaped ? child.ExitCode : null;
    File.WriteAllText(Path.Combine(evidence, name + "-exit.json"), JsonSerializer.Serialize(new { pid = child.Id, timeout, reaped, drained, killError, exitCode }));
    return !timeout && reaped && drained && killError is null ? exitCode!.Value : 124;
}
