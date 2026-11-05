using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Resources;

namespace WpfGfxShape.ComAcceptance;

internal static class AcceptancePaths
{
    private static readonly ResourceManager Resources = new("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(AcceptancePaths).Assembly);
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void InitializeSandboxDirectory()
    {
        // MTP resolves CLI result paths against the host working directory before assembly initialization.
        if (Sandbox) Directory.SetCurrentDirectory(AppContext.BaseDirectory);
    }

    internal static string AotDll => Path.GetFullPath(Environment.GetEnvironmentVariable("WPFGFX_CANDIDATE_DLL") ?? Resolve(Metadata("AotDllPath"), Sandbox, AppContext.BaseDirectory));
    internal static string OriginalDll
    {
        // Keep WPF type resolution outside callers that authenticate the candidate first.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        get => Path.GetFullPath(Environment.GetEnvironmentVariable("WPFGFX_ORIGINAL_DLL") ?? Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.Visual).Assembly.Location)!, "wpfgfx_cor3.dll"));
    }
    internal static bool Sandbox => Metadata("ComAcceptanceSandbox") == "true";
    internal static string Results
    {
        get
        {
            string path = Resolve(Metadata("CalibrationResultsPath"), Sandbox, AppContext.BaseDirectory);
            Directory.CreateDirectory(path);
            return path;
        }
    }

    internal static string Resolve(string path, bool sandbox, string root)
    {
        if (!sandbox)
        {
            if (!Path.IsPathFullyQualified(path)) throw new InvalidOperationException(Resources.GetString("AbsolutePathRequired"));
            return Path.GetFullPath(path);
        }
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path))
            throw new InvalidOperationException(Resources.GetString("RelativePathRequired"));
        string directory = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string resolved = Path.GetFullPath(Path.Combine(directory, path));
        if (!resolved.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(Resources.GetString("RelativePathRequired"));
        return resolved;
    }

    internal static string Metadata(string key) => typeof(AcceptancePaths).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(attribute => attribute.Key == key).Value ?? throw new InvalidOperationException(key);

    internal static string ValidateIdentity()
    {
        using var stream = File.OpenRead(AotDll);
        return ValidateIdentity(stream, Sandbox ? File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aot.sha256")).Trim() : null);
    }

    internal static string ValidateCandidateIdentity(string path)
    {
        using var stream = File.OpenRead(path);
        return ValidateIdentity(stream, File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aot.sha256")).Trim());
    }

    internal static string ValidateIdentity(Stream stream, string? expectedSha)
    {
        using var pe = new PEReader(stream, PEStreamOptions.LeaveOpen);
        Machine expected = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => Machine.Amd64,
            Architecture.Arm64 => Machine.Arm64,
            Architecture.X86 => Machine.I386,
            _ => throw new PlatformNotSupportedException()
        };
        if (pe.PEHeaders.PEHeader is null || pe.PEHeaders.CorHeader is not null || pe.PEHeaders.CoffHeader.Machine != expected)
            throw new InvalidDataException(Resources.GetString("InvalidNativeIdentity"));
        stream.Position = 0;
        string sha = Convert.ToHexString(SHA256.HashData(stream));
        if (expectedSha is not null) ValidateDigest(sha, expectedSha);
        return sha;
    }

    internal static void ValidateDigest(string sha, string expectedSha)
    {
        if (!sha.Equals(expectedSha, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(Resources.GetString("InvalidNativeIdentity"));
    }

    internal static void ValidateCandidateModules(string path, string[] modules, bool require = true)
    {
        if ((require && modules.Length != 1) || modules.Length > 1 || modules.Any(module =>
            !Path.GetFullPath(module).Equals(Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(Resources.GetString("UnexpectedNativeModules"));
    }
}

