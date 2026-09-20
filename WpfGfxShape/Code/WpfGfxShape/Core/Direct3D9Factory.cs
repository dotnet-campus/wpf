using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal static unsafe class Direct3D9Factory
{
    internal const int NotAvailableHResult = unchecked((int) 0x8876086A);
    internal const int NotFoundHResult = unchecked((int) 0x88760866);
    internal const int InvalidCallHResult = unchecked((int) 0x8876086C);
    internal const int WrongTextureFormatHResult = unchecked((int) 0x88760818);
    internal const int DriverInternalErrorHResult = unchecked((int) 0x88760827);
    internal const int OutOfVideoMemoryHResult = unchecked((int) 0x8876017C);
    internal const int OutOfMemoryHResult = unchecked((int) 0x8007000E);
    internal const int GenericFailureHResult = unchecked((int) 0x80004005);
    internal const int UnexpectedHResult = unchecked((int) 0x8000FFFF);
    internal const int InvalidArgumentHResult = unchecked((int) 0x80070057);
    internal const int NotImplementedHResult = unchecked((int) 0x80004001);
    internal const int NoInterfaceHResult = unchecked((int) 0x80004002);
    internal const int SuccessHResult = 0;
    internal const int UnsupportedTextureSizeHResult = unchecked((int) 0x8898000B);
    internal const int InternalErrorHResult = unchecked((int) 0x88980080);
    internal const int DeviceCannotRenderTextHResult = unchecked((int) 0x88980088);
    internal const int ClippedToEmptyHResult = 0x08980001;
    internal const int EmptyFillHResult = 0x08980002;
    internal const int NonInvertibleMatrixHResult = unchecked((int) 0x88980007);
    internal const int BadNumberHResult = unchecked((int) 0x8898000A);
    internal const int DeviceLostHResult = unchecked((int) 0x88760868);
    internal const int DeviceHungHResult = unchecked((int) 0x88760874);
    internal const int DeviceRemovedHResult = unchecked((int) 0x88760870);
    internal const int PresentModeChangedHResult = 0x08760877;
    internal const int PresentOccludedHResult = 0x08760878;
    internal const int InsufficientBufferHResult = unchecked((int) 0x88980002);
    internal const int ArithmeticOverflowHResult = unchecked((int) 0x80070216);
    internal const int DisplayStateInvalidHResult = unchecked((int) 0x88980006);
    internal const int DisplayFormatNotSupportedHResult = unchecked((int) 0x88980084);
    internal const int NoHardwareDeviceHResult = unchecked((int) 0x8898008D);
    internal const int NeedRecreateAndPresentHResult = unchecked((int) 0x8898008E);
    internal const int MaximumTextureSizeExceededHResult = unchecked((int) 0x8898009A);
    internal const int InsufficientGpuCapabilitiesHResult = unchecked((int) 0x8898009C);
    internal const int UnsupportedOperationHResult = unchecked((int) 0x88982F81);
    internal const int WgxInvalidCallHResult = unchecked((int) 0x88980085);
    internal const int UnsupportedPixelFormatHResult = unchecked((int) 0x88982F80);
    internal const int WinCodecInternalErrorHResult = unchecked((int) 0x88982F48);
    internal const int NotInitializedHResult = unchecked((int) 0x88982F0C);
    internal const int InvalidWindowHandleHResult = unchecked((int) 0x80070578);

    internal static bool IsOutOfMemory(int hresult)
    {
        return hresult is OutOfMemoryHResult
            or unchecked((int) 0x80070008)
            or unchecked((int) 0x800705AA)
            or unchecked((int) 0x800705AF)
            or unchecked((int) 0xD000009A)
            or unchecked((int) 0xD000012D)
            or unchecked((int) 0xD0000017)
            or unchecked((int) 0xD0000044);
    }

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

    internal static void AddRef(IDirect3D9* direct3D)
    {
        AddRef((void***) direct3D);
    }

    internal static void AddRef(nint instance)
    {
        AddRef((void***) instance);
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

    internal static void Release(IDirect3DQuery9* query)
    {
        if (query is not null)
        {
            Release((void***) query);
        }
    }

    internal static void Release(IDirect3DTexture9* texture)
    {
        if (texture is not null)
        {
            Release((void***) texture);
        }
    }

    internal static void Release(IDirect3DStateBlock9* stateBlock)
    {
        if (stateBlock is not null)
        {
            Release((void***) stateBlock);
        }
    }

    internal static void Release(IDirect3DVertexShader9* vertexShader)
    {
        if (vertexShader is not null)
        {
            Release((void***) vertexShader);
        }
    }

    internal static void Release(IDirect3DPixelShader9* pixelShader)
    {
        if (pixelShader is not null)
        {
            Release((void***) pixelShader);
        }
    }

    internal static void Release(IDirect3DVertexBuffer9* vertexBuffer)
    {
        if (vertexBuffer is not null)
        {
            Release((void***) vertexBuffer);
        }
    }

    internal static void Release(IDirect3DIndexBuffer9* indexBuffer)
    {
        if (indexBuffer is not null)
        {
            Release((void***) indexBuffer);
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

    internal static void Release(nint instance)
    {
        if (instance != 0)
        {
            Release((void***) instance);
        }
    }

    private static void AddRef(void*** instance)
    {
        if (instance is not null)
        {
            void** vtable = *instance;
            delegate* unmanaged[Stdcall]<void***, uint> addRef =
                (delegate* unmanaged[Stdcall]<void***, uint>) vtable[1];
            _ = addRef(instance);
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
