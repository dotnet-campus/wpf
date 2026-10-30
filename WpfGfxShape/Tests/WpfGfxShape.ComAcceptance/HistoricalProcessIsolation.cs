using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

namespace WpfGfxShape.ComAcceptance;

internal static class HistoricalProcessIsolation
{
    internal static async Task RunAsync(string method, Action scene)
    {
        string? role = Environment.GetEnvironmentVariable("JH_HISTORICAL_ROLE");
        if (role is not null)
        {
            Assert.AreEqual(method, role);
            File.WriteAllText(Path.Combine(NativeRun.PathsRoot, "historical-role.json"), JsonSerializer.Serialize(new { method, pid = Environment.ProcessId, arguments = Environment.GetCommandLineArgs() }));
            scene();
            return;
        }

        string root = Path.GetFullPath(Path.Combine(NativeRun.PathsRoot, "historical-isolation", method, Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(root);
        string filter = "FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests." + method;
        var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException())
        {
            UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        if (Path.GetFileNameWithoutExtension(start.FileName).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(NativeRun).Assembly.Location);
        foreach (string argument in new[] { "--supervise-exit", root, "90", filter }) start.ArgumentList.Add(argument);
        start.Environment["JH_HISTORICAL_ROLE"] = method;
        using var process = new Process { StartInfo = start };
        using var drain = new CancellationTokenSource();
        int? supervisorPid = null, exitCode = null;
        bool timedOut = false, drained = false, killAttempted = false, killReturned = false, waitTimedOut = false, drainCancelTimedOut = false;
        var errors = new List<string>();
        Task? copies = null;
        FileStream? stdout = null, stderr = null;
        try
        {
            if (!process.Start()) throw new InvalidOperationException();
            supervisorPid = process.Id;
            stdout = File.Create(Path.Combine(root, "supervisor-stdout.txt"));
            stderr = File.Create(Path.Combine(root, "supervisor-stderr.txt"));
            copies = Task.WhenAll(process.StandardOutput.BaseStream.CopyToAsync(stdout, drain.Token), process.StandardError.BaseStream.CopyToAsync(stderr, drain.Token));
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(110));
            try { await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false); }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested) { timedOut = true; }
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException)
        {
            errors.Add(exception.ToString());
        }
        finally
        {
            try
            {
                if (supervisorPid.HasValue && !process.HasExited)
                {
                    killAttempted = true;
                    try { process.Kill(entireProcessTree: true); killReturned = true; }
                    catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception) { errors.Add(exception.ToString()); }
                    using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    try { await process.WaitForExitAsync(cleanup.Token).ConfigureAwait(false); }
                    catch (OperationCanceledException) when (cleanup.IsCancellationRequested) { waitTimedOut = true; }
                }
                if (supervisorPid.HasValue && process.HasExited) exitCode = process.ExitCode;
                if (copies is not null)
                {
                    try { await copies.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); drained = true; }
                    catch (TimeoutException)
                    {
                        drain.Cancel();
                        try { await copies.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); }
                        catch (OperationCanceledException) when (drain.IsCancellationRequested) { }
                        catch (TimeoutException) { drainCancelTimedOut = true; }
                    }
                }
            }
            catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or UnauthorizedAccessException or OperationCanceledException)
            {
                errors.Add(exception.ToString());
            }
            finally
            {
                drain.Cancel();
                foreach (FileStream? stream in new[] { stdout, stderr })
                {
                    try { stream?.Dispose(); }
                    catch (IOException exception) { errors.Add(exception.ToString()); }
                }
                File.WriteAllText(Path.Combine(root, "parent-role-exit.json"), JsonSerializer.Serialize(new { method, parentPid = Environment.ProcessId, supervisorPid, exitCode, timedOut, drained, killAttempted, killReturned, waitTimedOut, drainCancelTimedOut, errors, arguments = start.ArgumentList.ToArray() }));
            }
        }
        string supervisionPath = Path.Combine(root, "supervision.json");
        string supervision = File.Exists(supervisionPath) ? File.ReadAllText(supervisionPath) : "missing supervision";
        string evidence = root + Environment.NewLine + supervision;
        Assert.IsEmpty(errors, evidence + string.Join(Environment.NewLine, errors));
        Assert.IsFalse(timedOut || waitTimedOut || drainCancelTimedOut, evidence);
        Assert.IsTrue(drained, evidence);
        Assert.AreEqual(0, exitCode, evidence);
        using var record = JsonDocument.Parse(supervision);
        Assert.AreEqual(filter, record.RootElement.GetProperty("Arguments")[1].GetString(), evidence);
        Assert.IsTrue(record.RootElement.GetProperty("HasExited").GetBoolean(), evidence);
        Assert.IsTrue(record.RootElement.GetProperty("OutputCollected").GetBoolean(), evidence);
        string receiptPath = Path.Combine(root, "historical-role.json");
        Assert.IsTrue(File.Exists(receiptPath), evidence);
        using var receipt = JsonDocument.Parse(File.ReadAllText(receiptPath));
        Assert.AreEqual(method, receipt.RootElement.GetProperty("method").GetString(), evidence);
        Assert.AreEqual(record.RootElement.GetProperty("Pid").GetInt32(), receipt.RootElement.GetProperty("pid").GetInt32(), evidence);
        string trxPath = Path.Combine(root, "TestResults", "child.trx");
        Assert.IsTrue(File.Exists(trxPath), evidence);
        XDocument trx = XDocument.Load(trxPath);
        XNamespace ns = trx.Root!.Name.Namespace;
        XElement counters = trx.Descendants(ns + "Counters").Single();
        Assert.AreEqual("1", (string?)counters.Attribute("executed"), evidence);
        Assert.AreEqual("1", (string?)counters.Attribute("passed"), evidence);
        XElement result = trx.Descendants(ns + "UnitTestResult").Single();
        Assert.AreEqual("Passed", (string?)result.Attribute("outcome"), evidence);
        XElement definition = trx.Descendants(ns + "TestMethod").Single();
        Assert.AreEqual(method, (string?)definition.Attribute("name"), evidence);
        Assert.AreEqual("WpfGfxShape.ComAcceptance.D3DImageTests", (string?)definition.Attribute("className"), evidence);
    }
}
