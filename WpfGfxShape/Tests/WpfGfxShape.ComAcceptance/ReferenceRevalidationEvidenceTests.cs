using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class ReferenceRevalidationEvidenceTests
{
    /// <summary>Records the consumed independent references and actual package identities without changing pixel expectations.</summary>
    [TestMethod]
    public void WhenGeneratedOfficialReferencesAreConsumedThenTheirModuleIdentityMatchesThePackage()
    {
        static object Identity(string path) => new
        {
            Path = path,
            Sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
            Version = FileVersionInfo.GetVersionInfo(path).FileVersion,
            ProductVersion = FileVersionInfo.GetVersionInfo(path).ProductVersion
        };
        string root = Path.Combine(AppContext.BaseDirectory, "ReferenceData");
        string glyphPath = Path.Combine(root, "glyph-arial-original.json");
        string drawingPath = Path.Combine(root, "original-wpf.json");
        using var glyph = JsonDocument.Parse(File.ReadAllText(glyphPath));
        using var drawing = JsonDocument.Parse(File.ReadAllText(drawingPath));
        string officialSha = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(AcceptancePaths.OriginalDll)));
        var masks = drawing.RootElement.GetProperty("Samples").EnumerateArray().Where(sample =>
            sample.GetProperty("TileMode").GetString() == "Tile"
            && sample.GetProperty("ScalingMode").GetString() == "LowQuality"
            && sample.GetProperty("Consumer").GetString() == "mask"
            && ((sample.GetProperty("SourceMode").GetInt32() == 17
                    && sample.GetProperty("AffineMode").GetInt32() is 0 or 3)
                || (sample.GetProperty("SourceMode").GetInt32() == 18
                    && sample.GetProperty("AffineMode").GetInt32() == 3))).ToArray();
        var evidence = new
        {
            Runtime = RuntimeInformation.FrameworkDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
            PointerSize = IntPtr.Size,
            BaseDirectory = AppContext.BaseDirectory,
            Candidate = Identity(AcceptancePaths.AotDll),
            Official = Identity(AcceptancePaths.OriginalDll),
            TestAssembly = Identity(typeof(ReferenceRevalidationEvidenceTests).Assembly.Location),
            TestAssemblyMvid = typeof(ReferenceRevalidationEvidenceTests).Module.ModuleVersionId,
            PresentationCore = Identity(typeof(System.Windows.Media.Visual).Assembly.Location),
            GlyphReference = Identity(glyphPath),
            DrawingReference = Identity(drawingPath),
            GlyphRaw = glyph.RootElement,
            DrawingHeader = new
            {
                Runtime = drawing.RootElement.GetProperty("Runtime"),
                Architecture = drawing.RootElement.GetProperty("Architecture"),
                NativeDll = drawing.RootElement.GetProperty("NativeDll"),
                NativeSha256 = drawing.RootElement.GetProperty("NativeSha256")
            },
            ConsumedMaskSamples = masks
        };
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, "az-reference-consumption.json"),
            JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
        CollectionAssert.AreEqual(new[] { officialSha, officialSha }, new[]
        {
            glyph.RootElement.GetProperty("OriginalLibrarySha256").GetString(),
            drawing.RootElement.GetProperty("NativeSha256").GetString()
        });
    }
}
