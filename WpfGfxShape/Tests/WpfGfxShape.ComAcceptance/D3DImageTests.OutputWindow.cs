using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OutputWindowClass
    {
        internal uint Size, Style;
        internal delegate* unmanaged[Stdcall]<nint, uint, nuint, nint, nint> Procedure;
        internal int ClassExtra, WindowExtra;
        internal nint Instance, Icon, Cursor, Background;
        internal char* Menu;
        internal char* Name;
        internal nint SmallIcon;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClassExW(ref OutputWindowClass windowClass);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint DefWindowProcW(nint window, uint message, nuint wParam, nint lParam);
    [DllImport("user32.dll")]
    private static extern int ValidateRect(nint window, nint rectangle);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandleW(string? name);

    private static nint CreateOutputWindow(string title, int x, int y)
        => (windowThread ?? throw new InvalidOperationException()).Invoke(() => CreateOwnedOutputWindow(title, x, y));

    private static nint CreateOwnedOutputWindow(string title, int x, int y)
    {
        const string className = "WpfGfxShape.HardwareOutput";
        fixed (char* name = className)
        {
            OutputWindowClass windowClass = new()
            {
                Size = (uint)sizeof(OutputWindowClass),
                Instance = GetModuleHandleW(null),
                Name = name,
                Procedure = &OutputWindowProcedure
            };
            ushort atom = RegisterClassExW(ref windowClass);
            Assert.IsTrue(atom != 0 || Marshal.GetLastWin32Error() == 1410);
            return CreateWindowExW(8, className, title, 0x90000000, x, y, 32, 16, 0, 0, windowClass.Instance, 0);
        }
    }

    private static void CompleteNativePresent(nint device, nint window)
    {
        nint query = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[118])(device, 8, &query));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, int>)(*(void***)query)[6])(query, 1));
            long deadline = Environment.TickCount64 + 5000;
            int result;
            do
            {
                ProcessWindowMessages(window);
                result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, int>)(*(void***)query)[7])(query, 0, 0, 1);
                Assert.IsTrue(result >= 0);
                Assert.IsTrue(Environment.TickCount64 < deadline);
            } while (result == 1);
            ProcessWindowMessages(window);
            Assert.AreEqual(0, DwmFlush());
            ProcessWindowMessages(window);
            Assert.AreEqual(0, DwmFlush());
        }
        finally { Release(query); }
    }

    private static void SendOutputWindowSettings(delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int> send, nint channel, uint target, nint window)
    {
        Assert.AreNotEqual(0, GetWindowRect(window, out WindowRectangle bounds));
        byte* packet = stackalloc byte[72];
        new Span<byte>(packet, 72).Clear();
        *(uint*)packet = 0x33;
        *(uint*)(packet + 4) = target;
        *(WindowRectangle*)(packet + 8) = bounds;
        *(float*)(packet + 32) = 1;
        *(int*)(packet + 44) = 1;
        Assert.AreEqual(0, send(packet, 72, 0, channel));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static nint OutputWindowProcedure(nint window, uint message, nuint wParam, nint lParam)
    {
#if NATIVE_PAINT_BASELINE
        if (NpbWindowMessage(window, message, wParam, lParam)) return 0;
#endif
        // The consumer owns every output pixel; a stock EDIT control must not repaint over Present.
        if (message == 0x14) return 1;
        if (message == 0x0f) { ValidateRect(window, 0); return 0; }
        return DefWindowProcW(window, message, wParam, lParam);
    }
}
