using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class ReplacementResourceCycleTests
{
    public TestContext TestContext { get; set; } = null!;
    private sealed record Cycle(int Index, uint TargetRelease, uint BitmapRelease, uint FactoryRelease, uint ProxyRelease, int Calls, int Disposals, ulong Sum, byte[][] Frames);
    private sealed record Event(int Cycle, string Name, long Value);
    private struct State { internal int Calls, Disposals; internal ulong Sum; }
    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal delegate* unmanaged[Stdcall]<Descriptor*, void> Dispose;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, int> Raise;
        internal nuint Handle;
    }

    /// <summary>Checks bounded real COM producer/consumer ownership and descriptor destruction against the official module.</summary>
    [TestMethod]
    public void WhenProducerClosesThenConsumerRetainsFreshPixelsAndFinalOwnersRelease()
    {
        string root = Path.Combine(AcceptancePaths.Results, "replacement-resource-z", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var official = new ReplacementConstrainedSequenceTests.Api(AcceptancePaths.OriginalDll);
        var candidate = new ReplacementConstrainedSequenceTests.Api(AcceptancePaths.AotDll);
        Save(Path.Combine(root, "identity.json"), new { Official = official.Identity, Candidate = candidate.Identity, Warmup = 4, Measured = 12, Width = 8, Height = 8, Backend = "Explicit offscreen software bitmap target; not HW or recovery", Isolation = "Two independent modules in one process" });
        Cycle[] left = Run(official, Path.Combine(root, "official"), out Event[] leftEvents);
        Cycle[] right = Run(candidate, Path.Combine(root, "candidate"), out Event[] rightEvents);
        string? failure = Detect(left, leftEvents) ?? Detect(right, rightEvents);
        for (int i = 0; i < left.Length; i++)
        {
            for (int frame = 0; frame < 3; frame++)
                if (!left[i].Frames[frame].SequenceEqual(right[i].Frames[frame])) failure ??= "pixel-difference";
            if (left[i].TargetRelease != right[i].TargetRelease || left[i].BitmapRelease != right[i].BitmapRelease || left[i].FactoryRelease != right[i].FactoryRelease || left[i].ProxyRelease != right[i].ProxyRelease || left[i].Calls != right[i].Calls || left[i].Disposals != right[i].Disposals || left[i].Sum != right[i].Sum)
                failure ??= "lifetime-difference";
        }
        Cycle[] missing = right.ToArray();
        missing[4] = missing[4] with { Disposals = 0 };
        Cycle[] duplicate = right.ToArray();
        duplicate[4] = duplicate[4] with { Disposals = 2 };
        Save(Path.Combine(root, "comparison.json"), new { Failure = failure, Threshold = 0, Cycles = 16, FramesPerRole = 48, MissingReleaseRejected = Detect(missing, rightEvents) is not null, DuplicateReleaseRejected = Detect(duplicate, rightEvents) is not null, Limit = "Negative controls mutate actual observations, not native pointers; memory has no arbitrary pass threshold" });
        var controls = new Dictionary<string, string?>();
        for (int owner = 0; owner < 4; owner++)
        {
            Cycle[] changed = right.ToArray();
            Cycle source = changed[4];
            changed[4] = owner switch
            {
                0 => source with { TargetRelease = 1 },
                1 => source with { BitmapRelease = 1 },
                2 => source with { FactoryRelease = 1 },
                _ => source with { ProxyRelease = 1 }
            };
            string? result = Detect(changed, rightEvents);
            controls.Add($"nonzero-release-{owner}", result);
            Assert.AreEqual("remaining-reference", result, root);
        }
        Event[] cycleEvents = rightEvents.Where(e => e.Cycle == 4).ToArray();
        foreach (Event entry in cycleEvents)
        {
            int position = Array.IndexOf(rightEvents, entry);
            controls.Add($"missing-event-{position}", Detect(right, rightEvents.Where((_, i) => i != position).ToArray()));
            var duplicated = rightEvents.ToList();
            duplicated.Insert(position, entry);
            controls.Add($"duplicate-event-{position}", Detect(right, duplicated.ToArray()));
        }
        var late = rightEvents.ToList();
        int callback = late.FindIndex(e => e.Cycle == 4 && e.Name == "frame-consumed-callbacks");
        Event moved = late[callback];
        late.RemoveAt(callback);
        late.Insert(late.FindIndex(e => e.Cycle == 4 && e.Name == "proxy-released") + 1, moved);
        controls.Add("callback-after-release", Detect(right, late.ToArray()));
        var early = rightEvents.ToList();
        int retained = early.FindIndex(e => e.Cycle == 4 && e.Name == "consumer-read-after-producer-close");
        Event read = early[retained];
        early.RemoveAt(retained);
        early.Insert(early.FindIndex(e => e.Cycle == 4 && e.Name == "producer-released"), read);
        controls.Add("retained-read-before-close", Detect(right, early.ToArray()));
        Save(Path.Combine(root, "detection-controls.json"), new { Controls = controls, Count = controls.Count, ObservationOnly = true });
        TestContext.WriteLine(root);
        Assert.IsNull(failure, root);
        Assert.IsNotNull(Detect(missing, rightEvents), root);
        Assert.IsNotNull(Detect(duplicate, rightEvents), root);
        foreach (var control in controls)
            Assert.IsNotNull(control.Value, control.Key + ": " + root);
    }

    private static Cycle[] Run(ReplacementConstrainedSequenceTests.Api api, string root, out Event[] observations)
    {
        Directory.CreateDirectory(root);
        var events = new List<Event>();
        var cycles = new List<Cycle>();
        var samples = new List<object>();
        var createProxy = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)NativeLibrary.GetExport(api.Module, "MILCreateEventProxy");
        float* color = stackalloc float[4];
        for (int index = 0; index < 16; index++)
        {
            nint factory = 0, target = 0, bitmap = 0, proxy = 0;
            State state = default;
            var frames = new List<byte[]>();
            uint tr = uint.MaxValue, br = uint.MaxValue, fr = uint.MaxValue, pr = uint.MaxValue;
            void Log(string name, long value)
            {
                events.Add(new(index, name, value));
                Save(Path.Combine(root, "events.json"), events);
            }
            try
            {
                Check(api.Create(&factory, 0x200184C0)); Log("factory-created", 1);
                Check(api.CreateTarget(factory, 8, 8, 16, 96, 96, 1, &target)); Log("producer-created", 1);
                Check(api.GetBitmap(target, &bitmap)); Log("consumer-acquired", 1);
                Descriptor descriptor = new() { Dispose = &OnDispose, Raise = &OnRaise, Handle = (nuint)(&state) };
                Check(createProxy(&descriptor, &proxy)); Log("proxy-created", 1);
                for (int update = 0; update < 2; update++)
                {
                    color[0] = update == 0 ? 1 : 0; color[1] = 0; color[2] = update == 1 ? 1 : 0; color[3] = 1;
                    Check(((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)target)[4])(target, color, 0));
                    Log("producer-updated", update);
                    byte[] pixels = Read(bitmap);
                    frames.Add(pixels);
                    File.WriteAllBytes(Path.Combine(root, $"cycle-{index:D2}-frame-{update}.bgra"), pixels);
                    fixed (byte* bytes = pixels)
                        Check(((delegate* unmanaged[Stdcall]<nint, byte*, uint, int>)(*(void***)proxy)[3])(proxy, bytes, (uint)pixels.Length));
                    Log("frame-consumed-callbacks", state.Calls);
                }
                tr = Release(target); target = 0; Log("producer-released", tr);
                byte[] retained = Read(bitmap);
                frames.Add(retained);
                File.WriteAllBytes(Path.Combine(root, $"cycle-{index:D2}-frame-2.bgra"), retained);
                Log("consumer-read-after-producer-close", retained.Length);
                br = Release(bitmap); bitmap = 0; Log("consumer-released", br);
                fr = Release(factory); factory = 0; Log("factory-released", fr);
                pr = Release(proxy); proxy = 0; Log("proxy-released", pr);
                Log("descriptor-disposals", state.Disposals);
            }
            finally
            {
                if (proxy != 0) Release(proxy);
                if (bitmap != 0) Release(bitmap);
                if (target != 0) Release(target);
                if (factory != 0) Release(factory);
            }
            cycles.Add(new(index, tr, br, fr, pr, state.Calls, state.Disposals, state.Sum, frames.ToArray()));
            using Process process = Process.GetCurrentProcess();
            samples.Add(new { Cycle = index, Phase = index < 4 ? "warmup" : "measured", process.PrivateMemorySize64, process.WorkingSet64, process.HandleCount, Threads = process.Threads.Count, ManagedBytes = GC.GetTotalMemory(false) });
            Save(Path.Combine(root, "cycles.json"), cycles);
            Save(Path.Combine(root, "resources.json"), samples);
        }
        observations = events.ToArray();
        return cycles.ToArray();
    }

    private static string? Detect(Cycle[] cycles, Event[] events)
    {
        if (cycles.Length != 16) return "missing-cycle";
        if (events.Length != 16 * 14) return "event-count";
        if (events.Any(e => e.Cycle < 0 || e.Cycle >= 16)) return "event-cycle";
        for (int index = 0; index < cycles.Length; index++)
        {
            Cycle cycle = cycles[index];
            if (cycle.TargetRelease != 0 || cycle.BitmapRelease != 0 || cycle.FactoryRelease != 0 || cycle.ProxyRelease != 0) return "remaining-reference";
            if (cycle.Index != index) return "cycle-identity";
            Event[] trace = events.Where(e => e.Cycle == cycle.Index).ToArray();
            (string Name, long Value)[] required =
            [
                ("factory-created", 1), ("producer-created", 1), ("consumer-acquired", 1),
                ("proxy-created", 1), ("producer-updated", 0), ("frame-consumed-callbacks", 1),
                ("producer-updated", 1), ("frame-consumed-callbacks", 2),
                ("producer-released", cycle.TargetRelease), ("consumer-read-after-producer-close", 256),
                ("consumer-released", cycle.BitmapRelease), ("factory-released", cycle.FactoryRelease),
                ("proxy-released", cycle.ProxyRelease), ("descriptor-disposals", cycle.Disposals)
            ];
            if (trace.Length != required.Length) return "event-multiplicity";
            int[] positions = new int[required.Length];
            for (int i = 0; i < required.Length; i++)
            {
                int[] matches = Enumerable.Range(0, trace.Length).Where(p => trace[p].Name == required[i].Name && trace[p].Value == required[i].Value).ToArray();
                if (matches.Length != 1) return "event-multiplicity";
                positions[i] = matches[0];
            }
            // These are required ownership edges, not a cross-role wall-clock total order.
            (int Before, int After)[] edges = [(0, 1), (1, 2), (0, 3), (2, 4), (3, 5), (4, 5), (5, 6), (6, 7), (7, 8), (8, 9), (9, 10), (10, 11), (7, 12), (12, 13)];
            if (edges.Any(e => positions[e.Before] >= positions[e.After])) return "event-order";
            if (cycle.Calls != 2 || cycle.Disposals != 1 || cycle.Sum != 65280) return "callback-lifetime";
            if (cycle.Frames.Length != 3) return "missing-frame";
            for (int frame = 0; frame < 3; frame++)
            {
                byte[] pixels = cycle.Frames[frame];
                if (pixels.Length != 256) return "frame-size";
                for (int p = 0; p < pixels.Length; p += 4)
                    if (pixels[p] != (frame == 0 ? 0 : 255) || pixels[p + 1] != 0 || pixels[p + 2] != (frame == 0 ? 255 : 0) || pixels[p + 3] != 255) return "blank-stale-or-wrong-color";
            }
        }
        return null;
    }

    private static byte[] Read(nint bitmap)
    {
        byte[] pixels = new byte[256];
        fixed (byte* bytes = pixels)
            Check(((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 32, 256, bytes));
        return pixels;
    }
    private static uint Release(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    private static void Check(int hr) { if (hr != 0) throw new InvalidOperationException($"HRESULT=0x{hr:X8}"); }
    private static void Save<T>(string path, T value) => File.WriteAllText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void OnDispose(Descriptor* descriptor) => ((State*)descriptor->Handle)->Disposals++;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int OnRaise(Descriptor* descriptor, byte* bytes, uint length)
    {
        State* state = (State*)descriptor->Handle;
        state->Calls++;
        for (uint i = 0; i < length; i++) state->Sum += bytes[i];
        return 0;
    }
}
