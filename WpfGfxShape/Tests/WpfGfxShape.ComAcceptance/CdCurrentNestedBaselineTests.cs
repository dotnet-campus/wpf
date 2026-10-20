#if CD_CURRENT_FILL_DIAGNOSTICS
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class CdCurrentNestedBaselineTests
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>Collects current-source pixels without treating collection success as exact compatibility.</summary>
    [TestMethod]
    public async Task WhenCurrentModulesRenderThenBaselineEvidenceIsRecorded()
    {
        string root = Environment.GetEnvironmentVariable("CD_FILL_ROOT") ?? Path.GetFullPath(Path.Combine(NativeRun.PathsRoot, "cd-baseline"));
        string? role = Environment.GetEnvironmentVariable("CD_FILL_ROLE");
        if (role is not null)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { Render(root, role); }
                catch (Exception error) { failure = error; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(30)));
            if (failure is not null) throw new AssertFailedException(failure.ToString());
            return;
        }
        foreach (string selected in new[] { "official", "baseline", "candidate", "official-repeat" })
        {
            string directory = Path.Combine(root, selected);
            Directory.CreateDirectory(directory);
            Environment.SetEnvironmentVariable("CD_FILL_ROLE", selected);
            Environment.SetEnvironmentVariable("CD_FILL_ROOT", root);
            int exit;
            try { exit = await ExitDiagnostics.RunAsync([directory, "45", "FullyQualifiedName~CdCurrentNestedBaselineTests"]); }
            finally
            {
                Environment.SetEnvironmentVariable("CD_FILL_ROLE", null);
                Environment.SetEnvironmentVariable("CD_FILL_ROOT", null);
            }
            Assert.AreEqual(0, exit, selected);
        }
        var results = new List<object>();
        bool exact = true;
        string[] files = Directory.GetFiles(Path.Combine(root, "official"), "*.pbgra32").Order().ToArray();
        Assert.AreEqual(18, files.Length);
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            byte[] official = File.ReadAllBytes(file);
            byte[] actual = File.ReadAllBytes(Path.Combine(root, "candidate", name));
            byte[] baseline = File.ReadAllBytes(Path.Combine(root, "baseline", name));
            Assert.AreEqual(official.Length, baseline.Length);
            byte[] repeat = File.ReadAllBytes(Path.Combine(root, "official-repeat", name));
            Assert.AreEqual(official.Length, actual.Length);
            Assert.AreEqual(official.Length, repeat.Length);
            int[] differences = Enumerable.Range(0, official.Length).Where(i => official[i] != actual[i]).ToArray();
            bool nonempty = new[] { official, baseline, actual, repeat }.All(p => p.Where((_, i) => i % 4 == 3).Any(b => b != 0));
            int repeatDifference = Enumerable.Range(0, official.Length).Count(i => official[i] != repeat[i]);
            Save(Path.Combine(root, name + ".diff.json"), differences.Select(i => new { offset = i, expected = official[i], actual = actual[i] }));
            int oldNewDifference = Enumerable.Range(0, official.Length).Count(i => baseline[i] != actual[i]);
            int baselineDifference = Enumerable.Range(0, official.Length).Count(i => baseline[i] != official[i]);
            bool noRegression = Enumerable.Range(0, official.Length).All(i => actual[i] == baseline[i] || Math.Abs(actual[i] - official[i]) < Math.Abs(baseline[i] - official[i]));
            results.Add(new { name, count = differences.Length, baselineDifference, oldNewDifference, noRegression, nonempty, repeatDifference, officialSha = Hash(file), baselineSha = Hash(Path.Combine(root, "baseline", name)), candidateSha = Hash(Path.Combine(root, "candidate", name)) });
            Save(Path.Combine(root, "comparison.json"), new { threshold = 0, evidenceOnly = true, hardwareVerified = false, results });
            Assert.IsTrue(noRegression, name);
            if (name is "rectangle.pbgra32" or "ellipse.pbgra32") Assert.AreEqual(0, differences.Length, name);
            exact &= differences.Length == 0 && nonempty;
            Assert.IsTrue(nonempty, name);
            Assert.AreEqual(0, repeatDifference, name);
        }
        Save(Path.Combine(root, "comparison.json"), new { threshold = 0, evidenceOnly = true, exact, hardwareVerified = false, results });
    }

    private static void Render(string root, string role)
    {
        string selected = role switch
        {
            "candidate" => AcceptancePaths.AotDll,
            "baseline" => Path.Combine(AppContext.BaseDirectory, "Baseline", "wpfgfx_cor3.dll"),
            _ => AcceptancePaths.OriginalDll
        };
        nint module = 0;
        foreach (Assembly owner in new[] { Assembly.Load("PresentationCore"), Assembly.Load("WindowsBase") })
            NativeLibrary.SetDllImportResolver(owner, (name, _, _) =>
            {
                if (!name.Equals("wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) && !name.Equals("wpfgfx_cor3", StringComparison.OrdinalIgnoreCase)) return 0;
                return module == 0 ? module = NativeLibrary.Load(selected) : module;
            });
        string directory = Path.Combine(root, role);
        Directory.CreateDirectory(directory);
        foreach (Type renderer in new[] { typeof(BzCurvedPrimitiveTests), typeof(BwCurvedFillPrecisionTests) })
            (renderer.GetMethod("RenderContent", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new MissingMethodException(renderer.FullName, "RenderContent")).Invoke(null, [directory]);
        Assert.AreNotEqual((nint)0, module);
        string actual = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        Assembly tests = typeof(CdCurrentNestedBaselineTests).Assembly;
        Save(Path.Combine(directory, "identity.json"), new { role, requested = selected, actual, moduleSha = Hash(actual), version = FileVersionInfo.GetVersionInfo(actual).FileVersion,
            testsSha = Hash(tests.Location), testsMvid = tests.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, backend = "RenderTargetBitmap software", hardwareVerified = false });
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static void Save(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value, Options));
}
#endif
