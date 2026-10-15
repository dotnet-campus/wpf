using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static class PerformanceInstrumentation
{
    private static uint _flags;
    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessageW(nint window, uint message, nuint wparam, nint lparam);

    [UnmanagedCallersOnly(EntryPoint = "SetMilPerfInstrumentationFlags", CallConvs = [typeof(CallConvStdcall)])]
    private static void SetFlags(uint flags) => Volatile.Write(ref _flags, flags);

    internal static int Presented(nint window, int result, bool signalPresent)
    {
        if (signalPresent && result >= 0 && (Volatile.Read(ref _flags) & 2) != 0)
            PostMessageW(window, 0x400, 123, 456);
        return result;
    }
}
