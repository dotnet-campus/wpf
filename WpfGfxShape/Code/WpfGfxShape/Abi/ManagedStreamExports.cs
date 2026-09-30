using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe class ManagedStreamExports
{
    private const int PointerError = unchecked((int)0x80004003);
    private const int Unexpected = unchecked((int)0x8000FFFF);

    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal delegate* unmanaged[Stdcall]<Descriptor*, void> Dispose;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, uint*, int> Read;
        internal delegate* unmanaged[Stdcall]<Descriptor*, long, uint, ulong*, int> Seek;
        internal delegate* unmanaged[Stdcall]<Descriptor*, void*, uint, int> Stat;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, uint*, int> Write;
        internal delegate* unmanaged[Stdcall]<Descriptor*, nint, ulong, ulong*, ulong*, int> CopyTo;
        internal delegate* unmanaged[Stdcall]<Descriptor*, ulong, int> SetSize;
        internal delegate* unmanaged[Stdcall]<Descriptor*, uint, int> Commit;
        internal delegate* unmanaged[Stdcall]<Descriptor*, int> Revert;
        internal delegate* unmanaged[Stdcall]<Descriptor*, ulong, ulong, uint, int> LockRegion, UnlockRegion;
        internal delegate* unmanaged[Stdcall]<Descriptor*, nint*, int> Clone;
        internal delegate* unmanaged[Stdcall]<Descriptor*, int*, int> CanWrite, CanSeek;
        internal nuint Handle;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StreamObject
    {
        internal void** Vtable;
        internal int References;
        internal Descriptor Descriptor;
    }

    private static class VtableOwner
    {
        internal static readonly nint Address = CreateVtable();
    }

    private static nint CreateVtable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(ManagedStreamExports), 16 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<StreamObject*, Guid*, nint*, int>)&QueryInterface;
        table[1] = (delegate* unmanaged[Stdcall]<StreamObject*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<StreamObject*, uint>)&Release;
        table[3] = (delegate* unmanaged[Stdcall]<StreamObject*, byte*, uint, uint*, int>)&Read;
        table[4] = (delegate* unmanaged[Stdcall]<StreamObject*, byte*, uint, uint*, int>)&Write;
        table[5] = (delegate* unmanaged[Stdcall]<StreamObject*, long, uint, ulong*, int>)&Seek;
        table[6] = (delegate* unmanaged[Stdcall]<StreamObject*, ulong, int>)&SetSize;
        table[7] = (delegate* unmanaged[Stdcall]<StreamObject*, nint, ulong, ulong*, ulong*, int>)&CopyTo;
        table[8] = (delegate* unmanaged[Stdcall]<StreamObject*, uint, int>)&Commit;
        table[9] = (delegate* unmanaged[Stdcall]<StreamObject*, int>)&Revert;
        table[10] = (delegate* unmanaged[Stdcall]<StreamObject*, ulong, ulong, uint, int>)&LockRegion;
        table[11] = (delegate* unmanaged[Stdcall]<StreamObject*, ulong, ulong, uint, int>)&UnlockRegion;
        table[12] = (delegate* unmanaged[Stdcall]<StreamObject*, void*, uint, int>)&Stat;
        table[13] = (delegate* unmanaged[Stdcall]<StreamObject*, nint*, int>)&Clone;
        table[14] = (delegate* unmanaged[Stdcall]<StreamObject*, int*, int>)&CanWrite;
        table[15] = (delegate* unmanaged[Stdcall]<StreamObject*, int*, int>)&CanSeek;
        return (nint)table;
    }

    [UnmanagedCallersOnly(EntryPoint = "MILCreateStreamFromStreamDescriptor", CallConvs = [typeof(CallConvStdcall)])]
    private static int Create(Descriptor* descriptor, nint* output)
    {
        if (descriptor == null || output == null) return PointerError;
        try
        {
            nint vtable = VtableOwner.Address;
            var instance = (StreamObject*)NativeMemory.Alloc((nuint)sizeof(StreamObject));
            instance->Vtable = (void**)vtable;
            instance->References = 1;
            instance->Descriptor = *descriptor;
            *output = (nint)instance;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        // No managed exception may unwind across the exported COM ABI.
        catch { return Unexpected; }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryInterface(StreamObject* instance, Guid* iid, nint* output)
    {
        if (output == null) return unchecked((int)0x80070057);
        *output = 0;
        if (*iid != new Guid("00000000-0000-0000-C000-000000000046") &&
            *iid != new Guid("0000000c-0000-0000-C000-000000000046") &&
            *iid != new Guid("3a55501a-bdcc-4e63-96bc-4ddb6f44ccdd")) return unchecked((int)0x80004002);
        Interlocked.Increment(ref instance->References);
        *output = (nint)instance;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(StreamObject* instance) => (uint)Interlocked.Increment(ref instance->References);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(StreamObject* instance)
    {
        int count = Interlocked.Decrement(ref instance->References);
        if (count == 0)
        {
            try { instance->Descriptor.Dispose(&instance->Descriptor); }
            finally { NativeMemory.Free(instance); }
        }
        return (uint)count;
    }

    [UnmanagedCallersOnly(EntryPoint = "MILIStreamWrite", CallConvs = [typeof(CallConvStdcall)])]
    private static int WriteExport(nint stream, byte* buffer, uint count, uint* written)
    {
        if (stream == 0) return PointerError;
        try { return ((delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int>)(*(void***)stream)[4])(stream, buffer, count, written); }
        catch { return Unexpected; }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Read(StreamObject* s, byte* buffer, uint count, uint* read)
    {
        try { return s->Descriptor.Read(&s->Descriptor, buffer, count, read); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Write(StreamObject* s, byte* buffer, uint count, uint* written)
    {
        uint local;
        try { return s->Descriptor.Write(&s->Descriptor, buffer, count, written == null ? &local : written); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Seek(StreamObject* s, long offset, uint origin, ulong* position)
    {
        ulong local;
        try { return s->Descriptor.Seek(&s->Descriptor, offset, origin, position == null ? &local : position); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetSize(StreamObject* s, ulong size)
    {
        try { return s->Descriptor.SetSize(&s->Descriptor, size); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CopyTo(StreamObject* s, nint target, ulong count, ulong* read, ulong* written)
    {
        ulong localRead, localWritten;
        try { return s->Descriptor.CopyTo(&s->Descriptor, target, count, read == null ? &localRead : read, written == null ? &localWritten : written); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Commit(StreamObject* s, uint flags)
    {
        try { return s->Descriptor.Commit(&s->Descriptor, flags); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Revert(StreamObject* s)
    {
        try { return s->Descriptor.Revert(&s->Descriptor); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockRegion(StreamObject* s, ulong offset, ulong count, uint flags)
    {
        try { return s->Descriptor.LockRegion(&s->Descriptor, offset, count, flags); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockRegion(StreamObject* s, ulong offset, ulong count, uint flags)
    {
        try { return s->Descriptor.UnlockRegion(&s->Descriptor, offset, count, flags); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Stat(StreamObject* s, void* stat, uint flags)
    {
        try { return s->Descriptor.Stat(&s->Descriptor, stat, flags); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Clone(StreamObject* s, nint* output)
    {
        try { return s->Descriptor.Clone(&s->Descriptor, output); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CanWrite(StreamObject* s, int* output)
    {
        try { return s->Descriptor.CanWrite(&s->Descriptor, output); }
        catch { return Unexpected; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CanSeek(StreamObject* s, int* output)
    {
        try { return s->Descriptor.CanSeek(&s->Descriptor, output); }
        catch { return Unexpected; }
    }
}
