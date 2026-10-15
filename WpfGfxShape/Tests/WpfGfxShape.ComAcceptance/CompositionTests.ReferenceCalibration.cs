using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionTests
{
    [TestMethod]
    public void WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements()
    {
        string referencePath = Path.Combine(AppContext.BaseDirectory, "ReferenceData", "original-wpf.json");
        using JsonDocument reference = JsonDocument.Parse(File.ReadAllText(referencePath));
        var results = new List<object>();
        var differences = new List<string>();
        var crossFormatDifferences = new List<string>();
        int mismatches = 0, directCompared = 0, directMismatches = 0, formatVerdictDifferences = 0;
        int controlCompared = 0, controlMismatches = 0;
        int bitmapCompared = 0, bitmapSourceDifferences = 0, aotMatchesBitmapOnly = 0, aotBitmapMismatches = 0;
        var bitmapGroups = new Dictionary<string, (int Compared, int Mismatches)>();
        var controlGroups = new Dictionary<string, (int Compared, int Mismatches)>();
        var groups = new Dictionary<string, (int Compared, int Mismatches)>();
        foreach (JsonElement sample in reference.RootElement.GetProperty("Samples").EnumerateArray())
        {
            int sourceMode = sample.GetProperty("SourceMode").GetInt32();
            int affineMode = sample.GetProperty("AffineMode").GetInt32();
            string tile = sample.GetProperty("TileMode").GetString()!;
            string scaling = sample.GetProperty("ScalingMode").GetString()!;
            string consumer = sample.GetProperty("Consumer").GetString()!;
            int tileMode = tile switch { "None" => 0, "FlipX" => 1, "FlipY" => 2, "FlipXY" => 3, "Tile" => 4, _ => throw new InvalidDataException(tile) };
            uint scalingMode = scaling switch { "LowQuality" => 1, "HighQuality" => 2, "NearestNeighbor" => 3, _ => throw new InvalidDataException(scaling) };
            int consumerMode = consumer switch { "fill" => 0, "stroke" => 1, "mask" => 2, _ => throw new InvalidDataException(consumer) };
            uint requestedScalingMode = scalingMode;
            // The independent padded-source probe proves Visual masks remain linear for all three labels.
            if (consumerMode == 2) scalingMode = 1;
            double[] targetGeometry = sourceMode == 18 ? [-10, -10, 20, 20]
                : consumerMode == 1 ? [1.5, 1.5, 1, 1] : [0, 0, 4, 4];
            CollectionAssert.AreEqual(targetGeometry, sample.GetProperty("TargetGeometry").EnumerateArray().Select(value => value.GetDouble()).ToArray());
            Assert.AreEqual(consumerMode == 1 ? 2 : 0, sample.GetProperty("PenThickness").GetInt32());
            Vector4[] surface = [];
            WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
                curves: true, drawingImage: true, affineMode: affineMode, sourceMode: sourceMode, tileMode: tileMode,
                sourceAlpha: 0.5f, floating: true, scalingMode: scalingMode, consumerMode: consumerMode,
                observeSurface: pixels => surface = pixels);
            int[] original = sample.GetProperty("PixelsBgra").EnumerateArray().Select(value => value.GetInt32()).ToArray();
            Assert.HasCount(16, surface);
            Assert.HasCount(64, original);
            int[] projected = surface.SelectMany(ProjectToPbgra32).ToArray();
            int[]? direct = null;
            bool? directMatches = null;
            {
                byte[] directSurface = [];
                WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
                    curves: true, drawingImage: true, affineMode: affineMode, sourceMode: sourceMode, tileMode: tileMode,
                    sourceAlpha: 0.5f, floating: false, scalingMode: scalingMode, consumerMode: consumerMode,
                    observeBgraSurface: pixels => directSurface = pixels);
                Assert.HasCount(64, directSurface);
                direct = directSurface.Select(value => (int)value).ToArray();
                directMatches = original.SequenceEqual(direct);
                directCompared++;
                if (directMatches == false)
                {
                    directMismatches++;
                    differences.Add($"source={sourceMode}, affine={affineMode}, tile={tile}, scaling={scaling}, consumer={consumer}: direct Pbgra32 differs from original WPF");
                    for (int pixel = 0; pixel < 16; pixel++)
                    {
                        int offset = pixel * 4;
                        if (!original.AsSpan(offset, 4).SequenceEqual(direct.AsSpan(offset, 4)))
                            differences.Add($"  direct ({pixel % 4},{pixel / 4}): original [{string.Join(',', original.AsSpan(offset, 4).ToArray())}], AOT [{string.Join(',', direct.AsSpan(offset, 4).ToArray())}]");
                    }
                }
            }
            int[]? controlOriginal = null, controlActual = null;
            bool? controlMatches = null;
            if (sourceMode is 17 or 18)
            {
                controlOriginal = sample.GetProperty("SolidTargetControlPixelsBgra").EnumerateArray().Select(value => value.GetInt32()).ToArray();
                byte[] controlSurface = [];
                WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
                    curves: true, drawingImage: true, affineMode: affineMode, sourceMode: sourceMode, tileMode: tileMode,
                    floating: false, scalingMode: scalingMode, consumerMode: consumerMode,
                    observeBgraSurface: pixels => controlSurface = pixels, solidTargetControl: true);
                Assert.HasCount(64, controlSurface);
                Assert.HasCount(64, controlOriginal);
                controlActual = controlSurface.Select(value => (int)value).ToArray();
                controlMatches = controlOriginal.SequenceEqual(controlActual);
                controlCompared++;
                if (controlMatches == false) controlMismatches++;
                string controlKey = $"source={sourceMode}, affine={affineMode}, consumer={consumer}";
                controlGroups.TryGetValue(controlKey, out var controlGroup);
                controlGroups[controlKey] = (controlGroup.Compared + 1, controlGroup.Mismatches + (controlMatches == false ? 1 : 0));
            }
            bool matches = original.SequenceEqual(projected);
            if (directMatches.HasValue && directMatches.Value != matches) formatVerdictDifferences++;
            string groupKey = $"source={sourceMode}, affine={affineMode}";
            groups.TryGetValue(groupKey, out var group);
            groups[groupKey] = (group.Compared + 1, group.Mismatches + (matches ? 0 : 1));
            string key = $"source={sourceMode}, affine={affineMode}, tile={tile}, scaling={scaling}, consumer={consumer}";
            if (controlMatches == false) differences.Add($"{key}: solid target geometry coverage differs from original WPF");
            if (directMatches == false) differences.Add($"{key}: direct Pbgra32 differs from original WPF");
            if (!matches)
            {
                mismatches++;
                crossFormatDifferences.Add(key);
                for (int pixel = 0; pixel < surface.Length; pixel++)
                {
                    int offset = pixel * 4;
                    if (!original.AsSpan(offset, 4).SequenceEqual(projected.AsSpan(offset, 4)))
                        crossFormatDifferences.Add($"  ({pixel % 4},{pixel / 4}): original [{string.Join(',', original[offset..(offset + 4)])}], projected AOT [{string.Join(',', projected[offset..(offset + 4)])}]");
                }
            }
            int[]? bitmapControl = null, aotBitmap = null;
            bool? aotBitmapMatches = null;
            bool? bitmapMatchesDrawing = null, aotMatchesBitmap = null;
            if (sourceMode is 17 or 18)
            {
                bitmapControl = sample.GetProperty("BitmapSourceControlPixelsBgra").EnumerateArray().Select(value => value.GetInt32()).ToArray();
                Assert.HasCount(64, bitmapControl);
                bitmapMatchesDrawing = bitmapControl.SequenceEqual(original);
                aotMatchesBitmap = bitmapControl.SequenceEqual(direct!);
                bitmapCompared++;
                if (bitmapMatchesDrawing == false) bitmapSourceDifferences++;
                if (aotMatchesBitmap == true && directMatches == false) aotMatchesBitmapOnly++;
                byte[] bitmapSurface = [];
                WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(0, false, 0, 0,
                    curves: true, drawingImage: true, affineMode: affineMode, sourceMode: sourceMode, tileMode: tileMode,
                    floating: false, scalingMode: scalingMode, consumerMode: consumerMode,
                    observeBgraSurface: pixels => bitmapSurface = pixels, bitmapSourceControl: true);
                Assert.HasCount(64, bitmapSurface);
                aotBitmap = bitmapSurface.Select(value => (int)value).ToArray();
                aotBitmapMatches = bitmapControl.SequenceEqual(aotBitmap);
                string bitmapKey = $"source={sourceMode}, affine={affineMode}, tile={tile}, scaling={scaling}";
                bitmapGroups.TryGetValue(bitmapKey, out var bitmapGroup);
                bitmapGroups[bitmapKey] = (bitmapGroup.Compared + 1, bitmapGroup.Mismatches + (aotBitmapMatches == false ? 1 : 0));
                if (aotBitmapMatches == false)
                {
                    aotBitmapMismatches++;
                    differences.Add($"{key}: AOT BitmapSource differs from original BitmapSource");
                }
            }
            results.Add(new
            {
                Key = key,
                RequestedScalingMode = requestedScalingMode,
                EffectiveScalingMode = scalingMode,
                ScalingEvidence = consumerMode == 2 ? "padding-consumer-scopes-4e32fcd742d343a7a22fca8a78643081.json: VisualMask remains linear" : "DrawingGroup content",
                Matches = matches,
                Width = 4,
                Height = 4,
                OriginalPixelsBgra = original,
                AotPixelsPrgbaFloat = surface.Select(pixel => new[] { pixel.X, pixel.Y, pixel.Z, pixel.W }).ToArray(),
                ProjectedPixelsBgra = projected,
                DirectPbgra32Matches = directMatches,
                DirectPbgra32Pixels = direct,
                SolidTargetControlMatches = controlMatches,
                OriginalSolidTargetPixels = controlOriginal,
                AotSolidTargetPixels = controlActual,
                OriginalBitmapSourcePixels = bitmapControl,
                OriginalBitmapMatchesDrawingImage = bitmapMatchesDrawing,
                AotDrawingMatchesOriginalBitmapSource = aotMatchesBitmap,
                AotBitmapSourcePixels = aotBitmap,
                AotBitmapMatchesOriginalBitmapSource = aotBitmapMatches
            });
        }
        string dll = AcceptancePaths.AotDll;
        string directory = AcceptancePaths.Results;
        Directory.CreateDirectory(directory);
        string report = Path.Combine(directory, $"wpf-calibration-{Guid.NewGuid():N}.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            OriginalDllSha256 = reference.RootElement.GetProperty("NativeSha256").GetString(),
            ReferenceFileSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(referencePath))),
            AotDllSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(dll))),
            Projection = "Unpremultiply linear RGB, clamp and convert to sRGB, premultiply alpha, round to nearest byte. Projection differences require review; no float golden is inferred.",
            Compared = results.Count,
            CrossFormatContract = "Diagnostic only: original Pbgra32 rendering is not a Prgba128Float rendering oracle.",
            CrossFormatDifferences = crossFormatDifferences,
            StrictByteTargetFormat = "6fddc324-4e03-4bfe-b185-3d77768dc910",
            DiagnosticFloatTargetFormat = "6fddc324-4e03-4bfe-b185-3d77768dc91a",
            Scope = "Complete 4x4 surface, 16 pixels per fixture",
            Mismatches = mismatches,
            DirectCompared = directCompared,
            DirectMismatches = directMismatches,
            FormatVerdictDifferences = formatVerdictDifferences,
            AotBitmapMismatches = aotBitmapMismatches,
            BitmapGroups = bitmapGroups.Select(group => new { Key = group.Key, group.Value.Compared, group.Value.Mismatches }).ToArray(),
            BitmapCompared = bitmapCompared,
            OriginalBitmapSourceDifferences = bitmapSourceDifferences,
            AotMatchesBitmapOnly = aotMatchesBitmapOnly,
            ControlCompared = controlCompared,
            ControlMismatches = controlMismatches,
            ControlGroups = controlGroups.Select(group => new { Key = group.Key, group.Value.Compared, group.Value.Mismatches }).ToArray(),
            Groups = groups.Select(group => new { Key = group.Key, group.Value.Compared, group.Value.Mismatches }).ToArray(),
            Results = results
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Calibration report: {report}");
        foreach (var difference in differences.Where(value => value.Contains("direct (", StringComparison.Ordinal)
            || value.Contains("direct Pbgra32 differs", StringComparison.Ordinal))) Console.WriteLine(difference);
        foreach (var group in groups)
            Console.WriteLine($"{group.Key}: compared={group.Value.Compared}, mismatches={group.Value.Mismatches}");
        Console.WriteLine($"Direct Pbgra32: compared={directCompared}, mismatches={directMismatches}, format verdict differences={formatVerdictDifferences}");
        Console.WriteLine($"Solid target control: compared={controlCompared}, mismatches={controlMismatches}");
        foreach (var group in controlGroups)
            Console.WriteLine($"Control {group.Key}: compared={group.Value.Compared}, mismatches={group.Value.Mismatches}");
        Console.WriteLine($"Bitmap source control: compared={bitmapCompared}, original source differences={bitmapSourceDifferences}, AOT matches bitmap only={aotMatchesBitmapOnly}");
        Console.WriteLine($"AOT BitmapSource: compared={bitmapCompared}, mismatches={aotBitmapMismatches}");
        foreach (var group in bitmapGroups)
            Console.WriteLine($"Bitmap {group.Key}: compared={group.Value.Compared}, mismatches={group.Value.Mismatches}");
        Assert.AreEqual(180, bitmapCompared);
        Assert.AreEqual(180, controlCompared);
        Assert.AreEqual(450, directCompared);
        Assert.HasCount(450, results);
        Assert.IsEmpty(differences, string.Join(Environment.NewLine, differences));
    }

    private static int[] ProjectToPbgra32(Vector4 color)
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
