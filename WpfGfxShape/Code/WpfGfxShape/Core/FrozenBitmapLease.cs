namespace WpfGfxShape.Core;

internal sealed class FrozenComSlot(nint pointer)
{
    private readonly object _gate = new();
    private nint _pointer = pointer;
    private int _pins;
    private bool _retired;

    internal nint Pointer
    {
        get { lock (_gate) return _pointer; }
    }

    internal FrozenBitmapLease Pin()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_retired, this);
            var lease = new FrozenBitmapLease(this, _pointer);
            _pins++;
            return lease;
        }
    }

    internal void Retire()
    {
        nint release = 0;
        lock (_gate)
        {
            if (_retired) return;
            _retired = true;
            if (_pins == 0)
            {
                release = _pointer;
                _pointer = 0;
            }
        }
        Direct3D9Factory.Release(release);
    }

    internal void Unpin()
    {
        nint release = 0;
        lock (_gate)
        {
            if (--_pins == 0 && _retired)
            {
                release = _pointer;
                _pointer = 0;
            }
        }
        // Clear ownership before calling a potentially reentrant external Release.
        Direct3D9Factory.Release(release);
    }
}

internal sealed class FrozenBitmapLease(FrozenComSlot slot, nint pointer) : IDisposable
{
    private FrozenComSlot? _slot = slot;
    internal nint Pointer { get; } = pointer;

    internal nint Acquire()
    {
        ObjectDisposedException.ThrowIf(_slot is null, this);
        Direct3D9Factory.AddRef(Pointer);
        return Pointer;
    }

    public void Dispose() => Interlocked.Exchange(ref _slot, null)?.Unpin();
}
