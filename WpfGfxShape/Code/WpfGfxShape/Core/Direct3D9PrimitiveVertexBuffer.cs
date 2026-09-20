namespace WpfGfxShape.Core;

internal sealed class Direct3D9PrimitiveVertexBuffer
{
    private readonly Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv2> _buffer = new();

    internal int VertexCount => _buffer.VertexCount;

    internal ReadOnlySpan<Direct3D9VertexXyzDiffuseUv2> Vertices => _buffer.Vertices;

    internal void Clear() => _buffer.Clear();

    internal int GetNewVertices(out Span<Direct3D9VertexXyzDiffuseUv2> vertices) => _buffer.GetNewVertices(out vertices);
}

internal sealed class Direct3D9PrimitiveVertexBufferDuv6
{
    private readonly Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzDiffuseUv6> _buffer = new();

    internal int VertexCount => _buffer.VertexCount;

    internal ReadOnlySpan<Direct3D9VertexXyzDiffuseUv6> Vertices => _buffer.Vertices;

    internal void Clear() => _buffer.Clear();

    internal int GetNewVertices(out Span<Direct3D9VertexXyzDiffuseUv6> vertices) => _buffer.GetNewVertices(out vertices);
}

internal sealed class Direct3D9PrimitiveVertexBufferXyzNormalDiffuseSpecularUv4
{
    private readonly Direct3D9PrimitiveVertexBuffer<Direct3D9VertexXyzNormalDiffuseSpecularUv4> _buffer = new();

    internal int VertexCount => _buffer.VertexCount;

    internal ReadOnlySpan<Direct3D9VertexXyzNormalDiffuseSpecularUv4> Vertices => _buffer.Vertices;

    internal void Clear() => _buffer.Clear();

    internal int GetNewVertices(out Span<Direct3D9VertexXyzNormalDiffuseSpecularUv4> vertices) => _buffer.GetNewVertices(out vertices);
}

internal sealed class Direct3D9PrimitiveVertexBuffer<TVertex>
    where TVertex : unmanaged
{
    private TVertex[] _vertices = [];
    private int _vertexCount;

    internal int VertexCount => _vertexCount;

    internal ReadOnlySpan<TVertex> Vertices => _vertices.AsSpan(0, _vertexCount);

    internal void Clear()
    {
        _vertexCount = 0;
    }

    internal int GetNewVertices(out Span<TVertex> vertices)
    {
        const int newVertexCount = 4;
        int requiredVertexCount = _vertexCount + newVertexCount;
        if (newVertexCount >= _vertices.Length - _vertexCount)
        {
            int result = Grow(newVertexCount);
            if (result < 0)
            {
                vertices = default;
                return result;
            }
        }

        vertices = _vertices.AsSpan(_vertexCount, newVertexCount);
        _vertexCount = requiredVertexCount;
        return 0;
    }

    private int Grow(int growth)
    {
        int newCapacity = _vertices.Length;
        do
        {
            newCapacity = newCapacity == 0 ? 4 : newCapacity * 2;
        }
        while (_vertices.Length + growth >= newCapacity);

        if (newCapacity >= ushort.MaxValue)
        {
            return Direct3D9Factory.InvalidArgumentHResult;
        }

        try
        {
            Array.Resize(ref _vertices, newCapacity);
            return 0;
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }
    }
}
