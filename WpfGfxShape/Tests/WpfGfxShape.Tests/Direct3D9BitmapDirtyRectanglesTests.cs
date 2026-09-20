using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitmapDirtyRectanglesTests
{
    private static Direct3D9BitmapRealizationRectangle* _nativeRectangles;
    private static uint _nativeRectangleCount;
    private static uint _newestUniquenessToken;
    private static uint _receivedCachedUniquenessToken;
    private static byte _dirtyRectanglesAreValid;
    private static int _addRefCount;
    private static int _releaseCount;

    [TestInitialize]
    public void Initialize()
    {
        _nativeRectangles = null;
        _nativeRectangleCount = 0;
        _newestUniquenessToken = 0;
        _receivedCachedUniquenessToken = 0;
        _dirtyRectanglesAreValid = 1;
        _addRefCount = 0;
        _releaseCount = 0;
    }

    [TestMethod]
    public void WhenNativeDirtyListHasMaximumLengthThenSlotTwelveCopiesAllRectanglesAndUpdatesUniqueness()
    {
        Direct3D9BitmapRealizationRectangle* rectangles = stackalloc Direct3D9BitmapRealizationRectangle[5]
        {
            new(1, 2, 3, 4),
            new(5, 6, 7, 8),
            new(9, 10, 11, 12),
            new(13, 14, 15, 16),
            new(17, 18, 19, 20)
        };
        _nativeRectangles = rectangles;
        _nativeRectangleCount = 5;
        _newestUniquenessToken = 42;
        using FakeBitmapObject bitmap = new();

        bool result = Direct3D9Bitmap.GetDirtyRectangles(
            bitmap.Instance,
            31,
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
            out uint newestUniquenessToken);

        Assert.AreEqual(
            (true, 31u, 42u, 5, new Direct3D9BitmapRealizationRectangle(17, 18, 19, 20), 0, 0),
            (result, _receivedCachedUniquenessToken, newestUniquenessToken, dirtyRectangles.Count, dirtyRectangles[4], _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenNativeBorrowedArrayChangesAfterQueryThenReturnedRectanglesRemainStable()
    {
        Direct3D9BitmapRealizationRectangle* rectangles = stackalloc Direct3D9BitmapRealizationRectangle[1]
        {
            new(3, 4, 8, 9)
        };
        _nativeRectangles = rectangles;
        _nativeRectangleCount = 1;
        using FakeBitmapObject bitmap = new();

        bool result = Direct3D9Bitmap.GetDirtyRectangles(
            bitmap.Instance,
            0,
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
            out _);
        rectangles[0] = new Direct3D9BitmapRealizationRectangle(30, 40, 80, 90);

        Assert.AreEqual(
            (true, new Direct3D9BitmapRealizationRectangle(3, 4, 8, 9)),
            (result, dirtyRectangles[0]));
    }

    [TestMethod]
    public void WhenNativeQueryReturnsFalseThenLatestUniquenessIsPreservedAndCacheIsCompletelyInvalid()
    {
        _dirtyRectanglesAreValid = 0;
        _newestUniquenessToken = 52;
        using FakeBitmapObject bitmap = new();

        bool result = Direct3D9Bitmap.GetDirtyRectangles(
            bitmap.Instance,
            41,
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
            out uint newestUniquenessToken);

        Assert.AreEqual(
            (false, 52u, 0, 0, 0),
            (result, newestUniquenessToken, dirtyRectangles.Count, _addRefCount, _releaseCount));
    }

    [TestMethod]
    public void WhenNativeDirtyListExceedsMaximumLengthThenListIsRejectedWithoutReadingIt()
    {
        Direct3D9BitmapRealizationRectangle rectangle = new(1, 2, 3, 4);
        _nativeRectangles = &rectangle;
        _nativeRectangleCount = 6;
        _newestUniquenessToken = 62;
        using FakeBitmapObject bitmap = new();

        bool result = Direct3D9Bitmap.GetDirtyRectangles(
            bitmap.Instance,
            51,
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
            out uint newestUniquenessToken);

        Assert.AreEqual((false, 62u, 0), (result, newestUniquenessToken, dirtyRectangles.Count));
    }

    [TestMethod]
    public void WhenBitmapPointerIsNullThenQueryReturnsInvalidDirtyInformationAndRetainsCachedUniqueness()
    {
        bool result = Direct3D9Bitmap.GetDirtyRectangles(
            0,
            71,
            out IReadOnlyList<Direct3D9BitmapRealizationRectangle> dirtyRectangles,
            out uint newestUniquenessToken);

        Assert.AreEqual((false, 71u, 0), (result, newestUniquenessToken, dirtyRectangles.Count));
    }

    [TestMethod]
    public void WhenUpdaterUsesNativeBitmapThenCachedTokenIsQueriedAndNewestTokenIsCommitted()
    {
        Direct3D9BitmapRealizationRectangle* rectangles = stackalloc Direct3D9BitmapRealizationRectangle[1]
        {
            new(10, 8, 30, 25)
        };
        _nativeRectangles = rectangles;
        _nativeRectangleCount = 1;
        _newestUniquenessToken = 12;
        using FakeBitmapObject bitmap = new();
        Direct3D9BitmapColorSourceRealizationState state = new(
            bitmap.Instance,
            MilPixelFormat.Pbgra32Bpp,
            _ => 12);
        Direct3D9BitmapRealizationRectangle required = new(10, 8, 30, 25);
        state.SetBitmapAndContextCacheParameters(
            23,
            new Direct3D9BitmapRealizationProperties(
                MilBitmapInterpolationMode.NearestNeighbor,
                Direct3D9TextureMipMapLevel.One,
                MilBitmapWrapMode.Extend,
                MilPixelFormat.Pbgra32Bpp,
                IsMinimumRealizationRectComputed: false,
                BitmapWidth: 40,
                BitmapHeight: 30,
                Width: 40,
                Height: 30)
            {
                SourceContained = new Direct3D9BitmapRealizationRectangle(0, 0, 40, 30)
            });
        state.SetRequiredRealizationBounds(required);
        state.Commit(11, required);
        Direct3D9BitmapTexturePopulator populator = new(
            (nint _, uint _, nint _, out nint bitmapLock, out bool copySource, out nint surface) =>
            {
                bitmapLock = 0;
                copySource = false;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint _, nint _, nint _, bool _) => Direct3D9Factory.SuccessHResult,
            () => Direct3D9Factory.SuccessHResult,
            state,
            _ => { });
        Direct3D9BitmapColorSourceTextureUpdater updater = new(state, populator);

        int result = updater.Update();

        Assert.AreEqual(
            (0, 11u, 12u, 0, 0),
            (result, _receivedCachedUniquenessToken, state.CachedUniquenessToken, _addRefCount, _releaseCount));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(void*** bitmap)
    {
        _addRefCount++;
        return 2;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(void*** bitmap)
    {
        _releaseCount++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static byte GetDirtyRectangles(
        void*** bitmap,
        Direct3D9BitmapRealizationRectangle** dirtyRectangles,
        uint* dirtyRectangleCount,
        uint* cachedUniquenessToken)
    {
        _receivedCachedUniquenessToken = *cachedUniquenessToken;
        *dirtyRectangles = _nativeRectangles;
        *dirtyRectangleCount = _nativeRectangleCount;
        *cachedUniquenessToken = _newestUniquenessToken;
        return _dirtyRectanglesAreValid;
    }

    private struct FakeBitmapObject : IDisposable
    {
        private nint _memory;
        internal nint Instance;

        public FakeBitmapObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 14);
            void** memory = (void**) _memory;
            Instance = (nint) memory;
            void** vtable = memory + 1;
            *memory = vtable;
            vtable[1] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &AddRef;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &Release;
            vtable[12] = (void*) (delegate* unmanaged[Stdcall]<void***, Direct3D9BitmapRealizationRectangle**, uint*, uint*, byte>) &GetDirtyRectangles;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Instance = 0;
        }
    }
}
