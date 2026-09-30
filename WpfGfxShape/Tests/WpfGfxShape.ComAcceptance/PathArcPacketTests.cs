using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class PathArcPacketTests
{
    [TestMethod]
    [DataRow(64, 0)]
    [DataRow(56, unchecked((int)0x88980403))]
    public void WhenArcPacketHasNativeSizeThenCommitValidatesWholeSegment(int segmentSize, int expected)
    {
        string path = typeof(PathArcPacketTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!;
        nint module = NativeLibrary.Load(path);
        var create = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, create(1, &connection));
            Assert.AreEqual(0, channelCreate(connection, 0, &channel));
            uint handle = 0;
            Assert.AreEqual(0, resource(channel, 73, &handle));
            int size = 20 + 48 + 40 + segmentSize;
            byte* packet = stackalloc byte[size];
            new Span<byte>(packet, size).Clear();
            *(uint*)packet = 0x7d; *(uint*)(packet + 4) = handle;
            *(uint*)(packet + 16) = (uint)(size - 20);
            *(uint*)(packet + 20) = (uint)(size - 20); *(uint*)(packet + 60) = 1;
            byte* figure = packet + 68;
            *(uint*)(figure + 4) = 14; *(uint*)(figure + 8) = 1;
            *(uint*)(figure + 12) = (uint)(40 + segmentSize); *(uint*)(figure + 32) = 40;
            *(uint*)(figure + 40) = 4;
            Assert.AreEqual(0, send(packet, (uint)size, 0, channel));
            Assert.AreEqual(0, close(channel));
            Assert.AreEqual(expected, commit(channel));
        }
        finally
        {
            if (channel != 0) destroy(channel);
            if (connection != 0) disconnect(connection);
        }
    }
}
