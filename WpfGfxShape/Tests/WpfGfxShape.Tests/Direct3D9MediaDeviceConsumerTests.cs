using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9MediaDeviceConsumerTests
{
    private static int _addRefCalls;
    private static int _releaseCalls;

    [TestInitialize]
    public void Initialize()
    {
        _addRefCalls = 0;
        _releaseCalls = 0;
    }

    [TestMethod]
    public void WhenConsumerIsInitializedThenBorrowedDeviceIsPassedOnceWithoutDeviceEntry()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        RecordingConsumer consumer = new();

        device.InitializeMediaDeviceConsumer(consumer);

        Assert.AreEqual(
            ((nint) deviceObject.Device, 1, 0, 0, false, false),
            (consumer.Device, consumer.CallCount, _addRefCalls, _releaseCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public void WhenExtendedDeviceConsumerIsInitializedThenBaseDeviceIdentityIsPassed()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);
        RecordingConsumer consumer = new();

        device.InitializeMediaDeviceConsumer(consumer);

        Assert.AreEqual(
            ((nint) deviceObject.Device, 1, 0, 0),
            (consumer.Device, consumer.CallCount, _addRefCalls, _releaseCalls));
    }

    [TestMethod]
    public void WhenConsumerRetainsDeviceThenConsumerOwnsReferenceCounting()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        RetainingConsumer consumer = new();

        device.InitializeMediaDeviceConsumer(consumer);
        consumer.Dispose();

        Assert.AreEqual((1, 1), (_addRefCalls, _releaseCalls));
    }

    [TestMethod]
    public void WhenConsumerReplacesAndGetsDeviceThenEveryOwnedReferenceIsIndependent()
    {
        using FakeDeviceObject previousDeviceObject = new();
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        RetainingConsumer consumer = new();

        consumer.SetDirect3DDevice9(previousDeviceObject.Device);
        device.InitializeMediaDeviceConsumer(consumer);
        device.InitializeMediaDeviceConsumer(consumer);
        IDirect3DDevice9* returnedDevice = consumer.GetDirect3DDevice9();
        _ = returnedDevice->Release();
        consumer.Dispose();

        Assert.AreEqual(
            ((nint) deviceObject.Device, 4, 4),
            ((nint) returnedDevice, _addRefCalls, _releaseCalls));
    }

    [TestMethod]
    public void WhenConsumerCallbackThrowsThenExceptionPropagatesWithoutDeviceStateChanges()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        ThrowingConsumer consumer = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => device.InitializeMediaDeviceConsumer(consumer));
        Assert.AreEqual(
            (1, 0, 0, false, false),
            (consumer.CallCount, _addRefCalls, _releaseCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public void WhenConsumerIsNullThenCallIsRejectedBeforeReadingDevice()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        Assert.ThrowsExactly<ArgumentNullException>(
            () => device.InitializeMediaDeviceConsumer(null!));
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenConsumerIsNotCalled()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        RecordingConsumer consumer = new();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.InitializeMediaDeviceConsumer(consumer));
        Assert.AreEqual(0, consumer.CallCount);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device, IDirect3DDevice9Ex* deviceEx = null)
    {
        return new Direct3D9Device(device, deviceEx, 0, Devtype.Hal, 0, default);
    }

    private sealed class RecordingConsumer : IDirect3D9MediaDeviceConsumer
    {
        internal nint Device { get; private set; }

        internal int CallCount { get; private set; }

        public void SetDirect3DDevice9(IDirect3DDevice9* device)
        {
            Device = (nint) device;
            CallCount++;
        }
    }

    private sealed class RetainingConsumer : IDirect3D9MediaDeviceConsumer, IDisposable
    {
        private IDirect3DDevice9* _device;

        public void SetDirect3DDevice9(IDirect3DDevice9* device)
        {
            if (_device is not null)
            {
                _ = _device->Release();
            }

            _device = device;
            _ = _device->AddRef();
        }

        internal IDirect3DDevice9* GetDirect3DDevice9()
        {
            _ = _device->AddRef();
            return _device;
        }

        public void Dispose()
        {
            if (_device is null)
            {
                return;
            }

            _ = _device->Release();
            _device = null;
        }
    }

    private sealed class ThrowingConsumer : IDirect3D9MediaDeviceConsumer
    {
        internal int CallCount { get; private set; }

        public void SetDirect3DDevice9(IDirect3DDevice9* device)
        {
            CallCount++;
            throw new InvalidOperationException();
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefDevice(IDirect3DDevice9* self)
    {
        _addRefCalls++;
        return (uint) _addRefCalls;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self)
    {
        _releaseCalls++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDeviceEx(IDirect3DDevice9Ex* self) => 0;

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        private nint _extendedMemory;
        internal IDirect3DDevice9* Device;
        internal IDirect3DDevice9Ex* DeviceEx;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &AddRefDevice;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;

            _extendedMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** extendedMemory = (void**) _extendedMemory;
            DeviceEx = (IDirect3DDevice9Ex*) extendedMemory;
            DeviceEx->LpVtbl = extendedMemory + 1;
            DeviceEx->LpVtbl[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9Ex*, uint>) &ReleaseDeviceEx;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _extendedMemory);
            NativeMemory.Free((void*) _memory);
            _extendedMemory = 0;
            _memory = 0;
            DeviceEx = null;
            Device = null;
        }
    }
}
