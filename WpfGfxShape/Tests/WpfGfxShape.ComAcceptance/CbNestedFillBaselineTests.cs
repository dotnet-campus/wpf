#if CB_ARCHIVED_FILL_DIAGNOSTICS
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class CbNestedFillBaselineTests
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>Records independent official, withdrawn baseline and BZ archive pixels without treating known product differences as passing compatibility.</summary>
    [TestMethod]
    public async Task WhenArchivedModulesRenderThenNestedBaselineIsClassified()
    {
        string root = Path.GetFullPath(Path.Combine(NativeRun.PathsRoot, "cb-baseline"));
        string? role = Environment.GetEnvironmentVariable("CB_FILL_ROLE");
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
        string acquisitionFile = Path.Combine(AppContext.BaseDirectory, "archive-source.json");
        if (File.Exists(acquisitionFile))
        {
            using JsonDocument acquisition = JsonDocument.Parse(File.ReadAllText(acquisitionFile));
            string source = acquisition.RootElement.GetProperty("path").GetString() ?? throw new InvalidOperationException();
            Save(Path.Combine(AppContext.BaseDirectory, "archive-acquisition.json"), new { source, exists = File.Exists(source) });
            Assert.IsTrue(File.Exists(source), "Required original archive is absent; no substitute module is permitted.");
            Assert.AreEqual(acquisition.RootElement.GetProperty("sha256").GetString(), Hash(source));
            string destination = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "baseline-" + RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(), "Native", "wpfgfx_cor3.dll"));
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? throw new InvalidOperationException());
            File.Copy(source, destination, true);
        }
        foreach (string selected in new[] { "official-old", "old", "official-new", "new" })
        {
            string directory = Path.Combine(root, selected);
            Directory.CreateDirectory(directory);
            var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException())
            { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = AppContext.BaseDirectory };
            foreach (string argument in new[] { "--filter", "FullyQualifiedName~CbNestedFillBaselineTests", "--report-trx", "--report-trx-filename", "child.trx", "--results-directory", Path.Combine(directory, "TestResults") })
                start.ArgumentList.Add(argument);
            start.Environment["CB_FILL_ROLE"] = selected;
            using var process = Process.Start(start) ?? throw new InvalidOperationException();
            Task<string> stdout = process.StandardOutput.ReadToEndAsync(), stderr = process.StandardError.ReadToEndAsync();
            bool timedOut = false;
            DateTimeOffset started = DateTimeOffset.UtcNow;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { timedOut = true; process.Kill(true); await process.WaitForExitAsync(); }
            File.WriteAllText(Path.Combine(directory, "stdout.txt"), await stdout);
            File.WriteAllText(Path.Combine(directory, "stderr.txt"), await stderr);
            Save(Path.Combine(directory, "supervision.json"), new { started, ended = DateTimeOffset.UtcNow, process.ExitCode, timedOut, killAttempted = timedOut, process.HasExited, arguments = start.ArgumentList.ToArray() });
            Assert.IsFalse(timedOut);
            Assert.AreEqual(0, process.ExitCode);
            Assert.IsTrue(File.Exists(Path.Combine(directory, "TestResults", "child.trx")));
        }
        var results = new List<object>();
        foreach (string file in Directory.GetFiles(Path.Combine(root, "old"), "*.pbgra32").Order())
        {
            string name = Path.GetFileName(file);
            byte[] old = Read("old"), current = Read("new"), officialOld = Read("official-old"), officialNew = Read("official-new");
            byte[] Read(string selected) => File.ReadAllBytes(Path.Combine(root, selected, name));
            bool nonempty = new[] { old, current, officialOld, officialNew }.All(p => p.Where((_, i) => i % 4 == 3).Any(b => b != 0));
            int oldDifference = Difference(old, officialOld), newDifference = Difference(current, officialNew);
            results.Add(new { name, oldDifference, newDifference, oldNewDifference = Difference(old, current), officialRepeatDifference = Difference(officialOld, officialNew), nonempty,
                oldSha = Convert.ToHexString(SHA256.HashData(old)), newSha = Convert.ToHexString(SHA256.HashData(current)), officialOldSha = Convert.ToHexString(SHA256.HashData(officialOld)), officialNewSha = Convert.ToHexString(SHA256.HashData(officialNew)) });
            Assert.IsTrue(nonempty, name);
            Assert.AreEqual(0, Difference(officialOld, officialNew), name);
        }
        Save(Path.Combine(root, "comparison.json"), new { threshold = 0, hardwareVerified = false, evidenceOnly = true, results });
    }

    private static int Difference(byte[] left, byte[] right)
    {
        Assert.AreEqual(left.Length, right.Length);
        return Enumerable.Range(0, left.Length).Count(i => left[i] != right[i]);
    }

    private static void Save(string path, object value) => File.WriteAllText(path, JsonSerializer.Serialize(value, Options));

    private static void Render(string root, string role)
    {
        string selected = role.StartsWith("official-", StringComparison.Ordinal) ? AcceptancePaths.OriginalDll : role == "new" ? AcceptancePaths.AotDll
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "baseline-" + RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(), "Native", "wpfgfx_cor3.dll"));
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
        Assembly tests = typeof(CbNestedFillBaselineTests).Assembly;
        Save(Path.Combine(directory, "identity.json"), new { role, requested = selected, actual, moduleSha = Hash(actual), version = FileVersionInfo.GetVersionInfo(actual).FileVersion,
            testsSha = Hash(tests.Location), testsMvid = tests.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, backend = "RenderTargetBitmap software", hardwareVerified = false });
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}
#endif
