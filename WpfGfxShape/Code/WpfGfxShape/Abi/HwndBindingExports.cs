using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe class HwndBindingExports
{
    private const int AccessDenied = unchecked((int)0x80070005);
    private const int InvalidArgument = unchecked((int)0x80070057);
    private const int OutOfMemory = unchecked((int)0x8007000E);
    private static readonly HashSet<nint> Windows = [];
    private static readonly nint DwmModule = LoadDwm();
    private static readonly nint AttachHint = FindDwmExport("DwmAttachMilContent");
    private static readonly nint DetachHint = FindDwmExport("DwmDetachMilContent");

    private static nint LoadDwm()
    {
        if (!OperatingSystem.IsWindows()) return 0;
        // Use the system directory, never a caller-controlled DLL search path.
        return NativeLibrary.TryLoad(Path.Combine(Environment.SystemDirectory, "dwmapi.dll"), out nint module) ? module : 0;
    }

    private static nint FindDwmExport(string name) =>
        DwmModule != 0 && NativeLibrary.TryGetExport(DwmModule, name, out nint address) ? address : 0;

    [UnmanagedCallersOnly(EntryPoint = "MilVisualTarget_AttachToHwnd", CallConvs = [typeof(CallConvStdcall)])]
    private static int AttachVisualTarget(nint window)
    {
        lock (PartitionManagerExports.CompositionGate)
        {
            try { return Windows.Add(window) ? 0 : AccessDenied; }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilVisualTarget_DetachFromHwnd", CallConvs = [typeof(CallConvStdcall)])]
    private static int DetachVisualTarget(nint window)
    {
        lock (PartitionManagerExports.CompositionGate)
            return Windows.Remove(window) ? 0 : InvalidArgument;
    }

    [UnmanagedCallersOnly(EntryPoint = "MilContent_AttachToHwnd", CallConvs = [typeof(CallConvStdcall)])]
    private static int AttachContent(nint window)
    {
        // Native content attachment is a best-effort DWM hint, not target ownership.
        if (AttachHint != 0) _ = ((delegate* unmanaged[Stdcall]<nint, int>)AttachHint)(window);
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "MilContent_DetachFromHwnd", CallConvs = [typeof(CallConvStdcall)])]
    private static int DetachContent(nint window)
    {
        if (DetachHint != 0) _ = ((delegate* unmanaged[Stdcall]<nint, int>)DetachHint)(window);
        return 0;
    }
}
