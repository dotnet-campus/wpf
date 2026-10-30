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
#if SANDBOX_DESKTOP_DIAGNOSTIC
        SandboxDesktopController.Capture("process-entry-before-fixture");
        if (args.Length > 0 && args[0] == "--owned-desktop")
            return SandboxDesktopController.Run(args[1..]);
#endif
        if (args.Length == 1 && args[0] == "--exclusive-focus-role")
            return D3DImageTests.RunExclusiveFocusRole();
#if RESOURCE_LIFECYCLE_TREND
        if (args.Length == 2 && args[0] == "--resource-lifecycle-trend-role")
            return UceChannelTests.RunResourceTrendRole(args[1]);
#endif
#if JJ_FULL_READBACK
        if (args.Length > 0 && args[0] == "--jj-full-readback") return JjFullReadback.Run(args[1..]);
#endif
#if FH_CRASH_EVIDENCE
        if (args.Length == 1 && args[0] == "--fh-normal-exit")
            return 0;
        if (args.Length > 0 && args[0] == "--fh-crash-evidence")
            return FhCrashEvidence.Run(args[1..]);
#endif
#if WIC_OWNER_OFFLINE_SYMBOLS
        if (args.Length == 2 && args[0] == "--wic-owner-offline-symbols")
            return await SamplerOfflineSymbols.RunWicOwnerAsync(args[1]);
#endif
#if CACHE_CONSUMER_SYMBOL_PREPARATION
        if (args.Length == 2 && args[0] == "--cache-consumer-symbol-preparation")
            return await SamplerOfflineSymbols.PrepareCacheConsumerAsync(args[1]);
#endif
#if SAMPLER_OFFLINE_SYMBOLS
        if (args.Length == 2 && args[0] == "--sampler-offline-symbols")
            return await SamplerOfflineSymbols.RunAsync(args[1]);
#endif
        if (args.Length == 2 && args[0] == "--sampler-native-crash")
            return SamplerNativeCrash.Run(args[1]);
        if (args.Length == 2 && args[0] == "--sampler-native-child")
            return await SamplerNativeCrash.RunChildAsync(args[1]);
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

#if RESOURCE_LIFECYCLE_TREND
    internal static async Task<int> ResourceTrendRoleAsync(string root, string role, string optIn, CancellationToken cancellationToken)
    {
        ResourceTrendCoordinator.ValidateOptIn(optIn);
        if (role is not ("original" or "candidate"))
            throw new ArgumentException(new System.Resources.ResourceManager("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(NativeRun).Assembly)
                .GetString("ResourceTrendRole", System.Globalization.CultureInfo.CurrentUICulture), nameof(role));
        Directory.CreateDirectory(root);
        string executable = Environment.ProcessPath ?? throw new InvalidOperationException(nameof(Environment.ProcessPath));
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
        };
        if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(NativeRun).Assembly.Location);
        start.ArgumentList.Add("--resource-lifecycle-trend-role");
        start.ArgumentList.Add(role);
        start.Environment[ResourceTrendCoordinator.OptInEnvironment] = optIn;
        start.Environment["WPFGFX_RUN_ROOT"] = root;
        start.Environment["WPFGFX_CANDIDATE_DLL"] = AcceptancePaths.AotDll;
        start.Environment["WPFGFX_ORIGINAL_DLL"] = AcceptancePaths.OriginalDll;
        cancellationToken.ThrowIfCancellationRequested();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(360));
        await using var stdout = File.Create(Path.Combine(root, "stdout.log"));
        await using var stderr = File.Create(Path.Combine(root, "stderr.log"));
        using var collection = new CancellationTokenSource();
        Task? output = null, errors = null;
        string status = "Running";
        Exception? primary = null;
        var cleanupErrors = new List<Exception>();
        bool killFailed = false, reaped = false, forcedTermination = false;
        int? exitCode = null;
        using Process process = Process.Start(start) ?? throw new InvalidOperationException(nameof(Process.Start));
        try
        {
            output = process.StandardOutput.BaseStream.CopyToAsync(stdout, collection.Token);
            errors = process.StandardError.BaseStream.CopyToAsync(stderr, collection.Token);
            await process.WaitForExitAsync(deadline.Token).ConfigureAwait(false);
            status = "Exited";
        }
        catch (Exception error)
        {
            primary = error;
            status = error is OperationCanceledException ? "TimedOutOrCancelled" : "Failed";
        }
        finally
        {
            // Own only this Start result; termination is not proof of native resource cleanup.
            try
            {
                if (!process.HasExited)
                {
                    forcedTermination = true;
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception error) { killFailed = true; cleanupErrors.Add(error); }
            using var exitDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                await process.WaitForExitAsync(exitDeadline.Token).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                reaped = process.HasExited;
                if (reaped) exitCode = process.ExitCode;
            }
            catch (Exception error) { cleanupErrors.Add(error); }
            foreach (Task? capture in new[] { output, errors })
            {
                if (capture is null) continue;
                try { await capture.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); }
                catch (Exception error) { cleanupErrors.Add(error); }
            }
            try { collection.Cancel(); }
            catch (Exception error) { cleanupErrors.Add(error); }
            if (killFailed) status = "KillFailed";
            else if (!reaped) status = "Unreaped";
            else if (forcedTermination) status = "ForcedTermination";
            else if (cleanupErrors.Count != 0) status = "CollectionFailed";
            try
            {
                ResourceTrendCoordinator.Save(Path.Combine(root, "supervision.json"), new
                {
                    role, pid = process.Id, status, exitCode, killFailed, reaped, forcedTermination,
                    resourceCleanup = forcedTermination || !reaped || exitCode != 0 ? "Unknown" : "SeeRoundBusiness",
                    failure = primary?.ToString(), cleanupErrors = cleanupErrors.Select(e => e.ToString()).ToArray(),
                    roleSeconds = 360, parentSeconds = 840, roundSeconds = 20,
                    exitWaitSeconds = 5, drainSecondsPerStream = 5, watchdogHardRealtime = false,
                    utc = DateTimeOffset.UtcNow, qpc = Stopwatch.GetTimestamp()
                });
            }
            catch (Exception error) { cleanupErrors.Add(error); }
        }
        if (primary is not null)
        {
            primary.Data["ResourceTrendCleanup"] = cleanupErrors.ToArray();
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw();
        }
        if (cleanupErrors.Count != 0) throw new AggregateException(cleanupErrors);
        if (forcedTermination || !reaped || killFailed) return 1;
        return exitCode ?? 1;
    }
#endif

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

    /// <summary>Compares one default-mode continuous media run, stopping before the candidate if the official control fails.</summary>
    [TestMethod]
    public async Task WhenContinuousMediaRunsOnceThenOriginalAndCandidateFramesMatch()
    {
        VerifyContinuousMediaNegativeControls();
        await CompareSceneAsync("media", "default", verifyContinuousMedia: true);
    }

    private static bool HasMediaSemantics(byte[] pixels, int platform)
    {
        if (pixels.Length != 32 * 16 * 4) return false;
        // Fixed geometric interior; these source predicates are not a universal pipeline guarantee.
        for (int y = 4; y < 12; y++)
            for (int x = 8; x < 24; x++)
            {
                int offset = (y * 32 + x) * 4;
                byte b = pixels[offset], g = pixels[offset + 1], r = pixels[offset + 2];
                if (pixels[offset + 3] != 255 || (r | g | b) == 0) return false;
                bool matches = platform switch
                {
                    0 => r > g && r > b,
                    1 => b > r && b > g,
                    2 => g > r && g > b,
                    3 => r > b && g > b,
                    _ => false
                };
                if (!matches) return false;
            }
        return true;
    }

    private static void VerifyContinuousMediaNegativeControls()
    {
        var black = new byte[2048];
        var transparent = new byte[2048];
        byte[][] controls = new uint[] { 0xffff0000, 0xff0000ff, 0xff00ff00, 0xffffff00 }
            .Select(color => Enumerable.Range(0, 512).SelectMany(_ => BitConverter.GetBytes(color)).ToArray()).ToArray();
        for (int offset = 3; offset < black.Length; offset += 4) black[offset] = 255;
        for (int platform = 0; platform < 4; platform++)
        {
            Assert.IsTrue(HasMediaSemantics(controls[platform], platform));
            Assert.IsFalse(HasMediaSemantics(black, platform));
            Assert.IsFalse(HasMediaSemantics(transparent, platform));
            Assert.IsFalse(HasMediaSemantics(controls[(platform + 1) % 4], platform));
            Assert.IsFalse(HasMediaSemantics(controls[platform ^ 1], platform));
        }
    }

    private static void VerifyContinuousMedia(string root, string role)
    {
        string directory = Path.Combine(root, "Results", role);
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "result.json")));
        var result = document.RootElement;
        Assert.AreEqual("Green", result.GetProperty("status").GetString());
        Assert.AreEqual("completed", result.GetProperty("stage").GetString());
        Assert.AreEqual(0, result.GetProperty("exitCode").GetInt32());
        Assert.AreEqual(role == "original" ? "OriginalOnly" : "PendingIndependentComparator", result.GetProperty("comparisonStatus").GetString());
        var frames = result.GetProperty("mediaFrames").EnumerateArray().ToArray();
        Assert.AreEqual(4, frames.Length);
        Assert.AreEqual(4, Directory.GetFiles(directory, "*.pbgra32").Length);
        var pixelsByFrame = new List<byte[]>();
        var inputIdentities = new List<object>();
        for (int index = 0; index < 4; index++)
        {
            int asset = index / 2;
            double requested = index % 2 == 0 ? 0.5 : 2.5;
            string name = $"media-{asset}-{(int)requested}.pbgra32";
            var frame = frames[index];
            Assert.AreEqual(asset, frame.GetProperty("asset").GetInt32());
            Assert.AreEqual(requested, frame.GetProperty("requestedPosition").GetDouble());
            Assert.AreEqual(name, frame.GetProperty("file").GetString());
            double actual = frame.GetProperty("actualPosition").GetDouble();
            Assert.IsTrue(double.IsFinite(actual) && actual >= requested && actual < requested + 1);
            if (index % 2 == 1) Assert.IsTrue(actual > frames[index - 1].GetProperty("actualPosition").GetDouble());
            Assert.IsTrue(frame.GetProperty("HasVideo").GetBoolean());
            Assert.IsTrue(frame.GetProperty("HasAudio").GetBoolean());
            Assert.AreEqual(2, frame.GetProperty("NaturalVideoWidth").GetInt32());
            Assert.AreEqual(1, frame.GetProperty("NaturalVideoHeight").GetInt32());
            byte[] pixels = File.ReadAllBytes(Path.Combine(directory, name));
            Assert.AreEqual(2048, pixels.Length);
            Assert.AreEqual(frame.GetProperty("sha256").GetString(), Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pixels)));
            Assert.IsTrue(HasMediaSemantics(pixels, index), Path.Combine(directory, name));
            foreach (byte[] previous in pixelsByFrame) Assert.IsFalse(pixels.AsSpan().SequenceEqual(previous));
            pixelsByFrame.Add(pixels);
        }
        var events = result.GetProperty("mediaEvents").EnumerateArray().ToArray();
        Assert.AreEqual(7, events.Length);
        for (int index = 0; index < 4; index++)
        {
            string kind = index % 2 == 0 ? "opened" : "ended";
            Assert.AreEqual(kind, events[index].GetProperty("kind").GetString());
            Assert.AreEqual(index / 2 + 1, events[index].GetProperty(kind).GetInt32());
        }
        foreach (string kind in new[] { "parallel-close-first", "parallel-close-second", "expected-failed-close" })
            Assert.AreEqual(1, events.Count(item => item.GetProperty("kind").GetString() == kind));
        Assert.AreEqual(0, events.Count(item => item.GetProperty("kind").GetString() == "failed"));
        Assert.AreEqual(2, result.GetProperty("mediaClosed").GetProperty("opened").GetInt32());
        Assert.AreEqual(2, result.GetProperty("mediaClosed").GetProperty("ended").GetInt32());
        for (int asset = 0; asset < 2; asset++)
        {
            string path = Path.Combine(directory, $"media-{asset}.avi");
            byte[] bytes = File.ReadAllBytes(path);
            inputIdentities.Add(new { asset, path, length = bytes.Length, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)) });
        }
        File.WriteAllText(Path.Combine(directory, "continuous-oracle.json"), System.Text.Json.JsonSerializer.Serialize(
            new { role, status = "Passed", width = 32, height = 16, frameBytes = 2048, interior = new { x = 8, y = 4, width = 16, height = 8 }, inputIdentities },
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }

    private static async Task CompareSceneAsync(string scene, string mode, bool verifyContinuousMedia = false)
    {
        string root = Path.Combine(AcceptancePaths.Results, "pcore", scene, mode, Guid.NewGuid().ToString("N"));
        string[] Role(string role) => ["--pcore", role, scene, mode];
        int original = await NativeRun.RoleAsync(root, Role("original"));
        if (verifyContinuousMedia)
        {
            Assert.AreEqual(0, original, "Original control failed: " + root);
            VerifyContinuousMedia(root, "original");
        }
        int candidate = await NativeRun.RoleAsync(root, Role("candidate"));
        Assert.AreEqual(0, original, "Original control failed: " + root);
        Assert.AreEqual(0, candidate, "Candidate failed: " + root);
        if (verifyContinuousMedia) VerifyContinuousMedia(root, "candidate");
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
