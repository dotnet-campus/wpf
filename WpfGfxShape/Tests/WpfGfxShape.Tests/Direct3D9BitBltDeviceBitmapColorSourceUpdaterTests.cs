using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9BitBltDeviceBitmapColorSourceUpdaterTests
{
    [TestMethod]
    public void WhenUpdateSucceedsThenDeviceContextsAreReleasedBeforeStretching()
    {
        using Direct3D9Device device = CreateDevice();
        List<string> calls = [];
        Direct3D9BitBltDeviceBitmapColorSourceUpdater updater = CreateUpdater(device, calls);
        Direct3D9BitmapRealizationRectangle[] dirtyRectangles =
        [
            new(1, 2, 11, 12),
            new(20, 21, 30, 31)
        ];

        int result = updater.Update(dirtyRectangles, 13);

        Assert.AreEqual(
            string.Join('|',
                $"result:{Direct3D9Factory.SuccessHResult}",
                "get-dc:13",
                "get-dc:17",
                "bitblt:23:Direct3D9BitmapRealizationRectangle { Left = 1, Top = 2, Right = 11, Bottom = 12 }:19:1:2",
                "bitblt:23:Direct3D9BitmapRealizationRectangle { Left = 20, Top = 21, Right = 30, Bottom = 31 }:19:20:21",
                "release-dc:13:19",
                "release-dc:17:23",
                "get-destination",
                "stretch:17:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 11, Bottom = 12 }:29:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 11, Bottom = 12 }:TexfNone",
                "stretch:17:Direct3D9SurfaceRect { Left = 20, Top = 21, Right = 30, Bottom = 31 }:29:Direct3D9SurfaceRect { Left = 20, Top = 21, Right = 30, Bottom = 31 }:TexfNone",
                "release-surface:29"),
            string.Join('|', [$"result:{result}", .. calls]));
    }

    [TestMethod]
    public void WhenSecondBitBltFailsThenFirstFailurePropagatesAndBothDeviceContextsAreReleased()
    {
        using Direct3D9Device device = CreateDevice();
        List<string> calls = [];
        int bitBltCalls = 0;
        Direct3D9BitBltDeviceBitmapColorSourceUpdater updater = CreateUpdater(
            device,
            calls,
            bitBlt: (_, rectangle, _, _, _) =>
            {
                bitBltCalls++;
                calls.Add($"bitblt:{rectangle.Left}");
                return bitBltCalls == 2
                    ? Direct3D9Factory.InvalidCallHResult
                    : Direct3D9Factory.SuccessHResult;
            });

        int result = updater.Update([new(1, 0, 2, 1), new(3, 0, 4, 1)], 13);

        Assert.AreEqual(
            $"{Direct3D9Factory.InvalidCallHResult}|get-dc:13|get-dc:17|bitblt:1|bitblt:3|release-dc:13:19|release-dc:17:23",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenSourceDeviceContextReleaseFailsThenFailurePropagatesAndCleanupRetriesWithoutStretching()
    {
        using Direct3D9Device device = CreateDevice();
        List<string> calls = [];
        int sourceReleaseCalls = 0;
        Direct3D9BitBltDeviceBitmapColorSourceUpdater updater = CreateUpdater(
            device,
            calls,
            releaseDeviceContext: (surface, deviceContext) =>
            {
                calls.Add($"release-dc:{surface}:{deviceContext}");
                if (surface == 13)
                {
                    sourceReleaseCalls++;
                    return sourceReleaseCalls == 1
                        ? Direct3D9Factory.InvalidCallHResult
                        : Direct3D9Factory.SuccessHResult;
                }

                return Direct3D9Factory.SuccessHResult;
            });

        int result = updater.Update([new(1, 2, 3, 4)], 13);

        Assert.AreEqual(
            $"{Direct3D9Factory.InvalidCallHResult}|get-dc:13|get-dc:17|bitblt:23:Direct3D9BitmapRealizationRectangle {{ Left = 1, Top = 2, Right = 3, Bottom = 4 }}:19:1:2|release-dc:13:19|release-dc:13:19|release-dc:17:23",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenThereAreNoDirtyRectanglesThenSurfacesAreUnlockedWithoutCopying()
    {
        using Direct3D9Device device = CreateDevice();
        List<string> calls = [];
        Direct3D9BitBltDeviceBitmapColorSourceUpdater updater = CreateUpdater(device, calls);

        int result = updater.Update([], 13);

        Assert.AreEqual(
            $"{Direct3D9Factory.SuccessHResult}|get-dc:13|get-dc:17|release-dc:13:19|release-dc:17:23|get-destination|release-surface:29",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenDestinationSurfaceRetrievalFailsWithPointerThenSurfaceIsReleased()
    {
        using Direct3D9Device device = CreateDevice();
        List<string> calls = [];
        Direct3D9BitBltDeviceBitmapColorSourceUpdater updater = CreateUpdater(
            device,
            calls,
            getDestinationSurface: (out nint surface) =>
            {
                calls.Add("get-destination-failed");
                surface = 29;
                return Direct3D9Factory.OutOfMemoryHResult;
            });

        int result = updater.Update([new(1, 2, 3, 4)], 13);

        Assert.AreEqual(
            $"{Direct3D9Factory.OutOfMemoryHResult}|get-dc:13|get-dc:17|bitblt:23:Direct3D9BitmapRealizationRectangle {{ Left = 1, Top = 2, Right = 3, Bottom = 4 }}:19:1:2|release-dc:13:19|release-dc:17:23|get-destination-failed|release-surface:29",
            $"{result}|{string.Join('|', calls)}");
    }

    private static Direct3D9BitBltDeviceBitmapColorSourceUpdater CreateUpdater(
        Direct3D9Device device,
        List<string> calls,
        Direct3D9BitBltBitmapRectangle? bitBlt = null,
        Direct3D9ReleaseBitmapSurfaceDeviceContext? releaseDeviceContext = null,
        Direct3D9GetBitmapDestinationSurface? getDestinationSurface = null)
    {
        return new(
            device,
            17,
            (nint surface, out nint deviceContext) =>
            {
                calls.Add($"get-dc:{surface}");
                deviceContext = surface == 13 ? 19 : 23;
                return Direct3D9Factory.SuccessHResult;
            },
            releaseDeviceContext ?? ((surface, deviceContext) =>
            {
                calls.Add($"release-dc:{surface}:{deviceContext}");
                return Direct3D9Factory.SuccessHResult;
            }),
            bitBlt ?? ((destinationDeviceContext, destinationRectangle, sourceDeviceContext, sourceX, sourceY) =>
            {
                calls.Add($"bitblt:{destinationDeviceContext}:{destinationRectangle}:{sourceDeviceContext}:{sourceX}:{sourceY}");
                return Direct3D9Factory.SuccessHResult;
            }),
            getDestinationSurface ?? ((out nint surface) =>
            {
                calls.Add("get-destination");
                surface = 29;
                return Direct3D9Factory.SuccessHResult;
            }),
            (sourceSurface, sourceRectangle, destinationSurface, destinationRectangle, filter) =>
            {
                calls.Add($"stretch:{sourceSurface}:{sourceRectangle}:{destinationSurface}:{destinationRectangle}:{filter}");
                return Direct3D9Factory.SuccessHResult;
            },
            surface => calls.Add($"release-surface:{surface}"));
    }

    private static Direct3D9Device CreateDevice() => new(
        null,
        null,
        0,
        Devtype.Hal,
        0,
        default);
}
