using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [DllImport("user32.dll")]
    private static extern int ShowWindow(nint window, int command);

    /// <summary>Verifies GPU pixels without depending on desktop visibility.</summary>
    [TestMethod]
    public void WhenNativeGpuClearsBackBufferThenStagingReadbackContainsClearColor()
        => RunNativeReadbackControl(false);

    /// <summary>Verifies the desktop prerequisite separately from production rendering.</summary>
    [TestMethod]
    public void WhenNativeGpuPresentsThenTopmostWindowIsObservable()
        => RunNativeReadbackControl(true);

    private static void RunNativeReadbackControl(bool observeDesktop)
    {
        nint window = CreateOutputWindow("Native GPU readback control", 40, 40);
        nint factory = 0, device = 0;
        try
        {
            Assert.AreNotEqual(0, window);
            factory = Direct3DCreate9(32);
            Assert.AreNotEqual(0, factory);
            PresentParameters parameters = new() { Width = 32, Height = 16, Format = 21, Count = 1, SwapEffect = 3, Window = window, Windowed = 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, nint, uint, PresentParameters*, nint*, int>)(*(void***)factory)[16])(factory, 0, 1, window, 0x26, &parameters, &device));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)device)[43])(device, 0, 0, 1, 0xff123456, 1, 0));
            Assert.AreEqual(0xff123456u, ReadNativeBackBufferPixel(device));
            if (observeDesktop)
            {
                EstablishObservableWindow(window);
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)device)[17])(device, 0, 0, window, 0));
                CompleteNativePresent(device, window);
                DiagnoseNativePresent(window, device, false);
                AssertCombinedPixel(window, 0x00563412);
            }
        }
        finally
        {
            Release(device); Release(factory);
            if (window != 0) DestroyWindow(window);
        }
    }

    private static uint ReadNativeBackBufferPixel(nint device)
    {
        nint backBuffer = 0, staging = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, nint*, int>)(*(void***)device)[18])(device, 0, 0, 0, &backBuffer));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, uint, nint*, nint, int>)(*(void***)device)[36])(device, 32, 16, 21, 2, &staging, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint, int>)(*(void***)device)[32])(device, backBuffer, staging));
            LockedSurface locked = default;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, LockedSurface*, nint, uint, int>)(*(void***)staging)[13])(staging, &locked, 0, 0x10));
            uint pixel;
            try { pixel = *(uint*)((byte*)locked.Bits + 8 * locked.Pitch + 16 * sizeof(uint)); }
            finally { Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)staging)[14])(staging)); }
            Console.WriteLine($"NativeGpuStaging pixel=0x{pixel:X8} expected=0xFF123456");
            return pixel;
        }
        finally { Release(staging); Release(backBuffer); }
    }

    private static void EstablishObservableWindow(nint window)
    {
        ShowWindow(window, 5);
        Assert.AreNotEqual(0, SetWindowPos(window, -1, 0, 0, 0, 0, 0x0043));
        SetForegroundWindow(window);
        UpdateWindow(window);
        ProcessWindowMessages(window);
        WindowPoint point = new() { X = 16, Y = 8 };
        Assert.AreNotEqual(0, ClientToScreen(window, ref point));
        nint covering = WindowFromPoint(point);
        char* name = stackalloc char[256];
        int count = GetClassNameW(covering, name, 256);
        string coveringClass = new(name, 0, Math.Max(0, count));
        nint desktop = OpenInputDesktop(0, 0, 1);
        string desktopName = "Unavailable";
        try
        {
            if (desktop != 0 && GetUserObjectInformationW(desktop, 2, name, 512, out _) != 0)
                desktopName = new string(name);
        }
        finally { if (desktop != 0) CloseDesktop(desktop); }
        Console.WriteLine($"DesktopPrerequisite target=0x{window:X} cover=0x{covering:X} coverClass={coveringClass} inputDesktop={desktopName} foreground=0x{GetForegroundWindow():X} point=({point.X},{point.Y})");
        Assert.AreEqual(window, covering);
    }
}
