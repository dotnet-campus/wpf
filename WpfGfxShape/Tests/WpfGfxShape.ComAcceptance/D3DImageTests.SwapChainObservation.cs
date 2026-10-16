using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ObservationMonitorInfo
    {
        internal uint Size;
        internal WindowRectangle Monitor, Work;
        internal uint Flags;
        internal fixed char Device[32];
    }

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint window);
    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint window, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetMonitorInfoW(nint monitor, ref ObservationMonitorInfo info);

    private static void PresentNativeControl(nint window, nint device, bool extended)
    {
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)device)[43])(device, 0, 0, 1, 0xff123456, 1, 0));
        int result = extended
            ? ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, uint, int>)(*(void***)device)[121])(device, 0, 0, window, 0, 0)
            : ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)device)[17])(device, 0, 0, window, 0);
        Assert.AreEqual(0, result);
        CompleteNativePresent(device, window);
    }

    private static void LogNativeSwapChain(nint window, nint device)
    {
        nint swapChain = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[14])(device, 0, &swapChain));
            PresentParameters parameters = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, PresentParameters*, int>)(*(void***)swapChain)[9])(swapChain, &parameters));
            ObservationMonitorInfo monitor = new() { Size = (uint)sizeof(ObservationMonitorInfo) };
            Assert.AreNotEqual(0, GetMonitorInfoW(MonitorFromWindow(window, 2), ref monitor));
            Console.WriteLine($"ActualSwapChain width={parameters.Width} height={parameters.Height} format={parameters.Format} count={parameters.Count} swapEffect={parameters.SwapEffect} hDeviceWindow=0x{parameters.Window:X} windowed={parameters.Windowed} dpi={GetDpiForWindow(window)} monitor=({monitor.Monitor.Left},{monitor.Monitor.Top},{monitor.Monitor.Right},{monitor.Monitor.Bottom}) sourceRect=NULL destinationRect=NULL hDestWindowOverride=0x{window:X}");
        }
        finally { Release(swapChain); }
    }
}
