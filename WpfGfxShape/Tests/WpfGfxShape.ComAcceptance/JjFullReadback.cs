#if JJ_FULL_READBACK
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

namespace WpfGfxShape.ComAcceptance;

internal static class JjFullReadback
{
    internal static int Run(string[] paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        if (paths.Length != 2) throw new ArgumentOutOfRangeException(nameof(paths));
        string destination = Path.Combine(AppContext.BaseDirectory, "Results", "jj-full-inventory");
        Directory.CreateDirectory(destination);
        var summaries = new List<object>();
        for (int index = 0; index < paths.Length; index++)
        {
            string arch = index == 0 ? "x64" : "x86";
            string trxPath = Path.GetFullPath(paths[index], AppContext.BaseDirectory);
            XDocument trx = XDocument.Load(trxPath);
            XElement root = trx.Root ?? throw new InvalidDataException(trxPath);
            XNamespace ns = root.Name.Namespace;
            XElement counters = trx.Descendants(ns + "Counters").Single();
            XElement[] results = root.Element(ns + "Results")!.Elements(ns + "UnitTestResult").ToArray();
            XElement[] failed = results.Where(result => (string?)result.Attribute("outcome") == "Failed").ToArray();
            Assert.AreEqual(int.Parse(counters.Attribute("total")!.Value), results.Length);
            Assert.AreEqual(int.Parse(counters.Attribute("failed")!.Value), failed.Length);
            Assert.AreEqual(results.Length, results.Select(result => (string?)result.Attribute("testId")).Distinct().Count());
            var definitions = trx.Descendants(ns + "UnitTest").ToDictionary(test => test.Attribute("id")!.Value);
            var failures = failed.Select(result =>
            {
                string id = result.Attribute("testId")!.Value;
                string name = result.Attribute("testName")!.Value;
                XElement definition = definitions[id];
                XElement method = definition.Element(ns + "TestMethod")!;
                string message = result.Descendants(ns + "Message").FirstOrDefault()?.Value ?? "";
                string stack = result.Descendants(ns + "StackTrace").FirstOrDefault()?.Value ?? "";
                string category = message.Contains("Could not find", StringComparison.OrdinalIgnoreCase) || message.Contains("FileNotFoundException", StringComparison.Ordinal) || message.Contains("DirectoryNotFoundException", StringComparison.Ordinal) ? "输入缺失" : "待定";
                string reason = category == "输入缺失" ? "原始错误明确报告文件或目录缺失；不推定产品正确" : "原文保留，未证明产品、夹具、显示前置或隔离子退出因果；不统归HW环境";
                return new { TestId = id, ParentTestName = (string?)method.Attribute("className") + "." + (string?)method.Attribute("name"), Name = name, Arguments = name.Contains('(') ? name[name.IndexOf('(')..] : "无显式DataRow名称；见RawResult", Source = (string?)method.Attribute("codeBase"), Category = category, Reason = reason, Message = message, Stack = stack, Output = result.Element(ns + "Output")?.Value, Definition = definition.ToString(), RawResult = result.ToString() };
            }).ToArray();
            File.WriteAllText(Path.Combine(destination, arch + "-failures.json"), JsonSerializer.Serialize(failures, new JsonSerializerOptions { WriteIndented = true }));
            var focus = results.Where(r => new[] { "Default", "Hy", "Listener", "Offscreen", "Create", "PartitionState", "HistoricalCapture" }.Any(term => r.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))).Select(r => new { Id = (string?)r.Attribute("testId"), Name = (string?)r.Attribute("testName"), Outcome = (string?)r.Attribute("outcome") }).ToArray();
            File.WriteAllText(Path.Combine(destination, arch + "-focus.json"), JsonSerializer.Serialize(focus, new JsonSerializerOptions { WriteIndented = true }));
            summaries.Add(new { Arch = arch, Input = trxPath, InputSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trxPath))), Counters = counters.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value), ResultCount = results.Length, UniqueTestIds = results.Select(r => (string?)r.Attribute("testId")).Distinct().Count(), FailedCount = failed.Length, Times = trx.Descendants(ns + "Times").Single().ToString(), Outcome = (string?)trx.Descendants(ns + "ResultSummary").Single().Attribute("outcome"), RunInfo = trx.Descendants(ns + "RunInfo").Select(info => info.ToString()).ToArray(), Categories = failures.GroupBy(f => f.Category).ToDictionary(g => g.Key, g => g.Count()) });
        }
        File.WriteAllText(Path.Combine(destination, "summary.json"), JsonSerializer.Serialize(summaries, new JsonSerializerOptions { WriteIndented = true }));
        return 0;
    }
}
#endif
