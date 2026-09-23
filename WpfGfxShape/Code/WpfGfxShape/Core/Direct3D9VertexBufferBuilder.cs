namespace WpfGfxShape.Core;

internal enum Direct3D9BuilderPrimitiveKind
{
    IndexedTriangleList,
    TriangleList,
    TriangleStrip,
    LineList,
}

internal readonly record struct Direct3D9BuilderDrawBatch(
    Direct3D9BuilderPrimitiveKind Kind,
    ReadOnlyMemory<Direct3D9ExpandedVertex> Vertices,
    ReadOnlyMemory<ushort> Indices);

internal delegate int Direct3D9DrawBuilderBatch(Direct3D9BuilderDrawBatch batch);
internal delegate int Direct3D9PackBuilderVertices(Direct3D9BuilderPrimitiveKind kind, Memory<Direct3D9ExpandedVertex> vertices, int groupSize);

internal sealed class Direct3D9VertexBufferBuilder
{
    private readonly Direct3D9ExpandedVertexConverter _converter;
    private readonly Func<int>? _realizePipeline;
    private readonly Direct3D9DrawBuilderBatch _drawBatch;
    private readonly Direct3D9PackBuilderVertices? _packVertices;
    private readonly List<Direct3D9ExpandedVertex> _indexedTriangleVertices = [];
    private readonly List<ushort> _indices = [];
    private readonly List<Direct3D9ExpandedVertex> _triangleListVertices = [];
    private readonly List<Direct3D9ExpandedVertex> _triangleStripVertices = [];
    private readonly List<Direct3D9ExpandedVertex> _lineListVertices = [];
    private readonly List<Direct3D9ExpandedVertex[]> _pendingTriangleStripVertices = [];
    private readonly List<Direct3D9ExpandedVertex[]> _pendingLineListVertices = [];
    private readonly Direct3D9WaffleTextureCoordinate?[] _waffleTextureCoordinates = new Direct3D9WaffleTextureCoordinate?[8];
    private Direct3D9ExpandedVertex[]? _precomputedVertices;
    private ushort[]? _precomputedIndices;
    private Direct3D9SurfaceRect? _outsideBounds;
    private bool _needInsideGeometry = true;
    private float _currentStratumTop = float.MaxValue;
    private float _currentStratumBottom = float.MinValue;
    private float _lastTrapezoidRight = float.MinValue;
    private bool _pipelineRealized;
    private bool _ended;
    private bool _hasFlushed;

    internal Direct3D9VertexBufferBuilder(
        Direct3D9ExpandedVertexConverter converter,
        Direct3D9DrawBuilderBatch drawBatch,
        Func<int>? realizePipeline = null,
        Direct3D9PackBuilderVertices? packVertices = null)
    {
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(drawBatch);
        _converter = converter;
        _drawBatch = drawBatch;
        _realizePipeline = realizePipeline;
        _packVertices = packVertices;
    }

    internal bool HasOutsideBounds => _outsideBounds.HasValue;
    internal bool HasFlushed => _hasFlushed;

    internal int SetTextureMapping(uint destinationCoordinateIndex, uint sourceCoordinateIndex, System.Numerics.Matrix3x2 pointToTexture) =>
        _converter.SetTextureMapping(destinationCoordinateIndex, sourceCoordinateIndex, pointToTexture);

    internal int SetWaffling(
        uint coordinateIndex,
        System.Numerics.Matrix3x2 pointToTexture,
        Direct3D9WaffleTextureSubrect subrect,
        Direct3D9WaffleMode mode)
    {
        if (coordinateIndex >= _waffleTextureCoordinates.Length || mode == Direct3D9WaffleMode.None)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        _waffleTextureCoordinates[coordinateIndex] = new Direct3D9WaffleTextureCoordinate(pointToTexture, mode, subrect);
        return Direct3D9Factory.SuccessHResult;
    }

    internal int SetConstantMapping(Direct3D9VertexFormatAttribute location, uint premultipliedSrgbColor) =>
        location is Direct3D9VertexFormatAttribute.Diffuse or Direct3D9VertexFormatAttribute.Specular
            ? _converter.SetConstantDiffuseMapping(premultipliedSrgbColor)
            : Direct3D9Factory.UnsupportedOperationHResult;

    internal int FinalizeMappings() => _converter.FinalizeMappings();
    internal bool IsEmpty => _indices.Count == 0 && _triangleListVertices.Count == 0 &&
                             _triangleStripVertices.Count == 0 && _lineListVertices.Count == 0 &&
                             _pendingTriangleStripVertices.Count == 0 && _pendingLineListVertices.Count == 0 &&
                             _precomputedVertices is null;

    internal void SetOutsideBounds(Direct3D9SurfaceRect? bounds, bool needInsideGeometry)
    {
        if (!needInsideGeometry && bounds is null)
        {
            throw new ArgumentException("Inside geometry can only be omitted when outside bounds are provided.", nameof(needInsideGeometry));
        }

        _outsideBounds = bounds;
        _needInsideGeometry = needInsideGeometry;
    }

    internal void BeginBuilding()
    {
        ResetGeometry();
        _hasFlushed = false;
        _pipelineRealized = false;
    }

    internal int PrepareStratum(float top, float bottom, bool trapezoid = false, float trapezoidLeft = 0, float trapezoidRight = 0)
    {
        if (!_outsideBounds.HasValue)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        if (top > bottom)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        Direct3D9SurfaceRect bounds = _outsideBounds.Value;
        bool endingOutside = top == bounds.Bottom && bottom == bounds.Bottom;
        if (!endingOutside && bottom < _currentStratumBottom)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        if (endingOutside || bottom != _currentStratumBottom)
        {
            if (_currentStratumTop != float.MaxValue)
            {
                float right = Math.Max(bounds.Right, _lastTrapezoidRight);
                AddTriangleStripVertices(
                    Vertex(right, _currentStratumTop, 0),
                    Vertex(right, _currentStratumBottom, 0),
                    Vertex(right, _currentStratumBottom, 0));
            }

            float gap = top - _currentStratumBottom;
            if (gap > 0)
            {
                float rectangleTop = _currentStratumBottom == float.MinValue ? bounds.Top : _currentStratumBottom;
                AddTriangleStripVertices(
                    Vertex(bounds.Left, rectangleTop, 0),
                    Vertex(bounds.Left, rectangleTop, 0),
                    Vertex(bounds.Left, top, 0),
                    Vertex(bounds.Right, rectangleTop, 0),
                    Vertex(bounds.Right, top, 0),
                    Vertex(bounds.Right, top, 0));
            }

            if (trapezoid)
            {
                float left = Math.Min(bounds.Left, trapezoidLeft);
                AddTriangleStripVertices(
                    Vertex(left, top, 0),
                    Vertex(left, top, 0),
                    Vertex(left, bottom, 0));
            }
        }

        if (trapezoid)
        {
            _lastTrapezoidRight = trapezoidRight;
        }

        _currentStratumTop = trapezoid ? top : float.MaxValue;
        _currentStratumBottom = bottom;
        _ended = false;
        return Direct3D9Factory.SuccessHResult;
    }

    internal void AddIndexedTriangleVertices(ReadOnlySpan<Direct3D9ExpandedVertex> vertices, ReadOnlySpan<ushort> indices)
    {
        EnsureMutable();
        if (_outsideBounds.HasValue)
        {
            throw new InvalidOperationException("Indexed triangle lists are not supported with outside geometry.");
        }

        int baseVertex = _indexedTriangleVertices.Count;
        if (baseVertex + vertices.Length > short.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(vertices));
        }

        _indexedTriangleVertices.AddRange(vertices);
        foreach (ushort index in indices)
        {
            if (index >= vertices.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(indices));
            }

            _indices.Add(checked((ushort) (baseVertex + index)));
        }
    }

    internal void AddTriangleListVertices(params ReadOnlySpan<Direct3D9ExpandedVertex> vertices)
    {
        EnsureMutable();
        if (!_needInsideGeometry)
        {
            return;
        }

        Direct3D9WaffleTextureCoordinate[] textureCoordinates = GetWaffleTextureCoordinates();
        if (textureCoordinates.Length == 0)
        {
            _triangleListVertices.AddRange(vertices);
            return;
        }

        if (vertices.Length % 3 != 0)
        {
            throw new ArgumentException("Triangle lists must contain complete triangles.", nameof(vertices));
        }

        Direct3D9TriangleWafflePipeline pipeline = Direct3D9WafflePipelineBuilder.BuildTriangle(
            textureCoordinates,
            (first, second, third) =>
            {
                _triangleListVertices.Add(Vertex(first));
                _triangleListVertices.Add(Vertex(second));
                _triangleListVertices.Add(Vertex(third));
                return Direct3D9Factory.SuccessHResult;
            });
        for (int index = 0; index < vertices.Length; index += 3)
        {
            int result = pipeline.Sink(Point(vertices[index]), Point(vertices[index + 1]), Point(vertices[index + 2]));
            if (result < 0)
            {
                throw new InvalidOperationException("Triangle waffling failed.");
            }
        }
    }

    internal void AddTriangleStripVertices(params ReadOnlySpan<Direct3D9ExpandedVertex> vertices)
    {
        EnsureMutable();
        _triangleStripVertices.AddRange(vertices);
    }

    internal void AddLineListVertices(params ReadOnlySpan<Direct3D9ExpandedVertex> vertices)
    {
        EnsureMutable();
        if (!_needInsideGeometry)
        {
            return;
        }

        Direct3D9WaffleTextureCoordinate[] textureCoordinates = GetWaffleTextureCoordinates();
        if (textureCoordinates.Length == 0)
        {
            _lineListVertices.AddRange(vertices);
            return;
        }

        if (vertices.Length % 2 != 0)
        {
            throw new ArgumentException("Line lists must contain complete lines.", nameof(vertices));
        }

        Direct3D9LineWafflePipeline pipeline = Direct3D9WafflePipelineBuilder.BuildLine(
            textureCoordinates,
            (start, end) =>
            {
                _lineListVertices.Add(Vertex(start));
                _lineListVertices.Add(Vertex(end));
                return Direct3D9Factory.SuccessHResult;
            });
        for (int index = 0; index < vertices.Length; index += 2)
        {
            int result = pipeline.Sink(Point(vertices[index]), Point(vertices[index + 1]));
            if (result < 0)
            {
                throw new InvalidOperationException("Line waffling failed.");
            }
        }
    }

    internal int AllocateTriangleStripVertices(int vertexCount, out Memory<Direct3D9ExpandedVertex> vertices) =>
        AllocateVertices(vertexCount, _pendingTriangleStripVertices, out vertices);

    internal int AllocateLineListVertices(int vertexCount, out Memory<Direct3D9ExpandedVertex> vertices) =>
        AllocateVertices(vertexCount, _pendingLineListVertices, out vertices);

    internal int AddComplexScan(
        int pixelY,
        IReadOnlyList<Direct3D9CoverageInterval> intervals,
        float viewportTop,
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates)
    {
        Direct3D9ComplexScanBuilder complexScanBuilder = new(
            viewportTop,
            _needInsideGeometry,
            _outsideBounds,
            textureCoordinates,
            (top, bottom) => PrepareStratum(top, bottom),
            _converter,
            AllocateLineListVertices,
            AllocateTriangleStripVertices);
        return complexScanBuilder.AddComplexScan(pixelY, intervals);
    }

    internal void SetPrecomputedIndexedTriangles(ReadOnlySpan<Direct3D9ExpandedVertex> vertices, ReadOnlySpan<uint> indices)
    {
        EnsureMutable();
        if (!IsEmpty || vertices.IsEmpty || indices.IsEmpty || vertices.Length > short.MaxValue)
        {
            throw new InvalidOperationException("Precomputed indexed geometry must be the only non-empty geometry source.");
        }

        _precomputedVertices = vertices.ToArray();
        _precomputedIndices = new ushort[indices.Length];
        for (int index = 0; index < indices.Length; index++)
        {
            if (indices[index] >= vertices.Length || indices[index] > ushort.MaxValue)
            {
                _precomputedVertices = null;
                _precomputedIndices = null;
                throw new ArgumentOutOfRangeException(nameof(indices));
            }

            _precomputedIndices[index] = (ushort) indices[index];
        }
    }

    internal int EndBuilding()
    {
        if (_ended)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        CommitPendingVertices(_pendingTriangleStripVertices, _triangleStripVertices);
        CommitPendingVertices(_pendingLineListVertices, _lineListVertices);

        if (_outsideBounds.HasValue)
        {
            int outsideResult = PrepareStratum(_outsideBounds.Value.Bottom, _outsideBounds.Value.Bottom);
            if (outsideResult < 0)
            {
                return outsideResult;
            }
        }

        int result = Expand(_indexedTriangleVertices);
        if (result >= 0) result = Expand(_triangleListVertices);
        if (result >= 0) result = Expand(_triangleStripVertices);
        if (result >= 0) result = Expand(_lineListVertices);
        if (result < 0)
        {
            return result;
        }

        result = PackWaffleCoordinates(_triangleListVertices, 3);
        if (result >= 0) result = PackWaffleCoordinates(_triangleStripVertices, 6);
        if (result >= 0) result = PackWaffleCoordinates(_lineListVertices, 2);
        if (result >= 0) result = Pack(Direct3D9BuilderPrimitiveKind.TriangleList, _triangleListVertices, 3);
        if (result >= 0) result = Pack(Direct3D9BuilderPrimitiveKind.TriangleStrip, _triangleStripVertices, 6);
        if (result >= 0) result = Pack(Direct3D9BuilderPrimitiveKind.LineList, _lineListVertices, 2);
        if (result < 0)
        {
            return result;
        }

        _ended = true;
        return Direct3D9Factory.SuccessHResult;
    }

    internal int FlushReset() => Flush(returnVertexBuffer: false, out _);

    internal int FlushTryGetVertexBuffer(out Direct3D9VertexBufferBuilder? vertexBuffer) =>
        Flush(returnVertexBuffer: true, out vertexBuffer);

    internal int DrawCached()
    {
        if (!_ended)
        {
            return Direct3D9Factory.InvalidCallHResult;
        }

        return Draw();
    }

    private int Flush(bool returnVertexBuffer, out Direct3D9VertexBufferBuilder? vertexBuffer)
    {
        vertexBuffer = null;
        int result = Direct3D9Factory.SuccessHResult;
        if (!_pipelineRealized && _realizePipeline is not null)
        {
            result = _realizePipeline();
            if (result >= 0)
            {
                _pipelineRealized = true;
            }
        }

        if (result >= 0)
        {
            result = EndBuilding();
        }

        if (result >= 0)
        {
            result = Draw();
        }

        if (returnVertexBuffer)
        {
            if (!_hasFlushed)
            {
                vertexBuffer = this;
            }
        }
        else
        {
            _hasFlushed = true;
            ResetGeometry();
        }

        return result;
    }

    private int Draw()
    {
        if (_precomputedVertices is not null)
        {
            Direct3D9ExpandedVertex[] converted = new Direct3D9ExpandedVertex[_precomputedVertices.Length];
            int result = _converter.TransferAndExpandVerticesGeneral(_precomputedVertices, converted, transformPosition: true);
            return result < 0
                ? result
                : _drawBatch(new Direct3D9BuilderDrawBatch(Direct3D9BuilderPrimitiveKind.IndexedTriangleList, converted, _precomputedIndices!));
        }

        int drawResult = Draw(Direct3D9BuilderPrimitiveKind.IndexedTriangleList, _indexedTriangleVertices, _indices);
        if (drawResult >= 0) drawResult = Draw(Direct3D9BuilderPrimitiveKind.TriangleList, _triangleListVertices, []);
        if (drawResult >= 0) drawResult = Draw(Direct3D9BuilderPrimitiveKind.TriangleStrip, _triangleStripVertices, []);
        if (drawResult >= 0) drawResult = Draw(Direct3D9BuilderPrimitiveKind.LineList, _lineListVertices, []);
        return drawResult;
    }

    private int Draw(Direct3D9BuilderPrimitiveKind kind, List<Direct3D9ExpandedVertex> vertices, List<ushort> indices)
    {
        if (vertices.Count == 0 || kind == Direct3D9BuilderPrimitiveKind.IndexedTriangleList && indices.Count == 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        return _drawBatch(new Direct3D9BuilderDrawBatch(kind, vertices.ToArray(), indices.ToArray()));
    }

    private int Expand(List<Direct3D9ExpandedVertex> vertices)
    {
        if (vertices.Count == 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        Direct3D9ExpandedVertex[] expanded = vertices.ToArray();
        int result = _converter.ExpandVertices(expanded);
        if (result >= 0)
        {
            vertices.Clear();
            vertices.AddRange(expanded);
        }

        return result;
    }

    private int PackWaffleCoordinates(List<Direct3D9ExpandedVertex> vertices, int groupSize)
    {
        if (vertices.Count == 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        if (vertices.Count % groupSize != 0)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        for (int coordinateIndex = 0; coordinateIndex < _waffleTextureCoordinates.Length; coordinateIndex++)
        {
            Direct3D9WaffleTextureCoordinate? coordinate = _waffleTextureCoordinates[coordinateIndex];
            if (coordinate is null)
            {
                continue;
            }

            Direct3D9WaffleTextureCoordinate current = coordinate.Value;
            bool flipX = (current.Mode & Direct3D9WaffleMode.FlipX) != 0;
            bool flipY = (current.Mode & Direct3D9WaffleMode.FlipY) != 0;
            for (int groupStart = 0; groupStart < vertices.Count; groupStart += groupSize)
            {
                float x = 0;
                float y = 0;
                for (int index = 0; index < groupSize; index++)
                {
                    System.Numerics.Vector2 uv = vertices[groupStart + index].GetTextureCoordinate(coordinateIndex);
                    x += uv.X;
                    y += uv.Y;
                }

                int tileX = SaturatingFloor(x / groupSize);
                int tileY = SaturatingFloor(y / groupSize);
                bool flipThisX = flipX && tileX % 2 != 0;
                bool flipThisY = flipY && tileY % 2 != 0;
                for (int index = 0; index < groupSize; index++)
                {
                    int vertexIndex = groupStart + index;
                    Direct3D9ExpandedVertex vertex = vertices[vertexIndex];
                    System.Numerics.Vector2 uv = vertex.GetTextureCoordinate(coordinateIndex);
                    float packedX = uv.X - tileX;
                    float packedY = uv.Y - tileY;
                    if (flipThisX)
                    {
                        packedX = 1 - packedX;
                    }

                    if (flipThisY)
                    {
                        packedY = 1 - packedY;
                    }

                    vertex.SetTextureCoordinate(
                        coordinateIndex,
                        new System.Numerics.Vector2(
                            current.Subrect.X + packedX * current.Subrect.Width,
                            current.Subrect.Y + packedY * current.Subrect.Height));
                    vertices[vertexIndex] = vertex;
                }
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private int Pack(Direct3D9BuilderPrimitiveKind kind, List<Direct3D9ExpandedVertex> vertices, int groupSize)
    {
        if (_packVertices is null || vertices.Count == 0)
        {
            return Direct3D9Factory.SuccessHResult;
        }

        if (vertices.Count % groupSize != 0)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        Direct3D9ExpandedVertex[] packed = vertices.ToArray();
        int result = _packVertices(kind, packed, groupSize);
        if (result >= 0)
        {
            vertices.Clear();
            vertices.AddRange(packed);
        }

        return result;
    }

    private int AllocateVertices(
        int vertexCount,
        List<Direct3D9ExpandedVertex[]> pendingVertices,
        out Memory<Direct3D9ExpandedVertex> vertices)
    {
        EnsureMutable();
        if (vertexCount < 0)
        {
            vertices = default;
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        Direct3D9ExpandedVertex[] allocation = new Direct3D9ExpandedVertex[vertexCount];
        pendingVertices.Add(allocation);
        vertices = allocation;
        return Direct3D9Factory.SuccessHResult;
    }

    private static void CommitPendingVertices(
        List<Direct3D9ExpandedVertex[]> pendingVertices,
        List<Direct3D9ExpandedVertex> destination)
    {
        foreach (Direct3D9ExpandedVertex[] vertices in pendingVertices)
        {
            destination.AddRange(vertices);
        }

        pendingVertices.Clear();
    }

    private void ResetGeometry()
    {
        _indexedTriangleVertices.Clear();
        _indices.Clear();
        _triangleListVertices.Clear();
        _triangleStripVertices.Clear();
        _lineListVertices.Clear();
        _pendingTriangleStripVertices.Clear();
        _pendingLineListVertices.Clear();
        _precomputedVertices = null;
        _precomputedIndices = null;
        _currentStratumTop = float.MaxValue;
        _currentStratumBottom = float.MinValue;
        _lastTrapezoidRight = float.MinValue;
        _ended = false;
    }

    private void EnsureMutable()
    {
        if (_ended)
        {
            throw new InvalidOperationException("The current build has already ended.");
        }

        if (_precomputedVertices is not null)
        {
            throw new InvalidOperationException("Precomputed indexed geometry cannot be combined with accumulated geometry.");
        }
    }

    private Direct3D9WaffleTextureCoordinate[] GetWaffleTextureCoordinates() =>
        _waffleTextureCoordinates.Where(static coordinate => coordinate.HasValue).Select(static coordinate => coordinate!.Value).ToArray();

    private static int SaturatingFloor(float value)
    {
        if (value >= int.MaxValue)
        {
            return int.MaxValue;
        }

        if (value <= int.MinValue)
        {
            return int.MinValue;
        }

        return (int) MathF.Floor(value);
    }

    private static Direct3D9WafflePoint Point(Direct3D9ExpandedVertex vertex) =>
        new(vertex.X, vertex.Y, BitConverter.UInt32BitsToSingle(vertex.Diffuse));

    private static Direct3D9ExpandedVertex Vertex(Direct3D9WafflePoint point) => Vertex(point.X, point.Y, point.Alpha);

    private static Direct3D9ExpandedVertex Vertex(float x, float y, float alpha) => new()
    {
        X = x,
        Y = y,
        Diffuse = BitConverter.SingleToUInt32Bits(alpha),
    };
}
