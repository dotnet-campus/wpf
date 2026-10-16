namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    [DataTestMethod]
    [DataRow(0u)]
    [DataRow(1u)]
    public void WhenOrdinaryGlyphCreateIsIgnoredThenItDoesNotReserveConsumerHandle(uint handle)
    {
        var api = new Api();
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            uint* ignored = stackalloc uint[] { 7, handle, 42 };
            Assert.AreEqual(0, api.Send(ignored, 12, 0, channel));
            uint* created = stackalloc uint[] { 7, 1, 49 };
            Assert.AreEqual(0, api.Send(created, 12, 0, channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(0, api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
        }
    }

    [TestMethod]
    public void WhenOrdinaryGlyphCreateIsFollowedByDeleteThenMissingConsumerHandleIsRejected()
    {
        var api = new Api();
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            uint glyph = 0;
            Assert.AreEqual(0, api.CreateResource(channel, 42, &glyph));
            int deleted = 0;
            Assert.AreEqual(0, api.ReleaseResource(channel, glyph, &deleted));
            Assert.AreEqual(1, deleted);
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
        }
    }

    [DataTestMethod]
    [DataRow(2u)]
    [DataRow(5u)]
    [DataRow(6u)]
    [DataRow(10u)]
    [DataRow(12u)]
    [DataRow(15u)]
    [DataRow(19u)]
    [DataRow(21u)]
    [DataRow(26u)]
    [DataRow(28u)]
    [DataRow(35u)]
    [DataRow(44u)]
    [DataRow(45u)]
    [DataRow(58u)]
    [DataRow(60u)]
    [DataRow(67u)]
    [DataRow(74u)]
    [DataRow(76u)]
    [DataRow(79u)]
    [DataRow(86u)]
    [DataRow(93u)]
    public void WhenNativeFactoryCannotCreateResourceTypeThenCommitRejectsIt(uint type)
    {
        var api = new Api();
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            uint resource = 0;
            Assert.AreEqual(0, api.CreateResource(channel, type, &resource));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
        }
    }

    [DataTestMethod]
    [DataRow(4)]
    [DataRow(8)]
    [DataRow(13)]
    [DataRow(16)]
    public void WhenCreatePacketSizeIsNotTwelveThenFirstFailureRemainsSticky(int size)
    {
        var api = new Api();
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            uint* packet = stackalloc uint[] { 7, 1, 49, 0xA5A5A5A5 };
            Assert.AreEqual(0, api.Send(packet, (uint)size, 0, channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
        }
    }

    [DataTestMethod]
    [DataRow(7u, 49u)]
    [DataRow(8u, 50u)]
    [DataRow(8u, 49u)]
    public void WhenCreateCollidesOrDeleteTypeOrHandleIsInvalidThenConsumerRejects(uint command, uint type)
    {
        var api = new Api();
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, api.Create(1, &connection));
            Assert.AreEqual(0, api.CreateChannel(connection, 0, &channel));
            uint resource = 0;
            Assert.AreEqual(0, api.CreateResource(channel, 49, &resource));
            uint* packet = stackalloc uint[] { command, command == 8 && type == 49 ? 999u : resource, type };
            Assert.AreEqual(0, api.Send(packet, 12, 0, channel));
            Assert.AreEqual(0, api.Close(channel));
            Assert.AreEqual(unchecked((int)0x88980403), api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
        }
    }
}
