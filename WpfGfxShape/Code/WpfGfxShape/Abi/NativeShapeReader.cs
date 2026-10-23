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
                || !SoftwareFillBezier.TryRound(deviceY, out int py)) return false;
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
                return Direct3D9Factory.InvalidArgumentHResult;
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
                            return Direct3D9Factory.InvalidArgumentHResult;
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

}
