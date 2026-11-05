using System.Diagnostics;
using System.Text.Json;

internal static class VideoRun
{
    internal static bool Run(string package, string evidence, string candidateSha, string harnessSha, string harnessMvid)
    {
        using var child = new Process { StartInfo = new ProcessStartInfo(Path.Combine(package, "CandidatePureVideo.exe")) { WorkingDirectory = package, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
        child.StartInfo.ArgumentList.Add("candidate");
        child.StartInfo.ArgumentList.Add(SceneSelection.Name);
        child.Start();
        int pid = child.Id;
        Task<string> stdout = child.StandardOutput.ReadToEndAsync(), stderr = child.StandardError.ReadToEndAsync();
        bool timeout = !child.WaitForExit(120000);
        string? killError = null;
        if (timeout)
        {
            try { child.Kill(true); }
            catch (InvalidOperationException error) { killError = error.ToString(); }
            catch (System.ComponentModel.Win32Exception error) { killError = error.ToString(); }
        }
        bool reaped = child.WaitForExit(10000);
        bool drained = Task.WaitAll(new Task[] { stdout, stderr }, 10000);
        if (drained)
        {
            File.WriteAllText(Path.Combine(evidence, "candidate-video.stdout.txt"), stdout.Result);
            File.WriteAllText(Path.Combine(evidence, "candidate-video.stderr.txt"), stderr.Result);
        }
        var failures = new List<string>();
        if (timeout || !reaped || !drained || killError is not null) failures.Add("process-lifecycle-failed");
        if (!reaped || child.ExitCode != 0) failures.Add("business-process-failed");
        string result = Path.Combine(package, "Evidence", "result.json");
        if (!File.Exists(result)) failures.Add("missing-result");
        else
        {
            File.Copy(result, Path.Combine(evidence, "candidate-video.json"), true);
            string raw = Path.Combine(evidence, "candidate-raw");
            Directory.CreateDirectory(raw);
            foreach (string file in Directory.GetFiles(Path.GetDirectoryName(result)!)) File.Copy(file, Path.Combine(raw, Path.GetFileName(file)), true);
            using var document = JsonDocument.Parse(File.ReadAllText(result));
            var receipt = document.RootElement;
            if (receipt.GetProperty("harness").GetProperty("sha256").GetString() != harnessSha || receipt.GetProperty("mvid").GetString() != harnessMvid) failures.Add("harness-identity-mismatch");
            if (!receipt.TryGetProperty("actualNative", out var native) || native.GetProperty("sha256").GetString() != candidateSha) failures.Add("native-identity-mismatch");
            if (receipt.GetProperty("stage").GetString() != SceneSelection.Completed) failures.Add("consumer-not-completed");
            if (!receipt.TryGetProperty("exitCode", out var exit) || exit.GetInt32() != 0) failures.Add("reported-business-failure");
        }
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(Path.Combine(evidence, "candidate-video-process.json"), JsonSerializer.Serialize(new { pid, timeout, reaped, drained, killError, exitCode = reaped ? (int?)child.ExitCode : null, passed = failures.Count == 0, failures }, options));
        return failures.Count == 0;
    }
}
