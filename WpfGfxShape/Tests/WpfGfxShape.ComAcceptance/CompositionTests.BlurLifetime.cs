namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(8)]
    public void WhenBlurTargetWriteLockFailsThenUnlockRestoresInputAndCallbackOwners(int action)
        => WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(action, blurLifetime: true);

    [TestMethod]
    [DataRow(0, 2.0)]
    [DataRow(3, 2.0)]
    [DataRow(4, 2.0)]
    [DataRow(8, 2.0)]
    [DataRow(0, 5.0)]
    [DataRow(3, 5.0)]
    [DataRow(4, 5.0)]
    [DataRow(8, 5.0)]
    public void WhenNonzeroBlurTargetWriteLockFailsThenUnlockRestoresInputAndCallbackOwners(int action, double radius)
        => WhenDecoderReadsDuringRenderThenReentrantLifecyclePreservesActivePixels(action, blurLifetime: true, blurRadius: radius);
}
