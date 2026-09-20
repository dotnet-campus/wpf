using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceIndexedUpDrawTests
{
    private static nint _vertexBuffer;
    private static nint _indexBuffer;
    private static nint _vertexLockData;
    private static nint _indexLockData;
    private static int _vertexLockResult;
    private static int _indexLockResult;
    private static int _vertexUnlockResult;
    private static int _indexUnlockResult;
    private static int _setStreamSourceResult;
    private static int _setIndicesResult;
    private static int _drawResult;
    private static int _vertexUnlockCount;
    private static int _indexUnlockCount;
    private static int _indexedDrawCount;
    private static int _indexedUpDrawCount;
    private static nint _streamSource;
    private static nint _indices;
    private static Primitivetype _primitiveType;
    private static int _baseVertexIndex;
    private static uint _minVertexIndex;
    private static uint _vertexCount;
    private static uint _startIndex;
    private static uint _primitiveCount;
    private static nint _inputIndices;
    private static Format _indexFormat;
    private static nint _inputVertices;
    private static uint _vertexStride;

    [TestInitialize]
    public void Initialize()
    {
        _vertexBuffer = 0;
        _indexBuffer = 0;
        _vertexLockData = 0;
        _indexLockData = 0;
        _vertexLockResult = 0;
        _indexLockResult = 0;
        _vertexUnlockResult = 0;
        _indexUnlockResult = 0;
        _setStreamSourceResult = 0;
        _setIndicesResult = 0;
        _drawResult = 0;
        _vertexUnlockCount = 0;
        _indexUnlockCount = 0;
        _indexedDrawCount = 0;
        _indexedUpDrawCount = 0;
        _streamSource = 0;
        _indices = 0;
        _primitiveType = 0;
        _baseVertexIndex = 0;
        _minVertexIndex = 0;
        _vertexCount = 0;
        _startIndex = 0;
        _primitiveCount = 0;
        _inputIndices = 0;
        _indexFormat = 0;
        _inputVertices = 0;
        _vertexStride = 0;
    }

    [TestMethod]
    public void WhenBothDynamicBuffersLockThenDataIsCopiedAndIndexedDrawUsesChunkOffsets()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        byte* copiedVertices = stackalloc byte[24];
        ushort* copiedIndices = stackalloc ushort[3];
        _vertexLockData = (nint) copiedVertices;
        _indexLockData = (nint) copiedIndices;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        for (int index = 0; index < 24; index++)
        {
            vertices[index] = (byte) (index + 1);
        }

        ushort* indices = stackalloc ushort[] { 2, 1, 0 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, indices, vertices, 8);

        Assert.AreEqual(
            (0, 1, 0, Primitivetype.Trianglelist, 0, 0u, 3u, 0u, 1u, (nint) vertexBufferObject.VertexBuffer, (nint) indexBufferObject.IndexBuffer),
            (result, _indexedDrawCount, _indexedUpDrawCount, _primitiveType, _baseVertexIndex, _minVertexIndex, _vertexCount, _startIndex, _primitiveCount, _streamSource, _indices));
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, new ReadOnlySpan<byte>(copiedVertices, 24).ToArray());
        CollectionAssert.AreEqual(new ushort[] { 2, 1, 0 }, new ReadOnlySpan<ushort>(copiedIndices, 3).ToArray());
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenVertexBufferCannotFitThenCachesAreClearedBeforeSlot84Fallback()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 8, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        ushort* indices = stackalloc ushort[] { 0, 1, 2 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, indices, vertices, 8);

        Assert.AreEqual(
            (0, 0, 1, Primitivetype.Trianglelist, 0u, 3u, 1u, (nint) indices, Format.Index16, (nint) vertices, 8u, (nint) 0, (nint) 0),
            (result, _indexedDrawCount, _indexedUpDrawCount, _primitiveType, _minVertexIndex, _vertexCount, _primitiveCount, _inputIndices, _indexFormat, _inputVertices, _vertexStride, _streamSource, _indices));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenIndexLockFailsThenVertexLockIsReleasedAndFallbackIsUsed()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        byte* copiedVertices = stackalloc byte[24];
        _vertexLockData = (nint) copiedVertices;
        _indexLockResult = Direct3D9Factory.GenericFailureHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        ushort* indices = stackalloc ushort[] { 0, 1, 2 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, indices, vertices, 8);

        Assert.AreEqual((0, 1, 0, 1), (result, _vertexUnlockCount, _indexUnlockCount, _indexedUpDrawCount));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenSetIndicesReturnsDriverInternalErrorThenDisplayStateInvalidIsReturnedAndDrawIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        byte* copiedVertices = stackalloc byte[24];
        ushort* copiedIndices = stackalloc ushort[3];
        _vertexLockData = (nint) copiedVertices;
        _indexLockData = (nint) copiedIndices;
        _setIndicesResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        ushort* indices = stackalloc ushort[] { 0, 1, 2 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, indices, vertices, 8);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 0, 0, 1, 1, (nint) indexBufferObject.IndexBuffer),
            (result, device.UnusableReasonHResult, _indexedDrawCount, _indexedUpDrawCount, _vertexUnlockCount, _indexUnlockCount, _indices));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenVertexUnlockFailsThenIndexLockIsReleasedAndDrawIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        byte* copiedVertices = stackalloc byte[24];
        ushort* copiedIndices = stackalloc ushort[3];
        _vertexLockData = (nint) copiedVertices;
        _indexLockData = (nint) copiedIndices;
        _vertexUnlockResult = Direct3D9Factory.GenericFailureHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        ushort* indices = stackalloc ushort[] { 0, 1, 2 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, indices, vertices, 8);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 2, 1, 0, 0),
            (result, _vertexUnlockCount, _indexUnlockCount, _indexedDrawCount, _indexedUpDrawCount));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenIndexUnlockFailsThenDrawIsSkippedWithoutFallback()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        byte* copiedVertices = stackalloc byte[24];
        ushort* copiedIndices = stackalloc ushort[3];
        _vertexLockData = (nint) copiedVertices;
        _indexLockData = (nint) copiedIndices;
        _indexUnlockResult = Direct3D9Factory.GenericFailureHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        ushort* indices = stackalloc ushort[] { 0, 1, 2 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, indices, vertices, 8);

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 1, 2, 0, 0),
            (result, _vertexUnlockCount, _indexUnlockCount, _indexedDrawCount, _indexedUpDrawCount));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenFastPathStreamBindingFailsThenIndicesAndDrawAreSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        byte* copiedVertices = stackalloc byte[24];
        ushort* copiedIndices = stackalloc ushort[3];
        _vertexLockData = (nint) copiedVertices;
        _indexLockData = (nint) copiedIndices;
        _setStreamSourceResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        ushort* indices = stackalloc ushort[] { 0, 1, 2 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, indices, vertices, 8);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, (nint) vertexBufferObject.VertexBuffer, (nint) 0, 0),
            (result, device.UnusableReasonHResult, _streamSource, _indices, _indexedDrawCount));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenFastPathIndexedDrawReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        byte* copiedVertices = stackalloc byte[24];
        ushort* copiedIndices = stackalloc ushort[3];
        _vertexLockData = (nint) copiedVertices;
        _indexLockData = (nint) copiedIndices;
        _drawResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 96, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        ushort* indices = stackalloc ushort[] { 0, 1, 2 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, indices, vertices, 8);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, 0, 1, 1),
            (result, device.UnusableReasonHResult, _indexedDrawCount, _indexedUpDrawCount, _vertexUnlockCount, _indexUnlockCount));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenFallbackDrawReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        _drawResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 8, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        ushort* indices = stackalloc ushort[] { 0, 1, 2 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, indices, vertices, 8);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1), (result, device.UnusableReasonHResult, _indexedUpDrawCount));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenFallbackExceedsMaxPrimitiveCountThenIndexedUpRemainsOneDraw()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        Caps9 capabilities = default;
        capabilities.MaxPrimitiveCount = 1;
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default, capabilities: capabilities);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 8, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[24];
        ushort* indices = stackalloc ushort[] { 0, 1, 2, 2, 1, 0 };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 2, indices, vertices, 8);

        Assert.AreEqual((0, 1, 2u), (result, _indexedUpDrawCount, _primitiveCount));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenFallbackUsesMaximum16BitIndexThenVertexRangeIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 8, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        byte* vertices = stackalloc byte[8];
        ushort* indices = stackalloc ushort[] { ushort.MaxValue - 2, ushort.MaxValue - 1, ushort.MaxValue };

        int result = device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, ushort.MaxValue, 1, indices, vertices, 8);

        Assert.AreEqual((0, 1, ushort.MaxValue, Format.Index16), (result, _indexedUpDrawCount, _vertexCount, _indexFormat));
        vertexBuffer.Dispose();
        indexBuffer.Dispose();
    }

    [TestMethod]
    public void WhenIndexedUpDrawIsCalledAfterDisposeThenThrows()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.VertexBuffer;
        _indexBuffer = (nint) indexBufferObject.IndexBuffer;
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9HardwareVertexBuffer.TryCreate(device, 24, out Direct3D9HardwareVertexBuffer? vertexBuffer);
        Direct3D9HardwareIndexBuffer.TryCreate(device, 24, out Direct3D9HardwareIndexBuffer? indexBuffer);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.DrawIndexedTriangleListUp(vertexBuffer!, indexBuffer!, 3, 1, null, null, 8));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateVertexBuffer(IDirect3DDevice9* self, uint length, uint usage, uint fvf, Pool pool, IDirect3DVertexBuffer9** buffer, void** sharedHandle)
    {
        *buffer = (IDirect3DVertexBuffer9*) _vertexBuffer;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateIndexBuffer(IDirect3DDevice9* self, uint length, uint usage, Format format, Pool pool, IDirect3DIndexBuffer9** buffer, void** sharedHandle)
    {
        *buffer = (IDirect3DIndexBuffer9*) _indexBuffer;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockBuffer(void* self, uint offset, uint size, void** data, uint flags)
    {
        bool isVertexBuffer = (nint) self == _vertexBuffer;
        *data = (void*) (isVertexBuffer ? _vertexLockData : _indexLockData);
        return isVertexBuffer ? _vertexLockResult : _indexLockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockBuffer(void* self)
    {
        if ((nint) self == _vertexBuffer)
        {
            _vertexUnlockCount++;
            return _vertexUnlockResult;
        }

        _indexUnlockCount++;
        return _indexUnlockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetStreamSource(IDirect3DDevice9* self, uint stream, IDirect3DVertexBuffer9* buffer, uint offset, uint stride)
    {
        _streamSource = (nint) buffer;
        return _setStreamSourceResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetIndices(IDirect3DDevice9* self, IDirect3DIndexBuffer9* buffer)
    {
        _indices = (nint) buffer;
        return _setIndicesResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int DrawIndexedPrimitive(IDirect3DDevice9* self, Primitivetype primitiveType, int baseVertexIndex, uint minVertexIndex, uint vertexCount, uint startIndex, uint primitiveCount)
    {
        _indexedDrawCount++;
        _primitiveType = primitiveType;
        _baseVertexIndex = baseVertexIndex;
        _minVertexIndex = minVertexIndex;
        _vertexCount = vertexCount;
        _startIndex = startIndex;
        _primitiveCount = primitiveCount;
        return _drawResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int DrawIndexedPrimitiveUp(IDirect3DDevice9* self, Primitivetype primitiveType, uint minVertexIndex, uint vertexCount, uint primitiveCount, void* indexData, Format indexFormat, void* vertexData, uint vertexStride)
    {
        _indexedUpDrawCount++;
        _primitiveType = primitiveType;
        _minVertexIndex = minVertexIndex;
        _vertexCount = vertexCount;
        _primitiveCount = primitiveCount;
        _inputIndices = (nint) indexData;
        _indexFormat = indexFormat;
        _inputVertices = (nint) vertexData;
        _vertexStride = vertexStride;
        return _drawResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 107);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &Release;
            vtable[26] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, Pool, IDirect3DVertexBuffer9**, void**, int>) &CreateVertexBuffer;
            vtable[27] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, Format, Pool, IDirect3DIndexBuffer9**, void**, int>) &CreateIndexBuffer;
            vtable[82] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, int, uint, uint, uint, uint, int>) &DrawIndexedPrimitive;
            vtable[84] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, uint, uint, void*, Format, void*, uint, int>) &DrawIndexedPrimitiveUp;
            vtable[100] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, IDirect3DVertexBuffer9*, uint, uint, int>) &SetStreamSource;
            vtable[104] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DIndexBuffer9*, int>) &SetIndices;
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
        private nint _vertexMemory;
        private nint _indexMemory;
        internal IDirect3DVertexBuffer9* VertexBuffer;
        internal IDirect3DIndexBuffer9* IndexBuffer;

        public FakeBufferObject()
        {
            _vertexMemory = CreateBuffer(out void* vertexBuffer);
            _indexMemory = CreateBuffer(out void* indexBuffer);
            VertexBuffer = (IDirect3DVertexBuffer9*) vertexBuffer;
            IndexBuffer = (IDirect3DIndexBuffer9*) indexBuffer;
        }

        private static nint CreateBuffer(out void* buffer)
        {
            nint memoryAddress = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 14);
            void** memory = (void**) memoryAddress;
            buffer = memory;
            void** vtable = memory + 1;
            *(void***) buffer = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &Release;
            vtable[11] = (void*) (delegate* unmanaged[Stdcall]<void*, uint, uint, void**, uint, int>) &LockBuffer;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<void*, int>) &UnlockBuffer;
            return memoryAddress;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _vertexMemory);
            NativeMemory.Free((void*) _indexMemory);
            _vertexMemory = 0;
            _indexMemory = 0;
            VertexBuffer = null;
            IndexBuffer = null;
        }
    }
}
