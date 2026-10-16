using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class UceChannelTests
{
    [DataTestMethod]
    [DataRow(false, 0, false)]
    [DataRow(false, 1, false)]
    [DataRow(false, 2, false)]
    [DataRow(false, 3, false)]
    [DataRow(true, 0, false)]
    [DataRow(false, 0, true)]
    [DataRow(false, 1, true)]
    [DataRow(true, 0, true)]
    public void WhenTypedBitmapBatchStopsThenCommittedSourceLivesUntilChannelClose(bool system, int failure, bool shared)
    {
        string path = system
            ? Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll")
            : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var duplicate = (delegate* unmanaged[Stdcall]<nint, uint, nint, uint*, int>)Export("MilResource_DuplicateHandle");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var transfer = (delegate* unmanaged[Stdcall]<uint, nint, nint, int>)Export("MilResource_SendCommandBitmapSource");
        var wrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)Export("MilResource_CreateCWICWrapperBitmap");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        using var wic = new SystemWicBitmap();
        nint connection = 0, channel = 0, other = 0, stream = 0, frame = 0, wrapper = 0;
        int disposals = 0;
        List<object> calls = [];
        int beforeClose = -1;
        void Check(string operation, int result)
        {
            calls.Add(new { operation, result });
            Assert.AreEqual(0, result, operation);
        }
        void ReleaseCom(ref nint value)
        {
            nint owned = value;
            value = 0;
            if (owned != 0) ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)owned)[2])(owned);
        }
        try
        {
            Check("Connect", connect(1, &connection));
            Check("Channel", channelCreate(connection, 0, &channel));
            uint bitmap = 0, wrongType = 0;
            Check("Create.Bitmap", create(channel, 95, &bitmap));
            Check("Create.Brush", create(channel, 49, &wrongType));
            byte[] bmp = new byte[62];
            bmp[0] = (byte)'B'; bmp[1] = (byte)'M';
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(2), bmp.Length);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(10), 54);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(14), 40);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(18), 2);
            BinaryPrimitives.WriteInt32LittleEndian(bmp.AsSpan(22), 1);
            bmp[26] = 1; bmp[28] = 24; bmp[56] = 255;
            stream = ManagedStreamTests.CreateCallbackStream(module, bmp, () => { }, () => disposals++);
            frame = wic.DecodeFrame(stream);
            Check("Wrap", wrap(frame, &wrapper));
            Check("Transfer", transfer(bitmap, wrapper, channel));
            ReleaseCom(ref wrapper); ReleaseCom(ref frame); ReleaseCom(ref stream);
            if (shared)
            {
                Check("Shared.Channel", channelCreate(connection, channel, &other));
                uint alias = 0;
                Check("Shared.Duplicate", duplicate(channel, bitmap, other, &alias));
                Check("Shared.Source.Close", close(channel));
                Check("Shared.Source.Commit", commit(channel));
                Check("Shared.Target.Close", close(other));
                Check("Shared.Target.Commit", commit(other));
                Assert.AreEqual(0, disposals);
            }
            if (failure != 0)
            {
                // A legal command header with an invalid typed target or incomplete payload.
                uint* invalid = stackalloc uint[] { 0x0D, failure == 1 ? wrongType : failure == 2 ? uint.MaxValue : bitmap, 0, 0, 0, 0, 0 };
                Check("Failure.Queue", send(invalid, failure == 3 ? 12u : 28u, 0, channel));
            }
            Check("Close", close(channel));
            int expected = failure == 0 ? 0 : unchecked((int)0x88980403);
            int first = commit(channel);
            calls.Add(new { operation = "Commit", result = first });
            Assert.AreEqual(expected, first);
            Assert.AreEqual(expected, commit(channel), "First failure remains sticky.");
            beforeClose = disposals;
            Assert.AreEqual(0, beforeClose, "Successful prefix is not atomically rolled back.");
            nint closed = channel;
            Check("Destroy", destroy(channel)); channel = 0;
            if (!system)
                Assert.AreEqual(unchecked((int)0x80070057), destroy(closed), "Duplicate close cannot release again.");
            if (shared)
            {
                Assert.AreEqual(0, disposals, "Shared target retains the real decoder after source close.");
                Check("Shared.Destroy", destroy(other)); other = 0;
            }
            Assert.AreEqual(1, disposals, "Real WIC decoder releases its external stream at final channel close.");
        }
        finally
        {
            if (channel != 0) destroy(channel);
            if (other != 0) destroy(other);
            if (connection != 0) disconnect(connection);
            ReleaseCom(ref wrapper); ReleaseCom(ref frame); ReleaseCom(ref stream);
            Directory.CreateDirectory(AcceptancePaths.Results);
            File.WriteAllText(Path.Combine(AcceptancePaths.Results, $"typed-batch-{system}-{failure}-{shared}.json"),
                JsonSerializer.Serialize(new { path, system, failure, shared, calls, beforeClose, disposals,
                    sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                    scope = "Real WIC frame and external stream lifetime; not proof of all COM references. DLL unload not applicable." },
                    new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
