using Silk.NET.Direct3D11;
using Silk.NET.Direct3D9;
using Silk.NET.DXGI;

namespace WpfGfxShape.Core;

internal static class DirectXBindingInfo
{
    internal static Type Direct3D9ApiType => typeof(D3D9);

    internal static Type Direct3D11ApiType => typeof(D3D11);

    internal static Type DxgiApiType => typeof(DXGI);
}
