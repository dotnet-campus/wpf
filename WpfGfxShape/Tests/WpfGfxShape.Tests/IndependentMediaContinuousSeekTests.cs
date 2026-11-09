using System.Reflection;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Managed MF execution with reflection holding the existing identity gate; not NativeAOT interleaving.</summary>
    [DataTestMethod]
    [DataRow("latest")]
    [DataRow("scrub")]
    [DataRow("close")]
    [DataRow("source")]
    [DataRow("reopen")]
    public async Task WhenIndependentMediaReceivesContinuousSeekThenLatestIntentOrInterruptionSettles(string action)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "continuous-seek-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            IndependentVideoPlaybackOwner owner = media.Owner!;
            EnableContinuousSeekScrub(media, action == "scrub");
            bool completes = action is "latest" or "scrub";
            object gate = typeof(IndependentMediaIdentity).GetField("_gate", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(media)!;
            string replacement = Path.Combine(root, "replacement.avi");
            WriteSourceReaderAvi(replacement);
            int first, second, latest, interrupted;
            long firstPosition, secondPosition, latestPosition, resetPosition;
            List<long> scrubPositions = new();
            owner.FrameReady += _ => scrubPositions.Add(QueryContinuousSeekPosition(media));
            Task seeks;
            lock (gate)
            {
                first = SubmitContinuousSeek(media, 5000000);
                firstPosition = QueryContinuousSeekPosition(media);
                seeks = media.Completion;
                second = SubmitContinuousSeek(media, 10000000);
                secondPosition = QueryContinuousSeekPosition(media);
                latest = SubmitContinuousSeek(media, 20000000);
                latestPosition = QueryContinuousSeekPosition(media);
                interrupted = InterruptContinuousSeek(media, action);
                if (action == "reopen") OpenContinuousSeekMedia(media, replacement);
                resetPosition = QueryContinuousSeekPosition(media);
            }
            await seeks.WaitAsync(TimeSpan.FromSeconds(15));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual((0, 0, 0, 0, 5000000L, 10000000L, 20000000L,
                completes ? 20000000L : 0L,
                completes ? IndependentMediaState.Paused : action == "reopen" ? IndependentMediaState.Opened : IndependentMediaState.Closed,
                completes ? 20000000L : 0L),
                (first, second, latest, interrupted, firstPosition, secondPosition, latestPosition,
                resetPosition, media.State, QueryContinuousSeekPosition(media)));
            if (action == "scrub")
                CollectionAssert.AreEqual(new long[] { 20000000, 20000000 }, scrubPositions.ToArray());
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>The real scrub callback reenters the public vtable before the old seek completes.</summary>
    [TestMethod]
    public async Task WhenIndependentMediaScrubReentersSeekThenOldCompletionPreservesNewTarget()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "reentrant-seek-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            OpenContinuousSeekMedia(media, input);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            EnableContinuousSeekScrub(media, true);
            List<long> positions = new();
            media.Owner!.FrameReady += _ =>
            {
                if (positions.Count == 0) Assert.AreEqual(0, SubmitContinuousSeek(media, 20000000));
                positions.Add(QueryContinuousSeekPosition(media));
            };
            Assert.AreEqual(0, SubmitContinuousSeek(media, 5000000));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            CollectionAssert.AreEqual(new long[] { 20000000, 20000000 }, positions.ToArray());
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    /// <summary>Holding the identity gate controls managed opening submission, not Native interleaving.</summary>
    [TestMethod]
    public async Task WhenIndependentMediaOpeningFailsThenAcceptedQueryTargetIsCleared()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            object gate = typeof(IndependentMediaIdentity).GetField("_gate", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(media)!;
            lock (gate)
            {
                OpenContinuousSeekMedia(media, Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString("N") + ".avi"));
                Assert.AreEqual(0, SubmitContinuousSeek(media, 20000000));
                Assert.AreEqual(20000000L, QueryContinuousSeekPosition(media));
            }
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            Assert.AreEqual((IndependentMediaState.Failed, 0L), (media.State, QueryContinuousSeekPosition(media)));
        }
        finally
        {
            CloseContinuousSeekMedia(media);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            ReleaseContinuousSeekMedia(media);
        }
    }

    private static unsafe void OpenContinuousSeekMedia(IndependentMediaIdentity media, string input)
    {
        nint* table = *(nint**)media.Pointer;
        fixed (char* path = input)
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, path));
    }

    private static unsafe void EnableContinuousSeekScrub(IndependentMediaIdentity media, bool enabled)
        => Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte, int>)(*(nint**)media.Pointer)[11])(media.Pointer, enabled ? (byte)1 : (byte)0));

    private static unsafe long QueryContinuousSeekPosition(IndependentMediaIdentity media)
    {
        long position = -1;
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, long*, int>)(*(nint**)media.Pointer)[6])(media.Pointer, &position));
        return position;
    }

    private static unsafe int SubmitContinuousSeek(IndependentMediaIdentity media, long timestamp)
        => ((delegate* unmanaged[Stdcall]<nint, long, int>)(*(nint**)media.Pointer)[7])(media.Pointer, timestamp);

    private static unsafe int InterruptContinuousSeek(IndependentMediaIdentity media, string action)
        => action switch
        {
            "close" => ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)media.Pointer)[5])(media.Pointer),
            "source" => ((delegate* unmanaged[Stdcall]<nint, char*, int>)(*(nint**)media.Pointer)[3])(media.Pointer, null),
            _ => 0
        };

    private static unsafe void CloseContinuousSeekMedia(IndependentMediaIdentity media)
        => ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)media.Pointer)[5])(media.Pointer);

    private static unsafe void ReleaseContinuousSeekMedia(IndependentMediaIdentity media)
        => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)media.Pointer)[2])(media.Pointer);
}
