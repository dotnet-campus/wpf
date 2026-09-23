using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedValueResourceTests
{
    [TestMethod]
    public void WhenValueCommandLayoutsAreMeasuredThenNativeSizesAndOffsetsMatch()
    {
        Assert.AreEqual(
            (16, 24, 24, 40, 24, 56, 20, 20, 24, 4, 8),
            (
                Marshal.SizeOf<MilDoubleResourceCommand>(),
                Marshal.SizeOf<MilColorResourceCommand>(),
                Marshal.SizeOf<MilPointResourceCommand>(),
                Marshal.SizeOf<MilRectResourceCommand>(),
                Marshal.SizeOf<MilSizeResourceCommand>(),
                Marshal.SizeOf<MilMatrixResourceCommand>(),
                Marshal.SizeOf<MilPoint3DResourceCommand>(),
                Marshal.SizeOf<MilVector3DResourceCommand>(),
                Marshal.SizeOf<MilQuaternionResourceCommand>(),
                Marshal.OffsetOf<MilDoubleResourceCommand>(nameof(MilDoubleResourceCommand.Type)).ToInt32(),
                Marshal.OffsetOf<MilDoubleResourceCommand>(nameof(MilDoubleResourceCommand.Value)).ToInt32()));
    }

    [TestMethod]
    public void WhenValuePacketsAreWrittenThenCommandIdsHandlesAndValuesMatchNativeLayout()
    {
        byte[] doublePacket = GeneratedProtocolPacketWriter.WriteDoubleResource(0x11223344, 1.5);
        byte[] colorPacket = GeneratedProtocolPacketWriter.WriteColorResource(7, new MilColorF(1, 0.25f, 0.5f, 0.75f));
        byte[] matrixPacket = GeneratedProtocolPacketWriter.WriteMatrixResource(8, new MilMatrix3x2D(1, 2, 3, 4, 5, 6));

        Assert.AreEqual(
            (14u, 0x11223344u, 1.5, 15u, 7u, 1f, 0.25f, 19u, 8u, 6d),
            (
                BitConverter.ToUInt32(doublePacket, 0),
                BitConverter.ToUInt32(doublePacket, 4),
                BitConverter.ToDouble(doublePacket, 8),
                BitConverter.ToUInt32(colorPacket, 0),
                BitConverter.ToUInt32(colorPacket, 4),
                BitConverter.ToSingle(colorPacket, 8),
                BitConverter.ToSingle(colorPacket, 12),
                BitConverter.ToUInt32(matrixPacket, 0),
                BitConverter.ToUInt32(matrixPacket, 4),
                BitConverter.ToDouble(matrixPacket, 48)));
    }

    [TestMethod]
    public void WhenFactoryCreatesValueResourcesThenEachResourceHasItsStrongValueType()
    {
        GeneratedProtocolHandleTable table = new();

        int[] results =
        [
            table.Create(1, MilResourceType.DoubleResource),
            table.Create(2, MilResourceType.ColorResource),
            table.Create(3, MilResourceType.PointResource),
            table.Create(4, MilResourceType.RectResource),
            table.Create(5, MilResourceType.SizeResource),
            table.Create(6, MilResourceType.MatrixResource),
            table.Create(7, MilResourceType.Point3DResource),
            table.Create(8, MilResourceType.Vector3DResource),
            table.Create(9, MilResourceType.QuaternionResource)
        ];
        _ = table.TryGetResource(1, out GeneratedProtocolResource? doubleResource);
        _ = table.TryGetResource(2, out GeneratedProtocolResource? colorResource);
        _ = table.TryGetResource(3, out GeneratedProtocolResource? pointResource);
        _ = table.TryGetResource(4, out GeneratedProtocolResource? rectResource);
        _ = table.TryGetResource(5, out GeneratedProtocolResource? sizeResource);
        _ = table.TryGetResource(6, out GeneratedProtocolResource? matrixResource);
        _ = table.TryGetResource(7, out GeneratedProtocolResource? point3DResource);
        _ = table.TryGetResource(8, out GeneratedProtocolResource? vector3DResource);
        _ = table.TryGetResource(9, out GeneratedProtocolResource? quaternionResource);

        Assert.AreEqual(
            (true, true, true, true, true, true, true, true, true, true),
            (
                results.All(result => result == 0),
                doubleResource is GeneratedValueResource<double>,
                colorResource is GeneratedValueResource<MilColorF>,
                pointResource is GeneratedValueResource<MilPoint2D>,
                rectResource is GeneratedValueResource<MilRectD>,
                sizeResource is GeneratedValueResource<MilSizeD>,
                matrixResource is GeneratedValueResource<MilMatrix3x2D>,
                point3DResource is GeneratedValueResource<MilPoint3F>,
                vector3DResource is GeneratedValueResource<MilPoint3F>,
                quaternionResource is GeneratedValueResource<MilQuaternionF>));
    }

    [TestMethod]
    public void WhenValueResourceIsCreatedUpdatedDuplicatedAndDeletedThenIdentityValueAndReferencesArePreserved()
    {
        GeneratedProtocolHandleTable source = new();
        GeneratedProtocolHandleTable target = new();
        GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, source);
        _ = channels.TryAdd(2, target);
        GeneratedProtocolRouter sourceRouter = new GeneratedProtocolProductionContext(1, channels).CreateRouter();
        GeneratedProtocolRouter targetRouter = new GeneratedProtocolProductionContext(2, channels).CreateRouter();

        int result = sourceRouter.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.DoubleResource),
            GeneratedProtocolPacketWriter.WriteDoubleResource(10, 12.5),
            GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(10, 2, 20)
        ]);
        int duplicateUpdateResult = targetRouter.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(20, 24.5));
        _ = source.TryGetResource(10, out GeneratedProtocolResource? sourceResource);
        _ = target.TryGetResource(20, out GeneratedProtocolResource? targetResource);
        GeneratedValueResource<double> valueResource = (GeneratedValueResource<double>) sourceResource!;
        int sourceDeleteResult = sourceRouter.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelDeleteResource(10, MilResourceType.DoubleResource));
        int targetDeleteResult = targetRouter.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelDeleteResource(20, MilResourceType.DoubleResource));

        Assert.AreEqual(
            (0, 0, true, 24.5, 2, 0, 0, 0),
            (result, duplicateUpdateResult, ReferenceEquals(sourceResource, targetResource), valueResource.Value, valueResource.ChangeCount, sourceDeleteResult, targetDeleteResult, valueResource.ReferenceCount));
    }

    [TestMethod]
    public void WhenValueUpdateIsInvalidThenOldValueAndChangeCountRemainUnchanged()
    {
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, table);
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, channels).CreateRouter();
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.DoubleResource));
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(10, 3.5));
        _ = table.TryGetResource(10, out GeneratedProtocolResource? resource);
        GeneratedValueResource<double> valueResource = (GeneratedValueResource<double>) resource!;

        int mismatchResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WritePointResource(10, new MilPoint2D(1, 2)));
        int missingResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(99, 8.5));
        byte[] truncated = GeneratedProtocolPacketWriter.WriteDoubleResource(10, 9.5)[..^1];
        int truncatedResult = router.ProcessPacket(truncated);
        byte[] oversized = [.. GeneratedProtocolPacketWriter.WriteDoubleResource(10, 10.5), 0];
        int oversizedResult = router.ProcessPacket(oversized);

        Assert.AreEqual(
            (Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 3.5, 1),
            (mismatchResult, missingResult, truncatedResult, oversizedResult, valueResource.Value, valueResource.ChangeCount));
    }

    [TestMethod]
    public void WhenInvalidResourceTypeOrCollisionIsCreatedThenFactoryAndTableRemainUnchanged()
    {
        GeneratedProtocolHandleTable table = new();
        int invalidResult = table.Create(1, MilResourceType.Last);
        int createResult = table.Create(2, MilResourceType.QuaternionResource);
        int collisionResult = table.Create(2, MilResourceType.DoubleResource);
        _ = table.TryGetResource(1, out GeneratedProtocolResource? invalidResource);
        _ = table.TryGetResource(2, out GeneratedProtocolResource? createdResource);

        Assert.AreEqual(
            (Direct3D9Factory.UceMalformedPacketHResult, 0, Direct3D9Factory.UceMalformedPacketHResult, null, MilResourceType.QuaternionResource, 1),
            (invalidResult, createResult, collisionResult, invalidResource, createdResource!.ResourceType, createdResource.ReferenceCount));
    }
}
