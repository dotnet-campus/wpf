using System.Numerics;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9SetVertexShaderFloat4Constants(
    uint startRegister,
    ReadOnlySpan<Vector4> constants);

internal sealed class Direct3D9VertexShaderConstantState
{
    private const uint TwoDimensionalTransformRegisterCount = 4;

    private readonly Dictionary<uint, Vector4> _constants = [];

    internal int SetConstants(
        uint startRegister,
        ReadOnlySpan<Vector4> constants,
        ref bool twoDimensionalTransformApplied,
        uint twoDimensionalTransformStartRegister,
        Direct3D9SetVertexShaderFloat4Constants forceSetConstants)
    {
        ArgumentNullException.ThrowIfNull(forceSetConstants);

        Check2DTransformIntersection(
            startRegister,
            (uint) constants.Length,
            ref twoDimensionalTransformApplied,
            twoDimensionalTransformStartRegister);

        for (int i = 0; i < constants.Length; i++)
        {
            uint register = unchecked(startRegister + (uint) i);
            if (!_constants.TryGetValue(register, out Vector4 cached) || cached != constants[i])
            {
                return ForceSetConstants(startRegister, constants, forceSetConstants);
            }
        }

        return 0;
    }

    internal int ForceSetConstants(
        uint startRegister,
        ReadOnlySpan<Vector4> constants,
        Direct3D9SetVertexShaderFloat4Constants forceSetConstants)
    {
        ArgumentNullException.ThrowIfNull(forceSetConstants);

        int result = forceSetConstants(startRegister, constants);
        return UpdateAfterForceSet(startRegister, constants, result);
    }

    internal int UpdateAfterForceSet(
        uint startRegister,
        ReadOnlySpan<Vector4> constants,
        int result)
    {
        for (int i = 0; i < constants.Length; i++)
        {
            uint register = unchecked(startRegister + (uint) i);
            if (result >= 0)
            {
                _constants[register] = constants[i];
            }
            else
            {
                _constants.Remove(register);
            }
        }

        return result;
    }

    private static void Check2DTransformIntersection(
        uint startRegister,
        uint registerCount,
        ref bool twoDimensionalTransformApplied,
        uint twoDimensionalTransformStartRegister)
    {
        if (unchecked(startRegister + registerCount) > twoDimensionalTransformStartRegister
            && startRegister < unchecked(twoDimensionalTransformStartRegister + TwoDimensionalTransformRegisterCount))
        {
            twoDimensionalTransformApplied = false;
        }
    }
}
