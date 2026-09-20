using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal delegate byte[]? Direct3D9AllocateSoftwareIntermediateBufferStorage(int byteLength);

internal sealed class Direct3D9SoftwareIntermediateBufferView
{
    private readonly Direct3D9SoftwareIntermediateBuffers _owner;
    private readonly int _bufferIndex;
    private readonly int _generation;

    internal Direct3D9SoftwareIntermediateBufferView(
        Direct3D9SoftwareIntermediateBuffers owner,
        int bufferIndex,
        int generation)
    {
        _owner = owner;
        _bufferIndex = bufferIndex;
        _generation = generation;
    }

    internal Span<MilColorF> Colors => _owner.GetBufferSpan(_bufferIndex, _generation);
}

internal sealed class Direct3D9SoftwareIntermediateBuffers : IDisposable
{
    private const int BufferCount = 3;
    private static readonly int ColorSize = Marshal.SizeOf<MilColorF>();

    private readonly Direct3D9AllocateSoftwareIntermediateBufferStorage _allocateStorage;
    private byte[]? _allocation;
    private int _individualBufferByteLength;
    private int _generation;
    private bool _hasAllocation;
    private bool _isDisposed;

    internal Direct3D9SoftwareIntermediateBuffers(
        Direct3D9AllocateSoftwareIntermediateBufferStorage? allocateStorage = null)
    {
        _allocateStorage = allocateStorage ?? (byteLength => GC.AllocateUninitializedArray<byte>(byteLength));
    }

    internal bool HasAllocation => _hasAllocation;

    internal int Generation => _generation;

    internal int AllocateBuffers(uint maximumWidth)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_hasAllocation)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        ulong individualBufferByteLength = (ulong) maximumWidth * (uint) ColorSize;
        ulong totalByteLength = individualBufferByteLength * BufferCount;
        if (individualBufferByteLength > int.MaxValue || totalByteLength > (ulong) Array.MaxLength)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        byte[]? allocation;
        try
        {
            allocation = _allocateStorage((int) totalByteLength);
        }
        catch (OutOfMemoryException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }
        catch (OverflowException)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        if (allocation is null || allocation.Length < (int) totalByteLength)
        {
            return Direct3D9Factory.OutOfMemoryHResult;
        }

        _allocation = allocation;
        _individualBufferByteLength = (int) individualBufferByteLength;
        _hasAllocation = true;
        _generation++;
        return Direct3D9Factory.SuccessHResult;
    }

    internal Direct3D9SoftwareIntermediateBufferView GetBuffer(int bufferIndex)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(bufferIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bufferIndex, BufferCount);
        if (!_hasAllocation)
        {
            throw new InvalidOperationException("Intermediate buffers have not been allocated.");
        }

        return new Direct3D9SoftwareIntermediateBufferView(this, bufferIndex, _generation);
    }

    internal void FreeBuffers()
    {
        if (!_hasAllocation)
        {
            return;
        }

        _allocation = null;
        _individualBufferByteLength = 0;
        _hasAllocation = false;
        _generation++;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        FreeBuffers();
        _isDisposed = true;
    }

    internal Span<MilColorF> GetBufferSpan(int bufferIndex, int generation)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_hasAllocation || generation != _generation || _allocation is null)
        {
            throw new InvalidOperationException("The intermediate buffer view is no longer valid.");
        }

        int offset = checked(bufferIndex * _individualBufferByteLength);
        return MemoryMarshal.Cast<byte, MilColorF>(
            _allocation.AsSpan(offset, _individualBufferByteLength));
    }
}
