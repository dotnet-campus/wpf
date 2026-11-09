using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
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

    private static string RuntimeId => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X86 => "win-x86",
        Architecture.X64 => "win-x64",
        _ => throw new PlatformNotSupportedException()
    };
    private static string selectedCase = StrictRenderCase.Rectangle;
    private static readonly string[] ContractKeys = ["businessEvents", "imageMetadata", "caseId", "sceneInput", "consumer", "cleanup", "stage", "failureStage", "exception", "exitCode", "originalAssertionsPassed", "pixelByteCount", "mediaAssets", "mediaOperations", "mediaEvents", "mediaContractPassed", "mediaLifecycle", "callbackDrain"];

    internal static async Task<int> RunAsync(string scene)
    {
        if (!StrictRenderCase.IsKnown(scene)) return 64;
        selectedCase = scene;
        if (scene is StrictRenderCase.MediaAudioInitializeClose or StrictRenderCase.MediaAudioSeek) StrictAudioInput.Prepare();
        string package = AppContext.BaseDirectory, evidence = Path.Combine(package, "Evidence");
        Directory.CreateDirectory(evidence);
        Save(Path.Combine(evidence, "parent-before.json"), new { pid = Environment.ProcessId, modules = Modules() });
        string official = Path.Combine(package, "Reference", "wpfgfx_cor3.dll"), candidate = Path.Combine(package, "Native", "wpfgfx_cor3.dll");
        string source = File.ReadAllText(Path.Combine(package, "official-source.txt")).Trim();
        using var certificate = X509Certificate.CreateFromSignedFile(official);
        Save(Path.Combine(evidence, "identities.json"), new { official = Identity(official), candidate = Identity(candidate), officialSource = source, signer = certificate.Subject, signerThumbprint = certificate.GetCertHashString(), harness = Identity(typeof(StrictRenderSupervisor).Assembly.Location), runtimeconfig = File.ReadAllText(Path.Combine(package, "WpfGfxShape.ComAcceptance.runtimeconfig.json")), identityMapping = "Each child PID maps to business-process; selected handle maps to selected-gfx. Module observations are samples, not continuous interception." });
        if (!source.Replace('\\', '/').Contains($"microsoft.windowsdesktop.app.runtime.{RuntimeId}/10.0.13/runtimes/{RuntimeId}/native/wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) || !certificate.Subject.Contains("Microsoft Corporation", StringComparison.Ordinal)) throw new InvalidDataException("Official source/signature identity mismatch.");
        using (var officialStream = File.OpenRead(official)) AcceptancePaths.ValidateIdentity(officialStream, Hash(official));
        AcceptancePaths.ValidateCandidateIdentity(candidate);
        Save(Path.Combine(evidence, "package-manifest.json"), Directory.GetFiles(package, "*", SearchOption.AllDirectories).Where(p => !p.StartsWith(evidence, StringComparison.OrdinalIgnoreCase)).Order().Select(p => new { path = Path.GetRelativePath(package, p), length = new FileInfo(p).Length, sha256 = Hash(p) }).ToArray());
        int original = await RunChildAsync(package, official, Path.Combine(evidence, "original")).ConfigureAwait(false);
        int repeat = await RunChildAsync(package, official, Path.Combine(evidence, "original-repeat")).ConfigureAwait(false);
        int translated = await RunChildAsync(package, candidate, Path.Combine(evidence, "candidate")).ConfigureAwait(false);
        byte[] a = ReadPixels(Path.Combine(evidence, "original", "pixels.pbgra32")), b = ReadPixels(Path.Combine(evidence, "candidate", "pixels.pbgra32"));
        int differences = Math.Abs(a.Length - b.Length), first = -1;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++) if (a[i] != b[i]) { differences++; if (first < 0) first = i; }
        if (first < 0 && a.Length != b.Length) first = Math.Min(a.Length, b.Length);
        using var ar = ReadReceipt(Path.Combine(evidence, "original", "result.json"));
        using var br = ReadReceipt(Path.Combine(evidence, "candidate", "result.json"));
        using var rr = ReadReceipt(Path.Combine(evidence, "original-repeat", "result.json"));
        byte[] repeated = ReadPixels(Path.Combine(evidence, "original-repeat", "pixels.pbgra32"));
        bool oracleStable = repeat == 0 && a.AsSpan().SequenceEqual(repeated) && ContractKeys.All(key => Value(ar.RootElement, key) == Value(rr.RootElement, key)) && ModuleGate(rr.RootElement, official);
        Save(Path.Combine(evidence, "oracle-stability.json"), new { oracleStable, repeatExit = repeat, bytes = repeated.Length, sha256 = Convert.ToHexString(SHA256.HashData(repeated)) });
        var contract = ContractKeys.Select(key => new { key, original = Value(ar.RootElement, key), candidate = Value(br.RootElement, key), equal = Value(ar.RootElement, key) == Value(br.RootElement, key) }).ToArray();
        bool moduleGate = ModuleGate(ar.RootElement, official) && ModuleGate(br.RootElement, candidate);
        bool parentGate = !Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(m => m.ModuleName.StartsWith("wpfgfx_", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Equals("PresentationCore.dll", StringComparison.OrdinalIgnoreCase));
        bool metadataGate = StrictRenderCase.MetadataMatches(ar.RootElement, scene) && StrictRenderCase.MetadataMatches(br.RootElement, scene) && StrictRenderCase.MetadataMatches(rr.RootElement, scene);
        bool passed = original == 0 && translated == 0 && oracleStable && metadataGate && a.Length == ExpectedBytes(ar.RootElement, scene) && b.Length == ExpectedBytes(br.RootElement, scene) && differences == 0 && contract.All(c => c.equal) && moduleGate && parentGate;
        Save(Path.Combine(evidence, "comparison.json"), new { passed, scene, oracleStable, metadataGate, moduleGate, parentGate, originalExit = original, candidateExit = translated, originalBytes = a.Length, candidateBytes = b.Length, pixelComparisonApplicable = !StrictRenderCase.IsMedia(scene), differingBytes = StrictRenderCase.IsMedia(scene) ? (int?)null : differences, firstDifference = StrictRenderCase.IsMedia(scene) ? (int?)null : first, originalByte = first >= 0 && first < a.Length ? (int?)a[first] : null, candidateByte = first >= 0 && first < b.Length ? (int?)b[first] : null, contract, threshold = 0 });
        Save(Path.Combine(evidence, "parent-after.json"), new { pid = Environment.ProcessId, modules = Modules() });
        Console.WriteLine(JsonSerializer.Serialize(new { passed, differences, first, original, translated }));
        return passed ? 0 : 1;
    }

    internal static async Task<int> ObserveAsync(string scene)
    {
        if (!StrictRenderCase.IsKnown(scene)) return 64;
        string evidence = Path.Combine(AppContext.BaseDirectory, "Evidence");
        Directory.CreateDirectory(evidence);
        Save(Path.Combine(evidence, "observer-before.json"), new { pid = Environment.ProcessId, modules = Modules() });
        using var parent = new Process { StartInfo = new ProcessStartInfo(Environment.ProcessPath!) { WorkingDirectory = AppContext.BaseDirectory, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
        parent.StartInfo.ArgumentList.Add("--strict-render-supervisor");
        parent.StartInfo.ArgumentList.Add(scene);
        parent.Start();
        int pid = parent.Id;
        Task<string> stdout = parent.StandardOutput.ReadToEndAsync(), stderr = parent.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        bool timedOut = false;
        try { await parent.WaitForExitAsync(timeout.Token).ConfigureAwait(false); }
        catch (OperationCanceledException) { timedOut = true; parent.Kill(true); if (!parent.WaitForExit(10000)) throw new TimeoutException("Supervisor was not reaped after termination."); }
        if (!Task.WaitAll([stdout, stderr], 10000)) throw new TimeoutException("Supervisor output did not drain.");
        File.WriteAllText(Path.Combine(evidence, "parent-stdout.txt"), await stdout.ConfigureAwait(false));
        File.WriteAllText(Path.Combine(evidence, "parent-stderr.txt"), await stderr.ConfigureAwait(false));
        Save(Path.Combine(evidence, "parent-process.json"), new { pid, observerPid = Environment.ProcessId, timedOut, reaped = parent.HasExited, drained = stdout.IsCompletedSuccessfully && stderr.IsCompletedSuccessfully, exit = parent.ExitCode });
        Save(Path.Combine(evidence, "observer-after.json"), new { pid = Environment.ProcessId, modules = Modules() });
        string results = Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(results);
        using var audit = new Process { StartInfo = new ProcessStartInfo(Environment.ProcessPath!) { WorkingDirectory = AppContext.BaseDirectory, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
        foreach (string argument in new[] { "--strict-render-audit", AppContext.BaseDirectory, Path.Combine(results, "audit-summary.json") }) audit.StartInfo.ArgumentList.Add(argument);
        audit.Start();
        int auditPid = audit.Id;
        Task<string> auditStdout = audit.StandardOutput.ReadToEndAsync(), auditStderr = audit.StandardError.ReadToEndAsync();
        bool auditTimeout = !audit.WaitForExit(120000);
        if (auditTimeout) audit.Kill(true);
        bool auditReaped = audit.WaitForExit(10000);
        bool auditDrained = Task.WaitAll([auditStdout, auditStderr], 10000);
        int auditOsExit = auditReaped ? audit.ExitCode : -1;
        int auditExit = auditTimeout || !auditReaped || !auditDrained ? 124 : auditOsExit;
        if (auditDrained)
        {
            File.WriteAllText(Path.Combine(results, "audit-stdout.txt"), await auditStdout.ConfigureAwait(false));
            File.WriteAllText(Path.Combine(results, "audit-stderr.txt"), await auditStderr.ConfigureAwait(false));
        }
        Save(Path.Combine(results, "audit-process.json"), new { pid = auditPid, parentPid = Environment.ProcessId, timeout = auditTimeout, reaped = auditReaped, drained = auditDrained, exit = auditOsExit });
        foreach (string role in new[] { "original", "original-repeat", "candidate" })
        {
            if (StrictRenderCase.IsMedia(scene))
            {
                using var receipt = ReadReceipt(Path.Combine(evidence, role, "result.json"));
                Save(Path.Combine(results, role + "-media.json"), ContractKeys.ToDictionary(key => key, key => receipt.RootElement.TryGetProperty(key, out var value) ? (object?)value : null));
                if (receipt.RootElement.TryGetProperty("audioEndpointCapability", out var capability))
                    Save(Path.Combine(results, role + "-audio-capability.json"), capability);
            }
            foreach (string name in new[] { "pixels.pbgra32", "process.json", "stdout.txt", "stderr.txt", "image.png" })
            {
                string source = Path.Combine(evidence, role, name);
                if (!File.Exists(source) || (name == "image.png" && auditExit == 0) || (name.EndsWith(".txt", StringComparison.Ordinal) && new FileInfo(source).Length == 0)) continue;
                File.Copy(source, Path.Combine(results, role + "-" + name), true);
            }
        }
        foreach (string name in new[] { "parent-process.json", "parent-stdout.txt", "parent-stderr.txt", "comparison.json" })
        {
            string source = Path.Combine(evidence, name);
            if (File.Exists(source) && new FileInfo(source).Length != 0) File.Copy(source, Path.Combine(results, name), true);
        }
        int exit = timedOut ? 124 : parent.ExitCode != 0 ? parent.ExitCode : auditExit;
        Save(Path.Combine(results, "execution.json"), new { observerPid = Environment.ProcessId, parentExit = parent.ExitCode, timedOut, auditExit, intendedExit = exit });
        var files = Directory.GetFiles(results).Select(path => new { name = Path.GetFileName(path), bytes = new FileInfo(path).Length }).ToArray();
        Save(Path.Combine(results, "export-receipt.json"), new { coverage = "All Results files except this receipt; receipt size excluded explicitly", fileCount = files.Length, bytes = files.Sum(f => f.bytes), files });
        return exit;
    }

    internal static int Audit(string packagePath, string summaryPath)
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
        byte[] left = ReadPixels(Path.Combine(evidence, "original", "pixels.pbgra32")), right = ReadPixels(Path.Combine(evidence, "candidate", "pixels.pbgra32"));
        string scene = new[] { StrictRenderCase.Rectangle, StrictRenderCase.ClosedBezier, StrictRenderCase.TranslatedScaledBezier, StrictRenderCase.RotatedBezier, StrictRenderCase.ClippedBezier, StrictRenderCase.AlphaOverlap, StrictRenderCase.MediaAudioRatePreflight, StrictRenderCase.MediaAudioSeek, StrictRenderCase.MediaAudioInitializeClose, StrictRenderCase.MediaSource, StrictRenderCase.MediaContinuousSeek, StrictRenderCase.MediaPlayPauseResume, StrictRenderCase.MediaSpeedRatio }.Single(value => StrictRenderCase.CaseId(value) == a.RootElement.GetProperty("caseId").GetString());
        using var repeat = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, "original-repeat", "result.json")));
        byte[] repeated = ReadPixels(Path.Combine(evidence, "original-repeat", "pixels.pbgra32"));
        checks["caseMetadata"] = StrictRenderCase.MetadataMatches(a.RootElement, scene) && StrictRenderCase.MetadataMatches(b.RootElement, scene) && StrictRenderCase.MetadataMatches(repeat.RootElement, scene);
        checks[StrictRenderCase.IsMedia(scene) ? "mediaNoPixels" : "rawExact"] = left.Length == ExpectedBytes(a.RootElement, scene) && right.Length == ExpectedBytes(b.RootElement, scene) && left.AsSpan().SequenceEqual(right);
        checks[StrictRenderCase.IsMedia(scene) ? "oracleNoPixels" : "oracleRawStable"] = left.AsSpan().SequenceEqual(repeated);
        if (scene is StrictRenderCase.MediaAudioInitializeClose or StrictRenderCase.MediaAudioSeek)
        {
            string inputPath = Path.Combine(package, "Inputs", "audio-initialize-close", "content.bin");
            byte[] expected = StrictAudioInput.Bytes();
            checks["fixedWaveInput"] = File.Exists(inputPath) && File.ReadAllBytes(inputPath).AsSpan().SequenceEqual(expected);
            string expectedHash = Convert.ToHexString(SHA256.HashData(expected));
            checks["fixedWaveReceipts"] = new[] { a.RootElement, b.RootElement, repeat.RootElement }.All(receipt =>
                receipt.GetProperty("mediaAssets")[0].GetProperty("sha256").GetString() == expectedHash
                && receipt.GetProperty("mediaAssets")[0].GetProperty("bytes").GetInt32() == expected.Length);
        }
        if (scene == StrictRenderCase.ClippedBezier)
        {
            checks["clipRegisteredPixels"] = new[] { left, repeated, right }.All(data => data.Length == 4096
                && Convert.ToHexString(data.AsSpan((12 * 32 + 12) * 4, 4)) == "C06020FF"
                && Convert.ToHexString(data.AsSpan((12 * 32 + 22) * 4, 4)) == "FFFFFFFF");
        }
        if (scene == StrictRenderCase.AlphaOverlap) checks["alphaRegisteredPixelsPremultipliedNonempty"] = new[] { left, repeated, right }.All(StrictRenderCase.AlphaPixelsMatch);
        foreach (string key in ContractKeys)
        {
            checks["contract:" + key] = Value(a.RootElement, key) == Value(b.RootElement, key);
            checks["oracle:" + key] = Value(a.RootElement, key) == Value(repeat.RootElement, key);
        }
        foreach (string role in new[] { "original", "original-repeat", "candidate" })
        {
            JsonElement receipt = role == "original" ? a.RootElement : role == "original-repeat" ? repeat.RootElement : b.RootElement;
            using var process = JsonDocument.Parse(File.ReadAllText(Path.Combine(evidence, role, "process.json")));
            JsonElement p = process.RootElement;
            checks[role + ":exit"] = p.GetProperty("exit").GetInt32() == 0 && !p.GetProperty("timeout").GetBoolean() && p.GetProperty("reaped").GetBoolean() && p.GetProperty("drained").GetBoolean() && p.GetProperty("pid").GetInt32() == receipt.GetProperty("pid").GetInt32();
            string selected = Path.Combine(package, role != "candidate" ? "Reference" : "Native", "wpfgfx_cor3.dll");
            using (var native = File.OpenRead(selected)) AcceptancePaths.ValidateIdentity(native, Hash(selected));
            checks[role + ":architecture"] = receipt.GetProperty("processArchitecture").GetString() == RuntimeInformation.ProcessArchitecture.ToString();
            var gfx = receipt.GetProperty("moduleEvidence").EnumerateArray().Where(m => Path.GetFileName(m.GetProperty("path").GetString()!).StartsWith("wpfgfx_", StringComparison.OrdinalIgnoreCase)).ToArray();
            checks[role + ":uniqueGfx"] = gfx.Length == 1 && Resolve(gfx[0].GetProperty("path").GetString()!) == selected && gfx[0].GetProperty("sha256").GetString() == Hash(selected) && gfx[0].GetProperty("address").GetString() == receipt.GetProperty("selectedHandle").GetString() && receipt.GetProperty("nativeBasenameHandleMatches").GetBoolean();
            checks[role + ":actualNative"] = Resolve(receipt.GetProperty("actualNative").GetProperty("path").GetString()!) == selected && receipt.GetProperty("actualNative").GetProperty("sha256").GetString() == Hash(selected);
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
        int totalDiff = Math.Abs(left.Length - right.Length), firstDiff = -1;
        var regions = new Dictionary<int, int>();
        for (int i = 0; i < Math.Min(left.Length, right.Length); i++)
        {
            if (left[i] == right[i]) continue;
            totalDiff++;
            if (firstDiff < 0) firstDiff = i;
            int row = i / a.RootElement.GetProperty("imageMetadata").GetProperty("stride").GetInt32();
            regions[row] = regions.GetValueOrDefault(row) + 1;
        }
        if (firstDiff < 0 && left.Length != right.Length) firstDiff = Math.Min(left.Length, right.Length);
        bool passed = checks.Values.All(v => v);
        string summary = Path.GetFullPath(summaryPath, AppContext.BaseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(summary)!);
        Save(summary, new
        {
            passed, scene, sceneInput = StrictRenderCase.Input(scene), package, evidence, summary,
            identities = identities.RootElement, checks, totalDiff, firstDiff, threshold = 0,
            originalByte = firstDiff >= 0 && firstDiff < left.Length ? (int?)left[firstDiff] : null,
            candidateByte = firstDiff >= 0 && firstDiff < right.Length ? (int?)right[firstDiff] : null,
            differingRows = regions,
            businessReceipts = new[] { a.RootElement, repeat.RootElement, b.RootElement }.Select(r => new { pid = r.GetProperty("pid"), architecture = r.GetProperty("processArchitecture"), actualNative = r.GetProperty("actualNative"), harness = r.GetProperty("harness"), contract = ContractKeys.ToDictionary(k => k, k => Value(r, k)) }).ToArray(),
            alphaPixels = scene == StrictRenderCase.AlphaOverlap ? new[] { left, repeated, right }.Select(data => new[] { 0, (8 * 32 + 8) * 4, (24 * 32 + 24) * 4, (16 * 32 + 16) * 4 }.Select(offset => Convert.ToHexString(data.AsSpan(offset, 4))).ToArray()).ToArray() : null,
            clippedOutPixels = scene == StrictRenderCase.ClippedBezier ? new[] { left, repeated, right }.Select(data => Convert.ToHexString(data.AsSpan((12 * 32 + 22) * 4, 4))).ToArray() : null,
            wholeRawRead = (StrictRenderCase.IsMedia(scene) ? Array.Empty<(string role, byte[] data)>() : new[] { (role: "original", data: left), (role: "original-repeat", data: repeated), (role: "candidate", data: right) })
                .Select(item => new { item.role, path = Path.Combine(evidence, item.role, "pixels.pbgra32"), bytes = item.data.Length, sha256 = Convert.ToHexString(SHA256.HashData(item.data)), backgroundBGRA = Convert.ToHexString(item.data.AsSpan(0, 4)), interiorBGRA = Convert.ToHexString(item.data.AsSpan(scene == StrictRenderCase.Rectangle ? (3 * 32 + 3) * 4 : (12 * 32 + 12) * 4, 4)) }).ToArray(),
            moduleCount = moduleChecks.Count, manifestCount = manifestChecks.Length,
            moduleFailures = moduleJson.RootElement.EnumerateArray().Where(m => !m.GetProperty("equal").GetBoolean()).Select(m => m.Clone()).ToArray(),
            boundary = "Whole raw files read; module snapshots are not continuous interception; no signature-chain or complete lifecycle certification. Observer/audit outer OS exit is not recorded."
        });
        Console.WriteLine(JsonSerializer.Serialize(new { passed, scene, totalDiff, firstDiff, failedChecks = checks.Where(c => !c.Value).Select(c => c.Key).ToArray(), moduleCount = moduleChecks.Count, manifestCount = manifestChecks.Length, summary }));
        return passed ? 0 : 1;
    }

    private static int ExpectedBytes(JsonElement receipt, string scene) => StrictRenderCase.IsMedia(scene) ? 0 : StrictRenderCase.ByteCount(receipt);
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
        foreach (string arg in new[] { "--strict-render-business", dll, selectedCase, output, Hash(dll) }) child.StartInfo.ArgumentList.Add(arg);
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
