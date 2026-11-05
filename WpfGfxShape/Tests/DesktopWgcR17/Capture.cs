using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Windows.Foundation;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace DesktopWgcR17;

public static class Capture
{
    private static readonly object LogGate = new();
    private static void Log(string stage, object value)
    {
        lock (LogGate)
        {
            Directory.CreateDirectory("Results");
            File.AppendAllText("Results/wgc-r18.jsonl", JsonSerializer.Serialize(new { stage, qpc = Stopwatch.GetTimestamp(), frequency = Stopwatch.Frequency, tid = Environment.CurrentManagedThreadId, value }) + Environment.NewLine);
        }
    }

    /// <summary>Starts event capture before one unchanged native control; diagnostic only, never a pixel oracle.</summary>
    public static void Observe(nint window, Action present, Func<long> presentReturn)
    {
        nint device = 0, context = 0, dxgi = 0, inspectable = 0, factory = 0, itemPointer = 0, text = 0;
        IDirect3DDevice? projectedDevice = null;
        Direct3D11CaptureFramePool? pool = null;
        GraphicsCaptureSession? session = null;
        TypedEventHandler<Direct3D11CaptureFramePool, object>? handler = null;
        object gate = new();
        using var signal = new AutoResetEvent(false);
        var frames = new Queue<(Direct3D11CaptureFrame Frame, long Event, long Arrival)>();
        long events = 0;
        bool stopping = false, invoked = false, selected = false;
        int observed = 0;
        var deadline = Stopwatch.StartNew();
        try
        {
            Log("supported", new { supported = GraphicsCaptureSession.IsSupported(), hwnd = (long)window, session = Process.GetCurrentProcess().SessionId, selection = "first valid frame with event QPC and SystemRelativeTime strictly after native Present return; no color filter", boundMs = 3000, maximumFrames = 32, originalAcceptanceExecution = false });
            Check(D3D11CreateDevice(0, 1, 0, 0x20, 0, 0, 7, out device, out uint level, out context));
            Log("D3D11CreateDevice", new { level });
            Guid dxgiId = new("54ec77fa-1377-44e6-8c32-88fd5f44c84c");
            Check(Marshal.QueryInterface(device, ref dxgiId, out dxgi));
            Check(CreateDirect3D11DeviceFromDXGIDevice(dxgi, out inspectable));
            projectedDevice = WinRT.MarshalInterface<IDirect3DDevice>.FromAbi(inspectable);
            string className = "Windows.Graphics.Capture.GraphicsCaptureItem";
            Check(WindowsCreateString(className, className.Length, out text));
            Guid interopId = new("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356");
            Check(RoGetActivationFactory(text, ref interopId, out factory));
            CreateItem(factory, window, out itemPointer);
            var item = GraphicsCaptureItem.FromAbi(itemPointer);
            int expectedWidth = item.Size.Width, expectedHeight = item.Size.Height;
            Log("CreateForWindow", new { hwnd = (long)window, expectedWidth, expectedHeight });
            pool = Direct3D11CaptureFramePool.CreateFreeThreaded(projectedDevice, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, item.Size);
            session = pool.CreateCaptureSession(item);
            handler = (sender, _) =>
            {
                lock (gate)
                {
                    long arrival = Stopwatch.GetTimestamp();
                    long sequence = ++events;
                    Log("FrameArrived", new { sequence, arrival, stopping });
                    if (stopping) return;
                    var next = sender.TryGetNextFrame();
                    if (next is null) Log("event-no-frame", new { sequence });
                    else if (frames.Count >= 32)
                    {
                        Log("discard", new { sequence, ticks = next.SystemRelativeTime.Ticks, reason = "bounded queue capacity; no color test" });
                        next.Dispose();
                    }
                    else frames.Enqueue((next, sequence, arrival));
                    signal.Set();
                }
            };
            pool.FrameArrived += handler;
            Log("subscribed-before-start", new { });
            session.StartCapture();
            Log("session-started-before-clear", new { });
            deadline.Restart();
            long baselineSequence = 0;
            while (baselineSequence == 0 && deadline.ElapsedMilliseconds < 3000 && observed < 32)
            {
                (Direct3D11CaptureFrame Frame, long Event, long Arrival)? baseline = null;
                lock (gate) { if (frames.Count != 0) baseline = frames.Dequeue(); }
                if (baseline is null)
                {
                    signal.WaitOne(Math.Max(0, 3000 - (int)deadline.ElapsedMilliseconds));
                    continue;
                }
                using var frame = baseline.Value.Frame;
                observed++;
                baselineSequence = baseline.Value.Event;
                Log("baseline-consumed", new { sequence = baselineSequence, arrival = baseline.Value.Arrival, ticks = frame.SystemRelativeTime.Ticks, width = frame.ContentSize.Width, height = frame.ContentSize.Height, elapsedMs = deadline.ElapsedMilliseconds });
                Save(frame, baselineSequence, "first baseline event; no color test");
            }
            if (baselineSequence == 0 || deadline.ElapsedMilliseconds >= 3000)
            {
                Log("baseline-timeout-no-present", new { baselineSequence, observed, events, elapsedMs = deadline.ElapsedMilliseconds, boundMs = 3000 });
                return;
            }
            long triggerSequence;
            lock (gate) triggerSequence = events;
            Log("baseline-ready", new { baselineSequence, triggerSequence, elapsedMs = deadline.ElapsedMilliseconds, totalWaitBoundMs = 6000 });
            invoked = true;
            Log("trigger-before", new { });
            present();
            long threshold = presentReturn();
            long threshold100ns = checked((long)((Int128)threshold * 10000000 / Stopwatch.Frequency));
            Log("trigger-after", new { presentReturnQpc = threshold, threshold100ns, boundMs = 3000 });
            while (!selected && observed < 32 && deadline.ElapsedMilliseconds < 6000)
            {
                (Direct3D11CaptureFrame Frame, long Event, long Arrival)? entry = null;
                lock (gate) { if (frames.Count != 0) entry = frames.Dequeue(); }
                if (entry is null)
                {
                    int remaining = Math.Max(0, 6000 - (int)deadline.ElapsedMilliseconds);
                    if (remaining == 0) break;
                    signal.WaitOne(remaining);
                    continue;
                }
                using var frame = entry.Value.Frame;
                observed++;
                long ticks = frame.SystemRelativeTime.Ticks;
                string reason = entry.Value.Event <= triggerSequence ? "event sequence not newer than pre-trigger watermark" : threshold <= 0 ? "missing Present return QPC" : entry.Value.Arrival <= threshold ? "event not after Present return" : ticks <= threshold100ns ? "frame time not after Present return" : frame.ContentSize.Width != expectedWidth || frame.ContentSize.Height != expectedHeight ? "invalid content size" : "selected first post-Present frame";
                selected = reason == "selected first post-Present frame";
                Log("frame-decision", new { sequence = entry.Value.Event, arrival = entry.Value.Arrival, ticks, threshold, threshold100ns, width = frame.ContentSize.Width, height = frame.ContentSize.Height, reason, selected });
                Save(frame, entry.Value.Event, reason);
            }
            Log(selected ? "selected-complete" : "bounded-timeout-or-limit", new { selected, observed, events, elapsedMs = deadline.ElapsedMilliseconds, boundMs = 3000 });
        }
        catch (Exception error) when (error is COMException or TypeLoadException or DllNotFoundException or EntryPointNotFoundException or PlatformNotSupportedException)
        {
            Log("unavailable", new { error = error.ToString(), invoked });
        }
        finally
        {
            lock (gate) stopping = true;
            if (pool is not null && handler is not null) { pool.FrameArrived -= handler; Log("unsubscribed", new { events }); }
            session?.Dispose(); Log("release-session", new { owned = session is not null });
            lock (gate)
            {
                while (frames.Count != 0)
                {
                    var remaining = frames.Dequeue();
                    using var frame = remaining.Frame;
                    Log("discard", new { sequence = remaining.Event, ticks = frame.SystemRelativeTime.Ticks, reason = "after selection or deadline; not reconsidered" });
                    Save(frame, remaining.Event, "after selection or deadline; not reconsidered");
                }
            }
            pool?.Dispose(); Log("release-pool", new { owned = pool is not null });
            projectedDevice?.Dispose(); Log("release-winrt-device", new { owned = projectedDevice is not null });
            Release("item-abi", itemPointer); Release("factory", factory); Release("inspectable", inspectable); Release("dxgi", dxgi); Release("context", context); Release("device", device);
            if (text != 0) Log("delete-hstring", new { hr = WindowsDeleteString(text) });
            Log("cleanup-complete", new { invoked, selected, events, observed });
        }
        if (!invoked) Log("capture-failed-no-present", new { });
    }

    private static void Save(Direct3D11CaptureFrame frame, long sequence, string reason)
    {
        using var surface = frame.Surface;
        var operation = SoftwareBitmap.CreateCopyFromSurfaceAsync(surface);
        var deadline = Stopwatch.StartNew();
        while (operation.Status == AsyncStatus.Started && deadline.ElapsedMilliseconds < 1000) Thread.Sleep(5);
        if (operation.Status == AsyncStatus.Started) { operation.Cancel(); Log("copy-timeout-cancelled", new { sequence, boundMs = 1000 }); return; }
        using var bitmap = operation.GetResults();
        int width = bitmap.PixelWidth, height = bitmap.PixelHeight;
        if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || width < 17 || height < 9 || width > 4096 || height > 4096)
        { Log("invalid-surface", new { sequence, width, height, format = bitmap.BitmapPixelFormat.ToString() }); return; }
        byte[] bytes = new byte[checked(width * height * 4)];
        var buffer = new Windows.Storage.Streams.Buffer((uint)bytes.Length);
        bitmap.CopyToBuffer(buffer);
        using (var reader = DataReader.FromBuffer(buffer)) reader.ReadBytes(bytes);
        string raw = $"Results/wgc-r18-{sequence}.bgra";
        File.WriteAllBytes(raw, bytes);
        string path = $"Results/wgc-r18-{sequence}.bmp";
        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write((ushort)0x4D42); writer.Write(54 + bytes.Length); writer.Write(0); writer.Write(54);
            writer.Write(40); writer.Write(width); writer.Write(-height); writer.Write((ushort)1); writer.Write((ushort)32); writer.Write(0); writer.Write(bytes.Length); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(bytes);
        }
        int offset = (8 * width + 16) * 4, target = 0, zero = 0;
        for (int i = 0; i < bytes.Length; i += 4)
        {
            if (bytes[i] == 0x56 && bytes[i + 1] == 0x34 && bytes[i + 2] == 0x12) target++;
            if (bytes[i] == 0 && bytes[i + 1] == 0 && bytes[i + 2] == 0 && bytes[i + 3] == 0) zero++;
        }
        Log("pixels", new { sequence, reason, width, height, rgb = $"{bytes[offset + 2]:X2}{bytes[offset + 1]:X2}{bytes[offset]:X2}", targetCount = target, zeroBgraCount = zero, path, raw, rawSha = Convert.ToHexString(SHA256.HashData(bytes)), bmpSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) });
    }
    private static void Release(string resource, nint pointer) { if (pointer != 0) Log("release-com", new { resource, remaining = Marshal.Release(pointer) }); }
    private static void Check(int hr) => Marshal.ThrowExceptionForHR(hr);
    private static unsafe void CreateItem(nint factory, nint window, out nint result)
    {
        Guid id = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");
        nint value = 0;
        Check(((delegate* unmanaged[Stdcall]<nint, nint, Guid*, nint*, int>)(*(nint**)factory)[3])(factory, window, &id, &value));
        result = value;
    }
    [DllImport("d3d11.dll")] private static extern int D3D11CreateDevice(nint adapter, uint driver, nint software, uint flags, nint levels, uint count, uint sdk, out nint device, out uint level, out nint context);
    [DllImport("d3d11.dll")] private static extern int CreateDirect3D11DeviceFromDXGIDevice(nint device, out nint result);
    [DllImport("combase.dll", CharSet = CharSet.Unicode)] private static extern int WindowsCreateString(string value, int length, out nint result);
    [DllImport("combase.dll")] private static extern int WindowsDeleteString(nint value);
    [DllImport("combase.dll")] private static extern int RoGetActivationFactory(nint name, ref Guid id, out nint result);
}
