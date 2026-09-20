using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9SurfaceTests
{
    private static int _addReferenceCount;
    private static int _releaseCount;
    private static int _getDeviceCount;
    private static int _releaseDeviceCount;
    private static int _getDeviceResult;
    private static nint _device;
    private static int _lockCount;
    private static int _unlockCount;
    private static int _lockResult;
    private static int _unlockResult;
    private static LockedRect _lockedRect;
    private static Direct3D9SurfaceRect _lockRectangle;
    private static uint _lockFlags;
    private static int _getDeviceContextCount;
    private static int _releaseDeviceContextCount;
    private static int _getDeviceContextResult;
    private static int _releaseDeviceContextResult;
    private static nint _deviceContext;
    private static nint _releasedDeviceContext;
    private static int _getDescriptionCount;
    private static int _getDescriptionResult;
    private static SurfaceDesc _description;

    [TestInitialize]
    public void Initialize()
    {
        _addReferenceCount = 0;
        _releaseCount = 0;
        _getDeviceCount = 0;
        _releaseDeviceCount = 0;
        _getDeviceResult = Direct3D9Factory.SuccessHResult;
        _device = 42;
        _lockCount = 0;
        _unlockCount = 0;
        _lockResult = Direct3D9Factory.SuccessHResult;
        _unlockResult = Direct3D9Factory.SuccessHResult;
        _lockedRect = new LockedRect(64, (void*) 42);
        _lockRectangle = default;
        _lockFlags = 0;
        _getDeviceContextCount = 0;
        _releaseDeviceContextCount = 0;
        _getDeviceContextResult = Direct3D9Factory.SuccessHResult;
        _releaseDeviceContextResult = Direct3D9Factory.SuccessHResult;
        _deviceContext = 42;
        _releasedDeviceContext = 0;
        _getDescriptionCount = 0;
        _getDescriptionResult = 0;
        _description = new SurfaceDesc(
            format: Format.A8R8G8B8,
            type: Resourcetype.Surface,
            pool: Pool.Default,
            width: 320,
            height: 240);
    }
    [TestMethod]
    public void WhenRenderTargetFormatWasNotTestedThenTestResultCannotBeRead()
    {
        Direct3D9TargetFormatTestStatus status = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => _ = status.TestHResult);
    }

    [TestMethod]
    public void WhenRenderTargetFormatTestSucceedsThenSuccessfulResultIsAvailable()
    {
        Direct3D9TargetFormatTestStatus status = new();

        _ = status.TestRenderTargetFormat(_ => Direct3D9Factory.SuccessHResult);

        Assert.AreEqual((true, Direct3D9Factory.SuccessHResult), (status.WasTested, status.TestHResult));
    }

    [TestMethod]
    public void WhenRenderTargetFormatTestFailsThenFailedResultIsAvailable()
    {
        Direct3D9TargetFormatTestStatus status = new();

        _ = status.TestRenderTargetFormat(_ => Direct3D9Factory.InvalidCallHResult);

        Assert.AreEqual((true, Direct3D9Factory.InvalidCallHResult), (status.WasTested, status.TestHResult));
    }

    [TestMethod]
    public void WhenGetDeviceContextSucceedsThenResultIsStoredAndHandleIsReleased()
    {
        Direct3D9TargetFormatTestStatus status = new();
        nint releasedDeviceContext = 0;

        int result = status.TestGetDeviceContext(
            () => new Direct3D9SurfaceDeviceContextResult(0, 42),
            deviceContext =>
            {
                releasedDeviceContext = deviceContext;
                return 0;
            });

        Assert.AreEqual((0, 42, true), (result, (int) releasedDeviceContext, status.WasGetDeviceContextTested));
    }

    [TestMethod]
    public void WhenGetDeviceContextReturnsNonzeroSuccessThenResultIsCachedAndReleaseFailureIsIgnored()
    {
        Direct3D9TargetFormatTestStatus status = new();
        int getCount = 0;
        int releaseCount = 0;

        int first = status.TestGetDeviceContext(
            () =>
            {
                getCount++;
                return new Direct3D9SurfaceDeviceContextResult(1, 42);
            },
            _ =>
            {
                releaseCount++;
                return Direct3D9Factory.DriverInternalErrorHResult;
            });
        int second = status.TestGetDeviceContext(
            () =>
            {
                getCount++;
                return new Direct3D9SurfaceDeviceContextResult(0, 0);
            },
            _ => 0);

        Assert.AreEqual((1, 1, 1, 1, 1),
            (first, second, status.GetDeviceContextHResult, getCount, releaseCount));
    }

    [TestMethod]
    public void WhenGetDeviceContextFailsWithHandleThenHandleIsStillReleased()
    {
        Direct3D9TargetFormatTestStatus status = new();
        int releaseCount = 0;

        int result = status.TestGetDeviceContext(
            () => new Direct3D9SurfaceDeviceContextResult(Direct3D9Factory.DriverInternalErrorHResult, 42),
            _ =>
            {
                releaseCount++;
                return Direct3D9Factory.InvalidCallHResult;
            });

        Assert.AreEqual((Direct3D9Factory.DriverInternalErrorHResult, 1), (result, releaseCount));
    }

    [TestMethod]
    public void WhenGetDeviceContextReturnsNoHandleThenReleaseIsNotCalled()
    {
        Direct3D9TargetFormatTestStatus status = new();
        int releaseCount = 0;

        status.TestGetDeviceContext(
            () => new Direct3D9SurfaceDeviceContextResult(Direct3D9Factory.InvalidCallHResult, 0),
            _ =>
            {
                releaseCount++;
                return 0;
            });

        Assert.AreEqual(0, releaseCount);
    }

    [TestMethod]
    public void WhenTestingGetDeviceContextTwiceThenFirstResultIsReused()
    {
        Direct3D9TargetFormatTestStatus status = new();
        int getCount = 0;

        int first = status.TestGetDeviceContext(
            () =>
            {
                getCount++;
                return new Direct3D9SurfaceDeviceContextResult(Direct3D9Factory.InvalidCallHResult, 0);
            },
            _ => 0);
        int second = status.TestGetDeviceContext(
            () =>
            {
                getCount++;
                return new Direct3D9SurfaceDeviceContextResult(0, 0);
            },
            _ => 0);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.InvalidCallHResult, 1), (first, second, getCount));
    }

    [TestMethod]
    public void WhenReinterpretingGetDeviceContextFailureThenReinterpretedResultIsStored()
    {
        Direct3D9TargetFormatTestStatus status = new();

        int result = status.TestGetDeviceContext(
            () => new Direct3D9SurfaceDeviceContextResult(Direct3D9Factory.GenericFailureHResult, 0),
            _ => 0,
            hResult => hResult == Direct3D9Factory.GenericFailureHResult
                ? Direct3D9Factory.OutOfMemoryHResult
                : hResult);

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, Direct3D9Factory.OutOfMemoryHResult), (result, status.GetDeviceContextHResult));
    }

    [TestMethod]
    public void WhenGenericGetDeviceContextFailureOccursNearGdiQuotaThenOutOfMemoryIsReturned()
    {
        int result = Direct3D9GuiHandleQuota.ReinterpretGetDeviceContextFailure(
            Direct3D9Factory.GenericFailureHResult,
            () => 88,
            () => 100);

        Assert.AreEqual(Direct3D9Factory.OutOfMemoryHResult, result);
    }

    [TestMethod]
    public void WhenGenericGetDeviceContextFailureOccursBelowGdiQuotaThenDriverInternalErrorIsReturned()
    {
        int result = Direct3D9GuiHandleQuota.ReinterpretGetDeviceContextFailure(
            Direct3D9Factory.GenericFailureHResult,
            () => 86,
            () => 100);

        Assert.AreEqual(Direct3D9Factory.DriverInternalErrorHResult, result);
    }

    [TestMethod]
    public void WhenGetDeviceContextFailureIsNotGenericThenQuotaIsNotQueried()
    {
        int result = Direct3D9GuiHandleQuota.ReinterpretGetDeviceContextFailure(
            Direct3D9Factory.InvalidCallHResult,
            () => throw new InvalidOperationException(),
            () => throw new InvalidOperationException());

        Assert.AreEqual(Direct3D9Factory.InvalidCallHResult, result);
    }

    [TestMethod]
    public void WhenGdiQuotaIsZeroThenDefaultQuotaSafetyMarginIsUsed()
    {
        Assert.AreEqual(8750u, Direct3D9GuiHandleQuota.CalculateTestBar(0));
    }

    [TestMethod]
    public void WhenReadingUntestedGetDeviceContextStatusThenThrowsInvalidOperationException()
    {
        Direct3D9TargetFormatTestStatus status = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => _ = status.GetDeviceContextHResult);
    }

    [TestMethod]
    public void WhenSurfaceIsCreatedThenDescriptionDimensionsAreReadOnceAndCached()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9ResourceManager manager = new();

        int result = Direct3D9Surface.TryCreate(manager, surfaceObject.Surface, out Direct3D9Surface? surface);
        SurfaceDesc firstDescription = surface!.GetDescription();
        _description = new SurfaceDesc(width: 1, height: 1);
        SurfaceDesc secondDescription = surface.GetDescription();

        Assert.AreEqual(
            (0, 1, 320u, 240u, firstDescription, 1),
            (result, _getDescriptionCount, firstDescription.Width, firstDescription.Height, secondDescription, manager.ResourceCount));
        surface.Dispose();
    }

    [TestMethod]
    public void WhenSurfaceDescriptionInitializationFailsThenNoDimensionsOrRegistrationAreExposed()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9ResourceManager manager = new();
        _getDescriptionResult = Direct3D9Factory.InvalidCallHResult;

        int result = Direct3D9Surface.TryCreate(manager, surfaceObject.Surface, out Direct3D9Surface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 1, 1, 0, null),
            (result, _getDescriptionCount, _releaseCount, manager.ResourceCount, surface));
    }

    [TestMethod]
    public void WhenCachedSurfaceDescriptionIsReadAfterReleaseThenObjectDisposedIsThrownWithoutNativeCall()
    {
        using FakeSurfaceObject surfaceObject = new();
        _ = Direct3D9Surface.TryCreate(new Direct3D9ResourceManager(), surfaceObject.Surface, out Direct3D9Surface? surface);
        surface!.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.GetDescription());
        Assert.AreEqual(1, _getDescriptionCount);
    }

    [TestMethod]
    public void WhenOptionalSurfaceAccessorHasNoNativeSurfaceThenNullIsBorrowed()
    {
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), null);

        IDirect3DSurface9* borrowedSurface = surface.SurfaceForDeviceCall;

        Assert.IsTrue(borrowedSurface is null);
    }

    [TestMethod]
    public void WhenRequiredSurfaceAccessorIsReadThenNativeIdentityIsBorrowedWithoutAddRef()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);

        IDirect3DSurface9* borrowedSurface = surface.Surface;

        Assert.AreEqual(((nint) surfaceObject.Surface, 0), ((nint) borrowedSurface, _addReferenceCount));
    }

    [TestMethod]
    public void WhenBorrowedSurfaceIsUsedSynchronouslyThenLockAndDeviceContextUseSameIdentityWithoutAddRef()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);

        int lockResult = surface.LockRect(out _, new Direct3D9SurfaceRect(1, 2, 3, 4), 0);
        Direct3D9SurfaceDeviceContextResult deviceContextResult = surface.GetDeviceContext();

        Assert.AreEqual((0, 0, 1, 1, 0),
            (lockResult, deviceContextResult.HResult, _lockCount, _getDeviceContextCount, _addReferenceCount));
    }

    [TestMethod]
    public void WhenSurfaceIsLockedThenRectangleFlagsAndNativeOutputArePassedThrough()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        Direct3D9SurfaceRect rectangle = new(1, 2, 30, 40);

        int result = surface.LockRect(out LockedRect lockedRect, rectangle, 0x8000_0010);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, rectangle, 0x8000_0010u, 64, (nint) 42),
            (result, _lockRectangle, _lockFlags, lockedRect.Pitch, (nint) lockedRect.PBits));
    }

    [TestMethod]
    public void WhenSurfaceLockFailsThenNativeFailureIsReturnedAndOutputStartsInitialized()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        _lockResult = Direct3D9Factory.InvalidCallHResult;
        _lockedRect = default;

        int result = surface.LockRect(out LockedRect lockedRect, new Direct3D9SurfaceRect(1, 2, 3, 4), 7);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, 0, 1, 0),
            (result, lockedRect.Pitch, (nint) lockedRect.PBits, _lockCount, _unlockCount));
    }

    [TestMethod]
    public void WhenSurfaceIsUnlockedThenNativeFailureIsReturnedExactlyOnce()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        _unlockResult = Direct3D9Factory.InvalidCallHResult;

        int result = surface.UnlockRect();

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 1), (result, _unlockCount));
    }

    [TestMethod]
    public void WhenReleasedSurfaceIsLockedOrUnlockedThenNativeCallsAreRejected()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        surface.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.LockRect(out _, default, 0));
        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.UnlockRect());
        Assert.AreEqual((0, 0), (_lockCount, _unlockCount));
    }

    [TestMethod]
    public void WhenSurfaceDeviceContextIsAcquiredThenNativeOutputAndReleaseHandleArePassedThrough()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        _releaseDeviceContextResult = Direct3D9Factory.InvalidCallHResult;

        Direct3D9SurfaceDeviceContextResult getResult = surface.GetDeviceContext();
        int releaseResult = surface.ReleaseDeviceContext(getResult.DeviceContext);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, (nint) 42, Direct3D9Factory.InvalidCallHResult, 1, 1, (nint) 42),
            (getResult.HResult, getResult.DeviceContext, releaseResult, _getDeviceContextCount, _releaseDeviceContextCount, _releasedDeviceContext));
    }

    [TestMethod]
    public void WhenSurfaceDeviceContextAcquisitionFailsThenOutputStartsInitializedAndNativeFailurePropagates()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        _getDeviceContextResult = Direct3D9Factory.InvalidCallHResult;
        _deviceContext = 0;

        Direct3D9SurfaceDeviceContextResult result = surface.GetDeviceContext();

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, (nint) 0, 1, 0),
            (result.HResult, result.DeviceContext, _getDeviceContextCount, _releaseDeviceContextCount));
    }

    [TestMethod]
    public void WhenSurfaceDeviceContextAcquisitionFailsWithHandleThenHandleIsReleasedAndOutputIsCleared()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        _getDeviceContextResult = Direct3D9Factory.InvalidCallHResult;
        _releaseDeviceContextResult = Direct3D9Factory.DriverInternalErrorHResult;

        Direct3D9SurfaceDeviceContextResult result = surface.GetDeviceContext();

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, (nint) 0, 1, 1, (nint) 42),
            (result.HResult, result.DeviceContext, _getDeviceContextCount, _releaseDeviceContextCount, _releasedDeviceContext));
    }

    [TestMethod]
    public void WhenReleasedSurfaceDeviceContextMethodsAreCalledThenNativeCallsAreRejected()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        surface.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.GetDeviceContext());
        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.ReleaseDeviceContext(42));
        Assert.AreEqual((0, 0), (_getDeviceContextCount, _releaseDeviceContextCount));
    }

    [TestMethod]
    public void WhenSurfaceDeviceIsAcquiredThenSlotOutputAndOwningReferenceArePassedThrough()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);

        nint expectedDevice = _device;
        int result = surface.GetDevice(out nint device);
        Direct3D9Factory.Release(device);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, expectedDevice, 1, 1),
            (result, device, _getDeviceCount, _releaseDeviceCount));
    }

    [TestMethod]
    public void WhenSurfaceDeviceAcquisitionFailsWithDeviceThenOwningReferenceIsReleasedAndOutputIsCleared()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        _getDeviceResult = Direct3D9Factory.InvalidCallHResult;

        int result = surface.GetDevice(out nint device);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, (nint) 0, 1, 1),
            (result, device, _getDeviceCount, _releaseDeviceCount));
    }

    [TestMethod]
    public void WhenReleasedSurfaceDeviceIsRequestedThenNativeCallIsRejected()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);
        surface.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.GetDevice(out _));
        Assert.AreEqual(0, _getDeviceCount);
    }

    [TestMethod]
    public void WhenSurfaceIsReleasedThenOptionalBorrowingIsRejected()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);

        surface.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = surface.SurfaceForDeviceCall);
    }

    [TestMethod]
    public void WhenManagedSurfaceFormatDiffersThenReadIsRejectedBeforeLocking()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9ResourceManager resourceManager = new();
        using Direct3D9Surface surface = new(resourceManager, surfaceObject.Surface);
        _description = new SurfaceDesc(
            format: Format.X8R8G8B8,
            type: Resourcetype.Surface,
            pool: Pool.Managed,
            width: 2,
            height: 2);
        uint useContextDepth = resourceManager.EnterUseContext();
        try
        {
            int result = surface.ReadIntoSystemMemoryBuffer(
                new Direct3D9BitmapRealizationRectangle(0, 0, 2, 2),
                [],
                MilPixelFormat.Pbgra32Bpp,
                8,
                16,
                42);

            Assert.AreEqual((Direct3D9Factory.WgxInvalidCallHResult, 0), (result, _lockCount));
        }
        finally
        {
            resourceManager.ExitUseContext(useContextDepth);
        }
    }

    [TestMethod]
    public void WhenManagedSurfaceIsReadWithClipsThenOnlyIntersectingRowsAreCopiedAndUnlockFailureIsIgnored()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9ResourceManager resourceManager = new();
        using Direct3D9Surface surface = new(resourceManager, surfaceObject.Surface);
        _description = new SurfaceDesc(
            format: Format.A8R8G8B8,
            type: Resourcetype.Surface,
            pool: Pool.Managed,
            width: 4,
            height: 3);
        byte* source = stackalloc byte[48];
        byte* destination = stackalloc byte[48];
        for (byte index = 0; index < 48; index++)
        {
            source[index] = index;
            destination[index] = 0xFF;
        }
        _lockedRect = new LockedRect(16, source);
        _unlockResult = Direct3D9Factory.InvalidCallHResult;
        uint useContextDepth = resourceManager.EnterUseContext();
        try
        {
            int result = surface.ReadIntoSystemMemoryBuffer(
                new Direct3D9BitmapRealizationRectangle(0, 0, 4, 3),
            [new Direct3D9BitmapRealizationRectangle(1, 1, 3, 3)],
            MilPixelFormat.Pbgra32Bpp,
            16,
            48,
            (nint) destination);

            Assert.AreEqual(
                $"{Direct3D9Factory.SuccessHResult}|1|1|{source[20]}|{source[27]}|{source[36]}|{source[43]}|255|255",
                $"{result}|{_lockCount}|{_unlockCount}|{destination[20]}|{destination[27]}|{destination[36]}|{destination[43]}|{destination[0]}|{destination[47]}");
        }
        finally
        {
            resourceManager.ExitUseContext(useContextDepth);
        }
    }

    [TestMethod]
    public void WhenSurfaceReadOccursOutsideUseContextThenItIsRejected()
    {
        using FakeSurfaceObject surfaceObject = new();
        using Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);

        int result = surface.ReadIntoSystemMemoryBuffer(
            new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
            [],
            MilPixelFormat.Pbgra32Bpp,
            4,
            4,
            42);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 0), (result, _lockCount));
    }

    [TestMethod]
    public void WhenReleasedSurfaceIsReadThenItIsRejected()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9ResourceManager resourceManager = new();
        Direct3D9Surface surface = new(resourceManager, surfaceObject.Surface);
        surface.Dispose();
        uint useContextDepth = resourceManager.EnterUseContext();
        try
        {
            Assert.ThrowsExactly<ObjectDisposedException>(() => surface.ReadIntoSystemMemoryBuffer(
            new Direct3D9BitmapRealizationRectangle(0, 0, 1, 1),
            [],
            MilPixelFormat.Pbgra32Bpp,
                4,
                4,
                42));
        }
        finally
        {
            resourceManager.ExitUseContext(useContextDepth);
        }
    }

    [TestMethod]
    public void WhenManagerDestroysSurfaceThenNativeIdentityIsReleasedOnceAndCleared()
    {
        using FakeSurfaceObject surfaceObject = new();
        Direct3D9Surface surface = new(new Direct3D9ResourceManager(), surfaceObject.Surface);

        surface.Dispose();

        Assert.AreEqual((1, 0), (_releaseCount, (nint) surface.NativeIdentity));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddReference(IDirect3DSurface9* self)
    {
        _addReferenceCount++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(IDirect3DSurface9* self)
    {
        _releaseCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetDevice(IDirect3DSurface9* self, nint* device)
    {
        _getDeviceCount++;
        *device = _device;
        return _getDeviceResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(void* self)
    {
        _releaseDeviceCount++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockRect(
        IDirect3DSurface9* self,
        LockedRect* lockedRect,
        Direct3D9SurfaceRect* rectangle,
        uint flags)
    {
        _lockCount++;
        _lockRectangle = *rectangle;
        _lockFlags = flags;
        *lockedRect = _lockedRect;
        return _lockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockRect(IDirect3DSurface9* self)
    {
        _unlockCount++;
        return _unlockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        _getDescriptionCount++;
        *description = _description;
        return _getDescriptionResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetDeviceContext(IDirect3DSurface9* self, nint* deviceContext)
    {
        _getDeviceContextCount++;
        *deviceContext = _deviceContext;
        return _getDeviceContextResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int ReleaseDeviceContext(IDirect3DSurface9* self, nint deviceContext)
    {
        _releaseDeviceContextCount++;
        _releasedDeviceContext = deviceContext;
        return _releaseDeviceContextResult;
    }

    private struct FakeSurfaceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DSurface9* Surface;

        public FakeSurfaceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 23);
            void** memory = (void**) _memory;
            Surface = (IDirect3DSurface9*) memory;
            void** vtable = memory + 1;
            void** device = memory + 19;
            Surface->LpVtbl = vtable;
            device[0] = memory + 20;
            ((void**) device[0])[2] = (void*) (delegate* unmanaged[Stdcall]<void*, uint>) &ReleaseDevice;
            _device = (nint) device;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &AddReference;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &Release;
            vtable[3] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint*, int>) &GetDevice;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetDescription;
            vtable[13] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, LockedRect*, Direct3D9SurfaceRect*, uint, int>) &LockRect;
            vtable[14] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, int>) &UnlockRect;
            vtable[15] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint*, int>) &GetDeviceContext;
            vtable[16] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, nint, int>) &ReleaseDeviceContext;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Surface = null;
        }
    }
}
