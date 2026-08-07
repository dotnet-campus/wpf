using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9Surface : Direct3D9Resource
{
    private IDirect3DSurface9* _surface;

    internal Direct3D9Surface(Direct3D9ResourceManager resourceManager, IDirect3DSurface9* surface)
        : base(resourceManager)
    {
        _surface = surface;
    }

    internal IDirect3DSurface9* Surface
    {
        get
        {
            ObjectDisposedException.ThrowIf(_surface is null, this);
            return _surface;
        }
    }

    internal SurfaceDesc GetDescription()
    {
        ObjectDisposedException.ThrowIf(_surface is null, this);

        SurfaceDesc description = default;
        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int> getDescription =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) vtable[12];
        int result = getDescription(_surface, &description);
        Marshal.ThrowExceptionForHR(result);
        return description;
    }

    protected override void ReleaseD3DResources()
    {
        Direct3D9Factory.Release(_surface);
        _surface = null;
    }
}
