using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9SystemMemoryReferenceTexture : IDisposable
{
    private IDirect3DTexture9* _texture;

    internal Direct3D9SystemMemoryReferenceTexture(IDirect3DTexture9* texture)
    {
        _texture = texture;
    }

    internal IDirect3DTexture9* Texture
    {
        get
        {
            ObjectDisposedException.ThrowIf(_texture is null, this);
            return _texture;
        }
    }

    public void Dispose()
    {
        Direct3D9Factory.Release(_texture);
        _texture = null;
    }
}
