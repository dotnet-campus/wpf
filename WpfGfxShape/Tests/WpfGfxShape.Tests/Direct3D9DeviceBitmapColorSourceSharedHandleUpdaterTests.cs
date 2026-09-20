using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed class Direct3D9DeviceBitmapColorSourceSharedHandleUpdaterTests
{
    [TestMethod]
    public void WhenUpdateSucceedsThenSharedSurfaceIsCopiedFlushedAndReleasedInNativeOrder()
    {
        List<string> calls = [];
        SurfaceDesc description = new(
            format: Format.A8R8G8B8,
            type: Resourcetype.Texture,
            usage: D3D9.UsageRendertarget,
            pool: Pool.Default,
            width: 64,
            height: 32);
        Direct3D9DeviceBitmapColorSourceSharedHandleUpdater updater = new(
            17,
            description,
            3,
            (nint surface, out nint device) =>
            {
                calls.Add($"get-device:{surface}");
                device = 23;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint device, SurfaceDesc actualDescription, uint levels, ref nint sharedHandle, out nint texture) =>
            {
                calls.Add($"open:{device}:{actualDescription.Width}x{actualDescription.Height}:{levels}:{sharedHandle}");
                sharedHandle = 19;
                texture = 29;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint texture, uint level, out nint surface) =>
            {
                calls.Add($"get-surface:{texture}:{level}");
                surface = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            (device, source, sourceRectangle, destination, destinationRectangle, filter) =>
            {
                calls.Add($"stretch:{device}:{source}:{sourceRectangle}:{destination}:{destinationRectangle}:{filter}");
                return Direct3D9Factory.SuccessHResult;
            },
            (device, surface, actualDescription) =>
            {
                calls.Add($"flush:{device}:{surface}:{actualDescription.Format}");
                return Direct3D9Factory.SuccessHResult;
            },
            surface => calls.Add($"release-surface:{surface}"),
            texture => calls.Add($"release-texture:{texture}"),
            device => calls.Add($"release-device:{device}"));
        Direct3D9BitmapRealizationRectangle[] dirtyRectangles =
        [
            new(1, 2, 11, 12),
            new(20, 21, 30, 31)
        ];

        int result = updater.Update(dirtyRectangles, 13);

        Assert.AreEqual(
            string.Join('|',
                $"result:{Direct3D9Factory.SuccessHResult}",
                "get-device:13",
                "open:23:64x32:3:17",
                "get-surface:29:0",
                "stretch:23:13:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 11, Bottom = 12 }:31:Direct3D9SurfaceRect { Left = 1, Top = 2, Right = 11, Bottom = 12 }:TexfNone",
                "stretch:23:13:Direct3D9SurfaceRect { Left = 20, Top = 21, Right = 30, Bottom = 31 }:31:Direct3D9SurfaceRect { Left = 20, Top = 21, Right = 30, Bottom = 31 }:TexfNone",
                "flush:23:31:A8R8G8B8",
                "release-surface:31",
                "release-texture:29",
                "release-device:23"),
            string.Join('|', [$"result:{result}", .. calls]));
    }

    [TestMethod]
    public void WhenSecondStretchFailsThenFirstFailurePropagatesWithoutFlush()
    {
        List<string> calls = [];
        int stretchCalls = 0;
        Direct3D9DeviceBitmapColorSourceSharedHandleUpdater updater = CreateUpdater(
            stretchRectangle: (_, _, rectangle, _, _, _) =>
            {
                calls.Add($"stretch:{rectangle.Left}");
                stretchCalls++;
                return stretchCalls == 2
                    ? Direct3D9Factory.InvalidCallHResult
                    : Direct3D9Factory.SuccessHResult;
            },
            flush: (_, _, _) =>
            {
                calls.Add("flush");
                return Direct3D9Factory.SuccessHResult;
            },
            releaseSurface: surface => calls.Add($"surface:{surface}"),
            releaseTexture: texture => calls.Add($"texture:{texture}"),
            releaseDevice: device => calls.Add($"device:{device}"));

        int result = updater.Update(
            [new(1, 0, 2, 1), new(3, 0, 4, 1), new(5, 0, 6, 1)],
            13);

        Assert.AreEqual(
            $"{Direct3D9Factory.InvalidCallHResult}|stretch:1|stretch:3|surface:31|texture:29|device:23",
            $"{result}|{string.Join('|', calls)}");
    }

    [TestMethod]
    public void WhenOpeningSharedTextureFailsWithNonNullTextureThenAcquiredInterfacesAreReleased()
    {
        List<string> cleanup = [];
        Direct3D9DeviceBitmapColorSourceSharedHandleUpdater updater = new(
            17,
            new SurfaceDesc(format: Format.A8R8G8B8, width: 16, height: 16),
            1,
            (nint _, out nint device) =>
            {
                device = 23;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, SurfaceDesc _, uint _, ref nint _, out nint texture) =>
            {
                texture = 29;
                return Direct3D9Factory.OutOfMemoryHResult;
            },
            (nint _, uint _, out nint surface) =>
            {
                surface = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _, _, _, _, _) => Direct3D9Factory.SuccessHResult,
            (_, _, _) => Direct3D9Factory.SuccessHResult,
            surface => cleanup.Add($"surface:{surface}"),
            texture => cleanup.Add($"texture:{texture}"),
            device => cleanup.Add($"device:{device}"));

        int result = updater.Update([new(0, 0, 1, 1)], 13);

        Assert.AreEqual(
            $"{Direct3D9Factory.OutOfMemoryHResult}|texture:29|device:23",
            $"{result}|{string.Join('|', cleanup)}");
    }

    private static Direct3D9DeviceBitmapColorSourceSharedHandleUpdater CreateUpdater(
        Direct3D9StretchSharedBitmapRectangle stretchRectangle,
        Direct3D9FlushSharedBitmapSurface flush,
        Action<nint> releaseSurface,
        Action<nint> releaseTexture,
        Action<nint> releaseDevice) => new(
            17,
            new SurfaceDesc(format: Format.A8R8G8B8, width: 16, height: 16),
            1,
            (nint _, out nint device) =>
            {
                device = 23;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, SurfaceDesc _, uint _, ref nint _, out nint texture) =>
            {
                texture = 29;
                return Direct3D9Factory.SuccessHResult;
            },
            (nint _, uint _, out nint surface) =>
            {
                surface = 31;
                return Direct3D9Factory.SuccessHResult;
            },
            stretchRectangle,
            flush,
            releaseSurface,
            releaseTexture,
            releaseDevice);
}
