using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class MilEffectList
{
    private const int Invalid = unchecked((int)0x80070057);
    private const int OutOfMemory = unchecked((int)0x8007000E);

    private struct Entry
    {
        internal Entry* Next;
        internal Guid Id;
        internal uint Size, Count;
        internal byte* Data;
        internal nint* Resources;
    }

    private struct Instance
    {
        internal void** Table;
        internal int References;
        internal uint Count, ResourceCount;
        internal Entry* First;
        internal Entry* Last;
    }

    private static class Tables
    {
        internal static readonly void** Address = CreateTable();
    }

    internal static int Create(out nint result)
    {
        result = 0;
        try
        {
            void** table = Tables.Address;
            Instance* self = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            self->Table = table;
            self->References = 1;
            result = (nint)self;
            return 0;
        }
        catch (OutOfMemoryException) { return OutOfMemory; }
    }

    internal static int AddAlphaScale(nint self, float scale)
    {
        Guid id = new("00000520-a8f2-4877-ba0a-fd2b6645fb94");
        return ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, void*, int>)(*(void***)self)[3])(self, &id, 4, &scale);
    }

    private static void** CreateTable()
    {
        void** t = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(MilEffectList), 17 * sizeof(nint));
        t[0] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, nint*, int>)&Query;
        t[1] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&AddRef;
        t[2] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&Release;
        t[3] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, uint, void*, int>)&Add;
        t[4] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, uint, void*, uint, nint*, int>)&AddWithResources;
        t[5] = (delegate* unmanaged[Stdcall]<Instance*, void>)&Clear;
        t[6] = (delegate* unmanaged[Stdcall]<Instance*, uint*, int>)&GetCount;
        t[7] = (delegate* unmanaged[Stdcall]<Instance*, uint, Guid*, int>)&GetId;
        t[8] = (delegate* unmanaged[Stdcall]<Instance*, uint, uint*, int>)&GetSize;
        t[9] = (delegate* unmanaged[Stdcall]<Instance*, uint, uint, void*, int>)&GetParameters;
        t[10] = (delegate* unmanaged[Stdcall]<Instance*, uint, uint*, int>)&GetResourceCount;
        t[11] = (delegate* unmanaged[Stdcall]<Instance*, uint, uint, nint*, int>)&GetResources;
        t[12] = (delegate* unmanaged[Stdcall]<Instance*, uint, void**, void>)&GetParamRef;
        t[13] = (delegate* unmanaged[Stdcall]<Instance*, uint, uint, nint*, void>)&GetResourcesNoAddRef;
        t[14] = (delegate* unmanaged[Stdcall]<Instance*, uint*, int>)&GetTotalResourceCount;
        t[15] = (delegate* unmanaged[Stdcall]<Instance*, uint, nint*, int>)&GetResource;
        t[16] = (delegate* unmanaged[Stdcall]<Instance*, uint, nint, int>)&ReplaceResource;
        return t;
    }

    private static Entry* At(Instance* self, uint index)
    {
        if (index >= self->Count) return null;
        Entry* entry = self->First;
        while (index-- != 0) entry = entry->Next;
        return entry;
    }

    private static nint* ResourceAt(Instance* self, uint index)
    {
        if (index >= self->ResourceCount) return null;
        for (Entry* e = self->First; e != null; e = e->Next)
        {
            if (index < e->Count) return e->Resources + index;
            index -= e->Count;
        }
        return null;
    }

    private static void Retain(nint resource)
    {
        if (resource != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)resource)[1])(resource);
    }

    private static void Free(Entry* entry)
    {
        while (entry != null)
        {
            Entry* next = entry->Next;
            for (uint i = entry->Count; i > 0; i--) Direct3D9Factory.Release(entry->Resources[i - 1]);
            NativeMemory.Free(entry->Resources);
            NativeMemory.Free(entry->Data);
            NativeMemory.Free(entry);
            entry = next;
        }
    }

    private static void ClearCore(Instance* self)
    {
        Entry* first = self->First;
        self->First = self->Last = null;
        self->Count = self->ResourceCount = 0;
        Free(first);
    }

    private static int AddCore(Instance* self, Guid* id, uint size, void* data, uint count, nint* resources)
    {
        if (id == null || (size != 0 && data == null) || (count != 0 && resources == null)) return Invalid;
        for (uint i = 0; i < count; i++) if (resources[i] == 0) return unchecked((int)0x80004003);
        Entry* entry = null;
        try
        {
            if (self->Count == uint.MaxValue || count > uint.MaxValue - self->ResourceCount) return OutOfMemory;
            entry = (Entry*)NativeMemory.AllocZeroed((nuint)sizeof(Entry));
            entry->Id = *id; entry->Size = size;
            if (size != 0)
            {
                entry->Data = (byte*)NativeMemory.Alloc(size);
                Buffer.MemoryCopy(data, entry->Data, size, size);
            }
            if (count != 0)
            {
                nuint bytes = checked((nuint)count * (nuint)sizeof(nint));
                entry->Resources = (nint*)NativeMemory.Alloc(bytes);
                // Copy before foreign AddRef callbacks can mutate caller storage.
                Buffer.MemoryCopy(resources, entry->Resources, (ulong)bytes, (ulong)bytes);
                for (uint i = 0; i < count; i++) { Retain(entry->Resources[i]); entry->Count++; }
            }
            if (self->Last == null) self->First = entry;
            else self->Last->Next = entry;
            self->Last = entry;
            self->Count++;
            self->ResourceCount += count;
            entry = null;
            return 0;
        }
        catch (OutOfMemoryException) { return OutOfMemory; }
        catch (OverflowException) { return OutOfMemory; }
        finally { Free(entry); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(Instance* self, Guid* id, nint* output)
    {
        if (id == null || output == null) return Invalid;
        *output = 0;
        if (*id != new Guid("00000000-0000-0000-c000-000000000046")
            && *id != new Guid("00000400-a8f2-4877-ba0a-fd2b6645fb94")
            && *id != new Guid("b3df038b-b57b-448f-b244-03b55c318277")) return Direct3D9Factory.NoInterfaceHResult;
        Interlocked.Increment(ref self->References);
        *output = (nint)self;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(Instance* self) => (uint)Interlocked.Increment(ref self->References);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(Instance* self)
    {
        int count = Interlocked.Decrement(ref self->References);
        if (count == 0) { ClearCore(self); NativeMemory.Free(self); }
        return (uint)count;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Add(Instance* self, Guid* id, uint size, void* data) => AddCore(self, id, size, data, 0, null);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int AddWithResources(Instance* self, Guid* id, uint size, void* data, uint count, nint* resources)
        => AddCore(self, id, size, data, count, resources);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Clear(Instance* self) => ClearCore(self);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetCount(Instance* self, uint* count) { if (count == null) return Invalid; *count = self->Count; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetId(Instance* self, uint index, Guid* id)
    { Entry* e = At(self, index); if (e == null || id == null) return Invalid; *id = e->Id; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSize(Instance* self, uint index, uint* size)
    { Entry* e = At(self, index); if (e == null || size == null) return Invalid; *size = e->Size; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetParameters(Instance* self, uint index, uint size, void* data)
    {
        Entry* e = At(self, index);
        if (e == null || data == null || size < e->Size) return Invalid;
        if (e->Size != 0) Buffer.MemoryCopy(e->Data, data, size, e->Size);
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetResourceCount(Instance* self, uint index, uint* count)
    { Entry* e = At(self, index); if (e == null || count == null) return Invalid; *count = e->Count; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetResources(Instance* self, uint index, uint count, nint* resources)
    {
        Entry* e = At(self, index);
        if (e == null || resources == null || count != e->Count) return Invalid;
        for (uint i = 0; i < count; i++) resources[i] = e->Resources[i];
        for (uint i = 0; i < count; i++) Retain(resources[i]);
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void GetParamRef(Instance* self, uint index, void** data)
    { if (data != null) { Entry* e = At(self, index); *data = e == null ? null : e->Data; } }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void GetResourcesNoAddRef(Instance* self, uint index, uint count, nint* resources)
    {
        Entry* e = At(self, index);
        if (e == null || resources == null || count != e->Count) return;
        for (uint i = 0; i < count; i++) resources[i] = e->Resources[i];
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetTotalResourceCount(Instance* self, uint* count)
    { if (count == null) return Invalid; *count = self->ResourceCount; return 0; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetResource(Instance* self, uint index, nint* resource)
    {
        nint* slot = ResourceAt(self, index);
        if (slot == null || resource == null) return Invalid;
        nint value = *slot; Retain(value); *resource = value;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int ReplaceResource(Instance* self, uint index, nint resource)
    {
        if (ResourceAt(self, index) == null) return Invalid;
        Retain(resource);
        nint* slot = ResourceAt(self, index);
        if (slot == null) { Direct3D9Factory.Release(resource); return Invalid; }
        nint previous = *slot; *slot = resource;
        Direct3D9Factory.Release(previous);
        return 0;
    }
}
