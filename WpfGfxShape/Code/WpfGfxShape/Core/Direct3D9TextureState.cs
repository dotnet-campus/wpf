namespace WpfGfxShape.Core;

internal static class Direct3D9TextureState
{
    internal static int ClearTextures(uint maximumTextureBlendStages, Func<uint, int> clearTexture)
    {
        ArgumentNullException.ThrowIfNull(clearTexture);

        for (uint stage = 0; stage < maximumTextureBlendStages; stage++)
        {
            int result = clearTexture(stage);
            if (result < 0)
            {
                return result;
            }
        }

        return 0;
    }
}
