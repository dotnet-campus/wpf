using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

// Software virtual contract and consumed Release/x64 bitmap prefix, not a complete MSVC object.
internal static unsafe class NativeImmediateSolidRealization
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Instance
    {
        internal void** Table;
        internal int References;
        internal int IsNull;
        internal void** BrushTable;
        internal float Red, Green, Blue, Alpha;
        internal nint BitmapBrush;
    }

    [StructLayout(LayoutKind.Explicit, Size = 384)]
    private struct BitmapBrush
    {
        [FieldOffset(0)] internal void** Table;
        [FieldOffset(76)] internal uint Space;
        [FieldOffset(80)] internal System.Numerics.Matrix4x4 Mapping;
        [FieldOffset(160)] internal System.Numerics.Vector2 Clip0;
        [FieldOffset(168)] internal System.Numerics.Vector2 Clip1;
        [FieldOffset(176)] internal System.Numerics.Vector2 Clip2;
        [FieldOffset(184)] internal System.Numerics.Vector2 Clip3;
        [FieldOffset(200)] internal int HasSourceClip;
        [FieldOffset(208)] internal nint Source;
        [FieldOffset(216)] internal uint Wrap;
        [FieldOffset(236)] internal float Opacity;
        [FieldOffset(240)] internal GeneratedImageTransform ExactMapping;
        [FieldOffset(288)] internal MilRectD ExactSourceClip;
        [FieldOffset(320)] internal GeneratedImageTransform ExactClipMapping;
        [FieldOffset(368)] internal int HasExactClipMapping;
        [FieldOffset(376)] internal SoftwareImageWorkingFormat WorkingFormat;
    }

    private static readonly void** RealizerTable = CreateRealizerTable();
    private static readonly void** SolidTable = CreateSolidTable();
    private static readonly void** BitmapTable = CreateBitmapTable();

    internal static nint CreateBitmap(nint source, GeneratedImageTransform? mapping = null,
        MilRectD? sourceClip = null, float opacity = 1, MilBitmapWrapMode wrapMode = MilBitmapWrapMode.Extend,
        GeneratedImageTransform? clipMapping = null,
        SoftwareImageWorkingFormat workingFormat = SoftwareImageWorkingFormat.Native)
    {
        BitmapBrush* brush = (BitmapBrush*)NativeMemory.AllocZeroed((nuint)sizeof(BitmapBrush));
        nint realization = 0;
        try
        {
            brush->Table = BitmapTable;
            brush->Mapping = System.Numerics.Matrix4x4.Identity;
            brush->ExactMapping = mapping ?? GeneratedImageTransform.Identity;
            brush->WorkingFormat = workingFormat;
            brush->Wrap = (uint)MilBitmapWrapMode.Border;
            brush->Opacity = opacity;
            if (mapping is { } transform)
            {
                brush->Mapping.M11 = (float)transform.ScaleX; brush->Mapping.M22 = (float)transform.ScaleY;
                brush->Mapping.M12 = (float)transform.M12; brush->Mapping.M21 = (float)transform.M21;
                brush->Mapping.M41 = (float)transform.X; brush->Mapping.M42 = (float)transform.Y;
                brush->Wrap = (uint)wrapMode;
                if (sourceClip is { } rectangle)
                {
                    brush->ExactSourceClip = rectangle;
                    if (clipMapping is { } independentClip)
                    {
                        brush->ExactClipMapping = independentClip;
                        brush->HasExactClipMapping = 1;
                    }
                    var clipTransform = clipMapping ?? transform;
                    System.Numerics.Vector2 Point(double x, double y)
                    {
                        var point = clipTransform.Apply(x, y);
                        return new((float)point.X, (float)point.Y);
                    }
                    brush->Clip0 = Point(rectangle.X, rectangle.Y);
                    brush->Clip1 = Point(rectangle.X + rectangle.Width, rectangle.Y);
                    brush->Clip2 = Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
                    brush->Clip3 = Point(rectangle.X, rectangle.Y + rectangle.Height);
                    brush->HasSourceClip = 1;
                }
            }
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source)[1])(source);
            brush->Source = source;
            realization = Create(null);
            ((Instance*)realization)->BitmapBrush = (nint)brush;
            return realization;
        }
        finally
        {
            if (realization == 0)
            {
                Direct3D9Factory.Release(brush->Source);
                NativeMemory.Free(brush);
            }
        }
    }

    internal static bool TryGetExactMapping(nint brush, out GeneratedImageTransform mapping)
    {
        mapping = default;
        if (*(void***)brush != BitmapTable) return false;
        // Only our allocation has this tail; foreign CMILBrush objects retain the native prefix contract.
        mapping = ((BitmapBrush*)brush)->ExactMapping;
        return true;
    }

    internal static bool TryGetExactSourceClip(nint brush, out MilRectD rectangle)
    {
        rectangle = default;
        if (*(void***)brush != BitmapTable || ((BitmapBrush*)brush)->HasSourceClip == 0) return false;
        rectangle = ((BitmapBrush*)brush)->ExactSourceClip;
        return true;
    }

    internal static bool TryGetExactClipMapping(nint brush, out GeneratedImageTransform mapping)
    {
        mapping = default;
        if (*(void***)brush != BitmapTable || ((BitmapBrush*)brush)->HasExactClipMapping == 0) return false;
        mapping = ((BitmapBrush*)brush)->ExactClipMapping;
        return true;
    }

    internal static SoftwareImageWorkingFormat GetWorkingFormat(nint brush)
        => *(void***)brush == BitmapTable ? ((BitmapBrush*)brush)->WorkingFormat : SoftwareImageWorkingFormat.Native;

    private static void Destroy(Instance* self)
    {
        BitmapBrush* brush = (BitmapBrush*)self->BitmapBrush;
        self->BitmapBrush = 0;
        if (brush != null)
        {
            nint source = brush->Source;
            brush->Source = 0;
            NativeMemory.Free(brush);
            Direct3D9Factory.Release(source);
        }
        NativeMemory.Free(self);
    }

    internal static nint Create(MilColorF? color)
    {
        Instance* instance = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
        instance->Table = RealizerTable;
        instance->BrushTable = SolidTable;
        instance->IsNull = color is null ? 1 : 0;
        if (color is { } value)
        {
            instance->Red = value.Red; instance->Green = value.Green;
            instance->Blue = value.Blue; instance->Alpha = value.Alpha;
        }
        // Native creators acquire the first reference after the zero-count constructor.
        ((delegate* unmanaged[Stdcall]<Instance*, uint>)RealizerTable[0])(instance);
        return (nint)instance;
    }

    internal static void ReleaseOwner(nint realization)
    {
        if (realization != 0)
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)realization)[1])(realization);
    }

    private static void** CreateRealizerTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(NativeImmediateSolidRealization), 12 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&AddRef;
        table[1] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&Release;
        table[2] = (delegate* unmanaged<Instance*, uint, nint>)&Delete;
        table[3] = (delegate* unmanaged<Instance*, nint, byte>)&NoTiling;
        table[4] = (delegate* unmanaged<Instance*, byte>)&NoClip;
        table[5] = (delegate* unmanaged<Instance*, nint, byte>)&NoTiling;
        table[6] = (delegate* unmanaged<Instance*, uint, uint, nint, nint, nint, int>)&Ensure;
        table[7] = (delegate* unmanaged<Instance*, void>)&NoFixups;
        table[8] = (delegate* unmanaged<Instance*, void>)&NoFixups;
        table[9] = (delegate* unmanaged<Instance*, byte, nint>)&GetBrush;
        table[10] = (delegate* unmanaged<Instance*, nint*, int>)&GetEffects;
        table[11] = (delegate* unmanaged<Instance*, nint>)&GetBrushCore;
        return table;
    }

    private static void** CreateSolidTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(NativeImmediateSolidRealization), 6 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)&QueryBrush;
        table[1] = (delegate* unmanaged[Stdcall]<nint, uint>)&BorrowedReference;
        table[2] = (delegate* unmanaged[Stdcall]<nint, uint>)&BorrowedReference;
        table[3] = (delegate* unmanaged<nint, uint>)&GetType;
        table[4] = (delegate* unmanaged<nint, byte>)&BrushNoTiling;
        table[5] = (delegate* unmanaged<nint, int>)&ZeroAlpha;
        return table;
    }

    private static void** CreateBitmapTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(NativeImmediateSolidRealization), 6 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)&QueryBrush;
        table[1] = (delegate* unmanaged[Stdcall]<nint, uint>)&BorrowedReference;
        table[2] = (delegate* unmanaged[Stdcall]<nint, uint>)&BorrowedReference;
        table[3] = (delegate* unmanaged<nint, uint>)&GetBitmapType;
        table[4] = (delegate* unmanaged<nint, byte>)&BrushNoTiling;
        table[5] = (delegate* unmanaged<nint, int>)&BitmapZeroAlpha;
        return table;
    }

    [UnmanagedCallersOnly]
    private static uint GetBitmapType(nint self) => 4;
    [UnmanagedCallersOnly]
    private static int BitmapZeroAlpha(nint self) => ((BitmapBrush*)self)->Opacity == 0 ? 1 : 0;

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
        => cache == 0 || cache == uint.MaxValue ? 0 : Direct3D9Factory.NotImplementedHResult;
    [UnmanagedCallersOnly]
    private static void NoFixups(Instance* self) { }
    [UnmanagedCallersOnly]
    private static nint GetBrush(Instance* self, byte transparent)
        => self->BitmapBrush != 0 ? self->BitmapBrush
            : self->IsNull == 0 || transparent != 0 ? (nint)(&self->BrushTable) : 0;
    [UnmanagedCallersOnly]
    private static nint GetBrushCore(Instance* self) => self->BitmapBrush != 0 ? self->BitmapBrush
        : self->IsNull == 0 ? (nint)(&self->BrushTable) : 0;
    [UnmanagedCallersOnly]
    private static int GetEffects(Instance* self, nint* effects)
    {
        if (effects == null) return unchecked((int)0x80004003);
        *effects = 0;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryBrush(nint self, Guid* id, nint* output)
    {
        if (output == null) return unchecked((int)0x80004003);
        *output = 0;
        if (id == null) return unchecked((int)0x80004003);
        if (*id != new Guid("00000000-0000-0000-c000-000000000046")) return Direct3D9Factory.NoInterfaceHResult;
        *output = self;
        return 0;
    }
    // LocalMILObject references are scope-bound and never delete the embedded brush.
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint BorrowedReference(nint self) => 1;
    [UnmanagedCallersOnly]
    private static uint GetType(nint self) => 1;
    [UnmanagedCallersOnly]
    private static byte BrushNoTiling(nint self) => 0;
    [UnmanagedCallersOnly]
    private static int ZeroAlpha(nint self) => ((float*)(self + sizeof(nint)))[3] == 0 ? 1 : 0;
}
