using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

public sealed partial class Direct3D9SurfaceRenderTargetTests
{
    [TestMethod]
    public unsafe void WhenWindowRectangleHasNoBrushOrPenThenNoContentIsSubmitted()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget target = new(device, Silk.NET.Direct3D9.MultisampleType.MultisampleNone);
        var rectangle = new GeneratedRenderDataInstruction(GeneratedRenderDataKind.DrawRectangle, [], new byte[32], [null, null]);

        int result = GeneratedHardwareVisualRenderer.Draw([rectangle], device, target,
            GeneratedImageTransform.Identity, new(0, 0, 24, 16), 1, MilCompositingMode.SourceOver);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public unsafe void WhenWindowRoundedRectangleHasNoBrushOrPenThenNoContentIsSubmitted()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget target = new(device, Silk.NET.Direct3D9.MultisampleType.MultisampleNone);
        var rectangle = new GeneratedRenderDataInstruction(GeneratedRenderDataKind.DrawRoundedRectangle, [], new byte[48], [null, null]);

        int result = GeneratedHardwareVisualRenderer.Draw([rectangle], device, target,
            GeneratedImageTransform.Identity, new(0, 0, 24, 16), 1, MilCompositingMode.SourceOver);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public unsafe void WhenWindowRoundedRectanglePacketIsTruncatedThenMalformedResultIsPreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget target = new(device, Silk.NET.Direct3D9.MultisampleType.MultisampleNone);
        var rectangle = new GeneratedRenderDataInstruction(GeneratedRenderDataKind.DrawRoundedRectangle, [], new byte[32], [null, null]);

        int result = GeneratedHardwareVisualRenderer.Draw([rectangle], device, target,
            GeneratedImageTransform.Identity, new(0, 0, 24, 16), 1, MilCompositingMode.SourceOver);

        Assert.AreEqual(Direct3D9Factory.UceMalformedPacketHResult, result);
    }

    [TestMethod]
    public unsafe void WhenWindowRectanglePacketHasNoResourceSlotsThenMalformedResultIsPreserved()
    {
        using Direct3D9Device device = CreateClearDevice([]);
        using Direct3D9SurfaceRenderTarget target = new(device, Silk.NET.Direct3D9.MultisampleType.MultisampleNone);
        var rectangle = new GeneratedRenderDataInstruction(GeneratedRenderDataKind.DrawRectangle, [], new byte[32], []);

        int result = GeneratedHardwareVisualRenderer.Draw([rectangle], device, target,
            GeneratedImageTransform.Identity, new(0, 0, 24, 16), 1, MilCompositingMode.SourceOver);

        Assert.AreEqual(Direct3D9Factory.UceMalformedPacketHResult, result);
    }
}
