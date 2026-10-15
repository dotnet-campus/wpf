namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    [TestMethod]
    [TestCategory("InternalTargetContract")]
    public void WhenNativeOpaqueLayerClearsThenOnlyLayerBoundsChange()
    {
        var api = new Api();
        nint factory = 0, target = 0, view = 0, output = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 4, 1, 16, 96, 96, 1, &target));
            Assert.AreEqual(0, api.GetBitmap(target, &output));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            float* bounds = stackalloc float[] { 1, 0, 3, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, float, nint, int>)(*(void***)view)[15])(view, (nint)bounds, 0, 0, 0, 1, 0));
            float* red = stackalloc float[] { 1, 0, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)view)[4])(view, red, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)view)[16])(view));
            uint* pixels = stackalloc uint[4];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 16, 16, pixels));
            Assert.AreEqual((0u, 0xffff0000u, 0xffff0000u, 0u), (pixels[0], pixels[1], pixels[2], pixels[3]));
        }
        finally { Release(output); Release(view); Release(target); Release(factory); }
    }
}
