#if KS_THREAD_SCOPE
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private const string KsNativeSha = "20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D";

    private static void KsRecord(string stage, object data)
    {
        Directory.CreateDirectory(AcceptancePaths.Results);
        string line = JsonSerializer.Serialize(new { stage, data, utc = DateTimeOffset.UtcNow,
            tid = GetCurrentThreadId(), managedThread = Environment.CurrentManagedThreadId });
        File.AppendAllText(Path.Combine(AcceptancePaths.Results, "ks-thread-scope.jsonl"), line + Environment.NewLine);
        Console.WriteLine(line);
        ExitDiagnostics.Stage("KS " + stage);
    }

    private static string KsSha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static string KsSession()
    {
        using Process process = Process.GetCurrentProcess();
        Assert.AreNotEqual(0, WTSQuerySessionInformationW(0, (uint)process.SessionId, 8, out nint buffer, out uint bytes));
        int state;
        try { Assert.IsTrue(bytes >= sizeof(int)); state = *(int*)buffer; }
        finally { if (buffer != 0) WTSFreeMemory(buffer); }
        Assert.AreEqual(0, state);
        nint desktop = OpenInputDesktop(0, 0, 1);
        Assert.AreNotEqual(0, desktop);
        string name;
        try
        {
            char* text = stackalloc char[256];
            Assert.AreNotEqual(0, GetUserObjectInformationW(desktop, 2, text, 512, out _));
            name = new string(text);
        }
        finally { Assert.AreNotEqual(0, CloseDesktop(desktop)); }
        return $"{process.SessionId}/{state}/{name}";
    }

    private sealed record KsPixels(uint Client, uint Screen, int X, int Y);

    private static KsPixels KsSample(nint window)
    {
        WindowPoint point = new() { X = 16, Y = 8 };
        Assert.AreNotEqual(0, ClientToScreen(window, ref point));
        Assert.AreEqual(window, WindowFromPoint(point));
        nint client = GetDC(window), screen = GetDC(0);
        Assert.AreNotEqual(0, client);
        Assert.AreNotEqual(0, screen);
        try
        {
            uint a = GetPixel(client, 16, 8), b = GetPixel(screen, point.X, point.Y);
            Assert.AreNotEqual(uint.MaxValue, a);
            Assert.AreNotEqual(uint.MaxValue, b);
            return new(a, b, point.X, point.Y);
        }
        finally
        {
            Assert.AreNotEqual(0, ReleaseDC(window, client));
            Assert.AreNotEqual(0, ReleaseDC(0, screen));
        }
    }

    // Keep the original completion order/count; instrumentation must not add a second completion.
    private static void KsComplete(nint device, nint window)
    {
        nint query = 0;
        try
        {
            int create = ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[118])(device, 8, &query);
            KsRecord("query create", new { hr = create, created = query != 0 });
            Assert.AreEqual(0, create);
            int issue = ((delegate* unmanaged[Stdcall]<nint, uint, int>)(*(void***)query)[6])(query, 1);
            KsRecord("query issue", new { hr = issue });
            Assert.AreEqual(0, issue);
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
            KsRecord("query complete", new { hr = result, polls, flushFlag = 1 });
            ProcessWindowMessages(window);
            int first = DwmFlush();
            KsRecord("DwmFlush 1", new { hr = first });
            Assert.AreEqual(0, first);
            ProcessWindowMessages(window);
            int second = DwmFlush();
            KsRecord("DwmFlush 2", new { hr = second });
            Assert.AreEqual(0, second);
        }
        finally
        {
            if (query != 0)
                KsRecord("query release", new { refs = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)query)[2])(query) });
        }
    }

    private static bool KsRole(string role, string session)
    {
        nint window = 0, factory = 0, device = 0;
        bool clean = true;
        try
        {
            Assert.AreEqual(session, KsSession());
            window = CreateOutputWindow("Two input shader acceptance", 40, 40);
            Assert.AreNotEqual(0, window);
            EstablishObservableWindow(window);
            uint owner = GetWindowThreadProcessId(window, out uint pid);
            KsRecord(role + " window", new { window = (long)window, owner, pid, dpi = GetDpiForWindow(window),
                adapter = 0, type = 1, behavior = "0x26", width = 32, height = 16, format = 0,
                count = 1, swapEffect = 1, windowed = 1, clear = "FF123456", clientX = 16, clientY = 8 });
            if (role == "B") Assert.AreEqual(owner, GetCurrentThreadId());
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual(0, factory);
            PresentParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            int create = ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &device);
            KsRecord(role + " device", new { hr = create, created = device != 0 });
            Assert.AreEqual(0, create);
            KsPixels before = KsSample(window);
            KsRecord(role + " baseline", before);
            Assert.AreEqual(session, KsSession());
            int clear = ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)device)[43])(device, 0, 0, 1, 0xff123456, 1, 0);
            KsRecord(role + " Clear", new { hr = clear });
            Assert.AreEqual(0, clear);
            int present = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)device)[17])(device, 0, 0, window, 0);
            KsRecord(role + " Present", new { hr = present });
            Assert.AreEqual(0, present);
            KsComplete(device, window);
            DiagnoseNativePresent(window, device, false);
            KsPixels after = KsSample(window);
            Assert.AreEqual(session, KsSession());
            bool baselineValid = before.Client != 0x00563412 && before.Screen != 0x00563412;
            bool consumer = baselineValid && after.Client == 0x00563412 && after.Screen == 0x00563412;
            KsRecord(role + " observation", new { before, after, baselineValid, consumer,
                expected = "00563412", session, productAcceptance = false });
            return consumer;
        }
        finally
        {
            if (device != 0)
            {
                uint refs = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)device)[2])(device);
                KsRecord(role + " device release", new { refs });
                clean &= refs == 0;
            }
            else KsRecord(role + " device not created", new { released = false });
            if (factory != 0)
            {
                uint refs = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)factory)[2])(factory);
                KsRecord(role + " factory release", new { refs });
                clean &= refs == 0;
            }
            else KsRecord(role + " factory not created", new { released = false });
            if (window != 0)
            {
                int destroyed = DestroyWindow(window);
                KsRecord(role + " DestroyWindow", new { result = destroyed });
                clean &= destroyed != 0;
            }
            KsRecord(role + " cleanup", new { clean });
            Assert.IsTrue(clean);
        }
    }

    /// <summary>Observes the original native contract once per lifecycle thread scope, in fixed A then B order.</summary>
    [TestMethod]
    public void WhenKsLifecycleThreadScopeIsComparedThenOwnerConsumerMustDeliver()
    {
        string test = typeof(D3DImageTests).Assembly.Location;
        string candidate = AcceptancePaths.AotDll;
        Assert.AreEqual(Architecture.X86, RuntimeInformation.ProcessArchitecture);
        Assert.AreEqual(".NET 10.0.13", RuntimeInformation.FrameworkDescription);
        Assert.AreEqual(KsNativeSha, KsSha(candidate));
        KsRecord("identity", new { test, sha = KsSha(test), mvid = typeof(D3DImageTests).Module.ModuleVersionId,
            runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            candidate, candidatePackageSha = KsSha(candidate), order = "A(test) then B(owner)", productAcceptance = false });
        string session = KsSession();
        bool a = false, b = false;
        try
        {
            a = KsRole("A", session);
            Assert.AreEqual(session, KsSession());
            b = (windowThread ?? throw new InvalidOperationException()).Invoke(() => KsRole("B", session));
            Assert.AreEqual(session, KsSession());
            using Process process = Process.GetCurrentProcess();
            var modules = process.Modules.Cast<ProcessModule>().Where(m =>
                string.Equals(m.ModuleName, "wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.ModuleName, "d3d9.dll", StringComparison.OrdinalIgnoreCase)).Select(m =>
                new { path = m.FileName, sha = KsSha(m.FileName) }).ToArray();
            KsRecord("actual modules", new { modules, candidateStatus = modules.Any(m =>
                string.Equals(Path.GetFullPath(m.path), Path.GetFullPath(candidate), StringComparison.OrdinalIgnoreCase)) ? "loaded" : "not loaded" });
            KsRecord("verdict", new { a, b, hypothesis = !a && b ? "supported weakly" : a && !b ? "contradicted" : "inconclusive",
                productAcceptance = false, confounding = "fixed order and independently recreated windows" });
        }
        finally
        {
            windowThread?.Dispose();
            windowThread = null;
            KsRecord("owner joined", new { joined = true });
        }
        Assert.IsTrue(b, "KS owner consumer failed; observations are not product acceptance.");
    }
}
#endif
