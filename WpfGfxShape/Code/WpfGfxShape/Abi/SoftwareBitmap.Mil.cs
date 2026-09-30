using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe partial class SoftwareBitmap
{
    private static readonly Guid MilBitmapId = new("c46d6fde-0e59-4cfd-89b1-c935906dfbd9");
    private static readonly Guid MilSourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
    private static readonly Guid MilLockId = new("d5ec87d4-5fdc-4b77-a924-8c0ede170a2e");
    private static Bitmap* FromMil(nint self) => (Bitmap*)(self - sizeof(nint));
    private static BitmapLock* FromMilLock(nint self) => (BitmapLock*)(self - sizeof(nint));

    private static class MilTables
    {
        internal static readonly void** Bitmap = CreateMilTable();
        internal static readonly void** Lock = CreateMilLockTable();
    }

    private static void** CreateMilTable()
    {
        void** t = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(SoftwareBitmap), 15 * sizeof(nint));
        t[0] = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)&MilQuery;
        t[1] = (delegate* unmanaged[Stdcall]<nint, uint>)&MilAddRef;
        t[2] = (delegate* unmanaged[Stdcall]<nint, uint>)&MilRelease;
        t[3] = (delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)&MilSize;
        t[4] = (delegate* unmanaged[Stdcall]<nint, int*, int>)&MilFormat;
        t[5] = (delegate* unmanaged[Stdcall]<nint, double*, double*, int>)&MilResolution;
        t[6] = (delegate* unmanaged[Stdcall]<nint, nint, int>)&MilCopyPalette;
        t[7] = (delegate* unmanaged[Stdcall]<nint, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int>)&MilCopyPixels;
        t[8] = (delegate* unmanaged[Stdcall]<nint, Direct3D9BitmapSourceRectangle*, uint, nint*, int>)&MilLock;
        t[9] = (delegate* unmanaged[Stdcall]<nint, nint, int>)&MilSetPalette;
        t[10] = (delegate* unmanaged[Stdcall]<nint, double, double, int>)&MilSetResolution;
        t[11] = (delegate* unmanaged[Stdcall]<nint, MilRectL*, int>)&MilDirty;
        t[12] = (delegate* unmanaged[Stdcall]<nint, MilRectL**, uint*, uint*, byte>)&MilDirtyRects;
        t[13] = (delegate* unmanaged[Stdcall]<nint, int>)&MilSourceState;
        t[14] = (delegate* unmanaged[Stdcall]<nint, uint*, void>)&MilToken;
        return t;
    }

    private static void** CreateMilLockTable()
    {
        void** t = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(SoftwareBitmap), 7 * sizeof(nint));
        t[0] = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)&MilLockQuery;
        t[1] = (delegate* unmanaged[Stdcall]<nint, uint>)&MilLockAddRef;
        t[2] = (delegate* unmanaged[Stdcall]<nint, uint>)&MilLockRelease;
        t[3] = (delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)&MilLockSize;
        t[4] = (delegate* unmanaged[Stdcall]<nint, uint*, int>)&MilLockStride;
        t[5] = (delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)&MilLockData;
        t[6] = (delegate* unmanaged[Stdcall]<nint, int*, int>)&MilLockFormat;
        return t;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilQuery(nint self, Guid* id, nint* output) => ((delegate* unmanaged[Stdcall]<Bitmap*, Guid*, nint*, int>)&Query)(FromMil(self), id, output);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint MilAddRef(nint self) => ((delegate* unmanaged[Stdcall]<Bitmap*, uint>)&AddRef)(FromMil(self));
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint MilRelease(nint self) => ReleaseReference((nint)FromMil(self));
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilSize(nint self, uint* w, uint* h) => ((delegate* unmanaged[Stdcall]<Bitmap*, uint*, uint*, int>)&GetSize)(FromMil(self), w, h);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilFormat(nint self, int* format)
    {
        if (format == null) return InvalidArgument;
        *format = FormatCode(FromMil(self)->Format); return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilResolution(nint self, double* x, double* y) => ((delegate* unmanaged[Stdcall]<Bitmap*, double*, double*, int>)&GetResolution)(FromMil(self), x, y);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilCopyPalette(nint self, nint palette) => ((delegate* unmanaged[Stdcall]<Bitmap*, nint, int>)&CopyPalette)(FromMil(self), palette);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilCopyPixels(nint self, Direct3D9BitmapSourceRectangle* r, uint stride, uint size, byte* pixels) => ((delegate* unmanaged[Stdcall]<Bitmap*, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int>)&CopyPixels)(FromMil(self), r, stride, size, pixels);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilLock(nint self, Direct3D9BitmapSourceRectangle* r, uint flags, nint* output)
    {
        if (output == null) return InvalidArgument;
        *output = 0;
        nint value = 0;
        int result = ((delegate* unmanaged[Stdcall]<Bitmap*, Direct3D9BitmapSourceRectangle*, uint, nint*, int>)&Lock)(FromMil(self), r, flags, &value);
        if (result >= 0) *output = (nint)(&((BitmapLock*)value)->MilVtable);
        return result;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilSetPalette(nint self, nint palette) => ((delegate* unmanaged[Stdcall]<Bitmap*, nint, int>)&SetPalette)(FromMil(self), palette);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilSetResolution(nint self, double x, double y) => ((delegate* unmanaged[Stdcall]<Bitmap*, double, double, int>)&SetResolution)(FromMil(self), x, y);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilDirty(nint self, MilRectL* r)
    {
        Bitmap* b = FromMil(self);
        b->Uniqueness++;
        if (r != null && (r->Left < 0 || r->Top < 0 || r->Right <= r->Left || r->Bottom <= r->Top || r->Right > b->Width || r->Bottom > b->Height)) return InvalidArgument;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static byte MilDirtyRects(nint self, MilRectL** rectangles, uint* count, uint* uniqueness)
    {
        if (rectangles == null || count == null || uniqueness == null) return 0;
        // This bitmap has no native resource-cache entries; changed tokens require a full update.
        uint current = FromMil(self)->Uniqueness;
        byte valid = *uniqueness == current ? (byte)1 : (byte)0;
        *rectangles = null; *count = 0; *uniqueness = current;
        return valid;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilSourceState(nint self) => 0;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void MilToken(nint self, uint* token) { if (token != null) *token = FromMil(self)->Uniqueness; }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilLockQuery(nint self, Guid* id, nint* output) => ((delegate* unmanaged[Stdcall]<BitmapLock*, Guid*, nint*, int>)&QueryLock)(FromMilLock(self), id, output);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint MilLockAddRef(nint self) => ((delegate* unmanaged[Stdcall]<BitmapLock*, uint>)&AddRefLock)(FromMilLock(self));
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint MilLockRelease(nint self) => ((delegate* unmanaged[Stdcall]<BitmapLock*, uint>)&ReleaseLock)(FromMilLock(self));
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilLockSize(nint self, uint* w, uint* h) => ((delegate* unmanaged[Stdcall]<BitmapLock*, uint*, uint*, int>)&GetLockSize)(FromMilLock(self), w, h);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilLockStride(nint self, uint* stride) => ((delegate* unmanaged[Stdcall]<BitmapLock*, uint*, int>)&GetStride)(FromMilLock(self), stride);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilLockData(nint self, uint* size, byte** pixels) => ((delegate* unmanaged[Stdcall]<BitmapLock*, uint*, byte**, int>)&GetData)(FromMilLock(self), size, pixels);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilLockFormat(nint self, int* format)
    {
        if (format == null) return InvalidArgument;
        *format = FormatCode(FromMilLock(self)->Owner->Format); return 0;
    }
}
