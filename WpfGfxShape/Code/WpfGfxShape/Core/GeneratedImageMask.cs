namespace WpfGfxShape.Core;

internal sealed unsafe class GeneratedImageMask : IDisposable
{
    internal nint Source { get; private set; }
    internal bool IsMilSource { get; }
    internal MilRectD Viewbox { get; }
    internal MilRectD Viewport { get; }
    internal bool RelativeViewbox { get; }
    internal MilStretch Stretch { get; }
    internal MilHorizontalAlignment AlignmentX { get; }
    internal MilVerticalAlignment AlignmentY { get; }
    internal GeneratedImageTransform Transform { get; }
    internal double Opacity { get; }
    internal uint ScalingMode { get; }

    private GeneratedImageMask(nint source, bool isMilSource, MilRectD viewbox, MilRectD viewport,
        GeneratedImageTransform transform, double opacity, uint scalingMode, bool relativeViewbox, MilStretch stretch, MilHorizontalAlignment alignmentX, MilVerticalAlignment alignmentY)
    {
        Source = source; IsMilSource = isMilSource; Viewbox = viewbox; Viewport = viewport;
        Transform = transform; Opacity = opacity; ScalingMode = scalingMode; RelativeViewbox = relativeViewbox;
        Stretch = stretch; AlignmentX = alignmentX; AlignmentY = alignmentY;
    }

    internal static bool TryCapture(GeneratedImageBrushResource brush, GeneratedImageTransform parent, uint scalingMode, out GeneratedImageMask? mask, MilRectD? contentBounds = null)
    {
        mask = null;
        // Relative units require the content bounds, not the target surface bounds.
        if (brush.TileMode != MilTileMode.None
            || !GeneratedImageTransform.TryResolve(brush.Transform, out var transform)) return false;
        var viewport = brush.CurrentViewport; var viewbox = brush.CurrentViewbox;
        if (brush.ViewportUnits == MilBrushMappingMode.RelativeToBoundingBox || brush.RelativeTransform is not null)
        {
            if (contentBounds is not { } bounds || !Valid(bounds)) return false;
            if (brush.ViewportUnits == MilBrushMappingMode.RelativeToBoundingBox)
                viewport = new(bounds.X + viewport.X * bounds.Width, bounds.Y + viewport.Y * bounds.Height,
                    viewport.Width * bounds.Width, viewport.Height * bounds.Height);
            if (!GeneratedImageTransform.TryResolve(brush.RelativeTransform, out var relative)) return false;
            var fromUnit = new GeneratedImageTransform(bounds.Width, bounds.Height, bounds.X, bounds.Y);
            if (!fromUnit.TryInvert(out var toUnit)) return false;
            transform = transform.Prepend(fromUnit.Prepend(relative).Prepend(toUnit));
        }
        double opacity = brush.CurrentOpacity;
        if (!Valid(viewport) || !Valid(viewbox) || !double.IsFinite(opacity)) return false;
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
                brush.ViewboxUnits == MilBrushMappingMode.RelativeToBoundingBox, brush.Stretch, brush.AlignmentX, brush.AlignmentY);
            source = 0;
            return true;
        }
        finally { Direct3D9Factory.Release(source); }
    }

    private static bool Valid(MilRectD r) => double.IsFinite(r.X) && double.IsFinite(r.Y)
        && double.IsFinite(r.Width) && double.IsFinite(r.Height) && r.Width > 0 && r.Height > 0;

    public void Dispose()
    {
        nint source = Source; Source = 0;
        Direct3D9Factory.Release(source);
    }
}
