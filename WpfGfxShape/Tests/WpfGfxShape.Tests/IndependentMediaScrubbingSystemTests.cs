using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    private static unsafe int IdentityScrub(nint self, byte enabled)
        => ((delegate* unmanaged[Stdcall]<nint, byte, int>)(*(nint**)self)[11])(self, enabled);

    [TestMethod]
    public async Task WhenPausedScrubSeeksThenRealFramesAndUiEpochsRemainPaused()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-media-stage11");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        ControlEvents events = new(identity);
        nint proxy = CreateControlProxy(events);
        Assert.AreEqual(0, identity.AttachEvents(proxy));
        Assert.AreEqual(1u, ReleaseControlProxy(proxy));
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry registry = new();
        Assert.IsTrue(registry.TryAdd(1, table));
        var router = new GeneratedProtocolProductionContext(1, registry).CreateRouter();
        List<object> snapshots = [], samples = [];
        System.Collections.Concurrent.ConcurrentQueue<object> frames = new();
        Stopwatch clock = Stopwatch.StartNew();
        IndependentVideoPlaybackOwner? owner = null;
        uint references = uint.MaxValue;
        long pauseTicks = 0;
        bool passed = false;
        void Snapshot(string operation, int hr)
            => snapshots.Add(new { operation, hr, state = identity.State.ToString(), epoch = owner?.Epoch,
                pts = owner?.LastTimestamp, published = owner?.PublishedFrames, identity.UiRequestGeneration,
                identity.UiConsumedGeneration, identity.UiFrameGeneration, wallTicks = clock.Elapsed.Ticks });
        try
        {
            Assert.AreEqual(0, IdentityScrub(identity.Pointer, 1));
            Assert.AreEqual(0, IdentityRate(identity.Pointer, 0));
            Snapshot("preopen-scrub-rate0", 0);
            Assert.AreEqual(0, IdentityOpen(identity.Pointer, input));
            await identity.Completion;
            await events.Opened.Task;
            owner = identity.Owner;
            Assert.IsNotNull(owner);
            // Completion can be replaced by the deferred rate command during Open.
            await identity.Completion;
            Assert.AreEqual(IndependentMediaState.Paused, identity.State);
            Snapshot("opened-paused", 0);
            Assert.AreEqual(0, table.Create(1, MilResourceType.MediaPlayer));
            Assert.AreEqual(0, table.Create(2, MilResourceType.RenderData));
            Assert.AreEqual(0, table.Create(3, MilResourceType.Visual));
            Assert.AreEqual(2u, IdentityRef(identity.Pointer, 1));
            Assert.AreEqual(0, router.ProcessPackets([
                GeneratedProtocolPacketWriter.WriteMediaPlayer(1, unchecked((ulong)identity.Pointer), 0),
                GeneratedProtocolPacketWriter.WriteVisualCreate(3),
                GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WriteDrawVideoRecord(new(0, 0, 2, 1), 1)),
                GeneratedProtocolPacketWriter.WriteVisualSetContent(3, 2)]));
            Assert.IsTrue(table.TryGetResource(3, out var visual));
            owner.FrameReady += pts => frames.Enqueue(new { pts, epoch = owner.Epoch, wallTicks = clock.Elapsed.Ticks });
            foreach (long target in new long[] { 5000000, 25000000 })
            {
                Assert.AreEqual(0, IdentityUiRequest(identity.Pointer));
                int hr = IdentitySeek(identity.Pointer, target);
                Assert.AreEqual(0, hr);
                await identity.Completion;
                Assert.AreEqual((IndependentMediaState.Paused, target), (identity.State, owner.LastTimestamp));
                Assert.AreEqual(identity.UiRequestGeneration, identity.UiConsumedGeneration);
                Snapshot("scrub-seek", hr);
                samples.Add(RenderPlayerTarget((GeneratedVisualResource)visual!, target, clock.Elapsed.Ticks));
            }
            Assert.AreEqual((2L, 2), (owner.Epoch, owner.PublishedFrames));
            long start = clock.Elapsed.Ticks;
            Snapshot("pause-before", 0);
            await Task.Delay(515);
            pauseTicks = clock.Elapsed.Ticks - start;
            Assert.IsTrue(pauseTicks >= TimeSpan.FromMilliseconds(515).Ticks);
            Assert.AreEqual((2, 25000000L), (owner.PublishedFrames, owner.LastTimestamp));
            Snapshot("pause-after", 0);
            Assert.AreEqual(0, IdentityScrub(identity.Pointer, 0));
            Assert.AreEqual(0, IdentityUiRequest(identity.Pointer));
            long generation = identity.UiFrameGeneration;
            Assert.AreEqual(0, IdentitySeek(identity.Pointer, 30000000));
            await identity.Completion;
            await Task.Delay(515);
            Assert.AreEqual((2, generation, 30000000L, IndependentMediaState.Paused),
                (owner.PublishedFrames, identity.UiFrameGeneration, owner.LastTimestamp, identity.State));
            Assert.AreEqual(2L, identity.UiConsumedGeneration);
            Snapshot("disabled-seek-no-publication", 0);
            Assert.AreEqual(0, IdentityRate(identity.Pointer, 1));
            await identity.Completion;
            await events.Ended.Task;
            await identity.Completion;
            Assert.AreEqual((IndependentMediaState.Closed, true, 39000000L), (identity.State, owner.Ended, owner.LastTimestamp));
            Assert.AreEqual(0, IdentityScrub(identity.Pointer, 1) - unchecked((int)0x80000013));
            Snapshot("ended-callback-close", events.CloseHr);
            Assert.AreEqual(0, identity.Failure);
            CollectionAssert.AreEqual(new uint[] { 1, 11 }.Concat(Enumerable.Repeat(13u, owner.PublishedFrames)).Append(7u).ToArray(), events.Kinds.ToArray());
            passed = true;
        }
        finally
        {
            IdentityCall(identity.Pointer, 5);
            await identity.Completion;
            table.Delete(3, MilResourceType.Visual);
            table.Delete(2, MilResourceType.RenderData);
            table.Delete(1, MilResourceType.MediaPlayer);
            references = IdentityRef(identity.Pointer, 2);
            File.WriteAllText(Path.Combine(root, "result.json"), JsonSerializer.Serialize(new
            {
                passed, snapshots, samples, frames, pauseTicks, events.Packets, events.CloseHr,
                references, registryRemoved = IndependentMediaIdentity.Resolve(identity.Pointer) is null,
                descriptorDisposed = events.Disposed, nativeReferences = "Unknown", owner?.Epoch,
                owner?.PublishedFrames, owner?.LastTimestamp, owner?.Ended,
                runtime = RuntimeInformation.FrameworkDescription,
                testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MediaFoundationResourceTests).Assembly.Location))),
                testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
                modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>()
                    .Where(m => new[] { "mfplat.dll", "mfreadwrite.dll", "mfsrcsnk.dll", "mfcore.dll" }.Contains(m.ModuleName, StringComparer.OrdinalIgnoreCase))
                    .Select(m => new { m.ModuleName, m.FileName, version = m.FileVersionInfo.FileVersion }).ToArray()
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.AreEqual((0u, true), (references, events.Disposed));
    }
}
