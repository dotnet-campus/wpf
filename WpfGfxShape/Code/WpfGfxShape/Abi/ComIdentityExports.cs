using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe class ComIdentityExports
{
    [UnmanagedCallersOnly(EntryPoint = "MilVersionCheck", CallConvs = [typeof(CallConvStdcall)])]
    private static int VersionCheck(uint version) => version == 0x200184C0 ? 0 : unchecked((int)0x88982F0B);

    [UnmanagedCallersOnly(EntryPoint = "MILAddRef", CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[1])(value);

    [UnmanagedCallersOnly(EntryPoint = "MILRelease", CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);

    [UnmanagedCallersOnly(EntryPoint = "MILQueryInterface", CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryInterface(nint value, Guid* iid, nint* output)
    {
        if (value == 0 || output == null) return unchecked((int)0x80070057);
        *output = 0;
        return ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)value)[0])(value, iid, output);
    }
}
