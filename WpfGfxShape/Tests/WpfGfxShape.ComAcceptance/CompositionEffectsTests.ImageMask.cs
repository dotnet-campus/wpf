using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionEffectsTests
{
    [TestMethod]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u)]
    [DataRow(false, 192f, 1u, 0u, 1d, 0u, 0xffff0000u)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u)]
    [DataRow(true, 192f, 1u, 0u, 1d, 0u, 0xffff0000u)]
    [DataRow(false, 96f, 3u, 2u, 2d, 0xbfbf0000u, 0xffff0000u)]
    [DataRow(false, 96f, 2u, 0u, 2d, 0u, 0xffff0000u)]
    [DataRow(false, 96f, 0u, 0u, 2d, 0u, 0xffff0000u)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true)]
    public void WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping(bool relativeViewbox, float dpi, uint stretch, uint alignmentX, double viewportHeight, uint first, uint second, bool relativeViewport = false)
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
        nint connection = 0, channel = 0, factory = 0, target = 0, contentTarget = 0, maskTarget = 0, content = 0, mask = 0, output = 0, heldLock = 0;
        try
        {
            Assert.AreEqual(0, connect(1, &connection)); Assert.AreEqual(0, createChannel(connection, 0, &channel));
            Assert.AreEqual(0, factoryCreate(&factory, 0x200184C0));
            Assert.AreEqual(0, targetCreate(factory, 2, 1, 16, 96, 96, 1, &target));
            Assert.AreEqual(0, targetCreate(factory, 2, 1, 16, 96, 96, 1, &contentTarget));
            Assert.AreEqual(0, targetCreate(factory, 2, 1, 16, dpi, dpi, 1, &maskTarget));
            Assert.AreEqual(0, getBitmap(target, &output)); Assert.AreEqual(0, getBitmap(contentTarget, &content)); Assert.AreEqual(0, getBitmap(maskTarget, &mask));
            float* red = stackalloc float[] { 1, 0, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)contentTarget)[4])(contentTarget, red, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)mask)[8])(mask, 0, 2, &heldLock));
            uint size; byte* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)heldLock)[5])(heldLock, &size, &pixels));
            ((uint*)pixels)[0] = 0; ((uint*)pixels)[1] = 0xff000000;
            Release(heldLock); heldLock = 0;
            uint image = 0, maskImage = 0, brush = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, resource(channel, 95, &image)); Assert.AreEqual(0, resource(channel, 95, &maskImage));
            Assert.AreEqual(0, resource(channel, 80, &brush)); Assert.AreEqual(0, resource(channel, 43, &data));
            Assert.AreEqual(0, resource(channel, 39, &visual)); Assert.AreEqual(0, resource(channel, 47, &destination));
            byte* sourcePacket = stackalloc byte[8 + sizeof(nint)];
            *(uint*)sourcePacket = 0x0c; *(uint*)(sourcePacket + 4) = image; *(nint*)(sourcePacket + 8) = content;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)content)[1])(content);
            Assert.AreEqual(0, send(sourcePacket, (uint)(8 + sizeof(nint)), 0, channel));
            *(uint*)(sourcePacket + 4) = maskImage; *(nint*)(sourcePacket + 8) = mask;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)mask)[1])(mask);
            Assert.AreEqual(0, send(sourcePacket, (uint)(8 + sizeof(nint)), 0, channel));
            byte* brushPacket = stackalloc byte[148]; new Span<byte>(brushPacket, 148).Clear();
            *(uint*)brushPacket = 0x81; *(uint*)(brushPacket + 4) = brush; *(double*)(brushPacket + 8) = 1;
            *(double*)(brushPacket + 32) = relativeViewport ? 1 : 2;
            *(uint*)(brushPacket + 108) = relativeViewport ? 1u : 0u; *(double*)(brushPacket + 40) = viewportHeight;
            *(double*)(brushPacket + 64) = relativeViewbox ? 1 : 2 * 96.0 / dpi;
            *(double*)(brushPacket + 72) = relativeViewbox ? 1 : 96.0 / dpi;
            *(uint*)(brushPacket + 112) = relativeViewbox ? 1u : 0u;
            *(uint*)(brushPacket + 124) = stretch; *(uint*)(brushPacket + 132) = alignmentX;
            *(uint*)(brushPacket + 144) = maskImage;
            Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
            byte* draw = stackalloc byte[60]; new Span<byte>(draw, 60).Clear();
            *(uint*)draw = 0x18; *(uint*)(draw + 4) = data; *(uint*)(draw + 8) = 48; *(uint*)(draw + 12) = 48; *(uint*)(draw + 16) = 0x47;
            *(double*)(draw + 20) = relativeViewport ? 1 : 0;
            *(double*)(draw + 36) = relativeViewport ? 1 : 2; *(double*)(draw + 44) = 1; *(uint*)(draw + 52) = image;
            Assert.AreEqual(0, send(draw, 60, 0, channel));
            uint* bind = stackalloc uint[] { 0x22, visual, data }; Assert.AreEqual(0, send(bind, 12, 0, channel));
            bind[0] = 0x23; bind[2] = brush; Assert.AreEqual(0, send(bind, 12, 0, channel));
            byte* targetPacket = stackalloc byte[36]; new Span<byte>(targetPacket, 36).Clear();
            *(uint*)targetPacket = 0x34; *(uint*)(targetPacket + 4) = destination; *(ulong*)(targetPacket + 16) = (ulong)target;
            *(uint*)(targetPacket + 24) = 2; *(uint*)(targetPacket + 28) = 1; Assert.AreEqual(0, send(targetPacket, 36, 0, channel));
            bind[0] = 0x35; bind[1] = destination; bind[2] = visual; Assert.AreEqual(0, send(bind, 12, 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)mask)[8])(mask, 0, 2, &heldLock));
            Assert.AreEqual(unchecked((int)0x88982F0D), present(connection));
            uint* result = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
            Assert.AreEqual((0u, 0u), (result[0], result[1]));
            Release(heldLock); heldLock = 0; Release(mask); mask = 0; Release(maskTarget); maskTarget = 0;
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
            Assert.AreEqual((first, second), (result[0], result[1]));
        }
        finally
        {
            Release(heldLock);
            if (channel != 0) destroy(channel); if (connection != 0) disconnect(connection);
            Release(output); Release(mask); Release(content); Release(maskTarget); Release(contentTarget); Release(target); Release(factory);
        }
    }
}
