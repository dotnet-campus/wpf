using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe partial class BitmapFormatConverter
{
    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(Guid* classId, nint outer, uint context, Guid* interfaceId, nint* instance);

    internal static int ClonePalette(nint source, out nint palette)
    {
        palette = 0;
        Guid classId = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
        Guid interfaceId = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
        nint factory = 0, value = 0;
        try
        {
            int result = CoCreateInstance(&classId, 0, 1, &interfaceId, &factory);
            if (result < 0) return result;
            result = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)factory)[9])(factory, &value);
            if (result < 0) return result;
            result = ((delegate* unmanaged[Stdcall]<nint, nint, int>)(*(void***)value)[6])(value, source);
            if (result < 0) return result;
            palette = value; value = 0;
            return 0;
        }
        finally { Direct3D9Factory.Release(value); Direct3D9Factory.Release(factory); }
    }

    internal static int CreateBitmapFromSource(nint source, out nint bitmap)
    {
        bitmap = 0;
        Guid classId = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
        Guid interfaceId = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
        nint factory = 0, value = 0;
        try
        {
            int result = CoCreateInstance(&classId, 0, 1, &interfaceId, &factory);
            if (result < 0) return result;
            result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)factory)[18])(factory, source, 0, &value);
            if (result < 0) return result;
            bitmap = value; value = 0;
            return 0;
        }
        finally { Direct3D9Factory.Release(value); Direct3D9Factory.Release(factory); }
    }

    internal static int CreateScaler(nint source, uint width, uint height, out nint scaler)
    {
        scaler = 0;
        Guid classId = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
        Guid interfaceId = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
        nint factory = 0, value = 0;
        try
        {
            int result = CoCreateInstance(&classId, 0, 1, &interfaceId, &factory);
            if (result < 0) return result;
            result = ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)factory)[11])(factory, &value);
            if (result < 0) return result;
            result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint, int>)(*(void***)value)[8])(value, source, width, height, 3);
            if (result < 0) return result;
            scaler = value; value = 0;
            return 0;
        }
        finally { Direct3D9Factory.Release(value); Direct3D9Factory.Release(factory); }
    }

    internal static int Create(nint source, Guid format, out nint converter)
    {
        converter = 0;
        Guid classId = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
        Guid interfaceId = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
        nint factory = 0, value = 0;
        try
        {
            int result = CoCreateInstance(&classId, 0, 1, &interfaceId, &factory);
            if (result < 0) return result;
            var create = (delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)factory)[10];
            result = create(factory, &value);
            if (result < 0) return result;
            var initialize = (delegate* unmanaged[Stdcall]<nint, nint, Guid*, uint, nint, double, uint, int>)(*(void***)value)[8];
            result = initialize(value, source, &format, 0, 0, 0, 0);
            if (result < 0) return result;
            converter = value;
            value = 0;
            return 0;
        }
        finally
        {
            Direct3D9Factory.Release(value);
            Direct3D9Factory.Release(factory);
        }
    }
}
