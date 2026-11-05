using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static class StrictRenderSupervisor
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

    internal static async Task<int> RunAsync()
    {
        string package = AppContext.BaseDirectory, evidence = Path.Combine(package, "Evidence");
        Directory.CreateDirectory(evidence);
        Save(Path.Combine(evidence, "parent-before.json"), new { pid = Environment.ProcessId, modules = Modules() });
        string official = Path.Combine(package, "Reference", "wpfgfx_cor3.dll"), candidate = Path.Combine(package, "Native", "wpfgfx_cor3.dll");
        string source = File.ReadAllText(Path.Combine(package, "official-source.txt")).Trim();
        using var certificate = X509Certificate.CreateFromSignedFile(official);
        Save(Path.Combine(evidence, "identities.json"), new { official = Identity(official), candidate = Identity(candidate), officialSource = source, signer = certificate.Subject, signerThumbprint = certificate.GetCertHashString(), harness = Identity(typeof(StrictRenderSupervisor).Assembly.Location), runtimeconfig = File.ReadAllText(Path.Combine(package, "WpfGfxShape.ComAcceptance.runtimeconfig.json")), identityMapping = "Each child PID maps to business-process; selected handle maps to selected-gfx. Module observations are samples, not continuous interception." });
        if (!source.Replace('\\', '/').Contains("microsoft.windowsdesktop.app.runtime.win-x64/10.0.13/", StringComparison.OrdinalIgnoreCase) || !certificate.Subject.Contains("Microsoft Corporation", StringComparison.Ordinal)) throw new InvalidDataException("Official source/signature identity mismatch.");
        AcceptancePaths.ValidateCandidateIdentity(candidate);
        Save(Path.Combine(evidence, "package-manifest.json"), Directory.GetFiles(package, "*", SearchOption.AllDirectories).Where(p => !p.StartsWith(evidence, StringComparison.OrdinalIgnoreCase)).Order().Select(p => new { path = Path.GetRelativePath(package, p), length = new FileInfo(p).Length, sha256 = Hash(p) }).ToArray());
        int original = await RunChildAsync(package, official, Path.Combine(evidence, "original")).ConfigureAwait(false);
        int translated = await RunChildAsync(package, candidate, Path.Combine(evidence, "candidate")).ConfigureAwait(false);
        byte[] a = ReadPixels(Path.Combine(evidence, "original", "pixels.pbgra32")), b = ReadPixels(Path.Combine(evidence, "candidate", "pixels.pbgra32"));
        int differences = Math.Abs(a.Length - b.Length), first = -1;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++) if (a[i] != b[i]) { differences++; if (first < 0) first = i; }
        if (first < 0 && a.Length != b.Length) first = Math.Min(a.Length, b.Length);
        using var ar = ReadReceipt(Path.Combine(evidence, "original", "result.json"));
        using var br = ReadReceipt(Path.Combine(evidence, "candidate", "result.json"));
        string[] keys = ["businessEvents", "imageMetadata", "caseId", "consumer", "cleanup", "stage", "failureStage", "exception", "exitCode", "originalAssertionsPassed", "pixelByteCount"];
        var contract = keys.Select(key => new { key, original = Value(ar.RootElement, key), candidate = Value(br.RootElement, key), equal = Value(ar.RootElement, key) == Value(br.RootElement, key) }).ToArray();
        bool moduleGate = ModuleGate(ar.RootElement, official) && ModuleGate(br.RootElement, candidate);
        bool parentGate = !Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(m => m.ModuleName.StartsWith("wpfgfx_", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Equals("PresentationCore.dll", StringComparison.OrdinalIgnoreCase));
        bool passed = original == 0 && translated == 0 && a.Length == 4096 && b.Length == 4096 && differences == 0 && contract.All(c => c.equal) && moduleGate && parentGate;
        Save(Path.Combine(evidence, "comparison.json"), new { passed, moduleGate, parentGate, originalExit = original, candidateExit = translated, originalBytes = a.Length, candidateBytes = b.Length, differingBytes = differences, firstDifference = first, originalByte = first >= 0 && first < a.Length ? (int?)a[first] : null, candidateByte = first >= 0 && first < b.Length ? (int?)b[first] : null, contract, threshold = 0 });
        Save(Path.Combine(evidence, "parent-after.json"), new { pid = Environment.ProcessId, modules = Modules() });
        Console.WriteLine(JsonSerializer.Serialize(new { passed, differences, first, original, translated }));
        return passed ? 0 : 1;
    }

    internal static async Task<int> ObserveAsync()
    {
        string evidence = Path.Combine(AppContext.BaseDirectory, "Evidence");
        Directory.CreateDirectory(evidence);
        Save(Path.Combine(evidence, "observer-before.json"), new { pid = Environment.ProcessId, modules = Modules() });
        using var parent = new Process { StartInfo = new ProcessStartInfo(Environment.ProcessPath!) { WorkingDirectory = AppContext.BaseDirectory, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
        parent.StartInfo.ArgumentList.Add("--strict-render-supervisor");
        parent.Start();
        int pid = parent.Id;
        Task<string> stdout = parent.StandardOutput.ReadToEndAsync(), stderr = parent.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        bool timedOut = false;
        try { await parent.WaitForExitAsync(timeout.Token).ConfigureAwait(false); }
        catch (OperationCanceledException) { timedOut = true; parent.Kill(true); await parent.WaitForExitAsync().ConfigureAwait(false); }
        File.WriteAllText(Path.Combine(evidence, "parent-stdout.txt"), await stdout.ConfigureAwait(false));
        File.WriteAllText(Path.Combine(evidence, "parent-stderr.txt"), await stderr.ConfigureAwait(false));
        Save(Path.Combine(evidence, "parent-process.json"), new { pid, observerPid = Environment.ProcessId, timedOut, reaped = parent.HasExited, exit = parent.ExitCode });
        Save(Path.Combine(evidence, "observer-after.json"), new { pid = Environment.ProcessId, modules = Modules() });
        return timedOut ? 124 : parent.ExitCode;
    }

    internal static int Audit(string packagePath)
    {
        string package = Path.GetFullPath(packagePath, AppContext.BaseDirectory), evidence = Path.Combine(package, "Evidence");
        if (!File.Exists(Path.Combine(evidence, "identities.json")) && File.Exists(Path.Combine(evidence, "Evidence", "identities.json"))) evidence = Path.Combine(evidence, "Evidence");
        using var identities = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, "identities.json")));
        string recordedRoot = Path.GetDirectoryName(identities.RootElement.GetProperty("harness").GetProperty("path").GetString()!)!;
        string Resolve(string path) => path.StartsWith(recordedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? Path.Combine(package, Path.GetRelativePath(recordedRoot, path)) : path;
        object CheckModule(JsonElement module)
        {
            string path = module.GetProperty("path").GetString()!, actual = Resolve(path);
            string expected = module.GetProperty("sha256").GetString()!;
            string? observed = File.Exists(actual) ? Hash(actual) : null;
            return new { path, actual, expected, observed, equal = expected == observed };
        }
        var moduleChecks = new List<object>();
        var checks = new Dictionary<string, bool>();
        using var a = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, "original", "result.json")));
        using var b = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, "candidate", "result.json")));
        byte[] left = File.ReadAllBytes(Path.Combine(evidence, "original", "pixels.pbgra32")), right = File.ReadAllBytes(Path.Combine(evidence, "candidate", "pixels.pbgra32"));
        checks["raw4096Exact"] = left.Length == 4096 && right.Length == 4096 && left.AsSpan().SequenceEqual(right);
        string[] keys = ["businessEvents", "imageMetadata", "caseId", "consumer", "cleanup", "stage", "failureStage", "exception", "exitCode", "originalAssertionsPassed", "pixelByteCount"];
        foreach (string key in keys) checks["contract:" + key] = Value(a.RootElement, key) == Value(b.RootElement, key);
        foreach (string role in new[] { "original", "candidate" })
        {
            JsonElement receipt = role == "original" ? a.RootElement : b.RootElement;
            using var process = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, role, "process.json")));
            JsonElement p = process.RootElement;
            checks[role + ":exit"] = p.GetProperty("exit").GetInt32() == 0 && !p.GetProperty("timeout").GetBoolean() && p.GetProperty("reaped").GetBoolean() && p.GetProperty("drained").GetBoolean() && p.GetProperty("pid").GetInt32() == receipt.GetProperty("pid").GetInt32();
            string selected = Path.Combine(package, role == "original" ? "Reference" : "Native", "wpfgfx_cor3.dll");
            var gfx = receipt.GetProperty("moduleEvidence").EnumerateArray().Where(m => Path.GetFileName(m.GetProperty("path").GetString()!).StartsWith("wpfgfx_", StringComparison.OrdinalIgnoreCase)).ToArray();
            checks[role + ":uniqueGfx"] = gfx.Length == 1 && Resolve(gfx[0].GetProperty("path").GetString()!) == selected && gfx[0].GetProperty("sha256").GetString() == Hash(selected) && gfx[0].GetProperty("address").GetString() == receipt.GetProperty("selectedHandle").GetString() && receipt.GetProperty("nativeBasenameHandleMatches").GetBoolean();
            checks[role + ":harness"] = receipt.GetProperty("harness").GetProperty("sha256").GetString() == Hash(Path.Combine(package, "WpfGfxShape.ComAcceptance.dll"));
            foreach (var module in receipt.GetProperty("moduleEvidence").EnumerateArray()) moduleChecks.Add(CheckModule(module));
        }
        foreach (string name in new[] { "parent-before", "parent-after", "observer-before", "observer-after" })
        {
            string file = Path.Combine(evidence, name + ".json");
            if (!File.Exists(file)) { checks[name] = false; continue; }
            using var snapshot = JsonDocument.Parse(File.ReadAllText(file));
            var modules = snapshot.RootElement.GetProperty("modules").EnumerateArray().ToArray();
            checks[name + ":noWpfGfx"] = !modules.Any(m => Path.GetFileName(m.GetProperty("path").GetString()!).StartsWith("wpfgfx_", StringComparison.OrdinalIgnoreCase) || Path.GetFileName(m.GetProperty("path").GetString()!).Equals("PresentationCore.dll", StringComparison.OrdinalIgnoreCase));
            foreach (var module in modules) moduleChecks.Add(CheckModule(module));
        }
        using var moduleJson = JsonDocument.Parse(JsonSerializer.Serialize(moduleChecks));
        checks["eachModuleHash"] = moduleJson.RootElement.EnumerateArray().All(m => m.GetProperty("equal").GetBoolean());
        string parentFile = Path.Combine(evidence, "parent-process.json");
        checks["parentOsExit"] = false;
        if (File.Exists(parentFile))
        {
            using var parent = JsonDocument.Parse(File.ReadAllText(parentFile));
            using var snapshot = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, "parent-after.json")));
            checks["parentOsExit"] = parent.RootElement.GetProperty("exit").GetInt32() == 0 && !parent.RootElement.GetProperty("timedOut").GetBoolean() && parent.RootElement.GetProperty("reaped").GetBoolean() && parent.RootElement.GetProperty("pid").GetInt32() == snapshot.RootElement.GetProperty("pid").GetInt32();
        }
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, "package-manifest.json")));
        var manifestChecks = manifest.RootElement.EnumerateArray().Select(m => new { path = m.GetProperty("path").GetString()!, sha256 = m.GetProperty("sha256").GetString()! }).ToArray();
        checks["packageManifest"] = manifestChecks.All(m => File.Exists(Path.Combine(package, m.path)) && Hash(Path.Combine(package, m.path)) == m.sha256);
        bool passed = checks.Values.All(v => v);
        Console.WriteLine(JsonSerializer.Serialize(new { passed, checks, originalBytes = left.Length, candidateBytes = right.Length, originalSha256 = Convert.ToHexString(SHA256.HashData(left)), candidateSha256 = Convert.ToHexString(SHA256.HashData(right)), moduleCount = moduleChecks.Count, manifestCount = manifestChecks.Length, moduleFailures = moduleJson.RootElement.EnumerateArray().Where(m => !m.GetProperty("equal").GetBoolean()).Select(m => m.Clone()).ToArray(), boundary = "Read-only evidence audit; module snapshots are not continuous interception; no signature-chain or complete lifecycle certification." }, Options));
        return passed ? 0 : 1;
    }

    private static byte[] ReadPixels(string path) => File.Exists(path) ? File.ReadAllBytes(path) : [];
    private static JsonDocument ReadReceipt(string path) => JsonDocument.Parse(File.Exists(path) ? File.ReadAllText(path) : "{}");
    private static string Value(JsonElement element, string key) => element.TryGetProperty(key, out var value) ? value.GetRawText() : "<absent>";
    private static bool ModuleGate(JsonElement receipt, string selected)
    {
        if (!receipt.TryGetProperty("moduleEvidence", out var modules)) return false;
        var gfx = modules.EnumerateArray().Where(m => Path.GetFileName(m.GetProperty("path").GetString()!).StartsWith("wpfgfx_", StringComparison.OrdinalIgnoreCase)).ToArray();
        return gfx.Length == 1 && gfx[0].GetProperty("path").GetString() == selected && gfx[0].GetProperty("sha256").GetString() == Hash(selected) && gfx[0].GetProperty("address").GetString() == receipt.GetProperty("selectedHandle").GetString() && receipt.GetProperty("nativeBasenameHandleMatches").GetBoolean();
    }

    private static async Task<int> RunChildAsync(string package, string dll, string output)
    {
        Directory.CreateDirectory(output);
        using var child = new Process { StartInfo = new ProcessStartInfo(Path.Combine(package, "WpfGfxShape.ComAcceptance.exe")) { WorkingDirectory = package, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
        foreach (string arg in new[] { "--strict-render-business", dll, "geometry-public-r27", output, Hash(dll) }) child.StartInfo.ArgumentList.Add(arg);
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
