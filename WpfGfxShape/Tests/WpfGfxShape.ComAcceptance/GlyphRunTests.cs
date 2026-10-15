using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class GlyphRunTests
{
    [DllImport("dwrite.dll", ExactSpelling = true)]
    private static extern int DWriteCreateFactory(uint factoryType, in Guid iid, out nint factory);

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenRealDirectWriteFontIsTransferredThenGlyphCreationAndReplacementSucceed(bool offsets)
    {
        Run(offsets, false, false);
    }

    [TestMethod]
    public void WhenGlyphPayloadIsTruncatedThenCommitReturnsMalformedPacket()
    {
        Run(false, true, false);
    }

    [TestMethod]
    public void WhenGlyphTransportRemainsOpenThenChannelDestroyReleasesRealFont()
    {
        Run(true, false, true);
    }

    private static void Run(bool offsets, bool truncated, bool discard)
    {
        string path = AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var resourceCreate = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        nint factory = 0, collection = 0, family = 0, font = 0, connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, DWriteCreateFactory(0, new Guid("b859ee5a-d838-4b5b-a2e8-1adc7d93db48"), out factory));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int, int>)(*(void***)factory)[3])(factory, &collection, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)collection)[4])(collection, 0, &family));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, nint*, int>)(*(void***)family)[5])(family, 0, &font));
            Assert.AreEqual(0, create(1, &connection));
            Assert.AreEqual(0, channelCreate(connection, 0, &channel));
            uint glyph = 0;
            Assert.AreEqual(0, resourceCreate(channel, 42, &glyph));
            byte* packet = stackalloc byte[90];
            new Span<byte>(packet, 90).Clear();
            *(uint*)packet = 0x3a;
            *(uint*)(packet + 4) = glyph;
            *(ulong*)(packet + 8) = (ulong)font;
            *(ushort*)(packet + 16) = offsets ? (ushort)0x10 : (ushort)0;
            *(float*)(packet + 28) = 12;
            *(double*)(packet + 48) = 16;
            *(double*)(packet + 56) = 16;
            *(ushort*)(packet + 64) = 1;
            *(ushort*)(packet + 76) = 1;
            *(float*)(packet + 78) = 8;
            AddRef(font);
            int sent = send(packet, truncated ? 81u : offsets ? 90u : 82u, 0, channel);
            if (sent < 0) Release(font);
            Assert.AreEqual(0, sent);
            if (!discard)
            {
                Assert.AreEqual(0, close(channel));
                Assert.AreEqual(truncated ? unchecked((int)0x88980403) : 0, commit(channel));
                if (!truncated)
                {
                    AddRef(font);
                    Assert.AreEqual(0, send(packet, offsets ? 90u : 82u, 0, channel));
                    Assert.AreEqual(0, close(channel));
                    Assert.AreEqual(0, commit(channel));
                }
            }
            Assert.AreEqual(0, destroy(channel));
            channel = 0;
            nint face = 0;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)font)[13])(font, &face));
            Release(face);
        }
        finally
        {
            if (channel != 0) destroy(channel);
            if (connection != 0) disconnect(connection);
            Release(font);
            Release(family);
            Release(collection);
            Release(factory);
            NativeLibrary.Free(module);
        }
    }

    private static void AddRef(nint value) => ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[1])(value);
    private static void Release(nint value)
    {
        if (value != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)value)[2])(value);
    }
}
