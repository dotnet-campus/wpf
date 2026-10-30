#if ORIGINAL_PRESENT_OFFLINE_TDH
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    /// <summary>Decodes the recovered file only; does not create a graphics device or trace session.</summary>
    [TestMethod]
    public void WhenRecoveredOriginalPresentEtlIsDecodedOfflineThenRuntimeBoundaryEvidenceIsRecorded()
    {
        OfflineTdh.Decode(false);
    }

#if DXGKRNL_OFFLINE_TDH
    /// <summary>Decodes only the fixed stage B ETL and records bounded kernel evidence.</summary>
    [TestMethod]
    public void WhenRecoveredDxgKrnlEtlIsDecodedOfflineThenBoundedKernelEvidenceIsRecorded()
    {
        OfflineTdh.Decode(true);
    }
#endif

    private static class OfflineTdh
    {
        private static readonly Guid D3D9 = new("783aca0a-790e-4d7f-8451-aa850511c6b9");
        private static readonly Guid DxgKrnl = new("802ec45a-1e99-4b83-9920-87c98277ba9d");
        private sealed record Descriptor(ushort Id, byte Version, byte Channel, byte Level, byte Opcode, ushort Task, ulong Keyword);
        private sealed record Property(string Name, uint Flags, ushort InType, ushort OutType, ushort SchemaCount, ushort SchemaLength, uint? Bytes, string? Value, string? Hex, uint? SizeStatus, uint? ValueStatus, string State);
        private sealed record Event(Guid Provider, Descriptor Descriptor, string SchemaHex, string PayloadHex, uint HeaderPid, uint HeaderTid, long RawTimestamp, ushort HeaderFlags, ushort PayloadBytes, uint SchemaStatus, string? TaskName, string? OpcodeName, string? EventName, uint? DecodingSource, List<Property> Properties, string Association);
        private sealed record FileHeader(uint ClockType, long Frequency, uint PointerSize, uint EventsLost, uint BuffersLost, uint BuffersWritten, long StartTime, long EndTime);
        private sealed class Report
        {
            public bool Kernel { get; set; }
            public ulong Window { get; set; }
            public long CompleteExit { get; set; }
            public long RetainedBytes { get; set; }
            public Dictionary<string, int> DescriptorCounts { get; set; } = [];
            public int AssociatedBlit { get; set; }
            public int AssociatedHistoryStart { get; set; }
            public int AssociatedHistoryInfo { get; set; }
            public string KernelNegativeControls { get; set; } = "NotEstablished.NoActualBridge";
            public int SchemaVersion { get; set; } = 3;
            public int TotalEvents { get; set; }
            public string? CleanupError { get; set; }
            public string? ReportWriteError { get; set; }
            public string Evidence { get; set; } = "OfflineRuntimeBoundaryOnly.NotKernelSubmission.NotDwm.NotScanout.NotPixelProof";
            public string Input { get; set; } = "inputs/original-present.etl";
            public long Bytes { get; set; }
            public string Sha256 { get; set; } = "";
            public string TestSha256 { get; set; } = "";
            public Guid Mvid { get; set; }
            public string Runtime { get; set; } = RuntimeInformation.FrameworkDescription;
            public string Architecture { get; set; } = RuntimeInformation.ProcessArchitecture.ToString();
            public int PointerSize { get; set; } = IntPtr.Size;
            public uint OriginalPid { get; set; }
            public uint OriginalTid { get; set; }
            public long OriginalBefore { get; set; }
            public long OriginalAfter { get; set; }
            public long OriginalFrequency { get; set; }
            public int OriginalResult { get; set; }
            public FileHeader? Header { get; set; }
            public bool ClockComparable { get; set; }
            public int? OpenError { get; set; }
            public uint? ProcessStatus { get; set; }
            public uint? CloseStatus { get; set; }
            public int CloseCalls { get; set; }
            public uint ConsumerEventsLost { get; set; }
            public uint BuffersRead { get; set; }
            public int CallbackThread { get; set; }
            public int ProcessingThread { get; set; }
            public int TdhSchemaErrors { get; set; }
            public int TdhPropertyErrors { get; set; }
            public int UnknownProperties { get; set; }
            public int UnknownDescriptors { get; set; }
            public int D3D9Events { get; set; }
            public int AssociatedStartStop { get; set; }
            public bool NegativeControlsPassed { get; set; }
            public string Conclusion { get; set; } = "Unknown";
            public string? Error { get; set; }
            public List<Event> Events { get; set; } = [];
        }

        internal static void Decode(bool kernel)
        {
            string root = Path.Combine(AppContext.BaseDirectory, "Results", kernel ? "dxgkrnl-offline" : "original-present-offline-tdh");
            Directory.CreateDirectory(root);
            Report report = new() { Kernel = kernel };
            if (kernel) report.Evidence = "BoundedKernelInitiationOrHistoryOnly.NotGpuCompletion.NotDwm.NotScanout.NotRootCause.NotProductPass";
            ExceptionDispatchInfo? failure = null;
            ExceptionDispatchInfo? cleanupFailure = null;
            ExceptionDispatchInfo? writeFailure = null;
            ulong handle = ulong.MaxValue;
            nint fileName = 0;
            RecordCallback? callback = null;
            Logfile logfile = default;
            try
            {
                if (IntPtr.Size != 4 || Marshal.SizeOf<Logfile>() != 416 || Marshal.SizeOf<Header>() != 272 || Marshal.SizeOf<Record>() != 104 || Marshal.SizeOf<EventHeader>() != 80 || Marshal.SizeOf<NativeDescriptor>() != 16 || Marshal.SizeOf<PropertyDescriptor>() != 16)
                    throw new InvalidOperationException("OfflineTdh.Layout");
                string input = Path.Combine(AppContext.BaseDirectory, report.Input);
                using JsonDocument controller = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "inputs", "controller.json")));
                JsonElement original = controller.RootElement;
                report.OriginalPid = original.GetProperty("ProcessId").GetUInt32();
                report.OriginalTid = original.GetProperty("ThreadId").GetUInt32();
                report.OriginalBefore = original.GetProperty("PresentBeforeQpc").GetInt64();
                report.OriginalAfter = original.GetProperty("PresentAfterQpc").GetInt64();
                report.OriginalFrequency = original.GetProperty("QpcFrequency").GetInt64();
                report.OriginalResult = original.GetProperty("PresentResult").GetInt32();
                report.Bytes = new FileInfo(input).Length;
                using (FileStream stream = File.OpenRead(input)) report.Sha256 = Convert.ToHexString(SHA256.HashData(stream));
                if (kernel)
                {
                    report.Window = ulong.Parse(original.GetProperty("Window").GetString()![2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    report.CompleteExit = original.GetProperty("CompleteExitQpc").GetInt64();
                    if (report.Bytes != 327680 || report.Sha256 != "045B1CD110EBFB3A7945DF24633BE97954007C7AE5F590F868B8FC8D38DD7CB3" ||
                        report.Bytes != original.GetProperty("EtlBytes").GetInt64() || report.Sha256 != original.GetProperty("EtlSha256").GetString() ||
                        report.OriginalPid != 5836 || report.OriginalTid != 11732 || report.OriginalBefore != 39957016165426 || report.OriginalAfter != 39957016171383 ||
                        report.Window != 0x181049C || report.OriginalResult != 0 || report.OriginalFrequency != 10000000 ||
                        !original.GetProperty("CompleteReturned").GetBoolean() || original.GetProperty("TruncatedBeforeComplete").GetBoolean() ||
                        original.GetProperty("StopStatus").GetUInt32() != 0 || original.GetProperty("StopReason").GetString() != "CompleteReturned" ||
                        report.CompleteExit >= original.GetProperty("StopRequestedQpc").GetInt64())
                        throw new InvalidDataException("OfflineTdh.KernelInputIdentity");
                }
                else if (report.Bytes != 131072 || report.Sha256 != "4085C964310A28A8FCEB686326F401B4405F8B8512C408307B89F81F050214E6" ||
                    report.Bytes != original.GetProperty("EtlBytes").GetInt64() || report.Sha256 != original.GetProperty("EtlSha256").GetString() ||
                    report.OriginalPid != 2748 || report.OriginalTid != 4736 || report.OriginalFrequency != 10000000 ||
                    report.OriginalBefore != 39911863244854 || report.OriginalAfter != 39911863255850 || report.OriginalResult != 0)
                    throw new InvalidDataException("OfflineTdh.InputIdentity");
                string assembly = typeof(D3DImageTests).Assembly.Location;
                using (FileStream stream = File.OpenRead(assembly)) report.TestSha256 = Convert.ToHexString(SHA256.HashData(stream));
                report.Mvid = typeof(D3DImageTests).Module.ModuleVersionId;
                report.NegativeControlsPassed = NegativeControls(report);
                if (!report.NegativeControlsPassed) throw new InvalidDataException("OfflineTdh.NegativeControls");
                fileName = Marshal.StringToHGlobalUni(input);
                report.ProcessingThread = Environment.CurrentManagedThreadId;
                callback = pointer =>
                {
                    if (failure is not null) return;
                    try
                    {
                        report.TotalEvents++;
                        if (report.TotalEvents > 100000) throw new InvalidDataException("OfflineTdh.EventLimit");
                        report.CallbackThread = Environment.CurrentManagedThreadId;
                        if (report.CallbackThread != report.ProcessingThread) throw new InvalidOperationException("OfflineTdh.CallbackThread");
                        ReadEvent(pointer, report);
                    }
                    catch (Exception error)
                    {
                        // No managed exception may unwind through the native callback.
                        failure = ExceptionDispatchInfo.Capture(error);
                    }
                };
                logfile.FileName = fileName;
                logfile.Mode = 0x10001000; // FILE (no REAL_TIME), EVENT_RECORD and RAW_TIMESTAMP.
                logfile.Callback = Marshal.GetFunctionPointerForDelegate(callback);
                handle = OpenTraceW(ref logfile);
                if (handle == ulong.MaxValue)
                {
                    report.OpenError = Marshal.GetLastWin32Error();
                    throw new Win32Exception(report.OpenError.Value);
                }
                Header h = logfile.Header;
                report.Header = new(h.Clock, h.Frequency, h.PointerSize, h.EventsLost, h.BuffersLost, h.BuffersWritten, h.StartTime, h.EndTime);
                report.ClockComparable = h.Clock == 1 && h.Frequency > 0 && h.Frequency == report.OriginalFrequency;
                report.ProcessStatus = ProcessTrace(ref handle, 1, 0, 0);
                failure?.Throw();
                if (report.ProcessStatus != 0) throw new Win32Exception(unchecked((int)report.ProcessStatus.Value));
                report.Conclusion = report.AssociatedStartStop > 0 ? "Observed.SchemaPresentStartStopWithinOriginalCall" : "Unknown.NoAssociatedSchemaPresentStartStop";
                if (kernel) Correlate(report);
            }
            catch (Exception error)
            {
                failure ??= ExceptionDispatchInfo.Capture(error);
                report.Error = failure.SourceException.ToString();
            }
            finally
            {
                try
                {
                    if (handle != ulong.MaxValue)
                    {
                        report.CloseCalls++;
                        report.CloseStatus = CloseTrace(handle);
                        if (report.CloseStatus != 0)
                            cleanupFailure = ExceptionDispatchInfo.Capture(new Win32Exception(unchecked((int)report.CloseStatus.Value)));
                    }
                }
                catch (Exception error)
                {
                    cleanupFailure = ExceptionDispatchInfo.Capture(error);
                }
                finally
                {
                    report.ConsumerEventsLost = logfile.EventsLost;
                    report.BuffersRead = logfile.BuffersRead;
                    GC.KeepAlive(callback);
                    if (fileName != 0) Marshal.FreeHGlobal(fileName);
                }
                report.CleanupError = cleanupFailure?.SourceException.ToString();
                try
                {
                    File.WriteAllText(Path.Combine(root, "decoded.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception writeError)
                {
                    writeFailure = ExceptionDispatchInfo.Capture(writeError);
                    report.ReportWriteError = writeError.ToString();
                }
            }
            if (failure is not null)
            {
                failure.SourceException.Data["OfflineTdh.CleanupError"] = report.CleanupError;
                failure.SourceException.Data["OfflineTdh.ReportWriteError"] = report.ReportWriteError;
                failure.Throw();
            }
            if (cleanupFailure is not null)
            {
                cleanupFailure.SourceException.Data["OfflineTdh.ReportWriteError"] = report.ReportWriteError;
                cleanupFailure.Throw();
            }
            writeFailure?.Throw();
        }

        private static void Reserve(Report report, uint bytes)
        {
            if (bytes > 1048576 || (report.RetainedBytes += bytes) > 16777216)
                throw new InvalidDataException("OfflineTdh.MemoryLimit");
        }

        private static bool Known(Event e)
        {
            Descriptor d = e.Descriptor;
            if (e.SchemaStatus != 0) return false;
            if (e.Provider == D3D9)
                return d.Version == 0 && d.Channel == 16 && d.Level == 0 && d.Task == 1 && d.Keyword == 0x8000000000000002 &&
                    ((d.Id == 1 && d.Opcode == 1) || (d.Id == 2 && d.Opcode == 2)) && e.TaskName == "Present";
            return e.Provider == DxgKrnl && d.Channel == 17 && d.Keyword == 0x4000000008000001 &&
                ((d.Id == 166 && d.Version == 0 && d.Level == 4 && d.Opcode == 0 && d.Task == 103) ||
                 (d.Id == 171 && d.Version == 2 && d.Level == 0 && d.Opcode == 1 && d.Task == 6) ||
                 (d.Id == 172 && d.Version == 2 && d.Level == 0 && d.Opcode == 0 && d.Task == 6));
        }

        private static ulong? Unsigned(Event e, string name, ushort type, uint bytes)
        {
            Property[] fields = e.Properties.Where(p => p.Name == name).ToArray();
            if (fields.Length != 1) return null;
            Property p = fields[0];
            return p.Flags == 0 && p.InType == type && p.SchemaCount == 1 && p.Bytes == bytes && p.State == "Decoded" &&
                ulong.TryParse(p.Value, NumberStyles.None, CultureInfo.InvariantCulture, out ulong value) ? value : null;
        }

        private static bool InFlight(Event e, Event start, Event stop, Report r) =>
            Known(e) && r.ClockComparable && e.HeaderPid == r.OriginalPid && e.HeaderTid == r.OriginalTid &&
            e.RawTimestamp >= start.RawTimestamp && e.RawTimestamp <= stop.RawTimestamp &&
            e.RawTimestamp >= r.OriginalBefore && e.RawTimestamp <= r.OriginalAfter;

        private static bool BlitMatch(Event e, Event start, Event stop, Report r) =>
            e.Provider == DxgKrnl && e.Descriptor.Id == 166 && InFlight(e, start, stop, r) && Unsigned(e, "hwnd", 10, 8) == r.Window;

        private static bool HistoryMatch(Event e, Event start, Event stop, Report r) =>
            e.Provider == DxgKrnl && e.Descriptor.Id == 171 && InFlight(e, start, stop, r) && Unsigned(e, "Token", 10, 8) is > 0;

        private static bool InfoMatch(Event e, Event history, Report r) =>
            Known(e) && e.Provider == history.Provider && e.Descriptor.Id == 172 &&
            e.RawTimestamp >= history.RawTimestamp && e.RawTimestamp <= r.CompleteExit &&
            Unsigned(history, "Token", 10, 8) is > 0 && Unsigned(e, "Token", 10, 8) == Unsigned(history, "Token", 10, 8);

        private static Event ChangedField(Event e, string name, ulong value) =>
            e with { Properties = e.Properties.Select(p => p.Name == name ? p with { Value = value.ToString(CultureInfo.InvariantCulture) } : p).ToList() };

        private static void Correlate(Report r)
        {
            r.AssociatedStartStop = 0;
            r.Events = r.Events.Select(e => e with { Association = Known(e) ? "Uncorrelated.NoExactBridge" : "Unknown.DescriptorVersionOrSchema.RawPreserved" }).ToList();
            Event[] starts = r.Events.Where(e => e.Provider == D3D9 && e.Descriptor.Id == 1 && Matches(e.HeaderPid, e.HeaderTid, e.RawTimestamp, Known(e), r.ClockComparable, r)).ToArray();
            Event[] stops = r.Events.Where(e => e.Provider == D3D9 && e.Descriptor.Id == 2 && Matches(e.HeaderPid, e.HeaderTid, e.RawTimestamp, Known(e), r.ClockComparable, r)).ToArray();
            r.Conclusion = "Unknown.NoUniqueRuntimePair";
            if (starts.Length != 1 || stops.Length != 1 || starts[0].RawTimestamp > stops[0].RawTimestamp) return;
            Event start = starts[0], stop = stops[0];
            r.AssociatedStartStop = 2;
            List<Event> blits = r.Events.Where(e => BlitMatch(e, start, stop, r)).ToList();
            List<Event> histories = r.Events.Where(e => HistoryMatch(e, start, stop, r)).ToList();
            Event? history = histories.Count == 1 ? histories[0] : null;
            // A reused token anywhere in this trace is not a unique bridge.
            if (history is not null && r.Events.Count(e => e.Provider == DxgKrnl && e.Descriptor.Id == 171 && Unsigned(e, "Token", 10, 8) == Unsigned(history, "Token", 10, 8)) != 1) history = null;
            List<Event> infos = history is null ? [] : r.Events.Where(e => InfoMatch(e, history, r)).ToList();
            r.AssociatedBlit = blits.Count;
            r.AssociatedHistoryStart = history is null ? 0 : 1;
            r.AssociatedHistoryInfo = infos.Count;
            List<string> controls = [];
            foreach (Event e in blits)
            {
                bool passed = !BlitMatch(e with { HeaderPid = e.HeaderPid + 1 }, start, stop, r) &&
                    !BlitMatch(e with { HeaderTid = e.HeaderTid + 1 }, start, stop, r) &&
                    !BlitMatch(e with { RawTimestamp = r.OriginalBefore - 1 }, start, stop, r) &&
                    !BlitMatch(ChangedField(e, "hwnd", r.Window + 1), start, stop, r) &&
                    !BlitMatch(e with { Descriptor = e.Descriptor with { Version = 255 } }, start, stop, r);
                if (!passed) throw new InvalidDataException("OfflineTdh.BlitNegativeControls");
                controls.Add("ActualBlit.PidTidHwndQpcVersion.Rejected");
            }
            if (history is not null)
            {
                bool passed = !HistoryMatch(history with { HeaderPid = history.HeaderPid + 1 }, start, stop, r) &&
                    !HistoryMatch(history with { HeaderTid = history.HeaderTid + 1 }, start, stop, r) &&
                    !HistoryMatch(history with { RawTimestamp = r.OriginalAfter + 1 }, start, stop, r) &&
                    !HistoryMatch(ChangedField(history, "Token", 0), start, stop, r) &&
                    !HistoryMatch(history with { Descriptor = history.Descriptor with { Version = 255 } }, start, stop, r);
                if (!passed) throw new InvalidDataException("OfflineTdh.HistoryNegativeControls");
                controls.Add("ActualHistoryStart.PidTidQpcZeroTokenVersion.Rejected");
                foreach (Event e in infos)
                {
                    if (InfoMatch(ChangedField(e, "Token", 0), history, r) ||
                        InfoMatch(e with { RawTimestamp = history.RawTimestamp - 1 }, history, r) ||
                        InfoMatch(e with { Descriptor = e.Descriptor with { Version = 255 } }, history, r))
                        throw new InvalidDataException("OfflineTdh.InfoNegativeControls");
                    controls.Add("ActualHistoryInfo.TokenQpcVersion.Rejected.SystemPidAllowed");
                }
            }
            r.KernelNegativeControls = controls.Count == 0 ? "NotEstablished.NoActualBridge" : string.Join(";", controls);
            r.Events = r.Events.Select(e => e with { Association = e == start || e == stop ? "Matched.UniqueRuntimePair" :
                blits.Contains(e) ? "Matched.ExactHwndKernelBlitInitiationOnly" : e == history ? "Matched.UniqueHistoryTokenStartOnly" :
                infos.Contains(e) ? "Matched.SameProviderSemanticTokenHandoffOnly" : e.Association }).ToList();
            r.Conclusion = $"RuntimePair.Blits={r.AssociatedBlit}.HistoryStarts={r.AssociatedHistoryStart}.HistoryInfos={r.AssociatedHistoryInfo}.NoCompletionOrDisplayOrRootCauseProof";
        }

        private static bool Matches(uint pid, uint tid, long timestamp, bool known, bool comparable, Report r) =>
            known && comparable && pid == r.OriginalPid && tid == r.OriginalTid && timestamp >= r.OriginalBefore && timestamp <= r.OriginalAfter;

        private static bool NegativeControls(Report r) =>
            Matches(r.OriginalPid, r.OriginalTid, r.OriginalBefore, true, true, r) &&
            !Matches(r.OriginalPid + 1, r.OriginalTid, r.OriginalBefore, true, true, r) &&
            !Matches(r.OriginalPid, r.OriginalTid + 1, r.OriginalBefore, true, true, r) &&
            !Matches(r.OriginalPid, r.OriginalTid, r.OriginalBefore - 1, true, true, r) &&
            !Matches(r.OriginalPid, r.OriginalTid, r.OriginalAfter + 1, true, true, r) &&
            !Matches(r.OriginalPid, r.OriginalTid, r.OriginalBefore, false, true, r) &&
            !Matches(r.OriginalPid, r.OriginalTid, r.OriginalBefore, true, false, r);

        private static void ReadEvent(nint pointer, Report report)
        {
            Record record = Marshal.PtrToStructure<Record>(pointer);
            EventHeader header = record.Header;
            NativeDescriptor d = header.Descriptor;
            string key = $"{header.Provider}/{d.Id}/{d.Version}/{d.Channel}/{d.Level}/{d.Opcode}/{d.Task}/{d.Keyword:X16}";
            report.DescriptorCounts[key] = report.DescriptorCounts.GetValueOrDefault(key) + 1;
            if (report.Kernel && !(header.Provider == D3D9 && d.Id is 1 or 2) && !(header.Provider == DxgKrnl && d.Id is 166 or 171 or 172)) return;
            if (report.Events.Count >= 4096) throw new InvalidDataException("OfflineTdh.TargetLimit");
            List<Property> properties = [];
            uint bytes = 0;
            uint status = TdhGetEventInformation(pointer, 0, 0, 0, ref bytes);
            string? task = null, opcode = null, name = null;
            uint? source = null;
            string schemaHex = "";
            Reserve(report, record.UserDataLength);
            string payloadHex = record.UserDataLength == 0 ? "" : Convert.ToHexString(new ReadOnlySpan<byte>((void*)record.UserData, record.UserDataLength));
            if (status == 122 && bytes >= 112)
            {
                Reserve(report, bytes);
                uint capacity = bytes;
                nint schema = Marshal.AllocHGlobal(checked((int)capacity));
                try
                {
                    status = TdhGetEventInformation(pointer, 0, 0, schema, ref bytes);
                    if (status == 0)
                    {
                        if (bytes < 112 || bytes > capacity) throw new InvalidDataException("OfflineTdh.SchemaSize");
                        // SDK TRACE_EVENT_INFO has a fixed 112-byte prefix, followed by 24-byte EVENT_PROPERTY_INFO entries.
                        schemaHex = Convert.ToHexString(new ReadOnlySpan<byte>((void*)schema, checked((int)bytes)));
                        source = U32(schema, 48);
                        task = Text(schema, bytes, U32(schema, 68));
                        opcode = Text(schema, bytes, U32(schema, 72));
                        name = source is 0 or 3 ? Text(schema, bytes, U32(schema, 92)) : null;
                        uint count = U32(schema, 100), top = U32(schema, 104);
                        if (top > count || 112UL + count * 24UL > bytes) throw new InvalidDataException("OfflineTdh.SchemaBounds");
                        for (uint index = 0; index < count; index++)
                        {
                            int offset = checked(112 + (int)index * 24);
                            uint flags = U32(schema, offset);
                            string propertyName = Text(schema, bytes, U32(schema, offset + 4)) ?? "";
                            ushort inType = U16(schema, offset + 8), outType = U16(schema, offset + 10);
                            ushort schemaCount = U16(schema, offset + 16), length = U16(schema, offset + 18);
                            if (index >= top || (flags & 1) != 0 || (flags & 128) != 0)
                            {
                                properties.Add(new(propertyName, flags, inType, outType, schemaCount, length, null, null, null, null, null, "Unknown.StructNestedOrCustom"));
                                report.UnknownProperties++;
                                continue;
                            }
                            ReadProperty(pointer, propertyName, flags, inType, outType, schemaCount, length, properties, report);
                        }
                    }
                }
                finally { Marshal.FreeHGlobal(schema); }
            }
            if (status != 0) report.TdhSchemaErrors++;
            bool present = header.Provider == D3D9 && status == 0 &&
                (d.Opcode == 1 || d.Opcode == 2) &&
                (string.Equals(task, "Present", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(name, d.Opcode == 1 ? "Present_Start" : "Present_Stop", StringComparison.OrdinalIgnoreCase));
            if (header.Provider == D3D9) report.D3D9Events++;
            if (!present) report.UnknownDescriptors++;
            bool match = Matches(header.Pid, header.Tid, header.Timestamp, present, report.ClockComparable, report);
            if (match) report.AssociatedStartStop++;
            report.Events.Add(new(header.Provider, new(d.Id, d.Version, d.Channel, d.Level, d.Opcode, d.Task, d.Keyword), schemaHex, payloadHex, header.Pid, header.Tid, header.Timestamp, header.Flags, record.UserDataLength, status, task, opcode, name, source, properties,
                match ? "Matched.RuntimeBoundaryOnly" : !present ? "UnknownDescriptor" : !report.ClockComparable ? "UnknownClock" : "NotMatched.PidTidOrQpc"));
        }

        private static void ReadProperty(nint record, string name, uint flags, ushort input, ushort output, ushort count, ushort length, List<Property> properties, Report report)
        {
            nint propertyName = Marshal.StringToHGlobalUni(name);
            try
            {
                PropertyDescriptor descriptor = new() { Name = unchecked((ulong)(nuint)propertyName), ArrayIndex = uint.MaxValue };
                uint sizeStatus = TdhGetPropertySize(record, 0, 0, 1, ref descriptor, out uint size);
                uint? valueStatus = null;
                string? value = null, hex = null;
                if (sizeStatus == 0)
                {
                    Reserve(report, size);
                    byte[] data = new byte[checked((int)size)];
                    fixed (byte* buffer = data)
                    {
                        valueStatus = TdhGetProperty(record, 0, 0, 1, ref descriptor, size, (nint)buffer);
                    }
                    if (valueStatus == 0)
                    {
                        hex = Convert.ToHexString(data);
                        value = (flags & (2u | 4u)) == 0 && count == 1 ? Scalar(input, data) : null;
                    }
                }
                bool error = sizeStatus != 0 || valueStatus != 0;
                if (error) report.TdhPropertyErrors++;
                if (value is null) report.UnknownProperties++;
                properties.Add(new(name, flags, input, output, count, length, sizeStatus == 0 ? size : null, value, hex, sizeStatus, valueStatus, error ? "TdhError" : value is null ? "UnknownTypeOrArray.RawHexPreserved" : "Decoded"));
            }
            finally { Marshal.FreeHGlobal(propertyName); }
        }

        private static string? Scalar(ushort type, byte[] data)
        {
            return (type, data.Length) switch
            {
                (1, _) when data.Length % 2 == 0 => System.Text.Encoding.Unicode.GetString(data).TrimEnd('\0'),
                (2, _) => System.Text.Encoding.Latin1.GetString(data).TrimEnd('\0'),
                (3, 1) => unchecked((sbyte)data[0]).ToString(CultureInfo.InvariantCulture),
                (4, 1) => data[0].ToString(CultureInfo.InvariantCulture),
                (5, 2) => BitConverter.ToInt16(data).ToString(CultureInfo.InvariantCulture),
                (6, 2) => BitConverter.ToUInt16(data).ToString(CultureInfo.InvariantCulture),
                (7, 4) => BitConverter.ToInt32(data).ToString(CultureInfo.InvariantCulture),
                (8 or 13 or 20, 4) => BitConverter.ToUInt32(data).ToString(CultureInfo.InvariantCulture),
                (9, 8) => BitConverter.ToInt64(data).ToString(CultureInfo.InvariantCulture),
                (10 or 21, 8) => BitConverter.ToUInt64(data).ToString(CultureInfo.InvariantCulture),
                (15, 16) => new Guid(data).ToString(),
                (16, 4) => "0x" + BitConverter.ToUInt32(data).ToString("X8", CultureInfo.InvariantCulture),
                (16, 8) => "0x" + BitConverter.ToUInt64(data).ToString("X16", CultureInfo.InvariantCulture),
                _ => null
            };
        }

        private static uint U32(nint pointer, int offset) => unchecked((uint)Marshal.ReadInt32(pointer, offset));
        private static ushort U16(nint pointer, int offset) => unchecked((ushort)Marshal.ReadInt16(pointer, offset));
        private static string? Text(nint pointer, uint bytes, uint offset)
        {
            if (offset == 0) return null;
            if ((offset & 1) != 0 || offset >= bytes) throw new InvalidDataException("OfflineTdh.StringBounds");
            int start = checked((int)offset), end = start;
            while ((ulong)end + 2 <= bytes && Marshal.ReadInt16(pointer, end) != 0) end = checked(end + 2);
            if ((ulong)end + 2 > bytes) throw new InvalidDataException("OfflineTdh.StringTerminator");
            return Marshal.PtrToStringUni(pointer + start, (end - start) / 2);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeDescriptor { internal ushort Id; internal byte Version, Channel, Level, Opcode; internal ushort Task; internal ulong Keyword; }
        [StructLayout(LayoutKind.Sequential)]
        private struct EventHeader
        {
            internal ushort Size, HeaderType, Flags, Property;
            internal uint Tid, Pid;
            internal long Timestamp;
            internal Guid Provider;
            internal NativeDescriptor Descriptor;
            internal ulong ProcessorTime;
            internal Guid Activity;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct Record
        {
            internal EventHeader Header;
            internal uint BufferContext;
            internal ushort ExtendedCount, UserDataLength;
            internal nint Extended, UserData, Context;
        }
        [StructLayout(LayoutKind.Explicit, Size = 272)]
        private struct Header
        {
            [FieldOffset(16)] internal long EndTime;
            [FieldOffset(36)] internal uint BuffersWritten;
            [FieldOffset(44)] internal uint PointerSize;
            [FieldOffset(48)] internal uint EventsLost;
            [FieldOffset(248)] internal long Frequency;
            [FieldOffset(256)] internal long StartTime;
            [FieldOffset(264)] internal uint Clock;
            [FieldOffset(268)] internal uint BuffersLost;
        }
        [StructLayout(LayoutKind.Explicit, Size = 416)]
        private struct Logfile
        {
            [FieldOffset(0)] internal nint FileName;
            [FieldOffset(16)] internal uint BuffersRead;
            [FieldOffset(20)] internal uint Mode;
            [FieldOffset(112)] internal Header Header;
            [FieldOffset(396)] internal uint EventsLost;
            [FieldOffset(400)] internal nint Callback;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct PropertyDescriptor { internal ulong Name; internal uint ArrayIndex, Reserved; }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate void RecordCallback(nint record);
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern ulong OpenTraceW(ref Logfile logfile);
        [DllImport("advapi32.dll", ExactSpelling = true)]
        private static extern uint ProcessTrace(ref ulong handle, uint count, nint start, nint end);
        [DllImport("advapi32.dll", ExactSpelling = true)]
        private static extern uint CloseTrace(ulong handle);
        [DllImport("tdh.dll", ExactSpelling = true)]
        private static extern uint TdhGetEventInformation(nint record, uint count, nint context, nint buffer, ref uint bytes);
        [DllImport("tdh.dll", ExactSpelling = true)]
        private static extern uint TdhGetPropertySize(nint record, uint count, nint context, uint descriptorCount, ref PropertyDescriptor descriptor, out uint bytes);
        [DllImport("tdh.dll", ExactSpelling = true)]
        private static extern uint TdhGetProperty(nint record, uint count, nint context, uint descriptorCount, ref PropertyDescriptor descriptor, uint bytes, nint buffer);
    }
}
#endif
