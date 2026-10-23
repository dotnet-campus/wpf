#if FH_CRASH_EVIDENCE
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Reflection.Metadata;
#if GX_GETDIRECT3D_STACK
using System.Reflection.Metadata.Ecma335;
#endif
using System.Text;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

// Explicit x86-only debugger role; the ordinary test runner and supervisor never enter it.
internal static unsafe class FhCrashEvidence
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct Startup
    {
        internal uint Size;
        internal nint Reserved, Desktop, Title;
        internal uint X, Y, Width, Height, XChars, YChars, Fill, Flags;
        internal ushort Show, ReservedSize;
        internal nint ReservedBytes, Input, Output, Error;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        internal nint Process, Thread;
        internal uint ProcessId, ThreadId;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateProcessW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateProcess(string application, char* command, nint processSecurity,
        nint threadSecurity, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint flags, nint environment,
        string directory, ref Startup startup, out ProcessInformation information);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WaitForDebugEvent(byte* debugEvent, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ContinueDebugEvent(uint process, uint thread, uint status);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DebugSetProcessKillOnExit([MarshalAs(UnmanagedType.Bool)] bool kill);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool TerminateProcess(nint process, uint code);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint GetFileType(nint file);
    [StructLayout(LayoutKind.Sequential)]
    private struct FileIdentity
    {
        internal uint Attributes, CreationLow, CreationHigh, AccessLow, AccessHigh, WriteLow, WriteHigh;
        internal uint Volume, SizeHigh, SizeLow, Links, IndexHigh, IndexLow;
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(nint file, out FileIdentity identity);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFilePointerEx(nint file, long distance, out long position, uint method);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReadFile(nint file, byte* buffer, uint count, out uint read, nint overlapped);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateHandle(nint sourceProcess, nint source, nint targetProcess, out nint target, uint access,
        [MarshalAs(UnmanagedType.Bool)] bool inherit, uint options);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(nint handle, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DebugActiveProcessStop(uint processId);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetModuleHandleW")]
    private static extern nint GetModuleHandle(string name);
    [DllImport("kernel32.dll", EntryPoint = "OutputDebugStringA", CharSet = CharSet.Ansi)]
    private static extern void OutputDebugString(string text);
    [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandle(nint handle, char* path, uint capacity, uint flags);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ReadProcessMemory(nint process, nint address, byte* buffer, nuint bytes, out nuint read);

    // winnt.h i386 CONTEXT: FLOATING_SAVE_AREA is 112 bytes; ExtendedRegisters is 512 bytes.
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct FloatingSaveArea
    {
        internal uint ControlWord, StatusWord, TagWord, ErrorOffset, ErrorSelector, DataOffset, DataSelector;
        internal fixed byte RegisterArea[80];
        internal uint Cr0NpxState;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct X86Context
    {
        internal uint ContextFlags, Dr0, Dr1, Dr2, Dr3, Dr6, Dr7;
        internal FloatingSaveArea FloatSave;
        internal uint SegGs, SegFs, SegEs, SegDs;
        internal uint Edi, Esi, Ebx, Edx, Ecx, Eax;
        internal uint Ebp, Eip, SegCs, EFlags, Esp, SegSs;
        internal fixed byte ExtendedRegisters[512];
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern nint OpenThread(uint access, [MarshalAs(UnmanagedType.Bool)] bool inherit, uint threadId);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetThreadContext(nint thread, X86Context* context);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetThreadContext(nint thread, X86Context* context);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SuspendThread(nint thread);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint ResumeThread(nint thread);
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentProcessId();
    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    private sealed class HardwareSample(nint process, uint pid, string root)
    {
        internal uint Tid, Before, After, InitialEsp, InitialEbp;
        internal int Phase;
        internal bool Owned, Paired;
        private uint[] points = [];
        private uint frame;
        private bool calleeFrames;
        private bool writeWatch;
        private uint previousValue;
        private int writeEvents;
        private bool writeControl;
#if GX_GETDIRECT3D_STACK
        private int gxPoint;
        private uint gxBeforeEsp, gxReturnEsp, gxThis, gxOutput, gxTarget;

        private void CaptureGetDirect3D(X86Context* context)
        {
            if (context->Ebp != frame) throw new InvalidOperationException("GX fixed F changed.");
            if (gxPoint == 0)
            {
                gxBeforeEsp = context->Esp;
                gxThis = Pointer("getdirect3d-this", context->Esp) ?? throw new InvalidDataException();
                gxOutput = Pointer("getdirect3d-out", checked(context->Esp + 4)) ?? throw new InvalidDataException();
                uint table = Pointer("getdirect3d-vtable", gxThis) ?? throw new InvalidDataException();
                gxTarget = Pointer("getdirect3d-slot6", checked(table + 24)) ?? throw new InvalidDataException();
                if (gxTarget != context->Edi || gxThis != context->Esi || gxOutput != frame - 0x10)
                    throw new InvalidOperationException("GX slot6 this/out/target authentication rejected.");
            }
            if (gxPoint == 1) gxReturnEsp = context->Esp;
            var module = Modules.Where(m => gxTarget >= m.Key && (ulong)gxTarget < (ulong)m.Key + m.Value.Size).ToArray();
            Log("gx-getdirect3d-point", new { Point = gxPoint + 1, FixedFrame = frame, InitialEsp, InitialEbp,
                BeforeEsp = gxBeforeEsp, ReturnEsp = gxReturnEsp,
                DeltaFromCall = (long)context->Esp - gxBeforeEsp,
                DeltaFromReturn = gxPoint == 2 ? (long)context->Esp - gxReturnEsp : 0,
                This = gxThis, Output = gxOutput, Target = gxTarget,
                Modules = module.Select(m => new { Base = m.Key, m.Value.Size, m.Value.Path,
                    Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(m.Value.Path))) }).ToArray(),
                Reads = new[] { Read("ESP-minus16-plus32", checked(context->Esp - 16), 48),
                    Read("fixed-F-through-plus8", frame, 8), Read("caller-return-slot", checked(frame + 4), 4),
                    Read("this", gxThis, 4), Read("out-local", gxOutput, 4), Read("slot6-entry", gxTarget, 32),
                    Read("point-instruction", context->Eip, 16) }, FullStackProven = false }, context);
        }
#endif
#if GU_HELPER_STACK
        private uint helperBeforeEsp;
        private bool helperBeforeCaptured;
#endif
        internal Dictionary<uint, (uint Size, string Path)> Modules = [];

        private uint? Pointer(string label, uint address)
        {
            byte[] bytes = new byte[4];
            bool ok; nuint count;
            fixed (byte* buffer = bytes) ok = ReadProcessMemory(process, (nint)address, buffer, 4, out count);
            int error = ok ? 0 : Marshal.GetLastPInvokeError();
            Log("qi-pointer-read", new { Label = label, Address = address, Requested = 4, Succeeded = ok,
                Error = error, BytesRead = (ulong)count, Hex = Convert.ToHexString(bytes.AsSpan(0, (int)Math.Min(count, 4))) });
            return ok && count == 4 ? BitConverter.ToUInt32(bytes) : null;
        }

        private void CaptureQi(X86Context* context)
        {
            var reads = new List<object> { Read("actual-call-stack", context->Esp, 32),
#if GU_HELPER_STACK
                Read("fixed-F-minus60-through-plus8", checked(frame - 0x60), 0x68),
#else
                Read("fixed-F-minus40-through-plus8", checked(frame - 0x40), 0x48),
#endif
                Read("actual-EDI-entry", context->Edi, 32) };
            uint? actualThis = Pointer("stack-this", context->Esp);
            uint? riid = Pointer("stack-riid", checked(context->Esp + 4));
            uint? output = Pointer("stack-out", checked(context->Esp + 8));
            uint? factory = Pointer("static-factory-local", checked(frame - 0x10));
            uint? table = actualThis is > 0 ? Pointer("actual-this-vtable", actualThis.Value) : null;
            uint? slot = table is > 0 ? Pointer("actual-vtable-slot0", table.Value) : null;
            uint? factoryTable = factory is > 0 ? Pointer("factory-vtable", factory.Value) : null;
            uint? factorySlot = factoryTable is > 0 ? Pointer("factory-slot0", factoryTable.Value) : null;
            if (riid is > 0) reads.Add(Read("actual-riid-GUID16", riid.Value, 16));
            if (output is > 0) reads.Add(Read("actual-out-before-DWORD", output.Value, 4));
            var module = Modules.Where(m => context->Edi >= m.Key && (ulong)context->Edi < (ulong)m.Key + m.Value.Size).ToArray();
#if GU_HELPER_STACK
            Log(helperBeforeCaptured ? "helper-return" : "helper-call-before", new { HelperBeforeEsp = helperBeforeEsp,
                EspDelta = helperBeforeCaptured ? (long)context->Esp - helperBeforeEsp : 0,
#else
            Log("factory-qi-before-call", new {
#endif
                FixedFrame = frame, InitialEbp, InitialEsp,
                ActualThis = actualThis, Riid = riid, Output = output, Factory = factory,
                Vtable = table, Slot0 = slot, FactoryVtable = factoryTable, FactorySlot0 = factorySlot,
                ExpectedRiid = frame - 0x24, ExpectedOutput = frame - 0x14,
                OutputEqualsF = output == frame, SavedP = Pointer("fixed-savedP", frame),
                Modules = module.Select(m => new { Base = m.Key, m.Value.Size, m.Value.Path,
                    Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(m.Value.Path))) }).ToArray(),
                Reads = reads, FullStackProven = false }, context);
        }

        private uint ReadDword(uint address)
        {
            uint value = 0;
            if (!ReadProcessMemory(process, (nint)address, (byte*)&value, 4, out nuint count) || count != 4)
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            return value;
        }

        internal void ArmWriteControl(uint tid, uint address)
        {
            if (Phase != 0 || Owned || tid == 0 || address == 0 || (address & 3) != 0)
                throw new InvalidOperationException("Write control address/thread/phase rejected.");
            Tid = tid; frame = address; previousValue = ReadDword(frame);
            if (previousValue != 0x12345678) throw new InvalidOperationException("Write control initial DWORD rejected.");
            points = [address]; writeControl = true; writeWatch = true;
            Context((thread, pointer) =>
            {
                X86Context* context = (X86Context*)pointer;
                if (context->Dr0 != 0 || context->Dr1 != 0 || context->Dr2 != 0 || context->Dr3 != 0 ||
                    (context->Dr7 & 0xFFFF23FFu) != 0 || (context->Dr6 & 0xE00Fu) != 0 || (context->EFlags & 0x100) != 0)
                    throw new InvalidOperationException("Write control requires unowned clean debug registers.");
                Owned = true;
                Write(thread, context, frame, 0, (1u << 0) | (1u << 16) | (3u << 18));
                Phase = 1;
                Log("write4-armed", new { FixedFrame = frame, InitialValue = previousValue,
                    LocalSlot = 0, RW = 1, LEN = 3, Bytes = 4,
                    OtherThreadsExcluded = false, MaximumEvents = 16 }, context);
            }, false);
        }
        internal bool Complete => Phase == points.Length + 1 && !Owned && (After == 0 || Paired);
        private readonly string trace = Path.Combine(root, "hardware.jsonl");

        internal void Log(string stage, object data, X86Context* context = null)
        {
            using var buffer = new MemoryStream();
            using (var json = new Utf8JsonWriter(buffer))
            {
                json.WriteStartObject();
                json.WriteString("Stage", stage);
                json.WriteNumber("Pid", pid); json.WriteNumber("Tid", Tid); json.WriteNumber("Phase", Phase);
                json.WritePropertyName("Data"); JsonSerializer.Serialize(json, data);
                if (context != null)
                {
                    json.WriteStartObject("Context");
                    json.WriteNumber("ContextFlags", context->ContextFlags);
                    json.WriteNumber("EIP", context->Eip); json.WriteNumber("ESP", context->Esp);
                    json.WriteNumber("EBP", context->Ebp); json.WriteNumber("EFLAGS", context->EFlags);
                    json.WriteNumber("EAX", context->Eax); json.WriteNumber("EBX", context->Ebx);
                    json.WriteNumber("ECX", context->Ecx); json.WriteNumber("EDX", context->Edx);
                    json.WriteNumber("ESI", context->Esi); json.WriteNumber("EDI", context->Edi);
                    json.WriteNumber("DR0", context->Dr0); json.WriteNumber("DR1", context->Dr1);
                    json.WriteNumber("DR2", context->Dr2); json.WriteNumber("DR3", context->Dr3);
                    json.WriteNumber("DR6", context->Dr6); json.WriteNumber("DR7", context->Dr7);
                    json.WriteEndObject();
                }
                json.WriteEndObject();
            }
            File.AppendAllText(trace, Encoding.UTF8.GetString(buffer.ToArray()) + Environment.NewLine);
        }

        private void Raw(string stage, X86Context* context, bool succeeded, int error)
        {
            using var stream = new MemoryStream();
            using (var json = new Utf8JsonWriter(stream))
            {
                json.WriteStartObject();
                json.WriteString("Stage", stage);
                json.WriteNumber("Pid", pid); json.WriteNumber("Tid", Tid); json.WriteNumber("Phase", Phase);
                json.WriteBoolean("Succeeded", succeeded); json.WriteNumber("Error", error);
                json.WriteNumber("ContextFlags", context->ContextFlags);
                json.WriteNumber("EIP", context->Eip); json.WriteNumber("ESP", context->Esp);
                json.WriteNumber("EBP", context->Ebp); json.WriteNumber("EFLAGS", context->EFlags);
                json.WriteNumber("DR0", context->Dr0); json.WriteNumber("DR1", context->Dr1);
                json.WriteNumber("DR2", context->Dr2); json.WriteNumber("DR3", context->Dr3);
                json.WriteNumber("DR6", context->Dr6); json.WriteNumber("DR7", context->Dr7);
                json.WriteEndObject();
            }
            File.AppendAllText(trace, Encoding.UTF8.GetString(stream.ToArray()) + Environment.NewLine);
        }

        private static void Layout()
        {
            if (IntPtr.Size != 4 || sizeof(X86Context) != 716 || sizeof(FloatingSaveArea) != 112 ||
                Marshal.OffsetOf<X86Context>(nameof(X86Context.Dr0)).ToInt32() != 4 ||
                Marshal.OffsetOf<X86Context>(nameof(X86Context.Dr7)).ToInt32() != 24 ||
                Marshal.OffsetOf<X86Context>(nameof(X86Context.Ebp)).ToInt32() != 180 ||
                Marshal.OffsetOf<X86Context>(nameof(X86Context.Eip)).ToInt32() != 184 ||
                Marshal.OffsetOf<X86Context>(nameof(X86Context.EFlags)).ToInt32() != 192 ||
                Marshal.OffsetOf<X86Context>(nameof(X86Context.Esp)).ToInt32() != 196 ||
                Marshal.OffsetOf<X86Context>(nameof(X86Context.ExtendedRegisters)).ToInt32() != 204)
                throw new PlatformNotSupportedException();
        }

        private void Context(Action<nint, nint> action, bool suspend)
        {
            Layout();
            nint thread = OpenThread(0x1A, false, Tid);
            if (thread == 0) throw new Win32Exception(Marshal.GetLastPInvokeError());
            bool incremented = false;
            int cleanupError = 0;
            try
            {
                if (suspend)
                {
                    uint previous = SuspendThread(thread);
                    if (previous == uint.MaxValue) throw new Win32Exception(Marshal.GetLastPInvokeError());
                    incremented = true;
                    Log("suspend", new { PreviousCount = previous });
                }
                byte* storage = stackalloc byte[731];
                X86Context* context = (X86Context*)(((nuint)storage + 15) & ~(nuint)15);
                *context = default;
                context->ContextFlags = 0x10017;
                bool obtained = GetThreadContext(thread, context);
                int error = obtained ? 0 : Marshal.GetLastPInvokeError();
                Raw(Owned ? "owned-get" : "before-get", context, obtained, error);
                if (!obtained) throw new Win32Exception(error);
                action(thread, (nint)context);
            }
            finally
            {
                try
                {
                    if (incremented)
                    {
                        uint previous = ResumeThread(thread);
                        Log("resume-own-increment", new { PreviousCount = previous });
                        if (previous == uint.MaxValue) cleanupError = Marshal.GetLastPInvokeError();
                    }
                }
                finally
                {
                    if (!CloseHandle(thread)) cleanupError = Marshal.GetLastPInvokeError();
                    if (cleanupError != 0) Log("context-cleanup-failed", new { Error = cleanupError });
                }
            }
            if (cleanupError != 0) throw new Win32Exception(cleanupError);
        }

        private void Write(nint thread, X86Context* context, uint dr0, uint dr1, uint dr7)
        {
            // Only the debug-register group is written. EIP and application EFlags (including RF/TF) are unchanged.
            context->ContextFlags = 0x10010;
            context->Dr0 = dr0; context->Dr1 = dr1; context->Dr2 = 0; context->Dr3 = 0;
            context->Dr6 = 0; context->Dr7 = dr7;
            bool set = SetThreadContext(thread, context);
            int setError = set ? 0 : Marshal.GetLastPInvokeError();
            Raw("set", context, set, setError);
            if (!set) throw new Win32Exception(setError);
            context->ContextFlags = 0x10017;
            bool obtained = GetThreadContext(thread, context);
            int getError = obtained ? 0 : Marshal.GetLastPInvokeError();
            Raw(dr7 == 0 ? "cleared-readback" : "set-readback-raw", context, obtained, getError);
            if (!obtained) throw new Win32Exception(getError);
            // Windows/CPU may return fixed reserved DR6/DR7 bits; verify all address, enable, type and length bits.
            bool verified = context->Dr0 == dr0 && context->Dr1 == dr1 && context->Dr2 == 0 && context->Dr3 == 0 &&
                (context->Dr7 & 0xFFFF23FFu) == dr7 && (context->Dr6 & 0xE00Fu) == 0;
            Log("set-readback", new { dr0, dr1, dr7, Verified = verified }, context);
            if (!verified) throw new InvalidOperationException();
        }

        internal void Arm(uint tid, uint before, uint after, uint middle = 0, uint tail = 0, bool repeatedControl = false)
        {
            if (Phase != 0 || Owned || tid == 0 || before == 0) throw new InvalidOperationException();
            points = middle == 0 ? (after == 0 ? [before] : [before, after])
                : tail == 0 ? [before, middle, after] : [before, middle, tail, after];
            if (repeatedControl)
            {
                if (middle != 0 || tail != 0 || after != checked(before + 6)) throw new InvalidOperationException();
                points = [before, before, before, after];
            }
            calleeFrames = tail != 0;
            Tid = tid; Before = before; After = after;
            Context((thread, pointer) =>
            {
                X86Context* context = (X86Context*)pointer;
                if (context->Dr0 != 0 || context->Dr1 != 0 || context->Dr2 != 0 || context->Dr3 != 0 ||
                    (context->Dr7 & 0xFFFF23FFu) != 0 || (context->Dr6 & 0xE00Fu) != 0 || (context->EFlags & 0x100) != 0)
                    throw new InvalidOperationException();
                Owned = true;
                Write(thread, context, Before, After, 1);
                Phase = 1;
                Log("armed", new { Before, After, NewThreads = "Not armed; only authenticated event TID" });
            }, false);
        }

        internal void Clear(bool suspend)
        {
            if (!Owned) return;
            Context((thread, pointer) =>
            {
                Write(thread, (X86Context*)pointer, 0, 0, 0);
                Owned = false;
                Log("clear-confirmed", new { SuspendRequested = suspend }, (X86Context*)pointer);
            }, suspend);
        }

        internal void Destroyed(string reason)
        {
            Log("thread-destroyed-not-readback", new { reason, WasOwned = Owned });
            Owned = false;
        }

        private object Read(string label, uint address, int count)
        {
            if ((ulong)address + (uint)count > 0x100000000UL) return new { Label = label, Address = address, Requested = count, Overflow = true };
            byte[] bytes = new byte[count];
            bool ok; nuint read;
            fixed (byte* buffer = bytes) ok = ReadProcessMemory(process, (nint)address, buffer, (nuint)count, out read);
            int error = ok ? 0 : Marshal.GetLastPInvokeError();
            return new { Label = label, Address = address, Requested = count, Succeeded = ok, Error = error,
                BytesRead = (ulong)read, Hex = Convert.ToHexString(bytes.AsSpan(0, checked((int)Math.Min(read, (nuint)count)))) };
        }

        internal bool Hit(uint tid, uint address, bool first)
        {
            if (!Owned || tid != Tid || !first || Phase < 1 || Phase > points.Length) return false;
            bool matched = false;
            Context((thread, pointer) =>
            {
                X86Context* context = (X86Context*)pointer;
                if (writeWatch)
                {
                    uint expectedDr7 = 0xD0001u | (writeControl ? 0u : 4u);
                    uint triggered = context->Dr6 & 0xE00Fu;
                    if (context->Dr0 != frame || context->Dr1 != (writeControl ? 0 : points[2]) ||
                        context->Dr2 != 0 || context->Dr3 != 0 || (context->Dr7 & 0xFFFF23FFu) != expectedDr7 ||
                        (context->EFlags & 0x100) != 0) return;
                    bool tail = !writeControl && triggered == 2 && context->Eip == points[2] && address == points[2];
                    if (triggered != 1 && !tail) return;
                    uint value = ReadDword(frame);
                    matched = true;
                    Raw("write4-hit-raw", context, true, 0);
                    Log(tail ? "write4-tail" : "write4-after-access", new { FixedFrame = frame,
                        OldValue = previousValue, NewValue = value, Changed = value != previousValue,
                        PostAccessEip = context->Eip, ExceptionAddress = address, Event = ++writeEvents,
                        OtherThreadsExcluded = false,
                        Reads = new[] { Read("fixed-slot-and-return", frame, 8), Read("bounded-stack", context->Esp, 128),
                            Read("post-access-EIP-minus32", context->Eip >= 32 ? context->Eip - 32 : 0, 64) } }, context);
                    previousValue = value;
                    if (writeControl || tail || writeEvents >= 16)
                    {
                        Write(thread, context, 0, 0, 0);
                        Owned = false; writeWatch = false;
                        Paired = writeControl && value == 0x76543210;
                        Phase = points.Length + 1;
                        Log("clear-confirmed", new { Paired, Tail = tail, BudgetReached = writeEvents >= 16 }, context);
                    }
                    else Write(thread, context, frame, points[2], expectedDr7);
                    return;
                }
                bool final = Phase == points.Length && After != 0;
                uint expected = points[Phase - 1];
                uint slot = final ? 2u : 1u;
                uint enable = final ? 4u : 1u;
                if (context->Eip != expected || address != expected || (context->Dr6 & 0xE00Fu) != slot ||
                    context->Dr0 != (final ? 0 : expected) || context->Dr1 != After || context->Dr2 != 0 || context->Dr3 != 0 ||
                    (context->Dr7 & 0xFFFF23FFu) != enable || (context->EFlags & 0x100) != 0) return;
                matched = true;
                Raw("phase-hit-raw", context, true, 0);
                if (calleeFrames && Phase == 2) frame = context->Ebp;
                if (Phase == 1) { InitialEsp = context->Esp; InitialEbp = context->Ebp; }
                var reads = new List<object> { Read("ESP", context->Esp, 256), Read("EBP-saved-frame", context->Ebp, 16),
                    Read("EIP", context->Eip, 16), Read("initial-EBP-saved-frame", InitialEbp, 16) };
                if (InitialEsp >= 32) reads.Add(Read("initial-ESP-minus32-callee-residual-slots", InitialEsp - 32, 48));
                if (InitialEbp >= 160) reads.Add(Read("caller-frame-storage", InitialEbp - 160, 192));
                if (calleeFrames && Phase >= 2)
                {
                    reads.Add(Read("fixed-entry-frame-savedP-and-ret", frame, 8));
                    reads.Add(Read("current-ESP-savedP-and-ret", context->Esp, 8));
                }
                Log("phase-hit", new { Address = address, Final = final,
                    InitialEsp, InitialEbp, FixedFrame = frame, Reads = reads, Decoded = false,
                    FrameEqualsExpected = calleeFrames && InitialEsp >= 8 && frame == InitialEsp - 8,
                    EspEqualsFixedFrame = calleeFrames && context->Esp == frame,
                    EbpEqualsFixedFrame = calleeFrames && context->Ebp == frame,
                    PairEstablished = final && context->Esp == InitialEsp, FullStackProven = false }, context);
                if (calleeFrames && Phase == 2)
                {
                    uint saved = ReadDword(frame);
                    uint ret = ReadDword(checked(frame + 4));
                    if (InitialEsp < 8 || frame != InitialEsp - 8 || context->Esp != frame ||
                        (frame & 3) != 0 || saved != InitialEbp || ret != After)
                        throw new InvalidOperationException("Write watch entry F/S/P/ret/alignment authentication rejected.");
                    previousValue = saved;
                    Write(thread, context, points[2], After, 1);
                    Phase++;
                    Log("qi-exec-transition-confirmed", new { FixedFrame = frame, SavedP = saved, Return = ret, Next = points[2] }, context);
                    return;
                }
                if (calleeFrames && Phase == 3)
                {
#if GX_GETDIRECT3D_STACK
                    CaptureGetDirect3D(context);
                    gxPoint++;
                    if (gxPoint < 3)
                    {
                        points[2] = checked(points[2] + (gxPoint == 1 ? 4u : 3u));
                        Write(thread, context, points[2], After, 1);
                        Log("gx-next-point", new { Point = gxPoint + 1, Next = points[2] }, context);
                        return;
                    }
                    Write(thread, context, 0, 0, 0);
                    Owned = false;
                    Phase = points.Length + 1;
                    Log("gx-clear-confirmed", new { Captured = gxPoint, ContinuedRealPath = true }, context);
                    return;
#else
#if GU_HELPER_STACK
                    if (!helperBeforeCaptured)
                    {
                        helperBeforeEsp = context->Esp;
                        CaptureQi(context);
                        helperBeforeCaptured = true;
                        points[2] = checked(points[2] + 5);
                        Write(thread, context, points[2], After, 1);
                        Log("helper-return-transition", new { Next = points[2] }, context);
                        return;
                    }
#endif
                    CaptureQi(context);
                    Write(thread, context, 0, 0, 0);
                    Owned = false;
                    Phase = points.Length + 1;
                    Log("qi-clear-confirmed", new { Captured = true, ContinuedRealPath = true }, context);
                    return;
#endif
                }
                if (Phase < points.Length)
                {
                    bool nextFinal = Phase + 1 == points.Length && After != 0;
                    Write(thread, context, nextFinal ? 0 : points[Phase], After, nextFinal ? 4u : 1u);
                    Phase++;
                    Log("transition-confirmed", new { Next = points[Phase - 1], Slot = nextFinal ? 1 : 0 }, context);
                }
                else
                {
                    Write(thread, context, 0, 0, 0);
                    Paired = After != 0 && context->Esp == InitialEsp;
                    Owned = false; Phase++;
                    Log("clear-confirmed", new { Paired }, context);
                }
            }, false);
            return matched;
        }
    }

    private const string CandidateSha = "1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53";

    internal static void CandidateReady(nint module, string actual)
    {
        string? token = Environment.GetEnvironmentVariable("FH_CANDIDATE_READY");
        if (string.IsNullOrWhiteSpace(token)) return;
        string sha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual)));
        if (sha != CandidateSha || IntPtr.Size != 4) throw new InvalidOperationException();
        OutputDebugString(token + ":" + ((uint)module).ToString("X8", System.Globalization.CultureInfo.InvariantCulture) + ":" + sha);
    }

    private static uint AuthenticateCandidate(nint process, uint address, uint mappedSize, string path, nint file = 0, string? audit = null)
    {
        void Log(object value)
        {
            if (audit != null) File.AppendAllText(audit, JsonSerializer.Serialize(value, new JsonSerializerOptions { IncludeFields = true }) + Environment.NewLine);
        }
        byte[] bytes;
        if (file == 0) bytes = File.ReadAllBytes(path);
        else
        {
            uint type = GetFileType(file);
            bool identified = GetFileInformationByHandle(file, out FileIdentity identity);
            int identityError = identified ? 0 : Marshal.GetLastPInvokeError();
            Log(new { Stage = "handle-identity-before", Handle = (long)file, Type = type, Identified = identified, Error = identityError, identity.Volume, identity.IndexHigh, identity.IndexLow, identity.SizeHigh, identity.SizeLow, identity.WriteHigh, identity.WriteLow, Path = path });
            if (type != 1 || !identified)
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Retained LOAD_DLL disk identity unavailable.");
            long length = ((long)identity.SizeHigh << 32) | identity.SizeLow;
            if (length <= 0 || length > 64 * 1024 * 1024) throw new InvalidDataException("Candidate file length outside diagnostic bound.");
            if (!SetFilePointerEx(file, 0, out long originalPosition, 1))
                throw new Win32Exception(Marshal.GetLastPInvokeError(), "Retained LOAD_DLL current position unavailable.");
            Log(new { Stage = "pointer-saved", Position = originalPosition, Length = length });
            bytes = new byte[checked((int)length)];
            int restoreError = 0;
            try
            {
                if (!SetFilePointerEx(file, 0, out _, 0)) throw new Win32Exception(Marshal.GetLastPInvokeError());
                int total = 0;
                fixed (byte* buffer = bytes)
                {
                    while (total < bytes.Length)
                    {
                        bool readOk = ReadFile(file, buffer + total, (uint)(bytes.Length - total), out uint count, 0);
                        int readError = readOk ? 0 : Marshal.GetLastPInvokeError();
                        Log(new { Stage = "synchronous-read", Offset = total, Requested = bytes.Length - total, Succeeded = readOk, BytesRead = count, Error = readError });
                        if (!readOk)
                            throw new Win32Exception(readError, "Retained LOAD_DLL synchronous read failed.");
                        if (count == 0) throw new EndOfStreamException("Candidate retained LOAD_DLL handle short read.");
                        total += checked((int)count);
                    }
                }
                if (!GetFileInformationByHandle(file, out FileIdentity after)) throw new Win32Exception(Marshal.GetLastPInvokeError());
                Log(new { Stage = "handle-identity-after", after.Volume, after.IndexHigh, after.IndexLow, after.SizeHigh, after.SizeLow, after.WriteHigh, after.WriteLow });
                if (identity.Volume != after.Volume || identity.IndexHigh != after.IndexHigh || identity.IndexLow != after.IndexLow ||
                    identity.SizeHigh != after.SizeHigh || identity.SizeLow != after.SizeLow ||
                    identity.WriteHigh != after.WriteHigh || identity.WriteLow != after.WriteLow)
                    throw new InvalidDataException("Retained LOAD_DLL identity changed during read.");
            }
            finally
            {
                bool restored = SetFilePointerEx(file, originalPosition, out long restoredPosition, 0);
                int positionError = restored ? 0 : Marshal.GetLastPInvokeError();
                Log(new { Stage = "pointer-restored", Succeeded = restored, Position = restoredPosition, Expected = originalPosition, Error = positionError });
                if (!restored)
                {
                    restoreError = positionError;
                    Trace.TraceError("Retained LOAD_DLL position restore failed: {0}", restoreError);
                }
            }
            if (restoreError != 0) throw new Win32Exception(restoreError, "Retained LOAD_DLL position restore failed.");
            if (!SHA256.HashData(bytes).AsSpan().SequenceEqual(SHA256.HashData(File.ReadAllBytes(path))))
                throw new InvalidOperationException("Candidate retained LOAD_DLL handle/path SHA mismatch.");
        }
        Log(new { Stage = "handle-sha", Sha256 = Convert.ToHexString(SHA256.HashData(bytes)), Bytes = bytes.Length });
        if (Convert.ToHexString(SHA256.HashData(bytes)) != CandidateSha) throw new InvalidOperationException("Candidate actual-path SHA mismatch.");
        using var image = new System.Reflection.PortableExecutable.PEReader(new MemoryStream(bytes));
        var headers = image.PEHeaders;
        var pe = headers.PEHeader ?? throw new BadImageFormatException();
        if (headers.CoffHeader.Machine != System.Reflection.PortableExecutable.Machine.I386 ||
            pe.Magic != System.Reflection.PortableExecutable.PEMagic.PE32 || pe.SizeOfImage != mappedSize ||
            mappedSize <= 0x1351F6 || (ulong)address + mappedSize > 0x100000000UL)
            throw new BadImageFormatException($"Candidate PE identity mismatch: {headers.CoffHeader.Machine}/{pe.Magic}; fileSize={pe.SizeOfImage}; mappedSize={mappedSize}; base={address:X8}.");
        byte[] mapped = new byte[headers.PEHeaderStartOffset + 84];
        fixed (byte* buffer = mapped)
        {
            if (!ReadProcessMemory(process, (nint)address, buffer, (nuint)mapped.Length, out nuint read) || read != (nuint)mapped.Length)
                throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
        int optional = headers.PEHeaderStartOffset;
        int nt = headers.CoffHeaderStartOffset - 4;
        // The loader can rewrite PE32 ImageBase; imports, relocation targets and writable data are not raw-file invariants.
        foreach (var field in new (string Name, int Offset, int Width)[] { ("DOS.Magic", 0, 2), ("DOS.e_lfanew", 60, 4),
            ("NT.Signature", nt, 4), ("COFF.Machine", nt + 4, 2), ("COFF.Sections", nt + 6, 2),
            ("COFF.TimeDateStamp", nt + 8, 4), ("COFF.OptionalSize", nt + 20, 2), ("COFF.Characteristics", nt + 22, 2),
            ("Optional.Magic", optional, 2), ("Optional.EntryPoint", optional + 16, 4),
            ("Optional.SectionAlignment", optional + 32, 4), ("Optional.FileAlignment", optional + 36, 4),
            ("Optional.SizeOfImage", optional + 56, 4), ("Optional.SizeOfHeaders", optional + 60, 4),
            ("Optional.Subsystem", optional + 68, 2), ("Optional.DllCharacteristics", optional + 70, 2) })
        {
            if (!mapped.AsSpan(field.Offset, field.Width).SequenceEqual(bytes.AsSpan(field.Offset, field.Width)))
                throw new BadImageFormatException($"Candidate stable field {field.Name}: rawOffset=0x{field.Offset:X}; mappedVA=0x{address + (uint)field.Offset:X8}; width={field.Width}; expectedRaw={Convert.ToHexString(bytes.AsSpan(field.Offset, field.Width))}; actualMapped={Convert.ToHexString(mapped.AsSpan(field.Offset, field.Width))}.");
        }
        uint preferredBase = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(optional + 28, 4));
        uint mappedBase = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(mapped.AsSpan(optional + 28, 4));
        if (mappedBase != preferredBase && mappedBase != address)
            throw new BadImageFormatException($"Candidate Optional.ImageBase: rawOffset=0x{optional + 28:X}; mappedVA=0x{address + (uint)optional + 28:X8}; expectedPreferred=0x{preferredBase:X8} or actualLoadBase=0x{address:X8}; actualMapped=0x{mappedBase:X8}.");
        foreach (var point in new[] { (Rva: 0x1351EB, Hex: "E880030000"), (Rva: 0x1351F0, Hex: "8BB568FFFFFF"),
            (Rva: 0x135570, Hex: "558BEC57565383EC44"),
#if GX_GETDIRECT3D_STACK
            (Rva: 0x135590, Hex: "4DF051568D4DB0BA08000000E8BF4E0A00FFD78BF883C4088D4DB0"),
#endif
            (Rva: 0x1355D5, Hex: "8D55EC528D55DC52518D4DB0BA0C000000E8754E0A00FFD7"), (Rva: 0x135694, Hex: "8BC68BD78D65F45B5E5F5DC3") })
        {
            byte[] expected = Convert.FromHexString(point.Hex);
            var section = headers.SectionHeaders.Single(s => point.Rva >= s.VirtualAddress &&
                (long)point.Rva + expected.Length <= (long)s.VirtualAddress + s.SizeOfRawData);
            int offset = checked(section.PointerToRawData + point.Rva - section.VirtualAddress);
            if (!bytes.AsSpan(offset, expected.Length).SequenceEqual(expected))
                throw new BadImageFormatException($"Candidate file instruction mismatch: RVA={point.Rva:X}; expected={point.Hex}; actual={Convert.ToHexString(bytes.AsSpan(offset, expected.Length))}.");
            byte[] actual = new byte[expected.Length];
            fixed (byte* buffer = actual)
            {
                if (!ReadProcessMemory(process, (nint)checked(address + (uint)point.Rva), buffer, (nuint)actual.Length, out nuint read) || read != (nuint)actual.Length)
                    throw new Win32Exception(Marshal.GetLastPInvokeError());
            }
            if (!actual.AsSpan().SequenceEqual(expected))
                throw new BadImageFormatException($"Candidate mapped instruction mismatch: RVA={point.Rva:X}; expected={point.Hex}; actual={Convert.ToHexString(actual)}.");
        }
        return address;
    }

    private static bool RecordContext(nint process, uint tid, string path,
        Dictionary<uint, (uint Size, string Path)> modules, bool control)
    {
        if (IntPtr.Size != 4 || sizeof(X86Context) != 716 || sizeof(FloatingSaveArea) != 112)
            throw new PlatformNotSupportedException();
        // Explicitly align the SDK structure; no register offsets are used to read it.
        byte* storage = stackalloc byte[716 + 15];
        X86Context* context = (X86Context*)(((nuint)storage + 15) & ~(nuint)15);
        *context = default;
        context->ContextFlags = 0x00010007; // CONTEXT_i386 | CONTROL | INTEGER | SEGMENTS.
        nint thread = OpenThread(0x0008, false, tid);
        int openError = thread == 0 ? Marshal.GetLastPInvokeError() : 0;
        bool obtained = thread != 0 && GetThreadContext(thread, context);
        int contextError = obtained ? 0 : thread == 0 ? openError : Marshal.GetLastPInvokeError();
        bool closed = thread == 0 || CloseHandle(thread);
        int closeError = closed ? 0 : Marshal.GetLastPInvokeError();
        X86Context value = *context;
        var instructionModule = modules.FirstOrDefault(m => value.Eip >= m.Key && (ulong)value.Eip < (ulong)m.Key + m.Value.Size);
        var reads = new List<object>();
        int total = 0;
        bool Read(string label, uint address, int length)
        {
            if (reads.Count >= 12 || total + length > 1024) throw new InvalidOperationException();
            total += length;
            byte[] bytes = new byte[length];
            bool ok;
            nuint read;
            int error;
            fixed (byte* buffer = bytes)
            {
                ok = ReadProcessMemory(process, (nint)address, buffer, (nuint)length, out read);
                error = ok ? 0 : Marshal.GetLastPInvokeError();
            }
            int valid = checked((int)Math.Min(read, (nuint)length));
            reads.Add(new { Label = label, Address = address, Requested = length, Succeeded = ok, Error = error,
                BytesRead = (ulong)read, Hex = Convert.ToHexString(bytes.AsSpan(0, valid)) });
            return ok && read == (nuint)length;
        }
        bool stackRead = false, invalidReadRejected = !control;
        if (obtained)
        {
            stackRead = Read("ESP", value.Esp, 256);
            Read("EIP-raw-not-decoded", value.Eip, 32);
            Read("EBP", value.Ebp, 64);
            Read("EAX", value.Eax, 32); Read("EBX", value.Ebx, 32);
            Read("ECX", value.Ecx, 32); Read("EDX", value.Edx, 32);
            Read("ESI", value.Esi, 32); Read("EDI", value.Edi, 32);
            if (control) invalidReadRejected = !Read("invalid-null-control", 0, 16);
        }
        bool validContext = obtained && closed && instructionModule.Key != 0 && stackRead && invalidReadRejected;
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            Api = "GetThreadContext", Architecture = "X86", NativeThreadId = tid, ContextSize = sizeof(X86Context),
            Alignment = (ulong)((nuint)context & 15), RequestedFlags = 0x10007, ReturnedFlags = value.ContextFlags,
            OpenSucceeded = thread != 0, OpenError = openError, Succeeded = obtained, Error = contextError,
            HandleClosed = closed, CloseError = closeError, ValidContext = validContext,
            value.Eip, value.Esp, value.Ebp, value.Eax, value.Ebx, value.Ecx, value.Edx, value.Esi, value.Edi,
            value.EFlags, value.SegCs, value.SegSs, value.SegDs, value.SegEs, value.SegFs, value.SegGs,
            ModuleBase = instructionModule.Key, ModulePath = instructionModule.Value.Path,
            ModuleSize = instructionModule.Value.Size, Reads = reads, RequestedBytes = total,
            InstructionDecoded = false, StackBalanceAuthenticated = false, FirstBadWriteAuthenticated = false
        }, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));
        return validContext;
    }

    private static int RecordModuleEvidence()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "fl-modules");
        Directory.CreateDirectory(root);
        string path = Path.Combine(Environment.SystemDirectory, "ntdll.dll");
        byte[] image = File.ReadAllBytes(path);
        using var pe = new System.Reflection.PortableExecutable.PEReader(new MemoryStream(image, false));
        var headers = pe.PEHeaders;
        int Offset(int rva)
        {
            foreach (var section in headers.SectionHeaders)
                if (rva >= section.VirtualAddress && (long)rva < (long)section.VirtualAddress + section.SizeOfRawData)
                    return checked(section.PointerToRawData + rva - section.VirtualAddress);
            throw new BadImageFormatException("RVA is not backed by section data.");
        }
        uint U32(int offset) => System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(image.AsSpan(offset, 4));
        ushort U16(int offset) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(image.AsSpan(offset, 2));
        int directory = Offset(headers.PEHeader!.ExportTableDirectory.RelativeVirtualAddress);
        int count = checked((int)U32(directory + 24));
        int functions = Offset(checked((int)U32(directory + 28)));
        int names = Offset(checked((int)U32(directory + 32)));
        int ordinals = Offset(checked((int)U32(directory + 36)));
        var exports = new Dictionary<string, uint>(StringComparer.Ordinal);
        for (int i = 0; i < count; i++)
        {
            int nameOffset = Offset(checked((int)U32(names + i * 4)));
            int end = Array.IndexOf(image, (byte)0, nameOffset);
            if (end < 0) throw new BadImageFormatException();
            string name = Encoding.ASCII.GetString(image, nameOffset, end - nameOffset);
            exports.Add(name, U32(functions + U16(ordinals + i * 2) * 4));
        }
        const int targetRva = 0x118B88;
        nint module = GetModuleHandle("ntdll.dll");
        byte[] mappedBytes = new byte[33];
        bool readOk;
        nuint read;
        int readError;
        using var process = Process.GetCurrentProcess();
        fixed (byte* buffer = mappedBytes)
        {
            readOk = ReadProcessMemory(process.Handle, module + targetRva - 16,
                buffer, (nuint)mappedBytes.Length, out read);
            readError = readOk ? 0 : Marshal.GetLastPInvokeError();
        }
        var evidence = new
        {
            Stage = "module-authentication-only", Root = root, Runtime = RuntimeInformation.FrameworkDescription,
            CandidateFileSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.AotDll))),
            CandidateLoaded = process.Modules.Cast<ProcessModule>().Any(m =>
                string.Equals(m.FileName, AcceptancePaths.AotDll, StringComparison.OrdinalIgnoreCase)),
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(), PointerSize = IntPtr.Size,
            TestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(NativeRun).Assembly.Location))),
            Mvid = typeof(NativeRun).Module.ModuleVersionId, Path = path,
            Sha256 = Convert.ToHexString(SHA256.HashData(image)), Machine = headers.CoffHeader.Machine.ToString(),
            Magic = headers.PEHeader.Magic.ToString(), headers.PEHeader.SizeOfImage,
            ModuleBase = (long)module, TargetRva = targetRva,
            TargetFileOffset = Offset(targetRva),
            FileBytesFromRvaMinus16 = Convert.ToHexString(image.AsSpan(Offset(targetRva - 16), 33)),
            ReadProcessMemorySucceeded = readOk, ReadProcessMemoryError = readError, BytesRead = (ulong)read,
            MappedBytesFromRvaMinus16 = Convert.ToHexString(mappedBytes.AsSpan(0, checked((int)read))),
            Exports = exports, ChildCreated = false, BreakpointAuthenticated = false
        };
        string json = JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(root, "module-evidence.json"), json);
        Console.WriteLine(json);
        return readOk && read == (nuint)mappedBytes.Length ? 0 : 2;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void WriteControlDword(uint* memory) => *memory = 0x76543210;

    internal static int Run(string[] args)
    {
        if (!OperatingSystem.IsWindows() || RuntimeInformation.ProcessArchitecture != Architecture.X86)
            throw new PlatformNotSupportedException();
        if (args.Length == 1 && args[0] == "auth-readonly")
        {
            string path = AcceptancePaths.AotDll;
            byte[] raw = File.ReadAllBytes(path);
            using var reader = new System.Reflection.PortableExecutable.PEReader(new MemoryStream(raw));
            var headers = reader.PEHeaders;
            nint module = NativeLibrary.Load(path);
            byte[] mapped = new byte[headers.PEHeaderStartOffset + 84];
            using var current = Process.GetCurrentProcess();
            bool ok; nuint read;
            fixed (byte* buffer = mapped) ok = ReadProcessMemory(current.Handle, module, buffer, (nuint)mapped.Length, out read);
            int error = ok ? 0 : Marshal.GetLastPInvokeError();
            string? rejection = null;
            try { AuthenticateCandidate(current.Handle, (uint)module, (uint)headers.PEHeader!.SizeOfImage, path); }
            catch (BadImageFormatException exception) { rejection = exception.ToString(); }
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "auth-readonly.json"), JsonSerializer.Serialize(new {
                Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                TestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(NativeRun).Assembly.Location))),
                Mvid = typeof(NativeRun).Module.ModuleVersionId, CandidateSha256 = Convert.ToHexString(SHA256.HashData(raw)),
                Path = path, Base = (uint)module, headers.PEHeaderStartOffset, headers.CoffHeaderStartOffset,
                Machine = headers.CoffHeader.Machine.ToString(), Magic = headers.PEHeader!.Magic.ToString(), headers.PEHeader.SizeOfImage,
                ReadSucceeded = ok, BytesRead = (ulong)read, Error = error,
                Raw = Convert.ToHexString(raw.AsSpan(0, mapped.Length)), Mapped = Convert.ToHexString(mapped),
                Differences = Enumerable.Range(0, mapped.Length).Where(i => raw[i] != mapped[i]).Select(i => new { Offset = i, Raw = raw[i], Mapped = mapped[i] }).ToArray(),
                Rejection = rejection, ArmAttempted = false }));
            return ok && read == (nuint)mapped.Length ? 0 : 2;
        }
#if GX_GETDIRECT3D_STACK
        if (args.Length == 1 && args[0] == "gx-binding")
        {
            using var input = typeof(NativeRun).Assembly.GetManifestResourceStream("GX.Binding.dll") ?? throw new InvalidDataException();
            using var copy = new MemoryStream();
            input.CopyTo(copy);
            byte[] binding = copy.ToArray();
            using var pe = new System.Reflection.PortableExecutable.PEReader(new MemoryStream(binding));
            var md = pe.GetMetadataReader();
            var lines = new List<string> { $"SHA256={Convert.ToHexString(SHA256.HashData(binding))};MVID={md.GetGuid(md.GetModuleDefinition().Mvid)};Runtime={RuntimeInformation.FrameworkDescription};CandidateLoaded=false;HistoricalIdentityAuthenticated=false" };
            foreach (var typeHandle in md.TypeDefinitions)
            {
                var type = md.GetTypeDefinition(typeHandle);
                if (md.GetString(type.Name) != "IDirect3DDevice9") continue;
                foreach (var handle in type.GetMethods())
                {
                    var method = md.GetMethodDefinition(handle);
                    if (md.GetString(method.Name) != "GetDirect3D") continue;
                    byte[] il = pe.GetMethodBody(method.RelativeVirtualAddress).GetILBytes() ?? throw new InvalidDataException();
                    lines.Add($"MethodToken={System.Reflection.Metadata.Ecma335.MetadataTokens.GetToken(handle):X8};DeclaredSignature={Convert.ToHexString(md.GetBlobBytes(method.Signature))};IL={Convert.ToHexString(il)}");
                }
            }
            int count = md.GetTableRowCount(System.Reflection.Metadata.Ecma335.TableIndex.StandAloneSig);
            for (int row = 1; row <= count; row++)
            {
                var handle = System.Reflection.Metadata.Ecma335.MetadataTokens.StandaloneSignatureHandle(row);
                lines.Add($"StandaloneToken={System.Reflection.Metadata.Ecma335.MetadataTokens.GetToken(handle):X8};Signature={Convert.ToHexString(md.GetBlobBytes(md.GetStandaloneSignature(handle).Signature))}");
            }
            File.WriteAllLines(Path.Combine(AppContext.BaseDirectory, "gx-binding.txt"), lines);
            return 0;
        }
#endif
        if (args.Length == 1 && args[0] == "preflight")
        {
            var diagnosticAssembly = typeof(NativeRun).Assembly;
            string[] required = { "WpfGfxShape.ComAcceptance.FhCrashEvidence:Run", "WpfGfxShape.ComAcceptance.FhCrashEvidence:CandidateReady",
                "WpfGfxShape.ComAcceptance.FhCrashEvidence+HardwareSample:Arm", "WpfGfxShape.ComAcceptance.FhCrashEvidence+HardwareSample:Hit",
                "WpfGfxShape.ComAcceptance.DrInteropAbiTests:WhenDrCandidateInteropCreateRunsThenIndependentAbiAndLifetimeAreRecorded" };
            using var metadataStream = File.OpenRead(diagnosticAssembly.Location);
            using var metadataPe = new System.Reflection.PortableExecutable.PEReader(metadataStream);
            var metadata = metadataPe.GetMetadataReader();
            string TypeName(TypeDefinitionHandle handle)
            {
                var definition = metadata.GetTypeDefinition(handle);
                var parent = definition.GetDeclaringType();
                return parent.IsNil ? metadata.GetString(definition.Namespace) + "." + metadata.GetString(definition.Name)
                    : TypeName(parent) + "+" + metadata.GetString(definition.Name);
            }
            foreach (string item in required)
            {
                string[] parts = item.Split(':');
                var typeHandle = metadata.TypeDefinitions.Single(h => TypeName(h) == parts[0]);
                var methodHandle = metadata.GetTypeDefinition(typeHandle).GetMethods()
                    .Single(h => metadata.GetString(metadata.GetMethodDefinition(h).Name) == parts[1]);
                var method = metadata.GetMethodDefinition(methodHandle);
                if (method.RelativeVirtualAddress == 0) throw new MissingMethodException(item);
                if (parts[1].StartsWith("WhenDr", StringComparison.Ordinal) && !method.GetCustomAttributes().Any(h =>
                {
                    var attribute = metadata.GetCustomAttribute(h);
                    if (attribute.Constructor.Kind != HandleKind.MemberReference) return false;
                    var member = metadata.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                    return member.Parent.Kind == HandleKind.TypeReference &&
                        metadata.GetString(metadata.GetTypeReference((TypeReferenceHandle)member.Parent).Name) == "TestMethodAttribute";
                })) throw new MissingMethodException("DR TestMethod metadata missing.");
            }
            var entry = metadataPe.PEHeaders.CorHeader?.EntryPointTokenOrRelativeVirtualAddress ?? 0;
            if ((entry & unchecked((int)0xFF000000)) != 0x06000000) throw new BadImageFormatException("Managed entry MethodDef missing.");
            string candidateSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.AotDll)));
            if (candidateSha != CandidateSha || File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "aot.sha256")).Trim() != candidateSha)
                throw new InvalidOperationException();
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "preflight.json"), JsonSerializer.Serialize(new { Required = required,
                TestSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(diagnosticAssembly.Location))), Mvid = diagnosticAssembly.ManifestModule.ModuleVersionId,
                CandidateSha256 = candidateSha, Runtime = RuntimeInformation.FrameworkDescription, Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                CandidateLoaded = false, Verified = true }));
            return 0;
        }
        if (args.Length == 1 && args[0] == "load-ready")
        {
            nint module = NativeLibrary.Load(AcceptancePaths.AotDll);
            CandidateReady(module, AcceptancePaths.AotDll);
            return 0;
        }
        if (args.Length == 1 && args[0] == "normal-ready")
        {
            string token = Environment.GetEnvironmentVariable("FH_CONTROL_READY") ?? throw new InvalidOperationException();
            OutputDebugString(token);
            return 0;
        }
        if (args.Length == 1 && args[0] == "write-ready")
        {
            string token = Environment.GetEnvironmentVariable("FH_CONTROL_READY") ?? throw new InvalidOperationException();
            uint* memory = (uint*)NativeMemory.AlignedAlloc(4, 4);
            if (memory == null) throw new InvalidOperationException("Write control allocation unavailable.");
            try
            {
                *memory = 0x12345678;
                uint tid = GetCurrentThreadId();
                OutputDebugString(token + ":" + ((uint)memory).ToString("X8", System.Globalization.CultureInfo.InvariantCulture));
                WriteControlDword(memory);
                uint actual = *memory;
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "write-control-child.json"),
                    JsonSerializer.Serialize(new { Pid = GetCurrentProcessId(), Tid = tid, AfterTid = GetCurrentThreadId(),
                        Address = (uint)memory, Actual = actual, Expected = 0x76543210u, AliveThroughWrite = true }));
                return actual == 0x76543210 && tid == GetCurrentThreadId() ? 0 : 3;
            }
            finally { NativeMemory.AlignedFree(memory); }
        }
        if (args.Length == 1 && args[0] == "hardware-ready")
        {
            string token = Environment.GetEnvironmentVariable("FH_CONTROL_READY") ?? throw new InvalidOperationException();
            nint target = NativeLibrary.GetExport(GetModuleHandle("kernel32.dll"), "GetCurrentProcessId");
            OutputDebugString(token + ":" + ((uint)target).ToString("X8", System.Globalization.CultureInfo.InvariantCulture));
            uint expectedPid = (uint)Environment.ProcessId;
            var calls = new List<object>();
            for (int call = 1; call <= 3; call++)
            {
                uint actualPid = ((delegate* unmanaged[Stdcall]<uint>)target)();
                calls.Add(new { Call = call, ActualPid = actualPid, ExpectedPid = expectedPid, Tid = GetCurrentThreadId() });
                if (actualPid != expectedPid) return 3;
            }
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "control-calls.json"), JsonSerializer.Serialize(calls));
            return 0;
        }
        if (args.Length == 1 && args[0] == "modules") return RecordModuleEvidence();
        if (args.Length != 1 || args[0] is not ("control" or "candidate" or "hw-control" or "load-control" or "write-control"))
            throw new ArgumentException(null, nameof(args));
        string mode = args[0];
        if (mode is "hw-control" or "candidate" or "write-control") Run(["preflight"]);
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "fh-" + mode);
        Directory.CreateDirectory(root);
        using var stream = new FileStream(Path.Combine(root, "native-events.jsonl"), FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        void Record(string stage, Action<Utf8JsonWriter>? fields = null)
        {
            if (stream.Length > 8 * 1024 * 1024) throw new IOException("Diagnostic event budget exceeded.");
            using (var json = new Utf8JsonWriter(stream))
            {
                json.WriteStartObject();
                json.WriteString("Stage", stage);
                json.WriteString("Utc", DateTimeOffset.UtcNow);
                fields?.Invoke(json);
                json.WriteEndObject();
                json.Flush();
            }
            stream.WriteByte(10);
            stream.Flush(true);
        }
        string executable = Environment.ProcessPath ?? throw new InvalidOperationException();
        string assembly = typeof(NativeRun).Assembly.Location;
        string native = AcceptancePaths.AotDll;
        Record("identity", j =>
        {
            j.WriteString("Mode", mode);
            j.WriteString("Runtime", RuntimeInformation.FrameworkDescription);
            j.WriteString("Architecture", RuntimeInformation.ProcessArchitecture.ToString());
            j.WriteNumber("PointerSize", IntPtr.Size);
            j.WriteNumber("SupervisorPid", Environment.ProcessId);
            j.WriteString("TestSha256", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly))));
            j.WriteString("Mvid", typeof(NativeRun).Module.ModuleVersionId);
            j.WriteString("CandidatePath", native);
            j.WriteString("CandidateSha256", Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(native))));
            j.WriteString("Root", root);
            j.WriteBoolean("DumpRequested", false);
        });
        string readyToken = "FM_READY_" + Guid.NewGuid().ToString("N");
        string child = mode == "write-control" ? "--fh-crash-evidence write-ready" : mode == "load-control" ? "--fh-crash-evidence load-ready" : mode == "hw-control" ? "--fh-crash-evidence hardware-ready" : mode == "control" ? "--fh-crash-evidence normal-ready" :
            "--filter FullyQualifiedName~WhenDrCandidateInteropCreateRunsThenIndependentAbiAndLifetimeAreRecorded --report-trx --report-trx-filename child.trx --results-directory \"" + Path.Combine(root, "TestResults") + "\" --no-progress";
        char[] command = ("\"" + executable + "\" " + child + '\0').ToCharArray();
        Startup startup = new() { Size = (uint)sizeof(Startup) };
        var environment = Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>()
            .ToDictionary(e => (string)e.Key, e => (string?)e.Value ?? "", StringComparer.OrdinalIgnoreCase);
        environment["FH_CONTROL_READY"] = readyToken;
        if (mode is "candidate" or "load-control") environment["FH_CANDIDATE_READY"] = readyToken;
        environment["WPFGFX_RUN_ROOT"] = root;
        environment["WPFGFX_EXIT_STAGES"] = Path.Combine(root, "stages.jsonl");
        char[] environmentBlock = (string.Join('\0', environment.OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
            .Select(e => e.Key + "=" + e.Value)) + "\0\0").ToCharArray();
        ProcessInformation info;
        fixed (char* commandBuffer = command)
        fixed (char* environmentBuffer = environmentBlock)
        {
            if (!CreateProcess(executable, commandBuffer, 0, 0, false, 2 | 0x08000000 | 0x400,
                (nint)environmentBuffer, AppContext.BaseDirectory, ref startup, out info))
                throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
        var hardware = new HardwareSample(info.Process, info.ProcessId, root);
        bool exited = false, killed = false, initialBreakpoint = false;
        uint exitCode = uint.MaxValue;
        int secondChance = 0, candidateContexts = 0;
        bool controlContextValid = false;
        var modules = new Dictionary<uint, (uint Size, string Path)>();
        hardware.Modules = modules;
        var retainedFiles = new Dictionary<uint, Microsoft.Win32.SafeHandles.SafeFileHandle>();
        var threads = new HashSet<uint>();
        var clock = Stopwatch.StartNew();
        var handles = new HashSet<nint> { info.Process, info.Thread };
        bool processCreated = false, exceptionSeen = false, ready = false;
        const uint creationFlags = 2 | 0x08000000 | 0x400;
        bool cleanupConfirmed = false;
        int diagnosticError = 0;
        byte* pending = stackalloc byte[96];
        try
        {
            if (!DebugSetProcessKillOnExit(true)) throw new Win32Exception(Marshal.GetLastPInvokeError());
            Record("launched", j =>
            {
                j.WriteNumber("ChildPid", info.ProcessId); j.WriteNumber("InitialThreadId", info.ThreadId);
                j.WriteNumber("CreationFlags", creationFlags); j.WriteString("Arguments", child);
            });
            byte* data = stackalloc byte[96];
            byte* header = stackalloc byte[4096];
            while (!exited)
            {
                if (clock.Elapsed.TotalSeconds > 120 && !killed)
                {
                    hardware.Clear(true);
                    if (!TerminateProcess(info.Process, 1460)) throw new Win32Exception(Marshal.GetLastPInvokeError());
                    killed = true;
                    Record("timeout-kill");
                }
                if (clock.Elapsed.TotalSeconds > 130) throw new TimeoutException();
                if (!WaitForDebugEvent(data, 100))
                {
                    int error = Marshal.GetLastPInvokeError();
                    if (error == 121) continue;
                    throw new Win32Exception(error);
                }
                uint kind = *(uint*)data, pid = *(uint*)(data + 4), tid = *(uint*)(data + 8);
                uint status = kind == 1 ? 0x80010001u : 0x00010002u;
                Record("debug-event", j =>
                {
                    j.WriteNumber("Kind", kind); j.WriteNumber("Pid", pid); j.WriteNumber("Tid", tid);
                    j.WriteBoolean("InitialBreakpointAccepted", initialBreakpoint); j.WriteBoolean("Ready", ready);
                });
                // Debug-event process/thread handles are closed by Windows after exit events.
                // Only PROCESS_INFORMATION and event file handles are owned here.
                if (kind is 3 or 6) handles.Add(*(nint*)(data + 12));
                try
                {
                    if (pid != info.ProcessId) throw new InvalidOperationException();
                    if (kind == 3)
                    {
                        if (processCreated || tid != info.ThreadId) throw new InvalidOperationException("Unexpected process creation event.");
                        processCreated = true;
                    }
                    if (kind is 2 or 3)
                    {
                        threads.Add(tid);
                        uint startAddress = *(uint*)(data + (kind == 3 ? 40 : 20));
                        Record("thread-created", j =>
                        {
                            j.WriteNumber("NativeThreadId", tid); j.WriteNumber("DebugEventKind", kind);
                            j.WriteBoolean("InitialProcessThread", kind == 3 && tid == info.ThreadId);
                            j.WriteNumber("StartAddress", startAddress);
                        });
                    }
                    if (kind is 3 or 6)
                    {
                        nint file = *(nint*)(data + 12);
                        uint address = *(uint*)(data + (kind == 3 ? 24 : 16));
                        string path = "";
                        uint size = 0;
                        try
                        {
                            char[] name = new char[32768];
                            fixed (char* nameBuffer = name)
                            {
                                uint length = file == 0 ? 0 : GetFinalPathNameByHandle(file, nameBuffer, (uint)name.Length, 0);
                                if (length > 0 && length < name.Length) path = new string(nameBuffer, 0, (int)length);
                            }
                            if (ReadProcessMemory(info.Process, (nint)address, header, 4096, out nuint read) && read == 4096)
                            {
                                int pe = *(int*)(header + 60);
                                if (pe >= 0 && pe <= 4096 - 84 && *(uint*)(header + pe) == 0x4550)
                                    size = *(uint*)(header + pe + 80);
                            }
                            if (mode is "candidate" or "load-control" && string.Equals(Path.GetFileName(path), "wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase))
                            {
                                if (file == 0 || file == -1 || !DuplicateHandle(-1, file, -1, out nint retained, 0, false, 2))
                                    throw new Win32Exception(Marshal.GetLastPInvokeError());
                                Record("file-duplicated", j => { j.WriteNumber("Original", (long)file); j.WriteNumber("Duplicate", (long)retained); j.WriteNumber("Base", address); });
                                var identityHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(retained, ownsHandle: true);
                                if (!retainedFiles.TryAdd(address, identityHandle))
                                {
                                    identityHandle.Dispose();
                                    throw new InvalidOperationException("Duplicate retained module base.");
                                }
                            }
                            modules[address] = (size, path);
                            string sha = File.Exists(path) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) : "";
                            Record("module-loaded", j => { j.WriteNumber("Base", address); j.WriteNumber("Size", size); j.WriteString("Path", path); j.WriteString("Sha256", sha); });
                        }
                        finally
                        {
                            if (file != 0 && file != -1 && handles.Remove(file))
                            {
                                bool closed = CloseHandle(file);
                                int closeError = closed ? 0 : Marshal.GetLastPInvokeError();
                                Record("event-file-close", j => { j.WriteNumber("Handle", (long)file); j.WriteBoolean("Succeeded", closed); j.WriteNumber("Error", closeError); });
                                if (!closed) diagnosticError = closeError;
                            }
                        }
                    }
                    if (kind == 4)
                    {
                        threads.Remove(tid);
                        if (tid == hardware.Tid) hardware.Destroyed("EXIT_THREAD_DEBUG_EVENT");
                    }
                    if (kind == 7)
                    {
                        uint unloaded = *(uint*)(data + 12);
                        modules.Remove(unloaded);
                        if (retainedFiles.Remove(unloaded, out var identityHandle)) identityHandle.Dispose();
                    }
                    if (kind == 8 && mode is "control" or "hw-control" or "candidate" or "load-control" or "write-control")
                    {
                        uint length = *(ushort*)(data + 18);
                        bool unicode = *(ushort*)(data + 16) != 0;
                        byte[] text = new byte[Math.Min(length * (unicode ? 2u : 1u), 4096u)];
                        bool readOk;
                        nuint read;
                        fixed (byte* buffer = text)
                            readOk = ReadProcessMemory(info.Process, *(nint*)(data + 12), buffer, (nuint)text.Length, out read);
                        string message = readOk ? (unicode ? Encoding.Unicode : Encoding.ASCII).GetString(text, 0, checked((int)read)).TrimEnd('\0') : "";
                        Record("debug-output-input", j => { j.WriteNumber("Pid", pid); j.WriteNumber("Tid", tid);
                            j.WriteBoolean("ReadSucceeded", readOk); j.WriteNumber("BytesRead", (ulong)read); j.WriteString("Text", message); });
                        if (mode == "write-control" && message.StartsWith(readyToken + ":", StringComparison.Ordinal))
                        {
                            if (!initialBreakpoint || ready || !processCreated || !threads.Contains(tid)) throw new InvalidOperationException();
                            uint target = uint.Parse(message.AsSpan(readyToken.Length + 1), System.Globalization.NumberStyles.HexNumber,
                                System.Globalization.CultureInfo.InvariantCulture);
                            hardware.ArmWriteControl(tid, target);
                            ready = true;
                        }
                        if (mode is "candidate" or "load-control" && message.StartsWith(readyToken + ":", StringComparison.Ordinal))
                        {
                            if (!initialBreakpoint || ready || !processCreated || !threads.Contains(tid)) throw new InvalidOperationException();
                            string[] parts = message.Split(':');
                            if (parts.Length != 3 || parts[2] != CandidateSha) throw new InvalidOperationException();
                            uint moduleBase = uint.Parse(parts[1], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                            var loaded = modules[moduleBase];
                            string actualPath = loaded.Path.Replace("\\\\?\\", "", StringComparison.Ordinal);
                            if (!string.Equals(Path.GetFullPath(actualPath), Path.GetFullPath(native), StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException();
                            hardware.Log("candidate-authentication-input", new { NativePid = pid, NativeTid = tid, Base = moduleBase,
                                loaded.Size, Path = loaded.Path, Stage = mode == "load-control" ? "load-only; no export/Create/Arm" : "first legal DR Create before" });
                            var identityHandle = retainedFiles[moduleBase];
                            retainedFiles.Remove(moduleBase);
                            try { AuthenticateCandidate(info.Process, moduleBase, loaded.Size, loaded.Path, identityHandle.DangerousGetHandle(), Path.Combine(root, "file-identity.jsonl")); }
                            catch (Exception error) when (error is BadImageFormatException or Win32Exception or IOException or InvalidOperationException)
                            {
                                hardware.Log("candidate-authentication-rejected", new { Error = error.ToString() });
                                throw;
                            }
                            finally
                            {
                                long retainedValue = (long)identityHandle.DangerousGetHandle();
                                identityHandle.Dispose();
                                Record("retained-file-disposed", j => { j.WriteNumber("Handle", retainedValue); j.WriteBoolean("IsClosed", identityHandle.IsClosed); });
                            }
                            hardware.Log("candidate-authenticated", new { NativePid = pid, NativeTid = tid, Base = moduleBase,
                                loaded.Size, Path = loaded.Path, ActualSha256 = CandidateSha, CallRva = "1351EB", ReturnRva = "1351F0",
                                CallBytes = "E880030000", ReturnBytes = "8BB568FFFFFF", Stage = mode == "load-control" ? "load-only; no export/Create/Arm" : "DR first legal surface Create before; output event TID" });
                            if (mode == "candidate") hardware.Arm(tid, checked(moduleBase + 0x1351EB), checked(moduleBase + 0x1351F0),
                                checked(moduleBase + 0x135573),
#if GX_GETDIRECT3D_STACK
                                checked(moduleBase + 0x1355A1));
#elif GU_HELPER_STACK
                                checked(moduleBase + 0x1355E6));
#else
                                checked(moduleBase + 0x1355EB));
#endif
                            ready = true;
                        }
                        if (mode == "hw-control" && message.StartsWith(readyToken + ":", StringComparison.Ordinal))
                        {
                            if (!initialBreakpoint || ready || !processCreated || !threads.Contains(tid)) throw new InvalidOperationException();
                            uint target = uint.Parse(message.AsSpan(readyToken.Length + 1), System.Globalization.NumberStyles.HexNumber,
                                System.Globalization.CultureInfo.InvariantCulture);
                            var loaded = modules.Single(m => target >= m.Key && (ulong)target < (ulong)m.Key + m.Value.Size);
                            nint export = NativeLibrary.GetExport(GetModuleHandle("kernel32.dll"), "GetCurrentProcessId");
                            using var current = Process.GetCurrentProcess();
                            var local = current.Modules.Cast<ProcessModule>().Single(m => export >= m.BaseAddress && export < m.BaseAddress + m.ModuleMemorySize);
                            string remotePath = loaded.Value.Path.Replace("\\\\?\\", "", StringComparison.Ordinal);
                            hardware.Log("control-export-validation-input", new { Target = target, RemoteBase = loaded.Key,
                                RemotePath = remotePath, LocalPath = local.FileName, LocalBase = (long)local.BaseAddress,
                                LocalExport = (long)export, SystemDirectory = Environment.SystemDirectory });
                            // WOW64 module enumeration can report System32 while the loaded file is in SysWOW64.
                            using var localFile = File.OpenHandle(local.FileName);
                            char[] canonicalName = new char[32768];
                            string localPath;
                            fixed (char* canonicalBuffer = canonicalName)
                            {
                                uint count = GetFinalPathNameByHandle(localFile.DangerousGetHandle(), canonicalBuffer, (uint)canonicalName.Length, 0);
                                if (count == 0 || count >= canonicalName.Length) throw new Win32Exception(Marshal.GetLastPInvokeError());
                                localPath = new string(canonicalBuffer, 0, (int)count).Replace("\\\\?\\", "", StringComparison.Ordinal);
                            }
                            string systemPath = Environment.Is64BitOperatingSystem
                                ? Environment.GetFolderPath(Environment.SpecialFolder.SystemX86) : Environment.SystemDirectory;
                            hardware.Log("control-canonical-validation", new { localPath, remotePath, systemPath,
                                RemoteRva = target - loaded.Key, LocalRva = (uint)(export - local.BaseAddress) });
                            if (!string.Equals(remotePath, localPath, StringComparison.OrdinalIgnoreCase) ||
                                target - loaded.Key != (uint)(export - local.BaseAddress) ||
                                !string.Equals(Path.GetDirectoryName(remotePath), systemPath, StringComparison.OrdinalIgnoreCase))
                                throw new InvalidOperationException();
                            hardware.Log("control-export-authenticated", new { Export = "GetCurrentProcessId", Target = target,
                                ModuleBase = loaded.Key, Path = remotePath,
                                Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(remotePath))), NativeThreadId = tid,
                                ContextSize = sizeof(X86Context), DebugOffset = 4, EbpOffset = 180, EipOffset = 184, EspOffset = 196 });
                            // Authenticate the complete straight-line function before selecting instruction boundaries.
                            byte[] controlBytes = Convert.FromHexString("64A120000000C3");
                            byte[] actualControl = new byte[controlBytes.Length];
                            fixed (byte* buffer = actualControl)
                            {
                                if (!ReadProcessMemory(info.Process, (nint)target, buffer, (nuint)actualControl.Length, out nuint count) ||
                                    count != (nuint)actualControl.Length) throw new Win32Exception(Marshal.GetLastPInvokeError());
                            }
                            hardware.Log("control-instruction-authentication", new { Target = target,
                                Expected = Convert.ToHexString(controlBytes), Actual = Convert.ToHexString(actualControl),
                                Instructions = "MOV EAX,FS:[20h]; RET", Boundaries = new[] { 0, 6 },
                                Calls = 3, StageOffsets = new[] { 0, 0, 0, 6 } });
                            if (!actualControl.AsSpan().SequenceEqual(controlBytes)) throw new InvalidOperationException("Control instructions differ.");
                            hardware.Arm(tid, target, checked(target + 6), repeatedControl: true);
                            ready = true;
                        }
                        if (message == readyToken)
                        {
                            if (!initialBreakpoint || ready || !processCreated) throw new InvalidOperationException("Unexpected ready phase.");
                            ready = true;
                        }
                        Record("debug-output", j => { j.WriteBoolean("ReadSucceeded", readOk); j.WriteBoolean("Ready", ready); j.WriteString("Text", message); });
                    }
                    if (kind == 1)
                    {
                        uint code = *(uint*)(data + 12), address = *(uint*)(data + 24), first = *(uint*)(data + 92);
                        var module = modules.FirstOrDefault(m => address >= m.Key && (ulong)address < (ulong)m.Key + m.Value.Size);
                        // CreateProcess debugging supplies one initial breakpoint before application entry.
                        // The contract does not require an exported function RVA; this is a one-use startup phase.
                        bool loaderBreakpoint = code == 0x80000003 && first == 1 && processCreated &&
                            !exceptionSeen && !initialBreakpoint && !ready && tid == info.ThreadId &&
                            string.Equals(Path.GetFileName(module.Value.Path), "ntdll.dll", StringComparison.OrdinalIgnoreCase);
                        exceptionSeen = true;
                        if (loaderBreakpoint) { initialBreakpoint = true; status = 0x00010002; }
                        if (code == 0x80000004 && hardware.Hit(tid, address, first == 1)) status = 0x00010002;
                        else if (!loaderBreakpoint && hardware.Owned) hardware.Clear(false);
                        if (first == 0) secondChance++;
                        if (loaderBreakpoint && mode == "control")
                            controlContextValid = RecordContext(info.Process, tid, Path.Combine(root, "context-control.json"), modules, true);
                        bool candidateAv = code == 0xC0000005 && first != 0 &&
                            string.Equals(module.Value.Path?.Replace("\\\\?\\", "", StringComparison.Ordinal), Path.GetFullPath(native), StringComparison.OrdinalIgnoreCase);
                        if (mode == "candidate" && candidateAv && candidateContexts < 2)
                        {
                            candidateContexts++;
                            RecordContext(info.Process, tid, Path.Combine(root, $"context-av-{candidateContexts}.json"), modules, false);
                        }
                        Record("exception", j =>
                        {
                            j.WriteNumber("Code", code); j.WriteString("CodeHex", $"0x{code:X8}");
                            j.WriteNumber("Address", address); j.WriteString("AddressHex", $"0x{address:X8}");
                            j.WriteBoolean("FirstChance", first != 0); j.WriteBoolean("LoaderBreakpoint", loaderBreakpoint);
                            j.WriteNumber("NativeThreadId", tid); j.WriteNumber("ModuleBase", module.Key);
                            j.WriteString("ModulePath", module.Value.Path ?? "");
                            if (module.Key != 0) j.WriteNumber("ModuleOffset", address - module.Key);
                            j.WriteStartArray("LiveNativeThreadIds"); foreach (uint thread in threads.Order()) j.WriteNumberValue(thread); j.WriteEndArray();
                            uint count = Math.Min(*(uint*)(data + 28), 15);
                            j.WriteStartArray("ExceptionInformation"); for (int i = 0; i < count; i++) j.WriteNumberValue(*(uint*)(data + 32 + i * 4)); j.WriteEndArray();
                        });
                        if (code == 0x80000003 && first != 0 && !initialBreakpoint)
                            throw new InvalidOperationException("Unverified startup breakpoint; crash reproduction is not authorized.");
                    }
                    if (kind == 5)
                    {
                        exitCode = *(uint*)(data + 12); exited = true;
                        hardware.Destroyed("EXIT_PROCESS_DEBUG_EVENT");
                    }
                }
                finally
                {
                    try
                    {
                        if (hardware.Owned && (diagnosticError != 0 || status == 0x80010001)) hardware.Clear(false);
                    }
                    finally
                    {
                    foreach (nint handle in handles.Where(h => h != 0 && h != -1 && h != info.Process && h != info.Thread).ToArray())
                    {
                        handles.Remove(handle);
                        if (!CloseHandle(handle)) diagnosticError = Marshal.GetLastPInvokeError();
                    }
                    bool continued = ContinueDebugEvent(pid, tid, status);
                    int continueError = continued ? 0 : Marshal.GetLastPInvokeError();
                    Record("continue", j =>
                    {
                        j.WriteNumber("Kind", kind); j.WriteNumber("Pid", pid); j.WriteNumber("Tid", tid);
                        j.WriteNumber("Status", status); j.WriteBoolean("Succeeded", continued); j.WriteNumber("Error", continueError);
                    });
                    if (!continued)
                    {
                        int error = continueError;
                        Record("continue-failed", j => j.WriteNumber("Win32Error", error));
                        diagnosticError = error;
                        hardware.Clear(false);
                    }
                    }
                }
                if (diagnosticError != 0) throw new Win32Exception(diagnosticError);
            }
            Record("completed", j =>
            {
                j.WriteNumber("ExitCode", exitCode); j.WriteString("ExitHex", $"0x{exitCode:X8}");
                j.WriteBoolean("TimedOut", killed); j.WriteNumber("SecondChanceExceptions", secondChance);
                j.WriteBoolean("ExitEventCollected", exited); j.WriteNumber("DumpBytes", 0);
                j.WriteBoolean("Ready", ready); j.WriteBoolean("InitialBreakpointAccepted", initialBreakpoint);
            });

        }
        finally
        {
            var cleanup = new List<string>();
            foreach (var identityHandle in retainedFiles.Values) identityHandle.Dispose();
            retainedFiles.Clear();
            uint before = WaitForSingleObject(info.Process, exited ? 1000u : 0u);
            if (hardware.Owned && before != 0)
            {
                try { hardware.Clear(true); }
                catch (Win32Exception error)
                {
                    diagnosticError = error.NativeErrorCode;
                    cleanup.Add("HardwareClearFailed=" + error);
                }
                catch (InvalidOperationException error)
                {
                    diagnosticError = 13;
                    cleanup.Add("HardwareReadbackFailed=" + error);
                }
            }
            if (before != 0)
            {
                bool terminated = TerminateProcess(info.Process, 1);
                cleanup.Add($"TerminateProcess={terminated};Error={Marshal.GetLastPInvokeError()}");
                var deadline = Stopwatch.StartNew();
                while (deadline.Elapsed < TimeSpan.FromSeconds(5) && WaitForSingleObject(info.Process, 0) != 0)
                {
                    if (!WaitForDebugEvent(pending, 100)) continue;
                    uint kind = *(uint*)pending;
                    if (kind is 3 or 6) handles.Add(*(nint*)(pending + 12));
                    bool continued = ContinueDebugEvent(*(uint*)(pending + 4), *(uint*)(pending + 8), kind == 1 ? 0x80010001u : 0x00010002u);
                    int continueError = continued ? 0 : Marshal.GetLastPInvokeError();
                    cleanup.Add($"CleanupContinue={continued};Error={continueError}");
                    if (!continued) { diagnosticError = continueError; break; }
                }
            }
            bool detached = DebugActiveProcessStop(info.ProcessId);
            cleanup.Add($"Detach={detached};Error={Marshal.GetLastPInvokeError()};ExitEvent={exited}");
            uint waited = WaitForSingleObject(info.Process, 5000);
            cleanup.Add($"WaitForExit={waited};Error={Marshal.GetLastPInvokeError()}");
            bool closed = true;
            foreach (nint handle in handles.Where(h => h != 0 && h != -1))
            {
                bool result = CloseHandle(handle);
                closed &= result;
                cleanup.Add($"CloseHandle=0x{handle:X};Success={result};Error={Marshal.GetLastPInvokeError()}");
            }
            File.WriteAllLines(Path.Combine(root, "cleanup.txt"), cleanup);
            cleanupConfirmed = waited == 0 && closed && diagnosticError == 0 && initialBreakpoint;
        }
        if (!cleanupConfirmed) return 2;
        int supervisorResult = mode == "load-control" ? (exitCode == 0 && secondChance == 0 && !killed && ready && !hardware.Owned ? 0 : 1) : mode is "hw-control" or "write-control" ? (exitCode == 0 && secondChance == 0 && !killed && ready && hardware.Complete ? 0 : 1) : mode == "control" ? (exitCode == 0 && secondChance == 0 && !killed && ready && controlContextValid ? 0 : 1) :
            (ready && hardware.Complete && exitCode == 0xC0000005 && secondChance > 0 && !killed ? 0 : 1);
        Record("supervisor-result", j => { j.WriteNumber("ExitCode", supervisorResult); j.WriteBoolean("CleanupConfirmed", cleanupConfirmed); });
        return supervisorResult;
    }
}
#endif
