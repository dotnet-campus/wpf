using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

// A single-use local PCM session. Completion includes owner-thread native cleanup.
internal sealed class IndependentAudioPlaybackOwner
{
    private readonly object _gate = new();
    private readonly CancellationTokenSource _cancel = new();
    private readonly AutoResetEvent _commandsReady = new(false);
    private readonly Queue<Command> _commands = new();
    private readonly TaskCompletionSource<int> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<int> _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<AudioMetadata> _metadata = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal sealed record AudioMetadata(AudioPcmFormat Format, ulong Duration100ns);
    internal Task<int> Ready => _ready.Task;
    internal Task<AudioMetadata> Metadata => _metadata.Task;
    internal bool InitiallyPaused { get; }
    internal double InitialBalance { get; }
    private bool _exited, _closing;
    private int _result;
    private Exception? _failure;
    internal Task<int> Completion => _completion.Task;
    internal IReadOnlyList<string> Evidence { get; private set; } = Array.Empty<string>();
    internal ulong SubmittedFrames { get; private set; }
    private long _position100ns;
    private ulong _duration100ns;
    private readonly AudioConsumptionTimeline _timeline = new();

    internal void ResetPosition(long mediaPosition100ns, ulong clockPosition, uint sampleRate)
    {
        lock (_gate)
        {
            _timeline.Reset(mediaPosition100ns, clockPosition, sampleRate);
            if (!_closing) _position100ns = mediaPosition100ns;
        }
    }

    internal void SubmitFrames(uint frames)
    {
        lock (_gate)
        {
            if (!_closing) _timeline.Submit(frames, frames);
        }
    }
    internal long Position100ns { get { lock (_gate) return _position100ns; } }

    internal static long ClockPosition100ns(ulong position, ulong frequency, ulong duration)
    {
        ArgumentOutOfRangeException.ThrowIfZero(frequency);
        UInt128 ticks = (UInt128)position * 10000000 / frequency;
        return (long)UInt128.Min(ticks, Math.Min(duration, (ulong)long.MaxValue));
    }

    internal void CapturePosition(ulong position, ulong frequency)
    {
        lock (_gate)
        {
            if (!_closing) _position100ns = Math.Max(_position100ns, _timeline.Position100ns(position, frequency, _duration100ns));
        }
    }

    internal WaitHandle CommandsReady => _commandsReady;

    internal Task<int> SeekAsync(long timestamp100ns)
    {
        if (timestamp100ns < 0) return Task.FromResult(unchecked((int)0x80070057));
        return Enqueue(CommandKind.Seek, 0, timestamp100ns);
    }

    internal enum CommandKind { Pause, Resume, Volume, Balance, Seek }
    internal sealed record Command(CommandKind Kind, double Value, long Timestamp100ns)
    {
        internal TaskCompletionSource<int> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    internal void NotifyOutputReady()
    {
        lock (_gate)
            _ready.TrySetResult(_closing ? unchecked((int)0x800704c7) : 0);
    }

    internal IndependentAudioPlaybackOwner(string path, bool initiallyPaused = false, double initialBalance = 0)
    {
        if (!double.IsFinite(initialBalance) || initialBalance < -1 || initialBalance > 1)
            throw new ArgumentOutOfRangeException(nameof(initialBalance));
        InitialBalance = initialBalance;
        InitiallyPaused = initiallyPaused;
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        Thread thread = new(() => Run(path)) { IsBackground = true, Name = nameof(IndependentAudioPlaybackOwner) };
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start();
    }

    // Completion acknowledges the native operation, never just queue admission.
    internal Task<int> PauseAsync() => Enqueue(CommandKind.Pause, 0);
    internal Task<int> ResumeAsync() => Enqueue(CommandKind.Resume, 0);
    internal Task<int> SetVolumeAsync(double value)
    {
        if (!double.IsFinite(value) || value < 0 || value > 1)
            return Task.FromResult(unchecked((int)0x80070057));
        return Enqueue(CommandKind.Volume, value);
    }

    internal Task<int> SetBalanceAsync(double value)
    {
        if (!double.IsFinite(value) || value < -1 || value > 1)
            return Task.FromResult(unchecked((int)0x80070057));
        return Enqueue(CommandKind.Balance, value);
    }

    private Task<int> Enqueue(CommandKind kind, double value, long timestamp100ns = 0)
    {
        lock (_gate)
        {
            if (_exited)
                return _failure is null ? Task.FromResult(_result < 0 ? _result : unchecked((int)0x8007139f)) : Task.FromException<int>(_failure);
            if (_closing) return Task.FromResult(unchecked((int)0x800704c7));
            Command command = new(kind, value, timestamp100ns);
            _commands.Enqueue(command);
            _commandsReady.Set();
            return command.Completion.Task;
        }
    }

    internal bool TryTakeCommand(out Command? command)
    {
        lock (_gate) return _commands.TryDequeue(out command);
    }

    internal async Task<int> CloseAsync(TimeSpan timeout)
    {
        lock (_gate)
        {
            _position100ns = 0;
            _timeline.Clear();
            _closing = true;
            if (!_exited)
            {
                _cancel.Cancel();
            }
        }
        // A timeout leaves native ownership with Run; it is not permission to release active COM.
        return await Completion.WaitAsync(timeout).ConfigureAwait(false);
    }

    private void Run(string path)
    {
        MediaFoundationAudioReader? reader = null;
        WasapiSharedOutput output = new();
        int result = 0;
        Exception? failure = null;
        try
        {
            _cancel.Token.ThrowIfCancellationRequested();
            result = MediaFoundationAudioReader.Create(path, out reader, requireAudioOnlyPcm: true);
            if (result >= 0 && reader is not null)
            {
                if (reader.Duration100ns is not ulong duration)
                    result = reader.DurationResult < 0 ? reader.DurationResult : unchecked((int)0xc00d36b4);
                else
                {
                    _duration100ns = duration;
                    _metadata.TrySetResult(new(reader.Format, duration));
                    _cancel.Token.ThrowIfCancellationRequested();
                    result = output.Render(reader, _cancel.Token, this);
                }
            }
            else if (result >= 0) result = unchecked((int)0x80004005);
        }
        catch (OperationCanceledException) { result = unchecked((int)0x800704c7); }
        catch (COMException error) { result = error.HResult; }
        catch (Exception error) when (error is IOException or ArgumentException or InvalidOperationException or OutOfMemoryException)
        {
            failure = error;
        }
        finally
        {
            int close = reader?.Close() ?? 0;
            if (result >= 0 && close < 0) result = close;
            List<string> evidence = (reader?.Evidence ?? []).Concat(output.Evidence).ToList();
            SubmittedFrames = output.SubmittedFrames;
            lock (_gate)
            {
                _result = result;
                _failure = failure;
                _exited = true;
                int initializationResult = result < 0 ? result : unchecked((int)0x8007139f);
                if (failure is null)
                {
                    _ready.TrySetResult(initializationResult);
                    _metadata.TrySetException(Marshal.GetExceptionForHR(initializationResult, new IntPtr(-1)) ?? new InvalidOperationException());
                }
                else
                {
                    _ready.TrySetException(failure);
                    _metadata.TrySetException(failure);
                }
                while (_commands.TryDequeue(out Command? command))
                {
                    int pendingResult = result < 0 ? result : unchecked((int)0x8007139f);
                    evidence.Add($"Command.{command.Kind}.NotExecuted:0x{pendingResult:X8}");
                    if (failure is null) command.Completion.SetResult(pendingResult);
                    else command.Completion.SetException(failure);
                }
                Evidence = evidence.ToArray();
                _commandsReady.Dispose();
                _cancel.Dispose();
            }
        }
        if (failure is null) _completion.SetResult(result);
        else _completion.SetException(failure);
    }
}
