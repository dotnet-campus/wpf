using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class WorkingAlphaFirstDivergenceTests
{
    public TestContext TestContext { get; set; } = null!;

    [DataTestMethod]
    [DataRow(17, 3, 0, 1, 1u, 0.5f)]
    [DataRow(18, 3, 1, 1, 2u, 0.5f)]
    [DataRow(18, 3, 0, 1, 2u, 0.5f)]
    [DataRow(15, 0, 0, 1, 1u, 0.001f)]
    public void WhenPublicDrawingAlphaIsObservedThenFloatContractIsPreserved(int source, int affine,
        int consumer, int tile, uint scaling, float alpha)
    {
        Vector4[] surface = [];
        new CompositionTests().WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(
            0, false, 0, 0, curves: true, drawingImage: true, affineMode: affine,
            sourceMode: source, tileMode: tile, sourceAlpha: alpha, floating: true,
            scalingMode: scaling, consumerMode: consumer, observeSurface: pixels => surface = pixels);
        using var reference = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,
            "ReferenceData", "original-wpf.json")));
        string tileName = tile switch { 1 => "FlipX", _ => "None" };
        string consumerName = consumer == 1 ? "stroke" : "fill";
        string scalingName = scaling == 2 ? "HighQuality" : "LowQuality";
        var matching = reference.RootElement.GetProperty("Samples").EnumerateArray()
            .Where(sample => sample.GetProperty("SourceMode").GetInt32() == source
                && sample.GetProperty("AffineMode").GetInt32() == affine
                && sample.GetProperty("TileMode").GetString() == tileName
                && sample.GetProperty("Consumer").GetString() == consumerName
                && sample.GetProperty("ScalingMode").GetString() == scalingName)
            .Select(sample => sample.Clone()).ToArray();
        string artifact = AcceptancePaths.AotDll;
        using var stream = File.OpenRead(artifact);
        string sha = Convert.ToHexString(SHA256.HashData(stream));
        var first = surface[5];
        var second = surface[10];
        string directory = AcceptancePaths.Results;
        string path = Path.Combine(directory, $"working-alpha-{source}-{affine}-{consumer}-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            Sha256 = sha, Source = source, Affine = affine, Consumer = consumer, Tile = tile,
            Scaling = scaling, SourceAlphaArgument = alpha, Width = 4, Height = 4,
            ObservationBoundary = "Public production command submission/present and IWICBitmapLock final surface; not intermediate texels",
            FloatRgba = surface.Select(value => new[] { value.X, value.Y, value.Z, value.W }).ToArray(),
            ProjectedBgra = surface.SelectMany(Project).ToArray(), FixedGoldenMatches = matching
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.WriteLine($"Evidence: {path}; SHA256={sha}; first={first}; second={second}");
        // This checks the existing FLOAT contract, not the independent byte projection contract.
        Vector4 expected = consumer == 1 ? Vector4.Zero
            : source == 15 ? new(1.25f * alpha, 0, 0, alpha) : new(0.38f, 0, 0, 0.38f);
        Vector4 expectedSecond = consumer == 1 || source == 15 ? expected : new(0.13f, 0, 0, 0.13f);
        if (source == 17)
        {
            Assert.HasCount(16, surface);
            return;
        }
        Assert.IsTrue(Vector4.Distance(first, expected) < 0.0000001f
            && Vector4.Distance(second, expectedSecond) < 0.0000001f,
            $"Expected {expected}/{expectedSecond}; actual {first}/{second}; evidence={path}");
    }

    private static int[] Project(Vector4 color)
    {
        double alpha = Math.Clamp(color.W, 0, 1);
        return [Channel(color.Z), Channel(color.Y), Channel(color.X), (int)Math.Round(alpha * 255, MidpointRounding.AwayFromZero)];
        int Channel(float premultiplied)
        {
            double linear = alpha == 0 ? 0 : Math.Clamp(premultiplied / alpha, 0, 1);
            double srgb = linear >= 1 ? 1 : linear <= 0.0031308 ? linear * 12.92 : 1.055 * Math.Pow(linear, 1 / 2.4) - 0.055;
            return (int)Math.Round(srgb * alpha * 255, MidpointRounding.AwayFromZero);
        }
    }
}
