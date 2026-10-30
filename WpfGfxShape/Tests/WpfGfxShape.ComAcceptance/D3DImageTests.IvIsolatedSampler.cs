#if IV_ISOLATED_SAMPLER
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    /// <summary>Validates a new isolated official two-input VisualBrush sampler, not the historical IS binary.</summary>
    [TestMethod]
    public void WhenIvOfficialIsolatedSamplerUpdatesThenContentAndResourcesConverge()
    {
        string root = Path.Combine(NativeRun.PathsRoot, "iv-sampler");
        Directory.CreateDirectory(root);
        var steps = new List<object>();
        var handles = new List<uint>();
        var events = new List<EventWaitHandle>();
        var pending = new List<object>();
        int sequence = 0;
        bool uncertain = false;
        void Record(string stage, object? detail = null)
        {
            steps.Add(new { Stage = stage, Detail = detail, Utc = DateTimeOffset.UtcNow });
            File.WriteAllText(Path.Combine(root, "ledger.json"), JsonSerializer.Serialize(new { Steps = steps, Resources = handles, Transfers = pending }, new JsonSerializerOptions { WriteIndented = true }));
            ExitDiagnostics.Stage("IV " + stage);
        }
        void Check(string stage, int hr)
        {
            Record(stage, new { HResult = hr });
            Assert.AreEqual(0, hr, stage);
        }
        nint module = NativeLibrary.Load(AcceptancePaths.OriginalDll);
        using var process = Process.GetCurrentProcess();
        string actual = process.Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        object Identity(string path) => new { Path = path, Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), Version = FileVersionInfo.GetVersionInfo(path).ProductVersion };
        File.WriteAllText(Path.Combine(root, "identity.json"), JsonSerializer.Serialize(new
        {
            Official = Identity(actual), CandidateDependency = Identity(AcceptancePaths.AotDll),
            Test = Identity(typeof(D3DImageTests).Assembly.Location), TestMvid = typeof(D3DImageTests).Module.ModuleVersionId,
            Wpf = Identity(typeof(System.Windows.Media.Visual).Assembly.Location), WpfMvid = typeof(System.Windows.Media.Visual).Module.ModuleVersionId,
            Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            NewCurrentInput = true, HistoricalIsExactReproduction = false
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
        var dirty = (delegate* unmanaged[Stdcall]<int, int, int, int, nint, int>)Export("InteropDeviceBitmap_AddDirtyRect");
        var detach = (delegate* unmanaged[Stdcall]<nint, void>)Export("InteropDeviceBitmap_Detach");
        nint connection = 0, channel = 0, window = 0, d3d = 0, device = 0;
        nint[] surfaces = new nint[2], bitmaps = new nint[2];
        bool initialized = false;
        uint Make(uint type)
        {
            uint handle = 0;
            Check("resource create", resource(channel, type, &handle));
            handles.Add(handle);
            Record("resource caller reference", new { type, handle });
            return handle;
        }
        void Send(byte[] bytes, bool separate = false)
        {
            string file = $"{sequence++:D3}-{BitConverter.ToUInt32(bytes):X2}.bin";
            File.WriteAllBytes(Path.Combine(root, file), bytes);
            Record("send before", new { File = file, Length = bytes.Length, Separate = separate, CallerStillOwnsBeforeCall = true });
            // Recording may succeed before a later SendCommand failure; quarantine until consumption is confirmed.
            uncertain = true;
            fixed (byte* pointer = bytes) Check("send recorded", send(pointer, (uint)bytes.Length, separate ? (byte)1 : (byte)0, channel));
        }
        byte[] Words(params uint[] words) => MemoryMarshal.AsBytes(words.AsSpan()).ToArray();
        void Submit()
        {
            Check("batch close", close(channel));
            Record("commit before: transfer outcome unknown");
            Check("batch commit (not completion)", commit(channel));
            Check("batch flush", flush(channel));
            // Flush alone does not prove a live partition consumed embedded references.
        }
        void Present(uint image)
        {
            var completion = new EventWaitHandle(false, EventResetMode.ManualReset);
            events.Add(completion);
            bool held = false;
            nint duplicate = 0;
            try
            {
                completion.SafeWaitHandle.DangerousAddRef(ref held);
                Assert.AreNotEqual(0, DuplicateHandle(-1, completion.SafeWaitHandle.DangerousGetHandle(), -1, &duplicate, 0, 0, 2));
            }
            finally { if (held) completion.SafeWaitHandle.DangerousRelease(); }
            pending.Add(new { Image = image, Original = (long)completion.SafeWaitHandle.DangerousGetHandle(), Duplicate = (long)duplicate, State = "caller-owned duplicate, not yet recorded" });
            Record("event duplicate created");
            byte[] bytes = new byte[16];
            MemoryMarshal.Write(bytes.AsSpan(), 0x0bu); MemoryMarshal.Write(bytes.AsSpan(4), image);
            MemoryMarshal.Write(bytes.AsSpan(8), (ulong)(nuint)duplicate);
            Send(bytes, true);
            Record("event recorded, not submitted", new { image, duplicate = (long)duplicate });
            Check("present commit (not completion)", commit(channel));
            bool completed = completion.WaitOne(TimeSpan.FromSeconds(5));
            Record("ProcessPresent completion", new { image, Completed = completed });
            Assert.IsTrue(completed);
            Check("present flush after event", flush(channel));
            Record("native duplicate consumed; original remains caller-owned", new { image, duplicate = (long)duplicate });
        }
        void Content(uint expected)
        {
            Check("DwmFlush", DwmFlush());
            nint dc = GetDC(window);
            uint pixel;
            try { pixel = GetPixel(dc, 16, 8); }
            finally { ReleaseDC(window, dc); }
            Record("consumer pixel", new { Expected = expected, Actual = pixel });
            Assert.AreEqual(expected, pixel);
        }
        void ReleaseObserved(string name, nint value)
        {
            if (value == 0) return;
            uint refs = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
            Record("COM release", new { Name = name, Pointer = (long)value, Refs = refs });
            Assert.AreEqual(0u, refs);
        }
        try
        {
            Check("initialize", initialize(0)); initialized = true;
            window = CreateOutputWindow("Two input shader acceptance", 40, 40);
            Assert.AreNotEqual(0, window);
            EstablishObservableWindow(window);
            d3d = Direct3DCreate9(32); Assert.AreNotEqual(0, d3d);
            PresentParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            nint createdDevice = 0;
            Check("D3D device", ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)d3d)[16])(d3d, 0, 1, window, 0x26, &parameters, &createdDevice));
            device = createdDevice;
            PresentNativeControl(window, device, false); DiagnoseNativePresent(window, device, false);
            Content(0x00563412u); VerifyTwoInputShaderOnNativeDevice(device);
            nint createdConnection = 0, createdChannel = 0;
            Check("connect async 0", connect(0, &createdConnection)); connection = createdConnection;
            Check("channel", createChannel(connection, 0, &createdChannel)); channel = createdChannel;
            uint[] images = new uint[2], visuals = new uint[2];
            for (int i = 0; i < 2; i++)
            {
                nint surface = 0, bitmap = 0;
                Check("surface", ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 12, 1, 21, 0, 0, 1, &surface, 0));
                surfaces[i] = surface;
                uint color = i == 0 ? 0xffff0000u : 0xff00ff00u;
                Check("producer fill", ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, color));
                Record("producer color", new { Index = i, Color = color });
                uint width = 0, height = 0;
                Check("interop", create(surface, 96, 96, 7, &Available, 0, &bitmap, &width, &height)); bitmaps[i] = bitmap;
                uint image = Make(97), data = Make(43), visual = Make(39);
                images[i] = image; visuals[i] = visual;
                byte[] update = new byte[24]; MemoryMarshal.Write(update.AsSpan(), 0x0au); MemoryMarshal.Write(update.AsSpan(4), image); MemoryMarshal.Write(update.AsSpan(8), (ulong)(nuint)bitmap);
                uint refs = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
                pending.Add(new { Image = image, Bitmap = (long)bitmap, AddRef = refs, State = "0xA transfer reference pending; caller retains original" });
                Record("interop AddRef before recording"); Send(update);
                DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 32, Height = 16, Image = image };
                Send(new ReadOnlySpan<byte>(&draw, sizeof(DrawPacket)).ToArray()); Send(Words(0x22, visual, data));
                Check("dirty", dirty(0, 0, 12, 1, bitmap));
            }
            uint shader = Make(33), implicitBrush = Make(34), brush = Make(82), effect = Make(38);
            byte[] packet = new byte[20 + TwoInputShaderTokens.Length * 4];
            MemoryMarshal.Write(packet.AsSpan(), 0x6cu); MemoryMarshal.Write(packet.AsSpan(4), shader); MemoryMarshal.Write(packet.AsSpan(8), 2u); MemoryMarshal.Write(packet.AsSpan(12), (uint)(TwoInputShaderTokens.Length * 4));
            TwoInputShaderTokens.CopyTo(MemoryMarshal.Cast<byte, uint>(packet.AsSpan(20))); Send(packet);
            packet = new byte[28]; MemoryMarshal.Write(packet.AsSpan(), 0x6du); MemoryMarshal.Write(packet.AsSpan(4), implicitBrush); MemoryMarshal.Write(packet.AsSpan(8), 1.0); Send(packet);
            packet = new byte[148]; MemoryMarshal.Write(packet.AsSpan(), 0x83u); MemoryMarshal.Write(packet.AsSpan(4), brush); MemoryMarshal.Write(packet.AsSpan(8), 1.0);
            foreach (int offset in new[] { 32, 40, 64, 72 }) MemoryMarshal.Write(packet.AsSpan(offset), 1.0);
            MemoryMarshal.Write(packet.AsSpan(80), 0.5); MemoryMarshal.Write(packet.AsSpan(88), 2.0); MemoryMarshal.Write(packet.AsSpan(108), 1u); MemoryMarshal.Write(packet.AsSpan(112), 1u); MemoryMarshal.Write(packet.AsSpan(124), 1u); MemoryMarshal.Write(packet.AsSpan(144), visuals[1]); Send(packet);
            packet = new byte[122]; MemoryMarshal.Write(packet.AsSpan(), 0x70u); MemoryMarshal.Write(packet.AsSpan(4), effect); MemoryMarshal.Write(packet.AsSpan(40), shader); MemoryMarshal.Write(packet.AsSpan(44), -1);
            MemoryMarshal.Write(packet.AsSpan(48), 2u); MemoryMarshal.Write(packet.AsSpan(52), 16u); MemoryMarshal.Write(packet.AsSpan(72), 16u); MemoryMarshal.Write(packet.AsSpan(76), 8u);
            for (int i = 0; i < 3; i++) MemoryMarshal.Write(packet.AsSpan(82 + i * 4), 0.5f);
            int[] info = [0, 1, 1, 1]; for (int i = 0; i < 4; i++) MemoryMarshal.Write(packet.AsSpan(98 + i * 4), info[i]);
            MemoryMarshal.Write(packet.AsSpan(114), implicitBrush); MemoryMarshal.Write(packet.AsSpan(118), brush); Send(packet); Send(Words(0x1d, visuals[0], effect));
            uint target = Make(46);
            packet = new byte[92]; MemoryMarshal.Write(packet.AsSpan(), 0x31u); MemoryMarshal.Write(packet.AsSpan(4), target); MemoryMarshal.Write(packet.AsSpan(8), (ulong)(nuint)window);
            MemoryMarshal.Write(packet.AsSpan(32), 32u); MemoryMarshal.Write(packet.AsSpan(36), 16u); MemoryMarshal.Write(packet.AsSpan(52), 1f); Send(packet);
            Assert.AreNotEqual(0, GetWindowRect(window, out WindowRectangle bounds));
            packet = new byte[72]; MemoryMarshal.Write(packet.AsSpan(), 0x33u); MemoryMarshal.Write(packet.AsSpan(4), target); MemoryMarshal.Write(packet.AsSpan(8), bounds); MemoryMarshal.Write(packet.AsSpan(32), 1f); MemoryMarshal.Write(packet.AsSpan(44), 1);
            Send(packet); Send(Words(0x35, target, visuals[0])); Submit();
            Present(images[0]); Present(images[1]); Content(0x0000ff80u); uncertain = false;
            Check("producer mutation", ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surfaces[1], 0, 0xff0000ffu));
            Record("producer mutation color", 0xff0000ffu);
            Check("mutation dirty", dirty(0, 0, 12, 1, bitmaps[1])); Present(images[1]); Content(0x00ff0080u); uncertain = false;
            for (int i = 0; i < 2; i++) { detach(bitmaps[i]); Record("Detach", i); }
            Send(Words(0x35, target, 0)); Send(Words(0x1d, visuals[0], 0));
            for (int i = 0; i < 2; i++) Send(Words(0x22, visuals[i], 0));
            // A terminal present confirms a live consumer before deleting its image resource.
            Submit(); Present(images[1]);
            for (int i = handles.Count - 1; i >= 0; i--)
            {
                int deleted = 0; Check("resource delete", delete(channel, handles[i], &deleted));
                Record("resource delete recorded", new { Handle = handles[i], Deleted = deleted }); Assert.AreEqual(1, deleted);
            }
            Submit(); Check("delete flush", flush(channel));
            Check("channel destroy", destroy(channel)); channel = 0;
            Check("disconnect", disconnect(connection)); connection = 0;
            Check("deinitialize", deinitialize()); initialized = false;
            uncertain = false;
            for (int i = 1; i >= 0; i--) { ReleaseObserved($"bitmap{i}", bitmaps[i]); bitmaps[i] = 0; ReleaseObserved($"surface{i}", surfaces[i]); surfaces[i] = 0; }
            ReleaseObserved("device", device); device = 0; ReleaseObserved("d3d", d3d); d3d = 0;
            Record("normal resource convergence complete");
        }
        finally
        {
            if (uncertain)
            {
                Record("NONNORMAL: unknown transfer; retain originals, duplicates and COM holdings; supervisor must terminate child; OS reclamation is not refs0");
                // Only the already-running supervisor may end this uncertain child; never close a possibly native-owned duplicate.
                Thread.Sleep(TimeSpan.FromSeconds(130));
                Environment.Exit(41);
            }
            if (channel != 0) Check("failure channel destroy", destroy(channel));
            if (connection != 0) Check("failure disconnect", disconnect(connection));
            if (initialized) Check("failure deinitialize", deinitialize());
            for (int i = 1; i >= 0; i--) { if (bitmaps[i] != 0) detach(bitmaps[i]); Release(bitmaps[i]); Release(surfaces[i]); }
            Release(device); Release(d3d);
            foreach (EventWaitHandle completion in events) completion.Dispose();
            if (window != 0) Assert.AreNotEqual(0, DestroyWindow(window));
            Record("original events and window closed");
        }
    }
}
#endif
