using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
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
    public void WhenCreatingDefaultDeviceThenRetainsAdapterDisplayFormat()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 device creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Displaymode displayMode = objects.GetAdapterDisplayMode(0);
        using Direct3D9Device device = objects.CreateDevice();

        Assert.AreEqual(displayMode.Format, device.DisplayMode.Format);
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
    public void WhenCheckingDefaultRenderTargetFormatThenDepthStencilMatchSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 depth-stencil matching requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.CheckRenderTargetFormat(Format.X8R8G8B8, _ => 0, out _);

        Assert.AreEqual(0, result);
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
    public void WhenTestingLockableRenderTargetDeviceContextThenResultIsCached()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 surface device-context testing requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using Direct3D9Surface surface = device.CreateRenderTarget(2, 3, Format.X8R8G8B8, lockable: true);
        Direct3D9TargetFormatTestStatus status = new();

        int first = surface.TestGetDeviceContext(status);
        int second = surface.TestGetDeviceContext(status);

        Assert.AreEqual((first, true), (second, status.WasGetDeviceContextTested));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenTestingDisposedRenderTargetDeviceContextThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 surface device-context testing requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        Direct3D9Surface surface = device.CreateRenderTarget(2, 3, Format.X8R8G8B8, lockable: true);
        surface.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => surface.TestGetDeviceContext(new Direct3D9TargetFormatTestStatus()));
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
    public void WhenPresentingAdditionalSwapChainThenReturnsUsableState()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 presentation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using var swapChain = device.CreateAdditionalSwapChain(2, 3);

        Direct3D9DeviceState state = swapChain.Present();

        Assert.IsTrue(state.IsOperational);
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
    public void WhenSettingRenderTargetThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 render-target changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using Direct3D9Surface renderTarget = device.CreateRenderTarget(16, 16);

        int result = device.SetRenderTarget(renderTarget);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingDepthStencilSurfaceThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 depth-stencil changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetDepthStencilSurface(null);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingDepthStencilSurfaceAfterDisposeThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 depth-stencil changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetDepthStencilSurface(null));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenCreatingTextureThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 texture creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        using Direct3D9Texture texture = device.CreateTexture(16, 16, pool: device.ManagedPool);

        Assert.IsTrue(texture.IsValid);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenAccessingTextureAfterDisposeThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 texture creation requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();
        Direct3D9Texture texture = device.CreateTexture(16, 16, pool: device.ManagedPool);
        texture.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = texture.Texture);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenSettingAlphaBlendEnableThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 render-state changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetRenderState(Renderstatetype.Alphablendenable, 1);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingPixelShaderThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 pixel-shader changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetPixelShader(null);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingVertexShaderThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 vertex-shader changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetVertexShader(null);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingStreamSourceThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 stream-source changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetStreamSource(0, null, 0, 0);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingIndicesThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 index-buffer changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetIndices(null);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenSettingTextureStageStateThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 texture-stage changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetTextureStageState(0, Texturestagestatetype.Colorop, (uint) Textureop.Selectarg1);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenSettingSamplerStateThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 sampler-state changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetSamplerState(0, Samplerstatetype.Magfilter, (uint) Texturefiltertype.Point);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingTextureThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 texture changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetTexture(0, null);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenSettingTextureVertexFormatThenDirect3DCallSucceeds()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 vertex-format changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int result = device.SetFlexibleVertexFormat((uint) (D3D9.FvfXyz | D3D9.FvfDiffuse | D3D9.FvfTex2));

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenBeginningAndEndingSceneThenDirect3DCallsSucceed()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 scene changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        using Direct3D9Device device = objects.CreateDevice();

        int beginResult = device.BeginScene();
        int endResult = device.EndScene();

        Assert.AreEqual((0, 0), (beginResult, endResult));
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
    public void WhenBeginningSceneOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 scene changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.BeginScene());
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenEndingSceneOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 scene changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.EndScene());
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenSettingTextureVertexFormatOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 vertex-format changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetFlexibleVertexFormat(0));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenDrawingPrimitiveOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 primitive drawing requires Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.DrawPrimitiveUp(Primitivetype.Trianglefan, 2, null, 32));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenSettingRenderStateOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 render-state changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.SetRenderState(Renderstatetype.Alphablendenable, 1));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingPixelShaderOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 pixel-shader changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetPixelShader(null));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingVertexShaderOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 vertex-shader changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetVertexShader(null));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingStreamSourceOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 stream-source changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetStreamSource(0, null, 0, 0));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingIndicesOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 index-buffer changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetIndices(null));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenSettingTextureStageStateOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 texture-stage changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.SetTextureStageState(0, Texturestagestatetype.Colorop, (uint) Textureop.Selectarg1));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenSettingSamplerStateOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 sampler-state changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => device.SetSamplerState(0, Samplerstatetype.Magfilter, (uint) Texturefiltertype.Point));
    }

    [TestMethod]
    [SupportedOSPlatform("windows5.1.2600")]
    public void WhenClearingTextureOnDisposedDeviceThenThrowsObjectDisposedException()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Direct3D 9 texture changes require Windows.");
        }

        using Direct3D9Objects objects = Direct3D9Factory.Create();
        Direct3D9Device device = objects.CreateDevice();
        device.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => device.SetTexture(0, null));
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
