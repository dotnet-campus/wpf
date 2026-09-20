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
public sealed unsafe class Direct3D9DeviceTransformTests
{
    private const Transformstatetype WorldTransform = (Transformstatetype) 256;

    private static Transformstatetype _state;
    private static Matrix4x4 _matrix;
    private static int _result;
    private static int _callCount;

    [TestInitialize]
    public void Initialize()
    {
        _state = default;
        _matrix = default;
        _result = 0;
        _callCount = 0;
    }

    [TestMethod]
    [DataRow(Transformstatetype.View)]
    [DataRow(WorldTransform)]
    public void WhenSettingTransformThenNativeSlot44ReceivesExactStateAndMatrix(Transformstatetype state)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Matrix4x4 matrix = CreateMatrix();

        int result = device.SetTransform(state, matrix);

        Assert.AreEqual((0, state, matrix, 1), (result, _state, _matrix, _callCount));
    }

    [TestMethod]
    public void WhenSettingKnownTransformAgainThenNativeCallIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Matrix4x4 matrix = CreateMatrix();

        int firstResult = device.SetTransform(Transformstatetype.Projection, matrix);
        int secondResult = device.SetTransform(Transformstatetype.Projection, matrix);

        Assert.AreEqual((0, 0, 1), (firstResult, secondResult, _callCount));
    }

    [TestMethod]
    public void WhenSettingTransformReturnsNonzeroSuccessThenStateIsCached()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Matrix4x4 matrix = CreateMatrix();
        _result = 1;

        int firstResult = device.SetTransform(Transformstatetype.View, matrix);
        int secondResult = device.SetTransform(Transformstatetype.View, matrix);

        Assert.AreEqual((1, 0, 1, 0), (firstResult, secondResult, _callCount, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenSettingTransformFailsThenOriginalHResultIsPreservedAndNextCallRetries(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Matrix4x4 matrix = CreateMatrix();
        _result = failureHResult;

        int firstResult = device.SetTransform(WorldTransform, matrix);
        _result = 0;
        int secondResult = device.SetTransform(WorldTransform, matrix);

        Assert.AreEqual((failureHResult, 0, 2, 0), (firstResult, secondResult, _callCount, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingTransformWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetTransform(Transformstatetype.View, CreateMatrix()));
        Assert.AreEqual(0, _callCount);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    private static Matrix4x4 CreateMatrix()
    {
        return new Matrix4x4(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetTransform(IDirect3DDevice9* self, Transformstatetype state, Matrix4x4* matrix)
    {
        _callCount++;
        _state = state;
        _matrix = *matrix;
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 47);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[44] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Transformstatetype, Matrix4x4*, int>) &SetTransform;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
