using System.Runtime.Versioning;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[SupportedOSPlatform("windows5.1.2600")]
public sealed class Direct3D9SoftwareRenderTargetAlphaTests
{
    [TestMethod]
    [DataRow((int) MilPixelFormat.Pbgra32Bpp, true)]
    [DataRow((int) MilPixelFormat.Bgr32Bpp, false)]
    public void WhenTargetFormatIsQueriedThenOnlyPremultipliedBgraHasAlpha(int pixelFormat, bool expected)
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget((MilPixelFormat) pixelFormat);

        bool result = renderTarget.HasAlpha();

        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void WhenSurfaceIsReboundThenAlphaStateUsesNewTargetFormat()
    {
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(MilPixelFormat.Pbgra32Bpp);

        int result = renderTarget.SetSurface((out Direct3D9SoftwareRenderTargetBinding binding) =>
        {
            binding = new Direct3D9SoftwareRenderTargetBinding(
                4,
                3,
                MilPixelFormat.Bgr32Bpp,
                96,
                96);
            return Direct3D9Factory.SuccessHResult;
        });

        Assert.AreEqual((Direct3D9Factory.SuccessHResult, false), (result, renderTarget.HasAlpha()));
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(true, true)]
    public void WhenDrawingGlyphsToAlphaTargetThenClearTypeRequiresForceHint(bool forceClearType, bool expected)
    {
        bool? targetSupportsClearType = null;
        using Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(
            MilPixelFormat.Pbgra32Bpp,
            forceClearType,
            supportsClearType => targetSupportsClearType = supportsClearType);

        int result = renderTarget.DrawGlyphs(new Direct3D9SurfaceRect(0, 0, 4, 3));

        Assert.AreEqual((Direct3D9Factory.SuccessHResult, expected), (result, targetSupportsClearType));
    }

    [TestMethod]
    public void WhenTargetIsDisposedThenAlphaQueryIsRejected()
    {
        Direct3D9SoftwareRenderTargetSurface renderTarget = CreateRenderTarget(MilPixelFormat.Pbgra32Bpp);
        renderTarget.Dispose();

        Action query = () => renderTarget.HasAlpha();

        Assert.ThrowsExactly<ObjectDisposedException>(query);
    }

    private static Direct3D9SoftwareRenderTargetSurface CreateRenderTarget(
        MilPixelFormat pixelFormat,
        bool forceClearType = false,
        Action<bool>? observeClearTypeSupport = null)
    {
        byte[] pixels = new byte[48];
        return new Direct3D9SoftwareRenderTargetSurface(
            width: 4,
            height: 3,
            lockTarget: (out byte[] targetPixels, out int targetStride) =>
            {
                targetPixels = pixels;
                targetStride = 16;
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
            pixelFormat: pixelFormat,
            ensureGlyphBrushRealization: () => Direct3D9Factory.SuccessHResult,
            hasRealizedGlyphBrush: () => true,
            drawGlyphsSoftwareRenderTarget: (_, _, _, _, _, supportsClearType) =>
            {
                observeClearTypeSupport?.Invoke(supportsClearType);
                return Direct3D9Factory.SuccessHResult;
            },
            forceClearType: forceClearType);
    }
}
