using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9MinimalTextureDescriptionTests
{
    private static nint _direct3DToReturn;
    private static int _getDirect3DResult;
    private static int _getDirect3DCallCount;
    private static int _direct3DReleaseCount;
    private static readonly List<Format> CheckedFormats = [];
    private static uint _adapterOrdinal;
    private static Devtype _deviceType;
    private static Format _adapterFormat;
    private static uint _usage;
    private static Resourcetype _resourceType;

    [TestInitialize]
    public void Initialize()
    {
        _direct3DToReturn = 0;
        _getDirect3DResult = 0;
        _getDirect3DCallCount = 0;
        _direct3DReleaseCount = 0;
        CheckedFormats.Clear();
        _adapterOrdinal = 0;
        _deviceType = 0;
        _adapterFormat = 0;
        _usage = 0;
        _resourceType = 0;
    }

    [TestMethod]
    public void WhenDimensionsExceedCapsThenTheyAreClampedAndSFalseIsReturnedWithoutFormatProbe()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = CreateCapabilities(maximumWidth: 64, maximumHeight: 32);
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        SurfaceDesc description = new(width: 80, height: 48);

        int result = device.GetMinimalTextureDescription(
            ref description,
            false,
            Direct3D9MinimalTextureDescriptionFlags.IgnoreFormat);

        Assert.AreEqual((1, 64u, 32u, 0),
            (result, description.Width, description.Height, _getDirect3DCallCount));
    }

    [TestMethod]
    public void WhenPowerOfTwoTexturesAreRequiredThenDimensionsRoundUp()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = CreateCapabilities(64, 64);
        capabilities.TextureCaps = (uint) D3D9.PtexturecapsPow2;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        SurfaceDesc description = new(width: 17, height: 31);

        int result = device.GetMinimalTextureDescription(
            ref description,
            false,
            Direct3D9MinimalTextureDescriptionFlags.IgnoreFormat);

        Assert.AreEqual((0, 32u, 32u), (result, description.Width, description.Height));
    }

    [TestMethod]
    public void WhenConditionalNonPowerOfTwoIsAllowedThenDimensionsArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        Caps9 capabilities = CreateCapabilities(64, 64);
        capabilities.TextureCaps =
            (uint) D3D9.PtexturecapsPow2 |
            (uint) D3D9.PtexturecapsNonpow2Conditional;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities);
        SurfaceDesc description = new(width: 17, height: 31);

        int result = device.GetMinimalTextureDescription(
            ref description,
            false,
            Direct3D9MinimalTextureDescriptionFlags.IgnoreFormat |
            Direct3D9MinimalTextureDescriptionFlags.NonPowerOfTwoConditionalAllowed);

        Assert.AreEqual((0, 17u, 31u), (result, description.Width, description.Height));
    }

    [TestMethod]
    public void WhenFormatNeedsUpgradeThenSlot6AndSlot10PreserveArgumentsAndReleaseDirect3D()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeDirect3DObject direct3DObject = new();
        _direct3DToReturn = (nint) direct3DObject.Direct3D;
        Caps9 capabilities = CreateCapabilities(64, 64);
        capabilities.AdapterOrdinal = 3;
        capabilities.DeviceType = Devtype.Ref;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities, Format.X8R8G8B8);
        SurfaceDesc description = new(
            format: Format.R5G6B5,
            type: Resourcetype.Texture,
            usage: 0x200,
            width: 16,
            height: 16);

        int result = device.GetMinimalTextureDescription(
            ref description,
            false,
            Direct3D9MinimalTextureDescriptionFlags.CheckFormat);

        Assert.AreEqual(
            (0, Format.R8G8B8, 1, 1, 3u, Devtype.Ref, Format.X8R8G8B8, 0x200u,
                Resourcetype.Texture, "R5G6B5,R8G8B8"),
            (result, description.Format, _getDirect3DCallCount, _direct3DReleaseCount,
                _adapterOrdinal, _deviceType, _adapterFormat, _usage, _resourceType,
                string.Join(',', CheckedFormats)));
    }

    [TestMethod]
    public void WhenAlphaPaletteIsUnsupportedThenP8ProbeIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeDirect3DObject direct3DObject = new();
        _direct3DToReturn = (nint) direct3DObject.Direct3D;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, CreateCapabilities(64, 64));
        SurfaceDesc description = new(format: Format.P8, type: Resourcetype.Texture, width: 8, height: 8);

        int result = device.GetMinimalTextureDescription(
            ref description,
            true,
            Direct3D9MinimalTextureDescriptionFlags.CheckFormat);

        Assert.AreEqual((0, Format.A8R8G8B8, "A8R8G8B8"),
            (result, description.Format, string.Join(',', CheckedFormats)));
    }

    [TestMethod]
    public void WhenGetDirect3DFailsThenAdjustedDimensionsAndOriginalFormatArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        _getDirect3DResult = Direct3D9Factory.InvalidCallHResult;
        Caps9 capabilities = CreateCapabilities(64, 32);
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities, Format.X8R8G8B8);
        SurfaceDesc description = new(
            format: Format.R5G6B5,
            type: Resourcetype.Texture,
            usage: 0x200,
            width: 80,
            height: 48);

        int result = device.GetMinimalTextureDescription(
            ref description,
            true,
            Direct3D9MinimalTextureDescriptionFlags.CheckAll);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 64u, 32u, Format.R5G6B5, 1, 0),
            (result, description.Width, description.Height, description.Format,
                _getDirect3DCallCount, _direct3DReleaseCount));
    }

    [TestMethod]
    public void WhenGetDirect3DReturnsNonzeroSuccessThenCachedArgumentsAreForwardedAndReferenceIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeDirect3DObject direct3DObject = new();
        _direct3DToReturn = (nint) direct3DObject.Direct3D;
        _getDirect3DResult = 1;
        Caps9 capabilities = CreateCapabilities(64, 64);
        capabilities.AdapterOrdinal = uint.MaxValue;
        capabilities.DeviceType = Devtype.SW;
        using Direct3D9Device device = CreateDevice(deviceObject.Device, capabilities, Format.A2R10G10B10);
        SurfaceDesc description = new(
            format: Format.R8G8B8,
            type: Resourcetype.Texture,
            usage: uint.MaxValue,
            width: 16,
            height: 16);

        int result = device.GetMinimalTextureDescription(
            ref description,
            false,
            Direct3D9MinimalTextureDescriptionFlags.CheckFormat);

        Assert.AreEqual(
            (0, Format.R8G8B8, 1, 1, uint.MaxValue, Devtype.SW, Format.A2R10G10B10,
                uint.MaxValue, Resourcetype.Texture),
            (result, description.Format, _getDirect3DCallCount, _direct3DReleaseCount,
                _adapterOrdinal, _deviceType, _adapterFormat, _usage, _resourceType));
    }

    [TestMethod]
    public void WhenDeviceIsDisposedThenMinimalDescriptionCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device, CreateCapabilities(64, 64));
        device.Dispose();
        SurfaceDesc description = default;

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.GetMinimalTextureDescription(
            ref description,
            false,
            Direct3D9MinimalTextureDescriptionFlags.CheckAll));
    }

    private static Caps9 CreateCapabilities(uint maximumWidth, uint maximumHeight)
    {
        Caps9 capabilities = default;
        capabilities.MaxTextureWidth = maximumWidth;
        capabilities.MaxTextureHeight = maximumHeight;
        return capabilities;
    }

    private static Direct3D9Device CreateDevice(
        IDirect3DDevice9* nativeDevice,
        Caps9 capabilities,
        Format adapterFormat = Format.A8R8G8B8)
    {
        return new Direct3D9Device(
            nativeDevice,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            capabilities: capabilities,
            displayMode: new Displaymode(format: adapterFormat));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetDirect3D(IDirect3DDevice9* self, IDirect3D9** direct3D)
    {
        _getDirect3DCallCount++;
        *direct3D = (IDirect3D9*) _direct3DToReturn;
        return _getDirect3DResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDirect3D(IDirect3D9* self)
    {
        _direct3DReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CheckDeviceFormat(
        IDirect3D9* self,
        uint adapterOrdinal,
        Devtype deviceType,
        Format adapterFormat,
        uint usage,
        Resourcetype resourceType,
        Format checkedFormat)
    {
        _adapterOrdinal = adapterOrdinal;
        _deviceType = deviceType;
        _adapterFormat = adapterFormat;
        _usage = usage;
        _resourceType = resourceType;
        CheckedFormats.Add(checkedFormat);
        return checkedFormat is Format.R8G8B8 or Format.A8R8G8B8 ? 0 : Direct3D9Factory.InvalidCallHResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 8);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[6] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3D9**, int>) &GetDirect3D;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeDirect3DObject : IDisposable
    {
        private nint _memory;
        internal IDirect3D9* Direct3D;

        public FakeDirect3DObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 12);
            void** memory = (void**) _memory;
            Direct3D = (IDirect3D9*) memory;
            void** vtable = memory + 1;
            Direct3D->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3D9*, uint>) &ReleaseDirect3D;
            vtable[10] = (void*) (delegate* unmanaged[Stdcall]<IDirect3D9*, uint, Devtype, Format, uint, Resourcetype, Format, int>) &CheckDeviceFormat;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Direct3D = null;
        }
    }
}
