using WpfGfxShape.Core;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class IndependentMediaOpenFailureTests
{
    [TestMethod]
    public async Task WhenClosedTwiceThenCompletionAndStateRemainClosed()
    {
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        try
        {
            Close(identity.Pointer);
            await identity.Completion;
            Close(identity.Pointer);
            await identity.Completion;
            Assert.AreEqual(IndependentMediaState.Closed, identity.State);
        }
        finally { Direct3D9Factory.Release(identity.Pointer); }
    }

    [TestMethod]
    public async Task WhenFinalReleaseFollowsCloseThenRegistryIsRemoved()
    {
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        Close(identity.Pointer);
        await identity.Completion;
        Direct3D9Factory.Release(identity.Pointer);
        await identity.Completion;
        Assert.IsNull(IndependentMediaIdentity.Resolve(identity.Pointer));
    }

    [TestMethod]
    public async Task WhenInvalidOpenFailsThenOwnerHasExitedBeforeCompletion()
    {
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        try
        {
            Assert.AreEqual(0, Open(identity.Pointer));
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.AreEqual((IndependentMediaState.Failed, true, unchecked((int)0x80070057)),
                (identity.State, identity.Owner!.ThreadExited, identity.Failure));
        }
        finally
        {
            Close(identity.Pointer);
            await identity.Completion;
            Direct3D9Factory.Release(identity.Pointer);
        }
    }

    [TestMethod]
    public async Task WhenFailedEpochIsClosedAndReopenedThenOnlyReplacementOwnerIsCurrent()
    {
        IndependentMediaIdentity identity = IndependentMediaIdentity.Create();
        try
        {
            Open(identity.Pointer);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(5));
            var previous = identity.Owner;
            Close(identity.Pointer);
            await identity.Completion;
            Open(identity.Pointer);
            await identity.Completion.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.AreEqual((false, true, true, IndependentMediaState.Failed),
                (ReferenceEquals(previous, identity.Owner), previous!.ThreadExited,
                    identity.Owner!.ThreadExited, identity.State));
        }
        finally
        {
            Close(identity.Pointer);
            await identity.Completion;
            Direct3D9Factory.Release(identity.Pointer);
        }
    }

    private static unsafe int Open(nint self)
    {
        fixed (char* path = " ")
            return ((delegate* unmanaged[Stdcall]<nint, char*, int>)(*(nint**)self)[3])(self, path);
    }

    private static unsafe int Close(nint self)
        => ((delegate* unmanaged[Stdcall]<nint, int>)(*(nint**)self)[5])(self);
}
