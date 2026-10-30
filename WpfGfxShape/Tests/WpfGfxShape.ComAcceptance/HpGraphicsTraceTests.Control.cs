#if HP_GRAPHICS_TRACE
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed partial class HpGraphicsTraceTests
{
    /// <summary>Checks one bounded private real-time ETW pipeline without starting any rendering.</summary>
    [TestMethod]
    public void WhenBoundedGraphicsControlRunsThenConsumptionAndCleanupAreRecorded() => RunBoundedControl(null);

    /// <summary>Records one fixed HM first-red presentation with a bounded, narrow ETW session.</summary>
    [TestMethod]
    public void WhenHrFirstRedRunsThenActualTraceBridgeIsRecorded() =>
        RunBoundedControl(() => new D3DImageTests().HrRunWithWindowOwner());

    private static void RunBoundedControl(Action? render)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", render is null ? "hq-graphics-trace" : "hr-present-trace");
        Directory.CreateDirectory(root);
        using var output = new StreamWriter(Path.Combine(root, "control.jsonl"), false);
        object gate = new();
        long written = 0;
        void Log(object value)
        {
            string line = JsonSerializer.Serialize(value);
            lock (gate)
            {
                written += System.Text.Encoding.UTF8.GetByteCount(line) + 2;
                if (written > 4 * 1024 * 1024) throw new InvalidDataException("HQ output budget");
                output.WriteLine(line);
                output.Flush();
            }
        }
        Assembly assembly = typeof(HpGraphicsTraceTests).Assembly;
        Log(new { kind = "identity", assembly = assembly.Location,
            sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))),
            mvid = assembly.ManifestModule.ModuleVersionId, runtime = RuntimeInformation.FrameworkDescription,
            architecture = RuntimeInformation.ProcessArchitecture.ToString(), pid = Environment.ProcessId,
            session = Process.GetCurrentProcess().SessionId, utc = DateTime.UtcNow,
            qpc = Stopwatch.GetTimestamp(), frequency = Stopwatch.Frequency, rendering = render is not null });
        string[] expected = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "package-files.txt")).Order(StringComparer.Ordinal).ToArray();
        string[] actual = Directory.GetFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(AppContext.BaseDirectory, p).Replace('\\', '/'))
            .Where(p => !p.StartsWith("Results/", StringComparison.OrdinalIgnoreCase) && !p.StartsWith("TestResults/", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal).ToArray();
        string native = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Native", "wpfgfx_cor3.dll"))));
        Log(new { kind = "package", expected = expected.Length, actual = actual.Length, missing = expected.Except(actual).ToArray(), extra = actual.Except(expected).ToArray(),
            native, nativeLoaded = false, runtimeconfig = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "WpfGfxShape.ComAcceptance.runtimeconfig.json")),
            contracts = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Contracts")).Select(Path.GetFileName).ToArray() });
        CollectionAssert.AreEqual(expected, actual);
        Assert.AreEqual(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aot.sha256")).Trim(), native);
        Assert.AreEqual(8, IntPtr.Size);
        Log(new { kind = "sdkLayout", sdk = "10.0.26100.0", properties = Marshal.SizeOf<TraceProperties>(), enable = Marshal.SizeOf<EnableParameters>(), filter = Marshal.SizeOf<FilterDescriptor>(),
            logfile = Marshal.SizeOf<LogFile>(), record = Marshal.SizeOf<EventRecord>(), descriptor = Marshal.SizeOf<Descriptor>(),
            callbackOffset = Marshal.OffsetOf<LogFile>(nameof(LogFile.Callback)).ToInt32(), userDataOffset = Marshal.OffsetOf<EventRecord>(nameof(EventRecord.UserData)).ToInt32(),
            unions = "WNODE history/timestamp; properties age/flush; logfile mode/callback; header kernel/user/processor time" });
        Assert.AreEqual(120, Marshal.SizeOf<TraceProperties>());
        Assert.AreEqual(48, Marshal.SizeOf<EnableParameters>());
        Assert.AreEqual(448, Marshal.SizeOf<LogFile>());
        Assert.AreEqual(112, Marshal.SizeOf<EventRecord>());
        Guid dwm = new("9e9bba3c-2e38-40cb-99f4-9e8281425164");
        Guid dxg = new("802ec45a-1e99-4b83-9920-87c98277ba9d");
        Security(dwm, Log);
        Security(dxg, Log);
        Verify(dwm, 67, 0x8000000000000001, Log);
        Verify(dxg, 166, 0x4000000008000001, Log);
        Verify(dxg, 501, 0x4000000008000001, Log);
        using var source = new HqControlSource();
        Guid control = source.Guid;
        string name = (render is null ? "HQ-" : "HR-") + Guid.NewGuid().ToString("N");
        IntPtr properties = Marshal.AllocHGlobal(1024);
        IntPtr loggerName = Marshal.StringToHGlobalUni(name);
        IntPtr token = IntPtr.Zero;
        TokenPrivilege previous = default;
        bool adjusted = false, started = false, joined = false;
        ulong sessionHandle = 0, consumerHandle = ulong.MaxValue;
        Thread? consumer = null, stopper = null;
        long setupQpc = Stopwatch.GetTimestamp(), captureQpc = 0, stopCompletedQpc = 0, marker = 0;
        Exception? renderFailure = null;
        using var stopCompleted = new ManualResetEventSlim();
        uint processStatus = uint.MaxValue, stopStatus = uint.MaxValue, closeStatus = uint.MaxValue;
        int callbacks = 0, controls = 0, graphics = 0, decoded = 0, callbackErrors = 0, budget = 0;
        long payloadBytes = 0;
        using var stopRequested = new ManualResetEventSlim();
        using var controlSeen = new ManualResetEventSlim();
        RecordCallback callback = recordPointer =>
        {
            try
            {
                EventRecord record = Marshal.PtrToStructure<EventRecord>(recordPointer);
                int count = Interlocked.Increment(ref callbacks);
                long bytes = Interlocked.Add(ref payloadBytes, record.UserDataLength);
                if (count > 2048 || bytes > 2 * 1024 * 1024) { Interlocked.Exchange(ref budget, 1); stopRequested.Set(); return; }
                bool own = record.Provider == control && record.Event.Id == 1;
                bool graphical = record.Provider == dwm || record.Provider == dxg;
                if (own) Interlocked.Increment(ref controls);
                if (graphical) Interlocked.Increment(ref graphics);
                if (!own && !graphical) return;
                byte[] raw = new byte[record.UserDataLength];
                if (raw.Length > 0) Marshal.Copy(record.UserData, raw, 0, raw.Length);
                uint size = 0;
                uint sizing = TdhGetEventInformation(recordPointer, 0, IntPtr.Zero, IntPtr.Zero, ref size);
                uint read = uint.MaxValue;
                var fields = new List<object>();
                if (sizing == 122 && size is > 0 and <= Limit)
                {
                    IntPtr metadata = Allocate(size);
                    try
                    {
                        read = TdhGetEventInformation(recordPointer, 0, IntPtr.Zero, metadata, ref size);
                        if (read == 0)
                        {
                            Check(size, 0, 112);
                            int top = Marshal.ReadInt32(metadata, 104);
                            if (top is < 0 or > 128) throw new InvalidDataException("HQ field limit");
                            for (int i = 0; i < top; i++)
                            {
                                int offset = checked(112 + i * 24);
                                Check(size, offset, 24);
                                string field = Text(metadata, size, Marshal.ReadInt32(metadata, offset + 4));
                                uint flags = unchecked((uint)Marshal.ReadInt32(metadata, offset));
                                fields.Add(new { field, flags, input = (ushort)Marshal.ReadInt16(metadata, offset + 8), outputType = (ushort)Marshal.ReadInt16(metadata, offset + 10),
                                    countOrIndex = (ushort)Marshal.ReadInt16(metadata, offset + 16), lengthOrIndex = (ushort)Marshal.ReadInt16(metadata, offset + 18),
                                    property = ReadScalarProperty(recordPointer, field, flags, (ushort)Marshal.ReadInt16(metadata, offset + 16)) });
                            }
                            Interlocked.Increment(ref decoded);
                        }
                    }
                    finally { Marshal.FreeHGlobal(metadata); }
                }
                Log(new { kind = "event", record.Provider, id = record.Event.Id, record.Event.Version, record.ProcessId, record.ThreadId, record.Timestamp,
                    record.Flags, record.UserDataLength, raw = Convert.ToHexString(raw), sizing, read, fields,
                    controlValue = own && raw.Length == 8 ? BitConverter.ToInt64(raw) : (long?)null,
                    controlPayloadMatches = own && raw.Length == 8 && BitConverter.ToInt64(raw) == marker,
                    controlDecode = own ? "known own Int64 payload; TDH status reported independently" : null,
                    targetAssociation = "requires exact HWND/token fields joined with hi-delivery.jsonl; time proximity alone is not attribution" });
                if (own) controlSeen.Set();
            }
            catch (Exception error) when (error is ArgumentException or InvalidDataException or OverflowException or IOException)
            {
                Interlocked.Increment(ref callbackErrors);
                stopRequested.Set();
            }
        };
        try
        {
            bool opened = OpenProcessToken(GetCurrentProcess(), 0x28, out token);
            Log(new { kind = "OpenProcessToken", opened, error = opened ? 0 : Marshal.GetLastWin32Error() });
            Assert.IsTrue(opened);
            bool lookup = LookupPrivilegeValueW(null, "SeSystemProfilePrivilege", out long luid);
            Log(new { kind = "LookupPrivilegeValue", lookup, error = lookup ? 0 : Marshal.GetLastWin32Error(), luid });
            Assert.IsTrue(lookup);
            var requested = new TokenPrivilege { Count = 1, Luid = luid, Attributes = 2 };
            bool adjust = AdjustTokenPrivileges(token, false, ref requested, 16, out previous, out uint returned);
            int adjustError = Marshal.GetLastWin32Error();
            adjusted = adjust && adjustError == 0 && previous.Count > 0;
            Log(new { kind = "AdjustTokenPrivileges", adjust, error = adjustError, returned, previous.Count, previous.Attributes, scope = "this process token only; restored in finally" });
            Assert.IsTrue(adjust && adjustError == 0);
            Marshal.Copy(new byte[1024], 0, properties, 1024);
            var props = new TraceProperties { WnodeBufferSize = 1024, ClientContext = 1, WnodeFlags = 0x20000,
                BufferSize = 64, MinimumBuffers = 2, MaximumBuffers = 4, LogFileMode = 0x100, FlushTimer = 1, LoggerNameOffset = 120 };
            Marshal.StructureToPtr(props, properties, false);
            Marshal.Copy((name + '\0').ToCharArray(), 0, properties + 120, name.Length + 1);
            Assert.IsTrue(Stopwatch.GetElapsedTime(setupQpc) < TimeSpan.FromSeconds(5));
            long startQpc = Stopwatch.GetTimestamp();
            captureQpc = startQpc;
            uint start = StartTraceW(out sessionHandle, name, properties);
            started = start == 0;
            Log(new { kind = "StartTrace", status = start, name, sessionHandle, startQpc, maxSeconds = 2, maxEvents = 2048, payloadLimit = 2097152, fileLimit = 4194304, etlFile = false });
            Assert.AreEqual(0u, start);
            stopper = new Thread(() =>
            {
                TimeSpan remaining = TimeSpan.FromSeconds(1.5) - Stopwatch.GetElapsedTime(startQpc);
                if (remaining > TimeSpan.Zero) stopRequested.Wait(remaining);
                long requestedQpc = Stopwatch.GetTimestamp();
                stopStatus = ControlTraceW(sessionHandle, name, properties, 1);
                stopCompletedQpc = Stopwatch.GetTimestamp();
                TraceProperties final = Marshal.PtrToStructure<TraceProperties>(properties);
                Log(new { kind = "StopTrace", status = stopStatus, name, requestedQpc, stopCompletedQpc,
                    seconds = Stopwatch.GetElapsedTime(startQpc, stopCompletedQpc).TotalSeconds,
                    final.EventsLost, final.LogBuffersLost, final.RealTimeBuffersLost, final.BuffersWritten });
                stopCompleted.Set();
            }) { IsBackground = true, Name = "HR-ETW-deadline" };
            stopper.Start();
            Enable(sessionHandle, control, [1], 5, ulong.MaxValue, Log);
            Enable(sessionHandle, dwm, [67], 4, 0x8000000000000001, Log);
            Enable(sessionHandle, dxg, [166, 501], 4, 0x4000000008000001, Log);
            var logfile = new LogFile { LoggerName = loggerName, Mode = 0x10001100, Callback = Marshal.GetFunctionPointerForDelegate(callback) };
            consumerHandle = OpenTraceW(ref logfile);
            int openError = consumerHandle == ulong.MaxValue ? Marshal.GetLastWin32Error() : 0;
            Log(new { kind = "OpenTrace", consumerHandle, error = openError });
            Assert.AreNotEqual(ulong.MaxValue, consumerHandle);
            consumer = new Thread(() =>
            {
                ulong handle = consumerHandle;
                processStatus = ProcessTrace(ref handle, 1, IntPtr.Zero, IntPtr.Zero);
            }) { IsBackground = true, Name = "HQ-ETW-consumer" };
            consumer.Start();
            marker = Stopwatch.GetTimestamp();
            source.Marker(marker);
            Log(new { kind = "controlWrite", marker, enabled = source.IsEnabled(), provider = control, eventId = 1 });
            if (render is not null && !stopCompleted.IsSet)
            {
                try { render(); }
                catch (AssertFailedException error) { renderFailure = error; }
            }
            Assert.IsTrue(stopCompleted.Wait(TimeSpan.FromSeconds(4)), "ETW STOP did not complete within its independent budget");
            Log(new { kind = "controllerEnd", elapsedSeconds = Stopwatch.GetElapsedTime(startQpc).TotalSeconds, controlObserved = controlSeen.IsSet });
        }
        finally
        {
            if (started)
            {
                stopRequested.Set();
                if (stopper is not null) Assert.IsTrue(stopper.Join(TimeSpan.FromSeconds(5)), "ETW stop thread did not join");
                else
                {
                    stopStatus = ControlTraceW(sessionHandle, name, properties, 1);
                    stopCompletedQpc = Stopwatch.GetTimestamp();
                    Log(new { kind = "StopTraceSetupFailure", status = stopStatus, stopCompletedQpc });
                }
                if (consumer is not null) joined = consumer.Join(TimeSpan.FromSeconds(3));
            }
            if (consumerHandle != ulong.MaxValue)
            {
                closeStatus = CloseTrace(consumerHandle);
                Log(new { kind = "CloseTrace", status = closeStatus });
                if (!joined && consumer is not null) joined = consumer.Join(TimeSpan.FromSeconds(3));
            }
            if (consumer is not null) Log(new { kind = "ProcessTrace", status = processStatus, joined });
            if (adjusted)
            {
                bool restored = AdjustTokenPrivileges(token, false, ref previous, 16, out _, out _);
                Log(new { kind = "restorePrivilege", restored, error = Marshal.GetLastWin32Error() });
            }
            if (token != IntPtr.Zero) Log(new { kind = "CloseToken", ok = CloseHandle(token) });
            Marshal.FreeHGlobal(loggerName);
            Marshal.FreeHGlobal(properties);
            GC.KeepAlive(callback);
            Log(new { kind = "cleanup", started, stopStatus, closeStatus, processStatus, joined, callbacks, controls, graphics, decoded, callbackErrors, budget, payloadBytes,
                rendering = render is not null, otherSessionsTouched = false, targetAssociation = "not inferred; inspect actual fields",
                captureQpc, stopCompletedQpc, setupQpc, renderFailure = renderFailure?.ToString() });
        }
        Assert.AreEqual(0u, stopStatus);
        Assert.IsTrue(joined);
        Assert.AreEqual(0u, processStatus);
        Assert.AreEqual(0u, closeStatus);
        Assert.AreEqual(0, callbackErrors);
        Assert.AreEqual(0, budget);
        Assert.IsTrue(controls > 0);
        Assert.IsTrue(Stopwatch.GetElapsedTime(captureQpc, stopCompletedQpc) <= TimeSpan.FromSeconds(2), "Capture exceeded two seconds");
        if (renderFailure is not null) throw renderFailure;
    }

    private static object ReadScalarProperty(IntPtr record, string name, uint flags, ushort count)
    {
        if ((flags & 0x23) != 0 || count != 1)
            return new { decoded = false, reason = "struct/custom/array requires separate schema handling", flags, count };
        IntPtr text = Marshal.StringToHGlobalUni(name);
        try
        {
            var descriptor = new PropertyDescriptor { Name = unchecked((ulong)text.ToInt64()), ArrayIndex = uint.MaxValue };
            uint size = 0;
            uint sizing = TdhGetPropertySize(record, 0, IntPtr.Zero, 1, ref descriptor, out size);
            if (sizing != 0 || size is not (1 or 2 or 4 or 8))
                return new { decoded = false, sizing, size };
            IntPtr data = Marshal.AllocHGlobal((int)size);
            try
            {
                uint status = TdhGetProperty(record, 0, IntPtr.Zero, 1, ref descriptor, size, data);
                byte[] raw = new byte[size];
                if (status == 0) Marshal.Copy(data, raw, 0, raw.Length);
                ulong? value = status != 0 ? null : size switch
                {
                    1 => raw[0], 2 => BitConverter.ToUInt16(raw), 4 => BitConverter.ToUInt32(raw),
                    8 => BitConverter.ToUInt64(raw), _ => null
                };
                return new { decoded = status == 0, sizing, status, size, raw = status == 0 ? Convert.ToHexString(raw) : null, value };
            }
            finally { Marshal.FreeHGlobal(data); }
        }
        finally { Marshal.FreeHGlobal(text); }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PropertyDescriptor { internal ulong Name; internal uint ArrayIndex, Reserved; }
    [DllImport("tdh.dll", ExactSpelling = true)]
    private static extern uint TdhGetPropertySize(IntPtr record, uint contextCount, IntPtr context, uint propertyCount, ref PropertyDescriptor descriptor, out uint size);
    [DllImport("tdh.dll", ExactSpelling = true)]
    private static extern uint TdhGetProperty(IntPtr record, uint contextCount, IntPtr context, uint propertyCount, ref PropertyDescriptor descriptor, uint size, IntPtr data);

    private static void Verify(Guid provider, ushort id, ulong keyword, Action<object> log)
    {
        var descriptor = new Descriptor { Id = id, Level = 4, Keyword = keyword };
        uint size = 0;
        uint sizing = TdhGetManifestEventInformation(ref provider, ref descriptor, IntPtr.Zero, ref size);
        log(new { kind = "schemaSizing", provider, id, sizing, size });
        Assert.AreEqual(122u, sizing);
        IntPtr data = Allocate(size);
        try
        {
            uint status = TdhGetManifestEventInformation(ref provider, ref descriptor, data, ref size);
            log(new { kind = "schemaVerified", provider, id, status, size, schemaOnly = true });
            Assert.AreEqual(0u, status);
            Check(size, 0, 112);
            Descriptor actual = Marshal.PtrToStructure<Descriptor>(data + 32);
            log(new { kind = "manifestDescriptor", provider, actual.Id, actual.Version, actual.Level, actual.Keyword });
            Assert.AreEqual(id, actual.Id);
            Assert.AreEqual(descriptor.Keyword, actual.Keyword);
            Assert.AreEqual(descriptor.Level, actual.Level);
        }
        finally { Marshal.FreeHGlobal(data); }
    }

    private static void Enable(ulong session, Guid provider, ushort[] ids, byte level, ulong keyword, Action<object> log)
    {
        IntPtr filter = Marshal.AllocHGlobal(4 + ids.Length * 2);
        IntPtr descriptor = Marshal.AllocHGlobal(16);
        try
        {
            Marshal.WriteInt32(filter, (ids.Length << 16) | 1);
            for (int i = 0; i < ids.Length; i++) Marshal.WriteInt16(filter, 4 + i * 2, unchecked((short)ids[i]));
            Marshal.StructureToPtr(new FilterDescriptor { Pointer = unchecked((ulong)filter.ToInt64()), Size = (uint)(4 + ids.Length * 2), Type = 0x80000200 }, descriptor, false);
            var parameters = new EnableParameters { Version = 2, Filters = descriptor, Count = 1 };
            uint status = EnableTraceEx2(session, ref provider, 1, level, keyword, 0, 100, ref parameters);
            log(new { kind = "EnableTraceEx2", provider, ids, level, keyword = keyword.ToString("X16"), status, filterType = "EVENT_ID; no fallback" });
            Assert.AreEqual(0u, status);
        }
        finally { Marshal.FreeHGlobal(descriptor); Marshal.FreeHGlobal(filter); }
    }

    [EventSource(Name = "WpfGfxShape-HQ-Control")]
    private sealed class HqControlSource : EventSource
    {
        internal HqControlSource() : base(EventSourceSettings.EtwManifestEventFormat) { }
        [Event(1, Level = EventLevel.Informational)]
        public void Marker(long qpc) => WriteEvent(1, qpc);
    }

    // SDK x64 unions occupy their largest member, not the sum of their alternatives.
    [StructLayout(LayoutKind.Explicit, Size = 120)]
    private struct TraceProperties
    {
        [FieldOffset(0)] internal uint WnodeBufferSize;
        [FieldOffset(40)] internal uint ClientContext;
        [FieldOffset(44)] internal uint WnodeFlags;
        [FieldOffset(48)] internal uint BufferSize;
        [FieldOffset(52)] internal uint MinimumBuffers;
        [FieldOffset(56)] internal uint MaximumBuffers;
        [FieldOffset(64)] internal uint LogFileMode;
        [FieldOffset(68)] internal uint FlushTimer;
        [FieldOffset(88)] internal uint EventsLost;
        [FieldOffset(92)] internal uint BuffersWritten;
        [FieldOffset(96)] internal uint LogBuffersLost;
        [FieldOffset(100)] internal uint RealTimeBuffersLost;
        [FieldOffset(116)] internal uint LoggerNameOffset;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct EnableParameters
    {
        internal uint Version, EnableProperty, ControlFlags;
        internal Guid Source;
        internal IntPtr Filters;
        internal uint Count;
    }
    [StructLayout(LayoutKind.Sequential)]
    private struct FilterDescriptor { internal ulong Pointer; internal uint Size, Type; }
    [StructLayout(LayoutKind.Explicit, Size = 448)]
    private struct LogFile
    {
        [FieldOffset(8)] internal IntPtr LoggerName;
        [FieldOffset(28)] internal uint Mode;
        [FieldOffset(424)] internal IntPtr Callback;
    }
    [StructLayout(LayoutKind.Explicit, Size = 112)]
    private struct EventRecord
    {
        [FieldOffset(4)] internal ushort Flags;
        [FieldOffset(8)] internal uint ThreadId;
        [FieldOffset(12)] internal uint ProcessId;
        [FieldOffset(16)] internal long Timestamp;
        [FieldOffset(24)] internal Guid Provider;
        [FieldOffset(40)] internal Descriptor Event;
        [FieldOffset(86)] internal ushort UserDataLength;
        [FieldOffset(96)] internal IntPtr UserData;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct TokenPrivilege { internal uint Count; internal long Luid; internal uint Attributes; }
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void RecordCallback(IntPtr record);
    [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern uint StartTraceW(out ulong handle, string name, IntPtr properties);
    [DllImport("advapi32.dll", ExactSpelling = true)]
    private static extern uint EnableTraceEx2(ulong handle, ref Guid provider, uint control, byte level, ulong any, ulong all, uint timeout, ref EnableParameters parameters);
    [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern uint ControlTraceW(ulong handle, string name, IntPtr properties, uint control);
    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern ulong OpenTraceW(ref LogFile logfile);
    [DllImport("advapi32.dll", ExactSpelling = true)]
    private static extern uint ProcessTrace(ref ulong handles, uint count, IntPtr start, IntPtr end);
    [DllImport("advapi32.dll", ExactSpelling = true)]
    private static extern uint CloseTrace(ulong handle);
    [DllImport("tdh.dll", ExactSpelling = true)]
    private static extern uint TdhGetEventInformation(IntPtr record, uint count, IntPtr context, IntPtr buffer, ref uint size);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetCurrentProcess();
    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);
    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);
    [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LookupPrivilegeValueW(string? system, string name, out long luid);
    [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AdjustTokenPrivileges(IntPtr token, [MarshalAs(UnmanagedType.Bool)] bool disableAll, ref TokenPrivilege state, uint length, out TokenPrivilege previous, out uint returned);
}
#endif
