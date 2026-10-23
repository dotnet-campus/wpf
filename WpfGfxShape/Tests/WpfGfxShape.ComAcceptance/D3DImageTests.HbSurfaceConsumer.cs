#if HB_SURFACE_CONSUMER
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    /// <summary>Observes a real asynchronous official D3DImage consumer without producer readback.</summary>
    [TestMethod]
    public void WhenHbOfficialSurfaceMutatesThenConsumerWindowMustChange() => RunHbConsumer(true);

    /// <summary>Observes the same independent consumer contract for the preserved candidate.</summary>
    [TestMethod]
    public void WhenHbCandidateSurfaceMutatesThenConsumerWindowMustChange() => RunHbConsumer(false);

    [DllImport("user32.dll")]
    private static extern nint GetWindowDpiAwarenessContext(nint window);

    [DllImport("user32.dll")]
    private static extern int AreDpiAwarenessContextsEqual(nint first, nint second);

    private static void RunHbConsumer(bool official)
    {
        string role = official ? "official" : "candidate";
        string root = NativeRun.PathsRoot;
        Directory.CreateDirectory(root);
        string trace = Path.Combine(root, "hb-consumer.jsonl");
        void Record(string stage, object? data = null)
        {
            using var stream = new FileStream(trace, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(stream, leaveOpen: true);
            writer.WriteLine(JsonSerializer.Serialize(new { Stage = stage, Data = data, Utc = DateTimeOffset.UtcNow,
                Qpc = Stopwatch.GetTimestamp(), Thread = Environment.CurrentManagedThreadId, NativeThread = GetCurrentThreadId() }));
            writer.Flush();
            stream.Flush(true);
            ExitDiagnostics.Stage("HB " + stage);
        }
        void Check(string stage, int hr)
        {
            Record(stage, new { HResult = hr });
            Assert.AreEqual(0, hr, stage);
        }
        string requested = official ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(requested);
        using var process = Process.GetCurrentProcess();
        string actual = process.Modules.Cast<ProcessModule>().Single(m => m.BaseAddress == module).FileName;
        string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        Record("identity", new { Role = role, Requested = requested, Actual = actual, Sha256 = Sha(actual),
            Version = FileVersionInfo.GetVersionInfo(actual).ProductVersion,
            Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            PointerSize = IntPtr.Size, TestSha256 = Sha(typeof(D3DImageTests).Assembly.Location),
            Mvid = typeof(D3DImageTests).Module.ModuleVersionId,
            WpfVersion = FileVersionInfo.GetVersionInfo(typeof(System.Windows.Media.Visual).Assembly.Location).ProductVersion,
            WpfSha256 = Sha(typeof(System.Windows.Media.Visual).Assembly.Location),
            ProducerReadback = false, HardwareConsumerProven = false });
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var initialize = (delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager");
        var deinitialize = (delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager");
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var releaseResource = (delegate* unmanaged[Stdcall]<nint, uint, int*, int>)Export("MilResource_ReleaseOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        var create = (delegate* unmanaged[Stdcall]<nint, double, double, uint, delegate* unmanaged[Stdcall]<int, uint, void>, int, nint*, uint*, uint*, int>)Export("InteropDeviceBitmap_Create");
        var dirty = (delegate* unmanaged[Stdcall]<int, int, int, int, nint, int>)Export("InteropDeviceBitmap_AddDirtyRect");
        var detach = (delegate* unmanaged[Stdcall]<nint, void>)Export("InteropDeviceBitmap_Detach");
        nint window = 0, d3d = 0, device = 0, surface = 0, bitmap = 0, connection = 0, channel = 0;
        bool initialized = false;
        var handles = new List<uint>();
        using var completion = new EventWaitHandle(false, EventResetMode.ManualReset);
        try
        {
            Record("native fresh prerequisite before", new { SwapEffect = 1, Evidence = Path.Combine(AcceptancePaths.Results, "bj-fresh-1.json") });
            try
            {
                CheckFreshDesktopDelivery(1, false);
                Record("native fresh prerequisite accepted");
            }
            catch (AssertFailedException error)
            {
                Record("native fresh prerequisite rejected", new { Error = error.ToString(), ConsumerEntered = false });
                throw;
            }
            window = CreateOutputWindow("HB independent consumer", 40, 40);
            Assert.AreNotEqual(0, window);
            EstablishObservableWindow(window);
            d3d = Direct3DCreate9(32);
            Assert.AreNotEqual(0, d3d);
            PresentParameters parameters = new() { Width = 32, Height = 16, Count = 1, SwapEffect = 1, Window = window, Windowed = 1 };
            Check("producer device", ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)d3d)[16])(d3d, 0, 1, window, 0x26, &parameters, &device));
            Check("producer surface", ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, uint, int, nint*, nint, int>)(*(void***)device)[28])(device, 32, 16, 21, 0, 0, 1, &surface, 0));
            uint width = 0, height = 0;
            Record("interop create before");
            Check("interop create", create(surface, 96, 96, 7, &Available, 0, &bitmap, &width, &height));
            Assert.AreEqual((32u, 16u), (width, height));
            Check("partition initialize", initialize(0));
            initialized = true;
            Check("async connect", connect(0, &connection));
            Check("channel create", channelCreate(connection, 0, &channel));
            uint image = 0, data = 0, visual = 0, destination = 0;
            Check("image resource", resource(channel, 97, &image)); handles.Add(image);
            Check("drawing resource", resource(channel, 43, &data)); handles.Add(data);
            Check("visual resource", resource(channel, 39, &visual)); handles.Add(visual);
            Check("target resource", resource(channel, 46, &destination)); handles.Add(destination);
            ImagePacket update = new() { Type = 0x0a, Handle = image, Interop = (ulong)bitmap };
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
            Check("image update", send(&update, (uint)sizeof(ImagePacket), 0, channel));
            DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 32, Height = 16, Image = image };
            Check("draw image", send(&draw, (uint)sizeof(DrawPacket), 0, channel));
            uint* content = stackalloc uint[] { 0x22, visual, data };
            Check("visual content", send(content, 12, 0, channel));
            byte* target = stackalloc byte[92];
            new Span<byte>(target, 92).Clear();
            *(uint*)target = 0x31; *(uint*)(target + 4) = destination; *(ulong*)(target + 8) = (ulong)window;
            *(uint*)(target + 32) = 32; *(uint*)(target + 36) = 16; *(float*)(target + 52) = 1;
            *(uint*)(target + 56) = 0x0c;
            nint dpiContext = GetWindowDpiAwarenessContext(window);
            int protocolContext = 0;
            foreach (int value in new[] { -1, -2, -3, -4 })
                if (AreDpiAwarenessContextsEqual(dpiContext, value) != 0) protocolContext = value;
            Assert.AreNotEqual(0, protocolContext);
            *(int*)(target + 72) = protocolContext;
            double dpiScale = GetDpiForWindow(window) / 96.0;
            Assert.IsTrue(dpiScale > 0);
            *(double*)(target + 76) = dpiScale; *(double*)(target + 84) = dpiScale;
            Assert.AreNotEqual(0, GetClientRect(window, out WindowRectangle clientBounds));
            Assert.AreEqual((32, 16), (clientBounds.Right - clientBounds.Left, clientBounds.Bottom - clientBounds.Top));
            Assert.AreNotEqual(0, GetWindowRect(window, out WindowRectangle targetBounds));
            WindowPoint sample = new() { X = 16, Y = 8 };
            Assert.AreNotEqual(0, ClientToScreen(window, ref sample));
            Record("target contract", new { PacketSize = 92, DpiXOffset = 76, DpiYOffset = 84,
                DpiX = *(double*)(target + 76), DpiY = *(double*)(target + 84),
                TargetFlags = *(uint*)(target + 56), TargetPixelFormat = *(uint*)(target + 68),
                DpiAwarenessContext = protocolContext, RawDpiContext = (long)dpiContext,
                OwnerThread = GetWindowThreadProcessId(window, out _), Client = clientBounds,
                ProducerFormat = 21, Width = 32, Height = 16, Window = (long)window,
                Left = targetBounds.Left, Top = targetBounds.Top, Right = targetBounds.Right, Bottom = targetBounds.Bottom,
                SampleScreenX = sample.X, SampleScreenY = sample.Y, Readback = "GetPixel(window DC, client 16,8)",
                Backend = "not independently observed", DuplicateDirectCloseCount = 0 });
            Check("target create", send(target, 92, 0, channel));
            SendOutputWindowSettings(send, channel, destination, window);
            uint* tree = stackalloc uint[] { 0x35, destination, visual };
            Check("target root", send(tree, 12, 0, channel));
            Check("setup close", close(channel));
            Check("setup commit", commit(channel));
            Check("setup flush", flush(channel));
            uint* invalidate = stackalloc uint[] { 0x37, destination, 0, 0, 32, 16 };
            Check("initial invalidate", send(invalidate, 24, 0, channel));
            Check("initial invalidate close", close(channel));
            Check("initial invalidate commit", commit(channel));
            uint[] colors = [0xffff0000u, 0xff0000ffu, 0xff00ff00u];
            for (int frame = 0; frame < colors.Length; frame++)
            {
                Check("producer color fill", ((delegate* unmanaged[Stdcall]<nint, nint, nint, uint, int>)(*(void***)device)[35])(device, surface, 0, colors[frame]));
                CompleteNativePresent(device, window);
                Record("producer update complete", new { Frame = frame, Color = colors[frame] });
                Check("dirty", dirty(0, 0, 32, 16, bitmap));
                Assert.IsTrue(completion.Reset());
                nint duplicate;
                bool held = false;
                try
                {
                    completion.SafeWaitHandle.DangerousAddRef(ref held);
                    Assert.AreNotEqual(0, DuplicateHandle(-1, completion.SafeWaitHandle.DangerousGetHandle(), -1, &duplicate, 0, 0, 2));
                }
                finally { if (held) completion.SafeWaitHandle.DangerousRelease(); }
                Record("duplicate transfer before", new { Frame = frame, Duplicate = (long)duplicate });
                PresentPacket packet = new() { Type = 0x0b, Handle = image, Event = (ulong)(nuint)duplicate };
                Check("present separate send", send(&packet, (uint)sizeof(PresentPacket), 1, channel));
                Check("present commit", commit(channel));
                bool signalled = completion.WaitOne(TimeSpan.FromSeconds(5));
                Record("event complete", new { Frame = frame, Signalled = signalled });
                Assert.IsTrue(signalled);
                Check("consumer flush", flush(channel));
                uint expected = ((colors[frame] & 255) << 16) | (colors[frame] & 0xff00) | ((colors[frame] >> 16) & 255);
                uint pixel = uint.MaxValue;
                long deadline = Environment.TickCount64 + 5000;
                do
                {
                    Check("dwm flush", DwmFlush());
                    nint dc = GetDC(window);
                    try { pixel = GetPixel(dc, 16, 8); }
                    finally { ReleaseDC(window, dc); }
                    if (pixel == expected) break;
                    Thread.Sleep(20);
                } while (Environment.TickCount64 < deadline);
                Record("consumer pixel", new { Frame = frame, ProducerColor = colors[frame], Expected = expected, Actual = pixel, Accepted = pixel == expected });
                Assert.AreEqual(expected, pixel);
                Assert.IsTrue(completion.Reset());
                Assert.IsTrue(completion.Set());
                Assert.IsTrue(completion.WaitOne(0));
            }
            Record("three consumer frames accepted");
        }
        finally
        {
            if (bitmap != 0) { Record("detach before"); detach(bitmap); Record("detach after"); }
            if (channel != 0)
            {
                for (int i = handles.Count - 1; i >= 0; i--)
                {
                    int deleted = 0;
                    Check("resource unregister", releaseResource(channel, handles[i], &deleted));
                    Record("resource deleted", new { Handle = handles[i], Deleted = deleted });
                    Assert.AreEqual(1, deleted);
                }
                Check("delete close", close(channel)); Check("delete commit", commit(channel)); Check("delete flush", flush(channel));
                Record("destroy before"); Check("destroy after", destroy(channel));
            }
            if (connection != 0) Check("disconnect", disconnect(connection));
            if (initialized) Check("partition deinitialize", deinitialize());
            uint Drop(nint value) => value == 0 ? 0 : ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
            Record("refs", new { Bitmap = Drop(bitmap), Surface = Drop(surface), Device = Drop(device), Factory = Drop(d3d) });
            if (window != 0)
            {
                int destroyed = DestroyWindow(window);
                Record("window destroy", new { Result = destroyed });
                Assert.AreNotEqual(0, destroyed);
            }
            Record("cleanup complete");
        }
    }
}
#endif
