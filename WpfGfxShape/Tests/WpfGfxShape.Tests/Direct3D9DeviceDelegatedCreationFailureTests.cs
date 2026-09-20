using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9DeviceDelegatedCreationFailureTests
{
    [TestMethod]
    public void WhenDelegatedDepthBufferCreationFailsWithSurfaceThenSurfaceIsReleasedAndOutputIsCleared()
    {
        Direct3D9Surface returnedSurface = new(new Direct3D9ResourceManager(), null);
        using Direct3D9Device device = CreateDevice(
            createDepthBuffer: (uint _, uint _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                surface = returnedSurface;
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = device.CreateDepthBuffer(
            1,
            1,
            MultisampleType.MultisampleNone,
            out Direct3D9Surface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, null),
            (result, returnedSurface.IsReleased, surface));
    }

    [TestMethod]
    public void WhenDelegatedRenderTargetCreationFailsWithSurfaceThenSurfaceIsReleasedAndOutputIsCleared()
    {
        Direct3D9Surface returnedSurface = new(new Direct3D9ResourceManager(), null);
        using Direct3D9Device device = CreateDevice(
            createRenderTarget: (uint _, uint _, Format _, MultisampleType _, out Direct3D9Surface? surface) =>
            {
                surface = returnedSurface;
                return Direct3D9Factory.GenericFailureHResult;
            });

        int result = device.TryCreateRenderTarget(
            1,
            1,
            Format.A8R8G8B8,
            MultisampleType.MultisampleNone,
            0,
            false,
            out Direct3D9Surface? surface);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, true, null),
            (result, returnedSurface.IsReleased, surface));
    }

    [TestMethod]
    public void WhenDelegatedAdditionalSwapChainCreationReportsDriverInternalErrorWithSwapChainThenDeviceIsInvalidatedAndSwapChainIsReleased()
    {
        int releaseCount = 0;
        int unusableNotificationCount = 0;
        Direct3D9SwapChain returnedSwapChain = new(
            new Direct3D9ResourceManager(),
            null,
            release: () => releaseCount++);
        using Direct3D9Device device = CreateDevice(
            unusableNotification: _ => unusableNotificationCount++,
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                swapChain = returnedSwapChain;
                return Direct3D9Factory.DriverInternalErrorHResult;
            });

        int result = device.TryCreateAdditionalSwapChain(
            new PresentParameters(backBufferCount: 1, windowed: true),
            out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, Direct3D9Factory.DisplayStateInvalidHResult,
                true, 1, 1, null),
            (result, device.UnusableReasonHResult, returnedSwapChain.IsReleased,
                releaseCount, unusableNotificationCount, swapChain));
    }

    [TestMethod]
    public void WhenAdditionalSwapChainCreationIsAttemptedOnUnusableDeviceThenCreationIsSkippedAndOutputIsCleared()
    {
        int creationCount = 0;
        using Direct3D9Device device = CreateDevice(
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                creationCount++;
                swapChain = new Direct3D9SwapChain(new Direct3D9ResourceManager(), null);
                return Direct3D9Factory.SuccessHResult;
            });
        device.MarkUnusable();

        int result = device.TryCreateAdditionalSwapChain(
            new PresentParameters(backBufferCount: 1, windowed: true),
            out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.DisplayStateInvalidHResult, 0, null),
            (result, creationCount, swapChain));
    }

    [TestMethod]
    public void WhenAdditionalSwapChainExceedsTextureLimitsThenOutOfVideoMemoryIsReturnedBeforeCreation()
    {
        int creationCount = 0;
        Caps9 capabilities = new()
        {
            MaxTextureWidth = 10,
            MaxTextureHeight = 20,
        };
        using Direct3D9Device device = CreateDevice(
            capabilities: capabilities,
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                creationCount++;
                swapChain = new Direct3D9SwapChain(new Direct3D9ResourceManager(), null);
                return Direct3D9Factory.SuccessHResult;
            });

        int result = device.TryCreateAdditionalSwapChain(
            new PresentParameters(
                backBufferWidth: 11,
                backBufferHeight: 20,
                backBufferCount: 1,
                windowed: true),
            out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfVideoMemoryHResult, 0, null, false),
            (result, creationCount, swapChain, device.IsUnusable));
    }

    [TestMethod]
    public void WhenDelegatedAdditionalSwapChainCreationHasUnknownTextureLimitsThenCreationIsAttempted()
    {
        int creationCount = 0;
        Direct3D9SwapChain returnedSwapChain = new(new Direct3D9ResourceManager(), null);
        using Direct3D9Device device = CreateDevice(
            createAdditionalSwapChain: (PresentParameters _, out Direct3D9SwapChain? swapChain) =>
            {
                creationCount++;
                swapChain = returnedSwapChain;
                return Direct3D9Factory.SuccessHResult;
            });

        int result = device.TryCreateAdditionalSwapChain(
            new PresentParameters(
                backBufferWidth: 12,
                backBufferHeight: 34,
                backBufferCount: 1,
                windowed: true),
            out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 1, returnedSwapChain),
            (result, creationCount, swapChain));
    }

    [TestMethod]
    public void WhenDelegatedAdditionalSwapChainCreationSucceedsThenParametersAndOwnershipArePreserved()
    {
        PresentParameters observedParameters = default;
        Direct3D9SwapChain returnedSwapChain = new(new Direct3D9ResourceManager(), null);
        using Direct3D9Device device = CreateDevice(
            capabilities: new Caps9 { MaxTextureWidth = 100, MaxTextureHeight = 200 },
            createAdditionalSwapChain: (PresentParameters parameters, out Direct3D9SwapChain? swapChain) =>
            {
                observedParameters = parameters;
                swapChain = returnedSwapChain;
                return 1;
            });
        PresentParameters presentParameters = new(
            backBufferWidth: 12,
            backBufferHeight: 34,
            backBufferFormat: Format.A8R8G8B8,
            backBufferCount: 3,
            windowed: true);

        int result = device.TryCreateAdditionalSwapChain(presentParameters, out Direct3D9SwapChain? swapChain);

        Assert.AreEqual(
            (1, presentParameters, returnedSwapChain, false),
            (result, observedParameters, swapChain, returnedSwapChain.IsReleased));
    }

    private static Direct3D9Device CreateDevice(
        Action<Direct3D9Device>? unusableNotification = null,
        Direct3D9CreateDepthBuffer? createDepthBuffer = null,
        Direct3D9CreateRenderTarget? createRenderTarget = null,
        Direct3D9CreateAdditionalSwapChain? createAdditionalSwapChain = null,
        Caps9 capabilities = default)
    {
        return new Direct3D9Device(
            null,
            null,
            0,
            Devtype.Hal,
            0,
            new PresentParameters(windowed: true),
            unusableNotification: unusableNotification,
            capabilities: capabilities,
            createDepthBuffer: createDepthBuffer,
            createRenderTarget: createRenderTarget,
            createAdditionalSwapChain: createAdditionalSwapChain);
    }
}
