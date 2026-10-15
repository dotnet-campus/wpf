namespace WpfGfxShape.Core;

internal readonly record struct GeneratedImageTransform(double ScaleX, double ScaleY, double X, double Y, double M12 = 0, double M21 = 0)
{
    internal static GeneratedImageTransform Identity => new(1, 1, 0, 0);
    internal bool IsPositiveAxisAligned => M12 == 0 && M21 == 0 && ScaleX > 0 && ScaleY > 0;
    internal double XLength => Math.Sqrt(ScaleX * ScaleX + M12 * M12);
    internal double YLength => Math.Sqrt(M21 * M21 + ScaleY * ScaleY);

    internal GeneratedImageTransform Prepend(GeneratedImageTransform local) => new(
        local.ScaleX * ScaleX + local.M12 * M21,
        local.M21 * M12 + local.ScaleY * ScaleY,
        local.X * ScaleX + local.Y * M21 + X,
        local.X * M12 + local.Y * ScaleY + Y,
        local.ScaleX * M12 + local.M12 * ScaleY,
        local.M21 * ScaleX + local.ScaleY * M21);

    internal bool TryDecompose(out GeneratedImageTransform scale, out GeneratedImageTransform rest)
    {
        float m11 = (float)ScaleX, m12 = (float)M12;
        float m21 = (float)M21, m22 = (float)ScaleY;
        float x = MathF.Sqrt(m11 * m11 + m12 * m12);
        float y = MathF.Sqrt(m21 * m21 + m22 * m22);
        if (float.IsNaN(x)) x = 0;
        if (float.IsNaN(y)) y = 0;
        scale = new(x, y, 0, 0);
        rest = default;
        if (!(x > 0) || !(y > 0) || !float.IsFinite(x) || !float.IsFinite(y)) return false;
        float inverseX = 1f / x, inverseY = 1f / y;
        rest = new(m11 * inverseX, m22 * inverseY, (float)X, (float)Y,
            m12 * inverseX, m21 * inverseY);
        return rest.IsFinite;
    }

    internal (double X, double Y) Apply(double x, double y) =>
        (x * ScaleX + y * M21 + X, x * M12 + y * ScaleY + Y);

    internal MilRectD Apply(MilRectD rectangle)
    {
        var a = Apply(rectangle.X, rectangle.Y);
        var b = Apply(rectangle.X + rectangle.Width, rectangle.Y);
        var c = Apply(rectangle.X, rectangle.Y + rectangle.Height);
        var d = Apply(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
        double left = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
        double top = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));
        return new(left, top,
            Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X)) - left,
            Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y)) - top);
    }

    internal bool TryInvert(out GeneratedImageTransform inverse)
    {
        double determinant = ScaleX * ScaleY - M12 * M21;
        inverse = default;
        if (!double.IsFinite(determinant) || determinant == 0) return false;
        inverse = new(ScaleY / determinant, ScaleX / determinant,
            (Y * M21 - X * ScaleY) / determinant, (X * M12 - Y * ScaleX) / determinant,
            -M12 / determinant, -M21 / determinant);
        return inverse.IsFinite;
    }

    private bool IsFinite => double.IsFinite(ScaleX) && double.IsFinite(ScaleY)
        && double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(M12) && double.IsFinite(M21);

    internal static double ReadAnimated(IReadOnlyList<GeneratedProtocolResource?> slots, int index, double fallback)
        => index < slots.Count && slots[index] is GeneratedValueResource<double> animated ? animated.Value : fallback;

    internal static bool TryResolve(GeneratedProtocolResource? resource, out GeneratedImageTransform value, int depth = 0)
    {
        value = Identity;
        if (depth > 256) return false;
        switch (resource)
        {
            case null:
                return true;
            case GeneratedTranslateTransformResource translate:
                value = new(1, 1, translate.CurrentValue.X, translate.CurrentValue.Y);
                break;
            case GeneratedScaleTransformResource scale:
                var s = scale.CurrentValue;
                value = new(s.First, s.Second, s.Third * (1 - s.First), s.Fourth * (1 - s.Second));
                break;
            case GeneratedRotateTransformResource rotate:
                var r = rotate.CurrentValue;
                double angle = r.Angle * Math.PI / 180;
                double sin = Math.Sin(angle), cos = Math.Cos(angle);
                value = new(cos, cos, r.CenterX * (1 - cos) + r.CenterY * sin,
                    r.CenterY * (1 - cos) - r.CenterX * sin, sin, -sin);
                break;
            case GeneratedSkewTransformResource skew:
                var k = skew.CurrentValue;
                double tx = Math.Tan(k.First * Math.PI / 180), ty = Math.Tan(k.Second * Math.PI / 180);
                value = new(1, 1, -k.Fourth * tx, -k.Third * ty, ty, tx);
                break;
            case GeneratedMatrixTransformResource matrix:
                var m = matrix.Animation is GeneratedValueResource<MilMatrix3x2D> animatedMatrix ? animatedMatrix.Value : matrix.Value;
                value = new(m.M11, m.M22, m.OffsetX, m.OffsetY, m.M12, m.M21);
                break;
            case GeneratedTransformGroupResource group:
                foreach (GeneratedProtocolResource child in group.Children)
                {
                    if (!TryResolve(child, out GeneratedImageTransform next, depth + 1)) return false;
                    value = next.Prepend(value);
                }
                break;
            default:
                return false;
        }
        return value.IsFinite;
    }
}
