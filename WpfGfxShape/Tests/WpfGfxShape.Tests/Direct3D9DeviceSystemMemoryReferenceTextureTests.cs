using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceSystemMemoryReferenceTextureTests
{
    private static uint _width;
    private static uint _height;
    private static uint _levels;
    private static uint _usage;
    private static Format _format;
    private static Pool _pool;
    private static nint _pixelReference;
    private static nint _textureToReturn;
    private static int _createResult;
    private static int _createCallCount;
    private static int _textureReleaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _width = 0;
        _height = 0;
        _levels = 0;
        _usage = 0;
        _format = 0;
        _pool = 0;
        _pixelReference = 0;
        _textureToReturn = 0;
        _createResult = 0;
        _createCallCount = 0;
        _textureReleaseCount = 0;
    }

    [TestMethod]
    public void WhenCreatingReferenceTextureThenArgumentsAndOwnershipMatchNativePath()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeTextureObject textureObject = new();
        _textureToReturn = (nint) textureObject.Texture;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        byte* pixels = stackalloc byte[128];
        SurfaceDesc description = new()
        {
            Width = 16,
            Height = 8,
            Usage = 0x200,
            Format = Format.A8R8G8B8,
            Pool = Pool.Systemmem
        };

        int result = device.TryCreateSystemMemoryReferenceTexture(description, pixels, out Direct3D9SystemMemoryReferenceTexture? texture);
        nint nativeTexture = (nint) texture!.Texture;
        texture.Dispose();

        Assert.AreEqual(
            (0, 16u, 8u, 1u, 0x200u, Format.A8R8G8B8, Pool.Systemmem, (nint) pixels, (nint) textureObject.Texture, 1, 1),
            (result, _width, _height, _levels, _usage, _format, _pool, _pixelReference, nativeTexture, _createCallCount, _textureReleaseCount));
    }

    [TestMethod]
    public void WhenReferenceTextureCreationFailsThenReturnedPointerIsReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeTextureObject textureObject = new();
        _textureToReturn = (nint) textureObject.Texture;
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateSystemMemoryReferenceTexture(default, (void*) 1, out Direct3D9SystemMemoryReferenceTexture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, null),
            (result, _createCallCount, _textureReleaseCount, texture));
    }

    [TestMethod]
    public void WhenReferenceTextureCreationReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeTextureObject textureObject = new();
        _textureToReturn = (nint) textureObject.Texture;
        _createResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateSystemMemoryReferenceTexture(default, (void*) 1, out Direct3D9SystemMemoryReferenceTexture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, 1, null),
            (result, device.UnusableReasonHResult, _textureReleaseCount, texture));
    }

    [TestMethod]
    public void WhenReferenceTextureCreationReturnsNonzeroSuccessThenResultAndOwnershipArePreserved()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeTextureObject textureObject = new();
        _textureToReturn = (nint) textureObject.Texture;
        _createResult = 1;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateSystemMemoryReferenceTexture(default, (void*) 1, out Direct3D9SystemMemoryReferenceTexture? texture);
        texture!.Dispose();

        Assert.AreEqual((1, 1, 1), (result, _createCallCount, _textureReleaseCount));
    }

    [TestMethod]
    public void WhenReferenceTextureCreationReturnsOrdinaryFailureThenDeviceRemainsUsable()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateSystemMemoryReferenceTexture(default, (void*) 1, out Direct3D9SystemMemoryReferenceTexture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, texture));
    }

    [TestMethod]
    public void WhenReferenceTextureCreationReturnsDeviceLostThenResultIsPreserved()
    {
        using FakeDeviceObject deviceObject = new();
        _createResult = Direct3D9Factory.DeviceLostHResult;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);

        int result = device.TryCreateSystemMemoryReferenceTexture(default, (void*) 1, out Direct3D9SystemMemoryReferenceTexture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.DeviceLostHResult, 0, 1, null),
            (result, device.UnusableReasonHResult, _createCallCount, texture));
    }

    [TestMethod]
    public void WhenCreatingReferenceTextureAfterDeviceLossWasProcessedThenNativeCreationStillRuns()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeTextureObject textureObject = new();
        _textureToReturn = (nint) textureObject.Texture;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.MarkUnusable();

        int result = device.TryCreateSystemMemoryReferenceTexture(default, (void*) 1, out Direct3D9SystemMemoryReferenceTexture? texture);
        texture!.Dispose();

        Assert.AreEqual((0, 1, 1), (result, _createCallCount, _textureReleaseCount));
    }

    [TestMethod]
    public void WhenCreatingReferenceTextureAfterDeviceWasReleasedThenNativeCreationIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.TryCreateSystemMemoryReferenceTexture(default, (void*) 1, out _));
        Assert.AreEqual(0, _createCallCount);
    }

    [TestMethod]
    public void WhenUsingReleasedReferenceTextureThenNativePointerAccessIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        using FakeTextureObject textureObject = new();
        _textureToReturn = (nint) textureObject.Texture;
        using Direct3D9Device device = CreateDevice(deviceObject.Device);
        device.TryCreateSystemMemoryReferenceTexture(default, (void*) 1, out Direct3D9SystemMemoryReferenceTexture? texture);
        texture!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = texture.Texture);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device)
    {
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self)
    {
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateTexture(
        IDirect3DDevice9* self,
        uint width,
        uint height,
        uint levels,
        uint usage,
        Format format,
        Pool pool,
        IDirect3DTexture9** texture,
        void** sharedHandle)
    {
        _createCallCount++;
        _width = width;
        _height = height;
        _levels = levels;
        _usage = usage;
        _format = format;
        _pool = pool;
        _pixelReference = sharedHandle is null ? 0 : (nint) (*sharedHandle);
        *texture = (IDirect3DTexture9*) _textureToReturn;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseTexture(IDirect3DTexture9* self)
    {
        _textureReleaseCount++;
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 26);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[23] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint, uint, uint, uint, Format, Pool, IDirect3DTexture9**, void**, int>) &CreateTexture;
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

        public FakeTextureObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            Texture = (IDirect3DTexture9*) memory;
            void** vtable = memory + 1;
            Texture->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &ReleaseTexture;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Texture = null;
        }
    }
}
