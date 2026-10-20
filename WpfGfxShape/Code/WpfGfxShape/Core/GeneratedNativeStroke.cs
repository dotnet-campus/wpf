namespace WpfGfxShape.Core;

// Immutable scene data captured before any COM callback can mutate resources.
internal sealed record GeneratedNativeStroke(SoftwareStrokeContours.Figure[] Figures,
    float Width, float Miter, MilPenCap Start, MilPenCap End, MilPenCap Dash,
    MilPenJoin Join, float Offset, float[] Dashes, GeneratedImageTransform Transform, bool Fillable,
    MilFillMode FillRule)
{
    internal bool TryGetBounds(bool localSpace, out MilRectD bounds)
    {
        // CDrawingContext::GetStrokeBounds uses a solid pen to keep brush mapping
        // stable during dash animation, with tolerance adjusted to device resolution.
        using var native = new WpfGfxShape.Abi.NativeGeneratedPath(this with { Dashes = [], Offset = 0 });
        var output = localSpace ? GeneratedImageTransform.Identity : Transform;
        var matrix = System.Numerics.Matrix4x4.Identity;
        matrix.M11 = (float)output.ScaleX; matrix.M22 = (float)output.ScaleY;
        matrix.M12 = (float)output.M12; matrix.M21 = (float)output.M21;
        matrix.M41 = (float)output.X; matrix.M42 = (float)output.Y;
        return WpfGfxShape.Abi.NativeStrokeReader.Widen(native.ShapePointer, native.PenPointer,
            matrix, out _, out bounds, createCoverage: false, toleranceTransform: Transform, managedShape: true) >= 0;
    }

    internal static bool TryCaptureFill(GeneratedProtocolResource geometry, GeneratedImageTransform transform,
        out GeneratedNativeStroke? snapshot)
        => TryCapture(geometry, new GeneratedPenResource(0, 1, MilPenCap.Flat, MilPenCap.Flat,
            MilPenCap.Flat, MilPenJoin.Miter, null), transform, out snapshot, fillPrimitive: true);

    internal static bool TryCapture(GeneratedProtocolResource geometry, GeneratedPenResource pen,
        GeneratedImageTransform transform, out GeneratedNativeStroke? snapshot, bool fillPrimitive = false)
    {
        snapshot = null;
        if (!SoftwareGroupStrokeContours.TryRead(geometry, out var figures, preserveGaps: true, deviceTransform: transform, fillPrimitive: fillPrimitive))
            return false;
        float width = MathF.Abs((float)pen.CurrentThickness), miter = (float)pen.MiterLimit;
        if (!float.IsFinite(width) || !float.IsFinite(miter)) return false;
        float offset = (float)(pen.DashStyle?.CurrentOffset ?? 0);
        var source = pen.DashStyle?.Dashes ?? [];
        if (!float.IsFinite(offset)) return false;
        // The resource dash path repeats odd patterns before CPlainPen::SetDashArray.
        var dashes = new float[source.Length * ((source.Length & 1) == 0 ? 1 : 2)];
        for (int i = 0; i < dashes.Length; i++)
        {
            dashes[i] = MathF.Abs((float)source[i % source.Length]);
            if (!float.IsFinite(dashes[i])) return false;
        }
        snapshot = new(figures.ToArray(), width, Math.Max(1, miter), pen.StartLineCap,
            pen.EndLineCap, pen.DashCap, pen.LineJoin, offset, dashes, transform,
            true, geometry switch
            {
                GeneratedPathGeometryResource path => path.FillRule,
                GeneratedGeometryGroupResource group => group.FillRule,
                _ => MilFillMode.Winding
            });
        return true;
    }
}
