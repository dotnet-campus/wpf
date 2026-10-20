using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static WpfGfxShape.ComAcceptance.ReplacementConstrainedSequenceTests;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class ReplacementDetectionControlTests
{
    /// <summary>Feeds mutated real output to the same detector used by the official differential.</summary>
    [DataTestMethod]
    [DataRow("wrong-color")]
    [DataRow("old-frame")]
    public void WhenRealFrameIsMutatedThenSharedDetectorRejectsIt(string mutation)
    {
        string root = NewRoot(mutation);
        Input input = new(104729, 2, 1, [new(true, 0, 0, 2, 1, 0), new(true, 0, 0, 2, 1, 1)]);
        var api = new Api(AcceptancePaths.OriginalDll);
        Observation original = Execute(api, input);
        Save(Path.Combine(root, "identity.json"), api.Identity);
        Save(Path.Combine(root, "input.json"), input);
        Save(Path.Combine(root, "original.json"), original);
        Assert.IsNull(Detect(input, original, original));
        byte[][] changed = original.Frames.Select(frame => frame.ToArray()).ToArray();
        if (mutation == "old-frame") changed[1] = changed[0].ToArray();
        else changed[1][0] ^= 255;
        Observation injected = new(changed, original.Results);
        string? failure = Detect(input, original, injected);
        Save(Path.Combine(root, "injected.json"), injected);
        Save(Path.Combine(root, "detection.json"), new { Mutation = mutation, Failure = failure });
        Assert.IsNotNull(failure, root);
    }

    /// <summary>Allows a legal no-op overwrite while still checking its real pixels and the independent differential.</summary>
    [TestMethod]
    public void WhenClipRewritesExistingColorThenUnchangedFrameIsLegal()
    {
        string root = NewRoot("legal-no-change");
        Input input = new(104729, 2, 1, [new(true, 0, 0, 2, 1, 0), new(false, 0, 0, 1, 1, 1), new(false, 1, 0, 2, 1, 0)]);
        var official = new Api(AcceptancePaths.OriginalDll);
        var candidate = new Api(AcceptancePaths.AotDll);
        Observation left = Execute(official, input), right = Execute(candidate, input);
        Save(Path.Combine(root, "identity.json"), new { Official = official.Identity, Candidate = candidate.Identity });
        Save(Path.Combine(root, "input.json"), input);
        Save(Path.Combine(root, "official.json"), left);
        Save(Path.Combine(root, "candidate.json"), right);
        Assert.IsNull(Detect(input, left, right), root);
    }

    /// <summary>Checks real event-proxy callback ordering, payload and final disposal on both modules.</summary>
    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenRealCallbacksRunThenSharedLifecycleDetectorAcceptsAndRejectsMutations(bool candidate)
    {
        string root = NewRoot("callback-" + candidate);
        var api = new Api(candidate ? AcceptancePaths.AotDll : AcceptancePaths.OriginalDll);
        var trace = new List<int>();
        GCHandle handle = GCHandle.Alloc(trace);
        nint proxy = 0;
        int[] expected = [17, 63, 101, 209, -1];
        try
        {
            Descriptor descriptor = new() { Dispose = &OnDispose, Raise = &OnRaise, Handle = (nuint)GCHandle.ToIntPtr(handle) };
            var create = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)NativeLibrary.GetExport(api.Module, "MILCreateEventProxy");
            Assert.AreEqual(0, create(&descriptor, &proxy));
            for (int index = 0; index < expected.Length - 1; index++)
            {
                byte value = (byte)expected[index];
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte*, uint, int>)(*(void***)proxy)[3])(proxy, &value, 1));
            }
        }
        finally
        {
            try { Release(proxy); }
            finally { handle.Free(); }
        }
        Save(Path.Combine(root, "identity.json"), api.Identity);
        Save(Path.Combine(root, "expected.json"), expected);
        Save(Path.Combine(root, "actual.json"), trace);
        Assert.IsNull(DetectLifecycle(expected, trace), root);
        Dictionary<string, int[]> mutations = new()
        {
            ["missing-callback"] = trace.Where((_, index) => index != 1).ToArray(),
            ["duplicate-callback"] = [trace[0], trace[0], .. trace.Skip(1)],
            ["missing-release"] = trace.Where(value => value != -1).ToArray(),
            ["duplicate-release"] = [.. trace, -1],
            ["after-release"] = [.. trace, 17],
            ["reordered-callback"] = [trace[1], trace[0], .. trace.Skip(2)]
        };
        foreach ((string name, int[] injected) in mutations)
        {
            string? failure = DetectLifecycle(expected, injected);
            Save(Path.Combine(root, name + ".json"), new { Trace = injected, Failure = failure });
            Assert.IsNotNull(failure, name + ":" + root);
        }
    }

    /// <summary>Exercises deletion reduction using the production differential detector, not an unrelated predicate.</summary>
    [TestMethod]
    public void WhenInjectedColorFailureShrinksThenReplayRetainsOneNecessaryBadOperation()
    {
        string root = NewRoot("shrink-control");
        Input input = new(104729, 2, 1, [new(true, 0, 0, 2, 1, 0), new(true, 0, 0, 2, 1, 1), new(true, 0, 0, 2, 1, 2), new(true, 0, 0, 2, 1, 1)]);
        var api = new Api(AcceptancePaths.OriginalDll);
        int attempts = 0;
        Input reduced = Reduce(input, trial =>
        {
            Observation official = Execute(api, trial);
            byte[][] frames = official.Frames.Select(frame => frame.ToArray()).ToArray();
            for (int index = 0; index < trial.Operations.Length; index++)
                if (trial.Operations[index].Color == 2) frames[index][0] ^= 255;
            Observation injected = new(frames, official.Results);
            string? failure = Detect(trial, official, injected);
            Save(Path.Combine(root, "trial-" + (++attempts) + ".json"), new { Input = trial, Official = official, Injected = injected, Failure = failure });
            return failure == "content";
        });
        Save(Path.Combine(root, "identity.json"), api.Identity);
        Save(Path.Combine(root, "input.json"), input);
        Save(Path.Combine(root, "minimal-input.json"), reduced);
        Save(Path.Combine(root, "summary.json"), new { Attempts = attempts, Remaining = reduced.Operations.Length, Injected = true });
        Assert.AreEqual(2, reduced.Operations.Length, root);
    }

    internal static string? DetectLifecycle(IReadOnlyList<int> expected, IReadOnlyList<int> actual)
    {
        if (actual.Count != expected.Count) return "event-count";
        for (int index = 0; index < expected.Count; index++)
            if (actual[index] != expected[index]) return "event-order-or-payload";
        return null;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal delegate* unmanaged[Stdcall]<Descriptor*, void> Dispose;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, int> Raise;
        internal nuint Handle;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void OnDispose(Descriptor* descriptor)
        => ((List<int>)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!).Add(-1);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int OnRaise(Descriptor* descriptor, byte* bytes, uint length)
    {
        ((List<int>)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!).Add(length == 1 ? bytes[0] : -2);
        return 0;
    }
}
