using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedTransformResourceTests
{
    [TestMethod]
    public void WhenTransformCommandLayoutsAreMeasuredThenNativeSizesAndOffsetsMatch()
    {
        Assert.AreEqual(
            (12, 32, 56, 56, 44, 60, 8, 24, 40, 56),
            (
                Marshal.SizeOf<MilTransformGroupCommand>(),
                Marshal.SizeOf<MilTranslateTransformCommand>(),
                Marshal.SizeOf<MilScaleTransformCommand>(),
                Marshal.SizeOf<MilSkewTransformCommand>(),
                Marshal.SizeOf<MilRotateTransformCommand>(),
                Marshal.SizeOf<MilMatrixTransformCommand>(),
                Marshal.OffsetOf<MilTranslateTransformCommand>(nameof(MilTranslateTransformCommand.X)).ToInt32(),
                Marshal.OffsetOf<MilTranslateTransformCommand>(nameof(MilTranslateTransformCommand.XAnimation)).ToInt32(),
                Marshal.OffsetOf<MilScaleTransformCommand>(nameof(MilScaleTransformCommand.ScaleXAnimation)).ToInt32(),
                Marshal.OffsetOf<MilMatrixTransformCommand>(nameof(MilMatrixTransformCommand.MatrixAnimation)).ToInt32()));
    }

    [TestMethod]
    public void WhenTransformPacketsAreWrittenThenCommandIdsAndPayloadsMatchNativeLayout()
    {
        byte[] group = GeneratedProtocolPacketWriter.WriteTransformGroup(1, 10, 11);
        byte[] translate = GeneratedProtocolPacketWriter.WriteTranslateTransform(2, 3, 4, 10, 11);
        byte[] matrix = GeneratedProtocolPacketWriter.WriteMatrixTransform(3, new MilMatrix3x2D(1, 2, 3, 4, 5, 6), 12);

        Assert.AreEqual(
            (0x72u, 1u, 8u, 10u, 11u, 0x73u, 3d, 4d, 10u, 11u, 0x77u, 6d, 12u),
            (
                BitConverter.ToUInt32(group, 0),
                BitConverter.ToUInt32(group, 4),
                BitConverter.ToUInt32(group, 8),
                BitConverter.ToUInt32(group, 12),
                BitConverter.ToUInt32(group, 16),
                BitConverter.ToUInt32(translate, 0),
                BitConverter.ToDouble(translate, 8),
                BitConverter.ToDouble(translate, 16),
                BitConverter.ToUInt32(translate, 24),
                BitConverter.ToUInt32(translate, 28),
                BitConverter.ToUInt32(matrix, 0),
                BitConverter.ToDouble(matrix, 48),
                BitConverter.ToUInt32(matrix, 56)));
    }

    [TestMethod]
    public void WhenFactoryCreatesTransformsThenEachResourceHasItsStrongType()
    {
        GeneratedProtocolHandleTable table = new();
        MilResourceType[] types =
        [
            MilResourceType.TransformGroup,
            MilResourceType.TranslateTransform,
            MilResourceType.ScaleTransform,
            MilResourceType.SkewTransform,
            MilResourceType.RotateTransform,
            MilResourceType.MatrixTransform
        ];
        foreach ((MilResourceType type, int index) in types.Select((type, index) => (type, index)))
        {
            Assert.AreEqual(0, table.Create((uint)index + 1, type));
        }

        Assert.AreEqual(
            (true, true, true, true, true, true),
            (
                GetResource<GeneratedTransformGroupResource>(table, 1) is not null,
                GetResource<GeneratedTranslateTransformResource>(table, 2) is not null,
                GetResource<GeneratedScaleTransformResource>(table, 3) is not null,
                GetResource<GeneratedSkewTransformResource>(table, 4) is not null,
                GetResource<GeneratedRotateTransformResource>(table, 5) is not null,
                GetResource<GeneratedMatrixTransformResource>(table, 6) is not null));
    }

    [TestMethod]
    public void WhenAllTransformsAreUpdatedThenValuesAndOptionalDependenciesMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        MilMatrix3x2D matrix = new(1, 2, 3, 4, 5, 6);
        int result = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.DoubleResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(11, MilResourceType.MatrixResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(20, MilResourceType.TranslateTransform),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(21, MilResourceType.ScaleTransform),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(22, MilResourceType.SkewTransform),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(23, MilResourceType.RotateTransform),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(24, MilResourceType.MatrixTransform),
            GeneratedProtocolPacketWriter.WriteTranslateTransform(20, 1, 2, 10),
            GeneratedProtocolPacketWriter.WriteScaleTransform(21, 3, 4, 5, 6, 10, 0, 10),
            GeneratedProtocolPacketWriter.WriteSkewTransform(22, 7, 8, 9, 10, 0, 10, 0, 10),
            GeneratedProtocolPacketWriter.WriteRotateTransform(23, 11, 12, 13, 10),
            GeneratedProtocolPacketWriter.WriteMatrixTransform(24, matrix, 11)
        ]);

        Assert.AreEqual(
            (0, (1d, 2d), (3d, 4d, 5d, 6d), (7d, 8d, 9d, 10d), (11d, 12d, 13d), matrix, 1, 2, 2, 1, true),
            (
                result,
                GetResource<GeneratedTranslateTransformResource>(table, 20).Value,
                GetResource<GeneratedScaleTransformResource>(table, 21).Value,
                GetResource<GeneratedSkewTransformResource>(table, 22).Value,
                GetResource<GeneratedRotateTransformResource>(table, 23).Value,
                GetResource<GeneratedMatrixTransformResource>(table, 24).Value,
                GetResource<GeneratedTranslateTransformResource>(table, 20).Animations.Count,
                GetResource<GeneratedScaleTransformResource>(table, 21).Animations.Count,
                GetResource<GeneratedSkewTransformResource>(table, 22).Animations.Count,
                GetResource<GeneratedRotateTransformResource>(table, 23).Animations.Count,
                GetResource<GeneratedMatrixTransformResource>(table, 24).Animation is not null));
    }

    [TestMethod]
    public void WhenDependencyLookupFailsThenExistingDataReferencesAndChangeCountRemainUnchanged()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.DoubleResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.ColorResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.TranslateTransform),
            GeneratedProtocolPacketWriter.WriteTranslateTransform(10, 3, 4, 1)
        ]);
        GeneratedTranslateTransformResource transform = GetResource<GeneratedTranslateTransformResource>(table, 10);
        GeneratedProtocolResource dependency = GetResource<GeneratedProtocolResource>(table, 1);
        int changeCount = transform.ChangeCount;

        int wrongType = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteTranslateTransform(10, 8, 9, 2));
        int unknown = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteTranslateTransform(10, 10, 11, 99));

        Assert.AreEqual(
            (Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, (3d, 4d), changeCount, 2),
            (wrongType, unknown, transform.Value, transform.ChangeCount, dependency.ReferenceCount));
    }

    [TestMethod]
    public void WhenNullAnimationHandlesAreUsedThenNoDependenciesAreRegistered()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        int result = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.TranslateTransform),
            GeneratedProtocolPacketWriter.WriteTranslateTransform(10, 1, 2)
        ]);

        Assert.AreEqual((0, 0), (result, GetResource<GeneratedTranslateTransformResource>(table, 10).Animations.Count));
    }

    [TestMethod]
    public void WhenTransformGroupIsUpdatedThenChildrenAreTypedReferencedAndReplaceable()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.TranslateTransform),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.ScaleTransform),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(3, MilResourceType.TransformGroup),
            GeneratedProtocolPacketWriter.WriteTransformGroup(3, 1, 2)
        ]);
        GeneratedProtocolResource first = GetResource<GeneratedProtocolResource>(table, 1);
        GeneratedProtocolResource second = GetResource<GeneratedProtocolResource>(table, 2);

        int replace = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteTransformGroup(3, 2));
        int nullChild = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteTransformGroup(3, 0));

        Assert.AreEqual(
            (0, Direct3D9Factory.UceMalformedPacketHResult, 1, 1, 2, 2),
            (replace, nullChild, GetResource<GeneratedTransformGroupResource>(table, 3).Children.Count, first.ReferenceCount, second.ReferenceCount, GetResource<GeneratedTransformGroupResource>(table, 3).ChangeCount));
    }

    [TestMethod]
    public void WhenTransformGroupPayloadIsMalformedThenOldChildrenRemainUnchanged()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.TranslateTransform),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.TransformGroup),
            GeneratedProtocolPacketWriter.WriteTransformGroup(2, 1)
        ]);
        byte[] mismatched = GeneratedProtocolPacketWriter.WriteTransformGroup(2, 1);
        BitConverter.GetBytes(8u).CopyTo(mismatched, 8);
        byte[] nonDivisible = GeneratedProtocolPacketWriter.WriteTransformGroup(2, 1);
        BitConverter.GetBytes(3u).CopyTo(nonDivisible, 8);
        Array.Resize(ref nonDivisible, 15);

        int first = router.ProcessPacket(mismatched);
        int second = router.ProcessPacket(nonDivisible);

        Assert.AreEqual(
            (Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 1, 1),
            (first, second, GetResource<GeneratedTransformGroupResource>(table, 2).Children.Count, GetResource<GeneratedTransformGroupResource>(table, 2).ChangeCount));
    }

    [TestMethod]
    public void WhenDependencyChangesThenNotificationPropagatesThroughTransformGroup()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.DoubleResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.TranslateTransform),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(3, MilResourceType.TransformGroup),
            GeneratedProtocolPacketWriter.WriteTranslateTransform(2, 10, 20, 1),
            GeneratedProtocolPacketWriter.WriteTransformGroup(3, 2)
        ]);
        GeneratedTranslateTransformResource transform = GetResource<GeneratedTranslateTransformResource>(table, 2);
        GeneratedTransformGroupResource group = GetResource<GeneratedTransformGroupResource>(table, 3);

        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(1, 42));

        Assert.AreEqual((0, 2, 2), (result, transform.ChangeCount, group.ChangeCount));
    }

    [TestMethod]
    public void WhenTransformHandlesAreDuplicatedAndDeletedThenDependenciesReleaseAfterLastHandle()
    {
        GeneratedProtocolHandleTable source = new();
        GeneratedProtocolHandleTable target = new();
        GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, source);
        _ = channels.TryAdd(2, target);
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, channels).CreateRouter();
        _ = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.DoubleResource),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(2, MilResourceType.TranslateTransform),
            GeneratedProtocolPacketWriter.WriteTranslateTransform(2, 3, 4, 1),
            GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(2, 2, 20)
        ]);
        GeneratedProtocolResource dependency = GetResource<GeneratedProtocolResource>(source, 1);
        GeneratedTranslateTransformResource transform = GetResource<GeneratedTranslateTransformResource>(source, 2);

        int deleteSource = source.Delete(2, MilResourceType.TranslateTransform);
        int deleteDependencyHandle = source.Delete(1, MilResourceType.DoubleResource);
        int deleteDuplicate = target.Delete(20, MilResourceType.TranslateTransform);

        Assert.AreEqual((0, 0, 0, 0, 0, true), (deleteSource, deleteDependencyHandle, deleteDuplicate, dependency.ReferenceCount, transform.ReferenceCount, transform.IsReleased));
    }

    [TestMethod]
    public void WhenPacketSequenceContainsInvalidTransformUpdateThenLaterPacketsAreNotProcessed()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        int result = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.TranslateTransform),
            GeneratedProtocolPacketWriter.WriteTranslateTransform(1, 1, 2, 99),
            GeneratedProtocolPacketWriter.WriteTranslateTransform(1, 3, 4)
        ]);

        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, (0d, 0d), 0), (result, GetResource<GeneratedTranslateTransformResource>(table, 1).Value, GetResource<GeneratedTranslateTransformResource>(table, 1).ChangeCount));
    }

    private static (GeneratedProtocolHandleTable Table, GeneratedProtocolRouter Router) CreateContext()
    {
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, table);
        return (table, new GeneratedProtocolProductionContext(1, channels).CreateRouter());
    }

    private static T GetResource<T>(GeneratedProtocolHandleTable table, uint handle) where T : GeneratedProtocolResource
    {
        Assert.IsTrue(table.TryGetResource(handle, out GeneratedProtocolResource? resource));
        return (T)resource!;
    }
}
