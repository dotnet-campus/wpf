using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    [DataRow(0f, true)]
    [DataRow(1f, true)]
    [DataRow(-1f, true)]
    [DataRow(0.03125f, false)]
    [DataRow(-0.03125f, true)]
    [DataRow(0.96875f, true)]
    [DataRow(0.96f, false)]
    [DataRow(1.03125f, false)]
    public void WhenTestingPixelBoundaryThenFix4HalfUpQuantizationMatchesNative(float value, bool expected)
    {
        bool result = Direct3D9SurfaceRenderTarget.IsOnPixelBoundary(value);

        Assert.AreEqual(expected, result);
    }
}
