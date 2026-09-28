using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class GeneratedEffectResourceTests
{
    [TestMethod]
    public void WhenEffectLayoutsAreMeasuredThenNativeSizesMatch()
    {
        Assert.AreEqual((20, 28, 28, 80, 80), (Marshal.SizeOf<MilPixelShaderCommand>(), Marshal.SizeOf<MilImplicitInputBrushCommand>(), Marshal.SizeOf<MilBlurEffectCommand>(), Marshal.SizeOf<MilDropShadowEffectCommand>(), Marshal.SizeOf<MilShaderEffectCommand>()));
    }

    [TestMethod]
    public void WhenPixelShaderIsWrittenThenGoldenBytesMatch()
    {
        byte[] expected = [0x6C, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 4, 0, 0, 0, 1, 0, 0, 0, 1, 2, 3, 4];
        CollectionAssert.AreEqual(expected, GeneratedProtocolPacketWriter.WriteEffect(new MilPixelShaderCommand(MilCommand.PixelShader, 1, MilShaderRenderMode.HardwareOnly, 4, 1), [1, 2, 3, 4]));
    }

    [TestMethod]
    public void WhenPixelShaderPacketIsMutatedThenStoredBytecodeIsUnchanged()
    {
        var (table, router) = Context();
        byte[] packet = GeneratedProtocolPacketWriter.WriteEffect(new MilPixelShaderCommand(MilCommand.PixelShader, 1, MilShaderRenderMode.Auto, 4, -1), [1, 2, 3, 4]);
        _ = router.ProcessPackets([Create(1, MilResourceType.PixelShader), packet]);
        Array.Clear(packet);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, Get<GeneratedPixelShaderResource>(table, 1).Bytecode.ToArray());
    }

    [TestMethod]
    [DataRow(3u, 4u)]
    [DataRow(0u, 3u)]
    [DataRow(0u, uint.MaxValue)]
    public void WhenShaderHeaderIsInvalidThenPreviousBytecodeRemains(uint mode, uint size)
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.PixelShader), GeneratedProtocolPacketWriter.WriteEffect(new MilPixelShaderCommand(MilCommand.PixelShader, 1, 0, 4, 0), [1, 2, 3, 4])]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteEffect(new MilPixelShaderCommand(MilCommand.PixelShader, 1, (MilShaderRenderMode)mode, size, 0), [5, 6, 7, 8]));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, (byte)1), (result, Get<GeneratedPixelShaderResource>(table, 1).Bytecode.Span[0]));
    }

    [TestMethod]
    public void WhenBlurAnimationChangesThenNotificationPropagates()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.BlurEffect), GeneratedProtocolPacketWriter.WriteEffect(new MilBlurEffectCommand(MilCommand.BlurEffect, 2, 4, 1, MilKernelType.Box, MilEffectRenderingBias.Quality))]);
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(1, 8));
        Assert.AreEqual(2, Get<GeneratedBlurEffectResource>(table, 2).ChangeCount);
    }

    [TestMethod]
    public void WhenBlurFamilyIsWrongThenOldStateAndReferencesRemain()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.BlurEffect), Create(3, MilResourceType.ColorResource), GeneratedProtocolPacketWriter.WriteEffect(new MilBlurEffectCommand(MilCommand.BlurEffect, 2, 4, 1, 0, 0))]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteEffect(new MilBlurEffectCommand(MilCommand.BlurEffect, 2, 9, 3, 0, 0)));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 4.0, 2), (result, Get<GeneratedBlurEffectResource>(table, 2).Data.Radius, Get<GeneratedValueResource<double>>(table, 1).ReferenceCount));
    }

    [TestMethod]
    public void WhenShadowHasRepeatedAnimationThenFinalDeleteReleasesAllReferences()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.DropShadowEffect), GeneratedProtocolPacketWriter.WriteEffect(new MilDropShadowEffectCommand(MilCommand.DropShadowEffect, 2, 1, new(), 2, 3, 4, 1, 0, 1, 1, 1, 0))]);
        var animation = Get<GeneratedValueResource<double>>(table, 1);
        _ = table.Delete(1, MilResourceType.DoubleResource);
        _ = table.Delete(2, MilResourceType.DropShadowEffect);
        Assert.AreEqual((0, true), (animation.ReferenceCount, animation.IsReleased));
    }

    [TestMethod]
    public void WhenShaderEffectHasPairedFloatPayloadThenTypedValuesMatch()
    {
        var (table, router) = Context();
        byte[] payload = new byte[18];
        BitConverter.GetBytes((short)2).CopyTo(payload, 0);
        BitConverter.GetBytes(1.5f).CopyTo(payload, 2);
        int result = router.ProcessPackets([Create(1, MilResourceType.ShaderEffect), GeneratedProtocolPacketWriter.WriteEffect(Shader(1) with { FloatRegistersSize = 2, FloatValuesSize = 16 }, payload)]);
        var data = Get<GeneratedShaderEffectResource>(table, 1).Data;
        Assert.AreEqual((0, (short)2, 1.5f), (result, data!.FloatRegisters[0], data.FloatValues[0]));
    }

    [TestMethod]
    public void WhenShaderPayloadPairIsIncompleteThenUpdateIsRejected()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.ShaderEffect), GeneratedProtocolPacketWriter.WriteEffect(Shader(1))]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteEffect(Shader(1) with { FloatRegistersSize = 2, FloatValuesSize = 4 }, new byte[6]));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 1), (result, Get<GeneratedShaderEffectResource>(table, 1).ChangeCount));
    }

    [TestMethod]
    public void WhenSamplerUsesImplicitBrushThenGraphNotificationPropagates()
    {
        var (table, router) = Context();
        byte[] payload = new byte[12];
        BitConverter.GetBytes(2u).CopyTo(payload, 8);
        _ = router.ProcessPackets([Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.ImplicitInputBrush), Create(3, MilResourceType.ShaderEffect),
            GeneratedProtocolPacketWriter.WriteEffect(new MilImplicitInputBrushCommand(MilCommand.ImplicitInputBrush, 2, 1, 1, 0, 0)),
            GeneratedProtocolPacketWriter.WriteEffect(Shader(3) with { SamplerInfoSize = 8, SamplerValuesSize = 4 }, payload)]);
        _ = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(1, 0.5));
        Assert.AreEqual(2, Get<GeneratedShaderEffectResource>(table, 3).ChangeCount);
    }

    [TestMethod]
    public void WhenEffectIsDuplicatedThenLastHandleReleasesShader()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.PixelShader), Create(2, MilResourceType.ShaderEffect), GeneratedProtocolPacketWriter.WriteEffect(Shader(2) with { PixelShader = 1 })]);
        var shader = Get<GeneratedPixelShaderResource>(table, 1);
        GeneratedProtocolHandleTable target = new();
        _ = table.DuplicateTo(2, target, 3);
        _ = table.Delete(1, MilResourceType.PixelShader);
        _ = table.Delete(2, MilResourceType.ShaderEffect);
        int before = shader.ReferenceCount;
        _ = target.Delete(3, MilResourceType.ShaderEffect);
        Assert.AreEqual((1, 0, true), (before, shader.ReferenceCount, shader.IsReleased));
    }

    [TestMethod]
    public void WhenSamplerModeIsInvalidThenPreviousStateRemains()
    {
        var (table, router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.ShaderEffect), GeneratedProtocolPacketWriter.WriteEffect(Shader(1))]);
        var previous = Get<GeneratedShaderEffectResource>(table, 1).Data;
        byte[] payload = new byte[12];
        BitConverter.GetBytes(3).CopyTo(payload, 4);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteEffect(Shader(1) with { SamplerInfoSize = 8, SamplerValuesSize = 4 }, payload));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, previous), (result, Get<GeneratedShaderEffectResource>(table, 1).Data));
    }

    [TestMethod]
    public void WhenVisualAlphaMaskUsesImplicitBrushThenUpdateSucceeds()
    {
        var (_, router) = Context();
        byte[] packet = new byte[12];
        BitConverter.GetBytes((uint)MilCommand.VisualSetAlphaMask).CopyTo(packet, 0);
        BitConverter.GetBytes(2u).CopyTo(packet, 4);
        BitConverter.GetBytes(1u).CopyTo(packet, 8);
        int result = router.ProcessPackets([Create(1, MilResourceType.ImplicitInputBrush), Create(2, MilResourceType.Visual), packet]);
        Assert.AreEqual(0, result);
    }

    private static MilShaderEffectCommand Shader(uint handle) => new(MilCommand.ShaderEffect, handle, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0);
    private static byte[] Create(uint handle, MilResourceType type) => GeneratedProtocolPacketWriter.WriteChannelCreateResource(handle, type);
    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter) Context()
    {
        GeneratedProtocolHandleTable table = new();
        GeneratedProtocolChannelRegistry registry = new();
        _ = registry.TryAdd(1, table);
        return (table, new GeneratedProtocolProductionContext(1, registry).CreateRouter());
    }
    private static T Get<T>(GeneratedProtocolHandleTable table, uint handle) where T : GeneratedProtocolResource
    {
        Assert.IsTrue(table.TryGetResource(handle, out var resource));
        return (T)resource!;
    }
}
