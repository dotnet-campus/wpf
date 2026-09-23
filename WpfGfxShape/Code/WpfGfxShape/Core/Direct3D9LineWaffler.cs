namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9WafflePoint(float X, float Y, float Alpha);

internal delegate int Direct3D9AddWaffleLine(
    Direct3D9WafflePoint start,
    Direct3D9WafflePoint end);

internal sealed class Direct3D9LineWaffler
{
    private float _a;
    private float _b;
    private float _c;
    private Direct3D9AddWaffleLine? _consumer;

    internal void Set(float a, float b, float c, Direct3D9AddWaffleLine consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);

        _a = a;
        _b = b;
        _c = c - MathF.Truncate(c);
        _consumer = consumer;
    }

    internal void SetConsumer(Direct3D9AddWaffleLine consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        _consumer = consumer;
    }

    internal int AddLine(Direct3D9WafflePoint start, Direct3D9WafflePoint end)
    {
        Direct3D9AddWaffleLine consumer = _consumer
            ?? throw new InvalidOperationException("The line waffler requires an output consumer.");

        float startScore = Score(start);
        float endScore = Score(end);
        if (startScore > endScore)
        {
            startScore = -startScore;
            endScore = -endScore;
        }

        if (!(endScore >= startScore))
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        int startCell = SaturatingFloor(startScore);
        int endCell = SaturatingFloor(endScore);
        if (startCell == endCell)
        {
            return consumer(start, end);
        }

        Direct3D9WafflePoint lastPoint = start;
        for (int boundary = startCell + 1; boundary < endCell + 1; boundary++)
        {
            Direct3D9WafflePoint nextPoint = SplitEdge(startScore, start, endScore, end, boundary);
            int result = consumer(lastPoint, nextPoint);
            if (result < 0)
            {
                return result;
            }

            lastPoint = nextPoint;
        }

        return consumer(lastPoint, end);
    }

    private float Score(Direct3D9WafflePoint point)
    {
        return point.X * _a + point.Y * _b + _c;
    }

    private static Direct3D9WafflePoint SplitEdge(
        float startScore,
        Direct3D9WafflePoint start,
        float endScore,
        Direct3D9WafflePoint end,
        float splitScore)
    {
        float startDistance = splitScore - startScore;
        float endDistance = endScore - splitScore;
        if (startDistance < endDistance)
        {
            float amount = Math.Clamp(startDistance / (startDistance + endDistance), 0f, 1f);
            return Interpolate(start, end, amount);
        }

        float reverseAmount = Math.Clamp(endDistance / (startDistance + endDistance), 0f, 1f);
        return Interpolate(end, start, reverseAmount);
    }

    private static Direct3D9WafflePoint Interpolate(
        Direct3D9WafflePoint start,
        Direct3D9WafflePoint end,
        float amount)
    {
        float inverseAmount = 1f - amount;
        return new Direct3D9WafflePoint(
            Math.Clamp(inverseAmount * start.X + amount * end.X, Math.Min(start.X, end.X), Math.Max(start.X, end.X)),
            Math.Clamp(inverseAmount * start.Y + amount * end.Y, Math.Min(start.Y, end.Y), Math.Max(start.Y, end.Y)),
            Math.Clamp(inverseAmount * start.Alpha + amount * end.Alpha, Math.Min(start.Alpha, end.Alpha), Math.Max(start.Alpha, end.Alpha)));
    }

    private static int SaturatingFloor(float value)
    {
        if (value >= int.MaxValue)
        {
            return int.MaxValue;
        }

        if (value <= int.MinValue)
        {
            return int.MinValue;
        }

        return (int) MathF.Floor(value);
    }
}
