#if CG_DWM_DELIVERY_DIAGNOSTICS
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private sealed class CgWindowState
    {
        internal int Paint, Erase, Dropped;
        internal readonly List<object> Messages = new(128);
        internal void Record(uint message)
        {
            if (message == 15) Paint++;
            if (message == 20) Erase++;
            if (Messages.Count < 128) Messages.Add(new { message, qpc = System.Diagnostics.Stopwatch.GetTimestamp(), Paint, Erase });
            else Dropped++;
        }
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<nint, CgWindowState> cgWindows = new();
    [ThreadStatic] private static Action<int, long, long, uint>? cgAcquisition;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct CgTiming
    {
        internal uint Size, RefreshNumerator, RefreshDenominator;
        internal long RefreshPeriod;
        internal uint ComposeNumerator, ComposeDenominator;
        internal long VBlank;
        internal ulong Refresh;
        internal uint DxRefresh;
        internal long Compose;
        internal ulong Frame;
        internal uint DxPresent;
        internal ulong RefreshFrame, Submitted;
        internal uint DxSubmitted;
        internal ulong Confirmed;
        internal uint DxConfirmed;
        internal ulong RefreshConfirmed;
        internal uint DxRefreshConfirmed;
        internal ulong Late;
        internal uint Outstanding;
        internal ulong Displayed;
        internal long DisplayedQpc;
        internal ulong RefreshDisplayed, Complete;
        internal long CompleteQpc;
        internal ulong Pending;
        internal long PendingQpc;
        internal ulong FramesDisplayed, FramesComplete, FramesPending, Available, Dropped, Missed,
            NextDisplayed, NextPresented, RefreshesDisplayed, RefreshesPresented, RefreshStarted,
            PixelsReceived, PixelsDrawn, BuffersEmpty;
    }

    [DllImport("dwmapi.dll")] private static extern int DwmGetCompositionTimingInfo(nint window, ref CgTiming timing);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")] private static extern int CgGetWindowLong(nint window, int index);

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static nint CgWindowProcedure(nint window, uint message, nuint wParam, nint lParam)
    {
        if (cgWindows.TryGetValue(window, out CgWindowState? state))
        {
            if (message is 15 or 20 or 24 or 71) state.Record(message);
            if (message == 20) return 1;
            if (message == 15)
            {
                ValidateRect(window, 0);
                return 0;
            }
        }
        if (message == 20) return 1;
        if (message == 15) { ValidateRect(window, 0); return 0; }
        return DefWindowProcW(window, message, wParam, lParam);
    }

    private static object CgSnapshot(nint window, CgWindowState state, string stage)
    {
        return (windowThread ?? throw new InvalidOperationException()).Invoke<object>(() =>
        {
            GetWindowRect(window, out WindowRectangle rectangle);
            GetClientRect(window, out WindowRectangle client);
            WindowPoint point = new() { X = 16, Y = 8 };
            int converted = ClientToScreen(window, ref point);
            nint dc = GetDC(window), screen = GetDC(0);
            uint clientPixel, screenPixel;
            try { clientPixel = GetPixel(dc, 16, 8); screenPixel = GetPixel(screen, point.X, point.Y); }
            finally { ReleaseDC(window, dc); ReleaseDC(0, screen); }
            CgTiming timing = new() { Size = (uint)sizeof(CgTiming) };
            int result = DwmGetCompositionTimingInfo(0, ref timing);
            int cloakResult = DwmGetWindowAttribute(window, 14, out uint cloaked, sizeof(uint));
            return new { stage, qpc = System.Diagnostics.Stopwatch.GetTimestamp(), state.Paint, state.Erase,
                hwnd = (long)window, owner = GetWindowThreadProcessId(window, out _),
                rect = new[] { rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom },
                client = new[] { client.Left, client.Top, client.Right, client.Bottom }, converted,
                screenPoint = new[] { point.X, point.Y }, cover = (long)WindowFromPoint(point),
                visible = IsWindowVisible(window), iconic = IsIconic(window), cloakResult, cloaked,
                style = CgGetWindowLong(window, -16), exStyle = CgGetWindowLong(window, -20),
                clientPixel, screenPixel, dwmResult = result, timingSize = sizeof(CgTiming),
                vblank = timing.VBlank, compose = timing.Compose, frame = timing.Frame,
                submitted = timing.Submitted, confirmed = timing.Confirmed, displayed = timing.Displayed,
                displayedQpc = timing.DisplayedQpc, completeQpc = timing.CompleteQpc,
                framesDisplayed = timing.FramesDisplayed, dropped = timing.Dropped };
        });
    }

    /// <summary>Observes an isolated no-GDI window without changing the ordinary HWND fixture.</summary>
    [TestMethod]
    public void WhenIndependentPaintPolicyIsControlledThenDwmDeliveryBoundaryIsRecorded()
    {
        PresentationWindowThread ownerThread = windowThread ?? throw new InvalidOperationException();
        const bool gdi = false;
        var state = new CgWindowState();
        var records = new List<object>();
        nint window = 0, factory = 0, device = 0;
        bool passed = true;
        try
        {
            window = (windowThread ?? throw new InvalidOperationException()).Invoke(() =>
            {
                const string className = "WpfGfxShape.CG.IsolatedOutput";
                fixed (char* name = className)
                {
                    OutputWindowClass definition = new() { Size = (uint)sizeof(OutputWindowClass), Instance = GetModuleHandleW(null), Name = name, Procedure = &CgWindowProcedure };
                    ushort atom = RegisterClassExW(ref definition);
                    Assert.IsTrue(atom != 0 || Marshal.GetLastWin32Error() == 1410);
                    nint created = CreateWindowExW(8, className, "CG isolated paint control", 0x90000000, 40, 40, 32, 16, 0, 0, definition.Instance, 0);
                    Assert.AreNotEqual(0, created);
                    cgWindows[created] = state;
                    return created;
                }
            });
            EstablishObservableWindow(window);
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual(0, factory);
            PresentParameters parameters = new() { Width = 32, Height = 16, Format = 21, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            nint createdDevice = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &createdDevice));
            device = createdDevice;
            foreach (uint expected in new uint[] { 0xffff0000, 0xff0000ff, 0xffff0000 })
            {
                var acquisitions = new List<object>(32);
                int acquisitionDropped = 0;
                cgAcquisition = (result, lastPresent, lastMouse, accumulated) =>
                {
                    if (acquisitions.Count < 32) acquisitions.Add(new { result, lastPresent, lastMouse, accumulated,
                        pointerOnly = result == 0 && lastPresent == 0, qpc = System.Diagnostics.Stopwatch.GetTimestamp() });
                    else acquisitionDropped++;
                };
                uint backbuffer = 0;
                uint? duplicated = null;
                int present = int.MinValue;
                long submitted = 0, completed = 0;
                string? failure = null;
                object? before = null, after = null;
                try
                {
                    duplicated = ReadDuplicatedDesktopPixel(window, () =>
                    {
                        (windowThread ?? throw new InvalidOperationException()).Invoke(() =>
                        {
                            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)device)[43])(device, 0, 0, 1, expected, 1, 0));
                            backbuffer = ReadNativeBackBufferPixel(device);
                            Assert.AreEqual(expected, backbuffer);
                            before = CgSnapshot(window, state, "before-present");
                            submitted = System.Diagnostics.Stopwatch.GetTimestamp();
                            present = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)device)[17])(device, 0, 0, window, 0);
                            Assert.AreEqual(0, present);
                            CompleteNativePresent(device, window);
                            completed = System.Diagnostics.Stopwatch.GetTimestamp();
                            after = CgSnapshot(window, state, "after-completion");
                            return 0;
                        });
                    });
                }
                catch (AssertFailedException error) { failure = error.ToString(); }
                finally { cgAcquisition = null; }
                object settled = CgSnapshot(window, state, "after-acquisition");
                WindowPoint point = new() { X = 16, Y = 8 };
                ClientToScreen(window, ref point);
                var front = ReadFreshGateFrontPixel(device, point);
                bool accepted = failure is null && duplicated.HasValue && (duplicated.Value & 0xffffff) == (expected & 0xffffff);
                passed &= accepted;
                records.Add(new { expected, backbuffer, present, submitted, completed, duplicated,
                    acquiredPixel = duplicated.HasValue, blackFrame = duplicated.HasValue && (duplicated.Value & 0xffffff) == 0,
                    accepted, failure, before, after, settled, frontResult = front.Item1, frontPixel = front.Item2,
                    acquisitions, acquisitionDropped });
            }
        }
        finally
        {
            if (window != 0) ownerThread.Invoke(() => { cgWindows.TryRemove(window, out _); return DestroyWindow(window); });
            Release(device); Release(factory);
            Assembly assembly = typeof(D3DImageTests).Assembly;
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"cg-dwm-{gdi}.json"), JsonSerializer.Serialize(new
            {
                gdi, passed, backend = "system-d3d9-and-dxgi-not-candidate", runtime = RuntimeInformation.FrameworkDescription,
                architecture = RuntimeInformation.ProcessArchitecture.ToString(), testPath = assembly.Location,
                testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
                testMvid = assembly.ManifestModule.ModuleVersionId, messages = state.Messages, droppedMessages = state.Dropped, records
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.IsTrue(passed);
    }
}
#endif
