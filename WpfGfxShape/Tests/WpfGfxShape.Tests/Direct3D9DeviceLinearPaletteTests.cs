using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceLinearPaletteTests
{
    private static uint _paletteNumber;
    private static uint[] _paletteEntries = [];
    private static int _setPaletteEntriesResult;
    private static int _setCurrentTexturePaletteResult;
    private static int _setPaletteEntriesCallCount;
    private static int _setCurrentTexturePaletteCallCount;

    [TestInitialize]
    public void Initialize()
    {
        _paletteNumber = uint.MaxValue;
        _paletteEntries = [];
        _setPaletteEntriesResult = 0;
        _setCurrentTexturePaletteResult = 0;
        _setPaletteEntriesCallCount = 0;
        _setCurrentTexturePaletteCallCount = 0;
    }

    [TestMethod]
    public void WhenSettingLinearPaletteThenSlot71ReceivesAllLinearEntriesAndSlot73SelectsPaletteZero()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.SetLinearPalette();

        Assert.AreEqual(
            (0, 0u, 256, 0u, 0x01010101u, 0x7F7F7F7Fu, 0xFFFFFFFFu, 1, 1),
            (result, _paletteNumber, _paletteEntries.Length, _paletteEntries[0], _paletteEntries[1],
                _paletteEntries[127], _paletteEntries[255], _setPaletteEntriesCallCount,
                _setCurrentTexturePaletteCallCount));
    }

    [TestMethod]
    public void WhenSettingPaletteEntriesFailsThenCurrentPaletteIsNotSelected()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _setPaletteEntriesResult = unchecked((int) 0x80070057);

        int result = device.SetLinearPalette();

        Assert.AreEqual((_setPaletteEntriesResult, 1, 0),
            (result, _setPaletteEntriesCallCount, _setCurrentTexturePaletteCallCount));
    }

    [TestMethod]
    public void WhenSelectingCurrentPaletteFailsThenHResultIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _setCurrentTexturePaletteResult = unchecked((int) 0x80004005);

        int result = device.SetLinearPalette();

        Assert.AreEqual((_setCurrentTexturePaletteResult, 1, 1),
            (result, _setPaletteEntriesCallCount, _setCurrentTexturePaletteCallCount));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenSettingPaletteEntriesReturnsThenFailureShortCircuitsAndSuccessContinuesWithoutUnusableSideEffects(
        int paletteEntriesResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _setPaletteEntriesResult = paletteEntriesResult;

        int result = device.SetLinearPalette();

        int expectedResult = paletteEntriesResult < 0 ? paletteEntriesResult : 0;
        Assert.AreEqual(
            (expectedResult, 0, 1, paletteEntriesResult >= 0 ? 1 : 0),
            (result, device.UnusableReasonHResult, _setPaletteEntriesCallCount,
                _setCurrentTexturePaletteCallCount));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(Direct3D9Factory.GenericFailureHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenSelectingCurrentPaletteReturnsThenHResultIsPreservedWithoutUnusableSideEffects(int currentPaletteResult)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _setCurrentTexturePaletteResult = currentPaletteResult;

        int result = device.SetLinearPalette();

        Assert.AreEqual(
            (currentPaletteResult, 0, 1, 1),
            (result, device.UnusableReasonHResult, _setPaletteEntriesCallCount,
                _setCurrentTexturePaletteCallCount));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void WhenPaletteCallReportsDriverInternalErrorThenHandleDieMappingIsApplied(bool failSettingEntries)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        if (failSettingEntries)
        {
            _setPaletteEntriesResult = Direct3D9Factory.DriverInternalErrorHResult;
        }
        else
        {
            _setCurrentTexturePaletteResult = Direct3D9Factory.DriverInternalErrorHResult;
        }

        int result = device.SetLinearPalette();

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1,
                failSettingEntries ? 0 : 1),
            (result, device.UnusableReasonHResult, _setPaletteEntriesCallCount,
                _setCurrentTexturePaletteCallCount));
    }

    [TestMethod]
    public void WhenDeviceWasMarkedUnusableThenPaletteCallsStillRun()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.SetLinearPalette();

        Assert.AreEqual((0, 1, 1),
            (result, _setPaletteEntriesCallCount, _setCurrentTexturePaletteCallCount));
    }

    [TestMethod]
    [DataRow(true, true, true, (int) Direct3D9GlyphAlphaTextureFormat.A8)]
    [DataRow(false, true, true, (int) Direct3D9GlyphAlphaTextureFormat.L8)]
    [DataRow(false, false, true, (int) Direct3D9GlyphAlphaTextureFormat.P8)]
    public void WhenGlyphAlphaTextureFormatIsInitializedThenNativePreferenceOrderIsUsed(
        bool supportsA8,
        bool supportsL8,
        bool supportsP8,
        int expectedFormatValue)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.UpdateTextureFormatSupport(new Direct3D9TextureFormatSupport(
            supportsA8,
            supportsP8,
            supportsL8,
            default,
            default,
            default,
            default,
            default));
        Direct3D9GlyphAlphaTextureFormat expectedFormat = (Direct3D9GlyphAlphaTextureFormat) expectedFormatValue;

        int result = device.InitializeGlyphAlphaTextureFormat();

        Assert.AreEqual(
            (0, expectedFormat, expectedFormat == Direct3D9GlyphAlphaTextureFormat.P8 ? 1 : 0),
            (result, device.GlyphAlphaTextureFormat, _setPaletteEntriesCallCount));
    }

    [TestMethod]
    public void WhenGlyphAlphaTextureFormatsAreUnsupportedThenFailureLeavesFormatUndefined()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.InitializeGlyphAlphaTextureFormat();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9GlyphAlphaTextureFormat.Undefined),
            (result, device.GlyphAlphaTextureFormat));
    }

    [TestMethod]
    public void WhenGlyphP8PaletteInitializationFailsThenFailureLeavesFormatUndefined()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.UpdateTextureFormatSupport(new Direct3D9TextureFormatSupport(
            SupportsA8: false,
            SupportsP8: true,
            SupportsL8: false,
            default,
            default,
            default,
            default,
            default));
        _setPaletteEntriesResult = Direct3D9Factory.GenericFailureHResult;

        int result = device.InitializeGlyphAlphaTextureFormat();

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, Direct3D9GlyphAlphaTextureFormat.Undefined, 1, 0),
            (result, device.GlyphAlphaTextureFormat, _setPaletteEntriesCallCount, _setCurrentTexturePaletteCallCount));
    }

    [TestMethod]
    public void WhenDeviceIsReleasedThenPaletteCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetLinearPalette());
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetPaletteEntries(IDirect3DDevice9* self, uint paletteNumber, uint* entries)
    {
        _setPaletteEntriesCallCount++;
        _paletteNumber = paletteNumber;
        _paletteEntries = new uint[256];
        for (int index = 0; index < _paletteEntries.Length; index++)
        {
            _paletteEntries[index] = entries[index];
        }

        return _setPaletteEntriesResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetCurrentTexturePalette(IDirect3DDevice9* self, uint paletteNumber)
    {
        _setCurrentTexturePaletteCallCount++;
        _paletteNumber = paletteNumber;
        return _setCurrentTexturePaletteResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 76);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[71] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint*, int>) &SetPaletteEntries;
            vtable[73] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, int>) &SetCurrentTexturePalette;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }
}
