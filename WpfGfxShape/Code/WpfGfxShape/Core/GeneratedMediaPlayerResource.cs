using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct MilMediaPlayerCommand
{
    internal readonly MilCommand Type;
    internal readonly uint Handle;
    internal readonly ulong Media;
    internal readonly int NotifyUceDirect;

    internal MilMediaPlayerCommand(uint handle, ulong media, int notifyUceDirect)
    {
        Type = MilCommand.MediaPlayer;
        Handle = handle;
        Media = media;
        NotifyUceDirect = notifyUceDirect;
    }
}

internal static partial class GeneratedProtocolPacketWriter
{
    // Serialization does not AddRef; the caller supplies one transport-owned reference.
    internal static byte[] WriteMediaPlayer(uint handle, ulong media, int notifyUceDirect)
        => WriteDrawing(new MilMediaPlayerCommand(handle, media, notifyUceDirect));
}

internal sealed unsafe partial class GeneratedMediaPlayerResource : GeneratedProtocolResource
{
    internal GeneratedMediaPlayerResource() : base(MilResourceType.MediaPlayer) { }

    private nint _systemMedia;
    private readonly object _systemGate = new();
    private Av.SystemMediaSource? _systemSource;
    private int _systemFramePending;
    private bool _systemCompositionRegistered;

    private Action? _systemListener;
    private void OnSystemEvent(Action listener)
    {
        lock (_systemGate)
        {
            if (_systemListener != listener || _systemMedia == 0 || IsReleased) return;
            Interlocked.Exchange(ref _systemFramePending, 1);
            _composition?.ScheduleCompositionPass();
        }
    }

    internal int AcquireSystemFrame(out Av.SystemVideoSample? sample)
    {
        sample = null;
        nint media;
        lock (_systemGate)
        {
            media = _systemMedia;
            if (media == 0 || IsReleased) return 0;
            Direct3D9Factory.AddRef(media);
        }
        Interlocked.Exchange(ref _systemFramePending, 0);
        try
        {
            int result = Av.SystemVideoSample.Acquire(media, false, true, out sample, out bool newFrame);
            if (result >= 0 && newFrame) NotifyFrameReady();
            return result;
        }
        finally
        {
            Direct3D9Factory.Release(media);
        }
    }

    private void ReleaseSystemMedia()
    {
        nint media;
        lock (_systemGate)
        {
            if (_systemListener is not null) _systemSource?.Unsubscribe(_systemListener);
            _systemListener = null;
            _systemSource = null;
            media = _systemMedia;
            _systemMedia = 0;
        }
        if (_systemCompositionRegistered)
        {
            _composition?.UnregisterVideo(this);
            _systemCompositionRegistered = false;
        }
        Direct3D9Factory.Release(media);
    }

    private bool _notifyUceDirect;
    internal bool NotifyUceDirect
    {
        get => Volatile.Read(ref _notifyUceDirect);
        private set => Volatile.Write(ref _notifyUceDirect, value);
    }

    internal int ProcessCommand(ReadOnlySpan<byte> packet)
    {
        if (IsReleased || packet.Length != Marshal.SizeOf<MilMediaPlayerCommand>())
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        MilMediaPlayerCommand value = MemoryMarshal.Read<MilMediaPlayerCommand>(packet);
        if (value.Type != MilCommand.MediaPlayer || (IntPtr.Size == 4 && value.Media > uint.MaxValue))
        {
            return Direct3D9Factory.UceMalformedPacketHResult;
        }

        NotifyUceDirect = value.NotifyUceDirect != 0;
        nint media = unchecked((nint)value.Media);
        nint provider = 0;
        try
        {
            if (media == 0)
            {
                return unchecked((int)0x80070006); // Native IFCNULL returns E_HANDLE.
            }

            Guid providerId = new("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7");
            void** vtable = *(void***)media;
            var queryInterface = (delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)vtable[0];
            int result = queryInterface(media, &providerId, &provider);
            if (result < 0)
            {
                return result;
            }

            if (provider == 0)
            {
                return Direct3D9Factory.NoInterfaceHResult;
            }

            Av.SystemMediaSource? source = Av.SystemMediaSource.Resolve(media);
            if (!_systemCompositionRegistered && _composition is not null)
            {
                int registration = _composition.RegisterVideo(this);
                if (registration < 0) return registration;
                _systemCompositionRegistered = true;
            }
            nint previous;
            lock (_systemGate)
            {
                if (_systemListener is not null) _systemSource?.Unsubscribe(_systemListener);
                previous = _systemMedia;
                _systemSource = source;
                _systemMedia = media;
                media = 0;
                Action? listener = null;
                listener = () => OnSystemEvent(listener!);
                _systemListener = listener;
                source?.Subscribe(listener);
            }
            Direct3D9Factory.Release(previous);
            NotifyChanged();
            return 0;
        }
        finally
        {
            Direct3D9Factory.Release(media);
            Direct3D9Factory.Release(provider);
        }
    }
}
