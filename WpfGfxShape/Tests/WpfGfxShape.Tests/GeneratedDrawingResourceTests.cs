using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedDrawingResourceTests
{
    [TestMethod]
    public void WhenDrawingCommandLayoutsAreMeasuredThenNativeSizesAndOffsetsMatch()
    {
        Assert.AreEqual((24, 52, 20, 16, 48, 48, 52, 20, 40),
            (Marshal.SizeOf<MilDashStyleCommand>(), Marshal.SizeOf<MilPenCommand>(), Marshal.SizeOf<MilGeometryDrawingCommand>(), Marshal.SizeOf<MilGlyphRunDrawingCommand>(), Marshal.SizeOf<MilImageDrawingCommand>(), Marshal.SizeOf<MilVideoDrawingCommand>(), Marshal.SizeOf<MilDrawingGroupCommand>(), Marshal.OffsetOf<MilDashStyleCommand>(nameof(MilDashStyleCommand.DashesSize)).ToInt32(), Marshal.OffsetOf<MilDrawingGroupCommand>(nameof(MilDrawingGroupCommand.EdgeMode)).ToInt32()));
    }

    [TestMethod]
    public void WhenDrawingPacketsAreWrittenThenIdsFieldsAndPayloadsMatchNativeLayout()
    {
        byte[] dash = GeneratedProtocolPacketWriter.WriteDashStyle(1, 2.5, [3d, 4d], 5);
        byte[] pen = GeneratedProtocolPacketWriter.WritePen(2, 3, 4, 6, 7, MilPenCap.Round, MilPenCap.Triangle, MilPenCap.Square, MilPenJoin.Bevel, 8);
        byte[] group = GeneratedProtocolPacketWriter.WriteDrawingGroup(9, 0.5, [10u, 11u], edge: MilEdgeMode.Aliased, scaling: MilBitmapScalingMode.NearestNeighbor, clearType: MilClearTypeHint.Enabled);
        Assert.AreEqual((0x85u, 1u, 2.5, 5u, 16u, 3d, 4d, 0x86u, 2u, 6u, 2u, 1u, 8u, 0x8Bu, 8u, 10u, 11u),
            (BitConverter.ToUInt32(dash, 0), BitConverter.ToUInt32(dash, 4), BitConverter.ToDouble(dash, 8), BitConverter.ToUInt32(dash, 16), BitConverter.ToUInt32(dash, 20), BitConverter.ToDouble(dash, 24), BitConverter.ToDouble(dash, 32),
                BitConverter.ToUInt32(pen, 0), BitConverter.ToUInt32(pen, 4), BitConverter.ToUInt32(pen, 24), BitConverter.ToUInt32(pen, 32), BitConverter.ToUInt32(pen, 44), BitConverter.ToUInt32(pen, 48),
                BitConverter.ToUInt32(group, 0), BitConverter.ToUInt32(group, 16), BitConverter.ToUInt32(group, 52), BitConverter.ToUInt32(group, 56)));
    }

    [TestMethod]
    public void WhenFactoryCreatesDrawingResourcesThenEachResourceHasItsStrongType()
    {
        GeneratedProtocolHandleTable table = new(); MilResourceType[] types = [MilResourceType.DashStyle, MilResourceType.Pen, MilResourceType.GeometryDrawing, MilResourceType.GlyphRunDrawing, MilResourceType.ImageDrawing, MilResourceType.VideoDrawing, MilResourceType.DrawingGroup];
        foreach ((MilResourceType type, int index) in types.Select((type, index) => (type, index))) Assert.AreEqual(0, table.Create((uint)index + 1, type));
        Assert.AreEqual((true, true, true, true, true, true, true), (Get<GeneratedDashStyleResource>(table, 1) is not null, Get<GeneratedPenResource>(table, 2) is not null, Get<GeneratedGeometryDrawingResource>(table, 3) is not null, Get<GeneratedGlyphRunDrawingResource>(table, 4) is not null, Get<GeneratedImageDrawingResource>(table, 5) is not null, Get<GeneratedVideoDrawingResource>(table, 6) is not null, Get<GeneratedDrawingGroupResource>(table, 7) is not null));
    }

    [TestMethod]
    public void WhenDashPenAndLeafDrawingsAreUpdatedThenValuesAndDependenciesMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); MilRectD rect = new(1, 2, 3, 4);
        int result = router.ProcessPackets([
            Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.RectResource), Create(3, MilResourceType.SolidColorBrush), Create(4, MilResourceType.LineGeometry), Create(5, MilResourceType.GlyphRun), Create(6, MilResourceType.DrawingImage), Create(7, MilResourceType.MediaPlayer),
            Create(10, MilResourceType.DashStyle), Create(11, MilResourceType.Pen), Create(12, MilResourceType.GeometryDrawing), Create(13, MilResourceType.GlyphRunDrawing), Create(14, MilResourceType.ImageDrawing), Create(15, MilResourceType.VideoDrawing),
            GeneratedProtocolPacketWriter.WriteDashStyle(10, 0.5, [1d, 2d], 1), GeneratedProtocolPacketWriter.WritePen(11, 3, 4, 3, 1, MilPenCap.Round, dashStyle: 10),
            GeneratedProtocolPacketWriter.WriteGeometryDrawing(12, 3, 11, 4), GeneratedProtocolPacketWriter.WriteGlyphRunDrawing(13, 5, 3), GeneratedProtocolPacketWriter.WriteImageDrawing(14, rect, 6, 2), GeneratedProtocolPacketWriter.WriteVideoDrawing(15, rect, 7, 2)
        ]);
        Assert.AreEqual((0, 0.5, 2, MilPenCap.Round, MilPenJoin.Miter, rect), (result, Get<GeneratedDashStyleResource>(table, 10).Offset, Get<GeneratedDashStyleResource>(table, 10).Dashes.Length, Get<GeneratedPenResource>(table, 11).StartLineCap, Get<GeneratedPenResource>(table, 11).LineJoin, Get<GeneratedImageDrawingResource>(table, 14).Rect));
    }

    [TestMethod]
    public void WhenDrawingGroupIsUpdatedThenStateAndChildrenMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        int result = router.ProcessPackets([Create(1, MilResourceType.LineGeometry), Create(2, MilResourceType.DoubleResource), Create(3, MilResourceType.SolidColorBrush), Create(4, MilResourceType.TranslateTransform), Create(5, MilResourceType.GuidelineSet), Create(10, MilResourceType.GeometryDrawing), Create(11, MilResourceType.ImageDrawing), Create(12, MilResourceType.DrawingGroup), GeneratedProtocolPacketWriter.WriteDrawingGroup(12, 0.5, [10u, 11u], 1, 2, 3, 4, 5, MilEdgeMode.Aliased, MilBitmapScalingMode.HighQuality, MilClearTypeHint.Enabled)]);
        Assert.AreEqual((0, 2, MilEdgeMode.Aliased), (result, Get<GeneratedDrawingGroupResource>(table, 12).Children.Count, Get<GeneratedDrawingGroupResource>(table, 12).EdgeMode));
    }

    [TestMethod]
    public void WhenDashPayloadLengthOrAlignmentIsInvalidThenOldDataRemainsUnchanged()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.DashStyle), GeneratedProtocolPacketWriter.WriteDashStyle(1, 1, [2d])]); GeneratedDashStyleResource dash = Get<GeneratedDashStyleResource>(table, 1); int count = dash.ChangeCount;
        byte[] mismatch = GeneratedProtocolPacketWriter.WriteDashStyle(1, 3, [4d]); BitConverter.GetBytes(16u).CopyTo(mismatch, 20);
        byte[] unaligned = [.. GeneratedProtocolPacketWriter.WriteDashStyle(1, 3, [4d]), 0]; BitConverter.GetBytes(9u).CopyTo(unaligned, 20);
        int a = router.ProcessPacket(mismatch); int b = router.ProcessPacket(unaligned);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 1d, 1, count), (a, b, dash.Offset, dash.Dashes.Length, dash.ChangeCount));
    }

    [TestMethod]
    public void WhenDrawingGroupChildOrDependencyTypeIsInvalidThenReplacementIsTransactional()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.GeometryDrawing), Create(2, MilResourceType.ColorResource), Create(3, MilResourceType.DrawingGroup), GeneratedProtocolPacketWriter.WriteDrawingGroup(3, 1, [1u])]); GeneratedDrawingGroupResource group = Get<GeneratedDrawingGroupResource>(table, 3); int count = group.ChangeCount;
        int nullChild = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDrawingGroup(3, 0.5, [0u])); int wrongChild = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDrawingGroup(3, 0.5, [2u]));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 1, count), (nullChild, wrongChild, group.Children.Count, group.ChangeCount));
    }

    [TestMethod]
    public void WhenPenOrGroupEnumIsInvalidThenUpdateIsRejected()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Pen), Create(2, MilResourceType.DrawingGroup)]);
        byte[] pen = GeneratedProtocolPacketWriter.WritePen(1, 1, 1); BitConverter.GetBytes(99u).CopyTo(pen, 32);
        byte[] group = GeneratedProtocolPacketWriter.WriteDrawingGroup(2, 1, []); BitConverter.GetBytes(99u).CopyTo(group, 40);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult), (router.ProcessPacket(pen), router.ProcessPacket(group)));
    }

    [TestMethod]
    public void WhenLeafDependencyChangesThenNotificationPropagatesThroughDrawingGroup()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.DashStyle), Create(3, MilResourceType.Pen), Create(4, MilResourceType.GeometryDrawing), Create(5, MilResourceType.DrawingGroup), GeneratedProtocolPacketWriter.WriteDashStyle(2, 0, [1d], 1), GeneratedProtocolPacketWriter.WritePen(3, 1, 1, dashStyle: 2), GeneratedProtocolPacketWriter.WriteGeometryDrawing(4, pen: 3), GeneratedProtocolPacketWriter.WriteDrawingGroup(5, 1, [4u])]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(1, 2));
        Assert.AreEqual((0, 2, 2, 2, 2), (result, Get<GeneratedDashStyleResource>(table, 2).ChangeCount, Get<GeneratedPenResource>(table, 3).ChangeCount, Get<GeneratedGeometryDrawingResource>(table, 4).ChangeCount, Get<GeneratedDrawingGroupResource>(table, 5).ChangeCount));
    }

    [TestMethod]
    public void WhenDrawingIsDuplicatedAndLastHandleDeletedThenDependenciesReleaseDeterministically()
    {
        GeneratedProtocolHandleTable source = new(); GeneratedProtocolHandleTable target = new(); GeneratedProtocolChannelRegistry channels = new(); _ = channels.TryAdd(1, source); _ = channels.TryAdd(2, target); GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, channels).CreateRouter();
        _ = router.ProcessPackets([Create(1, MilResourceType.SolidColorBrush), Create(2, MilResourceType.GeometryDrawing), GeneratedProtocolPacketWriter.WriteGeometryDrawing(2, brush: 1), GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(2, 2, 20)]); GeneratedProtocolResource dependency = Get<GeneratedProtocolResource>(source, 1); GeneratedProtocolResource drawing = Get<GeneratedProtocolResource>(source, 2);
        int a = source.Delete(2, MilResourceType.GeometryDrawing); int b = source.Delete(1, MilResourceType.SolidColorBrush); int c = target.Delete(20, MilResourceType.GeometryDrawing);
        Assert.AreEqual((0, 0, 0, 0, 0, true), (a, b, c, dependency.ReferenceCount, drawing.ReferenceCount, drawing.IsReleased));
    }

    [TestMethod]
    public void WhenDrawingPacketSequenceFailsThenLaterUpdatesAreNotProcessed()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); int result = router.ProcessPackets([Create(1, MilResourceType.ImageDrawing), GeneratedProtocolPacketWriter.WriteImageDrawing(1, new MilRectD(1, 2, 3, 4), imageSource: 99), GeneratedProtocolPacketWriter.WriteImageDrawing(1, new MilRectD(5, 6, 7, 8))]);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, default(MilRectD), 0), (result, Get<GeneratedImageDrawingResource>(table, 1).Rect, Get<GeneratedImageDrawingResource>(table, 1).ChangeCount));
    }

    private static byte[] Create(uint handle, MilResourceType type) => GeneratedProtocolPacketWriter.WriteChannelCreateResource(handle, type);
    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter) Context() { GeneratedProtocolHandleTable table = new(); GeneratedProtocolChannelRegistry registry = new(); _ = registry.TryAdd(1, table); return (table, new GeneratedProtocolProductionContext(1, registry).CreateRouter()); }
    private static T Get<T>(GeneratedProtocolHandleTable table, uint handle) where T : GeneratedProtocolResource { Assert.IsTrue(table.TryGetResource(handle, out GeneratedProtocolResource? resource)); return (T)resource!; }
}
