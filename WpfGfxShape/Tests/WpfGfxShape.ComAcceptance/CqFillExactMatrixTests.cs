#if CQ_FILL_EXACT_DIAGNOSTICS
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class CqFillExactMatrixTests
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>Checks real WPF fill against independently repeated official rendering without diagnostic exemptions.</summary>
    [TestMethod]
    public Task WhenframesFillMatchesOfficial() => CompareAsync("frames");

    /// <summary>Checks transformed and negative boundary fill.</summary>
    [TestMethod]
    public Task WhendeviceFillMatchesOfficial() => CompareAsync("device");

    /// <summary>Checks the retained narrow precision controls.</summary>
    [TestMethod]
    public Task WhenprecisionFillMatchesOfficial() => CompareAsync("precision");

    /// <summary>Checks sibling fill without historical exemptions.</summary>
    [TestMethod]
    public Task WhensiblingsFillMatchesOfficial() => CompareAsync("siblings");

    private static async Task CompareAsync(string group)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "cq-fill-exact", group);
        string? role = Environment.GetEnvironmentVariable("CQ_FILL_ROLE");
        if (role is not null)
        {
            Exception? failure = null;
            var thread = new Thread(() => { try { Render(root, role, group); } catch (Exception error) { failure = error; } });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(90)), root);
            if (failure is not null) throw new AssertFailedException(failure.ToString());
            return;
        }
        foreach (string selected in new[] { "official", "candidate", "official-repeat" })
        {
            Environment.SetEnvironmentVariable("CQ_FILL_ROLE", selected);
            int exit;
            try { exit = await ExitDiagnostics.RunAsync([Path.Combine(root, selected), "120", $"FullyQualifiedName~CqFillExactMatrixTests.When{group}Fill"]); }
            finally { Environment.SetEnvironmentVariable("CQ_FILL_ROLE", null); }
            Assert.AreEqual(0, exit, selected + ": " + root);
        }
        string[] files = Directory.GetFiles(Path.Combine(root, "official"), "*.pbgra32").Where(p => IsTarget(Path.GetFileName(p), group)).Order().ToArray();
        Assert.IsTrue(files.Length > 0, root);
        var rows = new List<object>();
        var failures = new List<string>();
        var table = new List<string> { "scene\tbyteDifferences\trepeatDifferences\tnonempty\tresult" };
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            byte[] expected = File.ReadAllBytes(file);
            byte[] actual = File.ReadAllBytes(Path.Combine(root, "candidate", name));
            byte[] repeat = File.ReadAllBytes(Path.Combine(root, "official-repeat", name));
            int differences = Differences(expected, actual), repeatDifferences = Differences(expected, repeat);
            bool nonempty = Nonempty(expected) && Nonempty(actual) && Nonempty(repeat);
            bool accepted = nonempty && differences == 0 && repeatDifferences == 0;
            if (!accepted) failures.Add(name);
            table.Add($"{name}\t{differences}\t{repeatDifferences}\t{nonempty}\t{(accepted ? "Passed" : "Failed")}");
            rows.Add(new { name, differences, repeatDifferences, nonempty, accepted, officialSha = Hash(expected), candidateSha = Hash(actual), repeatSha = Hash(repeat) });
            Save(Path.Combine(root, name + ".diff.json"), Enumerable.Range(0, expected.Length).Where(i => expected[i] != actual[i]).Select(i => new { offset = i, expected = expected[i], actual = actual[i] }));
        }
        bool discriminating = true;
        if (group == "device")
        {
            var discrimination = new List<object>();
            foreach (var pair in new[] { (family: "be", first: "B", second: "E"), (family: "df", first: "D", second: "F") })
            {
                string[] samples = Directory.GetFiles(Path.Combine(root, "official"), $"separator-{pair.family}-*-fill-{pair.first}.pbgra32");
                int changed = samples.Count(p => Differences(File.ReadAllBytes(p), File.ReadAllBytes(p.Replace($"-{pair.first}.pbgra32", $"-{pair.second}.pbgra32", StringComparison.Ordinal))) > 0);
                discrimination.Add(new { pair.family, samples = samples.Length, changed });
                discriminating &= changed > 0;
            }
            Save(Path.Combine(root, "discrimination.json"), discrimination);
        }
        File.WriteAllLines(Path.Combine(root, "scenes.tsv"), table);
        Save(Path.Combine(root, "comparison.json"), new { threshold = 0, exact = failures.Count == 0 && discriminating, discriminating, hardwareVerified = false, rows });
        WriteRoleEvidence(root, ["official", "candidate", "official-repeat"]);
        Assert.IsTrue(failures.Count == 0 && discriminating, root + ": " + string.Join(", ", failures));
    }

    /// <summary>Runs an independent old/new complete-buffer stroke control; it is not a fill exact result.</summary>
    [TestMethod]
    public async Task WhenHistoricalStrokeIsRenderedThenOldAndNewBuffersAreUnchanged()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "cq-fill-exact", "stroke");
        string? role = Environment.GetEnvironmentVariable("CQ_FILL_ROLE");
        if (role is not null)
        {
            Exception? failure = null;
            var thread = new Thread(() => { try { Render(root, role, "siblings"); } catch (Exception error) { failure = error; } });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(90)));
            if (failure is not null) throw new AssertFailedException(failure.ToString());
            return;
        }
        foreach (string selected in new[] { "baseline", "candidate" })
        {
            Environment.SetEnvironmentVariable("CQ_FILL_ROLE", selected);
            int exit;
            try { exit = await ExitDiagnostics.RunAsync([Path.Combine(root, selected), "120", "FullyQualifiedName~CqFillExactMatrixTests.WhenHistoricalStroke"]); }
            finally { Environment.SetEnvironmentVariable("CQ_FILL_ROLE", null); }
            Assert.AreEqual(0, exit, root);
        }
        var rows = new List<object>();
        bool unchanged = true;
        foreach (string file in Directory.GetFiles(Path.Combine(root, "baseline"), "*-stroke-*.pbgra32"))
        {
            byte[] old = File.ReadAllBytes(file), current = File.ReadAllBytes(Path.Combine(root, "candidate", Path.GetFileName(file)));
            int differences = Differences(old, current);
            bool nonempty = Nonempty(old) && Nonempty(current);
            unchanged &= differences == 0 && nonempty;
            rows.Add(new { name = Path.GetFileName(file), differences, nonempty });
        }
        Save(Path.Combine(root, "comparison.json"), new { fillExact = false, unchanged, baselineIsCurrentSnapshot = Environment.GetEnvironmentVariable("CQ_BASELINE_DLL") is null, rows });
        WriteRoleEvidence(root, ["baseline", "candidate"]);
        Assert.IsTrue(rows.Count > 0 && unchanged, root);
    }

    private static bool IsTarget(string name, string group) => group switch
    {
        "device" => name.Contains("-fill-", StringComparison.Ordinal) && (name.EndsWith("-real.pbgra32", StringComparison.Ordinal) || name.EndsWith("-real-cubic.pbgra32", StringComparison.Ordinal) || name.EndsWith("-line.pbgra32", StringComparison.Ordinal) || name.EndsWith("-line-sibling.pbgra32", StringComparison.Ordinal)),
        "siblings" => name.Contains("-fill-", StringComparison.Ordinal),
        _ => true
    };

    private static void Render(string root, string role, string group)
    {
        string selected = role switch { "candidate" => AcceptancePaths.AotDll, "baseline" => Environment.GetEnvironmentVariable("CQ_BASELINE_DLL") ?? AcceptancePaths.AotDll, _ => AcceptancePaths.OriginalDll };
        nint module = 0;
        foreach (Assembly owner in new[] { Assembly.Load("PresentationCore"), Assembly.Load("WindowsBase") })
            NativeLibrary.SetDllImportResolver(owner, (name, _, _) =>
            {
                if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
                return module == 0 ? module = NativeLibrary.Load(selected) : module;
            });
        string directory = Path.Combine(root, role);
        Directory.CreateDirectory(directory);
        if (group == "device") CurveDeviceQuantizationExperiment.Render(directory);
        else
        {
            Type renderer = group switch { "frames" => typeof(RoundedRectangleContractTests), "precision" => typeof(BwCurvedFillPrecisionTests), _ => typeof(BmFillControlPointTests) };
            string method = group == "frames" ? "Render" : "RenderContent";
            object[] arguments = group == "frames" ? [directory, true] : [directory];
            (renderer.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException()).Invoke(null, arguments);
            if (group == "frames")
                Save(Path.Combine(directory, "scene-contract.json"), new { width = 40, height = 32, stride = 160, frames = new[] { "rounded(4,4,32,24;8,6) red", "rounded(4,4,32,24;-80,60) blue", "rect(4,4,32,24) red", "ellipse(20,16;16,12) blue", "rounded cubic k=.5522847498307933984 rx8 ry6 red", "rounded cubic k=.5522847498307933984 rx16 ry12 blue", "cubic controlX4 red", "cubic controlX4.001 blue", "cubic controlX4.02 red", "cubic controlX4.04 blue" } });
            if (group == "siblings")
            {
                string primitives = Path.Combine(directory, "primitives");
                Directory.CreateDirectory(primitives);
                (typeof(BzCurvedPrimitiveTests).GetMethod("RenderContent", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException()).Invoke(null, [primitives]);
                foreach (string file in Directory.GetFiles(primitives)) File.Copy(file, Path.Combine(directory, "primitive-" + Path.GetFileName(file)), true);
            }
        }
        Assert.AreNotEqual((nint)0, module);
        string actual = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        Assembly tests = typeof(CqFillExactMatrixTests).Assembly;
        Save(Path.Combine(directory, "identity.json"), new { role, requested = selected, actual, sha256 = Hash(File.ReadAllBytes(actual)), version = FileVersionInfo.GetVersionInfo(actual).FileVersion, testsSha = Hash(File.ReadAllBytes(tests.Location)), testsMvid = tests.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, backend = "RenderTargetBitmap software", hardwareVerified = false });
    }

    private static void WriteRoleEvidence(string root, string[] roles)
    {
        var lines = new List<string>();
        foreach (string role in roles)
        {
            string directory = Path.Combine(root, role);
            lines.Add(role + " identity=" + JsonSerializer.Serialize(JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "identity.json"))).RootElement));
            lines.Add(role + " supervision=" + JsonSerializer.Serialize(JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "supervision.json"))).RootElement));
            var trx = System.Xml.Linq.XDocument.Load(Path.Combine(directory, "TestResults", "child.trx"));
            lines.Add(role + " TRX=" + trx.Descendants().Single(e => e.Name.LocalName == "Counters").ToString(System.Xml.Linq.SaveOptions.DisableFormatting));
        }
        File.WriteAllLines(Path.Combine(root, "role-evidence.txt"), lines);
    }

    private static int Differences(byte[] a, byte[] b) { Assert.AreEqual(a.Length, b.Length); return a.Zip(b).Count(p => p.First != p.Second); }
    private static bool Nonempty(byte[] pixels) => pixels.Where((_, i) => i % 4 == 3).Any(b => b != 0);
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static void Save(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value, Options));
}
#endif
