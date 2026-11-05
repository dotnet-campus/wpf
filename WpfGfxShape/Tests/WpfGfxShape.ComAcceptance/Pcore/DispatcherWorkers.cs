using System.Collections.Concurrent;
using System.Diagnostics;

namespace NativeAcceptance.Pcore;

internal sealed class DispatcherWorkers
{
    private static readonly ConcurrentQueue<DispatcherWorkers> Retained = new();
    internal readonly CountdownEvent Ready = new(3);
    internal readonly ManualResetEventSlim Start = new();
    internal readonly CancellationTokenSource Stop = new();
    internal readonly List<Thread> Started = new();

    internal bool JoinUntil(TimeSpan limit)
    {
        var elapsed = Stopwatch.StartNew();
        bool stopped = true;
        foreach (Thread thread in Started)
        {
            TimeSpan remaining = limit - elapsed.Elapsed;
            bool exited = thread.Join(remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero);
            stopped &= exited;
        }
        return stopped;
    }

    internal void Cleanup(ConcurrentQueue<string> failures, int cycle, ref int joined)
    {
        Stop.Cancel();
        Start.Set();
        bool stopped = JoinUntil(TimeSpan.FromSeconds(10));
        joined += Started.Count(thread => !thread.IsAlive);
        if (!stopped)
        {
            Retained.Enqueue(this);
            failures.Enqueue($"cycle={cycle}: Dispatcher cleanup deadline expired; synchronization ownership retained until process exit.");
            return;
        }
        Ready.Dispose();
        Start.Dispose();
        Stop.Dispose();
    }
}
