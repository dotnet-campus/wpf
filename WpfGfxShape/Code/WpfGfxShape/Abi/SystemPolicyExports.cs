using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class SystemPolicyExports
{
    private static long _elementId;

    [UnmanagedCallersOnly(EntryPoint = "RenderOptions_ForceSoftwareRenderingModeForProcess", CallConvs = [typeof(CallConvStdcall)])]
    private static void ForceSoftware(int value) => ProcessRenderPolicy.ForceSoftware = value != 0;

    [UnmanagedCallersOnly(EntryPoint = "RenderOptions_IsSoftwareRenderingForcedForProcess", CallConvs = [typeof(CallConvStdcall)])]
    private static int IsSoftwareForced() => ProcessRenderPolicy.ForceSoftware ? 1 : 0;

    [UnmanagedCallersOnly(EntryPoint = "RenderOptions_EnableHardwareAccelerationInRdp", CallConvs = [typeof(CallConvStdcall)])]
    private static void EnableHardwareInRdp(int value) => ProcessRenderPolicy.HardwareInRdp = value != 0;

    [UnmanagedCallersOnly(EntryPoint = "MILUpdateSystemParametersInfo", CallConvs = [typeof(CallConvStdcall)])]
    private static int UpdateSystemParameters() { ProcessRenderPolicy.ScheduleUpdate(); return 0; }

    [UnmanagedCallersOnly(EntryPoint = "WgxConnection_ShouldForceSoftwareForGraphicsStreamClient", CallConvs = [typeof(CallConvStdcall)])]
    private static int ShouldForceSoftwareForGraphicsStreamClient()
    {
        // Graphics stream clients force software only on Vista, not on Windows 7 and later.
        if (!OperatingSystem.IsWindowsVersionAtLeast(6) || OperatingSystem.IsWindowsVersionAtLeast(6, 1)) return 0;
        return GetGraphicsStreamClient(0, out _) == 0 ? 1 : 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "GetNextPerfElementId", CallConvs = [typeof(CallConvStdcall)])]
    private static long GetNextElementId() => Interlocked.Increment(ref _elementId);

    [UnmanagedCallersOnly(EntryPoint = "MILLoadResource", CallConvs = [typeof(CallConvStdcall)])]
    private static int LoadImageResource(char* source, byte** memory, int* size)
    {
        char* type = stackalloc char[6] { 'I', 'M', 'A', 'G', 'E', '\0' };
        nint resource = FindResourceW(0, source, type);
        if (resource == 0) return Direct3D9Factory.GenericFailureHResult;
        nint loaded = LoadResource(0, resource);
        if (loaded == 0) return Direct3D9Factory.GenericFailureHResult;
        *memory = (byte*)LockResource(loaded);
        *size = unchecked((int)SizeofResource(0, resource));
        return 0;
    }

    private static int GetGraphicsStreamClient(uint index, out Guid id)
    {
        id = default;
        if (!NativeLibrary.TryLoad("dwmapi.dll", typeof(SystemPolicyExports).Assembly,
                DllImportSearchPath.System32, out nint module))
            return unchecked((int)0x8007007E);

        try
        {
            if (!NativeLibrary.TryGetExport(module, "DwmGetGraphicsStreamClient", out nint address))
                return unchecked((int)0x8007007F);

            Guid clientId = default;
            int result = ((delegate* unmanaged[Stdcall]<uint, Guid*, int>)address)(index, &clientId);
            id = clientId;
            return result;
        }
        finally
        {
            NativeLibrary.Free(module);
        }
    }
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern nint FindResourceW(nint module, char* name, char* type);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern nint LoadResource(nint module, nint resource);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern nint LockResource(nint resource);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern uint SizeofResource(nint module, nint resource);
}
