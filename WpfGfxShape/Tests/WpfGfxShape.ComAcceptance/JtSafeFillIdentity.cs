#if JT_SAFE_FILL_IDENTITY
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class JtSafeFillIdentity
{
    /// <summary>Records loaded module identities without changing rendering or assertions.</summary>
    [AssemblyCleanup]
    public static void RecordLoadedModules()
    {
        using Process process = Process.GetCurrentProcess();
        var modules = process.Modules.Cast<ProcessModule>()
            .Where(module => string.Equals(Path.GetFileName(module.FileName), "wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase))
            .Select(module => new
            {
                Actual = module.FileName,
                Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(module.FileName))),
                Version = module.FileVersionInfo.FileVersion
            }).ToArray();
        string test = typeof(JtSafeFillIdentity).Assembly.Location;
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, "jt-loaded-identity.json"), JsonSerializer.Serialize(new
        {
            Pid = Environment.ProcessId,
            Runtime = RuntimeInformation.FrameworkDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            TestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(test))),
            TestMvid = typeof(JtSafeFillIdentity).Module.ModuleVersionId,
            Modules = modules
        }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
#endif
