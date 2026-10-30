#if HH_NATIVE_PRECONDITION
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [DllImport("user32.dll", EntryPoint = "GetWindowDpiAwarenessContext")]
    private static extern nint HhWindowContext(nint window);

    [DllImport("user32.dll", EntryPoint = "GetThreadDpiAwarenessContext")]
    private static extern nint HhThreadContext();

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int HhWindowLong(nint window, int index);

    /// <summary>Compares the native prerequisites without entering any composition consumer.</summary>
    [TestMethod]
    public void WhenHhNativePrerequisitesRunThenBothOriginalContentContractsAreRequired()
    {
        Directory.CreateDirectory(AcceptancePaths.Results);
        string trace = Path.Combine(AcceptancePaths.Results, "hh-native.jsonl");
        void Record(string stage, object? data = null) => File.AppendAllText(trace,
            JsonSerializer.Serialize(new { stage, data, qpc = Stopwatch.GetTimestamp(), pid = Environment.ProcessId,
                tid = GetCurrentThreadId(), threadContext = (long)HhThreadContext() }) + Environment.NewLine);
        string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        nint module = NativeLibrary.Load(AcceptancePaths.OriginalDll);
        using var process = Process.GetCurrentProcess();
        string actual = process.Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        string wpf = typeof(System.Windows.Media.Visual).Assembly.Location;
        Record("identity", new { actual, module = (long)module, officialSha = Sha(actual),
            officialVersion = FileVersionInfo.GetVersionInfo(actual).ProductVersion,
            testPath = typeof(D3DImageTests).Assembly.Location, testSha = Sha(typeof(D3DImageTests).Assembly.Location),
            mvid = typeof(D3DImageTests).Module.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(), wpf, wpfSha = Sha(wpf),
            wpfVersion = FileVersionInfo.GetVersionInfo(wpf).ProductVersion, consumerEntered = false });
        string? controlFailure = null, oldFailure = null;
        Record("HB control before");
        try { CheckFreshDesktopDelivery(1, false); }
        catch (AssertFailedException error) { controlFailure = error.ToString(); }
        Record("HB control released", new { failure = controlFailure });
        nint window = 0, factory = 0, device = 0;
        try
        {
            window = CreateOutputWindow("Two input shader acceptance", 40, 40);
            Assert.AreNotEqual(0, window);
            bxWindows[window] = Path.Combine(AcceptancePaths.Results, "hh-old-window.jsonl");
            EstablishObservableWindow(window);
            TraceDesktopWindow(window, "HH old established");
            Record("old window", new { window = (long)window, style = HhWindowLong(window, -16),
                exStyle = HhWindowLong(window, -20), dpi = GetDpiForWindow(window), context = (long)HhWindowContext(window),
                ownerTid = GetWindowThreadProcessId(window, out uint ownerPid), ownerPid });
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual(0, factory);
            PresentParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &device));
            Record("old submit before", new { device = (long)device, requestedFormat = 0, flags = 0, swapEffect = 1, width = 32, height = 16 });
            PresentNativeControl(window, device, false);
            Record("old completion");
            DiagnoseNativePresent(window, device, false);
            TraceDesktopWindow(window, "HH old sampled");
            WindowPoint point = new() { X = 16, Y = 8 };
            int converted = ClientToScreen(window, ref point);
            nint clientDc = GetDC(window), screenDc = GetDC(0);
            try
            {
                Record("old pixels", new { expectedColorRef = 0x00563412u, converted, x = point.X, y = point.Y,
                    client = GetPixel(clientDc, 16, 8), screen = GetPixel(screenDc, point.X, point.Y),
                    front = ReadFreshGateFrontPixel(device, point),
                    backbuffer = "not sampled: DISCARD contents undefined after Present",
                    duplicationFresh = "not acquired: preserve old submission and sampling sequence" });
            }
            finally { if (clientDc != 0) ReleaseDC(window, clientDc); if (screenDc != 0) ReleaseDC(0, screenDc); }
            AssertCombinedPixel(window, 0x00563412);
            Record("old accepted");
        }
        catch (AssertFailedException error) { oldFailure = error.ToString(); Record("old rejected", new { failure = oldFailure }); }
        finally
        {
            uint deviceRefs = device == 0 ? 0 : ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)device)[2])(device);
            uint factoryRefs = factory == 0 ? 0 : ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)factory)[2])(factory);
            int destroyed = window == 0 ? 0 : DestroyWindow(window);
            bxWindows.TryRemove(window, out _);
            Record("old cleanup complete", new { deviceRefs, factoryRefs, destroyed, consumerEntered = false });
        }
        Record("comparison complete", new { controlFailure, oldFailure, consumerEntered = false });
        Assert.IsNull(controlFailure, trace);
        Assert.IsNull(oldFailure, trace);
    }
}
#endif
