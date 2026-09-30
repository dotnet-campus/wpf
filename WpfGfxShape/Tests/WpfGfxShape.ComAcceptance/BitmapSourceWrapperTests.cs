using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class BitmapSourceWrapperTests
{
    private static nint Module => NativeLibrary.Load(typeof(BitmapSourceWrapperTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!);

    [TestMethod]
    public void WhenSourceIsNullThenWrapperRejectsIt()
    {
        var create = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(Module, "MilResource_CreateCWICWrapperBitmap");
        nint output = 0;
        Assert.AreEqual(unchecked((int)0x80070057), create(0, &output));
    }

    [TestMethod]
    public void WhenOutputIsNullThenWrapperRejectsIt()
    {
        using var wic = new SystemWicBitmap();
        nint bitmap = wic.Create(2, 1);
        try
        {
            var create = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(Module, "MilResource_CreateCWICWrapperBitmap");
            Assert.AreEqual(unchecked((int)0x80070057), create(bitmap, null));
        }
        finally { Release(bitmap); }
    }

    [TestMethod]
    public void WhenSourceOnlyInputIsReleasedThenWrapperRetainsExactPixels()
    {
        using var wic = new SystemWicBitmap();
        nint bitmap = wic.Create(2, 1), source = 0, wrapper = 0, bitmapLock = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock));
            uint size; byte* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels));
            ((uint*)pixels)[0] = 0xff123456; ((uint*)pixels)[1] = 0x80402010;
            Release(bitmapLock); bitmapLock = 0;
            source = wic.CreateSource(bitmap);
            Guid bitmapId = new("00000121-a8f2-4877-ba0a-fd2b6645fb94");
            nint unsupported = 0;
            Assert.AreEqual(unchecked((int)0x80004002), ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)source)[0])(source, &bitmapId, &unsupported));
            var create = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(Module, "MilResource_CreateCWICWrapperBitmap");
            Assert.AreEqual(0, create(source, &wrapper));
            Release(source); source = 0;
            Release(bitmap); bitmap = 0;
            uint* output = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)wrapper)[7])(wrapper, 0, 8, 8, (byte*)output));
            Assert.AreEqual((0xff123456u, 0x80402010u), (output[0], output[1]));
        }
        finally { Release(bitmapLock); Release(wrapper); Release(source); Release(bitmap); }
    }

    private static void Release(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }
}
