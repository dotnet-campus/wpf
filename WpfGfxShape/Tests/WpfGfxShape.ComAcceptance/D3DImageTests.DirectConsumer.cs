#if CONSUMER_DIRECT_DIAGNOSTIC
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    /// <summary>Checks three independent official consumer submissions without a native display prerequisite.</summary>
    [TestMethod]
    public void WhenDirectOfficialConsumerReceivesThreeColorsThenWindowDcMatches() => RunDirectConsumer(true);

    /// <summary>Checks the identical consumer sequence for the explicitly packaged current native module.</summary>
    [TestMethod]
    public void WhenDirectCurrentNativeConsumerReceivesThreeColorsThenWindowDcMatches() => RunDirectConsumer(false);

    /// <summary>Observes one official red submission twice at a fixed empirical interval, not a product latency guarantee.</summary>
    [TestMethod]
    public void WhenDirectOfficialConsumerReceivesRedThenFixedWindowIsObserved() => RunDirectConsumer(true, fixedWindow: true);

    /// <summary>Checks independent target clear before one image submission on the same official target.</summary>
    [TestMethod]
    public void WhenDirectOfficialTargetClearsGreenThenImageRedMatches() => RunDirectConsumer(true, clearControl: true);

    private static readonly System.Collections.Concurrent.ConcurrentQueue<(int Available, uint Version)> directCallbacks = new();

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static void DirectAvailable(int available, uint version) => directCallbacks.Enqueue((available, version));

    private static void RunDirectConsumer(bool official, bool fixedWindow = false, bool clearControl = false)
    {
        var resources = new System.Resources.ResourceManager("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(D3DImageTests).Assembly);
        ExitDiagnostics.Stage(resources.GetString(clearControl ? "DirectConsumerClearContract" : "DirectConsumerContract") ?? throw new InvalidOperationException());
        string trace = Path.Combine(NativeRun.PathsRoot, "direct-consumer.jsonl");
        Directory.CreateDirectory(NativeRun.PathsRoot);
        void Record(string stage, object? data = null)
        {
            using var stream = new FileStream(trace, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.WriteLine(JsonSerializer.Serialize(new { Stage = stage, Data = data, Utc = DateTimeOffset.UtcNow,
                Qpc = Stopwatch.GetTimestamp(), NativeThread = GetCurrentThreadId() }));
            writer.Flush();
            stream.Flush(true);
            ExitDiagnostics.Stage("direct " + stage);
        }
        void Check(string stage, int hr)
        {
            Record(stage, new { HResult = hr });
            Assert.AreEqual(0, hr, stage);
        }
        string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        string requested = official ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(requested);
        using var process = Process.GetCurrentProcess();
        string actual = process.Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        string actualSha = Sha(actual);
        Record("identity", new { Role = official ? "official" : "currentNative", Requested = requested, Actual = actual,
            Sha256 = actualSha, Version = FileVersionInfo.GetVersionInfo(actual).ProductVersion,
            Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
            TestSha256 = Sha(typeof(D3DImageTests).Assembly.Location), Mvid = typeof(D3DImageTests).Module.ModuleVersionId,
            WpfVersion = FileVersionInfo.GetVersionInfo(typeof(System.Windows.Media.Visual).Assembly.Location).ProductVersion,
            WpfSha256 = Sha(typeof(System.Windows.Media.Visual).Assembly.Location), Backend = "unknown", ProducerReadback = false });
        Assert.AreEqual(Path.GetFullPath(requested), Path.GetFullPath(actual), true);
        if (!official) Assert.AreEqual("20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D", actualSha);
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var releaseResource = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)Export("MilResource_ReleaseOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        var create = (delegate* unmanaged[Stdcall]<nint, double, double, uint, delegate* unmanaged[Stdcall]<int, uint, void>, int, nint*, uint*, uint*, int>)Export("InteropDeviceBitmap_Create");
        var dirty = (delegate* unmanaged[Stdcall]<int, int, int, int, nint, int>)Export("InteropDeviceBitmap_AddDirtyRect");
        var detach = (delegate* unmanaged[Stdcall]<nint, void>)Export("InteropDeviceBitmap_Detach");
        PresentationWindowThread ownerThread = windowThread ?? throw new InvalidOperationException();
        uint ownerId = ownerThread.Invoke(GetCurrentThreadId);
        Assert.AreNotEqual(0u, ownerId);
        nint window = 0, d3d = 0, device = 0, surface = 0, bitmap = 0, connection = 0, channel = 0;
        bool initialized = false;
        System.Runtime.ExceptionServices.ExceptionDispatchInfo? primaryFailure = null;
        var cleanupErrors = new List<Exception>();
        void Cleanup(string stage, Action action)
        {
            try { action(); }
            catch (Exception error)
            {
                // Cleanup must continue without replacing the original managed failure.
                cleanupErrors.Add(new InvalidOperationException(stage, error));
            }
        }
        (uint Pixel, long Before, long After) ObserveFixed(nint hwnd, uint targetHandle, int observation)
        {
            nint dc = GetDC(hwnd);
            Assert.AreNotEqual(0, dc);
            try
            {
                long before = Stopwatch.GetTimestamp();
                uint pixel = GetPixel(dc, 16, 8);
                long after = Stopwatch.GetTimestamp();
                Record("fixed raw pixel", new { Observation = observation, Actual = pixel, Expected = 255u,
                    BeforeQpc = before, AfterQpc = after, Frequency = Stopwatch.Frequency, Target = targetHandle,
                    Window = (long)hwnd, Owner = GetWindowThreadProcessId(hwnd, out _), Visible = IsWindowVisible(hwnd),
                    Iconic = IsIconic(hwnd), Foreground = (long)GetForegroundWindow() });
                WindowPoint point = new() { X = 16, Y = 8 };
                int translated = ClientToScreen(hwnd, ref point);
                Record("fixed window auxiliaries", new { Observation = observation, Translated = translated,
                    point.X, point.Y, Cover = translated != 0 ? (long)WindowFromPoint(point) : 0L });
                return (pixel, before, after);
            }
            finally { Cleanup("release fixed window DC", () => Assert.AreEqual(1, ReleaseDC(hwnd, dc))); }
        }
        var handles = new List<uint>();
        var completion = new EventWaitHandle(false, EventResetMode.ManualReset);
        Assert.IsTrue(directCallbacks.IsEmpty);
        try
        {
            Assert.AreEqual(16, sizeof(PresentPacket));
            Assert.AreSame(ownerThread, windowThread);
            window = ownerThread.Invoke(() => CreateOutputWindow("Direct independent consumer", 40, 40));
            Assert.AreNotEqual(0, window);
            Assert.AreEqual(ownerId, GetWindowThreadProcessId(window, out uint windowProcess));
            Assert.AreEqual((uint)Environment.ProcessId, windowProcess);
            EstablishObservableWindow(window);
            d3d = Direct3DCreate9(32);
            Assert.AreNotEqual(0, d3d);
            PresentParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            Check("producer device", ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)d3d)[16])(d3d, 0, 1, window, 0x26, &parameters, &device));
            Check("producer surface", ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 32, 16, 21, 0, 0, 1, &surface, 0));
            uint width = 0, height = 0;
            Check("interop create", create(surface, 96, 96, 7, &DirectAvailable, 0, &bitmap, &width, &height));
            Assert.AreEqual((32u, 16u), (width, height));
            Check("partition initialize", initialize(0)); initialized = true;
            Check("async connect", connect(0, &connection));
            Check("channel create", channelCreate(connection, 0, &channel));
            uint image = 0, data = 0, visual = 0, destination = 0;
            Check("image resource", resource(channel, 97, &image)); handles.Add(image);
            Check("drawing resource", resource(channel, 43, &data)); handles.Add(data);
            Check("visual resource", resource(channel, 39, &visual)); handles.Add(visual);
            Check("target resource", resource(channel, 46, &destination)); handles.Add(destination);
            ImagePacket update = new() { Type = 0x0a, Handle = image, Interop = (ulong)bitmap };
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
            Check("image update", send(&update, (uint)sizeof(ImagePacket), 0, channel));
            DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 32, Height = 16, Image = image };
            Check("draw image", send(&draw, (uint)sizeof(DrawPacket), 0, channel));
            uint* content = stackalloc uint[] { 0x22, visual, data };
            if (!clearControl) Check("visual content", send(content, 12, 0, channel));
            byte* target = stackalloc byte[92];
            new Span<byte>(target, 92).Clear();
            *(uint*)target = 0x31; *(uint*)(target + 4) = destination; *(ulong*)(target + 8) = (ulong)window;
            *(uint*)(target + 32) = 32; *(uint*)(target + 36) = 16; *(float*)(target + 52) = 1; *(uint*)(target + 56) = 12;
            nint dpiContext = GetWindowDpiAwarenessContext(window);
            int protocolContext = 0;
            foreach (int value in new[] { -1, -2, -3, -4 })
                if (AreDpiAwarenessContextsEqual(dpiContext, value) != 0) protocolContext = value;
            Assert.AreNotEqual(0, protocolContext);
            *(int*)(target + 72) = protocolContext;
            double scale = GetDpiForWindow(window) / 96.0;
            Assert.IsTrue(scale > 0);
            *(double*)(target + 76) = scale; *(double*)(target + 84) = scale;
            Assert.AreNotEqual(0, GetClientRect(window, out WindowRectangle bounds));
            Assert.AreEqual((32, 16), (bounds.Right - bounds.Left, bounds.Bottom - bounds.Top));
            Record("target contract", new { PacketSize = 92, PresentPacketSize = sizeof(PresentPacket), Flags = 12, DpiContext = protocolContext,
                Dpi = scale, Owner = GetWindowThreadProcessId(window, out _), Window = (long)window, Width = 32, Height = 16,
                Backend = "unknown", Readback = clearControl ? "GetPixel(window DC,16,8); independent green then image red; device warmup may change interop conditions" : fixedWindow ? "GetPixel(window DC,16,8); fixed two observations of one red submission" : "GetPixel(window DC,16,8); one observation per color" });
            Check("target create", send(target, 92, 0, channel));
            SendOutputWindowSettings(send, channel, destination, window);
            uint* tree = stackalloc uint[] { 0x35, destination, clearControl ? 0u : visual };
            if (clearControl)
            {
                byte* clearColor = stackalloc byte[24];
                new Span<byte>(clearColor, 24).Clear();
                *(uint*)clearColor = 0x36; *(uint*)(clearColor + 4) = destination;
                *(float*)(clearColor + 12) = 1; *(float*)(clearColor + 20) = 1;
                Record("independent clear contract", new { Target = destination, Channel = (long)channel,
                    Window = (long)window, Root = 0, R = 0, G = 1, B = 0, A = 1, Expected = 65280u,
                    MayWarmHardwareDevice = true, Backend = "unknown", ProductAcceptance = false });
                Check("green clear color", send(clearColor, 24, 0, channel));
            }
            Check("target root", send(tree, 12, 0, channel));
            Check("setup close", close(channel)); Check("setup commit", commit(channel)); Check("setup flush", flush(channel));
            uint* invalidate = stackalloc uint[] { 0x37, destination, 0, 0, 32, 16 };
            Check("initial invalidate", send(invalidate, 24, 0, channel));
            Check("invalidate close", close(channel)); Check("invalidate commit", commit(channel));
            if (clearControl)
            {
                Check("green consumer flush", flush(channel));
                Check("green dwm flush", DwmFlush());
                nint greenDc = GetDC(window);
                Assert.AreNotEqual(0, greenDc);
                uint greenPixel;
                try { greenPixel = GetPixel(greenDc, 16, 8); }
                finally { Cleanup("release green window DC", () => Assert.AreEqual(1, ReleaseDC(window, greenDc))); }
                Record("green consumer pixel", new { Target = destination, Window = (long)window,
                    Expected = 65280u, Actual = greenPixel, Accepted = greenPixel == 65280u });
                Assert.AreEqual(65280u, greenPixel);
                Assert.AreEqual(0, cleanupErrors.Count);
                Check("red visual content", send(content, 12, 0, channel));
                tree[2] = visual;
                Check("red target root", send(tree, 12, 0, channel));
                Check("red binding close", close(channel));
                Check("red binding commit", commit(channel));
                Check("red binding flush", flush(channel));
            }
            uint[] colors = [0xffff0000u, 0xff0000ffu, 0xff00ff00u];
            uint[] expected = [255u, 16711680u, 65280u];
            for (int frame = 0; frame < (fixedWindow || clearControl ? 1 : colors.Length); frame++)
            {
                Record("frame begin", new { Frame = frame, Color = colors[frame] });
                Check("producer color fill", ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, colors[frame]));
                CompleteNativePresent(device, window);
                Check("dirty", dirty(0, 0, 32, 16, bitmap));
                Assert.IsTrue(completion.Reset());
                nint duplicate;
                bool held = false;
                try
                {
                    completion.SafeWaitHandle.DangerousAddRef(ref held);
                    Assert.AreNotEqual(0, DuplicateHandle(-1, completion.SafeWaitHandle.DangerousGetHandle(), -1, &duplicate, 0, 0, 2));
                }
                finally { if (held) Cleanup("event temporary reference", () => completion.SafeWaitHandle.DangerousRelease()); }
                Record("duplicate transfer", new { Frame = frame, Duplicate = (long)duplicate,
                    Ownership = "send attempted: failure ownership unknown; no caller close; isolated process reclaims unresolved handle" });
                PresentPacket packet = new() { Type = 0x0b, Handle = image, Event = (ulong)(nuint)duplicate };
                int presentHr = send(&packet, (uint)sizeof(PresentPacket), 1, channel);
                Check("present separate send", presentHr);
                Check("present commit", commit(channel));
                bool signalled = completion.WaitOne(TimeSpan.FromSeconds(5));
                Record("event complete", new { Frame = frame, Signalled = signalled });
                Assert.IsTrue(signalled);
                Check("consumer flush", flush(channel));
                Check("dwm flush", DwmFlush());
                if (fixedWindow)
                {
                    var first = ObserveFixed(window, destination, 0);
                    long deadline = first.Before + 5 * Stopwatch.Frequency;
                    long schedulingAllowance = Stopwatch.Frequency / 4;
                    Record("fixed deadline", new { FirstQpc = first.Before, DeadlineQpc = deadline,
                        SchedulingAllowanceTicks = schedulingAllowance, WindowSeconds = 5, ProductLatencyGuarantee = false });
                    long remaining;
                    while ((remaining = deadline - Stopwatch.GetTimestamp()) > 0)
                        Thread.Sleep((int)Math.Clamp(Math.Ceiling(remaining * 1000.0 / Stopwatch.Frequency), 1, 50));
                    long secondFlushStart = Stopwatch.GetTimestamp();
                    Check("fixed second dwm flush", DwmFlush());
                    var second = ObserveFixed(window, destination, 1);
                    bool timingValid = first.After - first.Before <= schedulingAllowance && secondFlushStart >= deadline &&
                        second.After <= deadline + schedulingAllowance;
                    string classification = (first.Pixel == 255u, second.Pixel == 255u) switch
                    {
                        (true, true) => "both-match",
                        (false, true) => "first-mismatch-final-match",
                        (false, false) => "both-mismatch",
                        (true, false) => "first-match-final-mismatch"
                    };
                    Record("fixed observation result", new { Classification = classification,
                        First = first.Pixel, Final = second.Pixel, FirstQpc = first.Before, SecondQpc = second.Before,
                        FirstEndQpc = first.After, SecondEndQpc = second.After, SecondFlushStartQpc = secondFlushStart,
                        DeadlineQpc = deadline, Frequency = Stopwatch.Frequency,
                        ActualIntervalSeconds = (second.Before - first.Before) / (double)Stopwatch.Frequency,
                        TimingInvalid = !timingValid, ProductAcceptance = false, ColorReads = 2, Submissions = 1 });
                    Assert.AreEqual(255u, second.Pixel);
                    Assert.IsTrue(timingValid);
                    continue;
                }
                nint dc = GetDC(window);
                Assert.AreNotEqual(0, dc);
                uint pixel;
                try { pixel = GetPixel(dc, 16, 8); }
                finally { Cleanup("release window DC", () => Assert.AreEqual(1, ReleaseDC(window, dc))); }
                Record("consumer pixel", new { Frame = frame, Expected = expected[frame], Actual = pixel, Accepted = pixel == expected[frame] });
                Assert.AreEqual(expected[frame], pixel);
            }
            Record(clearControl ? "green and red consistent only with clear warmup; not product acceptance" : fixedWindow ? "fixed observation consistent; not product acceptance" : "three consumer frames accepted");
        }
        catch (Exception error)
        {
            // Preserve the original managed failure while every owned resource is cleaned up.
            primaryFailure = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error);
        }
        finally
        {
            Cleanup("event dispose", completion.Dispose);
            nint cleanupBitmap = bitmap, cleanupChannel = channel, cleanupConnection = connection;
            if (cleanupBitmap != 0) Cleanup("bitmap detach", () => { detach(cleanupBitmap); Record("detach after"); });
            if (channel != 0)
            {
                for (int i = handles.Count - 1; i >= 0; i--)
                {
                    uint handle = handles[i];
                    Cleanup("resource unregister", () =>
                    {
                        int deleted = 0;
                        int hr = releaseResource(cleanupChannel, handle, &deleted);
                        Record("resource deleted", new { Handle = handle, Deleted = deleted, HResult = hr });
                        Assert.AreEqual(0, hr);
                        Assert.AreEqual(1, deleted);
                    });
                }
                Cleanup("delete close", () => Check("delete close", close(cleanupChannel)));
                Cleanup("delete commit", () => Check("delete commit", commit(cleanupChannel)));
                Cleanup("delete flush", () => Check("delete flush", flush(cleanupChannel)));
                Cleanup("channel destroy", () => Check("channel destroy", destroy(cleanupChannel)));
            }
            if (cleanupConnection != 0) Cleanup("disconnect", () => Check("disconnect", disconnect(cleanupConnection)));
            if (initialized) Cleanup("partition deinitialize", () => Check("partition deinitialize", deinitialize()));
            void Drop(string name, nint value)
            {
                if (value == 0) return;
                Cleanup(name, () =>
                {
                    uint refs = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
                    Record("refs", new { Resource = name, Refs = refs });
                    Assert.AreEqual(0u, refs);
                });
            }
            Drop("bitmap", bitmap);
            Drop("surface", surface);
            Drop("device", device);
            Drop("factory", d3d);
            Cleanup("callbacks", () => Record("callbacks after release", directCallbacks.Select(value => new { value.Available, value.Version }).ToArray()));
            if (window != 0) Cleanup("window owner destroy", () =>
            {
                int result = ownerThread.Invoke(() =>
                {
                    uint current = GetCurrentThreadId();
                    uint actualOwner = GetWindowThreadProcessId(window, out _);
                    int destroyed = DestroyOwnedWindow(window);
                    Record("window owner destroy", new { Owner = ownerId, ActualOwner = actualOwner, Current = current, Result = destroyed });
                    Assert.AreEqual(ownerId, current);
                    Assert.AreEqual(ownerId, actualOwner);
                    return destroyed;
                });
                Assert.AreNotEqual(0, result);
            });
        }
        Cleanup("cleanup summary", () => Record("cleanup complete", new
        {
            Primary = primaryFailure?.SourceException.ToString(),
            Errors = cleanupErrors.Select(error => error.ToString()).ToArray()
        }));
        if (primaryFailure is not null)
        {
            primaryFailure.SourceException.Data["DirectConsumerCleanupErrors"] = cleanupErrors.ToArray();
            primaryFailure.Throw();
        }
        if (cleanupErrors.Count != 0) throw new AggregateException(cleanupErrors);
    }
}
#endif
