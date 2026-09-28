using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class GeneratedBitmapSourceResourceTests
{
    [TestMethod]
    public void WhenLayoutsAreMeasuredThenNativeOffsetsAndSizesMatch()
    {
        Assert.AreEqual((8 + IntPtr.Size, 8, 28, 12), (Marshal.SizeOf<MilBitmapSourceCommand>(), Marshal.OffsetOf<MilBitmapSourceCommand>(nameof(MilBitmapSourceCommand.Bitmap)).ToInt32(), Marshal.SizeOf<MilBitmapInvalidateCommand>(), Marshal.OffsetOf<MilBitmapInvalidateCommand>(nameof(MilBitmapInvalidateCommand.DirtyRect)).ToInt32()));
    }

    [TestMethod]
    public void WhenInvalidateIsWrittenThenGoldenBytesMatch()
    {
        CollectionAssert.AreEqual(new byte[] { 13, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 4, 0, 0, 0, 5, 0, 0, 0, 6, 0, 0, 0 }, GeneratedProtocolPacketWriter.WriteBitmapInvalidate(1, 2, new(3, 4, 5, 6)));
    }

    [TestMethod]
    public void WhenSourceIsWrittenThenNativeWidthGoldenBytesMatch()
    {
        byte[] expected = new byte[8 + IntPtr.Size];
        new byte[] { 12, 0, 0, 0, 1, 0, 0, 0, 4, 3, 2, 1 }.CopyTo(expected, 0);
        CollectionAssert.AreEqual(expected, GeneratedProtocolPacketWriter.WriteBitmapSource(1, 0x01020304));
    }

    [TestMethod]
    public void WhenSourceIsReplacedThenTransportAndPreviousReferencesAreReleased()
    {
        using BitmapStub first = new(); using BitmapStub second = new();
        var (table, router) = Context();
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(1, first.Instance));
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(1, second.Instance));
        bool replaced = Get(table).Bitmap == second.Instance && first.State->References == 0 && second.State->References == 1;
        _ = table.Delete(1, MilResourceType.BitmapSource);
        Assert.AreEqual((true, 0), (replaced, second.State->References));
    }

    [TestMethod]
    public void WhenSameSourceIsSentAgainThenOneOwnedReferenceRemains()
    {
        using BitmapStub bitmap = new(); var (table, router) = Context();
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(1, bitmap.Instance));
        bitmap.State->References++;
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(1, bitmap.Instance));
        int references = bitmap.State->References;
        _ = table.Delete(1, MilResourceType.BitmapSource);
        Assert.AreEqual((1, 0), (references, bitmap.State->References));
    }

    [TestMethod]
    public void WhenQueryFailsWithOutputThenOldSourceSurvivesAndBothTemporaryReferencesRelease()
    {
        using BitmapStub old = new(); using BitmapStub invalid = new(); var (table, router) = Context();
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(1, old.Instance));
        invalid.State->QueryResult = Direct3D9Factory.NoInterfaceHResult;
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(1, invalid.Instance));
        bool retained = Get(table).Bitmap == old.Instance;
        _ = table.Delete(1, MilResourceType.BitmapSource);
        Assert.AreEqual((Direct3D9Factory.NoInterfaceHResult, true, 0, 0), (result, retained, invalid.State->References, old.State->References));
    }

    [TestMethod]
    public void WhenNullSourceFailsThenOldSourceIsRetainedAndBatchStops()
    {
        using BitmapStub bitmap = new(); var (table, router) = Context();
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(1, bitmap.Instance));
        int result = router.ProcessPackets([GeneratedProtocolPacketWriter.WriteBitmapSource(1, 0), GeneratedProtocolPacketWriter.WriteBitmapInvalidate(1)]);
        var actual = (result, Get(table).Bitmap == bitmap.Instance, Get(table).ChangeCount, bitmap.State->DirtyCalls);
        _ = table.Delete(1, MilResourceType.BitmapSource);
        Assert.AreEqual((unchecked((int)0x80070006), true, 2, 0), actual);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(-1)]
    public void WhenInvalidatedThenNativeBoolAndRectangleAreForwarded(int useRectangle)
    {
        using BitmapStub bitmap = new(); var (table, router) = Context();
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(1, bitmap.Instance));
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapInvalidate(1, useRectangle, new(3, 4, 5, 6)));
        var actual = (bitmap.State->HasRectangle, bitmap.State->Rectangle, bitmap.State->DirtyCalls);
        _ = table.Delete(1, MilResourceType.BitmapSource);
        Assert.AreEqual((useRectangle != 0, useRectangle != 0 ? new MilRectL(3, 4, 5, 6) : default, 1), actual);
    }

    [TestMethod]
    public void WhenDirtyOperationFailsThenAllImageConsumersStillReceiveNotification()
    {
        using BitmapStub bitmap = new(); var (table, router) = Context();
        _ = router.ProcessPackets([GeneratedProtocolPacketWriter.WriteBitmapSource(1, bitmap.Instance), Create(2, MilResourceType.ImageBrush), Create(3, MilResourceType.ImageDrawing), Create(4, MilResourceType.RenderData),
            GeneratedProtocolPacketWriter.WriteTileBrush(MilCommand.ImageBrush, 2, 1, default, default, 0, 1, source: 1), GeneratedProtocolPacketWriter.WriteImageDrawing(3, default, 1), GeneratedProtocolPacketWriter.WriteRenderData(4, GeneratedProtocolPacketWriter.WriteDrawImageRecord(default, 1), GeneratedProtocolPacketWriter.WriteDrawImageRecord(default, 1))]);
        bitmap.State->DirtyResult = Direct3D9Factory.GenericFailureHResult;
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapInvalidate(1));
        var actual = (result, Resource(table, 2).ChangeCount, Resource(table, 3).ChangeCount, Resource(table, 4).ChangeCount);
        _ = table.Delete(1, MilResourceType.BitmapSource); _ = table.Delete(2, MilResourceType.ImageBrush); _ = table.Delete(3, MilResourceType.ImageDrawing); _ = table.Delete(4, MilResourceType.RenderData);
        Assert.AreEqual((Direct3D9Factory.GenericFailureHResult, 2, 2, 3, 0), (actual.result, actual.Item2, actual.Item3, actual.Item4, bitmap.State->References));
    }

    [TestMethod]
    public void WhenEmptyBitmapIsInvalidatedThenChangeIsNotified()
    {
        var (table, router) = Context();
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapInvalidate(1));
        Assert.AreEqual((0, 1), (result, Get(table).ChangeCount));
    }

    [TestMethod]
    public void WhenDuplicateOutlivesOriginalThenFinalDeleteReleasesBitmap()
    {
        using BitmapStub bitmap = new(); GeneratedProtocolHandleTable source = new(); GeneratedProtocolHandleTable target = new(); GeneratedProtocolChannelRegistry registry = new();
        _ = registry.TryAdd(1, source); _ = registry.TryAdd(2, target);
        GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, registry).CreateRouter();
        _ = router.ProcessPackets([Create(1, MilResourceType.BitmapSource), GeneratedProtocolPacketWriter.WriteBitmapSource(1, bitmap.Instance), GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(1, 2, 5)]);
        GeneratedBitmapSourceResource resource = Get(source);
        _ = source.Delete(1, MilResourceType.BitmapSource);
        bool retained = ReferenceEquals(resource, Resource(target, 5)) && bitmap.State->References == 1;
        _ = target.Delete(5, MilResourceType.BitmapSource);
        Assert.AreEqual((true, 0, true, (nint)0), (retained, bitmap.State->References, resource.IsReleased, resource.Bitmap));
    }

    [TestMethod]
    public void WhenPacketIsRejectedBeforeDispatchThenCallerStillOwnsTransportReference()
    {
        using BitmapStub bitmap = new(); var (table, router) = Context();
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(99, bitmap.Instance));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 1, (nint)0), (result, bitmap.State->References, Get(table).Bitmap));
    }

    [TestMethod]
    public void WhenMalformedPacketOrWrongTargetIsUsedThenNoNotificationOccurs()
    {
        var (table, router) = Context(); _ = router.ProcessPacket(Create(2, MilResourceType.DrawingImage));
        int shortPacket = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapInvalidate(1).AsSpan(0, 27));
        int wrongTarget = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapInvalidate(2));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 0), (shortPacket, wrongTarget, Get(table).ChangeCount));
    }

    [TestMethod]
    public void WhenQueryReturnsAdjustedInterfaceThenBitmapAddressIsRetained()
    {
        using BitmapStub transport = new(); using BitmapStub bitmap = new();
        transport.State->QueryOutput = bitmap.State;
        var (table, router) = Context();
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteBitmapSource(1, transport.Instance));
        var actual = (result, Get(table).Bitmap, transport.State->References, bitmap.State->References);
        _ = table.Delete(1, MilResourceType.BitmapSource);
        Assert.AreEqual((0, bitmap.Instance, 0, 2, 1), (actual.result, actual.Item2, actual.Item3, actual.Item4, bitmap.State->References));
    }

    [TestMethod]
    public void WhenReleasedResourceIsCalledThenTransportReferenceIsNotConsumed()
    {
        using BitmapStub bitmap = new(); var (table, _) = Context();
        GeneratedBitmapSourceResource resource = Get(table);
        _ = table.Delete(1, MilResourceType.BitmapSource);
        int result = resource.ProcessCommand(MilCommand.BitmapSource, GeneratedProtocolPacketWriter.WriteBitmapSource(1, bitmap.Instance));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 1), (result, bitmap.State->References));
    }

    private static byte[] Create(uint handle, MilResourceType type) => GeneratedProtocolPacketWriter.WriteChannelCreateResource(handle, type);
    private static GeneratedProtocolResource Resource(GeneratedProtocolHandleTable table, uint handle) { Assert.IsTrue(table.TryGetResource(handle, out GeneratedProtocolResource? value)); return value!; }
    private static GeneratedBitmapSourceResource Get(GeneratedProtocolHandleTable table) => (GeneratedBitmapSourceResource)Resource(table, 1);
    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter) Context() { GeneratedProtocolHandleTable table = new(); GeneratedProtocolChannelRegistry registry = new(); _ = registry.TryAdd(1, table); GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, registry).CreateRouter(); _ = router.ProcessPacket(Create(1, MilResourceType.BitmapSource)); return (table, router); }

    private struct BitmapState
    {
        internal void** Vtable;
        internal int References;
        internal int QueryResult;
        internal BitmapState* QueryOutput;
        internal int DirtyResult;
        internal int DirtyCalls;
        internal bool HasRectangle;
        internal MilRectL Rectangle;
    }

    private sealed class BitmapStub : IDisposable
    {
        internal BitmapState* State { get; }
        internal nint Instance => (nint)State;
        internal BitmapStub()
        {
            State = (BitmapState*)NativeMemory.AllocZeroed((nuint)sizeof(BitmapState));
            State->Vtable = (void**)NativeMemory.AllocZeroed(12, (nuint)sizeof(nint));
            State->References = 1;
            State->Vtable[0] = (delegate* unmanaged[Stdcall]<BitmapState*, Guid*, nint*, int>)&Query;
            State->Vtable[1] = (delegate* unmanaged[Stdcall]<BitmapState*, uint>)&AddRef;
            State->Vtable[2] = (delegate* unmanaged[Stdcall]<BitmapState*, uint>)&Release;
            State->Vtable[11] = (delegate* unmanaged[Stdcall]<BitmapState*, MilRectL*, int>)&Dirty;
        }
        public void Dispose() { NativeMemory.Free(State->Vtable); NativeMemory.Free(State); }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Query(BitmapState* state, Guid* id, nint* output)
    {
        *output = 0;
        if (*id != new Guid("C46D6FDE-0E59-4CFD-89B1-C935906DFBD9")) return Direct3D9Factory.NoInterfaceHResult;
        BitmapState* result = state->QueryOutput != null ? state->QueryOutput : state;
        result->References++; *output = (nint)result; return state->QueryResult;
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint AddRef(BitmapState* state) => (uint)++state->References;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(BitmapState* state) => (uint)--state->References;
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int Dirty(BitmapState* state, MilRectL* rectangle) { state->DirtyCalls++; state->HasRectangle = rectangle != null; state->Rectangle = rectangle != null ? *rectangle : default; return state->DirtyResult; }
}
