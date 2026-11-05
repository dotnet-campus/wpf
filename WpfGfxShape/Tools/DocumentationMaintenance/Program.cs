using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

if (args.Length == 1 && args[0] == "inventory")
{
    PollutionInventory.Run();
    return;
}
if (args.Length != 1 || args[0] != "verify")
    throw new ArgumentException("Expected verify.");

var names = new[] { "final-acceptance-remaining-matrix", "remaining-gap-closure-plan", "replacement-issues-and-plan-status", "wpfgfx-complete-replacement-validation-plan" };
var utf8 = new UTF8Encoding(false, true);
var input = Path.Combine(AppContext.BaseDirectory, "Input");
var results = Path.Combine(AppContext.BaseDirectory, "Results");
Directory.CreateDirectory(results);
var options = new JsonSerializerOptions { WriteIndented = true };
string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
bool Bom(byte[] bytes) => bytes.AsSpan().StartsWith(new byte[] { 239, 187, 191 });
int LineCount(byte[] bytes)
{
    using var reader = new StringReader(utf8.GetString(bytes.AsSpan(Bom(bytes) ? 3 : 0)));
    var count = 0;
    while (reader.ReadLine() is not null) count++;
    return count;
}
var receipts = new List<object>();
foreach (var name in names)
{
    var archived = File.ReadAllBytes(Path.Combine(input, "history", "handoffs", name + "-legacy.md"));
    using var prior = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(input, "history", "handoffs", name + "-audit.json")));
    var r = prior.RootElement;
    if (Hash(archived) != r.GetProperty("sourceSha256").GetString()
        || archived.Length != r.GetProperty("sourceBytes").GetInt32()
        || LineCount(archived) != r.GetProperty("sourceLines").GetInt32()
        || Bom(archived) != r.GetProperty("utf8Bom").GetBoolean()
        || !r.GetProperty("sourceUnchangedSinceArchivePackaging").GetBoolean())
        throw new InvalidDataException("Final published files differ: " + name);
    receipts.Add(new { name, verified = true, archiveLines = LineCount(archived), archiveBytes = archived.Length, archiveSha256 = Hash(archived), utf8Bom = Bom(archived) });
}
var supporting = new[] { "current/current-validation-matrix.md", "history/handoffs/README.md", "current/repository-organization-check.md", "current/current-recovery.md", "README.md", "specifications/governance-authority-traceability.md", "specifications/publicabi-family-delivery-plan.md" }
    .Select(p => { var b = File.ReadAllBytes(Path.Combine(input, p)); return new { path = p, sha256 = Hash(b), lines = LineCount(b), fullyRead = true }; }).ToArray();
var result = JsonSerializer.Serialize(new { verified = true, files = receipts, supporting, boundary = "Published archive copies checked against preserved audits. Deleted compatibility pointers are not verified. Not in-place hashes or full semantic review." }, options);
File.WriteAllText(Path.Combine(results, "verification.json"), result, utf8);
Console.WriteLine(result);
