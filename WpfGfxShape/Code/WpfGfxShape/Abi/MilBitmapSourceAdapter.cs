using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class MilBitmapSourceAdapter
{
    private struct Instance
    {
        internal void** Table;
        internal int References;
        internal nint Source;
    }

    private static readonly Guid SourceId = new("00000120-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly Guid UnknownId = new("00000000-0000-0000-c000-000000000046");
    private static readonly void** Table = CreateTable();

    internal static int Create(nint source, out nint wrapper)
    {
        wrapper = 0;
        if (source == 0) return Direct3D9Factory.InvalidArgumentHResult;
        Instance* value = null;
        try
        {
            value = (Instance*)NativeMemory.AllocZeroed((nuint)sizeof(Instance));
            value->Table = Table;
            value->References = 1;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source)[1])(source);
            value->Source = source;
            wrapper = (nint)value;
            value = null;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        finally { NativeMemory.Free(value); }
    }

    private static void** CreateTable()
    {
        void** table = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(MilBitmapSourceAdapter), 8 * sizeof(nint));
        table[0] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, nint*, int>)&Query;
        table[1] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&AddRef;
        table[2] = (delegate* unmanaged[Stdcall]<Instance*, uint>)&Release;
        table[3] = (delegate* unmanaged[Stdcall]<Instance*, uint*, uint*, int>)&Size;
        table[4] = (delegate* unmanaged[Stdcall]<Instance*, Guid*, int>)&Format;
        table[5] = (delegate* unmanaged[Stdcall]<Instance*, double*, double*, int>)&Resolution;
        table[6] = (delegate* unmanaged[Stdcall]<Instance*, nint, int>)&Palette;
        table[7] = (delegate* unmanaged[Stdcall]<Instance*, nint, uint, uint, byte*, int>)&Pixels;
        return table;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(Instance* self, Guid* id, nint* output)
    {
        if (output == null || id == null) return Direct3D9Factory.InvalidArgumentHResult;
        *output = 0;
        if (*id != SourceId && *id != UnknownId) return Direct3D9Factory.NoInterfaceHResult;
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
            nint source = self->Source;
            self->Source = 0;
            Direct3D9Factory.Release(source);
            NativeMemory.Free(self);
        }
        return (uint)count;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Size(Instance* self, uint* width, uint* height)
        => ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)self->Source)[3])(self->Source, width, height);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Format(Instance* self, Guid* format)
    {
        if (format == null) return Direct3D9Factory.InvalidArgumentHResult;
        int code;
        int result = ((delegate* unmanaged[Stdcall]<nint, int*, int>)(*(void***)self->Source)[4])(self->Source, &code);
        if (result < 0) return result;
        if (MilPixelFormatInfo.GetBitsPerPixel((MilPixelFormat)code) == 0)
            return Direct3D9Factory.UnsupportedPixelFormatHResult;
        Span<byte> bytes = stackalloc byte[16];
        new Guid("6fddc324-4e03-4bfe-b185-3d77768dc900").TryWriteBytes(bytes);
        bytes[15] = (byte)code;
        *format = new Guid(bytes);
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Resolution(Instance* self, double* x, double* y)
        => ((delegate* unmanaged[Stdcall]<nint, double*, double*, int>)(*(void***)self->Source)[5])(self->Source, x, y);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Palette(Instance* self, nint palette)
        => ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)self->Source)[6])(self->Source, palette);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Pixels(Instance* self, nint rect, uint stride, uint size, byte* pixels)
        => ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)self->Source)[7])(self->Source, rect, stride, size, pixels);
}
