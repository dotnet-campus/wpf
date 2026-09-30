using System.Numerics;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe partial class Direct3D9SoftwareImageRenderer
{
    private int AddSolidMask(nint effectList, float alpha)
    {
        nint bitmap = 0, bitmapLock = 0, milSource = 0;
        try
        {
            Guid format = _floating ? new("6fddc324-4e03-4bfe-b185-3d77768dc91a") : new("6fddc324-4e03-4bfe-b185-3d77768dc910");
            // A constant brush has the same alpha everywhere; Extend preserves it.
            int hr = SoftwareBitmap.Create(1, 1, 96, 96, format, out bitmap);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock);
            if (hr < 0) return hr;
            uint size; byte* pixels;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels);
            if (hr < 0) return hr;
            if (pixels == null || size < BytesPerPixel) return Direct3D9Factory.InvalidArgumentHResult;
            if (_floating) *(Vector4*)pixels = new(0, 0, 0, alpha);
            else *(uint*)pixels = (uint)MathF.Round(alpha * 255) << 24;
            Direct3D9Factory.Release(bitmapLock); bitmapLock = 0;
            Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &sourceId, &milSource);
            if (hr < 0) return hr;
            Guid effectId = new("00000521-a8f2-4877-ba0a-fd2b6645fb94");
            Matrix4x4 matrix = Matrix4x4.Identity;
            return ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, void*, uint, nint*, int>)(*(void***)effectList)[4])(
                effectList, &effectId, 64, &matrix, 1, &milSource);
        }
        finally
        {
            Direct3D9Factory.Release(milSource); Direct3D9Factory.Release(bitmapLock); Direct3D9Factory.Release(bitmap);
        }
    }
}
