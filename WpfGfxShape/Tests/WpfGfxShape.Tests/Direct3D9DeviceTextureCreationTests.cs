using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
public sealed unsafe class Direct3D9DeviceTextureCreationTests
{
    private static IDirect3DTexture9* _textureToReturn;
    private static uint _textureLevelCount;
    private static SurfaceDesc[] _textureLevelDescriptions = [];
    private static int _textureAddRefCount;
    private static int _textureReleaseCount;
    private static int _getTextureLevelDescriptionCallCount;
    private static int _getTextureLevelDescriptionResult;
    private static int _createTextureResult;
    private static (uint Width, uint Height, uint Levels, uint Usage, Format Format, Pool Pool)? _createTextureRequest;
    private static nint _sharedHandleInput;
    private static nint _sharedHandleOutput;
    private static bool _sharedHandleWasNull;

    [TestInitialize]
    public void Initialize()
    {
        _textureToReturn = null;
        _textureLevelCount = 1;
        _textureLevelDescriptions =
        [
            new SurfaceDesc(
                format: Format.A8R8G8B8,
                type: Resourcetype.Texture,
                pool: Pool.Default,
                width: 64,
                height: 32)
        ];
        _textureAddRefCount = 0;
        _textureReleaseCount = 0;
        _getTextureLevelDescriptionCallCount = 0;
        _getTextureLevelDescriptionResult = Direct3D9Factory.SuccessHResult;
        _createTextureResult = Direct3D9Factory.SuccessHResult;
        _createTextureRequest = null;
        _sharedHandleInput = 0;
        _sharedHandleOutput = 0;
        _sharedHandleWasNull = false;
    }

    [TestMethod]
    public void WhenCreatingTextureAfterDeviceLossWasProcessedThenDisplayStateInvalidIsReturnedWithoutNativeCall()
    {
        int callCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            createTexture: (_, _, _, _, _, _) =>
            {
                callCount++;
                return 0;
            });
        device.MarkUnusable();

        COMException exception = Assert.ThrowsExactly<COMException>(() => device.CreateTexture(16, 16));

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 0), (exception.ErrorCode, callCount));
    }

    [TestMethod]
    public void WhenCreatingBitmapTextureThenRequirementsArePassedDirectlyToDeviceCreation()
    {
        (uint Width, uint Height, uint Levels, uint Usage, Format Format, Pool Pool)? actual = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            createTexture: (width, height, levels, usage, format, pool) =>
            {
                actual = (width, height, levels, usage, format, pool);
                return Direct3D9Factory.SuccessHResult;
            });
        Direct3D9BitmapTextureRequirements requirements = new(
            new SurfaceDesc(
                format: Format.A2R10G10B10,
                type: Resourcetype.Texture,
                usage: D3D9.UsageAutogenmipmap | D3D9.UsageRendertarget,
                pool: Pool.Default,
                width: 320,
                height: 180),
            Levels: 0);

        int result = device.TryCreateBitmapTexture(requirements, isEvictable: false, out Direct3D9Texture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, (320u, 180u, 0u, D3D9.UsageAutogenmipmap | D3D9.UsageRendertarget, Format.A2R10G10B10, Pool.Default), null),
            (result, actual, texture));
    }

    [TestMethod]
    public void WhenBitmapTextureCreationFailsThenFirstErrorIsReturnedWithoutTexture()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            createTexture: (_, _, _, _, _, _) => Direct3D9Factory.InvalidCallHResult);
        Direct3D9BitmapTextureRequirements requirements = new(
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 16, height: 16),
            Levels: 1);

        int result = device.TryCreateBitmapTexture(requirements, isEvictable: true, out Direct3D9Texture? texture);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, null), (result, texture));
    }

    [TestMethod]
    public void WhenBitmapTextureCreationReturnsDriverInternalErrorThenDisplayStateInvalidIsReturned()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            createTexture: (_, _, _, _, _, _) => Direct3D9Factory.DriverInternalErrorHResult);
        Direct3D9BitmapTextureRequirements requirements = new(
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 16, height: 16),
            Levels: 1);

        int result = device.TryCreateBitmapTexture(requirements, isEvictable: false, out Direct3D9Texture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, null),
            (result, device.UnusableReasonHResult, texture));
    }

    [TestMethod]
    public void WhenCreatingBitmapTextureAfterDeviceLossThenDisplayStateInvalidIsReturnedWithoutNativeCall()
    {
        int callCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            createTexture: (_, _, _, _, _, _) =>
            {
                callCount++;
                return Direct3D9Factory.SuccessHResult;
            });
        device.MarkUnusable();
        Direct3D9BitmapTextureRequirements requirements = new(
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 16, height: 16),
            Levels: 1);

        int result = device.TryCreateBitmapTexture(requirements, isEvictable: false, out Direct3D9Texture? texture);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 0, null), (result, callCount, texture));
    }

    [TestMethod]
    [DataRow(0u, 1u)]
    [DataRow((uint) D3D9.UsageAutogenmipmap, 0u)]
    public void WhenCreatingLockableTextureThenMipLevelsFollowAutogenUsage(uint usage, uint expectedLevels)
    {
        uint? actualLevels = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            createTexture: (_, _, levels, _, _, _) =>
            {
                actualLevels = levels;
                return 0;
            });

        int result = device.TryCreateLockableTexture(16, 16, usage, Format.A8R8G8B8, Pool.Managed, out _);

        Assert.AreEqual((0, expectedLevels), (result, actualLevels));
    }

    [TestMethod]
    public void WhenCreatingLockableTextureThenNativeParametersUseNullSharedHandleAndWrapperOwnsReference()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _textureToReturn = textureObject.Texture;
        _textureLevelDescriptions =
        [
            new SurfaceDesc(format: Format.A2R10G10B10, type: Resourcetype.Texture, pool: Pool.Systemmem, width: 96, height: 48)
        ];
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);

        int result = device.TryCreateLockableTexture(
            96,
            48,
            D3D9.UsageDynamic,
            Format.A2R10G10B10,
            Pool.Systemmem,
            out Direct3D9Texture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, ((uint Width, uint Height, uint Levels, uint Usage, Format Format, Pool Pool)?) (96u, 48u, 1u, D3D9.UsageDynamic, Format.A2R10G10B10, Pool.Systemmem), true, 96u, 48u, 1, 1, 1),
            (result, _createTextureRequest, _sharedHandleWasNull, texture?.Width, texture?.Height,
                _textureAddRefCount, _textureReleaseCount, device.ResourceCount));
        texture?.Dispose();
    }

    [TestMethod]
    public void WhenLockableTextureNativeCreationFailsWithPointerThenPointerIsReleasedAndOutputIsEmpty()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _textureToReturn = textureObject.Texture;
        _createTextureResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);

        int result = device.TryCreateLockableTexture(16, 8, 0, Format.A8R8G8B8, Pool.Managed, out Direct3D9Texture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, true, 1, 0),
            (result, texture, _sharedHandleWasNull, _textureReleaseCount, device.ResourceCount));
    }

    [TestMethod]
    public void WhenLockableTextureWrapperCreationFailsThenNativeReferenceIsReleasedAndFirstErrorIsReturned()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _textureToReturn = textureObject.Texture;
        _textureLevelCount = 0;
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);

        int result = device.TryCreateLockableTexture(16, 8, 0, Format.A8R8G8B8, Pool.Managed, out Direct3D9Texture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, null, 0, 1, 0),
            (result, texture, _textureAddRefCount, _textureReleaseCount, device.ResourceCount));
    }

    [TestMethod]
    public void WhenLockableTextureLevelDescriptionFailsThenFirstErrorIsReturnedAndTemporaryReferenceIsReleased()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _textureToReturn = textureObject.Texture;
        _getTextureLevelDescriptionResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);

        int result = device.TryCreateLockableTexture(16, 8, 0, Format.A8R8G8B8, Pool.Managed, out Direct3D9Texture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, 1, 0, 1, 0, false),
            (result, texture, _getTextureLevelDescriptionCallCount, _textureAddRefCount,
                _textureReleaseCount, device.ResourceCount, device.IsEntered()));
    }

    [TestMethod]
    public void WhenLockableTextureCreationReturnsDriverInternalErrorThenDeviceBecomesUnusable()
    {
        using FakeDeviceObject deviceObject = new();
        _createTextureResult = Direct3D9Factory.DriverInternalErrorHResult;
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);

        int result = device.TryCreateLockableTexture(16, 8, 0, Format.A8R8G8B8, Pool.Managed, out Direct3D9Texture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DriverInternalErrorHResult, null),
            (result, device.UnusableReasonHResult, texture));
    }

    [TestMethod]
    public void WhenCreatingLockableTextureAfterDeviceLossWasProcessedThenNativeCreationStillRuns()
    {
        int callCount = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            default,
            createTexture: (_, _, _, _, _, _) =>
            {
                callCount++;
                return Direct3D9Factory.InvalidCallHResult;
            });
        device.MarkUnusable();

        int result = device.TryCreateLockableTexture(16, 16, 0, Format.A8R8G8B8, Pool.Managed, out _);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 1), (result, callCount));
    }

    [TestMethod]
    public void WhenCreatingLockableTextureAfterDeviceIsDisposedThenNativeCreationIsRejected()
    {
        using FakeDeviceObject deviceObject = new();
        Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() =>
            device.TryCreateLockableTexture(16, 8, 0, Format.A8R8G8B8, Pool.Managed, out _));

        Assert.IsNull(_createTextureRequest);
    }

    [TestMethod]
    public void WhenNativeTextureIsCreatedThenWrapperUsesActualLevelMetadataAndOwnsIndependentReference()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _textureToReturn = textureObject.Texture;
        _textureLevelCount = 3;
        _textureLevelDescriptions =
        [
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 64, height: 32),
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 32, height: 16),
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 16, height: 8)
        ];
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        Direct3D9BitmapTextureRequirements requirements = new(
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 320, height: 180),
            Levels: 0);

        int result = device.TryCreateBitmapTexture(requirements, isEvictable: false, out Direct3D9Texture? texture);

        Assert.AreEqual((0, 3u, 64u, 32u, 1, 1, 1),
            (result, texture?.LevelCount, texture?.Width, texture?.Height,
                _textureAddRefCount, _textureReleaseCount, device.ResourceCount));
        texture?.Dispose();
    }

    [TestMethod]
    public void WhenCreatingTextureWithSharedHandleThenParametersAndHandleArePassedDirectly()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _textureToReturn = textureObject.Texture;
        _sharedHandleOutput = (nint) 0x2468;
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        SurfaceDesc description = new(
            format: Format.A2R10G10B10,
            type: Resourcetype.Texture,
            usage: D3D9.UsageRendertarget,
            pool: Pool.Default,
            width: 320,
            height: 180);
        nint sharedHandle = (nint) 0x1357;

        int result = device.TryCreateTexture(description, 4, ref sharedHandle, out Direct3D9Texture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, ((uint Width, uint Height, uint Levels, uint Usage, Format Format, Pool Pool)?) (320u, 180u, 4u, D3D9.UsageRendertarget, Format.A2R10G10B10, Pool.Default), (nint) 0x1357, (nint) 0x2468, 1),
            (result, _createTextureRequest, _sharedHandleInput, sharedHandle, device.ResourceCount));
        texture?.Dispose();
    }

    [TestMethod]
    public void WhenSharedTextureCreationFailsWithTextureThenPointerIsReleasedAndOutputIsEmpty()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _textureToReturn = textureObject.Texture;
        _createTextureResult = Direct3D9Factory.InvalidCallHResult;
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        SurfaceDesc description = new(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 16, height: 8);
        nint sharedHandle = (nint) 0x1357;

        int result = device.TryCreateTexture(description, 1, ref sharedHandle, out Direct3D9Texture? texture);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, null, 1, 0),
            (result, texture, _textureReleaseCount, device.ResourceCount));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

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
        _createTextureRequest = (width, height, levels, usage, format, pool);
        *texture = _textureToReturn;
        _sharedHandleWasNull = sharedHandle is null;
        if (sharedHandle is not null)
        {
            _sharedHandleInput = (nint) (*sharedHandle);
            *sharedHandle = (void*) _sharedHandleOutput;
        }

        return _createTextureResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefTexture(IDirect3DTexture9* self)
    {
        return (uint) ++_textureAddRefCount;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseTexture(IDirect3DTexture9* self)
    {
        _textureReleaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint GetTextureLevelCount(IDirect3DTexture9* self) => _textureLevelCount;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetTextureLevelDescription(IDirect3DTexture9* self, uint level, SurfaceDesc* description)
    {
        _getTextureLevelDescriptionCallCount++;
        if (_getTextureLevelDescriptionResult < 0)
        {
            return _getTextureLevelDescriptionResult;
        }

        *description = _textureLevelDescriptions[level];
        return _getTextureLevelDescriptionResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 25);
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
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 19);
            void** memory = (void**) _memory;
            Texture = (IDirect3DTexture9*) memory;
            void** vtable = memory + 1;
            Texture->LpVtbl = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &AddRefTexture;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &ReleaseTexture;
            vtable[13] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &GetTextureLevelCount;
            vtable[17] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, SurfaceDesc*, int>) &GetTextureLevelDescription;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Texture = null;
        }
    }
}
