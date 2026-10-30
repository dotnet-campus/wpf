using System.Buffers.Binary;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class PartitionSchedulerTests
{
    /// <summary>Runs late registration on the production composition thread after a real command failure.</summary>
    [TestMethod]
    public async Task WhenFailedPartitionRegistersLateChannelThenRegistrationSucceedsAsync()
    {
        using var scheduler = new PartitionScheduler();
        var source = new SameThreadChannel(new SameThreadPartition(scheduler));
        var late = new SameThreadChannel(source.Partition);
        try
        {
            await RegisterAsync(scheduler, source);
            await FailAsync(scheduler, source);

            int result = await RegisterAsync(scheduler, late);

            Assert.AreEqual(0, result);
        }
        finally
        {
            await DestroyAsync(scheduler, source);
            await DestroyAsync(scheduler, late);
        }
    }

    /// <summary>Checks that successful attachment does not erase the business failure observed by a flush barrier.</summary>
    [TestMethod]
    public async Task WhenLateRegistrationSucceedsThenFlushRetainsFailureAsync()
    {
        using var scheduler = new PartitionScheduler();
        var source = new SameThreadChannel(new SameThreadPartition(scheduler));
        var late = new SameThreadChannel(source.Partition);
        try
        {
            await RegisterAsync(scheduler, source);
            int failure = await FailAsync(scheduler, source);
            await RegisterAsync(scheduler, late);

            int result = await FlushAsync(scheduler, late);

            Assert.AreEqual(failure, result);
        }
        finally
        {
            await DestroyAsync(scheduler, source);
            await DestroyAsync(scheduler, late);
        }
    }

    /// <summary>Checks the production unregister completion and the surviving shared channel's failure.</summary>
    [TestMethod]
    public async Task WhenSourceIsDestroyedThenSharedChannelSurvivesWithFailureAsync()
    {
        using var scheduler = new PartitionScheduler();
        var source = new SameThreadChannel(new SameThreadPartition(scheduler));
        var late = new SameThreadChannel(source.Partition);
        try
        {
            await RegisterAsync(scheduler, source);
            int failure = await FailAsync(scheduler, source);
            await RegisterAsync(scheduler, late);

            int destroyed = await DestroyAsync(scheduler, source);
            int flushed = await FlushAsync(scheduler, late);

            Assert.AreEqual((0, failure, 1), (destroyed, flushed, late.Partition.Channels.Count));
        }
        finally
        {
            await DestroyAsync(scheduler, source);
            await DestroyAsync(scheduler, late);
        }
    }

    /// <summary>Checks that another partition on the same scheduler remains healthy after the failed partition closes.</summary>
    [TestMethod]
    public async Task WhenFailedPartitionClosesThenIndependentPartitionRemainsHealthyAsync()
    {
        using var scheduler = new PartitionScheduler();
        var source = new SameThreadChannel(new SameThreadPartition(scheduler));
        var independent = new SameThreadChannel(new SameThreadPartition(scheduler));
        try
        {
            await RegisterAsync(scheduler, source);
            await RegisterAsync(scheduler, independent);
            await FailAsync(scheduler, source);
            await DestroyAsync(scheduler, source);

            int result = await FlushAsync(scheduler, independent);

            Assert.AreEqual(0, result);
        }
        finally
        {
            await DestroyAsync(scheduler, source);
            await DestroyAsync(scheduler, independent);
        }
    }

    private static async Task<int> RegisterAsync(PartitionScheduler scheduler, SameThreadChannel channel)
    {
        var work = new PartitionScheduler.Work(channel, null, register: true);
        scheduler.Submit(work);
        return await work.Completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static async Task<int> FailAsync(PartitionScheduler scheduler, SameThreadChannel channel)
    {
        // An unknown wire command goes through the real router; no partition state is injected.
        byte[] packet = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(packet, uint.MaxValue);
        Assert.AreEqual(0, channel.Send(packet, false));
        Assert.AreEqual(0, channel.CloseBatch());
        int result = await FlushAsync(scheduler, channel);
        Assert.AreEqual(unchecked((int)0x88980406), result);
        return result;
    }

    private static Task<int> FlushAsync(PartitionScheduler scheduler, SameThreadChannel channel) =>
        scheduler.SubmitBatches(channel, barrier: true).Completion.Task.WaitAsync(TimeSpan.FromSeconds(5));

    private static Task<int> DestroyAsync(PartitionScheduler scheduler, SameThreadChannel channel) =>
        scheduler.SubmitBatches(channel, destroy: true).Completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
}
