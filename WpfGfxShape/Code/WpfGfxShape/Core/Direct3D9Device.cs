using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using Windows.Win32;

namespace WpfGfxShape.Core;

internal enum Direct3D9DeviceStateSource
{
    CooperativeLevel,
    ExtendedCheck,
    Present
}

internal enum Direct3D9DeviceStateKind
{
    Operational,
    Occluded,
    ModeChanged,
    DeviceLost,
    Failure
}

internal readonly record struct Direct3D9DeviceState(int HResult, Direct3D9DeviceStateSource Source)
{
    internal Direct3D9DeviceStateKind Kind => HResult switch
    {
        0 => Direct3D9DeviceStateKind.Operational,
        Direct3D9Factory.PresentOccludedHResult => Direct3D9DeviceStateKind.Occluded,
        Direct3D9Factory.PresentModeChangedHResult => Direct3D9DeviceStateKind.ModeChanged,
        Direct3D9Factory.DeviceLostHResult or
        Direct3D9Factory.DeviceHungHResult or
        Direct3D9Factory.DeviceRemovedHResult => Direct3D9DeviceStateKind.DeviceLost,
        _ => Direct3D9DeviceStateKind.Failure
    };

    internal bool IsOperational => Kind is Direct3D9DeviceStateKind.Operational or Direct3D9DeviceStateKind.Occluded;

    internal bool RequiresDeviceRecreation => Kind is Direct3D9DeviceStateKind.ModeChanged or Direct3D9DeviceStateKind.DeviceLost;

    internal bool UsedExtendedCheck => Source == Direct3D9DeviceStateSource.ExtendedCheck;
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9Device : IDisposable
{
    private readonly Direct3D9ResourceManager _resourceManager = new();
    private readonly Action<Direct3D9Device>? _unusableNotification;
    private IDirect3DDevice9* _device;
    private IDirect3DDevice9Ex* _deviceEx;
    private bool _deviceLostProcessed;

    internal Direct3D9Device(
        IDirect3DDevice9* device,
        IDirect3DDevice9Ex* deviceEx,
        uint adapterOrdinal,
        Devtype deviceType,
        uint behaviorFlags,
        PresentParameters presentParameters,
        Action<Direct3D9Device>? unusableNotification = null)
    {
        _device = device;
        _deviceEx = deviceEx;
        _unusableNotification = unusableNotification;
        AdapterOrdinal = adapterOrdinal;
        DeviceType = deviceType;
        BehaviorFlags = behaviorFlags;
        PresentParameters = presentParameters;
    }

    internal IDirect3DDevice9* Device
    {
        get
        {
            ObjectDisposedException.ThrowIf(_device is null, this);
            return _device;
        }
    }

    internal uint AdapterOrdinal { get; }

    internal Devtype DeviceType { get; }

    internal uint BehaviorFlags { get; }

    internal PresentParameters PresentParameters { get; }

    internal bool IsExtended => _deviceEx is not null;

    internal bool IsUnusable => _deviceLostProcessed;

    internal int ResourceCount => _resourceManager.ResourceCount;

    internal Direct3D9ResourceManager ResourceManager => _resourceManager;

    internal void MarkUnusable()
    {
        if (_deviceLostProcessed)
        {
            return;
        }

        _deviceLostProcessed = true;
        _unusableNotification?.Invoke(this);
        _resourceManager.DestroyAllResources();
    }

    internal Direct3D9DeviceState CheckDeviceState(nint destinationWindow = 0)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        if (_deviceEx is not null)
        {
            void** extendedVtable = _deviceEx->LpVtbl;
            delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, nint, int> checkDeviceState =
                (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, nint, int>) extendedVtable[128];
            return new Direct3D9DeviceState(
                checkDeviceState(_deviceEx, destinationWindow),
                Direct3D9DeviceStateSource.ExtendedCheck);
        }

        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int> testCooperativeLevel =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, int>) vtable[3];
        return new Direct3D9DeviceState(
            testCooperativeLevel(_device),
            Direct3D9DeviceStateSource.CooperativeLevel);
    }

    internal Direct3D9Surface CreateRenderTarget(
        uint width,
        uint height,
        Format format = Format.A8R8G8B8,
        MultisampleType multiSampleType = MultisampleType.MultisampleNone,
        uint multiSampleQuality = 0,
        bool lockable = false)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        IDirect3DSurface9* surface = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int> createRenderTarget =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, MultisampleType, uint, int, IDirect3DSurface9**, void**, int>) vtable[28];
        int result = createRenderTarget(
            _device,
            width,
            height,
            format,
            multiSampleType,
            multiSampleQuality,
            lockable ? 1 : 0,
            &surface,
            null);
        if (result < 0)
        {
            Direct3D9Factory.Release(surface);
            Marshal.ThrowExceptionForHR(result);
        }
        if (surface is null)
        {
            throw new InvalidOperationException("Direct3D render target creation returned a null interface pointer.");
        }

        return new Direct3D9Surface(_resourceManager, surface);
    }

    internal Direct3D9SwapChain CreateAdditionalSwapChain(
        uint width,
        uint height,
        Format format = Format.X8R8G8B8)
    {
        ObjectDisposedException.ThrowIf(_device is null, this);

        PresentParameters presentParameters = new(
            backBufferWidth: width,
            backBufferHeight: height,
            backBufferFormat: format,
            backBufferCount: 1,
            swapEffect: Swapeffect.Discard,
            hDeviceWindow: (nint) PInvoke.GetDesktopWindow().Value,
            windowed: true,
            enableAutoDepthStencil: false,
            autoDepthStencilFormat: Format.Unknown);
        IDirect3DSwapChain9* swapChain = null;
        void** vtable = _device->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DDevice9*, PresentParameters*, IDirect3DSwapChain9**, int> createAdditionalSwapChain =
            (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, PresentParameters*, IDirect3DSwapChain9**, int>) vtable[13];
        int result = createAdditionalSwapChain(_device, &presentParameters, &swapChain);
        if (result < 0)
        {
            Direct3D9Factory.Release(swapChain);
            Marshal.ThrowExceptionForHR(result);
        }
        if (swapChain is null)
        {
            throw new InvalidOperationException("Direct3D additional swap chain creation returned a null interface pointer.");
        }

        return new Direct3D9SwapChain(_resourceManager, swapChain);
    }

    public void Dispose()
    {
        _resourceManager.DestroyAllResources();

        Direct3D9Factory.Release(_deviceEx);
        _deviceEx = null;

        Direct3D9Factory.Release(_device);
        _device = null;
    }
}
