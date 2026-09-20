using Silk.NET.Direct3D9;
using Silk.NET.DXGI;

namespace WpfGfxShape.Core;

internal static class Direct3D9MaterialState
{
    internal static int InitializeDefaultMaterial(Func<Material9, int> setMaterial)
    {
        ArgumentNullException.ThrowIfNull(setMaterial);

        Material9 material = new(
            diffuse: new D3Dcolorvalue(1.0f, 1.0f, 1.0f, 1.0f),
            ambient: new D3Dcolorvalue(0.0f, 0.0f, 0.0f, 0.0f),
            specular: new D3Dcolorvalue(0.0f, 0.0f, 0.0f, 0.0f),
            emissive: new D3Dcolorvalue(0.0f, 0.0f, 0.0f, 0.0f),
            power: 40.0f);

        return setMaterial(material);
    }
}
