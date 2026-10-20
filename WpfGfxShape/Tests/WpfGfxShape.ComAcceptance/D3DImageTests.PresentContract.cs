using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeDisplayMode
    {
        internal uint Width, Height, RefreshRate, Format;
    }

    /// <summary>Checks backbuffer and display-space front-buffer readback for both supported windowed swap effects.</summary>
    [TestMethod]
    [DataRow(1u)]
    [DataRow(3u)]
    public void WhenNativeGpuPresentsThenFrontBufferAndClientPixelsAgree(uint swapEffect)
        => CheckNativePresentCapture(swapEffect, false);

    /// <summary>Checks independent red, blue and red submissions without accepting timestamp freshness as content identity.</summary>
    [TestMethod]
    [DataRow(1u)]
    [DataRow(3u)]
    public void WhenNativeGpuAlternatesFramesThenEachDuplicatedPixelMatchesItsSubmission(uint swapEffect)
        => CheckNativePresentCapture(swapEffect, true);

    private static void CheckNativePresentCapture(uint swapEffect, bool alternating)
    {
        nint window = CreateOutputWindow("Native present contract", 40, 40);
        nint factory = 0, device = 0, staging = 0;
        try
        {
            Assert.AreNotEqual(0, window);
            EstablishObservableWindow(window);
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual(0, factory);
            PresentParameters parameters = new() { Width = 32, Height = 16, Format = 21, Count = 1, SwapEffect = swapEffect, Window = window, Windowed = 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &device));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)device)[43])(device, 0, 0, 1, 0xff123456, 1, 0));
            Assert.AreEqual(0xff123456u, ReadNativeBackBufferPixel(device));
            nint capturedDevice = device;
            uint duplicatedPixel = 0;
            uint[] colors = alternating ? [0xffff0000u, 0xff0000ffu, 0xffff0000u, 0xff123456u] : [0xff123456u];
            foreach (uint color in colors)
            {
            duplicatedPixel = ReadDuplicatedDesktopPixel(window, () =>
            {
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)capturedDevice)[43])(capturedDevice, 0, 0, 1, color, 1, 0));
                Assert.AreEqual(color, ReadNativeBackBufferPixel(capturedDevice));
                Console.WriteLine($"NativePresent submitting qpc={System.Diagnostics.Stopwatch.GetTimestamp()} expected=0x{color:X8} swapEffect={swapEffect}");
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)capturedDevice)[17])(capturedDevice, 0, 0, window, 0));
                CompleteNativePresent(capturedDevice, window);
                Console.WriteLine($"NativePresent completed qpc={System.Diagnostics.Stopwatch.GetTimestamp()} postPresentBackbuffer=0x{ReadNativeBackBufferPixel(capturedDevice):X8} swapEffect={swapEffect}");
            });
            Console.WriteLine($"NativePresent consumed qpc={System.Diagnostics.Stopwatch.GetTimestamp()} expected=0x{color:X8} actual=0x{duplicatedPixel:X8}");
            Assert.AreEqual(color & 0x00ffffffu, duplicatedPixel & 0x00ffffffu);
            }
            DiagnoseNativePresent(window, device, false);
            NativeDisplayMode mode = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, NativeDisplayMode*, int>)(*(void***)device)[8])(device, 0, &mode));
            // GetFrontBufferData requires display-sized A8R8G8B8 system memory, not a client-sized staging surface.
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, nint*, nint, int>)(*(void***)device)[36])(device, mode.Width, mode.Height, 21, 2, &staging, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint, int>)(*(void***)device)[33])(device, 0, staging));
            WindowPoint point = new() { X = 16, Y = 8 };
            Assert.AreNotEqual(0, ClientToScreen(window, ref point));
            Assert.IsTrue(point.X >= 0 && point.Y >= 0 && point.X < mode.Width && point.Y < mode.Height);
            LockedSurface locked = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, LockedSurface*, nint, uint, int>)(*(void***)staging)[13])(staging, &locked, 0, 0x10));
            uint pixel;
            try { pixel = *(uint*)((byte*)locked.Bits + point.Y * locked.Pitch + point.X * sizeof(uint)); }
            finally { Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)staging)[14])(staging)); }
            Console.WriteLine($"NativePresentContract windowed=1 swapEffect={swapEffect} display={mode.Width}x{mode.Height} format={mode.Format} screen=({point.X},{point.Y}) frontPixel=0x{pixel:X8}");
            Assert.AreEqual(0x00123456u, duplicatedPixel & 0x00ffffffu);
            Assert.AreEqual(0x00123456u, pixel & 0x00ffffffu);
            AssertCombinedPixel(window, 0x00563412u);
        }
        finally
        {
            Release(staging); Release(device); Release(factory);
            if (window != 0) DestroyWindow(window);
        }
    }
}
