using System.Numerics;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal static class Direct3D9DefaultState
{
    internal static int Initialize(
        Caps9 capabilities,
        Func<Renderstatetype, uint, int> setRenderState,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState,
        Func<uint, Samplerstatetype, uint, int> setSamplerState,
        Func<Transformstatetype, Matrix4x4, int> setTransform,
        Func<Material9, int> setMaterial,
        Func<uint, int> clearTexture,
        Func<int> clearPixelShader,
        Func<int> clearPrimaryStreamSource,
        Func<uint, int> clearAdditionalStreamSource,
        Func<int> clearIndices,
        Action? resetScissorAndClipCache = null,
        Func<int>? clearDepthStencilSurface = null)
    {
        ArgumentNullException.ThrowIfNull(setRenderState);
        ArgumentNullException.ThrowIfNull(setTextureStageState);
        ArgumentNullException.ThrowIfNull(setSamplerState);
        ArgumentNullException.ThrowIfNull(setTransform);
        ArgumentNullException.ThrowIfNull(setMaterial);
        ArgumentNullException.ThrowIfNull(clearTexture);
        ArgumentNullException.ThrowIfNull(clearPixelShader);
        ArgumentNullException.ThrowIfNull(clearPrimaryStreamSource);
        ArgumentNullException.ThrowIfNull(clearAdditionalStreamSource);
        ArgumentNullException.ThrowIfNull(clearIndices);

        int result = Direct3D9RenderState.Initialize(
            capabilities,
            setRenderState,
            clearDepthStencilSurface);
        if (result < 0)
        {
            return result;
        }

        uint maximumTextureBlendStages = Math.Min(8u, capabilities.MaxTextureBlendStages);
        result = Direct3D9TextureStageState.InitializeColorOperations(
            maximumTextureBlendStages,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        uint anisotropicFilterLevel = Math.Min(4u, Math.Max(1u, capabilities.MaxAnisotropy));
        result = Direct3D9SamplerState.InitializeMaximumAnisotropy(
            maximumTextureBlendStages,
            anisotropicFilterLevel,
            setSamplerState);
        if (result < 0)
        {
            return result;
        }

        result = Direct3D9TextureStageState.InitializeTextureCoordinateIndices(
            maximumTextureBlendStages,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = Direct3D9TextureStageState.InitializeTextureTransformFlags(
            maximumTextureBlendStages,
            setTextureStageState);
        if (result < 0)
        {
            return result;
        }

        result = Direct3D9TransformState.InitializeIdentityTransforms(setTransform);
        if (result < 0)
        {
            return result;
        }

        result = Direct3D9MaterialState.InitializeDefaultMaterial(setMaterial);
        if (result < 0)
        {
            return result;
        }

        result = Direct3D9TextureState.ClearTextures(maximumTextureBlendStages, clearTexture);
        if (result < 0)
        {
            return result;
        }

        result = clearPixelShader();
        if (result < 0)
        {
            return result;
        }

        result = clearPrimaryStreamSource();
        if (result < 0)
        {
            return result;
        }

        for (uint stream = 1; stream < capabilities.MaxStreams; stream++)
        {
            result = clearAdditionalStreamSource(stream);
            if (result < 0)
            {
                return result;
            }
        }

        result = clearIndices();
        if (result < 0)
        {
            return result;
        }

        resetScissorAndClipCache?.Invoke();
        return 0;
    }
}
