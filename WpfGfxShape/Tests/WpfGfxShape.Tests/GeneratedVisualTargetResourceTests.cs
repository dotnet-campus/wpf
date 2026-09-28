using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedVisualTargetResourceTests
{
    [TestMethod]
    public void WhenLayoutsAreMeasuredThenNativeSizesMatch()
    {
        Assert.AreEqual((20, 32, 8, 24, 12, 16, 16, 12, 24), (Marshal.SizeOf<MilGuidelineSetCommand>(), Marshal.SizeOf<MilBitmapCacheCommand>(), Marshal.SizeOf<MilVisualCreateCommand>(), Marshal.SizeOf<MilVisualSetOffsetCommand>(), Marshal.SizeOf<MilVisualDependencyCommand>(), Marshal.SizeOf<MilVisualInsertChildAtCommand>(), Marshal.SizeOf<MilVisualSetAlphaCommand>(), Marshal.SizeOf<MilTargetSetRootCommand>(), Marshal.SizeOf<MilTargetInvalidateCommand>()));
    }

    [TestMethod]
    public void WhenGuidelineSetIsUpdatedThenDynamicPayloadIsSeparated()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        int result = router.ProcessPackets([Create(1, MilResourceType.GuidelineSet), GeneratedProtocolPacketWriter.WriteGuidelineSet(1, [1d, 2d], [3d], true)]);
        GeneratedGuidelineSetResource resource = Get<GeneratedGuidelineSetResource>(table, 1);
        Assert.AreEqual((0, 2, 1, true, 1d, 3d), (result, resource.GuidelinesX.Length, resource.GuidelinesY.Length, resource.IsDynamic, resource.GuidelinesX[0], resource.GuidelinesY[0]));
    }

    [TestMethod]
    public void WhenGuidelinePayloadIsMalformedThenOldStateRemains()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.GuidelineSet), GeneratedProtocolPacketWriter.WriteGuidelineSet(1, [1d], [2d], false)]); GeneratedGuidelineSetResource resource = Get<GeneratedGuidelineSetResource>(table, 1); int count = resource.ChangeCount;
        byte[] malformed = GeneratedProtocolPacketWriter.WriteGuidelineSet(1, [3d], [4d], true); BitConverter.GetBytes(9u).CopyTo(malformed, 8);
        int result = router.ProcessPacket(malformed);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 1d, 2d, false, count), (result, resource.GuidelinesX[0], resource.GuidelinesY[0], resource.IsDynamic, resource.ChangeCount));
    }

    [TestMethod]
    public void WhenBitmapCacheAnimationChangesThenNotificationPropagates()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); int result = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.BitmapCache), GeneratedProtocolPacketWriter.WriteBitmapCache(2, 1.5, 1, true, true), GeneratedProtocolPacketWriter.WriteDoubleResource(1, 2)]); GeneratedBitmapCacheResource cache = Get<GeneratedBitmapCacheResource>(table, 2);
        Assert.AreEqual((0, 1.5, true, true, 2), (result, cache.RenderAtScale, cache.SnapsToDevicePixels, cache.EnableClearType, cache.ChangeCount));
    }

    [TestMethod]
    public void WhenVisualPropertiesAreUpdatedThenStrongStateAndReferencesMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); int result = router.ProcessPackets([Create(1, MilResourceType.Visual), Create(2, MilResourceType.TranslateTransform), Create(3, MilResourceType.LineGeometry), Create(4, MilResourceType.GeometryDrawing), Create(5, MilResourceType.SolidColorBrush), GeneratedProtocolPacketWriter.WriteVisualCreate(1), GeneratedProtocolPacketWriter.WriteVisualSetOffset(1, 2, 3), GeneratedProtocolPacketWriter.WriteVisualSetAlpha(1, 0.5), GeneratedProtocolPacketWriter.WriteVisualSetTransform(1, 2), GeneratedProtocolPacketWriter.WriteVisualSetClip(1, 3), GeneratedProtocolPacketWriter.WriteVisualSetContent(1, 4), GeneratedProtocolPacketWriter.WriteVisualSetAlphaMask(1, 5)]); GeneratedVisualResource visual = Get<GeneratedVisualResource>(table, 1);
        Assert.AreEqual((0, new MilPoint2D(2, 3), 0.5, MilResourceType.TranslateTransform, MilResourceType.LineGeometry, MilResourceType.GeometryDrawing, MilResourceType.SolidColorBrush), (result, visual.Offset, visual.Alpha, visual.Transform!.ResourceType, visual.Clip!.ResourceType, visual.Content!.ResourceType, visual.AlphaMask!.ResourceType));
    }

    [TestMethod]
    public void WhenVisualDependencyTypeIsInvalidThenReplacementIsTransactional()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Visual), Create(2, MilResourceType.TranslateTransform), Create(3, MilResourceType.ColorResource), GeneratedProtocolPacketWriter.WriteVisualSetTransform(1, 2)]); GeneratedVisualResource visual = Get<GeneratedVisualResource>(table, 1); int count = visual.ChangeCount;
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteVisualSetTransform(1, 3));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, MilResourceType.TranslateTransform, count), (result, visual.Transform!.ResourceType, visual.ChangeCount));
    }

    [TestMethod]
    public void WhenChildrenAreMutatedThenParentIdentityAndTransactionsMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Visual), Create(2, MilResourceType.Visual), Create(3, MilResourceType.Visual), GeneratedProtocolPacketWriter.WriteVisualInsertChildAt(1, 2, 0)]); GeneratedVisualResource parent = Get<GeneratedVisualResource>(table, 1); GeneratedVisualResource child = Get<GeneratedVisualResource>(table, 2); int count = parent.ChangeCount;
        int duplicate = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteVisualInsertChildAt(3, 2, 0)); int index = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteVisualInsertChildAt(1, 3, 9)); int remove = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteVisualRemoveChild(1, 2));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Direct3D9Factory.UceMalformedPacketHResult, 0, 0, null, count + 1), (duplicate, index, remove, parent.Children.Count, child.Parent, parent.ChangeCount));
    }

    [TestMethod]
    public void WhenTargetCommandsAreProcessedThenStateAndInvalidationMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); MilColorF color = new(1, 0.1f, 0.2f, 0.3f); MilRectL rect = new(1, 2, 3, 4); int result = router.ProcessPackets([Create(1, MilResourceType.HwndRenderTarget), Create(2, MilResourceType.Visual), GeneratedProtocolPacketWriter.WriteTargetSetRoot(1, 2), GeneratedProtocolPacketWriter.WriteTargetSetClearColor(1, color), GeneratedProtocolPacketWriter.WriteTargetInvalidate(1, rect), GeneratedProtocolPacketWriter.WriteTargetSetFlags(1, MilRenderTargetInitializationFlags.HardwareOnly | MilRenderTargetInitializationFlags.DisableDirtyRectangles)]); GeneratedTargetResource target = Get<GeneratedTargetResource>(table, 1);
        Assert.AreEqual((0, MilResourceType.Visual, color, rect, 1, MilRenderTargetInitializationFlags.HardwareOnly | MilRenderTargetInitializationFlags.DisableDirtyRectangles), (result, target.Root!.ResourceType, target.ClearColor, target.InvalidatedRect, target.InvalidationCount, target.Flags));
    }

    [TestMethod]
    public void WhenTargetFlagsAreInvalidThenOldFlagsRemain()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.HwndRenderTarget), GeneratedProtocolPacketWriter.WriteTargetSetFlags(1, MilRenderTargetInitializationFlags.SoftwareOnly)]); GeneratedTargetResource target = Get<GeneratedTargetResource>(table, 1); int count = target.ChangeCount;
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteTargetSetFlags(1, (MilRenderTargetInitializationFlags)0x10));
        Assert.AreEqual((Direct3D9Factory.InvalidArgumentHResult, MilRenderTargetInitializationFlags.SoftwareOnly, count), (result, target.Flags, target.ChangeCount));
    }

    [TestMethod]
    public void WhenVisualAndTargetAreDeletedThenDependenciesReleaseDeterministically()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Visual), Create(2, MilResourceType.Visual), Create(3, MilResourceType.HwndRenderTarget), GeneratedProtocolPacketWriter.WriteVisualInsertChildAt(1, 2, 0), GeneratedProtocolPacketWriter.WriteTargetSetRoot(3, 1)]); GeneratedProtocolResource parent = Get<GeneratedProtocolResource>(table, 1); GeneratedProtocolResource child = Get<GeneratedProtocolResource>(table, 2);
        int a = table.Delete(3, MilResourceType.HwndRenderTarget); int b = table.Delete(1, MilResourceType.Visual); int c = table.Delete(2, MilResourceType.Visual);
        Assert.AreEqual((0, 0, 0, 0, 0, true, true), (a, b, c, parent.ReferenceCount, child.ReferenceCount, parent.IsReleased, child.IsReleased));
    }

    private static byte[] Create(uint handle, MilResourceType type) => GeneratedProtocolPacketWriter.WriteChannelCreateResource(handle, type);
    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter) Context() { GeneratedProtocolHandleTable table = new(); GeneratedProtocolChannelRegistry registry = new(); _ = registry.TryAdd(1, table); return (table, new GeneratedProtocolProductionContext(1, registry).CreateRouter()); }
    private static T Get<T>(GeneratedProtocolHandleTable table, uint handle) where T : GeneratedProtocolResource { Assert.IsTrue(table.TryGetResource(handle, out GeneratedProtocolResource? resource)); return (T)resource!; }
}
