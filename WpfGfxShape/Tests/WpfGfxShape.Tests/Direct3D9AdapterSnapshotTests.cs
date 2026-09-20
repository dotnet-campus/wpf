using System.Collections.Immutable;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9AdapterSnapshotTests
{
    [TestMethod]
    public void WhenDirect3DAdapterOrderDiffersThenDisplaysAreReorderedAndReindexed()
    {
        Direct3D9AdapterArrangementResult result = Direct3D9AdapterSnapshotFactory.Arrange(
            Devices("first", "second", "third"),
            Monitors(("first", 10), ("second", 20), ("third", 30)),
            isMultiAdapterCodeEnabled: true,
            () => 2,
            adapter => adapter == 0 ? 20 : 10,
            (uint adapter, out long luid) =>
            {
                luid = 100 + adapter;
                return 0;
            });

        CollectionAssert.AreEqual(
            new[]
            {
                (0u, "second", (nint) 20, 100L),
                (1u, "first", (nint) 10, 101L),
                (2u, "third", (nint) 30, 0L),
            },
            result.Displays.Select(display =>
                (display.DisplayIndex, display.DeviceName, display.MonitorHandle, display.Direct3DAdapterLuid)).ToArray());
    }

    [TestMethod]
    public void WhenMultiAdapterCodeIsDisabledThenOnlyFirstDisplayIsAssociated()
    {
        List<uint> monitorRequests = [];
        Direct3D9AdapterArrangementResult result = Direct3D9AdapterSnapshotFactory.Arrange(
            Devices("first"),
            Monitors(("first", 10)),
            isMultiAdapterCodeEnabled: false,
            () => 3,
            adapter =>
            {
                monitorRequests.Add(adapter);
                return 999;
            },
            (uint _, out long luid) =>
            {
                luid = 42;
                return 0;
            });

        Assert.AreEqual((1u, 10, 42L, 1),
            (result.Direct3DAdapterCount, result.Displays[0].MonitorHandle, result.Displays[0].Direct3DAdapterLuid, monitorRequests.Count));
    }

    [TestMethod]
    public void WhenDirect3DReportsMoreAdaptersThanDisplaysThenDisplayStateInvalidIsReported()
    {
        COMException exception = Assert.ThrowsExactly<COMException>(() => Direct3D9AdapterSnapshotFactory.Arrange(
            Devices("first"),
            Monitors(("first", 10)),
            isMultiAdapterCodeEnabled: true,
            () => 2,
            _ => 10,
            Direct3D9AdapterSnapshotFactory.CreateLocallyUniqueLuidProvider()));

        Assert.AreEqual(Direct3D9Factory.DisplayStateInvalidHResult, exception.HResult);
    }

    [TestMethod]
    public void WhenDirect3DMonitorIsUnknownThenDisplayStateInvalidIsReported()
    {
        COMException exception = Assert.ThrowsExactly<COMException>(() => Direct3D9AdapterSnapshotFactory.Arrange(
            Devices("first"),
            Monitors(("first", 10)),
            isMultiAdapterCodeEnabled: true,
            () => 1,
            _ => 20,
            Direct3D9AdapterSnapshotFactory.CreateLocallyUniqueLuidProvider()));

        Assert.AreEqual(Direct3D9Factory.DisplayStateInvalidHResult, exception.HResult);
    }

    [TestMethod]
    public void WhenDirect3DReturnsNullMonitorThenDisplayRemainsUnassociated()
    {
        int luidCalls = 0;
        Direct3D9AdapterArrangementResult result = Direct3D9AdapterSnapshotFactory.Arrange(
            Devices("first"),
            Monitors(("first", 10)),
            isMultiAdapterCodeEnabled: true,
            () => 1,
            _ => 0,
            (uint _, out long luid) =>
            {
                luidCalls++;
                luid = 42;
                return 0;
            });

        Assert.AreEqual((0L, 0), (result.Displays[0].Direct3DAdapterLuid, luidCalls));
    }

    [TestMethod]
    public void WhenAdapterLuidQueryFailsThenFailureIsIgnoredAndReturnedValueIsKept()
    {
        Direct3D9AdapterArrangementResult result = Direct3D9AdapterSnapshotFactory.Arrange(
            Devices("first"),
            Monitors(("first", 10)),
            isMultiAdapterCodeEnabled: true,
            () => 1,
            _ => 10,
            (uint _, out long luid) =>
            {
                luid = 77;
                return unchecked((int) 0x80004005);
            });

        Assert.AreEqual(77, result.Displays[0].Direct3DAdapterLuid);
    }

    [TestMethod]
    public void WhenLocalLuidProviderIsUsedThenAdaptersReceiveUniqueValuesWithinDisplaySet()
    {
        Direct3D9AdapterArrangementResult result = Direct3D9AdapterSnapshotFactory.Arrange(
            Devices("first", "second"),
            Monitors(("first", 10), ("second", 20)),
            isMultiAdapterCodeEnabled: true,
            () => 2,
            adapter => adapter == 0 ? 10 : 20,
            Direct3D9AdapterSnapshotFactory.CreateLocallyUniqueLuidProvider());

        Assert.AreEqual(2, result.Displays.Select(display => display.Direct3DAdapterLuid).Distinct().Count());
    }

    private static Direct3D9DisplayDeviceEnumerationResult Devices(params string[] names)
    {
        return new Direct3D9DisplayDeviceEnumerationResult(
            names.Select(name => new Direct3D9DisplayDeviceSnapshot(name, 1)).ToImmutableArray(),
            false);
    }

    private static ImmutableArray<Direct3D9MonitorSnapshot> Monitors(params (string Name, nint Handle)[] monitors)
    {
        return monitors.Select(monitor => new Direct3D9MonitorSnapshot(
            monitor.Name,
            monitor.Handle,
            [new Direct3D9DisplayBounds(0, new Direct3D9SurfaceRect(0, 0, 100, 100))])).ToImmutableArray();
    }
}
