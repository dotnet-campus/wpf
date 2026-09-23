namespace WpfGfxShape.Core;

internal delegate int Direct3D9AddWaffleTriangle(
    Direct3D9WafflePoint first,
    Direct3D9WafflePoint second,
    Direct3D9WafflePoint third);

internal sealed class Direct3D9TriangleWaffler
{
    private float _a;
    private float _b;
    private float _c;
    private Direct3D9AddWaffleTriangle? _consumer;

    internal void Set(float a, float b, float c, Direct3D9AddWaffleTriangle consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);

        _a = a;
        _b = b;
        _c = c - MathF.Truncate(c);
        _consumer = consumer;
    }

    internal void SetConsumer(Direct3D9AddWaffleTriangle consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        _consumer = consumer;
    }

    internal int AddTriangle(
        Direct3D9WafflePoint first,
        Direct3D9WafflePoint second,
        Direct3D9WafflePoint third)
    {
        Direct3D9AddWaffleTriangle consumer = _consumer
            ?? throw new InvalidOperationException("The triangle waffler requires an output consumer.");

        Direct3D9WafflePoint[] vertices = [first, second, third];
        float[] scores = [Score(first), Score(second), Score(third)];
        SortByScore(vertices, scores);
        if (!(scores[1] >= scores[0]) || !(scores[2] >= scores[1]))
        {
            return Direct3D9Factory.BadNumberHResult;
        }

        int[] cells = [SaturatingFloor(scores[0]), SaturatingFloor(scores[1]), SaturatingFloor(scores[2])];
        if (cells[2] == int.MaxValue)
        {
            return Direct3D9Factory.BadNumberHResult;
        }

        for (int cell = cells[0]; cell <= cells[2]; cell++)
        {
            int leftCount = 0;
            int rightCount = 0;
            for (int index = 0; index < cells.Length; index++)
            {
                leftCount += cells[index] < cell ? 1 : 0;
                rightCount += cells[index] > cell ? 1 : 0;
            }

            int result = (leftCount << 4 | rightCount) switch
            {
                0x00 => SendTriangle(consumer, vertices[0], vertices[1], vertices[2]),
                0x01 => SendQuad(
                    consumer,
                    vertices[0],
                    vertices[1],
                    SplitEdge(scores[1], vertices[1], scores[2], vertices[2], cell + 1),
                    SplitEdge(scores[0], vertices[0], scores[2], vertices[2], cell + 1)),
                0x02 => SendTriangle(
                    consumer,
                    vertices[0],
                    SplitEdge(scores[0], vertices[0], scores[1], vertices[1], cell + 1),
                    SplitEdge(scores[0], vertices[0], scores[2], vertices[2], cell + 1)),
                0x10 => SendQuad(
                    consumer,
                    SplitEdge(scores[0], vertices[0], scores[1], vertices[1], cell),
                    vertices[1],
                    vertices[2],
                    SplitEdge(scores[2], vertices[2], scores[0], vertices[0], cell)),
                0x11 => SendPentagon(
                    consumer,
                    SplitEdge(scores[0], vertices[0], scores[1], vertices[1], cell),
                    vertices[1],
                    SplitEdge(scores[1], vertices[1], scores[2], vertices[2], cell + 1),
                    SplitEdge(scores[2], vertices[2], scores[0], vertices[0], cell + 1),
                    SplitEdge(scores[2], vertices[2], scores[0], vertices[0], cell)),
                0x12 => SendQuad(
                    consumer,
                    SplitEdge(scores[0], vertices[0], scores[1], vertices[1], cell),
                    SplitEdge(scores[0], vertices[0], scores[1], vertices[1], cell + 1),
                    SplitEdge(scores[0], vertices[0], scores[2], vertices[2], cell + 1),
                    SplitEdge(scores[0], vertices[0], scores[2], vertices[2], cell)),
                0x20 => SendTriangle(
                    consumer,
                    SplitEdge(scores[1], vertices[1], scores[2], vertices[2], cell),
                    vertices[2],
                    SplitEdge(scores[2], vertices[2], scores[0], vertices[0], cell)),
                0x21 => SendQuad(
                    consumer,
                    SplitEdge(scores[1], vertices[1], scores[2], vertices[2], cell),
                    SplitEdge(scores[1], vertices[1], scores[2], vertices[2], cell + 1),
                    SplitEdge(scores[2], vertices[2], scores[0], vertices[0], cell + 1),
                    SplitEdge(scores[2], vertices[2], scores[0], vertices[0], cell)),
                _ => Direct3D9Factory.BadNumberHResult
            };

            if (result < 0)
            {
                return result;
            }
        }

        return Direct3D9Factory.SuccessHResult;
    }

    private float Score(Direct3D9WafflePoint point)
    {
        return point.X * _a + point.Y * _b + _c;
    }

    private static void SortByScore(Direct3D9WafflePoint[] vertices, float[] scores)
    {
        SwapIfOutOfOrder(vertices, scores, 0, 1);
        SwapIfOutOfOrder(vertices, scores, 1, 2);
        SwapIfOutOfOrder(vertices, scores, 0, 1);
    }

    private static void SwapIfOutOfOrder(
        Direct3D9WafflePoint[] vertices,
        float[] scores,
        int first,
        int second)
    {
        if (scores[second] >= scores[first])
        {
            return;
        }

        (scores[first], scores[second]) = (scores[second], scores[first]);
        (vertices[first], vertices[second]) = (vertices[second], vertices[first]);
    }

    private static int SendTriangle(
        Direct3D9AddWaffleTriangle consumer,
        Direct3D9WafflePoint first,
        Direct3D9WafflePoint second,
        Direct3D9WafflePoint third)
    {
        return consumer(first, second, third);
    }

    private static int SendQuad(
        Direct3D9AddWaffleTriangle consumer,
        Direct3D9WafflePoint first,
        Direct3D9WafflePoint second,
        Direct3D9WafflePoint third,
        Direct3D9WafflePoint fourth)
    {
        int result = consumer(first, second, third);
        return result < 0 ? result : consumer(first, third, fourth);
    }

    private static int SendPentagon(
        Direct3D9AddWaffleTriangle consumer,
        Direct3D9WafflePoint first,
        Direct3D9WafflePoint second,
        Direct3D9WafflePoint third,
        Direct3D9WafflePoint fourth,
        Direct3D9WafflePoint fifth)
    {
        int result = consumer(first, second, third);
        if (result < 0)
        {
            return result;
        }

        result = consumer(first, third, fourth);
        return result < 0 ? result : consumer(first, fourth, fifth);
    }

    private static Direct3D9WafflePoint SplitEdge(
        float firstScore,
        Direct3D9WafflePoint first,
        float secondScore,
        Direct3D9WafflePoint second,
        float splitScore)
    {
        float firstDistance = splitScore - firstScore;
        float secondDistance = secondScore - splitScore;
        if (firstDistance < secondDistance)
        {
            float amount = Math.Clamp(firstDistance / (firstDistance + secondDistance), 0f, 1f);
            return Interpolate(first, second, amount);
        }

        float reverseAmount = Math.Clamp(secondDistance / (firstDistance + secondDistance), 0f, 1f);
        return Interpolate(second, first, reverseAmount);
    }

    private static Direct3D9WafflePoint Interpolate(
        Direct3D9WafflePoint first,
        Direct3D9WafflePoint second,
        float amount)
    {
        float inverseAmount = 1f - amount;
        return new Direct3D9WafflePoint(
            Math.Clamp(inverseAmount * first.X + amount * second.X, Math.Min(first.X, second.X), Math.Max(first.X, second.X)),
            Math.Clamp(inverseAmount * first.Y + amount * second.Y, Math.Min(first.Y, second.Y), Math.Max(first.Y, second.Y)),
            Math.Clamp(inverseAmount * first.Alpha + amount * second.Alpha, Math.Min(first.Alpha, second.Alpha), Math.Max(first.Alpha, second.Alpha)));
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
