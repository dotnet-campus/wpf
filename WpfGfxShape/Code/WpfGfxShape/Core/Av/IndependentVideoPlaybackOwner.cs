using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

internal sealed class IndependentVideoPlaybackOwner
{
    private readonly object _gate = new();
    private readonly BlockingCollection<Action> _commands = new();
    private readonly AutoResetEvent _commandReady = new(false);
    private readonly TaskCompletionSource _closed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private volatile bool _closing;
    private readonly Thread _thread;
    private readonly Task _completion;
    internal MediaFoundationSourceReader? ClosedReader { get; private set; }
    internal bool ThreadExited => !_thread.IsAlive;
    private bool _playing;
    private Task? _pendingRead, _flush;
    private TaskCompletionSource? _preroll, _seek;
    private long _seekTarget;
    private bool _scrub;
    private readonly Stopwatch _flushClock = new();
    private MediaFoundationSourceReader? _prepared;
    private TaskCompletionSource? _playCompletion;
    private Action<long>? _invalidated;
    private byte[]? _pending;
    private long _pendingTimestamp, _discardBefore, _origin;
    private bool _pendingEnd, _anchored;
    private readonly Stopwatch _clock = new();
    private double _rate = 1;
    internal long? Duration100ns { get; private set; }
    internal int DurationResult { get; private set; } = unchecked((int)0xc00d36e6);
    internal uint? Characteristics { get; private set; }
    internal bool? HasAudio { get; private set; }
    internal int AudioMetadataResult { get; private set; } = unchecked((int)0xc00d36e6);
    internal bool IsLocalSeekable { get; private set; }
    internal uint Width { get; private set; }
    internal uint Height { get; private set; }
    internal IndependentVideoFrameOwner Frames { get; } = new();
    internal event Action<long>? FrameReady;
    internal event Action? PlaybackEnded;
    internal event Action<int>? PlaybackFailed;
    internal bool Ended { get; private set; }
    internal int PublishedFrames { get; private set; }
    internal long LastTimestamp { get; private set; }
    private readonly object _positionGate = new();
    private long _position, _positionAnchor;
    private bool _positionRunning;
    internal long Position100ns
    {
        get { lock (_positionGate) return ReadPosition(); }
    }

    private long ScaleElapsed(long ticks)
    {
        double scaled = ticks * _rate;
        return scaled >= long.MaxValue ? long.MaxValue : (long)scaled;
    }

    private long ReadPosition()
    {
        long elapsed = _positionRunning ? ScaleElapsed(Stopwatch.GetElapsedTime(_positionAnchor).Ticks) : 0;
        long position = _position > long.MaxValue - elapsed ? long.MaxValue : _position + elapsed;
        return Duration100ns is long duration ? Math.Clamp(position, 0, duration) : position;
    }

    // Only owner control completion changes this timeline; frame publication retains its own PTS.
    private void SetPositionClock(bool running, long? position = null)
    {
        lock (_positionGate)
        {
            _position = _closing ? 0 : position ?? ReadPosition();
            _positionAnchor = Stopwatch.GetTimestamp();
            _positionRunning = running && !_closing;
        }
    }

    internal long Epoch { get; private set; }

    internal IndependentVideoPlaybackOwner()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        _thread = new(Run) { Name = nameof(IndependentVideoPlaybackOwner), IsBackground = false };
        _thread.SetApartmentState(ApartmentState.MTA);
        _thread.Start();
        _completion = WaitForExitAsync();
    }

    private Task Enqueue(Action action)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_closing, this);
            TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _commands.Add(() =>
            {
                if (_closing) { completion.SetCanceled(); return; }
                try { action(); completion.SetResult(); }
                // Command failures must reach their caller without terminating the MTA owner.
                catch (Exception error) { completion.SetException(error); }
            });
            _commandReady.Set();
            return completion.Task;
        }
    }

    internal Task OpenAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Enqueue(() =>
        {
            if (_prepared is not null) throw new InvalidOperationException();
            Marshal.ThrowExceptionForHR(MediaFoundationSourceReader.Create(path, out _prepared,
                captureMetadata: CaptureMetadata, callback: new MediaFoundationReaderCallback()));
            if (_prepared is null) throw new InvalidOperationException();
            Width = checked((uint)_prepared.DisplayWidth);
            Height = checked((uint)_prepared.DisplayHeight);
            Duration100ns = _prepared.Duration100ns;
            DurationResult = _prepared.DurationResult;
            Characteristics = _prepared.Characteristics;
            HasAudio = _prepared.HasAudio;
            AudioMetadataResult = _prepared.AudioMetadataResult;
            IsLocalSeekable = (Characteristics & 2) == 2 &&
                Path.IsPathFullyQualified(path) && !path.StartsWith(@"\\", StringComparison.Ordinal) &&
                (!Uri.TryCreate(path, UriKind.Absolute, out Uri? uri) || uri.IsFile && !uri.IsUnc);
        });
    }

    internal async Task PrerollAsync()
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await Enqueue(() =>
        {
            if (_prepared is null || _preroll is not null) throw new InvalidOperationException();
            if (_pending is not null) completion.TrySetResult();
            else _preroll = completion;
        }).ConfigureAwait(false);
        await completion.Task.ConfigureAwait(false);
    }

    // Compatibility entry used by the existing production rendering acceptance cases.
    internal async Task PlayAsync(string path, Action<long> frameInvalidated, double rate = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(frameInvalidated);
        TaskCompletionSource ended = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await Enqueue(() =>
        {
            if (_playCompletion is not null) throw new InvalidOperationException();
            if (_prepared is null)
            {
                Marshal.ThrowExceptionForHR(MediaFoundationSourceReader.Create(path, out _prepared,
                    callback: new MediaFoundationReaderCallback()));
                if (_prepared is null) throw new InvalidOperationException();
                Width = checked((uint)_prepared.DisplayWidth);
                Height = checked((uint)_prepared.DisplayHeight);
                Duration100ns = _prepared.Duration100ns;
                DurationResult = _prepared.DurationResult;
                Characteristics = _prepared.Characteristics;
            }
            SetRate(rate);
            _playCompletion = ended;
            _invalidated = frameInvalidated;
            Ended = false;
            _anchored = false;
            _playing = true;
            SetPositionClock(true);
        }).ConfigureAwait(false);
        await ended.Task.ConfigureAwait(false);
    }

    private void SetRate(double rate)
    {
        lock (_positionGate)
        {
            SetPositionClock(_positionRunning);
            if (_anchored)
            {
                _origin += Math.Min(long.MaxValue - _origin, ScaleElapsed(_clock.Elapsed.Ticks));
                _clock.Restart();
            }
            _rate = rate;
        }
    }

    internal Task SetPlayingAsync(bool playing, double? rate = null) => Enqueue(() =>
    {
        if (_prepared is null || (playing && Ended)) throw new InvalidOperationException();
        if (rate is > 0) SetRate(rate.Value);
        if (_playing == playing) return;
        _playing = playing;
        SetPositionClock(playing);
        _anchored = false;
    });

    internal async Task SeekAsync(long timestamp, bool stop = false, bool scrub = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(timestamp);
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await Enqueue(() =>
        {
            if (_prepared is null || _seek is not null) throw new InvalidOperationException();
            Marshal.ThrowExceptionForHR(IndependentMediaIdentity.ValidateSeek(timestamp, Duration100ns));
            if (!IsLocalSeekable) Marshal.ThrowExceptionForHR(unchecked((int)0x80004001));
            Marshal.ThrowExceptionForHR(_prepared.BeginFlush());
            SetPositionClock(false);
            _seek = completion;
            _seekTarget = timestamp;
            _flush = _prepared.PendingFlush;
            _flushClock.Restart();
            _pendingRead = null;
            _pending = null;
            _pendingEnd = false;
            if (stop) _playing = false;
            _scrub = scrub && !_playing;
        }).ConfigureAwait(false);
        await completion.Task.ConfigureAwait(false);
    }

    private void Pump()
    {
        if (_closing || _prepared is null) return;
        MediaFoundationSourceReader reader = _prepared;
        if (_flush is not null)
        {
            if (!_flush.IsCompleted)
            {
                if (_flushClock.Elapsed > TimeSpan.FromSeconds(10))
                    throw new TimeoutException();
                return;
            }
            _flush.GetAwaiter().GetResult();
            Marshal.ThrowExceptionForHR(reader.CompleteSeek(_seekTarget));
            _flush = null;
            Epoch++;
            _discardBefore = LastTimestamp = _seekTarget;
            SetPositionClock(_playing, _seekTarget);
            Ended = false;
            _anchored = false;
            if (!_scrub) { _seek?.TrySetResult(); _seek = null; }
        }
        if (_pendingRead is not null)
        {
            if (!_pendingRead.IsCompleted) return;
            _pendingRead.GetAwaiter().GetResult();
            _pendingRead = null;
            Marshal.ThrowExceptionForHR(reader.CompleteRead(out _, out uint flags, out long timestamp, out byte[]? pixels));
            _pendingEnd = (flags & 2) != 0;
            if (pixels is not null && timestamp >= _discardBefore)
            {
                _pending = pixels;
                _pendingTimestamp = timestamp;
            }
        }
        if (_pending is null)
        {
            if (_pendingEnd)
            {
                if (_preroll is not null || _scrub)
                    Marshal.ThrowExceptionForHR(unchecked((int)0x80070026));
                if (_playing) FinishPlayback();
                return;
            }
            if (_playing || _preroll is not null || _scrub)
            {
                Marshal.ThrowExceptionForHR(reader.BeginRead());
                _pendingRead = reader.PendingRead;
            }
            return;
        }
        _preroll?.TrySetResult();
        _preroll = null;
        if (_scrub)
        {
            lock (_gate)
            {
                if (_closing) return;
                Frames.Publish(_pending, reader.Width, reader.Height, reader.DisplayX, reader.DisplayY,
                    reader.DisplayWidth, reader.DisplayHeight, _pendingTimestamp);
                LastTimestamp = _pendingTimestamp;
                PublishedFrames++;
            }
            _pending = null;
            _scrub = false;
            if (!_closing) FrameReady?.Invoke(LastTimestamp);
            if (!_closing) _invalidated?.Invoke(LastTimestamp);
            _seek?.TrySetResult();
            _seek = null;
        }
        else if (_playing) Tick();
    }

    private void CaptureMetadata(MediaFoundationSourceReader reader)
    {
        Width = checked((uint)reader.DisplayWidth);
        Height = checked((uint)reader.DisplayHeight);
        Duration100ns = reader.Duration100ns;
        DurationResult = reader.DurationResult;
        Characteristics = reader.Characteristics;
        HasAudio = reader.HasAudio;
        AudioMetadataResult = reader.AudioMetadataResult;
    }

    internal Task CloseAsync()
    {
        lock (_gate)
        {
            if (!_closing)
            {
                _closing = true;
                SetPositionClock(false, 0);
                _prepared?.RequestCancel();
                _commands.CompleteAdding();
                _commandReady.Set();
            }
            return _completion;
        }
    }

    private async Task WaitForExitAsync()
    {
        try { await _closed.Task.ConfigureAwait(false); }
        finally { _thread.Join(); }
    }

    private void Run()
    {
        try
        {
            while (!_closing)
            {
                Task? waiting = _flush ?? _pendingRead;
                if (waiting is not null && !waiting.IsCompleted)
                {
                    int timeout = _flush is null ? Timeout.Infinite :
                        (int)Math.Clamp(10000 - _flushClock.ElapsedMilliseconds, 0, 10000);
                    if (_commands.Count == 0)
                        WaitHandle.WaitAny([_commandReady, ((IAsyncResult)waiting).AsyncWaitHandle], timeout);
                }
                else if (!_playing && _preroll is null && _seek is null && _commands.Count == 0)
                    _commandReady.WaitOne();
                if (_commands.TryTake(out Action? command))
                {
                    command();
                    continue;
                }
                try { Pump(); }
                catch (Exception error)
                {
                    _playing = false;
                    SetPositionClock(false, 0);
                    _preroll?.TrySetException(error);
                    _preroll = null;
                    _seek?.TrySetException(error);
                    _seek = null;
                    _scrub = false;
                    _flush = null;
                    _playCompletion?.TrySetException(error);
                    _playCompletion = null;
                    PlaybackFailed?.Invoke(error.HResult);
                }
            }
            // Already accepted commands must settle before close completes.
            while (_commands.TryTake(out Action? command)) command();
        }
        finally
        {
            List<Exception> errors = [];
            ClosedReader = _prepared;
            try { _prepared?.Dispose(); }
            catch (Exception error) { errors.Add(error); }
            finally { _prepared = null; _pending = null; }
            try { Frames.Dispose(); }
            catch (Exception error) { errors.Add(error); }
            _playCompletion?.TrySetCanceled();
            _preroll?.TrySetCanceled();
            _seek?.TrySetCanceled();
            try { _commands.Dispose(); }
            catch (Exception error) { errors.Add(error); }
            try { _commandReady.Dispose(); }
            catch (Exception error) { errors.Add(error); }
            // A reader failure must not skip independent resources or lose later failures.
            if (errors.Count == 0) _closed.TrySetResult();
            else _closed.TrySetException(errors.Count == 1 ? errors[0] : new AggregateException(errors));
        }
    }

    private void Tick()
    {
        MediaFoundationSourceReader reader = _prepared ?? throw new InvalidOperationException();
        if (_pending is null) return;
        if (!_anchored)
        {
            _origin = _pendingTimestamp;
            _clock.Restart();
            _anchored = true;
        }
        double remaining = (_pendingTimestamp - _origin) / _rate - _clock.Elapsed.Ticks;
        if (remaining > 0)
        {
            // A queued command interrupts the PTS wait; no cross-thread reader calls are needed.
            int milliseconds = (int)Math.Clamp((remaining + 9999) / 10000, 1, int.MaxValue);
            if (_commands.TryTake(out Action? command, milliseconds)) command();
            return;
        }
        lock (_gate)
        {
            if (_closing) return;
            Frames.Publish(_pending, reader.Width, reader.Height, reader.DisplayX, reader.DisplayY,
                reader.DisplayWidth, reader.DisplayHeight, _pendingTimestamp);
            LastTimestamp = _pendingTimestamp;
            PublishedFrames++;
        }
        _pending = null;
        // Close completion is after this callback boundary, never a synchronous self-join.
        if (!_closing) FrameReady?.Invoke(LastTimestamp);
        if (!_closing) _invalidated?.Invoke(LastTimestamp);
        if (_pendingEnd && !_closing) FinishPlayback();
    }

    private void FinishPlayback()
    {
        _playing = false;
        SetPositionClock(false, Duration100ns ?? Position100ns);
        Ended = true;
        _playCompletion?.TrySetResult();
        _playCompletion = null;
        PlaybackEnded?.Invoke();
    }
}
