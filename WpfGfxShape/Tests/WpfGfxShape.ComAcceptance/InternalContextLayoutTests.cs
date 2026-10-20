namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class InternalContextLayoutTests
{
    /// <summary>Checks the independent Release context offsets without using candidate ABI types.</summary>
    [TestMethod]
    public void WhenReleaseContextUsesProcessPointerWidthThenClipAndDrawStateOffsetsMatch()
    {
        Assert.AreEqual(IntPtr.Size == 4 ? (364, 384, 388) : (376, 400, 408),
            (InternalContextLayout.Clip, InternalContextLayout.RenderState, InternalContextLayout.WorldToDevice));
    }
}
