using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe partial class SoftwareBitmap
{
    private const int InvalidArgument = unchecked((int)0x80070057);
    private const int PointerError = unchecked((int)0x80004003);
    private const int AlreadyLocked = unchecked((int)0x88982F0D);
    private const int UnsupportedFormat = unchecked((int)0x88982F80);
    private const int OutOfMemory = unchecked((int)0x8007000E);
    private static readonly Guid UnknownId = new("00000000-0000-0000-c000-000000000046");
    private static readonly Guid BitmapId = new("00000121-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly Guid SourceId = new("00000120-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly Guid LockId = new("00000123-a8f2-4877-ba0a-fd2b6645fb94");

    [StructLayout(LayoutKind.Sequential)]
    internal struct Bitmap
    {
        internal void** Vtable;
        internal void** MilVtable;
        internal int References;
        internal uint Width, Height, Stride, Size, BitsPerPixel, Uniqueness;
        internal nint Palette;
        internal Guid Format;
        internal double DpiX, DpiY;
        internal byte* Pixels;
        internal int Locks;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapLock
    {
        internal void** Vtable;
        internal void** MilVtable;
        internal int References;
        internal byte* Temporary;
        internal uint Stride, Size;
        internal Bitmap* Owner;
        internal Direct3D9BitmapSourceRectangle Rectangle;
        internal uint Flags;
    }

    private static class Tables
    {
        internal static readonly void** Bitmap = CreateBitmapTable();
        internal static readonly void** Lock = CreateLockTable();
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial void* VirtualAlloc(void* address, nuint size, uint allocation, uint protection);
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int VirtualFree(void* address, nuint size, uint freeType);
    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial int VirtualProtect(void* address, nuint size, uint protection, uint* previous);

    private static int LastError() => Marshal.GetHRForLastWin32Error();

    internal static int Create(uint width, uint height, double dpiX, double dpiY, Guid format, out nint result, nint palette = 0)
    {
        result = 0;
        if (width == 0 || height == 0 || width > int.MaxValue || height > int.MaxValue) return InvalidArgument;
        int code = FormatCode(format);
        int bits = MilPixelFormatInfo.GetBitsPerPixel((MilPixelFormat)code);
        if (bits == 0) return UnsupportedFormat;
        bool indexed = code is >= 1 and <= 4;
        if (indexed && palette == 0) return PointerError;
        if (!indexed && palette != 0) return InvalidArgument;
        ulong rowBytes = ((ulong)width * (uint)bits + 7) / 8;
        ulong stride = (rowBytes + 3) & ~3UL;
        ulong size = stride * (height - 1) + rowBytes;
        ulong page = (uint)Environment.SystemPageSize;
        ulong allocation = ((size + page - 1) / page + 1) * page;
        if (size > uint.MaxValue || allocation > uint.MaxValue) return unchecked((int)0x80070216);
        Bitmap* bitmap = null;
        try
        {
            void** table = Tables.Bitmap;
            bitmap = (Bitmap*)NativeMemory.AllocZeroed((nuint)sizeof(Bitmap));
            bitmap->Pixels = (byte*)VirtualAlloc(null, (nuint)allocation, 0x3000, 2);
            if (bitmap->Pixels == null) return LastError();
            uint previous;
            if (VirtualProtect(bitmap->Pixels, (nuint)size, 4, &previous) == 0) return LastError();
            bitmap->Vtable = table;
            bitmap->References = 1;
            bitmap->Width = width;
            bitmap->Height = height;
            bitmap->Stride = (uint)stride;
            bitmap->Size = (uint)size;
            bitmap->BitsPerPixel = (uint)bits;
            bitmap->MilVtable = MilTables.Bitmap;
            bitmap->Uniqueness = 1;
            if (indexed)
            {
                int paletteResult = BitmapFormatConverter.ClonePalette(palette, out nint copy);
                if (paletteResult < 0) return paletteResult;
                bitmap->Palette = copy;
            }
            bitmap->Format = format;
            bitmap->DpiX = (float)dpiX;
            bitmap->DpiY = (float)dpiY;
            result = (nint)bitmap;
            bitmap = null;
            return 0;
        }
        catch (OutOfMemoryException) { return OutOfMemory; }
        finally
        {
            if (bitmap != null)
            {
                Direct3D9Factory.Release(bitmap->Palette);
                if (bitmap->Pixels != null) VirtualFree(bitmap->Pixels, 0, 0x8000);
                NativeMemory.Free(bitmap);
            }
        }
    }

    internal static int FormatCode(Guid format)
    {
        Span<byte> bytes = stackalloc byte[16];
        format.TryWriteBytes(bytes);
        int code = bytes[15];
        bytes[15] = 0;
        return code <= 0x1c && new Guid(bytes) == new Guid("6fddc324-4e03-4bfe-b185-3d77768dc900") ? code : 0;
    }

    internal static int Protect(nint bitmap)
    {
        uint previous;
        Bitmap* value = (Bitmap*)bitmap;
        return VirtualProtect(value->Pixels, value->Size, 2, &previous) != 0 ? 0 : LastError();
    }

    internal static void Retain(nint bitmap) => Interlocked.Increment(ref ((Bitmap*)bitmap)->References);

    internal static uint ReleaseReference(nint bitmap)
    {
        if (bitmap == 0) return 0;
        Bitmap* value = (Bitmap*)bitmap;
        int count = Interlocked.Decrement(ref value->References);
        if (count == 0)
        {
            Direct3D9Factory.Release(value->Palette);
            VirtualFree(value->Pixels, 0, 0x8000);
            NativeMemory.Free(value);
        }
        return (uint)count;
    }

    private static void** CreateBitmapTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(SoftwareBitmap), 11 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Bitmap*, Guid*, nint*, int>)&Query;
        table[1] = (delegate* unmanaged[Stdcall]<Bitmap*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<Bitmap*, uint>)&Release;
        table[3] = (delegate* unmanaged[Stdcall]<Bitmap*, uint*, uint*, int>)&GetSize;
        table[4] = (delegate* unmanaged[Stdcall]<Bitmap*, Guid*, int>)&GetFormat;
        table[5] = (delegate* unmanaged[Stdcall]<Bitmap*, double*, double*, int>)&GetResolution;
        table[6] = (delegate* unmanaged[Stdcall]<Bitmap*, nint, int>)&CopyPalette;
        table[7] = (delegate* unmanaged[Stdcall]<Bitmap*, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int>)&CopyPixels;
        table[8] = (delegate* unmanaged[Stdcall]<Bitmap*, Direct3D9BitmapSourceRectangle*, uint, nint*, int>)&Lock;
        table[9] = (delegate* unmanaged[Stdcall]<Bitmap*, nint, int>)&SetPalette;
        table[10] = (delegate* unmanaged[Stdcall]<Bitmap*, double, double, int>)&SetResolution;
        return table;
    }

    private static void** CreateLockTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(SoftwareBitmap), 7 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<BitmapLock*, Guid*, nint*, int>)&QueryLock;
        table[1] = (delegate* unmanaged[Stdcall]<BitmapLock*, uint>)&AddRefLock;
        table[2] = (delegate* unmanaged[Stdcall]<BitmapLock*, uint>)&ReleaseLock;
        table[3] = (delegate* unmanaged[Stdcall]<BitmapLock*, uint*, uint*, int>)&GetLockSize;
        table[4] = (delegate* unmanaged[Stdcall]<BitmapLock*, uint*, int>)&GetStride;
        table[5] = (delegate* unmanaged[Stdcall]<BitmapLock*, uint*, byte**, int>)&GetData;
        table[6] = (delegate* unmanaged[Stdcall]<BitmapLock*, Guid*, int>)&GetLockFormat;
        return table;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(Bitmap* self, Guid* id, nint* output)
    {
        if (output == null) return PointerError;
        *output = 0;
        if (id == null) return PointerError;
        if (*id == MilBitmapId || *id == MilSourceId)
        {
            Retain((nint)self); *output = (nint)(&self->MilVtable); return 0;
        }
        if (*id != UnknownId && *id != BitmapId && *id != SourceId) return unchecked((int)0x80004002);
        Retain((nint)self);
        *output = (nint)self;
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(Bitmap* self) => (uint)Interlocked.Increment(ref self->References);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(Bitmap* self) => ReleaseReference((nint)self);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetSize(Bitmap* self, uint* width, uint* height)
    {
        if (width == null || height == null) return InvalidArgument;
        *width = self->Width; *height = self->Height; return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetFormat(Bitmap* self, Guid* format)
    {
        if (format == null) return InvalidArgument;
        *format = self->Format; return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetResolution(Bitmap* self, double* x, double* y)
    {
        if (x == null || y == null) return InvalidArgument;
        *x = self->DpiX; *y = self->DpiY; return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CopyPalette(Bitmap* self, nint palette)
    {
        if (palette == 0) return InvalidArgument;
        if (self->Palette == 0) return unchecked((int)0x88982F45);
        return ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)palette)[6])(palette, self->Palette);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetPalette(Bitmap* self, nint palette)
    {
        if (palette == 0) return InvalidArgument;
        int result = BitmapFormatConverter.ClonePalette(palette, out nint copy);
        nint previous = self->Palette;
        self->Palette = copy;
        Direct3D9Factory.Release(previous);
        if (result >= 0) self->Uniqueness++;
        return result;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SetResolution(Bitmap* self, double x, double y)
    {
        if (self->DpiX != (float)x || self->DpiY != (float)y) self->Uniqueness++;
        self->DpiX = (float)x; self->DpiY = (float)y; return 0;
    }

    private static void CopyBits(byte* source, ulong sourceBit, byte* destination, ulong destinationBit, ulong count)
    {
        if (((sourceBit | destinationBit | count) & 7) == 0)
        {
            NativeMemory.Copy(source + (nuint)(sourceBit / 8), destination + (nuint)(destinationBit / 8), (nuint)(count / 8));
            return;
        }
        for (ulong bit = 0; bit < count; bit++)
        {
            ulong src = sourceBit + bit, dst = destinationBit + bit;
            int mask = 0x80 >> (int)(dst & 7);
            byte* value = destination + (nuint)(dst / 8);
            *value = (byte)((*value & ~mask) | (((source[(nuint)(src / 8)] >> (7 - (int)(src & 7))) & 1) != 0 ? mask : 0));
        }
    }

    private static bool ValidRectangle(Bitmap* self, Direct3D9BitmapSourceRectangle rectangle) =>
        rectangle.X >= 0 && rectangle.Y >= 0 && rectangle.Width > 0 && rectangle.Height > 0
        && (ulong)(uint)rectangle.X + (uint)rectangle.Width <= self->Width
        && (ulong)(uint)rectangle.Y + (uint)rectangle.Height <= self->Height;

    private static bool EnterLock(Bitmap* self, uint flags)
    {
        if ((flags & 2) != 0) return Interlocked.CompareExchange(ref self->Locks, -1, 0) == 0;
        int count;
        do
        {
            count = Volatile.Read(ref self->Locks);
            if (count < 0 || count == int.MaxValue) return false;
        } while (Interlocked.CompareExchange(ref self->Locks, count + 1, count) != count);
        return true;
    }

    private static void ExitLock(Bitmap* self, uint flags)
    {
        if ((flags & 2) != 0) Interlocked.Exchange(ref self->Locks, 0);
        else Interlocked.Decrement(ref self->Locks);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CopyPixels(Bitmap* self, Direct3D9BitmapSourceRectangle* requested, uint stride, uint size, byte* pixels)
    {
        var rectangle = requested == null ? new Direct3D9BitmapSourceRectangle(0, 0, (int)self->Width, (int)self->Height) : *requested;
        if (pixels == null || !ValidRectangle(self, rectangle)) return InvalidArgument;
        uint rowBytes = (uint)(((ulong)(uint)rectangle.Width * self->BitsPerPixel + 7) / 8);
        if (stride < rowBytes || (ulong)stride * (uint)(rectangle.Height - 1) + rowBytes > size) return unchecked((int)0x88982F8C);
        if (!EnterLock(self, 1)) return AlreadyLocked;
        try
        {
            for (uint row = 0; row < rectangle.Height; row++)
            {
                byte* destination = pixels + (nuint)row * stride;
                NativeMemory.Clear(destination, rowBytes);
                CopyBits(self->Pixels + ((nuint)(uint)rectangle.Y + row) * self->Stride,
                    (ulong)(uint)rectangle.X * self->BitsPerPixel, destination, 0, (ulong)(uint)rectangle.Width * self->BitsPerPixel);
            }
            return 0;
        }
        finally { ExitLock(self, 1); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Lock(Bitmap* self, Direct3D9BitmapSourceRectangle* requested, uint flags, nint* output)
    {
        if (output == null) return InvalidArgument;
        *output = 0;
        var rectangle = requested == null ? new Direct3D9BitmapSourceRectangle(0, 0, (int)self->Width, (int)self->Height) : *requested;
        if ((flags & 3) == 0 || (flags & ~3u) != 0 || !ValidRectangle(self, rectangle)) return InvalidArgument;
        if (!EnterLock(self, flags)) return AlreadyLocked;
        BitmapLock* value = null;
        try
        {
            void** table = Tables.Lock;
            value = (BitmapLock*)NativeMemory.AllocZeroed((nuint)sizeof(BitmapLock));
            if ((flags & 2) != 0)
            {
                uint previous;
                if (VirtualProtect(self->Pixels, self->Size, 4, &previous) == 0) return LastError();
            }
            value->Vtable = table; value->MilVtable = MilTables.Lock; value->References = 1; value->Owner = self;
            uint rowBytes = (uint)(((ulong)(uint)rectangle.Width * self->BitsPerPixel + 7) / 8);
            value->Stride = self->Stride;
            value->Size = (uint)(rectangle.Height - 1) * self->Stride + rowBytes;
            if (((ulong)(uint)rectangle.X * self->BitsPerPixel & 7) != 0)
            {
                value->Stride = (rowBytes + 3) & ~3u;
                ulong temporarySize = (ulong)value->Stride * (uint)rectangle.Height;
                if (temporarySize > uint.MaxValue) return unchecked((int)0x80070216);
                value->Size = (uint)temporarySize;
                value->Temporary = (byte*)NativeMemory.AllocZeroed(value->Size);
                if ((flags & 1) != 0)
                    for (uint row = 0; row < rectangle.Height; row++)
                        CopyBits(self->Pixels + ((nuint)(uint)rectangle.Y + row) * self->Stride,
                            (ulong)(uint)rectangle.X * self->BitsPerPixel, value->Temporary + (nuint)row * value->Stride, 0,
                            (ulong)(uint)rectangle.Width * self->BitsPerPixel);
            }
            if ((flags & 2) != 0) self->Uniqueness++;
            value->Rectangle = rectangle; value->Flags = flags;
            Retain((nint)self);
            *output = (nint)value;
            value = null;
            return 0;
        }
        catch (OutOfMemoryException) { return OutOfMemory; }
        finally
        {
            if (*output == 0)
            {
                if (value != null) NativeMemory.Free(value->Temporary);
                NativeMemory.Free(value); ExitLock(self, flags);
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int QueryLock(BitmapLock* self, Guid* id, nint* output)
    {
        if (output == null) return PointerError;
        *output = 0;
        if (id == null) return PointerError;
        if (*id == MilLockId)
        {
            Interlocked.Increment(ref self->References); *output = (nint)(&self->MilVtable); return 0;
        }
        if (*id != UnknownId && *id != LockId) return unchecked((int)0x80004002);
        Interlocked.Increment(ref self->References); *output = (nint)self; return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRefLock(BitmapLock* self) => (uint)Interlocked.Increment(ref self->References);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseLock(BitmapLock* self)
    {
        int count = Interlocked.Decrement(ref self->References);
        if (count == 0)
        {
            Bitmap* owner = self->Owner;
            if (self->Temporary != null && (self->Flags & 2) != 0)
                for (uint row = 0; row < self->Rectangle.Height; row++)
                    CopyBits(self->Temporary + (nuint)row * self->Stride, 0,
                        owner->Pixels + ((nuint)(uint)self->Rectangle.Y + row) * owner->Stride,
                        (ulong)(uint)self->Rectangle.X * owner->BitsPerPixel, (ulong)(uint)self->Rectangle.Width * owner->BitsPerPixel);
            NativeMemory.Free(self->Temporary);
            ExitLock(owner, self->Flags);
            NativeMemory.Free(self);
            ReleaseReference((nint)owner);
        }
        return (uint)count;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetLockSize(BitmapLock* self, uint* width, uint* height)
    {
        if (width == null || height == null) return InvalidArgument;
        *width = (uint)self->Rectangle.Width; *height = (uint)self->Rectangle.Height; return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetStride(BitmapLock* self, uint* stride)
    {
        if (stride == null) return InvalidArgument;
        *stride = self->Stride; return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetData(BitmapLock* self, uint* size, byte** pixels)
    {
        if (size == null || pixels == null) return InvalidArgument;
        Bitmap* owner = self->Owner;
        *size = self->Size;
        *pixels = self->Temporary != null ? self->Temporary : owner->Pixels + (nuint)(uint)self->Rectangle.Y * owner->Stride
            + (nuint)((ulong)(uint)self->Rectangle.X * owner->BitsPerPixel / 8);
        return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetLockFormat(BitmapLock* self, Guid* format)
    {
        if (format == null) return InvalidArgument;
        *format = self->Owner->Format; return 0;
    }
}
