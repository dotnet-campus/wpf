using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceIndexStateTests
{
    private static nint _indexData;
    private static int _result;
    private static int _callCount;

    [TestInitialize]
    public void Initialize()
    {
        _indexData = 0;
        _result = 0;
        _callCount = 0;
    }

    [TestMethod]
    public void WhenSettingIndicesThenNativeSlot104ReceivesExactPointer()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DIndexBuffer9* indexData = (IDirect3DIndexBuffer9*) 0x1234;

        int result = device.SetIndices(indexData);

        Assert.AreEqual((0, (nint) indexData, 1), (result, _indexData, _callCount));
    }

    [TestMethod]
    public void WhenClearingIndicesThenNativeSlot104ReceivesNull()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetIndices(null);

        Assert.AreEqual((0, 0, 1), (result, _indexData, _callCount));
    }

    [TestMethod]
    public void WhenSettingSameIndicesRepeatedlyThenEachCallIsSubmitted()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DIndexBuffer9* indexData = (IDirect3DIndexBuffer9*) 0x1234;

        int firstResult = device.SetIndices(indexData);
        int secondResult = device.SetIndices(indexData);

        Assert.AreEqual((0, 0, 2), (firstResult, secondResult, _callCount));
    }

    [TestMethod]
    public void WhenSettingIndicesReturnsNonzeroSuccessThenEachCallPreservesOriginalHResult()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = 1;
        IDirect3DIndexBuffer9* indexData = (IDirect3DIndexBuffer9*) 0x1234;

        int firstResult = device.SetIndices(indexData);
        int secondResult = device.SetIndices(indexData);

        Assert.AreEqual((1, 1, 2, 0), (firstResult, secondResult, _callCount, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenSettingIndicesFailsThenOriginalHResultIsPreservedWithoutErrorMapping(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = failureHResult;

        int result = device.SetIndices((IDirect3DIndexBuffer9*) 0x1234);

        Assert.AreEqual((failureHResult, 1, 0), (result, _callCount, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingIndicesAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetIndices((IDirect3DIndexBuffer9*) 0x1234);

        Assert.AreEqual((0, 1), (result, _callCount));
    }

    [TestMethod]
    public void WhenSettingIndicesWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetIndices((IDirect3DIndexBuffer9*) 0x1234));
        Assert.AreEqual(0, _callCount);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetIndices(IDirect3DDevice9* self, IDirect3DIndexBuffer9* indexData)
    {
        _callCount++;
        _indexData = (nint) indexData;
        return _result;
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
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[104] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DIndexBuffer9*, int>) &SetIndices;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
