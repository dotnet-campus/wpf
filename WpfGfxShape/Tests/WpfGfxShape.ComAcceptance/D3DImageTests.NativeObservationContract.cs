#if NATIVE_OBSERVATION_CONTRACT
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [ThreadStatic] private static NativeObservationScope? nativeObservation;

    private sealed class NativeObservationScope : IDisposable
    {
        private readonly StreamWriter writer;
        private nint window;
        private int sample;

        internal NativeObservationScope()
        {
            Directory.CreateDirectory("Results");
            writer = new StreamWriter("Results/native-observation-contract.jsonl", false, Encoding.UTF8) { AutoFlush = true };
            var assembly = typeof(D3DImageTests).Assembly;
            Write("identity", new { testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))), mvid = assembly.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size });
            nativeObservation = this;
        }

        private void Write(string operation, object value)
            => writer.WriteLine(JsonSerializer.Serialize(new { utc = DateTimeOffset.UtcNow, qpc = Stopwatch.GetTimestamp(), pid = Environment.ProcessId, tid = NoCurrentThread(), hwnd = window.ToInt64(), sample, operation, value }));

        private static string FailureMessage => new System.Resources.ResourceManager("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(D3DImageTests).Assembly).GetString("NativeObservationContract") ?? nameof(NativeObservationScope);

        private void Require(string operation, bool success, int? error = null)
        {
            Write(operation, new { success, error });
            Assert.IsTrue(success, FailureMessage + " " + operation + " " + error);
        }

        private void Token()
        {
            bool opened = NoOpenProcessToken(NoCurrentProcess(), 8, out nint token);
            int error = opened ? 0 : Marshal.GetLastWin32Error();
            Write("OpenProcessToken", new { requestedAccess = "TOKEN_QUERY", accessMask = 8, opened, error });
            Require("token query handle", opened, error);
            try
            {
                uint session = 0, elevation = 0;
                bool sessionOk = NoTokenInfo(token, 12, &session, 4, out uint sessionLength);
                int sessionError = sessionOk ? 0 : Marshal.GetLastWin32Error();
                bool elevationOk = NoTokenInfo(token, 20, &elevation, 4, out uint elevationLength);
                int elevationError = elevationOk ? 0 : Marshal.GetLastWin32Error();
                NoTokenInfo(token, 25, null, 0, out uint needed);
                int sizeError = Marshal.GetLastWin32Error();
                Write("token integrity capacity", new { needed, error = sizeError });
                Require("token integrity capacity", needed >= (uint)IntPtr.Size + 4 && needed <= 65536, sizeError);
                byte[] buffer = new byte[needed];
                bool integrityOk;
                int integrityError;
                uint integrity = 0;
                fixed (byte* p = buffer)
                {
                    integrityOk = NoTokenInfo(token, 25, p, needed, out uint used);
                    integrityError = integrityOk ? 0 : Marshal.GetLastWin32Error();
                    if (integrityOk)
                    {
                        nint sid = *(nint*)p;
                        bool valid = NoIsValidSid(sid);
                        Write("token integrity SID", new { valid, used });
                        Require("token integrity SID", valid);
                        byte count = *NoSidCount(sid);
                        Require("token integrity subauthority count", count > 0);
                        integrity = *NoSidAuthority(sid, (uint)(count - 1));
                    }
                }
                Write("token", new { sessionOk, sessionError, sessionLength, session, elevationOk, elevationError, elevationLength, elevation, integrityOk, integrityError, integrity });
                Require("token session", sessionOk, sessionError);
                Require("token elevation", elevationOk, elevationError);
                Require("token integrity", integrityOk, integrityError);
            }
            finally
            {
                bool closed = NoCloseHandle(token);
                CleanupRecord("CloseHandle token", new { closed, error = closed ? 0 : Marshal.GetLastWin32Error() });
            }
        }

        private (string Name, bool Input) Desktop(nint desktop, string role)
        {
            Require(role + " desktop handle", desktop != 0, desktop == 0 ? Marshal.GetLastWin32Error() : null);
            byte[] name = new byte[1024];
            bool named;
            uint used;
            fixed (byte* p = name) named = NoUserInfo(desktop, 2, p, (uint)name.Length, out used);
            int nameError = named ? 0 : Marshal.GetLastWin32Error();
            int input = 0;
            bool queried = NoUserInfo(desktop, 6, &input, 4, out uint inputLength);
            int inputError = queried ? 0 : Marshal.GetLastWin32Error();
            string text = named ? Encoding.Unicode.GetString(name, 0, (int)Math.Min(used, (uint)name.Length)).TrimEnd('\0') : "";
            Write(role + " desktop", new { name = text, named, nameError, used, queried, inputError, inputLength, input, borrowed = role != "input" });
            Require(role + " UOI_NAME", named, nameError);
            Require(role + " UOI_IO", queried, inputError);
            return (text, input != 0);
        }

        private NoPoint Coordinate(string role)
        {
            NoPoint point = new() { X = 16, Y = 8 };
            bool converted = NoClientToScreen(window, ref point);
            int error = converted ? 0 : Marshal.GetLastWin32Error();
            nint context = NoThreadDpi();
            Write(role + " coordinates", new { converted, error, x = point.X, y = point.Y, threadDpiContext = context.ToInt64(), awareness = NoDpiAwareness(context), thread = NoCurrentThread() });
            Require(role + " ClientToScreen", converted, error);
            Require(role + " thread DPI", context != 0);
            return point;
        }

        internal void Window(nint hwnd)
        {
            window = hwnd;
            Token();
            uint ownerTid = NoWindowThread(hwnd, out uint ownerPid);
            int ownerError = ownerTid != 0 ? 0 : Marshal.GetLastWin32Error();
            bool sessionOk = NoSession((uint)Environment.ProcessId, out uint session);
            int sessionError = sessionOk ? 0 : Marshal.GetLastWin32Error();
            bool ownerSessionOk = NoSession(ownerPid, out uint ownerSession);
            int ownerSessionError = ownerSessionOk ? 0 : Marshal.GetLastWin32Error();
            Write("actual owner", new { ownerTid, ownerPid, ownerError, sessionOk, sessionError, session, ownerSessionOk, ownerSessionError, ownerSession });
            Require("owner TID/PID", ownerTid != 0 && ownerPid == (uint)Environment.ProcessId, ownerError);
            Require("process session", sessionOk, sessionError);
            Require("owner session", ownerSessionOk && session == ownerSession, ownerSessionError);
            nint station = NoStation();
            NoFlags flags = default;
            bool flagsOk = NoUserInfo(station, 1, &flags, (uint)sizeof(NoFlags), out uint flagsLength);
            int flagsError = flagsOk ? 0 : Marshal.GetLastWin32Error();
            byte[] stationName = new byte[1024];
            bool stationNamed;
            uint stationNameLength;
            fixed (byte* p = stationName) stationNamed = NoUserInfo(station, 2, p, (uint)stationName.Length, out stationNameLength);
            int stationNameError = stationNamed ? 0 : Marshal.GetLastWin32Error();
            Write("process windowstation", new { borrowed = true, flagsOk, flagsError, flagsLength, flags.Inherit, flags.Reserved, flags.Flags, stationNamed, stationNameError, name = stationNamed ? Encoding.Unicode.GetString(stationName, 0, (int)Math.Min(stationNameLength, (uint)stationName.Length)).TrimEnd('\0') : "" });
            Require("station flags", flagsOk, flagsError);
            Require("station name", stationNamed, stationNameError);
            var testDesktop = Desktop(NoDesktop(NoCurrentThread()), "test");
            NoPoint testPoint = Coordinate("test");
            Require("owner dispatcher", windowThread is not null);
            var owner = windowThread!.Invoke(() =>
            {
                uint actualTid = NoCurrentThread();
                var desktop = Desktop(NoDesktop(actualTid), "owner");
                NoPoint point = Coordinate("owner");
                return (actualTid, desktop.Input, point);
            });
            Require("actual owner invoked", owner.actualTid == ownerTid);
            Desktop(NoDesktop(ownerTid), "owner from test");
            nint input = NoInputDesktop(0, false, 1);
            int inputError = input != 0 ? 0 : Marshal.GetLastWin32Error();
            Write("OpenInputDesktop", new { opened = input != 0, requestedAccess = 1, inputError });
            Require("OpenInputDesktop READOBJECTS", input != 0, inputError);
            bool member = false;
            try
            {
                Desktop(input, "input");
                NoEnumCallback callback = (candidate, _) => { if (candidate == hwnd) member = true; return true; };
                bool enumerated = NoEnumDesktop(input, callback, 0);
                int enumError = enumerated ? 0 : Marshal.GetLastWin32Error();
                GC.KeepAlive(callback);
                Write("input exact HWND membership", new { enumerated, enumError, member });
                Require("EnumDesktopWindows", enumerated, enumError);
            }
            finally
            {
                bool closed = NoCloseDesktop(input);
                CleanupRecord("CloseDesktop input", new { closed, error = closed ? 0 : Marshal.GetLastWin32Error() });
            }
            int processDpiHr = NoProcessDpi(0, out int processDpi);
            nint windowDpi = NoWindowDpi(hwnd);
            uint dpi = NoDpiForWindow(hwnd);
            Write("DPI", new { processDpiHr, processDpi, windowContext = windowDpi.ToInt64(), windowAwareness = NoDpiAwareness(windowDpi), dpi });
            int compositionHr = NoComposition(out bool enabled);
            int cloakHr = NoCloak(hwnd, 14, out uint cloak, 4);
            Marshal.SetLastPInvokeError(0);
            int style = NoWindowLong(hwnd, -20);
            int styleError = style == 0 ? Marshal.GetLastWin32Error() : 0;
            Write("DWM applicability", new { compositionHr, enabled, cloakHr, cloak, style, styleError, noRedirectionBitmap = (style & 0x200000) != 0 });
            Display(testPoint);
            Write("H1 snapshot", new { stationVisible = (flags.Flags & 1) != 0, ownerInput = owner.Input, member, testInput = testDesktop.Input, ownerSession, session, atomic = false });
            Require("H1 station visible", (flags.Flags & 1) != 0);
            Require("H1 owner input", owner.Input);
            Require("H1 exact HWND input member", member);
            Require("DPI process query", processDpiHr >= 0);
            Require("DPI window query", windowDpi != 0 && dpi != 0);
            Require("DWM queries", compositionHr >= 0 && cloakHr >= 0 && styleError == 0);
            Require("DWM applicable", enabled && cloak == 0);
            Require("H2 owner/test coordinates", testPoint.X == owner.point.X && testPoint.Y == owner.point.Y);
        }

        private void Display(NoPoint point)
        {
            nint monitor = NoMonitorWindow(window, 0), pointMonitor = NoMonitorPoint(point, 0);
            NoMonitor info = new() { Size = (uint)sizeof(NoMonitor) };
            bool infoOk = NoMonitorInfo(monitor, &info);
            int infoError = infoOk ? 0 : Marshal.GetLastWin32Error();
            string device = new(info.Device);
            Write("target monitor", new { present = monitor != 0, samePointMonitor = monitor == pointMonitor, infoOk, infoError, device, left = info.Monitor.Left, top = info.Monitor.Top, right = info.Monitor.Right, bottom = info.Monitor.Bottom });
            Require("monitor target/point", monitor != 0 && monitor == pointMonitor);
            Require("GetMonitorInfoW", infoOk, infoError);
            for (int attempt = 0; attempt < 2; attempt++)
            {
                int sizeResult = NoDisplaySizes(2, out uint paths, out uint modes);
                Write("CCD capacity", new { attempt, sizeResult, paths, modes, pathLayout = sizeof(NoPath), modeLayout = sizeof(NoMode) });
                Require("GetDisplayConfigBufferSizes", sizeResult == 0, sizeResult);
                NoPath[] pathBuffer = new NoPath[paths];
                NoMode[] modeBuffer = new NoMode[modes];
                int result;
                fixed (NoPath* p = pathBuffer) fixed (NoMode* m = modeBuffer) result = NoDisplayQuery(2, ref paths, p, ref modes, m, 0);
                Write("QueryDisplayConfig", new { attempt, result, paths, modes });
                if (result == 122 && attempt == 0) continue;
                Require("QueryDisplayConfig", result == 0, result);
                bool matched = false;
                for (int i = 0; i < paths; i++)
                {
                    NoPath path = pathBuffer[i];
                    NoSourceName source = new() { Header = new() { Type = 1, Size = (uint)sizeof(NoSourceName), Adapter = path.Source.Adapter, Id = path.Source.Id } };
                    int nameResult = NoDisplayName(&source.Header);
                    Require("DisplayConfigGetDeviceInfo source", nameResult == 0, nameResult);
                    if (!string.Equals(new string(source.Name), device, StringComparison.OrdinalIgnoreCase)) continue;
                    Require("CCD valid source index", path.Source.ModeIndex < modes);
                    NoMode mode = modeBuffer[path.Source.ModeIndex];
                    bool sourceIdentity = mode.Type == 1 && mode.Id == path.Source.Id && mode.Adapter.Value == path.Source.Adapter.Value;
                    bool contains = point.X >= mode.X && point.Y >= mode.Y && (long)point.X < (long)mode.X + mode.Width && (long)point.Y < (long)mode.Y + mode.Height;
                    Write("target CCD source", new { device, sourceIdentity, contains, luid = path.Source.Adapter.Value, sourceId = path.Source.Id, targetId = path.Target.Id, modeIndex = path.Source.ModeIndex, mode.X, mode.Y, mode.Width, mode.Height });
                    Require("CCD source identity/point", sourceIdentity && contains);
                    matched = true;
                }
                Require("CCD target source matched", matched);
                return;
            }
        }

        private bool Dc(nint dc, string role, int x, int y)
        {
            uint type = NoObjectType(dc);
            int map = NoMapMode(dc);
            bool wo = NoWindowOrg(dc, out NoPoint windowOrg); int woError = wo ? 0 : Marshal.GetLastWin32Error();
            bool vo = NoViewportOrg(dc, out NoPoint viewportOrg); int voError = vo ? 0 : Marshal.GetLastWin32Error();
            bool we = NoWindowExt(dc, out NoPoint windowExt); int weError = we ? 0 : Marshal.GetLastWin32Error();
            bool ve = NoViewportExt(dc, out NoPoint viewportExt); int veError = ve ? 0 : Marshal.GetLastWin32Error();
            NoPoint mapped = new() { X = x, Y = y };
            bool converted = NoLpToDp(dc, &mapped, 1); int conversionError = converted ? 0 : Marshal.GetLastWin32Error();
            int clip = NoClip(dc, out NoRect rect);
            bool visible = NoPointVisible(dc, x, y);
            Write(role + " same DC contract", new { dc = dc.ToInt64(), type, map, wo, woError, windowOrgX = windowOrg.X, windowOrgY = windowOrg.Y, vo, voError, viewportOrgX = viewportOrg.X, viewportOrgY = viewportOrg.Y, we, weError, windowExtX = windowExt.X, windowExtY = windowExt.Y, ve, veError, viewportExtX = viewportExt.X, viewportExtY = viewportExt.Y, converted, conversionError, logicalX = x, logicalY = y, deviceX = mapped.X, deviceY = mapped.Y, clip, rect.Left, rect.Top, rect.Right, rect.Bottom, visible });
            return dc != 0 && type == 3 && map != 0 && wo && vo && we && ve && converted && clip > 1 && visible && mapped.X == x && mapped.Y == y;
        }

        internal void BeforePixel(nint hwnd, nint dc)
        {
            Require("scope HWND", hwnd == window);
            sample++;
            Write("GetDC original", new { returned = dc.ToInt64(), lastErrorIsNotContractual = true });
            Require("GetDC original nonnull", dc != 0);
            bool valid = Dc(dc, "window", 16, 8);
            if (sample == 1)
            {
                NoPoint point = Coordinate("test at first sample");
                Require("owner dispatcher at sample", windowThread is not null);
                NoPoint owner = windowThread!.Invoke(() => Coordinate("owner at first sample"));
                nint screen = GetDC(0);
                Write("GetDC screen", new { returned = screen.ToInt64(), lastErrorIsNotContractual = true });
                try
                {
                    Require("GetDC screen nonnull", screen != 0);
                    bool screenValid = Dc(screen, "screen", point.X, point.Y);
                    uint raw = GetPixel(screen, point.X, point.Y);
                    Write("screen single read", new { raw = raw.ToString("X8"), validColor = raw != uint.MaxValue, screenValid, sameOwnerCoordinate = point.X == owner.X && point.Y == owner.Y, notCompositionEvidence = true, lastErrorIsNotContractual = true });
                    Write("screen comparison applicability", new { established = screenValid && raw != uint.MaxValue && point.X == owner.X && point.Y == owner.Y });
                }
                finally
                {
                    if (screen != 0) Released("screen", ReleaseDC(0, screen));
                }
            }
            Require("H2 original DC contract", valid);
        }

        internal void Pixel(uint raw, uint expected)
        {
            Write("original GetPixel", new { raw = raw.ToString("X8"), expected = expected.ToString("X8"), validColor = raw != uint.MaxValue, colorFormat = "00BBGGRR", lastErrorIsNotContractual = true });
            Require("GetPixel not CLR_INVALID", raw != uint.MaxValue);
        }

        private void CleanupRecord(string operation, object value)
        {
            try { Write(operation, value); }
            catch (IOException error) { Console.Error.WriteLine(FailureMessage + " " + operation + " " + error); }
            catch (UnauthorizedAccessException error) { Console.Error.WriteLine(FailureMessage + " " + operation + " " + error); }
        }

        internal void Released(string role, int result)
            => CleanupRecord(role + " ReleaseDC", new { result, sameThread = true });

        /// <summary>Releases only the diagnostic writer without replacing the fixture's primary failure.</summary>
        public void Dispose()
        {
            nativeObservation = null;
            CleanupRecord("scope complete", new { samples = sample });
            try { writer.Dispose(); }
            catch (IOException error) { Console.Error.WriteLine(FailureMessage + " " + error); }
            catch (UnauthorizedAccessException error) { Console.Error.WriteLine(FailureMessage + " " + error); }
        }
    }

    [StructLayout(LayoutKind.Sequential)] private struct NoPoint { internal int X, Y; }
    [StructLayout(LayoutKind.Sequential)] private struct NoRect { internal int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct NoFlags { internal int Inherit, Reserved; internal uint Flags; }
    [StructLayout(LayoutKind.Sequential)] private struct NoLuid { internal uint Low; internal int High; internal long Value => ((long)High << 32) | Low; }
    [StructLayout(LayoutKind.Sequential)] private struct NoSource { internal NoLuid Adapter; internal uint Id, ModeIndex, Status; }
    [StructLayout(LayoutKind.Sequential)] private struct NoTarget { internal NoLuid Adapter; internal uint Id, ModeIndex, Technology, Rotation, Scaling, RefreshNumerator, RefreshDenominator, Scanline; internal int Available; internal uint Status; }
    [StructLayout(LayoutKind.Sequential)] private struct NoPath { internal NoSource Source; internal NoTarget Target; internal uint Flags; }
    [StructLayout(LayoutKind.Explicit, Size = 64)] private struct NoMode { [FieldOffset(0)] internal uint Type; [FieldOffset(4)] internal uint Id; [FieldOffset(8)] internal NoLuid Adapter; [FieldOffset(16)] internal uint Width; [FieldOffset(20)] internal uint Height; [FieldOffset(28)] internal int X; [FieldOffset(32)] internal int Y; }
    [StructLayout(LayoutKind.Sequential)] private struct NoHeader { internal uint Type, Size; internal NoLuid Adapter; internal uint Id; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct NoSourceName { internal NoHeader Header; internal fixed char Name[32]; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct NoMonitor { internal uint Size; internal NoRect Monitor, Work; internal uint Flags; internal fixed char Device[32]; }
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate bool NoEnumCallback(nint hwnd, nint parameter);
    [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId")] private static extern uint NoCurrentThread();
    [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess")] private static extern nint NoCurrentProcess();
    [DllImport("kernel32.dll", EntryPoint = "ProcessIdToSessionId", SetLastError = true)] private static extern bool NoSession(uint pid, out uint session);
    [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true)] private static extern bool NoCloseHandle(nint handle);
    [DllImport("advapi32.dll", EntryPoint = "OpenProcessToken", SetLastError = true)] private static extern bool NoOpenProcessToken(nint process, uint access, out nint token);
    [DllImport("advapi32.dll", EntryPoint = "GetTokenInformation", SetLastError = true)] private static extern bool NoTokenInfo(nint token, int kind, void* buffer, uint length, out uint needed);
    [DllImport("advapi32.dll", EntryPoint = "IsValidSid")] private static extern bool NoIsValidSid(nint sid);
    [DllImport("advapi32.dll", EntryPoint = "GetSidSubAuthorityCount")] private static extern byte* NoSidCount(nint sid);
    [DllImport("advapi32.dll", EntryPoint = "GetSidSubAuthority")] private static extern uint* NoSidAuthority(nint sid, uint index);
    [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true)] private static extern uint NoWindowThread(nint hwnd, out uint pid);
    [DllImport("user32.dll", EntryPoint = "GetThreadDesktop", SetLastError = true)] private static extern nint NoDesktop(uint tid);
    [DllImport("user32.dll", EntryPoint = "GetProcessWindowStation")] private static extern nint NoStation();
    [DllImport("user32.dll", EntryPoint = "GetUserObjectInformationW", SetLastError = true)] private static extern bool NoUserInfo(nint handle, int index, void* info, uint length, out uint needed);
    [DllImport("user32.dll", EntryPoint = "OpenInputDesktop", SetLastError = true)] private static extern nint NoInputDesktop(uint flags, bool inherit, uint access);
    [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", SetLastError = true)] private static extern bool NoEnumDesktop(nint desktop, NoEnumCallback callback, nint parameter);
    [DllImport("user32.dll", EntryPoint = "CloseDesktop", SetLastError = true)] private static extern bool NoCloseDesktop(nint desktop);
    [DllImport("user32.dll", EntryPoint = "ClientToScreen", SetLastError = true)] private static extern bool NoClientToScreen(nint hwnd, ref NoPoint point);
    [DllImport("user32.dll", EntryPoint = "GetThreadDpiAwarenessContext")] private static extern nint NoThreadDpi();
    [DllImport("user32.dll", EntryPoint = "GetWindowDpiAwarenessContext")] private static extern nint NoWindowDpi(nint hwnd);
    [DllImport("user32.dll", EntryPoint = "GetAwarenessFromDpiAwarenessContext")] private static extern int NoDpiAwareness(nint context);
    [DllImport("user32.dll", EntryPoint = "GetDpiForWindow")] private static extern uint NoDpiForWindow(nint hwnd);
    [DllImport("shcore.dll", EntryPoint = "GetProcessDpiAwareness")] private static extern int NoProcessDpi(nint process, out int awareness);
    [DllImport("dwmapi.dll", EntryPoint = "DwmIsCompositionEnabled")] private static extern int NoComposition(out bool enabled);
    [DllImport("dwmapi.dll", EntryPoint = "DwmGetWindowAttribute")] private static extern int NoCloak(nint hwnd, uint attribute, out uint value, uint size);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)] private static extern int NoWindowLong(nint hwnd, int index);
    [DllImport("user32.dll", EntryPoint = "MonitorFromWindow")] private static extern nint NoMonitorWindow(nint hwnd, uint flags);
    [DllImport("user32.dll", EntryPoint = "MonitorFromPoint")] private static extern nint NoMonitorPoint(NoPoint point, uint flags);
    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", SetLastError = true)] private static extern bool NoMonitorInfo(nint monitor, NoMonitor* info);
    [DllImport("user32.dll", EntryPoint = "GetDisplayConfigBufferSizes")] private static extern int NoDisplaySizes(uint flags, out uint paths, out uint modes);
    [DllImport("user32.dll", EntryPoint = "QueryDisplayConfig")] private static extern int NoDisplayQuery(uint flags, ref uint paths, NoPath* path, ref uint modes, NoMode* mode, nint topology);
    [DllImport("user32.dll", EntryPoint = "DisplayConfigGetDeviceInfo")] private static extern int NoDisplayName(NoHeader* header);
    [DllImport("gdi32.dll", EntryPoint = "GetObjectType")] private static extern uint NoObjectType(nint dc);
    [DllImport("gdi32.dll", EntryPoint = "GetMapMode")] private static extern int NoMapMode(nint dc);
    [DllImport("gdi32.dll", EntryPoint = "GetWindowOrgEx", SetLastError = true)] private static extern bool NoWindowOrg(nint dc, out NoPoint point);
    [DllImport("gdi32.dll", EntryPoint = "GetViewportOrgEx", SetLastError = true)] private static extern bool NoViewportOrg(nint dc, out NoPoint point);
    [DllImport("gdi32.dll", EntryPoint = "GetWindowExtEx", SetLastError = true)] private static extern bool NoWindowExt(nint dc, out NoPoint point);
    [DllImport("gdi32.dll", EntryPoint = "GetViewportExtEx", SetLastError = true)] private static extern bool NoViewportExt(nint dc, out NoPoint point);
    [DllImport("gdi32.dll", EntryPoint = "LPtoDP", SetLastError = true)] private static extern bool NoLpToDp(nint dc, NoPoint* point, int count);
    [DllImport("gdi32.dll", EntryPoint = "GetClipBox")] private static extern int NoClip(nint dc, out NoRect rect);
    [DllImport("gdi32.dll", EntryPoint = "PtVisible")] private static extern bool NoPointVisible(nint dc, int x, int y);
}
#endif
