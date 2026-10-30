using System.Diagnostics;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class PartitionPresentStateTests
{
    /// <summary>Runs the real asynchronous HWND state contract in isolated official, candidate and repeated official processes.</summary>
    [TestMethod]
    [Timeout(115000)]
    public async Task WhenPartitionPresentStateRunsThenDynamicStateMatchesOfficialAsync()
    {
        string root = Path.Combine(NativeRun.PathsRoot, "partition-present-state", Guid.NewGuid().ToString("N"));
        foreach (string role in new[] { "official", "candidate", "official-repeat" })
        {
            string output = Path.GetFullPath(Path.Combine(root, role));
            Directory.CreateDirectory(output);
            var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException())
            {
                UseShellExecute = false,
                WorkingDirectory = AppContext.BaseDirectory
            };
            foreach (string argument in new[] { "--supervise-exit", output, "35", "FullyQualifiedName~WhenPartitionPresentStateRoleRunsThenRealContentAndSnapshotsShareState" })
                start.ArgumentList.Add(argument);
            start.Environment["IK_PARTITION_ROLE"] = role;
            start.Environment["IK_PARTITION_OUTPUT"] = output;
            using var process = Process.Start(start) ?? throw new InvalidOperationException();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(37));
            bool timedOut = false;
            try { await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false); }
            catch (OperationCanceledException)
            {
                timedOut = true;
                process.Kill(true);
                await process.WaitForExitAsync().ConfigureAwait(false);
            }
            File.WriteAllText(Path.Combine(output, "role-exit.json"), JsonSerializer.Serialize(new { role, pid = process.Id, exitCode = process.ExitCode, timedOut }));
            Assert.IsFalse(timedOut);
            Assert.AreEqual(0, process.ExitCode, output);
        }
    }
}

public sealed unsafe partial class D3DImageTests
{
    /// <summary>Checks complete state messages, real three-color consumption, events and cleanup; a direct default invocation runs candidate.</summary>
    [TestMethod]
    [Timeout(30000)]
    public void WhenPartitionPresentStateRoleRunsThenRealContentAndSnapshotsShareState()
    {
        string role = Environment.GetEnvironmentVariable("IK_PARTITION_ROLE") ?? "candidate";
        Assert.IsTrue(role is "official" or "official-repeat" or "candidate");
        string output = Environment.GetEnvironmentVariable("IK_PARTITION_OUTPUT")
            ?? Path.Combine(NativeRun.PathsRoot, "partition-present-state-direct", Guid.NewGuid().ToString("N"));
        RunHbConsumer(role != "candidate", true, output);
    }
}
