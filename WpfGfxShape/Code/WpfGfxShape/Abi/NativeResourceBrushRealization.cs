using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

// Implements the software virtual contract; not a complete MSVC resource object.
internal static unsafe class NativeResourceBrushRealization
{
    private sealed record State(SoftwareBrushRealization Brush, SoftwareImageRenderSession Session);

    [StructLayout(LayoutKind.Sequential)]
    private struct Instance
    {
        internal void** Table;
        internal int References;
        internal nint State;
    }

    private static readonly void** Table = CreateTable();

    internal static nint Create(SoftwareBrushRealization brush, SoftwareImageRenderSession session)
    {
        Instance* instance = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
        bool initialized = false;
        try
        {
            instance->State = GCHandle.ToIntPtr(GCHandle.Alloc(new State(brush, session)));
            instance->Table = Table;
            instance->References = 1;
            initialized = true;
            return (nint)instance;
        }
        finally
        {
            if (!initialized) NativeMemory.Free(instance);
        }
    }

    private static State GetState(Instance* self) => (State)GCHandle.FromIntPtr(self->State).Target!;

    private static void Destroy(Instance* self)
    {
        nint state = self->State;
        self->State = 0;
        NativeMemory.Free(self);
        GCHandle.FromIntPtr(state).Free();
    }

    private static void** CreateTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(NativeResourceBrushRealization), 12 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&AddRef;
        table[1] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&Release;
        table[2] = (delegate* unmanaged<Instance*, uint, nint>)&Delete;
        table[3] = (delegate* unmanaged<Instance*, nint, byte>)&NoTiling;
        table[4] = (delegate* unmanaged<Instance*, byte>)&NoClip;
        table[5] = (delegate* unmanaged<Instance*, nint, byte>)&NoTiling;
        table[6] = (delegate* unmanaged<Instance*, uint, uint, nint, nint, nint, int>)&Ensure;
        table[7] = (delegate* unmanaged<Instance*, void>)&Restore;
        table[8] = (delegate* unmanaged<Instance*, void>)&Free;
        table[9] = (delegate* unmanaged<Instance*, byte, nint>)&GetBrush;
        table[10] = (delegate* unmanaged<Instance*, nint*, int>)&GetEffects;
        table[11] = (delegate* unmanaged<Instance*, nint>)&GetBrushCore;
        return table;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(Instance* self) => (uint)Interlocked.Increment(ref self->References);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(Instance* self)
    {
        int count = Interlocked.Decrement(ref self->References);
        if (count == 0) Destroy(self);
        return (uint)count;
    }

    [UnmanagedCallersOnly]
    private static nint Delete(Instance* self, uint flags)
    {
        nint result = (nint)self;
        if ((flags & 1) != 0) Destroy(self);
        return result;
    }

    [UnmanagedCallersOnly]
    private static byte NoTiling(Instance* self, nint context) => 0;

    [UnmanagedCallersOnly]
    private static byte NoClip(Instance* self) => 0;

    [UnmanagedCallersOnly]
    private static int Ensure(Instance* self, uint cache, uint display, nint brushContext, nint context, nint creator)
    {
        if (cache is not (0 or uint.MaxValue)) return Direct3D9Factory.NotImplementedHResult;
        var state = GetState(self);
        try { return state.Brush.EnsureRealization(state.Session); }
        catch (OverflowException) { return Direct3D9Factory.InvalidArgumentHResult; }
    }

    // Software intermediates are ordinary bitmaps, so there is no meta texture to restore.
    [UnmanagedCallersOnly]
    private static void Restore(Instance* self) { }

    [UnmanagedCallersOnly]
    private static void Free(Instance* self) => GetState(self).Brush.FreeRealizationResources();

    private static nint BorrowBrush(Instance* self, byte transparent)
    {
        nint immediate = GetState(self).Brush.Immediate;
        return immediate == 0 ? 0
            : ((delegate* unmanaged<nint, byte, nint>)(*(void***)immediate)[9])(immediate, transparent);
    }

    [UnmanagedCallersOnly]
    private static nint GetBrush(Instance* self, byte transparent) => BorrowBrush(self, transparent);

    [UnmanagedCallersOnly]
    private static nint GetBrushCore(Instance* self) => BorrowBrush(self, 0);

    [UnmanagedCallersOnly]
    private static int GetEffects(Instance* self, nint* effects)
    {
        if (effects == null) return unchecked((int)0x80004003);
        *effects = 0;
        return 0;
    }
}
