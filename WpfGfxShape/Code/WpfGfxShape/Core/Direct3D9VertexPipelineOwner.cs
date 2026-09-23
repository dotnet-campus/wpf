using System.Numerics;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9CreateVertexBufferBuilder(
    Func<int> realizePipeline,
    out Direct3D9VertexBufferBuilder? builder);

internal delegate int Direct3D9CreateVertexConverter(
    out Direct3D9ExpandedVertexConverter? converter);

internal sealed class Direct3D9VertexPipelineOwner : IDisposable
{
    private readonly Direct3D9CreateVertexBufferBuilder? _createBuilder;
    private readonly Direct3D9CreateVertexConverter? _createConverter;
    private readonly Direct3D9DrawBuilderBatch? _drawBatch;
    private readonly Direct3D9PackBuilderVertices? _packVertices;
    private Direct3D9VertexBufferBuilder? _builder;
    private Direct3D9VertexBufferBuilder? _cachedVertexBuffer;
    private bool _disposed;

    internal Direct3D9VertexPipelineOwner(Direct3D9CreateVertexBufferBuilder createBuilder)
    {
        ArgumentNullException.ThrowIfNull(createBuilder);
        _createBuilder = createBuilder;
    }

    internal Direct3D9VertexPipelineOwner(
        Direct3D9CreateVertexConverter createConverter,
        Direct3D9DrawBuilderBatch drawBatch,
        Direct3D9PackBuilderVertices? packVertices = null)
    {
        ArgumentNullException.ThrowIfNull(createConverter);
        ArgumentNullException.ThrowIfNull(drawBatch);
        _createConverter = createConverter;
        _drawBatch = drawBatch;
        _packVertices = packVertices;
    }

    internal Direct3D9VertexBufferBuilder? Builder => _builder;

    internal Direct3D9VertexBufferBuilder? CachedVertexBuffer => _cachedVertexBuffer;

    internal int CreateBuilder(Func<int> realizePipeline)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(realizePipeline);
        ReleaseBuilder();

        Direct3D9VertexBufferBuilder? builder;
        int result;
        if (_createBuilder is not null)
        {
            result = _createBuilder(realizePipeline, out builder);
        }
        else
        {
            result = _createConverter!(out Direct3D9ExpandedVertexConverter? converter);
            builder = result < 0 || converter is null
                ? null
                : new Direct3D9VertexBufferBuilder(converter, _drawBatch!, realizePipeline, _packVertices);
        }

        if (result < 0)
        {
            return result;
        }

        if (builder is null)
        {
            return Direct3D9Factory.InternalErrorHResult;
        }

        _builder = builder;
        return Direct3D9Factory.SuccessHResult;
    }

    internal int SetTextureMapping(uint destinationCoordinateIndex, uint sourceCoordinateIndex, Matrix3x2 pointToTexture) =>
        GetBuilder().SetTextureMapping(destinationCoordinateIndex, sourceCoordinateIndex, pointToTexture);

    internal int SetWaffling(
        uint coordinateIndex,
        Matrix3x2 pointToTexture,
        Direct3D9WaffleTextureSubrect subrect,
        Direct3D9WaffleMode mode) =>
        GetBuilder().SetWaffling(coordinateIndex, pointToTexture, subrect, mode);

    internal int SetConstantMapping(Direct3D9VertexFormatAttribute location, uint premultipliedSrgbColor) =>
        GetBuilder().SetConstantMapping(location, premultipliedSrgbColor);

    internal int FinalizeMappings() => GetBuilder().FinalizeMappings();

    internal void SetOutsideBounds(Direct3D9SurfaceRect bounds, bool needInside) =>
        GetBuilder().SetOutsideBounds(bounds, needInside);

    internal int BeginBuilding()
    {
        GetBuilder().BeginBuilding();
        return Direct3D9Factory.SuccessHResult;
    }

    internal bool HasOutsideBounds => GetBuilder().HasOutsideBounds;

    internal int FlushTryGetVertexBuffer()
    {
        Direct3D9VertexBufferBuilder builder = GetBuilder();
        int result = builder.FlushTryGetVertexBuffer(out Direct3D9VertexBufferBuilder? vertexBuffer);
        if (result >= 0)
        {
            _cachedVertexBuffer = vertexBuffer;
        }

        return result;
    }

    internal int DrawCachedVertexBuffer()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _cachedVertexBuffer?.DrawCached() ?? Direct3D9Factory.InvalidCallHResult;
    }

    internal int AddComplexScan(
        int pixelY,
        IReadOnlyList<Direct3D9CoverageInterval> intervals,
        float viewportTop,
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates) =>
        GetBuilder().AddComplexScan(pixelY, intervals, viewportTop, textureCoordinates);

    internal void SetPrecomputedIndexedTriangles(
        ReadOnlySpan<Direct3D9ExpandedVertex> vertices,
        ReadOnlySpan<uint> indices) =>
        GetBuilder().SetPrecomputedIndexedTriangles(vertices, indices);

    internal void AddIndexedTriangleVertices(
        ReadOnlySpan<Direct3D9ExpandedVertex> vertices,
        ReadOnlySpan<ushort> indices) =>
        GetBuilder().AddIndexedTriangleVertices(vertices, indices);

    internal void AddTriangleListVertices(ReadOnlySpan<Direct3D9ExpandedVertex> vertices) =>
        GetBuilder().AddTriangleListVertices(vertices);

    internal void ReleaseBuilder() => _builder = null;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _builder = null;
        _cachedVertexBuffer = null;
    }

    private Direct3D9VertexBufferBuilder GetBuilder()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _builder ?? throw new InvalidOperationException("The vertex builder has not been created.");
    }
}
