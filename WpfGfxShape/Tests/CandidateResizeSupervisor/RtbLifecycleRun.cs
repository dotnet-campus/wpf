using System.Text.Json;

internal static class RtbLifecycleRun
{
    internal static bool Run(string package, string evidence, string candidateSha, string harnessSha, string harnessMvid)
    {
        using var child = new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(Path.Combine(package, "CandidateResize.exe")) { WorkingDirectory = package, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true } };
        child.StartInfo.ArgumentList.Add("candidate");
        child.StartInfo.ArgumentList.Add("rtb-lifecycle");
        child.Start();
        var stdout = child.StandardOutput.ReadToEndAsync();
        var stderr = child.StandardError.ReadToEndAsync();
        bool timeout = !child.WaitForExit(120000);
        string? killError = null;
        if (timeout)
        {
            try { child.Kill(true); }
            catch (InvalidOperationException error) { killError = error.ToString(); }
            catch (System.ComponentModel.Win32Exception error) { killError = error.ToString(); }
        }
        bool reaped = child.WaitForExit(10000);
        bool drained = Task.WaitAll([stdout, stderr], 10000);
        if (drained)
        {
            File.WriteAllText(Path.Combine(evidence, "rtb-lifecycle.stdout.txt"), stdout.Result);
            File.WriteAllText(Path.Combine(evidence, "rtb-lifecycle.stderr.txt"), stderr.Result);
        }
        var failures = new List<string>();
        if (timeout || !reaped || !drained || killError is not null) failures.Add("process-lifecycle-failed");
        if (!reaped || child.ExitCode != 0) failures.Add("business-process-failed");
        string raw = Path.Combine(evidence, "candidate-raw");
        Directory.CreateDirectory(raw);
        foreach (string file in Directory.GetFiles(Path.Combine(package, "Evidence"))) File.Copy(file, Path.Combine(raw, Path.GetFileName(file)), true);
        string result = Path.Combine(raw, "result.json");
        if (!File.Exists(result)) failures.Add("missing-result");
        else
        {
            using var document = JsonDocument.Parse(File.ReadAllText(result));
            var receipt = document.RootElement;
            if (receipt.GetProperty("harness").GetProperty("sha256").GetString() != harnessSha || receipt.GetProperty("mvid").GetString() != harnessMvid) failures.Add("harness-identity-mismatch");
            if (!receipt.TryGetProperty("actualNative", out var native) || native.GetProperty("sha256").GetString() != candidateSha) failures.Add("native-identity-mismatch");
            if (receipt.GetProperty("stage").GetString() != "rtb-lifecycle-completed") failures.Add("consumer-not-completed");
            if (!receipt.TryGetProperty("exitCode", out var exit) || exit.GetInt32() != 0) failures.Add("reported-business-failure");
        }
        if (reaped && child.ExitCode == 0)
        {
            using var events = JsonDocument.Parse(File.ReadAllText(Path.Combine(raw, "events.json")));
            using var resources = JsonDocument.Parse(File.ReadAllText(Path.Combine(raw, "resources.json")));
            var frames = new List<object>();
            for (int cycle = 0; cycle < 3; cycle++)
            {
                for (int frame = 0; frame < 4; frame++)
                {
                    int width = frame < 2 ? 24 : 40, height = frame < 2 ? 16 : 24;
                    string path = Path.Combine(raw, $"frame-{cycle}-{frame}-{width}x{height}.pbgra32");
                    byte[] pixels = File.ReadAllBytes(path);
                    byte[] expected = frame switch { 0 => [0, 0, 255, 255], 1 => [255, 0, 0, 255], 2 => [0, 255, 0, 255], _ => [0, 255, 255, 255] };
                    bool exact = pixels.Length == width * height * 4;
                    for (int offset = 0; offset < pixels.Length; offset++) exact &= pixels[offset] == expected[offset % 4];
                    if (!exact) failures.Add($"raw-frame-mismatch-{cycle}-{frame}");
                    frames.Add(new { cycle, frame, width, height, bytes = pixels.Length, exact, sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pixels)) });
                }
                var closed = events.RootElement.EnumerateArray().Single(x => x.GetProperty("cycle").GetInt32() == cycle && x.GetProperty("operation").GetString() == "close-drained");
                if (closed.GetProperty("closed").GetInt32() != 1 || closed.GetProperty("callbacks").GetInt32() == 0 || closed.GetProperty("callbacks").GetInt32() != closed.GetProperty("callbacksAtUnsubscribe").GetInt32()) failures.Add($"callbacks-not-converged-{cycle}");
                if (resources.RootElement[cycle].GetProperty("producerAlive").GetBoolean()) failures.Add($"producer-alive-{cycle}");
            }
            File.WriteAllText(Path.Combine(evidence, "rtb-raw-verification.json"), JsonSerializer.Serialize(new { frames, resources = resources.RootElement.Clone(), comRefs = "Not measured by original RunCycle; no COM refs0 certification" }, new JsonSerializerOptions { WriteIndented = true }));
        }
        File.WriteAllText(Path.Combine(evidence, "rtb-lifecycle-process.json"), JsonSerializer.Serialize(new { pid = child.Id, timeout, reaped, drained, killError, exitCode = reaped ? (int?)child.ExitCode : null, passed = failures.Count == 0, scope = "RTB content and window lifecycle only; desktop/nativeControl/full/x86 NOT certified", failures }, new JsonSerializerOptions { WriteIndented = true }));
        return failures.Count == 0;
    }
}
