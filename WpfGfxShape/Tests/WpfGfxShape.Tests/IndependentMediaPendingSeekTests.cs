using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaPendingSeekTests
{
    [DataTestMethod]
    [DataRow(0L, 0)]
    [DataRow(5000000L, 0)]
    [DataRow(-1L, unchecked((int)0x80070057))]
    public void WhenPositionIsSetBeforeMetadataThenLegalIntentIsAccepted(long timestamp, int expected)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint* table = *(nint**)media.Pointer;
        try
        {
            Assert.AreEqual(expected, ((delegate* unmanaged[Stdcall]<nint, long, int>)table[7])(media.Pointer, timestamp));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer); }
    }

    [TestMethod]
    public void WhenClosedThenPendingSeekCannotResurrectTheOwner()
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint* table = *(nint**)media.Pointer;
        try
        {
            ((delegate* unmanaged[Stdcall]<nint, long, int>)table[7])(media.Pointer, 5000000);
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(media.Pointer);
            Assert.AreEqual(unchecked((int)0x80000013), ((delegate* unmanaged[Stdcall]<nint, long, int>)table[7])(media.Pointer, 0));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(media.Pointer); }
    }
}
