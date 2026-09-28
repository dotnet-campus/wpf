using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedRenderDataVisual3DResourceTests
{
    [TestMethod]
    public void WhenLayoutsAreMeasuredThenNativeSizesMatch()
    {
        Assert.AreEqual((12, 40, 48, 40, 48, 56, 72, 40, 56, 16, 40, 40, 24, 12, 40, 16), (Marshal.SizeOf<MilRenderDataCommand>(), Marshal.SizeOf<MilDrawLineData>(), Marshal.SizeOf<MilDrawLineAnimateData>(), Marshal.SizeOf<MilDrawRectangleData>(), Marshal.SizeOf<MilDrawRectangleAnimateData>(), Marshal.SizeOf<MilDrawRoundedRectangleData>(), Marshal.SizeOf<MilDrawRoundedRectangleAnimateData>(), Marshal.SizeOf<MilDrawEllipseData>(), Marshal.SizeOf<MilDrawEllipseAnimateData>(), Marshal.SizeOf<MilDrawGeometryData>(), Marshal.SizeOf<MilDrawImageData>(), Marshal.SizeOf<MilDrawImageAnimateData>(), Marshal.SizeOf<MilPushOpacityMaskData>(), Marshal.SizeOf<MilVisual3DDependencyCommand>(), Marshal.SizeOf<MilViewport3DSetViewportCommand>(), Marshal.SizeOf<MilVisual3DInsertCommand>()));
    }

    [TestMethod]
    public void WhenRenderDataIsUpdatedThenTypedInstructionsAndDependenciesMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        int result = router.ProcessPackets([Create(1, MilResourceType.Pen), Create(2, MilResourceType.SolidColorBrush), Create(3, MilResourceType.LineGeometry), Create(4, MilResourceType.DrawingImage), Create(5, MilResourceType.GuidelineSet), Create(10, MilResourceType.RenderData), GeneratedProtocolPacketWriter.WriteRenderData(10, GeneratedProtocolPacketWriter.WriteDrawLineRecord(new(1, 2), new(3, 4), 1), GeneratedProtocolPacketWriter.WriteDrawRectangleRecord(new(1, 2, 3, 4), 2, 1), GeneratedProtocolPacketWriter.WriteDrawGeometryRecord(2, 1, 3), GeneratedProtocolPacketWriter.WriteDrawImageRecord(new(5, 6, 7, 8), 4), GeneratedProtocolPacketWriter.WritePushGuidelineSetRecord(5), GeneratedProtocolPacketWriter.WritePopRecord())]);
        GeneratedRenderDataResource renderData = Get<GeneratedRenderDataResource>(table, 10);
        Assert.AreEqual((0, 6, GeneratedRenderDataKind.DrawLine, GeneratedRenderDataKind.Pop, MilResourceType.Pen), (result, renderData.Instructions.Count, renderData.Instructions[0].Kind, renderData.Instructions[^1].Kind, renderData.Instructions[0].Resources[0].ResourceType));
    }

    [TestMethod]
    public void WhenAnimatedRenderDataIsUpdatedThenAllRemainingInstructionsMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        int result = router.ProcessPackets([Create(1, MilResourceType.Pen), Create(2, MilResourceType.SolidColorBrush), Create(3, MilResourceType.PointResource), Create(4, MilResourceType.RectResource), Create(5, MilResourceType.DoubleResource), Create(6, MilResourceType.DrawingImage), Create(7, MilResourceType.MediaPlayer), Create(8, MilResourceType.RenderData), GeneratedProtocolPacketWriter.WriteRenderData(8, GeneratedProtocolPacketWriter.WriteDrawLineAnimateRecord(new(), new(), 1, 3, 3), GeneratedProtocolPacketWriter.WriteDrawRectangleAnimateRecord(new(), 2, 1, 4), GeneratedProtocolPacketWriter.WriteDrawRoundedRectangleRecord(new(), 1, 2, 2, 1), GeneratedProtocolPacketWriter.WriteDrawRoundedRectangleAnimateRecord(new(), 1, 2, 2, 1, 4, 5, 5), GeneratedProtocolPacketWriter.WriteDrawEllipseRecord(new(), 1, 2, 2, 1), GeneratedProtocolPacketWriter.WriteDrawEllipseAnimateRecord(new(), 1, 2, 2, 1, 3, 5, 5), GeneratedProtocolPacketWriter.WriteDrawImageAnimateRecord(new(), 6, 4), GeneratedProtocolPacketWriter.WriteDrawVideoAnimateRecord(new(), 7, 4))]);
        GeneratedRenderDataResource renderData = Get<GeneratedRenderDataResource>(table, 8);
        Assert.AreEqual((0, 8, GeneratedRenderDataKind.DrawLineAnimate, GeneratedRenderDataKind.DrawVideoAnimate, 2), (result, renderData.Instructions.Count, renderData.Instructions[0].Kind, renderData.Instructions[^1].Kind, renderData.Instructions[^1].Resources.Count));
    }

    [TestMethod]
    public void WhenAnimatedEllipseHasNullDependenciesThenSlotsAndGeometryAreRetained()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        int result = router.ProcessPackets([Create(1, MilResourceType.SolidColorBrush), Create(2, MilResourceType.RenderData), GeneratedProtocolPacketWriter.WriteRenderData(2,
            GeneratedProtocolPacketWriter.WritePushOpacityRecord(0.5),
            GeneratedProtocolPacketWriter.WriteDrawEllipseAnimateRecord(new(3, 4), 5, 6, 1, 0, 0, 0, 0),
            GeneratedProtocolPacketWriter.WritePopRecord())]);
        GeneratedRenderDataInstruction instruction = Get<GeneratedRenderDataResource>(table, 2).Instructions[1];
        Assert.AreEqual((0, 5, true, 3.0, 6.0), (result, instruction.ResourceSlots.Count, instruction.ResourceSlots[1] is null,
            BitConverter.ToDouble(instruction.Data.Span), BitConverter.ToDouble(instruction.Data.Span[24..])));
    }

    [TestMethod]
    public void WhenInputPacketIsChangedThenCommittedRenderDataIsUnchanged()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        byte[] packet = GeneratedProtocolPacketWriter.WriteRenderData(1, GeneratedProtocolPacketWriter.WriteDrawEllipseRecord(new(3, 4), 5, 6, 0, 0));
        _ = router.ProcessPackets([Create(1, MilResourceType.RenderData), packet]);
        Array.Clear(packet);
        Assert.AreEqual(3.0, BitConverter.ToDouble(Get<GeneratedRenderDataResource>(table, 1).Instructions[0].Data.Span));
    }

    [TestMethod]
    public void WhenAnimationFamilyIsWrongThenPreviousStreamAndReferencesRemain()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.ColorResource), Create(3, MilResourceType.RenderData),
            GeneratedProtocolPacketWriter.WriteRenderData(3, GeneratedProtocolPacketWriter.WriteDrawEllipseAnimateRecord(new(), 1, 2, 0, 0, 0, 1, 0))]);
        GeneratedRenderDataInstruction previous = Get<GeneratedRenderDataResource>(table, 3).Instructions[0];
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteRenderData(3,
            GeneratedProtocolPacketWriter.WriteDrawEllipseAnimateRecord(new(), 9, 10, 0, 0, 0, 2, 0)));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, previous, 2, 1),
            (result, Get<GeneratedRenderDataResource>(table, 3).Instructions[0], Get<GeneratedValueResource<double>>(table, 1).ReferenceCount, Get<GeneratedValueResource<MilColorF>>(table, 2).ReferenceCount));
    }

    [TestMethod]
    public void WhenAnimationDependencyIsRepeatedThenFinalDeleteReleasesEveryReference()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.RenderData),
            GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WriteDrawEllipseAnimateRecord(new(), 1, 2, 0, 0, 0, 1, 1))]);
        GeneratedProtocolResource animation = Get<GeneratedValueResource<double>>(table, 1);
        _ = table.Delete(1, MilResourceType.DoubleResource);
        _ = table.Delete(2, MilResourceType.RenderData);
        Assert.AreEqual((0, true), (animation.ReferenceCount, animation.IsReleased));
    }

    [TestMethod]
    public void WhenOpacityAnimationIsWrittenThenGoldenBytesMatch()
    {
        byte[] expected = [24, 0, 0, 0, 0x50, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0xE0, 0x3F, 4, 3, 2, 1, 0, 0, 0, 0];
        CollectionAssert.AreEqual(expected, GeneratedProtocolPacketWriter.WritePushOpacityAnimateRecord(0.5, 0x01020304));
    }

    [TestMethod]
    public void WhenOpacityAnimationChangesThenBalancedStreamIsNotified()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.RenderData),
            GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WritePushOpacityAnimateRecord(0.5, 1), GeneratedProtocolPacketWriter.WritePopRecord())]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(1, 0.75));
        Assert.AreEqual((0, 2, GeneratedRenderDataKind.PushOpacityAnimate),
            (result, Get<GeneratedRenderDataResource>(table, 2).ChangeCount, Get<GeneratedRenderDataResource>(table, 2).Instructions[0].Kind));
    }

    [TestMethod]
    public void WhenOpacityAnimationPushIsUnbalancedThenOldStreamRemains()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.RenderData), GeneratedProtocolPacketWriter.WriteRenderData(1)]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteRenderData(1, GeneratedProtocolPacketWriter.WritePushOpacityAnimateRecord(0.5, 0)));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 0), (result, Get<GeneratedRenderDataResource>(table, 1).Instructions.Count));
    }

    [TestMethod]
    public void WhenRenderDataRecordOrStackIsMalformedThenOldStreamRemains()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Pen), Create(2, MilResourceType.RenderData), GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WriteDrawLineRecord(new(), new(), 1))]); GeneratedRenderDataResource resource = Get<GeneratedRenderDataResource>(table, 2); int count = resource.ChangeCount;
        byte[] badSize = GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WriteDrawLineRecord(new(), new(), 1)); BitConverter.GetBytes(7).CopyTo(badSize, 12);
        int first = router.ProcessPacket(badSize); int second = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WritePushOpacityRecord(0.5)));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 1, count), (first, second, resource.Instructions.Count, resource.ChangeCount));
    }

    [TestMethod]
    public void WhenRenderDataDependencyTypeIsInvalidThenReplacementIsTransactional()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Pen), Create(2, MilResourceType.ColorResource), Create(3, MilResourceType.RenderData), GeneratedProtocolPacketWriter.WriteRenderData(3, GeneratedProtocolPacketWriter.WriteDrawLineRecord(new(), new(), 1))]); GeneratedRenderDataResource resource = Get<GeneratedRenderDataResource>(table, 3); int count = resource.ChangeCount;
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteRenderData(3, GeneratedProtocolPacketWriter.WriteDrawLineRecord(new(), new(), 2)));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, MilResourceType.Pen, count), (result, resource.Instructions[0].Resources[0].ResourceType, resource.ChangeCount));
    }

    [TestMethod]
    public void WhenRenderDataDependencyChangesThenNotificationPropagates()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.DashStyle), Create(3, MilResourceType.Pen), Create(4, MilResourceType.RenderData), GeneratedProtocolPacketWriter.WriteDashStyle(2, 0, [1d], 1), GeneratedProtocolPacketWriter.WritePen(3, 1, 1, dashStyle: 2), GeneratedProtocolPacketWriter.WriteRenderData(4, GeneratedProtocolPacketWriter.WriteDrawLineRecord(new(), new(), 3))]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(1, 2));
        Assert.AreEqual((0, 2, 2, 2), (result, Get<GeneratedDashStyleResource>(table, 2).ChangeCount, Get<GeneratedPenResource>(table, 3).ChangeCount, Get<GeneratedRenderDataResource>(table, 4).ChangeCount));
    }

    [TestMethod]
    public void WhenVisual3DPropertiesAreUpdatedThenStrongStateMatches()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); int result = router.ProcessPackets([Create(1, MilResourceType.Visual3D), Create(2, MilResourceType.Model3DGroup), Create(3, MilResourceType.TranslateTransform3D), GeneratedProtocolPacketWriter.WriteVisual3DSetContent(1, 2), GeneratedProtocolPacketWriter.WriteVisual3DSetTransform(1, 3)]); GeneratedVisual3DResource visual = Get<GeneratedVisual3DResource>(table, 1);
        Assert.AreEqual((0, MilResourceType.Model3DGroup, MilResourceType.TranslateTransform3D), (result, visual.Content!.ResourceType, visual.Transform!.ResourceType));
    }

    [TestMethod]
    public void WhenVisual3DChildrenAreMutatedThenParentIdentityIsTransactional()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Visual3D), Create(2, MilResourceType.Visual3D), Create(3, MilResourceType.Visual3D), GeneratedProtocolPacketWriter.WriteVisual3DInsertChild(1, 2, 0)]); GeneratedVisual3DResource parent = Get<GeneratedVisual3DResource>(table, 1); GeneratedVisual3DResource child = Get<GeneratedVisual3DResource>(table, 2); int count = parent.ChangeCount;
        int crossParent = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteVisual3DInsertChild(3, 2, 0)); int invalidIndex = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteVisual3DInsertChild(1, 3, 9)); int remove = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteVisual3DRemoveChild(1, 2));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 0, 0, null, count + 1), (crossParent, invalidIndex, remove, parent.Children.Count, child.Parent, parent.ChangeCount));
    }

    [TestMethod]
    public void WhenViewport3DStateIsUpdatedThenCrossDimensionIdentityMatches()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); MilRectD viewport = new(1, 2, 3, 4); int result = router.ProcessPackets([Create(1, MilResourceType.Viewport3DVisual), Create(2, MilResourceType.PerspectiveCamera), Create(3, MilResourceType.Visual3D), GeneratedProtocolPacketWriter.WriteViewport3DSetCamera(1, 2), GeneratedProtocolPacketWriter.WriteViewport3DSetViewport(1, viewport), GeneratedProtocolPacketWriter.WriteViewport3DSetChild(1, 3)]); GeneratedViewport3DVisualResource visual = Get<GeneratedViewport3DVisualResource>(table, 1); GeneratedVisual3DResource child = Get<GeneratedVisual3DResource>(table, 3);
        Assert.AreEqual((0, MilResourceType.PerspectiveCamera, viewport, true), (result, visual.Camera!.ResourceType, visual.Viewport, ReferenceEquals(child.Parent, visual)));
    }

    [TestMethod]
    public void WhenViewportChildAlreadyHasParentThenUpdateIsRejected()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Viewport3DVisual), Create(2, MilResourceType.Viewport3DVisual), Create(3, MilResourceType.Visual3D), GeneratedProtocolPacketWriter.WriteViewport3DSetChild(1, 3)]); GeneratedViewport3DVisualResource second = Get<GeneratedViewport3DVisualResource>(table, 2); int count = second.ChangeCount;
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteViewport3DSetChild(2, 3));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, null, count), (result, second.Child, second.ChangeCount));
    }

    [TestMethod]
    public void WhenRenderDataAnd3DVisualsAreDeletedThenReferencesReleaseDeterministically()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Pen), Create(2, MilResourceType.RenderData), Create(3, MilResourceType.Viewport3DVisual), Create(4, MilResourceType.Visual3D), GeneratedProtocolPacketWriter.WriteRenderData(2, GeneratedProtocolPacketWriter.WriteDrawLineRecord(new(), new(), 1)), GeneratedProtocolPacketWriter.WriteViewport3DSetChild(3, 4)]); GeneratedProtocolResource pen = Get<GeneratedProtocolResource>(table, 1); GeneratedProtocolResource child = Get<GeneratedProtocolResource>(table, 4);
        int a = table.Delete(2, MilResourceType.RenderData); int b = table.Delete(1, MilResourceType.Pen); int c = table.Delete(3, MilResourceType.Viewport3DVisual); int d = table.Delete(4, MilResourceType.Visual3D);
        Assert.AreEqual((0, 0, 0, 0, 0, 0, true, true), (a, b, c, d, pen.ReferenceCount, child.ReferenceCount, pen.IsReleased, child.IsReleased));
    }

    private static byte[] Create(uint handle, MilResourceType type) => GeneratedProtocolPacketWriter.WriteChannelCreateResource(handle, type);
    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter) Context() { GeneratedProtocolHandleTable table = new(); GeneratedProtocolChannelRegistry registry = new(); _ = registry.TryAdd(1, table); return (table, new GeneratedProtocolProductionContext(1, registry).CreateRouter()); }
    private static T Get<T>(GeneratedProtocolHandleTable table, uint handle) where T : GeneratedProtocolResource { Assert.IsTrue(table.TryGetResource(handle, out GeneratedProtocolResource? resource)); return (T)resource!; }
}
