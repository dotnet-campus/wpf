using System.Runtime.InteropServices;

namespace WpfGfxShape.Core;

// The queue owns submitted batches until execution or discard on the composition thread.
internal sealed class PartitionScheduler : IDisposable
{
    internal sealed class Work(SameThreadChannel channel, Queue<List<byte[]>>? batches, bool destroy = false, bool register = false)
    {
        internal readonly SameThreadChannel Channel = channel;
        internal Queue<List<byte[]>>? Batches = batches;
        internal readonly bool Destroy = destroy;
        internal readonly bool Register = register;
        internal bool Compose = true;
        internal readonly TaskCompletionSource<int> Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private readonly object _gate = new();
    private readonly Queue<Work> _pending = [];
    private readonly HashSet<SameThreadPartition> _scheduled = [];

    internal void ScheduleCompositionPass(SameThreadPartition partition)
    {
        lock (_gate)
        {
            if (_stopping) return;
            _scheduled.Add(partition);
            Monitor.Pulse(_gate);
        }
    }
    private readonly Thread _thread;
    private readonly TaskCompletionSource<int> _initialized = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _stopping;
    private int _initializationResult;
    internal bool IsCurrentThread => Thread.CurrentThread == _thread;

    internal PartitionScheduler()
    {
        _thread = new Thread(Run) { IsBackground = true };
        _thread.SetApartmentState(ApartmentState.MTA);
        _thread.Start();
        _initializationResult = _initialized.Task.GetAwaiter().GetResult();
    }

    internal int InitializationResult => _initializationResult;

    internal static void RecordCommandFailure(uint channel, ReadOnlySpan<byte> packet, int result)
    {
        string message = $"UCE command failure: channel={channel}, thread={Environment.CurrentManagedThreadId}, HRESULT=0x{result:X8}, packet={Convert.ToHexString(packet)}";
        System.Diagnostics.Trace.TraceError(message);
        string? path = Environment.GetEnvironmentVariable("WPFGFX_DIAGNOSTIC_LOG");
        if (!string.IsNullOrWhiteSpace(path))
            System.IO.File.AppendAllText(path, message + Environment.NewLine);
    }

    internal static int RecordFailure(Exception error)
    {
        System.Diagnostics.Trace.TraceError("UCE failure: {0}", error);
        string? path = Environment.GetEnvironmentVariable("WPFGFX_DIAGNOSTIC_LOG");
        if (!string.IsNullOrWhiteSpace(path))
            System.IO.File.AppendAllText(path, error + Environment.NewLine);
        return error is System.ComponentModel.Win32Exception native
            ? unchecked((int)(0x80070000u | ((uint)native.NativeErrorCode & 0xffff)))
            : error.HResult;
    }

    internal int UpdatePriority(int priority)
    {
        if (priority is not (-2 or -1 or 0 or 1 or 2 or 15 or -15)) return unchecked((int)0x80070057);
        try
        {
            _thread.Priority = priority switch
            {
                <= -2 => ThreadPriority.Lowest,
                -1 => ThreadPriority.BelowNormal,
                0 => ThreadPriority.Normal,
                1 => ThreadPriority.AboveNormal,
                _ => ThreadPriority.Highest
            };
            return 0;
        }
        catch (ThreadStateException error) { return RecordFailure(error); }
        catch (System.ComponentModel.Win32Exception error) { return RecordFailure(error); }
        catch (UnauthorizedAccessException error) { return RecordFailure(error); }
    }

    internal Work SubmitBatches(SameThreadChannel channel, bool destroy = false, bool barrier = false)
    {
        var work = new Work(channel, null, destroy);
        lock (_gate)
        {
            if (_stopping) throw new InvalidOperationException();
            _pending.EnsureCapacity(_pending.Count + 1);
            work.Batches = channel.DetachBatches();
            work.Compose = !barrier || work.Batches.Count != 0;
            _pending.Enqueue(work);
            Monitor.Pulse(_gate);
        }
        return work;
    }

    internal void Submit(Work work)
    {
        lock (_gate)
        {
            if (_stopping) throw new InvalidOperationException();
            _pending.Enqueue(work);
            Monitor.Pulse(_gate);
        }
    }

    private void Run()
    {
        int initialized = CoInitializeEx(0, 0);
        _initialized.SetResult(initialized);
        if (initialized < 0) return;
        try
        {
            while (true)
            {
                Work? work = null;
                SameThreadPartition? scheduled = null;
                lock (_gate)
                {
                    while (_pending.Count == 0 && _scheduled.Count == 0 && !_stopping) Monitor.Wait(_gate);
                    if (_pending.Count != 0) work = _pending.Dequeue();
                    else if (_scheduled.Count != 0)
                    {
                        scheduled = _scheduled.First();
                        _scheduled.Remove(scheduled);
                    }
                    else return;
                }
                if (scheduled is not null)
                {
                    try { if (scheduled.Channels.Count != 0) scheduled.Compose(); }
                    catch (OutOfMemoryException error) { if (scheduled.Failure >= 0) scheduled.Failure = RecordFailure(error); }
                    catch (ArgumentException error) { if (scheduled.Failure >= 0) scheduled.Failure = RecordFailure(error); }
                    catch (InvalidOperationException error) { if (scheduled.Failure >= 0) scheduled.Failure = RecordFailure(error); }
                    catch (ExternalException error) { if (scheduled.Failure >= 0) scheduled.Failure = RecordFailure(error); }
                    catch (OverflowException error) { if (scheduled.Failure >= 0) scheduled.Failure = RecordFailure(error); }
                    continue;
                }
                if (work is null) continue;
                int result = 0;
                try
                {
                    if (work.Register) work.Channel.Register();
                    else if (work.Destroy)
                    {
                        try { result = work.Channel.CommitSubmitted(work.Batches!); }
                        finally { work.Channel.Destroy(); }
                    }
                    else
                    {
                        result = work.Channel.CommitSubmitted(work.Batches!);
                        SameThreadPartition partition = work.Channel.Partition;
                        if (result >= 0 && work.Compose)
                        {
                            result = partition.Compose();
                        }
                        partition.FlushPendingBadShaderNotifications();
                    }
                }
                catch (OutOfMemoryException error) { result = RecordFailure(error); }
                catch (ArgumentException error) { result = RecordFailure(error); }
                catch (InvalidOperationException error) { result = RecordFailure(error); }
                catch (ExternalException error) { result = RecordFailure(error); }
                catch (OverflowException error) { result = RecordFailure(error); }
                finally
                {
                    if (result < 0)
                        RecordCommandFailure(0, ReadOnlySpan<byte>.Empty, result);
                    if (result < 0 && work.Channel.Partition.Failure >= 0) work.Channel.Partition.Failure = result;
                    int failure = work.Channel.Partition.Failure;
                    if (failure < 0) work.Channel.WakeMessageWaiters();
                    // The native zombie back channel exposes only OOM/OOVM, not internal command failures.
                    work.Completion.SetResult(failure < 0
                        ? failure == unchecked((int)0x8876017C) ? failure
                            : Direct3D9Factory.IsOutOfMemory(failure) ? Direct3D9Factory.OutOfMemoryHResult
                            : unchecked((int)0x88980406)
                        : result);
                }
            }
        }
        finally { CoUninitialize(); }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _stopping = true;
            Monitor.PulseAll(_gate);
        }
        // Closing from external COM callbacks must not join the composition thread itself.
        if (!IsCurrentThread) _thread.Join();
    }

    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern int CoInitializeEx(nint reserved, uint flags);
    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern void CoUninitialize();
}
