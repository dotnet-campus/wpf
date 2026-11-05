namespace WpfGfxShape.Tests;

[TestClass]
public sealed class QueuedCallbackBarrierTests
{
    [TestMethod]
    public async Task WhenAdmissionTimesOutThenDelayedCallbackIsCancelledAsync()
    {
        var barrier = new QueuedCallbackBarrier();
        await Assert.ThrowsAsync<TimeoutException>(() => barrier.Admitted.WaitAsync(TimeSpan.Zero));
        await barrier.ReleaseAndDrainAsync();
        barrier.Run();
        Assert.IsFalse(barrier.Admitted.IsCompleted);
    }

    [TestMethod]
    public async Task WhenAdmissionIsCancelledThenDelayedCallbackIsCancelledAsync()
    {
        var barrier = new QueuedCallbackBarrier();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() => barrier.Admitted.WaitAsync(cancellation.Token));
        await barrier.ReleaseAndDrainAsync();
        barrier.Run();
        Assert.IsTrue(barrier.Drained.IsCompletedSuccessfully);
    }

    [TestMethod]
    public async Task WhenCallbackIsAdmittedThenReleaseDrainsItAsync()
    {
        var barrier = new QueuedCallbackBarrier();
        Task callback = Task.Run(barrier.Run);
        try
        {
            await barrier.Admitted.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsFalse(barrier.Drained.IsCompleted);
        }
        finally
        {
            await barrier.ReleaseAndDrainAsync();
            await callback.WaitAsync(TimeSpan.FromSeconds(10));
        }
    }
}
