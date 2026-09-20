namespace WpfGfxShape.Core;

internal delegate int Direct3D9SetPixelShaderBoolConstant(
    uint register,
    int constant);

internal sealed class Direct3D9PixelShaderBoolConstantState
{
    private readonly Dictionary<uint, int> _constants = [];

    internal int SetConstant(
        uint register,
        int constant,
        Direct3D9SetPixelShaderBoolConstant forceSetConstant)
    {
        ArgumentNullException.ThrowIfNull(forceSetConstant);

        return _constants.TryGetValue(register, out int cached) && cached == constant
            ? 0
            : ForceSetConstant(register, constant, forceSetConstant);
    }

    internal int ForceSetConstant(
        uint register,
        int constant,
        Direct3D9SetPixelShaderBoolConstant forceSetConstant)
    {
        ArgumentNullException.ThrowIfNull(forceSetConstant);

        int result = forceSetConstant(register, constant);
        if (result >= 0)
        {
            _constants[register] = constant;
        }
        else
        {
            _constants.Remove(register);
        }

        return result;
    }
}
