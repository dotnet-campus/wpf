using System.Collections.Immutable;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DisplaySetTests
{
    private static readonly Direct3D9DisplayBounds PrimaryBounds = new(
        0,
        new Direct3D9SurfaceRect(0, 0, 1920, 1080));
    private static readonly Direct3D9Display PrimaryDisplay = new(
        0,
        42,
        1,
        [PrimaryBounds],
        "DISPLAY1",
        1,
        new Direct3D9DisplaySettings(
            Direct3D9PixelGeometry.Rgb,
            2.2f,
            0.5f,
            1.0f,
            Direct3D9RenderingMode.ClearType),
        512,
        true,
        false,
        0x1414,
        0x008C,
        new Direct3D9DisplayMode(24, 1920, 1080, 60, Format.X8R8G8B8, Scanlineordering.Progressive),
        Displayrotation.DisplayrotationIdentity,
        new Direct3D9GraphicsAccelerationCaps(2, 1, 0x300, 0x300, 8192, 8192, 1, 32, 1, 512));
    private static readonly Direct3D9DisplaySetCharacteristics Characteristics = new(
        127437408000000000,
        2,
        false,
        [PrimaryBounds],
        [PrimaryDisplay]);

    [TestMethod]
    public void WhenBothUniquenessValuesMatchThenDisplaySetIsUpToDate()
    {
        Direct3D9DisplaySet displaySet = new(Characteristics, 3, 5, () => 3, () => 5);

        Assert.IsTrue(displaySet.IsUpToDate());
    }

    [DataTestMethod]
    [DataRow(4u, 5u)]
    [DataRow(3u, 6u)]
    public void WhenEitherUniquenessValueChangesThenDisplaySetIsOutOfDate(
        uint currentDisplayUniqueness,
        uint currentExternalUpdateCount)
    {
        Direct3D9DisplaySet displaySet = new(
            Characteristics,
            3,
            5,
            () => currentDisplayUniqueness,
            () => currentExternalUpdateCount);

        Assert.IsFalse(displaySet.IsUpToDate());
    }

    [TestMethod]
    public void WhenImmutableDisplayCharacteristicsMatchThenDisplaySetsAreEquivalent()
    {
        Direct3D9DisplaySet first = new(Characteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplaySet second = new(Characteristics, 3, 4, () => 3, () => 4);

        Assert.IsTrue(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenImmutableDisplayCharacteristicsDifferThenDisplaySetsAreNotEquivalent()
    {
        Direct3D9DisplaySet first = new(Characteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplaySet second = new(
            Characteristics with { Direct3DAdapterCount = 3 },
            1,
            2,
            () => 1,
            () => 2);

        Assert.IsFalse(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenDisplayBoundsHaveSameDpiMappingsInDifferentOrderThenDisplaySetsAreEquivalent()
    {
        Direct3D9DisplayBounds secondaryBounds = new(
            -4,
            new Direct3D9SurfaceRect(1920, 0, 3840, 1080));
        Direct3D9DisplaySetCharacteristics firstCharacteristics = Characteristics with
        {
            DisplayBounds = [PrimaryBounds, secondaryBounds],
            Displays = [PrimaryDisplay with { Bounds = [PrimaryBounds, secondaryBounds] }]
        };
        Direct3D9DisplaySetCharacteristics secondCharacteristics = Characteristics with
        {
            DisplayBounds = [secondaryBounds, PrimaryBounds],
            Displays = [PrimaryDisplay with { Bounds = [secondaryBounds, PrimaryBounds] }]
        };
        Direct3D9DisplaySet first = new(firstCharacteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplaySet second = new(secondCharacteristics, 3, 4, () => 3, () => 4);

        Assert.IsTrue(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenDisplayBoundsDifferThenDisplaySetsAreNotEquivalent()
    {
        Direct3D9DisplaySet first = new(Characteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplaySet second = new(
            Characteristics with
            {
                DisplayBounds = [PrimaryBounds with { Bounds = new Direct3D9SurfaceRect(0, 0, 2560, 1440) }]
            },
            3,
            4,
            () => 3,
            () => 4);

        Assert.IsFalse(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenDisplayMemorySizeDiffersThenDisplaySetsAreNotEquivalent()
    {
        Direct3D9DisplaySet first = new(Characteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplaySet second = new(
            Characteristics with
            {
                Displays = [PrimaryDisplay with { MemorySize = PrimaryDisplay.MemorySize + 1 }]
            },
            3,
            4,
            () => 3,
            () => 4);

        Assert.IsFalse(first.IsEquivalentTo(second));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public void WhenDisplaySettingsDifferThenDisplaySetsAreNotEquivalent(int field)
    {
        Direct3D9DisplaySettings settings = field switch
        {
            0 => PrimaryDisplay.Settings with { PixelStructure = Direct3D9PixelGeometry.Bgr },
            1 => PrimaryDisplay.Settings with { Gamma = 1.8f },
            2 => PrimaryDisplay.Settings with { ClearTypeLevel = 0.75f },
            _ => PrimaryDisplay.Settings with { DisplayRenderingMode = Direct3D9RenderingMode.Grayscale }
        };
        Direct3D9DisplaySet first = new(Characteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplaySet second = CreateDisplaySet(PrimaryDisplay with { Settings = settings });

        Assert.IsFalse(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenEitherDisplayLacksRenderingParametersThenDisplaySetsAreNotEquivalent()
    {
        Direct3D9DisplaySet first = new(Characteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplaySettings settings = PrimaryDisplay.Settings with { HasRenderingParameters = false };
        Direct3D9DisplaySet second = CreateDisplaySet(PrimaryDisplay with { Settings = settings });

        Assert.IsFalse(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenDisplayModeSizeIsNonZeroThenGetModeReturnsCachedMode()
    {
        Direct3D9DisplaySet displaySet = new(Characteristics, 1, 2, () => 1, () => 2);

        int result = displaySet.GetMode(0, out Direct3D9DisplayMode displayMode, out Displayrotation displayRotation);

        Assert.AreEqual((Direct3D9Factory.SuccessHResult, PrimaryDisplay.DisplayMode, PrimaryDisplay.DisplayRotation),
            (result, displayMode, displayRotation));
    }

    [TestMethod]
    public void WhenDisplayModeSizeIsZeroThenGetModeReturnsNotInitialized()
    {
        Direct3D9DisplayMode uninitialized = PrimaryDisplay.DisplayMode with { Size = 0 };
        Direct3D9DisplaySet displaySet = CreateDisplaySet(PrimaryDisplay with { DisplayMode = uninitialized });

        int result = displaySet.GetMode(0, out Direct3D9DisplayMode displayMode, out Displayrotation displayRotation);

        Assert.AreEqual((Direct3D9Factory.NotInitializedHResult, uninitialized, PrimaryDisplay.DisplayRotation),
            (result, displayMode, displayRotation));
    }

    [TestMethod]
    public void WhenAdapterIsOutOfRangeThenGetModeReturnsDisplayStateInvalid()
    {
        Direct3D9DisplaySet displaySet = new(Characteristics, 1, 2, () => 1, () => 2);

        int result = displaySet.GetMode(1, out Direct3D9DisplayMode displayMode, out Displayrotation displayRotation);

        Assert.AreEqual((Direct3D9Factory.DisplayStateInvalidHResult, default(Direct3D9DisplayMode), default(Displayrotation)),
            (result, displayMode, displayRotation));
    }

    [TestMethod]
    public void WhenDisplayModeDiffersThenDisplaySetsAreNotEquivalent()
    {
        Direct3D9DisplaySet first = new(Characteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplayMode mode = PrimaryDisplay.DisplayMode with { RefreshRate = 75 };
        Direct3D9DisplaySet second = CreateDisplaySet(PrimaryDisplay with { DisplayMode = mode });

        Assert.IsFalse(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenDisplayRotationDiffersThenDisplaySetsAreNotEquivalent()
    {
        Direct3D9DisplaySet first = new(Characteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplaySet second = CreateDisplaySet(
            PrimaryDisplay with { DisplayRotation = Displayrotation.Displayrotation90 });

        Assert.IsFalse(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenGraphicsAccelerationCapsDifferThenDisplaySetsAreNotEquivalent()
    {
        Direct3D9DisplaySet first = new(Characteristics, 1, 2, () => 1, () => 2);
        Direct3D9GraphicsAccelerationCaps caps = PrimaryDisplay.GraphicsAccelerationCaps with
        {
            MaxTextureWidth = PrimaryDisplay.GraphicsAccelerationCaps.MaxTextureWidth + 1
        };
        Direct3D9DisplaySet second = CreateDisplaySet(PrimaryDisplay with { GraphicsAccelerationCaps = caps });

        Assert.IsFalse(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenDisplaysHaveSameValuesInDifferentOrderThenDisplaySetsAreNotEquivalent()
    {
        Direct3D9Display secondaryDisplay = PrimaryDisplay with
        {
            DisplayIndex = 1,
            Direct3DAdapterLuid = 43,
            MonitorHandle = 2,
            DeviceName = "DISPLAY2"
        };
        Direct3D9DisplaySetCharacteristics firstCharacteristics = Characteristics with
        {
            Displays = [PrimaryDisplay, secondaryDisplay]
        };
        Direct3D9DisplaySetCharacteristics secondCharacteristics = Characteristics with
        {
            Displays = [secondaryDisplay, PrimaryDisplay]
        };
        Direct3D9DisplaySet first = new(firstCharacteristics, 1, 2, () => 1, () => 2);
        Direct3D9DisplaySet second = new(secondCharacteristics, 3, 4, () => 3, () => 4);

        Assert.IsFalse(first.IsEquivalentTo(second));
    }

    [TestMethod]
    public void WhenEquivalentDisplaySetSuppliesNewUniquenessThenCurrentSetBecomesUpToDate()
    { 
        uint displayUniqueness = 3;
        uint externalUpdateCount = 4;
        Direct3D9DisplaySet current = new(
            Characteristics,
            1,
            2,
            () => displayUniqueness,
            () => externalUpdateCount);
        Direct3D9DisplaySet replacement = new(
            Characteristics,
            displayUniqueness,
            externalUpdateCount,
            () => displayUniqueness,
            () => externalUpdateCount);

        current.UpdateUniqueness(replacement);

        Assert.IsTrue(current.IsUpToDate());
    }

    [TestMethod]
    public void WhenOutOfDateSetRemainsLatestThenDisplayStateIsUnchanged()
    {
        uint displayUniqueness = 1;
        Direct3D9DisplaySet? latest = null;
        Direct3D9DisplaySet displaySet = new(
            Characteristics,
            displayUniqueness,
            2,
            () => displayUniqueness,
            () => 2,
            () => latest);
        latest = displaySet;
        displayUniqueness++;

        Assert.IsFalse(displaySet.DangerousHasDisplayStateChanged());
    }

    [TestMethod]
    public void WhenOutOfDateSetIsReplacedThenDisplayStateHasChanged()
    {
        uint displayUniqueness = 1;
        Direct3D9DisplaySet latest = null!;
        Direct3D9DisplaySet displaySet = new(
            Characteristics,
            displayUniqueness,
            2,
            () => displayUniqueness,
            () => 2,
            () => latest);
        latest = new Direct3D9DisplaySet();
        displayUniqueness++;

        Assert.IsTrue(displaySet.DangerousHasDisplayStateChanged());
    }

    private static Direct3D9DisplaySet CreateDisplaySet(Direct3D9Display display)
    {
        return new Direct3D9DisplaySet(
            Characteristics with { Displays = [display] },
            3,
            4,
            () => 3,
            () => 4);
    }
}
