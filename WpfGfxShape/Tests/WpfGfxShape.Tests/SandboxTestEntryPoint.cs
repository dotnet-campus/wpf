#if SANDBOX_TEST_ENTRY_POINT
using Microsoft.Testing.Platform.Builder;

namespace WpfGfxShape.Tests;

internal static class SandboxTestEntryPoint
{
    private static async Task<int> Main(string[] args)
    {
        int resultsIndex = Array.IndexOf(args, "--results-directory");
        if (resultsIndex < 0)
            args = [.. args, "--results-directory", Path.Combine(AppContext.BaseDirectory, "TestResults")];
        else if (resultsIndex + 1 < args.Length)
            args[resultsIndex + 1] = Path.GetFullPath(args[resultsIndex + 1], AppContext.BaseDirectory);
        ITestApplicationBuilder builder = await TestApplication.CreateBuilderAsync(args);
        SelfRegisteredExtensions.AddSelfRegisteredExtensions(builder, args);
        using ITestApplication app = await builder.BuildAsync();
        int exitCode = await app.RunAsync();
        string results = Path.Combine(AppContext.BaseDirectory, "TestResults");
        Directory.CreateDirectory(results);
        File.WriteAllText(Path.Combine(results, "process-exit.json"), System.Text.Json.JsonSerializer.Serialize(new
        {
            pid = Environment.ProcessId,
            architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            exitCode
        }));
        return exitCode;
    }
}

[TestClass]
public sealed class SandboxOutputPreflightTests
{
    /// <summary>Persists actual executable, test and managed product identities inside the recovered package.</summary>
    [TestMethod]
    public void WhenSandboxPreflightRunsThenIdentityIsRecordedUnderBaseDirectory()
    {
        string results = Path.Combine(AppContext.BaseDirectory, "Results");
        Directory.CreateDirectory(results);
        var assemblies = new[] { typeof(SandboxOutputPreflightTests).Assembly, System.Reflection.Assembly.Load("wpfgfx_cor3") };
        string path = Path.Combine(results, "preflight-identity.json");
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new
        {
            BaseDirectory = AppContext.BaseDirectory,
            ProcessPath = Environment.ProcessPath,
            Architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            PointerSize = IntPtr.Size,
            Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            Assemblies = assemblies.Select(assembly => new
            {
                assembly.Location,
                assembly.FullName,
                Sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(assembly.Location)))
            })
        }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
        Assert.IsTrue(File.Exists(path));
    }
}
#endif
