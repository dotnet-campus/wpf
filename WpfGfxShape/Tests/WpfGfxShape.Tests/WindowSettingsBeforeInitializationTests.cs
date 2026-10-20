using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class WindowSettingsBeforeInitializationTests
{
    [TestMethod]
    public void WhenVBlankEnablePrecedesHwndThenStatusAcknowledgesAndDuplicateEnableIsIdempotent()
    {
        var partition = new SameThreadPartition();
        var channel = new SameThreadChannel(partition);
        try
        {
            channel.Send(Convert.FromHexString("0500000001000000"), true);
            int first = channel.Commit();
            channel.Send(Convert.FromHexString("0500000001000000"), true);
            int second = channel.Commit();
            byte[] status = new byte[64];
            bool received = channel.PeekMessage(status);
            Assert.AreEqual((0, 0, 1, true, 9u, 0),
                (first, second, partition.VBlankChannelCount, received, BitConverter.ToUInt32(status), BitConverter.ToInt32(status, 4)));
        }
        finally { channel.Destroy(); }
    }

    [TestMethod]
    public void WhenOneOfTwoVBlankSubscribersClosesThenOtherSubscriptionSurvives()
    {
        var partition = new SameThreadPartition();
        var first = new SameThreadChannel(partition);
        var second = new SameThreadChannel(partition);
        try
        {
            partition.SetVBlankSyncMode(first, true);
            partition.SetVBlankSyncMode(second, true);
            first.Destroy();
            int remaining = partition.VBlankChannelCount;
            second.Destroy();
            Assert.AreEqual((1, 0), (remaining, partition.VBlankChannelCount));
        }
        finally { first.Destroy(); second.Destroy(); }
    }

    [TestMethod]
    public void WhenVBlankCommandIsTruncatedThenNoSubscriptionIsCreated()
    {
        var partition = new SameThreadPartition();
        var channel = new SameThreadChannel(partition);
        try
        {
            channel.Send(Convert.FromHexString("05000000"), true);
            Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 0), (channel.Commit(), partition.VBlankChannelCount));
        }
        finally { channel.Destroy(); }
    }

    [TestMethod]
    public void WhenOutOfBandDisablePrecedesHwndInitializeThenWindowSettingsAreAccepted()
    {
        var target = new GeneratedTargetResource(MilResourceType.HwndRenderTarget);
        var table = new GeneratedProtocolHandleTable();
        byte[] packet = Convert.FromHexString("3300000002000000A4000000BB000000340600008503000000000000000000000000803F0000000000000000000000000000803F0000803F0000803F000000000100000000000000");
        try
        {
            int result = target.ProcessCommand(table, MilCommand.TargetUpdateWindowSettings, packet);
            Assert.AreEqual((0, 1424u, 714u), (result, target.Width, target.Height));
        }
        finally
        {
            target.Release();
        }
    }
}
