using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedBrushResourceTests
{
    [TestMethod]
    public void WhenBrushCommandLayoutsAreMeasuredThenNativeSizesAndOffsetsMatch()
    {
        Assert.AreEqual(
            (24, 48, 84, 108, 148, 36, 32, 72, 144),
            (Marshal.SizeOf<MilGradientStop>(), Marshal.SizeOf<MilSolidColorBrushCommand>(), Marshal.SizeOf<MilLinearGradientBrushCommand>(),
                Marshal.SizeOf<MilRadialGradientBrushCommand>(), Marshal.SizeOf<MilTileBrushCommand>(), Marshal.SizeOf<MilBitmapCacheBrushCommand>(),
                Marshal.OffsetOf<MilSolidColorBrushCommand>(nameof(MilSolidColorBrushCommand.OpacityAnimation)).ToInt32(),
                Marshal.OffsetOf<MilLinearGradientBrushCommand>(nameof(MilLinearGradientBrushCommand.GradientStopsSize)).ToInt32(),
                Marshal.OffsetOf<MilTileBrushCommand>(nameof(MilTileBrushCommand.Source)).ToInt32()));
    }

    [TestMethod]
    public void WhenBrushPacketsAreWrittenThenIdsFieldsAndGradientPayloadMatchNativeLayout()
    {
        MilGradientStop[] stops = [new(0.25, new MilColorF(1, 0.5f, 0.25f, 0.75f))];
        byte[] solid = GeneratedProtocolPacketWriter.WriteSolidColorBrush(1, 0.5, new MilColorF(1, 0.2f, 0.3f, 0.4f), 2, 3, 4, 5);
        byte[] linear = GeneratedProtocolPacketWriter.WriteLinearGradientBrush(6, 0.75, new MilPoint2D(1, 2), new MilPoint2D(3, 4), stops);
        byte[] tile = GeneratedProtocolPacketWriter.WriteTileBrush(MilCommand.ImageBrush, 7, 1, new MilRectD(1, 2, 3, 4), new MilRectD(5, 6, 7, 8), 0.5, 2, 9);

        Assert.AreEqual(
            (0x7Eu, 1u, 0.5, 2u, 5u, 0x7Fu, 24u, 0.25, 1f, 0x81u, 7u, 9u),
            (BitConverter.ToUInt32(solid, 0), BitConverter.ToUInt32(solid, 4), BitConverter.ToDouble(solid, 8), BitConverter.ToUInt32(solid, 32), BitConverter.ToUInt32(solid, 44),
                BitConverter.ToUInt32(linear, 0), BitConverter.ToUInt32(linear, 72), BitConverter.ToDouble(linear, 84), BitConverter.ToSingle(linear, 92),
                BitConverter.ToUInt32(tile, 0), BitConverter.ToUInt32(tile, 4), BitConverter.ToUInt32(tile, 144)));
    }

    [TestMethod]
    public void WhenFactoryCreatesBrushResourcesThenEachResourceHasItsStrongType()
    {
        GeneratedProtocolHandleTable table = new();
        MilResourceType[] types = [MilResourceType.SolidColorBrush, MilResourceType.LinearGradientBrush, MilResourceType.RadialGradientBrush, MilResourceType.ImageBrush, MilResourceType.DrawingBrush, MilResourceType.VisualBrush, MilResourceType.BitmapCacheBrush];
        foreach ((MilResourceType type, int index) in types.Select((type, index) => (type, index))) Assert.AreEqual(0, table.Create((uint)index + 1, type));
        Assert.AreEqual((true, true, true, true, true, true, true),
            (Get<GeneratedSolidColorBrushResource>(table, 1) is not null, Get<GeneratedLinearGradientBrushResource>(table, 2) is not null,
                Get<GeneratedRadialGradientBrushResource>(table, 3) is not null, Get<GeneratedImageBrushResource>(table, 4) is not null,
                Get<GeneratedDrawingBrushResource>(table, 5) is not null, Get<GeneratedVisualBrushResource>(table, 6) is not null,
                Get<GeneratedBitmapCacheBrushResource>(table, 7) is not null));
    }

    [TestMethod]
    public void WhenSolidAndGradientBrushesAreUpdatedThenValuesDependenciesAndStopsMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        MilGradientStop[] stops = [new(0, new MilColorF(1, 1, 0, 0)), new(1, new MilColorF(1, 0, 0, 1))];
        int result = router.ProcessPackets([
            Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.ColorResource), Create(3, MilResourceType.PointResource), Create(4, MilResourceType.TranslateTransform),
            Create(10, MilResourceType.SolidColorBrush), Create(11, MilResourceType.LinearGradientBrush), Create(12, MilResourceType.RadialGradientBrush),
            GeneratedProtocolPacketWriter.WriteSolidColorBrush(10, 0.5, new MilColorF(1, 0.2f, 0.3f, 0.4f), 1, 4, 4, 2),
            GeneratedProtocolPacketWriter.WriteLinearGradientBrush(11, 0.75, new MilPoint2D(1, 2), new MilPoint2D(3, 4), stops, 1, 4, 4, startAnimation: 3, endAnimation: 3),
            GeneratedProtocolPacketWriter.WriteRadialGradientBrush(12, 1, new MilPoint2D(5, 6), 7, 8, new MilPoint2D(9, 10), stops, 1, 4, 4, centerAnimation: 3, radiusXAnimation: 1, radiusYAnimation: 1, originAnimation: 3)
        ]);

        Assert.AreEqual((0, (0.5, new MilColorF(1, 0.2f, 0.3f, 0.4f)), 4, 2, 2),
            (result, Get<GeneratedSolidColorBrushResource>(table, 10).Value, Get<GeneratedSolidColorBrushResource>(table, 10).Dependencies.Count,
                Get<GeneratedLinearGradientBrushResource>(table, 11).Stops.Length, Get<GeneratedRadialGradientBrushResource>(table, 12).Stops.Length));
    }

    [TestMethod]
    public void WhenTileBrushesAreUpdatedThenSourceFamiliesAndTileFieldsMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        int result = router.ProcessPackets([
            Create(1, MilResourceType.DrawingImage), Create(2, MilResourceType.GeometryDrawing), Create(3, MilResourceType.Visual), Create(4, MilResourceType.RectResource),
            Create(10, MilResourceType.ImageBrush), Create(11, MilResourceType.DrawingBrush), Create(12, MilResourceType.VisualBrush),
            Tile(MilCommand.ImageBrush, 10, 1, 4, MilTileMode.Tile), Tile(MilCommand.DrawingBrush, 11, 2, 4, MilTileMode.FlipXY), Tile(MilCommand.VisualBrush, 12, 3, 4, MilTileMode.Extend)
        ]);

        Assert.AreEqual((0, MilTileMode.Tile, true, MilTileMode.FlipXY, true, MilTileMode.Extend, true),
            (result, Get<GeneratedImageBrushResource>(table, 10).TileMode, Get<GeneratedImageBrushResource>(table, 10).Source?.ResourceType == MilResourceType.DrawingImage,
                Get<GeneratedDrawingBrushResource>(table, 11).TileMode, Get<GeneratedDrawingBrushResource>(table, 11).Source?.ResourceType == MilResourceType.GeometryDrawing,
                Get<GeneratedVisualBrushResource>(table, 12).TileMode, Get<GeneratedVisualBrushResource>(table, 12).Source?.ResourceType == MilResourceType.Visual));
    }

    [TestMethod]
    public void WhenBitmapCacheBrushIsUpdatedThenCacheAndVisualDependenciesMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        int result = router.ProcessPackets([Create(1, MilResourceType.BitmapCache), Create(2, MilResourceType.Visual), Create(3, MilResourceType.BitmapCacheBrush), GeneratedProtocolPacketWriter.WriteBitmapCacheBrush(3, 1, bitmapCache: 1, internalTarget: 2)]);
        Assert.AreEqual((0, MilResourceType.BitmapCache, MilResourceType.Visual), (result, Get<GeneratedBitmapCacheBrushResource>(table, 3).BitmapCache?.ResourceType, Get<GeneratedBitmapCacheBrushResource>(table, 3).InternalTarget?.ResourceType));
    }

    [TestMethod]
    public void WhenDependencyTypeOrBrushEnumIsInvalidThenExistingBrushRemainsUnchanged()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets([Create(1, MilResourceType.ColorResource), Create(2, MilResourceType.ImageBrush), GeneratedProtocolPacketWriter.WriteTileBrush(MilCommand.ImageBrush, 2, 1, default, default, 0, 1)]);
        GeneratedImageBrushResource brush = Get<GeneratedImageBrushResource>(table, 2); int count = brush.ChangeCount;
        int wrongSource = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteTileBrush(MilCommand.ImageBrush, 2, 1, default, default, 0, 1, source: 1));
        byte[] wrongEnum = GeneratedProtocolPacketWriter.WriteTileBrush(MilCommand.ImageBrush, 2, 1, default, default, 0, 1); BitConverter.GetBytes(99u).CopyTo(wrongEnum, 128);
        int wrongEnumResult = router.ProcessPacket(wrongEnum);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, count, MilTileMode.None), (wrongSource, wrongEnumResult, brush.ChangeCount, brush.TileMode));
    }

    [TestMethod]
    public void WhenGradientPayloadLengthOrAlignmentIsInvalidThenOldStopsRemainUnchanged()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext(); MilGradientStop[] stops = [new(0, new MilColorF(1, 1, 1, 1))];
        _ = router.ProcessPackets([Create(1, MilResourceType.LinearGradientBrush), GeneratedProtocolPacketWriter.WriteLinearGradientBrush(1, 1, default, default, stops)]);
        GeneratedLinearGradientBrushResource brush = Get<GeneratedLinearGradientBrushResource>(table, 1); int count = brush.ChangeCount;
        byte[] mismatch = GeneratedProtocolPacketWriter.WriteLinearGradientBrush(1, 1, default, default, stops); BitConverter.GetBytes(48u).CopyTo(mismatch, 72);
        byte[] unaligned = [.. GeneratedProtocolPacketWriter.WriteLinearGradientBrush(1, 1, default, default, stops), 0]; BitConverter.GetBytes(25u).CopyTo(unaligned, 72);
        int first = router.ProcessPacket(mismatch); int second = router.ProcessPacket(unaligned);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 1, count), (first, second, brush.Stops.Length, brush.ChangeCount));
    }

    [TestMethod]
    public void WhenBrushDependencyChangesThenNotificationPropagates()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.SolidColorBrush), GeneratedProtocolPacketWriter.WriteSolidColorBrush(2, 1, default, opacityAnimation: 1)]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(1, 0.25));
        Assert.AreEqual((0, 2), (result, Get<GeneratedSolidColorBrushResource>(table, 2).ChangeCount));
    }

    [TestMethod]
    public void WhenBrushIsDuplicatedAndLastHandleDeletedThenDependenciesReleaseDeterministically()
    {
        GeneratedProtocolHandleTable source = new(); GeneratedProtocolHandleTable target = new(); GeneratedProtocolChannelRegistry channels = new(); _ = channels.TryAdd(1, source); _ = channels.TryAdd(2, target); GeneratedProtocolRouter router = new GeneratedProtocolProductionContext(1, channels).CreateRouter();
        _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.SolidColorBrush), GeneratedProtocolPacketWriter.WriteSolidColorBrush(2, 1, default, opacityAnimation: 1), GeneratedProtocolPacketWriter.WriteChannelDuplicateHandle(2, 2, 20)]);
        GeneratedProtocolResource dependency = Get<GeneratedProtocolResource>(source, 1); GeneratedProtocolResource brush = Get<GeneratedProtocolResource>(source, 2);
        int a = source.Delete(2, MilResourceType.SolidColorBrush); int b = source.Delete(1, MilResourceType.DoubleResource); int c = target.Delete(20, MilResourceType.SolidColorBrush);
        Assert.AreEqual((0, 0, 0, 0, 0, true), (a, b, c, dependency.ReferenceCount, brush.ReferenceCount, brush.IsReleased));
    }

    [TestMethod]
    public void WhenBrushPacketSequenceFailsThenLaterUpdatesAreNotProcessed()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = CreateContext();
        int result = router.ProcessPackets([Create(1, MilResourceType.SolidColorBrush), GeneratedProtocolPacketWriter.WriteSolidColorBrush(1, 1, default, transform: 99), GeneratedProtocolPacketWriter.WriteSolidColorBrush(1, 0.5, default)]);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, default((double, MilColorF)), 0), (result, Get<GeneratedSolidColorBrushResource>(table, 1).Value, Get<GeneratedSolidColorBrushResource>(table, 1).ChangeCount));
    }

    private static byte[] Create(uint handle, MilResourceType type) => GeneratedProtocolPacketWriter.WriteChannelCreateResource(handle, type);
    private static byte[] Tile(MilCommand type, uint handle, uint source, uint rectAnimation, MilTileMode tileMode) => GeneratedProtocolPacketWriter.WriteTileBrush(type, handle, 1, default, default, 0, 1, source, viewportAnimation: rectAnimation, viewboxAnimation: rectAnimation, tileMode: tileMode);
    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter) CreateContext() { GeneratedProtocolHandleTable table = new(); GeneratedProtocolChannelRegistry channels = new(); _ = channels.TryAdd(1, table); return (table, new GeneratedProtocolProductionContext(1, channels).CreateRouter()); }
    private static T Get<T>(GeneratedProtocolHandleTable table, uint handle) where T : GeneratedProtocolResource { Assert.IsTrue(table.TryGetResource(handle, out GeneratedProtocolResource? resource)); return (T)resource!; }
}
