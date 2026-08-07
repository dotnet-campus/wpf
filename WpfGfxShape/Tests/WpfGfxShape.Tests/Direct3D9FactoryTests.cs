using System.Runtime.Versioning;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class Direct3D9FactoryTests
{
    [TestMethod]
    public void WhenReleasingNullDirect3D9InterfaceThenDoesNotThrow()
    {
        Direct3D9Factory.Release((Silk.NET.Direct3D9.IDirect3D9*) null);
    }

    [TestMethod]
    public void WhenReleasingNullDirect3D9ExInterfaceThenDoesNotThrow()
    {
        Direct3D9Factory.Release((Silk.NET.Direct3D9.IDirect3D9Ex*) null);
    }

    [TestMethod]
    public void WhenReadingNotAvailableResultThenMatchesDirect3D9HResult()
    {
        Assert.AreEqual(unchecked((int) 0x8876086A), Direct3D9Factory.NotAvailableHResult);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCreatingDirect3D9ObjectsThenReturnsBaseInterface()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();

        Assert.IsTrue(objects.Direct3D is not null);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenEnumeratingAdaptersThenReturnsAtLeastOneAdapter()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 adapter enumeration requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();

        Assert.IsGreaterThan(0u, objects.GetAdapterCount());
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenQueryingDefaultAdapterThenReturnsDisplayMode()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 display mode queries require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();

        Direct3D9AdapterCapabilities capabilities = objects.GetAdapterCapabilities(0);

        Assert.IsGreaterThan(0u, capabilities.DisplayMode.Width);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenQueryingDefaultAdapterThenCapsRetainAdapterOrdinal()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 capability queries require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();

        Direct3D9AdapterCapabilities capabilities = objects.GetAdapterCapabilities(0);

        Assert.AreEqual(0u, capabilities.Caps.AdapterOrdinal);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenQueryingDisposedObjectsThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 capability queries require Windows.");
        }

        Direct3D9Objects objects = Direct3D9Factory.Create();
        objects.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => objects.GetAdapterCount());
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCreatingDefaultDeviceThenReturnsBaseDeviceInterface()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 device creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        Assert.IsTrue(device.Device is not null);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCreatingDefaultDeviceThenUsesOnePixelBackBuffer()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 device creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        Assert.AreEqual(1u, device.PresentParameters.BackBufferWidth);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCreatingRenderTargetThenReturnsSurfaceInterface()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 render target creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using var surface = device.CreateRenderTarget(2, 3);

        Assert.IsTrue(surface.Surface is not null);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCreatingRenderTargetThenDescriptionRetainsDimensions()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 render target creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using var surface = device.CreateRenderTarget(2, 3);

        Silk.NET.Direct3D9.SurfaceDesc description = surface.GetDescription();

        Assert.AreEqual(2u, description.Width);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenReadingDisposedRenderTargetThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 render target creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        var surface = device.CreateRenderTarget(2, 3);
        surface.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.GetDescription());
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCreatingAdditionalSwapChainThenReturnsSwapChainInterface()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 swap chain creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using var swapChain = device.CreateAdditionalSwapChain(2, 3);

        Assert.IsTrue(swapChain.SwapChain is not null);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCreatingAdditionalSwapChainThenParametersRetainDimensions()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 swap chain creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using var swapChain = device.CreateAdditionalSwapChain(2, 3);

        Silk.NET.Direct3D9.PresentParameters parameters = swapChain.GetPresentParameters();

        Assert.AreEqual(2u, parameters.BackBufferWidth);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenGettingAdditionalSwapChainBackBufferThenDescriptionRetainsDimensions()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 swap chain back buffer queries require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using var swapChain = device.CreateAdditionalSwapChain(2, 3);
        using var backBuffer = swapChain.GetBackBuffer();

        Silk.NET.Direct3D9.SurfaceDesc description = backBuffer.GetDescription();

        Assert.AreEqual(3u, description.Height);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenPresentingAdditionalSwapChainThenReturnsOperationalState()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 presentation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using var swapChain = device.CreateAdditionalSwapChain(2, 3);

        Direct3D9DeviceState state = swapChain.Present();

        Assert.AreEqual(Direct3D9DeviceStateKind.Operational, state.Kind);
    }

    [TestMethod]
    public void WhenClassifyingOccludedPresentThenStateRemainsOperational()
    {
        Direct3D9DeviceState state = new(
            Direct3D9Factory.PresentOccludedHResult,
            Direct3D9DeviceStateSource.Present);

        Assert.IsTrue(state.IsOperational);
    }

    [TestMethod]
    public void WhenClassifyingModeChangedPresentThenRequiresDeviceRecreation()
    {
        Direct3D9DeviceState state = new(
            Direct3D9Factory.PresentModeChangedHResult,
            Direct3D9DeviceStateSource.Present);

        Assert.IsTrue(state.RequiresDeviceRecreation);
    }

    [DataTestMethod]
    [DataRow(Direct3D9Factory.DeviceLostHResult)]
    [DataRow(Direct3D9Factory.DeviceHungHResult)]
    [DataRow(Direct3D9Factory.DeviceRemovedHResult)]
    public void WhenClassifyingLostDevicePresentThenRequiresDeviceRecreation(int hResult)
    {
        Direct3D9DeviceState state = new(hResult, Direct3D9DeviceStateSource.Present);

        Assert.IsTrue(state.RequiresDeviceRecreation);
    }

    [TestMethod]
    public void WhenClassifyingUnexpectedPresentFailureThenStateIsFailure()
    {
        Direct3D9DeviceState state = new(unchecked((int) 0x80004005), Direct3D9DeviceStateSource.Present);

        Assert.AreEqual(Direct3D9DeviceStateKind.Failure, state.Kind);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenReadingDisposedAdditionalSwapChainThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 swap chain queries require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        var swapChain = device.CreateAdditionalSwapChain(2, 3);
        swapChain.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => swapChain.GetPresentParameters());
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenPresentingDisposedAdditionalSwapChainThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 presentation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        var swapChain = device.CreateAdditionalSwapChain(2, 3);
        swapChain.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => swapChain.Present());
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCheckingCreatedDeviceStateThenDeviceIsOperational()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 device state checks require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        Direct3D9DeviceState state = device.CheckDeviceState();

        Assert.IsTrue(state.IsOperational);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCheckingCreatedDeviceStateThenUsesMatchingDevicePath()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 device state checks require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        Direct3D9DeviceState state = device.CheckDeviceState();

        Assert.AreEqual(device.IsExtended, state.UsedExtendedCheck);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCheckingDisposedDeviceStateThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 device state checks require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.CheckDeviceState());
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenReadingDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 device creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = (nint) device.Device);
    }
}
