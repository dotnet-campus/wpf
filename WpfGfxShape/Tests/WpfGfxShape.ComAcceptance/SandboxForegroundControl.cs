#if SANDBOX_FOREGROUND_CONTROL_DIAGNOSTIC
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static unsafe class SandboxForegroundControl
{
    private static readonly JsonSerializerOptions Json = new() { IncludeFields = true };
    private static string Journal(string root) => Path.Combine(root, "foreground-scope.jsonl");
    private static void Log(string root, object value)
    {
        using var stream = new FileStream(Journal(root), FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream, leaveOpen: true);
        writer.WriteLine(JsonSerializer.Serialize(value, Json));
        writer.Flush();
        stream.Flush(true);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct Placement
    {
        public uint Length, Flags, Show;
        public int MinX, MinY, MaxX, MaxY, Left, Top, Right, Bottom;
    }
    internal sealed record Target(long Hwnd, uint Pid, uint Tid, int Session, long StartTicks, string Name, string Class, Placement Placement);
    internal sealed record Entry(string Phase, Target Target);

    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint pid);
    [DllImport("user32.dll", EntryPoint = "GetClassNameW", CharSet = CharSet.Unicode)] private static extern int GetClassName(nint window, char* name, int length);
    [DllImport("user32.dll")] private static extern nint GetAncestor(nint window, uint flags);
    [DllImport("user32.dll", SetLastError = true)] private static extern int SetWindowPlacement(nint window, in Placement placement);
    [DllImport("user32.dll")] private static extern int ShowWindowAsync(nint window, int command);
    [DllImport("user32.dll")] private static extern int IsIconic(nint window);

    private static Target? Identify(nint window)
    {
        uint tid = GetWindowThreadProcessId(window, out uint pid);
        if (tid == 0 || GetAncestor(window, 2) != window) return null;
        using var process = Process.GetProcessById(checked((int)pid));
        using var caller = Process.GetCurrentProcess();
        string name = process.ProcessName + ".exe";
        if (process.SessionId != caller.SessionId || name != "PrismCodingAgent.WpfShell.Tests.exe") return null;
        char* buffer = stackalloc char[256];
        int count = GetClassName(window, buffer, 256);
        string @class = new(buffer, 0, count);
        const string prefix = "HwndWrapper[PrismCodingAgent.WpfShell.Tests;WPF test UI;";
        if (!@class.StartsWith(prefix, StringComparison.Ordinal) || !@class.EndsWith(']') ||
            !Guid.TryParseExact(@class[prefix.Length..^1], "D", out _)) return null;
        Placement placement = new() { Length = (uint)sizeof(Placement) };
        // GetWindowPlacement requires the caller-supplied length even for an output buffer.
        if (ReadPlacement(window, ref placement) == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        return new Target((long)window, pid, tid, process.SessionId, process.StartTime.ToUniversalTime().Ticks, name, @class, placement);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowPlacement", SetLastError = true)]
    private static extern int ReadPlacement(nint window, ref Placement placement);

    private static bool Same(Target saved, Target? current) => current is not null &&
        saved.Hwnd == current.Hwnd && saved.Pid == current.Pid && saved.Tid == current.Tid &&
        saved.Session == current.Session && saved.StartTicks == current.StartTicks && saved.Name == current.Name && saved.Class == current.Class;

    internal static void Yield(string root, string phase)
    {
        nint foreground = GetForegroundWindow();
        try
        {
            Target? target = Identify(foreground);
            if (target is null) { Log(root, new { Phase = phase, Result = "NoMatchingForeground", Foreground = (long)foreground }); return; }
            string[] prior = File.Exists(Path.Combine(root, "foreground-targets.jsonl")) ? File.ReadAllLines(Path.Combine(root, "foreground-targets.jsonl")) : [];
            if (prior.Select(line => JsonSerializer.Deserialize<Entry>(line, Json)!).Any(entry => entry.Target.Hwnd == target.Hwnd))
            { Log(root, new { Phase = phase, Result = "AlreadyHandled", Target = target }); return; }
            if (prior.Length >= 2) { Log(root, new { Phase = phase, Result = "BudgetExhausted" }); return; }
            if (!Same(target, Identify(foreground))) { Log(root, new { Phase = phase, Result = "UnknownIdentityChanged", Target = target }); return; }
            // Persist rollback before issuing the asynchronous cross-process request.
            using (var stream = new FileStream(Path.Combine(root, "foreground-targets.jsonl"), FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                using var writer = new StreamWriter(stream, leaveOpen: true);
                writer.WriteLine(JsonSerializer.Serialize(new Entry(phase, target), Json));
                writer.Flush(); stream.Flush(true);
            }
            int before = IsIconic(foreground);
            int request = ShowWindowAsync(foreground, 6);
            Log(root, new { Phase = phase, Result = "MinimizeRequested", Target = target, Request = request, BeforeIconic = before, Foreground = (long)GetForegroundWindow() });
            var watch = Stopwatch.StartNew();
            while (watch.Elapsed < TimeSpan.FromSeconds(2) && IsIconic(foreground) == 0 && Same(target, Identify(foreground))) Thread.Sleep(20);
            Log(root, new { Phase = phase, Result = IsIconic(foreground) != 0 ? "MinimizedObserved" : "UnknownNotObserved", Request = request, Iconic = IsIconic(foreground), Foreground = (long)GetForegroundWindow(), ElapsedMs = watch.ElapsedMilliseconds, IdentityMatches = Same(target, Identify(foreground)) });
        }
        catch (Exception exception) when (exception is Win32Exception or ArgumentException or InvalidOperationException or System.Security.SecurityException)
        { Log(root, new { Phase = phase, Result = "Unknown", Error = exception.ToString(), Foreground = (long)foreground }); }
    }

    internal static void BeforeInitial()
    {
        string? root = Environment.GetEnvironmentVariable("WPFGFX_RUN_ROOT");
        if (!string.IsNullOrWhiteSpace(root)) Yield(root, "initial-once");
    }

    internal static void Restore(string root)
    {
        string path = Path.Combine(root, "foreground-targets.jsonl");
        bool failed = false;
        foreach (string line in File.Exists(path) ? File.ReadAllLines(path).Reverse() : [])
        {
            Target target = JsonSerializer.Deserialize<Entry>(line, Json)!.Target;
            try
            {
                nint window = (nint)target.Hwnd;
                if (!Same(target, Identify(window)))
                { failed = true; Log(root, new { Phase = "restore", Result = "CleanupFailedIdentityOrExited", Target = target }); continue; }
                int request = SetWindowPlacement(window, target.Placement);
                int error = request == 0 ? Marshal.GetLastWin32Error() : 0;
                var watch = Stopwatch.StartNew();
                Target? actual = Identify(window);
                while (watch.Elapsed < TimeSpan.FromSeconds(2) && Same(target, actual) && !EqualPlacement(target.Placement, actual!.Placement))
                { Thread.Sleep(20); actual = Identify(window); }
                bool restored = request != 0 && Same(target, actual) && EqualPlacement(target.Placement, actual!.Placement);
                failed |= !restored;
                Log(root, new { Phase = "restore", Result = restored ? "Restored" : "CleanupFailed", Target = target, Request = request, Error = error, Actual = actual, Iconic = IsIconic(window), Foreground = (long)GetForegroundWindow() });
            }
            catch (Exception exception) when (exception is Win32Exception or ArgumentException or InvalidOperationException or System.Security.SecurityException)
            { failed = true; Log(root, new { Phase = "restore", Result = "CleanupFailedUnknown", Target = target, Error = exception.ToString() }); }
        }
        Log(root, new { Phase = "scope-exit", Result = failed ? "CleanupFailed" : "CleanupComplete" });
        if (failed) throw new InvalidOperationException(Journal(root));
    }

    private static bool EqualPlacement(Placement left, Placement right) => left.Show == right.Show &&
        left.MinX == right.MinX && left.MinY == right.MinY && left.MaxX == right.MaxX && left.MaxY == right.MaxY &&
        left.Left == right.Left && left.Top == right.Top && left.Right == right.Right && left.Bottom == right.Bottom;
}
#endif
