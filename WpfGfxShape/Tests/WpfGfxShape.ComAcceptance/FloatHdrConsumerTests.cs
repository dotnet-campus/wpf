using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class FloatHdrConsumerTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [DataRow(17, 0)]
    [DataRow(17, 3)]
    [DataRow(18, 0)]
    [DataRow(18, 3)]
    public void WhenDrawingImageUsesHdrColorThenWholeFloatSurfacePreservesLinearColorAndMaskAlpha(int source, int affine)
    {
        Vector4 color = new(1.25f, -0.25f, 0.1234567f, 0.5f);
        var observations = new List<object>();
        foreach (int tile in new[] { 0, 1, 2, 3, 4 })
        foreach (uint scaling in new[] { 1u, 2u, 3u })
        foreach (int consumer in new[] { 0, 1, 2 })
        {
            Vector4[] basis = Render(Vector4.One with { W = color.W });
            Vector4[] actual = Render(color);
            observations.Add(new
            {
                Source = source, Affine = affine, Tile = tile, Scaling = scaling, Consumer = consumer,
                Basis = basis.Select(Channels).ToArray(), Actual = actual.Select(Channels).ToArray(),
                RawFloatBuffer = Convert.ToHexString(System.Runtime.InteropServices.MemoryMarshal.AsBytes(actual.AsSpan()))
            });
            for (int index = 0; index < actual.Length; index++)
            {
                Vector4 expected = consumer == 2 ? basis[index]
                    : new(basis[index].X * color.X, basis[index].Y * color.Y, basis[index].Z * color.Z, basis[index].W);
                Assert.IsTrue(Vector4.Distance(expected, actual[index]) < 0.0000001f,
                    $"source={source}, affine={affine}, tile={tile}, scaling={scaling}, consumer={consumer}, pixel={index}: {expected} != {actual[index]}");
            }

            Vector4[] Render(Vector4 input)
            {
                Vector4[] result = [];
                new CompositionTests().WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(
                    0, false, 0, 0, curves: true, drawingImage: true, affineMode: affine,
                    sourceMode: source, tileMode: tile, floating: true, scalingMode: scaling,
                    consumerMode: consumer, observeSurface: pixels => result = pixels, sourceColor: input);
                return result;
            }
        }
        string report = Path.Combine(AcceptancePaths.Results, $"float-hdr-consumers-{source}-{affine}.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.AotDll))),
            Target = "System WIC Prgba128Float bitmap, MILFactoryCreateSWRenderTargetForBitmap, IWICBitmapLock",
            PixelFormat = "6fddc324-4e03-4bfe-b185-3d77768dc91a", Width = 4, Height = 4,
            Oracle = "Independent linear color-source relation on complete surfaces; alpha-only mask. Not original same-format golden.",
            Input = Channels(color), Observations = observations
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.AddResultFile(report);
    }

    private static float[] Channels(Vector4 value) => [value.X, value.Y, value.Z, value.W];
}
