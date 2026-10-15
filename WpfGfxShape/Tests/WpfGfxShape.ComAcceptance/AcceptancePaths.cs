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

    internal static string AotDll => Resolve(Metadata("AotDllPath"), Sandbox, AppContext.BaseDirectory);
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
        string path = AotDll;
        using var stream = File.OpenRead(path);
        using var pe = new PEReader(stream, PEStreamOptions.LeaveOpen);
        Machine expected = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => Machine.Amd64,
            Architecture.Arm64 => Machine.Arm64,
            Architecture.X86 => Machine.I386,
            _ => throw new PlatformNotSupportedException()
        };
        if (pe.PEHeaders.CorHeader is not null || pe.PEHeaders.CoffHeader.Machine != expected)
            throw new InvalidDataException(Resources.GetString("InvalidNativeIdentity"));
        stream.Position = 0;
        string sha = Convert.ToHexString(SHA256.HashData(stream));
        if (Sandbox && !string.Equals(sha, File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aot.sha256")).Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException(Resources.GetString("InvalidNativeIdentity"));
        return sha;
    }
}

[TestClass]
public sealed class AcceptanceInitialization
{
    /// <summary>Rejects an invalid artifact before any production native entry is invoked.</summary>
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        string sha = AcceptancePaths.ValidateIdentity();
        context.WriteLine($"Native AOT artifact: {AcceptancePaths.AotDll}; SHA256={sha}; sandbox={AcceptancePaths.Sandbox}");
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, "aot-identity.json"), System.Text.Json.JsonSerializer.Serialize(new
        {
            AotDllPath = AcceptancePaths.AotDll, Sha256 = sha, Sandbox = AcceptancePaths.Sandbox,
            CalibrationResultsPath = AcceptancePaths.Metadata("CalibrationResultsPath"),
            ResolvedResultsPath = AcceptancePaths.Results, WorkingDirectory = Directory.GetCurrentDirectory(),
            BaseDirectory = AppContext.BaseDirectory, Arguments = Environment.GetCommandLineArgs()
        }));
    }
}
