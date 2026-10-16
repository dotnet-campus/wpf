namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    public static IEnumerable<object[]> NativeFactoryTypes => new uint[]
    {
        1, 3, 4, 7, 8, 9, 11, 13, 14, 16, 17, 18, 20, 22, 23, 24, 25,
        27, 29, 30, 31, 32, 33, 34, 36, 37, 38, 39, 40, 41, 42, 43,
        46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 59, 61, 62,
        63, 64, 65, 66, 68, 69, 70, 71, 72, 73, 75, 77, 78, 80, 81,
        82, 83, 84, 85, 87, 88, 89, 90, 91, 92, 94, 95, 96, 97
    }.Select(type => new object[] { type });

    [DataTestMethod]
    [DynamicData(nameof(NativeFactoryTypes))]
    public void WhenNativeFactorySupportsTypeThenProductionCreatesAndReleasesIt(uint type)
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
            Assert.AreEqual(0, api.Commit(channel));
            int deleted = -1;
            Assert.AreEqual(0, api.ReleaseResource(channel, resource, &deleted));
            Assert.AreEqual(1, deleted);
            Assert.AreEqual(0, api.Close(channel));
            // The native factory supports GlyphRun, but ordinary channel creation ignores it.
            Assert.AreEqual(type == 42 ? unchecked((int)0x88980403) : 0, api.Commit(channel));
        }
        finally
        {
            if (channel != 0) api.DestroyChannel(channel);
            if (connection != 0) api.Disconnect(connection);
        }
    }
}
