using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
[DoNotParallelize]
public sealed unsafe class ShaderSamplerProtocolTests
{
    [TestMethod]
    [DataRow(0, 0)]
    [DataRow(16, 16)]
    [DataRow(17, 17)]
    [DataRow(16, 17)]
    [DataRow(17, 16)]
    public void WhenSamplerPayloadHasIgnoredTailThenPublicChannelAcceptsAndCloses(int infoCount, int handleCount)
    {
        string path = AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        nint connection = 0, channel = 0;
        try
        {
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, createChannel(connection, 0, &channel));
            uint effect = 0;
            Assert.AreEqual(0, create(channel, 38, &effect));
            byte[] packet = new byte[80 + infoCount * 8 + handleCount * 4];
            BitConverter.GetBytes(0x70u).CopyTo(packet, 0);
            BitConverter.GetBytes(effect).CopyTo(packet, 4);
            BitConverter.GetBytes(-1).CopyTo(packet, 44);
            BitConverter.GetBytes((uint)(infoCount * 8)).CopyTo(packet, 72);
            BitConverter.GetBytes((uint)(handleCount * 4)).CopyTo(packet, 76);
            // Every effective entry binds null to register zero; ignored tails are deliberately invalid.
            if (infoCount > 16) BitConverter.GetBytes(-1).CopyTo(packet, 80 + 16 * 8);
            if (handleCount > 16) BitConverter.GetBytes(uint.MaxValue).CopyTo(packet, 80 + infoCount * 8 + 16 * 4);
            fixed (byte* bytes = packet) Assert.AreEqual(0, send(bytes, (uint)packet.Length, 0, channel));
            Assert.AreEqual(0, close(channel));
            Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
        }
        finally
        {
            if (channel != 0) Assert.AreEqual(0, destroy(channel));
            if (connection != 0) Assert.AreEqual(0, disconnect(connection));
        }
    }
}
