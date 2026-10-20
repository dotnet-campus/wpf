using System.Buffers.Binary;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class WindowPresentNotificationTests
{
    [TestMethod]
    public void WhenPresentIsRequestedWithoutTargetsThenOneNoPresentMessageIsPosted()
    {
        var partition = new SameThreadPartition();
        var channel = new SameThreadChannel(partition);
        try
        {
            byte[] packet = new byte[12];
            BinaryPrimitives.WriteUInt32LittleEndian(packet, 6);
            BinaryPrimitives.WriteUInt64LittleEndian(packet.AsSpan(4), 0x123456789ABCDEF0);
            channel.Send(packet, false);
            channel.CloseBatch();
            Assert.AreEqual(0, channel.Commit());
            Assert.AreEqual(0, partition.Compose());
            byte[] message = new byte[64];
            Assert.IsTrue(channel.PeekMessage(message));
            byte[] expected = new byte[64];
            BinaryPrimitives.WriteUInt32LittleEndian(expected, 10);
            // Official exports.cs Message: Reserved@4, Presented.PresentationResults@8.
            BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(8), 1);
            CollectionAssert.AreEqual(expected, message);
            Assert.AreEqual(0, partition.Compose());
            Assert.IsFalse(channel.PeekMessage(message));
        }
        finally { channel.Destroy(); }
    }

    [TestMethod]
    public void WhenDuplicatePresentRequestsAreCommittedThenEachIsNotifiedOnlyAfterCompose()
    {
        var partition = new SameThreadPartition();
        var channel = new SameThreadChannel(partition);
        try
        {
            byte[] packet = new byte[12];
            BinaryPrimitives.WriteUInt32LittleEndian(packet, 6);
            BinaryPrimitives.WriteUInt64LittleEndian(packet.AsSpan(4), 123);
            channel.Send(packet, false);
            channel.Send(packet, false);
            channel.CloseBatch();
            Assert.AreEqual(0, channel.Commit());
            byte[] message = new byte[64];
            Assert.IsFalse(channel.PeekMessage(message));
            Assert.AreEqual(0, partition.Compose());
            byte[] expected = new byte[64];
            BinaryPrimitives.WriteUInt32LittleEndian(expected, 10);
            // Official exports.cs Message: Reserved@4, Presented.PresentationResults@8.
            BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(8), 1);
            var messages = new List<string>();
            for (int i = 0; i < 2; i++)
            {
                Assert.IsTrue(channel.PeekMessage(message));
                CollectionAssert.AreEqual(expected, message);
                messages.Add(Convert.ToHexString(message));
            }
            Assert.IsFalse(channel.PeekMessage(message));
            Assert.AreEqual(0, partition.Compose());
            Assert.IsFalse(channel.PeekMessage(message));
            string directory = Path.Combine(AppContext.BaseDirectory, "Results");
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "present-notifications.json"),
                System.Text.Json.JsonSerializer.Serialize(new { packet = Convert.ToHexString(packet), messages }));
        }
        finally { channel.Destroy(); }
    }

    [TestMethod]
    public void WhenPresentPacketIsOversizedThenPartitionPreservesMalformedPacket()
    {
        var partition = new SameThreadPartition();
        var channel = new SameThreadChannel(partition);
        try
        {
            byte[] packet = new byte[16];
            BinaryPrimitives.WriteUInt32LittleEndian(packet, 6);
            channel.Send(packet, false);
            channel.CloseBatch();
            Assert.AreEqual(Direct3D9Factory.UceMalformedPacketHResult, channel.Commit());
        }
        finally { channel.Destroy(); }
    }

    [TestMethod]
    public void WhenPresentPacketIsTruncatedThenPartitionPreservesMalformedPacket()
    {
        var partition = new SameThreadPartition();
        var channel = new SameThreadChannel(partition);
        try
        {
            channel.Send([6, 0, 0, 0], false);
            channel.CloseBatch();
            Assert.AreEqual(Direct3D9Factory.UceMalformedPacketHResult, channel.Commit());
        }
        finally { channel.Destroy(); }
    }

    [TestMethod]
    public void WhenVBlankSyncIsEnabledThenStatusUsesOfficialMessageLayout()
    {
        var partition = new SameThreadPartition();
        var channel = new SameThreadChannel(partition);
        try
        {
            int result = partition.SetVBlankSyncMode(channel, true);
            byte[] message = new byte[64];
            Assert.IsTrue(channel.PeekMessage(message));
            byte[] expected = new byte[64];
            BinaryPrimitives.WriteUInt32LittleEndian(expected, 9);
            // Official exports.cs Message: Reserved@4, SyncModeStatus@8.
            BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(8), result);
            CollectionAssert.AreEqual(expected, message);
        }
        finally
        {
            partition.SetVBlankSyncMode(channel, false);
            channel.Destroy();
        }
    }

    [TestMethod]
    public void WhenSubscribedChannelIsDestroyedThenNoNotificationRemains()
    {
        var partition = new SameThreadPartition();
        var channel = new SameThreadChannel(partition);
        partition.RequestPresentNotification(channel, 123);
        channel.Destroy();
        Assert.AreEqual(0, partition.Compose());
        Assert.IsFalse(channel.PeekMessage(new byte[64]));
    }
}
