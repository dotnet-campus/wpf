using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static unsafe class Program
{
    internal static readonly Stopwatch Clock = Stopwatch.StartNew();
    internal static readonly object OutputGate = new();
    internal static void Log(string name, object? detail = null)
    {
        lock (OutputGate) Console.WriteLine(JsonSerializer.Serialize(new { ms = Clock.ElapsedMilliseconds, pid = Environment.ProcessId, thread = Environment.CurrentManagedThreadId, name, detail }));
    }
    internal static void Check(int hr) { if (hr < 0) Marshal.ThrowExceptionForHR(hr); }
    internal static string Hex(int hr) => $"0x{hr:X8}";
    internal static void** Table(nint p) => *(void***)p;
    internal static uint Add(nint p) => ((delegate* unmanaged[Stdcall]<nint, uint>)Table(p)[1])(p);
    internal static uint Drop(nint p) => ((delegate* unmanaged[Stdcall]<nint, uint>)Table(p)[2])(p);

    private static int Main(string[] args)
    {
        Clock.Restart();
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        try
        {
            if (args is ["supervise"]) return Supervise();
            if (args.Length == 0) return Parent(false);
            if (args is ["negative-parent"]) return Parent(true);
            if (args.Length != 1 || args[0] is not ("normal" or "io-error"))
            { Console.Error.WriteLine("Invalid scenario parameter."); return 64; }
            return Child(args[0]);
        }
        catch (Exception e) when (e is COMException or InvalidOperationException or TimeoutException or IOException or System.ComponentModel.Win32Exception)
        { Console.Error.WriteLine(e); return 1; }
    }
    private static int Remaining(long deadline, int maximum) => (int)Math.Clamp(deadline - Clock.ElapsedMilliseconds, 0, maximum);

    private static (int? Exit, bool Passed, bool Collected) Run(string argument, string prefix, long deadline, int runLimit)
    {
        long startedMs = Clock.ElapsedMilliseconds;
        var start = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        if (argument.Length != 0) start.ArgumentList.Add(argument);
        using var output = new FileStream(prefix + ".stdout.jsonl", FileMode.Create, FileAccess.Write, FileShare.Read);
        using var error = new FileStream(prefix + ".stderr.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
        if (Remaining(deadline, int.MaxValue) <= 6000) throw new TimeoutException("Insufficient supervision budget.");
        using var child = Process.Start(start) ?? throw new InvalidOperationException("Process.Start");
        using var cancellation = new CancellationTokenSource();
        Task stdout = child.StandardOutput.BaseStream.CopyToAsync(output, cancellation.Token);
        Task stderr = child.StandardError.BaseStream.CopyToAsync(error, cancellation.Token);
        int waitMs = Math.Min(runLimit, Math.Max(0, Remaining(deadline, int.MaxValue) - 6000));
        bool timedOut = !child.WaitForExit(waitMs);
        string? killError = null;
        if (timedOut && Remaining(deadline, int.MaxValue) > 0)
        {
            try { child.Kill(true); }
            catch (Exception e) when (e is InvalidOperationException or System.ComponentModel.Win32Exception) { killError = e.ToString(); }
        }
        bool reaped = child.WaitForExit(Remaining(deadline, 4000));
        bool drained = Task.WaitAll([stdout, stderr], Remaining(deadline, 2000));
        if (!drained) cancellation.Cancel();
        int? exit = reaped ? child.ExitCode : null;
        long elapsedMs = Clock.ElapsedMilliseconds;
        bool passed = !timedOut && reaped && drained && killError is null && exit == 0 && elapsedMs < deadline;
        var receipt = new { argument, parentPid = Environment.ProcessId, childPid = child.Id, exit, timedOut, reaped, drained, killError, startedMs, elapsedMs, deadlineMs = deadline, waitMs, passed, hardRealtime = false };
        File.WriteAllText(prefix + ".exit.json", JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
        Log("child-receipt", receipt);
        return (exit, passed, !timedOut && reaped && drained && killError is null && elapsedMs < deadline);
    }
    private static int Parent(bool negative)
    {
        Directory.CreateDirectory("Evidence");
        const long deadline = 70000;
        bool failed = false;
        foreach (string group in negative ? new[] { "invalid-scenario" } : new[] { "normal", "io-error" })
        {
            if (Remaining(deadline, int.MaxValue) <= 6000) { failed = true; break; }
            var result = Run(group, $"Evidence/{group}", deadline, 30000);
            failed |= !result.Passed;
        }
        File.WriteAllText(negative ? "Evidence/negative-parent.json" : "Evidence/parent.json", JsonSerializer.Serialize(new { exit = failed ? 1 : 0, elapsedMs = Clock.ElapsedMilliseconds, deadlineMs = deadline, hardRealtime = false, binarySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(Program).Assembly.Location))), mvid = typeof(Program).Module.ModuleVersionId }, new JsonSerializerOptions { WriteIndented = true }));
        return failed ? 1 : 0;
    }
    private static int Supervise()
    {
        Directory.CreateDirectory("Evidence");
        var normal = Run("", "Evidence/parent", 90000, 76000);
        var negative = Run("negative-parent", "Evidence/negative-parent", 100000, 4000);
        bool passed = normal.Passed && negative.Collected && negative.Exit == 1;
        File.WriteAllText("Evidence/outer.json", JsonSerializer.Serialize(new { exit = passed ? 0 : 1, elapsedMs = Clock.ElapsedMilliseconds, normalPassed = normal.Passed, negativeExit = negative.Exit, hardRealtime = false }));
        return passed ? 0 : 1;
    }
    private static int Child(string group)
    {
        Log("start", new { group, budgetSeconds = 30 });
        Check(Native.CoInitializeEx(0, 0)); Log("CoInitializeEx-MTA");
        Check(Native.MFStartup(0x20070, 0)); Log("MFStartup");
        var stream = new ByteStream(Avi.Create());
        var callback = new ReaderCallback();
        nint attributes = 0, reader = 0, mediaType = 0;
        Check(Native.MFCreateAttributes(out attributes, 1));
        Guid key = new("1e3dbeac-bb43-4c35-b507-cd644464c965");
        Check(((delegate* unmanaged[Stdcall]<nint, Guid*, nint, int>)Table(attributes)[27])(attributes, &key, callback.Pointer));
        Log("Open-request");
        int open = Native.MFCreateSourceReaderFromByteStream(stream.Pointer, attributes, out reader);
        Log("Open-return", Hex(open)); Check(open); Drop(attributes); attributes = 0;
        Check(((delegate* unmanaged[Stdcall]<nint, uint, uint, nint*, int>)Table(reader)[5])(reader, 0xfffffffc, 0, &mediaType));
        Log("media-recognized"); Drop(mediaType);
        Request(reader, callback);
        if (!callback.ReadDone.Wait(4000) || callback.Samples == 0) throw new TimeoutException("first sample");
        Log("first-sample-confirmed");
        stream.Arm();
        bool entered = false;
        for (int i = 0; i < 128 && Clock.ElapsedMilliseconds < 11000; i++)
        {
            Request(reader, callback);
            int signal = WaitHandle.WaitAny([stream.Entry.WaitHandle, callback.ReadDone.WaitHandle], 2000);
            if (signal == 0) { entered = true; break; }
            if (signal == WaitHandle.WaitTimeout) throw new TimeoutException("read request");
            if (callback.LastStatus < 0 || callback.Eos) break;
        }
        Log("source-pending-observation", new { entered, stream.AsyncOutstanding, callback.Active, callback.Samples });
        if (entered && group == "normal")
        {
            stream.ReleaseGate(false);
            if (!callback.ReadDone.Wait(4000)) throw new TimeoutException("normal gate completion");
        }
        Log("Flush-request");
        int flush = ((delegate* unmanaged[Stdcall]<nint, uint, int>)Table(reader)[10])(reader, 0xfffffffe);
        Log("Flush-return", Hex(flush));
        stream.ReleaseGate(group == "io-error");
        Check(flush);
        if (!callback.FlushDone.Wait(4000)) throw new TimeoutException("OnFlush");
        if (!SpinWait.SpinUntil(() => callback.Active == 0 && stream.AsyncOutstanding == 0 && stream.SyncActive == 0, 4000)) throw new TimeoutException("drain");
        Log("drain", new { callback.Active, stream.AsyncOutstanding, stream.SyncActive });
        Log("reader-owner-Release", Drop(reader)); reader = 0;
        stream.ReleaseGate(group == "io-error");
        Log("callback-owner-Release", Drop(callback.Pointer));
        Log("stream-owner-Release", Drop(stream.Pointer));
        Log("MFShutdown-request"); Check(Native.MFShutdown()); Log("MFShutdown-return");
        Native.CoUninitialize(); Log("CoUninitialize-MTA");
        Log("result", new { group, entered, callback.Samples, callback.Flushes, negativeFlush = false, lateDrain = false, systemContractOnly = true });
        return 0;
    }
    private static void Request(nint reader, ReaderCallback callback)
    {
        callback.ReadDone.Reset(); Log("ReadSample-request");
        int hr = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint*, uint*, long*, nint*, int>)Table(reader)[9])(reader, 0xfffffffc, 0, null, null, null, null);
        Log("ReadSample-return", Hex(hr)); Check(hr);
    }
}

internal static class Native
{
    [DllImport("ole32.dll")] internal static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll")] internal static extern void CoUninitialize();
    [DllImport("mfplat.dll")] internal static extern int MFStartup(uint version, uint flags);
    [DllImport("mfplat.dll")] internal static extern int MFShutdown();
    [DllImport("mfplat.dll")] internal static extern int MFCreateAttributes(out nint attributes, uint size);
    [DllImport("mfreadwrite.dll")] internal static extern int MFCreateSourceReaderFromByteStream(nint stream, nint attributes, out nint reader);
    [DllImport("mfplat.dll")] internal static extern int MFCreateAsyncResult(nint value, nint callback, nint state, out nint result);
    [DllImport("mfplat.dll")] internal static extern int MFInvokeCallback(nint result);
}

// SDK 10.0.26100.0 mfobjects.h / mfreadwrite.h; identities follow the existing unmanaged COM style.
internal abstract unsafe class Identity
{
    [StructLayout(LayoutKind.Sequential)] internal struct Header { internal nint Table, Root; internal int Refs; }
    internal nint Pointer { get; }
    protected abstract Guid Iid { get; }
    protected Identity(int slots)
    {
        var p = (Header*)NativeMemory.AllocZeroed((nuint)sizeof(Header));
        p->Table = (nint)NativeMemory.AllocZeroed((nuint)(slots * sizeof(nint)));
        p->Root = GCHandle.ToIntPtr(GCHandle.Alloc(this)); p->Refs = 1; Pointer = (nint)p;
        var t = (void**)p->Table;
        t[0] = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)&Query;
        t[1] = (delegate* unmanaged[Stdcall]<nint, uint>)&Add;
        t[2] = (delegate* unmanaged[Stdcall]<nint, uint>)&Release;
    }
    internal static T Target<T>(nint p) where T : Identity => (T)GCHandle.FromIntPtr(((Header*)p)->Root).Target!;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(nint p, Guid* iid, nint* output)
    {
        if (output == null || iid == null) return unchecked((int)0x80004003);
        *output = 0; var target = Target<Identity>(p);
        if (*iid != target.Iid && *iid != new Guid("00000000-0000-0000-c000-000000000046")) return unchecked((int)0x80004002);
        Interlocked.Increment(ref ((Header*)p)->Refs); *output = p; Program.Log("QI-AddRef", new { type = target.GetType().Name, refs = ((Header*)p)->Refs }); return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Add(nint p)
    { int refs = Interlocked.Increment(ref ((Header*)p)->Refs); Program.Log("AddRef", new { type = Target<Identity>(p).GetType().Name, refs }); return (uint)refs; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(nint p)
    {
        int refs = Interlocked.Decrement(ref ((Header*)p)->Refs);
        Program.Log("Release", new { type = Target<Identity>(p).GetType().Name, refs });
        if (refs == 0) { GCHandle.FromIntPtr(((Header*)p)->Root).Free(); NativeMemory.Free((void*)((Header*)p)->Table); NativeMemory.Free((void*)p); }
        return (uint)refs;
    }
}

internal sealed unsafe class ReaderCallback : Identity
{
    protected override Guid Iid => new("deec8d99-fa1d-4d82-84c2-2c8969944867");
    internal readonly ManualResetEventSlim ReadDone = new(), FlushDone = new();
    internal int Active, Samples, Flushes, LastStatus;
    internal bool Eos;
    internal ReaderCallback() : base(6)
    {
        void** t = Program.Table(Pointer);
        t[3] = (delegate* unmanaged[Stdcall]<nint, int, uint, uint, long, nint, int>)&Read;
        t[4] = (delegate* unmanaged[Stdcall]<nint, uint, int>)&Flush;
        t[5] = (delegate* unmanaged[Stdcall]<nint, uint, nint, int>)&Event;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Read(nint p, int hr, uint stream, uint flags, long time, nint sample)
    {
        var s = Target<ReaderCallback>(p); Interlocked.Increment(ref s.Active);
        Program.Log("OnReadSample-enter", new { hr = Program.Hex(hr), stream, flags, time, sample = sample != 0, s.Active });
        s.LastStatus = hr; s.Eos = (flags & 2) != 0; if (sample != 0) Interlocked.Increment(ref s.Samples);
        // sample is borrowed, never retained by this observer.
        Interlocked.Decrement(ref s.Active); Program.Log("OnReadSample-leave", s.Active); s.ReadDone.Set(); return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Flush(nint p, uint stream)
    {
        var s = Target<ReaderCallback>(p); Interlocked.Increment(ref s.Active); Program.Log("OnFlush-enter", stream);
        Interlocked.Increment(ref s.Flushes); Interlocked.Decrement(ref s.Active); Program.Log("OnFlush-leave", s.Active); s.FlushDone.Set(); return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Event(nint p, uint stream, nint value)
    { var s = Target<ReaderCallback>(p); Interlocked.Increment(ref s.Active); Program.Log("OnEvent-enter", stream); Interlocked.Decrement(ref s.Active); Program.Log("OnEvent-leave", s.Active); return 0; }
}

internal sealed unsafe class ByteStream : Identity
{
    protected override Guid Iid => new("ad4c1b00-4bf7-422f-9175-756693d9130d");
    private const int NotImplemented = unchecked((int)0x80004001), IoError = unchecked((int)0x8007001f);
    private readonly byte[] _bytes;
    private readonly object _gate = new();
    private long _position;
    private bool _armed, _error, _closed;
    private Pending? _pending;
    private readonly Dictionary<nint, uint> _results = new();
    internal readonly ManualResetEventSlim Entry = new(), Permit = new();
    internal int AsyncOutstanding, SyncActive;
    private sealed record Pending(nint Result, nint Buffer, uint Count);
    internal ByteStream(byte[] bytes) : base(18)
    {
        _bytes = bytes; Program.Log("AVI", new { bytes.Length, sha256 = Convert.ToHexString(SHA256.HashData(bytes)) });
        void** t = Program.Table(Pointer);
        t[3] = (delegate* unmanaged[Stdcall]<nint, uint*, int>)&Capabilities;
        t[4] = (delegate* unmanaged[Stdcall]<nint, ulong*, int>)&Length;
        t[5] = (delegate* unmanaged[Stdcall]<nint, ulong, int>)&SetLength;
        t[6] = (delegate* unmanaged[Stdcall]<nint, ulong*, int>)&Position;
        t[7] = (delegate* unmanaged[Stdcall]<nint, ulong, int>)&SetPosition;
        t[8] = (delegate* unmanaged[Stdcall]<nint, int*, int>)&End;
        t[9] = (delegate* unmanaged[Stdcall]<nint, nint, uint, uint*, int>)&Read;
        t[10] = (delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, int>)&BeginRead;
        t[11] = (delegate* unmanaged[Stdcall]<nint, nint, uint*, int>)&EndRead;
        t[12] = (delegate* unmanaged[Stdcall]<nint, nint, uint, uint*, int>)&Write;
        t[13] = (delegate* unmanaged[Stdcall]<nint, nint, uint, nint, nint, int>)&BeginWrite;
        t[14] = (delegate* unmanaged[Stdcall]<nint, nint, uint*, int>)&EndWrite;
        t[15] = (delegate* unmanaged[Stdcall]<nint, int, long, uint, ulong*, int>)&Seek;
        t[16] = (delegate* unmanaged[Stdcall]<nint, int>)&Flush;
        t[17] = (delegate* unmanaged[Stdcall]<nint, int>)&Close;
    }
    internal void Arm() { lock (_gate) _armed = true; Program.Log("read-gate-armed-after-first-sample"); }
    internal void ReleaseGate(bool error)
    {
        Pending? pending;
        lock (_gate) { _armed = false; _error |= error; pending = _pending; _pending = null; }
        Program.Log("external-release", new { error, pending = pending != null }); Permit.Set();
        if (pending != null) Complete(pending);
    }
    private int Copy(nint buffer, uint count, out uint read)
    {
        lock (_gate)
        {
            read = 0; if (_error || _closed) return IoError;
            read = (uint)Math.Min(count, Math.Max(0, _bytes.LongLength - _position));
            Marshal.Copy(_bytes, checked((int)_position), buffer, checked((int)read)); _position += read; return 0;
        }
    }
    private void Complete(Pending pending)
    {
        int hr = Copy(pending.Buffer, pending.Count, out uint read);
        lock (_gate) _results.Add(pending.Result, read);
        int status = ((delegate* unmanaged[Stdcall]<nint, int, int>)Program.Table(pending.Result)[5])(pending.Result, hr);
        Program.Log("async-complete", new { hr = Program.Hex(hr), read, status = Program.Hex(status) });
        int invoke = Native.MFInvokeCallback(pending.Result); Program.Log("MFInvokeCallback", Program.Hex(invoke));
        Program.Log("async-result-owner-Release", Program.Drop(pending.Result));
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Capabilities(nint p, uint* value) { *value = 5; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Length(nint p, ulong* value) { *value = (ulong)Target<ByteStream>(p)._bytes.LongLength; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetLength(nint p, ulong value) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Position(nint p, ulong* value) { var s = Target<ByteStream>(p); lock (s._gate) *value = (ulong)s._position; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetPosition(nint p, ulong value)
    { var s = Target<ByteStream>(p); if (value > long.MaxValue) return unchecked((int)0x80070057); lock (s._gate) s._position = (long)value; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int End(nint p, int* value) { var s = Target<ByteStream>(p); lock (s._gate) *value = s._position >= s._bytes.LongLength ? 1 : 0; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Read(nint p, nint buffer, uint count, uint* read)
    {
        var s = Target<ByteStream>(p); Interlocked.Increment(ref s.SyncActive); bool armed;
        lock (s._gate) armed = s._armed;
        Program.Log("Read-entry", new { count, armed });
        if (armed) { s.Entry.Set(); if (!s.Permit.Wait(3000)) { lock (s._gate) s._error = true; Program.Log("sync-gate-budget-error"); } }
        int hr = s.Copy(buffer, count, out uint n); *read = n;
        Interlocked.Decrement(ref s.SyncActive); Program.Log("Read-leave", new { hr = Program.Hex(hr), read = n }); return hr;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int BeginRead(nint p, nint buffer, uint count, nint callback, nint state)
    {
        var s = Target<ByteStream>(p);
        int hr = Native.MFCreateAsyncResult(0, callback, state, out nint result); if (hr < 0) return hr;
        Program.Add(p); Interlocked.Increment(ref s.AsyncOutstanding);
        var pending = new Pending(result, buffer, count); bool held;
        lock (s._gate) { held = s._armed; if (held) { if (s._pending != null) { Program.Drop(result); Program.Drop(p); Interlocked.Decrement(ref s.AsyncOutstanding); return unchecked((int)0x800700aa); } s._pending = pending; } }
        Program.Log("BeginRead-entry", new { count, held, s.AsyncOutstanding });
        if (held) s.Entry.Set(); else s.Complete(pending);
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int EndRead(nint p, nint result, uint* read)
    {
        var s = Target<ByteStream>(p); *read = 0;
        lock (s._gate) { if (!s._results.Remove(result, out uint n)) return unchecked((int)0x80070057); *read = n; }
        int hr = ((delegate* unmanaged[Stdcall]<nint, int>)Program.Table(result)[4])(result);
        Interlocked.Decrement(ref s.AsyncOutstanding); Program.Log("EndRead", new { hr = Program.Hex(hr), read = *read, s.AsyncOutstanding }); Program.Drop(p); return hr;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Write(nint p, nint b, uint n, uint* written) { *written = 0; return NotImplemented; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int BeginWrite(nint p, nint b, uint n, nint cb, nint state) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int EndWrite(nint p, nint result, uint* written) { *written = 0; return NotImplemented; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Seek(nint p, int origin, long offset, uint flags, ulong* value)
    {
        var s = Target<ByteStream>(p); lock (s._gate)
        {
            long next = origin == 0 ? offset : origin == 1 ? s._position + offset : -1;
            if (next < 0) return unchecked((int)0x80070057); s._position = next; *value = (ulong)next;
        }
        Program.Log("Seek", new { origin, offset, flags, position = *value }); return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Flush(nint p) => 0;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Close(nint p) { var s = Target<ByteStream>(p); lock (s._gate) s._closed = true; s.ReleaseGate(true); Program.Log("stream-Close"); return 0; }
}

internal static class Avi
{
    internal static byte[] Create()
    {
        const int frames = 120, width = 320, height = 240, size = width * height * 3;
        using var stream = new MemoryStream(); using var writer = new BinaryWriter(stream, Encoding.ASCII);
        long Begin(string id, string? type = null) { writer.Write(Encoding.ASCII.GetBytes(id)); writer.Write(0); long start = stream.Position; if (type != null) writer.Write(Encoding.ASCII.GetBytes(type)); return start; }
        void End(long start) { long end = stream.Position; stream.Position = start - 4; writer.Write(checked((int)(end - start))); stream.Position = end; }
        long riff = Begin("RIFF", "AVI "), header = Begin("LIST", "hdrl"), main = Begin("avih");
        writer.Write(100000); writer.Write(size * 10); writer.Write(0); writer.Write(0); writer.Write(frames); writer.Write(0); writer.Write(1); writer.Write(size); writer.Write(width); writer.Write(height); writer.Write(new byte[16]); End(main);
        long list = Begin("LIST", "strl"), info = Begin("strh");
        writer.Write(Encoding.ASCII.GetBytes("vidsDIB ")); writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(1); writer.Write(10); writer.Write(0); writer.Write(frames); writer.Write(size); writer.Write(-1); writer.Write(0);
        writer.Write((short)0); writer.Write((short)0); writer.Write((short)width); writer.Write((short)height); End(info);
        long format = Begin("strf"); writer.Write(40); writer.Write(width); writer.Write(height); writer.Write((short)1); writer.Write((short)24); writer.Write(0); writer.Write(size); writer.Write(new byte[16]); End(format); End(list); End(header);
        long movi = Begin("LIST", "movi"); var pixels = new byte[size]; for (int i = 2; i < size; i += 3) pixels[i] = 255;
        for (int frame = 0; frame < frames; frame++) { long chunk = Begin("00db"); writer.Write(pixels); End(chunk); }
        End(movi); End(riff); return stream.ToArray();
    }
}
