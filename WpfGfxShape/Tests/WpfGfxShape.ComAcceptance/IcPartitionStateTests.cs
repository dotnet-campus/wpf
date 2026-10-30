#if IC_PARTITION_STATE
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Interop;
using System.Windows.Threading;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class IcPartitionStateTests
{
    /// <summary>Compares two real sibling channels without treating a static snapshot as a state transition.</summary>
    [TestMethod]
    [Timeout(100000)]
    public async Task WhenIcPartitionGroupRunsThenSiblingBroadcastMatchesOfficialAsync()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "ic-roles");
        var results = new List<string>();
        foreach (string role in new[] { "official", "candidate" })
        {
            string output = Path.Combine(root, role);
            Directory.CreateDirectory(output);
            var start = new ProcessStartInfo(Environment.ProcessPath ?? throw new InvalidOperationException()) { UseShellExecute = false, WorkingDirectory = AppContext.BaseDirectory };
            foreach (string arg in new[] { "--filter", "FullyQualifiedName~WhenIcPartitionRoleRunsThenTwoListenersRecordSnapshots", "--report-trx", "--report-trx-filename", "role.trx", "--results-directory", output, "--no-progress" }) start.ArgumentList.Add(arg);
            start.Environment["IC_PARTITION_ROLE"] = role;
            start.Environment["IC_PARTITION_OUTPUT"] = output;
            using var process = Process.Start(start) ?? throw new InvalidOperationException();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(35));
            bool timedOut = false;
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { timedOut = true; process.Kill(true); await process.WaitForExitAsync(); }
            File.WriteAllText(Path.Combine(output, "exit.json"), JsonSerializer.Serialize(new { role, pid = process.Id, exitCode = process.ExitCode, timedOut, arguments = start.ArgumentList.ToArray() }, new JsonSerializerOptions { WriteIndented = true }));
            Assert.IsFalse(timedOut);
            Assert.AreEqual(0, process.ExitCode);
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(output, "business.json")));
            results.Add(document.RootElement.GetProperty("observations").GetRawText());
        }
        Assert.AreEqual(results[0], results[1]);
    }

    /// <summary>Records registration, duplicate operations, window removal, re-registration and listener destruction.</summary>
    [TestMethod]
    [Timeout(30000)]
    public unsafe void WhenIcPartitionRoleRunsThenTwoListenersRecordSnapshots()
    {
        string role = Environment.GetEnvironmentVariable("IC_PARTITION_ROLE") ?? throw new InvalidOperationException();
        string root = Environment.GetEnvironmentVariable("IC_PARTITION_OUTPUT") ?? throw new InvalidOperationException();
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
                using var first = new HwndSource(new HwndSourceParameters("IC listener A") { ParentWindow = new nint(-3), Width = 1, Height = 1 });
                using var second = new HwndSource(new HwndSourceParameters("IC listener B") { ParentWindow = new nint(-3), Width = 1, Height = 1 });
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
        nint module = NativeLibrary.Load(role == "official" ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll);
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
        void Drain(string stage)
        {
            dispatcher!.Invoke(() => Record(stage + ".window-barrier"), DispatcherPriority.ApplicationIdle);
            var messages = new List<object>();
            for (int channel = 0; channel < 2; channel++)
            {
                if (channels[channel] == 0) continue;
                byte* bytes = stackalloc byte[64];
                for (int i = 0; i < 16; i++)
                {
                    new Span<byte>(bytes, 64).Clear(); int retrieved = 0;
                    Check(stage + ".peek-" + channel, peek(channels[channel], bytes, 64, &retrieved));
                    if (retrieved == 0) break;
                    Assert.AreEqual(1, retrieved);
                    byte[] sample = new ReadOnlySpan<byte>(bytes, 64).ToArray();
                    messages.Add(new { channel, hex = Convert.ToHexString(sample), type = BinaryPrimitives.ReadUInt32LittleEndian(sample), reserved = BinaryPrimitives.ReadUInt32LittleEndian(sample.AsSpan(4)), oldState = BinaryPrimitives.ReadUInt32LittleEndian(sample.AsSpan(8)), newState = BinaryPrimitives.ReadUInt32LittleEndian(sample.AsSpan(12)) });
                    Assert.AreEqual(5u, BinaryPrimitives.ReadUInt32LittleEndian(sample));
                    if (i == 15) throw new InvalidOperationException();
                }
            }
            observations.Add(new { stage, messages }); Record(stage + ".drained", new { count = messages.Count });
        }
        void Submit(int channel, uint enable, string stage)
        {
            uint* command = stackalloc uint[] { 3, enable };
            Check(stage + ".send", send(command, 8, 0, channels[channel]));
            Check(stage + ".flush", flush(channels[channel])); Drain(stage);
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
            Submit(0, 1, "A-enable"); Submit(1, 1, "B-enable");
            Submit(1, 1, "B-repeat-enable"); Submit(0, 0, "A-disable"); Submit(0, 0, "A-repeat-disable");
            Submit(1, 0, "B-disable"); Submit(1, 1, "B-reenable-alone"); Submit(0, 1, "A-reenable-both");
            Check("B-window-unregister", notify(channels[1], 0, 0));
            Submit(0, 0, "A-disable-before-windowless-broadcast"); Submit(0, 1, "A-enable-B-windowless");
            Submit(1, 0, "B-disable-final"); Submit(1, 1, "B-enable-before-destroy");
            Check("B-window-clear-before-destroy", notify(channels[1], 0, 0));
            Check("B-destroy-listening", destroy(channels[1])); channels[1] = 0;
            Submit(0, 0, "A-disable-after-B-destroy"); Submit(0, 1, "A-enable-after-B-destroy"); Submit(0, 0, "A-disable-final");
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
                File.WriteAllText(Path.Combine(root, "business.json"), JsonSerializer.Serialize(new { role, pid = Environment.ProcessId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size, loaded = Identity(loaded), test = Identity(typeof(IcPartitionStateTests).Assembly.Location), mvid = typeof(IcPartitionStateTests).Module.ModuleVersionId, originalChannels, windows = windows.Select(value => (long)value).ToArray(), observations, events, cleaned, joined, windowError = windowError?.ToString(), targetCommandsSubmitted = 0, actualStateTransitionCovered = false, refs = "notification AddRef/Release inferred from official source; not directly measured" }, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        Assert.IsTrue(cleaned && joined);
    }
}
#endif
