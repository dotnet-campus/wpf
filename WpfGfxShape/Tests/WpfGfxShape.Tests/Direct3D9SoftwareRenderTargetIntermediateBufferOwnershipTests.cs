using System.Runtime.Versioning;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed class Direct3D9SoftwareRenderTargetIntermediateBufferOwnershipTests
{
    [TestMethod]
    public void WhenSurfaceIsBoundThenDefaultIntermediateBuffersMatchSurfaceWidth()
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget();

        int result = renderTarget.SetSurface(CreateBinding(5));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, true, 5, 5, 5),
            (result,
                renderTarget.IntermediateBuffers.HasAllocation,
                renderTarget.IntermediateBuffers.GetBuffer(0).Colors.Length,
                renderTarget.IntermediateBuffers.GetBuffer(1).Colors.Length,
                renderTarget.IntermediateBuffers.GetBuffer(2).Colors.Length));
    }

    [TestMethod]
    public void WhenPostAllocationInitializationFailsThenOwnedBuffersAreFreedAndRetryCanSucceed()
    {
        int initializationCount = 0;
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(() =>
        {
            initializationCount++;
            return initializationCount == 1
                ? Direct3D9Factory.GenericFailureHResult
                : Direct3D9Factory.SuccessHResult;
        });

        int firstResult = renderTarget.SetSurface(CreateBinding(5));
        bool hasAllocationAfterFailure = renderTarget.IntermediateBuffers.HasAllocation;
        int secondResult = renderTarget.SetSurface(CreateBinding(3));

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, false, Direct3D9Factory.SuccessHResult, 3),
            (firstResult, hasAllocationAfterFailure, secondResult, renderTarget.IntermediateBuffers.GetBuffer(0).Colors.Length));
    }

    [TestMethod]
    public void WhenSurfaceIsReboundThenOldViewsAreInvalidatedAndNewWidthIsAllocated()
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget();
        _ = renderTarget.SetSurface(CreateBinding(5));
        Direct3D9SoftwareIntermediateBufferView oldView = renderTarget.IntermediateBuffers.GetBuffer(0);

        int result = renderTarget.SetSurface(CreateBinding(2));
        Action accessOldView = () => _ = oldView.Colors.Length;

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 2, typeof(InvalidOperationException)),
            (result,
                renderTarget.IntermediateBuffers.GetBuffer(0).Colors.Length,
                Assert.ThrowsExactly<InvalidOperationException>(accessOldView).GetType()));
    }

    [TestMethod]
    public void WhenSurfaceIsDisposedThenOwnedBuffersAndExistingViewsAreInvalidatedOnce()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget();
        _ = renderTarget.SetSurface(CreateBinding(4));
        Direct3D9SoftwareIntermediateBufferView view = renderTarget.IntermediateBuffers.GetBuffer(0);

        renderTarget.Dispose();
        renderTarget.Dispose();
        Action accessView = () => _ = view.Colors.Length;

        Assert.AreEqual(
            (false, typeof(ObjectDisposedException)),
            (renderTarget.IntermediateBuffers.HasAllocation, Assert.ThrowsExactly<ObjectDisposedException>(accessView).GetType()));
    }

    private static Direct3D9SoftwareRenderTargetSurface CreateRenderTarget(Func<int>? initializeBaseRenderTarget = null)
    {
        return new Direct3D9SoftwareRenderTargetSurface(
            width: 1,
            height: 1,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = new byte[4];
                stride = 4;
                return Direct3D9Factory.SuccessHResult;
            },
            unlockTarget: () => { },
            createSoftware3DSurface: (out Direct3D9Software3DSurface? surface) =>
            {
                surface = null;
                return Direct3D9Factory.NotAvailableHResult;
            },
            cleanup3DResources: _ => { },
            releaseSoftware3DSurface: _ => { },
            initializeBaseRenderTarget: initializeBaseRenderTarget);
    }

    private static Direct3D9BindSoftwareRenderTarget CreateBinding(uint width)
    {
        return (out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            binding = new Direct3D9SoftwareRenderTargetBinding(width, 1, MilPixelFormat.Pbgra32Bpp, 96, 96);
            return Direct3D9Factory.SuccessHResult;
        };
    }
}
