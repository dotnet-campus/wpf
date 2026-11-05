using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace WpfGfxShape.Core.Av;

// Opt-in backend only. URL creation remains synchronous and is not cancellable.
internal sealed class CancellableVideoReaderOwner
{
    private readonly object _gate = new();
    private readonly BlockingCollection<Action> _commands = new();
    private readonly TaskCompletionSource _exit = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Thread _thread;
    private readonly MediaFoundationReaderCallback _callback = new();
    private MediaFoundationSourceReader? _reader;
    private bool _closing;
    private readonly AutoResetEvent _commandReady = new(false);
    private Task? _pendingRead;
    private TaskCompletionSource? _readCompletion;
    private Action<uint, long, byte[]?>? _receive;
    private Task? _close;
    internal MediaFoundationSourceReader? Reader => _reader;
    internal MediaFoundationReaderCallback Callback => _callback;
    internal IndependentVideoFrameOwner Frames { get; } = new();
    internal bool ThreadExited => !_thread.IsAlive;

    internal CancellableVideoReaderOwner()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        _thread = new(Run) { Name = nameof(CancellableVideoReaderOwner), IsBackground = false };
        _thread.SetApartmentState(ApartmentState.MTA);
        _thread.Start();
    }

    private void Run()
    {
        try
        {
            while (!_commands.IsCompleted)
            {
                if (_commands.TryTake(out Action? action)) { action(); continue; }
                if (_pendingRead is null) { _commandReady.WaitOne(); continue; }
                if (!_pendingRead.IsCompleted)
                {
                    WaitHandle.WaitAny([_commandReady, ((IAsyncResult)_pendingRead).AsyncWaitHandle]);
                    continue;
                }
                try
                {
                    _pendingRead.GetAwaiter().GetResult();
                    int hr = _reader!.CompleteRead(out _, out uint flags, out long timestamp, out byte[]? pixels);
                    Marshal.ThrowExceptionForHR(hr);
                    lock (_gate) { if (_closing) throw new OperationCanceledException(); }
                    _receive!(flags, timestamp, pixels);
                    _readCompletion!.TrySetResult();
                }
                catch (OperationCanceledException) { _readCompletion!.TrySetCanceled(); }
                catch (Exception error) { _readCompletion!.TrySetException(error); }
                finally { _pendingRead = null; _readCompletion = null; _receive = null; }
            }
            _exit.TrySetResult();
        }
        catch (Exception error) { _exit.TrySetException(error); }
    }

    private Task Enqueue(Action action)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_closing, this);
            TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _commands.Add(() =>
            {
                try
                {
                    if (_closing) { completion.SetCanceled(); return; }
                    action(); completion.SetResult();
                }
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
            if (_reader is not null) throw new InvalidOperationException();
            Marshal.ThrowExceptionForHR(MediaFoundationSourceReader.Create(path, out _reader, callback: _callback));
        });
    }

    internal async Task ReadAsync(Action<uint, long, byte[]?> receive)
    {
        ArgumentNullException.ThrowIfNull(receive);
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await Enqueue(() =>
        {
            if (_reader is null || _pendingRead is not null) throw new InvalidOperationException();
            Marshal.ThrowExceptionForHR(_reader.BeginRead());
            _pendingRead = _reader.PendingRead;
            _readCompletion = completion;
            _receive = receive;
        }).ConfigureAwait(false);
        await completion.Task.ConfigureAwait(false);
    }

    internal Task CloseAsync()
    {
        lock (_gate)
        {
            if (_close is not null) return _close;
            _closing = true;
            _callback.RequestClose();
            TaskCompletionSource cleanup = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _commands.Add(() =>
            {
                try
                {
                    _readCompletion?.TrySetCanceled();
                    _pendingRead = null;
                    if (_reader is not null) Marshal.ThrowExceptionForHR(_reader.Close());
                    else _callback.ReleaseOwner();
                    cleanup.SetResult();
                    _commands.CompleteAdding();
                }
                catch (Exception error) { cleanup.SetException(error); _commands.CompleteAdding(); }
            });
            _commandReady.Set();
            _close = FinishCloseAsync(cleanup.Task);
            return _close;
        }
    }

    private async Task FinishCloseAsync(Task cleanup)
    {
        await cleanup.ConfigureAwait(false);
        await _exit.Task.ConfigureAwait(false);
        await Task.Run(() => _thread.Join()).ConfigureAwait(false);
    }
}
