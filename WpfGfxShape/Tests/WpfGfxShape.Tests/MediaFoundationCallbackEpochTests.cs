using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class MediaFoundationCallbackEpochTests
{
    private static void Read(MediaFoundationReaderCallback callback, int status, uint flags, long timestamp)
    {
        nint pointer = callback.Pointer;
        ((delegate* unmanaged[Stdcall]<nint, int, uint, uint, long, nint, int>)(*(nint**)pointer)[3])
            (pointer, status, 0, flags, timestamp, 0);
    }

    private static void Flush(MediaFoundationReaderCallback callback)
    {
        nint pointer = callback.Pointer;
        ((delegate* unmanaged[Stdcall]<nint, uint, int>)(*(nint**)pointer)[4])(pointer, 0);
    }

    [TestMethod]
    public void WhenOnFlushArrivesWithReadInFlightThenFenceWaitsForCallbackExit()
    {
        var callback = new MediaFoundationReaderCallback();
        using var entered = new ManualResetEventSlim();
        using var resume = new ManualResetEventSlim();
        callback.ReadCallbackAdmitted = () => { entered.Set(); resume.Wait(TimeSpan.FromSeconds(5)); };
        var thread = new Thread(() => Read(callback, 0, 0, 0));
        try
        {
            thread.Start();
            Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(5)));
            callback.BeginFlush();
            Flush(callback);
            Assert.IsFalse(callback.FlushCompletion.IsCompleted);
        }
        finally
        {
            resume.Set();
            thread.Join();
            callback.ReleaseOwner();
        }
    }

    [TestMethod]
    public void WhenFlushCompletesThenNextEpochCanReceiveEndOfStream()
    {
        var callback = new MediaFoundationReaderCallback();
        try
        {
            callback.BeginFlush();
            Flush(callback);
            callback.EndFlush();
            Read(callback, 0, 2, 123);
            int hr = callback.WaitSample(out _, out uint flags, out long timestamp, out nint sample);
            Assert.AreEqual((0, 2u, 123L, (nint)0, 1L, false),
                (hr, flags, timestamp, sample, callback.Epoch, callback.Closing));
        }
        finally { callback.ReleaseOwner(); }
    }

    [TestMethod]
    public void WhenSecondFlushStartsThenPreviousCompletionCannotSatisfyFence()
    {
        var callback = new MediaFoundationReaderCallback();
        try
        {
            callback.BeginFlush();
            Flush(callback);
            callback.EndFlush();
            callback.BeginFlush();
            Assert.IsFalse(callback.FlushCompletion.IsCompleted);
        }
        finally { callback.ReleaseOwner(); }
    }

    [TestMethod]
    public void WhenFailedReadCompletesThenOriginalHresultIsPreserved()
    {
        var callback = new MediaFoundationReaderCallback();
        try
        {
            Read(callback, unchecked((int)0x80070002), 0, 0);
            Assert.AreEqual(unchecked((int)0x80070002), callback.WaitSample(out _, out _, out _, out _));
        }
        finally { callback.ReleaseOwner(); }
    }

    [TestMethod]
    public void WhenTwoCallbacksArriveThenOnlyFirstCompletionIsQueued()
    {
        var callback = new MediaFoundationReaderCallback();
        try
        {
            Read(callback, 0, 0x100, 12);
            Read(callback, 0, 2, 99);
            callback.WaitSample(out _, out uint flags, out long timestamp, out _);
            Assert.AreEqual((0x100u, 12L), (flags, timestamp));
        }
        finally { callback.ReleaseOwner(); }
    }

    [TestMethod]
    public void WhenClosedCallbackArrivesThenReadStaysCancelled()
    {
        var callback = new MediaFoundationReaderCallback();
        try
        {
            callback.RequestClose();
            Read(callback, 0, 2, 0);
            Assert.AreEqual(unchecked((int)0x800704c7), callback.WaitSample(out _, out _, out _, out _));
        }
        finally { callback.ReleaseOwner(); }
    }

    [TestMethod]
    public void WhenReadArrivesDuringFlushThenItCannotCompleteNewEpoch()
    {
        var callback = new MediaFoundationReaderCallback();
        try
        {
            callback.BeginFlush();
            Read(callback, 0, 0, 10);
            Flush(callback);
            callback.EndFlush();
            Read(callback, 0, 2, 20);
            callback.WaitSample(out _, out _, out long timestamp, out _);
            Assert.AreEqual(20L, timestamp);
        }
        finally { callback.ReleaseOwner(); }
    }
}
