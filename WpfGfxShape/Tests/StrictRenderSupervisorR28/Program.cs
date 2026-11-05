using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

internal static class Program
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static void Save(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value, Options));
    private static object Identity(string path)
    {
        using var pe = new PEReader(File.OpenRead(path));
        var version = FileVersionInfo.GetVersionInfo(path);
        return new { path, sha256 = Hash(path), length = new FileInfo(path).Length, machine = pe.PEHeaders.CoffHeader.Machine.ToString(), version = version.FileVersion, product = version.ProductVersion };
    }
    private static object[] Modules() => Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Select(m => (object)new { path = m.FileName, address = m.BaseAddress.ToString("X"), sha256 = Hash(m.FileName) }).ToArray();
    private static async Task<int> Main()
    {
        string root = AppContext.BaseDirectory, package = Path.Combine(root, "Package"), evidence = Path.Combine(root, "Evidence");
        Directory.CreateDirectory(evidence);
        Save(Path.Combine(evidence, "parent-before.json"), new { pid = Environment.ProcessId, modules = Modules() });
        string official = Path.Combine(package, "Reference", "wpfgfx_cor3.dll"), candidate = Path.Combine(package, "Native", "wpfgfx_cor3.dll");
        string source = File.ReadAllText(Path.Combine(package, "official-source.txt")).Trim();
        using var certificate = X509Certificate.CreateFromSignedFile(official);
        Save(Path.Combine(evidence, "identities.json"), new { official = Identity(official), candidate = Identity(candidate), officialSource = source, signer = certificate.Subject, signerThumbprint = certificate.GetCertHashString(), harness = Identity(Path.Combine(package, "StrictRenderR28.dll")), parent = Identity(typeof(Program).Assembly.Location), runtimeconfig = File.ReadAllText(Path.Combine(package, "StrictRenderR28.runtimeconfig.json")), identityMapping = "Each child PID maps to business-process; module handle/address maps to selected-gfx. Raw identities retained. RTB creates no application HWND." });
        if (!source.Replace('\\', '/').Contains("microsoft.windowsdesktop.app.runtime.win-x64/10.0.13/", StringComparison.OrdinalIgnoreCase) || !certificate.Subject.Contains("Microsoft Corporation", StringComparison.Ordinal)) throw new InvalidDataException("Official source/signature identity mismatch.");
        Save(Path.Combine(evidence, "package-manifest.json"), Directory.GetFiles(package, "*", SearchOption.AllDirectories).Order().Select(p => new { path = Path.GetRelativePath(package, p), length = new FileInfo(p).Length, sha256 = Hash(p) }).ToArray());
        Save(Path.Combine(evidence, "source-manifest.json"), Directory.GetFiles(Path.Combine(root, "Sources")).Order().Select(p => new { path = Path.GetFileName(p), sha256 = Hash(p) }).ToArray());
        int original = await RunAsync(package, official, Path.Combine(evidence, "original")).ConfigureAwait(false);
        int translated = await RunAsync(package, candidate, Path.Combine(evidence, "candidate")).ConfigureAwait(false);
        string aPath = Path.Combine(evidence, "original", "pixels.pbgra32"), bPath = Path.Combine(evidence, "candidate", "pixels.pbgra32");
        byte[] a = File.Exists(aPath) ? File.ReadAllBytes(aPath) : [], b = File.Exists(bPath) ? File.ReadAllBytes(bPath) : [];
        int differences = Math.Abs(a.Length - b.Length), first = -1;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++) if (a[i] != b[i]) { differences++; if (first < 0) first = i; }
        if (first < 0 && a.Length != b.Length) first = Math.Min(a.Length, b.Length);
        using var ar = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, "original", "result.json")));
        using var br = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, "candidate", "result.json")));
        string[] keys = ["businessEvents", "imageMetadata", "caseId", "consumer", "cleanup", "stage", "failureStage", "exception", "exitCode", "originalAssertionsPassed", "pixelByteCount"];
        var contract = keys.Select(key => new { key, original = Value(ar.RootElement, key), candidate = Value(br.RootElement, key), equal = Value(ar.RootElement, key) == Value(br.RootElement, key) }).ToArray();
        bool ModuleGate(JsonElement receipt, string selected)
        {
            var gfx = receipt.GetProperty("moduleEvidence").EnumerateArray().Where(m => Path.GetFileName(m.GetProperty("path").GetString()!).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).ToArray();
            return gfx.Length == 1 && gfx[0].GetProperty("path").GetString() == selected && gfx[0].GetProperty("sha256").GetString() == Hash(selected) && gfx[0].GetProperty("address").GetString() == receipt.GetProperty("selectedHandle").GetString() && receipt.GetProperty("nativeBasenameHandleMatches").GetBoolean();
        }
        bool moduleGate = ModuleGate(ar.RootElement, official) && ModuleGate(br.RootElement, candidate);
        bool parentGate = !Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(m => m.ModuleName.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Equals("PresentationCore.dll", StringComparison.OrdinalIgnoreCase));
        bool passed = original == 0 && translated == 0 && a.Length == 4096 && b.Length == 4096 && differences == 0 && contract.All(c => c.equal) && moduleGate && parentGate;
        Save(Path.Combine(evidence, "comparison.json"), new { passed, moduleGate, parentGate, originalExit = original, candidateExit = translated, originalBytes = a.Length, candidateBytes = b.Length, differingBytes = differences, firstDifference = first, originalByte = first >= 0 && first < a.Length ? (int?)a[first] : null, candidateByte = first >= 0 && first < b.Length ? (int?)b[first] : null, contract, threshold = 0 });
        Save(Path.Combine(evidence, "parent-after.json"), new { pid = Environment.ProcessId, modules = Modules() });
        Console.WriteLine(JsonSerializer.Serialize(new { passed, differences, first, original, translated }));
        return passed ? 0 : 1;
    }
    private static string Value(JsonElement element, string key) => element.TryGetProperty(key, out var value) ? value.GetRawText() : "<absent>";
    private static async Task<int> RunAsync(string package, string dll, string output)
    {
        Directory.CreateDirectory(output);
        using var child = new Process { StartInfo = new ProcessStartInfo(Path.Combine(package, "StrictRenderR28.exe")) { WorkingDirectory = package, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
        foreach (string arg in new[] { dll, "geometry-public-r27", output, Hash(dll) }) child.StartInfo.ArgumentList.Add(arg);
        child.Start();
        int pid = child.Id;
        Task<string> stdout = child.StandardOutput.ReadToEndAsync(), stderr = child.StandardError.ReadToEndAsync();
        bool timeout = !child.WaitForExit(120000);
        string? killError = null;
        if (timeout) { try { child.Kill(true); } catch (InvalidOperationException ex) { killError = ex.ToString(); } catch (System.ComponentModel.Win32Exception ex) { killError = ex.ToString(); } }
        bool reaped = child.WaitForExit(10000);
        bool drained = Task.WaitAll([stdout, stderr], 10000);
        if (drained) { File.WriteAllText(Path.Combine(output, "stdout.txt"), await stdout.ConfigureAwait(false)); File.WriteAllText(Path.Combine(output, "stderr.txt"), await stderr.ConfigureAwait(false)); }
        int exit = reaped ? child.ExitCode : -1;
        Save(Path.Combine(output, "process.json"), new { pid, parentPid = Environment.ProcessId, selected = dll, timeout, reaped, drained, killError, exit });
        return timeout || !reaped || !drained || killError is not null ? -1 : exit;
    }
}
