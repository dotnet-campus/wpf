namespace WpfGfxShape.Core;

internal static class SoftwareCurvedStrokeContours
{
    internal static bool TryRead(GeneratedEllipseGeometryResource ellipse, out SoftwareStrokeContours.Figure figure,
        bool preserveCurves = false)
    {
        figure = default;
        if (!GeneratedImageTransform.TryResolve(ellipse.Transform, out var transform)) return false;
        var value = ellipse.CurrentValue;
        float x = (float)value.Center.X, y = (float)value.Center.Y;
        float rx = MathF.Abs((float)value.RadiusX), ry = MathF.Abs((float)value.RadiusY);
        if (!float.IsFinite(x) || !float.IsFinite(y) || !float.IsFinite(rx) || !float.IsFinite(ry)) return false;
        const float k = 0.5522847498307933984f;
        float mx = rx * k, my = ry * k;
        MilPoint2D[] controls = [new(x + rx, y), new(x + rx, y + my), new(x + mx, y + ry), new(x, y + ry),
            new(x - mx, y + ry), new(x - rx, y + my), new(x - rx, y),
            new(x - rx, y - my), new(x - mx, y - ry), new(x, y - ry),
            new(x + mx, y - ry), new(x + rx, y - my), new(x + rx, y)];
        for (int i = 0; i < controls.Length; i++)
        {
            var point = transform.Apply(controls[i].X, controls[i].Y);
            if (!float.IsFinite((float)point.X) || !float.IsFinite((float)point.Y)) return false;
            controls[i] = new((float)point.X, (float)point.Y);
        }
        var points = new List<MilPoint2D> { controls[0] };
        for (int i = 0; i < 12; i += 3)
        {
            if (preserveCurves)
            {
                points.Add(controls[i + 3]);
                continue;
            }
            foreach (var point in SoftwareBezierFlattener.Flatten(new(controls[i].X, controls[i].Y),
                new(controls[i + 1].X, controls[i + 1].Y), new(controls[i + 2].X, controls[i + 2].Y),
                new(controls[i + 3].X, controls[i + 3].Y)))
            {
                var next = new MilPoint2D((float)point.X, (float)point.Y);
                if (points[^1] != next) points.Add(next);
            }
        }
        var segments = new List<SoftwareStrokeContours.Segment>();
        for (int i = 0; i < 12; i += 3)
            segments.Add(new([controls[i + 1], controls[i + 2], controls[i + 3]], true, false));
        figure = new(points.ToArray(), true, SmoothJoins: Enumerable.Repeat(true, points.Count).ToArray(), Segments: segments.ToArray());
        return true;
    }

    internal static bool TryRead(GeneratedRectangleGeometryResource rectangle, out SoftwareStrokeContours.Figure figure,
        bool preserveCurves = false)
    {
        figure = default;
        var value = rectangle.CurrentValue;
        var r = value.Rect;
        if (r.Width < 0 || r.Height < 0) { figure = new([], true); return true; }
        if (!GeneratedImageTransform.TryResolve(rectangle.Transform, out var transform)) return false;
        if (value.RadiusX == 0 && value.RadiusY == 0)
        {
            var corners = new MilPoint2D[] { new(r.X, r.Y), new(r.X + r.Width, r.Y),
                new(r.X + r.Width, r.Y + r.Height), new(r.X, r.Y + r.Height) };
            for (int i = 0; i < corners.Length; i++)
            {
                var point = transform.Apply((float)corners[i].X, (float)corners[i].Y);
                if (!float.IsFinite((float)point.X) || !float.IsFinite((float)point.Y)) return false;
                corners[i] = new((float)point.X, (float)point.Y);
            }
            figure = new(corners, true);
            return true;
        }
        double rx = Math.Min(Math.Abs(value.RadiusX), r.Width / 2);
        double ry = Math.Min(Math.Abs(value.RadiusY), r.Height / 2);
        double l = r.X, t = r.Y, right = l + r.Width, bottom = t + r.Height;
        const double k = 1 - 0.5522847498307933984;
        double bx = rx * k, by = ry * k;
        MilPoint2D[] controls = [new(l, t + ry), new(l, t + by), new(l + bx, t), new(l + rx, t),
            new(right - rx, t), new(right - bx, t), new(right, t + by), new(right, t + ry),
            new(right, bottom - ry), new(right, bottom - by), new(right - bx, bottom), new(right - rx, bottom),
            new(l + rx, bottom), new(l + bx, bottom), new(l, bottom - by), new(l, bottom - ry)];
        for (int i = 0; i < controls.Length; i++)
        {
            var p = transform.Apply((float)controls[i].X, (float)controls[i].Y);
            if (!float.IsFinite((float)p.X) || !float.IsFinite((float)p.Y)) return false;
            controls[i] = new((float)p.X, (float)p.Y);
        }
        var points = new List<MilPoint2D>();
        void Add(MilPoint2D point)
        {
            if (points.Count == 0 || points[^1] != point) points.Add(point);
        }
        Add(controls[0]);
        for (int i = 0; i < controls.Length; i += 4)
        {
            var a = controls[i]; var b = controls[i + 1]; var c = controls[i + 2]; var d = controls[i + 3];
            if (preserveCurves) Add(d);
            else
                foreach (var p in SoftwareBezierFlattener.Flatten(new(a.X, a.Y), new(b.X, b.Y), new(c.X, c.Y), new(d.X, d.Y)))
                    Add(new(p.X, p.Y));
            Add(controls[(i + 4) % controls.Length]);
        }
        var segments = new List<SoftwareStrokeContours.Segment>();
        for (int i = 0; i < controls.Length; i += 4)
        {
            segments.Add(new([controls[i + 1], controls[i + 2], controls[i + 3]], true, false));
            segments.Add(new([controls[(i + 4) % controls.Length]], true, false));
        }
        figure = new(points.ToArray(), true, SmoothJoins: Enumerable.Repeat(true, points.Count).ToArray(), Segments: segments.ToArray());
        return true;
    }
}
