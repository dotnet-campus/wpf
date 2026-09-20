using System.Numerics;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9SetPipelineShaderMatrix3x2(
    nint shader,
    uint startRegister,
    Matrix3x2 matrix);

internal delegate int Direct3D9SetPipelineTextureMapping(
    nint vertexBuilder,
    uint destinationCoordinateIndex,
    uint sourceCoordinateIndex,
    Matrix3x2 devicePointToTextureUv);

internal unsafe delegate IDirect3DBaseTexture9* Direct3D9GetPipelineTexture();

internal enum Direct3D9VertexFormatAttribute : uint
{
    None = 0,
    Xyz = 0x3,
    Normal = 0x4,
    Diffuse = 0x8,
    Specular = 0x10,
    Uv1 = 0x100,
    Uv2 = 0x300,
    Uv3 = 0x700,
    Uv4 = 0xF00,
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class Direct3D9PipelineShaderMatrix3x2State
{
    private readonly Direct3D9SetVertexShaderFloat4Constants _setVertexShaderConstants;

    internal Direct3D9PipelineShaderMatrix3x2State(Direct3D9Device device)
        : this(GetSetter(device))
    {
    }

    private static Direct3D9SetVertexShaderFloat4Constants GetSetter(Direct3D9Device device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return device.SetVertexShaderConstants;
    }

    internal Direct3D9PipelineShaderMatrix3x2State(
        Direct3D9SetVertexShaderFloat4Constants setVertexShaderConstants)
    {
        ArgumentNullException.ThrowIfNull(setVertexShaderConstants);
        _setVertexShaderConstants = setVertexShaderConstants;
    }

    internal int SetMatrix3x2(nint shader, uint startRegister, Matrix3x2 matrix)
    {
        ArgumentOutOfRangeException.ThrowIfZero(shader);

        ReadOnlySpan<Vector4> constants =
        [
            new(matrix.M11, matrix.M21, matrix.M31, 0),
            new(matrix.M12, matrix.M22, matrix.M32, 0)
        ];

        return _setVertexShaderConstants(startRegister, constants);
    }
}

internal static class Direct3D9BitmapColorSourceTransform
{
    internal static int Calculate(
        Matrix3x2 bitmapToXSpace,
        uint textureWidth,
        uint textureHeight,
        uint bitmapWidth,
        uint bitmapHeight,
        uint prefilterWidth,
        uint prefilterHeight,
        Direct3D9BitmapRealizationRectangle prefilteredBitmap,
        Direct3D9TexelLayout layoutU,
        Direct3D9TexelLayout layoutV,
        out Matrix3x2 xSpaceToTextureUv)
    {
        ArgumentOutOfRangeException.ThrowIfZero(textureWidth);
        ArgumentOutOfRangeException.ThrowIfZero(textureHeight);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapWidth);
        ArgumentOutOfRangeException.ThrowIfZero(bitmapHeight);
        ArgumentOutOfRangeException.ThrowIfZero(prefilterWidth);
        ArgumentOutOfRangeException.ThrowIfZero(prefilterHeight);

        uint transformTextureWidth = layoutU is Direct3D9TexelLayout.EdgeWrapped or Direct3D9TexelLayout.EdgeMirrored
            ? prefilteredBitmap.Width
            : textureWidth;
        uint transformTextureHeight = layoutV is Direct3D9TexelLayout.EdgeWrapped or Direct3D9TexelLayout.EdgeMirrored
            ? prefilteredBitmap.Height
            : textureHeight;

        Matrix3x2 sourceToPrefiltered = new(
            transformTextureWidth,
            0,
            0,
            transformTextureHeight,
            prefilteredBitmap.Left,
            prefilteredBitmap.Top);
        float widthPrefilterScale = (float) bitmapWidth / prefilterWidth;
        float heightPrefilterScale = (float) bitmapHeight / prefilterHeight;
        Matrix3x2 prefilteredToXSpace = new(
            bitmapToXSpace.M11 * widthPrefilterScale,
            bitmapToXSpace.M12 * widthPrefilterScale,
            bitmapToXSpace.M21 * heightPrefilterScale,
            bitmapToXSpace.M22 * heightPrefilterScale,
            bitmapToXSpace.M31,
            bitmapToXSpace.M32);
        Matrix3x2 sourceToXSpace = sourceToPrefiltered * prefilteredToXSpace;

        if (!Matrix3x2.Invert(sourceToXSpace, out xSpaceToTextureUv))
        {
            xSpaceToTextureUv = default;
            return Direct3D9Factory.NonInvertibleMatrixHResult;
        }

        return Direct3D9Factory.SuccessHResult;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal static unsafe class Direct3D9BitmapColorSourceDeviceStateFactory
{
    internal static int TryCreate(
        Direct3D9Device device,
        IDirect3DBaseTexture9* texture,
        Direct3D9BitmapRealizationProperties realizationProperties,
        Matrix3x2 bitmapToXSpace,
        bool useHardwareTransform,
        uint? shaderTextureTransformRegister,
        Direct3D9SetPipelineShaderMatrix3x2? setShaderMatrix,
        out Direct3D9BitmapColorSourceDeviceState? state)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentOutOfRangeException.ThrowIfZero((nint) texture);

        state = null;
        int result = Direct3D9BitmapColorSourceTransform.Calculate(
            bitmapToXSpace,
            realizationProperties.LayoutU.Length,
            realizationProperties.LayoutV.Length,
            realizationProperties.BitmapWidth,
            realizationProperties.BitmapHeight,
            realizationProperties.Width,
            realizationProperties.Height,
            realizationProperties.SourceContained,
            realizationProperties.LayoutU.TexelLayout,
            realizationProperties.LayoutV.TexelLayout,
            out Matrix3x2 xSpaceToTextureUv);
        if (result < 0)
        {
            return result;
        }

        state = new Direct3D9BitmapColorSourceDeviceState(
            device,
            texture,
            realizationProperties.InterpolationMode,
            realizationProperties.LayoutU.TextureAddress,
            realizationProperties.LayoutV.TextureAddress,
            useHardwareTransform,
            shaderTextureTransformRegister,
            xSpaceToTextureUv,
            setShaderMatrix);
        return Direct3D9Factory.SuccessHResult;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9BitmapColorSourceDeviceState
{
    private readonly Direct3D9Device _device;
    private readonly Direct3D9GetPipelineTexture _getTexture;
    private readonly Direct3D9FilterMode _filterMode;
    private readonly Textureaddress _addressU;
    private readonly Textureaddress _addressV;
    private bool _useHardwareTransform;
    private uint? _shaderTextureTransformRegister;
    private readonly Matrix3x2 _devicePointToTextureUv;
    private readonly Direct3D9SetPipelineShaderMatrix3x2? _setShaderMatrix;

    internal Direct3D9BitmapColorSourceDeviceState(
        Direct3D9Device device,
        IDirect3DBaseTexture9* texture,
        MilBitmapInterpolationMode interpolationMode,
        Textureaddress addressU,
        Textureaddress addressV,
        bool useHardwareTransform,
        uint? shaderTextureTransformRegister,
        Matrix3x2 devicePointToTextureUv,
        Direct3D9SetPipelineShaderMatrix3x2? setShaderMatrix = null)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentOutOfRangeException.ThrowIfZero((nint) texture);

        if (shaderTextureTransformRegister.HasValue && setShaderMatrix is null)
        {
            throw new ArgumentNullException(nameof(setShaderMatrix));
        }

        _device = device;
        _getTexture = () => texture;
        _filterMode = GetFilterMode(device, interpolationMode);
        _addressU = addressU;
        _addressV = addressV;
        _useHardwareTransform = useHardwareTransform;
        _shaderTextureTransformRegister = shaderTextureTransformRegister;
        _devicePointToTextureUv = devicePointToTextureUv;
        _setShaderMatrix = setShaderMatrix;
    }

    internal Direct3D9BitmapColorSourceDeviceState(
        Direct3D9Device device,
        Direct3D9GetPipelineTexture getTexture,
        MilBitmapInterpolationMode interpolationMode,
        Textureaddress addressU,
        Textureaddress addressV,
        bool useHardwareTransform,
        uint? shaderTextureTransformRegister,
        Matrix3x2 devicePointToTextureUv,
        Direct3D9SetPipelineShaderMatrix3x2? setShaderMatrix = null)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(getTexture);
        if (shaderTextureTransformRegister.HasValue && setShaderMatrix is null)
        {
            throw new ArgumentNullException(nameof(setShaderMatrix));
        }

        _device = device;
        _getTexture = getTexture;
        _filterMode = GetFilterMode(device, interpolationMode);
        _addressU = addressU;
        _addressV = addressV;
        _useHardwareTransform = useHardwareTransform;
        _shaderTextureTransformRegister = shaderTextureTransformRegister;
        _devicePointToTextureUv = devicePointToTextureUv;
        _setShaderMatrix = setShaderMatrix;
    }

    internal Direct3D9PipelineColorSource CreatePipelineColorSource(Func<int> realize, bool isOpaque = false)
    {
        ArgumentNullException.ThrowIfNull(realize);

        return new Direct3D9PipelineColorSource(
            Direct3D9ColorSourceType.Texture,
            realize,
            SendDeviceStates,
            SendShaderData,
            ResetForPipelineReuse,
            SetTextureTransformHandle,
            SendVertexMapping,
            IsOpaque: isOpaque);
    }

    internal void ResetForPipelineReuse()
    {
        _shaderTextureTransformRegister = null;
        _useHardwareTransform = false;
    }

    internal void SetTextureTransformHandle(uint shaderTextureTransformRegister)
    {
        if (_setShaderMatrix is null)
        {
            throw new InvalidOperationException("A shader matrix setter is required before assigning a texture transform handle.");
        }

        if (_shaderTextureTransformRegister.HasValue)
        {
            throw new InvalidOperationException("The texture transform handle is already assigned for this pipeline use.");
        }

        _shaderTextureTransformRegister = shaderTextureTransformRegister;
    }

    internal int SendVertexMapping(
        nint vertexBuilder,
        Direct3D9VertexFormatAttribute location,
        Direct3D9SetPipelineTextureMapping? setTextureMapping = null)
    {
        if (location == Direct3D9VertexFormatAttribute.None)
        {
            throw new ArgumentOutOfRangeException(nameof(location));
        }

        _useHardwareTransform = vertexBuilder == 0;
        if (_useHardwareTransform)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        ArgumentNullException.ThrowIfNull(setTextureMapping);
        int coordinateIndex = location switch
        {
            Direct3D9VertexFormatAttribute.Uv1 => 0,
            Direct3D9VertexFormatAttribute.Uv2 or
                (Direct3D9VertexFormatAttribute.Uv2 & ~Direct3D9VertexFormatAttribute.Uv1) => 1,
            Direct3D9VertexFormatAttribute.Uv3 or
                (Direct3D9VertexFormatAttribute.Uv3 & ~Direct3D9VertexFormatAttribute.Uv2) => 2,
            Direct3D9VertexFormatAttribute.Uv4 or
                (Direct3D9VertexFormatAttribute.Uv4 & ~Direct3D9VertexFormatAttribute.Uv3) => 3,
            _ => -1
        };

        return coordinateIndex < 0
            ? Direct3D9Factory.NotImplementedHResult
            : setTextureMapping(vertexBuilder, (uint) coordinateIndex, uint.MaxValue, _devicePointToTextureUv);
    }

    internal int SendDeviceStates(uint stage, uint sampler)
    {
        int result = SetFilterMode(sampler);
        if (result < 0)
        {
            return result;
        }

        result = _device.SetSamplerState(sampler, Samplerstatetype.Addressu, (uint) _addressU);
        if (result < 0)
        {
            return result;
        }

        result = _device.SetSamplerState(sampler, Samplerstatetype.Addressv, (uint) _addressV);
        if (result < 0)
        {
            return result;
        }

        if (_addressU == Textureaddress.Border)
        {
            result = _device.SetSamplerState(sampler, Samplerstatetype.Bordercolor, 0);
            if (result < 0)
            {
                return result;
            }
        }

        result = _device.SetTextureStageState(stage, Texturestagestatetype.Texcoordindex, stage);
        if (result < 0)
        {
            return result;
        }

        if (!_shaderTextureTransformRegister.HasValue)
        {
            if (_useHardwareTransform)
            {
                Matrix4x4 transform = Matrix4x4.Identity;
                transform.M11 = _devicePointToTextureUv.M11;
                transform.M12 = _devicePointToTextureUv.M12;
                transform.M21 = _devicePointToTextureUv.M21;
                transform.M22 = _devicePointToTextureUv.M22;
                transform.M31 = _devicePointToTextureUv.M31;
                transform.M32 = _devicePointToTextureUv.M32;

                result = _device.SetTransform((Transformstatetype) ((uint) Transformstatetype.Texture0 + stage), transform);
                if (result < 0)
                {
                    return result;
                }

                result = _device.SetTextureStageState(
                    stage,
                    Texturestagestatetype.Texturetransformflags,
                    (uint) Texturetransformflags.Count2);
            }
            else
            {
                result = _device.SetTextureStageState(
                    stage,
                    Texturestagestatetype.Texturetransformflags,
                    (uint) Texturetransformflags.Disable);
            }

            if (result < 0)
            {
                return result;
            }
        }

        IDirect3DBaseTexture9* texture = _getTexture();
        if (texture is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        return _device.SetD3DTexture(sampler, texture);
    }

    internal int SendShaderData(nint shader) =>
        _shaderTextureTransformRegister is uint startRegister
            ? _setShaderMatrix!(shader, startRegister, _devicePointToTextureUv)
            : Direct3D9Factory.SuccessHResult;

    private int SetFilterMode(uint sampler)
    {
        int result = _device.SetSamplerState(
            sampler,
            Samplerstatetype.Magfilter,
            (uint) _filterMode.MagnificationFilter);
        if (result < 0)
        {
            return result;
        }

        result = _device.SetSamplerState(
            sampler,
            Samplerstatetype.Minfilter,
            (uint) _filterMode.MinificationFilter);
        if (result < 0)
        {
            return result;
        }

        return _device.SetSamplerState(
            sampler,
            Samplerstatetype.Mipfilter,
            (uint) _filterMode.MipmapFilter);
    }

    private static Direct3D9FilterMode GetFilterMode(
        Direct3D9Device device,
        MilBitmapInterpolationMode interpolationMode) =>
        interpolationMode switch
        {
            MilBitmapInterpolationMode.NearestNeighbor => new(
                Texturefiltertype.Point,
                Texturefiltertype.Point,
                Texturefiltertype.None),
            MilBitmapInterpolationMode.TriLinear => new(
                Texturefiltertype.Linear,
                Texturefiltertype.Linear,
                Texturefiltertype.Linear),
            MilBitmapInterpolationMode.Anisotropic => device.SupportedAnisotropicFilterMode,
            MilBitmapInterpolationMode.Linear or MilBitmapInterpolationMode.Cubic => new(
                Texturefiltertype.Linear,
                Texturefiltertype.Linear,
                Texturefiltertype.None),
            _ => throw new ArgumentOutOfRangeException(nameof(interpolationMode), interpolationMode, null)
        };
}
