#if OFFICIAL_WINDOW_HANG_DIAGNOSTIC
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

internal static partial class SamplerNativeCrash
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DebugBreakProcess(nint process);

    private static unsafe void CaptureOfficialWindowThreads(string root, uint pid, nint process)
    {
        // DEBUG_EVENT suspends every debuggee thread until ContinueDebugEvent.
        using Process owned = Process.GetProcessById(checked((int)pid));
        foreach (ProcessThread item in owned.Threads)
        {
            uint tid = checked((uint)item.Id);
            nint thread = OpenThread(0x0048, false, tid);
            int openError = thread == 0 ? Marshal.GetLastWin32Error() : 0;
            byte* context = (byte*)NativeMemory.AlignedAlloc(1232, 16);
            try
            {
                new Span<byte>(context, 1232).Clear();
                *(uint*)(context + 48) = 0x0010001F;
                bool captured = thread != 0 && GetThreadContext(thread, context);
                int error = captured ? 0 : thread == 0 ? openError : Marshal.GetLastWin32Error();
                Write(root, $"hang-thread-{tid}.json", new
                {
                    pid, tid, captured, error,
                    rip = $"0x{*(ulong*)(context + 248):X}", rsp = $"0x{*(ulong*)(context + 152):X}",
                    rawContext = captured ? Convert.ToBase64String(new ReadOnlySpan<byte>(context, 1232)) : null,
                    contextFlags = "CONTEXT_ALL_AMD64", capturedAtDebugEvent = true, stackDecoded = false
                });
            }
            finally
            {
                NativeMemory.AlignedFree(context);
                if (thread != 0)
                {
                    bool closed = CloseHandle(thread);
                    int closeError = closed ? 0 : Marshal.GetLastWin32Error();
                    Write(root, $"hang-thread-{tid}-cleanup.json", new { pid, tid, closed, closeError });
                }
            }
        }
        using var dump = File.Create(Path.Combine(root, "official-window-hang.dmp"));
        bool written = MiniDumpWriteDump(process, pid, dump.SafeFileHandle.DangerousGetHandle(), 0x00001000, 0, 0, 0);
        int dumpError = written ? 0 : Marshal.GetLastWin32Error();
        dump.Flush(true);
        dump.Position = 0;
        Write(root, "official-window-hang-dump.json", new
        {
            pid, written, dumpError, bytes = dump.Length,
            sha256 = written ? Convert.ToHexString(SHA256.HashData(dump)) : null,
            dumpExceptionStream = false, stackDecoded = false
        });
    }
}
#endif
