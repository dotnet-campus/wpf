using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using Windows.Win32;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9Objects : IDisposable
{
    private SafeHandle? _moduleHandle;
    private IDirect3D9* _direct3D;
    private IDirect3D9Ex* _direct3DEx;

    internal Direct3D9Objects(
        SafeHandle moduleHandle,
        IDirect3D9* direct3D,
        IDirect3D9Ex* direct3DEx)
    {
        ArgumentNullException.ThrowIfNull(moduleHandle);
        _moduleHandle = moduleHandle;
        _direct3D = direct3D;
        _direct3DEx = direct3DEx;
    }

    internal IDirect3D9* Direct3D => _direct3D;

    internal IDirect3D9Ex* Direct3DEx => _direct3DEx;

    internal uint GetAdapterCount()
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);

        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint> getAdapterCount =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint>) vtable[4];
        return getAdapterCount(_direct3D);
    }

    internal Direct3D9AdapterCapabilities GetAdapterCapabilities(
        uint adapterOrdinal,
        Devtype deviceType = Devtype.Hal)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);

        Displaymode displayMode = default;
        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Displaymode*, int> getAdapterDisplayMode =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Displaymode*, int>) vtable[8];
        int result = getAdapterDisplayMode(_direct3D, adapterOrdinal, &displayMode);
        Marshal.ThrowExceptionForHR(result);

        Caps9 caps = default;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Caps9*, int> getDeviceCaps =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Caps9*, int>) vtable[14];
        result = getDeviceCaps(_direct3D, adapterOrdinal, deviceType, &caps);
        Marshal.ThrowExceptionForHR(result);

        return new Direct3D9AdapterCapabilities(adapterOrdinal, deviceType, displayMode, caps);
    }

    internal Direct3D9Device CreateDevice(
        uint adapterOrdinal = D3D9.AdapterDefault,
        Devtype deviceType = Devtype.Hal)
    {
        Direct3D9AdapterCapabilities capabilities = GetAdapterCapabilities(adapterOrdinal, deviceType);
        nint focusWindow = (nint) PInvoke.GetDesktopWindow().Value;
        uint behaviorFlags = unchecked((uint) (D3D9.CreateFpuPreserve
            | D3D9.CreateMultithreaded
            | D3D9.CreateDisableDriverManagementEX));
        behaviorFlags |= unchecked((uint) ((capabilities.Caps.DevCaps & D3D9.DevcapsHwtransformandlight) != 0
            ? D3D9.CreateHardwareVertexprocessing
            : D3D9.CreateSoftwareVertexprocessing));
        PresentParameters presentParameters = new(
            backBufferWidth: 1,
            backBufferHeight: 1,
            backBufferFormat: Format.X8R8G8B8,
            backBufferCount: 1,
            swapEffect: Swapeffect.Discard,
            windowed: true,
            enableAutoDepthStencil: false,
            autoDepthStencilFormat: Format.Unknown);

        IDirect3DDevice9* device = null;
        int result = CreateDevice(
            adapterOrdinal,
            deviceType,
            focusWindow,
            behaviorFlags,
            &presentParameters,
            &device);
        if (result == Direct3D9Factory.InvalidCallHResult
            && (behaviorFlags & D3D9.CreateDisableDriverManagementEX) != 0)
        {
            behaviorFlags &= unchecked((uint) ~D3D9.CreateDisableDriverManagementEX);
            result = CreateDevice(
                adapterOrdinal,
                deviceType,
                focusWindow,
                behaviorFlags,
                &presentParameters,
                &device);
        }

        if (result < 0)
        {
            Direct3D9Factory.Release(device);
            Marshal.ThrowExceptionForHR(result);
        }
        if (device is null)
        {
            throw new InvalidOperationException("Direct3D device creation returned a null interface pointer.");
        }

        IDirect3DDevice9Ex* deviceEx = null;
        QueryDevice9Ex(device, &deviceEx);

        return new Direct3D9Device(device, deviceEx, adapterOrdinal, deviceType, behaviorFlags, presentParameters);
    }

    private int CreateDevice(
        uint adapterOrdinal,
        Devtype deviceType,
        nint focusWindow,
        uint behaviorFlags,
        PresentParameters* presentParameters,
        IDirect3DDevice9** device)
    {
        *device = null;
        if (_direct3DEx is not null)
        {
            IDirect3DDevice9Ex* deviceEx = null;
            void** vtable = _direct3DEx->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Devtype, nint, uint, PresentParameters*, Displaymodeex*, IDirect3DDevice9Ex**, int> createDeviceEx =
                (delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Devtype, nint, uint, PresentParameters*, Displaymodeex*, IDirect3DDevice9Ex**, int>) vtable[20];
            int result = createDeviceEx(
                _direct3DEx,
                adapterOrdinal,
                deviceType,
                focusWindow,
                behaviorFlags,
                presentParameters,
                null,
                &deviceEx);
            if (result >= 0)
            {
                QueryDevice9(deviceEx, device);
                Direct3D9Factory.Release((IDirect3DDevice9*) deviceEx);
                return *device is null ? Direct3D9Factory.InvalidCallHResult : result;
            }

            Direct3D9Factory.Release((IDirect3DDevice9*) deviceEx);
            return result;
        }

        void** baseVtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, nint, uint, PresentParameters*, IDirect3DDevice9**, int> createDevice =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, nint, uint, PresentParameters*, IDirect3DDevice9**, int>) baseVtable[16];
        return createDevice(
            _direct3D,
            adapterOrdinal,
            deviceType,
            focusWindow,
            behaviorFlags,
            presentParameters,
            device);
    }

    private static void QueryDevice9(IDirect3DDevice9Ex* deviceEx, IDirect3DDevice9** device)
    {
        Guid interfaceId = IDirect3DDevice9.Guid;
        void** vtable = deviceEx->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, Guid*, void**, int> queryInterface =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, Guid*, void**, int>) vtable[0];
        int result = queryInterface(deviceEx, &interfaceId, (void**) device);
        if (result < 0)
        {
            *device = null;
        }
    }

    private static void QueryDevice9Ex(IDirect3DDevice9* device, IDirect3DDevice9Ex** deviceEx)
    {
        *deviceEx = null;
        Guid interfaceId = IDirect3DDevice9Ex.Guid;
        void** vtable = device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Guid*, void**, int> queryInterface =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Guid*, void**, int>) vtable[0];
        int result = queryInterface(device, &interfaceId, (void**) deviceEx);
        if (result < 0)
        {
            *deviceEx = null;
        }
    }

    public void Dispose()
    {
        Direct3D9Factory.Release(_direct3DEx);
        _direct3DEx = null;

        Direct3D9Factory.Release(_direct3D);
        _direct3D = null;

        _moduleHandle?.Dispose();
        _moduleHandle = null;
    }
}

internal readonly record struct Direct3D9AdapterCapabilities(
    uint AdapterOrdinal,
    Devtype DeviceType,
    Displaymode DisplayMode,
    Caps9 Caps);
