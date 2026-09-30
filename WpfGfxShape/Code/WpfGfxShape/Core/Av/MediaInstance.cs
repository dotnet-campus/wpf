namespace WpfGfxShape.Core.Av;

internal sealed class MediaInstance : IDisposable
{
    private static int _nextId;
    private int _disposed;

    private MediaInstance(uint id, MediaEventProxy events)
    {
        Id = id;
        Events = events;
        Notifier = new CompositionNotifier(() => { _ = Events.RaiseEvent(13); });
    }

    internal uint Id { get; }
    internal MediaEventProxy Events { get; }
    internal CompositionNotifier Notifier { get; }

    internal static int Create(EventProxy proxy, out MediaInstance? instance)
    {
        instance = null;
        uint id = unchecked((uint)Interlocked.Increment(ref _nextId));
        int result = MediaEventProxy.Create(proxy, out MediaEventProxy? events);
        if (result < 0 || events is null) return result;
        try { instance = new MediaInstance(id, events); return 0; }
        catch (OutOfMemoryException) { events.Dispose(); return unchecked((int)0x8007000E); }
    }

    /// <summary>Shuts down event delivery and releases instance-owned event references. Videos must be unregistered first.</summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        Events.Shutdown();
        Events.Dispose();
    }
}
