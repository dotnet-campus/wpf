using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class GeometryUtility
{
    internal const int Invalid = unchecked((int)0x80070057);
    internal const int BadNumber = unchecked((int)0x8898000A);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Matrix
    {
        internal double M11, M12, M21, M22, X, Y;
        internal readonly GeneratedImageTransform Value => new((float)M11, (float)M22, (float)X, (float)Y, (float)M12, (float)M21);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Pen
    {
        internal double Thickness, Miter, Offset;
        internal MilPenCap Start, End, Dash;
        internal MilPenJoin Join;
        internal uint DashBytes;
    }

    internal static GeneratedImageTransform Transform(Matrix* matrix) => matrix == null ? GeneratedImageTransform.Identity : matrix->Value;

    internal static bool Finite(GeneratedImageTransform value) => double.IsFinite(value.ScaleX) && double.IsFinite(value.ScaleY)
        && double.IsFinite(value.M12) && double.IsFinite(value.M21) && double.IsFinite(value.X) && double.IsFinite(value.Y);

    internal static int Read(byte* data, uint size, MilFillMode rule, out GeneratedPathGeometryResource path)
    {
        path = new();
        if (data == null || size > int.MaxValue || size < 48 || rule > MilFillMode.Winding) return Invalid;
        var packet = new byte[checked((int)size + 12)];
        MemoryMarshal.Write(packet.AsSpan(4), in rule);
        MemoryMarshal.Write(packet.AsSpan(8), in size);
        new ReadOnlySpan<byte>(data, (int)size).CopyTo(packet.AsSpan(12));
        return path.ProcessUpdate(new GeneratedProtocolHandleTable(), packet) < 0 ? Invalid : 0;
    }

    internal static int ReadPolygon(MilPoint2D* points, byte* types, uint pointCount, uint segmentCount,
        out GeneratedPathGeometryResource path)
    {
        path = new();
        if (points == null || types == null || pointCount > int.MaxValue || segmentCount > int.MaxValue || pointCount == 0 || segmentCount == 0) return Invalid;
        int consumed = 1;
        var records = new List<byte[]>();
        int previous = 0;
        for (int i = 0; i < segmentCount; i++)
        {
            int arity = (types[i] & 3) switch { 1 => 1, 2 => 3, _ => 0 };
            if (arity == 0 || arity > pointCount - consumed) return Invalid;
            var record = new byte[16 + arity * 16];
            uint type = arity == 1 ? 1u : 2u, flags = (uint)(types[i] & 12), back = (uint)previous;
            MemoryMarshal.Write(record, in type); MemoryMarshal.Write(record.AsSpan(4), in flags);
            MemoryMarshal.Write(record.AsSpan(8), in back);
            new ReadOnlySpan<byte>(points + consumed, arity * 16).CopyTo(record.AsSpan(16));
            records.Add(record); previous = record.Length; consumed += arity;
        }
        if (consumed != pointCount) return Invalid;
        int size = checked(88 + records.Sum(r => r.Length));
        var packet = new byte[size];
        uint bytes = (uint)size, one = 1, figureBytes = (uint)(size - 48), figureFlags = 8u | ((types[0] & 16) != 0 ? 4u : 0u);
        MemoryMarshal.Write(packet, in bytes); MemoryMarshal.Write(packet.AsSpan(40), in one);
        MemoryMarshal.Write(packet.AsSpan(52), in figureFlags); MemoryMarshal.Write(packet.AsSpan(56), in segmentCount);
        MemoryMarshal.Write(packet.AsSpan(60), in figureBytes); MemoryMarshal.Write(packet.AsSpan(64), in points[0]);
        uint last = (uint)(size - 48 - previous);
        MemoryMarshal.Write(packet.AsSpan(80), in last);
        int offset = 88;
        foreach (var record in records) { record.CopyTo(packet, offset); offset += record.Length; }
        fixed (byte* p = packet) return Read(p, bytes, MilFillMode.Alternate, out path);
    }

    internal static int Figures(GeneratedPathGeometryResource path, Matrix* matrix, double tolerance, byte relative,
        out List<SoftwareStrokeContours.Figure> figures, bool stroke = false)
    {
        figures = [];
        if (!SoftwareStrokeContours.TryRead(path, out var source, preserveGaps: true)) return BadNumber;
        var transform = Transform(matrix);
        if (!Finite(transform)) return BadNumber;
        var all = source.SelectMany(f => f.Segments is { } segments
            ? segments.SelectMany(s => s.Points).Concat(f.Points.Take(1)) : f.Points).ToArray();
        double extent = all.Length == 0 ? 0 : Math.Max(all.Max(p => p.X) - all.Min(p => p.X), all.Max(p => p.Y) - all.Min(p => p.Y));
        double absolute = relative != 0 ? Math.Max(tolerance, 1e-12) * extent : Math.Max(tolerance, extent * 1e-12);
        if (double.IsNaN(absolute)) absolute = extent * 1e-12;
        absolute = Math.Max(absolute, 1e-12);
        var errorTransform = new GeneratedImageTransform(0.25 / absolute, 0.25 / absolute, 0, 0);
        foreach (var figure in source)
        {
            if (figure.Points.Length == 0) { figures.Add(figure); continue; }
            MilPoint2D Map(MilPoint2D p)
            {
                var q = transform.Apply((float)p.X, (float)p.Y);
                return new((float)q.X, (float)q.Y);
            }
            var points = new List<MilPoint2D> { Map(figure.Points[0]) };
            var smooth = new List<bool> { false };
            var runs = new List<SoftwareStrokeContours.Figure>();
            bool startsAtGap = false, hadGap = false;
            foreach (var segment in figure.Segments ?? [])
            {
                bool gap = stroke && segment.Gap;
                if (gap)
                {
                    hadGap = true;
                    if (points.Count > 1) runs.Add(new(points.ToArray(), false, startsAtGap, true, smooth.ToArray()));
                    points = [Map(segment.Points[^1])]; smooth = [false]; startsAtGap = true;
                    continue;
                }
                if (segment.Points.Length == 1) points.Add(Map(segment.Points[0]));
                else
                {
                    var a = points[^1]; var b = Map(segment.Points[0]); var c = Map(segment.Points[1]); var d = Map(segment.Points[2]);
                    foreach (var p in SoftwareBezierFlattener.Flatten(new(a.X, a.Y), new(b.X, b.Y), new(c.X, c.Y), new(d.X, d.Y), errorTransform))
                        points.Add(new((float)p.X, (float)p.Y));
                }
                while (smooth.Count < points.Count) smooth.Add(segment.Points.Length != 1);
                smooth[^1] = segment.Smooth;
            }
            if (points.Any(p => !float.IsFinite((float)p.X) || !float.IsFinite((float)p.Y))) return BadNumber;
            if (!hadGap) figures.Add(new(points.ToArray(), figure.Closed, SmoothJoins: smooth.ToArray(), Fillable: figure.Fillable));
            else
            {
                if (figure.Closed) { points.Add(Map(figure.Points[0])); smooth.Add(false); }
                if (points.Count > 1) runs.Add(new(points.ToArray(), false, startsAtGap, figure.Closed, smooth.ToArray()));
                if (figure.Closed && runs.Count > 1 && !runs[0].StartsAtGap && runs[^1].Points[^1] == runs[0].Points[0])
                {
                    var a = runs[0]; var b = runs[^1];
                    runs[0] = new(b.Points.Concat(a.Points.Skip(1)).ToArray(), false, b.StartsAtGap, a.EndsAtGap,
                        b.SmoothJoins!.Concat(a.SmoothJoins!.Skip(1)).ToArray());
                    runs.RemoveAt(runs.Count - 1);
                }
                figures.AddRange(runs);
            }
        }
        return 0;
    }

    internal static int Widen(List<SoftwareStrokeContours.Figure> figures, Pen* pen, double* dashes,
        out List<SoftwareStrokeContours.Figure> result)
    {
        result = [];
        if (pen == null || pen->DashBytes % 8 != 0 || pen->DashBytes > int.MaxValue || (pen->DashBytes != 0 && dashes == null)
            || pen->Start > MilPenCap.Triangle || pen->End > MilPenCap.Triangle || pen->Dash > MilPenCap.Triangle || pen->Join > MilPenJoin.Round) return Invalid;
        var values = new ReadOnlySpan<double>(dashes, (int)pen->DashBytes / 8).ToArray();
        if (values.Any(d => !float.IsFinite((float)d))) return BadNumber;
        var dash = values.Length == 0 ? null : new GeneratedDashStyleResource((float)pen->Offset, values);
        var style = new GeneratedPenResource((float)pen->Thickness, Math.Max(1, (float)pen->Miter), pen->Start, pen->End, pen->Dash, pen->Join, dash);
        if (!SoftwareStrokeWidening.TryCreate(figures, style, GeneratedImageTransform.Identity, out _, out _, false, outputContours: result)) return BadNumber;
        return 0;
    }

    internal static int Normalize(List<SoftwareStrokeContours.Figure> figures, MilFillMode rule, out List<SoftwareStrokeContours.Figure> result)
        => SoftwareBooleanContours.TryCombine(figures, rule, [], MilFillMode.Winding, MilCombineMode.Union, out result) ? 0 : BadNumber;

    internal static void Emit(List<SoftwareStrokeContours.Figure> figures, nint callback)
    {
        var emit = (delegate* unmanaged[Stdcall]<int, int, float*, uint, byte*, uint, void>)callback;
        foreach (var figure in figures)
        {
            if (figure.Points.Length == 0) continue;
            var points = figure.Points;
            if (figure.Closed && points[^1] != points[0]) points = [.. points, points[0]];
            var coordinates = new float[points.Length * 2];
            for (int i = 0; i < points.Length; i++) { coordinates[i * 2] = (float)points[i].X; coordinates[i * 2 + 1] = (float)points[i].Y; }
            var types = Enumerable.Repeat((byte)1, points.Length - 1).ToArray();
            fixed (float* p = coordinates)
            fixed (byte* t = types) emit(figure.Fillable ? 1 : 0, figure.Closed ? 1 : 0, p, (uint)points.Length, t, (uint)types.Length);
        }
    }

    internal static double Area(List<SoftwareStrokeContours.Figure> figures)
    {
        double area = 0;
        foreach (var figure in figures)
            for (int i = 0; i < figure.Points.Length; i++)
            {
                var a = figure.Points[i]; var b = figure.Points[(i + 1) % figure.Points.Length];
                area += a.X * b.Y - a.Y * b.X;
            }
        return Math.Abs(area) * 0.5;
    }

    internal static bool Hit(List<SoftwareStrokeContours.Figure> figures, MilFillMode rule, MilPoint2D point, double threshold)
    {
        int winding = 0;
        foreach (var figure in figures)
        {
            if (!figure.Fillable) continue;
            var points = figure.Points;
            for (int i = 0; i < points.Length; i++)
            {
                var a = points[i]; var b = points[(i + 1) % points.Length];
                double dx = b.X - a.X, dy = b.Y - a.Y;
                double length = dx * dx + dy * dy;
                double t = length == 0 ? 0 : Math.Clamp(((point.X - a.X) * dx + (point.Y - a.Y) * dy) / length, 0, 1);
                double x = point.X - a.X - t * dx, y = point.Y - a.Y - t * dy;
                if (x * x + y * y <= threshold * threshold) return true;
                bool up = a.Y <= point.Y && b.Y > point.Y, down = b.Y <= point.Y && a.Y > point.Y;
                if ((up || down) && a.X + (point.Y - a.Y) * dx / dy > point.X) winding += up ? 1 : -1;
            }
        }
        return rule == MilFillMode.Winding ? winding != 0 : (winding & 1) != 0;
    }
}
