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
    private Av.IndependentVideoFrameOwner? _independentFrames;
    private Av.IndependentVideoPlaybackOwner? _independentPlayer;

    internal void BindIndependentPlayer(Av.IndependentVideoPlaybackOwner player)
    {
        ArgumentNullException.ThrowIfNull(player);
        lock (_systemGate)
        {
            ObjectDisposedException.ThrowIf(IsReleased, this);
            if (_independentPlayer is not null) _independentPlayer.FrameReady -= OnIndependentFrame;
            _independentPlayer = player;
            _independentFrames = player.Frames;
            player.FrameReady += OnIndependentFrame;
        }
        NotifyFrameReady();
    }

    private void OnIndependentFrame(long timestamp)
    {
        lock (_systemGate)
        {
            if (IsReleased || _independentPlayer is null) return;
            NotifyFrameReady();
        }
    }

    internal void BindIndependentFrames(Av.IndependentVideoFrameOwner frames)
    {
        ArgumentNullException.ThrowIfNull(frames);
        lock (_systemGate)
        {
            ObjectDisposedException.ThrowIf(IsReleased, this);
            _independentFrames = frames;
        }
        NotifyFrameReady();
    }
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
        NotifyFrameReady();
    }

    internal int AcquireSystemFrame(out Av.SystemVideoSample? sample)
    {
        sample = null;
        nint media;
        lock (_systemGate)
        {
            if (IsReleased) return 0;
            if (_systemSource is not null) return _systemSource.Acquire(out sample);
            if (_independentFrames is not null) return _independentFrames.Acquire(out sample);
            media = _systemMedia;
            if (media == 0) return 0;
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
            if (_independentPlayer is not null) _independentPlayer.FrameReady -= OnIndependentFrame;
            _independentPlayer = null;
            _independentFrames = null;
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

            Av.SystemMediaSource? source = Av.SystemMediaSource.Resolve(media);
            if (source is null)
            {
                // Accept caller-owned providers without loading or creating an external backend.
                Guid providerId = new("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7");
                int query = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)(*(void***)media)[0])(media, &providerId, &provider);
                if (query < 0) return query;
                if (provider == 0) return Direct3D9Factory.NoInterfaceHResult;
            }
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
                if (_independentPlayer is not null) _independentPlayer.FrameReady -= OnIndependentFrame;
                _independentPlayer = null;
                _independentFrames = null;
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
