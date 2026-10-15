using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static class ProcessRenderPolicy
{
    private static readonly object SyncRoot = new();
    private static bool _forceSoftware;
    private static bool _hardwareInRdp;
    private static uint _generation;

    internal static bool ForceSoftware
    {
        get { lock (SyncRoot) return _forceSoftware; }
        set { lock (SyncRoot) { if (_forceSoftware != value) { _forceSoftware = value; _generation++; } } }
    }

    internal static bool HardwareInRdp
    {
        get { lock (SyncRoot) return _hardwareInRdp; }
        set { lock (SyncRoot) { if (_hardwareInRdp != value) { _hardwareInRdp = value; _generation++; } } }
    }

    internal static uint Generation { get { lock (SyncRoot) return _generation; } }
    internal static void ScheduleUpdate() { lock (SyncRoot) _generation++; }

    internal static bool ShouldUseSoftware()
    {
        lock (SyncRoot)
            return _forceSoftware || (!_hardwareInRdp && GetSystemMetrics(0x1000) != 0)
                || !Direct3D9RegistryDatabase.Create(1).IsAdapterEnabled(0);
    }

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int GetSystemMetrics(int index);
}
