using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal struct Direct3D9BufferSpaceLocator
{
    private readonly uint _bufferByteCapacity;
    private uint _currentByteInBuffer;
    private uint _numberOfBytesInLatestChunk;
    private uint _numberOfBytesPerElementInLatestChunk;

    internal Direct3D9BufferSpaceLocator(uint bufferByteCapacity)
    {
        _bufferByteCapacity = bufferByteCapacity;
    }

    internal uint CurrentBytePosition => _currentByteInBuffer;

    internal uint NumberOfBytesInLatestChunk => _numberOfBytesInLatestChunk;

    internal uint GetMaximumCapacity(uint elementSize)
    {
        return _bufferByteCapacity / elementSize;
    }

    internal uint GetNextUsableNumberOfElements(uint elementSize)
    {
        uint remaining = (_bufferByteCapacity - _currentByteInBuffer - _numberOfBytesInLatestChunk) / elementSize;
        if (remaining < 3)
        {
            remaining = _bufferByteCapacity / elementSize;
        }

        return remaining - remaining % 3;
    }

    internal void AdvanceToNextChunk(
        uint elementsRequired,
        uint elementSize,
        out uint lockFlags,
        out uint startElement)
    {
        _currentByteInBuffer += _numberOfBytesInLatestChunk;
        if (_currentByteInBuffer % elementSize != 0)
        {
            _currentByteInBuffer += elementSize - _currentByteInBuffer % elementSize;
        }

        _numberOfBytesInLatestChunk = elementsRequired * elementSize;
        _numberOfBytesPerElementInLatestChunk = elementSize;

        if (_numberOfBytesInLatestChunk + _currentByteInBuffer <= _bufferByteCapacity)
        {
            lockFlags = D3D9.LockNooverwrite;
        }
        else
        {
            lockFlags = D3D9.LockDiscard;
            _currentByteInBuffer = 0;
        }

        startElement = _currentByteInBuffer / _numberOfBytesPerElementInLatestChunk;
    }

    internal void ReportNumberOfElementsUsedInLastChunk(uint elementsUsed)
    {
        _numberOfBytesInLatestChunk = _numberOfBytesPerElementInLatestChunk * elementsUsed;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9VertexBuffer : IDisposable
{
    private IDirect3DVertexBuffer9* _vertexBuffer;

    internal Direct3D9VertexBuffer(IDirect3DVertexBuffer9* vertexBuffer)
    {
        _vertexBuffer = vertexBuffer;
    }

    internal IDirect3DVertexBuffer9* VertexBuffer
    {
        get
        {
            ObjectDisposedException.ThrowIf(_vertexBuffer is null, this);
            return _vertexBuffer;
        }
    }

    internal int Lock(uint offsetToLock, uint sizeToLock, out void* data, uint flags)
    {
        ObjectDisposedException.ThrowIf(_vertexBuffer is null, this);

        data = null;
        void** vtable = _vertexBuffer->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DVertexBuffer9*, uint, uint, void**, uint, int> lockBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DVertexBuffer9*, uint, uint, void**, uint, int>) vtable[11];
        fixed (void** dataPointer = &data)
        {
            return lockBuffer(_vertexBuffer, offsetToLock, sizeToLock, dataPointer, flags);
        }
    }

    internal int Unlock()
    {
        ObjectDisposedException.ThrowIf(_vertexBuffer is null, this);

        void** vtable = _vertexBuffer->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DVertexBuffer9*, int> unlockBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DVertexBuffer9*, int>) vtable[12];
        return unlockBuffer(_vertexBuffer);
    }

    public void Dispose()
    {
        Direct3D9Factory.Release(_vertexBuffer);
        _vertexBuffer = null;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9IndexBuffer : IDisposable
{
    private IDirect3DIndexBuffer9* _indexBuffer;

    internal Direct3D9IndexBuffer(IDirect3DIndexBuffer9* indexBuffer)
    {
        _indexBuffer = indexBuffer;
    }

    internal IDirect3DIndexBuffer9* IndexBuffer
    {
        get
        {
            ObjectDisposedException.ThrowIf(_indexBuffer is null, this);
            return _indexBuffer;
        }
    }

    internal int Lock(uint offsetToLock, uint sizeToLock, out void* data, uint flags)
    {
        ObjectDisposedException.ThrowIf(_indexBuffer is null, this);

        data = null;
        void** vtable = _indexBuffer->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DIndexBuffer9*, uint, uint, void**, uint, int> lockBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DIndexBuffer9*, uint, uint, void**, uint, int>) vtable[11];
        fixed (void** dataPointer = &data)
        {
            return lockBuffer(_indexBuffer, offsetToLock, sizeToLock, dataPointer, flags);
        }
    }

    internal int Unlock()
    {
        ObjectDisposedException.ThrowIf(_indexBuffer is null, this);

        void** vtable = _indexBuffer->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DIndexBuffer9*, int> unlockBuffer =
            (delegate* unmanaged[Stdcall]<IDirect3DIndexBuffer9*, int>) vtable[12];
        return unlockBuffer(_indexBuffer);
    }

    public void Dispose()
    {
        Direct3D9Factory.Release(_indexBuffer);
        _indexBuffer = null;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9HardwareVertexBuffer : Direct3D9Resource
{
    private Direct3D9BufferSpaceLocator _spaceLocator;
    private Direct3D9VertexBuffer? _vertexBuffer;
    private bool _isLocked;

    private Direct3D9HardwareVertexBuffer(
        Direct3D9ResourceManager resourceManager,
        Direct3D9VertexBuffer vertexBuffer,
        uint capacity)
        : base(resourceManager)
    {
        _vertexBuffer = vertexBuffer;
        _spaceLocator = new Direct3D9BufferSpaceLocator(capacity);
    }

    internal bool IsLocked
    {
        get
        {
            ObjectDisposedException.ThrowIf(!IsValid || _vertexBuffer is null, this);
            return _isLocked;
        }
    }

    internal IDirect3DVertexBuffer9* DangerousGetDirect3DVertexBuffer()
    {
        ObjectDisposedException.ThrowIf(!IsValid || _vertexBuffer is null, this);
        return _vertexBuffer.VertexBuffer;
    }

    internal static int TryCreate(
        Direct3D9Device device,
        uint capacity,
        out Direct3D9HardwareVertexBuffer? vertexBuffer)
    {
        ArgumentNullException.ThrowIfNull(device);

        vertexBuffer = null;
        int result = device.TryCreateVertexBuffer(
            capacity,
            D3D9.UsageWriteonly | D3D9.UsageDynamic,
            0,
            Pool.Default,
            out Direct3D9VertexBuffer? direct3DVertexBuffer);
        if (result < 0)
        {
            return result;
        }

        try
        {
            vertexBuffer = new Direct3D9HardwareVertexBuffer(device.ResourceManager, direct3DVertexBuffer!, capacity);
        }
        catch (OutOfMemoryException)
        {
            direct3DVertexBuffer!.Dispose();
            throw;
        }

        return result;
    }

    internal uint GetMaximumCapacity(uint vertexStride)
    {
        ObjectDisposedException.ThrowIf(!IsValid, this);
        return _spaceLocator.GetMaximumCapacity(vertexStride);
    }

    internal uint GetNextUsableNumberOfVertices(uint vertexStride)
    {
        ObjectDisposedException.ThrowIf(!IsValid, this);
        return _spaceLocator.GetNextUsableNumberOfElements(vertexStride);
    }

    internal int Lock(uint vertexCount, uint vertexStride, out void* lockedVertices, out uint startVertex)
    {
        ObjectDisposedException.ThrowIf(!IsValid || _vertexBuffer is null, this);

        lockedVertices = null;
        startVertex = 0;
        if (vertexCount > _spaceLocator.GetMaximumCapacity(vertexStride))
        {
            return Direct3D9Factory.InsufficientBufferHResult;
        }

        _spaceLocator.AdvanceToNextChunk(vertexCount, vertexStride, out uint lockFlags, out startVertex);
        int result = _vertexBuffer.Lock(
            _spaceLocator.CurrentBytePosition,
            _spaceLocator.NumberOfBytesInLatestChunk,
            out lockedVertices,
            lockFlags);
        if (result < 0)
        {
            return result;
        }

        if ((nuint) lockedVertices == _spaceLocator.CurrentBytePosition || lockedVertices is null)
        {
            _ = _vertexBuffer.Unlock();
            lockedVertices = null;
            return Direct3D9Factory.DriverInternalErrorHResult;
        }

        _isLocked = true;
        return result;
    }

    internal int Unlock(uint verticesUsed)
    {
        ObjectDisposedException.ThrowIf(!IsValid || _vertexBuffer is null, this);

        int result = _vertexBuffer.Unlock();
        if (result < 0)
        {
            return result;
        }

        _spaceLocator.ReportNumberOfElementsUsedInLastChunk(verticesUsed);
        _isLocked = false;
        return result;
    }

    protected override void ReleaseD3DResources()
    {
        _vertexBuffer?.Dispose();
        _vertexBuffer = null;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9HardwareIndexBuffer : Direct3D9Resource
{
    private const uint IndexSize = sizeof(ushort);

    private Direct3D9BufferSpaceLocator _spaceLocator;
    private Direct3D9IndexBuffer? _indexBuffer;
    private bool _isLocked;

    private Direct3D9HardwareIndexBuffer(
        Direct3D9ResourceManager resourceManager,
        Direct3D9IndexBuffer indexBuffer,
        uint capacity)
        : base(resourceManager)
    {
        _indexBuffer = indexBuffer;
        _spaceLocator = new Direct3D9BufferSpaceLocator(capacity);
    }

    internal bool IsLocked
    {
        get
        {
            ObjectDisposedException.ThrowIf(!IsValid || _indexBuffer is null, this);
            return _isLocked;
        }
    }

    internal IDirect3DIndexBuffer9* DangerousGetDirect3DIndexBuffer()
    {
        ObjectDisposedException.ThrowIf(!IsValid || _indexBuffer is null, this);
        return _indexBuffer.IndexBuffer;
    }

    internal static int TryCreate(
        Direct3D9Device device,
        uint capacity,
        out Direct3D9HardwareIndexBuffer? indexBuffer)
    {
        ArgumentNullException.ThrowIfNull(device);

        indexBuffer = null;
        int result = device.TryCreateIndexBuffer(
            capacity,
            D3D9.UsageWriteonly | D3D9.UsageDynamic,
            Format.Index16,
            Pool.Default,
            out Direct3D9IndexBuffer? direct3DIndexBuffer);
        if (result < 0)
        {
            return result;
        }

        try
        {
            indexBuffer = new Direct3D9HardwareIndexBuffer(device.ResourceManager, direct3DIndexBuffer!, capacity);
        }
        catch (OutOfMemoryException)
        {
            direct3DIndexBuffer!.Dispose();
            throw;
        }

        return result;
    }

    internal uint GetMaximumCapacity()
    {
        ObjectDisposedException.ThrowIf(!IsValid, this);
        return _spaceLocator.GetMaximumCapacity(IndexSize);
    }

    internal uint GetNextUsableNumberOfIndices()
    {
        ObjectDisposedException.ThrowIf(!IsValid, this);
        return _spaceLocator.GetNextUsableNumberOfElements(IndexSize);
    }

    internal int Lock(uint indexCount, out ushort* lockedIndices, out uint startIndex)
    {
        ObjectDisposedException.ThrowIf(!IsValid || _indexBuffer is null, this);

        lockedIndices = null;
        startIndex = 0;
        if (indexCount > _spaceLocator.GetMaximumCapacity(IndexSize))
        {
            return Direct3D9Factory.InsufficientBufferHResult;
        }

        _spaceLocator.AdvanceToNextChunk(indexCount, IndexSize, out uint lockFlags, out startIndex);
        int result = _indexBuffer.Lock(
            _spaceLocator.CurrentBytePosition,
            _spaceLocator.NumberOfBytesInLatestChunk,
            out void* lockedData,
            lockFlags);
        if (result < 0)
        {
            return result;
        }

        if ((nuint) lockedData == _spaceLocator.CurrentBytePosition || lockedData is null)
        {
            _ = _indexBuffer.Unlock();
            return Direct3D9Factory.DriverInternalErrorHResult;
        }

        lockedIndices = (ushort*) lockedData;
        _isLocked = true;
        return result;
    }

    internal int CopyFromInputBuffer(ReadOnlySpan<uint> indexStream, out uint startIndex)
    {
        int result = Lock((uint) indexStream.Length, out ushort* lockedIndices, out startIndex);
        if (result >= 0)
        {
            for (int index = 0; index < indexStream.Length; index++)
            {
                lockedIndices[index] = (ushort) indexStream[index];
            }
        }

        if (_isLocked)
        {
            int unlockResult = Unlock();
            if (result >= 0)
            {
                result = unlockResult;
            }
        }

        return result;
    }

    internal int Unlock()
    {
        ObjectDisposedException.ThrowIf(!IsValid || _indexBuffer is null, this);

        int result = _indexBuffer.Unlock();
        if (result >= 0)
        {
            _isLocked = false;
        }

        return result;
    }

    protected override void ReleaseD3DResources()
    {
        _indexBuffer?.Dispose();
        _indexBuffer = null;
    }
}
