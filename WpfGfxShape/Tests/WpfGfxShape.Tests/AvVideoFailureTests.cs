using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class AvVideoFailureTests
{
    [TestMethod]
    public void WhenSurfaceAcquisitionFailsThenCompositionReturnsFailure()
    {
        Source source = new() { AcquisitionResult = Direct3D9Factory.GenericFailureHResult };
        GeneratedMediaPlayerResource video = new(source);
        Assert.AreEqual(0, video.ProcessUpdate(source, true));

        int result = video.BeginComposition(false, out _);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenRendererBeginFailsThenCompositionReturnsFailure()
    {
        Source source = new() { BeginResult = Direct3D9Factory.GenericFailureHResult };
        GeneratedMediaPlayerResource video = new(source);
        Assert.AreEqual(0, video.ProcessUpdate(source, true));

        int result = video.BeginComposition(false, out _);

        Assert.AreEqual(Direct3D9Factory.GenericFailureHResult, result);
    }

    [TestMethod]
    public void WhenRendererEndFailsThenCompositionReturnsFailureAndReleasesReference()
    {
        Source source = new() { EndResult = Direct3D9Factory.GenericFailureHResult };
        GeneratedMediaPlayerResource video = new(source);
        Assert.AreEqual(0, video.ProcessUpdate(source, true));
        Assert.AreEqual(0, video.BeginComposition(false, out _));
        int releasesBeforeEnd = source.Releases;

        int result = video.EndComposition();

        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, releasesBeforeEnd + 1), (result, source.Releases));
    }

    private sealed class Source : IVideoMediaSource, IVideoSurfaceRendererProvider, IVideoSurfaceRenderer, IVideoCompositionOwner
    {
        internal int AcquisitionResult { get; init; }
        internal int BeginResult { get; init; }
        internal int EndResult { get; init; }
        internal int Releases { get; private set; }

        public int QuerySurfaceRendererProvider(out IVideoSurfaceRendererProvider? provider)
        {
            provider = this;
            return 0;
        }

        public int GetSurfaceRenderer(out IVideoSurfaceRenderer? renderer)
        {
            renderer = this;
            return AcquisitionResult;
        }

        public int RegisterResource(GeneratedMediaPlayerResource video) => 0;
        public int UnregisterResource(GeneratedMediaPlayerResource video) => 0;
        public int RegisterVideo(GeneratedMediaPlayerResource video) => 0;
        public void UnregisterVideo(GeneratedMediaPlayerResource video) { }
        public void ScheduleCompositionPass() { }
        public void AddRef() { }
        public void Release() => Releases++;
        public int BeginComposition(GeneratedMediaPlayerResource caller, bool displaySetChanged, bool syncChannel,
            ref long lastCompositionSampleTime, out bool newFrame)
        {
            newFrame = true;
            return BeginResult;
        }

        public int EndComposition(GeneratedMediaPlayerResource caller) => EndResult;
    }
}
