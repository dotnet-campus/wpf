using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedGeometryResourceTests
{
    [TestMethod]
    public void WhenGeometryCommandLayoutsAreMeasuredThenNativeSizesAndOffsetsMatch()
    {
        Assert.AreEqual(
            (52, 72, 56, 20, 24, 20, 40, 56, 16),
            (
                Marshal.SizeOf<MilLineGeometryCommand>(),
                Marshal.SizeOf<MilRectangleGeometryCommand>(),
                Marshal.SizeOf<MilEllipseGeometryCommand>(),
                Marshal.SizeOf<MilGeometryGroupCommand>(),
                Marshal.SizeOf<MilCombinedGeometryCommand>(),
                Marshal.SizeOf<MilPathGeometryCommand>(),
                Marshal.OffsetOf<MilLineGeometryCommand>(nameof(MilLineGeometryCommand.Transform)).ToInt32(),
                Marshal.OffsetOf<MilRectangleGeometryCommand>(nameof(MilRectangleGeometryCommand.Transform)).ToInt32(),
                Marshal.OffsetOf<MilPathGeometryCommand>(nameof(MilPathGeometryCommand.FiguresSize)).ToInt32()));
    }

    [TestMethod]
    public void WhenGeometryPacketsAreWrittenThenIdsFieldsAndPayloadsMatchNativeLayout()
    {
        byte[] line = GeneratedProtocolPacketWriter.WriteLineGeometry(1, new MilPoint2D(2, 3), new MilPoint2D(4, 5), 6, 7, 8);
        byte[] group = GeneratedProtocolPacketWriter.WriteGeometryGroup(9, MilFillMode.Winding, 6, 10, 11);
        byte[] path = GeneratedProtocolPacketWriter.WritePathGeometry(12, MilFillMode.Alternate, GeneratedProtocolPacketWriter.WriteEmptyPathFigures(), 6);

        Assert.AreEqual(
            (0x78u, 1u, 2d, 5d, 6u, 8u, 0x7Bu, 9u, 6u, 1u, 8u, 10u, 11u, 0x7Du, 48u, 48u),
            (
                BitConverter.ToUInt32(line, 0), BitConverter.ToUInt32(line, 4), BitConverter.ToDouble(line, 8),
                BitConverter.ToDouble(line, 32), BitConverter.ToUInt32(line, 40), BitConverter.ToUInt32(line, 48),
                BitConverter.ToUInt32(group, 0), BitConverter.ToUInt32(group, 4), BitConverter.ToUInt32(group, 8),
                BitConverter.ToUInt32(group, 12), BitConverter.ToUInt32(group, 16), BitConverter.ToUInt32(group, 20),
                BitConverter.ToUInt32(group, 24), BitConverter.ToUInt32(path, 0), BitConverter.ToUInt32(path, 16),
                BitConverter.ToUInt32(path, 20)));
    }

    [TestMethod]
    public void WhenFactoryCreatesGeometryResourcesThenEachResourceHasItsStrongType()
    {
        GeneratedProtocolHandleTable table = new();
        MilResourceType[] types =
        [
            MilResourceType.LineGeometry, MilResourceType.RectangleGeometry, MilResourceType.EllipseGeometry,
            MilResourceType.GeometryGroup, MilResourceType.CombinedGeometry, MilResourceType.PathGeometry
        ];
        foreach ((MilResourceType type, int index) in types.Select((type, index) => (type, index)))
        {
            Assert.AreEqual(0, table.Create((uint)index + 1, type));
        }

        Assert.AreEqual(
            (true, true, true, true, true, true),
            (
                Get<GeneratedLineGeometryResource>(table, 1) is not null,
                Get<GeneratedRectangleGeometryResource>(table, 2) is not null,
                Get<GeneratedEllipseGeometryResource>(table, 3) is not null,
                Get<GeneratedGeometryGroupResource>(table, 4) is not null,
                Get<GeneratedCombinedGeometryResource>(table, 5) is not null,
                Get<GeneratedPathGeometryResource>(table, 6) is not null));
    }

    [TestMethod]
    public void WhenAllGeometryResourcesAreUpdatedThenValuesDependenciesAndEnumsMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        MilRectD rect = new(1, 2, 3, 4);
        int result = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.TranslateTransform),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.PointResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(3, MilResourceType.DoubleResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(4, MilResourceType.RectResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.LineGeometry),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(11, MilResourceType.RectangleGeometry),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(12, MilResourceType.EllipseGeometry),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(13, MilResourceType.GeometryGroup),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(14, MilResourceType.CombinedGeometry),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(15, MilResourceType.PathGeometry),
            GeneratedProtocolPacketWriter.WriteLineGeometry(10, new MilPoint2D(1, 2), new MilPoint2D(3, 4), 1, 2, 2),
            GeneratedProtocolPacketWriter.WriteRectangleGeometry(11, 5, 6, rect, 1, 3, 3, 4),
            GeneratedProtocolPacketWriter.WriteEllipseGeometry(12, 7, 8, new MilPoint2D(9, 10), 1, 3, 3, 2),
            GeneratedProtocolPacketWriter.WriteGeometryGroup(13, MilFillMode.Winding, 1, 10, 11, 12),
            GeneratedProtocolPacketWriter.WriteCombinedGeometry(14, MilCombineMode.Xor, 1, 10, 13),
            GeneratedProtocolPacketWriter.WritePathGeometry(15, MilFillMode.Alternate, GeneratedProtocolPacketWriter.WriteEmptyPathFigures(), 1)
        ]);

        Assert.AreEqual(
            (0, (new MilPoint2D(1, 2), new MilPoint2D(3, 4)), (5d, 6d, rect), (7d, 8d, new MilPoint2D(9, 10)), MilFillMode.Winding, 3, MilCombineMode.Xor, true, 48),
            (
                result, Get<GeneratedLineGeometryResource>(table, 10).Value,
                Get<GeneratedRectangleGeometryResource>(table, 11).Value,
                Get<GeneratedEllipseGeometryResource>(table, 12).Value,
                Get<GeneratedGeometryGroupResource>(table, 13).FillRule,
                Get<GeneratedGeometryGroupResource>(table, 13).Children.Count,
                Get<GeneratedCombinedGeometryResource>(table, 14).CombineMode,
                Get<GeneratedCombinedGeometryResource>(table, 14).Geometry2 is GeneratedGeometryGroupResource,
                Get<GeneratedPathGeometryResource>(table, 15).Figures.Length));
    }

    [TestMethod]
    public void WhenDependencyLookupOrEnumValidationFailsThenExistingGeometryRemainsUnchanged()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.PointResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.ColorResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.LineGeometry),
            GeneratedProtocolPacketWriter.WriteLineGeometry(10, new MilPoint2D(1, 2), new MilPoint2D(3, 4), startPointAnimation: 1)
        ]);
        GeneratedLineGeometryResource line = Get<GeneratedLineGeometryResource>(table, 10);
        int changeCount = line.ChangeCount;

        int wrongType = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteLineGeometry(10, new MilPoint2D(5, 6), new MilPoint2D(7, 8), startPointAnimation: 2));
        int unknown = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteLineGeometry(10, new MilPoint2D(9, 10), new MilPoint2D(11, 12), transform: 99));

        Assert.AreEqual(
            (Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, (new MilPoint2D(1, 2), new MilPoint2D(3, 4)), changeCount),
            (wrongType, unknown, line.Value, line.ChangeCount));
    }

    [TestMethod]
    public void WhenGeometryGroupIsReplacedThenChildrenReferencesAreTransactional()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.LineGeometry),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.RectangleGeometry),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(3, MilResourceType.GeometryGroup),
            GeneratedProtocolPacketWriter.WriteGeometryGroup(3, MilFillMode.Alternate, 0, 1, 2)
        ]);
        GeneratedProtocolResource first = Get<GeneratedProtocolResource>(table, 1);
        GeneratedProtocolResource second = Get<GeneratedProtocolResource>(table, 2);

        int replace = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteGeometryGroup(3, MilFillMode.Winding, 0, 2));
        int nullChild = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteGeometryGroup(3, MilFillMode.Alternate, 0, 0));

        Assert.AreEqual(
            (0, Direct3D9Factory.UceMalformedPacketHResult, 1, MilFillMode.Winding, 1, 2),
            (replace, nullChild, Get<GeneratedGeometryGroupResource>(table, 3).Children.Count,
                Get<GeneratedGeometryGroupResource>(table, 3).FillRule, first.ReferenceCount, second.ReferenceCount));
    }

    [TestMethod]
    public void WhenPathGeometryContainsValidFigureAndLineThenPayloadIsAccepted()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        byte[] figures = CreateSingleLinePath();
        int result = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.PathGeometry),
            GeneratedProtocolPacketWriter.WritePathGeometry(1, MilFillMode.Winding, figures)
        ]);

        Assert.AreEqual((0, figures.Length, MilFillMode.Winding), (result, Get<GeneratedPathGeometryResource>(table, 1).Figures.Length, Get<GeneratedPathGeometryResource>(table, 1).FillRule));
    }

    [TestMethod]
    public void WhenPathGeometryInternalCountsBackPointersOrTrailingBytesAreInvalidThenUpdateIsRejected()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.PathGeometry),
            GeneratedProtocolPacketWriter.WritePathGeometry(1, MilFillMode.Alternate, GeneratedProtocolPacketWriter.WriteEmptyPathFigures())
        ]);
        GeneratedPathGeometryResource path = Get<GeneratedPathGeometryResource>(table, 1);
        int changeCount = path.ChangeCount;
        byte[] wrongCount = CreateSingleLinePath();
        WriteUInt32(wrongCount, 40, 2);
        byte[] wrongBackPointer = CreateSingleLinePath();
        WriteUInt32(wrongBackPointer, 48 + 40 + 8, 4);
        byte[] trailing = [.. CreateSingleLinePath(), 0];
        WriteUInt32(trailing, 0, (uint)trailing.Length);

        int countResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WritePathGeometry(1, MilFillMode.Winding, wrongCount));
        int backResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WritePathGeometry(1, MilFillMode.Winding, wrongBackPointer));
        int trailingResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WritePathGeometry(1, MilFillMode.Winding, trailing));

        Assert.AreEqual(
            (Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 48, changeCount),
            (countResult, backResult, trailingResult, path.Figures.Length, path.ChangeCount));
    }

    [TestMethod]
    public void WhenChildOrAnimationChangesThenNotificationPropagatesThroughGeometryGraph()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.PointResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.LineGeometry),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(3, MilResourceType.GeometryGroup),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(4, MilResourceType.CombinedGeometry),
            GeneratedProtocolPacketWriter.WriteLineGeometry(2, new MilPoint2D(1, 2), new MilPoint2D(3, 4), startPointAnimation: 1),
            GeneratedProtocolPacketWriter.WriteGeometryGroup(3, MilFillMode.Alternate, 0, 2),
            GeneratedProtocolPacketWriter.WriteCombinedGeometry(4, MilCombineMode.Union, geometry1: 3)
        ]);

        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WritePointResource(1, new MilPoint2D(8, 9)));

        Assert.AreEqual((0, 2, 2, 2), (result, Get<GeneratedLineGeometryResource>(table, 2).ChangeCount, Get<GeneratedGeometryGroupResource>(table, 3).ChangeCount, Get<GeneratedCombinedGeometryResource>(table, 4).ChangeCount));
    }

    [TestMethod]
    public void WhenGeometryIsDuplicatedAndLastHandleDeletedThenDependenciesReleaseDeterministically()
    {
        GeneratedProtocolHandleTable source = new();
        GeneratedProtocolHandleTable target = new();
        GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, source);
        _ = channels.TryAdd(2, target);
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, channels).CreateRouter();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.PointResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.LineGeometry),
            GeneratedProtocolPacketWriter.WriteLineGeometry(2, new MilPoint2D(1, 2), new MilPoint2D(3, 4), startPointAnimation: 1),
            GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(2, 2, 20)
        ]);
        GeneratedProtocolResource dependency = Get<GeneratedProtocolResource>(source, 1);
        GeneratedProtocolResource geometry = Get<GeneratedProtocolResource>(source, 2);

        int deleteSource = source.Delete(2, MilResourceType.LineGeometry);
        int deleteDependency = source.Delete(1, MilResourceType.PointResource);
        int deleteTarget = target.Delete(20, MilResourceType.LineGeometry);

        Assert.AreEqual((0, 0, 0, 0, 0, true), (deleteSource, deleteDependency, deleteTarget, dependency.ReferenceCount, geometry.ReferenceCount, geometry.IsReleased));
    }

    [TestMethod]
    public void WhenGeometryPacketSequenceFailsThenLaterUpdatesAreNotProcessed()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        int result = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.LineGeometry),
            GeneratedProtocolPacketWriter.WriteLineGeometry(1, new MilPoint2D(1, 2), new MilPoint2D(3, 4), transform: 99),
            GeneratedProtocolPacketWriter.WriteLineGeometry(1, new MilPoint2D(5, 6), new MilPoint2D(7, 8))
        ]);

        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, default((MilPoint2D, MilPoint2D)), 0), (result, Get<GeneratedLineGeometryResource>(table, 1).Value, Get<GeneratedLineGeometryResource>(table, 1).ChangeCount));
    }

    private static byte[] CreateSingleLinePath()
    {
        byte[] data = new byte[120];
        WriteUInt32(data, 0, (uint)data.Length);
        WriteUInt32(data, 40, 1);
        int figure = 48;
        WriteUInt32(data, figure, 0);
        WriteUInt32(data, figure + 8, 1);
        WriteUInt32(data, figure + 12, 72);
        WriteUInt32(data, figure + 32, 40);
        int segment = figure + 40;
        WriteUInt32(data, segment, (uint)MilSegmentType.Line);
        WriteUInt32(data, segment + 8, 0);
        return data;
    }

    private static void WriteUInt32(byte[] data, int offset, uint value) => BitConverter.GetBytes(value).CopyTo(data, offset);

    private static (GeneratedProtocolHandleTable Table, GeneratedProtocolRouter Router) CreateContext()
    {
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, table);
        return (table, new GeneratedProtocolProductionContext(1, channels).CreateRouter());
    }

    private static T Get<T>(GeneratedProtocolHandleTable table, uint handle) where T : GeneratedProtocolResource
    {
        Assert.IsTrue(table.TryGetResource(handle, out GeneratedProtocolResource? resource));
        return (T)resource!;
    }
}
