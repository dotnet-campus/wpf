using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DevicePixelShaderStateTests
{
    private static readonly List<nint> ShaderPointers = [];
    private static int _result;
    private static int _shaderAddRefCount;
    private static int _shaderReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        ShaderPointers.Clear();
        _result = 0;
        _shaderAddRefCount = 0;
        _shaderReleaseCount = 0;
    }

    [TestMethod]
    public void WhenSettingPixelShaderThenNativeSlot107ReceivesExactPointer()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DPixelShader9* shader = (IDirect3DPixelShader9*) 0x1234;

        int result = device.SetPixelShader(shader);

        Assert.AreEqual((0, "4660"), (result, string.Join(',', ShaderPointers)));
    }

    [TestMethod]
    public void WhenSettingPixelShaderThenPointerIsBorrowedWithoutReferenceCountChanges()
    {
        using FakeDeviceObject deviceObject = new();
        using FakePixelShaderObject shaderObject = new();

        using (Direct3D9Device device = CreateDevice(deviceObject.Device))
        {
            _ = device.SetPixelShader(shaderObject.Shader);
        }

        Assert.AreEqual((0, 0), (_shaderAddRefCount, _shaderReleaseCount));
    }

    [TestMethod]
    public void WhenClearingPixelShaderThenNativeSlot107ReceivesNull()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetPixelShader(null);

        Assert.AreEqual((0, "0"), (result, string.Join(',', ShaderPointers)));
    }

    [TestMethod]
    public void WhenSettingMatchingPixelShaderAfterNonzeroSuccessThenNativeCallIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DPixelShader9* shader = (IDirect3DPixelShader9*) 0x1234;
        _result = 1;

        int firstResult = device.SetPixelShader(shader);
        int secondResult = device.SetPixelShader(shader);

        Assert.AreEqual((1, 0, 1, 0), (firstResult, secondResult, ShaderPointers.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenSettingPixelShaderFailsThenOriginalHResultIsPreservedAndMatchingPointerIsRetried(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DPixelShader9* shader = (IDirect3DPixelShader9*) 0x1234;
        _result = failureHResult;

        int firstResult = device.SetPixelShader(shader);
        int secondResult = device.SetPixelShader(shader);

        Assert.AreEqual(
            (failureHResult, failureHResult, 2, 0),
            (firstResult, secondResult, ShaderPointers.Count, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingPixelShaderAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetPixelShader((IDirect3DPixelShader9*) 0x1234);

        Assert.AreEqual((0, 1), (result, ShaderPointers.Count));
    }

    [TestMethod]
    public void WhenSettingPixelShaderWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetPixelShader((IDirect3DPixelShader9*) 0x1234));
        Assert.AreEqual(0, ShaderPointers.Count);
    }

    [TestMethod]
    public void WhenForcingPixelShaderWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.ForceSetPixelShader((IDirect3DPixelShader9*) 0x1234));
        Assert.AreEqual(0, ShaderPointers.Count);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetPixelShader(IDirect3DDevice9* self, IDirect3DPixelShader9* shader)
    {
        ShaderPointers.Add((nint) shader);
        return _result;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefShader(IDirect3DPixelShader9* self)
    {
        _shaderAddRefCount++;
        return 2;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseShader(IDirect3DPixelShader9* self)
    {
        _shaderReleaseCount++;
        return 1;
    }

    private struct FakePixelShaderObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DPixelShader9* Shader;

        public FakePixelShaderObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Shader = (IDirect3DPixelShader9*) memory;
            void** vtable = memory + 1;
            Shader->LpVtbl = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DPixelShader9*, uint>) &AddRefShader;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DPixelShader9*, uint>) &ReleaseShader;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Shader = null;
        }
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 112);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[107] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DPixelShader9*, int>) &SetPixelShader;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
