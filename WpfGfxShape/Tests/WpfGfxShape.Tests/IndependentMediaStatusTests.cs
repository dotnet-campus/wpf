using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaStatusTests
{
    [DataTestMethod]
    [DataRow(12)]
    [DataRow(17)]
    public void WhenBooleanMetadataIsUnopenedThenStateAndPointerErrorsArePrecise(int slot)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        var getter = (delegate* unmanaged[Stdcall]<nint, byte*, int>)table[slot];
        try
        {
            byte value = 7;
            Assert.AreEqual((slot == 17 ? 0 : unchecked((int)0x8007139f), (byte)0), (getter(self, &value), value));
            Assert.AreEqual(unchecked((int)0x80004003), getter(self, null));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(self));
            value = 7;
            Assert.AreEqual((slot == 17 ? 0 : unchecked((int)0x80000013), (byte)0), (getter(self, &value), value));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self); }
    }

    [DataTestMethod]
    [DataRow(14)]
    [DataRow(15)]
    public void WhenProgressIsUnopenedThenItDoesNotClaimCompletion(int slot)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        var getter = (delegate* unmanaged[Stdcall]<nint, double*, int>)table[slot];
        try
        {
            double value = 7;
            Assert.AreEqual((0, 0d), (getter(self, &value), value));
            Assert.AreEqual(unchecked((int)0x80004003), getter(self, null));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(self));
            value = 7;
            Assert.AreEqual((0, 0d), (getter(self, &value), value));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self); }
    }

    [DataTestMethod]
    [DataRow(0d)]
    [DataRow(1d)]
    public void WhenRateIsSetBeforeOpenThenItIsAcceptedWithoutPretendingPlayback(double rate)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        try
        {
            Assert.AreEqual((0, IndependentMediaState.Closed),
                (((delegate* unmanaged[Stdcall]<nint, double, int>)table[8])(self, rate), media.State));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, int>)table[21])(self));
            Assert.AreEqual(0L, media.UiConsumedGeneration);
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self); }
    }
}
