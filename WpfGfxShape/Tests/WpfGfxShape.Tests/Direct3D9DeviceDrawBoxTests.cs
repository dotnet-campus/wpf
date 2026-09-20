using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceDrawBoxTests
{
    private static nint _vertexBuffer;
    private static nint _indexBuffer;
    private static Direct3D9VertexXyzDiffuseUv2[] _vertices = [];
    private static ushort[] _indices = [];
    private static readonly List<string> _bufferCreationOrder = [];
    private static readonly List<string> _releaseOrder = [];
    private static nint _device;
    private static int _vertexBufferCreateResult;
    private static int _indexBufferCreateResult;
    private static int _vertexBufferReleaseCount;
    private static int _indexBufferReleaseCount;
    private static int _drawCount;

    [TestInitialize]
    public void Initialize()
    {
        _vertexBuffer = 0;
        _indexBuffer = 0;
        _vertices = [];
        _indices = [];
        _bufferCreationOrder.Clear();
        _releaseOrder.Clear();
        _device = 0;
        _vertexBufferCreateResult = 0;
        _indexBufferCreateResult = 0;
        _vertexBufferReleaseCount = 0;
        _indexBufferReleaseCount = 0;
        _drawCount = 0;
    }

    [TestMethod]
    public void WhenBoxIsDrawnThenNativeMeshAndPipelineStateMatchOriginal()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        _indexBuffer = (nint) indexBufferObject.Buffer;
        List<(Renderstatetype State, uint Value)> renderStates = [];
        using Direct3D9Device device = CreateDevice(deviceObject.Device, (state, value) =>
        {
            renderStates.Add((state, value));
            return 0;
        });
        Assert.AreEqual(0, device.SetRenderState(Renderstatetype.Fillmode, (uint) Fillmode.Solid));
        Assert.AreEqual(0, device.SetRenderState(Renderstatetype.Zfunc, (uint) Cmpfunc.Lessequal));
        Assert.AreEqual(0, device.InitializeDynamicBuffers());

        int result = device.DrawBox(new Direct3D9Box(1, 2, 3, 4, 5, 6), Fillmode.Wireframe, 0x80402010);

        Assert.AreEqual((0, 1, 8, 36), (result, _drawCount, _vertices.Length, _indices.Length));
        Assert.AreEqual(
            new Direct3D9VertexXyzDiffuseUv2(5, 7, 9, 0x80402010),
            _vertices[6]);
        CollectionAssert.AreEqual(
            new ushort[]
            {
                0, 2, 1, 0, 3, 2, 4, 5, 6, 4, 6, 7, 0, 4, 3, 3, 4, 7,
                1, 2, 5, 2, 6, 5, 2, 3, 6, 3, 7, 6, 0, 1, 4, 1, 5, 4
            },
            _indices);
        CollectionAssert.Contains(renderStates, (Renderstatetype.Fillmode, (uint) Fillmode.Wireframe));
        CollectionAssert.Contains(renderStates, (Renderstatetype.Zfunc, (uint) Cmpfunc.Always));
        CollectionAssert.AreEqual(
            new[]
            {
                (Renderstatetype.Fillmode, (uint) Fillmode.Solid),
                (Renderstatetype.Zfunc, (uint) Cmpfunc.Lessequal)
            },
            renderStates.TakeLast(2).ToArray());
    }

    [TestMethod]
    public void WhenPipelineSetupFailsThenOriginalStatesAreRestoredAndFirstFailureIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            (_, _) => 0,
            setFlexibleVertexFormat: _ => Direct3D9Factory.GenericFailureHResult);
        Assert.AreEqual(0, device.SetRenderState(Renderstatetype.Fillmode, (uint) Fillmode.Solid));
        Assert.AreEqual(0, device.SetRenderState(Renderstatetype.Zfunc, (uint) Cmpfunc.Lessequal));

        int result = device.DrawBox(default, Fillmode.Wireframe, 0);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        Assert.AreEqual(0, device.GetRenderState(Renderstatetype.Fillmode, out uint fillMode));
        Assert.AreEqual((uint) Fillmode.Solid, fillMode);
        Assert.AreEqual(0, device.GetRenderState(Renderstatetype.Zfunc, out uint depthTest));
        Assert.AreEqual((uint) Cmpfunc.Lessequal, depthTest);
    }

    [TestMethod]
    public void WhenPipelineSetupReportsDriverInternalErrorThenCleanupFailuresAreIgnoredAndBothStatesAreRestored()
    {
        using FakeDeviceObject deviceObject = new();
        List<(Renderstatetype State, uint Value)> renderStates = [];
        int cleanupRestoreCount = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject.Device,
            (state, value) =>
            {
                renderStates.Add((state, value));
                if (state == Renderstatetype.Fillmode && value == (uint) Fillmode.Solid && renderStates.Count > 2)
                {
                    cleanupRestoreCount++;
                    return Direct3D9Factory.GenericFailureHResult;
                }
                if (state == Renderstatetype.Zfunc && value == (uint) Cmpfunc.Lessequal && renderStates.Count > 2)
                {
                    return Direct3D9Factory.InvalidCallHResult;
                }
                return 0;
            },
            setFlexibleVertexFormat: _ => Direct3D9Factory.DriverInternalErrorHResult);
        Assert.AreEqual(0, device.SetRenderState(Renderstatetype.Fillmode, (uint) Fillmode.Solid));
        Assert.AreEqual(0, device.SetRenderState(Renderstatetype.Zfunc, (uint) Cmpfunc.Lessequal));

        int result = device.DrawBox(default, Fillmode.Wireframe, 0);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
        Assert.AreEqual(1, cleanupRestoreCount);
        CollectionAssert.AreEqual(
            new[]
            {
                (Renderstatetype.Fillmode, (uint) Fillmode.Wireframe),
                (Renderstatetype.Zfunc, (uint) Cmpfunc.Always),
                (Renderstatetype.Fillmode, (uint) Fillmode.Solid),
                (Renderstatetype.Zfunc, (uint) Cmpfunc.Lessequal)
            },
            renderStates.Skip(2).ToArray());
    }

    [TestMethod]
    public void WhenFirstSuccessfulPathRestoreFailsThenCleanupRetriesItAndStillAttemptsDepthRestore()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        _indexBuffer = (nint) indexBufferObject.Buffer;
        List<(Renderstatetype State, uint Value)> renderStates = [];
        int fillRestoreCount = 0;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, (state, value) =>
        {
            renderStates.Add((state, value));
            if (state == Renderstatetype.Fillmode && value == (uint) Fillmode.Solid && renderStates.Count > 2)
            {
                fillRestoreCount++;
                return fillRestoreCount == 1
                    ? Direct3D9Factory.DriverInternalErrorHResult
                    : Direct3D9Factory.GenericFailureHResult;
            }
            if (state == Renderstatetype.Zfunc && value == (uint) Cmpfunc.Lessequal && renderStates.Count > 2)
            {
                return Direct3D9Factory.InvalidCallHResult;
            }
            return 0;
        });
        Assert.AreEqual(0, device.SetRenderState(Renderstatetype.Fillmode, (uint) Fillmode.Solid));
        Assert.AreEqual(0, device.SetRenderState(Renderstatetype.Zfunc, (uint) Cmpfunc.Lessequal));
        Assert.AreEqual(0, device.InitializeDynamicBuffers());

        int result = device.DrawBox(default, Fillmode.Wireframe, 0);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1),
            (result, device.UnusableReasonHResult, _drawCount));
        CollectionAssert.AreEqual(
            new[]
            {
                (Renderstatetype.Fillmode, (uint) Fillmode.Solid),
                (Renderstatetype.Fillmode, (uint) Fillmode.Solid),
                (Renderstatetype.Zfunc, (uint) Cmpfunc.Lessequal)
            },
            renderStates.TakeLast(3).ToArray());
    }

    [TestMethod]
    public void WhenDynamicBuffersAreNotInitializedThenAccessorsReturnNull()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);

        Assert.AreEqual((null, null), (device.Get3DVertexBuffer(), device.Get3DIndexBuffer()));
    }

    [TestMethod]
    public void WhenIndexBufferCreationFailsThenVertexBufferIsNotCreated()
    {
        using FakeDeviceObject deviceObject = new();
        _indexBufferCreateResult = Direct3D9Factory.GenericFailureHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);

        int result = device.InitializeDynamicBuffers();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Index", true, true, 0, 0),
            (result,
                string.Join(",", _bufferCreationOrder),
                device.Get3DVertexBuffer() is null,
                device.Get3DIndexBuffer() is null,
                _vertexBufferReleaseCount,
                _indexBufferReleaseCount));
    }

    [TestMethod]
    public void WhenVertexBufferCreationFailsThenCreatedIndexBufferIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject indexBufferObject = new();
        _indexBuffer = (nint) indexBufferObject.Buffer;
        _vertexBufferCreateResult = Direct3D9Factory.GenericFailureHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);

        int result = device.InitializeDynamicBuffers();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "Index,Vertex", true, true, 0, 1),
            (result,
                string.Join(",", _bufferCreationOrder),
                device.Get3DVertexBuffer() is null,
                device.Get3DIndexBuffer() is null,
                _vertexBufferReleaseCount,
                _indexBufferReleaseCount));
    }

    [TestMethod]
    public void WhenDynamicBufferInitializationIsRepeatedThenBuffersAreCreatedOnlyOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        _indexBuffer = (nint) indexBufferObject.Buffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);

        int firstResult = device.InitializeDynamicBuffers();
        Direct3D9HardwareVertexBuffer? vertexBuffer = device.Get3DVertexBuffer();
        Direct3D9HardwareIndexBuffer? indexBuffer = device.Get3DIndexBuffer();
        int secondResult = device.InitializeDynamicBuffers();

        Assert.AreEqual(
            (0, 0, "Index,Vertex", true, true, 0, 0),
            (firstResult,
                secondResult,
                string.Join(",", _bufferCreationOrder),
                ReferenceEquals(vertexBuffer, device.Get3DVertexBuffer()),
                ReferenceEquals(indexBuffer, device.Get3DIndexBuffer()),
                _vertexBufferReleaseCount,
                _indexBufferReleaseCount));
    }

    [TestMethod]
    public void WhenDynamicBuffersAreInitializedThenAccessorsReturnDeviceOwnedBuffers()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        _indexBuffer = (nint) indexBufferObject.Buffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);

        int result = device.InitializeDynamicBuffers();
        Direct3D9HardwareVertexBuffer? vertexBuffer = device.Get3DVertexBuffer();
        Direct3D9HardwareIndexBuffer? indexBuffer = device.Get3DIndexBuffer();

        Assert.AreEqual(
            (0, true, true, true, true),
            (result,
                vertexBuffer is not null,
                indexBuffer is not null,
                ReferenceEquals(vertexBuffer, device.Get3DVertexBuffer()),
                ReferenceEquals(indexBuffer, device.Get3DIndexBuffer())));
    }

    [TestMethod]
    public void WhenDeviceWithDynamicBuffersIsDisposedThenOwnedBuffersAreReleasedBeforeDeviceOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _device = (nint) deviceObject.Device;
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        _indexBuffer = (nint) indexBufferObject.Buffer;
        Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);
        Assert.AreEqual(0, device.InitializeDynamicBuffers());

        device.Dispose();
        device.Dispose();

        CollectionAssert.AreEqual(new[] { "Index", "Vertex", "Device" }, _releaseOrder);
        Assert.AreEqual((1, 1, 0), (_indexBufferReleaseCount, _vertexBufferReleaseCount, device.ResourceCount));
    }

    [TestMethod]
    public void WhenOnlyIndexBufferWasCreatedThenFailureAndDeviceCloseReleaseEachOwnerOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject indexBufferObject = new();
        _device = (nint) deviceObject.Device;
        _indexBuffer = (nint) indexBufferObject.Buffer;
        _vertexBufferCreateResult = Direct3D9Factory.GenericFailureHResult;
        Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);

        int result = device.InitializeDynamicBuffers();
        device.Dispose();
        device.Dispose();

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
        CollectionAssert.AreEqual(new[] { "Index", "Device" }, _releaseOrder);
        Assert.AreEqual((1, 0, 0), (_indexBufferReleaseCount, _vertexBufferReleaseCount, device.ResourceCount));
    }

    [TestMethod]
    public void WhenDynamicBufferAccessorsAreReadRepeatedlyThenTheyHaveNoDeviceOrComSideEffects()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeBufferObject vertexBufferObject = new();
        using FakeBufferObject indexBufferObject = new();
        _vertexBuffer = (nint) vertexBufferObject.Buffer;
        _indexBuffer = (nint) indexBufferObject.Buffer;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);
        Assert.AreEqual(0, device.InitializeDynamicBuffers());

        Direct3D9HardwareVertexBuffer? firstVertexBuffer = device.Get3DVertexBuffer();
        Direct3D9HardwareIndexBuffer? firstIndexBuffer = device.Get3DIndexBuffer();
        Direct3D9HardwareVertexBuffer? secondVertexBuffer = device.Get3DVertexBuffer();
        Direct3D9HardwareIndexBuffer? secondIndexBuffer = device.Get3DIndexBuffer();

        Assert.AreEqual(
            (true, true, false),
            (ReferenceEquals(firstVertexBuffer, secondVertexBuffer),
                ReferenceEquals(firstIndexBuffer, secondIndexBuffer),
                device.IsEntered()));
    }

    [TestMethod]
    public void WhenVertexBufferIsAccessedAfterDisposeThenThrows()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.Get3DVertexBuffer());
    }

    [TestMethod]
    public void WhenIndexBufferIsAccessedAfterDisposeThenThrows()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.Get3DIndexBuffer());
    }

    [TestMethod]
    public void WhenBoxIsDrawnAfterDisposeThenThrows()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, (_, _) => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.DrawBox(default, Fillmode.Solid, 0));
    }

    private static Direct3D9Device CreateDevice(
        IDirect3DDevice9* device,
        Direct3D9SetRenderState setRenderState,
        Direct3D9SetFlexibleVertexFormat? setFlexibleVertexFormat = null)
    {
        Caps9 capabilities = new() { MaxTextureBlendStages = 8 };
        return new Direct3D9Device(
            device,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: capabilities,
            setRenderState: setRenderState,
            setFlexibleVertexFormat: setFlexibleVertexFormat ?? (_ => 0),
            setPixelShader: _ => 0,
            setVertexShader: _ => 0,
            setTextureStageState: (_, _, _) => 0,
            setTexture: (_, _) => 0,
            setStreamSource: (_, _, _, _) => 0,
            setIndices: _ => 0);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void* self)
    {
        nint identity = (nint) self;
        if (identity == _vertexBuffer)
        {
            _vertexBufferReleaseCount++;
            _releaseOrder.Add("Vertex");
        }
        else if (identity == _indexBuffer)
        {
            _indexBufferReleaseCount++;
            _releaseOrder.Add("Index");
        }
        else if (identity == _device)
        {
            _releaseOrder.Add("Device");
        }

        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateVertexBuffer(IDirect3DDevice9* self, uint length, uint usage, uint fvf, Pool pool, IDirect3DVertexBuffer9** buffer, void** sharedHandle)
    {
        _bufferCreationOrder.Add("Vertex");
        *buffer = _vertexBufferCreateResult >= 0 ? (IDirect3DVertexBuffer9*) _vertexBuffer : null;
        return _vertexBufferCreateResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateIndexBuffer(IDirect3DDevice9* self, uint length, uint usage, Format format, Pool pool, IDirect3DIndexBuffer9** buffer, void** sharedHandle)
    {
        _bufferCreationOrder.Add("Index");
        *buffer = _indexBufferCreateResult >= 0 ? (IDirect3DIndexBuffer9*) _indexBuffer : null;
        return _indexBufferCreateResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockBuffer(void* self, uint offset, uint size, void** data, uint flags)
    {
        *data = null;
        return Direct3D9Factory.GenericFailureHResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int DrawIndexedPrimitiveUp(IDirect3DDevice9* self, Primitivetype primitiveType, uint minVertexIndex, uint vertexCount, uint primitiveCount, void* indexData, Format indexFormat, void* vertexData, uint vertexStride)
    {
        _drawCount++;
        _vertices = new ReadOnlySpan<Direct3D9VertexXyzDiffuseUv2>(vertexData, checked((int) vertexCount)).ToArray();
        _indices = new ReadOnlySpan<ushort>(indexData, checked((int) primitiveCount * 3)).ToArray();
        return 0;
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
            vtable[84] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Primitivetype, uint, uint, uint, void*, Format, void*, uint, int>) &DrawIndexedPrimitiveUp;
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
        internal void* Buffer;

        public FakeBufferObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 14);
            void** memory = (void**) _memory;
            Buffer = memory;
            void** vtable = memory + 1;
            *(void***) Buffer = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &Release;
            vtable[11] = (void*) (delegate* unmanaged[Stdcall]<void*, uint, uint, void**, uint, int>) &LockBuffer;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Buffer = null;
        }
    }
}
