namespace WpfGfxShape.Core;

internal sealed unsafe class GeneratedImageMask : IDisposable
{
    internal nint Source { get; private set; }
    private GeneratedVisualRenderer.DrawingSnapshot? _drawing;
    private GeneratedVisualRenderer.VisualSnapshot? _visual;
    private bool _disposed;
    internal GeneratedImageMask[]? VisibleTiles { get; private set; }
    [ThreadStatic] private static HashSet<GeneratedProtocolResource>? _activeDrawings;

    internal int RealizeDrawingSource(out nint source, out bool owned, bool floating = true)
    {
        source = 0;
        owned = false;
        if (_disposed) return Direct3D9Factory.WgxInvalidCallHResult;
        if (_visual is not null)
        {
            int result = _visual.Realize(out source, floating);
            owned = source != 0;
            return result;
        }
        if (_drawing is not null)
        {
            int result = _drawing.Realize(out source, floating);
            owned = source != 0;
            return result;
        }
        source = Source;
        return source == 0 ? Direct3D9Factory.WgxInvalidCallHResult : 0;
    }
    private Direct3D9SurfaceRect? _layerBounds;
    internal Direct3D9SurfaceRect? LayerBounds
    {
        get => _layerBounds;
        set
        {
            _layerBounds = value;
            if (VisibleTiles is { } tiles)
                foreach (var tile in tiles) tile.LayerBounds = value;
        }
    }
    internal bool HasDrawingSource => _drawing is not null || _visual is not null;
    internal bool CanRealizeDirectly => VisibleTiles is null
        && (IsMilSource || HasDrawingSource);
    internal bool IsPartitionedDrawingTile { get; private set; }
    internal bool HasCompleteDrawingTile => _drawing?.IsCompleteViewportTile == true;
    internal bool HasDeviceAlignedDrawing => _drawing?.IsDeviceAligned == true;
    internal bool HasCroppedDrawingTile => _drawing?.IsCroppedViewportTile == true;
    internal GeneratedImageTransform? DrawingPixelToDevice => _drawing?.PixelToDevice;
    internal bool IsMilSource { get; }
    internal MilRectD Viewbox { get; }
    internal MilRectD Viewport { get; }
    internal bool RelativeViewbox { get; }
    internal MilTileMode TileMode { get; }
    internal MilStretch Stretch { get; }
    internal MilHorizontalAlignment AlignmentX { get; }
    internal MilVerticalAlignment AlignmentY { get; }
    internal GeneratedImageTransform Transform { get; }
    internal double Opacity { get; }
    internal uint ScalingMode { get; }
    internal double ViewportScaleX { get; private set; }
    internal double ViewportScaleY { get; private set; }

    internal static double CalculateSurfaceSize(double extent)
        => Math.Max(1, Math.Min(Math.Ceiling(extent), int.MaxValue));

    internal static void AdjustSurfaceSize(ref double width, ref double height)
    {
        // Native 2D viewport-aligned intermediates reserve two texels for non-power-of-two tiling.
        const double maximum = 2048;
        double originalWidth = width, originalHeight = height;
        if (originalWidth > maximum - 2)
            width = originalHeight > maximum - 2 ? maximum : maximum - 2;
        if (originalHeight > maximum - 2)
            height = width > maximum - 2 ? maximum : maximum - 2;
    }

    private GeneratedImageMask(nint source, bool isMilSource, MilRectD viewbox, MilRectD viewport,
        GeneratedImageTransform transform, double opacity, uint scalingMode, bool relativeViewbox, MilStretch stretch, MilHorizontalAlignment alignmentX, MilVerticalAlignment alignmentY, MilTileMode tileMode)
    {
        Source = source; IsMilSource = isMilSource; Viewbox = viewbox; Viewport = viewport;
        Transform = transform; Opacity = opacity; ScalingMode = scalingMode; RelativeViewbox = relativeViewbox;
        Stretch = stretch; AlignmentX = alignmentX; AlignmentY = alignmentY; TileMode = tileMode;
    }

    internal static bool TryCapture(GeneratedImageBrushResource brush, GeneratedImageTransform parent, uint scalingMode, out GeneratedImageMask? mask, MilRectD? contentBounds = null, Direct3D9SurfaceRect? deviceClip = null, bool expandTiles = true, MilRectD? renderBounds = null, GeneratedImageTransform? worldTransform = null, bool infiniteRenderBounds = false, MilRectD? worldBounds = null)
    {
        mask = null;
        int hr = FrozenBrush.CapturePure(brush, out var frozen);
        if (hr < 0) return false;
        using (frozen)
            return TryCaptureFrozen(frozen!, parent, scalingMode, out mask, contentBounds, deviceClip,
                expandTiles, renderBounds, worldTransform, infiniteRenderBounds, worldBounds);
    }

    internal static bool TryCaptureFrozen(FrozenBrush brush, GeneratedImageTransform parent, uint scalingMode,
        out GeneratedImageMask? mask, MilRectD? contentBounds = null, Direct3D9SurfaceRect? deviceClip = null,
        bool expandTiles = true, MilRectD? renderBounds = null, GeneratedImageTransform? worldTransform = null,
        bool infiniteRenderBounds = false, MilRectD? worldBounds = null)
    {
        mask = null;
        worldTransform ??= parent;
        if (!infiniteRenderBounds)
            renderBounds ??= contentBounds is { } shapeBounds ? parent.Apply(shapeBounds) : null;
        // Native DoesContainContent checks both the resource and its bitmap content before mapping.
        if (!brush.HasContent) return true;
        // Relative units require the content bounds, not the target surface bounds.
        if (brush.TileMode is not (MilTileMode.None or MilTileMode.Tile or MilTileMode.FlipX or MilTileMode.FlipY or MilTileMode.FlipXY)
            ) return false;
        var transform = brush.Transform;
        var viewport = brush.CurrentViewport; var viewbox = brush.CurrentViewbox;
        if (Empty(viewport) || Empty(viewbox)) return true;
        if (brush.ViewportUnits == MilBrushMappingMode.RelativeToBoundingBox || brush.RelativeTransform is not null)
        {
            if (contentBounds is not { } bounds) return false;
            if (Empty(bounds)) return true;
            if (!Valid(bounds)) return false;
            if (brush.ViewportUnits == MilBrushMappingMode.RelativeToBoundingBox)
                viewport = new(bounds.X + viewport.X * bounds.Width, bounds.Y + viewport.Y * bounds.Height,
                    viewport.Width * bounds.Width, viewport.Height * bounds.Height);
            if (Empty(viewport)) return true;
            var relative = brush.RelativeTransform ?? GeneratedImageTransform.Identity;
            var fromUnit = new GeneratedImageTransform(bounds.Width, bounds.Height, bounds.X, bounds.Y);
            if (!fromUnit.TryInvert(out var toUnit)) return false;
            transform = transform.Prepend(fromUnit.Prepend(relative).Prepend(toUnit));
        }
        double opacity = brush.CurrentOpacity;
        if (!double.IsFinite(opacity)) return false;
        if (Empty(viewport) || Empty(viewbox)) return true;
        if (!Valid(viewport) || !Valid(viewbox)) return false;
        var tileMode = expandTiles ? brush.TileMode : MilTileMode.None;
        var deviceTransform = parent.Prepend(transform);
        // Native viewport intermediates decompose the two spaces before multiplying their scales.
        if (!transform.TryDecompose(out var brushScale, out _)
            || !worldTransform.Value.TryDecompose(out var worldScale, out _)) return true;
        double viewportScaleX = brushScale.ScaleX * worldScale.ScaleX;
        double viewportScaleY = brushScale.ScaleY * worldScale.ScaleY;
        // Drawing snapshots own integer-grid clipping and flip shifts; oversized grids require local tiles
        // because the native integer clipping guard cannot represent their visible interval.
        bool requiresLocalTile = brush.Source is not GeneratedDrawingImageResource
            || viewport.Width * viewportScaleX > int.MaxValue
            || viewport.Height * viewportScaleY > int.MaxValue;
        if (requiresLocalTile && tileMode != MilTileMode.None && deviceClip is { } clip
            && deviceTransform.TryInvert(out var inverse))
        {
            var visible = inverse.Apply(new(clip.Left, clip.Top, clip.Right - clip.Left, clip.Bottom - clip.Top));
            double ix = Math.Floor((visible.X - viewport.X) / viewport.Width);
            double iy = Math.Floor((visible.Y - viewport.Y) / viewport.Height);
            double originX = viewport.X + ix * viewport.Width, originY = viewport.Y + iy * viewport.Height;
            if (visible.Width <= viewport.Width && visible.Height <= viewport.Height
                && (visible.X + visible.Width > originX + viewport.Width
                    || visible.Y + visible.Height > originY + viewport.Height))
            {
                var tiles = new List<GeneratedImageMask>();
                bool captured = false;
                try
                {
                    int nx = visible.X + visible.Width > originX + viewport.Width ? 2 : 1;
                    int ny = visible.Y + visible.Height > originY + viewport.Height ? 2 : 1;
                    for (int y = 0; y < ny; y++)
                    for (int x = 0; x < nx; x++)
                    {
                        double indexX = ix + x, indexY = iy + y;
                        bool reflectX = tileMode is MilTileMode.FlipX or MilTileMode.FlipXY && Math.Abs(indexX % 2) == 1;
                        bool reflectY = tileMode is MilTileMode.FlipY or MilTileMode.FlipXY && Math.Abs(indexY % 2) == 1;
                        double ox = viewport.X + indexX * viewport.Width, oy = viewport.Y + indexY * viewport.Height;
                        var tileTransform = new GeneratedImageTransform(reflectX ? -1 : 1, reflectY ? -1 : 1,
                            reflectX ? ox + viewport.Width + viewport.X : ox - viewport.X,
                            reflectY ? oy + viewport.Height + viewport.Y : oy - viewport.Y);
                        if (!transform.TryInvert(out var inverseBrush)) return false;
                        var tileParent = deviceTransform.Prepend(tileTransform).Prepend(inverseBrush);
                        if (!TryCaptureFrozen(brush, tileParent, scalingMode, out var tileMask,
                            contentBounds, deviceClip, expandTiles: false, renderBounds: renderBounds,
                            worldTransform: worldTransform, infiniteRenderBounds: infiniteRenderBounds, worldBounds: worldBounds)) return false;
                        if (tileMask is not null) tiles.Add(tileMask);
                    }
                    if (tiles.Count == 0) return true;
                    mask = new(0, false, viewbox, viewport, deviceTransform, 1, scalingMode,
                        false, brush.Stretch, brush.AlignmentX, brush.AlignmentY, MilTileMode.None)
                        { VisibleTiles = tiles.ToArray() };
                    captured = true;
                    return true;
                }
                finally { if (!captured) for (int i = tiles.Count - 1; i >= 0; i--) tiles[i].Dispose(); }
            }
            if (visible.X + visible.Width <= originX + viewport.Width
                && visible.Y + visible.Height <= originY + viewport.Height)
            {
                bool flipX = tileMode is MilTileMode.FlipX or MilTileMode.FlipXY && Math.Abs(ix % 2) == 1;
                bool flipY = tileMode is MilTileMode.FlipY or MilTileMode.FlipXY && Math.Abs(iy % 2) == 1;
                var tile = new GeneratedImageTransform(flipX ? -1 : 1, flipY ? -1 : 1,
                    flipX ? originX + viewport.Width + viewport.X : originX - viewport.X,
                    flipY ? originY + viewport.Height + viewport.Y : originY - viewport.Y);
                deviceTransform = deviceTransform.Prepend(tile);
                tileMode = MilTileMode.None;
            }
        }
        if (brush.Source is GeneratedDrawingImageResource drawingImage)
        {
            var active = _activeDrawings ??= [];
            if (active.Count >= 256 || !active.Add(drawingImage)) return false;
            GeneratedVisualRenderer.DrawingSnapshot? drawing = null;
            try
            {
                if (!GeneratedVisualRenderer.DrawingSnapshot.TryCapture(drawingImage, viewbox,
                    brush.ViewboxUnits == MilBrushMappingMode.RelativeToBoundingBox, viewport,
                    deviceTransform, brush.Stretch, brush.AlignmentX, brush.AlignmentY,
                    tileMode, deviceClip, scalingMode, viewportScaleX, viewportScaleY, out drawing,
                    deviceAligned: brush.TileMode == MilTileMode.None,
                    renderBounds: renderBounds,
                    brushScaleX: brushScale.ScaleX,
                    brushScaleY: brushScale.ScaleY,
                    worldScaleX: worldScale.ScaleX,
                    worldScaleY: worldScale.ScaleY,
                    worldTransform: worldTransform, worldBounds: worldBounds, frozen: brush.Drawing)) return false;
                if (drawing is null) return true;
                viewbox = new(0, 0, drawing.Width, drawing.Height);
                mask = new(0, false, viewbox, viewport, deviceTransform, Math.Clamp(opacity, 0, 1),
                    scalingMode, false, brush.Stretch, brush.AlignmentX, brush.AlignmentY, tileMode)
                    { _drawing = drawing, IsPartitionedDrawingTile = brush.TileMode != MilTileMode.None && tileMode == MilTileMode.None,
                        ViewportScaleX = viewportScaleX, ViewportScaleY = viewportScaleY };
                drawing = null;
                return true;
            }
            finally { drawing?.Dispose(); active.Remove(drawingImage); }
        }
        // External source acquisition can mutate the brush; mapping belongs to this frame.
        var sourceOwner = brush.Source;
        bool isMilSource = sourceOwner is GeneratedBitmapSourceResource;
        bool relativeViewbox = brush.ViewboxUnits == MilBrushMappingMode.RelativeToBoundingBox;
        var stretch = brush.Stretch;
        var alignmentX = brush.AlignmentX;
        var alignmentY = brush.AlignmentY;
        nint source = brush.AcquireSource();
        if (source == 0) return false;
        try
        {
            mask = new(source, isMilSource, viewbox, viewport,
                deviceTransform, Math.Clamp(opacity, 0, 1), scalingMode,
                relativeViewbox, stretch, alignmentX, alignmentY, tileMode)
                { ViewportScaleX = viewportScaleX, ViewportScaleY = viewportScaleY };
            source = 0;
            return true;
        }
        finally { Direct3D9Factory.Release(source); }
    }

    internal static int CaptureCacheBrush(GeneratedBitmapCacheBrushResource brush, GeneratedImageTransform parent,
        uint scalingMode, MilRectD consumerBounds, Direct3D9SurfaceRect? clip, bool floating,
        double dpiX, double dpiY, out GeneratedImageMask? mask)
    {
        mask = null;
        int hr = FrozenCacheRequest.CapturePure(brush, [], out var request);
        if (hr < 0) return hr;
        using (request)
            return CaptureCacheBrushFrozen(request!, parent, scalingMode, consumerBounds, clip, floating, dpiX, dpiY, out mask);
    }

    internal static int CaptureCacheBrushFrozen(FrozenCacheRequest brush, GeneratedImageTransform parent,
        uint scalingMode, MilRectD consumerBounds, Direct3D9SurfaceRect? clip, bool floating,
        double dpiX, double dpiY, out GeneratedImageMask? mask)
    {
        mask = null;
        double opacity = brush.CurrentOpacity;
        if (!Valid(consumerBounds) || !double.IsFinite(opacity))
            return Direct3D9Factory.InvalidArgumentHResult;
        var transform = brush.Transform;
        var relative = brush.RelativeTransform ?? GeneratedImageTransform.Identity;
        if (brush.RelativeTransform is not null)
        {
            var sizing = new GeneratedImageTransform(consumerBounds.Width, consumerBounds.Height, consumerBounds.X, consumerBounds.Y);
            if (!sizing.TryInvert(out var inverse)) return Direct3D9Factory.NonInvertibleMatrixHResult;
            transform = transform.Prepend(sizing.Prepend(relative).Prepend(inverse));
        }
        var device = parent.Prepend(transform);
        if (!device.TryInvert(out _)) return Direct3D9Factory.NonInvertibleMatrixHResult;
        int hr = brush.RealizeFrozen(consumerBounds, device, dpiX, dpiY, floating, scalingMode, out nint source);
        if (hr < 0) return hr;
        try
        {
            mask = new(source, false, new(0, 0, 1, 1), consumerBounds, device,
                Math.Clamp(opacity, 0, 1), scalingMode, true, MilStretch.Fill,
                MilHorizontalAlignment.Left, MilVerticalAlignment.Top, MilTileMode.None);
            source = 0;
            return 0;
        }
        finally { Direct3D9Factory.Release(source); }
    }

    internal static int CaptureVisualBrush(GeneratedVisualBrushResource brush, GeneratedImageTransform parent,
        uint scalingMode, MilRectD consumerBounds, Direct3D9SurfaceRect? clip, out GeneratedImageMask? mask)
    {
        mask = null;
        int result = FrozenBrush.CapturePure(brush, out var frozen);
        if (result < 0) return result;
        using (frozen)
            return CaptureVisualBrushFrozen(frozen!, parent, scalingMode, consumerBounds, clip, out mask);
    }

    internal static int CaptureVisualBrushFrozen(FrozenBrush brush, GeneratedImageTransform parent,
        uint scalingMode, MilRectD consumerBounds, Direct3D9SurfaceRect? clip, out GeneratedImageMask? mask)
    {
        mask = null;
        if (!brush.HasContent) return 0;
        if (brush.Visual is not { } root || brush.TileMode != MilTileMode.None)
            return Direct3D9Factory.NotImplementedHResult;
        GeneratedVisualRenderer.VisualSnapshot? snapshot = null;
        try
        {
            int hr = GeneratedImageContentBounds.GetVisualOuterBounds(root, out var content);
            if (hr < 0) return hr;
            var box = brush.CurrentViewbox;
            var viewport = brush.CurrentViewport;
            double opacity = brush.CurrentOpacity;
            var stretch = brush.Stretch;
            var alignmentX = brush.AlignmentX;
            var alignmentY = brush.AlignmentY;
            if (brush.ViewboxUnits == MilBrushMappingMode.RelativeToBoundingBox)
                box = new(content.X + box.X * content.Width, content.Y + box.Y * content.Height,
                    box.Width * content.Width, box.Height * content.Height);
            if (brush.ViewportUnits == MilBrushMappingMode.RelativeToBoundingBox)
                viewport = new(consumerBounds.X + viewport.X * consumerBounds.Width,
                    consumerBounds.Y + viewport.Y * consumerBounds.Height,
                    viewport.Width * consumerBounds.Width, viewport.Height * consumerBounds.Height);
            if (Empty(content) || Empty(box) || Empty(viewport)) return 0;
            if (!Valid(content) || !Valid(box) || !Valid(viewport) || !double.IsFinite(opacity))
                return Direct3D9Factory.InvalidArgumentHResult;
            var transform = brush.Transform;
            var relative = brush.RelativeTransform ?? GeneratedImageTransform.Identity;
            if (brush.RelativeTransform is not null)
            {
                var sizing = new GeneratedImageTransform(consumerBounds.Width, consumerBounds.Height, consumerBounds.X, consumerBounds.Y);
                if (!sizing.TryInvert(out var inverse)) return Direct3D9Factory.NonInvertibleMatrixHResult;
                transform = transform.Prepend(sizing.Prepend(relative).Prepend(inverse));
            }
            var device = parent.Prepend(transform);
            var extent = device.Apply(viewport);
            double w = Math.Ceiling(Math.Abs(extent.Width)), h = Math.Ceiling(Math.Abs(extent.Height));
            AdjustSurfaceSize(ref w, ref h);
            if (!double.IsFinite(w) || !double.IsFinite(h)) return Direct3D9Factory.InvalidArgumentHResult;
            uint width = (uint)Math.Max(1, w), height = (uint)Math.Max(1, h);
            // Preserve the viewport padding by capturing the whole viewbox before brush stretch.
            var mapping = new GeneratedImageTransform(width / box.Width, height / box.Height,
                -box.X * width / box.Width, -box.Y * height / box.Height);
            hr = GeneratedVisualRenderer.DrawingSnapshot.RealizeTree(root, content, mapping, width, height,
                scalingMode, false, out var drawing);
            if (hr >= 0 && drawing is not null) snapshot = new(drawing);
            if (hr < 0) return hr;
            mask = new(0, false, new(0, 0, width, height), viewport, device,
                Math.Clamp(opacity, 0, 1), scalingMode, false, stretch,
                alignmentX, alignmentY, MilTileMode.None) { _visual = snapshot };
            snapshot = null;
            return 0;
        }
        finally { snapshot?.Dispose(); }
    }

    // Native tile mapping treats NaN rectangles as empty rather than realization failures.
    private static bool Empty(MilRectD r) => !(r.Width > 0) || !(r.Height > 0)
        || double.IsNaN(r.X) || double.IsNaN(r.Y);

    private static bool Valid(MilRectD r) => double.IsFinite(r.X) && double.IsFinite(r.Y)
        && double.IsFinite(r.Width) && double.IsFinite(r.Height) && r.Width > 0 && r.Height > 0;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        nint source = Source; Source = 0;
        var drawing = _drawing; _drawing = null;
        var tiles = VisibleTiles; VisibleTiles = null;
        if (tiles is not null)
            for (int i = tiles.Length - 1; i >= 0; i--) tiles[i].Dispose();
        var visual = _visual; _visual = null;
        visual?.Dispose();
        drawing?.Dispose();
        Direct3D9Factory.Release(source);
    }
}
