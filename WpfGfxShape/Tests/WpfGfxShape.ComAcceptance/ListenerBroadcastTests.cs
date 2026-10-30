using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Interop;
using System.Windows.Threading;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class ListenerBroadcastTests
{
    /// <summary>Compares registration snapshots with independent official and official-repeat processes.</summary>
    [TestMethod]
    [Timeout(110000)]
    public async Task WhenListenerBroadcastRunsThenEveryRegisteredChannelMatchesOfficialAsync()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "listener-broadcast");
        var observations = new List<string>();
        foreach (string role in new[] { "official", "official-repeat", "candidate" })
        {
            string output = Path.Combine(root, role);
            Directory.CreateDirectory(output);
            var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException()) { UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory };
            foreach (string argument in new[] { "--filter", "FullyQualifiedName~WhenListenerBroadcastRoleRunsThenSnapshotsHaveIndependentContents", "--report-trx", "--report-trx-filename", "role.trx", "--results-directory", output, "--no-progress" }) start.ArgumentList.Add(argument);
            start.Environment["LISTENER_BROADCAST_ROLE"] = role;
            start.Environment["LISTENER_BROADCAST_OUTPUT"] = output;
            using var process = Process.Start(start) ?? throw new InvalidOperationException();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            bool timedOut = false;
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { timedOut = true; process.Kill(true); await process.WaitForExitAsync(); }
            File.WriteAllText(Path.Combine(output, "exit.json"), JsonSerializer.Serialize(new { role, pid = process.Id, exitCode = process.ExitCode, timedOut, arguments = start.ArgumentList.ToArray() }, new JsonSerializerOptions { WriteIndented = true }));
            Assert.IsFalse(timedOut);
            Assert.AreEqual(0, process.ExitCode);
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "business.json")));
            observations.Add(document.RootElement.GetProperty("observations").GetRawText());
        }
        Assert.AreEqual(observations[0], observations[1]);
        Assert.AreEqual(observations[0], observations[2]);
    }

    /// <summary>Exercises sibling registration, duplicates, windowless queues and destruction with full message contents.</summary>
    [TestMethod]
    [Timeout(25000)]
    public unsafe void WhenListenerBroadcastRoleRunsThenSnapshotsHaveIndependentContents()
    {
        string role = Environment.GetEnvironmentVariable("LISTENER_BROADCAST_ROLE") ?? "candidate";
        string root = Environment.GetEnvironmentVariable("LISTENER_BROADCAST_OUTPUT") ?? Path.Combine(AppContext.BaseDirectory, "Results", "listener-broadcast-direct");
        Directory.CreateDirectory(root);
        var events = new List<object>();
        var observations = new List<object>();
        var gate = new object();
        int order = 0;
        void Record(string stage, object? data = null) { lock (gate) events.Add(new { order = ++order, stage, thread = Environment.CurrentManagedThreadId, data }); }
        void Check(string stage, int hr) { Record(stage, new { hr }); Marshal.ThrowExceptionForHR(hr); }
        using var ready = new ManualResetEventSlim();
        Dispatcher? dispatcher = null;
        Exception? windowError = null;
        nint[] windows = new nint[2];
        var owner = new Thread(() =>
        {
            try
            {
                dispatcher = Dispatcher.CurrentDispatcher;
                using var first = new HwndSource(new HwndSourceParameters("Listener A") { ParentWindow = new nint(-3), Width = 1, Height = 1 });
                using var second = new HwndSource(new HwndSourceParameters("Listener B") { ParentWindow = new nint(-3), Width = 1, Height = 1 });
                windows[0] = first.Handle; windows[1] = second.Handle;
                nint Hook(nint window, int message, nint w, nint l, ref bool handled)
                {
                    if (message == 0x8151 || message == 0x8152) Record("window-wake", new { window = (long)window, message, w = (long)w, l = (long)l });
                    return 0;
                }
                first.AddHook(Hook); second.AddHook(Hook);
                Record("windows-ready", windows.Select(value => (long)value).ToArray());
                ready.Set(); Dispatcher.Run();
            }
            catch (Exception error) when (error is InvalidOperationException || error is System.ComponentModel.Win32Exception)
            { windowError = error; ready.Set(); }
        });
        owner.SetApartmentState(ApartmentState.STA); owner.Start();
        nint module = NativeLibrary.Load(role.StartsWith("official", StringComparison.Ordinal) ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll);
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
        nint* channels = stackalloc nint[2]; channels[0] = 0; channels[1] = 0;
        long[] originalChannels = new long[2];
        bool initialized = false, cleaned = false, joined = false;
        void Drain(string stage, int expectedA, int expectedB)
        {
            Dispatcher current = dispatcher ?? throw new InvalidOperationException();
            current.Invoke(() => Record(stage + ".window-barrier"), DispatcherPriority.ApplicationIdle);
            var messages = new List<object>();
            int[] counts = new int[2];
            byte* bytes = stackalloc byte[64];
            for (int channel = 0; channel < 2; channel++)
            {
                if (channels[channel] == 0) continue;
                while (true)
                {
                    new Span<byte>(bytes, 64).Clear(); int retrieved = 0;
                    Check(stage + ".peek-" + channel, peek(channels[channel], bytes, 64, &retrieved));
                    if (retrieved == 0) break;
                    Assert.AreEqual(1, retrieved);
                    byte[] sample = new ReadOnlySpan<byte>(bytes, 64).ToArray();
                    counts[channel]++;
                    messages.Add(new { channel, hex = Convert.ToHexString(sample) });
                }
            }
            observations.Add(new { stage, counts, messages });
            Record(stage + ".drained", new { counts });
            Assert.AreEqual(expectedA, counts[0], stage);
            Assert.AreEqual(expectedB, counts[1], stage);
            byte[] expected = new byte[64];
            BinaryPrimitives.WriteUInt32LittleEndian(expected, 5);
            BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(8), 1);
            BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(12), 1);
            foreach (object message in messages)
            {
                using var json = JsonDocument.Parse(JsonSerializer.Serialize(message));
                Assert.AreEqual(Convert.ToHexString(expected), json.RootElement.GetProperty("hex").GetString(), stage);
            }
        }
        void Submit(int channel, uint enable, string stage, int expectedA, int expectedB)
        {
            uint* command = stackalloc uint[] { 3, enable };
            Check(stage + ".send", send(command, 8, 0, channels[channel]));
            Check(stage + ".flush", flush(channels[channel])); Drain(stage, expectedA, expectedB);
        }
        object Identity(string path) => new { path, sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), version = FileVersionInfo.GetVersionInfo(path).FileVersion };
        try
        {
            Assert.IsTrue(ready.Wait(TimeSpan.FromSeconds(5))); Assert.IsNull(windowError);
            Check("initialize", initialize(0)); initialized = true;
            Check("connect", connect(0, &connection));
            Check("create-A", create(connection, 0, channels));
            Check("create-B-from-A", create(connection, channels[0], channels + 1));
            Assert.AreNotEqual(channels[0], channels[1]);
            for (int i = 0; i < 2; i++) { originalChannels[i] = (long)channels[i]; Check("window-register-" + i, notify(channels[i], windows[i], (uint)(0x8151 + i))); }
            Record("sibling-channels", new { connection = (long)connection, channels = originalChannels, sourceForB = originalChannels[0] });
            Submit(0, 1, "A-enable", 1, 0); Submit(1, 1, "B-enable", 1, 1);
            Submit(1, 1, "B-repeat-enable", 0, 0); Submit(0, 0, "A-disable", 0, 0); Submit(0, 0, "A-repeat-disable", 0, 0);
            Submit(1, 0, "B-disable", 0, 0); Submit(1, 1, "B-reenable-alone", 0, 1); Submit(0, 1, "A-reenable-both", 1, 1);
            Check("B-window-unregister", notify(channels[1], 0, 0));
            Submit(0, 0, "A-disable-before-windowless-broadcast", 0, 0); Submit(0, 1, "A-enable-B-windowless", 1, 1);
            Submit(1, 0, "B-disable-final", 0, 0); Submit(1, 1, "B-enable-before-destroy", 1, 1);
            Check("B-window-clear-before-destroy", notify(channels[1], 0, 0));
            Check("B-destroy-listening", destroy(channels[1])); channels[1] = 0;
            Submit(0, 0, "A-disable-after-B-destroy", 0, 0); Submit(0, 1, "A-enable-after-B-destroy", 1, 0); Submit(0, 0, "A-disable-final", 0, 0);
        }
        finally
        {
            try
            {
                for (int i = 1; i >= 0; i--) if (channels[i] != 0) { Check("window-clear-" + i, notify(channels[i], 0, 0)); Check("destroy-" + i, destroy(channels[i])); channels[i] = 0; }
                if (connection != 0) Check("disconnect", disconnect(connection));
                if (initialized) Check("deinitialize", deinitialize()); cleaned = true;
            }
            finally
            {
                dispatcher?.BeginInvokeShutdown(DispatcherPriority.Send); joined = owner.Join(5000);
                using var process = Process.GetCurrentProcess();
                string loaded = process.Modules.Cast<ProcessModule>().Single(item => item.BaseAddress == module).FileName;
                File.WriteAllText(Path.Combine(root, "business.json"), JsonSerializer.Serialize(new { role, pid = Environment.ProcessId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, loaded = Identity(loaded), test = Identity(typeof(ListenerBroadcastTests).Assembly.Location), mvid = typeof(ListenerBroadcastTests).Module.ModuleVersionId, originalChannels, windows = windows.Select(value => (long)value).ToArray(), observations, events, cleaned, joined, windowError = windowError?.ToString(), targetCommandsSubmitted = 0, actualStateTransitionCovered = false }, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        Assert.IsTrue(cleaned && joined);
    }
}
