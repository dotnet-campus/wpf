using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class JmResolverIsolationTests
{
    /// <summary>Runs repeated rounded and mixed sampler families without changing the parent's resolver or dispatcher.</summary>
    [STATestMethod]
    [Timeout(115000)]
    public async Task WhenResolverFamiliesRepeatThenParentStateAndBusinessProcessesRemainIndependentAsync()
    {
        Assert.IsNull(Environment.GetEnvironmentVariable("JM_ROUNDED_ROLE"));
        string root = NativeRun.PathsRoot;
        Thread thread = Thread.CurrentThread;
        Dispatcher? dispatcher = Dispatcher.FromThread(thread);
        bool? started = dispatcher?.HasShutdownStarted, finished = dispatcher?.HasShutdownFinished;
        Assembly[] assemblies = [typeof(Visual).Assembly, typeof(Dispatcher).Assembly];
        object?[] before = assemblies.Select(Resolver).ToArray();
        void Snapshot(string phase) => File.WriteAllText(Path.Combine(root, "jm-parent-" + phase + ".json"), JsonSerializer.Serialize(new
        {
            phase, pid = Environment.ProcessId, dispatcherPresent = Dispatcher.FromThread(thread) is not null,
            sameDispatcher = ReferenceEquals(dispatcher, Dispatcher.FromThread(thread)),
            shutdownStarted = dispatcher?.HasShutdownStarted, shutdownFinished = dispatcher?.HasShutdownFinished,
            resolvers = assemblies.Select((assembly, index) => new { assembly = assembly.FullName, mvid = assembly.ManifestModule.ModuleVersionId, present = Resolver(assembly) is not null, unchanged = ReferenceEquals(before[index], Resolver(assembly)) })
        }, new JsonSerializerOptions { WriteIndented = true }));
        Snapshot("before");
        try
        {
            var rounded = new RoundedRectangleContractTests();
            await rounded.WhenRoundedContentChangesThenOfficialCornersAndFreshPixelsMatch();
            await new OffscreenSamplerTests().WhenOffscreenSamplerRoleRunsThenConsumerPixelsChange();
            await rounded.WhenCurveControlPointsCrossQuantizationBoundaryThenIndependentOfficialPixelsMatch();
            await rounded.WhenRoundedContentChangesThenOfficialCornersAndFreshPixelsMatch();
            new BitmapRenderTargetTests().WhenBitmapTargetBeginsNativeFrameThenInternalTargetCanBeAcquired();
            foreach (int index in Enumerable.Range(0, assemblies.Length)) Assert.AreSame(before[index], Resolver(assemblies[index]));
            Assert.AreSame(dispatcher, Dispatcher.FromThread(thread));
            Assert.AreEqual(started, dispatcher?.HasShutdownStarted);
            Assert.AreEqual(finished, dispatcher?.HasShutdownFinished);
            string[] identities = Directory.GetFiles(root, "identity.json", SearchOption.AllDirectories)
                .Where(path => path.Contains("rounded-ah", StringComparison.Ordinal) || path.Contains("curve-an", StringComparison.Ordinal))
                .Where(path => Path.GetFileName(Path.GetDirectoryName(path)) is "original" or "candidate")
                .ToArray();
            int[] pids = identities.Select(path =>
            {
                using var identity = JsonDocument.Parse(File.ReadAllText(path));
                return identity.RootElement.GetProperty("pid").GetInt32();
            }).Distinct().ToArray();
            Assert.IsGreaterThanOrEqualTo(6, pids.Length);
            Assert.IsFalse(pids.Contains(Environment.ProcessId));
            File.WriteAllText(Path.Combine(root, "jm-business-pids.json"), JsonSerializer.Serialize(pids));
        }
        finally { Snapshot("after"); }
    }

    private static object? Resolver(Assembly assembly)
    {
        FieldInfo? field = typeof(NativeLibrary).GetField("s_nativeDllResolveMap", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(field);
        object? map = field.GetValue(null);
        if (map is null) return null;
        MethodInfo? lookup = map.GetType().GetMethod("TryGetValue");
        Assert.IsNotNull(lookup);
        object?[] arguments = [assembly, null];
        bool found = (bool)(lookup.Invoke(map, arguments) ?? throw new InvalidOperationException());
        return found ? arguments[1] : null;
    }
}
