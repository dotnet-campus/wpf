using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static unsafe class ForegroundTopology
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ProcessEntry
    {
        internal uint Size, Usage, Pid;
        internal nuint Heap;
        internal uint Module, Threads, Parent;
        internal int Priority;
        internal uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        internal string Name;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint CreateToolhelp32Snapshot(uint flags, uint pid);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Process32FirstW(nint snapshot, ref ProcessEntry entry);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool Process32NextW(nint snapshot, ref ProcessEntry entry);
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);
    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint window, out uint pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetClassNameW(nint window, char* name, int capacity);

    internal static void Capture(string phase)
    {
        nint window = GetForegroundWindow();
        uint tid = GetWindowThreadProcessId(window, out uint owner);
        char* buffer = stackalloc char[256];
        int classLength = GetClassNameW(window, buffer, 256);
        int classError = classLength == 0 ? Marshal.GetLastWin32Error() : 0;
        string name = new(buffer, 0, classLength);
        var entries = new Dictionary<uint, ProcessEntry>();
        int snapshotError = 0;
        nint snapshot = CreateToolhelp32Snapshot(2, 0);
        if (snapshot == -1) snapshotError = Marshal.GetLastWin32Error();
        else
        {
            try
            {
                var entry = new ProcessEntry { Size = (uint)Marshal.SizeOf<ProcessEntry>(), Name = string.Empty };
                bool found = Process32FirstW(snapshot, ref entry);
                if (!found) snapshotError = Marshal.GetLastWin32Error();
                while (found)
                {
                    entries[entry.Pid] = entry;
                    found = Process32NextW(snapshot, ref entry);
                }
                int endError = Marshal.GetLastWin32Error();
                if (endError != 18) snapshotError = endError;
            }
            finally { CloseHandle(snapshot); }
        }
        object[] Chain(uint pid)
        {
            var chain = new List<object>();
            var visited = new HashSet<uint>();
            while (pid != 0 && visited.Add(pid) && chain.Count < 16)
            {
                bool found = entries.TryGetValue(pid, out ProcessEntry entry);
                int? session = null;
                string? path = null, error = null;
                try
                {
                    using var process = Process.GetProcessById(checked((int)pid));
                    session = process.SessionId;
                    path = process.MainModule?.FileName;
                }
                catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or ArgumentException or NotSupportedException)
                { error = exception.GetType().Name + ": " + exception.Message; }
                chain.Add(new { Pid = pid, Parent = found ? entry.Parent : (uint?)null, Name = found ? entry.Name : null, Session = session, Path = path, Error = error });
                if (!found) break;
                pid = entry.Parent;
            }
            return chain.ToArray();
        }
        var modules = new List<object>();
        string? moduleError = null;
        try
        {
            using var process = Process.GetCurrentProcess();
            foreach (ProcessModule module in process.Modules)
                if (module.ModuleName.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase) ||
                    module.ModuleName.Contains("d3d", StringComparison.OrdinalIgnoreCase) ||
                    module.ModuleName.Contains("dxgi", StringComparison.OrdinalIgnoreCase))
                    modules.Add(new { module.ModuleName, module.FileName, Base = $"0x{module.BaseAddress:X}", module.ModuleMemorySize });
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or NotSupportedException)
        { moduleError = exception.GetType().Name + ": " + exception.Message; }
        string line = "ForegroundTopology " + JsonSerializer.Serialize(new
        {
            Phase = phase, Window = $"0x{window:X}", Tid = tid, Owner = owner, Class = name, ClassError = classError,
            Caller = Environment.ProcessId, Supervisor = Environment.GetEnvironmentVariable("WPFGFX_SUPERVISOR_PID"),
            SnapshotError = snapshotError, CallerChain = Chain((uint)Environment.ProcessId), OwnerChain = Chain(owner),
            Modules = modules, ModuleError = moduleError
        });
        ExitDiagnostics.Stage(line);
        Console.WriteLine(line);
    }
}
