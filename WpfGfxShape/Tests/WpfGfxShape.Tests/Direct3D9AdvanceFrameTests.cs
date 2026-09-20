using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9AdvanceFrameTests
{
    [TestMethod]
    public unsafe void WhenDisplayAdvancesToSameFrameThenOriginalFrameNumberIsDelegatedOnce()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget renderTarget = CreateDisplayTarget(device);

        renderTarget.AdvanceFrame(uint.MaxValue);
        renderTarget.AdvanceFrame(uint.MaxValue);

        Assert.AreEqual(
            (1u, 1u, 1u, false),
            GetFrameState(device));
    }

    [TestMethod]
    public unsafe void WhenDisplayRenderingIsDisabledThenAdvanceFrameStillDelegatesWithinDeviceEntry()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget renderTarget = CreateDisplayTarget(device);
        Assert.AreEqual(0, renderTarget.Resize(0, 0));

        renderTarget.AdvanceFrame(37);

        Assert.AreEqual(
            (1u, 1u, 1u, false, false),
            (device.ResourceManager.CompletedFrameCount,
                device.ResourceManager.ReleasedResourcesFromLastFrameDestroyCount,
                device.ResourceManager.DelayedResourceDestroyCount,
                renderTarget.IsRenderingEnabled,
                device.IsEntered()));
    }

    [TestMethod]
    public unsafe void WhenDisplayAdvancesToNewFrameThenResourceManagerStagesRunAgain()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget renderTarget = CreateDisplayTarget(device);

        renderTarget.AdvanceFrame(1);
        renderTarget.AdvanceFrame(2);

        Assert.AreEqual(
            (2u, 2u, 2u, false),
            GetFrameState(device));
    }

    [TestMethod]
    public unsafe void WhenEndFrameFailsThenLaterResourceManagerStagesAreSkippedAndDeviceEntryIsExited()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9SurfaceRenderTarget renderTarget = CreateDisplayTarget(device);
        uint useContextDepth = device.EnterUseContext();

        Assert.ThrowsExactly<InvalidOperationException>(() => renderTarget.AdvanceFrame(1));

        device.ExitUseContext(useContextDepth);
        renderTarget.AdvanceFrame(1);

        Assert.AreEqual(
            (0u, 0u, 0u, false),
            GetFrameState(device));
    }

    [TestMethod]
    public unsafe void WhenDisplayIsInvalidThenAdvanceFrameSkipsDeviceAndResourceManager()
    {
        using Direct3D9Device device = new(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = null;
                return Direct3D9Factory.DisplayStateInvalidHResult;
            });
        using Direct3D9SurfaceRenderTarget renderTarget = CreateDisplayTarget(device);
        _ = renderTarget.Resize(16, 12);

        renderTarget.AdvanceFrame(1);

        Assert.AreEqual(
            (0u, 0u, 0u, false),
            GetFrameState(device));
        Assert.IsFalse(device.IsEntered());
    }

    [TestMethod]
    public unsafe void WhenDirectSurfaceAdvancesFrameThenDisplayOnlyStagesAreSkipped()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9Surface surface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: surface,
            width: 16,
            height: 12);

        renderTarget.AdvanceFrame(1);

        Assert.AreEqual(
            (0u, 0u, 0u, false),
            GetFrameState(device));
    }

    [TestMethod]
    public unsafe void WhenTextureBackedSurfaceAdvancesFrameThenDisplayOnlyStagesAreSkipped()
    {
        using Direct3D9Device device = CreateDevice();
        using Direct3D9Surface textureLevelSurface = CreateSurface();
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            renderTargetSurface: textureLevelSurface,
            width: 16,
            height: 12);

        renderTarget.AdvanceFrame(2);

        Assert.AreEqual(
            (0u, 0u, 0u, false),
            GetFrameState(device));
    }

    [TestMethod]
    public unsafe void WhenDisposedTargetAdvancesFrameThenOperationIsRejected()
    {
        using Direct3D9Device device = CreateDevice();
        Direct3D9SurfaceRenderTarget renderTarget = CreateDisplayTarget(device);
        renderTarget.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => renderTarget.AdvanceFrame(1));
    }

    private static unsafe Direct3D9Device CreateDevice()
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true));
    }

    private static unsafe Direct3D9SurfaceRenderTarget CreateDisplayTarget(Direct3D9Device device)
    {
        return new Direct3D9SurfaceRenderTarget(
            device,
            MultisampleType.MultisampleNone,
            associatedDisplayIndex: 0);
    }

    private static unsafe Direct3D9Surface CreateSurface()
    {
        return new Direct3D9Surface(new Direct3D9ResourceManager(), null);
    }

    private static (uint CompletedFrames, uint ReleasedResources, uint DelayedResources, bool IsEntered) GetFrameState(
        Direct3D9Device device)
    {
        return (
            device.ResourceManager.CompletedFrameCount,
            device.ResourceManager.ReleasedResourcesFromLastFrameDestroyCount,
            device.ResourceManager.DelayedResourceDestroyCount,
            device.IsEntered());
    }
}
