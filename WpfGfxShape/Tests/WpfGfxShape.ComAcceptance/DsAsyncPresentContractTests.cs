#if DS_ASYNC_PRESENT_CONTRACT
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class DsAsyncPresentContractTests
{
    /// <summary>Exercises only legal asynchronous event transfer without a surface or content claim.</summary>
    [TestMethod]
    public void WhenDsOfficialAsyncPresentRepeatsThenEventsAndCleanupComplete() => Observe(true);

    /// <summary>Exercises the same event contract against the preserved candidate artifact.</summary>
    [TestMethod]
    public void WhenDsCandidateAsyncPresentRepeatsThenEventsAndCleanupComplete() => Observe(false);

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PresentPacket
    {
        internal uint Type, Handle;
        internal ulong Event;
    }

    private static void Observe(bool official)
    {
        string role = official ? "official" : "candidate";
        string root = Path.Combine(AcceptancePaths.Results, "ds-async-present");
        Directory.CreateDirectory(root);
        string output = Path.Combine(root, $"{role}.json");
        string requested = official ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(requested);
        using var process = Process.GetCurrentProcess();
        string actual = process.Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        var version = FileVersionInfo.GetVersionInfo(actual);
        string assembly = typeof(DsAsyncPresentContractTests).Assembly.Location;
        var identity = new
        {
            Role = role, Requested = requested, Actual = actual,
            Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual))),
            version.FileVersion, version.ProductVersion,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
            Runtime = RuntimeInformation.FrameworkDescription,
            WpfAssembly = typeof(System.Windows.Media.Visual).Assembly.Location,
            WpfSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(System.Windows.Media.Visual).Assembly.Location))),
            WpfVersion = FileVersionInfo.GetVersionInfo(typeof(System.Windows.Media.Visual).Assembly.Location).ProductVersion,
            TestAssembly = assembly, TestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))),
            Mvid = typeof(DsAsyncPresentContractTests).Module.ModuleVersionId,
            Surface = false, ContentConsumptionProven = false,
            NativeCloseCountDirectlyObserved = false
        };
        var steps = new List<object>();
        int owner = Environment.CurrentManagedThreadId;
        void Record(string stage, int? hr = null, object? detail = null)
        {
            steps.Add(new { Stage = stage, HResult = hr, Detail = detail, Utc = DateTimeOffset.UtcNow,
                Qpc = Stopwatch.GetTimestamp(), Thread = Environment.CurrentManagedThreadId, NativeThread = GetCurrentThreadId() });
            File.WriteAllText(output, JsonSerializer.Serialize(new { Identity = identity, Steps = steps }, new JsonSerializerOptions { WriteIndented = true }));
            ExitDiagnostics.Stage($"DS {role} {stage} hr={hr}");
            Assert.AreEqual(owner, Environment.CurrentManagedThreadId);
        }
        void Check(string stage, int hr)
        {
            Record(stage, hr);
            Assert.AreEqual(0, hr, stage);
        }
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroyChannel = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var release = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)Export("MilResource_ReleaseOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        nint connection = 0, channel = 0;
        bool initialized = false;
        using var completion = new EventWaitHandle(false, EventResetMode.ManualReset);
        try
        {
            Assert.AreEqual(16, sizeof(PresentPacket));
            Assert.AreEqual((nint)8, Marshal.OffsetOf<PresentPacket>(nameof(PresentPacket.Event)));
            Record("initialize before");
            Check("initialize after", initialize(0));
            initialized = true;
            Check("connect async", connect(0, &connection));
            Check("channel create", createChannel(connection, 0, &channel));
            uint image = 0;
            Check("image resource create", resource(channel, 97, &image));
            Check("create batch close", close(channel));
            Check("create batch commit", commit(channel));
            Check("create flush", flush(channel));
            for (int frame = 0; frame < 3; frame++)
            {
                Assert.IsTrue(completion.Reset());
                bool held = false;
                nint duplicate;
                try
                {
                    completion.SafeWaitHandle.DangerousAddRef(ref held);
                    Assert.IsTrue(DuplicateHandle(GetCurrentProcess(), completion.SafeWaitHandle.DangerousGetHandle(),
                        GetCurrentProcess(), out duplicate, 0, false, 2));
                }
                finally
                {
                    if (held) completion.SafeWaitHandle.DangerousRelease();
                }
                Record("duplicate created", detail: new { Frame = frame, Duplicate = (long)duplicate, CallerOwnsOriginal = true });
                PresentPacket packet = new() { Type = 0x0b, Handle = image, Event = (ulong)(nuint)duplicate };
                // Once recording is attempted, failure is not proof of non-transfer; never blindly close the duplicate.
                Record("send separate before", detail: frame);
                Check("send separate after", send(&packet, (uint)sizeof(PresentPacket), 1, channel));
                Check("present commit", commit(channel));
                bool signalled = completion.WaitOne(TimeSpan.FromSeconds(5));
                Record("event wait", detail: new { Frame = frame, Signalled = signalled });
                Assert.IsTrue(signalled);
                Check("present flush", flush(channel));
                Assert.IsTrue(completion.Reset());
                Assert.IsFalse(completion.WaitOne(0));
                Assert.IsTrue(completion.Set());
                Assert.IsTrue(completion.WaitOne(0));
                Record("original remains usable", detail: frame);
            }
            int deleted = 0;
            Check("resource release", release(channel, image, &deleted));
            Assert.AreEqual(1, deleted);
            Check("delete batch close", close(channel));
            Check("delete batch commit", commit(channel));
            Check("delete flush", flush(channel));
        }
        finally
        {
            try
            {
                if (channel != 0)
                {
                    Record("destroy channel before");
                    Check("destroy channel after", destroyChannel(channel));
                }
            }
            finally
            {
                try
                {
                    if (connection != 0) Check("disconnect", disconnect(connection));
                }
                finally
                {
                    if (initialized) Check("deinitialize", deinitialize());
                }
            }
        }
        completion.Dispose();
        Record("original disposed; cleanup complete");
        // Native modules intentionally remain process-resident.
    }

    [DllImport("kernel32.dll")]
    private static extern nint GetCurrentProcess();
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateHandle(nint sourceProcess, nint source, nint targetProcess,
        out nint target, uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint options);
}
#endif
