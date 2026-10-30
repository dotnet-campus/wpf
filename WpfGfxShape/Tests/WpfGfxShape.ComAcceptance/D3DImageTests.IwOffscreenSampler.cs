#if IW_OFFSCREEN_SAMPLER
namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    /// <summary>Checks the independent official software bitmap consumer, without a display precondition.</summary>
    [STATestMethod]
    public void WhenIwOfficialOffscreenSamplerUpdatesThenConsumerPixelsChange()
        => RunOffscreenSampler(false, Path.Combine(NativeRun.PathsRoot, "iw-sampler"));

    /// <summary>Checks the candidate software bitmap consumer in a separate filtered process.</summary>
    [STATestMethod]
    public void WhenIwCandidateOffscreenSamplerUpdatesThenConsumerPixelsChange()
        => RunOffscreenSampler(true, Path.Combine(NativeRun.PathsRoot, "iw-sampler"));
}
#endif
