using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<nint, string> bxWindows = new();
    private static readonly object bxTraceLock = new();

    private static void TraceDesktopWindow(nint window, string stage, uint message = 0)
    {
        if (!bxWindows.TryGetValue(window, out string? path)) return;
        GetWindowRect(window, out WindowRectangle bounds);
        GetClientRect(window, out WindowRectangle client);
        WindowPoint point = new() { X = 16, Y = 8 };
        int converted = ClientToScreen(window, ref point);
        nint covering = WindowFromPoint(point);
        uint owner = GetWindowThreadProcessId(window, out uint pid);
        uint coverThread = GetWindowThreadProcessId(covering, out uint coverPid);
        char* name = stackalloc char[256];
        string DesktopName(nint desktop)
        {
            char* text = stackalloc char[256];
            return desktop != 0 && GetUserObjectInformationW(desktop, 2, text, 512, out _) != 0
                ? new string(text) : "Unavailable:" + Marshal.GetLastWin32Error();
        }
        nint input = OpenInputDesktop(0, 0, 1);
        string inputName;
        try { inputName = DesktopName(input); }
        finally { if (input != 0) CloseDesktop(input); }
        int count = GetClassNameW(covering, name, 256);
        string coverClass = new(name, 0, Math.Max(0, count));
        int session = System.Diagnostics.Process.GetCurrentProcess().SessionId;
        int query = WTSQuerySessionInformationW(0, (uint)session, 8, out nint buffer, out uint bytes);
        int state = -1;
        try { if (query != 0 && bytes >= sizeof(int)) state = *(int*)buffer; }
        finally { if (buffer != 0) WTSFreeMemory(buffer); }
        int cloakResult = DwmGetWindowAttribute(window, 14, out uint cloaked, sizeof(uint));
        string record = JsonSerializer.Serialize(new
        {
            stage, message, qpc = System.Diagnostics.Stopwatch.GetTimestamp(), pid = Environment.ProcessId,
            thread = GetCurrentThreadId(), hwnd = (long)window, owner, ownerPid = pid,
            session, sessionQuery = query, sessionState = state, inputDesktop = inputName,
            threadDesktop = DesktopName(GetThreadDesktop(GetCurrentThreadId())),
            ownerDesktop = DesktopName(GetThreadDesktop(owner)), foreground = (long)GetForegroundWindow(),
            visible = IsWindowVisible(window), iconic = IsIconic(window), cloakResult, cloaked,
            rect = new[] { bounds.Left, bounds.Top, bounds.Right, bounds.Bottom },
            client = new[] { client.Left, client.Top, client.Right, client.Bottom },
            converted, screen = new[] { point.X, point.Y }, cover = (long)covering, coverPid, coverThread, coverClass,
            monitor = (long)MonitorFromWindow(window, 2)
        });
        lock (bxTraceLock) File.AppendAllText(path, record + Environment.NewLine);
    }

    /// <summary>Runs public consumers only after both native swap effects pass the same-task content gate.</summary>
    [TestMethod]
    public void WhenFreshNativeGatePassesThenOfficialAndCandidatePublicTargetsAreCompared()
    {
        WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired(1);
        WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired(3);
        string stages = Path.Combine(AcceptancePaths.Results, "bj-public-stages.jsonl");
        void Record(string stage) => File.AppendAllText(stages, JsonSerializer.Serialize(new
        {
            stage, qpc = System.Diagnostics.Stopwatch.GetTimestamp(), thread = GetCurrentThreadId(),
            architecture = RuntimeInformation.ProcessArchitecture.ToString()
        }) + Environment.NewLine);
        Record("native-gates-passed-official-start");
        WhenOriginalPublicHwndTargetDrawsThenWindowContainsHardwarePixels();
        Record("official-passed-candidate-start");
        nint module = NativeLibrary.Load(AcceptancePaths.AotDll);
        LogHardwareModuleIdentity(module, AcceptancePaths.AotDll);
        RunPublicShaderFrames(40, 40, 21, false, 0, false);
        Record("candidate-passed");
    }

    /// <summary>Requires three new native desktop frames and records independent display observations even when acquisition times out.</summary>
    [TestMethod]
    [DataRow(1u)]
    [DataRow(3u)]
    public void WhenNativeOwnerThreadSubmitsRedBlueRedThenFreshDesktopContentIsRequired(uint swapEffect)
        => CheckFreshDesktopDelivery(swapEffect, false);

    [DllImport("gdi32.dll")]
    private static extern nint CreateSolidBrush(uint color);
    [DllImport("user32.dll")]
    private static extern int FillRect(nint dc, in WindowRectangle rectangle, nint brush);
    [DllImport("gdi32.dll")]
    private static extern int DeleteObject(nint value);
    [DllImport("gdi32.dll")]
    private static extern int GdiFlush();

#if BX_DESKTOP_DELIVERY_DIAGNOSTICS
    /// <summary>Uses same-window GDI delivery as a diagnostic control, never as a substitute for native fresh frames.</summary>
    [TestMethod]
    [DataRow(1u)]
    [DataRow(3u)]
    public void WhenSameWindowGdiControlPrecedesNativeFramesThenBothDeliveryBoundariesAreRecorded(uint swapEffect)
        => CheckFreshDesktopDelivery(swapEffect, true);
#endif

    private static void CheckFreshDesktopDelivery(uint swapEffect, bool gdiControl)
    {
        var observations = new List<object>();
        string evidence = Path.Combine(AcceptancePaths.Results, $"bj-fresh-{swapEffect}.json");
        nint window = CreateOutputWindow("BJ native fresh frame gate", 40, 40);
        if (gdiControl)
        {
            bxWindows[window] = Path.Combine(AcceptancePaths.Results, $"bx-window-{swapEffect}.jsonl");
            TraceDesktopWindow(window, "created");
        }
        nint factory = 0, device = 0;
        bool passed = true;
        try
        {
            EstablishObservableWindow(window);
            TraceDesktopWindow(window, "shown");
            if (gdiControl)
            {
                string? failure = null;
                uint pixel = 0;
                try
                {
                    pixel = ReadDuplicatedDesktopPixel(window, () =>
                    {
                        (windowThread ?? throw new InvalidOperationException()).Invoke(() =>
                        {
                            nint dc = GetDC(window), brush = CreateSolidBrush(0x0000ff00);
                            try
                            {
                                Assert.AreNotEqual(0, GetClientRect(window, out WindowRectangle rectangle));
                                Assert.AreNotEqual(0, FillRect(dc, in rectangle, brush));
                                Assert.AreNotEqual(0, GdiFlush());
                            }
                            finally { DeleteObject(brush); ReleaseDC(window, dc); }
                            Assert.AreEqual(0, DwmFlush());
                            TraceDesktopWindow(window, "gdi-flushed");
                            return 0;
                        });
                    });
                }
                catch (AssertFailedException error) { failure = error.ToString(); }
                observations.Add(new { control = "GDI-only-not-hardware", pixel, failure });
            }
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual(0, factory);
            PresentParameters parameters = new() { Width = 32, Height = 16, Format = 21, Count = 1, SwapEffect = swapEffect, Window = window, Windowed = 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &device));
            nint nativeDevice = device;
            uint owner = GetWindowThreadProcessId(window, out _);
            Assert.AreEqual(MonitorFromWindow(window, 2), ((delegate* unmanaged[Stdcall]<nint, uint, nint>)(*(void***)factory)[15])(factory, 0));
            foreach (uint expected in new uint[] { 0xffff0000, 0xff0000ff, 0xffff0000 })
            {
                long submitted = 0, completed = 0;
                uint submitThread = 0, backbuffer = 0, duplicated = 0;
                uint clientPixel = uint.MaxValue, screenPixel = uint.MaxValue, frontPixel = uint.MaxValue;
                int present = int.MinValue, frontResult = int.MinValue;
                string? failure = null;
                try
                {
                    duplicated = ReadDuplicatedDesktopPixel(window, () =>
                    {
                        (windowThread ?? throw new InvalidOperationException()).Invoke(() =>
                        {
                            submitThread = GetCurrentThreadId();
                            Console.WriteLine($"BJ owner-dispatch thread={submitThread} owner={owner} expected=0x{expected:X8}");
                            Assert.AreEqual(owner, submitThread);
                            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)nativeDevice)[43])(nativeDevice, 0, 0, 1, expected, 1, 0));
                            backbuffer = ReadNativeBackBufferPixel(nativeDevice);
                            Assert.AreEqual(expected, backbuffer);
                            TraceDesktopWindow(window, "before-present");
                            submitted = System.Diagnostics.Stopwatch.GetTimestamp();
                            present = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)nativeDevice)[17])(nativeDevice, 0, 0, window, 0);
                            Assert.AreEqual(0, present);
                            CompleteNativePresent(nativeDevice, window);
                            completed = System.Diagnostics.Stopwatch.GetTimestamp();
                            TraceDesktopWindow(window, "after-completion");
                            return 0;
                        });
                        DiagnoseNativePresent(window, nativeDevice, false);
                        WindowPoint point = new() { X = 16, Y = 8 };
                        Assert.AreNotEqual(0, ClientToScreen(window, ref point));
                        nint clientDc = GetDC(window), screenDc = GetDC(0);
                        try
                        {
                            clientPixel = GetPixel(clientDc, 16, 8);
                            screenPixel = GetPixel(screenDc, point.X, point.Y);
                        }
                        finally
                        {
                            if (clientDc != 0) ReleaseDC(window, clientDc);
                            if (screenDc != 0) ReleaseDC(0, screenDc);
                        }
                        (frontResult, frontPixel) = ReadFreshGateFrontPixel(nativeDevice, point);
                        Console.WriteLine($"BJ independent expected=0x{expected:X8} present=0x{present:X8} submitted={submitted} completed={completed} client=0x{clientPixel:X8} screen=0x{screenPixel:X8} frontResult=0x{frontResult:X8} front=0x{frontPixel:X8}");
                    });
                }
                catch (AssertFailedException error)
                {
                    failure = error.ToString();
                    Console.WriteLine($"BJ frame rejected expected=0x{expected:X8}: {failure}");
                }
                TraceDesktopWindow(window, "capture-ended");
                uint colorRef = ((expected & 255) << 16) | (expected & 0xff00) | ((expected >> 16) & 255);
                bool accepted = failure is null && (duplicated & 0xffffff) == (expected & 0xffffff)
                    && clientPixel == colorRef && screenPixel == colorRef && frontResult == 0
                    && (frontPixel & 0xffffff) == (expected & 0xffffff);
                passed &= accepted;
                observations.Add(new { expected, submitted, completed, submitThread, owner, backbuffer, present, duplicated, clientPixel, screenPixel, frontResult, frontPixel, accepted, failure });
            }
        }
        finally
        {
            Assembly assembly = typeof(D3DImageTests).Assembly;
            File.WriteAllText(evidence, JsonSerializer.Serialize(new
            {
                swapEffect, passed, runtime = RuntimeInformation.FrameworkDescription,
                architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size,
                testPath = assembly.Location, testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
                testMvid = assembly.ManifestModule.ModuleVersionId, observations
            }, new JsonSerializerOptions { WriteIndented = true }));
            Release(device); Release(factory);
            TraceDesktopWindow(window, "before-destroy");
            if (window != 0) DestroyWindow(window);
            TraceDesktopWindow(window, "after-destroy");
            bxWindows.TryRemove(window, out _);
        }
        Assert.IsTrue(passed, evidence);
    }

    private static (int Result, uint Pixel) ReadFreshGateFrontPixel(nint device, WindowPoint point)
    {
        nint surface = 0;
        try
        {
            NativeDisplayMode mode = default;
            int result = ((delegate* unmanaged[Stdcall]<nint, uint, NativeDisplayMode*, int>)(*(void***)device)[8])(device, 0, &mode);
            if (result != 0) return (result, uint.MaxValue);
            result = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, nint*, nint, int>)(*(void***)device)[36])(device, mode.Width, mode.Height, 21, 2, &surface, 0);
            if (result != 0) return (result, uint.MaxValue);
            result = ((delegate* unmanaged[Stdcall]<nint, uint, nint, int>)(*(void***)device)[33])(device, 0, surface);
            if (result != 0) return (result, uint.MaxValue);
            Assert.IsTrue(point.X >= 0 && point.Y >= 0 && point.X < mode.Width && point.Y < mode.Height);
            LockedSurface locked = default;
            result = ((delegate* unmanaged[Stdcall]<nint, LockedSurface*, nint, uint, int>)(*(void***)surface)[13])(surface, &locked, 0, 0x10);
            if (result != 0) return (result, uint.MaxValue);
            try { return (0, *(uint*)((byte*)locked.Bits + point.Y * locked.Pitch + point.X * sizeof(uint))); }
            finally { Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)surface)[14])(surface)); }
        }
        finally { Release(surface); }
    }
}
