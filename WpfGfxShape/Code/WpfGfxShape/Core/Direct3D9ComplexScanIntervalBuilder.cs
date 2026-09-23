namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9CoverageInterval(int PixelX, int Coverage);

internal sealed class Direct3D9ComplexScanIntervalBuilder
{
    private const int FullCoverage = 64;

    private readonly bool _needInsideGeometry;
    private readonly Direct3D9SurfaceRect? _outsideBounds;
    private readonly Direct3D9AddWaffleLine _lineSink;

    internal Direct3D9ComplexScanIntervalBuilder(
        bool needInsideGeometry,
        Direct3D9SurfaceRect? outsideBounds,
        Direct3D9AddWaffleLine lineSink)
    {
        ArgumentNullException.ThrowIfNull(lineSink);
        if (!needInsideGeometry && outsideBounds is null)
        {
            throw new ArgumentException("Inside geometry can only be omitted when outside bounds are provided.", nameof(needInsideGeometry));
        }

        _needInsideGeometry = needInsideGeometry;
        _outsideBounds = outsideBounds;
        _lineSink = lineSink;
    }

    internal int CountSegments(IReadOnlyList<Direct3D9CoverageInterval> intervals)
    {
        ValidateIntervals(intervals);

        int segmentCount = 0;
        for (int index = 0; index < intervals.Count - 1; index++)
        {
            Direct3D9CoverageInterval interval = intervals[index];
            if (interval.PixelX == int.MaxValue)
            {
                break;
            }

            if (NeedCoverageGeometry(interval.Coverage))
            {
                segmentCount++;
            }
        }

        return segmentCount;
    }

    internal int AddScan(int pixelY, IReadOnlyList<Direct3D9CoverageInterval> intervals)
    {
        ValidateIntervals(intervals);

        float pixelCenterY = pixelY + 0.5f;
        for (int index = 0; index < intervals.Count - 1; index++)
        {
            Direct3D9CoverageInterval interval = intervals[index];
            if (interval.PixelX == int.MaxValue)
            {
                break;
            }

            if (!NeedCoverageGeometry(interval.Coverage))
            {
                continue;
            }

            int begin = interval.PixelX;
            int end = intervals[index + 1].PixelX;
            if (_outsideBounds is Direct3D9SurfaceRect outsideBounds)
            {
                begin = Math.Max(begin, Math.Min(end, outsideBounds.Left));
                end = Math.Min(end, Math.Max(begin, outsideBounds.Right));
            }

            float coverage = (float)interval.Coverage / FullCoverage;
            int result = _lineSink(
                new Direct3D9WafflePoint(begin + 0.5f, pixelCenterY, coverage),
                new Direct3D9WafflePoint(end + 0.5f, pixelCenterY, coverage));
            if (result < 0)
            {
                return result;
            }
        }

        return 0;
    }

    private static void ValidateIntervals(IReadOnlyList<Direct3D9CoverageInterval> intervals)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        if (intervals.Count == 0 || intervals[^1].PixelX != int.MaxValue)
        {
            throw new ArgumentException("Coverage intervals must end with an INT_MAX sentinel.", nameof(intervals));
        }
    }

    private bool NeedCoverageGeometry(int coverage)
    {
        return (_needInsideGeometry || coverage != FullCoverage)
            && (_outsideBounds is not null || coverage != 0);
    }
}
