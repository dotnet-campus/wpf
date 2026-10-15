namespace WpfGfxShape.Core;

internal sealed unsafe class GeneratedImageMask : IDisposable
{
    internal nint Source { get; private set; }
    private GeneratedVisualRenderer.DrawingSnapshot? _drawing;
    [ThreadStatic] private static HashSet<GeneratedProtocolResource>? _activeDrawings;

    internal int RealizeDrawingSource(out nint source)
    {
        source = Source;
        return _drawing is null ? 0 : _drawing.Realize(out source);
    }
    internal bool HasDrawingSource => _drawing is not null;
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

    private GeneratedImageMask(nint source, bool isMilSource, MilRectD viewbox, MilRectD viewport,
        GeneratedImageTransform transform, double opacity, uint scalingMode, bool relativeViewbox, MilStretch stretch, MilHorizontalAlignment alignmentX, MilVerticalAlignment alignmentY, MilTileMode tileMode)
    {
        Source = source; IsMilSource = isMilSource; Viewbox = viewbox; Viewport = viewport;
        Transform = transform; Opacity = opacity; ScalingMode = scalingMode; RelativeViewbox = relativeViewbox;
        Stretch = stretch; AlignmentX = alignmentX; AlignmentY = alignmentY; TileMode = tileMode;
    }

    internal static bool TryCapture(GeneratedImageBrushResource brush, GeneratedImageTransform parent, uint scalingMode, out GeneratedImageMask? mask, MilRectD? contentBounds = null)
    {
        mask = null;
        // Native DoesContainContent checks both the resource and its bitmap content before mapping.
        if (brush.Source is null or GeneratedBitmapSourceResource { Bitmap: 0 }
            or GeneratedDoubleBufferedBitmapResource { HasContent: false }) return true;
        // Relative units require the content bounds, not the target surface bounds.
        if (brush.TileMode is not (MilTileMode.None or MilTileMode.Tile or MilTileMode.FlipX or MilTileMode.FlipY or MilTileMode.FlipXY)
            || !GeneratedImageTransform.TryResolve(brush.Transform, out var transform)) return false;
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
            if (!GeneratedImageTransform.TryResolve(brush.RelativeTransform, out var relative)) return false;
            var fromUnit = new GeneratedImageTransform(bounds.Width, bounds.Height, bounds.X, bounds.Y);
            if (!fromUnit.TryInvert(out var toUnit)) return false;
            transform = transform.Prepend(fromUnit.Prepend(relative).Prepend(toUnit));
        }
        double opacity = brush.CurrentOpacity;
        if (!double.IsFinite(opacity)) return false;
        if (Empty(viewport) || Empty(viewbox)) return true;
        if (!Valid(viewport) || !Valid(viewbox)) return false;
        if (brush.Source is GeneratedDrawingImageResource drawingImage)
        {
            var active = _activeDrawings ??= [];
            if (active.Count >= 256 || !active.Add(drawingImage)) return false;
            GeneratedVisualRenderer.DrawingSnapshot? drawing = null;
            try
            {
                if (!GeneratedVisualRenderer.DrawingSnapshot.TryCapture(drawingImage, viewbox,
                    brush.ViewboxUnits == MilBrushMappingMode.RelativeToBoundingBox, viewport,
                    parent.Prepend(transform), brush.Stretch, out drawing)) return false;
                if (drawing is null) return true;
                var bounds = drawing.Bounds;
                if (brush.ViewboxUnits == MilBrushMappingMode.RelativeToBoundingBox)
                    viewbox = new(viewbox.X * drawing.Width, viewbox.Y * drawing.Height,
                        viewbox.Width * drawing.Width, viewbox.Height * drawing.Height);
                else
                    viewbox = new((viewbox.X - bounds.X) * drawing.Width / bounds.Width,
                        (viewbox.Y - bounds.Y) * drawing.Height / bounds.Height,
                        viewbox.Width * drawing.Width / bounds.Width, viewbox.Height * drawing.Height / bounds.Height);
                mask = new(0, false, viewbox, viewport, parent.Prepend(transform), Math.Clamp(opacity, 0, 1),
                    scalingMode, false, brush.Stretch, brush.AlignmentX, brush.AlignmentY, brush.TileMode) { _drawing = drawing };
                drawing = null;
                return true;
            }
            finally { drawing?.Dispose(); active.Remove(drawingImage); }
        }
        nint source = brush.Source switch
        {
            GeneratedBitmapSourceResource bitmap => bitmap.AcquireBitmapSource(),
            GeneratedDoubleBufferedBitmapResource bitmap => bitmap.AcquireBitmapSource(),
            _ => 0
        };
        if (source == 0) return false;
        try
        {
            mask = new(source, brush.Source is GeneratedBitmapSourceResource, viewbox, viewport,
                parent.Prepend(transform), Math.Clamp(opacity, 0, 1), scalingMode,
                brush.ViewboxUnits == MilBrushMappingMode.RelativeToBoundingBox, brush.Stretch, brush.AlignmentX, brush.AlignmentY, brush.TileMode);
            source = 0;
            return true;
        }
        finally { Direct3D9Factory.Release(source); }
    }

    // Native tile mapping treats NaN rectangles as empty rather than realization failures.
    private static bool Empty(MilRectD r) => !(r.Width > 0) || !(r.Height > 0)
        || double.IsNaN(r.X) || double.IsNaN(r.Y);

    private static bool Valid(MilRectD r) => double.IsFinite(r.X) && double.IsFinite(r.Y)
        && double.IsFinite(r.Width) && double.IsFinite(r.Height) && r.Width > 0 && r.Height > 0;

    public void Dispose()
    {
        nint source = Source; Source = 0;
        Direct3D9Factory.Release(source);
        _drawing?.Dispose(); _drawing = null;
    }
}
