using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private static readonly object R16Gate = new();
    private static nint r16Window;
    private static long r18PresentReturn;
    private static void R18Mark(string stage) => R16Write(stage, new { qpc = System.Diagnostics.Stopwatch.GetTimestamp(), frequency = System.Diagnostics.Stopwatch.Frequency });
    private static void R18Run(nint window, nint device)
    {
        R18Audit(device);
        Action control = () => PresentNativeControl(window, device, false);
        Func<long> returned = () => r18PresentReturn;
        System.Reflection.Assembly.Load("DesktopWgcR17").GetType("DesktopWgcR17.Capture", true)!.GetMethod("Observe")!.Invoke(null, new object[] { window, control, returned });
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct R18Creation { internal uint Adapter, Type; internal nint Focus; internal uint Behavior; }
    [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
    private static extern int R18Metric(int index);
    private static void R18Audit(nint device)
    {
        R18Creation creation = default;
        int creationHr = ((delegate* unmanaged[Stdcall]<nint, R18Creation*, int>)(*(void***)device)[9])(device, &creation);
        byte* caps = stackalloc byte[304];
        new Span<byte>(caps, 304).Clear();
        int capsHr = ((delegate* unmanaged[Stdcall]<nint, void*, int>)(*(void***)device)[7])(device, caps);
        nint factory = 0;
        int factoryHr = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)device)[6])(device, &factory);
        byte* identifier = stackalloc byte[1104];
        new Span<byte>(identifier, 1104).Clear();
        int identifierHr = int.MinValue;
        try
        {
            if (factoryHr >= 0) identifierHr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, void*, int>)(*(void***)factory)[5])(factory, creation.Adapter, 0, identifier);
            Directory.CreateDirectory("Results");
            File.WriteAllBytes("Results/r18-device-caps.bin", new ReadOnlySpan<byte>(caps, 304).ToArray());
            File.WriteAllBytes("Results/r18-adapter-identifier.bin", new ReadOnlySpan<byte>(identifier, 1104).ToArray());
            R16Write("actual-device-audit", new { creationHr, creation.Adapter, deviceType = creation.Type, focus = (long)creation.Focus, behavior = creation.Behavior, softwareVertexProcessing = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)device)[78])(device), capsHr, capsType = *(uint*)caps, capsAdapter = *(uint*)(caps + 4), devCaps = *(uint*)(caps + 28), capsRaw = "r18-device-caps.bin", factoryHr, identifierHr, driver = Marshal.PtrToStringAnsi((nint)identifier), description = Marshal.PtrToStringAnsi((nint)(identifier + 512)), deviceName = Marshal.PtrToStringAnsi((nint)(identifier + 1024)), identifierRaw = "r18-adapter-identifier.bin", remote = R18Metric(0x1000), session = System.Diagnostics.Process.GetCurrentProcess().SessionId, ownerTid = GetWindowThreadProcessId(r16Window, out _), currentTid = GetCurrentThreadId(), interpretation = "SDK 26100: 0x26=FPU_PRESERVE|MULTITHREADED|SOFTWARE_VERTEXPROCESSING; HAL=1; NULLREF is device type 4, not a creation flag; unchanged" });
        }
        finally { if (factory != 0) R16Write("audit-release-factory", new { remaining = Marshal.Release(factory) }); }
    }
    private static void R16Write(string stage, object value)
    {
        lock (R16Gate)
        {
            Directory.CreateDirectory("Results");
            File.AppendAllText("Results/desktop-observation-r16.jsonl", JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, stage, tid = GetCurrentThreadId(), value }) + Environment.NewLine);
        }
    }

    private static void R16Begin(nint window)
    {
        r16Window = window;
        R16Write("before-native-present", new { hwnd = (long)window });
    }

    private static void R16Message(nint window, uint message, nuint wp, nint lp)
    {
        if (window == r16Window)
            R16Write("window-message", new { hwnd = (long)window, message, wp = (ulong)wp, lp = (long)lp });
    }

    private static void R16Observe(nint window)
    {
        GetClientRect(window, out WindowRectangle client);
        GetWindowRect(window, out WindowRectangle bounds);
        WindowPoint origin = new();
        int converted = ClientToScreen(window, ref origin);
        WindowPoint sample = new() { X = 16, Y = 8 };
        ClientToScreen(window, ref sample);
        int frameHr = R16Frame(window, 9, out WindowRectangle frame, 16);
        nint dc = GetDC(window);
        nint screen = GetDC(0);
        nint region = R16CreateRectRgn(0, 0, 0, 0);
        try
        {
            int clipType = R16GetClipBox(dc, out WindowRectangle clip);
            int regionType = R16GetWindowRgn(window, region);
            var intersecting = new List<object>();
            for (nint current = R16GetTopWindow(0); current != 0; current = R16GetWindow(current, 2))
            {
                if (IsWindowVisible(current) == 0 || GetWindowRect(current, out WindowRectangle other) == 0) continue;
                if (other.Right <= bounds.Left || other.Left >= bounds.Right || other.Bottom <= bounds.Top || other.Top >= bounds.Bottom) continue;
                uint tid = GetWindowThreadProcessId(current, out uint pid);
                intersecting.Add(new { hwnd = (long)current, tid, pid, bounds = new[] { other.Left, other.Top, other.Right, other.Bottom }, exStyle = (long)R16GetWindowLongPtr(current, -20), coversSample = (long)WindowFromPoint(sample) == (long)current });
            }
            R16Write("geometry-and-original-dc", new { hwnd = (long)window, client = new[] { client.Left, client.Top, client.Right, client.Bottom }, bounds = new[] { bounds.Left, bounds.Top, bounds.Right, bounds.Bottom }, converted, origin = new[] { origin.X, origin.Y }, sample = new[] { sample.X, sample.Y }, frameHr, frame = new[] { frame.Left, frame.Top, frame.Right, frame.Bottom }, clipType, clip = new[] { clip.Left, clip.Top, clip.Right, clip.Bottom }, regionType, pointVisible = R16PtVisible(dc, 16, 8), windowPixel = GetPixel(dc, 16, 8), screenPixel = GetPixel(screen, sample.X, sample.Y), intersecting });
            int width = client.Right - client.Left, height = client.Bottom - client.Top;
            if (converted == 0 || width <= 0 || height <= 0 || width > 4096 || height > 4096) throw new InvalidOperationException("Invalid diagnostic client bounds.");
            R16Capture("window-srccopy", dc, 0, 0, width, height, 0x00CC0020);
            R16Capture("screen-srccopy", screen, origin.X, origin.Y, width, height, 0x00CC0020);
            R16Capture("screen-captureblt", screen, origin.X, origin.Y, width, height, 0x40CC0020);
            R16Write("wgc-before", new { qpc = System.Diagnostics.Stopwatch.GetTimestamp(), frequency = System.Diagnostics.Stopwatch.Frequency, sample = new[] { 16, 8 }, windowPixel = GetPixel(dc, 16, 8) });
            R16Write("event-capture-already-completed", new { presentReturnQpc = r18PresentReturn, originalOracleUnchanged = true });
            R16Write("wgc-after", new { qpc = System.Diagnostics.Stopwatch.GetTimestamp(), frequency = System.Diagnostics.Stopwatch.Frequency, sample = new[] { 16, 8 }, windowPixel = GetPixel(dc, 16, 8) });
        }
        finally
        {
            R16DeleteObject(region);
            ReleaseDC(window, dc);
            ReleaseDC(0, screen);
        }
    }

    private static void R16Capture(string name, nint source, int x, int y, int width, int height, uint rop)
    {
        nint memory = R16CreateCompatibleDC(source);
        int* info = stackalloc int[11];
        new Span<int>(info, 11).Clear();
        info[0] = 40; info[1] = width; info[2] = -height; info[3] = 0x00200001;
        nint bitmap = R16CreateDIBSection(source, info, 0, out nint bits, 0, 0);
        nint previous = bitmap != 0 && memory != 0 ? R16SelectObject(memory, bitmap) : 0;
        try
        {
            if (previous == 0 || previous == -1 || bits == 0) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            int result = R16BitBlt(memory, 0, 0, width, height, source, x, y, rop);
            int error = result == 0 ? Marshal.GetLastWin32Error() : 0;
            R16GdiFlush();
            byte[] pixels = new byte[checked(width * height * 4)];
            Marshal.Copy(bits, pixels, 0, pixels.Length);
            string path = "Results/r16-" + name + ".bmp";
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write((ushort)0x4D42); writer.Write(54 + pixels.Length); writer.Write(0); writer.Write(54);
                writer.Write(40); writer.Write(width); writer.Write(-height); writer.Write((ushort)1); writer.Write((ushort)32);
                writer.Write(0); writer.Write(pixels.Length); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(pixels);
            }
            R16Write("capture-diagnostic-not-oracle", new { name, result, error, width, height, sourceOrigin = new[] { x, y }, rop, sampleBgr = BitConverter.ToUInt32(pixels, (8 * width + 16) * 4), sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))), path });
        }
        finally
        {
            if (previous != 0 && previous != -1) R16SelectObject(memory, previous);
            if (bitmap != 0) R16DeleteObject(bitmap);
            if (memory != 0) R16DeleteDC(memory);
        }
    }

    [DllImport("dwmapi.dll", EntryPoint = "DwmGetWindowAttribute")] private static extern int R16Frame(nint window, uint attribute, out WindowRectangle value, uint size);
    [DllImport("user32.dll", EntryPoint = "GetTopWindow")] private static extern nint R16GetTopWindow(nint window);
    [DllImport("user32.dll", EntryPoint = "GetWindow")] private static extern nint R16GetWindow(nint window, uint command);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static extern nint R16GetWindowLongPtr(nint window, int index);
    [DllImport("user32.dll", EntryPoint = "GetWindowRgn")] private static extern int R16GetWindowRgn(nint window, nint region);
    [DllImport("gdi32.dll", EntryPoint = "CreateRectRgn")] private static extern nint R16CreateRectRgn(int left, int top, int right, int bottom);
    [DllImport("gdi32.dll", EntryPoint = "GetClipBox")] private static extern int R16GetClipBox(nint dc, out WindowRectangle rectangle);
    [DllImport("gdi32.dll", EntryPoint = "PtVisible")] private static extern int R16PtVisible(nint dc, int x, int y);
    [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)] private static extern nint R16CreateCompatibleDC(nint dc);
    [DllImport("gdi32.dll", EntryPoint = "CreateDIBSection", SetLastError = true)] private static extern nint R16CreateDIBSection(nint dc, int* info, uint usage, out nint bits, nint section, uint offset);
    [DllImport("gdi32.dll", EntryPoint = "SelectObject", SetLastError = true)] private static extern nint R16SelectObject(nint dc, nint value);
    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")] private static extern int R16DeleteObject(nint value);
    [DllImport("gdi32.dll", EntryPoint = "DeleteDC")] private static extern int R16DeleteDC(nint value);
    [DllImport("gdi32.dll", EntryPoint = "GdiFlush")] private static extern int R16GdiFlush();
    [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)] private static extern int R16BitBlt(nint destination, int x, int y, int width, int height, nint source, int sourceX, int sourceY, uint rop);
}
