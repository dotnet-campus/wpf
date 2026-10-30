#if HW_NOTIFICATION_UNREGISTER
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Interop;
using System.Windows.Threading;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed class HwNotificationUnregisterTests
{
    private const int Notification = 0x8147;

    /// <summary>Runs the legal nonHW notification interleaving in separate official and candidate processes.</summary>
    [TestMethod]
    [Timeout(70000)]
    public void WhenHwUnregisterGroupRunsThenIndependentRolesRecordQueueBoundaries()
    {
        foreach (string role in new[] { "official", "candidate" })
        {
            string root = Path.Combine(AcceptancePaths.Results, "hw-roles", role);
            Directory.CreateDirectory(root);
            var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException())
            {
                UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory
            };
            foreach (string argument in new[] { "--filter", "FullyQualifiedName~WhenHwUnregisterRoleRunsThenInFlightWakeCanFinish", "--report-trx", "--report-trx-filename", "role.trx", "--results-directory", root, "--no-progress" })
                start.ArgumentList.Add(argument);
            start.Environment["HW_UNREGISTER_ROLE"] = role;
            start.Environment["HW_UNREGISTER_OUTPUT"] = root;
            using var process = Process.Start(start) ?? throw new InvalidOperationException();
            bool exited = process.WaitForExit(30000);
            if (!exited) { process.Kill(true); process.WaitForExit(); }
            File.WriteAllText(Path.Combine(root, "exit.json"), JsonSerializer.Serialize(new { role, exited, pid = process.Id, exitCode = process.ExitCode, arguments = start.ArgumentList.ToArray() }, new JsonSerializerOptions { WriteIndented = true }));
            Assert.IsTrue(exited);
            Assert.AreEqual(0, process.ExitCode);
        }
    }

    /// <summary>Records one real HWND wake, concurrent window unregistration, and retained nonHW back-channel messages.</summary>
    [TestMethod]
    [Timeout(25000)]
    public unsafe void WhenHwUnregisterRoleRunsThenInFlightWakeCanFinish()
    {
        string role = Environment.GetEnvironmentVariable("HW_UNREGISTER_ROLE") ?? throw new InvalidOperationException();
        string root = Environment.GetEnvironmentVariable("HW_UNREGISTER_OUTPUT") ?? throw new InvalidOperationException();
        string path = role == "official" ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        var events = new List<object>();
        var gate = new object();
        int order = 0, callbacks = 0;
        void Record(string stage, object? data = null)
        {
            lock (gate) events.Add(new { order = ++order, stage, thread = Environment.CurrentManagedThreadId, qpc = Stopwatch.GetTimestamp(), data });
        }
        void Check(string stage, int hr)
        {
            Record(stage, new { hr });
            Marshal.ThrowExceptionForHR(hr);
        }
        using var ready = new ManualResetEventSlim();
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        using var exited = new ManualResetEventSlim();
        Exception? windowError = null;
        Dispatcher? dispatcher = null;
        nint hwnd = 0;
        bool callbackReleased = false;
        var owner = new Thread(() =>
        {
            try
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                using var source = new HwndSource(new HwndSourceParameters("HW nonHW notification consumer")
                {
                    ParentWindow = new nint(-3), Width = 1, Height = 1, WindowStyle = 0
                });
                source.AddHook((nint window, int message, nint wParam, nint lParam, ref bool handled) =>
                {
                    if (message != Notification) return 0;
                    Interlocked.Increment(ref callbacks);
                    Record("dispatch-enter", new { window = (long)window, message, wParam = (long)wParam, lParam = (long)lParam });
                    entered.Set();
                    callbackReleased = release.Wait(TimeSpan.FromSeconds(6));
                    Record("callbackexit", new { callbackReleased });
                    handled = true;
                    exited.Set();
                    return 0;
                });
                hwnd = source.Handle;
                Record("window-ready", new { hwnd = (long)hwnd });
                ready.Set();
                Dispatcher.Run();
                Record("window-dispose");
            }
            catch (Exception error) when (error is InvalidOperationException or System.ComponentModel.Win32Exception or ExternalException)
            {
                windowError = error;
                ready.Set();
                Record("window-error", error.ToString());
            }
        });
        owner.SetApartmentState(ApartmentState.STA);
        owner.Start();
        nint module = NativeLibrary.Load(path);
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var create = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var notify = (delegate* unmanaged[Stdcall]<nint, nint, uint, int>)Export("MilChannel_SetNotificationWindow");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        var peek = (delegate* unmanaged[Stdcall]<nint, void*, nuint, int*, int>)Export("MilComposition_PeekNextMessage");
        nint connection = 0;
        nint* channelStorage = stackalloc nint[1];
        channelStorage[0] = 0;
        bool initialized = false, ownerJoined = false;
        var samples = new List<object>();
        object Identity(string file) => new { path = Path.GetFullPath(file), sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))), version = FileVersionInfo.GetVersionInfo(file).FileVersion };
        void Submit(uint enable, string stage)
        {
            uint* packet = stackalloc uint[] { 3, enable };
            Record(stage + ".submit", new { type = 3, enable });
            Check(stage + ".send", send(packet, 8, 0, channelStorage[0]));
            Check(stage + ".flush", flush(channelStorage[0]));
        }
        int Drain(string stage)
        {
            byte* bytes = stackalloc byte[64];
            int count = 0;
            for (int i = 0; i < 16; i++)
            {
                new Span<byte>(bytes, 64).Clear();
                int retrieved = 0;
                Check(stage + ".peek", peek(channelStorage[0], bytes, 64, &retrieved));
                if (retrieved == 0)
                {
                    Record(stage + ".drain-complete", new { count });
                    return count;
                }
                uint type = *(uint*)bytes;
                samples.Add(new { stage, type, hex = Convert.ToHexString(new ReadOnlySpan<byte>(bytes, 64)) });
                Assert.AreEqual(5u, type);
                count++;
            }
            throw new InvalidOperationException();
        }
        try
        {
            Assert.IsTrue(ready.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsNull(windowError);
            Assert.AreNotEqual(nint.Zero, hwnd);
            Check("initialize", initialize(0)); initialized = true;
            Check("connect", connect(0, &connection));
            Check("create", create(connection, 0, channelStorage));
            Record("register-enter", new { hwnd = (long)hwnd, Notification });
            Check("register-return", notify(channelStorage[0], hwnd, Notification));
            Submit(1, "first");
            Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(5)));
            Record("unregister-enter");
            Check("unregister-return", notify(channelStorage[0], 0, 0));
            release.Set();
            Assert.IsTrue(exited.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(callbackReleased);
            Assert.AreEqual(1, Drain("retained"));
            Submit(0, "partition-unregister");
            Assert.AreEqual(0, Drain("disabled"));
            Submit(1, "after-window-unregister");
            Assert.AreEqual(1, Drain("new-backchannel-without-window"));
            Submit(0, "final-partition-unregister");
            Assert.AreEqual(0, Drain("final"));
            var barrier = dispatcher!.InvokeAsync(() => Record("owner-barrier"));
            Assert.IsTrue(barrier.Task.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(1, Volatile.Read(ref callbacks));
        }
        finally
        {
            release.Set();
            if (channelStorage[0] != 0) Check("destroy", destroy(channelStorage[0]));
            if (connection != 0) Check("disconnect", disconnect(connection));
            if (initialized) Check("deinitialize", deinitialize());
            dispatcher?.BeginInvokeShutdown(DispatcherPriority.Send);
            ownerJoined = owner.Join(TimeSpan.FromSeconds(7));
            Record("owner-joined", new { ownerJoined, callbacks });
            string assembly = typeof(HwNotificationUnregisterTests).Assembly.Location;
            string actual = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
            File.WriteAllText(Path.Combine(root, "business.json"), JsonSerializer.Serialize(new
            {
                role, pid = Environment.ProcessId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                test = Identity(assembly), testMvid = typeof(HwNotificationUnregisterTests).Module.ModuleVersionId,
                loaded = Identity(actual), wpf = Identity(typeof(HwndSource).Assembly.Location),
                scope = "nonHW CompositionDeviceStateChange class5; real message-only HWND hook; no HW render target or SyncModeStatus",
                events, samples, callbacks, ownerJoined, windowError = windowError?.ToString(),
                refs = "no caller COM resources; channel destroyed, connection disconnected, partition deinitialized; DLL process-resident"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.IsTrue(ownerJoined);
    }
}
#endif
