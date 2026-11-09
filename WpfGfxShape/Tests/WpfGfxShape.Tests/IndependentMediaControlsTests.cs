using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Checks pre-open rate, audio metadata, UI requests and real MF failure through the production COM proxy.</summary>
    [TestMethod]
    public async Task WhenWpfStatusCasesRunThenRealMetadataRequestsAndFailureArePreserved()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "stage10-status");
        Directory.CreateDirectory(root);
        await RunCase("audio-paused", true, 0, false);
        await RunCase("video-only-autoplay", false, 1, false);
        await RunCase("missing", false, 0, true);

        async Task RunCase(string name, bool audioExpected, double rate, bool missing)
        {
            string path = Path.Combine(root, missing ? Guid.NewGuid().ToString("N") + ".avi" : name + ".avi");
            if (!missing) WriteSourceReaderAvi(path, audioExpected);
            Assert.AreEqual(!missing, File.Exists(path));
            IndependentMediaIdentity media = IndependentMediaIdentity.Create();
            ControlEvents events = new(media);
            nint proxy = CreateControlProxy(events);
            List<object> calls = [];
            uint references = uint.MaxValue;
            int close = -1, failure = 0;
            bool passed = false;
            try
            {
                Assert.AreEqual(0, media.AttachEvents(proxy));
                Assert.AreEqual(1u, ReleaseControlProxy(proxy));
                Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
                Assert.AreEqual(0, IdentityUiRequest(media.Pointer));
                calls.Add(new { operation = "preopen", rate, request = media.UiRequestGeneration, consumed = media.UiConsumedGeneration, frame = media.UiFrameGeneration });
                Assert.AreEqual((1L, 0L, 0L), (media.UiRequestGeneration, media.UiConsumedGeneration, media.UiFrameGeneration));
                Assert.AreEqual(0, IdentityOpen(media.Pointer, path));
                if (missing || rate == 0) await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                if (missing)
                {
                    failure = await events.Failed.Task.WaitAsync(TimeSpan.FromSeconds(10));
                    Assert.IsTrue(failure < 0);
                    Assert.AreEqual((IndependentMediaState.Failed, failure), (media.State, media.Failure));
                    CollectionAssert.AreEqual(new uint[] { 8 }, events.Kinds.ToArray());
                    calls.Add(new { operation = "MF-failure", hr = $"0x{failure:X8}", path, exists = File.Exists(path) });
                }
                else
                {
                    await events.Opened.Task.WaitAsync(TimeSpan.FromSeconds(10));
                    IndependentVideoPlaybackOwner owner = media.Owner!;
                    Assert.AreEqual(audioExpected, owner.HasAudio);
                    VerifyWpfStatus(media.Pointer, calls, audioExpected);
                    if (rate == 0)
                    {
                        await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                        Assert.AreEqual(IndependentMediaState.Paused, media.State);
                        Assert.AreEqual(0, owner.PublishedFrames);
                        Assert.AreEqual((1L, 0L, 0L), (media.UiRequestGeneration, media.UiConsumedGeneration, media.UiFrameGeneration));
                        await Task.Delay(150);
                        Assert.AreEqual((IndependentMediaState.Paused, 0), (media.State, owner.PublishedFrames));
                        Assert.AreEqual(0, IdentityRate(media.Pointer, 1));
                    }
                    // Fixed observation points, independent of pixels and sample arrival.
                    await Task.Delay(350);
                    Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
                    await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                    Assert.IsTrue(owner.LastTimestamp > 0);
                    Assert.AreEqual(1L, media.UiConsumedGeneration);
                    calls.Add(new { operation = "first-consumption", request = media.UiRequestGeneration, consumed = media.UiConsumedGeneration, frame = media.UiFrameGeneration, pts = owner.LastTimestamp, state = media.State.ToString() });
                    Assert.AreEqual(0, IdentityUiRequest(media.Pointer));
                    Assert.AreEqual((2L, 1L), (media.UiRequestGeneration, media.UiConsumedGeneration));
                    Assert.AreEqual(0, IdentityRate(media.Pointer, 1));
                    long previous = owner.LastTimestamp;
                    await Task.Delay(350);
                    Assert.AreEqual(0, IdentityRate(media.Pointer, 0));
                    await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                    Assert.IsTrue(owner.LastTimestamp > previous);
                    Assert.AreEqual(2L, media.UiConsumedGeneration);
                    calls.Add(new { operation = "second-consumption", request = media.UiRequestGeneration, consumed = media.UiConsumedGeneration, frame = media.UiFrameGeneration, pts = owner.LastTimestamp });
                }
                close = IdentityCall(media.Pointer, 5);
                Assert.AreEqual(0, close);
                await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                long generation = media.UiRequestGeneration;
                Assert.AreEqual(unchecked((int)0x80000013), IdentityUiRequest(media.Pointer));
                Assert.AreEqual(generation, media.UiRequestGeneration);
                int count = events.Kinds.Count;
                await Task.Delay(200);
                Assert.AreEqual(count, events.Kinds.Count);
                calls.Add(new { operation = "closed-rejection", request = generation, consumed = media.UiConsumedGeneration, notificationCount = count, boundedWindowMilliseconds = 200 });
                passed = true;
            }
            finally
            {
                IdentityCall(media.Pointer, 5);
                await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                references = IdentityRef(media.Pointer, 2);
                File.WriteAllText(Path.Combine(root, name + ".json"), JsonSerializer.Serialize(new
                {
                    name, passed, audioExpected, rate, missing, path, calls, failure, close,
                    packets = events.Packets.ToArray(), kinds = events.Kinds.ToArray(), references,
                    registryRemoved = IndependentMediaIdentity.Resolve(media.Pointer) is null, events.Disposed,
                    testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MediaFoundationResourceTests).Assembly.Location))),
                    testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
            Assert.AreEqual(0u, references);
            Assert.IsTrue(events.Disposed);
        }
    }

    private static unsafe int IdentityUiRequest(nint self)
        => ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)self)[21])(self);

    private static unsafe void VerifyWpfStatus(nint self, List<object> calls, bool audioExpected = true)
    {
        nint* table = *(nint**)self;
        byte audio = 0, buffering = 1;
        double download = 0, buffered = 0;
        int audioHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[17])(self, &audio);
        int bufferingHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[12])(self, &buffering);
        int downloadHr = ((delegate* unmanaged[Stdcall]<nint, double*, int>)table[14])(self, &download);
        int bufferedHr = ((delegate* unmanaged[Stdcall]<nint, double*, int>)table[15])(self, &buffered);
        calls.Add(new { operation = "WpfStatus", audioHr, audio, bufferingHr, buffering, downloadHr, download, bufferedHr, buffered });
        Assert.AreEqual((0, audioExpected ? (byte)1 : (byte)0, 0, (byte)0, 0, 1d, 0, 1d),
            (audioHr, audio, bufferingHr, buffering, downloadHr, download, bufferedHr, buffered));
    }

    [TestMethod]
    public async Task WhenComControlsPauseSeekResumeThenRealEventsAndDrawVideoRemainOrdered()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-media-stage6");
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
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, registry).CreateRouter();
        List<object> frames = [], samples = [], calls = [];
        void DurationSnapshot(string operation)
        {
            var snapshot = IdentityDuration(identity.Pointer);
            calls.Add(new { operation, state = identity.State.ToString(), snapshot.lengthHr, snapshot.length, snapshot.pauseHr, snapshot.canPause });
            Assert.AreEqual((0, 40000000L, 0, (byte)1), snapshot);
        }
        events.BeforeClose = () =>
        {
            DurationSnapshot("EndedLength");
            Assert.AreEqual(40000000L, IdentityQuery(identity.Pointer).position);
        };
        Stopwatch clock = Stopwatch.StartNew();
        TaskCompletionSource paused = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IndependentVideoPlaybackOwner? owner = null;
        long pauseDuration = 0;
        uint finalReferences = uint.MaxValue;
        try
        {
            int open = IdentityOpen(identity.Pointer, input);
            calls.Add(new { operation = "Open", hr = open });
            Assert.AreEqual(0, open);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            await events.Opened.Task.WaitAsync(TimeSpan.FromSeconds(10));
            owner = identity.Owner;
            Assert.IsNotNull(owner);
            Assert.AreEqual(40000000L, owner.Duration100ns);
            DurationSnapshot("OpenedLength");
            VerifyWpfStatus(identity.Pointer, calls);
            Assert.AreEqual(0, IdentityUiRequest(identity.Pointer));
            owner.FrameReady += _ =>
            {
                if (identity.UiConsumedGeneration == 0) throw new InvalidOperationException("UI frame request was not consumed.");
            };
            Assert.AreEqual(unchecked((int)0xc00d36b2), IdentitySeek(identity.Pointer, 40000001));
            Assert.AreEqual(IndependentMediaState.Opened, identity.State);
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
            owner.FrameReady += timestamp =>
            {
                frames.Add(new { timestamp, epoch = owner.Epoch, wallTicks = clock.Elapsed.Ticks, thread = Environment.CurrentManagedThreadId });
                if (timestamp == 5000000 || timestamp == 25000000)
                    samples.Add(RenderPlayerTarget((GeneratedVisualResource)visual!, timestamp, clock.Elapsed.Ticks));
                if (timestamp == 5000000)
                {
                    DurationSnapshot("PlayingLength");
                    int hr = IdentityRate(identity.Pointer, 0);
                    calls.Add(new { operation = "Pause", hr, timestamp });
                    Assert.AreEqual(0, hr);
                    paused.SetResult();
                }
                if (owner.Epoch > 0) Assert.IsTrue(timestamp >= 25000000);
            };
            int play = IdentityRate(identity.Pointer, 1);
            calls.Add(new { operation = "Play", hr = play });
            Assert.AreEqual(0, play);
            await paused.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(IndependentMediaState.Paused, identity.State);
            DurationSnapshot("PausedLength");
            int count = owner.PublishedFrames;
            long position = owner.LastTimestamp;
            var before = IdentityQuery(identity.Pointer);
            calls.Add(new { operation = "PauseBefore", before.positionHr, before.position, before.videoHr, before.video, naturalSize = IdentityNaturalSize(identity.Pointer), count, wallTicks = clock.Elapsed.Ticks });
            Assert.AreEqual((0, 0, (byte)1), (before.positionHr, before.videoHr, before.video));
            // Pause freezes the transport clock at control completion, not at the last decoded PTS.
            Assert.AreEqual(5000000L, position);
            Assert.AreEqual(owner.Position100ns, before.position);
            long start = clock.Elapsed.Ticks;
            // A fixed wall-clock observation window, independent of image content.
            await Task.Delay(TimeSpan.FromMilliseconds(515));
            pauseDuration = clock.Elapsed.Ticks - start;
            Assert.IsTrue(pauseDuration >= TimeSpan.FromMilliseconds(515).Ticks);
            Assert.AreEqual((count, position), (owner.PublishedFrames, owner.LastTimestamp));
            var after = IdentityQuery(identity.Pointer);
            calls.Add(new { operation = "PauseAfter", after.positionHr, after.position, after.videoHr, after.video, naturalSize = IdentityNaturalSize(identity.Pointer), count = owner.PublishedFrames, wallTicks = clock.Elapsed.Ticks });
            Assert.AreEqual(before, after);
            int stop = IdentityCall(identity.Pointer, 4);
            calls.Add(new { operation = "Stop", hr = stop });
            Assert.AreEqual(0, stop);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(IndependentMediaState.Stopped, identity.State);
            DurationSnapshot("StoppedLength");
            Assert.AreEqual(0L, owner.LastTimestamp);
            int seek = IdentitySeek(identity.Pointer, 25000000);
            calls.Add(new { operation = "Seek", hr = seek, target = 25000000, wallTicks = clock.Elapsed.Ticks });
            Assert.AreEqual(0, seek);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(IndependentMediaState.Paused, identity.State);
            Assert.AreEqual(2L, owner.Epoch);
            DurationSnapshot("SeekCompletedLength");
            int unsupportedRate = IdentityRate(identity.Pointer, -1);
            calls.Add(new { operation = "NegativeRate", hr = unsupportedRate });
            Assert.AreEqual(unchecked((int)0x80004001), unsupportedRate);
            int resume = IdentityRate(identity.Pointer, 1);
            calls.Add(new { operation = "Resume", hr = resume });
            Assert.AreEqual(0, resume);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            await events.Ended.Task.WaitAsync(TimeSpan.FromSeconds(10));
            // Ended callback reenters Close; completion must not self-join the event thread.
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(IndependentMediaState.Closed, identity.State);
            var closedLength = IdentityDuration(identity.Pointer);
            calls.Add(new { operation = "ClosedLength", closedLength.lengthHr, closedLength.length, closedLength.pauseHr, closedLength.canPause });
            Assert.AreEqual((0, 0L, 0, (byte)0), closedLength);
            Assert.AreEqual((21, 39000000L, 2), (owner.PublishedFrames, owner.LastTimestamp, samples.Count));
            Assert.AreEqual(0, identity.Failure);
            CollectionAssert.AreEqual(new uint[] { 1, 11 }.Concat(Enumerable.Repeat(13u, owner.PublishedFrames)).Append(7u).ToArray(), events.Kinds.ToArray());
            Assert.AreEqual(0, events.CloseHr);
            Assert.AreEqual(unchecked((int)0x80000013), owner.Frames.Acquire(out _));
        }
        finally
        {
            IdentityCall(identity.Pointer, 5);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            _ = table.Delete(3, MilResourceType.Visual);
            _ = table.Delete(2, MilResourceType.RenderData);
            _ = table.Delete(1, MilResourceType.MediaPlayer);
            finalReferences = IdentityRef(identity.Pointer, 2);
            File.WriteAllText(Path.Combine(root, "result.json"), JsonSerializer.Serialize(new
            {
                calls, frames, samples, pauseWindowTicks = pauseDuration,
                packets = events.Packets.ToArray(), eventKinds = events.Kinds.ToArray(), events.CloseHr,
                finalReferences, registryRemoved = IndependentMediaIdentity.Resolve(identity.Pointer) is null,
                state = identity.State.ToString(), identity.Failure, published = owner?.PublishedFrames,
                ended = owner?.Ended, epoch = owner?.Epoch, descriptorDisposed = events.Disposed,
                runtime = RuntimeInformation.FrameworkDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
                testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MediaFoundationResourceTests).Assembly.Location))),
                modules = Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Where(m => m.ModuleName.StartsWith("mf", StringComparison.OrdinalIgnoreCase)).Select(m => new { m.ModuleName, m.FileName, version = m.FileVersionInfo.FileVersion }).ToArray(),
                backend = "source-linked production Av and GeneratedMediaPlayerResource -> production DrawVideo software RTB; not NativeAOT",
                nativeReferences = "Unknown; individual backend counters not exported"
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
        Assert.AreEqual(0u, finalReferences);
        Assert.IsTrue(events.Disposed);
    }

    private static unsafe (int lengthHr, long length, int pauseHr, byte canPause) IdentityDuration(nint self)
    {
        nint* table = *(nint**)self;
        long length = -1;
        byte canPause = 255;
        int hr = ((delegate* unmanaged[Stdcall]<nint, long*, int>)table[20])(self, &length);
        int pauseHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[13])(self, &canPause);
        return (hr, length, pauseHr, canPause);
    }

    private static unsafe object IdentityNaturalSize(nint self)
    {
        uint width = 0, height = 0;
        nint* table = *(nint**)self;
        int widthHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[19])(self, &width);
        int heightHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)table[18])(self, &height);
        Assert.AreEqual((0, 2u, 0, 1u), (widthHr, width, heightHr, height));
        return new { widthHr, width, heightHr, height };
    }

    private static unsafe int IdentitySeek(nint self, long timestamp)
        => ((delegate* unmanaged[Stdcall]<nint, long, int>)(*(nint**)self)[7])(self, timestamp);

    private sealed class ControlEvents(IndependentMediaIdentity identity)
    {
        internal readonly TaskCompletionSource Opened = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource Ended = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource<int> Failed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly System.Collections.Concurrent.ConcurrentQueue<uint> Kinds = new();
        internal readonly System.Collections.Concurrent.ConcurrentQueue<object> Packets = new();
        internal bool Disposed;
        internal int CloseHr = -1;
        internal Action? BeforeClose;
        internal Action<uint>? OnEvent;
        internal void Receive(byte[] packet)
        {
            uint kind = BinaryPrimitives.ReadUInt32LittleEndian(packet);
            Kinds.Enqueue(kind);
            Packets.Enqueue(new { kind, epoch = identity.Owner?.Epoch, bytes = Convert.ToHexString(packet), thread = Environment.CurrentManagedThreadId, wall = Stopwatch.GetTimestamp() });
            OnEvent?.Invoke(kind);
            if (kind == 1) Opened.TrySetResult();
            if (kind == 8) Failed.TrySetResult(BinaryPrimitives.ReadInt32LittleEndian(packet.AsSpan(4)));
            if (kind == 7)
            {
                BeforeClose?.Invoke();
                CloseHr = IdentityCall(identity.Pointer, 5);
                Ended.TrySetResult();
            }
        }
    }
    private static unsafe nint CreateControlProxy(ControlEvents events)
    {
        GCHandle handle = GCHandle.Alloc(events);
        EventProxyDescriptor descriptor = new() { Dispose = &DisposeControlProxy, RaiseEvent = &RaiseControlEvent, Handle = (nuint)GCHandle.ToIntPtr(handle) };
        var method = typeof(WpfGfxShape.Abi.EventProxyExports).GetMethod("Create", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        var create = (delegate* unmanaged[Stdcall]<EventProxyDescriptor*, nint*, int>)method.MethodHandle.GetFunctionPointer();
        nint proxy = 0;
        Assert.AreEqual(0, create(&descriptor, &proxy));
        return proxy;
    }

    private static unsafe uint ReleaseControlProxy(nint proxy)
        => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)proxy)[2])(proxy);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe void DisposeControlProxy(EventProxyDescriptor* descriptor)
    {
        GCHandle handle = GCHandle.FromIntPtr((nint)descriptor->Handle);
        ((ControlEvents)handle.Target!).Disposed = true;
        handle.Free();
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int RaiseControlEvent(EventProxyDescriptor* descriptor, byte* packet, uint size)
    {
        ((ControlEvents)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!).Receive(new ReadOnlySpan<byte>(packet, checked((int)size)).ToArray());
        return 0;
    }
}
