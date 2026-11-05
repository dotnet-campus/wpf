using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaDurationTests
{
    [DataTestMethod]
    [DataRow(IndependentMediaState.Opened)]
    [DataRow(IndependentMediaState.Playing)]
    [DataRow(IndependentMediaState.Paused)]
    [DataRow(IndependentMediaState.Stopped)]
    [DataRow(IndependentMediaState.Seeking)]
    [DataRow(IndependentMediaState.Ended)]
    public void WhenOpenedStateHasDurationThenLengthIsUnchanged(int state)
    {
        int hr = IndependentMediaIdentity.QueryLength((IndependentMediaState)state, 40000000, 0, out long value);
        Assert.AreEqual((0, 40000000L), (hr, value));
    }

    [DataTestMethod]
    [DataRow(IndependentMediaState.Closed)]
    [DataRow(IndependentMediaState.Opening)]
    public void WhenSourceIsUnavailableThenLengthDoesNotLeak(int state)
    {
        int hr = IndependentMediaIdentity.QueryLength((IndependentMediaState)state, 40000000, 0, out long value);
        Assert.AreEqual((unchecked((int)0x8007139F), 0L), (hr, value));
    }

    [TestMethod]
    public void WhenDurationIsUnknownThenOriginalFailureIsPreserved()
    {
        int hr = IndependentMediaIdentity.QueryLength(IndependentMediaState.Opened, null, unchecked((int)0xc00d36e6), out long value);
        Assert.AreEqual((unchecked((int)0xc00d36e6), 0L), (hr, value));
    }

    [DataTestMethod]
    [DataRow(0L)]
    [DataRow(long.MaxValue)]
    public void WhenDurationIsRepresentableThenItIsNotRounded(long raw)
    {
        int hr = MediaFoundationSourceReader.DecodeDuration(0, 21, (ulong)raw, out long? duration);
        Assert.AreEqual((0, (long?)raw), (hr, duration));
    }

    [DataTestMethod]
    [DataRow(20, 1UL)]
    [DataRow(21, 9223372036854775808UL)]
    [DataRow(21, ulong.MaxValue)]
    public void WhenDurationTypeOrRangeIsInvalidThenItIsRejected(int type, ulong raw)
    {
        int hr = MediaFoundationSourceReader.DecodeDuration(0, (ushort)type, raw, out long? duration);
        Assert.AreEqual((unchecked((int)0xc00d36b4), (long?)null), (hr, duration));
    }

    [DataTestMethod]
    [DataRow(-1L, unchecked((int)0x80070057))]
    [DataRow(0L, 0)]
    [DataRow(39999999L, 0)]
    [DataRow(40000000L, 0)]
    [DataRow(40000001L, unchecked((int)0xc00d36b2))]
    [DataRow(long.MaxValue, unchecked((int)0xc00d36b2))]
    public void WhenSeekIsCheckedThenNoClampingOccurs(long position, int expected)
        => Assert.AreEqual(expected, IndependentMediaIdentity.ValidateSeek(position, 40000000));

    [TestMethod]
    public void WhenDurationIsUnknownThenSeekIsNotLimitedToZero()
        => Assert.AreEqual(0, IndependentMediaIdentity.ValidateSeek(long.MaxValue, null));

    [TestMethod]
    public void WhenComLengthIsUnopenedClosedOrNullThenPreciseErrorsAndClearedOutputsAreReturned()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        var length = (delegate* unmanaged[Stdcall]<nint, long*, int>)table[20];
        try
        {
            long value = -1;
            int unopened = length(self, &value);
            Assert.AreEqual((unchecked((int)0x8007139F), 0L), (unopened, value));
            Assert.AreEqual(unchecked((int)0x80004003), length(self, null));
            byte pause = 1;
            int pauseHr = ((delegate* unmanaged[Stdcall]<nint, byte*, int>)table[13])(self, &pause);
            Assert.AreEqual((0, (byte)0), (pauseHr, pause));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(self));
            value = -1;
            int closed = length(self, &value);
            Assert.AreEqual((unchecked((int)0x80000013), 0L), (closed, value));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self); }
        long invalid = -1;
        int invalidHr = length(self, &invalid);
        Assert.AreEqual((unchecked((int)0x80000013), 0L), (invalidHr, invalid));
    }
}
