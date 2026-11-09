using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Deferred unsupported intent fails on Open, not at source-free submission.</summary>
    [DataTestMethod]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    [DataRow(double.NaN)]
    [DataRow(-1d)]
    public async Task WhenUnsupportedTargetPrecedesOpenThenApplicationFailsAsync(double rate)
    {
        var media = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            OpenContinuousSeekMedia(media, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.avi"));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual((IndependentMediaState.Failed, unchecked((int)0x80004001)), (media.State, media.Failure));
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>Pause retains the last nonzero target, including its application failure.</summary>
    [DataTestMethod]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    [DataRow(double.NaN)]
    [DataRow(-1d)]
    public async Task WhenPauseFollowsUnsupportedTargetThenOpenStillFailsApplicationAsync(double rate)
    {
        var media = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
            OpenContinuousSeekMedia(media, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.avi"));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual((IndependentMediaState.Failed, unchecked((int)0x80004001)), (media.State, media.Failure));
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>A target submitted after reusable Close belongs to the next source generation.</summary>
    [DataTestMethod]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    [DataRow(double.NaN)]
    [DataRow(-1d)]
    public async Task WhenUnsupportedTargetFollowsCloseThenNewOpenAppliesNewTargetAsync(double rate)
    {
        var media = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(0, IdentityRate(media.Pointer, 2));
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            Assert.AreEqual(IndependentMediaState.Closed, media.State);
            OpenContinuousSeekMedia(media, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.avi"));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual((IndependentMediaState.Failed, unchecked((int)0x80004001)), (media.State, media.Failure));
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>Close discards the old unsupported rate before the next source generation.</summary>
    [TestMethod]
    public async Task WhenCloseCancelsUnsupportedTargetThenNewOpenUsesResetTargetAsync()
    {
        var media = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(0, IdentityRate(media.Pointer, double.PositiveInfinity));
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            OpenContinuousSeekMedia(media, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.avi"));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreNotEqual(unchecked((int)0x80004001), media.Failure);
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }
}
