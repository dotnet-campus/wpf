using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

if (args.Length == 1 && args[0] == "inventory")
{
    PollutionInventory.Run();
    return;
}
if (args.Length != 1 || args[0] is not ("archive" or "pointer" or "verify"))
    throw new ArgumentException("Expected archive, pointer or verify.");
var names = new[] { "final-acceptance-remaining-matrix", "remaining-gap-closure-plan", "replacement-issues-and-plan-status", "wpfgfx-complete-replacement-validation-plan" };
var utf8 = new UTF8Encoding(false, true);
var input = Path.Combine(AppContext.BaseDirectory, "Input");
var output = Path.Combine(AppContext.BaseDirectory, "DocumentationOutput");
var results = Path.Combine(AppContext.BaseDirectory, "Results");
Directory.CreateDirectory(results);
var allowedDocuments = names.SelectMany(n => new[] { Legacy(n), Audit(n), n + ".md" }).ToHashSet(StringComparer.Ordinal);
var options = new JsonSerializerOptions { WriteIndented = true };
string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
bool Bom(byte[] bytes) => bytes.AsSpan().StartsWith(new byte[] { 239, 187, 191 });
string Decode(byte[] bytes) => utf8.GetString(bytes.AsSpan(Bom(bytes) ? 3 : 0));
string[] TextLines(byte[] bytes)
{
    using var reader = new StringReader(Decode(bytes));
    var lines = new List<string>();
    while (reader.ReadLine() is { } line) lines.Add(line);
    return lines.ToArray();
}
string Legacy(string name) => "handoffs/history/" + name + "-legacy.md";
string Audit(string name) => "handoffs/history/" + name + "-audit.json";
void Save(string relative, byte[] bytes)
{
    if (!allowedDocuments.Contains(relative)) throw new InvalidDataException("Output is not on the document whitelist.");
    var path = Path.Combine(output, relative);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
    stream.Write(bytes);
}
var receipts = new List<object>();
var excerpts = new StringBuilder("# Task6 machine-selected reading map (not semantic review)\n");
foreach (var name in names)
{
    var original = File.ReadAllBytes(Path.Combine(input, name + ".md"));
    var lines = TextLines(original);
    if (args[0] == "verify")
    {
        var archived = File.ReadAllBytes(Path.Combine(input, Legacy(name)));
        using var prior = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(input, Audit(name))));
        var r = prior.RootElement;
        if (Hash(original) != r.GetProperty("pointerSha256").GetString()
            || lines.Length != r.GetProperty("pointerLines").GetInt32()
            || Hash(archived) != r.GetProperty("sourceSha256").GetString()
            || archived.Length != r.GetProperty("sourceBytes").GetInt32()
            || TextLines(archived).Length != r.GetProperty("sourceLines").GetInt32()
            || Bom(archived) != r.GetProperty("utf8Bom").GetBoolean()
            || !r.GetProperty("sourceUnchangedSinceArchivePackaging").GetBoolean())
            throw new InvalidDataException("Final published files differ: " + name);
        receipts.Add(new { name, verified = true, pointerLines = lines.Length, pointerSha256 = Hash(original), archiveLines = TextLines(archived).Length, archiveBytes = archived.Length, archiveSha256 = Hash(archived), utf8Bom = Bom(archived) });
        continue;
    }
    byte[] archive;
    if (args[0] == "archive")
    {
        Save(Legacy(name), original);
        archive = File.ReadAllBytes(Path.Combine(output, Legacy(name)));
    }
    else
    {
        archive = File.ReadAllBytes(Path.Combine(input, Legacy(name)));
        using var prior = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(input, Audit(name))));
        if (Hash(original) != prior.RootElement.GetProperty("sourceSha256").GetString())
            throw new InvalidDataException("Source changed since archive packaging: " + name);
    }
    if (!original.AsSpan().SequenceEqual(archive)) throw new InvalidDataException("Archive differs: " + name);
    var pointer = $"# 历史规划兼容入口：{name}\n\n本页不再维护累计状态、预算或并行执行计划。\n\n- [唯一当前恢复入口](handoffs/current-recovery.md)\n- [当前严格替换验收矩阵与顺序](handoffs/current-validation-matrix.md)\n- [当前权威对照合同](handoffs/current-comparison-contract.md)\n- [完整原字节历史归档](handoffs/history/{name}-legacy.md)\n- [归档审计与原路径基准](handoffs/history/README.md)\n\n旧“唯一当前”、原版永禁与额度限制不具当前权威；保全不等于全文语义审核。\n";
    var receipt = new
    {
        phase = args[0], source = "Docs/" + name + ".md", archive = "Docs/" + Legacy(name), originalLinkBase = "Docs/",
        sourceBytes = original.Length, sourceLines = lines.Length, sourceSha256 = Hash(original),
        archiveBytes = archive.Length, archiveLines = TextLines(archive).Length, archiveSha256 = Hash(archive),
        byteExact = true, utf8Bom = Bom(original),
        pointerLines = TextLines(utf8.GetBytes(pointer)).Length, pointerSha256 = Hash(utf8.GetBytes(pointer)),
        sourceUnchangedSinceArchivePackaging = args[0] == "pointer", firstLine = lines.First(), lastLine = lines.Last(),
        boundary = "Complete published input copies, not in-place workspace hashes. Archive fully reread and byte-compared. Pointer phase repackages landed archive and unchanged source. Preservation is not full semantic review."
    };
    receipts.Add(receipt);
    Save(Audit(name), utf8.GetBytes(JsonSerializer.Serialize(receipt, options)));
    if (args[0] == "pointer") Save(name + ".md", utf8.GetBytes(pointer));
    excerpts.AppendLine("\n## " + name + " / " + lines.Length + " lines");
    for (var i = 0; i < lines.Length; i++)
        if (i < 12 || i >= lines.Length - 65 || lines[i].StartsWith("## ", StringComparison.Ordinal))
            excerpts.AppendLine($"{i + 1}: {lines[i]}");
}
if (args[0] == "archive")
    File.WriteAllText(Path.Combine(results, "reading-map.md"), excerpts.ToString(), utf8);
if (args[0] == "verify")
{
    var supporting = new[] { "handoffs/current-validation-matrix.md", "handoffs/history/README.md", "documentation-maintenance.md", "handoffs/current-recovery.md", "README.md", "governance-authority-traceability.md", "publicabi-family-delivery-plan.md" }
        .Select(p => { var b = File.ReadAllBytes(Path.Combine(input, p)); return new { path = p, sha256 = Hash(b), lines = TextLines(b).Length, fullyRead = true }; }).ToArray();
    var result = JsonSerializer.Serialize(new { verified = true, files = receipts, supporting, boundary = "Fresh final published copies, not in-place hashes or full semantic review." }, options);
    File.WriteAllText(Path.Combine(results, "verification.json"), result, utf8);
    Console.WriteLine(result);
}
Console.WriteLine(JsonSerializer.Serialize(receipts, options));
