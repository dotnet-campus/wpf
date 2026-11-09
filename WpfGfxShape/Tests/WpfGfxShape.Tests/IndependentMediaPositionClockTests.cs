using System.Diagnostics;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Uses real MF and the public COM getter; waits for properties, not exact wall-clock offsets.</summary>
    [TestMethod]
    public async Task WhenIndependentMediaClockRunsThenPauseResumeAndSeekPreserveTimeline()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "position-clock-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            IndependentVideoPlaybackOwner owner = media.Owner!;
            TaskCompletionSource<(long First, long Second)> betweenFrames = new(TaskCreationOptions.RunContinuationsAsynchronously);
            owner.FrameReady += timestamp =>
            {
                if (timestamp != 0) return;
                // The owner cannot publish another frame while this bounded real-frame callback runs.
                long first = QueryContinuousSeekPosition(media);
                bool advanced = SpinWait.SpinUntil(() => QueryContinuousSeekPosition(media) > first, TimeSpan.FromSeconds(1));
                if (advanced) betweenFrames.TrySetResult((first, QueryContinuousSeekPosition(media)));
                else betweenFrames.TrySetException(new TimeoutException());
            };
            Assert.AreEqual(0, IdentityRate(media.Pointer, 1));
            var positions = await betweenFrames.Task.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.IsTrue(positions.Second > positions.First);
            Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            long paused = QueryContinuousSeekPosition(media);
            await Task.Delay(100);
            Assert.AreEqual(paused, QueryContinuousSeekPosition(media));
            Assert.AreEqual(0, IdentityRate(media.Pointer, 1));
            await WaitForPositionPropertyAsync(() => QueryContinuousSeekPosition(media) > paused);
            Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.IsTrue(QueryContinuousSeekPosition(media) >= paused);
            Assert.AreEqual(0, SubmitContinuousSeek(media, 20000000));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(20000000L, QueryContinuousSeekPosition(media));
            Assert.AreEqual(0, IdentityRate(media.Pointer, 1));
            await WaitForPositionPropertyAsync(() => QueryContinuousSeekPosition(media) > 20000000);
            Assert.IsTrue(QueryContinuousSeekPosition(media) >= 20000000);
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>Checks EOS duration and source retirement through the production vtable, without reflection interleaving.</summary>
    [DataTestMethod]
    [DataRow("close")]
    [DataRow("source")]
    [DataRow("reopen")]
    public async Task WhenIndependentMediaClockEndsThenDurationAndSourceResetArePreserved(string action)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "position-reset-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            IndependentVideoPlaybackOwner retired = media.Owner!;
            Assert.AreEqual(0, IdentityRate(media.Pointer, 1));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(IndependentMediaState.Ended, media.State);
            Assert.AreEqual(retired.Duration100ns, QueryContinuousSeekPosition(media));
            Assert.AreEqual(0, InterruptContinuousSeek(media, action));
            if (action == "reopen")
            {
                string replacement = Path.Combine(root, "replacement.avi");
                WriteSourceReaderAvi(replacement);
                OpenContinuousSeekMedia(media, replacement);
            }
            Assert.AreEqual(0L, QueryContinuousSeekPosition(media));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(0L, QueryContinuousSeekPosition(media));
            Assert.AreEqual(0L, retired.Position100ns);
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    private static async Task WaitForPositionPropertyAsync(Func<bool> property)
    {
        Stopwatch timeout = Stopwatch.StartNew();
        while (!property())
        {
            if (timeout.Elapsed > TimeSpan.FromSeconds(10)) throw new TimeoutException();
            await Task.Delay(10);
        }
    }
}
