using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class PackedBitmapTests
{
    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(2, 2)]
    [DataRow(3, 4)]
    public void WhenUnalignedIndexedLockIsWrittenThenNeighborsAndPaletteSnapshotArePreserved(int code, int bits)
    {
        int apartment = CoInitializeEx(0, 0);
        Assert.IsTrue(apartment >= 0 || apartment == unchecked((int)0x80010106));
        nint factory = 0, palette = 0, copiedPalette = 0, owner = 0, bitmap = 0, mil = 0, bitmapLock = 0, milLock = 0, identity = 0;
        try
        {
            Guid clsid = new("cacaf262-9370-4615-a13b-9f5539da4c0a"), iid = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
            Assert.AreEqual(0, CoCreateInstance(&clsid, 0, 1, &iid, &factory));
            var createPalette = (delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)factory)[9];
            Assert.AreEqual(0, createPalette(factory, &palette));
            Assert.AreEqual(0, createPalette(factory, &copiedPalette));
            uint* colors = stackalloc uint[] { 0xff000000, 0xffffffff };
            var initialize = (delegate* unmanaged[Stdcall]<nint, uint*, uint, int>)(*(void***)palette)[4];
            Assert.AreEqual(0, initialize(palette, colors, 2));
            string path = typeof(PackedBitmapTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!;
            nint module = NativeLibrary.Load(path);
            var create = (delegate* unmanaged[Stdcall]<uint, uint, double, double, Guid*, nint, nint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapCreate");
            var getBack = (delegate* unmanaged[Stdcall]<nint, nint*, uint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapGetBackBuffer");
            Guid format = new($"6fddc324-4e03-4bfe-b185-3d77768dc9{code:x2}");
            Assert.AreEqual(0, create(9, 2, 96, 96, &format, palette, &owner));
            Assert.AreEqual(0, getBack(owner, &bitmap, null));
            colors[1] = 0xff00ff00;
            Assert.AreEqual(0, initialize(palette, colors, 2));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)bitmap)[6])(bitmap, copiedPalette));
            uint count = 0;
            uint* copied = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint*, uint*, int>)(*(void***)copiedPalette)[9])(copiedPalette, 2, copied, &count));
            Assert.AreEqual(0xffffffffu, copied[1]);
            Guid milId = new("c46d6fde-0e59-4cfd-89b1-c935906dfbd9");
            Assert.AreEqual(0, Query(bitmap, &milId, &mil));
            int nativeFormat = -1;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int*, int>)(*(void***)mil)[4])(mil, &nativeFormat));
            Assert.AreEqual(code, nativeFormat);
            Guid unknown = new("00000000-0000-0000-c000-000000000046");
            Assert.AreEqual(0, Query(mil, &unknown, &identity));
            Assert.AreEqual(bitmap, identity);
            int* rectangle = stackalloc int[] { 1, 0, 1, 2 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, uint, nint*, int>)(*(void***)mil)[8])(mil, rectangle, 3, &milLock));
            Guid wicLockId = new("00000123-a8f2-4877-ba0a-fd2b6645fb94");
            Assert.AreEqual(0, Query(milLock, &wicLockId, &bitmapLock));
            Guid lockFormat = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)bitmapLock)[6])(bitmapLock, &lockFormat));
            Assert.AreEqual(format, lockFormat);
            uint stride = 0, size = 0;
            byte* data = null;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)milLock)[4])(milLock, &stride));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)milLock)[5])(milLock, &size, &data));
            data[0] = data[stride] = 0xff;
            Release(bitmapLock); bitmapLock = 0;
            Release(milLock); milLock = 0;
            byte* pixels = stackalloc byte[16];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, null, 8, 16, pixels));
            Assert.AreEqual((byte)(((1 << bits) - 1) << (8 - 2 * bits)), pixels[0]);
            Assert.AreEqual(pixels[0], pixels[8]);
            Assert.AreEqual((byte)0, pixels[1]);
        }
        finally
        {
            Release(bitmapLock); Release(milLock); Release(identity); Release(mil); Release(bitmap); Release(owner);
            Release(copiedPalette); Release(palette); Release(factory);
            if (apartment >= 0) CoUninitialize();
        }
    }

    [TestMethod]
    [DataRow(5, 1)]
    [DataRow(6, 2)]
    [DataRow(7, 4)]
    public void WhenPackedGrayIsCopiedThenUnalignedPixelsAndDirtyTokensMatch(int code, int bits)
    {
        int apartment = CoInitializeEx(0, 0);
        Assert.IsTrue(apartment >= 0 || apartment == unchecked((int)0x80010106));
        nint owner = 0, bitmap = 0, mil = 0, bitmapLock = 0;
        try
        {
            string path = typeof(PackedBitmapTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!;
            nint module = NativeLibrary.Load(path);
            var create = (delegate* unmanaged[Stdcall]<uint, uint, double, double, Guid*, nint, nint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapCreate");
            var getBack = (delegate* unmanaged[Stdcall]<nint, nint*, uint*, int>)NativeLibrary.GetExport(module, "MILSwDoubleBufferedBitmapGetBackBuffer");
            Guid format = new($"6fddc324-4e03-4bfe-b185-3d77768dc9{code:x2}");
            Assert.AreEqual(0, create(9, 1, 96, 96, &format, 0, &owner));
            Assert.AreEqual(0, getBack(owner, &bitmap, null));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, null, 2, &bitmapLock));
            uint size = 0;
            byte* pixels = null;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels));
            pixels[0] = 0x5a;
            Release(bitmapLock); bitmapLock = 0;
            int* rectangle = stackalloc int[] { 1, 0, 1, 1 };
            byte copied = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, rectangle, 1, 1, &copied));
            Assert.AreEqual((byte)((0x5a << bits) & (0xff << (8 - bits))), copied);
            Guid milId = new("c46d6fde-0e59-4cfd-89b1-c935906dfbd9");
            Assert.AreEqual(0, Query(bitmap, &milId, &mil));
            uint token = 0;
            ((delegate* unmanaged[Stdcall]<nint, uint*, void>)(*(void***)mil)[14])(mil, &token);
            nint rectangles = -1;
            uint count = 99;
            var dirty = (delegate* unmanaged[Stdcall]<nint, nint*, uint*, uint*, byte>)(*(void***)mil)[12];
            Assert.AreEqual((byte)1, dirty(mil, &rectangles, &count, &token));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, int>)(*(void***)mil)[11])(mil, null));
            Assert.AreEqual((byte)0, dirty(mil, &rectangles, &count, &token));
            Assert.AreEqual(((nint)0, 0u), (rectangles, count));
            Assert.AreEqual((byte)1, dirty(mil, &rectangles, &count, &token));
        }
        finally
        {
            Release(bitmapLock); Release(mil); Release(bitmap); Release(owner);
            if (apartment >= 0) CoUninitialize();
        }
    }

    private static int Query(nint instance, Guid* iid, nint* result) => ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)instance)[0])(instance, iid, result);
    private static void Release(nint instance) { if (instance != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance); }
    [DllImport("ole32.dll")] private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll")] private static extern void CoUninitialize();
    [DllImport("ole32.dll")] private static extern int CoCreateInstance(Guid* clsid, nint outer, uint context, Guid* iid, nint* instance);
}
