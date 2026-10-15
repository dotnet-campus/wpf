using System.Numerics;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe partial class Direct3D9SoftwareImageRenderer
{
    private int AddTiledImageMask(nint effectList, GeneratedImageMask mask, nint source, uint sourceWidth, uint sourceHeight,
        GeneratedImageTransform mapping, MilRectD sourceBox, Guid format, Func<nint, int>? consumer = null)
    {
        var viewport = mask.Viewport;
        var a = mask.Transform.Apply(viewport.X, viewport.Y);
        var b = mask.Transform.Apply(viewport.X + viewport.Width, viewport.Y);
        var c = mask.Transform.Apply(viewport.X, viewport.Y + viewport.Height);
        double idealWidth = Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        double idealHeight = Math.Sqrt(Math.Pow(c.X - a.X, 2) + Math.Pow(c.Y - a.Y, 2));
        if (!double.IsFinite(idealWidth) || !double.IsFinite(idealHeight)
            || idealWidth > int.MaxValue || idealHeight > int.MaxValue) return Direct3D9Factory.InvalidArgumentHResult;
        if (idealWidth == 0 || idealHeight == 0) return 0;
        uint tileWidth = (uint)Math.Max(1, Math.Ceiling(idealWidth));
        uint tileHeight = (uint)Math.Max(1, Math.Ceiling(idealHeight));
        var tileToDevice = mask.Transform.Prepend(new(viewport.Width / tileWidth, viewport.Height / tileHeight, viewport.X, viewport.Y));
        if (!tileToDevice.TryInvert(out var deviceToTile)) return 0;
        nint tile = 0, tileLock = 0, repeated = 0, repeatedLock = 0, milSource = 0;
        try
        {
            int hr = SoftwareBitmap.Create(tileWidth, tileHeight, 96, 96, format, out tile);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)tile)[8])(tile, 0, 2, &tileLock);
            if (hr < 0) return hr;
            uint size, stride; byte* pixels;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)tileLock)[5])(tileLock, &size, &pixels);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)tileLock)[4])(tileLock, &stride);
            if (hr < 0) return hr;
            using (var renderer = new Direct3D9SoftwareImageRenderer(pixels, size, stride, tileWidth, tileHeight, _floating))
            {
                hr = renderer.DrawTransformed(source, new(0, 0, sourceWidth, sourceHeight),
                    new(deviceToTile.Prepend(mapping), null, mask.ScalingMode, MilCompositingMode.SourceOver, sourceBox), mask.IsMilSource);
                if (hr < 0) return hr;
            }
            Direct3D9Factory.Release(tileLock); tileLock = 0;
            hr = SoftwareBitmap.Create(_width, _height, 96, 96, format, out repeated);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)repeated)[8])(repeated, 0, 2, &repeatedLock);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)repeatedLock)[5])(repeatedLock, &size, &pixels);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)repeatedLock)[4])(repeatedLock, &stride);
            if (hr < 0) return hr;
            MilBitmapWrapMode wrap = mask.TileMode switch
            {
                MilTileMode.FlipX => MilBitmapWrapMode.FlipX,
                MilTileMode.FlipY => MilBitmapWrapMode.FlipY,
                MilTileMode.FlipXY => MilBitmapWrapMode.FlipXY,
                _ => MilBitmapWrapMode.Tile
            };
            using (var renderer = new Direct3D9SoftwareImageRenderer(pixels, size, stride, _width, _height, _floating))
            {
                hr = renderer.BeginOpacityLayer(mask.Opacity);
                if (hr < 0) return hr;
                hr = renderer.DrawTransformed(tile, new(0, 0, tileWidth, tileHeight),
                    new(tileToDevice, null, mask.ScalingMode, MilCompositingMode.SourceOver, PrefilterEnabled: false, WrapMode: wrap), false);
                if (hr < 0) return hr;
                hr = renderer.EndOpacityLayer();
                if (hr < 0) return hr;
            }
            Direct3D9Factory.Release(repeatedLock); repeatedLock = 0;
            if (consumer is not null) return consumer(repeated);
            Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)repeated)[0])(repeated, &sourceId, &milSource);
            if (hr < 0) return hr;
            Guid effectId = new("00000521-a8f2-4877-ba0a-fd2b6645fb94");
            Matrix4x4 identity = Matrix4x4.Identity;
            return ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, void*, uint, nint*, int>)(*(void***)effectList)[4])(
                effectList, &effectId, 64, &identity, 1, &milSource);
        }
        finally
        {
            Direct3D9Factory.Release(milSource); Direct3D9Factory.Release(repeatedLock); Direct3D9Factory.Release(repeated);
            Direct3D9Factory.Release(tileLock); Direct3D9Factory.Release(tile);
        }
    }

    internal int DrawBrush(GeneratedImageMask? brush, SoftwareImageCoverage? coverage, SoftwareImageDrawingContext context, MilColorF? solidColor = null)
    {
        if (solidColor is { } color) return DrawSolidBrush(color, coverage, context);
        if (brush is null) return 0;
        nint realization = 0;
        try
        {
            int result = RealizeBrush(brush, out realization);
            if (result < 0 || realization == 0) return result;
            return BitmapRenderTargetExports.DrawSoftwareRealization(this, realization, coverage,
                context with { WorldToDevice = GeneratedImageTransform.Identity, BitmapScalingMode = 3,
                    PrefilterEnabled = false });
        }
        finally { NativeImmediateSolidRealization.ReleaseOwner(realization); }
    }

    internal int RealizeBrush(GeneratedImageMask brush, out nint realization)
    {
        nint candidate = 0;
        realization = 0;
        try
        {
            int result = AddImageMask(0, brush, bitmap =>
            {
                nint source = 0;
                try
                {
                    Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
                    int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(
                        bitmap, &sourceId, &source);
                    if (hr < 0) return hr;
                    candidate = NativeImmediateSolidRealization.CreateBitmap(source);
                    return 0;
                }
                finally { Direct3D9Factory.Release(source); }
            });
            if (result < 0) return result;
            realization = candidate;
            candidate = 0;
            return result;
        }
        finally { NativeImmediateSolidRealization.ReleaseOwner(candidate); }
    }

    private int AddImageMask(nint effectList, GeneratedImageMask mask, Func<nint, int>? consumer = null)
    {
        nint bitmap = 0, bitmapLock = 0, milSource = 0, source = 0;
        try
        {
            int sourceResult = mask.RealizeDrawingSource(out source);
            if (sourceResult < 0) return sourceResult;
            double dpiX, dpiY;
            int hr = ((delegate* unmanaged[Stdcall]<nint, double*, double*, int>)(*(void***)source)[5])(source, &dpiX, &dpiY);
            if (hr < 0) return hr;
            float nativeDpiX = (float)dpiX, nativeDpiY = (float)dpiY;
            const float minimumDpi = 96f * (1f / 0xFFFFFF);
            // Native ImageBrush disables DPI scaling for negative or near-zero resolution.
            dpiX = nativeDpiX < 0 || Math.Abs(nativeDpiX) <= minimumDpi ? 96 : nativeDpiX;
            dpiY = nativeDpiY < 0 || Math.Abs(nativeDpiY) <= minimumDpi ? 96 : nativeDpiY;
            if (!double.IsFinite(dpiX) || !double.IsFinite(dpiY)) return Direct3D9Factory.InvalidArgumentHResult;
            uint width, height;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)source)[3])(source, &width, &height);
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
            if (mask.TileMode != MilTileMode.None && mask.Transform.TryInvert(out _))
                return AddTiledImageMask(effectList, mask, source, width, height, mapping, sourceBox, format, consumer);
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
                hr = renderer.DrawTransformed(source, new(0, 0, width, height),
                    new(mapping, null, mask.ScalingMode, MilCompositingMode.SourceOver, sourceBox), mask.IsMilSource);
                if (hr < 0) return hr;
                hr = renderer.EndOpacityLayer();
                if (hr < 0) return hr;
            }
            Direct3D9Factory.Release(bitmapLock); bitmapLock = 0;
            if (consumer is not null) return consumer(bitmap);
            Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &sourceId, &milSource);
            if (hr < 0) return hr;
            Guid effectId = new("00000521-a8f2-4877-ba0a-fd2b6645fb94");
            Matrix4x4 identity = Matrix4x4.Identity;
            return ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, void*, uint, nint*, int>)(*(void***)effectList)[4])(
                effectList, &effectId, 64, &identity, 1, &milSource);
        }
        finally { Direct3D9Factory.Release(milSource); Direct3D9Factory.Release(bitmapLock); Direct3D9Factory.Release(bitmap);
            if (mask.HasDrawingSource) Direct3D9Factory.Release(source); }
    }
}
