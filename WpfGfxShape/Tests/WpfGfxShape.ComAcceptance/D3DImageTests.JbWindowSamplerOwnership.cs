#if JB_WINDOW_SAMPLER_CONTRACT
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    /// <summary>Measures official 0xA transfer and replacement ownership without a window consumer or Present.</summary>
    [TestMethod]
    public void WhenJbOfficialInteropTransferIsClearedThenReferencesConverge() => JbObserveOwnership(true);

    /// <summary>Measures the preserved candidate using the same isolated ownership contract.</summary>
    [TestMethod]
    public void WhenJbCandidateInteropTransferIsClearedThenReferencesConverge() => JbObserveOwnership(false);

    [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)]
    private static extern int JbCloseHandle(nint handle);

    private static void JbObserveOwnership(bool official)
    {
        string root = Path.Combine(NativeRun.PathsRoot, "jb-ownership");
        Directory.CreateDirectory(root);
        var steps = new List<object>();
        void Record(string stage, object? detail = null)
        {
            steps.Add(new { Stage = stage, Detail = detail, Utc = DateTimeOffset.UtcNow });
            File.WriteAllText(Path.Combine(root, "ownershipledger.json"), JsonSerializer.Serialize(steps, new JsonSerializerOptions { WriteIndented = true }));
            ExitDiagnostics.Stage("JB " + stage);
        }
        void Check(string stage, int hr)
        {
            Record(stage, new { HResult = hr });
            Assert.AreEqual(0, hr, stage);
        }
        nint module = NativeLibrary.Load(official ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll);
        using var process = Process.GetCurrentProcess();
        string actual = process.Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        object Identity(string path) => new { Path = path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), Version = FileVersionInfo.GetVersionInfo(path).ProductVersion };
        File.WriteAllText(Path.Combine(root, "identity.json"), JsonSerializer.Serialize(new
        {
            Official = official, ActualModule = Identity(actual), Test = Identity(typeof(D3DImageTests).Assembly.Location),
            TestMvid = typeof(D3DImageTests).Module.ModuleVersionId,
            Wpf = Identity(typeof(System.Windows.Media.Visual).Assembly.Location), WpfMvid = typeof(System.Windows.Media.Visual).Module.ModuleVersionId,
            Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            Contract = "JB_WINDOW_SAMPLER_CONTRACT", WindowConsumerExecuted = false, PresentExecuted = false,
            NativeDisplayPreconditionExecuted = false, SoftwareFallbackUsed = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var delete = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)Export("MilResource_ReleaseOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        var create = (delegate* unmanaged[Stdcall]<nint, double, double, uint, delegate* unmanaged[Stdcall]<int, uint, void>, int, nint*, uint*, uint*, int>)Export("InteropDeviceBitmap_Create");
        var detach = (delegate* unmanaged[Stdcall]<nint, void>)Export("InteropDeviceBitmap_Detach");
        nint connection = 0, channel = 0, window = 0, d3d = 0, device = 0, surface = 0, bitmap = 0;
        bool initialized = false, uncertain = false, completed = false;
        uint Ref(nint value, int slot) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[slot])(value);
        uint ObserveRefs(string stage)
        {
            uint added = Ref(bitmap, 1);
            uint remaining = Ref(bitmap, 2);
            Record(stage, new { Added = added, Remaining = remaining });
            Assert.AreEqual(added - 1, remaining);
            return remaining;
        }
        void Submit()
        {
            Check("close batch", close(channel));
            Check("commit (not consumption proof)", commit(channel));
            Check("sync flush (not embedded ownership proof)", flush(channel));
        }
        uint image = 0;
        void Update(nint pointer, string file)
        {
            byte[] bytes = new byte[24];
            MemoryMarshal.Write(bytes.AsSpan(), 0x0au);
            MemoryMarshal.Write(bytes.AsSpan(4), image);
            MemoryMarshal.Write(bytes.AsSpan(8), (ulong)(nuint)pointer);
            File.WriteAllBytes(Path.Combine(root, file), bytes);
            uncertain = true;
            Record("0xA send entering; embedded transfer uncertain", new { Pointer = (long)pointer, File = file });
            fixed (byte* data = bytes) Check("0xA recorded", send(data, 24, 0, channel));
            Submit();
        }
        try
        {
            // This duplicate is never sent: cancellation before recording has an unambiguous caller owner.
            using (var completion = new EventWaitHandle(false, EventResetMode.ManualReset))
            {
                nint duplicate = 0;
                bool held = false;
                try
                {
                    completion.SafeWaitHandle.DangerousAddRef(ref held);
                    Assert.AreNotEqual(0, DuplicateHandle(-1, completion.SafeWaitHandle.DangerousGetHandle(), -1, &duplicate, 0, 0, 2));
                    Record("unrecorded duplicate caller-owned", new { Original = (long)completion.SafeWaitHandle.DangerousGetHandle(), Duplicate = (long)duplicate });
                    Assert.AreNotEqual(0, duplicate);
                    nint callerOwnedDuplicate = duplicate;
                    duplicate = 0;
                    Assert.AreNotEqual(0, JbCloseHandle(callerOwnedDuplicate));
                    Assert.IsFalse(completion.WaitOne(0));
                    Assert.IsTrue(completion.Set());
                    Assert.IsTrue(completion.WaitOne(0));
                    Record("unrecorded duplicate closed once; original usable");
                }
                finally
                {
                    if (duplicate != 0) JbCloseHandle(duplicate);
                    if (held) completion.SafeWaitHandle.DangerousRelease();
                }
            }
            Record("original completion disposed; no event delivered");
            Check("initialize", initialize(0)); initialized = true;
            window = CreateOutputWindow("JB ownership control", 40, 40);
            Assert.AreNotEqual(0, window);
            d3d = Direct3DCreate9(32); Assert.AreNotEqual(0, d3d);
            PresentParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            Check("hardware device", ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)d3d)[16])(d3d, 0, 1, window, 0x26, &parameters, &device));
            Check("surface", ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 12, 1, 21, 0, 0, 1, &surface, 0));
            uint width = 0, height = 0;
            nint createdBitmap = 0;
            Check("interop bitmap", create(surface, 96, 96, 7, &Available, 0, &createdBitmap, &width, &height));
            bitmap = createdBitmap;
            Assert.AreNotEqual(0, bitmap);
            uint baseline = ObserveRefs("bitmap caller baseline");
            Assert.AreEqual(1u, baseline);
            Check("async connection", connect(0, &connection));
            nint createdChannel = 0;
            Check("channel", createChannel(connection, 0, &createdChannel)); channel = createdChannel;
            uint createdImage = 0;
            Check("image resource97", resource(channel, 97, &createdImage)); image = createdImage;
            uint transfer = Ref(bitmap, 1);
            Record("bitmap AddRef for 0xA", new { TransferRefs = transfer });
            uncertain = true;
            Update(bitmap, "update-transfer.bin");
            Assert.AreEqual(baseline + 1, ObserveRefs("after transfer and flush"));
            Update(0, "update-clear.bin");
            Assert.AreEqual(baseline, ObserveRefs("after null replacement and flush"));
            uncertain = false;
            int removed = 0;
            Check("release image resource", delete(channel, image, &removed));
            Record("image caller reference deleted", new { Removed = removed });
            Assert.AreNotEqual(0, removed);
            Submit();
            Check("destroy channel", destroy(channel)); channel = 0;
            Check("disconnect", disconnect(connection)); connection = 0;
            Check("deinitialize", deinitialize()); initialized = false;
            detach(bitmap); Record("bitmap detached");
            uint bitmapRefs = Ref(bitmap, 2); bitmap = 0;
            Record("bitmap final release", new { Refs = bitmapRefs }); Assert.AreEqual(0u, bitmapRefs);
            uint surfaceRefs = Ref(surface, 2); surface = 0;
            Record("surface final release", new { Refs = surfaceRefs }); Assert.AreEqual(0u, surfaceRefs);
            uint deviceRefs = Ref(device, 2); device = 0;
            Record("device final release", new { Refs = deviceRefs }); Assert.AreEqual(0u, deviceRefs);
            uint factoryRefs = Ref(d3d, 2); d3d = 0;
            Record("factory final release", new { Refs = factoryRefs }); Assert.AreEqual(0u, factoryRefs);
            Assert.AreNotEqual(0, DestroyWindow(window)); window = 0;
            completed = true;
            Record("normal ownership contract complete; no window consumer or Present");
        }
        finally
        {
            if (uncertain)
            {
                Record("unknown transfer quarantined; supervisor must terminate; not normal refs0");
                using var quarantine = new EventWaitHandle(false, EventResetMode.ManualReset);
                quarantine.WaitOne();
            }
            if (!completed)
            {
                Record("incomplete control cleanup; not a passing lifecycle");
                if (channel != 0) Record("failure destroy", new { HResult = destroy(channel) });
                if (connection != 0) Record("failure disconnect", new { HResult = disconnect(connection) });
                if (initialized) Record("failure deinitialize", new { HResult = deinitialize() });
                if (bitmap != 0) { detach(bitmap); Record("failure bitmap release", new { Refs = Ref(bitmap, 2) }); }
                if (surface != 0) Record("failure surface release", new { Refs = Ref(surface, 2) });
                if (device != 0) Record("failure device release", new { Refs = Ref(device, 2) });
                if (d3d != 0) Record("failure factory release", new { Refs = Ref(d3d, 2) });
                if (window != 0) Record("failure window destroy", new { Result = DestroyWindow(window) });
            }
        }
    }
}
#endif
