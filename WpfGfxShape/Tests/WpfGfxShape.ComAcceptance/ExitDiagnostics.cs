using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static class ExitDiagnostics
{
    internal static void Stage(string name)
    {
        string? path = Environment.GetEnvironmentVariable("WPFGFX_EXIT_STAGES");
        if (string.IsNullOrWhiteSpace(path)) return;
        using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.WriteLine(JsonSerializer.Serialize(new { Utc = DateTimeOffset.UtcNow, Pid = Environment.ProcessId, Thread = Environment.CurrentManagedThreadId, Stage = name }));
        writer.Flush();
        stream.Flush(true);
    }

    private static readonly System.Resources.ResourceManager Resources = new("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(ExitDiagnostics).Assembly);

    internal sealed record Options(string Root, int Seconds, string? Filter)
    {
        internal string Mode => Filter is null ? "full" : "filtered";
        internal string[] ChildArguments => [.. Filter is null ? Array.Empty<string>() : new[] { "--filter", Filter },
            "--report-trx", "--report-trx-filename", "child.trx", "--results-directory", Path.Combine(Root, "TestResults"),
            "--diagnostic", "--diagnostic-synchronous-write", "--diagnostic-output-directory", Path.Combine(Root, "Diagnostic"), "--no-progress"];
    }

    internal static Options Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length != 3) throw new ArgumentException(Resources.GetString("ExitArguments"), nameof(args));
        bool full = args[0] == "--full";
        string root = args[full ? 1 : 0];
        if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException(Resources.GetString("ExitArguments"), nameof(args));
        int seconds = int.Parse(args[full ? 2 : 1], CultureInfo.InvariantCulture);
        if (seconds < 1 || seconds > (full ? 1500 : 120))
            throw new ArgumentOutOfRangeException(nameof(args), Resources.GetString("ExitBudget"));
        if (!full && string.IsNullOrWhiteSpace(args[2]))
            throw new ArgumentException(Resources.GetString("ExitArguments"), nameof(args));
        return new Options(Path.GetFullPath(root), seconds, full ? null : args[2]);
    }

    internal static async Task<int> RunAsync(string[] args)
    {
        Options options = Parse(args);
        string root = options.Root;
        int seconds = options.Seconds;
        Directory.CreateDirectory(root);
        string executable = Environment.ProcessPath ?? throw new InvalidOperationException();
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true,
            WorkingDirectory = AppContext.BaseDirectory
        };
        if (Path.GetFileNameWithoutExtension(executable).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(NativeRun).Assembly.Location);
        foreach (string argument in options.ChildArguments)
            start.ArgumentList.Add(argument);
        start.Environment["WPFGFX_EXIT_STAGES"] = Path.Combine(root, "stages.jsonl");
        start.Environment["WPFGFX_RUN_ROOT"] = root;
        var record = new ExitRecord
        {
            StartedUtc = DateTimeOffset.UtcNow, SupervisorPid = Environment.ProcessId,
            Executable = executable, Arguments = start.ArgumentList.ToArray(), TimeoutSeconds = seconds, Mode = options.Mode,
            Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            TestAssembly = typeof(NativeRun).Assembly.Location,
            TestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(NativeRun).Assembly.Location))),
            TestMvid = typeof(NativeRun).Assembly.ManifestModule.ModuleVersionId
        };
        void Save()
        {
            string path = Path.Combine(root, "supervision.json");
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            JsonSerializer.Serialize(stream, record, new JsonSerializerOptions { WriteIndented = true });
            stream.Flush(true);
        }
        Save();
        using var process = new Process { StartInfo = start };
        using var cancel = new CancellationTokenSource();
        ConsoleCancelEventHandler onCancel = (_, e) => { e.Cancel = true; cancel.Cancel(); };
        Console.CancelKeyPress += onCancel;
        using var stdout = new FileStream(Path.Combine(root, "stdout.txt"), FileMode.Create, FileAccess.Write, FileShare.Read);
        using var stderr = new FileStream(Path.Combine(root, "stderr.txt"), FileMode.Create, FileAccess.Write, FileShare.Read);
        using var drainCancellation = new CancellationTokenSource();
        Task? output = null, error = null;
        try
        {
            if (!process.Start()) throw new InvalidOperationException();
            record.Pid = process.Id;
            Save();
            output = process.StandardOutput.BaseStream.CopyToAsync(stdout, drainCancellation.Token);
            error = process.StandardError.BaseStream.CopyToAsync(stderr, drainCancellation.Token);
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancel.Token);
            deadline.CancelAfter(TimeSpan.FromSeconds(seconds));
            try
            {
                await process.WaitForExitAsync(deadline.Token).ConfigureAwait(false);
                record.Reason = "exited";
            }
            catch (OperationCanceledException) when (deadline.IsCancellationRequested)
            {
                record.Cancelled = cancel.IsCancellationRequested;
                record.TimedOut = !record.Cancelled;
                record.Reason = record.Cancelled ? "cancelled" : "timeout";
                record.KillAttempted = true;
                Save();
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                record.KillReturned = true;
                using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await process.WaitForExitAsync(cleanup.Token).ConfigureAwait(false);
            }
            record.HasExited = process.HasExited;
            record.ExitCode = process.ExitCode;
            record.ExitHex = $"0x{unchecked((uint)process.ExitCode):X8}";
            try
            {
                await Task.WhenAll(output, error).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                record.OutputCollected = true;
            }
            catch (TimeoutException)
            {
                record.OutputDrainTimedOut = true;
                drainCancellation.Cancel();
                try { await Task.WhenAll(output, error).ConfigureAwait(false); }
                catch (OperationCanceledException) when (drainCancellation.IsCancellationRequested) { }
            }
            return record.Cancelled ? 130 : record.TimedOut || record.OutputDrainTimedOut ? 124 : process.ExitCode;
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or OperationCanceledException)
        {
            record.SupervisorError = exception.ToString();
            if (record.Pid.HasValue && !process.HasExited)
            {
                record.KillAttempted = true;
                process.Kill(entireProcessTree: true);
                record.KillReturned = true;
                using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await process.WaitForExitAsync(cleanup.Token).ConfigureAwait(false);
                record.HasExited = process.HasExited;
                record.ExitCode = process.ExitCode;
                record.ExitHex = $"0x{unchecked((uint)process.ExitCode):X8}";
            }
            throw;
        }
        finally
        {
            Console.CancelKeyPress -= onCancel;
            drainCancellation.Cancel();
            record.EndedUtc = DateTimeOffset.UtcNow;
            stdout.Flush(true);
            stderr.Flush(true);
            Save();
        }
    }

    private sealed class ExitRecord
    {
        public DateTimeOffset StartedUtc { get; set; }
        public DateTimeOffset? EndedUtc { get; set; }
        public int SupervisorPid { get; set; }
        public int? Pid { get; set; }
        public string Executable { get; set; } = "";
        public string[] Arguments { get; set; } = [];
        public string Mode { get; set; } = "";
        public int TimeoutSeconds { get; set; }
        public string Runtime { get; set; } = "";
        public string Architecture { get; set; } = "";
        public string TestAssembly { get; set; } = "";
        public string TestSha256 { get; set; } = "";
        public Guid TestMvid { get; set; }
        public string? Reason { get; set; }
        public bool TimedOut { get; set; }
        public bool Cancelled { get; set; }
        public bool KillAttempted { get; set; }
        public bool KillReturned { get; set; }
        public bool HasExited { get; set; }
        public int? ExitCode { get; set; }
        public string? ExitHex { get; set; }
        public bool OutputCollected { get; set; }
        public bool OutputDrainTimedOut { get; set; }
        public string? SupervisorError { get; set; }
    }
}
