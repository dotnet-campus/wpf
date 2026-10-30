using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private static void JhPresentImage(delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int> send,
        delegate* unmanaged[Stdcall]<nint, int> commit, delegate* unmanaged[Stdcall]<nint, int> flush,
        nint channel, uint image)
    {
        using var completion = new EventWaitHandle(false, EventResetMode.ManualReset);
        bool held = false;
        nint duplicate = 0;
        try
        {
            completion.SafeWaitHandle.DangerousAddRef(ref held);
            Assert.AreNotEqual(0, DuplicateHandle(-1, completion.SafeWaitHandle.DangerousGetHandle(), -1, &duplicate, 0, 0, 2));
        }
        finally { if (held) completion.SafeWaitHandle.DangerousRelease(); }
        // ProcessPresent owns the duplicate. A failed send/commit has unknown ownership;
        // only this isolated process may reclaim it, never a guessed CloseHandle here.
        PresentPacket packet = new() { Type = 0x0b, Handle = image, Event = (ulong)(nuint)duplicate };
        Assert.AreEqual(0, send(&packet, (uint)sizeof(PresentPacket), 1, channel));
        Assert.AreEqual(0, commit(channel));
        Assert.IsTrue(completion.WaitOne(TimeSpan.FromSeconds(5)));
        Assert.AreEqual(0, flush(channel));
        ExitDiagnostics.Stage($"JH ProcessPresent completed image={image} duplicate={duplicate}; not a refs0 claim");
    }
}
