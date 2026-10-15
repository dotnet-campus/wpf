using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
public sealed unsafe class SystemVideoSampleTests
{
    [TestMethod]
    public void WhenMediaIsNullThenSampleAcquisitionReturnsInvalidArgument()
    {
        Assert.AreEqual(unchecked((int)0x80070057), SystemVideoSample.Acquire(0, out _));
    }

    [TestMethod]
    public void WhenRealSystemMediaHasNoFrameThenSoftwareSampleLeaseCanBeClosed()
    {
        int apartment = CoInitializeEx(0, 2);
        Assert.IsTrue(apartment >= 0 || apartment == unchecked((int)0x80010106));
        nint factory = 0, media = 0, proxy = 0;
        try
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Tests", "WpfGfxShape.WpfReference", "TestResults", "original-wpf-e07cbceaf453471dacb4cb12c99bdbc2.json")))
                directory = directory.Parent;
            Assert.IsNotNull(directory);
            string fixture = Path.Combine(directory.FullName, "Tests", "WpfGfxShape.WpfReference", "TestResults", "original-wpf-e07cbceaf453471dacb4cb12c99bdbc2.json");
            using JsonDocument reference = JsonDocument.Parse(File.ReadAllText(fixture));
            nint module = NativeLibrary.Load(reference.RootElement.GetProperty("NativeDll").GetString()!);
            var createFactory = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createMedia = (delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateMediaPlayer");
            Assert.AreEqual(0, createFactory(&factory, 0x200184C0));
            var createProxy = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)NativeLibrary.GetExport(module, "MILCreateEventProxy");
            Descriptor descriptor = new() { Dispose = &DisposeDescriptor, Raise = &RaiseEvent };
            Assert.AreEqual(0, createProxy(&descriptor, &proxy));
            Assert.AreEqual(0, createMedia(factory, proxy, 1, &media));
            int result = SystemVideoSample.Acquire(media, out SystemVideoSample? sample);
            using (sample)
            {
                Assert.AreEqual(0, result);
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
