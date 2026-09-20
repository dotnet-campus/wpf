using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceInterfaceOwnershipTests
{
    private static readonly List<string> ReleaseOrder = [];

    [TestInitialize]
    public void Initialize() => ReleaseOrder.Clear();

    [TestMethod]
    public void WhenBaseDeviceIsDisposedThenOwnedInterfaceIsReleasedOnce()
    {
        using FakeDeviceObject baseDevice = new("base");
        Direct3D9Device device = CreateDevice(baseDevice.Device, null);

        device.Dispose();
        device.Dispose();

        Assert.AreEqual("base", string.Join(',', ReleaseOrder));
    }

    [TestMethod]
    public void WhenBaseAndExtendedDevicesAreDisposedThenInterfacesAreReleasedInNativeOrder()
    {
        using FakeDeviceObject baseDevice = new("base");
        using FakeDeviceObject extendedDevice = new("extended");
        Direct3D9Device device = CreateDevice(baseDevice.Device, extendedDevice.DeviceEx);

        device.Dispose();

        Assert.AreEqual("extended,base", string.Join(',', ReleaseOrder));
    }

    [TestMethod]
    public void WhenBaseAndExtendedInterfacesHaveSameAddressThenBothOwnedReferencesAreReleased()
    {
        using FakeDeviceObject deviceObject = new("device");
        Direct3D9Device device = CreateDevice(deviceObject.Device, deviceObject.DeviceEx);

        device.Dispose();

        Assert.AreEqual("device,device", string.Join(',', ReleaseOrder));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenInterfaceBackedAccessIsRejected()
    {
        using FakeDeviceObject baseDevice = new("base");
        Direct3D9Device device = CreateDevice(baseDevice.Device, null);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.GetRasterStatus(0, out _));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device, IDirect3DDevice9Ex* deviceEx)
    {
        return new Direct3D9Device(device, deviceEx, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void* self)
    {
        nint* memory = (nint*) self;
        GCHandle handle = GCHandle.FromIntPtr(memory[1]);
        ReleaseOrder.Add((string) handle.Target!);
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        private GCHandle _nameHandle;
        internal IDirect3DDevice9* Device;
        internal IDirect3DDevice9Ex* DeviceEx;

        internal FakeDeviceObject(string name)
        {
            _nameHandle = GCHandle.Alloc(name);
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            nint* memory = (nint*) _memory;
            Device = (IDirect3DDevice9*) memory;
            DeviceEx = (IDirect3DDevice9Ex*) memory;
            void** vtable = (void**) (memory + 2);
            memory[0] = (nint) vtable;
            memory[1] = GCHandle.ToIntPtr(_nameHandle);
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &Release;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            if (_nameHandle.IsAllocated)
            {
                _nameHandle.Free();
            }

            _memory = 0;
            Device = null;
            DeviceEx = null;
        }
    }
}
