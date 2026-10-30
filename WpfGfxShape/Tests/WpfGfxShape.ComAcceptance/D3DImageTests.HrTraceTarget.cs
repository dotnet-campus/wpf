#if HP_GRAPHICS_TRACE
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [StructLayout(LayoutKind.Explicit, Size = 72)]
    private struct HrPath
    {
        [FieldOffset(0)] internal long SourceAdapter;
        [FieldOffset(8)] internal uint SourceId;
        [FieldOffset(20)] internal long TargetAdapter;
        [FieldOffset(28)] internal uint TargetId;
        [FieldOffset(60)] internal int Available;
        [FieldOffset(68)] internal uint Flags;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
    private struct HrSourceName
    {
        internal uint Type, Size;
        internal long Adapter;
        internal uint Id;
        internal fixed char Name[32];
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct HrMonitor
    {
        internal uint Size;
        internal fixed int Rectangles[8];
        internal uint Flags;
        internal fixed char Name[32];
    }
    [DllImport("user32.dll", EntryPoint = "GetDisplayConfigBufferSizes")]
    private static extern int HrConfigSizes(uint flags, out uint paths, out uint modes);
    [DllImport("user32.dll", EntryPoint = "QueryDisplayConfig")]
    private static extern int HrConfig(uint flags, ref uint paths, HrPath* path, ref uint modes, byte* mode, nint topology);
    [DllImport("user32.dll", EntryPoint = "DisplayConfigGetDeviceInfo")]
    private static extern int HrSource(HrSourceName* name);
    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", SetLastError = true)]
    private static extern int HrMonitorInfo(nint monitor, HrMonitor* info);

    internal void HrRunWithWindowOwner()
    {
        if (windowThread is not null) throw new InvalidOperationException("HR requires its own existing fixture owner lifecycle");
        windowThread = new PresentationWindowThread();
        try { WhenHrFixedFirstRedRunsThenOriginalContractIsRecorded(); }
        finally { StopPresentationThread(); }
    }

    private static void HrTopology(nint window, long luid, Action<string, object> record)
    {
        HrMonitor monitor = new() { Size = (uint)sizeof(HrMonitor) };
        int monitorStatus = HrMonitorInfo(MonitorFromWindow(window, 2), &monitor);
        string monitorName = monitorStatus == 0 ? "" : new string(monitor.Name);
        int sizes = HrConfigSizes(2, out uint paths, out uint modes);
        record("HR topology sizing", new { sizes, paths, modes, monitorStatus, monitorName, luid, qpc = Stopwatch.GetTimestamp() });
        if (sizes != 0 || paths > 4096 || modes > 16384) return;
        HrPath[] values = new HrPath[paths];
        byte[] modeValues = new byte[checked(modes * 64)];
        fixed (HrPath* p = values)
        fixed (byte* m = modeValues)
        {
            int status = HrConfig(2, ref paths, p, ref modes, m, 0);
            record("HR topology read", new { status, paths, modes });
            if (status != 0) return;
            for (int i = 0; i < paths; i++)
            {
                HrPath path = p[i];
                HrSourceName source = new() { Type = 1, Size = (uint)sizeof(HrSourceName), Adapter = path.SourceAdapter, Id = path.SourceId };
                int sourceStatus = HrSource(&source);
                string sourceName = sourceStatus == 0 ? new string(source.Name) : "";
                record("HR active path", new { sourceStatus, sourceName, path.SourceAdapter, path.SourceId, path.TargetAdapter, path.TargetId,
                    path.Available, path.Flags, exactTarget = sourceStatus == 0 && monitorStatus != 0 && path.SourceAdapter == luid && sourceName == monitorName,
                    meaning = "configured active path only; not target scanout", qpc = Stopwatch.GetTimestamp() });
            }
        }
    }
}
#endif
