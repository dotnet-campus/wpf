using System.Reflection;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    private static unsafe int SubmitGenerationStop(nint pointer)
        => ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)pointer)[4])(pointer);

    /// <summary>Real MF and public COM queue under the existing identity lock; no synthetic observer or Native claim.</summary>
    [DataTestMethod]
    [DataRow(0.0)]
    [DataRow(1.0)]
    [DataRow(0.5)]
    [DataRow(2.0)]
    public async Task WhenIndependentMediaControlQueuesBeforeSeekThenLatestRateOwnsSeekState(double rate)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "control-target-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            object gate = typeof(IndependentMediaIdentity).GetField("_gate", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(media)!;
            lock (gate)
            {
                Assert.AreEqual(0, IdentityRate(media.Pointer, rate == 0 ? 2 : 0));
                Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
                Assert.AreEqual(0, SubmitContinuousSeek(media, 10000000));
                Assert.AreEqual(10000000L, QueryContinuousSeekPosition(media));
            }
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(rate > 0 ? IndependentMediaState.Playing : IndependentMediaState.Paused, media.State);
            if (rate > 0) await WaitForPositionPropertyAsync(() => QueryContinuousSeekPosition(media) > 10000000);
            else Assert.AreEqual(10000000L, QueryContinuousSeekPosition(media));
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>Real MF/COM and owner commands; reflection delays an observer, not a Native continuation-frequency test.</summary>
    [DataTestMethod]
    [DataRow("resume")]
    [DataRow("half-rate")]
    [DataRow("double-rate")]
    [DataRow("pause")]
    [DataRow("seek")]
    [DataRow("stop")]
    [DataRow("ended")]
    [DataRow("failed")]
    [DataRow("close")]
    [DataRow("source")]
    [DataRow("reopen")]
    [DataRow("error")]
    public async Task WhenIndependentMediaOldControlCompletesThenNewIntentRetainsState(string action)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "control-generation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        TaskCompletionSource delayed = new();
        Task observed = Task.CompletedTask;
        try
        {
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            IndependentVideoPlaybackOwner owner = media.Owner!;
            await owner.SetPlayingAsync(false).WaitAsync(TimeSpan.FromSeconds(15));
            Type type = typeof(IndependentMediaIdentity);
            object gate = type.GetField("_gate", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(media)!;
            MethodInfo observe = type.GetMethod("ObserveControlAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
            IndependentMediaState oldState = action == "pause" ? IndependentMediaState.Playing : IndependentMediaState.Paused;
            observed = (Task)observe.Invoke(media, [owner, delayed.Task, oldState])!;
            IndependentMediaState expected;
            lock (gate)
            {
                switch (action)
                {
                    case "half-rate":
                    case "double-rate":
                        Assert.AreEqual(0, IdentityRate(media.Pointer, action == "half-rate" ? 0.5 : 2));
                        break;
                    case "resume":
                    case "pause":
                    case "error":
                        Assert.AreEqual(0, IdentityRate(media.Pointer, action == "pause" ? 0 : 1));
                        break;
                    case "seek": Assert.AreEqual(0, SubmitContinuousSeek(media, 20000000)); break;
                    case "stop": Assert.AreEqual(0, SubmitGenerationStop(media.Pointer)); break;
                    case "ended": type.GetMethod("OnEnded", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner]); break;
                    case "failed": type.GetMethod("OnFailed", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(media, [owner, unchecked((int)0x80004005)]); break;
                    default:
                        Assert.AreEqual(0, InterruptContinuousSeek(media, action));
                        if (action == "reopen")
                        {
                            string replacement = Path.Combine(root, "replacement.avi");
                            WriteSourceReaderAvi(replacement);
                            OpenContinuousSeekMedia(media, replacement);
                        }
                        break;
                }
                expected = media.State;
                // Inline TCS completion under the existing gate deterministically precedes the new seek's commit.
                if (action is not ("resume" or "half-rate" or "double-rate" or "pause" or "error")) delayed.SetResult();
                if (action is "seek" or "stop") Assert.AreEqual(expected, media.State);
            }
            if (action is "resume" or "half-rate" or "double-rate" or "pause" or "error")
                await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            if (action is "resume" or "half-rate" or "double-rate" or "pause") delayed.SetResult();
            if (action == "error") delayed.SetException(new InvalidOperationException());
            await observed.WaitAsync(TimeSpan.FromSeconds(15));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            IndependentMediaState final = action switch
            {
                "resume" or "half-rate" or "double-rate" => IndependentMediaState.Playing,
                "pause" or "seek" => IndependentMediaState.Paused,
                "stop" => IndependentMediaState.Stopped,
                "ended" => IndependentMediaState.Ended,
                "failed" or "error" => IndependentMediaState.Failed,
                "reopen" => IndependentMediaState.Paused,
                _ => IndependentMediaState.Closed
            };
            Assert.AreEqual(final, media.State);
            if (action == "seek") Assert.AreEqual(20000000L, QueryContinuousSeekPosition(media));
            if (action == "error") Assert.AreEqual(new InvalidOperationException().HResult, media.Failure);
        }
        finally
        {
            delayed.TrySetResult();
            await observed.WaitAsync(TimeSpan.FromSeconds(15));
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }
}
