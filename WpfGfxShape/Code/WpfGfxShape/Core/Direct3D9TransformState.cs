using System.Numerics;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int TryGetTransform(Transformstatetype state, out Matrix4x4 matrix);

internal static class Direct3D9TransformState
{
    private const Transformstatetype World = (Transformstatetype) 256;

    internal static int InitializeIdentityTransforms(
        Func<Transformstatetype, Matrix4x4, int> setTransform)
    {
        return SetTransforms(Matrix4x4.Identity, setTransform);
    }

    internal static int Set2DTransformsForFixedFunction(
        Matrix4x4 projection,
        ref bool transformsApplied,
        Func<Transformstatetype, Matrix4x4, int> setTransform)
    {
        ArgumentNullException.ThrowIfNull(setTransform);

        if (transformsApplied)
        {
            return 0;
        }

        int result = SetTransforms(projection, setTransform);
        if (result >= 0)
        {
            transformsApplied = true;
        }

        return result;
    }

    internal static int Set2DTransformForVertexShader(
        Matrix4x4 projection,
        uint startRegister,
        ref bool transformApplied,
        ref uint appliedStartRegister,
        Func<uint, Matrix4x4, int> setVertexShaderConstants)
    {
        return Set2DTransformForVertexShader(
            startRegister,
            ref transformApplied,
            ref appliedStartRegister,
            (Transformstatetype _, out Matrix4x4 matrix) =>
            {
                matrix = projection;
                return 0;
            },
            setVertexShaderConstants);
    }

    internal static int Set2DTransformForVertexShader(
        uint startRegister,
        ref bool transformApplied,
        ref uint appliedStartRegister,
        TryGetTransform getTransform,
        Func<uint, Matrix4x4, int> setVertexShaderConstants)
    {
        ArgumentNullException.ThrowIfNull(getTransform);
        ArgumentNullException.ThrowIfNull(setVertexShaderConstants);

        if (transformApplied && startRegister == appliedStartRegister)
        {
            return 0;
        }

        int result = getTransform(Transformstatetype.Projection, out Matrix4x4 projection);
        if (result < 0)
        {
            return result;
        }

        result = setVertexShaderConstants(startRegister, Matrix4x4.Transpose(projection));
        if (result >= 0)
        {
            appliedStartRegister = startRegister;
            transformApplied = true;
        }

        return result;
    }

    internal static int Set3DTransforms(
        Matrix4x4 world,
        Matrix4x4 view,
        Matrix4x4 projection,
        Matrix4x4 viewportProjectionModifier,
        Matrix4x4 surfaceToClip,
        Func<Transformstatetype, Matrix4x4, int> setTransform)
    {
        ArgumentNullException.ThrowIfNull(setTransform);

        int result = setTransform(World, world);
        if (result < 0)
        {
            return result;
        }

        result = setTransform(Transformstatetype.View, view);
        if (result < 0)
        {
            return result;
        }

        Matrix4x4 projectionModifier = viewportProjectionModifier * surfaceToClip;
        return setTransform(Transformstatetype.Projection, projection * projectionModifier);
    }

    internal static int Set3DTransformForVertexShader(
        Matrix4x4 world,
        Matrix4x4 view,
        Matrix4x4 projection,
        uint startRegister,
        Func<uint, Matrix4x4, int> setVertexShaderConstants)
    {
        return Set3DTransformForVertexShader(
            startRegister,
            (Transformstatetype state, out Matrix4x4 matrix) =>
            {
                matrix = state switch
                {
                    World => world,
                    Transformstatetype.View => view,
                    Transformstatetype.Projection => projection,
                    _ => default
                };
                return 0;
            },
            setVertexShaderConstants);
    }

    internal static int Set3DTransformForVertexShader(
        uint startRegister,
        TryGetTransform getTransform,
        Func<uint, Matrix4x4, int> setVertexShaderConstants)
    {
        ArgumentNullException.ThrowIfNull(getTransform);
        ArgumentNullException.ThrowIfNull(setVertexShaderConstants);

        int result = getTransform(World, out Matrix4x4 world);
        if (result < 0)
        {
            return result;
        }

        result = getTransform(Transformstatetype.View, out Matrix4x4 view);
        if (result < 0)
        {
            return result;
        }

        Matrix4x4 worldView = world * view;
        result = setVertexShaderConstants(startRegister, Matrix4x4.Transpose(worldView));
        if (result < 0)
        {
            return result;
        }

        result = getTransform(Transformstatetype.Projection, out Matrix4x4 projection);
        if (result < 0)
        {
            return result;
        }

        result = setVertexShaderConstants(startRegister + 4, Matrix4x4.Transpose(worldView * projection));
        if (result < 0)
        {
            return result;
        }

        Matrix4x4 normalTransform = GetAdjoint(worldView);
        if (worldView.GetDeterminant() < 0f)
        {
            normalTransform *= -1f;
        }

        return setVertexShaderConstants(startRegister + 8, normalTransform);
    }

    private static Matrix4x4 GetAdjoint(Matrix4x4 matrix)
    {
        float x00 = matrix.M11;
        float x01 = matrix.M12;
        float x10 = matrix.M21;
        float x11 = matrix.M22;
        float x20 = matrix.M31;
        float x21 = matrix.M32;
        float x30 = matrix.M41;
        float x31 = matrix.M42;

        float y01 = (x00 * x11) - (x10 * x01);
        float y02 = (x00 * x21) - (x20 * x01);
        float y03 = (x00 * x31) - (x30 * x01);
        float y12 = (x10 * x21) - (x20 * x11);
        float y13 = (x10 * x31) - (x30 * x11);
        float y23 = (x20 * x31) - (x30 * x21);

        float x02 = matrix.M13;
        float x03 = matrix.M14;
        float x12 = matrix.M23;
        float x13 = matrix.M24;
        float x22 = matrix.M33;
        float x23 = matrix.M34;
        float x32 = matrix.M43;
        float x33 = matrix.M44;

        float z33 = (x02 * y12) - (x12 * y02) + (x22 * y01);
        float z23 = (x12 * y03) - (x32 * y01) - (x02 * y13);
        float z13 = (x02 * y23) - (x22 * y03) + (x32 * y02);
        float z03 = (x22 * y13) - (x32 * y12) - (x12 * y23);
        float z32 = (x13 * y02) - (x23 * y01) - (x03 * y12);
        float z22 = (x03 * y13) - (x13 * y03) + (x33 * y01);
        float z12 = (x23 * y03) - (x33 * y02) - (x03 * y23);
        float z02 = (x13 * y23) - (x23 * y13) + (x33 * y12);

        y01 = (x02 * x13) - (x12 * x03);
        y02 = (x02 * x23) - (x22 * x03);
        y03 = (x02 * x33) - (x32 * x03);
        y12 = (x12 * x23) - (x22 * x13);
        y13 = (x12 * x33) - (x32 * x13);
        y23 = (x22 * x33) - (x32 * x23);

        float z30 = (x11 * y02) - (x21 * y01) - (x01 * y12);
        float z20 = (x01 * y13) - (x11 * y03) + (x31 * y01);
        float z10 = (x21 * y03) - (x31 * y02) - (x01 * y23);
        float z00 = (x11 * y23) - (x21 * y13) + (x31 * y12);
        float z31 = (x00 * y12) - (x10 * y02) + (x20 * y01);
        float z21 = (x10 * y03) - (x30 * y01) - (x00 * y13);
        float z11 = (x00 * y23) - (x20 * y03) + (x30 * y02);
        float z01 = (x20 * y13) - (x30 * y12) - (x10 * y23);

        return new Matrix4x4(
            z00, z10, z20, z30,
            z01, z11, z21, z31,
            z02, z12, z22, z32,
            z03, z13, z23, z33);
    }

    private static int SetTransforms(
        Matrix4x4 projection,
        Func<Transformstatetype, Matrix4x4, int> setTransform)
    {
        ArgumentNullException.ThrowIfNull(setTransform);

        int result = setTransform(World, Matrix4x4.Identity);
        if (result < 0)
        {
            return result;
        }

        result = setTransform(Transformstatetype.View, Matrix4x4.Identity);
        if (result < 0)
        {
            return result;
        }

        return setTransform(Transformstatetype.Projection, projection);
    }
}
