using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class ChannelMessageEventLifecycleTests
{
    private const int InvalidHandle = unchecked((int)0x80070006);

    /// <summary>Checks repeated destruction through the real channel owner.</summary>
    [TestMethod]
    public void WhenDestroyedTwiceThenWaitRetainsInvalidHandleResult()
    {
        var channel = new SameThreadChannel(new SameThreadPartition());
        channel.Destroy();
        channel.Destroy();

        Assert.AreEqual(InvalidHandle, Wait(channel, 0).Result);
    }

    /// <summary>Checks the scheduler's legal late wake after channel destruction.</summary>
    [TestMethod]
    public void WhenDestroyedChannelIsWokenThenWakeIsNoOp()
    {
        var channel = new SameThreadChannel(new SameThreadPartition());
        channel.Destroy();

        channel.WakeMessageWaiters();
        channel.PostMessage(1);

        Assert.AreEqual(InvalidHandle, Wait(channel, 0).Result);
    }

    /// <summary>Preserves the existing core result without rewriting the output on an early return.</summary>
    [TestMethod]
    public void WhenWaitStartsAfterDestroyThenOutputIsUnchanged()
    {
        var channel = new SameThreadChannel(new SameThreadPartition());
        channel.Destroy();

        Assert.AreEqual((InvalidHandle, uint.MaxValue), Wait(channel, 0));
    }

    /// <summary>Preserves immediate message queue notification independently of the event's signal.</summary>
    [TestMethod]
    public void WhenMessageIsQueuedThenWaitReturnsQueueIndex()
    {
        var channel = new SameThreadChannel(new SameThreadPartition());
        try
        {
            channel.PostMessage(1);

            Assert.AreEqual((0, 0u), Wait(channel, 0));
        }
        finally { channel.Destroy(); }
    }

    /// <summary>Preserves the native timeout result for a healthy empty queue.</summary>
    [TestMethod]
    public void WhenQueueIsEmptyThenZeroTimeoutIsPreserved()
    {
        var channel = new SameThreadChannel(new SameThreadPartition());
        try
        {
            Assert.AreEqual((0, 258u), Wait(channel, 0));
        }
        finally { channel.Destroy(); }
    }

    /// <summary>Checks the actual native wait path with a caller-owned signaled handle.</summary>
    [TestMethod]
    public unsafe void WhenExternalHandleIsSignaledThenWaitReturnsExternalIndex()
    {
        var channel = new SameThreadChannel(new SameThreadPartition());
        using var signal = new EventWaitHandle(true, EventResetMode.ManualReset);
        bool added = false;
        var handle = signal.SafeWaitHandle;
        try
        {
            handle.DangerousAddRef(ref added);
            nint native = handle.DangerousGetHandle();
            uint output = uint.MaxValue;
            int result = channel.WaitForMessage(1, &native, false, 0, &output);

            Assert.AreEqual((0, 0u), (result, output));
        }
        finally
        {
            if (added) handle.DangerousRelease();
            channel.Destroy();
        }
    }

    /// <summary>Exercises bounded concurrent entry and notification without private state injection.</summary>
    [DataTestMethod]
    [DataRow("Post")]
    [DataRow("Wake")]
    [DataRow("Destroy")]
    public async Task WhenWaitRacesNotificationThenExistingResultIsPreservedAsync(string operation)
    {
        var channel = new SameThreadChannel(new SameThreadPartition());
        using var entered = new ManualResetEventSlim();
        Task<(int Result, uint Output)> waiter = Task.Run(() =>
        {
            entered.Set();
            return Wait(channel, 3000);
        });
        try
        {
            Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(5)));
            switch (operation)
            {
                case "Post":
                    channel.PostMessage(1);
                    break;
                case "Wake":
                    // Entry is public behavior, not proof of the private borrow point. Pulse until observed.
                    Assert.IsTrue(SpinWait.SpinUntil(() =>
                    {
                        channel.WakeMessageWaiters();
                        return waiter.IsCompleted;
                    }, TimeSpan.FromSeconds(2)));
                    break;
                case "Destroy":
                    channel.Destroy();
                    break;
            }

            var actual = await waiter.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.AreEqual(operation == "Destroy" ? InvalidHandle : 0, actual.Result);
            if (operation != "Destroy") Assert.AreEqual(0u, actual.Output);
        }
        finally
        {
            channel.Destroy();
            await waiter.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    private static unsafe (int Result, uint Output) Wait(SameThreadChannel channel, uint timeout)
    {
        uint output = uint.MaxValue;
        int result = channel.WaitForMessage(0, null, false, timeout, &output);
        return (result, output);
    }
}
