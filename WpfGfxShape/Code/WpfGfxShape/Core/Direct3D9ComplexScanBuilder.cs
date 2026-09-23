namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9ComplexScanVertex(float X, float Y, float Alpha);

internal delegate int Direct3D9PrepareComplexScanStratum(float top, float bottom);

internal delegate int Direct3D9AllocateComplexScanVertices(
    int vertexCount,
    out Memory<Direct3D9ComplexScanVertex> vertices);

internal delegate int Direct3D9AllocateExpandedVertices(
    int vertexCount,
    out Memory<Direct3D9ExpandedVertex> vertices);

internal sealed class Direct3D9ComplexScanBuilder
{
    private readonly Direct3D9PrepareComplexScanStratum _prepareStratum;
    private readonly Direct3D9AllocateComplexScanVertices _allocateLineListVertices;
    private readonly Direct3D9AllocateComplexScanVertices _allocateTriangleStripVertices;
    private readonly Direct3D9AllocateExpandedVertices? _allocateExpandedLineListVertices;
    private readonly Direct3D9AllocateExpandedVertices? _allocateExpandedTriangleStripVertices;
    private readonly Direct3D9ExpandedVertexConverter? _expandedVertexConverter;
    private readonly Direct3D9ComplexScanLineBuilder _lineBuilder;
    private readonly bool _needInsideGeometry;
    private readonly Direct3D9SurfaceRect? _outsideBounds;

    internal Direct3D9ComplexScanBuilder(
        float viewportTop,
        bool needInsideGeometry,
        Direct3D9SurfaceRect? outsideBounds,
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates,
        Direct3D9PrepareComplexScanStratum prepareStratum,
        Direct3D9AllocateComplexScanVertices allocateLineListVertices,
        Direct3D9AllocateComplexScanVertices allocateTriangleStripVertices)
    {
        ArgumentNullException.ThrowIfNull(textureCoordinates);
        ArgumentNullException.ThrowIfNull(prepareStratum);
        ArgumentNullException.ThrowIfNull(allocateLineListVertices);
        ArgumentNullException.ThrowIfNull(allocateTriangleStripVertices);
        if (!needInsideGeometry && outsideBounds is null)
        {
            throw new ArgumentException("Inside geometry can only be omitted when outside bounds are provided.", nameof(needInsideGeometry));
        }

        _prepareStratum = prepareStratum;
        _allocateLineListVertices = allocateLineListVertices;
        _allocateTriangleStripVertices = allocateTriangleStripVertices;
        _needInsideGeometry = needInsideGeometry;
        _outsideBounds = outsideBounds;
        _lineBuilder = new Direct3D9ComplexScanLineBuilder(
            viewportTop,
            textureCoordinates,
            AddDirectLineNotAllocated,
            AddLineAsTriangleStrip);
    }

    internal Direct3D9ComplexScanBuilder(
        float viewportTop,
        bool needInsideGeometry,
        Direct3D9SurfaceRect? outsideBounds,
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates,
        Direct3D9PrepareComplexScanStratum prepareStratum,
        Direct3D9ExpandedVertexConverter expandedVertexConverter,
        Direct3D9AllocateExpandedVertices allocateLineListVertices,
        Direct3D9AllocateExpandedVertices allocateTriangleStripVertices)
        : this(
            viewportTop,
            needInsideGeometry,
            outsideBounds,
            textureCoordinates,
            prepareStratum,
            AllocateBasicVerticesNotSupported,
            AllocateBasicVerticesNotSupported)
    {
        ArgumentNullException.ThrowIfNull(expandedVertexConverter);
        ArgumentNullException.ThrowIfNull(allocateLineListVertices);
        ArgumentNullException.ThrowIfNull(allocateTriangleStripVertices);
        _expandedVertexConverter = expandedVertexConverter;
        _allocateExpandedLineListVertices = allocateLineListVertices;
        _allocateExpandedTriangleStripVertices = allocateTriangleStripVertices;
    }

    internal int AddComplexScan(int pixelY, IReadOnlyList<Direct3D9CoverageInterval> intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);

        int result = _prepareStratum(pixelY, pixelY + 1f);
        if (result < 0)
        {
            return result;
        }

        Direct3D9ComplexScanIntervalBuilder intervalBuilder;
        if (_lineBuilder.SelectPath(pixelY + 0.5f) == Direct3D9ComplexScanLinePath.DirectLineList)
        {
            intervalBuilder = new Direct3D9ComplexScanIntervalBuilder(
                _needInsideGeometry,
                _outsideBounds,
                AddDirectLineNotAllocated);
            int segmentCount = intervalBuilder.CountSegments(intervals);
            if (segmentCount == 0)
            {
                return 0;
            }

            int vertexIndex = 0;
            if (_expandedVertexConverter is not null)
            {
                result = _allocateExpandedLineListVertices!(segmentCount * 2, out Memory<Direct3D9ExpandedVertex> vertices);
                if (result < 0)
                {
                    return result;
                }

                if (vertices.Length < segmentCount * 2)
                {
                    throw new InvalidOperationException("The line-list allocation returned fewer vertices than requested.");
                }

                intervalBuilder = new Direct3D9ComplexScanIntervalBuilder(
                    _needInsideGeometry,
                    _outsideBounds,
                    (start, end) =>
                    {
                        Span<Direct3D9ExpandedVertex> span = vertices.Span;
                        int conversionResult = _expandedVertexConverter.TransferAndExpandComplexScanVertex(
                            new Direct3D9ComplexScanVertex(start.X, start.Y, start.Alpha),
                            ref span[vertexIndex++],
                            transformPosition: false);
                        return conversionResult < 0
                            ? conversionResult
                            : _expandedVertexConverter.TransferAndExpandComplexScanVertex(
                                new Direct3D9ComplexScanVertex(end.X, end.Y, end.Alpha),
                                ref span[vertexIndex++],
                                transformPosition: false);
                    });
            }
            else
            {
                result = _allocateLineListVertices(segmentCount * 2, out Memory<Direct3D9ComplexScanVertex> vertices);
                if (result < 0)
                {
                    return result;
                }

                if (vertices.Length < segmentCount * 2)
                {
                    throw new InvalidOperationException("The line-list allocation returned fewer vertices than requested.");
                }

                intervalBuilder = new Direct3D9ComplexScanIntervalBuilder(
                    _needInsideGeometry,
                    _outsideBounds,
                    (start, end) =>
                    {
                        Span<Direct3D9ComplexScanVertex> span = vertices.Span;
                        span[vertexIndex++] = new Direct3D9ComplexScanVertex(start.X, start.Y, start.Alpha);
                        span[vertexIndex++] = new Direct3D9ComplexScanVertex(end.X, end.Y, end.Alpha);
                        return 0;
                    });
            }
        }
        else
        {
            intervalBuilder = new Direct3D9ComplexScanIntervalBuilder(
                _needInsideGeometry,
                _outsideBounds,
                _lineBuilder.AddLine);
        }

        return intervalBuilder.AddScan(pixelY, intervals);
    }

    private static int AddDirectLineNotAllocated(Direct3D9WafflePoint start, Direct3D9WafflePoint end)
    {
        throw new InvalidOperationException("Direct line-list output requires a batch allocation.");
    }

    private static int AllocateBasicVerticesNotSupported(
        int vertexCount,
        out Memory<Direct3D9ComplexScanVertex> vertices)
    {
        vertices = default;
        return Direct3D9Factory.NotImplementedHResult;
    }

    private int AddLineAsTriangleStrip(Direct3D9WafflePoint start, Direct3D9WafflePoint end)
    {
        if (_expandedVertexConverter is not null)
        {
            int expandedResult = _allocateExpandedTriangleStripVertices!(6, out Memory<Direct3D9ExpandedVertex> expandedVertices);
            if (expandedResult < 0)
            {
                return expandedResult;
            }

            if (expandedVertices.Length < 6)
            {
                throw new InvalidOperationException("The triangle-strip allocation returned fewer vertices than requested.");
            }

            return WriteExpandedTriangleStrip(start, end, expandedVertices.Span);
        }

        int result = _allocateTriangleStripVertices(6, out Memory<Direct3D9ComplexScanVertex> vertices);
        if (result < 0)
        {
            return result;
        }

        if (vertices.Length < 6)
        {
            throw new InvalidOperationException("The triangle-strip allocation returned fewer vertices than requested.");
        }

        float x0 = start.X - 0.5f;
        float x1 = end.X - 0.5f;
        float top = start.Y - 0.5f;
        float bottom = start.Y + 0.5f;
        Span<Direct3D9ComplexScanVertex> span = vertices.Span;
        span[0] = new Direct3D9ComplexScanVertex(x0, top, start.Alpha);
        span[1] = new Direct3D9ComplexScanVertex(x0, top, start.Alpha);
        span[2] = new Direct3D9ComplexScanVertex(x0, bottom, start.Alpha);
        span[3] = new Direct3D9ComplexScanVertex(x1, top, start.Alpha);
        span[4] = new Direct3D9ComplexScanVertex(x1, bottom, start.Alpha);
        span[5] = new Direct3D9ComplexScanVertex(x1, bottom, start.Alpha);
        return 0;
    }

    private int WriteExpandedTriangleStrip(
        Direct3D9WafflePoint start,
        Direct3D9WafflePoint end,
        Span<Direct3D9ExpandedVertex> vertices)
    {
        float x0 = start.X - 0.5f;
        float x1 = end.X - 0.5f;
        float top = start.Y - 0.5f;
        float bottom = start.Y + 0.5f;
        Span<Direct3D9ComplexScanVertex> source = stackalloc Direct3D9ComplexScanVertex[6]
        {
            new(x0, top, start.Alpha),
            new(x0, top, start.Alpha),
            new(x0, bottom, start.Alpha),
            new(x1, top, start.Alpha),
            new(x1, bottom, start.Alpha),
            new(x1, bottom, start.Alpha),
        };
        return _expandedVertexConverter!.TransferAndExpandComplexScanVertices(source, vertices, transformPosition: false);
    }
}
