using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class TileMappingExports
{
    [UnmanagedCallersOnly(EntryPoint = "MilUtility_GetTileBrushMapping", CallConvs = [typeof(CallConvStdcall)])]
    private static void GetMapping(Matrix4x4* transform, Matrix4x4* relative, uint stretch,
        uint alignmentX, uint alignmentY, uint viewportUnits, uint viewboxUnits,
        MilRectD* shape, MilRectD* content, MilRectD* viewport, MilRectD* box,
        Matrix4x4* result, int* empty)
    {
        *empty = 0;
        if (viewportUnits == 1) *viewport = Absolute(*shape, *viewport);
        if (viewboxUnits == 1) *box = Absolute(*content, *box);
        if (Empty(*viewport) || Empty(*box))
        {
            *empty = 1;
            return;
        }
        double sx = 1, sy = 1;
        if (stretch != 0)
        {
            sx = viewport->Width / box->Width;
            sy = viewport->Height / box->Height;
            if (stretch == 2) sx = sy = sx < sy ? sx : sy;
            if (stretch == 3) sx = sy = sx > sy ? sx : sy;
        }
        (double tx, double ax) = Align(box->X, box->Width, viewport->X, viewport->Width, alignmentX);
        (double ty, double ay) = Align(box->Y, box->Height, viewport->Y, viewport->Height, alignmentY);
        Matrix4x4 mapping = new((float)sx, 0, 0, 0, 0, (float)sy, 0, 0,
            0, 0, 1, 0, (float)(tx * sx + ax), (float)(ty * sy + ay), 0, 1);
        mapping = Multiply(Matrix4x4.Identity, mapping);
        Matrix4x4 brush = Matrix4x4.Identity;
        if (relative != null && shape->Width != 0 && shape->Height != 0)
        {
            float x = (float)shape->X, y = (float)shape->Y;
            float w = (float)shape->Width, h = (float)shape->Height;
            float hw = h / w, wh = w / h;
            brush.M11 = relative->M11;
            brush.M12 = relative->M12 * hw;
            brush.M21 = relative->M21 * wh;
            brush.M22 = relative->M22;
            brush.M41 = relative->M41 * w - relative->M11 * x - relative->M21 * y * wh + x;
            brush.M42 = relative->M42 * h - relative->M12 * x * hw - relative->M22 * y + y;
        }
        if (transform != null) brush = Multiply(brush, *transform);
        *result = Multiply(mapping, brush);
    }

    private static MilRectD Absolute(MilRectD bounds, MilRectD value) => Empty(bounds) || Empty(value)
        ? new(0, 0, -1, -1)
        : new(bounds.X + value.X * bounds.Width, bounds.Y + value.Y * bounds.Height,
            value.Width * bounds.Width, value.Height * bounds.Height);

    private static bool Empty(MilRectD value) => !(value.Width >= 0) || !(value.Height >= 0)
        || double.IsNaN(value.X) || double.IsNaN(value.Y);

    private static (double Translation, double Alignment) Align(double start, double size,
        double target, double targetSize, uint alignment) => alignment switch
        {
            0 => (-start, target),
            1 => (-(start + size / 2), target + targetSize / 2),
            2 => (-(start + size), target + targetSize),
            _ => (0, 0)
        };

    // Scalar evaluation preserves native float rounding instead of SIMD fused operations.
    private static Matrix4x4 Multiply(Matrix4x4 left, Matrix4x4 right)
    {
        Matrix4x4 result = default;
        float* a = (float*)&left, b = (float*)&right, c = (float*)&result;
        for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
                c[row * 4 + column] = (a[row * 4] * b[column] + a[row * 4 + 2] * b[8 + column])
                    + (a[row * 4 + 1] * b[4 + column] + a[row * 4 + 3] * b[12 + column]);
        return result;
    }
}
