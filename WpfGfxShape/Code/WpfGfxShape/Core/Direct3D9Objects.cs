using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D9;
using Windows.Win32;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9AdapterCapabilities(
    uint AdapterOrdinal,
    Devtype DeviceType,
    Displaymode DisplayMode,
    Caps9 Caps);

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9Objects : IDisposable
{
    private SafeHandle? _moduleHandle;
    private Direct3D9SoftwareRasterizerLoader? _softwareRasterizerLoader;
    private IDirect3D9* _direct3D;
    private IDirect3D9Ex* _direct3DEx;

    internal Direct3D9Objects(
        SafeHandle moduleHandle,
        IDirect3D9* direct3D,
        IDirect3D9Ex* direct3DEx,
        Direct3D9SoftwareRasterizerLoader? softwareRasterizerLoader = null)
    {
        ArgumentNullException.ThrowIfNull(moduleHandle);
        _moduleHandle = moduleHandle;
        _softwareRasterizerLoader = softwareRasterizerLoader ?? new Direct3D9SoftwareRasterizerLoader();
        _direct3D = direct3D;
        _direct3DEx = direct3DEx;
    }

    internal IDirect3D9* Direct3D
    {
        get
        {
            ObjectDisposedException.ThrowIf(_direct3D is null, this);
            return _direct3D;
        }
    }

    internal IDirect3D9Ex* Direct3DEx
    {
        get
        {
            ObjectDisposedException.ThrowIf(_direct3D is null, this);
            return _direct3DEx;
        }
    }

    internal uint GetAdapterCount()
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint> getAdapterCount =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint>) vtable[4];
        return getAdapterCount(_direct3D);
    }

    internal nint GetAdapterMonitor(uint adapterOrdinal)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, nint> getAdapterMonitor =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, nint>) vtable[15];
        return getAdapterMonitor(_direct3D, adapterOrdinal);
    }

    internal int GetAdapterLuid(uint adapterOrdinal, out long adapterLuid)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        adapterLuid = 0;
        if (_direct3DEx is null)
        {
            return unchecked((int) 0x80004001);
        }

        Luid luid = default;
        void** vtable = _direct3DEx->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Luid*, int> getAdapterLuid =
            (delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Luid*, int>) vtable[21];
        int result = getAdapterLuid(_direct3DEx, adapterOrdinal, &luid);
        adapterLuid = ((long) luid.High << 32) | luid.Low;
        return result;
    }

    internal Displaymode GetAdapterDisplayMode(uint adapterOrdinal)
    {
        int result = GetAdapterDisplayMode(adapterOrdinal, out Direct3D9DisplayMode displayMode);
        Marshal.ThrowExceptionForHR(result);
        return new Displaymode(displayMode.Width, displayMode.Height, displayMode.RefreshRate, displayMode.Format);
    }

    internal int GetAdapterDisplayMode(uint adapterOrdinal, out Direct3D9DisplayMode displayMode)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);

        Displaymode nativeDisplayMode = default;
        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Displaymode*, int> getAdapterDisplayMode =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Displaymode*, int>) vtable[8];
        int result = getAdapterDisplayMode(_direct3D, adapterOrdinal, &nativeDisplayMode);
        displayMode = result < 0
            ? default
            : new Direct3D9DisplayMode(
                (uint) Marshal.OffsetOf<Displaymodeex>(nameof(Displaymodeex.ScanLineOrdering)),
                nativeDisplayMode.Width,
                nativeDisplayMode.Height,
                nativeDisplayMode.RefreshRate,
                nativeDisplayMode.Format,
                Scanlineordering.Unknown);
        return result;
    }

    internal int GetAdapterDisplayModeEx(
        uint adapterOrdinal,
        out Direct3D9DisplayMode displayMode,
        out Displayrotation displayRotation)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        displayMode = default;
        displayRotation = 0;
        Displayrotation nativeDisplayRotation = 0;
        if (_direct3DEx is null)
        {
            return unchecked((int) 0x80004001);
        }

        Displaymodeex nativeDisplayMode = default;
        nativeDisplayMode.Size = (uint) sizeof(Displaymodeex);
        void** vtable = _direct3DEx->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Displaymodeex*, Displayrotation*, int> getAdapterDisplayModeEx =
            (delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Displaymodeex*, Displayrotation*, int>) vtable[19];
        int result = getAdapterDisplayModeEx(_direct3DEx, adapterOrdinal, &nativeDisplayMode, &nativeDisplayRotation);
        displayRotation = nativeDisplayRotation;
        if (result >= 0)
        {
            displayMode = new Direct3D9DisplayMode(
                nativeDisplayMode.Size,
                nativeDisplayMode.Width,
                nativeDisplayMode.Height,
                nativeDisplayMode.RefreshRate,
                nativeDisplayMode.Format,
                nativeDisplayMode.ScanLineOrdering);
        }

        return result;
    }

    internal Direct3D9AdapterCapabilities GetAdapterCapabilities(uint adapterOrdinal, Devtype deviceType = Devtype.Hal)
    {
        Displaymode displayMode = GetAdapterDisplayMode(adapterOrdinal);
        int result = GetDeviceCaps(adapterOrdinal, deviceType, out Caps9 caps);
        Marshal.ThrowExceptionForHR(result);

        return new Direct3D9AdapterCapabilities(adapterOrdinal, deviceType, displayMode, caps);
    }

    internal int GetDeviceCaps(uint adapterOrdinal, Devtype deviceType, out Caps9 caps)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        Caps9 nativeCaps = default;
        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Caps9*, int> getDeviceCaps =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Caps9*, int>) vtable[14];
        int result = getDeviceCaps(_direct3D, adapterOrdinal, deviceType, &nativeCaps);
        caps = nativeCaps;
        return result;
    }

    internal int GetAdapterIdentifier(uint adapterOrdinal, out AdapterIdentifier9 identifier)
    {
        const uint noDriverVersion = 0x00000004;

        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        AdapterIdentifier9 nativeIdentifier = default;
        int result = unchecked((int) 0x80004001);
        if (_direct3DEx is not null)
        {
            void** exVtable = _direct3DEx->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, uint, AdapterIdentifier9*, int> getExAdapterIdentifier =
                (delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, uint, AdapterIdentifier9*, int>) exVtable[5];
            result = getExAdapterIdentifier(_direct3DEx, adapterOrdinal, noDriverVersion, &nativeIdentifier);
        }

        if (_direct3DEx is null || result < 0)
        {
            nativeIdentifier = default;
            void** vtable = _direct3D->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3D9*, uint, uint, AdapterIdentifier9*, int> getAdapterIdentifier =
                (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, uint, AdapterIdentifier9*, int>) vtable[5];
            result = getAdapterIdentifier(_direct3D, adapterOrdinal, 0, &nativeIdentifier);
        }

        identifier = nativeIdentifier;
        return result;
    }

    internal int CheckWindowedDisplayFormat(uint adapterOrdinal, Format displayFormat)
    {
        return CheckDeviceType(adapterOrdinal, Devtype.Hal, displayFormat, Format.X8R8G8B8);
    }

    internal Direct3D9Device CreateDevice(uint adapterOrdinal = D3D9.AdapterDefault, Devtype deviceType = Devtype.Hal)
    {
        Direct3D9AdapterCapabilities capabilities = GetAdapterCapabilities(adapterOrdinal, deviceType);
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

        return CreateDevice(
            new Direct3D9DeviceCreationParameters(
                adapterOrdinal,
                deviceType,
                behaviorFlags,
                presentParameters,
                DisplayMode: capabilities.DisplayMode,
                Capabilities: capabilities.Caps),
            unusableNotification: null,
            disposedNotification: null);
    }

    internal Direct3D9DeviceManager CreateDeviceManager(Direct3D9DisplaySetManager displaySetManager)
    {
        ArgumentNullException.ThrowIfNull(displaySetManager);
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        Direct3D9RegistryDatabase registryDatabase = Direct3D9RegistryDatabase.Create(GetAdapterCount());
        return new Direct3D9DeviceManager(
            CreateManagedDevice,
            GetAdapterCount,
            null,
            CheckDeviceType,
            registryDatabase.IsAdapterEnabled,
            registryDatabase.DisableAdapter,
            registryDatabase.HandleAdapterUnexpectedError,
            testLevel1Device: Direct3D9Level1DeviceTest.Test,
            checkDeviceFormat: CheckDeviceFormat,
            maximumMultisampleTypeProvider: Direct3D9MultisampleSupportFactory.ReadConfiguredMaximum,
            checkDeviceMultisampleType: CheckDeviceMultisampleType,
            initializeDynamicBuffers: device => device.InitializeDynamicBuffers(),
            initializeRenderState: device => Direct3D9DefaultState.Initialize(
                device.Capabilities,
                device.ForceSetRenderState,
                device.ForceSetTextureStageState,
                device.ForceSetSamplerState,
                device.ForceSetTransform,
                device.SetMaterial,
                stage => device.ForceSetTexture(stage, null),
                () => device.ForceSetPixelShader(null),
                () => device.ForceSetStreamSource(null, 0),
                stream => device.SetStreamSource(stream, null, 0, 0),
                () => device.ForceSetIndices(null),
                device.ResetScissorAndClipCache,
                () => device.ForceSetDepthStencilSurface(null, 0, 0)),
            initializeTextPixelShaders: device => device.InitializeTextPixelShadersFromResources(),
            ensureSoftwareRasterizerRegistered: () => displaySetManager
                .DangerousGetLatestDisplaySet()
                .EnsureSoftwareRasterizerRegistered(_softwareRasterizerLoader!, RegisterSoftwareDevice),
            latestDisplaySetProvider: displaySetManager.DangerousGetLatestDisplaySet,
            adapterDisplayModeProbe: adapterOrdinal => GetAdapterDisplayMode(adapterOrdinal, out _),
            getDeviceCaps: GetDeviceCaps,
            checkRenderTargetFormat: (device, format) => device.CheckRenderTargetFormat(format, _ => 0, out _));
    }

    private int RegisterSoftwareDevice(nint getSoftwareInfo)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, nint, int> registerSoftwareDevice =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, nint, int>) vtable[15];
        return registerSoftwareDevice(_direct3D, getSoftwareInfo);
    }

    private int CheckDeviceType(
        uint adapterOrdinal,
        Devtype deviceType,
        Format displayFormat,
        Format targetFormat)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, Format, int, int> checkDeviceType =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, Format, int, int>) vtable[9];
        return checkDeviceType(_direct3D, adapterOrdinal, deviceType, displayFormat, targetFormat, 1);
    }

    private int CheckDeviceFormat(
        uint adapterOrdinal,
        Devtype deviceType,
        Format adapterFormat,
        uint usage,
        Resourcetype resourceType,
        Format checkedFormat)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, uint, Resourcetype, Format, int> checkDeviceFormat =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, uint, Resourcetype, Format, int>) vtable[10];
        return checkDeviceFormat(
            _direct3D,
            adapterOrdinal,
            deviceType,
            adapterFormat,
            usage,
            resourceType,
            checkedFormat);
    }

    private int CheckDeviceMultisampleType(
        uint adapterOrdinal,
        Devtype deviceType,
        Format surfaceFormat,
        bool windowed,
        MultisampleType multisampleType)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);
        void** vtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, int, MultisampleType, uint*, int> checkDeviceMultisampleType =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, int, MultisampleType, uint*, int>) vtable[11];
        return checkDeviceMultisampleType(
            _direct3D,
            adapterOrdinal,
            deviceType,
            surfaceFormat,
            windowed ? 1 : 0,
            multisampleType,
            null);
    }

    private Direct3D9Device CreateManagedDevice(
        Direct3D9DeviceCreationParameters creationParameters,
        Action<Direct3D9Device> unusableNotification,
        Action<Direct3D9Device> disposedNotification)
    {
        return CreateDevice(creationParameters, unusableNotification, disposedNotification);
    }

    private Direct3D9Device CreateDevice(
        Direct3D9DeviceCreationParameters creationParameters,
        Action<Direct3D9Device>? unusableNotification,
        Action<Direct3D9Device>? disposedNotification)
    {
        ObjectDisposedException.ThrowIf(_direct3D is null, this);

        uint behaviorFlags = creationParameters.BehaviorFlags;
        PresentParameters presentParameters = creationParameters.PresentParameters;
        nint focusWindow = creationParameters.FocusWindow != 0
            ? creationParameters.FocusWindow
            : (nint) PInvoke.GetDesktopWindow().Value;
        IDirect3DDevice9* device = null;
        int result = CreateDevice(
            creationParameters.AdapterOrdinal,
            creationParameters.DeviceType,
            focusWindow,
            behaviorFlags,
            &presentParameters,
            &device,
            creationParameters.UseExtendedDeviceCreate);
        if (creationParameters.RetryWithoutDriverManagementEx
            && result == Direct3D9Factory.InvalidCallHResult
            && (behaviorFlags & D3D9.CreateDisableDriverManagementEX) != 0)
        {
            behaviorFlags &= unchecked((uint) ~D3D9.CreateDisableDriverManagementEX);
            result = CreateDevice(
                creationParameters.AdapterOrdinal,
                creationParameters.DeviceType,
                focusWindow,
                behaviorFlags,
                &presentParameters,
                &device,
                creationParameters.UseExtendedDeviceCreate);
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
        IDirect3DSurface9* dummyBackBuffer = null;
        uint realizationCacheIndex = Direct3D9ResourceCacheIndexManager.InvalidIndex;
        try
        {
            Caps9 capabilities = GetDeviceCapabilities(device);
            if (capabilities.DeviceType != Devtype.SW
                && capabilities.PixelShaderVersion < 0xFFFF0200)
            {
                Marshal.ThrowExceptionForHR(Direct3D9Factory.InsufficientGpuCapabilitiesHResult);
            }

            QueryDevice9Ex(device, &deviceEx);
            GetBackBuffer(device, &dummyBackBuffer);
            realizationCacheIndex = Direct3D9ResourceCacheIndexManager.AcquireIndex();
            Direct3D9Device managedDevice = new(
                device,
                deviceEx,
                capabilities.AdapterOrdinal,
                capabilities.DeviceType,
                behaviorFlags,
                presentParameters,
                unusableNotification,
                disposedNotification,
                capabilities,
                creationParameters.DisplayMode,
                _direct3D,
                focusWindow: focusWindow,
                adapterLuid: creationParameters.AdapterLuid,
                realizationCacheIndex: realizationCacheIndex,
                ownsRealizationCacheIndex: realizationCacheIndex != Direct3D9ResourceCacheIndexManager.InvalidIndex,
                dummyBackBuffer: dummyBackBuffer);
            realizationCacheIndex = Direct3D9ResourceCacheIndexManager.InvalidIndex;
            dummyBackBuffer = null;
            return managedDevice;
        }
        catch
        {
            Direct3D9ResourceCacheIndexManager.ReleaseIndex(realizationCacheIndex);
            Direct3D9Factory.Release(dummyBackBuffer);
            Direct3D9Factory.Release((IDirect3DDevice9*) deviceEx);
            Direct3D9Factory.Release(device);
            throw;
        }
    }

    private static Caps9 GetDeviceCapabilities(IDirect3DDevice9* device)
    {
        Caps9 capabilities = default;
        void** vtable = device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Caps9*, int> getDeviceCaps =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Caps9*, int>) vtable[7];
        int result = getDeviceCaps(device, &capabilities);
        Marshal.ThrowExceptionForHR(result);
        return capabilities;
    }

    private static void GetBackBuffer(IDirect3DDevice9* device, IDirect3DSurface9** backBuffer)
    {
        void** vtable = device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, BackbufferType, IDirect3DSurface9**, int> getBackBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, BackbufferType, IDirect3DSurface9**, int>) vtable[18];
        int result = getBackBuffer(device, 0, 0, BackbufferType.Mono, backBuffer);
        Marshal.ThrowExceptionForHR(result);
        if (*backBuffer is null)
        {
            throw new InvalidOperationException("Direct3D back-buffer retrieval returned a null interface pointer.");
        }
    }

    private int CreateDevice(
        uint adapterOrdinal,
        Devtype deviceType,
        nint focusWindow,
        uint behaviorFlags,
        PresentParameters* presentParameters,
        IDirect3DDevice9** device,
        bool useExtendedDeviceCreate = true)
    {
        *device = null;
        if (useExtendedDeviceCreate && _direct3DEx is not null)
        {
            IDirect3DDevice9Ex* deviceEx = null;
            void** vtable = _direct3DEx->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Devtype, nint, uint, PresentParameters*, Displaymodeex*, IDirect3DDevice9Ex**, int> createDeviceEx =
                (delegate* unmanaged[Stdcall]<IDirect3D9Ex*, uint, Devtype, nint, uint, PresentParameters*, Displaymodeex*, IDirect3DDevice9Ex**, int>) vtable[20];
            int result = createDeviceEx(_direct3DEx, adapterOrdinal, deviceType, focusWindow, behaviorFlags, presentParameters, null, &deviceEx);
            if (result >= 0)
            {
                result = QueryDevice9(deviceEx, device);
            }

            Direct3D9Factory.Release((IDirect3DDevice9*) deviceEx);
            return result;
        }

        void** baseVtable = _direct3D->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, nint, uint, PresentParameters*, IDirect3DDevice9**, int> createDevice =
            (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, nint, uint, PresentParameters*, IDirect3DDevice9**, int>) baseVtable[16];
        return createDevice(_direct3D, adapterOrdinal, deviceType, focusWindow, behaviorFlags, presentParameters, device);
    }

    private static int QueryDevice9(IDirect3DDevice9Ex* deviceEx, IDirect3DDevice9** device)
    {
        *device = null;
        Guid interfaceId = IDirect3DDevice9.Guid;
        void** vtable = deviceEx->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, Guid*, void**, int> queryInterface =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, Guid*, void**, int>) vtable[0];
        int result = queryInterface(deviceEx, &interfaceId, (void**) device);
        if (result < 0)
        {
            Direct3D9Factory.Release(*device);
            *device = null;
        }

        return result;
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
            Direct3D9Factory.Release(*deviceEx);
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
        _softwareRasterizerLoader?.Dispose();
        _softwareRasterizerLoader = null;
    }
}
