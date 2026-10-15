using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

// Owns the system player and its event proxy. Native CMilSlaveVideo registration
// is intentionally replaced by a managed subscription, never a fabricated pointer.
internal sealed unsafe class SystemMediaSource
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Identity { internal void** Table; internal nint Root; }
    private readonly object _gate = new();
    private sealed class Subscription(Action listener)
    {
        private Action? _listener = listener;
        internal void Cancel() => Interlocked.Exchange(ref _listener, null);
        internal void Invoke() => Volatile.Read(ref _listener)?.Invoke();
    }
    private readonly Dictionary<Action, Subscription> _listeners = [];
    private nint _media, _proxy, _client;
    private int _references = 1;
    private int _shutdown;
    private Identity* _identity;
    private static readonly nint Table = CreateTable();
    private static readonly nint ProviderTable = CreateProviderTable();
    private Identity* _providerIdentity;
    internal nint Media => _media;

    internal static int Create(nint client, byte canOpen, out nint output)
    {
        output = 0;
        var source = new SystemMediaSource();
        GCHandle root = default;
        GCHandle eventRoot = default;
        nint factory = 0;
        try
        {
            root = GCHandle.Alloc(source);
            source._identity = (Identity*)NativeMemory.AllocZeroed((nuint)sizeof(Identity));
            source._identity->Table = (void**)Table;
            source._identity->Root = GCHandle.ToIntPtr(root);
            source._providerIdentity = (Identity*)NativeMemory.AllocZeroed((nuint)sizeof(Identity));
            source._providerIdentity->Table = (void**)ProviderTable;
            source._providerIdentity->Root = GCHandle.ToIntPtr(root);
            source._client = client;
            Direct3D9Factory.AddRef(client);
            eventRoot = GCHandle.Alloc(source);
            EventProxyDescriptor descriptor = new() { Dispose = &DisposeDescriptor, RaiseEvent = &OnEvent, Handle = (nuint)GCHandle.ToIntPtr(eventRoot) };
            nint module = SystemMediaModule.Acquire();
            int hr = ((delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory"))(&factory, GeneratedProtocolFingerprint.MilSdkVersion);
            if (hr < 0) return hr;
            nint proxy = 0, media = 0;
            hr = ((delegate* unmanaged[Stdcall]<EventProxyDescriptor*, nint*, int>)NativeLibrary.GetExport(module, "MILCreateEventProxy"))(&descriptor, &proxy);
            source._proxy = proxy;
            if (hr < 0) return hr;
            eventRoot = default;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, byte, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateMediaPlayer"))(factory, proxy, canOpen, &media);
            source._media = media;
            if (hr < 0) return hr;
            output = (nint)source._identity;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (DllNotFoundException) { return unchecked((int)0x8007007E); }
        catch (DirectoryNotFoundException) { return unchecked((int)0x8007007E); }
        finally
        {
            Direct3D9Factory.Release(factory);
            if (output == 0)
            {
                source.Destroy();
                if (eventRoot.IsAllocated) eventRoot.Free();
                if (root.IsAllocated) root.Free();
                NativeMemory.Free(source._providerIdentity);
                NativeMemory.Free(source._identity);
            }
        }
    }

    internal static SystemMediaSource? Resolve(nint media)
        => media != 0 && *(nint*)media == Table ? Target((Identity*)media) : null;
    internal void Subscribe(Action listener) { lock (_gate) _listeners.TryAdd(listener, new Subscription(listener)); }
    internal void Unsubscribe(Action listener)
    {
        lock (_gate)
            if (_listeners.Remove(listener, out Subscription? subscription)) subscription.Cancel();
    }
    private void Notify()
    {
        Subscription[] listeners;
        lock (_gate) listeners = [.. _listeners.Values];
        // Snapshot owns managed storage; cancellation never waits for a callback
        // that may itself rebind or unsubscribe this resource.
        foreach (Subscription listener in listeners) listener.Invoke();
    }
    private void Destroy()
    {
        if (_media != 0) _ = ShutdownNative();
        Direct3D9Factory.Release(_media); _media = 0;
        Direct3D9Factory.Release(_proxy); _proxy = 0;
        Direct3D9Factory.Release(_client); _client = 0;
    }
    private static SystemMediaSource Target(Identity* self) => (SystemMediaSource)GCHandle.FromIntPtr(self->Root).Target!;
    private uint Retain()
    {
        int current;
        do { current = Volatile.Read(ref _references); if (current == 0) return 0; }
        while (Interlocked.CompareExchange(ref _references, checked(current + 1), current) != current);
        return (uint)(current + 1);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void DisposeDescriptor(EventProxyDescriptor* descriptor)
        => GCHandle.FromIntPtr((nint)descriptor->Handle).Free();
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int OnEvent(EventProxyDescriptor* descriptor, byte* bytes, uint count)
    {
        try
        {
            var source = (SystemMediaSource)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!;
            if (source.Retain() == 0) return 0;
            try
            {
                if (count >= 4) source.Notify();
                nint client = source._client;
                return client == 0 ? 0 : ((delegate* unmanaged[Stdcall]<nint, byte*, uint, int>)(*(void***)client)[3])(client, bytes, count);
            }
            finally { source.Drop(); }
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch { return unchecked((int)0x8000FFFF); }
    }
    private uint Drop()
    {
        int remaining = Interlocked.Decrement(ref _references);
        if (remaining == 0)
        {
            Destroy();
            GCHandle.FromIntPtr(_identity->Root).Free();
            NativeMemory.Free(_providerIdentity);
            NativeMemory.Free(_identity);
        }
        return (uint)remaining;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(Identity* self) => Target(self).Retain();
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(Identity* self) => Target(self).Drop();
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(Identity* self, Guid* iid, nint* output)
    {
        if (output == null) return unchecked((int)0x80004003);
        *output = 0;
        if (iid == null) return unchecked((int)0x80004003);
        var source = Target(self);
        if (*iid == new Guid("00000000-0000-0000-C000-000000000046") || *iid == new Guid("00000141-A8F2-4877-BA0A-FD2B6645FB94"))
        { source.Retain(); *output = (nint)self; return 0; }
        if (*iid == new Guid("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7"))
        {
            source.Retain();
            *output = (nint)source._providerIdentity;
            return 0;
        }
        return ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)source._media)[0])(source._media, iid, output);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int ProviderQuery(Identity* self, Guid* iid, nint* output)
    {
        if (output == null) return unchecked((int)0x80004003);
        *output = 0;
        if (iid == null) return unchecked((int)0x80004003);
        var source = Target(self);
        if (*iid == new Guid("00000000-0000-0000-C000-000000000046") || *iid == new Guid("00000141-A8F2-4877-BA0A-FD2B6645FB94"))
        {
            source.Retain();
            *output = (nint)source._identity;
            return 0;
        }
        if (*iid != new Guid("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7")) return unchecked((int)0x80004002);
        source.Retain();
        *output = (nint)self;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSurfaceRenderer(Identity* self, nint* output)
    {
        if (output == null) return unchecked((int)0x80004003);
        *output = 0;
        nint media = Target(self)._media, provider = 0;
        Guid iid = new("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7");
        int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)media)[0])(media, &iid, &provider);
        if (result < 0) return result;
        try
        {
            return ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)provider)[3])(provider, output);
        }
        finally { Direct3D9Factory.Release(provider); }
    }

    private static nint CreateProviderTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(SystemMediaSource), 4 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Identity*, Guid*, nint*, int>)&ProviderQuery;
        table[1] = (delegate* unmanaged[Stdcall]<Identity*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<Identity*, uint>)&Release;
        table[3] = (delegate* unmanaged[Stdcall]<Identity*, nint*, int>)&GetSurfaceRenderer;
        return (nint)table;
    }
    private static nint Native(Identity* self) => Target(self)._media;
    private static int Call(Identity* self, int slot) { nint m = Native(self); return ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)m)[slot])(m); }
    private static int Double(Identity* self, int slot, double value) { nint m = Native(self); return ((delegate* unmanaged[Stdcall]<nint, double, int>)(*(void***)m)[slot])(m, value); }
    private static int BoolOut(Identity* self, int slot, byte* value) { nint m = Native(self); return ((delegate* unmanaged[Stdcall]<nint, byte*, int>)(*(void***)m)[slot])(m, value); }
    private static int DoubleOut(Identity* self, int slot, double* value) { nint m = Native(self); return ((delegate* unmanaged[Stdcall]<nint, double*, int>)(*(void***)m)[slot])(m, value); }
    private static int UIntOut(Identity* self, int slot, uint* value) { nint m = Native(self); return ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)m)[slot])(m, value); }
    private static int TimeOut(Identity* self, int slot, long* value) { nint m = Native(self); return ((delegate* unmanaged[Stdcall]<nint, long*, int>)(*(void***)m)[slot])(m, value); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Open(Identity* s, char* url) { nint m = Native(s); return ((delegate* unmanaged[Stdcall]<nint, char*, int>)(*(void***)m)[3])(m, url); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Stop(Identity* s) => Call(s, 4);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Close(Identity* s) => Call(s, 5);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Position(Identity* s, long* v) => TimeOut(s, 6, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int SetPosition(Identity* s, long v) { nint m = Native(s); return ((delegate* unmanaged[Stdcall]<nint, long, int>)(*(void***)m)[7])(m, v); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Rate(Identity* s, double v) => Double(s, 8, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Volume(Identity* s, double v) => Double(s, 9, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Balance(Identity* s, double v) => Double(s, 10, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Scrub(Identity* s, byte v) { nint m = Native(s); return ((delegate* unmanaged[Stdcall]<nint, byte, int>)(*(void***)m)[11])(m, v); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Buffering(Identity* s, byte* v) => BoolOut(s, 12, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Pause(Identity* s, byte* v) => BoolOut(s, 13, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Download(Identity* s, double* v) => DoubleOut(s, 14, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Progress(Identity* s, double* v) => DoubleOut(s, 15, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Video(Identity* s, byte* v) => BoolOut(s, 16, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Audio(Identity* s, byte* v) => BoolOut(s, 17, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Height(Identity* s, uint* v) => UIntOut(s, 18, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Width(Identity* s, uint* v) => UIntOut(s, 19, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Length(Identity* s, long* v) => TimeOut(s, 20, v);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Update(Identity* s) => Call(s, 21);
    private int ShutdownNative()
    {
        // Native Shutdown queues state-engine teardown and must not be submitted twice.
        if (Interlocked.Exchange(ref _shutdown, 1) != 0) return 0;
        return ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)_media)[22])(_media);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Shutdown(Identity* s) => Target(s).ShutdownNative();
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])] private static int Exit(Identity* s) => Call(s, 23);
    private static nint CreateTable()
    {
        void** t = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(SystemMediaSource), 24 * sizeof(nint));
        t[0] = (delegate* unmanaged[Stdcall]<Identity*, Guid*, nint*, int>)&Query;
        t[1] = (delegate* unmanaged[Stdcall]<Identity*, uint>)&AddRef;
        t[2] = (delegate* unmanaged[Stdcall]<Identity*, uint>)&Release;
        t[3] = (delegate* unmanaged[Stdcall]<Identity*, char*, int>)&Open;
        t[4] = (delegate* unmanaged[Stdcall]<Identity*, int>)&Stop;
        t[5] = (delegate* unmanaged[Stdcall]<Identity*, int>)&Close;
        t[6] = (delegate* unmanaged[Stdcall]<Identity*, long*, int>)&Position;
        t[7] = (delegate* unmanaged[Stdcall]<Identity*, long, int>)&SetPosition;
        t[8] = (delegate* unmanaged[Stdcall]<Identity*, double, int>)&Rate;
        t[9] = (delegate* unmanaged[Stdcall]<Identity*, double, int>)&Volume;
        t[10] = (delegate* unmanaged[Stdcall]<Identity*, double, int>)&Balance;
        t[11] = (delegate* unmanaged[Stdcall]<Identity*, byte, int>)&Scrub;
        t[12] = (delegate* unmanaged[Stdcall]<Identity*, byte*, int>)&Buffering;
        t[13] = (delegate* unmanaged[Stdcall]<Identity*, byte*, int>)&Pause;
        t[14] = (delegate* unmanaged[Stdcall]<Identity*, double*, int>)&Download;
        t[15] = (delegate* unmanaged[Stdcall]<Identity*, double*, int>)&Progress;
        t[16] = (delegate* unmanaged[Stdcall]<Identity*, byte*, int>)&Video;
        t[17] = (delegate* unmanaged[Stdcall]<Identity*, byte*, int>)&Audio;
        t[18] = (delegate* unmanaged[Stdcall]<Identity*, uint*, int>)&Height;
        t[19] = (delegate* unmanaged[Stdcall]<Identity*, uint*, int>)&Width;
        t[20] = (delegate* unmanaged[Stdcall]<Identity*, long*, int>)&Length;
        t[21] = (delegate* unmanaged[Stdcall]<Identity*, int>)&Update;
        t[22] = (delegate* unmanaged[Stdcall]<Identity*, int>)&Shutdown;
        t[23] = (delegate* unmanaged[Stdcall]<Identity*, int>)&Exit;
        return (nint)t;
    }
}
