#if DC_FULL_RESULT_RECOVERY
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class DcFullResultRecoveryTests
{
    /// <summary>Copies only allowlisted evidence from existing DB full tasks without changing those tasks.</summary>
    [TestMethod]
    public async Task WhenExplicitRecoveryIsRequestedThenExistingDbFullEvidenceIsCopiedAsync()
    {
        const string tasks = @"C:\CodingAgentSandbox\Tasks";
        string destination = Path.Combine(AppContext.BaseDirectory, "Results", "dc-full-recovery");
        Directory.CreateDirectory(destination);
        var entries = new List<object>();
        var watch = Stopwatch.StartNew();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        int scanned = 0, matched = 0, copied = 0;
        long bytes = 0;
        string status = "complete";
        try
        {
            if (!Directory.Exists(tasks))
            {
                status = "task-root-not-visible";
                return;
            }
            foreach (string task in Directory.EnumerateDirectories(tasks, "*", SearchOption.TopDirectoryOnly))
            {
                deadline.Token.ThrowIfCancellationRequested();
                if (++scanned > 4096) { status = "task-limit"; break; }
                string id = Path.GetFileName(task);
                if (!Guid.TryParseExact(id, "N", out _) || IsLink(task)) continue;
                foreach (string arch in new[] { "x64", "x86" })
                {
                    string root = Path.Combine(task, "Results", "db-full-" + arch);
                    if (IsLink(Path.Combine(task, "Results")) || !Directory.Exists(root) || IsLink(root)) continue;
                    matched++;
                    entries.Add(new { TaskId = id, Architecture = arch, Source = root, Status = "matched" });
                    string[] paths =
                    [
                        $"Results/db-full-{arch}/supervision.json", $"Results/db-full-{arch}/stdout.txt",
                        $"Results/db-full-{arch}/stderr.txt", $"Results/db-full-{arch}/stages.jsonl",
                        $"Results/db-full-{arch}/TestResults/child.trx", "Results/aot-identity.json",
                        "Results/az-reference-consumption.json", "aot.sha256", "package-files.txt",
                        "artifacts/bw-curved-fill/precision/candidate/identity.json",
                        "artifacts/bw-curved-fill/precision/official/identity.json"
                    ];
                    foreach (string relative in paths)
                    {
                        deadline.Token.ThrowIfCancellationRequested();
                        string source = Path.Combine(task, relative.Replace('/', Path.DirectorySeparatorChar));
                        string parent = task;
                        bool link = false;
                        foreach (string part in relative.Split('/'))
                        {
                            parent = Path.Combine(parent, part);
                            if (Path.Exists(parent) && IsLink(parent)) { link = true; break; }
                        }
                        if (link) { entries.Add(new { TaskId = id, Source = source, Status = "link-rejected" }); continue; }
                        if (!File.Exists(source)) { entries.Add(new { TaskId = id, Source = source, Status = "missing" }); continue; }
                        try
                        {
                            await using var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 81920, true);
                            long length = input.Length;
                            if (length > 32 * 1024 * 1024 || bytes + length > 256 * 1024 * 1024 || copied >= 256)
                            {
                                entries.Add(new { TaskId = id, Source = source, Status = "size-or-count-limit", Length = length });
                                continue;
                            }
                            string target = Path.Combine(destination, id, relative.Replace('/', Path.DirectorySeparatorChar));
                            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? destination);
                            await using (var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
                            {
                                byte[] buffer = new byte[81920];
                                long remaining = length;
                                while (remaining > 0)
                                {
                                    int read = await input.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)), deadline.Token).ConfigureAwait(false);
                                    if (read == 0) break;
                                    await output.WriteAsync(buffer.AsMemory(0, read), deadline.Token).ConfigureAwait(false);
                                    remaining -= read;
                                    bytes += read;
                                }
                                copied++;
                            }
                            await using var snapshot = File.OpenRead(target);
                            string sha = Convert.ToHexString(await SHA256.HashDataAsync(snapshot, deadline.Token).ConfigureAwait(false));
                            entries.Add(new { TaskId = id, Source = source, Destination = target, Status = "snapshot", InitialLength = length, CopiedLength = snapshot.Length, Sha256 = sha });
                        }
                        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                        {
                            entries.Add(new { TaskId = id, Source = source, Status = "read-failed", Error = exception.ToString() });
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException) when (deadline.IsCancellationRequested) { status = "time-limit"; }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            status = "enumeration-failed";
            entries.Add(new { Source = tasks, Error = exception.ToString() });
        }
        finally
        {
            var report = new { Status = status, TaskRoot = tasks, CurrentTask = AppContext.BaseDirectory, Scanned = scanned, Matched = matched, Copied = copied, Bytes = bytes, Elapsed = watch.Elapsed, Entries = entries };
            await File.WriteAllTextAsync(Path.Combine(destination, "recovery.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true })).ConfigureAwait(false);
        }
    }

    private static bool IsLink(string path) => Path.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
}
#endif
