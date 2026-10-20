using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe class RetailPlayerExports
{
    // Retail apifunc.cpp validates required pointers but does not instantiate the PRERELEASE player.
    [UnmanagedCallersOnly(EntryPoint = "MilPlayer_Create", CallConvs = [typeof(CallConvStdcall)])]
    private static int Create(nint* player) => player == null ? unchecked((int)0x80070057) : unchecked((int)0x80004001);

    [UnmanagedCallersOnly(EntryPoint = "MilPlayer_Process", CallConvs = [typeof(CallConvStdcall)])]
    private static int Process(nint player, byte* header, byte* rdpHeader, byte* data, uint size) =>
        player == 0 || header == null ? unchecked((int)0x80070057) : unchecked((int)0x80004001);
}
