using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed unsafe class JzWicPacketWidthTests
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BitmapSourceCommand
    {
        internal uint Type;
        internal uint Handle;
        internal nint Bitmap;
    }

    /// <summary>Checks the independently declared wire layout without candidate ABI types.</summary>
    [TestMethod]
    public void WhenWicPacketUsesNativePointerThenLayoutIsTwelveOrSixteenBytes()
    {
        Assert.AreEqual((IntPtr.Size == 4 ? 12 : 16, 0, 4, 8),
            (Unsafe.SizeOf<BitmapSourceCommand>(),
             Marshal.OffsetOf<BitmapSourceCommand>(nameof(BitmapSourceCommand.Type)).ToInt32(),
             Marshal.OffsetOf<BitmapSourceCommand>(nameof(BitmapSourceCommand.Handle)).ToInt32(),
             Marshal.OffsetOf<BitmapSourceCommand>(nameof(BitmapSourceCommand.Bitmap)).ToInt32()));
    }

    /// <summary>Sends only a legal native-width packet to each real implementation.</summary>
    [DataTestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void WhenLegalWicPacketIsSentThenRealChannelCommitsAndCleansUp(bool official)
    {
        string path = official ? AcceptancePaths.OriginalDll : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var wrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateCWICWrapperBitmap");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        string actual = System.Diagnostics.Process.GetCurrentProcess().Modules.Cast<System.Diagnostics.ProcessModule>()
            .Single(item => item.BaseAddress == module).FileName;
        Console.WriteLine($"JZ official={official} path={actual} SHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(actual)))} pointer={IntPtr.Size} bytes={sizeof(BitmapSourceCommand)} runtime={RuntimeInformation.FrameworkDescription}");
        using var wic = new SystemWicBitmap();
        nint connection = 0, channel = 0, bitmap = 0, wrapper = 0;
        try
        {
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, createChannel(connection, 0, &channel));
            uint handle = 0;
            Assert.AreEqual(0, create(channel, 95, &handle));
            bitmap = wic.Create(2, 1);
            Assert.AreEqual(0, wrap(bitmap, &wrapper));
            BitmapSourceCommand packet = new() { Type = 0x0c, Handle = handle, Bitmap = wrapper };
            Assert.AreEqual(0, send(&packet, (uint)sizeof(BitmapSourceCommand), 0, channel));
            wrapper = 0;
            Console.WriteLine($"JZ Send Command0xC handle32={handle} bytes={sizeof(BitmapSourceCommand)} ownership=transferred");
            Assert.AreEqual(0, close(channel));
            Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Console.WriteLine("JZ close/commit/present=0");
        }
        finally
        {
            if (channel != 0) Assert.AreEqual(0, destroy(channel));
            if (connection != 0) Assert.AreEqual(0, disconnect(connection));
            if (wrapper != 0) Marshal.Release(wrapper);
            if (bitmap != 0) Console.WriteLine($"JZ bitmap final release refs={Marshal.Release(bitmap)}");
            Console.WriteLine("JZ channel/connection cleanup complete; module remains resident");
        }
    }

}
