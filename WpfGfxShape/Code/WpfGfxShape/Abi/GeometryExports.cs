using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;
using static WpfGfxShape.Abi.GeometryUtility;

namespace WpfGfxShape.Abi;

internal static unsafe class GeometryExports
{
    [UnmanagedCallersOnly(EntryPoint = "MilUtility_ArcToBezier", CallConvs = [typeof(CallConvStdcall)])]
    private static void Arc(MilPoint2D start, MilPoint2D radii, double rotation, byte large, byte sweep,
        MilPoint2D end, Matrix* matrix, MilPoint2D* output, int* pieces)
    {
        if (output == null || pieces == null) return;
        *pieces = -1;
        try
        {
            if (!SoftwareArcConverter.TryConvert(start, end, new(radii.X, radii.Y), rotation, large != 0, sweep != 0, out var controls, out bool line)) return;
            *pieces = line ? 0 : controls.Length == 0 ? -1 : controls.Length / 3;
            if (line) controls = [end];
            var transform = Transform(matrix);
            for (int i = 0; i < controls.Length; i++)
            {
                var p = transform.Apply((float)controls[i].X, (float)controls[i].Y);
                output[i] = new((float)p.X, (float)p.Y);
            }
        }
        catch (OutOfMemoryException) { *pieces = -1; }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_PathGeometryFlatten", CallConvs = [typeof(CallConvStdcall)])]
    private static int Flatten(Matrix* matrix, MilFillMode rule, byte* data, uint size, double tolerance, byte relative, nint callback, MilFillMode* output)
        => Shape(matrix, rule, data, size, tolerance, relative, callback, output, false, null, null);

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_PathGeometryOutline", CallConvs = [typeof(CallConvStdcall)])]
    private static int Outline(Matrix* matrix, MilFillMode rule, byte* data, uint size, double tolerance, byte relative, nint callback, MilFillMode* output)
        => Shape(matrix, rule, data, size, tolerance, relative, callback, output, true, null, null);

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_PathGeometryWiden", CallConvs = [typeof(CallConvStdcall)])]
    private static int WidenPath(Pen* pen, double* dashes, Matrix* matrix, MilFillMode rule, byte* data, uint size, double tolerance, byte relative, nint callback, MilFillMode* output)
        => pen == null ? Invalid : Shape(matrix, rule, data, size, tolerance, relative, callback, output, false, pen, dashes);

    private static int Shape(Matrix* matrix, MilFillMode rule, byte* data, uint size, double tolerance, byte relative, nint callback, MilFillMode* output, bool outline, Pen* pen, double* dashes)
    {
        if (callback == 0 || output == null) return unchecked((int)0x80070006);
        try
        {
            int hr = Read(data, size, rule, out var path);
            if (hr < 0) return hr;
            hr = Figures(path, matrix, tolerance, relative, out var figures, pen != null);
            if (hr < 0) return hr;
            if (pen != null)
            {
                hr = Widen(figures, pen, dashes, out figures);
                if (hr < 0) return hr;
                hr = Normalize(figures, MilFillMode.Winding, out figures);
            }
            else if (outline) hr = Normalize(figures, rule, out figures);
            if (hr < 0) return hr;
            // geometry_api.cpp deliberately leaves the widening output reference untouched.
            if (pen == null) *output = rule;
            Emit(figures, callback);
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (OverflowException) { return Invalid; }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_PathGeometryCombine", CallConvs = [typeof(CallConvStdcall)])]
    private static int Combine(Matrix* resultMatrix, Matrix* firstMatrix, MilFillMode firstRule, byte* firstData, uint firstSize,
        Matrix* secondMatrix, MilFillMode secondRule, byte* secondData, uint secondSize, double tolerance, byte relative, nint callback, MilCombineMode mode, MilFillMode* output)
    {
        if (callback == 0 || output == null) return unchecked((int)0x80070006);
        if (resultMatrix == null || firstMatrix == null || secondMatrix == null || mode > MilCombineMode.Exclude) return Invalid;
        try
        {
            int hr = Read(firstData, firstSize, firstRule, out var first);
            if (hr < 0) return hr;
            hr = Read(secondData, secondSize, secondRule, out var second);
            if (hr < 0) return hr;
            hr = Figures(first, firstMatrix, tolerance, relative, out var a);
            if (hr < 0) return hr;
            hr = Figures(second, secondMatrix, tolerance, relative, out var b);
            if (hr < 0) return hr;
            if (!SoftwareBooleanContours.TryCombine(a, firstRule, b, secondRule, mode, out var result)) return BadNumber;
            var transform = Transform(resultMatrix);
            if (!Finite(transform)) return BadNumber;
            for (int i = 0; i < result.Count; i++)
            {
                var points = result[i].Points.Select(p => { var q = transform.Apply(p.X, p.Y); return new MilPoint2D((float)q.X, (float)q.Y); }).ToArray();
                if (points.Any(p => !float.IsFinite((float)p.X) || !float.IsFinite((float)p.Y))) return BadNumber;
                result[i] = result[i] with { Points = points };
            }
            *output = MilFillMode.Winding;
            Emit(result, callback);
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (OverflowException) { return Invalid; }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_GeometryGetArea", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetArea(MilFillMode rule, byte* data, uint size, Matrix* matrix, double tolerance, byte relative, double* area)
    {
        if (area == null) return Invalid;
        try
        {
            int hr = Read(data, size, rule, out var path);
            if (hr < 0) return hr;
            hr = Figures(path, matrix, tolerance, relative, out var figures);
            if (hr < 0) return hr;
            var points = figures.SelectMany(f => f.Points).ToArray();
            // The native scanner leaves the output reference untouched for a degenerate workspace.
            if (points.Length == 0 || Math.Max(points.Max(p => p.X) - points.Min(p => p.X),
                points.Max(p => p.Y) - points.Min(p => p.Y)) < 1.1754943508222875e-38) return 0;
            hr = Normalize(figures, rule, out figures);
            if (hr < 0) return hr;
            *area = Area(figures);
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (OverflowException) { return Invalid; }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_PathGeometryBounds", CallConvs = [typeof(CallConvStdcall)])]
    private static int PathBounds(Pen* pen, double* dashes, Matrix* world, MilFillMode rule, byte* data, uint size,
        Matrix* geometry, double tolerance, byte relative, byte skipHollows, MilRectD* output)
        => Bounds(pen, dashes, world, rule, data, size, null, null, 0, 0, geometry, tolerance, relative, skipHollows, output, false);

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_PolygonBounds", CallConvs = [typeof(CallConvStdcall)])]
    private static int PolygonBounds(Matrix* world, Pen* pen, double* dashes, MilPoint2D* points, byte* types, uint pointCount,
        uint segmentCount, Matrix* geometry, double tolerance, byte relative, byte skipHollows, MilRectD* output)
        => Bounds(pen, dashes, world, MilFillMode.Alternate, null, 0, points, types, pointCount, segmentCount, geometry, tolerance, relative, skipHollows, output, true);

    private static int Bounds(Pen* pen, double* dashes, Matrix* world, MilFillMode rule, byte* data, uint size,
        MilPoint2D* points, byte* types, uint pointCount, uint segmentCount, Matrix* geometry,
        double tolerance, byte relative, byte skipHollows, MilRectD* output, bool polygon)
    {
        if (output == null) return Invalid;
        try
        {
            GeneratedPathGeometryResource path;
            int hr = polygon ? ReadPolygon(points, types, pointCount, segmentCount, out path) : Read(data, size, rule, out path);
            if (hr < 0) return hr;
            var transform = Transform(world).Prepend(Transform(geometry));
            if (!Finite(transform)) return BadNumber;
            var bytes = path.Figures;
            if (skipHollows == 0)
                for (int offset = 48; offset < bytes.Length; offset += (int)MemoryMarshal.Read<uint>(bytes.AsSpan(offset + 12)))
                { uint flags = MemoryMarshal.Read<uint>(bytes.AsSpan(offset + 4)) | 8; MemoryMarshal.Write(bytes.AsSpan(offset + 4), in flags); }
            if (!GeneratedGeometryBounds.TryGet(path, transform, out var bounds)) return BadNumber;
            bool empty = true;
            for (int offset = 48; offset < bytes.Length; offset += (int)MemoryMarshal.Read<uint>(bytes.AsSpan(offset + 12)))
                if ((MemoryMarshal.Read<uint>(bytes.AsSpan(offset + 4)) & 8) != 0) empty = false;
            if (pen != null)
            {
                hr = Figures(path, geometry, tolerance, relative, out var figures, true);
                if (hr < 0) return hr;
                hr = Widen(figures, pen, dashes, out var widened);
                if (hr < 0) return hr;
                var w = Transform(world);
                var values = widened.SelectMany(f => f.Points).Select(p => w.Apply(p.X, p.Y)).ToArray();
                if (values.Length > 0)
                {
                    double l = values.Min(p => p.X), t = values.Min(p => p.Y), r = values.Max(p => p.X), b = values.Max(p => p.Y);
                    if (!empty) { l = Math.Min(l, bounds.X); t = Math.Min(t, bounds.Y); r = Math.Max(r, bounds.X + bounds.Width); b = Math.Max(b, bounds.Y + bounds.Height); }
                    bounds = new(l, t, r - l, b - t); empty = false;
                }
            }
            // PathGeometryBounds uses MilRectD edges; PolygonBounds uses point-and-size.
            *output = empty ? new(double.PositiveInfinity, double.PositiveInfinity, polygon ? double.NegativeInfinity : double.PositiveInfinity,
                polygon ? double.NegativeInfinity : double.PositiveInfinity)
                : new((float)bounds.X, (float)bounds.Y, polygon ? (float)(bounds.X + bounds.Width) - (float)bounds.X : (float)(bounds.X + bounds.Width),
                    polygon ? (float)(bounds.Y + bounds.Height) - (float)bounds.Y : (float)(bounds.Y + bounds.Height));
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (OverflowException) { return Invalid; }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_PathGeometryHitTest", CallConvs = [typeof(CallConvStdcall)])]
    private static int PathHit(Matrix* matrix, Pen* pen, double* dashes, MilFillMode rule, byte* data, uint size,
        double threshold, byte relative, MilPoint2D* point, int* hit)
        => HitTest(matrix, pen, dashes, rule, data, size, null, null, 0, 0, threshold, relative, point, hit, false);

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_PolygonHitTest", CallConvs = [typeof(CallConvStdcall)])]
    private static int PolygonHit(Matrix* matrix, Pen* pen, double* dashes, MilPoint2D* points, byte* types, uint pointCount,
        uint segmentCount, double threshold, byte relative, MilPoint2D* point, int* hit)
        => HitTest(matrix, pen, dashes, MilFillMode.Alternate, null, 0, points, types, pointCount, segmentCount, threshold, relative, point, hit, true);

    private static int HitTest(Matrix* matrix, Pen* pen, double* dashes, MilFillMode rule, byte* data, uint size,
        MilPoint2D* points, byte* types, uint pointCount, uint segmentCount, double threshold, byte relative, MilPoint2D* point, int* hit, bool polygon)
    {
        if (point == null || hit == null) return Invalid;
        try
        {
            GeneratedPathGeometryResource path;
            int hr = polygon ? ReadPolygon(points, types, pointCount, segmentCount, out path) : Read(data, size, rule, out path);
            if (hr < 0) return hr;
            hr = Figures(path, matrix, threshold, relative, out var figures, pen != null);
            if (hr < 0) return hr;
            var p = new MilPoint2D((float)point->X, (float)point->Y);
            if (!double.IsFinite(p.X) || !double.IsFinite(p.Y)) return BadNumber;
            if (relative != 0)
            {
                var all = figures.SelectMany(f => f.Points).ToArray();
                threshold *= all.Length == 0 ? 0 : Math.Max(all.Max(v => v.X) - all.Min(v => v.X), all.Max(v => v.Y) - all.Min(v => v.Y));
            }
            if (pen != null)
            {
                hr = Widen(figures, pen, dashes, out figures);
                if (hr < 0) return hr;
                rule = MilFillMode.Winding;
            }
            *hit = Hit(figures, rule, p, Math.Max(0, threshold)) ? 1 : 0;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (OverflowException) { return Invalid; }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_PathGeometryHitTestPathGeometry", CallConvs = [typeof(CallConvStdcall)])]
    private static int Relation(Matrix* firstMatrix, MilFillMode firstRule, byte* firstData, uint firstSize,
        Matrix* secondMatrix, MilFillMode secondRule, byte* secondData, uint secondSize, double tolerance, byte relative, uint* relation)
    {
        if (relation == null) return Invalid;
        try
        {
            int hr = Read(firstData, firstSize, firstRule, out var first);
            if (hr < 0) return hr;
            hr = Read(secondData, secondSize, secondRule, out var second);
            if (hr < 0) return hr;
            hr = Figures(first, firstMatrix, tolerance, relative, out var a);
            if (hr < 0) return hr;
            hr = Figures(second, secondMatrix, tolerance, relative, out var b);
            if (hr < 0) return hr;
            if (!SoftwareBooleanContours.TryCombine(a, firstRule, b, secondRule, MilCombineMode.Intersect, out var intersection)
                || !SoftwareBooleanContours.TryCombine(a, firstRule, b, secondRule, MilCombineMode.Exclude, out var aOnly)
                || !SoftwareBooleanContours.TryCombine(b, secondRule, a, firstRule, MilCombineMode.Exclude, out var bOnly)) return BadNumber;
            *relation = intersection.Count == 0 ? 1u : aOnly.Count == 0 ? 2u : bOnly.Count == 0 ? 3u : 4u;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (OverflowException) { return Invalid; }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilUtility_GetPointAtLengthFraction", CallConvs = [typeof(CallConvStdcall)])]
    private static int PointAtLength(Matrix* matrix, MilFillMode rule, byte* data, uint size, double fraction, MilPoint2D* point, MilPoint2D* tangent)
    {
        if (point == null || tangent == null) return Invalid;
        try
        {
            int hr = Read(data, size, rule, out var path);
            if (hr < 0) return hr;
            if (!SoftwareAnimationPath.TryGetPoint(path, Transform(matrix), fraction, out var position, out var direction)) return Invalid;
            *point = position;
            *tangent = direction;
            return 0;
        }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (OverflowException) { return Invalid; }
    }
}
