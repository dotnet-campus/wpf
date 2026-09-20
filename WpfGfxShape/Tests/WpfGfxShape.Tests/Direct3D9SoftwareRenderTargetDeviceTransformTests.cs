using System.Numerics;
using System.Runtime.Versioning;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed class Direct3D9SoftwareRenderTargetDeviceTransformTests
{
    [TestMethod]
    public void WhenRenderTargetIsCreatedThenInitialDpiDefinesDeviceTransformAndUniquenessStartsAtOne()
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(dpiX: 120, dpiY: 144);

        Assert.AreEqual(
            (1u, Matrix3x2.CreateScale(120, 144)),
            (renderTarget.GetResizeUniqueness(), renderTarget.GetDeviceTransform()));
    }

    [TestMethod]
    [DataRow(4u, 3u)]
    [DataRow(8u, 6u)]
    public void WhenSurfaceIsReboundThenEverySuccessfulBindingAdvancesUniquenessAndCommitsDpiTransform(
        uint width,
        uint height)
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget();

        int result = renderTarget.SetSurface(CreateBinding(width, height, 120.25, 144.75));

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 2u, Matrix3x2.CreateScale(120.25f, 144.75f)),
            (result, renderTarget.GetResizeUniqueness(), renderTarget.GetDeviceTransform()));
    }

    [TestMethod]
    public void WhenBindingFailsThenUniquenessAdvancesButDeviceTransformIsNotPartiallyChanged()
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget();
        Matrix3x2 oldTransform = renderTarget.GetDeviceTransform();

        int result = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            binding = new Direct3D9SoftwareRenderTargetBinding(8, 6, MilPixelFormat.Bgr32Bpp, 120, 144);
            return Direct3D9Factory.GenericFailureHResult;
        });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 2u, oldTransform),
            (result, renderTarget.GetResizeUniqueness(), renderTarget.GetDeviceTransform()));
    }

    [TestMethod]
    [DataRow(0.0, 96.0)]
    [DataRow(96.0, -1.0)]
    [DataRow(double.NaN, 96.0)]
    [DataRow(96.0, double.PositiveInfinity)]
    [DataRow(double.MaxValue, 96.0)]
    public void WhenSurfaceDpiCannotFormDeviceTransformThenBindingFailsWithoutPartialState(
        double dpiX,
        double dpiY)
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget();
        Direct3D9SoftwareRenderTargetState oldState = renderTarget.State;
        Matrix3x2 oldTransform = renderTarget.GetDeviceTransform();

        int result = renderTarget.SetSurface(CreateBinding(8, 6, dpiX, dpiY));

        Assert.AreEqual(
            (Direct3D9Factory.InvalidArgumentHResult, oldState, oldTransform),
            (result, renderTarget.State, renderTarget.GetDeviceTransform()));
    }

    [TestMethod]
    public void WhenPostDpiInitializationFailsThenTransformIsPreservedAndRetryCommitsNewTransform()
    {
        int allocationCount = 0;
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            allocateIntermediateBuffers: _ => ++allocationCount == 1
                ? Direct3D9Factory.OutOfMemoryHResult
                : Direct3D9Factory.SuccessHResult);
        Matrix3x2 oldTransform = renderTarget.GetDeviceTransform();

        int firstResult = renderTarget.SetSurface(CreateBinding(8, 6, 120, 144));
        Matrix3x2 transformAfterFailure = renderTarget.GetDeviceTransform();
        int secondResult = renderTarget.SetSurface(CreateBinding(8, 6, 120, 144));

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, oldTransform, Direct3D9Factory.SuccessHResult, 3u, Matrix3x2.CreateScale(120, 144)),
            (firstResult, transformAfterFailure, secondResult, renderTarget.GetResizeUniqueness(), renderTarget.GetDeviceTransform()));
    }

    [TestMethod]
    public void WhenRenderTargetIsDisposedThenUniquenessAndTransformQueriesAndBindingAreRejected()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget();
        renderTarget.Dispose();

        Action queryUniqueness = () => renderTarget.GetResizeUniqueness();
        Action queryTransform = () => renderTarget.GetDeviceTransform();
        Action bind = () => renderTarget.SetSurface(CreateBinding(4, 3, 96, 96));

        Assert.AreEqual(
            (typeof(ObjectDisposedException), typeof(ObjectDisposedException), typeof(ObjectDisposedException)),
            (Assert.ThrowsExactly<ObjectDisposedException>(queryUniqueness).GetType(),
                Assert.ThrowsExactly<ObjectDisposedException>(queryTransform).GetType(),
                Assert.ThrowsExactly<ObjectDisposedException>(bind).GetType()));
    }

    private static Direct3D9SoftwareRenderTargetSurface CreateRenderTarget(
        double dpiX = 96,
        double dpiY = 96,
        Func<Direct3D9SoftwareRenderTargetState, int>? allocateIntermediateBuffers = null)
    {
        return new Direct3D9SoftwareRenderTargetSurface(
            width: 4,
            height: 3,
            lockTarget: (out byte[] pixels, out int stride) =>
            {
                pixels = new byte[48];
                stride = 16;
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
            dpiX: dpiX,
            dpiY: dpiY,
            allocateIntermediateBuffers: allocateIntermediateBuffers);
    }

    private static Direct3D9BindSoftwareRenderTarget CreateBinding(
        uint width,
        uint height,
        double dpiX,
        double dpiY)
    {
        return (out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            binding = new Direct3D9SoftwareRenderTargetBinding(
                width,
                height,
                MilPixelFormat.Bgr32Bpp,
                dpiX,
                dpiY);
            return Direct3D9Factory.SuccessHResult;
        };
    }
}
