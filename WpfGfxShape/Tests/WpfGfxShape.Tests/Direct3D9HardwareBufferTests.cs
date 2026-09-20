using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9HardwareBufferTests
{
    private static uint _createLength;
    private static uint _createUsage;
    private static uint _createFlexibleVertexFormat;
    private static Format _createFormat;
    private static Pool _createPool;
    private static nint _bufferToReturn;
    private static int _createResult;
    private static uint _lockOffset;
    private static uint _lockSize;
    private static uint _lockFlags;
    private static nint _lockDataToReturn;
    private static nint _vertexBufferIdentity;
    private static nint _vertexLockDataToReturn;
    private static nint _indexLockDataToReturn;
    private static int _lockResult;
    private static int _lockCallCount;
    private static int _unlockResult;
    private static int _unlockCallCount;
    private static int _releaseCount;
    private static nint _releasedVertexBufferIdentity;
    private static nint _releasedIndexBufferIdentity;
    private static int _vertexBufferReleaseCount;
    private static int _indexBufferReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _createLength = 0;
        _createUsage = 0;
        _createFlexibleVertexFormat = 0;
        _createFormat = 0;
        _createPool = 0;
        _bufferToReturn = 0;
        _createResult = 0;
        _lockOffset = 0;
        _lockSize = 0;
        _lockFlags = 0;
        _lockDataToReturn = 0;
        _vertexBufferIdentity = 0;
        _vertexLockDataToReturn = 0;
        _indexLockDataToReturn = 0;
        _lockResult = 0;
        _lockCallCount = 0;
        _unlockResult = 0;
        _unlockCallCount = 0;
        _releaseCount = 0;
        _releasedVertexBufferIdentity = 0;
        _releasedIndexBufferIdentity = 0;
        _vertexBufferReleaseCount = 0;
        _indexBufferReleaseCount = 0;
    }

    [TestMethod]
    public void WhenCreatingHardwareVertexBufferThenDynamicDefaultResourceIsRegistered()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = Direct3D9HardwareVertexBuffer.TryCreate(device, 600, out Direct3D9HardwareVertexBuffer? buffer);

        Assert.AreEqual(
            (0, 600u, (uint) (D3D9.UsageWriteonly | D3D9.UsageDynamic), 0u, Pool.Default, 1),
            (result, _createLength, _createUsage, _createFlexibleVertexFormat, _createPool, device.ResourceCount));
        buffer!.Dispose();
    }

    [TestMethod]
    public void WhenCreatingHardwareIndexBufferThenIndex16DynamicDefaultResourceIsRegistered()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = Direct3D9HardwareIndexBuffer.TryCreate(device, 300, out Direct3D9HardwareIndexBuffer? buffer);

        Assert.AreEqual(
            (0, 300u, (uint) (D3D9.UsageWriteonly | D3D9.UsageDynamic), Format.Index16, Pool.Default, 1),
            (result, _createLength, _createUsage, _createFormat, _createPool, device.ResourceCount));
        buffer!.Dispose();
    }

    [TestMethod]
    public void WhenHardwareVertexBufferCreationFailsWithPointerThenPointerIsReleasedAndResourceIsNotRegistered()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        _releasedVertexBufferIdentity = _bufferToReturn;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = Direct3D9HardwareVertexBuffer.TryCreate(device, 600, out Direct3D9HardwareVertexBuffer? buffer);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, 0, 1, 1),
            (result, buffer, device.ResourceCount, _releaseCount, _vertexBufferReleaseCount));
    }

    [TestMethod]
    public void WhenHardwareIndexBufferCreationFailsWithPointerThenPointerIsReleasedAndResourceIsNotRegistered()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        _releasedIndexBufferIdentity = _bufferToReturn;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = Direct3D9HardwareIndexBuffer.TryCreate(device, 300, out Direct3D9HardwareIndexBuffer? buffer);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, 0, 1, 1),
            (result, buffer, device.ResourceCount, _releaseCount, _indexBufferReleaseCount));
    }

    [TestMethod]
    public void WhenVertexChunksFitThenNoOverwriteUsesReportedVertexCountAndAlignment()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        _lockDataToReturn = (nint) 0x1000;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 128, out Direct3D9HardwareVertexBuffer? buffer);

        buffer!.Lock(3, 12, out _, out _);
        buffer.Unlock(2);
        int result = buffer.Lock(2, 16, out void* lockedVertices, out uint startVertex);

        Assert.AreEqual(
            (0, 32u, 32u, (uint) D3D9.LockNooverwrite, 2u, (nint) 0x1000, true),
            (result, _lockOffset, _lockSize, _lockFlags, startVertex, (nint) lockedVertices, buffer.IsLocked));
        buffer.Unlock(2);
    }

    [TestMethod]
    public void WhenNextIndexChunkDoesNotFitThenBufferIsDiscarded()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        _lockDataToReturn = (nint) 0x2000;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 12, out Direct3D9HardwareIndexBuffer? buffer);

        buffer!.Lock(3, out _, out _);
        buffer.Unlock();
        int result = buffer.Lock(4, out ushort* lockedIndices, out uint startIndex);

        Assert.AreEqual(
            (0, 0u, 8u, (uint) D3D9.LockDiscard, 0u, (nint) 0x2000, true),
            (result, _lockOffset, _lockSize, _lockFlags, startIndex, (nint) lockedIndices, buffer.IsLocked));
        buffer.Unlock();
    }

    [TestMethod]
    public void WhenRequestedChunkExceedsCapacityThenNativeLockIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? buffer);

        int result = buffer!.Lock(3, 12, out void* lockedVertices, out uint startVertex);

        Assert.AreEqual(
            (Direct3D9Factory.InsufficientBufferHResult, 0, 0, 0u, (nint) 0, 0u),
            (result, _lockCallCount, _unlockCallCount, _lockFlags, (nint) lockedVertices, startVertex));
    }

    [TestMethod]
    public void WhenSuccessfulLockReturnsNullThenBufferIsUnlockedAndDriverErrorIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);

        int result = buffer!.Lock(3, out ushort* lockedIndices, out _);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, 1, 1, (nint) 0, false),
            (result, _lockCallCount, _unlockCallCount, (nint) lockedIndices, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenRequestedIndexChunkExceedsCapacityThenNativeLockIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 4, out Direct3D9HardwareIndexBuffer? buffer);

        int result = buffer!.Lock(3, out ushort* lockedIndices, out uint startIndex);

        Assert.AreEqual(
            (Direct3D9Factory.InsufficientBufferHResult, 0, 0, 0u, (nint) 0, 0u, false),
            (result, _lockCallCount, _unlockCallCount, _lockFlags, (nint) lockedIndices, startIndex, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenVertexLockFailsThenHResultAndInitializedOutputsArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        _lockResult = Direct3D9Factory.GenericFailureHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 64, out Direct3D9HardwareVertexBuffer? buffer);

        int result = buffer!.Lock(3, 8, out void* lockedVertices, out uint startVertex);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 1, 0, 0u, 24u, (uint) D3D9.LockNooverwrite, (nint) 0, 0u, false),
            (result, _lockCallCount, _unlockCallCount, _lockOffset, _lockSize, _lockFlags,
                (nint) lockedVertices, startVertex, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenIndexLockFailsThenHResultAndInitializedOutputsArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        _lockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);

        int result = buffer!.Lock(3, out ushort* lockedIndices, out uint startIndex);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 0, 0u, 6u, (uint) D3D9.LockNooverwrite, (nint) 0, 0u, false),
            (result, _lockCallCount, _unlockCallCount, _lockOffset, _lockSize, _lockFlags,
                (nint) lockedIndices, startIndex, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenSuccessfulVertexLockReturnsLockOffsetThenCompensationUnlockFailureIsIgnored()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        _lockDataToReturn = (nint) 0x3000;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 64, out Direct3D9HardwareVertexBuffer? buffer);
        buffer!.Lock(3, 8, out _, out _);
        buffer.Unlock(3);
        _lockDataToReturn = (nint) 24;
        _unlockResult = Direct3D9Factory.InvalidCallHResult;

        int result = buffer.Lock(3, 8, out void* lockedVertices, out uint startVertex);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, 2, 2, 24u, 24u,
                (uint) D3D9.LockNooverwrite, (nint) 0, 3u, false),
            (result, _lockCallCount, _unlockCallCount, _lockOffset, _lockSize, _lockFlags,
                (nint) lockedVertices, startVertex, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenSuccessfulIndexLockReturnsLockOffsetThenCompensationUnlockFailureIsIgnored()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        _lockDataToReturn = (nint) 0x4000;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);
        buffer!.Lock(3, out _, out _);
        buffer.Unlock();
        _lockDataToReturn = (nint) 6;
        _unlockResult = Direct3D9Factory.GenericFailureHResult;

        int result = buffer.Lock(3, out ushort* lockedIndices, out uint startIndex);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, 2, 2, 6u, 6u,
                (uint) D3D9.LockNooverwrite, (nint) 0, 3u, false),
            (result, _lockCallCount, _unlockCallCount, _lockOffset, _lockSize, _lockFlags,
                (nint) lockedIndices, startIndex, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenUnlockFailsThenVertexBufferRemainsLockedAndUsageIsNotReported()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        _lockDataToReturn = (nint) 0x3000;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 64, out Direct3D9HardwareVertexBuffer? buffer);
        buffer!.Lock(3, 8, out _, out _);
        _unlockResult = Direct3D9Factory.InvalidCallHResult;

        int result = buffer.Unlock(1);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, true), (result, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenVertexUnlockFailsThenRetrySuccessCommitsUsageAndAdvancesFromUsedVertices()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        _lockDataToReturn = (nint) 0x3000;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 128, out Direct3D9HardwareVertexBuffer? buffer);
        buffer!.Lock(4, 8, out _, out _);
        _unlockResult = Direct3D9Factory.InvalidCallHResult;

        int failureResult = buffer.Unlock(1);
        uint nextAfterFailure = buffer.GetNextUsableNumberOfVertices(8);
        bool lockedAfterFailure = buffer.IsLocked;
        _unlockResult = 0;
        int successResult = buffer.Unlock(2);
        bool lockedAfterSuccess = buffer.IsLocked;
        int lockResult = buffer.Lock(2, 8, out _, out uint startVertex);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 12u, true, 0, false, 0, 2u, 16u, 16u, true, 2, 2),
            (failureResult, nextAfterFailure, lockedAfterFailure, successResult, lockedAfterSuccess,
                lockResult, startVertex, _lockOffset, _lockSize, buffer.IsLocked, _lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenIndexUnlockFailsThenRetrySuccessAdvancesByFullLockedChunk()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        _lockDataToReturn = (nint) 0x4000;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);
        buffer!.Lock(3, out _, out _);
        _unlockResult = Direct3D9Factory.GenericFailureHResult;

        int failureResult = buffer.Unlock();
        uint nextAfterFailure = buffer.GetNextUsableNumberOfIndices();
        bool lockedAfterFailure = buffer.IsLocked;
        _unlockResult = 0;
        int successResult = buffer.Unlock();
        int lockResult = buffer.Lock(3, out _, out uint startIndex);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 9u, true, 0, 0, 3u, 6u, 6u, true, 2, 2),
            (failureResult, nextAfterFailure, lockedAfterFailure, successResult, lockResult, startIndex,
                _lockOffset, _lockSize, buffer.IsLocked, _lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenVertexUnlockFailsThenIndexUnlockStateRemainsIndependent()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        _vertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _vertexLockDataToReturn = (nint) 0x5000;
        _indexLockDataToReturn = (nint) 0x6000;
        vertexBuffer!.Lock(3, 8, out _, out _);
        indexBuffer!.Lock(3, out _, out _);
        _unlockResult = Direct3D9Factory.InvalidCallHResult;

        int vertexResult = vertexBuffer.Unlock(2);
        _unlockResult = 0;
        int indexResult = indexBuffer.Unlock();

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, true, false, 2),
            (vertexResult, indexResult, vertexBuffer.IsLocked, indexBuffer.IsLocked, _unlockCallCount));
    }

    [TestMethod]
    public void WhenDisposedVertexBufferIsUnlockedThenNativeUnlockIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? buffer);
        buffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => buffer.Unlock(1));
        Assert.AreEqual(0, _unlockCallCount);
    }

    [TestMethod]
    public void WhenDisposedIndexBufferIsUnlockedThenNativeUnlockIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);
        buffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => buffer.Unlock());
        Assert.AreEqual(0, _unlockCallCount);
    }

    [TestMethod]
    public void WhenCopyingInputIndicesThenOrderIsPreservedAndBufferIsUnlocked()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        ushort* destination = stackalloc ushort[3];
        _lockDataToReturn = (nint) destination;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);
        ReadOnlySpan<uint> input = [2, 0, 1];

        int result = buffer!.CopyFromInputBuffer(input, out uint startIndex);

        Assert.AreEqual(
            (0, 0u, 2u, 0u, 1u, false),
            (result, startIndex, destination[0], destination[1], destination[2], buffer.IsLocked));
    }

    [TestMethod]
    public void WhenUnlockingCopiedIndicesFailsThenFailureIsReturnedAndBufferRemainsLocked()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        ushort* destination = stackalloc ushort[3];
        _lockDataToReturn = (nint) destination;
        _unlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);

        int result = buffer!.CopyFromInputBuffer([0, 1, 2], out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 1, true), (result, _unlockCallCount, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenRelockFailsAfterUnlockFailureThenPendingLockIsReleasedAndLockFailureIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        ushort* destination = stackalloc ushort[3];
        _lockDataToReturn = (nint) destination;
        _unlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);
        buffer!.CopyFromInputBuffer([0, 1, 2], out _);
        _lockResult = Direct3D9Factory.GenericFailureHResult;
        _unlockResult = 0;

        int result = buffer.CopyFromInputBuffer([2, 1, 0], out _);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2, 2, false),
            (result, _lockCallCount, _unlockCallCount, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenCopyingTooManyIndicesThenLockFailureIsReturnedWithoutWritingOrUnlocking()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 4, out Direct3D9HardwareIndexBuffer? buffer);

        int result = buffer!.CopyFromInputBuffer([0, 1, 2], out uint startIndex);

        Assert.AreEqual(
            (Direct3D9Factory.InsufficientBufferHResult, 0u, 0, 0, false),
            (result, startIndex, _lockCallCount, _unlockCallCount, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenCopyingIndicesAfterPreviousChunkThenLockStartIndexAndWordConversionArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        ushort* destination = stackalloc ushort[3];
        _lockDataToReturn = (nint) destination;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);
        _ = buffer!.CopyFromInputBuffer([0, 1, 2], out _);

        int result = buffer.CopyFromInputBuffer([uint.MaxValue, 0x10002, 3], out uint startIndex);

        Assert.AreEqual(
            (0, 3u, ushort.MaxValue, (ushort) 2, (ushort) 3, 2, 2, false),
            (result, startIndex, destination[0], destination[1], destination[2], _lockCallCount, _unlockCallCount, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenRelockAndCleanupUnlockFailThenLockFailureIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        ushort* destination = stackalloc ushort[3];
        _lockDataToReturn = (nint) destination;
        _unlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);
        _ = buffer!.CopyFromInputBuffer([0, 1, 2], out _);
        _lockResult = Direct3D9Factory.GenericFailureHResult;

        int result = buffer.CopyFromInputBuffer([2, 1, 0], out uint startIndex);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 3u, 2, 2, true),
            (result, startIndex, _lockCallCount, _unlockCallCount, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenUnlockFailureIsRecoveredThenCopyCanBeRetried()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        ushort* destination = stackalloc ushort[3];
        _lockDataToReturn = (nint) destination;
        _unlockResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);
        _ = buffer!.CopyFromInputBuffer([0, 1, 2], out _);
        _unlockResult = 0;
        _ = buffer.Unlock();

        int result = buffer.CopyFromInputBuffer([2, 1, 0], out uint startIndex);

        Assert.AreEqual(
            (0, 3u, (ushort) 2, (ushort) 1, (ushort) 0, 2, 3, false),
            (result, startIndex, destination[0], destination[1], destination[2], _lockCallCount, _unlockCallCount, buffer.IsLocked));
    }

    [TestMethod]
    public void WhenDisposedIndexBufferCopiesInputThenNativeLockAndUnlockAreSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? buffer);
        buffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => buffer.CopyFromInputBuffer([0, 1, 2], out _));
        Assert.AreEqual((0, 0), (_lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenIndexedArraysAreResetThenBorrowedStreamsCountsAndProgressAreReplaced()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 144, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 12, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertexDestination = stackalloc byte[144];
        ushort* indexDestination = stackalloc ushort[6];
        _vertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _vertexLockDataToReturn = (nint) vertexDestination;
        _indexLockDataToReturn = (nint) indexDestination;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);
        _ = renderer.PrepareIndexed(vertexBuffer!, indexBuffer!, out _);
        Vector3[] positions = [new(10, 11, 12), new(13, 14, 15), new(16, 17, 18), new(19, 20, 21)];
        uint[] diffuseColors = [100u, 200u, 300u, 400u];
        Vector2[] textureCoordinates = [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f), new(0.7f, 0.8f)];
        uint[] indices = [2u, 0u, 1u, 3u, 3u, 3u];

        renderer.SetArrays(positions, diffuseColors, textureCoordinates, 3, indices, 3);
        positions[0] = new(22, 23, 24);
        diffuseColors[0] = 500;
        textureCoordinates[0] = new(0.9f, 1.0f);
        indices[0] = 1;
        int result = renderer.PrepareIndexed(vertexBuffer, indexBuffer, out Direct3D9GeometryRenderBatch batch);

        Assert.AreEqual(
            (0, new Direct3D9GeometryRenderBatch(3, 3, 1, true), new Vector3(22, 23, 24), 500u, new Vector2(0.9f, 1.0f), (1, 0, 1)),
            (result, batch, *(Vector3*) vertexDestination, *(uint*) (vertexDestination + 12), *(Vector2*) (vertexDestination + 16),
                (indexDestination[0], indexDestination[1], indexDestination[2])));
    }

    [TestMethod]
    public void WhenNonIndexedArraysAreResetThenOptionalStreamsAndProgressRestartFromBorrowedInput()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        byte* destination = stackalloc byte[72];
        _lockDataToReturn = (nint) destination;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [10u, 20u, 30u],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            uint.MaxValue);
        _ = renderer.PrepareNonIndexed(vertexBuffer!, out _);
        Vector3[] positions = [new(10, 11, 12), new(13, 14, 15), new(16, 17, 18)];
        Vector2[] textureCoordinates = [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f)];

        renderer.SetArrays(positions, [], textureCoordinates, 3, [], 3);
        positions[0] = new(19, 20, 21);
        textureCoordinates[0] = new(0.7f, 0.8f);
        int result = renderer.PrepareNonIndexed(vertexBuffer, out Direct3D9GeometryRenderBatch batch);

        Assert.AreEqual(
            (0, new Direct3D9GeometryRenderBatch(0, 0, 1, true), new Vector3(19, 20, 21), uint.MaxValue, new Vector2(0.7f, 0.8f), 2),
            (result, batch, *(Vector3*) destination, *(uint*) (destination + 12), *(Vector2*) (destination + 16), _lockCallCount));
    }

    [TestMethod]
    public void WhenCopyingIndexedDiffuseVerticesThenSeparateStreamsArePackedInNaturalVertexOrder()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* destination = stackalloc byte[72];
        ushort* indexDestination = stackalloc ushort[3];
        _vertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _vertexLockDataToReturn = (nint) destination;
        _indexLockDataToReturn = (nint) indexDestination;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [10u, 20u, 30u],
            [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f)],
            [2u, 0u, 1u],
            0);

        int result = renderer.PrepareIndexed(vertexBuffer!, indexBuffer!, out _);

        Assert.AreEqual(
            (0,
                new Vector3(1, 2, 3), 10u, new Vector2(0.1f, 0.2f),
                new Vector3(4, 5, 6), 20u, new Vector2(0.3f, 0.4f),
                new Vector3(7, 8, 9), 30u, new Vector2(0.5f, 0.6f)),
            (result,
                *(Vector3*) destination, *(uint*) (destination + 12), *(Vector2*) (destination + 16),
                *(Vector3*) (destination + 24), *(uint*) (destination + 36), *(Vector2*) (destination + 40),
                *(Vector3*) (destination + 48), *(uint*) (destination + 60), *(Vector2*) (destination + 64)));
    }

    [TestMethod]
    public void WhenCopyingIndexedNormalVerticesWithoutInputNormalsThenDefaultNormalIsRepeated()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* destination = stackalloc byte[96];
        ushort* indexDestination = stackalloc ushort[3];
        _vertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _vertexLockDataToReturn = (nint) destination;
        _indexLockDataToReturn = (nint) indexDestination;
        Direct3D9GeometryRenderer<Vector3> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f)],
            [2u, 0u, 1u],
            Vector3.UnitZ);

        int result = renderer.PrepareIndexed(vertexBuffer!, indexBuffer!, out _);

        Assert.AreEqual(
            (0, Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitZ, new Vector2(0.1f, 0.2f), new Vector2(0.5f, 0.6f)),
            (result,
                *(Vector3*) (destination + 12), *(Vector3*) (destination + 44), *(Vector3*) (destination + 76),
                *(Vector2*) (destination + 24), *(Vector2*) (destination + 88)));
    }

    [TestMethod]
    public void WhenPreparingIndexedGeometryThenVerticesAndIndicesArePackedInInputOrder()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertexDestination = stackalloc byte[72];
        ushort* indexDestination = stackalloc ushort[3];
        _vertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _vertexLockDataToReturn = (nint) vertexDestination;
        _indexLockDataToReturn = (nint) indexDestination;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [10u, 20u, 30u],
            [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f)],
            [2u, 0u, 1u],
            0);

        int result = renderer.PrepareIndexed(vertexBuffer!, indexBuffer!, out Direct3D9GeometryRenderBatch batch);

        Assert.AreEqual(
            (0, new Direct3D9GeometryRenderBatch(0, 0, 1, true), new Vector3(1, 2, 3), 20u, new Vector2(0.5f, 0.6f), (2, 0, 1), 2),
            (result, batch, *(Vector3*) vertexDestination, *(uint*) (vertexDestination + 36), *(Vector2*) (vertexDestination + 64), (indexDestination[0], indexDestination[1], indexDestination[2]), _unlockCallCount));
    }

    [TestMethod]
    public void WhenIndexedGeometryUsesMultipleIndexChunksThenVertexStartIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 144, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertexDestination = stackalloc byte[144];
        ushort* indexDestination = stackalloc ushort[3];
        _vertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _vertexLockDataToReturn = (nint) vertexDestination;
        _indexLockDataToReturn = (nint) indexDestination;
        Direct3D9GeometryRenderer<uint> previousRenderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [],
            0);
        _ = previousRenderer.PrepareNonIndexed(vertexBuffer!, out _);
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [10u, 20u, 30u],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u, 2u, 1u, 0u],
            0);

        int firstResult = renderer.PrepareIndexed(vertexBuffer, indexBuffer!, out Direct3D9GeometryRenderBatch firstBatch);
        int secondResult = renderer.PrepareIndexed(vertexBuffer, indexBuffer, out Direct3D9GeometryRenderBatch secondBatch);
        int thirdResult = renderer.PrepareIndexed(vertexBuffer, indexBuffer, out Direct3D9GeometryRenderBatch completedBatch);

        Assert.AreEqual(
            (0, 0, 0,
                new Direct3D9GeometryRenderBatch(3, 0, 1, true),
                new Direct3D9GeometryRenderBatch(3, 0, 1, true),
                new Direct3D9GeometryRenderBatch(3, 0, 0, false),
                4, 4),
            (firstResult, secondResult, thirdResult, firstBatch, secondBatch, completedBatch, _lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenFirstIndexedVertexLockFailsThenIndexBufferIsNotTouched()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        _vertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _lockResult = Direct3D9Factory.GenericFailureHResult;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [0u, 1u, 2u],
            0);

        int result = renderer.PrepareIndexed(vertexBuffer!, indexBuffer!, out Direct3D9GeometryRenderBatch batch);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, new Direct3D9GeometryRenderBatch(0, 0, 0, true), 1, 0),
            (result, batch, _lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenDisposedVertexBufferPreparesIndexedGeometryThenNativeBuffersAreNotTouched()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        vertexBuffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
        {
            Direct3D9GeometryRenderer<uint> renderer = new(
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [],
                [Vector2.Zero, Vector2.Zero, Vector2.Zero],
                [0u, 1u, 2u],
                0);
            renderer.PrepareIndexed(vertexBuffer, indexBuffer!, out _);
        });
        Assert.AreEqual((0, 0), (_lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenDisposedIndexBufferPreparesIndexedGeometryThenVertexBufferIsNotLocked()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 6, out Direct3D9HardwareIndexBuffer? indexBuffer);
        indexBuffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
        {
            Direct3D9GeometryRenderer<uint> renderer = new(
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [],
                [Vector2.Zero, Vector2.Zero, Vector2.Zero],
                [0u, 1u, 2u],
                0);
            renderer.PrepareIndexed(vertexBuffer!, indexBuffer, out _);
        });
        Assert.AreEqual((0, 0), (_lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenPreparingNonIndexedGeometryThenIndexOrderAndDefaultVertexDataAreUsed()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        byte* destination = stackalloc byte[72];
        _lockDataToReturn = (nint) destination;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f)],
            [2u, 0u, 1u],
            0xFFFFFFFF);

        int result = renderer.PrepareNonIndexed(vertexBuffer!, out Direct3D9GeometryRenderBatch batch);

        Assert.AreEqual(
            (0, new Direct3D9GeometryRenderBatch(0, 0, 1, true), new Vector3(7, 8, 9), 0xFFFFFFFFu, new Vector2(0.3f, 0.4f), false),
            (result, batch, *(Vector3*) destination, *(uint*) (destination + 12), *(Vector2*) (destination + 64), vertexBuffer.IsLocked));
    }

    [TestMethod]
    public void WhenExplicitIndicesUseMultipleChunksThenSecondInputIndexStartSelectsAndPacksCompleteDiffuseVertices()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        byte* destination = stackalloc byte[72];
        _lockDataToReturn = (nint) destination;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9), new(10, 11, 12), new(13, 14, 15), new(16, 17, 18)],
            [10u, 20u, 30u, 40u, 50u, 60u],
            [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f), new(0.7f, 0.8f), new(0.9f, 1.0f), new(1.1f, 1.2f)],
            [2u, 0u, 1u, 5u, 3u, 4u],
            0);

        _ = renderer.PrepareNonIndexed(vertexBuffer!, out _);
        int result = renderer.PrepareNonIndexed(vertexBuffer, out Direct3D9GeometryRenderBatch batch);

        Assert.AreEqual(
            (0, new Direct3D9GeometryRenderBatch(0, 0, 1, true),
                new Vector3(16, 17, 18), 60u, new Vector2(1.1f, 1.2f),
                new Vector3(10, 11, 12), 40u, new Vector2(0.7f, 0.8f),
                new Vector3(13, 14, 15), 50u, new Vector2(0.9f, 1.0f)),
            (result, batch,
                *(Vector3*) destination, *(uint*) (destination + 12), *(Vector2*) (destination + 16),
                *(Vector3*) (destination + 24), *(uint*) (destination + 36), *(Vector2*) (destination + 40),
                *(Vector3*) (destination + 48), *(uint*) (destination + 60), *(Vector2*) (destination + 64)));
    }

    [TestMethod]
    public void WhenImplicitIndicesAreUsedThenContinuousVerticesAndDefaultNormalsArePackedWithFullStride()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        byte* destination = stackalloc byte[96];
        _lockDataToReturn = (nint) destination;
        Direct3D9GeometryRenderer<Vector3> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f)],
            [],
            Vector3.UnitY);

        int result = renderer.PrepareNonIndexed(vertexBuffer!, out Direct3D9GeometryRenderBatch batch);

        Assert.AreEqual(
            (0, new Direct3D9GeometryRenderBatch(0, 0, 1, true),
                new Vector3(1, 2, 3), Vector3.UnitY, new Vector2(0.1f, 0.2f),
                new Vector3(4, 5, 6), Vector3.UnitY, new Vector2(0.3f, 0.4f),
                new Vector3(7, 8, 9), Vector3.UnitY, new Vector2(0.5f, 0.6f)),
            (result, batch,
                *(Vector3*) destination, *(Vector3*) (destination + 12), *(Vector2*) (destination + 24),
                *(Vector3*) (destination + 32), *(Vector3*) (destination + 44), *(Vector2*) (destination + 56),
                *(Vector3*) (destination + 64), *(Vector3*) (destination + 76), *(Vector2*) (destination + 88)));
    }

    [TestMethod]
    public void WhenAllNonIndexedGeometryWasPreparedThenNextBatchDoesNotRenderOrLock()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        byte* destination = stackalloc byte[72];
        _lockDataToReturn = (nint) destination;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [new(0.1f, 0.2f), new(0.3f, 0.4f), new(0.5f, 0.6f)],
            [],
            0xFFFFFFFF);
        renderer.PrepareNonIndexed(vertexBuffer!, out _);

        int result = renderer.PrepareNonIndexed(vertexBuffer, out Direct3D9GeometryRenderBatch batch);

        Assert.AreEqual((0, new Direct3D9GeometryRenderBatch(0, 0, 0, false), 1), (result, batch, _lockCallCount));
    }

    [TestMethod]
    public void WhenNonIndexedGeometryUsesMultipleVertexChunksThenAvailableVerticesAreClippedAndInputOrderAdvances()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        byte* destination = stackalloc byte[72];
        _lockDataToReturn = (nint) destination;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9), new(10, 11, 12), new(13, 14, 15), new(16, 17, 18)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [2u, 0u, 1u, 5u, 3u, 4u],
            0xFFFFFFFF);

        int firstResult = renderer.PrepareNonIndexed(vertexBuffer!, out Direct3D9GeometryRenderBatch firstBatch);
        Vector3 firstChunkPosition = *(Vector3*) destination;
        int secondResult = renderer.PrepareNonIndexed(vertexBuffer, out Direct3D9GeometryRenderBatch secondBatch);
        Vector3 secondChunkPosition = *(Vector3*) destination;
        int thirdResult = renderer.PrepareNonIndexed(vertexBuffer, out Direct3D9GeometryRenderBatch completedBatch);

        Assert.AreEqual(
            (0, 0, 0,
                new Direct3D9GeometryRenderBatch(0, 0, 1, true),
                new Direct3D9GeometryRenderBatch(0, 0, 1, true),
                new Direct3D9GeometryRenderBatch(0, 0, 0, false),
                new Vector3(7, 8, 9), new Vector3(16, 17, 18), 2, 2),
            (firstResult, secondResult, thirdResult, firstBatch, secondBatch, completedBatch,
                firstChunkPosition, secondChunkPosition, _lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenNonIndexedVertexLockFailsThenProgressIsNotCommittedAndTheSameChunkCanBeRetried()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        byte* destination = stackalloc byte[72];
        _lockDataToReturn = (nint) destination;
        _lockResult = Direct3D9Factory.GenericFailureHResult;
        Direct3D9GeometryRenderer<uint> renderer = new(
            [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
            [],
            [Vector2.Zero, Vector2.Zero, Vector2.Zero],
            [2u, 0u, 1u],
            0);

        int firstResult = renderer.PrepareNonIndexed(vertexBuffer!, out Direct3D9GeometryRenderBatch failedBatch);
        _lockResult = 0;
        int secondResult = renderer.PrepareNonIndexed(vertexBuffer, out Direct3D9GeometryRenderBatch retriedBatch);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0,
                new Direct3D9GeometryRenderBatch(0, 0, 0, true),
                new Direct3D9GeometryRenderBatch(0, 0, 1, true),
                new Vector3(7, 8, 9), 2, 1, false),
            (firstResult, secondResult, failedBatch, retriedBatch, *(Vector3*) destination,
                _lockCallCount, _unlockCallCount, vertexBuffer.IsLocked));
    }

    [TestMethod]
    public void WhenDisposedVertexBufferPreparesNonIndexedGeometryThenNativeLockAndUnlockAreSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 72, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        vertexBuffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
        {
            Direct3D9GeometryRenderer<uint> renderer = new(
                [new(1, 2, 3), new(4, 5, 6), new(7, 8, 9)],
                [],
                [Vector2.Zero, Vector2.Zero, Vector2.Zero],
                [],
                0);
            renderer.PrepareNonIndexed(vertexBuffer, out _);
        });
        Assert.AreEqual((0, 0), (_lockCallCount, _unlockCallCount));
    }

    [TestMethod]
    public void WhenQueryingVertexCapacityThenByteCapacityIsDividedByStrideWithoutChangingLocatorState()
    {
        Direct3D9BufferSpaceLocator locator = new(100);

        uint capacityForFourByteElements = locator.GetMaximumCapacity(4);
        uint capacityForSixByteElements = locator.GetMaximumCapacity(6);

        Assert.AreEqual(
            (25u, 16u, 0u, 0u),
            (capacityForFourByteElements, capacityForSixByteElements, locator.CurrentBytePosition, locator.NumberOfBytesInLatestChunk));
    }

    [TestMethod]
    public void WhenQueryingInitialVertexIntervalsThenStrideConversionAndTriangleAlignmentAreIndependent()
    {
        Direct3D9BufferSpaceLocator locator = new(100);

        uint fourByteElements = locator.GetNextUsableNumberOfElements(4);
        uint sixByteElements = locator.GetNextUsableNumberOfElements(6);

        Assert.AreEqual((24u, 15u), (fourByteElements, sixByteElements));
    }

    [TestMethod]
    public void WhenQueryingVertexIntervalAfterNoOverwriteThenOnlyRemainingContinuousSpaceIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.VertexBuffer;
        _lockDataToReturn = (nint) 0x4000;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 100, out Direct3D9HardwareVertexBuffer? buffer);
        buffer!.Lock(3, 12, out _, out _);
        buffer.Unlock(2);
        int lockCallCount = _lockCallCount;

        uint maximumCapacity = buffer.GetMaximumCapacity(12);
        uint firstQuery = buffer.GetNextUsableNumberOfVertices(12);
        uint secondQuery = buffer.GetNextUsableNumberOfVertices(12);

        Assert.AreEqual((8u, 6u, 6u, lockCallCount), (maximumCapacity, firstQuery, secondQuery, _lockCallCount));
    }

    [TestMethod]
    public void WhenLatestVertexChunkExactlyFillsBufferThenNextIntervalWrapsToFullTriangleCapacity()
    {
        Direct3D9BufferSpaceLocator locator = new(24);
        locator.AdvanceToNextChunk(3, 8, out _, out _);
        locator.ReportNumberOfElementsUsedInLastChunk(3);

        uint nextUsableElements = locator.GetNextUsableNumberOfElements(8);

        Assert.AreEqual(3u, nextUsableElements);
    }

    [TestMethod]
    public void WhenRemainingVertexBytesCannotHoldThreeElementsThenNextIntervalUsesWrappedCapacity()
    {
        Direct3D9BufferSpaceLocator locator = new(50);
        locator.AdvanceToNextChunk(3, 8, out _, out _);
        locator.ReportNumberOfElementsUsedInLastChunk(3);

        uint nextUsableElements = locator.GetNextUsableNumberOfElements(10);

        Assert.AreEqual(3u, nextUsableElements);
    }

    [TestMethod]
    public void WhenQueryingIndexCapacityThenFixedSixteenBitStrideIsUsedWithoutLocking()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 20, out Direct3D9HardwareIndexBuffer? buffer);

        uint maximumCapacity = buffer!.GetMaximumCapacity();
        uint firstQuery = buffer.GetNextUsableNumberOfIndices();
        uint secondQuery = buffer.GetNextUsableNumberOfIndices();

        Assert.AreEqual((10u, 9u, 9u, 0), (maximumCapacity, firstQuery, secondQuery, _lockCallCount));
    }

    [TestMethod]
    public void WhenQueryingIndexIntervalAfterNoOverwriteThenReportedUsageControlsRemainingSpace()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject bufferObject = new();
        _bufferToReturn = (nint) bufferObject.IndexBuffer;
        _lockDataToReturn = (nint) 0x5000;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 20, out Direct3D9HardwareIndexBuffer? buffer);
        buffer!.Lock(3, out _, out _);
        buffer.Unlock();
        int lockCallCount = _lockCallCount;

        uint nextUsableIndices = buffer.GetNextUsableNumberOfIndices();

        Assert.AreEqual((6u, lockCallCount), (nextUsableIndices, _lockCallCount));
    }

    [TestMethod]
    public void WhenQueryingVertexCapacityWithZeroStrideThenExistingDivisionProtectionIsPreserved()
    {
        Direct3D9BufferSpaceLocator locator = new(24);

        Assert.ThrowsExactly<DivideByZeroException>(() => locator.GetMaximumCapacity(0));
    }

    [TestMethod]
    public void WhenQueryingInitialHardwareBuffersThenUnderlyingIdentitiesAreBorrowedWithoutSideEffects()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);

        nint firstVertexIdentity = (nint) vertexBuffer!.DangerousGetDirect3DVertexBuffer();
        nint secondVertexIdentity = (nint) vertexBuffer.DangerousGetDirect3DVertexBuffer();
        nint firstIndexIdentity = (nint) indexBuffer!.DangerousGetDirect3DIndexBuffer();
        nint secondIndexIdentity = (nint) indexBuffer.DangerousGetDirect3DIndexBuffer();

        Assert.AreEqual(
            ((nint) vertexBufferObject.VertexBuffer, firstVertexIdentity, firstVertexIdentity,
                (nint) indexBufferObject.IndexBuffer, firstIndexIdentity, firstIndexIdentity,
                false, false, 0, 0, 0),
            (firstVertexIdentity, secondVertexIdentity, (nint) vertexBuffer.DangerousGetDirect3DVertexBuffer(),
                firstIndexIdentity, secondIndexIdentity, (nint) indexBuffer.DangerousGetDirect3DIndexBuffer(),
                vertexBuffer.IsLocked, indexBuffer.IsLocked, _lockCallCount, _unlockCallCount, _releaseCount));
    }

    [TestMethod]
    public void WhenHardwareBuffersLockAndUnlockThenCachedStatesRemainIndependent()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        _vertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _vertexLockDataToReturn = (nint) 0x6000;
        _indexLockDataToReturn = (nint) 0x7000;

        vertexBuffer!.Lock(3, 8, out _, out _);
        (bool Vertex, bool Index) afterVertexLock = (vertexBuffer.IsLocked, indexBuffer!.IsLocked);
        indexBuffer.Lock(3, out _, out _);
        (bool Vertex, bool Index) afterIndexLock = (vertexBuffer.IsLocked, indexBuffer.IsLocked);
        vertexBuffer.Unlock(3);
        (bool Vertex, bool Index) afterVertexUnlock = (vertexBuffer.IsLocked, indexBuffer.IsLocked);
        indexBuffer.Unlock();

        Assert.AreEqual(
            ((true, false), (true, true), (false, true), (false, false), 2, 2, 0),
            (afterVertexLock, afterIndexLock, afterVertexUnlock,
                (vertexBuffer.IsLocked, indexBuffer.IsLocked), _lockCallCount, _unlockCallCount, _releaseCount));
    }

    [TestMethod]
    public void WhenHardwareBufferLocksFailThenCachedStatesAndBorrowedIdentitiesRemainUnchanged()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        _lockResult = Direct3D9Factory.GenericFailureHResult;

        int vertexResult = vertexBuffer!.Lock(3, 8, out _, out _);
        int indexResult = indexBuffer!.Lock(3, out _, out _);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9Factory.GenericFailureHResult,
                false, false, (nint) vertexBufferObject.VertexBuffer, (nint) indexBufferObject.IndexBuffer, 2, 0, 0),
            (vertexResult, indexResult, vertexBuffer.IsLocked, indexBuffer.IsLocked,
                (nint) vertexBuffer.DangerousGetDirect3DVertexBuffer(),
                (nint) indexBuffer.DangerousGetDirect3DIndexBuffer(),
                _lockCallCount, _unlockCallCount, _releaseCount));
    }

    [TestMethod]
    public void WhenLockedHardwareBuffersAreDisposedThenNativeBuffersAreReleasedWithoutUnlock()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _lockDataToReturn = (nint) 0x1000;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        vertexBuffer!.Lock(3, 8, out _, out _);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _lockDataToReturn = (nint) 0x2000;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        indexBuffer!.Lock(3, out _, out _);
        _releasedVertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _releasedIndexBufferIdentity = (nint) indexBufferObject.IndexBuffer;

        vertexBuffer.Dispose();
        indexBuffer.Dispose();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();

        Assert.AreEqual(
            (0, 1, 1, 2, 0),
            (_unlockCallCount, _vertexBufferReleaseCount, _indexBufferReleaseCount, _releaseCount, device.ResourceCount));
    }

    [TestMethod]
    public void WhenManagerDestroysLockedHardwareBuffersThenNativeBuffersAreReleasedWithoutUnlock()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        _lockDataToReturn = (nint) 0x1000;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        vertexBuffer!.Lock(3, 8, out _, out _);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        _lockDataToReturn = (nint) 0x2000;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        indexBuffer!.Lock(3, out _, out _);
        _releasedVertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _releasedIndexBufferIdentity = (nint) indexBufferObject.IndexBuffer;

        device.MarkUnusable();
        vertexBuffer.Dispose();
        indexBuffer.Dispose();

        Assert.AreEqual(
            (0, 1, 1, 2, 0, true, true),
            (_unlockCallCount, _vertexBufferReleaseCount, _indexBufferReleaseCount, _releaseCount,
                device.ResourceCount, vertexBuffer.IsReleased, indexBuffer.IsReleased));
    }

    [TestMethod]
    public void WhenHardwareBuffersAreDisposedThenReadOnlyQueriesAreRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        vertexBuffer!.Dispose();
        indexBuffer!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => vertexBuffer.GetMaximumCapacity(8));
        Assert.ThrowsExactly<ObjectDisposedException>(() => vertexBuffer.GetNextUsableNumberOfVertices(8));
        Assert.ThrowsExactly<ObjectDisposedException>(() => vertexBuffer.IsLocked);
        Assert.ThrowsExactly<ObjectDisposedException>(() => vertexBuffer.DangerousGetDirect3DVertexBuffer());
        Assert.ThrowsExactly<ObjectDisposedException>(() => indexBuffer.GetMaximumCapacity());
        Assert.ThrowsExactly<ObjectDisposedException>(() => indexBuffer.GetNextUsableNumberOfIndices());
        Assert.ThrowsExactly<ObjectDisposedException>(() => indexBuffer.IsLocked);
        Assert.ThrowsExactly<ObjectDisposedException>(() => indexBuffer.DangerousGetDirect3DIndexBuffer());
    }

    [TestMethod]
    public void WhenHardwareBuffersAreDisposedThenUnderlyingResourcesAreReleasedOnceAndRemovedIndependently()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        _releasedVertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _releasedIndexBufferIdentity = (nint) indexBufferObject.IndexBuffer;

        vertexBuffer!.Dispose();
        vertexBuffer.Dispose();
        (int VertexReleases, int IndexReleases, int Resources, nint IndexIdentity) afterVertexRelease =
            (_vertexBufferReleaseCount, _indexBufferReleaseCount, device.ResourceCount,
                (nint) indexBuffer!.DangerousGetDirect3DIndexBuffer());
        indexBuffer.Dispose();
        indexBuffer.Dispose();

        Assert.AreEqual(
            ((1, 0, 1, (nint) indexBufferObject.IndexBuffer), (1, 1, 0, 2)),
            (afterVertexRelease,
                (_vertexBufferReleaseCount, _indexBufferReleaseCount, device.ResourceCount, _releaseCount)));
    }

    [TestMethod]
    public void WhenDeviceBecomesUnusableThenBothHardwareBuffersAreReleasedOnceAndLaterCleanupIsIdempotent()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _bufferToReturn = (nint) vertexBufferObject.VertexBuffer;
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        _bufferToReturn = (nint) indexBufferObject.IndexBuffer;
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        _releasedVertexBufferIdentity = (nint) vertexBufferObject.VertexBuffer;
        _releasedIndexBufferIdentity = (nint) indexBufferObject.IndexBuffer;

        device.MarkUnusable();
        device.MarkUnusable();
        vertexBuffer!.Dispose();
        indexBuffer!.Dispose();

        Assert.AreEqual(
            (1, 1, 2, 0, true, true),
            (_vertexBufferReleaseCount, _indexBufferReleaseCount, _releaseCount, device.ResourceCount,
                vertexBuffer.IsReleased, indexBuffer.IsReleased));
    }

    [TestMethod]
    public void WhenQueryingNextUsableElementsThenTriangleMultipleAndWrapRulesArePreserved()
    {
        Direct3D9BufferSpaceLocator locator = new(20);
        locator.AdvanceToNextChunk(8, 2, out _, out _);
        locator.ReportNumberOfElementsUsedInLastChunk(8);

        uint nextUsableElements = locator.GetNextUsableNumberOfElements(2);

        Assert.AreEqual(9u, nextUsableElements);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self)
    {
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateVertexBuffer(
        IDirect3DDevice9* self,
        uint length,
        uint usage,
        uint flexibleVertexFormat,
        Pool pool,
        IDirect3DVertexBuffer9** buffer,
        void** sharedHandle)
    {
        _createLength = length;
        _createUsage = usage;
        _createFlexibleVertexFormat = flexibleVertexFormat;
        _createPool = pool;
        *buffer = (IDirect3DVertexBuffer9*) _bufferToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateIndexBuffer(
        IDirect3DDevice9* self,
        uint length,
        uint usage,
        Format format,
        Pool pool,
        IDirect3DIndexBuffer9** buffer,
        void** sharedHandle)
    {
        _createLength = length;
        _createUsage = usage;
        _createFormat = format;
        _createPool = pool;
        *buffer = (IDirect3DIndexBuffer9*) _bufferToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockBuffer(void* self, uint offsetToLock, uint sizeToLock, void** data, uint flags)
    {
        _lockCallCount++;
        _lockOffset = offsetToLock;
        _lockSize = sizeToLock;
        _lockFlags = flags;
        nint lockData = _lockDataToReturn;
        if (_vertexBufferIdentity != 0)
        {
            lockData = (nint) self == _vertexBufferIdentity
                ? _vertexLockDataToReturn
                : _indexLockDataToReturn;
        }

        *data = (void*) lockData;
        return _lockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockBuffer(void* self)
    {
        _unlockCallCount++;
        return _unlockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseBuffer(void* self)
    {
        _releaseCount++;
        nint identity = (nint) self;
        if (identity == _releasedVertexBufferIdentity)
        {
            _vertexBufferReleaseCount++;
        }
        else if (identity == _releasedIndexBufferIdentity)
        {
            _indexBufferReleaseCount++;
        }

        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 30);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[26] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, Pool, IDirect3DVertexBuffer9**, void**, int>) &CreateVertexBuffer;
            vtable[27] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DIndexBuffer9**, void**, int>) &CreateIndexBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeBufferObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DVertexBuffer9* VertexBuffer;
        internal IDirect3DIndexBuffer9* IndexBuffer;

        public FakeBufferObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 14);
            void** memory = (void**) _memory;
            VertexBuffer = (IDirect3DVertexBuffer9*) memory;
            IndexBuffer = (IDirect3DIndexBuffer9*) memory;
            void** vtable = memory + 1;
            VertexBuffer->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &ReleaseBuffer;
            vtable[11] = (void*) (delegate* unmanaged[Stdcall]<void*, uint, uint, void**, uint, int>) &LockBuffer;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<void*, int>) &UnlockBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            VertexBuffer = null;
            IndexBuffer = null;
        }
    }
}
