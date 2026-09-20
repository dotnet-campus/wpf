using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9StateBlock : IDisposable
{
    private IDirect3DStateBlock9* _stateBlock;

    internal Direct3D9StateBlock(IDirect3DStateBlock9* stateBlock)
    {
        _stateBlock = stateBlock;
    }

    internal IDirect3DStateBlock9* StateBlock
    {
        get
        {
            ObjectDisposedException.ThrowIf(_stateBlock is null, this);
            return _stateBlock;
        }
    }

    public void Dispose()
    {
        Direct3D9Factory.Release(_stateBlock);
        _stateBlock = null;
    }
}
