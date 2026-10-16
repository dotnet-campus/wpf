using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

string input = Path.Combine(AppContext.BaseDirectory, "Input");
string output = Path.Combine(AppContext.BaseDirectory, "Results");
Directory.CreateDirectory(output);
var jsonReports = new List<object>();
foreach (string path in Directory.GetFiles(input, "float-hdr-consumers-*.json").Order())
{
    byte[] bytes = File.ReadAllBytes(path);
    using JsonDocument document = JsonDocument.Parse(bytes);
    JsonElement root = document.RootElement;
    double[] color = root.GetProperty("Input").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    var combinations = new HashSet<string>();
    int pixels = 0, channels = 0, nonzeroPixels = 0, rawMismatches = 0;
    double maxDistance = 0;
    foreach (JsonElement observation in root.GetProperty("Observations").EnumerateArray())
    {
        string key = string.Join("/", new[] { "Source", "Affine", "Tile", "Scaling", "Consumer" }.Select(x => observation.GetProperty(x).GetInt32()));
        if (!combinations.Add(key)) throw new InvalidDataException("Duplicate combination: " + key);
        JsonElement basis = observation.GetProperty("Basis"), actual = observation.GetProperty("Actual");
        if (basis.GetArrayLength() != 16 || actual.GetArrayLength() != 16) throw new InvalidDataException("Surface size: " + key);
        byte[] raw = Convert.FromHexString(observation.GetProperty("RawFloatBuffer").GetString()!);
        if (raw.Length != 256) throw new InvalidDataException("Raw length: " + key);
        int consumer = observation.GetProperty("Consumer").GetInt32();
        for (int p = 0; p < 16; p++)
        {
            double distance = 0;
            bool nonzero = false;
            for (int c = 0; c < 4; c++)
            {
                float a = actual[p][c].GetSingle();
                float b = basis[p][c].GetSingle();
                float expected = consumer == 2 || c == 3 ? b : b * (float)color[c];
                if (!float.IsFinite(a) || !float.IsFinite(b)) throw new InvalidDataException("Nonfinite: " + key);
                distance += (double)(a - expected) * (a - expected);
                if (BitConverter.SingleToInt32Bits(a) != BitConverter.ToInt32(raw, (p * 4 + c) * 4)) rawMismatches++;
                nonzero |= a != 0;
                channels++;
            }
            maxDistance = Math.Max(maxDistance, Math.Sqrt(distance));
            if (nonzero) nonzeroPixels++;
            pixels++;
        }
    }
    jsonReports.Add(new { File = Path.GetFileName(path), FileSha256 = Convert.ToHexString(SHA256.HashData(bytes)), ProductionSha256 = root.GetProperty("Sha256").GetString(), Target = root.GetProperty("Target").GetString(), PixelFormat = root.GetProperty("PixelFormat").GetString(), Combinations = combinations.Order().ToArray(), Pixels = pixels, Channels = channels, NonzeroPixels = nonzeroPixels, RawMismatches = rawMismatches, MaxFloatRelationDistance = maxDistance, EntireDocumentParsed = true, NativeRawPaddingVerified = false });
}
var current = ReadTrx(Path.Combine(input, "current.trx"));
var previous = ReadTrx(Path.Combine(input, "previous.trx"));
var report = new { JsonReports = jsonReports, GenProto = ReadTrx(Path.Combine(input, "genproto.trx")), Recheck = ReadTrx(Path.Combine(input, "recheck.trx")), Current = current, Previous = previous, AddedFailures = current.Failures.Where(x => !previous.Failures.Any(y => y.FullName == x.FullName)).ToArray(), RemovedFailures = previous.Failures.Where(x => !current.Failures.Any(y => y.FullName == x.FullName)).ToArray(), Identity = JsonSerializer.Deserialize<JsonElement>(File.ReadAllBytes(Path.Combine(input, "aot-identity.json"))) };
File.WriteAllText(Path.Combine(output, "audit.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Parsed {jsonReports.Count} complete JSON; current {current.RunId}: {current.Counters}; previous {previous.RunId}: {previous.Counters}");
foreach (var failure in report.AddedFailures) Console.WriteLine("ADDED " + failure.FullName + "\n" + failure.Message);

static TrxReport ReadTrx(string path)
{
    XDocument document = XDocument.Load(path);
    XNamespace ns = document.Root!.Name.Namespace;
    var definitions = document.Descendants(ns + "UnitTest").ToDictionary(x => (string)x.Attribute("id")!, x => x.Element(ns + "TestMethod")!);
    var failures = document.Descendants(ns + "UnitTestResult").Where(x => (string?)x.Attribute("outcome") != "Passed").Select(x =>
    {
        XElement method = definitions[(string)x.Attribute("testId")!];
        return new Failure((string?)method.Attribute("className") + "." + (string?)method.Attribute("name") + " :: " + (string?)x.Attribute("testName"), (string?)x.Attribute("outcome"), x.Descendants(ns + "Message").FirstOrDefault()?.Value, x.Descendants(ns + "StackTrace").FirstOrDefault()?.Value);
    }).OrderBy(x => x.FullName).ToArray();
    XElement counters = document.Descendants(ns + "Counters").Single();
    return new TrxReport((string)document.Root.Attribute("id")!, string.Join(" ", counters.Attributes().Select(x => x.Name + "=" + x.Value)), document.Descendants(ns + "UnitTestResult").Count(), failures);
}
internal sealed record Failure(string FullName, string? Outcome, string? Message, string? StackTrace);
internal sealed record TrxReport(string RunId, string Counters, int Results, Failure[] Failures);
