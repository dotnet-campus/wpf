using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class EventProxyTests
{
    private delegate* unmanaged[Stdcall]<Descriptor*, nint*, int> _create;
    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void LoadPublishedLibrary()
    {
        Assert.IsTrue(OperatingSystem.IsWindows());
        string path = typeof(EventProxyTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "AotDllPath").Value!;
        Assert.IsTrue(File.Exists(path), path);
        using (var stream = File.OpenRead(path))
        using (var pe = new PEReader(stream))
        {
            Assert.IsNull(pe.PEHeaders.CorHeader, path);
            Machine expected = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => Machine.Amd64,
                Architecture.Arm64 => Machine.Arm64,
                Architecture.X86 => Machine.I386,
                _ => throw new PlatformNotSupportedException()
            };
            Assert.AreEqual(expected, pe.PEHeaders.CoffHeader.Machine, path);
        }
        TestContext.WriteLine($"{path}; SHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}");
        nint module = NativeLibrary.Load(path);
        // Native AOT unloading is not assumed. Keep the module mapped for the test process lifetime.
        Assert.IsTrue(NativeLibrary.TryGetExport(module, "MILCreateEventProxy", out nint entry), "MILCreateEventProxy");
        _create = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)entry;
    }

    [TestMethod]
    public void WhenOutputIsNullThenCreateReturnsPointerError()
    {
        State state = default;
        Descriptor descriptor = MakeDescriptor(&state);
        Assert.AreEqual(unchecked((int)0x80004003), _create(&descriptor, null));
    }

    [TestMethod]
    public void WhenDescriptorIsNullThenCreateReturnsPointerError()
    {
        nint instance = 0;
        Assert.AreEqual(unchecked((int)0x80004003), _create(null, &instance));
    }

    [TestMethod]
    public void WhenQueryOutputIsNullThenPointerErrorIsReturned()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            Guid iid = new("00000000-0000-0000-C000-000000000046");
            Assert.AreEqual(unchecked((int)0x80004003), Query(instance, &iid, null));
        }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenIidIsUnknownThenOutputIsCleared()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            Guid iid = new("68b7e2de-0477-4edf-b3b2-453cdd40be87");
            nint output = -1;
            int hr = Query(instance, &iid, &output);
            Assert.AreEqual((unchecked((int)0x80004002), (nint)0), (hr, output));
        }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenInterfacesAreQueriedThenUnknownIdentityIsStable()
    {
        State state = default;
        nint instance = Create(&state), proxy = 0, identity = 0, second = 0;
        try
        {
            Guid iid = new("342efd8b-669a-4d16-b163-d75f5ffd1a10");
            Guid unknown = new("00000000-0000-0000-C000-000000000046");
            Assert.AreEqual(0, Query(instance, &iid, &proxy));
            Assert.AreNotEqual((nint)0, proxy);
            Assert.AreEqual(0, Query(instance, &unknown, &identity));
            Assert.AreNotEqual((nint)0, identity);
            Assert.AreEqual(0, Query(proxy, &unknown, &second));
            Assert.AreEqual(identity, second);
        }
        finally { Release(second); Release(identity); Release(proxy); Release(instance); }
    }

    [TestMethod]
    public void WhenEventIsRaisedThenBytesAndHResultCrossNativeBoundary()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            byte value = 42;
            int hr = Raise(instance, &value, 1);
            Assert.AreEqual((unchecked((int)0x80004005), 1, 1u, (byte)42), (hr, state.Calls, state.Length, state.First));
        }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenCallerDescriptorIsClearedThenNativeCopyRemainsValid()
    {
        State state = default;
        Descriptor descriptor = MakeDescriptor(&state);
        nint instance = 0;
        Assert.AreEqual(0, _create(&descriptor, &instance));
        Assert.AreNotEqual((nint)0, instance);
        descriptor = default;
        try { byte value = 42; _ = Raise(instance, &value, 1); }
        finally { Release(instance); }
        Assert.AreEqual((1, 1), (state.Calls, state.Disposals));
    }

    [TestMethod]
    public void WhenLastReferenceIsReleasedThenDisposeOccursExactlyOnce()
    {
        State state = default;
        nint instance = Create(&state);
        var addRef = (delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[1];
        _ = addRef(instance);
        Release(instance);
        int before = state.Disposals;
        Release(instance);
        Assert.AreEqual((0, 1), (before, state.Disposals));
    }

    [TestMethod]
    public void WhenCreateFailsThenCallerDescriptorIsNotDisposed()
    {
        State state = default;
        Descriptor descriptor = MakeDescriptor(&state);
        _ = _create(&descriptor, null);
        Assert.AreEqual(0, state.Disposals);
    }

    [TestMethod]
    public void WhenBothCreateArgumentsAreNullThenPointerErrorIsReturned()
    {
        Assert.AreEqual(unchecked((int)0x80004003), _create(null, null));
    }

    [TestMethod]
    [DataRow("00000000-0000-0000-C000-000000000046")]
    [DataRow("342efd8b-669a-4d16-b163-d75f5ffd1a10")]
    public void WhenQueryReferenceOutlivesCreatorThenObjectRemainsAlive(string interfaceId)
    {
        State state = default;
        nint instance = Create(&state), queried = 0;
        try
        {
            Guid iid = new(interfaceId);
            Assert.AreEqual(0, Query(instance, &iid, &queried));
            Assert.AreNotEqual((nint)0, queried);
            Release(instance);
            instance = 0;
            Assert.AreEqual(0, state.Disposals);
            // Both supported interfaces belong to this event proxy; query back to its event interface.
            Guid eventId = new("342efd8b-669a-4d16-b163-d75f5ffd1a10");
            nint eventInterface = 0;
            try
            {
                Assert.AreEqual(0, Query(queried, &eventId, &eventInterface));
                Assert.AreNotEqual((nint)0, eventInterface);
                byte value = 9;
                _ = Raise(eventInterface, &value, 1);
                Assert.AreEqual((1, (byte)9), (state.Calls, state.First));
            }
            finally { Release(eventInterface); }
        }
        finally { Release(queried); Release(instance); }
        Assert.AreEqual(1, state.Disposals);
    }

    [TestMethod]
    public void WhenQueryFailsThenItDoesNotRetainAnExtraReference()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            Guid iid = new("68b7e2de-0477-4edf-b3b2-453cdd40be87");
            nint output = 0;
            Assert.AreEqual(unchecked((int)0x80004002), Query(instance, &iid, &output));
        }
        finally { Release(instance); }
        Assert.AreEqual(1, state.Disposals);
    }

    [TestMethod]
    public void WhenQueryFailsThenSubsequentCallbackStillWorks()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            Guid iid = new("68b7e2de-0477-4edf-b3b2-453cdd40be87");
            nint output = 0;
            _ = Query(instance, &iid, &output);
            byte value = 7;
            _ = Raise(instance, &value, 1);
            Assert.AreEqual((1, (byte)7), (state.Calls, state.First));
        }
        finally { Release(instance); }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(unchecked((int)0x80070057))]
    [DataRow(unchecked((int)0x80004005))]
    public void WhenCallbackReturnsHResultThenExactValueIsPreserved(int expected)
    {
        State state = new() { UseResult = true, Result = expected };
        nint instance = Create(&state);
        try { Assert.AreEqual(expected, Raise(instance, null, 0)); }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenPayloadIsEmptyThenCallbackReceivesNullAndZeroLength()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            _ = Raise(instance, null, 0);
            Assert.AreEqual((1, 0u, (nint)0), (state.Calls, state.Length, state.Payload));
        }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenBinaryPayloadContainsZerosThenEveryByteIsPreserved()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            byte* bytes = stackalloc byte[] { 0, 255, 128, 1, 0, 42 };
            _ = Raise(instance, bytes, 6);
            Assert.AreEqual((6u, 426ul, (byte)42, (nint)bytes), (state.Length, state.Sum, state.Last, state.Payload));
            CollectionAssert.AreEqual(new byte[] { 0, 255, 128, 1, 0, 42 }, new ReadOnlySpan<byte>(state.Snapshot, 6).ToArray());
            CollectionAssert.AreEqual(new byte[] { 0, 255, 128, 1, 0, 42 }, new ReadOnlySpan<byte>(bytes, 6).ToArray());
        }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenCallbackFailsThenNextEventIsNotSuppressed()
    {
        State state = new() { UseResult = true, Result = unchecked((int)0x80004005) };
        nint instance = Create(&state);
        try
        {
            _ = Raise(instance, null, 0);
            state.Result = 0;
            int result = Raise(instance, null, 0);
            Assert.AreEqual((0, 2), (result, state.Calls));
        }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenOneObjectIsReleasedThenOtherObjectRemainsIndependent()
    {
        State first = default, second = default;
        nint one = Create(&first), two = 0;
        try
        {
            two = Create(&second);
            Assert.AreNotEqual(one, two);
            Release(one);
            one = 0;
            _ = Raise(two, null, 0);
            Assert.AreEqual((1, 0, 1), (first.Disposals, second.Disposals, second.Calls));
        }
        finally { Release(two); Release(one); }
        Assert.AreEqual(1, second.Disposals);
    }

    [TestMethod]
    public void WhenObjectIsNeverUsedThenFinalReleaseStillDisposesDescriptor()
    {
        State state = default;
        nint instance = Create(&state);
        Release(instance);
        Assert.AreEqual((0, 1), (state.Calls, state.Disposals));
    }

    [TestMethod]
    public void WhenCallbackQueriesAndReleasesThenIdentityAndLifetimeRemainValid()
    {
        State state = default;
        nint instance = Create(&state);
        state.ReentrantInstance = instance;
        try
        {
            _ = Raise(instance, null, 0);
            Assert.AreEqual((0, instance, 0), (state.QueryResult, state.QueryIdentity, state.Disposals));
        }
        finally { Release(instance); }
        Assert.AreEqual(1, state.Disposals);
    }

    [TestMethod]
    public void WhenAnotherOwnerReleasesDuringCallbackThenReleaseDoesNotWaitForCallback()
    {
        using var state = new ConcurrentState();
        GCHandle root = GCHandle.Alloc(state);
        nint instance = 0;
        Task<int>? raising = null;
        Task? releasing = null;
        bool extraReference = false;
        try
        {
            instance = CreateConcurrent(root);
            state.Instance = instance;
            state.BlockCallback = true;
            _ = AddReference(instance);
            extraReference = true;
            raising = Task.Run(() => Raise(instance, null, 0));
            Assert.IsTrue(state.Entered.Wait(TimeSpan.FromSeconds(10)), "Callback did not enter.");
            releasing = Task.Run(() => Release(instance));
            extraReference = false;
            Assert.IsTrue(releasing.Wait(TimeSpan.FromSeconds(5)), "A non-final Release waited for the callback lock.");
        }
        finally
        {
            state.Continue.Set();
            try
            {
                raising?.GetAwaiter().GetResult();
                releasing?.GetAwaiter().GetResult();
            }
            finally
            {
                if (extraReference) Release(instance);
                Release(instance);
                root.Free();
            }
        }
        Assert.AreEqual((1, 1, 0, 0), (state.Calls, state.Disposals, state.Errors, state.Active));
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(4)]
    [DataRow(8)]
    public void WhenConcurrentOwnersRaiseAndReleaseThenFinalOwnerDisposesExactlyOnce(int workerCount)
    {
        using var state = new ConcurrentState();
        GCHandle root = GCHandle.Alloc(state);
        nint instance = 0;
        var workers = new List<Task<int>>();
        try
        {
            instance = CreateConcurrent(root);
            state.Instance = instance;
            for (int index = 0; index < workerCount; index++)
            {
                _ = AddReference(instance);
                try
                {
                    workers.Add(Task.Run(() =>
                    {
                        try
                        {
                            state.Continue.Wait();
                            return Raise(instance, null, 0);
                        }
                        finally { Release(instance); }
                    }));
                }
                catch
                {
                    Release(instance);
                    throw;
                }
            }
            state.Continue.Set();
            Task.WaitAll(workers.ToArray());
            CollectionAssert.AreEqual(Enumerable.Repeat(0, workerCount).ToArray(), workers.Select(worker => worker.Result).ToArray());
            Assert.AreEqual((workerCount, 0, 0, 0), (state.Calls, state.Disposals, state.Errors, state.Active));
        }
        finally
        {
            state.Continue.Set();
            try { Task.WaitAll(workers.ToArray()); }
            finally
            {
                Release(instance);
                root.Free();
            }
        }
        Assert.AreEqual(1, state.Disposals);
    }

    private nint CreateConcurrent(GCHandle root)
    {
        Descriptor descriptor = new() { Dispose = &OnConcurrentDispose, Raise = &OnConcurrentRaise, Handle = (nuint)GCHandle.ToIntPtr(root) };
        nint instance = 0;
        Assert.AreEqual(0, _create(&descriptor, &instance));
        return instance;
    }

    private static uint AddReference(nint instance)
        => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[1])(instance);

    private sealed class ConcurrentState : IDisposable
    {
        internal readonly ManualResetEventSlim Entered = new(false);
        internal readonly ManualResetEventSlim Continue = new(false);
        internal nint Instance;
        internal int Calls, Disposals, Errors, Active;
        internal bool BlockCallback;

        public void Dispose()
        {
            Entered.Dispose();
            Continue.Dispose();
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void OnConcurrentDispose(Descriptor* descriptor)
    {
        var state = (ConcurrentState)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!;
        if (Volatile.Read(ref state.Active) != 0) Interlocked.Increment(ref state.Errors);
        Interlocked.Increment(ref state.Disposals);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int OnConcurrentRaise(Descriptor* descriptor, byte* bytes, uint length)
    {
        var state = (ConcurrentState)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!;
        if (Interlocked.Increment(ref state.Active) != 1) Interlocked.Increment(ref state.Errors);
        Interlocked.Increment(ref state.Calls);
        Guid iid = new("00000000-0000-0000-C000-000000000046");
        nint identity = 0;
        int result = Query(state.Instance, &iid, &identity);
        if (result != 0 || identity != state.Instance) Interlocked.Increment(ref state.Errors);
        Release(identity);
        state.Entered.Set();
        if (state.BlockCallback && !state.Continue.Wait(TimeSpan.FromSeconds(15))) Interlocked.Increment(ref state.Errors);
        Interlocked.Decrement(ref state.Active);
        return 0;
    }

    private nint Create(State* state)
    {
        Descriptor descriptor = MakeDescriptor(state);
        nint instance = 0;
        Assert.AreEqual(0, _create(&descriptor, &instance));
        Assert.AreNotEqual((nint)0, instance);
        return instance;
    }

    private static Descriptor MakeDescriptor(State* state) => new() { Dispose = &OnDispose, Raise = &OnRaise, Handle = (nuint)state };
    private static int Query(nint instance, Guid* iid, nint* output)
        => ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)instance)[0])(instance, iid, output);
    private static int Raise(nint instance, byte* bytes, uint length)
        => ((delegate* unmanaged[Stdcall]<nint, byte*, uint, int>)(*(void***)instance)[3])(instance, bytes, length);
    private static void Release(nint instance)
    {
        if (instance != 0) _ = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal delegate* unmanaged[Stdcall]<Descriptor*, void> Dispose;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, int> Raise;
        internal nuint Handle;
    }
    private struct State
    {
        internal int Calls, Disposals;
        internal uint Length;
        internal byte First, Last;
        internal ulong Sum;
        internal nint Payload;
        internal bool UseResult;
        internal int Result;
        internal nint ReentrantInstance, QueryIdentity;
        internal int QueryResult;
        internal fixed byte Snapshot[6];
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void OnDispose(Descriptor* descriptor) => ((State*)descriptor->Handle)->Disposals++;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int OnRaise(Descriptor* descriptor, byte* bytes, uint length)
    {
        State* state = (State*)descriptor->Handle;
        state->Calls++;
        state->Length = length;
        state->First = length == 0 ? (byte)0 : bytes[0];
        state->Last = length == 0 ? (byte)0 : bytes[length - 1];
        state->Payload = (nint)bytes;
        state->Sum = 0;
        for (uint index = 0; index < length; index++)
        {
            state->Sum += bytes[index];
            if (index < 6) state->Snapshot[index] = bytes[index];
        }
        if (state->ReentrantInstance != 0)
        {
            Guid iid = new("00000000-0000-0000-C000-000000000046");
            nint identity = 0;
            state->QueryResult = Query(state->ReentrantInstance, &iid, &identity);
            state->QueryIdentity = identity;
            Release(identity);
        }
        return state->UseResult ? state->Result : unchecked((int)0x80004005);
    }
}
