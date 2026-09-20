using System.Numerics;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal enum Direct3D9MeshShaderType
{
    Diffuse,
    Specular,
    Emissive
}

internal delegate int Direct3D9GetMeshShaderSurfaceSource(
    out Direct3D9ImmediateBrushRealizer? brushRealizer);

internal delegate int Direct3D9DeriveMeshHardwareBrush(
    nint brush,
    Direct3D9ProjectedMeshState brushContext,
    out Direct3D9MeshHardwareBrush? hardwareBrush);

internal delegate int Direct3D9ProcessMeshEffects(
    nint effects,
    Direct3D9ProjectedMeshState effectContext,
    Direct3D9FixedFunctionPipelineItemBuilder builder);

internal delegate int Direct3D9DrawFixedFunctionMeshShader(
    Direct3D9FixedFunctionPassInputs passInputs,
    MilCompositingMode compositingMode,
    Direct3D9FixedFunctionLightingValues requiredLightingValues);

internal delegate int Direct3D9InitializeFixedFunctionMeshShaderPipeline(
    Direct3D9FixedFunctionPassInputs passInputs,
    MilCompositingMode compositingMode,
    ref Direct3D9GeometryRenderer<uint> renderer,
    out Direct3D9Pipeline? pipeline);

internal delegate int Direct3D9InitializeShaderMeshPipeline(
    Direct3D9FixedFunctionPassInputs passInputs,
    MilCompositingMode compositingMode,
    ref Direct3D9GeometryRenderer<Vector3> renderer,
    out Direct3D9Pipeline? pipeline);

internal sealed class Direct3D9ShaderMeshDrawData
{
    internal Direct3D9ShaderMeshDrawData(
        uint passCount,
        ReadOnlyMemory<Vector3> positions,
        ReadOnlyMemory<Vector3> normals,
        ReadOnlyMemory<Vector2> textureCoordinates,
        ReadOnlyMemory<uint> indices,
        Direct3D9InitializeShaderMeshPipeline initializePipeline,
        Direct3D9CreateLightingColorSource? createLightingColorSource = null,
        Action<Direct3D9FixedFunctionLightingValues>? setLightingPass = null)
    {
        ArgumentOutOfRangeException.ThrowIfZero(passCount);
        ArgumentNullException.ThrowIfNull(initializePipeline);

        PassCount = passCount;
        Positions = positions;
        Normals = normals;
        TextureCoordinates = textureCoordinates;
        Indices = indices;
        InitializePipeline = initializePipeline;
        CreateLightingColorSource = createLightingColorSource;
        SetLightingPass = setLightingPass;
    }

    internal uint PassCount { get; }
    internal ReadOnlyMemory<Vector3> Positions { get; }
    internal ReadOnlyMemory<Vector3> Normals { get; }
    internal ReadOnlyMemory<Vector2> TextureCoordinates { get; }
    internal ReadOnlyMemory<uint> Indices { get; }
    internal Direct3D9InitializeShaderMeshPipeline InitializePipeline { get; }
    internal Direct3D9CreateLightingColorSource? CreateLightingColorSource { get; }
    internal Action<Direct3D9FixedFunctionLightingValues>? SetLightingPass { get; }
}

internal sealed class Direct3D9FixedFunctionMeshDrawData
{
    internal Direct3D9FixedFunctionMeshDrawData(
        uint passCount,
        ReadOnlyMemory<Vector3> positions,
        ReadOnlyMemory<Vector2> textureCoordinates,
        ReadOnlyMemory<uint> indices,
        ReadOnlyMemory<uint> diffuseColors,
        ReadOnlyMemory<uint> specularColors,
        MilColorF materialEmissiveColor,
        Func<int> precomputeLighting,
        Direct3D9FixedFunctionMeshPipelineInitializer pipelineInitializer,
        Direct3D9CreateLightingColorSource? createLightingColorSource = null)
        : this(
            passCount,
            positions,
            textureCoordinates,
            indices,
            diffuseColors,
            specularColors,
            materialEmissiveColor,
            precomputeLighting,
            (pipelineInitializer ?? throw new ArgumentNullException(nameof(pipelineInitializer))).Initialize,
            createLightingColorSource)
    {
    }

    internal Direct3D9FixedFunctionMeshDrawData(
        uint passCount,
        ReadOnlyMemory<Vector3> positions,
        ReadOnlyMemory<Vector2> textureCoordinates,
        ReadOnlyMemory<uint> indices,
        ReadOnlyMemory<uint> diffuseColors,
        ReadOnlyMemory<uint> specularColors,
        MilColorF materialEmissiveColor,
        Func<int> precomputeLighting,
        Direct3D9InitializeFixedFunctionMeshShaderPipeline initializePipeline,
        Direct3D9CreateLightingColorSource? createLightingColorSource = null)
    {
        ArgumentOutOfRangeException.ThrowIfZero(passCount);
        ArgumentNullException.ThrowIfNull(precomputeLighting);
        ArgumentNullException.ThrowIfNull(initializePipeline);

        PassCount = passCount;
        Positions = positions;
        TextureCoordinates = textureCoordinates;
        Indices = indices;
        DiffuseColors = diffuseColors;
        SpecularColors = specularColors;
        MaterialEmissiveColor = materialEmissiveColor;
        PrecomputeLighting = precomputeLighting;
        InitializePipeline = initializePipeline;
        CreateLightingColorSource = createLightingColorSource;
    }

    internal uint PassCount { get; }
    internal ReadOnlyMemory<Vector3> Positions { get; }
    internal ReadOnlyMemory<Vector2> TextureCoordinates { get; }
    internal ReadOnlyMemory<uint> Indices { get; }
    internal ReadOnlyMemory<uint> DiffuseColors { get; }
    internal ReadOnlyMemory<uint> SpecularColors { get; }
    internal MilColorF MaterialEmissiveColor { get; }
    internal Func<int> PrecomputeLighting { get; }
    internal Direct3D9InitializeFixedFunctionMeshShaderPipeline InitializePipeline { get; }
    internal Direct3D9CreateLightingColorSource? CreateLightingColorSource { get; }
}

internal sealed class Direct3D9MeshHardwareBrush : IDisposable
{
    private readonly Func<Direct3D9FixedFunctionPipelineItemBuilder, int> _sendOperations;
    private readonly Action _release;
    private bool _isDisposed;

    internal Direct3D9MeshHardwareBrush(
        Func<Direct3D9FixedFunctionPipelineItemBuilder, int> sendOperations,
        Action release)
    {
        ArgumentNullException.ThrowIfNull(sendOperations);
        ArgumentNullException.ThrowIfNull(release);

        _sendOperations = sendOperations;
        _release = release;
    }

    internal int SendOperations(Direct3D9FixedFunctionPipelineItemBuilder builder)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(builder);
        return _sendOperations(builder);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _release();
        _isDisposed = true;
    }
}

internal sealed class Direct3D9FixedFunctionMeshShaderFactory
{
    private readonly Func<Renderstatetype, uint, int> _setRenderState;
    private readonly Direct3D9DeriveMeshHardwareBrush _deriveHardwareBrush;
    private readonly Direct3D9ProcessMeshEffects? _processEffects;
    private readonly Direct3D9DrawFixedFunctionMeshShader _drawFixedFunction;
    private readonly Action<nint> _addRefEffectList;
    private readonly Action<nint> _releaseEffectList;
    private readonly bool _zBufferEnabled;

    internal Direct3D9FixedFunctionMeshShaderFactory(
        Func<Renderstatetype, uint, int> setRenderState,
        Direct3D9DeriveMeshHardwareBrush deriveHardwareBrush,
        Direct3D9DrawFixedFunctionMeshShader drawFixedFunction,
        Action<nint> addRefEffectList,
        Action<nint> releaseEffectList,
        bool zBufferEnabled,
        Direct3D9ProcessMeshEffects? processEffects = null)
    {
        ArgumentNullException.ThrowIfNull(setRenderState);
        ArgumentNullException.ThrowIfNull(deriveHardwareBrush);
        ArgumentNullException.ThrowIfNull(drawFixedFunction);
        ArgumentNullException.ThrowIfNull(addRefEffectList);
        ArgumentNullException.ThrowIfNull(releaseEffectList);

        _setRenderState = setRenderState;
        _deriveHardwareBrush = deriveHardwareBrush;
        _processEffects = processEffects;
        _drawFixedFunction = drawFixedFunction;
        _addRefEffectList = addRefEffectList;
        _releaseEffectList = releaseEffectList;
        _zBufferEnabled = zBufferEnabled;
    }

    internal int Derive(
        Direct3D9MeshShaderType shaderType,
        Direct3D9GetMeshShaderSurfaceSource getSurfaceSource,
        Direct3D9ProjectedMeshState brushContext,
        out Direct3D9DerivedMeshShader? shader)
    {
        return DeriveCore(shaderType, getSurfaceSource, brushContext, null, null, out shader);
    }

    private int DeriveCore(
        Direct3D9MeshShaderType shaderType,
        Direct3D9GetMeshShaderSurfaceSource getSurfaceSource,
        Direct3D9ProjectedMeshState brushContext,
        Direct3D9ShaderMeshDrawData? shaderDrawData,
        Direct3D9Device? device,
        out Direct3D9DerivedMeshShader? shader)
    {
        ArgumentNullException.ThrowIfNull(getSurfaceSource);

        shader = null;
        int result = getSurfaceSource(out Direct3D9ImmediateBrushRealizer? brushRealizer);
        if (result < 0)
        {
            brushRealizer?.Release();
            return result;
        }

        if (brushRealizer is null)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        try
        {
            nint brush = brushRealizer.GetRealizedBrush(convertNullToTransparent: true);
            if (brush == 0)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            result = _deriveHardwareBrush(brush, brushContext, out Direct3D9MeshHardwareBrush? hardwareBrush);
            if (result < 0)
            {
                hardwareBrush?.Dispose();
                return result;
            }

            if (hardwareBrush is null)
            {
                return Direct3D9Factory.InternalErrorHResult;
            }

            return CreateShader(
                shaderType,
                hardwareBrush,
                brushRealizer.Effects,
                brushContext,
                shaderDrawData,
                device,
                out shader);
        }
        finally
        {
            brushRealizer.Release();
        }
    }

    internal int Derive(
        Direct3D9MeshShaderType shaderType,
        Direct3D9GetMeshShaderSurfaceSource getSurfaceSource,
        Direct3D9ProjectedMeshState brushContext,
        Direct3D9FixedFunctionMeshDrawData drawData,
        Direct3D9Device device,
        out Direct3D9DerivedMeshShader? shader)
    {
        return Derive(shaderType, getSurfaceSource, brushContext, drawData, null, device, out shader);
    }

    internal int Derive(
        Direct3D9MeshShaderType shaderType,
        Direct3D9GetMeshShaderSurfaceSource getSurfaceSource,
        Direct3D9ProjectedMeshState brushContext,
        Direct3D9FixedFunctionMeshDrawData fixedFunctionDrawData,
        Direct3D9ShaderMeshDrawData? shaderDrawData,
        Direct3D9Device device,
        out Direct3D9DerivedMeshShader? shader)
    {
        ArgumentNullException.ThrowIfNull(fixedFunctionDrawData);
        ArgumentNullException.ThrowIfNull(device);

        Direct3D9FixedFunctionMeshShaderFactory factory = new(
            _setRenderState,
            _deriveHardwareBrush,
            (passInputs, compositingMode, lightingValues) =>
                Direct3D9FixedFunctionMeshRenderer.Render(
                    fixedFunctionDrawData.PassCount,
                    lightingValues,
                    fixedFunctionDrawData.Positions.Span,
                    fixedFunctionDrawData.TextureCoordinates.Span,
                    fixedFunctionDrawData.Indices.Span,
                    fixedFunctionDrawData.DiffuseColors.Span,
                    fixedFunctionDrawData.SpecularColors.Span,
                    fixedFunctionDrawData.MaterialEmissiveColor,
                    zBufferEnabled: false,
                    device,
                    fixedFunctionDrawData.PrecomputeLighting,
                    (uint _, MilCompositingMode mode, ref Direct3D9GeometryRenderer<uint> renderer, out Direct3D9Pipeline? pipeline) =>
                        fixedFunctionDrawData.InitializePipeline(passInputs, mode, ref renderer, out pipeline),
                    fixedFunctionDrawData.CreateLightingColorSource),
            _addRefEffectList,
            _releaseEffectList,
            _zBufferEnabled,
            _processEffects);
        return factory.DeriveCore(
            shaderType,
            getSurfaceSource,
            brushContext,
            shaderDrawData,
            device,
            out shader);
    }

    private int CreateShader(
        Direct3D9MeshShaderType shaderType,
        Direct3D9MeshHardwareBrush hardwareBrush,
        nint effects,
        Direct3D9ProjectedMeshState effectContext,
        Direct3D9ShaderMeshDrawData? shaderDrawData,
        Direct3D9Device? device,
        out Direct3D9DerivedMeshShader? shader)
    {
        shader = null;
        bool effectListRetained = false;

        try
        {
            if (effects != 0)
            {
                _addRefEffectList(effects);
                effectListRetained = true;
            }

            MilCompositingMode compositingMode = shaderType == Direct3D9MeshShaderType.Diffuse
                ? MilCompositingMode.SourceOver
                : MilCompositingMode.SourceAdd;
            Direct3D9FixedFunctionLightingValues lightingValues = shaderType switch
            {
                Direct3D9MeshShaderType.Diffuse => Direct3D9FixedFunctionLightingValues.Diffuse,
                Direct3D9MeshShaderType.Specular => Direct3D9FixedFunctionLightingValues.Specular,
                Direct3D9MeshShaderType.Emissive => Direct3D9FixedFunctionLightingValues.Emissive,
                _ => throw new ArgumentOutOfRangeException(nameof(shaderType))
            };

            Direct3D9FixedFunctionPassInputs passInputs = CreatePassInputs(hardwareBrush, effects, effectContext);
            shader = new Direct3D9DerivedMeshShader(
                () => Begin(shaderType),
                () => shaderDrawData is not null
                    && device is not null
                    && device.PixelShaderVersion >= 0xFFFF0200
                    && device.VertexShaderVersion >= 0xFFFE0200,
                () => shaderDrawData is null || device is null
                    ? Direct3D9Factory.UnsupportedOperationHResult
                    : Direct3D9ShaderMeshRenderer.Render(
                        shaderDrawData.PassCount,
                        shaderDrawData.Positions.Span,
                        shaderDrawData.Normals.Span,
                        shaderDrawData.TextureCoordinates.Span,
                        shaderDrawData.Indices.Span,
                        device,
                        (uint _, ref Direct3D9GeometryRenderer<Vector3> renderer, out Direct3D9Pipeline? pipeline) =>
                            shaderDrawData.InitializePipeline(passInputs, compositingMode, ref renderer, out pipeline),
                        shaderDrawData.CreateLightingColorSource,
                        lightingValues,
                        shaderDrawData.SetLightingPass),
                () => _drawFixedFunction(passInputs, compositingMode, lightingValues),
                static () => Direct3D9Factory.SuccessHResult,
                () =>
                {
                    hardwareBrush.Dispose();
                    if (effects != 0)
                    {
                        _releaseEffectList(effects);
                    }
                });
            return Direct3D9Factory.SuccessHResult;
        }
        catch (OutOfMemoryException)
        {
            hardwareBrush.Dispose();
            if (effectListRetained)
            {
                _releaseEffectList(effects);
            }

            return Direct3D9Factory.OutOfMemoryHResult;
        }
    }

    private int Begin(Direct3D9MeshShaderType shaderType)
    {
        if (!_zBufferEnabled)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        uint zWriteEnabled = shaderType == Direct3D9MeshShaderType.Diffuse ? 1u : 0u;
        return _setRenderState(Renderstatetype.Zwriteenable, zWriteEnabled);
    }

    private Direct3D9FixedFunctionPassInputs CreatePassInputs(
        Direct3D9MeshHardwareBrush hardwareBrush,
        nint effects,
        Direct3D9ProjectedMeshState effectContext)
    {
        return new Direct3D9FixedFunctionPassInputs(
            builder =>
            {
                int result = hardwareBrush.SendOperations(builder);
                if (result >= 0 && effects != 0 && _processEffects is not null)
                {
                    result = _processEffects(effects, effectContext, builder);
                }

                return result;
            },
            sendGeometryModifiers: Direct3D9GeometryRenderer<uint>.SendGeometryModifiers);
    }
}
