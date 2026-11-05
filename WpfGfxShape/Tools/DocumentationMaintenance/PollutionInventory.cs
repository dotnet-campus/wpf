using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static class PollutionInventory
{
    internal static void Run()
    {
        var root = AppContext.BaseDirectory;
        var docs = Path.Combine(root, "Snapshot", "Docs");
        var prior = Path.Combine(root, "PriorArtifacts");
        var results = Path.Combine(root, "Results");
        Directory.CreateDirectory(results);
        string Relative(string file) => Path.GetRelativePath(docs, file).Replace('\\', '/');
        string Hash(string file) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file)));
        bool Nested(string p) => p.StartsWith("Docs/", StringComparison.OrdinalIgnoreCase)
            || p.StartsWith("Output/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/Output/", StringComparison.OrdinalIgnoreCase);
        bool Input(string p) => p.StartsWith("Input/", StringComparison.OrdinalIgnoreCase)
            || p.Contains("/Input/", StringComparison.OrdinalIgnoreCase);
        bool Historical(string p) => p.StartsWith("handoffs/history/", StringComparison.OrdinalIgnoreCase)
            || p.StartsWith("history/", StringComparison.OrdinalIgnoreCase)
            || p.StartsWith("completed-work/", StringComparison.OrdinalIgnoreCase)
            || p.StartsWith("handoffs/completed-work/", StringComparison.OrdinalIgnoreCase)
            || p.StartsWith("investigations/", StringComparison.OrdinalIgnoreCase);
        var files = Directory.GetFiles(docs, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
        var artifacts = Directory.GetFiles(prior, "*", SearchOption.AllDirectories)
            .GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key!, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);
        var runtimeNames = artifacts.Values.SelectMany(v => v)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "Input" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(f => Path.GetExtension(f) is ".dll" or ".exe" or ".pdb" || f.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".runtimeconfig.json", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        bool Candidate(string p) => Nested(p) || Input(p) || runtimeNames.Contains(Path.GetFileName(p));
        var texts = files.Where(f => Path.GetExtension(f).Equals(".md", StringComparison.OrdinalIgnoreCase))
            .Where(f => !Candidate(Relative(f)))
            .ToDictionary(Relative, File.ReadAllText, StringComparer.Ordinal);
        var rows = new List<object>();
        var cleanup = new List<string>();
        var counts = new Dictionary<string, int> { ["authoritativeDocuments"] = 0, ["historicalEvidence"] = 0, ["generatedPollution"] = 0, ["undetermined"] = 0 };
        foreach (var file in files)
        {
            var p = Relative(file);
            var sha = Hash(file);
            var candidate = Candidate(p);
            var same = artifacts.TryGetValue(Path.GetFileName(file), out var possible)
                ? possible.Where(f => new FileInfo(f).Length == new FileInfo(file).Length && Hash(f) == sha)
                    .Select(f => Path.GetRelativePath(prior, f).Replace('\\', '/')).Order(StringComparer.Ordinal).ToArray()
                : Array.Empty<string>();
            // Only explicit path/link mentions count; a common basename alone is not provenance.
            var mentions = texts.Where(t => t.Value.Contains("Docs/" + p, StringComparison.OrdinalIgnoreCase)
                || t.Value.Contains("Docs\\" + p.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase)
                || t.Value.Contains("](" + Path.GetRelativePath(Path.GetDirectoryName(Path.Combine(docs, t.Key))!, file).Replace('\\', '/'), StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Key).ToArray();
            var authoritativeReferences = mentions.Where(p => !Historical(p) && p != "documentation-maintenance.md").ToArray();
            var category = candidate ? same.Length > 0 ? "generatedPollution" : "undetermined"
                : Historical(p) ? "historicalEvidence"
                : Path.GetExtension(p).Equals(".md", StringComparison.OrdinalIgnoreCase) ? "authoritativeDocuments" : "undetermined";
            counts[category]++;
            var eligible = candidate && same.Length > 0 && authoritativeReferences.Length == 0;
            if (eligible) cleanup.Add(p + "\t" + sha);
            rows.Add(new { path = p, bytes = new FileInfo(file).Length, sha256 = sha, category,
                suspectedGenerated = candidate, origin = same.Length > 0 ? "Exact stored artifact copy; causal task attribution not certified" : "Unknown",
                artifactMatches = same, documentMentions = mentions, authoritativeReferences,
                manualCleanupEligible = eligible, hostDeleted = false });
        }
        var previousInventory = Path.Combine(prior, "task6-final-package", "Input", "docs-files.txt");
        var old = File.ReadAllLines(previousInventory).Select(p => p.Replace('\\', '/'))
            .Select(p => p[(p.IndexOf("Docs/", StringComparison.OrdinalIgnoreCase) + 5)..]).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var current = files.Select(Relative).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var archiveInventory = File.ReadAllLines(Path.Combine(prior, "task6-archive-package", "Input", "docs-files.txt"))
            .Select(p => p.Replace('\\', '/')).Select(p => p[(p.IndexOf("Docs/", StringComparison.OrdinalIgnoreCase) + 5)..])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var groups = files.Select(Relative).GroupBy(p => p.StartsWith("Docs/", StringComparison.OrdinalIgnoreCase) ? "nested-Docs"
            : p.StartsWith("Input/", StringComparison.OrdinalIgnoreCase) ? "root-Input"
            : p.StartsWith("Output/", StringComparison.OrdinalIgnoreCase) ? "root-Output"
            : p.StartsWith("handoffs/Input/", StringComparison.OrdinalIgnoreCase) ? "handoffs-Input"
            : p.StartsWith("handoffs/Output/", StringComparison.OrdinalIgnoreCase) ? "handoffs-Output"
            : runtimeNames.Contains(Path.GetFileName(p)) ? "runtime-and-tool" : "documents-and-evidence")
            .ToDictionary(g => g.Key, g => g.Count());
        var report = new { total = files.Length, counts, groups, archiveInventoryCount = archiveInventory.Count,
            addedSinceArchiveInventory = current.Except(archiveInventory, StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToArray(),
            missingSinceArchiveInventory = archiveInventory.Except(current, StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToArray(),
            previousInventoryCount = old.Count,
            addedSinceTask6FinalInventory = current.Except(old, StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToArray(),
            missingSinceTask6FinalInventory = old.Except(current, StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToArray(),
            baseline848FileSetAvailable = false, semanticUnreviewed = "Unknown", hostDeleted = 0,
            boundary = "Published complete snapshots, not host in-place hashing. Categories are structural, not semantic approval. References are explicit path/link scans, not full semantic certification. Matching artifacts prove bytes, not creation history. No deletion performed.", files = rows };
        File.WriteAllText(Path.Combine(results, "pollution-inventory.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
        File.WriteAllLines(Path.Combine(results, "manual-cleanup-whitelist.tsv"), cleanup, new UTF8Encoding(false));
        Console.WriteLine(JsonSerializer.Serialize(new { total = files.Length, counts, previousInventoryCount = old.Count, cleanupCandidates = cleanup.Count, hostDeleted = 0 }));
    }
}
