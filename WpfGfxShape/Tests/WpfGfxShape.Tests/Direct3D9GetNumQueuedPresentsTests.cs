using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9GetNumQueuedPresentsTests
{
    [TestMethod]
    public unsafe void WhenDisplayIsInvalidThenSurfaceTargetReturnsZeroWithoutDeviceEntry()
    {
        int queryCalls = 0;
        Caps9 capabilities = default;
        Displaymode displayMode = new(format: Format.X8R8G8B8);
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            displayMode: displayMode,
            getNumQueuedPresents: (out uint queuedPresentCount) =>
            {
                queryCalls++;
                queuedPresentCount = 2;
                return 0;
            });
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            pixelFormat: MilPixelFormat.Bgr32Bpp101010,
            width: 16,
            height: 12);

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual(
            (0, 0u, 0, false, false),
            (result, queuedPresentCount, queryCalls, device.IsEntered(), renderTarget.IsValid));
    }

    [TestMethod]
    public unsafe void WhenTextureBackedTargetIsValidThenSurfaceSemanticsForwardOnceInsideDeviceEntry()
    {
        int queryCalls = 0;
        Direct3D9Device? queriedDevice = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            getNumQueuedPresents: (out uint queuedPresentCount) =>
            {
                queryCalls++;
                queuedPresentCount = 3;
                Assert.IsTrue(queriedDevice!.IsEntered());
                return Direct3D9Factory.GenericFailureHResult;
            });
        queriedDevice = device;
        using Direct3D9Surface textureLevelSurface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: textureLevelSurface,
            width: 16,
            height: 12);

        int result = renderTarget.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 3u, 1, false),
            (result, queuedPresentCount, queryCalls, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenGpuMarkersHaveNotBeenTestedThenQueryReturnsZeroWithoutCreatingQueries()
    {
        int createdQueries = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                createdQueries++;
                query = new Direct3D9GpuQuery(() => 0, _ => 0);
                return 0;
            });

        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 0u, 0), (result, queuedPresentCount, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenGpuMarkersAreDisabledThenQueryReturnsZeroWithoutRetryingCreation()
    {
        int createdQueries = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                createdQueries++;
                query = null;
                return Direct3D9Factory.GenericFailureHResult;
            });
        _ = device.InsertGpuMarker(1);

        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 0u, 1), (result, queuedPresentCount, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenMoreThanTwoMarkersRemainAfterUnflushedScanThenNewestMarkerIsCheckedAgainWithFlush()
    {
        List<bool> flushes = [];
        int createdQueries = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
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
                            return flush ? 0 : 1;
                        });
                return 0;
            });
        _ = device.InsertGpuMarker(1);
        _ = device.InsertGpuMarker(2);
        _ = device.InsertGpuMarker(3);

        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);

        CollectionAssert.AreEqual(new[] { false, false, false, true }, flushes);
        Assert.AreEqual((0, 0u, 4), (result, queuedPresentCount, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenMarkerStatusFailsThenFirstQueryStillReturnsSuccessWithZeroOutputAndDisablesMarkers()
    {
        int statusCalls = 0;
        int disposedQueries = 0;
        int createdQueries = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                createdQueries++;
                query = createdQueries == 1
                    ? new Direct3D9GpuQuery(() => 0, _ => 0)
                    : new Direct3D9GpuQuery(
                        () => 0,
                        _ =>
                        {
                            statusCalls++;
                            return Direct3D9Factory.GenericFailureHResult;
                        },
                        () => disposedQueries++);
                return 0;
            });
        _ = device.InsertGpuMarker(1);

        int firstResult = device.GetNumQueuedPresents(out uint firstQueuedPresentCount);
        int secondResult = device.GetNumQueuedPresents(out uint secondQueuedPresentCount);

        Assert.AreEqual(
            (0, 0u, 0, 0u, 1, 1),
            (firstResult, firstQueuedPresentCount, secondResult, secondQueuedPresentCount, statusCalls, disposedQueries));
    }

    [TestMethod]
    public unsafe void WhenDeviceSupportsWddmThenActiveMarkersAreNotQueriedOrReported()
    {
        int createdQueries = 0;
        int statusCalls = 0;
        Caps9 capabilities = default;
        capabilities.Caps2 = D3D9.Caps2Canshareresource;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: capabilities,
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                createdQueries++;
                query = new Direct3D9GpuQuery(
                    () => 0,
                    _ =>
                    {
                        statusCalls++;
                        return 0;
                    });
                return 0;
            });
        _ = device.InsertGpuMarker(1);

        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 0u, 2, 0), (result, queuedPresentCount, createdQueries, statusCalls));
    }

    [TestMethod]
    public unsafe void WhenNoMarkerHasBeenConsumedThenActiveMarkerCountIsNotReported()
    {
        int createdQueries = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createGpuQuery: (out Direct3D9GpuQuery? query) =>
            {
                createdQueries++;
                query = createdQueries == 1
                    ? new Direct3D9GpuQuery(() => 0, _ => 0)
                    : new Direct3D9GpuQuery(() => 0, _ => 1);
                return 0;
            });
        _ = device.InsertGpuMarker(1);

        int result = device.GetNumQueuedPresents(out uint queuedPresentCount);

        Assert.AreEqual((0, 0u, 2), (result, queuedPresentCount, createdQueries));
    }

    [TestMethod]
    public unsafe void WhenDeviceIsDisposedThenQueuedPresentQueryRejectsUseAfterRepeatedDispose()
    {
        Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));
        device.Dispose();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.GetNumQueuedPresents(out _));
    }

    private static unsafe Direct3D9Surface CreateSurface()
    {
        return new Direct3D9Surface(new Direct3D9ResourceManager(), null);
    }
}
