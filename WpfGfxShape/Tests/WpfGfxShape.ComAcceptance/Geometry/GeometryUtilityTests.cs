using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class GeometryUtilityTests
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Point(double x, double y) { public double X = x, Y = y; }
    [StructLayout(LayoutKind.Sequential)]
    private struct Matrix { public double M11, M12, M21, M22, X, Y; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Pen { public double Width, Miter, Offset; public uint Start, End, Dash, Join, DashBytes; }
    private sealed record Figure(bool Filled, bool Closed, double[] Points, byte[] Types);
    private sealed record Observation(string Name, int HResult, double[] Values, Figure[] Figures);
    private sealed class Receiver { internal List<Figure> Figures = []; internal string? Error; internal bool Reject; }
    [ThreadStatic] private static Receiver? receiver;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Receive(int filled, int closed, float* points, uint count, byte* types, uint segments)
    {
        var target = receiver;
        if (target is null) return;
        try
        {
            var values = new double[checked((int)count * 2)];
            for (int i = 0; i < values.Length; i++) values[i] = points[i];
            target.Figures.Add(new(filled != 0, closed != 0, values, new ReadOnlySpan<byte>(types, (int)segments).ToArray()));
            if (target.Reject) target.Error = "ReceiverRejectedFigure";
        }
        catch (OverflowException error) { target.Error = error.GetType().Name; }
    }

    [TestMethod]
    [DataRow("normal")]
    [DataRow("empty")]
    [DataRow("degenerate")]
    [DataRow("nonfinite")]
    [DataRow("curves")]
    [DataRow("complex")]
    [DataRow("winding")]
    [DataRow("transform")]
    [DataRow("stroke")]
    [DataRow("relative")]
    [DataRow("callback-reject")]
    [DataRow("null-callback")]
    public void WhenGeometryFamilyIsInvokedThenPublicOriginalAndCandidateAgree(string scenario)
    {
        string reference = Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll");
        string candidate = AcceptancePaths.AotDll;
        var original = Invoke(reference, scenario);
        var actual = Invoke(candidate, scenario);
        string evidence = Path.Combine(AcceptancePaths.Results, $"geometry-{scenario}-{Guid.NewGuid():N}.json");
        File.WriteAllText(evidence, JsonSerializer.Serialize(new
        {
            Scenario = scenario,
            Reference = Identity(reference), Candidate = Identity(candidate), Original = original, Actual = actual,
            CallbackContract = "void Stdcall; receiver rejection is recorded locally, not an HRESULT return"
        }, new JsonSerializerOptions { WriteIndented = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals }));
        var differences = new List<string>();
        for (int i = 0; i < original.Count; i++)
        {
            var a = original[i]; var b = actual[i];
            if (a.HResult != b.HResult) differences.Add($"{a.Name}: HRESULT {a.HResult:X8}/{b.HResult:X8}");
            if (a.HResult >= 0 && a.Values.Length == b.Values.Length)
                for (int j = 0; j < a.Values.Length; j++)
                    if (!Equivalent(a.Values[j], b.Values[j], a.Name == "length" ? 0.002 : 0.0001)) differences.Add($"{a.Name}[{j}]: {a.Values[j]}/{b.Values[j]}");
            if (a.HResult >= 0 && a.Name is "flatten" or "outline" or "widen" or "combine")
            {
                if ((a.Figures.Length == 0) != (b.Figures.Length == 0)) differences.Add($"{a.Name}: empty output mismatch");
                double[] x = Coverage(a.Figures), y = Coverage(b.Figures);
                if (!x.SequenceEqual(y)) differences.Add($"{a.Name}: output contour fill mismatch");
            }
        }
        Assert.AreEqual("", string.Join("; ", differences), evidence + ": " + string.Join("; ", differences));
    }

    private static object Identity(string path) => new { Path = path, Version = FileVersionInfo.GetVersionInfo(path).FileVersion,
        Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) };
    private static bool Equivalent(double a, double b, double tolerance) => a.Equals(b) || double.IsNaN(a) && double.IsNaN(b)
        || Math.Abs(a - b) <= tolerance * Math.Max(1, Math.Max(Math.Abs(a), Math.Abs(b)));

    private static List<Observation> Invoke(string path, string scenario)
    {
        nint library = NativeLibrary.Load(path);
        nint Export(string name) => NativeLibrary.GetExport(library, "MilUtility_" + name);
        var output = new List<Observation>();
        byte[] packet = Packet(scenario), other = Packet("other");
        uint rule = scenario == "winding" ? 1u : 0u;
        Matrix identity = new() { M11 = 1, M22 = 1 };
        Matrix matrix = scenario == "transform" ? new() { M11 = 1.5, M12 = 0.25, M21 = -0.5, M22 = 2, X = 3, Y = -2 } : identity;
        Pen pen = new() { Width = 2, Miter = 4, Start = 2, End = 2, Dash = 2, Join = 2, DashBytes = scenario == "stroke" ? 24u : 0u };
        double* dashes = stackalloc double[3] { 2, 1, 3 };
        double tolerance = scenario == "relative" ? 0.001 : 0.01;
        byte relative = scenario == "relative" ? (byte)1 : (byte)0;
        nint callback = scenario == "null-callback" ? 0 : (nint)(delegate* unmanaged[Stdcall]<int, int, float*, uint, byte*, uint, void>)&Receive;
        var target = new Receiver { Reject = scenario == "callback-reject" };
        receiver = target;
        try
        {
            fixed (byte* data = packet)
            fixed (byte* second = other)
            {
                uint size = (uint)packet.Length;
                foreach (string operation in new[] { "flatten", "outline", "widen", "combine" })
                {
                    target.Figures.Clear(); uint fill = 0; int hr;
                    if (operation == "combine")
                        hr = ((delegate* unmanaged[Stdcall]<Matrix*, Matrix*, uint, byte*, uint, Matrix*, uint, byte*, uint, double, byte, nint, uint, uint*, int>)Export("PathGeometryCombine"))(&identity, &matrix, rule, data, size, &identity, 0, second, (uint)other.Length, tolerance, relative, callback, 0, &fill);
                    else if (operation == "widen")
                        hr = ((delegate* unmanaged[Stdcall]<Pen*, double*, Matrix*, uint, byte*, uint, double, byte, nint, uint*, int>)Export("PathGeometryWiden"))(&pen, pen.DashBytes == 0 ? null : dashes, &matrix, rule, data, size, tolerance, relative, callback, &fill);
                    else
                        hr = ((delegate* unmanaged[Stdcall]<Matrix*, uint, byte*, uint, double, byte, nint, uint*, int>)Export(operation == "flatten" ? "PathGeometryFlatten" : "PathGeometryOutline"))(&matrix, rule, data, size, tolerance, relative, callback, &fill);
                    output.Add(new(operation, hr, [fill], target.Figures.ToArray()));
                }
                double area = -123;
                int result = ((delegate* unmanaged[Stdcall]<uint, byte*, uint, Matrix*, double, byte, double*, int>)Export("GeometryGetArea"))(rule, data, size, &matrix, tolerance, relative, &area);
                output.Add(new("area", result, [area], []));
                Point point = default, tangent = default;
                result = ((delegate* unmanaged[Stdcall]<Matrix*, uint, byte*, uint, double, Point*, Point*, int>)Export("GetPointAtLengthFraction"))(&matrix, rule, data, size, 0.37, &point, &tangent);
                output.Add(new("length", result, [point.X, point.Y, tangent.X, tangent.Y], []));
                double* bounds = stackalloc double[4];
                result = ((delegate* unmanaged[Stdcall]<Pen*, double*, Matrix*, uint, byte*, uint, Matrix*, double, byte, byte, double*, int>)Export("PathGeometryBounds"))(scenario == "stroke" ? &pen : null, scenario == "stroke" ? dashes : null, &identity, rule, data, size, &matrix, tolerance, relative, 0, bounds);
                output.Add(new("bounds", result, new ReadOnlySpan<double>(bounds, 4).ToArray(), []));
                int hit = -1; point = new(5, 5);
                result = ((delegate* unmanaged[Stdcall]<Matrix*, Pen*, double*, uint, byte*, uint, double, byte, Point*, int*, int>)Export("PathGeometryHitTest"))(&matrix, scenario == "stroke" ? &pen : null, scenario == "stroke" ? dashes : null, rule, data, size, tolerance, relative, &point, &hit);
                output.Add(new("hit", result, [hit], []));
                uint relation = 99;
                result = ((delegate* unmanaged[Stdcall]<Matrix*, uint, byte*, uint, Matrix*, uint, byte*, uint, double, byte, uint*, int>)Export("PathGeometryHitTestPathGeometry"))(&matrix, rule, data, size, &identity, 0, second, (uint)other.Length, tolerance, relative, &relation);
                output.Add(new("relation", result, [relation], []));
                Point* polygon = stackalloc Point[5] { new(0, 0), new(10, 0), new(10, 10), new(0, 10), new(0, 0) };
                byte* types = stackalloc byte[4] { 17, 1, 1, 1 };
                result = ((delegate* unmanaged[Stdcall]<Matrix*, Pen*, double*, Point*, byte*, uint, uint, Matrix*, double, byte, byte, double*, int>)Export("PolygonBounds"))(&identity, null, null, polygon, types, 5, 4, &matrix, tolerance, relative, 0, bounds);
                output.Add(new("polygon-bounds", result, new ReadOnlySpan<double>(bounds, 4).ToArray(), []));
                result = ((delegate* unmanaged[Stdcall]<Matrix*, Pen*, double*, Point*, byte*, uint, uint, double, byte, Point*, int*, int>)Export("PolygonHitTest"))(&matrix, null, null, polygon, types, 5, 4, tolerance, relative, &point, &hit);
                output.Add(new("polygon-hit", result, [hit], []));
                Point* controls = stackalloc Point[12]; int pieces = -99;
                ((delegate* unmanaged[Stdcall]<Point, Point, double, byte, byte, Point, Matrix*, Point*, int*, void>)Export("ArcToBezier"))(new(0, 0), new(8, 5), 30, 1, 1, new(10, 10), &matrix, controls, &pieces);
                var values = new List<double> { pieces };
                for (int i = 0; i < Math.Max(0, pieces * 3); i++) { values.Add(controls[i].X); values.Add(controls[i].Y); }
                output.Add(new("arc", 0, values.ToArray(), []));
                output.Add(new("receiver-error", 0, [target.Error is null ? 0 : 1], []));
            }
        }
        finally
        {
            receiver = null;
            // NativeAOT DLL unloading is not supported; retain both modules until process exit.
        }
        return output;
    }

    private static byte[] Packet(string scenario)
    {
        var figures = new List<(Point Start, Point[] Points, uint Type)>();
        if (scenario != "empty")
        {
            double x = scenario == "other" ? 5 : scenario == "nonfinite" ? double.NaN : 0;
            Point start = new(x, 0);
            if (scenario == "curves") figures.Add((start, [new(0, 15), new(10, -5), new(10, 10), new(15, 10), new(15, 0), start], 6));
            else if (scenario == "degenerate") figures.Add((start, [start, start, start], 5));
            else figures.Add((start, [new(x + 10, 0), new(x + 10, 10), new(x, 10), start], 5));
            if (scenario is "complex" or "winding") figures.Add((new(2, 2), [new(8, 2), new(8, 8), new(2, 8), new(2, 2)], 5));
        }
        int size = 48 + figures.Sum(f => 56 + f.Points.Length * 16);
        var data = new byte[size];
        void Put(int offset, uint value) => MemoryMarshal.Write(data.AsSpan(offset), in value);
        Put(0, (uint)size); Put(40, (uint)figures.Count);
        int position = 48, previous = 0;
        foreach (var figure in figures)
        {
            int length = 56 + figure.Points.Length * 16;
            Put(position, (uint)previous); Put(position + 4, 12); Put(position + 8, 1); Put(position + 12, (uint)length);
            var start = figure.Start; MemoryMarshal.Write(data.AsSpan(position + 16), in start);
            Put(position + 32, 40); Put(position + 40, figure.Type); Put(position + 52, (uint)figure.Points.Length);
            for (int i = 0; i < figure.Points.Length; i++) MemoryMarshal.Write(data.AsSpan(position + 56 + i * 16), in figure.Points[i]);
            position += length; previous = length;
        }
        return data;
    }

    private static double[] Contour(Figure figure)
    {
        var points = figure.Points;
        if (points.Length == 0) return [];
        var result = new List<double> { points[0], points[1] };
        int position = 2;
        foreach (byte type in figure.Types)
        {
            if ((type & 3) == 1)
            {
                result.Add(points[position++]); result.Add(points[position++]);
                continue;
            }
            double x = result[^2], y = result[^1];
            double ax = points[position++], ay = points[position++];
            double bx = points[position++], by = points[position++];
            double ex = points[position++], ey = points[position++];
            Append(x, y, ax, ay, bx, by, ex, ey);
        }
        return result.ToArray();

        void Append(double x, double y, double ax, double ay, double bx, double by, double ex, double ey)
        {
            // Subdivide the curve, not its control polygon, at the invocation's absolute tolerance.
            if (Math.Max(Math.Max(Math.Abs(x - 2 * ax + bx), Math.Abs(y - 2 * ay + by)),
                Math.Max(Math.Abs(ax - 2 * bx + ex), Math.Abs(ay - 2 * by + ey))) <= 0.01)
            {
                result.Add(ex); result.Add(ey); return;
            }
            double p = (x + ax) / 2, q = (y + ay) / 2, r = (ax + bx) / 2, s = (ay + by) / 2;
            double u = (bx + ex) / 2, v = (by + ey) / 2;
            double a = (p + r) / 2, b = (q + s) / 2, c = (r + u) / 2, d = (s + v) / 2;
            double m = (a + c) / 2, n = (b + d) / 2;
            Append(x, y, p, q, a, b, m, n);
            Append(m, n, c, d, u, v, ex, ey);
        }
    }

    private static double[] Coverage(Figure[] figures)
    {
        var result = new List<double>();
        for (int y = -4; y <= 20; y += 2)
        for (int x = -4; x <= 20; x += 2)
        {
            int winding = 0;
            foreach (var figure in figures)
            {
                if (!figure.Filled) continue;
                var points = Contour(figure);
                for (int i = 0; i < points.Length; i += 2)
                {
                    int j = (i + 2) % points.Length;
                    double ax = points[i], ay = points[i + 1], bx = points[j], by = points[j + 1];
                    double px = x + 0.37, py = y + 0.29;
                    bool up = ay <= py && by > py, down = by <= py && ay > py;
                    if ((up || down) && ax + (py - ay) * (bx - ax) / (by - ay) > px) winding += up ? 1 : -1;
                }
            }
            result.Add(winding == 0 ? 0 : 1);
        }
        return result.ToArray();
    }
}
