#if IS_SHARED_CONSUMER_EXPERIMENT
// Unaccepted IS experiment; excluded from default compilation and test discovery.
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private sealed class SharedConsumerEvent : IDisposable
    {
        internal readonly EventWaitHandle Completion = new(false, EventResetMode.ManualReset);
        internal nint Duplicate;
        internal bool Submitted;
        internal bool Completed;

        public void Dispose() => Completion.Dispose();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int CloseHandle(nint handle);

    private static void SharedConsumerPresent(
        delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int> send,
        delegate* unmanaged[Stdcall]<nint, int> commit,
        delegate* unmanaged[Stdcall]<nint, int> flush,
        nint channel, uint image, List<SharedConsumerEvent> events, ref bool submissionFailed)
    {
        var pending = new SharedConsumerEvent();
        events.Add(pending);
        nint duplicate = 0;
        bool held = false;
        try
        {
            pending.Completion.SafeWaitHandle.DangerousAddRef(ref held);
            if (DuplicateHandle(-1, pending.Completion.SafeWaitHandle.DangerousGetHandle(), -1, &duplicate, 0, 0, 2) == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError());
        }
        finally { if (held) pending.Completion.SafeWaitHandle.DangerousRelease(); }
        pending.Duplicate = duplicate;
        PresentPacket packet = new() { Type = 0x0b, Handle = image, Event = (ulong)(nuint)duplicate };
        int hr = send(&packet, (uint)sizeof(PresentPacket), 1, channel);
        ExitDiagnostics.Stage($"IS event send image={image} duplicate={duplicate} hr={hr:X8}");
        submissionFailed = hr < 0;
        Assert.AreEqual(0, hr);
        // Commit can transfer a batch even when it returns failure. Never close here.
        pending.Submitted = true;
        hr = commit(channel);
        submissionFailed = hr < 0;
        ExitDiagnostics.Stage($"IS event commit image={image} hr={hr:X8}");
        Assert.AreEqual(0, hr);
        pending.Completed = pending.Completion.WaitOne(TimeSpan.FromSeconds(5));
        ExitDiagnostics.Stage($"IS event complete image={image} signalled={pending.Completed}");
        Assert.IsTrue(pending.Completed);
        Assert.AreEqual(0, flush(channel));
    }

    /// <summary>Exercises default shared geometry content, events and final resource convergence.</summary>
    [TestMethod]
    public void WhenSharedConsumerGeometryClosesThenResourcesAndEventsConverge()
        => RunHistoricalCaptureControl(false, true, false, 0xff00ff00u, 0x0000ff00u);

    /// <summary>Exercises default shared sampler content, events and final resource convergence.</summary>
    [TestMethod]
    public void WhenSharedConsumerSamplerClosesThenResourcesAndEventsConverge()
        => RunHistoricalCaptureControl(false, false, true, 0xff0000ffu, 0x00ff0080u);
}
#endif
