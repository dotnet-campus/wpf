using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class JlPresentInvalidArgumentTests
{
    /// <summary>Retains original pixel and cleanup assertions for bounded safe-fill siblings.</summary>
    [TestMethod]
    [DataRow(7, 0, 1d)]
    [DataRow(8, 0, 1d)]
    [DataRow(7, 4, 1d)]
    [DataRow(1, 0, 1d)]
    [DataRow(2, 3, 1d)]
    [DataRow(1, 0, 1e8)]
    public void WhenSafeFillRectangleIsClippedThenCompositionPixelsRemainExact(int sourceMode, int tileMode, double scale)
    {
        new CompositionTests().WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(
            0, false, 0xffff0000u, 0xffff0000u, coordinateScale: scale,
            curves: true, drawingImage: true, sourceMode: sourceMode, tileMode: tileMode);
    }

#if JL_PRESENT_INVENTORY
    /// <summary>Retains exact full-run identities without counting nested role results.</summary>
    [TestMethod]
    public void WhenFullFailuresAreReadThenPresentInvalidArgumentsAreListedExactly()
    {
        var report = new StringBuilder();
        foreach (string arch in new[] { "x64", "x86" })
        {
            string path = Path.Combine(AppContext.BaseDirectory, "inputs", arch + ".trx");
            XDocument document = XDocument.Load(path);
            XElement root = document.Root ?? throw new InvalidDataException(path);
            XNamespace ns = root.Name.Namespace;
            XElement results = root.Element(ns + "Results") ?? throw new InvalidDataException(path);
            XElement[] failures = results.Elements(ns + "UnitTestResult")
                .Where(result => (string?)result.Attribute("outcome") == "Failed").ToArray();
            Assert.HasCount(93, failures);
            XElement[] present = failures.Where(result =>
            {
                string message = result.Descendants(ns + "Message").FirstOrDefault()?.Value ?? "";
                return message.Contains("-2147024809", StringComparison.Ordinal)
                    && message.Contains("present(connection)", StringComparison.Ordinal);
            }).ToArray();
            report.AppendLine($"## {arch}: {present.Length} / 93; input SHA256 {Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}");
            foreach (XElement result in present)
            {
                report.AppendLine($"- {result.Attribute("testId")?.Value} | {result.Attribute("testName")?.Value}");
                report.AppendLine(result.Descendants(ns + "Message").First().Value);
                report.AppendLine(result.Descendants(ns + "StackTrace").FirstOrDefault()?.Value);
            }
        }
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, "jl-present-inventory.md"), report.ToString());
    }

#endif

#if JO_PRESENT_REPRO
    /// <summary>Reproduces the legal large-viewbox composition with the unchanged pixel assertions.</summary>
    [TestMethod]
    public void WhenCandidateDrawingImageUsesLargeViewboxThenSynchronousCompositionSucceeds()
    {
        try
        {
            new CompositionTests().WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(
                0, false, 4294901760, 4294901760, curves: true, drawingImage: true, sourceMode: 7);
        }
        finally
        {
            ProcessModule actual = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                .Single(item => string.Equals(Path.GetFileName(item.FileName), "wpfgfx_cor3.dll", StringComparison.OrdinalIgnoreCase));
            string test = typeof(JlPresentInvalidArgumentTests).Assembly.Location;
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, "jp-actual-identity.txt"),
                $"Actual={actual.FileName}\nSHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual.FileName)))}\nVersion={actual.FileVersionInfo.FileVersion}\nArchitecture={RuntimeInformation.ProcessArchitecture}\nRuntime={RuntimeInformation.FrameworkDescription}\nTestSHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(test)))}\nTestMVID={typeof(JlPresentInvalidArgumentTests).Assembly.ManifestModule.ModuleVersionId}\nBase={AppContext.BaseDirectory}\nPID={Environment.ProcessId}\n");
        }
    }
#endif

#if JL_PRESENT_OFFICIAL
    /// <summary>Uses the independent official module for a legal synchronous bitmap composition.</summary>
    [TestMethod]
    public void WhenOfficialDrawingImageUsesLargeViewboxThenSynchronousCompositionSucceeds()
    {
        string? previous = Environment.GetEnvironmentVariable("WPFGFX_CANDIDATE_DLL");
        string official = AcceptancePaths.OriginalDll;
        nint module = NativeLibrary.Load(official);
        ProcessModule actual = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
            .Single(item => item.BaseAddress == module);
        File.WriteAllText(Path.Combine(AcceptancePaths.Results, "jl-official-identity.txt"),
            $"Actual={actual.FileName}\nSHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual.FileName)))}\nVersion={actual.FileVersionInfo.FileVersion}\nArchitecture={RuntimeInformation.ProcessArchitecture}\nRuntime={RuntimeInformation.FrameworkDescription}\nTestMVID={typeof(JlPresentInvalidArgumentTests).Assembly.ManifestModule.ModuleVersionId}\n");
        try
        {
            Environment.SetEnvironmentVariable("WPFGFX_CANDIDATE_DLL", official);
            new CompositionTests().WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(
                0, false, 4294901760, 4294901760, curves: true, drawingImage: true, sourceMode: 7);
        }
        finally
        {
            Environment.SetEnvironmentVariable("WPFGFX_CANDIDATE_DLL", previous);
        }
    }
#endif
}
