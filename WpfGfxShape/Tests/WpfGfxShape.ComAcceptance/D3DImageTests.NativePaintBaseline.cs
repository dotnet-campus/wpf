#if NATIVE_PAINT_BASELINE
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct NpbParameters
    {
        public uint Width, Height, Format, Count, Multisample, Quality, SwapEffect;
        public nint Window;
        public int Windowed, AutoDepth;
        public uint DepthFormat, Flags, Refresh, Interval;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NpbCreation
    {
        public uint Adapter, Type;
        public nint Focus;
        public uint Behavior;
    }

    private struct NpbEvent
    {
        public long Sequence, Qpc, Window, WParam, LParam;
        public uint Tid, Message;
        public int Depth, Update, Validate;
    }

    private sealed class NpbState
    {
        internal nint Window, Device;
        internal bool Armed, Entered, Overflow, Reentered;
        internal int Depth, Count, ClearCount, PresentCount, ClearHr, PresentHr, Update, Validate;
        internal long ClearQpc, PresentQpc, ReturnedQpc;
        internal Exception? CallbackFailure;
        internal readonly NpbEvent[] Events = new NpbEvent[64];
    }

    // The owner retains this strong reference through DestroyWindow; no managed delegate is exported.
    private static NpbState? npbState;

    [DllImport("user32.dll", EntryPoint = "GetUpdateRect", SetLastError = true)]
    private static extern int NpbGetUpdateRect(nint window, out WindowRectangle rectangle, int erase);
    [DllImport("user32.dll", EntryPoint = "InvalidateRect", SetLastError = true)]
    private static extern int NpbInvalidateRect(nint window, nint rectangle, int erase);
    [DllImport("user32.dll", EntryPoint = "GetThreadDpiAwarenessContext")]
    private static extern nint NpbThreadDpi();

    private static bool NpbWindowMessage(nint window, uint message, nuint wParam, nint lParam)
    {
        NpbState? state = npbState;
        if (state is null || window != state.Window) return false;
        state.Depth++;
        if (state.Depth > 1) state.Reentered = true;
        int index = -1;
        try
        {
            if (message is 0x0f or 0x14 or 0x85 or 0x05 or 0x18 or 0x47 or 0x02e0 or 0x02 or 0x82)
            {
                if (state.Count == state.Events.Length) state.Overflow = true;
                else
                {
                    index = state.Count++;
                    state.Events[index] = new NpbEvent { Sequence = index + 1, Qpc = Stopwatch.GetTimestamp(),
                        Window = (long)window, Tid = GetCurrentThreadId(), Message = message,
                        WParam = unchecked((long)wParam), LParam = (long)lParam, Depth = state.Depth };
                }
            }
            if (message != 0x0f) return false;
            int update = NpbGetUpdateRect(window, out WindowRectangle rectangle, 0);
            if (index >= 0) state.Events[index].Update = update;
            if (state.Armed)
            {
                state.Armed = false;
                state.Entered = true;
                state.Update = update != 0 && rectangle.Right > rectangle.Left && rectangle.Bottom > rectangle.Top ? 1 : 0;
                if (state.Update != 0) NpbSubmit(state);
            }
            state.Validate = ValidateRect(window, 0);
            if (index >= 0) state.Events[index].Validate = state.Validate;
            return true;
        }
        catch (Exception error)
        {
            state.CallbackFailure = error;
            state.Armed = false;
            state.Validate = ValidateRect(window, 0);
            return true;
        }
        finally { state.Depth--; }
    }

    private static void NpbSubmit(NpbState state)
    {
        state.ClearQpc = Stopwatch.GetTimestamp();
        state.ClearCount++;
        state.ClearHr = ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)state.Device)[43])(
            state.Device, 0, 0, 1, 0xff123456, 1, 0);
        if (state.ClearHr != 0) return;
        state.PresentQpc = Stopwatch.GetTimestamp();
        state.PresentCount++;
        state.PresentHr = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)state.Device)[17])(
            state.Device, 0, 0, state.Window, 0);
    }

    private static void NpbHr(List<object> records, string operation, int hr)
    {
        records.Add(new { operation, hr, tid = GetCurrentThreadId(), qpc = Stopwatch.GetTimestamp() });
        Assert.AreEqual(0, hr);
    }

    private static uint NpbRelease(List<object> records, string operation, nint value)
    {
        uint refs = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
        records.Add(new { operation, pointer = (long)value, refs, tid = GetCurrentThreadId(), qpc = Stopwatch.GetTimestamp() });
        return refs;
    }

    private static void NpbComplete(nint device, nint window, List<object> records)
    {
        nint query = 0;
        Exception? primary = null;
        var cleanup = new List<string>();
        try
        {
            int hr = ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[118])(device, 8, &query);
            NpbHr(records, "CreateQuery.EVENT", hr);
            Assert.AreNotEqual(0, query);
            NpbHr(records, "Query.Issue.END", ((delegate* unmanaged[Stdcall]<nint, uint, int>)(*(void***)query)[6])(query, 1));
            long deadline = Environment.TickCount64 + 5000;
            int result, polls = 0;
            do
            {
                ProcessWindowMessages(window);
                result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, int>)(*(void***)query)[7])(query, 0, 0, 1);
                polls++;
                Assert.IsTrue(result >= 0);
                Assert.IsTrue(Environment.TickCount64 < deadline);
            } while (result == 1);
            records.Add(new { operation = "Query.GetData", hr = result, polls, flags = 1, deadline, qpc = Stopwatch.GetTimestamp() });
            ProcessWindowMessages(window);
            NpbHr(records, "DwmFlush.1", DwmFlush());
            ProcessWindowMessages(window);
            NpbHr(records, "DwmFlush.2", DwmFlush());
        }
        catch (Exception error) { primary = error; }
        finally
        {
            NpbCleanupRelease(records, "query.Release", query, cleanup, true);
            NpbCleanup(() => records.Add(new { operation = "query.cleanup", errors = cleanup }), cleanup);
        }
        if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
        Assert.AreEqual(0, cleanup.Count);
    }

    private static object NpbInvariant(nint window, nint device, List<object> records, uint dpi, nint dpiContext)
    {
        uint owner = GetWindowThreadProcessId(window, out uint pid);
        Assert.AreEqual(GetCurrentThreadId(), owner);
        Assert.AreEqual((uint)Environment.ProcessId, pid);
        Assert.AreEqual(dpi, GetDpiForWindow(window));
        Assert.AreEqual(dpiContext, NpbThreadDpi());
        Assert.AreNotEqual(0, GetClientRect(window, out WindowRectangle client));
        Assert.AreEqual((0, 0, 32, 16), (client.Left, client.Top, client.Right, client.Bottom));
        NpbCreation creation = default;
        NpbHr(records, "GetCreationParameters", ((delegate* unmanaged[Stdcall]<nint, NpbCreation*, int>)(*(void***)device)[9])(device, &creation));
        Assert.AreEqual((0u, 1u, window, 0x26u), (creation.Adapter, creation.Type, creation.Focus, creation.Behavior));
        nint chain = 0;
        NpbParameters actual = default;
        Exception? primary = null;
        var cleanup = new List<string>();
        try
        {
            NpbHr(records, "GetSwapChain.0", ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[14])(device, 0, &chain));
            Assert.AreNotEqual(0, chain);
            NpbHr(records, "GetPresentParameters", ((delegate* unmanaged[Stdcall]<nint, NpbParameters*, int>)(*(void***)chain)[9])(chain, &actual));
            Assert.AreEqual((32u, 16u, 22u, 1u, 1u, window, 1),
                (actual.Width, actual.Height, actual.Format, actual.Count, actual.SwapEffect, actual.Window, actual.Windowed));
            Assert.AreEqual((0u, 0u, 0, 0u, 0u, 0u, 0u),
                (actual.Multisample, actual.Quality, actual.AutoDepth, actual.DepthFormat, actual.Flags, actual.Refresh, actual.Interval));
        }
        catch (Exception error) { primary = error; }
        finally
        {
            NpbCleanupRelease(records, "chain.Release", chain, cleanup, false);
            NpbCleanup(() => records.Add(new { operation = "chain.cleanup", errors = cleanup }), cleanup);
        }
        if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
        Assert.AreEqual(0, cleanup.Count);
        WindowPoint point = new() { X = 16, Y = 8 };
        Assert.AreNotEqual(0, ClientToScreen(window, ref point));
        Assert.AreEqual(window, WindowFromPoint(point));
        return new { hwnd = NpbHex(window), owner, pid, dpi, dpiContext = NpbHex(dpiContext),
            client = new { client.Left, client.Top, client.Right, client.Bottom },
            creation = NpbCreationDto(creation), actual = NpbParametersDto(actual),
            sample = new { x = 16, y = 8, screenX = point.X, screenY = point.Y }, qpc = Stopwatch.GetTimestamp() };
    }

    private static string NpbHex(nint value) => $"0x{unchecked((ulong)(nuint)value):X16}";

    private static object NpbCreationDto(NpbCreation value) => new
    {
        value.Adapter, value.Type, Focus = NpbHex(value.Focus), value.Behavior
    };

    private static object NpbParametersDto(NpbParameters value) => new
    {
        value.Width, value.Height, value.Format, value.Count, value.Multisample, value.Quality,
        value.SwapEffect, Window = NpbHex(value.Window), value.Windowed, value.AutoDepth,
        value.DepthFormat, value.Flags, value.Refresh, value.Interval
    };

    private static readonly JsonSerializerOptions npbJsonOptions = new() { WriteIndented = true, IncludeFields = true };

    private static void NpbWrite(string name, object report)
    {
        string json = JsonSerializer.Serialize(report, npbJsonOptions);
        Directory.CreateDirectory(NativeRun.PathsRoot);
        File.WriteAllText(Path.Combine(NativeRun.PathsRoot, name), json);
    }

    private static void NpbCleanup(Action action, List<string> errors)
    {
        try { action(); }
        catch (Exception error) { errors.Add(error.ToString()); }
    }

    private static void NpbCleanupRelease(List<object> records, string operation, nint value, List<string> errors, bool requireZero)
    {
        if (value == 0) return;
        try
        {
            uint refs = NpbRelease(records, operation, value);
            if (requireZero && refs != 0) errors.Add(operation + ".nonzero");
        }
        catch (Exception error) { errors.Add(operation + ": " + error); }
    }

    private static Exception? NpbTryWrite(string name, Func<object> report)
    {
        try { NpbWrite(name, report()); return null; }
        catch (Exception error)
        {
            var failures = new List<Exception> { error };
            try { Console.Error.WriteLine(error); }
            catch (Exception stderrError) { failures.Add(stderrError); }
            try { NpbWrite("native-paint-baseline-recording-error.json", new { name, error = error.ToString() }); }
            catch (Exception terminalError)
            {
                failures.Add(terminalError);
                try { Console.Error.WriteLine(terminalError); }
                catch (Exception stderrError) { failures.Add(stderrError); }
            }
            return failures.Count == 1 ? error : new AggregateException(failures);
        }
    }

    private sealed record NpbRoleResult(string Role, uint Pixel, bool Target, bool BaselineKnown);

    private static NpbRoleResult NpbRole(string role, List<object> roles)
    {
        var records = new List<object>();
        var cleanup = new List<string>();
        var state = new NpbState();
        nint factory = 0, device = 0, window = 0;
        object? before = null, after = null;
        Exception? primary = null;
        NpbRoleResult? result = null;
        long createdQpc = 0, destroyedQpc = 0;
        try
        {
            window = CreateOutputWindow("Two input shader acceptance", 40, 40);
            createdQpc = Stopwatch.GetTimestamp();
            Assert.AreNotEqual(0, window);
            state.Window = window;
            npbState = state;
            EstablishObservableWindow(window);
            uint dpi = GetDpiForWindow(window);
            nint dpiContext = NpbThreadDpi();
            Assert.AreEqual(96u, dpi);
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual(0, factory);
            NpbParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            NpbHr(records, "CreateDevice", ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, NpbParameters*, nint*, int>)(*(void***)factory)[16])(
                factory, 0, 1, window, 0x26, &parameters, &device));
            Assert.AreNotEqual(0, device);
            state.Device = device;
            NpbHr(records, "TestCooperativeLevel", ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)device)[3])(device));
            before = NpbInvariant(window, device, records, dpi, dpiContext);
            Assert.AreEqual(0, NpbGetUpdateRect(window, out _, 0));
            if (role == "A") NpbSubmit(state);
            else
            {
                state.Armed = true;
                Assert.AreNotEqual(0, NpbInvalidateRect(window, 0, 0));
                Assert.AreNotEqual(0, UpdateWindow(window));
                Assert.IsTrue(state.Entered);
                Assert.AreEqual(1, state.Update);
                Assert.AreNotEqual(0, state.Validate);
            }
            state.ReturnedQpc = Stopwatch.GetTimestamp();
            Assert.IsNull(state.CallbackFailure);
            Assert.AreEqual((1, 1, 0, 0), (state.ClearCount, state.PresentCount, state.ClearHr, state.PresentHr));
            NpbComplete(device, window, records);
            after = NpbInvariant(window, device, records, dpi, dpiContext);
            long dcBefore = Stopwatch.GetTimestamp();
            nint dc = GetDC(window);
            uint pixel = uint.MaxValue;
            int release = 0;
            Exception? observationFailure = null;
            try { if (dc != 0) pixel = GetPixel(dc, 16, 8); }
            catch (Exception error) { observationFailure = error; }
            finally
            {
                if (dc != 0) NpbCleanup(() => release = ReleaseDC(window, dc), cleanup);
            }
            if (observationFailure is not null) ExceptionDispatchInfo.Capture(observationFailure).Throw();
            long dcAfter = Stopwatch.GetTimestamp();
            records.Add(new { operation = "single.client.observation", dc = (long)dc, pixel, release, dcBefore, dcAfter, tid = GetCurrentThreadId() });
            Assert.AreNotEqual(0, dc);
            Assert.AreNotEqual(uint.MaxValue, pixel);
            Assert.AreNotEqual(0, release);
            Assert.IsFalse(state.Overflow);
            Assert.IsFalse(state.Reentered);
            Assert.IsNull(state.CallbackFailure);
            bool target = Math.Abs((int)(pixel & 255) - 0x12) <= 1 && Math.Abs((int)((pixel >> 8) & 255) - 0x34) <= 1 &&
                Math.Abs((int)((pixel >> 16) & 255) - 0x56) <= 1;
            // The one-DC-per-role budget cannot certify a pre-submit non-target baseline.
            result = new(role, pixel, target, false);
        }
        catch (Exception error) { primary = error; }
        finally
        {
            state.Armed = false;
            NpbCleanupRelease(records, "device.Release", device, cleanup, true);
            NpbCleanupRelease(records, "factory.Release", factory, cleanup, true);
            if (window != 0) NpbCleanup(() =>
            {
                int destroyed = DestroyWindow(window);
                destroyedQpc = Stopwatch.GetTimestamp();
                records.Add(new { operation = "DestroyWindow", hwnd = NpbHex(window), destroyed, tid = GetCurrentThreadId(), qpc = destroyedQpc });
                if (destroyed == 0) cleanup.Add("DestroyWindow.zero");
            }, cleanup);
            npbState = null;
            GC.KeepAlive(state);
        }
        string deviceDto = NpbHex(device);
        Exception? recording = NpbTryWrite($"native-paint-baseline-{role}.partial.json", () =>
        {
            object partial = new { role, window = NpbHex(window), factory = NpbHex(factory), device = deviceDto, createdQpc, destroyedQpc,
                before, after, result, records, cleanup, primary = primary?.ToString(),
                callbackFailure = state.CallbackFailure?.ToString(), state.Entered, state.Update, state.Validate,
                state.ClearCount, state.PresentCount, state.ClearHr, state.PresentHr, state.ClearQpc, state.PresentQpc,
                state.ReturnedQpc, state.Overflow, state.Reentered, events = state.Events.Take(state.Count).ToArray() };
            roles.Add(partial);
            return partial;
        });
        if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
        if (recording is not null) ExceptionDispatchInfo.Capture(recording).Throw();
        Assert.AreEqual(0, cleanup.Count);
        return result ?? throw new InvalidOperationException();
    }

    /// <summary>Compares one direct and one real paint-scoped native submission without changing the product fixture.</summary>
    [TestMethod]
    public void WhenPaintScopedNativePresentIsComparedThenOriginalClientPixelIsRequired()
    {
        var roles = new List<object>();
        NpbRoleResult? a = null, b = null;
        Exception? primary = null;
        string classification = "Invalid.BasicApiOrParametersOrCleanup";
        string candidate = AcceptancePaths.AotDll;
        string candidateSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(candidate)));
        object Report() => new { classification, primary = primary?.ToString(), a, b, roles,
            expected = "00563412", thresholdPerChannel = 1, baselineEvidence = "NotObserved.SingleDcBudget.TargetMatchIsInsufficient",
            candidateFile = new { path = candidate, sha256 = candidateSha, productExportsInvoked = false },
            runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            pointerSize = IntPtr.Size, pid = Environment.ProcessId,
            testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(D3DImageTests).Assembly.Location))),
            mvid = typeof(D3DImageTests).Assembly.ManifestModule.ModuleVersionId,
            frequency = Stopwatch.Frequency, scanoutOrFrameIdentity = false };
        try
        {
            roles.Add(new { role = "Synthetic.NotExecuted", before = new {
                creation = NpbCreationDto(new NpbCreation { Focus = (nint)(-1) }),
                actual = NpbParametersDto(new NpbParameters { Window = (nint)(-1) }),
                client = new { Left = 0, Top = 0, Right = 32, Bottom = 16 } },
                after = new { creation = NpbCreationDto(default), actual = NpbParametersDto(default) },
                events = new[] { new NpbEvent { Window = -1, WParam = -1, LParam = -1 } },
                result = new NpbRoleResult("Synthetic", 0, false, false),
                records = new object[] { new { pointer = NpbHex((nint)(-1)), refs = 0u } },
                cleanup = Array.Empty<string>(), primary = "Synthetic.Exception.NotThrown" });
            NpbWrite("native-paint-baseline-serialization-selfcheck.json", Report());
            roles.Clear();
            Assert.AreEqual(4, IntPtr.Size);
            Assert.AreEqual(56, sizeof(NpbParameters));
            Assert.AreEqual(28, Marshal.OffsetOf<NpbParameters>(nameof(NpbParameters.Window)).ToInt32());
            Assert.AreEqual(32, Marshal.OffsetOf<NpbParameters>(nameof(NpbParameters.Windowed)).ToInt32());
            Assert.AreEqual(16, sizeof(NpbCreation));
            Assert.AreEqual(48, sizeof(OutputWindowClass));
            Assert.AreEqual("15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620", candidateSha);
            PresentationWindowThread owner = windowThread ?? throw new InvalidOperationException();
            a = owner.Invoke(() => NpbRole("A", roles));
            b = owner.Invoke(() => NpbRole("B", roles));
            classification = $"A{(a.Target ? "Correct" : "Wrong")}B{(b.Target ? "Correct" : "Wrong")}";
        }
        catch (Exception error) { primary = error; }
        Exception? recording = NpbTryWrite("native-paint-baseline.json", Report);
        if (primary is not null) ExceptionDispatchInfo.Capture(primary).Throw();
        if (recording is not null) ExceptionDispatchInfo.Capture(recording).Throw();
        ExitDiagnostics.Stage("NativePaintBaseline " + classification);
        Assert.IsNotNull(a);
        Assert.IsNotNull(b);
        Assert.IsTrue(a.Target && b.Target && a.BaselineKnown && b.BaselineKnown);
    }
}
#endif
