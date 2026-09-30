namespace WpfGfxShape.Core;

internal static class SoftwareArcConverter
{
    internal static bool TryConvert(MilPoint2D start, MilPoint2D end, MilSizeD radius, double rotation,
        bool large, bool sweep, out MilPoint2D[] controls, out bool line)
    {
        controls = [];
        line = false;
        float sx = (float)start.X, sy = (float)start.Y, ex = (float)end.X, ey = (float)end.Y;
        float rx = MathF.Abs((float)radius.Width), ry = MathF.Abs((float)radius.Height);
        float angle = (float)rotation;
        if (!float.IsFinite(sx) || !float.IsFinite(sy) || !float.IsFinite(ex) || !float.IsFinite(ey)
            || !float.IsFinite(rx) || !float.IsFinite(ry) || !float.IsFinite(angle)) return false;
        float x = (ex - sx) * 0.5f, y = (ey - sy) * 0.5f;
        float chord = x * x + y * y;
        if (!float.IsFinite(chord)) return false;
        if (chord < 1e-12f) return true;
        if (rx * rx <= chord * 1e-12f || ry * ry <= chord * 1e-12f)
        {
            line = true;
            return true;
        }
        float cos = 1, sin = 0;
        if (MathF.Abs(angle) >= 1e-6f)
        {
            angle = -angle * (MathF.PI / 180);
            cos = MathF.Cos(angle); sin = MathF.Sin(angle);
            (x, y) = (x * cos - y * sin, x * sin + y * cos);
        }
        x /= rx; y /= ry;
        chord = x * x + y * y;
        if (!float.IsFinite(chord) || chord == 0) return false;
        float cx, cy;
        if (chord > 1)
        {
            float scale = MathF.Sqrt(chord);
            rx *= scale; ry *= scale; x /= scale; y /= scale;
            cx = cy = 0;
        }
        else
        {
            float scale = MathF.Sqrt((1 - chord) / chord);
            cx = (large != sweep ? -1 : 1) * scale * y;
            cy = (large != sweep ? 1 : -1) * scale * x;
        }
        float ax = -x - cx, ay = -y - cy, bx = x - cx, by = y - cy;
        float dot = ax * bx + ay * by, cross = ax * by - ay * bx;
        int pieces = dot >= 0 ? (large ? 4 : 1) : (large ? 3 : 2);
        float arcCos = dot, arcSin = cross;
        if (pieces > 1)
        {
            float arc = MathF.Atan2(cross, dot);
            if (sweep && arc < 0) arc += 2 * MathF.PI;
            if (!sweep && arc > 0) arc -= 2 * MathF.PI;
            arcCos = MathF.Cos(arc / pieces); arcSin = MathF.Sin(arc / pieces);
        }
        double half = (1.0 + arcCos) * 0.5;
        double distance = half >= 0 && half < 1 ? (4.0 / 3) * (1 - Math.Sqrt(half)) / Math.Sqrt(1 - half) : 0;
        if (distance <= 1e-6) distance = 0;
        float d = (float)(sweep ? distance : -distance);
        MilPoint2D Map(float px, float py) => new(
            cos * rx * (px + cx) + sin * ry * (py + cy) + (ex + sx) * 0.5f,
            -sin * rx * (px + cx) + cos * ry * (py + cy) + (ey + sy) * 0.5f);
        controls = new MilPoint2D[pieces * 3];
        for (int i = 0; i < pieces; i++)
        {
            float nx = i == pieces - 1 ? bx : ax * arcCos - ay * arcSin;
            float ny = i == pieces - 1 ? by : ax * arcSin + ay * arcCos;
            controls[i * 3] = Map(ax - d * ay, ay + d * ax);
            controls[i * 3 + 1] = Map(nx + d * ny, ny - d * nx);
            controls[i * 3 + 2] = i == pieces - 1 ? end : Map(nx, ny);
            ax = nx; ay = ny;
        }
        return true;
    }
}
