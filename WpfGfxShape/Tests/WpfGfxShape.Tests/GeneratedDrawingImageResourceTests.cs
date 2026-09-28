using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedDrawingImageResourceTests
{
    [TestMethod]
    public void WhenPacketIsWrittenThenIndependentGoldenBytesMatch()
    {
        CollectionAssert.AreEqual(new byte[] { 0x71, 0, 0, 0, 4, 3, 2, 1, 8, 7, 6, 5 }, GeneratedProtocolPacketWriter.WriteDrawingImage(0x01020304, 0x05060708));
    }

    [TestMethod]
    public void WhenLayoutIsMeasuredThenNativeSizeAndOffsetMatch()
    {
        Assert.AreEqual((12, 8), (Marshal.SizeOf<MilDrawingImageCommand>(), Marshal.OffsetOf<MilDrawingImageCommand>(nameof(MilDrawingImageCommand.Drawing)).ToInt32()));
    }

    [TestMethod]
    public void WhenDrawingChangesThenAllImageConsumersAreNotified()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.GeometryDrawing), Create(2, MilResourceType.DrawingImage), Create(3, MilResourceType.ImageBrush), Create(4, MilResourceType.ImageDrawing), Create(5, MilResourceType.RenderData),
            GeneratedProtocolPacketWriter.WriteDrawingImage(2, 1),
            GeneratedProtocolPacketWriter.WriteTileBrush(MilCommand.ImageBrush, 3, 1, default, default, 0, 1, source: 2),
            GeneratedProtocolPacketWriter.WriteImageDrawing(4, default, 2),
            GeneratedProtocolPacketWriter.WriteRenderData(5, GeneratedProtocolPacketWriter.WriteDrawImageRecord(default, 2), GeneratedProtocolPacketWriter.WriteDrawImageRecord(default, 2))]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteGeometryDrawing(1));
        Assert.AreEqual((0, 2, 2, 2, 3, 5), (result, Get(table, 2).ChangeCount, Get(table, 3).ChangeCount, Get(table, 4).ChangeCount, Get(table, 5).ChangeCount, Get(table, 2).ReferenceCount));
    }

    [TestMethod]
    public void WhenDependencyIsReplacedThenOldDrawingNoLongerNotifies()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.GeometryDrawing), Create(2, MilResourceType.DrawingImage), Create(3, MilResourceType.DrawingGroup), GeneratedProtocolPacketWriter.WriteDrawingImage(2, 1), GeneratedProtocolPacketWriter.WriteDrawingImage(2, 3), GeneratedProtocolPacketWriter.WriteGeometryDrawing(1)]);
        Assert.AreEqual((1, 2, 2, true), (Get(table, 1).ReferenceCount, Get(table, 3).ReferenceCount, Get(table, 2).ChangeCount, ReferenceEquals(((GeneratedDrawingImageResource)Get(table, 2)).Drawing, Get(table, 3))));
    }

    [TestMethod]
    public void WhenNullReplacesDrawingThenDependencyIsReleased()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.GeometryDrawing), Create(2, MilResourceType.DrawingImage), GeneratedProtocolPacketWriter.WriteDrawingImage(2, 1), GeneratedProtocolPacketWriter.WriteDrawingImage(2)]);
        Assert.AreEqual((1, true), (Get(table, 1).ReferenceCount, ((GeneratedDrawingImageResource)Get(table, 2)).Drawing is null));
    }

    [TestMethod]
    [DataRow(99u)]
    [DataRow(3u)]
    public void WhenDependencyIsInvalidThenBatchStopsAndOldIdentitySurvives(uint invalid)
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.GeometryDrawing), Create(2, MilResourceType.DrawingImage), Create(3, MilResourceType.SolidColorBrush), GeneratedProtocolPacketWriter.WriteDrawingImage(2, 1)]);
        int result = router.ProcessPackets([GeneratedProtocolPacketWriter.WriteDrawingImage(2, invalid), GeneratedProtocolPacketWriter.WriteDrawingImage(2)]);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 2, 1, true), (result, Get(table, 1).ReferenceCount, Get(table, 2).ChangeCount, ReferenceEquals(Get(table, 1), ((GeneratedDrawingImageResource)Get(table, 2)).Drawing)));
    }

    [TestMethod]
    [DataRow(11)]
    [DataRow(13)]
    public void WhenPacketSizeIsInvalidThenUpdateIsRejected(int size)
    {
        var (table, router) = Context();
        _ = router.ProcessPacket(Create(2, MilResourceType.DrawingImage));
        byte[] packet = new byte[size];
        GeneratedProtocolPacketWriter.WriteDrawingImage(2).AsSpan(0, Math.Min(size, 12)).CopyTo(packet);
        Assert.AreEqual(Direct3D9Factory.UceMalformedPacketHResult, router.ProcessPacket(packet));
    }

    [TestMethod]
    public void WhenDuplicateOutlivesOriginalHandlesThenIdentityAndFinalReleaseArePreserved()
    {
        GeneratedProtocolHandleTable source = new(); GeneratedProtocolHandleTable target = new(); GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, source); _ = channels.TryAdd(2, target);
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, channels).CreateRouter();
        _ = router.ProcessPackets([Create(1, MilResourceType.GeometryDrawing), Create(2, MilResourceType.DrawingImage), GeneratedProtocolPacketWriter.WriteDrawingImage(2, 1), GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(2, 2, 20)]);
        GeneratedProtocolResource drawing = Get(source, 1); GeneratedDrawingImageResource image = (GeneratedDrawingImageResource)Get(source, 2);
        _ = source.Delete(1, MilResourceType.GeometryDrawing); _ = source.Delete(2, MilResourceType.DrawingImage);
        bool retained = ReferenceEquals(image.Drawing, drawing) && ReferenceEquals(Get(target, 20), image) && drawing.ReferenceCount == 1;
        _ = target.Delete(20, MilResourceType.DrawingImage);
        Assert.AreEqual((true, true, true, true), (retained, image.IsReleased, drawing.IsReleased, image.Drawing is null));
    }

    [TestMethod]
    public void WhenRepeatedRenderDataReferencesAreRemovedThenWholeChainIsReleased()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.GeometryDrawing), Create(2, MilResourceType.DrawingImage), Create(3, MilResourceType.RenderData), GeneratedProtocolPacketWriter.WriteDrawingImage(2, 1), GeneratedProtocolPacketWriter.WriteRenderData(3, GeneratedProtocolPacketWriter.WriteDrawImageRecord(default, 2), GeneratedProtocolPacketWriter.WriteDrawImageRecord(default, 2))]);
        GeneratedProtocolResource drawing = Get(table, 1); GeneratedProtocolResource image = Get(table, 2);
        _ = table.Delete(1, MilResourceType.GeometryDrawing); _ = table.Delete(2, MilResourceType.DrawingImage);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteRenderData(3));
        Assert.AreEqual((0, true, true), (result, image.IsReleased, drawing.IsReleased));
    }

    private static byte[] Create(uint handle, MilResourceType type) => GeneratedProtocolPacketWriter.WriteChannelCreateResource(handle, type);
    private static GeneratedProtocolResource Get(GeneratedProtocolHandleTable table, uint handle) { Assert.IsTrue(table.TryGetResource(handle, out GeneratedProtocolResource? resource)); return resource!; }
    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter) Context() { GeneratedProtocolHandleTable table = new(); GeneratedProtocolChannelRegistry channels = new(); _ = channels.TryAdd(1, table); return (table, new GeneratedProtocolProductionContext(1, channels).CreateRouter()); }
}
