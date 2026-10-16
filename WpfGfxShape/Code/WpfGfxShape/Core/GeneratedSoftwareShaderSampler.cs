using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal static unsafe class GeneratedSoftwareShaderSampler
{
    internal static int Realize(SoftwareBrushRealization brush, uint width, uint height,
        bool bilinear, out SoftwareShaderSampler sampler, bool floating = false)
    {
        sampler = default;
        nint target = 0, bitmap = 0;
        SoftwareImageRenderSession? session = null;
        try
        {
            int hr = BitmapRenderTargetExports.CreateDrawingIntermediate(width, height,
                out target, out bitmap, floating: floating);
            if (hr < 0) return hr;
            hr = SoftwareImageRenderSession.Open(target, width, height, out session);
            if (hr < 0) return hr;
            if (session is null) return Direct3D9Factory.WgxInvalidCallHResult;
            var clip = new Direct3D9SurfaceRect(0, 0, checked((int)width), checked((int)height));
            hr = session.DrawPath(brush, null, null, null, new(0, 0, width, height),
                new(GeneratedImageTransform.Identity, clip, 0, MilCompositingMode.SourceCopy));
            if (hr < 0) return hr;
            session.Dispose();
            session = null;
            if (floating)
            {
                System.Numerics.Vector4[] values = new System.Numerics.Vector4[checked((int)(width * height))];
                fixed (System.Numerics.Vector4* destination = values)
                    hr = ((delegate* unmanaged[Stdcall]<nint, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int>)(*(void***)bitmap)[7])
                        (bitmap, null, checked(width * 16), checked((uint)values.Length * 16), (byte*)destination);
                if (hr < 0) return hr;
                sampler = new([], checked((int)width), checked((int)height), bilinear) { FloatPixels = values };
                return 0;
            }
            uint[] pixels = new uint[checked((int)(width * height))];
            fixed (uint* destination = pixels)
                hr = ((delegate* unmanaged[Stdcall]<nint, Direct3D9BitmapSourceRectangle*, uint, uint, byte*, int>)(*(void***)bitmap)[7])
                    (bitmap, null, checked(width * 4), checked((uint)pixels.Length * 4), (byte*)destination);
            if (hr < 0) return hr;
            sampler = new(pixels, checked((int)width), checked((int)height), bilinear);
            return 0;
        }
        finally
        {
            session?.Dispose();
            Direct3D9Factory.Release(bitmap);
            Direct3D9Factory.Release(target);
        }
    }
}
