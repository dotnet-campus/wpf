using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9SystemMemoryUpdateSurface : IDisposable
{
    private IDirect3DSurface9* _surface;

    internal Direct3D9SystemMemoryUpdateSurface(IDirect3DSurface9* surface)
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

    internal int LockRect(out LockedRect lockedRect, Direct3D9SurfaceRect rectangle, uint flags)
    {
        ObjectDisposedException.ThrowIf(_surface is null, this);

        lockedRect = default;
        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, LockedRect*, Direct3D9SurfaceRect*, uint, int> lockRect =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, LockedRect*, Direct3D9SurfaceRect*, uint, int>) vtable[13];
        fixed (LockedRect* lockedRectPointer = &lockedRect)
        {
            return lockRect(_surface, lockedRectPointer, &rectangle, flags);
        }
    }

    internal int UnlockRect()
    {
        ObjectDisposedException.ThrowIf(_surface is null, this);

        void** vtable = _surface->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DSurface9*, int> unlockRect =
            (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, int>) vtable[14];
        return unlockRect(_surface);
    }

    public void Dispose()
    {
        Direct3D9Factory.Release(_surface);
        _surface = null;
    }
}
