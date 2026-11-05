using System.Diagnostics;
using System.Text.Json;

internal static class SupervisorProbe
{
    internal static int Run(string root, bool videoOnly = false)
    {
        var results = new List<object>();
        bool passed = true;
        foreach (string mode in videoOnly ? new[] { "normal" } : new[] { "normal", "verify-failure-exit" })
        {
            using var child = new Process { StartInfo = new ProcessStartInfo(Path.Combine(root, "CandidateVideoSupervisor.exe")) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
            if (mode != "normal") child.StartInfo.ArgumentList.Add(mode);
            child.Start();
            var stdout = child.StandardOutput.ReadToEndAsync();
            var stderr = child.StandardError.ReadToEndAsync();
            bool timeout = !child.WaitForExit(180000);
            if (timeout) child.Kill(true);
            bool reaped = child.WaitForExit(10000);
            bool drained = Task.WaitAll(new Task[] { stdout, stderr }, 10000);
            int expected = mode == "normal" ? 0 : 1;
            int? exitCode = reaped ? child.ExitCode : null;
            bool valid = !timeout && reaped && drained && exitCode == expected;
            passed &= valid;
            string evidence = Path.Combine(root, "Evidence");
            File.Copy(Path.Combine(evidence, "process-results.json"), Path.Combine(evidence, mode + "-process-results.json"), true);
            if (drained)
            {
                File.WriteAllText(Path.Combine(evidence, mode + "-supervisor.stdout.txt"), stdout.Result);
                File.WriteAllText(Path.Combine(evidence, mode + "-supervisor.stderr.txt"), stderr.Result);
            }
            results.Add(new { mode, pid = child.Id, timeout, reaped, drained, expected, exitCode, passed = valid });
        }
        File.WriteAllText(Path.Combine(root, "Evidence", "supervisor-exit-probe.json"), JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
        return passed ? 0 : 1;
    }
}
