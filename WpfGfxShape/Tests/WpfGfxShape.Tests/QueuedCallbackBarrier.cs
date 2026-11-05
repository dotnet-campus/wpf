namespace WpfGfxShape.Tests;

internal sealed class QueuedCallbackBarrier
{
    private readonly object _gate = new();
    private readonly TaskCompletionSource _admitted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _resume = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _drained = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _started, _cancelled;

    internal Task Admitted => _admitted.Task;
    internal Task Drained => _drained.Task;

    internal void Run()
    {
        lock (_gate)
        {
            if (_cancelled) return;
            _started = true;
        }
        try
        {
            _admitted.TrySetResult();
            _resume.Task.GetAwaiter().GetResult();
        }
        finally { _drained.TrySetResult(); }
    }

    internal Task ReleaseAndDrainAsync()
    {
        lock (_gate)
        {
            _cancelled = true;
            _resume.TrySetResult();
            // A queued but unstarted callback is cancelled under the same admission lock.
            if (!_started) _drained.TrySetResult();
            return _drained.Task;
        }
    }
}
