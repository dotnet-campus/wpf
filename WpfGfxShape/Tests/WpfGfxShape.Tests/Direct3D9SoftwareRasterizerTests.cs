using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9SoftwareRasterizerTests
{
    [TestMethod]
    public void WhenSelectingSoftwareRasterizerThenExpectedModuleAndEntryPointAreUsed()
    {
        string expectedLibrary = OperatingSystem.IsWindowsVersionAtLeast(6)
            ? Direct3D9SoftwareRasterizerLoader.CurrentLibraryName
            : Direct3D9SoftwareRasterizerLoader.DownlevelLibraryName;

        Assert.AreEqual(
            (expectedLibrary, "D3D9GetSWInfo"),
            (Direct3D9SoftwareRasterizerLoader.GetLibraryName(), Direct3D9SoftwareRasterizerLoader.GetSoftwareInfoEntryPoint));
    }

    [TestMethod]
    public void WhenLoadingSoftwareRasterizerTwiceThenModuleAndExportAreResolvedOnce()
    {
        int loadCount = 0;
        int exportCount = 0;
        using TestModuleHandle moduleHandle = new();
        using Direct3D9SoftwareRasterizerLoader loader = new(
            _ =>
            {
                loadCount++;
                return moduleHandle;
            },
            (_, _) =>
            {
                exportCount++;
                return 42;
            });

        nint first = loader.GetSoftwareInfo();
        nint second = loader.GetSoftwareInfo();

        Assert.AreEqual((42, 42, 1, 1), ((int) first, (int) second, loadCount, exportCount));
    }

    [TestMethod]
    public void WhenModuleLoadFailsThenFailureIsCached()
    {
        int loadCount = 0;
        using Direct3D9SoftwareRasterizerLoader loader = new(
            _ =>
            {
                loadCount++;
                throw new Win32Exception(126);
            });

        Assert.ThrowsExactly<Win32Exception>(() => loader.GetSoftwareInfo());
        Assert.ThrowsExactly<Win32Exception>(() => loader.GetSoftwareInfo());

        Assert.AreEqual(1, loadCount);
    }

    [TestMethod]
    public void WhenExportResolutionFailsThenLoadedModuleIsReleasedAndFailureIsCached()
    {
        TestModuleHandle moduleHandle = new();
        int exportCount = 0;
        using Direct3D9SoftwareRasterizerLoader loader = new(
            _ => moduleHandle,
            (_, _) =>
            {
                exportCount++;
                throw new EntryPointNotFoundException();
            });

        Assert.ThrowsExactly<EntryPointNotFoundException>(() => loader.GetSoftwareInfo());
        Assert.ThrowsExactly<EntryPointNotFoundException>(() => loader.GetSoftwareInfo());

        Assert.AreEqual((1, 1), (exportCount, moduleHandle.ReleaseCount));
    }

    [TestMethod]
    public void WhenDisposingLoadedRasterizerThenModuleIsReleasedOnce()
    {
        TestModuleHandle moduleHandle = new();
        Direct3D9SoftwareRasterizerLoader loader = new(_ => moduleHandle, (_, _) => 42);
        loader.GetSoftwareInfo();

        loader.Dispose();
        loader.Dispose();

        Assert.AreEqual(1, moduleHandle.ReleaseCount);
    }

    [TestMethod]
    public void WhenRegisteringTwiceThenFirstSuccessfulResultIsReused()
    {
        int registrationCount = 0;
        using Direct3D9SoftwareRasterizerLoader loader = new(_ => new TestModuleHandle(), (_, _) => 42);
        Direct3D9SoftwareRasterizerRegistration registration = new(
            loader,
            getSoftwareInfo =>
            {
                registrationCount++;
                return getSoftwareInfo == 42 ? 0 : Direct3D9Factory.InvalidCallHResult;
            });

        registration.EnsureRegistered();
        registration.EnsureRegistered();

        Assert.AreEqual(1, registrationCount);
    }

    [TestMethod]
    public void WhenRegistrationFailsThenFirstFailureIsReused()
    {
        int registrationCount = 0;
        using Direct3D9SoftwareRasterizerLoader loader = new(_ => new TestModuleHandle(), (_, _) => 42);
        Direct3D9SoftwareRasterizerRegistration registration = new(
            loader,
            _ =>
            {
                registrationCount++;
                return Direct3D9Factory.InvalidCallHResult;
            });

        COMException first = Assert.ThrowsExactly<COMException>(() => registration.EnsureRegistered());
        COMException second = Assert.ThrowsExactly<COMException>(() => registration.EnsureRegistered());

        Assert.AreEqual((Direct3D9Factory.InvalidCallHResult, Direct3D9Factory.InvalidCallHResult, 1),
            (first.HResult, second.HResult, registrationCount));
    }

    [TestMethod]
    public void WhenDisplayStateChangedThenRegistrationIsNotAttempted()
    {
        int registrationCount = 0;
        using Direct3D9SoftwareRasterizerLoader loader = new(_ => new TestModuleHandle(), (_, _) => 42);
        Direct3D9DisplaySet displaySet = new(() => true);
        Direct3D9SoftwareRasterizerRegistration registration = new(
            loader,
            _ =>
            {
                registrationCount++;
                return 0;
            },
            displaySet);

        COMException exception = Assert.ThrowsExactly<COMException>(() => registration.EnsureRegistered());

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, 0), (exception.HResult, registrationCount));
    }

    [TestMethod]
    public void WhenDisplaySetsRegisterSoftwareRasterizerThenEachSetCachesItsOwnResult()
    {
        int registrationCount = 0;
        using Direct3D9SoftwareRasterizerLoader loader = new(_ => new TestModuleHandle(), (_, _) => 42);
        Direct3D9DisplaySet first = new();
        Direct3D9DisplaySet second = new();
        int RegisterSoftwareDevice(nint _) => ++registrationCount >= 0 ? 0 : Direct3D9Factory.InvalidCallHResult;

        first.EnsureSoftwareRasterizerRegistered(loader, RegisterSoftwareDevice);
        first.EnsureSoftwareRasterizerRegistered(loader, RegisterSoftwareDevice);
        second.EnsureSoftwareRasterizerRegistered(loader, RegisterSoftwareDevice);

        Assert.AreEqual(2, registrationCount);
    }

    [TestMethod]
    [DataRow(32767u, 32767u, (int) MilBitmapWrapMode.Tile, true)]
    [DataRow(32768u, 32767u, (int) MilBitmapWrapMode.Tile, false)]
    [DataRow(16383u, 32767u, (int) MilBitmapWrapMode.FlipX, true)]
    [DataRow(16384u, 32767u, (int) MilBitmapWrapMode.FlipX, false)]
    [DataRow(32767u, 16383u, (int) MilBitmapWrapMode.FlipY, true)]
    [DataRow(32767u, 16384u, (int) MilBitmapWrapMode.FlipY, false)]
    [DataRow(16383u, 16383u, (int) MilBitmapWrapMode.FlipXY, true)]
    [DataRow(16384u, 16383u, (int) MilBitmapWrapMode.FlipXY, false)]
    [DataRow(16383u, 16384u, (int) MilBitmapWrapMode.FlipXY, false)]
    public void WhenSelectingMmxBilinearSpanThenFixed16InputRangeIsRequired(
        uint width,
        uint height,
        int wrapMode,
        bool expected)
    {
        bool result = Direct3D9SoftwareBilinearSpan.CanUseMmx(width, height, (MilBitmapWrapMode) wrapMode);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow((int) MilBitmapWrapMode.Extend, 655360, 1310720, -36, -228)]
    [DataRow((int) MilBitmapWrapMode.Tile, 655360, 1310720, -36, -228)]
    [DataRow((int) MilBitmapWrapMode.Border, 655360, 1310720, -36, -228)]
    [DataRow((int) MilBitmapWrapMode.FlipX, 1310720, 1310720, 0, -228)]
    [DataRow((int) MilBitmapWrapMode.FlipY, 655360, 2621440, -36, 0)]
    [DataRow((int) MilBitmapWrapMode.FlipXY, 1310720, 2621440, 0, 0)]
    public void WhenInitializingMmxBilinearSpanThenWrapModeControlsModulusAndEdgeIncrement(
        int wrapMode,
        int expectedModulusWidth,
        int expectedModulusHeight,
        int expectedXEdgeIncrement,
        int expectedYEdgeIncrement)
    {
        Direct3D9SoftwareBilinearFixedPointState state = Direct3D9SoftwareBilinearSpan.CreateMmxFixedPointState(
            Matrix3x2.Identity,
            10,
            20,
            12,
            (MilBitmapWrapMode) wrapMode);

        Assert.AreEqual(
            (expectedModulusWidth, expectedModulusHeight, expectedXEdgeIncrement, expectedYEdgeIncrement),
            (state.ModulusWidth, state.ModulusHeight, state.XEdgeIncrement, state.YEdgeIncrement));
    }

    [TestMethod]
    public void WhenInitializingMmxBilinearSpanThenTransformUsesFixed16Increments()
    {
        Matrix3x2 deviceToTexture = new(1.5f, -0.25f, 0.5f, 2, 3.25f, -4.5f);

        Direct3D9SoftwareBilinearFixedPointState state = Direct3D9SoftwareBilinearSpan.CreateMmxFixedPointState(
            deviceToTexture,
            10,
            20,
            40,
            MilBitmapWrapMode.Tile);

        Assert.AreEqual(
            (98304, -16384, 32768, 131072, 212992, -294912, 98304, -16384),
            (state.M11, state.M12, state.M21, state.M22, state.Dx, state.Dy, state.UIncrement, state.VIncrement));
    }

    [TestMethod]
    public void WhenFixed16TranslationOverflowsThenDeviceOriginRecentersMapping()
    {
        Matrix3x2 deviceToTexture = Matrix3x2.CreateTranslation(32768, -32768);

        Direct3D9SoftwareBilinearFixedPointState state = Direct3D9SoftwareBilinearSpan.CreateMmxFixedPointState(
            deviceToTexture,
            10,
            20,
            40,
            MilBitmapWrapMode.Tile);

        Assert.AreEqual((-32768, 32768, 0, 0), (state.XDeviceOffset, state.YDeviceOffset, state.Dx, state.Dy));
    }

    [TestMethod]
    public void WhenMmxBilinearInputExceedsFixed16RangeThenStateIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9SoftwareBilinearSpan.CreateMmxFixedPointState(
                Matrix3x2.Identity,
                16384,
                10,
                40,
                MilBitmapWrapMode.FlipX));
    }

    [TestMethod]
    [DataRow(0, 655360, 0)]
    [DataRow(655360, 655360, 0)]
    [DataRow(1310720, 655360, 0)]
    [DataRow(-655360, 655360, 0)]
    [DataRow(2031616, 655360, 65536)]
    [DataRow(-2031616, 655360, 589824)]
    public void WhenWrappingBilinearCoordinateThenPositionIsMovedIntoCanonicalRange(
        int coordinate,
        int modulus,
        int expected)
    {
        (int u, int v) = Direct3D9SoftwareBilinearSpan.WrapCanonicalPosition(
            coordinate,
            coordinate,
            modulus,
            modulus,
            MilBitmapWrapMode.Tile);

        Assert.AreEqual((expected, expected), (u, v));
    }

    [TestMethod]
    public void WhenWrappingFlipBilinearCoordinateThenDoubledCanonicalRangeIsPreserved()
    {
        (int u, int v) = Direct3D9SoftwareBilinearSpan.WrapCanonicalPosition(
            983040,
            -65536,
            1310720,
            2621440,
            MilBitmapWrapMode.FlipXY);

        Assert.AreEqual((983040, 2555904), (u, v));
    }

    [TestMethod]
    public void WhenCanonicalWrappingIsRequestedForBorderThenRequestIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9SoftwareBilinearSpan.WrapCanonicalPosition(
                0,
                0,
                65536,
                65536,
                MilBitmapWrapMode.Border));
    }

    [TestMethod]
    [DataRow((int) MilBitmapWrapMode.Extend, -32768L, 16384L, 0, 0, 0, 1, true, true, true, true)]
    [DataRow((int) MilBitmapWrapMode.Border, -32768L, 16384L, -1, 0, 0, 1, false, true, false, true)]
    [DataRow((int) MilBitmapWrapMode.Tile, -32768L, 16384L, 2, 0, 0, 1, true, true, true, true)]
    [DataRow((int) MilBitmapWrapMode.FlipX, 163840L, 16384L, 2, 0, 2, 1, true, true, true, true)]
    public void WhenSelectingFallbackBilinearTexelsThenWrapModeControlsEachSample(
        int wrapMode,
        long u,
        long v,
        int expectedX1,
        int expectedY1,
        int expectedX2,
        int expectedY2,
        bool expectedAInside,
        bool expectedBInside,
        bool expectedCInside,
        bool expectedDInside)
    {
        Direct3D9SoftwareBilinearTexelSelection selection =
            Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(
                u,
                v,
                3,
                2,
                (MilBitmapWrapMode) wrapMode);

        Assert.AreEqual(
            (expectedX1, expectedY1, expectedX2, expectedY2, 128, 64,
                expectedAInside, expectedBInside, expectedCInside, expectedDInside),
            (selection.X1, selection.Y1, selection.X2, selection.Y2, selection.XFraction, selection.YFraction,
                selection.AInside, selection.BInside, selection.CInside, selection.DInside));
    }

    [TestMethod]
    [DataRow((int) MilBitmapWrapMode.FlipX, 229376, 81920, 2, 1, 1, 2)]
    [DataRow((int) MilBitmapWrapMode.FlipY, 81920, 212992, 1, 2, 2, 1)]
    [DataRow((int) MilBitmapWrapMode.FlipXY, 229376, 212992, 2, 2, 1, 1)]
    public void WhenSelectingFlippedTileInteriorTexelsThenMirroredSampleOrderIsPreserved(
        int wrapMode,
        int u,
        int v,
        int expectedX1,
        int expectedY1,
        int expectedX2,
        int expectedY2)
    {
        Direct3D9SoftwareBilinearTexelSelection selection =
            Direct3D9SoftwareBilinearSpan.SelectFlippedTileInteriorTexels(
                u,
                v,
                3,
                3,
                (MilBitmapWrapMode) wrapMode);

        Assert.AreEqual(
            (expectedX1, expectedY1, expectedX2, expectedY2),
            (selection.X1, selection.Y1, selection.X2, selection.Y2));
    }

    [TestMethod]
    public void WhenSelectingFlippedTileInteriorTexelsThenOriginalFractionsArePreserved()
    {
        Direct3D9SoftwareBilinearTexelSelection selection =
            Direct3D9SoftwareBilinearSpan.SelectFlippedTileInteriorTexels(
                229376,
                212992,
                3,
                3,
                MilBitmapWrapMode.FlipXY);

        Assert.AreEqual((128, 64), (selection.XFraction, selection.YFraction));
    }

    [TestMethod]
    [DataRow((int) MilBitmapWrapMode.FlipX, 229376L, 81920L)]
    [DataRow((int) MilBitmapWrapMode.FlipY, 81920L, 212992L)]
    [DataRow((int) MilBitmapWrapMode.FlipXY, 229376L, 212992L)]
    public void WhenSelectingFlippedTileInteriorTexelsThenSelectionMatches64BitFallback(
        int wrapMode,
        long u,
        long v)
    {
        Direct3D9SoftwareBilinearTexelSelection interior =
            Direct3D9SoftwareBilinearSpan.SelectFlippedTileInteriorTexels(
                (int) u,
                (int) v,
                3,
                3,
                (MilBitmapWrapMode) wrapMode);
        Direct3D9SoftwareBilinearTexelSelection fallback =
            Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(
                u,
                v,
                3,
                3,
                (MilBitmapWrapMode) wrapMode);

        Assert.AreEqual(interior, fallback);
    }

    [TestMethod]
    [DataRow(10, 20, 3, 0, 10, 20, 20, 30, 8u, 4u)]
    [DataRow(19, 29, -3, -4, 10, 20, 20, 30, 8u, 3u)]
    [DataRow(12, 22, 0, 0, 10, 20, 20, 30, 7u, 7u)]
    [DataRow(10, 29, -1, 1, 10, 20, 20, 30, 9u, 1u)]
    [DataRow(196608, 65536, 65536, 0, 196608, 327680, 0, 196608, 9u, 2u)]
    [DataRow(458752, 65536, -65536, 0, 327680, 524288, 0, 196608, 9u, 3u)]
    public void WhenCalculatingInteriorSpanLengthThenNearestBoundaryOrRemainingCountWins(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        int uMin,
        int uMax,
        int vMin,
        int vMax,
        uint count,
        uint expected)
    {
        uint length = Direct3D9SoftwareBilinearSpan.CalculateInteriorSpanLength(
            u,
            v,
            uIncrement,
            vIncrement,
            uMin,
            uMax,
            vMin,
            vMax,
            count);

        Assert.AreEqual(expected, length);
    }

    [TestMethod]
    [DataRow(10, 20, 3, 2, 10, 20, 20, 30, 8u)]
    [DataRow(19, 29, -3, -4, 10, 20, 20, 30, 8u)]
    [DataRow(458752, 327680, -65536, 65536, 327680, 524288, 327680, 524288, 9u)]
    public void WhenCalculatingInteriorSpanLengthThenLastPixelRemainsInsideCurrentRegion(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        int uMin,
        int uMax,
        int vMin,
        int vMax,
        uint count)
    {
        uint length = Direct3D9SoftwareBilinearSpan.CalculateInteriorSpanLength(
            u,
            v,
            uIncrement,
            vIncrement,
            uMin,
            uMax,
            vMin,
            vMax,
            count);
        long uLast = u + ((length - 1) * (long) uIncrement);
        long vLast = v + ((length - 1) * (long) vIncrement);

        Assert.IsTrue(length >= 1
            && length <= count
            && uLast >= uMin
            && uLast < uMax
            && vLast >= vMin
            && vLast < vMax);
    }

    [TestMethod]
    public void WhenCalculatingInteriorSpanLengthOutsideRegionThenRequestIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9SoftwareBilinearSpan.CalculateInteriorSpanLength(
                20,
                20,
                1,
                1,
                10,
                20,
                20,
                30,
                1));
    }

    [TestMethod]
    [DataRow(196608, 65536, 65536, 0, 9u, 2u, 327680, 65536, 7u)]
    [DataRow(458752, 327680, -65536, 65536, 9u, 3u, 262144, 524288, 6u)]
    [DataRow(196608, 327680, 0, 0, 4u, 4u, 196608, 327680, 0u)]
    public void WhenAdvancingInteriorSpanThenCoordinatesAndRemainingCountMatchNativeLoop(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        uint count,
        uint processedCount,
        int expectedU,
        int expectedV,
        uint expectedRemainingCount)
    {
        Direct3D9SoftwareBilinearInteriorAdvance advance =
            Direct3D9SoftwareBilinearSpan.AdvanceInteriorSpan(
                u,
                v,
                uIncrement,
                vIncrement,
                count,
                processedCount);

        Assert.AreEqual(
            (expectedU, expectedV, expectedRemainingCount),
            (advance.U, advance.V, advance.RemainingCount));
    }

    [TestMethod]
    [DataRow(327680, 4u, true, (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored)]
    [DataRow(262144, 4u, true, (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored)]
    [DataRow(196608, 4u, true, (int) Direct3D9SoftwareBilinearInteriorRegion.Boundary)]
    [DataRow(458752, 4u, true, (int) Direct3D9SoftwareBilinearInteriorRegion.Boundary)]
    [DataRow(131072, 4u, true, (int) Direct3D9SoftwareBilinearInteriorRegion.Forward)]
    [DataRow(262144, 4u, false, (int) Direct3D9SoftwareBilinearInteriorRegion.Boundary)]
    public void WhenClassifyingAdvancedCoordinateThenCanonicalHalfOrBoundaryIsReported(
        int coordinate,
        uint bitmapSize,
        bool flipEnabled,
        int expectedRegion)
    {
        Direct3D9SoftwareBilinearInteriorRegion region =
            Direct3D9SoftwareBilinearSpan.ClassifyInteriorRegion(coordinate, bitmapSize, flipEnabled);

        Assert.AreEqual((Direct3D9SoftwareBilinearInteriorRegion) expectedRegion, region);
    }

    [TestMethod]
    public void WhenInteriorSpanLeavesForwardHalfThenAdvancedCoordinateReclassifiesAtAdjacentBoundary()
    {
        uint length = Direct3D9SoftwareBilinearSpan.CalculateInteriorSpanLength(
            196608,
            65536,
            65536,
            0,
            196608,
            327680,
            0,
            196608,
            9);
        Direct3D9SoftwareBilinearInteriorAdvance advance =
            Direct3D9SoftwareBilinearSpan.AdvanceInteriorSpan(
                196608,
                65536,
                65536,
                0,
                9,
                length);

        Assert.AreEqual(
            Direct3D9SoftwareBilinearInteriorRegion.Mirrored,
            Direct3D9SoftwareBilinearSpan.ClassifyInteriorRegion(advance.U, 4, true));
    }

    [TestMethod]
    public void WhenInteriorSpanLeavesMirroredHalfThenAdvancedCoordinateReclassifiesAtCanonicalBoundary()
    {
        uint length = Direct3D9SoftwareBilinearSpan.CalculateInteriorSpanLength(
            393216,
            65536,
            -65536,
            0,
            262144,
            458752,
            0,
            196608,
            9);
        Direct3D9SoftwareBilinearInteriorAdvance advance =
            Direct3D9SoftwareBilinearSpan.AdvanceInteriorSpan(
                393216,
                65536,
                -65536,
                0,
                9,
                length);

        Assert.AreEqual(
            Direct3D9SoftwareBilinearInteriorRegion.Boundary,
            Direct3D9SoftwareBilinearSpan.ClassifyInteriorRegion(advance.U, 4, true));
    }

    [TestMethod]
    [DataRow(-65536, 65536, 4u, 4u, (int) MilBitmapWrapMode.Tile, 196608, 65536,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Boundary,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Forward, true)]
    [DataRow(524288, 327680, 4u, 4u, (int) MilBitmapWrapMode.FlipXY, 0, 327680,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Forward,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored, false)]
    [DataRow(-327680, -262144, 4u, 4u, (int) MilBitmapWrapMode.FlipXY, 196608, 262144,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Boundary,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored, true)]
    [DataRow(786432, 720896, 4u, 4u, (int) MilBitmapWrapMode.FlipXY, 262144, 196608,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Boundary, true)]
    public void WhenClassifyingLoopTopThenCanonicalUvStateControlsFallback(
        int u,
        int v,
        uint bitmapWidth,
        uint bitmapHeight,
        int wrapMode,
        int expectedU,
        int expectedV,
        int expectedURegion,
        int expectedVRegion,
        bool expectedRequiresFallback)
    {
        Direct3D9SoftwareBilinearLoopState state =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                u,
                v,
                bitmapWidth,
                bitmapHeight,
                (MilBitmapWrapMode) wrapMode);

        Assert.AreEqual(
            (expectedU,
                expectedV,
                (Direct3D9SoftwareBilinearInteriorRegion) expectedURegion,
                (Direct3D9SoftwareBilinearInteriorRegion) expectedVRegion,
                expectedRequiresFallback),
            (state.U, state.V, state.URegion, state.VRegion, state.RequiresFallback));
    }

    [TestMethod]
    public void WhenClassifyingNonTiledLoopTopThenRequestIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                0,
                0,
                4,
                4,
                MilBitmapWrapMode.Extend));
    }

    [TestMethod]
    [DataRow(65536, 131072, (int) MilBitmapWrapMode.FlipXY, 0, 196608, 0, 196608, false)]
    [DataRow(327680, 131072, (int) MilBitmapWrapMode.FlipXY, 262144, 458752, 0, 196608, true)]
    [DataRow(65536, 327680, (int) MilBitmapWrapMode.FlipXY, 0, 196608, 262144, 458752, true)]
    [DataRow(327680, 327680, (int) MilBitmapWrapMode.FlipXY, 262144, 458752, 262144, 458752, true)]
    public void WhenSelectingInteriorBoundsThenCanonicalRegionsChooseNativeRectangle(
        int u,
        int v,
        int wrapMode,
        int expectedUMinimum,
        int expectedUMaximum,
        int expectedVMinimum,
        int expectedVMaximum,
        bool expectedIsFlipped)
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                u,
                v,
                4,
                4,
                (MilBitmapWrapMode) wrapMode);
        Direct3D9SoftwareBilinearInteriorBounds bounds =
            Direct3D9SoftwareBilinearSpan.SelectInteriorBounds(loopState, 4, 4);

        Assert.AreEqual(
            (expectedUMinimum, expectedUMaximum, expectedVMinimum, expectedVMaximum, expectedIsFlipped),
            (bounds.UMinimum, bounds.UMaximum, bounds.VMinimum, bounds.VMaximum, bounds.IsFlipped));
    }

    [TestMethod]
    public void WhenSelectingInteriorBoundsThenBoundsFeedInteriorLengthCalculation()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                327680,
                65536,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        Direct3D9SoftwareBilinearInteriorBounds bounds =
            Direct3D9SoftwareBilinearSpan.SelectInteriorBounds(loopState, 4, 4);

        uint length = Direct3D9SoftwareBilinearSpan.CalculateInteriorSpanLength(
            loopState.U,
            loopState.V,
            65536,
            0,
            bounds.UMinimum,
            bounds.UMaximum,
            bounds.VMinimum,
            bounds.VMaximum,
            9);

        Assert.AreEqual(2u, length);
    }

    [TestMethod]
    public void WhenSelectingInteriorBoundsForBoundaryThenRequestIsRejected()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                196608,
                65536,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9SoftwareBilinearSpan.SelectInteriorBounds(loopState, 4, 4));
    }

    [TestMethod]
    [DataRow(65536, 65536, 65536, 0, 9u, 2u, false, 196608, 65536, 7u,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Boundary,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Forward, true)]
    [DataRow(327680, 65536, 65536, 0, 9u, 2u, true, 458752, 65536, 7u,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Boundary,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Forward, true)]
    [DataRow(65536, 327680, 0, -65536, 9u, 2u, true, 65536, 196608, 7u,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Forward,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Boundary, true)]
    [DataRow(327680, 327680, 0, 0, 3u, 3u, true, 327680, 327680, 0u,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored, false)]
    public void WhenCreatingInteriorBatchPlanThenNativeLoopStateIsProduced(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        uint count,
        uint expectedCount,
        bool expectedIsFlipped,
        int expectedU,
        int expectedV,
        uint expectedRemainingCount,
        int expectedURegion,
        int expectedVRegion,
        bool expectedRequiresFallback)
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                u,
                v,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Direct3D9SoftwareBilinearInteriorBatchPlan plan =
            Direct3D9SoftwareBilinearSpan.CreateInteriorBatchPlan(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                uIncrement,
                vIncrement,
                count);

        Assert.AreEqual(
            (expectedCount,
                expectedIsFlipped,
                expectedU,
                expectedV,
                expectedRemainingCount,
                (Direct3D9SoftwareBilinearInteriorRegion) expectedURegion,
                (Direct3D9SoftwareBilinearInteriorRegion) expectedVRegion,
                expectedRequiresFallback),
            (plan.Count,
                plan.IsFlipped,
                plan.U,
                plan.V,
                plan.RemainingCount,
                plan.NextLoopState.URegion,
                plan.NextLoopState.VRegion,
                plan.NextLoopState.RequiresFallback));
    }

    [TestMethod]
    public void WhenInteriorBatchAdvancesAcrossBoundaryGapThenAdjacentMirroredRegionIsReclassified()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                131072,
                65536,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Direct3D9SoftwareBilinearInteriorBatchPlan plan =
            Direct3D9SoftwareBilinearSpan.CreateInteriorBatchPlan(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                131072,
                0,
                4);

        Assert.AreEqual(
            (1u, 262144, 3u, Direct3D9SoftwareBilinearInteriorRegion.Mirrored, false),
            (plan.Count,
                plan.U,
                plan.RemainingCount,
                plan.NextLoopState.URegion,
                plan.NextLoopState.RequiresFallback));
    }

    [TestMethod]
    [DataRow(65664, 131328, 32768, -16384, 2u, 98432, 114944, 131200, 98560,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Forward,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Forward)]
    [DataRow(65664, 327936, 0, 16384, 3u, 65664, 360704, 65664, 377088,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Forward,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored)]
    [DataRow(327808, 65664, -16384, 0, 3u, 295040, 65664, 278656, 65664,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Forward)]
    [DataRow(327808, 327936, 16384, -16384, 3u, 360576, 295168, 376960, 278784,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored,
        (int) Direct3D9SoftwareBilinearInteriorRegion.Mirrored)]
    public void WhenCreatingInteriorSampleSliceThenNativeSamplingTrajectoryIsPreserved(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        uint count,
        int expectedLastU,
        int expectedLastV,
        int expectedAdvancedU,
        int expectedAdvancedV,
        int expectedURegion,
        int expectedVRegion)
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                u,
                v,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Direct3D9SoftwareBilinearInteriorSampleSlice slice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorSampleSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                uIncrement,
                vIncrement,
                count);

        Assert.AreEqual(
            (count,
                u,
                v,
                expectedLastU,
                expectedLastV,
                expectedAdvancedU,
                expectedAdvancedV,
                (Direct3D9SoftwareBilinearInteriorRegion) expectedURegion,
                (Direct3D9SoftwareBilinearInteriorRegion) expectedVRegion,
                true),
            (slice.Count,
                slice.FirstSample.U,
                slice.FirstSample.V,
                slice.LastSample.U,
                slice.LastSample.V,
                slice.AdvancedSample.U,
                slice.AdvancedSample.V,
                slice.LastSample.URegion,
                slice.LastSample.VRegion,
                slice.LastSample.U >= slice.Bounds.UMinimum
                    && slice.LastSample.U < slice.Bounds.UMaximum
                    && slice.LastSample.V >= slice.Bounds.VMinimum
                    && slice.LastSample.V < slice.Bounds.VMaximum));
    }

    [TestMethod]
    public void WhenInteriorSampleSliceReachesBoundaryThenLastAndAdvancedSamplesUseDifferentStates()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                131200,
                65664,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Direct3D9SoftwareBilinearInteriorSampleSlice slice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorSampleSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                32768,
                0,
                5);

        Assert.AreEqual(
            (2u, 163968, 65664, 196736, 65664,
                Direct3D9SoftwareBilinearInteriorRegion.Forward,
                Direct3D9SoftwareBilinearInteriorRegion.Boundary,
                false,
                true),
            (slice.Count,
                slice.LastSample.U,
                slice.LastSample.V,
                slice.AdvancedSample.U,
                slice.AdvancedSample.V,
                slice.LastSample.URegion,
                slice.AdvancedSample.URegion,
                slice.LastSample.RequiresFallback,
                slice.AdvancedSample.RequiresFallback));
    }

    [TestMethod]
    [DataRow(65664, 131328, 32768, -16384, 2u)]
    [DataRow(65664, 327936, 0, 16384, 3u)]
    [DataRow(327808, 65664, -16384, 0, 3u)]
    [DataRow(327808, 327936, 16384, -16384, 3u)]
    public void WhenCreatingInteriorTexelSelectionSliceThenFirstAndLastSamplesMatchNativeIteration(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        uint count)
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                u,
                v,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        Direct3D9SoftwareBilinearInteriorSampleSlice sampleSlice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorSampleSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                uIncrement,
                vIncrement,
                count);

        Direct3D9SoftwareBilinearInteriorTexelSelectionSlice selectionSlice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorTexelSelectionSlice(
                sampleSlice,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        (int iteratedU, int iteratedV) = Enumerable
            .Range(1, checked((int) sampleSlice.Count - 1))
            .Aggregate((U: u, V: v), (current, _) =>
                (current.U + uIncrement, current.V + vIncrement));

        Direct3D9SoftwareBilinearTexelSelection expectedFirst =
            Direct3D9SoftwareBilinearSpan.SelectFlippedTileInteriorTexels(
                u,
                v,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        Direct3D9SoftwareBilinearTexelSelection expectedLast =
            Direct3D9SoftwareBilinearSpan.SelectFlippedTileInteriorTexels(
                iteratedU,
                iteratedV,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Assert.AreEqual(
            (sampleSlice.Count,
                expectedFirst,
                expectedLast,
                (u >> 8) & 0xFF,
                (v >> 8) & 0xFF,
                (iteratedU >> 8) & 0xFF,
                (iteratedV >> 8) & 0xFF),
            (selectionSlice.Count,
                selectionSlice.FirstSelection,
                selectionSlice.LastSelection,
                selectionSlice.FirstSelection.XFraction,
                selectionSlice.FirstSelection.YFraction,
                selectionSlice.LastSelection.XFraction,
                selectionSlice.LastSelection.YFraction));
    }

    [TestMethod]
    [DataRow(65664, 131328, 32768, -16384, 2u)]
    [DataRow(65664, 327936, 0, 16384, 3u)]
    [DataRow(327808, 65664, -16384, 0, 3u)]
    [DataRow(327808, 327936, 16384, -16384, 3u)]
    public void WhenAssemblingInteriorArgbSliceThenSelectionOrderMatchesTileFallback(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        uint count)
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                u,
                v,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        Direct3D9SoftwareBilinearInteriorSampleSlice sampleSlice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorSampleSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                uIncrement,
                vIncrement,
                count);
        Direct3D9SoftwareBilinearInteriorTexelSelectionSlice selectionSlice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorTexelSelectionSlice(
                sampleSlice,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Direct3D9SoftwareBilinearTexelSelection first = selectionSlice.FirstSelection;
        Direct3D9SoftwareBilinearTexelSelection last = selectionSlice.LastSelection;
        Direct3D9SoftwareBilinearInteriorArgbSlice result =
            Direct3D9SoftwareBilinearSpan.AssembleInteriorArgbSlice(
                selectionSlice,
                GetTexelArgb(first.X1, first.Y1),
                GetTexelArgb(first.X2, first.Y1),
                GetTexelArgb(first.X1, first.Y2),
                GetTexelArgb(first.X2, first.Y2),
                GetTexelArgb(last.X1, last.Y1),
                GetTexelArgb(last.X2, last.Y1),
                GetTexelArgb(last.X1, last.Y2),
                GetTexelArgb(last.X2, last.Y2));
        uint expectedFirst = AssembleFallback(first);
        uint expectedLast = AssembleFallback(last);

        Assert.AreEqual(
            (sampleSlice.Count, expectedFirst, expectedLast),
            (result.Count, result.FirstArgb, result.LastArgb));

        static uint AssembleFallback(Direct3D9SoftwareBilinearTexelSelection selection) =>
            Direct3D9SoftwareBilinearSpan.Assemble64BitFallbackArgb(
                selection,
                GetTexelArgb(selection.X1, selection.Y1),
                GetTexelArgb(selection.X2, selection.Y1),
                GetTexelArgb(selection.X1, selection.Y2),
                GetTexelArgb(selection.X2, selection.Y2),
                0,
                MilBitmapWrapMode.FlipXY);

        static uint GetTexelArgb(int x, int y) =>
            0x40000000u | ((uint) (y * 4 + x + 1) * 0x00070401u);
    }

    [TestMethod]
    [DataRow(65664, 131328, 32768, -16384, 2u)]
    [DataRow(65664, 327936, 0, 16384, 3u)]
    [DataRow(327808, 65664, -16384, 0, 3u)]
    [DataRow(327808, 327936, 16384, -16384, 3u)]
    public void WhenCreatingInteriorArgbSliceThenCombinedResultMatchesNativeStages(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        uint count)
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                u,
                v,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        Direct3D9SoftwareBilinearInteriorSampleSlice sampleSlice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorSampleSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                uIncrement,
                vIncrement,
                count);
        Direct3D9SoftwareBilinearInteriorTexelSelectionSlice selectionSlice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorTexelSelectionSlice(
                sampleSlice,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        Direct3D9SoftwareBilinearTexelSelection first = selectionSlice.FirstSelection;
        Direct3D9SoftwareBilinearTexelSelection last = selectionSlice.LastSelection;
        uint firstA = GetTexelArgb(first.X1, first.Y1);
        uint firstB = GetTexelArgb(first.X2, first.Y1);
        uint firstC = GetTexelArgb(first.X1, first.Y2);
        uint firstD = GetTexelArgb(first.X2, first.Y2);
        uint lastA = GetTexelArgb(last.X1, last.Y1);
        uint lastB = GetTexelArgb(last.X2, last.Y1);
        uint lastC = GetTexelArgb(last.X1, last.Y2);
        uint lastD = GetTexelArgb(last.X2, last.Y2);
        Direct3D9SoftwareBilinearInteriorArgbSlice expected =
            Direct3D9SoftwareBilinearSpan.AssembleInteriorArgbSlice(
                selectionSlice,
                firstA,
                firstB,
                firstC,
                firstD,
                lastA,
                lastB,
                lastC,
                lastD);

        Direct3D9SoftwareBilinearInteriorArgbSlice result =
            Direct3D9SoftwareBilinearSpan.CreateInteriorArgbSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                uIncrement,
                vIncrement,
                count,
                firstA,
                firstB,
                firstC,
                firstD,
                lastA,
                lastB,
                lastC,
                lastD);

        Assert.AreEqual(expected, result);

        static uint GetTexelArgb(int x, int y) =>
            0x40000000u | ((uint) (y * 4 + x + 1) * 0x00070401u);
    }

    [TestMethod]
    public void WhenCreatingInteriorArgbSliceReachesBoundaryThenCountStopsAtLastInteriorSample()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                131200,
                65664,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Direct3D9SoftwareBilinearInteriorArgbSlice result =
            Direct3D9SoftwareBilinearSpan.CreateInteriorArgbSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                32768,
                0,
                5,
                0x40102030,
                0x40203040,
                0x40304050,
                0x40405060,
                0x40506070,
                0x40607080,
                0x40708090,
                0x408090A0);

        Assert.AreEqual(2u, result.Count);
    }

    [TestMethod]
    [DataRow(65664, 131328, 32768, -16384, 2u)]
    [DataRow(65664, 327936, 0, 16384, 3u)]
    [DataRow(327808, 65664, -16384, 0, 3u)]
    [DataRow(327808, 327936, 16384, -16384, 3u)]
    public void WhenCreatingInteriorArgbBatchThenArgbAndOuterAdvanceMatchExistingStages(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        uint count)
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                u,
                v,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        Direct3D9SoftwareBilinearInteriorSampleSlice sampleSlice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorSampleSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                uIncrement,
                vIncrement,
                count);
        Direct3D9SoftwareBilinearInteriorTexelSelectionSlice selectionSlice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorTexelSelectionSlice(
                sampleSlice,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        Direct3D9SoftwareBilinearTexelSelection first = selectionSlice.FirstSelection;
        Direct3D9SoftwareBilinearTexelSelection last = selectionSlice.LastSelection;
        Direct3D9SoftwareBilinearInteriorArgbSlice expectedArgb =
            Direct3D9SoftwareBilinearSpan.AssembleInteriorArgbSlice(
                selectionSlice,
                GetTexelArgb(first.X1, first.Y1),
                GetTexelArgb(first.X2, first.Y1),
                GetTexelArgb(first.X1, first.Y2),
                GetTexelArgb(first.X2, first.Y2),
                GetTexelArgb(last.X1, last.Y1),
                GetTexelArgb(last.X2, last.Y1),
                GetTexelArgb(last.X1, last.Y2),
                GetTexelArgb(last.X2, last.Y2));

        Direct3D9SoftwareBilinearInteriorArgbBatch result =
            Direct3D9SoftwareBilinearSpan.CreateInteriorArgbBatch(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                uIncrement,
                vIncrement,
                count,
                GetTexelArgb(first.X1, first.Y1),
                GetTexelArgb(first.X2, first.Y1),
                GetTexelArgb(first.X1, first.Y2),
                GetTexelArgb(first.X2, first.Y2),
                GetTexelArgb(last.X1, last.Y1),
                GetTexelArgb(last.X2, last.Y1),
                GetTexelArgb(last.X1, last.Y2),
                GetTexelArgb(last.X2, last.Y2));

        Assert.AreEqual(
            (expectedArgb.Count,
                expectedArgb.FirstArgb,
                expectedArgb.LastArgb,
                sampleSlice.AdvancedSample.U,
                sampleSlice.AdvancedSample.V,
                count - sampleSlice.Count,
                sampleSlice.AdvancedSample,
                Direct3D9SoftwareBilinearSpan.ClassifyNextBranch(
                    count - sampleSlice.Count,
                    sampleSlice.AdvancedSample)),
            (result.Count,
                result.FirstArgb,
                result.LastArgb,
                result.U,
                result.V,
                result.RemainingCount,
                result.NextLoopState,
                result.NextBranch));

        static uint GetTexelArgb(int x, int y) =>
            0x40000000u | ((uint) (y * 4 + x + 1) * 0x00070401u);
    }

    [TestMethod]
    public void WhenCreatingInteriorArgbBatchReachesBoundaryThenNextLoopStateRequiresFallback()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                131200,
                65664,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Direct3D9SoftwareBilinearInteriorArgbBatch result =
            Direct3D9SoftwareBilinearSpan.CreateInteriorArgbBatch(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                32768,
                0,
                5,
                0x40102030,
                0x40203040,
                0x40304050,
                0x40405060,
                0x40506070,
                0x40607080,
                0x40708090,
                0x408090A0);

        Assert.AreEqual(
            (2u, 196736, 65664, 3u, true, Direct3D9SoftwareBilinearNextBranch.Fallback),
            (result.Count,
                result.U,
                result.V,
                result.RemainingCount,
                result.NextLoopState.RequiresFallback,
                result.NextBranch));
    }

    [TestMethod]
    public void WhenCreatingInteriorArgbBatchConsumesRemainingPixelsThenNextBranchIsComplete()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                65664,
                65664,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Direct3D9SoftwareBilinearInteriorArgbBatch result =
            Direct3D9SoftwareBilinearSpan.CreateInteriorArgbBatch(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                16384,
                16384,
                2,
                0x40102030,
                0x40203040,
                0x40304050,
                0x40405060,
                0x40506070,
                0x40607080,
                0x40708090,
                0x408090A0);

        Assert.AreEqual(
            (0u, Direct3D9SoftwareBilinearNextBranch.Complete),
            (result.RemainingCount, result.NextBranch));
    }

    [TestMethod]
    public void WhenCreatingInteriorArgbBatchLeavesInteriorPixelsThenNextBranchIsInterior()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                65664,
                65664,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Direct3D9SoftwareBilinearInteriorArgbBatch result =
            Direct3D9SoftwareBilinearSpan.CreateInteriorArgbBatch(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                262144,
                0,
                3,
                0x40102030,
                0x40203040,
                0x40304050,
                0x40405060,
                0x40506070,
                0x40607080,
                0x40708090,
                0x408090A0);

        Assert.AreEqual(
            (2u, Direct3D9SoftwareBilinearNextBranch.Interior),
            (result.RemainingCount, result.NextBranch));
    }

    [TestMethod]
    public void WhenAdvancingFallbackBatchRemainsOnBoundaryThenNextBranchIsFallback()
    {
        Direct3D9SoftwareBilinearFallbackBatchAdvance result =
            Direct3D9SoftwareBilinearSpan.AdvanceFallbackBatch(
                -196608,
                65536,
                4,
                4,
                MilBitmapWrapMode.Extend,
                32768,
                0,
                5,
                1);

        Assert.AreEqual(
            (1u, -163840, 65536, 4u, Direct3D9SoftwareBilinearNextBranch.Fallback),
            (result.Count,
                result.U,
                result.V,
                result.RemainingCount,
                result.NextBranch));
    }

    [TestMethod]
    public void WhenAdvancingExtendFallbackBatchReachesTextureThenNextBranchIsInterior()
    {
        Direct3D9SoftwareBilinearFallbackBatchAdvance result =
            Direct3D9SoftwareBilinearSpan.AdvanceFallbackBatch(
                -32768,
                65536,
                4,
                4,
                MilBitmapWrapMode.Extend,
                32768,
                0,
                5,
                1);

        Assert.AreEqual(
            (1u, 0, 65536, 4u, Direct3D9SoftwareBilinearNextBranch.Interior),
            (result.Count,
                result.U,
                result.V,
                result.RemainingCount,
                result.NextBranch));
    }

    [TestMethod]
    [DataRow(196736, 196736, 32768, 32768, 262272, 262272)]
    [DataRow(196736, 262016, 32768, -32768, 262272, 196480)]
    [DataRow(262016, 196736, -32768, 32768, 196480, 262272)]
    [DataRow(262016, 262016, -32768, -32768, 196480, 196480)]
    public void WhenAdvancingFallbackBatchReachesInteriorThenNextBranchIsInterior(
        int u,
        int v,
        int uIncrement,
        int vIncrement,
        int expectedU,
        int expectedV)
    {
        Direct3D9SoftwareBilinearFallbackBatchAdvance result =
            Direct3D9SoftwareBilinearSpan.AdvanceFallbackBatch(
                u,
                v,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                uIncrement,
                vIncrement,
                5,
                2);

        Assert.AreEqual(
            (2u, expectedU, expectedV, 3u, Direct3D9SoftwareBilinearNextBranch.Interior),
            (result.Count,
                result.U,
                result.V,
                result.RemainingCount,
                result.NextBranch));
    }

    [TestMethod]
    public void WhenAdvancingFallbackBatchConsumesRemainingPixelsThenNextBranchIsComplete()
    {
        Direct3D9SoftwareBilinearFallbackBatchAdvance result =
            Direct3D9SoftwareBilinearSpan.AdvanceFallbackBatch(
                196736,
                196736,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                32768,
                32768,
                2,
                2);

        Assert.AreEqual(
            (0u, Direct3D9SoftwareBilinearNextBranch.Complete),
            (result.RemainingCount, result.NextBranch));
    }

    [TestMethod]
    public void WhenCreatingFallbackBatchSliceThenCandidateCountUsesNearestApproachingDimension()
    {
        Direct3D9SoftwareBilinearFallbackBatchSlice result =
            Direct3D9SoftwareBilinearSpan.CreateFallbackBatchSlice(
                -196608,
                327680,
                4,
                4,
                MilBitmapWrapMode.Extend,
                65536,
                -32768,
                10,
                2);

        Assert.AreEqual(
            (3u, 2u, -65536, 262144, 8u, Direct3D9SoftwareBilinearNextBranch.Fallback),
            (result.CandidateCount,
                result.Count,
                result.U,
                result.V,
                result.RemainingCount,
                result.NextBranch));
    }

    [TestMethod]
    public void WhenCreatingFallbackBatchSliceActualCountIsNarrowerThenAdvanceUsesActualCount()
    {
        Direct3D9SoftwareBilinearFallbackBatchSlice result =
            Direct3D9SoftwareBilinearSpan.CreateFallbackBatchSlice(
                -65536,
                65536,
                4,
                4,
                MilBitmapWrapMode.Border,
                32768,
                0,
                5,
                1);

        Assert.AreEqual(
            (2u, 1u, -32768, 65536, 4u, Direct3D9SoftwareBilinearNextBranch.Fallback),
            (result.CandidateCount,
                result.Count,
                result.U,
                result.V,
                result.RemainingCount,
                result.NextBranch));
    }

    [TestMethod]
    public void WhenCreatingFallbackBatchSliceCandidateReachesTextureThenNextBranchIsInterior()
    {
        Direct3D9SoftwareBilinearFallbackBatchSlice result =
            Direct3D9SoftwareBilinearSpan.CreateFallbackBatchSlice(
                -32768,
                65536,
                4,
                4,
                MilBitmapWrapMode.Extend,
                32768,
                0,
                5,
                1);

        Assert.AreEqual(
            (1u, 1u, 0, 65536, 4u, Direct3D9SoftwareBilinearNextBranch.Interior),
            (result.CandidateCount,
                result.Count,
                result.U,
                result.V,
                result.RemainingCount,
                result.NextBranch));
    }

    [TestMethod]
    public void WhenCreatingFallbackBatchSliceCandidateIsLimitedByRemainingCountThenCompletes()
    {
        Direct3D9SoftwareBilinearFallbackBatchSlice result =
            Direct3D9SoftwareBilinearSpan.CreateFallbackBatchSlice(
                -196608,
                65536,
                4,
                4,
                MilBitmapWrapMode.Extend,
                32768,
                0,
                2,
                2);

        Assert.AreEqual(
            (2u, 2u, 0u, Direct3D9SoftwareBilinearNextBranch.Complete),
            (result.CandidateCount, result.Count, result.RemainingCount, result.NextBranch));
    }

    [TestMethod]
    public void WhenCreatingFallbackBatchSliceActualCountExceedsCandidateThenRequestIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9SoftwareBilinearSpan.CreateFallbackBatchSlice(
                -32768,
                65536,
                4,
                4,
                MilBitmapWrapMode.Extend,
                32768,
                0,
                5,
                2));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void WhenCreatingFallbackRequestForTileOrFlipThenAllRemainingPixelsAreRequested(
        int wrapModeValue)
    {
        Direct3D9SoftwareBilinearFallbackRequestSlice result =
            Direct3D9SoftwareBilinearSpan.CreateFallbackRequestSlice(
                -32768,
                65536,
                4,
                4,
                (MilBitmapWrapMode) wrapModeValue,
                32768,
                0,
                5,
                2);

        Assert.AreEqual(
            (5u, 2u, 3u),
            (result.RequestedCount, result.Count, result.RemainingCount));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(5)]
    public void WhenCreatingFallbackRequestForExtendOrBorderThenEstimatedCountIsRequested(
        int wrapModeValue)
    {
        Direct3D9SoftwareBilinearFallbackRequestSlice result =
            Direct3D9SoftwareBilinearSpan.CreateFallbackRequestSlice(
                -65536,
                65536,
                4,
                4,
                (MilBitmapWrapMode) wrapModeValue,
                32768,
                0,
                5,
                1);

        Assert.AreEqual(
            (2u, 1u, -32768, 4u, Direct3D9SoftwareBilinearNextBranch.Fallback),
            (result.RequestedCount,
                result.Count,
                result.U,
                result.RemainingCount,
                result.NextBranch));
    }

    [TestMethod]
    public void WhenCreatingFallbackRequestActualCountExceedsSelectedRequestThenRequestIsRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9SoftwareBilinearSpan.CreateFallbackRequestSlice(
                -32768,
                65536,
                4,
                4,
                MilBitmapWrapMode.Border,
                32768,
                0,
                5,
                2));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void WhenTileOrFlipFallbackStopsWithRemainingPixelsOnBoundaryThenRequestIsRejected(
        int wrapModeValue)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9SoftwareBilinearSpan.CreateFallbackRequestSlice(
                196608,
                65536,
                4,
                4,
                (MilBitmapWrapMode) wrapModeValue,
                32768,
                0,
                3,
                1));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void WhenTileOrFlipFallbackConsumesAllPixelsThenBoundaryStopIsAccepted(
        int wrapModeValue)
    {
        Direct3D9SoftwareBilinearFallbackRequestSlice result =
            Direct3D9SoftwareBilinearSpan.CreateFallbackRequestSlice(
                196608,
                65536,
                4,
                4,
                (MilBitmapWrapMode) wrapModeValue,
                32768,
                0,
                1,
                1);

        Assert.AreEqual(
            (0u, Direct3D9SoftwareBilinearNextBranch.Complete),
            (result.RemainingCount, result.NextBranch));
    }

    [TestMethod]
    public void WhenSelectingAdvancedBoundaryAsInteriorTexelsThenRequestIsRejected()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                131200,
                65664,
                4,
                4,
                MilBitmapWrapMode.FlipXY);
        Direct3D9SoftwareBilinearInteriorSampleSlice sampleSlice =
            Direct3D9SoftwareBilinearSpan.CreateInteriorSampleSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                32768,
                0,
                5);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            Direct3D9SoftwareBilinearSpan.SelectFlippedTileInteriorTexels(
                sampleSlice.AdvancedSample.U,
                sampleSlice.AdvancedSample.V,
                4,
                4,
                MilBitmapWrapMode.FlipXY));
    }

    [TestMethod]
    public void WhenInteriorBatchAdvanceOverflowsAcceleratedCoordinatesThenRequestIsRejected()
    {
        Direct3D9SoftwareBilinearLoopState loopState =
            Direct3D9SoftwareBilinearSpan.ClassifyCanonicalLoopState(
                65536,
                65536,
                4,
                4,
                MilBitmapWrapMode.FlipXY);

        Assert.ThrowsExactly<OverflowException>(() =>
            Direct3D9SoftwareBilinearSpan.CreateInteriorSampleSlice(
                loopState,
                4,
                4,
                MilBitmapWrapMode.FlipXY,
                int.MaxValue,
                0,
                2));
    }

    [TestMethod]
    [DataRow(163840L, 65536L, 65536, 0, 229376L, 65536L, true)]
    [DataRow(196608L, 65536L, 65536, 0, 0L, 65536L, false)]
    public void WhenAdvancingFallbackTilePositionThenCanonicalStateControlsEarlyOut(
        long u,
        long v,
        int uIncrement,
        int vIncrement,
        long expectedU,
        long expectedV,
        bool expectedIsOnBorder)
    {
        Direct3D9SoftwareBilinearAdvance advance =
            Direct3D9SoftwareBilinearSpan.Advance64BitFallbackTilePosition(
                u,
                v,
                uIncrement,
                vIncrement,
                4,
                4,
                MilBitmapWrapMode.Tile);

        Assert.AreEqual(
            (expectedU, expectedV, expectedIsOnBorder),
            (advance.U, advance.V, advance.IsOnBorder));
    }

    [TestMethod]
    public void WhenAdvancingFallbackFlipPositionThenDoubledCanonicalRangeIsPreserved()
    {
        Direct3D9SoftwareBilinearAdvance advance =
            Direct3D9SoftwareBilinearSpan.Advance64BitFallbackTilePosition(
                458752,
                -65536,
                65536,
                0,
                4,
                2,
                MilBitmapWrapMode.FlipXY);

        Assert.AreEqual((0L, 196608L, true), (advance.U, advance.V, advance.IsOnBorder));
    }

    [TestMethod]
    [DataRow(0x10203040u, 0x50607080u, 0, 0x10203040u)]
    [DataRow(0x10203040u, 0x50607080u, 128, 0x30405060u)]
    [DataRow(0x10203040u, 0x50607080u, 255, 0x50607080u)]
    [DataRow(0x00000000u, 0x01010101u, 128, 0x01010101u)]
    public void WhenLinearlyInterpolatingArgbThenNativeChannelRoundingIsPreserved(
        uint first,
        uint second,
        int fraction,
        uint expected)
    {
        uint result = Direct3D9SoftwareBilinearSpan.InterpolateLinearArgb(first, second, fraction);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [DataRow(0x10203040u, 0x50607080u, 0x90A0B0C0u, 0xD0E0F000u, 0, 0, 0x10203040u)]
    [DataRow(0x10203040u, 0x50607080u, 0x90A0B0C0u, 0xD0E0F000u, 128, 128, 0x70809060u)]
    [DataRow(0x00000000u, 0x01010101u, 0x01010101u, 0x01010101u, 128, 128, 0x01010101u)]
    public void WhenBilinearlyInterpolatingArgbThenNativeChannelRoundingIsPreserved(
        uint a,
        uint b,
        uint c,
        uint d,
        int xFraction,
        int yFraction,
        uint expected)
    {
        uint result = Direct3D9SoftwareBilinearSpan.InterpolateBilinearArgb(
            a,
            b,
            c,
            d,
            xFraction,
            yFraction);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void WhenExtendClampsXThenBilinearOutputEqualsYLinearOutput()
    {
        const uint a = 0x80402010;
        const uint c = 0xC0604020;

        uint bilinear = Direct3D9SoftwareBilinearSpan.InterpolateBilinearArgb(a, a, c, c, 173, 91);
        uint linear = Direct3D9SoftwareBilinearSpan.InterpolateLinearArgb(a, c, 91);

        Assert.AreEqual(linear, bilinear);
    }

    [TestMethod]
    public void WhenExtendClampsYThenBilinearOutputEqualsXLinearOutput()
    {
        const uint a = 0x80402010;
        const uint b = 0xC0604020;

        uint bilinear = Direct3D9SoftwareBilinearSpan.InterpolateBilinearArgb(a, b, a, b, 173, 91);
        uint linear = Direct3D9SoftwareBilinearSpan.InterpolateLinearArgb(a, b, 173);

        Assert.AreEqual(linear, bilinear);
    }

    [TestMethod]
    public void WhenAssemblingBorderArgbThenOnlyInsideTexelsReplaceBorderColor()
    {
        Direct3D9SoftwareBilinearTexelSelection selection =
            Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(
                -32768,
                16384,
                3,
                2,
                MilBitmapWrapMode.Border);

        uint result = Direct3D9SoftwareBilinearSpan.Assemble64BitFallbackArgb(
            selection,
            0x10203040,
            0x50607080,
            0x90A0B0C0,
            0xD0E0F000,
            0x08040201,
            MilBitmapWrapMode.Border);
        uint expected = Direct3D9SoftwareBilinearSpan.InterpolateBilinearArgb(
            0x08040201,
            0x50607080,
            0x08040201,
            0xD0E0F000,
            128,
            64);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void WhenAssemblingCompletelyOutsideBorderArgbThenBorderColorIsReturned()
    {
        Direct3D9SoftwareBilinearTexelSelection selection =
            Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(
                -131072,
                -131072,
                3,
                2,
                MilBitmapWrapMode.Border);

        uint result = Direct3D9SoftwareBilinearSpan.Assemble64BitFallbackArgb(
            selection,
            0x10203040,
            0x50607080,
            0x90A0B0C0,
            0xD0E0F000,
            0x08040201,
            MilBitmapWrapMode.Border);

        Assert.AreEqual(0x08040201u, result);
    }

    [TestMethod]
    public void WhenAssemblingExtendArgbAfterXClampThenYLinearPathIsUsed()
    {
        Direct3D9SoftwareBilinearTexelSelection selection =
            Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(
                -32768,
                16384,
                3,
                2,
                MilBitmapWrapMode.Extend);

        uint result = Direct3D9SoftwareBilinearSpan.Assemble64BitFallbackArgb(
            selection,
            0x80402010,
            0x80402010,
            0xC0604020,
            0xC0604020,
            0,
            MilBitmapWrapMode.Extend);
        uint expected = Direct3D9SoftwareBilinearSpan.InterpolateLinearArgb(0x80402010, 0xC0604020, 64);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void WhenAssemblingExtendArgbAfterYClampThenXLinearPathIsUsed()
    {
        Direct3D9SoftwareBilinearTexelSelection selection =
            Direct3D9SoftwareBilinearSpan.Select64BitFallbackTexels(
                98304,
                131072,
                3,
                2,
                MilBitmapWrapMode.Extend);

        uint result = Direct3D9SoftwareBilinearSpan.Assemble64BitFallbackArgb(
            selection,
            0x80402010,
            0xC0604020,
            0x80402010,
            0xC0604020,
            0,
            MilBitmapWrapMode.Extend);
        uint expected = Direct3D9SoftwareBilinearSpan.InterpolateLinearArgb(0x80402010, 0xC0604020, 128);

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void WhenInterpolatingPremultipliedArgbThenChannelsRemainBoundedByAlpha()
    {
        uint result = Direct3D9SoftwareBilinearSpan.InterpolateBilinearArgb(
            0x80402010,
            0x40201008,
            0xC0603018,
            0x20100804,
            113,
            197);
        byte alpha = (byte) (result >> 24);

        Assert.IsTrue((byte) (result >> 16) <= alpha && (byte) (result >> 8) <= alpha && (byte) result <= alpha);
    }

    [TestMethod]
    [DataRow(16383u, 16383u, 0x3FFE0000L, 0L, 0u, 0, 0, false)]
    [DataRow(16384u, 16383u, 0L, 0L, 1u, 0, 0, true)]
    [DataRow(16383u, 16384u, 0L, 0L, 1u, 0, 0, true)]
    [DataRow(16383u, 16383u, 0x3FFE0001L, 0L, 0u, 0, 0, true)]
    [DataRow(16383u, 16383u, -0x3FFE0001L, 0L, 0u, 0, 0, true)]
    [DataRow(16383u, 16383u, 0x3FFD0000L, 0L, 2u, 0x10000, 0, true)]
    [DataRow(16383u, 16383u, 0L, -0x3FFD0000L, 2u, 0, -0x10000, true)]
    public void WhenSelectingBilinearPathThenLargeTextureOrSpanUses64BitFallback(
        uint width,
        uint height,
        long u,
        long v,
        uint count,
        int uIncrement,
        int vIncrement,
        bool expected)
    {
        bool result = Direct3D9SoftwareBilinearSpan.Requires64BitFallback(
            width,
            height,
            u,
            v,
            count,
            uIncrement,
            vIncrement);

        Assert.AreEqual(expected, result);
    }

    private sealed class TestModuleHandle : SafeHandle
    {
        internal TestModuleHandle()
            : base(0, true)
        {
            SetHandle(1);
        }

        internal int ReleaseCount { get; private set; }

        public override bool IsInvalid => false;

        protected override bool ReleaseHandle()
        {
            ReleaseCount++;
            handle = 0;
            return true;
        }
    }
}
