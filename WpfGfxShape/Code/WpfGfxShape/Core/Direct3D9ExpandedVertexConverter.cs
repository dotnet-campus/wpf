using System.Numerics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Sequential)]
internal struct Direct3D9ExpandedVertex
{
    internal float X;
    internal float Y;
    internal float Z;
    internal uint Diffuse;
    internal float U0;
    internal float V0;
    internal float U1;
    internal float V1;
    internal float U2;
    internal float V2;
    internal float U3;
    internal float V3;
    internal float U4;
    internal float V4;
    internal float U5;
    internal float V5;
    internal float U6;
    internal float V6;
    internal float U7;
    internal float V7;

    internal readonly Vector2 GetTextureCoordinate(int index) => index switch
    {
        0 => new Vector2(U0, V0),
        1 => new Vector2(U1, V1),
        2 => new Vector2(U2, V2),
        3 => new Vector2(U3, V3),
        4 => new Vector2(U4, V4),
        5 => new Vector2(U5, V5),
        6 => new Vector2(U6, V6),
        7 => new Vector2(U7, V7),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    internal void SetTextureCoordinate(int index, Vector2 value)
    {
        switch (index)
        {
            case 0: U0 = value.X; V0 = value.Y; break;
            case 1: U1 = value.X; V1 = value.Y; break;
            case 2: U2 = value.X; V2 = value.Y; break;
            case 3: U3 = value.X; V3 = value.Y; break;
            case 4: U4 = value.X; V4 = value.Y; break;
            case 5: U5 = value.X; V5 = value.Y; break;
            case 6: U6 = value.X; V6 = value.Y; break;
            case 7: U7 = value.X; V7 = value.Y; break;
            default: throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}

internal sealed class Direct3D9ExpandedVertexConverter
{
    private const uint SupportedAttributes = (uint) Direct3D9VertexFormatAttribute.Xyz |
                                             (uint) Direct3D9VertexFormatAttribute.Diffuse |
                                             (uint) Direct3D9VertexFormatAttribute.Uv8;
    private const uint TextureCoordinateMask = (uint) Direct3D9VertexFormatAttribute.Uv8;

    private readonly Direct3D9VertexFormatAttribute _inputFormat;
    private readonly Direct3D9VertexFormatAttribute _outputFormat;
    private readonly Direct3D9VertexFormatAttribute _generatedFormat;
    private readonly Direct3D9VertexFormatAttribute _antiAliasScaleLocation;
    private readonly Matrix3x2[] _pointToTexture = new Matrix3x2[8];
    private readonly bool[] _textureMappings = new bool[8];
    private Matrix3x2 _positionTransform = Matrix3x2.Identity;
    private float _staticZ;
    private uint _staticDiffuse;
    private bool _hasZMapping;
    private bool _hasDiffuseMapping;
    private bool _finalized;

    private Direct3D9ExpandedVertexConverter(
        Direct3D9VertexFormatAttribute inputFormat,
        Direct3D9VertexFormatAttribute outputFormat,
        Direct3D9VertexFormatAttribute antiAliasScaleLocation)
    {
        _inputFormat = inputFormat;
        _outputFormat = outputFormat;
        _generatedFormat = outputFormat & ~inputFormat;
        _antiAliasScaleLocation = antiAliasScaleLocation;
        UsesFastPath = SelectFastPath();
    }

    internal bool UsesFastPath { get; }

    internal static int Create(
        Direct3D9VertexFormatAttribute inputFormat,
        Direct3D9VertexFormatAttribute outputFormat,
        Direct3D9VertexFormatAttribute antiAliasScaleLocation,
        out Direct3D9ExpandedVertexConverter? converter)
    {
        converter = null;
        uint input = (uint) inputFormat;
        uint output = (uint) outputFormat;
        uint antiAlias = (uint) antiAliasScaleLocation;
        if ((input & (uint) Direct3D9VertexFormatAttribute.Xy) != (uint) Direct3D9VertexFormatAttribute.Xy ||
            (output & (uint) Direct3D9VertexFormatAttribute.Xy) != (uint) Direct3D9VertexFormatAttribute.Xy ||
            (input & ~SupportedAttributes) != 0 ||
            (output & ~SupportedAttributes) != 0 ||
            (antiAlias & ~(uint) Direct3D9VertexFormatAttribute.Diffuse) != 0)
        {
            return Direct3D9Factory.NotImplementedHResult;
        }

        if (!HasCumulativeTextureCoordinates(input) || !HasCumulativeTextureCoordinates(output))
        {
            return Direct3D9Factory.NotImplementedHResult;
        }

        converter = new Direct3D9ExpandedVertexConverter(inputFormat, outputFormat, antiAliasScaleLocation);
        return Direct3D9Factory.SuccessHResult;
    }

    internal void SetPositionTransform(Matrix3x2 positionTransform)
    {
        _positionTransform = positionTransform;
    }

    internal int SetZMapping(float z)
    {
        if ((_generatedFormat & Direct3D9VertexFormatAttribute.Z) == 0 || _hasZMapping)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        _staticZ = z;
        _hasZMapping = true;
        return Direct3D9Factory.SuccessHResult;
    }

    internal int SetConstantDiffuseMapping(uint diffuse)
    {
        if ((_generatedFormat & Direct3D9VertexFormatAttribute.Diffuse) == 0 || _hasDiffuseMapping)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        _staticDiffuse = diffuse;
        _hasDiffuseMapping = true;
        return Direct3D9Factory.SuccessHResult;
    }

    internal int SetTextureMapping(uint destinationCoordinateIndex, uint sourceCoordinateIndex, Matrix3x2 pointToTexture)
    {
        int textureCoordinateCount = GetTextureCoordinateCount(_outputFormat);
        if (destinationCoordinateIndex >= textureCoordinateCount || destinationCoordinateIndex >= 8)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        if (sourceCoordinateIndex != uint.MaxValue)
        {
            return Direct3D9Factory.NotImplementedHResult;
        }

        int index = (int) destinationCoordinateIndex;
        if (_textureMappings[index] || !IsTextureCoordinateGenerated(index))
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        _pointToTexture[index] = pointToTexture;
        _textureMappings[index] = true;
        return Direct3D9Factory.SuccessHResult;
    }

    internal int FinalizeMappings()
    {
        if (_finalized)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        if ((_generatedFormat & Direct3D9VertexFormatAttribute.Z) != 0 && !_hasZMapping)
        {
            _staticZ = 0.5f;
        }

        if ((_generatedFormat & Direct3D9VertexFormatAttribute.Diffuse) != 0 && !_hasDiffuseMapping)
        {
            _staticDiffuse = uint.MaxValue;
        }

        int textureCoordinateCount = GetTextureCoordinateCount(_outputFormat);
        for (int index = 0; index < textureCoordinateCount; index++)
        {
            if (IsTextureCoordinateGenerated(index) && !_textureMappings[index])
            {
                return Direct3D9Factory.NotInitializedHResult;
            }
        }

        _finalized = true;
        return Direct3D9Factory.SuccessHResult;
    }

    internal int ExpandVertices(Span<Direct3D9ExpandedVertex> vertices)
    {
        return Convert(vertices, vertices, transformPosition: false, forceGeneralPath: !UsesFastPath);
    }

    internal int TransferAndExpandVertices(
        ReadOnlySpan<Direct3D9ExpandedVertex> input,
        Span<Direct3D9ExpandedVertex> output,
        bool transformPosition)
    {
        return Convert(input, output, transformPosition, forceGeneralPath: !UsesFastPath);
    }

    internal int TransferAndExpandComplexScanVertices(
        ReadOnlySpan<Direct3D9ComplexScanVertex> input,
        Span<Direct3D9ExpandedVertex> output,
        bool transformPosition)
    {
        if (!_finalized)
        {
            return Direct3D9Factory.NotInitializedHResult;
        }

        if (output.Length < input.Length)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        for (int index = 0; index < input.Length; index++)
        {
            WriteComplexScanVertex(input[index], ref output[index], transformPosition);
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal int TransferAndExpandComplexScanVertex(
        Direct3D9ComplexScanVertex input,
        ref Direct3D9ExpandedVertex output,
        bool transformPosition)
    {
        if (!_finalized)
        {
            return Direct3D9Factory.NotInitializedHResult;
        }

        WriteComplexScanVertex(input, ref output, transformPosition);
        return Direct3D9Factory.SuccessHResult;
    }

    internal int TransferAndExpandVerticesGeneral(
        ReadOnlySpan<Direct3D9ExpandedVertex> input,
        Span<Direct3D9ExpandedVertex> output,
        bool transformPosition)
    {
        return Convert(input, output, transformPosition, forceGeneralPath: true);
    }

    private void WriteComplexScanVertex(
        Direct3D9ComplexScanVertex input,
        ref Direct3D9ExpandedVertex output,
        bool transformPosition)
    {
        Direct3D9ExpandedVertex basicVertex = new()
        {
            X = input.X,
            Y = input.Y,
            Diffuse = BitConverter.SingleToUInt32Bits(input.Alpha),
        };
        ConvertVertex(in basicVertex, ref output, transformPosition);
    }

    private int Convert(
        ReadOnlySpan<Direct3D9ExpandedVertex> input,
        Span<Direct3D9ExpandedVertex> output,
        bool transformPosition,
        bool forceGeneralPath)
    {
        if (!_finalized)
        {
            return Direct3D9Factory.NotInitializedHResult;
        }

        if (output.Length < input.Length)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        if (UsesFastPath && !forceGeneralPath)
        {
            ConvertCore(input, output, transformPosition);
        }
        else
        {
            ConvertCore(input, output, transformPosition);
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private void ConvertCore(
        ReadOnlySpan<Direct3D9ExpandedVertex> input,
        Span<Direct3D9ExpandedVertex> output,
        bool transformPosition)
    {
        for (int index = 0; index < input.Length; index++)
        {
            ConvertVertex(in input[index], ref output[index], transformPosition);
        }
    }

    private void ConvertVertex(
        in Direct3D9ExpandedVertex input,
        ref Direct3D9ExpandedVertex output,
        bool transformPosition)
    {
        float x = input.X;
        float y = input.Y;
        if (transformPosition)
        {
            Vector2 transformed = Vector2.Transform(new Vector2(x, y), _positionTransform);
            x = transformed.X;
            y = transformed.Y;
        }

        output.X = x;
        output.Y = y;
        output.Z = (_generatedFormat & Direct3D9VertexFormatAttribute.Z) != 0 ? _staticZ : input.Z;
        output.Diffuse = (_generatedFormat & Direct3D9VertexFormatAttribute.Diffuse) != 0
            ? GenerateDiffuse(input.Diffuse)
            : input.Diffuse;

        int textureCoordinateCount = GetTextureCoordinateCount(_outputFormat);
        Vector2 position = new(x, y);
        for (int coordinateIndex = 0; coordinateIndex < textureCoordinateCount; coordinateIndex++)
        {
            Vector2 coordinate = IsTextureCoordinateGenerated(coordinateIndex)
                ? Vector2.Transform(position, _pointToTexture[coordinateIndex])
                : input.GetTextureCoordinate(coordinateIndex);
            output.SetTextureCoordinate(coordinateIndex, coordinate);
        }
    }

    private uint GenerateDiffuse(uint inputDiffuse)
    {
        uint diffuse = _hasDiffuseMapping ? _staticDiffuse : uint.MaxValue;
        if ((_antiAliasScaleLocation & Direct3D9VertexFormatAttribute.Diffuse) == 0)
        {
            return diffuse;
        }

        if (inputDiffuse == 0)
        {
            return 0;
        }

        if (inputDiffuse == 0x3F800000)
        {
            return diffuse;
        }

        float falloff = BitConverter.UInt32BitsToSingle(inputDiffuse);
        uint coverage = (uint) MathF.Floor((falloff * 256f) + 0.5f);
        if (coverage > 255)
        {
            return diffuse;
        }

        uint aaGg = (((diffuse & 0xFF000000) >> 8) | ((diffuse >> 8) & 0xFF)) * coverage;
        uint rrBb = ((diffuse & 0x00FF0000) | (diffuse & 0xFF)) * coverage;
        return ((aaGg + 0x00800080) & 0xFF00FF00) |
               (((rrBb + 0x00800080) >> 8) & 0x00FF00FF);
    }

    private bool SelectFastPath()
    {
        Direct3D9VertexFormatAttribute fastSupport = Direct3D9VertexFormatAttribute.Z |
                                                      Direct3D9VertexFormatAttribute.Diffuse |
                                                      Direct3D9VertexFormatAttribute.Uv1;
        if ((_generatedFormat & ~fastSupport) == 0)
        {
            return true;
        }

        return _generatedFormat == (Direct3D9VertexFormatAttribute.Z | Direct3D9VertexFormatAttribute.Uv8) &&
               (_inputFormat == Direct3D9VertexFormatAttribute.Xy ||
                (_inputFormat == (Direct3D9VertexFormatAttribute.Xy | Direct3D9VertexFormatAttribute.Diffuse) &&
                 _antiAliasScaleLocation == Direct3D9VertexFormatAttribute.Diffuse));
    }

    private bool IsTextureCoordinateGenerated(int index)
    {
        uint coordinateBit = 0x100u << index;
        return ((uint) _generatedFormat & coordinateBit) != 0;
    }

    private static bool HasCumulativeTextureCoordinates(uint format)
    {
        uint textureCoordinates = format & TextureCoordinateMask;
        return textureCoordinates == 0 || (textureCoordinates & (textureCoordinates + 0x100u)) == 0;
    }

    private static int GetTextureCoordinateCount(Direct3D9VertexFormatAttribute format)
    {
        uint textureCoordinates = (uint) format & TextureCoordinateMask;
        int count = 0;
        for (uint coordinateBit = 0x100u; (textureCoordinates & coordinateBit) != 0; coordinateBit <<= 1)
        {
            count++;
        }

        return count;
    }
}
