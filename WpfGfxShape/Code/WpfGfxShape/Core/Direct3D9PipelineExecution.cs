namespace WpfGfxShape.Core;

internal sealed class Direct3D9Pipeline
{
    private readonly IReadOnlyList<Direct3D9PipelineColorSource?> _colorSources;
    private readonly IReadOnlyList<IDisposable?> _ownedColorSources;
    private readonly Func<nint, int>? _sendDeviceStates;
    private readonly Func<nint, int>? _drawPrimitive;
    private readonly Func<int>? _beginBuilding;
    private readonly Func<int>? _sendGeometry;
    private readonly Func<bool>? _hasOutsideBounds;
    private readonly Direct3D9FlushPipelineVertexBuilder? _flushTryGetVertexBuffer;
    private readonly Action _releaseColorSources;
    private readonly Action? _releaseVertexBuilder;
    private readonly Direct3D9VertexPipelineOwner? _vertexOwner;
    private readonly Func<int>? _sendTypedDeviceStates;
    private readonly Direct3D9SendGeometryToVertexBuilder? _sendTypedGeometry;
    private nint _geometryGenerator;
    private nint _vertexBuffer;
    private bool _hasVertexBuilder;
    private bool _hasTypedGeometry;
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

    internal Direct3D9Pipeline(
        nint geometryGenerator,
        IReadOnlyList<Direct3D9PipelineColorSource?> colorSources,
        Direct3D9VertexPipelineOwner vertexOwner,
        Func<int> sendDeviceStates,
        Direct3D9SendGeometryToVertexBuilder sendGeometry,
        Action releaseColorSources,
        IReadOnlyList<IDisposable?>? ownedColorSources = null)
        : this(colorSources, vertexOwner, sendDeviceStates, sendGeometry, releaseColorSources, ownedColorSources)
    {
        ArgumentOutOfRangeException.ThrowIfZero(geometryGenerator);
        _geometryGenerator = geometryGenerator;
    }

    internal Direct3D9Pipeline(
        IReadOnlyList<Direct3D9PipelineColorSource?> colorSources,
        Direct3D9VertexPipelineOwner vertexOwner,
        Func<int> sendDeviceStates,
        Direct3D9SendGeometryToVertexBuilder sendGeometry,
        Action releaseColorSources,
        IReadOnlyList<IDisposable?>? ownedColorSources = null)
    {
        ArgumentNullException.ThrowIfNull(colorSources);
        ArgumentNullException.ThrowIfNull(vertexOwner);
        ArgumentNullException.ThrowIfNull(sendDeviceStates);
        ArgumentNullException.ThrowIfNull(sendGeometry);
        ArgumentNullException.ThrowIfNull(releaseColorSources);

        _colorSources = colorSources;
        _ownedColorSources = ownedColorSources ?? [];
        _vertexOwner = vertexOwner;
        _sendTypedDeviceStates = sendDeviceStates;
        _sendTypedGeometry = sendGeometry;
        _releaseColorSources = releaseColorSources;
        _hasVertexBuilder = true;
        _hasTypedGeometry = true;
    }

    internal nint GeometryGenerator => _geometryGenerator;

    internal int Execute()
    {
        if (_vertexOwner is not null)
        {
            return ExecuteTyped();
        }

        if (_geometryGenerator == 0 || (!_hasVertexBuilder && _vertexBuffer == 0))
        {
            throw new InvalidOperationException("The pipeline must be initialized before execution.");
        }

        if (_vertexBuffer != 0)
        {
            int result = _sendDeviceStates!(_vertexBuffer);
            return result < 0 ? result : _drawPrimitive!(_vertexBuffer);
        }

        int buildResult = _beginBuilding!();
        if (buildResult < 0)
        {
            return buildResult;
        }

        buildResult = _sendGeometry!();
        if (buildResult < 0)
        {
            return buildResult;
        }

        if (buildResult == Direct3D9Factory.EmptyFillHResult && !_hasOutsideBounds!())
        {
            return Direct3D9Factory.SuccessHResult;
        }

        buildResult = _flushTryGetVertexBuffer!(out _vertexBuffer);
        if (buildResult < 0)
        {
            _vertexBuffer = 0;
            return buildResult;
        }

        ReleaseVertexBuilder();
        return Direct3D9Factory.SuccessHResult;
    }

    private int ExecuteTyped()
    {
        if (!_hasTypedGeometry || (!_hasVertexBuilder && _vertexOwner!.CachedVertexBuffer is null))
        {
            throw new InvalidOperationException("The pipeline must be initialized before execution.");
        }

        if (_vertexOwner!.CachedVertexBuffer is not null)
        {
            int cachedStateResult = _sendTypedDeviceStates!();
            return cachedStateResult < 0 ? cachedStateResult : _vertexOwner.DrawCachedVertexBuffer();
        }

        int result = _vertexOwner.BeginBuilding();
        if (result < 0)
        {
            return result;
        }

        Direct3D9VertexBufferBuilder builder = _vertexOwner.Builder
            ?? throw new InvalidOperationException("The pipeline vertex builder is unavailable.");
        result = _sendTypedGeometry!(builder);
        if (result < 0)
        {
            return result;
        }

        if (result == Direct3D9Factory.EmptyFillHResult && !builder.HasOutsideBounds)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        result = _vertexOwner.FlushTryGetVertexBuffer();
        if (result < 0)
        {
            return result;
        }

        _vertexOwner.ReleaseBuilder();
        _hasVertexBuilder = false;
        return Direct3D9Factory.SuccessHResult;
    }

    internal int RealizeColorSourcesAndSendState(nint vertexBuffer)
    {
        int result = RealizeColorSources();
        return result < 0 ? result : _sendDeviceStates!(vertexBuffer);
    }

    internal int RealizeColorSourcesAndSendState()
    {
        int result = RealizeColorSources();
        return result < 0 ? result : _sendTypedDeviceStates!();
    }

    internal void ReleaseExpensiveResources()
    {
        if (_hasColorSources)
        {
            _hasColorSources = false;
            for (int index = _ownedColorSources.Count - 1; index >= 0; index--)
            {
                _ownedColorSources[index]?.Dispose();
            }

            _releaseColorSources();
        }

        ReleaseVertexBuilder();
        _vertexOwner?.Dispose();
        _geometryGenerator = 0;
        _hasTypedGeometry = false;
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
        if (_vertexOwner is not null)
        {
            _vertexOwner.ReleaseBuilder();
        }
        else
        {
            _releaseVertexBuilder!();
        }
    }
}
