using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe partial class CompositionEffectsTests
{
    [TestMethod]
    [DataRow(16u, 0.5, 0xff7f007fu, -1f)]
    [DataRow(26u, 0.5, 0u, -1f)]
    [DataRow(16u, 0.00196851, 0xff0000feu, -1f)]
    [DataRow(16u, 0.75, 0x02020202u, 0.007843138f)]
    [DataRow(26u, 0.5, 0u, 0.5f)]
    public void WhenOpacityLayerEndsThenProductionEffectScalesWholeGroup(uint format, double opacity, uint expected, float maskAlpha)
    {
        string path = typeof(CompositionEffectsTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!;
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
            Assert.AreEqual(0, targetCreate(factory, 1, 1, format, 96, 96, 1, &target));
            Assert.AreEqual(0, targetCreate(factory, 1, 1, format, 96, 96, 1, &sourceTarget));
            Assert.AreEqual(0, getBitmap(sourceTarget, &source));
            Assert.AreEqual(0, getBitmap(target, &output));
            WritePixel(source, format, new(1.25f, -0.25f, 0.1234567f, 1), maskAlpha >= 0 ? 0xffffffff : 0xfffe0000);
            WritePixel(output, format, new(0, 0, 1, 1), maskAlpha >= 0 ? 0u : 0xff0000ff);
            uint image = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, resource(channel, 95, &image));
            Assert.AreEqual(0, resource(channel, 43, &data));
            Assert.AreEqual(0, resource(channel, 39, &visual));
            Assert.AreEqual(0, resource(channel, 47, &destination));
            byte* sourcePacket = stackalloc byte[8 + sizeof(nint)];
            *(uint*)sourcePacket = 0x0c; *(uint*)(sourcePacket + 4) = image; *(nint*)(sourcePacket + 8) = source;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)source)[1])(source);
            Assert.AreEqual(0, send(sourcePacket, (uint)(8 + sizeof(nint)), 0, channel));
            byte* packet = stackalloc byte[132];
            new Span<byte>(packet, 132).Clear();
            *(uint*)packet = 0x18; *(uint*)(packet + 4) = data; *(uint*)(packet + 8) = 120;
            *(uint*)(packet + 12) = 16; *(uint*)(packet + 16) = 0x4f; *(double*)(packet + 20) = maskAlpha >= 0 ? 1 : opacity;
            *(uint*)(packet + 28) = 48; *(uint*)(packet + 32) = 0x47;
            *(double*)(packet + 52) = 1; *(double*)(packet + 60) = 1; *(uint*)(packet + 68) = image;
            new ReadOnlySpan<byte>(packet + 28, 48).CopyTo(new Span<byte>(packet + 76, 48));
            *(uint*)(packet + 124) = 8; *(uint*)(packet + 128) = 0x56;
            Assert.AreEqual(0, send(packet, 132, 0, channel));
            uint* bind = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(bind, 12, 0, channel));
            byte* targetPacket = stackalloc byte[36]; new Span<byte>(targetPacket, 36).Clear();
            *(uint*)targetPacket = 0x34; *(uint*)(targetPacket + 4) = destination;
            *(ulong*)(targetPacket + 16) = (ulong)target; *(uint*)(targetPacket + 24) = 1; *(uint*)(targetPacket + 28) = 1;
            Assert.AreEqual(0, send(targetPacket, 36, 0, channel));
            bind[0] = 0x35; bind[1] = destination; bind[2] = visual;
            Assert.AreEqual(0, send(bind, 12, 0, channel));
            if (maskAlpha >= 0)
            {
                uint brush = 0;
                Assert.AreEqual(0, resource(channel, 75, &brush));
                byte* brushPacket = stackalloc byte[48]; new Span<byte>(brushPacket, 48).Clear();
                *(uint*)brushPacket = 0x7e; *(uint*)(brushPacket + 4) = brush;
                *(double*)(brushPacket + 8) = 1; *(float*)(brushPacket + 16) = maskAlpha;
                Assert.AreEqual(0, send(brushPacket, 48, 0, channel));
                bind[0] = 0x23; bind[1] = visual; bind[2] = brush;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                byte* alphaPacket = stackalloc byte[16];
                *(uint*)alphaPacket = 0x20; *(uint*)(alphaPacket + 4) = visual; *(double*)(alphaPacket + 8) = opacity;
                Assert.AreEqual(0, send(alphaPacket, 16, 0, channel));
            }
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Release(sourceTarget); sourceTarget = 0; Release(source); source = 0;
            Assert.AreEqual(0, present(connection));
            if (format == 16)
            {
                uint pixel;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 4, 4, (byte*)&pixel));
                Assert.AreEqual(expected, pixel);
            }
            else
            {
                Vector4 pixel;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 16, 16, (byte*)&pixel));
                float factor = (float)opacity * (maskAlpha >= 0 ? maskAlpha : 1);
                Vector4 value = new Vector4(1.25f, -0.25f, 0.1234567f, 1) * factor + new Vector4(0, 0, 1, 1) * (1 - factor);
                Assert.IsTrue(Vector4.Distance(value, pixel) < 0.0000001f);
            }
            if (maskAlpha >= 0)
            {
                // Reuse the bound brush through PushOpacityMask; Pop must not affect the following image.
                uint maskBrush = bind[2];
                uint opacityResource = 0;
                Assert.AreEqual(0, resource(channel, 49, &opacityResource));
                byte* animatedOpacity = stackalloc byte[16];
                *(uint*)animatedOpacity = 0x0e; *(uint*)(animatedOpacity + 4) = opacityResource; *(double*)(animatedOpacity + 8) = 0;
                Assert.AreEqual(0, send(animatedOpacity, 16, 0, channel));
                byte* animatedBrush = stackalloc byte[48]; new Span<byte>(animatedBrush, 48).Clear();
                *(uint*)animatedBrush = 0x7e; *(uint*)(animatedBrush + 4) = maskBrush;
                *(double*)(animatedBrush + 8) = 1; *(float*)(animatedBrush + 16) = maskAlpha;
                *(uint*)(animatedBrush + 32) = opacityResource;
                Assert.AreEqual(0, send(animatedBrush, 48, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                WritePixel(output, format, Vector4.Zero, 0);
                Assert.AreEqual(0, present(connection));
                AssertTransparent(output, format);
                *(double*)(animatedOpacity + 8) = double.NaN;
                Assert.AreEqual(0, send(animatedOpacity, 16, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(unchecked((int)0x80004001), present(connection));
                AssertTransparent(output, format);
                *(double*)(animatedOpacity + 8) = 1;
                Assert.AreEqual(0, send(animatedOpacity, 16, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, present(connection));
                if (format == 16)
                {
                    uint pixel;
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 4, 4, (byte*)&pixel));
                    Assert.AreEqual(expected, pixel);
                }
                else
                {
                    Vector4 pixel;
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 16, 16, (byte*)&pixel));
                    Vector4 restored = new Vector4(1.25f, -0.25f, 0.1234567f, 1) * (maskAlpha * (float)opacity);
                    Assert.IsTrue(Vector4.Distance(restored, pixel) < 0.0000001f);
                }
                uint colorResource = 0;
                Assert.AreEqual(0, resource(channel, 50, &colorResource));
                byte* animatedColor = stackalloc byte[24]; new Span<byte>(animatedColor, 24).Clear();
                *(uint*)animatedColor = 0x0f; *(uint*)(animatedColor + 4) = colorResource;
                *(float*)(animatedColor + 12) = 1;
                Assert.AreEqual(0, send(animatedColor, 24, 0, channel));
                *(uint*)(animatedBrush + 44) = colorResource;
                Assert.AreEqual(0, send(animatedBrush, 48, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                WritePixel(output, format, Vector4.Zero, 0);
                Assert.AreEqual(0, present(connection));
                AssertTransparent(output, format);
                *(float*)(animatedColor + 8) = float.NaN;
                Assert.AreEqual(0, send(animatedColor, 24, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(unchecked((int)0x80004001), present(connection));
                AssertTransparent(output, format);
                *(float*)(animatedColor + 8) = maskAlpha;
                Assert.AreEqual(0, send(animatedColor, 24, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, present(connection));
                AssertMaskedPixel(output, format, expected, opacity, maskAlpha);
                *(uint*)(animatedBrush + 44) = 0;
                Assert.AreEqual(0, send(animatedBrush, 48, 0, channel));
                *(float*)(animatedColor + 8) = 0;
                Assert.AreEqual(0, send(animatedColor, 24, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                WritePixel(output, format, Vector4.Zero, 0);
                Assert.AreEqual(0, present(connection));
                AssertMaskedPixel(output, format, expected, opacity, maskAlpha);
                bind[0] = 0x23; bind[1] = visual; bind[2] = 0;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                byte* alphaReset = stackalloc byte[16];
                *(uint*)alphaReset = 0x20; *(uint*)(alphaReset + 4) = visual; *(double*)(alphaReset + 8) = 1;
                Assert.AreEqual(0, send(alphaReset, 16, 0, channel));
                byte* masked = stackalloc byte[148]; new Span<byte>(masked, 148).Clear();
                *(uint*)masked = 0x18; *(uint*)(masked + 4) = data; *(uint*)(masked + 8) = 136;
                *(uint*)(masked + 12) = 32; *(uint*)(masked + 16) = 0x4e;
                *(uint*)(masked + 36) = maskBrush;
                new ReadOnlySpan<byte>(packet + 28, 48).CopyTo(new Span<byte>(masked + 44, 48));
                *(uint*)(masked + 92) = 8; *(uint*)(masked + 96) = 0x56;
                new ReadOnlySpan<byte>(packet + 28, 48).CopyTo(new Span<byte>(masked + 100, 48));
                Assert.AreEqual(0, send(masked, 148, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                WritePixel(output, format, Vector4.Zero, 0);
                Assert.AreEqual(0, present(connection));
                if (format == 16)
                {
                    uint pixel;
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 4, 4, (byte*)&pixel));
                    Assert.AreEqual(0xffffffffu, pixel);
                }
                else
                {
                    Vector4 pixel;
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 16, 16, (byte*)&pixel));
                    Assert.AreEqual(new Vector4(1.25f, -0.25f, 0.1234567f, 1), pixel);
                }
            }
        }
        finally
        {
            if (channel != 0) destroy(channel);
            if (connection != 0) disconnect(connection);
            Release(output); Release(source); Release(sourceTarget); Release(target); Release(factory);
        }
    }

    private static void AssertMaskedPixel(nint bitmap, uint format, uint expected, double opacity, float maskAlpha)
    {
        if (format == 16)
        {
            uint pixel;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 4, 4, (byte*)&pixel));
            Assert.AreEqual(expected, pixel);
        }
        else
        {
            Vector4 pixel;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, 16, 16, (byte*)&pixel));
            Vector4 value = new Vector4(1.25f, -0.25f, 0.1234567f, 1) * (maskAlpha * (float)opacity);
            Assert.IsTrue(Vector4.Distance(value, pixel) < 0.0000001f);
        }
    }

    private static void AssertTransparent(nint bitmap, uint format)
    {
        byte* bytes = stackalloc byte[16];
        uint size = format == 16 ? 4u : 16u;
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)bitmap)[7])(bitmap, 0, size, size, bytes));
        CollectionAssert.AreEqual(new byte[size], new ReadOnlySpan<byte>(bytes, (int)size).ToArray());
    }

    private static void WritePixel(nint bitmap, uint format, Vector4 value, uint packed)
    {
        nint locked = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &locked));
            uint size; byte* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)locked)[5])(locked, &size, &pixels));
            if (format == 16) *(uint*)pixels = packed;
            else *(Vector4*)pixels = value;
        }
        finally { Release(locked); }
    }

    private static void Release(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }
}
