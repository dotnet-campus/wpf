using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenDisplayRenderingIsDisabledThenVBlankWaitStillDelegatesWithinDeviceEntry()
    {
        Queue<long> timestamps = new([0, 1]);
        int waitCalls = 0;
        Direct3D9Device? waitedDevice = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            waitForVBlank: index =>
            {
                waitCalls++;
                Assert.AreEqual(0u, index);
                Assert.IsTrue(waitedDevice!.IsEntered());
                return 1;
            },
            getTimestamp: () => timestamps.Dequeue(),
            timestampFrequency: 1000);
        waitedDevice = device;
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 0));

        int result = renderTarget.WaitForVBlank();

        Assert.AreEqual((1, 1, false, false),
            (result, waitCalls, renderTarget.IsRenderingEnabled, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenWaitingForVBlankThenFirstFastSuccessEnablesLaterCalls()
    {
        Queue<long> timestamps = new([0, 74]);
        List<uint> swapChainIndices = [];
        Direct3D9Device? waitedDevice = null;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            waitForVBlank: index =>
            {
                swapChainIndices.Add(index);
                Assert.IsTrue(waitedDevice!.IsEntered());
                return 0;
            },
            getTimestamp: () => timestamps.Dequeue(),
            timestampFrequency: 1000);
        waitedDevice = device;
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int firstResult = renderTarget.WaitForVBlank();
        int secondResult = renderTarget.WaitForVBlank();

        Assert.AreEqual((0, 0, "0,0", false), (firstResult, secondResult, string.Join(',', swapChainIndices), device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenFirstVBlankWaitReachesThresholdThenSupportIsDisabled()
    {
        Queue<long> timestamps = new([0, 75]);
        int waitCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            waitForVBlank: _ =>
            {
                waitCalls++;
                return 0;
            },
            getTimestamp: () => timestamps.Dequeue(),
            timestampFrequency: 1000);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int firstResult = renderTarget.WaitForVBlank();
        int secondResult = renderTarget.WaitForVBlank();

        Assert.AreEqual(
            (Direct3D9Factory.NoHardwareDeviceHResult, Direct3D9Factory.NoHardwareDeviceHResult, 1),
            (firstResult, secondResult, waitCalls));
    }

    [TestMethod]
    public unsafe void WhenVBlankWaitFailsThenFailureIsNormalizedAndSupportIsDisabled()
    {
        Queue<long> timestamps = new([0, 1]);
        int waitCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            waitForVBlank: _ =>
            {
                waitCalls++;
                return Direct3D9Factory.GenericFailureHResult;
            },
            getTimestamp: () => timestamps.Dequeue(),
            timestampFrequency: 1000);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int firstResult = renderTarget.WaitForVBlank();
        int secondResult = renderTarget.WaitForVBlank();

        Assert.AreEqual(
            (Direct3D9Factory.NoHardwareDeviceHResult, Direct3D9Factory.NoHardwareDeviceHResult, 1),
            (firstResult, secondResult, waitCalls));
    }

    [TestMethod]
    public unsafe void WhenFirstVBlankWaitReportsDriverInternalErrorThenSupportIsDisabledWithoutInvalidatingDevice()
    {
        Queue<long> timestamps = new([0, 1]);
        int waitCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            waitForVBlank: _ =>
            {
                waitCalls++;
                return Direct3D9Factory.DriverInternalErrorHResult;
            },
            getTimestamp: () => timestamps.Dequeue(),
            timestampFrequency: 1000);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int firstResult = renderTarget.WaitForVBlank();
        int secondResult = renderTarget.WaitForVBlank();

        Assert.AreEqual(
            (Direct3D9Factory.NoHardwareDeviceHResult, Direct3D9Factory.NoHardwareDeviceHResult, 1, false, 0),
            (firstResult, secondResult, waitCalls, device.IsUnusable, device.UnusableReasonHResult));
    }

    [TestMethod]
    public unsafe void WhenSupportedVBlankWaitReportsDriverInternalErrorThenLaterCallsContinueWithoutInvalidatingDevice()
    {
        Queue<long> timestamps = new([0, 1]);
        int waitCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            waitForVBlank: _ => ++waitCalls == 2 ? Direct3D9Factory.DriverInternalErrorHResult : 0,
            getTimestamp: () => timestamps.Dequeue(),
            timestampFrequency: 1000);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        int firstResult = renderTarget.WaitForVBlank();
        int secondResult = renderTarget.WaitForVBlank();
        int thirdResult = renderTarget.WaitForVBlank();

        Assert.AreEqual(
            (0, Direct3D9Factory.NoHardwareDeviceHResult, 0, 3, false, 0),
            (firstResult, secondResult, thirdResult, waitCalls, device.IsUnusable, device.UnusableReasonHResult));
    }

    [TestMethod]
    public unsafe void WhenDisplayIsInvalidThenVBlankWaitSkipsDeviceCall()
    {
        int waitCalls = 0;
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            capabilities: new Caps9 { MaxTextureWidth = 4096, MaxTextureHeight = 4096 },
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = null;
                return Direct3D9Factory.DisplayStateInvalidHResult;
            },
            waitForVBlank: _ =>
            {
                waitCalls++;
                return 0;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        _ = renderTarget.Resize(16, 12);

        int result = renderTarget.WaitForVBlank();

        Assert.AreEqual(
            (Direct3D9Factory.NoHardwareDeviceHResult, 0, false),
            (result, waitCalls, device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenDisposedThenWaitForVBlankRejectsUse()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));
        Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.WaitForVBlank());
    }
}
