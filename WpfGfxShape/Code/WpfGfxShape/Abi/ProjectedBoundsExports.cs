using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Abi;

internal static unsafe class ProjectedBoundsExports
{
    [UnmanagedCallersOnly(EntryPoint = "MIL3DCalcProjected2DBounds", CallConvs = [typeof(CallConvStdcall)])]
    private static int Calculate(Matrix4x4* matrix, float* box, float* rect)
    {
        if (matrix == null || box == null || rect == null) return unchecked((int)0x80070057);
        rect[0] = rect[1] = -float.MaxValue;
        rect[2] = rect[3] = float.MaxValue;
        Span<Vector4> vertices = stackalloc Vector4[8];
        for (int i = 0; i < 8; i++)
        {
            vertices[i] = Vector4.Transform(new Vector4(box[0] + ((i & 1) != 0 ? box[3] : 0),
                box[1] + ((i & 2) != 0 ? box[4] : 0), box[2] + ((i & 4) != 0 ? box[5] : 0), 1), *matrix);
            for (int j = 0; j < 4; j++)
                if (!(vertices[i][j] > -float.MaxValue && vertices[i][j] < float.MaxValue)) return 0;
        }
        Vector2 min = default, max = default;
        bool first = true;
        for (int i = 0; i < 8; i++)
            for (int bit = 1; bit <= 4; bit <<= 1)
            {
                if ((i & bit) != 0) continue;
                Vector4 a = vertices[i], b = vertices[i | bit];
                double enter = 0, exit = 1;
                bool visible = true;
                for (int plane = 0; plane < 2; plane++)
                {
                    double ca = plane == 0 ? 0.0001 * a.W + a.Z : 1.0001 * a.W - a.Z;
                    double cb = plane == 0 ? 0.0001 * b.W + b.Z : 1.0001 * b.W - b.Z;
                    if (ca < 0 && cb < 0) { visible = false; break; }
                    if (ca < 0) enter = Math.Max(enter, ca / (ca - cb));
                    else if (cb < 0) exit = Math.Min(exit, ca / (ca - cb));
                }
                if (!visible || exit < enter) continue;
                Bound(Interpolate(a, b, enter), ref min, ref max, ref first);
                Bound(Interpolate(a, b, exit), ref min, ref max, ref first);
            }
        rect[0] = min.X; rect[1] = min.Y; rect[2] = max.X; rect[3] = max.Y;
        return 0;
    }

    private static Vector4 Interpolate(Vector4 a, Vector4 b, double t) => new(
        (float)((1 - t) * a.X + t * b.X), (float)((1 - t) * a.Y + t * b.Y),
        (float)((1 - t) * a.Z + t * b.Z), (float)((1 - t) * a.W + t * b.W));

    private static void Bound(Vector4 point, ref Vector2 min, ref Vector2 max, ref bool first)
    {
        if (point.W == 0) { min = new(-float.MaxValue / 2); max = new(float.MaxValue / 2); first = false; return; }
        Vector2 projected = new(point.X / point.W, point.Y / point.W);
        if (first) { min = max = projected; first = false; return; }
        if (projected.X < min.X) min.X = projected.X;
        else if (projected.X > max.X) max.X = projected.X;
        if (projected.Y < min.Y) min.Y = projected.Y;
        else if (projected.Y > max.Y) max.Y = projected.Y;
    }
}
