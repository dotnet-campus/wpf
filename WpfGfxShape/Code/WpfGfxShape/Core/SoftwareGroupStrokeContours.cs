namespace WpfGfxShape.Core;

internal static class SoftwareGroupStrokeContours
{
    internal static bool TryRead(GeneratedProtocolResource geometry, out List<SoftwareStrokeContours.Figure> figures,
        bool preserveGaps = false, GeneratedImageTransform? deviceTransform = null, bool fillPrimitive = false)
    {
        figures = [];
        return Append(geometry, GeneratedImageTransform.Identity, figures,
            new HashSet<GeneratedProtocolResource>(), preserveGaps, deviceTransform, fillPrimitive);
    }

    private static void Flatten(List<SoftwareStrokeContours.Figure> figures, GeneratedImageTransform? deviceTransform)
    {
        for (int i = 0; i < figures.Count; i++)
        {
            var figure = figures[i];
            if (figure.Segments is not { Length: > 0 } segments || figure.Points.Length == 0) continue;
            var points = new List<MilPoint2D> { figure.Points[0] };
            foreach (var segment in segments)
            {
                var values = segment.Points;
                if (values.Length == 1) points.Add(values[0]);
                else
                {
                    var start = points[^1];
                    foreach (var end in SoftwareBezierFlattener.Flatten(new(start.X, start.Y),
                        new(values[0].X, values[0].Y), new(values[1].X, values[1].Y),
                        new(values[2].X, values[2].Y), deviceTransform))
                        points.Add(new(end.X, end.Y));
                }
            }
            figures[i] = figure with { Points = points.ToArray(), Segments = null };
        }
    }

    private static MilFillMode FillRule(GeneratedProtocolResource? geometry) => geometry switch
    {
        GeneratedPathGeometryResource path => path.FillRule,
        GeneratedGeometryGroupResource group => group.FillRule,
        _ => MilFillMode.Winding
    };

    private static bool Append(GeneratedProtocolResource geometry, GeneratedImageTransform parent,
        List<SoftwareStrokeContours.Figure> output, HashSet<GeneratedProtocolResource> active,
        bool preserveGaps, GeneratedImageTransform? deviceTransform, bool fillPrimitive)
    {
        if (!active.Add(geometry) || active.Count > 256) return false;
        try
        {
            if (geometry is GeneratedGeometryGroupResource group)
            {
                if (!GeneratedImageTransform.TryResolve(group.Transform, out var local)) return false;
                var transform = parent.Prepend(local);
                foreach (var child in group.Children)
                    if (!Append(child, transform, output, active, preserveGaps, deviceTransform, fillPrimitive)) return false;
                return true;
            }
            if (geometry is GeneratedCombinedGeometryResource combined)
            {
                if (!GeneratedImageTransform.TryResolve(combined.Transform, out var local)) return false;
                var transform = parent.Prepend(local);
                var first = new List<SoftwareStrokeContours.Figure>();
                var second = new List<SoftwareStrokeContours.Figure>();
                if (combined.Geometry1 is { } a && !Append(a, transform, first, active, true, deviceTransform, fillPrimitive)) return false;
                if (combined.Geometry2 is { } b && !Append(b, transform, second, active, true, deviceTransform, fillPrimitive)) return false;
                Flatten(first, deviceTransform);
                Flatten(second, deviceTransform);
                if (!SoftwareBooleanContours.TryCombine(first, FillRule(combined.Geometry1), second,
                    FillRule(combined.Geometry2), combined.CombineMode, out var result)) return false;
                output.AddRange(result);
                return true;
            }
            List<SoftwareStrokeContours.Figure> figures;
            if (geometry is GeneratedPathGeometryResource path)
            {
                if (!SoftwareStrokeContours.TryRead(path, out figures, preserveGaps, fillLocalFloat: fillPrimitive)) return false;
            }
            else if (geometry is GeneratedLineGeometryResource line)
            {
                if (!GeneratedImageTransform.TryResolve(line.Transform, out var transform)) return false;
                var value = line.CurrentValue;
                var a = transform.Apply((float)value.StartPoint.X, (float)value.StartPoint.Y);
                var b = transform.Apply((float)value.EndPoint.X, (float)value.EndPoint.Y);
                figures = [new([new((float)a.X, (float)a.Y), new((float)b.X, (float)b.Y)], false, Fillable: false)];
            }
            else if (geometry is GeneratedRectangleGeometryResource rectangle)
            {
                if (!SoftwareCurvedStrokeContours.TryRead(rectangle, out var figure, preserveCurves: preserveGaps, fillPrimitive: fillPrimitive)) return false;
                figures = [figure];
            }
            else if (geometry is GeneratedEllipseGeometryResource ellipse)
            {
                if (!SoftwareCurvedStrokeContours.TryRead(ellipse, out var figure, preserveCurves: preserveGaps, fillPrimitive: fillPrimitive)) return false;
                figures = [figure];
            }
            else return false;
            foreach (var figure in figures)
            {
                var points = new MilPoint2D[figure.Points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    var point = parent.Apply(figure.Points[i].X, figure.Points[i].Y);
                    if (!float.IsFinite((float)point.X) || !float.IsFinite((float)point.Y)) return false;
                    points[i] = new((float)point.X, (float)point.Y);
                }
                SoftwareStrokeContours.Segment[]? segments = null;
                if (figure.Segments is { } source)
                {
                    segments = new SoftwareStrokeContours.Segment[source.Length];
                    for (int i = 0; i < source.Length; i++)
                    {
                        var controls = new MilPoint2D[source[i].Points.Length];
                        for (int j = 0; j < controls.Length; j++)
                        {
                            var p = parent.Apply(source[i].Points[j].X, source[i].Points[j].Y);
                            if (!float.IsFinite((float)p.X) || !float.IsFinite((float)p.Y)) return false;
                            controls[j] = new((float)p.X, (float)p.Y);
                        }
                        segments[i] = source[i] with { Points = controls };
                    }
                }
                output.Add(figure with { Points = points, Segments = segments });
            }
            return true;
        }
        finally { active.Remove(geometry); }
    }
}
