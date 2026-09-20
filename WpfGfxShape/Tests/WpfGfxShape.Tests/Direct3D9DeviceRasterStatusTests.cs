using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Core;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceRasterStatusTests
{
    private const uint ReadScanlineCapability = 0x00020000;
    private static uint _swapChainIndex;
    private static int _result;
    private static int _callCount;
    private static bool _writeRasterStatus;

    [TestInitialize]
    public void Initialize()
    {
        _swapChainIndex = 0;
        _result = 0;
        _callCount = 0;
        _writeRasterStatus = true;
    }

    [TestMethod]
    public void WhenReadScanlineCapabilityIsPresentThenGetScanLineIsSupported()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = default;
        capabilities.Caps = ReadScanlineCapability;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);

        Assert.IsTrue(device.SupportsGetScanLine);
    }

    [TestMethod]
    public void WhenReadScanlineCapabilityIsAbsentThenGetScanLineIsNotSupported()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);

        Assert.IsFalse(device.SupportsGetScanLine);
    }

    [TestMethod]
    public void WhenGettingRasterStatusThenNativeSlot19ReceivesSwapChainAndReturnsStatus()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);

        int result = device.GetRasterStatus(3, out RasterStatus rasterStatus);

        Assert.AreEqual(
            (0, 3u, true, 47u, 1),
            (result, _swapChainIndex, (bool) rasterStatus.InVBlank, rasterStatus.ScanLine, _callCount));
    }

    [TestMethod]
    public void WhenGettingRasterStatusWithoutCapabilityThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);

        int result = device.GetRasterStatus(uint.MaxValue, out _);

        Assert.AreEqual((0, uint.MaxValue, 1), (result, _swapChainIndex, _callCount));
    }

    [TestMethod]
    public void WhenGettingRasterStatusFailsThenHResultAndNativeOutputAreReturnedUnchanged()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        _result = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.GetRasterStatus(0, out RasterStatus rasterStatus);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, true, 47u, 0),
            (result, (bool) rasterStatus.InVBlank, rasterStatus.ScanLine, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenNativeFailureDoesNotWriteRasterStatusThenOutputRemainsDefault()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        _result = Direct3D9Factory.DriverInternalErrorHResult;
        _writeRasterStatus = false;

        int result = device.GetRasterStatus(0, out RasterStatus rasterStatus);

        Assert.AreEqual(
            (Direct3D9Factory.DriverInternalErrorHResult, false, 0u),
            (result, (bool) rasterStatus.InVBlank, rasterStatus.ScanLine));
    }

    [TestMethod]
    public void WhenRasterStatusDeviceWasMarkedUnusableThenNativeCallStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.MarkUnusable();

        int result = device.GetRasterStatus(0, out _);

        Assert.AreEqual((0, 1), (result, _callCount));
    }

    [TestMethod]
    public void WhenRasterStatusDeviceIsReleasedThenCallsAreRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = device.SupportsGetScanLine);
        Assert.ThrowsExactly<ObjectDisposedException>(() => device.GetRasterStatus(0, out _));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device, Caps9 capabilities)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default, capabilities: capabilities);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetRasterStatus(IDirect3DDevice9* self, uint swapChainIndex, RasterStatus* rasterStatus)
    {
        _callCount++;
        _swapChainIndex = swapChainIndex;
        if (_writeRasterStatus)
        {
            rasterStatus->InVBlank = new Bool32(true);
            rasterStatus->ScanLine = 47;
        }

        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 21);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[19] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, RasterStatus*, int>) &GetRasterStatus;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
