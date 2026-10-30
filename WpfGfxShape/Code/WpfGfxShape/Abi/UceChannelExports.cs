using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe partial class UceChannelExports
{
    private const int InvalidArgument = unchecked((int)0x80070057);
    private const int NotImplemented = unchecked((int)0x80004001);
    private const int OutOfMemory = unchecked((int)0x8007000E);
    private sealed class Connection
    {
        internal readonly int Thread = Environment.CurrentManagedThreadId;
        internal bool Presenting;
        internal PartitionScheduler? Scheduler;
        internal bool Disconnected;
    }
    private sealed record Channel(Connection Connection, SameThreadChannel State, bool IsRoot)
    {
        internal readonly int Thread = Environment.CurrentManagedThreadId;
    }
    private static readonly object Gate = new();
    private static readonly Dictionary<nint, Connection> Connections = [];
    private static readonly Dictionary<nint, Channel> Channels = [];
    private static nint _nextIdentity;

    private static nint NewIdentity() => checked(++_nextIdentity);
    private static bool TryChannel(nint identity, out Channel? channel) =>
        Channels.TryGetValue(identity, out channel) && channel.Thread == Environment.CurrentManagedThreadId;

    // MediaSystem serializes its shared service channel, but the last Dispatcher
    // may close and submit it on a different thread from the one that created it.
    private static bool TryLifecycleChannel(nint identity, out Channel? channel) =>
        Channels.TryGetValue(identity, out channel)
        && (channel.Thread == Environment.CurrentManagedThreadId
            || (channel.IsRoot && channel.Connection.Scheduler is not null));

    [UnmanagedCallersOnly(EntryPoint = "WgxConnection_Create", CallConvs = [typeof(CallConvStdcall)])]
    private static int Create(byte synchronous, nint* output)
    {
        if (output == null) return InvalidArgument;
        *output = 0;
        bool ownsScheduler = false;
        try
        {
            lock (Gate)
            {
                try
                {
                    var connection = new Connection();
                    if (synchronous == 0)
                    {
                        int result = PartitionManagerExports.Acquire(out connection.Scheduler);
                        if (result < 0) return result;
                        ownsScheduler = true;
                    }
                    nint identity = NewIdentity();
                    Connections.Add(identity, connection);
                    ownsScheduler = false;
                    *output = identity;
                    return 0;
                }
                catch (OutOfMemoryException) { return OutOfMemory; }
                catch (OverflowException) { return OutOfMemory; }
                catch (InvalidOperationException error) { return PartitionScheduler.RecordFailure(error); }
                catch (ArgumentException error) { return PartitionScheduler.RecordFailure(error); }
                catch (System.ComponentModel.Win32Exception error) { return PartitionScheduler.RecordFailure(error); }
                catch (ExternalException error) { return PartitionScheduler.RecordFailure(error); }
            }
        }
        finally
        {
            // Release may join a worker whose callback needs Gate.
            if (ownsScheduler) PartitionManagerExports.Release();
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "WgxConnection_SameThreadPresent", CallConvs = [typeof(CallConvStdcall)])]
    private static int Present(nint identity)
    {
        lock (Gate)
        {
            if (!Connections.TryGetValue(identity, out Connection? connection) || connection.Thread != Environment.CurrentManagedThreadId) return InvalidArgument;
            if (connection.Scheduler is not null) return InvalidArgument;
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
                    try
                    {
                        if (partition.Failure < 0) return partition.Failure;
                        // Native synchronous composition does not enter CSlaveHWndRenderTarget::Present.
                        int result = partition.Targets.Render(signalPresent: false);
                        if (result < 0)
                        {
                            PartitionScheduler.RecordCommandFailure(0, [], result);
                            return result;
                        }
                    }
                    finally { partition.FlushPendingBadShaderNotifications(); }
                }
                return 0;
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
            catch (ArgumentException error) { return PartitionScheduler.RecordFailure(error); }
            catch (InvalidOperationException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ExternalException error) { return PartitionScheduler.RecordFailure(error); }
            catch (OverflowException error) { return PartitionScheduler.RecordFailure(error); }
            finally { connection.Presenting = false; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilComposition_PeekNextMessage", CallConvs = [typeof(CallConvStdcall)])]
    private static int PeekNextMessage(nint identity, void* message, nuint size, int* retrieved)
    {
        if (retrieved == null || (message == null && size != 0)) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            *retrieved = channel!.State.PeekMessage(new Span<byte>(message, (int)nuint.Min(size, 64))) ? 1 : 0;
            return 0;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "WgxConnection_Disconnect", CallConvs = [typeof(CallConvStdcall)])]
    private static int Disconnect(nint identity)
    {
        PartitionScheduler? stop = null;
        lock (Gate)
        {
            if (!Connections.TryGetValue(identity, out Connection? connection)
                || (connection.Scheduler is null && connection.Thread != Environment.CurrentManagedThreadId)) return InvalidArgument;
            Connections.Remove(identity);
            connection.Disconnected = true;
            if (!Channels.Values.Any(channel => ReferenceEquals(channel.Connection, connection))) stop = connection.Scheduler;
        }
        if (stop is not null) PartitionManagerExports.Release();
        return 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "MilConnection_CreateChannel", CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateChannel(nint identity, nint source, nint* output)
    {
        if (output == null) return InvalidArgument;
        *output = 0;
        PartitionScheduler.Work? registration = null;
        nint created = 0;
        lock (Gate)
        {
            if (!Connections.TryGetValue(identity, out Connection? connection)
                || (connection.Scheduler is null && connection.Thread != Environment.CurrentManagedThreadId)) return InvalidArgument;
            SameThreadPartition? partition = null;
            if (source != 0)
            {
                if (!Channels.TryGetValue(source, out Channel? sourceChannel) || !ReferenceEquals(sourceChannel.Connection, connection)
                    || (connection.Scheduler is null && sourceChannel.Thread != Environment.CurrentManagedThreadId)) return InvalidArgument;
                partition = sourceChannel.State.Partition;
            }
            Channel? channel = null;
            bool accepted = false;
            try
            {
                nint handle = NewIdentity();
                Channels.EnsureCapacity(Channels.Count + 1);
                channel = new Channel(connection, new SameThreadChannel(partition ?? new SameThreadPartition(connection.Scheduler)), source == 0);
                Channels.Add(handle, channel);
                created = handle;
                if (connection.Scheduler is null) channel.State.Register();
                else
                {
                    registration = new PartitionScheduler.Work(channel.State, null, register: true);
                    connection.Scheduler.Submit(registration);
                }
                accepted = true;
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
            catch (OverflowException) { return OutOfMemory; }
            catch (InvalidOperationException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ArgumentException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ExternalException error) { return PartitionScheduler.RecordFailure(error); }
            finally
            {
                if (!accepted)
                {
                    Channels.Remove(created);
                    try { channel?.State.Destroy(); }
                    catch (OutOfMemoryException error) { PartitionScheduler.RecordFailure(error); }
                    catch (OverflowException error) { PartitionScheduler.RecordFailure(error); }
                    catch (InvalidOperationException error) { PartitionScheduler.RecordFailure(error); }
                    catch (ArgumentException error) { PartitionScheduler.RecordFailure(error); }
                    catch (ExternalException error) { PartitionScheduler.RecordFailure(error); }
                }
            }
        }
        int result = registration?.Completion.Task.GetAwaiter().GetResult() ?? 0;
        if (result >= 0) *output = created;
        else
        {
            bool stop;
            lock (Gate)
            {
                // The worker has already rolled back the failed registration.
                Channels.Remove(created, out Channel? channel);
                stop = channel is not null && channel.Connection.Disconnected
                    && !Channels.Values.Any(other => ReferenceEquals(other.Connection, channel.Connection));
            }
            if (stop) PartitionManagerExports.Release();
        }
        return result;
    }

    [UnmanagedCallersOnly(EntryPoint = "MilConnection_DestroyChannel", CallConvs = [typeof(CallConvStdcall)])]
    private static int DestroyChannel(nint identity)
    {
        PartitionScheduler.Work? cleanup = null;
        PartitionScheduler? scheduler = null;
        bool stop = false;
        lock (Gate)
        {
            if (!TryLifecycleChannel(identity, out Channel? channel)
                && !(channel is not null && channel.Connection.Scheduler?.IsCurrentThread == true)) return InvalidArgument;
            scheduler = channel.Connection.Scheduler;
            try
            {
                if (scheduler is null)
                {
                    Channels.Remove(identity);
                    channel.State.Destroy();
                }
                else
                {
                    channel.State.CloseBatch();
                    cleanup = scheduler.SubmitBatches(channel.State, destroy: true);
                }
                Channels.Remove(identity);
                stop = channel.Connection.Disconnected && !Channels.Values.Any(other => ReferenceEquals(other.Connection, channel.Connection));
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
            catch (InvalidOperationException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ArgumentException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ExternalException error) { return PartitionScheduler.RecordFailure(error); }
        }
        int result = cleanup is not null && scheduler?.IsCurrentThread != true
            ? cleanup.Completion.Task.GetAwaiter().GetResult() : 0;
        if (stop && scheduler is not null) PartitionManagerExports.Release();
        return result;
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
            catch (OverflowException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ArgumentException error) { return PartitionScheduler.RecordFailure(error); }
            catch (InvalidOperationException error) { return PartitionScheduler.RecordFailure(error); }
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

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_BeginCommand", CallConvs = [typeof(CallConvStdcall)])]
    private static int BeginCommand(nint identity, void* data, uint size, uint extra)
    {
        if (data == null || size < 4) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            if (size > int.MaxValue) return OutOfMemory;
            try { return channel!.State.BeginCommand(new ReadOnlySpan<byte>(data, (int)size), extra); }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_AppendCommandData", CallConvs = [typeof(CallConvStdcall)])]
    private static int AppendCommandData(nint identity, void* data, uint size)
    {
        if (data == null && size != 0) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            if (size > int.MaxValue) return unchecked((int)0x8000FFFF);
            return channel!.State.AppendCommandData(new ReadOnlySpan<byte>(data, (int)size));
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_EndCommand", CallConvs = [typeof(CallConvStdcall)])]
    private static int EndCommand(nint identity)
    {
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            try { return channel!.State.EndCommand(); }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilResource_GetRefCountOnChannel", CallConvs = [typeof(CallConvStdcall)])]
    private static int GetRefCount(nint identity, uint handle, uint* output)
    {
        if (handle == 0) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            int result = channel!.State.GetRefCount(handle, out uint count);
            if (result >= 0 && output != null) *output = count;
            return result;
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
            if (command == MilCommand.GlyphRunCreate && size < 76)
                return Direct3D9Factory.UceMalformedPacketHResult;
            // Do not accept ownership-bearing packets until discard cleanup is connected.
            if (command == MilCommand.BitmapSource && size != Marshal.SizeOf<MilBitmapSourceCommand>())
                return Direct3D9Factory.UceMalformedPacketHResult;
            if ((command == MilCommand.DoubleBufferedBitmap && size != Marshal.SizeOf<MilDoubleBufferedBitmapCommand>())
                || (command == MilCommand.DoubleBufferedBitmapCopyForward && size != Marshal.SizeOf<MilDoubleBufferedBitmapCopyCommand>()))
                return Direct3D9Factory.UceMalformedPacketHResult;
            if ((command == MilCommand.D3DImage && size != Marshal.SizeOf<MilD3DImageCommand>())
                || (command == MilCommand.D3DImagePresent && size != Marshal.SizeOf<MilD3DImagePresentCommand>()))
                return Direct3D9Factory.UceMalformedPacketHResult;
            if (command == MilCommand.MediaPlayer)
            {
                if (size != Marshal.SizeOf<MilMediaPlayerCommand>())
                    return Direct3D9Factory.UceMalformedPacketHResult;
                MilMediaPlayerCommand media = MemoryMarshal.Read<MilMediaPlayerCommand>(packet);
                if (IntPtr.Size == 4 && media.Media > uint.MaxValue)
                    return Direct3D9Factory.UceMalformedPacketHResult;
            }
            if (command is MilCommand.ChannelDuplicateHandle or MilCommand.TransportDestroyResourcesOnChannel or MilCommand.TransportSyncFlush)
                return NotImplemented;
            try { return channel!.State.Send(packet, separate != 0); }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilResource_SendCommandBitmapSource", CallConvs = [typeof(CallConvStdcall)])]
    private static int SendBitmapSource(uint handle, nint bitmap, nint identity)
    {
        if (bitmap == 0) return InvalidArgument;
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            Direct3D9Factory.AddRef(bitmap);
            bool queued = false;
            try
            {
                var command = new MilBitmapSourceCommand(handle, bitmap);
                int result = channel!.State.Send(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref command, 1)), false);
                queued = result >= 0;
                return result;
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
            finally
            {
                // Native SendCommand transfers the extra reference only on success.
                if (!queued) Direct3D9Factory.Release(bitmap);
            }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilResource_SendCommandMedia", CallConvs = [typeof(CallConvStdcall)])]
    private static int SendMedia(uint handle, nint media, nint identity, byte notifyUceDirect)
    {
        lock (Gate)
        {
            if (!TryChannel(identity, out Channel? channel)) return InvalidArgument;
            try
            {
                // The media master supplies the reference; native apifunc does not AddRef.
                var command = new MilMediaPlayerCommand(handle, unchecked((ulong)media), notifyUceDirect != 0 ? 1 : 0);
                return channel!.State.Send(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref command, 1)), false);
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_CloseBatch", CallConvs = [typeof(CallConvStdcall)])]
    private static int Close(nint identity)
    {
        lock (Gate)
        {
            if (!TryLifecycleChannel(identity, out Channel? channel)) return InvalidArgument;
            try { return channel!.State.CloseBatch(); }
            catch (OutOfMemoryException) { return OutOfMemory; }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilChannel_CommitChannel", CallConvs = [typeof(CallConvStdcall)])]
    private static int Commit(nint identity)
    {
        lock (Gate)
        {
            if (!TryLifecycleChannel(identity, out Channel? channel)) return InvalidArgument;
            try
            {
                if (channel!.Connection.Scheduler is null) return channel.State.Commit();
                channel.Connection.Scheduler.SubmitBatches(channel.State);
                return 0;
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
            catch (InvalidOperationException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ArgumentException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ExternalException error) { return PartitionScheduler.RecordFailure(error); }
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "MilComposition_SyncFlush", CallConvs = [typeof(CallConvStdcall)])]
    private static int SyncFlush(nint identity)
    {
        PartitionScheduler.Work? work = null;
        lock (Gate)
        {
            if (!TryLifecycleChannel(identity, out Channel? channel)) return InvalidArgument;
            if (channel!.Connection.Scheduler?.IsCurrentThread == true) return unchecked((int)0x8000FFFF);
            try
            {
                int result = channel.State.CloseBatch();
                if (result < 0) return result;
                if (channel.Connection.Scheduler is null) return channel.State.Commit();
                work = channel.Connection.Scheduler.SubmitBatches(channel.State, barrier: true);
            }
            catch (OutOfMemoryException) { return OutOfMemory; }
            catch (InvalidOperationException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ArgumentException error) { return PartitionScheduler.RecordFailure(error); }
            catch (ExternalException error) { return PartitionScheduler.RecordFailure(error); }
        }
        return work.Completion.Task.GetAwaiter().GetResult();
    }
}
