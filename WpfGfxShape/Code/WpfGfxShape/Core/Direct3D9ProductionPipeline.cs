namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9ProductionPipelineMapping(
    Direct3D9PipelineColorSource? ColorSource,
    Direct3D9VertexFormatAttribute Location,
    bool IsConstant);

internal sealed class Direct3D9ProductionPipelineInitializer
{
    private readonly IReadOnlyList<Direct3D9ProductionPipelineMapping> _mappings;
    private readonly IReadOnlyList<Direct3D9PipelineColorSource?> _colorSources;
    private readonly IReadOnlyList<IDisposable?> _ownedColorSources;
    private readonly Direct3D9VertexPipelineOwner _vertexOwner;
    private readonly Func<int> _sendDeviceStates;
    private readonly Action _releaseColorSources;
    private bool _initialized;

    internal Direct3D9ProductionPipelineInitializer(
        IReadOnlyList<Direct3D9PipelineColorSource?> colorSources,
        Direct3D9VertexPipelineOwner vertexOwner,
        Func<int> sendDeviceStates,
        Action releaseColorSources,
        IReadOnlyList<IDisposable?>? ownedColorSources = null,
        IReadOnlyList<Direct3D9ProductionPipelineMapping>? mappings = null)
    {
        ArgumentNullException.ThrowIfNull(colorSources);
        ArgumentNullException.ThrowIfNull(vertexOwner);
        ArgumentNullException.ThrowIfNull(sendDeviceStates);
        ArgumentNullException.ThrowIfNull(releaseColorSources);
        _colorSources = colorSources;
        _mappings = mappings ?? colorSources
            .Select(static colorSource => new Direct3D9ProductionPipelineMapping(
                colorSource,
                Direct3D9VertexFormatAttribute.Uv1,
                IsConstant: false))
            .ToArray();
        _vertexOwner = vertexOwner;
        _sendDeviceStates = sendDeviceStates;
        _releaseColorSources = releaseColorSources;
        _ownedColorSources = ownedColorSources ?? [];
    }

    internal static Direct3D9ProductionPipelineInitializer CreateShader(
        IReadOnlyList<Direct3D9ShaderPipelineItem> items,
        Direct3D9VertexPipelineOwner vertexOwner,
        Func<int> sendDeviceStates,
        Action releaseColorSources)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new Direct3D9ProductionPipelineInitializer(
            items.Select(static item => item.ColorSource).ToArray(),
            vertexOwner,
            sendDeviceStates,
            releaseColorSources,
            items.Select(static item => item.LifetimeOwner ?? item.Ownership).ToArray(),
            items.Select(static item => new Direct3D9ProductionPipelineMapping(
                item.ColorSource,
                item.TextureCoordinates,
                item.TextureCoordinates is Direct3D9VertexFormatAttribute.Diffuse or Direct3D9VertexFormatAttribute.Specular)).ToArray());
    }

    internal static Direct3D9ProductionPipelineInitializer CreateFixedFunction(
        IReadOnlyList<Direct3D9FixedFunctionPipelineItem> items,
        Direct3D9VertexPipelineOwner vertexOwner,
        Func<int> sendDeviceStates,
        Action releaseColorSources)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new Direct3D9ProductionPipelineInitializer(
            items.Select(static item => item.ColorSource).ToArray(),
            vertexOwner,
            sendDeviceStates,
            releaseColorSources,
            items.Select(static item => item.LifetimeOwner ?? item.Ownership).ToArray(),
            items.Select(static item => new Direct3D9ProductionPipelineMapping(
                item.ColorSource,
                item.SourceLocation,
                item.SourceLocation is Direct3D9VertexFormatAttribute.Diffuse or Direct3D9VertexFormatAttribute.Specular)).ToArray());
    }

    internal int Initialize(
        nint geometryGenerator,
        Direct3D9SendGeometryToVertexBuilder sendGeometry,
        bool verticesArePreGenerated,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        out Direct3D9Pipeline? pipeline)
    {
        ArgumentOutOfRangeException.ThrowIfZero(geometryGenerator);
        return InitializeCore(geometryGenerator, sendGeometry, verticesArePreGenerated, outsideBounds, needInside, out pipeline);
    }

    internal int Initialize(
        Direct3D9PathGeometryGenerator geometryGenerator,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        out Direct3D9Pipeline? pipeline)
    {
        ArgumentNullException.ThrowIfNull(geometryGenerator);
        return InitializeCore(null, geometryGenerator.SendGeometry, geometryGenerator.VerticesArePreGenerated, outsideBounds, needInside, out pipeline);
    }

    private int InitializeCore(
        nint? geometryGenerator,
        Direct3D9SendGeometryToVertexBuilder sendGeometry,
        bool verticesArePreGenerated,
        Direct3D9SurfaceRect? outsideBounds,
        bool needInside,
        out Direct3D9Pipeline? pipeline)
    {
        ArgumentNullException.ThrowIfNull(sendGeometry);
        if (_initialized)
        {
            throw new InvalidOperationException("The production pipeline initializer has already been used.");
        }

        pipeline = null;
        Direct3D9Pipeline? realizingPipeline = null;
        int result = _vertexOwner.CreateBuilder(() => realizingPipeline?.RealizeColorSourcesAndSendState() ?? Direct3D9Factory.InvalidCallHResult);
        if (result < 0)
        {
            ReleaseFailureResources();
            return result;
        }

        Direct3D9VertexBufferBuilder builder = _vertexOwner.Builder
            ?? throw new InvalidOperationException("Vertex builder creation succeeded without returning a builder.");
        Direct3D9VertexBufferBuilder? mappingBuilder = verticesArePreGenerated ? null : builder;
        foreach (Direct3D9ProductionPipelineMapping mapping in _mappings)
        {
            Direct3D9PipelineColorSource? colorSource = mapping.ColorSource;
            if (colorSource is null)
            {
                continue;
            }

            if (mapping.IsConstant && colorSource.SendBuilderConstantVertexMapping is not null)
            {
                result = colorSource.SendBuilderConstantVertexMapping(mappingBuilder, mapping.Location);
            }
            else if (!mapping.IsConstant && colorSource.SendBuilderVertexMapping is not null)
            {
                result = colorSource.SendBuilderVertexMapping(mappingBuilder, mapping.Location);
            }
            else
            {
                continue;
            }

            if (result < 0)
            {
                ReleaseFailureResources();
                return result;
            }
        }

        result = builder.FinalizeMappings();
        if (result < 0)
        {
            ReleaseFailureResources();
            return result;
        }

        if (outsideBounds.HasValue)
        {
            builder.SetOutsideBounds(outsideBounds.Value, needInside);
        }

        pipeline = geometryGenerator.HasValue
            ? new Direct3D9Pipeline(
                geometryGenerator.Value,
                _colorSources,
                _vertexOwner,
                _sendDeviceStates,
                sendGeometry,
                _releaseColorSources,
                _ownedColorSources)
            : new Direct3D9Pipeline(
                _colorSources,
                _vertexOwner,
                _sendDeviceStates,
                sendGeometry,
                _releaseColorSources,
                _ownedColorSources);
        realizingPipeline = pipeline;
        _initialized = true;
        return Direct3D9Factory.SuccessHResult;
    }

    private void ReleaseFailureResources()
    {
        _vertexOwner.Dispose();
        for (int index = _ownedColorSources.Count - 1; index >= 0; index--)
        {
            _ownedColorSources[index]?.Dispose();
        }

        _releaseColorSources();
    }
}
