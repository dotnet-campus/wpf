namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    /// <summary>Verifies native no-brush paths still acquire and deterministically release the surface lock.</summary>
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
    public void WhenNativePathHasNoBrushThenSurfaceLockContractIsPreserved(uint format, bool infinite, bool locked)
    {
        var api = new Api();
        nint factory = 0, target = 0, view = 0, bitmap = 0, heldLock = 0, verificationLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, format, 96, 96, 1, &target));
            Guid id = new("b73b1159-a295-4c76-bb56-c18e282ae007");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)target)[0])(target, &id, &view));
            Assert.AreEqual(0, api.GetBitmap(target, &bitmap));
            byte* context = stackalloc byte[2048];
            new Span<byte>(context, 2048).Clear();
            *(int*)(context + 376) = 1;
            var path = (delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, nint, nint, int>)(*(void***)view)[10];
            var infinitePath = (delegate* unmanaged[Stdcall]<nint, nint, nint, nint, int>)(*(void***)view)[11];
            var acquire = (delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8];
            Assert.AreEqual(0, acquire(bitmap, 0, 2, &heldLock));
            Release(locked ? 0 : heldLock);
            heldLock = locked ? heldLock : 0;
            int result = infinite ? infinitePath(view, (nint)context, 0, 0)
                : path(view, (nint)context, 0, 0, 0, 0, 0);
            Assert.AreEqual(locked ? unchecked((int)0x88982F0D) : 0, result);
            Release(heldLock);
            heldLock = 0;
            int retry = infinite ? infinitePath(view, (nint)context, 0, 0)
                : path(view, (nint)context, 0, 0, 0, 0, 0);
            Assert.AreEqual(0, retry);
            Assert.AreEqual(0, acquire(bitmap, 0, 2, &verificationLock));
        }
        finally { Release(verificationLock); Release(heldLock); Release(bitmap); Release(view); Release(target); Release(factory); }
    }
}
