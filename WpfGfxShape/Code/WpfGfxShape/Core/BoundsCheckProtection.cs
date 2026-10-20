using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal static class BoundsCheckProtection
{
    private static int _disabled;

    internal static bool Disabled => Volatile.Read(ref _disabled) != 0;

    // BOOL is four bytes even though the native switch stores a C++ bool.
    [UnmanagedCallersOnly(EntryPoint = "WpfGfx_SetDisableBoundsCheckProtection", CallConvs = [typeof(CallConvStdcall)])]
    private static void SetDisabled(int disabled) => Volatile.Write(ref _disabled, disabled != 0 ? 1 : 0);
}
