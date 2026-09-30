using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class ManagedStreamTests
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public void WhenProductionStreamsCopyThenPayloadAndOptionalCountsArePreserved(bool omitRead, bool omitWritten)
    {
        var source = new MemorySource(_create);
        var target = new MemorySource(_create);
        nint first = 0, second = 0;
        try
        {
            Assert.AreEqual(0, source.Create(out first));
            Assert.AreEqual(0, target.Create(out second));
            WriteBytes(first, [0, 255, 128, 1, 0, 42]);
            SeekTo(first, 1, 0);
            ulong read = 99, written = 99;
            int result = ((delegate* unmanaged[Stdcall]<nint, nint, ulong, ulong*, ulong*, int>)(*(void***)first)[7])(
                first, second, 4, omitRead ? null : &read, omitWritten ? null : &written);
            Assert.AreEqual((0, omitRead ? 99ul : 4ul, omitWritten ? 99ul : 4ul), (result, read, written));
            SeekTo(second, 0, 0);
            CollectionAssert.AreEqual(new byte[] { 255, 128, 1, 0 }, ReadBytes(second, 4));
            Assert.AreEqual((5ul, 4ul), (SeekTo(first, 0, 1), SeekTo(second, 0, 1)));
        }
        finally { Release(second); Release(first); }
        Assert.AreEqual((1, 1), (source.Disposals, target.Disposals));
    }

    [TestMethod]
    public void WhenCopyExceedsSourceThenPartialCountsAndFalseAreReturned()
    {
        var source = new MemorySource(_create);
        var target = new MemorySource(_create);
        nint first = 0, second = 0;
        try
        {
            Assert.AreEqual(0, source.Create(out first));
            Assert.AreEqual(0, target.Create(out second));
            WriteBytes(first, [0, 255]);
            SeekTo(first, 0, 0);
            ulong read = 0, written = 0;
            int result = ((delegate* unmanaged[Stdcall]<nint, nint, ulong, ulong*, ulong*, int>)(*(void***)first)[7])(first, second, 9, &read, &written);
            Assert.AreEqual((1, 2ul, 2ul), (result, read, written));
            SeekTo(second, 0, 0);
            CollectionAssert.AreEqual(new byte[] { 0, 255 }, ReadBytes(second, 2));
        }
        finally { Release(second); Release(first); }
    }

    [TestMethod]
    public void WhenSizeAndPositionChangeThenFullStatAndBytesMatch()
    {
        var source = new MemorySource(_create);
        nint instance = 0;
        try
        {
            Assert.AreEqual(0, source.Create(out instance));
            WriteBytes(instance, [0, 255, 128, 42]);
            SeekTo(instance, -2, 2);
            WriteBytes(instance, [17]);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, ulong, int>)(*(void***)instance)[6])(instance, 6));
            SeekTo(instance, 0, 0);
            CollectionAssert.AreEqual(new byte[] { 0, 255, 17, 42, 0, 0 }, ReadBytes(instance, 6));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, ulong, int>)(*(void***)instance)[6])(instance, 3));
            byte* storage = stackalloc byte[88];
            new Span<byte>(storage, 88).Fill(0xCC);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, void*, uint, int>)(*(void***)instance)[12])(instance, storage, 1));
            STATSTG stat = Marshal.PtrToStructure<STATSTG>((nint)storage);
            Assert.AreEqual((null, 2, 3L, 2, 0, Guid.Empty, 0, 0),
                (stat.pwcsName, stat.type, stat.cbSize, stat.grfMode, stat.grfLocksSupported, stat.clsid, stat.grfStateBits, stat.reserved));
            Assert.AreEqual((101, 102, 201, 202, 301, 302),
                (stat.mtime.dwLowDateTime, stat.mtime.dwHighDateTime, stat.ctime.dwLowDateTime, stat.ctime.dwHighDateTime, stat.atime.dwLowDateTime, stat.atime.dwHighDateTime));
            Assert.AreEqual((byte)0xCC, storage[IntPtr.Size == 8 ? 80 : 72]);
        }
        finally { Release(instance); }
        Assert.AreEqual(1, source.Disposals);
    }

    [TestMethod]
    public void WhenOriginalIsReleasedThenRealCloneRetainsSnapshotAndIndependentPosition()
    {
        var source = new MemorySource(_create);
        nint instance = 0, clone = 0;
        try
        {
            Assert.AreEqual(0, source.Create(out instance));
            WriteBytes(instance, [0, 255, 128, 42]);
            SeekTo(instance, 1, 0);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)instance)[13])(instance, &clone));
            Assert.AreNotEqual(instance, clone);
            WriteBytes(instance, [17]);
            Assert.AreEqual(1ul, SeekTo(clone, 0, 1));
            Release(instance);
            instance = 0;
            Assert.AreEqual((1, 0), (source.Disposals, source.CloneSource!.Disposals));
            CollectionAssert.AreEqual(new byte[] { 255, 128, 42 }, ReadBytes(clone, 3));
            SeekTo(clone, 0, 0);
            WriteBytes(clone, [99]);
            SeekTo(clone, 0, 0);
            CollectionAssert.AreEqual(new byte[] { 99, 255, 128, 42 }, ReadBytes(clone, 4));
        }
        finally { Release(clone); Release(instance); }
        Assert.AreEqual((1, 1), (source.Disposals, source.CloneSource!.Disposals));
    }

    private static void WriteBytes(nint stream, byte[] bytes)
    {
        uint written = 0;
        fixed (byte* data = bytes)
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int>)(*(void***)stream)[4])(stream, data, (uint)bytes.Length, &written));
        Assert.AreEqual((uint)bytes.Length, written);
    }

    private static byte[] ReadBytes(nint stream, int count)
    {
        byte[] bytes = new byte[count];
        uint read = 0;
        fixed (byte* data = bytes)
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int>)(*(void***)stream)[3])(stream, data, (uint)count, &read));
        Assert.AreEqual((uint)count, read);
        return bytes;
    }

    internal static nint CreateCallbackStream(nint module, byte[] bytes, Action beforeRead, Action? disposed = null)
    {
        var create = (delegate* unmanaged[Stdcall]<Descriptor*, nint*, int>)NativeLibrary.GetExport(module, "MILCreateStreamFromStreamDescriptor");
        var source = new MemorySource(create) { BeforeRead = beforeRead, Disposed = disposed };
        source.Stream.Write(bytes);
        source.Stream.Position = 0;
        Marshal.ThrowExceptionForHR(source.Create(out nint stream));
        return stream;
    }

    private static ulong SeekTo(nint stream, long offset, uint origin)
    {
        ulong position = 0;
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, long, uint, ulong*, int>)(*(void***)stream)[5])(stream, offset, origin, &position));
        return position;
    }

    // This is a caller-owned descriptor source, not a replacement COM implementation.
    private sealed class MemorySource(delegate* unmanaged[Stdcall]<Descriptor*, nint*, int> create)
    {
        internal readonly MemoryStream Stream = new();
        internal int Disposals;
        internal Action? BeforeRead;
        internal Action? Disposed;
        internal MemorySource? CloneSource;
        private readonly delegate* unmanaged[Stdcall]<Descriptor*, nint*, int> _create = create;

        internal int Create(out nint instance)
        {
            GCHandle root = default;
            instance = 0;
            try
            {
                root = GCHandle.Alloc(this);
                Descriptor descriptor = new()
                {
                    Dispose = &MemoryDispose, Read = &MemoryRead, Write = &MemoryWrite,
                    Seek = &MemorySeek, SetSize = &MemorySize, Stat = &MemoryStat,
                    CopyTo = &MemoryCopy, Clone = &MemoryClone, CanWrite = &MemoryCapability, CanSeek = &MemoryCapability, Handle = (State*)GCHandle.ToIntPtr(root)
                };
                nint output = 0;
                int result = _create(&descriptor, &output);
                if (result >= 0)
                {
                    instance = output;
                    root = default;
                }
                else Stream.Dispose();
                return result;
            }
            catch
            {
                Stream.Dispose();
                throw;
            }
            finally { if (root.IsAllocated) root.Free(); }
        }

        internal int Clone(nint* output)
        {
            var copy = new MemorySource(_create);
            try
            {
                copy.Stream.Write(Stream.ToArray());
                copy.Stream.Position = Stream.Position;
                int result = copy.Create(out nint instance);
                *output = instance;
                if (result >= 0) CloneSource = copy;
                return result;
            }
            catch { copy.Stream.Dispose(); throw; }
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MemoryCapability(Descriptor* descriptor, int* value)
    {
        *value = 1;
        return 0;
    }

    private static MemorySource Memory(Descriptor* descriptor)
        => (MemorySource)GCHandle.FromIntPtr((nint)descriptor->Handle).Target!;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static void MemoryDispose(Descriptor* descriptor)
    {
        var root = GCHandle.FromIntPtr((nint)descriptor->Handle);
        var source = (MemorySource)root.Target!;
        source.Disposals++;
        source.Stream.Dispose();
        root.Free();
        source.Disposed?.Invoke();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MemoryRead(Descriptor* d, byte* data, uint count, uint* read)
    {
        try
        {
            Memory(d).BeforeRead?.Invoke();
            int actual = Memory(d).Stream.Read(new Span<byte>(data, checked((int)count)));
            if (read != null) *read = (uint)actual;
            return actual == count ? 0 : 1;
        }
        catch { return unchecked((int)0x80004005); }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MemoryWrite(Descriptor* d, byte* data, uint count, uint* written)
    {
        try
        {
            Memory(d).Stream.Write(new ReadOnlySpan<byte>(data, checked((int)count)));
            *written = count;
            return 0;
        }
        catch { return unchecked((int)0x80004005); }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MemorySeek(Descriptor* d, long offset, uint origin, ulong* position)
    {
        try { *position = (ulong)Memory(d).Stream.Seek(offset, (SeekOrigin)origin); return 0; }
        catch { return unchecked((int)0x80004005); }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MemorySize(Descriptor* d, ulong size)
    {
        try { Memory(d).Stream.SetLength(checked((long)size)); return 0; }
        catch { return unchecked((int)0x80004005); }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MemoryStat(Descriptor* d, void* output, uint flags)
    {
        try
        {
            // Fixed Windows STATSTG layout; the assertion reads it through the BCL definition.
            byte* bytes = (byte*)output;
            int sizeOffset = IntPtr.Size == 8 ? 16 : 8;
            new Span<byte>(bytes, sizeOffset + 64).Clear();
            *(uint*)(bytes + IntPtr.Size) = 2;
            *(ulong*)(bytes + sizeOffset) = (ulong)Memory(d).Stream.Length;
            *(uint*)(bytes + sizeOffset + 8) = 101; *(uint*)(bytes + sizeOffset + 12) = 102;
            *(uint*)(bytes + sizeOffset + 16) = 201; *(uint*)(bytes + sizeOffset + 20) = 202;
            *(uint*)(bytes + sizeOffset + 24) = 301; *(uint*)(bytes + sizeOffset + 28) = 302;
            *(uint*)(bytes + sizeOffset + 32) = 2;
            return 0;
        }
        catch { return unchecked((int)0x80004005); }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MemoryCopy(Descriptor* d, nint target, ulong count, ulong* read, ulong* written)
    {
        *read = 0; *written = 0;
        try
        {
            Span<byte> buffer = stackalloc byte[256];
            while (*read < count)
            {
                int actual = Memory(d).Stream.Read(buffer[..(int)Math.Min(256ul, count - *read)]);
                if (actual == 0) return 1;
                *read += (uint)actual;
                uint sent = 0;
                int result;
                fixed (byte* data = buffer)
                    result = ((delegate* unmanaged[Stdcall]<nint, byte*, uint, uint*, int>)(*(void***)target)[4])(target, data, (uint)actual, &sent);
                *written += sent;
                if (result < 0) return result;
                if (sent != actual) return unchecked((int)0x80030070);
            }
            return 0;
        }
        catch { return unchecked((int)0x80004005); }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int MemoryClone(Descriptor* d, nint* output)
    {
        try { return Memory(d).Clone(output); }
        catch { return unchecked((int)0x80004005); }
    }
}
