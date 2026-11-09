using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaNullPresenceTests
{
    /// <summary>Reflection assembles initial metadata; the actual proxy callback drain controls completion, not Native decode interleaving.</summary>
    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenNullSourceIsSubmittedThenPresenceSurvivesUntilDrain(bool completed)
    {
        using var fixture = new PendingSource();
        Assert.AreEqual(0, Open(fixture.Media, null));
        if (completed) fixture.Drain();
        Assert.AreEqual((0, completed ? (byte)0 : (byte)1), Presence(fixture.Media));
    }

    /// <summary>A repeated null does not replace the pending commit or clear its cached presence.</summary>
    [TestMethod]
    public void WhenNullIsRepeatedThenPendingCommitIsUnchanged()
    {
        using var fixture = new PendingSource();
        Open(fixture.Media, null);
        Task pending = fixture.Media.Completion;
        Open(fixture.Media, null);
        Assert.AreEqual((pending, (0, (byte)1)), (fixture.Media.Completion, Presence(fixture.Media)));
    }

    /// <summary>Reflection publishes replacement metadata while the old real callback is admitted; this is not a Native interleaving proof.</summary>
    [TestMethod]
    public void WhenNewSourcePrecedesOldDrainThenOldCommitCannotClearNewPresence()
    {
        using var fixture = new PendingSource();
        Open(fixture.Media, null);
        Task oldCommit = fixture.Media.Completion;
        Open(fixture.Media, "replacement-source");
        var owner = fixture.Media.Owner!;
        // Keep the worker's metadata publication outside the deterministic assertion window.
        fixture.Media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
        Set(owner, "Width", 2u);
        Set(owner, "Height", 1u);
        typeof(IndependentMediaIdentity).GetMethod("PublishMetadata", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(fixture.Media, [owner]);
        fixture.Drain();
        oldCommit.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
        Assert.AreEqual((0, (byte)1), Presence(fixture.Media));
    }

    /// <summary>The initial null source remains a synchronous no-op.</summary>
    [TestMethod]
    public void WhenInitialNullIsRepeatedThenCompletionRemainsSynchronous()
    {
        var media = IndependentMediaIdentity.Create();
        try
        {
            Task completion = media.Completion;
            Open(media, null);
            Open(media, null);
            Assert.AreEqual((completion, true, (0, (byte)0)), (media.Completion, media.Completion.IsCompletedSuccessfully, Presence(media)));
        }
        finally { Release(media); }
    }

    /// <summary>Submitting the same source preserves both completion and presence.</summary>
    [TestMethod]
    public void WhenSameSourceIsSubmittedThenPresenceAndCompletionAreUnchanged()
    {
        using var fixture = new PendingSource();
        Task completion = fixture.Media.Completion;
        Open(fixture.Media, "assembled-source");
        Assert.AreEqual((completion, (0, (byte)1)), (fixture.Media.Completion, Presence(fixture.Media)));
    }

    private sealed class PendingSource : IDisposable
    {
        internal readonly IndependentMediaIdentity Media = IndependentMediaIdentity.Create();
        internal readonly ManualResetEventSlim Entered = new();
        internal readonly ManualResetEventSlim Proceed = new();
        private readonly EventProxy _proxy;
        private readonly MediaEventProxy _events;

        internal PendingSource()
        {
            GCHandle root = GCHandle.Alloc(this);
            EventProxyDescriptor descriptor = new() { Dispose = &DisposeHandle, RaiseEvent = &Receive, Handle = (nuint)GCHandle.ToIntPtr(root) };
            EventProxy.Create(descriptor, out EventProxy? proxy);
            _proxy = proxy!;
            MediaEventProxy.Create(_proxy, out MediaEventProxy? events);
            _events = events!;
            Field(Media, "_path", "assembled-source");
            Field(Media, "_hasVideo", true);
            Field(Media, "_events", _events);
            var owner = new IndependentVideoPlaybackOwner();
            Set(owner, "Width", 2u);
            Set(owner, "Height", 1u);
            Field(Media, "_owner", owner);
            Set(Media, "State", IndependentMediaState.Opened);
            Assert.AreEqual(0, _events.RaiseEvent(1));
            Assert.IsTrue(Entered.Wait(TimeSpan.FromSeconds(10)));
        }

        internal void Drain()
        {
            Proceed.Set();
            _events.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            // The clear-source continuation may still be acquiring the identity lock.
            Task cleanup = (Task)typeof(IndependentMediaIdentity).GetField("_cleanup", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(Media)!;
            cleanup.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Proceed.Set();
            _events.Dispose();
            Release(Media);
            Media.Completion.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            _proxy.Release();
            Entered.Dispose();
            Proceed.Dispose();
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Receive(EventProxyDescriptor* descriptor, byte* data, uint size)
    {
        var fixture = (PendingSource)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!;
        fixture.Entered.Set();
        return fixture.Proceed.Wait(TimeSpan.FromSeconds(10)) ? 0 : unchecked((int)0x800705B4);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void DisposeHandle(EventProxyDescriptor* descriptor) => GCHandle.FromIntPtr((nint)descriptor->Handle).Free();

    private static void Field(object target, string name, object value)
        => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

    private static void Set(object target, string name, object value)
        => target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(target, value);

    private static int Open(IndependentMediaIdentity media, string? path)
    {
        nint self = media.Pointer;
        fixed (char* text = path) return ((delegate* unmanaged[Stdcall]<nint, char*, int>)(*(nint**)self)[3])(self, text);
    }

    private static (int, byte) Presence(IndependentMediaIdentity media)
    {
        byte value = 0;
        nint self = media.Pointer;
        int result = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)(*(nint**)self)[16])(self, &value);
        return (result, value);
    }

    private static void Release(IndependentMediaIdentity media)
        => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)media.Pointer)[2])(media.Pointer);
}
