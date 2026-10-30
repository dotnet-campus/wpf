using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

namespace WpfGfxShape.ComAcceptance;

internal static class JmResolverIsolation
{
    internal static async Task RunAsync(string root, string method, string role)
    {
        string output = Path.Combine(root, role);
        Directory.CreateDirectory(output);
        string filter = "FullyQualifiedName=WpfGfxShape.ComAcceptance.RoundedRectangleContractTests." + method;
        var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException())
        {
            UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        if (Path.GetFileNameWithoutExtension(start.FileName).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(NativeRun).Assembly.Location);
        foreach (string argument in new[] { "--supervise-exit", output, "35", filter }) start.ArgumentList.Add(argument);
        start.Environment["JM_ROUNDED_ROLE"] = role;
        start.Environment["JM_ROUNDED_METHOD"] = method;
        using var process = new Process { StartInfo = start };
        using var drain = new CancellationTokenSource();
        using var stdout = File.Create(Path.Combine(output, "supervisor-stdout.txt"));
        using var stderr = File.Create(Path.Combine(output, "supervisor-stderr.txt"));
        int? pid = null, exit = null;
        bool timedOut = false, killed = false, waitTimedOut = false, drained = false;
        Task? copies = null;
        try
        {
            if (!process.Start()) throw new InvalidOperationException();
            pid = process.Id;
            copies = Task.WhenAll(process.StandardOutput.BaseStream.CopyToAsync(stdout, drain.Token), process.StandardError.BaseStream.CopyToAsync(stderr, drain.Token));
            try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(45)).ConfigureAwait(false); }
            catch (TimeoutException) { timedOut = true; }
        }
        finally
        {
            try
            {
                if (pid.HasValue && !process.HasExited)
                {
                    killed = true;
                    process.Kill(entireProcessTree: true);
                    try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); }
                    catch (TimeoutException) { waitTimedOut = true; }
                }
                if (pid.HasValue && process.HasExited) exit = process.ExitCode;
                if (copies is not null)
                {
                    try { await copies.WaitAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(false); drained = true; }
                    catch (TimeoutException)
                    {
                        drain.Cancel();
                        try { await copies.WaitAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(false); }
                        catch (OperationCanceledException) when (drain.IsCancellationRequested) { }
                    }
                }
            }
            finally
            {
                drain.Cancel();
                File.WriteAllText(Path.Combine(output, "parent-role-exit.json"), JsonSerializer.Serialize(new { method, role, parentPid = Environment.ProcessId, supervisorPid = pid, exit, timedOut, killed, waitTimedOut, drained }));
            }
        }
        Assert.IsFalse(timedOut || killed || waitTimedOut, output);
        Assert.IsTrue(drained, output);
        Assert.AreEqual(0, exit, output);
        using var supervision = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "supervision.json")));
        JsonElement record = supervision.RootElement;
        Assert.AreEqual(filter, record.GetProperty("Arguments")[1].GetString());
        Assert.IsTrue(record.GetProperty("HasExited").GetBoolean());
        Assert.IsTrue(record.GetProperty("OutputCollected").GetBoolean());
        using var identity = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, role, "identity.json")));
        int businessPid = identity.RootElement.GetProperty("pid").GetInt32();
        Assert.AreEqual(record.GetProperty("Pid").GetInt32(), businessPid);
        Assert.AreNotEqual(Environment.ProcessId, businessPid);
        Assert.AreNotEqual(pid, (int?)businessPid);
        var trx = XDocument.Load(Path.Combine(output, "TestResults", "child.trx"));
        XNamespace ns = trx.Root!.Name.Namespace;
        XElement counters = trx.Descendants(ns + "Counters").Single();
        Assert.AreEqual("1", (string?)counters.Attribute("executed"));
        Assert.AreEqual("1", (string?)counters.Attribute("passed"));
        Assert.AreEqual("Passed", (string?)trx.Descendants(ns + "UnitTestResult").Single().Attribute("outcome"));
        Assert.AreEqual(method, (string?)trx.Descendants(ns + "TestMethod").Single().Attribute("name"));
        foreach (string file in Directory.GetFiles(Path.Combine(output, role)))
            File.Copy(file, Path.Combine(output, Path.GetFileName(file)), overwrite: true);
    }
}
