using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class WicBitmapAdapter
{
    private const int InvalidArgument = unchecked((int)0x80070057);
    private const int NoInterface = unchecked((int)0x80004002);
    private static readonly Guid UnknownId = new("00000000-0000-0000-c000-000000000046");
    private static readonly Guid MilBitmapId = new("c46d6fde-0e59-4cfd-89b1-c935906dfbd9");
    private static readonly Guid MilSourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
    private static readonly Guid WicBitmapId = new("00000121-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly Guid WicSourceId = new("00000120-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly Guid MilLockId = new("d5ec87d4-5fdc-4b77-a924-8c0ede170a2e");

    private struct Instance
    {
        internal void** Table;
        internal void** WicTable;
        internal nint Source;
        internal int References;
        internal uint Token;
    }

    private static class Tables
    {
        internal static readonly void** Mil = CreateTable(false);
        internal static readonly void** Wic = CreateTable(true);
        internal static readonly void** Lock = CreateLockTable();
    }

    internal static int Create(nint source, out nint bitmap)
    {
        bitmap = 0;
        Guid iid = WicBitmapId;
        nint owned = 0;
        try
        {
            int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)source)[0])(source, &iid, &owned);
            if (result < 0) return result;
            if (owned == 0) return NoInterface;
            void** mil = Tables.Mil;
            void** wic = Tables.Wic;
            Instance* value = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            value->Table = mil; value->WicTable = wic; value->Source = owned; value->References = 1;
            owned = 0; bitmap = (nint)value;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        finally { Direct3D9Factory.Release(owned); }
    }

    private static Instance* Owner(nint self) => *(void***)self == Tables.Wic ? (Instance*)(self - sizeof(nint)) : (Instance*)self;

    private static void** CreateTable(bool wic)
    {
        void** t = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(WicBitmapAdapter), 15 * sizeof(nint));
        t[0] = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)&Query;
        t[1] = (delegate* unmanaged[Stdcall]<nint, uint>)&AddRef;
        t[2] = (delegate* unmanaged[Stdcall]<nint, uint>)&Release;
        t[3] = (delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)&Size;
        t[4] = wic ? (void*)(delegate* unmanaged[Stdcall]<nint, Guid*, int>)&WicFormat : (void*)(delegate* unmanaged[Stdcall]<nint, int*, int>)&MilFormat;
        t[5] = (delegate* unmanaged[Stdcall]<nint, double*, double*, int>)&Resolution;
        t[6] = (delegate* unmanaged[Stdcall]<nint, nint, int>)&CopyPalette;
        t[7] = (delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)&CopyPixels;
        t[8] = (delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)&Lock;
        t[9] = (delegate* unmanaged[Stdcall]<nint, nint, int>)&SetPalette;
        t[10] = (delegate* unmanaged[Stdcall]<nint, double, double, int>)&SetResolution;
        t[11] = (delegate* unmanaged[Stdcall]<nint, MilRectL*, int>)&Dirty;
        t[12] = (delegate* unmanaged[Stdcall]<nint, nint*, uint*, uint*, byte>)&DirtyRects;
        t[13] = (delegate* unmanaged[Stdcall]<nint, int>)&SourceState;
        t[14] = (delegate* unmanaged[Stdcall]<nint, uint*, void>)&Token;
        return t;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(nint self, Guid* id, nint* output)
    {
        if (id == null || output == null) return InvalidArgument;
        *output = 0;
        Instance* owner = Owner(self);
        if (*id == UnknownId || *id == MilBitmapId || *id == MilSourceId) *output = (nint)owner;
        else if (*id == WicBitmapId || *id == WicSourceId) *output = (nint)(&owner->WicTable);
        else return NoInterface;
        Interlocked.Increment(ref owner->References);
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(nint self) => (uint)Interlocked.Increment(ref Owner(self)->References);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(nint self)
    {
        Instance* owner = Owner(self);
        int count = Interlocked.Decrement(ref owner->References);
        if (count == 0) { Direct3D9Factory.Release(owner->Source); NativeMemory.Free(owner); }
        return (uint)count;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Size(nint self, uint* w, uint* h)
    {
        nint source = Owner(self)->Source;
        return ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)source)[3])(source, w, h);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int WicFormat(nint self, Guid* format)
    {
        nint source = Owner(self)->Source;
        return ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)source)[4])(source, format);
    }
    private static int GetMilFormat(nint source, int slot, int* format)
    {
        if (format == null) return InvalidArgument;
        Guid guid;
        int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)source)[slot])(source, &guid);
        if (result < 0) return result;
        int code = SoftwareBitmap.FormatCode(guid);
        if (code == 0) return Direct3D9Factory.UnsupportedPixelFormatHResult;
        *format = code;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MilFormat(nint self, int* format) => GetMilFormat(Owner(self)->Source, 4, format);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Resolution(nint self, double* x, double* y)
    {
        nint source = Owner(self)->Source;
        return ((delegate* unmanaged[Stdcall]<nint, double*, double*, int>)(*(void***)source)[5])(source, x, y);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CopyPalette(nint self, nint palette)
    {
        nint source = Owner(self)->Source;
        return ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)source)[6])(source, palette);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CopyPixels(nint self, nint rect, uint stride, uint size, byte* pixels)
    {
        nint source = Owner(self)->Source;
        return ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)source)[7])(source, rect, stride, size, pixels);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Lock(nint self, nint rect, uint flags, nint* output)
    {
        if (output == null) return InvalidArgument;
        *output = 0;
        nint source = Owner(self)->Source, locked = 0;
        try
        {
            int result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)source)[8])(source, rect, flags, &locked);
            if (result < 0) return result;
            if (*(void***)self == Tables.Wic) { *output = locked; locked = 0; return result; }
            void** table = Tables.Lock;
            Instance* value = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            value->Table = table; value->Source = locked; value->References = 1;
            *output = (nint)value; locked = 0;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        finally { Direct3D9Factory.Release(locked); }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetPalette(nint self, nint palette)
    {
        Instance* owner = Owner(self); owner->Token++;
        return ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)owner->Source)[9])(owner->Source, palette);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetResolution(nint self, double x, double y)
    {
        Instance* owner = Owner(self);
        double oldX, oldY;
        int result = ((delegate* unmanaged[Stdcall]<nint, double*, double*, int>)(*(void***)owner->Source)[5])(owner->Source, &oldX, &oldY);
        if (result >= 0 && (x != oldX || y != oldY)) owner->Token++;
        return ((delegate* unmanaged[Stdcall]<nint, double, double, int>)(*(void***)owner->Source)[10])(owner->Source, x, y);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Dirty(nint self, MilRectL* rect)
    {
        Instance* owner = Owner(self);
        uint w, h;
        int result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)owner->Source)[3])(owner->Source, &w, &h);
        if (result < 0) return result;
        if (rect != null && (rect->Left < 0 || rect->Top < 0 || rect->Right <= rect->Left || rect->Bottom <= rect->Top || rect->Right > w || rect->Bottom > h)) return InvalidArgument;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static byte DirtyRects(nint self, nint* rects, uint* count, uint* token)
    {
        if (rects == null || count == null || token == null) return 0;
        uint current = Owner(self)->Token;
        byte valid = *token == current ? (byte)1 : (byte)0;
        *rects = 0; *count = 0; *token = current;
        return valid;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SourceState(nint self) => 0;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Token(nint self, uint* token) { if (token != null) *token = Owner(self)->Token; }

    private static void** CreateLockTable()
    {
        void** t = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(WicBitmapAdapter), 7 * sizeof(nint));
        t[0] = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)&LockQuery;
        t[1] = (delegate* unmanaged[Stdcall]<nint, uint>)&AddRef;
        t[2] = (delegate* unmanaged[Stdcall]<nint, uint>)&Release;
        t[3] = (delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)&Size;
        t[4] = (delegate* unmanaged[Stdcall]<nint, uint*, int>)&LockStride;
        t[5] = (delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)&LockData;
        t[6] = (delegate* unmanaged[Stdcall]<nint, int*, int>)&LockFormat;
        return t;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockQuery(nint self, Guid* id, nint* output)
    {
        if (id == null || output == null) return InvalidArgument;
        *output = 0;
        if (*id != UnknownId && *id != MilLockId) return NoInterface;
        Interlocked.Increment(ref ((Instance*)self)->References); *output = self;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockStride(nint self, uint* stride)
    {
        nint source = ((Instance*)self)->Source;
        return ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)source)[4])(source, stride);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockData(nint self, uint* size, byte** pixels)
    {
        nint source = ((Instance*)self)->Source;
        return ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)source)[5])(source, size, pixels);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockFormat(nint self, int* format) => GetMilFormat(((Instance*)self)->Source, 6, format);
}
