namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    /// <summary>Verifies clipped scRGB clear without conversion to an eight-bit intermediate.</summary>
    [TestMethod]
    [TestCategory("InternalTargetContract")]
    public void WhenNativeFloatLayerClearsThenPremultipliedHdrPixelsRespectLayerAndClip()
    {
        var api = new Api();
        nint factory = 0, target = 0, view = 0, output = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 4, 1, 26, 96, 96, 1, &target));
            Assert.AreEqual(0, api.GetBitmap(target, &output));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            float* bounds = stackalloc float[] { 1, 0, 3, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, float, nint, int>)(*(void***)view)[15])(view, (nint)bounds, 0, 0, 0, 1, 0));
            byte* clip = stackalloc byte[20];
            *(int*)clip = 0;
            float* clipBounds = (float*)(clip + 4);
            clipBounds[0] = 2; clipBounds[1] = 0; clipBounds[2] = 4; clipBounds[3] = 1;
            float* color = stackalloc float[] { 3, -0.5f, 0.123456f, 0.5f };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)view)[4])(view, color, (nint)clip));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)view)[16])(view));
            float* pixels = stackalloc float[16];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, float*, int>)(*(void***)output)[7])(output, 0, 64, 64, pixels));
            CollectionAssert.AreEqual(new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 1.5f, -0.25f, 0.061728f, 0.5f, 0, 0, 0, 0 }, new ReadOnlySpan<float>(pixels, 16).ToArray());
        }
        finally { Release(output); Release(view); Release(target); Release(factory); }
    }
}
