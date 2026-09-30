using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct EventProxyDescriptor
{
    internal delegate* unmanaged[Stdcall]<EventProxyDescriptor*, void> Dispose;
    internal delegate* unmanaged[Stdcall]<EventProxyDescriptor*, byte*, uint, int> RaiseEvent;
    internal nuint Handle;
}

// Internal lifetime owner, not an IMILEventProxy COM object.
internal sealed unsafe class EventProxy
{
    private readonly object _lock = new();
    private EventProxyDescriptor* _descriptor;
    private int _references = 1;
    private bool _isShutdown;
    private nint _nativeIdentity;
    private GCHandle _nativeRoot;

    internal void AttachNativeIdentity(nint identity, GCHandle root)
    {
        _nativeIdentity = identity;
        _nativeRoot = root;
    }

    private EventProxy() { }

    internal static int Create(in EventProxyDescriptor descriptor, out EventProxy? proxy)
    {
        proxy = null;
        if (descriptor.Dispose == null || descriptor.RaiseEvent == null)
        {
            return unchecked((int)0x80070057);
        }

        try
        {
            var value = new EventProxy();
            value._descriptor = (EventProxyDescriptor*)NativeMemory.Alloc((nuint)sizeof(EventProxyDescriptor));
            *value._descriptor = descriptor;
            proxy = value;
            return 0;
        }
        catch (OutOfMemoryException)
        {
            return unchecked((int)0x8007000E);
        }
    }

    internal uint AddRef()
    {
        int current;
        do
        {
            current = Volatile.Read(ref _references);
            if (current == 0) return 0;
        }
        while (Interlocked.CompareExchange(ref _references, checked(current + 1), current) != current);
        return (uint)(current + 1);
    }

    internal uint Release()
    {
        int current;
        do
        {
            current = Volatile.Read(ref _references);
            if (current == 0) return 0;
        }
        while (Interlocked.CompareExchange(ref _references, current - 1, current) != current);

        int remaining = current - 1;
        if (remaining == 0)
        {
            lock (_lock)
            {
                EventProxyDescriptor* descriptor = _descriptor;
                _descriptor = null;
                _isShutdown = true;
                try
                {
                    descriptor->Dispose(descriptor);
                }
                finally
                {
                    NativeMemory.Free(descriptor);
                    if (_nativeRoot.IsAllocated) _nativeRoot.Free();
                    NativeMemory.Free((void*)_nativeIdentity);
                    _nativeIdentity = 0;
                }
            }
        }

        return (uint)remaining;
    }

    internal int RaiseEvent(ReadOnlySpan<byte> packet)
    {
        fixed (byte* bytes = packet)
        {
            return RaiseEvent(bytes, (uint)packet.Length);
        }
    }

    internal int RaiseEvent(byte* bytes, uint length)
    {
        lock (_lock)
        {
            if (_references == 0)
            {
                return unchecked((int)0x80070006);
            }

            if (_isShutdown)
            {
                return 0;
            }

            // A reentrant callback may release the caller's reference; retain descriptor storage until return.
            _ = AddRef();
            try
            {
                return _descriptor->RaiseEvent(_descriptor, bytes, length);
            }
            finally
            {
                _ = Release();
            }
        }
    }

    internal void Shutdown()
    {
        lock (_lock)
        {
            _isShutdown = true;
        }
    }
}
