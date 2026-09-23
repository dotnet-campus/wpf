using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedProtocolTests
{
    [TestMethod]
    public void WhenProtocolConstantsAreReadThenFrozenIdsMatchConsumedGeneratedOutputs()
    {
        Assert.AreEqual(
            (0x200184C0u, 0x0BDDCB2Bu, 0x8D, 0x61, 1u, 9u, 98u),
            (
                GeneratedProtocolFingerprint.MilSdkVersion,
                GeneratedProtocolFingerprint.DwmSdkVersion,
                GeneratedProtocolFingerprint.LastCommandId,
                GeneratedProtocolFingerprint.LastResourceTypeId,
                (uint) MilCommand.TransportSyncFlush,
                (uint) MilCommand.ChannelDuplicateHandle,
                (uint) MilResourceType.Last));
    }

    [TestMethod]
    public void WhenCommandLayoutsAreMeasuredThenSizesAndOffsetsMatchNativePackOneStructures()
    {
        Assert.AreEqual(
            (4, 8, 12, 12, 16, 4, 4, 8, 4, 8, 4, 8, 12),
            (
                Marshal.SizeOf<MilTransportSyncFlushCommand>(),
                Marshal.SizeOf<MilTransportDestroyResourcesOnChannelCommand>(),
                Marshal.SizeOf<MilChannelCreateResourceCommand>(),
                Marshal.SizeOf<MilChannelDeleteResourceCommand>(),
                Marshal.SizeOf<MilChannelDuplicateHandleCommand>(),
                Marshal.OffsetOf<MilTransportDestroyResourcesOnChannelCommand>(nameof(MilTransportDestroyResourcesOnChannelCommand.Channel)).ToInt32(),
                Marshal.OffsetOf<MilChannelCreateResourceCommand>(nameof(MilChannelCreateResourceCommand.Handle)).ToInt32(),
                Marshal.OffsetOf<MilChannelCreateResourceCommand>(nameof(MilChannelCreateResourceCommand.ResourceType)).ToInt32(),
                Marshal.OffsetOf<MilChannelDeleteResourceCommand>(nameof(MilChannelDeleteResourceCommand.Handle)).ToInt32(),
                Marshal.OffsetOf<MilChannelDeleteResourceCommand>(nameof(MilChannelDeleteResourceCommand.ResourceType)).ToInt32(),
                Marshal.OffsetOf<MilChannelDuplicateHandleCommand>(nameof(MilChannelDuplicateHandleCommand.Original)).ToInt32(),
                Marshal.OffsetOf<MilChannelDuplicateHandleCommand>(nameof(MilChannelDuplicateHandleCommand.TargetChannel)).ToInt32(),
                Marshal.OffsetOf<MilChannelDuplicateHandleCommand>(nameof(MilChannelDuplicateHandleCommand.Duplicate)).ToInt32()));
    }

    [TestMethod]
    public void WhenPacketsAreWrittenThenGoldenBytesMatchLittleEndianNativeLayouts()
    {
        byte[] sync = GeneratedProtocolPacketWriter.WriteTransportSyncFlush();
        byte[] destroy = GeneratedProtocolPacketWriter.WriteTransportDestroyResourcesOnChannel(0x11223344);
        byte[] create = GeneratedProtocolPacketWriter.WriteChannelCreateResource(0x55667788, MilResourceType.Visual);
        byte[] delete = GeneratedProtocolPacketWriter.WriteChannelDeleteResource(0x99AABBCC, MilResourceType.BitmapCache);
        byte[] duplicate = GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(1, 2, 3);

        CollectionAssert.AreEqual(
            new byte[]
            {
                1, 0, 0, 0,
                2, 0, 0, 0, 0x44, 0x33, 0x22, 0x11,
                7, 0, 0, 0, 0x88, 0x77, 0x66, 0x55, 39, 0, 0, 0,
                8, 0, 0, 0, 0xCC, 0xBB, 0xAA, 0x99, 94, 0, 0, 0,
                9, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0
            },
            sync.Concat(destroy).Concat(create).Concat(delete).Concat(duplicate).ToArray());
    }

    [TestMethod]
    public void WhenCorePacketsAreRoutedThenTypedHandlersReceiveDecodedValues()
    {
        List<string> calls = [];
        GeneratedProtocolRouter router = new(new GeneratedProtocolHandlers(
            () => Record(calls, "Flush"),
            channel => Record(calls, $"Destroy:{channel}"),
            (handle, type) => Record(calls, $"Create:{handle}:{type}"),
            (handle, type) => Record(calls, $"Delete:{handle}:{type}"),
            (original, channel, duplicate) => Record(calls, $"Duplicate:{original}:{channel}:{duplicate}")));

        int result = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteTransportSyncFlush(),
            GeneratedProtocolPacketWriter.WriteTransportDestroyResourcesOnChannel(7),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.Visual),
            GeneratedProtocolPacketWriter.WriteChannelDeleteResource(10, MilResourceType.Visual),
            GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(10, 8, 11)
        ]);

        Assert.AreEqual(
            (0, "Flush|Destroy:7|Create:10:Visual|Delete:10:Visual|Duplicate:10:8:11"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenPacketSizeIsNotExactThenMalformedPacketIsReturnedWithoutCallingHandler()
    {
        int calls = 0;
        GeneratedProtocolRouter router = CreateRouter(() => calls++);
        byte[] truncated = GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.Visual)[..^1];
        byte[] oversized = [.. GeneratedProtocolPacketWriter.WriteTransportSyncFlush(), 0];

        int truncatedResult = router.ProcessPacket(truncated);
        int oversizedResult = router.ProcessPacket(oversized);

        Assert.AreEqual(
            (Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 0),
            (truncatedResult, oversizedResult, calls));
    }

    [TestMethod]
    public void WhenCommandIsUnknownThenUnknownPacketIsReturned()
    {
        GeneratedProtocolRouter router = CreateRouter();

        int result = router.ProcessPacket([0xFE, 0, 0, 0]);

        Assert.AreEqual(Direct3D9Factory.UceUnknownPacketHResult, result);
    }

    [TestMethod]
    public void WhenPacketHandlerFailsThenBatchStopsAtFirstFailure()
    {
        List<string> calls = [];
        GeneratedProtocolRouter router = new(new GeneratedProtocolHandlers(
            () => Record(calls, "First"),
            _ => Record(calls, "Unexpected"),
            (_, _) =>
            {
                calls.Add("Fail");
                return Direct3D9Factory.GenericFailureHResult;
            },
            (_, _) => Record(calls, "Unexpected"),
            (_, _, _) => Record(calls, "Unexpected")));

        int result = router.ProcessPackets(
        [
            GeneratedProtocolPacketWriter.WriteTransportSyncFlush(),
            GeneratedProtocolPacketWriter.WriteChannelCreateResource(1, MilResourceType.Visual),
            GeneratedProtocolPacketWriter.WriteChannelDeleteResource(1, MilResourceType.Visual)
        ]);

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, "First|Fail"),
            (result, string.Join('|', calls)));
    }

    [TestMethod]
    public void WhenProductionCommandsCreateDuplicateAndDeleteThenResourceIdentityAndReferencesArePreserved()
    {
        GeneratedProtocolHandleTable source = new();
        GeneratedProtocolHandleTable target = new();
        GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, source);
        _ = channels.TryAdd(2, target);
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, channels).CreateRouter();

        int createResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.Visual));
        int duplicateResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(10, 2, 20));
        _ = source.TryGetResource(10, out GeneratedProtocolResource? sourceResource);
        _ = target.TryGetResource(20, out GeneratedProtocolResource? targetResource);
        int deleteResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelDeleteResource(10, MilResourceType.Visual));

        Assert.AreEqual(
            (0, 0, 0, true, 1),
            (createResult, duplicateResult, deleteResult, ReferenceEquals(sourceResource, targetResource), targetResource!.ReferenceCount));
    }

    [TestMethod]
    public void WhenHandleOrTypeIsInvalidThenMalformedPacketIsReturnedWithoutChangingTable()
    {
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, table);
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, channels).CreateRouter();
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.Visual));

        int collisionResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.BitmapCache));
        int mismatchResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelDeleteResource(10, MilResourceType.BitmapCache));
        int missingResult = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(99, 1, 20));
        _ = table.TryGetResource(10, out GeneratedProtocolResource? resource);

        Assert.AreEqual(
            (Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, MilResourceType.Visual),
            (collisionResult, mismatchResult, missingResult, resource!.ResourceType));
    }

    [TestMethod]
    public void WhenTargetChannelDoesNotExistThenHandleLookupFailureIsReturned()
    {
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry channels = new();
        _ = channels.TryAdd(1, table);
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, channels).CreateRouter();
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelCreateResource(10, MilResourceType.Visual));

        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(10, 2, 20));

        Assert.AreEqual(Direct3D9Factory.UceHandleLookupFailedHResult, result);
    }

    private static GeneratedProtocolRouter CreateRouter(Action? onCall = null)
    {
        int Handler()
        {
            onCall?.Invoke();
            return Direct3D9Factory.SuccessHResult;
        }

        return new GeneratedProtocolRouter(new GeneratedProtocolHandlers(
            Handler,
            _ => Handler(),
            (_, _) => Handler(),
            (_, _) => Handler(),
            (_, _, _) => Handler()));
    }

    private static int Record(List<string> calls, string value)
    {
        calls.Add(value);
        return Direct3D9Factory.SuccessHResult;
    }
}
