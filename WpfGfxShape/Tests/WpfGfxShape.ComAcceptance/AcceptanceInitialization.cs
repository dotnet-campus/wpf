using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class AcceptanceInitialization
{
    /// <summary>Checks the authenticated artifact digest format.</summary>
    [TestMethod]
    public void WhenArtifactIsValidatedThenIdentityIsSha256()
    {
        Assert.AreEqual(64, AcceptancePaths.ValidateIdentity().Length);
    }

    /// <summary>Rejects an invalid artifact before any production native entry is invoked.</summary>
    [AssemblyInitialize]
    public static void Initialize(TestContext context)
    {
        string sha = AcceptancePaths.ValidateIdentity();
        context.WriteLine($"Native AOT artifact: {AcceptancePaths.AotDll}; SHA256={sha}; sandbox={AcceptancePaths.Sandbox}");
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, "aot-identity.json"), System.Text.Json.JsonSerializer.Serialize(new
        {
            AotDllPath = AcceptancePaths.AotDll, Sha256 = sha, Sandbox = AcceptancePaths.Sandbox,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
            CalibrationResultsPath = AcceptancePaths.Metadata("CalibrationResultsPath"),
            ResolvedResultsPath = AcceptancePaths.Results, WorkingDirectory = Directory.GetCurrentDirectory(),
            BaseDirectory = AppContext.BaseDirectory, Arguments = Environment.GetCommandLineArgs()
        }));
    }
}
