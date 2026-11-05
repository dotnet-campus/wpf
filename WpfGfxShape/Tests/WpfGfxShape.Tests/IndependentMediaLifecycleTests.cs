using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaLifecycleTests
{
    private sealed class Observation
    {
        internal required IndependentMediaIdentity Media;
        internal bool Release;
        internal uint Result;
        internal bool Returned;
        internal ManualResetEventSlim? Entered;
        internal ManualResetEventSlim? Continue;
        internal int Calls;
    }

    [TestMethod]
    public void WhenProxyClosesWithAnAdmittedCallbackThenQueuedAndLateEventsAreRejected()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        using ManualResetEventSlim entered = new();
        using ManualResetEventSlim proceed = new();
        Observation observation = new() { Media = media, Entered = entered, Continue = proceed };
        GCHandle root = GCHandle.Alloc(observation);
        EventProxyDescriptor descriptor = new() { Dispose = &Dispose, RaiseEvent = &Receive, Handle = (nuint)GCHandle.ToIntPtr(root) };
        Assert.AreEqual(0, EventProxy.Create(descriptor, out EventProxy? proxy));
        Assert.AreEqual(0, MediaEventProxy.Create(proxy!, out MediaEventProxy? events));
        try
        {
            Assert.AreEqual(0, events!.RaiseEvent(1));
            Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(0, events.RaiseEvent(13));
            events.Dispose();
            Assert.IsFalse(events.Completion.IsCompleted);
            Assert.IsTrue(events.RaiseEvent(13) < 0);
            proceed.Set();
            events.Completion.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            Assert.AreEqual(1, observation.Calls);
        }
        finally
        {
            proceed.Set();
            events!.Dispose();
            proxy!.Release();
            Release(media.Pointer);
        }
    }

    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenCallbackClosesOrReleasesIdentityThenCallbackReturnsWithoutSelfJoin(bool release)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        Observation observation = new() { Media = media, Release = release };
        GCHandle root = GCHandle.Alloc(observation);
        EventProxyDescriptor descriptor = new() { Dispose = &Dispose, RaiseEvent = &Receive, Handle = (nuint)GCHandle.ToIntPtr(root) };
        Assert.AreEqual(0, EventProxy.Create(descriptor, out EventProxy? proxy));
        try
        {
            Assert.AreEqual(0, proxy!.RaiseEvent(ReadOnlySpan<byte>.Empty));
            media.Completion.GetAwaiter().GetResult();
            Assert.AreEqual((true, 0u, IndependentMediaState.Closed), (observation.Returned, observation.Result, media.State));
        }
        finally
        {
            proxy!.Release();
            if (!release) Release(media.Pointer);
        }
    }

    [TestMethod]
    public void WhenTwoExternalClosersRaceThenBothAreAcceptedAndNewOperationsAreRejected()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        using Barrier start = new(3);
        try
        {
            Task<int> first = Task.Run(() => { start.SignalAndWait(); return Close(media.Pointer); });
            Task<int> second = Task.Run(() => { start.SignalAndWait(); return Close(media.Pointer); });
            start.SignalAndWait();
            Task.WaitAll(first, second);
            media.Completion.GetAwaiter().GetResult();
            nint* table = *(nint**)media.Pointer;
            int rate = ((delegate* unmanaged[Stdcall]<nint, double, int>)table[8])(media.Pointer, 1);
            Assert.AreEqual((0, 0, unchecked((int)0x80000013), IndependentMediaState.Closed), (first.Result, second.Result, rate, media.State));
        }
        finally { Release(media.Pointer); }
    }

    [DataTestMethod]
    [DataRow(22)]
    [DataRow(23)]
    public void WhenTerminalSlotRunsThenItIsDistinctAndControlsAreRejected(int slot)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            nint* table = *(nint**)media.Pointer;
            Assert.AreNotEqual(table[5], table[22]);
            Assert.AreNotEqual(table[22], table[23]);
            Assert.AreEqual(0, Call(media.Pointer, slot));
            Assert.AreEqual(0, Call(media.Pointer, slot));
            Assert.AreEqual(unchecked((int)0x80000013), ((delegate* unmanaged[Stdcall]<nint, double, int>)table[8])(media.Pointer, 1));
            Assert.AreEqual(unchecked((int)0x80000013), ((delegate* unmanaged[Stdcall]<nint, long, int>)table[7])(media.Pointer, 0));
            long position = 42;
            Assert.AreEqual(unchecked((int)0x80000013), ((delegate* unmanaged[Stdcall]<nint, long*, int>)table[6])(media.Pointer, &position));
            Assert.AreEqual(0L, position);
            fixed (char* path = "unused.avi")
                Assert.AreEqual(unchecked((int)0x80000013), ((delegate* unmanaged[Stdcall]<nint, char*, int>)table[3])(media.Pointer, path));
            Assert.AreEqual(0, Call(media.Pointer, 22));
            Assert.AreEqual(0, Close(media.Pointer));
            media.Completion.GetAwaiter().GetResult();
        }
        finally { Assert.AreEqual(0u, Release(media.Pointer)); }
        Assert.IsNull(IndependentMediaIdentity.Resolve(media.Pointer));
    }

    /// <summary>Verifies a terminal identity cannot acquire a new proxy registration.</summary>
    [DataTestMethod]
    [DataRow(22)]
    [DataRow(23)]
    public void WhenTerminalIdentityRebindsThenRegistrationIsRejected(int slot)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        Observation observation = new() { Media = media };
        GCHandle root = GCHandle.Alloc(observation);
        EventProxyDescriptor descriptor = new() { Dispose = &Dispose, RaiseEvent = &Receive, Handle = (nuint)GCHandle.ToIntPtr(root) };
        Assert.AreEqual(0, EventProxy.Create(descriptor, out EventProxy? proxy));
        try
        {
            Assert.AreEqual(0, Call(media.Pointer, slot));
            Assert.AreEqual(unchecked((int)0x80000013), media.AttachEvents(proxy!));
        }
        finally { proxy!.Release(); Release(media.Pointer); }
    }

    private static int Call(nint self, int slot) => ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)self)[slot])(self);
    private static int Close(nint self) => ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)self)[5])(self);
    private static uint Release(nint self) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(nint**)self)[2])(self);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Receive(EventProxyDescriptor* descriptor, byte* bytes, uint length)
    {
        Observation observation = (Observation)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!;
        Interlocked.Increment(ref observation.Calls);
        if (observation.Entered is not null)
        {
            observation.Entered.Set();
            return observation.Continue!.Wait(TimeSpan.FromSeconds(5)) ? 0 : unchecked((int)0x800705B4);
        }
        observation.Result = observation.Release ? Release(observation.Media.Pointer) : unchecked((uint)Close(observation.Media.Pointer));
        observation.Returned = true;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void Dispose(EventProxyDescriptor* descriptor) => GCHandle.FromIntPtr((nint)descriptor->Handle).Free();
}
