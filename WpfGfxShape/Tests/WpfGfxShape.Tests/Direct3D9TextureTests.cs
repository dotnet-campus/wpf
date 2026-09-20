using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9TextureTests
{
    private static uint _level;
    private static uint _flags;
    private static Direct3D9SurfaceRect _rectangle;
    private static bool _hasRectangle;
    private static int _lockResult;
    private static int _unlockResult;
    private static int _addDirtyResult;
    private static int _releaseCount;
    private static int _addRefCount;
    private static uint _levelCount;
    private static int _getLevelDescriptionResult;
    private static SurfaceDesc[] _levelDescriptions = [];
    private static int _getSurfaceLevelResult;
    private static int _getSurfaceLevelCallCount;
    private static int _surfaceGetDescriptionResult;
    private static int _surfaceGetDescriptionCallCount;
    private static int _surfaceAddRefCount;
    private static int _surfaceReleaseCount;
    private static IDirect3DSurface9* _surface;
    private static int _generateMipSubLevelsCallCount;
    private static uint _getSurfaceLevelFailureLevel;
    private static int _stretchRectResult;
    private static int _stretchRectCallCount;
    private static Texturefiltertype _stretchRectFilter;
    private static readonly List<string> ReleaseOrder = [];

    [TestInitialize]
    public void Initialize()
    {
        _level = uint.MaxValue;
        _flags = 0;
        _rectangle = default;
        _hasRectangle = false;
        _lockResult = 0;
        _unlockResult = 0;
        _addDirtyResult = 0;
        _releaseCount = 0;
        _addRefCount = 0;
        _levelCount = 1;
        _getLevelDescriptionResult = 0;
        _getSurfaceLevelResult = 0;
        _getSurfaceLevelCallCount = 0;
        _surfaceGetDescriptionResult = 0;
        _surfaceGetDescriptionCallCount = 0;
        _surfaceAddRefCount = 0;
        _surfaceReleaseCount = 0;
        _surface = null;
        _generateMipSubLevelsCallCount = 0;
        _getSurfaceLevelFailureLevel = uint.MaxValue;
        _stretchRectResult = 0;
        _stretchRectCallCount = 0;
        _stretchRectFilter = default;
        ReleaseOrder.Clear();
        _levelDescriptions =
        [
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 64, height: 32)
        ];
    }

    [TestMethod]
    public void WhenLockingTextureThenLevelRectangleFlagsAndLockedDataAreForwarded()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        Direct3D9SurfaceRect rectangle = new(1, 2, 30, 40);

        int result = texture.LockRect(out LockedRect lockedRect, rectangle, 0x10);

        Assert.AreEqual((0, 0u, true, rectangle, 0x10u, 256, (nint) 42),
            (result, _level, _hasRectangle, _rectangle, _flags, lockedRect.Pitch, (nint) lockedRect.PBits));
    }

    [TestMethod]
    public void WhenLockingWholeTextureThenNullRectangleIsForwarded()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);

        int result = texture.LockRect(out LockedRect lockedRect, null, 0x20);

        Assert.AreEqual((0, 0u, false, 0x20u, 256, (nint) 42),
            (result, _level, _hasRectangle, _flags, lockedRect.Pitch, (nint) lockedRect.PBits));
    }

    [TestMethod]
    public void WhenLockingTextureFailsThenHResultIsReturned()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        _lockResult = Direct3D9Factory.InvalidCallHResult;

        int result = texture.LockRect(out _, new Direct3D9SurfaceRect(0, 0, 1, 1), 0);

        Assert.AreEqual(Direct3D9Factory.InvalidCallHResult, result);
    }

    [TestMethod]
    public void WhenUnlockingTextureThenLevelZeroAndHResultAreForwarded()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        _unlockResult = Direct3D9Factory.GenericFailureHResult;

        int result = texture.UnlockRect();

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 0u), (result, _level));
    }

    [TestMethod]
    public void WhenAddingDirtyRectangleThenRectangleAndHResultAreForwarded()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        Direct3D9SurfaceRect rectangle = new(3, 4, 50, 60);
        _addDirtyResult = Direct3D9Factory.OutOfMemoryHResult;

        int result = texture.AddDirtyRect(rectangle);

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, rectangle), (result, _rectangle));
    }

    [TestMethod]
    public void WhenUsingReleasedTextureThenLockOperationsAreRejected()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        texture.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => texture.UnlockRect());
    }

    [TestMethod]
    public void WhenReadingTextureSizeThenLevelZeroDimensionsAreReturned()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);

        Assert.AreEqual((64u, 64u), (texture.Width, texture.Height));
    }

    [TestMethod]
    public void WhenReadingReleasedTextureWidthThenCallIsRejected()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        texture.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = texture.Width);
    }

    [TestMethod]
    public void WhenReadingReleasedTextureHeightThenCallIsRejected()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        texture.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = texture.Height);
    }

    [TestMethod]
    public void WhenWrappingExistingTextureThenInitializationAddsReferenceAndRegistersResource()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        _levelCount = 2;
        _levelDescriptions =
        [
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 64, height: 32),
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 32, height: 16)
        ];

        int result = Direct3D9Texture.TryCreate(manager, textureObject.Texture, isEvictable: false, out Direct3D9Texture? texture);

        Assert.AreEqual((0, 1, 1, 2u, 64u, 32u),
            (result, _addRefCount, manager.ResourceCount, texture?.LevelCount, texture?.Width, texture?.Height));
        texture?.Dispose();
    }

    [TestMethod]
    public void WhenExistingTextureInitializationFailsThenOwnershipAndRegistrationRemainWithCaller()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        _getLevelDescriptionResult = Direct3D9Factory.InvalidCallHResult;

        int result = Direct3D9Texture.TryCreate(manager, textureObject.Texture, isEvictable: true, out Direct3D9Texture? texture);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 0, 0, 0, null),
            (result, _addRefCount, _releaseCount, manager.ResourceCount, texture));
    }

    [TestMethod]
    public void WhenWrappingExistingTextureAsEvictableThenItIsMarkedAfterSuccessfulInitialization()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();

        int result = Direct3D9Texture.TryCreate(manager, textureObject.Texture, isEvictable: true, out Direct3D9Texture? texture);

        Assert.AreEqual((0, true, 1), (result, texture?.IsEvictable, manager.ResourceCount));
        texture?.Dispose();
    }

    [TestMethod]
    public void WhenGettingTextureSurfaceLevelTwiceThenNativeSurfaceIsCachedAndIndependentWrappersAreReturned()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9Texture texture = new(manager, textureObject.Texture, 64, 64);

        int firstResult = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? first);
        int secondResult = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? second);

        Assert.AreEqual((0, 0, false, 1, 4, 1, 4),
            (firstResult, secondResult, ReferenceEquals(first, second), _getSurfaceLevelCallCount,
                _surfaceAddRefCount, _surfaceReleaseCount, manager.ResourceCount));
        first?.Dispose();
        second?.Dispose();
    }

    [TestMethod]
    public void WhenTextureSurfaceLevelIsCachedThenTextureMemoryIsNotCountedAgain()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        _ = Direct3D9Texture.TryCreate(manager, textureObject.Texture, isEvictable: false, out Direct3D9Texture? texture);

        int result = texture!.TryGetSurfaceLevel(0, out Direct3D9Surface? surface);
        uint memoryWithBorrowedSurface = manager.TotalVideoMemoryConsumption;
        surface!.Dispose();
        uint memoryWithCachedSurface = manager.TotalVideoMemoryConsumption;
        texture.Dispose();

        Assert.AreEqual(
            (0, 8192u, 8192u, 0u),
            (result, memoryWithBorrowedSurface, memoryWithCachedSurface, manager.TotalVideoMemoryConsumption));
    }

    [TestMethod]
    public void WhenReturnedTextureSurfaceIsReleasedThenCachedSurfaceRemainsUsable()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9Texture texture = new(manager, textureObject.Texture, 64, 64);
        _ = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? first);
        first?.Dispose();

        int result = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? second);

        Assert.AreEqual((0, 1, 4, 2, 3),
            (result, _getSurfaceLevelCallCount, _surfaceAddRefCount, _surfaceReleaseCount, manager.ResourceCount));
        second?.Dispose();
    }

    [TestMethod]
    public void WhenTextureIsReleasedWithDelayThenNativeTextureIsReleasedOnTheNextFrame()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        Direct3D9Texture texture = new(manager, textureObject.Texture, 64, 64);
        manager.UnusedNotification(texture);

        uint currentFrameCount = manager.DestroyResources(Direct3D9DestroyResourcesStyle.WithDelay);
        int currentFrameReleaseCount = _releaseCount;
        uint nextFrameCount = manager.DestroyReleasedResourcesFromLastFrame();

        Assert.AreEqual((0u, 0, 1u, 1),
            (currentFrameCount, currentFrameReleaseCount, nextFrameCount, _releaseCount));
    }

    [TestMethod]
    public void WhenTextureIsReleasedThenNativeTexturePrecedesCachedSurfaceLevels()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        Direct3D9Texture texture = new(manager, textureObject.Texture, 64, 64);
        _ = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? returnedSurface);
        returnedSurface?.Dispose();
        ReleaseOrder.Clear();

        texture.Dispose();

        CollectionAssert.AreEqual(new[] { "Texture", "Surface" }, ReleaseOrder);
    }

    [TestMethod]
    public void WhenTextureIsReleasedThenReturnedSurfaceLevelRemainsUsableUntilItsOwnRelease()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        Direct3D9Texture texture = new(manager, textureObject.Texture, 64, 64);
        _ = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? returnedSurface);

        texture.Dispose();
        SurfaceDesc description = returnedSurface!.GetDescription();

        Assert.AreEqual((64u, 32u, 1, 2, 1),
            (description.Width, description.Height, _releaseCount, _surfaceReleaseCount, manager.ResourceCount));
        returnedSurface.Dispose();
    }

    [TestMethod]
    public void WhenManagerDestroysTextureAndReturnedSurfaceThenBothAreInvalidatedAndReleasedOnce()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        Direct3D9Texture texture = new(manager, textureObject.Texture, 64, 64);
        _ = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? returnedSurface);

        manager.DestroyAllResources();

        Assert.AreEqual((true, true, 1, 3, 0),
            (texture.IsReleased, returnedSurface!.IsReleased, _releaseCount, _surfaceReleaseCount, manager.ResourceCount));
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = texture.Texture);
        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = returnedSurface.Surface);
        texture.Dispose();
        returnedSurface.Dispose();
    }

    [TestMethod]
    public void WhenGettingTextureSurfaceDescriptionThenInitializationSnapshotIsReused()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9Texture texture = new(manager, textureObject.Texture, 64, 64);

        int result = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? surface);
        SurfaceDesc firstDescription = surface!.GetDescription();
        _levelDescriptions[0] = new SurfaceDesc(
            format: Format.L8,
            type: Resourcetype.Surface,
            pool: Pool.Systemmem,
            width: 1,
            height: 1);
        SurfaceDesc secondDescription = surface.GetDescription();

        Assert.AreEqual((0, 1, firstDescription, firstDescription),
            (result, _surfaceGetDescriptionCallCount, firstDescription, secondDescription));
        surface.Dispose();
    }

    [TestMethod]
    public void WhenTextureSurfaceInitializationFailsThenAllSurfaceReferencesAndRegistrationAreCleanedUp()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        using Direct3D9Texture texture = new(manager, textureObject.Texture, 64, 64);
        _surfaceGetDescriptionResult = Direct3D9Factory.InvalidCallHResult;

        int result = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? surface);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, null, 2, 2, 1),
            (result, surface, _surfaceAddRefCount, _surfaceReleaseCount, manager.ResourceCount));
    }

    [TestMethod]
    public void WhenTextureSurfaceLevelIsOutOfRangeThenNativeTextureIsNotCalled()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);

        int result = texture.TryGetSurfaceLevel(1, out Direct3D9Surface? surface);

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, null, 0),
            (result, surface, _getSurfaceLevelCallCount));
    }

    [TestMethod]
    public void WhenTextureSurfaceQueryFailsWithInterfaceThenTemporaryReferenceIsReleased()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        _getSurfaceLevelResult = Direct3D9Factory.OutOfMemoryHResult;
        _getSurfaceLevelFailureLevel = 0;

        int result = texture.TryGetSurfaceLevel(0, out Direct3D9Surface? surface);

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, null, 1, 1),
            (result, surface, _surfaceAddRefCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenNativeTextureIsAccessedThenPointerIsBorrowedWithoutReferenceCountChanges()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9Texture texture = CreateTexture(textureObject.Texture);

        nint nativeTexture = (nint) texture.Texture;
        texture.Dispose();

        Assert.AreEqual(((nint) textureObject.Texture, 0, 1),
            (nativeTexture, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenNativeSurfaceLevelReferenceIsReturnedThenItOutlivesTextureUntilCallerRelease()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9Texture texture = CreateTexture(textureObject.Texture);

        int result = texture.TryGetSurfaceLevelReference(0, out nint surface);
        texture.Dispose();
        int releasesBeforeCallerRelease = _surfaceReleaseCount;
        Direct3D9Factory.Release(surface);

        Assert.AreEqual((0, (nint) _surface, 4, 3, 4),
            (result, surface, _surfaceAddRefCount, releasesBeforeCallerRelease, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenNativeSurfaceLevelQueryFailsWithInterfaceThenOutputIsClearedAndReferenceIsReleased()
    {
        using FakeTextureObject textureObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        _getSurfaceLevelResult = Direct3D9Factory.OutOfMemoryHResult;
        _getSurfaceLevelFailureLevel = 0;

        int result = texture.TryGetSurfaceLevelReference(0, out nint surface);

        Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, 0, 1, 1),
            (result, surface, _surfaceAddRefCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenEvictableTextureSurfaceQueryFailsThenTextureIsStillRegisteredAsUsed()
    {
        using FakeTextureObject textureObject = new();
        Direct3D9ResourceManager manager = new();
        _ = Direct3D9Texture.TryCreate(manager, textureObject.Texture, isEvictable: true, out Direct3D9Texture? texture);
        using (texture)
        {
            _getSurfaceLevelResult = Direct3D9Factory.OutOfMemoryHResult;
            _getSurfaceLevelFailureLevel = 0;
            uint depth = manager.EnterUseContext();
            try
            {
                int result = texture!.TryGetSurfaceLevel(0, out Direct3D9Surface? surface);

                Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, null, depth),
                    (result, surface, texture.ActiveUseContextDepth));
            }
            finally
            {
                manager.ExitUseContext(depth);
            }
        }
    }

    [TestMethod]
    public void WhenUpdatingSingleLevelTextureThenNoNativeMipmapWorkRuns()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        using Direct3D9Device device = CreateDevice(deviceObject.Device, canAutoGenerateMipmaps: true);

        int result = texture.UpdateMipmapLevels(device);

        Assert.AreEqual((0, 0, 0, 0),
            (result, _generateMipSubLevelsCallCount, _getSurfaceLevelCallCount, _stretchRectCallCount));
    }

    [TestMethod]
    public void WhenUpdatingAutomaticMipmapsThenGenerateMipSubLevelsRunsWithoutSurfaceWork()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _levelCount = 3;
        _levelDescriptions = CreateMipmapDescriptions();
        int createResult = Direct3D9Texture.TryCreate(
            new Direct3D9ResourceManager(), textureObject.Texture, isEvictable: false, out Direct3D9Texture? texture);
        using (texture)
        using (Direct3D9Device device = CreateDevice(deviceObject.Device, canAutoGenerateMipmaps: true))
        {
            int result = texture!.UpdateMipmapLevels(device);

            Assert.AreEqual((0, 0, 1, 0, 0),
                (createResult, result, _generateMipSubLevelsCallCount, _getSurfaceLevelCallCount, _stretchRectCallCount));
        }
    }

    [TestMethod]
    public void WhenUpdatingManualMipmapsThenEachLevelIsLinearlyStretchedFromPreviousLevel()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _levelCount = 3;
        _levelDescriptions = CreateMipmapDescriptions();
        _ = Direct3D9Texture.TryCreate(
            new Direct3D9ResourceManager(), textureObject.Texture, isEvictable: false, out Direct3D9Texture? texture);
        using (texture)
        using (Direct3D9Device device = CreateDevice(deviceObject.Device, canAutoGenerateMipmaps: false))
        {
            int result = texture!.UpdateMipmapLevels(device);

            Assert.AreEqual((0, 0, 3, 2, Texturefiltertype.Linear, 9, 6),
                (result, _generateMipSubLevelsCallCount, _getSurfaceLevelCallCount, _stretchRectCallCount,
                    _stretchRectFilter, _surfaceAddRefCount, _surfaceReleaseCount));
        }
    }

    [TestMethod]
    public void WhenManualMipmapSurfaceQueryFailsThenFirstHResultStopsFurtherWork()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _levelCount = 3;
        _levelDescriptions = CreateMipmapDescriptions();
        _getSurfaceLevelFailureLevel = 1;
        _getSurfaceLevelResult = Direct3D9Factory.OutOfMemoryHResult;
        _ = Direct3D9Texture.TryCreate(
            new Direct3D9ResourceManager(), textureObject.Texture, isEvictable: false, out Direct3D9Texture? texture);
        using (texture)
        using (Direct3D9Device device = CreateDevice(deviceObject.Device, canAutoGenerateMipmaps: false))
        {
            int result = texture!.UpdateMipmapLevels(device);

            Assert.AreEqual((Direct3D9Factory.OutOfMemoryHResult, 2, 0, 4, 3),
                (result, _getSurfaceLevelCallCount, _stretchRectCallCount, _surfaceAddRefCount, _surfaceReleaseCount));
        }
    }

    [TestMethod]
    public void WhenManualMipmapStretchFailsThenFirstHResultStopsFurtherLevelsAndCachedSurfacesAreReleasedWithTexture()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        _levelCount = 3;
        _levelDescriptions = CreateMipmapDescriptions();
        _stretchRectResult = Direct3D9Factory.InvalidCallHResult;
        _ = Direct3D9Texture.TryCreate(
            new Direct3D9ResourceManager(), textureObject.Texture, isEvictable: false, out Direct3D9Texture? texture);
        int result;
        using (texture)
        using (Direct3D9Device device = CreateDevice(deviceObject.Device, canAutoGenerateMipmaps: false))
        {
            result = texture!.UpdateMipmapLevels(device);
        }

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, 2, 1, Texturefiltertype.Linear, 6, 6),
            (result, _getSurfaceLevelCallCount, _stretchRectCallCount, _stretchRectFilter,
                _surfaceAddRefCount, _surfaceReleaseCount));
    }

    [TestMethod]
    public void WhenUpdatingReleasedTextureThenCallIsRejected()
    {
        using FakeTextureObject textureObject = new();
        using FakeDeviceObject deviceObject = new();
        Direct3D9Texture texture = CreateTexture(textureObject.Texture);
        using Direct3D9Device device = CreateDevice(deviceObject.Device, canAutoGenerateMipmaps: false);
        texture.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => texture.UpdateMipmapLevels(device));
    }

    private static SurfaceDesc[] CreateMipmapDescriptions()
    {
        return
        [
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 64, height: 32),
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 32, height: 16),
            new SurfaceDesc(format: Format.A8R8G8B8, type: Resourcetype.Texture, pool: Pool.Default, width: 16, height: 8)
        ];
    }

    private static Direct3D9Texture CreateTexture(IDirect3DTexture9* texture)
    {
        return new Direct3D9Texture(new Direct3D9ResourceManager(), texture, 64, 64);
    }

    private static Direct3D9Device CreateDevice(IDirect3DDevice9* device, bool canAutoGenerateMipmaps)
    {
        Caps9 capabilities = new()
        {
            Caps2 = canAutoGenerateMipmaps ? (uint) D3D9.Caps2Canautogenmipmap : 0
        };
        return new Direct3D9Device(device, null, 0, Devtype.Hal, 0, default, capabilities: capabilities);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(IDirect3DTexture9* self)
    {
        _addRefCount++;
        return (uint) _addRefCount;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(IDirect3DTexture9* self)
    {
        _releaseCount++;
        ReleaseOrder.Add("Texture");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint GetLevelCount(IDirect3DTexture9* self)
    {
        return _levelCount;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetLevelDescription(IDirect3DTexture9* self, uint level, SurfaceDesc* description)
    {
        if (_getLevelDescriptionResult < 0)
        {
            return _getLevelDescriptionResult;
        }

        *description = _levelDescriptions[level];
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceLevel(IDirect3DTexture9* self, uint level, IDirect3DSurface9** surface)
    {
        _getSurfaceLevelCallCount++;
        _level = level;
        _surfaceAddRefCount++;
        *surface = _surface;
        return level == _getSurfaceLevelFailureLevel ? _getSurfaceLevelResult : 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void GenerateMipSubLevels(IDirect3DTexture9* self)
    {
        _generateMipSubLevelsCallCount++;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint SurfaceAddRef(IDirect3DSurface9* self)
    {
        return (uint) ++_surfaceAddRefCount;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint SurfaceRelease(IDirect3DSurface9* self)
    {
        _surfaceReleaseCount++;
        ReleaseOrder.Add("Surface");
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceDescription(IDirect3DSurface9* self, SurfaceDesc* description)
    {
        _surfaceGetDescriptionCallCount++;
        *description = _levelDescriptions[_level];
        return _surfaceGetDescriptionResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockRect(
        IDirect3DTexture9* self,
        uint level,
        LockedRect* lockedRect,
        Direct3D9SurfaceRect* rectangle,
        uint flags)
    {
        _level = level;
        _flags = flags;
        _hasRectangle = rectangle is not null;
        if (rectangle is not null)
        {
            _rectangle = *rectangle;
        }

        lockedRect->Pitch = 256;
        lockedRect->PBits = (void*) 42;
        return _lockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockRect(IDirect3DTexture9* self, uint level)
    {
        _level = level;
        return _unlockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int AddDirtyRect(IDirect3DTexture9* self, Direct3D9SurfaceRect* rectangle)
    {
        _rectangle = *rectangle;
        return _addDirtyResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int StretchRect(
        IDirect3DDevice9* self,
        IDirect3DSurface9* source,
        Direct3D9SurfaceRect* sourceRectangle,
        IDirect3DSurface9* destination,
        Direct3D9SurfaceRect* destinationRectangle,
        Texturefiltertype filter)
    {
        _stretchRectCallCount++;
        _stretchRectFilter = filter;
        return _stretchRectResult;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 107);
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[34] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, IDirect3DSurface9*, Direct3D9SurfaceRect*, IDirect3DSurface9*, Direct3D9SurfaceRect*, Texturefiltertype, int>) &StretchRect;
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
        private nint _surfaceMemory;
        internal IDirect3DTexture9* Texture;

        public FakeTextureObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 24);
            void** memory = (void**) _memory;
            Texture = (IDirect3DTexture9*) memory;
            void** vtable = memory + 1;
            Texture->LpVtbl = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &AddRef;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &Release;
            vtable[13] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &GetLevelCount;
            vtable[16] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, void>) &GenerateMipSubLevels;
            vtable[17] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, SurfaceDesc*, int>) &GetLevelDescription;
            vtable[18] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, IDirect3DSurface9**, int>) &GetSurfaceLevel;
            vtable[19] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, LockedRect*, Direct3D9SurfaceRect*, uint, int>) &LockRect;
            vtable[20] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, int>) &UnlockRect;
            vtable[21] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, Direct3D9SurfaceRect*, int>) &AddDirtyRect;

            _surfaceMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 15);
            void** surfaceMemory = (void**) _surfaceMemory;
            _surface = (IDirect3DSurface9*) surfaceMemory;
            void** surfaceVtable = surfaceMemory + 1;
            _surface->LpVtbl = surfaceVtable;
            surfaceVtable[1] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &SurfaceAddRef;
            surfaceVtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, uint>) &SurfaceRelease;
            surfaceVtable[12] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DSurface9*, SurfaceDesc*, int>) &GetSurfaceDescription;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _surfaceMemory);
            NativeMemory.Free((void*) _memory);
            _surfaceMemory = 0;
            _memory = 0;
            _surface = null;
            Texture = null;
        }
    }
}
