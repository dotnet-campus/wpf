using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class MediaExportsTests
{
    [TestMethod]
    [Timeout(30000)]
    public void WhenRealCallbackRebindsAndUnsubscribesThenPublicLifecycleCompletes()
    {
        string path = Path.Combine(Path.GetTempPath(), $"wpf-rebind-callback-{Guid.NewGuid():N}.avi");
        WriteSolidVideo(path, 0xffff0000u);
        using var source = new SystemMedia(candidate: true);
        using var replacement = new SystemMedia(candidate: true);
        nint module = LoadCandidate(), media = source.Media, next = replacement.Media;
        int entered = 0;
        using var completed = new ManualResetEventSlim();
        source.Callback = () =>
        {
            if (Interlocked.Exchange(ref entered, 1) != 0) return;
            nint connection = 0, channel = 0;
            var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
            var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
            try
            {
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create"))(1, &connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel"))(connection, 0, &channel));
                uint player = 0;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel"))(channel, 1, &player));
                BindPlayer(module, channel, player, media);
                BindPlayer(module, channel, player, next);
                Assert.AreEqual(0, destroy(channel)); channel = 0;
                Assert.AreEqual(0, disconnect(connection)); connection = 0;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MILMediaClose"))(media));
            }
            finally
            {
                if (channel != 0) _ = destroy(channel);
                if (connection != 0) _ = disconnect(connection);
                completed.Set();
            }
        };
        var open = (delegate* unmanaged[Stdcall]<nint, char*, int>)NativeLibrary.GetExport(module, "MILMediaOpen");
        fixed (char* url = path) Assert.AreEqual(0, open(media, url));
        Assert.IsTrue(completed.Wait(TimeSpan.FromSeconds(15)), "Real callback lifecycle did not complete before timeout.");
        Assert.IsNull(source.CallbackError);
    }

    private static void BindPlayer(nint module, nint channel, uint player, nint media)
    {
        byte* packet = stackalloc byte[20];
        new Span<byte>(packet, 20).Clear();
        *(uint*)packet = 0x17; *(uint*)(packet + 4) = player;
        *(ulong*)(packet + 8) = (ulong)media;
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)media)[1])(media);
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand"))(packet, 20, 0, channel));
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch"))(channel));
        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel"))(channel));
    }
}
