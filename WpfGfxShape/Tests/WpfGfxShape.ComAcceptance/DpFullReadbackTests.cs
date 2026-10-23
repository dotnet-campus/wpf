#if DP_FULL_READBACK
using System.Text.Json;
using System.Text.RegularExpressions;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class DpFullReadbackTests
{
    /// <summary>Reads the two completed DP task directories without executing acceptance cases or modifying their evidence.</summary>
    [TestMethod]
    public void WhenExplicitReadbackRunsThenInterruptedFullEvidenceIsSummarized()
    {
        var summaries = new List<object>();
        foreach (var task in new[] { (Arch: "x86", Id: "624f2b6cf6f54172af4894312e8908b9"), (Arch: "x64", Id: "2f63b2fd89e7405fa076d5faa1a65464") })
        {
            string root = Path.Combine(@"C:\CodingAgentSandbox\Tasks", task.Id);
            string run = Path.Combine(root, "Results", "dp-full-" + task.Arch);
            string diagnostic = Directory.GetFiles(Path.Combine(run, "Diagnostic"), "*.diag").Single();
            var states = new Dictionary<string, (string Name, string State)>();
            foreach (string line in File.ReadLines(diagnostic))
            {
                if (!line.StartsWith("TestNodeUpdateMessage", StringComparison.Ordinal)) continue;
                Match match = Regex.Match(line, @"Value = (?<id>[^}]+) \}, DisplayName = (?<name>.*?), Properties =");
                Match state = Regex.Match(line, @"(?<state>Passed|Failed|InProgress|Skipped|Error|Timeout)TestNodeStateProperty");
                if (match.Success && state.Success) states[match.Groups["id"].Value.Trim()] = (match.Groups["name"].Value, state.Groups["state"].Value);
            }
            string[] stages = File.Exists(Path.Combine(run, "stages.jsonl")) ? File.ReadAllLines(Path.Combine(run, "stages.jsonl")) : [];
            string[] stdout = File.ReadAllLines(Path.Combine(run, "stdout.txt"));
            var manifest = File.ReadAllLines(Path.Combine(root, "package-files.txt"));
            var files = manifest.Select(path => new { Path = path, Bytes = new FileInfo(Path.Combine(root, path)).Length }).ToArray();
            summaries.Add(new
            {
                task.Arch, task.Id, Root = root,
                ParentTrx = Path.Combine(run, "TestResults", "child.trx"), ParentTrxExists = File.Exists(Path.Combine(run, "TestResults", "child.trx")),
                Diagnostic = diagnostic, DiagnosticLines = File.ReadLines(diagnostic).Count(),
                StateCounts = states.Values.GroupBy(value => value.State).ToDictionary(group => group.Key, group => group.Count()),
                Completed = states.Where(pair => pair.Value.State != "InProgress").Select(pair => new { Uid = pair.Key, pair.Value.Name, pair.Value.State }).ToArray(),
                InProgress = states.Where(pair => pair.Value.State == "InProgress").Select(pair => new { Uid = pair.Key, pair.Value.Name }).ToArray(),
                StdoutFailures = stdout.Where(line => line.StartsWith("failed ", StringComparison.Ordinal)).ToArray(),
                StageCount = stages.Length, LastStages = stages.TakeLast(12).ToArray(),
                FileCount = files.Length, TotalBytes = files.Sum(file => file.Bytes),
                EvidenceFiles = Directory.GetFiles(root, "*.json", SearchOption.AllDirectories).Select(path => Path.GetRelativePath(root, path)).Where(path => !path.StartsWith("Diagnostic", StringComparison.Ordinal)).ToArray()
            });
        }
        Directory.CreateDirectory("Results");
        File.WriteAllText(Path.Combine("Results", "dp-full-readback.json"), JsonSerializer.Serialize(summaries, new JsonSerializerOptions { WriteIndented = true }));
    }
}
#endif
