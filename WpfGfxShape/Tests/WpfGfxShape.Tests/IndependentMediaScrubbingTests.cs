using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class IndependentMediaScrubbingTests
{
    [DataTestMethod]
    [DataRow((byte)0)]
    [DataRow((byte)1)]
    public void WhenScrubIsSetBeforeOpenThenItDoesNotCreatePlayback(byte enabled)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        try
        {
            int hr = ((delegate* unmanaged[Stdcall]<nint, byte, int>)table[11])(self, enabled);
            Assert.AreEqual((0, IndependentMediaState.Closed, true), (hr, media.State, media.Owner is null));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self); }
    }

    [DataTestMethod]
    [DataRow((byte)0)]
    [DataRow((byte)1)]
    public void WhenClosedThenScrubSetterReturnsClosedWithoutConfusingCanPause(byte enabled)
    {
        IndependentMediaIdentity media = IndependentMediaIdentity.Create();
        nint self = media.Pointer;
        nint* table = *(nint**)self;
        try
        {
            ((delegate* unmanaged[Stdcall]<nint, int>)table[5])(self);
            Assert.AreEqual(unchecked((int)0x80000013),
                ((delegate* unmanaged[Stdcall]<nint, byte, int>)table[11])(self, enabled));
        }
        finally { ((delegate* unmanaged[Stdcall]<nint, uint>)table[2])(self); }
    }
}
