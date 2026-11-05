using System.Reflection;
using WpfGfxShape.Core;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class IndependentMediaFrameLifecycleTests
{
    [DataTestMethod]
    [DataRow(IndependentMediaState.Closed)]
    [DataRow(IndependentMediaState.Opening)]
    [DataRow(IndependentMediaState.Closing)]
    public void WhenPlayerHasNoLifecycleFrameThenAcquireReturnsNoContent(int state)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            SetState(media, (IndependentMediaState)state);
            if ((IndependentMediaState)state == IndependentMediaState.Closing)
                typeof(IndependentMediaIdentity).GetField("_closing", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, true);
            int result = media.Source.Acquire(out var sample);
            Assert.AreEqual((0, false), (result, sample is not null));
        }
        finally { Direct3D9Factory.Release(media.Pointer); }
    }

    [TestMethod]
    public void WhenActiveOwnerIsMissingThenAcquireDoesNotHideFailure()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            SetState(media, IndependentMediaState.Playing);
            Assert.AreEqual(unchecked((int)0x8007139F), media.Source.Acquire(out _));
        }
        finally { Direct3D9Factory.Release(media.Pointer); }
    }

    [TestMethod]
    public void WhenActiveFrameOwnerIsClosedThenAcquirePreservesClosedError()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        IndependentVideoPlaybackOwner owner = new();
        try
        {
            owner.Frames.Dispose();
            typeof(IndependentMediaIdentity).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, owner);
            SetState(media, IndependentMediaState.Playing);
            Assert.AreEqual(unchecked((int)0x80000013), media.Source.Acquire(out _));
        }
        finally { Direct3D9Factory.Release(media.Pointer); }
    }

    [DataTestMethod]
    [DataRow(unchecked((int)0x80004005))]
    [DataRow(unchecked((int)0xc00d36b4))]
    public unsafe void WhenFailedThenUiRequestAndEmptyFramePreserveOriginalFailure(int failure)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            SetState(media, IndependentMediaState.Failed);
            typeof(IndependentMediaIdentity).GetProperty("Failure", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, failure);
            nint* table = *(nint**)media.Pointer;
            int request = ((delegate* unmanaged[Stdcall]<nint, int>)table[21])(media.Pointer);
            int acquire = media.Source.Acquire(out var sample);
            Assert.AreEqual((0, 0, false, failure, 1L, 0L, true),
                (request, acquire, sample is not null, media.Failure, media.UiRequestGeneration, media.UiFrameGeneration, media.Owner is null));
        }
        finally { Direct3D9Factory.Release(media.Pointer); }
    }

    [TestMethod]
    public unsafe void WhenTerminalThenUiRequestAndFrameRejectClosedIdentity()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        try
        {
            nint* table = *(nint**)media.Pointer;
            ((delegate* unmanaged[Stdcall]<nint, int>)table[22])(media.Pointer);
            int request = ((delegate* unmanaged[Stdcall]<nint, int>)table[21])(media.Pointer);
            Assert.AreEqual((unchecked((int)0x80000013), unchecked((int)0x80000013)),
                (request, media.Source.Acquire(out _)));
        }
        finally { Direct3D9Factory.Release(media.Pointer); }
    }

    private static void SetState(IndependentMediaIdentity media, IndependentMediaState state)
        => typeof(IndependentMediaIdentity).GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(media, state);
}
