namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    /// <summary>Verifies native no-intersection semantics and restoration after an empty layer.</summary>
    [TestMethod]
    [TestCategory("InternalTargetContract")]
    [DataRow(16u, 0)]
    [DataRow(16u, 1)]
    [DataRow(16u, 2)]
    [DataRow(16u, 3)]
    [DataRow(26u, 0)]
    [DataRow(26u, 1)]
    [DataRow(26u, 2)]
    [DataRow(26u, 3)]
    public void WhenNativeLayerBoundsContainNaNThenEmptyLayerConsumesClearAndRestoresParent(uint format, int coordinate)
    {
        var api = new Api();
        nint factory = 0, target = 0, view = 0, output = 0, heldLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 4, 2, format, 96, 96, 1, &target));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            var begin = (delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, float, nint, int>)(*(void***)view)[15];
            var end = (delegate* unmanaged[Stdcall]<nint, int>)(*(void***)view)[16];
            var bounds = (delegate* unmanaged[Stdcall]<nint, float*, void>)(*(void***)view)[3];
            var clear = (delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)view)[4];
            float* outer = stackalloc float[] { 1, 0, 3, 2 };
            float* invalid = stackalloc float[] { 1, 0, 3, 2 };
            invalid[coordinate] = float.NaN;
            float* actual = stackalloc float[4];
            float* color = stackalloc float[] { 1, 0, 0, 1 };
            Assert.AreEqual(0, begin(view, (nint)outer, 0, 0, 0, 1, 0));
            // Nonempty fixup layers are unsupported; an empty layer must not consume their pointers.
            Assert.AreEqual(0, begin(view, (nint)invalid, 0, 1, 1, 0.5f, 1));
            bounds(view, actual);
            Assert.AreEqual((0f, 0f, 0f, 0f), (actual[0], actual[1], actual[2], actual[3]));
            Assert.AreEqual(0, api.GetBitmap(target, &output));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)output)[8])(output, 0, 2, &heldLock));
            Assert.AreEqual(0, clear(view, color, 0));
            Release(heldLock);
            heldLock = 0;
            Assert.AreEqual(0, end(view));
            bounds(view, actual);
            Assert.AreEqual((1f, 0f, 3f, 2f), (actual[0], actual[1], actual[2], actual[3]));
            Assert.AreEqual(0, clear(view, color, 0));
            Assert.AreEqual(0, end(view));
            bounds(view, actual);
            Assert.AreEqual((0f, 0f, 4f, 2f), (actual[0], actual[1], actual[2], actual[3]));
        }
        finally { Release(heldLock); Release(output); Release(view); Release(target); Release(factory); }
    }
}
