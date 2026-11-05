namespace WpfGfxShape.Core.Av;

// Independent backend adapter. No original-wpfgfx module or provider is created here.
internal sealed class SystemMediaSource
{
    private readonly IndependentMediaIdentity _identity;
    private readonly object _gate = new();
    private readonly HashSet<Action> _listeners = new();

    internal SystemMediaSource(IndependentMediaIdentity identity) => _identity = identity;

    internal static int Create(nint client, byte canOpen, out nint output, bool requireVideoOnly = false)
    {
        output = 0;
        if (client == 0 || canOpen > 1) return unchecked((int)0x80070057);
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        identity.RequireVideoOnly = requireVideoOnly;
        try
        {
            int result = identity.AttachEvents(client);
            if (result < 0) return result;
            output = identity.Pointer;
            return 0;
        }
        finally
        {
            if (output == 0) Direct3D9Factory.Release(identity.Pointer);
        }
    }

    // Lookup only: an unknown opaque address must never be dereferenced here.
    internal static SystemMediaSource? Resolve(nint media) => IndependentMediaIdentity.Resolve(media)?.Source;

    internal int Acquire(out SystemVideoSample? sample) => _identity.AcquireFrame(out sample);

    internal void Subscribe(Action listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_gate) _listeners.Add(listener);
    }

    internal void Unsubscribe(Action listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_gate) _listeners.Remove(listener);
    }

    internal void NotifyFrame()
    {
        Action[] snapshot;
        lock (_gate) snapshot = _listeners.ToArray();
        foreach (Action listener in snapshot)
        {
            // Removal prevents new admission; a callback already admitted may finish.
            lock (_gate) { if (!_listeners.Contains(listener)) continue; }
            listener();
        }
    }
}
