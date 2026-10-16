using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

string input = Path.Combine(AppContext.BaseDirectory, "Input");
string output = Path.Combine(AppContext.BaseDirectory, "Results");
Directory.CreateDirectory(output);
var documents = new List<object>();
var runs = new List<object>();
var failures = new Dictionary<string, string[]>();
foreach (string file in Directory.GetFiles(input, "*.json", SearchOption.AllDirectories).Order())
{
    byte[] bytes = File.ReadAllBytes(file);
    using JsonDocument document = JsonDocument.Parse(bytes);
    documents.Add(new { File = Path.GetRelativePath(input, file), Sha256 = Convert.ToHexString(SHA256.HashData(bytes)), Content = document.RootElement.Clone() });
}
foreach (string file in Directory.GetFiles(input, "*.trx", SearchOption.AllDirectories).Order())
{
    XDocument document = XDocument.Load(file);
    XNamespace ns = document.Root!.Name.Namespace;
    XElement[] results = document.Descendants(ns + "UnitTestResult").ToArray();
    string[] failed = results.Where(x => (string?)x.Attribute("outcome") == "Failed").Select(x => (string)x.Attribute("testName")!).Order().ToArray();
    string name = Path.GetRelativePath(input, file);
    failures.Add(name, failed);
    runs.Add(new { File = name, RunId = (string?)document.Root.Attribute("id"), Counters = document.Descendants(ns + "Counters").Single().Attributes().ToDictionary(x => x.Name.LocalName, x => x.Value), Results = results.Select(x => new { Name = (string?)x.Attribute("testName"), Outcome = (string?)x.Attribute("outcome"), Error = x.Element(ns + "Output")?.Element(ns + "ErrorInfo")?.Value }).ToArray(), EntireDocumentParsed = true });
}
string[] current = failures.Single(x => x.Key == Path.Combine("full", "async-drain-final-full.trx")).Value;
string[] baseline = failures.Single(x => x.Key.StartsWith("baseline" + Path.DirectorySeparatorChar, StringComparison.Ordinal)).Value;
File.WriteAllText(Path.Combine(output, "audit.json"), JsonSerializer.Serialize(new { Runs = runs, Documents = documents, AddedFailures = current.Except(baseline).ToArray(), RemovedFailures = baseline.Except(current).ToArray() }, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText(Path.Combine(output, "summary.json"), JsonSerializer.Serialize(new { JsonDocuments = documents.Count, TrxDocuments = runs.Count, CurrentFailures = current, AddedFailures = current.Except(baseline).ToArray(), RemovedFailures = baseline.Except(current).ToArray() }, new JsonSerializerOptions { WriteIndented = true }));
var drain = Directory.GetFiles(input, "async-wic-drain-*.json", SearchOption.AllDirectories).Order().Select(file =>
{
    using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(file));
    JsonElement root = document.RootElement;
    return new { File = Path.GetRelativePath(input, file), System = root.GetProperty("system").GetBoolean(), State = root.GetProperty("queueState").GetInt32(), Completed = root.GetProperty("completed").GetBoolean(), Sha256 = root.GetProperty("sha256").GetString(), Disposals = root.GetProperty("disposals").GetInt32(), ReleaseThread = root.GetProperty("releaseThread").GetUInt32(), Reentry = root.GetProperty("reentry").GetInt32(), Calls = string.Join(",", root.GetProperty("calls").EnumerateArray().Select(call => $"{call.GetProperty("operation").GetString()}={call.GetProperty("result").GetInt32()}")) };
}).ToArray();
File.WriteAllText(Path.Combine(output, "drain-summary.json"), string.Join(Environment.NewLine, drain.Select(row => JsonSerializer.Serialize(row))));
Console.WriteLine($"Parsed {documents.Count} complete JSON documents and {runs.Count} complete TRX documents; added={current.Except(baseline).Count()}, removed={baseline.Except(current).Count()}.");
