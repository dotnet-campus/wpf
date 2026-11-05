using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

string root = Path.Combine(AppContext.BaseDirectory, "Input");
string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
JsonDocument Read(string path) => JsonDocument.Parse(File.ReadAllBytes(path));
string Value(JsonElement e, string key) => e.TryGetProperty(key, out var v) ? v.GetRawText() : "absent";
var runs = new List<object>();
string provenancePath = Path.Combine(AppContext.BaseDirectory, "Provenance", "wpfgfx_cor3.dll");
using var run2Identities = Read(Path.Combine(root, "run2", "Evidence", "identities.json"));
string selectedOfficialSha = run2Identities.RootElement.GetProperty("official").GetProperty("sha256").GetString() ?? throw new InvalidDataException();
string provenanceSha = Hash(provenancePath);
Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Evidence"));
File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "Evidence", "official-provenance.json"), JsonSerializer.Serialize(new
{
    packageOriginalPath = run2Identities.RootElement.GetProperty("officialSource").GetString(),
    publishedReadOnlyCopyPath = provenancePath,
    sha256 = provenanceSha,
    selectedOfficialSha256 = selectedOfficialSha,
    matches = provenanceSha == selectedOfficialSha,
    length = new FileInfo(provenancePath).Length,
    trustScope = "SHA256 equality of the NuGet-cache file copied by publish; no Authenticode chain, revocation, NuGet package signature or download provenance validation."
}, new JsonSerializerOptions { WriteIndented = true }));
bool passed = provenanceSha == selectedOfficialSha;
foreach (string run in new[] { "run1", "run2" })
{
    string directory = Path.Combine(root, run), evidence = Path.Combine(directory, "Evidence"), package = Path.Combine(directory, "Package");
    var inventory = Directory.GetFiles(evidence, "*", SearchOption.AllDirectories).Order().Select(p => new { path = Path.GetRelativePath(evidence, p), length = new FileInfo(p).Length, sha256 = Hash(p) }).ToArray();
    using var original = Read(Path.Combine(evidence, "original/result.json"));
    using var candidate = Read(Path.Combine(evidence, "candidate/result.json"));
    using var op = Read(Path.Combine(evidence, "original/process.json"));
    using var cp = Read(Path.Combine(evidence, "candidate/process.json"));
    using var before = Read(Path.Combine(evidence, "parent-before.json"));
    using var after = Read(Path.Combine(evidence, "parent-after.json"));
    using var identities = Read(Path.Combine(evidence, "identities.json"));
    using var comparison = Read(Path.Combine(evidence, "comparison.json"));
    using var manifest = Read(Path.Combine(evidence, "package-manifest.json"));
    using var sources = Read(Path.Combine(evidence, "source-manifest.json"));
    var manifestChecks = manifest.RootElement.EnumerateArray().Select(m => { string path = m.GetProperty("path").GetString()!; string file = Path.Combine(package, path); return new { path, matches = File.Exists(file) && Hash(file) == m.GetProperty("sha256").GetString() && new FileInfo(file).Length == m.GetProperty("length").GetInt64() }; }).ToArray();
    var sourceChecks = sources.RootElement.EnumerateArray().Select(m => { string path = m.GetProperty("path").GetString()!; return new { path, matches = Hash(Path.Combine(directory, "Sources", path)) == m.GetProperty("sha256").GetString() }; }).ToArray();
    byte[] a = File.ReadAllBytes(Path.Combine(evidence, "original/pixels.pbgra32")), b = File.ReadAllBytes(Path.Combine(evidence, "candidate/pixels.pbgra32"));
    int count = Math.Abs(a.Length - b.Length), first = -1;
    for (int i = 0; i < Math.Min(a.Length, b.Length); i++) if (a[i] != b[i]) { count++; if (first < 0) first = i; }
    if (first < 0 && a.Length != b.Length) first = Math.Min(a.Length, b.Length);
    string[] keys = ["businessEvents", "imageMetadata", "cleanup", "exception", "failureStage", "exitCode", "caseId", "consumer", "stage", "originalAssertionsPassed", "pixelByteCount"];
    var contracts = keys.Select(key => new { key, original = Value(original.RootElement, key), candidate = Value(candidate.RootElement, key), equal = Value(original.RootElement, key) == Value(candidate.RootElement, key) }).ToArray();
    var children = new List<object>();
    bool moduleGate = true, runtimeGate = true, processGate = true;
    foreach (var pair in new[] { (role: "original", receipt: original.RootElement, process: op.RootElement, folder: "Reference"), (role: "candidate", receipt: candidate.RootElement, process: cp.RootElement, folder: "Native") })
    {
        var receipt = pair.receipt;
        bool hasModuleEvidence = receipt.TryGetProperty("moduleEvidence", out var moduleEvidence);
        var modules = hasModuleEvidence ? moduleEvidence.EnumerateArray().ToArray() : Array.Empty<JsonElement>();
        var gfx = modules.Where(m => Path.GetFileName(m.GetProperty("path").GetString()!).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).ToArray();
        string selected = pair.process.GetProperty("selected").GetString()!, digest = Hash(Path.Combine(package, pair.folder, "wpfgfx_cor3.dll"));
        bool gate = gfx.Length == 1 && gfx[0].GetProperty("path").GetString() == selected && gfx[0].GetProperty("address").GetString() == receipt.GetProperty("selectedHandle").GetString() && gfx[0].GetProperty("sha256").GetString() == digest && receipt.GetProperty("actualNative").GetProperty("sha256").GetString() == digest && receipt.GetProperty("nativeBasenameHandleMatches").GetBoolean();
        bool snapshots = new[] { "baseline", "preSceneModules", "finalModules" }.All(key => receipt.GetProperty(key).EnumerateArray().Where(m => Path.GetFileName(m.GetString()!).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).All(m => m.GetString() == selected));
        moduleGate &= gate && snapshots;
        var runtimeChecks = modules.Where(m => m.GetProperty("path").GetString()!.StartsWith(Path.GetDirectoryName(selected)![..^pair.folder.Length], StringComparison.OrdinalIgnoreCase)).Select(m => { string name = Path.GetFileName(m.GetProperty("path").GetString()!); string local = name == "wpfgfx_cor3.dll" ? Path.Combine(package, pair.folder, name) : Path.Combine(package, name); return new { name, matches = File.Exists(local) && Hash(local) == m.GetProperty("sha256").GetString() }; }).ToArray();
        runtimeGate &= hasModuleEvidence && runtimeChecks.All(c => c.matches);
        bool process = pair.process.GetProperty("exit").GetInt32() == 0 && !pair.process.GetProperty("timeout").GetBoolean() && pair.process.GetProperty("reaped").GetBoolean() && pair.process.GetProperty("drained").GetBoolean() && pair.process.GetProperty("killError").ValueKind == JsonValueKind.Null && receipt.GetProperty("pid").GetInt32() == pair.process.GetProperty("pid").GetInt32() && pair.process.GetProperty("parentPid").GetInt32() == before.RootElement.GetProperty("pid").GetInt32();
        processGate &= process;
        children.Add(new { pair.role, process = pair.process.Clone(), moduleGate = gate, snapshots, runtimeChecks, stdout = File.ReadAllText(Path.Combine(evidence, pair.role, "stdout.txt")), stderr = File.ReadAllText(Path.Combine(evidence, pair.role, "stderr.txt")), receipt = receipt.Clone() });
    }
    bool parentClean = new[] { before.RootElement, after.RootElement }.All(p => !p.GetProperty("modules").EnumerateArray().Any(m => Path.GetFileName(m.GetProperty("path").GetString()!).Contains("wpfgfx", StringComparison.OrdinalIgnoreCase) || Path.GetFileName(m.GetProperty("path").GetString()!).Equals("PresentationCore.dll", StringComparison.OrdinalIgnoreCase)));
    using var pe = new PEReader(File.OpenRead(Path.Combine(package, "StrictRenderR28.dll")));
    var metadata = pe.GetMetadataReader();
    string mvid = metadata.GetGuid(metadata.GetModuleDefinition().Mvid).ToString();
    string harnessHash = Hash(Path.Combine(package, "StrictRenderR28.dll"));
    bool harnessGate = new[] { original.RootElement, candidate.RootElement }.All(r => r.GetProperty("mvid").GetString() == mvid && r.GetProperty("harness").GetProperty("sha256").GetString() == harnessHash);
    using var certificate = X509CertificateLoader.LoadCertificate(X509Certificate.CreateFromSignedFile(Path.Combine(package, "Reference/wpfgfx_cor3.dll")).GetRawCertData());
    bool identityGate = Hash(Path.Combine(package, "Reference/wpfgfx_cor3.dll")) == "AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1" && Hash(Path.Combine(package, "Native/wpfgfx_cor3.dll")) == "8EE1C6895313DB7AB9F95B90D45EF1D0249695229ABE227D1F73C725A8D9FC6D" && certificate.Subject == identities.RootElement.GetProperty("signer").GetString();
    bool distinctPids = op.RootElement.GetProperty("pid").GetInt32() != cp.RootElement.GetProperty("pid").GetInt32();
    bool runPassed = count == 0 && a.Length == 4096 && b.Length == 4096 && moduleGate && parentClean && contracts.All(c => c.equal) && processGate && distinctPids && runtimeGate && harnessGate && identityGate && manifestChecks.All(c => c.matches) && sourceChecks.All(c => c.matches);
    passed &= runPassed;
    runs.Add(new { run, passed = runPassed, count, first, originalBytes = a.Length, candidateBytes = b.Length, originalSha256 = Convert.ToHexString(SHA256.HashData(a)), candidateSha256 = Convert.ToHexString(SHA256.HashData(b)), moduleGate, parentClean, processGate, distinctPids, runtimeGate, harnessGate, harnessHash, mvid, executableHash = Hash(Path.Combine(package, "StrictRenderR28.exe")), identityGate, signer = certificate.Subject, identities = identities.RootElement.Clone(), comparison = comparison.RootElement.Clone(), inventory, manifestChecks, sourceChecks, contracts, children });
}
Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Evidence"));
File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "Evidence/readback.json"), JsonSerializer.Serialize(new { passed, runs }, new JsonSerializerOptions { WriteIndented = true }));
using var full = JsonDocument.Parse(JsonSerializer.Serialize(new { passed, runs }));
var summary = new { passed, runs = full.RootElement.GetProperty("runs").EnumerateArray().Select(r => r.EnumerateObject().Where(p => p.Name is not "children" and not "inventory" and not "manifestChecks" and not "comparison" and not "contracts" and not "identities").ToDictionary(p => p.Name, p => p.Value.Clone())).ToArray() };
File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "Evidence/summary.json"), JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(JsonSerializer.Serialize(summary));
return passed ? 0 : 1;
