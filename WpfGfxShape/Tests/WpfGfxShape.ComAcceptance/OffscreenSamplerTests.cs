using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class OffscreenSamplerTests
{
    /// <summary>Compares the original shader and complete consumer pixels across independent software roles.</summary>
    [TestMethod]
    [Timeout(115000)]
    public async Task WhenOffscreenSamplerGroupRunsThenPixelsAndUpdatesMatchOfficialAsync()
    {
        Assert.IsNull(Environment.GetEnvironmentVariable("IY_OFFSCREEN_ROLE"));
        string root = Path.GetFullPath(Path.Combine(NativeRun.PathsRoot, "offscreen-sampler", Guid.NewGuid().ToString("N")));
        var outputs = new List<string>();
        foreach (string role in new[] { "official", "candidate", "official-repeat" })
            outputs.Add(await RunRoleAsync(role, Path.Combine(root, role)).ConfigureAwait(false));
        foreach (string name in new[] { "shader.bin", "initial.bgra", "mutation.bgra" })
        {
            byte[] official = File.ReadAllBytes(Path.Combine(outputs[0], name));
            Assert.AreEqual(name == "shader.bin" ? 304 : 1024, official.Length);
            CollectionAssert.AreEqual(official, File.ReadAllBytes(Path.Combine(outputs[1], name)));
            CollectionAssert.AreEqual(official, File.ReadAllBytes(Path.Combine(outputs[2], name)));
        }
    }

    /// <summary>Runs candidate in a fresh role process by default; only an explicit role executes WPF.</summary>
    [STATestMethod]
    [Timeout(40000)]
    public async Task WhenOffscreenSamplerRoleRunsThenConsumerPixelsChange()
    {
        string? role = Environment.GetEnvironmentVariable("IY_OFFSCREEN_ROLE");
        if (role is null)
        {
            string root = Path.Combine(NativeRun.PathsRoot, "offscreen-sampler-direct", Guid.NewGuid().ToString("N"));
            await RunRoleAsync("candidate", root).ConfigureAwait(false);
        }
        else
        {
            Assert.IsTrue(role is "official" or "official-repeat" or "candidate");
            string? root = Environment.GetEnvironmentVariable("IY_OFFSCREEN_OUTPUT");
            Assert.IsFalse(string.IsNullOrWhiteSpace(root));
            Assert.IsTrue(Path.IsPathFullyQualified(root));
            D3DImageTests.RunOffscreenSampler(role == "candidate", root);
        }
    }

    /// <summary>Checks repeated direct/group calls and a subsequent sibling without parent resolver or dispatcher mutation.</summary>
    [STATestMethod]
    [Timeout(115000)]
    public async Task WhenOffscreenSamplerRepeatsInOneParentThenSiblingAndParentStateRemainUsableAsync()
    {
        Assert.IsNull(Environment.GetEnvironmentVariable("IY_OFFSCREEN_ROLE"));
        Thread parentThread = Thread.CurrentThread;
        Dispatcher? dispatcher = Dispatcher.FromThread(parentThread);
        bool? shutdown = dispatcher?.HasShutdownStarted;
        Assembly[] assemblies = [typeof(Visual).Assembly, typeof(Dispatcher).Assembly];
        bool[] before = assemblies.Select(HasResolver).ToArray();
        for (int iteration = 0; iteration < 2; iteration++)
        {
            await WhenOffscreenSamplerRoleRunsThenConsumerPixelsChange();
            await WhenOffscreenSamplerGroupRunsThenPixelsAndUpdatesMatchOfficialAsync();
            new BitmapRenderTargetTests().WhenBitmapTargetBeginsNativeFrameThenInternalTargetCanBeAcquired();
            CollectionAssert.AreEqual(before, assemblies.Select(HasResolver).ToArray());
            Assert.AreSame(dispatcher, Dispatcher.FromThread(parentThread));
            Assert.AreEqual(shutdown, dispatcher?.HasShutdownStarted);
        }
    }

    private static bool HasResolver(Assembly assembly)
    {
        FieldInfo? field = typeof(NativeLibrary).GetField("s_nativeDllResolveMap", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(field);
        object? map = field.GetValue(null);
        if (map is null) return false;
        MethodInfo? lookup = map.GetType().GetMethod("TryGetValue");
        Assert.IsNotNull(lookup);
        return (bool)(lookup.Invoke(map, [assembly, null]) ?? throw new InvalidOperationException());
    }

    private static async Task<string> RunRoleAsync(string role, string output)
    {
        output = Path.GetFullPath(output);
        Directory.CreateDirectory(output);
        var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException()) { UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory };
        foreach (string argument in new[] { "--supervise-exit", output, "30", "FullyQualifiedName~WhenOffscreenSamplerRoleRunsThenConsumerPixelsChange" }) start.ArgumentList.Add(argument);
        start.Environment["IY_OFFSCREEN_ROLE"] = role;
        start.Environment["IY_OFFSCREEN_OUTPUT"] = Path.Combine(output, "sampler");
        using var process = Process.Start(start) ?? throw new InvalidOperationException();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(35));
        bool timedOut = false;
        try { await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false); }
        catch (OperationCanceledException) { timedOut = true; process.Kill(true); await process.WaitForExitAsync().ConfigureAwait(false); }
        File.WriteAllText(Path.Combine(output, "role-exit.json"), JsonSerializer.Serialize(new { role, parentPid = Environment.ProcessId, pid = process.Id, exitCode = process.ExitCode, timedOut }));
        Assert.IsFalse(timedOut);
        Assert.AreEqual(0, process.ExitCode, output);
        var trx = XDocument.Load(Path.Combine(output, "TestResults", "child.trx"));
        XNamespace ns = trx.Root!.Name.Namespace;
        var counters = trx.Descendants(ns + "Counters").Single();
        Assert.AreEqual("1", (string?)counters.Attribute("executed"));
        Assert.AreEqual("1", (string?)counters.Attribute("passed"));
        string sampler = Path.Combine(output, "sampler");
        using var evidence = JsonDocument.Parse(File.ReadAllText(Path.Combine(sampler, "evidence.json")));
        int childPid = evidence.RootElement.EnumerateArray().Single(e => e.GetProperty("Stage").GetString() == "identity").GetProperty("Detail").GetProperty("Pid").GetInt32();
        Assert.AreNotEqual(Environment.ProcessId, childPid);
        Assert.AreNotEqual(process.Id, childPid);
        Assert.AreEqual(304L, new FileInfo(Path.Combine(sampler, "shader.bin")).Length);
        Assert.AreEqual(1024L, new FileInfo(Path.Combine(sampler, "initial.bgra")).Length);
        Assert.AreEqual(1024L, new FileInfo(Path.Combine(sampler, "mutation.bgra")).Length);
        return sampler;
    }
}
