using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe partial class ManagedStreamTests
{
    private delegate* unmanaged[Stdcall]<Descriptor*, nint*, int> _create;
    private delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int> _write;
    public TestContext TestContext { get; set; } = null!;

    [TestInitialize]
    public void LoadPublishedLibrary()
    {
        string path = typeof(ManagedStreamTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "AotDllPath").Value!;
        using (var file = File.OpenRead(path))
        using (var pe = new PEReader(file))
        {
            Assert.IsNull(pe.PEHeaders.CorHeader);
            Assert.AreEqual(RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => Machine.Amd64,
                Architecture.Arm64 => Machine.Arm64,
                Architecture.X86 => Machine.I386,
                _ => throw new PlatformNotSupportedException()
            }, pe.PEHeaders.CoffHeader.Machine);
        }
        TestContext.WriteLine($"{path}; SHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}");
        nint module = NativeLibrary.Load(path);
        Assert.IsTrue(NativeLibrary.TryGetExport(module, "MILCreateStreamFromStreamDescriptor", out nint entry));
        _create = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)entry;
        Assert.IsTrue(NativeLibrary.TryGetExport(module, "MILIStreamWrite", out entry));
        _write = (delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int>)entry;
    }

    [TestMethod]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(true, true)]
    public void WhenCreateArgumentIsNullThenCallerRetainsDescriptor(bool nullDescriptor, bool nullOutput)
    {
        State state = default;
        Descriptor descriptor = MakeDescriptor(&state);
        nint output = -1;
        int result = _create(nullDescriptor ? null : &descriptor, nullOutput ? null : &output);
        Assert.AreEqual((unchecked((int)0x80004003), (nint)(-1), 0), (result, output, state.Disposals));
    }

    [TestMethod]
    [DataRow("00000000-0000-0000-C000-000000000046")]
    [DataRow("0000000c-0000-0000-C000-000000000046")]
    [DataRow("3a55501a-bdcc-4e63-96bc-4ddb6f44ccdd")]
    public void WhenSupportedInterfaceIsQueriedThenReferenceSurvivesOriginalRelease(string interfaceId)
    {
        State state = default;
        nint instance = Create(&state), queried = 0, firstIdentity = 0, secondIdentity = 0;
        try
        {
            Guid iid = new(interfaceId), unknown = new("00000000-0000-0000-C000-000000000046");
            Assert.AreEqual(0, Query(instance, &iid, &queried));
            Assert.AreEqual(0, Query(instance, &unknown, &firstIdentity));
            Assert.AreEqual(0, Query(queried, &unknown, &secondIdentity));
            Assert.AreEqual(firstIdentity, secondIdentity);
            Release(instance);
            instance = 0;
            Assert.AreEqual(0, state.Disposals);
        }
        finally { Release(secondIdentity); Release(firstIdentity); Release(queried); Release(instance); }
        Assert.AreEqual(1, state.Disposals);
    }

    [TestMethod]
    [DataRow("0c733a30-2a1c-11ce-ade5-00aa0044773d")]
    [DataRow("3b438dac-2650-4907-9364-367045e534bf")]
    public void WhenInterfaceIsUnsupportedThenOutputIsCleared(string interfaceId)
    {
        State state = default;
        nint instance = Create(&state), output = -1;
        try
        {
            Guid iid = new(interfaceId);
            Assert.AreEqual((unchecked((int)0x80004002), (nint)0), (Query(instance, &iid, &output), output));
        }
        finally { Release(instance); }
        Assert.AreEqual(1, state.Disposals);
    }

    [TestMethod]
    public void WhenQueryOutputIsNullThenInvalidArgumentIsReturned()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            Guid iid = new("00000000-0000-0000-C000-000000000046");
            Assert.AreEqual(unchecked((int)0x80070057), Query(instance, &iid, null));
        }
        finally { Release(instance); }
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(unchecked((int)0x80004005))]
    public void WhenAllSlotsAreInvokedThenArgumentsAndHResultsAreForwarded(int expected)
    {
        State state = new() { Result = expected };
        Descriptor descriptor = MakeDescriptor(&state);
        nint instance = 0;
        Assert.AreEqual(0, _create(&descriptor, &instance));
        descriptor = default;
        try
        {
            void** table = *(void***)instance;
            byte data = 0xA5;
            uint count = 0;
            ulong position = 0, written = 0;
            const ulong large = 0x123456789abcdef0;
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int>)table[3])(instance, &data, 1, &count));
            Assert.AreEqual((3, (nint)(&data), 1ul, 1u, (byte)0x5A), (state.Slot, state.Pointer, state.First, count, data));
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int>)table[4])(instance, &data, 1, &count));
            Assert.AreEqual((4, (nint)(&data), 1ul, 1u, (byte)0x5A), (state.Slot, state.Pointer, state.First, count, state.Byte));
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, long, uint, ulong*, int>)table[5])(instance, -0x123456789L, 2, &position));
            Assert.AreEqual((5, unchecked((ulong)(-0x123456789L)), 2u, large), (state.Slot, state.First, state.Flags, position));
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, ulong, int>)table[6])(instance, large));
            Assert.AreEqual((6, large), (state.Slot, state.First));
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, nint, ulong, ulong*, ulong*, int>)table[7])(instance, instance, large, &position, &written));
            Assert.AreEqual((7, instance, large, 7ul, 8ul), (state.Slot, state.Pointer, state.First, position, written));
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, uint, int>)table[8])(instance, 9));
            Assert.AreEqual((8, 9u), (state.Slot, state.Flags));
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, int>)table[9])(instance));
            Assert.AreEqual(9, state.Slot);
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, ulong, ulong, uint, int>)table[10])(instance, large, 123, 4));
            Assert.AreEqual((10, large, 123ul, 4u), (state.Slot, state.First, state.Second, state.Flags));
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, ulong, ulong, uint, int>)table[11])(instance, large, 456, 5));
            Assert.AreEqual((11, large, 456ul, 5u), (state.Slot, state.First, state.Second, state.Flags));
            byte* stat = stackalloc byte[80];
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, void*, uint, int>)table[12])(instance, stat, 1));
            Assert.AreEqual((12, (nint)stat, 1u, (byte)0x7B), (state.Slot, state.Pointer, state.Flags, stat[0]));
            nint clone = -1;
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)table[13])(instance, &clone));
            Assert.AreEqual((13, (nint)0), (state.Slot, clone));
            int capability = 0;
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, int*, int>)table[14])(instance, &capability));
            Assert.AreEqual((14, 1), (state.Slot, capability));
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, int*, int>)table[15])(instance, &capability));
            Assert.AreEqual((15, 2, 13, 0), (state.Slot, capability, state.Calls, state.Disposals));
        }
        finally { Release(instance); }
        Assert.AreEqual(1, state.Disposals);
    }

    [TestMethod]
    public void WhenOptionalOutputsAreNullThenCallbacksReceiveWritableStorage()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            void** table = *(void***)instance;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int>)table[4])(instance, null, 0, null));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, long, uint, ulong*, int>)table[5])(instance, 0, 0, null));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, ulong, ulong*, ulong*, int>)table[7])(instance, instance, 0, null, null));
            Assert.AreEqual((3, 0), (state.Calls, state.NullOutputs));
        }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenReadCountIsNullThenItIsPassedThroughUnchanged()
    {
        State state = default;
        nint instance = Create(&state);
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int>)(*(void***)instance)[3])(instance, null, 0, null));
            Assert.AreEqual(1, state.NullOutputs);
        }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenExportWritesThenRealStreamCallbackReceivesPayload()
    {
        State state = new() { Result = 1 };
        nint instance = Create(&state);
        try
        {
            byte value = 0xFF;
            Assert.AreEqual(1, _write(instance, &value, 1, null));
            Assert.AreEqual((4, (byte)0xFF, 0), (state.Slot, state.Byte, state.NullOutputs));
        }
        finally { Release(instance); }
    }

    [TestMethod]
    public void WhenExportStreamIsNullThenPointerErrorIsReturned()
        => Assert.AreEqual(unchecked((int)0x80004003), _write(0, null, 0, null));

    [TestMethod]
    public void WhenAdditionalReferenceExistsThenOnlyFinalReleaseDisposes()
    {
        State state = default;
        nint instance = Create(&state);
        _ = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[1])(instance);
        Release(instance);
        try { Assert.AreEqual(0, state.Disposals); }
        finally { Release(instance); }
        Assert.AreEqual(1, state.Disposals);
    }

    private nint Create(State* state)
    {
        Descriptor descriptor = MakeDescriptor(state);
        nint instance = 0;
        Assert.AreEqual(0, _create(&descriptor, &instance));
        return instance;
    }

    private static int Query(nint instance, Guid* iid, nint* output)
        => ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)instance)[0])(instance, iid, output);
    private static void Release(nint instance)
    {
        if (instance != 0) _ = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)instance)[2])(instance);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        internal delegate* unmanaged[Stdcall]<Descriptor*, void> Dispose;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, uint*, int> Read;
        internal delegate* unmanaged[Stdcall]<Descriptor*, long, uint, ulong*, int> Seek;
        internal delegate* unmanaged[Stdcall]<Descriptor*, void*, uint, int> Stat;
        internal delegate* unmanaged[Stdcall]<Descriptor*, byte*, uint, uint*, int> Write;
        internal delegate* unmanaged[Stdcall]<Descriptor*, nint, ulong, ulong*, ulong*, int> CopyTo;
        internal delegate* unmanaged[Stdcall]<Descriptor*, ulong, int> SetSize;
        internal delegate* unmanaged[Stdcall]<Descriptor*, uint, int> Commit;
        internal delegate* unmanaged[Stdcall]<Descriptor*, int> Revert;
        internal delegate* unmanaged[Stdcall]<Descriptor*, ulong, ulong, uint, int> Lock, Unlock;
        internal delegate* unmanaged[Stdcall]<Descriptor*, nint*, int> Clone;
        internal delegate* unmanaged[Stdcall]<Descriptor*, int*, int> CanWrite, CanSeek;
        internal State* Handle;
    }
    private struct State
    {
        internal int Result, Slot, Calls, Disposals, NullOutputs;
        internal nint Pointer;
        internal ulong First, Second;
        internal uint Flags;
        internal byte Byte;
    }
    private static Descriptor MakeDescriptor(State* state) => new()
    {
        Dispose = &DisposeCallback, Read = &ReadCallback, Seek = &SeekCallback, Stat = &StatCallback,
        Write = &WriteCallback, CopyTo = &CopyCallback, SetSize = &SizeCallback, Commit = &CommitCallback,
        Revert = &RevertCallback, Lock = &LockCallback, Unlock = &UnlockCallback, Clone = &CloneCallback,
        CanWrite = &CanWriteCallback, CanSeek = &CanSeekCallback, Handle = state
    };
    private static int Record(Descriptor* descriptor, int slot)
    {
        descriptor->Handle->Slot = slot;
        descriptor->Handle->Calls++;
        return descriptor->Handle->Result;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void DisposeCallback(Descriptor* d) => d->Handle->Disposals++;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int ReadCallback(Descriptor* d, byte* b, uint n, uint* output)
    {
        d->Handle->Pointer = (nint)b; d->Handle->First = n;
        if (n != 0) b[0] = 0x5A;
        if (output != null) *output = n; else d->Handle->NullOutputs++;
        return Record(d, 3);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int WriteCallback(Descriptor* d, byte* b, uint n, uint* output)
    {
        d->Handle->Pointer = (nint)b; d->Handle->First = n;
        if (n != 0) d->Handle->Byte = b[0];
        if (output != null) *output = n; else d->Handle->NullOutputs++;
        return Record(d, 4);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SeekCallback(Descriptor* d, long offset, uint origin, ulong* output)
    {
        d->Handle->First = unchecked((ulong)offset); d->Handle->Flags = origin;
        if (output != null) *output = 0x123456789abcdef0; else d->Handle->NullOutputs++;
        return Record(d, 5);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int StatCallback(Descriptor* d, void* stat, uint flags)
    {
        d->Handle->Pointer = (nint)stat; d->Handle->Flags = flags; *(byte*)stat = 0x7B;
        return Record(d, 12);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CopyCallback(Descriptor* d, nint target, ulong n, ulong* read, ulong* written)
    {
        d->Handle->Pointer = target; d->Handle->First = n;
        if (read != null) *read = 7; else d->Handle->NullOutputs++;
        if (written != null) *written = 8; else d->Handle->NullOutputs++;
        return Record(d, 7);
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int SizeCallback(Descriptor* d, ulong size) { d->Handle->First = size; return Record(d, 6); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CommitCallback(Descriptor* d, uint flags) { d->Handle->Flags = flags; return Record(d, 8); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int RevertCallback(Descriptor* d) => Record(d, 9);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockCallback(Descriptor* d, ulong offset, ulong count, uint flags)
    { d->Handle->First = offset; d->Handle->Second = count; d->Handle->Flags = flags; return Record(d, 10); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockCallback(Descriptor* d, ulong offset, ulong count, uint flags)
    { d->Handle->First = offset; d->Handle->Second = count; d->Handle->Flags = flags; return Record(d, 11); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CloneCallback(Descriptor* d, nint* output) { *output = 0; return Record(d, 13); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CanWriteCallback(Descriptor* d, int* output) { *output = 1; return Record(d, 14); }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CanSeekCallback(Descriptor* d, int* output) { *output = 2; return Record(d, 15); }
}
