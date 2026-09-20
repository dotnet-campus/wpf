using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceBitmapColorSourcePixelCopierTests
{
    [TestMethod]
    public void WhenSingleClipIntersectsThenReadUsesAdjustedSurfaceAndBufferCoordinates()
    {
        using Direct3D9Device device = CreateDevice();
        bool getWasScoped = false;
        bool readWasScoped = false;
        int releaseCalls = 0;
        Direct3D9DeviceBitmapColorSourcePixelCopier copier = new(
            device,
            new Direct3D9BitmapRealizationRectangle(10, 20, 90, 100),
            new Direct3D9BitmapRealizationRectangle(15, 25, 80, 90),
            (out nint surface) =>
            {
                getWasScoped = device.IsEntered() && device.IsInUseContext();
                surface = 41;
                return Direct3D9Factory.SuccessHResult;
            },
            (surface, sourceRectangle, clipRectangles, format, stride, bufferSize, buffer) =>
            {
                readWasScoped = device.IsEntered() && device.IsInUseContext();
                Assert.AreEqual(
                    ((nint) 41, new Direct3D9BitmapRealizationRectangle(10, 15, 50, 55), 0, MilPixelFormat.Pbgra32Bpp, 400u, 13960u, (nint) 7040),
                    (surface, sourceRectangle, clipRectangles.Count, format, stride, bufferSize, buffer));
                return Direct3D9Factory.SuccessHResult;
            },
            _ => releaseCalls++);

        int result = copier.CopyPixels(
            new Direct3D9BitmapRealizationRectangle(10, 20, 70, 80),
            [new Direct3D9BitmapRealizationRectangle(20, 35, 60, 75)],
            MilPixelFormat.Pbgra32Bpp,
            20000,
            1000,
            400);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, true, true, 1, false, false),
            (result, getWasScoped, readWasScoped, releaseCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public void WhenSurfaceReadFailsThenFailurePropagatesAndSurfaceIsReleasedAfterUse()
    {
        using Direct3D9Device device = CreateDevice();
        bool releasedInsideScopes = false;
        Direct3D9DeviceBitmapColorSourcePixelCopier copier = new(
            device,
            new Direct3D9BitmapRealizationRectangle(0, 0, 64, 64),
            new Direct3D9BitmapRealizationRectangle(0, 0, 64, 64),
            (out nint surface) =>
            {
                surface = 73;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _, _, _, _, _, _) => Direct3D9Factory.InvalidCallHResult,
            surface => releasedInsideScopes = surface == 73 && device.IsEntered() && device.IsInUseContext());

        int result = copier.CopyPixels(
            new Direct3D9BitmapRealizationRectangle(1, 2, 3, 4),
            [],
            MilPixelFormat.Bgr24Bpp,
            100,
            200,
            12);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, true, false, false),
            (result, releasedInsideScopes, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public void WhenValidBoundsDoNotIntersectThenCopySucceedsWithoutUsingSurface()
    {
        using Direct3D9Device device = CreateDevice();
        int surfaceCalls = 0;
        Direct3D9DeviceBitmapColorSourcePixelCopier copier = new(
            device,
            new Direct3D9BitmapRealizationRectangle(0, 0, 64, 64),
            new Direct3D9BitmapRealizationRectangle(0, 0, 10, 10),
            (out nint surface) =>
            {
                surfaceCalls++;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _, _, _, _, _, _) => Direct3D9Factory.GenericFailureHResult,
            _ => { });

        int result = copier.CopyPixels(
            new Direct3D9BitmapRealizationRectangle(20, 20, 30, 30),
            [],
            MilPixelFormat.Pbgra32Bpp,
            400,
            100,
            40);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 0, false, false),
            (result, surfaceCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public void WhenSurfaceOperationsReturnNonzeroSuccessThenReadResultPropagatesAndUseContextExits()
    {
        using Direct3D9Device device = CreateDevice();
        uint getDepth = 0;
        uint readDepth = 0;
        Direct3D9DeviceBitmapColorSourcePixelCopier copier = new(
            device,
            new Direct3D9BitmapRealizationRectangle(0, 0, 64, 64),
            new Direct3D9BitmapRealizationRectangle(0, 0, 64, 64),
            (out nint surface) =>
            {
                getDepth = device.ResourceManager.CurrentUseContextDepth;
                surface = 91;
                return 1;
            },
            (_, _, _, _, _, _, _) =>
            {
                readDepth = device.ResourceManager.CurrentUseContextDepth;
                return 2;
            },
            _ => { });

        int result = copier.CopyPixels(
            new Direct3D9BitmapRealizationRectangle(1, 2, 3, 4),
            [],
            MilPixelFormat.Pbgra32Bpp,
            128,
            200,
            16);

        Assert.AreEqual((2, 1u, 1u, 0u),
            (result, getDepth, readDepth, device.ResourceManager.CurrentUseContextDepth));
    }

    [TestMethod]
    public void WhenGuardedBufferInsetIsInvalidThenEarlyReturnRestoresOuterUseContext()
    {
        using Direct3D9Device device = CreateDevice();
        uint outerDepth = device.EnterUseContext();
        int surfaceCalls = 0;
        Direct3D9DeviceBitmapColorSourcePixelCopier copier = new(
            device,
            new Direct3D9BitmapRealizationRectangle(0, 0, 64, 64),
            new Direct3D9BitmapRealizationRectangle(0, 1, 64, 64),
            (out nint surface) =>
            {
                surfaceCalls++;
                surface = 0;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _, _, _, _, _, _) => Direct3D9Factory.SuccessHResult,
            _ => { });

        int result = copier.CopyPixels(
            new Direct3D9BitmapRealizationRectangle(0, 0, 1, 2),
            [],
            MilPixelFormat.Pbgra32Bpp,
            1,
            200,
            uint.MaxValue);

        Assert.AreEqual(
            (Direct3D9Factory.ArithmeticOverflowHResult, 0, outerDepth, true),
            (result, surfaceCalls, device.ResourceManager.CurrentUseContextDepth, device.IsInUseContext()));

        device.ExitUseContext(outerDepth);
    }

    [TestMethod]
    public void WhenNestedCopyUsesNewAndOuterResourcesThenGuardOnlyCompletesInnerResource()
    {
        using Direct3D9Device device = CreateDevice();
        uint outerDepth = device.EnterUseContext();
        using TestResource outerResource = new(device.ResourceManager);
        TestResource? innerResource = null;
        uint observedInnerDepth = 0;
        Direct3D9DeviceBitmapColorSourcePixelCopier copier = new(
            device,
            new Direct3D9BitmapRealizationRectangle(0, 0, 64, 64),
            new Direct3D9BitmapRealizationRectangle(0, 0, 64, 64),
            (out nint surface) =>
            {
                observedInnerDepth = device.ResourceManager.CurrentUseContextDepth;
                innerResource = new TestResource(device.ResourceManager);
                device.Use(outerResource);
                surface = 117;
                return Direct3D9Factory.SuccessHResult;
            },
            (_, _, _, _, _, _, _) => Direct3D9Factory.SuccessHResult,
            _ => { });

        int result = copier.CopyPixels(
            new Direct3D9BitmapRealizationRectangle(1, 2, 3, 4),
            [],
            MilPixelFormat.Pbgra32Bpp,
            128,
            200,
            16);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, outerDepth + 1, outerDepth, outerDepth, 0u, true, true, true, true),
            (result, observedInnerDepth, device.ResourceManager.CurrentUseContextDepth,
                outerResource.ActiveUseContextDepth, innerResource!.ActiveUseContextDepth,
                outerResource.IsValid, innerResource.IsValid, outerResource.IsManaged, innerResource.IsManaged));

        innerResource.Dispose();
        device.ExitUseContext(outerDepth);
    }

    private static Direct3D9Device CreateDevice() => new(
        null,
        null,
        0,
        Devtype.Hal,
        0,
        default);

    private sealed class TestResource(Direct3D9ResourceManager manager)
        : Direct3D9Resource(manager, isEvictable: true)
    {
        protected override void ReleaseD3DResources()
        {
        }
    }
}
