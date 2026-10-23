using System.Diagnostics;
using System.Reflection;
using Microsoft.Testing.Platform.Builder;

namespace WpfGfxShape.ComAcceptance;

internal static class NativeRun
{
    internal static string PathsRoot => Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT") ?? AcceptancePaths.Results;

    [STAThread]
    private static async Task<int> Main(string[] args)
    {
#if FH_CRASH_EVIDENCE
        if (args.Length == 1 && args[0] == "--fh-normal-exit")
            return 0;
        if (args.Length > 0 && args[0] == "--fh-crash-evidence")
            return FhCrashEvidence.Run(args[1..]);
#endif
        if (args.Length > 0 && args[0] == "--supervise-exit")
            return await ExitDiagnostics.RunAsync(args[1..]);
        if (args.Length > 0 && args[0] == "--bm-fill-control-points")
        {
            await new BmFillControlPointTests().WhenFillControlPointsAreQuantizedThenIndependentSiblingsMatch();
            return 0;
        }
        if (args.Length > 0 && args[0] == "--official-source")
            return await OfficialProtectionSourceTests.RunAsync(args[1]);
        if (args.Length > 0 && args[0] == "--pcore")
            return NativeAcceptance.Pcore.Program.Run(args[1..]);
        if (args.Length > 0 && args[0] == "--public-abi")
            return NativeAcceptance.PublicAbi.Program.RunRole(args[1..]);
        if (args.Length > 0 && args[0] == "--player-hwnd")
            return NativeAcceptance.PlayerHwnd.Program.RunRole(args[1..]);
        if (!args.Contains("--results-directory", StringComparer.Ordinal))
            args = [.. args, "--results-directory", Path.Combine(AppContext.BaseDirectory, "TestResults")];
        ITestApplicationBuilder builder = await TestApplication.CreateBuilderAsync(args);
        builder.AddSelfRegisteredExtensions(args);
        using ITestApplication app = await builder.BuildAsync();
        return await app.RunAsync();
    }

    internal static async Task<int> RoleAsync(string root, params string[] arguments)
    {
        Directory.CreateDirectory(root);
        var start = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false };
        if (Path.GetFileNameWithoutExtension(Environment.ProcessPath).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(NativeRun).Assembly.Location);
        foreach (string argument in arguments) start.ArgumentList.Add(argument);
        start.Environment["WPFGFX_RUN_ROOT"] = root;
        start.Environment["WPFGFX_CANDIDATE_DLL"] = AcceptancePaths.AotDll;
        start.Environment["WPFGFX_ORIGINAL_DLL"] = AcceptancePaths.OriginalDll;
        using Process process = Process.Start(start) ?? throw new InvalidOperationException();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(true); await process.WaitForExitAsync(); throw; }
        return process.ExitCode;
    }
}

[TestClass]
public sealed class PublicAbiComparisonTests
{
    /// <summary>Uses a fresh official report as the candidate oracle in the same isolated run.</summary>
    [TestMethod]
    public async Task WhenPublicAbiRunsThenSameRunOfficialContractIsUsed()
    {
        string root = Path.Combine(AcceptancePaths.Results, "publicabi", Guid.NewGuid().ToString("N"));
        int original = await NativeRun.RoleAsync(root, "--public-abi", "reference");
        string report = Directory.GetFiles(Path.Combine(root, "Results"), "report.json", SearchOption.AllDirectories).Single();
        File.Copy(report, Path.Combine(root, "original-publicabi.json"));
        int candidate = await NativeRun.RoleAsync(root, "--public-abi", "candidate", Path.Combine(root, "original-publicabi.json"));
        Assert.AreEqual(0, original, "Original control failed: " + root);
        Assert.AreEqual(0, candidate, "Candidate failed: " + root);
    }
}

[TestClass]
public sealed class PcoreComparisonTests
{
    /// <summary>Executes each WPF module in a fresh process and compares same-run observable output.</summary>
    [TestMethod]
    [DataRow("primitive")]
    [DataRow("images")]
    [DataRow("nested")]
    [DataRow("blur")]
    [DataRow("shadow")]
    [DataRow("lifetime")]
    [DataRow("media")]
    [DataRow("dispatcher")]
    public async Task WhenSameSceneRunsThenOriginalAndCandidatePixelsMatch(string scene)
    {
        foreach (string mode in new[] { "default", "false", "true" })
            await CompareSceneAsync(scene, mode);
    }

    private static async Task CompareSceneAsync(string scene, string mode)
    {
        string root = Path.Combine(AcceptancePaths.Results, "pcore", scene, mode, Guid.NewGuid().ToString("N"));
        string[] Role(string role) => ["--pcore", role, scene, mode];
        int original = await NativeRun.RoleAsync(root, Role("original"));
        int candidate = await NativeRun.RoleAsync(root, Role("candidate"));
        Assert.AreEqual(0, original, "Original control failed: " + root);
        Assert.AreEqual(0, candidate, "Candidate failed: " + root);
        string baseline = Path.Combine(root, "Baseline");
        string compared = Path.Combine(root, "ComparisonCandidate");
        Directory.CreateDirectory(baseline);
        Directory.CreateDirectory(compared);
        foreach (string file in Directory.GetFiles(Path.Combine(root, "Results", "original")))
            File.Copy(file, Path.Combine(baseline, Path.GetFileName(file)), true);
        foreach (string file in Directory.GetFiles(Path.Combine(root, "Results", "candidate")))
            File.Copy(file, Path.Combine(compared, Path.GetFileName(file)), true);
        string comparison = scene == "primitive" ? "compare" : "compare-" + scene;
        Assert.AreEqual(0, await NativeRun.RoleAsync(root, "--pcore", comparison), root);
    }
}
