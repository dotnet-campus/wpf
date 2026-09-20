using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceShaderCreationTests
{
    private static nint _shaderFunction;
    private static nint _vertexShaderToReturn;
    private static nint _pixelShaderToReturn;
    private static int _createVertexShaderResult;
    private static int _createPixelShaderResult;
    private static int _createVertexShaderCallCount;
    private static int _createPixelShaderCallCount;
    private static int _vertexShaderReleaseCount;
    private static int _pixelShaderReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _shaderFunction = 0;
        _vertexShaderToReturn = 0;
        _pixelShaderToReturn = 0;
        _createVertexShaderResult = 0;
        _createPixelShaderResult = 0;
        _createVertexShaderCallCount = 0;
        _createPixelShaderCallCount = 0;
        _vertexShaderReleaseCount = 0;
        _pixelShaderReleaseCount = 0;
    }

    [TestMethod]
    public void WhenCreatingVertexShaderAfterDeviceLossWasProcessedThenSlot91CreationStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeVertexShaderObject shaderObject = new();
        _vertexShaderToReturn = (nint) shaderObject.VertexShader;
        uint shaderFunction = 0xFFFE0200;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.TryCreateVertexShader(&shaderFunction, out Direct3D9VertexShader? shader);
        nint nativeShader = (nint) shader!.VertexShader;
        shader.Dispose();

        Assert.AreEqual(
            (0, (nint) (void*) &shaderFunction, (nint) shaderObject.VertexShader, 1, 1),
            (result, _shaderFunction, nativeShader, _createVertexShaderCallCount, _vertexShaderReleaseCount));
    }

    [TestMethod]
    public void WhenCreatingPixelShaderAfterDeviceLossWasProcessedThenSlot106CreationStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakePixelShaderObject shaderObject = new();
        _pixelShaderToReturn = (nint) shaderObject.PixelShader;
        uint shaderFunction = 0xFFFF0200;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.TryCreatePixelShader(&shaderFunction, out Direct3D9PixelShader? shader);
        nint nativeShader = (nint) shader!.PixelShader;
        shader.Dispose();

        Assert.AreEqual(
            (0, (nint) (void*) &shaderFunction, (nint) shaderObject.PixelShader, 1, 1),
            (result, _shaderFunction, nativeShader, _createPixelShaderCallCount, _pixelShaderReleaseCount));
    }

    [TestMethod]
    public void WhenVertexShaderCreationReturnsNonzeroSuccessThenResultAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeVertexShaderObject shaderObject = new();
        _vertexShaderToReturn = (nint) shaderObject.VertexShader;
        _createVertexShaderResult = 1;
        uint shaderFunction = 0xFFFE0300;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateVertexShader(&shaderFunction, out Direct3D9VertexShader? shader);
        nint nativeShader = (nint) shader!.VertexShader;
        shader.Dispose();

        Assert.AreEqual(
            (1, (nint) (void*) &shaderFunction, (nint) shaderObject.VertexShader, 1, 1),
            (result, _shaderFunction, nativeShader, _createVertexShaderCallCount, _vertexShaderReleaseCount));
    }

    [TestMethod]
    public void WhenPixelShaderCreationReturnsNonzeroSuccessThenResultAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakePixelShaderObject shaderObject = new();
        _pixelShaderToReturn = (nint) shaderObject.PixelShader;
        _createPixelShaderResult = 1;
        uint shaderFunction = 0xFFFF0300;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreatePixelShader(&shaderFunction, out Direct3D9PixelShader? shader);
        nint nativeShader = (nint) shader!.PixelShader;
        shader.Dispose();

        Assert.AreEqual(
            (1, (nint) (void*) &shaderFunction, (nint) shaderObject.PixelShader, 1, 1),
            (result, _shaderFunction, nativeShader, _createPixelShaderCallCount, _pixelShaderReleaseCount));
    }

    [TestMethod]
    public void WhenVertexShaderCreationFailsWithPointerThenPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeVertexShaderObject shaderObject = new();
        _vertexShaderToReturn = (nint) shaderObject.VertexShader;
        _createVertexShaderResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateVertexShader(null, out Direct3D9VertexShader? shader);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, null),
            (result, _createVertexShaderCallCount, _vertexShaderReleaseCount, shader));
    }

    [TestMethod]
    public void WhenVertexShaderCreationFailsNormallyThenResultIsPreservedWithoutMarkingDeviceUnusable()
    {
        using FakeDeviceObject deviceObject = new();
        _createVertexShaderResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateVertexShader(null, out Direct3D9VertexShader? shader);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 0, null),
            (result, _createVertexShaderCallCount, device.UnusableReasonHResult, shader));
    }

    [TestMethod]
    public void WhenVertexShaderCreationReturnsDeviceLostThenOriginalResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        _createVertexShaderResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateVertexShader(null, out Direct3D9VertexShader? shader);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 1, 0, null),
            (result, _createVertexShaderCallCount, device.UnusableReasonHResult, shader));
    }

    [TestMethod]
    public void WhenPixelShaderCreationFailsWithPointerThenPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakePixelShaderObject shaderObject = new();
        _pixelShaderToReturn = (nint) shaderObject.PixelShader;
        _createPixelShaderResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreatePixelShader(null, out Direct3D9PixelShader? shader);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, null),
            (result, _createPixelShaderCallCount, _pixelShaderReleaseCount, shader));
    }

    [TestMethod]
    public void WhenPixelShaderCreationFailsNormallyThenResultIsPreservedWithoutMarkingDeviceUnusable()
    {
        using FakeDeviceObject deviceObject = new();
        _createPixelShaderResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreatePixelShader(null, out Direct3D9PixelShader? shader);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 0, null),
            (result, _createPixelShaderCallCount, device.UnusableReasonHResult, shader));
    }

    [TestMethod]
    public void WhenPixelShaderCreationReturnsDeviceLostThenOriginalResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        _createPixelShaderResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreatePixelShader(null, out Direct3D9PixelShader? shader);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 1, 0, null),
            (result, _createPixelShaderCallCount, device.UnusableReasonHResult, shader));
    }

    [TestMethod]
    public void WhenShaderCreationReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        _createVertexShaderResult = Direct3D9Factory.DriverInternalErrorHResult;
        _createPixelShaderResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int vertexResult = device.TryCreateVertexShader(null, out _);
        int pixelResult = device.TryCreatePixelShader(null, out _);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DisplayStateInvalidHResult,
                Direct3D9Factory.DriverInternalErrorHResult),
            (vertexResult, pixelResult, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenUsingReleasedShadersThenNativePointerAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeVertexShaderObject vertexShaderObject = new();
        using FakePixelShaderObject pixelShaderObject = new();
        _vertexShaderToReturn = (nint) vertexShaderObject.VertexShader;
        _pixelShaderToReturn = (nint) pixelShaderObject.PixelShader;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryCreateVertexShader(null, out Direct3D9VertexShader? vertexShader);
        device.TryCreatePixelShader(null, out Direct3D9PixelShader? pixelShader);
        vertexShader!.Dispose();
        pixelShader!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = vertexShader.VertexShader);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = pixelShader.PixelShader);
    }

    [TestMethod]
    public void WhenShadersAreDisposedRepeatedlyThenEachNativeIdentityIsReleasedOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeVertexShaderObject vertexShaderObject = new();
        using FakePixelShaderObject pixelShaderObject = new();
        _vertexShaderToReturn = (nint) vertexShaderObject.VertexShader;
        _pixelShaderToReturn = (nint) pixelShaderObject.PixelShader;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryCreateVertexShader(null, out Direct3D9VertexShader? vertexShader);
        device.TryCreatePixelShader(null, out Direct3D9PixelShader? pixelShader);

        vertexShader!.Dispose();
        vertexShader.Dispose();
        pixelShader!.Dispose();
        pixelShader.Dispose();

        Assert.AreEqual((1, 1), (_vertexShaderReleaseCount, _pixelShaderReleaseCount));
    }

    [TestMethod]
    public void WhenPixelShaderEffectIsDisposedThenManagedResourceAndShaderIdentityAreReleasedOnce()
    {
        using FakeDeviceObject deviceObject = new();
        using FakePixelShaderObject shaderObject = new();
        _pixelShaderToReturn = (nint) shaderObject.PixelShader;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9PixelShaderEffect.TryCreate(device, null, 37, out Direct3D9PixelShaderEffect? effect);

        effect!.Dispose();
        effect.Dispose();

        Assert.AreEqual((0, 1, 0u), (device.ResourceCount, _pixelShaderReleaseCount, device.ResourceManager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenManagerDestroysPixelShaderEffectThenDeviceIsNotOwnedAndMembersAreProtected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakePixelShaderObject shaderObject = new();
        _pixelShaderToReturn = (nint) shaderObject.PixelShader;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Direct3D9PixelShaderEffect.TryCreate(device, null, 37, out Direct3D9PixelShaderEffect? effect);

        device.ResourceManager.DestroyResource(effect!);
        effect.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = effect.PixelShader);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = effect.DeviceNoReference);
        Assert.AreEqual((0, 1, 0u), (device.ResourceCount, _pixelShaderReleaseCount, device.ResourceManager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenCreatingVertexShaderAfterDeviceWasDisposedThenNativeCreationIsNotCalled()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.TryCreateVertexShader(null, out _));
        Assert.AreEqual(0, _createVertexShaderCallCount);
    }

    [TestMethod]
    public void WhenCreatingPixelShaderAfterDeviceWasDisposedThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.TryCreatePixelShader(null, out _));
        Assert.AreEqual(0, _createPixelShaderCallCount);
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
    private static int CreateVertexShader(
        IDirect3DDevice9* self,
        uint* shaderFunction,
        IDirect3DVertexShader9** vertexShader)
    {
        _createVertexShaderCallCount++;
        _shaderFunction = (nint) shaderFunction;
        *vertexShader = (IDirect3DVertexShader9*) _vertexShaderToReturn;
        return _createVertexShaderResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreatePixelShader(
        IDirect3DDevice9* self,
        uint* shaderFunction,
        IDirect3DPixelShader9** pixelShader)
    {
        _createPixelShaderCallCount++;
        _shaderFunction = (nint) shaderFunction;
        *pixelShader = (IDirect3DPixelShader9*) _pixelShaderToReturn;
        return _createPixelShaderResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseVertexShader(IDirect3DVertexShader9* self)
    {
        _vertexShaderReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleasePixelShader(IDirect3DPixelShader9* self)
    {
        _pixelShaderReleaseCount++;
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 109);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[91] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint*, IDirect3DVertexShader9**, int>) &CreateVertexShader;
            vtable[106] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint*, IDirect3DPixelShader9**, int>) &CreatePixelShader;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeVertexShaderObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DVertexShader9* VertexShader;

        public FakeVertexShaderObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            VertexShader = (IDirect3DVertexShader9*) memory;
            void** vtable = memory + 1;
            VertexShader->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DVertexShader9*, uint>) &ReleaseVertexShader;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            VertexShader = null;
        }
    }

    private struct FakePixelShaderObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DPixelShader9* PixelShader;

        public FakePixelShaderObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            PixelShader = (IDirect3DPixelShader9*) memory;
            void** vtable = memory + 1;
            PixelShader->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DPixelShader9*, uint>) &ReleasePixelShader;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            PixelShader = null;
        }
    }
}
