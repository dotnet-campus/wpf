using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal static class Direct3D9TextureStageState
{
    private const uint TextureStageCount = 8;

    internal static int DisableTextureStage(
        uint stage,
        uint maximumTextureBlendStages,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        ArgumentNullException.ThrowIfNull(setTextureStageState);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(stage, maximumTextureBlendStages);

        return stage < maximumTextureBlendStages
            ? setTextureStageState(stage, Texturestagestatetype.Colorop, (uint) Textureop.Disable)
            : 0;
    }

    internal static int InitializeColorOperations(
        uint maximumTextureBlendStages,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        ArgumentNullException.ThrowIfNull(setTextureStageState);

        for (uint stage = 0; stage < TextureStageCount; stage++)
        {
            int result = setTextureStageState(
                stage,
                Texturestagestatetype.Colorop,
                (uint) Textureop.Disable);
            if (result < 0)
            {
                return result;
            }
        }

        for (uint stage = TextureStageCount + 1; stage < maximumTextureBlendStages; stage++)
        {
            int result = setTextureStageState(
                stage,
                Texturestagestatetype.Colorop,
                (uint) Textureop.Disable);
            if (result < 0)
            {
                return result;
            }
        }

        return 0;
    }

    internal static int InitializeTextureCoordinateIndices(
        uint maximumTextureBlendStages,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        ArgumentNullException.ThrowIfNull(setTextureStageState);

        for (uint stage = 0; stage < maximumTextureBlendStages; stage++)
        {
            int result = setTextureStageState(
                stage,
                Texturestagestatetype.Texcoordindex,
                stage);
            if (result < 0)
            {
                return result;
            }
        }

        return 0;
    }

    internal static int InitializeTextureTransformFlags(
        uint maximumTextureBlendStages,
        Func<uint, Texturestagestatetype, uint, int> setTextureStageState)
    {
        ArgumentNullException.ThrowIfNull(setTextureStageState);

        for (uint stage = 0; stage < maximumTextureBlendStages; stage++)
        {
            int result = setTextureStageState(
                stage,
                Texturestagestatetype.Texturetransformflags,
                (uint) Texturetransformflags.Disable);
            if (result < 0)
            {
                return result;
            }
        }

        return 0;
    }
}
