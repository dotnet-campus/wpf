using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

internal sealed class SameThreadPartition
{
    private enum CompositionDeviceState : uint
    {
        Normal = 0,
        NoDevice = 1
    }

    // Matches CComposition construction before any successful window presentation.
    private CompositionDeviceState _deviceState = CompositionDeviceState.NoDevice;

    internal readonly GeneratedProtocolChannelRegistry Channels = new();
    internal readonly GeneratedTargetRegistry Targets;
    internal int Failure;
    internal static bool RenderWithoutDisplays;
    internal readonly Av.VideoComposition? Videos;

    internal SameThreadPartition(PartitionScheduler? scheduler = null)
    {
        Targets = new GeneratedTargetRegistry();
        if (scheduler is not null) Videos = new Av.VideoComposition(() => scheduler.ScheduleCompositionPass(this));
    }

    internal int Compose()
    {
        if (Failure < 0) return Failure;
        int result = Videos?.BeginProcessVideo(false) ?? 0;
        try
        {
            if (result >= 0 && _vblankChannels.Count != 0)
            {
                result = DwmFlush();
                if (result < 0) PartitionScheduler.RecordCommandFailure(0, "DwmFlush"u8, result);
            }
            if (result >= 0 && (RenderWithoutDisplays || ChannelGraphicsCapabilities.HasDisplays()))
            {
                result = Targets.Render(presentResult: ProcessSuccessfulWindowPresent);
                if (result < 0) PartitionScheduler.RecordCommandFailure(0, "Targets.Render"u8, result);
            }
            if (result < 0 && Failure >= 0) Failure = result;
            if (result >= 0) NotifyPresentListeners();
            return result;
        }
        finally
        {
            Videos?.EndProcessVideo();
            FlushPendingBadShaderNotifications();
        }
    }

    private readonly List<(SameThreadChannel Channel, ulong FrameTime)> _presentListeners = [];

    internal void RequestPresentNotification(SameThreadChannel channel, ulong frameTime)
        => _presentListeners.Add((channel, frameTime));

    internal void RemovePresentNotifications(SameThreadChannel channel)
        => _presentListeners.RemoveAll(listener => ReferenceEquals(listener.Channel, channel));

    private void NotifyPresentListeners()
    {
        if (_presentListeners.Count == 0) return;
        bool canPresent = Targets.HasPresentableTargets
            && (RenderWithoutDisplays || ChannelGraphicsCapabilities.HasDisplays());
        ulong frameTime = 0;
        foreach (var listener in _presentListeners)
            if (listener.FrameTime != 0 && (frameTime == 0 || listener.FrameTime < frameTime)) frameTime = listener.FrameTime;
        foreach (var listener in _presentListeners)
        {
            byte[] message = new byte[64];
            BinaryPrimitives.WriteUInt32LittleEndian(message, 0x0A);
            // A completed render does not establish a VSync/DWM presentation timestamp.
            BinaryPrimitives.WriteUInt32LittleEndian(message.AsSpan(8), canPresent ? 2u : 1u);
            BinaryPrimitives.WriteUInt32LittleEndian(message.AsSpan(12), canPresent ? 60u : 0u);
            BinaryPrimitives.WriteUInt64LittleEndian(message.AsSpan(16), canPresent ? frameTime : 0);
            listener.Channel.PostMessage(message);
        }
        _presentListeners.Clear();
    }

    private readonly HashSet<SameThreadChannel> _vblankChannels = [];
    internal int VBlankChannelCount => _vblankChannels.Count;

    internal int SetVBlankSyncMode(SameThreadChannel channel, bool enable)
    {
        if (!enable)
        {
            if (_vblankChannels.Remove(channel) && _vblankChannels.Count == 0) timeEndPeriod(1);
            return 0;
        }
        int result = 0;
        if (!_vblankChannels.Contains(channel))
        {
            _vblankChannels.EnsureCapacity(_vblankChannels.Count + 1);
            if (_vblankChannels.Count == 0 && timeBeginPeriod(1) != 0)
                result = unchecked((int)0x80004005);
            if (result >= 0) _vblankChannels.Add(channel);
        }
        byte[] message = new byte[64];
        BinaryPrimitives.WriteUInt32LittleEndian(message, 0x09);
        BinaryPrimitives.WriteInt32LittleEndian(message.AsSpan(8), result);
        channel.PostMessage(message);
        return result;
    }

    [DllImport("winmm.dll", ExactSpelling = true)]
    private static extern uint timeBeginPeriod(uint period);
    [DllImport("winmm.dll", ExactSpelling = true)]
    private static extern uint timeEndPeriod(uint period);
    [DllImport("dwmapi.dll", ExactSpelling = true)]
    private static extern int DwmFlush();

    private uint _nextChannel;
    private readonly HashSet<SameThreadChannel> _notifications = [];
    private bool _pendingBadShader;

    private void ProcessSuccessfulWindowPresent(int result)
    {
        if (result != 0 || _deviceState != CompositionDeviceState.NoDevice) return;
        CompositionDeviceState previous = _deviceState;
        _deviceState = CompositionDeviceState.Normal;
        byte[] message = new byte[64];
        BinaryPrimitives.WriteUInt32LittleEndian(message, 0x05);
        BinaryPrimitives.WriteUInt32LittleEndian(message.AsSpan(8), (uint)previous);
        BinaryPrimitives.WriteUInt32LittleEndian(message.AsSpan(12), (uint)_deviceState);
        foreach (var listener in _notifications) listener.PostMessage(message);
    }

    internal void SetPendingBadShaderNotification() => _pendingBadShader = true;
    internal void RegisterNotifications(SameThreadChannel channel, bool enable)
    {
        if (!enable) { _notifications.Remove(channel); return; }
        if (_notifications.Add(channel))
        {
            byte[] message = new byte[64];
            BinaryPrimitives.WriteUInt32LittleEndian(message, 0x05);
            BinaryPrimitives.WriteUInt32LittleEndian(message.AsSpan(8), (uint)_deviceState);
            BinaryPrimitives.WriteUInt32LittleEndian(message.AsSpan(12), (uint)_deviceState);
            foreach (var listener in _notifications) listener.PostMessage(message);
        }
    }

    internal void FlushPendingBadShaderNotifications()
    {
        if (!_pendingBadShader) return;
        _pendingBadShader = false;
        foreach (var channel in _notifications) channel.PostMessage(0x10);
    }
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
    private Queue<List<byte[]>> _closed = [];
    private List<byte[]> _open = [];
    internal const int MissingEndCommand = unchecked((int)0x88980411);
    internal const int MissingBeginCommand = unchecked((int)0x88980412);
    private byte[]? _command;
    private int _commandLength;
    private uint _blockRemaining = 0x1000;
    private ulong _totalAllocations = 0x1000;
    private uint _nextHandle;

    private int Reserve(uint size)
    {
        ulong required = ((ulong)size + 7) & ~3UL;
        if (required > uint.MaxValue) return unchecked((int)0x80070216);
        if (_blockRemaining < required)
        {
            ulong allocation = required + Math.Min(_totalAllocations, 0x10000UL);
            if (allocation > uint.MaxValue) return unchecked((int)0x80070216);
            _blockRemaining = (uint)allocation;
            _totalAllocations += allocation;
        }
        return 0;
    }

    internal int BeginCommand(ReadOnlySpan<byte> header, uint extra)
    {
        if (_command is not null) return MissingEndCommand;
        ulong total = (ulong)header.Length + extra;
        if (total > uint.MaxValue) return unchecked((int)0x80070216);
        int result = Reserve((uint)total);
        if (result < 0) return result;
        if (_blockRemaining - 4 > Array.MaxLength) return unchecked((int)0x8007000E);
        byte[] command = new byte[(int)_blockRemaining - 4];
        header.CopyTo(command);
        _command = command;
        _commandLength = header.Length;
        return 0;
    }

    internal int AppendCommandData(ReadOnlySpan<byte> data)
    {
        if (_command is null) return MissingBeginCommand;
        if (data.Length > _command.Length - _commandLength) return unchecked((int)0x8000FFFF);
        data.CopyTo(_command.AsSpan(_commandLength));
        _commandLength += data.Length;
        return 0;
    }

    internal int EndCommand()
    {
        if (_command is null) return MissingBeginCommand;
        byte[] packet = _command.AsSpan(0, _commandLength).ToArray();
        _open.Add(packet);
        _blockRemaining -= (uint)((_commandLength + 7L) & ~3L);
        _command = null;
        _commandLength = 0;
        return 0;
    }

    internal int GetRefCount(uint handle, out uint references)
    {
        references = 0;
        if (_command is not null) return MissingEndCommand;
        if (!_handles.TryGetValue(handle, out Entry? entry)) return unchecked((int)0x8000FFFF);
        references = entry.References;
        return 0;
    }

    private void RecordPacket(byte[] packet)
    {
        int result = Reserve((uint)packet.Length);
        if (result < 0) Marshal.ThrowExceptionForHR(result);
        _open.Add(packet);
        _blockRemaining -= (uint)((packet.Length + 7L) & ~3L);
    }
    private bool _committing;
    private bool _destroyRequested;
    private bool _destroyed;
    private readonly Queue<byte[]> _messages = [];
    internal SameThreadPartition Partition { get; }

    private readonly EventWaitHandle _messageEvent = new(false, EventResetMode.AutoReset);
    private nint _notificationWindow;
    private uint _notificationMessage;
    internal bool ReceiveBroadcastMessages;

    internal void SetNotificationWindow(nint window, uint message)
    {
        lock (_messages)
        {
            _notificationWindow = window;
            _notificationMessage = message;
        }
    }

    internal void PostMessage(uint type)
    {
        byte[] message = new byte[64];
        BinaryPrimitives.WriteUInt32LittleEndian(message, type);
        PostMessage(message);
    }

    internal void PostMessage(byte[] message)
    {
        lock (_messages)
        {
            if (_destroyed) return;
            bool notify = _messages.Count == 0 && _notificationWindow != 0;
            _messages.Enqueue(message);
            if (notify) _ = PostMessageW(_notificationWindow, _notificationMessage, 0, 0);
            _messageEvent.Set();
        }
    }

    internal void WakeMessageWaiters()
    {
        lock (_messages)
        {
            if (!_destroyed) _messageEvent.Set();
        }
    }

    internal unsafe int WaitForMessage(uint count, nint* handles, bool waitAll, uint timeout, uint* output)
    {
        nint* nativeHandles = stackalloc nint[64];
        for (uint i = 0; i < count; i++) nativeHandles[i] = handles[i];
        bool added = false;
        Microsoft.Win32.SafeHandles.SafeWaitHandle? queueHandle = null;
        try
        {
            lock (_messages)
            {
                if (_destroyed) return unchecked((int)0x80070006);
                if (Partition.Failure < 0) return Partition.Failure;
                if (_messages.Count != 0) { *output = count; return 0; }
                queueHandle = _messageEvent.SafeWaitHandle;
                queueHandle.DangerousAddRef(ref added);
                _messageEvent.Reset();
                nativeHandles[count] = queueHandle.DangerousGetHandle();
            }
            uint result = WaitForMultipleObjects(count + 1, nativeHandles, waitAll ? 1 : 0, timeout);
            *output = result;
            if (result == uint.MaxValue) return Marshal.GetHRForLastWin32Error();
            lock (_messages)
            {
                if (_destroyed) return unchecked((int)0x80070006);
                return Partition.Failure < 0 ? Partition.Failure : 0;
            }
        }
        finally { if (added) queueHandle?.DangerousRelease(); }
    }

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern unsafe uint WaitForMultipleObjects(uint count, nint* handles, int waitAll, uint timeout);
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int PostMessageW(nint window, uint message, nuint wParam, nint lParam);
    internal bool PeekMessage(Span<byte> destination)
    {
        lock (_messages)
        {
            if (!_messages.TryDequeue(out byte[]? message)) { destination.Clear(); return false; }
            message.AsSpan(0, destination.Length).CopyTo(destination);
            return true;
        }
    }
    private readonly uint _channel;

    internal SameThreadChannel(SameThreadPartition partition)
    {
        bool initialized = false;
        try
        {
            Partition = partition;
            _resources = new GeneratedProtocolHandleTable(partition.Videos, targets: partition.Targets) { BadShaderNotification = partition.SetPendingBadShaderNotification };
            _channel = partition.AllocateChannel();
            _router = new GeneratedProtocolProductionContext(_channel, partition.Channels).CreateRouter();
            initialized = true;
        }
        finally
        {
            if (!initialized) _messageEvent.Dispose();
        }
    }

    internal bool IsRegistered { get; private set; }

    internal void Register()
    {
        if (!Partition.Channels.TryAdd(_channel, _resources)) throw new InvalidOperationException();
        IsRegistered = true;
    }

    internal int DuplicateTo(uint original, SameThreadChannel target, out uint duplicate)
    {
        duplicate = 0;
        if (_command is not null || target._command is not null) return MissingEndCommand;
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
        if (_command is not null) return MissingEndCommand;
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
        RecordPacket(packet);
        _handles.Add(candidate, entry);
        _nextHandle = candidate;
        handle = candidate;
        return 0;
    }

    internal int Release(uint handle, out bool deleted)
    {
        deleted = false;
        if (_command is not null) return MissingEndCommand;
        if (!_handles.TryGetValue(handle, out Entry? entry)) return unchecked((int)0x8000FFFF);
        if (entry.References == 1)
        {
            RecordPacket(GeneratedProtocolPacketWriter.WriteChannelDeleteResource(handle, entry.Type));
            _handles.Remove(handle);
            deleted = true;
        }
        else entry.References--;
        return 0;
    }

    internal int Send(ReadOnlySpan<byte> packet, bool separate)
    {
        if (!separate && _command is not null) return MissingEndCommand;
        byte[] copy = packet.ToArray();
        if (separate) _closed.Enqueue([copy]);
        else RecordPacket(copy);
        return 0;
    }

    internal int CloseBatch()
    {
        if (_command is not null) return MissingEndCommand;
        if (_open.Count == 0) return 0;
        List<byte[]> next = [];
        _closed.Enqueue(_open);
        _open = next;
        _blockRemaining = 0x1000;
        _totalAllocations = 0x1000;
        return 0;
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

    internal Queue<List<byte[]>> DetachBatches()
    {
        Queue<List<byte[]>> replacement = [];
        Queue<List<byte[]>> batches = _closed;
        _closed = replacement;
        return batches;
    }

    internal int CommitSubmitted(Queue<List<byte[]>> batches)
    {
        if (_committing) return unchecked((int)0x8000FFFF);
        _committing = true;
        try { return CommitBatches(batches); }
        finally
        {
            _committing = false;
            if (_destroyRequested) ReleaseResources();
        }
    }

    private int CommitBatches() => CommitBatches(_closed);

    private int CommitBatches(Queue<List<byte[]>> batches)
    {
        while (batches.TryDequeue(out List<byte[]>? batch))
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
                        if (command == MilCommand.GlyphRunCreate && packet.Length >= 76)
                        {
                            uint handle = BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(4));
                            if (handle != 0 && !_resources.TryGetResource(handle, out _))
                            {
                                int created = _resources.Create(handle, MilResourceType.GlyphRun);
                                if (created < 0)
                                {
                                    Partition.Failure = created;
                                    continue;
                                }
                            }
                            transferred = _resources.TryGetResource(handle, out GeneratedProtocolResource? glyph)
                                && glyph is GeneratedGlyphRunResource && !glyph.IsReleased;
                        }
                        if (command is MilCommand.DoubleBufferedBitmap or MilCommand.DoubleBufferedBitmapCopyForward)
                            transferred = _resources.TryGetResource(BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(4)), out GeneratedProtocolResource? target)
                                && target is GeneratedDoubleBufferedBitmapResource && !target.IsReleased;
                        if ((command == MilCommand.D3DImage && packet.Length == Marshal.SizeOf<MilD3DImageCommand>())
                            || (command == MilCommand.D3DImagePresent && packet.Length == Marshal.SizeOf<MilD3DImagePresentCommand>()))
                            transferred = _resources.TryGetResource(BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(4)), out GeneratedProtocolResource? image)
                                && image is GeneratedD3DImageResource && !image.IsReleased;
                        if (command == MilCommand.MediaPlayer && packet.Length == Marshal.SizeOf<MilMediaPlayerCommand>())
                            transferred = _resources.TryGetResource(BinaryPrimitives.ReadUInt32LittleEndian(packet.AsSpan(4)), out GeneratedProtocolResource? media)
                                && media is GeneratedMediaPlayerResource && !media.IsReleased;
                        int result;
                        if ((uint)command == 3)
                        {
                            result = packet.Length == 8 ? 0 : Direct3D9Factory.UceMalformedPacketHResult;
                            if (result >= 0) Partition.RegisterNotifications(this, BinaryPrimitives.ReadInt32LittleEndian(packet.AsSpan(4)) != 0);
                        }
                        else if (command == MilCommand.PartitionSetVBlankSyncMode)
                        {
                            result = packet.Length == 8
                                ? Partition.SetVBlankSyncMode(this, BinaryPrimitives.ReadInt32LittleEndian(packet.AsSpan(4)) != 0)
                                : Direct3D9Factory.UceMalformedPacketHResult;
                        }
                        else if (command == MilCommand.PartitionNotifyPresent)
                        {
                            result = packet.Length == 12 ? 0 : Direct3D9Factory.UceMalformedPacketHResult;
                            if (result >= 0) Partition.RequestPresentNotification(this, BinaryPrimitives.ReadUInt64LittleEndian(packet.AsSpan(4)));
                        }
                        else if (command == MilCommand.PartitionNotifyPolicyChangeForNonInteractiveMode)
                        {
                            result = packet.Length == 8 ? 0 : Direct3D9Factory.UceMalformedPacketHResult;
                            if (result >= 0) SameThreadPartition.RenderWithoutDisplays = BinaryPrimitives.ReadInt32LittleEndian(packet.AsSpan(4)) != 0;
                        }
                        else if (command == MilCommand.ChannelRequestTier)
                        {
                            result = packet.Length == 8 ? 0 : Direct3D9Factory.UceMalformedPacketHResult;
                            if (result >= 0) PostMessage(ChannelGraphicsCapabilities.CreateTierMessage(BinaryPrimitives.ReadInt32LittleEndian(packet.AsSpan(4))));
                        }
                        else result = _router.ProcessPacket(packet);
                        if (result < 0 && Partition.Failure >= 0)
                        {
                            PartitionScheduler.RecordCommandFailure(_channel, packet, result);
                            Partition.Failure = result;
                        }
                    }
                }
                catch (OutOfMemoryException error) { PreserveFailure(error); }
                catch (ArgumentException error) { PreserveFailure(error); }
                catch (InvalidOperationException error) { PreserveFailure(error); }
                catch (ExternalException error) { PreserveFailure(error); }
                catch (OverflowException error) { PreserveFailure(error); }
                finally
                {
                    if (!transferred) ReleasePacket(packet);
                }
            }
        }
        return Partition.Failure;
    }

    private void PreserveFailure(Exception error)
    {
        int result = PartitionScheduler.RecordFailure(error);
        if (Partition.Failure >= 0) Partition.Failure = result;
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
        if (command == MilCommand.MediaPlayer && packet.Length == Marshal.SizeOf<MilMediaPlayerCommand>())
        {
            nint media = (nint)MemoryMarshal.Read<MilMediaPlayerCommand>(packet).Media;
            packet.AsSpan(8, 8).Clear();
            Direct3D9Factory.Release(media);
        }
        if (command == MilCommand.GlyphRunCreate && packet.Length >= 76)
        {
            nint font = (nint)BinaryPrimitives.ReadUInt64LittleEndian(packet.AsSpan(8));
            packet.AsSpan(8, 8).Clear();
            Direct3D9Factory.Release(font);
        }
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
        if (command == MilCommand.D3DImage && packet.Length == Marshal.SizeOf<MilD3DImageCommand>())
        {
            var image = MemoryMarshal.Read<MilD3DImageCommand>(packet);
            packet.AsSpan(8).Clear();
            Direct3D9Factory.Release((nint)image.SoftwareBitmap);
            Direct3D9Factory.Release((nint)image.InteropBitmap);
        }
        if (command == MilCommand.D3DImagePresent && packet.Length == Marshal.SizeOf<MilD3DImagePresentCommand>())
        {
            nint handle = (nint)MemoryMarshal.Read<MilD3DImagePresentCommand>(packet).Event;
            packet.AsSpan(8).Clear();
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
        lock (_messages)
        {
            if (_destroyed) return;
            _destroyed = true;
            _notificationWindow = 0;
            _messages.Clear();
            // Wake before releasing ownership; active native waits retain their borrowed handle.
            try { _messageEvent.Set(); }
            finally { _messageEvent.Dispose(); }
        }
        _command = null;
        _commandLength = 0;
        Partition.RegisterNotifications(this, false);
        Partition.RemovePresentNotifications(this);
        Partition.SetVBlankSyncMode(this, false);
        if (IsRegistered)
        {
            Partition.Channels.Remove(_channel);
            IsRegistered = false;
        }
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
