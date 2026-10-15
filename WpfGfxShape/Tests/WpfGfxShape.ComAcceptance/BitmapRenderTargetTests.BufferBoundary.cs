namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class BitmapRenderTargetTests
{
    [TestMethod]
    [DataRow(16u, 8u)]
    [DataRow(26u, 32u)]
    public void WhenBitmapCopyBufferIsShortThenFailureLeavesBitmapReadableAfterLockRelease(uint format, uint rowBytes)
    {
        var api = new Api();
        nint factory = 0, target = 0, bitmap = 0, bitmapLock = 0;
        try
        {
            Assert.AreEqual(0, api.Create(&factory, 0x200184C0));
            Assert.AreEqual(0, api.CreateTarget(factory, 2, 1, format, 96, 96, 1, &target));
            Assert.AreEqual(0, api.GetBitmap(target, &bitmap));
            byte* buffer = stackalloc byte[32];
            new Span<byte>(buffer, 32).Fill(0xA5);
            var copy = (delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7];
            Assert.AreEqual(unchecked((int)0x88982F8C), copy(bitmap, 0, rowBytes, rowBytes - 1, buffer));
            Assert.IsTrue(new ReadOnlySpan<byte>(buffer, 32).ToArray().All(value => value == 0xA5));
            var acquire = (delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8];
            Assert.AreEqual(unchecked((int)0x80070057), acquire(bitmap, 0, 1, null));
            Assert.AreEqual(0, acquire(bitmap, 0, 1, &bitmapLock));
            uint stride = 0, size = 0;
            byte* pixels = null;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels));
            Assert.AreEqual(rowBytes, stride);
            Assert.IsGreaterThanOrEqualTo(rowBytes, size);
            Assert.AreNotEqual((nint)0, (nint)pixels);
            Release(bitmapLock); bitmapLock = 0;
            Assert.AreEqual(0, copy(bitmap, 0, rowBytes, rowBytes, buffer));
            Assert.IsTrue(new ReadOnlySpan<byte>(buffer, (int)rowBytes).ToArray().All(value => value == 0));
        }
        finally { Release(bitmapLock); Release(bitmap); Release(target); Release(factory); }
    }
}
