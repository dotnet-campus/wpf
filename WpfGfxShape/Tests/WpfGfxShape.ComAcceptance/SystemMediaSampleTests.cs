using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class SystemMediaSampleTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed(bool candidate)
    {
        int apartment = CoInitializeEx(0, 2);
        Assert.IsTrue(apartment >= 0 || apartment == unchecked((int)0x80010106));
        nint factory = 0, media = 0, proxy = 0;
        try
        {
            nint module = NativeLibrary.Load(candidate ? AcceptancePaths.AotDll : AcceptancePaths.OriginalDll);
            var createFactory = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createMedia = (delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateMediaPlayer");
            Assert.AreEqual(0, createFactory(&factory, 0x200184C0));
            var createProxy = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)NativeLibrary.GetExport(module, "MILCreateEventProxy");
            Descriptor descriptor = new() { Dispose = &DisposeDescriptor, Raise = &RaiseEvent };
            Assert.AreEqual(0, createProxy(&descriptor, &proxy));
            Assert.AreEqual(0, createMedia(factory, proxy, 1, &media));
            nint provider = 0, renderer = 0, source = 0;
            bool compositionBegun = false, renderBegun = false;
            try
            {
                Guid providerId = new("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7");
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)media)[0])(media, &providerId, &provider));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)provider)[3])(provider, &renderer));
                Assert.AreNotEqual((nint)0, renderer);
                long time = -1;
                int ready = 0;
                int composition = ((delegate* unmanaged[Stdcall]<nint, nint, int, int, long*, int*, int>)(*(void***)renderer)[3])(renderer, 0, 0, 1, &time, &ready);
                compositionBegun = true;
                Assert.AreEqual(0, composition);
                int render = ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)(*(void***)renderer)[4])(renderer, 0, &source);
                renderBegun = render >= 0;
                Assert.AreEqual(0, render);
                Assert.AreEqual(0, ready);
            }
            finally
            {
                if (renderBegun) Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)renderer)[5])(renderer));
                if (compositionBegun) Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)renderer)[6])(renderer, 0));
                Release(source);
                Release(renderer);
                Release(provider);
            }
        }
        finally
        {
            if (media != 0)
            {
                _ = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)media)[22])(media);
                Release(media);
            }
            Release(proxy);
            Release(factory);
            if (apartment >= 0) CoUninitialize();
        }
    }

    private static void Release(nint instance)
    {
        if (instance != 0) _ = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal delegate* unmanaged[Stdcall]<Descriptor*, void> Dispose;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, int> Raise;
        internal nuint Handle;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void DisposeDescriptor(Descriptor* descriptor) { }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int RaiseEvent(Descriptor* descriptor, byte* data, uint size) => 0;

    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern int CoInitializeEx(nint reserved, uint mode);

    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern void CoUninitialize();
}
