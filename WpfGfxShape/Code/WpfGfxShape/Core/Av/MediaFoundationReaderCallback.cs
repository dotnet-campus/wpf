using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

// Windows SDK 10.0.26100.0 mfreadwrite.h: IMFSourceReaderCallbackVtbl (964-1012).
internal sealed unsafe class MediaFoundationReaderCallback
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Identity { internal void** Table; internal nint Root; internal int References; }
    private readonly object _gate = new();
    private readonly ManualResetEventSlim _changed = new();
    private nint _identity;
    private nint _sample;
    private int _active;
    private bool _ready, _closing, _flushed, _flushing, _reading;
    private TaskCompletionSource _readCompletion = NewCompletion();
    private TaskCompletionSource _flushCompletion = NewCompletion();
    internal long Epoch { get; private set; }
    internal Task ReadCompletion { get { lock (_gate) return _readCompletion.Task; } }
    internal Task FlushCompletion { get { lock (_gate) return _flushCompletion.Task; } }
    private static TaskCompletionSource NewCompletion() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _status;
    private uint _stream, _flags;
    private long _timestamp;
    internal Action? ReadCallbackAdmitted { get; set; }
    internal int SamplesRetained { get; private set; }
    internal int SamplesReleased { get; private set; }
    internal int ReadCallbacks { get; private set; }
    internal int FlushCallbacks { get; private set; }
    internal bool Closing { get { lock (_gate) return _closing; } }
    internal bool Drained { get { lock (_gate) return _flushed && _active == 0; } }
    internal nint Pointer => _identity;
    private static readonly nint Vtable = CreateVtable();

    internal MediaFoundationReaderCallback()
    {
        Identity* value = (Identity*)NativeMemory.AllocZeroed((nuint)sizeof(Identity));
        value->Table = (void**)Vtable;
        value->Root = GCHandle.ToIntPtr(GCHandle.Alloc(this));
        value->References = 1;
        _identity = (nint)value;
    }

    private static nint CreateVtable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(MediaFoundationReaderCallback), 6 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Identity*, Guid*, nint*, int>)&QueryInterface;
        table[1] = (delegate* unmanaged[Stdcall]<Identity*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<Identity*, uint>)&Release;
        table[3] = (delegate* unmanaged[Stdcall]<Identity*, int, uint, uint, long, nint, int>)&OnReadSample;
        table[4] = (delegate* unmanaged[Stdcall]<Identity*, uint, int>)&OnFlush;
        table[5] = (delegate* unmanaged[Stdcall]<Identity*, uint, nint, int>)&OnEvent;
        return (nint)table;
    }

    private static MediaFoundationReaderCallback Target(Identity* self)
        => (MediaFoundationReaderCallback)GCHandle.FromIntPtr(self->Root).Target!;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryInterface(Identity* self, Guid* iid, nint* output)
    {
        if (output == null) return unchecked((int)0x80004003);
        *output = 0;
        if (iid == null) return unchecked((int)0x80004003);
        if (*iid != new Guid("00000000-0000-0000-c000-000000000046") &&
            *iid != new Guid("deec8d99-fa1d-4d82-84c2-2c8969944867")) return unchecked((int)0x80004002);
        Interlocked.Increment(ref self->References);
        *output = (nint)self;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(Identity* self) => (uint)Interlocked.Increment(ref self->References);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(Identity* self)
    {
        int remaining = Interlocked.Decrement(ref self->References);
        if (remaining == 0) { GCHandle.FromIntPtr(self->Root).Free(); NativeMemory.Free(self); }
        return (uint)remaining;
    }

    private void Enter() { lock (_gate) { _active++; } }
    private void Leave()
    {
        lock (_gate)
        {
            _active--;
            if (_flushed && _active == 0) _flushCompletion.TrySetResult();
            _changed.Set();
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int OnReadSample(Identity* self, int status, uint stream, uint flags, long timestamp, nint sample)
    {
        var target = Target(self);
        target.Enter();
        try
        {
            // The native argument is borrowed. Hold precisely one reference across this callback.
            lock (target._gate)
            {
                target.ReadCallbacks++;
                if (sample != 0)
                {
                    ((delegate* unmanaged[Stdcall]<nint, uint>)MediaFoundationSample.Table(sample)[1])(sample);
                    target.SamplesRetained++;
                }
            }
            try { target.ReadCallbackAdmitted?.Invoke(); }
            catch { status = unchecked((int)0x80004005); }
            lock (target._gate)
            {
                if (target._closing || target._flushing || target._ready)
                {
                    if (sample != 0) { MediaFoundationSample.Release(sample); target.SamplesReleased++; }
                }
                else
                {
                    target._sample = sample; target._status = status; target._stream = stream;
                    target._flags = flags; target._timestamp = timestamp; target._ready = true;
                    target._reading = false;
                    target._readCompletion.TrySetResult();
                }
            }
            return 0;
        }
        catch { return unchecked((int)0x80004005); }
        finally { target.Leave(); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int OnFlush(Identity* self, uint stream)
    {
        var target = Target(self);
        target.Enter();
        try { lock (target._gate) { target.FlushCallbacks++; target._flushed = true; } return 0; }
        finally { target.Leave(); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int OnEvent(Identity* self, uint stream, nint mediaEvent)
    {
        var target = Target(self);
        target.Enter();
        try { return 0; }
        finally { target.Leave(); }
    }

    internal int RequestRead(nint reader)
    {
        lock (_gate)
        {
            if (_closing || _flushing) return unchecked((int)0x800704c7);
            if (_reading || _ready) return unchecked((int)0xc00d36b2);
            _reading = true;
            _readCompletion = NewCompletion();
        }
        // MF may dispatch a callback before ReadSample returns: never hold the gate across COM.
        int result = ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint*, uint*, long*, nint*, int>)MediaFoundationSample.Table(reader)[9])
            (reader, 0xfffffffc, 0, null, null, null, null);
        lock (_gate)
        {
            if (result < 0)
            {
                _reading = false;
                _status = result;
                _stream = _flags = 0;
                _timestamp = 0;
                _ready = true;
                _readCompletion.TrySetResult();
            }
            return result;
        }
    }

    internal void BeginFlush()
    {
        lock (_gate)
        {
            if (_flushing) return;
            _flushing = true;
            _flushed = false;
            _flushCompletion = NewCompletion();
            _reading = _ready = false;
            if (_sample != 0) { MediaFoundationSample.Release(_sample); SamplesReleased++; _sample = 0; }
            _readCompletion.TrySetCanceled();
            _changed.Set();
        }
    }

    internal void EndFlush()
    {
        lock (_gate)
        {
            if (_closing || !_flushed || _active != 0) throw new InvalidOperationException();
            _flushing = false;
            Epoch++;
        }
    }

    internal void RequestClose()
    {
        lock (_gate)
        {
            _closing = true;
            _readCompletion.TrySetCanceled();
            if (_sample != 0) { MediaFoundationSample.Release(_sample); SamplesReleased++; _sample = 0; }
            _changed.Set();
        }
    }

    internal int WaitSample(out uint stream, out uint flags, out long timestamp, out nint sample)
    {
        for (;;)
        {
            lock (_gate)
            {
                stream = flags = 0; timestamp = 0; sample = 0;
                if (_closing) return unchecked((int)0x800704c7);
                if (_ready)
                {
                    _ready = false; stream = _stream; flags = _flags; timestamp = _timestamp;
                    sample = _sample; _sample = 0;
                    return _status;
                }
                _changed.Reset();
            }
            _changed.Wait();
        }
    }

    internal bool WaitDrain(TimeSpan timeout)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        for (;;)
        {
            lock (_gate)
            {
                if (_flushed && _active == 0) return true;
                _changed.Reset();
            }
            var remaining = timeout - clock.Elapsed;
            if (remaining <= TimeSpan.Zero || !_changed.Wait(remaining)) return false;
        }
    }

    internal uint ReleaseOwner()
    {
        nint value = Interlocked.Exchange(ref _identity, 0);
        return value == 0 ? 0 : MediaFoundationSample.Release(value);
    }
}
