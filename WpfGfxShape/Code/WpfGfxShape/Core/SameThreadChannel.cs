using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal sealed class SameThreadPartition
{
    internal readonly GeneratedProtocolChannelRegistry Channels = new();
    internal readonly GeneratedTargetRegistry Targets = new();
    internal int Failure;
    private uint _nextChannel;

    internal uint AllocateChannel() => checked(++_nextChannel);
}

internal sealed class SameThreadChannel
{
    private sealed class Entry(MilResourceType type)
    {
        internal readonly MilResourceType Type = type;
        internal uint References = 1;
    }

    private readonly Dictionary<uint, Entry> _handles = [];
    private readonly GeneratedProtocolHandleTable _resources;
    private readonly GeneratedProtocolRouter _router;
    private readonly Queue<List<byte[]>> _closed = [];
    private List<byte[]> _open = [];
    private uint _nextHandle;
    private bool _committing;
    private bool _destroyRequested;
    private bool _destroyed;
    internal SameThreadPartition Partition { get; }
    private readonly uint _channel;

    internal SameThreadChannel(SameThreadPartition partition)
    {
        Partition = partition;
        _resources = new GeneratedProtocolHandleTable(targets: partition.Targets);
        _channel = partition.AllocateChannel();
        _router = new GeneratedProtocolProductionContext(_channel, partition.Channels).CreateRouter();
    }

    internal void Register() => Partition.Channels.TryAdd(_channel, _resources);

    internal int DuplicateTo(uint original, SameThreadChannel target, out uint duplicate)
    {
        duplicate = 0;
        if (!ReferenceEquals(Partition, target.Partition)) return unchecked((int)0x80070057);
        if (!_handles.TryGetValue(original, out Entry? entry)) return unchecked((int)0x80070006);
        if (target._nextHandle == uint.MaxValue) return unchecked((int)0x8007000E);
        uint candidate = target._nextHandle + 1;
        var copy = new Entry(entry.Type);
        byte[] packet = GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(original, target._channel, candidate);
        target._handles.EnsureCapacity(target._handles.Count + 1);
        _open.Add(packet);
        target._handles.Add(candidate, copy);
        target._nextHandle = candidate;
        duplicate = candidate;
        return 0;
    }

    internal int CreateOrAddRef(MilResourceType type, ref uint handle)
    {
        if (handle != 0)
        {
            if (!_handles.TryGetValue(handle, out Entry? existing)) return unchecked((int)0x80070006);
            if (existing.References == uint.MaxValue) return unchecked((int)0x8000FFFF);
            existing.References++;
            return 0;
        }

        if (type is <= MilResourceType.Null or >= MilResourceType.Last) return Direct3D9Factory.UceMalformedPacketHResult;
        if (_nextHandle == uint.MaxValue) return unchecked((int)0x8007000E);
        uint candidate = _nextHandle + 1;
        byte[] packet = GeneratedProtocolPacketWriter.WriteChannelCreateResource(candidate, type);
        var entry = new Entry(type);
        _handles.EnsureCapacity(_handles.Count + 1);
        _open.Add(packet);
        _handles.Add(candidate, entry);
        _nextHandle = candidate;
        handle = candidate;
        return 0;
    }

    internal int Release(uint handle, out bool deleted)
    {
        deleted = false;
        if (!_handles.TryGetValue(handle, out Entry? entry)) return unchecked((int)0x8000FFFF);
        if (entry.References == 1)
        {
            _open.Add(GeneratedProtocolPacketWriter.WriteChannelDeleteResource(handle, entry.Type));
            _handles.Remove(handle);
            deleted = true;
        }
        else entry.References--;
        return 0;
    }

    internal void Send(ReadOnlySpan<byte> packet, bool separate)
    {
        byte[] copy = packet.ToArray();
        if (separate) _closed.Enqueue([copy]);
        else _open.Add(copy);
    }

    internal void CloseBatch()
    {
        if (_open.Count == 0) return;
        List<byte[]> next = [];
        _closed.Enqueue(_open);
        _open = next;
    }

    internal int Commit()
    {
        if (_committing) return unchecked((int)0x8000FFFF);
        if (_destroyed) return unchecked((int)0x80070057);
        _committing = true;
        try
        {
            return CommitBatches();
        }
        finally
        {
            _committing = false;
            if (_destroyRequested) ReleaseResources();
        }
    }

    private int CommitBatches()
    {
        while (_closed.TryDequeue(out List<byte[]>? batch))
        {
            foreach (byte[] packet in batch)
            {
                bool transferred = false;
                try
                {
                    if (Partition.Failure >= 0 && !_destroyRequested)
                    {
                        // ProcessSource consumes the transport reference even when QI fails.
                        if (TryGetBitmapPacket(packet, out MilBitmapSourceCommand bitmap))
                            transferred = _resources.TryGetResource(bitmap.Handle, out GeneratedProtocolResource? resource)
                                && resource is GeneratedBitmapSourceResource && !resource.IsReleased;
                        MilCommand command = (MilCommand)BinaryPrimitives.ReadUInt32LittleEndian(packet);
                        if (command is MilCommand.DoubleBufferedBitmap or MilCommand.DoubleBufferedBitmapCopyForward)
                            transferred = _resources.TryGetResource(BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(4)), out GeneratedProtocolResource? target)
                                && target is GeneratedDoubleBufferedBitmapResource && !target.IsReleased;
                        int result = _router.ProcessPacket(packet);
                        if (Partition.Failure >= 0) Partition.Failure = result;
                    }
                }
                catch (OutOfMemoryException)
                {
                    if (Partition.Failure >= 0) Partition.Failure = unchecked((int)0x8007000E);
                }
                finally
                {
                    if (!transferred) ReleasePacket(packet);
                }
            }
        }
        return Partition.Failure;
    }

    private static bool TryGetBitmapPacket(byte[] packet, out MilBitmapSourceCommand command)
    {
        if (packet.Length == Marshal.SizeOf<MilBitmapSourceCommand>()
            && BinaryPrimitives.ReadUInt32LittleEndian(packet) == (uint)MilCommand.BitmapSource)
        {
            command = MemoryMarshal.Read<MilBitmapSourceCommand>(packet);
            return true;
        }
        command = default;
        return false;
    }

    private static void ReleasePacket(byte[] packet)
    {
        MilCommand command = (MilCommand)BinaryPrimitives.ReadUInt32LittleEndian(packet);
        if (command == MilCommand.DoubleBufferedBitmap && packet.Length == Marshal.SizeOf<MilDoubleBufferedBitmapCommand>())
        {
            nint doubleBitmap = (nint)MemoryMarshal.Read<MilDoubleBufferedBitmapCommand>(packet).Bitmap;
            packet.AsSpan(8, 8).Clear();
            Direct3D9Factory.Release(doubleBitmap);
        }
        if (command == MilCommand.DoubleBufferedBitmapCopyForward && packet.Length == Marshal.SizeOf<MilDoubleBufferedBitmapCopyCommand>())
        {
            nint handle = (nint)MemoryMarshal.Read<MilDoubleBufferedBitmapCopyCommand>(packet).Event;
            packet.AsSpan(8, 8).Clear();
            GeneratedDoubleBufferedBitmapResource.Complete(handle);
        }
        if (TryGetBitmapPacket(packet, out MilBitmapSourceCommand bitmap))
        {
            // Clear ownership before calling external COM, which may reenter.
            packet.AsSpan(8).Clear();
            Direct3D9Factory.Release(bitmap.Bitmap);
        }
    }

    internal void Destroy()
    {
        if (_destroyed || _destroyRequested) return;
        if (_committing)
        {
            // The active packet may still be using a resource across a COM callback.
            _destroyRequested = true;
            return;
        }
        try
        {
            CloseBatch();
            Commit();
        }
        finally
        {
            ReleaseResources();
        }
    }

    private void ReleaseResources()
    {
        if (_destroyed) return;
        _destroyed = true;
        Partition.Channels.Remove(_channel);
        while (_open.Count != 0)
        {
            int index = _open.Count - 1;
            byte[] packet = _open[index];
            _open.RemoveAt(index);
            ReleasePacket(packet);
        }
        while (_closed.TryDequeue(out List<byte[]>? batch))
            foreach (byte[] packet in batch) ReleasePacket(packet);
        _handles.Clear();
        _resources.ReleaseAll();
        if (Partition.Channels.Count == 0) Partition.Targets.ReleaseAll();
    }
}
