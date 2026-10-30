#if SANDBOX_DESKTOP_DIAGNOSTIC
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static class SandboxDesktopController
{
    private const uint DesktopAccess = 0x1ff;
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        internal uint Size;
        internal string? Reserved, Desktop, Title;
        internal uint X, Y, Width, Height, XChars, YChars, Fill, Flags;
        internal ushort Show, ReservedSize;
        internal nint ReservedPointer, Input, Output, Error;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInfo { internal nint Process, Thread; internal uint Pid, Tid; }
    [StructLayout(LayoutKind.Sequential)]
    private struct Accounting
    {
        internal long User, Kernel, PeriodUser, PeriodKernel;
        internal uint Faults, Total, Active, Terminated;
    }
    [DllImport("user32.dll", SetLastError = true)] private static extern nint OpenInputDesktop(uint flags, bool inherit, uint access);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern nint CreateDesktopW(string name, nint device, nint mode, uint flags, uint access, nint security);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SwitchDesktop(nint desktop);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CloseDesktop(nint desktop);
    [DllImport("user32.dll")] private static extern nint GetProcessWindowStation();
    [DllImport("user32.dll")] private static extern nint GetThreadDesktop(uint thread);
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetUserObjectInformationW(nint handle, int index, nint buffer, uint size, out uint needed);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CreateProcessW(string application, [In, Out] char[] command, nint processAttributes, nint threadAttributes, bool inherit, uint flags, nint environment, string directory, ref StartupInfo startup, out ProcessInfo process);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern nint CreateJobObjectW(nint security, string? name);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool AssignProcessToJobObject(nint job, nint process);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool TerminateJobObject(nint job, uint code);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool QueryInformationJobObject(nint job, int information, out Accounting accounting, uint size, out uint returned);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint ResumeThread(nint thread);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(nint handle, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetExitCodeProcess(nint process, out uint code);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool TerminateProcess(nint process, uint code);
    [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CloseHandle(nint handle);
    [DllImport("advapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool OpenProcessToken(nint process, uint access, out nint token);
    [DllImport("advapi32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetTokenInformation(nint token, int information, out uint value, uint size, out uint returned);

    private static string? log;
    private static readonly object Gate = new();
    private static void Write(string phase, object value)
    {
        if (log is null) return;
        lock (Gate)
        {
            // Separate per-process journals avoid cross-process truncation and protocol stdout pollution.
            using var stream = new FileStream(log + "." + Environment.ProcessId + ".jsonl", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            JsonSerializer.Serialize(stream, new { Phase = phase, Pid = Environment.ProcessId, Tid = GetCurrentThreadId(), Session = Process.GetCurrentProcess().SessionId, Qpc = Stopwatch.GetTimestamp(), Utc = DateTimeOffset.UtcNow, Value = value });
            stream.WriteByte(10);
            stream.Flush(true);
        }
    }
    private static bool Api(string name, bool result)
    {
        int error = result ? 0 : Marshal.GetLastWin32Error();
        Write(name, new { Result = result, Error = error });
        return result;
    }
    private static string Name(nint handle)
    {
        nint buffer = Marshal.AllocHGlobal(2048);
        try
        {
            if (!Api("GetUserObjectInformationW.name", GetUserObjectInformationW(handle, 2, buffer, 2048, out uint needed)))
                return "Unknown";
            return Marshal.PtrToStringUni(buffer) ?? "Unknown";
        }
        finally { Marshal.FreeHGlobal(buffer); }
    }
    internal static void Capture(string phase)
    {
        log ??= Environment.GetEnvironmentVariable("WPFGFX_OWNED_DESKTOP_LOG");
        if (log is null) return;
        nint input = OpenInputDesktop(0, false, 0x101);
        Api("OpenInputDesktop.capture", input != 0);
        try
        {
            Write(phase, new { Station = Name(GetProcessWindowStation()), Desktop = Name(GetThreadDesktop(GetCurrentThreadId())), Input = input == 0 ? "Unknown" : Name(input), Arguments = Environment.GetCommandLineArgs() });
        }
        finally { if (input != 0) Api("CloseDesktop.capture", CloseDesktop(input)); }
    }
    internal static int Run(string[] args)
    {
        if (args.Length != 2 || args[1] is not ("loss" or "official" or "full")) throw new ArgumentException(new System.Resources.ResourceManager("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(NativeRun).Assembly).GetString("ExitArguments"), nameof(args));
        string root = Path.GetFullPath(args[0]);
        Directory.CreateDirectory(root);
        log = Path.Combine(root, "desktop");
        string? previous = Environment.GetEnvironmentVariable("WPFGFX_OWNED_DESKTOP_LOG");
        Environment.SetEnvironmentVariable("WPFGFX_OWNED_DESKTOP_LOG", log);
        nint original = 0, owned = 0, job = 0;
        ProcessInfo child = default;
        bool assigned = false, restored = false, reaped = true, cleanup = true;
        int result = 125;
        string originalName = "Unknown";
        try
        {
            Capture("parent-before");
            nint station = GetProcessWindowStation();
            string stationName = Name(station);
            nint flags = Marshal.AllocHGlobal(12);
            try
            {
                if (!Api("GetUserObjectInformationW.flags", GetUserObjectInformationW(station, 1, flags, 12, out _))) return 125;
                uint value = unchecked((uint)Marshal.ReadInt32(flags, 8));
                Write("station", new { Name = stationName, Flags = value, Visible = (value & 1) != 0 });
                if ((value & 1) == 0) return 125;
            }
            finally { Marshal.FreeHGlobal(flags); }
            using (var self = Process.GetCurrentProcess())
            {
                if (!Api("OpenProcessToken", OpenProcessToken(self.Handle, 8, out nint token))) return 125;
                try
                {
                    if (!Api("GetTokenInformation.TokenSessionId", GetTokenInformation(token, 12, out uint session, 4, out _))) return 125;
                    Write("token", new { Session = session, ProcessSession = self.SessionId, RequestedDesktopAccess = DesktopAccess });
                    if (session != self.SessionId) return 125;
                }
                finally { Api("CloseHandle.token", CloseHandle(token)); }
            }
            original = OpenInputDesktop(0, false, DesktopAccess);
            if (!Api("OpenInputDesktop.original", original != 0)) return 125;
            originalName = Name(original);
            string desktopName = "WpfGfxOwned_" + Environment.ProcessId + "_" + Guid.NewGuid().ToString("N");
            owned = CreateDesktopW(desktopName, 0, 0, 0, DesktopAccess, 0);
            if (!Api("CreateDesktopW", owned != 0)) return 125;
            Write("created", new { Original = originalName, Owned = Name(owned), Station = stationName });
            job = CreateJobObjectW(0, null);
            if (!Api("CreateJobObjectW", job != 0)) return 125;
            string executable = Environment.ProcessPath ?? throw new InvalidOperationException();
            string fqn = "FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests." + (args[1] == "loss" ? "WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover" : "WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive");
            if (executable.Contains('"') || root.Contains('"')) throw new ArgumentException(new System.Resources.ResourceManager("WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(NativeRun).Assembly).GetString("ExitArguments"), nameof(args));
            bool full = args[1] == "full";
            var command = new StringBuilder(full
                ? $"\"{executable}\" --supervise-exit --full \"{root}\" 1500"
                : $"\"{executable}\" --supervise-exit \"{root}\" 120 \"{fqn}\"");
            var startup = new StartupInfo { Size = (uint)Marshal.SizeOf<StartupInfo>(), Desktop = stationName + "\\" + desktopName };
            Write("launch", new { Executable = executable, Command = command.ToString(), Desktop = startup.Desktop, Flags = 4, Environment = "null: inherited complete process environment", Parent = Environment.ProcessId });
            if (!Api("CreateProcessW", CreateProcessW(executable, (command.ToString() + '\0').ToCharArray(), 0, 0, false, 4, 0, AppContext.BaseDirectory, ref startup, out child))) return 125;
            reaped = false;
            Write("child", new { child.Pid, child.Tid, Parent = Environment.ProcessId });
            assigned = Api("AssignProcessToJobObject", AssignProcessToJobObject(job, child.Process));
            if (!assigned) return 125;
            if (!Api("SwitchDesktop.owned", SwitchDesktop(owned))) return 125;
            Capture("parent-after-switch");
            uint resumed = ResumeThread(child.Thread);
            if (!Api("ResumeThread", resumed != uint.MaxValue)) return 125;
            uint wait = WaitForSingleObject(child.Process, full ? 1650000u : 145000u);
            Write("WaitForSingleObject.child", new { Status = wait, Error = wait == uint.MaxValue ? Marshal.GetLastWin32Error() : 0 });
            if (wait == 0 && Api("GetExitCodeProcess", GetExitCodeProcess(child.Process, out uint exit))) result = unchecked((int)exit);
            else result = 124;
        }
        catch (Exception error) when (error is Win32Exception or InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Write("controller-error", new { Type = error.GetType().FullName, error.Message });
            result = 125;
        }
        finally
        {
            if (child.Process != 0)
            {
                bool done = WaitForSingleObject(child.Process, 0) == 0;
                if (assigned)
                {
                    bool queried = Api("QueryInformationJobObject.before-cleanup", QueryInformationJobObject(job, 1, out Accounting before, (uint)Marshal.SizeOf<Accounting>(), out _));
                    Write("job-before-cleanup", before);
                    if (!queried || before.Active != 0) cleanup &= Api("TerminateJobObject.owned-only", TerminateJobObject(job, 124));
                }
                else if (!done) cleanup &= Api("TerminateProcess.unassigned-owned-child", TerminateProcess(child.Process, 124));
                reaped = WaitForSingleObject(child.Process, 10000) == 0;
                if (assigned)
                {
                    long deadline = Stopwatch.GetTimestamp() + 10 * Stopwatch.Frequency;
                    do
                    {
                        bool queried = Api("QueryInformationJobObject.reap", QueryInformationJobObject(job, 1, out Accounting final, (uint)Marshal.SizeOf<Accounting>(), out _));
                        Write("job-reap", new { final.Total, final.Active, final.Terminated });
                        if (!queried) { reaped = false; break; }
                        if (final.Active == 0) break;
                        reaped = false;
                        Thread.Sleep(100);
                    } while (Stopwatch.GetTimestamp() < deadline);
                    reaped = Api("QueryInformationJobObject.final", QueryInformationJobObject(job, 1, out Accounting last, (uint)Marshal.SizeOf<Accounting>(), out _)) && last.Active == 0 && WaitForSingleObject(child.Process, 0) == 0;
                }
            }
            Capture("parent-before-restore");
            if (original != 0)
            {
                bool switched = Api("SwitchDesktop.original", SwitchDesktop(original));
                nint current = OpenInputDesktop(0, false, 0x101);
                bool opened = Api("OpenInputDesktop.restored", current != 0);
                string actual = opened ? Name(current) : "Unknown";
                restored = switched && opened && actual == originalName;
                Write("restored", new { Expected = originalName, Actual = actual, Confirmed = restored });
                if (current != 0) cleanup &= Api("CloseDesktop.restored-check", CloseDesktop(current));
            }
            else restored = owned == 0;
            if (child.Thread != 0) cleanup &= Api("CloseHandle.child-thread", CloseHandle(child.Thread));
            if (child.Process != 0) cleanup &= Api("CloseHandle.child-process", CloseHandle(child.Process));
            if (job != 0) cleanup &= Api("CloseHandle.job", CloseHandle(job));
            if (owned != 0) cleanup &= Api("CloseDesktop.owned", CloseDesktop(owned));
            if (original != 0) cleanup &= Api("CloseDesktop.original", CloseDesktop(original));
            Capture("parent-after-restore");
            Write("final", new { ChildResult = result, Restored = restored, Reaped = reaped, HandlesClosed = cleanup, Result = restored && reaped && cleanup ? result : 126 });
            Environment.SetEnvironmentVariable("WPFGFX_OWNED_DESKTOP_LOG", previous);
        }
        return restored && reaped && cleanup ? result : 126;
    }
}
#endif
