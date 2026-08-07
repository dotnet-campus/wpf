using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.AbiIntegration;

[TestClass]
public sealed class NativeAotAbiIntegrationTests
{
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromMinutes(10);
    private static readonly string RuntimeIdentifier = GetRuntimeIdentifier();

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow("Debug")]
    [DataRow("Release")]
    public async Task WhenPublishedLibraryIsInvokedThenProtocolPasses(string configuration)
    {
        PublishedLibrary library = await PublishAsync(configuration).ConfigureAwait(false);

        HostExecution execution = await RunHostAsync("protocol", library.Path, library.Sha256, configuration).ConfigureAwait(false);

        Assert.AreEqual(0, execution.ExitCode, execution.Diagnostics);
        Assert.IsNotNull(execution.Result, execution.Diagnostics);
        Assert.IsTrue(string.Equals(Path.GetFullPath(library.Path), execution.Result.ExpectedModulePath, StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(string.Equals(Path.GetFullPath(library.Path), execution.Result.ActualModulePath, StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(string.Equals(library.Sha256, execution.Result.ActualSha256, StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    [DataRow("Debug")]
    [DataRow("Release")]
    public async Task WhenPublishedLibraryIsInvokedConcurrentlyThenCallsRemainStable(string configuration)
    {
        PublishedLibrary library = await PublishAsync(configuration).ConfigureAwait(false);

        HostExecution execution = await RunHostAsync("concurrent", library.Path, library.Sha256, configuration).ConfigureAwait(false);

        Assert.AreEqual(0, execution.ExitCode, execution.Diagnostics);
    }

    [TestMethod]
    public async Task WhenExportIsMissingThenIsolatedHostReportsExpectedFailure()
    {
        PublishedLibrary library = await PublishAsync("Release").ConfigureAwait(false);

        HostExecution execution = await RunHostAsync("missing-export", library.Path, library.Sha256, "Release").ConfigureAwait(false);

        Assert.AreEqual(0, execution.ExitCode, execution.Diagnostics);
    }

    [TestMethod]
    public async Task WhenLibraryIsMissingThenIsolatedHostReportsExpectedFailure()
    {
        string missingPath = Path.Combine(GetResultsDirectory(), "missing", "wpfgfx_cor3.dll");

        HostExecution execution = await RunHostAsync("missing-dll", missingPath, string.Empty, GetCurrentConfiguration()).ConfigureAwait(false);

        Assert.AreEqual(0, execution.ExitCode, execution.Diagnostics);
    }

    [TestMethod]
    public async Task WhenLibraryArchitectureDoesNotMatchThenIsolatedHostReportsExpectedFailure()
    {
        PublishedLibrary library = await PublishAsync("Release").ConfigureAwait(false);
        string wrongArchitecturePath = Path.Combine(GetResultsDirectory(), "wrong-architecture", "wpfgfx_cor3.dll");
        Directory.CreateDirectory(Path.GetDirectoryName(wrongArchitecturePath)!);
        await CreateWrongArchitecturePeVariantAsync(library.Path, wrongArchitecturePath).ConfigureAwait(false);

        HostExecution execution = await RunHostAsync("bad-image", wrongArchitecturePath, CalculateSha256(wrongArchitecturePath), "Release").ConfigureAwait(false);

        Assert.AreEqual(0, execution.ExitCode, execution.Diagnostics);
    }

    [TestMethod]
    public async Task WhenLibraryIsNotAPortableExecutableThenIsolatedHostReportsExpectedFailure()
    {
        string invalidLibraryPath = Path.Combine(GetResultsDirectory(), "bad-image", "wpfgfx_cor3.dll");
        Directory.CreateDirectory(Path.GetDirectoryName(invalidLibraryPath)!);
        await File.WriteAllTextAsync(invalidLibraryPath, "not a portable executable").ConfigureAwait(false);

        HostExecution execution = await RunHostAsync("bad-image", invalidLibraryPath, CalculateSha256(invalidLibraryPath), GetCurrentConfiguration()).ConfigureAwait(false);

        Assert.AreEqual(0, execution.ExitCode, execution.Diagnostics);
    }

    private static async Task CreateWrongArchitecturePeVariantAsync(string sourcePath, string destinationPath)
    {
        byte[] image = await File.ReadAllBytesAsync(sourcePath).ConfigureAwait(false);
        if (image.Length < 0x40 || image[0] != (byte)'M' || image[1] != (byte)'Z')
        {
            throw new InvalidDataException("Published library does not contain a valid DOS header.");
        }

        int peHeaderOffset = BitConverter.ToInt32(image, 0x3c);
        int machineOffset = peHeaderOffset + 4;
        if (peHeaderOffset < 0 || machineOffset + sizeof(ushort) > image.Length ||
            image[peHeaderOffset] != (byte)'P' || image[peHeaderOffset + 1] != (byte)'E')
        {
            throw new InvalidDataException("Published library does not contain a valid PE header.");
        }

        const ushort amd64Machine = 0x8664;
        const ushort arm64Machine = 0xaa64;
        ushort wrongMachine = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? amd64Machine
            : arm64Machine;
        BitConverter.TryWriteBytes(image.AsSpan(machineOffset, sizeof(ushort)), wrongMachine);
        await File.WriteAllBytesAsync(destinationPath, image).ConfigureAwait(false);
    }

    private static async Task<PublishedLibrary> PublishAsync(string configuration)
    {
        string repositoryRoot = FindRepositoryRoot();
        string projectPath = Path.Combine(repositoryRoot, "Code", "WpfGfxShape", "WpfGfxShape.csproj");
        string publishDirectory = Path.Combine(repositoryRoot, "artifacts", "publish", "WpfGfxShape", configuration, RuntimeIdentifier);
        string libraryPath = Path.Combine(publishDirectory, "wpfgfx_cor3.dll");

        ProcessExecution restore = await RunDotNetAsync(
            repositoryRoot,
            ["restore", projectPath, "--runtime", RuntimeIdentifier]).ConfigureAwait(false);
        Assert.AreEqual(0, restore.ExitCode, restore.Diagnostics);

        ProcessExecution publish = await RunDotNetAsync(
            repositoryRoot,
            [
                "publish",
                projectPath,
                "--configuration", configuration,
                "--runtime", RuntimeIdentifier,
                "--self-contained", "true",
                "--no-restore",
                "--output", publishDirectory,
            ]).ConfigureAwait(false);
        Assert.AreEqual(0, publish.ExitCode, publish.Diagnostics);
        Assert.IsTrue(File.Exists(libraryPath), $"Published DLL was not found at '{libraryPath}'.");

        return new PublishedLibrary(libraryPath, CalculateSha256(libraryPath));
    }

    private async Task<HostExecution> RunHostAsync(string caseName, string libraryPath, string expectedSha256, string configuration)
    {
        string repositoryRoot = FindRepositoryRoot();
        string hostProjectPath = Path.Combine(repositoryRoot, "Tests", "WpfGfxShape.AbiIntegration.Host", "WpfGfxShape.AbiIntegration.Host.csproj");
        string resultPath = Path.Combine(GetResultsDirectory(), configuration, caseName, "result.json");
        Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);

        ProcessExecution process = await RunDotNetAsync(
            repositoryRoot,
            [
                "run",
                "--project", hostProjectPath,
                "--configuration", GetCurrentConfiguration(),
                "--no-build",
                "--",
                "--case", caseName,
                "--dll", libraryPath,
                "--expected-sha256", expectedSha256,
                "--result", resultPath,
            ]).ConfigureAwait(false);

        HostResult? result = File.Exists(resultPath)
            ? JsonSerializer.Deserialize<HostResult>(await File.ReadAllTextAsync(resultPath).ConfigureAwait(false))
            : null;

        return new HostExecution(process.ExitCode, process.Diagnostics, result);
    }

    private static async Task<ProcessExecution> RunDotNetAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        ProcessStartInfo startInfo = new("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start dotnet process.");
        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();

        using CancellationTokenSource timeout = new(ProcessTimeout);
        try
        {
            await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"dotnet process exceeded {ProcessTimeout}.");
        }

        return new ProcessExecution(
            process.ExitCode,
            await standardOutput.ConfigureAwait(false),
            await standardError.ConfigureAwait(false));
    }

    private string GetResultsDirectory()
    {
        string directory = Path.Combine(
            FindRepositoryRoot(),
            "artifacts",
            "test-results",
            "MANAGED-PINVOKE-ABI-INTEGRATION",
            RuntimeIdentifier,
            TestContext.TestName ?? "unknown");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string GetCurrentConfiguration()
    {
        string baseDirectory = AppContext.BaseDirectory;
        return baseDirectory.Contains($"{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            ? "Release"
            : "Debug";
    }

    private static string GetRuntimeIdentifier()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "win-x64",
            Architecture.Arm64 => "win-arm64",
            Architecture.X86 => "win-x86",
            Architecture.Arm => throw new PlatformNotSupportedException("Windows ARM32 is not part of the wpfgfx architecture matrix."),
            _ => throw new PlatformNotSupportedException($"Unsupported process architecture '{RuntimeInformation.ProcessArchitecture}'."),
        };
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WpfGfxShape.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the WpfGfxShape repository root.");
    }

    private static string CalculateSha256(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private sealed record PublishedLibrary(string Path, string Sha256);

    private sealed record ProcessExecution(int ExitCode, string StandardOutput, string StandardError)
    {
        internal string Diagnostics => $"stdout:{Environment.NewLine}{StandardOutput}{Environment.NewLine}stderr:{Environment.NewLine}{StandardError}";
    }

    private sealed record HostExecution(int ExitCode, string Diagnostics, HostResult? Result);

    private sealed record HostResult(
        bool Passed,
        string? ExpectedModulePath,
        string? ActualModulePath,
        string? ActualSha256,
        string? Error);
}
