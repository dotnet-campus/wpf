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

    internal static int CreateCom(nint client, out EventProxy? proxy)
    {
        proxy = null;
        if (client == 0) return unchecked((int)0x80070057);
        Guid iid = new("342efd8b-669a-4d16-b163-d75f5ffd1a10");
        nint retained = 0;
        int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(nint**)client)[0])(client, &iid, &retained);
        if (result < 0) return result;
        if (retained == 0) return unchecked((int)0x80004002);
        try
        {
            EventProxyDescriptor descriptor = new() { Dispose = &ReleaseCom, RaiseEvent = &RaiseCom, Handle = (nuint)retained };
            result = Create(descriptor, out proxy);
            if (result >= 0) retained = 0;
            return result;
        }
        finally
        {
            if (retained != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)retained)[2])(retained);
        }
    }

    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static void ReleaseCom(EventProxyDescriptor* descriptor)
    {
        nint client = (nint)descriptor->Handle;
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)client)[2])(client);
    }

    [System.Runtime.InteropServices.UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvStdcall)])]
    private static int RaiseCom(EventProxyDescriptor* descriptor, byte* bytes, uint length)
    {
        nint client = (nint)descriptor->Handle;
        return ((delegate* unmanaged[Stdcall]<nint, byte*, uint, int>)(*(nint**)client)[3])(client, bytes, length);
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
            EventProxyDescriptor* descriptor;
            lock (_lock)
            {
                descriptor = _descriptor;
                _descriptor = null;
                _isShutdown = true;
            }
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
        EventProxyDescriptor* descriptor;
        lock (_lock)
        {
            if (AddRef() == 0)
            {
                return unchecked((int)0x80070006);
            }

            descriptor = _isShutdown ? null : _descriptor;
        }

        // The retained reference protects descriptor storage during lock-free callbacks.
        try
        {
            return descriptor == null ? 0 : descriptor->RaiseEvent(descriptor, bytes, length);
        }
        finally
        {
            _ = Release();
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
