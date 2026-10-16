using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionEffectsTests
{
    [TestMethod]
    [DataRow(0.25f, 0xff400000u, false)]
    [DataRow(0.5f, 0xff800000u, false)]
    [DataRow(0.5f, 0x00800000u, true)]
    public void WhenSoftwareOnlyShaderUsesConstantThenGenericTargetConsumesCpuPixels(float red, uint expected, bool killNegativeW)
        => RunSoftwareShader(red, expected, killNegativeW);

    [TestMethod]
    [DataRow(1, 64)]
    [DataRow(2, 191)]
    [DataRow(3, 0)]
    [DataRow(4, 223)]
    [DataRow(5, 32)]
    [DataRow(6, 255)]
    [DataRow(7, 255)]
    [DataRow(8, 96)]
    [DataRow(9, 128)]
    [DataRow(10, 64)]
    [DataRow(11, 128)]
    [DataRow(14, 255)]
    [DataRow(15, 0)]
    [DataRow(18, 175)]
    [DataRow(19, 64)]
    [DataRow(20, 128)]
    [DataRow(21, 128)]
    [DataRow(22, 96)]
    [DataRow(23, 96)]
    [DataRow(24, 96)]
    [DataRow(32, 127)]
    [DataRow(33, 0)]
    [DataRow(35, 64)]
    [DataRow(36, 147)]
    [DataRow(37, 191)]
    [DataRow(88, 128)]
    [DataRow(90, 255)]
    public void WhenSoftwareOnlyOpcodeExecutesThenPublicTargetReturnsIndependentPixel(int opcode, int red)
    {
        uint a = opcode == 37 ? 0xa0e40006u : 0xa0e40000u;
        int count = opcode switch { 1 or 6 or 7 or 14 or 15 or 19 or 35 or 36 => 1, 4 or 18 or 37 or 88 or 90 => 3, _ => 2 };
        List<uint> words = [0xffff0200];
        for (uint register = 0; register <= 7; register++)
        {
            float value = register switch { 0 => .25f, 2 => .75f, 6 => 0, 7 => 1, _ => .5f };
            words.AddRange([0x05000051, 0xa00f0000 | register, BitConverter.SingleToUInt32Bits(value),
                BitConverter.SingleToUInt32Bits(value), BitConverter.SingleToUInt32Bits(value), BitConverter.SingleToUInt32Bits(value)]);
        }
        words.Add((uint)((count+1)<<24) | (uint)opcode);
        words.Add(0x80010000);
        words.Add(a);
        if (count > 1) words.Add(0xa0e40001);
        if (count > 2) words.Add(0xa0e40002);
        words.AddRange([0x02000001, 0x80010800, 0x80000000, 0x02000001, 0x80080800, 0xa0ff0007, 0xffff]);
        RunSoftwareShader(.25f, 0xff000000u | ((uint)red << 16), false, words.ToArray());
    }

    [TestMethod]
    public void WhenRejectedShaderFallsBackThenIdentityPixelsAndBadShaderFifoAreReal()
        => RunSoftwareShader(.25f, 0xffffffff, false, [0xffff0200, 0x01000028, 0x80000000, 0xffff], true);

    [TestMethod]
    [DataRow(0, 0xff404040u)]
    [DataRow(1, 0xff808080u)]
    [DataRow(2, 0xff808080u)]
    [DataRow(3, 0xff808080u)]
    [DataRow(4, 0xff808080u)]
    public void WhenSoftwareGenericSamplerCombinesTwoInputsThenOffscreenPixelsAreReal(int samplerKind, uint expected)
        => RunSoftwareShader(.25f, expected, false,
            [0xffff0200, 0x0200001f, 0x80000000, 0xb0030000,
             0x0200001f, 0x90000000, 0xa00f0800, 0x0200001f, 0x90000000, 0xa00f0801,
             0x03000042, 0x800f0000, 0xb0e40000, 0xa0e40800,
             0x03000042, 0x800f0001, 0xb0e40000, 0xa0e40801,
             0x03000002, 0x800f0000, 0x80e40000, 0x80e40001,
             0x03000005, 0x80070800, 0x80e40000, 0xa0000000,
             0x02000001, 0x80080800, 0xa0ff0000, 0xffff], samplerKind: samplerKind);

    [TestMethod]
    [DataRow(-0.25f)]
    [DataRow(1.25f)]
    [DataRow(0.1234567f)]
    public void WhenSoftwareShaderWritesFloatTargetThenUnsaturatedConstantSurvives(float red)
        => RunSoftwareShader(red, 0, false, floating: true);

    [TestMethod]
    [DataRow(0, false)]
    [DataRow(0, true)]
    [DataRow(1, false)]
    [DataRow(1, true)]
    [DataRow(3, false)]
    [DataRow(3, true)]
    [DataRow(4, false)]
    [DataRow(4, true)]
    public void WhenFloatShaderSamplesRealConsumerThenHdrAndAlphaSurvive(int samplerKind, bool bilinear)
        => RunSoftwareShader(1.25f, 0, false,
            [0xffff0200, 0x0200001f, 0x80000000, 0xb0030000,
             0x0200001f, 0x90000000, 0xa00f0800,
             0x0200001f, 0x90000000, 0xa00f0801,
             0x03000042, 0x800f0800, 0xb0e40000, samplerKind == 0 ? 0xa0e40800u : 0xa0e40801u, 0xffff],
            samplerKind: samplerKind, floating: true, sampleInput: true, bilinear: bilinear);

    private void RunSoftwareShader(float red, uint expected, bool killNegativeW, uint[]? overrideWords = null, bool notification = false,
        int samplerKind = -1, bool floating = false, bool sampleInput = false, bool bilinear = false)
    {
        string path = AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var factoryCreate = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
        var targetCreate = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateBitmapRenderTarget");
        var getBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapGetBitmap");
        nint connection = 0, channel = 0, factory = 0, target = 0, sourceTarget = 0, source = 0, output = 0;
        try
        {
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, createChannel(connection, 0, &channel));
            Assert.AreEqual(0, factoryCreate(&factory, 0x200184C0));
            Assert.AreEqual(0, targetCreate(factory, 5, 1, floating ? 26u : 16u, 96, 96, 1, &target));
            Assert.AreEqual(0, targetCreate(factory, 1, 1, floating ? 26u : 16u, 96, 96, 1, &sourceTarget));
            Assert.AreEqual(0, getBitmap(sourceTarget, &source));
            Assert.AreEqual(0, getBitmap(target, &output));
            WritePixel(source, floating ? 26u : 16u, sampleInput ? new Vector4(1.25f, -0.25f, 0.1234567f, 0.5f) : Vector4.One, 0xffffffff);
            uint image = 0, data = 0, visual = 0, destination = 0, shader = 0, effect = 0;
            Assert.AreEqual(0, resource(channel, 95, &image));
            Assert.AreEqual(0, resource(channel, 43, &data));
            Assert.AreEqual(0, resource(channel, 39, &visual));
            Assert.AreEqual(0, resource(channel, 47, &destination));
            Assert.AreEqual(0, resource(channel, 33, &shader));
            Assert.AreEqual(0, resource(channel, 38, &effect));
            byte* sourcePacket = stackalloc byte[16];
            *(uint*)sourcePacket = 0x0c; *(uint*)(sourcePacket + 4) = image; *(nint*)(sourcePacket + 8) = source;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source)[1])(source);
            Assert.AreEqual(0, send(sourcePacket, 16, 0, channel));
            byte* drawing = stackalloc byte[60]; new Span<byte>(drawing, 60).Clear();
            *(uint*)drawing = 0x18; *(uint*)(drawing + 4) = data; *(uint*)(drawing + 8) = 48;
            *(uint*)(drawing + 12) = 48; *(uint*)(drawing + 16) = 0x47;
            *(double*)(drawing + 36) = 5; *(double*)(drawing + 44) = 1; *(uint*)(drawing + 52) = image;
            Assert.AreEqual(0, send(drawing, 60, 0, channel));
            uint* bind = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(bind, 12, 0, channel));
            uint[] words = overrideWords ?? (killNegativeW
                ? [0xffff0200, 0x02000001, 0x800f0000, 0xa0e40000, 0x01000041, 0x800f0000, 0x02000001, 0x800f0800, 0xa0e40000, 0xffff]
                : [0xffff0200, 0x02000001, 0x800f0800, 0xa0e40000, 0xffff]);
            uint* shaderPacket = stackalloc uint[5 + words.Length];
            shaderPacket[0] = 0x6c; shaderPacket[1] = shader; shaderPacket[2] = 1;
            shaderPacket[3] = (uint)(words.Length * 4); shaderPacket[4] = 1;
            words.AsSpan().CopyTo(new Span<uint>(shaderPacket + 5, words.Length));
            Assert.AreEqual(0, send(shaderPacket, (uint)((5 + words.Length) * 4), 0, channel));
            uint implicitBrush = 0, secondary = 0;
            if (samplerKind >= 0)
            {
                Assert.AreEqual(0, resource(channel, 34, &implicitBrush));
                byte* implicitPacket = stackalloc byte[28]; new Span<byte>(implicitPacket, 28).Clear();
                *(uint*)implicitPacket = 0x6d; *(uint*)(implicitPacket + 4) = implicitBrush;
                *(double*)(implicitPacket + 8) = 1;
                Assert.AreEqual(0, send(implicitPacket, 28, 0, channel));
                if (samplerKind > 0)
                {
                    uint samplerVisual = 0;
                    Assert.AreEqual(0, resource(channel, 39, &samplerVisual));
                    uint* samplerBind = stackalloc uint[] { 0x22, samplerVisual, data };
                    Assert.AreEqual(0, send(samplerBind, 12, 0, channel));
                    Assert.AreEqual(0, resource(channel, samplerKind == 2 ? 83u : samplerKind >= 3 ? 80u : 82u, &secondary));
                    if (samplerKind == 2)
                    {
                        uint mode = 0;
                        Assert.AreEqual(0, resource(channel, 94, &mode));
                        byte* modePacket = stackalloc byte[32]; new Span<byte>(modePacket, 32).Clear();
                        *(uint*)modePacket = 0x8d; *(uint*)(modePacket + 4) = mode; *(double*)(modePacket + 8) = 1;
                        Assert.AreEqual(0, send(modePacket, 32, 0, channel));
                        byte* cachePacket = stackalloc byte[36]; new Span<byte>(cachePacket, 36).Clear();
                        *(uint*)cachePacket = 0x84; *(uint*)(cachePacket + 4) = secondary;
                        *(double*)(cachePacket + 8) = 1; *(uint*)(cachePacket + 28) = mode;
                        *(uint*)(cachePacket + 32) = samplerVisual;
                        Assert.AreEqual(0, send(cachePacket, 36, 0, channel));
                    }
                    else
                    {
                        byte* brushPacket = stackalloc byte[148]; new Span<byte>(brushPacket, 148).Clear();
                        *(uint*)brushPacket = samplerKind >= 3 ? 0x81u : 0x83u; *(uint*)(brushPacket + 4) = secondary;
                        *(double*)(brushPacket + 8) = 1;
                        foreach (int offset in new[] { 32, 40, 64, 72 }) *(double*)(brushPacket + offset) = 1;
                        *(uint*)(brushPacket + 108) = 1; *(uint*)(brushPacket + 112) = 1;
                        uint samplerSource = samplerVisual;
                        if (samplerKind == 3) samplerSource = image;
                        if (samplerKind == 4)
                        {
                            uint imageDrawing = 0, drawingImage = 0;
                            Assert.AreEqual(0, resource(channel, 89, &imageDrawing));
                            byte* imageDrawingPacket = stackalloc byte[48]; new Span<byte>(imageDrawingPacket, 48).Clear();
                            *(uint*)imageDrawingPacket = 0x89; *(uint*)(imageDrawingPacket + 4) = imageDrawing;
                            *(double*)(imageDrawingPacket + 24) = 5; *(double*)(imageDrawingPacket + 32) = 1;
                            *(uint*)(imageDrawingPacket + 40) = image;
                            Assert.AreEqual(0, send(imageDrawingPacket, 48, 0, channel));
                            Assert.AreEqual(0, resource(channel, 59, &drawingImage));
                            uint* drawingImagePacket = stackalloc uint[] { 0x71, drawingImage, imageDrawing };
                            Assert.AreEqual(0, send(drawingImagePacket, 12, 0, channel));
                            samplerSource = drawingImage;
                        }
                        *(uint*)(brushPacket + 124) = 1; *(uint*)(brushPacket + 144) = samplerSource;
                        Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                    }
                }
            }
            uint effectLength = samplerKind >= 0 ? 122u : 98u;
            byte* effectPacket = stackalloc byte[(int)effectLength]; new Span<byte>(effectPacket, (int)effectLength).Clear();
            *(uint*)effectPacket = 0x70; *(uint*)(effectPacket + 4) = effect; *(uint*)(effectPacket + 40) = shader;
            *(int*)(effectPacket + 44) = -1; *(uint*)(effectPacket + 48) = 2; *(uint*)(effectPacket + 52) = 16;
            *(Vector4*)(effectPacket + 82) = new(red, 0, 0, killNegativeW ? -1 : 1);
            if (samplerKind >= 0)
            {
                *(uint*)(effectPacket + 72) = 16; *(uint*)(effectPacket + 76) = 8;
                *(int*)(effectPacket + 98) = 0; *(int*)(effectPacket + 102) = bilinear ? 2 : 1;
                *(int*)(effectPacket + 106) = 1; *(int*)(effectPacket + 110) = bilinear ? 2 : 1;
                *(uint*)(effectPacket + 114) = implicitBrush; *(uint*)(effectPacket + 118) = secondary;
            }
            Assert.AreEqual(0, send(effectPacket, effectLength, 0, channel));
            bind[0] = 0x1d; bind[1] = visual; bind[2] = effect;
            Assert.AreEqual(0, send(bind, 12, 0, channel));
            byte* targetPacket = stackalloc byte[36]; new Span<byte>(targetPacket, 36).Clear();
            *(uint*)targetPacket = 0x34; *(uint*)(targetPacket + 4) = destination; *(ulong*)(targetPacket + 16) = (ulong)target;
            *(uint*)(targetPacket + 24) = 5; *(uint*)(targetPacket + 28) = 1;
            Assert.AreEqual(0, send(targetPacket, 36, 0, channel));
            bind[0] = 0x35; bind[1] = destination; bind[2] = visual;
            Assert.AreEqual(0, send(bind, 12, 0, channel));
            if (notification)
            {
                uint* registration = stackalloc uint[] { 3, 1 };
                Assert.AreEqual(0, send(registration, 8, 0, channel));
            }
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            if (notification)
            {
                var peek = (delegate* unmanaged[Stdcall]<nint, void*, nuint, int*, int>)NativeLibrary.GetExport(module, "MilComposition_PeekNextMessage");
                byte* message = stackalloc byte[80];
                new Span<byte>(message,80).Fill(0xcc);
                int retrieved = 0;
                Assert.AreEqual(0, peek(channel,message,4,&retrieved));
                Assert.AreEqual(1,retrieved);
                Assert.AreEqual(5u,*(uint*)message);
                Assert.AreEqual(0, peek(channel,message,56,&retrieved));
                Assert.AreEqual(1,retrieved);
                Assert.AreEqual(0x10u,*(uint*)message);
                CollectionAssert.AreEqual(new byte[52],new Span<byte>(message+4,52).ToArray());
                Assert.AreEqual((byte)0xcc,message[64]);
                Assert.AreEqual(0,present(connection));
                Assert.AreEqual(0,peek(channel,message,80,&retrieved));
                Assert.AreEqual(0,retrieved);
                CollectionAssert.AreEqual(new byte[64],new Span<byte>(message,64).ToArray());
                Assert.AreEqual((byte)0xcc,message[64]);
            }
            if (floating)
            {
                Guid format;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)output)[4])(output, &format));
                Assert.AreEqual(new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91a"), format);
                Vector4* values = stackalloc Vector4[5];
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 80, 80, (byte*)values));
                CollectionAssert.AreEqual(Enumerable.Repeat(sampleInput ? new Vector4(1.25f, -0.25f, 0.1234567f, 0.5f) : new Vector4(red, 0, 0, 1), 5).ToArray(), new ReadOnlySpan<Vector4>(values, 5).ToArray());
                return;
            }
            uint* pixels = stackalloc uint[5];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 20, 20, (byte*)pixels));
            CollectionAssert.AreEqual(Enumerable.Repeat(expected, 5).ToArray(), new ReadOnlySpan<uint>(pixels, 5).ToArray());
        }
        finally
        {
            if (channel != 0) destroy(channel);
            if (connection != 0) disconnect(connection);
            Release(output); Release(source); Release(sourceTarget); Release(target); Release(factory);
        }
    }
}
