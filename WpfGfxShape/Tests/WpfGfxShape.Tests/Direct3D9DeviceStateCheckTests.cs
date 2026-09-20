using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceStateCheckTests
{
    [TestMethod]
    public void WhenExtendedCheckReportsModeChangedThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject,
            _ => Direct3D9Factory.PresentModeChangedHResult);

        Direct3D9DeviceState state = device.CheckDeviceState();

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9DeviceStateKind.Failure, 0),
            (state.HResult, state.Kind, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenExtendedCheckReportsOccludedThenSuccessIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject,
            _ => Direct3D9Factory.PresentOccludedHResult);

        Direct3D9DeviceState state = device.CheckDeviceState();

        Assert.AreEqual((0, Direct3D9DeviceStateKind.Operational), (state.HResult, state.Kind));
    }

    [TestMethod]
    public void WhenExtendedCheckReportsUnexpectedFailureThenResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(
            deviceObject,
            _ => Direct3D9Factory.GenericFailureHResult);

        Direct3D9DeviceState state = device.CheckDeviceState();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9DeviceStateKind.Failure, 0),
            (state.HResult, state.Kind, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DeviceHungHResult)]
    [DataRow(Direct3D9Factory.DeviceRemovedHResult)]
    public void WhenExtendedCheckReportsLostDeviceThenDeviceIsMarkedUnusable(int nativeResult)
    {
        using FakeDeviceObject deviceObject = new();
        int notificationCount = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject,
            _ => nativeResult,
            _ => notificationCount++);

        Direct3D9DeviceState state = device.CheckDeviceState();

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DisplayStateInvalidHResult, 1),
            (state.HResult, device.UnusableReasonHResult, notificationCount));
    }

    [TestMethod]
    public void WhenCheckingExtendedDeviceStateThenDestinationWindowIsForwarded()
    {
        using FakeDeviceObject deviceObject = new();
        nint receivedWindow = 0;
        using Direct3D9Device device = CreateDevice(
            deviceObject,
            window =>
            {
                receivedWindow = window;
                return 0;
            });

        Direct3D9DeviceState state = device.CheckDeviceState(42);

        Assert.AreEqual((0, (nint) 42, true), (state.HResult, receivedWindow, state.UsedExtendedCheck));
    }

    [TestMethod]
    public void WhenCheckingBaseDeviceStateThenNotImplementedIsReturnedWithoutNativeCall()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(
            deviceObject.Device,
            null,
            0,
            Devtype.Hal,
            0,
            default);

        Direct3D9DeviceState state = device.CheckDeviceState();

        Assert.AreEqual(
            (Direct3D9Factory.NotImplementedHResult, Direct3D9DeviceStateKind.Failure, false),
            (state.HResult, state.Kind, state.UsedExtendedCheck));
    }

    [TestMethod]
    public void WhenCheckingDisposedExtendedDeviceStateThenThrows()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject, _ => 0);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.CheckDeviceState());
    }

    private static Direct3D9Device CreateDevice(
        FakeDeviceObject deviceObject,
        Func<nint, int> checkDeviceState,
        Action<Direct3D9Device>? unusableNotification = null)
    {
        return new Direct3D9Device(
            deviceObject.Device,
            deviceObject.DeviceEx,
            0,
            Devtype.Hal,
            0,
            default,
            unusableNotification: unusableNotification,
            checkDeviceState: checkDeviceState);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDeviceEx(IDirect3DDevice9Ex* self) => 0;

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

            _deviceExMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** deviceExMemory = (void**) _deviceExMemory;
            DeviceEx = (IDirect3DDevice9Ex*) deviceExMemory;
            DeviceEx->LpVtbl = deviceExMemory + 1;
            DeviceEx->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, uint>) &ReleaseDeviceEx;
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
