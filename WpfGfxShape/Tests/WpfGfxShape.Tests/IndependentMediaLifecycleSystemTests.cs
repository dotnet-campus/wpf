using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Verifies that closing a source preserves registration and reopening emits real events.</summary>
    [TestMethod]
    public async Task WhenClosedSourceReopensThenSameProxyReceivesOpenedAndFrames()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-media-stage13");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "reopen.avi");
        WriteSourceReaderAvi(input);
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        ControlEvents events = new(identity);
        nint proxy = CreateControlProxy(events);
        Assert.AreEqual(0, identity.AttachEvents(proxy));
        Assert.AreEqual(1u, ReleaseControlProxy(proxy));
        int opened = 0, frames = 0;
        bool passed = false;
        uint references = uint.MaxValue;
        try
        {
            for (int cycle = 0; cycle < 2; cycle++)
            {
                TaskCompletionSource openedEvent = new(TaskCreationOptions.RunContinuationsAsynchronously);
                TaskCompletionSource frameEvent = new(TaskCreationOptions.RunContinuationsAsynchronously);
                events.OnEvent = kind =>
                {
                    if (kind == 1) { Interlocked.Increment(ref opened); openedEvent.TrySetResult(); }
                    if (kind == 13) { Interlocked.Increment(ref frames); frameEvent.TrySetResult(); }
                };
                Assert.AreEqual(0, IdentityOpen(identity.Pointer, input));
                await openedEvent.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.AreEqual(0, IdentityRate(identity.Pointer, 1));
                await frameEvent.Task.WaitAsync(TimeSpan.FromSeconds(10));
                IndependentVideoPlaybackOwner owner = identity.Owner!;
                Assert.AreEqual(0, IdentityCall(identity.Pointer, 5));
                await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.IsTrue(owner.ThreadExited);
                Assert.IsFalse(events.Disposed);
                CollectionAssert.AreEqual(Enumerable.Range(0, cycle + 1).SelectMany(_ => new uint[] { 1, 11 }).ToArray(),
                    events.Kinds.Where(kind => kind != 13).ToArray());
            }
            Assert.AreEqual(2, opened);
            Assert.IsTrue(frames >= 2);
            Assert.AreEqual(0, IdentityCall(identity.Pointer, 22));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(unchecked((int)0x80000013), IdentityOpen(identity.Pointer, input));
            passed = true;
        }
        finally
        {
            IdentityCall(identity.Pointer, 22);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            references = IdentityRef(identity.Pointer, 2);
            File.WriteAllText(Path.Combine(root, "close-reopen.json"), JsonSerializer.Serialize(new
            {
                passed, opened, frames, references, descriptorDisposed = events.Disposed,
                registryRemoved = IndependentMediaIdentity.Resolve(identity.Pointer) is null,
                packets = events.Packets.ToArray(), cleanup = identity.Owner?.ClosedReader?.Cleanup.ToArray()
            }, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    private static unsafe string ReadClosedLease(SystemVideoSample lease)
    {
        nint source = lease.Source;
        byte[] pixels = new byte[8];
        fixed (byte* target = pixels)
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)source)[7])(source, 0, 8, 8, target));
        return Convert.ToHexString(pixels);
    }

    [TestMethod]
    public async Task WhenMissingOpenFailsThenCallbackCanCloseAndRelease()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-media-stage12");
        Directory.CreateDirectory(root);
        foreach (bool release in new[] { false, true })
        {
            IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
            ControlEvents events = new(identity);
            TaskCompletionSource returned = new(TaskCreationOptions.RunContinuationsAsynchronously);
            nint proxy = CreateControlProxy(events);
            Assert.AreEqual(0, identity.AttachEvents(proxy));
            Assert.AreEqual(1u, ReleaseControlProxy(proxy));
            uint callbackResult = uint.MaxValue;
            IndependentVideoPlaybackOwner? callbackOwner = null;
            events.OnEvent = kind =>
            {
                if (kind != 8) return;
                callbackOwner = identity.Owner;
                callbackResult = release ? IdentityRef(identity.Pointer, 2) : unchecked((uint)IdentityCall(identity.Pointer, 5));
                returned.TrySetResult();
            };
            bool passed = false;
            uint references = uint.MaxValue;
            try
            {
                Assert.AreEqual(0, IdentityOpen(identity.Pointer, Path.Combine(root, Guid.NewGuid().ToString("N") + ".missing")));
                await returned.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.AreEqual(0u, callbackResult);
                Assert.AreEqual(unchecked((int)0x80070002), identity.Failure);
                Assert.AreEqual(IndependentMediaState.Closed, identity.State);
                Assert.IsTrue(callbackOwner!.ThreadExited);
                Assert.AreEqual(release, events.Disposed);
                passed = true;
            }
            finally
            {
                if (!release || callbackResult != 0)
                {
                    IdentityCall(identity.Pointer, 5);
                    await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                    references = IdentityRef(identity.Pointer, 2);
                }
                else references = callbackResult;
                File.WriteAllText(Path.Combine(root, release ? "failed-release.json" : "failed-close.json"), JsonSerializer.Serialize(new
                {
                    passed, release, callbackResult, references, failure = identity.Failure,
                    state = identity.State.ToString(), threadExited = identity.Owner?.ThreadExited,
                    descriptorDisposed = events.Disposed, registryRemoved = IndependentMediaIdentity.Resolve(identity.Pointer) is null,
                    packets = events.Packets.ToArray(), cleanup = identity.Owner?.ClosedReader?.Cleanup.ToArray(),
                    nativeReferences = "Unknown"
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }

    [TestMethod]
    public async Task WhenLifecycleCallbacksRunThenRealCloseAndReleaseDrainBeforeCompletion()
    {
        string root = Path.Combine(AppContext.BaseDirectory, "Results", "independent-media-stage12");
        Directory.CreateDirectory(root);
        string input = Path.Combine(root, "source.avi");
        WriteSourceReaderAvi(input);
        foreach (string mode in new[] { "callback-close", "callback-release", "external-close", "callback-shutdown", "callback-exit", "external-shutdown", "opened-shutdown", "opened-exit" })
        {
            IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
            ControlEvents events = new(identity);
            ConcurrentQueue<string> order = new();
            using ManualResetEventSlim exitCallback = new();
            TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
            TaskCompletionSource returned = new(TaskCreationOptions.RunContinuationsAsynchronously);
            nint proxy = CreateControlProxy(events);
            Assert.AreEqual(0, identity.AttachEvents(proxy));
            Assert.AreEqual(1u, ReleaseControlProxy(proxy));
            IndependentVideoPlaybackOwner? owner = null;
            SystemVideoSample? lease = null;
            int accepted = -1, afterClose = -1;
            uint references = uint.MaxValue;
            bool passed = false, pendingAtAcceptance = false;
            int closeFrames = -1;
            string? leasePixels = null;
            int lateWindowMilliseconds = 250;
            int latePackets = -1;
            int triggered = 0;
            bool exitLeftOwnerRunning = false;
            int packetsAtAcceptance = -1;
            events.OnEvent = kind =>
            {
                bool openedCase = mode.StartsWith("opened-", StringComparison.Ordinal);
                if (openedCase) owner = identity.Owner;
                if ((openedCase ? kind != 1 : kind != 13 || owner is null || owner.LastTimestamp < 5000000)
                    || Interlocked.Exchange(ref triggered, 1) != 0) return;
                order.Enqueue("callback-enter");
                if (!openedCase)
                {
                    Assert.AreEqual(0, owner!.Frames.Acquire(out lease));
                    Assert.IsNotNull(lease);
                }
                if (mode == "callback-release")
                {
                    references = IdentityRef(identity.Pointer, 2);
                    accepted = 0;
                }
                else if (mode == "callback-close") accepted = IdentityCall(identity.Pointer, 5);
                else if (mode is "callback-shutdown" or "opened-shutdown") accepted = IdentityCall(identity.Pointer, 22);
                else if (mode is "callback-exit" or "opened-exit")
                {
                    accepted = IdentityCall(identity.Pointer, 23);
                    exitLeftOwnerRunning = owner is not null && !owner.ThreadExited;
                }
                entered.TrySetResult();
                // Fixed handshake, never random timing or a synchronous close wait on this thread.
                if (!exitCallback.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException();
                order.Enqueue("callback-exit");
                returned.TrySetResult();
            };
            try
            {
                Assert.AreEqual(0, IdentityRate(identity.Pointer, 0));
                Assert.AreEqual(0, IdentityOpen(identity.Pointer, input));
                // Receive completes Opened only after OnEvent returns; opened-case callbacks deliberately block there.
                await (mode.StartsWith("opened-", StringComparison.Ordinal) ? entered.Task : events.Opened.Task)
                    .WaitAsync(TimeSpan.FromSeconds(10));
                if (!mode.StartsWith("opened-", StringComparison.Ordinal))
                {
                    await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                    owner = identity.Owner;
                    Assert.IsNotNull(owner);
                    Assert.AreEqual(0, IdentityRate(identity.Pointer, 1));
                }
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                if (mode == "external-close")
                {
                    int[] closes = await Task.WhenAll(Task.Run(() => IdentityCall(identity.Pointer, 5)), Task.Run(() => IdentityCall(identity.Pointer, 5)));
                    CollectionAssert.AreEqual(new[] { 0, 0 }, closes);
                    accepted = closes[0];
                }
                if (mode == "external-shutdown") accepted = await Task.Run(() => IdentityCall(identity.Pointer, 22));
                if (mode is "callback-exit" or "opened-exit")
                {
                    Assert.IsTrue(exitLeftOwnerRunning);
                    Assert.AreEqual(0, IdentityCall(identity.Pointer, 23));
                    Assert.AreEqual(0, IdentityCall(identity.Pointer, 22));
                }
                packetsAtAcceptance = events.Packets.Count;
                order.Enqueue("close-accepted");
                pendingAtAcceptance = !identity.Completion.IsCompleted;
                Assert.IsTrue(pendingAtAcceptance);
                closeFrames = owner.PublishedFrames;
                exitCallback.Set();
                await returned.Task.WaitAsync(TimeSpan.FromSeconds(10));
                await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                order.Enqueue("owner-completed");
                Assert.AreEqual(0, accepted);
                Assert.AreEqual(closeFrames, owner.PublishedFrames);
                Assert.IsTrue(owner.ThreadExited);
                Assert.AreEqual(IndependentMediaState.Closed, identity.State);
                Assert.AreEqual(mode is not ("callback-close" or "external-close"), events.Disposed);
                Assert.AreEqual(packetsAtAcceptance, events.Packets.Count);
                if (!mode.StartsWith("opened-", StringComparison.Ordinal))
                {
                    Assert.IsNotNull(lease);
                    Assert.IsTrue(lease.Source != 0);
                    leasePixels = ReadClosedLease(lease);
                    Assert.AreEqual("0000FFFF0000FFFF", leasePixels);
                }
                int packetsAtClose = events.Packets.Count;
                await Task.Delay(lateWindowMilliseconds);
                latePackets = events.Packets.Count - packetsAtClose;
                Assert.AreEqual(0, latePackets);
                afterClose = owner.Frames.Acquire(out _);
                Assert.AreEqual(unchecked((int)0x80000013), afterClose);
                Assert.IsNotNull(owner.ClosedReader);
                Assert.AreEqual((1, 1), (owner.ClosedReader.ShutdownCount, owner.ClosedReader.UninitializeCount));
                passed = true;
            }
            finally
            {
                exitCallback.Set();
                if (mode != "callback-release" || references != 0) IdentityCall(identity.Pointer, 5);
                await identity.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                lease?.Dispose();
                if (mode != "callback-release" || references != 0) references = IdentityRef(identity.Pointer, 2);
                File.WriteAllText(Path.Combine(root, mode + ".json"), JsonSerializer.Serialize(new
                {
                    mode, passed, accepted, pendingAtAcceptance, afterClose, closeFrames,
                    exitLeftOwnerRunning, packetsAtAcceptance, observerComplete = identity.Completion.IsCompleted,
                    leasePixels, lateWindowMilliseconds, latePackets,
                    publishedFrames = owner?.PublishedFrames, threadExited = owner?.ThreadExited,
                    state = identity.State.ToString(), references,
                    registryRemoved = IndependentMediaIdentity.Resolve(identity.Pointer) is null,
                    descriptorDisposed = events.Disposed, callbackOrder = order.ToArray(),
                    packets = events.Packets.ToArray(),
                    cleanup = owner?.ClosedReader?.Cleanup.ToArray(),
                    sampleReleases = owner?.ClosedReader?.SampleReleaseCount,
                    bufferReleases = owner?.ClosedReader?.BufferReleaseCount,
                    shutdownCount = owner?.ClosedReader?.ShutdownCount,
                    uninitializeCount = owner?.ClosedReader?.UninitializeCount,
                    readerFinalReferences = owner?.ClosedReader?.ReaderFinalReferences,
                    nativeReferences = "Unknown",
                    runtime = RuntimeInformation.FrameworkDescription,
                    architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                    testSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MediaFoundationResourceTests).Assembly.Location))),
                    testMvid = typeof(MediaFoundationResourceTests).Assembly.ManifestModule.ModuleVersionId,
                    backend = "source-linked production identity/eventproxy/synchronous MF owner; not current NativeAOT media execution"
                }, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}
