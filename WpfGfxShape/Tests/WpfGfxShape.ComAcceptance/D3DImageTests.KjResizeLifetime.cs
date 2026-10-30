#if !KJ_RESIZE_LIFETIME && !KK_RESIZE_LIFETIME && !KL_RESIZE_LIFETIME
#define KN_RESIZE_LIFETIME
#endif
#if KJ_RESIZE_LIFETIME || KK_RESIZE_LIFETIME || KL_RESIZE_LIFETIME || KN_RESIZE_LIFETIME
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private static readonly ConcurrentQueue<(int Available, uint Version)> KjNotifications = new();

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void KjAvailable(int available, uint version) => KjNotifications.Enqueue((available, version));

    /// <summary>Observes actual target replacement and caller-owned producer lifetime without desktop capture.</summary>
    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    [DoNotParallelize]
#if KN_RESIZE_LIFETIME
    public void WhenKnTargetResizesInPlaceThenProducerLifetimeSurvives(bool resize)
#elif KL_RESIZE_LIFETIME
    public void WhenKlLegalCandidateTargetSettingsAreSubmittedThenProducerLifetimeIsObservable(bool resize)
#elif KK_RESIZE_LIFETIME
    public void WhenKkOfficialTargetSettingsAreSubmittedThenProducerLifetimeIsObservable(bool resize)
#else
    public void WhenKjTargetSettingsAreSubmittedThenProducerLifetimeIsObservable(bool resize)
#endif
    {
#if KN_RESIZE_LIFETIME
        const string role = "kn-inplace-lifetime";
        string nativePath = AcceptancePaths.AotDll;
#elif KL_RESIZE_LIFETIME
        const string role = "kl-candidate-lifetime";
        string nativePath = AcceptancePaths.AotDll;
#elif KK_RESIZE_LIFETIME
        const string role = "kk-official-lifetime";
        string nativePath = AcceptancePaths.OriginalDll;
#else
        const string role = "kj-lifetime";
        string nativePath = AcceptancePaths.AotDll;
#endif
        string root = Path.Combine(NativeRun.PathsRoot, role, resize ? "resize" : "baseline");
        Directory.CreateDirectory(root);
        var events = new List<object>();
        void Record(string stage, object? detail = null)
        {
            events.Add(new { Stage = stage, Detail = detail, Utc = DateTimeOffset.UtcNow });
            File.WriteAllText(Path.Combine(root, "lifetime.json"), JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true }));
            ExitDiagnostics.Stage("KJ " + stage);
        }
        void Check(string stage, int result)
        {
            Record(stage, new { HResult = result });
            Assert.AreEqual(0, result, stage);
        }
        nint module = NativeLibrary.Load(nativePath);
        using var process = Process.GetCurrentProcess();
        string actual = process.Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        object Identity(string path) => new { Path = path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), Version = FileVersionInfo.GetVersionInfo(path).ProductVersion };
        File.WriteAllText(Path.Combine(root, "identity.json"), JsonSerializer.Serialize(new
        {
            ActualNative = Identity(actual), Test = Identity(typeof(D3DImageTests).Assembly.Location),
            TestMvid = typeof(D3DImageTests).Module.ModuleVersionId,
            Wpf = Identity(typeof(System.Windows.Media.Visual).Assembly.Location),
            Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            Resize = resize, ConsumerPixelsObserved = false, DesktopCaptureExecuted = false,
            Contract = role + ": public callback, surface references, cooperative level, software snapshot status; no shader acceptance"
        }, new JsonSerializerOptions { WriteIndented = true }));
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
#if KK_RESIZE_LIFETIME || KL_RESIZE_LIFETIME || KN_RESIZE_LIFETIME
        var compose = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_SameThreadPresent");
#endif
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        var create = (delegate* unmanaged[Stdcall]<nint, double, double, uint, delegate* unmanaged[Stdcall]<int, uint, void>, int, nint*, uint*, uint*, int>)Export("InteropDeviceBitmap_Create");
        var snapshot = (delegate* unmanaged[Stdcall]<nint, nint*, int>)Export("InteropDeviceBitmap_GetAsSoftwareBitmap");
        var detach = (delegate* unmanaged[Stdcall]<nint, void>)Export("InteropDeviceBitmap_Detach");
        nint connection = 0, channel = 0, window = 0, factory = 0, device = 0, surface = 0, bitmap = 0;
        bool initialized = false;
        uint target = 0;
        KjNotifications.Clear();
        uint Ref(nint pointer, int slot) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)pointer)[slot])(pointer);
        uint References(nint pointer)
        {
            uint added = Ref(pointer, 1), remaining = Ref(pointer, 2);
            Assert.AreEqual(added - 1, remaining);
            return remaining;
        }
        void Send(byte[] packet)
        {
            fixed (byte* bytes = packet) Check("send " + BitConverter.ToUInt32(packet), send(bytes, (uint)packet.Length, 0, channel));
            Check("close", close(channel)); Check("commit", commit(channel)); Check("flush", flush(channel));
#if KK_RESIZE_LIFETIME || KL_RESIZE_LIFETIME || KN_RESIZE_LIFETIME
            Check("same thread compose (not D3DImage Present)", compose(connection));
#endif
        }
        void Settings(uint width, uint height)
        {
            byte[] packet = new byte[72];
            MemoryMarshal.Write(packet.AsSpan(), 0x33u); MemoryMarshal.Write(packet.AsSpan(4), target);
            MemoryMarshal.Write(packet.AsSpan(16), width); MemoryMarshal.Write(packet.AsSpan(20), height);
            MemoryMarshal.Write(packet.AsSpan(32), 1f); MemoryMarshal.Write(packet.AsSpan(44), 1);
            Send(packet);
        }
        int SnapshotStatus(string stage)
        {
            nint copy = 0;
            try
            {
                int result = snapshot(bitmap, &copy);
                Record(stage, new { HResult = result, HasBitmap = copy != 0 });
                Assert.AreEqual(result >= 0, copy != 0);
                return result;
            }
            finally { if (copy != 0) Record("snapshot release", Ref(copy, 2)); }
        }
        try
        {
            Check("initialize", initialize(0)); initialized = true;
            window = CreateOutputWindow("KJ resource lifetime", 40, 40); Assert.AreNotEqual(0, window);
#if KK_RESIZE_LIFETIME || KL_RESIZE_LIFETIME || KN_RESIZE_LIFETIME
            nint createdConnection = 0;
            Check("connect synchronous", connect(1, &createdConnection)); connection = createdConnection;
#else
            Check("connect", connect(0, &connection));
#endif
            nint createdChannel = 0;
            Check("channel", createChannel(connection, 0, &createdChannel)); channel = createdChannel;
            uint createdTarget = 0;
            Check("target resource46", resource(channel, 46, &createdTarget)); target = createdTarget;
            byte[] packet = new byte[92];
            MemoryMarshal.Write(packet.AsSpan(), 0x31u); MemoryMarshal.Write(packet.AsSpan(4), target);
            MemoryMarshal.Write(packet.AsSpan(8), (ulong)(nuint)window);
            MemoryMarshal.Write(packet.AsSpan(32), 32u); MemoryMarshal.Write(packet.AsSpan(36), 16u);
            MemoryMarshal.Write(packet.AsSpan(52), 1f);
#if KK_RESIZE_LIFETIME || KL_RESIZE_LIFETIME || KN_RESIZE_LIFETIME
            MemoryMarshal.Write(packet.AsSpan(56), 12u);
            MemoryMarshal.Write(packet.AsSpan(72), -1);
            MemoryMarshal.Write(packet.AsSpan(76), 1d);
            MemoryMarshal.Write(packet.AsSpan(84), 1d);
            Record("public target contract", new { ResourceType = 46, Flags = 12, DpiAwarenessContext = -1, DpiX = 1d, DpiY = 1d, Adapter = 0, Width = 32, Height = 16 });
#endif
            Send(packet);
            Settings(32, 16);
            factory = Direct3DCreate9(32); Assert.AreNotEqual(0, factory);
            PresentParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            Check("producer HAL", ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &device));
            Check("producer render target surface", ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 12, 1, 21, 0, 0, 1, &surface, 0));
            Check("producer fill (not consumer pixels)", ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, 0xff123456));
            uint callerRefs = References(surface), width = 0, height = 0;
            nint createdBitmap = 0;
            Check("interop create", create(surface, 96, 96, 701, &KjAvailable, 0, &createdBitmap, &width, &height)); bitmap = createdBitmap;
            Assert.AreEqual(12u, width); Assert.AreEqual(1u, height);
            uint beforeRefs = References(surface);
            Assert.IsTrue(beforeRefs > callerRefs);
            Check("snapshot before", SnapshotStatus("snapshot before"));
            Check("producer cooperative before", ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)device)[3])(device));
            Record("before settings", new { TargetHandle = target, Window = (long)window, Producer = (long)device, Surface = (long)surface, Bitmap = (long)bitmap, CallerRefs = callerRefs, SurfaceRefs = beforeRefs });
            KjNotifications.Clear();
            if (resize) Assert.AreNotEqual(0, SetWindowPos(window, 0, 0, 0, 64, 24, 0x16));
            Settings(resize ? 64u : 32u, resize ? 24u : 16u);
            var notifications = KjNotifications.Select(n => new { n.Available, n.Version }).ToArray();
            uint afterRefs = References(surface);
            int status = SnapshotStatus("snapshot after");
            Check("producer cooperative after", ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)device)[3])(device));
            Record("after settings", new { SurfaceRefs = afterRefs, SnapshotHResult = status, Notifications = notifications });
            Assert.IsTrue(notifications.All(n => n.Version == 701));
            bool invalid = notifications.Any(n => n.Available == 0);
#if KN_RESIZE_LIFETIME
            Assert.IsFalse(invalid);
            Assert.AreEqual(beforeRefs, afterRefs);
            Assert.AreEqual(0, status);
#else
            if (!resize)
            {
                Assert.IsFalse(invalid); Assert.AreEqual(beforeRefs, afterRefs); Assert.AreEqual(0, status);
            }
            else if (invalid)
            {
                Assert.AreEqual(callerRefs, afterRefs); Assert.IsTrue(status < 0);
            }
            else
            {
                Assert.AreEqual(beforeRefs, afterRefs); Assert.AreEqual(0, status);
            }
#endif
            Record("observation complete; not shader acceptance", new { Invalid = invalid, Resize = resize });
        }
        finally
        {
            // Keep the caller's producer alive until all target destruction callbacks have completed.
            if (channel != 0) { Check("cleanup destroy channel", destroy(channel)); channel = 0; }
            if (connection != 0) { Check("cleanup disconnect", disconnect(connection)); connection = 0; }
            if (initialized) { Check("cleanup deinitialize", deinitialize()); initialized = false; }
            Record("cleanup target callbacks", KjNotifications.Select(n => new { n.Available, n.Version }).ToArray());
            if (bitmap != 0) { detach(bitmap); Record("cleanup detach"); uint refs = Ref(bitmap, 2); bitmap = 0; Record("cleanup bitmap release", refs); Assert.AreEqual(0u, refs); }
            if (surface != 0) { uint refs = Ref(surface, 2); surface = 0; Record("cleanup surface release", refs); Assert.AreEqual(0u, refs); }
            if (device != 0) { uint refs = Ref(device, 2); device = 0; Record("cleanup device release", refs); Assert.AreEqual(0u, refs); }
            if (factory != 0) { uint refs = Ref(factory, 2); factory = 0; Record("cleanup factory release", refs); Assert.AreEqual(0u, refs); }
            if (window != 0) { Assert.AreNotEqual(0, DestroyWindow(window)); Record("cleanup window destroyed"); }
            Record("cleanup complete");
        }
    }
}
#endif
