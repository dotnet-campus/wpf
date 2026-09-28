using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class GeneratedMediaPlayerResourceTests
{
    [TestMethod]
    public void WhenLayoutIsMeasuredThenFixedWidthFieldsMatch()
    {
        Assert.AreEqual((20, 8, 16), (Marshal.SizeOf<MilMediaPlayerCommand>(), Marshal.OffsetOf<MilMediaPlayerCommand>(nameof(MilMediaPlayerCommand.Media)).ToInt32(), Marshal.OffsetOf<MilMediaPlayerCommand>(nameof(MilMediaPlayerCommand.NotifyUceDirect)).ToInt32()));
    }

    [TestMethod]
    public void WhenPacketIsWrittenThenIndependentGoldenBytesMatch()
    {
        CollectionAssert.AreEqual(new byte[] { 23, 0, 0, 0, 1, 0, 0, 0, 8, 7, 6, 5, 4, 3, 2, 1, 2, 0, 0, 0 }, GeneratedProtocolPacketWriter.WriteMediaPlayer(1, 0x0102030405060708, 2));
    }

    [TestMethod]
    public void WhenProviderNeedsNativeBridgeThenFailureReleasesBothReferencesWithoutNotification()
    {
        using MediaStub media = new();
        var (table, router, resource) = Context();
        int result = router.ProcessPacket(Packet(media, -1));
        _ = table.Delete(1, MilResourceType.MediaPlayer);
        Assert.AreEqual((unchecked((int)0x80004001), 0, 2, true, 0), (result, media.State->References, media.State->Releases, resource.NotifyUceDirect, resource.ChangeCount));
    }

    [TestMethod]
    public void WhenQueryFailsWithOutputThenFirstErrorAndCleanupArePreserved()
    {
        using MediaStub media = new();
        media.State->QueryResult = Direct3D9Factory.NoInterfaceHResult;
        var (_, router, _) = Context();
        int result = router.ProcessPacket(Packet(media));
        Assert.AreEqual((Direct3D9Factory.NoInterfaceHResult, 0, 2), (result, media.State->References, media.State->Releases));
    }

    [TestMethod]
    public void WhenQueryReturnsNullThenNoInterfaceAndTransportReleaseResult()
    {
        using MediaStub media = new(); media.State->NullOutput = 1;
        var (_, router, _) = Context();
        int result = router.ProcessPacket(Packet(media));
        Assert.AreEqual((Direct3D9Factory.NoInterfaceHResult, 0, 1), (result, media.State->References, media.State->Releases));
    }

    [TestMethod]
    public void WhenNullSourceIsReceivedThenFlagIsUpdatedWithoutNotification()
    {
        var (_, router, resource) = Context();
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteMediaPlayer(1, 0, 2));
        Assert.AreEqual((unchecked((int)0x80070006), true, 0), (result, resource.NotifyUceDirect, resource.ChangeCount));
    }

    [TestMethod]
    public void WhenWrongHandleIsRejectedThenTransportReferenceRemainsWithCaller()
    {
        using MediaStub media = new(); var (_, router, _) = Context();
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteMediaPlayer(99, (ulong)media.State, 1));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 1, 0), (result, media.State->References, media.State->Releases));
    }

    [TestMethod]
    public void WhenShortPacketIsRejectedThenTransportReferenceRemainsWithCaller()
    {
        using MediaStub media = new(); var (_, router, _) = Context();
        int result = router.ProcessPacket(Packet(media).AsSpan(0, 19));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 1, 0), (result, media.State->References, media.State->Releases));
    }

    [TestMethod]
    public void WhenReleasedResourceIsCalledThenTransportReferenceIsNotConsumed()
    {
        using MediaStub media = new(); var (table, _, resource) = Context();
        _ = table.Delete(1, MilResourceType.MediaPlayer);
        int result = resource.ProcessCommand(Packet(media));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 1), (result, media.State->References));
    }

    [TestMethod]
    public void WhenBatchFailsThenUnexecutedPacketReferenceRemainsWithCaller()
    {
        using MediaStub first = new(); using MediaStub second = new(); var (_, router, _) = Context();
        int result = router.ProcessPackets([Packet(first), Packet(second)]);
        Assert.AreEqual((unchecked((int)0x80004001), 0, 1), (result, first.State->References, second.State->References));
    }

    [TestMethod]
    public void WhenConsumersAndDuplicateOutliveHandleThenLastDependencyReleasesMedia()
    {
        var (table, router, resource) = Context(); GeneratedProtocolHandleTable duplicate = new();
        _ = table.DuplicateTo(1, duplicate, 5);
        _ = table.Create(2, MilResourceType.VideoDrawing); _ = table.Create(3, MilResourceType.RenderData);
        Assert.AreEqual(0, router.ProcessPackets([
            GeneratedProtocolPacketWriter.WriteVideoDrawing(2, default, 1),
            GeneratedProtocolPacketWriter.WriteRenderData(3, GeneratedProtocolPacketWriter.WriteDrawVideoRecord(default, 1), GeneratedProtocolPacketWriter.WriteDrawVideoAnimateRecord(default, 1, 0))]));
        _ = table.Delete(1, MilResourceType.MediaPlayer); _ = duplicate.Delete(5, MilResourceType.MediaPlayer);
        int retainedReferences = resource.ReferenceCount;
        _ = table.Delete(2, MilResourceType.VideoDrawing);
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteRenderData(3));
        Assert.AreEqual((3, true), (retainedReferences, resource.IsReleased));
    }

    private static byte[] Packet(MediaStub media, int notify = 0) => GeneratedProtocolPacketWriter.WriteMediaPlayer(1, (ulong)media.State, notify);
    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter, GeneratedMediaPlayerResource) Context()
    {
        GeneratedProtocolHandleTable table = new(); GeneratedProtocolChannelRegistry registry = new();
        _ = registry.TryAdd(1, table); _ = table.Create(1, MilResourceType.MediaPlayer);
        _ = table.TryGetResource(1, out GeneratedProtocolResource? resource);
        return (table, new GeneratedProtocolProductionContext(1, registry).CreateRouter(), (GeneratedMediaPlayerResource)resource!);
    }

    private struct MediaState
    {
        internal void** Vtable;
        internal int References;
        internal int Releases;
        internal int QueryResult;
        internal int NullOutput;
    }

    private sealed class MediaStub : IDisposable
    {
        internal MediaState* State { get; }
        internal MediaStub()
        {
            State = (MediaState*)NativeMemory.AllocZeroed((nuint)sizeof(MediaState));
            State->Vtable = (void**)NativeMemory.AllocZeroed(3, (nuint)sizeof(nint)); State->References = 1;
            State->Vtable[0] = (delegate* unmanaged[Stdcall]<MediaState*, Guid*, nint*, int>)&Query;
            State->Vtable[1] = (delegate* unmanaged[Stdcall]<MediaState*, uint>)&AddRef;
            State->Vtable[2] = (delegate* unmanaged[Stdcall]<MediaState*, uint>)&Release;
        }
        public void Dispose() { NativeMemory.Free(State->Vtable); NativeMemory.Free(State); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(MediaState* state, Guid* id, nint* output)
    {
        *output = 0;
        if (*id != new Guid("E6F1CC74-A0EB-4EBE-8241-089D1CB079D7")) return Direct3D9Factory.NoInterfaceHResult;
        if (state->NullOutput == 0) { state->References++; *output = (nint)state; }
        return state->QueryResult;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(MediaState* state) => (uint)++state->References;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(MediaState* state) { state->Releases++; return (uint)--state->References; }
}
