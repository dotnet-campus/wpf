using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceBitmapColorSourceUpdaterTests
{
    [TestMethod]
    public void WhenSharedHandleIsAvailableThenSharedUpdateRunsInsideDeviceScope()
    {
        using Direct3D9Device device = CreateDevice();
        bool sharedWasEntered = false;
        int softwareCalls = 0;
        Direct3D9DeviceBitmapColorSourceUpdater updater = new(
            device,
            usesSharedHandle: true,
            (dirtyRectangles, sourceSurface) =>
            {
                sharedWasEntered = device.IsEntered();
                Assert.AreEqual(2, dirtyRectangles.Length);
                Assert.AreEqual((nint) 41, sourceSurface);
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _) =>
            {
                softwareCalls++;
                return Direct3D9Factory.GenericFailureHResult;
            });
        Direct3D9BitmapRealizationRectangle[] dirtyRectangles =
        [
            new(1, 2, 3, 4),
            new(5, 6, 7, 8)
        ];

        int result = updater.UpdateSurface(dirtyRectangles, 41);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, true, 0, false),
            (result, sharedWasEntered, softwareCalls, device.IsEntered()));
    }

    [TestMethod]
    public void WhenSharedHandleIsUnavailableThenSoftwareFailurePropagatesAndDeviceScopeExits()
    {
        using Direct3D9Device device = CreateDevice();
        int sharedCalls = 0;
        bool softwareWasEntered = false;
        Direct3D9DeviceBitmapColorSourceUpdater updater = new(
            device,
            usesSharedHandle: false,
            (_, _) =>
            {
                sharedCalls++;
                return Direct3D9Factory.SuccessHResult;
            },
            (dirtyRectangles, sourceSurface) =>
            {
                softwareWasEntered = device.IsEntered();
                Assert.AreEqual(1, dirtyRectangles.Length);
                Assert.AreEqual((nint) 73, sourceSurface);
                return Direct3D9Factory.InvalidCallHResult;
            });
        Direct3D9BitmapRealizationRectangle[] dirtyRectangles =
        [
            new(11, 12, 13, 14)
        ];

        int result = updater.UpdateSurface(dirtyRectangles, 73);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 0, true, false),
            (result, sharedCalls, softwareWasEntered, device.IsEntered()));
    }

    private static Direct3D9Device CreateDevice() => new(
        null,
        null,
        0,
        Devtype.Hal,
        0,
        default);
}
