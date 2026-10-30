#if HN_DISPLAY_STATE
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [StructLayout(LayoutKind.Sequential)] private struct HnLuid { internal uint Low; internal int High; internal long Value => ((long)High << 32) | Low; }
    [StructLayout(LayoutKind.Sequential)] private struct HnRational { internal uint Numerator, Denominator; }
    [StructLayout(LayoutKind.Sequential)] private struct HnSource { internal HnLuid Adapter; internal uint Id, ModeIndex, Status; }
    [StructLayout(LayoutKind.Sequential)] private struct HnTarget
    {
        internal HnLuid Adapter; internal uint Id, ModeIndex, Technology, Rotation, Scaling;
        internal HnRational Refresh; internal uint Scanline; internal int Available; internal uint Status;
    }
    [StructLayout(LayoutKind.Sequential)] private struct HnPath { internal HnSource Source; internal HnTarget Target; internal uint Flags; }
    [StructLayout(LayoutKind.Explicit, Size = 64)] private struct HnMode
    {
        [FieldOffset(0)] internal uint Type; [FieldOffset(4)] internal uint Id; [FieldOffset(8)] internal HnLuid Adapter;
        [FieldOffset(16)] internal uint Width; [FieldOffset(20)] internal uint Height; [FieldOffset(24)] internal uint PixelFormat;
        [FieldOffset(28)] internal int X; [FieldOffset(32)] internal int Y;
    }
    [StructLayout(LayoutKind.Sequential)] private struct HnHeader { internal uint Type, Size; internal HnLuid Adapter; internal uint Id; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct HnSourceName { internal HnHeader Header; internal fixed char Name[32]; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct HnTargetName
    {
        internal HnHeader Header; internal uint Flags, Technology; internal ushort Manufacturer, Product;
        internal uint Connector; internal fixed char Friendly[64]; internal fixed char Path[128];
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct HnMonitor
    {
        internal uint Size; internal WindowRectangle Monitor, Work; internal uint Flags; internal fixed char Device[32];
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct HnDisplayDevice
    {
        internal uint Size; internal fixed char Name[32]; internal fixed char Description[128]; internal uint Flags;
        internal fixed char Id[128]; internal fixed char Key[128];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)] private struct HnTiming
    {
        internal uint Size; internal HnRational Refresh; internal ulong QpcRefreshPeriod; internal HnRational Compose;
        internal ulong QpcVBlank, RefreshCount; internal uint DxRefresh; internal ulong QpcCompose, Frame; internal uint DxPresent;
        internal ulong RefreshFrame, FrameSubmitted; internal uint DxPresentSubmitted; internal ulong FrameConfirmed;
        internal uint DxPresentConfirmed; internal ulong RefreshConfirmed; internal uint DxRefreshConfirmed;
        internal ulong FramesLate; internal uint FramesOutstanding;
        internal ulong FrameDisplayed, QpcFrameDisplayed, RefreshFrameDisplayed, FrameComplete, QpcFrameComplete,
            FramePending, QpcFramePending, FramesDisplayed, FramesComplete, FramesPending, FramesAvailable,
            FramesDropped, FramesMissed, RefreshNextDisplayed, RefreshNextPresented, RefreshesDisplayed,
            RefreshesPresented, RefreshStarted, PixelsReceived, PixelsDrawn, BuffersEmpty;
    }
    [DllImport("user32.dll")] private static extern int GetDisplayConfigBufferSizes(uint flags, out uint paths, out uint modes);
    [DllImport("user32.dll")] private static extern int QueryDisplayConfig(uint flags, ref uint paths, HnPath* path, ref uint modes, HnMode* mode, nint topology);
    [DllImport("user32.dll")] private static extern int DisplayConfigGetDeviceInfo(HnHeader* header);
    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", SetLastError = true)] private static extern int HnGetMonitorInfo(nint monitor, HnMonitor* info);
    [DllImport("user32.dll", EntryPoint = "EnumDisplayDevicesW", CharSet = CharSet.Unicode)] private static extern int HnEnumDevices(string? device, uint index, HnDisplayDevice* value, uint flags);
    [DllImport("user32.dll", EntryPoint = "EnumDisplaySettingsExW", CharSet = CharSet.Unicode)] private static extern int HnEnumMode(string device, int mode, byte* value, uint flags);
    [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true)] private static extern int HnSpi(uint action, uint parameter, out int value, uint flags);
    [DllImport("dwmapi.dll")] private static extern int DwmGetCompositionTimingInfo(nint window, HnTiming* timing);

    private static void HnSnapshot(nint window, long expectedLuid, string phase, Action<string, object> record)
    {
        long begin = Stopwatch.GetTimestamp();
        // CCD uses natural 4-byte packing; DWM_TIMING_INFO explicitly uses pack(1) in dwmapi.h.
        Assert.AreEqual(72, sizeof(HnPath)); Assert.AreEqual(48, sizeof(HnTarget)); Assert.AreEqual(64, sizeof(HnMode));
        Assert.AreEqual(84, sizeof(HnSourceName)); Assert.AreEqual(420, sizeof(HnTargetName)); Assert.AreEqual(104, sizeof(HnMonitor));
        nint monitor = MonitorFromWindow(window, 2);
        HnMonitor info = new() { Size = (uint)sizeof(HnMonitor) };
        int monitorResult = HnGetMonitorInfo(monitor, &info), monitorError = monitorResult == 0 ? Marshal.GetLastWin32Error() : 0;
        string deviceName = monitorResult != 0 ? new string(info.Device) : string.Empty;
        record("HN monitor", new { phase, begin, window = (long)window, monitor = (long)monitor, monitorResult, monitorError,
            deviceName, expectedLuid, monitorFlags = info.Flags, pathSize = sizeof(HnPath), modeSize = sizeof(HnMode), timingSize = sizeof(HnTiming) });
        foreach (uint flags in new uint[] { 2, 1 })
        {
            int sizes = GetDisplayConfigBufferSizes(flags, out uint paths, out uint modes);
            record("HN config sizes", new { phase, flags, sizes, paths, modes });
            if (sizes != 0 || paths > 4096 || modes > 16384) continue;
            HnPath[] pathBuffer = new HnPath[paths]; HnMode[] modeBuffer = new HnMode[modes];
            int query;
            fixed (HnPath* p = pathBuffer) fixed (HnMode* m = modeBuffer) query = QueryDisplayConfig(flags, ref paths, p, ref modes, m, 0);
            record("HN config query", new { phase, flags, query, paths, modes });
            if (query != 0) continue;
            for (int i = 0; i < paths; i++)
            {
                HnPath p = pathBuffer[i];
                HnSourceName source = new() { Header = new() { Type = 1, Size = (uint)sizeof(HnSourceName), Adapter = p.Source.Adapter, Id = p.Source.Id } };
                HnTargetName target = new() { Header = new() { Type = 2, Size = (uint)sizeof(HnTargetName), Adapter = p.Target.Adapter, Id = p.Target.Id } };
                int sourceResult = DisplayConfigGetDeviceInfo(&source.Header), targetResult = DisplayConfigGetDeviceInfo(&target.Header);
                string sourceName = sourceResult == 0 ? new string(source.Name) : string.Empty;
                record("HN path", new { phase, flags, index = i, pathFlags = p.Flags, active = (p.Flags & 1) != 0,
                    sourceLuid = p.Source.Adapter.Value, sourceId = p.Source.Id, sourceMode = p.Source.ModeIndex, sourceStatus = p.Source.Status,
                    targetLuid = p.Target.Adapter.Value, targetId = p.Target.Id, targetMode = p.Target.ModeIndex, targetStatus = p.Target.Status,
                    p.Target.Available, p.Target.Technology, p.Target.Rotation, p.Target.Scaling, p.Target.Scanline,
                    refreshNumerator = p.Target.Refresh.Numerator, refreshDenominator = p.Target.Refresh.Denominator,
                    sourceResult, sourceName, targetResult, targetFlags = target.Flags,
                    friendly = targetResult == 0 ? new string(target.Friendly) : null, targetPath = targetResult == 0 ? new string(target.Path) : null,
                    matchesWindowDevice = sourceResult == 0 && monitorResult != 0 && sourceName == deviceName,
                    matchesD3dLuid = p.Source.Adapter.Value == expectedLuid });
            }
            for (int i = 0; i < modes; i++)
            {
                HnMode mode = modeBuffer[i];
                record("HN mode", new { phase, flags, index = i, type = mode.Type, id = mode.Id, luid = mode.Adapter.Value,
                    sourceModeApplicable = mode.Type == 1, width = mode.Type == 1 ? mode.Width : 0,
                    height = mode.Type == 1 ? mode.Height : 0, pixelFormat = mode.Type == 1 ? mode.PixelFormat : 0,
                    x = mode.Type == 1 ? mode.X : 0, y = mode.Type == 1 ? mode.Y : 0 });
            }
        }
        HnDisplayDevice display = new() { Size = (uint)sizeof(HnDisplayDevice) };
        int enumerated = HnEnumDevices(deviceName, 0, &display, 1);
        record("HN display device", new { phase, enumerated, flags = display.Flags, name = new string(display.Name), description = new string(display.Description), id = new string(display.Id) });
        for (uint i = 0; i < 32; i++)
        {
            display = new() { Size = (uint)sizeof(HnDisplayDevice) };
            if (HnEnumDevices(null, i, &display, 0) == 0) break;
            record("HN adapter device", new { phase, index = i, flags = display.Flags, name = new string(display.Name), description = new string(display.Description), id = new string(display.Id) });
        }
        // DEVMODEW: dmSize at 68, dmFields at 72; display union at 76; total SDK size 220.
        byte* dm = stackalloc byte[220]; new Span<byte>(dm, 220).Clear(); *(ushort*)(dm + 68) = 220;
        int currentMode = HnEnumMode(deviceName, -1, dm, 0);
        record("HN current mode", new { phase, currentMode, size = *(ushort*)(dm + 68), fields = *(uint*)(dm + 72),
            x = *(int*)(dm + 76), y = *(int*)(dm + 80), bits = *(uint*)(dm + 168), width = *(uint*)(dm + 172), height = *(uint*)(dm + 176), frequency = *(uint*)(dm + 184) });
        uint session = (uint)Process.GetCurrentProcess().SessionId;
        foreach (int kind in new int[] { 8, 16, 25 })
        {
            int result = WTSQuerySessionInformationW(0, session, kind, out nint buffer, out uint bytes);
            int error = result == 0 ? Marshal.GetLastWin32Error() : 0;
            try
            {
                // WTSINFOEXW has DWORD Level then an 8-byte aligned union; level 1 flags at offset 16.
                record("HN WTS", new { phase, session, kind, result, error, bytes,
                    state = result != 0 && kind == 8 && bytes >= 4 ? *(int*)buffer : -1,
                    protocol = result != 0 && kind == 16 && bytes >= 2 ? *(ushort*)buffer : -1,
                    level = result != 0 && kind == 25 && bytes >= 4 ? *(uint*)buffer : 0,
                    sessionFlags = result != 0 && kind == 25 && bytes >= 20 && *(uint*)buffer == 1 ? *(int*)((byte*)buffer + 16) : -1 });
            }
            finally { if (buffer != 0) WTSFreeMemory(buffer); }
        }
        nint input = OpenInputDesktop(0, 0, 1); int inputError = input == 0 ? Marshal.GetLastWin32Error() : 0;
        try { record("HN desktop", new { phase, inputError, inputAvailable = input != 0, inputName = input != 0 ? HiObjectName(input) : null,
            currentName = HiObjectName(GetThreadDesktop(GetCurrentThreadId())) }); }
        finally { if (input != 0) CloseDesktop(input); }
        int saverResult = HnSpi(0x72, 0, out int saver, 0), saverError = saverResult == 0 ? Marshal.GetLastWin32Error() : 0;
        int cloakResult = DwmGetWindowAttribute(window, 14, out uint cloak, 4);
        HnTiming timing = new() { Size = (uint)sizeof(HnTiming) };
        int timingResult = DwmGetCompositionTimingInfo(0, &timing);
        record("HN DWM session boundary", new { phase, saverResult, saverError, saver, cloakResult, cloak,
            exStyle = HiWindowLong(window, -20), noRedirectionBitmap = (HiWindowLong(window, -20) & 0x200000) != 0,
            timingResult, timingSize = timing.Size, timing.QpcVBlank, timing.QpcCompose, timing.QpcRefreshPeriod,
            timing.Frame, timing.FrameSubmitted, timing.FrameConfirmed, timing.FrameDisplayed, timing.QpcFrameDisplayed,
            timing.FrameComplete, timing.QpcFrameComplete, timing.FramesOutstanding,
            displayPower = "Unavailable: no synchronous read-only current display-power query used; notifications are not a current-state query",
            timingScope = "NULL HWND global composition timing; not proof of this window consumption", end = Stopwatch.GetTimestamp() });
    }
}
#endif
