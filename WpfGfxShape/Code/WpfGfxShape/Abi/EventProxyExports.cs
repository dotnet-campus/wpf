using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Abi;

internal static unsafe class EventProxyExports
{
    private const int PointerError = unchecked((int)0x80004003);
    private const int NoInterface = unchecked((int)0x80004002);
    private const int OutOfMemory = unchecked((int)0x8007000E);
    private const int Unexpected = unchecked((int)0x8000FFFF);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeObject
    {
        internal void** Vtable;
        internal nint Root;
    }

    private static class VtableOwner
    {
        // Process-lifetime vtable, independent of individual COM object lifetime.
        internal static readonly nint Address = CreateVtable();
    }

    private static nint CreateVtable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(EventProxyExports), 4 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<NativeObject*, Guid*, nint*, int>)&QueryInterface;
        table[1] = (delegate* unmanaged[Stdcall]<NativeObject*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<NativeObject*, uint>)&Release;
        table[3] = (delegate* unmanaged[Stdcall]<NativeObject*, byte*, uint, int>)&RaiseEvent;
        return (nint)table;
    }

    [UnmanagedCallersOnly(EntryPoint = "MILCreateEventProxy", CallConvs = [typeof(CallConvStdcall)])]
    private static int Create(EventProxyDescriptor* descriptor, nint* output)
    {
        // Native exports.cpp does not modify the output before both pointer checks succeed.
        if (output == null || descriptor == null) return PointerError;
        NativeObject* native = null;
        GCHandle root = default;
        EventProxy? proxy = null;
        try
        {
            nint table = VtableOwner.Address;
            native = (NativeObject*)NativeMemory.AllocZeroed((nuint)sizeof(NativeObject));
            // Allocate the handle before transferring descriptor ownership to EventProxy.
            root = GCHandle.Alloc(null);
            int result = EventProxy.Create(*descriptor, out proxy);
            if (result < 0 || proxy is null) return result < 0 ? result : Unexpected;
            root.Target = proxy;
            native->Vtable = (void**)table;
            native->Root = GCHandle.ToIntPtr(root);
            proxy.AttachNativeIdentity((nint)native, root);
            *output = (nint)native;
            native = null;
            root = default;
            proxy = null;
            return 0;
        }
        catch (OutOfMemoryException) { return OutOfMemory; }
        // ABI exception barrier: report failure rather than unwind into an unmanaged caller.
        catch { return Unexpected; }
        finally
        {
            proxy?.Release();
            if (root.IsAllocated) root.Free();
            NativeMemory.Free(native);
        }
    }

    private static EventProxy GetTarget(NativeObject* self)
        => (EventProxy)GCHandle.FromIntPtr(self->Root).Target!;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryInterface(NativeObject* self, Guid* iid, nint* output)
    {
        if (output == null) return PointerError;
        *output = 0;
        if (self == null || iid == null) return PointerError;
        try
        {
            if (*iid != new Guid("00000000-0000-0000-C000-000000000046")
                && *iid != new Guid("342efd8b-669a-4d16-b163-d75f5ffd1a10")) return NoInterface;
            _ = GetTarget(self).AddRef();
            *output = (nint)self;
            return 0;
        }
        catch (OutOfMemoryException) { return OutOfMemory; }
        catch { return Unexpected; }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(NativeObject* self)
    {
        try { return GetTarget(self).AddRef(); }
        // IUnknown count methods have no HRESULT failure channel; never fake a successful count.
        catch { Environment.FailFast(null); return 0; }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(NativeObject* self)
    {
        try { return GetTarget(self).Release(); }
        catch { Environment.FailFast(null); return 0; }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int RaiseEvent(NativeObject* self, byte* bytes, uint length)
    {
        if (self == null) return PointerError;
        try { return GetTarget(self).RaiseEvent(bytes, length); }
        catch (OutOfMemoryException) { return OutOfMemory; }
        catch { return Unexpected; }
    }
}
