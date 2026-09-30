using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class DoubleBufferedBitmapExports
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Instance
    {
        internal void** Vtable;
        internal int References;
        internal nint Back, Front, Converter;
        internal int Count;
        internal fixed int Rectangles[20];
    }

    private static class Table
    {
        internal static readonly void** Address = CreateTable();
    }

    private static void** CreateTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(DoubleBufferedBitmapExports), 3 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, nint*, int>)&Query;
        table[1] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&Release;
        return table;
    }

    [UnmanagedCallersOnly(EntryPoint = "MILSwDoubleBufferedBitmapCreate", CallConvs = [typeof(CallConvStdcall)])]
    private static int Create(uint width, uint height, double dpiX, double dpiY, Guid* format, nint palette, nint* output)
    {
        if (output == null || format == null) return unchecked((int)0x80004003);
        *output = 0;
        nint back = 0, front = 0, converter = 0;
        Instance* instance = null;
        try
        {
            int result = SoftwareBitmap.Create(width, height, dpiX, dpiY, *format, out back, palette);
            if (result < 0) return result;
            Guid frontFormat = MilPixelFormatInfo.HasAlphaChannel((MilPixelFormat)SoftwareBitmap.FormatCode(*format))
                ? new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910")
                : new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90e");
            result = SoftwareBitmap.Create(width, height, dpiX, dpiY, frontFormat, out front);
            if (result < 0) return result;
            if (*format != frontFormat)
            {
                result = BitmapFormatConverter.Create(back, frontFormat, out converter);
                if (result < 0) return result;
            }
            void** table = Table.Address;
            instance = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            instance->Vtable = table; instance->References = 1;
            instance->Back = back; instance->Front = front; instance->Converter = converter;
            *output = (nint)instance;
            back = front = converter = 0;
            instance = null;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        finally
        {
            NativeMemory.Free(instance);
            Direct3D9Factory.Release(converter);
            SoftwareBitmap.ReleaseReference(front);
            SoftwareBitmap.ReleaseReference(back);
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MILSwDoubleBufferedBitmapGetBackBuffer", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetBack(Instance* self, nint* output, uint* size)
    {
        if (self == null || output == null) return unchecked((int)0x80004003);
        SoftwareBitmap.Retain(self->Back);
        *output = self->Back;
        if (size != null) *size = ((SoftwareBitmap.Bitmap*)self->Back)->Size;
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "MILSwDoubleBufferedBitmapProtectBackBuffer", CallConvs = [typeof(CallConvStdcall)])]
    private static int Protect(Instance* self) => self == null ? unchecked((int)0x80004003) : SoftwareBitmap.Protect(self->Back);

    [UnmanagedCallersOnly(EntryPoint = "MILSwDoubleBufferedBitmapAddDirtyRect", CallConvs = [typeof(CallConvStdcall)])]
    private static int AddDirty(Instance* self, Direct3D9BitmapSourceRectangle* rectangle)
    {
        if (self == null || rectangle == null) return unchecked((int)0x80004003);
        var r = *rectangle;
        if (r.X < 0 || r.Y < 0 || r.Width < 0 || r.Height < 0) return unchecked((int)0x80070216);
        if (r.Width == 0 || r.Height == 0) return 0;
        SoftwareBitmap.Bitmap* back = (SoftwareBitmap.Bitmap*)self->Back;
        long right = (long)r.X + r.Width, bottom = (long)r.Y + r.Height;
        if (right > back->Width || bottom > back->Height) return unchecked((int)0x80070057);
        var dirty = new MilRectL(r.X, r.Y, (int)right, (int)bottom);
        MilRectL* rectangles = (MilRectL*)self->Rectangles;
        if (r.X == 0 && r.Y == 0 && right == back->Width && bottom == back->Height)
        {
            rectangles[0] = dirty; self->Count = 1; return 0;
        }
        for (int i = 0; i < self->Count; i++)
        {
            MilRectL old = rectangles[i];
            if (old.Left <= dirty.Left && old.Top <= dirty.Top && old.Right >= dirty.Right && old.Bottom >= dirty.Bottom) return 0;
        }
        if (self->Count == 5)
        {
            for (int i = 0; i < self->Count; i++)
            {
                MilRectL old = rectangles[i];
                dirty = new MilRectL(Math.Min(old.Left, dirty.Left), Math.Min(old.Top, dirty.Top), Math.Max(old.Right, dirty.Right), Math.Max(old.Bottom, dirty.Bottom));
            }
            rectangles[0] = dirty; self->Count = 1;
        }
        else rectangles[self->Count++] = dirty;
        return 0;
    }

    internal static nint AcquireBitmapSource(nint instance, bool useBackBuffer)
    {
        Instance* self = (Instance*)instance;
        nint source = useBackBuffer ? (self->Converter != 0 ? self->Converter : self->Back) : self->Front;
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source)[1])(source);
        return source;
    }

    internal static int CopyForward(nint instance)
    {
        Instance* self = (Instance*)instance;
        while (self->Count > 0)
        {
            MilRectL dirty = ((MilRectL*)self->Rectangles)[--self->Count];
            var rectangle = new Direct3D9BitmapSourceRectangle(dirty.Left, dirty.Top, dirty.Right - dirty.Left, dirty.Bottom - dirty.Top);
            nint bitmapLock = 0;
            try
            {
                int result = Direct3D9Bitmap.Lock(self->Front, rectangle, MilBitmapLockFlags.Write, out bitmapLock);
                if (result < 0) return result;
                result = Direct3D9BitmapLock.GetStride(bitmapLock, out uint stride);
                if (result < 0) return result;
                result = Direct3D9BitmapLock.GetDataPointer(bitmapLock, out uint size, out nint pixels);
                if (result < 0) return result;
                result = Direct3D9BitmapSource.CopyPixels(self->Converter != 0 ? self->Converter : self->Back, rectangle, stride, size, pixels);
                if (result < 0) return result;
            }
            finally { Direct3D9Factory.Release(bitmapLock); }
        }
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(Instance* self, Guid* id, nint* output)
    {
        if (output == null || id == null) return unchecked((int)0x80004003);
        *output = 0;
        if (*id != new Guid("00000000-0000-0000-c000-000000000046")) return unchecked((int)0x80004002);
        Interlocked.Increment(ref self->References); *output = (nint)self; return 0;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(Instance* self) => (uint)Interlocked.Increment(ref self->References);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(Instance* self)
    {
        int count = Interlocked.Decrement(ref self->References);
        if (count == 0)
        {
            SoftwareBitmap.ReleaseReference(self->Back);
            SoftwareBitmap.ReleaseReference(self->Front);
            Direct3D9Factory.Release(self->Converter);
            NativeMemory.Free(self);
        }
        return (uint)count;
    }
}
