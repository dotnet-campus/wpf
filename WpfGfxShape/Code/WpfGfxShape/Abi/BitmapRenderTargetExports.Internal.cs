using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe partial class BitmapRenderTargetExports
{
    private static readonly Guid InternalTargetId = new("b73b1159-a295-4c76-bb56-c18e282ae007");

    [StructLayout(LayoutKind.Sequential)]
    private struct InternalView
    {
        internal void** Table;
        internal void** CreatorTable;
        internal int UsedHardware;
        internal Instance* Owner;
        internal System.Numerics.Matrix4x4 DeviceTransform;
        internal byte ForceClearType;
    }

    private static class InternalTables
    {
        internal static readonly void** Main = CreateInternalTable();
        internal static readonly void** Creator = CreateCreatorTable();
    }

    private static void InitializeInternalView(Instance* owner)
    {
        if (owner->Internal.Table != null) return;
        owner->Internal.Owner = owner;
        owner->Internal.DeviceTransform = System.Numerics.Matrix4x4.Identity;
        owner->Internal.CreatorTable = InternalTables.Creator;
        owner->Internal.Table = InternalTables.Main;
    }

    private static void** CreateInternalTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(InternalView), 23 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<InternalView*, Guid*, nint*, int>)&InternalQuery;
        table[1] = (delegate* unmanaged[Stdcall]<InternalView*, uint>)&InternalAddRef;
        table[2] = (delegate* unmanaged[Stdcall]<InternalView*, uint>)&InternalRelease;
        table[3] = (delegate* unmanaged[Stdcall]<InternalView*, float*, void>)&InternalBounds;
        table[4] = (delegate* unmanaged[Stdcall]<InternalView*, float*, nint, int>)&InternalClear;
        table[5] = (delegate* unmanaged[Stdcall]<InternalView*, nint, uint, byte, float, int>)&InternalBegin3D;
        table[6] = (delegate* unmanaged[Stdcall]<InternalView*, int>)&InternalEnd3D;
        table[7] = (delegate* unmanaged[Stdcall]<InternalView*, nint>)&InternalTransform;
        table[8] = (delegate* unmanaged[Stdcall]<InternalView*, nint, nint, nint, int>)&InternalBitmap;
        table[9] = (delegate* unmanaged[Stdcall]<InternalView*, nint, nint, nint, nint, nint, int>)&InternalMesh;
        table[10] = (delegate* unmanaged[Stdcall]<InternalView*, nint, nint, nint, nint, nint, nint, int>)&InternalPath;
        table[11] = (delegate* unmanaged[Stdcall]<InternalView*, nint, nint, nint, int>)&InternalInfinitePath;
        table[12] = (delegate* unmanaged[Stdcall]<InternalView*, nint, nint, nint, uint, uint, nint, int>)&InternalEffect;
        table[13] = (delegate* unmanaged[Stdcall]<InternalView*, nint, int>)&InternalGlyphs;
        table[14] = (delegate* unmanaged[Stdcall]<InternalView*, nint, nint, nint, nint, int>)&InternalVideo;
        table[15] = (delegate* unmanaged[Stdcall]<InternalView*, nint, uint, nint, nint, float, nint, int>)&InternalBeginLayer;
        table[16] = (delegate* unmanaged[Stdcall]<InternalView*, int>)&InternalEndLayer;
        table[17] = (delegate* unmanaged[Stdcall]<InternalView*, void>)&InternalAbortLayers;
        table[18] = (delegate* unmanaged[Stdcall]<InternalView*, uint*, int>)&InternalType;
        table[19] = (delegate* unmanaged[Stdcall]<InternalView*, byte, int>)&InternalClearType;
        table[20] = (delegate* unmanaged[Stdcall]<InternalView*, uint>)&InternalCacheIndex;
        table[21] = (delegate* unmanaged[Stdcall]<InternalView*, uint*, int>)&InternalQueued;
        table[22] = (delegate* unmanaged[Stdcall]<InternalView*, nint>)&InternalMeta;
        return table;
    }

    private static void** CreateCreatorTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(InternalView), 2 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<nint, uint, uint, ulong, uint, nint*, nint, int>)&InternalCreateIntermediate;
        table[1] = (delegate* unmanaged[Stdcall]<nint, nint, int>)&InternalDisplays;
        return table;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalQuery(InternalView* self, Guid* id, nint* output)
        => ((delegate* unmanaged[Stdcall]<Instance*, Guid*, nint*, int>)self->Owner->Vtable[0])(self->Owner, id, output);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint InternalAddRef(InternalView* self)
        => ((delegate* unmanaged[Stdcall]<Instance*, uint>)self->Owner->Vtable[1])(self->Owner);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint InternalRelease(InternalView* self)
        => ((delegate* unmanaged[Stdcall]<Instance*, uint>)self->Owner->Vtable[2])(self->Owner);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void InternalBounds(InternalView* self, float* bounds)
        => ((delegate* unmanaged[Stdcall]<Instance*, float*, void>)self->Owner->Vtable[3])(self->Owner, bounds);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalClear(InternalView* self, float* color, nint clip)
        => ((delegate* unmanaged[Stdcall]<Instance*, float*, nint, int>)self->Owner->Vtable[4])(self->Owner, color, clip);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalBegin3D(InternalView* self, nint bounds, uint mode, byte useZ, float z) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalEnd3D(InternalView* self) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static nint InternalTransform(InternalView* self) => (nint)(&self->DeviceTransform);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static nint InternalMeta(InternalView* self) => 0;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalBitmap(InternalView* self, nint context, nint bitmap, nint effects)
    {
        try { return DrawInternalBitmap(self, context, bitmap, effects); }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (OverflowException) { return InvalidArgument; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalInfinitePath(InternalView* self, nint context, nint brushContext, nint brush) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalMesh(InternalView* self, nint context, nint brush, nint mesh, nint shader, nint effects) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalPath(InternalView* self, nint context, nint brushContext, nint path, nint pen, nint stroke, nint fill) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalEffect(InternalView* self, nint context, nint scale, nint effect, uint width, uint height, nint input) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalGlyphs(InternalView* self, nint parameters) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalVideo(InternalView* self, nint context, nint renderer, nint source, nint effects) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalBeginLayer(InternalView* self, nint bounds, uint mode, nint mask, nint matrix, float alpha, nint brush) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalEndLayer(InternalView* self) => WpfGfxShape.Core.Direct3D9Factory.WgxInvalidCallHResult;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void InternalAbortLayers(InternalView* self) { }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalType(InternalView* self, uint* type)
    {
        if (type == null) return InvalidArgument;
        *type = 0x400;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalClearType(InternalView* self, byte force)
    {
        self->ForceClearType = force != 0 ? (byte)1 : (byte)0;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint InternalCacheIndex(InternalView* self) => 0;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalQueued(InternalView* self, uint* count)
        => ((delegate* unmanaged[Stdcall]<Instance*, uint*, int>)self->Owner->Vtable[10])(self->Owner, count);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalCreateIntermediate(nint self, uint width, uint height, ulong usage, uint flags, nint* output, nint displays)
    {
        if (output == null) return InvalidArgument;
        *output = 0;
        if (width > (1u << 24) || height > (1u << 24))
            return WpfGfxShape.Core.Direct3D9Factory.UnsupportedTextureSizeHResult;
        InternalView* view = (InternalView*)(self - sizeof(nint));
        nint source = view->Owner->Bitmap;
        Guid format;
        int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)source)[4])(source, &format);
        if (result < 0) return result;
        uint code = (uint)SoftwareBitmap.FormatCode(format);
        // Both supported target formats are already premultiplied blending formats.
        if (code is not (16 or 26)) return WpfGfxShape.Core.Direct3D9Factory.UnsupportedPixelFormatHResult;
        return CreateBitmapTarget(width, height, code, 96, 96, output);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int InternalDisplays(nint self, nint displays) => NotImplemented;
}
