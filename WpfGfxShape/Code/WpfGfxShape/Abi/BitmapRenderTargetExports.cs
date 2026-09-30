using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe partial class BitmapRenderTargetExports
{
    private const int InvalidArgument = unchecked((int)0x80070057);
    private const int NotImplemented = unchecked((int)0x80004001);
    private static readonly Guid UnknownId = new("00000000-0000-0000-c000-000000000046");
    private static readonly Guid FactoryId = new("00000002-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly Guid TargetId = new("00000020-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly Guid BitmapTargetId = new("00000201-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly Guid MilBitmapId = new("c46d6fde-0e59-4cfd-89b1-c935906dfbd9");

    [StructLayout(LayoutKind.Sequential)]
    private struct Instance
    {
        internal void** Vtable;
        internal int References;
        internal nint Bitmap;
        internal InternalView Internal;
    }

    private static class Tables
    {
        internal static readonly void** Factory = CreateFactoryTable();
        internal static readonly void** Target = CreateTargetTable();
    }

    private static void** CreateFactoryTable()
    {
        void** t = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(BitmapRenderTargetExports), 8 * sizeof(nint));
        t[0] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, nint*, int>)&Query;
        t[1] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&AddRef;
        t[2] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&Release;
        t[3] = (delegate* unmanaged[Stdcall]<Instance*, byte*, int*, int>)&UpdateDisplay;
        t[4] = (delegate* unmanaged[Stdcall]<Instance*, byte, uint*, nint, void>)&QueryCaps;
        t[5] = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)&CreateTarget;
        t[6] = (delegate* unmanaged[Stdcall]<Instance*, nint, nint*, int>)&CreateForBitmap;
        t[7] = (delegate* unmanaged[Stdcall]<Instance*, nint, byte, nint*, int>)&CreateMedia;
        return t;
    }

    private static void** CreateTargetTable()
    {
        void** t = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(BitmapRenderTargetExports), 11 * sizeof(nint));
        t[0] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, nint*, int>)&Query;
        t[1] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&AddRef;
        t[2] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&Release;
        t[3] = (delegate* unmanaged[Stdcall]<Instance*, float*, void>)&Bounds;
        t[4] = (delegate* unmanaged[Stdcall]<Instance*, float*, nint, int>)&Clear;
        t[5] = (delegate* unmanaged[Stdcall]<Instance*, nint, uint, byte, float, int>)&Begin3D;
        t[6] = (delegate* unmanaged[Stdcall]<Instance*, int>)&End3D;
        t[7] = (delegate* unmanaged[Stdcall]<Instance*, nint*, int>)&GetMilBitmap;
        t[8] = t[7]; t[9] = t[7];
        t[10] = (delegate* unmanaged[Stdcall]<Instance*, uint*, int>)&Queued;
        return t;
    }

    [UnmanagedCallersOnly(EntryPoint = "MILCreateFactory", CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateFactory(nint* output, uint version)
    {
        if (output == null) return InvalidArgument;
        if (version != 0x200184C0) return unchecked((int)0x88982F0B);
        try
        {
            void** table = Tables.Factory;
            Instance* value = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            value->Vtable = table; value->References = 1;
            *output = (nint)value;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
    }

    [UnmanagedCallersOnly(EntryPoint = "MILFactoryCreateBitmapRenderTarget", CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateTargetExport(nint factory, uint width, uint height, uint format, float dpiX, float dpiY, uint flags, nint* output)
    {
        if (factory == 0) return InvalidArgument;
        return ((delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)(*(void***)factory)[5])(factory, width, height, format, dpiX, dpiY, flags, output);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateTarget(nint factory, uint width, uint height, uint format, float dpiX, float dpiY, uint flags, nint* output)
    {
        if (factory == 0 || width == 0 || height == 0 || dpiX <= 0 || dpiY <= 0 || output == null) return InvalidArgument;
        if ((flags & ~3u) != 0 || flags == 3) return InvalidArgument;
        if (format is not (16 or 26)) return Direct3D9Factory.UnsupportedPixelFormatHResult;
        if (flags == 2) return NotImplemented;
        return CreateBitmapTarget(width, height, format, dpiX, dpiY, output);
    }

    private static int CreateBitmapTarget(uint width, uint height, uint format, float dpiX, float dpiY, nint* output)
    {
        *output = 0;
        nint bitmap = 0;
        try
        {
            Span<byte> bytes = stackalloc byte[16];
            new Guid("6fddc324-4e03-4bfe-b185-3d77768dc900").TryWriteBytes(bytes);
            bytes[15] = (byte)format;
            int result = SoftwareBitmap.Create(width, height, dpiX, dpiY, new Guid(bytes), out bitmap);
            if (result < 0) return result;
            void** table = Tables.Target;
            Instance* target = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            target->Vtable = table; target->References = 1; target->Bitmap = bitmap;
            bitmap = 0;
            *output = (nint)target;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        finally { SoftwareBitmap.ReleaseReference(bitmap); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(Instance* self, Guid* id, nint* output)
    {
        if (output == null || id == null) return InvalidArgument;
        *output = 0;
        bool target = self->Bitmap != 0;
        if (target && *id == InternalTargetId)
        {
            InitializeInternalView(self);
            Interlocked.Increment(ref self->References);
            *output = (nint)(&self->Internal);
            return 0;
        }
        if (*id != UnknownId && !(target ? *id == TargetId || *id == BitmapTargetId : *id == FactoryId))
            return unchecked((int)0x80004002);
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
        if (count == 0)
        {
            Direct3D9Factory.Release(self->Bitmap);
            NativeMemory.Free(self);
        }
        return (uint)count;
    }

    [UnmanagedCallersOnly(EntryPoint = "MILRenderTargetBitmapGetBitmap", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetBitmap(Instance* self, nint* output)
    {
        if (self == null || output == null) return InvalidArgument;
        *output = 0;
        nint bitmap = 0;
        int result = ((delegate* unmanaged[Stdcall]<Instance*, nint*, int>)self->Vtable[9])(self, &bitmap);
        if (result < 0) return result;
        try
        {
            Guid id = new("00000121-a8f2-4877-ba0a-fd2b6645fb94");
            return ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &id, output);
        }
        finally { Direct3D9Factory.Release(bitmap); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int GetMilBitmap(Instance* self, nint* output)
    {
        if (output == null) return InvalidArgument;
        *output = 0;
        Guid id = MilBitmapId;
        int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)self->Bitmap)[0])(self->Bitmap, &id, output);
        if (result != Direct3D9Factory.NoInterfaceHResult) return result;
        result = WicBitmapAdapter.Create(self->Bitmap, out nint adapter);
        if (result >= 0) *output = adapter;
        return result;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Bounds(Instance* self, float* bounds)
    {
        uint width = 0, height = 0;
        int result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)self->Bitmap)[3])(self->Bitmap, &width, &height);
        bounds[0] = bounds[1] = 0;
        bounds[2] = result >= 0 ? width : 0;
        bounds[3] = result >= 0 ? height : 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "MILRenderTargetBitmapClear", CallConvs = [typeof(CallConvStdcall)])]
    private static int ClearTransparent(Instance* self)
    {
        if (self == null) return InvalidArgument;
        Guid id = TargetId;
        nint target = 0;
        int result = ((delegate* unmanaged[Stdcall]<Instance*, Guid*, nint*, int>)self->Vtable[0])(self, &id, &target);
        try
        {
            if (result < 0) return result;
            float* color = stackalloc float[4];
            new Span<float>(color, 4).Clear();
            NativeAliasedClip clip = new() { IsNull = 1 };
            return ((delegate* unmanaged[Stdcall]<nint, float*, NativeAliasedClip*, int>)(*(void***)target)[4])(target, color, &clip);
        }
        finally { Direct3D9Factory.Release(target); }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeAliasedClip
    {
        internal int IsNull;
        internal float Left, Top, Right, Bottom;
    }

    private static int ClearPixels(Instance* self)
    {
        nint bitmapLock = 0;
        nint bitmap = self->Bitmap;
        int result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock);
        if (result < 0) return result;
        try
        {
            uint size;
            byte* data;
            result = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &data);
            if (result >= 0) NativeMemory.Clear(data, size);
            return result;
        }
        finally { Direct3D9Factory.Release(bitmapLock); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Clear(Instance* self, float* color, nint clip)
    {
        if (color == null) return 0;
        nint bitmap = self->Bitmap;
        Guid format;
        int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)bitmap)[4])(bitmap, &format);
        if (result < 0) return result;
        uint width, height;
        result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)bitmap)[3])(bitmap, &width, &height);
        if (result < 0) return result;
        if (width > int.MaxValue || height > int.MaxValue) return InvalidArgument;
        if (SoftwareBitmap.FormatCode(format) != 16)
        {
            if ((clip == 0 || ((NativeAliasedClip*)clip)->IsNull != 0) && color[0] == 0 && color[1] == 0 && color[2] == 0 && color[3] == 0)
                return ClearPixels(self);
            return NotImplemented;
        }
        nint bitmapLock = 0;
        result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock);
        if (result < 0) return result;
        try
        {
            int left = 0, top = 0, right = (int)width, bottom = (int)height;
            if (clip != 0 && *(int*)clip == 0)
            {
                float* bounds = (float*)(clip + sizeof(int));
                if (float.IsNaN(bounds[0]) || float.IsNaN(bounds[1]) || float.IsNaN(bounds[2]) || float.IsNaN(bounds[3])) return 0;
                left = ClipCoordinate(bounds[0], right);
                top = ClipCoordinate(bounds[1], bottom);
                right = ClipCoordinate(bounds[2], right);
                bottom = ClipCoordinate(bounds[3], bottom);
            }
            if (right <= left || bottom <= top) return 0;
            uint size = 0, stride = 0;
            byte* data = null;
            result = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &data);
            if (result < 0) return result;
            result = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride);
            if (result < 0) return result;
            if (data == null || stride < (ulong)width * 4 || (height != 0 && (ulong)(height - 1) * stride + (ulong)width * 4 > size))
                return InvalidArgument;
            // Native MilColorF is RGBA; the managed color value is constructed ARGB.
            uint pixel = Direct3D9SoftwareRenderTargetSurface.ConvertToSrgb(new MilColorF(color[3], color[0], color[1], color[2]), true);
            for (int y = top; y < bottom; y++)
            {
                uint* row = (uint*)(data + (nuint)y * stride);
                for (int x = left; x < right; x++) row[x] = pixel;
            }
            return 0;
        }
        finally { Direct3D9Factory.Release(bitmapLock); }
    }

    private static int ClipCoordinate(float value, int extent)
    {
        if (value <= 0) return 0;
        if (value >= extent) return extent;
        // GpFix4Round(GpRealToFix4(value) - 1), with inclusive top-left ties.
        return (int)(((long)MathF.Floor(value * 16f + 0.5f) + 7) >> 4);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Queued(Instance* self, uint* count)
    {
        if (count == null) return InvalidArgument;
        *count = 0; return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Begin3D(Instance* self, nint bounds, uint mode, byte useZ, float z) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int End3D(Instance* self) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UpdateDisplay(Instance* self, byte* changed, int* count) => NotImplemented;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void QueryCaps(Instance* self, byte common, uint* uniqueness, nint caps)
    {
        // No display set has been acquired by this software-only factory yet.
        // Match CMILFactory's failed-display-set fallback rather than report hardware support.
        *uniqueness = 0;
        NativeMemory.Clear((void*)caps, 10 * sizeof(uint));
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateForBitmap(Instance* self, nint bitmap, nint* output)
    {
        if (bitmap == 0 || output == null) return InvalidArgument;
        *output = 0;
        nint owned = 0;
        try
        {
            Guid wicId = new("00000121-a8f2-4877-ba0a-fd2b6645fb94");
            int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &wicId, &owned);
            if (result < 0) return result;
            if (owned == 0) return Direct3D9Factory.NoInterfaceHResult;
            Guid milId = MilBitmapId;
            nint mil = 0;
            result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)owned)[0])(owned, &milId, &mil);
            Direct3D9Factory.Release(mil);
            if (result == Direct3D9Factory.NoInterfaceHResult)
            {
                result = WicBitmapAdapter.Create(owned, out nint adapter);
                if (result < 0) return result;
                try
                {
                    nint view = 0;
                    result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)adapter)[0])(adapter, &wicId, &view);
                    if (result < 0) return result;
                    Direct3D9Factory.Release(owned);
                    owned = view;
                }
                finally { Direct3D9Factory.Release(adapter); }
            }
            else if (result < 0) return result;
            Guid format;
            result = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)owned)[4])(owned, &format);
            if (result < 0) return result;
            if (SoftwareBitmap.FormatCode(format) is not (16 or 26)) return Direct3D9Factory.UnsupportedPixelFormatHResult;
            void** table = Tables.Target;
            Instance* target = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            target->Vtable = table;
            target->References = 1;
            target->Bitmap = owned;
            owned = 0;
            *output = (nint)target;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        finally { Direct3D9Factory.Release(owned); }
    }

    [UnmanagedCallersOnly(EntryPoint = "MILFactoryCreateSWRenderTargetForBitmap", CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateForBitmapExport(nint factory, nint bitmap, nint* output)
    {
        if (factory == 0) return InvalidArgument;
        return ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)(*(void***)factory)[6])(factory, bitmap, output);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateMedia(Instance* self, nint proxy, byte any, nint* output) => NotImplemented;
}
