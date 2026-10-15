using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class NativeShapeReader
{
    internal static int Capture(nint shape, GeneratedImageTransform transform, out SoftwareImageCoverage? coverage,
        Direct3D9SurfaceRect? clip = null)
    {
        coverage = null;
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
            var p = transform.Apply(point[0], point[1]);
            if (!double.IsFinite(p.X) || !double.IsFinite(p.Y))
                return Direct3D9Factory.InvalidArgumentHResult;
            var previous = new MilPoint2D(point[0], point[1]);
            var points = new List<MilPoint2D> { new(p.X, p.Y) };
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
                        var end = transform.Apply(values[0], values[1]);
                        if (!double.IsFinite(end.X) || !double.IsFinite(end.Y))
                            return Direct3D9Factory.InvalidArgumentHResult;
                        points.Add(new(end.X, end.Y));
                        previous = new(values[0], values[1]);
                    }
                    else if ((type & 3) == 2)
                    {
                        for (int coordinate = 0; coordinate < 6; coordinate++)
                            if (!float.IsFinite(values[coordinate])) return Direct3D9Factory.InvalidArgumentHResult;
                        foreach (var end in SoftwareBezierFlattener.Flatten(new(previous.X, previous.Y),
                            new(values[0], values[1]), new(values[2], values[3]), new(values[4], values[5]), transform))
                        {
                            var device = transform.Apply(end.X, end.Y);
                            if (!double.IsFinite(device.X) || !double.IsFinite(device.Y))
                                return Direct3D9Factory.InvalidArgumentHResult;
                            points.Add(new(device.X, device.Y));
                        }
                        previous = new(values[4], values[5]);
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
