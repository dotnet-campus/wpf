using System.Collections.Immutable;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9DisplaySetFactoryTests
{
    private static readonly Direct3D9DisplaySetCharacteristics Characteristics = new(
        127437408000000000,
        0,
        false,
        ImmutableArray<Direct3D9DisplayBounds>.Empty,
        ImmutableArray<Direct3D9Display>.Empty);

    [TestMethod]
    public void WhenDirect3DInitializationFailsThenDisplaySetCreationContinuesWithFailureResult()
    {
        const int initializationFailure = Direct3D9Factory.NotAvailableHResult;

        using Direct3D9DisplaySet displaySet = Direct3D9DisplaySetFactory.Create(
            1,
            2,
            () => 1,
            () => 2,
            () => new Direct3D9DisplaySetInitialization(initializationFailure, null),
            initialization =>
            {
                Assert.AreEqual(initializationFailure, initialization.HResult);
                return Characteristics;
            });

        Assert.AreEqual((initializationFailure, false), (displaySet.Direct3DInitializationHResult, displaySet.HasDirect3D9Ex));
    }

    [TestMethod]
    public void WhenLaterInitializationFailsThenDirect3DInitializationOwnerIsDisposed()
    {
        TrackingDisposable owner = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => Direct3D9DisplaySetFactory.Create(
            1,
            2,
            () => 1,
            () => 2,
            () => new Direct3D9DisplaySetInitialization(0, null, owner),
            _ => throw new InvalidOperationException("Enumeration failed.")));

        Assert.IsTrue(owner.IsDisposed);
    }

    [TestMethod]
    public void WhenSnapshotStagesRunThenInitializationCarriesTheirResultsInNativeOrder()
    {
        List<string> stages = [];
        Direct3D9DisplayDeviceEnumerationResult devices = new(
            [new Direct3D9DisplayDeviceSnapshot("display", 1)],
            true);
        ImmutableArray<Direct3D9MonitorSnapshot> monitors =
        [new("display", 10, [new Direct3D9DisplayBounds(-1, new(0, 0, 20, 20))])];

        using Direct3D9DisplaySet displaySet = Direct3D9DisplaySetFactory.Create(
            1,
            2,
            () => 1,
            () => 2,
            () => new Direct3D9DisplaySetInitialization(0, null),
            initialization =>
            {
                stages.Add("devices");
                return devices;
            },
            initialization =>
            {
                stages.Add($"monitors:{initialization.DisplayDevices.Devices.Single().DeviceName}");
                return monitors;
            },
            initialization =>
            {
                stages.Add($"adapters:{initialization.Monitors.Single().MonitorHandle}");
                return new Direct3D9AdapterArrangementResult(
                    1,
                    [new(0, 42, 10, initialization.Monitors.Single().Bounds, "display", 1)]);
            },
            initialization =>
            {
                stages.Add($"modes:{initialization.Adapters.Displays.Single().Direct3DAdapterLuid}");
                return [new Direct3D9DisplayModeSnapshot(0, default, 0, 32)];
            },
            initialization =>
            {
                stages.Add($"settings:{initialization.DisplayModes.Single().BitsPerPixel}");
                return new Direct3D9DisplaySettingsSnapshotResult(
                    default,
                    [new Direct3D9CompiledDisplaySettings(
                        new Direct3D9DisplaySettings(Direct3D9PixelGeometry.Rgb, 1.8f, 0.5f, 1.0f, Direct3D9RenderingMode.ClearType),
                        true,
                        default)]);
            },
            initialization =>
            {
                stages.Add($"characteristics:{initialization.DisplaySettings.Displays.Single().Settings.Gamma}");
                return Characteristics;
            });

        CollectionAssert.AreEqual(
            new[] { "devices", "monitors:display", "adapters:10", "modes:42", "settings:32", "characteristics:1.8" },
            stages);
    }

    [TestMethod]
    public void WhenMonitorSnapshotStageFailsThenDirect3DInitializationOwnerIsDisposed()
    {
        TrackingDisposable owner = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => Direct3D9DisplaySetFactory.Create(
            1,
            2,
            () => 1,
            () => 2,
            () => new Direct3D9DisplaySetInitialization(0, null, owner),
            _ => default,
            _ => throw new InvalidOperationException("Monitor enumeration failed."),
            _ => Characteristics));

        Assert.IsTrue(owner.IsDisposed);
    }

    [TestMethod]
    public void WhenAdapterSnapshotStageFailsThenDirect3DInitializationOwnerIsDisposed()
    {
        TrackingDisposable owner = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => Direct3D9DisplaySetFactory.Create(
            1,
            2,
            () => 1,
            () => 2,
            () => new Direct3D9DisplaySetInitialization(0, null, owner),
            _ => default,
            _ => [],
            _ => throw new InvalidOperationException("Adapter arrangement failed."),
            _ => Characteristics));

        Assert.IsTrue(owner.IsDisposed);
    }

    [TestMethod]
    public void WhenDisplayModeSnapshotStageFailsThenDirect3DInitializationOwnerIsDisposed()
    {
        TrackingDisposable owner = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => Direct3D9DisplaySetFactory.Create(
            1,
            2,
            () => 1,
            () => 2,
            () => new Direct3D9DisplaySetInitialization(0, null, owner),
            _ => default,
            _ => [],
            _ => default,
            _ => throw new InvalidOperationException("Display mode read failed."),
            _ => Characteristics));

        Assert.IsTrue(owner.IsDisposed);
    }

    [TestMethod]
    public void WhenDisplaySettingsSnapshotStageFailsThenDirect3DInitializationOwnerIsDisposed()
    {
        TrackingDisposable owner = new();

        Assert.ThrowsExactly<InvalidOperationException>(() => Direct3D9DisplaySetFactory.Create(
            1,
            2,
            () => 1,
            () => 2,
            () => new Direct3D9DisplaySetInitialization(0, null, owner),
            _ => default,
            _ => [],
            _ => default,
            _ => [],
            _ => throw new InvalidOperationException("Display settings read failed."),
            _ => Characteristics));

        Assert.IsTrue(owner.IsDisposed);
    }

    [TestMethod]
    public void WhenCharacteristicsAreCreatedThenSnapshotsBecomeCompleteDisplaysAndBoundsAreUnioned()
    {
        Direct3D9DisplaySettings firstSettings = new(
            Direct3D9PixelGeometry.Rgb,
            1.8f,
            0.5f,
            1.0f,
            Direct3D9RenderingMode.ClearType);
        Direct3D9DisplaySettings secondSettings = new(
            Direct3D9PixelGeometry.Flat,
            2.2f,
            0.25f,
            0.0f,
            Direct3D9RenderingMode.Grayscale);
        using Direct3D9DisplaySetInitialization initialization = new(0, null);
        initialization.SetDisplayDevices(new Direct3D9DisplayDeviceEnumerationResult(
            [new("display1", 1), new("display2", 2)],
            true));
        initialization.SetMonitors(
        [
            new("display1", 10, [new(-1, new(-100, 0, 100, 100)), new(-2, new(0, 0, 200, 200))]),
            new("display2", 20, [new(-1, new(100, -50, 300, 150)), new(-2, new(200, 0, 400, 300))])
        ]);
        initialization.SetAdapters(new Direct3D9AdapterArrangementResult(
            1,
            [
                new(0, 42, 10, initialization.Monitors[0].Bounds, "display1", 1),
                new(1, 84, 20, initialization.Monitors[1].Bounds, "display2", 2)
            ]));
        initialization.SetDisplayModes(
        [
            new(0, new(40, 1920, 1080, 60, Format.X8R8G8B8, Scanlineordering.Progressive), Displayrotation.DisplayrotationIdentity, 32),
            new(1, new(40, 1280, 1024, 75, Format.R5G6B5, Scanlineordering.Interlaced), Displayrotation.Displayrotation90, 16)
        ]);
        initialization.SetDisplaySettings(new Direct3D9DisplaySettingsSnapshotResult(
            default,
            [new(firstSettings, true, default), new(secondSettings, false, default)]));

        Direct3D9DisplaySetCharacteristics characteristics = Direct3D9DisplaySetFactory.CreateCharacteristics(initialization);

        Assert.AreEqual(
            (127437408000000000UL, 1U, true, 2, 2),
            (characteristics.RequiredVideoDriverDate,
             characteristics.Direct3DAdapterCount,
             characteristics.IsNonLocalDevicePresent,
             characteristics.DisplayBounds.Length,
             characteristics.Displays.Length));
        Assert.AreEqual(new Direct3D9SurfaceRect(-100, -50, 300, 150), characteristics.DisplayBounds[0].Bounds);
        Assert.AreEqual(new Direct3D9SurfaceRect(0, 0, 400, 300), characteristics.DisplayBounds[1].Bounds);
        Assert.AreEqual(
            (1U, 84L, (nint) 20, "display2", 2U, secondSettings, 16U),
            (characteristics.Displays[1].DisplayIndex,
             characteristics.Displays[1].Direct3DAdapterLuid,
             characteristics.Displays[1].MonitorHandle,
             characteristics.Displays[1].DeviceName,
             characteristics.Displays[1].StateFlags,
             characteristics.Displays[1].Settings,
             characteristics.Displays[1].GraphicsAccelerationCaps.BitsPerPixel));
        Assert.AreEqual(initialization.DisplayModes[1].DisplayMode, characteristics.Displays[1].DisplayMode);
        Assert.AreEqual(Displayrotation.Displayrotation90, characteristics.Displays[1].DisplayRotation);
    }

    [TestMethod]
    public void WhenCharacteristicSnapshotCountsDifferThenDisplayStateIsInvalid()
    {
        TrackingDisposable owner = new();
        Direct3D9DisplaySetInitialization initialization = new(0, null, owner);
        initialization.SetAdapters(new Direct3D9AdapterArrangementResult(
            0,
            [new(0, 0, 10, [], "display", 1)]));
        initialization.SetDisplayModes([]);
        initialization.SetDisplaySettings(new Direct3D9DisplaySettingsSnapshotResult(default, []));

        System.Runtime.InteropServices.COMException exception = Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
            () => Direct3D9DisplaySetFactory.CreateCharacteristics(initialization));
        initialization.Dispose();

        Assert.AreEqual(Direct3D9Factory.DisplayStateInvalidHResult, exception.HResult);
        Assert.IsTrue(owner.IsDisposed);
    }

    [TestMethod]
    public void WhenDisplaySetIsDisposedThenDirect3DInitializationOwnerIsDisposedOnce()
    {
        TrackingDisposable owner = new();
        Direct3D9DisplaySet displaySet = Direct3D9DisplaySetFactory.Create(
            1,
            2,
            () => 1,
            () => 2,
            () => new Direct3D9DisplaySetInitialization(0, null, owner),
            _ => Characteristics);

        displaySet.Dispose();
        displaySet.Dispose();

        Assert.AreEqual(1, owner.DisposeCount);
    }

    private sealed class TrackingDisposable : IDisposable
    {
        internal int DisposeCount { get; private set; }

        internal bool IsDisposed => DisposeCount > 0;

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}
