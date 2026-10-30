using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class NativeShapeReader
{
    internal static int Capture(nint shape, GeneratedImageTransform transform, out SoftwareImageCoverage? coverage,
        Direct3D9SurfaceRect? clip = null)
    {
        coverage = null;
        bool TryRasterPoint(float x, float y, out SoftwareFillBezier.Point point)
        {
            float deviceX = (float)transform.ScaleX * 16f * x + (float)transform.M21 * 16f * y
                + ((float)transform.X - 0.5f) * 16f;
            float deviceY = (float)transform.M12 * 16f * x + (float)transform.ScaleY * 16f * y
                + ((float)transform.Y - 0.5f) * 16f;
            point = default;
            if (!SoftwareFillBezier.TryRound(deviceX, out int px)
                || !SoftwareFillBezier.TryRound(deviceY, out int py))
            {
                JpBrushTrace.Record("Capture.TryRound", Direct3D9Factory.InvalidArgumentHResult,
                    $"input=({x:R},{y:R});raster=({deviceX:R},{deviceY:R});transform={transform};clip={clip}");
                return false;
            }
            point = new(px, py);
            return true;
        }
        static MilPoint2D DevicePoint(SoftwareFillBezier.Point point)
            => new(point.X / 16d + 0.5, point.Y / 16d + 0.5);
        if (shape == 0) return 0;
        void** table = *(void***)shape;
        uint count = ((delegate* unmanaged<nint, uint>)table[4])(shape);
        var fill = (MilFillMode)((delegate* unmanaged<nint, uint>)table[6])(shape);
        var figures = new List<MilPoint2D[]>();
        for (uint i = 0; i < count; i++)
        {
            nint figure = ((delegate* unmanaged<nint, uint, nint>)table[5])(shape, i);
            if (figure == 0) return Direct3D9Factory.InvalidArgumentHResult;
            void** methods = *(void***)figure;
            if (((delegate* unmanaged<nint, byte>)methods[1])(figure) != 0
                || ((delegate* unmanaged<nint, byte>)methods[8])(figure) == 0) continue;
            nint start = ((delegate* unmanaged<nint, nint>)methods[22])(figure);
            if (start == 0) return Direct3D9Factory.InvalidArgumentHResult;
            float* point = (float*)start;
            if (!float.IsFinite(point[0]) || !float.IsFinite(point[1]))
                return Direct3D9Factory.InvalidArgumentHResult;
            if (!TryRasterPoint(point[0], point[1], out var previous))
                return CaptureSafeRectangle(shape, transform, fill, clip, out coverage);
            var points = new List<MilPoint2D> { DevicePoint(previous) };
            if (((delegate* unmanaged<nint, byte>)methods[15])(figure) != 0)
            {
                bool last;
                do
                {
                    byte type = 0;
                    nint data = 0;
                    last = ((delegate* unmanaged<nint, byte*, nint*, byte>)methods[16])(figure, &type, &data) != 0;
                    if (data == 0) return Direct3D9Factory.InvalidArgumentHResult;
                    float* values = (float*)data;
                    if ((type & 3) == 1)
                    {
                        if (!float.IsFinite(values[0]) || !float.IsFinite(values[1]))
                            return Direct3D9Factory.InvalidArgumentHResult;
                        if (!TryRasterPoint(values[0], values[1], out previous))
                            return CaptureSafeRectangle(shape, transform, fill, clip, out coverage);
                        points.Add(DevicePoint(previous));
                    }
                    else if ((type & 3) == 2)
                    {
                        for (int coordinate = 0; coordinate < 6; coordinate++)
                            if (!float.IsFinite(values[coordinate])) return Direct3D9Factory.InvalidArgumentHResult;
                        if (!TryRasterPoint(values[0], values[1], out var first)
                            || !TryRasterPoint(values[2], values[3], out var second)
                            || !TryRasterPoint(values[4], values[5], out var lastPoint))
                            return Direct3D9Factory.InvalidArgumentHResult;
                        foreach (var end in SoftwareFillBezier.Flatten([previous, first, second, lastPoint]))
                            points.Add(DevicePoint(end));
                        previous = lastPoint;
                    }
                    else return Direct3D9Factory.NotImplementedHResult;
                    if (!last && ((delegate* unmanaged<nint, byte>)methods[17])(figure) == 0)
                        return Direct3D9Factory.InvalidArgumentHResult;
                } while (!last);
            }
            figures.Add(points.ToArray());
        }
        if (!SoftwarePathCoverage.TryCreateFigures(figures, fill, out var path, clip)) return Direct3D9Factory.InvalidArgumentHResult;
        if (path is null) return Direct3D9Factory.UnexpectedHResult;
        coverage = new(path);
        return 0;
    }

    private static int CaptureSafeRectangle(nint shape, GeneratedImageTransform transform, MilFillMode fill,
        Direct3D9SurfaceRect? clip, out SoftwareImageCoverage? coverage)
    {
        coverage = null;
        if (transform.M12 != 0 || transform.M21 != 0) return Direct3D9Factory.InvalidArgumentHResult;
        void** table = *(void***)shape;
        if (((delegate* unmanaged<nint, uint>)table[4])(shape) != 1) return Direct3D9Factory.InvalidArgumentHResult;
        nint figure = ((delegate* unmanaged<nint, uint, nint>)table[5])(shape, 0);
        if (figure == 0) return Direct3D9Factory.InvalidArgumentHResult;
        void** methods = *(void***)figure;
        float* start = (float*)((delegate* unmanaged<nint, nint>)methods[22])(figure);
        if (start == null) return Direct3D9Factory.InvalidArgumentHResult;
        var vertices = new List<MilPoint2D> { new(start[0], start[1]) };
        if (((delegate* unmanaged<nint, byte>)methods[15])(figure) == 0) return Direct3D9Factory.InvalidArgumentHResult;
        bool last;
        do
        {
            byte type = 0;
            nint data = 0;
            last = ((delegate* unmanaged<nint, byte*, nint*, byte>)methods[16])(figure, &type, &data) != 0;
            if (data == 0 || (type & 3) != 1 || vertices.Count > 4) return Direct3D9Factory.InvalidArgumentHResult;
            float* values = (float*)data;
            vertices.Add(new(values[0], values[1]));
            if (!last && ((delegate* unmanaged<nint, byte>)methods[17])(figure) == 0)
                return Direct3D9Factory.InvalidArgumentHResult;
        } while (!last);
        if (vertices.Count == 5 && vertices[0].Equals(vertices[4])) vertices.RemoveAt(4);
        if (vertices.Count != 4) return Direct3D9Factory.InvalidArgumentHResult;
        for (int i = 0; i < 4; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % 4];
            var c = vertices[(i + 2) % 4];
            if (!double.IsFinite(a.X) || !double.IsFinite(a.Y)
                || !((a.X == b.X && a.Y != b.Y && b.Y == c.Y && b.X != c.X)
                    || (a.Y == b.Y && a.X != b.X && b.X == c.X && b.Y != c.Y)))
                return Direct3D9Factory.InvalidArgumentHResult;
        }
        // Clip a proven straight rectangle in device space before 28.4 conversion.
        // WPF RenderingConstants.h and ClipToSafeDeviceBounds use this safe extent.
        const double safe = 524287;
        double x0 = transform.ScaleX * vertices.Min(p => p.X) + transform.X;
        double x1 = transform.ScaleX * vertices.Max(p => p.X) + transform.X;
        double y0 = transform.ScaleY * vertices.Min(p => p.Y) + transform.Y;
        double y1 = transform.ScaleY * vertices.Max(p => p.Y) + transform.Y;
        if (!double.IsFinite(x0) || !double.IsFinite(x1) || !double.IsFinite(y0) || !double.IsFinite(y1))
            return Direct3D9Factory.InvalidArgumentHResult;
        double left = Math.Max(-safe, Math.Min(x0, x1)), right = Math.Min(safe, Math.Max(x0, x1));
        double top = Math.Max(-safe, Math.Min(y0, y1)), bottom = Math.Min(safe, Math.Max(y0, y1));
        MilPoint2D[][] rectangles = left < right && top < bottom
            ? [[new(left, top), new(right, top), new(right, bottom), new(left, bottom)]] : [];
        if (!SoftwarePathCoverage.TryCreateFigures(rectangles, fill, out var path, clip) || path is null)
            return Direct3D9Factory.InvalidArgumentHResult;
        coverage = new(path);
        return 0;
    }

}
