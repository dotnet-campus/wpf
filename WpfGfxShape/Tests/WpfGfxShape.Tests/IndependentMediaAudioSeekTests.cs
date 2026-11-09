using System.Reflection;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    private static FieldInfo AudioSeekField(string name) => typeof(IndependentMediaIdentity).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

    /// <summary>Public vtable admission with a real failed owner; no synthetic device success or Native interleaving claim.</summary>
    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task WhenPublicAudioSeekReachesFailedOwnerThenOriginalFailureSettlesAndTargetsClear(bool stop)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        IndependentAudioPlaybackOwner audio = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        int failure = await audio.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        try
        {
            AudioSeekField("_audioOwner").SetValue(media, audio);
            typeof(IndependentMediaIdentity).GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, IndependentMediaState.Opened);
            Assert.AreEqual(0, stop ? SubmitGenerationStop(media.Pointer) : IdentitySeek(media.Pointer, 20000000));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual((unchecked((int)0x80070003), failure, IndependentMediaState.Failed, null, null),
                (media.Failure, failure, media.State, AudioSeekField("_pendingSeek").GetValue(media), AudioSeekField("_seekPosition").GetValue(media)));
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>Controlled observer task and source state exercise identity responsibility, not a replacement owner or Native timing.</summary>
    [DataTestMethod]
    [DataRow("latest")]
    [DataRow("close")]
    [DataRow("null")]
    [DataRow("source")]
    public async Task WhenPublicAudioSeekIsPendingThenLatestQueryOrSourceRetirementWins(string action)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        IndependentAudioPlaybackOwner audio = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        await audio.Completion.WaitAsync(TimeSpan.FromSeconds(20));
        TaskCompletionSource<int> delayed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task observer = Task.CompletedTask;
        try
        {
            AudioSeekField("_audioOwner").SetValue(media, audio);
            typeof(IndependentMediaIdentity).GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, IndependentMediaState.Seeking);
            AudioSeekField("_seekAfter").SetValue(media, IndependentMediaState.Paused);
            AudioSeekField("_seekPosition").SetValue(media, 10000000L);
            AudioSeekField("_path").SetValue(media, "original.wav");
            observer = (Task)typeof(IndependentMediaIdentity).GetMethod("ObserveAudioSeekAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(media, [audio, AudioSeekField("_sourceGeneration").GetValue(media), delayed.Task])!;
            AudioSeekField("_audioSeekOperation").SetValue(media, observer);
            object gate = AudioSeekField("_gate").GetValue(media)!;
            lock (gate)
            {
                Assert.AreEqual(0, IdentitySeek(media.Pointer, 20000000));
                Assert.AreEqual(20000000L, QueryContinuousSeekPosition(media));
                Assert.AreEqual(0, IdentitySeek(media.Pointer, 30000000));
                Assert.AreEqual(30000000L, QueryContinuousSeekPosition(media));
                if (action == "close") CloseContinuousSeekMedia(media);
                if (action == "null") IdentityOpen(media.Pointer, null);
                if (action == "source")
                {
                    AudioSeekField("_sourceGeneration").SetValue(media, new object());
                    AudioSeekField("_audioOwner").SetValue(media, null);
                    AudioSeekField("_seekPosition").SetValue(media, 40000000L);
                }
                delayed.SetResult(0);
            }
            await observer.WaitAsync(TimeSpan.FromSeconds(10));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(action == "source" ? 40000000L : 0L, QueryContinuousSeekPosition(media));
            if (action == "latest") Assert.AreEqual(unchecked((int)0x80070003), media.Failure);
        }
        finally
        {
            delayed.TrySetResult(unchecked((int)0x800704c7));
            await observer.WaitAsync(TimeSpan.FromSeconds(10));
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>Opening target is consumed after real Ready failure without hanging or reporting E_NOTIMPL.</summary>
    [TestMethod]
    public async Task WhenPublicAudioOpeningHasTargetThenRealInitializationFailureClearsIt()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        IndependentAudioPlaybackOwner audio = new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.wav"));
        try
        {
            AudioSeekField("_audioOwner").SetValue(media, audio);
            typeof(IndependentMediaIdentity).GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, IndependentMediaState.Opening);
            Assert.AreEqual(0, IdentitySeek(media.Pointer, 20000000));
            Assert.AreEqual(20000000L, QueryContinuousSeekPosition(media));
            Task opening = (Task)typeof(IndependentMediaIdentity).GetMethod("ObserveAudioOpenAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(media, [audio, AudioSeekField("_sourceGeneration").GetValue(media)])!;
            await opening.WaitAsync(TimeSpan.FromSeconds(20));
            Assert.AreEqual((unchecked((int)0x80070003), IndependentMediaState.Failed, 0L),
                (media.Failure, media.State, QueryContinuousSeekPosition(media)));
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            ReleaseContinuousSeekMedia(media);
        }
    }
}
