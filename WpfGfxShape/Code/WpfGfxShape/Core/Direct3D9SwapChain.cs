using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9SwapChain : Direct3D9Resource
{
    private readonly Direct3D9ResourceManager _resourceManager;
    private IDirect3DSwapChain9* _swapChain;

    internal Direct3D9SwapChain(Direct3D9ResourceManager resourceManager, IDirect3DSwapChain9* swapChain)
        : base(resourceManager)
    {
        _resourceManager = resourceManager;
        _swapChain = swapChain;
    }

    internal IDirect3DSwapChain9* SwapChain
    {
        get
        {
            ObjectDisposedException.ThrowIf(_swapChain is null, this);
            return _swapChain;
        }
    }

    internal PresentParameters GetPresentParameters()
    {
        ObjectDisposedException.ThrowIf(_swapChain is null, this);

        PresentParameters presentParameters = default;
        void** vtable = _swapChain->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, PresentParameters*, int> getPresentParameters =
            (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, PresentParameters*, int>) vtable[9];
        int result = getPresentParameters(_swapChain, &presentParameters);
        Marshal.ThrowExceptionForHR(result);
        return presentParameters;
    }

    internal Direct3D9Surface GetBackBuffer(uint index = 0)
    {
        ObjectDisposedException.ThrowIf(_swapChain is null, this);

        IDirect3DSurface9* surface = null;
        void** vtable = _swapChain->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint, BackbufferType, IDirect3DSurface9**, int> getBackBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, uint, BackbufferType, IDirect3DSurface9**, int>) vtable[5];
        int result = getBackBuffer(_swapChain, index, BackbufferType.Mono, &surface);
        if (result < 0)
        {
            Direct3D9Factory.Release(surface);
            Marshal.ThrowExceptionForHR(result);
        }
        if (surface is null)
        {
            throw new InvalidOperationException("Direct3D swap chain returned a null back buffer interface pointer.");
        }

        return new Direct3D9Surface(_resourceManager, surface);
    }

    internal Direct3D9DeviceState Present()
    {
        ObjectDisposedException.ThrowIf(_swapChain is null, this);

        void** vtable = _swapChain->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, void*, void*, nint, void*, uint, int> present =
            (delegate* unmanaged[Stdcall]<IDirect3DSwapChain9*, void*, void*, nint, void*, uint, int>) vtable[3];
        int result = present(_swapChain, null, null, 0, null, 0);
        return new Direct3D9DeviceState(result, Direct3D9DeviceStateSource.Present);
    }

    protected override void ReleaseD3DResources()
    {
        Direct3D9Factory.Release(_swapChain);
        _swapChain = null;
    }
}
