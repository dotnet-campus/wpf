using Microsoft.Testing.Platform.Builder;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal static class SamplerNativeCrash
{
        private const string Method = nameof(D3DImageTests.WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain);
    private const uint Continue = 0x00010002, NotHandled = 0x80010001;

    internal static async Task<int> RunChildAsync(string root)
    {
        Directory.CreateDirectory(root);
        using var stdout = new StreamWriter(Path.Combine(root, "child-stdout.txt")) { AutoFlush = true };
        using var stderr = new StreamWriter(Path.Combine(root, "child-stderr.txt")) { AutoFlush = true };
        Console.SetOut(stdout);
        Console.SetError(stderr);
        Environment.SetEnvironmentVariable("WPFGFX_RUN_ROOT", root);
        Environment.SetEnvironmentVariable("WPFGFX_EXIT_STAGES", Path.Combine(root, "stages.jsonl"));
        Environment.SetEnvironmentVariable("JH_HISTORICAL_ROLE", Method);
        Environment.SetEnvironmentVariable("WPFGFX_SAMPLER_CRASH_DIAGNOSTIC", "1");
        string[] args = ["--filter", "FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests." + Method,
            "--report-trx", "--report-trx-filename", "child.trx", "--results-directory", Path.Combine(root, "TestResults"), "--no-progress"];
        ITestApplicationBuilder builder = await TestApplication.CreateBuilderAsync(args);
        builder.AddSelfRegisteredExtensions(args);
        using ITestApplication app = await builder.BuildAsync();
        return await app.RunAsync();
    }

    internal static unsafe int Run(string directory)
    {
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException();
        string root = Path.GetFullPath(directory);
        Directory.CreateDirectory(root);
        string exe = Environment.ProcessPath ?? throw new InvalidOperationException();
        if (!Path.GetFileName(exe).Equals("WpfGfxShape.ComAcceptance.exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(nameof(exe));
        string candidate = AcceptancePaths.AotDll;
        string candidateHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(candidate)));
        if (candidateHash != "632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671")
            throw new InvalidOperationException(candidateHash);
        Write(root, "identity.json", new { candidate, candidateHash, official = AcceptancePaths.OriginalDll,
            officialHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.OriginalDll))),
            testHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(NativeRun).Assembly.Location))),
            mvid = typeof(NativeRun).Module.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size,
            method = "WpfGfxShape.ComAcceptance.D3DImageTests." + Method, timeoutSeconds = 120,
            dumpExceptionStream = false, stackDecoded = false });
        using var events = new FileStream(Path.Combine(root, "events.jsonl"), FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        var evidenceErrors = new List<string>();
        void Record(object value)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value) + "\n");
                events.Write(bytes);
                events.Flush(true);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                evidenceErrors.Add(error.ToString());
                Console.Error.WriteLine(error);
            }
        }
        var startup = new Startup { Size = (uint)sizeof(Startup) };
        string command = $"\"{exe}\" --sampler-native-child \"{root}\"";
        char[] commandBuffer = (command + '\0').ToCharArray();
        bool created;
        ProcessInfo process;
        fixed (char* buffer = commandBuffer)
            created = CreateProcessW(exe, buffer, 0, 0, false, 1, 0, AppContext.BaseDirectory, ref startup, out process);
        int createError = created ? 0 : Marshal.GetLastWin32Error();
        Record(new { api = "CreateProcessW", created, createError, process.Pid, process.Tid, flags = "DEBUG_PROCESS", command, startupSize = sizeof(Startup), eventSize = sizeof(DebugEvent) });
        if (!created) throw new Win32Exception(createError);
        var watch = Stopwatch.StartNew();
        bool exited = false, secondChance = false, timedOut = false;
        uint? exitCode = null;
        var breakpoints = new HashSet<uint>();
        var processes = new Dictionary<uint, nint>();
        var modules = new Dictionary<(uint Pid, ulong Address), (string? Path, int? Size, string? Hash)>();
        try
        {
            while (!exited || processes.Count != 0)
            {
                if (watch.Elapsed.TotalSeconds > (timedOut ? 135 : 120))
                {
                    if (timedOut) throw new TimeoutException();
                    timedOut = true;
                    foreach (var target in processes)
                    {
                        bool terminated = TerminateProcess(target.Value, 1460);
                        int error = terminated ? 0 : Marshal.GetLastWin32Error();
                        Record(new { api = "TerminateProcess", pid = target.Key, terminated, error });
                    }
                    if (processes.Count == 0)
                    {
                        bool terminated = TerminateProcess(process.Process, 1460);
                        Record(new { api = "TerminateProcess", pid = process.Pid, terminated, error = terminated ? 0 : Marshal.GetLastWin32Error() });
                    }
                }
                bool received = WaitForDebugEvent(out DebugEvent item, 250);
                if (!received)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == 121) continue;
                    throw new Win32Exception(error);
                }
                uint status = item.Code == 1 ? NotHandled : Continue;
                int continueError = 0;
                try
                {
                    Record(new { api = "WaitForDebugEvent", item.Code, item.Pid, item.Tid });
                    if (item.Code == 3)
                    {
                        nint handle = OpenProcess(0x00100451, false, item.Pid);
                        int error = handle == 0 ? Marshal.GetLastWin32Error() : 0;
                        Record(new { api = "OpenProcess", item.Pid, success = handle != 0, error });
                        if (handle == 0) throw new Win32Exception(error);
                        processes.Add(item.Pid, handle);
                    }
                    if (item.Code is 3 or 6)
                    {
                        nint file = item.File;
                        ulong address = (ulong)(item.Code == 3 ? item.ImageBase : item.DllBase);
                        try
                        {
                            char[] name = new char[32768];
                            uint length;
                            fixed (char* buffer = name)
                                length = file == 0 ? 0 : GetFinalPathNameByHandleW(file, buffer, (uint)name.Length, 0);
                            int error = length == 0 && file != 0 ? Marshal.GetLastWin32Error() : 0;
                            string? path = length > 0 && length < name.Length ? new string(name, 0, (int)length) : null;
                            int? size = null;
                            string? hash = null;
                            if (path is not null)
                            {
                                try
                                {
                                    using var stream = File.OpenRead(path);
                                    hash = Convert.ToHexString(SHA256.HashData(stream));
                                    stream.Position = 0;
                                    using var pe = new PEReader(stream);
                                    size = pe.PEHeaders.PEHeader?.SizeOfImage;
                                }
                                catch (Exception failure) when (failure is IOException or UnauthorizedAccessException or BadImageFormatException)
                                {
                                    Record(new { moduleEvidenceError = failure.ToString(), item.Pid, path, address });
                                }
                            }
                            var module = new { address = $"0x{address:X}", path, size, hash, error };
                            modules[(item.Pid, address)] = (path, size, hash);
                            Record(new { item.Pid, moduleLoad = module });
                        }
                        finally
                        {
                            if (file != 0)
                            {
                                bool closed = CloseHandle(file);
                                Record(new { moduleFileClosed = closed, error = closed ? 0 : Marshal.GetLastWin32Error() });
                            }
                        }
                    }
                    if (item.Code == 7) modules.Remove((item.Pid, (ulong)item.File));
                    if (item.Code == 1)
                    {
                        status = NotHandled;
                        if (item.ExceptionCode == 0x80000003 && item.FirstChance != 0 && breakpoints.Add(item.Pid))
                        {
                            status = Continue;
                        }
                        Record(new { exceptionCode = $"0x{item.ExceptionCode:X8}", address = $"0x{item.ExceptionAddress:X}", item.FirstChance, item.Pid, item.Tid, status });
                        if (item.FirstChance == 0)
                        {
                            secondChance = true;
                            string prefix = $"second-chance-{item.Pid}-{item.Tid}";
                            try
                            {
                                var loaded = modules.Where(pair => pair.Key.Pid == item.Pid).Select(pair => new
                                {
                                    address = $"0x{pair.Key.Address:X}", pair.Value.Path, pair.Value.Size, pair.Value.Hash,
                                    faultOffset = item.ExceptionAddress >= pair.Key.Address && item.ExceptionAddress - pair.Key.Address < (ulong)pair.Value.Size.GetValueOrDefault()
                                        ? $"0x{item.ExceptionAddress - pair.Key.Address:X}" : null
                                }).ToArray();
                                nint thread = OpenThread(0x0048, false, item.Tid);
                                int openError = thread == 0 ? Marshal.GetLastWin32Error() : 0;
                                try
                                {
                                    byte* context = (byte*)NativeMemory.AlignedAlloc(1232, 16);
                                    try
                                    {
                                        new Span<byte>(context, 1232).Clear();
                                        *(uint*)(context + 48) = 0x0010001F;
                                        bool captured = thread != 0 && GetThreadContext(thread, context);
                                        int contextError = captured ? 0 : thread == 0 ? openError : Marshal.GetLastWin32Error();
                                        Write(root, prefix + "-context.json", new { captured, contextError, item.Pid, item.Tid,
                                            rip = $"0x{*(ulong*)(context + 248):X}", rsp = $"0x{*(ulong*)(context + 152):X}",
                                            rawContext = captured ? Convert.ToBase64String(new ReadOnlySpan<byte>(context, 1232)) : null });
                                    }
                                    finally { NativeMemory.AlignedFree(context); }
                                }
                                finally
                                {
                                    if (thread != 0)
                                    {
                                        bool closed = CloseHandle(thread);
                                        Record(new { threadContextHandleClosed = closed, error = closed ? 0 : Marshal.GetLastWin32Error() });
                                    }
                                }
                                Write(root, prefix + "-exception.json", new { code = $"0x{item.ExceptionCode:X8}", address = $"0x{item.ExceptionAddress:X}", item.Pid, item.Tid,
                                    firstChance = false, parameterCount = item.ParameterCount, operation = item.Information0,
                                    inaccessibleAddress = $"0x{item.Information1:X}", modules = loaded,
                                    dumpExceptionStream = false, stackDecoded = false });
                                using var dump = File.Create(Path.Combine(root, prefix + ".dmp"));
                                bool written = MiniDumpWriteDump(processes[item.Pid], item.Pid, dump.SafeFileHandle.DangerousGetHandle(), 0, 0, 0, 0);
                                int error = written ? 0 : Marshal.GetLastWin32Error();
                                dump.Flush(true);
                                dump.Position = 0;
                                Write(root, prefix + "-dump.json", new { written, error, bytes = dump.Length, sha256 = written ? Convert.ToHexString(SHA256.HashData(dump)) : null });
                            }
                            catch (Exception error) when (error is IOException or UnauthorizedAccessException or Win32Exception or InvalidOperationException)
                            {
                                Record(new { evidenceError = error.ToString(), item.Pid, item.Tid });
                            }
                        }
                    }
                    if (item.Code == 5)
                    {
                        Record(new { processExit = item.Pid, code = $"0x{item.ExceptionCode:X8}" });
                        if (item.Pid == process.Pid) { exitCode = item.ExceptionCode; exited = true; }
                        if (processes.Remove(item.Pid, out nint handle))
                        {
                            bool closed = CloseHandle(handle);
                            Record(new { processHandleClosed = closed, item.Pid, error = closed ? 0 : Marshal.GetLastWin32Error() });
                        }
                    }
                }
                finally
                {
                    bool continued = ContinueDebugEvent(item.Pid, item.Tid, status);
                    int error = continued ? 0 : Marshal.GetLastWin32Error();
                    Record(new { api = "ContinueDebugEvent", continued, error, status, item.Pid, item.Tid });
                    if (!continued) continueError = error;
                }
                if (continueError != 0) throw new Win32Exception(continueError);
            }
            Write(root, "exit.json", new { exited, exitCode, exitHex = $"0x{exitCode:X8}", secondChance, timedOut, elapsedSeconds = watch.Elapsed.TotalSeconds, evidenceErrors });
            return timedOut ? 1460 : secondChance ? unchecked((int)(exitCode.GetValueOrDefault() == 0 ? 1 : exitCode.Value)) : unchecked((int)exitCode.GetValueOrDefault(1));
        }
        finally
        {
            if (!exited)
            {
                bool terminated = TerminateProcess(process.Process, 1);
                Record(new { cleanupTerminate = terminated, error = terminated ? 0 : Marshal.GetLastWin32Error() });
            }
            foreach (var target in processes)
            {
                bool terminated = TerminateProcess(target.Value, 1);
                int error = terminated ? 0 : Marshal.GetLastWin32Error();
                Record(new { cleanupPid = target.Key, terminated, error });
            }
            var cleanup = Stopwatch.StartNew();
            while ((!exited || processes.Count != 0) && cleanup.Elapsed.TotalSeconds < 15)
            {
                bool received = WaitForDebugEvent(out DebugEvent pending, 100);
                int waitError = received ? 0 : Marshal.GetLastWin32Error();
                if (!received)
                {
                    if (waitError == 121) continue;
                    Record(new { cleanupWaitError = waitError });
                    break;
                }
                Record(new { cleanupEvent = pending.Code, pending.Pid, pending.Tid });
                if (pending.Code == 3)
                {
                    nint handle = OpenProcess(0x00100451, false, pending.Pid);
                    int error = handle == 0 ? Marshal.GetLastWin32Error() : 0;
                    Record(new { cleanupOpenPid = pending.Pid, error });
                    if (handle != 0)
                    {
                        processes[pending.Pid] = handle;
                        bool terminated = TerminateProcess(handle, 1);
                        int terminateError = terminated ? 0 : Marshal.GetLastWin32Error();
                        Record(new { cleanupNewPid = pending.Pid, terminated, terminateError });
                    }
                }
                if (pending.Code is 3 or 6 && pending.File != 0)
                {
                    bool closed = CloseHandle(pending.File);
                    int error = closed ? 0 : Marshal.GetLastWin32Error();
                    Record(new { cleanupFileClosed = closed, error });
                }
                bool continued = ContinueDebugEvent(pending.Pid, pending.Tid, pending.Code == 1 ? NotHandled : Continue);
                int continueError = continued ? 0 : Marshal.GetLastWin32Error();
                Record(new { cleanupContinued = continued, continueError, pending.Pid, pending.Tid });
                if (pending.Code == 5)
                {
                    if (pending.Pid == process.Pid) exited = true;
                    if (processes.Remove(pending.Pid, out nint handle))
                    {
                        uint wait = WaitForSingleObject(handle, 0);
                        int error = wait == uint.MaxValue ? Marshal.GetLastWin32Error() : 0;
                        bool closed = CloseHandle(handle);
                        int closeError = closed ? 0 : Marshal.GetLastWin32Error();
                        Record(new { cleanupReaped = pending.Pid, wait, error, closed, closeError });
                    }
                }
                if (!continued) break;
            }
            foreach (var target in processes)
            {
                bool stopped = DebugActiveProcessStop(target.Key);
                int stopError = stopped ? 0 : Marshal.GetLastWin32Error();
                uint wait = WaitForSingleObject(target.Value, 0);
                int waitError = wait == uint.MaxValue ? Marshal.GetLastWin32Error() : 0;
                bool closed = CloseHandle(target.Value);
                int closeError = closed ? 0 : Marshal.GetLastWin32Error();
                Record(new { cleanupPid = target.Key, wait, waitError, stopped, stopError, closed, closeError });
            }
            uint rootWait = WaitForSingleObject(process.Process, 5000);
            int rootWaitError = rootWait == uint.MaxValue ? Marshal.GetLastWin32Error() : 0;
            Record(new { rootReap = rootWait, rootWaitError, rootSignaled = rootWait == 0,
                rootWaitBudgetMilliseconds = 5000, exitDebugEventReceived = exited, cleanupSeconds = cleanup.Elapsed.TotalSeconds });
            bool threadClosed = CloseHandle(process.Thread);
            int threadError = threadClosed ? 0 : Marshal.GetLastWin32Error();
            bool processClosed = CloseHandle(process.Process);
            Record(new { threadClosed, threadError, processClosed, processError = processClosed ? 0 : Marshal.GetLastWin32Error() });
        }
    }

    private static void Write(string root, string name, object value) => File.WriteAllText(Path.Combine(root, name), JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true }));

    // x64 only: union starts at 16; EXCEPTION_RECORD is 152 bytes, DEBUG_EVENT is 176.
    [StructLayout(LayoutKind.Explicit, Size = 176)]
    private struct DebugEvent
    {
        [FieldOffset(0)] internal uint Code;
        [FieldOffset(4)] internal uint Pid;
        [FieldOffset(8)] internal uint Tid;
        [FieldOffset(16)] internal nint File;
        [FieldOffset(24)] internal nint DllBase;
        [FieldOffset(40)] internal nint ImageBase;
        [FieldOffset(16)] internal uint ExceptionCode;
        [FieldOffset(32)] internal ulong ExceptionAddress;
        [FieldOffset(40)] internal uint ParameterCount;
        [FieldOffset(48)] internal ulong Information0;
        [FieldOffset(56)] internal ulong Information1;
        [FieldOffset(168)] internal uint FirstChance;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct Startup
    {
        internal uint Size;
        internal nint Reserved, Desktop, Title;
        internal uint X, Y, Width, Height, XChars, YChars, Fill, Flags;
        internal ushort Show, ReservedBytes;
        internal nint ReservedPointer, Input, Output, Error;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInfo
    {
        internal nint Process, Thread;
        internal uint Pid, Tid;
    }
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    private static extern unsafe bool CreateProcessW(string application, char* command, nint processAttributes, nint threadAttributes, bool inherit, uint flags, nint environment, string directory, ref Startup startup, out ProcessInfo process);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern nint OpenProcess(uint access, bool inherit, uint pid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern nint OpenThread(uint access, bool inherit, uint tid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern unsafe bool GetThreadContext(nint thread, void* context);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(nint handle, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool DebugActiveProcessStop(uint pid);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool WaitForDebugEvent(out DebugEvent item, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool ContinueDebugEvent(uint pid, uint tid, uint status);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool TerminateProcess(nint process, uint code);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool CloseHandle(nint handle);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)] private static extern unsafe uint GetFinalPathNameByHandleW(nint file, char* name, uint length, uint flags);
    [DllImport("dbghelp.dll", SetLastError = true)] private static extern bool MiniDumpWriteDump(nint process, uint pid, nint file, uint type, nint exception, nint streams, nint callback);
}
