using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    /// <summary>Records actual candidate identity for historical geometry and two-sampler updates.</summary>
    [TestMethod]
    [DataRow(true, false, 0xff00ff00u, 0x0000ff00u)]
    [DataRow(true, false, 0xff0000ffu, 0x00ff0000u)]
    [DataRow(false, false, 0xff00ff00u, 0x0000ff80u)]
    [DataRow(false, true, 0xff0000ffu, 0x00ff0080u)]
    [DataRow(false, true, 0u, 0x00000080u)]
    public void WhenHistoricalCaptureCandidateRunsThenExpectedUpdatesRemain(bool geometry, bool cache, uint color, uint expected)
        => RunHistoricalCaptureControl(false, geometry, cache, color, expected);

    /// <summary>Runs official geometry separately so an ABI failure cannot abort candidate regressions.</summary>
    [TestMethod]
    public Task WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain()
        => HistoricalProcessIsolation.RunAsync(nameof(WhenHistoricalCaptureOfficialGeometryRunsThenExpectedUpdatesRemain),
            () => RunHistoricalCaptureControl(true, true, false, 0xff00ff00u, 0x0000ff00u));

    /// <summary>Runs the official two-sampler scene in its own filtered process.</summary>
    [TestMethod]
    public Task WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain()
        => HistoricalProcessIsolation.RunAsync(nameof(WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain),
            () => RunHistoricalCaptureControl(true, false, false, 0xff00ff00u, 0x0000ff80u));

    private static void RunHistoricalCaptureControl(bool official, bool geometry, bool cache, uint color, uint expected)
    {
        string path = official ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        ExitDiagnostics.Stage($"historical load before official={official} geometry={geometry} cache={cache} color={color:X8} expected={expected:X8} path={path}");
        nint module = NativeLibrary.Load(path);
        ExitDiagnostics.Stage("historical load after");
        LogHardwareModuleIdentity(module, path);
        Console.WriteLine($"HistoricalControl official={official} geometry={geometry} cache={cache} color=0x{color:X8} expected=0x{expected:X8}");
        ExitDiagnostics.Stage("historical consumer before");
        RunVisualCacheConsumer(geometry || cache, color, expected, geometry, official: official);
        ExitDiagnostics.Stage("historical consumer after");
    }
}
