#if ORIGINAL_PRESENT_SYSTEM_TRACE
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [ThreadStatic] private static OriginalPresentSystemTrace? originalPresentTrace;

    private sealed class OriginalPresentSystemTrace : IDisposable
    {
        private static readonly Guid Provider = new("783aca0a-790e-4d7f-8451-aa850511c6b9");
        private readonly object gate = new();
        private readonly TraceReport report = new();
        private readonly string name = "WpfGfxShape-OriginalPresent-" + Guid.NewGuid().ToString("N");
        private string root = "";
        private nint properties;
        private ulong session;
        private bool owned, stopped, disposed, attempted;
        private System.Threading.Timer? deadline;

        internal OriginalPresentSystemTrace()
        {
            if (originalPresentTrace is not null) throw new InvalidOperationException(TraceMessage());
            originalPresentTrace = this;
            report.Phases.Add(new("ScopeCreated", Stopwatch.GetTimestamp()));
        }

        private static string TraceMessage() => new System.Resources.ResourceManager(
            "WpfGfxShape.ComAcceptance.AcceptanceResources", typeof(D3DImageTests).Assembly)
            .GetString("OriginalPresentTraceContract", CultureInfo.CurrentUICulture) ?? nameof(OriginalPresentSystemTrace);

        private static bool DiagnosticError(Exception error) => error is IOException or UnauthorizedAccessException
            or Win32Exception or InvalidOperationException or ArgumentException or NotSupportedException
            or System.Security.SecurityException or OverflowException or TypeLoadException;

        private void Error(string operation, Exception error) => report.Errors.Add(new(operation, error.GetType().FullName ?? "", error.Message));
        private void Api(string operation, long status) => report.Apis.Add(new(operation, status, Stopwatch.GetTimestamp()));
        private static string Pointer(nint value) => "0x" + unchecked((nuint)value).ToString("X", CultureInfo.InvariantCulture);

        internal void Before(nint window, nint device, bool extended)
        {
            lock (gate)
            {
                if (attempted) return;
                attempted = true;
                report.Window = Pointer(window);
                report.Device = Pointer(device);
                report.Extended = extended;
                report.ThreadId = TraceGetCurrentThreadId();
                try
                {
#if ORIGINAL_PRESENT_DXGKRNL
                    report.WindowObservedQpc = Stopwatch.GetTimestamp();
                    report.WindowOwnerThreadId = TraceGetWindowThreadProcessId(window, out uint ownerProcess);
                    report.WindowOwnerProcessId = ownerProcess;
                    report.WindowOwnerQueryError = report.WindowOwnerThreadId == 0 ? Marshal.GetLastWin32Error() : 0;
#endif
                    Start();
                }
                catch (Exception error) when (DiagnosticError(error))
                {
                    Error("Start", error);
                    StopCore("SetupFailure");
                }
                // No file I/O or diagnostic query between this mark and the original call.
                report.PresentBeforeQpc = Stopwatch.GetTimestamp();
            }
        }

        internal void After(long qpc, int? result)
        {
            lock (gate)
            {
                report.PresentAfterQpc = qpc;
                report.PresentResult = result;
#if !ORIGINAL_PRESENT_DXGKRNL
                StopCore("PresentReturned");
#endif
            }
        }

#if ORIGINAL_PRESENT_DXGKRNL
        internal void CompleteExit(long qpc, bool entered, bool returned)
        {
            lock (gate)
            {
                report.CompleteExitQpc = qpc;
                report.CompleteEntered = entered;
                report.CompleteReturned = returned;
                report.CompleteState = returned ? "Returned" : entered ? "Aborted" : "NotEntered.OriginalCallOrAssertAborted";
                report.TruncatedBeforeComplete = report.StopRequestedQpc is long stop && stop < qpc;
                StopCore(returned ? "CompleteReturned" : "OriginalBodyAborted");
            }
        }
#endif

        private void Start()
        {
            int expected = IntPtr.Size == 4 ? 108 : 112;
            report.PropertiesSize = Marshal.SizeOf<TraceProperties>();
            report.LogFileNameOffsetField = (int)Marshal.OffsetOf<TraceProperties>(nameof(TraceProperties.LogFileNameOffset));
            report.LoggerThreadOffset = (int)Marshal.OffsetOf<TraceProperties>(nameof(TraceProperties.LoggerThreadId));
            if (Marshal.SizeOf<TraceWnode>() != 48 || report.PropertiesSize != 120
                || report.LogFileNameOffsetField != expected || report.LoggerThreadOffset != 104
                || Marshal.SizeOf<TracePrivilege>() != 16)
                throw new InvalidOperationException(TraceMessage());
            // Exercise the exact report type before acquiring a token or session.
            _ = JsonSerializer.Serialize(report);
            root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Results", name));
            Directory.CreateDirectory(root);
            report.SessionName = name;
            report.EtlPath = Path.Combine(root, "original-present.etl");
            report.Phases.Add(new("Setup", Stopwatch.GetTimestamp()));
            int loggerBytes = checked((name.Length + 1) * 2);
            int fileBytes = checked((report.EtlPath.Length + 1) * 2);
            int total = checked(120 + loggerBytes + fileBytes);
            properties = Marshal.AllocHGlobal(total);
            new Span<byte>((void*)properties, total).Clear();
            TraceProperties value = new()
            {
                Wnode = new() { BufferSize = (uint)total, ClientContext = 1, Flags = 0x20000 },
                BufferSize = 64, MinimumBuffers = 2, MaximumBuffers = 8,
                MaximumFileSize = 16, LogFileMode = 1, FlushTimer = 1,
                LoggerNameOffset = 120, LogFileNameOffset = (uint)(120 + loggerBytes)
            };
            Marshal.StructureToPtr(value, properties, false);
            Marshal.Copy((name + '\0').ToCharArray(), 0, properties + 120, name.Length + 1);
            Marshal.Copy((report.EtlPath + '\0').ToCharArray(), 0, properties + 120 + loggerBytes, report.EtlPath.Length + 1);
            File.WriteAllText(Path.Combine(root, "setup.json"), JsonSerializer.Serialize(report));
            deadline = new System.Threading.Timer(_ => Deadline(), null, Timeout.Infinite, Timeout.Infinite);
            nint token = 0;
            TracePrivilege previous = default;
            bool restore = false;
            try
            {
                bool opened = TraceOpenProcessToken(TraceGetCurrentProcess(), 0x28, out token);
                Api("OpenProcessToken", opened ? 0 : Marshal.GetLastWin32Error());
                if (!opened) return;
                bool lookup = TraceLookupPrivilegeValueW(null, "SeSystemProfilePrivilege", out TraceLuid luid);
                Api("LookupPrivilegeValueW", lookup ? 0 : Marshal.GetLastWin32Error());
                if (!lookup) return;
                TracePrivilege requested = new() { Count = 1, Luid = luid, Attributes = 2 };
                Marshal.SetLastPInvokeError(0);
                bool adjusted = TraceAdjustTokenPrivileges(token, false, ref requested, 16, out previous, out uint returned);
                int adjustError = Marshal.GetLastWin32Error();
                Api("AdjustTokenPrivileges.Bool", adjusted ? 1 : 0);
                Api("AdjustTokenPrivileges.LastError", adjustError);
                report.PreviousPrivilegeCount = previous.Count;
                report.PreviousPrivilegeAttributes = previous.Attributes;
                report.PreviousPrivilegeBytes = returned;
                restore = adjusted && previous.Count != 0;
                if (!adjusted || adjustError != 0) return;
                report.StartRequestedQpc = Stopwatch.GetTimestamp();
                uint status = TraceStartTraceW(out session, name, properties);
                Api("StartTraceW", status);
                if (status != 0) return;
                owned = true;
                report.SessionHandle = session.ToString("X16", CultureInfo.InvariantCulture);
                report.SessionGuid = Marshal.PtrToStructure<TraceProperties>(properties).Wnode.Guid;
                deadline.Change(TimeSpan.FromSeconds(2), Timeout.InfiniteTimeSpan);
                Guid provider = Provider;
                status = TraceEnableTraceEx2(session, ref provider, 1, 6, 0xf, 0, 500, 0);
                Api("EnableTraceEx2.D3D9", status);
                report.ProviderEnabled = status == 0;
                if (status != 0) StopCore("EnableFailed");
#if ORIGINAL_PRESENT_DXGKRNL
                if (status == 0)
                {
                    Guid kernel = new("802ec45a-1e99-4b83-9920-87c98277ba9d");
                    status = TraceEnableTraceEx2(session, ref kernel, 1, 4, 0x08000000, 0, 500, 0);
                    Api("EnableTraceEx2.DxgKrnl", status);
                    report.DxgKrnlEnabled = status == 0;
                    if (status != 0) StopCore("EnableDxgKrnlFailed");
                }
#endif
            }
            finally
            {
                if (restore)
                {
                    Marshal.SetLastPInvokeError(0);
                    bool restored = TraceRestoreTokenPrivileges(token, false, ref previous, 0, 0, 0);
                    int error = Marshal.GetLastWin32Error();
                    Api("RestoreTokenPrivileges.Bool", restored ? 1 : 0);
                    Api("RestoreTokenPrivileges.LastError", error);
                    report.PrivilegeRestored = restored && error == 0;
                    if (!report.PrivilegeRestored) StopCore("PrivilegeRestoreFailed");
                }
                else report.PrivilegeRestored = true;
                if (token != 0)
                {
                    bool closed = TraceCloseHandle(token);
                    Api("CloseTokenHandle", closed ? 0 : Marshal.GetLastWin32Error());
                }
            }
        }

        private void Deadline()
        {
            lock (gate)
            {
                if (disposed) return;
                StopCore("Deadline");
            }
        }

        private void StopCore(string reason)
        {
            if (!owned || stopped) return;
            // Exactly one controller owns STOP. Never fall back to a name lookup or retry.
            stopped = true;
            report.StopReason = reason;
            report.StopRequestedQpc = Stopwatch.GetTimestamp();
#if ORIGINAL_PRESENT_DXGKRNL
            report.TruncatedBeforeComplete = report.CompleteExitQpc is null || report.StopRequestedQpc < report.CompleteExitQpc;
#endif
            try
            {
                uint status = TraceControlTraceW(session, null, properties, 1);
                report.StopCompletedQpc = Stopwatch.GetTimestamp();
                Api("ControlTraceW.Stop", status);
                report.StopStatus = status;
                TraceProperties final = Marshal.PtrToStructure<TraceProperties>(properties);
                report.EventsLost = final.EventsLost;
                report.LogBuffersLost = final.LogBuffersLost;
                report.RealTimeBuffersLost = final.RealTimeBuffersLost;
                report.BuffersWritten = final.BuffersWritten;
                report.NumberOfBuffers = final.NumberOfBuffers;
                report.LoggerThreadId = Pointer(final.LoggerThreadId);
                report.StatisticsValid = status == 0;
            }
            catch (Exception error) when (DiagnosticError(error)) { Error("Stop", error); }
        }

        /// <summary>Stops only this scope's owned session and preserves independent diagnostic failures.</summary>
        public void Dispose()
        {
            lock (gate)
            {
                if (disposed) return;
                StopCore("ScopeFinally");
                disposed = true;
                originalPresentTrace = null;
                deadline?.Dispose();
                if (properties != 0) { Marshal.FreeHGlobal(properties); properties = 0; }
                report.Phases.Add(new("ScopeClosed", Stopwatch.GetTimestamp()));
                try
                {
                    if (root.Length == 0)
                    {
                        root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Results", name));
                        Directory.CreateDirectory(root);
                    }
                    if (report.StopStatus == 0 && File.Exists(report.EtlPath))
                    {
                        try
                        {
                            report.EtlBytes = new FileInfo(report.EtlPath).Length;
                            using FileStream stream = File.OpenRead(report.EtlPath);
                            report.EtlSha256 = Convert.ToHexString(SHA256.HashData(stream));
                        }
                        catch (Exception error) when (DiagnosticError(error)) { Error("EtlSummary", error); }
                    }
                    File.WriteAllText(Path.Combine(root, "controller.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception error) when (DiagnosticError(error))
                {
                    Error("Report", error);
                    try { Console.Error.WriteLine(JsonSerializer.Serialize(new { kind = "OriginalPresentTraceLogFailure", report.Errors, report.Apis })); }
                    catch (Exception loggingError) when (DiagnosticError(loggingError))
                    {
                        Debug.WriteLine(loggingError);
                    }
                }
            }
        }

        private sealed record ApiResult(string Api, long Status, long Qpc);
        private sealed record Phase(string Name, long Qpc);
        private sealed record Failure(string Operation, string Type, string Message);
        private sealed class TraceReport
        {
#if ORIGINAL_PRESENT_DXGKRNL
            public string Diagnostic { get; } = "OriginalPresentDxgKrnl.StageA.RawOnly";
            public string FilterPolicy { get; } = "BroadKeyword.NoEventIdFilter.AllOtherRawEventsRetained.NotTargetDecoded";
            public string DescriptorSource { get; } = "https://github.com/GameTechDev/PresentMon/blob/main/PresentData/ETW/Microsoft_Windows_DxgKrnl.h (mutable main; not runtime TDH certification)";
            public string RequestedDescriptors { get; } = "DxgKrnl:166v0/L4/C17/O0/T103;171v2/L0/C17/O1/T6;172v2/L0/C17/O0/T6;keyword=0x4000000008000001";
            public string DxgKrnlProvider { get; } = "802ec45a-1e99-4b83-9920-87c98277ba9d";
            public byte DxgKrnlLevel { get; } = 4;
            public string DxgKrnlMatchAny { get; } = "0x0000000008000000";
            public string MatchAll { get; } = "0x0000000000000000";
            public int EnableTimeoutMilliseconds { get; } = 500;
            public bool DxgKrnlEnabled { get; set; }
            public uint WindowOwnerThreadId { get; set; }
            public uint WindowOwnerProcessId { get; set; }
            public int WindowOwnerQueryError { get; set; }
            public long? WindowObservedQpc { get; set; }
            public string WindowLifetimeBoundary { get; } = "ExistingVisualCacheScope.BeforePresentThroughCompleteExit.BeforeWindowDestruction.NotCreationTimestamp";
            public long? CompleteExitQpc { get; set; }
            public bool CompleteEntered { get; set; }
            public bool CompleteReturned { get; set; }
            public string CompleteState { get; set; } = "NotReached";
            public bool TruncatedBeforeComplete { get; set; }
            public string EventHeaderIdentity { get; } = "SourcePID/TID/rawQPC retained in ETL; not decoded in stage A";
#endif
            public int SchemaVersion { get; } = 1;
            public string Evidence { get; } = "RawEtlOnly.NotPresentationProof.NoDecoding.NoCorrelation";
            public string ProviderGuid { get; } = Provider.ToString();
            public string Keyword { get; } = "0x000000000000000F";
            public byte Level { get; } = 6;
            public int DeadlineMilliseconds { get; } = 2000;
            public int BufferKiB { get; } = 64;
            public int MinimumBuffers { get; } = 2;
            public int MaximumBuffers { get; } = 8;
            public int MaximumFileMiB { get; } = 16;
            public string FileMode { get; } = "Sequential";
            public int ProcessId { get; } = Environment.ProcessId;
            public int PointerSize { get; } = IntPtr.Size;
            public long QpcFrequency { get; } = Stopwatch.Frequency;
            public string AssemblyPath { get; } = typeof(D3DImageTests).Assembly.Location;
            public Guid Mvid { get; } = typeof(D3DImageTests).Assembly.ManifestModule.ModuleVersionId;
            public string Runtime { get; } = RuntimeInformation.FrameworkDescription;
            public string SessionName { get; set; } = "";
            public string SessionHandle { get; set; } = "";
            public Guid SessionGuid { get; set; }
            public string EtlPath { get; set; } = "";
            public string Window { get; set; } = "";
            public string Device { get; set; } = "";
            public bool Extended { get; set; }
            public uint ThreadId { get; set; }
            public int PropertiesSize { get; set; }
            public int LogFileNameOffsetField { get; set; }
            public int LoggerThreadOffset { get; set; }
            public long? StartRequestedQpc { get; set; }
            public long? PresentBeforeQpc { get; set; }
            public long? PresentAfterQpc { get; set; }
            public int? PresentResult { get; set; }
            public long? StopRequestedQpc { get; set; }
            public long? StopCompletedQpc { get; set; }
            public string StopReason { get; set; } = "";
            public uint? StopStatus { get; set; }
            public bool ProviderEnabled { get; set; }
            public bool PrivilegeRestored { get; set; }
            public uint PreviousPrivilegeCount { get; set; }
            public uint PreviousPrivilegeAttributes { get; set; }
            public uint PreviousPrivilegeBytes { get; set; }
            public bool StatisticsValid { get; set; }
            public uint? EventsLost { get; set; }
            public uint? LogBuffersLost { get; set; }
            public uint? RealTimeBuffersLost { get; set; }
            public uint? BuffersWritten { get; set; }
            public uint? NumberOfBuffers { get; set; }
            public string LoggerThreadId { get; set; } = "";
            public long? EtlBytes { get; set; }
            public string EtlSha256 { get; set; } = "";
            public List<ApiResult> Apis { get; } = [];
            public List<Phase> Phases { get; } = [];
            public List<Failure> Errors { get; } = [];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct TraceWnode
        {
            internal uint BufferSize, ProviderId;
            internal ulong HistoricalContext;
            internal long TimeStamp;
            internal Guid Guid;
            internal uint ClientContext, Flags;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        private struct TraceProperties
        {
            internal TraceWnode Wnode;
            internal uint BufferSize, MinimumBuffers, MaximumBuffers, MaximumFileSize, LogFileMode, FlushTimer, EnableFlags;
            internal int AgeLimit;
            internal uint NumberOfBuffers, FreeBuffers, EventsLost, BuffersWritten, LogBuffersLost, RealTimeBuffersLost;
            internal nint LoggerThreadId;
            internal uint LogFileNameOffset, LoggerNameOffset;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct TraceLuid { internal uint Low; internal int High; }
        [StructLayout(LayoutKind.Sequential)]
        private struct TracePrivilege { internal uint Count; internal TraceLuid Luid; internal uint Attributes; }

        [DllImport("advapi32.dll", EntryPoint = "StartTraceW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint TraceStartTraceW(out ulong handle, string name, nint properties);
        [DllImport("advapi32.dll", EntryPoint = "EnableTraceEx2", ExactSpelling = true)]
        private static extern uint TraceEnableTraceEx2(ulong handle, ref Guid provider, uint control, byte level, ulong any, ulong all, uint timeout, nint parameters);
        [DllImport("advapi32.dll", EntryPoint = "ControlTraceW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint TraceControlTraceW(ulong handle, string? name, nint properties, uint control);
#if ORIGINAL_PRESENT_DXGKRNL
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true, ExactSpelling = true)]
        private static extern uint TraceGetWindowThreadProcessId(nint window, out uint process);
#endif
        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        private static extern uint TraceGetCurrentThreadId();
        [DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess", ExactSpelling = true)]
        private static extern nint TraceGetCurrentProcess();
        [DllImport("advapi32.dll", EntryPoint = "OpenProcessToken", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TraceOpenProcessToken(nint process, uint access, out nint token);
        [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TraceLookupPrivilegeValueW(string? system, string name, out TraceLuid luid);
        [DllImport("advapi32.dll", EntryPoint = "AdjustTokenPrivileges", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TraceAdjustTokenPrivileges(nint token, [MarshalAs(UnmanagedType.Bool)] bool disableAll, ref TracePrivilege state, uint length, out TracePrivilege previous, out uint returned);
        [DllImport("advapi32.dll", EntryPoint = "AdjustTokenPrivileges", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TraceRestoreTokenPrivileges(nint token, [MarshalAs(UnmanagedType.Bool)] bool disableAll, ref TracePrivilege state, uint length, nint previous, nint returned);
        [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TraceCloseHandle(nint handle);
    }
}
#endif
