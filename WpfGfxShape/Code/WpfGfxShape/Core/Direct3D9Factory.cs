using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal static unsafe class Direct3D9Factory
{
    internal const int NotAvailableHResult = unchecked((int) 0x8876086A);
    internal const int InvalidCallHResult = unchecked((int) 0x8876086C);
    internal const int DeviceLostHResult = unchecked((int) 0x88760868);
    internal const int DeviceHungHResult = unchecked((int) 0x88760874);
    internal const int DeviceRemovedHResult = unchecked((int) 0x88760870);
    internal const int PresentModeChangedHResult = 0x08760877;
    internal const int PresentOccludedHResult = 0x08760878;

    internal static Direct3D9Objects Create()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Direct3D 9 is only available on Windows.");
        }

        SafeHandle moduleHandle = DirectXSystemModule.Load(DirectXModule.Direct3D9);
        IDirect3D9* direct3D = null;
        IDirect3D9Ex* direct3DEx = null;

        try
        {
            nint module = moduleHandle.DangerousGetHandle();
            delegate* unmanaged[Stdcall]<uint, IDirect3D9*> create9 =
                (delegate* unmanaged[Stdcall]<uint, IDirect3D9*>) NativeLibrary.GetExport(
                    module,
                    DirectXModuleInfo.Direct3DCreate9EntryPoint);

            if (NativeLibrary.TryGetExport(module, DirectXModuleInfo.Direct3DCreate9ExEntryPoint, out nint create9ExAddress))
            {
                delegate* unmanaged[Stdcall]<uint, IDirect3D9Ex**, int> create9Ex =
                    (delegate* unmanaged[Stdcall]<uint, IDirect3D9Ex**, int>) create9ExAddress;
                int result = create9Ex(DirectXModuleInfo.Direct3D9SdkVersion, &direct3DEx);
                if (result != NotAvailableHResult)
                {
                    Marshal.ThrowExceptionForHR(result);
                }

                if (direct3DEx is not null)
                {
                    QueryDirect3D9(direct3DEx, &direct3D);
                }
            }

            if (direct3D is null)
            {
                direct3D = create9(DirectXModuleInfo.Direct3D9SdkVersion);
            }
            if (direct3D is null)
            {
                throw new InvalidOperationException("Direct3DCreate9 returned a null interface pointer.");
            }

            Direct3D9Objects objects = new(moduleHandle, direct3D, direct3DEx);
            moduleHandle = null!;
            direct3D = null;
            direct3DEx = null;
            return objects;
        }
        finally
        {
            Release(direct3DEx);
            Release(direct3D);
            moduleHandle?.Dispose();
        }
    }

    internal static void Release(IDirect3D9* direct3D)
    {
        if (direct3D is not null)
        {
            Release((void***) direct3D);
        }
    }

    internal static void Release(IDirect3D9Ex* direct3DEx)
    {
        if (direct3DEx is not null)
        {
            Release((void***) direct3DEx);
        }
    }

    internal static void Release(IDirect3DDevice9* device)
    {
        if (device is not null)
        {
            Release((void***) device);
        }
    }

    internal static void Release(IDirect3DDevice9Ex* device)
    {
        if (device is not null)
        {
            Release((void***) device);
        }
    }

    internal static void Release(IDirect3DSurface9* surface)
    {
        if (surface is not null)
        {
            Release((void***) surface);
        }
    }

    internal static void Release(IDirect3DSwapChain9* swapChain)
    {
        if (swapChain is not null)
        {
            Release((void***) swapChain);
        }
    }

    private static void QueryDirect3D9(IDirect3D9Ex* direct3DEx, IDirect3D9** direct3D)
    {
        Guid interfaceId = IDirect3D9.Guid;
        void** vtable = direct3DEx->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9Ex*, Guid*, void**, int> queryInterface =
            (delegate* unmanaged[Stdcall]<IDirect3D9Ex*, Guid*, void**, int>) vtable[0];
        int result = queryInterface(direct3DEx, &interfaceId, (void**) direct3D);
        if (result < 0)
        {
            *direct3D = null;
        }
    }

    private static void Release(void*** instance)
    {
        void** vtable = *instance;
        delegate* unmanaged[Stdcall]<void***, uint> release =
            (delegate* unmanaged[Stdcall]<void***, uint>) vtable[2];
        _ = release(instance);
    }
}
