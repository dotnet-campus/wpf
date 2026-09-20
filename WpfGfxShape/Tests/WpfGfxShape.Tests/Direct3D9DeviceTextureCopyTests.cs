using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceTextureCopyTests
{
    private static readonly List<string> Calls = [];
    private static nint _sourceSurface;
    private static nint _destinationSurface;
    private static int _sourceGetSurfaceLevelResult;
    private static int _destinationGetSurfaceLevelResult;
    private static int _stretchRectResult;
    private static uint _sourceLevel;
    private static uint _destinationLevel;
    private static bool _sourceRectangleWasNull;
    private static bool _destinationRectangleWasNull;
    private static Texturefiltertype _filter;

    [TestInitialize]
    public void Initialize()
    {
        Calls.Clear();
        _sourceSurface = 0;
        _destinationSurface = 0;
        _sourceGetSurfaceLevelResult = 0;
        _destinationGetSurfaceLevelResult = 0;
        _stretchRectResult = 0;
        _sourceLevel = uint.MaxValue;
        _destinationLevel = uint.MaxValue;
        _sourceRectangleWasNull = false;
        _destinationRectangleWasNull = false;
        _filter = Texturefiltertype.Point;
    }

    [TestMethod]
    public void WhenCopyingTextureThenLevelZeroSurfacesAreCopiedWithoutFilteringAndReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceSurfaceObject = new("ReleaseSourceSurface");
        using FakeSurfaceObject destinationSurfaceObject = new("ReleaseDestinationSurface");
        using FakeTextureObject sourceTextureObject = new(true, sourceSurfaceObject.Surface);
        using FakeTextureObject destinationTextureObject = new(false, destinationSurfaceObject.Surface);
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.CopyTexture(sourceTextureObject.Texture, destinationTextureObject.Texture);

        CollectionAssert.AreEqual(
            new[] { "GetSourceSurface", "GetDestinationSurface", "StretchRect", "ReleaseSourceSurface", "ReleaseDestinationSurface" },
            Calls);
        Assert.AreEqual(
            (0, 0u, 0u, true, true, Texturefiltertype.None),
            (result, _sourceLevel, _destinationLevel, _sourceRectangleWasNull, _destinationRectangleWasNull, _filter));
    }

    [TestMethod]
    public void WhenSourceSurfaceLookupFailsThenReturnedSurfaceIsReleasedAndCopyStops()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceSurfaceObject = new("ReleaseSourceSurface");
        using FakeTextureObject sourceTextureObject = new(true, sourceSurfaceObject.Surface);
        using FakeTextureObject destinationTextureObject = new(false, null);
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _sourceGetSurfaceLevelResult = Direct3D9Factory.InvalidCallHResult;

        int result = device.CopyTexture(sourceTextureObject.Texture, destinationTextureObject.Texture);

        CollectionAssert.AreEqual(new[] { "GetSourceSurface", "ReleaseSourceSurface" }, Calls);
        Assert.AreEqual(Direct3D9Factory.InvalidCallHResult, result);
    }

    [TestMethod]
    public void WhenSourceSurfaceLookupReturnsDriverInternalErrorThenTemporarySurfaceIsReleasedBeforeFailureIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceSurfaceObject = new("ReleaseSourceSurface");
        using FakeTextureObject sourceTextureObject = new(true, sourceSurfaceObject.Surface);
        using FakeTextureObject destinationTextureObject = new(false, null);
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _sourceGetSurfaceLevelResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.CopyTexture(sourceTextureObject.Texture, destinationTextureObject.Texture);

        CollectionAssert.AreEqual(new[] { "GetSourceSurface", "ReleaseSourceSurface" }, Calls);
        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    public void WhenDestinationSurfaceLookupFailsThenBothReturnedSurfacesAreReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceSurfaceObject = new("ReleaseSourceSurface");
        using FakeSurfaceObject destinationSurfaceObject = new("ReleaseDestinationSurface");
        using FakeTextureObject sourceTextureObject = new(true, sourceSurfaceObject.Surface);
        using FakeTextureObject destinationTextureObject = new(false, destinationSurfaceObject.Surface);
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _destinationGetSurfaceLevelResult = Direct3D9Factory.InvalidCallHResult;

        int result = device.CopyTexture(sourceTextureObject.Texture, destinationTextureObject.Texture);

        CollectionAssert.AreEqual(
            new[] { "GetSourceSurface", "GetDestinationSurface", "ReleaseSourceSurface", "ReleaseDestinationSurface" },
            Calls);
        Assert.AreEqual(Direct3D9Factory.InvalidCallHResult, result);
    }

    [TestMethod]
    public void WhenDestinationSurfaceLookupReturnsDriverInternalErrorThenTemporarySurfacesAreReleasedBeforeFailureIsMapped()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceSurfaceObject = new("ReleaseSourceSurface");
        using FakeSurfaceObject destinationSurfaceObject = new("ReleaseDestinationSurface");
        using FakeTextureObject sourceTextureObject = new(true, sourceSurfaceObject.Surface);
        using FakeTextureObject destinationTextureObject = new(false, destinationSurfaceObject.Surface);
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _destinationGetSurfaceLevelResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.CopyTexture(sourceTextureObject.Texture, destinationTextureObject.Texture);

        CollectionAssert.AreEqual(
            new[] { "GetSourceSurface", "GetDestinationSurface", "ReleaseSourceSurface", "ReleaseDestinationSurface" },
            Calls);
        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult),
            (result, device.UnusableReasonHResult));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(Direct3D9Factory.InvalidCallHResult)]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    public void WhenCopyReturnsThenResultIsPreservedAndTemporarySurfacesAreReleased(int expectedResult)
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceSurfaceObject = new("ReleaseSourceSurface");
        using FakeSurfaceObject destinationSurfaceObject = new("ReleaseDestinationSurface");
        using FakeTextureObject sourceTextureObject = new(true, sourceSurfaceObject.Surface);
        using FakeTextureObject destinationTextureObject = new(false, destinationSurfaceObject.Surface);
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _stretchRectResult = expectedResult;

        int result = device.CopyTexture(sourceTextureObject.Texture, destinationTextureObject.Texture);

        CollectionAssert.AreEqual(
            new[] { "GetSourceSurface", "GetDestinationSurface", "StretchRect", "ReleaseSourceSurface", "ReleaseDestinationSurface" },
            Calls);
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    public void WhenCopyReturnsDriverInternalErrorThenDeviceIsMarkedUnusableAndSurfacesAreReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceSurfaceObject = new("ReleaseSourceSurface");
        using FakeSurfaceObject destinationSurfaceObject = new("ReleaseDestinationSurface");
        using FakeTextureObject sourceTextureObject = new(true, sourceSurfaceObject.Surface);
        using FakeTextureObject destinationTextureObject = new(false, destinationSurfaceObject.Surface);
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        _stretchRectResult = Direct3D9Factory.DriverInternalErrorHResult;

        int result = device.CopyTexture(sourceTextureObject.Texture, destinationTextureObject.Texture);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 5),
            (result, device.UnusableReasonHResult, Calls.Count));
    }

    [TestMethod]
    public void WhenCopyingAfterDeviceLossWasProcessedThenNativeCallsStillRun()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeSurfaceObject sourceSurfaceObject = new("ReleaseSourceSurface");
        using FakeSurfaceObject destinationSurfaceObject = new("ReleaseDestinationSurface");
        using FakeTextureObject sourceTextureObject = new(true, sourceSurfaceObject.Surface);
        using FakeTextureObject destinationTextureObject = new(false, destinationSurfaceObject.Surface);
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.CopyTexture(sourceTextureObject.Texture, destinationTextureObject.Texture);

        Assert.AreEqual((0, 5), (result, Calls.Count));
    }

    [TestMethod]
    public void WhenCopyingWithReleasedDeviceThenCallIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.CopyTexture((IDirect3DTexture9*) 1, (IDirect3DTexture9*) 2));
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseSourceSurface(IDirect3DSurface9* self)
    {
        Calls.Add("ReleaseSourceSurface");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDestinationSurface(IDirect3DSurface9* self)
    {
        Calls.Add("ReleaseDestinationSurface");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseTexture(IDirect3DTexture9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSourceSurfaceLevel(IDirect3DTexture9* self, uint level, IDirect3DSurface9** surface)
    {
        Calls.Add("GetSourceSurface");
        _sourceLevel = level;
        *surface = (IDirect3DSurface9*) _sourceSurface;
        return _sourceGetSurfaceLevelResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetDestinationSurfaceLevel(IDirect3DTexture9* self, uint level, IDirect3DSurface9** surface)
    {
        Calls.Add("GetDestinationSurface");
        _destinationLevel = level;
        *surface = (IDirect3DSurface9*) _destinationSurface;
        return _destinationGetSurfaceLevelResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int StretchRect(
        IDirect3DDevice9* self,
        IDirect3DSurface9* source,
        Direct3D9SurfaceRect* sourceRectangle,
        IDirect3DSurface9* destination,
        Direct3D9SurfaceRect* destinationRectangle,
        Texturefiltertype filter)
    {
        Calls.Add("StretchRect");
        _sourceRectangleWasNull = sourceRectangle is null;
        _destinationRectangleWasNull = destinationRectangle is null;
        _filter = filter;
        return _stretchRectResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) (sizeof(void*) * 120 + sizeof(IDirect3DDevice9)));
            void** vtable = (void**) _memory;
            Device = (IDirect3DDevice9*) (vtable + 120);
            Device->LpVtbl = vtable;
            vtable[2] = (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[34] = (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int>) &StretchRect;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakeTextureObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DTexture9* Texture;

        public FakeTextureObject(bool source, IDirect3DSurface9* surface)
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) (sizeof(void*) * 22 + sizeof(IDirect3DTexture9)));
            void** vtable = (void**) _memory;
            Texture = (IDirect3DTexture9*) (vtable + 22);
            Texture->LpVtbl = vtable;
            vtable[2] = (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &ReleaseTexture;
            vtable[18] = source
                ? (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) &GetSourceSurfaceLevel
                : (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) &GetDestinationSurfaceLevel;
            if (source)
            {
                _sourceSurface = (nint) surface;
            }
            else
            {
                _destinationSurface = (nint) surface;
            }
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Texture = null;
        }
    }

    private struct FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        public FakeSurfaceObject(string releaseCall)
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) (sizeof(void*) * 3 + sizeof(IDirect3DSurface9)));
            void** vtable = (void**) _memory;
            Surface = (IDirect3DSurface9*) (vtable + 3);
            Surface->LpVtbl = vtable;
            vtable[2] = releaseCall == "ReleaseSourceSurface"
                ? (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseSourceSurface
                : (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &ReleaseDestinationSurface;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
