using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Threading;
using System.Xml.Linq;

namespace WpfGfxShape.ComAcceptance;

internal static class WindowSequenceRoleIsolation
{
    private const string RoleVariable = "KA_WINDOW_SEQUENCE_ROLE";
    private const string MethodVariable = "KA_WINDOW_SEQUENCE_METHOD";

    internal static bool ExecuteRole(string method, Func<string, int> scene)
    {
        string? role = Environment.GetEnvironmentVariable(RoleVariable);
        string? selectedMethod = Environment.GetEnvironmentVariable(MethodVariable);
        if (role is null && selectedMethod is null) return false;
        Assert.AreEqual(method, selectedMethod);
        Assert.IsTrue(role is "original" or "candidate");
        string output = NativeRun.PathsRoot;
        Directory.CreateDirectory(output);
        int exit = -1;
        try
        {
            exit = scene(role);
            Assert.AreEqual(0, exit, output);
        }
        finally
        {
            File.WriteAllText(Path.Combine(output, "window-role.json"), JsonSerializer.Serialize(new
            {
                method, role, executed = true, pid = Environment.ProcessId, exit,
                runtime = RuntimeInformation.FrameworkDescription,
                testSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(WindowSequenceRoleIsolation).Assembly.Location))),
                mvid = typeof(WindowSequenceRoleIsolation).Module.ModuleVersionId,
                arguments = Environment.GetCommandLineArgs()
            }));
        }
        return true;
    }

    internal static async Task RunAsync(string root, string method, string role)
    {
        string output = Path.Combine(root, role);
        Directory.CreateDirectory(output);
        string filter = "FullyQualifiedName=WpfGfxShape.ComAcceptance.ReplacementStateSequenceWindowTests." + method;
        var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException())
        {
            UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        if (Path.GetFileNameWithoutExtension(start.FileName).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(NativeRun).Assembly.Location);
        foreach (string argument in new[] { "--supervise-exit", output, "65", filter }) start.ArgumentList.Add(argument);
        start.Environment[RoleVariable] = role;
        start.Environment[MethodVariable] = method;
        start.Environment["WPFGFX_CANDIDATE_DLL"] = AcceptancePaths.AotDll;
        start.Environment["WPFGFX_ORIGINAL_DLL"] = AcceptancePaths.OriginalDll;
        using var process = new Process { StartInfo = start };
        using var drain = new CancellationTokenSource();
        using var stdout = File.Create(Path.Combine(output, "supervisor-stdout.txt"));
        using var stderr = File.Create(Path.Combine(output, "supervisor-stderr.txt"));
        Task? copies = null;
        int? pid = null, exit = null;
        bool timedOut = false, killed = false, waitTimedOut = false, drained = false, drainCancelTimedOut = false;
        var errors = new List<string>();
        try
        {
            if (!process.Start()) throw new InvalidOperationException();
            pid = process.Id;
            copies = Task.WhenAll(process.StandardOutput.BaseStream.CopyToAsync(stdout, drain.Token), process.StandardError.BaseStream.CopyToAsync(stderr, drain.Token));
            try { await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(75)).ConfigureAwait(false); }
            catch (TimeoutException) { timedOut = true; }
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
        {
            errors.Add(exception.ToString());
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
                        catch (TimeoutException) { drainCancelTimedOut = true; }
                    }
                }
            }
            catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or OperationCanceledException)
            {
                errors.Add(exception.ToString());
            }
            finally
            {
                drain.Cancel();
                File.WriteAllText(Path.Combine(output, "parent-role-exit.json"), JsonSerializer.Serialize(new
                {
                    method, role, parentPid = Environment.ProcessId, supervisorPid = pid, exit,
                    timedOut, killed, waitTimedOut, drained, drainCancelTimedOut, errors, arguments = start.ArgumentList.ToArray()
                }));
            }
        }
        string content = Path.Combine(output, role);
        if (Directory.Exists(content))
            foreach (string file in Directory.GetFiles(content)) File.Copy(file, Path.Combine(output, Path.GetFileName(file)), overwrite: true);
    }

    internal static void Validate(string root, string method, string role)
    {
        string output = Path.Combine(root, role);
        using var parent = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "parent-role-exit.json")));
        JsonElement p = parent.RootElement;
        Assert.AreEqual(0, p.GetProperty("errors").GetArrayLength(), output);
        foreach (string flag in new[] { "timedOut", "killed", "waitTimedOut", "drainCancelTimedOut" }) Assert.IsFalse(p.GetProperty(flag).GetBoolean(), output);
        Assert.IsTrue(p.GetProperty("drained").GetBoolean(), output);
        Assert.AreEqual(0, p.GetProperty("exit").GetInt32(), output);
        using var supervision = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "supervision.json")));
        JsonElement s = supervision.RootElement;
        Assert.IsTrue(s.GetProperty("HasExited").GetBoolean(), output);
        Assert.IsTrue(s.GetProperty("OutputCollected").GetBoolean(), output);
        using var receipt = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "window-role.json")));
        JsonElement r = receipt.RootElement;
        Assert.AreEqual(method, r.GetProperty("method").GetString(), output);
        Assert.AreEqual(role, r.GetProperty("role").GetString(), output);
        Assert.IsTrue(r.GetProperty("executed").GetBoolean(), output);
        Assert.AreEqual(0, r.GetProperty("exit").GetInt32(), output);
        Assert.AreEqual(s.GetProperty("Pid").GetInt32(), r.GetProperty("pid").GetInt32(), output);
        Assert.AreNotEqual(Environment.ProcessId, r.GetProperty("pid").GetInt32(), output);
        Assert.AreNotEqual(p.GetProperty("supervisorPid").GetInt32(), r.GetProperty("pid").GetInt32(), output);
        XDocument trx = XDocument.Load(Path.Combine(output, "TestResults", "child.trx"));
        XNamespace ns = trx.Root!.Name.Namespace;
        XElement counters = trx.Descendants(ns + "Counters").Single();
        Assert.AreEqual("1", (string?)counters.Attribute("executed"), output);
        Assert.AreEqual("1", (string?)counters.Attribute("passed"), output);
        Assert.AreEqual("Passed", (string?)trx.Descendants(ns + "UnitTestResult").Single().Attribute("outcome"), output);
        XElement definition = trx.Descendants(ns + "TestMethod").Single();
        Assert.AreEqual(method, (string?)definition.Attribute("name"), output);
        Assert.AreEqual("WpfGfxShape.ComAcceptance.ReplacementStateSequenceWindowTests", (string?)definition.Attribute("className"), output);
    }

    internal static int RunControl(string role)
    {
        string selected = role == "original" ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(selected);
        using Process process = Process.GetCurrentProcess();
        string actual = process.Modules.Cast<ProcessModule>().Single(item => item.BaseAddress == module).FileName;
        File.WriteAllText(Path.Combine(NativeRun.PathsRoot, "control-native.json"), JsonSerializer.Serialize(new
        {
            role, pid = Environment.ProcessId, selected, actual,
            sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))),
            runtime = RuntimeInformation.FrameworkDescription
        }));
        Assert.AreEqual(Path.GetFullPath(selected), Path.GetFullPath(actual), ignoreCase: true);
        // The module stays resident, just as in the content roles; no parent resolver or dispatcher is touched.
        return 0;
    }
}
