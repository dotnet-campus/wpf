using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionEffectsTests
{
    [TestMethod]
    [DataRow(6u)]
    [DataRow(10u)]
    public void WhenFloatGradientMaskLockFailsThenRetryPreservesHdrSpan(uint width)
    {
        string path = AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        nint connection = 0, channel = 0, factory = 0, target = 0, colorTarget = 0, maskTarget = 0;
        nint color = 0, mask = 0, output = 0, heldLock = 0;
        try
        {
            var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
            var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
            var createFactory = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
            var createTarget = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateBitmapRenderTarget");
            var getBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapGetBitmap");
            var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
            var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
            var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, createChannel(connection, 0, &channel));
            Assert.AreEqual(0, createFactory(&factory, 0x200184C0));
            Assert.AreEqual(0, createTarget(factory, width, 1, 26, 96, 96, 1, &target));
            Assert.AreEqual(0, createTarget(factory, 1, 1, 26, 96, 96, 1, &colorTarget));
            Assert.AreEqual(0, createTarget(factory, 2, 1, 26, 96, 96, 1, &maskTarget));
            Assert.AreEqual(0, getBitmap(target, &output));
            Assert.AreEqual(0, getBitmap(colorTarget, &color));
            Assert.AreEqual(0, getBitmap(maskTarget, &mask));
            Vector4 hdr = new(1.25f, -0.25f, 0.1234567f, 1);
            WritePixel(color, 26, hdr, 0);
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)mask)[8])(mask, 0, 2, &heldLock));
            uint size; byte* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)heldLock)[5])(heldLock, &size, &pixels));
            ((Vector4*)pixels)[0] = new(0, 0, 0, 0.25f);
            ((Vector4*)pixels)[1] = new(0, 0, 0, 0.75f);
            Release(heldLock); heldLock = 0;
            uint image = 0, maskImage = 0, brush = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, resource(channel, 95, &image));
            Assert.AreEqual(0, resource(channel, 95, &maskImage));
            Assert.AreEqual(0, resource(channel, 80, &brush));
            Assert.AreEqual(0, resource(channel, 43, &data));
            Assert.AreEqual(0, resource(channel, 39, &visual));
            Assert.AreEqual(0, resource(channel, 47, &destination));
            byte* sourcePacket = stackalloc byte[16]; new Span<byte>(sourcePacket, 16).Clear();
            *(uint*)sourcePacket = 0x0c; *(uint*)(sourcePacket + 4) = image; *(nint*)(sourcePacket + 8) = color;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)color)[1])(color);
            Assert.AreEqual(0, send(sourcePacket, 16, 0, channel));
            *(uint*)(sourcePacket + 4) = maskImage; *(nint*)(sourcePacket + 8) = mask;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)mask)[1])(mask);
            Assert.AreEqual(0, send(sourcePacket, 16, 0, channel));
            byte* brushPacket = stackalloc byte[148]; new Span<byte>(brushPacket, 148).Clear();
            *(uint*)brushPacket = 0x81; *(uint*)(brushPacket + 4) = brush; *(double*)(brushPacket + 8) = 1;
            *(double*)(brushPacket + 32) = width; *(double*)(brushPacket + 40) = 1;
            *(double*)(brushPacket + 64) = 2; *(double*)(brushPacket + 72) = 1;
            *(uint*)(brushPacket + 124) = 1; *(uint*)(brushPacket + 144) = maskImage;
            Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
            byte* draw = stackalloc byte[60]; new Span<byte>(draw, 60).Clear();
            *(uint*)draw = 0x18; *(uint*)(draw + 4) = data; *(uint*)(draw + 8) = 48;
            *(uint*)(draw + 12) = 48; *(uint*)(draw + 16) = 0x47;
            *(double*)(draw + 36) = width; *(double*)(draw + 44) = 1; *(uint*)(draw + 52) = image;
            Assert.AreEqual(0, send(draw, 60, 0, channel));
            uint* bind = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(bind, 12, 0, channel));
            bind[0] = 0x23; bind[2] = brush;
            Assert.AreEqual(0, send(bind, 12, 0, channel));
            byte* targetPacket = stackalloc byte[36]; new Span<byte>(targetPacket, 36).Clear();
            *(uint*)targetPacket = 0x34; *(uint*)(targetPacket + 4) = destination; *(ulong*)(targetPacket + 16) = (ulong)target;
            *(uint*)(targetPacket + 24) = width; *(uint*)(targetPacket + 28) = 1;
            Assert.AreEqual(0, send(targetPacket, 36, 0, channel));
            bind[0] = 0x35; bind[1] = destination; bind[2] = visual;
            Assert.AreEqual(0, send(bind, 12, 0, channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch"))(channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel"))(channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)mask)[8])(mask, 0, 2, &heldLock));
            Assert.AreEqual(unchecked((int)0x88982F0D), present(connection));
            Vector4[] actual = new Vector4[width];
            fixed (Vector4* values = actual)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, width * 16, width * 16, (byte*)values));
            CollectionAssert.AreEqual(new Vector4[width], actual);
            Release(heldLock); heldLock = 0;
            Assert.AreEqual(0, present(connection));
            fixed (Vector4* values = actual)
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, width * 16, width * 16, (byte*)values));
            float inverseScale = (float)(1d / (width / 2f));
            float coordinate = inverseScale * 0.5f - 0.5f;
            float end = width * inverseScale + coordinate;
            float increment = (end - coordinate) / width;
            for (int x = 0; x < width; x++, coordinate += increment)
            {
                Vector4 expected = hdr * (0.25f + 0.5f * Math.Clamp(coordinate, 0, 1));
                Assert.IsTrue(Vector4.Distance(expected, actual[x]) < 0.0000001f, $"Pixel {x}: expected {expected}, actual {actual[x]}");
            }
        }
        finally
        {
            Release(heldLock); Release(output); Release(mask); Release(color);
            Release(maskTarget); Release(colorTarget); Release(target); Release(factory);
            if (channel != 0) ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel"))(channel);
            if (connection != 0) ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect"))(connection);
            NativeLibrary.Free(module);
        }
    }
}
