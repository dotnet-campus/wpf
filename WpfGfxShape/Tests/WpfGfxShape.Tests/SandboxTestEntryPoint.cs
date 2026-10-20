#if SANDBOX_TEST_ENTRY_POINT
using Microsoft.Testing.Platform.Builder;

namespace WpfGfxShape.Tests;

internal static class SandboxTestEntryPoint
{
    private static async Task<int> Main(string[] args)
    {
        if (!args.Contains("--results-directory", StringComparer.Ordinal))
            args = [.. args, "--results-directory", Path.Combine(AppContext.BaseDirectory, "TestResults")];
        ITestApplicationBuilder builder = await TestApplication.CreateBuilderAsync(args);
        SelfRegisteredExtensions.AddSelfRegisteredExtensions(builder, args);
        using ITestApplication app = await builder.BuildAsync();
        return await app.RunAsync();
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
