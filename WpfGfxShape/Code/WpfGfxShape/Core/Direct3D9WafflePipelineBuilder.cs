using System.Numerics;

namespace WpfGfxShape.Core;

[Flags]
internal enum Direct3D9WaffleMode
{
    None = 0,
    Enabled = 1,
    FlipX = 2,
    FlipY = 4,
}

internal readonly record struct Direct3D9WaffleTextureSubrect(
    float X,
    float Y,
    float Width,
    float Height);

internal readonly record struct Direct3D9WaffleTextureCoordinate(
    Matrix3x2 PointToTexture,
    Direct3D9WaffleMode Mode,
    Direct3D9WaffleTextureSubrect Subrect = default);

internal readonly record struct Direct3D9LineWafflePipeline(
    Direct3D9AddWaffleLine Sink,
    bool WafflersUsed,
    int WafflerCount);

internal readonly record struct Direct3D9TriangleWafflePipeline(
    Direct3D9AddWaffleTriangle Sink,
    bool WafflersUsed,
    int WafflerCount);

internal static class Direct3D9WafflePipelineBuilder
{
    private const float MinimumWaffleWidthPixels = 0.25f;
    private const float MaximumWaffleMagnitudeSquared =
        1f / (MinimumWaffleWidthPixels * MinimumWaffleWidthPixels);

    internal static Direct3D9LineWafflePipeline BuildLine(
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates,
        Direct3D9AddWaffleLine finalSink)
    {
        ArgumentNullException.ThrowIfNull(textureCoordinates);
        ArgumentNullException.ThrowIfNull(finalSink);

        List<Direct3D9LineWaffler> wafflers = [];
        AddLineWafflers(textureCoordinates, wafflers);
        if (wafflers.Count == 0)
        {
            return new Direct3D9LineWafflePipeline(finalSink, false, 0);
        }

        for (int index = 0; index < wafflers.Count - 1; index++)
        {
            wafflers[index].SetConsumer(wafflers[index + 1].AddLine);
        }

        wafflers[^1].SetConsumer(finalSink);
        return new Direct3D9LineWafflePipeline(wafflers[0].AddLine, true, wafflers.Count);
    }

    internal static Direct3D9TriangleWafflePipeline BuildTriangle(
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates,
        Direct3D9AddWaffleTriangle finalSink)
    {
        ArgumentNullException.ThrowIfNull(textureCoordinates);
        ArgumentNullException.ThrowIfNull(finalSink);

        List<Direct3D9TriangleWaffler> wafflers = [];
        AddTriangleWafflers(textureCoordinates, wafflers);
        if (wafflers.Count == 0)
        {
            return new Direct3D9TriangleWafflePipeline(finalSink, false, 0);
        }

        for (int index = 0; index < wafflers.Count - 1; index++)
        {
            wafflers[index].SetConsumer(wafflers[index + 1].AddTriangle);
        }

        wafflers[^1].SetConsumer(finalSink);
        return new Direct3D9TriangleWafflePipeline(wafflers[0].AddTriangle, true, wafflers.Count);
    }

    private static void AddLineWafflers(
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates,
        List<Direct3D9LineWaffler> wafflers)
    {
        foreach (Direct3D9WaffleTextureCoordinate coordinate in textureCoordinates)
        {
            if (coordinate.Mode == Direct3D9WaffleMode.None)
            {
                continue;
            }

            Matrix3x2 matrix = coordinate.PointToTexture;
            AddLineWaffler(matrix.M11, matrix.M21, matrix.M31, wafflers);
            AddLineWaffler(matrix.M12, matrix.M22, matrix.M32, wafflers);
        }
    }

    private static void AddTriangleWafflers(
        IReadOnlyList<Direct3D9WaffleTextureCoordinate> textureCoordinates,
        List<Direct3D9TriangleWaffler> wafflers)
    {
        foreach (Direct3D9WaffleTextureCoordinate coordinate in textureCoordinates)
        {
            if (coordinate.Mode == Direct3D9WaffleMode.None)
            {
                continue;
            }

            Matrix3x2 matrix = coordinate.PointToTexture;
            AddTriangleWaffler(matrix.M11, matrix.M21, matrix.M31, wafflers);
            AddTriangleWaffler(matrix.M12, matrix.M22, matrix.M32, wafflers);
        }
    }

    private static void AddLineWaffler(
        float a,
        float b,
        float c,
        List<Direct3D9LineWaffler> wafflers)
    {
        if (a * a + b * b >= MaximumWaffleMagnitudeSquared)
        {
            return;
        }

        Direct3D9LineWaffler waffler = new();
        waffler.Set(a, b, c, static (_, _) => 0);
        wafflers.Add(waffler);
    }

    private static void AddTriangleWaffler(
        float a,
        float b,
        float c,
        List<Direct3D9TriangleWaffler> wafflers)
    {
        if (a * a + b * b >= MaximumWaffleMagnitudeSquared)
        {
            return;
        }

        Direct3D9TriangleWaffler waffler = new();
        waffler.Set(a, b, c, static (_, _, _) => 0);
        wafflers.Add(waffler);
    }
}
