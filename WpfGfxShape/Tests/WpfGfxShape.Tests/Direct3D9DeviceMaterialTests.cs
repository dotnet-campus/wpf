using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceMaterialTests
{
    private static Material9 _material;
    private static int _result;
    private static int _callCount;

    [TestInitialize]
    public void Initialize()
    {
        _material = default;
        _result = 0;
        _callCount = 0;
    }

    [TestMethod]
    public void WhenSettingMaterialThenNativeSlot49ReceivesExactValues()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        Material9 material = CreateMaterial();

        int result = device.SetMaterial(material);

        Assert.AreEqual(
            (0, 1, MaterialValues(material)),
            (result, _callCount, MaterialValues(_material)));
    }

    [TestMethod]
    public void WhenSettingMaterialReturnsNonzeroSuccessThenOriginalHResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = 1;

        int result = device.SetMaterial(CreateMaterial());

        Assert.AreEqual((1, 1, 0), (result, _callCount, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DriverInternalErrorHResult)]
    public void WhenSettingMaterialFailsThenOriginalHResultIsPreservedWithoutErrorMapping(int failureHResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _result = failureHResult;

        int result = device.SetMaterial(CreateMaterial());

        Assert.AreEqual((failureHResult, 1, 0), (result, _callCount, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenSettingMaterialWithReleasedDeviceThenNativeCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetMaterial(CreateMaterial()));
        Assert.AreEqual(0, _callCount);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    private static Material9 CreateMaterial()
    {
        return new Material9
        {
            Diffuse = new(1, 2, 3, 4),
            Ambient = new(5, 6, 7, 8),
            Specular = new(9, 10, 11, 12),
            Emissive = new(13, 14, 15, 16),
            Power = 17,
        };
    }

    private static (float, float, float, float, float, float, float, float, float, float, float, float, float, float, float, float, float) MaterialValues(Material9 material)
    {
        return (
            material.Diffuse.R, material.Diffuse.G, material.Diffuse.B, material.Diffuse.A,
            material.Ambient.R, material.Ambient.G, material.Ambient.B, material.Ambient.A,
            material.Specular.R, material.Specular.G, material.Specular.B, material.Specular.A,
            material.Emissive.R, material.Emissive.G, material.Emissive.B, material.Emissive.A,
            material.Power);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetMaterial(IDirect3DDevice9* self, Material9* material)
    {
        _callCount++;
        _material = *material;
        return _result;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 52);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[49] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, Material9*, int>) &SetMaterial;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
