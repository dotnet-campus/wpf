using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe class ColorContextExports
{
    private const int InvalidArgument = unchecked((int)0x80070057);

    [UnmanagedCallersOnly(EntryPoint = "IWICColorContext_GetProfileBytes_Proxy", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetProfileBytes(nint context, uint size, byte* buffer, uint* actual)
    {
        if (context == 0 || actual == null) return InvalidArgument;
        return ((delegate* unmanaged[Stdcall]<nint, uint, byte*, uint*, int>)(*(void***)context)[7])(context, size, buffer, actual);
    }

    [UnmanagedCallersOnly(EntryPoint = "IWICColorContext_GetType_Proxy", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetType(nint context, uint* type)
    {
        if (context == 0 || type == null) return InvalidArgument;
        return ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)context)[6])(context, type);
    }

    [UnmanagedCallersOnly(EntryPoint = "IWICColorContext_GetExifColorSpace_Proxy", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetExifColorSpace(nint context, uint* value)
    {
        if (context == 0 || value == null) return InvalidArgument;
        return ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)context)[8])(context, value);
    }
}
