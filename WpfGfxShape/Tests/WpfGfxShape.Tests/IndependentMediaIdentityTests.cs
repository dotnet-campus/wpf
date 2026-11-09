using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaIdentityTests
{
    [DataTestMethod]
    [DataRow(IndependentMediaState.Opened, 2u, 1u, true)]
    [DataRow(IndependentMediaState.Playing, 2u, 1u, true)]
    [DataRow(IndependentMediaState.Paused, 2u, 1u, true)]
    [DataRow(IndependentMediaState.Stopped, 2u, 1u, true)]
    [DataRow(IndependentMediaState.Seeking, 2u, 1u, true)]
    [DataRow(IndependentMediaState.Ended, 2u, 1u, true)]
    [DataRow(IndependentMediaState.Closed, 2u, 1u, false)]
    [DataRow(IndependentMediaState.Opening, 2u, 1u, false)]
    [DataRow(IndependentMediaState.Closing, 2u, 1u, false)]
    [DataRow(IndependentMediaState.Failed, 2u, 1u, false)]
    [DataRow(IndependentMediaState.Opened, 0u, 0u, false)]
    [DataRow(IndependentMediaState.Paused, 2u, 0u, false)]
    [DataRow(IndependentMediaState.Seeking, 0u, 1u, false)]
    public void WhenVideoMetadataAndLifecycleAreCheckedThenAvailabilityIsTruthful(int state, uint width, uint height, bool expected)
    {
        Assert.AreEqual(expected, IndependentMediaIdentity.HasAvailableVideo((IndependentMediaState)state, width, height));
    }

    [TestMethod]
    public void WhenUnopenedThenVideoAudioAndNaturalSizeAreZero()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint* v = *(nint**)media.Pointer;
        byte video = 1, audio = 1;
        uint width = 1, height = 1;
        try
        {
            int videoHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)v[16])(media.Pointer, &video);
            int audioHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)v[17])(media.Pointer, &audio);
            int widthHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)v[19])(media.Pointer, &width);
            int heightHr = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)v[18])(media.Pointer, &height);
            Assert.AreEqual((0, (byte)0, 0, (byte)0, 0, 0u, 0, 0u),
                (videoHr, video, audioHr, audio, widthHr, width, heightHr, height));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)v[2])(media.Pointer); }
    }

    [TestMethod]
    public void WhenQueriedThenUnknownAndMediaHaveSameIdentityAndBalancedReferences()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* v = *(nint**)self;
        Guid iid = new("00000000-0000-0000-c000-000000000046");
        nint output = 0;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)v[0])(self, &iid, &output));
            Assert.AreEqual(self, output);
            Assert.AreEqual(1u, ((delegate* unmanaged[Stdcall]<nint, uint>)v[2])(output));
            iid = new("00000141-a8f2-4877-ba0a-fd2b6645fb94");
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)v[0])(self, &iid, &output));
            Assert.AreEqual(self, output);
            Assert.AreEqual(1u, ((delegate* unmanaged[Stdcall]<nint, uint>)v[2])(output));
            Assert.AreEqual(2u, ((delegate* unmanaged[Stdcall]<nint, uint>)v[1])(self));
            Assert.AreEqual(1u, ((delegate* unmanaged[Stdcall]<nint, uint>)v[2])(self));
        }
        finally { Assert.AreEqual(0u, ((delegate* unmanaged[Stdcall]<nint, uint>)v[2])(self)); }
        Assert.IsNull(IndependentMediaIdentity.Resolve(self));
    }

    [TestMethod]
    public void WhenUnsupportedInterfaceIsQueriedThenOutputIsCleared()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint* v = *(nint**)media.Pointer;
        Guid iid = new("ffffffff-ffff-ffff-ffff-ffffffffffff");
        nint output = -1;
        try
        {
            int hr = ((delegate* unmanaged[Stdcall]<nint, Guid*, nint*, int>)v[0])(media.Pointer, &iid, &output);
            Assert.AreEqual((unchecked((int)0x80004002), (nint)0), (hr, output));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)v[2])(media.Pointer); }
    }

    [TestMethod]
    public void WhenOneInstanceClosesThenOtherRemainsIndependentAndUnsupportedIsPrecise()
    {
        IndependentMediaIdentity first = IndependentMediaIdentity.Create(), second = IndependentMediaIdentity.Create();
        nint* a = *(nint**)first.Pointer;
        nint* b = *(nint**)second.Pointer;
        try
        {
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)a[5])(first.Pointer));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)a[5])(first.Pointer));
            Assert.AreEqual(IndependentMediaState.Closed, first.State);
            Assert.AreEqual(unchecked((int)0x80000013), ((delegate* unmanaged[Stdcall]<nint, long, int>)a[7])(first.Pointer, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, long, int>)b[7])(second.Pointer, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, double, int>)b[8])(second.Pointer, 2));
            // Native SetRate admits a source-free target; unsupported application is tested on Open.
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, double, int>)b[8])(second.Pointer, -1));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, double, int>)b[9])(second.Pointer, 1));
            Assert.AreEqual(1.0, second.TargetVolume);
            Assert.AreEqual(0.5, first.TargetVolume);
            Assert.AreEqual(IndependentMediaState.Closed, second.State);
        }
        finally
        {
            Assert.AreEqual(0u, ((delegate* unmanaged[Stdcall]<nint, uint>)a[2])(first.Pointer));
            Assert.AreEqual(0u, ((delegate* unmanaged[Stdcall]<nint, uint>)b[2])(second.Pointer));
        }
    }
}
