using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

string root = AppContext.BaseDirectory;
string output = Path.Combine(root, "Evidence");
Directory.CreateDirectory(output);
string native = Path.Combine(root, "Native", "wpfgfx_cor3.dll");
using var stream = File.OpenRead(native);
using var pe = new PEReader(stream, PEStreamOptions.LeaveOpen);
stream.Position = 0;
string sha = Convert.ToHexString(SHA256.HashData(stream));
var files = Directory.EnumerateFiles(Path.Combine(root, "Source"), "*", SearchOption.AllDirectories)
    .OrderBy(path => Path.GetRelativePath(root, path), StringComparer.Ordinal)
    .Select(path => new { path = Path.GetRelativePath(root, path).Replace('\\', '/'), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), length = new FileInfo(path).Length }).ToArray();
string manifest = string.Join("\n", files.Select(file => $"{file.sha256}  {file.path}")) + "\n";
File.WriteAllText(Path.Combine(output, "source.sha256"), manifest, new UTF8Encoding(false));
var evidence = new
{
    purpose = "Read-only artifact identity; no NativeLibrary.Load, no WPF, no business acceptance",
    nativePath = native,
    nativeSha256 = sha,
    nativeLength = stream.Length,
    machine = pe.PEHeaders.CoffHeader.Machine.ToString(),
    hasOptionalHeader = pe.PEHeaders.PEHeader is not null,
    hasClrHeader = pe.PEHeaders.CorHeader is not null,
    processArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    framework = RuntimeInformation.FrameworkDescription,
    sourceManifestSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(manifest))),
    sourceFileCount = files.Length,
    sourceFiles = files,
    formalHarnessPackageCreated = false,
    finalPackageAotManifestMatch = "NotEvaluated",
    nativeLoaded = false
};
File.WriteAllText(Path.Combine(output, "identity.json"), JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(JsonSerializer.Serialize(new { sha, machine = pe.PEHeaders.CoffHeader.Machine.ToString(), hasClrHeader = pe.PEHeaders.CorHeader is not null, sourceFileCount = files.Length }));
return pe.PEHeaders.CoffHeader.Machine == Machine.Amd64 && pe.PEHeaders.PEHeader is not null && pe.PEHeaders.CorHeader is null ? 0 : 1;
