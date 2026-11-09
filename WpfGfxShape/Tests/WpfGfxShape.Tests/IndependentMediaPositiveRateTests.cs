using System.Diagnostics;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Checks the direct COM contract, not WPF SpeedRatio coercion.</summary>
    [DataTestMethod]
    [DataRow(-1.0)]
    [DataRow(double.NaN)]
    [DataRow(double.PositiveInfinity)]
    [DataRow(double.NegativeInfinity)]
    public void WhenIndependentMediaHasNoSourceThenDirectComRateTargetIsAccepted(double rate)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try { Assert.AreEqual(0, IdentityRate(media.Pointer, rate)); }
        finally { ReleaseContinuousSeekMedia(media); }
    }

    /// <summary>Real MF extreme-rate waits remain interruptible; private clock checks are managed state checks, not Native evidence.</summary>
    [DataTestMethod]
    [DataRow(double.Epsilon)]
    [DataRow(double.MaxValue)]
    public async Task WhenIndependentMediaFiniteExtremeRateRunsThenPauseAndReopenRemainBounded(double rate)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "extreme-rate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            IndependentVideoPlaybackOwner owner = media.Owner!;
            var rateField = typeof(IndependentVideoPlaybackOwner).GetField("_rate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            Assert.AreEqual(1.0, rateField.GetValue(owner));
            Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            await owner.SetPlayingAsync(false).WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(rate, rateField.GetValue(owner));
            var scale = typeof(IndependentVideoPlaybackOwner).GetMethod("ScaleElapsed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            Assert.AreEqual(rate == double.MaxValue ? long.MaxValue : 0L, scale.Invoke(owner, [long.MaxValue]));
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(1.0, rateField.GetValue(media.Owner));
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>Exercises real MF frames and COM rate/position with bounded properties, not exact wall-clock equality.</summary>
    [DataTestMethod]
    [DataRow(0.5)]
    [DataRow(2.0)]
    public async Task WhenIndependentMediaPositiveRateRunsThenClockFramesPauseAndSeekAgree(double rate)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "positive-rate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            IndependentVideoPlaybackOwner owner = media.Owner!;
            TaskCompletionSource observed = new(TaskCreationOptions.RunContinuationsAsynchronously);
            long firstWall = Stopwatch.GetTimestamp();
            owner.FrameReady += timestamp =>
            {
                if (timestamp == 0)
                {
                    long before = Stopwatch.GetTimestamp();
                    long start = QueryContinuousSeekPosition(media);
                    long after = Stopwatch.GetTimestamp();
                    bool advanced = SpinWait.SpinUntil(() => QueryContinuousSeekPosition(media) > start + 100000, TimeSpan.FromSeconds(2));
                    long endBefore = Stopwatch.GetTimestamp();
                    long end = QueryContinuousSeekPosition(media);
                    long endAfter = Stopwatch.GetTimestamp();
                    double lower = Stopwatch.GetElapsedTime(after, endBefore).Ticks * rate;
                    double upper = Stopwatch.GetElapsedTime(before, endAfter).Ticks * rate;
                    if (!advanced || end - start < Math.Floor(lower) || end - start > Math.Ceiling(upper))
                        observed.TrySetException(new AssertFailedException($"rate={rate}, delta={end - start}, bracket=[{lower},{upper}]"));
                }
                if (timestamp < 10000000) return;
                // PTS remains the decoder timestamp. At a due frame the independent query clock
                // must have progressed too; this rejects a HRESULT-only/position-only change.
                long elapsed = Stopwatch.GetElapsedTime(firstWall).Ticks;
                long position = QueryContinuousSeekPosition(media);
                if (elapsed * rate >= timestamp && position >= timestamp)
                    observed.TrySetResult();
                else observed.TrySetException(new AssertFailedException($"rate={rate}, PTS={timestamp}, wall={elapsed}, position={position}"));
            };
            Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            await observed.Task.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            long paused = QueryContinuousSeekPosition(media);
            await owner.SetPlayingAsync(false);
            Assert.AreEqual(paused, QueryContinuousSeekPosition(media));
            Assert.AreEqual(0, SubmitContinuousSeek(media, 20000000));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(20000000L, QueryContinuousSeekPosition(media));
            Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            await WaitForPositionPropertyAsync(() => QueryContinuousSeekPosition(media) > 20000000);
            Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(IndependentMediaState.Paused, media.State);
            Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            await WaitForPositionPropertyAsync(() => media.State == IndependentMediaState.Ended);
            Assert.AreEqual(IndependentMediaState.Ended, media.State);
            Assert.AreEqual(owner.Duration100ns, QueryContinuousSeekPosition(media));
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(0L, QueryContinuousSeekPosition(media));
            Assert.AreNotSame(owner, media.Owner);
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }
}
