using System.Numerics;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

[Flags]
internal enum Direct3D9ColorSourceType
{
    Constant = 1,
    Texture = 2,
    PrecomputedComponent = 4,
    Programmatic = 8,
}

internal delegate int Direct3D9SendPipelineVertexMapping(
    nint vertexBuilder,
    Direct3D9VertexFormatAttribute location,
    Direct3D9SetPipelineTextureMapping? setTextureMapping);

internal delegate int Direct3D9SetPipelineConstantMapping(
    nint vertexBuilder,
    Direct3D9VertexFormatAttribute location,
    uint premultipliedSrgbColor);

internal delegate int Direct3D9SendPipelineConstantVertexMapping(
    nint vertexBuilder,
    Direct3D9VertexFormatAttribute location,
    Direct3D9SetPipelineConstantMapping? setConstantMapping);

internal sealed record Direct3D9PipelineColorSource(
    Direct3D9ColorSourceType SourceType,
    Func<int> Realize,
    Func<uint, uint, int>? SendDeviceStates = null,
    Func<nint, int>? SendShaderData = null,
    Action? ResetForPipelineReuse = null,
    Action<uint>? SetTextureTransformHandle = null,
    Direct3D9SendPipelineVertexMapping? SendVertexMapping = null,
    Action<float>? AlphaScale = null,
    Direct3D9SendPipelineConstantVertexMapping? SendConstantVertexMapping = null,
    bool IsOpaque = false);

internal delegate int Direct3D9CreateSolidColorTextureSource(
    MilColorF color,
    out Direct3D9BitmapPipelineColorSource? colorSource);

internal delegate int Direct3D9SetPipelineShaderFloat4(
    nint shader,
    uint handle,
    Vector4 value);

internal sealed class Direct3D9ConstantColorSource : IDisposable
{
    private readonly MilColorF _color;
    private readonly Direct3D9CreateSolidColorTextureSource? _createSolidColorTextureSource;
    private readonly Direct3D9SetPipelineShaderFloat4? _setShaderFloat4;
    private Direct3D9BitmapPipelineColorSource? _texturedColorSource;
    private uint? _colorShaderHandle;
    private bool _disposed;

    internal Direct3D9ConstantColorSource(
        MilColorF color,
        Direct3D9CreateSolidColorTextureSource? createSolidColorTextureSource = null,
        Direct3D9SetPipelineShaderFloat4? setShaderFloat4 = null)
    {
        _color = color;
        _createSolidColorTextureSource = createSolidColorTextureSource;
        _setShaderFloat4 = setShaderFloat4;
        PipelineColorSource = new Direct3D9PipelineColorSource(
            Direct3D9ColorSourceType.Constant | Direct3D9ColorSourceType.Texture,
            Realize,
            SendDeviceStates,
            SendShaderData,
            ResetForPipelineReuse,
            SendVertexMapping: SendTextureVertexMapping,
            SendConstantVertexMapping: SendConstantVertexMapping);
        PrimaryColorSource = new Direct3D9PrimaryColorSource(sender => sender.SetConstant(this));
    }

    internal Direct3D9PipelineColorSource PipelineColorSource { get; }

    internal Direct3D9PrimaryColorSource PrimaryColorSource { get; }

    internal void SetColorShaderHandle(uint handle)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_colorShaderHandle.HasValue)
        {
            throw new InvalidOperationException("The color shader handle has already been set.");
        }

        _colorShaderHandle = handle;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _texturedColorSource?.Dispose();
        _texturedColorSource = null;
    }

    private int Realize()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _texturedColorSource?.PipelineColorSource.Realize() ?? Direct3D9Factory.SuccessHResult;
    }

    private int SendDeviceStates(uint stage, uint sampler)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (sampler == uint.MaxValue)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        return _texturedColorSource?.PipelineColorSource.SendDeviceStates?.Invoke(stage, sampler)
            ?? Direct3D9Factory.InvalidCallHResult;
    }

    private int SendShaderData(nint shader)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_colorShaderHandle is not uint handle)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        if (shader == 0 || _setShaderFloat4 is null)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        float alpha = Math.Clamp(_color.Alpha, 0, 1);
        Vector4 color = new(
            ConvertScRgbToSrgb(_color.Red) * alpha,
            ConvertScRgbToSrgb(_color.Green) * alpha,
            ConvertScRgbToSrgb(_color.Blue) * alpha,
            alpha);
        return _setShaderFloat4(shader, handle, color);
    }

    private void ResetForPipelineReuse()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _colorShaderHandle = null;
    }

    private int SendConstantVertexMapping(
        nint vertexBuilder,
        Direct3D9VertexFormatAttribute location,
        Direct3D9SetPipelineConstantMapping? setConstantMapping)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (location is not Direct3D9VertexFormatAttribute.Diffuse and not Direct3D9VertexFormatAttribute.Specular)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        if (vertexBuilder == 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        if (setConstantMapping is null)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        return setConstantMapping(vertexBuilder, location, ConvertToPremultipliedSrgb(_color));
    }

    private int SendTextureVertexMapping(
        nint vertexBuilder,
        Direct3D9VertexFormatAttribute location,
        Direct3D9SetPipelineTextureMapping? setTextureMapping)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (location != Direct3D9VertexFormatAttribute.Uv1)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        if (_texturedColorSource is null)
        {
            if (_createSolidColorTextureSource is null)
            {
                return Direct3D9Factory.UnsupportedOperationHResult;
            }

            Direct3D9BitmapPipelineColorSource? colorSource;
            int result;
            try
            {
                result = _createSolidColorTextureSource(_color, out colorSource);
            }
            catch (OutOfMemoryException)
            {
                return Direct3D9Factory.OutOfMemoryHResult;
            }

            if (result < 0)
            {
                colorSource?.Dispose();
                return result;
            }

            if (colorSource is null)
            {
                return Direct3D9Factory.OutOfMemoryHResult;
            }

            _texturedColorSource = colorSource;
        }

        Direct3D9SendPipelineVertexMapping? sendVertexMapping = _texturedColorSource.PipelineColorSource.SendVertexMapping;
        return sendVertexMapping is null
            ? Direct3D9Factory.InvalidCallHResult
            : sendVertexMapping(vertexBuilder, location, setTextureMapping);
    }

    private static uint ConvertToPremultipliedSrgb(MilColorF color)
    {
        float alpha = Math.Clamp(color.Alpha, 0, 1);
        byte alphaByte = ConvertToByte(alpha);
        byte red = ConvertToByte(ConvertScRgbToSrgb(color.Red) * alpha);
        byte green = ConvertToByte(ConvertScRgbToSrgb(color.Green) * alpha);
        byte blue = ConvertToByte(ConvertScRgbToSrgb(color.Blue) * alpha);
        return (uint) (alphaByte << 24 | red << 16 | green << 8 | blue);
    }

    private static float ConvertScRgbToSrgb(float value)
    {
        if (!(value > 0))
        {
            return 0;
        }

        if (value >= 1)
        {
            return 1;
        }

        const float lookupScale = 3354;
        float lookupValue = MathF.Floor((lookupScale * value) + 0.5f) / lookupScale;
        return lookupValue <= 0.0031308f
            ? lookupValue * 12.92f
            : (1.055f * MathF.Pow(lookupValue, 1f / 2.4f)) - 0.055f;
    }

    private static byte ConvertToByte(float value) =>
        (byte) Math.Clamp(MathF.Floor((value * byte.MaxValue) + 0.5f), 0, byte.MaxValue);
}

internal sealed class Direct3D9LightingColorSource : IDisposable
{
    private readonly Action? _release;
    private bool _disposed;

    internal Direct3D9LightingColorSource(Action? release = null)
    {
        _release = release;
        PipelineColorSource = new Direct3D9PipelineColorSource(
            Direct3D9ColorSourceType.Programmatic,
            Realize,
            SendDeviceStates,
            SendVertexMapping: SendVertexMapping);
    }

    internal Direct3D9PipelineColorSource PipelineColorSource { get; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _release?.Invoke();
    }

    private int Realize()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Direct3D9Factory.SuccessHResult;
    }

    private int SendDeviceStates(uint stage, uint sampler)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Direct3D9Factory.SuccessHResult;
    }

    private int SendVertexMapping(
        nint vertexBuilder,
        Direct3D9VertexFormatAttribute location,
        Direct3D9SetPipelineTextureMapping? setTextureMapping)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Direct3D9Factory.SuccessHResult;
    }
}

internal sealed class Direct3D9ConstantAlphaScalableColorSource : IDisposable
{
    private readonly Direct3D9SetPipelineShaderFloat4? _setShaderFloat4;
    private float _alpha;
    private uint? _shaderAlphaHandle;
    private bool _disposed;

    internal Direct3D9ConstantAlphaScalableColorSource(
        float alpha,
        Direct3D9SetPipelineShaderFloat4? setShaderFloat4 = null)
    {
        ValidateAlpha(alpha);
        _alpha = alpha;
        _setShaderFloat4 = setShaderFloat4;
        PipelineColorSource = new Direct3D9PipelineColorSource(
            Direct3D9ColorSourceType.Constant,
            Realize,
            SendDeviceStates,
            SendShaderData,
            ResetForPipelineReuse,
            AlphaScale: AlphaScale,
            SendConstantVertexMapping: SendVertexMapping);
    }

    internal Direct3D9PipelineColorSource PipelineColorSource { get; }

    internal MilColorF GetColor()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new(_alpha, 1, 1, 1);
    }

    internal void SetShaderAlphaHandle(uint handle)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_shaderAlphaHandle.HasValue)
        {
            throw new InvalidOperationException("The alpha shader handle has already been set.");
        }

        _shaderAlphaHandle = handle;
    }

    public void Dispose()
    {
        _disposed = true;
    }

    private int Realize()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Direct3D9Factory.SuccessHResult;
    }

    private int SendDeviceStates(uint stage, uint sampler)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return Direct3D9Factory.SuccessHResult;
    }

    private int SendShaderData(nint shader)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_shaderAlphaHandle is not uint handle)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        if (shader == 0 || _setShaderFloat4 is null)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        return _setShaderFloat4(shader, handle, new Vector4(1, 1, 1, _alpha));
    }

    private void ResetForPipelineReuse()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _shaderAlphaHandle = null;
    }

    private void AlphaScale(float alphaScale)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ValidateAlpha(alphaScale);
        _alpha *= alphaScale;
    }

    private int SendVertexMapping(
        nint vertexBuilder,
        Direct3D9VertexFormatAttribute location,
        Direct3D9SetPipelineConstantMapping? setConstantMapping)
    {
        if (location is not Direct3D9VertexFormatAttribute.Diffuse and not Direct3D9VertexFormatAttribute.Specular)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        if (vertexBuilder == 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        if (setConstantMapping is null)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        byte channel = (byte) Math.Clamp(MathF.Floor((_alpha * byte.MaxValue) + 0.5f), 0, byte.MaxValue);
        uint color = (uint) (channel << 24 | channel << 16 | channel << 8 | channel);
        return setConstantMapping(vertexBuilder, location, color);
    }

    private static void ValidateAlpha(float alpha)
    {
        if (!float.IsFinite(alpha) || alpha < 0 || alpha > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(alpha));
        }
    }
}

internal enum Direct3D9ShaderTransparencyEffect
{
    NoTransparency,
    HasTransparency,
    BlendsColorSource,
}

internal enum Direct3D9ShaderTextureFunction
{
    MultiplyTextureNoTransformFromTextureCoordinate,
    MultiplyTextureTransformFromVertexUv,
}

internal enum Direct3D9ShaderConstantFunction
{
    MultiplyConstant,
    MultiplyByInputDiffuse,
    MultiplyByInputDiffuseNonPremultipliedInput,
    MultiplyAlpha,
    MultiplyAlphaNonPremultiplied,
}

internal enum Direct3D9ShaderAlphaMultiplyOperation
{
    Multiply,
    MultiplyAlphaOnly,
}

internal sealed record Direct3D9ShaderPipelineItem
{
    internal Direct3D9ShaderPipelineItem(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null,
        Direct3D9ShaderTransparencyEffect transparencyEffect = Direct3D9ShaderTransparencyEffect.NoTransparency,
        Direct3D9ShaderTextureFunction? textureFunction = null,
        bool takeOwnership = false)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle, transparencyEffect, textureFunction)
    {
        Ownership = takeOwnership ? colorSource : colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItem(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null,
        Direct3D9ShaderTransparencyEffect transparencyEffect = Direct3D9ShaderTransparencyEffect.NoTransparency,
        Direct3D9ShaderTextureFunction? textureFunction = null,
        Direct3D9ShaderConstantFunction? constantFunction = null,
        IDisposable? lifetimeOwner = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;
        TransparencyEffect = transparencyEffect;
        TextureFunction = textureFunction;
        ConstantFunction = constantFunction;
        LifetimeOwner = lifetimeOwner;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal IDisposable? LifetimeOwner { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }

    internal Direct3D9ShaderTransparencyEffect TransparencyEffect { get; }

    internal Direct3D9ShaderTextureFunction? TextureFunction { get; }

    internal Direct3D9ShaderConstantFunction? ConstantFunction { get; }
}

internal static class Direct3D9PipelineCompositionFinalizer
{
    internal static MilCompositingMode FinalizeFixedFunction(
        MilCompositingMode compositingMode,
        bool antiAliasUsed,
        Direct3D9FinalizedFixedFunctionPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        if (compositingMode != MilCompositingMode.SourceOver ||
            antiAliasUsed ||
            pipeline.FirstUnusedStage != 1 ||
            pipeline.Items.Count == 0)
        {
            return compositingMode;
        }

        Direct3D9FixedFunctionPipelineItem primaryItem = pipeline.Items[0];
        bool sourceIsOpaque = primaryItem.BlendOperation == Direct3D9FixedFunctionBlendOperation.SelectSource &&
            primaryItem.ColorSource?.IsOpaque == true;
        bool ignoresSourceAlpha =
            primaryItem.BlendOperation == Direct3D9FixedFunctionBlendOperation.SelectSourceColorIgnoreAlpha;

        return sourceIsOpaque || ignoresSourceAlpha
            ? MilCompositingMode.SourceCopy
            : compositingMode;
    }

    internal static MilCompositingMode FinalizeShader(
        MilCompositingMode compositingMode,
        bool antiAliasUsed,
        IReadOnlyList<Direct3D9ShaderPipelineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (compositingMode != MilCompositingMode.SourceOver || antiAliasUsed)
        {
            return compositingMode;
        }

        foreach (Direct3D9ShaderPipelineItem item in items)
        {
            if (item.TransparencyEffect == Direct3D9ShaderTransparencyEffect.HasTransparency ||
                item.TransparencyEffect == Direct3D9ShaderTransparencyEffect.BlendsColorSource &&
                item.ColorSource?.IsOpaque == false)
            {
                return compositingMode;
            }
        }

        return MilCompositingMode.SourceCopy;
    }
}

#if false
internal sealed record Direct3D9ShaderPipelineItemRemoved
{
    internal Direct3D9ShaderPipelineItemRemoved(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemRemoved(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemOriginal
{
    internal Direct3D9ShaderPipelineItemOriginal(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemOriginal(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemOld
{
    internal Direct3D9ShaderPipelineItemOld(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemOld(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemUnused
{
    internal Direct3D9ShaderPipelineItemUnused(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemUnused(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemPlaceholder
{
    internal Direct3D9ShaderPipelineItemPlaceholder(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemPlaceholder(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemDeprecated
{
    internal Direct3D9ShaderPipelineItemDeprecated(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemDeprecated(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemBackup
{
    internal Direct3D9ShaderPipelineItemBackup(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemBackup(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItem2
{
    internal Direct3D9ShaderPipelineItem2(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItem2(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItem1
{
    internal Direct3D9ShaderPipelineItem1(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItem1(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItem0
{
    internal Direct3D9ShaderPipelineItem0(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItem0(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemX
{
    internal Direct3D9ShaderPipelineItemX(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemX(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemY
{
    internal Direct3D9ShaderPipelineItemY(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemY(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemZ
{
    internal Direct3D9ShaderPipelineItemZ(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemZ(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemA
{
    internal Direct3D9ShaderPipelineItemA(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemA(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemB
{
    internal Direct3D9ShaderPipelineItemB(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemB(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemC
{
    internal Direct3D9ShaderPipelineItemC(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemC(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemD
{
    internal Direct3D9ShaderPipelineItemD(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemD(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemE
{
    internal Direct3D9ShaderPipelineItemE(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemE(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemF
{
    internal Direct3D9ShaderPipelineItemF(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemF(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemG
{
    internal Direct3D9ShaderPipelineItemG(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemG(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemH
{
    internal Direct3D9ShaderPipelineItemH(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemH(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemI
{
    internal Direct3D9ShaderPipelineItemI(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemI(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemJ
{
    internal Direct3D9ShaderPipelineItemJ(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemJ(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemK
{
    internal Direct3D9ShaderPipelineItemK(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemK(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemL
{
    internal Direct3D9ShaderPipelineItemL(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemL(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemM
{
    internal Direct3D9ShaderPipelineItemM(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemM(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemN
{
    internal Direct3D9ShaderPipelineItemN(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemN(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemO
{
    internal Direct3D9ShaderPipelineItemO(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemO(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemP
{
    internal Direct3D9ShaderPipelineItemP(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemP(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemQ
{
    internal Direct3D9ShaderPipelineItemQ(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemQ(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemR
{
    internal Direct3D9ShaderPipelineItemR(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemR(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemS
{
    internal Direct3D9ShaderPipelineItemS(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemS(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemT
{
    internal Direct3D9ShaderPipelineItemT(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemT(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemU
{
    internal Direct3D9ShaderPipelineItemU(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemU(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemV
{
    internal Direct3D9ShaderPipelineItemV(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemV(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemW
{
    internal Direct3D9ShaderPipelineItemW(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemW(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemV2
{
    internal Direct3D9ShaderPipelineItemV2(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemV2(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemV3
{
    internal Direct3D9ShaderPipelineItemV3(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemV3(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemV4
{
    internal Direct3D9ShaderPipelineItemV4(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemV4(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemV5
{
    internal Direct3D9ShaderPipelineItemV5(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemV5(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemV6
{
    internal Direct3D9ShaderPipelineItemV6(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemV6(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemV7
{
    internal Direct3D9ShaderPipelineItemV7(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemV7(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemV8
{
    internal Direct3D9ShaderPipelineItemV8(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemV8(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemV9
{
    internal Direct3D9ShaderPipelineItemV9(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemV9(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemFinal
{
    internal Direct3D9ShaderPipelineItemFinal(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemFinal(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemCopy
{
    internal Direct3D9ShaderPipelineItemCopy(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemCopy(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemTemp
{
    internal Direct3D9ShaderPipelineItemTemp(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemTemp(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemReplacement
{
    internal Direct3D9ShaderPipelineItemReplacement(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemReplacement(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemNew
{
    internal Direct3D9ShaderPipelineItemNew(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemNew(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemCurrent
{
    internal Direct3D9ShaderPipelineItemCurrent(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemCurrent(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemReal
{
    internal Direct3D9ShaderPipelineItemReal(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemReal(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

internal sealed record Direct3D9ShaderPipelineItemActual
{
    internal Direct3D9ShaderPipelineItemActual(
        uint sampler,
        Direct3D9BitmapPipelineColorSource colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
        : this(sampler, colorSource.PipelineColorSource, textureCoordinates, textureTransformHandle)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9ShaderPipelineItemActual(
        uint sampler,
        Direct3D9PipelineColorSource? colorSource,
        Direct3D9VertexFormatAttribute textureCoordinates = Direct3D9VertexFormatAttribute.None,
        uint? textureTransformHandle = null)
    {
        if (textureTransformHandle.HasValue && colorSource?.SetTextureTransformHandle is null)
        {
            throw new ArgumentException("The color source does not accept a texture transform handle.", nameof(colorSource));
        }

        Sampler = sampler;
        ColorSource = colorSource;
        TextureCoordinates = textureCoordinates;

        colorSource?.ResetForPipelineReuse?.Invoke();
        if (textureTransformHandle is uint handle)
        {
            colorSource!.SetTextureTransformHandle!(handle);
        }
    }

    internal uint Sampler { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal Direct3D9VertexFormatAttribute TextureCoordinates { get; }
}

#endif

internal enum Direct3D9FixedFunctionBlendOperation
{
    Nop = -1,
    SelectSource,
    Multiply,
    SelectSourceColorIgnoreAlpha,
    MultiplyColorIgnoreAlpha,
    BumpMap,
    MultiplyByAlpha,
    MultiplyAlphaOnly,
}

internal enum Direct3D9FixedFunctionBlendArgument
{
    None,
    Current,
    Diffuse,
    Specular,
    Texture,
}

internal sealed record Direct3D9FixedFunctionPipelineItem
{
    internal Direct3D9FixedFunctionPipelineItem(
        Direct3D9VertexFormatAttribute sourceLocation,
        Direct3D9BitmapPipelineColorSource colorSource,
        uint stage = 0,
        uint sampler = uint.MaxValue,
        Direct3D9FixedFunctionBlendOperation blendOperation = Direct3D9FixedFunctionBlendOperation.Nop,
        Direct3D9FixedFunctionBlendArgument source1 = Direct3D9FixedFunctionBlendArgument.None,
        Direct3D9FixedFunctionBlendArgument source2 = Direct3D9FixedFunctionBlendArgument.None)
        : this(sourceLocation, colorSource.PipelineColorSource, stage, sampler, blendOperation, source1, source2)
    {
        Ownership = colorSource.AddRef();
    }

    internal Direct3D9FixedFunctionPipelineItem(
        Direct3D9VertexFormatAttribute sourceLocation,
        Direct3D9PipelineColorSource? colorSource,
        uint stage = 0,
        uint sampler = uint.MaxValue,
        Direct3D9FixedFunctionBlendOperation blendOperation = Direct3D9FixedFunctionBlendOperation.Nop,
        Direct3D9FixedFunctionBlendArgument source1 = Direct3D9FixedFunctionBlendArgument.None,
        Direct3D9FixedFunctionBlendArgument source2 = Direct3D9FixedFunctionBlendArgument.None,
        IDisposable? lifetimeOwner = null)
    {
        SourceLocation = sourceLocation;
        ColorSource = colorSource;
        Stage = stage;
        Sampler = sampler;
        BlendOperation = blendOperation;
        Source1 = source1;
        Source2 = source2;
        LifetimeOwner = lifetimeOwner;
        colorSource?.ResetForPipelineReuse?.Invoke();
    }

    internal Direct3D9VertexFormatAttribute SourceLocation { get; }

    internal Direct3D9PipelineColorSource? ColorSource { get; }

    internal Direct3D9BitmapPipelineColorSource? Ownership { get; }

    internal IDisposable? LifetimeOwner { get; }

    internal uint Stage { get; init; }

    internal uint Sampler { get; }

    internal Direct3D9FixedFunctionBlendOperation BlendOperation { get; init; }

    internal Direct3D9FixedFunctionBlendArgument Source1 { get; init; }

    internal Direct3D9FixedFunctionBlendArgument Source2 { get; init; }
}

internal sealed record Direct3D9FinalizedFixedFunctionPipeline(
    IReadOnlyList<Direct3D9FixedFunctionPipelineItem> Items,
    uint FirstUnusedStage);

internal static class Direct3D9FixedFunctionPipelineFinalizer
{
    internal static Direct3D9FinalizedFixedFunctionPipeline FinalizeBlendOperations(
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("The fixed-function pipeline requires a primary operation.", nameof(items));
        }

        Direct3D9FixedFunctionPipelineItem[] finalizedItems = [.. items];
        uint firstUnusedStage = finalizedItems.Max(static item => item.Stage) + 1;
        if (firstUnusedStage == 1)
        {
            return new Direct3D9FinalizedFixedFunctionPipeline(finalizedItems, firstUnusedStage);
        }

        int firstNonTextureStage = -1;
        for (int index = 0; index < finalizedItems.Length; index++)
        {
            if (finalizedItems[index].Source1 != Direct3D9FixedFunctionBlendArgument.Texture)
            {
                firstNonTextureStage = index;
                break;
            }
        }

        if (firstNonTextureStage <= 0)
        {
            return new Direct3D9FinalizedFixedFunctionPipeline(finalizedItems, firstUnusedStage);
        }

        Direct3D9FixedFunctionPipelineItem firstItem = finalizedItems[0];
        Direct3D9FixedFunctionPipelineItem collapsibleItem = finalizedItems[firstNonTextureStage];
        Direct3D9FixedFunctionBlendOperation combinedOperation =
            firstItem.BlendOperation == Direct3D9FixedFunctionBlendOperation.SelectSourceColorIgnoreAlpha
                ? Direct3D9FixedFunctionBlendOperation.MultiplyColorIgnoreAlpha
                : collapsibleItem.BlendOperation;

        finalizedItems[0] = firstItem with
        {
            BlendOperation = combinedOperation,
            Source1 = collapsibleItem.Source1,
            Source2 = firstItem.Source1,
        };
        finalizedItems[firstNonTextureStage] = collapsibleItem with
        {
            BlendOperation = Direct3D9FixedFunctionBlendOperation.Nop,
        };

        for (int index = firstNonTextureStage; index < finalizedItems.Length; index++)
        {
            finalizedItems[index] = finalizedItems[index] with
            {
                Stage = finalizedItems[index].Stage - 1,
            };
        }

        return new Direct3D9FinalizedFixedFunctionPipeline(finalizedItems, firstUnusedStage - 1);
    }
}

internal readonly record struct Direct3D9FixedFunctionTextureStageOperation(
    bool UsesTexture,
    Textureop ColorOperation,
    uint ColorArgument1,
    uint ColorArgument2,
    Textureop AlphaOperation,
    uint AlphaArgument1,
    uint AlphaArgument2);

internal static class Direct3D9FixedFunctionTextureStageOperations
{
    private const uint Current = 1;
    private const uint Diffuse = 0;
    private const uint Texture = 2;
    private const uint AlphaReplicate = 0x20;

    private static readonly Direct3D9FixedFunctionTextureStageOperation DiffuseSource =
        new(false, Textureop.Selectarg1, Diffuse, Current, Textureop.Selectarg1, Diffuse, Current);
    private static readonly Direct3D9FixedFunctionTextureStageOperation SelectTexture =
        new(true, Textureop.Selectarg1, Texture, Current, Textureop.Selectarg1, Texture, Current);
    private static readonly Direct3D9FixedFunctionTextureStageOperation PremultipliedTextureTimesCurrent =
        new(true, Textureop.Modulate, Texture, Current, Textureop.Modulate, Texture, Current);
    private static readonly Direct3D9FixedFunctionTextureStageOperation PremultipliedTextureTimesDiffuse =
        new(true, Textureop.Modulate, Texture, Diffuse, Textureop.Modulate, Texture, Current);
    private static readonly Direct3D9FixedFunctionTextureStageOperation OpaqueTextureTimesCurrent =
        new(true, Textureop.Modulate, Texture, Current, Textureop.Selectarg1, Current, Current);
    private static readonly Direct3D9FixedFunctionTextureStageOperation OpaqueTextureTimesDiffuse =
        new(true, Textureop.Modulate, Texture, Diffuse, Textureop.Selectarg1, Current, Current);
    private static readonly Direct3D9FixedFunctionTextureStageOperation MaskTextureTimesCurrent =
        new(true, Textureop.Modulate, Texture | AlphaReplicate, Current, Textureop.Modulate, Texture, Current);
    private static readonly Direct3D9FixedFunctionTextureStageOperation BumpMapTexture =
        new(true, Textureop.Bumpenvmap, Texture, Diffuse, Textureop.Modulate, Texture, Current);
    private static readonly Direct3D9FixedFunctionTextureStageOperation SelectTextureColorMultiplyDiffuseAlpha =
        new(true, Textureop.Selectarg1, Texture, Diffuse, Textureop.Modulate, Texture, Diffuse);
    private static readonly Direct3D9FixedFunctionTextureStageOperation SelectTextureColorMultiplyCurrentAlpha =
        new(true, Textureop.Selectarg1, Texture, Current, Textureop.Modulate, Texture, Current);
    private static readonly Direct3D9FixedFunctionTextureStageOperation SelectDiffuseColorMultiplyTextureAlpha =
        new(true, Textureop.Selectarg2, Texture, Diffuse, Textureop.Modulate, Texture, Diffuse);
    private static readonly Direct3D9FixedFunctionTextureStageOperation SelectCurrentColorMultiplyTextureAlpha =
        new(true, Textureop.Selectarg2, Texture, Current, Textureop.Modulate, Texture, Current);

    internal static bool TryGet(
        Direct3D9FixedFunctionBlendOperation operation,
        Direct3D9FixedFunctionBlendArgument source1,
        Direct3D9FixedFunctionBlendArgument source2,
        out Direct3D9FixedFunctionTextureStageOperation stageOperation)
    {
        Direct3D9FixedFunctionTextureStageOperation? result = (operation, source1, source2) switch
        {
            (Direct3D9FixedFunctionBlendOperation.SelectSource, Direct3D9FixedFunctionBlendArgument.Diffuse, Direct3D9FixedFunctionBlendArgument.None) => DiffuseSource,
            (Direct3D9FixedFunctionBlendOperation.SelectSource, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendArgument.None) => SelectTexture,
            (Direct3D9FixedFunctionBlendOperation.Multiply, Direct3D9FixedFunctionBlendArgument.Current, Direct3D9FixedFunctionBlendArgument.Texture) => PremultipliedTextureTimesCurrent,
            (Direct3D9FixedFunctionBlendOperation.Multiply, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendArgument.Current) => PremultipliedTextureTimesCurrent,
            (Direct3D9FixedFunctionBlendOperation.Multiply, Direct3D9FixedFunctionBlendArgument.Diffuse, Direct3D9FixedFunctionBlendArgument.Texture) => PremultipliedTextureTimesDiffuse,
            (Direct3D9FixedFunctionBlendOperation.Multiply, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendArgument.Diffuse) => PremultipliedTextureTimesDiffuse,
            (Direct3D9FixedFunctionBlendOperation.SelectSourceColorIgnoreAlpha, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendArgument.None) => OpaqueTextureTimesCurrent,
            (Direct3D9FixedFunctionBlendOperation.MultiplyColorIgnoreAlpha, Direct3D9FixedFunctionBlendArgument.Current, Direct3D9FixedFunctionBlendArgument.Texture) => OpaqueTextureTimesCurrent,
            (Direct3D9FixedFunctionBlendOperation.MultiplyColorIgnoreAlpha, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendArgument.Current) => OpaqueTextureTimesCurrent,
            (Direct3D9FixedFunctionBlendOperation.MultiplyColorIgnoreAlpha, Direct3D9FixedFunctionBlendArgument.Diffuse, Direct3D9FixedFunctionBlendArgument.Texture) => OpaqueTextureTimesDiffuse,
            (Direct3D9FixedFunctionBlendOperation.MultiplyColorIgnoreAlpha, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendArgument.Diffuse) => OpaqueTextureTimesDiffuse,
            (Direct3D9FixedFunctionBlendOperation.BumpMap, _, Direct3D9FixedFunctionBlendArgument.Texture) => BumpMapTexture,
            (Direct3D9FixedFunctionBlendOperation.BumpMap, Direct3D9FixedFunctionBlendArgument.Texture, _) => BumpMapTexture,
            (Direct3D9FixedFunctionBlendOperation.MultiplyByAlpha, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendArgument.Current) => MaskTextureTimesCurrent,
            (Direct3D9FixedFunctionBlendOperation.MultiplyAlphaOnly, Direct3D9FixedFunctionBlendArgument.Current, Direct3D9FixedFunctionBlendArgument.Texture) => SelectCurrentColorMultiplyTextureAlpha,
            (Direct3D9FixedFunctionBlendOperation.MultiplyAlphaOnly, Direct3D9FixedFunctionBlendArgument.Diffuse, Direct3D9FixedFunctionBlendArgument.Texture) => SelectDiffuseColorMultiplyTextureAlpha,
            (Direct3D9FixedFunctionBlendOperation.MultiplyAlphaOnly, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendArgument.Current) => SelectTextureColorMultiplyCurrentAlpha,
            (Direct3D9FixedFunctionBlendOperation.MultiplyAlphaOnly, Direct3D9FixedFunctionBlendArgument.Texture, Direct3D9FixedFunctionBlendArgument.Diffuse) => SelectTextureColorMultiplyDiffuseAlpha,
            _ => null,
        };

        stageOperation = result.GetValueOrDefault();
        return result.HasValue;
    }
}

internal sealed class Direct3D9FixedFunctionPipelineDeviceStateSender
{
    private readonly IReadOnlyList<Direct3D9FixedFunctionPipelineItem> _items;
    private readonly uint _firstUnusedStage;
    private readonly Func<uint, Direct3D9FixedFunctionTextureStageOperation, int> _setTextureStageOperation;
    private readonly Func<uint, int> _disableTextureStage;
    private readonly Func<nint, int> _sendVertexFormat;
    private readonly Func<int> _setAlphaBlendMode;
    private readonly Func<int> _clearPixelShader;
    private readonly Func<int> _clearVertexShader;

    internal Direct3D9FixedFunctionPipelineDeviceStateSender(
        Direct3D9FinalizedFixedFunctionPipeline pipeline,
        Func<uint, Direct3D9FixedFunctionTextureStageOperation, int> setTextureStageOperation,
        Func<uint, int> disableTextureStage,
        Func<nint, int> sendVertexFormat,
        Func<int> setAlphaBlendMode,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader)
        : this(
            pipeline.Items,
            pipeline.FirstUnusedStage,
            setTextureStageOperation,
            disableTextureStage,
            sendVertexFormat,
            setAlphaBlendMode,
            clearPixelShader,
            clearVertexShader)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
    }

    internal Direct3D9FixedFunctionPipelineDeviceStateSender(
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items,
        uint firstUnusedStage,
        Func<uint, Direct3D9FixedFunctionTextureStageOperation, int> setTextureStageOperation,
        Func<uint, int> disableTextureStage,
        Func<nint, int> sendVertexFormat,
        Func<int> setAlphaBlendMode,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(setTextureStageOperation);
        ArgumentNullException.ThrowIfNull(disableTextureStage);
        ArgumentNullException.ThrowIfNull(sendVertexFormat);
        ArgumentNullException.ThrowIfNull(setAlphaBlendMode);
        ArgumentNullException.ThrowIfNull(clearPixelShader);
        ArgumentNullException.ThrowIfNull(clearVertexShader);

        _items = items;
        _firstUnusedStage = firstUnusedStage;
        _setTextureStageOperation = setTextureStageOperation;
        _disableTextureStage = disableTextureStage;
        _sendVertexFormat = sendVertexFormat;
        _setAlphaBlendMode = setAlphaBlendMode;
        _clearPixelShader = clearPixelShader;
        _clearVertexShader = clearVertexShader;
    }

    internal int SendDeviceStates(nint vertexBuffer)
    {
        ArgumentOutOfRangeException.ThrowIfZero(vertexBuffer);

        foreach (Direct3D9FixedFunctionPipelineItem item in _items)
        {
            if (item.BlendOperation != Direct3D9FixedFunctionBlendOperation.Nop)
            {
                if (!Direct3D9FixedFunctionTextureStageOperations.TryGet(
                    item.BlendOperation,
                    item.Source1,
                    item.Source2,
                    out Direct3D9FixedFunctionTextureStageOperation operation))
                {
                    return Direct3D9Factory.NotImplementedHResult;
                }

                int result = _setTextureStageOperation(item.Stage, operation);
                if (result < 0)
                {
                    return result;
                }
            }

            if (item.ColorSource is not null)
            {
                if (item.ColorSource.SendDeviceStates is null)
                {
                    throw new InvalidOperationException("The color source does not provide fixed-function device-state operations.");
                }

                int result = item.ColorSource.SendDeviceStates(item.Stage, item.Sampler);
                if (result < 0)
                {
                    return result;
                }
            }
        }

        int stateResult = _disableTextureStage(_firstUnusedStage);
        if (stateResult < 0)
        {
            return stateResult;
        }

        stateResult = _sendVertexFormat(vertexBuffer);
        if (stateResult < 0)
        {
            return stateResult;
        }

        stateResult = _setAlphaBlendMode();
        if (stateResult < 0)
        {
            return stateResult;
        }

        stateResult = _clearPixelShader();
        return stateResult < 0 ? stateResult : _clearVertexShader();
    }
}

internal delegate int Direct3D9FlushPipelineVertexBuilder(out nint vertexBuffer);

internal sealed record Direct3D9PrimaryColorSource(
    Func<Direct3D9PipelineOperationSender, int> SendOperations);

internal sealed class Direct3D9PipelineOperationSender
{
    private readonly Func<int> _processEffects;
    private readonly Func<int> _sendGeometryModifiers;
    private readonly Func<int> _sendLighting;
    private readonly Func<int> _processClip;
    private readonly Func<Direct3D9ConstantColorSource, int>? _setConstant;
    private readonly Func<Direct3D9BitmapPipelineColorSource, int>? _setTexture;

    internal Direct3D9PipelineOperationSender(
        Func<int> processEffects,
        Func<int> sendGeometryModifiers,
        Func<int> sendLighting,
        Func<int> processClip,
        Func<Direct3D9ConstantColorSource, int>? setConstant = null,
        Func<Direct3D9BitmapPipelineColorSource, int>? setTexture = null)
    {
        ArgumentNullException.ThrowIfNull(processEffects);
        ArgumentNullException.ThrowIfNull(sendGeometryModifiers);
        ArgumentNullException.ThrowIfNull(sendLighting);
        ArgumentNullException.ThrowIfNull(processClip);

        _processEffects = processEffects;
        _sendGeometryModifiers = sendGeometryModifiers;
        _sendLighting = sendLighting;
        _processClip = processClip;
        _setConstant = setConstant;
        _setTexture = setTexture;
    }

    internal int SetConstant(Direct3D9ConstantColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        return _setConstant?.Invoke(colorSource) ?? Direct3D9Factory.InvalidCallHResult;
    }

    internal int SetTexture(Direct3D9BitmapPipelineColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        if (_setTexture is null)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        Direct3D9BitmapPipelineColorSource ownedColorSource = colorSource.AddRef();
        int result = _setTexture(ownedColorSource);
        if (result < 0)
        {
            ownedColorSource.Dispose();
        }

        return result;
    }

    internal int SendPipelineOperations(Direct3D9PrimaryColorSource primaryColorSource)
    {
        ArgumentNullException.ThrowIfNull(primaryColorSource);
        ArgumentNullException.ThrowIfNull(primaryColorSource.SendOperations);

        int result = primaryColorSource.SendOperations(this);
        if (result < 0)
        {
            return result;
        }

        result = _processEffects();
        if (result < 0)
        {
            return result;
        }

        result = _sendGeometryModifiers();
        if (result < 0)
        {
            return result;
        }

        result = _sendLighting();
        return result < 0 ? result : _processClip();
    }
}

internal delegate int Direct3D9SetupShaderPipeline(
    bool is2D,
    MilCompositingMode compositingMode,
    nint geometryGenerator,
    nint primaryColorSource,
    nint effects,
    nint effectContext);

internal delegate int Direct3D9SetupPathShaderPipeline(
    bool is2D,
    MilCompositingMode compositingMode,
    nint geometryGenerator,
    nint primaryColorSource,
    nint effects,
    Direct3D9PathBrushContext effectContext);

internal delegate int Direct3D9SetupPipelineVertexBuilder(out nint vertexBuilder);

internal delegate int Direct3D9FinalizePipelineVertexMappings(nint vertexBuilder);

internal delegate void Direct3D9ReleasePipelineVertexBuilder(nint vertexBuilder);

internal delegate void Direct3D9SetPipelineOutsideBounds(
    nint vertexBuilder,
    Direct3D9SurfaceRect outsideBounds,
    bool needInside);

internal delegate int Direct3D9GetPipelineShader(out nint shader);

internal delegate void Direct3D9ReleasePipelineShader(nint shader);

internal sealed class Direct3D9ShaderPipelineDeviceStateSender
{
    private readonly bool _is2D;
    private readonly nint _shader;
    private readonly IReadOnlyList<Direct3D9ShaderPipelineItem> _items;
    private readonly Func<nint, int> _sendVertexFormat;
    private readonly Func<int> _setAlphaBlendMode;
    private readonly Func<nint, bool, int> _setShaderState;

    internal Direct3D9ShaderPipelineDeviceStateSender(
        bool is2D,
        nint shader,
        IReadOnlyList<Direct3D9ShaderPipelineItem> items,
        Func<nint, int> sendVertexFormat,
        Func<int> setAlphaBlendMode,
        Func<nint, bool, int> setShaderState)
    {
        ArgumentOutOfRangeException.ThrowIfZero(shader);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(sendVertexFormat);
        ArgumentNullException.ThrowIfNull(setAlphaBlendMode);
        ArgumentNullException.ThrowIfNull(setShaderState);

        _is2D = is2D;
        _shader = shader;
        _items = items;
        _sendVertexFormat = sendVertexFormat;
        _setAlphaBlendMode = setAlphaBlendMode;
        _setShaderState = setShaderState;
    }

    internal int SendDeviceStates(nint vertexBuffer)
    {
        foreach (Direct3D9ShaderPipelineItem item in _items)
        {
            Direct3D9PipelineColorSource? colorSource = item.ColorSource;
            if (colorSource is null)
            {
                continue;
            }

            if (colorSource.SendDeviceStates is null || colorSource.SendShaderData is null)
            {
                throw new InvalidOperationException("The color source does not provide shader device-state operations.");
            }

            int result = colorSource.SendDeviceStates(item.Sampler, item.Sampler);
            if (result < 0)
            {
                return result;
            }

            result = colorSource.SendShaderData(_shader);
            if (result < 0)
            {
                return result;
            }
        }

        if (_is2D)
        {
            ArgumentOutOfRangeException.ThrowIfZero(vertexBuffer);

            int result = _sendVertexFormat(vertexBuffer);
            if (result < 0)
            {
                return result;
            }
        }

        int alphaBlendResult = _setAlphaBlendMode();
        return alphaBlendResult < 0 ? alphaBlendResult : _setShaderState(_shader, _is2D);
    }
}

internal sealed class Direct3D9ShaderPipelineInitializer
{
    private readonly bool _is2D;
    private readonly Direct3D9SetupShaderPipeline _setup;
    private readonly Direct3D9SetupPathShaderPipeline? _setupPath;
    private readonly Direct3D9SetupPipelineVertexBuilder _setupVertexBuilder;
    private readonly IReadOnlyList<Direct3D9ShaderPipelineItem> _items;
    private readonly Direct3D9SetPipelineTextureMapping? _setTextureMapping;
    private readonly Direct3D9SetPipelineConstantMapping? _setConstantMapping;
    private readonly Func<bool> _verticesArePreGenerated;
    private readonly Direct3D9FinalizePipelineVertexMappings _finalizeVertexMappings;
    private readonly Direct3D9ReleasePipelineVertexBuilder _releaseVertexBuilder;
    private readonly Direct3D9SetPipelineOutsideBounds _setOutsideBounds;
    private readonly Direct3D9GetPipelineShader _getShader;
    private readonly Direct3D9ReleasePipelineShader _releaseShader;

    internal Direct3D9ShaderPipelineInitializer(
        bool is2D,
        Direct3D9SetupShaderPipeline setup,
        Direct3D9SetupPipelineVertexBuilder setupVertexBuilder,
        Direct3D9SetPipelineOutsideBounds setOutsideBounds,
        Direct3D9GetPipelineShader getShader,
        Direct3D9ReleasePipelineShader? releaseShader = null,
        IReadOnlyList<Direct3D9ShaderPipelineItem>? items = null,
        Direct3D9SetPipelineTextureMapping? setTextureMapping = null,
        Func<bool>? verticesArePreGenerated = null,
        Direct3D9FinalizePipelineVertexMappings? finalizeVertexMappings = null,
        Direct3D9ReleasePipelineVertexBuilder? releaseVertexBuilder = null,
        Direct3D9SetupPathShaderPipeline? setupPath = null,
        Direct3D9SetPipelineConstantMapping? setConstantMapping = null)
    {
        ArgumentNullException.ThrowIfNull(setup);
        ArgumentNullException.ThrowIfNull(setupVertexBuilder);
        ArgumentNullException.ThrowIfNull(setOutsideBounds);
        ArgumentNullException.ThrowIfNull(getShader);

        _is2D = is2D;
        _setup = setup;
        _setupPath = setupPath;
        _setupVertexBuilder = setupVertexBuilder;
        _items = items ?? [];
        _setTextureMapping = setTextureMapping;
        _setConstantMapping = setConstantMapping;
        _verticesArePreGenerated = verticesArePreGenerated ?? (() => false);
        _finalizeVertexMappings = finalizeVertexMappings ?? (_ => Direct3D9Factory.SuccessHResult);
        _releaseVertexBuilder = releaseVertexBuilder ?? (_ => { });
        _setOutsideBounds = setOutsideBounds;
        _getShader = getShader;
        _releaseShader = releaseShader ?? (_ => { });
    }

    internal nint GeometryGenerator { get; private set; }

    internal nint VertexBuilder { get; private set; }

    internal nint Shader { get; private set; }

    internal int InitializeForRendering(
        MilCompositingMode compositingMode,
        nint geometryGenerator,
        nint primaryColorSource,
        nint effects,
        nint effectContext,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside)
    {
        ArgumentOutOfRangeException.ThrowIfZero(effectContext);

        return InitializeForRenderingCore(
            compositingMode,
            geometryGenerator,
            primaryColorSource,
            outsideBounds,
            needInside,
            () => _setup(
                _is2D,
                compositingMode,
                geometryGenerator,
                primaryColorSource,
                effects,
                effectContext));
    }

    internal int InitializeForRendering(
        MilCompositingMode compositingMode,
        nint geometryGenerator,
        nint primaryColorSource,
        nint effects,
        Direct3D9PathBrushContext effectContext,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside)
    {
        ArgumentNullException.ThrowIfNull(effectContext);

        Direct3D9SetupPathShaderPipeline setupPath = _setupPath
            ?? throw new InvalidOperationException("The shader pipeline does not provide path setup operations.");
        return InitializeForRenderingCore(
            compositingMode,
            geometryGenerator,
            primaryColorSource,
            outsideBounds,
            needInside,
            () => setupPath(
                _is2D,
                compositingMode,
                geometryGenerator,
                primaryColorSource,
                effects,
                effectContext));
    }

    private int InitializeForRenderingCore(
        MilCompositingMode compositingMode,
        nint geometryGenerator,
        nint primaryColorSource,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        Func<int> setup)
    {
        ArgumentOutOfRangeException.ThrowIfZero(geometryGenerator);
        ArgumentOutOfRangeException.ThrowIfZero(primaryColorSource);

        if (Shader != 0)
        {
            throw new InvalidOperationException("The shader pipeline is already initialized.");
        }

        int result = setup();
        if (result < 0)
        {
            return result;
        }

        if (_is2D)
        {
            result = SetupVertexBuilderAndMappings();
            if (result < 0)
            {
                return result;
            }
        }

        if (outsideBounds.HasValue)
        {
            if (VertexBuilder == 0)
            {
                throw new InvalidOperationException("Outside bounds require a vertex builder.");
            }

            _setOutsideBounds(VertexBuilder, outsideBounds.GetValueOrDefault(), needInside);
        }

        GeometryGenerator = geometryGenerator;

        return AcquireShader();
    }

    internal int ReInitialize(
        MilCompositingMode compositingMode,
        nint primaryColorSource,
        nint effects,
        nint effectContext,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        bool hasCachedVertexBuffer)
    {
        ArgumentOutOfRangeException.ThrowIfZero(effectContext);

        if (GeometryGenerator == 0 || Shader == 0)
        {
            throw new InvalidOperationException("The shader pipeline is not initialized.");
        }

        int result = _setup(
            _is2D,
            compositingMode,
            GeometryGenerator,
            primaryColorSource,
            effects,
            effectContext);
        if (result < 0)
        {
            return result;
        }

        if (!hasCachedVertexBuffer && VertexBuilder == 0)
        {
            result = SetupVertexBuilderAndMappings();
            if (result < 0)
            {
                return result;
            }
        }

        if (outsideBounds.HasValue)
        {
            if (VertexBuilder == 0)
            {
                throw new InvalidOperationException("Outside bounds require a vertex builder.");
            }

            _setOutsideBounds(VertexBuilder, outsideBounds.GetValueOrDefault(), needInside);
        }

        nint previousShader = Shader;
        Shader = 0;
        _releaseShader(previousShader);

        return AcquireShader();
    }

    private int AcquireShader()
    {
        int result = _getShader(out nint shader);
        if (result < 0)
        {
            if (shader != 0)
            {
                _releaseShader(shader);
            }

            Shader = 0;
            return result;
        }

        Shader = shader;
        return result;
    }

    private int SetupVertexBuilderAndMappings()
    {
        int result = _setupVertexBuilder(out nint vertexBuilder);
        VertexBuilder = vertexBuilder;
        if (result < 0)
        {
            ReleaseFailedVertexBuilder();
            return result;
        }

        if (VertexBuilder == 0)
        {
            throw new InvalidOperationException("Vertex builder setup succeeded without returning a builder.");
        }

        nint mappingVertexBuilder = _verticesArePreGenerated() ? 0 : VertexBuilder;

        foreach (Direct3D9ShaderPipelineItem item in _items)
        {
            Direct3D9PipelineColorSource? colorSource = item.ColorSource;
            if (colorSource is null || item.TextureCoordinates == Direct3D9VertexFormatAttribute.None)
            {
                continue;
            }

            if (item.TextureCoordinates is Direct3D9VertexFormatAttribute.Diffuse or Direct3D9VertexFormatAttribute.Specular)
            {
                if (colorSource.SendConstantVertexMapping is null)
                {
                    throw new InvalidOperationException("The color source does not provide constant vertex mapping.");
                }

                result = colorSource.SendConstantVertexMapping(mappingVertexBuilder, item.TextureCoordinates, _setConstantMapping);
            }
            else
            {
                if (colorSource.SendVertexMapping is null)
                {
                    throw new InvalidOperationException("The color source does not provide texture vertex mapping.");
                }

                result = colorSource.SendVertexMapping(mappingVertexBuilder, item.TextureCoordinates, _setTextureMapping);
            }

            if (result < 0)
            {
                ReleaseFailedVertexBuilder();
                return result;
            }
        }

        result = _finalizeVertexMappings(VertexBuilder);
        if (result < 0)
        {
            ReleaseFailedVertexBuilder();
        }

        return result;
    }

    private void ReleaseFailedVertexBuilder()
    {
        if (VertexBuilder == 0)
        {
            return;
        }

        nint vertexBuilder = VertexBuilder;
        VertexBuilder = 0;
        _releaseVertexBuilder(vertexBuilder);
    }
}

internal sealed class Direct3D9ShaderPipelineItemBuilder : IDisposable
{
    private readonly List<Direct3D9ShaderPipelineItem> _items = [];
    private readonly bool _is2D;
    private readonly Direct3D9VertexFormatAttribute _incomingVertexFormat;
    private readonly Direct3D9ShaderAlphaMultiplyOperation _alphaMultiplyOperation;
    private readonly Func<Direct3D9ShaderTextureFunction, uint?> _getTextureTransformHandle;
    private readonly Func<Direct3D9ShaderConstantFunction, uint?> _getConstantColorHandle;
    private bool _ownershipTransferred;
    private uint _nextSampler;

    internal Direct3D9ShaderPipelineItemBuilder(
        Direct3D9VertexFormatAttribute incomingVertexFormat = Direct3D9VertexFormatAttribute.None,
        Func<Direct3D9ShaderTextureFunction, uint?>? getTextureTransformHandle = null,
        bool is2D = true,
        Direct3D9ShaderAlphaMultiplyOperation alphaMultiplyOperation = Direct3D9ShaderAlphaMultiplyOperation.Multiply,
        Func<Direct3D9ShaderConstantFunction, uint?>? getConstantColorHandle = null)
    {
        _is2D = is2D;
        _incomingVertexFormat = incomingVertexFormat;
        _alphaMultiplyOperation = alphaMultiplyOperation;
        _getTextureTransformHandle = getTextureTransformHandle ?? (_ => null);
        _getConstantColorHandle = getConstantColorHandle ?? (_ => null);
    }

    internal int SetConstant(Direct3D9ConstantColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        if (_items.Count != 0)
        {
            throw new InvalidOperationException("The primary color source has already been set.");
        }

        bool usesDiffuse = _is2D && (_incomingVertexFormat & Direct3D9VertexFormatAttribute.Diffuse) == 0;
        Direct3D9ShaderConstantFunction constantFunction = usesDiffuse
            ? _alphaMultiplyOperation == Direct3D9ShaderAlphaMultiplyOperation.Multiply
                ? Direct3D9ShaderConstantFunction.MultiplyByInputDiffuse
                : Direct3D9ShaderConstantFunction.MultiplyByInputDiffuseNonPremultipliedInput
            : Direct3D9ShaderConstantFunction.MultiplyConstant;
        uint? colorHandle = usesDiffuse ? null : _getConstantColorHandle(constantFunction);

        try
        {
            _items.Add(new Direct3D9ShaderPipelineItem(
                uint.MaxValue,
                colorSource.PipelineColorSource,
                usesDiffuse ? Direct3D9VertexFormatAttribute.Diffuse : Direct3D9VertexFormatAttribute.None,
                constantFunction: constantFunction,
                lifetimeOwner: colorSource));
            if (colorHandle is uint handle)
            {
                colorSource.SetColorShaderHandle(handle);
            }
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal int MultiplyConstantAlpha(Direct3D9ConstantAlphaScalableColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("A primary color source operation is required.");
        }

        float alpha = colorSource.GetColor().Alpha;
        if (alpha == 1)
        {
            colorSource.Dispose();
            return Direct3D9Factory.SuccessHResult;
        }

        for (int index = _items.Count - 1; index >= 0; index--)
        {
            Action<float>? alphaScale = _items[index].ColorSource?.AlphaScale;
            if (alphaScale is not null)
            {
                alphaScale(alpha);
                colorSource.Dispose();
                return Direct3D9Factory.SuccessHResult;
            }
        }

        bool usesDiffuse = _is2D && (_incomingVertexFormat & Direct3D9VertexFormatAttribute.Diffuse) == 0;
        Direct3D9ShaderConstantFunction constantFunction = usesDiffuse
            ? _alphaMultiplyOperation == Direct3D9ShaderAlphaMultiplyOperation.Multiply
                ? Direct3D9ShaderConstantFunction.MultiplyByInputDiffuse
                : Direct3D9ShaderConstantFunction.MultiplyByInputDiffuseNonPremultipliedInput
            : _alphaMultiplyOperation == Direct3D9ShaderAlphaMultiplyOperation.Multiply
                ? Direct3D9ShaderConstantFunction.MultiplyAlpha
                : Direct3D9ShaderConstantFunction.MultiplyAlphaNonPremultiplied;
        uint? alphaHandle = usesDiffuse ? null : _getConstantColorHandle(constantFunction);

        try
        {
            _items.Add(new Direct3D9ShaderPipelineItem(
                uint.MaxValue,
                colorSource.PipelineColorSource,
                usesDiffuse ? Direct3D9VertexFormatAttribute.Diffuse : Direct3D9VertexFormatAttribute.None,
                constantFunction: constantFunction,
                lifetimeOwner: colorSource));
            if (alphaHandle is uint handle)
            {
                colorSource.SetShaderAlphaHandle(handle);
            }
        }
        catch (OutOfMemoryException)
        {
            colorSource.Dispose();
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal int SetTexture(Direct3D9BitmapPipelineColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        if (_items.Count != 0)
        {
            throw new InvalidOperationException("The primary color source has already been set.");
        }

        bool usesVertexUv = (_incomingVertexFormat & Direct3D9VertexFormatAttribute.Uv1) != 0;
        Direct3D9ShaderTextureFunction textureFunction = usesVertexUv
            ? Direct3D9ShaderTextureFunction.MultiplyTextureTransformFromVertexUv
            : Direct3D9ShaderTextureFunction.MultiplyTextureNoTransformFromTextureCoordinate;
        Direct3D9VertexFormatAttribute textureCoordinates = usesVertexUv
            ? Direct3D9VertexFormatAttribute.Uv1
            : GetTextureCoordinate(_nextSampler);
        uint? textureTransformHandle = usesVertexUv ? _getTextureTransformHandle(textureFunction) : null;

        try
        {
            _items.Add(new Direct3D9ShaderPipelineItem(
                _nextSampler++,
                colorSource,
                textureCoordinates,
                textureTransformHandle,
                textureFunction: textureFunction,
                takeOwnership: true));
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal IReadOnlyList<Direct3D9ShaderPipelineItem> DetachItems()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("A primary color source operation is required.");
        }

        _ownershipTransferred = true;
        return _items;
    }

    public void Dispose()
    {
        if (!_ownershipTransferred)
        {
            ReleaseOwnedColorSources(_items);
        }
    }

    internal static void ReleaseOwnedColorSources(IReadOnlyList<Direct3D9ShaderPipelineItem> items)
    {
        foreach (Direct3D9ShaderPipelineItem item in items)
        {
            (item.LifetimeOwner ?? item.Ownership)?.Dispose();
        }
    }

    private static Direct3D9VertexFormatAttribute GetTextureCoordinate(uint sampler) => sampler switch
    {
        0 => Direct3D9VertexFormatAttribute.Uv1,
        1 => Direct3D9VertexFormatAttribute.Uv2,
        2 => Direct3D9VertexFormatAttribute.Uv3,
        3 => Direct3D9VertexFormatAttribute.Uv4,
        _ => Direct3D9VertexFormatAttribute.None,
    };
}

internal sealed class Direct3D9FixedFunctionPipelineItemBuilder : IDisposable
{
    private readonly List<Direct3D9FixedFunctionPipelineItem> _items = [];
    private readonly Direct3D9VertexFormatAttribute _incomingVertexFormat;
    private Direct3D9VertexFormatAttribute _availableVertexFormat;
    private Direct3D9VertexFormatAttribute _generatedVertexFormat;
    private bool _ownershipTransferred;
    private uint _nextStage;
    private uint _nextSampler;

    internal Direct3D9FixedFunctionPipelineItemBuilder(
        Direct3D9VertexFormatAttribute incomingVertexFormat = Direct3D9VertexFormatAttribute.None)
    {
        _incomingVertexFormat = incomingVertexFormat;
        _availableVertexFormat = (
            Direct3D9VertexFormatAttribute.Diffuse |
            Direct3D9VertexFormatAttribute.Specular |
            Direct3D9VertexFormatAttribute.Uv4) & ~incomingVertexFormat;
    }

    internal Direct3D9VertexFormatAttribute GeneratedVertexFormat => _generatedVertexFormat;

    internal int SetConstant(Direct3D9ConstantColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        if (_items.Count != 0)
        {
            throw new InvalidOperationException("The primary color source has already been set.");
        }

        Direct3D9VertexFormatAttribute sourceLocation;
        Direct3D9FixedFunctionBlendArgument sourceArgument;
        uint sampler;
        if (TryGenerateVertexAttribute(Direct3D9VertexFormatAttribute.Diffuse))
        {
            sourceLocation = Direct3D9VertexFormatAttribute.Diffuse;
            sourceArgument = Direct3D9FixedFunctionBlendArgument.Diffuse;
            sampler = uint.MaxValue;
        }
        else if (IsAvailableForReference(Direct3D9VertexFormatAttribute.Uv1))
        {
            sourceLocation = Direct3D9VertexFormatAttribute.Uv1;
            sourceArgument = Direct3D9FixedFunctionBlendArgument.Texture;
            sampler = _nextSampler;
        }
        else
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        try
        {
            _items.Add(new Direct3D9FixedFunctionPipelineItem(
                sourceLocation,
                colorSource.PipelineColorSource,
                _nextStage,
                sampler,
                Direct3D9FixedFunctionBlendOperation.SelectSource,
                sourceArgument,
                lifetimeOwner: colorSource));
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        _nextStage++;
        if (sampler != uint.MaxValue)
        {
            _nextSampler++;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal int SetTexture(Direct3D9BitmapPipelineColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        return SetTextureCore(colorSource.PipelineColorSource, colorSource);
    }

    internal int SetTexture(Direct3D9PipelineColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        return SetTextureCore(colorSource, null);
    }

    internal int MultiplyAlphaMask(Direct3D9BitmapPipelineColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        return MultiplyAlphaMaskCore(colorSource.PipelineColorSource, colorSource);
    }

    internal int MultiplyAlphaMask(Direct3D9PipelineColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        return MultiplyAlphaMaskCore(colorSource, null);
    }

    internal int MultiplyConstantAlpha(float alpha)
    {
        return MultiplyConstantAlpha(
            alpha,
            static value => new Direct3D9ConstantAlphaScalableColorSource(value).PipelineColorSource);
    }

    internal int MultiplyConstantAlpha(
        float alpha,
        Func<float, Direct3D9PipelineColorSource?> createScalableColorSource)
    {
        ArgumentNullException.ThrowIfNull(createScalableColorSource);
        EnsurePrimaryOperation();

        if (!float.IsFinite(alpha) || alpha < 0 || alpha > 1)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        if (alpha == 1)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        for (int index = _items.Count - 1; index >= 0; index--)
        {
            Direct3D9PipelineColorSource? colorSource = _items[index].ColorSource;
            if (colorSource?.AlphaScale is not null)
            {
                colorSource.AlphaScale(alpha);
                return Direct3D9Factory.SuccessHResult;
            }
        }

        Direct3D9VertexFormatAttribute sourceLocation;
        Direct3D9FixedFunctionBlendArgument sourceArgument;
        uint sampler;
        if (IsAvailableForGeneration(Direct3D9VertexFormatAttribute.Diffuse))
        {
            sourceLocation = Direct3D9VertexFormatAttribute.Diffuse;
            sourceArgument = Direct3D9FixedFunctionBlendArgument.Diffuse;
            sampler = uint.MaxValue;
        }
        else if (IsAvailableForReference(Direct3D9VertexFormatAttribute.Uv1))
        {
            sourceLocation = Direct3D9VertexFormatAttribute.Uv1;
            sourceArgument = Direct3D9FixedFunctionBlendArgument.Texture;
            sampler = _nextSampler;
        }
        else if (IsAvailableForGeneration(Direct3D9VertexFormatAttribute.Specular))
        {
            sourceLocation = Direct3D9VertexFormatAttribute.Specular;
            sourceArgument = Direct3D9FixedFunctionBlendArgument.Specular;
            sampler = uint.MaxValue;
        }
        else
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        Direct3D9PipelineColorSource? scalableColorSource;
        try
        {
            scalableColorSource = createScalableColorSource(alpha);
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        if (scalableColorSource is null)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        try
        {
            _items.Add(new Direct3D9FixedFunctionPipelineItem(
                sourceLocation,
                scalableColorSource,
                _nextStage,
                sampler,
                blendOperation: Direct3D9FixedFunctionBlendOperation.Multiply,
                source1: sourceArgument,
                source2: Direct3D9FixedFunctionBlendArgument.Current));
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        if (IsAvailableForGeneration(sourceLocation))
        {
            GenerateVertexAttribute(sourceLocation);
        }

        _nextStage++;
        if (sampler != uint.MaxValue)
        {
            _nextSampler++;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    internal int ProcessAlphaScaleEffect(
        uint parameterSize,
        uint resourceCount,
        float alpha)
    {
        if (parameterSize != sizeof(float) || resourceCount != 0)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        return MultiplyConstantAlpha(alpha);
    }

    internal int ProcessAlphaScaleEffect(
        uint parameterSize,
        uint resourceCount,
        float alpha,
        Func<float, Direct3D9PipelineColorSource?> createScalableColorSource)
    {
        if (parameterSize != sizeof(float) || resourceCount != 0)
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        return MultiplyConstantAlpha(alpha, createScalableColorSource);
    }

    internal int AddLighting(Direct3D9LightingColorSource colorSource)
    {
        ArgumentNullException.ThrowIfNull(colorSource);
        EnsurePrimaryOperation();

        const Direct3D9VertexFormatAttribute sourceLocation = Direct3D9VertexFormatAttribute.Diffuse;
        if (!IsAvailableForGeneration(sourceLocation) && !IsAvailableForReference(sourceLocation))
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        try
        {
            _items.Add(new Direct3D9FixedFunctionPipelineItem(
                sourceLocation,
                colorSource.PipelineColorSource,
                _nextStage,
                blendOperation: Direct3D9FixedFunctionBlendOperation.Multiply,
                source1: Direct3D9FixedFunctionBlendArgument.Diffuse,
                source2: Direct3D9FixedFunctionBlendArgument.Current,
                lifetimeOwner: colorSource));
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        if (IsAvailableForGeneration(sourceLocation))
        {
            GenerateVertexAttribute(sourceLocation);
        }

        _nextStage++;
        return Direct3D9Factory.SuccessHResult;
    }

    internal IReadOnlyList<Direct3D9FixedFunctionPipelineItem> DetachItems()
    {
        EnsurePrimaryOperation();
        _ownershipTransferred = true;
        return _items;
    }

    public void Dispose()
    {
        if (!_ownershipTransferred)
        {
            ReleaseOwnedColorSources(_items);
        }
    }

    internal static void ReleaseOwnedColorSources(IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items)
    {
        foreach (Direct3D9FixedFunctionPipelineItem item in items)
        {
            (item.LifetimeOwner ?? item.Ownership)?.Dispose();
        }
    }

    private int SetTextureCore(
        Direct3D9PipelineColorSource colorSource,
        Direct3D9BitmapPipelineColorSource? ownership)
    {
        if (_items.Count != 0)
        {
            throw new InvalidOperationException("The primary color source has already been set.");
        }

        if (!TryReserveVertexAttribute(Direct3D9VertexFormatAttribute.Uv1))
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        _items.Add(ownership is null
            ? new Direct3D9FixedFunctionPipelineItem(
                Direct3D9VertexFormatAttribute.Uv1,
                colorSource,
                _nextStage++,
                _nextSampler++,
                Direct3D9FixedFunctionBlendOperation.SelectSource,
                Direct3D9FixedFunctionBlendArgument.Texture)
            : new Direct3D9FixedFunctionPipelineItem(
                Direct3D9VertexFormatAttribute.Uv1,
                ownership,
                _nextStage++,
                _nextSampler++,
                Direct3D9FixedFunctionBlendOperation.SelectSource,
                Direct3D9FixedFunctionBlendArgument.Texture));
        return Direct3D9Factory.SuccessHResult;
    }

    private int MultiplyAlphaMaskCore(
        Direct3D9PipelineColorSource colorSource,
        Direct3D9BitmapPipelineColorSource? ownership)
    {
        EnsurePrimaryOperation();
        Direct3D9VertexFormatAttribute sourceLocation = VerticesArePreGenerated
            ? Direct3D9VertexFormatAttribute.Uv1
            : Direct3D9VertexFormatAttribute.Uv2;
        if (!TryReserveVertexAttribute(sourceLocation))
        {
            return Direct3D9Factory.UnsupportedOperationHResult;
        }

        _items.Add(ownership is null
            ? new Direct3D9FixedFunctionPipelineItem(
                sourceLocation,
                colorSource,
                _nextStage++,
                _nextSampler++,
                Direct3D9FixedFunctionBlendOperation.MultiplyByAlpha,
                Direct3D9FixedFunctionBlendArgument.Texture,
                Direct3D9FixedFunctionBlendArgument.Current)
            : new Direct3D9FixedFunctionPipelineItem(
                sourceLocation,
                ownership,
                _nextStage++,
                _nextSampler++,
                Direct3D9FixedFunctionBlendOperation.MultiplyByAlpha,
                Direct3D9FixedFunctionBlendArgument.Texture,
                Direct3D9FixedFunctionBlendArgument.Current));
        return Direct3D9Factory.SuccessHResult;
    }

    private bool VerticesArePreGenerated => IsAvailableForReference(Direct3D9VertexFormatAttribute.Uv1);

    private bool TryReserveVertexAttribute(Direct3D9VertexFormatAttribute attribute) =>
        TryGenerateVertexAttribute(attribute) || IsAvailableForReference(attribute);

    private bool TryGenerateVertexAttribute(Direct3D9VertexFormatAttribute attribute)
    {
        if (!IsAvailableForGeneration(attribute))
        {
            return false;
        }

        GenerateVertexAttribute(attribute);
        return true;
    }

    private bool IsAvailableForGeneration(Direct3D9VertexFormatAttribute attribute) =>
        (_availableVertexFormat & attribute) != 0;

    private void GenerateVertexAttribute(Direct3D9VertexFormatAttribute attribute)
    {
        _generatedVertexFormat |= attribute;
        _availableVertexFormat &= ~attribute;
    }

    private bool IsAvailableForReference(Direct3D9VertexFormatAttribute attribute) =>
        (_incomingVertexFormat & attribute) != 0;

    private void EnsurePrimaryOperation()
    {
        if (_items.Count == 0)
        {
            throw new InvalidOperationException("A primary color source operation is required.");
        }
    }
}

internal sealed class Direct3D9FixedFunctionPassInputs
{
    private readonly Func<Direct3D9FixedFunctionPipelineItemBuilder, int> _sendPrimaryOperations;
    private readonly Func<nint, Direct3D9FixedFunctionPipelineItemBuilder, int>? _processEffects;
    private readonly Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? _sendGeometryModifiers;
    private readonly Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? _sendLighting;
    private readonly Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? _processClip;
    private readonly nint _effectContext;
    private readonly Direct3D9VertexFormatAttribute _incomingVertexFormat;

    internal Direct3D9FixedFunctionPassInputs(
        Func<Direct3D9FixedFunctionPipelineItemBuilder, int> sendPrimaryOperations,
        Func<nint, Direct3D9FixedFunctionPipelineItemBuilder, int>? processEffects = null,
        nint effectContext = 0,
        Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? sendGeometryModifiers = null,
        Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? sendLighting = null,
        Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? processClip = null,
        Direct3D9VertexFormatAttribute incomingVertexFormat = Direct3D9VertexFormatAttribute.None)
    {
        ArgumentNullException.ThrowIfNull(sendPrimaryOperations);
        if (processEffects is not null)
        {
            ArgumentOutOfRangeException.ThrowIfZero(effectContext);
        }

        _sendPrimaryOperations = sendPrimaryOperations;
        _processEffects = processEffects;
        _effectContext = effectContext;
        _sendGeometryModifiers = sendGeometryModifiers;
        _sendLighting = sendLighting;
        _processClip = processClip;
        _incomingVertexFormat = incomingVertexFormat;
    }

    internal int CreateItems(out IReadOnlyList<Direct3D9FixedFunctionPipelineItem>? items) =>
        CreateItems(sendLighting: null, out items);

    internal int CreateItems(
        Func<Direct3D9FixedFunctionPipelineItemBuilder, int>? sendLighting,
        out IReadOnlyList<Direct3D9FixedFunctionPipelineItem>? items)
    {
        items = null;
        using Direct3D9FixedFunctionPipelineItemBuilder builder = new(_incomingVertexFormat);

        int result = _sendPrimaryOperations(builder);
        if (result >= 0 && _processEffects is not null)
        {
            result = _processEffects(_effectContext, builder);
        }

        if (result >= 0 && _sendGeometryModifiers is not null)
        {
            result = _sendGeometryModifiers(builder);
        }

        if (result >= 0 && _sendLighting is not null)
        {
            result = _sendLighting(builder);
        }

        if (result >= 0 && sendLighting is not null)
        {
            result = sendLighting(builder);
        }

        if (result >= 0 && _processClip is not null)
        {
            result = _processClip(builder);
        }

        if (result >= 0)
        {
            items = builder.DetachItems();
        }

        return result;
    }
}

internal sealed class Direct3D9FixedFunctionPipelineInitializer
{ 
    private readonly IReadOnlyList<Direct3D9FixedFunctionPipelineItem> _items;
    private readonly Direct3D9SetupPipelineVertexBuilder _setupVertexBuilder;
    private readonly Direct3D9SetPipelineTextureMapping? _setTextureMapping;
    private readonly Direct3D9SetPipelineConstantMapping? _setConstantMapping;
    private readonly Func<bool> _verticesArePreGenerated;
    private readonly Direct3D9FinalizePipelineVertexMappings _finalizeVertexMappings;
    private readonly Direct3D9ReleasePipelineVertexBuilder _releaseVertexBuilder;
    private readonly Direct3D9SetPipelineOutsideBounds _setOutsideBounds;
    private readonly Direct3D9FixedFunctionPipelineDeviceStateSender _deviceStateSender;

    internal Direct3D9FixedFunctionPipelineInitializer(
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items,
        Direct3D9SetupPipelineVertexBuilder setupVertexBuilder,
        Direct3D9FinalizePipelineVertexMappings finalizeVertexMappings,
        Direct3D9ReleasePipelineVertexBuilder releaseVertexBuilder,
        Direct3D9SetPipelineOutsideBounds setOutsideBounds,
        Func<uint, Direct3D9FixedFunctionTextureStageOperation, int> setTextureStageOperation,
        Func<uint, int> disableTextureStage,
        Func<nint, int> sendVertexFormat,
        Func<int> setAlphaBlendMode,
        Func<int> clearPixelShader,
        Func<int> clearVertexShader,
        Direct3D9SetPipelineTextureMapping? setTextureMapping = null,
        Func<bool>? verticesArePreGenerated = null,
        Direct3D9SetPipelineConstantMapping? setConstantMapping = null)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(setupVertexBuilder);
        ArgumentNullException.ThrowIfNull(finalizeVertexMappings);
        ArgumentNullException.ThrowIfNull(releaseVertexBuilder);
        ArgumentNullException.ThrowIfNull(setOutsideBounds);

        Direct3D9FinalizedFixedFunctionPipeline finalizedPipeline =
            Direct3D9FixedFunctionPipelineFinalizer.FinalizeBlendOperations(items);
        _items = finalizedPipeline.Items;
        _deviceStateSender = new Direct3D9FixedFunctionPipelineDeviceStateSender(
            finalizedPipeline,
            setTextureStageOperation,
            disableTextureStage,
            sendVertexFormat,
            setAlphaBlendMode,
            clearPixelShader,
            clearVertexShader);
        _setupVertexBuilder = setupVertexBuilder;
        _setTextureMapping = setTextureMapping;
        _setConstantMapping = setConstantMapping;
        _verticesArePreGenerated = verticesArePreGenerated ?? (() => false);
        _finalizeVertexMappings = finalizeVertexMappings;
        _releaseVertexBuilder = releaseVertexBuilder;
        _setOutsideBounds = setOutsideBounds;
    }

    internal int InitializeForRendering(
        ref Direct3D9GeometryRenderer<uint> renderer,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        Func<nint, int> drawPrimitive,
        Func<int> beginBuilding,
        Func<bool> hasOutsideBounds,
        Direct3D9FlushPipelineVertexBuilder flushTryGetVertexBuffer,
        Action releaseColorSources,
        out Direct3D9Pipeline? pipeline)
    {
        return InitializeForRendering(
            checked((nint) renderer.FlexibleVertexFormat),
            outsideBounds,
            needInside,
            drawPrimitive,
            beginBuilding,
            () => Direct3D9GeometryRenderer<uint>.SendGeometry(0),
            hasOutsideBounds,
            flushTryGetVertexBuffer,
            releaseColorSources,
            out pipeline);
    }

    internal int InitializeForRendering(
        nint geometryGenerator,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        Func<nint, int> drawPrimitive,
        Func<int> beginBuilding,
        Func<int> sendGeometry,
        Func<bool> hasOutsideBounds,
        Direct3D9FlushPipelineVertexBuilder flushTryGetVertexBuffer,
        Action releaseColorSources,
        out Direct3D9Pipeline? pipeline)
    { 
        ArgumentOutOfRangeException.ThrowIfZero(geometryGenerator);
        ArgumentNullException.ThrowIfNull(drawPrimitive);
        ArgumentNullException.ThrowIfNull(beginBuilding);
        ArgumentNullException.ThrowIfNull(sendGeometry);
        ArgumentNullException.ThrowIfNull(hasOutsideBounds);
        ArgumentNullException.ThrowIfNull(flushTryGetVertexBuffer);
        ArgumentNullException.ThrowIfNull(releaseColorSources);

        pipeline = null;
        int result = _setupVertexBuilder(out nint vertexBuilder);
        if (result < 0)
        {
            ReleaseVertexBuilder(vertexBuilder);
            ReleaseOwnedColorSources();
            return result;
        }

        if (vertexBuilder == 0)
        {
            ReleaseOwnedColorSources();
            throw new InvalidOperationException("Vertex builder setup succeeded without returning a builder.");
        }

        nint mappingVertexBuilder = _verticesArePreGenerated() ? 0 : vertexBuilder;
        foreach (Direct3D9FixedFunctionPipelineItem item in _items)
        {
            Direct3D9PipelineColorSource? colorSource = item.ColorSource;
            if (colorSource is null)
            {
                continue;
            }

            if (item.SourceLocation is Direct3D9VertexFormatAttribute.Diffuse or Direct3D9VertexFormatAttribute.Specular
                && colorSource.SendConstantVertexMapping is not null)
            {
                result = colorSource.SendConstantVertexMapping(mappingVertexBuilder, item.SourceLocation, _setConstantMapping);
            }
            else if (colorSource.SendVertexMapping is not null)
            {
                result = colorSource.SendVertexMapping(mappingVertexBuilder, item.SourceLocation, _setTextureMapping);
            }
            else
            {
                _releaseVertexBuilder(vertexBuilder);
                ReleaseOwnedColorSources();
                throw new InvalidOperationException("The color source does not provide vertex mapping.");
            }
            if (result < 0)
            {
                _releaseVertexBuilder(vertexBuilder);
                ReleaseOwnedColorSources();
                return result;
            }
        }

        result = _finalizeVertexMappings(vertexBuilder);
        if (result < 0)
        {
            _releaseVertexBuilder(vertexBuilder);
            ReleaseOwnedColorSources();
            return result;
        }

        if (outsideBounds.HasValue)
        {
            _setOutsideBounds(vertexBuilder, outsideBounds.GetValueOrDefault(), needInside);
        }

        IReadOnlyList<Direct3D9PipelineColorSource?> colorSources = _items
            .Select(static item => item.ColorSource)
            .ToArray();
        IReadOnlyList<IDisposable?> ownedColorSources = _items
            .Select(static item => item.LifetimeOwner ?? item.Ownership)
            .ToArray();

        try
        {
            pipeline = new Direct3D9Pipeline(
                geometryGenerator,
                colorSources,
                _deviceStateSender.SendDeviceStates,
                drawPrimitive,
                beginBuilding,
                sendGeometry,
                hasOutsideBounds,
                flushTryGetVertexBuffer,
                releaseColorSources,
                () => _releaseVertexBuilder(vertexBuilder),
                resetColorSources: false,
                ownedColorSources);
        }
        catch
        {
            _releaseVertexBuilder(vertexBuilder);
            ReleaseOwnedColorSources();
            throw;
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private void ReleaseOwnedColorSources()
    {
        foreach (Direct3D9FixedFunctionPipelineItem item in _items)
        {
            (item.LifetimeOwner ?? item.Ownership)?.Dispose();
        }
    }

    private void ReleaseVertexBuilder(nint vertexBuilder)
    {
        if (vertexBuilder != 0)
        {
            _releaseVertexBuilder(vertexBuilder);
        }
    }
}

internal sealed class Direct3D9Pipeline
{
    private readonly IReadOnlyList<Direct3D9PipelineColorSource?> _colorSources;
    private readonly IReadOnlyList<IDisposable?> _ownedColorSources;
    private readonly Func<nint, int> _sendDeviceStates;
    private readonly Func<nint, int> _drawPrimitive;
    private readonly Func<int> _beginBuilding;
    private readonly Func<int> _sendGeometry;
    private readonly Func<bool> _hasOutsideBounds;
    private readonly Direct3D9FlushPipelineVertexBuilder _flushTryGetVertexBuffer;
    private readonly Action _releaseColorSources;
    private readonly Action _releaseVertexBuilder;
    private nint _geometryGenerator;
    private nint _vertexBuffer;
    private bool _hasVertexBuilder;
    private bool _hasColorSources = true;

    internal Direct3D9Pipeline(
        nint geometryGenerator,
        IReadOnlyList<Direct3D9PipelineColorSource?> colorSources,
        Func<nint, int> sendDeviceStates,
        Func<nint, int> drawPrimitive,
        Func<int> beginBuilding,
        Func<int> sendGeometry,
        Func<bool> hasOutsideBounds,
        Direct3D9FlushPipelineVertexBuilder flushTryGetVertexBuffer,
        Action releaseColorSources,
        Action releaseVertexBuilder,
        bool resetColorSources = true,
        IReadOnlyList<IDisposable?>? ownedColorSources = null)
    {
        ArgumentOutOfRangeException.ThrowIfZero(geometryGenerator);
        ArgumentNullException.ThrowIfNull(colorSources);
        ArgumentNullException.ThrowIfNull(sendDeviceStates);
        ArgumentNullException.ThrowIfNull(drawPrimitive);
        ArgumentNullException.ThrowIfNull(beginBuilding);
        ArgumentNullException.ThrowIfNull(sendGeometry);
        ArgumentNullException.ThrowIfNull(hasOutsideBounds);
        ArgumentNullException.ThrowIfNull(flushTryGetVertexBuffer);
        ArgumentNullException.ThrowIfNull(releaseColorSources);
        ArgumentNullException.ThrowIfNull(releaseVertexBuilder);

        _geometryGenerator = geometryGenerator;
        _colorSources = colorSources;
        _ownedColorSources = ownedColorSources ?? [];
        _sendDeviceStates = sendDeviceStates;
        _drawPrimitive = drawPrimitive;
        _beginBuilding = beginBuilding;
        _sendGeometry = sendGeometry;
        _hasOutsideBounds = hasOutsideBounds;
        _flushTryGetVertexBuffer = flushTryGetVertexBuffer;
        _releaseColorSources = releaseColorSources;
        _releaseVertexBuilder = releaseVertexBuilder;

        if (resetColorSources)
        {
            foreach (Direct3D9PipelineColorSource? colorSource in colorSources)
            {
                colorSource?.ResetForPipelineReuse?.Invoke();
            }
        }

        _hasVertexBuilder = true;
    }

    internal nint GeometryGenerator => _geometryGenerator;

    internal int Execute()
    {
        if (_geometryGenerator == 0 || (!_hasVertexBuilder && _vertexBuffer == 0))
        {
            throw new InvalidOperationException("The pipeline must be initialized before execution.");
        }

        if (_vertexBuffer != 0)
        {
            int result = _sendDeviceStates(_vertexBuffer);
            return result < 0 ? result : _drawPrimitive(_vertexBuffer);
        }

        int buildResult = _beginBuilding();
        if (buildResult < 0)
        {
            return buildResult;
        }

        buildResult = _sendGeometry();
        if (buildResult < 0)
        {
            return buildResult;
        }

        if (buildResult == Direct3D9Factory.EmptyFillHResult && !_hasOutsideBounds())
        {
            return Direct3D9Factory.SuccessHResult;
        }

        buildResult = _flushTryGetVertexBuffer(out _vertexBuffer);
        if (buildResult < 0)
        {
            _vertexBuffer = 0;
            return buildResult;
        }

        ReleaseVertexBuilder();
        return Direct3D9Factory.SuccessHResult;
    }

    internal int RealizeColorSourcesAndSendState(nint vertexBuffer)
    {
        int result = RealizeColorSources();
        return result < 0 ? result : _sendDeviceStates(vertexBuffer);
    }

    internal void ReleaseExpensiveResources()
    {
        if (_hasColorSources)
        {
            _hasColorSources = false;
            foreach (IDisposable? ownedColorSource in _ownedColorSources)
            {
                ownedColorSource?.Dispose();
            }

            _releaseColorSources();
        }

        ReleaseVertexBuilder();
        _geometryGenerator = 0;
        _vertexBuffer = 0;
    }

    private int RealizeColorSources()
    {
        foreach (Direct3D9PipelineColorSource? colorSource in _colorSources)
        {
            if (colorSource is null)
            {
                continue;
            }

            int result = colorSource.SourceType switch
            {
                Direct3D9ColorSourceType.PrecomputedComponent => Direct3D9Factory.SuccessHResult,
                Direct3D9ColorSourceType.Constant => Direct3D9Factory.SuccessHResult,
                Direct3D9ColorSourceType.Texture => colorSource.Realize(),
                Direct3D9ColorSourceType.Texture | Direct3D9ColorSourceType.Constant => colorSource.Realize(),
                Direct3D9ColorSourceType.Programmatic => Direct3D9Factory.SuccessHResult,
                _ => Direct3D9Factory.InternalErrorHResult,
            };

            if (result < 0)
            {
                return result;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private void ReleaseVertexBuilder()
    {
        if (!_hasVertexBuilder)
        {
            return;
        }

        _hasVertexBuilder = false;
        _releaseVertexBuilder();
    }
}
