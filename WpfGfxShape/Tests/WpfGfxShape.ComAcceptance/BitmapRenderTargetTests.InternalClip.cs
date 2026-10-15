using System.Numerics;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    [TestMethod]
    [TestCategory("InternalTargetContract")]
    [DataRow(16u, 0f, 0f)]
    [DataRow(26u, 0f, 0f)]
    [DataRow(16u, 2f, 3f)]
    [DataRow(26u, 2f, 3f)]
    [DataRow(16u, -2f, -1f)]
    [DataRow(26u, -2f, -1f)]
    public void WhenInternalBitmapClipHasNoTargetAreaThenLockedTargetIsNotConsumed(uint format, float left, float right)
    {
        var api = new Api();
        nint factory = 0, target = 0, sourceTarget = 0, source = 0, output = 0, view = 0, heldLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, format, 96, 96, 1, &target));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, format, 96, 96, 1, &sourceTarget));
            Assert.AreEqual(0, api.GetBitmap(target, &output));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)sourceTarget)[7])(sourceTarget, &source));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            byte* context = stackalloc byte[2048];
            byte* state = stackalloc byte[116];
            new Span<byte>(context, 2048).Clear();
            new Span<byte>(state, 116).Clear();
            *(float*)(context + 380) = left;
            *(float*)(context + 388) = right;
            *(float*)(context + 392) = 1;
            *(nint*)(context + 400) = (nint)state;
            *(Matrix4x4*)(context + 408) = Matrix4x4.Identity;
            *(Matrix4x4*)(state + 4) = Matrix4x4.Identity;
            *(uint*)(state + 84) = 1;
            *(float*)(state + 92) = MathF.Sqrt(2);
            *(uint*)(state + 96) = 1;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)output)[8])(output, 0, 2, &heldLock));

            int result = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[8])(view, (nint)context, source, 0);

            Assert.AreEqual(0, result);
        }
        finally
        {
            Release(heldLock); Release(view); Release(output); Release(source); Release(sourceTarget); Release(target); Release(factory);
        }
    }
}
