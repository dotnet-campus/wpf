using System.Text;
using System.Text.Json;
using System.Xml.Linq;

internal static class Cleanup
{
    public static int Main(string[] args) => Run(args[0]);

    public static int Run(string root)
    {
        root = Path.GetFullPath(root);
        string tests = Path.Combine(root, "WpfGfxShape", "Tests");
        string artifacts = Path.Combine(root, "WpfGfxShape", "artifacts");
        string[] removed = ["GenProtoC5Audit.Diagnostics", "GenProtoNativeLayoutDiagnostics", "GenProtoX86Clr.Diagnostics", "NativeCaller", "Stage4Layout.Diagnostics", "Stage4Layout.X86.Diagnostics", "Tools", "WpfGfxShape.AbiIntegration", "WpfGfxShape.AbiIntegration.Host", "WpfGfxShape.ComAcceptance.Host", "WpfGfxShape.GenProto.Tests", "WpfGfxShape.LegacyHash.Tests", "WpfGfxShape.PlayerHwndAcceptance", "WpfGfxShape.PresentationCoreE2E", "WpfGfxShape.PublicAbi", "WpfGfxShape.WpfReference", "X86SharedSupport.Diagnostics"];
        string evidence = Path.Combine(artifacts, "test-results", "consolidation", "final-evidence.md");
        Directory.CreateDirectory(Path.GetDirectoryName(evidence)!);
        var text = new StringBuilder();
        string run = Path.Combine(root, ".coding-agent", "sandbox-results", "2a96fad1007146139499e9060eecae40");
        ReadTrx(Path.Combine(run, "Results", "integration-final-full.trx"), text);
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, ".coding-agent", "sandbox-results"), "*.trx", SearchOption.AllDirectories).Where(p => p.Contains(Path.DirectorySeparatorChar + "core" + Path.DirectorySeparatorChar)))
        {
            XDocument doc = XDocument.Load(file);
            if (doc.Descendants().Any(e => e.Name.LocalName == "Counters" && (string?)e.Attribute("total") == "4224")) ReadTrx(file, text);
        }
        string reports = Path.Combine(run, "Results", "publicabi");
        foreach (string file in Directory.GetFiles(reports, "report.json", SearchOption.AllDirectories))
        {
            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(file));
            text.AppendLine("## JSON " + file);
            foreach (JsonProperty p in doc.RootElement.EnumerateObject())
                if (p.Name is "RunId" or "Mode" or "Path" or "Sha256" or "OriginalSha256" or "Runtime" or "Architecture" or "Missing" or "Extra" or "OfficialProtectionContract" || p.Name.Contains("Module", StringComparison.OrdinalIgnoreCase))
                    text.AppendLine(p.Name + ": " + p.Value.GetRawText());
        }
        foreach (string file in Directory.GetFiles(Path.Combine(run, "Results", "pcore"), "*.json", SearchOption.AllDirectories))
        {
            using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(file));
            if (doc.RootElement.ValueKind != JsonValueKind.Object || !doc.RootElement.TryGetProperty("actualNativeModules", out JsonElement modules)) continue;
            text.AppendLine("## Pcore " + file);
            text.AppendLine("actualNativeModules: " + modules.GetRawText());
            foreach (string name in new[] { "role", "scene", "status", "exception", "exitCode" })
                if (doc.RootElement.TryGetProperty(name, out JsonElement value)) text.AppendLine(name + ": " + value.GetRawText());
        }
        File.WriteAllText(evidence, text.ToString());
        int deletedDirectories = 0;
        int deletedFiles = 0;
        long deletedBytes = 0;
        foreach (string name in removed)
        {
            string path = Path.Combine(tests, name);
            if (Directory.Exists(path))
            {
                deletedDirectories += 1 + Directory.GetDirectories(path, "*", SearchOption.AllDirectories).Length;
                foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                {
                    deletedFiles++;
                    deletedBytes += new FileInfo(file).Length;
                }
                Directory.Delete(path, true);
            }
        }
        string[] remaining = Directory.GetDirectories(tests).Select(Path.GetFileName).Order().ToArray()!;
        if (!remaining.SequenceEqual(new[] { "WpfGfxShape.ComAcceptance", "WpfGfxShape.Tests" })) throw new InvalidOperationException("Unexpected test directory.");
        if (Directory.GetFiles(tests, "*.*proj", SearchOption.AllDirectories).Length != 2) throw new InvalidOperationException("Unexpected test project.");
        foreach (string path in new[] { "cleanup-tool", "cleanup-publishobj", "obj/Cleanup", "bin/Cleanup" })
        {
            string directory = Path.Combine(artifacts, path);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        foreach (string path in new[] { "artifacts/bin/Cleanup.cs", "artifacts/obj/Cleanup.cs" })
        {
            string directory = Path.Combine(root, path);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        foreach (string file in Directory.GetFiles(Path.Combine(root, "eng", "process-cleanup")))
            if (Path.GetFileName(file) is not ("Cleanup.cs" or "README.md")) File.Delete(file);
        foreach (string directory in Directory.GetDirectories(Path.Combine(root, "eng", "process-cleanup")))
            Directory.Delete(directory, true);
        Console.WriteLine("原始报告已完整解析");
        Console.WriteLine("两个正式测试项目已核对");
        Console.WriteLine($"本次业务清理：{deletedFiles}文件/{deletedDirectories}目录/{deletedBytes}字节；迁移0");
        Console.WriteLine("既有30项目17目录账未重复累计");
        Console.WriteLine("临时Apply及发布编译产物已清理");
        Console.WriteLine("生产失败保留，证据已写入artifacts");
        return 0;
    }

    private static void ReadTrx(string file, StringBuilder text)
    {
        XDocument doc = XDocument.Load(file);
        XNamespace ns = doc.Root!.Name.Namespace;
        XElement[] results = doc.Descendants(ns + "UnitTestResult").ToArray();
        var definitions = doc.Descendants(ns + "UnitTest").ToDictionary(e => (string)e.Attribute("id")!, e => (string?)e.Element(ns + "TestMethod")?.Attribute("className"));
        text.AppendLine("## TRX " + file);
        text.AppendLine(doc.Root.Name.LocalName + " " + string.Join("; ", doc.Root.Attributes().Select(a => a.Name + "=" + a.Value)));
        text.AppendLine(doc.Descendants(ns + "Times").Single().ToString());
        text.AppendLine(doc.Descendants(ns + "Counters").Single().ToString());
        text.AppendLine("Parsed results: " + results.Length);
        foreach (var group in results.Where(e => (string?)e.Attribute("outcome") != "Passed").GroupBy(e => definitions[(string)e.Attribute("testId")!]))
        {
            text.AppendLine("### " + group.Key + " count=" + group.Count());
            foreach (XElement result in group)
            {
                text.AppendLine("- " + result.Attribute("testName")?.Value + " | " + result.Attribute("outcome")?.Value);
                text.AppendLine(result.Descendants(ns + "Message").FirstOrDefault()?.Value);
            }
        }
    }
}
