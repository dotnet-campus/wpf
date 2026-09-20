using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal static class Direct3D9RenderState
{
    internal static int Initialize(
        Caps9 capabilities,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<int>? clearDepthStencilSurface = null)
    {
        ArgumentNullException.ThrowIfNull(setRenderState);

        if ((capabilities.SrcBlendCaps & (uint) D3D9.PblendcapsBlendfactor) != 0)
        {
            int result = setRenderState(Renderstatetype.Blendfactor, 0);
            if (result < 0)
            {
                return result;
            }
        }

        (Renderstatetype State, uint Value)[] defaults =
        [
            (Renderstatetype.Zenable, 0),
            (Renderstatetype.Zwriteenable, 0),
            (Renderstatetype.Fillmode, 3),
            (Renderstatetype.Shademode, 2),
            (Renderstatetype.Alphatestenable, 0),
            (Renderstatetype.Lastpixel, 0),
            (Renderstatetype.Antialiasedlineenable, 0),
            (Renderstatetype.Cullmode, 1),
            (Renderstatetype.Zfunc, 4),
            (Renderstatetype.Ditherenable, 0),
            (Renderstatetype.Fogenable, 0),
            (Renderstatetype.Depthbias, 0),
            (Renderstatetype.Stencilenable, 0),
            (Renderstatetype.Stencilref, 0),
            (Renderstatetype.Stencilfunc, 6),
            (Renderstatetype.Stencilfail, 1),
            (Renderstatetype.Stencilzfail, 1),
            (Renderstatetype.Stencilpass, 1),
            (Renderstatetype.Stencilmask, uint.MaxValue),
            (Renderstatetype.Stencilwritemask, 0),
            (Renderstatetype.Twosidedstencilmode, 0),
            (Renderstatetype.Clipping, 1),
            (Renderstatetype.Lighting, 0),
            (Renderstatetype.Specularenable, 0),
            (Renderstatetype.Colorvertex, 1),
            (Renderstatetype.Normalizenormals, 0),
            (Renderstatetype.Diffusematerialsource, 1),
            (Renderstatetype.Specularmaterialsource, 1),
            (Renderstatetype.Ambientmaterialsource, 0),
            (Renderstatetype.Ambient, 0),
            (Renderstatetype.Vertexblend, 0),
            (Renderstatetype.Clipplaneenable, 0),
            (Renderstatetype.Multisampleantialias, 1),
            (Renderstatetype.Multisamplemask, uint.MaxValue),
            (Renderstatetype.Colorwriteenable, 0x0000000F),
            (Renderstatetype.Blendop, 1)
        ];

        foreach ((Renderstatetype state, uint value) in defaults)
        {
            int result = setRenderState(state, value);
            if (result < 0)
            {
                return result;
            }

            if (state == Renderstatetype.Ambientmaterialsource && clearDepthStencilSurface is not null)
            {
                result = clearDepthStencilSurface();
                if (result < 0)
                {
                    return result;
                }
            }
        }

        if ((capabilities.RasterCaps & (uint) D3D9.PrastercapsScissortest) != 0)
        {
            return setRenderState(Renderstatetype.Scissortestenable, 0);
        }

        return 0;
    }
}
