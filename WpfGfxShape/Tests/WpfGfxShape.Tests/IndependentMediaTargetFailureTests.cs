using System.Reflection;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

public sealed partial class MediaFoundationResourceTests
{
    /// <summary>Preserves asynchronous failure when WPF submits initialization targets in either order.</summary>
    [DataTestMethod]
    [DataRow(false, 0d)]
    [DataRow(false, 1d)]
    [DataRow(true, 0d)]
    [DataRow(true, 1d)]
    public async Task WhenOpenFailureCrossesTargetUpdatesThenOriginalFailureIsRaisedOnceAsync(bool failureFirst, double rate)
    {
        var media = IndependentMediaIdentity.Create();
        var events = new ControlEvents(media);
        nint proxy = CreateControlProxy(events);
        object gate = typeof(IndependentMediaIdentity).GetField("_gate", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(media)!;
        try
        {
            Assert.AreEqual(0, media.AttachEvents(proxy));
            ReleaseControlProxy(proxy);
            proxy = 0;
            // Holding the production admission lock prevents failure completion from overtaking targets.
            lock (gate)
            {
                Assert.AreEqual(0, IdentityOpen(media.Pointer, " "));
                if (!failureFirst)
                {
                    Assert.AreEqual(0, IdentitySeek(media.Pointer, 0));
                    Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
                }
            }
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            if (failureFirst)
            {
                Assert.AreEqual(0, IdentitySeek(media.Pointer, 0));
                Assert.AreEqual(0, IdentityRate(media.Pointer, rate));
            }
            int failure = await events.Failed.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual((unchecked((int)0x80070057), unchecked((int)0x80070057), IndependentMediaState.Failed, 1),
                (failure, media.Failure, media.State, events.Kinds.Count(kind => kind == 8)));
            Assert.AreEqual(unchecked((int)0x80070057), IdentitySeek(media.Pointer, -1));
            Assert.AreEqual(0, IdentityRate(media.Pointer, 2));
            var previous = media.Owner;
            IdentityCall(media.Pointer, 5);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual(0, IdentityOpen(media.Pointer, " "));
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.AreEqual((false, true, IndependentMediaState.Failed, unchecked((int)0x80070057)),
                (ReferenceEquals(previous, media.Owner), previous!.ThreadExited, media.State, media.Failure));
        }
        finally
        {
            if (proxy != 0) ReleaseControlProxy(proxy);
            IdentityCall(media.Pointer, 5);
            await media.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            IdentityRef(media.Pointer, 2);
        }
    }
}
