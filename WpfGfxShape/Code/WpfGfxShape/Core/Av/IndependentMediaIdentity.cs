using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

internal enum IndependentMediaState { Closed, Opening, Opened, Playing, Paused, Stopped, Seeking, Ended, Closing, Failed }

// Independent transport identity shared by the public factory and resource bridge.
internal sealed class IndependentMediaIdentity
{
    private static readonly object RegistryGate = new();
    private static readonly Dictionary<nint, IndependentMediaIdentity> Registry = new();
    private static readonly nint Vtable = BuildVtable();
    private static readonly Guid MediaId = new("00000141-a8f2-4877-ba0a-fd2b6645fb94");
    private static readonly Guid UnknownId = new("00000000-0000-0000-c000-000000000046");
    private const int NotImplemented = unchecked((int)0x80004001);
    private const int PointerError = unchecked((int)0x80004003);
    private const int ClosedError = unchecked((int)0x80000013);
    private readonly object _gate = new();
    private uint _references = 1;
    private bool _closing;
    private bool _terminal;
    private bool _stopRequested;
    private Task _exitEvents = Task.CompletedTask;
    private string? _path;
    private object _sourceGeneration = new();
    private object _controlGeneration = new();
    private IndependentMediaState? _controlTarget;
    private bool _hasAudio;
    private bool _hasVideo;
    private double? _deferredRate;
    private double _targetRate = 1;
    private bool? _targetPlaying;
    private (long Timestamp, bool Stop)? _pendingSeek;
    private long? _seekPosition;
    private IndependentMediaState _seekAfter;
    private bool _scrubbingEnabled;
    private double _targetVolume = 0.5;
    private double _targetBalance;
    internal bool RequireVideoOnly { get; set; }
    internal double TargetVolume { get { lock (_gate) return _targetVolume; } }
    internal double TargetBalance { get { lock (_gate) return _targetBalance; } }
    private bool _uiFrameRequested;
    internal long UiRequestGeneration { get; private set; }
    internal long UiFrameGeneration { get; private set; }
    internal long UiConsumedGeneration { get; private set; }
    private IndependentVideoPlaybackOwner? _owner;
    private IndependentAudioPlaybackOwner? _audioOwner;
    private long? _audioDuration;
    private bool _audioAtOrigin;
    private Task _audioCompletion = Task.CompletedTask;
    private Task _audioSeekOperation = Task.CompletedTask;
    private Task _operation = Task.CompletedTask;
    private Task _cleanup = Task.CompletedTask;
    private MediaEventProxy? _events;
    private EventProxy? _registeredProxy;
    internal int AttachEvents(nint client)
    {
        int result = EventProxy.CreateCom(client, out EventProxy? proxy);
        if (result < 0 || proxy is null) return result;
        try { return AttachEvents(proxy); }
        finally { proxy.Release(); }
    }

    internal int AttachEvents(EventProxy proxy)
    {
        lock (_gate)
        {
            if (_terminal || (_closing && State != IndependentMediaState.Closed)) return ClosedError;
            if (_registeredProxy is not null) return unchecked((int)0x8007139F);
            if (proxy.AddRef() == 0) return ClosedError;
            _registeredProxy = proxy;
            return 0;
        }
    }

    private void Notify(IndependentVideoPlaybackOwner owner, uint kind, int failure = 0)
    {
        MediaEventProxy? events;
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_owner, owner)) return;
            events = _events;
        }
        // Dispose rejects queued work; an already admitted callback retains its own proxy reference.
        int result = events?.RaiseEvent(kind, failure) ?? 0;
        lock (_gate) { if (!_closing && ReferenceEquals(_owner, owner) && result < 0) Failure = result; }
    }
    internal Task Completion { get { lock (_gate) return _operation; } }
    internal IndependentMediaState State { get; private set; }
    internal IndependentVideoPlaybackOwner? Owner { get { lock (_gate) return _owner; } }
    internal int Failure { get; private set; }
    internal nint Pointer { get; }
    internal SystemMediaSource Source { get; }

    private readonly HashSet<string> _frameDiagnostics = new();

    internal int AcquireFrame(out SystemVideoSample? sample)
    {
        lock (_gate)
        {
            sample = null;
            int result;
            string phase;
            if (_terminal) { result = ClosedError; phase = "terminal"; }
            else if (State == IndependentMediaState.Failed)
            {
                // Failure is reported by MediaFailed; an absent frame is not a broken render target.
                result = 0;
                phase = "failed-no-source";
            }
            else if (_closing || State is IndependentMediaState.Closed or IndependentMediaState.Opening)
            {
                // Native DrawVideo accepts no source; an unopened/closing player is not a broken lease.
                result = 0;
                phase = "lifecycle-absence";
            }
            else if (_audioOwner is not null) { result = 0; phase = "audio-no-frame"; }
            else if (_owner is null) { result = unchecked((int)0x8007139F); phase = "missing-owner"; }
            else { result = _owner.Frames.Acquire(out sample); phase = "owner-acquire"; }
            string? path = Environment.GetEnvironmentVariable("WPFGFX_DIAGNOSTIC_LOG");
            if (!string.IsNullOrWhiteSpace(path))
            {
                string key = $"{phase}/{State}/{result:X8}/{sample is not null}";
                if (_frameDiagnostics.Add(key))
                    System.IO.File.AppendAllText(path, $"VideoFrame tick={Environment.TickCount64} phase={phase} state={State} closing={_closing} terminal={_terminal} owner={_owner is not null} uiRequest={UiRequestGeneration} uiFrame={UiFrameGeneration} hr=0x{result:X8} source={sample is not null} pts={sample?.Timestamp} lease=caller-owned{Environment.NewLine}");
            }
            return result;
        }
    }

    private unsafe IndependentMediaIdentity()
    {
        Source = new SystemMediaSource(this);
        Pointer = (nint)NativeMemory.Alloc((nuint)sizeof(nint));
        *(nint*)Pointer = Vtable;
    }

    internal static unsafe IndependentMediaIdentity Create()
    {
        IndependentMediaIdentity value = new();
        try { lock (RegistryGate) Registry.Add(value.Pointer, value); }
        catch { NativeMemory.Free((void*)value.Pointer); throw; }
        return value;
    }

    internal static IndependentMediaIdentity? Resolve(nint pointer)
    {
        lock (RegistryGate) return Registry.GetValueOrDefault(pointer);
    }

    private int BeginOpen(string? path)
    {
        MediaEventProxy? retiredEvents = null;
        bool sourceChanged = false;
        try
        {
            lock (_gate)
            {
                if (_terminal) return ClosedError;
                // Compare the actual source submitted to MF, not a lossy URI normalization.
                if (!_closing && string.Equals(_path, path, StringComparison.Ordinal)) return 0;
                MediaEventProxy? events = null;
                if (path is not null && _registeredProxy is not null)
                {
                    int result = MediaEventProxy.Create(_registeredProxy, out events);
                    if (result < 0) return result;
                }
                IndependentVideoPlaybackOwner? owner;
                try { owner = path is null ? null : new IndependentVideoPlaybackOwner(); }
                catch { retiredEvents = events; throw; }
                retiredEvents = _events;
                retiredEvents?.Invalidate();
                IndependentVideoPlaybackOwner? retiredOwner = _owner;
                // SharedState survives Open; only the replacement's metadata may supersede it.
                if (_closing && path is not null)
                    _hasAudio = _hasVideo = false;
                else if (State != IndependentMediaState.Opening && retiredOwner is not null)
                {
                    _hasAudio = retiredOwner?.HasAudio == true;
                    _hasVideo = HasAvailableVideo() || State == IndependentMediaState.Failed &&
                        retiredOwner is { Width: > 0, Height: > 0 };
                }
                Task audioCleanup = RetireAudio();
                _owner = owner;
                _events = events;
                _cleanup = CompleteCloseAsync(retiredOwner,
                    Task.WhenAll(_operation, _exitEvents, _cleanup, audioCleanup), retiredEvents);
                _closing = false;
                _stopRequested = false;
                _pendingSeek = null;
                _seekPosition = null;
                _deferredRate = _targetPlaying.HasValue ? (_targetPlaying.Value ? _targetRate : 0) : null;
                // UI requests belong to the media identity, not the replacement source.
                _path = path;
                object generation = _sourceGeneration = new();
                _controlGeneration = new();
                _controlTarget = null;
                sourceChanged = true;
                Failure = 0;
                State = path is null ? IndependentMediaState.Closed : IndependentMediaState.Opening;
                if (owner is null) _operation = _cleanup = CompleteClearSourceAsync(_cleanup, generation);
                else
                {
                    owner.FrameReady += timestamp => OnFrame(owner, timestamp);
                    owner.PlaybackEnded += () => OnEnded(owner);
                    owner.PlaybackFailed += failure => OnFailed(owner, failure);
                    _operation = ObserveOpenAsync(owner, path!);
                }
                return 0;
            }
        }
        finally
        {
            retiredEvents?.Dispose();
            // Resource acquisition takes the resource lock before _gate; notify only after releasing _gate.
            if (sourceChanged) Source.NotifyFrame();
        }
    }

    private async Task CompleteClearSourceAsync(Task cleanup, object generation)
    {
        await cleanup.ConfigureAwait(false);
        lock (_gate)
        {
            // Like BeginUrlArc, clear presence at source commit, not at Open submission.
            if (_terminal || _closing || _owner is not null || _path is not null ||
                !ReferenceEquals(_sourceGeneration, generation)) return;
            _hasAudio = _hasVideo = false;
        }
    }

    private void PublishMetadata(IndependentVideoPlaybackOwner owner)
    {
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_owner, owner)) return;
            _hasAudio = owner.HasAudio == true;
            _hasVideo = owner is { Width: > 0, Height: > 0 };
        }
    }

    private bool TryCommitOpen(IndependentVideoPlaybackOwner owner)
    {
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_owner, owner)) return false;
            State = IndependentMediaState.Opened;
            // Reader negotiation alone is not the playback state's metadata commit.
            PublishMetadata(owner);
            return true;
        }
    }

    private async Task ObserveOpenAsync(IndependentVideoPlaybackOwner owner, string path)
    {
        try
        {
            lock (_gate)
            {
                if (_closing || !ReferenceEquals(_owner, owner)) return;
                if (_targetPlaying.HasValue && (!double.IsFinite(_targetRate) || _targetRate < 0))
                    Marshal.ThrowExceptionForHR(NotImplemented);
            }
            if (RequireVideoOnly)
            {
                // Probe owns a separate COM/MF balance; never infer content from the URL.
                var probe = await Task.Run(() =>
                {
                    int hr = MediaFoundationAudioReader.ProbeAudioOnlyPcm(path, out bool audio);
                    return (Result: hr, Audio: audio);
                }).ConfigureAwait(false);
                Marshal.ThrowExceptionForHR(probe.Result);
                if (probe.Audio)
                {
                    await owner.CloseAsync().ConfigureAwait(false);
                    IndependentAudioPlaybackOwner audio;
                    object generation;
                    lock (_gate)
                    {
                        if (_closing || !ReferenceEquals(_owner, owner)) return;
                        generation = _sourceGeneration;
                        // A new paused reader is already at the origin. WPF Open submits Position=0
                        // after the native Open; this is not an audio seek or Stop request.
                        if (_deferredRate is double requestedRate && requestedRate is not (0 or 1))
                            Marshal.ThrowExceptionForHR(NotImplemented);
                        audio = _audioOwner = new IndependentAudioPlaybackOwner(path, initiallyPaused: true, initialBalance: _targetBalance);
                        _audioAtOrigin = true;
                        _owner = null;
                    }
                    await ObserveAudioOpenAsync(audio, generation).ConfigureAwait(false);
                    return;
                }
            }
            await owner.OpenAsync(path).ConfigureAwait(false);
            if (RequireVideoOnly)
            {
                if (owner.HasAudio is null)
                    Marshal.ThrowExceptionForHR(owner.AudioMetadataResult < 0 ? owner.AudioMetadataResult : unchecked((int)0xc00d36e6));
                if (owner.HasAudio == true)
                    Marshal.ThrowExceptionForHR(unchecked((int)0xc00d36b4));
            }
            MediaEventProxy? events;
            while (true)
            {
                await owner.PrerollAsync().ConfigureAwait(false);
                (long Timestamp, bool Stop)? pending;
                bool scrub;
                lock (_gate)
                {
                    if (_closing || !ReferenceEquals(_owner, owner)) return;
                    pending = _pendingSeek;
                    _pendingSeek = null;
                    scrub = _scrubbingEnabled && !(_deferredRate > 0);
                    if (pending is null)
                    {
                        if (!TryCommitOpen(owner)) return;
                        _seekPosition = null;
                        events = _events;
                        break;
                    }
                }
                await owner.SeekAsync(pending.Value.Timestamp, pending.Value.Stop,
                    scrub && !pending.Value.Stop).ConfigureAwait(false);
            }
            events?.RaiseEvent(1);
            events?.RaiseEvent(11);
            Task control = Task.CompletedTask;
            lock (_gate)
            {
                if (!_closing && ReferenceEquals(_owner, owner) && _deferredRate is double rate)
                {
                    _deferredRate = null;
                    BeginPlay(rate);
                    // Play owns the whole playback lifetime; only Pause has a finite control completion.
                    if (rate == 0) control = _operation;
                }
            }
            // Open must not wait for the newly admitted playback to reach its end.
            await control.ConfigureAwait(false);
        }
        catch (Exception error)
        {
            PublishMetadata(owner);
            lock (_gate)
            {
                if (ReferenceEquals(_owner, owner) && !_closing)
                {
                    Failure = error.HResult;
                    _uiFrameRequested = false;
                    _deferredRate = null;
                    _pendingSeek = null;
                    _seekPosition = null;
                }
            }
            Exception? cleanupError = null;
            try { await owner.CloseAsync().ConfigureAwait(false); }
            catch (Exception closeError) { cleanupError = closeError; }
            MediaEventProxy? events = null;
            lock (_gate)
            {
                if (ReferenceEquals(_owner, owner) && !_closing && State != IndependentMediaState.Failed)
                {
                    Failure = error.HResult;
                    State = IndependentMediaState.Failed;
                    events = _events;
                }
            }
            // Capture this epoch's proxy before dispatch: callbacks may close or release the identity.
            events?.RaiseEvent(8, error.HResult);
            if (cleanupError is not null) throw new AggregateException(error, cleanupError);
        }
    }

    private Task RetireAudio()
    {
        IndependentAudioPlaybackOwner? audio = _audioOwner;
        _audioOwner = null;
        _audioAtOrigin = false;
        _audioDuration = null;
        if (audio is null) return Task.CompletedTask;
        return Task.WhenAll(audio.CloseAsync(Timeout.InfiniteTimeSpan), audio.Completion, _audioCompletion, _audioSeekOperation);
    }

    private async Task ObserveAudioOpenAsync(IndependentAudioPlaybackOwner audio, object generation)
    {
        int result = await AudioResultAsync(audio.Ready).ConfigureAwait(false);
        if (audio.Metadata.IsCompletedSuccessfully)
        {
            var metadata = await audio.Metadata.ConfigureAwait(false);
            lock (_gate)
            {
                if (_closing || !ReferenceEquals(_sourceGeneration, generation) || !ReferenceEquals(_audioOwner, audio)) return;
                _hasAudio = true;
                _hasVideo = false;
                _audioDuration = checked((long)metadata.Duration100ns);
            }
        }
        if (result >= 0) result = await AudioResultAsync(audio.SetVolumeAsync(TargetVolume)).ConfigureAwait(false);
        MediaEventProxy? events;
        double? rate;
        while (result >= 0)
        {
            (long Timestamp, bool Stop) pending;
            lock (_gate)
            {
                if (_closing || !ReferenceEquals(_sourceGeneration, generation) || !ReferenceEquals(_audioOwner, audio)) return;
                if (_pendingSeek is not { } target)
                {
                    State = IndependentMediaState.Opened;
                    _seekPosition = null;
                    break;
                }
                pending = target;
                _pendingSeek = null;
            }
            result = ValidateSeek(pending.Timestamp, _audioDuration);
            if (result >= 0 && (pending.Timestamp != 0 || pending.Stop || !_audioAtOrigin))
                result = await SubmitAudioSeekAsync(audio, pending.Timestamp, pending.Stop).ConfigureAwait(false);
        }
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_sourceGeneration, generation) || !ReferenceEquals(_audioOwner, audio)) return;
            if (State == IndependentMediaState.Failed && result >= 0) result = Failure;
            if (result < 0)
            {
                _pendingSeek = null;
                _seekPosition = null;
                State = IndependentMediaState.Failed;
            }
            Failure = result < 0 ? result : 0;
            events = _events;
            rate = _deferredRate;
            _deferredRate = null;
        }
        events?.RaiseEvent(result < 0 ? 8u : 1u, result < 0 ? result : 0);
        if (result < 0) { await audio.CloseAsync(Timeout.InfiniteTimeSpan).ConfigureAwait(false); return; }
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_sourceGeneration, generation) || !ReferenceEquals(_audioOwner, audio)) return;
            _audioCompletion = ObserveAudioCompletionAsync(audio, generation);
            if (rate.HasValue) BeginPlay(rate.Value);
        }
    }

    private async Task ObserveAudioCompletionAsync(IndependentAudioPlaybackOwner audio, object generation)
    {
        int result;
        try { result = await audio.Completion.ConfigureAwait(false); }
        catch (Exception error) when (error is IOException or ArgumentException or InvalidOperationException or OutOfMemoryException)
        {
            result = error.HResult;
        }
        MediaEventProxy? events;
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_sourceGeneration, generation) || !ReferenceEquals(_audioOwner, audio) || result >= 0) return;
            events = State == IndependentMediaState.Failed ? null : _events;
            Failure = result;
            _pendingSeek = null;
            _seekPosition = null;
            State = IndependentMediaState.Failed;
        }
        events?.RaiseEvent(8, result);
    }

    private async Task ObserveAudioControlAsync(IndependentAudioPlaybackOwner audio, Task<int> command, IndependentMediaState? target)
    {
        object generation = _controlGeneration;
        object sourceGeneration = _sourceGeneration;
        int result = await AudioResultAsync(command).ConfigureAwait(false);
        MediaEventProxy? events = null;
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_sourceGeneration, sourceGeneration) || !ReferenceEquals(_audioOwner, audio)) return;
            if (result < 0)
            {
                if (State != IndependentMediaState.Failed) events = _events;
                Failure = result;
                State = IndependentMediaState.Failed;
            }
            else if (target.HasValue && ReferenceEquals(_controlGeneration, generation) && State != IndependentMediaState.Failed)
            {
                _controlTarget = null;
                State = target.Value;
            }
        }
        events?.RaiseEvent(8, result);
    }

    private int BeginPlay(double rate)
    {
        lock (_gate)
        {
            if (_terminal) return ClosedError;
            // With no source this is target admission, not an owner/device command.
            if (_owner is null && _audioOwner is null && State == IndependentMediaState.Closed)
            {
                if (rate != 0) _targetRate = rate;
                _targetPlaying = rate != 0;
                _deferredRate = rate;
                return 0;
            }
            if (_closing) return ClosedError;
            if (!double.IsFinite(rate) || rate < 0) return NotImplemented;
            if (_audioOwner is not null && rate is not (0 or 1)) return NotImplemented;
            if (rate != 0) _targetRate = rate;
            _targetPlaying = rate != 0;
            if (_audioOwner is not null)
            {
                if (rate is not (0 or 1)) return NotImplemented;
                if (State == IndependentMediaState.Failed) return Failure;
                if (State is IndependentMediaState.Opening or IndependentMediaState.Seeking)
                {
                    _deferredRate = rate;
                    return 0;
                }
                _controlGeneration = new();
                _controlTarget = rate == 0 ? IndependentMediaState.Paused : IndependentMediaState.Playing;
                var audio = _audioOwner;
                if (rate == 1) _audioAtOrigin = false;
                _operation = Task.WhenAll(_operation, ObserveAudioControlAsync(audio,
                    rate == 0 ? audio.PauseAsync() : audio.ResumeAsync(),
                    rate == 0 ? IndependentMediaState.Paused : IndependentMediaState.Playing));
                return 0;
            }
            // WmpPlayer submits target updates independently of asynchronous decode failure.
            if (State is IndependentMediaState.Closed or IndependentMediaState.Opening or IndependentMediaState.Seeking or IndependentMediaState.Failed)
            {
                _deferredRate = rate;
                return 0;
            }
            if (_owner is null || State is not (IndependentMediaState.Opened or IndependentMediaState.Playing or IndependentMediaState.Paused or IndependentMediaState.Stopped or IndependentMediaState.Ended))
                return unchecked((int)0x8007139F);
            if (rate > 0 && State == IndependentMediaState.Ended)
            {
                _controlGeneration = new();
                _controlTarget = null;
                _seekAfter = IndependentMediaState.Playing;
                State = IndependentMediaState.Seeking;
                _operation = ObserveReplayAsync(_owner, rate);
            }
            else if (rate > 0 && State == IndependentMediaState.Opened && _path is not null)
            {
                _controlGeneration = new();
                _controlTarget = null;
                State = IndependentMediaState.Playing;
                _operation = ObservePlayAsync(_owner, _path, rate);
            }
            else
            {
                _controlTarget = rate > 0 ? IndependentMediaState.Playing : IndependentMediaState.Paused;
                _operation = ObserveControlAsync(_owner, _owner.SetPlayingAsync(rate > 0, rate), _controlTarget.Value);
            }
            return 0;
        }
    }

    private async Task ObserveReplayAsync(IndependentVideoPlaybackOwner owner, double rate)
    {
        try
        {
            await owner.SeekAsync(0).ConfigureAwait(false);
            await ObserveSeekAsync(owner, owner.SetPlayingAsync(true, rate), IndependentMediaState.Playing).ConfigureAwait(false);
        }
        catch (Exception error) { OnFailed(owner, error.HResult); }
    }

    private async Task ObservePlayAsync(IndependentVideoPlaybackOwner owner, string path, double rate)
    {
        try
        {
            await owner.PlayAsync(path, static _ => { }, rate).ConfigureAwait(false);
            lock (_gate) { if (!_closing && ReferenceEquals(_owner, owner) && owner.Ended) State = IndependentMediaState.Ended; }
        }
        catch (OperationCanceledException) when (_closing) { }
        catch (Exception error)
        {
            OnFailed(owner, error.HResult);
        }
    }

    private void OnFrame(IndependentVideoPlaybackOwner owner, long timestamp)
    {
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_owner, owner)) return;
            UiFrameGeneration++;
            if (_uiFrameRequested)
            {
                _uiFrameRequested = false;
                UiConsumedGeneration = UiRequestGeneration;
            }
        }
        Source.NotifyFrame();
        Notify(owner, 13);
    }

    private int RequestUiFrame()
    {
        lock (_gate)
        {
            if (_closing) return ClosedError;
            // Like CompositionNotifier, reserve the next composition notification, not a replay of an old frame.
            UiRequestGeneration++;
            _uiFrameRequested = true;
            return 0;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UiFrame(nint self) => Resolve(self)?.RequestUiFrame() ?? ClosedError;
    private void OnEnded(IndependentVideoPlaybackOwner owner)
    {
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_owner, owner)) return;
            _controlGeneration = new();
            _controlTarget = null;
            State = IndependentMediaState.Ended;
        }
        Notify(owner, 7);
    }
    private void OnFailed(IndependentVideoPlaybackOwner owner, int failure)
    {
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_owner, owner) || State == IndependentMediaState.Failed) return;
            _pendingSeek = null;
            _seekPosition = null;
            Failure = failure;
            _controlGeneration = new();
            _controlTarget = null;
            State = IndependentMediaState.Failed;
        }
        Notify(owner, 8, failure);
    }
    private async Task ObserveControlAsync(IndependentVideoPlaybackOwner owner, Task command, IndependentMediaState state)
    {
        object generation;
        lock (_gate) generation = _controlGeneration = new();
        try
        {
            await command.ConfigureAwait(false);
            lock (_gate)
            {
                // FIFO commands do not order their asynchronously dispatched continuations.
                if (!_closing && ReferenceEquals(_owner, owner) && ReferenceEquals(_controlGeneration, generation) &&
                    State is not (IndependentMediaState.Ended or IndependentMediaState.Failed))
                {
                    _controlTarget = null;
                    State = state;
                    if (_deferredRate is double rate)
                    {
                        _deferredRate = null;
                        BeginPlay(rate);
                    }
                }
            }
        }
        catch (Exception error) { OnFailed(owner, error.HResult); }
    }
    private async Task ObserveSeekAsync(IndependentVideoPlaybackOwner owner, Task command, IndependentMediaState after)
    {
        try
        {
            while (true)
            {
                await command.ConfigureAwait(false);
                lock (_gate)
                {
                    if (_closing || !ReferenceEquals(_owner, owner) || State == IndependentMediaState.Failed) return;
                    if (_pendingSeek is { } pending)
                    {
                        _pendingSeek = null;
                        after = _seekAfter;
                        bool scrub = _scrubbingEnabled && after != IndependentMediaState.Playing && !pending.Stop;
                        // Only this chain owns SeekAsync: MF flush and scrub must settle before the next seek.
                        command = owner.SeekAsync(pending.Timestamp, pending.Stop, scrub);
                        continue;
                    }
                    // This chain owns every accepted seek; an older completion must drain newer intent first.
                    _seekPosition = null;
                    State = after;
                    if (_deferredRate is double rate)
                    {
                        _deferredRate = null;
                        BeginPlay(rate);
                    }
                    return;
                }
            }
        }
        catch (Exception error) { OnFailed(owner, error.HResult); }
    }

    private static async Task<int> AudioResultAsync(Task<int> command)
    {
        try { return await command.ConfigureAwait(false); }
        catch (Exception error) when (error is COMException or IOException or ArgumentException or InvalidOperationException or OutOfMemoryException or OperationCanceledException)
        {
            return error.HResult;
        }
    }

    private async Task<int> SubmitAudioSeekAsync(IndependentAudioPlaybackOwner audio, long timestamp, bool stop)
    {
        lock (_gate)
        {
            if (_closing || !ReferenceEquals(_audioOwner, audio)) return unchecked((int)0x800704c7);
            _audioAtOrigin = false;
        }
        int result = stop ? await AudioResultAsync(audio.PauseAsync()).ConfigureAwait(false) : 0;
        if (result >= 0) result = await AudioResultAsync(audio.SeekAsync(timestamp)).ConfigureAwait(false);
        return result;
    }

    private async Task ObserveAudioSeekAsync(IndependentAudioPlaybackOwner audio, object generation, Task<int> command)
    {
        while (true)
        {
            int result = await AudioResultAsync(command).ConfigureAwait(false);
            MediaEventProxy? events = null;
            lock (_gate)
            {
                if (_closing || !ReferenceEquals(_audioOwner, audio) || !ReferenceEquals(_sourceGeneration, generation)) return;
                if (result < 0 || State == IndependentMediaState.Failed)
                {
                    if (result < 0)
                    {
                        if (State != IndependentMediaState.Failed) events = _events;
                        Failure = result;
                        State = IndependentMediaState.Failed;
                    }
                    _pendingSeek = null;
                    _seekPosition = null;
                }
                else if (_pendingSeek is { } pending)
                {
                    _pendingSeek = null;
                    command = SubmitAudioSeekAsync(audio, pending.Timestamp, pending.Stop);
                    continue;
                }
                else
                {
                    _audioAtOrigin = audio.Position100ns == 0;
                    _seekPosition = null;
                    State = _seekAfter;
                    if (_deferredRate is double rate)
                    {
                        _deferredRate = null;
                        BeginPlay(rate);
                    }
                }
            }
            events?.RaiseEvent(8, result);
            return;
        }
    }

    private int BeginSeek(long timestamp, bool stop = false)
    {
        lock (_gate)
        {
            if (_closing) return ClosedError;
            if (_audioOwner is { } audio)
            {
                int audioBoundary = ValidateSeek(timestamp, State == IndependentMediaState.Opening ? null : _audioDuration);
                if (audioBoundary < 0) return audioBoundary;
                if (_audioAtOrigin && timestamp == 0 && !stop && _seekPosition is null) return 0;
                if (State == IndependentMediaState.Failed) return Failure;
                _seekPosition = timestamp;
                if (State is IndependentMediaState.Opening or IndependentMediaState.Seeking)
                {
                    _pendingSeek = (timestamp, stop);
                    if (stop) _seekAfter = IndependentMediaState.Stopped;
                    return 0;
                }
                _seekAfter = stop ? IndependentMediaState.Stopped : (_controlTarget ?? State) == IndependentMediaState.Playing
                    ? IndependentMediaState.Playing : IndependentMediaState.Paused;
                _controlGeneration = new();
                _controlTarget = null;
                State = IndependentMediaState.Seeking;
                _audioSeekOperation = ObserveAudioSeekAsync(audio, _sourceGeneration, SubmitAudioSeekAsync(audio, timestamp, stop));
                _operation = Task.WhenAll(_operation, _audioSeekOperation);
                return 0;
            }
            int boundary = ValidateSeek(timestamp, State is IndependentMediaState.Closed or IndependentMediaState.Opening or IndependentMediaState.Failed ? null : _owner?.Duration100ns);
            if (boundary < 0) return boundary;
            // A failed open still accepts target intent; only a new Open may start another owner.
            if (State is IndependentMediaState.Closed or IndependentMediaState.Opening or IndependentMediaState.Failed)
            {
                _pendingSeek = (timestamp, stop);
                _seekPosition = timestamp;
                return 0;
            }
            if (_owner is null) return NotImplemented;
            IndependentMediaState intent = State == IndependentMediaState.Seeking ? _seekAfter : _controlTarget ?? State;
            IndependentMediaState after = stop ? IndependentMediaState.Stopped : intent == IndependentMediaState.Playing
                ? IndependentMediaState.Playing : IndependentMediaState.Paused;
            bool scrub = _scrubbingEnabled && after != IndependentMediaState.Playing && !stop;
            if (scrub && _owner.Duration100ns is null)
                return _owner.DurationResult < 0 ? _owner.DurationResult : NotImplemented;
            _controlGeneration = new();
            _controlTarget = null;
            _seekAfter = after;
            _seekPosition = timestamp;
            if (State == IndependentMediaState.Seeking)
            {
                _pendingSeek = (timestamp, stop);
                return 0;
            }
            State = IndependentMediaState.Seeking;
            _operation = ObserveSeekAsync(_owner, _owner.SeekAsync(timestamp, stop, scrub), after);
            return 0;
        }
    }

    private int BeginClose(bool terminal = false)
    {
        MediaEventProxy? events = null;
        EventProxy? registration = null;
        lock (_gate)
        {
            _terminal |= terminal;
            _targetRate = 1;
            _targetPlaying = null;
            _deferredRate = null;
            if (terminal)
            {
                registration = _registeredProxy;
                _registeredProxy = null;
            }
            if (!_stopRequested)
            {
                _stopRequested = true;
                _closing = true;
                _controlGeneration = new();
                _controlTarget = null;
                _pendingSeek = null;
                _seekPosition = null;
                _uiFrameRequested = false;
                _deferredRate = null;
                events = _events;
                _events = null;
                IndependentVideoPlaybackOwner? owner = _owner;
                // Close clears source values, not the SharedState presence cache.
                if (State != IndependentMediaState.Opening && owner is not null)
                {
                    _hasAudio = owner.HasAudio == true;
                    _hasVideo = HasAvailableVideo() || State == IndependentMediaState.Failed &&
                        owner is { Width: > 0, Height: > 0 };
                }
                _owner = null;
                State = IndependentMediaState.Closed;
                _operation = _cleanup = CompleteCloseAsync(owner, Task.WhenAll(_operation, _exitEvents, _cleanup, RetireAudio()), events);
            }
        }
        // Releasing a client may reenter the player; never do that under the identity lock.
        try { events?.Dispose(); }
        finally { registration?.Release(); }
        return 0;
    }

    private int BeginProcessExit()
    {
        MediaEventProxy? events;
        EventProxy? registration;
        lock (_gate)
        {
            _terminal = _closing = true;
            _controlGeneration = new();
            _controlTarget = null;
            registration = _registeredProxy;
            _registeredProxy = null;
            _uiFrameRequested = false;
            _deferredRate = null;
            events = _events;
            _events = null;
            if (events is not null) _exitEvents = events.Completion;
        }
        // WmpPlayer::ProcessExitHandler only shuts down the proxy. No MF operation or wait here.
        try { registration?.Shutdown(); events?.Shutdown(); }
        finally
        {
            try { events?.Dispose(); }
            finally { registration?.Release(); }
        }
        return 0;
    }

    private Task CompleteCloseAsync(IndependentVideoPlaybackOwner? owner, Task pending, MediaEventProxy? events)
    {
        return Task.WhenAll(owner?.CloseAsync() ?? Task.CompletedTask, pending,
            events?.Completion ?? Task.CompletedTask);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int Query(nint self, Guid* iid, nint* output)
    {
        if (output == null) return PointerError;
        *output = 0;
        if (iid == null) return PointerError;
        lock (RegistryGate)
        {
            if (!Registry.TryGetValue(self, out IndependentMediaIdentity? value)) return ClosedError;
            if (*iid != MediaId && *iid != UnknownId) return unchecked((int)0x80004002);
            value._references++;
            *output = self;
            return 0;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(nint self)
    {
        lock (RegistryGate) return Registry.TryGetValue(self, out IndependentMediaIdentity? value) ? ++value._references : 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe uint Release(nint self)
    {
        IndependentMediaIdentity? value;
        lock (RegistryGate)
        {
            if (!Registry.TryGetValue(self, out value)) return 0;
            if (--value._references != 0) return value._references;
            Registry.Remove(self);
        }
        value.BeginClose(true);
        _ = value.FreeIdentityAsync();
        return 0;
    }

    private async Task FreeIdentityAsync()
    {
        Task completion = Completion;
        try { await completion.ConfigureAwait(false); }
        catch (Exception error)
        {
            // IUnknown.Release returns a reference count, not an asynchronous cleanup HRESULT.
            // Read the aggregate: awaiting WhenAll alone only rethrows one of its failures.
            Console.Error.WriteLine(completion.Exception ?? error);
        }
        finally { FreeIdentity(); }
    }

    private unsafe void FreeIdentity() => NativeMemory.Free((void*)Pointer);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int Open(nint self, char* path)
    {
        try { return Resolve(self)?.BeginOpen(path == null ? null : new string(path)) ?? ClosedError; }
        catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
        catch (Exception error) { return error.HResult; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Close(nint self) => Resolve(self)?.BeginClose() ?? ClosedError;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Shutdown(nint self) => Resolve(self)?.BeginClose(true) ?? ClosedError;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int ProcessExitHandler(nint self) => Resolve(self)?.BeginProcessExit() ?? ClosedError;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Unsupported(nint self) => Resolve(self)?._closing == false ? NotImplemented : ClosedError;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Seek(nint self, long time)
    {
        try { return Resolve(self)?.BeginSeek(time) ?? ClosedError; }
        catch (Exception error) { return error.HResult; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Stop(nint self)
    {
        try { return Resolve(self)?.BeginSeek(0, true) ?? ClosedError; }
        catch (Exception error) { return error.HResult; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Rate(nint self, double rate)
    {
        try { return Resolve(self)?.BeginPlay(rate) ?? ClosedError; }
        catch (Exception error) { return error.HResult; }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Volume(nint self, double value) => Resolve(self)?.SetAudioTarget(value, false) ?? ClosedError;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Balance(nint self, double value) => Resolve(self)?.SetAudioTarget(value, true) ?? ClosedError;

    private int SetAudioTarget(double value, bool balance)
    {
        if (!double.IsFinite(value) || value < (balance ? -1 : 0) || value > 1)
            return unchecked((int)0x80070057);
        lock (_gate)
        {
            if (_closing || _terminal) return ClosedError;
            if (_audioOwner is not null)
            {
                if (State == IndependentMediaState.Failed) return Failure;
                Task<int> command = balance ? _audioOwner.SetBalanceAsync(value) : _audioOwner.SetVolumeAsync(value);
                if (command.IsCompletedSuccessfully && command.Result < 0) return command.Result;
                // Target cache records admission; the observer reports native failure, never rewrites targets.
                _operation = Task.WhenAll(_operation, ObserveAudioControlAsync(_audioOwner, command, null));
            }
            if (balance) _targetBalance = value;
            else _targetVolume = value;
            return 0;
        }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Scrub(nint self, byte value)
    {
        IndependentMediaIdentity? media = Resolve(self);
        if (media is null) return ClosedError;
        lock (media._gate)
        {
            if (media._closing) return ClosedError;
            if (media.State is IndependentMediaState.Failed or IndependentMediaState.Seeking)
                return unchecked((int)0x8007139F);
            media._scrubbingEnabled = value != 0;
            return 0;
        }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int Position(nint self, long* value)
    {
        if (value == null) return PointerError;
        *value = 0;
        IndependentMediaIdentity? media = Resolve(self);
        if (media is null) return ClosedError;
        lock (media._gate)
        {
            if (media._terminal) return ClosedError;
            if (media._audioOwner is not null)
            {
                *value = media._seekPosition ?? media._audioOwner.Position100ns;
                return 0;
            }
            *value = media._seekPosition is long target
                ? target
                : media.State == IndependentMediaState.Ended && media._owner?.Duration100ns is long duration
                    ? duration : media._owner?.Position100ns ?? 0;
            return 0;
        }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int BooleanUnsupported(nint self, byte* value)
    {
        if (value == null) return PointerError;
        *value = 0;
        return Resolve(self)?._closing == false ? NotImplemented : ClosedError;
    }
    internal static bool HasAvailableVideo(IndependentMediaState state, uint width, uint height)
        => width > 0 && height > 0 && state is IndependentMediaState.Opened or IndependentMediaState.Playing
            or IndependentMediaState.Paused or IndependentMediaState.Stopped or IndependentMediaState.Seeking or IndependentMediaState.Ended;

    internal static int ValidateSeek(long timestamp, long? duration)
        => timestamp < 0 ? unchecked((int)0x80070057)
            : duration.HasValue && timestamp > duration.Value ? unchecked((int)0xc00d36b2) : 0;

    internal static int QueryLength(IndependentMediaState state, long? duration, int metadataResult, out long value)
    {
        value = 0;
        if (state == IndependentMediaState.Closing) return ClosedError;
        if (state is IndependentMediaState.Closed or IndependentMediaState.Opening) return 0;
        if (state == IndependentMediaState.Failed)
        {
            if (duration is >= 0) value = duration.Value;
            return 0;
        }
        if (state is not (IndependentMediaState.Opened or IndependentMediaState.Playing or IndependentMediaState.Paused
            or IndependentMediaState.Stopped or IndependentMediaState.Seeking or IndependentMediaState.Ended))
            return unchecked((int)0x8007139F);
        if (!duration.HasValue) return metadataResult < 0 ? metadataResult : unchecked((int)0xc00d36e6);
        if (duration.Value < 0) return unchecked((int)0xc00d36b4);
        value = duration.Value;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int CanPause(nint self, byte* value)
    {
        if (value == null) return PointerError;
        *value = 0;
        IndependentMediaIdentity? media = Resolve(self);
        if (media is null) return ClosedError;
        lock (media._gate)
        {
            if (media._terminal) return ClosedError;
            if (media.HasAvailableVideo() && (media._owner?.Characteristics & 4) == 4) *value = 1;
            return 0;
        }
    }

    private bool HasAvailableVideo() => HasAvailableVideo(State, _owner?.Width ?? 0, _owner?.Height ?? 0);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int Video(nint self, byte* value)
    {
        if (value == null) return PointerError;
        *value = 0;
        IndependentMediaIdentity? media = Resolve(self);
        if (media is null) return ClosedError;
        lock (media._gate)
        {
            if (media._terminal) return ClosedError;
            *value = (media.State is IndependentMediaState.Opening or IndependentMediaState.Closed ? media._hasVideo :
                media.HasAvailableVideo() || media.State == IndependentMediaState.Failed &&
                media._owner is { Width: > 0, Height: > 0 }) ? (byte)1 : (byte)0;
            return 0;
        }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int Progress(nint self, double* value)
    {
        if (value == null) return PointerError;
        *value = 0;
        IndependentMediaIdentity? media = Resolve(self);
        if (media is null) return ClosedError;
        lock (media._gate)
        {
            if (media._terminal) return ClosedError;
            if (media.State is IndependentMediaState.Closed or IndependentMediaState.Opening) return 0;
            int result = media.LocalStatus();
            if (result >= 0) *value = 1;
            return result;
        }
    }

    private int LocalStatus()
    {
        if (_closing) return ClosedError;
        if (!HasAvailableVideo()) return unchecked((int)0x8007139F);
        return _owner?.IsLocalSeekable == true ? 0 : NotImplemented;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int Buffering(nint self, byte* value)
    {
        if (value == null) return PointerError;
        *value = 0;
        IndependentMediaIdentity? media = Resolve(self);
        if (media is null) return ClosedError;
        lock (media._gate) return media.LocalStatus();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int AudioPresent(nint self, byte* value)
    {
        if (value == null) return PointerError;
        *value = 0;
        IndependentMediaIdentity? media = Resolve(self);
        if (media is null) return ClosedError;
        lock (media._gate)
        {
            if (media._terminal) return ClosedError;
            if (media._audioOwner is not null)
            {
                *value = media._hasAudio ? (byte)1 : (byte)0;
                return 0;
            }
            // WmpPlayer keeps player-level metadata across source Close.
            if (media.State is IndependentMediaState.Opening or IndependentMediaState.Closed)
            {
                *value = media._hasAudio ? (byte)1 : (byte)0;
                return 0;
            }
            // Failed reports the last known metadata, independently of frame availability.
            if (media.State == IndependentMediaState.Failed)
            {
                *value = media._owner?.HasAudio == true ? (byte)1 : (byte)0;
                return 0;
            }
            if (!media.HasAvailableVideo()) return unchecked((int)0x8007139F);
            if (media._owner?.HasAudio is not bool audio)
                return media._owner?.AudioMetadataResult is int hr && hr < 0 ? hr : NotImplemented;
            *value = audio ? (byte)1 : (byte)0;
            return 0;
        }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int Height(nint self, uint* value) => Dimension(self, value, false);
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int Width(nint self, uint* value) => Dimension(self, value, true);
    private static unsafe int Dimension(nint self, uint* value, bool width)
    {
        if (value == null) return PointerError;
        *value = 0;
        IndependentMediaIdentity? media = Resolve(self);
        if (media is null) return ClosedError;
        lock (media._gate)
        {
            if (media._terminal) return ClosedError;
            if ((media.HasAvailableVideo() || media.State == IndependentMediaState.Failed) && media._owner is { } owner)
                *value = width ? owner.Width : owner.Height;
            return 0;
        }
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int Length(nint self, long* value)
    {
        if (value == null) return PointerError;
        *value = 0;
        IndependentMediaIdentity? media = Resolve(self);
        if (media is null) return ClosedError;
        lock (media._gate)
        {
            if (media._terminal) return ClosedError;
            if (media._audioOwner is not null && media._audioDuration is long audioDuration)
            {
                *value = audioDuration;
                return 0;
            }
            int hr = QueryLength(media.State, media._owner?.Duration100ns,
                media._owner?.DurationResult ?? unchecked((int)0xc00d36e6), out long length);
            *value = length;
            return hr;
        }
    }

    private static unsafe nint BuildVtable()
    {
        nint* table = (nint*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IndependentMediaIdentity), 24 * sizeof(nint));
        table[0] = (nint)(delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)&Query;
        table[1] = (nint)(delegate* unmanaged[Stdcall]<nint, uint>)&AddRef;
        table[2] = (nint)(delegate* unmanaged[Stdcall]<nint, uint>)&Release;
        table[3] = (nint)(delegate* unmanaged[Stdcall]<nint, char*, int>)&Open;
        table[4] = (nint)(delegate* unmanaged[Stdcall]<nint, int>)&Stop;
        table[5] = (nint)(delegate* unmanaged[Stdcall]<nint, int>)&Close;
        table[6] = (nint)(delegate* unmanaged[Stdcall]<nint, long*, int>)&Position;
        table[7] = (nint)(delegate* unmanaged[Stdcall]<nint, long, int>)&Seek;
        table[8] = (nint)(delegate* unmanaged[Stdcall]<nint, double, int>)&Rate;
        table[9] = (nint)(delegate* unmanaged[Stdcall]<nint, double, int>)&Volume;
        table[10] = (nint)(delegate* unmanaged[Stdcall]<nint, double, int>)&Balance;
        table[11] = (nint)(delegate* unmanaged[Stdcall]<nint, byte, int>)&Scrub;
        table[12] = (nint)(delegate* unmanaged[Stdcall]<nint, byte*, int>)&Buffering;
        table[17] = (nint)(delegate* unmanaged[Stdcall]<nint, byte*, int>)&AudioPresent;
        table[13] = (nint)(delegate* unmanaged[Stdcall]<nint, byte*, int>)&CanPause;
        table[14] = table[15] = (nint)(delegate* unmanaged[Stdcall]<nint, double*, int>)&Progress;
        table[16] = (nint)(delegate* unmanaged[Stdcall]<nint, byte*, int>)&Video;
        table[18] = (nint)(delegate* unmanaged[Stdcall]<nint, uint*, int>)&Height;
        table[19] = (nint)(delegate* unmanaged[Stdcall]<nint, uint*, int>)&Width;
        table[20] = (nint)(delegate* unmanaged[Stdcall]<nint, long*, int>)&Length;
        table[21] = (nint)(delegate* unmanaged[Stdcall]<nint, int>)&UiFrame;
        table[22] = (nint)(delegate* unmanaged[Stdcall]<nint, int>)&Shutdown;
        table[23] = (nint)(delegate* unmanaged[Stdcall]<nint, int>)&ProcessExitHandler;
        return (nint)table;
    }
}
