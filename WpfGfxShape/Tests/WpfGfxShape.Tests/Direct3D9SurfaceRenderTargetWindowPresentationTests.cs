using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenCreatedThenWindowPresentationStateMatchesNativeDefaults()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        Assert.AreEqual(
            (new Direct3D9WindowPosition(0, 0), new Direct3D9LayeredWindowProperties(0xC, 255, 0, 0)),
            (renderTarget.WindowPosition, renderTarget.LayeredWindowProperties));
    }

    [TestMethod]
    public unsafe void WhenPositionChangesThenLatestPositionIsStoredWithoutEnteringDeviceScopes()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        renderTarget.SetPosition(new Direct3D9WindowPosition(-20, 30));
        renderTarget.SetPosition(new Direct3D9WindowPosition(40, -50));

        Assert.AreEqual(
            (new Direct3D9WindowPosition(40, -50), false, false),
            (renderTarget.WindowPosition, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenPositionIsStillStoredWithoutPresentationSideEffects()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 12));
        Direct3D9WindowPosition position = new(-37, 91);

        renderTarget.SetPosition(position);

        Assert.AreEqual(
            (position, false, 0, false, false),
            (renderTarget.WindowPosition, renderTarget.IsRenderingEnabled, presentCalls, device.IsEntered(), device.IsInUseContext()));
    }

    [TestMethod]
    public unsafe void WhenPresentPropertiesCombineEffectsThenLayeredWindowStateMatchesNativeRules()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initializationFlags: Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha);

        renderTarget.UpdatePresentProperties(
            MilTransparencyFlags.ConstantAlpha | MilTransparencyFlags.PerPixelAlpha | MilTransparencyFlags.ColorKey,
            96,
            0x00112233);

        Assert.AreEqual(
            new Direct3D9LayeredWindowProperties(0xB, 96, 1, 0x00112233),
            renderTarget.LayeredWindowProperties);
    }

    [TestMethod]
    public unsafe void WhenPerPixelAlphaLacksDestinationAlphaThenOpaqueFallbackIsUsed()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);

        renderTarget.UpdatePresentProperties(MilTransparencyFlags.PerPixelAlpha, 64, 0x00112233);

        Assert.AreEqual(
            new Direct3D9LayeredWindowProperties(0xC, 64, 0, 0x00112233),
            renderTarget.LayeredWindowProperties);
    }

    [TestMethod]
    public unsafe void WhenOpaquePresentPropertiesAreRestoredThenAlphaStateReturnsToDefaults()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initializationFlags: Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha);
        renderTarget.UpdatePresentProperties(MilTransparencyFlags.PerPixelAlpha, 64, 0x00112233);

        renderTarget.UpdatePresentProperties(MilTransparencyFlags.Opaque, 0, 0x00FFFFFF);

        Assert.AreEqual(
            new Direct3D9LayeredWindowProperties(0xC, 255, 0, 0x00112233),
            renderTarget.LayeredWindowProperties);
    }

    [TestMethod]
    [DataRow((int)MilTransparencyFlags.ConstantAlpha, false, 0xA, 0)]
    [DataRow((int)MilTransparencyFlags.ColorKey, false, 0x9, 0)]
    [DataRow((int)MilTransparencyFlags.PerPixelAlpha, true, 0xA, 1)]
    public unsafe void WhenPresentPropertiesUseIndividualEffectsThenEachNativeGateIsPreserved(
        int transparencyFlags,
        bool needsDestinationAlpha,
        int expectedUpdateFlags,
        int expectedAlphaFormat)
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initializationFlags: needsDestinationAlpha
                ? Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha
                : Direct3D9RenderTargetInitializationFlags.None);

        renderTarget.UpdatePresentProperties((MilTransparencyFlags)transparencyFlags, 73, 0x00445566);

        Assert.AreEqual(
            new Direct3D9LayeredWindowProperties((uint)expectedUpdateFlags, 73, (byte)expectedAlphaFormat, 0x00445566),
            renderTarget.LayeredWindowProperties);
    }

    [TestMethod]
    public unsafe void WhenPresentPropertiesChangeThenLatestValuesReplacePriorEffectsWithoutSideEffects()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initializationFlags: Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha);
        renderTarget.UpdatePresentProperties(
            MilTransparencyFlags.ConstantAlpha | MilTransparencyFlags.PerPixelAlpha,
            91,
            0x00112233);
        int resourceCount = device.ResourceCount;

        renderTarget.UpdatePresentProperties(MilTransparencyFlags.ColorKey, 37, 0x00445566);

        Assert.AreEqual(
            (new Direct3D9LayeredWindowProperties(0x9, 37, 0, 0x00445566), true, 0, 0, false, false, resourceCount),
            (renderTarget.LayeredWindowProperties, renderTarget.IsRenderingEnabled, renderTarget.DisplayInvalidHResult,
                presentCalls, device.IsEntered(), device.IsInUseContext(), device.ResourceCount));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenPresentPropertiesAreStillStoredWithoutSideEffects()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(
            device,
            MultisampleType.MultisampleNone,
            initializationFlags: Direct3D9RenderTargetInitializationFlags.NeedDestinationAlpha);
        Assert.AreEqual(0, renderTarget.Resize(0, 12));
        int resourceCount = device.ResourceCount;

        renderTarget.UpdatePresentProperties(
            MilTransparencyFlags.ConstantAlpha | MilTransparencyFlags.PerPixelAlpha | MilTransparencyFlags.ColorKey,
            82,
            0x00778899);

        Assert.AreEqual(
            (new Direct3D9LayeredWindowProperties(0xB, 82, 1, 0x00778899), false, 0, false, 0, false, false, resourceCount),
            (renderTarget.LayeredWindowProperties, renderTarget.IsRenderingEnabled, renderTarget.DisplayInvalidHResult,
                renderTarget.HasValidContents, presentCalls, device.IsEntered(), device.IsInUseContext(), device.ResourceCount));
    }

    [TestMethod]
    public unsafe void WhenInvalidatedRectsAreClearedThenDirtyAndEmptyInvalidationsAreRemovedIdempotentlyWithoutSideEffects()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Assert.AreEqual(0, renderTarget.InvalidateRect(new Direct3D9SurfaceRect(1, 2, 7, 9)));
        Assert.AreEqual(0, renderTarget.InvalidateRect(default));
        int resourceCount = device.ResourceCount;

        int firstResult = renderTarget.ClearInvalidatedRects();
        int secondResult = renderTarget.ClearInvalidatedRects();
        int presentResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual(
            (0, 0, 0, 0, true, 0, false, false, resourceCount),
            (firstResult, secondResult, presentResult, presentCalls, renderTarget.IsRenderingEnabled,
                renderTarget.DisplayInvalidHResult, device.IsEntered(), device.IsInUseContext(), device.ResourceCount));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenInvalidatedRectsAreClearedWithoutSideEffects()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return 0;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 12));
        int resourceCount = device.ResourceCount;

        int firstResult = renderTarget.ClearInvalidatedRects();
        int secondResult = renderTarget.ClearInvalidatedRects();

        Assert.AreEqual(
            (0, 0, false, 0, false, 0, false, false, resourceCount),
            (firstResult, secondResult, renderTarget.IsRenderingEnabled, renderTarget.DisplayInvalidHResult,
                renderTarget.HasValidContents, presentCalls, device.IsEntered(), device.IsInUseContext(), device.ResourceCount));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(Direct3D9Factory.InvalidArgumentHResult)]
    public unsafe void WhenWindowPresentDelegatesThenBaseResultIsReturnedAfterExactlyOneCall(int presentResult)
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return presentResult;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Assert.AreEqual(0, renderTarget.InvalidateRect(default));

        int result = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual((presentResult, 1), (result, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenRenderingIsDisabledThenRectPresentReturnsRememberedSuccessWithoutCallingSwapChain()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return Direct3D9Factory.GenericFailureHResult;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(0, 12));

        int result = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual((0, 0), (result, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenRectPresentReturnsDisplayInvalidThenLaterPresentReturnsRememberedFailureWithoutCallingSwapChain()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return Direct3D9Factory.DisplayStateInvalidHResult;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Assert.AreEqual(0, renderTarget.InvalidateRect(default));

        int firstResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));
        int secondResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DisplayStateInvalidHResult, 1),
            (firstResult, secondResult, presentCalls));
    }

    [TestMethod]
    public unsafe void WhenRectPresentReturnsOtherFailureThenLaterPresentReturnsRememberedSuccessWithoutCallingSwapChain()
    {
        int presentCalls = 0;
        using Direct3D9Device device = CreatePresentDevice(() =>
        {
            presentCalls++;
            return Direct3D9Factory.InvalidArgumentHResult;
        });
        using Direct3D9SurfaceRenderTarget renderTarget = new(device, MultisampleType.MultisampleNone);
        Assert.AreEqual(0, renderTarget.Resize(16, 12));
        Assert.AreEqual(0, renderTarget.InvalidateRect(default));

        int firstResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));
        int secondResult = renderTarget.Present(new Direct3D9SurfaceRect(0, 0, 16, 12));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, 0, 1),
            (firstResult, secondResult, presentCalls));
    }
}
