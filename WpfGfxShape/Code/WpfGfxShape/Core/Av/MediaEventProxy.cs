using System.Buffers.Binary;

namespace WpfGfxShape.Core.Av;

internal sealed class MediaEventProxy : IDisposable
{
    private readonly object _lock = new();
    private EventProxy? _proxy;
    private StateThread? _thread;
    private int _lastDispatchResult;

    private MediaEventProxy(EventProxy proxy) => _proxy = proxy;
    internal int LastDispatchResult => Volatile.Read(ref _lastDispatchResult);
    internal uint ThreadId => _thread?.ThreadId ?? 0;

    internal static int Create(EventProxy proxy, out MediaEventProxy? value)
    {
        ArgumentNullException.ThrowIfNull(proxy);
        value = null;
        MediaEventProxy candidate;
        try { candidate = new MediaEventProxy(proxy); }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        if (proxy.AddRef() == 0) return unchecked((int)0x80070006);
        int result = StateThread.Acquire(out candidate._thread);
        if (result < 0) { proxy.Release(); return result; }
        value = candidate;
        return 0;
    }

    internal int RaiseEvent(uint eventType, int failureHr = 0) => Enqueue(eventType, null, null, failureHr);
    internal int RaiseEvent(uint eventType, string? type, string? param, int failureHr = 0)
        => Enqueue(eventType, type ?? string.Empty, param ?? string.Empty, failureHr);

    private int Enqueue(uint eventType, string? type, string? param, int failureHr)
    {
        EventProxy? proxy;
        lock (_lock) { proxy = _proxy; }
        if (proxy is null || proxy.AddRef() == 0) return unchecked((int)0x80070006);
        bool transferred = false;
        try
        {
            lock (_lock)
            {
                if (_proxy != proxy || _thread is null) return unchecked((int)0x80070006);
                EventItem item;
                try { item = new EventItem(this, proxy, eventType, type, param, failureHr); }
                catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
                int result = _thread.AddItem(item);
                transferred = result >= 0;
                return result;
            }
        }
        finally
        {
            // Callback code can dispose this owner; never acquire its callback lock under the owner lock.
            if (!transferred) proxy.Release();
        }
    }

    internal void Shutdown()
    {
        EventProxy? proxy;
        lock (_lock) { proxy = _proxy; }
        if (proxy is null || proxy.AddRef() == 0) return;
        try { proxy.Shutdown(); }
        finally { proxy.Release(); }
    }

    /// <summary>Releases this owner's references; queued items retain their own proxy reference.</summary>
    public void Dispose()
    {
        EventProxy? proxy;
        StateThread? thread;
        lock (_lock) { proxy = _proxy; thread = _thread; _proxy = null; _thread = null; }
        proxy?.Release();
        thread?.Release();
    }

    private sealed class EventItem(MediaEventProxy owner, EventProxy proxy, uint eventType, string? type, string? param, int failureHr) : IAvEventWorkItem
    {
        public void Run()
        {
            int result = BuildPacket(eventType, failureHr, type, param, out byte[]? packet);
            if (result >= 0 && packet is not null) result = proxy.RaiseEvent(packet);
            Volatile.Write(ref owner._lastDispatchResult, result);
        }
        public void Dispose() => proxy.Release();
    }

    internal static int BuildPacket(uint eventType, int failureHr, string? type, string? param, out byte[]? packet)
    {
        packet = null;
        ReadOnlySpan<char> first = type.AsSpan();
        ReadOnlySpan<char> second = param.AsSpan();
        int zero = first.IndexOf('\0');
        if (zero >= 0) first = first[..zero];
        zero = second.IndexOf('\0');
        if (zero >= 0) second = second[..zero];
        if (type is null) second = default;
        long size = 20L + 2L * (first.Length + (long)second.Length);
        if (size > 4096) return unchecked((int)0x80070057);
        try { packet = new byte[(int)size]; }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        BinaryPrimitives.WriteUInt32LittleEndian(packet, eventType);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(4), failureHr);
        BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(8), (uint)first.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(12), (uint)second.Length);
        int offset = 16;
        // Preserve UTF-16 code units (including unpaired surrogates), not encoding replacement fallbacks.
        foreach (char character in first) { BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(offset), character); offset += 2; }
        foreach (char character in second) { BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(offset), character); offset += 2; }
        return 0;
    }
}
