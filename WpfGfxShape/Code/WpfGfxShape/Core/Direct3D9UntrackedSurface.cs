using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9UntrackedSurface : IDisposable
{
    private IDirect3DSurface9* _surface;

    internal Direct3D9UntrackedSurface(IDirect3DSurface9* surface)
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

    public void Dispose()
    {
        Direct3D9Factory.Release(_surface);
        _surface = null;
    }
}
