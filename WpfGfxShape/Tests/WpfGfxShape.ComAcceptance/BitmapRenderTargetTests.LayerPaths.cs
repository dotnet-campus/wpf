using System.Numerics;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    /// <summary>Verifies that native path consumers skip dependencies and locks for empty clips.</summary>
    [TestMethod]
    [TestCategory("InternalTargetContract")]
    [DataRow(16u, false, false)]
    [DataRow(26u, false, false)]
    [DataRow(16u, true, false)]
    [DataRow(26u, true, false)]
    [DataRow(16u, false, true)]
    [DataRow(26u, false, true)]
    [DataRow(16u, true, true)]
    [DataRow(26u, true, true)]
    public void WhenNativePathClipIsEmptyThenDependenciesAreSkippedAndParentIsRestored(uint format, bool infinite, bool contextClip)
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
            float* outer = stackalloc float[] { 1, 0, 3, 2 };
            Assert.AreEqual(0, begin(view, (nint)outer, 0, 0, 0, 1, 0));
            Assert.AreEqual(0, begin(view, (nint)outer, 0, 0, 0, contextClip ? 1 : 0, 0));
            byte* context = stackalloc byte[2048];
            new Span<byte>(context, 2048).Clear();
            *(int*)(context + 376) = contextClip ? 0 : 1;
            *(float*)(context + 380) = 8;
            *(float*)(context + 384) = 0;
            *(float*)(context + 388) = 9;
            *(float*)(context + 392) = 2;
            *(Matrix4x4*)(context + 408) = Matrix4x4.Identity;
            Assert.AreEqual(0, api.GetBitmap(target, &output));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)output)[8])(output, 0, 2, &heldLock));
            var path = (delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, nint, nint, int>)(*(void***)view)[10];
            var infinitePath = (delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[11];
            int result = infinite ? infinitePath(view, (nint)context, 1, 1)
                : path(view, (nint)context, 1, 1, 1, 1, 1);
            Assert.AreEqual(0, result);
            Release(heldLock);
            heldLock = 0;
            Assert.AreEqual(0, end(view));
            *(int*)(context + 376) = 1;
            // After restoration use a valid no-brush call; sentinel dependencies are only legal in the skipped branch.
            int restored = infinite ? infinitePath(view, (nint)context, 0, 0)
                : path(view, (nint)context, 0, 0, 0, 0, 0);
            Assert.AreEqual(0, restored);
            Assert.AreEqual(0, end(view));
        }
        finally { Release(heldLock); Release(output); Release(view); Release(target); Release(factory); }
    }
}
