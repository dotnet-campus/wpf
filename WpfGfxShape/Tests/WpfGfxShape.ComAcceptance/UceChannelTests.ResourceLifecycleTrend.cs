#if RESOURCE_LIFECYCLE_TREND
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    /// <summary>Runs the opt-in, fixed same-process lifecycle sequence in isolated official-first roles.</summary>
    [TestMethod]
    public Task WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync() =>
        ResourceTrendCoordinator.RunAsync();

    internal static int RunResourceTrendRole(string role)
    {
        ResourceTrendCoordinator.RequireOptIn();
        if (role is not ("original" or "candidate"))
            throw new ArgumentException(new ResourceManager("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(NativeRun).Assembly)
                .GetString("ResourceTrendRole", CultureInfo.CurrentUICulture), nameof(role));
        string root = NativeRun.PathsRoot;
        Directory.CreateDirectory(root);
        var states = new List<SharedRoundState>();
        var samples = new List<ResourceTrendSample>();
#if RESOURCE_LIFECYCLE_HANDLE_TYPES
        var handleTypes = new ProcessHandleTypeSnapshot();
#endif
        int completedRounds = 0, activeRound = -1;
        string phase = "RoleStart";
        SharedRoundState? current = null;
        object deadlineLock = new();
        bool stopped = false;
        void Checkpoint(string status, string? error = null)
        {
            lock (deadlineLock) ResourceTrendCoordinator.Save(Path.Combine(root, "progress.json"), new
        {
            role, pid = Environment.ProcessId, status, activeRound, phase, completedRounds,
            unexecutedRounds = Enumerable.Range(Math.Max(0, activeRound + 1), 16 - Math.Max(0, activeRound + 1)).ToArray(),
            unfinishedRound = activeRound >= completedRounds ? activeRound : (int?)null,
            completed = completedRounds == 16 && status == "Passed", utc = DateTimeOffset.UtcNow,
            qpc = Stopwatch.GetTimestamp(), error, workerExit = "NotProven: callback exit is not OS thread exit",
            watchdogHardRealtime = false,
            resourceCleanup = status == "TimedOut" ? "Unknown: process termination is not resource release" : "SeeRoundBusiness"
        });
        }
        void Timeout(string boundary)
        {
            lock (deadlineLock)
            {
                if (stopped) return;
                try { Checkpoint("TimedOut", boundary); }
                catch (Exception error) { Console.Error.WriteLine(error); }
                // Best-effort thread-pool watchdog; outer supervision owns the larger boundary.
                try
                {
                    using var self = Process.GetCurrentProcess();
                    self.Kill();
                }
                catch (Exception error) { Console.Error.WriteLine(error); }
            }
        }
        using var roleTimer = new Timer(_ => Timeout("Role360Seconds"), null, TimeSpan.FromSeconds(360), System.Threading.Timeout.InfiniteTimeSpan);
        using var roundTimer = new Timer(_ => Timeout("Round20Seconds"), null, System.Threading.Timeout.InfiniteTimeSpan, System.Threading.Timeout.InfiniteTimeSpan);
        void ValidateCallbacks()
        {
            foreach (var item in states)
            {
                Assert.AreEqual(1, Volatile.Read(ref item.CallbackEntries));
                Assert.AreEqual(1, Volatile.Read(ref item.CallbackExited));
                Assert.AreEqual(1, Volatile.Read(ref item.Disposals));
                Assert.IsTrue(item.CallbackErrors.IsEmpty);
            }
        }
        void Sample(string point)
        {
            phase = point;
            var sample = ResourceTrendSample.Capture(role, activeRound, point, states, current);
            samples.Add(sample);
            File.AppendAllText(Path.Combine(root, "samples.jsonl"), JsonSerializer.Serialize(sample) + Environment.NewLine);
            Checkpoint("Running");
#if RESOURCE_LIFECYCLE_HANDLE_TYPES
            if (ProcessHandleTypeSnapshot.IsPoint(activeRound, point))
            {
                var observed = current is not null && !states.Contains(current) ? states.Append(current) : states;
                handleTypes.Append(root, role, activeRound, point, observed.Select(s => s.CallbackThread).ToArray());
            }
#endif
        }
        try
        {
            Checkpoint("Starting");
            nint module = NativeLibrary.Load(Path.GetFullPath(role == "original" ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll));
            Sample("Initial");
            for (int round = 0; round < 16; round++)
            {
                lock (deadlineLock)
                {
                    activeRound = round;
                    roundTimer.Change(TimeSpan.FromSeconds(20), System.Threading.Timeout.InfiniteTimeSpan);
                }
                ValidateCallbacks();
                current = null;
                Sample("BeforeInitialize");
                string roundRoot = Path.Combine(root, $"round-{round:D2}");
                Directory.CreateDirectory(roundRoot);
                var state = new SharedRoundState { CallbackPath = Path.Combine(roundRoot, "callbacks.jsonl"),
                    EventPath = Path.Combine(roundRoot, "events.jsonl") };
                current = state;
                RunAsyncSharedOwnership(role == "original", module, roundRoot, state, () => Sample("AfterSourceCloseTargetUpdate"));
                states.Add(state);
                ValidateCallbacks();
                Sample("AfterCleanup");
                Thread.Sleep(TimeSpan.FromSeconds(1));
                ValidateCallbacks();
                Sample("AfterCleanupPlusOneSecond");
                lock (deadlineLock)
                {
                    completedRounds++;
                    roundTimer.Change(System.Threading.Timeout.InfiniteTimeSpan, System.Threading.Timeout.InfiniteTimeSpan);
                }
                Checkpoint("Running");
            }
            Assert.AreEqual(65, samples.Count);
            ValidateCallbacks();
            ResourceTrendCoordinator.Save(Path.Combine(root, "descriptive.json"), ResourceTrendSample.Describe(samples));
#if RUNTIME_HEAP_OWNER_DIAGNOSTIC
            if (role == "candidate")
            {
                var observe = (delegate* unmanaged[Stdcall]<uint, long*, uint, int>)NativeLibrary.GetExport(module, "WpfGfxShape_RuntimeHeapDiagnostic_v1");
                void ObserveHeap(string boundary, uint operation)
                {
                    long* values = stackalloc long[6];
                    int result = observe(operation, values, 6);
                    Assert.AreEqual(0, result);
                    ResourceTrendCoordinator.Save(Path.Combine(root, boundary + ".json"), new
                    {
                        boundary, diagnosticOnly = true, memoryGreen = (bool?)null,
                        modulePath = Path.GetFullPath(AcceptancePaths.AotDll),
                        moduleSha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(AcceptancePaths.AotDll))),
                        tracked = values[0], schedulerAlive = values[1], threadAlive = values[2],
                        dllGc0 = values[3], dllGc1 = values[4], dllGc2 = values[5],
                        hostGc0 = GC.CollectionCount(0), hostGc1 = GC.CollectionCount(1), hostGc2 = GC.CollectionCount(2)
                    });
                    handleTypes.Append(root, role, activeRound, boundary, states.Select(s => s.CallbackThread).ToArray());
                }
                ObserveHeap("BeforeHostGc", 0);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                ObserveHeap("AfterHostGc", 0);
                ObserveHeap("AfterDllGc", 1);
            }
#endif
            lock (deadlineLock) { stopped = true; Checkpoint("Passed"); }
            return 0;
        }
        catch (Exception error)
        {
            lock (deadlineLock)
            {
                stopped = true;
                try { Checkpoint("Failed", error.ToString()); }
                catch (Exception saveError) { error.Data["ProgressSaveFailure"] = saveError.ToString(); Console.Error.WriteLine(error); }
            }
            return 1;
        }
    }

    private sealed record ResourceTrendSample(string Role, int Round, string Phase, string Stage, int Pid,
        DateTimeOffset Utc, long Qpc, long QpcFrequency, Dictionary<string, double?> Metrics,
        Dictionary<string, string> Errors, int CallbackEntries, int CallbackExits, int Disposals,
        uint[] CallbackThreads, int RoundCallbackEntries, int RoundCallbackExits, int RoundDisposals,
        SharedKnownOwners? KnownOwners, string KnownOwnership, string WorkerExit)
    {
        internal static ResourceTrendSample Capture(string role, int round, string phase, List<SharedRoundState> states, SharedRoundState? current)
        {
            var metrics = new Dictionary<string, double?>();
            var errors = new Dictionary<string, string>();
            void Read(string name, Func<double> read)
            {
                try { metrics[name] = read(); }
                catch (Exception error) { metrics[name] = null; errors[name] = error.ToString(); }
            }
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            Read("PrivateBytes", () => process.PrivateMemorySize64);
            Read("WorkingSet", () => process.WorkingSet64);
            Read("Handles", () => process.HandleCount);
            Read("Threads", () =>
            {
                var threads = process.Threads;
                try { return threads.Count; }
                finally { foreach (ProcessThread thread in threads) thread.Dispose(); }
            });
            Read("ManagedBytes", () => GC.GetTotalMemory(false));
            Read("NaturalGC0", () => GC.CollectionCount(0));
            Read("NaturalGC1", () => GC.CollectionCount(1));
            Read("NaturalGC2", () => GC.CollectionCount(2));
            var observed = current is not null && !states.Contains(current) ? states.Append(current).ToArray() : states.ToArray();
            return new(role, round, phase, round < 0 ? "Initial" : round < 4 ? "Warmup" : "Measurement",
                Environment.ProcessId, DateTimeOffset.UtcNow, Stopwatch.GetTimestamp(), Stopwatch.Frequency,
                metrics, errors, observed.Sum(s => Volatile.Read(ref s.CallbackEntries)),
                observed.Sum(s => Volatile.Read(ref s.CallbackExited)), observed.Sum(s => Volatile.Read(ref s.Disposals)),
                observed.Select(s => s.CallbackThread).ToArray(),
                current is null ? 0 : Volatile.Read(ref current.CallbackEntries),
                current is null ? 0 : Volatile.Read(ref current.CallbackExited),
                current is null ? 0 : Volatile.Read(ref current.Disposals),
                current?.KnownOwners,
                phase is "AfterCleanup" or "AfterCleanupPlusOneSecond" ? "ActualRoundBusinessCleanupVerified" : "SeeRoundEventsAndBusiness",
                "NotProven: callback thread identity recorded; OS thread exit not asserted");
        }

        internal static object Describe(List<ResourceTrendSample> samples)
        {
            var baseline = samples.Single(s => s.Round == 3 && s.Phase == "AfterCleanupPlusOneSecond");
            var summaries = new List<object>();
            foreach (string phase in samples.Where(s => s.Round >= 4).Select(s => s.Phase).Distinct())
            foreach (string metric in baseline.Metrics.Keys)
            {
                var points = samples.Where(s => s.Round >= 4 && s.Phase == phase).OrderBy(s => s.Round).ToArray();
                double? origin = baseline.Metrics[metric];
                bool known = points.Length == 12 && origin.HasValue && points.All(s => s.Metrics[metric].HasValue);
                double[] values = known ? points.Select(s => s.Metrics[metric]!.Value).ToArray() : [];
                object[] windows = known ? Enumerable.Range(0, 3).Select(w =>
                {
                    double[] v = values.Skip(w * 4).Take(4).Order().ToArray();
                    return (object)new { firstRound = 4 + w * 4, lastRound = 7 + w * 4,
                        min = v[0], max = v[3], median = (v[1] + v[2]) / 2 };
                }).ToArray() : [];
                double? slope = known ? Enumerable.Range(0, 12).Sum(i => (i - 5.5) * values[i]) /
                    Enumerable.Range(0, 12).Sum(i => (i - 5.5) * (i - 5.5)) : null;
                summaries.Add(new { phase, metric, status = known ? "DescriptiveOnly" : "Unknown",
                    baseline = origin, slopePerRound = slope, firstLastDelta = known ? values[^1] - values[0] : (double?)null,
                    deltas = points.Select(s => new { s.Round, value = s.Metrics[metric],
                        delta = origin.HasValue && s.Metrics[metric].HasValue ? s.Metrics[metric] - origin : null }).ToArray(), windows });
            }
            return new { conclusion = "DescriptiveOnly", memoryGreen = (bool?)null, forcedGC = false,
                workerExit = "NotProven", samples = samples.Count, summaries };
        }
    }
}

internal static class ResourceTrendCoordinator
{
    internal const string SupervisionFilter = "FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests." +
        nameof(UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync);
    internal const string OptInEnvironment = "WPFGFX_RESOURCE_TREND";
    internal const string OptInValue = "async-shared-wic-v1";
    private static readonly ResourceManager Resources = new("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(NativeRun).Assembly);
    internal static void ValidateOptIn(string? value)
    {
        if (value != OptInValue)
            throw new InvalidOperationException(Resources.GetString("ResourceTrendOptIn", CultureInfo.CurrentUICulture));
    }

    internal static void RequireOptIn()
    {
        ValidateOptIn(Environment.GetEnvironmentVariable(OptInEnvironment));
    }

    internal static void Save(string path, object value) => File.WriteAllText(path,
        JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));

    internal static async Task RunAsync()
    {
        // This compile-gated, dedicated coordinator is the explicit opt-in; never alter the process environment.
        const string optIn = OptInValue;
        ValidateOptIn(optIn);
        string root = Path.Combine(NativeRun.PathsRoot, "ResourceLifecycleTrend", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        using var parent = new CancellationTokenSource(TimeSpan.FromSeconds(840));
        string status = "Running";
        string? failure = null;
        var roles = new List<object>();
        try
        {
            foreach (string role in new[] { "original", "candidate" })
            {
                string roleRoot = Path.Combine(root, role);
                int exit = await NativeRun.ResourceTrendRoleAsync(roleRoot, role, optIn, parent.Token).ConfigureAwait(false);
                using var report = JsonDocument.Parse(File.ReadAllText(Path.Combine(roleRoot, "progress.json")));
                bool passed = exit == 0 && report.RootElement.GetProperty("completed").GetBoolean() &&
                    report.RootElement.GetProperty("completedRounds").GetInt32() == 16 &&
                    report.RootElement.GetProperty("status").GetString() == "Passed" &&
                    File.ReadLines(Path.Combine(roleRoot, "samples.jsonl")).Count() == 65;
                roles.Add(new { role, exit, passed, progress = report.RootElement.Clone() });
                Save(Path.Combine(root, "comparison.json"), new { status, roles, conclusion = "DescriptiveOnly", workerExit = "NotProven" });
                Assert.IsTrue(passed, role);
            }
            status = "PassedKnownOwnershipAndCallbacksOnly";
        }
        catch (Exception error)
        {
            status = "Failed"; failure = error.ToString();
            try { Save(Path.Combine(root, "comparison.json"), new { status, roles, failure, memoryConclusion = "DescriptiveOnly", workerExit = "NotProven" }); }
            catch (Exception saveError) { error.Data["ComparisonSaveFailure"] = saveError.ToString(); }
            throw;
        }
        Save(Path.Combine(root, "comparison.json"), new { status, roles, failure,
            memoryConclusion = "DescriptiveOnly", workerExit = "NotProven", longTermLeakFreedom = "NotProven" });
    }
}
#endif
