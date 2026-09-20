using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceTextureFormatSelectionTests
{
    [TestMethod]
    [DataRow((int) MilPixelFormat.Bgr24Bpp, false, (int) MilPixelFormat.Bgr32Bpp)]
    [DataRow((int) MilPixelFormat.Bgra32Bpp, false, (int) MilPixelFormat.Pbgra32Bpp)]
    [DataRow((int) MilPixelFormat.Bgr24Bpp, true, (int) MilPixelFormat.Pbgra32Bpp)]
    public void WhenDestinationUsesStandardPrecisionThenAlphaSelectsCachedBgrFormat(
        int bitmapSourceFormat,
        bool forceAlpha,
        int expectedTextureFormat)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, CreateFormatSupport());

        int result = device.GetSupportedTextureFormat(
            (MilPixelFormat) bitmapSourceFormat,
            MilPixelFormat.Bgr32Bpp,
            forceAlpha,
            out MilPixelFormat textureSourceFormat);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, (MilPixelFormat) expectedTextureFormat),
            (result, textureSourceFormat));
    }

    [TestMethod]
    [DataRow((int) MilPixelFormat.Rgb128BppFloat, false, (int) MilPixelFormat.Rgb128BppFloat)]
    [DataRow((int) MilPixelFormat.Rgb128BppFloat, true, (int) MilPixelFormat.Prgba128BppFloat)]
    [DataRow((int) MilPixelFormat.Bgr24Bpp, false, (int) MilPixelFormat.Bgr32Bpp101010)]
    [DataRow((int) MilPixelFormat.Bgra32Bpp, false, (int) MilPixelFormat.Prgba128BppFloat)]
    [DataRow((int) MilPixelFormat.Rgb48Bpp, false, (int) MilPixelFormat.Prgba128BppFloat)]
    public void WhenDestinationUsesHighPrecisionThenSourcePrecisionAndAlphaSelectCachedFormat(
        int bitmapSourceFormat,
        bool forceAlpha,
        int expectedTextureFormat)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, CreateFormatSupport());

        int result = device.GetSupportedTextureFormat(
            (MilPixelFormat) bitmapSourceFormat,
            MilPixelFormat.Bgr32Bpp101010,
            forceAlpha,
            out MilPixelFormat textureSourceFormat);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, (MilPixelFormat) expectedTextureFormat),
            (result, textureSourceFormat));
    }

    [TestMethod]
    [DataRow((int) MilPixelFormat.Bgr24Bpp, (int) MilPixelFormat.Bgr32Bpp, false, 1001)]
    [DataRow((int) MilPixelFormat.Bgra32Bpp, (int) MilPixelFormat.Bgr32Bpp, false, 1002)]
    [DataRow((int) MilPixelFormat.Bgr24Bpp, (int) MilPixelFormat.Bgr32Bpp101010, false, 1003)]
    [DataRow((int) MilPixelFormat.Rgb128BppFloat, (int) MilPixelFormat.Bgr32Bpp101010, false, 1004)]
    [DataRow((int) MilPixelFormat.Rgb128BppFloat, (int) MilPixelFormat.Bgr32Bpp101010, true, 1005)]
    public void WhenFormatIsSelectedThenExactCachedSupportFieldIsReturned(
        int bitmapSourceFormat,
        int destinationSurfaceFormat,
        bool forceAlpha,
        int expectedTextureFormat)
    {
        Direct3D9TextureFormatSupport formatSupport = new(
            SupportsA8: false,
            SupportsP8: false,
            SupportsL8: false,
            SupportFor128BppPrgbaFloat: (MilPixelFormat) 1005,
            SupportFor128BppRgbFloat: (MilPixelFormat) 1004,
            SupportFor32BppBgr101010: (MilPixelFormat) 1003,
            SupportFor32BppPbgra: (MilPixelFormat) 1002,
            SupportFor32BppBgr: (MilPixelFormat) 1001);
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, formatSupport);

        int result = device.GetSupportedTextureFormat(
            (MilPixelFormat) bitmapSourceFormat,
            (MilPixelFormat) destinationSurfaceFormat,
            forceAlpha,
            out MilPixelFormat textureSourceFormat);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, (MilPixelFormat) expectedTextureFormat),
            (result, textureSourceFormat));
    }

    [TestMethod]
    public void WhenSelectedFormatIsUnsupportedThenNativePixelFormatErrorIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device, default);

        int result = device.GetSupportedTextureFormat(
            MilPixelFormat.Bgra32Bpp,
            MilPixelFormat.Bgr32Bpp,
            false,
            out MilPixelFormat textureSourceFormat);

        Assert.AreEqual(
            (Direct3D9Factory.UnsupportedPixelFormatHResult, MilPixelFormat.Undefined),
            (result, textureSourceFormat));
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenTextureFormatSelectionIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, CreateFormatSupport());
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.GetSupportedTextureFormat(
            MilPixelFormat.Bgr32Bpp,
            MilPixelFormat.Bgr32Bpp,
            false,
            out _));
    }

    private static Direct3D9TextureFormatSupport CreateFormatSupport() => new(
        SupportsA8: true,
        SupportsP8: true,
        SupportsL8: true,
        SupportFor128BppPrgbaFloat: MilPixelFormat.Prgba128BppFloat,
        SupportFor128BppRgbFloat: MilPixelFormat.Rgb128BppFloat,
        SupportFor32BppBgr101010: MilPixelFormat.Bgr32Bpp101010,
        SupportFor32BppPbgra: MilPixelFormat.Pbgra32Bpp,
        SupportFor32BppBgr: MilPixelFormat.Bgr32Bpp);

    private static Direct3D9Device CreateDevice(
        IDirect3DDevice9* device,
        Direct3D9TextureFormatSupport textureFormatSupport)
    {
        Direct3D9Device result = new(device, null, 0, default, 0, default);
        result.UpdateTextureFormatSupport(textureFormatSupport);
        return result;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
