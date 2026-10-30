#if RESOURCE_LIFECYCLE_HANDLE_TYPES
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

internal sealed class ProcessHandleTypeSnapshot
{
    internal const int MaxEntries = 16384;
    private double _roleSeconds;
    private bool _stop;

    internal static bool IsPoint(int round, string phase) =>
#if RUNTIME_HEAP_OWNER_DIAGNOSTIC
        (round == 15 && phase is "BeforeHostGc" or "AfterHostGc" or "AfterDllGc") ||
#endif
        (round == -1 && phase == "Initial") ||
        (round == 0 && phase is "BeforeInitialize" or "AfterSourceCloseTargetUpdate" or "AfterCleanup" or "AfterCleanupPlusOneSecond") ||
        (round == 4 && phase == "AfterSourceCloseTargetUpdate") ||
        (round is 3 or 4 or 8 or 15 && phase == "AfterCleanupPlusOneSecond");

    internal void Append(string root, string role, int round, string phase, uint[] callbackThreads)
    {
        if (!IsPoint(round, phase)) return;
        long start = Stopwatch.GetTimestamp();
        Snapshot result = new(role, round, phase) { BudgetSeconds = Math.Max(0, Math.Min(2, 10 - _roleSeconds)) };
        try
        {
            if (_stop || _roleSeconds >= 10) result.Reason = "RoleDiagnosticBudgetOrCleanupFailure";
            else Capture(result, start);
            result.Callbacks = callbackThreads.Select((tid, index) => new CallbackCandidate(index, tid,
                $"round-{index:D2}/callbacks.jsonl", $"round-{index:D2}/business.json")).ToArray();
            foreach (var thread in result.Threads)
                thread.MatchedCallbackRounds = result.Callbacks.Where(c => thread.Tid.Status == "Succeeded" && c.Tid != 0 && c.Tid == thread.Tid.Value)
                    .Select(c => c.Round).ToArray();
        }
        catch (Exception error) when (error is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException
            or InvalidOperationException or ArgumentException or OverflowException or OutOfMemoryException)
        {
            // The diagnostic must not replace an existing business failure or turn missing evidence into zero.
            result.Status = "Unavailable";
            result.Error = error.ToString();
        }
        result.EndQpc = Stopwatch.GetTimestamp();
        result.Seconds = Stopwatch.GetElapsedTime(start).TotalSeconds;
        _roleSeconds += result.Seconds;
        result.RoleSeconds = _roleSeconds;
        _stop |= result.Opened != result.Closed || result.BuffersOpened != result.BuffersClosed;
        try
        {
            File.AppendAllText(Path.Combine(root, "handle-types.jsonl"), JsonSerializer.Serialize(result) + Environment.NewLine);
            var write = new WriteRecord(role, round, phase, Stopwatch.GetElapsedTime(start).TotalSeconds);
            File.AppendAllText(Path.Combine(root, "handle-types-writes.jsonl"), JsonSerializer.Serialize(write) + Environment.NewLine);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        {
            Console.Error.WriteLine(error);
            _stop = true;
        }
        _roleSeconds += Math.Max(0, Stopwatch.GetElapsedTime(start).TotalSeconds - result.Seconds);
    }

    internal static bool TryCount(ReadOnlySpan<byte> data, int pointerSize, out int count)
    {
        count = 0;
        if (pointerSize != 8 || data.Length < 16 || data.Length > 4 * 1024 * 1024) return false;
        ulong value = BinaryPrimitives.ReadUInt64LittleEndian(data);
        if (value > MaxEntries || value > (ulong)((data.Length - 16) / 40)) return false;
        count = checked((int)value);
        return true;
    }

    internal static string? ParseType(ReadOnlySpan<byte> data, ulong bufferAddress)
    {
        if (data.Length < 104 || data.Length > 65536) return null;
        ushort length = BinaryPrimitives.ReadUInt16LittleEndian(data);
        ushort maximum = BinaryPrimitives.ReadUInt16LittleEndian(data[2..]);
        ulong address = BinaryPrimitives.ReadUInt64LittleEndian(data[8..]);
        if (length == 0 || (length & 1) != 0 || (maximum & 1) != 0 || length > maximum || address < bufferAddress) return null;
        ulong offset = address - bufferAddress;
        if (offset < 104 || offset > (ulong)data.Length || maximum > (ulong)data.Length - offset) return null;
        string name = Encoding.Unicode.GetString(data.Slice(checked((int)offset), length));
        return string.IsNullOrWhiteSpace(name) || name.Contains('\0') ? null : name;
    }

    private static bool ResizeStatus(int status) => status is unchecked((int)0xC0000004) or unchecked((int)0xC0000023) or unchecked((int)0x80000005);

    private static void Capture(Snapshot result, long start)
    {
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X64 || IntPtr.Size != 8)
        {
            result.Status = "Unsupported";
            result.Reason = "WindowsX64SchemaOnly";
            return;
        }
        nint module = 0;
        try
        {
            result.BeforeHandles = ReadCount(result, "Before");
            module = NativeLibrary.Load(Path.Combine(Environment.SystemDirectory, "ntdll.dll"));
            if (!NativeLibrary.TryGetExport(module, "NtQueryInformationProcess", out nint processAddress) ||
                !NativeLibrary.TryGetExport(module, "NtQueryObject", out nint objectAddress))
            {
                result.Status = "Unsupported";
                result.Reason = "EntryPointMissing";
                return;
            }
            var processQuery = Marshal.GetDelegateForFunctionPointer<Query>(processAddress);
            var objectQuery = Marshal.GetDelegateForFunctionPointer<Query>(objectAddress);
            byte[]? table = QueryBytes(processQuery, GetCurrentProcess(), 51, 65536, 4 * 1024 * 1024, 5, result, start, out int status);
            if (table is null)
            {
                result.Status = status is unchecked((int)0xC0000003) or unchecked((int)0xC0000002) ? "Unsupported" : "Unavailable";
                return;
            }
            if (!TryCount(table, IntPtr.Size, out int count))
            {
                result.Status = "Unsupported";
                result.Reason = "InvalidTableLayoutOrCount";
                return;
            }
            result.Enumerated = count;
            result.Status = "Partial";
            for (int index = 0; index < count; index++)
            {
                if (Stopwatch.GetElapsedTime(start).TotalSeconds >= result.BudgetSeconds || result.AllocatedNativeBytes >= 16 * 1024 * 1024)
                { result.Reason = "SnapshotTimeOrAllocationBudget"; break; }
                ulong value = BinaryPrimitives.ReadUInt64LittleEndian(table.AsSpan(checked(16 + index * 40), 8));
                if (value == 0 || value >= long.MaxValue) { result.Failures.Add(new(index, "InvalidOriginalValue", null)); continue; }
                nint duplicate = 0;
                bool owned = false;
                ThreadObservation? thread = null;
                try
                {
                    if (!DuplicateHandle(GetCurrentProcess(), (nint)value, GetCurrentProcess(), out duplicate, 0, false, 2))
                    {
                        result.Failures.Add(new(index, "DuplicateHandleRaceOrDenied", Marshal.GetLastPInvokeError()));
                        continue;
                    }
                    owned = true;
                    result.Opened++;
                    string? type = QueryType(objectQuery, duplicate, result, start);
                    if (type is null) result.Failures.Add(new(index, "TypeUnknown", null));
                    else
                    {
                        result.Classified++;
                        result.Types[type] = result.Types.GetValueOrDefault(type) + 1;
                        if (type == "Thread")
                        {
                            thread = new ThreadObservation(index, value);
                            result.Threads.Add(thread);
                            ObserveThread(duplicate, thread, result, start);
                        }
                    }
                }
                finally
                {
                    if (owned)
                    {
                        long closeStart = Stopwatch.GetTimestamp();
                        bool closed = CloseHandle(duplicate);
                        int? closeError = closed ? null : Marshal.GetLastPInvokeError();
                        long closeEnd = Stopwatch.GetTimestamp();
                        if (thread is not null) thread.Close = new(closed ? "Succeeded" : "Failed", closed ? 1u : 0u,
                            closeError, closeStart, closeEnd);
                        if (closed) result.Closed++;
                        else result.Failures.Add(new(index, "CloseOwnDuplicate", closeError));
                    }
                }
                if (result.Opened != result.Closed) { result.Reason = "OwnDuplicateCloseFailure"; break; }
            }
            if (result.Classified == count && result.Opened == result.Closed) result.Status = "Complete";
        }
        finally
        {
            if (module != 0)
            {
                try { NativeLibrary.Free(module); }
                catch (InvalidOperationException error) { result.CleanupErrors.Add(error.ToString()); }
            }
            result.AfterHandles = ReadCount(result, "After");
        }
    }

    private static uint? ReadCount(Snapshot result, string boundary)
    {
        if (GetProcessHandleCount(GetCurrentProcess(), out uint count)) return count;
        result.Failures.Add(new(-1, boundary + "HandleCount", Marshal.GetLastPInvokeError()));
        return null;
    }

    private static byte[]? QueryBytes(Query query, nint handle, int informationClass, int initial, int maximum, int attempts,
        Snapshot result, long start, out int finalStatus)
    {
        finalStatus = 0;
        int capacity = initial;
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            if (Stopwatch.GetElapsedTime(start).TotalSeconds >= result.BudgetSeconds ||
                result.AllocatedNativeBytes + capacity > 16 * 1024 * 1024) return null;
            nint buffer = Marshal.AllocHGlobal(capacity);
            result.BuffersOpened++;
            result.AllocatedNativeBytes += capacity;
            try
            {
                long callStart = Stopwatch.GetTimestamp();
                finalStatus = query(handle, informationClass, buffer, (uint)capacity, out uint returned);
                result.Calls.Add(new(informationClass, finalStatus, returned, capacity, Stopwatch.GetElapsedTime(callStart).TotalSeconds));
                if (finalStatus == 0)
                {
                    if (returned > capacity || returned < 16) return null;
                    byte[] bytes = new byte[checked((int)returned)];
                    Marshal.Copy(buffer, bytes, 0, bytes.Length);
                    return bytes;
                }
                if (!ResizeStatus(finalStatus) || returned > maximum) return null;
                capacity = checked((int)Math.Max((long)capacity * 2, returned));
                if (capacity > maximum) return null;
            }
            finally { Marshal.FreeHGlobal(buffer); result.BuffersClosed++; }
        }
        return null;
    }

    private static string? QueryType(Query query, nint duplicate, Snapshot result, long start)
    {
        int capacity = 1024;
        for (int attempt = 0; attempt < 3; attempt++)
        {
            if (Stopwatch.GetElapsedTime(start).TotalSeconds >= result.BudgetSeconds ||
                result.AllocatedNativeBytes + capacity > 16 * 1024 * 1024) return null;
            nint buffer = Marshal.AllocHGlobal(capacity);
            result.BuffersOpened++;
            result.AllocatedNativeBytes += capacity;
            try
            {
                long callStart = Stopwatch.GetTimestamp();
                // Synchronous NT calls cannot be cancelled; role/parent supervision is the hard outer boundary.
                int status = query(duplicate, 2, buffer, (uint)capacity, out uint returned);
                result.Calls.Add(new(2, status, returned, capacity, Stopwatch.GetElapsedTime(callStart).TotalSeconds));
                if (status == 0)
                {
                    if (returned > capacity || returned < 104) return null;
                    byte[] data = new byte[checked((int)returned)];
                    Marshal.Copy(buffer, data, 0, data.Length);
                    return ParseType(data, (ulong)buffer);
                }
                if (!ResizeStatus(status) || returned > 65536) return null;
                capacity = checked((int)Math.Max((long)capacity * 2, returned));
                if (capacity > 65536) return null;
            }
            finally { Marshal.FreeHGlobal(buffer); result.BuffersClosed++; }
        }
        return null;
    }

    internal sealed record Snapshot(string Role, int Round, string Phase)
    {
        public int SchemaVersion { get; init; } = 2;
        public string Schema { get; init; } = "phnt-ntpsapi-x64-header16-entry40;winternl-public-type104;NotStablePublicContract";
        public string OS { get; init; } = RuntimeInformation.OSDescription;
        public string Architecture { get; init; } = RuntimeInformation.ProcessArchitecture.ToString();
        public int PointerSize { get; init; } = IntPtr.Size;
        public int Pid { get; init; } = Environment.ProcessId;
        public DateTimeOffset Utc { get; init; } = DateTimeOffset.UtcNow;
        public long Qpc { get; init; } = Stopwatch.GetTimestamp();
        public long QpcFrequency { get; init; } = Stopwatch.Frequency;
        public long EndQpc { get; set; }
        public string Status { get; set; } = "Unavailable";
        public string? Reason { get; set; }
        public string? Error { get; set; }
        public string Conclusion { get; init; } = "DescriptiveOnly;TypeNotOwner;ConcurrentCloseOrReusePossible;DoesNotGateBusiness";
        public uint? BeforeHandles { get; set; }
        public uint? AfterHandles { get; set; }
        public int? Enumerated { get; set; }
        public int Classified { get; set; }
        public int? Unknown => Enumerated - Classified;
        public Dictionary<string, int> Types { get; init; } = new(StringComparer.Ordinal);
        public int Opened { get; set; }
        public int Closed { get; set; }
        public int Outstanding => Opened - Closed;
        public int BuffersOpened { get; set; }
        public int BuffersClosed { get; set; }
        public long AllocatedNativeBytes { get; set; }
        public double BudgetSeconds { get; init; } = 2;
        public double Seconds { get; set; }
        public double RoleSeconds { get; set; }
        public List<ThreadObservation> Threads { get; init; } = [];
        public CallbackCandidate[] Callbacks { get; set; } = [];
        public Dictionary<uint, int> HandlesPerTid => Threads.Where(t => t.Tid.Status == "Succeeded" && t.Tid.Value is > 0)
            .GroupBy(t => t.Tid.Value.GetValueOrDefault()).ToDictionary(g => g.Key, g => g.Count());
        public int DistinctTid => HandlesPerTid.Count;
        public string ThreadCorrelationStatus => Status != "Complete" || Threads.Any(t =>
            t.Tid.Status != "Succeeded" || t.ExitCode.Status != "Succeeded" || t.State == "Unknown" ||
            t.Close.Status != "Succeeded" || t.Consistency == "TransitionOrInconsistent") ? "Partial" : "ObservedNonAtomicAmbiguous";
        public double ThreadQuerySeconds => Threads.Sum(t => t.QuerySeconds);
        public string CorrelationSemantics { get; init; } = "Ambiguous:NonAtomic;TidAndHandleReusePossible;CallbackCandidateNotOwnerOrSchedulerLifetime;ReadCallbackFilesForQpcPidThread;NoEventOwnerOrRootOrFinalCloseProof";
        public List<Call> Calls { get; init; } = [];
        public List<Failure> Failures { get; init; } = [];
        public List<string> CleanupErrors { get; init; } = [];
    }

    internal static string ClassifyThread(uint? waitResult) => waitResult switch
    {
        0 => "TerminatedAtWait",
        258 => "NotTerminatedAtWait",
        _ => "Unknown"
    };

    internal static string TidStatus(uint tid) => tid == 0 ? "Failed" : "Succeeded";

    private static void ObserveThread(nint duplicate, ThreadObservation thread, Snapshot snapshot, long start)
    {
        bool InBudget() => Stopwatch.GetElapsedTime(start).TotalSeconds < snapshot.BudgetSeconds;
        if (!InBudget()) return;
        long before = Stopwatch.GetTimestamp();
        uint tid = GetThreadId(duplicate);
        int? error = tid == 0 ? Marshal.GetLastPInvokeError() : null;
        long after = Stopwatch.GetTimestamp();
        thread.Tid = new(TidStatus(tid), tid, error, before, after);
        if (!InBudget()) return;
        // The snapshot layout cannot reliably establish duplicate rights. A zero wait never consumes a Thread signal.
        before = Stopwatch.GetTimestamp();
        uint wait = WaitForSingleObject(duplicate, 0);
        error = wait == uint.MaxValue ? Marshal.GetLastPInvokeError() : null;
        after = Stopwatch.GetTimestamp();
        thread.Wait = new(wait == uint.MaxValue ? "Failed" : wait is 0 or 258 ? "Succeeded" : "UnexpectedResult",
            wait, error, before, after);
        if (!InBudget()) return;
        before = Stopwatch.GetTimestamp();
        bool success = GetExitCodeThread(duplicate, out uint exitCode);
        error = success ? null : Marshal.GetLastPInvokeError();
        after = Stopwatch.GetTimestamp();
        thread.ExitCode = new(success ? "Succeeded" : "Failed", success ? exitCode : null, error, before, after);
    }

    internal sealed record QueryObservation(string Status, uint? Value, int? Win32Error, long BeforeQpc, long AfterQpc)
    {
        public string ErrorClass => Status == "NotRunBudget" ? "NotObserved" : Win32Error switch
        {
            5 => "AccessDenied",
            null => "NoWin32FailureRecorded",
            _ => "OtherWin32Error"
        };
    }

    internal sealed record ThreadObservation(int EntryIndex, ulong OriginalHandleValue)
    {
        public int SchemaVersion { get; init; } = 1;
        public string Type { get; init; } = "Thread";
        public string SynchronizePermission { get; init; } = "Unknown;AttemptZeroWaitWithoutElevation";
        public QueryObservation Tid { get; set; } = new("NotRunBudget", null, null, 0, 0);
        public QueryObservation Wait { get; set; } = new("NotRunBudget", null, null, 0, 0);
        public QueryObservation ExitCode { get; set; } = new("NotRunBudget", null, null, 0, 0);
        public QueryObservation Close { get; set; } = new("NotObserved", null, null, 0, 0);
        public int OwnDuplicatesOpened { get; init; } = 1;
        public int OwnDuplicatesClosed => Close.Status == "Succeeded" ? 1 : 0;
        public int Outstanding => OwnDuplicatesOpened - OwnDuplicatesClosed;
        public int[] MatchedCallbackRounds { get; set; } = [];
        public string IdentityConsistency { get; init; } = "Ambiguous:OriginalMayHaveBeenReusedBeforeDuplicate;NoCreationTime;NonAtomicQueries";
        public string State => ClassifyThread(Wait.Value);
        public string Consistency => Wait.Value == 258 && ExitCode.Status == "Succeeded" && ExitCode.Value != 259
            ? "TransitionOrInconsistent" : "NonAtomic";
        public double QuerySeconds => (Tid.AfterQpc - Tid.BeforeQpc + Wait.AfterQpc - Wait.BeforeQpc +
            ExitCode.AfterQpc - ExitCode.BeforeQpc) / (double)Stopwatch.Frequency;
    }

    internal sealed record CallbackCandidate(int Round, uint Tid, string CallbacksPath, string BusinessPath);
    internal sealed record Call(int InformationClass, int Status, uint ReturnLength, int Capacity, double Seconds);
    internal sealed record Failure(int EntryIndex, string Operation, int? Win32Error);
    private sealed record WriteRecord(string Role, int Round, string Phase, double CaptureAndFirstWriteSeconds);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate int Query(nint handle, int informationClass, nint buffer, uint length, out uint returned);
    [DllImport("kernel32.dll")]
    private static extern nint GetCurrentProcess();
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetProcessHandleCount(nint process, out uint count);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateHandle(nint sourceProcess, nint source, nint targetProcess, out nint duplicate,
        uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint options);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint GetThreadId(nint thread);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(nint handle, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetExitCodeThread(nint thread, out uint exitCode);
}
#endif
