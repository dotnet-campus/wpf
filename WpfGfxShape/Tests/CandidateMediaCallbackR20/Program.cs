using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Tests;

internal static class Program
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static void Save(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value, Options));
    private static object Identity(string path) => new { path, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) };

    private static async Task<int> Main(string[] args)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Evidence");
        Directory.CreateDirectory(root);
        if (args.Length == 1 && args[0] == "child") return await RunTestsAsync(root);
        if (args.Length != 0) return 64;
        string exe = Environment.ProcessPath!;
        using var child = new Process { StartInfo = new(exe) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = AppContext.BaseDirectory } };
        child.StartInfo.ArgumentList.Add("child");
        child.Start();
        Task<string> stdout = child.StandardOutput.ReadToEndAsync();
        Task<string> stderr = child.StandardError.ReadToEndAsync();
        bool timeout = false;
        string? killError = null;
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(150));
        try { await child.WaitForExitAsync(deadline.Token); }
        catch (OperationCanceledException)
        {
            timeout = true;
            try { child.Kill(entireProcessTree: true); }
            catch (InvalidOperationException error) { killError = error.ToString(); }
            catch (System.ComponentModel.Win32Exception error) { killError = error.ToString(); }
            await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));
        }
        string output = await stdout.WaitAsync(TimeSpan.FromSeconds(15));
        string errors = await stderr.WaitAsync(TimeSpan.FromSeconds(15));
        File.WriteAllText(Path.Combine(root, "child.stdout.txt"), output);
        File.WriteAllText(Path.Combine(root, "child.stderr.txt"), errors);
        int exit = !timeout && child.ExitCode == 0 && killError is null ? 0 : 1;
        Save(Path.Combine(root, "process-results.json"), new { parentPid = Environment.ProcessId, parentExit = exit, childPid = child.Id, childExit = child.ExitCode, timeout, reaped = child.HasExited, drained = stdout.IsCompletedSuccessfully && stderr.IsCompletedSuccessfully, killError, architecture = RuntimeInformation.ProcessArchitecture.ToString(), executable = Identity(exe) });
        Console.WriteLine($"parent exit={exit}; child exit={child.ExitCode}; timeout={timeout}; reaped={child.HasExited}");
        return exit;
    }

    private static async Task<int> RunTestsAsync(string root)
    {
        Environment.SetEnvironmentVariable("WPFGFX_RUN_ROOT", root);
        var test = new MediaFoundationResourceTests();
        var results = new List<object>();
        Save(Path.Combine(root, "identity.json"), new { runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, test = Identity(typeof(MediaFoundationResourceTests).Assembly.Location), testMvid = typeof(MediaFoundationResourceTests).Module.ModuleVersionId, implementation = Identity(Assembly.Load("WpfGfxShape.ManagedAcceptance").Location), coverage = "managed production assembly with real system MF callbacks; no app NativeAOT entry" });
        (string Name, Func<Task> Run)[] tests = [
            (nameof(test.WhenRealReadCallbackIsAdmittedThenCloseFlushesAndDrainsAsync), test.WhenRealReadCallbackIsAdmittedThenCloseFlushesAndDrainsAsync),
            (nameof(test.WhenAsyncReceiveRequestsCloseFromAnotherThreadThenAdmissionDoesNotHoldOwnerLockAsync), test.WhenAsyncReceiveRequestsCloseFromAnotherThreadThenAdmissionDoesNotHoldOwnerLockAsync),
            (nameof(test.WhenAsyncReaderReadsOriginalAviThenFortyFramesAndEosArePreservedAsync), test.WhenAsyncReaderReadsOriginalAviThenFortyFramesAndEosArePreservedAsync)
        ];
        int exit = 0;
        foreach (var entry in tests)
        {
            string? error = null;
            Console.WriteLine($"START {entry.Name}");
            var clock = Stopwatch.StartNew();
            try { await entry.Run(); }
            catch (Exception failure) when (failure is not OutOfMemoryException and not StackOverflowException)
            { error = failure.ToString(); exit = 1; Console.Error.WriteLine(error); }
            results.Add(new { name = entry.Name, passed = error is null, elapsedMs = clock.ElapsedMilliseconds, error });
            Save(Path.Combine(root, "result.json"), new { exit, results });
            Console.WriteLine($"END {entry.Name} passed={error is null}");
            if (error is not null) break;
        }
        using var process = Process.GetCurrentProcess();
        Save(Path.Combine(root, "resources.json"), new { process.Id, threads = process.Threads.Cast<ProcessThread>().Select(t => new { t.Id, state = t.ThreadState.ToString() }).ToArray(), modules = process.Modules.Cast<ProcessModule>().Where(m => m.ModuleName.Contains("mf", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).Select(m => new { m.ModuleName, m.FileName }).ToArray() });
        return exit;
    }
}
