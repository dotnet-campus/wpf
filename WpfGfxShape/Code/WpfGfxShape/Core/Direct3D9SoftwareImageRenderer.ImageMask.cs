using System.Numerics;
using WpfGfxShape.Abi;

namespace WpfGfxShape.Core;

internal sealed unsafe partial class Direct3D9SoftwareImageRenderer
{
    private int AddTiledImageMask(nint effectList, GeneratedImageMask mask, nint source, uint sourceWidth, uint sourceHeight,
        GeneratedImageTransform mapping, MilRectD sourceBox, Guid format, Func<nint, int>? consumer = null,
        Func<nint, GeneratedImageTransform, MilRectD?, MilBitmapWrapMode, int>? directConsumer = null)
    {
        var viewport = mask.Viewport;
        double idealWidth = viewport.Width * mask.ViewportScaleX;
        double idealHeight = viewport.Height * mask.ViewportScaleY;
        if (!double.IsFinite(idealWidth) || !double.IsFinite(idealHeight)) return Direct3D9Factory.InvalidArgumentHResult;
        if (idealWidth == 0 || idealHeight == 0) return 0;
        double width = GeneratedImageMask.CalculateSurfaceSize(idealWidth);
        double height = GeneratedImageMask.CalculateSurfaceSize(idealHeight);
        GeneratedImageMask.AdjustSurfaceSize(ref width, ref height);
        bool capturedDrawingTile = mask.HasCompleteDrawingTile || mask.HasCroppedDrawingTile;
        uint tileWidth = capturedDrawingTile ? sourceWidth : (uint)width;
        uint tileHeight = capturedDrawingTile ? sourceHeight : (uint)height;
        var tileToDevice = capturedDrawingTile ? mapping
            : mask.Transform.Prepend(new(viewport.Width / tileWidth, viewport.Height / tileHeight, viewport.X, viewport.Y));
        if (!tileToDevice.TryInvert(out var deviceToTile)) return 0;
        nint tile = 0, tileLock = 0, repeated = 0, repeatedLock = 0, milSource = 0;
        try
        {
            var sourceToTile = deviceToTile.Prepend(mapping);
            bool reuseDrawingTile = capturedDrawingTile && sourceWidth == tileWidth && sourceHeight == tileHeight
                && sourceBox == new MilRectD(0, 0, sourceWidth, sourceHeight);
            int hr;
            uint size, stride; byte* pixels;
            if (reuseDrawingTile)
            {
                // The captured drawing already owns the complete tile grid; another raster pass loses precision.
                ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source)[1])(source);
                tile = source;
            }
            else
            {
                hr = SoftwareBitmap.Create(tileWidth, tileHeight, 96, 96, format, out tile);
                if (hr < 0) return hr;
                hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)tile)[8])(tile, 0, 2, &tileLock);
                if (hr < 0) return hr;
                hr = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)tileLock)[5])(tileLock, &size, &pixels);
                if (hr < 0) return hr;
                hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)tileLock)[4])(tileLock, &stride);
                if (hr < 0) return hr;
                using (var renderer = new Direct3D9SoftwareImageRenderer(pixels, size, stride, tileWidth, tileHeight, _floating))
                {
                    hr = renderer.DrawTransformed(source, new(0, 0, sourceWidth, sourceHeight),
                        new(sourceToTile, null, mask.ScalingMode, MilCompositingMode.SourceOver, sourceBox,
                            WorkingFormat: mask.HasDrawingSource && !_floating ? SoftwareImageWorkingFormat.DrawingImagePbgra32 : SoftwareImageWorkingFormat.Native,
                            PrefilterEnabled: mask.HasDrawingSource ? false : null,
                            WrapMode: mask.HasDrawingSource && mask.ScalingMode != 3
                                ? MilBitmapWrapMode.Border : MilBitmapWrapMode.Extend), mask.IsMilSource);
                    if (hr < 0) return hr;
                }
                Direct3D9Factory.Release(tileLock); tileLock = 0;
            }
            if (directConsumer is not null)
            {
                Guid tileSourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
                hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)tile)[0])(tile, &tileSourceId, &milSource);
                if (hr < 0) return hr;
                MilBitmapWrapMode tileWrap = mask.TileMode switch
                {
                    MilTileMode.FlipX => MilBitmapWrapMode.FlipX,
                    MilTileMode.FlipY => MilBitmapWrapMode.FlipY,
                    MilTileMode.FlipXY => MilBitmapWrapMode.FlipXY,
                    _ => MilBitmapWrapMode.Tile
                };
                return directConsumer(milSource, tileToDevice, null, tileWrap);
            }
            var bounds = GetMaskBounds(mask.LayerBounds);
            uint repeatedWidth = (uint)(bounds.Right - bounds.Left), repeatedHeight = (uint)(bounds.Bottom - bounds.Top);
            if (repeatedWidth == 0 || repeatedHeight == 0) return 0;
            var toLocal = new GeneratedImageTransform(1, 1, -bounds.Left, -bounds.Top);
            hr = SoftwareBitmap.Create(repeatedWidth, repeatedHeight, 96, 96, format, out repeated);
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
            using (var renderer = new Direct3D9SoftwareImageRenderer(pixels, size, stride, repeatedWidth, repeatedHeight, _floating))
            {
                hr = renderer.DrawTransformed(tile, new(0, 0, tileWidth, tileHeight),
                    new(toLocal.Prepend(tileToDevice), null, mask.ScalingMode, MilCompositingMode.SourceOver,
                        WorkingFormat: mask.HasDrawingSource && !_floating ? SoftwareImageWorkingFormat.DrawingImagePbgra32 : SoftwareImageWorkingFormat.Native,
                        PrefilterEnabled: false, WrapMode: wrap, AlphaScale: (float)mask.Opacity), false);
                if (hr < 0) return hr;
            }
            Direct3D9Factory.Release(repeatedLock); repeatedLock = 0;
            if (consumer is not null) return consumer(repeated);
            Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)repeated)[0])(repeated, &sourceId, &milSource);
            if (hr < 0) return hr;
            Guid effectId = new("00000521-a8f2-4877-ba0a-fd2b6645fb94");
            Matrix4x4 effectMapping = Matrix4x4.CreateTranslation(bounds.Left, bounds.Top, 0);
            return ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, void*, uint, nint*, int>)(*(void***)effectList)[4])(
                effectList, &effectId, 64, &effectMapping, 1, &milSource);
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
                context with { WorldToDevice = GeneratedImageTransform.Identity,
                    BitmapScalingMode = brush.CanRealizeDirectly ? brush.ScalingMode : 3,
                    PrefilterEnabled = brush.CanRealizeDirectly ? null : false });
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
                    candidate = NativeImmediateSolidRealization.CreateBitmap(source, workingFormat: brush.HasDrawingSource && !_floating
                        ? SoftwareImageWorkingFormat.DrawingImagePbgra32 : SoftwareImageWorkingFormat.Native);
                    return 0;
                }
                finally { Direct3D9Factory.Release(source); }
            }, directConsumer: (source, mapping, sourceBox, wrap) =>
            {
                candidate = brush.HasDeviceAlignedDrawing
                    ? NativeImmediateSolidRealization.CreateBitmap(source, mapping, brush.Viewport, (float)brush.Opacity,
                        brush.ScalingMode == 3 ? MilBitmapWrapMode.Extend : MilBitmapWrapMode.Border, brush.Transform,
                        brush.HasDrawingSource && !_floating ? SoftwareImageWorkingFormat.DrawingImagePbgra32 : SoftwareImageWorkingFormat.Native)
                    : NativeImmediateSolidRealization.CreateBitmap(source, mapping, sourceBox, (float)brush.Opacity, wrap,
                        workingFormat: brush.HasDrawingSource && !_floating ? SoftwareImageWorkingFormat.DrawingImagePbgra32 : SoftwareImageWorkingFormat.Native);
                return 0;
            });
            if (result < 0) return result;
            realization = candidate;
            candidate = 0;
            return result;
        }
        finally { NativeImmediateSolidRealization.ReleaseOwner(candidate); }
    }

    private int AddVisibleTileMask(nint effectList, GeneratedImageMask[] tiles, Func<nint, int>? consumer, Direct3D9SurfaceRect? layerBounds)
    {
        if (!_floating)
        {
            // Partition contributions must be combined before quantizing the single brush.
            byte[] accumulation = new byte[checked((int)((ulong)_width * _height * 16))];
            fixed (byte* pixels = accumulation)
            {
                using var renderer = new Direct3D9SoftwareImageRenderer(pixels, (uint)accumulation.Length,
                    checked(_width * 16), _width, _height, true);
                return renderer.AddVisibleTileMask(effectList, tiles, consumer, layerBounds);
            }
        }
        nint bitmap = 0, bitmapLock = 0, source = 0;
        try
        {
            Guid format = _floating ? new("6fddc324-4e03-4bfe-b185-3d77768dc91a") : new("6fddc324-4e03-4bfe-b185-3d77768dc910");
            var bounds = GetMaskBounds(layerBounds);
            uint localWidth = (uint)(bounds.Right - bounds.Left), localHeight = (uint)(bounds.Bottom - bounds.Top);
            if (localWidth == 0 || localHeight == 0) return 0;
            int hr = SoftwareBitmap.Create(localWidth, localHeight, 96, 96, format, out bitmap);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock);
            if (hr < 0) return hr;
            uint size, stride; byte* pixels;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride);
            if (hr < 0) return hr;
            byte* destinationPixels = pixels;
            uint destinationStride = stride;
            foreach (var tile in tiles)
            {
                hr = AddImageMask(0, tile, image =>
                {
                    nint tileLock = 0;
                    try
                    {
                        int result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)image)[8])(
                            image, 0, 1, &tileLock);
                        if (result < 0) return result;
                        uint tileSize, tileStride; byte* tilePixels;
                        result = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)tileLock)[5])(
                            tileLock, &tileSize, &tilePixels);
                        if (result < 0) return result;
                        result = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)tileLock)[4])(tileLock, &tileStride);
                        if (result < 0) return result;
                        ulong rowBytes = (ulong)localWidth * (uint)BytesPerPixel;
                        ulong required = (ulong)(localHeight - 1) * tileStride + rowBytes;
                        if (tilePixels == null || tileStride < rowBytes || required > tileSize)
                            return Direct3D9Factory.InvalidArgumentHResult;
                        // Tiles partition one brush, not overlapping SourceOver layers.
                        for (uint y = 0; y < localHeight; y++)
                        for (uint x = 0; x < localWidth; x++)
                        {
                            if (_floating)
                            {
                                float* destination = (float*)(destinationPixels + y * destinationStride) + x * 4;
                                float* contribution = (float*)(tilePixels + y * tileStride) + x * 4;
                                for (int component = 0; component < 4; component++)
                                    destination[component] += contribution[component];
                            }
                            else
                            {
                                byte* destination = destinationPixels + y * destinationStride + x * 4;
                                byte* contribution = tilePixels + y * tileStride + x * 4;
                                for (int component = 0; component < 4; component++)
                                    destination[component] = (byte)Math.Min(255, destination[component] + contribution[component]);
                            }
                        }
                        return 0;
                    }
                    finally { Direct3D9Factory.Release(tileLock); }
                });
                if (hr < 0) return hr;
            }
            Direct3D9Factory.Release(bitmapLock); bitmapLock = 0;
            if (consumer is not null) return consumer(bitmap);
            Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &sourceId, &source);
            if (hr < 0) return hr;
            Guid effectId = new("00000521-a8f2-4877-ba0a-fd2b6645fb94");
            Matrix4x4 effectMapping = Matrix4x4.CreateTranslation(bounds.Left, bounds.Top, 0);
            return ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, void*, uint, nint*, int>)(*(void***)effectList)[4])(
                effectList, &effectId, 64, &effectMapping, 1, &source);
        }
        finally { Direct3D9Factory.Release(source); Direct3D9Factory.Release(bitmapLock); Direct3D9Factory.Release(bitmap); }
    }

    private Direct3D9SurfaceRect GetMaskBounds(Direct3D9SurfaceRect? requestedBounds)
    {
        var requested = requestedBounds ?? TargetBounds;
        int left = Math.Clamp(requested.Left, 0, checked((int)_width));
        int top = Math.Clamp(requested.Top, 0, checked((int)_height));
        return new(left, top, Math.Clamp(requested.Right, left, checked((int)_width)),
            Math.Clamp(requested.Bottom, top, checked((int)_height)));
    }

    private int AddImageMask(nint effectList, GeneratedImageMask mask, Func<nint, int>? consumer = null,
        Func<nint, GeneratedImageTransform, MilRectD?, MilBitmapWrapMode, int>? directConsumer = null)
    {
        if (mask.VisibleTiles is { } tiles) return AddVisibleTileMask(effectList, tiles, consumer, mask.LayerBounds);
        nint bitmap = 0, bitmapLock = 0, milSource = 0, source = 0;
        bool sourceOwned = false;
        try
        {
            int sourceResult = mask.RealizeDrawingSource(out source, out sourceOwned, _floating);
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
            left = Math.Max(0, left * dpiX / 96);
            top = Math.Max(0, top * dpiY / 96);
            right = Math.Min(width, right * dpiX / 96);
            bottom = Math.Min(height, bottom * dpiY / 96);
            var sourceBox = new MilRectD(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
            if (mask.DrawingPixelToDevice is { } drawingMapping)
            {
                mapping = drawingMapping;
                sourceBox = new(0, 0, width, height);
            }
            if (directConsumer is not null && mask.CanRealizeDirectly && mask.TileMode == MilTileMode.None)
            {
                if (mask.IsMilSource)
                    return directConsumer(source, mapping, sourceBox, MilBitmapWrapMode.Extend);
                Guid directSourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
                hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)source)[0])(
                    source, &directSourceId, &milSource);
                if (hr < 0) return hr;
                return directConsumer(milSource, mapping, sourceBox, MilBitmapWrapMode.Extend);
            }
            Guid format = _floating ? new("6fddc324-4e03-4bfe-b185-3d77768dc91a") : new("6fddc324-4e03-4bfe-b185-3d77768dc910");
            if (mask.TileMode != MilTileMode.None && mask.Transform.TryInvert(out _))
                return AddTiledImageMask(effectList, mask, source, width, height, mapping, sourceBox, format, consumer, mask.CanRealizeDirectly ? directConsumer : null);
            var requested = mask.LayerBounds ?? TargetBounds;
            int maskLeft = Math.Clamp(requested.Left, 0, (int)_width), maskTop = Math.Clamp(requested.Top, 0, (int)_height);
            int maskRight = Math.Clamp(requested.Right, maskLeft, (int)_width), maskBottom = Math.Clamp(requested.Bottom, maskTop, (int)_height);
            if (maskRight == maskLeft || maskBottom == maskTop) return 0;
            uint maskWidth = (uint)(maskRight - maskLeft), maskHeight = (uint)(maskBottom - maskTop);
            var toLocal = new GeneratedImageTransform(1, 1, -maskLeft, -maskTop);
            hr = SoftwareBitmap.Create(maskWidth, maskHeight, 96, 96, format, out bitmap);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock);
            if (hr < 0) return hr;
            uint size, stride; byte* pixels;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels);
            if (hr < 0) return hr;
            hr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride);
            if (hr < 0) return hr;
            SoftwareImageCoverage? sourceClip = null;
            if (mask.HasDeviceAlignedDrawing && !SoftwareImageCoverage.TryCreate(mask.Viewport, toLocal.Prepend(mask.Transform),
                out sourceClip, new(0, 0, (int)maskWidth, (int)maskHeight)))
                return Direct3D9Factory.InvalidArgumentHResult;
            if (mask.HasDeviceAlignedDrawing)
            {
                var clipMapping = toLocal.Prepend(mask.Transform);
                var a = clipMapping.Apply(mask.Viewport.X, mask.Viewport.Y);
                var b = clipMapping.Apply(mask.Viewport.X + mask.Viewport.Width, mask.Viewport.Y);
                var c = clipMapping.Apply(mask.Viewport.X + mask.Viewport.Width, mask.Viewport.Y + mask.Viewport.Height);
                var d = clipMapping.Apply(mask.Viewport.X, mask.Viewport.Y + mask.Viewport.Height);
                MilPoint2D[] polygon = [new(a.X, a.Y), new(b.X, b.Y), new(c.X, c.Y), new(d.X, d.Y)];
                if (polygon.All(point => Math.Abs(point.X) < 524288 && Math.Abs(point.Y) < 524288))
                {
                    if (!SoftwarePathCoverage.TryCreateFigures([polygon], MilFillMode.Winding, out var path))
                        return Direct3D9Factory.InvalidArgumentHResult;
                    if (path is null) return Direct3D9Factory.UnexpectedHResult;
                    sourceClip = new SoftwareImageCoverage(path);
                }
            }
            using (var renderer = new Direct3D9SoftwareImageRenderer(pixels, size, stride, maskWidth, maskHeight, _floating))
            {
                hr = renderer.DrawTransformed(source, new(0, 0, width, height),
                    new(toLocal.Prepend(mapping), null, mask.ScalingMode, MilCompositingMode.SourceOver, sourceBox,
                        WorkingFormat: mask.HasDrawingSource && !_floating ? SoftwareImageWorkingFormat.DrawingImagePbgra32 : SoftwareImageWorkingFormat.Native,
                        PrefilterEnabled: mask.HasDrawingSource ? false : null,
                        GeometryMask: sourceClip,
                        WrapMode: mask.HasDrawingSource && !mask.IsPartitionedDrawingTile && mask.ScalingMode != 3
                            ? MilBitmapWrapMode.Border : MilBitmapWrapMode.Extend,
                        AlphaScale: (float)mask.Opacity), mask.IsMilSource);
                if (hr < 0) return hr;
            }
            Direct3D9Factory.Release(bitmapLock); bitmapLock = 0;
            if (consumer is not null) return consumer(bitmap);
            Guid sourceId = new("dd0bf622-0650-4a1e-b20f-4b4ab6edfca3");
            hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)bitmap)[0])(bitmap, &sourceId, &milSource);
            if (hr < 0) return hr;
            Guid effectId = new("00000521-a8f2-4877-ba0a-fd2b6645fb94");
            Matrix4x4 effectMapping = Matrix4x4.CreateTranslation(maskLeft, maskTop, 0);
            return ((delegate* unmanaged[Stdcall]<nint, Guid*, uint, void*, uint, nint*, int>)(*(void***)effectList)[4])(
                effectList, &effectId, 64, &effectMapping, 1, &milSource);
        }
        finally { Direct3D9Factory.Release(milSource); Direct3D9Factory.Release(bitmapLock); Direct3D9Factory.Release(bitmap);
            if (sourceOwned) Direct3D9Factory.Release(source); }
    }
}
