using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Verifies real source duration and replay through the production COM identity.</summary>
    [TestMethod]
    public async Task WhenDurationSourceEndsThenReplayUsesNewEpochAndClockAnchor()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-media-stage8");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        Dictionary<string, string>? metadata = null;
        await Task.Run(() =>
        {
            Assert.AreEqual(0, MediaFoundationSourceReader.Create(input, out var reader));
            using (reader)
            {
                Assert.IsNotNull(reader);
                metadata = new(reader.SourceMetadata);
                Assert.AreEqual("21", metadata["duration100ns.vt"]);
                Assert.AreEqual("40000000", metadata["duration100ns"]);
                Assert.AreEqual("19", metadata["characteristics.vt"]);
            }
        });
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        IndependentVideoPlaybackOwner? owner = null;
        List<object> frames = [], calls = [];
        TaskCompletionSource replayPaused = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Stopwatch clock = new();
        uint finalReferences = uint.MaxValue;
        try
        {
            Assert.AreEqual(0, IdentityOpen(identity.Pointer, input));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            owner = identity.Owner;
            Assert.IsNotNull(owner);
            Assert.AreEqual((0, 40000000L, 0, (byte)1), IdentityDuration(identity.Pointer));
            Assert.AreEqual(0, IdentitySeek(identity.Pointer, 39000000));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(1L, owner.Epoch);
            owner.FrameReady += timestamp =>
            {
                frames.Add(new { timestamp, epoch = owner.Epoch, wallTicks = clock.Elapsed.Ticks });
                if (owner.Epoch == 2 && timestamp == 1000000)
                {
                    int pauseHr = IdentityRate(identity.Pointer, 0);
                    calls.Add(new { operation = "ReplayPause", pauseHr, timestamp, epoch = owner.Epoch });
                    Assert.AreEqual(0, pauseHr);
                    replayPaused.TrySetResult();
                }
            };
            TaskCompletionSource ended = new(TaskCreationOptions.RunContinuationsAsynchronously);
            owner.PlaybackEnded += () => ended.TrySetResult();
            Assert.AreEqual(0, IdentityRate(identity.Pointer, 1));
            await ended.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(IndependentMediaState.Ended, identity.State);
            var endedLength = IdentityDuration(identity.Pointer);
            var endedPosition = IdentityQuery(identity.Pointer);
            calls.Add(new { operation = "Ended", endedLength.lengthHr, endedLength.length, endedLength.pauseHr, endedLength.canPause, endedPosition.positionHr, endedPosition.position, epoch = owner.Epoch });
            Assert.AreEqual((0, 40000000L, 0, (byte)1), endedLength);
            Assert.AreEqual(40000000L, endedPosition.position);
            Assert.AreEqual(39000000L, owner.LastTimestamp);
            int overrun = IdentitySeek(identity.Pointer, 40000001);
            calls.Add(new { operation = "Overrun", hr = overrun, epoch = owner.Epoch, state = identity.State.ToString() });
            Assert.AreEqual(unchecked((int)0xc00d36b2), overrun);
            Assert.AreEqual((IndependentMediaState.Ended, 1L), (identity.State, owner.Epoch));
            clock.Start();
            int replayHr = IdentityRate(identity.Pointer, 1);
            calls.Add(new { operation = "Replay", hr = replayHr });
            Assert.AreEqual(0, replayHr);
            await replayPaused.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual((IndependentMediaState.Paused, 2L, 1000000L, false), (identity.State, owner.Epoch, owner.LastTimestamp, owner.Ended));
            Assert.IsTrue(clock.Elapsed.Ticks >= 1000000);
            Assert.AreEqual((0, 40000000L, 0, (byte)1), IdentityDuration(identity.Pointer));
            Assert.AreEqual(3, owner.PublishedFrames);
        }
        finally
        {
            int closeHr = IdentityCall(identity.Pointer, 5);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            var closedLength = IdentityDuration(identity.Pointer);
            calls.Add(new { operation = "Close", hr = closeHr, closedLength.lengthHr, closedLength.length, closedLength.pauseHr, closedLength.canPause });
            finalReferences = IdentityRef(identity.Pointer, 2);
            File.WriteAllText(Path.Combine(root, "duration-replay.json"), JsonSerializer.Serialize(new
            {
                metadata, calls, frames, finalReferences,
                registryRemoved = IndependentMediaIdentity.Resolve(identity.Pointer) is null,
                state = identity.State.ToString(), identity.Failure,
                runtime = RuntimeInformation.FrameworkDescription,
                architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
                testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MediaFoundationResourceTests).Assembly.Location))),
                sourceSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(input))),
                modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.StartsWith("mf", StringComparison.OrdinalIgnoreCase)).Select(m => new { m.ModuleName, m.FileName, version = m.FileVersionInfo.FileVersion }).ToArray(),
                backend = "source-linked production identity; no audio output; not NativeAOT"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.AreEqual(0u, finalReferences);
    }
}
