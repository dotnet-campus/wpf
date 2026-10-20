using WpfGfxShape.Abi;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class NativeResourceBrushRealizationTests
{
    [DataTestMethod]
    [DataRow(16)]
    [DataRow(128)]
    public void WhenResourceRealizerMembersAreRepeatedThenStateAndEffectsRemainIntact(int repetitions)
    {
        SoftwareBrushRealization.Capture(null, GeneratedImageTransform.Identity, 0, default, out var captured);
        using var brush = captured!;
        using var session = new SoftwareImageRenderSession();
        nint realizer = NativeResourceBrushRealization.Create(brush, session);
        try
        {
            for (int i = 0; i < repetitions; i++)
            {
                int result = NativeBrushRealizerReader.Realize(realizer, 0x1234, 0x5678, 0x1357,
                    out nint realizedBrush, out nint effects);
                Assert.AreEqual((0, (nint)0, (nint)0), (result, realizedBrush, effects));
                ((delegate* unmanaged[Thiscall]<nint, void>)(*(void***)realizer)[8])(realizer);
            }
        }
        finally { NativeImmediateSolidRealization.ReleaseOwner(realizer); }
    }

    [TestMethod]
    public void WhenResourceRealizerReferenceIsRetainedThenOwnerReleaseKeepsStateAlive()
    {
        SoftwareBrushRealization.Capture(null, GeneratedImageTransform.Identity, 0, default, out var captured);
        using var brush = captured!;
        using var session = new SoftwareImageRenderSession();
        nint realizer = NativeResourceBrushRealization.Create(brush, session);
        void** table = *(void***)realizer;
        ((delegate* unmanaged[Stdcall]<nint, uint>)table[0])(realizer);
        NativeImmediateSolidRealization.ReleaseOwner(realizer);
        try
        {
            int result = NativeBrushRealizerReader.Realize(realizer, 0, 0, 0, out _, out nint effects);
            Assert.AreEqual((0, (nint)0), (result, effects));
        }
        finally { NativeImmediateSolidRealization.ReleaseOwner(realizer); }
    }
}
