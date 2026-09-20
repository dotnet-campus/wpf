using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal static class Direct3D9SamplerState
{
    internal static int InitializeMaximumAnisotropy(
        uint maximumTextureBlendStages,
        uint anisotropicFilterLevel,
        Func<uint, Samplerstatetype, uint, int> setSamplerState)
    {
        ArgumentNullException.ThrowIfNull(setSamplerState);

        int result = 0;
        for (uint stage = 0; stage < maximumTextureBlendStages; stage++)
        {
            result = setSamplerState(
                stage,
                Samplerstatetype.Maxanisotropy,
                anisotropicFilterLevel);
            if (result < 0)
            {
                return result;
            }
        }

        return result;
    }
}
