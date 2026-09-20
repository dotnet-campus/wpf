using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceRenderTargetDataTests
{
    private static nint _source;
    private static nint _destination;
    private static int _result;
    private static int _callCount;

    [TestInitialize]
    public void Initialize()
    {
        _source = 0;
        _destination = 0;
        _result = 0;
        _callCount = 0;
    }

    [TestMethod]
    public void WhenGettingRenderTargetDataThenNativeSlot32ReceivesBorrowedSurfaces()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        IDirect3DSurface9* source = (IDirect3DSurface9*) 1;
        IDirect3DSurface9* destination = (IDirect3DSurface9*) 2;
        _result = Direct3D9Factory.InvalidCallHResult;

        int result = device.GetRenderTargetData(source, destination);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, (nint) source, (nint) destination, 1),
            (result, _source, _destination, _callCount));
    }

    [TestMethod]
    public void WhenGettingRenderTargetDataReturnsNonzeroSuccessThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = 1;

        int result = device.GetRenderTargetData((IDirect3DSurface9*) 1, (IDirect3DSurface9*) 2);

        Assert.AreEqual((1, 1), (result, _callCount));
    }

    [TestMethod]
    public void WhenGettingRenderTargetDataReturnsOrdinaryFailureThenOriginalHResultIsPreservedWithoutMarkingDeviceUnusable()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = Direct3D9Factory.GenericFailureHResult;

        int result = device.GetRenderTargetData((IDirect3DSurface9*) 1, (IDirect3DSurface9*) 2);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, 1),
            (result, device.UnusableReasonHResult, _callCount));
    }

    [TestMethod]
    public void WhenGettingRenderTargetDataReturnsDeviceLostThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = Direct3D9Factory.DeviceLostHResult;

        int result = device.GetRenderTargetData((IDirect3DSurface9*) 1, (IDirect3DSurface9*) 2);

        Assert.AreEqual(Direct3D9Factory.DisplayStateInvalidHResult, result);
    }

    [TestMethod]
    public void WhenGettingRenderTargetDataReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.GetRenderTargetData((IDirect3DSurface9*) 1, (IDirect3DSurface9*) 2);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenGettingRenderTargetDataAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.GetRenderTargetData((IDirect3DSurface9*) 1, (IDirect3DSurface9*) 2);

        Assert.AreEqual((0, 1), (result, _callCount));
    }

    [TestMethod]
    public void WhenGettingRenderTargetDataWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.GetRenderTargetData((IDirect3DSurface9*) 1, (IDirect3DSurface9*) 2));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetRenderTargetData(
        IDirect3DDevice9* self,
        IDirect3DSurface9* source,
        IDirect3DSurface9* destination)
    {
        _callCount++;
        _source = (nint) source;
        _destination = (nint) destination;
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 36);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[32] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, IDirect3DSurface9*, int>) &GetRenderTargetData;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
