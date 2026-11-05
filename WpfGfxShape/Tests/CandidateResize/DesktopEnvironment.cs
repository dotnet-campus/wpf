using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;

namespace NativeAcceptance.Pcore;

internal static class DesktopEnvironment
{
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] private static extern nint GetThreadDesktop(uint thread);
    [DllImport("user32.dll", SetLastError = true)] private static extern nint OpenInputDesktop(uint flags, bool inherit, uint access);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool CloseDesktop(nint desktop);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern bool GetUserObjectInformation(nint handle, int index, [Out] char[] value, uint length, out uint needed);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint process);
    [DllImport("dwmapi.dll")] private static extern int DwmIsCompositionEnabled(out bool enabled);

    private static object Name(nint desktop)
    {
        var value = new char[512];
        bool success = GetUserObjectInformation(desktop, 2, value, 1024, out uint needed);
        int error = success ? 0 : Marshal.GetLastWin32Error();
        return new { handle = (long)desktop, success, name = new string(value).TrimEnd('\0'), error, needed };
    }

    internal static void Record(Window window, int cycle, string directory)
    {
        long qpc = Stopwatch.GetTimestamp();
        nint hwnd = new WindowInteropHelper(window).Handle;
        uint ownerThread = GetWindowThreadProcessId(hwnd, out uint ownerProcess);
        uint currentThread = GetCurrentThreadId();
        nint input = OpenInputDesktop(0, false, 1);
        int inputError = input == 0 ? Marshal.GetLastWin32Error() : 0;
        object inputDesktop = input == 0 ? new { error = inputError } : Name(input);
        bool closed = input == 0 || CloseDesktop(input);
        int dwmResult = DwmIsCompositionEnabled(out bool compositionEnabled);
        File.AppendAllText(Path.Combine(directory, "desktop-environment.jsonl"), JsonSerializer.Serialize(new { cycle, qpc, frequency = Stopwatch.Frequency, hwnd = (long)hwnd, ownerThread, ownerProcess, currentThread, session = Process.GetCurrentProcess().SessionId, ownerDesktop = Name(GetThreadDesktop(ownerThread)), inputDesktop, inputClosed = closed, dwmResult, compositionEnabled, lockState = "Unknown; desktop names do not prove lock state" }) + Environment.NewLine);
    }
}
