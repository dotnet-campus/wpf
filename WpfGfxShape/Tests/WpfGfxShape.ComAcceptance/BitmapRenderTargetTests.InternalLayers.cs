using System.Numerics;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    [TestMethod]
    [TestCategory("InternalTargetContract")]
    [DataRow(16u, false)]
    [DataRow(26u, false)]
    [DataRow(16u, true)]
    [DataRow(26u, true)]
    public void WhenNativeLayersEndOrAbortThenParentBoundsAreRestored(uint format, bool abort)
    {
        var api = new Api();
        nint factory = 0, target = 0, view = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 4, 2, format, 96, 96, 1, &target));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            var begin = (delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, float, nint, int>)(*(void***)view)[15];
            var end = (delegate* unmanaged[Stdcall]<nint, int>)(*(void***)view)[16];
            float* outer = stackalloc float[] { 1, 0, 3, 2 };
            float* inner = stackalloc float[] { 2, 0, 4, 1 };
            float* bounds = stackalloc float[4];
            Assert.AreEqual(0, begin(view, (nint)outer, 0, 0, 0, 1, 0));
            Assert.AreEqual(0, begin(view, (nint)inner, 0, 0, 0, 1, 0));
            ((delegate* unmanaged[Stdcall]<nint, float*, void>)(*(void***)view)[3])(view, bounds);
            Assert.AreEqual((2f, 0f, 3f, 1f), (bounds[0], bounds[1], bounds[2], bounds[3]));
            if (abort)
                ((delegate* unmanaged[Stdcall]<nint, void>)(*(void***)view)[17])(view);
            else
            {
                Assert.AreEqual(0, end(view));
                Assert.AreEqual(0, end(view));
            }
            ((delegate* unmanaged[Stdcall]<nint, float*, void>)(*(void***)view)[3])(view, bounds);
            Assert.AreEqual((0f, 0f, 4f, 2f), (bounds[0], bounds[1], bounds[2], bounds[3]));
        }
        finally { Release(view); Release(target); Release(factory); }
    }

    [TestMethod]
    [TestCategory("InternalTargetContract")]
    [DataRow(16u)]
    [DataRow(26u)]
    public void WhenNativeZeroOpacityLayerDrawsThenLockedTargetIsNotConsumed(uint format)
    {
        var api = new Api();
        nint factory = 0, target = 0, view = 0, sourceTarget = 0, source = 0, output = 0, heldLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, format, 96, 96, 1, &target));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, format, 96, 96, 1, &sourceTarget));
            Assert.AreEqual(0, api.GetBitmap(target, &output));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)sourceTarget)[7])(sourceTarget, &source));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            float* bounds = stackalloc float[] { 0, 0, 2, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, float, nint, int>)(*(void***)view)[15])(view, (nint)bounds, 0, 0, 0, 0, 0));
            byte* context = stackalloc byte[2048];
            byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear();
            new Span<byte>(state, 116).Clear();
            *(int*)(context + InternalContextLayout.Clip) = 1;
            *(nint*)(context + InternalContextLayout.RenderState) = (nint)state;
            *(Matrix4x4*)(context + InternalContextLayout.WorldToDevice) = Matrix4x4.Identity;
            *(uint*)(state + 84) = 1;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)output)[8])(output, 0, 2, &heldLock));
            int result = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[8])(view, (nint)context, source, 0);
            Assert.AreEqual(0, result);
        }
        finally { Release(heldLock); Release(output); Release(source); Release(sourceTarget); Release(view); Release(target); Release(factory); }
    }

    [TestMethod]
    [TestCategory("InternalTargetContract")]
    public void WhenNativeLayerNeedsAlphaFixupThenFailurePreservesParentLayer()
    {
        var api = new Api();
        nint factory = 0, target = 0, view = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 4, 2, 16, 96, 96, 1, &target));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            var begin = (delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, float, nint, int>)(*(void***)view)[15];
            float* bounds = stackalloc float[] { 1, 0, 3, 2 };
            Assert.AreEqual(0, begin(view, (nint)bounds, 0, 0, 0, 1, 0));
            Assert.AreEqual(unchecked((int)0x80004001), begin(view, (nint)bounds, 0, 0, 0, 0.5f, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)view)[16])(view));
            ((delegate* unmanaged[Stdcall]<nint, float*, void>)(*(void***)view)[3])(view, bounds);
            Assert.AreEqual((0f, 0f, 4f, 2f), (bounds[0], bounds[1], bounds[2], bounds[3]));
        }
        finally { Release(view); Release(target); Release(factory); }
    }
}
