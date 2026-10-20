using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class AvCompositionNotifierTests
{
    [TestMethod]
    public void WhenFrameCallbackUnregistersOnAnotherThreadThenRegistrationLockIsNotHeld()
    {
        CompositionNotifier notifier = new(() => { });
        Notification notification = new();
        notification.OnFrame = () => Task.Run(() => notifier.UnregisterResource(notification)).Wait(TimeSpan.FromSeconds(5));
        _ = notifier.RegisterResource(notification);

        notifier.NotifyComposition();

        Assert.IsTrue(notification.Completed);
    }

    [TestMethod]
    public void WhenInvalidationCallbackUnregistersOnAnotherThreadThenRegistrationLockIsNotHeld()
    {
        CompositionNotifier notifier = new(() => { });
        Notification notification = new();
        notification.OnInvalidation = () => Task.Run(() => notifier.UnregisterResource(notification)).Wait(TimeSpan.FromSeconds(5));
        _ = notifier.RegisterResource(notification);

        notifier.InvalidateLastCompositionSampleTime();

        Assert.IsTrue(notification.Completed);
    }

    [TestMethod]
    public void WhenFrameCallbackRequestsNextUiFrameThenRequestSurvivesCurrentPass()
    {
        int uiFrames = 0;
        CompositionNotifier notifier = new(() => uiFrames++);
        Notification notification = new();
        notification.OnFrame = () => { notifier.NeedUIFrameUpdate(); return true; };
        _ = notifier.RegisterResource(notification);

        notifier.NotifyComposition();
        notifier.UnregisterResource(notification);
        notifier.NotifyComposition();

        Assert.AreEqual(1, uiFrames);
    }

    [TestMethod]
    public void WhenCallbackUnregistersAndReentersThenOnlyOuterSnapshotCompletes()
    {
        var events = new List<string>();
        CompositionNotifier notifier = new(() => events.Add("ui"));
        Notification notification = new();
        notification.OnFrame = () =>
        {
            events.Add("enter");
            notifier.UnregisterResource(notification);
            events.Add("unregister");
            notifier.NotifyComposition();
            events.Add("reentrant-return");
            return true;
        };
        _ = notifier.RegisterResource(notification);
        notifier.NotifyComposition();
        notifier.NotifyComposition();
        notifier.InvalidateLastCompositionSampleTime();

        CollectionAssert.AreEqual(new[] { "enter", "unregister", "reentrant-return" }, events);
    }

    [TestMethod]
    public void WhenRegistrationCyclesCloseThenCallbacksRemainSilent()
    {
        int calls = 0;
        CompositionNotifier notifier = new(() => { });
        for (int cycle = 0; cycle < 64; cycle++)
        {
            Notification notification = new() { OnFrame = () => { calls++; return true; } };
            _ = notifier.RegisterResource(notification);
            _ = notifier.RegisterResource(notification);
            notifier.NotifyComposition();
            notifier.UnregisterResource(notification);
            notifier.NotifyComposition();
        }

        Assert.AreEqual(64, calls);
    }

    private sealed class Notification : ICompositionVideoNotification
    {
        internal Func<bool> OnFrame { get; set; } = () => true;
        internal Func<bool> OnInvalidation { get; set; } = () => true;
        internal bool Completed { get; private set; }

        public bool NewFrame()
        {
            Completed = OnFrame();
            return true;
        }

        public void InvalidateLastCompositionSampleTime() => Completed = OnInvalidation();
    }
}
