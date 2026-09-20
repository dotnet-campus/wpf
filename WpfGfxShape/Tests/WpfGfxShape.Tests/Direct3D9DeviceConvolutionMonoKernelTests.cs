using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceConvolutionMonoKernelTests
{
    private static uint _width;
    private static uint _height;
    private static nint _rows;
    private static nint _columns;
    private static int _result;
    private static int _callCount;

    [TestInitialize]
    public void Initialize()
    {
        _width = 0;
        _height = 0;
        _rows = -1;
        _columns = -1;
        _result = 0;
        _callCount = 0;
    }

    [TestMethod]
    public void WhenSettingConvolutionMonoKernelThenNativeSlot119ReceivesExactDimensionsAndNullArrays()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);

        int result = device.SetConvolutionMonoKernel(uint.MaxValue, 0x80000000u);

        Assert.AreEqual((0, uint.MaxValue, 0x80000000u, 0, 0, 1),
            (result, _width, _height, _rows, _columns, _callCount));
    }

    [DataTestMethod]
    [DataRow(1)]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenNativeResultIsNotDriverInternalErrorThenHResultIsPreserved(int nativeResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        _result = nativeResult;

        int result = device.SetConvolutionMonoKernel(3, 5);

        Assert.AreEqual((nativeResult, 0, 1), (result, device.UnusableReasonHResult, _callCount));
    }

    [TestMethod]
    public void WhenNativeResultIsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        _result = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.SetConvolutionMonoKernel(3, 5);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1),
            (result, device.UnusableReasonHResult, _callCount));
    }

    [TestMethod]
    public void WhenSettingKernelAfterDeviceLossWasProcessedThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        device.MarkUnusable();

        int result = device.SetConvolutionMonoKernel(7, 9);

        Assert.AreEqual((0, 1), (result, _callCount));
    }

    [TestMethod]
    public void WhenSettingKernelWithoutExtendedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, null);

        Assert.ThrowsExactly<InvalidOperationException>(() => device.SetConvolutionMonoKernel(3, 5));
        Assert.AreEqual(0, _callCount);
    }

    [TestMethod]
    public void WhenSettingKernelWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetConvolutionMonoKernel(3, 5));
        Assert.AreEqual(0, _callCount);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device, IDirect3DDevice9Ex* deviceEx)
    {
        return new Direct3D9Device(device, deviceEx, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDeviceEx(IDirect3DDevice9Ex* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetConvolutionMonoKernel(
        IDirect3DDevice9Ex* self,
        uint width,
        uint height,
        float* rows,
        float* columns)
    {
        _callCount++;
        _width = width;
        _height = height;
        _rows = (nint) rows;
        _columns = (nint) columns;
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _deviceMemory;
        private nint _deviceExMemory;
        internal IDirect3DDevice9* Device;
        internal IDirect3DDevice9Ex* DeviceEx;

        public FakeDeviceObject()
        {
            _deviceMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** deviceMemory = (void**) _deviceMemory;
            Device = (IDirect3DDevice9*) deviceMemory;
            Device->LpVtbl = deviceMemory + 1;
            Device->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;

            _deviceExMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 121);
            void** deviceExMemory = (void**) _deviceExMemory;
            DeviceEx = (IDirect3DDevice9Ex*) deviceExMemory;
            DeviceEx->LpVtbl = deviceExMemory + 1;
            DeviceEx->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, uint>) &ReleaseDeviceEx;
            DeviceEx->LpVtbl[119] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, uint, uint, float*, float*, int>) &SetConvolutionMonoKernel;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _deviceExMemory);
            NativeMemory.Free((void*) _deviceMemory);
            _deviceExMemory = 0;
            _deviceMemory = 0;
            DeviceEx = null;
            Device = null;
        }
    }
}
