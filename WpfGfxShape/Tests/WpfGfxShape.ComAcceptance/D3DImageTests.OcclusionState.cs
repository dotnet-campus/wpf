using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct OcclusionDisplayMode
    {
        internal uint Size, Width, Height, Refresh, Format, Scanline;
    }

    private delegate int OcclusionWindowCallback(nint window, nint parameter);
    [DllImport("user32.dll")]
    private static extern int EnumWindows(OcclusionWindowCallback callback, nint parameter);
    [DllImport("shell32.dll")]
    private static extern int SHQueryUserNotificationState(out int state);

    private static void LogOccludedDeviceState(nint window, nint device, int originalResult)
    {
        // Query the original device before the original assertion; never retry presentation or restore state.
        long qpc = Stopwatch.GetTimestamp();
        int state = ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)device)[128])(device, window);
        ExclusiveCreationParameters creation = default;
        int creationHr = ((delegate* unmanaged[Stdcall]<nint, ExclusiveCreationParameters*, int>)(*(void***)device)[9])(device, &creation);
        OcclusionDisplayMode display = new() { Size = (uint)sizeof(OcclusionDisplayMode) };
        uint rotation = 0;
        int displayHr = ((delegate* unmanaged[Stdcall]<nint, uint, OcclusionDisplayMode*, uint*, int>)(*(void***)device)[133])(device, 0, &display, &rotation);
        nint factory = 0, chain = 0;
        try
        {
            int factoryHr = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)device)[6])(device, &factory);
            NativeDisplayMode adapterMode = default;
            int? adapterHr = null;
            nint adapterMonitor = 0;
            if (factoryHr == 0 && factory != 0 && creationHr == 0)
            {
                adapterHr = ((delegate* unmanaged[Stdcall]<nint, uint, NativeDisplayMode*, int>)(*(void***)factory)[8])(factory, creation.Adapter, &adapterMode);
                adapterMonitor = ((delegate* unmanaged[Stdcall]<nint, uint, nint>)(*(void***)factory)[15])(factory, creation.Adapter);
            }
            int chainHr = ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)device)[14])(device, 0, &chain);
            PresentParameters actual = default;
            int? parametersHr = chainHr == 0 && chain != 0
                ? ((delegate* unmanaged[Stdcall]<nint, PresentParameters*, int>)(*(void***)chain)[9])(chain, &actual) : null;
            uint ownerThread = GetWindowThreadProcessId(window, out uint ownerPid);
            int cloakHr = DwmGetWindowAttribute(window, 14, out uint cloak, sizeof(uint));
            nint input = OpenInputDesktop(0, 0, 1);
            int inputError = input == 0 ? Marshal.GetLastWin32Error() : 0;
            string inputName;
            try
            {
                char* name = stackalloc char[256];
                inputName = input != 0 && GetUserObjectInformationW(input, 2, name, 512, out _) != 0 ? new string(name) : "Unavailable";
            }
            finally { if (input != 0) CloseDesktop(input); }
            var windows = new List<object>();
            int enumeration = EnumWindows((hwnd, _) =>
            {
                uint thread = GetWindowThreadProcessId(hwnd, out uint pid);
                GetWindowRect(hwnd, out WindowRectangle bounds);
                int hr = DwmGetWindowAttribute(hwnd, 14, out uint cloaked, sizeof(uint));
                windows.Add(new { Hwnd = (long)hwnd, Pid = pid, Thread = thread, Visible = IsWindowVisible(hwnd), Iconic = IsIconic(hwnd), CloakHr = hr, Cloaked = cloaked,
                    Left = bounds.Left, Top = bounds.Top, Right = bounds.Right, Bottom = bounds.Bottom, Monitor = (long)MonitorFromWindow(hwnd, 2) });
                return 1;
            }, 0);
            int notificationHr = SHQueryUserNotificationState(out int notificationState);
            using Process process = Process.GetCurrentProcess();
            var record = new
            {
                Utc = DateTimeOffset.UtcNow, Qpc = qpc, Frequency = Stopwatch.Frequency, Pid = Environment.ProcessId, process.SessionId,
                TestAssembly = typeof(D3DImageTests).Assembly.Location, TestMvid = typeof(D3DImageTests).Assembly.ManifestModule.ModuleVersionId,
                CallStack = Environment.StackTrace, OriginalNativeHr = originalResult, OriginalAssertionExpected = 0,
                Device = (long)device, Hwnd = (long)window, OwnerPid = ownerPid, OwnerThread = ownerThread, CurrentThread = GetCurrentThreadId(),
                CheckDeviceStateHr = state, CreationHr = creationHr, creation.Adapter, creation.Type, Focus = (long)creation.Focus, creation.Flags,
                DisplayHr = displayHr, Display = new { display.Size, display.Width, display.Height, display.Refresh, display.Format, display.Scanline, Rotation = rotation },
                FactoryHr = factoryHr, AdapterHr = adapterHr, AdapterMode = new { adapterMode.Width, adapterMode.Height, adapterMode.RefreshRate, adapterMode.Format }, AdapterMonitor = (long)adapterMonitor,
                ChainHr = chainHr, ParametersHr = parametersHr, Parameters = new { actual.Width, actual.Height, actual.Format, actual.Count, actual.SwapEffect, Window = (long)actual.Window, actual.Windowed, actual.Refresh, actual.Interval },
                Foreground = (long)GetForegroundWindow(), Visible = IsWindowVisible(window), Iconic = IsIconic(window), CloakHr = cloakHr, Cloaked = cloak,
                InputDesktop = inputName, InputError = inputError, WindowMonitor = (long)MonitorFromWindow(window, 2),
                NotificationHr = notificationHr, NotificationState = notificationState, FullscreenOwner = "Not exposed by D3D9; notification state and window bounds are not proof of ownership",
                Enumeration = enumeration, Windows = windows
            };
            string json = JsonSerializer.Serialize(record);
            Console.WriteLine("PresentOcclusionState " + json);
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"present-occlusion-{Environment.ProcessId}-{Guid.NewGuid():N}.json"), json);
        }
        finally { Release(chain); Release(factory); }
    }
}
