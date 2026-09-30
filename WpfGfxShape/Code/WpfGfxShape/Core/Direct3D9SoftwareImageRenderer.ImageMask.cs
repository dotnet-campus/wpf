using System.Numerics;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe partial class Direct3D9SoftwareImageRenderer
{
    private int AddImageMask(nint effectList, GeneratedImageMask mask)
    {
        nint bitmap = 0, bitmapLock = 0, milSource = 0;
        try
        {
            double dpiX, dpiY;
            int hr = ((delegate* unmanaged[Stdcall]<nint, double*, double*, int>)(*(void***)mask.Source)[5])(mask.Source, &dpiX, &dpiY);
            if (hr < 0) return hr;
            if (!double.IsFinite(dpiX) || !double.IsFinite(dpiY) || dpiX <= 0 || dpiY <= 0) return Direct3D9Factory.InvalidArgumentHResult;
            uint width, height;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)mask.Source)[3])(mask.Source, &width, &height);
            if (hr < 0) return hr;
            var box = mask.Viewbox;
            if (mask.RelativeViewbox)
            {
                double contentWidth = width * 96.0 / dpiX, contentHeight = height * 96.0 / dpiY;
                box = new(box.X * contentWidth, box.Y * contentHeight, box.Width * contentWidth, box.Height * contentHeight);
            }
            var viewport = mask.Viewport;
            double sx = viewport.Width / box.Width, sy = viewport.Height / box.Height;
            switch (mask.Stretch)
            {
                case MilStretch.None: sx = sy = 1; break;
                case MilStretch.Uniform: sx = sy = Math.Min(sx, sy); break;
                case MilStretch.UniformToFill: sx = sy = Math.Max(sx, sy); break;
            }
            double ax = (uint)mask.AlignmentX * 0.5, ay = (uint)mask.AlignmentY * 0.5;
            double tx = viewport.X + viewport.Width * ax - (box.X + box.Width * ax) * sx;
            double ty = viewport.Y + viewport.Height * ay - (box.Y + box.Height * ay) * sy;
            if (!double.IsFinite(sx) || !double.IsFinite(sy) || sx <= 0 || sy <= 0
                || !double.IsFinite(tx) || !double.IsFinite(ty)) return Direct3D9Factory.InvalidArgumentHResult;
            var mapping = mask.Transform.Prepend(new(sx, sy, tx, ty))
                .Prepend(new(96 / dpiX, 96 / dpiY, 0, 0));
            double left = Math.Max(box.X, (viewport.X - tx) / sx);
            double top = Math.Max(box.Y, (viewport.Y - ty) / sy);
            double right = Math.Min(box.X + box.Width, (viewport.X + viewport.Width - tx) / sx);
            double bottom = Math.Min(box.Y + box.Height, (viewport.Y + viewport.Height - ty) / sy);
            var sourceBox = new MilRectD(left * dpiX / 96, top * dpiY / 96,
                Math.Max(0, right - left) * dpiX / 96, Math.Max(0, bottom - top) * dpiY / 96);
            Guid format = _floating ? new("6fddc324-4e03-4bfe-b185-3d77768dc91a") : new("6fddc324-4e03-4bfe-b185-3d77768dc910");
            hr = SoftwareBitmap.Create(_width, _height, 96, 96, format, out bitmap);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock);
            if (hr < 0) return hr;
            uint size, stride; byte* pixels;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride);
            if (hr < 0) return hr;
            using (var renderer = new Direct3D9SoftwareImageRenderer(pixels, size, stride, _width, _height, _floating))
            {
                hr = renderer.BeginOpacityLayer(mask.Opacity);
                if (hr < 0) return hr;
                hr = renderer.DrawTransformed(mask.Source, new(0, 0, width, height),
                    new(mapping, null, mask.ScalingMode, MilCompositingMode.SourceOver, sourceBox), mask.IsMilSource);
                if (hr < 0) return hr;
                hr = renderer.EndOpacityLayer();
                if (hr < 0) return hr;
            }
            Direct3D9Factory.Release(bitmapLock); bitmapLock = 0;
            Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &sourceId, &milSource);
            if (hr < 0) return hr;
            Guid effectId = new("00000521-a8f2-4877-ba0a-fd2b6645fb94");
            Matrix4x4 identity = Matrix4x4.Identity;
            return ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, void*, uint, nint*, int>)(*(void***)effectList)[4])(
                effectList, &effectId, 64, &identity, 1, &milSource);
        }
        finally { Direct3D9Factory.Release(milSource); Direct3D9Factory.Release(bitmapLock); Direct3D9Factory.Release(bitmap); }
    }
}
