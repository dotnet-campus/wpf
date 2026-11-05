using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Observes real MF event ordering for two independently opened production identities.</summary>
    [TestMethod]
    public async Task WhenRealOpenEpochsPrerollThenOpenedPrecedesOnePrerolledAsync()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Results", "stage19-" + Guid.NewGuid().ToString("N") + ".avi");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        WriteSourceReaderAvi(path, false);
        for (int epoch = 0; epoch < 2; epoch++)
        {
            var media = IndependentMediaIdentity.Create();
            var events = new ControlEvents(media);
            var prerolled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            events.OnEvent = kind => { if (kind == 11) prerolled.TrySetResult(); };
            nint proxy = CreateControlProxy(events);
            try
            {
                Assert.AreEqual(0, media.AttachEvents(proxy));
                ReleaseControlProxy(proxy);
                proxy = 0;
                Assert.AreEqual(0, IdentityOpen(media.Pointer, path));
                await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                await prerolled.Task.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.AreEqual(IndependentMediaState.Opened, media.State);
                Assert.AreEqual(false, media.Owner!.HasAudio);
                Assert.AreEqual(0, media.Owner.AudioMetadataResult);
                Assert.AreEqual(2u, media.Owner.Width);
                Assert.AreEqual(1u, media.Owner.Height);
                Assert.AreEqual(40000000L, media.Owner.Duration100ns);
                CollectionAssert.AreEqual(new uint[] { 1, 11 }, events.Kinds.Where(k => k is 1 or 11).ToArray());
                Console.WriteLine($"Stage19 epoch={epoch} Opened=1 Prerolled=11 metadata=2x1/no-audio duration=40000000 packets={System.Text.Json.JsonSerializer.Serialize(events.Packets)}");
            }
            finally
            {
                if (proxy != 0) ReleaseControlProxy(proxy);
                IdentityCall(media.Pointer, 5);
                IdentityRef(media.Pointer, 2);
            }
        }
    }

    /// <summary>Uses real asynchronous MF seeking, without assigning lifecycle state.</summary>
    [DataTestMethod]
    [DataRow(0d)]
    [DataRow(1d)]
    public async Task WhenRateIsSetDuringRealSeekingThenDeferredIntentIsAcceptedAsync(double rate)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Results", "stage19-seek-" + Guid.NewGuid().ToString("N") + ".avi");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        WriteSourceReaderAvi(path, false);
        var media = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(0, IdentityOpen(media.Pointer, path));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            var owner = media.Owner!;
            var barrier = new QueuedCallbackBarrier();
            var enqueue = typeof(IndependentVideoPlaybackOwner).GetMethod("Enqueue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            Task queued = (Task)enqueue.Invoke(owner, new object[] { (Action)barrier.Run })!;
            try
            {
                await barrier.Admitted.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.AreEqual(0, IdentitySeek(media.Pointer, 25000000));
                Assert.AreEqual(IndependentMediaState.Seeking, media.State);
                Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            }
            finally { await barrier.ReleaseAndDrainAsync(); }
            await queued.WaitAsync(TimeSpan.FromSeconds(10));
            Task command;
            do
            {
                command = media.Completion;
                await command.WaitAsync(TimeSpan.FromSeconds(10));
            }
            while (!ReferenceEquals(command, media.Completion));
            Assert.AreEqual(rate == 0 ? IndependentMediaState.Paused : IndependentMediaState.Playing, media.State);
        }
        finally
        {
            IdentityCall(media.Pointer, 5);
            IdentityRef(media.Pointer, 2);
        }
    }
}
