using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9GpuMarkerTests
{
    [TestMethod]
    public void WhenMarkerHasNotBeenIssuedThenStatusIsNotConsumedWithoutQuerying()
    {
        int getDataCalls = 0;
        using Direct3D9GpuMarker marker = new(
            new Direct3D9GpuQuery(
                () => 0,
                _ =>
                {
                    getDataCalls++;
                    return 0;
                }),
            7);

        int result = marker.CheckStatus(false, out bool consumed);

        Assert.AreEqual((0, false, 0), (result, consumed, getDataCalls));
    }

    [TestMethod]
    public void WhenQueryReturnsFalseThenMarkerRemainsUnconsumed()
    {
        using Direct3D9GpuMarker marker = new(
            new Direct3D9GpuQuery(() => 0, _ => 1),
            7);
        _ = marker.InsertIntoCommandStream();

        int result = marker.CheckStatus(false, out bool consumed);

        Assert.AreEqual((0, false), (result, consumed));
    }

    [TestMethod]
    public void WhenMarkerIsConsumedThenLaterChecksUseCachedStatus()
    {
        int getDataCalls = 0;
        using Direct3D9GpuMarker marker = new(
            new Direct3D9GpuQuery(
                () => 0,
                _ =>
                {
                    getDataCalls++;
                    return 0;
                }),
            7);
        _ = marker.InsertIntoCommandStream();

        _ = marker.CheckStatus(false, out _);
        int result = marker.CheckStatus(true, out bool consumed);

        Assert.AreEqual((0, true, 1), (result, consumed, getDataCalls));
    }

    [TestMethod]
    public unsafe void WhenNewestMarkerIsConsumedThenItAndItsPredecessorsAreReused()
    {
        Queue<int>[] statuses =
        [
            new([1]),
            new([0, 1]),
            new([1, 1])
        ];
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            if (createdQueries == 0)
            {
                createdQueries++;
                query = new Direct3D9GpuQuery(() => 0, _ => 0);
                return 0;
            }

            Queue<int> queryStatuses = statuses[createdQueries - 1];
            createdQueries++;
            query = new Direct3D9GpuQuery(() => 0, _ => queryStatuses.Dequeue());
            return 0;
        });
        _ = device.InsertGpuMarker(1);
        _ = device.InsertGpuMarker(2);
        _ = device.InsertGpuMarker(3);

        int firstResult = device.GetNumQueuedPresents(out uint firstCount);
        _ = device.InsertGpuMarker(4);
        int secondResult = device.GetNumQueuedPresents(out uint secondCount);

        Assert.AreEqual((0, 1u, 0, 2u, 4), (firstResult, firstCount, secondResult, secondCount, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenPresentTimestampFailsThenCountIsRetainedAndMarkerIsNotInserted()
    {
        const int timestampFailure = unchecked((int)0x8007001F);
        int timestampCalls = 0;
        int createdQueries = 0;
        List<bool> flushes = [];
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getPresentTimestamp: (out ulong timestamp) =>
            {
                timestampCalls++;
                timestamp = 0;
                return timestampCalls <= 3 ? timestampFailure : 0;
            },
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                createdQueries++;
                query = createdQueries == 1
                    ? new Direct3D9GpuQuery(() => 0, _ => 0)
                    : new Direct3D9GpuQuery(
                        () => 0,
                        flush =>
                        {
                            flushes.Add(flush);
                            return 1;
                        });
                return 0;
            });

        int firstResult = device.RecordSuccessfulPresent();
        int secondResult = device.RecordSuccessfulPresent();
        int thirdResult = device.RecordSuccessfulPresent();
        int fourthResult = device.RecordSuccessfulPresent();
        _ = device.GetNumQueuedPresents(out _);

        Assert.AreEqual(
            (timestampFailure, timestampFailure, timestampFailure, 0, 4, 2, true),
            (firstResult, secondResult, thirdResult, fourthResult, timestampCalls, createdQueries, flushes.Single()));
    }

    [TestMethod]
    public unsafe void WhenPresentTimestampSucceedsThenMarkerIsInsertedAfterTimestamp()
    {
        List<string> operations = [];
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getPresentTimestamp: (out ulong timestamp) =>
            {
                operations.Add("timestamp");
                timestamp = 7;
                return 0;
            },
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                query = new Direct3D9GpuQuery(
                    () =>
                    {
                        operations.Add("issue");
                        return 0;
                    },
                    _ => 0);
                return 0;
            });
        operations.Clear();

        int result = device.RecordSuccessfulPresent();

        CollectionAssert.AreEqual(new[] { "timestamp", "issue" }, operations);
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public unsafe void WhenGpuThrottlingIsDisabledThenPresentTimestampIsNotRequested()
    {
        int timestampCalls = 0;
        int createdQueries = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getPresentTimestamp: (out ulong timestamp) =>
            {
                timestampCalls++;
                timestamp = 0;
                return Direct3D9Factory.GenericFailureHResult;
            },
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                createdQueries++;
                query = null;
                return 0;
            },
            gpuThrottlingDisabled: true);

        int result = device.RecordSuccessfulPresent();

        Assert.AreEqual((0, 0, 0), (result, timestampCalls, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenThreePresentsSucceededThenOnlyNewestMarkerCheckFlushes()
    {
        List<bool> flushes = [];
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            query = createdQueries == 1
                ? new Direct3D9GpuQuery(() => 0, _ => 0)
                : new Direct3D9GpuQuery(
                    () => 0,
                    flush =>
                    {
                        flushes.Add(flush);
                        return 1;
                    });
            return 0;
        });
        _ = device.RecordSuccessfulPresent(1);
        _ = device.RecordSuccessfulPresent(2);
        _ = device.RecordSuccessfulPresent(3);

        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);

        CollectionAssert.AreEqual(new[] { true, false, false }, flushes);
        Assert.AreEqual((0, 0u), (result, queuedPresentCount));
    }

    [TestMethod]
    public unsafe void WhenInitialQueryCreationFailsWithQueryThenTemporaryQueryIsDisposedAndMarkersAreDisabled()
    {
        int disposedQueries = 0;
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            query = new Direct3D9GpuQuery(
                () => 0,
                _ => 0,
                () => disposedQueries++);
            return Direct3D9Factory.GenericFailureHResult;
        });

        int firstResult = device.InsertGpuMarker(1);
        int secondResult = device.InsertGpuMarker(2);
        int queuedResult = device.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual(
            (0, 0, 0, 0u, 1, 1),
            (firstResult, secondResult, queuedResult, queuedPresentCount, createdQueries, disposedQueries));
    }

    [TestMethod]
    public unsafe void WhenMarkerQueryCreationFailsWithQueryThenTemporaryQueryIsDisposedAndExistingMarkersAreReleased()
    {
        int disposedQueries = 0;
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            query = new Direct3D9GpuQuery(
                () => 0,
                _ => 1,
                () => disposedQueries++);
            return createdQueries == 3 ? Direct3D9Factory.GenericFailureHResult : 0;
        });
        _ = device.InsertGpuMarker(1);

        int result = device.InsertGpuMarker(2);
        int queuedResult = device.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual(
            (0, 0, 0u, 3, 3),
            (result, queuedResult, queuedPresentCount, createdQueries, disposedQueries));
    }

    [TestMethod]
    public unsafe void WhenQueryStatusFailsThenMarkersAreDisabledAndDisposed()
    {
        int disposedQueries = 0;
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            query = createdQueries == 1
                ? new Direct3D9GpuQuery(() => 0, _ => 0)
                : new Direct3D9GpuQuery(
                    () => 0,
                    _ => Direct3D9Factory.GenericFailureHResult,
                    () => disposedQueries++);
            return 0;
        });
        _ = device.InsertGpuMarker(1);

        int firstResult = device.GetNumQueuedPresents(out uint firstCount);
        _ = device.InsertGpuMarker(2);
        int secondResult = device.GetNumQueuedPresents(out uint secondCount);

        Assert.AreEqual((0, 0u, 1, 0, 0u, 2), (firstResult, firstCount, disposedQueries, secondResult, secondCount, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenForcedNewestMarkerIsConsumedThenPresentCountKeepsNextCheckForced()
    {
        List<bool> flushes = [];
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            query = createdQueries == 1
                ? new Direct3D9GpuQuery(() => 0, _ => 0)
                : new Direct3D9GpuQuery(
                    () => 0,
                    flush =>
                    {
                        flushes.Add(flush);
                        return 0;
                    });
            return 0;
        });
        _ = device.RecordSuccessfulPresent(1);
        _ = device.RecordSuccessfulPresent(2);
        _ = device.RecordSuccessfulPresent(3);

        _ = device.GetNumQueuedPresents(out _);
        _ = device.RecordSuccessfulPresent(4);
        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);

        CollectionAssert.AreEqual(new[] { true, true }, flushes);
        Assert.AreEqual((0, 0u, 4), (result, queuedPresentCount, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenNewestMarkerQueryFailsThenScanningStopsAndAllMarkersAreDisposed()
    {
        List<ulong> queriedMarkerIds = [];
        int disposedQueries = 0;
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            ulong markerId = (ulong)(createdQueries - 1);
            query = createdQueries == 1
                ? new Direct3D9GpuQuery(() => 0, _ => 0)
                : new Direct3D9GpuQuery(
                    () => 0,
                    _ =>
                    {
                        queriedMarkerIds.Add(markerId);
                        return markerId == 3 ? Direct3D9Factory.GenericFailureHResult : 1;
                    },
                    () => disposedQueries++);
            return 0;
        });
        _ = device.InsertGpuMarker(1);
        _ = device.InsertGpuMarker(2);
        _ = device.InsertGpuMarker(3);

        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);
        _ = device.InsertGpuMarker(4);

        CollectionAssert.AreEqual(new ulong[] { 3 }, queriedMarkerIds);
        Assert.AreEqual((0, 0u, 3, 4), (result, queuedPresentCount, disposedQueries, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenNewestMarkerReportsDeviceLostThenItAndItsPredecessorsAreReused()
    {
        List<ulong> queriedMarkerIds = [];
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            ulong markerId = (ulong)(createdQueries - 1);
            query = createdQueries == 1
                ? new Direct3D9GpuQuery(() => 0, _ => 0)
                : new Direct3D9GpuQuery(
                    () => 0,
                    _ =>
                    {
                        queriedMarkerIds.Add(markerId);
                        return markerId == 3 ? Direct3D9Factory.DeviceLostHResult : 1;
                    });
            return 0;
        });
        _ = device.InsertGpuMarker(1);
        _ = device.InsertGpuMarker(2);
        _ = device.InsertGpuMarker(3);

        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);
        _ = device.InsertGpuMarker(4);

        CollectionAssert.AreEqual(new ulong[] { 3 }, queriedMarkerIds);
        Assert.AreEqual((0, 0u, 4), (result, queuedPresentCount, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenMarkerQueryCreationReportsDeviceLostThenMarkersRemainEnabled()
    {
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            query = createdQueries == 2
                ? null
                : new Direct3D9GpuQuery(() => 0, _ => 1);
            return createdQueries == 2 ? Direct3D9Factory.DeviceLostHResult : 0;
        });

        int firstResult = device.InsertGpuMarker(1);
        int secondResult = device.InsertGpuMarker(2);

        Assert.AreEqual((0, 0, 3), (firstResult, secondResult, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenReusedMarkerIssueReportsNotAvailableThenItIsDisposedOnceAndMarkersRemainEnabled()
    {
        int disposedQueries = 0;
        int createdQueries = 0;
        int markerIssueCalls = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            query = createdQueries == 1
                ? new Direct3D9GpuQuery(() => 0, _ => 0)
                : new Direct3D9GpuQuery(
                    () => ++markerIssueCalls == 2 ? Direct3D9Factory.NotAvailableHResult : 0,
                    _ => 0,
                    () => disposedQueries++);
            return 0;
        });
        _ = device.InsertGpuMarker(1);
        _ = device.GetNumQueuedPresents(out _);

        int failedResult = device.InsertGpuMarker(2);
        int nextResult = device.InsertGpuMarker(3);

        Assert.AreEqual((0, 0, 3, 1), (failedResult, nextResult, createdQueries, disposedQueries));
    }

    [TestMethod]
    public unsafe void WhenMarkerIssueFailsUnexpectedlyThenTemporaryMarkerIsReleasedBeforeExistingMarkersAreDisabled()
    {
        List<string> releases = [];
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            string queryName = createdQueries switch
            {
                1 => "probe",
                2 => "active",
                _ => "temporary"
            };
            query = new Direct3D9GpuQuery(
                () => queryName == "temporary" ? Direct3D9Factory.GenericFailureHResult : 0,
                _ => 1,
                () => releases.Add(queryName));
            return 0;
        });
        _ = device.InsertGpuMarker(1);

        int failedResult = device.InsertGpuMarker(2);
        int nextResult = device.InsertGpuMarker(3);

        CollectionAssert.AreEqual(new[] { "probe", "temporary", "active" }, releases);
        Assert.AreEqual((0, 0, 3), (failedResult, nextResult, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenMarkersAreDisabledThenLastIdStillRejectsOnlyLowerIds()
    {
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            query = null;
            return Direct3D9Factory.GenericFailureHResult;
        });

        int firstResult = device.InsertGpuMarker(10);
        int lowerResult = device.InsertGpuMarker(9);
        int equalResult = device.InsertGpuMarker(10);
        int higherResult = device.InsertGpuMarker(11);

        Assert.AreEqual((0, 0, 0, 0, 1), (firstResult, lowerResult, equalResult, higherResult, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenActiveMarkerBacklogExceedsLimitThenMarkersAreDisabled()
    {
        int disposedQueries = 0;
        int createdQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            createdQueries++;
            query = new Direct3D9GpuQuery(() => 0, _ => 1, () => disposedQueries++);
            return 0;
        });

        for (ulong id = 1; id <= 36; id++)
        {
            _ = device.InsertGpuMarker(id);
        }

        _ = device.InsertGpuMarker(37);
        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 0u, 37, 37), (result, queuedPresentCount, disposedQueries, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenDeviceBecomesUnusableThenMarkersAreReleasedBeforeNotification()
    {
        int disposedQueries = 0;
        int disposedQueriesAtNotification = -1;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            unusableNotification: _ => disposedQueriesAtNotification = disposedQueries,
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                query = new Direct3D9GpuQuery(() => 0, _ => 1, () => disposedQueries++);
                return 0;
            });
        _ = device.InsertGpuMarker(1);
        _ = device.InsertGpuMarker(2);

        device.MarkUnusable();

        Assert.AreEqual((3, 3), (disposedQueriesAtNotification, disposedQueries));
    }

    [TestMethod]
    public unsafe void WhenUnusableDeviceIsMarkedAgainThenMarkersAreNotReleasedAgain()
    {
        int disposedQueries = 0;
        using Direct3D9Device device = CreateDevice((out Direct3D9GpuQuery? query) =>
        {
            query = new Direct3D9GpuQuery(() => 0, _ => 1, () => disposedQueries++);
            return 0;
        });
        _ = device.InsertGpuMarker(1);

        device.MarkUnusable();
        device.MarkUnusable();

        Assert.AreEqual(2, disposedQueries);
    }

    private static unsafe Direct3D9Device CreateDevice(Direct3D9CreateGpuQuery createGpuQuery)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createGpuQuery: createGpuQuery);
    }
}
