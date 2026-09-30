using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class BitmapSourceExports
{
    [UnmanagedCallersOnly(EntryPoint = "MilResource_CreateCWICWrapperBitmap", CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateWrapper(nint source, nint* output)
    {
        if (source == 0 || output == null) return Direct3D9Factory.InvalidArgumentHResult;
        nint bitmap = 0, adapter = 0;
        try
        {
            Guid format;
            int result = ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)source)[4])(source, &format);
            if (result < 0) return result;
            uint width, height;
            result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)source)[3])(source, &width, &height);
            if (result < 0) return result;
            byte bits = MilPixelFormatInfo.GetBitsPerPixel((MilPixelFormat)SoftwareBitmap.FormatCode(format));
            if (bits == 0) return Direct3D9Factory.UnsupportedPixelFormatHResult;
            ulong stride = (((ulong)width * bits + 31) / 32) * 4;
            if (stride == 0 || stride > int.MaxValue || height >= (ulong)int.MaxValue / stride)
                return unchecked((int)0x80070216);
            Guid bitmapId = new("00000121-a8f2-4877-ba0a-fd2b6645fb94");
            result = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)source)[0])(source, &bitmapId, &bitmap);
            if (result < 0)
            {
                Direct3D9Factory.Release(bitmap); bitmap = 0;
                result = BitmapFormatConverter.CreateBitmapFromSource(source, out bitmap);
                if (result < 0) return result;
            }
            result = WicBitmapAdapter.Create(bitmap, out adapter);
            if (result < 0) return result;
            // The export returns the WIC view; the command consumer acquires the MIL view.
            Guid sourceId = new("00000120-a8f2-4877-ba0a-fd2b6645fb94");
            return ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)adapter)[0])(adapter, &sourceId, output);
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        finally { Direct3D9Factory.Release(adapter); Direct3D9Factory.Release(bitmap); }
    }
}
