#if HI_NATIVE_DELIVERY || HL_NATIVE_PRESENT_DIFFERENTIAL || HM_NATIVE_POSITIVE_ANCHOR || HN_DISPLAY_STATE || HP_GRAPHICS_TRACE
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [DllImport("user32.dll")] private static extern nint GetProcessWindowStation();
    [DllImport("user32.dll")] private static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")] private static extern int HiWindowLong(nint window, int index);
    [DllImport("dwmapi.dll")] private static extern int DwmIsCompositionEnabled(out int enabled);
    [DllImport("advapi32.dll", SetLastError = true)] private static extern int GetTokenInformation(nint token, int information, void* buffer, uint size, out uint needed);

    [StructLayout(LayoutKind.Sequential)]
    private struct HiStatistics
    {
        internal uint PresentCount, PresentRefreshCount, SyncRefreshCount;
        internal long SyncQpcTime, SyncGpuTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HiSurfaceDescription
    {
        internal uint Format, Type, Usage, Pool, MultiSampleType, MultiSampleQuality, Width, Height;
    }

    private static uint HiReadBackBufferPixel(nint device, Action<string, object> record)
    {
        nint backBuffer = 0, staging = 0;
        try
        {
            int acquired = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, nint*, int>)(*(void***)device)[18])(device, 0, 0, 0, &backBuffer);
            record("readback acquire", new { acquired, backBuffer = (long)backBuffer });
            Assert.AreEqual(0, acquired);
            HiSurfaceDescription description = default;
            int described = ((delegate* unmanaged[Stdcall]<nint, HiSurfaceDescription*, int>)(*(void***)backBuffer)[12])(backBuffer, &description);
            record("readback descriptor", new { described, description.Format, description.Type, description.Usage, description.Pool,
                description.MultiSampleType, description.MultiSampleQuality, description.Width, description.Height });
            Assert.AreEqual(0, described);
            Assert.AreEqual(22u, description.Format);
            Assert.IsTrue(description.Width > 16 && description.Height > 8);
            int created = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, nint*, nint, int>)(*(void***)device)[36])
                (device, description.Width, description.Height, description.Format, 2, &staging, 0);
            record("readback staging", new { created, description.Format, description.Width, description.Height, pool = 2 });
            Assert.AreEqual(0, created);
            int copied = ((delegate* unmanaged[Stdcall]<nint, nint, nint, int>)(*(void***)device)[32])(device, backBuffer, staging);
            record("readback copy", new { copied });
            Assert.AreEqual(0, copied);
            LockedSurface locked = default;
            int lockedResult = ((delegate* unmanaged[Stdcall]<nint, LockedSurface*, nint, uint, int>)(*(void***)staging)[13])(staging, &locked, 0, 0x10);
            record("readback lock", new { lockedResult, locked.Pitch, bits = (long)locked.Bits });
            Assert.AreEqual(0, lockedResult);
            uint pixel;
            int unlocked = int.MinValue;
            try
            {
                Assert.IsTrue(locked.Bits != 0 && locked.Pitch >= description.Width * sizeof(uint));
                pixel = *(uint*)((byte*)locked.Bits + 8 * locked.Pitch + 16 * sizeof(uint));
            }
            finally { unlocked = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)staging)[14])(staging); }
            record("readback unlock", new { unlocked, pixel, rgb = pixel & 0xffffff, format = description.Format,
                alphaApplicable = false });
            Assert.AreEqual(0, unlocked);
            return pixel;
        }
        finally
        {
            uint stagingRefs = staging == 0 ? 0 : ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)staging)[2])(staging);
            uint backBufferRefs = backBuffer == 0 ? 0 : ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)backBuffer)[2])(backBuffer);
            record("readback release", new { stagingRefs, backBufferRefs });
        }
    }

    private static string HiObjectName(nint handle)
    {
        char* text = stackalloc char[256];
        int result = GetUserObjectInformationW(handle, 2, text, 512, out uint needed);
        return result != 0 ? new string(text) : $"Unavailable:{Marshal.GetLastWin32Error()}:needed={needed}";
    }

    /// <summary>Distinguishes windowed blit and flip delivery without entering any candidate consumer.</summary>
    [TestMethod]
#if HP_GRAPHICS_TRACE
    public void WhenHrFixedFirstRedRunsThenOriginalContractIsRecorded()
#elif HN_DISPLAY_STATE
    public void WhenHnDisplayStateRunsThenSinglePositiveAnchorIsCorrelatedWithTopology()
#elif HM_NATIVE_POSITIVE_ANCHOR
    public void WhenHmNativePositiveAnchorRunsThenMatchedD3D9ComparisonIsGatedByFreshDelivery()
#elif HL_NATIVE_PRESENT_DIFFERENTIAL
    public void WhenHlNativePresentDifferentialRunsThenApiAndOwnerBoundariesAreRecorded()
#else
    public void WhenHiNativeDeliveryRunsThenBlitAndFlipOutputBoundariesAreRecorded()
#endif
    {
        Directory.CreateDirectory(AcceptancePaths.Results);
        string trace = Path.Combine(AcceptancePaths.Results, "hi-delivery.jsonl");
        void Record(string stage, object data) => File.AppendAllText(trace, JsonSerializer.Serialize(new
        { stage, qpc = Stopwatch.GetTimestamp(), utc = DateTimeOffset.UtcNow, pid = Environment.ProcessId, tid = GetCurrentThreadId(), data }) + Environment.NewLine);
        string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        using var process = Process.GetCurrentProcess();
        using var identity = WindowsIdentity.GetCurrent();
        byte* token = stackalloc byte[1024];
        int integrityResult = GetTokenInformation(identity.Token, 25, token, 1024, out uint integrityNeeded);
        string integrity = integrityResult != 0 ? new SecurityIdentifier(*(nint*)token).Value : $"Unavailable:{Marshal.GetLastWin32Error()}";
        int sessionQuery = WTSQuerySessionInformationW(0, (uint)process.SessionId, 16, out nint protocolBuffer, out uint protocolBytes);
        int protocol = -1;
        try { if (sessionQuery != 0 && protocolBytes >= 2) protocol = *(ushort*)protocolBuffer; }
        finally { if (protocolBuffer != 0) WTSFreeMemory(protocolBuffer); }
        string test = typeof(D3DImageTests).Assembly.Location;
        string wpf = typeof(System.Windows.Media.Visual).Assembly.Location;
        Record("identity", new { test, testSha = Sha(test), mvid = typeof(D3DImageTests).Module.ModuleVersionId,
            runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            wpf, wpfSha = Sha(wpf), wpfVersion = FileVersionInfo.GetVersionInfo(wpf).ProductVersion,
            session = process.SessionId, windowStation = HiObjectName(GetProcessWindowStation()),
            desktop = HiObjectName(GetThreadDesktop(GetCurrentThreadId())), integrityResult, integrityNeeded, integrity,
            remoteMetric = GetSystemMetrics(0x1000), sessionQuery, protocol, consumerEntered = false });
        bool allAccepted = true;
#if HN_DISPLAY_STATE || HM_NATIVE_POSITIVE_ANCHOR || HP_GRAPHICS_TRACE
#if HN_DISPLAY_STATE || HP_GRAPHICS_TRACE
        foreach (bool extended in new bool[] { true })
#else
        foreach (bool extended in new bool[] { true, false })
#endif
        {
            if (!allAccepted)
            {
                Record("comparison not executed", new { reason = "Positive anchor rejected; no parameter retry", extended, consumerEntered = false });
                break;
            }
            const uint effect = 1;
            const bool ownerSubmit = true;
            Record("control", new { extended, ownerSubmit, effect, positiveAnchor = extended });
#elif HL_NATIVE_PRESENT_DIFFERENTIAL
        foreach (uint control in new uint[] { 0, 1, 2 })
        {
            const uint effect = 1;
            bool extended = control == 1;
            bool ownerSubmit = control != 0;
            Record("control", new { control, extended, ownerSubmit, effect });
#else
        foreach (uint effect in new uint[] { 1, 5 })
        {
            bool extended = true, ownerSubmit = true;
#endif
            nint window = 0, factory = 0, device = 0, chain = 0, chainEx = 0;
            try
            {
                window = CreateOutputWindow("HI native delivery", 40, 40);
                Assert.AreNotEqual(0, window);
                bxWindows[window] = Path.Combine(AcceptancePaths.Results, $"hi-window-{effect}-{extended}-{ownerSubmit}.jsonl");
                EstablishObservableWindow(window);
                TraceDesktopWindow(window, "HI established");
                int compositionResult = DwmIsCompositionEnabled(out int composition);
                Record("window", new { effect, window = (long)window, style = HiWindowLong(window, -16), exStyle = HiWindowLong(window, -20),
                    compositionResult, composition, dpi = GetDpiForWindow(window), monitor = (long)MonitorFromWindow(window, 2) });
                if (extended) Assert.AreEqual(0, Direct3DCreate9Ex(32, &factory));
                else factory = Direct3DCreate9(32);
                Assert.AreNotEqual(0, factory);
                long luid = 0;
                int luidResult = int.MinValue;
                if (extended) luidResult = ((delegate* unmanaged[Stdcall]<nint, uint, long*, int>)(*(void***)factory)[21])(factory, 0, &luid);
                nint adapterMonitor = ((delegate* unmanaged[Stdcall]<nint, uint, nint>)(*(void***)factory)[15])(factory, 0);
                PresentParameters parameters = new() { Width = 32, Height = 16, Format = 22, Count = 2,
                    SwapEffect = effect, Window = window, Windowed = 1, Interval = 1 };
#if HL_NATIVE_PRESENT_DIFFERENTIAL
                parameters.Count = 1;
                parameters.Interval = 0;
#endif
                int creation = extended
                    ? ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint, nint*, int>)(*(void***)factory)[20])(factory, 0, 1, window, 0x26, &parameters, 0, &device)
                    : ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &device);
                Record("creation", new { effect, creation, luidResult, luid, adapterMonitor = (long)adapterMonitor, behavior = 0x26,
                    extended, ownerSubmit, parameters.Count, parameters.Format, parameters.Interval, overrideWindow = (long)window,
                    ownerThread = GetWindowThreadProcessId(window, out _), creationThread = GetCurrentThreadId(),
                    luidApplicable = extended, stateApplicable = extended });
#if HM_NATIVE_POSITIVE_ANCHOR || HN_DISPLAY_STATE || HP_GRAPHICS_TRACE
                Record("complete creation contract", new { sdk = 32, adapter = 0, deviceType = 1, behavior = 0x26,
                    parameters.Width, parameters.Height, parameters.Format, parameters.Count, parameters.SwapEffect,
                    parameters.Windowed, parameters.Interval, parameters.Multisample, parameters.Quality, parameters.AutoDepth,
                    parameters.DepthFormat, parameters.Flags, parameters.Refresh, presentSource = 0, presentDestination = 0, dirtyRegion = 0,
                    presentFlags = 0, fullscreenMode = 0, extended, ownerSubmit });
#endif
                Assert.AreEqual(0, creation);
#if HN_DISPLAY_STATE
                HnSnapshot(window, luid, "before anchor", Record);
#endif
#if HP_GRAPHICS_TRACE
                HrTopology(window, luid, Record);
#endif
                Assert.AreEqual(MonitorFromWindow(window, 2), adapterMonitor);
                LogNativeSwapChain(window, device);
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[14])(device, 0, &chain));
#if HM_NATIVE_POSITIVE_ANCHOR || HN_DISPLAY_STATE || HP_GRAPHICS_TRACE
                PresentParameters actual = default;
                int actualResult = ((delegate* unmanaged[Stdcall]<nint, PresentParameters*, int>)(*(void***)chain)[9])(chain, &actual);
                Record("actual presentation descriptor", new { actualResult, actual.Width, actual.Height, actual.Format, actual.Count,
                    actual.Multisample, actual.Quality, actual.SwapEffect, window = (long)actual.Window, actual.Windowed,
                    actual.AutoDepth, actual.DepthFormat, actual.Flags, actual.Refresh, actual.Interval });
                Assert.AreEqual(0, actualResult);
#endif
                Guid iid = new("91886caf-1c3d-4d2e-a0ab-3e4c7d8d3303");
                int chainQuery = Marshal.QueryInterface(chain, in iid, out chainEx);
                Record("swapchain interface", new { effect, chainQuery });
                nint native = device;
                foreach (uint color in new uint[] {
#if HN_DISPLAY_STATE || HP_GRAPHICS_TRACE
                    0xffff0000
#elif HL_NATIVE_PRESENT_DIFFERENTIAL || HM_NATIVE_POSITIVE_ANCHOR
                    0xffff0000, 0xff0000ff, 0xffff0000, 0xff123456
#else
                    0xffff0000, 0xff0000ff, 0xffff0000
#endif
                })
                {
                    long submitted = 0, completed = 0;
                    int present = int.MinValue, stateBefore = int.MinValue, stateAfter = int.MinValue;
                    uint backbuffer = 0, duplicated = 0, client = uint.MaxValue, screen = uint.MaxValue, front = uint.MaxValue;
                    int frontResult = int.MinValue, clearResult = int.MinValue;
                    bool presentCalled = false;
                    string? failure = null;
                    try
                    {
                        duplicated = ReadDuplicatedDesktopPixel(window, () =>
                        {
                            int Submit()
                            {
                                if (extended) stateBefore = ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)native)[128])(native, window);
                                clearResult = ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)native)[43])(native, 0, 0, 1, color, 1, 0);
                                Record("clear", new { effect, color, clearResult });
                                Assert.AreEqual(0, clearResult);
                                backbuffer = HiReadBackBufferPixel(native, (stage, data) => Record(stage, new { effect, color, data }));
                                submitted = Stopwatch.GetTimestamp();
                                presentCalled = true;
                                present = extended
                                    ? ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, uint, int>)(*(void***)native)[121])(native, 0, 0, window, 0, 0)
                                    : ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)native)[17])(native, 0, 0, window, 0);
                                Record("present returned", new { effect, color, presentCalled, present, submitted, returned = Stopwatch.GetTimestamp(), stateBefore });
                                Assert.IsTrue(present >= 0);
                                CompleteNativePresent(native, window);
                                completed = Stopwatch.GetTimestamp();
                                if (extended) stateAfter = ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)native)[128])(native, window);
                                Record("GPU completed", new { extended, ownerSubmit, color, completed, stateAfter, stateApplicable = extended });
                                return 0;
                            }
                            if (ownerSubmit) (windowThread ?? throw new InvalidOperationException()).Invoke(Submit);
                            else Submit();
                            WindowPoint point = new() { X = 16, Y = 8 };
                            Assert.AreNotEqual(0, ClientToScreen(window, ref point));
                            nint clientDc = GetDC(window), screenDc = GetDC(0);
                            try { client = GetPixel(clientDc, 16, 8); screen = GetPixel(screenDc, point.X, point.Y); }
                            finally { if (clientDc != 0) ReleaseDC(window, clientDc); if (screenDc != 0) ReleaseDC(0, screenDc); }
                            (frontResult, front) = ReadFreshGateFrontPixel(native, point);
                        });
                    }
                    catch (AssertFailedException error) { failure = error.ToString(); }
                    uint lastCount = 0;
                    HiStatistics stats = default;
                    int countResult = chainEx == 0 ? unchecked((int)0x80004002) : ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)chainEx)[10])(chainEx, &lastCount);
                    int statsResult = chainEx == 0 ? unchecked((int)0x80004002) : ((delegate* unmanaged[Stdcall]<nint, HiStatistics*, int>)(*(void***)chainEx)[11])(chainEx, &stats);
                    uint colorRef = ((color & 255) << 16) | (color & 0xff00) | ((color >> 16) & 255);
                    bool accepted = failure is null && (backbuffer & 0xffffff) == (color & 0xffffff) && client == colorRef && screen == colorRef &&
                        (duplicated & 0xffffff) == (color & 0xffffff) && frontResult == 0 && (front & 0xffffff) == (color & 0xffffff);
#if HM_NATIVE_POSITIVE_ANCHOR || HN_DISPLAY_STATE || HP_GRAPHICS_TRACE
                    accepted &= presentCalled && present == 0 && (!extended || (stateBefore == 0 && stateAfter == 0));
#endif
#if HN_DISPLAY_STATE
                    HnSnapshot(window, luid, "after anchor", Record);
#endif
                    allAccepted &= accepted;
                    Record("frame", new { extended, ownerSubmit, stateApplicable = extended, effect, color, submitted, completed, clearResult, backbuffer, presentCalled, present, stateBefore, stateAfter,
                        duplicated, client, screen, frontResult, front, countResult, lastCount, statsResult,
                        stats.PresentCount, stats.PresentRefreshCount, stats.SyncRefreshCount, stats.SyncQpcTime, stats.SyncGpuTime, accepted, failure });
                    TraceDesktopWindow(window, "HI sampled");
#if HM_NATIVE_POSITIVE_ANCHOR || HP_GRAPHICS_TRACE
                    if (!accepted)
                    {
                        Record("control stopped", new { extended, color, present, stateBefore, stateAfter, reason = "First rejected frame; remaining frames not executed" });
                        break;
                    }
#endif
                }
            }
            catch (AssertFailedException error) { allAccepted = false; Record("control rejected", new { effect, failure = error.ToString() }); }
            finally
            {
                Release(chainEx); Release(chain);
                uint deviceRefs = device == 0 ? 0 : ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)device)[2])(device);
                uint factoryRefs = factory == 0 ? 0 : ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)factory)[2])(factory);
                int destroyed = window == 0 ? 0 : DestroyWindow(window);
                bxWindows.TryRemove(window, out _);
                Record("cleanup", new { extended, ownerSubmit, effect, deviceRefs, factoryRefs, destroyed });
            }
        }
        foreach (ProcessModule module in process.Modules)
        {
            string name = module.ModuleName;
            if (name.Equals("d3d9.dll", StringComparison.OrdinalIgnoreCase) || name.Equals("dxgi.dll", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("d3d11.dll", StringComparison.OrdinalIgnoreCase) || name.Equals("dwmapi.dll", StringComparison.OrdinalIgnoreCase))
                Record("system module", new { name, path = module.FileName, sha = Sha(module.FileName), version = module.FileVersionInfo.ProductVersion });
        }
        Record("complete", new { allAccepted, consumerEntered = false });
        Assert.IsTrue(allAccepted, trace);
    }
}
#endif
