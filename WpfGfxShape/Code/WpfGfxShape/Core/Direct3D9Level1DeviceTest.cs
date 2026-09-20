using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal static unsafe class Direct3D9Level1DeviceTest
{
    internal static int Test(Direct3D9Device device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return Test(
            device.Capabilities,
            device.SetRenderState,
            () => device.SetPixelShader(null),
            () => device.SetVertexShader(null),
            device.SetTextureStageState,
            stage => device.SetD3DTexture(stage, null),
            device.SetSamplerState);
    }

    internal static int Test(
        Caps9 capabilities,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader)
    {
        return Test(
            capabilities,
            setRenderState,
            clearPixelShader,
            clearVertexShader,
            (_, _, _) => 0,
            _ => 0);
    }

    internal static int Test(
        Caps9 capabilities,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState,
        Func<uint, int> clearTexture,
        Func<uint, Samplerstatetype, uint, int>? setSamplerState = null)
    {
        ArgumentNullException.ThrowIfNull(setRenderState);
        ArgumentNullException.ThrowIfNull(clearPixelShader);
        ArgumentNullException.ThrowIfNull(clearVertexShader);
        ArgumentNullException.ThrowIfNull(setTextureStageState);
        ArgumentNullException.ThrowIfNull(clearTexture);
        bool testTextureState = setSamplerState is not null;
        setSamplerState ??= (_, _, _) => 0;

        int result = Direct3D9HardwareCapabilities.CheckDeviceLevel1(capabilities);
        if (result < 0)
        {
            return result;
        }

        result = SetAlphaSolidBrushState(
            capabilities.MaxTextureBlendStages,
            setRenderState,
            clearPixelShader,
            clearVertexShader,
            setTextureStageState,
            clearTexture);
        if (result < 0)
        {
            return result;
        }

        if (!testTextureState)
        {
            return 0;
        }

        result = SetNearestNeighborTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetLinearTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetTriLinearTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetNearestNeighborDiffuseTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetLinearDiffuseTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetTriLinearDiffuseTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetNearestNeighborSpecularTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetLinearSpecularTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetTriLinearSpecularTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetAlphaSolidBrushState(
            capabilities.MaxTextureBlendStages,
            setRenderState,
            clearPixelShader,
            clearVertexShader,
            setTextureStageState,
            clearTexture);
        if (result < 0)
        {
            return result;
        }

        result = SetNearestNeighborMaskedTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetLinearMaskedTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetTriLinearMaskedTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetNearestNeighborDiffuseMaskedTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetLinearDiffuseMaskedTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetTriLinearDiffuseMaskedTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetNearestNeighborSpecularMaskedTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = SetLinearSpecularMaskedTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        return SetTriLinearSpecularMaskedTextureState(
            capabilities.MaxTextureBlendStages,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    internal static int SetAlphaSolidBrushState(
        uint maxTextureBlendStages,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState,
        Func<uint, int> clearTexture)
    {
        int result = setRenderState(Renderstatetype.Alphablendenable, 1);
        if (result < 0)
        {
            return result;
        }

        result = setRenderState(Renderstatetype.Srcblend, (uint) Blend.One);
        if (result < 0)
        {
            return result;
        }

        result = setRenderState(Renderstatetype.Destblend, (uint) Blend.Invsrcalpha);
        if (result < 0)
        {
            return result;
        }

        result = clearPixelShader();
        if (result < 0)
        {
            return result;
        }

        result = clearVertexShader();
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Colorop, (uint) Textureop.Selectarg1);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Colorarg1, (uint) D3D9.TADiffuse);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Colorarg2, (uint) D3D9.TACurrent);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Alphaop, (uint) Textureop.Selectarg1);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Alphaarg1, (uint) D3D9.TADiffuse);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Alphaarg2, (uint) D3D9.TACurrent);
        if (result < 0)
        {
            return result;
        }

        result = clearTexture(0);
        if (result < 0)
        {
            return result;
        }

        return DisableTextureStage(1, maxTextureBlendStages, setTextureStageState);
    }

    internal static int SetNearestNeighborTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Point,
            Texturefiltertype.Point,
            Texturefiltertype.None,
            Textureop.Selectarg1,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    internal static int SetNearestNeighborMaskedTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Point,
            Texturefiltertype.Point,
            Texturefiltertype.None,
            Textureop.Selectarg1,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            1);
    }

    internal static int SetLinearMaskedTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.None,
            Textureop.Selectarg1,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            1);
    }

    internal static int SetTriLinearMaskedTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Textureop.Selectarg1,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            1);
    }

    internal static int SetNearestNeighborDiffuseMaskedTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Point,
            Texturefiltertype.Point,
            Texturefiltertype.None,
            Textureop.Modulate,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            1);
    }

    internal static int SetLinearDiffuseMaskedTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.None,
            Textureop.Modulate,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            1);
    }

    internal static int SetTriLinearDiffuseMaskedTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Textureop.Modulate,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            1);
    }

    internal static int SetNearestNeighborSpecularMaskedTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Point,
            Texturefiltertype.Point,
            Texturefiltertype.None,
            Textureop.Modulate,
            D3D9.TASpecular,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            1);
    }

    internal static int SetLinearSpecularMaskedTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.None,
            Textureop.Modulate,
            D3D9.TASpecular,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            1);
    }

    internal static int SetTriLinearSpecularMaskedTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Textureop.Modulate,
            D3D9.TASpecular,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            1);
    }

    internal static int SetLinearTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.None,
            Textureop.Selectarg1,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    internal static int SetTriLinearTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Textureop.Selectarg1,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    internal static int SetNearestNeighborDiffuseTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetNearestNeighborDiffuseTextureState(
            maxTextureBlendStages,
            Direct3D9TextureBlendMode.Default,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    internal static int SetNearestNeighborDiffuseTextureState(
        uint maxTextureBlendStages,
        Direct3D9TextureBlendMode blendMode,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Point,
            Texturefiltertype.Point,
            Texturefiltertype.None,
            Textureop.Modulate,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState,
            blendMode: blendMode);
    }

    internal static int SetNearestNeighborSpecularTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Point,
            Texturefiltertype.Point,
            Texturefiltertype.None,
            Textureop.Modulate,
            D3D9.TASpecular,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    internal static int SetLinearSpecularTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.None,
            Textureop.Modulate,
            D3D9.TASpecular,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    internal static int SetTriLinearSpecularTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Textureop.Modulate,
            D3D9.TASpecular,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    internal static int SetLinearDiffuseTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.None,
            Textureop.Modulate,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    internal static int SetTriLinearDiffuseTextureState(
        uint maxTextureBlendStages,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        return SetTextureState(
            maxTextureBlendStages,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Texturefiltertype.Linear,
            Textureop.Modulate,
            D3D9.TACurrent,
            clearPixelShader,
            clearVertexShader,
            setSamplerState,
            setRenderState,
            setTextureStageState);
    }

    private static int SetTextureState(
        uint maxTextureBlendStages,
        Texturefiltertype magFilter,
        Texturefiltertype minFilter,
        Texturefiltertype mipFilter,
        Textureop colorOperation,
        uint colorArgument2,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState,
        uint maskCount = 0,
        Direct3D9TextureBlendMode blendMode = Direct3D9TextureBlendMode.Default)
    {
        (uint alphaBlendEnable, Blend sourceBlend, Blend destinationBlend) = blendMode switch
        {
            Direct3D9TextureBlendMode.Default => (1u, Blend.One, Blend.Invsrcalpha),
            Direct3D9TextureBlendMode.Copy => (0u, Blend.One, Blend.Zero),
            Direct3D9TextureBlendMode.ApplyVectorAlpha => (1u, Blend.Zero, Blend.Invsrccolor),
            Direct3D9TextureBlendMode.AddColors => (1u, Blend.One, Blend.One),
            _ => throw new ArgumentOutOfRangeException(nameof(blendMode))
        };

        int result = clearPixelShader();
        if (result < 0)
        {
            return result;
        }

        result = clearVertexShader();
        if (result < 0)
        {
            return result;
        }

        result = setSamplerState(0, Samplerstatetype.Magfilter, (uint) magFilter);
        if (result < 0)
        {
            return result;
        }

        result = setSamplerState(0, Samplerstatetype.Minfilter, (uint) minFilter);
        if (result < 0)
        {
            return result;
        }

        result = setSamplerState(0, Samplerstatetype.Mipfilter, (uint) mipFilter);
        if (result < 0)
        {
            return result;
        }

        result = setRenderState(Renderstatetype.Alphablendenable, alphaBlendEnable);
        if (result < 0)
        {
            return result;
        }

        result = setRenderState(Renderstatetype.Srcblend, (uint) sourceBlend);
        if (result < 0)
        {
            return result;
        }

        result = setRenderState(Renderstatetype.Destblend, (uint) destinationBlend);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Colorop, (uint) colorOperation);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Colorarg1, (uint) D3D9.TATexture);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Colorarg2, colorArgument2);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Alphaop, (uint) Textureop.Selectarg1);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Alphaarg1, (uint) D3D9.TATexture);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(0, Texturestagestatetype.Alphaarg2, (uint) D3D9.TACurrent);
        if (result < 0)
        {
            return result;
        }

        result = setTextureStageState(
            0,
            Texturestagestatetype.Texturetransformflags,
            (uint) Texturetransformflags.Disable);
        if (result < 0)
        {
            return result;
        }

        for (uint stage = 1; stage <= maskCount; stage++)
        {
            result = setSamplerState(stage, Samplerstatetype.Magfilter, (uint) magFilter);
            if (result < 0)
            {
                return result;
            }

            result = setSamplerState(stage, Samplerstatetype.Minfilter, (uint) minFilter);
            if (result < 0)
            {
                return result;
            }

            result = setSamplerState(stage, Samplerstatetype.Mipfilter, (uint) mipFilter);
            if (result < 0)
            {
                return result;
            }

            result = setTextureStageState(stage, Texturestagestatetype.Colorop, (uint) Textureop.Modulate);
            if (result < 0)
            {
                return result;
            }

            result = setTextureStageState(
                stage,
                Texturestagestatetype.Colorarg1,
                (uint) (D3D9.TATexture | D3D9.TAAlphareplicate));
            if (result < 0)
            {
                return result;
            }

            result = setTextureStageState(stage, Texturestagestatetype.Colorarg2, (uint) D3D9.TACurrent);
            if (result < 0)
            {
                return result;
            }

            result = setTextureStageState(stage, Texturestagestatetype.Alphaop, (uint) Textureop.Modulate);
            if (result < 0)
            {
                return result;
            }

            result = setTextureStageState(stage, Texturestagestatetype.Alphaarg1, (uint) D3D9.TATexture);
            if (result < 0)
            {
                return result;
            }

            result = setTextureStageState(stage, Texturestagestatetype.Alphaarg2, (uint) D3D9.TACurrent);
            if (result < 0)
            {
                return result;
            }

            result = setTextureStageState(
                stage,
                Texturestagestatetype.Texturetransformflags,
                (uint) Texturetransformflags.Disable);
            if (result < 0)
            {
                return result;
            }
        }

        return DisableTextureStage(maskCount + 1, maxTextureBlendStages, setTextureStageState);
    }

    internal static int DisableTextureStage(
        uint stage,
        uint maxTextureBlendStages,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        ArgumentNullException.ThrowIfNull(setTextureStageState);

        return stage < maxTextureBlendStages
            ? setTextureStageState(stage, Texturestagestatetype.Colorop, (uint) Textureop.Disable)
            : 0;
    }
}
