using System.Collections.Immutable;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9MonitorSnapshotTests
{
    [TestMethod]
    public void WhenMonitorsAreEnumeratedThenDevicesReceiveHandlesAndBoundsForEveryDpiContext()
    {
        ImmutableArray<Direct3D9DisplayDeviceSnapshot> devices =
        [
            new("first", 1),
            new("second", 1),
        ];

        ImmutableArray<Direct3D9MonitorSnapshot> result = Direct3D9MonitorSnapshotFactory.Enumerate(
            devices,
            [-1, -2],
            (dpiContext, callback) =>
            {
                callback(Entry("unknown", 30, new(0, 0, 1, 1)));
                callback(Entry("second", 20, new(20, 0, 40, 20)));
                callback(Entry("first", 10, dpiContext == -1 ? new(0, 0, 20, 20) : new(0, 0, 40, 40)));
            });

        CollectionAssert.AreEqual(
            new[]
            {
                ("first", (nint) 10, 2),
                ("second", (nint) 20, 2),
            },
            result.Select(snapshot => (snapshot.DeviceName, snapshot.MonitorHandle, snapshot.Bounds.Length)).ToArray());
    }

    [TestMethod]
    public void WhenMonitorIsMissingThenDisplayStateInvalidIsReported()
    {
        COMException exception = Assert.ThrowsExactly<COMException>(() => Direct3D9MonitorSnapshotFactory.Enumerate(
            [new Direct3D9DisplayDeviceSnapshot("missing", 1)],
            [-1],
            (_, _) => { }));

        Assert.AreEqual(Direct3D9Factory.DisplayStateInvalidHResult, exception.HResult);
    }

    [TestMethod]
    public void WhenDeviceReceivesDuplicateBoundsForDpiContextThenDisplayStateInvalidIsReported()
    {
        COMException exception = Assert.ThrowsExactly<COMException>(() => Direct3D9MonitorSnapshotFactory.Enumerate(
            [new Direct3D9DisplayDeviceSnapshot("display", 1)],
            [-1],
            (_, callback) =>
            {
                callback(Entry("display", 10, new(0, 0, 20, 20)));
                callback(Entry("display", 10, new(0, 0, 20, 20)));
            }));

        Assert.AreEqual(Direct3D9Factory.DisplayStateInvalidHResult, exception.HResult);
    }

    [TestMethod]
    public void WhenMonitorDeviceNameIsInvalidThenInvalidArgumentIsReported()
    {
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => Direct3D9MonitorSnapshotFactory.Enumerate(
            [new Direct3D9DisplayDeviceSnapshot("display", 1)],
            [-1],
            (_, callback) => callback(new Direct3D9MonitorEnumerationEntry(10, new char[32], new(0, 0, 20, 20)))));

        Assert.AreEqual(unchecked((int) 0x80070057), exception.HResult);
    }

    [TestMethod]
    public void WhenMonitorEnumeratorReportsFailureThenItsHResultIsPreserved()
    {
        const int failure = unchecked((int) 0x8007001F);

        COMException exception = Assert.ThrowsExactly<COMException>(() => Direct3D9MonitorSnapshotFactory.Enumerate(
            [new Direct3D9DisplayDeviceSnapshot("display", 1)],
            [-1],
            (_, _) => Marshal.ThrowExceptionForHR(failure)));

        Assert.AreEqual(failure, exception.HResult);
    }

    private static Direct3D9MonitorEnumerationEntry Entry(
        string deviceName,
        nint monitorHandle,
        Direct3D9SurfaceRect bounds)
    {
        char[] buffer = new char[32];
        deviceName.AsSpan().CopyTo(buffer);
        return new Direct3D9MonitorEnumerationEntry(monitorHandle, buffer, bounds);
    }
}
