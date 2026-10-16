#:property TargetFramework=net10.0
#:property ImplicitUsings=enable
#:property Nullable=enable

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

Cleanup.Run(args.Length == 0 ? Directory.GetCurrentDirectory() : args[0]);

public static partial class Cleanup
{
    [System.Text.RegularExpressions.GeneratedRegex(@"\[([^\]]+)\]\(([^)]+)\)")]
    private static partial System.Text.RegularExpressions.Regex MarkdownLinks();
    /// <summary>Executes the explicitly authorized process-material cleanup.</summary>
    public static void Run(string root)
    {
        string Full(string path) => Path.GetFullPath(Path.Combine(root, path));
        var deleted = new List<Deletion>();
        var retained = new List<Retention>();
        string summary = Full("WpfGfxShape/artifacts/process-cleanup/summary.json");
        if (File.Exists(summary))
        {
            using var prior = JsonDocument.Parse(File.ReadAllText(summary));
            deleted.AddRange(prior.RootElement.GetProperty("deleted").Deserialize(CleanupJsonContext.Default.ListDeletion)!);
            retained.AddRange(prior.RootElement.GetProperty("retained").Deserialize(CleanupJsonContext.Default.ListRetention)!);
        }
        int historical = deleted.Count;
        void Remove(string path)
        {
            string full = Full(path);
            if (Directory.Exists(full))
            {
                string[] files = Directory.GetFiles(full, "*", SearchOption.AllDirectories);
                deleted.Add(new(path, files.Length, Directory.GetDirectories(full, "*", SearchOption.AllDirectories).Length + 1, files.Sum(p => new FileInfo(p).Length)));
                Directory.Delete(full, true);
            }
            else if (File.Exists(full))
            {
                deleted.Add(new(path, 1, 0, new FileInfo(full).Length));
                File.Delete(full);
            }
        }
        // These are generated run summaries, not independent inputs or product contracts.
        string[] ledgerReports = ["current-user-scope-overrides.json", "remaining-wire-mapping-finite-acceptance.json", "mediaown-required-finite-acceptance.json", "nativeonly-packet-execution-finite-acceptance.json", "wirecompiler-finite-acceptance.json", "x86-clr-wire-finite-acceptance.json", "drawing-float-closeout-evidence.md", "media-provider-closeout-evidence.md", "processcreate-factory-evidence-20261015.md", "shader-sampler-realization-evidence.md", "software-sampler-validation-20261013.md"];
        string[] obsoleteDocs = [
            "documentation-readonly-audit.md", "docs-governance-report.md", "docs-organization-report.md",
            "migration-master-plan.md", "session-roadmap.md", "drawbitmap-effects-float-implementation-plan.md", "overall-progress-assessment.md", "11-first-coding-step.md",
            "history/plans/session-roadmap.md", "history/plans/drawbitmap-effects-float-implementation-plan.md", "history/plans/overall-progress-assessment.md", "history/plans/11-first-coding-step.md",
            "handoffs/progress-completed-work.md", "handoffs/progress-hw-rendertarget.md",
            "investigations/geometry-family-wip.md", "investigations/closed/geometry-family-wip.md", "investigations/glyph-outline-wip.md", "investigations/active/glyph-outline-wip.md", "investigations/PresentationCore-dropshadow-wip.md",
            "investigations/genproto-evidence-recovery.md", "investigations/active/genproto-evidence-recovery.md",
            "completed-work/GenProto-C5-activeincludes.md", "completed-work/GenProto-C5-final-adjudication.md", "completed-work/GenProto-C5-selected-build-evaluation.md", "completed-work/GenProto-factory-physical-finite-closure.md", "completed-work/GenProto-milcmd-build-contract-finite.md", "completed-work/GenProto-milcmd-recursive-physical-progress.md", "completed-work/GenProto-nativeonly-packet-execution-finite.md", "completed-work/GenProto-pen-producer-consumer-finite.md", "completed-work/GenProto-remaining-wire-mapping-finite.md", "completed-work/GenProto-required-final-reconciliation.md", "completed-work/GenProto-wirecompiler-finite-acceptance.md", "completed-work/GenProto-x86-clr-wire-finite.md",
            "handoffs/completed-work/GenProto-finite-closure-audit.md", "handoffs/completed-work/GenProto-layer-contract-review.md"
        ];
        // Repair surviving Markdown navigation before removing its obsolete destinations.
        var removed = ledgerReports.Select(p => Full("WpfGfxShape/Ledger/" + p))
            .Concat(obsoleteDocs.Select(p => Full("WpfGfxShape/Docs/" + p))).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (string file in Directory.GetFiles(Full("WpfGfxShape/Docs"), "*.md", SearchOption.AllDirectories))
        {
            if (removed.Contains(file)) continue;
            string text = File.ReadAllText(file);
            string updated = MarkdownLinks().Replace(text, match =>
            {
                string target = match.Groups[2].Value.Split('#')[0];
                if (target.Length == 0 || Uri.TryCreate(target, UriKind.Absolute, out _)) return match.Value;
                string destination = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, target));
                if (!removed.Contains(destination)) return match.Value;
                string current = destination.EndsWith("current-user-scope-overrides.json", StringComparison.OrdinalIgnoreCase)
                    ? "WpfGfxShape/Docs/decisions/DEC-0005-x86-x64-acceptance-scope.md"
                    : "WpfGfxShape/Docs/history-index.md";
                return $"[{match.Groups[1].Value}（已清理；合同/说明）]({Path.GetRelativePath(Path.GetDirectoryName(file)!, Full(current)).Replace('\\', '/')})";
            });
            if (updated != text) File.WriteAllText(file, updated);
        }
        foreach (string path in ledgerReports) Remove("WpfGfxShape/Ledger/" + path);
        Remove("WpfGfxShape/Ledger/views");
        foreach (string path in obsoleteDocs) Remove("WpfGfxShape/Docs/" + path);
        Remove("WpfGfxShape/Investigations/GenProtoEvidence");
        Remove("WpfGfxShape/Tests/ReferenceData");
        Remove("WpfGfxShape/Tests/WpfGfxShape.PublicAbi/ReferenceData/projection-glyph-reference.json");
        void RemoveOutputs(string directory)
        {
            foreach (string child in Directory.GetDirectories(directory))
                if (Path.GetFileName(child) is "TestResults" or "bin" or "obj") Remove(Path.GetRelativePath(root, child));
                else RemoveOutputs(child);
        }
        RemoveOutputs(Full("WpfGfxShape/Tests"));
        // Retain evidence IDs and subject contracts; removed run payloads cannot satisfy them.
        string evidence = Full("WpfGfxShape/Ledger/evidence.jsonl");
        byte[] bytes = File.ReadAllBytes(evidence);
        var reader = new Utf8JsonReader(bytes, new JsonReaderOptions { AllowMultipleValues = true });
        var rows = new List<string>();
        while (reader.Read())
        {
            var row = JsonNode.Parse(ref reader)!.AsObject();
            foreach (string key in new[] { "path", "hash", "sha256", "runId", "notes", "blockerReason", "executed", "configuration", "architecture" }) row.Remove(key);
            row["path"] = "";
            row["hash"] = "";
            row["executed"] = false;
            row["status"] = "Pending";
            if (row.ContainsKey("satisfactionState")) row["satisfactionState"] = "Blocked";
            rows.Add(row.ToJsonString(CleanupJsonContext.Default.Options));
        }
        File.WriteAllLines(evidence, rows);
        foreach (string path in new[] { "WpfGfxShape/artifacts/process-cleanup/publish", "WpfGfxShape/artifacts/process-cleanup/obj", "WpfGfxShape/artifacts/process-cleanup/Apply.proj", "artifacts/bin/Cleanup", "artifacts/obj/Cleanup", "artifacts/bin/Cleanup.cs", "artifacts/obj/Cleanup.cs", "eng/process-cleanup/obj", "eng/process-cleanup/publish" }) Remove(path);
        var currentDeleted = deleted.Skip(historical).ToArray();
        var currentRun = new CurrentRun(currentDeleted.Sum(p => p.files), currentDeleted.Sum(p => p.directories), currentDeleted.Sum(p => p.bytes), 0);
        Directory.CreateDirectory(Path.GetDirectoryName(summary)!);
        File.WriteAllText(summary, JsonSerializer.Serialize(new CleanupSummary(deleted, retained, currentRun, "Historical first batch: 34903 files / 2178 directories / approximately 7.35 GB; not included in currentRun"), CleanupJsonContext.Default.CleanupSummary));
        string report = Full("WpfGfxShape/artifacts/process-cleanup/cleanreport.json");
        if (currentDeleted.Length > 0)
            File.WriteAllText(report, JsonSerializer.Serialize(new CleanupSummary(currentDeleted.ToList(), [], currentRun, "Docs/Ledger cleanup; production incomplete; GenProto full not restored or run"), CleanupJsonContext.Default.CleanupSummary));
        Console.WriteLine("文档与 Ledger 过程清理完成。");
        Console.WriteLine($"本次实际删除：{currentRun.files} 文件 / {currentRun.directories} 目录 / {currentRun.bytes} 字节");
        Console.WriteLine(currentRun.files == 0 ? "本次没有新增删除。" : "已执行显式用途清单，未搬家备份运行产物。");
        Console.WriteLine("本次迁移项：0；用户合同归并既有决议。");
        Console.WriteLine("历史验收非当前；GenProto 全量尚未恢复，生产总门仍未完成。");
        Console.WriteLine($"详细报告路径：{summary}");
    }
}
internal sealed record Deletion(string path, int files, int directories, long bytes);
internal sealed record Retention(string source, string target);
internal sealed record CurrentRun(int files, int directories, long bytes, int migrations);
internal sealed record CleanupSummary(List<Deletion> deleted, List<Retention> retained, CurrentRun currentRun, string historicalNote);
[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(CleanupSummary))]
[JsonSerializable(typeof(List<Deletion>))]
[JsonSerializable(typeof(List<Retention>))]
[JsonSerializable(typeof(JsonObject))]
internal sealed partial class CleanupJsonContext : JsonSerializerContext;
