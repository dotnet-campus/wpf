using System.Buffers.Binary;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class BitmapCachePacketContractTests
{
    [TestMethod]
    public void WhenWriterUsesPackedContractThenAllBytesMatchIndependentDefinition()
    {
        Assert.AreEqual(Convert.ToHexString(Packet(1, 0, 0, 0)), Convert.ToHexString(GeneratedProtocolPacketWriter.WriteBitmapCache(1, 1)));
    }

    [DataTestMethod]
    [DataRow(0, 0)]
    [DataRow(0, 1)]
    [DataRow(1, 0)]
    [DataRow(1, 1)]
    public void WhenPackedFlagsAreValidThenCacheStateMatches(int snaps, int clearType)
    {
        var (table, router) = Context();
        int result = router.ProcessPacket(Packet(1, 0, snaps, clearType));
        var cache = Get(table);
        Assert.AreEqual((0, 1d, snaps != 0, clearType != 0), (result, cache.RenderAtScale, cache.SnapsToDevicePixels, cache.EnableClearType));
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(8)]
    [DataRow(24)]
    [DataRow(27)]
    [DataRow(29)]
    [DataRow(32)]
    [DataRow(36)]
    public void WhenPacketLengthIsNotExactly28ThenUpdateIsRejected(int length)
    {
        var (_, router) = Context();
        byte[] packet = Packet(1, 0, 0, 0);
        Array.Resize(ref packet, length);
        Assert.AreEqual(Direct3D9Factory.UceMalformedPacketHResult, router.ProcessPacket(packet));
    }

    [DataTestMethod]
    [DataRow(2, 0)]
    [DataRow(-1, 0)]
    [DataRow(0, 2)]
    [DataRow(0, -1)]
    public void WhenFlagsAreInvalidThenExistingStateIsPreserved(int snaps, int clearType)
    {
        var (table, router) = Context();
        _ = router.ProcessPacket(Packet(1, 0, 1, 1));
        var cache = Get(table);
        int count = cache.ChangeCount;
        int result = router.ProcessPacket(Packet(1, 0, snaps, clearType));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, true, true, count), (result, cache.SnapsToDevicePixels, cache.EnableClearType, cache.ChangeCount));
    }

    [DataTestMethod]
    [DataRow(2u)]
    [DataRow(99u)]
    public void WhenAnimationHandleIsWrongTypeOrMissingThenUpdateIsRejected(uint animation)
    {
        var (_, router) = Context();
        Assert.AreEqual(Direct3D9Factory.UceMalformedPacketHResult, router.ProcessPacket(Packet(1, animation, 0, 0)));
    }

    [TestMethod]
    public void WhenAnimationHandleIsDoubleResourceThenAnimatedScaleIsUsed()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([GeneratedProtocolPacketWriter.WriteChannelCreateResource(3, MilResourceType.DoubleResource), GeneratedProtocolPacketWriter.WriteDoubleResource(3, 2.5)]);
        int result = router.ProcessPacket(Packet(1, 3, 0, 0));
        Assert.AreEqual((0, 2.5), (result, Get(table).CurrentRenderAtScale));
    }

    private static byte[] Packet(uint handle, uint animation, int snaps, int clearType)
    {
        // Independent Pack=1 wire definition: no trailing double-alignment padding.
        byte[] packet = new byte[28];
        BinaryPrimitives.WriteUInt32LittleEndian(packet, 0x8D);
        BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(4), handle);
        BinaryPrimitives.WriteDoubleLittleEndian(packet.AsSpan(8), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(16), animation);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(20), snaps);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(24), clearType);
        return packet;
    }

    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter) Context()
    {
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry registry = new();
        _ = registry.TryAdd(1, table);
        var router = new GeneratedProtocolProductionContext(1, registry).CreateRouter();
        _ = router.ProcessPackets([GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.BitmapCache), GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.SolidColorBrush)]);
        return (table, router);
    }

    private static GeneratedBitmapCacheResource Get(GeneratedProtocolHandleTable table)
    {
        Assert.IsTrue(table.TryGetResource(1, out GeneratedProtocolResource? resource));
        return (GeneratedBitmapCacheResource)resource!;
    }
}
