using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Preserves the public pure-video restriction without applying it to internal AV decoding.</summary>
    [TestMethod]
    public async Task WhenPublicVideoOnlyPolicySeesAudioThenRealOpenFailsBeforePrerollAsync()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-media-stage15");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "public-audio-negative.avi");
        WriteSourceReaderAvi(input);
        nint client = IndependentMediaResourceBindingTests.CreateClient(out var disposals, out _, _ => { });
        Assert.AreEqual(0, SystemMediaSource.Create(client, 1, out nint pointer, requireVideoOnly: true));
        IndependentMediaIdentity media = IndependentMediaIdentity.Resolve(pointer)!;
        IndependentVideoPlaybackOwner? owner = null;
        try
        {
            Assert.AreEqual(0, IdentityOpen(pointer, input));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            owner = media.Owner;
            Assert.IsNotNull(owner);
            Assert.AreEqual(true, owner.HasAudio);
            Assert.AreEqual(unchecked((int)0xc00d36b4), media.Failure);
            Assert.AreEqual(IndependentMediaState.Failed, media.State);
            Assert.AreEqual(0, owner.PublishedFrames);
            Assert.IsTrue(owner.ThreadExited);
            File.WriteAllText(Path.Combine(root, "public-audio-negative.json"), JsonSerializer.Serialize(new
            {
                media.Failure, State = media.State.ToString(), owner.HasAudio, owner.PublishedFrames, owner.ThreadExited
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        finally
        {
            IdentityCall(pointer, 5);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(0u, IdentityRef(pointer, 2));
            Assert.AreEqual(0u, IdentityRef(client, 2));
        }
    }

    /// <summary>Runs the internal factory and resource transport against real MF, separately counting resource and COM events.</summary>
    [TestMethod]
    public async Task WhenInternalBridgePlaysThenResourceNotificationsAndDrawVideoAreReal()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-media-stage15");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        System.Collections.Concurrent.ConcurrentQueue<string> packetBytes = new();
        nint client = IndependentMediaResourceBindingTests.CreateClient(out var disposals, out var packets,
            bytes => packetBytes.Enqueue(Convert.ToHexString(bytes)));
        Assert.AreEqual(0, SystemMediaSource.Create(client, 1, out nint media));
        IndependentMediaIdentity identity = IndependentMediaIdentity.Resolve(media)!;
        Assert.IsFalse(identity.RequireVideoOnly);
        SystemMediaSource source = SystemMediaSource.Resolve(media)!;
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry registry = new();
        Assert.IsTrue(registry.TryAdd(1, table));
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, registry).CreateRouter();
        List<object> samples = [];
        List<int> changes = [];
        int subscribed = 0, atUnsubscribe = 0, afterUnsubscribe = 0, resourceBeforeDelete = 0, resourceAfterDelete = 0;
        uint final = uint.MaxValue;
        bool passed = false;
        SystemVideoSample? retainedLease = null;
        string? lateLeasePixels = null;
        bool leaseDisposed = false;
        int missingFailure = 0, missingAtUnsubscribe = 0, missingAfterUnsubscribe = 0;
        long? missingObservationStart = null, missingObservationEnd = null;
        bool? missingOwnerThreadExited = null;
        IndependentVideoPlaybackOwner? owner = null;
        Action listener = () => Interlocked.Increment(ref subscribed);
        try
        {
            Assert.AreEqual(0, table.Create(1, MilResourceType.MediaPlayer));
            Assert.AreEqual(0, table.Create(2, MilResourceType.RenderData));
            Assert.AreEqual(0, table.Create(3, MilResourceType.Visual));
            Assert.AreEqual(2u, IdentityRef(media, 1));
            Assert.AreEqual(0, router.ProcessPackets([
                GeneratedProtocolPacketWriter.WriteMediaPlayer(1, unchecked((ulong)media), 0),
                GeneratedProtocolPacketWriter.WriteVisualCreate(3),
                GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WriteDrawVideoRecord(new(0, 0, 2, 1), 1)),
                GeneratedProtocolPacketWriter.WriteVisualSetContent(3, 2)]));
            Assert.IsTrue(table.TryGetResource(1, out var resource));
            Assert.IsTrue(table.TryGetResource(3, out var visual));
            source.Subscribe(listener);
            Assert.AreEqual(0, IdentityOpen(media, input));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            owner = identity.Owner;
            Assert.IsNotNull(owner);
            int initialChanges = resource!.ChangeCount;
            owner.FrameReady += OnFrame;
            void OnFrame(long timestamp)
            {
                changes.Add(resource.ChangeCount);
                if (timestamp == 5000000)
                {
                    Assert.AreEqual(0, source.Acquire(out retainedLease));
                    Assert.IsNotNull(retainedLease);
                }
                if (timestamp == 5000000 || timestamp == 25000000)
                    samples.Add(RenderPlayerTarget((GeneratedVisualResource)visual!, timestamp, 0));
                if (timestamp == 10000000)
                {
                    source.Unsubscribe(listener);
                    atUnsubscribe = Volatile.Read(ref subscribed);
                }
                if (timestamp == 30000000)
                {
                    resourceBeforeDelete = resource.ChangeCount;
                    // Drop graph references first, then the transport reference while still in a frame callback.
                    Assert.AreEqual(0, table.Delete(3, MilResourceType.Visual));
                    Assert.AreEqual(0, table.Delete(2, MilResourceType.RenderData));
                    Assert.AreEqual(0, table.Delete(1, MilResourceType.MediaPlayer));
                }
            }
            Assert.AreEqual(0, IdentityRate(media, 1));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(15));
            owner.FrameReady -= OnFrame;
            afterUnsubscribe = Volatile.Read(ref subscribed);
            resourceAfterDelete = resource.ChangeCount;
            Assert.AreEqual(40, owner.PublishedFrames);
            Assert.AreEqual(2, samples.Count);
            Assert.IsTrue(changes.Count == 40 && changes[5] > initialChanges);
            Assert.IsTrue(atUnsubscribe > 0);
            Assert.AreEqual(atUnsubscribe, afterUnsubscribe);
            Assert.IsTrue(resourceBeforeDelete > initialChanges);
            Assert.AreEqual(resourceBeforeDelete, resourceAfterDelete);
            Assert.IsTrue(packets() > 0);
            Assert.AreEqual(0, IdentityCall(media, 5));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsTrue(owner.ThreadExited);
            Assert.AreEqual(0, source.Acquire(out var closedSample));
            Assert.IsNull(closedSample);
            source.Subscribe(listener);
            Assert.AreEqual(0, IdentityOpen(media, Path.Combine(root, Guid.NewGuid().ToString("N") + ".missing")));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            missingFailure = identity.Failure;
            Assert.AreEqual(unchecked((int)0x80070002), missingFailure);
            missingOwnerThreadExited = identity.Owner!.ThreadExited;
            Assert.IsTrue(missingOwnerThreadExited.Value);
            source.Unsubscribe(listener);
            missingAtUnsubscribe = Volatile.Read(ref subscribed);
            missingObservationStart = Stopwatch.GetTimestamp();
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            missingObservationEnd = Stopwatch.GetTimestamp();
            missingAfterUnsubscribe = Volatile.Read(ref subscribed);
            Assert.AreEqual(missingAtUnsubscribe, missingAfterUnsubscribe);
            passed = true;
        }
        finally
        {
            source.Unsubscribe(listener);
            IdentityCall(media, 5);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            _ = table.Delete(3, MilResourceType.Visual);
            _ = table.Delete(2, MilResourceType.RenderData);
            _ = table.Delete(1, MilResourceType.MediaPlayer);
            final = IdentityRef(media, 2);
            Core.Direct3D9Factory.Release(client);
            try
            {
                if (retainedLease is not null)
                {
                    lateLeasePixels = ReadClosedLease(retainedLease);
                    Assert.AreEqual("0000FFFF0000FFFF", lateLeasePixels);
                }
            }
            finally
            {
                retainedLease?.Dispose();
                leaseDisposed = retainedLease is not null;
            }
            File.WriteAllText(Path.Combine(root, "bridge.json"), JsonSerializer.Serialize(new
            {
                passed, lateLeasePixels, leaseDisposed, missingFailure, missingAtUnsubscribe, missingAfterUnsubscribe,
                missingOwnerThreadExited, missingObservationStart, missingObservationEnd,
                missingObservationMilliseconds = missingObservationStart is long start && missingObservationEnd is long end
                    ? (double?)Stopwatch.GetElapsedTime(start, end).TotalMilliseconds : null,
                packetBytes = packetBytes.ToArray(), final, registryRemoved = IndependentMediaIdentity.Resolve(media) is null,
                descriptorDisposals = disposals(), comPackets = packets(), resourceChanges = changes,
                subscribed, atUnsubscribe, afterUnsubscribe, observationWindow = "PTS 1s to 3.9s; no admitted callback pending at removal on owner thread",
                resourceBeforeDelete, resourceAfterDelete, unbindWindow = "PTS 3s to 3.9s", samples,
                owner?.PublishedFrames, owner?.ThreadExited, state = identity.State.ToString(), identity.Failure,
                cleanup = owner?.ClosedReader?.Cleanup,
                runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(), pointerSize = IntPtr.Size,
                testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MediaFoundationResourceTests).Assembly.Location))),
                testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
                modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.StartsWith("mf", StringComparison.OrdinalIgnoreCase) || m.ModuleName.Contains("wpfgfx", StringComparison.OrdinalIgnoreCase)).Select(m => new { m.ModuleName, m.FileName, version = m.FileVersionInfo.FileVersion }).ToArray(),
                scope = "source-linked managed production bridge; fixture Native ABI not this internal bridge; public factory remains gated; native/lease bottom-level references Unknown"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.AreEqual(0u, final);
        Assert.IsNull(SystemMediaSource.Resolve(media));
        Assert.AreEqual(1, disposals());
    }
}
