using System.Collections.Concurrent;
using NativeAcceptance.Pcore;

namespace WpfGfxShape.AcceptanceLifecycle.Tests;

[TestClass]
public sealed class DispatcherWorkersTests
{
    [TestMethod]
    public void WhenNoWorkersStartedThenCleanupCompletes()
    {
        var owner = new DispatcherWorkers();
        var failures = new ConcurrentQueue<string>();
        int joined = 0;
        owner.Cleanup(failures, 0, ref joined);
        Assert.IsEmpty(failures);
    }

    [TestMethod]
    public void WhenWorkerWaitsForStartThenCleanupReleasesAndJoinsIt()
    {
        var owner = new DispatcherWorkers();
        var thread = new Thread(() => owner.Start.Wait()) { IsBackground = true };
        thread.Start();
        owner.Started.Add(thread);
        int joined = 0;
        owner.Cleanup(new ConcurrentQueue<string>(), 0, ref joined);
        Assert.AreEqual(1, joined);
    }

    [TestMethod]
    public void WhenJoinDeadlineExpiresThenSynchronizationRemainsUsable()
    {
        var owner = new DispatcherWorkers();
        var thread = new Thread(() => owner.Start.Wait()) { IsBackground = true };
        thread.Start();
        owner.Started.Add(thread);
        bool stopped = owner.JoinUntil(TimeSpan.Zero);
        int joined = 0;
        owner.Cleanup(new ConcurrentQueue<string>(), 0, ref joined);
        Assert.IsFalse(stopped);
    }

    [TestMethod]
    public void WhenWorkerRemainsAliveThenCleanupRetainsSynchronizationAndReportsFailure()
    {
        var owner = new DispatcherWorkers();
        var release = new ManualResetEventSlim();
        var thread = new Thread(() => { release.Wait(); owner.Ready.Signal(); }) { IsBackground = true };
        thread.Start();
        owner.Started.Add(thread);
        var failures = new ConcurrentQueue<string>();
        int joined = 0;
        try
        {
            owner.Cleanup(failures, 0, ref joined);
        }
        finally
        {
            release.Set();
            thread.Join(TimeSpan.FromSeconds(5));
        }
        Assert.HasCount(1, failures);
    }
}
