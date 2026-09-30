using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe class UceChannelExports
{
    private const int InvalidArgument = unchecked((int)0x80070057);
    private const int NotImplemented = unchecked((int)0x80004001);
    private const int OutOfMemory = unchecked((int)0x8007000E);
    private sealed class Connection
    {
        internal readonly int Thread = Environment.CurrentManagedThreadId;
        internal bool Presenting;
    }
    private sealed record Channel(Connection Connection, SameThreadChannel State, bool IsRoot);
    private static readonly object Gate = new();
    private static readonly Dictionary<nint, Connection> Connections = [];
    private static readonly Dictionary<nint, Channel> Channels = [];
    private static nint _nextIdentity;

    private static nint NewIdentity() => checked(++_nextIdentity);
    private static bool TryChannel(nint identity, out Channel? channel) =>
        Channels.TryGetValue(identity, out channel) && channel.Connection.Thread == Environment.CurrentManagedThreadId;

    [UnmanagedCallersOnly(EntryPoint = "WgxConnection_Create", CallConvs = [typeof(CallConvStdcall)])]
    private static int Create(byte synchronous, nint* output)
    {
        if (output == null) return InvalidArgument;
        if (synchronous == 0) return NotImplemented;
        lock (Gate)
        {
            try
            {
                var connection = new Connection();
                nint identity = NewIdentity();
                Connections.Add(identity, connection);
                *output = identity;
                return 0;
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
            catch (OverflowException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "WgxConnection_SameThreadPresent", CallConvs = [typeof(CallConvStdcall)])]
    private static int Present(nint identity)
    {
        lock (Gate)
        {
            if (!Connections.TryGetValue(identity, out Connection? connection) || connection.Thread != Environment.CurrentManagedThreadId) return InvalidArgument;
            if (connection.Presenting) return unchecked((int)0x8000FFFF);
            connection.Presenting = true;
            try
            {
                // Native PresentAllPartitions composes only entries without a source channel.
                SameThreadPartition[] partitions = Channels.OrderBy(entry => entry.Key)
                    .Where(entry => ReferenceEquals(entry.Value.Connection, connection) && entry.Value.IsRoot)
                    .Select(entry => entry.Value.State.Partition).ToArray();
                foreach (SameThreadPartition partition in partitions)
                {
                    if (partition.Failure < 0) return partition.Failure;
                    int result = partition.Targets.Render();
                    if (result < 0) return result;
                }
                return 0;
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
            finally { connection.Presenting = false; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "WgxConnection_Disconnect", CallConvs = [typeof(CallConvStdcall)])]
    private static int Disconnect(nint identity)
    {
        lock (Gate)
        {
            if (!Connections.TryGetValue(identity, out Connection? connection) || connection.Thread != Environment.CurrentManagedThreadId) return InvalidArgument;
            // Channels retain the connection after its caller-owned reference is released.
            Connections.Remove(identity);
            return 0;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilConnection_CreateChannel", CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateChannel(nint identity, nint source, nint* output)
    {
        if (output == null) return InvalidArgument;
        lock (Gate)
        {
            if (!Connections.TryGetValue(identity, out Connection? connection) || connection.Thread != Environment.CurrentManagedThreadId) return InvalidArgument;
            SameThreadPartition? partition = null;
            if (source != 0)
            {
                if (!TryChannel(source, out Channel? sourceChannel) || !ReferenceEquals(sourceChannel!.Connection, connection)) return InvalidArgument;
                partition = sourceChannel.State.Partition;
            }
            try
            {
                nint handle = NewIdentity();
                Channels.EnsureCapacity(Channels.Count + 1);
                var channel = new Channel(connection, new SameThreadChannel(partition ?? new SameThreadPartition()), source == 0);
                channel.State.Register();
                Channels.Add(handle, channel);
                *output = handle;
                return 0;
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
            catch (OverflowException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilConnection_DestroyChannel", CallConvs = [typeof(CallConvStdcall)])]
    private static int DestroyChannel(nint identity)
    {
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            Channels.Remove(identity);
            try { channel!.State.Destroy(); return 0; }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilResource_CreateOrAddRefOnChannel", CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateResource(nint identity, uint type, uint* handle)
    {
        if (handle == null) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            try { return channel!.State.CreateOrAddRef((MilResourceType)type, ref *handle); }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilResource_DuplicateHandle", CallConvs = [typeof(CallConvStdcall)])]
    private static int Duplicate(nint source, uint original, nint target, uint* output)
    {
        if (output == null) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(source, out Channel? from) || !TryChannel(target, out Channel? to)
                || !ReferenceEquals(from!.Connection, to!.Connection)) return InvalidArgument;
            try
            {
                int result = from.State.DuplicateTo(original, to.State, out uint duplicate);
                if (result >= 0) *output = duplicate;
                return result;
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilResource_ReleaseOnChannel", CallConvs = [typeof(CallConvStdcall)])]
    private static int ReleaseResource(nint identity, uint handle, int* deleted)
    {
        if (handle == 0) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            if (deleted != null) *deleted = 0;
            try
            {
                int result = channel!.State.Release(handle, out bool removed);
                if (deleted != null) *deleted = removed ? 1 : 0;
                return result;
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilResource_SendCommand", CallConvs = [typeof(CallConvStdcall)])]
    private static int Send(void* data, uint size, byte separate, nint identity)
    {
        if (data == null || size < sizeof(uint) || size > int.MaxValue) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            var packet = new ReadOnlySpan<byte>(data, (int)size);
            MilCommand command = (MilCommand)BinaryPrimitives.ReadUInt32LittleEndian(packet);
            // Do not accept ownership-bearing packets until discard cleanup is connected.
            if (command == MilCommand.BitmapSource && size != Marshal.SizeOf<MilBitmapSourceCommand>())
                return Direct3D9Factory.UceMalformedPacketHResult;
            if ((command == MilCommand.DoubleBufferedBitmap && size != Marshal.SizeOf<MilDoubleBufferedBitmapCommand>())
                || (command == MilCommand.DoubleBufferedBitmapCopyForward && size != Marshal.SizeOf<MilDoubleBufferedBitmapCopyCommand>()))
                return Direct3D9Factory.UceMalformedPacketHResult;
            if (command is MilCommand.MediaPlayer or MilCommand.D3DImage
                or MilCommand.D3DImagePresent
                or MilCommand.ChannelDuplicateHandle or MilCommand.TransportDestroyResourcesOnChannel or MilCommand.TransportSyncFlush)
                return NotImplemented;
            try { channel!.State.Send(packet, separate != 0); return 0; }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_CloseBatch", CallConvs = [typeof(CallConvStdcall)])]
    private static int Close(nint identity)
    {
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            try { channel!.State.CloseBatch(); return 0; }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_CommitChannel", CallConvs = [typeof(CallConvStdcall)])]
    private static int Commit(nint identity)
    {
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            try { return channel!.State.Commit(); }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }
}
