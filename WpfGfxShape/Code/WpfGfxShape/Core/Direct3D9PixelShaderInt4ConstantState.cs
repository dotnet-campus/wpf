namespace WpfGfxShape.Core;

internal delegate int Direct3D9SetPixelShaderInt4Constant(
    uint register,
    ReadOnlySpan<int> constant);

internal sealed class Direct3D9PixelShaderInt4ConstantState
{
    private readonly Dictionary<uint, int> _constants = [];

    internal int SetConstant(
        uint register,
        ReadOnlySpan<int> constant,
        Direct3D9SetPixelShaderInt4Constant forceSetConstant)
    {
        ArgumentNullException.ThrowIfNull(forceSetConstant);
        ValidateConstant(constant);

        return _constants.TryGetValue(register, out int cached) && cached == constant[0]
            ? 0
            : ForceSetConstant(register, constant, forceSetConstant);
    }

    internal int ForceSetConstant(
        uint register,
        ReadOnlySpan<int> constant,
        Direct3D9SetPixelShaderInt4Constant forceSetConstant)
    {
        ArgumentNullException.ThrowIfNull(forceSetConstant);
        ValidateConstant(constant);

        int result = forceSetConstant(register, constant);
        if (result >= 0)
        {
            _constants[register] = constant[0];
        }
        else
        {
            _constants.Remove(register);
        }

        return result;
    }

    private static void ValidateConstant(ReadOnlySpan<int> constant)
    {
        if (constant.Length != 4)
        {
            throw new ArgumentException("An int4 pixel shader constant must contain exactly four elements.", nameof(constant));
        }
    }
}
